using NAudio.CoreAudioApi;
using System;
using System.Diagnostics;
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
                if (dataFlow == DataFlow.Capture)
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

                if (deviceId == Guid.Parse("be543746-e7d1-4fb8-9fe7-0f5190d7d70f"))
                {
                    deviceId = Guid.Parse("3b920af3-98d5-4672-8deb-19cd564358e1");
                }
                else if (deviceId == Guid.Parse("3b920af3-98d5-4672-8deb-19cd564358e1"))
                {
                    deviceId = Guid.Parse("be543746-e7d1-4fb8-9fe7-0f5190d7d70f");
                }

                if (!audioPlayer.Initialized)
                {
                    return;
                }

                if (audioPlayer.Device == deviceId)
                {
                    return;
                }

                deviceAccessor.SetDeviceById(deviceId);
                audioPlayer.SetDevice();
            };
        }
    }
}
