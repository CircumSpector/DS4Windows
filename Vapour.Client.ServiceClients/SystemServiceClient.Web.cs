using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;

using Microsoft.Extensions.Logging;

using Vapour.Server.System;
using Vapour.Shared.Common.Core;

namespace Vapour.Client.ServiceClients;

public sealed class SystemServiceClientException : Exception
{
    internal SystemServiceClientException(string message) : base(message) { }
}

/// <summary>
///     REST API Client for system actions.
/// </summary>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public sealed partial class SystemServiceClient
{
    /// <inheritdoc />
    public async Task<bool> IsHostRunning()
    {
        using HttpClient client = _clientFactory.CreateClient(Constants.ServerHostHttpClientName);

        HttpResponseMessage result = await client.GetAsync("/api/system/host/status");

        if (!result.IsSuccessStatusCode)
        {
            throw new SystemServiceClientException($"Could not get the system host status: {result.ReasonPhrase}");
        }

        SystemHostStatusResponse response = await result.Content.ReadFromJsonAsync<SystemHostStatusResponse>();

        return response?.IsRunning ?? false;
    }

    /// <inheritdoc />
    public async Task StartHost()
    {
        using HttpClient client = _clientFactory.CreateClient(Constants.ServerHostHttpClientName);

        HttpResponseMessage result = await client.PostAsync("/api/system/host/start", null);

        if (!result.IsSuccessStatusCode)
        {
            throw new SystemServiceClientException($"Could not start the host: {result.ReasonPhrase}");
        }
    }

    /// <inheritdoc />
    public async Task StopHost()
    {
        using HttpClient client = _clientFactory.CreateClient(Constants.ServerHostHttpClientName);

        HttpResponseMessage result = await client.PostAsync("/api/system/host/stop", null);

        if (!result.IsSuccessStatusCode)
        {
            throw new SystemServiceClientException($"Could not get the system list: {result.ReasonPhrase}");
        }
    }

    /// <inheritdoc />
    public async Task<SystemFilterDriverStatusResponse> GetFilterDriverStatus()
    {
        using HttpClient client = _clientFactory.CreateClient(Constants.ServerHostHttpClientName);

        HttpResponseMessage result = await client.GetAsync("/api/system/filterdriver/status");

        if (!result.IsSuccessStatusCode)
        {
            throw new SystemServiceClientException(
                $"Could not get the system filter driver status: {result.ReasonPhrase}");
        }

        SystemFilterDriverStatusResponse response =
            await result.Content.ReadFromJsonAsync<SystemFilterDriverStatusResponse>();
        return response;
    }

    /// <inheritdoc />
    public async Task SystemFilterSetDriverEnabled(bool isEnabled)
    {
        using HttpClient client = _clientFactory.CreateClient(Constants.ServerHostHttpClientName);

        string action = isEnabled ? "enable" : "disable";

        HttpResponseMessage result =
            await client.PostAsync($"/api/system/filterdriver/action/{action}", null);

        if (!result.IsSuccessStatusCode)
        {
            throw new SystemServiceClientException($"Could not set the filter driver enabled: {result.ReasonPhrase}");
        }
    }

    /// <inheritdoc />
    public async Task SystemFilterInstallDriver()
    {
        using HttpClient client = _clientFactory.CreateClient(Constants.ServerHostHttpClientName);

        HttpResponseMessage result =
            await client.PostAsync("/api/system/filterdriver/action/install", null);

        if (result.StatusCode == HttpStatusCode.Conflict)
        {
            _logger.LogWarning("Driver installation failed: {Reason}", await result.Content.ReadAsStringAsync());
        }
        else if (!result.IsSuccessStatusCode)
        {
            throw new SystemServiceClientException($"Filter driver installation failed: {result.ReasonPhrase}");
        }
    }

    /// <inheritdoc />
    public async Task SystemFilterUninstallDriver()
    {
        using HttpClient client = _clientFactory.CreateClient(Constants.ServerHostHttpClientName);

        HttpResponseMessage result =
            await client.PostAsync("/api/system/filterdriver/action/uninstall", null);

        if (result.StatusCode == HttpStatusCode.NotAcceptable)
        {
            _logger.LogWarning("Driver removal failed: {Reason}", await result.Content.ReadAsStringAsync());
        }
        else if (!result.IsSuccessStatusCode)
        {
            throw new SystemServiceClientException($"Filter driver removal failed: {result.ReasonPhrase}");
        }
    }
}
