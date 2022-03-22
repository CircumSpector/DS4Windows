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
                .ForPath(dest => dest.LeftStick.ControlModeSettings.DeadZone, cfg => cfg.MapFrom(src => src.LSModInfo.DeadZone))
                .ForPath(dest => dest.LeftStick.ControlModeSettings.AntiDeadZone, cfg => cfg.MapFrom(src => src.LSModInfo.AntiDeadZone))
                .ForPath(dest => dest.LeftStick.ControlModeSettings.MaxZone, cfg => cfg.MapFrom(src => src.LSModInfo.DeadZone))
                .ForPath(dest => dest.LeftStick.ControlModeSettings.MaxOutput, cfg => cfg.MapFrom(src => src.LSModInfo.MaxOutput))
                .ForPath(dest => dest.LeftStick.ControlModeSettings.DeadZoneType, cfg => cfg.MapFrom(src => src.LSModInfo.DZType))
                .ForPath(dest => dest.LeftStick.ControlModeSettings.VerticalScale, cfg => cfg.MapFrom(src => src.LSModInfo.VerticalScale))
                .ForPath(dest => dest.LeftStick.ControlModeSettings.Sensitivity, cfg => cfg.MapFrom(src => src.LSSens))
                .ForPath(dest => dest.LeftStick.OutputSettings, cfg => cfg.MapFrom(src => src.LSOutputSettings.Mode))
            #endregion
            #region Right Stick
                .ForPath(dest => dest.RightStick.ControlModeSettings.DeadZone, cfg => cfg.MapFrom(src => src.RSModInfo.DeadZone))
                .ForPath(dest => dest.RightStick.ControlModeSettings.AntiDeadZone, cfg => cfg.MapFrom(src => src.RSModInfo.AntiDeadZone))
                .ForPath(dest => dest.RightStick.ControlModeSettings.MaxZone, cfg => cfg.MapFrom(src => src.RSModInfo.DeadZone))
                .ForPath(dest => dest.RightStick.ControlModeSettings.MaxOutput, cfg => cfg.MapFrom(src => src.RSModInfo.MaxOutput))
                .ForPath(dest => dest.RightStick.ControlModeSettings.DeadZoneType, cfg => cfg.MapFrom(src => src.RSModInfo.DZType))
                .ForPath(dest => dest.RightStick.ControlModeSettings.VerticalScale, cfg => cfg.MapFrom(src => src.RSModInfo.VerticalScale))
                .ForPath(dest => dest.RightStick.ControlModeSettings.Sensitivity, cfg => cfg.MapFrom(src => src.RSSens))
                .ForPath(dest => dest.RightStick.OutputSettings, cfg => cfg.MapFrom(src => src.RSOutputSettings.Mode));
            #endregion

            CreateMap<ProfileEditViewModel, IProfile>()
                .ForMember(dest => dest.DisplayName, cfg => cfg.MapFrom(src => src.Name))
            #region Left Stick
                .ForPath(dest => dest.LSModInfo.DeadZone, cfg => cfg.MapFrom(src => src.LeftStick.ControlModeSettings.DeadZone))
                .ForPath(dest => dest.LSModInfo.AntiDeadZone, cfg => cfg.MapFrom(src => src.LeftStick.ControlModeSettings.AntiDeadZone))
                .ForPath(dest => dest.LSModInfo.MaxZone, cfg => cfg.MapFrom(src => src.LeftStick.ControlModeSettings.DeadZone))
                .ForPath(dest => dest.LSModInfo.MaxOutput, cfg => cfg.MapFrom(src => src.LeftStick.ControlModeSettings.MaxOutput))
                .ForPath(dest => dest.LSModInfo.DZType, cfg => cfg.MapFrom(src => src.LeftStick.ControlModeSettings.DeadZoneType))
                .ForPath(dest => dest.LSModInfo.VerticalScale, cfg => cfg.MapFrom(src => src.LeftStick.ControlModeSettings.VerticalScale))
                .ForPath(dest => dest.LSSens, cfg => cfg.MapFrom(src => src.LeftStick.ControlModeSettings.Sensitivity))
                .ForPath(dest => dest.LSOutputSettings.Mode, cfg => cfg.MapFrom(src => src.LeftStick.OutputSettings))
            #endregion
            #region Right Stick
                .ForPath(dest => dest.RSModInfo.DeadZone, cfg => cfg.MapFrom(src => src.RightStick.ControlModeSettings.DeadZone))
                .ForPath(dest => dest.RSModInfo.AntiDeadZone, cfg => cfg.MapFrom(src => src.RightStick.ControlModeSettings.AntiDeadZone))
                .ForPath(dest => dest.RSModInfo.MaxZone, cfg => cfg.MapFrom(src => src.RightStick.ControlModeSettings.DeadZone))
                .ForPath(dest => dest.RSModInfo.MaxOutput, cfg => cfg.MapFrom(src => src.RightStick.ControlModeSettings.MaxOutput))
                .ForPath(dest => dest.RSModInfo.DZType, cfg => cfg.MapFrom(src => src.RightStick.ControlModeSettings.DeadZoneType))
                .ForPath(dest => dest.RSModInfo.VerticalScale, cfg => cfg.MapFrom(src => src.RightStick.ControlModeSettings.VerticalScale))
                .ForPath(dest => dest.RSSens, cfg => cfg.MapFrom(src => src.RightStick.ControlModeSettings.Sensitivity))
                .ForPath(dest => dest.RSOutputSettings.Mode, cfg => cfg.MapFrom(src => src.RightStick.OutputSettings));
            #endregion
        }
    }
}
