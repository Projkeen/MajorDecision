using MajorDecision.Data.Dto;
using MajorDecision.Data.Services.Abstract;
using MajorDecision.Web.Models.Entities;
using MajorDecision.Web.Models.ViewModels;
using MajorDecision.Web.Models.ViewModels.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MajorDecision.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
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

        [HttpGet("user-profile")]
        public async Task<IActionResult> ManageProfile()
        {
            var currentUserId = await CurrentUserIdAsync();
            if (currentUserId == null)
                return Unauthorized();
            var model = await _profileService.GetProfileAsync(currentUserId);
            if (model == null)
                return NotFound();
            return Ok(new { model, Message = $"Profile {model.Username}" });
        }

        [HttpPost("add-or-delete-profile-photo")]
        public async Task<IActionResult> UploadOrDeleteImage([FromForm] IFormFile? photo = null)
        {
            var currentUserId = await CurrentUserIdAsync();
            var (success, message) = await _profileService.UploadOrDeleteImageAsync(currentUserId, photo/*, _hostingEnvironment.WebRootPath*/);
            if (success == true)
                return Ok(new { Model = photo, Message = message });
            return BadRequest(new { Message = message });
        }

        [HttpPatch("edit-user-profile")]
        public async Task<IActionResult> EditProfile([FromBody] EditProfileRequest request)
        {
            if (ModelState.IsValid == false)
                return BadRequest(new { ModelState, Message = "Invalid data" });
            var currentUserId = await CurrentUserIdAsync();
            var (success, message) = await _profileService.UpdateProfileAsync(currentUserId, request);
            if (success == false)
                return BadRequest(new { Message = message });
            var updatedProfile = await _profileService.GetProfileAsync(currentUserId);
            return Ok(new { Model = updatedProfile, Message = message });

        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePassword model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var currentUserId = await CurrentUserIdAsync();
            var result = await _authService.ChangePasswordAsync(model, currentUserId);
            if (result.StatusCode != 1)
                return BadRequest(new { result.StatusCode, result.Message });
            return Ok(new { result.StatusCode, result.Message });
        }

        [HttpDelete("delete-account")]
        public async Task<IActionResult> DeleteAccount([FromQuery] string confirm)
        {
            if (confirm.ToLower() != "delete")
                return BadRequest(new { Message = "You must type 'delete' to confirm." });
            var currentUserId = await CurrentUserIdAsync();
            var deleted = await _profileService.DeleteAccountAsync(currentUserId);
            if (deleted != true)
                return BadRequest(new { Message = "Error" });
            return Ok(new { Message = "User has been deleted, we are waiting for you again" });
        }

        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications()
        {
            var currentUserId = await CurrentUserIdAsync();
            var notifications = await _notifications.GetAndCheckReadAsync(currentUserId);
            if (!notifications.Any())
                return NotFound(new { Message = "No notifications" });
            return Ok(new { Notifications = notifications, Message = $"Notifications for {notifications.First().ReceiverName}" });
        }

        [HttpPost("accept-request-to-friends")]
        public async Task<IActionResult> AcceptRequestToFriend([FromQuery] string senderId)
        {
            var currentUserId = await CurrentUserIdAsync();
            var (check, message) = await _friendshipService.AcceptAsync(currentUserId, senderId);
            if (check != true)
            {
                return NotFound("Error");
            }
            return Ok(new { Message = $"You are friends with {message}" });
        }

        [HttpPost("decline-request-to-friends")]
        public async Task<IActionResult> DeclineRequestToFriend([FromQuery] string senderId)
        {
            var currentUserId = await CurrentUserIdAsync();
            var (check, message) = await _friendshipService.DeclineAsync(currentUserId, senderId);
            if (check != true)
            {
                return NotFound("Error");
            }
            return Ok(new { Message = $"You canceled request to friend with {message}" });
        }

        [HttpPost("confirm-notification")]
        public async Task<IActionResult> ReadCheckConfirmNotification([FromQuery] int id)
        {
            var currentUserId = await CurrentUserIdAsync();
            var check = await _notifications.DeleteAsync(id, currentUserId);
            if (check != true)
            {
                return NotFound("Error");
            }
            return Ok(new { Message = "Notification is read" });
        }

        [HttpGet("friends-list")]
        public async Task<IActionResult> GetFriendsList()
        {
            var currentUserId = await CurrentUserIdAsync();
            var friends = await _friendshipService.GetFriendsAsync(currentUserId);
            if (!friends.Any())
                return NotFound("Error");
            return Ok(new { friends, Message = $"Friends user's id: {currentUserId}" });
        }
    }
}
