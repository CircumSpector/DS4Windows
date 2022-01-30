using System;
using System.Runtime.InteropServices;
using DS4Windows.Shared.Devices.HID.Devices.Reports;
using Microsoft.Extensions.Logging;

namespace DS4Windows.Shared.Devices.HID.Devices
{
    public class DualShock4CompatibleHidDevice : CompatibleHidDevice
    {
        private const byte SerialFeatureId = 18;

        protected readonly int ReportStartOffset;

        public DualShock4CompatibleHidDevice(InputDeviceType deviceType, HidDevice source,
            CompatibleHidDeviceFeatureSet featureSet, IServiceProvider serviceProvider) : base(deviceType, source,
            featureSet, serviceProvider)
        {
            Serial = ReadSerial(SerialFeatureId);

            if (Serial is null)
                throw new ArgumentException("Could not retrieve a valid serial number.");

            Logger.LogInformation("Got serial {Serial} for {Device}", Serial, this);

            var inputReportSize = Capabilities.InputReportByteLength;

            InputReportArray = new byte[inputReportSize];
            InputReportBuffer = Marshal.AllocHGlobal(inputReportSize);

            if (Connection is ConnectionType.Usb or ConnectionType.SonyWirelessAdapter)
            {
                ReportStartOffset = 0;
                //
                // TODO: finish me
                // 
            }
            else
            {
                ReportStartOffset = 1;
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