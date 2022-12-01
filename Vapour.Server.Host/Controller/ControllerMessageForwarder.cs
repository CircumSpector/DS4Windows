using Microsoft.AspNetCore.SignalR;

using Vapour.Server.Controller;
using Vapour.Shared.Devices.HID;
using Vapour.Shared.Devices.HostedServices;

namespace Vapour.Server.Host.Controller;

/// <inheritdoc />
public sealed class ControllerMessageForwarder : IControllerMessageForwarder
{
    private readonly IHubContext<ControllerMessageHub, IControllerMessageClient> _hubContext;

    public ControllerMessageForwarder(ControllerManagerHost controllerManagerHost,
        IHubContext<ControllerMessageHub, IControllerMessageClient> hubContext)
    {
        _hubContext = hubContext;

        controllerManagerHost.ControllerReady += async device =>
        {
            await _hubContext.Clients.All.ControllerConnected(MapControllerConnected(device));
        };
        
        controllerManagerHost.ControllerRemoved += async device =>
        {
            await _hubContext.Clients.All.ControllerDisconnected(new ControllerDisconnectedMessage
            {
                ControllerDisconnectedId = device.SourceDevice.InstanceId
            });
        };
    }

    public async Task SendIsHostRunning(bool isRunning)
    {
        await _hubContext.Clients.All.IsHostRunningChanged(new IsHostRunningChangedMessage
        {
            IsRunning = isRunning
        });
    }

    public ControllerConnectedMessage MapControllerConnected(ICompatibleHidDevice hidDevice)
    {
        ControllerConnectedMessage message = new()
        {
            Description = hidDevice.SourceDevice.Description,
            DeviceType = hidDevice.DeviceType,
            DisplayName = hidDevice.SourceDevice.DisplayName,
            InstanceId = hidDevice.SourceDevice.InstanceId,
            ManufacturerString = hidDevice.SourceDevice.ManufacturerString,
            ParentInstance = hidDevice.SourceDevice.ParentInstance,
            Path = hidDevice.SourceDevice.Path,
            ProductString = hidDevice.SourceDevice.ProductString,
            SerialNumberString = hidDevice.SerialString,
            Connection = hidDevice.Connection.GetValueOrDefault(),
            SelectedProfileId = hidDevice.CurrentProfile.Id,
            IsFiltered = hidDevice.IsFiltered
        };

        return message;
    }
}