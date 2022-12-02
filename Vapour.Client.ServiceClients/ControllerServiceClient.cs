using System.Net.Http;
using System.Windows;

using Microsoft.AspNetCore.SignalR.Client;

using Vapour.Server.Controller;
using Vapour.Shared.Common.Core;
using Vapour.Shared.Devices.Services;

namespace Vapour.Client.ServiceClients;

public sealed partial class ControllerServiceClient : IControllerServiceClient
{
    private readonly IHttpClientFactory _clientFactory;
    private Action<ControllerConnectedMessage> _connectedAction;
    private Action<ControllerDisconnectedMessage> _disconnectedAction;
    private Action<IsHostRunningChangedMessage> _hostRunningHandler;
    private Action<ControllerConfigurationChangedMessage> _controllerConfigurationChangedAction;
    private HubConnection _hubConnection;

    public ControllerServiceClient(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
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

    public async void StartListening(
        Action<ControllerConnectedMessage> connectedHandler,
        Action<ControllerDisconnectedMessage> disconnectedHandler,
        Action<ControllerConfigurationChangedMessage> controllerConfigurationChangedHandler,
        Action<IsHostRunningChangedMessage> hostRunningChangedHandler = null,
        CancellationToken ct = default
    )
    {
        _connectedAction = connectedHandler;
        _disconnectedAction = disconnectedHandler;
        _controllerConfigurationChangedAction = controllerConfigurationChangedHandler;
        _hostRunningHandler = hostRunningChangedHandler;

        _hubConnection = new HubConnectionBuilder()
            .WithUrl($"{Constants.WebsocketUrl}/ControllerMessages")
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<ControllerConnectedMessage>("ControllerConnected", async message =>
        {
            await Application.Current.Dispatcher.BeginInvoke(() =>
            {
                _connectedAction?.Invoke(message);
            });
        });

        _hubConnection.On<ControllerDisconnectedMessage>("ControllerDisconnected", async message =>
        {
            await Application.Current.Dispatcher.BeginInvoke(() =>
            {
                _disconnectedAction?.Invoke(message);
            });
        });

        _hubConnection.On<IsHostRunningChangedMessage>("IsHostRunningChanged", async message =>
        {
            await Application.Current.Dispatcher.BeginInvoke(() =>
            {
                _hostRunningHandler?.Invoke(message);
            });
        });

        _hubConnection.On<ControllerConfigurationChangedMessage>("ControllerConfigurationChanged", async message =>
        {
            await Application.Current.Dispatcher.BeginInvoke(() =>
            {
                _controllerConfigurationChangedAction?.Invoke(message);
            });
        });

        await _hubConnection.StartAsync(ct);
    }
}