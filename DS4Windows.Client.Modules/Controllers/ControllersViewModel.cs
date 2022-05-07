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
using Websocket.Client;

namespace DS4Windows.Client.Modules.Controllers
{
    public class ControllersViewModel : NavigationTabViewModel<IControllersViewModel, IControllersView>,  IControllersViewModel
    {
        private readonly IProfilesService profilesService;
        private readonly IServiceProvider serviceProvider;
        private readonly IControllerDriverManagementService controllerDriverManagementService;

        public ControllersViewModel( 
            IProfilesService profilesService, 
            IServiceProvider serviceProvider,
            IControllerDriverManagementService controllerDriverManagementService)
        {
            this.serviceProvider = serviceProvider;
            this.controllerDriverManagementService = controllerDriverManagementService;
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

            var client = new WebsocketClient(new Uri("wss://localhost:5001/controller/ws", UriKind.Absolute));
            
            client.ReconnectTimeout = TimeSpan.FromSeconds(30);
            client.ReconnectionHappened.Subscribe(info => Log.Information($"Reconnection happened, type: {info.Type}"));

            client.MessageReceived.Subscribe(ProcessControllerMessage);

            client.Start();
        }

        private async void ProcessControllerMessage(ResponseMessage msg)
        {
            var messageBase = JsonConvert.DeserializeObject<MessageBase>(msg.Text);
            if (messageBase.MessageName == ControllerConnectedMessage.Name)
            {
                await Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    var device = JsonConvert.DeserializeObject<ControllerConnectedMessage>(msg.Text); 
                    CreateControllerItem(device);
                });
            }
            else if (messageBase.MessageName == ControllerDisconnectedMessage.Name)
            {
                await Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    var device = JsonConvert.DeserializeObject<ControllerDisconnectedMessage>(msg.Text); 
                    RemoveControllerItem(device.ControllerDisconnectedId);
                });
            }
        }

        private async Task CreateControllerItems()
        {
            var client = new HttpClient();
            var result = await client.GetAsync("https://localhost:5001/controller/list");
            if (result.IsSuccessStatusCode)
            {
                var list = JsonConvert.DeserializeObject<List<ControllerConnectedMessage>>(
                    await result.Content.ReadAsStringAsync());
                foreach (var controller in list)
                {
                    CreateControllerItem(controller);
                }
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

        private void RemoveControllerItem(string instanceId)
        {
            var existing = ControllerItems.SingleOrDefault(i => i.InstanceId == instanceId);
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
        public override string Header => "Controllers";

        public override int TabIndex => 1;

        #endregion
    }
}
