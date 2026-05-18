using System;
using System.ComponentModel.DataAnnotations;

namespace Confectionery.Models
{
    public class Review
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }

        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        [Required]
        public string Text { get; set; }

        public int Rating { get; set; } 

        public string AdminReply { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
