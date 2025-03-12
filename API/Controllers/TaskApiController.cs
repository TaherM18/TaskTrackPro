using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;
using Repositories.Models;

namespace API.Controllers
{
    [ApiController]
    [Route("api/task")]
    public class TaskApiController : ControllerBase
    {
        private readonly ITaskInterface _taskRepo;

        public TaskApiController(ITaskInterface taskInterface)
        {
            _taskRepo = taskInterface;
        }

        #region Get: Get All
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var taskList = await _taskRepo.GetAll();
            if (taskList == null)
            {
                return StatusCode(500, new { message = "There was some error while retrieving tasks." });
            }
            return Ok(new
            {
                success = true,
                message = "Tasks retrieved successfully.",
                data = taskList
            });
        }
        #endregion


        #region Get: Get One
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne(string id)
        {
            var task = await _taskRepo.GetOne(id);
            if (task == null)
            {
                return NotFound(new { success = false, message = "Task not found." });
            }
            return Ok(new
            {
                success = true,
                message = "Task retrieved successfully.",
                data = task
            });
        }
        #endregion


        #region Get: Get All By User
        [HttpGet("user/{id}")]
        public async Task<IActionResult> GetByUser(string id)
        {
            var taskList = await _taskRepo.GetAllByUser(id);
            if (taskList == null)
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
                data = taskList
            });
        }
        #endregion


        #region Post: Create
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Repositories.Models.Task model)
        {
            int affectedRows = await _taskRepo.Add(model);
            if (affectedRows <= 0)
            {
                return StatusCode(500, new { message = "Failed to add task." });
            }

            return Ok(new { message = "Task added successfully!" });
        }
        #endregion


        #region Put: Update
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] Repositories.Models.Task model)
        {
            int affectedRows = await _taskRepo.Update(model);
            if (affectedRows <= 0)
            {
                return NotFound(new { message = "Task not found or not updated." });
            }

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


            int affectedRows = await _taskRepo.Delete(id);
            if (affectedRows <= 0)
            {
                return NotFound(new { message = "Task not found or already deleted." });
            }

            return Ok(new { message = "Task deleted successfully!" });
        }
        #endregion
    }
}
