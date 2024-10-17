using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Threading.Tasks;
using TMS.Data;
using TMS.Models;
using TMS.Services.Interfaces;

namespace TMS.Services.Repository
{
    public class TasksRepo : ITask
    {
        private readonly ApplicationDBContext _context;
        private readonly ILogger<TasksRepo> _logger;
        public TasksRepo(ApplicationDBContext context, ILogger<TasksRepo> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<string> CreateTasksAsync(Tasks tasks)
        {
            try
            {
                var TitleParam = new SqlParameter("@Title", tasks.Title ?? (object)DBNull.Value);
                var DescParam = new SqlParameter("@Description", tasks.Description ?? (object)DBNull.Value);
                var userIdParam = new SqlParameter("@UserId", tasks.UserId.HasValue ? (object)tasks.UserId.Value : DBNull.Value);

                var responseMessageParam = new SqlParameter("@ResponseMessage", SqlDbType.NVarChar, 256) { Direction = ParameterDirection.Output };

                await _context.Database.ExecuteSqlRawAsync(
                   "EXEC [dbo].[CreateTask] @Title, @Description, @UserId, @ResponseMessage OUTPUT",
                   TitleParam, DescParam, userIdParam, responseMessageParam 
                   );

                string ResponseMessage = responseMessageParam.Value == DBNull.Value ? string.Empty : (string)responseMessageParam.Value;

                return ResponseMessage;


            }
            catch (Exception ex)
            {
                return "Internal Server Error :" + ex.Message;
                _logger.LogError(ex.Message);

            }
        }

        public async Task<bool> DeleteTaskAsync(int Taskid)
        {

            var tid = new SqlParameter("@TaskId", Taskid);

            var isDeletedParam = new SqlParameter("@IsDeleted", SqlDbType.Bit)
            {
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC [dbo].[DeleteTask] @TaskId, @IsDeleted OUTPUT", 
                tid, isDeletedParam
                );

            return (bool)isDeletedParam.Value;
        }

        public async Task<IEnumerable<Tasks>> GetTasksAsync()
        {
            

            try
            {
                var tasks = await _context.tasks.FromSqlRaw("EXEC [dbo].[GetAllTasks]").ToListAsync();
                return tasks;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving tasks.");
                throw;
            }
        }

        public async Task<string> UpdateTasksAsync(Tasks tasks, int taskId)
        {
            try
            {
                var titleParam = new SqlParameter("@Title", tasks.Title ?? (object)DBNull.Value);
                var descParam = new SqlParameter("@Description", tasks.Description ?? (object)DBNull.Value);
                var taskIdParam = new SqlParameter("@TaskId", taskId); 

                var responseMessageParam = new SqlParameter("@ResponseMessage", SqlDbType.NVarChar, 256)
                {
                    Direction = ParameterDirection.Output
                };

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC [dbo].[UpdateTask] @TaskId, @Title, @Description, @ResponseMessage OUTPUT",
                    taskIdParam, titleParam, descParam, responseMessageParam
                );

                string responseMessage = responseMessageParam.Value == DBNull.Value ? string.Empty : (string)responseMessageParam.Value;

                return responseMessage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the task.");
                return "Internal Server Error: " + ex.Message;
            }
        }


    }
}
