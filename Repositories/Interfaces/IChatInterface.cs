using Repositories.Models;

namespace Repositories.Interfaces
{
    public interface IChatInterface
    {
        Task<int> SaveChat(Chat chat);
        Task<List<Chat>?> GetChatHistory(string senderId, string receiverId);
        Task<int> MarkChatAsRead(int chatId);
        Task<List<Chat>?> GetUnreadChats(Guid userId);
        Task<bool> MarkAllChatsAsRead(Guid senderId, Guid receiverId);
    }
}