using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace GrpcAudioStreaming.Client
{
    public class App(IServiceProvider serviceProvider, IOptions<AppSettings> appSettings)
    {
        private readonly AppSettings _appSettings = appSettings.Value;
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        public async Task Run(string[] args)
        {
            Client client = null;

            while (true)
            {
                try
                {
                    client ??= _serviceProvider.GetService<Client>();
                    await client.ReceiveAndPlayData();
                }
                catch (Exception)
                {
                    client?.Disconnect();
                    client = null;

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