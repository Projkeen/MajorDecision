using MajorDecision.Web.Data;
using MajorDecision.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Net;

namespace MajorDecision.Web.Controllers
{
    public class DecisionController : Controller
    {
        private readonly ApplicationDbContext _db;
        
        public DecisionController(ApplicationDbContext db)
        {
                _db=db;
        }

        //private static List<Decision> answer = new List<Decision>();
        public IActionResult Index()
        {
            //List<Decision> decisions = _db.Decisions.ToList();
            //return View(decisions);
            return View(); 
            
        }

        [HttpPost]
        public IActionResult Index(Decision decision)
        {
            if (ModelState.IsValid)
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

                _db.Decisions.Add(decision);
                _db.SaveChanges();
                ViewBag.message = decision.Answer;
                return View();
                //return RedirectToAction(nameof(Answer));
            }
            //return RedirectToAction("Index","Decision");
            //return decision.Answer;
            return View();
        }

        public IActionResult AnswersHistory()
        {
            //ViewBag.Id = id;
            //Decision? decisionFromDb = _db.Decisions.FirstOrDefault(u=>u.Id==id);
            List<Decision> decisions = _db.Decisions.ToList();
            //List<Decision> decisions = _db.Decisions.Single(i => i.Id == 0);
            //Decision? decisionFromDb = _db.Decisions.OrderBy(u=>u.Id).Last();
            //return View(decisionFromDb);
            //List<Decision> answer = _db.Decisions.SingleOrDefault();

            return View(decisions);
        }

        //public IActionResult Answer()
        //{
        //    //List<Decision> decisions = _db.Decisions.ToList();
        //    //Decision? decisionFromDb = _db.Decisions.Find(id);
        //    return View();
        //}


    }
}
