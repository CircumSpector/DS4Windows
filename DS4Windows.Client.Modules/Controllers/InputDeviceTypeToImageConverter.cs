using DS4Windows.Shared.Devices.HID;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace DS4Windows.Client.Modules.Controllers
{
    public class InputDeviceTypeToImageConverter : IValueConverter
    {
        private const string imageLocationRoot = "pack://application:,,,/DS4Windows.Client.Modules;component/Controllers/Images";
        private static BitmapImage dualSenseImageLocation = new BitmapImage(new Uri($"{imageLocationRoot}/dualsense.jpg", UriKind.Absolute));
        private static BitmapImage dualShockV2ImageLocation = new BitmapImage(new Uri($"{imageLocationRoot}/dualshockv2.jpg", UriKind.Absolute));
        private static BitmapImage joyconLeftImageLocation = new BitmapImage(new Uri($"{imageLocationRoot}/joyconleft.jpg", UriKind.Absolute));
        private static BitmapImage joyconRightImageLocation = new BitmapImage(new Uri($"{imageLocationRoot}/joyconright.jpg", UriKind.Absolute));
        private static BitmapImage switchProImageLocation = new BitmapImage(new Uri($"{imageLocationRoot}/switchpro.jpg", UriKind.Absolute));
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }
            else
            {
                var type = (InputDeviceType)value;
                switch (type)
                {
                    case InputDeviceType.DualSense: return dualSenseImageLocation;
                    case InputDeviceType.DualShock4: return dualShockV2ImageLocation;
                    case InputDeviceType.JoyConL: return joyconLeftImageLocation;
                    case InputDeviceType.JoyConR: return joyconRightImageLocation;
                    case InputDeviceType.SwitchPro: return switchProImageLocation;
                    default: return null;
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
