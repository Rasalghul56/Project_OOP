using Confectionery.Models;
using Confectionery.ViewModels.Base;

namespace Confectionery.ViewModels.Client
{
    public class CartItemViewModel : BaseViewModel
    {
        private int _quantity;

        public Product Product { get; }

        public int Quantity
        {
            get => _quantity;
            set
            {
                if (value < 1) return;
                SetProperty(ref _quantity, value);
                OnPropertyChanged(nameof(Subtotal));
            }
        }

        public decimal Subtotal => Product.Price * Quantity;

        public CartItemViewModel(Product product, int quantity = 1)
        {
            Product = product;
            _quantity = quantity;
        }
    }
}
