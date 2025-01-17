using Simplify.Models;

namespace Simplify.Interfaces.Worklayer
{
    public interface IUserService
    {
        Task<List<UserAccount>> Get();
        Task<int> Add(UserAccount user);
        Task<UserAccount> GetById(int? id);
        Task<int> EditCredentials(UserAccount account);
        Task<int> EditInformation(UserAccount account);
        Task UpdatePreferences(int? userId, UserPreferences preferences);
        Task<UserPreferences> GetPreferences(int? userId);
    }
}
