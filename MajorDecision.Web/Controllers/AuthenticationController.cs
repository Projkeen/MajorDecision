using MajorDecision.Web.Data.Repositories.Abstract;
using MajorDecision.Web.Models.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MajorDecision.Web.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly IAuthenticationService _service;
        public AuthenticationController(IAuthenticationService service)
        {
            _service = service;
        }

        public IActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registration(Registration model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _service.RegistrationAsync(model);
            if (result.StatusCode == 1)
            {
                TempData["msg"] = result.Message;
                return RedirectToAction(nameof(Login));
            }
            else
            {
                TempData["msg"] = result.Message;
                return RedirectToAction(nameof(Registration));
            }
            
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(Login model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _service.LoginAsync(model);
            if (result.StatusCode == 1)
            {
                return RedirectToAction("Index", "Decision");
            }
            else
            {
                TempData["msg"] = result.Message;
                return RedirectToAction(nameof(Login));
            }
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _service.LogoutAsync();
            return RedirectToAction("Index", "Decision");
        }

        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Authorize, HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePassword model)
        {
            if (!ModelState.IsValid)
                return View(model);
            var result = await _service.ChangePasswordAsync(model, User.Identity.Name);
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

