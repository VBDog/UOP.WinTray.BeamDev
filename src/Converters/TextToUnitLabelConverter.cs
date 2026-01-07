using System;
using System.Globalization;
using System.Windows.Data;
using UOP.WinTray.Projects;
using UOP.WinTray.Projects.Enums;

namespace UOP.WinTray.UI.Converters
{
    /// <summary>
    /// Converter to get Unit label for the selected unit, send as parameter
    /// </summary>
    public class TextToUnitLabelConverter : IMultiValueConverter
    {
        /// <summary>
        /// Converter to get unit label of the property 
        /// </summary>
        /// <param name="values">0 - uopProperties, 1 - uppUnitFamilies</param>
        /// <param name="targetType"></param>
        /// <param name="parameter">Name of the property</param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values.Length > 1 && parameter != null)
            {
                var unit = (uppUnitFamilies)values[1];
                if (values[0] is uopProperties aProps)
                {
                    string propertyName = System.Convert.ToString(parameter);
                    
                    uopProperty aProp = null;
                    if (aProps.TryGet(propertyName,out aProp))
                    {
                        return aProp.UnitString(unit);
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Convert back for Text To Unit Label Converter 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetTypes"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return default;
        }
    }
}
