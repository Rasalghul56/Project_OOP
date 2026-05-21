using System.Windows.Input;
using Confectionery.Helpers;
using Confectionery.Services;
using Confectionery.UnitOfWork;
using Confectionery.ViewModels.Base;


namespace Confectionery.ViewModels.Admin
{
    public class AdminViewModel : BaseViewModel
    {
        private readonly IUnitOfWork _uow;

        public DashboardViewModel       DashboardVM { get; }
        public ProductManagementViewModel ProductsVM  { get; }
        public OrderManagementViewModel  OrdersVM    { get; }
        public ReviewManagementViewModel ReviewsVM   { get; }

        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public ICommand ShowDashboardCommand { get; }
        public ICommand ShowProductsCommand  { get; }
        public ICommand ShowOrdersCommand    { get; }
        public ICommand ShowReviewsCommand   { get; }
        public ICommand ToggleThemeCommand   { get; }
        public ICommand ToggleLangCommand    { get; }
        public ICommand LogoutCommand        { get; }

        public AdminViewModel(IUnitOfWork uow, INavigationService navigation)
        {
            _uow = uow;

            DashboardVM = new DashboardViewModel(uow);
            ProductsVM  = new ProductManagementViewModel(uow);
            OrdersVM    = new OrderManagementViewModel(uow);
            ReviewsVM   = new ReviewManagementViewModel(uow);

            ShowDashboardCommand = new RelayCommand(p => CurrentView = DashboardVM);
            ShowProductsCommand  = new RelayCommand(p => CurrentView = ProductsVM);
            ShowOrdersCommand    = new RelayCommand(p => CurrentView = OrdersVM);
            ShowReviewsCommand   = new RelayCommand(p => CurrentView = ReviewsVM);
            ToggleThemeCommand   = new RelayCommand(p => ThemeService.Toggle());
            ToggleLangCommand    = new RelayCommand(p => LanguageService.Toggle());
            LogoutCommand = new RelayCommand(p =>
            {
                SessionService.Logout();
                navigation.NavigateToLogin();
            });

            CurrentView = DashboardVM;
        }
    }
}
