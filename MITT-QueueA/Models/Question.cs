using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MITT_QueueA.Models
{
    public class Question
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime DateAsked { get; set; }
        public string UserId { get; set; }

        public virtual ICollection<Answer> Answers { get; set; }
        public virtual ICollection<Tag> Tags { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<QuestionVote> UserVotes { get; set; }

        public Question()
        {
            UserVotes = new List<QuestionVote>();
            Answers = new HashSet<Answer>();
            Tags = new HashSet<Tag>();
        }
    }
}