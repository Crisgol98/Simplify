using Simplify.Models;

namespace Simplify.Interfaces.ADO
{
    public interface ITaskRepository
    {
        Task<List<UserTask>> GetTasks();
        Task<int> AddTask(UserTask task);
        Task<int> EditTask(UserTask task);
    }
}
