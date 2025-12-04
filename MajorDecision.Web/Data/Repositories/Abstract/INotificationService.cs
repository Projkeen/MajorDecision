using MajorDecision.Web.Models;

namespace MajorDecision.Web.Data.Repositories.Abstract
{
    public interface INotificationService
    {
        Task<List<Notification>> GetUnreadNotificationsForUserAsync(string userId);
    }
}
