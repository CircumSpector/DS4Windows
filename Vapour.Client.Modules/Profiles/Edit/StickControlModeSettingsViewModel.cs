using System.ComponentModel;
using System.Diagnostics;

using CommunityToolkit.Mvvm.Input;

using Vapour.Client.Core;
using Vapour.Client.Core.ViewModel;
using Vapour.Shared.Common;
using Vapour.Shared.Common.Types;

namespace Vapour.Client.Modules.Profiles.Edit;

public sealed partial class StickControlModeSettingsViewModel :
    ViewModel<IStickControlModeSettingsViewModel>,
    IStickControlModeSettingsViewModel
{
    private readonly IDeviceValueConverters _deviceValueConverters;

    private int _antiSnapbackDelta;

    private int _antiSnapbackTimeout;

    private BezierCurve _customCurve;

    private StickDeadZoneInfo.DeadZoneType _deadZoneType;

    private int _fuzz;

    private bool _isAntiSnapback;

    private bool _isSquareStick;

    private CurveMode _outputCurve;

    private double _squareStickRoundness;

    public StickControlModeSettingsViewModel(IDeviceValueConverters deviceValueConverters)
    {
        _deviceValueConverters = deviceValueConverters;
    }

    public bool IsRadialSet => DeadZoneType == StickDeadZoneInfo.DeadZoneType.Radial;

    public bool IsCustomCurveSelected => OutputCurve == CurveMode.Custom;

    public double RotationConverted
    {
        get => _deviceValueConverters.RotationConvertFrom(Rotation);
        set => Rotation = _deviceValueConverters.RotationConvertTo(value);
    }

    public StickDeadZoneInfo.DeadZoneType DeadZoneType
    {
        get => _deadZoneType;
        set => SetProperty(ref _deadZoneType, value);
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

    public bool IsSquareStick
    {
        get => _isSquareStick;
        set => SetProperty(ref _isSquareStick, value);
    }

    public double SquareStickRoundness
    {
        get => _squareStickRoundness;
        set => SetProperty(ref _squareStickRoundness, Math.Round(value, 0));
    }

    public double Rotation { get; set; }

    public int Fuzz
    {
        get => _fuzz;
        set => SetProperty(ref _fuzz, value);
    }

    public bool IsAntiSnapback
    {
        get => _isAntiSnapback;
        set => SetProperty(ref _isAntiSnapback, value);
    }

    public int AntiSnapbackDelta
    {
        get => _antiSnapbackDelta;
        set => SetProperty(ref _antiSnapbackDelta, value);
    }

    public int AntiSnapbackTimeout
    {
        get => _antiSnapbackTimeout;
        set => SetProperty(ref _antiSnapbackTimeout, value);
    }

    [RelayCommand]
    private void ShowCustomCurve()
    {
        ProcessStartInfo processStartInfo = new(Constants.BezierCurveEditorPath) { UseShellExecute = true };
        Process.Start(processStartInfo);
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName == nameof(DeadZoneType))
        {
            OnPropertyChanged(nameof(IsRadialSet));
        }
        else if (e.PropertyName == nameof(DeadZone))
        {
            OnPropertyChanged(nameof(DeadZoneConverted));
        }
        else if (e.PropertyName == nameof(XDeadZone))
        {
            OnPropertyChanged(nameof(XDeadZoneConverted));
        }
        else if (e.PropertyName == nameof(YDeadZone))
        {
            OnPropertyChanged(nameof(YDeadZoneConverted));
        }
        else if (e.PropertyName == nameof(OutputCurve))
        {
            if (OutputCurve != CurveMode.Custom)
            {
                CustomCurve = null;
            }

            OnPropertyChanged(nameof(IsCustomCurveSelected));
        }
        else if (e.PropertyName == nameof(IsSquareStick) && !IsSquareStick)
        {
            SquareStickRoundness = SquareStickInfo.DefaultSquareStickRoundness;
        }
        else if (e.PropertyName == nameof(Rotation))
        {
            OnPropertyChanged(nameof(RotationConverted));
        }
    }

    #region Radial Properties

    public int DeadZone { get; set; }

    public double DeadZoneConverted
    {
        get => _deviceValueConverters.DeadZoneIntToDouble(DeadZone);
        set => DeadZone = _deviceValueConverters.DeadZoneDoubleToInt(value);
    }

    private int _antiDeadZone;

    public int AntiDeadZone
    {
        get => _antiDeadZone;
        set => SetProperty(ref _antiDeadZone, value);
    }

    private int _maxZone;

    public int MaxZone
    {
        get => _maxZone;
        set => SetProperty(ref _maxZone, value);
    }

    private int _maxOutput;

    public int MaxOutput
    {
        get => _maxOutput;
        set => SetProperty(ref _maxOutput, value);
    }

    private bool _forceMaxOutput;

    public bool ForceMaxOutput
    {
        get => _forceMaxOutput;
        set => SetProperty(ref _forceMaxOutput, value);
    }

    private int _verticalScale;

    public int VerticalScale
    {
        get => _verticalScale;
        set => SetProperty(ref _verticalScale, value);
    }

    private double _sensitivity;

    public double Sensitivity
    {
        get => _sensitivity;
        set => SetProperty(ref _sensitivity, Math.Round(value, 1));
        //Math round needed because wpf slider control when binding to double and tick increment
        //of 0.1 when it hits values like 1.2 and 2.2 it sends 1.20000000002 or something like that
    }

    #endregion

    #region X Properties

    public int XDeadZone { get; set; }

    public double XDeadZoneConverted
    {
        get => _deviceValueConverters.DeadZoneIntToDouble(XDeadZone);
        set => XDeadZone = _deviceValueConverters.DeadZoneDoubleToInt(value);
    }

    private int _xMaxZone;

    public int XMaxZone
    {
        get => _xMaxZone;
        set => SetProperty(ref _xMaxZone, value);
    }

    private int _xAntiDeadZone;

    public int XAntiDeadZone
    {
        get => _xAntiDeadZone;
        set => SetProperty(ref _xAntiDeadZone, value);
    }

    private int _xMaxOutput;

    public int XMaxOutput
    {
        get => _xMaxOutput;
        set => SetProperty(ref _xMaxOutput, value);
    }

    #endregion

    #region Y Properties

    public int YDeadZone { get; set; }

    public double YDeadZoneConverted
    {
        get => _deviceValueConverters.DeadZoneIntToDouble(YDeadZone);
        set => YDeadZone = _deviceValueConverters.DeadZoneDoubleToInt(value);
    }

    private int _yMaxZone;

    public int YMaxZone
    {
        get => _yMaxZone;
        set => SetProperty(ref _yMaxZone, value);
    }

    private int _yAntiDeadZone;

    public int YAntiDeadZone
    {
        get => _yAntiDeadZone;
        set => SetProperty(ref _yAntiDeadZone, value);
    }

    private int _yMaxOutput;

    public int YMaxOutput
    {
        get => _yMaxOutput;
        set => SetProperty(ref _yMaxOutput, value);
    }

    #endregion
}