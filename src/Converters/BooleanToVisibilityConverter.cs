using System;
using System.Windows;
using System.Windows.Data;

namespace UOP.WinTray.UI.Converters
{
    /// <summary>
    /// Converts boolean value to Visible/collapsed and vice versa.
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// converts boolean to Visible/collapsed 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool negate = parameter is bool && (bool)parameter;
            if (value is bool && (bool)value)
            {
                return negate ? Visibility.Collapsed : Visibility.Visible;
            }
            return negate ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Converts Visible/Collapsed to boolean.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Visibility && (Visibility)value == Visibility.Visible)
            {
                return true;
            }
            return false;
        }

        public static Visibility ConvertBool(bool aVal)
        {
            return (!aVal) ? Visibility.Collapsed : Visibility.Visible;
        }

        public static bool ConvertVis(Visibility aVal)
        {
            return aVal == Visibility.Visible;
        }
    }

}
