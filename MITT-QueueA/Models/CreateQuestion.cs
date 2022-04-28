using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MITT_QueueA.Models
{
    public class CreateQuestion
    {
        [Required]
        [StringLength(120, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 10)]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Description")]
        [StringLength(2000, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 10)]
        public string Content { get; set; }

        public ICollection<string> Tags { get; set; }
        public CreateQuestion()
        {
            Tags = new HashSet<string>();
        }
    }
}