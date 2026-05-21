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
    public class ProductDetailViewModel : BaseViewModel
    {
        private readonly IUnitOfWork _uow;
        private readonly CartViewModel _cart;

        private int _newRating = 5;
        private string _newText;
        private string _statusMessage;
        private bool _canAddReview;
        private bool _isReviewFormVisible;

        public Product Product { get; }

        public ObservableCollection<Review> Reviews { get; } = new ObservableCollection<Review>();

        public string AverageRating => ReviewRatingHelper.FormatDetailed(Reviews);

        public int NewRating
        {
            get => _newRating;
            set => SetProperty(ref _newRating, value);
        }

        public string NewText
        {
            get => _newText;
            set => SetProperty(ref _newText, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool CanAddReview
        {
            get => _canAddReview;
            private set => SetProperty(ref _canAddReview, value);
        }

        public bool IsReviewFormVisible
        {
            get => _isReviewFormVisible;
            set => SetProperty(ref _isReviewFormVisible, value);
        }

        public string NoReviewReason { get; private set; }

        public ICommand AddToCartCommand { get; }
        public ICommand SubmitReviewCommand { get; }
        public ICommand ToggleReviewFormCommand { get; }

        public ProductDetailViewModel(IUnitOfWork uow, Product product, CartViewModel cart)
        {
            _uow = uow;
            _cart = cart;
            Product = product;

            AddToCartCommand = new RelayCommand(p => _cart.AddProduct(Product));

            ToggleReviewFormCommand = new RelayCommand(p =>
                IsReviewFormVisible = !IsReviewFormVisible);

            SubmitReviewCommand = new RelayCommand(ExecuteSubmit,
                p => CanAddReview && !string.IsNullOrWhiteSpace(NewText));

            LoadReviews();
        }

        private void LoadReviews()
        {
            Reviews.Clear();
            foreach (var r in _uow.Reviews.GetByProduct(Product.Id))
                Reviews.Add(r);

            SyncProductReviews();
            OnPropertyChanged(nameof(AverageRating));

            var user = SessionService.CurrentUser;
            if (user == null)
            {
                CanAddReview = false;
                NoReviewReason = "Войдите в систему, чтобы оставить отзыв.";
                return;
            }

            bool alreadyReviewed = _uow.Reviews.UserHasReviewedProduct(user.Id, Product.Id);
            bool hasOrdered = _uow.Reviews.UserHasOrderedProduct(user.Id, Product.Id);

            if (alreadyReviewed)
            {
                CanAddReview = false;
                NoReviewReason = "Вы уже оставили отзыв на этот товар.";
            }
            else if (!hasOrdered)
            {
                CanAddReview = false;
                NoReviewReason = "Отзыв можно оставить только после получения заказа.";
            }
            else
            {
                CanAddReview = true;
                NoReviewReason = null;
            }

            OnPropertyChanged(nameof(NoReviewReason));
        }

        private void ExecuteSubmit(object p)
        {
            var user = SessionService.CurrentUser;
            if (user == null) return;

            var rating = NewRating;
            if (rating < 1) rating = 1;
            if (rating > 5) rating = 5;

            var review = new Review
            {
                UserId = user.Id,
                ProductId = Product.Id,
                Rating = rating,
                Text = NewText.Trim()
            };

            _uow.Reviews.Add(review);
            _uow.Save();

            NewText = null;
            NewRating = 5;
            IsReviewFormVisible = false;
            StatusMessage = "Спасибо за ваш отзыв!";
            LoadReviews();
        }

        private void SyncProductReviews()
        {
            if (Product.Reviews == null)
                Product.Reviews = new System.Collections.Generic.List<Review>();
            else
                Product.Reviews.Clear();
            foreach (var r in Reviews)
                Product.Reviews.Add(r);
        }
    }
}
