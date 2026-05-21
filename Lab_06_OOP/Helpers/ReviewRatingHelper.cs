using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Confectionery.Models;
using Confectionery.Services;

namespace Confectionery.Helpers
{

    public static class ReviewRatingHelper
    {
        public static IList<Review> AsReviewList(object value)
        {
            if (value == null) return new List<Review>();
            if (value is Product product) return WithValidRating(product.Reviews);
            if (value is IEnumerable<Review> typed) return typed.ToList();
            if (value is IEnumerable enumerable && !(value is string))
                return enumerable.OfType<Review>().ToList();
            return new List<Review>();
        }

        public static IList<Review> WithValidRating(IEnumerable<Review> reviews)
        {
            if (reviews == null) return new List<Review>();
            return reviews.Where(r => r.Rating >= 1 && r.Rating <= 5).ToList();
        }

        public static string NoRatingText()
            => Application.Current?.TryFindResource("Showcase_RatingNone") as string
               ?? (LanguageService.IsEnglish ? "n/a" : "—");

        public static double? GetAverage(IEnumerable<Review> reviews)
        {
            var list = WithValidRating(reviews);
            return list.Any() ? list.Average(r => r.Rating) : (double?)null;
        }


        public static string FormatCompact(IEnumerable<Review> reviews)
        {
            var avg = GetAverage(reviews);
            return avg.HasValue ? $"{avg.Value:F1}" : NoRatingText();
        }

        public static string FormatCompact(object value)
            => FormatCompact(AsReviewList(value));

        public static string FormatDetailed(IEnumerable<Review> reviews, string noRatingsText = null)
        {
            if (noRatingsText == null)
                noRatingsText = LanguageService.IsEnglish ? "No ratings yet" : "Нет оценок";
            var list = WithValidRating(reviews);
            if (!list.Any()) return noRatingsText;
            var avg = list.Average(r => r.Rating);
            return $"★ {avg:F1} ({list.Count} отз.)";
        }

        public static string FormatSingleReview(int rating)
        {
            if (rating < 1 || rating > 5) return "—";
            return $"{rating} ⭐";
        }
    }

}
