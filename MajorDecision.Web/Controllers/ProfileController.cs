using MajorDecision.Web.Data;
using MajorDecision.Web.Data.Repositories.Abstract;
using MajorDecision.Web.Models;
using MajorDecision.Web.Models.Authentication;
using Microsoft.AspNetCore.Hosting;
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
        private readonly IAuthenticationService _service;
        private readonly ApplicationDbContext _db;

        public ProfileController(UserManager<ApplicationUser> userManager, IWebHostEnvironment hostingEnvironment, IAuthenticationService service, ApplicationDbContext db)
        {
            _userManager = userManager;
            _hostingEnvironment = hostingEnvironment;
            _service = service;
            _db = db;
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
        public async Task<IActionResult> UploadOrDeleteImage(UserViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            //var currentUser = await _userManager.FindByIdAsync(model.Id);
            //var currentUser = User.Identity.Name;
            string uniqueFileName = null;
            if (currentUser.ProfilePicture == null)
            {
                if (model.Photo != null)
                {
                    string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "images", "profileImages");
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Photo.FileName;
                    //uniqueFileName = model.Photo.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        model.Photo.CopyTo(fileStream);
                    }
                    //model.Photo.CopyTo(new FileStream(filePath, FileMode.Create));
                        currentUser.ProfilePicture = uniqueFileName;                    
                }
                else
                {
                    return RedirectToAction("ManageProfile", TempData["msg"] = "No file");
                }
            }
            else
            {
                if (model.Photo == null)
                {
                    var fileNameForDelete = currentUser.ProfilePicture.ToString();
                    var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "images",
                        "profileImages", fileNameForDelete.ToString());

                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                    currentUser.ProfilePicture = uniqueFileName;
                }
                else
                {
                    return RedirectToAction("ManageProfile", TempData["msg"] = "Error! ");
                }                
            }
            await _userManager.UpdateAsync(currentUser);
            return RedirectToAction("ManageProfile");
            //currentUser.ProfilePicture = uniqueFileName;

            //ApplicationUser updateUser = new ApplicationUser
            //{
            //    ProfilePicture = uniqueFileName
            //};
            //await _userManager.UpdateAsync(currentUser);

            //var result= _userManager.Users.Add(updateUser);
        }


        public async Task<IActionResult> EditUser(/*string id*/)
        {
            //if (!string.IsNullOrEmpty(id))
            //{
            //    ApplicationUser user=await _userManager.FindByIdAsync(id);
            //    if(user!=null)
            //    {
            //        UserViewModel model = new UserViewModel()
            //        {
            //            FirstName = user.Name,
            //            Username = user.UserName,
            //            Id = user.Id,
            //            Email = user.Email,
            //        };
            //        return View(model);
            //    }
            //}
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
            //return View("ManageProfile");
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(UserViewModel model)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(model.Id);
            if (user != null)
            {
                user.UserName = model.Username;
                user.Name = model.FirstName;
                user.Email = model.Email;
                IdentityResult result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("ManageProfile", "Profile", TempData["msg"] = "User data was updated!");
                }
            }
            return RedirectToAction("ManageProfile", "Profile", TempData["msg"] = "Smthg wrong (This username already using or username must be entered)");


            return RedirectToAction("ManageProfile");
            //var checkUser = await _userManager.FindByNameAsync(model.Username);
            //var user = await _userManager.GetUserAsync(User);
            //user.UserName = model.Username;
            //user.Email = model.Email;
            //user.Name = model.FirstName;
            //if (user.UserName != null & user.Name != null & checkUser.ToString() != user.UserName)
            //{
            //    await _userManager.UpdateAsync(user);
            //    TempData["msg"] = "User data has updated";
            //    return RedirectToAction("ManageProfile");
            //}
            //else
            //{
            //    TempData["msg"] = "Smthg wrong (This username already using or username must be entered)";
            //    return RedirectToAction("ManageProfile");
            //}
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

        public async Task<IActionResult> DeleteAccount(string str)
        {
            var user = HttpContext.User;
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null & str == "Delete")
            {
                var decisionPages = _db.DiscussionPages.Where(x => x.ApplicationUserId == user.FindFirst(ClaimTypes.NameIdentifier).Value);
                var decisions = _db.Decisions.Where(x => x.ApplicationUserId == user.FindFirst(ClaimTypes.NameIdentifier).Value);
                _db.Decisions.RemoveRange(decisions);
                _db.DiscussionPages.RemoveRange(decisionPages);
                IdentityResult result = await _userManager.DeleteAsync(currentUser);
                if (result.Succeeded)
                {
                    await _service.LogoutAsync();
                    return RedirectToAction("Index", "Home", TempData["msg"] = "User has been deleted, we are waiting for you again");
                }
                //else
                //{
                //    return View();
                //}
            }
            return RedirectToAction("ManageProfile", TempData["msg"] = "Error");
        }
    }
}
