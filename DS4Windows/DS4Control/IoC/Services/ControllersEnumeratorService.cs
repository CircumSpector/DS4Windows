using System;
using Microsoft.Extensions.Logging;

namespace DS4WinWPF.DS4Control.IoC.Services
{
    internal interface IControllersEnumeratorService
    {
        void RefreshDevices();
    }

    internal class ControllersEnumeratorService : IControllersEnumeratorService
    {
        private readonly ILogger<ControllersEnumeratorService> logger;

        public ControllersEnumeratorService(ILogger<ControllersEnumeratorService> logger)
        {
            this.logger = logger;
        }

        public void RefreshDevices()
        {
            throw new NotImplementedException();
        }
    }
}