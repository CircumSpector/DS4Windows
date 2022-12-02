using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Nefarius.ViGEm.Client;
using Vapour.Shared.Common.Types;
using Vapour.Shared.Devices.HID;
using Vapour.Shared.Devices.Services;

namespace Vapour.Shared.Devices.Output;
public class OutputProcessor : IOutputProcessor
{
    private readonly IControllerInputReportProcessor _inputReportProcessor;
    private IOutDevice _controllerDevice;
    public OutputProcessor(ICompatibleHidDevice hidDevice, IControllerInputReportProcessor inputReportProcessor, IServiceProvider serviceProvider)
    {
        _inputReportProcessor = inputReportProcessor;
        
        Services = serviceProvider;
        Logger = Services.GetRequiredService<ILogger<OutputProcessor>>();
        HidDevice = hidDevice;
    }

    private IServiceProvider Services { get; }
    private ILogger<OutputProcessor> Logger { get; }
    public ICompatibleHidDevice HidDevice { get; }

    public void StartOutputProcessing()
    {
        if (HidDevice.CurrentConfiguration.OutputDeviceType != OutputDeviceType.None)
        {
            _inputReportProcessor.InputReportAvailable += _inputReportProcessor_InputReportAvailable;
            _controllerDevice = CreateControllerOutDevice();
            _controllerDevice.Connect();
        }
    }

    public void StopOutputProcessing()
    {
        if (_controllerDevice != null)
        {
            _inputReportProcessor.InputReportAvailable -= _inputReportProcessor_InputReportAvailable;
            _controllerDevice.Disconnect();
            _controllerDevice = null;
        }
    }

    private void _inputReportProcessor_InputReportAvailable(ICompatibleHidDevice arg1, CompatibleHidDeviceInputReport report)
    {
        if (HidDevice.CurrentConfiguration.OutputDeviceType != OutputDeviceType.None && _controllerDevice != null)
        {
            report = UpdateBasedOnConfiguration(HidDevice.CurrentConfiguration, report);
            _controllerDevice.ConvertAndSendReport(report);
        }
    }

    private CompatibleHidDeviceInputReport UpdateBasedOnConfiguration(ControllerConfiguration configuration, CompatibleHidDeviceInputReport report)
    {
        if (configuration.IsPassthru)
        {
            return report;
        }
        //TODO: fill in processing the configuration against the current report
        Guid profileId = configuration.ProfileId;
        return report;
    }

    private IOutDevice CreateControllerOutDevice()
    {
        IOutDevice outDevice;
        var client = Services.GetService<ViGEmClient>();
        if (HidDevice.CurrentConfiguration.OutputDeviceType == OutputDeviceType.Xbox360Controller)
        {
            outDevice = new Xbox360OutDevice(client);
        }
        else
        {
            outDevice = new DS4OutDevice(client);
        }
        return outDevice;
    }
}
