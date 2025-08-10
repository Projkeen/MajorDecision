using System.ComponentModel.DataAnnotations;

namespace MajorDecision.Web.Models.Authentication
{
    public class Login
    {
        [Required]
        public string UsernameOrEmail { get; set; }    
        [Required]
        public string Password { get; set; }
        [Display(Name ="Remember me?")]
        public bool RememberMe { get; set; }
    }
}
