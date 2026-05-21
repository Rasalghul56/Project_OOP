using Lab_06_OOP;
using Lab_06_OOP.Commands;
using Lab_06_OOP.UserControls;
using Lab_06_OOP.ViewModels;
using ConfectioneryShop.Views;
using System.Windows;
using System.Windows.Input;

namespace ConfectioneryShop
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;

            AddHandler(BatchPreparationControl.PreviewBatchPreparedEvent, new RoutedEventHandler(Window_PreviewBatchPrepared));
            AddHandler(BatchPreparationControl.BatchPreparedEvent, new RoutedEventHandler(Window_BatchPrepared));
            AddHandler(PackagingControl.PackagingConfirmedEvent, new RoutedEventHandler(Window_PackagingConfirmed));
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            string body = App.T("S_About_Version") + "\n" + App.T("S_About_Theme");
            MessageBox.Show(body, App.T("S_AboutCaption"), MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SetRussianLanguage(object sender, RoutedEventArgs e)
        {
            App.ApplyLanguage("ru");
            _viewModel.SelectedLanguage = "ru";
        }

        private void SetEnglishLanguage(object sender, RoutedEventArgs e)
        {
            App.ApplyLanguage("en");
            _viewModel.SelectedLanguage = "en";
        }

        private void SetRoseTheme(object sender, RoutedEventArgs e)
        {
            App.ApplyTheme("rose");
            _viewModel.SelectedTheme = "rose";
        }

        private void SetGrayTheme(object sender, RoutedEventArgs e)
        {
            App.ApplyTheme("gray");
            _viewModel.SelectedTheme = "gray";
        }

        private void OpenDbAdmin_Click(object sender, RoutedEventArgs e)
        {
            if (!_viewModel.IsAdmin)
            {
                MessageBox.Show("Раздел доступен только администратору.", "База данных",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var vm = new DbAdminViewModel(() => _viewModel.ReloadFromDatabase());
            var w = new DbTablesAdminWindow(vm) { Owner = this };
            w.ShowDialog();
        }

        private void CreateTastingSet_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _viewModel != null
                           && _viewModel.SelectedProduct != null
                           && BatchControl != null
                           && BatchControl.BatchSize > 0
                           && _viewModel.SelectedProduct.Quantity >= BatchControl.BatchSize;
        }

        private void CreateTastingSet_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var product = _viewModel.SelectedProduct;
            if (product == null)
                return;

            product.Quantity -= BatchControl.BatchSize;
            product.IsOutOfStock = product.Quantity <= 0;
            _viewModel.PersistProductStock(product);
            _viewModel.RefreshCommand.Execute(null);
            LogRouteStep($"[Command] Создан набор из \"{product.ShortName}\". Списано: {BatchControl.BatchSize} шт.");
        }

        private void BatchControl_TasteCheck(object sender, RoutedEventArgs e)
        {
            LogRouteStep("[Control] Direct: проверка вкуса выполнена.");
        }

        private void BatchControl_PreviewBatchPrepared(object sender, RoutedEventArgs e)
        {
            LogRouteStep("[Control] Tunneling: событие дошло до источника.");
        }

        private void BatchControl_BatchPrepared(object sender, RoutedEventArgs e)
        {
            LogRouteStep("[Control] Bubbling: событие поднялось от источника.");
        }

        private void PackagingControl_PackagingConfirmed(object sender, RoutedEventArgs e)
        {
            LogRouteStep("[Control] Bubble: упаковка подтверждена.");
        }

        private void Window_PreviewBatchPrepared(object sender, RoutedEventArgs e)
        {
            LogRouteStep("[Window] Tunneling: сначала обработано окном.");
        }

        private void Window_BatchPrepared(object sender, RoutedEventArgs e)
        {
            LogRouteStep("[Window] Bubbling: потом дошло до окна.");
        }

        private void Window_PackagingConfirmed(object sender, RoutedEventArgs e)
        {
            LogRouteStep("[Window] Bubbling: событие упаковки всплыло к окну.");
        }

        private void LogRouteStep(string text)
        {
            RoutingLogList.Items.Insert(0, text);
            while (RoutingLogList.Items.Count > 20)
                RoutingLogList.Items.RemoveAt(RoutingLogList.Items.Count - 1);
        }
    }
}
