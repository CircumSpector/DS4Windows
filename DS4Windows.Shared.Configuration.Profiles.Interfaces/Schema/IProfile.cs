using System.IO;
using System.Net.NetworkInformation;
using DS4Windows.Shared.Common.Legacy;
using DS4Windows.Shared.Common.Types;
using DS4Windows.Shared.Configuration.Profiles.Types;

namespace DS4Windows.Shared.Configuration.Profiles.Schema;

public interface IProfile
{
    delegate void ProfilePropertyChangedEventHandler(object sender,
        ProfilePropertyChangedEventArgs e);

    int BluetoothPollRate { get; set; }
    ButtonMouseInfo ButtonMouseInfo { get; set; }
    PhysicalAddress DeviceId { get; set; }
    bool DisableVirtualController { get; set; }
    string DisplayName { get; set; }
    bool DoubleTap { get; set; }
    bool Ds4Mapping { get; set; }
    bool EnableOutputDataToDS4 { get; set; }
    bool EnableTouchToggle { get; set; }
    string ExtendedDisplayName { get; }
    string FileName { get; }
    GyroControlsInfo GyroControlsInfo { get; set; }
    int GyroInvert { get; set; }
    int GyroMouseHorizontalAxis { get; set; }
    GyroMouseInfo GyroMouseInfo { get; set; }
    int GyroMouseStickHorizontalAxis { get; set; }
    GyroMouseStickInfo GyroMouseStickInfo { get; set; }
    bool GyroMouseStickToggle { get; set; }
    bool GyroMouseStickTriggerTurns { get; set; }
    bool GyroMouseToggle { get; set; }
    GyroOutMode GyroOutputMode { get; set; }
    int GyroSensitivity { get; set; }
    int GyroSensVerticalScale { get; set; }
    GyroDirectionalSwipeInfo GyroSwipeInfo { get; set; }
    bool GyroTriggerTurns { get; set; }
    Guid Id { get; }
    int IdleDisconnectTimeout { get; set; }
    int? Index { get; set; }
    bool IsDefaultProfile { get; }
    bool IsImmutable { get; set; }
    bool IsLinkedProfile { get; set; }
    bool IsOutputDeviceEnabled { get; set; }
    TriggerDeadZoneZInfo L2ModInfo { get; set; }
    BezierCurve L2OutCurve { get; set; }
    CurveMode L2OutCurveMode { get; set; }
    TriggerOutputSettings L2OutputSettings { get; set; }
    double L2Sens { get; set; }
    string LaunchProgram { get; set; }
    LightbarSettingInfo LightbarSettingInfo { get; set; }
    bool LowerRCOn { get; set; }
    StickAntiSnapbackInfo LSAntiSnapbackInfo { get; set; }
    StickDeadZoneInfo LSModInfo { get; set; }
    BezierCurve LSOutCurve { get; set; }
    CurveMode LSOutCurveMode { get; set; }
    StickOutputSetting LSOutputSettings { get; set; }
    double LSRotation { get; set; }
    double LSSens { get; set; }
    OutputDeviceType OutputDeviceType { get; set; }
    ControlSettingsGroup PerControlSettings { get; set; }
    TriggerDeadZoneZInfo R2ModInfo { get; set; }
    BezierCurve R2OutCurve { get; set; }
    CurveMode R2OutCurveMode { get; set; }
    TriggerOutputSettings R2OutputSettings { get; set; }
    double R2Sens { get; set; }
    StickAntiSnapbackInfo RSAntiSnapbackInfo { get; set; }
    StickDeadZoneInfo RSModInfo { get; set; }
    BezierCurve RSOutCurve { get; set; }
    CurveMode RSOutCurveMode { get; set; }
    StickOutputSetting RSOutputSettings { get; set; }
    double RSRotation { get; set; }
    double RSSens { get; set; }
    int RumbleAutostopTime { get; set; }
    byte RumbleBoost { get; set; }
    bool SAMouseStickTriggerCond { get; set; }
    string SAMouseStickTriggers { get; set; }
    SASteeringWheelEmulationAxisType SASteeringWheelEmulationAxis { get; set; }
    int SASteeringWheelEmulationRange { get; set; }
    bool SATriggerCondition { get; set; }
    string SATriggers { get; set; }
    int SAWheelFuzzValues { get; set; }
    int ScrollSensitivity { get; set; }
    SquareStickInfo SquStickInfo { get; set; }
    bool StartTouchpadOff { get; set; }
    double SXAntiDeadZone { get; set; }
    double SXDeadZone { get; set; }
    double SXMaxZone { get; set; }
    BezierCurve SXOutCurve { get; set; }
    CurveMode SXOutCurveMode { get; set; }
    double SXSens { get; set; }
    double SZAntiDeadZone { get; set; }
    double SZDeadZone { get; set; }
    double SZMaxZone { get; set; }
    BezierCurve SZOutCurve { get; set; }
    CurveMode SZOutCurveMode { get; set; }
    double SZSens { get; set; }
    byte TapSensitivity { get; set; }
    bool TouchClickPassthru { get; set; }
    IList<int> TouchDisInvertTriggers { get; set; }
    TouchpadOutMode TouchOutMode { get; set; }
    TouchpadAbsMouseSettings TouchPadAbsMouse { get; set; }
    int TouchPadInvert { get; set; }
    bool TouchpadJitterCompensation { get; set; }
    TouchPadRelMouseSettings TouchPadRelMouse { get; set; }
    byte TouchSensitivity { get; set; }
    double TrackballFriction { get; set; }
    bool TrackballMode { get; set; }
    SteeringWheelSmoothingInfo WheelSmoothInfo { get; set; }

    event ProfilePropertyChangedEventHandler ProfilePropertyChanged;

    bool Equals(object obj);
    string GetAbsoluteFilePath(string parentDirectory);
    int GetHashCode();
    void OnPropertyChanged(string propertyName, object before, object after);
    string ToString();
    IProfile WithChangeNotification(ProfilePropertyChangedEventHandler handler);

    void Serialize(Stream stream);
}