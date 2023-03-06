using System.Text.RegularExpressions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using Vapour.Client.Core.DependencyInjection;

namespace Vapour.Shared.Common.Tracing;

/// <summary>
///     Registers services required for telemetry and performance metrics.
/// </summary>
public partial class OpenTelemetryRegistrar : IServiceRegistrar
{
    /// <summary>
    ///     Only scan assemblies starting with this name.
    /// </summary>
    private const string AssemblyPrefix = "Vapour";

    public void ConfigureServices(IHostBuilder builder, HostBuilderContext context, IServiceCollection services)
    {
        // Get list of assemblies, register them all as potential tracing and metric sources
        string[] assemblyNames = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(assembly =>
            {
                string name = assembly.GetName().Name;
                return name != null && name.StartsWith(AssemblyPrefix);
            })
            .Select(assembly => CleanupRegex().Replace(assembly.GetName().Name!, string.Empty))
            .ToArray();

        if (bool.TryParse(context.Configuration.GetSection("OpenTelemetry:IsTracingEnabled").Value,
                out bool isTracingEnabled) &&
            isTracingEnabled)
        {
            // Add OpenTelemetry tracing
            services.AddOpenTelemetry()
                .ConfigureResource(rb => rb
                    .AddService(serviceName: AssemblyPrefix))
                .WithTracing(tb => tb
                    .AddSource(assemblyNames)
                    .AddJaegerExporter(options => { options.ExportProcessorType = ExportProcessorType.Simple; }));
        }

        if (bool.TryParse(context.Configuration.GetSection("OpenTelemetry:IsMetricsEnabled").Value,
                out bool isMetricsEnabled) &&
            isMetricsEnabled)
        {
            // Add OpenTelemetry metrics
            // TODO: migrate to new version
            //services.AddOpenTelemetryMetrics(tm => tm.AddMeter(assemblyNames));
        }
    }

    /// <summary>
    ///     Regex to remove meta details from full assembly names.
    /// </summary>
    [GeneratedRegex(", (Version|Culture|PublicKeyToken)=[0-9.\\w]+")]
    private static partial Regex CleanupRegex();
}