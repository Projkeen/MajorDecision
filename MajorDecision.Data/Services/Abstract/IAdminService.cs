using MajorDecision.Data.Dto;
using MajorDecision.Web.Models.Entities;
using MajorDecision.Web.Models.ViewModels;

namespace MajorDecision.Data.Services.Abstract
{
    public interface IAdminService
    {
        Task<List<UserViewModel>> GetAllUsersAsync(string currentUserId);
        Task<UserViewModel?> GetUserInfoAsync(string userId);
        Task<(bool Success, string Message)> ChangeUserRoleAsync(string userId, string toggledRole = "Admin", string defaultRole = "User");
        Task<int> ClearDecisionsWithoutApplicationUserIdAsync();
        Task<int> ClearAllProfilePicturesAsync(/*string webRootPath*/);
        Task<List<AnswerSentence>> GetAnswersAsync();
        Task AddAnswerAsync(CreateAnswerDto answerDto);
        Task<bool> EditAnswerAsync(int id, CreateAnswerDto answerDto);
        Task<bool> DeleteAnswerAsync(int id);
    }
}
