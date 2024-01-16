using System.ComponentModel;
using System.Diagnostics;

using CommunityToolkit.Mvvm.ComponentModel;
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

    [ObservableProperty]
    private int _antiDeadZone;

    [ObservableProperty]
    private BezierCurve _customCurve;

    [ObservableProperty]
    private int _hipFireDelay;

    [ObservableProperty]
    private int _maxOutput;

    [ObservableProperty]
    private int _maxZone;

    [ObservableProperty]
    private CurveMode _outputCurve;

    private double _sensitivity;

    [ObservableProperty]
    private TriggerEffects _triggerEffect;

    [ObservableProperty]
    private TwoStageTriggerMode _twoStageTriggerMode;

    public TriggerButtonsEditViewModel(IDeviceValueConverters deviceValueConverters)
    {
        _deviceValueConverters = deviceValueConverters;
        ShowCustomCurveCommand = new RelayCommand(OnShowCustomCurve);
    }

    public bool IsCustomCurveSelected => OutputCurve == CurveMode.Custom;

    public RelayCommand ShowCustomCurveCommand { get; }

    public int DeadZone { get; set; }

    public double DeadZoneConverted
    {
        get => _deviceValueConverters.DeadZoneIntToDouble(DeadZone);
        set => DeadZone = _deviceValueConverters.DeadZoneDoubleToInt(value);
    }

    public double Sensitivity
    {
        get => _sensitivity;
        set => SetProperty(ref _sensitivity, Math.Round(value, 1));
        //Math round needed because wpf slider control when binding to double and tick increment
        //of 0.1 when it hits values like 1.2 and 2.2 it sends 1.20000000002 or something like that
    }

    private void OnShowCustomCurve()
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