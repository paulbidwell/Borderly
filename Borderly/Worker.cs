using Borderly.Config;
using Borderly.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;

namespace Borderly
{
    public class Worker(IConfiguration config, IImageProcessor processor) : BackgroundService
    {
        private static readonly ConcurrentDictionary<string, byte> _processingFiles = new();
        private readonly Settings _settings = config.GetSection("Settings").Get<Settings>()
                        ?? throw new InvalidOperationException("Missing configuration: Settings");

        private readonly Profile[] _profiles = config.GetSection("Profiles").Get<Profile[]>()
                        ?? throw new InvalidOperationException("Missing configuration: Profiles");

        private readonly HashSet<string> _allowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".tif", ".tiff"
        };

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            Directory.CreateDirectory(_settings.InputDirectory);
            Directory.CreateDirectory(_settings.OutputDirectory);

            if (_settings.ProcessedFileOption == ProcessedFileOption.Move && !string.IsNullOrEmpty(_settings.ProcessedDirectory))
            {
                Directory.CreateDirectory(_settings.ProcessedDirectory);
            }

            foreach (var file in Directory.EnumerateFiles(_settings.InputDirectory, "*.*")
                         .Where(f => _allowedExtensions.Contains(Path.GetExtension(f))))
            {
                _ = HandleFileAsync(file);
            }

            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var file in Directory.EnumerateFiles(_settings.InputDirectory, "*.*")
                                              .Where(f => _allowedExtensions.Contains(Path.GetExtension(f))))
                {
                    _ = HandleFileAsync(file);
                }

                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task HandleFileAsync(string path)
        {
            if (!_processingFiles.TryAdd(path, 0))
            {
                return;
            }

            try
            {
                if (!_allowedExtensions.Contains(Path.GetExtension(path)))
                {
                    return;
                }

                var maxWait = TimeSpan.FromMinutes(1);
                var interval = TimeSpan.FromMilliseconds(500);
                var waited = TimeSpan.Zero;

                while (waited < maxWait && !await IsFileReadyAsync(path))
                {
                    await Task.Delay(interval);
                    waited += interval;
                }

                if (!await IsFileReadyAsync(path))
                {
                    return;
                }

                try
                {
                    foreach (var profile in _profiles)
                    {
                        await Task.Run(() => processor.Process(path, profile));
                    }

                    switch (_settings.ProcessedFileOption)
                    {
                        case ProcessedFileOption.Delete:
                            File.Delete(path);
                            break;

                        case ProcessedFileOption.Move:
                            if (!string.IsNullOrEmpty(_settings.ProcessedDirectory))
                            {
                                var destination = Path.Combine(_settings.ProcessedDirectory, Path.GetFileName(path));
                                File.Move(path, destination, overwrite: true);
                            }
                            break;

                        case ProcessedFileOption.None:
                            break;

                        default:
                            throw new ArgumentOutOfRangeException($"Unexpected ProcessedFileOption: {_settings.ProcessedFileOption}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing {path}: {ex}");
                }
            }
            finally
            {
                _processingFiles.TryRemove(path, out _);
            }
        }

        private static async Task<bool> IsFileReadyAsync(string filePath)
        {
            try
            {
                await using var stream = new FileStream(
                    filePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.None,
                    bufferSize: 4096,
                    useAsync: true);

                var buffer = new byte[1];
                await stream.ReadExactlyAsync(buffer, 0, 0);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}