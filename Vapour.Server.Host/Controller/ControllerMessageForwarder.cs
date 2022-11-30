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
        controllerManagerHost.ControllerReady += ControllersEnumeratorService_ControllerReady;
        controllerManagerHost.ControllerRemoved += ControllersEnumeratorService_ControllerRemoved;
    }

    public async Task SendIsHostRunning(bool isRunning)
    {
        await _hubContext.Clients.All.IsHostRunningChanged(new IsHostRunningChangedMessage(isRunning));
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

    private async void ControllersEnumeratorService_ControllerReady(ICompatibleHidDevice hidDevice)
    {
        await _hubContext.Clients.All.ControllerConnected(MapControllerConnected(hidDevice));
    }

    private async void ControllersEnumeratorService_ControllerRemoved(ICompatibleHidDevice obj)
    {
        await _hubContext.Clients.All.ControllerDisconnected(new ControllerDisconnectedMessage
        {
            ControllerDisconnectedId = obj.SourceDevice.InstanceId
        });
    }
}