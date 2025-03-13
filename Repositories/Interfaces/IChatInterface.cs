using Repositories.Models;

namespace Repositories.Interfaces
{
    public interface IChatInterface
    {
        Task<int> SaveChat(Chat chat);
        Task<List<Chat>?> GetChatHistory(int senderId, int receiverId);
        Task<int> MarkChatAsRead(int chatId);
    }
}