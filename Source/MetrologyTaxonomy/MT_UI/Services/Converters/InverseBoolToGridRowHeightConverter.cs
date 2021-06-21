using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace MT_UI.Services.Converters
{
    class InverseBoolToGridRowHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ((bool)value == false) ? new GridLength(1, GridUnitType.Star) : new GridLength(0, GridUnitType.Pixel);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw null;
        }
    }
}
