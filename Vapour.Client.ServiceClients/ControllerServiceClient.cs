using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Windows;
using Vapour.Server;
using Vapour.Server.Controller;
using Vapour.Shared.Common.Core;
using Newtonsoft.Json;
using Serilog;
using Websocket.Client;

namespace Vapour.Client.ServiceClients
{
    public class ControllerServiceClient : IControllerServiceClient
    {
        private Action<ControllerConnectedMessage> connectedAction;
        private Action<ControllerDisconnectedMessage> disconnectedAction;
        private Action<IsHostRunningChangedMessage> hostRunningHandler;
        private WebsocketClient websocketClient;

        public ControllerServiceClient()
        {
            Application.Current.Exit += Current_Exit;
        }

        private async void Current_Exit(object sender, ExitEventArgs e)
        {
            if (websocketClient != null)
            {
                await websocketClient.Stop(WebSocketCloseStatus.NormalClosure, string.Empty);
                websocketClient.Dispose();
            }
        }

        public async Task WaitForService()
        {
            var client = new HttpClient();

            while (true)
            {
                try
                {
                    var result = await client.GetAsync($"{Constants.HttpUrl}/controller/ping");
                    if (result.IsSuccessStatusCode)
                    {
                        break;
                    }
                }
                catch
                {
                    // ignored
                }
                await Task.Delay(500);
            }
        }

        public async Task<List<ControllerConnectedMessage>> GetControllerList()
        {
            var client = new HttpClient();
            var result = await client.GetAsync($"{Constants.HttpUrl}/controller/list");
            if (result.IsSuccessStatusCode)
            {
                var content = await result.Content.ReadAsStringAsync();
                var list = JsonConvert.DeserializeObject<List<ControllerConnectedMessage>>(content);

                return list;
            }

            throw new Exception($"Could not get the controller list {result.ReasonPhrase}");
        }

        public async void StartWebSocket(
            Action<ControllerConnectedMessage> connectedHandler, 
            Action<ControllerDisconnectedMessage> disconnectedHandler,
            Action<IsHostRunningChangedMessage> hostRunningChangedHandler = null
        )
        {
            connectedAction = connectedHandler;
            disconnectedAction = disconnectedHandler;
            hostRunningHandler = hostRunningChangedHandler;

            websocketClient = new WebsocketClient(new Uri($"{Constants.WebsocketUrl}/controller/ws", UriKind.Absolute));
            
            websocketClient.ReconnectTimeout = TimeSpan.FromSeconds(30);
            websocketClient.ReconnectionHappened.Subscribe(info =>
            {
                Log.Information($"Reconnection happened, type: {info.Type}");
                websocketClient.MessageReceived.Subscribe(ProcessControllerMessage);
            });

            websocketClient.MessageReceived.Subscribe(ProcessControllerMessage);

            await websocketClient.Start();
        }

        public async Task<bool> IsHostRunning()
        {
            var client = new HttpClient();
            var result = await client.GetAsync($"{Constants.HttpUrl}/controller/ishostrunning");
            if (result.IsSuccessStatusCode)
            {
                var content = await result.Content.ReadAsStringAsync();
                return bool.Parse(content);
            }

            throw new Exception($"Could not get the controller list {result.ReasonPhrase}");
        }

        public async Task StartHost()
        {
            var client = new HttpClient();
            var result = await client.GetAsync($"{Constants.HttpUrl}/controller/starthost");
            if (result.IsSuccessStatusCode)
            {
            }
            else
            {
                throw new Exception($"Could not get the controller list {result.ReasonPhrase}");
            }
        }

        public async Task StopHost()
        {
            var client = new HttpClient();
            var result = await client.GetAsync($"{Constants.HttpUrl}/controller/stophost");
            if (result.IsSuccessStatusCode)
            {
            }
            else
            {
                throw new Exception($"Could not get the controller list {result.ReasonPhrase}");
            }
        }

        private async void ProcessControllerMessage(ResponseMessage msg)
        {
            var messageBase = JsonConvert.DeserializeObject<MessageBase>(msg.Text);
            if (messageBase.MessageName == ControllerConnectedMessage.Name)
            {
                await Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    var device = JsonConvert.DeserializeObject<ControllerConnectedMessage>(msg.Text);
                    if (connectedAction != null)
                    {
                        connectedAction(device);
                    }
                });
            }
            else if (messageBase.MessageName == ControllerDisconnectedMessage.Name)
            {
                await Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    var device = JsonConvert.DeserializeObject<ControllerDisconnectedMessage>(msg.Text);
                    if (disconnectedAction != null)
                    {
                        disconnectedAction(device);
                    }
                });
            }
            else if (messageBase.MessageName == IsHostRunningChangedMessage.Name)
            {
                if (hostRunningHandler != null)
                {
                    var isHostRunning = JsonConvert.DeserializeObject<IsHostRunningChangedMessage>(msg.Text);
                    hostRunningHandler(isHostRunning);
                }
            }
        }
    }
}
