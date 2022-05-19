using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using DS4Windows.Client.Core.ViewModel;
using DS4Windows.Client.ServiceClients;
using DS4Windows.Server.Controller;

namespace DS4Windows.Client.TrayApplication
{
    public interface ITrayViewModel : IViewModel<ITrayViewModel>
    {

    }

    public class TrayViewModel :ViewModel<ITrayViewModel>, ITrayViewModel
    {
        private readonly IControllerServiceClient controllerServiceClient;
        private readonly IProfileServiceClient profileServiceClient;

        public TrayViewModel(IControllerServiceClient controllerServiceClient, IProfileServiceClient profileServiceClient)
        {
            this.controllerServiceClient = controllerServiceClient;
            this.profileServiceClient = profileServiceClient;
            ShowClientCommand = new RelayCommand(OnShowClient);
            ChangeHostStateCommand = new RelayCommand(ChangeHostState);
        }

        public ObservableCollection<ControllerConnectedMessage> ConnectedControllers { get; set; } =
            new ObservableCollection<ControllerConnectedMessage>();

        private bool isHostRunning;
        public bool IsHostRunning
        {
            get => isHostRunning;
            set => SetProperty(ref isHostRunning, value);
        }

        private string hostButtonText;
        public string HostButtonText
        {
            get => hostButtonText;
            set => SetProperty(ref hostButtonText, value);
        }

        public override async Task Initialize()
        {
            await controllerServiceClient.WaitForService();
            var controllerList = await controllerServiceClient.GetControllerList();
            foreach (var connectedController in controllerList)
            {
                ConnectedControllers.Add(connectedController);
            }

            IsHostRunning = await controllerServiceClient.IsHostRunning();
            controllerServiceClient.StartWebSocket(OnControllerConnected, OnControllerDisconnected, OnHostRunningChanged);
        }

        private void OnHostRunningChanged(IsHostRunningChangedMessage obj)
        {
            IsHostRunning = obj.IsRunning;
        }

        private void OnControllerDisconnected(ControllerDisconnectedMessage obj)
        {
            var existingController =
                ConnectedControllers.SingleOrDefault(c => c.InstanceId == obj.ControllerDisconnectedId);
            if (existingController != null)
            {
                ConnectedControllers.Remove(existingController);
            }
        }

        private void OnControllerConnected(ControllerConnectedMessage obj)
        {
            if (ConnectedControllers.All(c => c.InstanceId != obj.InstanceId))
            {
                ConnectedControllers.Add(obj);
            }
        }

        public IRelayCommand ShowClientCommand { get; }

        private void OnShowClient()
        {
            
        }

        public IRelayCommand ChangeHostStateCommand { get; }

        private async void ChangeHostState()
        {
            if (IsHostRunning)
            {
                await controllerServiceClient.StopHost();
            }
            else
            {
                await controllerServiceClient.StartHost();
            }
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.PropertyName == nameof(IsHostRunning))
            {
                HostButtonText = IsHostRunning ? "Stop" : "Start";
            }
        }
    }
}
