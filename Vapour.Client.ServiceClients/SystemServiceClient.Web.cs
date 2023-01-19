using System.Net.Http;
using System.Net.Http.Json;

using Vapour.Server.System;
using Vapour.Shared.Common.Core;

namespace Vapour.Client.ServiceClients;

    public sealed partial class SystemServiceClient
    {
        public async Task<bool> IsHostRunning()
        {
            using HttpClient client = _clientFactory.CreateClient(Constants.ServerHostHttpClientName);
            HttpResponseMessage result = await client.GetAsync("/api/system/host/status");

            if (!result.IsSuccessStatusCode)
            {
                throw new Exception($"Could not get the system host status {result.ReasonPhrase}");
            }

            SystemHostStatusResponse response = await result.Content.ReadFromJsonAsync<SystemHostStatusResponse>();
            return response?.IsRunning ?? false;
        }

        public async Task StartHost()
        {
            using HttpClient client = _clientFactory.CreateClient(Constants.ServerHostHttpClientName);
            HttpResponseMessage result = await client.PostAsync("/api/system/host/start", null);
            if (!result.IsSuccessStatusCode)
            {
                throw new Exception($"Could not start the host {result.ReasonPhrase}");
            }
        }

        public async Task StopHost()
        {
            using HttpClient client = _clientFactory.CreateClient(Constants.ServerHostHttpClientName);
            HttpResponseMessage result = await client.PostAsync("/api/system/host/stop", null);
            if (!result.IsSuccessStatusCode)
            {
                throw new Exception($"Could not get the system list {result.ReasonPhrase}");
            }
        }

        public async Task<SystemFilterDriverStatusResponse> GetFilterDriverStatus()
        {
            using HttpClient client = _clientFactory.CreateClient(Constants.ServerHostHttpClientName);
            HttpResponseMessage result = await client.GetAsync("/api/system/filterdriver/status");

            if (!result.IsSuccessStatusCode)
            {
                throw new Exception($"Could not get the system filter driver status {result.ReasonPhrase}");
            }

            SystemFilterDriverStatusResponse response =
                await result.Content.ReadFromJsonAsync<SystemFilterDriverStatusResponse>();
            return response;
        }

        public async Task SystemFilterSetDriverEnabled(bool isEnabled)
        {
            using HttpClient client = _clientFactory.CreateClient(Constants.ServerHostHttpClientName);

            HttpResponseMessage result =
                await client.PostAsync($"/api/system/filterdriver/setenable/{isEnabled}", null);
            if (!result.IsSuccessStatusCode)
            {
                throw new Exception($"Could not set the filter driver enabled {result.ReasonPhrase}");
            }
        }
}

