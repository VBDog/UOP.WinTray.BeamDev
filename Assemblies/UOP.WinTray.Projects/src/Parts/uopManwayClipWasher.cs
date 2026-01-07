using UOP.DXFGraphics;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Materials;

namespace UOP.WinTray.Projects.Parts
{
    public class uopManwayClipWasher : uopPart
    {
        public override uppPartTypes BasePartType => uppPartTypes.ManwayWasher;
       
        public uopManwayClipWasher() : base(Enums.uppPartTypes.ManwayWasher, uppProjectFamilies.uopFamMD, "", "", true)
        {
            InitializeProperties();
        }

        internal uopManwayClipWasher(uopManwayClipWasher aPartToCopy) : base(uppPartTypes.ManwayWasher, uppProjectFamilies.uopFamMD, "", "", true)
        {
            InitializeProperties();
            if(aPartToCopy != null) { Copy(aPartToCopy);  }

        }
        public uopManwayClipWasher(uopTrayAssembly aParent) : base(uppPartTypes.ManwayClip, uppProjectFamilies.uopFamMD, "", "", true)
        {
            InitializeProperties();
            if (aParent != null)
            {
                uopMaterials mtrls = uopGlobals.goSheetMetalOptions();
                uopPart aDeck = aParent.DeckObj;
                uppSheetGages aGage = aDeck.SheetMetal.IsMetric ? uppSheetGages.Gage3pt5mm : uppSheetGages.Gage10;
                SubPart(aParent);
                SheetMetal = mtrls.GetByFamilyAndGauge(aDeck.SheetMetalFamily, aGage);
                HardwareMaterial = aParent.RangeHardwareMaterial;

                SubPart(aParent);

            }


        }

        private void InitializeProperties()
        {
            
            Length = 3.5;
            Width = 0.75;
            SparePercentage = 5;
            PartNumber = "65";
            SparePercentage = 5;
            Category = "Manway Attachment";
        }


        public uopManwayClipWasher Clone() => new uopManwayClipWasher(this);

        public override uopPart Clone(bool aFlag = false) => (uopPart)this.Clone();

        public override void UpdatePartProperties()
        {
            throw new System.NotImplementedException();
        }


        public dxeHole Slot => new dxeHole(CenterDXF.Moved(-1,0,-0.5 * Thickness) ,aDiameter: 0.25,aLength: mdGlobals.gsBigHole,aDepth: Thickness, aRotation:90,aInset:1.375 );

        public override double Weight { get => ((Width * Length) - Slot.Area) * base.SheetMetalWeightMultiplier; }
        public override void UpdatePartWeight() => base.Weight = this.Weight;

        public dxeHole Hole => new dxeHole(CenterDXF, aDiameter: mdGlobals.gsBigHole, aDepth: Thickness, aInset: 2.375);

        public dxePolygon Profile(double ScaleFactor = 1)
       => uopPolygons.ManwayWasher_Profile(this, ScaleFactor);

        public dxePolygon View_Plan(bool bSuppressAngle, bool bSuppressHole = false, bool bSuppressSlot = false,string aLineType = "")
        => uopPolygons.ManwayWasher_Plan(this, bSuppressAngle, bSuppressHole, bSuppressSlot, aLineType);
        
    }
}