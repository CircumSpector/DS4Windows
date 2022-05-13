using System.Windows.Media;
using AutoMapper;
using DS4Windows.Client.Modules.Profiles.Edit;
using DS4Windows.Shared.Configuration.Profiles.Schema;

namespace DS4Windows.Client.Modules.Profiles.Utils
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
                .ForPath(dest => dest.LeftStick.FlickRealWorldCalibtration, cfg => cfg.MapFrom(src => src.LSOutputSettings.OutputSettings.FlickSettings.RealWorldCalibration))
                .ForPath(dest => dest.LeftStick.FlickThreshold, cfg => cfg.MapFrom(src => src.LSOutputSettings.OutputSettings.FlickSettings.FlickThreshold))
                .ForPath(dest => dest.LeftStick.FlickTime, cfg => cfg.MapFrom(src => src.LSOutputSettings.OutputSettings.FlickSettings.FlickTime))
                .ForPath(dest => dest.LeftStick.FlickMinAngleThreshold, cfg => cfg.MapFrom(src => src.LSOutputSettings.OutputSettings.FlickSettings.MinAngleThreshold))
                .ForPath(dest => dest.LeftStick.ControlModeSettings.OutputCurve, cfg => cfg.MapFrom(src => src.LSOutCurveMode))
                .ForPath(dest => dest.LeftStick.ControlModeSettings.CustomCurve, cfg => cfg.MapFrom(src => src.LSOutCurve))
                .ForPath(dest => dest.LeftStick.ControlModeSettings.IsSquareStick, cfg => cfg.MapFrom(src => src.SquStickInfo.LSMode))
                .ForPath(dest => dest.LeftStick.ControlModeSettings.SquareStickRoundness, cfg => cfg.MapFrom(src => src.SquStickInfo.LSRoundness))
                .ForPath(dest => dest.LeftStick.ControlModeSettings.Rotation, cfg => cfg.MapFrom(src => src.LSRotation))
                .ForPath(dest => dest.LeftStick.ControlModeSettings.Fuzz, cfg => cfg.MapFrom(src => src.LSModInfo.Fuzz))
                .ForPath(dest => dest.LeftStick.ControlModeSettings.IsAntiSnapback, cfg => cfg.MapFrom(src => src.LSAntiSnapbackInfo.Enabled))
                .ForPath(dest => dest.LeftStick.ControlModeSettings.AntiSnapbackDelta, cfg => cfg.MapFrom(src => src.LSAntiSnapbackInfo.Delta))
                .ForPath(dest => dest.LeftStick.ControlModeSettings.AntiSnapbackTimeout, cfg => cfg.MapFrom(src => src.LSAntiSnapbackInfo.Timeout))

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
                .ForPath(dest => dest.RightStick.OutputSettings, cfg => cfg.MapFrom(src => src.RSOutputSettings.Mode))
                .ForPath(dest => dest.RightStick.FlickRealWorldCalibtration, cfg => cfg.MapFrom(src => src.RSOutputSettings.OutputSettings.FlickSettings.RealWorldCalibration))
                .ForPath(dest => dest.RightStick.FlickThreshold, cfg => cfg.MapFrom(src => src.RSOutputSettings.OutputSettings.FlickSettings.FlickThreshold))
                .ForPath(dest => dest.RightStick.FlickTime, cfg => cfg.MapFrom(src => src.RSOutputSettings.OutputSettings.FlickSettings.FlickTime))
                .ForPath(dest => dest.RightStick.FlickMinAngleThreshold, cfg => cfg.MapFrom(src => src.RSOutputSettings.OutputSettings.FlickSettings.MinAngleThreshold))
                .ForPath(dest => dest.RightStick.ControlModeSettings.OutputCurve, cfg => cfg.MapFrom(src => src.RSOutCurveMode))
                .ForPath(dest => dest.RightStick.ControlModeSettings.CustomCurve, cfg => cfg.MapFrom(src => src.RSOutCurve))
                .ForPath(dest => dest.RightStick.ControlModeSettings.IsSquareStick, cfg => cfg.MapFrom(src => src.SquStickInfo.RSMode))
                .ForPath(dest => dest.RightStick.ControlModeSettings.SquareStickRoundness, cfg => cfg.MapFrom(src => src.SquStickInfo.RSRoundness))
                .ForPath(dest => dest.RightStick.ControlModeSettings.Rotation, cfg => cfg.MapFrom(src => src.RSRotation))
                .ForPath(dest => dest.RightStick.ControlModeSettings.Fuzz, cfg => cfg.MapFrom(src => src.RSModInfo.Fuzz))
                .ForPath(dest => dest.RightStick.ControlModeSettings.IsAntiSnapback, cfg => cfg.MapFrom(src => src.RSAntiSnapbackInfo.Enabled))
                .ForPath(dest => dest.RightStick.ControlModeSettings.AntiSnapbackDelta, cfg => cfg.MapFrom(src => src.RSAntiSnapbackInfo.Delta))
                .ForPath(dest => dest.RightStick.ControlModeSettings.AntiSnapbackTimeout, cfg => cfg.MapFrom(src => src.RSAntiSnapbackInfo.Timeout))

            #endregion

            #region L2 Button
                .ForPath(dest => dest.L2Button.DeadZone, cfg => cfg.MapFrom(src => src.L2ModInfo.DeadZone))
                .ForPath(dest => dest.L2Button.AntiDeadZone, cfg => cfg.MapFrom(src => src.L2ModInfo.AntiDeadZone))
                .ForPath(dest => dest.L2Button.MaxZone, cfg => cfg.MapFrom(src => src.L2ModInfo.MaxZone))
                .ForPath(dest => dest.L2Button.MaxOutput, cfg => cfg.MapFrom(src => src.L2ModInfo.MaxOutput))
                .ForPath(dest => dest.L2Button.Sensitivity, cfg => cfg.MapFrom(src => src.L2Sens))
                .ForPath(dest => dest.L2Button.HipFireDelay, cfg => cfg.MapFrom(src => src.L2OutputSettings.HipFireMs))
                .ForPath(dest => dest.L2Button.OutputCurve, cfg => cfg.MapFrom(src => src.L2OutCurveMode))
                .ForPath(dest => dest.L2Button.CustomCurve, cfg => cfg.MapFrom(src => src.L2OutCurve))
                .ForPath(dest => dest.L2Button.TwoStageTriggerMode, cfg => cfg.MapFrom(src => src.L2OutputSettings.TwoStageMode))
                .ForPath(dest => dest.L2Button.TriggerEffect, cfg => cfg.MapFrom(src => src.L2OutputSettings.TriggerEffect))

            #endregion

            #region R2 Button

                .ForPath(dest => dest.R2Button.DeadZone, cfg => cfg.MapFrom(src => src.R2ModInfo.DeadZone))
                .ForPath(dest => dest.R2Button.AntiDeadZone, cfg => cfg.MapFrom(src => src.R2ModInfo.AntiDeadZone))
                .ForPath(dest => dest.R2Button.MaxZone, cfg => cfg.MapFrom(src => src.R2ModInfo.MaxZone))
                .ForPath(dest => dest.R2Button.MaxOutput, cfg => cfg.MapFrom(src => src.R2ModInfo.MaxOutput))
                .ForPath(dest => dest.R2Button.Sensitivity, cfg => cfg.MapFrom(src => src.R2Sens))
                .ForPath(dest => dest.R2Button.HipFireDelay, cfg => cfg.MapFrom(src => src.R2OutputSettings.HipFireMs))
                .ForPath(dest => dest.R2Button.OutputCurve, cfg => cfg.MapFrom(src => src.R2OutCurveMode))
                .ForPath(dest => dest.R2Button.CustomCurve, cfg => cfg.MapFrom(src => src.R2OutCurve))
                .ForPath(dest => dest.R2Button.TwoStageTriggerMode, cfg => cfg.MapFrom(src => src.R2OutputSettings.TwoStageMode))
                .ForPath(dest => dest.R2Button.TriggerEffect, cfg => cfg.MapFrom(src => src.R2OutputSettings.TriggerEffect))

            #endregion

            #region SixAxisX
                
                .ForPath(dest => dest.SixAxisX.DeadZone, cfg => cfg.MapFrom(src => src.SXDeadZone))
                .ForPath(dest => dest.SixAxisX.AntiDeadZone, cfg => cfg.MapFrom(src => src.SXAntiDeadZone))
                .ForPath(dest => dest.SixAxisX.MaxZone, cfg => cfg.MapFrom(src => src.SXMaxZone))
                .ForPath(dest => dest.SixAxisX.Sensitivity, cfg => cfg.MapFrom(src => src.SXSens))
                .ForPath(dest => dest.SixAxisX.OutputCurve, cfg => cfg.MapFrom(src => src.SXOutCurveMode))
                .ForPath(dest => dest.SixAxisX.CustomCurve, cfg => cfg.MapFrom(src => src.SXOutCurve))

            #endregion

            #region SixAxisZ

                .ForPath(dest => dest.SixAxisZ.DeadZone, cfg => cfg.MapFrom(src => src.SZDeadZone))
                .ForPath(dest => dest.SixAxisZ.AntiDeadZone, cfg => cfg.MapFrom(src => src.SZAntiDeadZone))
                .ForPath(dest => dest.SixAxisZ.MaxZone, cfg => cfg.MapFrom(src => src.SZMaxZone))
                .ForPath(dest => dest.SixAxisZ.Sensitivity, cfg => cfg.MapFrom(src => src.SZSens))
                .ForPath(dest => dest.SixAxisZ.OutputCurve, cfg => cfg.MapFrom(src => src.SZOutCurveMode))
                .ForPath(dest => dest.SixAxisZ.CustomCurve, cfg => cfg.MapFrom(src => src.SZOutCurve));

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
                .ForPath(dest => dest.LSOutputSettings.OutputSettings.FlickSettings.RealWorldCalibration, cfg => cfg.MapFrom(src => src.LeftStick.FlickRealWorldCalibtration))
                .ForPath(dest => dest.LSOutputSettings.OutputSettings.FlickSettings.FlickThreshold, cfg => cfg.MapFrom(src => src.LeftStick.FlickThreshold))
                .ForPath(dest => dest.LSOutputSettings.OutputSettings.FlickSettings.FlickTime, cfg => cfg.MapFrom(src => src.LeftStick.FlickTime))
                .ForPath(dest => dest.LSOutputSettings.OutputSettings.FlickSettings.MinAngleThreshold, cfg => cfg.MapFrom(src => src.LeftStick.FlickMinAngleThreshold))
                .ForPath(dest => dest.LSOutCurveMode, cfg => cfg.MapFrom(src => src.LeftStick.ControlModeSettings.OutputCurve))
                .ForPath(dest => dest.LSOutCurve, cfg => cfg.MapFrom(src => src.LeftStick.ControlModeSettings.CustomCurve))
                .ForPath(dest => dest.SquStickInfo.LSMode, cfg => cfg.MapFrom(src => src.LeftStick.ControlModeSettings.IsSquareStick))
                .ForPath(dest => dest.SquStickInfo.LSRoundness, cfg => cfg.MapFrom(src => src.LeftStick.ControlModeSettings.SquareStickRoundness))
                .ForPath(dest => dest.LSRotation, cfg => cfg.MapFrom(src => src.LeftStick.ControlModeSettings.Rotation))
                .ForPath(dest => dest.LSModInfo.Fuzz, cfg => cfg.MapFrom(src => src.LeftStick.ControlModeSettings.Fuzz))
                .ForPath(dest => dest.LSAntiSnapbackInfo.Enabled, cfg => cfg.MapFrom(src => src.LeftStick.ControlModeSettings.IsAntiSnapback))
                .ForPath(dest => dest.LSAntiSnapbackInfo.Delta, cfg => cfg.MapFrom(src => src.LeftStick.ControlModeSettings.AntiSnapbackDelta))
                .ForPath(dest => dest.LSAntiSnapbackInfo.Timeout, cfg => cfg.MapFrom(src => src.LeftStick.ControlModeSettings.AntiSnapbackTimeout))

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
                .ForPath(dest => dest.RSOutputSettings.Mode, cfg => cfg.MapFrom(src => src.RightStick.OutputSettings))
                .ForPath(dest => dest.RSOutputSettings.OutputSettings.FlickSettings.RealWorldCalibration, cfg => cfg.MapFrom(src => src.RightStick.FlickRealWorldCalibtration))
                .ForPath(dest => dest.RSOutputSettings.OutputSettings.FlickSettings.FlickThreshold, cfg => cfg.MapFrom(src => src.RightStick.FlickThreshold))
                .ForPath(dest => dest.RSOutputSettings.OutputSettings.FlickSettings.FlickTime, cfg => cfg.MapFrom(src => src.RightStick.FlickTime))
                .ForPath(dest => dest.RSOutputSettings.OutputSettings.FlickSettings.MinAngleThreshold, cfg => cfg.MapFrom(src => src.RightStick.FlickMinAngleThreshold))
                .ForPath(dest => dest.RSOutCurveMode, cfg => cfg.MapFrom(src => src.RightStick.ControlModeSettings.OutputCurve))
                .ForPath(dest => dest.RSOutCurve, cfg => cfg.MapFrom(src => src.RightStick.ControlModeSettings.CustomCurve))
                .ForPath(dest => dest.SquStickInfo.RSMode, cfg => cfg.MapFrom(src => src.RightStick.ControlModeSettings.IsSquareStick))
                .ForPath(dest => dest.SquStickInfo.RSRoundness, cfg => cfg.MapFrom(src => src.RightStick.ControlModeSettings.SquareStickRoundness))
                .ForPath(dest => dest.RSRotation, cfg => cfg.MapFrom(src => src.RightStick.ControlModeSettings.Rotation))
                .ForPath(dest => dest.RSModInfo.Fuzz, cfg => cfg.MapFrom(src => src.RightStick.ControlModeSettings.Fuzz))
                .ForPath(dest => dest.RSAntiSnapbackInfo.Enabled, cfg => cfg.MapFrom(src => src.RightStick.ControlModeSettings.IsAntiSnapback))
                .ForPath(dest => dest.RSAntiSnapbackInfo.Delta, cfg => cfg.MapFrom(src => src.RightStick.ControlModeSettings.AntiSnapbackDelta))
                .ForPath(dest => dest.RSAntiSnapbackInfo.Timeout, cfg => cfg.MapFrom(src => src.RightStick.ControlModeSettings.AntiSnapbackTimeout))

            #endregion

            #region L2 Button

                .ForPath(dest => dest.L2ModInfo.DeadZone, cfg => cfg.MapFrom(src => src.L2Button.DeadZone))
                .ForPath(dest => dest.L2ModInfo.AntiDeadZone, cfg => cfg.MapFrom(src => src.L2Button.AntiDeadZone))
                .ForPath(dest => dest.L2ModInfo.MaxZone, cfg => cfg.MapFrom(src => src.L2Button.MaxZone))
                .ForPath(dest => dest.L2ModInfo.MaxOutput, cfg => cfg.MapFrom(src => src.L2Button.MaxOutput))
                .ForPath(dest => dest.L2Sens, cfg => cfg.MapFrom(src => src.L2Button.Sensitivity))
                .ForPath(dest => dest.L2OutputSettings.HipFireMs, cfg => cfg.MapFrom(src => src.L2Button.HipFireDelay))
                .ForPath(dest => dest.L2OutCurveMode, cfg => cfg.MapFrom(src => src.L2Button.OutputCurve))
                .ForPath(dest => dest.L2OutCurve, cfg => cfg.MapFrom(src => src.L2Button.CustomCurve))
                .ForPath(dest => dest.L2OutputSettings.TwoStageMode, cfg => cfg.MapFrom(src => src.L2Button.TwoStageTriggerMode))
                .ForPath(dest => dest.L2OutputSettings.TriggerEffect, cfg => cfg.MapFrom(src => src.L2Button.TriggerEffect))

            #endregion

            #region R2 Button

                .ForPath(dest => dest.R2ModInfo.DeadZone, cfg => cfg.MapFrom(src => src.R2Button.DeadZone))
                .ForPath(dest => dest.R2ModInfo.AntiDeadZone, cfg => cfg.MapFrom(src => src.R2Button.AntiDeadZone))
                .ForPath(dest => dest.R2ModInfo.MaxZone, cfg => cfg.MapFrom(src => src.R2Button.MaxZone))
                .ForPath(dest => dest.R2ModInfo.MaxOutput, cfg => cfg.MapFrom(src => src.R2Button.MaxOutput))
                .ForPath(dest => dest.R2Sens, cfg => cfg.MapFrom(src => src.R2Button.Sensitivity))
                .ForPath(dest => dest.R2OutputSettings.HipFireMs, cfg => cfg.MapFrom(src => src.R2Button.HipFireDelay))
                .ForPath(dest => dest.R2OutCurveMode, cfg => cfg.MapFrom(src => src.R2Button.OutputCurve))
                .ForPath(dest => dest.R2OutCurve, cfg => cfg.MapFrom(src => src.R2Button.CustomCurve))
                .ForPath(dest => dest.R2OutputSettings.TwoStageMode, cfg => cfg.MapFrom(src => src.R2Button.TwoStageTriggerMode))
                .ForPath(dest => dest.R2OutputSettings.TriggerEffect, cfg => cfg.MapFrom(src => src.R2Button.TriggerEffect))

            #endregion

            #region SixAxisX
                
                .ForPath(dest => dest.SXDeadZone, cfg => cfg.MapFrom(src => src.SixAxisX.DeadZone))
                .ForPath(dest => dest.SXAntiDeadZone, cfg => cfg.MapFrom(src => src.SixAxisX.AntiDeadZone))
                .ForPath(dest => dest.SXMaxZone, cfg => cfg.MapFrom(src => src.SixAxisX.MaxZone))
                .ForPath(dest => dest.SXSens, cfg => cfg.MapFrom(src => src.SixAxisX.Sensitivity))
                .ForPath(dest => dest.SXOutCurveMode, cfg => cfg.MapFrom(src => src.SixAxisX.OutputCurve))
                .ForPath(dest => dest.SXOutCurve, cfg => cfg.MapFrom(src => src.SixAxisX.CustomCurve))

            #endregion

            #region SixAxisZ

                .ForPath(dest => dest.SZDeadZone, cfg => cfg.MapFrom(src => src.SixAxisZ.DeadZone))
                .ForPath(dest => dest.SZAntiDeadZone, cfg => cfg.MapFrom(src => src.SixAxisZ.AntiDeadZone))
                .ForPath(dest => dest.SZMaxZone, cfg => cfg.MapFrom(src => src.SixAxisZ.MaxZone))
                .ForPath(dest => dest.SZSens, cfg => cfg.MapFrom(src => src.SixAxisZ.Sensitivity))
                .ForPath(dest => dest.SZOutCurveMode, cfg => cfg.MapFrom(src => src.SixAxisZ.OutputCurve))
                .ForPath(dest => dest.SZOutCurve, cfg => cfg.MapFrom(src => src.SixAxisZ.CustomCurve));

            #endregion
        }
    }
}
