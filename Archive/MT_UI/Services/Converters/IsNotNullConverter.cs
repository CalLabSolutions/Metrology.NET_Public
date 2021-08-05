using System;
using Windows.UI.Xaml.Data;

namespace MT_UI.Services.Converters
{
    class IsNotNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value != null);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
