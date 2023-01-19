using System.Net.Http;
using System.Net.Http.Json;

using Vapour.Server.InputSource;
using Vapour.Server.InputSource.Configuration;
using Vapour.Shared.Common.Core;
using Vapour.Shared.Devices.Services.Configuration;

namespace Vapour.Client.ServiceClients;

public sealed partial class InputSourceServiceClient
{
    public async Task<List<InputSourceCreatedMessage>> GetInputSourceList()
    {
        using HttpClient client = _clientFactory.CreateClient(Constants.ServerHostHttpClientName);
        HttpResponseMessage result = await client.GetAsync("/api/inputsource/list");
        if (result.IsSuccessStatusCode)
        {
            return await result.Content.ReadFromJsonAsync<List<InputSourceCreatedMessage>>();
        }

        throw new Exception($"Could not get the input source list {result.ReasonPhrase}");
    }

    public async Task SaveDefaultInputSourceConfiguration(string inputSourceKey,
        InputSourceConfiguration inputSourceConfiguration)
    {
        using HttpClient client = _clientFactory.CreateClient(Constants.ServerHostHttpClientName);

        HttpResponseMessage result =
            await client.PutAsync($"/api/inputsource/configuration",
                JsonContent.Create(new InputSourceSetConfigRequest
                {
                    InputSourceKey = inputSourceKey, InputSourceConfiguration = inputSourceConfiguration
                }));
        if (!result.IsSuccessStatusCode)
        {
            throw new Exception($"Could not save default input source config {result.ReasonPhrase}");
        }
    }

    public async Task<List<GameInfo>> GetGameSelectionList(string inputSourceKey, GameSource gameSource)
    {
        using HttpClient client = _clientFactory.CreateClient();

        HttpResponseMessage result = await client.PostAsync(
            new Uri($"{Constants.HttpUrl}/api/game/list", UriKind.Absolute),
            JsonContent.Create(new GameListRequest { InputSourceKey = inputSourceKey, GameSource = gameSource }));

        if (result.IsSuccessStatusCode)
        {
            var gameList = await result.Content.ReadFromJsonAsync<List<GameInfo>>();
            return gameList;
        }

        throw new Exception($"Could not get game list {result.ReasonPhrase}");
    }

    public async Task SaveGameConfiguration(string inputSourceKey, GameInfo gameInfo,
        InputSourceConfiguration inputSourceConfiguration)
    {
        using HttpClient client = _clientFactory.CreateClient();

        HttpResponseMessage result = await client.PostAsync(
            new Uri($"{Constants.HttpUrl}/api/inputsource/game/save", UriKind.Absolute),
            JsonContent.Create(new SaveInputSourceGameConfigurationRequest { InputSourceKey = inputSourceKey, InputSourceConfiguration = inputSourceConfiguration, GameInfo = gameInfo}));

        if (!result.IsSuccessStatusCode)
        {
            throw new Exception($"Could not save game configuration {result.ReasonPhrase}");
        }
    }

    public async Task<List<InputSourceConfiguration>> GetGameInputSourceConfigurations(string inputSourceKey)
    {
        using HttpClient client = _clientFactory.CreateClient();

        HttpResponseMessage result = await client.GetAsync(
            new Uri($"{Constants.HttpUrl}/api/inputsource/configuration/games/{inputSourceKey}", UriKind.Absolute));

        if (result.IsSuccessStatusCode)
        {
            var gameConfigurationList = await result.Content.ReadFromJsonAsync<List<InputSourceConfiguration>>();
            return gameConfigurationList;
        }

        throw new Exception($"Could not get game configuration list {result.ReasonPhrase}");
    }

    public async Task DeleteGameConfiguration(string inputSourceKey, string gameId)
    {
        using HttpClient client = _clientFactory.CreateClient();

        HttpResponseMessage result = await client.DeleteAsync(
            new Uri($"{Constants.HttpUrl}/api/inputsource/configuration/games/delete/{inputSourceKey}/{gameId}", UriKind.Absolute));

        if (!result.IsSuccessStatusCode)
        {
            throw new Exception($"Could not delete game configuration {result.ReasonPhrase}");
        }
    }
}