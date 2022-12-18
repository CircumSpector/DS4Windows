using System.Net.Http;
using System.Net.Http.Json;
using System.Web;

using Vapour.Server.Controller;
using Vapour.Server.Controller.Configuration;
using Vapour.Shared.Common.Core;
using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Client.ServiceClients;

public sealed partial class ControllerServiceClient
{
    public async Task<List<ControllerConnectedMessage>> GetControllerList()
    {
        using HttpClient client = _clientFactory.CreateClient(Constants.ServerHostHttpClientName);
        HttpResponseMessage result = await client.GetAsync("/api/controller/list");
        if (result.IsSuccessStatusCode)
        {
            return await result.Content.ReadFromJsonAsync<List<ControllerConnectedMessage>>();
        }

        throw new Exception($"Could not get the controller list {result.ReasonPhrase}");
    }

    public async Task<bool> IsHostRunning()
    {
        using HttpClient client = _clientFactory.CreateClient(Constants.ServerHostHttpClientName);
        HttpResponseMessage result = await client.GetAsync("/api/controller/host/status");

        if (!result.IsSuccessStatusCode)
        {
            throw new Exception($"Could not get the controller host status {result.ReasonPhrase}");
        }

        ControllerHostStatusResponse response = await result.Content.ReadFromJsonAsync<ControllerHostStatusResponse>();
        return response?.IsRunning ?? false;
    }

    public async Task StartHost()
    {
        using HttpClient client = _clientFactory.CreateClient(Constants.ServerHostHttpClientName);
        HttpResponseMessage result = await client.PostAsync("/api/controller/host/start", null);
        if (!result.IsSuccessStatusCode)
        {
            throw new Exception($"Could not start the host {result.ReasonPhrase}");
        }
    }

    public async Task StopHost()
    {
        using HttpClient client = _clientFactory.CreateClient(Constants.ServerHostHttpClientName);
        HttpResponseMessage result = await client.PostAsync("/api/controller/host/stop", null);
        if (!result.IsSuccessStatusCode)
        {
            throw new Exception($"Could not get the controller list {result.ReasonPhrase}");
        }
    }

    public async Task<ControllerFilterDriverStatusResponse> GetFilterDriverStatus()
    {
        using HttpClient client = _clientFactory.CreateClient(Constants.ServerHostHttpClientName);
        HttpResponseMessage result = await client.GetAsync("/api/controller/filterdriver/status");

        if (!result.IsSuccessStatusCode)
        {
            throw new Exception($"Could not get the controller filter driver status {result.ReasonPhrase}");
        }

        ControllerFilterDriverStatusResponse response =
            await result.Content.ReadFromJsonAsync<ControllerFilterDriverStatusResponse>();
        return response;
    }

    public async Task ControllerFilterSetDriverEnabled(bool isEnabled)
    {
        using HttpClient client = _clientFactory.CreateClient(Constants.ServerHostHttpClientName);

        HttpResponseMessage result =
            await client.PostAsync($"/api/controller/filterdriver/setenable/{isEnabled}", null);
        if (!result.IsSuccessStatusCode)
        {
            throw new Exception($"Could not set the filter driver enabled {result.ReasonPhrase}");
        }
    }

    public async Task FilterController(string instanceId)
    {
        using HttpClient client = _clientFactory.CreateClient(Constants.ServerHostHttpClientName);

        HttpResponseMessage result =
            await client.PostAsync($"/api/controller/filter/{HttpUtility.UrlEncode(instanceId)}",
                null);
        if (!result.IsSuccessStatusCode)
        {
            throw new Exception($"Could not filter the controller {result.ReasonPhrase}");
        }
    }

    public async Task UnfilterController(string instanceId)
    {
        using HttpClient client = _clientFactory.CreateClient(Constants.ServerHostHttpClientName);

        HttpResponseMessage result =
            await client.PostAsync($"/api/controller/unfilter/{HttpUtility.UrlEncode(instanceId)}",
                null);
        if (!result.IsSuccessStatusCode)
        {
            throw new Exception($"Could not unfilter the controller {result.ReasonPhrase}");
        }
    }

    public async Task SaveDefaultControllerConfiguration(string controllerKey,
        ControllerConfiguration controllerConfiguration)
    {
        using HttpClient client = _clientFactory.CreateClient(Constants.ServerHostHttpClientName);

        HttpResponseMessage result =
            await client.PutAsync($"/api/controller/configuration",
                JsonContent.Create(new ControllerSetConfigRequest
                {
                    ControllerKey = controllerKey, ControllerConfiguration = controllerConfiguration
                }));
        if (!result.IsSuccessStatusCode)
        {
            throw new Exception($"Could not save default controller config {result.ReasonPhrase}");
        }
    }

    public async Task<List<GameInfo>> GetGameSelectionList(string controllerKey, GameSource gameSource)
    {
        using HttpClient client = _clientFactory.CreateClient();

        HttpResponseMessage result = await client.PostAsync(
            new Uri($"{Constants.HttpUrl}/api/game/list", UriKind.Absolute),
            JsonContent.Create(new GameListRequest { ControllerKey = controllerKey, GameSource = gameSource }));

        if (result.IsSuccessStatusCode)
        {
            var gameList = await result.Content.ReadFromJsonAsync<List<GameInfo>>();
            return gameList;
        }

        throw new Exception($"Could not get game list {result.ReasonPhrase}");
    }

    public async Task SaveGameConfiguration(string controllerKey, GameInfo gameInfo,
        ControllerConfiguration controllerConfiguration)
    {
        using HttpClient client = _clientFactory.CreateClient();

        HttpResponseMessage result = await client.PostAsync(
            new Uri($"{Constants.HttpUrl}/api/controller/game/save", UriKind.Absolute),
            JsonContent.Create(new SaveControllerGameConfigurationRequest { ControllerKey = controllerKey, ControllerConfiguration = controllerConfiguration, GameInfo = gameInfo}));

        if (!result.IsSuccessStatusCode)
        {
            throw new Exception($"Could not save game configuration {result.ReasonPhrase}");
        }
    }

    public async Task<List<ControllerConfiguration>> GetGameControllerConfigurations(string controllerKey)
    {
        using HttpClient client = _clientFactory.CreateClient();

        HttpResponseMessage result = await client.GetAsync(
            new Uri($"{Constants.HttpUrl}/api/controller/configuration/games/{controllerKey}", UriKind.Absolute));

        if (result.IsSuccessStatusCode)
        {
            var gameConfigurationList = await result.Content.ReadFromJsonAsync<List<ControllerConfiguration>>();
            return gameConfigurationList;
        }

        throw new Exception($"Could not get game configuration list {result.ReasonPhrase}");
    }

    public async Task DeleteGameConfiguration(string controllerKey, string gameId)
    {
        using HttpClient client = _clientFactory.CreateClient();

        HttpResponseMessage result = await client.DeleteAsync(
            new Uri($"{Constants.HttpUrl}/api/controller/configuration/games/delete/{controllerKey}/{gameId}", UriKind.Absolute));

        if (!result.IsSuccessStatusCode)
        {
            throw new Exception($"Could not delete game configuration {result.ReasonPhrase}");
        }
    }
}