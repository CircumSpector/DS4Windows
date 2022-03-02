using DS4Windows.Client.Core.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace DS4Windows.Client.Core.Tracing
{
    public class OpenTelemetryRegistrar : IServiceRegistrar
    {
        public void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
            /*if (bool.TryParse(configuration.GetSection("OpenTelemetry:IsTracingEnabled").Value, out var isEnabled) &&
                isEnabled)
                //
                // Initialize OpenTelemetry
                // 
                //TODO: open telemetry keeps giving me problems, need to figure it out and fix it later
                services.AddOpenTelemetryTracing(builder => builder
                    .SetSampler(new AlwaysOnSampler())
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(Constants.ApplicationName))
                    .AddSource(Constants.ApplicationName)
                    .AddSource(TracingSources.DevicesAssemblyActivitySourceName)
                    .AddSource(TracingSources.CommonAssemblyActivitySourceName)
                    .AddSource(TracingSources.ConfigurationApplicationAssemblyActivitySourceName)
                    .AddJaegerExporter(options => { options.ExportProcessorType = ExportProcessorType.Simple; })
            
                );
            */
        }

        public void Initialize(IServiceProvider services)
        {
        }
    }
}
