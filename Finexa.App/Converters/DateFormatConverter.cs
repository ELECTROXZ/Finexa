using Microsoft.UI.Xaml.Data;
using System;

namespace Finexa_App.Converters
{
    public class DateFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime dt)
            {
                return dt.ToString("dd-MMM-yyyy");
            }
            if (value is DateTimeOffset dto)
            {
                return dto.ToString("dd-MMM-yyyy");
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
