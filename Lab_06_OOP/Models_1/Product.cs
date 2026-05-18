namespace ConfectioneryShop.Models
{
    /// <summary>
    /// Класс Product - описывает товар 
    /// </summary>
    public class Product
    {
        public int Id { get; set; }
        /// <summary>Связь с таблицей Category (лаб. 8).</summary>
        public int CategoryId { get; set; }
        /// <summary>Связь с таблицей Manufacturer (лаб. 8).</summary>
        public int ManufacturerId { get; set; }
        public string ShortName { get; set; }
        public string FullName { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }
        /// <summary>Фото из БД (VARBINARY), приоритет над ImagePath для отображения.</summary>
        public byte[] PhotoData { get; set; }
        public string Category { get; set; }
        public double Rating { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public string Country { get; set; }
        public int Discount { get; set; }
        public bool IsOutOfStock { get; set; }
        public int SoldCount { get; set; }
        public string Manufacturer { get; set; }
    }
}