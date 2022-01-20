using System;
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

        protected override void ProcessInputReport(byte[] report)
        {
            var state = new DualShock4CompatibleInputReport()
            {
                ReportId = report[0 + ReportStartOffset],

                LeftThumbX = report[1 + ReportStartOffset],
                LeftThumbY = report[2 + ReportStartOffset],
                RightThumbX = report[3 + ReportStartOffset],
                RightThumbY = report[4 + ReportStartOffset],
                LeftTrigger = report[5 + ReportStartOffset],
                RightTrigger = report[6 + ReportStartOffset],

                Triangle = (report[8 + ReportStartOffset] & (1 << 7)) != 0,
                Circle = (report[8 + ReportStartOffset] & (1 << 6)) != 0,
                Cross = (report[8 + ReportStartOffset] & (1 << 5)) != 0,
                Square = (report[8 + ReportStartOffset] & (1 << 4)) != 0,

                DPad = (DPadDirection)(report[8 + ReportStartOffset] & 0x0F),

                LeftThumb = (report[9 + ReportStartOffset] & (1 << 7)) != 0,
                RightThumb = (report[9 + ReportStartOffset] & (1 << 6)) != 0,
                Options = (report[9 + ReportStartOffset] & (1 << 5)) != 0,
                Share = (report[9 + ReportStartOffset] & (1 << 4)) != 0,
                RightTriggerButton = (report[9 + ReportStartOffset] & (1 << 3)) != 0,
                LeftTriggerButton = (report[9 + ReportStartOffset] & (1 << 2)) != 0,
                RightShoulder = (report[9 + ReportStartOffset] & (1 << 1)) != 0,
                LeftShoulder = (report[9 + ReportStartOffset] & (1 << 0)) != 0,

                PS = (report[10 + ReportStartOffset] & (1 << 0)) != 0,
                Mute = (report[10 + ReportStartOffset] & (1 << 2)) != 0,
            };
        }
    }
}