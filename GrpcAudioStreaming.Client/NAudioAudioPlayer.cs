using Microsoft.Extensions.Options;
using NAudio.Wave;
using System;
using System.Threading.Tasks;

namespace GrpcAudioStreaming.Client
{
    public class NAudioAudioPlayer(IOptions<AppSettings> appSettings, DeviceAccessor deviceAccessor) : IDisposable
    {
        private readonly AppSettings _appSettings = appSettings.Value;
        private readonly DeviceAccessor _deviceAccessor = deviceAccessor;

        private BufferedWaveProvider _bufferedWaveProvider;
        private WaveOutEvent _player;
        private WaveFormat _waveFormat;

        public bool Initialized { get; private set; }

        public Guid Device { get; private set; }
        public PlaybackState PlaybackState => _player?.PlaybackState ?? PlaybackState.Stopped;

        public void Init(WaveFormat waveFormat)
        {
            _waveFormat = waveFormat;

            Device = _deviceAccessor.Device;
            InitPlayer(Device);

            Initialized = true;
        }

        public void InitPlayer(Guid deviceId)
        {
            _bufferedWaveProvider = new BufferedWaveProvider(_waveFormat)
            {
                BufferDuration = TimeSpan.FromMilliseconds(_appSettings.BufferDuration),
                DiscardOnBufferOverflow = _appSettings.DiscardOnBufferOverflow,
            };

            _player = new WaveOutEvent
            {
                DeviceNumber = -1,
                DesiredLatency = _appSettings.PlayerDesiredLatency,
                Volume = _appSettings.Volume,
            };

            _player.Init(_bufferedWaveProvider);
        }

        public void AddSample(byte[] sample)
        {
            if (PlaybackState == PlaybackState.Playing)
            {
                _bufferedWaveProvider.AddSamples(sample, 0, sample.Length);
            }
        }

        public void Play()
        {
            _bufferedWaveProvider?.ClearBuffer();
            _player?.Play();
        }

        public void Stop()
        {
            _player?.Stop();
            _bufferedWaveProvider?.ClearBuffer();
        }

        public async Task Restart()
        {
            _player?.Stop();
            _bufferedWaveProvider?.ClearBuffer();

            await Task.Delay(100);

            _player?.Play();
        }

        public void SetDevice(Guid deviceId)
        {
            Stop();
            _player?.Dispose();

            Device = deviceId;
            InitPlayer(Device);

            Play();
        }

        public void Dispose()
        {
            Stop();
            _player?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}