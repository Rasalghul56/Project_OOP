using System.Windows;
using Lab_06_OOP.ViewModels;

namespace ConfectioneryShop.Views
{
    public partial class DbTablesAdminWindow : Window
    {
        private readonly DbAdminViewModel _vm;

        public DbTablesAdminWindow(DbAdminViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = _vm;
            Closed += (_, __) => _vm.OnWindowClosed();
        }

        private void TabCategories_Click(object sender, RoutedEventArgs e)
        {
            _vm.SelectedManufacturerRow = null;
            _vm.SelectedCategoryRow = null;
            MainGrid.SelectedItem = null;
            _vm.ActiveTable = 0;
            _vm.NotifySelectionCommands();
        }

        private void TabManufacturers_Click(object sender, RoutedEventArgs e)
        {
            _vm.SelectedCategoryRow = null;
            _vm.SelectedManufacturerRow = null;
            MainGrid.SelectedItem = null;
            _vm.ActiveTable = 1;
            _vm.NotifySelectionCommands();
        }

        private void TabProducts_Click(object sender, RoutedEventArgs e)
        {
            _vm.SelectedCategoryRow = null;
            _vm.SelectedManufacturerRow = null;
            MainGrid.SelectedItem = null;
            _vm.ActiveTable = 2;
            _vm.NotifySelectionCommands();
        }

        private void MainGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (_vm == null) return;
            var row = MainGrid.SelectedItem as System.Data.DataRowView;
            _vm.SelectedCategoryRow = _vm.ActiveTable == 0 ? row : null;
            _vm.SelectedManufacturerRow = _vm.ActiveTable == 1 ? row : null;
            _vm.NotifySelectionCommands();
        }
    }
}
