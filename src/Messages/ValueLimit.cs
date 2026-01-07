using System;
using UOP.WinTray.Projects;
using UOP.WinTray.Projects.Enums;

namespace UOP.WinTray.UI.Messages
{
    /// <summary>
    /// Invoked to update the main form project Tree
    /// </summary>
    public class ValueLimit
    {
        public ValueLimit() { }
        public ValueLimit(string aPropertyName, object aMin = null, object aMax = null, string aMinSuffix = null, string aMaxSuffix = null, bool bAllowZero = false, bool bAbsValue = false)
        {
            PropertyName = aPropertyName;
            Max = aMax; Min = aMin; MaxSuffix = aMaxSuffix; MinSuffix = aMinSuffix; AllowZero = bAllowZero; AbsValue = bAbsValue;
        }
        public string PropertyName { get; set; }

        private object _Max;
        public object Max { get => _Max; set => _Max = value; }
        private object _Min;
        public object Min { get => _Min; set => _Min = value; }
        public string MaxSuffix { get; set; }
        public string MinSuffix { get; set; }
        public bool AllowZero { get; set; }
        public bool AbsValue { get; set; }
        public string ValidateProperty(uopProperty prop, uopUnit aUnits = null, uppUnitFamilies aDisplayUnits = uppUnitFamilies.Undefined)
        {
            string result = null;
            if (prop == null) return result;
            double dval;
            int ival;
            double pdval;
            int pival;
            string limval;
            if (aUnits == null) aUnits = prop.Units;

            if (aDisplayUnits != uppUnitFamilies.Metric && aDisplayUnits != uppUnitFamilies.English) aDisplayUnits = uppUnitFamilies.English;
            if (_Min != null)
            {
                if (_Min is double)
                {
                    dval = (double)_Min;
                    pdval = prop.ValueD;
                    if (AbsValue) pdval = Math.Abs(pdval);
                    bool doit = true;
                    if (!AllowZero && pdval == 0)
                    {
                        limval = (aUnits != null) ? aUnits.UnitValueString(0, aDisplayUnits) : dval.ToString("0.0###");
                        result = $"{prop.DisplayName} Must Be Greater Than {limval}";

                    }
                    else
                    {
                        if (AllowZero && pdval == 0) doit = false;
                        if (pdval < dval && doit)
                        {
                            limval = (aUnits != null) ? aUnits.UnitValueString(dval, aDisplayUnits) : dval.ToString("0.0###");
                            result = $"{prop.DisplayName} Must Be Greater Than or Equal To {limval}";



                        }

                    }

                }
                else if (_Min is int)
                {

                    ival = (int)_Min;
                    pival = prop.ValueI;
                    if (AbsValue) pival = Math.Abs(pival);
                    if (!AllowZero && pival == 0)
                    {
                        limval = (aUnits != null) ? aUnits.UnitValueString(0, aDisplayUnits) : "0";
                        result = $"{prop.DisplayName} Must Be Greater Than {limval}";

                    }
                   
                    if (pival < ival)
                    {

                        result = $"{prop.DisplayName} Must Be Greater Than or Equal To {ival}";
                    }
                }
                if (result != null)
                {
                    if (!string.IsNullOrWhiteSpace(MinSuffix)) result += MinSuffix;
                    return result;
                }
            }

            if (_Max != null)
            {
                if (_Max is double)
                {
                    dval = (double)_Max;
                    pdval = prop.ValueD;
                    if (AbsValue) pdval = Math.Abs(pdval);

                    if (pdval > dval)
                    {
                        limval = (aUnits != null) ? aUnits.UnitValueString(dval, aDisplayUnits) : dval.ToString("0.0###");



                        result = $"{prop.DisplayName} Must Be Less Than or Equal To {limval}";
                    }
                }
                else if (_Max is int)
                {
                    ival = (int)_Max;
                    pival = prop.ValueI;
                    if (AbsValue) pival = Math.Abs(pival);
                    if (pival > ival)
                    {
                        result = $"{prop.DisplayName} Must Be Less Than or Equal To {ival}";
                    }
                }
                if (result != null)
                {
                    if (!string.IsNullOrWhiteSpace(MaxSuffix)) result += MaxSuffix;
                    return result;
                }
            }

            return result;
        }
    }
}
