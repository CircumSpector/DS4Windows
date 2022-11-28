using Microsoft.Extensions.Logging;

using Vapour.Shared.Common.Types;
using Vapour.Shared.Devices.HID;
using Vapour.Shared.Devices.Output;

namespace Vapour.Shared.Devices.Services;
public class ControllerInputReportProcessorService : IControllerInputReportProcessorService
{
    private readonly Dictionary<string, IOutDevice> _outDevices;
    private readonly Dictionary<string, IControllerInputReportProcessor> _controllerInputReportProcessors;
    private readonly IOutputSlotManager _outputSlotManager;
    private readonly IServiceProvider _serviceProvider;

    public ControllerInputReportProcessorService(ILogger<ControllerInputReportProcessorService> logger,
        IServiceProvider serviceProvider,
        IOutputSlotManager outputSlotManager)
    {
        _serviceProvider = serviceProvider;
        _outputSlotManager = outputSlotManager;
        _controllerInputReportProcessors = new Dictionary<string, IControllerInputReportProcessor>();
        _outDevices = new Dictionary<string, IOutDevice>();
    }

    public ICompatibleHidDevice CreateReportProcessor(IHidDevice hidDevice, CompatibleDeviceIdentification deviceIdentification)
    {
        CompatibleHidDevice device = CompatibleHidDevice.CreateFrom(
            deviceIdentification.DeviceType,
            hidDevice,
            deviceIdentification.FeatureSet,
            _serviceProvider
        );

        if (hidDevice is HidDeviceOverWinUsb)
        {
            device.IsFiltered = true;
        }

        IOutDevice outDevice = _outputSlotManager.AllocateController(OutputDeviceType.Xbox360Controller);
        outDevice.Connect();
        if (!_outDevices.ContainsKey(hidDevice.InstanceId))
        {
            _outDevices.Add(hidDevice.InstanceId, outDevice);
        }

        var inputReportProcessor = new ControllerInputReportProcessor(device, _serviceProvider);
        _controllerInputReportProcessors.Add(hidDevice.InstanceId, inputReportProcessor);
        inputReportProcessor.InputReportAvailable += InputReportProcessor_InputReportAvailable;

        inputReportProcessor.StartInputReportReader();
        device.OnAfterStartListening();

        return device;
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
        outDevice.ConvertAndSendReport(report);
    }
}
