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
        private static readonly ConcurrentDictionary<string, string> _userConnectionMap = new ConcurrentDictionary<string, string>();

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
                
                // Store connection in both Redis and memory
                await db.HashSetAsync("UserConnections", userId, Context.ConnectionId);
                _userConnectionMap.AddOrUpdate(userId, Context.ConnectionId, (key, oldValue) => Context.ConnectionId);
                
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
                
                // Remove connection from both Redis and memory
                await db.HashDeleteAsync("UserConnections", userId);
                _userConnectionMap.TryRemove(userId, out _);
                
                await db.HashSetAsync("UserStatus", userId, "offline");
                
                await Clients.Others.SendAsync("UserOffline", userId);
                Console.WriteLine($"User {userId} disconnected");
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task<bool> CheckUserStatus(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return false;
            
            var db = _redis.GetDatabase();
            var status = await db.HashGetAsync("UserStatus", userId);
            
            return status.HasValue && status.ToString() == "online";
        }

        public async System.Threading.Tasks.Task SendMessageToUser(string receiverId, Chat message)
        {
            try
            {
                var db = _redis.GetDatabase();
                string? connectionId = null;
                bool isReceiverOnline = false;
                
                // Check if user is online in both Redis and memory
                var redisConnectionId = await db.HashGetAsync("UserConnections", receiverId);
                if (redisConnectionId.HasValue && !string.IsNullOrEmpty(redisConnectionId))
                {
                    isReceiverOnline = true;
                    connectionId = redisConnectionId;
                }
                else if (_userConnectionMap.TryGetValue(receiverId, out string? cachedConnectionId))
                {
                    isReceiverOnline = true;
                    connectionId = cachedConnectionId;
                }
                
                if (isReceiverOnline && !string.IsNullOrEmpty(connectionId))
                {
                    // Try to send directly to the client first
                    try 
                    {
                        await Clients.Client(connectionId).SendAsync("ReceiveMessage", message);
                        
                        // Also send to the user's group as a fallback
                        await Clients.Group(receiverId).SendAsync("ReceiveMessage", message);
                        
                        // Update the sender with the delivery status
                        await Clients.User(message.SenderId.ToString()).SendAsync("MessageStatus", message.ChatId, "delivered");
                        await Clients.Client(Context.ConnectionId).SendAsync("MessageStatus", message.ChatId, "delivered");
                        
                        Console.WriteLine($"Message sent via SignalR from {message.SenderId} to {receiverId}");
                    }
                    catch (Exception ex)
                    {
                        // If direct client send fails, try group send as fallback
                        await Clients.Group(receiverId).SendAsync("ReceiveMessage", message);
                        Console.WriteLine($"Direct client send failed, using group fallback: {ex.Message}");
                    }
                }
                else
                {
                    // User is offline, queue the message for later delivery
                    Console.WriteLine($"Receiver {receiverId} is offline, message queued");
                    
                    // Update the sender that the message is sent but not delivered
                    await Clients.User(message.SenderId.ToString()).SendAsync("MessageStatus", message.ChatId, "sent");
                    await Clients.Client(Context.ConnectionId).SendAsync("MessageStatus", message.ChatId, "sent");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message via SignalR: {ex.Message}");
            }
        }

        // Add a new method to acknowledge messages
        public async System.Threading.Tasks.Task AcknowledgeMessage(int chatId, string senderId)
        {
            try
            {
                // Mark as read in database
                await _chatRepository.MarkChatAsRead(chatId);
                
                // Notify the sender that their message was read
                await Clients.User(senderId).SendAsync("MessageStatus", chatId, "read");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error acknowledging message: {ex.Message}");
            }
        }
    }
}