using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Materials;
using UOP.WinTray.Projects.Structures;

namespace UOP.WinTray.Projects
{
    public abstract class uopMaterial
    {

        TMATERIAL tStruc;

        #region Constructors
        public uopMaterial(uppMaterialTypes aType) => tStruc = new TMATERIAL(aType);

        internal uopMaterial(TMATERIAL aStructure) => tStruc = aStructure;

        #endregion

        internal TMATERIAL Structure { get => tStruc; set => tStruc = value; }

        public void SubPart(uopPart aPart)
        {
            if (aPart == null) return;
            tStruc.PartIndex = aPart.Index;
            tStruc.PartType = aPart.PartType;
            tStruc.RangeGUID = aPart.RangeGUID;
        }

        /// <summary>
        /// returns an new object with the same properties as the cloned object
        /// </summary>
        public abstract  uopMaterial Clone(bool aFlag = false);

        public string FriendlyName(bool bSuppressGageName = false)=> tStruc.FriendlyName(bSuppressGageName);

        public bool IsEqual(uopMaterial aMaterial, bool bCompareSpec = false)
        {
            if (aMaterial == null) return false;
            return aMaterial.Structure.IsEqual(tStruc,bCompareSpec);

        }
        public bool IsMetric { get => tStruc.IsMetric; set => tStruc.IsMetric = value; }
        /// <summary>
        /// returns True if the material object is a stainless steel material
        /// </summary>
        public bool IsStainless { get => tStruc.IsStainless; set => tStruc.IsStainless = value; }

        public string FamilySelectName => tStruc.FamilySelectName;

        public string GageName { get => tStruc.GageName; set => tStruc.GageName = value; }
        public int Index  => tStruc.Index;

        public virtual uppSheetGages SheetGage { get => tStruc.SheetGage; set => tStruc.SheetGage = value; }

        /// <summary>
        /// returns the name of the material object
        /// </summary>
        public string MaterialName { get => tStruc.MaterialName; set => tStruc.MaterialName = value; }

        public uppMaterialTypes MaterialType { get => tStruc.Type; }
        /// <summary>
        /// returns a string that fully describes the material
        /// strings like Carbon Steel¸10 ga.¸0.1345¸0.2089¸-1,-1.
        /// string deliminator is ascii character 184
        /// </summary>
        public string Descriptor => tStruc.Descriptor;

        public uppPartTypes PartType { get => tStruc.PartType; set => tStruc.PartType = value; }
        public dynamic Tag { get => tStruc.Tag; set => tStruc.Tag = value; }
        public string SpecName { get => tStruc.Spec; set => tStruc.Spec = value; }
        public uppSpecTypes SpecType => tStruc.SpecType;
        public string SpanName { get => tStruc.SpanName; set => tStruc.SpanName = value; }
        public string RangeGUID { get => tStruc.RangeGUID; set => tStruc.RangeGUID = value; }
        public int PartIndex { get => tStruc.PartIndex; set => tStruc.PartIndex = value; }
        public double Thickness { get => tStruc.Thickness; set => tStruc.Thickness = value; }
        public uppMetalFamilies Family { get => tStruc.Family; set => tStruc.Family = value; }

        public double WeightMultiplier => tStruc.WeightMultiplier;
        public double Density { get => tStruc.Density; set => tStruc.Density = value; }

        public string FamilyName { get => tStruc.FamilyName; set => tStruc.FamilyName = value; }

        public override string ToString() => this.Descriptor;
        


        #region Shared Methods

        internal static uopMaterial Create(TMATERIAL aStructure)
        {
            switch (aStructure.Type)
            {
                case uppMaterialTypes.SheetMetal:
                    return new uopSheetMetal(aStructure);
                case uppMaterialTypes.Hardware:
                    return new  uopHardwareMaterial(aStructure);
                case uppMaterialTypes.Tubing:
                    return new uopTubeMaterial(aStructure);
                case 0:
                    return null;

            }
            return null;
        }

        #endregion
    }
}
