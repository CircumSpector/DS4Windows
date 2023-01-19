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
        InputSourceMessage message = new()
        {
            CurrentConfiguration = inputSource.Configuration,
            InputSourceKey = inputSource.InputSourceKey,
            Controller1 = new()
            {
                Description = inputSource.Controller1.SourceDevice.Description,
                DeviceType = inputSource.Controller1.CurrentDeviceInfo.DeviceType,
                DisplayName = inputSource.Controller1.CurrentDeviceInfo.Name,
                InstanceId = inputSource.Controller1.SourceDevice.InstanceId,
                ManufacturerString = inputSource.Controller1.SourceDevice.ManufacturerString,
                ParentInstance = inputSource.Controller1.SourceDevice.ParentInstance,
                Path = inputSource.Controller1.SourceDevice.Path,
                ProductString = inputSource.Controller1.SourceDevice.ProductString,
                SerialNumberString = inputSource.Controller1.SerialString,
                Connection = inputSource.Controller1.Connection.GetValueOrDefault(),
                IsFiltered = inputSource.Controller1.IsFiltered,
                Vid = inputSource.Controller1.CurrentDeviceInfo.Vid,
                Pid = inputSource.Controller1.CurrentDeviceInfo.Pid,
            }
        };
        
        if (inputSource.Controller2 != null)
        {
            message.Controller2 = new()
            {
                Description = inputSource.Controller2.SourceDevice.Description,
                DeviceType = inputSource.Controller2.CurrentDeviceInfo.DeviceType,
                DisplayName = inputSource.Controller2.CurrentDeviceInfo.Name,
                InstanceId = inputSource.Controller2.SourceDevice.InstanceId,
                ManufacturerString = inputSource.Controller2.SourceDevice.ManufacturerString,
                ParentInstance = inputSource.Controller2.SourceDevice.ParentInstance,
                Path = inputSource.Controller2.SourceDevice.Path,
                ProductString = inputSource.Controller2.SourceDevice.ProductString,
                SerialNumberString = inputSource.Controller2.SerialString,
                Connection = inputSource.Controller2.Connection.GetValueOrDefault(),
                IsFiltered = inputSource.Controller2.IsFiltered,
                Vid = inputSource.Controller2.CurrentDeviceInfo.Vid,
                Pid = inputSource.Controller2.CurrentDeviceInfo.Pid,
            };
        }
        
        return message;
    }
}