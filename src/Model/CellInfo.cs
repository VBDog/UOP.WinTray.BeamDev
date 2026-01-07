using System;
using System.Windows.Media;
using UOP.WinTray.Projects;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Tables;

namespace UOP.WinTray.UI.Model
{
    /// <summary>
    /// cell information class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CellInfo : BindingObject
    {

        #region Properties
        private string _CellValue;
        public string Value
        {
            get => _CellValue;
            set
            {
                if (_CellValue == null || !_CellValue.Equals(value))
                {
                    _CellValue = value;
                    NotifyPropertyChanged("Value");
                }
            }
        }

        private string _ForeColor;
        public string ForeColor
        {
            get => _ForeColor;
            set
            {
                if (_ForeColor != value)
                {
                    _ForeColor = value;
                    NotifyPropertyChanged("ForeColor");
                }
            }
        }

        private string _ColumnHeader;
        public string ColumnHeader
        {
            get => _ColumnHeader;
            set
            {
                if (_ColumnHeader != value)
                {
                    _ColumnHeader = value;
                    NotifyPropertyChanged("ColumnHeader");
                }
            }
        }

        #endregion Properties

        #region Constructors

        public CellInfo()
        {
            Value = "";
            ForeColor = "";
            ColumnHeader = "";
        }
        public CellInfo(string value, string aForeColor = "")
        {
            Value = value;
            ForeColor = aForeColor;
            ColumnHeader = "";
        }

        public CellInfo(uopProperty property, uppUnitFamilies aUnitFamily = uppUnitFamilies.Default,
            bool bZerosAsNullString = true, bool bDefaultAsNullString = false,
            int aOverridePrecision = -1, bool bIncludeThousandsSeps = true,
            bool bIncludeUnitString = false, string aFormatString = "",
            bool bYesNoForBool = false)
        {
            if(property == null)
            {
                Value = "";
                ForeColor = "Black";
                ColumnHeader = "";
            }
            else
            {
                if (property.HasUnits)
                    Value = property.UnitValue(aUnitFamily, bZerosAsNullString, bDefaultAsNullString, aOverridePrecision, bIncludeThousandsSeps, bIncludeUnitString, aFormatString, null, bYesNoForBool);
                else
                    Value = property.ValueS;
                ForeColor = property.BrushColorString;
          
                ColumnHeader = property.Name;

            }
  
        }
        public CellInfo(uopTableCell cell, uppUnitFamilies aUnitFamily = uppUnitFamilies.Default,
         bool bZerosAsNullString = false, bool bDefaultAsNullString = false,
         int aOverridePrecision = -1, bool bIncludeThousandsSeps = true,
         bool bIncludeUnitString = false, string aFormatString = "",
         bool bYesNoForBool = false)
        {
            if (cell == null)
            {
                Value = "";
                ForeColor = "Black";
                ColumnHeader = "";
            }
            else
            {
                Value = cell.Property.UnitValue(aUnitFamily, bZerosAsNullString, bDefaultAsNullString, aOverridePrecision, bIncludeThousandsSeps, bIncludeUnitString, aFormatString, null, bYesNoForBool);
                ForeColor = cell.Property.BrushColorString;
            }

        }

        #endregion Constructors

        public override string ToString()
        {
            return $"CellInfo [{Value}]";
        }
    }

}