using System;
using System.Collections.Generic;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;

namespace UOP.WinTray.Projects.Documents
{
    /// <summary>
    /// UOP Doc Calculation Model
    /// </summary>
    public class uopDocCalculation : uopDocument , ICloneable
    {
        #region Constructors

        public uopDocCalculation() : base(uppDocumentTypes.Calculation) { }

        public uopDocCalculation(uppCalculationType aCalcType) : base(uppDocumentTypes.Calculation) { base.SubType = (int)aCalcType; }


        internal uopDocCalculation(uopDocCalculation aDocToCopy) : base(uppDocumentTypes.Calculation)
        {
            base.Copy(aDocToCopy);
        }
        internal uopDocCalculation(TDOCUMENT aDoc, mzQuestions aOptions = null, mzQuestions aTrayQuery = null) : base(uppDocumentTypes.Calculation)
        {
            aDoc.DocumentType = uppDocumentTypes.Calculation;
            Structure = aDoc.Clone();
            base.TrayQuery = aTrayQuery;
            base.Options = aOptions;

        }


        #endregion

        /// <summary>
        /// returns the Clone of the object
        /// </summary>
        public uopDocCalculation Clone() => new uopDocCalculation(this);

        /// <summary>
        /// returns the Clone of the object
        /// </summary>
        public override uopDocument Clone(bool aFlag = false) => (uopDocument)this.Clone();

        /// <summary>
        /// returns the Clone of the object
        /// </summary>

        object ICloneable.Clone() => (object)this.Clone();


        //================== CALC SPECIFIC PROPERTIES and METHODS FOLLOW =============
     
        public uppCalculationType CalcType { get => (uppCalculationType)base.SubType; set => base.SubType = (int)value; }
      

        public string ProjectName { get => Structure.String1; set { TDOCUMENT str = Structure; str.String1 = value; Structure = str; } }
        
        public string CalcName { get => Structure.String3; set { TDOCUMENT str = Structure; str.String3 = value; Structure = str; } }


        public uppUnitFamilies Units { get => base.DisplayUnits; set => base.DisplayUnits = value; }

        public List<string> ReportData { get =>  TVALUES.ToStringList( base.Data); set => base.Data = TVALUES.FromStringList(value); }

    }
}
