using System.Threading;
using System.Threading.Tasks;
using DS4WinWPF.DS4Control.IoC.Services;
using Microsoft.Extensions.Hosting;

namespace DS4WinWPF.DS4Control.IoC.HostedServices
{
    internal class ControllerManagerHost : IHostedService
    {
        private readonly IControllersEnumeratorService enumeratorService;

        public ControllerManagerHost(IControllersEnumeratorService enumeratorService)
        {
            this.enumeratorService = enumeratorService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Task.Run(() => enumeratorService.EnumerateDevices(), cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}