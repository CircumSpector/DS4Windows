using Microsoft.AspNetCore.SignalR;

using Vapour.Server.Controller;
using Vapour.Shared.Configuration.Profiles.Services;
using Vapour.Shared.Devices.HID;
using Vapour.Shared.Devices.HostedServices;
using Vapour.Shared.Devices.Services;

namespace Vapour.Server.Host.Controller;

/// <inheritdoc />
public sealed class ControllerMessageForwarder : IControllerMessageForwarder
{
    private readonly IHubContext<ControllerMessageHub, IControllerMessageClient> _hubContext;
    private readonly IProfilesService _profilesService;

    public ControllerMessageForwarder(ControllerManagerHost controllerManagerHost,
        IControllerConfigurationService controllerConfigurationService,
        IHubContext<ControllerMessageHub, IControllerMessageClient> hubContext,
        IProfilesService profilesService)
    {
        _hubContext = hubContext;
        _profilesService = profilesService;

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
            DeviceType = hidDevice.DeviceType,
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
            ProfileName = _profilesService.AvailableProfiles.Values.Single(i => i.Id == hidDevice.CurrentConfiguration.ProfileId).DisplayName
        };

        return message;
    }
}