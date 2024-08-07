﻿using GrpcAudioStreaming.Proto.Codecs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
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

            services.Configure<AppSettings>(configuration.GetSection(AppSettings.RootPath));

            services.AddSingleton<DeviceAccessor>();
            services.AddTransient<App>();
            services.AddScoped<AudioPlayer>();
            services.AddSingleton(CodecFactory.GetOrDefault(configuration.GetValue<string>($"{AppSettings.RootPath}:Codec")));
            services.AddSingleton<Client>();
            services.AddSingleton<CustomApplicationContext>();
        }
    }
}