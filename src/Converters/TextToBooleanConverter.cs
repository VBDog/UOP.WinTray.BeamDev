using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace UOP.WinTray.UI.Converters
{
    public class TextToBooleanConverter : IValueConverter
    {
        /// <summary>
        /// Convert text to visibility
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(System.Convert.ToString(value)))
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        /// <summary>
        /// Convert back visibility to text
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility && (Visibility)value == Visibility.Visible ? "true" : "false";
        }
    }
}
