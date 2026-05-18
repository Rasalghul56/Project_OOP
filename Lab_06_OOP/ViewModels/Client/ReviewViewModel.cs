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
    public class ReviewViewModel : BaseViewModel
    {
        private readonly IUnitOfWork _uow;
        private readonly int _productId;

        private int _newRating = 5;
        private string _newText;
        private string _statusMessage;
        private bool _canAddReview;

        public ObservableCollection<Review> Reviews { get; } = new ObservableCollection<Review>();

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
            set => SetProperty(ref _canAddReview, value);
        }

        public ICommand SubmitReviewCommand { get; }

        public ReviewViewModel(IUnitOfWork uow, int productId)
        {
            _uow = uow;
            _productId = productId;

            SubmitReviewCommand = new RelayCommand(ExecuteSubmit, p =>
                CanAddReview && !string.IsNullOrWhiteSpace(NewText));

            LoadReviews();
        }

        private void LoadReviews()
        {
            Reviews.Clear();
            foreach (var r in _uow.Reviews.GetByProduct(_productId))
                Reviews.Add(r);

            var user = SessionService.CurrentUser;
            if (user != null)
            {
                bool alreadyReviewed = _uow.Reviews.UserHasReviewedProduct(user.Id, _productId);
                bool hasOrdered = _uow.Reviews.UserHasOrderedProduct(user.Id, _productId);
                CanAddReview = hasOrdered && !alreadyReviewed;

                if (!hasOrdered)
                    StatusMessage = "Вы можете оставить отзыв только после получения заказа.";
                else if (alreadyReviewed)
                    StatusMessage = "Вы уже оставили отзыв на этот товар.";
            }
        }

        private void ExecuteSubmit(object p)
        {
            var user = SessionService.CurrentUser;
            if (user == null) return;

            var review = new Review
            {
                UserId = user.Id,
                ProductId = _productId,
                Rating = NewRating,
                Text = NewText
            };

            _uow.Reviews.Add(review);
            _uow.Save();

            NewText = null;
            NewRating = 5;
            StatusMessage = "Отзыв успешно добавлен!";
            LoadReviews();
        }
    }
}
