using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Simplify.Interfaces.Worklayer;
using Simplify.Models;
using System.Globalization;
using System.Security.Claims;
using UserTask = Simplify.Models.UserTask;
namespace Simplify.Controllers
{
	public class TaskController : Controller
	{
		private readonly ITaskService _taskService;
		private readonly IUserService _userService;

		public TaskController(ITaskService taskService, IUserService userService)
		{
			_taskService = taskService;
			_userService = userService;
		}

		[Authorize]
		public async Task<IActionResult> Index()
		{
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
			string? strUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(strUserId) || !int.TryParse(strUserId, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }
			UserAccount user = await _userService.GetUserById(userId);
			ViewBag.Schedule = await _taskService.GenerateWeeklySchedule();
            return View(user);
		}
		public IActionResult Details(int taskId)
		{
			ViewBag.task = _taskService.GetTaskById(taskId);
			return View();
		}
		public IActionResult Edit(int taskId)
		{
			ViewBag.task = _taskService.GetTaskById(taskId);
			return View();
		}
		[HttpPost]
		public async Task<IActionResult> Edit(UserTask task)
		{
			if (await _taskService.EditTask(task))
			{
				return Json(new { success = true });
			}
			return Json(new { success = false, message = "Task could not be edited" });
		}
		[HttpGet]
		public IActionResult AddTask()
		{
			return View();
		}
		[HttpPost]
		public async Task<IActionResult> AddTask(UserTask task)
		{
			try
			{
				await _taskService.AddTask(task);
				return Json(new { success = true });
			} catch (Exception e) {
				return Json(new { success = false, message = $"Error: {e.Message}" });
			}
		}
		[HttpGet]
		public async Task<IActionResult> GetTasks()
		{
			try
			{
                var tasks = await _taskService.GetTasks();
                return Json(new { success = true, tasks });
            } catch (Exception e)
			{
				return Json(new { success = false, message = e.Message });
			}
		}
    }
}