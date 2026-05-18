using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Confectionery.Helpers;
using Confectionery.Models;
using Confectionery.UnitOfWork;
using Confectionery.ViewModels.Base;

namespace Confectionery.ViewModels.Admin
{
    public class TopProductItem
    {
        public string Name  { get; set; }
        public int    Count { get; set; }
        public decimal Revenue { get; set; }
    }

    public class DashboardViewModel : BaseViewModel
    {
        private readonly IUnitOfWork _uow;

        private int     _totalOrders;
        private decimal _totalRevenue;
        private int     _ordersAccepted;
        private int     _ordersPreparing;
        private int     _ordersReady;
        private int     _ordersCompleted;
        private int     _totalProducts;
        private int     _totalClients;

        public int     TotalOrders     { get => _totalOrders;     private set => SetProperty(ref _totalOrders, value); }
        public decimal TotalRevenue    { get => _totalRevenue;    private set => SetProperty(ref _totalRevenue, value); }
        public int     OrdersAccepted  { get => _ordersAccepted;  private set => SetProperty(ref _ordersAccepted, value); }
        public int     OrdersPreparing { get => _ordersPreparing; private set => SetProperty(ref _ordersPreparing, value); }
        public int     OrdersReady     { get => _ordersReady;     private set => SetProperty(ref _ordersReady, value); }
        public int     OrdersCompleted { get => _ordersCompleted; private set => SetProperty(ref _ordersCompleted, value); }
        public int     TotalProducts   { get => _totalProducts;   private set => SetProperty(ref _totalProducts, value); }
        public int     TotalClients    { get => _totalClients;    private set => SetProperty(ref _totalClients, value); }

        public ObservableCollection<TopProductItem> TopProducts { get; }
            = new ObservableCollection<TopProductItem>();

        public ICommand RefreshCommand { get; }

        public DashboardViewModel(IUnitOfWork uow)
        {
            _uow = uow;
            RefreshCommand = new RelayCommand(p => LoadStats());
            LoadStats();
        }

        private void LoadStats()
        {
            var orders   = _uow.Orders.GetAllWithDetails().ToList();
            var products = _uow.Products.GetAll().ToList();
            var clients  = _uow.Users.GetAll().Where(u => u.Role == "Client").ToList();

            TotalOrders     = orders.Count;
            TotalRevenue    = orders.Sum(o => o.TotalPrice);
            OrdersAccepted  = orders.Count(o => o.Status == OrderStatus.Accepted);
            OrdersPreparing = orders.Count(o => o.Status == OrderStatus.Preparing);
            OrdersReady     = orders.Count(o => o.Status == OrderStatus.Ready);
            OrdersCompleted = orders.Count(o => o.Status == OrderStatus.Completed);
            TotalProducts   = products.Count;
            TotalClients    = clients.Count;

            // Топ 5 товаров по количеству заказов
            var topRaw = orders
                .SelectMany(o => o.OrderItems)
                .GroupBy(oi => oi.ProductId)
                .Select(g => new TopProductItem
                {
                    Name    = g.First().Product?.Name ?? "—",
                    Count   = g.Sum(oi => oi.Quantity),
                    Revenue = g.Sum(oi => oi.UnitPrice * oi.Quantity)
                })
                .OrderByDescending(x => x.Count)
                .Take(5);

            TopProducts.Clear();
            foreach (var t in topRaw) TopProducts.Add(t);
        }
    }
}
