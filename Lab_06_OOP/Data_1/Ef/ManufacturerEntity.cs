using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConfectioneryShop.Data.Ef
{
    [Table("Manufacturer")]
    public class ManufacturerEntity
    {
        [Key]
        [Column("ManufacturerId")]
        public int ManufacturerId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(120)]
        public string Country { get; set; }

        public byte[] LogoImage { get; set; }

        public virtual ICollection<ProductEntity> Products { get; set; } = new List<ProductEntity>();
    }
}
