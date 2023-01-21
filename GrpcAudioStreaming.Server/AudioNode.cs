using System;

namespace AudioSharer.Models
{
    public class AudioNode
    {
        public Guid Id { get; set; }

        public byte[] Data { get; set; }
    }

    public class AudioNodeViewModel
    {
        public Guid Id { get; set; }

        public string Data { get; set; }
    }
}