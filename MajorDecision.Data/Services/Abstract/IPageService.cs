using MajorDecision.Data.Dto;
using MajorDecision.Web.Models.Entities;
using System.Reflection;

namespace MajorDecision.Data.Services.Abstract
{
    public interface IPageService
    {
        Task<List<DiscussionPageVM>> GetPagesAsync();        
        Task<DiscussionPageVM> CreateFromDecisionAsync(int decisionId, ApplicationUser currentUser);
        Task<(bool Success, string Message)> DeletePageAsync(int pageId, string userId, bool isAdmin);
        Task<(bool Success, string Message)> AddDescriptionAsync(int pageId, string description, string userId);
        Task<(bool Success, string Message)> AddCommentAsync(string text, int discussionPageId, string userId);
        Task<(bool Success, string Message, int? DiscussionPageId)> DeleteCommentAsync(int commentId, string userId, bool isAdmin);        
        Task<DiscussionPageVM> GetByIdAsync(int? id);
        Task SaveChanges();
    }
}
