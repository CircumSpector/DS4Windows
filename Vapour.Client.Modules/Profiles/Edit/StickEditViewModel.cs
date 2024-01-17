using CommunityToolkit.Mvvm.ComponentModel;

using Vapour.Client.Core.ViewModel;
using Vapour.Shared.Common.Types;

namespace Vapour.Client.Modules.Profiles.Edit;

public sealed partial class StickEditViewModel :
    ViewModel<IStickEditViewModel>,
    IStickEditViewModel
{
    private readonly IViewModelFactory _viewModelFactory;

    private double _flickMinAngleThreshold;

    [ObservableProperty]
    private double _flickRealWorldCalibration;

    private double _flickThreshold;

    private double _flickTime;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsControlModeSet))]
    [NotifyPropertyChangedFor(nameof(IsFlickStickSet))]
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

    public IStickControlModeSettingsViewModel ControlModeSettings { get; private set; }

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

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ControlModeSettings.Dispose();
        }

        base.Dispose(disposing);
    }
}