namespace ConfectioneryShop.Models
{


    public class Product
    {
        public int Id { get; set; }

        public int CategoryId { get; set; }

        public int ManufacturerId { get; set; }
        public string ShortName { get; set; }
        public string FullName { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }

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
