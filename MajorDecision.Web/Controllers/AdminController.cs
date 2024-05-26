using MajorDecision.Web.Data;
using MajorDecision.Web.Data.Repositories.Abstract;
using MajorDecision.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MajorDecision.Web.Controllers
{
    [Authorize(Roles ="Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        IWebHostEnvironment _hostingEnvironment;
        private readonly IAuthenticationService _service;
        private readonly ApplicationDbContext _db;

        public AdminController(UserManager<ApplicationUser> userManager, IWebHostEnvironment hostingEnvironment, IAuthenticationService service, ApplicationDbContext db)
        {
            _userManager = userManager;
            _hostingEnvironment = hostingEnvironment;
            _service = service;
            _db = db;
        }
        public IActionResult Display()
        {
            var users = _db.Users.ToList();
            return View(users.ToList());
        }

        public async Task<IActionResult> ClearDataWithoutApplicationUserId()        
        {
            var decisionsWithoutApplicationUserId = _db.Decisions.Where(decisions => decisions.ApplicationUserId == null).ToList();
            if (decisionsWithoutApplicationUserId.Count > 0)
            {
                _db.Decisions.RemoveRange(decisionsWithoutApplicationUserId);
                await _db.SaveChangesAsync();
                return RedirectToAction("Display", "Admin", TempData["msg"] = "data has been deleted");
            }
            
            return RedirectToAction("Display", "Admin", TempData["msg"] = "No data");
        }

    }
}
