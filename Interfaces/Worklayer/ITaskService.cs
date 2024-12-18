using Simplify.Models;

namespace Simplify.Interfaces.Worklayer
{
    public interface ITaskService
    {
        Task<List<UserTask>> GetTasks();
        Task<List<UserTask>> GetTasksByUserId(int userId);
        Task<UserTask> GetTaskById(int taskId);
        Task<List<WeeklyScheduleSlot>> GenerateWeeklySchedule();
        Task<bool> AddTask(UserTask task);
        Task<bool> EditTask(UserTask task);
    }
}
