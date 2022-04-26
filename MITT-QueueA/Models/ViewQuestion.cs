using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MITT_QueueA.Models
{
    public class ViewQuestion
    {
        public Question Question { get; set; }
        public Answer WorkingAnswer { get; set; }
        public Comment WorkingComment { get; set; }
    }
}