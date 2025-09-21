namespace GrpcAudioStreaming.Client.Models;

public class PlayerSettings
{
    public const string SectionName = "Player";

    public string Engine { get; set; }
    public bool DiscardOnBufferOverflow { get; set; }
    public int DesiredLatency { get; set; }
    public int BufferDuration { get; set; }
    public string DeviceName { get; set; }
    public int Volume { get; set; }
}