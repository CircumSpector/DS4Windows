using System.ComponentModel;

using Vapour.Client.Core.ViewModel;
using Vapour.Shared.Common.Types;

namespace Vapour.Client.Modules.Profiles.Edit;

public sealed class StickEditViewModel :
    ViewModel<IStickEditViewModel>,
    IStickEditViewModel
{
    private readonly IViewModelFactory _viewModelFactory;

    private double _flickMinAngleThreshold;

    private double _flickRealWorldCalibration;

    private double _flickThreshold;

    private double _flickTime;

    private StickMode _outputSettings;

    public StickEditViewModel(IViewModelFactory viewModelFactory)
    {
        _viewModelFactory = viewModelFactory;
    }

    public bool IsControlModeSet => OutputSettings == StickMode.Controls;
    public bool IsFlickStickSet => OutputSettings == StickMode.FlickStick;

    public override async Task Initialize()
    {
        ControlModeSettings =
            await _viewModelFactory.Create<IStickControlModeSettingsViewModel, IStickControlModeSettingsView>();
    }

    public StickMode OutputSettings
    {
        get => _outputSettings;
        set => SetProperty(ref _outputSettings, value);
    }

    public IStickControlModeSettingsViewModel ControlModeSettings { get; private set; }

    public double FlickRealWorldCalibration
    {
        get => _flickRealWorldCalibration;
        set => SetProperty(ref _flickRealWorldCalibration, value);
    }

    public double FlickThreshold
    {
        get => _flickThreshold;
        set => SetProperty(ref _flickThreshold, Math.Round(value, 1));
    }

    public double FlickTime
    {
        get => _flickTime;
        set => SetProperty(ref _flickTime, Math.Round(value, 1));
    }

    public double FlickMinAngleThreshold
    {
        get => _flickMinAngleThreshold;
        set => SetProperty(ref _flickMinAngleThreshold, Math.Round(value, 1));
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName == nameof(OutputSettings))
        {
            OnPropertyChanged(nameof(IsControlModeSet));
            OnPropertyChanged(nameof(IsFlickStickSet));
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ControlModeSettings.Dispose();
        }

        base.Dispose(disposing);
    }
}