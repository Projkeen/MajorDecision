using MajorDecision.Web.Data.Repositories.Abstract;
using MajorDecision.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace MajorDecision.Web.Data.Repositories.Implementation
{
    public class NotificationService: INotificationService
    {
        private readonly ApplicationDbContext _db;

        public NotificationService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<Notification>> GetUnreadNotificationsForUserAsync(string userId)
        {
            //return await _db.Notifications.Include(n => n.User).Where(n => n.UserId == userId && !n.IsRead).OrderByDescending(n => n.CreatedAt).ToListAsync();
            return await _db.Notifications.Include(n => n.Receiver).Where(n => n.ReceiverId == userId).OrderByDescending(n => n.CreatedAt).ToListAsync();
        }
    }
}
