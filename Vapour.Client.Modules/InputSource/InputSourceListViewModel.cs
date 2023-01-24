using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

using CommunityToolkit.Mvvm.Input;

using MaterialDesignThemes.Wpf;

using Microsoft.Extensions.DependencyInjection;

using PropertyChanged;

using Vapour.Client.Core.ViewModel;
using Vapour.Client.Modules.Profiles;
using Vapour.Client.ServiceClients;
using Vapour.Server.InputSource;
using Vapour.Server.InputSource.Configuration;
using Vapour.Shared.Configuration.Profiles.Schema;

using Constants = Vapour.Client.Modules.Main.Constants;

namespace Vapour.Client.Modules.InputSource;

[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed class InputSourceListViewModel :
    NavigationTabViewModel<IInputSourceListViewModel, IInputSourceListView>,
    IInputSourceListViewModel
{
    private readonly IInputSourceServiceClient _inputSourceService;
    private readonly IProfileServiceClient _profilesService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IViewModelFactory _viewModelFactory;
    private string _pair1DeviceKey;
    private string _pair2DeviceKey;

    public InputSourceListViewModel(
        IProfileServiceClient profilesService,
        IServiceProvider serviceProvider,
        IInputSourceServiceClient inputSourceService,
        IViewModelFactory viewModelFactory)
    {
        _serviceProvider = serviceProvider;
        _inputSourceService = inputSourceService;
        _viewModelFactory = viewModelFactory;
        _profilesService = profilesService;

        ConfigureCommand = new RelayCommand<IInputSourceItemViewModel>(OnConfigure);
        PairCommand = new RelayCommand<IInputSourceItemViewModel>(OnPair);

        CreateSelectableProfileItems();
    }

    public RelayCommand<IInputSourceItemViewModel> ConfigureCommand { get; }
    public RelayCommand<IInputSourceItemViewModel> PairCommand { get; }
    public ObservableCollection<IInputSourceItemViewModel> InputSourceItems { get; } = new();
    public ObservableCollection<ISelectableProfileItemViewModel> SelectableProfileItems { get; } = new();

    public override async Task Initialize()
    {
        await CreateInputSourceItems();

        _inputSourceService.InInputSourceCreated += CreateInputSourceItemHandler;
        _inputSourceService.OnInputSourceRemoved += RemoveInputSourceItem;
        _inputSourceService.OnInputSourceConfigurationChanged += OnInputSourceConfigurationChanged;
        _inputSourceService.StartListening();
        //_profilesService.StartListening();
    }

    private async Task CreateInputSourceItems()
    {
        List<InputSourceMessage> list = await _inputSourceService.GetInputSourceList();

        foreach (InputSourceMessage inputSource in list)
        {
            await CreateInputSourceItem(inputSource);
        }
    }

    private async void CreateInputSourceItemHandler(InputSourceMessage inputSource)
    {
        await CreateInputSourceItem(inputSource);
    }

    private async Task CreateInputSourceItem(InputSourceMessage inputSource)
    {
        if (InputSourceItems.All(i => i.InputSourceKey != inputSource.InputSourceKey))
        {
            IInputSourceItemViewModel inputSourceItem = _serviceProvider.GetService<IInputSourceItemViewModel>();
            await inputSourceItem.SetInputSource(inputSource);
            InputSourceItems.Add(inputSourceItem);
        }
    }

    private void RemoveInputSourceItem(InputSourceRemovedMessage inputSource)
    {
        IInputSourceItemViewModel existing =
            InputSourceItems.SingleOrDefault(i => i.InputSourceKey == inputSource.InputSourceKey);
        if (existing != null)
        {
            InputSourceItems.Remove(existing);
            existing.Dispose();
        }
    }

    private void CreateSelectableProfileItems()
    {
        foreach (IProfile item in _profilesService.ProfileList)
        {
            CreateProfileItem(item);
        }

        _profilesService.ProfileList.CollectionChanged += InputSourceListViewModel_CollectionChanged;
    }

    private void InputSourceListViewModel_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            foreach (IProfile profile in e.NewItems)
            {
                CreateProfileItem(profile);
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            foreach (IProfile profile in e.OldItems)
            {
                RemoveProfileItem(profile);
            }
        }
    }

    private void CreateProfileItem(IProfile profile)
    {
        if (SelectableProfileItems.All(p => p.Id != profile.Id))
        {
            ISelectableProfileItemViewModel selectableProfileItem =
                _serviceProvider.GetService<ISelectableProfileItemViewModel>();
            selectableProfileItem.SetProfile(profile);
            SelectableProfileItems.Add(selectableProfileItem);
        }
    }

    private void RemoveProfileItem(IProfile profile)
    {
        ISelectableProfileItemViewModel existing = SelectableProfileItems.SingleOrDefault(p => p.Id == profile.Id);
        if (existing != null)
        {
            existing.Dispose();
            SelectableProfileItems.Remove(existing);
        }
    }

    private async void OnConfigure(IInputSourceItemViewModel inputSourceItem)
    {
        IInputSourceConfigureViewModel inputSourceConfigureViewModel =
            await _viewModelFactory.Create<IInputSourceConfigureViewModel, IInputSourceConfigureView>();
        await inputSourceConfigureViewModel.SetInputSourceToConfigure(inputSourceItem);
        await DialogHost.Show(inputSourceConfigureViewModel.MainView, Constants.DialogHostName,
            new DialogClosingEventHandler((o, e) =>
            {
                inputSourceConfigureViewModel.Dispose();
            }));
    }

    private async void OnPair(IInputSourceItemViewModel inputSourceItem)
    {
        //if (string.IsNullOrEmpty(_pair1DeviceKey))
        //{
        //    _pair1DeviceKey = inputSourceItem.Controller1.DeviceKey;
        //}
        //else
        //{
        //    await _inputSourceService.SaveDefaultInputSourceConfiguration(
        //        InputSourceUtil.CreateInputSourceKey(_pair1DeviceKey, _pair2DeviceKey), null);
        //}
    }

    [SuppressPropertyChangedWarnings]
    private void OnInputSourceConfigurationChanged(
        InputSourceConfigurationChangedMessage inputSourceConfigurationChangedMessage)
    {
        IInputSourceItemViewModel inputSource = InputSourceItems.SingleOrDefault(i =>
            i.InputSourceKey == inputSourceConfigurationChangedMessage.InputSourceKey);
        if (inputSource != null)
        {
            inputSource.ConfigurationSetFromUser = false;
            inputSource.CurrentConfiguration = inputSourceConfigurationChangedMessage.InputSourceConfiguration;
            inputSource.ConfigurationSetFromUser = true;
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (ISelectableProfileItemViewModel profile in SelectableProfileItems.ToList())
            {
                profile.Dispose();
                SelectableProfileItems.Remove(profile);
            }
        }

        base.Dispose(disposing);
    }

    #region configuration properties

    //TODO: Change to pull localization values
    public override string Header => "Input Sources";

    public override int TabIndex => 1;

    #endregion
}