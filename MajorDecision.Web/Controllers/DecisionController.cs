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
using MajorDecision.Web.Data.Repositories.Abstract;

namespace MajorDecision.Web.Controllers
{
    //[Authorize]
    public class DecisionController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IDecision? _decision;
        public DecisionController(IDecision decision, ApplicationDbContext db)
        {
            _db = db;
            _decision = decision;
        }

        //private static List<Decision> answer = new List<Decision>();
        public IActionResult Index()
        {
            //List<Decision> decisions = _db.Decisions.ToList();
            //return View(decisions);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> IndexAsync(Decision decision, string lucky)
        {
            //var secret = decision.SecretMethod();
            var user = HttpContext.User;
            //var userId = _db.UserLogins.Find(ClaimTypes.NameIdentifier).UserId;
            if (decision.Question != null)
            {
                if (lucky == "answer")
                {
                    await _decision.ShowAnswerBySecondMethodAsync(decision);
                }
                else
                {
                    await _decision.ShowAnswerByFirstMethodAsync(decision);
                }

                if (User.Identity.IsAuthenticated)
                {
                    decision.ApplicationUserId = user.FindFirst(ClaimTypes.NameIdentifier).Value;
                    _db.Decisions.Add(decision);
                    _db.SaveChanges();
                    ModelState.Clear();
                    ViewBag.message = decision.Answer;
                }
                else
                {
                    _db.Decisions.Add(decision);
                    _db.SaveChanges();
                    ModelState.Clear();
                    ViewBag.message = decision.Answer;
                }
                return View();
            }
            else
            {
                TempData["AlertMessage"] = "You must enter the question";
                return RedirectToAction("Index", "Decision");
            }

            //return RedirectToAction(nameof(Answer));           
            //return RedirectToAction("Index","Decision");
            //return decision.Answer;
            //return View();
        }

        public async Task <IActionResult> AnswersHistory(int pageNumber, string searchString)
        {
            //ViewBag.Id = id;
            //Decision? decisionFromDb = _db.Decisions.FirstOrDefault(u=>u.Id==id);
            //List<Decision> decisions = _db.Decisions.ToList();
            //List<Decision> decisions = _db.Decisions.Single(i => i.Id == 0);
            //return View(decisionFromDb);  
            //List<Decision> answer = _db.Decisions.SingleOrDefault();
            //ViewData["Filter"] = searchString;
            if (User.Identity.IsAuthenticated)
            {
                var user = HttpContext.User;
                //var decisions =  await _db.Decisions.Where(x => x.ApplicationUserId == user.FindFirst(ClaimTypes.NameIdentifier).Value).ToListAsync();
                var decisions = _decision.GetAllAsync();
                var descDecisions = decisions.OrderByDescending(x => x.DateOfQuestion).Where(x => x.ApplicationUserId == user.FindFirst(ClaimTypes.NameIdentifier).Value);
                if (!String.IsNullOrEmpty(searchString))
                {
                    descDecisions = decisions.Where(d => d.Answer.Contains(searchString) || d.Question.Contains(searchString) || d.DateOfQuestion.ToString().Contains(searchString));
                }
                if (pageNumber < 1)
                {
                    pageNumber = 1;
                }
                int pageSize = 13;                
                return View(await PaginatedList<DecisionVM>.CreateAsync(descDecisions, pageNumber, pageSize));
            }
            else
            {
                TempData["msg"] = "You must be loggin in";
                return RedirectToAction("Login", "Authentication"); 
            }
        }

        //method not using anymore
        public IActionResult SearchHistory(string searchString)
        {
            var user = HttpContext.User;
            var decisions = from d in _db.Decisions.Where(x => x.ApplicationUserId == user.FindFirst(ClaimTypes.NameIdentifier).Value) select d;
            if (!String.IsNullOrEmpty(searchString))
            {
                decisions = decisions.Where(d => d.Answer.Contains(searchString) || d.Question.Contains(searchString) || d.DateOfQuestion.ToString().Contains(searchString));
            }
            return View("AnswersHistory", decisions.ToList());            
        }

        //public IActionResult Answer()
        //{
        //    //List<Decision> decisions = _db.Decisions.ToList();
        //    //Decision? decisionFromDb = _db.Decisions.Find(id);
        //    return View();
        //}

        [HttpPost]
        public IActionResult DeleteHistory(IEnumerable<int> decisionIdsToDelete)
        {
            if (decisionIdsToDelete.Count() != 0)
            {
                List<Decision> decisions = _db.Decisions.Where(x => decisionIdsToDelete.Contains(x.Id)).ToList();
                foreach (Decision decision in decisions)
                {
                    _db.Decisions.Remove(decision);
                    _db.SaveChanges();
                }
                TempData["AlertMessage"] = "Deleted successfully";
                return RedirectToAction("AnswersHistory");
            }
            TempData["AlertMessage"] = "You must select";
            return RedirectToAction("AnswersHistory");

        }

        [HttpPost]
        public IActionResult DeleteAllHistory()
        {
            var user = HttpContext.User;
            //foreach (var item in _db.Decisions)
            //{
            //    _db.Decisions.Remove(item);
            //}
            var decisions = _db.Decisions.Where(x => x.ApplicationUserId == user.FindFirst(ClaimTypes.NameIdentifier).Value);
            _db.Decisions.RemoveRange(decisions);
            _db.SaveChanges();
            return RedirectToAction("AnswersHistory");
        }

        //public IActionResult DeleteHistory(IEnumerable<int> decisionIdsToDelete)
        //{
        //    _db.Decisions.Where(x => decisionIdsToDelete.Contains(x.Id)).ToList().ForEach(y => _db.Decisions.Remove(y));
        //    _db.SaveChanges();
        //    return RedirectToAction("AnswersHistory");
        //}

        [HttpGet]
        public IActionResult Download() //don't show russian letters, and not separate fields in rows
        {
            var user = HttpContext.User;
            var decisions = _db.Decisions.Where(x => x.ApplicationUserId == user.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (decisions.Count() != 0)
            {
                string[] dataShows = new string[] { "User Question Answer Date Of Question" };
                //var decisions = _db.Decisions.Where(x => x.ApplicationUserId == user.FindFirst(ClaimTypes.NameIdentifier).Value);
                string csv = string.Empty;
                foreach (string dataShow in dataShows)
                {
                    csv += dataShow + ',';
                }
                csv += "\r\n";

                foreach (var decision in decisions)
                {
                    csv += user.Identity.Name.Replace(",", ";") + ',';
                    csv += decision.Question.Replace(",", ";") + ',';
                    csv += decision.Answer.Replace(",", ";") + ',';
                    csv += decision.DateOfQuestion;
                    csv += "\r\n";
                }
                //byte[] bytes = Encoding.ASCII.GetBytes(csv);
                //byte[] bytes = Encoding.UTF8.GetBytes(csv);
                byte[] bytes = Encoding.Unicode.GetBytes(csv);
                return File(bytes, "text/csv", user.Identity.Name + " Answers.csv");
            }
            else
            {
                TempData["AlertMessage"] = "No data";
                return RedirectToAction("AnswersHistory");
            }

        }

        [HttpGet]
        public async Task<IActionResult> LastQuestions()
        {
            if (User.Identity.IsAuthenticated)
            {
                var questions = await _db.Decisions.OrderByDescending(x => x.DateOfQuestion).Take(20).Select(p=>p.Question).ToListAsync();  
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
