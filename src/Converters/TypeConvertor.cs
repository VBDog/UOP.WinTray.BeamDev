using System;
using System.Windows.Data;

namespace UOP.WinTray.UI
{
    public class TypeConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.GetType().Name;
        }

        public object ConvertBack(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception("Can't convert back");
        }
    }
}
