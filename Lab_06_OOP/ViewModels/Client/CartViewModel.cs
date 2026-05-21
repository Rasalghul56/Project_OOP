using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Confectionery.Helpers;
using Confectionery.Models;
using Confectionery.Services;
using Confectionery.UnitOfWork;
using Confectionery.ViewModels.Base;

namespace Confectionery.ViewModels.Client
{
    public class CartViewModel : BaseViewModel
    {
        private static readonly Random Rng = new Random();

        private readonly IUnitOfWork _uow;


        private DeliveryType _deliveryType = DeliveryType.Pickup;
        private string _selectedPickupLocation;
        private string _deliveryAddress;
        private string _deliveryPhone;
        private PaymentMethod _paymentMethod = PaymentMethod.Cash;
        private string _comment;
        private string _statusMessage;


        public string[] PickupLocations { get; } = { "г. Минск, ул. Бобруйская, 25" };

        public ObservableCollection<CartItemViewModel> Items { get; }
            = new ObservableCollection<CartItemViewModel>();

        public DeliveryType DeliveryType
        {
            get => _deliveryType;
            set
            {
                SetProperty(ref _deliveryType, value);
                OnPropertyChanged(nameof(IsPickup));
                OnPropertyChanged(nameof(IsDelivery));
            }
        }

        public bool IsPickup
        {
            get => _deliveryType == DeliveryType.Pickup;
            set { if (value) DeliveryType = DeliveryType.Pickup; }
        }

        public bool IsDelivery
        {
            get => _deliveryType == DeliveryType.Delivery;
            set { if (value) DeliveryType = DeliveryType.Delivery; }
        }

        public string SelectedPickupLocation
        {
            get => _selectedPickupLocation;
            set => SetProperty(ref _selectedPickupLocation, value);
        }

        public string DeliveryAddress
        {
            get => _deliveryAddress;
            set => SetProperty(ref _deliveryAddress, value);
        }

        public string DeliveryPhone
        {
            get => _deliveryPhone;
            set => SetProperty(ref _deliveryPhone, value);
        }

        public PaymentMethod PaymentMethod
        {
            get => _paymentMethod;
            set => SetProperty(ref _paymentMethod, value);
        }

        public bool PaymentCash
        {
            get => _paymentMethod == PaymentMethod.Cash;
            set { if (value) PaymentMethod = PaymentMethod.Cash; }
        }

        public bool PaymentCard
        {
            get => _paymentMethod == PaymentMethod.Card;
            set { if (value) PaymentMethod = PaymentMethod.Card; }
        }

        public string Comment
        {
            get => _comment;
            set => SetProperty(ref _comment, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        private string _validationError;
        public string ValidationError
        {
            get => _validationError;
            set => SetProperty(ref _validationError, value);
        }

        public decimal Total     => Items.Sum(i => i.Subtotal);
        public int    ItemCount  => Items.Count;
        public bool   IsEmpty    => !Items.Any();


        public bool IsCashPayment
        {
            get => _paymentMethod == PaymentMethod.Cash;
            set { if (value) PaymentMethod = PaymentMethod.Cash; }
        }

        public bool IsCardPayment
        {
            get => _paymentMethod == PaymentMethod.Card;
            set { if (value) PaymentMethod = PaymentMethod.Card; }
        }


        public event Action<int> ItemsChanged;

        public ICommand IncreaseCommand  { get; }
        public ICommand DecreaseCommand  { get; }
        public ICommand RemoveCommand    { get; }
        public ICommand ClearCartCommand { get; }
        public ICommand PlaceOrderCommand { get; }

        public CartViewModel(IUnitOfWork uow)
        {
            _uow = uow;
            _selectedPickupLocation = PickupLocations[0];

            IncreaseCommand = new RelayCommand(p =>
            {
                if (p is CartItemViewModel item) { item.Quantity++; RefreshTotal(); }
            });

            DecreaseCommand = new RelayCommand(p =>
            {
                if (p is CartItemViewModel item && item.Quantity > 1) { item.Quantity--; RefreshTotal(); }
            });

            RemoveCommand = new RelayCommand(p =>
            {
                if (p is CartItemViewModel item) { Items.Remove(item); RefreshTotal(); }
            });

            ClearCartCommand = new RelayCommand(p =>
            {
                Items.Clear();
                RefreshTotal();
            }, p => Items.Any());

            PlaceOrderCommand = new RelayCommand(ExecutePlaceOrder, p => Items.Any());
        }

        public void AddProduct(Product product)
        {
            if (!product.IsAvailable) return;

            var existing = Items.FirstOrDefault(i => i.Product.Id == product.Id);
            if (existing != null)
                existing.Quantity++;
            else
                Items.Add(new CartItemViewModel(product));
            RefreshTotal();
        }

        private void RefreshTotal()
        {
            OnPropertyChanged(nameof(Total));
            OnPropertyChanged(nameof(ItemCount));
            OnPropertyChanged(nameof(IsEmpty));
            ItemsChanged?.Invoke(ItemCount);
        }


        private bool ValidateForm()
        {
            if (IsDelivery)
            {
                if (string.IsNullOrWhiteSpace(DeliveryAddress))
                {
                    ValidationError = "Укажите адрес доставки.";
                    return false;
                }
                if (!System.Text.RegularExpressions.Regex.IsMatch(
                        DeliveryAddress, @"[а-яёА-ЯЁa-zA-Z]"))
                {
                    ValidationError = "Адрес должен содержать название улицы.";
                    return false;
                }
                if (string.IsNullOrWhiteSpace(DeliveryPhone))
                {
                    ValidationError = "Укажите номер телефона для доставки.";
                    return false;
                }
                if (!PhoneValidationHelper.IsValid(DeliveryPhone))
                {
                    ValidationError = PhoneValidationHelper.GetErrorMessage();
                    return false;
                }
            }
            ValidationError = null;
            return true;
        }

        private void ExecutePlaceOrder(object p)
        {
            if (!ValidateForm()) return;

            var user = SessionService.CurrentUser;
            if (user == null) return;


            int orderNumber = Rng.Next(100000, 999999);

            var order = new Order
            {
                OrderNumber    = orderNumber,
                UserId         = user.Id,
                DeliveryType   = DeliveryType,
                PickupLocation = IsPickup ? SelectedPickupLocation : null,
                DeliveryAddress = IsDelivery ? DeliveryAddress : null,
                DeliveryPhone  = IsDelivery ? DeliveryPhone : null,
                PaymentMethod  = PaymentMethod,
                Comment        = Comment,
                TotalPrice     = Total,
                Status         = OrderStatus.Accepted
            };

            foreach (var item in Items)
            {
                order.OrderItems.Add(new OrderItem
                {
                    ProductId   = item.Product.Id,
                    ProductName = item.Product.Name,
                    Quantity    = item.Quantity,
                    UnitPrice   = item.Product.Price
                });
            }

            _uow.Orders.Add(order);
            _uow.Save();

            ValidationError = null;
            Items.Clear();
            Comment       = null;
            DeliveryAddress = null;
            DeliveryPhone   = null;
            RefreshTotal();

            StatusMessage = $"Заказ №{orderNumber} оформлен! Ожидайте подтверждения.";
        }
    }
}
