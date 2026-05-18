using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Confectionery.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; }

        public string Description { get; set; }

        public string Composition { get; set; }

        [Required]
        public decimal Price { get; set; }

        public double Weight { get; set; }

        public string ImagePath { get; set; }

        public bool IsAvailable { get; set; } = true;

        public int? CategoryId { get; set; }
        public virtual Category Category { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
