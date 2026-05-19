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
    public class OrdersViewModel : BaseViewModel
    {
        private readonly IUnitOfWork _uow;
        private readonly CartViewModel _cart;

        private Order _selectedOrder;
        private string _statusMessage;

        public ObservableCollection<Order> Orders { get; } = new ObservableCollection<Order>();

        /// <summary>Fired when the user reads a notification (sidebar badge must be refreshed).</summary>
        public event Action NotificationCountChanged;

        // ── Count of highlighted (unread) orders ─────────────────────────────
        public int  UnreadCount => Orders.Count(o => o.IsStatusChanged);
        public bool HasUnread   => UnreadCount > 0;

        public Order SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                SetProperty(ref _selectedOrder, value);

                // When the user clicks on a highlighted order → mark it as read immediately
                if (value != null && value.IsStatusChanged)
                {
                    value.IsStatusChanged = false;          // triggers DataTrigger in the View
                    _uow.ClearSingleOrderNotification(value.Id);  // SQL UPDATE in DB

                    OnPropertyChanged(nameof(UnreadCount));
                    OnPropertyChanged(nameof(HasUnread));
                    NotificationCountChanged?.Invoke();     // tell sidebar to refresh badge
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public ICommand RefreshCommand      { get; }
        public ICommand RepeatOrderCommand  { get; }

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

            // GetByUser uses AsNoTracking — always fresh data from DB.
            var freshOrders = _uow.Orders.GetByUser(user.Id).ToList();

            // Copy the DB notification flag into the transient, notifiable IsStatusChanged.
            // Notifications are cleared one by one as the user clicks each order.
            foreach (var o in freshOrders)
                o.IsStatusChanged = o.HasStatusNotification;

            Orders.Clear();
            foreach (var o in freshOrders)
                Orders.Add(o);

            OnPropertyChanged(nameof(UnreadCount));
            OnPropertyChanged(nameof(HasUnread));
        }
    }
}
