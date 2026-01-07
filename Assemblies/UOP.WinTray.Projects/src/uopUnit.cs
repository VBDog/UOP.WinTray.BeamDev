using System;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Structures;

namespace UOP.WinTray.Projects
{
    public class uopUnit :ICloneable
    {

        TUNIT tStruc;

        #region Constructors
        public uopUnit() { tStruc = new TUNIT(uppUnitTypes.Undefined); }

        internal uopUnit(TUNIT aStructure) { tStruc = aStructure; }

        public uopUnit(uppUnitTypes aUnitType, uppUnitFamilies aUnitSystem = uppUnitFamilies.English)
        {
            tStruc = new TUNIT(aUnitType, aUnitSystem);
        }


        #endregion

        public uopUnit Clone() => new uopUnit(tStruc.Clone());

        object ICloneable.Clone() => (object)this.Clone();


        public bool IsDefined => tStruc.IsDefined;

        public string Name => tStruc.Name;

        public string SILabel => tStruc.SILabel;

        public string MetricLabel => tStruc.MetricLabel;

        public string EnglishLabel => tStruc.EnglishLabel;
       
        public uppUnitFamilies UnitSystem => tStruc.UnitSystem;

        public double SIFactor => tStruc.SIFactor;

        public double MetricFactor => tStruc.MetricFactor;

        public int Precision(uppUnitFamilies aFamily) => tStruc.Precision(aFamily);

        public int Index => tStruc.Index;

        public uppUnitTypes UnitType => tStruc.UnitType;



        public override string ToString() => "uopUnits [ " + Name + "]";


        public string Label(uppUnitFamilies aUnitFamily, bool addleadSpace = false) => tStruc.Label(aUnitFamily, addleadSpace);

        public double ConversionFactor(uppUnitFamilies aFamily) => tStruc.ConversionFactor(aFamily);
        /// <summary>
        /// Format the string 
        /// ^the string used to format the values assigned to this unit object
        /// </summary>
        /// <param name="aUnits"></param>
        /// <param name="aUnitFamily"></param>
        /// <param name="aOverridePrecision"></param>
        /// <param name="bIncludeThousandsSeps"></param>
        /// <returns></returns>
        public string FormatString(uppUnitFamilies aUnitFamily = uppUnitFamilies.Default, int aOverridePrecision = -1, bool bIncludeThousandsSeps = false) => tStruc.FormatString(aUnitFamily, aOverridePrecision, bIncludeThousandsSeps);

        public double ConvertValue(dynamic aValue, uppUnitFamilies aFromUnits, uppUnitFamilies aToUnits = uppUnitFamilies.English, int aPrecis = -1)
        => tStruc.ConvertValue(aValue, aFromUnits, aToUnits, aPrecis);


        public double ConvertValue(dynamic aValue, out double rEnglishValue, uppUnitFamilies aFromUnits, uppUnitFamilies aToUnits, int aPrecis = -1)
        => tStruc.ConvertValue(aValue, out rEnglishValue, aFromUnits, aToUnits, aPrecis);
    public string UnitValueString(dynamic aValue,  uppUnitFamilies aToUnits, uppUnitFamilies aFromUnits = uppUnitFamilies.English, string aPrefix = null, bool bAddLabel = true, int aPrecis = -1, bool bIncludeThousandsSeps = true, bool bZeroAsNullString = false)
      => tStruc.UnitValueString(aValue, aToUnits, aFromUnits, aPrefix, bAddLabel, aPrecis, bIncludeThousandsSeps, bZeroAsNullString);

        public void SetUnitPrecision(uppUnitFamilies aFamily, int aPrecis) => tStruc.SetUnitPrecision(aFamily, aPrecis);
    }
}
