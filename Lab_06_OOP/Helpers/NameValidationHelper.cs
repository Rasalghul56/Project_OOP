using System.Text.RegularExpressions;
using System.Windows;

namespace Confectionery.Helpers
{
    public static class NameValidationHelper
    {
        private const int MinLength = 2;
        private const int MaxLength = 100;


        private static readonly Regex NameRegex = new Regex(
            @"^[\p{L}]+(?:[\s\-'][\p{L}]+)*$",
            RegexOptions.Compiled);

        public static bool IsValid(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            var trimmed = name.Trim();
            if (trimmed.Length < MinLength || trimmed.Length > MaxLength) return false;
            return NameRegex.IsMatch(trimmed);
        }

        public static string GetErrorMessage()
            => Application.Current?.TryFindResource("Validation_NameFormat") as string
               ?? "Имя: от 2 до 100 символов, только буквы, пробел, дефис и апостроф.";
    }
}
