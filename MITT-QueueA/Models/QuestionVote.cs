using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MITT_QueueA.Models
{
    public class QuestionVote
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual Question Question { get; set; }
        public bool IsUpvote { get; set; }

    }
}