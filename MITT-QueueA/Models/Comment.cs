﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MITT_QueueA.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime DateAdded { get; set; }
        public string UserId { get; set; }
        public int AnswerId { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual Answer Answer { get; set; }
    }
}