using Microsoft.AspNetCore.Mvc;
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

        public NotificationApiController(RabbitMqService rabbitMqService, RedisService redisService)
        {
            _rabbitMqService = rabbitMqService;
            _redis = redisService;
        }

        [HttpPost]
        public async Task<int> SaveNotification(Notification notification)
        {
            return await _rabbitMqService.SendMessage(notification);
        }

        [HttpGet]
        public async Task<string> GetNotification()
        {
            return await _redis.GetStringAsync("Notifications");
        }
    }
}
