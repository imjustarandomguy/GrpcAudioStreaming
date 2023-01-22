using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using NAudio.Wave;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GrpcAudioStreaming.Client
{
    public class Client : IDisposable
    {
        private readonly AudioPlayer _audioPlayer;
        private readonly AsyncServerStreamingCall<AudioSample> _audioStream;

        public Client()
        {
            // When calling insecure gRPC services this switch must be set before creating the GrpcChannel/HttpClient.
            // https://docs.microsoft.com/en-us/aspnet/core/grpc/troubleshoot?view=aspnetcore-3.0#call-insecure-grpc-services-with-net-core-client
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            var handler = new SocketsHttpHandler
            {
                PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                KeepAlivePingTimeout = TimeSpan.FromSeconds(30)
            };

            var channel = GrpcChannel.ForAddress("http://10.0.0.221:5001", new GrpcChannelOptions
            {
                //HttpHandler = handler
            });
            var client = new AudioStream.AudioStreamClient(channel);
            var format = client.GetFormat(new Empty());

            _audioPlayer = new AudioPlayer(format.ToWaveFormat());
            _audioPlayer.Play();

            _audioStream = client.GetStream(new Empty());
        }

        public async Task ReceiveAndPlayData()
        {
            await foreach (var sample in _audioStream.ResponseStream.ReadAllAsync())
            {
                _audioPlayer.AddSample(sample.Data.ToByteArray());
            }
        }

        public void Dispose()
        {
            _audioStream?.Dispose();
            _audioPlayer?.Dispose();
        }
    }
}