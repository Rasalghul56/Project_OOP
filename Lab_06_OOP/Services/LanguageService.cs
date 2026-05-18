using System;
using System.Linq;
using System.Windows;

namespace Confectionery.Services
{
    public static class LanguageService
    {
        private const string RuUri = "/Lab_06_OOP;component/Resources/Strings.ru.xaml";
        private const string EnUri = "/Lab_06_OOP;component/Resources/Strings.en.xaml";

        public static bool IsEnglish { get; private set; }

        /// <summary>Вызывается после смены языка — ViewModels могут подписаться.</summary>
        public static event Action LanguageChanged;

        public static void Toggle()
        {
            IsEnglish = !IsEnglish;
            Apply(IsEnglish ? EnUri : RuUri);
            LanguageChanged?.Invoke();
        }

        private static void Apply(string relativeUri)
        {
            var dicts = Application.Current.Resources.MergedDictionaries;
            var old = dicts.FirstOrDefault(d =>
                d.Source != null &&
                (d.Source.OriginalString.Contains("Strings.ru") ||
                 d.Source.OriginalString.Contains("Strings.en")));

            if (old != null) dicts.Remove(old);
            dicts.Add(new ResourceDictionary
            {
                Source = new Uri(relativeUri, UriKind.Relative)
            });
        }
    }
}
