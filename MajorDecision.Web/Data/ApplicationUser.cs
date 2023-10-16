using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Net.Sockets;

namespace MajorDecision.Web.Models
{
    public class ApplicationUser : IdentityUser
    {          
        public string Name { get; set; }
        public string? ProfilePicture { get; set; }
        public virtual ICollection<Decision> Decisions { get; set; }
    }
}

//public virtual ICollection<Decision> Decisions { get; set; }