using System.Diagnostics;
using System.Net.Http.Headers;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Polly;
using Polly.Contrib.WaitAndRetry;

using Vapour.Client.Core.DependencyInjection;
using Vapour.Shared.Common.Core;

namespace Vapour.Client.ServiceClients;

/// <summary>
///     Registers services required for HTTP/WS server-to-client communication.
/// </summary>
public sealed class ServiceClientsRegistrar : IServiceRegistrar
{
    public void ConfigureServices(IHostBuilder builder, HostBuilderContext context, IServiceCollection services)
    {
        services.AddHttpClient(Constants.ServerHostHttpClientName, client =>
        {
            // REST server base URI
            client.BaseAddress = new Uri(Constants.HttpUrl);
            // User-Agent
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(
                Assembly.GetExecutingAssembly().GetName().Name ?? nameof(ServiceClientsRegistrar),
                FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion)
            );
        });
            // retry policy in case of request failure
            //.AddTransientHttpErrorPolicy(pb =>
            //    pb.WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 5)));

        services.AddSingleton<IProfileServiceClient, ProfileServiceClient>();
        services.AddSingleton<IControllerServiceClient, ControllerServiceClient>();
    }
}