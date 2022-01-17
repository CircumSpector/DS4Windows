using System;
using System.Threading;
using System.Threading.Tasks;
using DS4WinWPF.DS4Control.IoC.Services;
using Microsoft.Extensions.Hosting;

namespace DS4WinWPF.DS4Control.IoC.HostedServices
{
    internal class ControllerManagerHost : BackgroundService
    {
        private readonly IControllerEnumeratorService controllerEnumeratorService;

        public ControllerManagerHost(IControllerEnumeratorService controllerEnumeratorService)
        {
            this.controllerEnumeratorService = controllerEnumeratorService;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }
    }
}