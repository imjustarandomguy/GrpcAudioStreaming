using NAudio.Wave;
using System;

namespace GrpcAudioStreaming.Client
{
    public class AudioPlayer : IDisposable
    {
        private readonly IWavePlayer _wavePlayer;
        private readonly BufferedWaveProvider _bufferedWaveProvider;

        public AudioPlayer(WaveFormat waveFormat)
        {
            _wavePlayer = new WaveOutEvent
            {
                DesiredLatency = 100,
            };
            _bufferedWaveProvider = new BufferedWaveProvider(waveFormat) { BufferDuration = TimeSpan.FromSeconds(1) };
            _wavePlayer.Init(_bufferedWaveProvider);
        }

        public void AddSample(byte[] sample)
        {
            _bufferedWaveProvider.AddSamples(sample, 0, sample.Length);

            if (_bufferedWaveProvider.BufferedDuration.TotalMilliseconds > 250)
            {
                if (_wavePlayer.PlaybackState != PlaybackState.Playing)
                {
                    _wavePlayer.Play();
                }
            }
        }

        public void Play()
        {
            _wavePlayer.Play();
        }

        public void Stop()
        {
            _wavePlayer.Stop();
        }

        public void Dispose()
        {
            _wavePlayer.Stop();
            _wavePlayer.Dispose();
        }
    }
}