using System;

namespace GrpcAudioStreaming.Proto.Codecs
{
    public class RawPcmCodec : ICodec
    {
        public byte[] Encode(byte[] data, int offset, int length)
        {
            var encoded = new byte[length];

            Array.Copy(data, offset, encoded, 0, length);

            return encoded;
        }

        public byte[] Decode(byte[] data, int offset, int length)
        {
            var decoded = new byte[length];

            Array.Copy(data, offset, decoded, 0, length);

            return decoded;
        }
    }
}
