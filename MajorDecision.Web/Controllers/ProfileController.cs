using MajorDecision.Web.Data;
using MajorDecision.Web.Data.Repositories.Abstract;
using MajorDecision.Web.Models;
using MajorDecision.Web.Models.Entities;
using MajorDecision.Web.Models.ViewModels;
using MajorDecision.Web.Models.ViewModels.Authentication;
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
        private readonly INotificationService _notifications;

        public ProfileController(UserManager<ApplicationUser> userManager, IWebHostEnvironment hostingEnvironment,
                                IAuthenticationService service, ApplicationDbContext db, INotificationService notifications)
        {
            _userManager = userManager;
            _hostingEnvironment = hostingEnvironment;
            _service = service;
            _db = db;
            _notifications = notifications;
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
            var user = await _userManager.GetUserAsync(User);
            //var user = HttpContext.User; FindFirst(ClaimTypes.NameIdentifier).Value);
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null & str == "Delete")
            {
                var decisionPages = _db.DiscussionPages.Where(x => x.ApplicationUserId == user.Id);
                var decisions = _db.Decisions.Where(x => x.ApplicationUserId == user.Id);
                var friends = _db.Friends.Where(x => x.SenderId == user.Id || x.ReceiverId == user.Id);
                var notifications = _db.Notifications.Where(x => x.SenderId == user.Id || x.ReceiverId == user.Id);
                _db.Decisions.RemoveRange(decisions);
                _db.DiscussionPages.RemoveRange(decisionPages);
                _db.Friends.RemoveRange(friends);
                _db.Notifications.RemoveRange(notifications);
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

        [HttpGet]
        public async Task<IActionResult> Notifications()
        {
            var user = await _userManager.GetUserAsync(User);
            var notifications = await _notifications.GetUnreadNotificationsForUserAsync(user.Id);
            var notificationsVM = new List<NotificationVM>();
            foreach (var notification in notifications)
            {
                var notificationVM = new NotificationVM
                {
                    Id = notification.Id,
                    Message = notification.Message,
                    SenderId = notification.SenderId,
                    //SenderId = notif.UserId
                    Type = notification.Type
                };
                notification.IsRead = true;
                notificationsVM.Add(notificationVM);
                //_db.Notifications.Update(n);
                //await _db.SaveChangesAsync();
            }

            return View(notificationsVM);
        }


        [HttpPost]
        public async Task<IActionResult> AcceptRequestToFriend(string senderId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            //var not = await _db.Notifications.FindAsync(id);
            //var request = not.UserId;
            //var a = await _db.Friends.First(currentUser);
            if (string.IsNullOrEmpty(senderId) || string.IsNullOrEmpty(currentUser.Id))
            {
                TempData["AlertMessage"] = "Error";
                return RedirectToAction("Notifications", "Profile");
            }
            var friendship = await _db.Friends.FirstOrDefaultAsync(f => f.SenderId == senderId && f.ReceiverId == currentUser.Id &&
            f.Statuses == AppUserFriendship.Status.Pending);
            if (friendship == null)
            {
                TempData["AlertMessage"] = "Error";
                return RedirectToAction("Notifications", "Profile");
            }
            friendship.Statuses = AppUserFriendship.Status.Accepted;
            friendship.BecameFriendsDate = DateTime.UtcNow;
            var receiver = await _userManager.FindByIdAsync(currentUser.Id);
            var notificationForSender = new Notification
            {
                ReceiverId = senderId,
                SenderId = receiver.Id,
                //UserId = receiver.Id,
                Message = $"User {receiver.UserName} accepted your friend request",
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                Type = "Simple"
            };
            await _db.Notifications.AddAsync(notificationForSender);
            var notificationToDelete = await _db.Notifications.FirstOrDefaultAsync(x => x.ReceiverId == currentUser.Id && x.SenderId == senderId);
            if (notificationToDelete != null)
            {
                _db.Notifications.Remove(notificationToDelete);
            }
            await _db.SaveChangesAsync();
            return RedirectToAction("Notifications", "Profile");
        }

        [HttpPost]
        public async Task<IActionResult> DeclineRequestToFriend(string senderId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (string.IsNullOrEmpty(senderId) || string.IsNullOrEmpty(currentUser.Id))
            {
                TempData["AlertMessage"] = "Error";
                return RedirectToAction("Notifications", "Profile");
            }
            var friendship = await _db.Friends.FirstOrDefaultAsync(f => f.SenderId == senderId && f.ReceiverId == currentUser.Id && f.Statuses == AppUserFriendship.Status.Pending);
            if (friendship == null)
            {
                TempData["AlertMessage"] = "Error";
                return RedirectToAction("Notifications", "Profile");
            }

            friendship.Statuses = AppUserFriendship.Status.Rejected;

            var notificationToDelete = await _db.Notifications.FirstOrDefaultAsync(x => x.ReceiverId == currentUser.Id && x.SenderId == senderId);
            if (notificationToDelete != null)
            {
                _db.Notifications.Remove(notificationToDelete);
            }
            await _db.SaveChangesAsync();
            return RedirectToAction("Notifications", "Profile");
        }

        [HttpPost]
        public async Task<IActionResult> ReadCheckConfirmNotification(int id)
        {
            var notificationToDelete = await _db.Notifications.FindAsync(id);
            if (notificationToDelete != null)
            {
                _db.Notifications.Remove(notificationToDelete);
                await _db.SaveChangesAsync();
                return RedirectToAction("Notifications", "Profile");
            }
            else
            {
                TempData["AlertMessage"] = "Error";
                return RedirectToAction("Notifications", "Profile");
            }

        }

        [HttpGet]
        public async Task<IActionResult> GetFriendsList()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            //var friendships = await _db.Friends.Where(f => f.Statuses == AppUserFriendship.Status.Accepted &&
            //        (f.SenderId == currentUser.Id || f.ReceiverId == currentUser.Id)).ToListAsync();
            var friends = await _db.Friends.Where(f => f.Statuses == AppUserFriendship.Status.Accepted &&
               (f.SenderId == currentUser.Id || f.ReceiverId == currentUser.Id)).Select(f => f.SenderId == currentUser.Id ? f.Receiver : f.Sender).ToListAsync();
            //var friendIds = friendships.Select(f => f.SenderId == currentUser.Id ? f.ReceiverId : f.SenderId).ToList();
            //var friends = await _db.Users.Where(u => friendIds.Contains(u.Id)).ToListAsync();
            return View(friends);
        }
    }
}
