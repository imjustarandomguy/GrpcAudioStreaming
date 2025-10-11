namespace GrpcAudioStreaming.Client.Models;

public class ClientSettings
{
    public const string SectionName = "Client";

    public string ServerUrl { get; set; }
    public bool AttemptAutomaticReconnect { get; set; }
    public int AutomaticReconnectDelay { get; set; }
}