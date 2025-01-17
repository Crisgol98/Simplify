using Microsoft.Extensions.Caching.Memory;
using Simplify.ADO;
using Simplify.Interfaces.ADO;
using Simplify.Interfaces.Worklayer;
using Simplify.Models;
using Simplify.Resources.Utils;
using System.Globalization;
using System.Security.Claims;
using System.Security.Cryptography.Xml;

namespace Simplify.WorkLayer
{
    public class TaskService : ITaskService
    {

        private readonly ITaskRepository _taskRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMemoryCache _cache;
        private readonly IHttpContextAccessor _httpContext;

        public TaskService(ITaskRepository taskRepository, IUserRepository userRepository, IMemoryCache cache, IHttpContextAccessor httpContextAccessor)
        {
            _cache = cache;
            _taskRepository = taskRepository;
            _userRepository = userRepository;
            _httpContext = httpContextAccessor;
        }

        public async Task<bool> Add(UserTask task)
        {
            try
            {
                int result = await _taskRepository.Add(task);
                if (result == 0)
                {
                    return false;
                }
                _cache.Remove($"tasks_${task.UserId}");
                _cache.Remove($"schedule_{task.UserId}");
                return true;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task Edit(UserTask newTask, int? userId)
        {
            await _taskRepository.Edit(newTask);
            _cache.Remove($"tasks_${userId}");
            _cache.Remove($"schedule_{userId}");
        }
        public async Task Delete(int taskId, int? userId)
        {
            await _taskRepository.Delete(taskId);
            _cache.Remove($"tasks_${userId}");
            _cache.Remove($"schedule_{userId}");
        }
        public async Task UpdateState(int taskId, string state, int? remainingTime, int? userId)
        {
            await _taskRepository.UpdateState(taskId, state, remainingTime);
            _cache.Remove($"tasks_${userId}");
            _cache.Remove($"schedule_{userId}");
        }
        public async Task<List<UserTask>> Get(int? userId)
        {
            if (!_cache.TryGetValue($"tasks_${userId}", out List<UserTask> tasks))
            {
                tasks = await _taskRepository.Get();
                tasks = tasks.Where(t => t.State != null && t.Priority != null).ToList();

                _cache.Set($"tasks_${userId}", tasks, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
                });
            }
            return tasks;
        }
        public async Task<UserTask> GetByTaskId(int taskId, int? userId)
        {
            var tasks = await Get(userId);
            return tasks.FirstOrDefault(t => t.Id == taskId);
        }
        public async Task<List<UserTask>> GetByUserId(int? userId)
        {
            List<UserTask> tasks = await Get(userId);

            return tasks.Where(t => t.UserId == userId && t.State != "Cancelado").ToList();
        }
        public async Task<List<UserTask>> GetCancelledByUserId(int? userId)
        {
            List<UserTask> tasks = await Get(userId);
            return tasks.Where(t => t.UserId == userId && t.State == "Cancelado").ToList();
        }

        public async Task<List<ScheduleSlot>> GenerateDailySchedule(int? userId, DateTime date)
        {
            string cacheKey = $"schedule_{userId}";

            if (_cache.TryGetValue(cacheKey, out List<ScheduleSlot> cachedSchedule))
            {
                return cachedSchedule;
            }

            var tasks = await GetByUserId(userId);
            tasks = tasks.Where(t => t.State != "Finalizado").ToList();
            var preferences = await _userRepository.GetPreferences(userId);

            if (preferences == null || preferences.WorkingHours == null || !preferences.WorkingHours.Any())
            {
                preferences.StartTime = new TimeSpan(9, 0, 0);
                preferences.EndTime = new TimeSpan(22, 30, 0);
                preferences.WorkingHours = new List<TimeRange>
                {
                    new TimeRange { Period = "Mañana", Hours = 4 },
                    new TimeRange { Period = "Tarde", Hours = 4 },
                    new TimeRange { Period = "Noche", Hours = 0 }
                };
                preferences.BreakLength = new TimeSpan(0, 30, 0);
                preferences.BreakFrequency = new TimeSpan(2, 0, 0);
            }

            var timeSlots = GenerateTimeSlotsFromPreferences(date, preferences);

            var orderedTasks = tasks
                .OrderBy(t => t.DueDate)
                .ThenBy(t => t.Priority switch
                {
                    "Baja" => 1,
                    "Media" => 2,
                    "Alta" => 3,
                    _ => 0
                }).ThenBy(t => t.RemainingTime)
                .ToList();

            TimeSpan timeWorkedSinceLastBreak = TimeSpan.Zero;
            DateTime? lastTaskEndTime = null;
            ScheduleSlot? lastPeriodLastTask = null;
            ScheduleSlot? lastTask = null;
            string currentPeriod = DetermineCurrentPeriod(DateTime.Now.TimeOfDay);

            foreach (var task in orderedTasks)
            {
                if (task.RemainingTime <= 0) continue;

                var remainingDuration = TimeSpan.FromMinutes(Convert.ToDouble(task.RemainingTime));
                var taskSlots = (int)Math.Ceiling(remainingDuration.TotalMinutes / 30.0);
                var totalTaskMinutes = taskSlots * 30.0;
                var breakSlotsNeeded = (int)Math.Ceiling(totalTaskMinutes / preferences.BreakFrequency.TotalMinutes);
                var slotsNeeded = taskSlots + breakSlotsNeeded;

                var availableSlots = FindAvailableSlots(timeSlots, slotsNeeded);
                if (availableSlots == null) continue;

                foreach (var slot in availableSlots)
                {
                    if (slot.IsBreak.GetValueOrDefault())
                    {
                        continue;
                    }

                    var currentSlotDateTime = date.Date + slot?.StartTime;

                    if (lastTaskEndTime.HasValue)
                    {
                        var timeSinceLastTask = currentSlotDateTime - lastTaskEndTime.GetValueOrDefault();

                        if (lastTask != null && slot != null && lastTask?.Period != slot?.Period && timeSinceLastTask >= TimeSpan.FromHours(1.5))
                        {
                            timeWorkedSinceLastBreak = TimeSpan.Zero;
                        }
                        else if (timeSinceLastTask >= TimeSpan.FromHours(1))
                        {
                        }
                    }

                    if (slot != null && currentPeriod != slot?.Period)
                    {
                        currentPeriod = slot?.Period ?? "Tarde";
                        lastPeriodLastTask = lastTask;
                    }

                    slot?.Tasks.Add(task);

                    if (slot != null && !slot.IsBreak.GetValueOrDefault())
                    {
                        timeWorkedSinceLastBreak += TimeSpan.FromMinutes(30);
                        remainingDuration -= TimeSpan.FromMinutes(30);
                    }

                    if (timeWorkedSinceLastBreak >= preferences.BreakFrequency)
                    {
                        var nextSlot = FindNextAvailableSlot(timeSlots, slot ?? new ScheduleSlot());
                        var secondNextSlot = FindSecondNextAvailableSlot(timeSlots, slot ?? new ScheduleSlot());
                        var isSlotInAnotherPeriod = IsNextSlotInDifferentPeriod(timeSlots, slot ?? new ScheduleSlot());
                        var taskIndex = orderedTasks.IndexOf(task);
                        var isLastTask = taskIndex == orderedTasks.Count -1;
                        var isLastSlotForTask = remainingDuration <= TimeSpan.Zero;
                        if (nextSlot != null && secondNextSlot != null && !isSlotInAnotherPeriod && !(isLastTask && isLastSlotForTask))
                        {
                            nextSlot.IsBreak = true;
                            lastTaskEndTime = date.Date + nextSlot?.StartTime;
                            timeWorkedSinceLastBreak = TimeSpan.Zero;
                        }
                    }

                    if (slot != null && remainingDuration <= TimeSpan.Zero)
                    {
                        lastTaskEndTime = date.Date + slot?.StartTime;
                        break;
                    }
                    lastTask = slot;
                }
            }

            timeSlots = AdjustTimeSlotsForWorkAndBreaks(timeSlots);

            _cache.Set(cacheKey, timeSlots, TimeSpan.FromDays(1));

            return timeSlots;
        }

        private ScheduleSlot? FindSecondNextAvailableSlot(List<ScheduleSlot> slots, ScheduleSlot currentSlot)
        {
            var currentIndex = slots.IndexOf(currentSlot);
            return slots
                .Skip(currentIndex + 2)
                .FirstOrDefault(s => s.Tasks.Count == 0 && !s.IsBreak.GetValueOrDefault());
        }

        private bool IsNextSlotInDifferentPeriod(List<ScheduleSlot> slots, ScheduleSlot currentSlot)
        {
            var currentIndex = slots.IndexOf(currentSlot);
            if (currentIndex == -1 || currentIndex + 1 >= slots.Count)
            {
                return false;
            }

            var nextSlot = slots[currentIndex + 1];

            return currentSlot.Period != nextSlot.Period;
        }


        private ScheduleSlot? FindNextAvailableSlot(List<ScheduleSlot> slots, ScheduleSlot currentSlot)
        {
            var currentIndex = slots.IndexOf(currentSlot);
            return slots
                .Skip(currentIndex + 1)
                .FirstOrDefault(s => s.Tasks.Count == 0 && !s.IsBreak.GetValueOrDefault());
        }

        private string DetermineCurrentPeriod(TimeSpan time)
        {
            if (time >= new TimeSpan(6, 0, 0) && time < new TimeSpan(14, 0, 0))
                return "Mañana";
            if (time >= new TimeSpan(14, 0, 0) && time < new TimeSpan(20, 0, 0))
                return "Tarde";
            return "Noche";
        }

        private List<ScheduleSlot> GenerateTimeSlotsFromPreferences(DateTime date, UserPreferences prefs)
        {
            var slots = new List<ScheduleSlot>();
            var now = DateTime.Now.TimeOfDay;

            string currentPeriod = DetermineCurrentPeriod(now);

            var orderedWorkingHours = OrderWorkingHoursByCurrentPeriod(prefs?.WorkingHours, currentPeriod);

            var currentTime = prefs?.StartTime;
            if (date.Date == DateTime.Today && now > currentTime)
            {
                var totalMinutes = now.TotalMinutes;
                var minutesIntoCurrentSlot = totalMinutes % 30;

                if (minutesIntoCurrentSlot < 15)
                {
                    currentTime = TimeSpan.FromMinutes(Math.Floor(totalMinutes / 30.0) * 30);
                }
                else
                {
                    currentTime = TimeSpan.FromMinutes(Math.Ceiling(totalMinutes / 30.0) * 30);
                }
            }

            foreach (var workingHour in orderedWorkingHours)
            {
                TimeSpan startOfPeriod;
                TimeSpan endOfPeriod;

                switch (workingHour?.Period)
                {
                    case "Mañana":
                        startOfPeriod = new TimeSpan(6, 0, 0);
                        endOfPeriod = new TimeSpan(14, 0, 0);
                        break;
                    case "Tarde":
                        startOfPeriod = new TimeSpan(14, 30, 0);
                        endOfPeriod = new TimeSpan(20, 0, 0);
                        break;
                    case "Noche":
                        startOfPeriod = new TimeSpan(20, 0, 0);
                        endOfPeriod = new TimeSpan(23, 59, 59);
                        break;
                    default:
                        continue;
                }

                if (date.Date == DateTime.Today && now > startOfPeriod)
                {
                    startOfPeriod = currentTime ?? TimeSpan.FromMinutes(DateTime.Now.Minute);
                }

                var maxWorkingTime = startOfPeriod.Add(new TimeSpan(workingHour.Hours, 0, 0));
                endOfPeriod = endOfPeriod > maxWorkingTime ? maxWorkingTime : endOfPeriod;

                while (startOfPeriod <= endOfPeriod && startOfPeriod <= prefs?.EndTime)
                {
                    slots.Add(new ScheduleSlot
                    {
                        Day = date,
                        StartTime = startOfPeriod,
                        Period = workingHour.Period,
                        Tasks = new List<UserTask>(),
                        IsBreak = false
                    });

                    startOfPeriod = startOfPeriod.Add(TimeSpan.FromMinutes(30));
                }
            }

            return slots;
        }
        private List<TimeRange> OrderWorkingHoursByCurrentPeriod(List<TimeRange>? workingHours, string currentPeriod)
        {
            var orderedHours = new List<TimeRange>();

            // Verificamos si workingHours es null
            if (workingHours == null) return orderedHours;  // Si es null, retornamos una lista vacía

            // Definimos los períodos en el orden deseado
            var periods = new[] { "Mañana", "Tarde", "Noche" };

            // Encontramos el índice del período actual
            var currentIndex = Array.IndexOf(periods, currentPeriod);

            if (currentIndex == -1) return orderedHours;  // Si no se encuentra el período, retornamos una lista vacía

            // Agregamos el período actual
            orderedHours.AddRange(workingHours.Where(wh => wh.Period == periods[currentIndex]));

            // Dependiendo del período actual, agregamos los siguientes períodos
            switch (currentPeriod)
            {
                case "Mañana":
                    // Si es "Mañana", agregamos "Tarde" y "Noche"
                    orderedHours.AddRange(workingHours.Where(wh => wh.Period == "Tarde"));
                    orderedHours.AddRange(workingHours.Where(wh => wh.Period == "Noche"));
                    break;
                case "Tarde":
                    // Si es "Tarde", solo agregamos "Noche"
                    orderedHours.AddRange(workingHours.Where(wh => wh.Period == "Noche"));
                    break;
                case "Noche":
                    // Si es "Noche", no agregamos más períodos
                    break;
            }

            return orderedHours;
        }



        private List<ScheduleSlot> AdjustTimeSlotsForWorkAndBreaks(List<ScheduleSlot> timeSlots)
        {
            var adjustedSlots = new List<ScheduleSlot>();
            foreach (var slot in timeSlots)
            {
                if (slot.Tasks.Count > 0 || slot.IsBreak.GetValueOrDefault())
                {
                    adjustedSlots.Add(slot);
                }
            }
            return adjustedSlots;
        }
        private List<ScheduleSlot> FindAvailableSlots(List<ScheduleSlot> allSlots, int slotsNeeded)
        {
            List<ScheduleSlot> availableSlots = new();

            for (int i = 0; i < allSlots.Count; i++)
            {
                var slot = allSlots[i];

                if (slot.Tasks.Count == 0 && !slot.IsBreak.GetValueOrDefault())
                {
                    availableSlots.Add(slot);

                    if (availableSlots.Count == slotsNeeded)
                    {
                        return availableSlots;
                    }
                }
                else
                {
                    availableSlots.Clear();
                }
            }

            return availableSlots;
        }
    }
}
