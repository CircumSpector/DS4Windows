using System.ComponentModel;
using System.Diagnostics;

using CommunityToolkit.Mvvm.ComponentModel;
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

    [ObservableProperty]
    private int _antiSnapbackDelta;

    [ObservableProperty]
    private int _antiSnapbackTimeout;

    [ObservableProperty]
    private BezierCurve _customCurve;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsRadialSet))]
    private StickDeadZoneInfo.DeadZoneType _deadZoneType;

    [ObservableProperty]
    private int _fuzz;

    [ObservableProperty]
    private bool _isAntiSnapback;

    [ObservableProperty]
    private bool _isSquareStick;

    [ObservableProperty]
    private CurveMode _outputCurve;

    private double _squareStickRoundness;

    public StickControlModeSettingsViewModel(IDeviceValueConverters deviceValueConverters)
    {
        _deviceValueConverters = deviceValueConverters;
        ShowCustomCurveCommand = new RelayCommand(OnShowCustomCurve);
    }

    public bool IsRadialSet => DeadZoneType == StickDeadZoneInfo.DeadZoneType.Radial;

    public bool IsCustomCurveSelected => OutputCurve == CurveMode.Custom;

    public RelayCommand ShowCustomCurveCommand { get; }

    public double RotationConverted
    {
        get => _deviceValueConverters.RotationConvertFrom(Rotation);
        set => Rotation = _deviceValueConverters.RotationConvertTo(value);
    }

    public double SquareStickRoundness
    {
        get => _squareStickRoundness;
        set => SetProperty(ref _squareStickRoundness, Math.Round(value, 0));
    }

    public double Rotation { get; set; }

    private void OnShowCustomCurve()
    {
        ProcessStartInfo processStartInfo = new(Constants.BezierCurveEditorPath) { UseShellExecute = true };
        Process.Start(processStartInfo);
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName == nameof(DeadZone))
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

    [ObservableProperty]
    private int _antiDeadZone;

    [ObservableProperty]
    private int _maxZone;

    [ObservableProperty]
    private int _maxOutput;

    [ObservableProperty]
    private bool _forceMaxOutput;

    [ObservableProperty]
    private int _verticalScale;

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

    [ObservableProperty]
    private int _xMaxZone;

    [ObservableProperty]
    private int _xAntiDeadZone;

    [ObservableProperty]
    private int _xMaxOutput;

    #endregion

    #region Y Properties

    public int YDeadZone { get; set; }

    public double YDeadZoneConverted
    {
        get => _deviceValueConverters.DeadZoneIntToDouble(YDeadZone);
        set => YDeadZone = _deviceValueConverters.DeadZoneDoubleToInt(value);
    }

    [ObservableProperty]
    private int _yMaxZone;

    [ObservableProperty]
    private int _yAntiDeadZone;

    [ObservableProperty]
    private int _yMaxOutput;

    #endregion
}