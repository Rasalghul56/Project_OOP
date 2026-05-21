using System;
using System.ComponentModel;
using System.Linq;
using System.Data;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ConfectioneryShop.Data;
using ConfectioneryShop.Data.Ef;
using Lab_06_OOP.Commands;

namespace Lab_06_OOP.ViewModels
{


    public class DbAdminViewModel : INotifyPropertyChanged
    {
        private readonly Action _onClosedRefresh;
        private int _activeTable;
        private DataView _categories;
        private DataView _manufacturers;
        private DataView _products;
        private DataView _queryResult;
        private string _statsText = "—";
        private string _categoriesSummary = "—";
        private string _minPriceFilter = "0";
        private string _categoryPart = "";
        private string _efLabResult = "—";
        private string _efSearchWords = "";

        public DbAdminViewModel(Action onClosedRefresh)
        {
            _onClosedRefresh = onClosedRefresh;
            RefreshCommand = new RelayCommand(_ => ReloadAll());
            DeleteCategoryCommand = new RelayCommand(_ => DeleteCategory(), _ => SelectedCategoryRow != null);
            AddCategoryCommand = new RelayCommand(_ => AddCategory());
            DeleteManufacturerCommand = new RelayCommand(_ => DeleteManufacturer(), _ => SelectedManufacturerRow != null);
            AddManufacturerCommand = new RelayCommand(_ => AddManufacturer());
            RunParamQueryCommand = new RelayCommand(_ => RunParamQuery());
            RunSpFilterCommand = new RelayCommand(_ => RunSpFilter());
            LoadStatsAsyncCommand = new RelayCommand(_ => { _ = LoadStatsAsync(); });
            LoadCategoriesSummaryAsyncCommand = new RelayCommand(_ => { _ = LoadCategoriesSummaryAsync(); });
            EfLoadProductsAsyncCommand = new RelayCommand(_ => { _ = EfLoadProductsAsync(); });
            EfLinqSearchCommand = new RelayCommand(_ => EfRunLinqSearch());
            EfSortByPriceCommand = new RelayCommand(_ => EfSortByPrice());
            EfTransactionDemoCommand = new RelayCommand(_ => EfTransactionDemo());
            EfCrudTagDemoCommand = new RelayCommand(_ => EfCrudTagDemo());
            EfInStockCountAsyncCommand = new RelayCommand(_ => { _ = EfInStockCountAsync(); });
            ReloadAll();
            _ = LoadCategoriesSummaryAsync();
        }

        public int ActiveTable
        {
            get => _activeTable;
            set
            {
                _activeTable = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentGrid));
            }
        }

        public DataView CurrentGrid =>
            ActiveTable == 0 ? Categories : ActiveTable == 1 ? Manufacturers : Products;

        public DataView Categories
        {
            get => _categories;
            private set { _categories = value; OnPropertyChanged(); }
        }

        public DataView Manufacturers
        {
            get => _manufacturers;
            private set { _manufacturers = value; OnPropertyChanged(); }
        }

        public DataView Products
        {
            get => _products;
            private set { _products = value; OnPropertyChanged(); }
        }

        public DataView QueryResult
        {
            get => _queryResult;
            private set { _queryResult = value; OnPropertyChanged(); }
        }

        public string StatsText
        {
            get => _statsText;
            private set { _statsText = value; OnPropertyChanged(); }
        }

        public string CategoriesSummary
        {
            get => _categoriesSummary;
            private set { _categoriesSummary = value; OnPropertyChanged(); }
        }

        public string MinPriceFilter
        {
            get => _minPriceFilter;
            set { _minPriceFilter = value; OnPropertyChanged(); }
        }

        public string CategoryPart
        {
            get => _categoryPart;
            set { _categoryPart = value; OnPropertyChanged(); }
        }

        public string EfLabResult
        {
            get => _efLabResult;
            private set { _efLabResult = value; OnPropertyChanged(); }
        }

        public string EfSearchWords
        {
            get => _efSearchWords;
            set { _efSearchWords = value; OnPropertyChanged(); }
        }

        public DataRowView SelectedCategoryRow { get; set; }
        public DataRowView SelectedManufacturerRow { get; set; }

        public ICommand RefreshCommand { get; }
        public ICommand DeleteCategoryCommand { get; }
        public ICommand AddCategoryCommand { get; }
        public ICommand DeleteManufacturerCommand { get; }
        public ICommand AddManufacturerCommand { get; }
        public ICommand RunParamQueryCommand { get; }
        public ICommand RunSpFilterCommand { get; }
        public ICommand LoadStatsAsyncCommand { get; }
        public ICommand LoadCategoriesSummaryAsyncCommand { get; }
        public ICommand EfLoadProductsAsyncCommand { get; }
        public ICommand EfLinqSearchCommand { get; }
        public ICommand EfSortByPriceCommand { get; }
        public ICommand EfTransactionDemoCommand { get; }
        public ICommand EfCrudTagDemoCommand { get; }
        public ICommand EfInStockCountAsyncCommand { get; }

        public void NotifySelectionCommands()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        public void OnWindowClosed()
        {
            _onClosedRefresh?.Invoke();
        }

        private void ReloadAll()
        {
            Categories = ConfectioneryRepository.LoadCategoriesTable().DefaultView;
            Manufacturers = ConfectioneryRepository.LoadManufacturersTable().DefaultView;
            Products = ConfectioneryRepository.LoadProductsTable().DefaultView;
            OnPropertyChanged(nameof(CurrentGrid));
        }

        private void DeleteCategory()
        {
            if (SelectedCategoryRow == null) return;
            try
            {
                var id = Convert.ToInt32(SelectedCategoryRow["CategoryId"]);
                ConfectioneryRepository.DeleteCategory(id);
                ReloadAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Нельзя удалить: возможно, на категорию ссылаются товары.\n" + ex.Message,
                    "База данных", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void AddCategory()
        {
            var name = Microsoft.VisualBasic.Interaction.InputBox("Название категории:", "Новая категория", "");
            if (string.IsNullOrWhiteSpace(name)) return;
            try
            {
                ConfectioneryRepository.InsertCategory(name.Trim());
                ReloadAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "База данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteManufacturer()
        {
            if (SelectedManufacturerRow == null) return;
            try
            {
                var id = Convert.ToInt32(SelectedManufacturerRow["ManufacturerId"]);
                ConfectioneryRepository.DeleteManufacturer(id);
                ReloadAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Нельзя удалить: возможно, есть товары этого производителя.\n" + ex.Message,
                    "База данных", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void AddManufacturer()
        {
            var name = Microsoft.VisualBasic.Interaction.InputBox("Название производителя:", "Производитель", "");
            if (string.IsNullOrWhiteSpace(name)) return;
            var country = Microsoft.VisualBasic.Interaction.InputBox("Страна (необязательно):", "Производитель",
