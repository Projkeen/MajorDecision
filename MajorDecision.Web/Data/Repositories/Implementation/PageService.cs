using MajorDecision.Web.Data.Repositories.Abstract;
using MajorDecision.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace MajorDecision.Web.Data.Repositories.Implementation
{
    public class PageService : IPageService
    {
        private ApplicationDbContext _context;

        public PageService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Create(DiscussionPage page)
        {
            _context.DiscussionPages.Add(page);
            await _context.SaveChangesAsync();
        }      

        public async Task<DiscussionPage> GetById(int? id)
        {
            var page = await _context.DiscussionPages.Include(l => l.ApplicationUser).Include(l => l.Comments)
                .ThenInclude(l => l.ApplicationUser).FirstOrDefaultAsync(m => m.Id == id);
            return page;
        }

        public IQueryable<DiscussionPage> GetPages()
        {
            var pages = _context.DiscussionPages.Include(u => u.ApplicationUser);
            return pages;
        }

        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }
    }
}
