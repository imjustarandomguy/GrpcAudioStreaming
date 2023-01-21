﻿using AudioSharer.Models;
using AudioSharer.Utils;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AudioSharer
{
    public class AudioStreamerService
    {
        private WasapiLoopbackCapture _capture = null!;

        public Dictionary<string, Consumer> Consumers { get; private set; } = new Dictionary<string, Consumer>();

        public AsyncEnumerableSource<AudioNode> Source { get; private set; } = new AsyncEnumerableSource<AudioNode>();

        public WaveFormat WaveFormat { get; private set; } = null!;

        public void RegisterNewConsumer(Consumer consumer)
        {
            if (string.IsNullOrEmpty(consumer.Id)) return;

            Console.WriteLine($"Registering new consumer {consumer.Id}");

            Consumers.Add(consumer.Id, consumer);

            if (Consumers.Count == 1)
            {
                Console.WriteLine($"Consumer detected. Starting the recording.");
                InitiateRecording();
            }
        }

        public void UnregisterConsumer(string? consumerId)
        {
            if (string.IsNullOrEmpty(consumerId)) return;

            Console.WriteLine($"Unregistering new consumer {consumerId}");

            var removed = Consumers.Remove(consumerId);

            if (removed && Consumers.Count <= 0)
            {
                Console.WriteLine($"No consumers active. Stopping the recording.");
                Dispose();
            }
        }

        public void Dispose()
        {
            Consumers = new Dictionary<string, Consumer>();
            _capture?.StopRecording();
        }

        private void InitiateRecording()
        {
            _capture = new WasapiLoopbackCapture
            {
                WaveFormat = new WaveFormat(44100, 16, 2)
            };

            Source = new AsyncEnumerableSource<AudioNode>();
            WaveFormat = _capture.WaveFormat;

            _capture.DataAvailable += OnDataAvailable;

            _capture.RecordingStopped += OnRecordingStop;

            _capture.StartRecording();
        }

        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            var node = new AudioNode
            {
                Data = e.Buffer,
            };

            Source.YieldReturn(node);
        }

        private void OnRecordingStop(object sender, StoppedEventArgs e)
        {
            _capture.Dispose();
            Source.Complete();
        }
    }
}