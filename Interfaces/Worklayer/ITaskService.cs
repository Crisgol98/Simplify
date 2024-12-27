using Simplify.Models;

namespace Simplify.Interfaces.Worklayer
{
    public interface ITaskService
    {
        Task<List<UserTask>> Get();
        Task<bool> Add(UserTask task);
        Task Edit(UserTask task);
        Task Delete(int taskId);
        Task UpdateState(int taskId, string state, int remainingTime);
        Task<List<UserTask>> GetByUserId(int? userId);
        Task<List<UserTask>> GetCancelledByUserId(int? userId);
        Task<UserTask> GetByTaskId(int taskId);
        Task<List<ScheduleSlot>> GenerateDailySchedule(int userId, DateTime date);
    }
}
