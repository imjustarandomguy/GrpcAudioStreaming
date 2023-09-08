namespace GrpcAudioStreaming.Proto.Codecs
{
    public interface ICodec
    {
        byte[] Encode(byte[] data, int offset, int length);

        byte[] Decode(byte[] data, int offset, int length);
    }
}
