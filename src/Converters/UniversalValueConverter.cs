using System;
using System.ComponentModel;
using System.Windows.Data;

namespace UOP.WinTray.UI.Converters
{
    /// <summary>
    /// A value converter can obtain a suitable TypeConverter for the target property then perform the required conversion
    /// </summary>
    public class UniversalValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (null == value)
                return null ;

            // obtain the conveter for the target type
            TypeConverter converter = TypeDescriptor.GetConverter(targetType);

            try
            {
                // determine if the supplied value is of a suitable type
                if (converter.CanConvertFrom(value.GetType()))
                {
                    // return the converted value
                    return converter.ConvertFrom(value);
                }
                else
                {
                    // try to convert from the string representation
                    return converter.ConvertFrom(value.ToString());
                }
            }
            catch 
            {
                return value;
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
