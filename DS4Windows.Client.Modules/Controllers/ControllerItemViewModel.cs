using DS4Windows.Client.Core.ViewModel;
using DS4Windows.Shared.Configuration.Profiles.Services;
using DS4Windows.Shared.Devices.HID;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DS4Windows.Client.Modules.Controllers
{
    public class ControllerItemViewModel : ViewModel<IControllerItemViewModel>, IControllerItemViewModel
    {
        private const string imageLocationRoot = "pack://application:,,,/DS4Windows.Client.Modules;component/Controllers/Images";
        private readonly IProfilesService profilesService;
        private static BitmapImage dualSenseImageLocation = new BitmapImage(new Uri($"{imageLocationRoot}/dualsense.jpg", UriKind.Absolute));
        private static BitmapImage dualShockV2ImageLocation = new BitmapImage(new Uri($"{imageLocationRoot}/dualshockv2.jpg", UriKind.Absolute));
        private static BitmapImage joyconLeftImageLocation = new BitmapImage(new Uri($"{imageLocationRoot}/joyconleft.jpg", UriKind.Absolute));
        private static BitmapImage joyconRightImageLocation = new BitmapImage(new Uri($"{imageLocationRoot}/joyconright.jpg", UriKind.Absolute));
        private static BitmapImage switchProImageLocation = new BitmapImage(new Uri($"{imageLocationRoot}/switchpro.jpg", UriKind.Absolute));
        private static BitmapImage BluetoothImageLocation = new BitmapImage(new Uri($"{imageLocationRoot}/BT_white.png", UriKind.Absolute));
        private static BitmapImage UsbImageLocation = new BitmapImage(new Uri($"{imageLocationRoot}/USB_white.png", UriKind.Absolute));

        public ControllerItemViewModel(IProfilesService profilesService)
        {
            this.profilesService = profilesService;
        }

        #region Props

        private CompatibleHidDevice? device;

        public string? InstanceId { get; set; }

        public BitmapImage? DeviceImage { get; set; }

        public string? DisplayText { get; private set; }

        public BitmapImage? ConnectionTypeImage { get; private set; }

        public bool IsExclusive { get; private set; }

        public decimal BatteryPercentage { get; private set; }

        public Guid? SelectedProfileId { get; set; }

        public SolidColorBrush? CurrentColor { get; set; }

        #endregion

        public void SetDevice(CompatibleHidDevice? device)
        {
            this.device = device;
            MapProperties();
        }

        private void MapProperties()
        {
            DisplayText = $"{device.DeviceType} ({device.SerialNumberString})";

            if (device.Connection == ConnectionType.Bluetooth)
            {
                ConnectionTypeImage = BluetoothImageLocation;
            }
            else
            {
                ConnectionTypeImage = UsbImageLocation;
            }

            switch (device.DeviceType)
            {
                case InputDeviceType.DualSense: 
                    DeviceImage = dualSenseImageLocation;
                    break;
                case InputDeviceType.DualShock4: 
                    DeviceImage = dualShockV2ImageLocation;
                    break;
                case InputDeviceType.JoyConL: 
                    DeviceImage = joyconLeftImageLocation;
                    break;
                case InputDeviceType.JoyConR: 
                    DeviceImage = joyconRightImageLocation;
                    break;
                case InputDeviceType.SwitchPro: 
                    DeviceImage = switchProImageLocation;
                    break;
            }

            SelectedProfileId = this.profilesService.ActiveProfiles.Single(p => p.DeviceId == device.Serial).Id;
        }
    }
}
