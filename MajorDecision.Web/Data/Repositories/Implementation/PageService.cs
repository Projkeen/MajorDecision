using MajorDecision.Web.Data.Repositories.Abstract;
using MajorDecision.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace MajorDecision.Web.Data.Repositories.Implementation
{
    public class PageService : IPageService
    {
        private ApplicationDbContext _db;

        public PageService(ApplicationDbContext db)
        {
            _db = db;
        }
                
        public async Task<DiscussionPage?> CreateFromDecisionAsync(int decisionId, ApplicationUser currentUser)
        {
            var decision = await _db.Decisions.FirstOrDefaultAsync(d => d.Id == decisionId);
            if (decision == null)
                return null;

            var page = new DiscussionPage
            {
                Question = decision.Question,
                Answer = decision.Answer,
                Image = currentUser.ProfilePicture,
                DateOfCreating = DateTime.Now,
                ApplicationUserId = currentUser.Id,
                ApplicationUser = currentUser
            };

            _db.DiscussionPages.Add(page);
            await _db.SaveChangesAsync();
            return page;
        }

        public async Task<(bool Success, string Message)> DeletePageAsync(int pageId, string userId, bool isAdmin)
        {
            var page = await _db.DiscussionPages.FirstOrDefaultAsync(p => p.Id == pageId);
            if (page == null)
                return (false, "Page not found");
            if (!isAdmin && page.ApplicationUserId != userId)
                return (false, "You can only delete your own pages");

            _db.DiscussionPages.Remove(page);
            await _db.SaveChangesAsync();
            return (true, $"Page <{page.Question}> has been deleted");
        }

        public async Task<bool> AddDescriptionAsync(int pageId, string description)
        {
            var page = await _db.DiscussionPages.FirstOrDefaultAsync(p => p.Id == pageId);
            if (page == null)
                return false;

            page.Description = description;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<DiscussionPage?> AddCommentAsync(string text, int discussionPageId, string userId)
        {
            var pageExists = await _db.DiscussionPages.AnyAsync(p => p.Id == discussionPageId);
            if (!pageExists)
                return null;

            _db.Comments.Add(new Comment
            {
                Text = text,
                DateOfComment = DateTime.Now,
                ApplicationUserId = userId,
                DiscussionPageId = discussionPageId
            });
            await _db.SaveChangesAsync();

            return await GetById(discussionPageId);
        }

        public async Task<(bool Success, string Message, int? DiscussionPageId)> DeleteCommentAsync(int commentId, string userId, bool isAdmin)
        {
            var comment = await _db.Comments.FirstOrDefaultAsync(c => c.Id == commentId);
            if (comment == null)
                return (false, "Comment not found", null);
            if (!isAdmin && comment.ApplicationUserId != userId)
                return (false, "You can only delete your own comments", null);

            var pageId = comment.DiscussionPageId;
            _db.Comments.Remove(comment);
            await _db.SaveChangesAsync();
            return (true, $"Comment <{comment.Text}> has been deleted", pageId);
        }

        public async Task<DiscussionPage> GetById(int? id)
        {
            var page = await _db.DiscussionPages.Include(l => l.ApplicationUser).Include(l => l.Comments).ThenInclude(l => l.ApplicationUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            return page;
        }

        public IQueryable<DiscussionPage> GetPages()
        {
            var pages = _db.DiscussionPages.Include(u => u.ApplicationUser);
            return pages;
        }

        public async Task SaveChanges()
        {
            await _db.SaveChangesAsync();
        }
    }
}
