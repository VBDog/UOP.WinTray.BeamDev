using UOP.WinTray.Projects;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Parts;
using UOP.WinTray.Projects.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace UOP.WinTray.UI.Converters
{
    /// <summary>
    /// Converter to get property Value in selected unit. Input is properties list and unit family, parameter is Tag name(Property Name)
    /// </summary>
    public class TextToPropertyValueConverter : IMultiValueConverter
    {
        #region Constants

        private const string FEED_DENSITY = "FeedDensity";
        private const string FEED_VOLUME_RATE = "FeedVolumeRate";
        private const string NOZZLE_PIPE = "NOZZLEPIPE";
        private const string PIPE = "PIPE";
        private readonly Dictionary<string, string> dependantProperties = new() { { "LiquidDensity", "LiquidRate" }, { "VaporDensity", "VaporRate" }, { "LiquidDensityFromAbove", "LiquidFromAbove" }, { "VaporDensityFromBelow", "VaporFromBelow" } };
        private readonly HashSet<string> emptyPropName = new() { "DesignRate", "VaporPercentage", "FeedLiquidVolumeRate", "FeedLiquidPercentage", "FeedVaporVolumeRate", "FeedVaporPercentage", "FeedVolumeRate", "FeedDensity" };

        #endregion

        /// <summary>
        /// Gets value in selected unit for the tag send as parameter 
        /// </summary>
        /// <param name="values"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string propertyName = System.Convert.ToString(parameter);
            string returnValue = string.Empty;
            if (values != null && values.Length > 1 && parameter != null)
            {
                if (values[0] is uopProperties aProps)
                {
                    returnValue = GetValueFromProperties(values, aProps, propertyName);
                }
                else if (values[0] is mdDistributorCase distCase)
                {
                    returnValue = GetValueFromCase(values, distCase, propertyName);
                }
                else if (values[0] is mdDistributor caseOwner)
                {
                    returnValue = GetValueFromOwner(values, caseOwner, propertyName);
                }
                if (emptyPropName.Contains(propertyName) && string.IsNullOrEmpty(returnValue))
                {
                    returnValue = "0";
                }
            }
            return returnValue;
        }

        /// <summary>
        /// Convert back set value to default unit
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

        /// <summary>
        /// Get value from Property list
        /// </summary>
        /// <param name="values"></param>
        /// <param name="aProps"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private string GetValueFromProperties(object[] values, uopProperties aProps, string propertyName)
        {
            bool isTextBox = values.Length <= 2 || System.Convert.ToBoolean(values[2]);
            uppUnitFamilies unit = (uppUnitFamilies)values[1];
            uopProperty aProp = null;
            aProps.TryGet(propertyName, out aProp);
            if (CheckDependantPropertyNull(aProps, propertyName))
            {
                return "-";
            }
            if (aProp != null)
            {
                if (isTextBox)
                {
                    return aProp.UnitValueString(unit, false, aProp.Value < 0);
                }
                else
                {
                    return aProp.UnitValueString(unit, true, true, bIncludeThousandsSeps: true, bIncludeUnitString: true);
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Get value from Current Case
        /// </summary>
        /// <param name="values"></param>
        /// <param name="distCase"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private string GetValueFromCase(object[] values, mdDistributorCase distCase, string propertyName)
        {
            uppUnitFamilies unit = (uppUnitFamilies)values[1];
            if (propertyName == FEED_DENSITY)
            {
                return distCase.FeedDensity.UnitValueString( unit, true, true, bIncludeThousandsSeps: true, bIncludeUnitString: true);
            }
            else if (propertyName == FEED_VOLUME_RATE)
            {
                return distCase.FeedVolumeRate.UnitValueString(unit, true, true, bIncludeThousandsSeps: true, bIncludeUnitString: true);
            }
            return string.Empty;
        }

        /// <summary>
        /// Get Value from Current Owner
        /// </summary>
        /// <param name="values"></param>
        /// <param name="distributor"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private string GetValueFromOwner(object[] values, mdDistributor distributor, string propertyName)
        {
            uopProperties aProps = distributor.CurrentProperties();
            uopProperty aProp = aProps.Item(propertyName);

            uppUnitFamilies unit = (uppUnitFamilies)values[1];
            bool isTextBox = values.Length <= 2 || System.Convert.ToBoolean(values[2]);
            if (aProp != null)
            {
                if (isTextBox)
                {
                    if (propertyName.ToUpper() == NOZZLE_PIPE)
                    {
                        uopPipe oNozzlePipe = distributor.NozzleObject;
                        return oNozzlePipe?.IDDescriptor;
                    }
                    else if (propertyName.ToUpper() == PIPE)
                    {
                        uopPipe oPipe = distributor.PipeObject;
                        return oPipe?.IDDescriptor;
                    }
                    bool bDefaultAsNullString = mzUtils.IsNumeric(aProp.Value) && System.Convert.ToDouble(aProp.Value) < 0;
                    return aProp.UnitValueString(unit, false, bDefaultAsNullString);
                }
                else
                {
                    return "";
                }
            }

            return null;
        }

        /// <summary>
        /// return true if value is 0 or null
        /// </summary>
        /// <param name="aProps"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private bool CheckDependantPropertyNull(uopProperties aProps, string propertyName)
        {
            if (dependantProperties.ContainsKey(propertyName) && aProps != null)
            {
                return aProps.HasMember(propertyName);

                //uopProperty dependantProperty = aProps.Item(dependantProperties[propertyName]);
                //if (dependantProperty != null && (dependantProperty.Value == 0 || dependantProperty.Value == dependantProperty.NullValue))
                //{
                //    return true;
                //}
            }
            return false;
        }
    }
}
