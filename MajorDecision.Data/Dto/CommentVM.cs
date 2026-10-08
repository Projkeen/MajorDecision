using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajorDecision.Data.Dto
{
    public class CommentVM
    {
        public int Id { get; set; }
        public string Text { get; set; } = null!;
        public DateTime DateOfComment { get; set; }
        public string? ApplicationUserId { get; set; }
        public string Author { get; set; } = null!;
        public string? AuthorProfilePicture { get; set; }
    }
}
