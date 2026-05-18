using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Confectionery.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
