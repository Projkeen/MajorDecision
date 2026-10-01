using MajorDecision.Web.Models.Entities;

namespace MajorDecision.Web.Models
{
    public class Notification
    {
        public int Id { get; set; }
        //public int? FriendshipRequestId { get; set; }
        public string? SenderId { get; set; }
        public string? ReceiverId { get; set; }
        public ApplicationUser? Receiver { get; set; }
        //public string UserId {  get; set; }
        //public ApplicationUser User { get; set; }
        public string? Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public string? Type { get; set; }    
    }
}
