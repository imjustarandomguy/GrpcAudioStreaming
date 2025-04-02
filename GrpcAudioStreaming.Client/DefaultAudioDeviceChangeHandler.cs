using NAudio.CoreAudioApi;
using System.Threading.Tasks;

namespace GrpcAudioStreaming.Client
{
    internal class DefaultAudioDeviceChangeHandler
    {
        public DefaultAudioDeviceChangeHandler(AudioPlayer audioPlayer)
        {
            DefaultAudioDeviceChangeNotifier changeNotifier = new();
            changeNotifier.DefaultDeviceChanged += async (DataFlow dataFlow, Role deviceRole, string defaultDeviceId) =>
            {
                if (!audioPlayer.Initialized) return;

                await Task.Delay(5000);

                await audioPlayer.Restart();
            };
        }
    }
}
