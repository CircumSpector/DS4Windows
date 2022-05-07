using System.Collections.ObjectModel;
using System.Management;
using System.Net.WebSockets;
using DS4Windows.Shared.Devices.HID;
using DS4Windows.Shared.Devices.Services;
using DS4Windows.Shared.Devices.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DS4Windows.Server.Host
{
    public class ControllerManagerApi
    {
        public static void RegisterRoutes(WebApplication app)
        {
            app.MapGet("/controller/ws",
                async (HttpContext context, ControllerManagerApi api) => await api.Get(context));
            app.Services.GetService<ControllerManagerApi>();
        }

        private WebSocket controllerWebSocket;
        public async Task<IResult> Get(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest) return Results.BadRequest();

            controllerWebSocket = await context.WebSockets.AcceptWebSocketAsync();
            return Results.Ok();
        }

        //private static async Task Echo(WebSocket webSocket)
        //{
        //    var buffer = new byte[1024 * 4];
        //    var receiveResult = await webSocket.ReceiveAsync(
        //        new ArraySegment<byte>(buffer), CancellationToken.None);

        //    while (!receiveResult.CloseStatus.HasValue)
        //    {
        //        await webSocket.SendAsync(
        //            new ArraySegment<byte>(buffer, 0, receiveResult.Count),
        //            receiveResult.MessageType,
        //            receiveResult.EndOfMessage,
        //            CancellationToken.None);

        //        receiveResult = await webSocket.ReceiveAsync(
        //            new ArraySegment<byte>(buffer), CancellationToken.None);
        //    }

        //    await webSocket.CloseAsync(
        //        receiveResult.CloseStatus.Value,
        //        receiveResult.CloseStatusDescription,
        //        CancellationToken.None);
        //}
    }
}
