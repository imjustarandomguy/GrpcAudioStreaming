using GrpcAudioStreaming.Client.Models;
using Microsoft.Extensions.Options;
using NAudio.Wave;
using System.Linq;

namespace GrpcAudioStreaming.Client.Device
{
    public class DeviceAccessor
    {
        public string DeviceId { get; set; }
        public string ModuleName { get; set; }

        public DeviceAccessor(IOptions<PlayerSettings> playerSettings)
        {
            var deviceName = playerSettings.Value.DeviceName;
            var device = DirectSoundOut.Devices.FirstOrDefault(device => device.Description == deviceName);

            //DeviceId = device?.Guid ?? Guid.Empty;
            DeviceId = device?.ModuleName;
        }

        public void SetDeviceById(string deviceId)
        {
            DeviceId = deviceId;
        }
    }
}
