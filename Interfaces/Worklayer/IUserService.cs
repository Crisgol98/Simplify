using Simplify.Models;

namespace Simplify.Interfaces.Worklayer
{
    public interface IUserService
    {
        Task<List<UserAccount>> GetUsers();
        Task<UserAccount> GetUserById(int id);
    }
}
