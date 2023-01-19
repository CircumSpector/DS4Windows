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

    public InputSourceCreatedMessage MapInputSourceCreated(IInputSource inputSource)
    {
        var hidDevice = inputSource.Controller1;
        InputSourceCreatedMessage message = new()
        {
            Description = hidDevice.SourceDevice.Description,
            DeviceType = hidDevice.CurrentDeviceInfo.DeviceType,
            DisplayName = hidDevice.CurrentDeviceInfo.Name,
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
            Pid = hidDevice.CurrentDeviceInfo.Pid,
            InputSourceKey = inputSource.InputSourceKey
        };

        return message;
    }
}