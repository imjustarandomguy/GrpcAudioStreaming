using GrpcAudioStreaming.Proto.Codecs;
using GrpcAudioStreaming.Server.Models;
using GrpcAudioStreaming.Server.Utils;
using NAudio.Wave;
using System;
using System.Collections.Generic;

namespace GrpcAudioStreaming.Server.Services.Recorders;

public interface ILoopbackAudioStreamerService
{
    public Dictionary<string, AudioConsumer> Consumers { get; }
    public AsyncEnumerableSource<(ReadOnlyMemory<byte> data, double lengthMs)> Source { get; }
    public WaveFormat WaveFormat { get; }

    public void SetCodec(ICodec codec);

    public void SetWaveFormat(int sampleRate, int bits, int channels);

    public void RegisterNewConsumer(AudioConsumer consumer);

    public void UnregisterConsumer(string consumerId);

    public void Dispose();
}
