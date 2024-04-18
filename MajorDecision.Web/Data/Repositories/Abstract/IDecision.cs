using MajorDecision.Web.Models;

namespace MajorDecision.Web.Data.Repositories.Abstract
{
    public interface IDecision
    {
        public Task <Decision> ShowAnswerByFirstMethodAsync(Decision decision);
        public Task <Decision> ShowAnswerBySecondMethodAsync(Decision decision);
    }
}
