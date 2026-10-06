using MajorDecision.Web.Models.Entities;
using MajorDecision.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Claims;
using MajorDecision.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using MajorDecision.Data.Services.Abstract;
using MajorDecision.Data.Dto;

namespace MajorDecision.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IAdminService _adminService;

        public AdminController(UserManager<ApplicationUser> userManager, IWebHostEnvironment hostingEnvironment, RoleManager<IdentityRole> roleManager,
            ApplicationDbContext db, IAdminService adminService)
        {
            _userManager = userManager;
            _db = db;
            _roleManager = roleManager;
            _hostingEnvironment = hostingEnvironment;
            _adminService = adminService;
        }

        [HttpGet]
        public async Task<IActionResult> DisplayUsers()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var users = await _adminService.GetAllUsersAsync(currentUser.Id);
            return View(users);
        }

        public string GetRole(string Id)
        {
            var userRole = _db.Users.Where(u => u.Id == Id).Join(_roleManager.Roles, u => u.Id, r => r.Id, (u, r) => r.Name).FirstOrDefault();

            return userRole;
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeUserRole(string Id)
        {
            var (success, message) = await _adminService.ChangeUserRoleAsync(Id);
            TempData["msg"] = message;
            return RedirectToAction("DisplayUsers");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearDataWithoutApplicationUserId()
        {
            var count = await _adminService.ClearDecisionsWithoutApplicationUserIdAsync();
            if (count > 0)
            {
                TempData["msg"] = "Data has been deleted";
            }
            else
            {
                TempData["msg"] = "No data";
            }

            return RedirectToAction("DisplayUsers");
        }

        [HttpGet]
        public async Task<IActionResult> UserInfo(string Id)
        {
            var model = await _adminService.GetUserInfoAsync(Id);
            if (model == null)
                return NotFound();

            if (string.IsNullOrEmpty(model.ProfilePictureUrl))
            {
                ViewData["Photo"] = "/images/NoImage.png";
            }
            else
            {
                ViewData["Photo"] = "/images/profileImages/" + model.ProfilePictureUrl;
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> DisplayAnswers()
        {
            var answers = await _adminService.GetAnswersAsync();
            return View(answers);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAnswerSentence(CreateAnswerDto newAnswer)
        {
            if (ModelState.IsValid)
            {
                await _adminService.AddAnswerAsync(newAnswer);
                return RedirectToAction("DisplayAnswers");
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Manage(int id)
        {
            var answers = await _adminService.GetAnswersAsync();
            var answer = answers.FirstOrDefault(a => a.Id == id);

            if (answer == null)
                return NotFound();

            return View(answer);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAnswerSentence(int id, CreateAnswerDto newAnswer)
        {
            if (ModelState.IsValid)
            {
                await _adminService.EditAnswerAsync(id, newAnswer);
                return RedirectToAction("DisplayAnswers");
            }

            TempData["msg"] = "Field <Answer> is required";
            return RedirectToAction("Manage");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAnswerSentence(int id)
        {
            await _adminService.DeleteAnswerAsync(id);
            return RedirectToAction("DisplayAnswers");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearAllProfilePictures()
        {
            var count = await _adminService.ClearAllProfilePicturesAsync(/*_hostingEnvironment.WebRootPath*/);
            if (count > 0)
            {
                TempData["msg"] = "Profile photos have been deleted";
            }
            else
            {
                TempData["msg"] = "No photo";
            }

            return RedirectToAction("DisplayUsers");
        }
    }
}
