using MajorDecision.Web.Data;
using MajorDecision.Web.Data.Repositories.Abstract;
using MajorDecision.Web.Models;
using MajorDecision.Web.Models.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using System.Security.Claims;

namespace MajorDecision.Web.Controllers
{
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        IWebHostEnvironment _hostingEnvironment;

        public ProfileController(UserManager<ApplicationUser> userManager, IWebHostEnvironment hostingEnvironment)
        {
            _userManager = userManager;
            _hostingEnvironment = hostingEnvironment;
        }

        //public void DisplayUser()
        //{
        //    ViewBag.Profile = _userManager.Users.Where(x => x.Id.Equals(HttpContext.User)).FirstOrDefault();
        //} 

        [HttpGet]
        public async Task<IActionResult> ManageProfile()
        {
            ModelState.Clear();
            var user = await _userManager.GetUserAsync(User);
            //var user = HttpContext.User;
            //var user = await _userManager.FindByIdASync(Id);
            //DisplayUser();           
            var userClaims = await _userManager.GetClaimsAsync(user);
            var userRoles = await _userManager.GetRolesAsync(user);

            var model = new UserViewModel
            {
                Id = user.Id,
                FirstName = user.Name,
                Username = user.UserName,
                Email = user.Email,
                Claims = userClaims.Select(c => c.Value).ToList(),
                Roles = userRoles,
            };

            ViewData["Photo"] = user.ProfilePicture;

            return View(model);
        }

        public async Task<IActionResult> UploadImage()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage(UserViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            //var currentUser = await _userManager.FindByIdAsync(model.Id);
            //var currentUser = User.Identity.Name;
            string uniqueFileName = null;
            if (model.Photo != null)
            {
                string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "images", "profileImages");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Photo.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                model.Photo.CopyTo(new FileStream(filePath, FileMode.Create));
            }

            currentUser.ProfilePicture = uniqueFileName;


            //ApplicationUser updateUser = new ApplicationUser
            //{
            //    ProfilePicture = uniqueFileName
            //};
            await _userManager.UpdateAsync(currentUser);

            //var result= _userManager.Users.Add(updateUser);
            return RedirectToAction("ManageProfile");
        }

        public async Task<IActionResult> EditUser()
        {
            //var user = await _userManager.GetUserAsync(User);
            //var userClaims = await _userManager.GetClaimsAsync(user);
            //var userRoles = await _userManager.GetRolesAsync(user);

            //var model = new UserViewModel
            //{
            //    Id = user.Id,
            //    FirstName = user.Name,
            //    Username = user.UserName,
            //    Email = user.Email,
            //    Claims = userClaims.Select(c => c.Value).ToList(),
            //    Roles = userRoles,
            //};
            //return PartialView("_EditUserPartialView");
            //return View("ManageProfile");
            return View();

        }

        [HttpPost]
        public async Task<IActionResult> EditUser(UserViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            var userExists = await _userManager.FindByNameAsync(model.Username);
            user.UserName = model.Username;
            user.Email = model.Email;
            user.Name = model.FirstName;
            if (user.UserName != null && user.Name != null)
            {
                await _userManager.UpdateAsync(user);
                TempData["msg"] = "User data has updated";
                return RedirectToAction("ManageProfile");
            }           
            else
            {
                TempData["msg"] = "Smthg wrong (This username already using or username must be entered)";
                return RedirectToAction("ManageProfile");
            }
            //var result = await _userManager.UpdateAsync(user);
            //if (result.Succeeded)
            //{
            //    TempData["msg"] = "User data has updated";
            //    return RedirectToAction("ManageProfile");
            //}
            //else
            //{
            //    TempData["msg"] = "Smthg wrong (This username already using or username must be entered)";
            //    return RedirectToAction("ManageProfile");
            //}            
        }
    }
}
