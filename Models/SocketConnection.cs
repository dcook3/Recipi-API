using System.Net.WebSockets;

namespace Recipi_API.Models
{
    public class SocketConnection
    {
        public Guid Id { get; set; }

        public int UserId {  get; set; }
        public int ConversationId {  get; set; }
        public WebSocket WebSocket { get; set; } = null!;
    }
}