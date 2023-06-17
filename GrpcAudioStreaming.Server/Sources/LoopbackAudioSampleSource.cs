using Google.Protobuf;
using GrpcAudioStreaming.Server.Extensions;
using GrpcAudioStreaming.Server.Models;
using GrpcAudioStreaming.Server.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GrpcAudioStreaming.Server.Sources
{
    public class LoopbackAudioSampleSource : IAudioSampleSource, IDisposable
    {
        public event EventHandler<AudioSample> AudioSampleCreated;

        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly LoopbackAudioStreamerService _audioStreamer;
        private readonly string consumerId;

        public AudioFormat AudioFormat { get; }

        public LoopbackAudioSampleSource(LoopbackAudioStreamerService audioStreamer)
        {
            consumerId = Guid.NewGuid().ToString();

            _audioStreamer = audioStreamer;
            _cancellationTokenSource = new CancellationTokenSource();
            AudioFormat = _audioStreamer.WaveFormat.ToAudioFormat();
        }

        public Task StartStreaming()
        {
            _audioStreamer.RegisterNewConsumer(new LoopbackAudioConsumer { Id = consumerId });
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
                    Data = UnsafeByteOperations.UnsafeWrap(data),
                });
            }

            //var bytePool = ArrayPool<byte>.Shared;

            //await foreach (var data in _audioStreamer.Source.GetAsyncEnumerable(cancellationToken))
            //{
            //    // Rent a byte array from the pool
            //    var byteArray = bytePool.Rent(data.Length);

            //    // Copy the data to the rented byte array
            //    data.Span.CopyTo(byteArray);

            //    // Create the AudioSample with the ByteString
            //    OnAudioSampleCreated(new AudioSample
            //    {
            //        Timestamp = DateTime.Now.ToString("o"),
            //        Data = ByteString.CopyFrom(byteArray, 0, data.Length),
            //    });

            //    // Return the rented byte array to the pool
            //    bytePool.Return(byteArray);
            //}
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