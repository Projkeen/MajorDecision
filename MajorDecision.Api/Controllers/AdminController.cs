using Azure.Core.Pipeline;
using MajorDecision.Data;
using MajorDecision.Data.Dto;
using MajorDecision.Data.Services.Abstract;
using MajorDecision.Web.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MajorDecision.Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    [ApiController]
    public class AdminController : ControllerBase
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

        [HttpGet("get-users")]
        public async Task<IActionResult> DisplayUsers()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var users = await _adminService.GetAllUsersAsync(currentUser.Id);
            if (users != null)
            {
                return Ok(new { users, Message = "all users from Db" });
            }
            return Ok(new { users, Message = "No users" });

        }

        [HttpGet("user-info/{id}")]
        public async Task<IActionResult> UserInfo(string Id)
        {
            var model = await _adminService.GetUserInfoAsync(Id);
            if (model == null)
                return NotFound("User not found");
            return Ok(new { model, Message = $"user data {model.Username}" });
        }

        [HttpPost("change-role-for-user/{id}")]
        public async Task<IActionResult> ChangeUserRole(string Id)
        {
            var (success, message) = await _adminService.ChangeUserRoleAsync(Id);
            return Ok(new { success, message });
        }

        [HttpDelete("clear-data-without-app-user-id")]
        public async Task<IActionResult> ClearDataWithoutApplicationUserId()
        {
            var count = await _adminService.ClearDecisionsWithoutApplicationUserIdAsync();
            if (count > 0)
            {
                return Ok(new { Message = "Data without application user Id has been deleted" });
            }
            else
            {
                return Ok(new { Message = "No data" });
            }
        }

        [HttpDelete("clear-all-profile-photos-from-users")]
        public async Task<IActionResult> ClearAllProfilePictures()
        {
            var count = await _adminService.ClearAllProfilePicturesAsync(/*_hostingEnvironment.WebRootPath*/);
            if (count > 0)
            {
                return Ok(new { Message = "Profile photos have been deleted" });
            }
            else
            {
                return Ok(new { Message = "No photos" });
            }
        }

        [HttpGet("display-answers")]
        public async Task<IActionResult> DisplayAnswers()
        {
            var answers = await _adminService.GetAnswersAsync();
            return Ok(new { answers, Message = "Answers from Db" });
        }

        [HttpPost("add-answer-sentence")]
        public async Task<IActionResult> AddAnswerSentence([FromBody] CreateAnswerDto newAnswer)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _adminService.AddAnswerAsync(newAnswer);

            return StatusCode(201, newAnswer);
        }

        [HttpPut("update-answer/{id}")]        
        public async Task<IActionResult> EditAnswerSentence([FromRoute]int id, [FromBody] CreateAnswerDto newAnswer)
        {
            await _adminService.EditAnswerAsync(id, newAnswer);            
            return Ok(new { Message = "Data was updated" });
        }

        [HttpDelete("delete-answer/{id}")]        
        public async Task<IActionResult> DeleteAnswerSentence([FromRoute] int id)
        {
            await _adminService.DeleteAnswerAsync(id);
            return Ok(new { Message = "Data has been deleted" });
        }        
    }
}
