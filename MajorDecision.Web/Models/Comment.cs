using Microsoft.Extensions.Hosting;

namespace MajorDecision.Web.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime DateOfComment { get; set; }
        public string? ApplicationUserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
        public int DiscussionPageId { get; set; }
        public DiscussionPage? DiscussionPages { get; set; }
    }
}
