using Borderly.Config;
using Borderly.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Borderly
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var hostBuilder = Host.CreateDefaultBuilder(args);

            if (OperatingSystem.IsWindows())
            {
                hostBuilder.UseWindowsService();
            }
           
            hostBuilder
                .ConfigureAppConfiguration(config => config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true))
                .ConfigureServices((hostContext, services) =>
                {
                    var settings = hostContext.Configuration
                        .GetSection("Settings")
                        .Get<Settings>() ?? throw new InvalidOperationException("Missing Settings configuration");
                    services.AddSingleton(settings);

                    services.AddSingleton<IImageProcessor, ImageProcessor>();
                    services.AddHostedService<Worker>();
                })
                .Build()
                .Run();
        }
    }
}