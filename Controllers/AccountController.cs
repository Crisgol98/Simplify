using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Simplify.Models;
using Microsoft.AspNetCore.Authorization;
using Simplify.Interfaces.Worklayer;
using Simplify.Resources.Utils;

namespace Simplify.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {

        private readonly IUserService _userService;
        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Dashboard", "Task");
            }
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string name, string password)
        {
            List<UserAccount> users = await _userService.Get();
            UserAccount? foundUser = users.FirstOrDefault(u => u.Username == name || u.Email == name);
            if (foundUser == null)
            {
                ViewBag.ErrorMessage = "Nombre de usuario o email no encontrado.";
                return View();
            }
            if (!Utils.VerifyPassword(password, foundUser.Password))
            {
                ViewBag.ErrorMessage = "Contraseña incorrecta.";
                return View();
            }
            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, foundUser.Name),
                    new Claim(ClaimTypes.NameIdentifier, foundUser.Id.ToString()),
                    new Claim(ClaimTypes.Role, "User")
                };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true
            };
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                                      new ClaimsPrincipal(claimsIdentity),
                                      authProperties);

            return RedirectToAction("Dashboard", "Task");
        }
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            HttpContext.Session.Clear();

            return RedirectToAction("Login", "Account");
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Introduction()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(UserAccount user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    int result = await _userService.Add(user);
                    if (result == -1)
                    {
                        return Json(new { success = false, message = "El correo electrónico ya existe." });
                    } else if (result == -2)
                    {
                        return Json(new { success = false, message = "El nombre de usuario ya existe." });
                    }
                    user.Id = result;
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Name),
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Role, "User")
                    };
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true
                    };
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                                              new ClaimsPrincipal(claimsIdentity),
                                              authProperties);
                    return Json(new { success = true });
                } catch (Exception e)
                {
                    return Json(new { success = false, message = e.Message });
                }
            } else
            {
                var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

                return Json(new { success = false, message = errors.First() });
            }
        }
        [HttpGet]
        public IActionResult Dashboard()
        {
            return View();
        }
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Configuration()
        {
            string? userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? userId = Utils.ParseOrDefault<int>(userIdStr, null);
            if (userId == null)
            {
                return RedirectToAction("Logout");
            }
            UserAccount user = await _userService.GetById(userId);
            if (user == null)
            {
                return RedirectToAction("Logout");
            }
            return View(user);
        }
        [HttpGet]
        public async Task<IActionResult> LoadPartial(string view)
        {
            string? userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? userId = Utils.ParseOrDefault<int>(userIdStr, null);
            if (userId == null)
            {
                return RedirectToAction("Logout");
            }
            switch (view)
            {
                case "Credentials":
                    UserAccount user = await _userService.GetById(userId);
                    return PartialView("_AccountDetailsPartial", user);
                case "Settings":
                    UserPreferences preferences = await _userService.GetPreferences(userId);
                    return PartialView("_SettingsPartial", preferences);
                default:
                    return PartialView("_NotFoundPartial");
            }
        }
        public async Task<IActionResult> UpdateCredentials(UserAccount account)
        {
            try
            {
                string? userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int? userId = Utils.ParseOrDefault<int>(userIdStr, null);
                if (userId.HasValue)
                {
                    account.Id = userId.Value;

                    if (await _userService.EditCredentials(account) == -1)
                    {
                        return Json(new { success = false, message = "El nombre de usuario introducido ya existe." });
                    }
                    return Json(new { success = true, message = "Datos actualizados correctamente." });
                }
                else
                {
                    return Json(new { success = false, message = "No se pudo obtener el ID del usuario." });
                }
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = $"Hubo un problema al actualizar los datos: {e.Message}" });
            }
        }
        public async Task<IActionResult> UpdateInformation(UserAccount account)
        {
            try
            {
                string? userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int? userId = Utils.ParseOrDefault<int>(userIdStr, null);
                if (userId.HasValue)
                {
                    account.Id = userId.Value;

                    if (await _userService.EditInformation(account) == -1)
                    {
                        return Json(new { success = false, message = "El correo electrónico ya está siendo utilizado." });
                    }
                    return Json(new { success = true, message = "Datos actualizados correctamente." });
                }
                else
                {
                    return Json(new { success = false, message = "No se pudo obtener el ID del usuario." });
                }
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = $"Hubo un problema al actualizar los datos: {e.Message}" });
            }
        }
        public async Task<IActionResult> UpdatePreferences(UserPreferences preferences)
        {
            string? userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? userId = Utils.ParseOrDefault<int>(userIdStr, null);
            if (userId == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                await _userService.UpdatePreferences(userId, preferences);
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = e.Message });
            }
        }
    }
}
