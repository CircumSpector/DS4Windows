using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Web;
using System.Windows;

using Serilog;

using Vapour.Server;
using Vapour.Server.Controller;
using Vapour.Shared.Common.Core;
using Vapour.Shared.Configuration.Profiles.Schema;

using Websocket.Client;

namespace Vapour.Client.ServiceClients;

public sealed class ProfileServiceClient : IProfileServiceClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private Action<ProfileChangedMessage> _profileChangedHandler;
    private WebsocketClient _websocketClient;

    public ProfileServiceClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        Application.Current.Exit += Current_Exit;
    }

    public ObservableCollection<IProfile> ProfileList { get; private set; }

    public async Task Initialize()
    {
        using HttpClient client = _httpClientFactory.CreateClient();
        HttpResponseMessage result = await client.GetAsync(new Uri($"{Constants.HttpUrl}/api/profile/list"));
        if (result.IsSuccessStatusCode)
        {
            ProfileList =
                new ObservableCollection<IProfile>(await result.Content.ReadFromJsonAsync<List<ProfileItem>>() ??
                                                   Enumerable.Empty<ProfileItem>());
        }
        else
        {
            throw new Exception($"Could not get the profile list {result.ReasonPhrase}");
        }
    }

    public async Task<IProfile> CreateNewProfile()
    {
        using HttpClient client = _httpClientFactory.CreateClient();
        HttpResponseMessage result =
            await client.PostAsync(new Uri($"{Constants.HttpUrl}/api/profile/new", UriKind.Absolute), null);
        if (result.IsSuccessStatusCode)
        {
            return await result.Content.ReadFromJsonAsync<ProfileItem>();
        }

        throw new Exception($"Could not get new {result.ReasonPhrase}");
    }

    public async Task DeleteProfile(Guid id)
    {
        using HttpClient client = _httpClientFactory.CreateClient();
        HttpResponseMessage result =
            await client.DeleteAsync(new Uri($"{Constants.HttpUrl}/api/profile/delete/{id}", UriKind.Absolute));

        if (!result.IsSuccessStatusCode)
        {
            throw new Exception($"Could not delete profile {result.ReasonPhrase}");
        }

        ProfileList.Remove(ProfileList.Single(i => i.Id == id));
    }

    public async Task<IProfile> SaveProfile(IProfile profile)
    {
        using HttpClient client = _httpClientFactory.CreateClient();

        HttpResponseMessage result = await client.PostAsync(
            new Uri($"{Constants.HttpUrl}/api/profile/save", UriKind.Absolute),
            JsonContent.Create(profile));

        if (result.IsSuccessStatusCode)
        {
            ProfileItem savedProfile = await result.Content.ReadFromJsonAsync<ProfileItem>();

            IProfile existing = ProfileList.SingleOrDefault(i => i.Id == savedProfile.Id);
            if (existing != null)
            {
                int existingIndex = ProfileList.IndexOf(existing);
                ProfileList[existingIndex] = savedProfile;
            }
            else
            {
                ProfileList.Add(savedProfile);
            }

            return savedProfile;
        }

        throw new Exception($"Could not save profile {result.ReasonPhrase}");
    }

    public async Task SetProfile(string controllerKey, Guid profileId)
    {
        using HttpClient client = _httpClientFactory.CreateClient();
        HttpResponseMessage result =
            await client.GetAsync(
                new Uri($"{Constants.HttpUrl}/api/profile/set/{HttpUtility.UrlEncode(controllerKey)}/{HttpUtility.UrlEncode(profileId.ToString())}", UriKind.Absolute));

        if (!result.IsSuccessStatusCode)
        {
            throw new Exception($"Could not set profile {profileId} to controller {controllerKey} {result.ReasonPhrase}");
        }
    }

    public async void StartWebSocket(Action<ProfileChangedMessage> profileChangedHandler)
    {
        _profileChangedHandler = profileChangedHandler;
        
        _websocketClient = new WebsocketClient(new Uri($"{Constants.WebsocketUrl}/api/profile/ws", UriKind.Absolute));

        _websocketClient.ReconnectTimeout = TimeSpan.FromSeconds(30);
        _websocketClient.ReconnectionHappened.Subscribe(info =>
        {
            Log.Information($"Reconnection happened, type: {info.Type}");
            _websocketClient.MessageReceived.Subscribe(ProcessProfileMessage);
        });

        _websocketClient.MessageReceived.Subscribe(ProcessProfileMessage);

        await _websocketClient.Start();
    }

    private async void ProcessProfileMessage(ResponseMessage msg)
    {
        MessageBase messageBase = JsonSerializer.Deserialize<MessageBase>(msg.Text);
        if (messageBase.MessageName == ProfileChangedMessage.Name)
        {
            await Application.Current.Dispatcher.BeginInvoke(() =>
            {
                ProfileChangedMessage device = JsonSerializer.Deserialize<ProfileChangedMessage>(msg.Text);
                _profileChangedHandler?.Invoke(device);
            });
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
}