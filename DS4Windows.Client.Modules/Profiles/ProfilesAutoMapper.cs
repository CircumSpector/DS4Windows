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
                .ForPath(dest => dest.LeftStick.ControlModeSettings.ForceMaxOutput, cfg => cfg.MapFrom(src => src.LSModInfo.MaxOutputForce))
                .ForPath(dest => dest.LeftStick.ControlModeSettings.DeadZoneType, cfg => cfg.MapFrom(src => src.LSModInfo.DZType))
                .ForPath(dest => dest.LeftStick.ControlModeSettings.VerticalScale, cfg => cfg.MapFrom(src => src.LSModInfo.VerticalScale))
                .ForPath(dest => dest.LeftStick.ControlModeSettings.Sensitivity, cfg => cfg.MapFrom(src => src.LSSens))
                .ForPath(dest => dest.LeftStick.ControlModeSettings.XDeadZone, cfg => cfg.MapFrom(src => src.LSModInfo.XAxisDeadInfo.DeadZone))
                .ForPath(dest => dest.LeftStick.ControlModeSettings.XMaxZone, cfg => cfg.MapFrom(src => src.LSModInfo.XAxisDeadInfo.MaxZone))
                .ForPath(dest => dest.LeftStick.ControlModeSettings.XAntiDeadZone, cfg => cfg.MapFrom(src => src.LSModInfo.XAxisDeadInfo.AntiDeadZone))
                .ForPath(dest => dest.LeftStick.ControlModeSettings.XMaxOutput, cfg => cfg.MapFrom(src => src.LSModInfo.XAxisDeadInfo.MaxOutput))
                .ForPath(dest => dest.LeftStick.ControlModeSettings.YDeadZone, cfg => cfg.MapFrom(src => src.LSModInfo.YAxisDeadInfo.DeadZone))
                .ForPath(dest => dest.LeftStick.ControlModeSettings.YMaxZone, cfg => cfg.MapFrom(src => src.LSModInfo.YAxisDeadInfo.MaxZone))
                .ForPath(dest => dest.LeftStick.ControlModeSettings.YAntiDeadZone, cfg => cfg.MapFrom(src => src.LSModInfo.YAxisDeadInfo.AntiDeadZone))
                .ForPath(dest => dest.LeftStick.ControlModeSettings.YMaxOutput, cfg => cfg.MapFrom(src => src.LSModInfo.YAxisDeadInfo.MaxOutput))
                .ForPath(dest => dest.LeftStick.OutputSettings, cfg => cfg.MapFrom(src => src.LSOutputSettings.Mode))
                
            #endregion

            #region Right Stick

                .ForPath(dest => dest.RightStick.ControlModeSettings.DeadZone, cfg => cfg.MapFrom(src => src.RSModInfo.DeadZone))
                .ForPath(dest => dest.RightStick.ControlModeSettings.AntiDeadZone, cfg => cfg.MapFrom(src => src.RSModInfo.AntiDeadZone))
                .ForPath(dest => dest.RightStick.ControlModeSettings.MaxZone, cfg => cfg.MapFrom(src => src.RSModInfo.DeadZone))
                .ForPath(dest => dest.RightStick.ControlModeSettings.MaxOutput, cfg => cfg.MapFrom(src => src.RSModInfo.MaxOutput))
                .ForPath(dest => dest.RightStick.ControlModeSettings.ForceMaxOutput, cfg => cfg.MapFrom(src => src.RSModInfo.MaxOutputForce))
                .ForPath(dest => dest.RightStick.ControlModeSettings.DeadZoneType, cfg => cfg.MapFrom(src => src.RSModInfo.DZType))
                .ForPath(dest => dest.RightStick.ControlModeSettings.VerticalScale, cfg => cfg.MapFrom(src => src.RSModInfo.VerticalScale))
                .ForPath(dest => dest.RightStick.ControlModeSettings.Sensitivity, cfg => cfg.MapFrom(src => src.RSSens))
                .ForPath(dest => dest.RightStick.ControlModeSettings.XDeadZone, cfg => cfg.MapFrom(src => src.RSModInfo.XAxisDeadInfo.DeadZone))
                .ForPath(dest => dest.RightStick.ControlModeSettings.XMaxZone, cfg => cfg.MapFrom(src => src.RSModInfo.XAxisDeadInfo.MaxZone))
                .ForPath(dest => dest.RightStick.ControlModeSettings.XAntiDeadZone, cfg => cfg.MapFrom(src => src.RSModInfo.XAxisDeadInfo.AntiDeadZone))
                .ForPath(dest => dest.RightStick.ControlModeSettings.XMaxOutput, cfg => cfg.MapFrom(src => src.RSModInfo.XAxisDeadInfo.MaxOutput))
                .ForPath(dest => dest.RightStick.ControlModeSettings.YDeadZone, cfg => cfg.MapFrom(src => src.RSModInfo.YAxisDeadInfo.DeadZone))
                .ForPath(dest => dest.RightStick.ControlModeSettings.YMaxZone, cfg => cfg.MapFrom(src => src.RSModInfo.YAxisDeadInfo.MaxZone))
                .ForPath(dest => dest.RightStick.ControlModeSettings.YAntiDeadZone, cfg => cfg.MapFrom(src => src.RSModInfo.YAxisDeadInfo.AntiDeadZone))
                .ForPath(dest => dest.RightStick.ControlModeSettings.YMaxOutput, cfg => cfg.MapFrom(src => src.RSModInfo.YAxisDeadInfo.MaxOutput))
                .ForPath(dest => dest.RightStick.OutputSettings, cfg => cfg.MapFrom(src => src.RSOutputSettings.Mode));

            #endregion

            CreateMap<ProfileEditViewModel, IProfile>()
                .ForMember(dest => dest.DisplayName, cfg => cfg.MapFrom(src => src.Name))

            #region Left Stick

                .ForPath(dest => dest.LSModInfo.DeadZone, cfg => cfg.MapFrom(src => src.LeftStick.ControlModeSettings.DeadZone))
                .ForPath(dest => dest.LSModInfo.AntiDeadZone, cfg => cfg.MapFrom(src => src.LeftStick.ControlModeSettings.AntiDeadZone))
                .ForPath(dest => dest.LSModInfo.MaxZone, cfg => cfg.MapFrom(src => src.LeftStick.ControlModeSettings.DeadZone))
                .ForPath(dest => dest.LSModInfo.MaxOutput, cfg => cfg.MapFrom(src => src.LeftStick.ControlModeSettings.MaxOutput))
                .ForPath(dest => dest.LSModInfo.MaxOutputForce, cfg => cfg.MapFrom(src => src.LeftStick.ControlModeSettings.ForceMaxOutput))
                .ForPath(dest => dest.LSModInfo.DZType, cfg => cfg.MapFrom(src => src.LeftStick.ControlModeSettings.DeadZoneType))
                .ForPath(dest => dest.LSModInfo.VerticalScale, cfg => cfg.MapFrom(src => src.LeftStick.ControlModeSettings.VerticalScale))
                .ForPath(dest => dest.LSSens, cfg => cfg.MapFrom(src => src.LeftStick.ControlModeSettings.Sensitivity))
                .ForPath(dest => dest.LSModInfo.XAxisDeadInfo.DeadZone, cfg => cfg.MapFrom(src => src.LeftStick.ControlModeSettings.XDeadZone))
                .ForPath(dest => dest.LSModInfo.XAxisDeadInfo.MaxZone, cfg => cfg.MapFrom(src => src.LeftStick.ControlModeSettings.XMaxZone))
                .ForPath(dest => dest.LSModInfo.XAxisDeadInfo.AntiDeadZone, cfg => cfg.MapFrom(src => src.LeftStick.ControlModeSettings.XAntiDeadZone))
                .ForPath(dest => dest.LSModInfo.XAxisDeadInfo.MaxOutput, cfg => cfg.MapFrom(src => src.LeftStick.ControlModeSettings.XMaxOutput))
                .ForPath(dest => dest.LSModInfo.YAxisDeadInfo.DeadZone, cfg => cfg.MapFrom(src => src.LeftStick.ControlModeSettings.YDeadZone))
                .ForPath(dest => dest.LSModInfo.YAxisDeadInfo.MaxZone, cfg => cfg.MapFrom(src => src.LeftStick.ControlModeSettings.YMaxZone))
                .ForPath(dest => dest.LSModInfo.YAxisDeadInfo.AntiDeadZone, cfg => cfg.MapFrom(src => src.LeftStick.ControlModeSettings.YAntiDeadZone))
                .ForPath(dest => dest.LSModInfo.YAxisDeadInfo.MaxOutput, cfg => cfg.MapFrom(src => src.LeftStick.ControlModeSettings.YMaxOutput))
                .ForPath(dest => dest.LSOutputSettings.Mode, cfg => cfg.MapFrom(src => src.LeftStick.OutputSettings))

            #endregion

            #region Right Stick
                .ForPath(dest => dest.RSModInfo.DeadZone, cfg => cfg.MapFrom(src => src.RightStick.ControlModeSettings.DeadZone))
                .ForPath(dest => dest.RSModInfo.AntiDeadZone, cfg => cfg.MapFrom(src => src.RightStick.ControlModeSettings.AntiDeadZone))
                .ForPath(dest => dest.RSModInfo.MaxZone, cfg => cfg.MapFrom(src => src.RightStick.ControlModeSettings.DeadZone))
                .ForPath(dest => dest.RSModInfo.MaxOutput, cfg => cfg.MapFrom(src => src.RightStick.ControlModeSettings.MaxOutput))
                .ForPath(dest => dest.RSModInfo.MaxOutputForce, cfg => cfg.MapFrom(src => src.RightStick.ControlModeSettings.ForceMaxOutput))
                .ForPath(dest => dest.RSModInfo.DZType, cfg => cfg.MapFrom(src => src.RightStick.ControlModeSettings.DeadZoneType))
                .ForPath(dest => dest.RSModInfo.VerticalScale, cfg => cfg.MapFrom(src => src.RightStick.ControlModeSettings.VerticalScale))
                .ForPath(dest => dest.RSSens, cfg => cfg.MapFrom(src => src.RightStick.ControlModeSettings.Sensitivity))
                .ForPath(dest => dest.RSModInfo.XAxisDeadInfo.DeadZone, cfg => cfg.MapFrom(src => src.RightStick.ControlModeSettings.XDeadZone))
                .ForPath(dest => dest.RSModInfo.XAxisDeadInfo.MaxZone, cfg => cfg.MapFrom(src => src.RightStick.ControlModeSettings.XMaxZone))
                .ForPath(dest => dest.RSModInfo.XAxisDeadInfo.AntiDeadZone, cfg => cfg.MapFrom(src => src.RightStick.ControlModeSettings.XAntiDeadZone))
                .ForPath(dest => dest.RSModInfo.XAxisDeadInfo.MaxOutput, cfg => cfg.MapFrom(src => src.RightStick.ControlModeSettings.XMaxOutput))
                .ForPath(dest => dest.RSModInfo.YAxisDeadInfo.DeadZone, cfg => cfg.MapFrom(src => src.RightStick.ControlModeSettings.YDeadZone))
                .ForPath(dest => dest.RSModInfo.YAxisDeadInfo.MaxZone, cfg => cfg.MapFrom(src => src.RightStick.ControlModeSettings.YMaxZone))
                .ForPath(dest => dest.RSModInfo.YAxisDeadInfo.AntiDeadZone, cfg => cfg.MapFrom(src => src.RightStick.ControlModeSettings.YAntiDeadZone))
                .ForPath(dest => dest.RSModInfo.YAxisDeadInfo.MaxOutput, cfg => cfg.MapFrom(src => src.RightStick.ControlModeSettings.YMaxOutput))
                .ForPath(dest => dest.RSOutputSettings.Mode, cfg => cfg.MapFrom(src => src.RightStick.OutputSettings));
            #endregion
        }
    }
}
