using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4WinWPF.DS4Control.IoC.Services
{
    internal interface ISupportedControllersService
    {
        void RefreshDevices();
    }

    internal class SupportedControllersService : ISupportedControllersService
    {


        public void RefreshDevices()
        {
            throw new NotImplementedException();
        }
    }
}