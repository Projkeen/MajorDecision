using MajorDecision.Data.Services.Abstract;
using MajorDecision.Web.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MajorDecision.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DiscussionPageController : ControllerBase
    {
        private IPageService _pageService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DiscussionPageController(IPageService pageService, UserManager<ApplicationUser> userManager)
        {
            _pageService = pageService;
            _userManager = userManager;
        }

        [HttpGet("display-pages")]
        public async Task <IActionResult> DisplayPages()
        {
            if (User.Identity.IsAuthenticated)
            {
                var pages = await _pageService.GetPagesAsync();
                return Ok(pages);
            }
            else
            {
                return Unauthorized("You must be logged in");
            }
        }

        [HttpPost("create-page/{id}")]
        public async Task<IActionResult> CreatePage([FromRoute]int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var page = await _pageService.CreateFromDecisionAsync(id, currentUser);
            if (page == null)
            {
                return NotFound("Error");
            }
            return Ok(page);
        }

        [HttpDelete("delete-page/{id}")]
        public async Task<IActionResult> DeletePage([FromRoute]int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var (success, message) = await _pageService.DeletePageAsync(id, currentUser.Id, User.IsInRole("Admin"));
            if (success==false)            
                return BadRequest(message);            
            return Ok(message);
        }

        [HttpGet("get-page/{id}")]
        public async Task<IActionResult> GetPage([FromRoute] int id)
        {            
            var page = await _pageService.GetByIdAsync(id);
            if(page == null)
                return NotFound("Page not found");
            return Ok(page);
        }

        [HttpPost("add-description/{id}")]
        public async Task<IActionResult> AddDescription([FromRoute] int id, [FromQuery] string description)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var (result, message) = await _pageService.AddDescriptionAsync(id, description, currentUser.Id);
            if(result == false) return BadRequest(new { Message = message });
            var page = await _pageService.GetByIdAsync(id);
            return CreatedAtAction(nameof(GetPage), new { id }, new { Page = page, Message = message });           
        }

        [HttpPost("add-comment/{id}")]
        public async Task<IActionResult> AddComment([FromRoute] int id, [FromQuery] string comment)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var (result, message) = await _pageService.AddCommentAsync(comment, id, currentUser.Id);
            if (result == false) return BadRequest(new { Message = message });
            var page = await _pageService.GetByIdAsync(id);
            return CreatedAtAction(nameof(GetPage), new {id}, new {Page=page, Message = message});
        }

        [HttpDelete("delete-comment/{id}")]
        public async Task<IActionResult> DeleteComment([FromRoute] int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var (result, message, pageId) = await _pageService.DeleteCommentAsync(id, currentUser.Id, User.IsInRole("Admin"));
            if (result == false) return BadRequest(new { Message = message });
            var page = await _pageService.GetByIdAsync(pageId);
            return CreatedAtAction(nameof(GetPage), new { id }, new { Page = page, Message = message });
        }
        
    }
}
