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

namespace MajorDecision.Web.Controllers
{
    //[Authorize]
    public class DecisionController : Controller
    {
        private readonly ApplicationDbContext _db;

        public DecisionController(ApplicationDbContext db)
        {
            _db = db;
        }

        //private static List<Decision> answer = new List<Decision>();
        public IActionResult Index()
        {
            //List<Decision> decisions = _db.Decisions.ToList();
            //return View(decisions);
            return View();

        }

        [HttpPost]
        public IActionResult Index(Decision decision, string lucky)
        {

            var user = HttpContext.User;
            //var userId = _db.UserLogins.Find(ClaimTypes.NameIdentifier).UserId;
            if (lucky == "answer")
            {
                if (decision.Question != null)
                {
                    //Decision decision = new Decision();
                    string answersPath = $"{Environment.CurrentDirectory}\\Answers.json";
                    string[] Answers;
                    Random random = new Random();

                    var file = System.IO.File.ReadAllText(answersPath);
                    Answers = JsonConvert.DeserializeObject<string[]>(file);
                    int index = random.Next(Answers.Length);
                    decision.Answer = Answers[index];
                    decision.DateOfQuestion = DateTime.Now;
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

            }
            else
            {
                if (decision.Question != null)
                {
                    Random random = new Random();
                    int rnd = random.Next(100);
                    decision.Answer = decision.Question.Length.ToString();
                    int ans = int.Parse(decision.Answer);
                    if ((ans % 2 == 0) && (rnd % 2 == 0))
                    {
                        decision.Answer = "yes";
                    }
                    else if ((ans % 2 != 0) && (rnd % 2 != 0))
                    {
                        decision.Answer = "yes";
                    }
                    else
                    {
                        decision.Answer = "no";
                    }

                    decision.DateOfQuestion = DateTime.Now;
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


                    //return RedirectToAction(nameof(Answer));
                }

            }

            //return RedirectToAction("Index","Decision");
            //return decision.Answer;
            return View();
        }


        public  IActionResult AnswersHistory()
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
               
                var decisions = _db.Decisions.Where(x => x.ApplicationUserId == user.FindFirst(ClaimTypes.NameIdentifier).Value);
                return View(decisions.ToList());
            }
            else
            {
                return RedirectToAction("Login", "Authentication");
            }
               
        }

        public IActionResult SearchHistory(string searchString)
        {
            var user = HttpContext.User;
            var decisions = from d in _db.Decisions.Where(x => x.ApplicationUserId == user.FindFirst(ClaimTypes.NameIdentifier).Value) select d;
            if (!String.IsNullOrEmpty(searchString))
            {
                decisions = decisions.Where(d => d.Answer.Contains(searchString) || d.Question.Contains(searchString));

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
            if (decisionIdsToDelete == null)
            {
                TempData["AlertMessage"] = "you must select";
                return RedirectToAction("AnswersHistory");
            }
            else
            {
                List<Decision> decisions = _db.Decisions.Where(x => decisionIdsToDelete.Contains(x.Id)).ToList();
                foreach (Decision decision in decisions)
                {
                    _db.Decisions.Remove(decision);
                }
                _db.SaveChanges();
                return RedirectToAction("AnswersHistory");
            }
            
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


    }

}
