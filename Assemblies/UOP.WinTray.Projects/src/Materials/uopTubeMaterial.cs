using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Structures;

namespace UOP.WinTray.Projects.Materials
{
    public class uopTubeMaterial: uopMaterial
    {
        #region Constructors
        public uopTubeMaterial() : base(uppMaterialTypes.Tubing) { }

        internal uopTubeMaterial(uopTubeMaterial aMaterialToCopy) : base(uppMaterialTypes.Tubing) { base.Structure = aMaterialToCopy.Structure; }

        internal uopTubeMaterial(TMATERIAL aStructure, uopPart aPart = null) : base(uppMaterialTypes.SheetMetal) { if (aStructure.Type == base.MaterialType) base.Structure = aStructure; if (aPart != null) SubPart(aPart); }


        #endregion

        public uopTubeMaterial Clone() => new uopTubeMaterial(this);

        public override uopMaterial Clone(bool aFlag = false) => (uopMaterial)this.Clone();


        public override string ToString()
        {
            return base.ToString();
        }
    }
}