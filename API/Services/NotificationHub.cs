using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class NotificationHub : Hub
{
    public async Task SendNotification(string userId, object notification)
    {
        System.Console.WriteLine("IN notification hub :: " + userId);
        await Clients.All.SendAsync("ReceiveNotification", notification);
    }
}
