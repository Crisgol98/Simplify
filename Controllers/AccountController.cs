using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Simplify.Models;
using Microsoft.AspNetCore.Authorization;
using Simplify.Interfaces.Worklayer;

namespace Simplify.Controllers
{
    public class AccountController : Controller
    {

        private readonly IUserService _userService;
        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Task");
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(string name, string password)
        {
            List<UserAccount> users = await _userService.GetUsers();
            UserAccount? foundUser = users.FirstOrDefault(u => u.Username == name || u.Email == name);
            if (foundUser == null)
            {
                ViewBag.ErrorMessage = "Nombre de usuario o email no encontrado.";
                return View();
            }
            if (foundUser.Password != password)
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

            return RedirectToAction("Index", "Task");
        }
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            HttpContext.Session.Clear();

            return RedirectToAction("Login", "Account");
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
        [Authorize]
        [HttpGet]
        public IActionResult Configuration()
        {
            return View();
        }
        public IActionResult LoadPartial(string view)
        {
            switch (view)
            {
                case "Credentials":
                    return PartialView("_CredentialsPartial");
                case "Settings":
                    return PartialView("_SettingsPartial");
                default:
                    return PartialView("_NotFoundPartial");
            }
        }
        public IActionResult EditCredentials(UserAccount user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    return Json(new { success = true });
                } catch (Exception e)
                {
                    return Json(new { success = false, message = "Hubo un problema al actualizar los datos. Intenta nuevamente." });
                }
            }
            return Json(new { success = false, message = "Hay errores en los datos proporcionados. Verifica e intenta nuevamente." });
        }
    }
}
