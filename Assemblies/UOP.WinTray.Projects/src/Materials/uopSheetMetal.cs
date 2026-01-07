using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Structures;


namespace UOP.WinTray.Projects.Materials
{
    /// <summary>
    /// ^sheet metal object
    /// ~used as the material object property for any part formed from sheet metal
    /// </summary>
    public class uopSheetMetal : uopMaterial
    {
        #region Constructors
        public uopSheetMetal() : base(uppMaterialTypes.SheetMetal) { }

        public uopSheetMetal(string aFamilyName, string aGageName, bool bIsStainless, double aDensity, string aSpec = "") : base(uppMaterialTypes.SheetMetal) 
        {
            base.FamilyName = aFamilyName;
            base.GageName = aGageName;
            base.IsStainless = bIsStainless;
            base.Density = aDensity;
            base.SpecName = aSpec;

        }

        internal uopSheetMetal(uopSheetMetal aMaterialToCopy) : base(uppMaterialTypes.SheetMetal) { base.Structure = aMaterialToCopy.Structure; }


        internal uopSheetMetal(TMATERIAL aStructure,uppPartTypes aPartType = uppPartTypes.Undefined) : base(uppMaterialTypes.SheetMetal) { if (aStructure.Type == base.MaterialType) { base.Structure = aStructure; base.PartType = aPartType; } }

        #endregion

        public uopSheetMetal Clone() => new uopSheetMetal(this);


        
        public override uopMaterial Clone(bool aFlag = false) => (uopMaterial)this.Clone();

        public override string ToString()
        {
            return base.ToString();
        }
    }
}