using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MITT_QueueA.Models
{
    public class Comment
    {
        public int Id { get; set; }
        [Display(Name = "Comment")]
        [StringLength(500, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 10)]
        public string Message { get; set; }
        public DateTime DateAdded { get; set; }
        public string UserId { get; set; }
        public int AnswerId { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual Answer Answer { get; set; }

        [NotMapped]
        public int QuestionId { get; set; }
    }
}