using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajorDecision.Data.Dto
{
    public class EditProfileRequest
    {
        public string? FirstName { get; set; }
        public string? Username { get; set; }
        [EmailAddress]
        public string? Email { get; set; }
    }
}
