using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Confectionery.Helpers;
using Confectionery.Models;
using Confectionery.Services;
using Confectionery.UnitOfWork;
using Confectionery.ViewModels.Base;

namespace Confectionery.ViewModels.Admin
{
    public class OrderManagementViewModel : BaseViewModel
    {
        private readonly IUnitOfWork _uow;

        private Order _selectedOrder;
        private int _filterIndex;   
        private int _statusIndex;   
        private DateTime? _dateFrom;
        private DateTime? _dateTo;

        public ObservableCollection<Order> Orders          { get; } = new ObservableCollection<Order>();
        public ObservableCollection<string> StatusFilter   { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> AvailableStatuses { get; } = new ObservableCollection<string>();

        public Order SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                SetProperty(ref _selectedOrder, value);
                if (value != null) _statusIndex = (int)value.Status; 
                OnPropertyChanged(nameof(SelectedStatusInDropdown));
            }
        }

        public string SelectedStatusFilter
        {
            get => StatusFilter.Count > _filterIndex ? StatusFilter[_filterIndex] : null;
            set
            {
                var idx = StatusFilter.IndexOf(value);
                if (idx < 0) return;
                _filterIndex = idx;
                OnPropertyChanged(nameof(SelectedStatusFilter));
                LoadOrders();
            }
        }

        public string SelectedStatusInDropdown
        {
            get => AvailableStatuses.Count > _statusIndex ? AvailableStatuses[_statusIndex] : null;
            set
            {
                var idx = AvailableStatuses.IndexOf(value);
                if (idx < 0) return;
                _statusIndex = idx;
                OnPropertyChanged(nameof(SelectedStatusInDropdown));
            }
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

        public ICommand RefreshCommand      { get; }
        public ICommand UpdateStatusCommand { get; }
        public ICommand ResetFilterCommand  { get; }

        public OrderManagementViewModel(IUnitOfWork uow)
        {
            _uow = uow;

            RefreshCommand = new RelayCommand(p => LoadOrders());

            UpdateStatusCommand = new RelayCommand(ExecuteUpdateStatus,
                p => SelectedOrder != null);

            ResetFilterCommand = new RelayCommand(p =>
            {
                _filterIndex = 0;
                OnPropertyChanged(nameof(SelectedStatusFilter));
                DateFrom = null;
                DateTo   = null;
            });

            LanguageService.LanguageChanged += RebuildLists;
            RebuildLists();
            LoadOrders();
        }

        private static string L(string key)
            => Application.Current.TryFindResource(key) as string ?? key;

        private void RebuildLists()
        {
            var prevFilter = _filterIndex;
            var prevStatus = _statusIndex;

            StatusFilter.Clear();
            StatusFilter.Add(L("Status_All"));
            StatusFilter.Add(L("Status_Accepted"));
            StatusFilter.Add(L("Status_Preparing"));
            StatusFilter.Add(L("Status_Ready"));
            StatusFilter.Add(L("Status_Completed"));

            AvailableStatuses.Clear();
            AvailableStatuses.Add(L("Status_Accepted"));
            AvailableStatuses.Add(L("Status_Preparing"));
            AvailableStatuses.Add(L("Status_Ready"));
            AvailableStatuses.Add(L("Status_Completed"));

            _filterIndex = prevFilter < StatusFilter.Count ? prevFilter : 0;
            _statusIndex = prevStatus < AvailableStatuses.Count ? prevStatus : 0;

            OnPropertyChanged(nameof(SelectedStatusFilter));
            OnPropertyChanged(nameof(SelectedStatusInDropdown));
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

                if (_filterIndex == 0 || (int)o.Status == _filterIndex - 1)
                    Orders.Add(o);
            }
        }

        private void ExecuteUpdateStatus(object p)
        {
            if (SelectedOrder == null) return;

            SelectedOrder.Status = (OrderStatus)_statusIndex;
            SelectedOrder.HasStatusNotification = true;
            _uow.Orders.Update(SelectedOrder);
            _uow.Save();
            LoadOrders();
        }
    }
}
