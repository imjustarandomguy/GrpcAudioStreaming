using NAudio.Codecs;
using NAudio.Wave;
using System;

namespace GrpcAudioStreaming.Proto.Codecs
{
    public class MuLawCodec : ICodec
    {
        public void Initialize(WaveFormat waveFormat, int frameSize = 480) { }

        public int Encode(ReadOnlySpan<byte> input, Span<byte> output)
        {
            int outIndex = 0;
            for (int n = 0; n < input.Length; n += 2)
            {
                short sample = (short)(input[n] | (input[n + 1] << 8));
                output[outIndex++] = MuLawEncoder.LinearToMuLawSample(sample);
            }
            return outIndex;
        }

        public int Decode(ReadOnlySpan<byte> input, Span<byte> output)
        {
            int outIndex = 0;
            for (int n = 0; n < input.Length; n++)
            {
                short decodedSample = MuLawDecoder.MuLawToLinearSample(input[n]);
                output[outIndex++] = (byte)(decodedSample & 0xFF);
                output[outIndex++] = (byte)(decodedSample >> 8);
            }
            return outIndex;
        }

        public int GetMaxEncodedSize(int inputLength)
        {
            return inputLength / 2;
        }

        public int GetMaxDecodedSize(int inputLength)
        {
            return inputLength * 2;
        }
    }
}
