using System.Globalization;

namespace ServiceImplementation.Helpers
{
    public static class CurrencyFormatter
    {
        private static readonly CultureInfo EgyptianCulture = new CultureInfo("ar-EG");

        public static string ToEgp(this decimal amount)
        {
            // Example output: ٢٥٠٠ ج.م
            return amount.ToString("C0", EgyptianCulture);
        }

        public static string ToEgpRange(decimal min, decimal max)
        {
            if (min == max) return min.ToEgp();
            return $"{min.ToEgp()} - {max.ToEgp()}";
        }
    }
}
