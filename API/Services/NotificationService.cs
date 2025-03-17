using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Repositories.Interfaces;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace API.Services
{
    public class NotificationBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(100); // Polling interval

        public NotificationBackgroundService(IServiceScopeFactory scopeFactory, IHubContext<NotificationHub> hubContext)
        {
            _scopeFactory = scopeFactory;
            _hubContext = hubContext;
        }

        protected override async System.Threading.Tasks.Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("✅ Notification Background Service Started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var notificationRepo = scope.ServiceProvider.GetRequiredService<INotificationInterface>();

                        // 🔹 Fetch all unread notifications from the database
                        List<Notification> notifications = await notificationRepo.GetAllUnreadNotifications();

                        if (notifications.Any())
                        {
                            Console.WriteLine($"🔔 {notifications.Count} unread notifications found!");

                            // 🔹 Group notifications by UserId and send them via SignalR
                            var groupedNotifications = notifications.GroupBy(n => n.UserId);

                            foreach (var group in groupedNotifications)
                            {
                                string userId = group.Key.ToString();
                                var userNotifications = group.ToList();

                                Console.WriteLine($"📢 Sending {userNotifications.Count} notifications to User {userId}");

                                // 🔹 Send notifications to the specific user
                                await _hubContext.Clients.All.SendAsync("ReceiveNotifications", notifications);

                                // ✅ Optionally mark notifications as sent (update the DB)
                                // await notificationRepo.MarkNotificationsAsSent(userNotifications);
                            }
                        }
                        else
                        {
                            Console.WriteLine("⏳ No new unread notifications...");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error in NotificationBackgroundService: {ex.Message}");
                }

                // 🔹 Wait before fetching again (polling interval)
                await System.Threading.Tasks.Task.Delay(_interval, stoppingToken);
            }

            Console.WriteLine("⚠ Notification Background Service Stopped");
        }
    }
}
