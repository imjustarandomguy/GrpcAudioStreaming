using System;

namespace GrpcAudioStreaming.Proto.Codecs
{
    public static class CodecFactory 
    {
        public static Codecs GetOrDefault(ICodec codec)
        {
            if (codec is MuLawCodec)
            {
                return Codecs.Mulaw;
            }

            if (codec is OpusCodec)
            {
                return Codecs.Opus;
            }

            if (codec is Opus2Codec)
            {
                return Codecs.Opus2;
            }

            return Codecs.Pcm;
        }

        public static ICodec GetOrDefault(string codec) 
        {
            Enum.TryParse(codec, out Codecs codecValue);
            return GetOrDefault(codecValue);
        }

        public static ICodec GetOrDefault(Codecs codec)
        {
            return codec switch
            {
                Codecs.Mulaw => new MuLawCodec(),
                Codecs.Opus => new OpusCodec(),
                Codecs.Opus2 => new Opus2Codec(),
                _ => new RawPcmCodec(),
            };
        }
    }
}
