using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using Repositories.Interfaces;
using Repositories.Models;
using StackExchange.Redis;

namespace API.Services
{
    public class ChatHub : Hub
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IChatInterface _chatRepository;

        public ChatHub(IConnectionMultiplexer redis, IChatInterface chatRepository)
        {
            _redis = redis;
            _chatRepository = chatRepository;
        }

        public override async System.Threading.Tasks.Task OnConnectedAsync()
        {
            var userId = Context.GetHttpContext()?.Request.Query["userId"].ToString();
            if (!string.IsNullOrEmpty(userId))
            {
                var db = _redis.GetDatabase();
                await db.HashSetAsync("UserConnections", userId, Context.ConnectionId);
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
                await db.HashSetAsync("UserStatus", userId, "online");
                
                await Clients.Others.SendAsync("UserOnline", userId);
                Console.WriteLine($"User {userId} connected with Connection ID: {Context.ConnectionId}");
            }
            await base.OnConnectedAsync();
        }

        public override async System.Threading.Tasks.Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.GetHttpContext()?.Request.Query["userId"].ToString();
            if (!string.IsNullOrEmpty(userId))
            {
                var db = _redis.GetDatabase();
                await db.HashDeleteAsync("UserConnections", userId);
                await db.HashSetAsync("UserStatus", userId, "offline");
                
                await Clients.Others.SendAsync("UserOffline", userId);
                Console.WriteLine($"User {userId} disconnected");
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async System.Threading.Tasks.Task SendMessageToUser(string receiverId, Chat message)
        {
            try
            {
                // Send only to the specific user
                await Clients.User(receiverId).SendAsync("ReceiveMessage", message);
                
                // Don't send back to sender
                if (Context.UserIdentifier != message.SenderId.ToString())
                {
                    await Clients.User(message.SenderId.ToString()).SendAsync("MessageStatus", message.ChatId, "delivered");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
            }
        }
    }
}