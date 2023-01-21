using Google.Protobuf;
using NAudio.Wave;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GrpcAudioStreaming.Server
{
    public class LoopbackAudioSampleSource : IAudioSampleSource, IDisposable
    {
        public event EventHandler<AudioSample> AudioSampleCreated;

        private readonly WasapiLoopbackCapture _capture;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public AudioFormat AudioFormat { get; }

        public LoopbackAudioSampleSource()
        {
            _capture = new WasapiLoopbackCapture
            {
                WaveFormat = new WaveFormat(44100, 16, 2)
            };
            AudioFormat = _capture.WaveFormat.ToAudioFormat();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public Task StartStreaming()
        {
            return Stream(_capture, _cancellationTokenSource.Token);
        }

        public void StopStreaming()
        {
            _cancellationTokenSource.Cancel();
        }

        private async Task Stream(WasapiLoopbackCapture stream, CancellationToken cancellationToken)
        {
            stream.DataAvailable += OnDataAvailable;

            stream.RecordingStopped += OnRecordingStop;

            stream.StartRecording();

            try
            {
                await Task.Delay(-1, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                stream.StopRecording();
            }
        }

        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            var data = e.Buffer;

            var audioSample = new AudioSample
            {
                Timestamp = DateTime.Now.ToString("o"),
                Data = ByteString.CopyFrom(data),
            };

            OnAudioSampleCreated(audioSample);
        }

        private void OnRecordingStop(object sender, StoppedEventArgs e)
        {
            _capture.Dispose();
        }

        protected virtual void OnAudioSampleCreated(AudioSample audioSample)
        {
            AudioSampleCreated?.Invoke(this, audioSample);
        }

        public void Dispose()
        {
            _capture.Dispose();
        }
    }
}