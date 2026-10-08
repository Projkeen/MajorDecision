using MajorDecision.Data;
using MajorDecision.Data.Dto;
using MajorDecision.Data.Services.Abstract;
using MajorDecision.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;


namespace MajorDecision.Data.Services.Implementation
{
    public class PageService : IPageService
    {
        private ApplicationDbContext _db;

        public PageService(ApplicationDbContext db)
        {
            _db = db;
        }
                
        public async Task<DiscussionPageVM> CreateFromDecisionAsync(int decisionId, ApplicationUser currentUser)
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

            var pageVM = new DiscussionPageVM
            {
                Id = page.Id,
                Question = page.Question,
                Answer = page.Answer,                
                Image = currentUser.ProfilePicture,
                DateOfCreating = page.DateOfCreating,
                ApplicationUserId = page.ApplicationUserId,
                Author = page.ApplicationUser.UserName,
                AuthorProfilePicture = page.ApplicationUser.ProfilePicture,   
            };
            return pageVM;
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

        public async Task<(bool Success, string Message)> AddDescriptionAsync(int pageId, string description, string userId)
        {
            var page = await _db.DiscussionPages.FirstOrDefaultAsync(p => p.Id == pageId);
            if (page == null || page.ApplicationUserId != userId)
                return (false, "you can not do this");

            page.Description = description;
            await _db.SaveChangesAsync();
            return (true, "Description added!");
        }

        public async Task<(bool Success, string Message)> AddCommentAsync(string text, int discussionPageId, string userId)
        {
            //var pageExists = await _db.DiscussionPages.AnyAsync(p => p.Id == discussionPageId);
            //if (!pageExists)
            //    return null;
            var page = await _db.DiscussionPages.FirstOrDefaultAsync(p => p.Id == discussionPageId);
            if (page == null || page.ApplicationUserId == userId)
                return (false, "you can not comment this");

            _db.Comments.Add(new Comment
            {
                Text = text,
                DateOfComment = DateTime.Now,
                ApplicationUserId = userId,
                DiscussionPageId = discussionPageId
            });
            await _db.SaveChangesAsync();

            return(true, "Comment added!");
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
            return (true, $"Comment <{comment.Text}> has been deleted, page id = {pageId}", pageId);
        }

        public async Task<DiscussionPageVM> GetByIdAsync(int? id)
        {
            //var page = await _db.DiscussionPages.Include(l => l.ApplicationUser).Include(l => l.Comments).ThenInclude(l => l.ApplicationUser)
            //    .FirstOrDefaultAsync(m => m.Id == id);
            var findPage = _db.DiscussionPages.Where(p => p.Id == id);
            var page = findPage.Select(p => new DiscussionPageVM
            {
                Id = p.Id,
                Question = p.Question,
                Answer = p.Answer,
                Description = p.Description,
                DateOfCreating = p.DateOfCreating,
                ApplicationUserId = p.ApplicationUserId,
                Author = p.ApplicationUser.UserName,
                AuthorProfilePicture = p.ApplicationUser.ProfilePicture,
                Comments = p.Comments!
                .OrderBy(c => c.DateOfComment)
                .Select(c => new CommentVM
                {
                    Id = c.Id,
                    Text = c.Text,
                    DateOfComment = c.DateOfComment,
                    ApplicationUserId = c.ApplicationUserId,
                    Author = c.ApplicationUser.UserName,
                    AuthorProfilePicture = c.ApplicationUser.ProfilePicture
                }).ToList()
            });
            var pageVM = await page.FirstOrDefaultAsync();
            return pageVM;
        }

        public async Task<List<DiscussionPageVM>> GetPagesAsync()
        {
            var pages = await _db.DiscussionPages.Include(u => u.ApplicationUser).Select(p => new DiscussionPageVM
            {    
                Id = p.Id,
                Question = p.Question,
                Answer = p.Answer,
                Description = p.Description,
                Image = p.Image,
                DateOfCreating = p.DateOfCreating,
                ApplicationUserId = p.ApplicationUserId,
                Author = p.ApplicationUser!.UserName!,
                AuthorProfilePicture = p.ApplicationUser.ProfilePicture,
                Comments = p.Comments
                .OrderBy(c => c.DateOfComment)
                .Select(c => new CommentVM
                {
                    Id = c.Id,
                    Text = c.Text,
                    DateOfComment = c.DateOfComment,
                    ApplicationUserId = c.ApplicationUserId,
                    Author = c.ApplicationUser.UserName,
                    AuthorProfilePicture = c.ApplicationUser.ProfilePicture
                }).ToList(),
            }).ToListAsync();
            return pages;
        }

        public async Task SaveChanges()
        {
            await _db.SaveChangesAsync();
        }
    }
}
