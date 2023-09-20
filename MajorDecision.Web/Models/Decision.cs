using MajorDecision.Web.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MajorDecision.Web.Models
{
    public class Decision
    {
        [Key]
        public int Id { get; set; }
        public string Question { get; set; }
        public string? Answer { get; set; }
        public DateTime DateOfQuestion { get; set; }
      

    }
}

//public int ApplicationUserId { get; set; }
//[ForeignKey("ApplicationUserId")]
//public ApplicationUser ApplicationUser { get; set; }
