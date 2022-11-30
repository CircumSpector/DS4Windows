using System.Collections.ObjectModel;
using System.Collections.Specialized;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;

using Vapour.Client.Core.ViewModel;
using Vapour.Client.Modules.Profiles;
using Vapour.Client.ServiceClients;
using Vapour.Server.Controller;
using Vapour.Shared.Configuration.Profiles.Schema;

namespace Vapour.Client.Modules.Controllers;

[UsedImplicitly]
public sealed class ControllersViewModel :
    NavigationTabViewModel<IControllersViewModel, IControllersView>,
    IControllersViewModel
{
    private readonly IControllerServiceClient _controllerService;
    private readonly IProfileServiceClient _profilesService;
    private readonly IServiceProvider _serviceProvider;

    public ControllersViewModel(
        IProfileServiceClient profilesService,
        IServiceProvider serviceProvider,
        IControllerServiceClient controllerService)
    {
        _serviceProvider = serviceProvider;
        _controllerService = controllerService;
        _profilesService = profilesService;

        CreateSelectableProfileItems();
    }

    public ObservableCollection<IControllerItemViewModel> ControllerItems { get; } = new();
    public ObservableCollection<ISelectableProfileItemViewModel> SelectableProfileItems { get; } = new();

    public override async Task Initialize()
    {
        await CreateControllerItems();

        _controllerService.StartListening(CreateControllerItem, RemoveControllerItem);
        _profilesService.StartWebSocket(OnProfileChanged);
    }

    private async Task CreateControllerItems()
    {
        List<ControllerConnectedMessage> list = await _controllerService.GetControllerList();

        foreach (ControllerConnectedMessage controller in list)
        {
            CreateControllerItem(controller);
        }
    }

    private void CreateControllerItem(ControllerConnectedMessage device)
    {
        if (ControllerItems.All(i => i.InstanceId != device.InstanceId))
        {
            IControllerItemViewModel deviceItem = _serviceProvider.GetService<IControllerItemViewModel>();
            deviceItem.SetDevice(device);
            ControllerItems.Add(deviceItem);
        }
    }

    private void RemoveControllerItem(ControllerDisconnectedMessage device)
    {
        IControllerItemViewModel existing =
            ControllerItems.SingleOrDefault(i => i.InstanceId == device.ControllerDisconnectedId);
        if (existing != null)
        {
            ControllerItems.Remove(existing);
            existing.Dispose();
        }
    }

    private void CreateSelectableProfileItems()
    {
        foreach (IProfile item in _profilesService.ProfileList)
        {
            CreateProfileItem(item);
        }

        _profilesService.ProfileList.CollectionChanged += ControllersViewModel_CollectionChanged;
    }

    private void ControllersViewModel_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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

    private void OnProfileChanged(ProfileChangedMessage profileChangedMessage)
    {
        var controller = ControllerItems.SingleOrDefault(i => i.Serial == profileChangedMessage.ControllerKey);
        if (controller != null)
        {
            controller.SelectedProfileId = profileChangedMessage.NewProfileId;
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
    public override string Header => "Controllers";

    public override int TabIndex => 1;

    #endregion
}