using System;
using System.Linq;
using System.Text.RegularExpressions;
using DS4Windows.Client.Core.DependencyInjection;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace DS4Windows.Client.Core.Tracing;

[UsedImplicitly]
public class OpenTelemetryRegistrar : IServiceRegistrar
{
    private const string AssemblyPrefix = "DS4Windows";

    private static readonly Regex Cleanup = new(", (Version|Culture|PublicKeyToken)=[0-9.\\w]+", RegexOptions.Compiled);

    public void ConfigureServices(IConfiguration configuration, IServiceCollection services)
    {
        // Get list of assemblies, register them all as potential tracing sources
        var assemblyNames = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(assembly =>
            {
                var name = assembly.GetName().Name;
                return name != null && name.StartsWith(AssemblyPrefix);
            })
            .Select(assembly => Cleanup.Replace(assembly.GetName().Name!, string.Empty))
            .ToArray();

        if (bool.TryParse(configuration.GetSection("OpenTelemetry:IsTracingEnabled").Value, out var isEnabled) &&
            isEnabled)
            //
            // Initialize OpenTelemetry
            // 
            services.AddOpenTelemetryTracing(builder => builder
                .SetSampler(new AlwaysOnSampler())
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(AssemblyPrefix))
                .AddSource(assemblyNames)
                .AddJaegerExporter(options => { options.ExportProcessorType = ExportProcessorType.Simple; })
            );
    }

    public void Initialize(IServiceProvider services)
    {
    }
}