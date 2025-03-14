using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;
using Services;

namespace API.Controllers
{
    [ApiController]
    [Route("api/task")]
    public class TaskApiController : ControllerBase
    {
        private readonly ITaskInterface _taskRepo;
        private readonly RedisService _redisService;
                    private readonly ElasticsearchService _elasticService;

        private const int CACHE_DURATION_MINUTES = 30;

        public TaskApiController(ITaskInterface taskInterface, RedisService redisService, ElasticsearchService elasticsearchService)
        {
            _taskRepo = taskInterface;
            _redisService = redisService;
            _elasticService = elasticsearchService;
        }
        #region Elastic search Task Name
        [HttpGet("search/{title}")]
        public async Task<IActionResult> SearchTask(string title)
        {
            var tasks = await _elasticService.SearchTaskByTitleAsync(title);
            return tasks == null || !tasks.Any()
                ? NotFound(new { success = false, message = "No tasks found." })
                : Ok(new { success = true, data = tasks });
        }

        #endregion

        #region Get: Get All
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            const string cacheKey = "taskList";
            var cachedTaskList = await _redisService.GetListAsync<Repositories.Models.Task>(cacheKey);

            if (cachedTaskList == null || cachedTaskList.Count == 0)
            {
                cachedTaskList = await _taskRepo.GetAll();
                if (cachedTaskList != null && cachedTaskList.Any())
                {
                    await _redisService.SetListAsync(cacheKey, cachedTaskList, TimeSpan.FromMinutes(CACHE_DURATION_MINUTES));
                }
            }
            if (cachedTaskList == null)
            {
                // Handle the case when the list is null
                return StatusCode(500, new { message = "There was some error while retrieving tasks." });
            }
            return Ok(new
            {
                success = true,
                message = "Tasks retrieved successfully.",
                data = cachedTaskList
            });
        }
        #endregion


        #region Get: Get One
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne(string id)
        {
            string taskKey = $"task_{id}";
            var cachedTask = await _redisService.GetObjectAsync<Repositories.Models.Task>(taskKey);

            if (cachedTask == null)
            {
                cachedTask = await _taskRepo.GetOne(id);
                if (cachedTask != null)
                {
                    await _redisService.SetObjectAsync(taskKey, cachedTask, TimeSpan.FromMinutes(CACHE_DURATION_MINUTES));
                }
            }
            if (cachedTask == null)
            {
                return NotFound(new { success = false, message = "Task not found." });
            }
            return Ok(new
            {
                success = true,
                message = "Task retrieved successfully.",
                data = cachedTask
            });
        }
        #endregion


        #region Get: Get All By User
        [HttpGet("user/{id}")]
        public async Task<IActionResult> GetByUser(string id)
        {
            string cacheKey = $"userTaskList_{id}";
            var cachedTaskList = await _redisService.GetListAsync<Repositories.Models.Task>(cacheKey);

            if (cachedTaskList == null || cachedTaskList.Count == 0)
            {
                cachedTaskList = await _taskRepo.GetAllByUser(id);
                if (cachedTaskList != null && cachedTaskList.Any())
                {
                    await _redisService.SetListAsync(cacheKey, cachedTaskList, TimeSpan.FromMinutes(CACHE_DURATION_MINUTES));
                }
            }
            if (cachedTaskList == null)
            {
                return StatusCode(500, new
                {
                    message = "There was some error while retrieving tasks for user.",
                });
            }
            return Ok(new
            {
                success = true,
                message = "Tasks for user retrieved successfully.",
                data = cachedTaskList
            });
        }
        #endregion


        #region Post: Create
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] Repositories.Models.Task model)
        {
            int affectedRows = await _taskRepo.Add(model);
            if (affectedRows <= 0)
            {
                return StatusCode(500, new { message = "Failed to add task." });
            }

            // Invalidate relevant caches
            await InvalidateTaskCaches(model.UserId.ToString());
            
            return Ok(new { message = "Task added successfully!" });
        }
        #endregion


        #region Put: Update
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] Repositories.Models.Task task)
        {
            int affectedRows = await _taskRepo.Update(task);
            if (affectedRows <= 0)
            {
                return NotFound(new { message = "Task not found or not updated." });
            }

            // Invalidate relevant caches
            await InvalidateTaskCaches(task.UserId.ToString(), task.TaskId.ToString());

            return Ok(new { message = "Task updated successfully" });
        }
        #endregion


        #region Delete: Delete Task
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new { success = false, message = "Invalid ID." });
            }

            // Get the task before deletion to know which caches to invalidate
            var task = await _taskRepo.GetOne(id);
            if (task == null)
            {
                return NotFound(new { message = "Task not found." });
            }

            int affectedRows = await _taskRepo.Delete(id);
            if (affectedRows <= 0)
            {
                return NotFound(new { message = "Task not found or already deleted." });
            }

            // Invalidate relevant caches
            await InvalidateTaskCaches(task.UserId.ToString(), task.TaskId.ToString());

            return Ok(new { message = "Task deleted successfully!" });
        }
        #endregion

        #region Private Helper Methods
        private async Task InvalidateTaskCaches(string userId, string? taskId = null)
        {
            // Invalidate global task list
            await _redisService.KeyDeleteAsync("taskList");

            // Invalidate user's task list
            await _redisService.KeyDeleteAsync($"userTaskList_{userId}");

            // Invalidate specific task cache if taskId is provided
            if (!string.IsNullOrEmpty(taskId))
            {
                await _redisService.KeyDeleteAsync($"task_{taskId}");
            }
        }
        #endregion
    }
}
