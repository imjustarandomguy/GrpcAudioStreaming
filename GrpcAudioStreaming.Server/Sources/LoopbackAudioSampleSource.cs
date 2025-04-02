using Google.Protobuf;
using GrpcAudioStreaming.Server.Extensions;
using GrpcAudioStreaming.Server.Models;
using GrpcAudioStreaming.Server.Services.Recorders;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GrpcAudioStreaming.Server.Sources
{
    public class LoopbackAudioSampleSource : IAudioSampleSource, IDisposable
    {
        public event EventHandler<AudioSample> AudioSampleCreated;

        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ILoopbackAudioStreamerService _audioStreamer;
        private AudioConsumer _consumer;

        public AudioFormat AudioFormat { get; }

        public LoopbackAudioSampleSource(ILoopbackAudioStreamerService audioStreamer)
        {
            _audioStreamer = audioStreamer;
            _cancellationTokenSource = new CancellationTokenSource();
            AudioFormat = _audioStreamer.WaveFormat.ToAudioFormat();
        }

        public Task StartStreaming(AudioConsumer consumer)
        {
            _consumer = consumer;
            _audioStreamer.RegisterNewConsumer(consumer);
            return Stream(_cancellationTokenSource.Token);
        }

        public void StopStreaming()
        {
            _audioStreamer.UnregisterConsumer(_consumer.Id);
            _cancellationTokenSource.Cancel();
        }

        private async Task Stream(CancellationToken cancellationToken)
        {
            var audioSample = new AudioSample();

            await foreach (var (data, lengthMs) in _audioStreamer.Source.GetAsyncEnumerable(cancellationToken))
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
            _audioStreamer.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}