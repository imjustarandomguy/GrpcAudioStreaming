using Google.Protobuf;
using GrpcAudioStreaming.Server.Extensions;
using GrpcAudioStreaming.Server.Models;
using GrpcAudioStreaming.Server.Services.Recorders;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GrpcAudioStreaming.Server.Streamers.Grpc
{
    public class LoopbackGrpcStreamer : IGrpcStreamer, IDisposable
    {
        public event EventHandler<AudioSample> AudioSampleCreated;

        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ILoopbackAudioStreamerService _audioStreamerService;
        private AudioConsumer _consumer;

        public AudioFormat AudioFormat { get; }

        public LoopbackGrpcStreamer(ILoopbackAudioStreamerService audioStreamerService)
        {
            _audioStreamerService = audioStreamerService;
            _cancellationTokenSource = new CancellationTokenSource();
            AudioFormat = _audioStreamerService.WaveFormat.ToAudioFormat();
        }

        public Task StartStreaming(AudioConsumer consumer)
        {
            _consumer = consumer;
            _audioStreamerService.RegisterNewConsumer(consumer);
            return Stream(_cancellationTokenSource.Token);
        }

        public void StopStreaming()
        {
            _audioStreamerService.UnregisterConsumer(_consumer.Id);
            _cancellationTokenSource.Cancel();
        }

        private async Task Stream(CancellationToken cancellationToken)
        {
            var audioSample = new AudioSample();

            await foreach (var (data, lengthMs) in _audioStreamerService.Source.GetAsyncEnumerable(cancellationToken))
            {
                audioSample.Timestamp = DateTime.Now.AddMilliseconds(-lengthMs).ToString("o");
                audioSample.Data = UnsafeByteOperations.UnsafeWrap(data);

                OnAudioSampleCreated(audioSample);
            }
        }

        protected virtual void OnAudioSampleCreated(AudioSample audioSample)
        {
            AudioSampleCreated?.Invoke(this, audioSample);
        }

        public void Dispose()
        {
            _audioStreamerService.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}