using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4WinWPF.DS4Control.IoC.Services
{
    internal interface IControllersEnumeratorService
    {
        void RefreshDevices();
    }

    internal class ControllersEnumeratorService : IControllersEnumeratorService
    {


        public void RefreshDevices()
        {
            throw new NotImplementedException();
        }
    }
}