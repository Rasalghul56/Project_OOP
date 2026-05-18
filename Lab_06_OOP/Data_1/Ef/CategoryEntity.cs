using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConfectioneryShop.Data.Ef
{
    [Table("Category")]
    public class CategoryEntity
    {
        [Key]
        [Column("CategoryId")]
        public int CategoryId { get; set; }

        [Required]
        [MaxLength(120)]
        public string Name { get; set; }

        public System.DateTime? LastUpdated { get; set; }

        public virtual ICollection<ProductEntity> Products { get; set; } = new List<ProductEntity>();
    }
}
