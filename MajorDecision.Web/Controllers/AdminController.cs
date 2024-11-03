using MajorDecision.Web.Data;
using MajorDecision.Web.Data.Repositories.Abstract;
using MajorDecision.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Collections.Immutable;
using System.Data;
using System.Reflection.Metadata;
using System.Security.Claims;

namespace MajorDecision.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;
        private readonly RoleManager<IdentityRole> _roleManager;
        IWebHostEnvironment _hostingEnvironment;

        public AdminController(UserManager<ApplicationUser> userManager, IWebHostEnvironment hostingEnvironment, RoleManager<IdentityRole> roleManager,
            ApplicationDbContext db)
        {
            _userManager = userManager;
            _db = db;
            _roleManager = roleManager;
            _hostingEnvironment = hostingEnvironment;
        }
        public async Task<IActionResult> DisplayUsers()
        {
            List<UserViewModel> userViewModels = new List<UserViewModel>();
            var currentUser = await _userManager.GetUserAsync(User);
            var users = await _db.Users.Where(u => u.Id != currentUser.Id).ToListAsync();
            //var model=new UserViewModel
            foreach (var user in users)
            {
                UserViewModel model = new UserViewModel();
                model.Id = user.Id;
                model.Username = user.UserName;
                model.FirstName = user.Name;
                model.Email = user.Email;
                userViewModels.Add(model);
            }
            return View(userViewModels);

            //return View(users);
            //var users = _db.Users.ToList();
            //var roleAdmin = _db.UserRoles.Find("Admin");
            //var roleUser = _db.Users.Find("User");
            //var model = new List<UserViewModel>();
            //{
            //    foreach (var user in model)
            //    { Id = user.Id;
            //    FirstName = user.Name,
            //    Username = user.UserName,
            //    Email = user.Email,
            //    Claims = userClaims.Select(c => c.Value).ToList(),
            //    Roles = userRoles,}

            //};
            //ApplicationUser user = await _userManager.FindByIdAsync(model.Id);
            //if (user != null)
            //{
            //    user.UserName = model.Username;
            //    user.Name = model.FirstName;
            //    user.Email = model.Email;
            //    IdentityResult result = await _userManager.UpdateAsync(user);
            //    if (result.Succeeded)
            //    {
            //        return RedirectToAction("ManageProfile", TempData["msg"] = "User data was updated!");
            //    }
            //}
            //return RedirectToAction("ManageProfile", TempData["msg"] = "Smthg wrong (This username already using or username must be entered)");

            //return View(users.ToList());
        }

        public string GetRole(string Id)
        {
            var userRole = _db.Users.Where(u => u.Id == Id).Join(_roleManager.Roles, u => u.Id, r => r.Id, (u, r) => r.Name).FirstOrDefault();


            return userRole;
        }

        [HttpGet]
        public async Task<IActionResult> ChangeUserRole(string Id)
        {
            //var model = new List<UserRoleViewModel>();
            //foreach (var user in _db.Users)
            //{
            //    var userRoleViewModel = new UserRoleViewModel
            //    {
            //        UserId = user.Id,
            //        UserName = user.UserName

            //    };
            //    model.Add(userRoleViewModel);
            //}
            //return View(model);
            //var role = await _roleManager.FindByIdAsync(Id);
            //EditRoleViewModel model = new EditRoleViewModel
            //{
            //    RoleId = role.Id,
            //    RoleName = role.Name
            //};

            //model.Users = new List<string>();
            //foreach(var user in _userManager.Users.ToList())
            //{
            //    if (await _userManager.IsInRoleAsync(user, role.Name))
            //    {
            //        model.Users.Add(user.UserName);
            //    }

            //}
            //ChangeRoleViewModel userRoles = new ChangeRoleViewModel();
            var user = _db.Users.Where(x => x.Id == Id).SingleOrDefault();
            //var userInRole = _db.UserRoles.Where(x => x.UserId == Id).Select(x => x.RoleId).ToList();
            //var userInRole = _db.UserRoles.Where(x => x.UserId == Id).Select(x => x.RoleId).Contains("User").ToString();
            var roles = await _userManager.GetRolesAsync(user);
            var ifAdmin = roles.Single();
            //var j = roles.First();
            string roleName = "Admin";
            var role = await _roleManager.Roles.SingleAsync(r => r.Name == roleName);
            //var roleName=_db.UserRoles.Where(r=>r.UserId==Id && r.RoleId==roles.ToString()).Select(r=>r.)
            //userInRole.ToString();
            if (ifAdmin == "Admin")//role.Name)
                                   //"1d4d9a0b-80aa-47ad-baf0-027d4b5b1225"
                                   //(await _roleManager.RoleExistsAsync("User"))
            {
                await _userManager.RemoveFromRoleAsync(user, "Admin");
                await _userManager.AddToRoleAsync(user, "User");
                _db.SaveChanges();
                TempData["msg"] = $"User {user.UserName} has the role <User>";
            }
            else
            {
                await _userManager.RemoveFromRoleAsync(user, "User");
                await _userManager.AddToRoleAsync(user, "Admin");
                _db.SaveChanges();
                TempData["msg"] = $"User {user.UserName} has the role <Admin>";
            }
            //model.Users = new;
            //foreach (var role in await _roleManager.Roles.ToListAsync())
            //{
            //    var changeRoleViewModel = new ChangeRoleViewModel
            //    {
            //        Role = role.Name
            //    };

            //    changeRoleViewModel.Role=
            //}

            //userInRole.ToString();
            //userRoles.Roles=await _roleManager.Roles.Select(x=>new )


            return RedirectToAction("DisplayUsers", "Admin");
        }

        public async Task<IActionResult> ClearDataWithoutApplicationUserId()
        {
            var decisionsWithoutApplicationUserId = _db.Decisions.Where(decisions => decisions.ApplicationUserId == null).ToList();
            if (decisionsWithoutApplicationUserId.Count > 0)
            {
                _db.Decisions.RemoveRange(decisionsWithoutApplicationUserId);
                await _db.SaveChangesAsync();
                return RedirectToAction("DisplayUsers", "Admin", TempData["msg"] = "Data has been deleted");
            }

            return RedirectToAction("DisplayUsers", "Admin", TempData["msg"] = "No data");
        }

        [HttpGet]
        public async Task<IActionResult> UserInfo(string Id)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(Id);
            var userClaims = await _userManager.GetClaimsAsync(user);
            var userRoles = await _userManager.GetRolesAsync(user);
            //var user = _db.Users.Where(x => x.Id == Id).SingleOrDefault();
            UserViewModel model = new UserViewModel
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
        //[HttpPost]
        //public async Task<IActionResult> UserInfo(UserViewModel model)
        //{
        //    ApplicationUser user = await _userManager.FindByIdAsync(model.Id);
        //    //var user = _db.Users.Where(x => x.Id == Id).SingleOrDefault();
        //    //UserViewModel model = new UserViewModel();
        //    if (user != null)
        //    {
        //        user.UserName = model.Username;
        //        user.Name = model.FirstName;
        //        user.Email = model.Email;
        //        user.ProfilePicture = model.Photo.ToString();
        //    };

        //    return RedirectToAction("UserInfo", "Admin");
        //ApplicationUser user = await _userManager.FindByIdAsync(model.Id);
        //if (user != null)
        //{
        //    user.UserName = model.Username;
        //    user.Name = model.FirstName;
        //    user.Email = model.Email;
        //    IdentityResult result = await _userManager.UpdateAsync(user);
        //    if (result.Succeeded)
        //    {
        //        return RedirectToAction("ManageProfile", TempData["msg"] = "User data was updated!");
        //    }
        //}
        //return RedirectToAction("ManageProfile", TempData["msg"] = "Smthg wrong (This username already using or username must be entered)");

        //public async Task<IActionResult> CreateAdminRole(RoleViewModel model)
        //{

        //    IdentityRole identityRole = new IdentityRole()
        //    {
        //        //Name = model.Role
        //        Name = "Admin"
        //    };
        //        IdentityResult result = await _roleManager.CreateAsync(identityRole);

        //        if (result.Succeeded)
        //        {
        //            return RedirectToAction("Display", "Admin", TempData["msg"] = "Role created!");
        //        }                


        //    return RedirectToAction("Display", "Admin", TempData["msg"] = "Error!");
        //}

        //[HttpPost]
        //public async Task <IActionResult> 

        public async Task<IActionResult> DisplayAnswers()
        {
            var answers = await _db.Answers.ToListAsync();
            return View(answers);
        }


        public IActionResult AddAnswerSentence(Answers newAnswer)
        {
            if (ModelState.IsValid)
            {
                _db.Answers.Add(newAnswer);
                _db.SaveChanges();
                return RedirectToAction("DisplayAnswers");
            }
            return View();
        }

        public IActionResult Manage(int id)
        {
            var answer = _db.Answers.Find(id);
            return View(answer);
        }


        [HttpPost]
        public IActionResult EditAnswerSentence(Answers modAnswer)
        {
            if (ModelState.IsValid)
            {
                _db.Answers.Update(modAnswer);
                _db.SaveChanges();
                return RedirectToAction("DisplayAnswers");
            }
            TempData["msg"] = "Field <Answer> is required";
            return RedirectToAction("Manage");
        }

        [HttpPost]
        public IActionResult DeleteAnswerSentence(int id)
        {
            var modAnswer= _db.Answers.Find(id);
            _db.Answers.Remove(modAnswer);
            _db.SaveChanges();
            return RedirectToAction("DisplayAnswers");
        }

        public async Task<IActionResult> ClearAllProfilePictures()
        {
            string wwwrootPath = Path.Combine(_hostingEnvironment.WebRootPath, "images", "profileImages");

            if (Directory.Exists(wwwrootPath))
            {
                foreach (string file in Directory.GetFiles(wwwrootPath))
                {
                    System.IO.File.Delete(file);
                }
            }
            else
            {
                return RedirectToAction("DisplayUsers", "Admin", TempData["msg"] = "No folder");                
            }

            var usersWithProfilePicture = _db.Users.Where(u => u.ProfilePicture != null).ToList();
            //string empty = null;
            if (usersWithProfilePicture.Count > 0)
            {
                foreach (var user in usersWithProfilePicture)
                {
                    user.ProfilePicture = null;
                }
                _db.SaveChanges();
            }
            else
            {
                return RedirectToAction("DisplayUsers", "Admin", TempData["msg"] = "No photo");
            }    
            return RedirectToAction("DisplayUsers", "Admin", TempData["msg"] = "Profile photos have been deleted");
        }
    }
    
}
