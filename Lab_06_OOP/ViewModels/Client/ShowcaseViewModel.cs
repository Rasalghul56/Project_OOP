using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Confectionery.Helpers;
using Confectionery.Models;
using Confectionery.Services;
using Confectionery.UnitOfWork;
using Confectionery.ViewModels.Base;

namespace Confectionery.ViewModels.Client
{

    public class ShowcaseViewModel : BaseViewModel
    {
        private readonly IUnitOfWork  _uow;
        private readonly CartViewModel _cart;

        private string _welcomeText;

        public ObservableCollection<Product>  PopularProducts    { get; } = new ObservableCollection<Product>();
        public ObservableCollection<Product>  NewProducts        { get; } = new ObservableCollection<Product>();
        public ObservableCollection<Product>  RecommendedProducts{ get; } = new ObservableCollection<Product>();
        public ObservableCollection<Category> Categories         { get; } = new ObservableCollection<Category>();

        public string WelcomeText
        {
            get => _welcomeText;
            private set => SetProperty(ref _welcomeText, value);
        }

        public ICommand AddToCartCommand  { get; }
        public ICommand ShowDetailCommand { get; }

        public ICommand NavigateToCatalogCommand { get; set; }
        public ICommand NavigateToCategoryCommand { get; }

        public ShowcaseViewModel(IUnitOfWork uow, CartViewModel cart)
        {
            _uow  = uow;
            _cart = cart;

            AddToCartCommand  = new RelayCommand(p => { if (p is Product pr) _cart.AddProduct(pr); });
            ShowDetailCommand = new RelayCommand(p =>
            {
                if (!(p is Product pr)) return;
                var full = _uow.Products.GetWithDetails(pr.Id);
                var vm   = new ProductDetailViewModel(_uow, full, _cart);
                new Views.Client.ProductDetailWindow { DataContext = vm }.Show();
            });
            NavigateToCategoryCommand = new RelayCommand(p =>
                NavigateToCatalogCommand?.Execute(p));

            LanguageService.LanguageChanged += OnLanguageChanged;
            UpdateWelcomeText();
            Load();
        }

        private void OnLanguageChanged()
        {
            UpdateWelcomeText();
            Load();
        }

        private void UpdateWelcomeText()
        {
            var user = SessionService.CurrentUser;
            if (user == null)
                WelcomeText = GetResourceString("Showcase_WelcomeGuest");
            else
            {
                var fmt = GetResourceString("Showcase_WelcomeUser");
                WelcomeText = string.Format(fmt, user.Name);
            }
        }

        private static string GetResourceString(string key)
            => Application.Current.TryFindResource(key) as string ?? key;

        public void Load()
        {
            LoadPopular();
            LoadNew();
            LoadRecommended();
            LoadCategories();
        }

        private void LoadPopular()
        {
            PopularProducts.Clear();
            var all = _uow.Products.GetAvailable().ToList();
            _uow.Products.AttachReviews(all);

            var withReviews = all
                .Where(p => ReviewRatingHelper.GetAverage(p.Reviews).HasValue)
                .OrderByDescending(p => ReviewRatingHelper.GetAverage(p.Reviews).Value)
                .Take(6)
                .ToList();

            if (withReviews.Count < 6)
            {
                var rest = all
                    .Where(p => !ReviewRatingHelper.GetAverage(p.Reviews).HasValue)
                    .OrderBy(p => p.Name)
                    .Take(6 - withReviews.Count);
                withReviews.AddRange(rest);
            }

            foreach (var p in withReviews) PopularProducts.Add(p);
        }

        private void LoadNew()
        {
            NewProducts.Clear();
            var items = _uow.Products.GetAvailable()
                .OrderByDescending(p => p.Id)
                .Take(4)
                .ToList();
            _uow.Products.AttachReviews(items);
            foreach (var p in items) NewProducts.Add(p);
        }

        private void LoadRecommended()
        {
            RecommendedProducts.Clear();
            var user = SessionService.CurrentUser;
            if (user == null) return;

            var orders = _uow.Orders.GetByUser(user.Id);
            var orderedIds = orders
                .SelectMany(o => o.OrderItems)
                .Select(oi => oi.ProductId)
                .Distinct()
                .ToHashSet();

            if (orderedIds.Count > 0)
            {
                var categories = orders
                    .SelectMany(o => o.OrderItems.Select(oi => oi.Product?.CategoryId ?? 0))
                    .Distinct()
                    .ToList();

                var recs = _uow.Products.GetAvailable()
                    .Where(p => p.CategoryId.HasValue && categories.Contains(p.CategoryId.Value) && !orderedIds.Contains(p.Id))
                    .OrderByDescending(p => p.Id)
                    .Take(4)
                    .ToList();
                _uow.Products.AttachReviews(recs);

                foreach (var p in recs) RecommendedProducts.Add(p);
            }

            if (RecommendedProducts.Count < 4)
            {
                var more = _uow.Products.GetAvailable()
                    .Where(p => !orderedIds.Contains(p.Id) && !RecommendedProducts.Contains(p))
                    .Take(4 - RecommendedProducts.Count)
                    .ToList();
                _uow.Products.AttachReviews(more);
                foreach (var p in more) RecommendedProducts.Add(p);
            }
        }

        private void LoadCategories()
        {
            Categories.Clear();
            foreach (var c in _uow.Categories.GetAll()) Categories.Add(c);
        }
    }
}
