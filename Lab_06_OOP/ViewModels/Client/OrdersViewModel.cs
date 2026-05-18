using System.Collections.Generic;
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
    public class OrdersViewModel : BaseViewModel
    {
        private readonly IUnitOfWork _uow;
        private readonly CartViewModel _cart;

        private Order _selectedOrder;
        private string _statusMessage;

        public ObservableCollection<Order> Orders { get; } = new ObservableCollection<Order>();

        public Order SelectedOrder
        {
            get => _selectedOrder;
            set => SetProperty(ref _selectedOrder, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public ICommand RefreshCommand { get; }
        public ICommand RepeatOrderCommand { get; }

        public OrdersViewModel(IUnitOfWork uow, CartViewModel cart)
        {
            _uow  = uow;
            _cart = cart;

            RefreshCommand = new RelayCommand(p => LoadOrders());

            RepeatOrderCommand = new RelayCommand(p =>
            {
                if (!(p is Order order)) return;

                foreach (var item in order.OrderItems)
                {
                    var product = _uow.Products.GetById(item.ProductId);
                    if (product != null && product.IsAvailable)
                        _cart.AddProduct(product);
                }

                StatusMessage = $"Товары из заказа №{order.OrderNumber} добавлены в корзину.";
            }, p => p is Order o && o.Status == OrderStatus.Completed);

            LoadOrders();
        }

        public void LoadOrders()
        {
            StatusMessage = null;
            var user = SessionService.CurrentUser;
            if (user == null) return;

            var freshOrders = _uow.Orders.GetByUser(user.Id).ToList();

            // Mark which orders have a new status so the View can highlight them
            foreach (var o in freshOrders)
                o.IsStatusChanged = OrderNotificationService.HasChanged(o.Id, o.Status);

            // Record current statuses as seen
            OrderNotificationService.MarkSeen(freshOrders);

            Orders.Clear();
            foreach (var o in freshOrders)
                Orders.Add(o);
        }
    }
}
