using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Net.WebSockets;

using Recipi_API.Models.Data_Models;
using Recipi_API.Models;
using System.Text;
using System;

namespace Recipi_API.Services
{
    public class WebSocketHandler: IWebSocketHandler
    {
        List<SocketConnection> websocketConnections = new List<SocketConnection>();

        public async Task HandleConnection(Guid id, WebSocket webSocket, IWebSocketService socketService)
        {
            lock (websocketConnections) {
              websocketConnections.Add(new SocketConnection {
                  Id = id,
                  WebSocket = webSocket
              });
            }

            await socketService.SendMessageToSockets($"Socket {id} is online", websocketConnections);
            while (webSocket.State == WebSocketState.Open)
            {
                var message = await socketService.ReceiveMessage(id, webSocket);
                if (message != null && message != "close")
                {
                    await socketService.SendMessageToSockets(message, websocketConnections);
                }
                else if (message == "close")
                {
                    var targetSocket = websocketConnections.Single(r => r.WebSocket == webSocket);
                    websocketConnections.Remove(targetSocket);
                }
            }
        }
    }
}
