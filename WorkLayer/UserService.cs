using Microsoft.Extensions.Caching.Memory;
using Simplify.Interfaces.ADO;
using Simplify.Interfaces.Worklayer;
using Simplify.Models;

namespace Simplify.WorkLayer
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _ado;
        private readonly IMemoryCache _cache;
        private readonly IHttpContextAccessor _httpContext;

        public UserService(IUserRepository userRepository, IMemoryCache cache, IHttpContextAccessor httpContextAccessor)
        {
            _cache = cache;
            _ado = userRepository;
            _httpContext = httpContextAccessor;
        }
        public async Task<List<UserAccount>> GetUsers()
        {
            if (!_cache.TryGetValue("tasks", out List<UserAccount> tasks))
            {
                tasks = await _ado.GetUsers();
                // Guardar en caché
                _cache.Set("users", tasks, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
                });
            }
            return tasks;
        }
        public async Task<UserAccount> GetUserById(int id)
        {
            var users = await GetUsers();
            return users.FirstOrDefault(u => u.Id == id);
        }
    }
}
