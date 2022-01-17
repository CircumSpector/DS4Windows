using System;
using System.Threading;
using System.Threading.Tasks;
using DS4WinWPF.DS4Control.IoC.Services;
using Microsoft.Extensions.Hosting;

namespace DS4WinWPF.DS4Control.IoC.HostedServices
{
    internal class ControllerManagerHost : BackgroundService
    {
        private readonly IDeviceEnumeratorService deviceEnumeratorService;

        public ControllerManagerHost(IDeviceEnumeratorService deviceEnumeratorService)
        {
            this.deviceEnumeratorService = deviceEnumeratorService;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }
    }
}