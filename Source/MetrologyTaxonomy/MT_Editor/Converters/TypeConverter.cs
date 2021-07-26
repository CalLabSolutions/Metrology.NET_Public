using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace MT_Editor.Converters
{
    public enum Types
    {
        None,
        Measure,
        Source
    }

    internal class TypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter.Equals(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((bool)value) == true ? parameter : DependencyProperty.UnsetValue;
        }
    }
}