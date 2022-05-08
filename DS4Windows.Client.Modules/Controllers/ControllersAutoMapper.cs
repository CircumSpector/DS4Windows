using AutoMapper;
using DS4Windows.Shared.Devices.HID;
using System.Windows.Media.Imaging;
using DS4Windows.Server;

namespace DS4Windows.Client.Modules.Controllers
{
    public class ControllersAutoMapper : Profile
    {
        public ControllersAutoMapper()
        {
            CreateMap<ControllerConnectedMessage, ControllerItemViewModel>()
                .ForMember(dest => dest.DisplayText, cfg => cfg.MapFrom(src => $"{src.DeviceType} ({src.SerialNumberString})"))
                .ForMember(dest => dest.ConnectionTypeImage, cfg => cfg.MapFrom(src => src.Connection == ConnectionType.Bluetooth ? ControllerItemViewModel.BluetoothImageLocation : ControllerItemViewModel.UsbImageLocation))
                .ForMember(dest => dest.InstanceId, cfg => cfg.MapFrom(src => src.InstanceId))
                .ForMember(dest => dest.ParentInstance, cfg => cfg.MapFrom(src => src.ParentInstance))
                .ForMember(dest => dest.SelectedProfileId, cfg => cfg.MapFrom(src => src.SelectedProfileId))
                .ForMember(dest => dest.DeviceImage, cfg => cfg.MapFrom((src, dest) =>
                {
                    BitmapImage deviceImage = null;
                    switch (src.DeviceType)
                    {
                        case InputDeviceType.DualSense:
                            deviceImage = ControllerItemViewModel.dualSenseImageLocation;
                            break;
                        case InputDeviceType.DualShock4:
                            deviceImage = ControllerItemViewModel.dualShockV2ImageLocation;
                            break;
                        case InputDeviceType.JoyConL:
                            deviceImage = ControllerItemViewModel.joyconLeftImageLocation;
                            break;
                        case InputDeviceType.JoyConR:
                            deviceImage = ControllerItemViewModel.joyconRightImageLocation;
                            break;
                        case InputDeviceType.SwitchPro:
                            deviceImage = ControllerItemViewModel.switchProImageLocation;
                            break;
                    }
                    return deviceImage;
                }));
        }
    }
}
