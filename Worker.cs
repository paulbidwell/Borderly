    using Borderly.Config;
using Borderly.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Borderly
{
    public class Worker(IConfiguration config, IImageProcessor processor, SemaphoreSlim semaphoreSlim, FileSystemWatcher watcher) : BackgroundService
    {
        private readonly Settings _settings = config.GetSection("Settings").Get<Settings>()
            ?? throw new InvalidOperationException("Missing configuration: Settings");

        private readonly IEnumerable<Profile> _profiles = config.GetSection("Profiles").Get<List<Profile>>()
            ?? throw new InvalidOperationException("Missing configuration: Profiles");

        private SemaphoreSlim _semaphoreSlim = semaphoreSlim;
        private FileSystemWatcher _watcher = watcher;
        private readonly string[] _supportedExtensions = new[] { ".jpg", ".jpeg", ".png", ".tif", ".tiff" };

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            Directory.CreateDirectory(_settings.InputDirectory);
            Directory.CreateDirectory(_settings.OutputDirectory);

            if (_settings.ProcessedFileOption == ProcessedFileOption.Move && !string.IsNullOrEmpty(_settings.ProcessedDirectory))
            {
                Directory.CreateDirectory(_settings.ProcessedDirectory);
            }

            _semaphoreSlim = new SemaphoreSlim(_settings.MaxConcurrency);

            _watcher = new FileSystemWatcher(_settings.InputDirectory)
            {
                IncludeSubdirectories = false,
                EnableRaisingEvents = true,
                Filter = "*.*"
            };

            _watcher.Created += (sender, args) =>
            {
                if (IsSupported(args.FullPath))
                {
                    _ = HandleFileAsync(args.FullPath);
                }
            };

            var files = Directory.GetFiles(_settings.InputDirectory)
                .Where(f => _supportedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()));

            foreach (var file in files)
                _ = HandleFileAsync(file);

            await base.StartAsync(cancellationToken);
        }

        private bool IsSupported(string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return _supportedExtensions.Contains(ext);
        }

        private async Task HandleFileAsync(string path)
        {
            if (!IsSupported(path))
                return;

            await _semaphoreSlim.WaitAsync();

            try
            {
                foreach (var profile in _profiles)
                {
                    processor.Process(path, profile);
                }

                switch (_settings.ProcessedFileOption)
                {
                    case ProcessedFileOption.Delete:
                        if (File.Exists(path))
                        {
                            File.Delete(path);
                        }
                        break;

                    case ProcessedFileOption.Move:
                        if (!string.IsNullOrEmpty(_settings.ProcessedDirectory))
                        {
                            var destPath = Path.Combine(_settings.ProcessedDirectory, Path.GetFileName(path));
                            File.Move(path, destPath, overwrite: true);
                        }
                        break;

                    case ProcessedFileOption.None:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Delay(Timeout.Infinite, stoppingToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _watcher.Dispose();
            return base.StopAsync(cancellationToken);
        }
    }
}
