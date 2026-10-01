using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
namespace MajorDecision.Web.Models.ViewModels
{
    public class UserViewModel
    {
        public UserViewModel()
        {
            Claims = new List<string>();
            Roles = new List<string>();
        }
        public string Id { get; set; }
        public string FirstName { get; set; }
        //public string? LastName { get; set; }
        [EmailAddress]
        public string? Email { get; set; }
        [Required]
        public string Username { get; set; }
        public string ProfilePictureUrl { get; set; }
        [Required]
        public IFormFile? Photo { get; set; }
        public List<string> Claims { get; set; }
        public IList<string> Roles { get; set; }


    }
}
