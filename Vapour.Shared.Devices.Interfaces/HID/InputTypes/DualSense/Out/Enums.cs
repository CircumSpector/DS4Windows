using System.Diagnostics.CodeAnalysis;

namespace Vapour.Shared.Devices.HID.InputTypes.DualSense.Out;

[Flags]
public enum Config1 : byte
{
    EnableRumbleEmulation = 0x01,
    UseRumbleNotHaptics = 0x02,
    AllowRightTriggerFFB = 0x04,
    AllowLeftTriggerFFB = 0x08,
    AllowHeadphoneVolume = 0x10,
    AllowSpeakerVolume = 0x20,
    AllowMicVolume = 0x40,
    AllowAudioControl = 0x80,

    All = EnableRumbleEmulation | UseRumbleNotHaptics | AllowRightTriggerFFB |
                            AllowLeftTriggerFFB | AllowHeadphoneVolume | AllowSpeakerVolume |
                            AllowMicVolume | AllowAudioControl
}

[Flags]
public enum Config2 : byte
{
    AllowMuteLight = 0x01,
    AllowAudioMute = 0x02,
    AllowLedColor = 0x04,
    ResetLights = 0x08,
    AllowPlayerIndicators = 0x10,
    AllowHapticLowPassFilter = 0x20,
    AllowMotorPowerLevel = 0x40,
    AllowAudioControl2 = 0x80,

    All = AllowMuteLight | AllowAudioMute | AllowLedColor | AllowPlayerIndicators |
                             AllowHapticLowPassFilter | AllowMotorPowerLevel | AllowAudioControl2 | ResetLights
}

[Flags]
public enum BtExtraConfig : byte
{
    Unknown1 = 0x01,
    EnableHid = 0x02,
    Unknown2 = 0x04,
    Unknown3 = 0x08
}

public enum PlayerLedBrightness : byte
{
    Bright = 0x01,
    Medium = 0x02,
    Dim = 0x04,
}

[Flags]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public enum PlayerLedLights : byte
{
    None = 0x00,
    Left = 0x01,
    MiddleLeft = 0x02,
    Middle = 0x04,
    MiddleRight = 0x08,
    Right = 0x10,
    Player1 = Middle,
    Player2 = MiddleLeft | MiddleRight,
    Player3 = Left | Middle | Right,
    Player4 = Left | MiddleLeft | MiddleRight | Right,
    All = Left | MiddleLeft | Middle | MiddleRight | Right,

    PlayerLightsFade = 0x20
}