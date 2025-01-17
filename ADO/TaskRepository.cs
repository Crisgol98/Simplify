using Microsoft.Data.SqlClient;
using Simplify.Interfaces.ADO;
using Simplify.Models;
using System.Data;

namespace Simplify.ADO
{
    public class TaskRepository : ITaskRepository
    {

        private readonly string _connectionString;
        public TaskRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<UserTask>> Get()
        {
            var tasks = new List<UserTask>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                await con.OpenAsync();

                string query = @"
                SELECT
                    t.Id,
                    t.Name,
                    ts.Name AS State,
                    tp.Name AS Priority,
                    t.Description,
                    t.CreatedAt,
                    t.DueDate,
                    t.EstimatedTime,
                    t.RemainingTime,
                    t.UserId
                FROM Tasks t
                INNER JOIN TaskStates ts ON t.State = ts.Id
                INNER JOIN TaskPriorities tp ON t.Priority = tp.Id";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var task = new UserTask
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.IsDBNull(1) ? null : reader.GetString(1),
                                State = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                Priority = reader.IsDBNull(3) ? null : reader.GetString(3),
                                Description = reader.IsDBNull(4) ? "" : reader.GetString(4),
                                CreatedAt = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                                DueDate = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
                                EstimatedTime = reader.IsDBNull(7) ? null : reader.GetInt32(7),
                                RemainingTime = reader.IsDBNull(8) ? null : reader.GetInt32(8),
                                UserId = reader.GetInt32(9)
                            };
                            tasks.Add(task);
                        }
                    }
                }
            }

            return tasks;
        }

        public async Task<int> Add(UserTask task)
        {
            try
            {
                int result = 0;
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    await con.OpenAsync();
                    string query = "INSERT INTO Tasks (UserId, Name, Description, Priority, State, DueDate, EstimatedTime, RemainingTime) " +
                                   "VALUES (@UserId, @Name, @Description, @Priority, @State, @DueDate, @EstimatedTime, @RemainingTime)";
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText = query;
                        cmd.Parameters.AddWithValue("@UserId", task.UserId);
                        cmd.Parameters.AddWithValue("@Name", task.Name ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Description", task.Description ?? "");
                        cmd.Parameters.AddWithValue("@Priority", task.Priority ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@State", 1);
                        cmd.Parameters.AddWithValue("@DueDate", task.DueDate ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@EstimatedTime", task.EstimatedTime ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@RemainingTime", task.EstimatedTime ?? (object)DBNull.Value);
                        result = await cmd.ExecuteNonQueryAsync();
                        con.Close();
                        con.Dispose();
                    }
                }
                return result;
            }
            catch
            {
                throw;
            }
        }
        public async Task Edit(UserTask newTask)
        {
            try
            {
                int result = 0;
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    await con.OpenAsync();
                    string query = "UPDATE Tasks SET Name = @Name, Description = @Description, " +
                                   "Priority = @Priority, EstimatedTime = @EstimatedTime, " +
                                   "RemainingTime = @RemainingTime, DueDate = @DueDate WHERE Id = @Id";
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText = query;
                        cmd.Parameters.AddWithValue("@Name", newTask.Name ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Description", newTask.Description ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Priority", newTask.Priority ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@EstimatedTime", newTask.EstimatedTime ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@RemainingTime", newTask.RemainingTime ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@DueDate", newTask.DueDate ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Id", newTask.Id);

                        result = await cmd.ExecuteNonQueryAsync();
                        con.Close();
                        con.Dispose();
                    }
                }
            }
            catch
            {
                throw;
            }
        }
        public async Task Delete(int taskId)
        {
            try
            {
                int result = 0;
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    await con.OpenAsync();
                    string query = "UPDATE Tasks SET State = 3 WHERE Id = @Id";
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText = query;
                        cmd.Parameters.AddWithValue("@Id", taskId);
                        await cmd.ExecuteNonQueryAsync();
                        con.Close();
                        con.Dispose();
                    }
                }
            }
            catch
            {
                throw;
            }
        }
        public async Task UpdateState(int taskId, string state, int? remainingTime)
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
                            const string getStateIdQuery = "SELECT Id FROM TaskStates WHERE Name = @State";
                            int stateId = 0;

                            using (var cmd = new SqlCommand(getStateIdQuery, con, transaction))
                            {
                                cmd.Parameters.AddWithValue("@State", state);

                                using (var reader = await cmd.ExecuteReaderAsync())
                                {
                                    if (await reader.ReadAsync())
                                    {
                                        stateId = reader.GetInt32(0);
                                    }
                                    else
                                    {
                                        throw new Exception($"El estado '{state}' no se encontró en la base de datos.");
                                    }
                                }
                            }
                            const string updateTaskQuery = @"
                        UPDATE Tasks
                        SET State = @StateId,
                            RemainingTime = RemainingTime - @RemainingTime
                        WHERE Id = @TaskId";

                            using (var cmd = new SqlCommand(updateTaskQuery, con, transaction))
                            {
                                cmd.Parameters.AddWithValue("@TaskId", taskId);
                                cmd.Parameters.AddWithValue("@StateId", stateId);
                                cmd.Parameters.AddWithValue("@RemainingTime", remainingTime ?? 0);

                                await cmd.ExecuteNonQueryAsync();
                            }
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }



    }
}
