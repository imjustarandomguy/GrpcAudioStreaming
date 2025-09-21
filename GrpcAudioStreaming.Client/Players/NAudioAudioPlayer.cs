using GrpcAudioStreaming.Client.Device;
using GrpcAudioStreaming.Client.Models;
using Microsoft.Extensions.Options;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Threading.Tasks;

namespace GrpcAudioStreaming.Client.Players
{
    public class NAudioAudioPlayer(IOptions<PlayerSettings> playerSettings, DeviceAccessor deviceAccessor) : IAudioPlayer, IDisposable
    {
        private readonly PlayerSettings _playerSettings = playerSettings.Value;
        private readonly DeviceAccessor _deviceAccessor = deviceAccessor;

        private BufferedWaveProvider _bufferedWaveProvider;
        private IWavePlayer _player;
        private WaveFormat _waveFormat;

        public bool Initialized { get; private set; }

        public string DeviceId { get; private set; }
        public PlaybackState PlaybackState => _player?.PlaybackState ?? PlaybackState.Stopped;

        public void Init(WaveFormat waveFormat)
        {
            _waveFormat = waveFormat;

            DeviceId = _deviceAccessor.DeviceId;
            InitPlayer(DeviceId);

            Initialized = true;
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

        public void SetDevice(string deviceId)
        {
            Stop();
            _player?.Dispose();

            DeviceId = deviceId;
            InitPlayer(DeviceId);

            Play();
        }

        public void Dispose()
        {
            Stop();
            _player?.Dispose();
            GC.SuppressFinalize(this);
        }

        private void InitPlayer(string deviceId)
        {
            _bufferedWaveProvider = new BufferedWaveProvider(_waveFormat)
            {
                BufferDuration = TimeSpan.FromMilliseconds(_playerSettings.BufferDuration),
                DiscardOnBufferOverflow = _playerSettings.DiscardOnBufferOverflow,
                ReadFully = _playerSettings.ReadFully,
            };

            var device = new MMDeviceEnumerator().GetDevice(deviceId);

            _player = new WasapiOut(device, AudioClientShareMode.Shared, true, _playerSettings.DesiredLatency);

            //_player = new DirectSoundOut(_deviceAccessor.Device, _playerSettings.DesiredLatency);

            //_player = new WaveOutEvent
            //{
            //    DeviceNumber = -1,
            //    DesiredLatency = _playerSettings.DesiredLatency,
            //};

            if (_playerSettings.Volume >= 0)
            {
                _player.Volume = _playerSettings.Volume;
            }

            _player.Init(_bufferedWaveProvider);
        }
    }
}