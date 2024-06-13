using Microsoft.Extensions.Options;
using NAudio.Wave;
using System;

namespace GrpcAudioStreaming.Client
{
    public class AudioPlayer : DirectSoundOut
    {
        private readonly AppSettings _appSettings;
        private BufferedWaveProvider _bufferedWaveProvider;

        public bool Initilized { get; private set; }

        public AudioPlayer(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
            //DesiredLatency = _appSettings.PlayerDesiredLatency;
            Volume = _appSettings.Volume;
        }

        public void Init(WaveFormat waveFormat)
        {
            _bufferedWaveProvider = new BufferedWaveProvider(waveFormat)
            {
                BufferDuration = TimeSpan.FromMilliseconds(_appSettings.BufferDuration),
                DiscardOnBufferOverflow = _appSettings.DiscardOnBufferOverflow,
            };

            Init(_bufferedWaveProvider);

            Initilized = true;
        }

        public void AddSample(byte[] sample)
        {
            _bufferedWaveProvider.AddSamples(sample, 0, sample.Length);
        }

        public virtual new void Play()
        {
            _bufferedWaveProvider.ClearBuffer();
            base.Play();
        }

        public virtual new void Stop()
        {
            base.Stop();
            _bufferedWaveProvider.ClearBuffer();
        }
    }
}