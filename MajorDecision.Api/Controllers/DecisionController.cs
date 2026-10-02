using MajorDecision.Data.Dto;
using MajorDecision.Data.Services.Abstract;
using MajorDecision.Web.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MajorDecision.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DecisionController : ControllerBase
    {
        private readonly IDecisionService _decisionService;
        private readonly UserManager<ApplicationUser> _userManager;
        public DecisionController(IDecisionService decisionService, UserManager<ApplicationUser> userManager)
        {
            _decisionService = decisionService;
            _userManager = userManager;
        }

        [HttpPost("index")]
        public async Task<IActionResult> Index([FromBody] DecisionRequest request, [FromQuery] string? lucky)
        {
            if (string.IsNullOrWhiteSpace(request.Question))
            {
                return BadRequest(new { Message = "You must enter the question" });
            }
            ApplicationUser? user;
            if (User.Identity!.IsAuthenticated)
            {
                user = await _userManager.GetUserAsync(User);
            }
            else
            {
                user = null;
            }
            var result = await _decisionService.ShowAnswerAsync(request.Question, lucky, user);
            return Ok(new { result.Answer });
        }
    }
}
