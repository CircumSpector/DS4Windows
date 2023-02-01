using System.Diagnostics.CodeAnalysis;

namespace Vapour.Shared.Devices.HID.InputTypes.DualSense;

public static class DualSense
{
    public static class In
    {
        public const byte UsbReportId = 0x01;
        public const byte BtReportId = 0x31;
        public const byte UsbReportDataOffset = 1;
        public const byte BtReportDataOffset = 2;

        public const byte ReportIdIndex = 0;
        public const byte LeftThumbXIndex = 0;
        public const byte LeftThumbYIndex = 1;
        public const byte RightThumbXIndex = 2;
        public const byte RightThumbYIndex = 3;
        public const byte LeftTriggerIndex = 4;
        public const byte RightTriggerIndex = 5;
        public const byte Buttons1Index = 7;
        public const byte Buttons2Index = 8;
        public const byte Buttons3Index = 9;

        public const byte Touch1Index = 32;
        public const byte Touch2Index = 36;
        public const byte TouchDataLength = 9;
    }

    public static class Out
    {
        public const byte ReportDataLength = 47;
        public const byte UsbReportId = 0x02;
        public const byte BtReportId = 0x31;
        public const byte BtCommandCountMax = 0xF0;
        public const byte BtCrcCalculateLength = 74;
        public const byte BtCrcDataLength = 4;

        public const byte ReportIdIndex = 0;
        public const byte BtExtraConfigIndex = 1;
        public const byte UsbReportDataOffset = 1;
        public const byte BtReportDataOffset = 2;

        public const byte Config1Index = 0;
        public const byte Config2Index = 1;
        public const byte RumbleLeftIndex = 2;
        public const byte RumbleRightIndex = 3;
        public const byte PlayerLedBrightnessIndex = 42;
        public const byte PlayerLedIndex = 43;
        public const byte LedRIndex = 44;
        public const byte LedGIndex = 45;
        public const byte LedBIndex = 46;

        public static class Config1
        {
            public const byte EnableRumbleEmulation = 0x01;
            public const byte UseRumbleNotHaptics = 0x02;
            public const byte AllowRightTriggerFFB = 0x04;
            public const byte AllowLeftTriggerFFB = 0x08;
            public const byte AllowHeadphoneVolume = 0x10;
            public const byte AllowSpeakerVolume = 0x20;
            public const byte AllowMicVolume = 0x40;
            public const byte AllowAudioControl = 0x80;

            public const byte All = EnableRumbleEmulation | UseRumbleNotHaptics | AllowRightTriggerFFB |
                                    AllowLeftTriggerFFB | AllowHeadphoneVolume | AllowSpeakerVolume |
                                    AllowMicVolume | AllowAudioControl;
        }

        public static class Config2
        {
            public const byte AllowMuteLight = 0x01;
            public const byte AllowAudioMute = 0x02;
            public const byte AllowLedColor = 0x04;
            public const byte ResetLights = 0x08;
            public const byte AllowPlayerIndicators = 0x10;
            public const byte AllowHapticLowPassFilter = 0x20;
            public const byte AllowMotorPowerLevel = 0x40;

            public const byte AllowAudioControl2 = 0x80;

            public const byte All = AllowMuteLight | AllowAudioMute | AllowLedColor | AllowPlayerIndicators |
                                     AllowHapticLowPassFilter | AllowMotorPowerLevel | AllowAudioControl2 | ResetLights;
        }
        
        public static class BtExtraConfig
        {
            public const byte Unknown1 = 0x01;
            public const byte EnableHid = 0x02;
            public const byte Unknown2 = 0x04;
            public const byte Unknown3 = 0x08;
        }

        public static class PlayeLedBrightness
        {
            public const byte Bright = 0x01;
            public const byte Medium = 0x02;
            public const byte Dim = 0x04;
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        public static class PlayerLedLights
        {
            public const byte None = 0x00;
            public const byte Left = 0x01;
            public const byte MiddleLeft = 0x02;
            public const byte Middle = 0x04;
            public const byte MiddleRight = 0x08;
            public const byte Right = 0x10;
            public const byte Player1 = Middle;
            public const byte Player2 = MiddleLeft | MiddleRight;
            public const byte Player3 = Left | Middle | Right;
            public const byte Player4 = Left | MiddleLeft | MiddleRight | Right;
            public const byte All = Left | MiddleLeft | Middle | MiddleRight | Right;

            public const byte PlayerLightsFade = 0x20;
        }
    }
    
    public static class Feature
    {
        public const byte SerialId = 9;
    }
}