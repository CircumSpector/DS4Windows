using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using DS4Windows.Shared.Core.HID.Devices.Reports;
using Microsoft.Extensions.Logging;

namespace DS4Windows.Shared.Core.HID.Devices
{
    public class DualSenseCompatibleHidDevice : CompatibleHidDevice
    {
        private const byte SerialFeatureId = 9;
        private const int UsbInputReportSize = 64;
        private const int BthInputReportSize = 547;

        protected readonly int ReportStartOffset;

        public DualSenseCompatibleHidDevice(InputDeviceType deviceType, HidDevice source,
            CompatibleHidDeviceFeatureSet featureSet, IServiceProvider serviceProvider) : base(deviceType, source,
            featureSet, serviceProvider)
        {
            Serial = ReadSerial(SerialFeatureId);

            if (Serial is null)
                throw new ArgumentException("Could not retrieve a valid serial number.");

            Logger.LogInformation("Got serial {Serial} for {Device}", Serial, this);

            if (Connection is ConnectionType.Usb or ConnectionType.SonyWirelessAdapter)
            {
                ReportStartOffset = 0;
                InputReportArray = new byte[UsbInputReportSize];
                InputReportBuffer = Marshal.AllocHGlobal(InputReportArray.Length);

                //
                // TODO: finish me
                // 
            }
            else
            {
                ReportStartOffset = 1;
                InputReportArray = new byte[BthInputReportSize];
                InputReportBuffer = Marshal.AllocHGlobal(InputReportArray.Length);
            }

            StartInputReportReader();
        }

        protected override void ProcessInputReport(byte[] inputReport)
        {
            var touchX = ((inputReport[35 + ReportStartOffset] & 0xF) << 8) | inputReport[34 + ReportStartOffset];

            var state = new DualShock4CompatibleInputReport()
            {
                ReportId = inputReport[0 + ReportStartOffset],

                LeftThumbX = inputReport[1 + ReportStartOffset],
                LeftThumbY = inputReport[2 + ReportStartOffset],
                RightThumbX = inputReport[3 + ReportStartOffset],
                RightThumbY = inputReport[4 + ReportStartOffset],
                LeftTrigger = inputReport[5 + ReportStartOffset],
                RightTrigger = inputReport[6 + ReportStartOffset],

                Triangle = (inputReport[8 + ReportStartOffset] & (1 << 7)) != 0,
                Circle = (inputReport[8 + ReportStartOffset] & (1 << 6)) != 0,
                Cross = (inputReport[8 + ReportStartOffset] & (1 << 5)) != 0,
                Square = (inputReport[8 + ReportStartOffset] & (1 << 4)) != 0,

                DPad = (DPadDirection)(inputReport[8 + ReportStartOffset] & 0x0F),

                LeftThumb = (inputReport[9 + ReportStartOffset] & (1 << 7)) != 0,
                RightThumb = (inputReport[9 + ReportStartOffset] & (1 << 6)) != 0,
                Options = (inputReport[9 + ReportStartOffset] & (1 << 5)) != 0,
                Share = (inputReport[9 + ReportStartOffset] & (1 << 4)) != 0,
                RightTriggerButton = (inputReport[9 + ReportStartOffset] & (1 << 3)) != 0,
                LeftTriggerButton = (inputReport[9 + ReportStartOffset] & (1 << 2)) != 0,
                RightShoulder = (inputReport[9 + ReportStartOffset] & (1 << 1)) != 0,
                LeftShoulder = (inputReport[9 + ReportStartOffset] & (1 << 0)) != 0,

                PS = (inputReport[10 + ReportStartOffset] & (1 << 0)) != 0,
                Mute = (inputReport[10 + ReportStartOffset] & (1 << 2)) != 0,

                TrackPadTouch1 = new TrackPadTouch()
                {
                    RawTrackingNum = inputReport[33 + ReportStartOffset],
                    Id = (byte)(inputReport[33 + ReportStartOffset] & 0x7f),
                    IsActive =  (inputReport[33 + ReportStartOffset] & 0x80) == 0,
                    X = (short)(((ushort)(inputReport[35 + ReportStartOffset] & 0x0f) << 8) |
                                inputReport[34 + ReportStartOffset]),
                    Y = (short)((inputReport[36 + ReportStartOffset] << 4) |
                                ((ushort)(inputReport[35 + ReportStartOffset] & 0xf0) >> 4))
                },
                TrackPadTouch2 = new TrackPadTouch()
                {
                    RawTrackingNum = inputReport[37 + ReportStartOffset],
                    Id = (byte)(inputReport[37 + ReportStartOffset] & 0x7f),
                    IsActive =  (inputReport[37 + ReportStartOffset] & 0x80) == 0,
                    X = (short)(((ushort)(inputReport[39 + ReportStartOffset] & 0x0f) << 8) |
                                inputReport[38 + ReportStartOffset]),
                    Y = (short)((inputReport[40 + ReportStartOffset] << 4) |
                                ((ushort)(inputReport[39 + ReportStartOffset] & 0xf0) >> 4))
                },
                TouchPacketCounter = inputReport[41 + ReportStartOffset],
                Touch1 =  inputReport[33 + ReportStartOffset] >> 7 == 0,
                TouchIdentifier1 = (byte)(inputReport[33 + ReportStartOffset] & 0x7f),
                Touch2 = inputReport[37 + ReportStartOffset] >> 7 == 0,
                TouchIdentifier2 = (byte)(inputReport[37 + ReportStartOffset] & 0x7f),
                TouchIsOnLeftSide = !(touchX >= 1920 * 2 / 5), // TODO: port const
                TouchIsOnRightSide = !(touchX < 1920 * 2 / 5) // TODO: port const
            };

            if (state.TouchOneFingerActive && state.TouchIsOnLeftSide)
            {
                Debug.WriteLine("Touch left active");
            }

            Debug.WriteLine("Processed");
        }
    }
}