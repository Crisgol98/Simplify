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

        public async Task<List<UserAccount>> Get()
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
        public async Task<int> Add(UserAccount user)
        {
            try
            {
                using (var con = new SqlConnection(_connectionString))
                {
                    await con.OpenAsync();
                    string checkEmailQuery = "SELECT COUNT(1) FROM Users WHERE Email = @Email";
                    using (var checkEmailCommand = new SqlCommand(checkEmailQuery, con))
                    {
                        checkEmailCommand.Parameters.AddWithValue("@Email", user.Email);
                        int emailExists = (int)(await checkEmailCommand.ExecuteScalarAsync() ?? 0);

                        if (emailExists > 0)
                        {
                            return -1;
                        }
                    }

                    string checkUsernameQuery = "SELECT COUNT(1) FROM Users WHERE Username = @Username";
                    using (var checkUsernameCommand = new SqlCommand(checkUsernameQuery, con))
                    {
                        checkUsernameCommand.Parameters.AddWithValue("@Username", user.Username);
                        int usernameExists = (int)(await checkUsernameCommand.ExecuteScalarAsync() ?? 0);

                        if (usernameExists > 0)
                        {
                            return -2;
                        }
                    }
                    const string insertUserQuery = @"
                    INSERT INTO Users (Name, Email, Password, Username)
                    VALUES (@Name, @Email, @Password, @Username);
                    SELECT SCOPE_IDENTITY();";

                    using (var cmd = new SqlCommand(insertUserQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@Name", user.Name);
                        cmd.Parameters.AddWithValue("@Email", user.Email);
                        cmd.Parameters.AddWithValue("@Password", user.Password);
                        cmd.Parameters.AddWithValue("@Username", user.Username);

                        return (int)(decimal)await cmd.ExecuteScalarAsync();
                    }
                }
            }
            catch
            {
                throw;
            }
        }
        public async Task<int> EditCredentials(UserAccount account)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();


                    string checkQuery = "SELECT COUNT(*) FROM Users WHERE Username = @Username AND Id != @Id";
                    using (var checkCommand = new SqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@Username", account.Username);
                        checkCommand.Parameters.AddWithValue("@Id", account.Id);

                        int existingUserCount = (int)(await checkCommand.ExecuteScalarAsync() ?? 0);
                        if (existingUserCount > 0)
                        {
                            return -1;
                        }
                    }


                    string updateQuery = "UPDATE Users SET Username = @Username, Password = @Password WHERE Id = @Id";
                    using (var updateCommand = new SqlCommand(updateQuery, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@Username", account.Username);
                        updateCommand.Parameters.AddWithValue("@Password", account.Password);
                        updateCommand.Parameters.AddWithValue("@Id", account.Id);
                        return await updateCommand.ExecuteNonQueryAsync();
                    }
                }
            }
            catch
            {
                throw;
            }
        }
        public async Task<int> EditInformation(UserAccount account)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();


                    string checkEmailQuery = "SELECT COUNT(1) FROM Users WHERE Email = @Email AND Id != @Id";
                    using (var checkEmailCommand = new SqlCommand(checkEmailQuery, connection))
                    {
                        checkEmailCommand.Parameters.AddWithValue("@Email", account.Email);
                        checkEmailCommand.Parameters.AddWithValue("@Id", account.Id);
                        int emailExists = (int)await checkEmailCommand.ExecuteScalarAsync();

                        if (emailExists > 0)
                        {
                            return -1;
                        }
                    }


                    string updateQuery = "UPDATE Users SET Name = @Name, Email = @Email WHERE Id = @Id";
                    using (var updateCommand = new SqlCommand(updateQuery, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@Name", account.Name);
                        updateCommand.Parameters.AddWithValue("@Email", account.Email);
                        updateCommand.Parameters.AddWithValue("@Id", account.Id);
                        return await updateCommand.ExecuteNonQueryAsync();
                    }
                }
            }
            catch
            {
                throw;
            }
        }
        public async Task<UserPreferences>? GetPreferences(int? userId)
        {
            var preferences = new UserPreferences
            {
                WorkingHours = new List<TimeRange>()
            };

            try
            {
                using (var con = new SqlConnection(_connectionString))
                {
                    await con.OpenAsync();
                    using (var transaction = con.BeginTransaction())
                    {
                        try
                        {

                            const string preferencesQuery = "SELECT StartTime, EndTime, BreakLength, BreakFrequency FROM UserPreferences WHERE UserId = @UserId";
                            using (var cmd = new SqlCommand(preferencesQuery, con, transaction))
                            {
                                cmd.Parameters.AddWithValue("@UserId", userId);

                                using (var reader = await cmd.ExecuteReaderAsync())
                                {
                                    if (await reader.ReadAsync())
                                    {
                                        preferences.StartTime = reader.IsDBNull(reader.GetOrdinal("StartTime"))
                                            ? TimeSpan.Zero
                                            : reader.GetTimeSpan(reader.GetOrdinal("StartTime"));

                                        preferences.EndTime = reader.IsDBNull(reader.GetOrdinal("EndTime"))
                                            ? TimeSpan.Zero
                                            : reader.GetTimeSpan(reader.GetOrdinal("EndTime"));

                                        preferences.BreakLength = reader.IsDBNull(reader.GetOrdinal("BreakLength"))
                                            ? TimeSpan.Zero
                                            : reader.GetTimeSpan(reader.GetOrdinal("BreakLength"));

                                        preferences.BreakFrequency = reader.IsDBNull(reader.GetOrdinal("BreakFrequency"))
                                            ? TimeSpan.Zero
                                            : reader.GetTimeSpan(reader.GetOrdinal("BreakFrequency"));

                                    }
                                }
                            }


                            int userPreferenceId = 0;
                            const string getUserPreferenceIdQuery = "SELECT Id FROM UserPreferences WHERE UserId = @UserId";
                            using (var cmd = new SqlCommand(getUserPreferenceIdQuery, con, transaction))
                            {
                                cmd.Parameters.AddWithValue("@UserId", userId);
                                var result = await cmd.ExecuteScalarAsync();
                                userPreferenceId = result != DBNull.Value ? Convert.ToInt32(result) : 0;
                            }


                            const string workingHoursQuery = "SELECT Period, Horas FROM UserWorkingHours WHERE UserPreferenceId = @UserPreferenceId";
                            using (var cmd = new SqlCommand(workingHoursQuery, con, transaction))
                            {
                                cmd.Parameters.AddWithValue("@UserPreferenceId", userPreferenceId);

                                using (var reader = await cmd.ExecuteReaderAsync())
                                {
                                    while (await reader.ReadAsync())
                                    {
                                        preferences.WorkingHours.Add(new TimeRange
                                        {
                                            Period = reader.IsDBNull(reader.GetOrdinal("Period")) ? string.Empty : reader.GetString(reader.GetOrdinal("Period")),
                                            Hours = reader.IsDBNull(reader.GetOrdinal("Horas")) ? 0 : reader.GetInt32(reader.GetOrdinal("Horas"))
                                        });
                                    }
                                }
                            }


                            transaction.Commit();
                        }
                        catch
                        {

                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch
            {
                throw;
            }

            return preferences;
        }
        public async Task InsertPreferences(int userId, UserPreferences preferences)
        {
            try
            {
                using (var con = new SqlConnection(_connectionString))
                {
                    await con.OpenAsync();
                    using (var transaction = con.BeginTransaction())
                    {
                        try
                        {

                            const string insertPreferencesQuery = @"
                        INSERT INTO UserPreferences (UserId, StartTime, EndTime, BreakLength, BreakFrequency)
                        VALUES (@UserId, @StartTime, @EndTime, @BreakLength, @BreakFrequency);
                        SELECT SCOPE_IDENTITY();";

                            int userPreferenceId = 0;

                            using (var cmd = new SqlCommand(insertPreferencesQuery, con, transaction))
                            {
                                cmd.Parameters.AddWithValue("@UserId", userId);
                                cmd.Parameters.AddWithValue("@StartTime", preferences.StartTime);
                                cmd.Parameters.AddWithValue("@EndTime", preferences.EndTime);
                                cmd.Parameters.AddWithValue("@BreakLength", preferences.BreakLength);
                                cmd.Parameters.AddWithValue("@BreakFrequency", preferences.BreakFrequency);


                                userPreferenceId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                            }


                            const string insertWorkingHoursQuery = @"
                        INSERT INTO UserWorkingHours (UserPreferenceId, Period, Horas)
                        VALUES (@UserPreferenceId, @Period, @Horas);";

                            foreach (var workingHour in preferences.WorkingHours)
                            {
                                using (var cmd = new SqlCommand(insertWorkingHoursQuery, con, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@UserPreferenceId", userPreferenceId);
                                    cmd.Parameters.AddWithValue("@Period", workingHour.Period);
                                    cmd.Parameters.AddWithValue("@Horas", workingHour.Hours);

                                    await cmd.ExecuteNonQueryAsync();
                                }
                            }


                            transaction.Commit();
                        }
                        catch
                        {

                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch
            {
                throw;
            }
        }
        public async Task UpdatePreferences(int? userId, UserPreferences preferences)
        {
            try
            {
                using (var con = new SqlConnection(_connectionString))
                {
                    await con.OpenAsync();
                    using (var transaction = con.BeginTransaction())
                    {
                        try
                        {

                            const string checkPreferencesQuery = @"
                            SELECT Id FROM UserPreferences WHERE UserId = @UserId";
                            int userPreferenceId = 0;

                            using (var cmd = new SqlCommand(checkPreferencesQuery, con, transaction))
                            {
                                cmd.Parameters.AddWithValue("@UserId", userId);

                                var result = await cmd.ExecuteScalarAsync();
                                userPreferenceId = result != DBNull.Value ? Convert.ToInt32(result) : 0;
                            }

                            if (userPreferenceId == 0)
                            {

                                const string insertPreferencesQuery = @"
                                INSERT INTO UserPreferences (UserId, StartTime, EndTime, BreakLength, BreakFrequency)
                                VALUES (@UserId, @StartTime, @EndTime, @BreakLength, @BreakFrequency);
                                SELECT SCOPE_IDENTITY();";

                                using (var cmd = new SqlCommand(insertPreferencesQuery, con, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@UserId", userId);
                                    cmd.Parameters.AddWithValue("@StartTime", preferences.StartTime);
                                    cmd.Parameters.AddWithValue("@EndTime", preferences.EndTime);
                                    cmd.Parameters.AddWithValue("@BreakLength", preferences.BreakLength);
                                    cmd.Parameters.AddWithValue("@BreakFrequency", preferences.BreakFrequency);


                                    userPreferenceId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                                }
                            }
                            else
                            {

                                const string updatePreferencesQuery = @"
                                UPDATE UserPreferences
                                SET StartTime = @StartTime,
                                    EndTime = @EndTime,
                                    BreakLength = @BreakLength,
                                    BreakFrequency = @BreakFrequency
                                WHERE Id = @UserPreferenceId";

                                using (var cmd = new SqlCommand(updatePreferencesQuery, con, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@UserPreferenceId", userPreferenceId);
                                    cmd.Parameters.AddWithValue("@StartTime", preferences.StartTime);
                                    cmd.Parameters.AddWithValue("@EndTime", preferences.EndTime);
                                    cmd.Parameters.AddWithValue("@BreakLength", preferences.BreakLength);
                                    cmd.Parameters.AddWithValue("@BreakFrequency", preferences.BreakFrequency);

                                    await cmd.ExecuteNonQueryAsync();
                                }
                            }


                            const string deleteWorkingHoursQuery = @"
                            DELETE FROM UserWorkingHours WHERE UserPreferenceId = @UserPreferenceId";

                            using (var cmd = new SqlCommand(deleteWorkingHoursQuery, con, transaction))
                            {
                                cmd.Parameters.AddWithValue("@UserPreferenceId", userPreferenceId);
                                await cmd.ExecuteNonQueryAsync();
                            }


                            const string insertWorkingHoursQuery = @"
                            INSERT INTO UserWorkingHours (UserPreferenceId, Period, Horas)
                            VALUES (@UserPreferenceId, @Period, @Horas);";

                            foreach (var workingHour in preferences.WorkingHours)
                            {
                                using (var cmd = new SqlCommand(insertWorkingHoursQuery, con, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@UserPreferenceId", userPreferenceId);
                                    cmd.Parameters.AddWithValue("@Period", workingHour.Period);
                                    cmd.Parameters.AddWithValue("@Horas", workingHour.Hours);

                                    await cmd.ExecuteNonQueryAsync();
                                }
                            }


                            transaction.Commit();
                        }
                        catch
                        {

                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch
            {
                throw;
            }
        }

    }
}
