namespace Vapour.Shared.Devices.HID.InputTypes.SteamDeck;

[Flags]
public enum SteamDeckPacketType : byte
{
    PT_INPUT = 0x01,
    PT_HOTPLUG = 0x03,
    PT_IDLE = 0x04,
    PT_OFF = 0x9f,
    PT_AUDIO = 0xb6,
    PT_CLEAR_MAPPINGS = 0x81,
    PT_CONFIGURE = 0x87,
    PT_LED = 0x87,
    PT_CALIBRATE_JOYSTICK = 0xbf,
    PT_CALIBRATE_TRACKPAD = 0xa7,
    PT_SET_AUDIO_INDICES = 0xc1,
    PT_LIZARD_BUTTONS = 0x85,
    PT_LIZARD_MOUSE = 0x8e,
    PT_FEEDBACK = 0x8f,
    PT_RESET = 0x95,
    PT_GET_SERIAL = 0xAE,
}

[Flags]
public enum SteamDeckPacketLength : byte
{
    PL_LED = 0x03,
    PL_OFF = 0x04,
    PL_FEEDBACK = 0x07,
    PL_CONFIGURE = 0x15,
    PL_CONFIGURE_BT = 0x0f,
    PL_GET_SERIAL = 0x15,
}

[Flags]
public enum SteamDeckConfigType : byte
{
    CT_LED = 0x2d,
    CT_CONFIGURE = 0x32,
    CONFIGURE_BT = 0x18,
}

[Flags]
public enum SteamDeckButtons0 : UInt16
{
    BTN_L5 = 0b1000000000000000,
    BTN_OPTIONS = 0b0100000000000000,
    BTN_STEAM = 0b0010000000000000,
    BTN_MENU = 0b0001000000000000,
    BTN_DPAD_DOWN = 0b0000100000000000,
    BTN_DPAD_LEFT = 0b0000010000000000,
    BTN_DPAD_RIGHT = 0b0000001000000000,
    BTN_DPAD_UP = 0b0000000100000000,
    BTN_A = 0b0000000010000000,
    BTN_X = 0b0000000001000000,
    BTN_B = 0b0000000000100000,
    BTN_Y = 0b0000000000010000,
    BTN_L1 = 0b0000000000001000,
    BTN_R1 = 0b0000000000000100,
    BTN_L2 = 0b0000000000000010,
    BTN_R2 = 0b0000000000000001,
}

[Flags]
public enum SteamDeckButton2 : byte
{
    BTN_RSTICK_PRESS = 0b00000100,
}

[Flags]
public enum SteamDeckButton4 : byte
{
    BTN_LSTICK_TOUCH = 0b01000000,
    BTN_RSTICK_TOUCH = 0b10000000,
    BTN_R4 = 0b00000100,
    BTN_L4 = 0b00000010,
}

[Flags]
public enum SteamDeckButton5 : byte
{
    BTN_QUICK_ACCESS = 0b00000100,
}