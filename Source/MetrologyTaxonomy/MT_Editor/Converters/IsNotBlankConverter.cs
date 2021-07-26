using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MT_Editor.Converters
{
    class IsNotBlankConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((string)value == "")
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
