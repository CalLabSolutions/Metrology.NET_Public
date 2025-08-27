using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Forms;

namespace MT_Editor.Converters
{
    internal class HexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string strValue && !strValue.StartsWith("&#x", StringComparison.OrdinalIgnoreCase))
            {
                string retValue = WebUtility.HtmlEncode(strValue);
                if (retValue.Length > 0 && retValue.Equals(strValue, StringComparison.OrdinalIgnoreCase) == false)
                {
                    retValue = "&" + retValue + ";";
                }
            }
            return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string strValue && strValue.StartsWith("&#x", StringComparison.OrdinalIgnoreCase))
            {
                return WebUtility.HtmlDecode(strValue);
            }
            return value;
        }
    }
}
