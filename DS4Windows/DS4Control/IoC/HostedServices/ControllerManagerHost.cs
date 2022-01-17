using System;
using System.Threading;
using System.Threading.Tasks;
using DS4WinWPF.DS4Control.IoC.Services;
using Microsoft.Extensions.Hosting;

namespace DS4WinWPF.DS4Control.IoC.HostedServices
{
    class ControllerManagerHost : BackgroundService
    {
        private readonly IControllerManagerService controllerManagerService;

        public ControllerManagerHost(IControllerManagerService controllerManagerService)
        {
            this.controllerManagerService = controllerManagerService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //
            // TODO: implement me!
            // 
            await Task.Delay(-1, stoppingToken);
        }
    }
}
