using MajorDecision.Web.Models.Entities;
using System.Reflection;

namespace MajorDecision.Data.Services.Abstract
{
    public interface IPageService
    {
        IQueryable<DiscussionPage> GetPages();        
        Task<DiscussionPage?> CreateFromDecisionAsync(int decisionId, ApplicationUser currentUser);
        Task<(bool Success, string Message)> DeletePageAsync(int pageId, string userId, bool isAdmin);
        Task<bool> AddDescriptionAsync(int pageId, string description);
        Task<DiscussionPage?> AddCommentAsync(string text, int discussionPageId, string userId);
        Task<(bool Success, string Message, int? DiscussionPageId)> DeleteCommentAsync(int commentId, string userId, bool isAdmin);        
        Task<DiscussionPage> GetById(int? id);
        Task SaveChanges();
    }
}
