using Recipi_API.Models;
using System.Net.WebSockets;

namespace Recipi_API.Services
{
    public interface IWebSocketService
    {
        public Task InitializeConnection(string message, List<SocketConnection> websocketConnections);
        public Task<string?> ReceiveMessage(Guid id, WebSocket webSocket);
        public Task SendMessageToSockets(string message, List<SocketConnection>webSocketConnections);
    }
}
