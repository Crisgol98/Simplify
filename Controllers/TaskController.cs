using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Simplify.Interfaces.Worklayer;
using Simplify.Models;
using System.Globalization;
using System.Security.Claims;
using Simplify.Resources.Utils;
using System.Threading.Tasks;
namespace Simplify.Controllers
{
	[Authorize]
    public class TaskController : Controller
	{
		private readonly ITaskService _taskService;
		private readonly IUserService _userService;

		public TaskController(ITaskService taskService, IUserService userService)
		{
			_taskService = taskService;
			_userService = userService;
		}
        public async Task<IActionResult> Dashboard()
		{
			string? strUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(strUserId) || !int.TryParse(strUserId, out int userId))
            {
                return RedirectToAction("Logout", "Account");
            }
			UserAccount user = await _userService.GetById(userId);
			ViewBag.Schedule = await _taskService.GenerateDailySchedule(userId, DateTime.Today);
            return View(user);
		}
        public async Task<IActionResult> Index()
        {
            string? userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? userId = Utils.ParseOrDefault<int>(userIdStr, null);
            if (userId == null)
            {
                return RedirectToAction("Logout");
            }

            var tasks = await _taskService.GetByUserId(userId);


            var tasksGroupedByState = new Dictionary<string, List<UserTask>>
            {
                { "En proceso", tasks.Where(t => t.State == "En proceso").ToList() },
                { "Cancelado", tasks.Where(t => t.State == "Cancelado").ToList() },
                { "Finalizado", tasks.Where(t => t.State == "Finalizado").ToList() }
            };

            return View(tasksGroupedByState);
        }
        public IActionResult Details(int taskId)
		{
            string? userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? userId = Utils.ParseOrDefault<int>(userIdStr, null);
            if (userId == null)
            {
                return RedirectToAction("Logout", "Account");
            }
            ViewBag.task = _taskService.GetByTaskId(taskId, userId);
			return View();
		}
		public IActionResult Edit(int taskId)
		{
            string? userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? userId = Utils.ParseOrDefault<int>(userIdStr, null);
            if (userId == null)
            {
                return RedirectToAction("Logout", "Account");
            }
            ViewBag.task = _taskService.GetByTaskId(taskId, userId);
			return View();
		}
		[HttpPost]
		public async Task<IActionResult> Edit(UserTask task)
		{
            try
            {
                string? userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int? userId = Utils.ParseOrDefault<int>(userIdStr, null);
                if (userId == null)
                {
                    return RedirectToAction("Logout", "Account");
                }
                await _taskService.Edit(task, userId);
                return Json(new { success = true });
            } catch(Exception e)
            {
                return Json(new { success = false, message = e.Message });
            }
        }
		[HttpGet]
		public IActionResult Add()
		{
			return View();
		}
		[HttpPost]
		public async Task<IActionResult> Add(UserTask task)
		{
			try
			{
                string? userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int? userId = Utils.ParseOrDefault<int>(userIdStr, null);
                if (userId == null)
                {
                    return RedirectToAction("Logout", "Account");
                }
                task.UserId = userId ?? 0;
                await _taskService.Add(task);
				return Json(new { success = true });
			} catch (Exception e) {
				return Json(new { success = false, message = $"Error: {e.Message}" });
			}
		}
        [HttpPost]
        public async Task<IActionResult> Delete(string taskId)
        {
            try
            {
                string? userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int? userId = Utils.ParseOrDefault<int>(userIdStr, null);
                if (userId == null)
                {
                    return RedirectToAction("Logout", "Account");
                }
                if (!int.TryParse(taskId, out int parsedTaskId))
                {
                    return Json(new { success = false, message = "No se ha podido parsear el id de la tarea" });
                }
                await _taskService.Delete(parsedTaskId, userId);
                return Json(new { success = true });
            } catch (Exception e)
            {
                return Json(new { success = false, message = e.Message });
            }
        }
		[HttpGet]
		public async Task<IActionResult> Get()
		{
			try
			{
				string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var tasks = await _taskService.GetByUserId(Utils.ParseOrDefault<int>(userId));
                return Json(new { success = true, tasks });
            } catch (Exception e)
			{
				return Json(new { success = false, message = e.Message });
			}
		}
        [HttpGet]
		public async Task<IActionResult> GetTasksData()
		{
			try
			{
				var today = DateTime.Today;
                string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var tasks = await _taskService.GetByUserId(Utils.ParseOrDefault<int>(userId));
                var completedTasks = tasks.Where(t => t.State?.ToLower() == "finalizado").ToList();
                var pendingTasks = tasks.Where(t => t.State?.ToLower() != "finalizado").ToList();
                var overdueTasks = pendingTasks.Where(t => t.DueDate < today).ToList();

                var statistics = new
                {
                    Total = tasks.Count,
                    Completed = completedTasks.Count,
                    Pending = pendingTasks.Count,
                    Overdue = overdueTasks.Count,
                    CompletionRate = tasks.Any()
						? (double)completedTasks.Count / tasks.Count * 100
						: 0,
                    TasksByPriority = new
                    {
                        High = tasks.Count(t => t.Priority?.ToLower() == "alta"),
                        Medium = tasks.Count(t => t.Priority?.ToLower() == "media"),
                        Low = tasks.Count(t => t.Priority?.ToLower() == "baja")
                    }
                };
                return Ok(statistics);
            } catch (Exception e) {
				return Json(new { success = false, message = e.Message});
			}
		}
        [HttpGet]
        public async Task<IActionResult> GetDetails(int taskId)
        {
            try
            {
                string? userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int? userId = Utils.ParseOrDefault<int>(userIdStr, null);
                if (userId == null)
                {
                    return RedirectToAction("Logout", "Account");
                }
                var task = await _taskService.GetByTaskId(taskId, userId);
                return Json(new { success = true, task });
            } catch (Exception e)
            {
                return Json(new { success = false, message = e.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetTasksForCalendar()
        {
            string? strUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(strUserId) || !int.TryParse(strUserId, out int userId))
            {
                return RedirectToAction("Logout", "Account");
            }

            var tasks = await _taskService.GetByUserId(userId);
            tasks = tasks.Where(t => t.State != "Finalizado").ToList();

            var calendarEvents = tasks.Select(task => new
            {
                id = task.Id,
                title = task.Name,
                start = task.DueDate?.ToString("yyyy-MM-dd"),
                end = task.DueDate?.ToString("yyyy-MM-dd"),
                description = task.Description,
                color = task.Priority switch
                {
                    "Alta" => "#dc3545",
                    "Media" => "#ffc107",
                    "Baja" => "#28a745",
                    _ => "#007bff"
                }
            });

            return Ok(calendarEvents);
        }
        [HttpGet]
        public async Task<IActionResult> GetFilteredTasks(string category, string priority)
        {
            string? userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? userId = Utils.ParseOrDefault<int>(userIdStr, null);
            if (userId == null)
            {
                return RedirectToAction("Logout");
            }

            List<UserTask> tasks;


            switch (category)
            {
                case "in_process":
                    tasks = (await _taskService.GetByUserId(userId)).Where(t => t.State == "En proceso").ToList();
                    break;
                case "cancelled":
                    tasks = (await _taskService.GetCancelledByUserId(userId)).Where(t => t.State == "Cancelado").ToList();
                    break;
                case "completed":
                    tasks = (await _taskService.GetByUserId(userId)).Where(t => t.State == "Finalizado").ToList();
                    break;
                default:
                    tasks = (await _taskService.GetByUserId(userId)).ToList();
                    break;
            }

            if (!string.IsNullOrEmpty(priority))
            {
                tasks = tasks.Where(t => t.Priority == priority).ToList();
            }

            return Ok(tasks);
        }
        [HttpGet]
        public async Task<IActionResult> Search(string searchText, string category, string priority)
        {
            string? userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? userId = Utils.ParseOrDefault<int>(userIdStr, null);
            if (userId == null)
            {
                return RedirectToAction("Logout", "Account");
            }
            var tasks = await _taskService.Get(userId);


            if (!string.IsNullOrEmpty(searchText))
            {
                tasks = tasks.Where(t => t.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                          t.Description.Contains(searchText, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(category))
            {
                tasks = tasks.Where(t => t.State.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(priority))
            {
                tasks = tasks.Where(t => t.Priority.Equals(priority, StringComparison.OrdinalIgnoreCase)).ToList();
            }


            return Json(tasks);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateState(int id, string state, int? remainingTime)
        {
            try
            {
                string? userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int? userId = Utils.ParseOrDefault<int>(userIdStr, null);
                if (userId == null)
                {
                    return RedirectToAction("Logout", "Account");
                }
                await _taskService.UpdateState(id, state, remainingTime, userId);
                return Json(new { success = true, message = "El estado de la tarea se actualizó correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

    }
}