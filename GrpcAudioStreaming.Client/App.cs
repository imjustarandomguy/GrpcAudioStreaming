using GrpcAudioStreaming.Client.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace GrpcAudioStreaming.Client
{
    public class App(IServiceProvider serviceProvider, IOptions<ClientSettings> clientSettings)
    {
        private readonly ClientSettings _clientSettings = clientSettings.Value;
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

                    if (_clientSettings.AttemptAutomaticReconnect)
                    {
                        await Task.Delay(_clientSettings.AutomaticReconnectDelay * 1000);
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