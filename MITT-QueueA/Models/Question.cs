using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MITT_QueueA.Models
{
    public class Question
    {
        public int Id { get; set; }
        [Required]
        [StringLength(150, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 10)]
        public string Title { get; set; }
        public DateTime DateAsked { get; set; }
        public string UserId { get; set; }

        public virtual ICollection<Answer> Answers { get; set; }
        public virtual ICollection<Tag> Tags { get; set; }
        public virtual ApplicationUser User { get; set; }

        public Question()
        {
            Answers = new HashSet<Answer>();
            Tags = new HashSet<Tag>();
        }
    }
}