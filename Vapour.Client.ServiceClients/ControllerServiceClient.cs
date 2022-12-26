using System.Net.Http;
using System.Windows;

using Microsoft.AspNetCore.SignalR.Client;

using Vapour.Server.Controller;
using Vapour.Server.Controller.Configuration;
using Vapour.Shared.Common.Core;

namespace Vapour.Client.ServiceClients;

public sealed partial class ControllerServiceClient : IControllerServiceClient
{
    private readonly IHttpClientFactory _clientFactory; 
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

    public event Action<ControllerConnectedMessage> OnControllerConnected;
    public event Action<ControllerDisconnectedMessage> OnControllerDisconnected;
    public event Action<ControllerConfigurationChangedMessage> OnControllerConfigurationChanged;
    public event Action<IsHostRunningChangedMessage> OnIsHostRunningChanged;

    public async void StartListening(CancellationToken ct = default)
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl($"{Constants.WebsocketUrl}/ControllerMessages")
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<ControllerConnectedMessage>("ControllerConnected", async message =>
        {
            await Application.Current.Dispatcher.BeginInvoke(() =>
            {
                OnControllerConnected?.Invoke(message);
            });
        });

        _hubConnection.On<ControllerDisconnectedMessage>("ControllerDisconnected", async message =>
        {
            await Application.Current.Dispatcher.BeginInvoke(() =>
            {
                OnControllerDisconnected?.Invoke(message);
            });
        });

        _hubConnection.On<IsHostRunningChangedMessage>("IsHostRunningChanged", async message =>
        {
            await Application.Current.Dispatcher.BeginInvoke(() =>
            {
                OnIsHostRunningChanged?.Invoke(message);
            });
        });

        _hubConnection.On<ControllerConfigurationChangedMessage>("ControllerConfigurationChanged", async message =>
        {
            await Application.Current.Dispatcher.BeginInvoke(() =>
            {
                OnControllerConfigurationChanged?.Invoke(message);
            });
        });

        await _hubConnection.StartAsync(ct);
    }
}