using System;
using System.Linq;
using System.Windows;

namespace Confectionery.Services
{
    public enum AppTheme { Light, Warm, Dark }

    public static class ThemeService
    {
        private const string LightUri = "/Lab_06_OOP;component/Resources/Themes/LightTheme.xaml";
        private const string WarmUri  = "/Lab_06_OOP;component/Resources/Themes/WarmTheme.xaml";
        private const string DarkUri  = "/Lab_06_OOP;component/Resources/Themes/DarkTheme.xaml";

        public static AppTheme Current { get; private set; } = AppTheme.Light;

        public static event Action ThemeChanged;

        /// <summary>Циклично: Light → Warm → Dark → Light → ...</summary>
        public static void Toggle()
        {
            Current = Current == AppTheme.Light  ? AppTheme.Warm
                    : Current == AppTheme.Warm   ? AppTheme.Dark
                    :                              AppTheme.Light;
            Apply();
            ThemeChanged?.Invoke();
        }

        private static void Apply()
        {
            var uri = Current == AppTheme.Light ? LightUri
                    : Current == AppTheme.Warm  ? WarmUri
                    :                             DarkUri;

            var dicts = Application.Current.Resources.MergedDictionaries;
            var old = dicts.FirstOrDefault(d =>
                d.Source != null && (
                    d.Source.OriginalString.Contains("LightTheme") ||
                    d.Source.OriginalString.Contains("WarmTheme")  ||
                    d.Source.OriginalString.Contains("DarkTheme")));

            if (old != null) dicts.Remove(old);
            dicts.Add(new ResourceDictionary { Source = new Uri(uri, UriKind.Relative) });
        }
    }
}
