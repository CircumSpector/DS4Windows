using System.Net.Http;
using System.Windows;

using Microsoft.AspNetCore.SignalR.Client;

using Vapour.Server.System;
using Vapour.Shared.Common.Core;

namespace Vapour.Client.ServiceClients;
public sealed partial class SystemServiceClient : ISystemServiceClient
{
    private readonly IHttpClientFactory _clientFactory;
    private HubConnection _hubConnection;

    public SystemServiceClient(IHttpClientFactory clientFactory)
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
                HttpResponseMessage result = await client.GetAsync($"{Constants.HttpUrl}/api/system/ping", ct);
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

    public event Action<IsHostRunningChangedMessage> OnIsHostRunningChanged;

    public async void StartListening(CancellationToken ct = default)
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl($"{Constants.WebsocketUrl}/SystemMessages")
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<IsHostRunningChangedMessage>("IsHostRunningChanged", async message =>
        {
            await Application.Current.Dispatcher.BeginInvoke(() =>
            {
                OnIsHostRunningChanged?.Invoke(message);
            });
        });

        await _hubConnection.StartAsync(ct);
    }
}
