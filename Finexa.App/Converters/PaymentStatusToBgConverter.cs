using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;

namespace Finexa_App.Converters
{
    public class PaymentStatusToBgConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string status = value?.ToString() ?? "Unpaid";
            return status switch
            {
                "Paid" => new SolidColorBrush(Windows.UI.Color.FromArgb(40, 76, 175, 80)),
                "Partial" => new SolidColorBrush(Windows.UI.Color.FromArgb(40, 255, 152, 0)),
                "Unpaid" => new SolidColorBrush(Windows.UI.Color.FromArgb(40, 244, 67, 54)),
                _ => new SolidColorBrush(Windows.UI.Color.FromArgb(40, 158, 158, 158))
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
