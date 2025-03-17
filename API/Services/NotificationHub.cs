using Microsoft.AspNetCore.SignalR;
using Repositories.Interfaces;
using Repositories.Models;
using System.Threading.Tasks;

public class NotificationHub : Hub
{
    private readonly INotificationInterface _noti;

    public NotificationHub(INotificationInterface n)
    {
        _noti = n;
    }

    // public async System.Threading.Tasks.Task FetchNotifications(string userId)
    // {
    //     System.Console.WriteLine("IN notification hub :: " + userId);
    //     var notifications = await _noti.GetAllUnreadByUserId(Guid.Parse(userId));
    //     await Clients.Caller.SendAsync("ReceiveNotifications", notifications); 
    // }

    public async System.Threading.Tasks.Task FetchNotifications(string userId)
    {
        List<Notification> notifications = await _noti.GetAllUnreadByUserId(Guid.Parse(userId));
        Console.WriteLine("Fetch :: " + string.Join(", ", notifications.Select(n => n.ToString())));
        // Console.WriteLine("Fetch :: " + notifications[0].Title);
        await Clients.User(userId).SendAsync("ReceiveNotifications", notifications);
    }

    // ✅ Add a method to broadcast notifications in real-time
    public async System.Threading.Tasks.Task SendNotificationToUser(string userId)
    {
        var notifications = await _noti.GetAllUnreadByUserId(Guid.Parse(userId));
        Console.WriteLine("Send :: " + notifications.ToString());
        await Clients.User(userId).SendAsync("ReceiveNotifications", notifications);
    }
}
