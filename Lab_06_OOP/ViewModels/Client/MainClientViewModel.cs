using System.Windows.Input;
using Confectionery.Helpers;
using Confectionery.Services;
using Confectionery.UnitOfWork;
using Confectionery.ViewModels.Base;

namespace Confectionery.ViewModels.Client
{
    public class MainClientViewModel : BaseViewModel
    {
        private readonly IUnitOfWork _uow;

        public ShowcaseViewModel  ShowcaseVM  { get; }
        public CatalogViewModel   CatalogVM   { get; }
        public CartViewModel      CartVM      { get; }
        public OrdersViewModel    OrdersVM    { get; }
        public ProfileViewModel   ProfileVM   { get; }

        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        private string _userName;
        public string UserName
        {
            get => _userName;
            set => SetProperty(ref _userName, value);
        }

        private int _cartItemCount;
        public int CartItemCount
        {
            get => _cartItemCount;
            private set
            {
                SetProperty(ref _cartItemCount, value);
                OnPropertyChanged(nameof(CartHasItems));
            }
        }

        public bool CartHasItems => CartItemCount > 0;

        public ICommand ShowShowcaseCommand { get; }
        public ICommand ShowCatalogCommand  { get; }
        public ICommand ShowCartCommand     { get; }
        public ICommand ShowOrdersCommand   { get; }
        public ICommand ShowProfileCommand  { get; }
        public ICommand ToggleThemeCommand  { get; }
        public ICommand ToggleLangCommand   { get; }
        public ICommand LogoutCommand       { get; }

        public MainClientViewModel(IUnitOfWork uow, INavigationService navigation)
        {
            _uow = uow;

            CartVM      = new CartViewModel(uow);
            CatalogVM   = new CatalogViewModel(uow, CartVM);
            OrdersVM    = new OrdersViewModel(uow, CartVM);
            ProfileVM   = new ProfileViewModel(uow);
            ShowcaseVM  = new ShowcaseViewModel(uow, CartVM);

            UserName = SessionService.CurrentUser?.Name;

            CartVM.ItemsChanged += count => CartItemCount = count;

            ShowShowcaseCommand = new RelayCommand(p =>
            {
                ShowcaseVM.Load();
                CurrentView = ShowcaseVM;
            });
            ShowCatalogCommand = new RelayCommand(p => CurrentView = CatalogVM);
            ShowCartCommand    = new RelayCommand(p => CurrentView = CartVM);
            ShowOrdersCommand  = new RelayCommand(p =>
            {
                OrdersVM.LoadOrders();
                CurrentView = OrdersVM;
            });
            ShowProfileCommand = new RelayCommand(p => CurrentView = ProfileVM);
            ToggleThemeCommand = new RelayCommand(p => ThemeService.Toggle());
            ToggleLangCommand  = new RelayCommand(p => LanguageService.Toggle());
            LogoutCommand = new RelayCommand(p =>
            {
                SessionService.Logout();
                navigation.NavigateToLogin();
            });

            // Showcase может навигировать в каталог
            ShowcaseVM.NavigateToCatalogCommand = ShowCatalogCommand;

            // Открываемся с Витрины
            CurrentView = ShowcaseVM;
        }
    }
}
