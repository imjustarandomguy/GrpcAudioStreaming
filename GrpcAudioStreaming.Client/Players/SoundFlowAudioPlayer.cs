using GrpcAudioStreaming.Client.Models;
using Microsoft.Extensions.Options;
using NAudio.Wave;
using SoundFlow.Abstracts;
using SoundFlow.Abstracts.Devices;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Components;
using SoundFlow.Enums;
using SoundFlow.Providers;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcAudioStreaming.Client.Players
{
    public class SoundFlowAudioPlayer(IOptions<PlayerSettings> playerSettings) : IAudioPlayer, IDisposable
    {
        private readonly PlayerSettings _playerSettings = playerSettings.Value;

        private RawDataProvider _dataProvider;
        private MiniAudioEngine _audioEngine;
        private AudioPlaybackDevice _device;
        private RingBuffer _ringBuffer;
        private SoundPlayer _player;

        public bool Initialized { get; private set; }

        public string DeviceId { get; private set; }
        public NAudio.Wave.PlaybackState PlaybackState { get; private set; }

        public void Init(WaveFormat waveFormat)
        {
            _audioEngine = new MiniAudioEngine();

            var defaultPlaybackDevice = _audioEngine.PlaybackDevices.FirstOrDefault(d => d.IsDefault);
            if (defaultPlaybackDevice.Id == IntPtr.Zero)
            {
                Console.WriteLine("No default playback device found.");
                return;
            }

            var audioFormat = new SoundFlow.Structs.AudioFormat
            {
                Format = waveFormat.BitsPerSample == 16 ? SampleFormat.S16 : SampleFormat.F32,
                SampleRate = waveFormat.SampleRate,
                Channels = waveFormat.Channels,
            };

            _device = _audioEngine.InitializePlaybackDevice(defaultPlaybackDevice, audioFormat);

            int bytesPerSample = waveFormat.BitsPerSample / 8;
            int bufferSize = (_playerSettings.BufferDuration * waveFormat.SampleRate * waveFormat.Channels * bytesPerSample) / 1000;
            _ringBuffer = new RingBuffer(bufferSize);

            _dataProvider = new RawDataProvider(new RingBufferStream(_ringBuffer), audioFormat.Format, audioFormat.SampleRate, audioFormat.Channels);

            _player = new SoundPlayer(_audioEngine, audioFormat, _dataProvider);

            _device.Start();

            Initialized = true;
        }

        public void AddSample(byte[] sample)
        {
            _ringBuffer.Write(sample, _playerSettings.DiscardOnBufferOverflow);
        }

        public void Play()
        {
            PlaybackState = NAudio.Wave.PlaybackState.Playing;
            _player.Play();
        }

        public void Stop()
        {
            PlaybackState = NAudio.Wave.PlaybackState.Stopped;
            _player.Stop();
        }

        public async Task Restart()
        {
            Stop();

            // remove data currently in buffer
            while (_ringBuffer.Count > 0)
            {
                byte[] temp = new byte[1024];
                _ringBuffer.Read(temp, 0, Math.Min(temp.Length, _ringBuffer.Count));
            }

            await Task.Delay(100);

            Play();
        }

        public void Dispose()
        {
            _device?.Dispose();
            _audioEngine?.Dispose();
            _dataProvider?.Dispose();
            _player?.Dispose();
            GC.SuppressFinalize(this);
        }

        public void SetDevice(string deviceId)
        {
            DeviceId = deviceId;
            // SoundFlow currently does not support setting output device by GUID
            // This method is kept for interface compatibility
        }
    }

    public class RingBufferStream : Stream
    {
        private readonly RingBuffer _buffer;

        public RingBufferStream(RingBuffer buffer)
        {
            _buffer = buffer;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _buffer.Read(buffer, offset, count);
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }

    public class RingBuffer
    {
        private readonly byte[] _buffer;
        private int _head;
        private int _tail;
        private int _count;
        private readonly object _lock = new();

        public int Capacity => _buffer.Length;
        public int Count
        {
            get
            {
                lock (_lock) { return _count; }
            }
        }

        public RingBuffer(int capacity)
        {
            _buffer = new byte[capacity];
            _head = 0;
            _tail = 0;
            _count = 0;
        }

        public bool Write(byte[] data, bool discardOnOverflow)
        {
            lock (_lock)
            {
                if (data.Length > Capacity)
                    throw new ArgumentException("Data too large for buffer.");

                if (_count + data.Length > Capacity)
                {
                    if (discardOnOverflow)
                        return false; // Discard data
                    // Overwrite oldest data
                    int overflow = (_count + data.Length) - Capacity;
                    _tail = (_tail + overflow) % Capacity;
                    _count -= overflow;
                }

                int firstPart = Math.Min(data.Length, Capacity - _head);
                Array.Copy(data, 0, _buffer, _head, firstPart);
                if (data.Length > firstPart)
                {
                    Array.Copy(data, firstPart, _buffer, 0, data.Length - firstPart);
                }
                _head = (_head + data.Length) % Capacity;
                _count += data.Length;
                return true;
            }
        }

        public int Read(byte[] dest, int offset, int count)
        {
            lock (_lock)
            {
                int toRead = Math.Min(count, _count);
                int firstPart = Math.Min(toRead, Capacity - _tail);
                Array.Copy(_buffer, _tail, dest, offset, firstPart);
                if (toRead > firstPart)
                {
                    Array.Copy(_buffer, 0, dest, offset + firstPart, toRead - firstPart);
                }
                _tail = (_tail + toRead) % Capacity;
                _count -= toRead;
                return toRead;
            }
        }
    }
}