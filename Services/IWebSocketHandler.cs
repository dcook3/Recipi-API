using Recipi_API.Models;
using System.Net.WebSockets;

namespace Recipi_API.Services
{
    public interface IWebSocketHandler
    {
        public Task HandleConnection(int userId, int conversationId, WebSocket webSocket, IWebSocketService webSocketService);
    }
}
