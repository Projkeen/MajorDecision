using MajorDecision.Data;
using MajorDecision.Data.Services.Abstract;
using MajorDecision.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace MajorDecision.Data.Services.Implementation
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _db;

        public NotificationService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<NotificationVM>> GetAndCheckReadAsync(string userId)
        {
            var notifications = await _db.Notifications.Where(n => n.ReceiverId == userId).OrderByDescending(n => n.CreatedAt).ToListAsync();

            foreach (var n in notifications) n.IsRead = true;
            await _db.SaveChangesAsync();

            return notifications.Select(n => new NotificationVM
            {
                Id = n.Id,
                Message = n.Message,
                SenderId = n.SenderId,
                ReceiverId = n.ReceiverId,
                ReceiverName = n.Receiver.UserName,
                Type = n.Type,
                IsRead = true
            }).ToList();
        }

        public async Task<bool> DeleteAsync(int notificationId, string userId)
        {
            var notification = await _db.Notifications.FirstOrDefaultAsync(n => n.Id == notificationId && n.ReceiverId == userId);
            if (notification == null)
                return false;

            _db.Notifications.Remove(notification);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
