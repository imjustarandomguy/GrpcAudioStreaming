using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Net.Http;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GrpcAudioStreaming.Client
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new CustomApplicationContext());

            while (true)
            {
                try
                {
                    var client = new Client();
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