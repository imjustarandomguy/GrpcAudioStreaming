namespace GrpcAudioStreaming.Client.Models
{
    public enum ClientState
    {
        None,
        Errored,
        Connected,
        Connecting,
        GracefullyDisconnected,
    }
}