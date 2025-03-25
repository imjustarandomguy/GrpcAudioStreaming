using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcAudioStreaming.Proto.Codecs;
using GrpcAudioStreaming.Server.Models;
using GrpcAudioStreaming.Server.Sources;
using GrpcAudioStreaming.Server.Utils;
using System;
using System.Threading.Tasks;

namespace GrpcAudioStreaming.Server.Services
{
    public class GrpcAudioStreamService(ICodec codec, IAudioSampleSource audioSampleSource) : AudioStream.AudioStreamBase
    {
        private readonly IAudioSampleSource _audioSampleSource = audioSampleSource;

        public override Task GetStream(Empty request, IServerStreamWriter<AudioSample> responseStream, ServerCallContext context)
        {
            var audioConsumer = new AudioConsumer(Id: Guid.NewGuid().ToString(), Ip: new Uri(context.Peer.Replace("ipv4:", "ipv4://"))?.Host);

            var responseQueue = new GrpcStreamResponseQueue<AudioSample>(responseStream)
            {
                OnComplete = _audioSampleSource.StopStreaming,
            };

            _audioSampleSource.AudioSampleCreated += async (object sender, AudioSample audioSample) =>
            {
                await responseQueue.WriteAsync(audioSample);
            };

            return _audioSampleSource.StartStreaming(audioConsumer);
        }

        public override Task<AudioFormat> GetFormat(Empty request, ServerCallContext context)
        {
            _audioSampleSource.AudioFormat.Codec = CodecFactory.GetOrDefault(codec).ToString();
            _audioSampleSource.AudioFormat.Timestamp = DateTime.UtcNow.ToString("o");
            return Task.FromResult(_audioSampleSource.AudioFormat);
        }
    }
}
