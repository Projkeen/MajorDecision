using MajorDecision.Web.Data;
using MajorDecision.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using MajorDecision.Web.Models.ViewModels;
using MajorDecision.Web.Models.Entities;
using static MajorDecision.Web.Data.AppUserFriendship;
using MajorDecision.Data.Services.Abstract;
using MajorDecision.Data.Dto;

namespace MajorDecision.Web.Controllers
{
    //[Authorize]
    public class DecisionController : Controller
    {
        private readonly IDecisionService _decisionService;
        private readonly UserManager<ApplicationUser> _userManager;
        public DecisionController(IDecisionService decisionService, UserManager<ApplicationUser> userManager)
        {
            _decisionService = decisionService;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(DecisionRequest request, string lucky)
        {
            if (string.IsNullOrWhiteSpace(request.Question))
            {
                TempData["AlertMessage"] = "You must enter the question";
                return RedirectToAction("Index");
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
            ModelState.Clear();
            ViewBag.message = result.Answer;
            return View();
        }

        //[Authorize]
        public async Task<IActionResult> AnswersHistory(int pageNumber, string? searchString)
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                var history = await _decisionService.GetHistoryAsync(user.Id, pageNumber, searchString);
                return View(history);
            }
            else
            {
                TempData["msg"] = "You must be loggin in";
                return RedirectToAction("Login", "Authentication");
            }
        }

        //method not using anymore
        //public IActionResult SearchHistory(string searchString)
        //{
        //    var user = HttpContext.User;
        //    var decisions = from d in _db.Decisions.Where(x => x.ApplicationUserId == user.FindFirst(ClaimTypes.NameIdentifier).Value) select d;
        //    if (!String.IsNullOrEmpty(searchString))
        //    {
        //        decisions = decisions.Where(d => d.Answer.Contains(searchString) || d.Question.Contains(searchString) || d.DateOfQuestion.ToString().Contains(searchString));
        //    }
        //    return View("AnswersHistory", decisions.ToList());            
        //}        

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteHistory(IEnumerable<int> decisionIdsToDelete)
        {
            var user = await _userManager.GetUserAsync(User);
            if (decisionIdsToDelete.Count() != 0)
            {
                int[] idsToDelete;
                if (decisionIdsToDelete != null)
                {
                    idsToDelete = decisionIdsToDelete.ToArray();
                }
                else
                {
                    idsToDelete = Array.Empty<int>();
                }
                var deletedCount = await _decisionService.DeleteAsync(idsToDelete, user.Id);
                TempData["AlertMessage"] = "Deleted successfully";
                return RedirectToAction("AnswersHistory");
            }
            TempData["AlertMessage"] = "You must select";
            return RedirectToAction("AnswersHistory");
        }

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAllHistory()
        {
            var user = await _userManager.GetUserAsync(User);
            await _decisionService.DeleteAllAsync(user.Id);
            return RedirectToAction("AnswersHistory");
        }

        [Authorize, HttpGet]
        public async Task<IActionResult> DownloadAnswers()
        {
            var user = await _userManager.GetUserAsync(User);
            var decisions = await _decisionService.GetForDownloadAsync(user.Id);

            if (decisions.Count == 0)
            {
                TempData["AlertMessage"] = "No data";
                return RedirectToAction("AnswersHistory");
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

        //[HttpGet, Authorize]
        [HttpGet]
        public async Task<IActionResult> LastQuestions()
        {
            if (User.Identity.IsAuthenticated)
            {
                var questions = await _decisionService.GetLastQuestionsAsync();
                return View(questions);
            }
            else
            {
                TempData["msg"] = "You must be loggin in";
                return RedirectToAction("Login", "Authentication");
            }
        }        
    }
}

