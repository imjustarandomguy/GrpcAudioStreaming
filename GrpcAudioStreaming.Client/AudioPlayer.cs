using Microsoft.Extensions.Options;
using NAudio.Wave;
using System;
using System.Threading.Tasks;

namespace GrpcAudioStreaming.Client
{
    public class AudioPlayer : DirectSoundOut
    {
        private readonly AppSettings _appSettings;
        private BufferedWaveProvider _bufferedWaveProvider;

        public bool Initialized { get; private set; }

        public AudioPlayer(IOptions<AppSettings> appSettings, DeviceAccessor deviceAccessor)
            : base(deviceAccessor.Device, appSettings.Value.PlayerDesiredLatency)
        {
            _appSettings = appSettings.Value;
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

            Initialized = true;
        }

        public void AddSample(byte[] sample)
        {
            if (PlaybackState == PlaybackState.Playing)
            {
                _bufferedWaveProvider.AddSamples(sample, 0, sample.Length);
            }
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

        public async Task Restart()
        {
            base.Stop();

            _bufferedWaveProvider.ClearBuffer();

            await Task.Delay(100);

            base.Play();
        }
    }
}