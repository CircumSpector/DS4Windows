using System.Diagnostics;

using CommunityToolkit.Mvvm.Input;

using Vapour.Client.Core;
using Vapour.Client.Core.ViewModel;
using Vapour.Shared.Common;
using Vapour.Shared.Common.Types;

namespace Vapour.Client.Modules.Profiles.Edit;

public sealed class SixAxisEditViewModel :
    ViewModel<ISixAxisEditViewModel>,
    ISixAxisEditViewModel
{
    private readonly IDeviceValueConverters _deviceValueConverters;

    private double _antiDeadZone;

    private BezierCurve _customCurve;

    private double _deadZone;

    private double _maxZone;

    private CurveMode _outputCurve;

    private double _sensitivity;

    public SixAxisEditViewModel(IDeviceValueConverters deviceValueConverters)
    {
        _deviceValueConverters = deviceValueConverters;
        ShowCustomCurveCommand = new RelayCommand(OnShowCustomCurve);
    }

    public bool IsCustomCurveSelected => OutputCurve == CurveMode.Custom;

    public RelayCommand ShowCustomCurveCommand { get; }

    public double DeadZone
    {
        get => _deadZone;
        set => SetProperty(ref _deadZone, Math.Round(value, 1));
    }

    public double AntiDeadZone
    {
        get => _antiDeadZone;
        set => SetProperty(ref _antiDeadZone, Math.Round(value, 1));
    }

    public double MaxZone
    {
        get => _maxZone;
        set => SetProperty(ref _maxZone, Math.Round(value, 1));
    }

    public double Sensitivity
    {
        get => _sensitivity;
        set => SetProperty(ref _sensitivity, Math.Round(value, 1));
        //Math round needed because wpf slider control when binding to double and tick increment
        //of 0.1 when it hits values like 1.2 and 2.2 it sends 1.20000000002 or something like that
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

    private void OnShowCustomCurve()
    {
        ProcessStartInfo processStartInfo = new ProcessStartInfo(Constants.BezierCurveEditorPath);
        processStartInfo.UseShellExecute = true;
        Process.Start(processStartInfo);
    }
}