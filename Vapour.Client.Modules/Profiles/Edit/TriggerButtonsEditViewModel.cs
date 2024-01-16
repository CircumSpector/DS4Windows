using System.ComponentModel;
using System.Diagnostics;

using CommunityToolkit.Mvvm.Input;

using Vapour.Client.Core;
using Vapour.Client.Core.ViewModel;
using Vapour.Shared.Common;
using Vapour.Shared.Common.Types;

namespace Vapour.Client.Modules.Profiles.Edit;

public sealed partial class TriggerButtonsEditViewModel :
    ViewModel<ITriggerButtonsEditViewModel>,
    ITriggerButtonsEditViewModel
{
    private readonly IDeviceValueConverters _deviceValueConverters;

    private int _antiDeadZone;

    private BezierCurve _customCurve;

    private int _hipFireDelay;

    private int _maxOutput;

    private int _maxZone;

    private CurveMode _outputCurve;

    private double _sensitivity;

    private TriggerEffects _triggerEffect;

    private TwoStageTriggerMode _twoStageTriggerMode;

    public TriggerButtonsEditViewModel(IDeviceValueConverters deviceValueConverters)
    {
        _deviceValueConverters = deviceValueConverters;
    }

    public bool IsCustomCurveSelected => OutputCurve == CurveMode.Custom;

    public int DeadZone { get; set; }

    public double DeadZoneConverted
    {
        get => _deviceValueConverters.DeadZoneIntToDouble(DeadZone);
        set => DeadZone = _deviceValueConverters.DeadZoneDoubleToInt(value);
    }

    public int AntiDeadZone
    {
        get => _antiDeadZone;
        set => SetProperty(ref _antiDeadZone, value);
    }

    public int MaxZone
    {
        get => _maxZone;
        set => SetProperty(ref _maxZone, value);
    }

    public int MaxOutput
    {
        get => _maxOutput;
        set => SetProperty(ref _maxOutput, value);
    }

    public double Sensitivity
    {
        get => _sensitivity;
        set => SetProperty(ref _sensitivity, Math.Round(value, 1));
        //Math round needed because wpf slider control when binding to double and tick increment
        //of 0.1 when it hits values like 1.2 and 2.2 it sends 1.20000000002 or something like that
    }

    public int HipFireDelay
    {
        get => _hipFireDelay;
        set => SetProperty(ref _hipFireDelay, value);
    }

    public CurveMode OutputCurve
    {
        get => _outputCurve;
        set => SetProperty(ref _outputCurve, value);
    }

    public BezierCurve CustomCurve
    {
        get => _customCurve;
        set => SetProperty(ref _customCurve, value);
    }

    public TwoStageTriggerMode TwoStageTriggerMode
    {
        get => _twoStageTriggerMode;
        set => SetProperty(ref _twoStageTriggerMode, value);
    }

    public TriggerEffects TriggerEffect
    {
        get => _triggerEffect;
        set => SetProperty(ref _triggerEffect, value);
    }

    [RelayCommand]
    private void ShowCustomCurve()
    {
        ProcessStartInfo processStartInfo = new ProcessStartInfo(Constants.BezierCurveEditorPath);
        processStartInfo.UseShellExecute = true;
        Process.Start(processStartInfo);
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName == nameof(OutputCurve))
        {
            if (OutputCurve != CurveMode.Custom)
            {
                CustomCurve = null;
            }

            OnPropertyChanged(nameof(IsCustomCurveSelected));
        }
    }
}