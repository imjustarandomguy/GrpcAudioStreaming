using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace GrpcAudioStreaming.Client
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            try
            {
                // When calling insecure gRPC services this switch must be set before creating the GrpcChannel/HttpClient.
                // https://docs.microsoft.com/en-us/aspnet/core/grpc/troubleshoot?view=aspnetcore-3.0#call-insecure-grpc-services-with-net-core-client
                AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

                var handler = new SocketsHttpHandler
                {
                    PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                    KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                    KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
                    EnableMultipleHttp2Connections = true
                };

                var channel = GrpcChannel.ForAddress("http://10.0.0.221:5001", new GrpcChannelOptions
                {
                    HttpHandler = handler
                });
                var client = new AudioStream.AudioStreamClient(channel);
                var format = client.GetFormat(new Empty());
                var audioStream = client.GetStream(new Empty(), null, DateTime.UtcNow.AddHours(5));

                using var audioPlayer = new AudioPlayer(format.ToWaveFormat());
                audioPlayer.Play();

                await foreach (var sample in audioStream.ResponseStream.ReadAllAsync())
                {
                    audioPlayer.AddSample(sample.Data.ToByteArray());
                }
            }
            catch (Exception ex) { }
        }
    }
}