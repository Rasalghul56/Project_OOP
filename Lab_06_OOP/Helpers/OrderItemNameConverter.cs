using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Confectionery.Models;

namespace Confectionery.Helpers
{
    /// <summary>Отображаемое название позиции заказа (сохранённое имя или «товар удалён»).</summary>
    public class OrderItemNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is OrderItem oi)
            {
                if (!string.IsNullOrWhiteSpace(oi.ProductName))
                    return oi.ProductName;
                if (oi.Product != null && !string.IsNullOrWhiteSpace(oi.Product.Name))
                    return oi.Product.Name;
            }

            return Application.Current.TryFindResource("OrderItem_Deleted") as string
                   ?? "Товар удалён";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
