namespace MajorDecision.Web.Models
{
    public class DiscussionPage
    {
        public int Id { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public string? Description { get; set; }
        public DateTime DateOfCreating { get; set; }
        public string? ApplicationUserId { get; set; }
        public  string? Image { get; set; }
        //public IFormFile? UserPhoto { get; set; }
        public virtual ApplicationUser? ApplicationUser { get; set; }
        public List<Comment>? Comments { get; set; }
    }
}
