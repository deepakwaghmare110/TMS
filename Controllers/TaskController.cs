using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Threading.Tasks;
using TMS.Data;
using TMS.Models;
using TMS.Services.Interfaces;
using TMS.Services.Repository;

namespace TMS.Controllers
{
    [Route("task")]
    [Authorize]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly ILogger<TaskController> _logger;
        private readonly ITask _taskRepo;
        private readonly ICache _cache;
        private readonly ApplicationDBContext _context;
        public TaskController(ILogger<TaskController> log, ITask task, ICache cache, ApplicationDBContext dBContext)
        {
            _logger = log;
            _taskRepo = task;
            _cache = cache;
            _context = dBContext;
        }

        [HttpPost]
        public async Task<IActionResult> AddTask([FromBody] Tasks tasksDTO) 
        {
            if (tasksDTO is null)
            {
                return BadRequest("Task data is required.");
            }
            if (string.IsNullOrEmpty(tasksDTO.Title))
            {
                return BadRequest("Task title is required.");
            }
            if (tasksDTO.UserId == null || tasksDTO.UserId <= 0)
            {
                return BadRequest("User id is invalid or empty.");
            }
            if (string.IsNullOrEmpty(tasksDTO.Description))
            {
                return BadRequest("Task description is required.");
            }
            Tasks tasks = new()
            {
                Title = tasksDTO.Title,
                UserId = tasksDTO.UserId,
                Description = tasksDTO.Description
            };

            try
            {
                var response = await _taskRepo.CreateTasksAsync(tasks);

                if (response is not null)
                {
                    return Ok(response);
                }
                else
                {
                    return StatusCode(500, "An unexpected error occurred.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error :"+ex.Message);

            }

        }

        [HttpPut]
        public async Task<IActionResult> UpdateTask([FromBody] Tasks tasksDTO, [FromQuery] string taskId)
        {
            if (string.IsNullOrEmpty(taskId))
            {
                return BadRequest("Task id is required.");
            }
            if (tasksDTO is null)
            {
                return BadRequest("Task data is required.");
            }
            if (string.IsNullOrEmpty(tasksDTO.Title))
            {
                return BadRequest("Task title is required.");
            }
            if (string.IsNullOrEmpty(tasksDTO.Description))
            {
                return BadRequest("Task description is required.");
            }
            Tasks tasks = new()
            {
                Title = tasksDTO.Title,
                Description = tasksDTO.Description
            };
            int tid = Int32.Parse(taskId);
            try
            {
                var response = await _taskRepo.UpdateTasksAsync(tasks, tid);

                if (response is not null)
                {
                    return Ok(response);
                }
                else
                {
                    return StatusCode(500, "An unexpected error occurred while updating task.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred.");
                _logger.LogInformation(ex.Message);

            }

        }


        [HttpGet]
        public async Task<IActionResult> GetTasks()
        {
            try
            {
                var cacheData = _cache.GetData<IEnumerable<Tasks>>("Tasks");
                if (cacheData != null && cacheData.Count() > 0)
                {
                    return Ok(cacheData);
                }

                //cacheData = await _context.users.ToListAsync();
                cacheData = await _taskRepo.GetTasksAsync();

                var expiryTime = DateTimeOffset.Now.AddSeconds(60);
                _cache.SetData<IEnumerable<Tasks>>("Tasks", cacheData, expiryTime);
                return Ok(cacheData);
            }
            catch (Exception ex)
            {

                return StatusCode(500, "An unexpected error occurred while fetching Tasks. ");
                _logger.LogInformation(ex.Message);
            }

        }

        [HttpDelete]
        public async Task<IActionResult> deleteTask([FromQuery] string taskId)
        {
            if(string.IsNullOrEmpty(taskId))
            {
                return BadRequest("Task id is required.");
            }
            try 
            {
                int tId = Int32.Parse(taskId);

                var response = await _taskRepo.DeleteTaskAsync(tId);

                if(response)
                {
                    return Ok("Task deleted.");
                }else
                {
                    return StatusCode(404, "Task id not found");
                }

            }
            catch(Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred while deleting Tasks. ");
                _logger.LogInformation(ex.Message);
            }

        }
    }
}
