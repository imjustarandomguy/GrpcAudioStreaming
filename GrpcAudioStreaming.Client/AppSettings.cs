﻿namespace GrpcAudioStreaming.Client
{
    public class AppSettings
    {
        public string ServerUrl { get; set; }

        public int PlayerDesiredLatency { get; set; }

        public int BufferDuration { get; set; }

        public bool DiscardOnBufferOverflow { get; set; }

        public bool AttemptAutomaticReconnect { get; set; }

        public int AutomaticReconnectDelay { get; set; }
    }
}