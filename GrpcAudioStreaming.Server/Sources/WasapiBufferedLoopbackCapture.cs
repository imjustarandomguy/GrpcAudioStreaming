using NAudio.CoreAudioApi;

namespace GrpcAudioStreaming.Server.Sources
{
    public class WasapiBufferedLoopbackCapture : WasapiCapture
    {
        //
        // Summary:
        //     Initialises a new instance of the WASAPI capture class
        //
        // Parameters:
        //   audioBufferMillisecondsLength:
        //     Length of the audio buffer in milliseconds. A lower value means lower latency
        //     but increased CPU usage.
        public WasapiBufferedLoopbackCapture(int audioBufferMillisecondsLength = 100)
            : this(GetDefaultLoopbackCaptureDevice(), audioBufferMillisecondsLength: audioBufferMillisecondsLength)
        {
        }

        //
        // Summary:
        //     Initialises a new instance of the WASAPI capture class
        //
        // Parameters:
        //   useEventSync:
        //     true if sync is done with event. false use sleep.
        //
        //   audioBufferMillisecondsLength:
        //     Length of the audio buffer in milliseconds. A lower value means lower latency
        //     but increased CPU usage.
        public WasapiBufferedLoopbackCapture(bool useEventSync, int audioBufferMillisecondsLength = 100)
            : this(GetDefaultLoopbackCaptureDevice(), useEventSync: useEventSync, audioBufferMillisecondsLength: audioBufferMillisecondsLength)
        {
        }

        //
        // Summary:
        //     Initialises a new instance of the WASAPI capture class
        //
        // Parameters:
        //   captureDevice:
        //     Capture device to use
        //
        //   useEventSync:
        //     true if sync is done with event. false use sleep.
        //
        //   audioBufferMillisecondsLength:
        //     Length of the audio buffer in milliseconds. A lower value means lower latency
        //     but increased CPU usage.
        public WasapiBufferedLoopbackCapture(MMDevice captureDevice, bool useEventSync = false, int audioBufferMillisecondsLength = 100)
            : base(captureDevice, useEventSync, audioBufferMillisecondsLength)
        {
        }

        //
        // Summary:
        //     Gets the default audio loopback capture device
        //
        // Returns:
        //     The default audio loopback capture device
        public static MMDevice GetDefaultLoopbackCaptureDevice()
        {
            return new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        }

        //
        // Summary:
        //     Specify loopback
        protected override AudioClientStreamFlags GetAudioClientStreamFlags()
        {
            return AudioClientStreamFlags.Loopback | base.GetAudioClientStreamFlags();
        }
    }
}
