using Microsoft.Extensions.Logging;

using Nefarius.ViGEm.Client;

using Vapour.Shared.Common.Types;
using Vapour.Shared.Configuration.Profiles.Schema;
using Vapour.Shared.Configuration.Profiles.Services;
using Vapour.Shared.Devices.HID;
using Vapour.Shared.Devices.Output;

namespace Vapour.Shared.Devices.Services;
public class ControllerInputReportProcessorService : IControllerInputReportProcessorService
{
    private readonly Dictionary<string, IOutDevice> _outDevices;
    private readonly Dictionary<string, IControllerInputReportProcessor> _controllerInputReportProcessors;
    private readonly ViGEmClient _client;
    private readonly IServiceProvider _serviceProvider;

    public ControllerInputReportProcessorService(
        ILogger<ControllerInputReportProcessorService> logger,
        IServiceProvider serviceProvider,
        ViGEmClient client)
    {
        _serviceProvider = serviceProvider;
        _client = client;
        _controllerInputReportProcessors = new Dictionary<string, IControllerInputReportProcessor>();
        _outDevices = new Dictionary<string, IOutDevice>();
    }

    public void StartProcessing(ICompatibleHidDevice device, CompatibleDeviceIdentification deviceIdentification)
    {
        if (!_outDevices.ContainsKey(device.SourceDevice.InstanceId))
        {
            IOutDevice outDevice = GetOutputController();
            outDevice.Connect();
            _outDevices.Add(device.SourceDevice.InstanceId, outDevice);
        }

        var inputReportProcessor = new ControllerInputReportProcessor(device, _serviceProvider);
        _controllerInputReportProcessors.Add(device.SourceDevice.InstanceId, inputReportProcessor);
        inputReportProcessor.InputReportAvailable += InputReportProcessor_InputReportAvailable;

        inputReportProcessor.StartInputReportReader();
        device.OnAfterStartListening();
    }

    public void StopProcessing(ICompatibleHidDevice hidDevice)
    {
        var instanceId = hidDevice.SourceDevice.InstanceId;
        if (_controllerInputReportProcessors.ContainsKey(instanceId))
        {
            var inputReportProcessor = _controllerInputReportProcessors[instanceId];
            inputReportProcessor.StopInputReportReader();
            inputReportProcessor.InputReportAvailable -= InputReportProcessor_InputReportAvailable;

            if (_outDevices.ContainsKey(instanceId))
            {
                IOutDevice outDevice = _outDevices[instanceId];
                outDevice.Disconnect();
                _outDevices.Remove(instanceId);
            }
        }
    }

    private void InputReportProcessor_InputReportAvailable(ICompatibleHidDevice device, CompatibleHidDeviceInputReport report)
    {
        IOutDevice outDevice = _outDevices[device.SourceDevice.InstanceId];
        report = UpdateBasedOnProfile(device.CurrentProfile, report);
        outDevice.ConvertAndSendReport(report);
    }

    private IOutDevice GetOutputController()
    {
        //TODO:  change to look at profile for current controller and return necessary output device
        return new Xbox360OutDevice(_client);
    }

    private CompatibleHidDeviceInputReport UpdateBasedOnProfile(IProfile profile, CompatibleHidDeviceInputReport report)
    {
        //TODO: fill in processing the profile against the current report
        var profileId = profile.Id;
        return report;
    }
}
