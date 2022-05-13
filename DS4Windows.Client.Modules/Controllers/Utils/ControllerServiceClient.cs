using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using DS4Windows.Server;
using DS4Windows.Server.Controller;
using DS4Windows.Shared.Common.Core;
using Newtonsoft.Json;
using Serilog;
using Websocket.Client;

namespace DS4Windows.Client.Modules.Controllers.Utils
{
    public class ControllerServiceClient : IControllerServiceClient
    {
        private Action<ControllerConnectedMessage> connectedAction;
        private Action<ControllerDisconnectedMessage> disconnectedAction;

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

        public async void StartWebSocket(Action<ControllerConnectedMessage> connectedHandler, Action<ControllerDisconnectedMessage> disconnectedHandler)
        {
            connectedAction = connectedHandler;
            disconnectedAction = disconnectedHandler;

            var client = new WebsocketClient(new Uri($"{Constants.WebsocketUrl}/controller/ws", UriKind.Absolute));
            
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
        }
    }
}
