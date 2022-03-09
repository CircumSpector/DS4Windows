using DS4Windows.Client.Core.ViewModel;
using DS4Windows.Shared.Devices.HID;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4Windows.Client.Modules.Controllers
{
    public class ControllerItemViewModel : ViewModel<IControllerItemViewModel>, IControllerItemViewModel
    {
        public CompatibleHidDevice? Device { get; set; }
    }
}
