using Microsoft.EntityFrameworkCore;
using Recipi_API.Models;
using Recipi_API.Models.Data_Models;
using System.Security.Claims;

namespace Recipi_API.Services
{
    public class UserMessagesService: IUserMessagesService
    {
        private readonly static RecipiDbContext db = new();
        private readonly IUserService userService;

        public UserMessagesService(IUserService _userService)
        {
            userService = _userService;
        }

        public async Task<List<Conversation>> GetConversations(int userId)
        {
            List<Conversation> conversations = await db.Conversations.Where(convo =>
                 convo.UserId1 == userId
                || convo.UserId2 == userId
            ).ToListAsync();

            return conversations;
        }

        public async Task<bool> CreateConversation(Conversation convo)
        {
            db.Conversations.Add(convo);
            var res = await db.SaveChangesAsync();
            return res > 0;
        }

        public async Task<bool> DeleteConversation(int conversationId)
        {
            db.Conversations.RemoveRange(db.Conversations.Where(convo => convo.ConversationId == conversationId));
            var res = await db.SaveChangesAsync();
            return res > 0;
        }

        public async Task<List<Message>> GetMessagesFromConversation(int conversationID)
        {
            List<Message> messages = await db.Messages.Where(message =>
                 message.ConversationId == conversationID 
            ).ToListAsync();

            return messages;
        }

        public async Task<bool> CreateMessage(Message msg)
        {
            db.Messages.Add(msg);
            var res = await db.SaveChangesAsync();
            return res > 0;
        }

        public async Task<bool> UpdateMessage(Message msg)
        {
            db.Messages.Update(msg);
            var res = await db.SaveChangesAsync();
            return res > 0;
        }

        public async Task<bool> DeleteMessage(Message msg)
        {
            db.Messages.Remove(msg);
            var res = await db.SaveChangesAsync();
            return res > 0;
        }
    }
}
