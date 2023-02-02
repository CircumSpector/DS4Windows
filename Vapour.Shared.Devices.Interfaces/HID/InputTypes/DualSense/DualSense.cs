﻿using System.Diagnostics.CodeAnalysis;

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
        public const byte SticksAndTriggersOffSet = 0;
        public const byte ButtonsOffset = 7;

        public const byte TouchDataOffset = 32;


        [Flags]
        public enum DualSenseButtons1 : byte
        {
            Square = 0x10,
            Cross = 0x20,
            Circle = 0x40,
            Triangle = 0x80
        }

        [Flags]
        public enum DualSenseButtons2 : byte
        {
            L1 = 0x01,
            R1 = 0x02,
            L2 = 0x04,
            R2 = 0x08,
            Create = 0x10,
            Options = 0x20,
            L3 = 0x40,
            R3 = 0x80
        }

        [Flags]
        public enum DualSenseButtons3 : byte
        {
            Home = 0x01,
            Pad = 0x02,
            Mute = 0x04,
            LFunction = 0x08,
            RFunction = 0x10,
            LPaddle = 0x20,
            RPaddle = 0x40,
            Unknown = 0x80
        }
    }

    public static class Out
    {
        public const int UsbReportLength = 64;
        public const int BtReportLength = 547;
        public const byte UsbReportId = 0x02;
        public const byte BtReportId = 0x31;
        public const byte BtCrcCalculateLength = 74;

        public const byte Config1Index = 0;
        public const byte Config2Index = 1;
        public const byte RumbleOffset = 2;
        public const byte LedOffset = 42;

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
    }
    
    public static class Feature
    {
        public const byte SerialId = 9;
    }
}