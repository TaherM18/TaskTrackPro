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
        public async Task<IActionResult> SaveNotification(Notification notification)
        {
            if (await _noti.Add(notification) > 1)
            {
                return Ok(await _rabbitMqService.SendMessage(notification));

            }
            else
            {
                return BadRequest(new
                {
                    message = "Something went wrong",
                    data = notification
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetNotification()
        {
            return Ok(await _redis.GetStringAsync("Notifications"));
        }

        [HttpGet]
        [Route("test")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _noti.GetAll());
        }
    }
}
