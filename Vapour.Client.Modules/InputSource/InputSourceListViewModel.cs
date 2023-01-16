using System.Collections.ObjectModel;
using System.Collections.Specialized;

using CommunityToolkit.Mvvm.Input;

using JetBrains.Annotations;

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

[UsedImplicitly]
public sealed class InputSourceListViewModel :
    NavigationTabViewModel<IInputSourceListViewModel, IInputSourceListView>,
    IInputSourceListViewModel
{
    private readonly IInputSourceServiceClient _inputSourceService;
    private readonly IViewModelFactory _viewModelFactory;
    private readonly IProfileServiceClient _profilesService;
    private readonly IServiceProvider _serviceProvider;

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

        CreateSelectableProfileItems();
    }

    public RelayCommand<IInputSourceItemViewModel> ConfigureCommand { get; }
    public ObservableCollection<IInputSourceItemViewModel> InputSourceItems { get; } = new();
    public ObservableCollection<ISelectableProfileItemViewModel> SelectableProfileItems { get; } = new();

    public override async Task Initialize()
    {
        await CreateInputSourceItems();

        _inputSourceService.InInputSourceCreated += CreateInputSourceItem;
        _inputSourceService.OnInputSourceRemoved += RemoveInputSourceItem;
        _inputSourceService.OnInputSourceConfigurationChanged += OnInputSourceConfigurationChanged; 
        _inputSourceService.StartListening();
        //_profilesService.StartListening();
    }

    private async Task CreateInputSourceItems()
    {
        List<InputSourceCreatedMessage> list = await _inputSourceService.GetInputSourceList();

        foreach (InputSourceCreatedMessage inputSource in list)
        {
            CreateInputSourceItem(inputSource);
        }
    }

    private void CreateInputSourceItem(InputSourceCreatedMessage device)
    {
        if (InputSourceItems.All(i => i.InputSourceKey != device.InputSourceKey))
        {
            IInputSourceItemViewModel inputSourceItem = _serviceProvider.GetService<IInputSourceItemViewModel>();
            inputSourceItem.SetDevice(device);
            InputSourceItems.Add(inputSourceItem);
        }
    }

    private void RemoveInputSourceItem(InputSourceRemovedMessage device)
    {
        IInputSourceItemViewModel existing =
            InputSourceItems.SingleOrDefault(i => i.InputSourceKey == device.InputSourceKey);
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
        IInputSourceConfigureViewModel inputSourceConfigureViewModel = await _viewModelFactory.Create<IInputSourceConfigureViewModel, IInputSourceConfigureView>();
        await inputSourceConfigureViewModel.SetInputSourceToConfigure(inputSourceItem);
        await DialogHost.Show(inputSourceConfigureViewModel.MainView, Constants.DialogHostName, new DialogClosingEventHandler((o, e) =>
        {
            inputSourceConfigureViewModel.Dispose();
        }));
    }

    [SuppressPropertyChangedWarnings]
    private void OnInputSourceConfigurationChanged(InputSourceConfigurationChangedMessage inputSourceConfigurationChangedMessage)
    {
        var inputSource = InputSourceItems.SingleOrDefault(i => i.InputSourceKey == inputSourceConfigurationChangedMessage.InputSourceKey);
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