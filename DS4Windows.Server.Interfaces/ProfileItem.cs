using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using DS4Windows.Shared.Common.Types;

namespace DS4Windows.Server
{
    public class ProfileItem
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; }
        public OutputDeviceType OutputDeviceType { get; set; }
        public SolidColorBrush LightbarColor { get; set; }
        public string TouchpadMode { get; set; }
        public string GyroMode { get; set; }
    }
}
