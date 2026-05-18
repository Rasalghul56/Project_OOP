using System.Windows;
using System.Windows.Controls;

namespace Lab_06_OOP.UserControls
{
    public partial class PackagingControl : UserControl
    {
        public static readonly DependencyProperty BoxCountProperty =
            DependencyProperty.Register(
                nameof(BoxCount),
                typeof(int),
                typeof(PackagingControl),
                new FrameworkPropertyMetadata(3, null, CoerceBoxCount),
                ValidateBoxCount);

        public static readonly DependencyProperty AvailableBoxesProperty =
            DependencyProperty.Register(
                nameof(AvailableBoxes),
                typeof(int),
                typeof(PackagingControl),
                new FrameworkPropertyMetadata(10, OnAvailableBoxesChanged),
                ValidateAvailableBoxes);

        public static readonly DependencyProperty RibbonCountProperty =
            DependencyProperty.Register(
                nameof(RibbonCount),
                typeof(int),
                typeof(PackagingControl),
                new FrameworkPropertyMetadata(1, null, CoerceRibbonCount),
                ValidateRibbonCount);

        public static readonly RoutedEvent PackagingConfirmedEvent =
            EventManager.RegisterRoutedEvent(
                nameof(PackagingConfirmed),
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(PackagingControl));

        public PackagingControl()
        {
            InitializeComponent();
        }

        public int BoxCount
        {
            get => (int)GetValue(BoxCountProperty);
            set => SetValue(BoxCountProperty, value);
        }

        public int AvailableBoxes
        {
            get => (int)GetValue(AvailableBoxesProperty);
            set => SetValue(AvailableBoxesProperty, value);
        }

        public int RibbonCount
        {
            get => (int)GetValue(RibbonCountProperty);
            set => SetValue(RibbonCountProperty, value);
        }

        public event RoutedEventHandler PackagingConfirmed
        {
            add => AddHandler(PackagingConfirmedEvent, value);
            remove => RemoveHandler(PackagingConfirmedEvent, value);
        }

        private static bool ValidateBoxCount(object value)
        {
            return value is int intValue && intValue >= 1 && intValue <= 500;
        }

        private static object CoerceBoxCount(DependencyObject d, object baseValue)
        {
            var control = (PackagingControl)d;
            var requested = (int)baseValue;
            return requested > control.AvailableBoxes ? control.AvailableBoxes : requested;
        }

        private static bool ValidateAvailableBoxes(object value)
        {
            return value is int intValue && intValue >= 1 && intValue <= 1000;
        }

        private static void OnAvailableBoxesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.CoerceValue(BoxCountProperty);
            d.CoerceValue(RibbonCountProperty);
        }

        private static bool ValidateRibbonCount(object value)
        {
            return value is int intValue && intValue >= 0 && intValue <= 500;
        }

        private static object CoerceRibbonCount(DependencyObject d, object baseValue)
        {
            var control = (PackagingControl)d;
            var requested = (int)baseValue;
            return requested > control.BoxCount ? control.BoxCount : requested;
        }

        private void ConfirmPackagingButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(PackagingConfirmedEvent, this));
        }
    }
}
