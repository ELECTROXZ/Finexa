using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;

namespace Finexa_App.Converters
{
    public class PaymentStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string status = value?.ToString() ?? "Unpaid";
            return status switch
            {
                "Paid" => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 76, 175, 80)),      // Green
                "Partial" => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 152, 0)),   // Orange
                "Unpaid" => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 244, 67, 54)),    // Red
                _ => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 158, 158, 158))         // Grey
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
