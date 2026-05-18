using System.Windows;
using System.Windows.Controls;

namespace Lab_06_OOP.UserControls
{
    public partial class BatchPreparationControl : UserControl
    {
        public static readonly DependencyProperty BatchSizeProperty =
            DependencyProperty.Register(
                nameof(BatchSize),
                typeof(int),
                typeof(BatchPreparationControl),
                new FrameworkPropertyMetadata(8, null, CoerceBatchSize),
                ValidateBatchSize);

        public static readonly DependencyProperty MaxBatchSizeProperty =
            DependencyProperty.Register(
                nameof(MaxBatchSize),
                typeof(int),
                typeof(BatchPreparationControl),
                new FrameworkPropertyMetadata(20, OnMaxBatchSizeChanged),
                ValidateMaxBatchSize);

        public static readonly DependencyProperty DietModeProperty =
            DependencyProperty.Register(
                nameof(DietMode),
                typeof(bool),
                typeof(BatchPreparationControl),
                new PropertyMetadata(false));

        public static readonly RoutedEvent TasteCheckEvent =
            EventManager.RegisterRoutedEvent(
                nameof(TasteCheck),
                RoutingStrategy.Direct,
                typeof(RoutedEventHandler),
                typeof(BatchPreparationControl));

        public static readonly RoutedEvent PreviewBatchPreparedEvent =
            EventManager.RegisterRoutedEvent(
                nameof(PreviewBatchPrepared),
                RoutingStrategy.Tunnel,
                typeof(RoutedEventHandler),
                typeof(BatchPreparationControl));

        public static readonly RoutedEvent BatchPreparedEvent =
            EventManager.RegisterRoutedEvent(
                nameof(BatchPrepared),
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(BatchPreparationControl));

        public BatchPreparationControl()
        {
            InitializeComponent();
        }

        public int BatchSize
        {
            get => (int)GetValue(BatchSizeProperty);
            set => SetValue(BatchSizeProperty, value);
        }

        public int MaxBatchSize
        {
            get => (int)GetValue(MaxBatchSizeProperty);
            set => SetValue(MaxBatchSizeProperty, value);
        }

        public bool DietMode
        {
            get => (bool)GetValue(DietModeProperty);
            set => SetValue(DietModeProperty, value);
        }

        public event RoutedEventHandler TasteCheck
        {
            add => AddHandler(TasteCheckEvent, value);
            remove => RemoveHandler(TasteCheckEvent, value);
        }

        public event RoutedEventHandler PreviewBatchPrepared
        {
            add => AddHandler(PreviewBatchPreparedEvent, value);
            remove => RemoveHandler(PreviewBatchPreparedEvent, value);
        }

        public event RoutedEventHandler BatchPrepared
        {
            add => AddHandler(BatchPreparedEvent, value);
            remove => RemoveHandler(BatchPreparedEvent, value);
        }

        private static bool ValidateBatchSize(object value)
        {
            return value is int intValue && intValue >= 1 && intValue <= 200;
        }

        private static object CoerceBatchSize(DependencyObject d, object baseValue)
        {
            var control = (BatchPreparationControl)d;
            var requested = (int)baseValue;
            return requested > control.MaxBatchSize ? control.MaxBatchSize : requested;
        }

        private static bool ValidateMaxBatchSize(object value)
        {
            return value is int intValue && intValue >= 1 && intValue <= 200;
        }

        private static void OnMaxBatchSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.CoerceValue(BatchSizeProperty);
        }

        private void TasteCheckButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(TasteCheckEvent, this));
        }

        private void PrepareBatchButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(PreviewBatchPreparedEvent, this));
            RaiseEvent(new RoutedEventArgs(BatchPreparedEvent, this));
        }
    }
}
