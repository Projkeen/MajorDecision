using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MajorDecision.Web.Controllers
{
    [Authorize(Roles ="Admin")]
    public class AdminController : Controller
    {
        public IActionResult Display()
        {
            return View();
        }
    }
}
