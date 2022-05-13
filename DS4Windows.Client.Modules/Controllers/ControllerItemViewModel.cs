using AutoMapper;
using DS4Windows.Client.Core.ViewModel;
using DS4Windows.Shared.Configuration.Profiles.Services;
using DS4Windows.Shared.Devices.HID;
using System;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DS4Windows.Server;
using DS4Windows.Server.Controller;

namespace DS4Windows.Client.Modules.Controllers
{
    public class ControllerItemViewModel : ViewModel<IControllerItemViewModel>, IControllerItemViewModel
    {
        private const string imageLocationRoot = "pack://application:,,,/DS4Windows.Client.Modules;component/Controllers/Images";
        private readonly IMapper mapper;
        public static BitmapImage dualSenseImageLocation = new BitmapImage(new Uri($"{imageLocationRoot}/dualsense.jpg", UriKind.Absolute));
        public static BitmapImage dualShockV2ImageLocation = new BitmapImage(new Uri($"{imageLocationRoot}/dualshockv2.jpg", UriKind.Absolute));
        public static BitmapImage joyconLeftImageLocation = new BitmapImage(new Uri($"{imageLocationRoot}/joyconleft.jpg", UriKind.Absolute));
        public static BitmapImage joyconRightImageLocation = new BitmapImage(new Uri($"{imageLocationRoot}/joyconright.jpg", UriKind.Absolute));
        public static BitmapImage switchProImageLocation = new BitmapImage(new Uri($"{imageLocationRoot}/switchpro.jpg", UriKind.Absolute));
        public static BitmapImage BluetoothImageLocation = new BitmapImage(new Uri($"{imageLocationRoot}/BT.png", UriKind.Absolute));
        public static BitmapImage UsbImageLocation = new BitmapImage(new Uri($"{imageLocationRoot}/USB_white.png", UriKind.Absolute));

        public ControllerItemViewModel(IMapper mapper)
        {
            this.mapper = mapper;
        }

        #region Props

        private PhysicalAddress serial;
        public PhysicalAddress Serial
        {
            get => serial;
            private set => SetProperty(ref serial, value);
        }

        private BitmapImage deviceImage;
        public BitmapImage DeviceImage
        {
            get => deviceImage; 
            private set => SetProperty(ref deviceImage, value); 
        }

        private string displayText;
        public string DisplayText
        {
            get => displayText; 
            private set => SetProperty(ref displayText, value); 
        }

        private BitmapImage connectionTypeImage;
        public BitmapImage ConnectionTypeImage
        {
            get => connectionTypeImage; 
            private set => SetProperty(ref connectionTypeImage, value); 
        }

        private decimal batteryPercentage;
        public decimal BatteryPercentage
        {
            get => batteryPercentage; 
            private set => SetProperty(ref batteryPercentage, value); 
        }

        private Guid selectedProfileId;
        public Guid SelectedProfileId
        {
            get => selectedProfileId; 
            set => SetProperty(ref selectedProfileId, value); 
        }

        private SolidColorBrush currentColor;
        public SolidColorBrush CurrentColor
        {
            get => currentColor;
            set => SetProperty(ref currentColor, value);
        }

        public string InstanceId { get; set; }
        public string ParentInstance { get; set; }

        #endregion

        public void SetDevice(ControllerConnectedMessage device)
        {
            mapper.Map(device, this);
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedProfileId))
            {
                //var activeProfile = profilesService.ActiveProfiles.Single(p => p.DeviceId != null && p.DeviceId.Equals(Serial));
                //var slotIndex = profilesService.ActiveProfiles.IndexOf(activeProfile);
                //profilesService.SetActiveTo(slotIndex, activeProfile);
            }

            base.OnPropertyChanged(e);
        }
    }
}
