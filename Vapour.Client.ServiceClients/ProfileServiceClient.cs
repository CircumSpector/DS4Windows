using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.SignalR.Client;

using Vapour.Shared.Common.Core;
using Vapour.Shared.Configuration.Profiles.Schema;

namespace Vapour.Client.ServiceClients;

public sealed partial class ProfileServiceClient : IProfileServiceClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    private HubConnection _hubConnection;

    public ProfileServiceClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public ObservableCollection<IProfile> ProfileList { get; private set; }

    public async Task Initialize()
    {
        using HttpClient client = _httpClientFactory.CreateClient();

        HttpResponseMessage result = await client.GetAsync(new Uri($"{Constants.HttpUrl}/api/profile/list"));

        if (result.IsSuccessStatusCode)
        {
            ProfileList =
                new ObservableCollection<IProfile>(
                    await result.Content.ReadFromJsonAsync<List<ProfileItem>>() ??
                    Enumerable.Empty<ProfileItem>()
                );
        }
        else
        {
            throw new Exception($"Could not get the profile list {result.ReasonPhrase}");
        }
    }

    public async void StartListening(CancellationToken ct = default)
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl($"{Constants.WebsocketUrl}/ProfileMessages")
            .WithAutomaticReconnect()
            .Build();

        await _hubConnection.StartAsync(ct);
    }
}