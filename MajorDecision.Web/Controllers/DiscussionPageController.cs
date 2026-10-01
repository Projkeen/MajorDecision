using MajorDecision.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using MajorDecision.Web.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using MajorDecision.Data.Services.Abstract;

namespace MajorDecision.Web.Controllers
{
    [Authorize]
    public class DiscussionPageController : Controller
    {
        private IPageService _pageService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DiscussionPageController(IPageService pageService, UserManager<ApplicationUser> userManager)
        {
            _pageService = pageService;
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

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePage(int Id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var page = await _pageService.CreateFromDecisionAsync(Id, currentUser);
            if (page == null)
            {
                TempData["msg"] = "Decision not found";
            }
            return RedirectToAction("DisplayPages");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePage(int Id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var (success, message) = await _pageService.DeletePageAsync(Id, currentUser!.Id, User.IsInRole("Admin"));
            TempData["msg"] = message;
            return RedirectToAction("DisplayPages");
        }

        [HttpGet]
        public async Task<IActionResult> Discussion(int id)
        {
            var page = await _pageService.GetById(id);
            return View(page);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDescription(int Id, string description)
        {
            await _pageService.AddDescriptionAsync(Id, description);
            return RedirectToAction(nameof(Discussion), new { id = Id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(Comment comment)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            await _pageService.AddCommentAsync(comment.Text, comment.DiscussionPageId, currentUser!.Id);
            return RedirectToAction(nameof(Discussion), new { id = comment.DiscussionPageId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int Id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var (success, message, pageId) = await _pageService.DeleteCommentAsync(Id, currentUser!.Id, User.IsInRole("Admin"));
            TempData["msg"] = message;

            if (pageId == null)
                return RedirectToAction(nameof(DisplayPages));

            return RedirectToAction(nameof(Discussion), new { id = pageId });
        }
    }
}
