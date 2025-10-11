using NAudio.Wave;
using System;

namespace GrpcAudioStreaming.Client.Extensions
{
    public static class WaveFormatExtensions
    {
        public static WaveFormat ToWaveFormat(this AudioFormat audioFormat)
        {
            return WaveFormat.CreateCustomFormat(
                WaveFormatEncoding.Pcm,
                audioFormat.SampleRate,
                audioFormat.Channels,
                audioFormat.AverageBytesPerSecond,
                audioFormat.BlockAlign,
                audioFormat.BitsPerSample);
        }
    }
}