using Simplify.Models;

namespace Simplify.Interfaces.ADO
{
    public interface ITaskRepository
    {
        Task<List<UserTask>> Get();
        Task<int> Add(UserTask task);
        Task Edit(UserTask task);
        Task Delete(int taskId);
        Task UpdateState(int taskId, string state, int? remainingTime);
    }
}
