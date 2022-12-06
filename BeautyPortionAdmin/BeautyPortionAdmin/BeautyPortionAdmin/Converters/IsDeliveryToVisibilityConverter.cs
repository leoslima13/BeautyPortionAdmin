using System;
using System.Globalization;
using BeautyPortionAdmin.Models;
using Xamarin.Forms;

namespace BeautyPortionAdmin.Converters
{
    public class IsDeliveryToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            var deliveryMode = (DeliveryMode)value;

            return deliveryMode == DeliveryMode.Delivery;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
