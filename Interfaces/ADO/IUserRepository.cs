using Simplify.Models;

namespace Simplify.Interfaces.ADO
{
    public interface IUserRepository
    {
        Task<List<UserAccount>> GetUsers();
    }
}
