using System;
using System.Windows.Data;
using System.Windows.Media;

namespace UOP.WinTray.UI.Converters
{
    /// <summary>
    /// Type to convert string representation of color into instance of SolidColorBrush for the give color string
    /// </summary>
    public class TextToColorConverter : IValueConverter
    {
        #region Constants

        private const string RED = "RED";
        private const string BLUE = "BLUE";
        private const string LIME = "LIME";

        #endregion

        /// <summary>
        /// Converts string value of color into SolidColorBrush type
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (null != value)
            {
                string text = value.ToString().ToUpper();
                switch (text)
                {
                    case RED:
                        return new SolidColorBrush(Colors.Red);
                    case BLUE:
                        return new SolidColorBrush(Colors.Blue);
                    case LIME:
                        return new SolidColorBrush(Colors.Lime);
                    default:
                        return new SolidColorBrush(Colors.Black);
                }
            }

            return Colors.Black;
        }

        /// <summary>
        /// It is not implemented as reverse conversion is not needed.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

}
