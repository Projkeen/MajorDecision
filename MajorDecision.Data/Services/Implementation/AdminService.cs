using MajorDecision.Data;
using MajorDecision.Data.Services.Abstract;
using MajorDecision.Web.Models.Entities;
using MajorDecision.Web.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MajorDecision.Data.Services.Implementation
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;        

        public AdminService(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;            
        }

        public async Task<List<UserViewModel>> GetAllUsersAsync(string currentUserId)
        {
            var users = await _db.Users.Where(u => u.Id != currentUserId).ToListAsync();
            var result = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                result.Add(new UserViewModel
                {
                    Id = user.Id,
                    Username = user.UserName!,
                    FirstName = user.Name,
                    Email = user.Email,
                    Roles = roles,
                    ProfilePictureUrl = user.ProfilePicture ?? string.Empty
                });
            }
            return result;
        }

        public async Task<UserViewModel?> GetUserInfoAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return null;

            var claims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            return new UserViewModel
            {
                Id = user.Id,
                FirstName = user.Name,
                Username = user.UserName!,
                Email = user.Email,
                Claims = claims.Select(c => c.Value).ToList(),
                Roles = roles,
                ProfilePictureUrl = user.ProfilePicture
            };
        }

        public async Task<(bool Success, string Message)> ChangeUserRoleAsync(string userId, string adminRole = "Admin", string userRole = "User")
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return (false, "User not found");

            var isInToggledRole = await _userManager.IsInRoleAsync(user, adminRole);
            if (isInToggledRole)
            {
                await _userManager.RemoveFromRoleAsync(user, adminRole);
                if (!await _userManager.IsInRoleAsync(user, userRole))
                    await _userManager.AddToRoleAsync(user, userRole);
                return (true, $"User {user.UserName} has the role <{userRole}>");
            }
            else
            {
                await _userManager.RemoveFromRoleAsync(user, userRole);
                if (!await _userManager.IsInRoleAsync(user, adminRole))
                    await _userManager.AddToRoleAsync(user, adminRole);
                return (true, $"User {user.UserName} has the role <{adminRole}>");
            }
        }

        public async Task<int> ClearDecisionsWithoutApplicationUserIdAsync()
        {
            var decisionsWithoutApplicationUserId = await _db.Decisions.Where(d => d.ApplicationUserId == null).ToListAsync();
            if (decisionsWithoutApplicationUserId.Count == 0)
                return 0;

            _db.Decisions.RemoveRange(decisionsWithoutApplicationUserId);
            await _db.SaveChangesAsync();
            return decisionsWithoutApplicationUserId.Count;
        }

        public async Task<int> ClearAllProfilePicturesAsync(string webRootPath)
        {
            var folder = Path.Combine(webRootPath, "images", "profileImages");
            if (Directory.Exists(folder))
            {
                foreach (var file in Directory.GetFiles(folder))
                    File.Delete(file);
            }

            var usersWithPicture = await _db.Users.Where(u => u.ProfilePicture != null).ToListAsync();
            foreach (var user in usersWithPicture)
                user.ProfilePicture = null;
            await _db.SaveChangesAsync();
            return usersWithPicture.Count;
        }

        public async Task<List<Answers>> GetAnswersAsync()
        {
            var answers = await _db.Answers.ToListAsync();
            return answers;
        }

        public async Task AddAnswerAsync(Answers answer)
        {
            _db.Answers.Add(answer);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> EditAnswerAsync(Answers answer)
        {
            var existing = await _db.Answers.FindAsync(answer.Id);
            if (existing == null)
                return false;

            existing.Answer = answer.Answer;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAnswerAsync(int id)
        {
            var existing = await _db.Answers.FindAsync(id);
            if (existing == null)
                return false;

            _db.Answers.Remove(existing);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
