namespace MajorDecision.Web.Models
{
    public class NotificationVM
    {
        public int Id { get; set; }
        public string? Message { get; set; }
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        public string? ReceiverName { get; set; }
        public string Type { get; set; }
        public bool IsRead { get; set; }
    }
}
