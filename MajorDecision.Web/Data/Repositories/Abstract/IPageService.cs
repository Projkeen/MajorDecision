using MajorDecision.Web.Models;
using System.Reflection;

namespace MajorDecision.Web.Data.Repositories.Abstract
{
    public interface IPageService
    {
        IQueryable<DiscussionPage> GetPages();
        Task Create(DiscussionPage page);
        //Task Update(DiscussionPage page);
        //Task Delete(int id);
        Task<DiscussionPage> GetById(int? id);
        Task SaveChanges();
    }
}
