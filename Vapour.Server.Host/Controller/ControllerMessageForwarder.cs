using Microsoft.AspNetCore.SignalR;

using Vapour.Server.Controller;
using Vapour.Server.Controller.Configuration;
using Vapour.Shared.Devices.HID;
using Vapour.Shared.Devices.Services;
using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Server.Host.Controller;

/// <inheritdoc />
public sealed class ControllerMessageForwarder : IControllerMessageForwarder
{
    private readonly IHubContext<ControllerMessageHub, IControllerMessageClient> _hubContext;

    public ControllerMessageForwarder(ICurrentControllerDataSource currentControllerDataSource,
        IControllerConfigurationService controllerConfigurationService,
        IHubContext<ControllerMessageHub, IControllerMessageClient> hubContext)
    {
        _hubContext = hubContext;

        currentControllerDataSource.ControllerAdded += async (o, device) =>
        {
            await _hubContext.Clients.All.ControllerConnected(MapControllerConnected(device));
        };

        currentControllerDataSource.ControllerRemoved += async (o, device) =>
        {
            await _hubContext.Clients.All.ControllerDisconnected(new ControllerDisconnectedMessage
            {
                ControllerDisconnectedId = device.SourceDevice.InstanceId
            });
        };

        controllerConfigurationService.OnActiveConfigurationChanged += async (o, e) =>
        {
            await _hubContext.Clients.All.ControllerConfigurationChanged(new ControllerConfigurationChangedMessage
            {
                ControllerKey = e.ControllerKey, ControllerConfiguration = e.ControllerConfiguration
            });
        };
    }

    public async Task SendIsHostRunning(bool isRunning)
    {
        await _hubContext.Clients.All.IsHostRunningChanged(new IsHostRunningChangedMessage { IsRunning = isRunning });
    }

    public ControllerConnectedMessage MapControllerConnected(ICompatibleHidDevice hidDevice)
    {
        ControllerConnectedMessage message = new()
        {
            Description = hidDevice.SourceDevice.Description,
            DeviceType = hidDevice.CurrentDeviceInfo.DeviceType,
            DisplayName = hidDevice.SourceDevice.DisplayName,
            InstanceId = hidDevice.SourceDevice.InstanceId,
            ManufacturerString = hidDevice.SourceDevice.ManufacturerString,
            ParentInstance = hidDevice.SourceDevice.ParentInstance,
            Path = hidDevice.SourceDevice.Path,
            ProductString = hidDevice.SourceDevice.ProductString,
            SerialNumberString = hidDevice.SerialString,
            Connection = hidDevice.Connection.GetValueOrDefault(),
            CurrentConfiguration = hidDevice.CurrentConfiguration,
            IsFiltered = hidDevice.IsFiltered,
            Vid = hidDevice.CurrentDeviceInfo.Vid,
            Pid = hidDevice.CurrentDeviceInfo.Pid
        };

        return message;
    }
}