using NAudio.Codecs;
using System;

namespace GrpcAudioStreaming.Proto.Codecs
{
    public class MuLawCodec : ICodec
    {
        public byte[] Encode(byte[] data, int offset, int length)
        {
            var encoded = new byte[length / 2];
            var span = new ReadOnlySpan<byte>(data, offset, length);
            int outIndex = 0;

            for (int n = 0; n < span.Length; n += 2)
            {
                short sample = (short)(span[n] | (span[n + 1] << 8));
                encoded[outIndex++] = MuLawEncoder.LinearToMuLawSample(sample);
            }
            return encoded;
        }

        public byte[] Decode(byte[] data, int offset, int length)
        {
            var decoded = new byte[length * 2];
            int outIndex = 0;

            for (int n = 0; n < length; n++)
            {
                short decodedSample = MuLawDecoder.MuLawToLinearSample(data[n + offset]);
                decoded[outIndex++] = (byte)(decodedSample & 0xFF);
                decoded[outIndex++] = (byte)(decodedSample >> 8);
            }

            return decoded;
        }

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
    }
}
