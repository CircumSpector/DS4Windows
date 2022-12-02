using System.Net.Http;
using System.Net.Http.Json;
using System.Web;

using Vapour.Server.Controller;
using Vapour.Shared.Common.Core;
using Vapour.Shared.Devices.Services;

namespace Vapour.Client.ServiceClients;

public sealed partial class ControllerServiceClient
{
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

    public async Task<bool> IsHostRunning()
    {
        using HttpClient client = _clientFactory.CreateClient();
        HttpResponseMessage result = await client.GetAsync($"{Constants.HttpUrl}/api/controller/host/status");

        if (!result.IsSuccessStatusCode)
        {
            throw new Exception($"Could not get the controller host status {result.ReasonPhrase}");
        }

        ControllerHostStatusResponse response = await result.Content.ReadFromJsonAsync<ControllerHostStatusResponse>();
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

    public async Task<ControllerFilterDriverStatusResponse> GetFilterDriverStatus()
    {
        using HttpClient client = _clientFactory.CreateClient();
        HttpResponseMessage result = await client.GetAsync($"{Constants.HttpUrl}/api/controller/filterdriver/status");

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
        using HttpClient client = _clientFactory.CreateClient();

        HttpResponseMessage result =
            await client.PostAsync($"{Constants.HttpUrl}/api/controller/filterdriver/setenable/{isEnabled}", null);
        if (!result.IsSuccessStatusCode)
        {
            throw new Exception($"Could not set the filter driver enabled {result.ReasonPhrase}");
        }
    }

    public async Task FilterController(string instanceId)
    {
        using HttpClient client = _clientFactory.CreateClient();

        HttpResponseMessage result =
            await client.PostAsync($"{Constants.HttpUrl}/api/controller/filter/{HttpUtility.UrlEncode(instanceId)}",
                null);
        if (!result.IsSuccessStatusCode)
        {
            throw new Exception($"Could not filter the controller {result.ReasonPhrase}");
        }
    }

    public async Task UnfilterController(string instanceId)
    {
        using HttpClient client = _clientFactory.CreateClient();

        HttpResponseMessage result =
            await client.PostAsync($"{Constants.HttpUrl}/api/controller/unfilter/{HttpUtility.UrlEncode(instanceId)}",
                null);
        if (!result.IsSuccessStatusCode)
        {
            throw new Exception($"Could not unfilter the controller {result.ReasonPhrase}");
        }
    }

    public async Task SaveDefaultControllerConfiguration(string controllerKey,
        ControllerConfiguration controllerConfiguration)
    {
        using HttpClient client = _clientFactory.CreateClient();

        HttpResponseMessage result =
            await client.PostAsync($"{Constants.HttpUrl}/api/controller/setconfig/",
                JsonContent.Create(new ControllerSetConfigRequest
                {
                    ControllerKey = controllerKey,
                    ControllerConfiguration = controllerConfiguration
                }));
        if (!result.IsSuccessStatusCode)
        {
            throw new Exception($"Could not save default controller config {result.ReasonPhrase}");
        }
    }
}