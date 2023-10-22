using System.ComponentModel.DataAnnotations;

namespace MajorDecision.Web.Models.Authentication
{
    public class ChangePassword
    {
        [Required]
        public string? CurrentPassword { get; set; }
        [Required]
        public string? NewPassword { get; set; }
        [Required]
        public string? PasswordConfirm { get; set; }
    }
}
