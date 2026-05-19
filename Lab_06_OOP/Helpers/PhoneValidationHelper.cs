using System.Text.RegularExpressions;
using System.Windows;

namespace Confectionery.Helpers
{
    public static class PhoneValidationHelper
    {
        private static readonly Regex PhoneRegex = new Regex(
            @"^\+375(29|44|33)\d{7}$",
            RegexOptions.Compiled);

        public static bool IsValid(string phone)
            => !string.IsNullOrWhiteSpace(phone) && PhoneRegex.IsMatch(phone.Trim());

        public static string GetErrorMessage()
            => Application.Current?.TryFindResource("Validation_Phone") as string
               ?? "Телефон должен быть в формате +375(29|33|44)XXXXXXX";
    }
}
