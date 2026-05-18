using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Confectionery.Models
{
    public enum OrderStatus
    {
        Accepted = 0,
        Preparing = 1,
        Ready = 2,
        Completed = 3
    }

    public enum DeliveryType
    {
        Pickup = 0,
        Delivery = 1
    }

    public enum PaymentMethod
    {
        Cash = 0,
        Card = 1
    }

    public class Order
    {
        public int Id { get; set; }

        /// <summary>Случайный 6-значный номер для отображения клиенту.</summary>
        public int OrderNumber { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public OrderStatus Status { get; set; } = OrderStatus.Accepted;

        public DeliveryType DeliveryType { get; set; }

        // Для самовывоза
        [MaxLength(300)]
        public string PickupLocation { get; set; }

        // Для доставки
        [MaxLength(500)]
        public string DeliveryAddress { get; set; }

        [MaxLength(20)]
        public string DeliveryPhone { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        [MaxLength(500)]
        public string Comment { get; set; }

        public decimal TotalPrice { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
