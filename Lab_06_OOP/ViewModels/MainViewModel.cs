using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Lab_06_OOP;
using ConfectioneryShop.Data;
using ConfectioneryShop.Models;
using ConfectioneryShop.Views;
using Lab_06_OOP.Commands;

namespace Lab_06_OOP.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly List<Product> _allProducts = new List<Product>();
        private readonly Stack<IHistoryAction> _undoStack = new Stack<IHistoryAction>();
        private readonly Stack<IHistoryAction> _redoStack = new Stack<IHistoryAction>();
        private string _searchText;
        private string _selectedCategory;
        private string _minPrice;
        private string _maxPrice;
        private string _filterCountry;
        private string _minRating;
        private string _maxRating;
        private bool _onlyInStock;
        private bool _useSimpleList;
        private int _roleIndex;
        private Product _selectedProduct;
        private int _displayedCount;
        private string _selectedTheme = "rose";
        private string _selectedLanguage = "ru";
        private UserProfile _userProfile;
        private string _productSortKey = "Name";

        public MainViewModel()
        {
            Products = new ObservableCollection<Product>();
            Categories = new ObservableCollection<string>();
            Themes = new ObservableCollection<string> { "rose", "gray" };
            Languages = new ObservableCollection<string> { "ru", "en" };
            _productSortKey = ConfigurationManager.AppSettings["DefaultProductSort"] ?? "Name";
            RefreshCommand = new RelayCommand(_ => ApplyFilter());
            AddProductCommand = new RelayCommand(_ => AddProduct(), _ => IsAdmin);
            EditProductCommand = new RelayCommand(_ => EditProduct(), _ => SelectedProduct != null && IsAdmin);
            DeleteProductCommand = new RelayCommand(_ => DeleteProduct(), _ => SelectedProduct != null && IsAdmin);
            OpenDetailCommand = new RelayCommand(_ => OpenProductDetail(), _ => SelectedProduct != null);
            OpenProfileCommand = new RelayCommand(_ => OpenProfile());
            UndoCommand = new RelayCommand(_ => Undo(), _ => CanUndo);
            RedoCommand = new RelayCommand(_ => Redo(), _ => CanRedo);
            try
            {
                DatabaseInitializer.EnsureDatabaseCreated();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось подключиться к базе или создать её.\n" + ex.Message +
                    "\n\nУстановите SQL Server Express LocalDB (часто входит в Visual Studio).",
                    "База данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }

            Load();
            UserProfile = new UserProfile
            {
                FullName = string.Empty,
                Email = string.Empty,
                Phone = string.Empty
            };
        }

        public ObservableCollection<Product> Products { get; }
        public ObservableCollection<string> Categories { get; }
        public ObservableCollection<string> Themes { get; }
        public ObservableCollection<string> Languages { get; }

        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); ApplyFilter(); }
        }

        public string SelectedCategory
        {
            get => _selectedCategory;
            set { _selectedCategory = value; OnPropertyChanged(); ApplyFilter(); }
        }

        public string MinPrice
        {
            get => _minPrice;
            set { _minPrice = value; OnPropertyChanged(); }
        }

        public string MaxPrice
        {
            get => _maxPrice;
            set { _maxPrice = value; OnPropertyChanged(); }
        }

        public string FilterCountry
        {
            get => _filterCountry;
            set { _filterCountry = value; OnPropertyChanged(); ApplyFilter(); }
        }

        public string MinRating
        {
            get => _minRating;
            set { _minRating = value; OnPropertyChanged(); ApplyFilter(); }
        }

        public string MaxRating
        {
            get => _maxRating;
            set { _maxRating = value; OnPropertyChanged(); ApplyFilter(); }
        }

        public bool OnlyInStock
        {
            get => _onlyInStock;
            set { _onlyInStock = value; OnPropertyChanged(); ApplyFilter(); }
        }

        public bool UseSimpleList
        {
            get => _useSimpleList;
            set { _useSimpleList = value; OnPropertyChanged(); }
        }

        public int RoleIndex
        {
            get => _roleIndex;
            set
            {
                _roleIndex = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsAdmin));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool IsAdmin => RoleIndex == 0;

        /// <summary>Сортировка списка: Name | Price | Rating (из конфигурации и UI).</summary>
        public string ProductSortKey
        {
            get => _productSortKey;
            set
            {
                _productSortKey = string.IsNullOrWhiteSpace(value) ? "Name" : value;
                OnPropertyChanged();
                ApplyFilter();
            }
        }

        public Product SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                _selectedProduct = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public int DisplayedCount
        {
            get => _displayedCount;
            private set { _displayedCount = value; OnPropertyChanged(); }
        }

        public UserProfile UserProfile
        {
            get => _userProfile;
            set { _userProfile = value; OnPropertyChanged(); }
        }

        public string SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                _selectedTheme = value;
                OnPropertyChanged();
                App.ApplyTheme(value);
            }
        }

        public string SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                _selectedLanguage = value;
                OnPropertyChanged();
                App.ApplyLanguage(value);
            }
        }

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;

        public ICommand RefreshCommand { get; }
        public ICommand AddProductCommand { get; }
        public ICommand EditProductCommand { get; }
        public ICommand DeleteProductCommand { get; }
        public ICommand OpenDetailCommand { get; }
        public ICommand OpenProfileCommand { get; }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OpenProductDetail()
        {
            if (SelectedProduct == null) return;
            var w = new ProductDetailWindow(SelectedProduct, this) { Owner = Application.Current.MainWindow };
            w.ShowDialog();
        }

        public bool OpenEditWindow(Product product)
        {
            if (!IsAdmin) return false;
            if (product == null) return false;

            var beforeEdit = CloneProduct(product);
            if (ShowEditor(product) != true) return false;
            PushAction(new EditProductAction(product, beforeEdit, CloneProduct(product)));
            LogAction($"EDIT: {product.ShortName}");
            ApplyFilter();
            ConfectioneryRepository.UpdateProduct(product);
            return true;
        }

        public void RemoveProduct(Product product)
        {
            if (!IsAdmin) return;
            if (product == null || !_allProducts.Contains(product)) return;
            if (SelectedProduct == product) SelectedProduct = null;
            ConfectioneryRepository.DeleteProduct(product.Id);
            _allProducts.Remove(product);
            PushAction(new RemoveProductAction(CloneProduct(product)));
            LogAction($"DELETE: {product.ShortName}");
            RebuildCategories();
            ApplyFilter();
        }

        public void ReloadFromDatabase()
        {
            try
            {
                DatabaseInitializer.EnsureDatabaseCreated();
                _allProducts.Clear();
                _allProducts.AddRange(ConfectioneryRepository.LoadProducts());
                RebuildCategories();
                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "База данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void PersistProductStock(Product product)
        {
            if (product == null || product.Id <= 0) return;
            try
            {
                ConfectioneryRepository.UpdateProductStock(product.Id, product.Quantity, product.IsOutOfStock);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "База данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Load()
        {
            _allProducts.Clear();
            try
            {
                _allProducts.AddRange(ConfectioneryRepository.LoadProducts());
            }
            catch
            {
                _allProducts.AddRange(DataService.LoadProducts());
            }

            RebuildCategories();
            ApplyFilter();
        }

        private void RebuildCategories()
        {
            Categories.Clear();
            foreach (var c in _allProducts.Select(p => p.Category).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().OrderBy(s => s))
                Categories.Add(c);
        }

        private void ApplyFilter()
        {
            IEnumerable<Product> q = _allProducts;
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var s = SearchText.Trim();
                q = q.Where(p =>
                    (p.ShortName != null && p.ShortName.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (p.FullName != null && p.FullName.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (p.Description != null && p.Description.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0));
            }
            if (!string.IsNullOrWhiteSpace(SelectedCategory))
                q = q.Where(p => p.Category == SelectedCategory);
            if (decimal.TryParse(MinPrice?.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var min))
                q = q.Where(p => p.Price >= min);
            if (decimal.TryParse(MaxPrice?.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var max))
                q = q.Where(p => p.Price <= max);
            if (!string.IsNullOrWhiteSpace(FilterCountry))
            {
                var c = FilterCountry.Trim();
                q = q.Where(p => p.Country != null && p.Country.IndexOf(c, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            if (double.TryParse(MinRating?.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var rmin))
                q = q.Where(p => p.Rating >= rmin);
            if (double.TryParse(MaxRating?.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var rmax))
                q = q.Where(p => p.Rating <= rmax);
            if (OnlyInStock)
                q = q.Where(p => !p.IsOutOfStock && p.Quantity > 0);

            IEnumerable<Product> ordered = q;
            switch ((ProductSortKey ?? "Name").Trim())
            {
                case "Price":
                    ordered = q.OrderBy(p => p.Price).ThenBy(p => p.ShortName);
                    break;
                case "Rating":
                    ordered = q.OrderByDescending(p => p.Rating).ThenBy(p => p.ShortName);
                    break;
                default:
                    ordered = q.OrderBy(p => p.ShortName);
                    break;
            }

            Products.Clear();
            foreach (var p in ordered)
                Products.Add(p);
            DisplayedCount = Products.Count;
            OnPropertyChanged(nameof(Products));
        }

        private void AddProduct()
        {
            if (!IsAdmin) return;
            var product = new Product { Id = 0 };
            var w = new ProductAddWindow { Owner = Application.Current.MainWindow };
            w.DataContext = new ProductFormViewModel(product, w);
            if (w.ShowDialog() == true)
            {
                var newId = ConfectioneryRepository.InsertProduct(product);
                product.Id = newId;
                _allProducts.Add(product);
                PushAction(new AddProductAction(CloneProduct(product)));
                LogAction($"ADD: {product.ShortName}");
                RebuildCategories();
                ApplyFilter();
            }
        }

        private void EditProduct()
        {
            if (!IsAdmin) return;
            if (SelectedProduct == null) return;
            OpenEditWindow(SelectedProduct);
        }

        private void DeleteProduct()
        {
            if (!IsAdmin) return;
            if (SelectedProduct == null) return;
            if (MessageBox.Show(App.T("S_Msg_DeleteSelected"), App.T("S_Msg_DeleteTitle"), MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;
            RemoveProduct(SelectedProduct);
        }

        private static bool? ShowEditor(Product product)
        {
            var w = new ProductEditWindow { Owner = Application.Current.MainWindow };
            w.DataContext = new ProductFormViewModel(product, w);
            return w.ShowDialog();
        }

        private void OpenProfile()
        {
            var profileWindow = new UserProfileWindow { Owner = Application.Current.MainWindow };
            profileWindow.DataContext = this;
            profileWindow.ShowDialog();
        }

        private void Undo()
        {
            if (!CanUndo) return;
            var action = _undoStack.Pop();
            action.Undo(_allProducts);
            _redoStack.Push(action);
            LogAction("UNDO");
            FinalizeHistoryOperation();
        }

        private void Redo()
        {
            if (!CanRedo) return;
            var action = _redoStack.Pop();
            action.Redo(_allProducts);
            _undoStack.Push(action);
            LogAction("REDO");
            FinalizeHistoryOperation();
        }

        private void FinalizeHistoryOperation()
        {
            RebuildCategories();
            ApplyFilter();
            try
            {
                ConfectioneryRepository.SyncProductsWithList(_allProducts);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "База данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            CommandManager.InvalidateRequerySuggested();
            OnPropertyChanged(nameof(CanUndo));
            OnPropertyChanged(nameof(CanRedo));
        }

        private void PushAction(IHistoryAction action)
        {
            _undoStack.Push(action);
            _redoStack.Clear();
            OnPropertyChanged(nameof(CanUndo));
            OnPropertyChanged(nameof(CanRedo));
            CommandManager.InvalidateRequerySuggested();
        }

        private static Product CloneProduct(Product product)
        {
            return new Product
            {
                Id = product.Id,
                CategoryId = product.CategoryId,
                ManufacturerId = product.ManufacturerId,
                ShortName = product.ShortName,
                FullName = product.FullName,
                Description = product.Description,
                ImagePath = product.ImagePath,
                PhotoData = product.PhotoData == null ? null : (byte[])product.PhotoData.Clone(),
                Category = product.Category,
                Rating = product.Rating,
                Price = product.Price,
                Quantity = product.Quantity,
                Color = product.Color,
                Size = product.Size,
                Country = product.Country,
                Discount = product.Discount,
                IsOutOfStock = product.IsOutOfStock,
                SoldCount = product.SoldCount,
                Manufacturer = product.Manufacturer
            };
        }

        private static void LogAction(string message)
        {
            try
            {
                var logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data_1");
                if (!Directory.Exists(logDir))
                    Directory.CreateDirectory(logDir);

                var logPath = Path.Combine(logDir, "actions.log");
                File.AppendAllText(logPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {message}{Environment.NewLine}");
            }
            catch
            {
            }
        }

        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private interface IHistoryAction
        {
            void Undo(List<Product> products);
            void Redo(List<Product> products);
        }

        private class AddProductAction : IHistoryAction
        {
            private readonly Product _product;

            public AddProductAction(Product product)
            {
                _product = product;
            }

            public void Undo(List<Product> products)
            {
                var existing = products.FirstOrDefault(p => p.Id == _product.Id);
                if (existing != null) products.Remove(existing);
            }

            public void Redo(List<Product> products)
            {
                if (products.All(p => p.Id != _product.Id))
                    products.Add(CloneProduct(_product));
            }
        }

        private class RemoveProductAction : IHistoryAction
        {
            private readonly Product _product;

            public RemoveProductAction(Product product)
            {
                _product = product;
            }

            public void Undo(List<Product> products)
            {
                if (products.All(p => p.Id != _product.Id))
                    products.Add(CloneProduct(_product));
            }

            public void Redo(List<Product> products)
            {
                var existing = products.FirstOrDefault(p => p.Id == _product.Id);
                if (existing != null) products.Remove(existing);
            }
        }

        private class EditProductAction : IHistoryAction
        {
            private readonly int _productId;
            private readonly Product _before;
            private readonly Product _after;

            public EditProductAction(Product target, Product before, Product after)
            {
                _productId = target.Id;
                _before = before;
                _after = after;
            }

            public void Undo(List<Product> products)
            {
                Replace(products, _productId, _before);
            }

            public void Redo(List<Product> products)
            {
                Replace(products, _productId, _after);
            }

            private static void Replace(List<Product> products, int id, Product source)
            {
                var existing = products.FirstOrDefault(p => p.Id == id);
                if (existing == null) return;
                existing.ShortName = source.ShortName;
                existing.FullName = source.FullName;
                existing.Description = source.Description;
                existing.ImagePath = source.ImagePath;
                existing.PhotoData = source.PhotoData == null ? null : (byte[])source.PhotoData.Clone();
                existing.CategoryId = source.CategoryId;
                existing.ManufacturerId = source.ManufacturerId;
                existing.Category = source.Category;
                existing.Rating = source.Rating;
                existing.Price = source.Price;
                existing.Quantity = source.Quantity;
                existing.Color = source.Color;
                existing.Size = source.Size;
                existing.Country = source.Country;
                existing.Discount = source.Discount;
                existing.IsOutOfStock = source.IsOutOfStock;
                existing.SoldCount = source.SoldCount;
                existing.Manufacturer = source.Manufacturer;
            }
        }
    }
}

