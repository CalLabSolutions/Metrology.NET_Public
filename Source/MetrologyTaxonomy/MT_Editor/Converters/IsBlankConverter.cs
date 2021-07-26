using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MT_Editor.Converters
{
    class IsBlankConverter : IValueConverter
    {
       public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((string)value == "")
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
