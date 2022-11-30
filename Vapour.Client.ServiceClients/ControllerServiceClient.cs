using System.Net.Http;
using System.Net.WebSockets;
using System.Text.Json;
using System.Windows;

using Serilog;

using Vapour.Server;
using Vapour.Server.Controller;
using Vapour.Shared.Common.Core;

using Websocket.Client;

namespace Vapour.Client.ServiceClients;

public sealed partial class ControllerServiceClient : IControllerServiceClient
{
    private readonly IHttpClientFactory _clientFactory;
    private Action<ControllerConnectedMessage> _connectedAction;
    private Action<ControllerDisconnectedMessage> _disconnectedAction;
    private Action<IsHostRunningChangedMessage> _hostRunningHandler;

    [Obsolete] private WebsocketClient _websocketClient;

    public ControllerServiceClient(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
        Application.Current.Exit += Current_Exit;
    }

    public async Task WaitForService(CancellationToken ct = default)
    {
        using HttpClient client = _clientFactory.CreateClient();

        while (!ct.IsCancellationRequested)
        {
            try
            {
                HttpResponseMessage result = await client.GetAsync($"{Constants.HttpUrl}/api/controller/ping", ct);
                if (result.IsSuccessStatusCode)
                {
                    break;
                }
            }
            catch
            {
                // ignored
            }

            try
            {
                await Task.Delay(500, ct);
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch { }
        }
    }

    [Obsolete]
    public async void StartWebSocket(
        Action<ControllerConnectedMessage> connectedHandler,
        Action<ControllerDisconnectedMessage> disconnectedHandler,
        Action<IsHostRunningChangedMessage> hostRunningChangedHandler = null
    )
    {
        _connectedAction = connectedHandler;
        _disconnectedAction = disconnectedHandler;
        _hostRunningHandler = hostRunningChangedHandler;

        _websocketClient =
            new WebsocketClient(new Uri($"{Constants.WebsocketUrl}/api/controller/ws", UriKind.Absolute));

        _websocketClient.ReconnectTimeout = TimeSpan.FromSeconds(30);
        _websocketClient.ReconnectionHappened.Subscribe(info =>
        {
            Log.Information($"Reconnection happened, type: {info.Type}");
            _websocketClient.MessageReceived.Subscribe(ProcessControllerMessage);
        });

        _websocketClient.MessageReceived.Subscribe(ProcessControllerMessage);

        await _websocketClient.Start();
    }

    [Obsolete]
    private async void Current_Exit(object sender, ExitEventArgs e)
    {
        if (_websocketClient != null)
        {
            await _websocketClient.Stop(WebSocketCloseStatus.NormalClosure, string.Empty);
            _websocketClient.Dispose();
        }
    }

    private async void ProcessControllerMessage(ResponseMessage msg)
    {
        MessageBase messageBase = JsonSerializer.Deserialize<MessageBase>(msg.Text);
        if (messageBase.MessageName == ControllerConnectedMessage.Name)
        {
            await Application.Current.Dispatcher.BeginInvoke(() =>
            {
                ControllerConnectedMessage device = JsonSerializer.Deserialize<ControllerConnectedMessage>(msg.Text);
                _connectedAction?.Invoke(device);
            });
        }
        else if (messageBase.MessageName == ControllerDisconnectedMessage.Name)
        {
            await Application.Current.Dispatcher.BeginInvoke(() =>
            {
                ControllerDisconnectedMessage device =
                    JsonSerializer.Deserialize<ControllerDisconnectedMessage>(msg.Text);
                _disconnectedAction?.Invoke(device);
            });
        }
        else if (messageBase.MessageName == IsHostRunningChangedMessage.Name)
        {
            if (_hostRunningHandler != null)
            {
                IsHostRunningChangedMessage isHostRunning =
                    JsonSerializer.Deserialize<IsHostRunningChangedMessage>(msg.Text);
                _hostRunningHandler(isHostRunning);
            }
        }
    }
}