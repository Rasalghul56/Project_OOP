using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConfectioneryShop.Data.Ef
{
    [Table("Product")]
    public class ProductEntity
    {
        [Key]
        [Column("ProductId")]
        public int ProductId { get; set; }

        public int CategoryId { get; set; }

        public int ManufacturerId { get; set; }

        [Required]
        [MaxLength(120)]
        public string ShortName { get; set; }

        [MaxLength(300)]
        public string FullName { get; set; }

        public string Description { get; set; }

        [MaxLength(500)]
        public string ImagePath { get; set; }

        public byte[] Photo { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public double Rating { get; set; }

        public int Discount { get; set; }

        [MaxLength(80)]
        public string Color { get; set; }

        [MaxLength(80)]
        public string Size { get; set; }

        public int SoldCount { get; set; }

        public bool IsOutOfStock { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public virtual CategoryEntity Category { get; set; }

        [ForeignKey(nameof(ManufacturerId))]
        public virtual ManufacturerEntity Manufacturer { get; set; }

        public virtual ICollection<TagEntity> Tags { get; set; } = new List<TagEntity>();
    }
}
