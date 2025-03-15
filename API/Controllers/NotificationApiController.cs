using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;
using Repositories.Models;

namespace API.Controllers
{
    [Route("api/notification")]
    [ApiController]
    public class NotificationApiController : ControllerBase
    {
        private readonly INotificationInterface _notification;

        public NotificationApiController(INotificationInterface notification)
        {
            _notification = notification;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetAll(Guid userId)
        {
            var notifications = await _notification.GetAllByUserId(userId);
            return Ok(new { data = notifications });
        }

        [HttpGet("unread-count/{userId}")]
        public async Task<IActionResult> GetUnreadCount(Guid userId)
        {
            var count = await _notification.GetUnreadCount(userId);
            return Ok(new { count });
        }

        [HttpPut("mark-read/{notificationId}")]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            var result = await _notification.MarkAsRead(notificationId);
            if (!result)
                return NotFound(new { message = "Notification not found" });

            return Ok(new { message = "Notification marked as read" });
        }

        [HttpPut("mark-all-read/{userId}")]
        public async Task<IActionResult> MarkAllAsRead(Guid userId)
        {
            var result = await _notification.MarkAllAsRead(userId);
            return Ok(new { message = "All notifications marked as read" });
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] Notification notification)
        {
            var notificationId = await _notification.Add(notification);
            return Ok(new { 
                message = "Notification created successfully",
                notificationId = notificationId
            });
        }
    }
}