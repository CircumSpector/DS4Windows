using DS4Windows.Client.Core.DependencyInjection;
using DS4Windows.Shared.Common.Core;
using DS4Windows.Shared.Common.Telemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace DS4Windows.Client.Core.Tracing
{
    public class OpenTelemetryRegistrar : IServiceRegistrar
    {
        public void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
            if (bool.TryParse(configuration.GetSection("OpenTelemetry:IsTracingEnabled").Value, out var isEnabled) &&
                isEnabled)
                //
                // Initialize OpenTelemetry
                // 
                services.AddOpenTelemetryTracing(builder => builder
                    .SetSampler(new AlwaysOnSampler())
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(Constants.ApplicationName))
                    .AddSource(Constants.ApplicationName)
                    .AddSource(TracingSources.DevicesAssemblyActivitySourceName)
                    .AddSource(TracingSources.CommonAssemblyActivitySourceName)
                    .AddSource(TracingSources.ConfigurationApplicationAssemblyActivitySourceName)
                    .AddJaegerExporter(options => { options.ExportProcessorType = ExportProcessorType.Simple; })
                );
        }
    }
}
