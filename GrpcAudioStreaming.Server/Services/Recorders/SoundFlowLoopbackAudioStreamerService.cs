using GrpcAudioStreaming.Proto.Codecs;
using GrpcAudioStreaming.Server.Settings;
using Microsoft.Extensions.Options;
using SoundFlow.Abstracts.Devices;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Components;
using SoundFlow.Enums;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace GrpcAudioStreaming.Server.Services.Recorders;

public class SoundFlowLoopbackAudioStreamerService(ICodec codec, IOptions<AudioSettings> audioSettings) : AbstractLoopbackAudioStreamerService(codec, audioSettings)
{
    private MiniAudioEngine _audioEngine;
    private AudioCaptureDevice _device;
    private Recorder _recorder;

    protected override void InitiateRecording()
    {
        base.InitiateRecording();

        WaveFormat = NAudio.Wave.WaveFormat.CreateIeeeFloatWaveFormat(audioSettings.SampleRate, audioSettings.Channels);
        //WaveFormat = new WaveFormat(audioSettings.SampleRate, 16, audioSettings.Channels);

        var defaultCaptureDevice = _audioEngine.CaptureDevices.FirstOrDefault(d => d.IsDefault);
        if (defaultCaptureDevice.Id == IntPtr.Zero)
        {
            Console.WriteLine("No default capture device found.");
            return;
        }

        var audioFormat = new SoundFlow.Structs.AudioFormat
        {
            Format = WaveFormat.BitsPerSample == 16 ? SampleFormat.S16 : SampleFormat.F32,
            SampleRate = WaveFormat.SampleRate,
            Channels = WaveFormat.Channels,
        };

        _audioEngine = new MiniAudioEngine();
        _device = _audioEngine.InitializeLoopbackDevice(audioFormat);

        _recorder = new Recorder(_device, ProcessAudio);

        _recorder.StartRecording();
    }

    protected override void StopRecording()
    {
        _recorder?.StopRecording();
        _recorder?.Dispose();

        _device?.Stop();
        _device?.Dispose();

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
