using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcAudioStreaming.Client.Device;
using GrpcAudioStreaming.Client.Extensions;
using GrpcAudioStreaming.Client.Models;
using GrpcAudioStreaming.Client.Players;
using GrpcAudioStreaming.Proto.Codecs;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GrpcAudioStreaming.Client
{
    public class Client
    {
        private readonly ICodec _codec;
        private readonly IAudioPlayer _audioPlayer;
        private readonly ClientSettings _clientSettings;
        private readonly AsyncServerStreamingCall<AudioSample> _audioStream;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public ClientState State = ClientState.None;

        public Client(IAudioPlayer audioPlayer, DefaultAudioDeviceChangeHandler defaultAudioDeviceChangeHandler, IOptions<ClientSettings> clientSettings)
        {
            _audioPlayer = audioPlayer;
            _clientSettings = clientSettings.Value;
            _cancellationTokenSource = new CancellationTokenSource();

            var handler = new SocketsHttpHandler
            {
                PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
            };

            try
            {
                var channel = GrpcChannel.ForAddress(_clientSettings.ServerUrl, new GrpcChannelOptions { HttpHandler = handler });
                var client = new AudioStream.AudioStreamClient(channel);
                var format = client.GetFormat(new Empty());
                var waveFormat = format.ToWaveFormat();

                _codec = CodecFactory.GetOrDefault(format.Codec);
                _codec.Initialize(waveFormat);

                _audioPlayer.Init(format.ToWaveFormat());
                _audioPlayer.Play();

                _audioStream = client.GetStream(new Empty());

                State = ClientState.Connected;

                defaultAudioDeviceChangeHandler.Init(_audioPlayer);
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
                var decoded = new byte[_codec.GetMaxDecodedSize(sample.Data.Length)];
                var decodedLength = _codec.Decode(sample.Data.Span, decoded);

                _audioPlayer.AddSample(decoded.AsSpan(0, decodedLength).ToArray());
            }
        }

        public void Disconnect()
        {
            _cancellationTokenSource.Cancel();
            State = ClientState.GracefullyDisconnected;
        }
    }
}