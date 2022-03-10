using DS4Windows.Client.Core.ViewModel;
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
        private readonly IServiceProvider serviceProvider;

        public ControllersViewModel(IControllersEnumeratorService controllersEnumeratorService, IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;

            foreach (var controller in controllersEnumeratorService.SupportedDevices)
            {
                CreateControllerItem(controller);
            }

            controllersEnumeratorService.ControllerReady += ControllersEnumeratorService_ControllerReady;
            controllersEnumeratorService.ControllerRemoved += ControllersEnumeratorService_ControllerRemoved;
            ControllersEnumeratorService = controllersEnumeratorService;
        }

        public ObservableCollection<IControllerItemViewModel> ControllerItems { get; } = new ObservableCollection<IControllerItemViewModel>();
        public IControllersEnumeratorService ControllersEnumeratorService { get; }

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
            if (!ControllerItems.Any(i => i.Device.InstanceId == device.InstanceId))
            {
                var deviceItem = serviceProvider.GetService<IControllerItemViewModel>();
                deviceItem.Device = device;
                ControllerItems.Add(deviceItem);
            }
        }

        private void RemoveControllerItem(CompatibleHidDevice device)
        {
            var existing = ControllerItems.SingleOrDefault(i => i.Device.InstanceId == device.InstanceId);
            if (existing != null)
            {
                ControllerItems.Remove(existing);
                existing.Dispose();
            }
        }

        #region configuration properties

        //TODO: Change to pull localization values
        public override string? Header => "Controllers";

        public override int TabIndex => 1;

        #endregion
    }
}
