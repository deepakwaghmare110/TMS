using TMS.Models;

namespace TMS.Services.Interfaces
{
    public interface ITask
    {
        public Task<IEnumerable<Tasks>> GetTasksAsync();
        public Task<string> CreateTasksAsync(Tasks tasks);
        public Task<string> UpdateTasksAsync(Tasks tasks, int taskId);
        public Task<bool> DeleteTaskAsync(int Taskid);
    }
}
