using NAudio.CoreAudioApi;
using System;
using System.Text.RegularExpressions;

namespace GrpcAudioStreaming.Client
{
    public class DefaultAudioDeviceChangeHandler(DeviceAccessor deviceAccessor)
    {
        public void Init(NAudioAudioPlayer audioPlayer)
        {
            var changeNotifier = new DefaultAudioDeviceChangeNotifier();

            changeNotifier.DefaultDeviceChanged += (DataFlow dataFlow, Role deviceRole, string defaultDeviceId) =>
            {
                if (!audioPlayer.Initialized)
                {
                    return;
                }

                if (dataFlow != DataFlow.Render)
                {
                    return;
                }

                if (deviceRole != Role.Multimedia)
                {
                    return;
                }

                string pattern = @"\{([0-9a-fA-F-]{36})\}";
                var match = Regex.Match(defaultDeviceId, pattern);

                if (!match.Success)
                {
                    return;
                }

                var deviceId = Guid.Parse(match.Groups[1].Value);

                if (audioPlayer.Device == deviceId)
                {
                    return;
                }

                deviceAccessor.SetDeviceById(deviceId);
                audioPlayer.SetDevice(deviceId);
            };
        }
    }
}
