using DS4Windows.Client.Core.ViewModel;
using DS4Windows.Client.Modules.Profiles;
using DS4Windows.Server;
using DS4Windows.Shared.Configuration.Profiles.Schema;
using DS4Windows.Shared.Configuration.Profiles.Services;
using DS4Windows.Shared.Devices.DriverManagement;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using DS4Windows.Client.Modules.Controllers.Utils;
using DS4Windows.Client.Modules.Profiles.Utils;
using DS4Windows.Shared.Common.Core;
using Websocket.Client;

namespace DS4Windows.Client.Modules.Controllers
{
    public class ControllersViewModel : NavigationTabViewModel<IControllersViewModel, IControllersView>,  IControllersViewModel
    {
        private readonly IProfileServiceClient profilesService;
        private readonly IServiceProvider serviceProvider;
        private readonly IControllerDriverManagementService controllerDriverManagementService;
        private readonly IControllerServiceClient controllerService;

        public ControllersViewModel( 
            IProfileServiceClient profilesService, 
            IServiceProvider serviceProvider,
            IControllerDriverManagementService controllerDriverManagementService,
            IControllerServiceClient controllerService)
        {
            this.serviceProvider = serviceProvider;
            this.controllerDriverManagementService = controllerDriverManagementService;
            this.controllerService = controllerService;
            this.profilesService = profilesService;

            HideCommand = new RelayCommand<IControllerItemViewModel>(HideController);
            UnhideCommand = new RelayCommand<IControllerItemViewModel>(UnHideController);
            
            CreateSelectableProfileItems();
            
        }

        public RelayCommand<IControllerItemViewModel> HideCommand { get; }
        public RelayCommand<IControllerItemViewModel> UnhideCommand { get; }
        public ObservableCollection<IControllerItemViewModel> ControllerItems { get; } = new ObservableCollection<IControllerItemViewModel>();
        public ObservableCollection<ISelectableProfileItemViewModel> SelectableProfileItems { get; } = new ObservableCollection<ISelectableProfileItemViewModel>();
        
        public override async Task Initialize()
        {
            await CreateControllerItems();

            controllerService.StartWebSocket(CreateControllerItem, RemoveControllerItem);
        }

        private async Task CreateControllerItems()
        {
            var list = await controllerService.GetControllerList();
            
            foreach (var controller in list)
            {
                CreateControllerItem(controller);
            }
        }

        private void CreateControllerItem(ControllerConnectedMessage device)
        {
            if (!ControllerItems.Any(i => i.InstanceId == device.InstanceId))
            {
                var deviceItem = serviceProvider.GetService<IControllerItemViewModel>();
                deviceItem.SetDevice(device);
                ControllerItems.Add(deviceItem);
            }
        }

        private void RemoveControllerItem(ControllerDisconnectedMessage device)
        {
            var existing = ControllerItems.SingleOrDefault(i => i.InstanceId == device.ControllerDisconnectedId);
            if (existing != null)
            {
                ControllerItems.Remove(existing);
                existing.Dispose();
            }
        }

        private void CreateSelectableProfileItems()
        {
            foreach (var item in profilesService.ProfileList)
            {
                CreateProfileItem(item);
            }

            profilesService.ProfileList.CollectionChanged += ControllersViewModel_CollectionChanged;
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
            if (!SelectableProfileItems.Any(p => p.Id == profile.Id))
            {
                var selectableProfileItem = serviceProvider.GetService<ISelectableProfileItemViewModel>();
                selectableProfileItem.SetProfile(profile);
                SelectableProfileItems.Add(selectableProfileItem);
            }
        }

        private void RemoveProfileItem(IProfile profile)
        {
            var existing = SelectableProfileItems.SingleOrDefault(p => p.Id == profile.Id);
            if (existing != null)
            {
                existing.Dispose();
                SelectableProfileItems.Remove(existing);
            }
        }

        public void HideController(IControllerItemViewModel controller)
        {
            controllerDriverManagementService.HideController(controller.ParentInstance);
        }

        public void UnHideController(IControllerItemViewModel controller)
        {
            controllerDriverManagementService.UnhideController(controller.ParentInstance);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var profile in SelectableProfileItems.ToList())
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
}
