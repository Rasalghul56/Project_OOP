using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Confectionery.Helpers;
using Confectionery.Models;
using Confectionery.UnitOfWork;
using Confectionery.ViewModels.Base;

namespace Confectionery.ViewModels.Admin
{
    public class OrderManagementViewModel : BaseViewModel
    {
        private readonly IUnitOfWork _uow;

        private Order _selectedOrder;
        private string _selectedStatusFilter;
        private DateTime? _dateFrom;
        private DateTime? _dateTo;
        private string _newStatus;

        public ObservableCollection<Order> Orders { get; } = new ObservableCollection<Order>();

        public ObservableCollection<string> StatusFilter { get; } = new ObservableCollection<string>
        {
            "Все", "Принят", "Готовится", "Готов", "Выполнен"
        };

        public ObservableCollection<string> AvailableStatuses { get; } = new ObservableCollection<string>
        {
            "Принят", "Готовится", "Готов", "Выполнен"
        };

        public Order SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                SetProperty(ref _selectedOrder, value);
                if (value != null) NewStatus = MapStatus(value.Status);
            }
        }

        public string SelectedStatusFilter
        {
            get => _selectedStatusFilter;
            set { SetProperty(ref _selectedStatusFilter, value); LoadOrders(); }
        }

        public DateTime? DateFrom
        {
            get => _dateFrom;
            set { SetProperty(ref _dateFrom, value); LoadOrders(); }
        }

        public DateTime? DateTo
        {
            get => _dateTo;
            set { SetProperty(ref _dateTo, value); LoadOrders(); }
        }

        public string NewStatus
        {
            get => _newStatus;
            set => SetProperty(ref _newStatus, value);
        }

        public ICommand RefreshCommand       { get; }
        public ICommand UpdateStatusCommand  { get; }
        public ICommand ResetFilterCommand   { get; }

        public OrderManagementViewModel(IUnitOfWork uow)
        {
            _uow = uow;

            RefreshCommand = new RelayCommand(p => LoadOrders());

            UpdateStatusCommand = new RelayCommand(ExecuteUpdateStatus,
                p => SelectedOrder != null && !string.IsNullOrWhiteSpace(NewStatus));

            ResetFilterCommand = new RelayCommand(p =>
            {
                SelectedStatusFilter = "Все";
                DateFrom = null;
                DateTo   = null;
            });

            SelectedStatusFilter = "Все";
            LoadOrders();
        }

        private void LoadOrders()
        {
            Orders.Clear();

            IEnumerable<Order> result;
            if (DateFrom.HasValue && DateTo.HasValue)
                result = _uow.Orders.GetByDateRange(DateFrom.Value, DateTo.Value.AddDays(1));
            else
                result = _uow.Orders.GetAllWithDetails();

            foreach (var o in result)
            {
                if (SelectedStatusFilter == "Все" || MapStatus(o.Status) == SelectedStatusFilter)
                    Orders.Add(o);
            }
        }

        private void ExecuteUpdateStatus(object p)
        {
            if (SelectedOrder == null) return;
            var status = ParseStatus(NewStatus);
            SelectedOrder.Status = status;
            _uow.Orders.Update(SelectedOrder);
            _uow.Save();
            LoadOrders();
        }

        private static string MapStatus(OrderStatus s)
        {
            switch (s)
            {
                case OrderStatus.Accepted:  return "Принят";
                case OrderStatus.Preparing: return "Готовится";
                case OrderStatus.Ready:     return "Готов";
                case OrderStatus.Completed: return "Выполнен";
                default: return s.ToString();
            }
        }

        private static OrderStatus ParseStatus(string s)
        {
            switch (s)
            {
                case "Готовится": return OrderStatus.Preparing;
                case "Готов":     return OrderStatus.Ready;
                case "Выполнен":  return OrderStatus.Completed;
                default:          return OrderStatus.Accepted;
            }
        }
    }
}
