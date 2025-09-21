using System;

namespace GrpcAudioStreaming.Proto.Codecs
{
    public interface ICodec
    {
        byte[] Encode(byte[] data, int offset, int length);

        byte[] Decode(byte[] data, int offset, int length);

        int Encode(ReadOnlySpan<byte> input, Span<byte> output);

        int Decode(ReadOnlySpan<byte> input, Span<byte> output);

        int GetMaxEncodedSize(int inputLength);
    }
}
