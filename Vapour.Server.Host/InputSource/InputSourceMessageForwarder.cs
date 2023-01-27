using Microsoft.AspNetCore.SignalR;

using Vapour.Server.InputSource;
using Vapour.Server.InputSource.Configuration;
using Vapour.Shared.Devices.Services;
using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Server.Host.InputSource;

/// <inheritdoc />
public sealed class InputSourceMessageForwarder : IInputSourceMessageForwarder
{
    private readonly IHubContext<InputSourceMessageHub, IInputSourceMessageClient> _hubContext;

    public InputSourceMessageForwarder(IInputSourceDataSource inputSourceDataSource,
        IInputSourceConfigurationService inputSourceConfigurationService,
        IHubContext<InputSourceMessageHub, IInputSourceMessageClient> hubContext)
    {
        _hubContext = hubContext;

        inputSourceDataSource.InputSourceCreated += async (inputSource) =>
        {
            await _hubContext.Clients.All.InputSourceCreated(MapInputSourceCreated(inputSource));
        };

        inputSourceDataSource.InputSourceRemoved += async (inputSource) =>
        {
            await _hubContext.Clients.All.InputSourceRemoved(new InputSourceRemovedMessage
            {
                InputSourceKey = inputSource.InputSourceKey
            });
        };

        inputSourceConfigurationService.OnActiveConfigurationChanged += async (o, e) =>
        {
            await _hubContext.Clients.All.InputSourceConfigurationChanged(new InputSourceConfigurationChangedMessage
            {
                InputSourceKey = e.InputSourceKey, InputSourceConfiguration = e.InputSourceConfiguration
            });
        };
    }

    public InputSourceMessage MapInputSourceCreated(IInputSource inputSource)
    {
        var controllers = inputSource.GetControllers().Select(c => new InputSourceController
        {
            Description = c.SourceDevice.Description,
            DeviceType = c.CurrentDeviceInfo.DeviceType,
            DisplayName = c.CurrentDeviceInfo.Name,
            InstanceId = c.SourceDevice.InstanceId,
            ManufacturerString = c.SourceDevice.ManufacturerString,
            ParentInstance = c.SourceDevice.ParentInstance,
            Path = c.SourceDevice.Path,
            ProductString = c.SourceDevice.ProductString,
            SerialNumberString = c.SerialString,
            Connection = c.Connection.GetValueOrDefault(),
            IsFiltered = c.IsFiltered,
            Vid = c.CurrentDeviceInfo.VendorId,
            Pid = c.CurrentDeviceInfo.ProductId,
            DeviceKey = c.DeviceKey
        }).ToList();

        InputSourceMessage message = new()
        {
            CurrentConfiguration = inputSource.Configuration,
            InputSourceKey = inputSource.InputSourceKey,
            Controllers = controllers
        };
        
        return message;
    }
}