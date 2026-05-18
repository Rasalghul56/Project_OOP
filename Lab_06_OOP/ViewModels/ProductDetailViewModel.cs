using System.Windows;
using System.Windows.Input;
using ConfectioneryShop.Models;
using Lab_06_OOP;
using Lab_06_OOP.Commands;

namespace Lab_06_OOP.ViewModels
{
    public class ProductDetailViewModel
    {
        private readonly MainViewModel _main;
        private readonly Window _window;

        public ProductDetailViewModel(Product product, MainViewModel mainViewModel, Window window)
        {
            Product = product;
            _main = mainViewModel;
            _window = window;
            EditCommand = new RelayCommand(_ => Edit(), _ => _main.IsAdmin);
            DeleteCommand = new RelayCommand(_ => Delete(), _ => _main.IsAdmin);
            CloseCommand = new RelayCommand(_ => _window.Close());
        }

        public Product Product { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand CloseCommand { get; }

        private void Edit()
        {
            if (!_main.OpenEditWindow(Product)) return;
            _window.DataContext = new ProductDetailViewModel(Product, _main, _window);
        }

        private void Delete()
        {
            if (MessageBox.Show(App.T("S_Msg_DeleteOne"), App.T("S_Msg_DeleteTitle"), MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;
            _main.RemoveProduct(Product);
            _window.Close();
        }
    }
}

