using GrpcAudioStreaming.Server.Models;
using GrpcAudioStreaming.Server.Settings;
using GrpcAudioStreaming.Server.Utils;
using Microsoft.Extensions.Options;
using NAudio.Wave;
using System;
using System.Collections.Generic;

namespace GrpcAudioStreaming.Server.Services
{
    public partial class LoopbackAudioStreamerService : IDisposable
    {
        private Memory<byte> _buffer = new byte[1024];
        private readonly AudioSettings _audioSettings;
        private WasapiLoopbackCapture _capture = null!;

        public Dictionary<string, LoopbackAudioConsumer> Consumers { get; private set; } = new Dictionary<string, LoopbackAudioConsumer>();
        public AsyncEnumerableSource<Memory<byte>> Source { get; private set; } = new AsyncEnumerableSource<Memory<byte>>();
        public WaveFormat WaveFormat { get; private set; } = null!;

        public LoopbackAudioStreamerService(IOptions<AudioSettings> audioSettings)
        {
            _audioSettings = audioSettings.Value;
            WaveFormat = new WaveFormat(_audioSettings.SampleRate, _audioSettings.BitsPerSample, _audioSettings.Channels);
        }

        public void RegisterNewConsumer(LoopbackAudioConsumer consumer)
        {
            if (string.IsNullOrEmpty(consumer.Id)) return;

            Console.WriteLine($"Registering new consumer: {consumer.Id}");

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

            Console.WriteLine($"Unregistering consumer: {consumerId}");

            var removed = Consumers.Remove(consumerId);

            if (removed && Consumers.Count <= 0)
            {
                Console.WriteLine($"No consumers active. Stopping the recording.");
                Dispose();
            }
        }

        public void Dispose()
        {
            Consumers = new Dictionary<string, LoopbackAudioConsumer>();
            _capture?.StopRecording();
            GC.SuppressFinalize(this);
        }

        private void InitiateRecording()
        {
            Source = new AsyncEnumerableSource<Memory<byte>>();

            _capture = new WasapiLoopbackCapture { WaveFormat = WaveFormat };

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

            Source.YieldReturn(_buffer[..e.BytesRecorded]);
        }

        private void OnRecordingStop(object sender, StoppedEventArgs e)
        {
            _capture.Dispose();
            Source.Complete();
            _capture = null;
        }
    }
}