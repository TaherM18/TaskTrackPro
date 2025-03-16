using Repositories.Models;

namespace Repositories.Interfaces
{
    public interface INotificationInterface
    {
        public Task<List<Notification>> GetAll();
        public Task<List<Notification>> GetAllByUser(int uid);
        public Task<int> Add(Notification notification);
        public Task<int> Seen(int id);
    }
}