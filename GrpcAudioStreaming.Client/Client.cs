using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcAudioStreaming.Client.Extensions;
using GrpcAudioStreaming.Client.Models;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GrpcAudioStreaming.Client
{
    public class Client
    {
        private readonly AppSettings _appSettings;
        private readonly AudioPlayer _audioPlayer;
        private readonly AsyncServerStreamingCall<AudioSample> _audioStream;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public ClientState State = ClientState.None;

        public Client(AudioPlayer audioPlayer, IOptions<AppSettings> appSettings)
        {
            _audioPlayer = audioPlayer;
            _appSettings = appSettings.Value;
            _cancellationTokenSource = new CancellationTokenSource();

            var handler = new SocketsHttpHandler
            {
                PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                KeepAlivePingTimeout = TimeSpan.FromSeconds(30)
            };

            try
            {
                var channel = GrpcChannel.ForAddress(_appSettings.ServerUrl, new GrpcChannelOptions { HttpHandler = handler });
                var client = new AudioStream.AudioStreamClient(channel);
                var format = client.GetFormat(new Empty());

                _audioPlayer.Init(format.ToWaveFormat());
                _audioPlayer.Play();

                _audioStream = client.GetStream(new Empty());

                State = ClientState.Connected;
            }
            catch (Exception)
            {
                State = ClientState.Errored;
                throw;
            }
        }

        public async Task ReceiveAndPlayData()
        {
            await foreach (var sample in _audioStream.ResponseStream.ReadAllAsync(_cancellationTokenSource.Token))
            {
                //var succes = MemoryMarshal.TryGetArray(sample.Data.Memory, out var segment);

                //var content = succes
                //    ? new ByteArrayContent(segment.Array, segment.Offset, segment.Count)
                //    : new ByteArrayContent(sample.Data.ToByteArray());

                //_audioPlayer.AddSample(await content.ReadAsByteArrayAsync());

                _audioPlayer.AddSample(sample.Data.ToByteArray());
            }
        }

        public void Disconnect()
        {
            _cancellationTokenSource.Cancel();
            State = ClientState.GracefullyDisconnected;
        }
    }
}