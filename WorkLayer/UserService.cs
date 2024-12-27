using Microsoft.Extensions.Caching.Memory;
using Simplify.Interfaces.ADO;
using Simplify.Interfaces.Worklayer;
using Simplify.Models;

namespace Simplify.WorkLayer
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMemoryCache _cache;
        private readonly IHttpContextAccessor _httpContext;

        public UserService(IUserRepository userRepository, IMemoryCache cache, IHttpContextAccessor httpContextAccessor)
        {
            _cache = cache;
            _userRepository = userRepository;
            _httpContext = httpContextAccessor;
        }
        public async Task<List<UserAccount>> Get()
        {
            if (!_cache.TryGetValue("tasks", out List<UserAccount> tasks))
            {
                tasks = await _userRepository.Get();

                _cache.Set("users", tasks, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
                });
            }
            return tasks;
        }
        public async Task<UserAccount> GetById(int? id)
        {
            var users = await Get();
            return users.FirstOrDefault(u => u.Id == id);
        }
        public async Task<int> EditCredentials(UserAccount account)
        {
            try
            {
                return await _userRepository.EditCredentials(account);
            } catch
            {
                throw;
            }
        }
        public async Task<int> EditInformation(UserAccount account)
        {
            try
            {
                return await _userRepository.EditInformation(account);
            }
            catch
            {
                throw;
            }
        }
        public async Task UpdatePreferences(int? userId, UserPreferences preferences)
        {
            await _userRepository.UpdatePreferences(userId, preferences);
            _cache.Remove("users");
        }
        public async Task<UserPreferences> GetPreferences(int? userId)
        {
            return await _userRepository.GetPreferences(userId);
        }
    }
}
