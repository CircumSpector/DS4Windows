using AutoMapper;
using DS4Windows.Shared.Configuration.Profiles.Schema;
using System.Windows.Media;

namespace DS4Windows.Client.Modules.Profiles
{
    public class ProfilesAutoMapper : Profile
    {
        public ProfilesAutoMapper()
        {
            CreateMap<IProfile, ProfileListItemViewModel>()
                .ForMember(dest => dest.Id, cfg => cfg.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, cfg => cfg.MapFrom(src => src.DisplayName))
                .ForMember(dest => dest.OutputControllerType, cfg => cfg.MapFrom(src => src.OutputDeviceType.ToString()))
                .ForMember(dest => dest.LightbarColor, cfg => cfg.MapFrom(src => new SolidColorBrush(src.LightbarSettingInfo.Ds4WinSettings.Led.ToColor())))
                .ForMember(dest => dest.TouchpadMode, cfg => cfg.MapFrom(src => src.TouchOutMode.ToString()))
                .ForMember(dest => dest.GyroMode, cfg => cfg.MapFrom(src => src.GyroOutputMode.ToString()));

            CreateMap<IProfile, ProfileEditViewModel>()
                .ForMember(dest => dest.Name, cfg => cfg.MapFrom(src => src.DisplayName))
            #region Left Stick
                .ForPath(dest => dest.LeftStick.DeadZone, cfg => cfg.MapFrom(src => src.LSModInfo.DeadZone))
                .ForPath(dest => dest.LeftStick.AntiDeadZone, cfg => cfg.MapFrom(src => src.LSModInfo.AntiDeadZone))
                .ForPath(dest => dest.LeftStick.MaxZone, cfg => cfg.MapFrom(src => src.LSModInfo.DeadZone))
                .ForPath(dest => dest.LeftStick.MaxOutput, cfg => cfg.MapFrom(src => src.LSModInfo.MaxOutput))
            #endregion
            #region Right Stick
                .ForPath(dest => dest.RightStick.DeadZone, cfg => cfg.MapFrom(src => src.RSModInfo.DeadZone))
                .ForPath(dest => dest.RightStick.AntiDeadZone, cfg => cfg.MapFrom(src => src.RSModInfo.AntiDeadZone))
                .ForPath(dest => dest.RightStick.MaxZone, cfg => cfg.MapFrom(src => src.RSModInfo.DeadZone))
                .ForPath(dest => dest.RightStick.MaxOutput, cfg => cfg.MapFrom(src => src.RSModInfo.MaxOutput));
            #endregion

            CreateMap<ProfileEditViewModel, IProfile>()
                .ForMember(dest => dest.DisplayName, cfg => cfg.MapFrom(src => src.Name))
            #region Left Stick
                .ForPath(dest => dest.LSModInfo.DeadZone, cfg => cfg.MapFrom(src => src.LeftStick.DeadZone))
                .ForPath(dest => dest.LSModInfo.AntiDeadZone, cfg => cfg.MapFrom(src => src.LeftStick.AntiDeadZone))
                .ForPath(dest => dest.LSModInfo.MaxZone, cfg => cfg.MapFrom(src => src.LeftStick.DeadZone))
                .ForPath(dest => dest.LSModInfo.MaxOutput, cfg => cfg.MapFrom(src => src.LeftStick.MaxOutput))
            #endregion
            #region Right Stick
                .ForPath(dest => dest.RSModInfo.DeadZone, cfg => cfg.MapFrom(src => src.RightStick.DeadZone))
                .ForPath(dest => dest.RSModInfo.AntiDeadZone, cfg => cfg.MapFrom(src => src.RightStick.AntiDeadZone))
                .ForPath(dest => dest.RSModInfo.MaxZone, cfg => cfg.MapFrom(src => src.RightStick.DeadZone))
                .ForPath(dest => dest.RSModInfo.MaxOutput, cfg => cfg.MapFrom(src => src.RightStick.MaxOutput));
            #endregion
        }
    }
}
