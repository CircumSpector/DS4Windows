﻿using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Text.Json.Serialization;

using JetBrains.Annotations;

using Vapour.Shared.Common.Legacy;
using Vapour.Shared.Common.Types;
using Vapour.Shared.Configuration.Profiles.Types;

namespace Vapour.Shared.Configuration.Profiles.Schema;

/// <summary>
///     Controller profile definition.
/// </summary>
public class VapourProfile : 
    IEquatable<VapourProfile>,
    INotifyPropertyChanged, 
    IProfile
{
    public delegate void ProfilePropertyChangedEventHandler([CanBeNull] object sender,
        ProfilePropertyChangedEventArgs e);

    /// <summary>
    ///     Default file extension for persisted profile files.
    /// </summary>
    public const string FileExtension = ".json";

    /// <summary>
    ///     Create new <see cref="VapourProfile" />. Constructor must remain public for deep cloning to work.
    /// </summary>
    public VapourProfile()
    {
    }

    protected VapourProfile(int index) : this()
    {
        Index = index;
    }

    #region Non-persisted properties

    /// <summary>
    ///     Sanitized XML file name derived from <see cref="DisplayName" />.
    /// </summary>
    [JsonIgnore]
    public string FileName => GetValidFileName(DisplayName);

    /// <summary>
    ///     The controller slot index this profile is loaded, if applicable. Useful to speed up lookup.
    /// </summary>
    /// <remarks>This value is assigned at runtime and not persisted.</remarks>
    [JsonIgnore]
    public int? Index { get; set; }

    /// <summary>
    ///     The controller ID this profile is currently attached to.
    /// </summary>
    /// <remarks>This value is assigned at runtime and not persisted.</remarks>
    [JsonIgnore]
    public PhysicalAddress DeviceId { get; set; }

    /// <summary>
    ///     If true, is the default profile. There can only be one.
    /// </summary>
    [JsonIgnore]
    public bool IsDefaultProfile => Equals(Id, Constants.DefaultProfileId);

    /// <summary>
    ///     If true, this profile is linked to the current slots device' MAC/ID.
    /// </summary>
    /// <remarks>This value is assigned at runtime and not persisted.</remarks>
    [JsonIgnore]
    public bool IsLinkedProfile { get; set; }

    /// <summary>
    ///     State information if an output device is active.
    /// </summary>
    /// <remarks>This value is assigned at runtime and not persisted.</remarks>
    [JsonIgnore]
    public bool IsOutputDeviceEnabled { get; set; }

    /// <summary>
    ///     Dynamically built display name (for use in UI).
    /// </summary>
    [UsedImplicitly]
    [JsonIgnore]
    public string ExtendedDisplayName =>
        IsImmutable
            ? $"{DisplayName} (r/o)"
            : DisplayName;

    #endregion

    #region Persisted properties

    /// <summary>
    ///     Auto-generated unique ID for this profile.
    /// </summary>
    [JsonInclude]
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>
    ///     Friendly, user-changeable name of this profile.
    /// </summary>
    public string DisplayName { get; set; } = "Default";

    /// <summary>
    ///     If set, prohibits editing this profile via UI.
    /// </summary>
    public bool IsImmutable { get; set; }

    public bool EnableTouchToggle { get; set; } = true;

    public ButtonMouseInfo ButtonMouseInfo { get; set; } = new();

    public GyroControlsInfo GyroControlsInfo { get; set; } = new();

    public int IdleDisconnectTimeout { get; set; } = 0;

    public bool EnableOutputDataToDS4 { get; set; } = true;

    public bool TouchpadJitterCompensation { get; set; } = true;

    public bool LowerRCOn { get; set; } = false;

    public bool TouchClickPassthru { get; set; } = false;

    public byte RumbleBoost { get; set; } = 100;

    public int RumbleAutostopTime { get; set; } = 0;

    public byte TouchSensitivity { get; set; } = 100;

    public StickDeadZoneInfo LSModInfo { get; set; } = new();

    public StickDeadZoneInfo RSModInfo { get; set; } = new();

    public TriggerDeadZoneZInfo L2ModInfo { get; set; } = new();

    public TriggerDeadZoneZInfo R2ModInfo { get; set; } = new();

    public double LSRotation { get; set; } = 0.0;

    public double RSRotation { get; set; } = 0.0;

    public double SXDeadZone { get; set; } = 0.25;

    public double SZDeadZone { get; set; } = 0.25;

    public double SXMaxZone { get; set; } = 1.0;

    public double SZMaxZone { get; set; } = 1.0;

    public double SXAntiDeadZone { get; set; } = 0.0;

    public double SZAntiDeadZone { get; set; } = 0.0;

    public double L2Sens { get; set; } = 1;

    public double R2Sens { get; set; } = 1;

    public double LSSens { get; set; } = 1;

    public double RSSens { get; set; } = 1;

    public double SXSens { get; set; } = 1;

    public double SZSens { get; set; } = 1;

    public byte TapSensitivity { get; set; } = 0;

    public bool DoubleTap { get; set; } = false;

    public int ScrollSensitivity { get; set; } = 0;

    public int TouchPadInvert { get; set; } = 0;

    public int BluetoothPollRate { get; set; } = 4;

    public StickOutputSetting LSOutputSettings { get; set; } = new();

    public StickOutputSetting RSOutputSettings { get; set; } = new();

    public TriggerOutputSettings L2OutputSettings { get; set; } = new();

    public TriggerOutputSettings R2OutputSettings { get; set; } = new();

    public string LaunchProgram { get; set; }

    public bool DisableVirtualController { get; set; } = false;

    public bool StartTouchpadOff { get; set; } = false;

    public TouchpadOutMode TouchOutMode { get; set; } = TouchpadOutMode.Mouse;

    public string SATriggers { get; set; } = "-1";

    public bool SATriggerCondition { get; set; } = true;

    public GyroOutMode GyroOutputMode { get; set; } = GyroOutMode.Controls;

    public string SAMouseStickTriggers { get; set; } = "-1";

    public bool SAMouseStickTriggerCond { get; set; } = true;

    public GyroMouseStickInfo GyroMouseStickInfo { get; set; } = new();

    public GyroDirectionalSwipeInfo GyroSwipeInfo { get; set; } = new();

    public bool GyroMouseStickToggle { get; set; } = false;

    public bool GyroMouseStickTriggerTurns { get; set; } = true;

    public int GyroMouseStickHorizontalAxis { get; set; }

    public SASteeringWheelEmulationAxisType SASteeringWheelEmulationAxis { get; set; } =
        SASteeringWheelEmulationAxisType.None;

    public int SASteeringWheelEmulationRange { get; set; } = 360;

    public int SAWheelFuzzValues { get; set; } = 0;

    public SteeringWheelSmoothingInfo WheelSmoothInfo { get; set; } = new();

    public IList<int> TouchDisInvertTriggers { get; set; } = new List<int> { -1 };

    public int GyroSensitivity { get; set; } = 100;

    public int GyroSensVerticalScale { get; set; } = 100;

    public int GyroInvert { get; set; } = 0;

    public bool GyroTriggerTurns { get; set; } = true;

    public GyroMouseInfo GyroMouseInfo { get; set; } = new();

    public int GyroMouseHorizontalAxis { get; set; } = 0;

    public bool GyroMouseToggle { get; set; } = false;

    public SquareStickInfo SquStickInfo { get; set; } = new();

    public StickAntiSnapbackInfo LSAntiSnapbackInfo { get; set; } = new();

    public StickAntiSnapbackInfo RSAntiSnapbackInfo { get; set; } = new();

    public CurveMode LSOutCurveMode { get; set; } = new();

    public BezierCurve LSOutCurve { get; set; } = new();

    public CurveMode RSOutCurveMode { get; set; } = new();

    public BezierCurve RSOutCurve { get; set; } = new();

    public CurveMode L2OutCurveMode { get; set; } = new();

    public BezierCurve L2OutCurve { get; set; } = new();

    public CurveMode R2OutCurveMode { get; set; } = new();

    public BezierCurve R2OutCurve { get; set; } = new();

    public CurveMode SXOutCurveMode { get; set; } = new();

    public BezierCurve SXOutCurve { get; set; } = new();

    public CurveMode SZOutCurveMode { get; set; } = new();

    public BezierCurve SZOutCurve { get; set; } = new();

    public bool TrackballMode { get; set; } = false;

    public double TrackballFriction { get; set; } = 10.0;

    public TouchpadAbsMouseSettings TouchPadAbsMouse { get; set; } = new();

    public TouchPadRelMouseSettings TouchPadRelMouse { get; set; } = new();

    public OutputDeviceType OutputDeviceType { get; set; } = OutputDeviceType.Xbox360Controller;

    public bool Ds4Mapping { get; set; } = false;

    public LightbarSettingInfo LightbarSettingInfo { get; set; } = new();

    /// <summary>
    ///     Collection of <see cref="SpecialAction"/>s.
    /// </summary>
    // TODO: implement me!
    //public IList<SpecialAction> SpecialActions { get; set; } = new List<SpecialAction>();

    /// <summary>
    ///     Collection of <see cref="DS4ControlSettingsV3"/>.
    /// </summary>
    public ControlSettingsGroup PerControlSettings { get; set; } = new(
        (from DS4ControlItem dc in Enum.GetValues(typeof(DS4ControlItem))
         where dc != DS4ControlItem.None
         select new DS4ControlSettingsV3(dc)).ToList());

    #endregion

    public bool Equals(VapourProfile other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id);
    }

    [CanBeNull] public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    ///     Returns a file name from a friendly profile display name.
    /// </summary>
    /// <param name="profileName">The profile name.</param>
    /// <returns>The file system file name with extension.</returns>
    public static string GetValidFileName(string profileName)
    {
        //
        // Strip extension, if included in name
        // 
        if (profileName.EndsWith(FileExtension, StringComparison.OrdinalIgnoreCase))
            profileName = profileName.Remove(profileName.LastIndexOf(FileExtension,
                StringComparison.OrdinalIgnoreCase));

        //
        // Strip invalid characters
        // 
        profileName = new string(profileName.Where(m => !Path.GetInvalidFileNameChars().Contains(m)).ToArray());

        //
        // Add extension
        // 
        return $"{profileName}{FileExtension}";
    }

    /// <summary>
    ///     Returns a new copy of the default <see cref="VapourProfile"/> with default settings.
    /// </summary>
    /// <param name="index">An optional slot index.</param>
    /// <returns>The new <see cref="VapourProfile"/>.</returns>
    public static VapourProfile CreateDefaultProfile(int index = default)
    {
        return new VapourProfile(index)
        {
            Id = Constants.DefaultProfileId
        };
    }

    /// <summary>
    ///     Returns a new unique <see cref="VapourProfile"/> with default settings.
    /// </summary>
    /// <param name="index">An optional slot index.</param>
    /// <returns>The new <see cref="VapourProfile"/>.</returns>
    public static VapourProfile CreateNewProfile(int index = default)
    {
        return new VapourProfile(index);
    }

    [CanBeNull] public event IProfile.ProfilePropertyChangedEventHandler ProfilePropertyChanged;

    [UsedImplicitly]
    public void OnPropertyChanged(string propertyName, object before, object after)
    {
        ProfilePropertyChanged?.Invoke(this, new ProfilePropertyChangedEventArgs(propertyName, before, after));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    ///     Register a <see cref="ProfilePropertyChangedEventHandler"/> for this <see cref="VapourProfile"/>.
    /// </summary>
    /// <param name="handler">The <see cref="ProfilePropertyChangedEventHandler"/>.</param>
    /// <returns>This instance of <see cref="VapourProfile"/>.</returns>
    public IProfile WithChangeNotification([CanBeNull] IProfile.ProfilePropertyChangedEventHandler handler)
    {
        ProfilePropertyChanged += (sender, args) => { handler?.Invoke(sender, args); };

        return this;
    }

    /// <summary>
    ///     Builds an absolute file system path to this <see cref="VapourProfile"/>.
    /// </summary>
    /// <param name="parentDirectory">The parent directory.</param>
    /// <returns>The resulting absolute path.</returns>
    public string GetAbsoluteFilePath(string parentDirectory)
    {
        return Path.Combine(parentDirectory, FileName);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((VapourProfile)obj);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public override string ToString()
    {
        return $"{DisplayName} ({Id})";
    }
}