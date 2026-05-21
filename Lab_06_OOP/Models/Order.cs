using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

    public class Order : INotifyPropertyChanged
    {
        public int Id { get; set; }


        public int OrderNumber { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public OrderStatus Status { get; set; } = OrderStatus.Accepted;

        public DeliveryType DeliveryType { get; set; }


        [MaxLength(300)]
        public string PickupLocation { get; set; }


        [MaxLength(500)]
        public string DeliveryAddress { get; set; }

        [MaxLength(20)]
        public string DeliveryPhone { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        [MaxLength(500)]
        public string Comment { get; set; }

        public decimal TotalPrice { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();


        public bool HasStatusNotification { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;

        private bool _isStatusChanged;

        [NotMapped]
        public bool IsStatusChanged
        {
            get => _isStatusChanged;
            set
            {
                if (_isStatusChanged == value) return;
                _isStatusChanged = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsStatusChanged)));
            }
        }
    }
}
