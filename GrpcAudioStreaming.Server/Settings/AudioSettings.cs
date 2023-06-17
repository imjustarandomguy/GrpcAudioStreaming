namespace GrpcAudioStreaming.Server.Settings
{
    public class AudioSettings
    {
        public const string RootPath = "AudioSettings";

        public int SampleRate { get; set; } = 44100;

        public int BitsPerSample { get; set; } = 16;

        public int Channels { get; set; } = 2;
    }
}
