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
    public class CatalogViewModel : BaseViewModel
    {
        private readonly IUnitOfWork _uow;
        private readonly CartViewModel _cart;

        private string _searchText;
        private Category _selectedCategory;
        private string _selectedSort;

        private readonly List<Product> _allProducts = new List<Product>();

        public ObservableCollection<Category> Categories { get; } = new ObservableCollection<Category>();
        public ObservableCollection<Product> Products { get; } = new ObservableCollection<Product>();
        public ObservableCollection<string> SortOptions { get; } = new ObservableCollection<string>();

        private static string[] GetLocalizedSortOptions()
        {
            return LanguageService.IsEnglish
                ? new[] { "Name A–Z", "Name Z–A", "Price ↑", "Price ↓", "Rating ↓" }
                : new[] { "По названию А–Я", "По названию Я–А", "Цена ↑", "Цена ↓", "Рейтинг ↓" };
        }

        private static string AllCategoriesLabel
            => LanguageService.IsEnglish ? "All categories" : "Все категории";

        public string SearchText
        {
            get => _searchText;
            set { SetProperty(ref _searchText, value); ApplyFilter(); }
        }

        public Category SelectedCategory
        {
            get => _selectedCategory;
            set { SetProperty(ref _selectedCategory, value); ApplyFilter(); }
        }

        public string SelectedSort
        {
            get => _selectedSort;
            set { SetProperty(ref _selectedSort, value); ApplyFilter(); }
        }

        public ICommand AddToCartCommand { get; }
        public ICommand ClearFiltersCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ShowDetailCommand { get; }

        // Backward compat alias used in XAML
        public ICommand ClearCategoryCommand => ClearFiltersCommand;

        public CatalogViewModel(IUnitOfWork uow, CartViewModel cart)
        {
            _uow = uow;
            _cart = cart;

            RebuildSortOptions();

            AddToCartCommand  = new RelayCommand(p => { if (p is Product pr) _cart.AddProduct(pr); });
            ClearFiltersCommand = new RelayCommand(_ =>
            {
                _searchText = null;
                OnPropertyChanged(nameof(SearchText));
                SelectedSort     = SortOptions.FirstOrDefault();
                SelectedCategory = Categories.FirstOrDefault();
                // ApplyFilter вызывается цепочкой через SelectedCategory setter
            });
            RefreshCommand    = new RelayCommand(p => LoadData());
            ShowDetailCommand = new RelayCommand(p => { if (p is Product pr) OpenDetail(pr); });

            LanguageService.LanguageChanged += OnLanguageChanged;

            LoadData();
        }

        private void OnLanguageChanged()
        {
            // Пересоздаём сортировки, сохраняя индекс выбранного
            int sortIdx = SortOptions.IndexOf(SelectedSort);
            RebuildSortOptions();
            SelectedSort = sortIdx >= 0 && sortIdx < SortOptions.Count
                ? SortOptions[sortIdx]
                : SortOptions[0];

            // Пересоздаём список категорий (чтобы обновился ярлык «Все»)
            var prevId = SelectedCategory?.Id ?? 0;
            LoadCategories();
            SelectedCategory = Categories.FirstOrDefault(c => c.Id == prevId)
                               ?? Categories.First();

            ApplyFilter();
        }

        private void LoadCategories()
        {
            Categories.Clear();
            Categories.Add(new Category { Id = 0, Name = AllCategoriesLabel });
            foreach (var c in _uow.Categories.GetAll()) Categories.Add(c);
        }

        private void RebuildSortOptions()
        {
            SortOptions.Clear();
            foreach (var s in GetLocalizedSortOptions()) SortOptions.Add(s);
            if (SelectedSort == null) SelectedSort = SortOptions[0];
        }

        private void LoadData()
        {
            LoadCategories();
            SelectedCategory = Categories.First();

            _allProducts.Clear();
            foreach (var p in _uow.Products.GetAvailable()) _allProducts.Add(p);

            ApplyFilter();
        }

        private void ApplyFilter()
        {
            IEnumerable<Product> filtered = _allProducts;

            if (SelectedCategory != null && SelectedCategory.Id != 0)
                filtered = filtered.Where(p => p.CategoryId == SelectedCategory.Id);

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var q = SearchText.ToLower();
                filtered = filtered.Where(p =>
                    p.Name.ToLower().Contains(q) ||
                    (p.Description != null && p.Description.ToLower().Contains(q)));
            }

            filtered = ApplySort(filtered);

            Products.Clear();
            foreach (var p in filtered) Products.Add(p);
        }

        private IEnumerable<Product> ApplySort(IEnumerable<Product> source)
        {
            // Сравниваем по индексу в текущем SortOptions (независимо от языка)
            int idx = SortOptions.IndexOf(SelectedSort);
            switch (idx)
            {
                case 1: return source.OrderByDescending(p => p.Name);
                case 2: return source.OrderBy(p => p.Price);
                case 3: return source.OrderByDescending(p => p.Price);
                case 4: return source.OrderByDescending(p =>
                    p.Reviews != null && p.Reviews.Any()
                        ? p.Reviews.Average(r => r.Rating) : 0);
                default: return source.OrderBy(p => p.Name);
            }
        }

        private void OpenDetail(Product product)
        {
            var full = _uow.Products.GetWithDetails(product.Id);
            var vm   = new ProductDetailViewModel(_uow, full, _cart);
            new Views.Client.ProductDetailWindow { DataContext = vm }.Show();
        }
    }
}
