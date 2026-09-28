using Microsoft.UI.Xaml.Data;
using System;

namespace Finexa_App.Converters
{
    public class CurrencyFormatter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is decimal amount)
            {
                return $"₹{amount:N2}";
            }
            if (value is double amt)
            {
                return $"₹{amt:N2}";
            }
            return "₹0.00";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is string str)
            {
                string clean = str.Replace("₹", "").Replace(",", "").Trim();
                if (decimal.TryParse(clean, out decimal res))
                {
                    return res;
                }
            }
            return 0.00m;
        }
    }
}
