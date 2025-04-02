using GrpcAudioStreaming.Proto.Codecs;
using GrpcAudioStreaming.Server.Settings;
using Microsoft.Extensions.Options;
using SoundFlow.Abstracts;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Components;
using SoundFlow.Enums;
using System;
using System.Runtime.InteropServices;

namespace GrpcAudioStreaming.Server.Services.Recorders;

public class SoundFlowLoopbackAudioStreamerService(ICodec codec, IOptions<AudioSettings> audioSettings) : AbstractLoopbackAudioStreamerService(codec, audioSettings)
{
    private AudioEngine _audioEngine;
    private Recorder _recorder;

    protected override void InitiateRecording()
    {
        base.InitiateRecording();

        // TODO: Force to use 32-bit float format for now. Need to add support for 16-bit PCM.
        WaveFormat = NAudio.Wave.WaveFormat.CreateIeeeFloatWaveFormat(audioSettings.SampleRate, audioSettings.Channels);

        var sampleFormat = WaveFormat.BitsPerSample == 16 ? SampleFormat.S16 : SampleFormat.F32;

        _audioEngine = new MiniAudioEngine(WaveFormat.SampleRate, Capability.Loopback, sampleFormat: sampleFormat);

        _recorder = new Recorder(ProcessAudio, sampleFormat: sampleFormat, sampleRate: WaveFormat.SampleRate, channels: WaveFormat.Channels);

        _recorder.StartRecording();
    }

    protected override void StopRecording()
    {
        _recorder?.StopRecording();
        _recorder?.Dispose();
        _audioEngine?.Dispose();

        base.StopRecording();
    }

    private void ProcessAudio(Span<float> samples, Capability capability)
    {
        var data = MemoryMarshal.AsBytes(samples);
        var encodedData = Encode(data);
        AddData(data, encodedData);
    }
}
