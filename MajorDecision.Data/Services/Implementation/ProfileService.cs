using MajorDecision.Data;
using MajorDecision.Data.Dto;
using MajorDecision.Data.Services.Abstract;
using MajorDecision.Web.Models.Entities;
using MajorDecision.Web.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace MajorDecision.Data.Services.Implementation
{
    public class ProfileService : IProfileService
    {
        private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const long MaxImageBytes = 5 * 1024 * 1024;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;
        private readonly string _profileImagesPath;


        public ProfileService(UserManager<ApplicationUser> userManager, ApplicationDbContext db, IConfiguration configuration)
        {
            _userManager = userManager;
            _db = db;
            _profileImagesPath = configuration["Storage:ProfileImagesPath"];
        }

        public async Task<UserViewModel?> GetProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);

            return new UserViewModel
            {
                Id = user.Id,
                FirstName = user.Name,
                Username = user.UserName!,
                Email = user.Email,
                ProfilePictureUrl = user.ProfilePicture,
                Claims = claims.Select(c => c.Value).ToList(),
                Roles = roles
            };
        }

        public async Task<(bool Success, string Message)> UpdateProfileAsync(string userId, EditProfileRequest request)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return (false, "User not found");

            if (!string.IsNullOrWhiteSpace(request.FirstName))
                user.Name = request.FirstName;
            if (!string.IsNullOrWhiteSpace(request.Username))
                user.UserName = request.Username;
            if (!string.IsNullOrWhiteSpace(request.Email))
                user.Email = request.Email;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return (true, "User data was updated!");
            }
            else
            {
                return (false, string.Join("; ", result.Errors.Select(e => e.Description)));
            }
        }

        public async Task<(bool Success, string Message)> UploadOrDeleteImageAsync(string userId, IFormFile? photo /*, string webRootPath*/)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return (false, "User not found");

            var folder = Path.Combine(_profileImagesPath/*, "images", "profileImages"*/);
            Directory.CreateDirectory(folder);

            if (photo == null)
            {
                if (user.ProfilePicture == null) return (false, "No file");

                var existingPath = Path.Combine(folder, user.ProfilePicture);
                if (File.Exists(existingPath)) File.Delete(existingPath);
                user.ProfilePicture = null;
                await _userManager.UpdateAsync(user);
                return (true, "Photo removed");
            }

            var extension = Path.GetExtension(photo.FileName);
            if (!AllowedImageExtensions.Contains(extension))
                return (false, "Unsupported file type");
            if (photo.Length > MaxImageBytes)
                return (false, "File is too large");

            if (user.ProfilePicture != null)
            {
                var oldPath = Path.Combine(folder, user.ProfilePicture);
                if (File.Exists(oldPath)) File.Delete(oldPath);
            }

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(folder, fileName);
            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await photo.CopyToAsync(stream);
            }

            user.ProfilePicture = fileName;
            await _userManager.UpdateAsync(user);
            return (true, "Photo updated");
        }

        public async Task<bool> DeleteAccountAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            _db.Decisions.RemoveRange(_db.Decisions.Where(d => d.ApplicationUserId == userId));
            _db.DiscussionPages.RemoveRange(_db.DiscussionPages.Where(p => p.ApplicationUserId == userId));
            _db.Friends.RemoveRange(_db.Friends.Where(f => f.SenderId == userId || f.ReceiverId == userId));
            _db.Notifications.RemoveRange(_db.Notifications.Where(n => n.SenderId == userId || n.ReceiverId == userId));
            await _db.SaveChangesAsync();

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }
    }
}

