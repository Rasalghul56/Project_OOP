using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Microsoft.Win32;
using Confectionery.Helpers;
using Confectionery.Models;
using Confectionery.UnitOfWork;
using Confectionery.ViewModels.Base;

namespace Confectionery.ViewModels.Admin
{
    public class ProductManagementViewModel : BaseViewModel
    {
        private readonly IUnitOfWork _uow;
        private readonly List<Product> _allProducts = new List<Product>();

        private Product _selectedProduct;
        private string _searchText;
        private bool _isEditing;
        private string _statusMessage;

        // Форма
        private string _formName;
        private string _formDescription;
        private string _formComposition;
        private string _formPrice;
        private string _formWeight;
        private string _formImagePath;
        private bool _formIsAvailable = true;
        private Category _formCategory;

        public ObservableCollection<Product> Products { get; } = new ObservableCollection<Product>();
        public ObservableCollection<Category> Categories { get; } = new ObservableCollection<Category>();

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    OnPropertyChanged(nameof(HasSearchText));
                    ApplyFilter();
                }
            }
        }

        public bool HasSearchText => !string.IsNullOrWhiteSpace(SearchText);

        public Product SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                SetProperty(ref _selectedProduct, value);
                if (value != null) FillForm(value);
            }
        }

        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        private string _formError;
        public string FormError
        {
            get => _formError;
            set => SetProperty(ref _formError, value);
        }

        public string FormName { get => _formName; set => SetProperty(ref _formName, value); }
        public string FormDescription { get => _formDescription; set => SetProperty(ref _formDescription, value); }
        public string FormComposition { get => _formComposition; set => SetProperty(ref _formComposition, value); }
        public string FormPrice { get => _formPrice; set => SetProperty(ref _formPrice, value); }
        public string FormWeight { get => _formWeight; set => SetProperty(ref _formWeight, value); }
        public string FormImagePath { get => _formImagePath; set => SetProperty(ref _formImagePath, value); }
        public bool FormIsAvailable { get => _formIsAvailable; set => SetProperty(ref _formIsAvailable, value); }
        public Category FormCategory { get => _formCategory; set => SetProperty(ref _formCategory, value); }

        public ICommand AddNewCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand BrowseImageCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ClearSearchCommand { get; }

        public ProductManagementViewModel(IUnitOfWork uow)
        {
            _uow = uow;

            ClearSearchCommand = new RelayCommand(_ => SearchText = null);

            AddNewCommand = new RelayCommand(p => StartAdd());
            EditCommand = new RelayCommand(p => IsEditing = true, p => SelectedProduct != null);
            DeleteCommand = new RelayCommand(p => ExecuteDelete(), p => SelectedProduct != null);
            SaveCommand = new RelayCommand(p => ExecuteSave());
            CancelCommand = new RelayCommand(p => { IsEditing = false; SelectedProduct = null; ClearForm(); FormError = null; });
            BrowseImageCommand = new RelayCommand(p => BrowseImage());
            RefreshCommand = new RelayCommand(p => LoadData());

            LoadData();
        }

        private void LoadData()
        {
            _allProducts.Clear();
            _allProducts.AddRange(_uow.Products.GetAllWithCategory());

            Categories.Clear();
            foreach (var c in _uow.Categories.GetAll())
                Categories.Add(c);

            ApplyFilter();
            StatusMessage = null;
        }

        private void ApplyFilter()
        {
            IEnumerable<Product> filtered = _allProducts;

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var q = SearchText.Trim().ToLower();
                filtered = filtered.Where(p =>
                    (p.Name != null && p.Name.ToLower().Contains(q)) ||
                    (p.Category?.Name != null && p.Category.Name.ToLower().Contains(q)) ||
                    (p.Description != null && p.Description.ToLower().Contains(q)) ||
                    (p.Composition != null && p.Composition.ToLower().Contains(q)));
            }

            var selectedId = SelectedProduct?.Id;
            Products.Clear();
            foreach (var p in filtered.OrderBy(p => p.Name))
                Products.Add(p);

            if (selectedId.HasValue)
                SelectedProduct = Products.FirstOrDefault(p => p.Id == selectedId.Value);
        }

        private void StartAdd()
        {
            SelectedProduct = null;
            ClearForm();
            IsEditing = true;
        }

        private void FillForm(Product p)
        {
            FormName = p.Name;
            FormDescription = p.Description;
            FormComposition = p.Composition;
            FormPrice = p.Price.ToString("F2");
            FormWeight = p.Weight.ToString();
            FormImagePath = p.ImagePath;
            FormIsAvailable = p.IsAvailable;
            FormCategory = Categories.FirstOrDefault(c => c.Id == p.CategoryId);
        }

        private void ClearForm()
        {
            FormName = FormDescription = FormComposition = FormPrice = FormWeight = FormImagePath = null;
            FormIsAvailable = true;
            FormCategory = null;
        }

        private bool IsFormPriceValid()
        {
            if (decimal.TryParse(FormPrice?.Replace(',', '.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var val))
                return val > 0;
            return false;
        }

        private static readonly System.Text.RegularExpressions.Regex OnlyDigits =
            new System.Text.RegularExpressions.Regex(@"^\d+$");

        private bool ValidateForm()
        {
            FormError = null;

            if (string.IsNullOrWhiteSpace(FormName))
            { FormError = "Введите название товара."; return false; }

            if (OnlyDigits.IsMatch(FormName.Trim()))
            { FormError = "Название не может состоять только из цифр."; return false; }

            if (FormName.Trim().Length < 2)
            { FormError = "Название должно содержать минимум 2 символа."; return false; }

            if (string.IsNullOrWhiteSpace(FormPrice))
            { FormError = "Введите цену товара."; return false; }

            if (!decimal.TryParse(FormPrice.Replace(',', '.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var p) || p <= 0)
            { FormError = "Цена должна быть числом больше нуля."; return false; }

            if (!string.IsNullOrWhiteSpace(FormWeight))
            {
                if (!double.TryParse(FormWeight.Replace(',', '.'),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var w) || w <= 0)
                { FormError = "Вес должен быть числом больше нуля."; return false; }
            }

            if (FormCategory == null)
            { FormError = "Выберите категорию товара."; return false; }

            return true;
        }

        private void ExecuteSave()
        {
            if (!ValidateForm()) return;

            decimal.TryParse(FormPrice?.Replace(',', '.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var price);

            double.TryParse(FormWeight?.Replace(',', '.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var weight);

            if (SelectedProduct == null)
            {
                var product = new Product
                {
                    Name = FormName,
                    Description = FormDescription,
                    Composition = FormComposition,
                    Price = price,
                    Weight = weight,
                    ImagePath = FormImagePath,
                    IsAvailable = FormIsAvailable,
                    CategoryId = FormCategory?.Id
                };
                _uow.Products.Add(product);
                StatusMessage = "Товар добавлен.";
            }
            else
            {
                SelectedProduct.Name = FormName;
                SelectedProduct.Description = FormDescription;
                SelectedProduct.Composition = FormComposition;
                SelectedProduct.Price = price;
                SelectedProduct.Weight = weight;
                SelectedProduct.ImagePath = FormImagePath;
                SelectedProduct.IsAvailable = FormIsAvailable;
                SelectedProduct.CategoryId = FormCategory?.Id;
                _uow.Products.Update(SelectedProduct);
                StatusMessage = "Товар обновлён.";
            }

            _uow.Save();
            IsEditing = false;
            LoadData();
        }

        private void ExecuteDelete()
        {
            if (SelectedProduct == null) return;
            _uow.Orders.DetachProductFromOrderItems(SelectedProduct.Id, SelectedProduct.Name);
            _uow.Products.Delete(SelectedProduct.Id);
            _uow.Save();
            StatusMessage = "Товар удалён.";
            LoadData();
        }

        private void BrowseImage()
        {
            var dlg = new OpenFileDialog
            {
                Title = "Выберите изображение",
                Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
            };
            if (dlg.ShowDialog() != true) return;

            // Copy image to AppData — persists across rebuilds and reinstalls
            Directory.CreateDirectory(ImageHelper.ImagesDirectory);

            var ext      = Path.GetExtension(dlg.FileName);
            var fileName = Guid.NewGuid().ToString("N") + ext;
            var dest     = Path.Combine(ImageHelper.ImagesDirectory, fileName);

            File.Copy(dlg.FileName, dest, overwrite: true);

            // Store only the filename — path is resolved at runtime via ImageHelper
            FormImagePath = fileName;
        }
    }
}
