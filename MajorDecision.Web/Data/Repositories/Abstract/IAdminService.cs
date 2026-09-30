using MajorDecision.Web.Models.Entities;
using MajorDecision.Web.Models.ViewModels;

namespace MajorDecision.Web.Data.Repositories.Abstract
{
    public interface IAdminService
    {
        Task<List<UserViewModel>> GetAllUsersAsync(string currentUserId);
        Task<UserViewModel?> GetUserInfoAsync(string userId);
        Task<(bool Success, string Message)> ChangeUserRoleAsync(string userId, string toggledRole = "Admin", string defaultRole = "User");
        Task<int> ClearDecisionsWithoutApplicationUserIdAsync();
        Task<int> ClearAllProfilePicturesAsync(string webRootPath);
        Task<List<Answers>> GetAnswersAsync();
        Task AddAnswerAsync(Answers answer);
        Task<bool> EditAnswerAsync(Answers answer);
        Task<bool> DeleteAnswerAsync(int id);
    }
}
