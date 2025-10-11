using GrpcAudioStreaming.Client.Device;
using GrpcAudioStreaming.Client.Models;
using Microsoft.Extensions.Options;

namespace GrpcAudioStreaming.Client.Players;

public static class PlayerEngineFactory
{
    public static IAudioPlayer GetOrDefault(string engine, DeviceAccessor deviceAccessor, IOptions<PlayerSettings> playerSettings)
    {
        return engine switch
        {
            "SoundFlow" => new SoundFlowAudioPlayer(playerSettings),
            _ => new NAudioAudioPlayer(playerSettings, deviceAccessor),
        };
    }
}
