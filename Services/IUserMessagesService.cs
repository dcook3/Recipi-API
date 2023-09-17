using Recipi_API.Models;

namespace Recipi_API.Services
{
    public interface IUserMessagesService
    {
        Task<List<Conversation>> GetConversations(int postId);
        Task<bool> CreateConversation(Conversation convo);
        Task<bool> DeleteConversation(int conversationId);
    }
}