﻿using Vapour.Client.Core.ViewModel;
using Vapour.Shared.Common.Types;

namespace Vapour.Client.Modules.Profiles.Edit;

public interface IStickControlModeSettingsViewModel : IViewModel<IStickControlModeSettingsViewModel>
{
    int DeadZone { get; set; }
    int AntiDeadZone { get; set; }
    int MaxZone { get; set; }
    int MaxOutput { get; set; }
    StickDeadZoneInfo.DeadZoneType DeadZoneType { get; set; }
    double Sensitivity { get; set; }
    int VerticalScale { get; set; }
    int XDeadZone { get; set; }
    int XMaxZone { get; set; }
    int XAntiDeadZone { get; set; }
    int XMaxOutput { get; set; }
    int YDeadZone { get; set; }
    int YMaxZone { get; set; }
    int YAntiDeadZone { get; set; }
    int YMaxOutput { get; set; }
    bool ForceMaxOutput { get; set; }
    CurveMode OutputCurve { get; set; }
    BezierCurve CustomCurve { get; set; }
    double SquareStickRoundness { get; set; }
    bool IsSquareStick { get; set; }
    double Rotation { get; set; }
    int AntiSnapbackTimeout { get; set; }
    int AntiSnapbackDelta { get; set; }
    bool IsAntiSnapback { get; set; }
    int Fuzz { get; set; }
}