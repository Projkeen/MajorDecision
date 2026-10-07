using MajorDecision.Data;
using MajorDecision.Data.Services.Abstract;
using MajorDecision.Web.Data;
using MajorDecision.Web.Models;
using MajorDecision.Web.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static MajorDecision.Web.Data.AppUserFriendship;

namespace MajorDecision.Data.Services.Implementation
{
    public class FriendshipService : IFriendshipService
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public FriendshipService(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task UndergroundMethod(string senderId, IEnumerable<string> receiverIds)
        {
            var sender = await _userManager.FindByIdAsync(senderId);
            if (sender == null)
                return;

            var receiverIdList = receiverIds.Distinct().Where(id => id != senderId).ToList();
            if (receiverIdList.Count == 0)
                return;

            //var existing = await _db.Friends.Where(fr => (fr.SenderId == senderId && receiverIdList.Contains(fr.ReceiverId))
            //              || (receiverIdList.Contains(fr.SenderId) && fr.ReceiverId == senderId && fr.Statuses != Status.Rejected))
            //                .Select(fr => fr.SenderId == senderId ? fr.ReceiverId : fr.SenderId).ToListAsync();

            var relations = await _db.Friends.Where(fr => fr.SenderId == senderId && receiverIdList.Contains(fr.ReceiverId) || receiverIdList.Contains(fr.SenderId)
                            && fr.ReceiverId == senderId).ToListAsync();
            var existing = relations.Where(fr => fr.Statuses != Status.Rejected).Select(fr => fr.SenderId == senderId ? fr.ReceiverId : fr.SenderId).ToHashSet();
            var rejectedMine = relations.Where(fr => fr.SenderId == senderId && fr.Statuses == Status.Rejected && !existing.Contains(fr.ReceiverId)).ToList();
            foreach (var fr in rejectedMine)
            {
                fr.Statuses = Status.Pending;
                fr.RequestDate = DateTime.Now;
            }
            var newReceiverIds = receiverIdList.Except(existing).Except(rejectedMine.Select(fr => fr.ReceiverId)).ToList();

            if (newReceiverIds.Count == 0 && rejectedMine.Count == 0)
                return;

            //var newReceiverIds = receiverIdList.Except(existing).ToList();
            //if (newReceiverIds.Count == 0)
            //    return;
            var requests = newReceiverIds.Select(receiverId => new AppUserFriendship
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Statuses = Status.Pending,
                RequestDate = DateTime.Now
            }).ToList();

            var notifyIds = newReceiverIds.Concat(rejectedMine.Select(fr => fr.ReceiverId)).ToList();
            var notifications = notifyIds.Select(receiverId => new Notification
            {
                ReceiverId = receiverId,
                SenderId = senderId,
                Message = $"User {sender.Name} sent a friend request",
                CreatedAt = DateTime.Now,
                IsRead = false,
                Type = "Action"
            }).ToList();

            //var requests = new List<AppUserFriendship>();
            //var notifications = new List<Notification>();

            //foreach (var receiverId in newReceiverIds)
            //{
            //    requests.Add(new AppUserFriendship
            //    {
            //        SenderId = senderId,
            //        ReceiverId = receiverId,
            //        Statuses = Status.Pending,
            //        RequestDate = DateTime.Now
            //    });
            //    notifications.Add(new Notification
            //    {
            //        ReceiverId = receiverId,
            //        SenderId = senderId,
            //        Message = $"User {sender.Name} sent a friend request",
            //        CreatedAt = DateTime.Now,
            //        IsRead = false,
            //        Type = "Action"
            //    });
            //}

            _db.Friends.AddRange(requests);
            _db.Notifications.AddRange(notifications);
            await _db.SaveChangesAsync();
        }

        public async Task<(bool Success, string Message)> AcceptAsync(string currentUserId, string senderId)
        {
            var friendship = await _db.Friends.Include(f => f.Sender)
                .FirstOrDefaultAsync(f => f.SenderId == senderId && f.ReceiverId == currentUserId && f.Statuses == Status.Pending);
            if (friendship == null)
                return new(false, "No request");

            friendship.Statuses = Status.Accepted;
            friendship.BecameFriendsDate = DateTime.Now;            

            var receiver = await _userManager.FindByIdAsync(currentUserId);
            _db.Notifications.Add(new Notification
            {
                ReceiverId = senderId,
                SenderId = currentUserId,                
                Message = $"User {receiver!.UserName} accepted your friend request",
                CreatedAt = DateTime.Now,
                IsRead = false,
                Type = "Simple"
            });

            var requestNotification = await _db.Notifications.FirstOrDefaultAsync(n => n.ReceiverId == currentUserId && n.SenderId == senderId);
            if (requestNotification != null)
            {
                _db.Notifications.Remove(requestNotification);
            }

            await _db.SaveChangesAsync();
            return new(true, friendship.Sender.UserName);
        }

        public async Task<(bool Success, string Message)> DeclineAsync(string currentUserId, string senderId)
        {
            var friendship = await _db.Friends.Include(f => f.Sender)
                .FirstOrDefaultAsync(f => f.SenderId == senderId && f.ReceiverId == currentUserId && f.Statuses == Status.Pending);
            if (friendship == null)
                return new(false, "No request");

            friendship.Statuses = Status.Rejected;

            var requestNotification = await _db.Notifications.FirstOrDefaultAsync(n => n.ReceiverId == currentUserId && n.SenderId == senderId);
            if (requestNotification != null)
                _db.Notifications.Remove(requestNotification);

            await _db.SaveChangesAsync();
            return new(true, friendship.Sender.UserName);
        }

        public async Task<List<ApplicationUser>> GetFriendsAsync(string userId)
        {
            return await _db.Friends.Where(f => f.Statuses == Status.Accepted && (f.SenderId == userId || f.ReceiverId == userId))
                .Select(f => f.SenderId == userId ? f.Receiver : f.Sender).ToListAsync();
        }
    }
}

