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
    /// <summary>Главная витрина: популярное, новинки, рекомендации, категории.</summary>
    public class ShowcaseViewModel : BaseViewModel
    {
        private readonly IUnitOfWork  _uow;
        private readonly CartViewModel _cart;

        public ObservableCollection<Product>  PopularProducts    { get; } = new ObservableCollection<Product>();
        public ObservableCollection<Product>  NewProducts        { get; } = new ObservableCollection<Product>();
        public ObservableCollection<Product>  RecommendedProducts{ get; } = new ObservableCollection<Product>();
        public ObservableCollection<Category> Categories         { get; } = new ObservableCollection<Category>();

        public string WelcomeText => $"Добро пожаловать, {SessionService.CurrentUser?.Name ?? "гость"}! 👋";

        public ICommand AddToCartCommand  { get; }
        public ICommand ShowDetailCommand { get; }

        // Навигация — ссылки на команды главного окна (устанавливаются снаружи)
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
            {
                // Перейти в каталог с выбранной категорией (NavigateToCatalogCommand задаётся снаружи)
                NavigateToCatalogCommand?.Execute(p);
            });

            Load();
        }

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

            // Топ-6 по среднему рейтингу (у которых есть отзывы), остальные — по имени
            var withReviews = all
                .Where(p => p.Reviews != null && p.Reviews.Any())
                .OrderByDescending(p => p.Reviews.Average(r => r.Rating))
                .Take(6)
                .ToList();

            // Дополняем без отзывов, если меньше 6
            if (withReviews.Count < 6)
            {
                var rest = all
                    .Where(p => p.Reviews == null || !p.Reviews.Any())
                    .OrderBy(p => p.Name)
                    .Take(6 - withReviews.Count);
                withReviews.AddRange(rest);
            }

            foreach (var p in withReviews) PopularProducts.Add(p);
        }

        private void LoadNew()
        {
            NewProducts.Clear();
            // Последние 4 добавленных товара
            var items = _uow.Products.GetAvailable()
                .OrderByDescending(p => p.Id)
                .Take(4);
            foreach (var p in items) NewProducts.Add(p);
        }

        private void LoadRecommended()
        {
            RecommendedProducts.Clear();
            var user = SessionService.CurrentUser;
            if (user == null) return;

            // Товары из прошлых заказов пользователя, которые он ещё не заказывал недавно
            var orders = _uow.Orders.GetByUser(user.Id);
            var orderedIds = orders
                .SelectMany(o => o.OrderItems)
                .Select(oi => oi.ProductId)
                .Distinct()
                .ToHashSet();

            if (orderedIds.Count > 0)
            {
                // Похожие: одна категория, но новые позиции
                var categories = orders
                    .SelectMany(o => o.OrderItems.Select(oi => oi.Product?.CategoryId ?? 0))
                    .Distinct()
                    .ToList();

                var recs = _uow.Products.GetAvailable()
                    .Where(p => p.CategoryId.HasValue && categories.Contains(p.CategoryId.Value) && !orderedIds.Contains(p.Id))
                    .OrderByDescending(p => p.Id)
                    .Take(4)
                    .ToList();

                foreach (var p in recs) RecommendedProducts.Add(p);
            }

            // Если рекомендаций мало — добавляем популярные
            if (RecommendedProducts.Count < 4)
            {
                var more = _uow.Products.GetAvailable()
                    .Where(p => !orderedIds.Contains(p.Id) && !RecommendedProducts.Contains(p))
                    .Take(4 - RecommendedProducts.Count);
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
