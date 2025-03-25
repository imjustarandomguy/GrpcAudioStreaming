using GrpcAudioStreaming.Proto.Codecs;
using GrpcAudioStreaming.Server.Models;
using GrpcAudioStreaming.Server.Settings;
using GrpcAudioStreaming.Server.Sources;
using GrpcAudioStreaming.Server.Utils;
using Microsoft.Extensions.Options;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;

namespace GrpcAudioStreaming.Server.Services
{
    public partial class LoopbackAudioStreamerService : IDisposable
    {
        private readonly ICodec _codec;
        private byte[] _buffer = new byte[1024];
        private readonly AudioSettings _audioSettings;
        private WasapiCapture _capture = null!;

        public Dictionary<string, AudioConsumer> Consumers { get; private set; } = new Dictionary<string, AudioConsumer>();
        public AsyncEnumerableSource<(Memory<byte> data, double lengthMs)> Source { get; private set; } = new AsyncEnumerableSource<(Memory<byte> data, double lengthMs)>();
        public WaveFormat WaveFormat { get; private set; } = null!;

        public LoopbackAudioStreamerService(ICodec codec, IOptions<AudioSettings> audioSettings)
        {
            _codec = codec;
            _audioSettings = audioSettings.Value;
            WaveFormat = new WaveFormat(_audioSettings.SampleRate, _audioSettings.BitsPerSample, _audioSettings.Channels);
        }

        public void RegisterNewConsumer(AudioConsumer consumer)
        {
            if (string.IsNullOrEmpty(consumer.Id)) return;

            Console.WriteLine($"Registering new consumer. Id: {consumer.Id}, Ip: {consumer.Ip}");

            Consumers.Add(consumer.Id, consumer);

            if (Consumers.Count == 1)
            {
                Console.WriteLine($"Consumer detected. Starting the recording.");
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
                Dispose();
            }
        }

        public void Dispose()
        {
            Consumers = [];
            _capture?.StopRecording();
            GC.SuppressFinalize(this);
        }

        private void InitiateRecording()
        {
            Source = new AsyncEnumerableSource<(Memory<byte> data, double lengthMs)>();

            _capture = new WasapiBufferedLoopbackCapture(useEventSync: true, audioBufferMillisecondsLength: 10) { WaveFormat = WaveFormat };

            _capture.DataAvailable += OnDataAvailable;

            _capture.RecordingStopped += OnRecordingStop;

            _capture.StartRecording();
        }

        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            if (_buffer.Length < e.BytesRecorded)
            {
                _buffer = new byte[e.BytesRecorded];
            }

            e.Buffer.AsMemory()[..e.BytesRecorded].CopyTo(_buffer);

            Source.YieldReturn((_codec.Encode(_buffer, 0, e.BytesRecorded), GetRecordingLengthInMs(e.BytesRecorded)));
        }

        private void OnRecordingStop(object sender, StoppedEventArgs e)
        {
            _capture.Dispose();
            Source.Complete();
            _capture = null;
        }

        private double GetRecordingLengthInMs(int bytesRecorded)
        {
            return bytesRecorded / (double)(WaveFormat.SampleRate * WaveFormat.Channels * (WaveFormat.BitsPerSample / 8)) * 1000;
        }
    }
}
