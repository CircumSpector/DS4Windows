﻿using System.Net.WebSockets;
using System.Text;

using Newtonsoft.Json;

using Vapour.Server.Controller;
using Vapour.Shared.Configuration.Profiles.Services;
using Vapour.Shared.Devices.HostedServices;
using Vapour.Shared.Devices.Interfaces.HID;

namespace Vapour.Server.Host.Controller;

public sealed class ControllerMessageForwarder : IControllerMessageForwarder
{
    private readonly IProfilesService profilesService;
    private readonly List<WebSocket> sockets = new();

    public ControllerMessageForwarder(ControllerManagerHost controllerManagerHost, IProfilesService profilesService)
    {
        this.profilesService = profilesService;
        controllerManagerHost.ControllerReady += ControllersEnumeratorService_ControllerReady;
        controllerManagerHost.ControllerRemoved += ControllersEnumeratorService_ControllerRemoved;
    }

    public async Task StartListening(WebSocket newSocket)
    {
        sockets.Add(newSocket);

        await Task.Run(async () =>
        {
            var buffer = new byte[1024 * 4];
            var result = await newSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!result.CloseStatus.HasValue)
                result = await newSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            sockets.Remove(newSocket);
            await newSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        });
    }

    public async Task SendIsHostRunning(bool isRunning)
    {
        foreach (var socket in sockets)
            if (socket is { State: WebSocketState.Open })
            {
                var data = new ArraySegment<byte>(
                    Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new IsHostRunningChangedMessage(isRunning))));
                await socket.SendAsync(data, WebSocketMessageType.Text, true, CancellationToken.None);
            }
    }

    public ControllerConnectedMessage MapControllerConnected(ICompatibleHidDevice hidDevice)
    {
        var message = new ControllerConnectedMessage
        {
            Description = hidDevice.Description,
            DeviceType = hidDevice.DeviceType,
            DisplayName = hidDevice.DisplayName,
            InstanceId = hidDevice.InstanceId,
            ManufacturerString = hidDevice.ManufacturerString,
            ParentInstance = hidDevice.ParentInstance,
            Path = hidDevice.Path,
            ProductString = hidDevice.ProductString,
            SerialNumberString = hidDevice.SerialNumberString,
            //Serial = hidDevice.Serial,
            Connection = hidDevice.Connection.GetValueOrDefault(),
            SelectedProfileId = profilesService.ActiveProfiles
                .Single(p => p.DeviceId != null && p.DeviceId.Equals(hidDevice.Serial)).Id
        };

        return message;
    }

    private async void ControllersEnumeratorService_ControllerReady(ICompatibleHidDevice hidDevice)
    {
        foreach (var socket in sockets)
            if (socket is { State: WebSocketState.Open })
            {
                var data = new ArraySegment<byte>(
                    Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(MapControllerConnected(hidDevice))));
                await socket.SendAsync(data, WebSocketMessageType.Text, true, CancellationToken.None);
            }
    }

    private async void ControllersEnumeratorService_ControllerRemoved(ICompatibleHidDevice obj)
    {
        foreach (var socket in sockets)
            if (socket is { State: WebSocketState.Open })
            {
                var message = new ControllerDisconnectedMessage
                {
                    ControllerDisconnectedId = obj.InstanceId
                };
                var data = new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message)));
                await socket.SendAsync(data, WebSocketMessageType.Text, true, CancellationToken.None);
            }
    }
}