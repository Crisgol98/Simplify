using Microsoft.Extensions.Caching.Memory;
using Simplify.ADO;
using Simplify.Interfaces.ADO;
using Simplify.Interfaces.Worklayer;
using Simplify.Models;
using System.Globalization;

namespace Simplify.WorkLayer
{
    public class TaskService : ITaskService
    {

        private readonly ITaskRepository _taskRepository;
        private readonly IMemoryCache _cache;
        private readonly IHttpContextAccessor _httpContext;

        public TaskService(ITaskRepository taskRepository, IMemoryCache cache, IHttpContextAccessor httpContextAccessor)
        {
            _cache = cache;
            _taskRepository = taskRepository;
            _httpContext = httpContextAccessor;
        }

        public async Task<bool> AddTask(UserTask task)
        {
            try
            {
                int result = await _taskRepository.AddTask(task);
                if (result == 0)
                {
                    return false;
                }
                _cache.Remove("tasks");
                return true;
            }
            catch (Exception e)
            {
                throw;
            }
        }
        public async Task<List<UserTask>> GetTasks()
        {
            if (!_cache.TryGetValue("tasks", out List<UserTask> tasks))
            {
                tasks = await _taskRepository.GetTasks();
                tasks = tasks.Where(t => t.State != null && t.Priority != null).ToList();
                // Guardar en caché
                _cache.Set("tasks", tasks, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
                });
            }
            return tasks;
        }
        public async Task<List<UserTask>> GetTasksByUserId(int userId)
        {
            List<UserTask> tasks = await GetTasks();

            return tasks.Where(t => t.UserId == userId).ToList();
        }
        public async Task<List<WeeklyScheduleSlot>> GenerateWeeklySchedule()
        {
            var tasks = await GetTasks();
            var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1); // Lunes
            var timeSlots = Enumerable.Range(0, 7 * 48) // 7 días, 48 intervalos por día
                .Select(i => new WeeklyScheduleSlot
                {
                    Day = startOfWeek.AddDays(i / 48).Date,
                    StartTime = TimeSpan.FromHours(9) + TimeSpan.FromMinutes((i % 48) * 30),
                    Tasks = new List<UserTask>(),
                    IsBreak = false
                })
                .ToList();

            // Ordenar las tareas por fecha de vencimiento y prioridad
            var orderedTasks = tasks.OrderBy(t => t.DueDate).ThenBy(t => t.Priority).ToList();

            foreach (var task in orderedTasks)
            {
                var remainingDuration = TimeSpan.FromMinutes(Convert.ToDouble(task.RemainingTime));
                var dueDate = task.DueDate;

                TimeSpan timeWorked = TimeSpan.Zero;

                // Asignar tareas dentro de la semana, priorizando los slots disponibles
                foreach (var slot in timeSlots.Where(s => s.Day <= dueDate && remainingDuration > TimeSpan.Zero))
                {
                    if (remainingDuration <= TimeSpan.Zero) break;

                    if (slot.Tasks.Count == 0 && !slot.IsBreak.GetValueOrDefault())
                    {
                        slot.Tasks.Add(task);
                        remainingDuration -= TimeSpan.FromMinutes(30);
                        timeWorked += TimeSpan.FromMinutes(30);

                        // Programar descansos después de 2 horas de trabajo
                        if (timeWorked >= TimeSpan.FromHours(2))
                        {
                            var breakSlot = timeSlots.FirstOrDefault(s =>
                                s.Day == slot.Day &&
                                s.StartTime > slot.StartTime &&
                                s.Tasks.Count == 0 &&
                                !s.IsBreak.GetValueOrDefault());

                            if (breakSlot != null)
                            {
                                breakSlot.IsBreak = true;
                                timeWorked = TimeSpan.Zero;
                            }
                        }
                    }
                }

                // Si aún queda tiempo por asignar después de la fecha de vencimiento, asignar los intervalos restantes
                if (remainingDuration > TimeSpan.Zero)
                {
                    foreach (var slot in timeSlots.Where(s => s.Day > dueDate && remainingDuration > TimeSpan.Zero))
                    {
                        if (slot.Tasks.Count == 0 && !slot.IsBreak.GetValueOrDefault())
                        {
                            slot.Tasks.Add(task);
                            remainingDuration -= TimeSpan.FromMinutes(30);
                            timeWorked += TimeSpan.FromMinutes(30);

                            if (timeWorked >= TimeSpan.FromHours(2))
                            {
                                var breakSlot = timeSlots.FirstOrDefault(s =>
                                    s.Day == slot.Day &&
                                    s.StartTime > slot.StartTime &&
                                    s.Tasks.Count == 0 &&
                                    !s.IsBreak.GetValueOrDefault());

                                if (breakSlot != null)
                                {
                                    breakSlot.IsBreak = true;
                                    timeWorked = TimeSpan.Zero;
                                }
                            }
                        }
                    }
                }
            }

            return timeSlots;
        }

        public async Task<UserTask> GetTaskById(int taskId)
        {
            var tasks = await GetTasks();
            return tasks.FirstOrDefault(t => t.Id == taskId);
        }
        public async Task<bool> EditTask(UserTask newTask)
        {
            int result = await _taskRepository.EditTask(newTask);
            if (result == 0)
            {
                return false;
            }
            _cache.Remove("tasks");
            return true;
        }
    }
}
