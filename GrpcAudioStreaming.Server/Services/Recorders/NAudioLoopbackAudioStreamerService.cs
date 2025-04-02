using GrpcAudioStreaming.Proto.Codecs;
using GrpcAudioStreaming.Server.Settings;
using GrpcAudioStreaming.Server.Sources;
using Microsoft.Extensions.Options;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Runtime.InteropServices;

namespace GrpcAudioStreaming.Server.Services.Recorders;

public class NAudioLoopbackAudioStreamerService(ICodec codec, IOptions<AudioSettings> audioSettings) : AbstractLoopbackAudioStreamerService(codec, audioSettings)
{
    private WasapiCapture _capture = null!;

    protected override void InitiateRecording()
    {
        base.InitiateRecording();

        // TODO: Force to use 16-bit PCM format for now. Need to add support for 32-bit float.
        WaveFormat = new WaveFormat(audioSettings.SampleRate, 16, audioSettings.Channels);

        _capture = new WasapiBufferedLoopbackCapture(useEventSync: true, audioBufferMillisecondsLength: 10) { WaveFormat = WaveFormat };

        _capture.DataAvailable += OnDataAvailable;

        _capture.RecordingStopped += OnRecordingStop;

        _capture.StartRecording();
    }

    protected override void StopRecording()
    {
        _capture?.StopRecording();
        _capture?.Dispose();

        base.StopRecording();
    }

    private void OnDataAvailable(object sender, WaveInEventArgs e)
    {
        var data = MemoryMarshal.AsBytes(e.Buffer.AsSpan(0, e.BytesRecorded));
        var encodedData = Encode(data);
        AddData(data, encodedData);
    }

    private void OnRecordingStop(object sender, StoppedEventArgs e)
    {
        _capture?.Dispose();
    }
}
