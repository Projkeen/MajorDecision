using Azure.Identity;
using MajorDecision.Data.Services.Abstract;
using MajorDecision.Web.Models.ViewModels.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static MajorDecision.Web.Data.AppUserFriendship;

namespace MajorDecision.Web.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly IAuthenticationService _authService;
        public AuthenticationController(IAuthenticationService authService)
        {
            _authService = authService;
        }

        public IActionResult Registration()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Registration(Registration model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var (status, user, role) = await _authService.RegistrationAsync(model);
            if (status.StatusCode == 1 || user != null)
            {
                TempData["msg"] = status.Message;
                return RedirectToAction(nameof(Login));
            }
            else
            {
                TempData["msg"] = status.Message;
                return RedirectToAction(nameof(Registration));
            }
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(Login model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (status, user, role) = await _authService.LoginAsync(model);
            if (status.StatusCode == 1 || user != null)
            {
                return RedirectToAction("Index", "Decision");
            }
            else
            {
                TempData["msg"] = status.Message;
                return RedirectToAction(nameof(Login));
            }
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            return RedirectToAction("Index", "Decision");
        }

        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Authorize, HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePassword model)
        {
            if (!ModelState.IsValid)
                return View(model);
            var result = await _authService.ChangePasswordAsync(model, User.Identity.Name);
            if (result.StatusCode == 1)
            {
                TempData["msg"] = result.Message;
                return RedirectToAction("ManageProfile", "Profile");
            }
            else
            {
                TempData["msg"] = result.Message;
                return RedirectToAction(nameof(ChangePassword));
            }
        }
    }
}

