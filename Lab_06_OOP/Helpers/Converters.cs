using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Confectionery.Models;
using Confectionery.Services;

namespace Confectionery.Helpers
{
    public class OrderStatusToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is OrderStatus)) return value?.ToString();
            switch ((OrderStatus)value)
            {
                case OrderStatus.Accepted: return "Принят";
                case OrderStatus.Preparing: return "Готовится";
                case OrderStatus.Ready: return "Готов";
                case OrderStatus.Completed: return "Выполнен";
                default: return value.ToString();
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class OrderStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is OrderStatus)) return Brushes.Gray;
            switch ((OrderStatus)value)
            {
                case OrderStatus.Accepted: return new SolidColorBrush(Color.FromRgb(255, 167, 38));
                case OrderStatus.Preparing: return new SolidColorBrush(Color.FromRgb(41, 182, 246));
                case OrderStatus.Ready: return new SolidColorBrush(Color.FromRgb(102, 187, 106));
                case OrderStatus.Completed: return new SolidColorBrush(Color.FromRgb(120, 144, 156));
                default: return Brushes.Gray;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class DeliveryTypeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is DeliveryType)) return value?.ToString();
            return (DeliveryType)value == DeliveryType.Pickup ? "Самовывоз" : "Доставка";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (value is bool b && b) ? Visibility.Visible : Visibility.Collapsed;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (value is bool b && b) ? Visibility.Collapsed : Visibility.Visible;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value != null ? Visibility.Visible : Visibility.Collapsed;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class RatingToStarsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Может прийти int или ICollection<Review>
            if (value is int rating)
                return $"{rating:F1}";
            if (value is IEnumerable<Review> reviews)
            {
                var list = reviews.ToList();
                return list.Any() ? $"{list.Average(r => r.Rating):F1}" : "—";
            }
            return "—";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>Конвертирует коллекцию Reviews в среднее значение рейтинга (double).</summary>
    /// <summary>Переводит название категории при активном английском языке.</summary>
    public class CategoryNameConverter : IValueConverter
    {
        private static readonly Dictionary<string, string> RuToEn = new Dictionary<string, string>(
            System.StringComparer.OrdinalIgnoreCase)
        {
            {"Торты",        "Cakes"},
            {"Пирожные",     "Pastries"},
            {"Печенье",      "Cookies"},
            {"Конфеты",      "Candies"},
            {"Напитки",      "Drinks"},
            {"Шоколад",      "Chocolate"},
            {"Зефир",        "Marshmallow"},
            {"Все категории","All categories"}
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!LanguageService.IsEnglish) return value;
            if (value is string s && RuToEn.TryGetValue(s, out var en)) return en;
            return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class ReviewsAverageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable<Review> reviews)
            {
                var list = reviews.ToList();
                return list.Any() ? list.Average(r => r.Rating) : 0.0;
            }
            return 0.0;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class ImagePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var path = value as string;
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return null;
            try
            {
                var img = new BitmapImage();
                img.BeginInit();
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.UriSource = new Uri(path, UriKind.Absolute);
                img.EndInit();
                return img;
            }
            catch
            {
                return null;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class PriceFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal d) return $"{d:F2} Br";
            if (value is double dbl) return $"{dbl:F2} Br";
            return value?.ToString();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>Используется для привязки IsChecked RadioButton к целочисленному рейтингу.</summary>
    public class RatingEqualConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int v && parameter is string s && int.TryParse(s, out var p))
                return v == p;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b && parameter is string s && int.TryParse(s, out var p))
                return p;
            return System.Windows.DependencyProperty.UnsetValue;
        }
    }

    public class PaymentMethodToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Models.PaymentMethod)) return value?.ToString();
            return (Models.PaymentMethod)value == Models.PaymentMethod.Cash
                ? "Наличными при получении"
                : "Картой при получении";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
