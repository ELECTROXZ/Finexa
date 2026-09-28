using Microsoft.UI.Xaml.Data;
using System;

namespace Finexa_App.Converters
{
    public class NegativeCurrencyFormatter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is decimal amount)
            {
                return amount == 0 ? "₹0.00" : $"-₹{amount:N2}";
            }
            if (value is double amt)
            {
                return amt == 0 ? "₹0.00" : $"-₹{amt:N2}";
            }
            return "₹0.00";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
