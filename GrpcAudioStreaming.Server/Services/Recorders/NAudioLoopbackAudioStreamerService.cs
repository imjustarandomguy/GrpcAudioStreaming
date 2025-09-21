using GrpcAudioStreaming.Proto;
using GrpcAudioStreaming.Proto.Codecs;
using GrpcAudioStreaming.Server.Settings;
using GrpcAudioStreaming.Server.Sources;
using Microsoft.Extensions.Options;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Runtime.InteropServices;

namespace GrpcAudioStreaming.Server.Services.Recorders;

public class NAudioLoopbackAudioStreamerService : AbstractLoopbackAudioStreamerService
{
    private string _deviceId = WasapiBufferedLoopbackCapture.GetDefaultLoopbackCaptureDevice().ID;
    private WasapiCapture _capture = null!;

    public NAudioLoopbackAudioStreamerService(ICodec codec, IOptions<AudioSettings> audioSettings) : base(codec, audioSettings)
    {
        DefaultAudioDeviceChangeHandler.Init((deviceId) =>
        {
            if (string.IsNullOrEmpty(deviceId) || deviceId == _deviceId)
            {
                return;
            }

            _capture?.StopRecording();
            _capture?.Dispose();

            _deviceId = deviceId;
            InitiateCapture(_deviceId);
        });
    }

    protected override void InitiateRecording()
    {
        base.InitiateRecording();

        // TODO: Force to use 16-bit PCM format for now. Need to add support for 32-bit float.
        WaveFormat = new WaveFormat(audioSettings.SampleRate, 16, audioSettings.Channels);

        InitiateCapture(_deviceId);
    }

    protected override void StopRecording()
    {
        _capture?.StopRecording();
        _capture?.Dispose();

        base.StopRecording();
    }

    private void InitiateCapture(string deviceId)
    {
        var device = new MMDeviceEnumerator().GetDevice(deviceId);

        _capture = new WasapiBufferedLoopbackCapture(device, useEventSync: true, audioBufferMillisecondsLength: 10) { WaveFormat = WaveFormat };

        _capture.DataAvailable += OnDataAvailable;

        _capture.RecordingStopped += OnRecordingStop;

        _capture.StartRecording();
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
