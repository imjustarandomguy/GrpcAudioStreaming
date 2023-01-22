using Google.Protobuf;
using GrpcAudioStreaming.Server.Extensions;
using GrpcAudioStreaming.Server.Models;
using GrpcAudioStreaming.Server.Services;
using NAudio.Wave;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GrpcAudioStreaming.Server.Sources
{
    public class LoopbackAudioSampleSource : IAudioSampleSource, IDisposable
    {
        public event EventHandler<AudioSample> AudioSampleCreated;

        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly AudioStreamerService _audioStreamer;
        private readonly string consumerId;

        public AudioFormat AudioFormat { get; }

        public LoopbackAudioSampleSource(AudioStreamerService audioStreamer)
        {
            consumerId = Guid.NewGuid().ToString();

            _audioStreamer = audioStreamer;
            _cancellationTokenSource = new CancellationTokenSource();
            AudioFormat = new WaveFormat(44100, 16, 2).ToAudioFormat();
        }

        public Task StartStreaming()
        {
            _audioStreamer.RegisterNewConsumer(new Consumer { Id = consumerId });
            return Stream(_cancellationTokenSource.Token);
        }

        public void StopStreaming()
        {
            _audioStreamer.UnregisterConsumer(consumerId);
            _cancellationTokenSource.Cancel();
        }

        private async Task Stream(CancellationToken cancellationToken)
        {
            await foreach (var data in _audioStreamer.Source.GetAsyncEnumerable(cancellationToken))
            {
                OnAudioSampleCreated(new AudioSample
                {
                    Timestamp = DateTime.Now.ToString("o"),
                    Data = ByteString.CopyFrom(data),
                });
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