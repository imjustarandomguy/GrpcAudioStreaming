using GrpcAudioStreaming.Server.Models;
using System;
using System.Threading.Tasks;

namespace GrpcAudioStreaming.Server.Streamers.Grpc
{
    public interface IGrpcStreamer
    {
        event EventHandler<AudioSample> AudioSampleCreated;

        AudioFormat AudioFormat { get; }

        Task StartStreaming(AudioConsumer consumer);

        void StopStreaming();
    }
}