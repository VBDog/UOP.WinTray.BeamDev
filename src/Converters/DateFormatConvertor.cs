using System;
using System.Globalization;
using System.Windows.Data;

namespace UOP.WinTray.UI
{
    /// <summary>
    /// format date in ddMMMyy format.
    /// </summary>
    public class DateFormatConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return DateTime.Now.ToString("ddMMMyy").ToUpper();
            }
            else if (value is DateTime)
            {
                DateTime tempValue = (DateTime)value;
                return tempValue.ToString("ddMMMyy", System.Globalization.CultureInfo.InvariantCulture).ToUpper();
            }
            else
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // value will always be in DateTime format
            DateTime tempValue = (DateTime)value;
            return tempValue.ToString("ddMMMyy", System.Globalization.CultureInfo.InvariantCulture).ToUpper();
        }
    }
}
