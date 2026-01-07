using System;
using System.Windows.Data;

namespace UOP.WinTray.UI.Converters
{
    /// <summary>
    /// Radiobutton check to bool converter.
    /// </summary>
    public class RadioButtonCheckedConverter : IValueConverter

    {
        /// <summary>
        /// Convert text to bool.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter,

            System.Globalization.CultureInfo culture)
        {

            return value.Equals(parameter);
        }


        /// <summary>
        /// Convert back bool to check.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter,

            System.Globalization.CultureInfo culture)

        {

            return value.Equals(true) ? parameter : Binding.DoNothing;
        }
    }
}
