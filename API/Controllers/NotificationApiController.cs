using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;
using Repositories.Models;
using Services;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    public class NotificationApiController : ControllerBase
    {
        private readonly RabbitMqService _rabbitMqService;
        private readonly RedisService _redis;
        private readonly INotificationInterface _noti;

        public NotificationApiController(RabbitMqService rabbitMqService, RedisService redisService, INotificationInterface notification)
        {
            _rabbitMqService = rabbitMqService;
            _redis = redisService;
            _noti = notification;
        }

        [HttpPost]
        public async Task<int> SaveNotification(Notification notification)
        {
            await _noti.Add(notification);
            return await _rabbitMqService.SendMessage(notification);
        }

        [HttpGet]
        public async Task<string> GetNotification()
        {
            return await _redis.GetStringAsync("Notifications");
        }

        [HttpGet]
        [Route("test")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _noti.GetAll());
        }
    }
}
