using Microsoft.Extensions.Options;
using NAudio.Wave;
using System;
using System.Linq;

namespace GrpcAudioStreaming.Client
{
    public class DeviceAccessor
    {
        public Guid Device { get; set; }

        public DeviceAccessor(IOptions<AppSettings> settings) 
        {
            var deviceName = settings.Value.DeviceName;
            var device = DirectSoundOut.Devices.FirstOrDefault(device => device.Description == deviceName);

            Device = device?.Guid ?? Guid.Empty;
        }

        public void SetDeviceById(Guid deviceId)
        {
            Device = deviceId;
        }

        public void SetDeviceByName(string deviceName)
        {
            var device = DirectSoundOut.Devices.FirstOrDefault(device => device.Description == deviceName);
            Device = device?.Guid ?? Guid.Empty;
        }
    }
}
