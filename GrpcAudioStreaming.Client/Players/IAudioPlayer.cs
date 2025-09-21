using NAudio.Wave;
using System;
using System.Threading.Tasks;

namespace GrpcAudioStreaming.Client.Players
{
    public interface IAudioPlayer
    {
        Guid Device { get; }
        bool Initialized { get; }
        PlaybackState PlaybackState { get; }

        void AddSample(byte[] sample);
        void Dispose();
        void Init(WaveFormat waveFormat);
        void Play();
        Task Restart();
        void SetDevice(Guid deviceId);
        void Stop();
    }
}