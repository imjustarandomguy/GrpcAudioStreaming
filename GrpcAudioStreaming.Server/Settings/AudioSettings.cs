namespace GrpcAudioStreaming.Server.Settings
{
    public class AudioSettings
    {
        public const string RootPath = "AudioSettings";

        public int SampleRate { get; set; } = 48000;

        public int BitsPerSample { get; set; } = 16;

        public int Channels { get; set; } = 2;
    }
}
