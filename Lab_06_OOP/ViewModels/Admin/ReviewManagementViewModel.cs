using System.Collections.ObjectModel;
using System.Windows.Input;
using Confectionery.Helpers;
using Confectionery.Models;
using Confectionery.UnitOfWork;
using Confectionery.ViewModels.Base;

namespace Confectionery.ViewModels.Admin
{
    public class ReviewManagementViewModel : BaseViewModel
    {
        private readonly IUnitOfWork _uow;

        private Review _selectedReview;
        private string _replyText;
        private string _statusMessage;

        public ObservableCollection<Review> Reviews { get; } = new ObservableCollection<Review>();

        public Review SelectedReview
        {
            get => _selectedReview;
            set
            {
                SetProperty(ref _selectedReview, value);
                AdminReply = value?.AdminReply;
            }
        }

        // Псевдоним, который ожидает XAML
        public string AdminReply
        {
            get => _replyText;
            set => SetProperty(ref _replyText, value);
        }

        public string ReplyText
        {
            get => _replyText;
            set => SetProperty(ref _replyText, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public ICommand RefreshCommand     { get; }
        public ICommand SaveReplyCommand   { get; }
        public ICommand SendReplyCommand   { get; }
        public ICommand DeleteReviewCommand { get; }

        public ReviewManagementViewModel(IUnitOfWork uow)
        {
            _uow = uow;

            RefreshCommand = new RelayCommand(p => LoadReviews());

            SaveReplyCommand = SendReplyCommand = new RelayCommand(p =>
            {
                if (SelectedReview == null) return;
                SelectedReview.AdminReply = AdminReply;
                _uow.Reviews.Update(SelectedReview);
                _uow.Save();
                StatusMessage = "Ответ сохранён.";
                LoadReviews();
            }, p => SelectedReview != null);

            DeleteReviewCommand = new RelayCommand(p =>
            {
                if (SelectedReview == null) return;
                _uow.Reviews.Delete(SelectedReview.Id);
                _uow.Save();
                StatusMessage = "Отзыв удалён.";
                LoadReviews();
            }, p => SelectedReview != null);

            LoadReviews();
        }

        private void LoadReviews()
        {
            Reviews.Clear();
            foreach (var r in _uow.Reviews.GetAllWithDetails())
                Reviews.Add(r);
        }
    }
}
