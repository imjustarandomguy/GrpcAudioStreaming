using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcAudioStreaming.Server.Models;
using GrpcAudioStreaming.Server.Sources;
using System;
using System.Threading.Tasks;

namespace GrpcAudioStreaming.Server.Services
{
    public class GrpcAudioStreamService : AudioStream.AudioStreamBase
    {
        private readonly IAudioSampleSource _audioSampleSource;
        private IServerStreamWriter<AudioSample> _responseStream;

        public GrpcAudioStreamService(IAudioSampleSource audioSampleSource)
        {
            _audioSampleSource = audioSampleSource;
        }

        public override Task GetStream(Empty request, IServerStreamWriter<AudioSample> responseStream, ServerCallContext context)
        {
            var audioConsumer = new AudioConsumer(Id: Guid.NewGuid().ToString(), Ip: new Uri(context.Peer.Replace("ipv4:", "ipv4://"))?.Host);

            _responseStream = responseStream;
            _audioSampleSource.AudioSampleCreated += OnAudioSampleCreated;

            return _audioSampleSource.StartStreaming(audioConsumer);
        }

        public override Task<AudioFormat> GetFormat(Empty request, ServerCallContext context)
        {
            return Task.FromResult(_audioSampleSource.AudioFormat);
        }

        private async void OnAudioSampleCreated(object sender, AudioSample audioSample)
        {
            try
            {
                await _responseStream.WriteAsync(audioSample);
            }
            catch (Exception)
            {
                _audioSampleSource.StopStreaming();
            }
        }
    }
}
