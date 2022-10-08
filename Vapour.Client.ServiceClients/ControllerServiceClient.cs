using System.Net.Http;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Windows;

using Newtonsoft.Json;

using Serilog;

using Vapour.Server;
using Vapour.Server.Controller;
using Vapour.Shared.Common.Core;

using Websocket.Client;

namespace Vapour.Client.ServiceClients;

public class ControllerServiceClient : IControllerServiceClient
{
    private readonly IHttpClientFactory _clientFactory;
    private Action<ControllerConnectedMessage> _connectedAction;
    private Action<ControllerDisconnectedMessage> _disconnectedAction;
    private Action<IsHostRunningChangedMessage> _hostRunningHandler;
    private WebsocketClient _websocketClient;

    public ControllerServiceClient(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
        Application.Current.Exit += Current_Exit;
    }

    public async Task WaitForService()
    {
        using HttpClient client = _clientFactory.CreateClient();

        while (true)
        {
            try
            {
                HttpResponseMessage result = await client.GetAsync($"{Constants.HttpUrl}/api/controller/ping");
                if (result.IsSuccessStatusCode)
                {
                    break;
                }
            }
            catch
            {
                // ignored
            }

            await Task.Delay(500);
        }
    }

    public async Task<List<ControllerConnectedMessage>> GetControllerList()
    {
        using HttpClient client = _clientFactory.CreateClient();
        HttpResponseMessage result = await client.GetAsync($"{Constants.HttpUrl}/api/controller/list");
        if (result.IsSuccessStatusCode)
        {
            return await result.Content.ReadFromJsonAsync<List<ControllerConnectedMessage>>();
        }

        throw new Exception($"Could not get the controller list {result.ReasonPhrase}");
    }

    public async void StartWebSocket(
        Action<ControllerConnectedMessage> connectedHandler,
        Action<ControllerDisconnectedMessage> disconnectedHandler,
        Action<IsHostRunningChangedMessage> hostRunningChangedHandler = null
    )
    {
        _connectedAction = connectedHandler;
        _disconnectedAction = disconnectedHandler;
        _hostRunningHandler = hostRunningChangedHandler;

        _websocketClient = new WebsocketClient(new Uri($"{Constants.WebsocketUrl}/api/controller/ws", UriKind.Absolute));

        _websocketClient.ReconnectTimeout = TimeSpan.FromSeconds(30);
        _websocketClient.ReconnectionHappened.Subscribe(info =>
        {
            Log.Information($"Reconnection happened, type: {info.Type}");
            _websocketClient.MessageReceived.Subscribe(ProcessControllerMessage);
        });

        _websocketClient.MessageReceived.Subscribe(ProcessControllerMessage);

        await _websocketClient.Start();
    }

    public async Task<bool> IsHostRunning()
    {
        using HttpClient client = _clientFactory.CreateClient();
        HttpResponseMessage result = await client.GetAsync($"{Constants.HttpUrl}/api/controller/host/status");
        
        if (!result.IsSuccessStatusCode)
        {
            throw new Exception($"Could not get the controller host status {result.ReasonPhrase}");
        }

        var response = await result.Content.ReadFromJsonAsync<ControllerHostStatusResponse>();
        return response?.IsRunning ?? false;
    }

    public async Task StartHost()
    {
        using HttpClient client = _clientFactory.CreateClient();
        HttpResponseMessage result = await client.PostAsync($"{Constants.HttpUrl}/api/controller/host/start", null);
        if (!result.IsSuccessStatusCode)
        {
            throw new Exception($"Could not start the host {result.ReasonPhrase}");
        }
    }

    public async Task StopHost()
    {
        using HttpClient client = _clientFactory.CreateClient();
        HttpResponseMessage result = await client.PostAsync($"{Constants.HttpUrl}/api/controller/host/stop", null);
        if (!result.IsSuccessStatusCode)
        {
            throw new Exception($"Could not get the controller list {result.ReasonPhrase}");
        }
    }

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
        MessageBase messageBase = JsonConvert.DeserializeObject<MessageBase>(msg.Text);
        if (messageBase.MessageName == ControllerConnectedMessage.Name)
        {
            await Application.Current.Dispatcher.BeginInvoke(() =>
            {
                ControllerConnectedMessage device = JsonConvert.DeserializeObject<ControllerConnectedMessage>(msg.Text);
                _connectedAction?.Invoke(device);
            });
        }
        else if (messageBase.MessageName == ControllerDisconnectedMessage.Name)
        {
            await Application.Current.Dispatcher.BeginInvoke(() =>
            {
                ControllerDisconnectedMessage device =
                    JsonConvert.DeserializeObject<ControllerDisconnectedMessage>(msg.Text);
                _disconnectedAction?.Invoke(device);
            });
        }
        else if (messageBase.MessageName == IsHostRunningChangedMessage.Name)
        {
            if (_hostRunningHandler != null)
            {
                IsHostRunningChangedMessage isHostRunning =
                    JsonConvert.DeserializeObject<IsHostRunningChangedMessage>(msg.Text);
                _hostRunningHandler(isHostRunning);
            }
        }
    }
}