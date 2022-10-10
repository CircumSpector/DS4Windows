using System.Net.WebSockets;
using System.Text;

using Newtonsoft.Json;

using Vapour.Server.Controller;
using Vapour.Shared.Configuration.Profiles.Services;
using Vapour.Shared.Devices.HostedServices;
using Vapour.Shared.Devices.Interfaces.HID;

namespace Vapour.Server.Host.Controller;

public sealed class ControllerMessageForwarder : IControllerMessageForwarder
{
    private readonly IProfilesService _profilesService;
    private readonly List<WebSocket> _sockets = new();

    public ControllerMessageForwarder(ControllerManagerHost controllerManagerHost, IProfilesService profilesService)
    {
        _profilesService = profilesService;
        controllerManagerHost.ControllerReady += ControllersEnumeratorService_ControllerReady;
        controllerManagerHost.ControllerRemoved += ControllersEnumeratorService_ControllerRemoved;
    }

    public async Task StartListening(WebSocket newSocket)
    {
        _sockets.Add(newSocket);

        await Task.Run(async () =>
        {
            byte[] buffer = new byte[1024 * 4];
            WebSocketReceiveResult result =
                await newSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!result.CloseStatus.HasValue)
            {
                result = await newSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            _sockets.Remove(newSocket);
            await newSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        });
    }

    public async Task SendIsHostRunning(bool isRunning)
    {
        foreach (WebSocket socket in _sockets)
        {
            if (socket is { State: WebSocketState.Open })
            {
                ArraySegment<byte> data = new ArraySegment<byte>(
                    Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new IsHostRunningChangedMessage(isRunning))));
                await socket.SendAsync(data, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
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
            SerialNumberString = hidDevice.SourceDevice.SerialNumberString,
            //Serial = hidDevice.Serial,
            Connection = hidDevice.Connection.GetValueOrDefault(),
            SelectedProfileId = _profilesService.ActiveProfiles
                .Single(p => p.DeviceId != null && p.DeviceId.Equals(hidDevice.Serial)).Id
        };

        return message;
    }

    private async void ControllersEnumeratorService_ControllerReady(ICompatibleHidDevice hidDevice)
    {
        foreach (WebSocket socket in _sockets)
        {
            if (socket is { State: WebSocketState.Open })
            {
                ArraySegment<byte> data = new ArraySegment<byte>(
                    Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(MapControllerConnected(hidDevice))));
                await socket.SendAsync(data, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }

    private async void ControllersEnumeratorService_ControllerRemoved(ICompatibleHidDevice obj)
    {
        foreach (WebSocket socket in _sockets)
        {
            if (socket is { State: WebSocketState.Open })
            {
                ControllerDisconnectedMessage message = new()
                {
                    ControllerDisconnectedId = obj.SourceDevice.InstanceId
                };
                ArraySegment<byte> data =
                    new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message)));
                await socket.SendAsync(data, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}