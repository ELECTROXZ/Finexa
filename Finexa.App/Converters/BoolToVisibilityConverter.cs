using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace Finexa_App.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool val = value is bool b && b;
            
            // support inverse conversion if parameter is "Inverse"
            if (parameter is string p && p == "Inverse")
            {
                val = !val;
            }

            return val ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value is Visibility vis && vis == Visibility.Visible;
        }
    }
}
