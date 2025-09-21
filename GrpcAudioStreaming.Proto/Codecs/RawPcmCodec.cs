using NAudio.Wave;
using System;

namespace GrpcAudioStreaming.Proto.Codecs
{
    public class RawPcmCodec : ICodec
    {
        public void Initialize(WaveFormat waveFormat, int frameSize = 480) { }

        public int Encode(ReadOnlySpan<byte> input, Span<byte> output)
        {
            input.CopyTo(output);
            return input.Length;
        }

        public int Decode(ReadOnlySpan<byte> input, Span<byte> output)
        {
            input.CopyTo(output);
            return input.Length;
        }

        public int GetMaxEncodedSize(int inputLength)
        {
            return inputLength;
        }

        public int GetMaxDecodedSize(int inputLength)
        {
            return inputLength;
        }
    }
}
