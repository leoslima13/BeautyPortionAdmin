using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Xamarin.Forms;

namespace BeautyPortionAdmin.Converters
{
    public class PriceConverter : IValueConverter
    {
        private readonly string _format = "{0:R$ ###,###,##0.00}";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            return string.Format(_format, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            string valueFromString = Regex.Replace(value.ToString(), @"\D", "");

            if (valueFromString.Length <= 0)
                return 0m;

            if (!long.TryParse(valueFromString, out long valueLong))
                return 0m;

            if (valueLong <= 0)
                return 0m;

            return valueLong / 100m;
        }
    }
}
