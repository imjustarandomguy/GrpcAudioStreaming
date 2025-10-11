using GrpcAudioStreaming.Proto.Codecs;
using GrpcAudioStreaming.Server.Settings;
using Microsoft.Extensions.Options;

namespace GrpcAudioStreaming.Server.Services.Recorders;

public static class RecorderEngineFactory
{
    public static ILoopbackAudioStreamerService GetOrDefault(string engine, ICodec codec, IOptions<AudioSettings> audioSettings)
    {
        return engine switch
        {
            "SoundFlow" => new SoundFlowLoopbackAudioStreamerService(codec, audioSettings),
            _ => new NAudioLoopbackAudioStreamerService(codec, audioSettings),
        };
    }
}
