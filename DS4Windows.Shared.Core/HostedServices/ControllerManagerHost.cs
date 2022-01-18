using System.Threading;
using System.Threading.Tasks;
using DS4Windows.Shared.Core.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DS4Windows.Shared.Core.HostedServices
{
    public class ControllerManagerHost : IHostedService
    {
        private readonly IControllersEnumeratorService enumeratorService;

        private readonly ILogger<ControllerManagerHost> logger;

        public ControllerManagerHost(IControllersEnumeratorService enumeratorService,
            ILogger<ControllerManagerHost> logger)
        {
            this.enumeratorService = enumeratorService;
            this.logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting device enumeration");

            await Task.Run(() => enumeratorService.EnumerateDevices(), cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}