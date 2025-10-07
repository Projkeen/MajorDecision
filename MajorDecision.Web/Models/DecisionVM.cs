namespace MajorDecision.Web.Models
{
    public class DecisionVM
    {
        public int Id { get; set; }
        public string Question { get; set; }
        public string? Answer { get; set; }
        public DateTime DateOfQuestion { get; set; }

        public string? ApplicationUserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}
