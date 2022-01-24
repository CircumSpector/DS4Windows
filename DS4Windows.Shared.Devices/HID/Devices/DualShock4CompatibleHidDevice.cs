using System;
using System.Runtime.InteropServices;
using DS4Windows.Shared.Devices.HID.Devices.Reports;
using Microsoft.Extensions.Logging;

namespace DS4Windows.Shared.Devices.HID.Devices
{
    public class DualShock4CompatibleHidDevice : CompatibleHidDevice
    {
        private const byte SerialFeatureId = 18;
        private const int UsbInputReportSize = 64;
        private const int BthInputReportSize = 547;

        protected readonly int ReportStartOffset;

        public DualShock4CompatibleHidDevice(InputDeviceType deviceType, HidDevice source,
            CompatibleHidDeviceFeatureSet featureSet, IServiceProvider serviceProvider) : base(deviceType, source,
            featureSet, serviceProvider)
        {
            Serial = Connection == ConnectionType.SonyWirelessAdapter
                ? GenerateFakeHwSerial()
                : ReadSerial(SerialFeatureId);

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

        protected override CompatibleHidDeviceInputReport InputReport { get; } = new DualShock4CompatibleInputReport();

        protected override void ProcessInputReport(byte[] inputReport)
        {
            InputReport.ParseFrom(inputReport, ReportStartOffset);
        }
    }
}