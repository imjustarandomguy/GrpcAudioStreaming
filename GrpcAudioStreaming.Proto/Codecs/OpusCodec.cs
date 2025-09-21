using System;
using Concentus.Structs;
using Concentus.Enums;

namespace GrpcAudioStreaming.Proto.Codecs
{
    public class OpusCodec : ICodec
    {
        private readonly OpusEncoder _encoder;
        private readonly OpusDecoder _decoder;
        private readonly int _sampleRate;
        private readonly int _channels;
        private readonly int _frameSize;

        public OpusCodec(int sampleRate = 16000, int channels = 1, int frameSize = 960)
        {
            _sampleRate = sampleRate;
            _channels = channels;
            _frameSize = frameSize;
            _encoder = new OpusEncoder(_sampleRate, _channels, OpusApplication.OPUS_APPLICATION_AUDIO);
            _decoder = new OpusDecoder(_sampleRate, _channels);
        }

        public byte[] Encode(byte[] data, int offset, int length)
        {
            short[] pcm = new short[length / 2];
            for (int i = 0; i < pcm.Length; i++)
            {
                pcm[i] = (short)(data[offset + i * 2] | (data[offset + i * 2 + 1] << 8));
            }
            byte[] encoded = new byte[4000]; // Opus frame size upper bound
            int encodedLength = _encoder.Encode(pcm, 0, _frameSize, encoded, 0, encoded.Length);
            Array.Resize(ref encoded, encodedLength);
            return encoded;
        }

        public byte[] Decode(byte[] data, int offset, int length)
        {
            short[] decodedPcm = new short[_frameSize * _channels];
            int decodedSamples = _decoder.Decode(data, offset, length, decodedPcm, 0, _frameSize, false);
            byte[] decoded = new byte[decodedSamples * 2];
            for (int i = 0; i < decodedSamples; i++)
            {
                decoded[i * 2] = (byte)(decodedPcm[i] & 0xFF);
                decoded[i * 2 + 1] = (byte)(decodedPcm[i] >> 8);
            }
            return decoded;
        }

        public int Encode(ReadOnlySpan<byte> input, Span<byte> output)
        {
            int samples = input.Length / 2;
            short[] pcm = new short[samples];
            for (int i = 0; i < samples; i++)
            {
                pcm[i] = (short)(input[i * 2] | (input[i * 2 + 1] << 8));
            }
            byte[] temp = new byte[output.Length];
            int encodedLength = _encoder.Encode(pcm, 0, _frameSize, temp, 0, temp.Length);
            temp.AsSpan(0, encodedLength).CopyTo(output);
            return encodedLength;
        }

        public int Decode(ReadOnlySpan<byte> input, Span<byte> output)
        {
            short[] decodedPcm = new short[_frameSize * _channels];
            int decodedSamples = _decoder.Decode(input.ToArray(), 0, input.Length, decodedPcm, 0, _frameSize, false);
            for (int i = 0; i < decodedSamples; i++)
            {
                output[i * 2] = (byte)(decodedPcm[i] & 0xFF);
                output[i * 2 + 1] = (byte)(decodedPcm[i] >> 8);
            }
            return decodedSamples * 2;
        }

        public int GetMaxEncodedSize(int inputLength)
        {
            return 4000;
        }
    }
}
