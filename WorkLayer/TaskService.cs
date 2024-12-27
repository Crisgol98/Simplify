using Microsoft.Extensions.Caching.Memory;
using Simplify.ADO;
using Simplify.Interfaces.ADO;
using Simplify.Interfaces.Worklayer;
using Simplify.Models;
using System.Globalization;
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
                _cache.Remove("tasks");
                return true;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task Edit(UserTask newTask)
        {
            await _taskRepository.Edit(newTask);
            _cache.Remove("tasks");
        }
        public async Task Delete(int taskId)
        {
            await _taskRepository.Delete(taskId);
            _cache.Remove("tasks");
        }
        public async Task<List<UserTask>> Get()
        {
            if (!_cache.TryGetValue("tasks", out List<UserTask> tasks))
            {
                tasks = await _taskRepository.Get();
                tasks = tasks.Where(t => t.State != null && t.Priority != null).ToList();

                _cache.Set("tasks", tasks, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
                });
            }
            return tasks;
        }
        public async Task<UserTask> GetByTaskId(int taskId)
        {
            var tasks = await Get();
            return tasks.FirstOrDefault(t => t.Id == taskId);
        }
        public async Task<List<UserTask>> GetByUserId(int? userId)
        {
            List<UserTask> tasks = await Get();

            return tasks.Where(t => t.UserId == userId && t.State != "Cancelado").ToList();
        }
        public async Task<List<UserTask>> GetCancelledByUserId(int? userId)
        {
            List<UserTask> tasks = await Get();
            return tasks.Where(t => t.UserId == userId && t.State == "Cancelado").ToList();
        }

        public async Task<List<ScheduleSlot>> GenerateDailySchedule(int userId, DateTime date)
        {
            var tasks = await GetByUserId(userId);
            var preferences = await _userRepository.GetPreferences(userId);


            if (preferences == null || preferences.WorkingHours == null || !preferences.WorkingHours.Any())
            {
                preferences.StartTime = new TimeSpan(9, 0, 0);
                preferences.EndTime = new TimeSpan(22, 30, 0);

                preferences.WorkingHours = new List<TimeRange>();

                preferences.WorkingHours.Add(new TimeRange
                {
                    Period = "Mañana",
                    Hours = 4
                });

                preferences.WorkingHours.Add(new TimeRange
                {
                    Period = "Tarde",
                    Hours = 4
                });

                preferences.WorkingHours.Add(new TimeRange
                {
                    Period = "Noche",
                    Hours = 0
                });

                preferences.BreakLength = new TimeSpan(0, 30, 0);
                preferences.BreakFrequency = new TimeSpan(2, 0, 0);
            }
            var timeSlots = GenerateTimeSlotsFromPreferences(date, preferences);
            // Ordenar las tareas por prioridad y utilizando una formula para calcular un peso dependiendo
            // del remainingTime y la prioridad de la tarea
            var orderedTasks = tasks
                .OrderBy(t => t.DueDate)
                .ThenBy(t => t.Priority switch
                {
                    "Baja" => 1,
                    "Media" => 2,
                    "Alta" => 3,
                    _ => 0
                })
                .ToList();

            TimeSpan timeWorkedSinceLastBreak = TimeSpan.Zero;
            DateTime? lastBreakTime = null;
            ScheduleSlot? lastPeriodLastTask = null;
            ScheduleSlot? lastTask = null;
            string currentPeriod = preferences.WorkingHours.FirstOrDefault(wh => wh.Hours > 0).Period;
            bool needsBreakAfterPeriodChange = false;

            foreach (var task in orderedTasks)
            {
                if (task.RemainingTime <= 0) continue;

                var remainingDuration = TimeSpan.FromMinutes(Convert.ToDouble(task.RemainingTime));
                var slotsNeeded = (int)Math.Ceiling(remainingDuration.TotalMinutes / 30.0);

                var availableSlots = FindAvailableSlots(timeSlots, slotsNeeded);
                if (availableSlots == null) continue;

                foreach (var slot in availableSlots)
                {

                    if (currentPeriod != slot.Period)
                    {
                        if (!string.IsNullOrEmpty(currentPeriod))
                        {
                            needsBreakAfterPeriodChange = timeWorkedSinceLastBreak >= preferences.BreakFrequency;
                        }
                        currentPeriod = slot.Period;

                        if (needsBreakAfterPeriodChange)
                        {
                            var firstSlotOfPeriod = timeSlots.FirstOrDefault(s =>
                                s.Period == currentPeriod &&
                                s.Tasks.Count == 0 &&
                                !s.IsBreak.GetValueOrDefault());

                            if (firstSlotOfPeriod != null)
                            {
                                firstSlotOfPeriod.IsBreak = true;
                                lastBreakTime = date.Date + firstSlotOfPeriod.StartTime;
                                timeWorkedSinceLastBreak = TimeSpan.Zero;
                                needsBreakAfterPeriodChange = false;
                                continue;
                            }
                        }
                        lastPeriodLastTask = lastTask;
                    }


                    if (needsBreakAfterPeriodChange && slot.Tasks.Count == 0)
                    {
                        slot.IsBreak = true;
                        lastBreakTime = date.Date + slot.StartTime;
                        timeWorkedSinceLastBreak = TimeSpan.Zero;
                        needsBreakAfterPeriodChange = false;
                        continue;
                    }


                    slot.Tasks.Add(task);


                    if (!slot.IsBreak.GetValueOrDefault())
                    {
                        timeWorkedSinceLastBreak += TimeSpan.FromMinutes(30);
                        remainingDuration -= TimeSpan.FromMinutes(30);
                    }


                    if (timeWorkedSinceLastBreak >= preferences.BreakFrequency)
                    {
                        var nextSlot = FindNextAvailableSlot(timeSlots, slot);
                        if (nextSlot != null)
                        {
                            var currentSlotDateTime = date.Date + slot.StartTime;
                            var nextSlotDateTime = date.Date + nextSlot.StartTime;

                            bool canTakeBreak = true;
                            if (lastBreakTime.HasValue || lastPeriodLastTask != null && lastPeriodLastTask.StartTime > lastPeriodLastTask.StartTime + (nextSlot.StartTime + TimeSpan.FromHours(1.5)))
                            {
                                var timeSinceLastBreak = currentSlotDateTime - lastBreakTime.GetValueOrDefault();
                                canTakeBreak = timeSinceLastBreak >= preferences.BreakFrequency && timeSinceLastBreak < preferences.BreakFrequency + TimeSpan.FromHours(2);
                            }


                            if (canTakeBreak &&
                                (nextSlotDateTime - currentSlotDateTime) <= preferences.BreakLength)
                            {
                                nextSlot.IsBreak = true;
                                lastBreakTime = nextSlotDateTime;
                                timeWorkedSinceLastBreak = TimeSpan.Zero;
                            }
                        }
                    }


                    if (remainingDuration <= TimeSpan.Zero)
                    {
                        break;
                    }
                    lastTask = slot;
                }
            }


            timeSlots = AdjustTimeSlotsForWorkAndBreaks(timeSlots);
            return timeSlots;
        }



        private ScheduleSlot? FindNextAvailableSlot(List<ScheduleSlot> slots, ScheduleSlot currentSlot)
        {
            var currentIndex = slots.IndexOf(currentSlot);
            return slots
                .Skip(currentIndex + 1)
                .FirstOrDefault(s => s.Tasks.Count == 0 && !s.IsBreak.GetValueOrDefault());
        }


        private List<ScheduleSlot> GenerateTimeSlotsFromPreferences(DateTime date, UserPreferences prefs)
        {
            var slots = new List<ScheduleSlot>();
            var currentTime = prefs.StartTime;


            foreach (var workingHour in prefs.WorkingHours)
            {

                TimeSpan startOfPeriod = currentTime;
                TimeSpan endOfPeriod = new TimeSpan();


                if (workingHour.Period == "Mañana")
                {
                    endOfPeriod = startOfPeriod.Add(new TimeSpan(workingHour.Hours, 0, 0));
                    endOfPeriod = (endOfPeriod > new TimeSpan(14, 0, 0)) ? new TimeSpan(14, 0, 0) : endOfPeriod;
                }

                else if (workingHour.Period == "Tarde")
                {

                    startOfPeriod = new TimeSpan(14, 0, 0);
                    endOfPeriod = startOfPeriod.Add(new TimeSpan(workingHour.Hours, 0, 0));
                    endOfPeriod = (endOfPeriod > new TimeSpan(20, 0, 0)) ? new TimeSpan(20, 0, 0) : endOfPeriod;
                }

                else if (workingHour.Period == "Noche")
                {

                    startOfPeriod = new TimeSpan(20, 0, 0);
                    endOfPeriod = startOfPeriod.Add(new TimeSpan(workingHour.Hours, 0, 0));
                    endOfPeriod = (endOfPeriod > prefs.EndTime) ? prefs.EndTime : endOfPeriod;
                }


                while (startOfPeriod < endOfPeriod)
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


                currentTime = endOfPeriod;
            }

            return slots;
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
        public async Task UpdateState(int userId, string state, int remainingTime)
        {
            await _taskRepository.UpdateState(userId, state, remainingTime);
            _cache.Remove("tasks");
        }
    }
}
