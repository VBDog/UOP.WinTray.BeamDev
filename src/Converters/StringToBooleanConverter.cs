using System;
using System.Globalization;
using System.Windows.Data;

namespace UOP.WinTray.UI.Converters
{
    /// <summary>
    /// Converter to convert empty value and zero to false
    /// </summary>
    public class StringToBooleanConverter : IValueConverter
    {
        /// <summary>
        /// Convert method
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string text = System.Convert.ToString(value);
            if (string.IsNullOrEmpty(text) || text == "0")
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Convert back method
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return default;
        }
    }
}
