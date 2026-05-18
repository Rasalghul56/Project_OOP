using ConfectioneryShop.Models;
using Lab_06_OOP.ViewModels;

namespace ConfectioneryShop.Views
{
    public partial class ProductDetailWindow
    {
        public ProductDetailWindow(Product product, MainViewModel mainViewModel)
        {
            InitializeComponent();
            DataContext = new ProductDetailViewModel(product, mainViewModel, this);
        }
    }
}

