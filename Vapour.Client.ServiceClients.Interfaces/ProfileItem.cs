using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Vapour.Shared.Common.Legacy;
using Vapour.Shared.Common.Types;
using Vapour.Shared.Configuration.Profiles.Schema;

namespace Vapour.Client.ServiceClients
{
    public class ProfileItem : IProfile
    {
        public int BluetoothPollRate { get; set; }
        public ButtonMouseInfo ButtonMouseInfo { get; set; }
        public PhysicalAddress DeviceId { get; set; }
        public bool DisableVirtualController { get; set; }
        public string DisplayName { get; set; }
        public bool DoubleTap { get; set; }
        public bool Ds4Mapping { get; set; }
        public bool EnableOutputDataToDS4 { get; set; }
        public bool EnableTouchToggle { get; set; }
        public string ExtendedDisplayName { get; set; }
        public string FileName { get; set; }
        public GyroControlsInfo GyroControlsInfo { get; set; }
        public int GyroInvert { get; set; }
        public int GyroMouseHorizontalAxis { get; set; }
        public GyroMouseInfo GyroMouseInfo { get; set; }
        public int GyroMouseStickHorizontalAxis { get; set; }
        public GyroMouseStickInfo GyroMouseStickInfo { get; set; }
        public bool GyroMouseStickToggle { get; set; }
        public bool GyroMouseStickTriggerTurns { get; set; }
        public bool GyroMouseToggle { get; set; }
        public GyroOutMode GyroOutputMode { get; set; }
        public int GyroSensitivity { get; set; }
        public int GyroSensVerticalScale { get; set; }
        public GyroDirectionalSwipeInfo GyroSwipeInfo { get; set; }
        public bool GyroTriggerTurns { get; set; }
        public Guid Id { get; set; }
        public int IdleDisconnectTimeout { get; set; }
        public int? Index { get; set; }
        public bool IsDefaultProfile { get; set; }
        public bool IsImmutable { get; set; }
        public bool IsLinkedProfile { get; set; }
        public bool IsOutputDeviceEnabled { get; set; }
        public TriggerDeadZoneZInfo L2ModInfo { get; set; }
        public BezierCurve L2OutCurve { get; set; }
        public CurveMode L2OutCurveMode { get; set; }
        public TriggerOutputSettings L2OutputSettings { get; set; }
        public double L2Sens { get; set; }
        public string LaunchProgram { get; set; }
        public LightbarSettingInfo LightbarSettingInfo { get; set; }
        public bool LowerRCOn { get; set; }
        public StickAntiSnapbackInfo LSAntiSnapbackInfo { get; set; }
        public StickDeadZoneInfo LSModInfo { get; set; }
        public BezierCurve LSOutCurve { get; set; }
        public CurveMode LSOutCurveMode { get; set; }
        public StickOutputSetting LSOutputSettings { get; set; }
        public double LSRotation { get; set; }
        public double LSSens { get; set; }
        public OutputDeviceType OutputDeviceType { get; set; }
        public ControlSettingsGroup PerControlSettings { get; set; } = new(
        (from DS4ControlItem dc in Enum.GetValues(typeof(DS4ControlItem))
        where dc != DS4ControlItem.None
            select new DS4ControlSettingsV3(dc)).ToList());
        public TriggerDeadZoneZInfo R2ModInfo { get; set; }
        public BezierCurve R2OutCurve { get; set; }
        public CurveMode R2OutCurveMode { get; set; }
        public TriggerOutputSettings R2OutputSettings { get; set; }
        public double R2Sens { get; set; }
        public StickAntiSnapbackInfo RSAntiSnapbackInfo { get; set; }
        public StickDeadZoneInfo RSModInfo { get; set; }
        public BezierCurve RSOutCurve { get; set; }
        public CurveMode RSOutCurveMode { get; set; }
        public StickOutputSetting RSOutputSettings { get; set; }
        public double RSRotation { get; set; }
        public double RSSens { get; set; }
        public int RumbleAutostopTime { get; set; }
        public byte RumbleBoost { get; set; }
        public bool SAMouseStickTriggerCond { get; set; }
        public string SAMouseStickTriggers { get; set; }
        public SASteeringWheelEmulationAxisType SASteeringWheelEmulationAxis { get; set; }
        public int SASteeringWheelEmulationRange { get; set; }
        public bool SATriggerCondition { get; set; }
        public string SATriggers { get; set; }
        public int SAWheelFuzzValues { get; set; }
        public int ScrollSensitivity { get; set; }
        public SquareStickInfo SquStickInfo { get; set; }
        public bool StartTouchpadOff { get; set; }
        public double SXAntiDeadZone { get; set; }
        public double SXDeadZone { get; set; }
        public double SXMaxZone { get; set; }
        public BezierCurve SXOutCurve { get; set; }
        public CurveMode SXOutCurveMode { get; set; }
        public double SXSens { get; set; }
        public double SZAntiDeadZone { get; set; }
        public double SZDeadZone { get; set; }
        public double SZMaxZone { get; set; }
        public BezierCurve SZOutCurve { get; set; }
        public CurveMode SZOutCurveMode { get; set; }
        public double SZSens { get; set; }
        public byte TapSensitivity { get; set; }
        public bool TouchClickPassthru { get; set; }
        public IList<int> TouchDisInvertTriggers { get; set; }
        public TouchpadOutMode TouchOutMode { get; set; }
        public TouchpadAbsMouseSettings TouchPadAbsMouse { get; set; }
        public int TouchPadInvert { get; set; }
        public bool TouchpadJitterCompensation { get; set; }
        public TouchPadRelMouseSettings TouchPadRelMouse { get; set; }
        public byte TouchSensitivity { get; set; }
        public double TrackballFriction { get; set; }
        public bool TrackballMode { get; set; }
        public SteeringWheelSmoothingInfo WheelSmoothInfo { get; set; }
        public event IProfile.ProfilePropertyChangedEventHandler ProfilePropertyChanged;
        public string GetAbsoluteFilePath(string parentDirectory)
        {
            throw new NotImplementedException();
        }

        public void OnPropertyChanged(string propertyName, object before, object after)
        {
            throw new NotImplementedException();
        }

        public IProfile WithChangeNotification(IProfile.ProfilePropertyChangedEventHandler handler)
        {
            throw new NotImplementedException();
        }

        public void Serialize(Stream stream)
        {
            throw new NotImplementedException();
        }

        public Task SerializeAsync(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
