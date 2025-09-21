using GrpcAudioStreaming.Proto.Codecs;
using GrpcAudioStreaming.Server.Models;
using GrpcAudioStreaming.Server.Settings;
using GrpcAudioStreaming.Server.Utils;
using Microsoft.Extensions.Options;
using NAudio.Wave;
using System;
using System.Collections.Generic;

namespace GrpcAudioStreaming.Server.Services.Recorders;

public abstract class AbstractLoopbackAudioStreamerService : ILoopbackAudioStreamerService, IDisposable
{
    protected readonly AudioSettings audioSettings;
    protected ICodec Codec { get; private set; }

    public Dictionary<string, AudioConsumer> Consumers { get; private set; } = [];
    public AsyncEnumerableSource<(ReadOnlyMemory<byte> data, double lengthMs)> Source { get; protected set; } = new AsyncEnumerableSource<(ReadOnlyMemory<byte> data, double lengthMs)>();
    public WaveFormat WaveFormat { get; protected set; } = null!;

    public AbstractLoopbackAudioStreamerService(ICodec codec, IOptions<AudioSettings> audioSettings)
    {
        this.audioSettings = audioSettings.Value;

        WaveFormat = this.audioSettings.BitsPerSample == 16
            ? new WaveFormat(this.audioSettings.SampleRate, this.audioSettings.BitsPerSample, this.audioSettings.Channels)
            : WaveFormat.CreateIeeeFloatWaveFormat(this.audioSettings.SampleRate, this.audioSettings.Channels);
        Codec = codec;

        codec.Initialize(WaveFormat);
    }

    public void SetWaveFormat(int sampleRate, int bits, int channels)
    {
        WaveFormat = new WaveFormat(sampleRate, bits, channels);
    }

    public void SetCodec(ICodec codec)
    {
        Codec = codec;
    }

    public void RegisterNewConsumer(AudioConsumer consumer)
    {
        if (string.IsNullOrEmpty(consumer.Id)) return;

        Console.WriteLine($"Registering new consumer. Id: {consumer.Id}, Ip: {consumer.Ip}");

        Consumers.Add(consumer.Id, consumer);

        if (Consumers.Count == 1)
        {
            Console.WriteLine($"Consumer detected. Starting the recording. SampleRate: {WaveFormat.SampleRate}, Channels: {WaveFormat.Channels}, BitsPerSample: {WaveFormat.BitsPerSample}.");
            InitiateRecording();
        }
    }

    public void UnregisterConsumer(string consumerId)
    {
        if (string.IsNullOrEmpty(consumerId)) return;

        var consumer = Consumers.GetValueOrDefault(consumerId);

        if (consumer is null) return;

        Console.WriteLine($"Unregistering consumer. Id: {consumer.Id}, Ip: {consumer.Ip}");

        var removed = Consumers.Remove(consumer.Id);

        if (removed && Consumers.Count <= 0)
        {
            Console.WriteLine($"No consumers active. Stopping the recording.");
            StopRecording();
        }
    }

    public void Dispose()
    {
        Consumers = [];
        Source?.Complete(); 
        GC.SuppressFinalize(this);
    }

    protected double GetPcmRecordingLengthInMs(int bytesRecorded)
    {
        return bytesRecorded / (double)(WaveFormat.SampleRate * WaveFormat.Channels * (WaveFormat.BitsPerSample / 8)) * 1000;
    }

    protected ReadOnlyMemory<byte> Encode(ReadOnlySpan<byte> data)
    {
        int maxOutputSize = Codec.GetMaxEncodedSize(data.Length);
        var outputBuffer = System.Buffers.ArrayPool<byte>.Shared.Rent(maxOutputSize);

        try
        {
            int encodedLength = Codec.Encode(data, outputBuffer);
            return new ReadOnlyMemory<byte>(outputBuffer, 0, encodedLength);
        }
        finally
        {
            System.Buffers.ArrayPool<byte>.Shared.Return(outputBuffer);
        }
    }

    protected void AddData(ReadOnlySpan<byte> data, ReadOnlyMemory<byte> encodedData)
    {
        Source.YieldReturn((encodedData, GetPcmRecordingLengthInMs(data.Length)));
    }

    protected virtual void InitiateRecording()
    {
        Source = new AsyncEnumerableSource<(ReadOnlyMemory<byte> data, double lengthMs)>();
    }

    protected virtual void StopRecording()
    {
        Dispose();
    }
}
