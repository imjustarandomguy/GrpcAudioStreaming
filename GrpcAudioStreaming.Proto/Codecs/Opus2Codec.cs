using NAudio.Wave;
using OpusSharp.Core;
using OpusSharp.Core.Extensions;
using System;
using System.Runtime.InteropServices;

namespace GrpcAudioStreaming.Proto.Codecs
{
    public class Opus2Codec : ICodec
    {
        private OpusEncoder _encoder;
        private OpusDecoder _decoder;
        private WaveFormat _waveFormat = new(48000, 16, 2);
        private int _frameSize = 480;

        public void Initialize(WaveFormat waveFormat, int frameSize = 480)
        {
            _waveFormat = waveFormat;
            _frameSize = frameSize;

            _encoder = new OpusEncoder(_waveFormat.SampleRate, _waveFormat.Channels, OpusPredefinedValues.OPUS_APPLICATION_RESTRICTED_LOWDELAY);
            _decoder = new OpusDecoder(_waveFormat.SampleRate, _waveFormat.Channels);

            Console.WriteLine("Complexity: " + _encoder.GetComplexity());
        }

        public int Encode(Span<byte> input, Span<byte> output)
        {
            var pcmSamples = MemoryMarshal.Cast<byte, short>(input);
            int encodedBytes = _encoder.Encode(pcmSamples, _frameSize, output, output.Length);

            return encodedBytes;
        }

        public int Decode(ReadOnlySpan<byte> input, Span<byte> output)
        {
            var pcmSamples = MemoryMarshal.Cast<byte, short>(output);
            var decodedSamples = _decoder.Decode(input.ToArray(), input.Length, pcmSamples, _frameSize, false);
            var decodedBytes = decodedSamples * _waveFormat.Channels * sizeof(short);

            return decodedBytes;
        }

        public int GetMaxEncodedSize(int inputLength)
        {
            return 4096;
        }

        public int GetMaxDecodedSize(int inputLength)
        {
            return 20480;
        }
    }
}
