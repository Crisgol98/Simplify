using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Simplify.Interfaces.ADO;
using Simplify.Interfaces.Worklayer;
using Simplify.Models;
using Simplify.Resources.Utils;

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
        public async Task<int> Add(UserAccount user)
        {
            try
            {
                if (string.IsNullOrEmpty(user.Password))
                {
                    throw new ArgumentException("La contraseña no puede estar vacía");
                }

                user.Password = Utils.HashPassword(user.Password);

                int result = await _userRepository.Add(user);
                if (result >= 0)
                {
                    _cache.Remove("users");
                }
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<UserAccount> GetById(int? id)
        {
            var users = await Get();
            return users.FirstOrDefault(u => u.Id == id);
        }
        public async Task<int> EditCredentials(UserAccount account)
        {
            int result = await _userRepository.EditCredentials(account);
            if (result >= 0)
            {
                _cache.Remove("users");
            }
            return result;
        }
        public async Task<int> EditInformation(UserAccount account)
        {
            int result = await _userRepository.EditInformation(account);
            if (result >= 0)
            {
                _cache.Remove("users");
            }
            return result;
        }
        public async Task UpdatePreferences(int? userId, UserPreferences preferences)
        {
            await _userRepository.UpdatePreferences(userId, preferences);
            _cache.Remove("users");
            _cache.Remove($"tasks_${userId}");
            _cache.Remove($"schedule_{userId}");
        }
        public async Task<UserPreferences> GetPreferences(int? userId)
        {
            return await _userRepository.GetPreferences(userId);
        }
    }
}
