using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Structures;


namespace UOP.WinTray.Projects.Materials
{
    public class uopHardwareMaterial : uopMaterial
    {

        #region Constructors
        public uopHardwareMaterial() : base(uppMaterialTypes.Hardware) { }

        internal uopHardwareMaterial(uopHardwareMaterial aMaterialToCopy) : base(uppMaterialTypes.Hardware) { base.Structure = aMaterialToCopy.Structure; }


        internal uopHardwareMaterial(TMATERIAL aStructure,uopPart aOwner = null) : base(uppMaterialTypes.Hardware) {  if (aStructure.Type == base.MaterialType) base.Structure = aStructure; base.SubPart(aOwner); }

        #endregion

        public uopHardwareMaterial Clone() => new uopHardwareMaterial(this);

        public override uopMaterial Clone(bool aFlag = false) => (uopMaterial)this.Clone();

        public string Name => FamilySelectName;

        public override string ToString()
        {
            return base.ToString();
        }

    }
}
