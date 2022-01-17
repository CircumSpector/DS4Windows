using System;
using System.Threading;
using System.Threading.Tasks;
using DS4WinWPF.DS4Control.IoC.Services;
using Microsoft.Extensions.Hosting;

namespace DS4WinWPF.DS4Control.IoC.HostedServices
{
    internal class ControllerManagerHost : BackgroundService
    {
        private readonly IHidDeviceEnumeratorService hidDeviceEnumeratorService;

        public ControllerManagerHost(IHidDeviceEnumeratorService hidDeviceEnumeratorService)
        {
            this.hidDeviceEnumeratorService = hidDeviceEnumeratorService;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }
    }
}