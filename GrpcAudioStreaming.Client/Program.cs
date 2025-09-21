using GrpcAudioStreaming.Client.Device;
using GrpcAudioStreaming.Client.Models;
using GrpcAudioStreaming.Client.Players;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GrpcAudioStreaming.Client
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            using var serviceProvider = serviceCollection.BuildServiceProvider();

            new Thread(delegate ()
            {
                Application.Run(serviceProvider.GetService<CustomApplicationContext>());
            }).Start();

            SetHighPriority();

            await serviceProvider.GetService<App>().Run(args);
        }

        private static void SetHighPriority()
        {
            using Process p = Process.GetCurrentProcess();
            p.PriorityClass = ProcessPriorityClass.High;
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            services.Configure<ClientSettings>(configuration.GetSection(ClientSettings.SectionName));
            services.Configure<PlayerSettings>(configuration.GetSection(PlayerSettings.SectionName));

            services.AddSingleton<DeviceAccessor>();
            services.AddTransient<App>();
            services.AddScoped(provider =>
            {
                var deviceAccessor = provider.GetRequiredService<DeviceAccessor>();
                var playerSettings = provider.GetRequiredService<IOptions<PlayerSettings>>();

                return PlayerEngineFactory.GetOrDefault(configuration.GetValue<string>("Engine"), deviceAccessor, playerSettings);
            });
            services.AddSingleton<Client>();
            services.AddSingleton<CustomApplicationContext>();
            services.AddScoped<DefaultAudioDeviceChangeHandler>();
        }
    }
}