using MajorDecision.Web.Models.ViewModels;
using Microsoft.AspNetCore.Http;

namespace MajorDecision.Data.Services.Abstract
{
    public interface IProfileService
    {
        Task<UserViewModel?> GetProfileAsync(string userId);
        Task<(bool Success, string Message)> UpdateProfileAsync(string userId, UserViewModel model);
        Task<(bool Success, string Message)> UploadOrDeleteImageAsync(string userId, IFormFile? photo, string webRootPath);
        Task<bool> DeleteAccountAsync(string userId);
    }
}
