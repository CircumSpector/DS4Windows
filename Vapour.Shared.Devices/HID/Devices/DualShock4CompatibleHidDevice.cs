using System.Net.NetworkInformation;

using Microsoft.Extensions.Logging;

using Vapour.Shared.Devices.HID.Devices.Reports;

namespace Vapour.Shared.Devices.HID.Devices;

public sealed class DualShock4CompatibleHidDevice : CompatibleHidDevice
{
    private static readonly PhysicalAddress BlankSerial = PhysicalAddress.Parse("00:00:00:00:00:00");

    private const byte SerialFeatureId = 18;

    private readonly int _reportStartOffset;

    public DualShock4CompatibleHidDevice(InputDeviceType deviceType, IHidDevice source,
        CompatibleHidDeviceFeatureSet featureSet, IServiceProvider serviceProvider) : base(deviceType, source,
        featureSet, serviceProvider)
    {
        Serial = ReadSerial(SerialFeatureId);

        if (Serial is null)
        {
            throw new ArgumentException("Could not retrieve a valid serial number.");
        }

        Logger.LogInformation("Got serial {Serial} for {Device}", Serial, this);

        if (Connection is ConnectionType.Usb or ConnectionType.SonyWirelessAdapter)
        {
            _reportStartOffset = 0;
        }
        //
        // TODO: finish me
        // 
        else
        {
            _reportStartOffset = 0; // TODO: this works, investigate why :D
        }
    }

    public override CompatibleHidDeviceInputReport InputReport { get; } = new DualShock4CompatibleInputReport();

    public override void OnAfterStartListening()
    {
        /*
         * TODO
         * migrate and implement properly, this is a workaround to get devices to work
         * that power off themselves or stop sending reports if they don't receive at
         * least one of these output report packets.
         */
        if (SourceDevice.Service == InputDeviceService.WinUsb)
        {
            byte[] initialOutBuffer =
            {
                0x05, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
            };

            SourceDevice.WriteOutputReportViaInterrupt(initialOutBuffer, 500);
        }
    }

    public override void ProcessInputReport(ReadOnlySpan<byte> input)
    {
        // invalid input report ID
        if (input[0] == 0x00)
        {
            return;
        }

        // device is Sony Wireless Adapter but controller is not connected
        if (Connection == ConnectionType.SonyWirelessAdapter)
        {
            if ((input[31] & 0x04) != 0)
            {
                return;
            }
            
            // controller connected, refresh serial
            if (Equals(Serial, BlankSerial))
            {
                Serial = ReadSerial(SerialFeatureId);
            }
        }

        InputReport.Parse(input.Slice(_reportStartOffset));
    }
}