using NAudio.Codecs;
using System;

namespace GrpcAudioStreaming.Proto.Codecs
{
    public class ALawCodec : ICodec
    {
        public byte[] Encode(byte[] data, int offset, int length)
        {
            byte[] encoded = new byte[length / 2];
            int outIndex = 0;

            for (int n = 0; n < length; n += 2)
            {
                encoded[outIndex++] = ALawEncoder.LinearToALawSample(BitConverter.ToInt16(data, offset + n));
            }

            return encoded;
        }

        public byte[] Decode(byte[] data, int offset, int length)
        {
            byte[] decoded = new byte[length * 2];
            int outIndex = 0;

            for (int n = 0; n < length; n++)
            {
                short decodedSample = ALawDecoder.ALawToLinearSample(data[n + offset]);
                decoded[outIndex++] = (byte)(decodedSample & 0xFF);
                decoded[outIndex++] = (byte)(decodedSample >> 8);
            }

            return decoded;
        }
    }
}
