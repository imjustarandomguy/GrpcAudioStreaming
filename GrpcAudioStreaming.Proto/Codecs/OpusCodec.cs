using Concentus;
using Concentus.Enums;
using NAudio.Wave;
using System;
using System.Buffers;
using System.Runtime.InteropServices;

namespace GrpcAudioStreaming.Proto.Codecs
{
    public class OpusCodec : ICodec
    {
        // Use ArrayPool for buffer reuse
        private byte[] _pcmBuffer = Array.Empty<byte>();
        private int _pcmBufferCount = 0;
        private IOpusEncoder _encoder;
        private IOpusDecoder _decoder;
        private WaveFormat _waveFormat = new WaveFormat(48000, 16, 2);
        private int _frameSize = 480;

        public void Initialize(WaveFormat waveFormat, int frameSize = 480)
        {
            _waveFormat = waveFormat;
            _frameSize = frameSize;

            _encoder = OpusCodecFactory.CreateEncoder(_waveFormat.SampleRate, _waveFormat.Channels, OpusApplication.OPUS_APPLICATION_RESTRICTED_LOWDELAY);
            _decoder = OpusCodecFactory.CreateDecoder(_waveFormat.SampleRate, _waveFormat.Channels);

            // Allocate buffer for 1 second of audio max
            int maxPcmBytes = _waveFormat.SampleRate * _waveFormat.Channels * 2;
            _pcmBuffer = ArrayPool<byte>.Shared.Rent(maxPcmBytes);
            _pcmBufferCount = 0;
        }

        public int Encode(Span<byte> input, Span<byte> output)
        {
            // Append input to buffer
            input.CopyTo(_pcmBuffer.AsSpan(_pcmBufferCount));
            _pcmBufferCount += input.Length;

            int bytesWritten = 0;
            int frameBytes = _frameSize * _waveFormat.Channels * 2;

            while (_pcmBufferCount >= frameBytes)
            {
                // Use MemoryMarshal.Cast for conversion
                var frameSpan = _pcmBuffer.AsSpan(0, frameBytes);
                var pcmShorts = MemoryMarshal.Cast<byte, short>(frameSpan);

                int maxEncoded = output.Length - bytesWritten - 2;
                if (maxEncoded <= 0) break;

                int encodedLength = _encoder.Encode(pcmShorts, _frameSize, output.Slice(bytesWritten + 2, maxEncoded), maxEncoded);

                // Write 2-byte big-endian length
                output[bytesWritten] = (byte)(encodedLength >> 8);
                output[bytesWritten + 1] = (byte)(encodedLength & 0xFF);

                bytesWritten += 2 + encodedLength;

                // Shift buffer left
                Buffer.BlockCopy(_pcmBuffer, frameBytes, _pcmBuffer, 0, _pcmBufferCount - frameBytes);
                _pcmBufferCount -= frameBytes;
            }

            return bytesWritten;
        }

        public int Decode(ReadOnlySpan<byte> input, Span<byte> output)
        {
            int inputOffset = 0;
            int outputOffset = 0;
            Span<short> pcmShorts = stackalloc short[_frameSize * _waveFormat.Channels];

            while (inputOffset + 2 <= input.Length)
            {
                int frameLength = (input[inputOffset] << 8) | input[inputOffset + 1];
                inputOffset += 2;

                if (inputOffset + frameLength > input.Length) break;

                int decodedSamples = _decoder.Decode(input.Slice(inputOffset, frameLength), pcmShorts, _frameSize, false);

                int bytesToCopy = decodedSamples * _waveFormat.Channels * 2;

                // Use MemoryMarshal.Cast for conversion
                var pcmBytes = MemoryMarshal.AsBytes(pcmShorts.Slice(0, decodedSamples * _waveFormat.Channels));
                pcmBytes.CopyTo(output.Slice(outputOffset, bytesToCopy));
                outputOffset += bytesToCopy;

                inputOffset += frameLength;
            }

            return outputOffset;
        }

        public int GetMaxEncodedSize(int inputLength)
        {
            return 4000;
        }

        public int GetMaxDecodedSize(int inputLength)
        {
            return inputLength * 50;
        }
    }
}
