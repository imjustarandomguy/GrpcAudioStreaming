using NAudio.CoreAudioApi;
using System;

namespace GrpcAudioStreaming.Proto
{
    public class DefaultAudioDeviceChangeHandler()
    {
        private static readonly DefaultAudioDeviceChangeNotifier changeNotifier = new();

        public static void Init(Action<string> callback)
        {
            changeNotifier.DefaultDeviceChanged += (dataFlow, deviceRole, defaultDeviceId) =>
            {
                if (dataFlow != DataFlow.Render)
                {
                    return;
                }

                if (deviceRole != Role.Multimedia)
                {
                    return;
                }

                //string pattern = @"\{([0-9a-fA-F-]{36})\}";
                //var match = Regex.Match(defaultDeviceId, pattern);

                //if (!match.Success)
                //{
                //    return;
                //}

                //var deviceId = Guid.Parse(match.Groups[1].Value);
                var deviceId = defaultDeviceId;

                callback?.Invoke(deviceId);
            };
        }
    }
}
