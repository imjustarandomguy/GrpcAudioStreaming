using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace GrpcAudioStreaming.Client
{
    public class App
    {
        private readonly AppSettings _appSettings;
        private readonly IServiceProvider _serviceProvider;

        public App(IServiceProvider serviceProvider, IOptions<AppSettings> appSettings)
        {
            _serviceProvider = serviceProvider;
            _appSettings = appSettings.Value;
        }

        public async Task Run(string[] args)
        {
            while (true)
            {
                try
                {
                    var client = _serviceProvider.GetService<Client>();
                    await client.ReceiveAndPlayData();
                }
                catch (Exception)
                {
                    if (_appSettings.AttemptAutomaticReconnect)
                    {
                        await Task.Delay(_appSettings.AutomaticReconnectDelay * 1000);
                    }
                    else
                    {
                        Environment.Exit(0);
                    }
                }
            }
        }
    }
}