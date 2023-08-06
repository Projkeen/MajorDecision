using MajorDecision.Web.Data;
using MajorDecision.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System;
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
            Random random = new Random();
            int rnd=random.Next(100);
            decision.Answer = decision.Question.Length.ToString();
            int ans=int.Parse(decision.Answer);
            if ((ans % 2 == 0) && (rnd % 2 == 0))
            {
                decision.Answer = "yes";
            }
            else if((ans % 2 != 0) && (rnd % 2 != 0))
            {
                decision.Answer = "yes";
            }
            else
            {
                decision.Answer = "no";
            }       

            _db.Decisions.Add(decision);
            _db.SaveChanges();
            //return RedirectToAction("Index","Decision");
            return RedirectToAction(nameof(Answer));
        }

        public IActionResult Answer()
        {
            List<Decision> decisions = _db.Decisions.ToList();
             
            return View(decisions);
        }


    }
}
