using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;

namespace Finexa_App.Converters
{
    public class BalanceColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is decimal amount)
            {
                if (amount > 0) // Receivable - Red
                {
                    return new SolidColorBrush(Windows.UI.Color.FromArgb(255, 244, 67, 54));
                }
                if (amount < 0) // Payable - Green
                {
                    return new SolidColorBrush(Windows.UI.Color.FromArgb(255, 76, 175, 80));
                }
            }
            // Zero / Neutral - Default / Grey
            return new SolidColorBrush(Windows.UI.Color.FromArgb(255, 158, 158, 158));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
