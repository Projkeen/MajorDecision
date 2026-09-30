using MajorDecision.Web.Models.Entities;

namespace MajorDecision.Web.Data.Repositories.Abstract
{
    public interface IFriendshipService
    {
        Task UndergroundMethod(string senderId, IEnumerable<string> receiverIds);
        Task<bool> AcceptAsync(string currentUserId, string senderId);
        Task<bool> DeclineAsync(string currentUserId, string senderId);
        Task<List<ApplicationUser>> GetFriendsAsync(string userId);
    }
}
