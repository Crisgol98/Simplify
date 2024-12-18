using Microsoft.Data.SqlClient;
using Simplify.Interfaces.ADO;
using Simplify.Models;

namespace Simplify.ADO
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;
        public UserRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<UserAccount>> GetUsers()
        {
            var users = new List<UserAccount>();
            try
            {
                using (var con = new SqlConnection(_connectionString))
                {
                    const string query = "SELECT Id, Username, Name, Email, Password FROM Users";
                    await con.OpenAsync();
                    using (var cmd = new SqlCommand(query, con))
                    {
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                users.Add(new UserAccount
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    Username = reader.GetString(reader.GetOrdinal("Username")).Trim(),
                                    Name = reader.GetString(reader.GetOrdinal("Name")).Trim(),
                                    Email = reader.GetString(reader.GetOrdinal("Email")).Trim(),
                                    Password = reader.GetString(reader.GetOrdinal("Password")).Trim()
                                });
                            }
                        }
                    }
                }
            }
            catch
            {
                throw;
            }

            return users;
        }
    }
}
