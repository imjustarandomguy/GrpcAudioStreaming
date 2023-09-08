using NAudio.Codecs;
using System;

namespace GrpcAudioStreaming.Proto.Codecs
{
    public class MuLawCodec : ICodec
    {
        public byte[] Encode(byte[] data, int offset, int length)
        {
            var encoded = new byte[length / 2];
            int outIndex = 0;

            for (int n = 0; n < length; n += 2)
            {
                encoded[outIndex++] = MuLawEncoder.LinearToMuLawSample(BitConverter.ToInt16(data, offset + n));
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
    }
}
