using System.ComponentModel.DataAnnotations;

namespace MajorDecision.Web.Models.Authentication
{
    public class Login
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
