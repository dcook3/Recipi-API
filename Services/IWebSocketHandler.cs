using Recipi_API.Models;
using System.Net.WebSockets;

namespace Recipi_API.Services
{
    public interface IWebSocketHandler
    {
        public Task HandleConnection(Guid id, WebSocket webSocket, IWebSocketService webSocketService);
    }
}
