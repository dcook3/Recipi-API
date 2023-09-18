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

        public async Task HandleConnection(int userId, int conversationId, WebSocket webSocket, IWebSocketService socketService)
        {
            Guid socketID = new Guid();
            lock (websocketConnections) {
              websocketConnections.Add(new SocketConnection {
                  Id = socketID,
                  ConversationId = conversationId,
                  UserId = userId,
                  WebSocket = webSocket
              });
            }

            while (webSocket.State == WebSocketState.Open)
            {
                var message = await socketService.ReceiveMessage(userId, conversationId, webSocket);
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
