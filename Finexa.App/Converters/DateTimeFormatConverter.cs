using Microsoft.UI.Xaml.Data;
using System;

namespace Finexa_App.Converters
{
    public class DateTimeFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime dt)
            {
                return dt.ToString("dd-MMM-yyyy hh:mm tt");
            }
            if (value is DateTimeOffset dto)
            {
                return dto.ToString("dd-MMM-yyyy hh:mm tt");
            }
            return "N/A";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
