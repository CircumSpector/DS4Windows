using AutoMapper;

using Vapour.Server.InputSource;
using Vapour.Shared.Devices.HID;

namespace Vapour.Client.Modules.InputSource.Utils;

public class InputSourceAutoMapper : Profile
{
    public InputSourceAutoMapper()
    {
        CreateMap<InputSourceController, InputSourceControllerItemViewModel>()
            .ForMember(dest => dest.DisplayText,
                cfg => cfg.MapFrom(src => $"{src.DisplayName} ({src.SerialNumberString})"))
            .ForMember(dest => dest.Serial,
                cfg => cfg.MapFrom(src => src.SerialNumberString))
            .ForMember(dest => dest.ConnectionTypeImage,
                cfg => cfg.MapFrom(src =>
                    src.Connection == ConnectionType.Bluetooth
                        ? InputSourceControllerItemViewModel.BluetoothImageLocation
                        : InputSourceControllerItemViewModel.UsbImageLocation))
            .ForMember(dest => dest.InstanceId, cfg => cfg.MapFrom(src => src.InstanceId))
            .ForMember(dest => dest.ParentInstance, cfg => cfg.MapFrom(src => src.ParentInstance))
            .ForMember(dest => dest.IsFiltered, cfg => cfg.MapFrom(src => src.IsFiltered))
            .ForMember(dest => dest.DeviceImage,
                cfg => cfg.MapFrom(src => InputSourceControllerItemViewModel.GetDeviceImage(src.DisplayName)));

        CreateMap<InputSourceMessage, InputSourceItemViewModel>()
            .ForMember(dest => dest.Controllers, cfg => cfg.Ignore())
            .ForMember(dest => dest.InputSourceKey, cfg => cfg.MapFrom(src => src.InputSourceKey))
            .ForMember(dest => dest.CurrentConfiguration, cfg => cfg.MapFrom(src => src.CurrentConfiguration));
    }
}