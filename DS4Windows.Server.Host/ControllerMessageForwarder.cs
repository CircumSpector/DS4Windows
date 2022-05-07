using System.Net.WebSockets;
using System.Text;
using System.Windows.Threading;
using DS4Windows.Shared.Configuration.Profiles.Services;
using DS4Windows.Shared.Devices.HID;
using DS4Windows.Shared.Devices.Services;
using Newtonsoft.Json;

namespace DS4Windows.Server
{
    public class ControllerMessageForwarder : IControllerMessageForwarder
    {
        private readonly IControllersEnumeratorService controllersEnumeratorService;
        private readonly IProfilesService profilesService;
        private WebSocket socket;

        public ControllerMessageForwarder(IControllersEnumeratorService controllersEnumeratorService, IProfilesService profilesService)
        {
            this.profilesService = profilesService;
            this.controllersEnumeratorService = controllersEnumeratorService;
            controllersEnumeratorService.ControllerReady += ControllersEnumeratorService_ControllerReady;
            controllersEnumeratorService.ControllerRemoved += ControllersEnumeratorService_ControllerRemoved;
        }

        public async Task StartListening(WebSocket newSocket)
        {
            socket = newSocket;
            var waitSource = new TaskCompletionSource<object>();
            await waitSource.Task;
        }

        public ControllerConnectedMessage MapControllerConnected(ICompatibleHidDevice hidDevice)
        {
            var message = new ControllerConnectedMessage
            {
                Description = hidDevice.Description,
                DeviceType = hidDevice.DeviceType,
                DisplayName = hidDevice.DisplayName,
                InstanceId = hidDevice.InstanceId,
                ManufacturerString = hidDevice.ManufacturerString,
                ParentInstance = hidDevice.ParentInstance,
                Path = hidDevice.Path,
                ProductString = hidDevice.ProductString,
                SerialNumberString = hidDevice.SerialNumberString,
                //Serial = hidDevice.Serial,
                Connection = hidDevice.Connection.GetValueOrDefault(),
                SelectedProfileId = profilesService.ActiveProfiles.Single(p => p.DeviceId != null && p.DeviceId.Equals(hidDevice.Serial)).Id
            };

            return message;
        }

        private async void ControllersEnumeratorService_ControllerReady(ICompatibleHidDevice hidDevice)
        {
            if (socket is { State: WebSocketState.Open })
            {
                await Dispatcher.CurrentDispatcher.BeginInvoke(async () =>
                {
                    var data = new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(MapControllerConnected(hidDevice))));
                    await socket.SendAsync(data, WebSocketMessageType.Text, true, CancellationToken.None);
                });
                
            }
        }

        private async void ControllersEnumeratorService_ControllerRemoved(ICompatibleHidDevice obj)
        {
            if (socket is { State: WebSocketState.Open })
            {
                await Dispatcher.CurrentDispatcher.BeginInvoke(async () =>
                {
                    var message = new ControllerDisconnectedMessage
                    {
                        ControllerDisconnectedId = obj.InstanceId
                    };
                    var data = new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message)));
                    await socket.SendAsync(data, WebSocketMessageType.Text, true, CancellationToken.None);
                });
            }
        }
    }
}
