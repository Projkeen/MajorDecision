using MajorDecision.Web.Models;

namespace MajorDecision.Web.Data.Repositories.Abstract
{
    public interface IDecision
    {
        Task <Decision> ShowAnswerByFirstMethodAsync(Decision decision);
        Task <Decision> ShowAnswerBySecondMethodAsync(Decision decision);
        IQueryable<DecisionVM> GetAllAsync();
    }
}
