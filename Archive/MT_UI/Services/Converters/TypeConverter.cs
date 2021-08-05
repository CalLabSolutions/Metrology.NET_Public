using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace MT_UI.Services.Converters
{
    public enum Types
    {
        None,
        Measure,
        Source
    }

    public class TypeConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, string language)
        {
            return parameter.Equals(value);
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, string language)
        {
            return ((bool)value) == true ? parameter : DependencyProperty.UnsetValue;
        }
    }
}
