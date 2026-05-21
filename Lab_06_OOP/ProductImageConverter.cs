using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using ConfectioneryShop.Models;

namespace Lab_06_OOP
{


    public class ProductImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Product p)
            {
                if (p.PhotoData != null && p.PhotoData.Length > 0)
                {
                    try
                    {
                        var img = new BitmapImage();
                        img.BeginInit();
                        img.CacheOption = BitmapCacheOption.OnLoad;
                        img.StreamSource = new MemoryStream(p.PhotoData);
                        img.EndInit();
                        img.Freeze();
                        return img;
                    }
                    catch
                    {

                    }
                }

                if (!string.IsNullOrWhiteSpace(p.ImagePath))
                {
                    var s = p.ImagePath.Trim();
                    try
                    {
                        if (s.StartsWith("pack:", StringComparison.OrdinalIgnoreCase) ||
                            s.Contains(":/") || s.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
                        {
                            var bmp = new BitmapImage();
                            bmp.BeginInit();
                            bmp.UriSource = new Uri(s, UriKind.Absolute);
                            bmp.CacheOption = BitmapCacheOption.OnLoad;
                            bmp.EndInit();
                            bmp.Freeze();
                            return bmp;
                        }

                        try
                        {
                            var rel = new BitmapImage();
                            rel.BeginInit();
                            rel.UriSource = new Uri(s, UriKind.Relative);
                            rel.CacheOption = BitmapCacheOption.OnLoad;
                            rel.EndInit();
                            rel.Freeze();
                            return rel;
                        }
                        catch
                        {

                        }

                        var disk = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, s);
                        if (File.Exists(disk))
                        {
                            var bmp = new BitmapImage();
                            bmp.BeginInit();
                            bmp.UriSource = new Uri(disk, UriKind.Absolute);
                            bmp.CacheOption = BitmapCacheOption.OnLoad;
                            bmp.EndInit();
                            bmp.Freeze();
                            return bmp;
                        }
                    }
                    catch
                    {

                    }
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
