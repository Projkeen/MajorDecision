using MajorDecision.Web.Models;

namespace MajorDecision.Web.Data.Repositories.Abstract
{
    public interface INotificationService
    {
        //Task<List<Notification>> GetUnreadNotificationsForUserAsync(string userId);
        Task<List<NotificationVM>> GetAndCheckReadAsync(string userId);
        Task<bool> DeleteAsync(int notificationId, string userId);
    }
}
