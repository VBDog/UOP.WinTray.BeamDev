using System;
using System.Globalization;
using System.Windows.Data;
using UOP.WinTray.Projects.Utilities;

namespace UOP.WinTray.UI.Converters
{
    /// <summary>
    /// Conver the double null value to emty string
    /// </summary>
    public class EditConstraintsDoubleToNullConveter : IValueConverter
    {
        #region Contants

        private const string NULL = "";

        #endregion

        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = mzUtils.VarToDouble( value);
            if (val == 0)
                return NULL;
            return val;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return 0;
            if(value.ToString()== NULL)
            {
                return 0;
            }
            return value;
        }

        #endregion
    }
}
