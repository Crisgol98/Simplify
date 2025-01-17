using Simplify.Models;

namespace Simplify.Interfaces.ADO
{
    public interface IUserRepository
    {
        Task<List<UserAccount>> Get();
        Task<int> Add(UserAccount user);
        Task<int> EditCredentials(UserAccount account);
        Task<int> EditInformation(UserAccount account);
        Task<UserPreferences>? GetPreferences(int? userId);
        Task InsertPreferences(int userId, UserPreferences preferences);
        Task UpdatePreferences(int? userId, UserPreferences preferences);
    }
}
