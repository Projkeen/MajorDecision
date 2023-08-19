namespace MajorDecision.Web.Models
{
    public class Decision
    {
        public int Id { get; set; }
        public string Question { get; set; }
        public string? Answer { get; set; }
        public DateTime DateOfQuestion { get; set; }
    }
}
