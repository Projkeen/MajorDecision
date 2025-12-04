using MajorDecision.Web.Models.Entities;

namespace MajorDecision.Web.Data
{
    public class AppUserFriendship
    {
        //public int Id { get; set; }
        public string SenderId { get; set; }
        public ApplicationUser Sender { get; set; }
        public string ReceiverId { get; set; }
        public ApplicationUser Receiver { get; set; }
        //public string Status { get; set; } = "Pending";
        public Status Statuses { get; set; }
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
        public DateTime? BecameFriendsDate { get; set; }

        public enum Status
        {
            Pending,
            Accepted,
            Rejected,
            Blocked,
            Spam
        };
    }
}
