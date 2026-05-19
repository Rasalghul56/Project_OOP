namespace Confectionery.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public virtual Order Order { get; set; }

        /// <summary>Ссылка на товар; null если товар удалён из каталога.</summary>
        public int? ProductId { get; set; }
        public virtual Product Product { get; set; }

        /// <summary>Название на момент заказа — остаётся в истории после удаления товара.</summary>
        public string ProductName { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }
    }
}
