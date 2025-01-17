using Simplify.Models;

namespace Simplify.Interfaces.Worklayer
{
    public interface ITaskService
    {
        Task<List<UserTask>> Get(int? userId);
        Task<bool> Add(UserTask task);
        Task Edit(UserTask task, int? userId);
        Task Delete(int taskId, int? userId);
        Task UpdateState(int taskId, string state, int? remainingTime, int? userId);
        Task<List<UserTask>> GetByUserId(int? userId);
        Task<List<UserTask>> GetCancelledByUserId(int? userId);
        Task<UserTask> GetByTaskId(int taskId, int? userId);
        Task<List<ScheduleSlot>> GenerateDailySchedule(int? userId, DateTime date);
    }
}
