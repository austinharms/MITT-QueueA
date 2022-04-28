using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MITT_QueueA.Models
{
    public class Answer
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Answer")]
        [StringLength(2000, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 10)]
        public string Content { get; set; }
        public bool IsQuestion { get; set; }
        public bool AcceptedAnswer { get; set; }
        public DateTime DateAnswered { get; set; }
        public int QuestionId { get; set; }
        public string UserId { get; set; }

        public virtual Question Question { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<AnswerVote> UserVotes { get; set; }

        public Answer()
        {
            Comments = new HashSet<Comment>();
            UserVotes = new HashSet<AnswerVote>();
        }

        [NotMapped]
        public int Rating { get; set; }
        [NotMapped]
        public bool UserUpvote { get; set; } = false;
        [NotMapped]
        public bool UserDownvote { get; set; } = false;
    }
}