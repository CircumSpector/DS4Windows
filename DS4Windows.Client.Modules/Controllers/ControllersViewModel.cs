using DS4Windows.Client.Core.ViewModel;
using DS4Windows.Client.Modules.Profiles;
using DS4Windows.Shared.Configuration.Profiles.Services;
using DS4Windows.Shared.Devices.HID;
using DS4Windows.Shared.Devices.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace DS4Windows.Client.Modules.Controllers
{
    public class ControllersViewModel : NavigationTabViewModel<IControllersViewModel, IControllersView>,  IControllersViewModel
    {
        private readonly IControllersEnumeratorService controllersEnumeratorService;
        private readonly IProfilesService profilesService;
        private readonly IServiceProvider serviceProvider;

        public ControllersViewModel(IControllersEnumeratorService controllersEnumeratorService, IProfilesService profilesService, IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.controllersEnumeratorService = controllersEnumeratorService;
            this.profilesService = profilesService;

            CreateSelectableProfileItems();
            CreateControllerItems();
        }

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

        private void ControllersEnumeratorService_ControllerRemoved(CompatibleHidDevice obj)
        {
            RemoveControllerItem(obj);
        }

        private void ControllersEnumeratorService_ControllerReady(CompatibleHidDevice obj)
        {
            CreateControllerItem(obj);
        }

        private void CreateControllerItem(CompatibleHidDevice device)
        {
            if (!ControllerItems.Any(i => i.InstanceId == device.InstanceId))
            {
                var deviceItem = serviceProvider.GetService<IControllerItemViewModel>();
                deviceItem.SetDevice(device);
                ControllerItems.Add(deviceItem);
            }
        }

        private void RemoveControllerItem(CompatibleHidDevice device)
        {
            var existing = ControllerItems.SingleOrDefault(i => i.InstanceId == device.InstanceId);
            if (existing != null)
            {
                ControllerItems.Remove(existing);
                existing.Dispose();
            }
        }

        private void CreateSelectableProfileItems()
        {
            foreach (var item in this.profilesService.AvailableProfiles)
            {
                var selectableProfileItem = serviceProvider.GetService<ISelectableProfileItemViewModel>();
                selectableProfileItem.SetProfile(item);
                SelectableProfileItems.Add(selectableProfileItem);
            }
        }

        #region configuration properties

        //TODO: Change to pull localization values
        public override string? Header => "Controllers";

        public override int TabIndex => 1;

        #endregion
    }
}
