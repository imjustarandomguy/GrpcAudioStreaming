using NAudio.Wave;
using System;

namespace GrpcAudioStreaming.Proto.Codecs
{
    public interface ICodec
    {
        void Initialize(WaveFormat waveFormat, int frameSize = 480);

        int Encode(Span<byte> input, Span<byte> output);

        int Decode(ReadOnlySpan<byte> input, Span<byte> output);

        int GetMaxEncodedSize(int inputLength);

        int GetMaxDecodedSize(int inputLength);
    }
}
