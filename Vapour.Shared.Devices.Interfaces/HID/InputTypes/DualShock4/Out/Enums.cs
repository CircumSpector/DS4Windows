namespace Vapour.Shared.Devices.HID.InputTypes.DualShock4.Out;

[Flags]
public enum Config1 : byte
{
    EnableRumbleUpdate = 0x01,
    EnableLedUpdate = 0x02,
    EnableLedBlink = 0x04,
    EnableExtWrite = 0x08,
    EnableVolumeLeftUpdate = 0x10,
    EnableVolumeRightUpdate = 0x20,
    EnableVolumeMicUpdate = 0x40,
    EnableVolumeSpeakerUpdate = 0x80,

    All = EnableRumbleUpdate | EnableLedUpdate | EnableLedBlink | EnableExtWrite |
                            EnableVolumeLeftUpdate | EnableVolumeRightUpdate |
                            EnableVolumeMicUpdate | EnableVolumeSpeakerUpdate
}

[Flags]
public enum BtExtraConfig2 : byte
{
    EnableSomething = 0x20,
    EnableAudio = 0x80
}

[Flags]
public enum BtExtraConfig : byte
{
    EnableCrc = 0x40,
    EnableHid = 0x80
}