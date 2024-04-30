using System.ComponentModel.DataAnnotations;

namespace MajorDecision.Web.Models.Authentication
{
    public class Registration
    {
        [Required]
        public string FirstName { get; set; }
        //public string? LastName { get; set; }
        [EmailAddress]
        public string? Email { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        [Required, Compare("Password")]
        public string PasswordConfirm { get; set; }
        public string? Role { get; set; }
        public string? SecretPassword { get; set; }
    }
}
