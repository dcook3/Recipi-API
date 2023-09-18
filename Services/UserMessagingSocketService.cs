using Recipi_API.Models;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;

namespace Recipi_API.Services
{
    public class UserMessagingSocketService: IWebSocketService
    {
        public async Task InitializeConnection(string message, List<SocketConnection> websocketConnections)
        {
            await SendMessageToSockets(message, websocketConnections);
        }

        public async Task<string?> ReceiveMessage(int id, int conversationId, WebSocket webSocket)
        {
            var arraySegment = new ArraySegment<byte>(new byte[1024 * 4]);

            try
            {
                var receivedMessage = await webSocket.ReceiveAsync(arraySegment, CancellationToken.None);

                if (receivedMessage.MessageType == WebSocketMessageType.Close)
                {
                    return "close";
                }

                if (receivedMessage.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.Default.GetString(arraySegment).TrimEnd('\0');
                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        return ($"{id},{conversationId},{message}");
                    }
                }
            }
            catch (Exception ex)
            {
                return "close";
            }
            return null;
        }

        public async Task SendMessageToSockets(string message, List<SocketConnection> websocketConnections)
        {
            IEnumerable<SocketConnection> toSentTo;

            lock (websocketConnections)
            {
                toSentTo = websocketConnections.ToList();
            }

            var tasks = toSentTo.Select(async websocketConnection =>
            {
                try
                {
                    var bytes = Encoding.Default.GetBytes(message);
                    var arraySegment = new ArraySegment<byte>(bytes);
                    await websocketConnection.WebSocket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch (Exception ex)
                {

                }
            });
            await Task.WhenAll(tasks);
        }
    }
}
