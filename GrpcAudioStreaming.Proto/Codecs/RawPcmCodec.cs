using System;

namespace GrpcAudioStreaming.Proto.Codecs
{
    public class RawPcmCodec : ICodec
    {
        public byte[] Encode(byte[] data, int offset, int length)
        {
            var encoded = new byte[length];
            new ReadOnlySpan<byte>(data, offset, length).CopyTo(encoded);
            return encoded;
        }

        public byte[] Decode(byte[] data, int offset, int length)
        {
            var decoded = new byte[length];
            new ReadOnlySpan<byte>(data, offset, length).CopyTo(decoded);
            return decoded;
        }

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
    }
}
