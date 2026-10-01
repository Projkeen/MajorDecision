using MajorDecision.Web.Models;
using MajorDecision.Web.Models.Entities;
using MajorDecision.Web.Models.ViewModels;

namespace MajorDecision.Data.Services.Abstract
{
    public interface IDecisionService
    {
        Task <Decision> ShowAnswerAsync(Decision decision, string lucky, ApplicationUser currentUser);
        Task<PaginatedList<DecisionVM>> GetHistoryAsync(string userId, int pageNumber, string? searchString, int pageSize = 13);
        IQueryable<DecisionVM> GetAllAsync();
        Task<int> DeleteAsync(IEnumerable<int> decisionIds, string userId);
        Task DeleteAllAsync(string userId);
        Task<List<Decision>> GetForDownloadAsync(string userId);
        Task<List<string>> GetLastQuestionsAsync(int count = 20);        
              
    }
}
