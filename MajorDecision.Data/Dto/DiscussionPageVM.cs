using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajorDecision.Data.Dto
{
    public class DiscussionPageVM
    {
        public int Id { get; set; }
        public string Question { get; set; } = null!;
        public string? Answer { get; set; }
        public string? Description { get; set; } = null;
        public string? Image { get; set; }
        public DateTime DateOfCreating { get; set; }
        public string? ApplicationUserId { get; set; }
        public string Author { get; set; } = null!;
        public string? AuthorProfilePicture { get; set; }
        public List<CommentVM> Comments { get; set; } = new();
    }
}
