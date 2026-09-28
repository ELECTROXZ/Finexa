using Microsoft.UI.Xaml.Data;
using System;

namespace Finexa_App.Converters
{
    public class DecimalToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is decimal dec)
            {
                return (double)dec;
            }
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is double dbl)
            {
                return (decimal)dbl;
            }
            if (value is float flt)
            {
                return (decimal)flt;
            }
            if (value is int val)
            {
                return (decimal)val;
            }
            return 0.0m;
        }
    }
}
