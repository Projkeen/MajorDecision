using MajorDecision.Web.Data;
using MajorDecision.Web.Data.Repositories.Abstract;
using MajorDecision.Web.Models;
using MajorDecision.Web.Models.Entities;
using MajorDecision.Web.Models.ViewModels;
using MajorDecision.Web.Models.ViewModels.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using System.Security.Claims;

namespace MajorDecision.Web.Controllers
{
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IProfileService _profileService;
        private readonly IFriendshipService _friendshipService;
        private readonly IAuthenticationService _authService;       
        private readonly INotificationService _notifications;

        public ProfileController(UserManager<ApplicationUser> userManager, IWebHostEnvironment hostingEnvironment, IAuthenticationService authService,
            INotificationService notifications, IProfileService profileService, IFriendshipService friendshipService)
        {
            _userManager = userManager;
            _hostingEnvironment = hostingEnvironment;
            _authService = authService;            
            _notifications = notifications;
            _profileService = profileService;
            _friendshipService = friendshipService;
        }
        
        private async Task<string?> CurrentUserIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)            
                return null;            
            else            
                return user.Id;            
        }

        [HttpGet]
        public async Task<IActionResult> ManageProfile()
        {   
            var currentUserId = await CurrentUserIdAsync();
            var model = await _profileService.GetProfileAsync(currentUserId);
            ViewData["Photo"] = model.ProfilePictureUrl;
            return View(model);
        }

        public async Task<IActionResult> UploadImage()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadOrDeleteImage(UserViewModel model)
        {
            var currentUserId = await CurrentUserIdAsync();
            var (success, message) = await _profileService.UploadOrDeleteImageAsync(currentUserId, model.Photo, _hostingEnvironment.WebRootPath);
            TempData["msg"] = message;
            return RedirectToAction("ManageProfile");
        }

        public async Task<IActionResult> EditUser(/*string id*/)
        {            
            return View();
            //return View("ManageProfile");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(UserViewModel model)
        {
            var currentUserId = await CurrentUserIdAsync();
            var (success, message) = await _profileService.UpdateProfileAsync(currentUserId, model);
            TempData["msg"] = message;
            return RedirectToAction("ManageProfile");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount(string confirm)
        {            
            if (confirm != "Delete")
            {
                TempData["msg"] = "Error";
                return RedirectToAction("ManageProfile");
            }

            var currentUserId = await CurrentUserIdAsync();
            var deleted = await _profileService.DeleteAccountAsync(currentUserId);
            if (!deleted)
            {
                TempData["msg"] = "Error";
                return RedirectToAction("ManageProfile");
            }

            await _authService.LogoutAsync();
            TempData["msg"] = "User has been deleted, we are waiting for you again";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Notifications()
        {
            var currentUserId = await CurrentUserIdAsync();
            var notifications = await _notifications.GetAndCheckReadAsync(currentUserId);
            return View(notifications);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptRequestToFriend(string senderId)
        {
            var currentUserId = await CurrentUserIdAsync();
            var check = await _friendshipService.AcceptAsync(currentUserId, senderId);
            if (check != true)
            {
                TempData["AlertMessage"] = "Error"; 
            }            
            return RedirectToAction("Notifications", "Profile");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeclineRequestToFriend(string senderId)
        {
            var currentUserId = await CurrentUserIdAsync();
            var check = await _friendshipService.DeclineAsync(currentUserId, senderId);
            if (check != true)
            {
                TempData["AlertMessage"] = "Error";
            }
            return RedirectToAction("Notifications", "Profile");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ReadCheckConfirmNotification(int id)
        {
            var currentUserId = await CurrentUserIdAsync();
            var check = await _notifications.DeleteAsync(id, currentUserId);
            if (check != true)
            {
                TempData["AlertMessage"] = "Error";
            }
            return RedirectToAction("Notifications", "Profile");
        }

        [HttpGet]
        public async Task<IActionResult> GetFriendsList()
        {
            var currentUserId = await CurrentUserIdAsync();
            var friends = await _friendshipService.GetFriendsAsync(currentUserId);
            return View(friends);
        }
    }
}
