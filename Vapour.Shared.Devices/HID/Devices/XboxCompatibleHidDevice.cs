using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Vapour.Shared.Devices.HID.Devices.Reports;

namespace Vapour.Shared.Devices.HID.Devices;
public class XboxCompatibleHidDevice : CompatibleHidDevice
{
    private const byte SerialFeatureId = 0x03;

    public XboxCompatibleHidDevice(InputDeviceType deviceType, IHidDevice source, CompatibleHidDeviceFeatureSet featureSet, IServiceProvider serviceProvider) : base(deviceType, source, featureSet, serviceProvider)
    {
        Serial = ReadSerial(SerialFeatureId);

        if (Serial is null)
        {
            throw new ArgumentException("Could not retrieve a valid serial number.");
        }

        Logger.LogInformation("Got serial {Serial} for {Device}", Serial, this);
    }

    public override CompatibleHidDeviceInputReport InputReport { get; } = new XboxCompatibleInputReport();
    public override void ProcessInputReport(ReadOnlySpan<byte> input)
    {
        InputReport.Parse(input);
    }
}
