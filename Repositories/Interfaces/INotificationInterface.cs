using Repositories.Models;

namespace Repositories.Interfaces
{
    public interface INotificationInterface
    {
        Task<List<Notification>?> GetAllByUserId(Guid userId);
        Task<int> GetUnreadCount(Guid userId);
        Task<bool> MarkAsRead(int notificationId);
        Task<bool> MarkAllAsRead(Guid userId);
        Task<int> Add(Notification notification);
    }
}