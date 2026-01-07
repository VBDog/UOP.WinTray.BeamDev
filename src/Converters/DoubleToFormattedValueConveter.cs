using System;
using System.Globalization;
using System.Windows.Data;

namespace UOP.WinTray.UI.Converters
{
    /// <summary>
    /// Conver the double 0 value to formatted string
    /// </summary>
    public class DoubleToFormattedValueConveter : IValueConverter
    {
        #region Contants

        private const string FORMAT = "";

        #endregion

        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (double)value;
            if (val == 0)
                return FORMAT;
            return val;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value.ToString() == FORMAT)
            {
                return 0;
            }
            return value;
        }

        #endregion
    }
}
