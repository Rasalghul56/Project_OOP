using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConfectioneryShop.Data.Ef
{
    /// <summary>Метка товара (многие-ко-многим с Product через ProductTag).</summary>
    [Table("Tag")]
    public class TagEntity
    {
        [Key]
        [Column("TagId")]
        public int TagId { get; set; }

        [Required]
        [MaxLength(80)]
        public string Name { get; set; }

        public virtual ICollection<ProductEntity> Products { get; set; } = new List<ProductEntity>();
    }
}
