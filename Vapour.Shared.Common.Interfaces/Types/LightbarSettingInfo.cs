

using System.Diagnostics.CodeAnalysis;

using PropertyChanged;

namespace Vapour.Shared.Common.Types;

/// <summary>
///     Lightbar behaviour settings.
/// </summary>
[AddINotifyPropertyChangedInterface]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class LightbarSettingInfo
{
    public LightbarMode Mode { get; set; } = LightbarMode.DS4Win;

    public LightbarDS4WinInfo Ds4WinSettings { get; set; } = new();

    public event EventHandler ModeChanged;

    private void OnModeChanged()
    {
        ModeChanged?.Invoke(this, EventArgs.Empty);
    }
}