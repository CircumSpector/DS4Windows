using DS4Windows.Client.Core.ViewModel;
using DS4Windows.Client.Modules.Profiles;
using DS4Windows.Shared.Configuration.Profiles.Schema;
using DS4Windows.Shared.Configuration.Profiles.Services;
using DS4Windows.Shared.Devices.HID;
using DS4Windows.Shared.Devices.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using DS4Windows.Shared.Devices.DriverManagement;
using Microsoft.Toolkit.Mvvm.Input;

namespace DS4Windows.Client.Modules.Controllers
{
    public class ControllersViewModel : NavigationTabViewModel<IControllersViewModel, IControllersView>,  IControllersViewModel
    {
        private readonly IControllersEnumeratorService controllersEnumeratorService;
        private readonly IProfilesService profilesService;
        private readonly IServiceProvider serviceProvider;
        private readonly IControllerDriverManagementService controllerDriverManagementService;

        public ControllersViewModel(
            IControllersEnumeratorService controllersEnumeratorService, 
            IProfilesService profilesService, 
            IServiceProvider serviceProvider,
            IControllerDriverManagementService controllerDriverManagementService)
        {
            this.serviceProvider = serviceProvider;
            this.controllerDriverManagementService = controllerDriverManagementService;
            this.controllersEnumeratorService = controllersEnumeratorService;
            this.profilesService = profilesService;

            HideCommand = new RelayCommand<IControllerItemViewModel>(HideController);
            UnhideCommand = new RelayCommand<IControllerItemViewModel>(UnHideController);
            
            CreateSelectableProfileItems();
            CreateControllerItems();
        }

        public RelayCommand<IControllerItemViewModel> HideCommand { get; }
        public RelayCommand<IControllerItemViewModel> UnhideCommand { get; }
        public ObservableCollection<IControllerItemViewModel> ControllerItems { get; } = new ObservableCollection<IControllerItemViewModel>();
        public ObservableCollection<ISelectableProfileItemViewModel> SelectableProfileItems { get; } = new ObservableCollection<ISelectableProfileItemViewModel>();

        private void CreateControllerItems()
        {
            foreach (var controller in controllersEnumeratorService.SupportedDevices)
            {
                CreateControllerItem(controller);
            }

            controllersEnumeratorService.ControllerReady += ControllersEnumeratorService_ControllerReady;
            controllersEnumeratorService.ControllerRemoved += ControllersEnumeratorService_ControllerRemoved;
        }

        private void ControllersEnumeratorService_ControllerRemoved(ICompatibleHidDevice obj)
        {
            RemoveControllerItem(obj);
        }

        private void ControllersEnumeratorService_ControllerReady(ICompatibleHidDevice obj)
        {
            CreateControllerItem(obj);
        }

        private void CreateControllerItem(ICompatibleHidDevice device)
        {
            if (!ControllerItems.Any(i => i.Serial == device.Serial))
            {
                var deviceItem = serviceProvider.GetService<IControllerItemViewModel>();
                deviceItem.SetDevice(device);
                ControllerItems.Add(deviceItem);
            }
        }

        private void RemoveControllerItem(ICompatibleHidDevice device)
        {
            var existing = ControllerItems.SingleOrDefault(i => i.Serial == device.Serial);
            if (existing != null)
            {
                ControllerItems.Remove(existing);
                existing.Dispose();
            }
        }

        private void CreateSelectableProfileItems()
        {
            foreach (var item in profilesService.AvailableProfiles)
            {
                CreateProfileItem(item);
            }

            ((INotifyCollectionChanged)profilesService.AvailableProfiles).CollectionChanged += ControllersViewModel_CollectionChanged; ;
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
                ((INotifyCollectionChanged)profilesService.AvailableProfiles).CollectionChanged -= ControllersViewModel_CollectionChanged;
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
        public override string? Header => "Controllers";

        public override int TabIndex => 1;

        #endregion
    }
}
