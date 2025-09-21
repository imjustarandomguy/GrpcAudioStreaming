using GrpcAudioStreaming.Client.Models;
using Microsoft.Extensions.Options;
using NAudio.Wave;
using System;
using System.Linq;

namespace GrpcAudioStreaming.Client.Device
{
    public class DeviceAccessor
    {
        public Guid Device { get; set; }

        public DeviceAccessor(IOptions<PlayerSettings> playerSettings)
        {
            var deviceName = playerSettings.Value.DeviceName;
            var device = DirectSoundOut.Devices.FirstOrDefault(device => device.Description == deviceName);

            Device = device?.Guid ?? Guid.Empty;
        }

        public void SetDeviceById(Guid deviceId)
        {
            Device = deviceId;
        }
    }
}
