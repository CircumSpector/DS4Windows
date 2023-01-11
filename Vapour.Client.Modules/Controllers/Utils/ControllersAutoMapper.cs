using System.Windows.Media.Imaging;

using AutoMapper;

using Vapour.Server.Controller;
using Vapour.Shared.Devices.HID;

namespace Vapour.Client.Modules.Controllers.Utils;

public class ControllersAutoMapper : Profile
{
    public ControllersAutoMapper()
    {
        CreateMap<ControllerConnectedMessage, ControllerItemViewModel>()
            .ForMember(dest => dest.DisplayText,
                cfg => cfg.MapFrom(src => $"{src.DeviceType} ({src.SerialNumberString})"))
            .ForMember(dest => dest.Serial,
                cfg => cfg.MapFrom(src => src.SerialNumberString))
            .ForMember(dest => dest.ConnectionTypeImage,
                cfg => cfg.MapFrom(src =>
                    src.Connection == ConnectionType.Bluetooth
                        ? ControllerItemViewModel.BluetoothImageLocation
                        : ControllerItemViewModel.UsbImageLocation))
            .ForMember(dest => dest.InstanceId, cfg => cfg.MapFrom(src => src.InstanceId))
            .ForMember(dest => dest.ParentInstance, cfg => cfg.MapFrom(src => src.ParentInstance))
            .ForMember(dest => dest.CurrentConfiguration, cfg => cfg.MapFrom(src => src.CurrentConfiguration))
            .ForMember(dest => dest.IsFiltered, cfg => cfg.MapFrom(src => src.IsFiltered))
            .ForMember(dest => dest.DeviceImage, cfg => cfg.MapFrom((src, dest) =>
            {
                BitmapImage deviceImage = null;

                var key = ControllerItemViewModel.ImageLocations.Keys.SingleOrDefault(k =>
                    k.Vid == src.Vid && k.Pid == src.Pid);
                if (key != null)
                {
                    deviceImage = ControllerItemViewModel.ImageLocations[key];
                }

                return deviceImage;
            }));
    }
}