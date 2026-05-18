using System.IO;
using System.Windows;
using System.Windows.Input;
using ConfectioneryShop.Models;
using Lab_06_OOP.Commands;
using Microsoft.Win32;

namespace Lab_06_OOP.ViewModels
{
    public class ProductFormViewModel
    {
        public ProductFormViewModel(Product product, Window window)
        {
            Product = product;
            SaveCommand = new RelayCommand(_ =>
            {
                window.DialogResult = true;
                window.Close();
            });
            CancelCommand = new RelayCommand(_ =>
            {
                window.DialogResult = false;
                window.Close();
            });
            PickPhotoCommand = new RelayCommand(_ =>
            {
                var dlg = new OpenFileDialog
                {
                    Filter = "Изображения|*.png;*.jpg;*.jpeg;*.bmp|Все файлы|*.*"
                };
                if (dlg.ShowDialog() != true) return;
                Product.PhotoData = File.ReadAllBytes(dlg.FileName);
                if (string.IsNullOrWhiteSpace(Product.ImagePath))
                    Product.ImagePath = Path.GetFileName(dlg.FileName);
            });
        }

        public Product Product { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand PickPhotoCommand { get; }
    }
}
