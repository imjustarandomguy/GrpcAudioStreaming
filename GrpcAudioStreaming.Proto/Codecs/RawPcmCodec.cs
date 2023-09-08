namespace GrpcAudioStreaming.Proto.Codecs
{
    public class RawPcmCodec : ICodec
    {
        public byte[] Encode(byte[] data, int offset, int length)
        {
            return data;
        }

        public byte[] Decode(byte[] data, int offset, int length)
        {
            return data;
        }
    }
}
