using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MajorDecision.Web.Models
{
    public class ApplicationUser : IdentityUser
    {          
        public string Name { get; set; }
        public string? ProfilePicture { get; set; }
    }
}