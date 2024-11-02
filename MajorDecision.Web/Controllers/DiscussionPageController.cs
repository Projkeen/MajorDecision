using MajorDecision.Web.Data.Repositories.Abstract;
using MajorDecision.Web.Data;
using MajorDecision.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace MajorDecision.Web.Controllers
{
    public class DiscussionPageController : Controller
    {
        private IPageService _pageService;
        private readonly IDecision? _decision;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public DiscussionPageController(IPageService pageService, IDecision? decision, ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _pageService = pageService;
            _decision = decision;
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> DisplayPages()
        {
            if (User.Identity.IsAuthenticated)
            {
                var pages = _pageService.GetPages();
                return View(await pages.ToListAsync());
            }
            else
            {
                TempData["msg"] = "You must be loggin in";
                return RedirectToAction("Login", "Authentication");
            }
        }


        public async Task<IActionResult> CreatePage(int Id)
        {
            var user = HttpContext.User;
            var currentUser = await _userManager.GetUserAsync(User);
            //var user = await _userManager.GetUserAsync(User);
            var decision = _db.Decisions.FirstOrDefault(f => f.Id == Id);
            var page = new DiscussionPage();
            page.Question = decision.Question;
            page.Answer = decision.Answer;
            page.Image = currentUser.ProfilePicture;
            page.DateOfCreating = DateTime.Now;
            //page.UserPhoto = _userManager.GetUserId(user.ProfilePicture);
            //post.ApplicationUserId = user.FindFirst(ClaimTypes.NameIdentifier).Value;
            page.ApplicationUserId = _userManager.GetUserId(user);
            page.ApplicationUser = currentUser;
            //post.ApplicationUser = user.Identity.Name.ToList();
            await _pageService.Create(page);
            //return PartialView("_PostPartialView", post);
            return RedirectToAction("DisplayPages");
        }

        [HttpGet]
        public async Task<IActionResult> DeletePage(int Id)
        {
            var user = HttpContext.User;
            var page = await _pageService.GetById(Id);
            _db.DiscussionPages.Remove(page);
            _db.SaveChanges();
            if (user.IsInRole("Admin"))
            {
                TempData["msg"] = $"Page with Question <{page.Question}> has been deleted";
            }
            else
            {
                TempData["msg"] = $"Your Page <{page.Question}> has been deleted";
            }
            return RedirectToAction("DisplayPages");
        }        

        [HttpGet]
        public async Task<IActionResult> Discussion(int id)
        {
            var page = await _pageService.GetById(id);
            return View(page);
        }

        [HttpPost]
        public async Task<IActionResult> AddDescription(DiscussionPage page)
        {
            var model = await _pageService.GetById(page.Id);
            //var decision = _db.DiscussionPage.FirstOrDefault(f => f.Id == Id);
            //var page = new DiscussionPage();

            if (model != null)
            {
                model.Description = page.Description;
                _db.SaveChanges();
                return View("Discussion", model);
            }
            return View("Discussion", model);
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(Comment comment)
        {
            comment.DateOfComment = DateTime.Now;
            _db.Comments.Add(comment);
            _db.SaveChanges();
            var model = await _pageService.GetById(comment.DiscussionPageId);
            //var decision = _db.DiscussionPages.FirstOrDefault(f => f.Id == Id);
            //var page = new DiscussionPage();                        
            return RedirectToAction("Discussion", model);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteComment(int Id)
        {
            var user = HttpContext.User;
            var page = _db.Comments.Where(c => c.Id == Id).Select(c => c.DiscussionPages).FirstOrDefault();
            var comment = await _db.Comments.FindAsync(Id);
            _db.Comments.Remove(comment);
            _db.SaveChanges();
            if (user.IsInRole("Admin"))
            {
                TempData["msg"] = $"Comment<{comment.Text}> has been deleted";
            }
            else
            {
                TempData["msg"] = $"Your comment <{comment.Text}> has been deleted";
            }
            return RedirectToAction("Discussion", page);
        }
    }
}
