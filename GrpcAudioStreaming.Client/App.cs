using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace GrpcAudioStreaming.Client
{
    public class App
    {
        private readonly AppSettings _appSettings;

        public App(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings?.Value ?? throw new ArgumentNullException(nameof(appSettings));
        }

        public async Task Run(string[] args)
        {
            while (true)
            {
                try
                {
                    using var client = new Client();
                    await client.ReceiveAndPlayData();
                }
                catch (Exception)
                {
                    await Task.Delay(60 * 1000);
                }
            }
        }
    }
}