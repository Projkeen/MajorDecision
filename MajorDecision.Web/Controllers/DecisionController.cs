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
using MajorDecision.Web.Models.ViewModels;
using MajorDecision.Web.Models.Entities;
using static MajorDecision.Web.Data.AppUserFriendship;

namespace MajorDecision.Web.Controllers
{
    //[Authorize]
    public class DecisionController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IDecision? _decision;
        private readonly UserManager<ApplicationUser> _userManager;
        public DecisionController(IDecision decision, ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _decision = decision;
            _userManager = userManager;
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
            //var user = HttpContext.User; (user.FindFirst(ClaimTypes.NameIdentifier).Value)
            var user = await _userManager.GetUserAsync(User);   
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
                    var answersWithAppUserId = _db.Decisions.Where(x => x.ApplicationUserId != null & x.ApplicationUserId != user.Id).ToList();
                    var checkSameQuestions = answersWithAppUserId.Where(i => i.Question == decision.Question).ToList();
                    var questionsFromDiscussion = _db.DiscussionPages.Where(x => x.Question == decision.Question).ToList();
                    var findReceiverIds = checkSameQuestions.Select(i => i.ApplicationUserId).Distinct().ToList();
                    if (questionsFromDiscussion != null & (questionsFromDiscussion.ToString().ToLower() == decision.Question.ToLower()))
                    {
                        decision.ApplicationUserId = user.Id;
                        _db.Decisions.Add(decision);
                        await _db.SaveChangesAsync();
                        ModelState.Clear();
                    }
                    else if (findReceiverIds.Count > 0)
                    {
                        await UndergroundMethod(user.Id, findReceiverIds);
                    }
                    decision.ApplicationUserId = user.Id;
                    _db.Decisions.Add(decision);
                    await _db.SaveChangesAsync();
                    ModelState.Clear();                   
                }
                else
                {
                    _db.Decisions.Add(decision);
                    _db.SaveChanges();
                    ModelState.Clear();                    
                }
                ViewBag.message = decision.Answer;
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

        public async Task UndergroundMethod(string SenderId, List<string> ReceiverIds)
        {
            // var currentUser= await _userManager.GetUserAsync(User);
            var sender = await _userManager.FindByIdAsync(SenderId);
            //if (SenderId == ReceiverId)
            //{
            //    return;
            //}

            // Check already sending request

            //var existingRequest = await _db.Friends
            //    .FirstOrDefaultAsync(fr =>
            //        (fr.SenderId == SenderId && fr.ReceiverId == ReceiverId) ||
            //        (fr.SenderId == ReceiverId && fr.ReceiverId == SenderId && fr.Statuses != Status.Rejected) ||
            //        (fr.Statuses == Status.Accepted &&
            //         ((fr.SenderId == SenderId && fr.ReceiverId == ReceiverId) || (fr.SenderId == ReceiverId && fr.ReceiverId == SenderId)))
            //    );


            var existingRequest = await _db.Friends.FirstOrDefaultAsync(fr =>
                    (fr.SenderId == SenderId && ReceiverIds.Contains(fr.ReceiverId)) ||
                    (fr.ReceiverId == SenderId && ReceiverIds.Contains(fr.SenderId) && fr.Statuses != Status.Rejected));

            //var existingRequest = await _db.Friends
            //    .FirstOrDefaultAsync(fr =>
            //        (fr.SenderId == SenderId && fr.ReceiverId == ReceiverId) ||
            //        (fr.SenderId == ReceiverId && fr.ReceiverId == SenderId && fr.Status != "Rejected") ||
            //        (fr.Status == "Accepted" &&
            //         ((fr.SenderId == SenderId && fr.ReceiverId == ReceiverId) || (fr.SenderId == ReceiverId && fr.ReceiverId == SenderId)))
            //    );

            if (existingRequest != null)
            {
                return;
            }

            // creating requests for friendship

            var requests = new List<AppUserFriendship>();
            var notifications = new List<Notification>();

            foreach (var receiverId in ReceiverIds)
            {
                var request = new AppUserFriendship
                {
                    SenderId = SenderId,
                    ReceiverId = receiverId,
                    Statuses = Status.Pending,
                    RequestDate = DateTime.UtcNow
                };
                requests.Add(request);
                var notification = new Notification
                {
                    ReceiverId = receiverId,
                    SenderId = SenderId,
                    Message = $"User {sender.Name} sent a friend request",
                    CreatedAt = DateTime.Now,
                    IsRead = false,
                    Type = "Action"
                };
                notifications.Add(notification);
            }

            //var request = new AppUserFriendship
            //{
            //    SenderId = SenderId,
            //    ReceiverId = ReceiverId,
            //    Statuses = Status.Pending,
            //    RequestDate = DateTime.UtcNow
            //};

            _db.Friends.AddRange(requests);
            _db.Notifications.AddRange(notifications);
            await _db.SaveChangesAsync();
        }
    }
}

