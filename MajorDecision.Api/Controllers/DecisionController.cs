using MajorDecision.Data.Dto;
using MajorDecision.Data.Services.Abstract;
using MajorDecision.Web.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

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

        [HttpGet("answerslogs")]
        public async Task<IActionResult> GetAllAnswers([FromQuery] int pageNumber, [FromQuery] string? searchString)
        {
            if (User.Identity!.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                var history = await _decisionService.GetHistoryAsync(user!.Id, pageNumber, searchString);
                if (history.Items.Count != 0)
                {
                    return Ok(new { Data = history, Message = $"Data from user {user.UserName}" });
                }
                return Ok(new { Data = history, Message = $"No Data from user {user.UserName}" });
            }
            else
            {
                var decisions = await _decisionService.GetAllAsync().ToListAsync();
                if (decisions.Count != 0)
                {
                    return Ok(new { Data = decisions, Message = "All questions and answers from DB" });
                }
                return Ok(new { Data = decisions, Message = "No data" });
            }
        }

        [Authorize]
        [HttpDelete("deletedecision")]
        public async Task<IActionResult> DeleteDecision([FromBody] IEnumerable<int> decisionId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (decisionId.Count() != 0)
            {
                var deletedCount = await _decisionService.DeleteAsync(decisionId, user.Id);
                if (deletedCount != 0)
                {
                    return Ok(new { Message = $"Decisions from user {user.UserName} has been deleted" });
                }
                return Ok(new { Message = $" No Decisions from user {user.UserName}" });
            }
            return BadRequest(new { Message = "You must enter the id's" });
        }

        [Authorize]
        [HttpDelete("delete-all")]
        public async Task<IActionResult> DeleteAllDecisions()
        {
            var user = await _userManager.GetUserAsync(User);
            await _decisionService.DeleteAllAsync(user.Id);
            return Ok(new { Message = $" No Decisions from user {user.UserName}" });
        }

        [Authorize]
        [HttpGet("download-logs")]
        public async Task<IActionResult> DownloadAnswers()
        {
            var user = await _userManager.GetUserAsync(User);
            var decisions = await _decisionService.GetForDownloadAsync(user.Id);
            if (decisions.Count == 0)
            {
                return Ok(new { Message = $" No Data from user {user.UserName}" });
            }

            var csv = new StringBuilder();
            csv.AppendLine("User;Question;Answer;Date Of Question");

            foreach (var d in decisions)
            {
                csv.AppendLine(string.Join(';', CsvEscape(user.UserName ?? ""), CsvEscape(d.Question), CsvEscape(d.Answer ?? ""), d.DateOfQuestion.ToString("O")));
            }
            var utf8WithBom = new UTF8Encoding(true);
            var bytes = utf8WithBom.GetPreamble().Concat(utf8WithBom.GetBytes(csv.ToString())).ToArray();

            return File(bytes, "text/csv", $"{user.UserName} Answers.csv");
        }

        private static string CsvEscape(string value)
        {
            if (value.Contains(';') || value.Contains('"') || value.Contains('\n'))
                return $"\"{value.Replace("\"", "\"\"")}\"";
            return value;
        }

        [Authorize]
        [HttpGet("last20questions")]
        public async Task<IActionResult> LastQuestions()
        {
            var questions = await _decisionService.GetLastQuestionsAsync();
            return Ok(new {questions, Message = "Last 20 questions" });
        }
    }    
}
