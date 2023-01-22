using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GrpcAudioStreaming.Client
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            new Thread(delegate ()
            {
                Application.Run(new CustomApplicationContext());
            }).Start();

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