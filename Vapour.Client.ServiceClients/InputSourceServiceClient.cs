using System.Net.Http;
using System.Windows;

using Microsoft.AspNetCore.SignalR.Client;

using Vapour.Server.InputSource;
using Vapour.Server.InputSource.Configuration;
using Vapour.Shared.Common.Core;

namespace Vapour.Client.ServiceClients;

public sealed partial class InputSourceServiceClient : IInputSourceServiceClient
{
    private readonly IHttpClientFactory _clientFactory; 
    private HubConnection _hubConnection;

    public InputSourceServiceClient(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }
    
    public event Action<InputSourceMessage> InInputSourceCreated;
    public event Action<InputSourceRemovedMessage> OnInputSourceRemoved;
    public event Action<InputSourceConfigurationChangedMessage> OnInputSourceConfigurationChanged;

    public async void StartListening(CancellationToken ct = default)
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl($"{Constants.WebsocketUrl}/InputSourceMessages")
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<InputSourceMessage>("InputSourceCreated", async message =>
        {
            await Application.Current.Dispatcher.BeginInvoke(() =>
            {
                InInputSourceCreated?.Invoke(message);
            });
        });

        _hubConnection.On<InputSourceRemovedMessage>("InputSourceRemoved", async message =>
        {
            await Application.Current.Dispatcher.BeginInvoke(() =>
            {
                OnInputSourceRemoved?.Invoke(message);
            });
        });
        
        _hubConnection.On<InputSourceConfigurationChangedMessage>("InputSourceConfigurationChanged", async message =>
        {
            await Application.Current.Dispatcher.BeginInvoke(() =>
            {
                OnInputSourceConfigurationChanged?.Invoke(message);
            });
        });

        await _hubConnection.StartAsync(ct);
    }
}