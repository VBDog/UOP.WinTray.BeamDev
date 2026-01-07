using UOP.DXFGraphics;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Generators;


namespace UOP.WinTray.Projects.Parts
{
    public class uopRingClip : uopPart
    {
        public override uppPartTypes BasePartType => uppPartTypes.RingClip;
        private uppRingClipSizes _Size;
        private bool _ForEndSupport;
     

        public uopRingClip() : base(uppPartTypes.RingClip, uppProjectFamilies.uopFamMD, "", "", true)
        {
            InitializeProperties();
        }


        public uopRingClip(uopPart aParent) : base(uppPartTypes.RingClip, uppProjectFamilies.uopFamMD, "", "", true)
        {

            InitializeProperties();
            SubPart(aParent, Category);
        }

        internal uopRingClip(uopRingClip aPartToCopy) : base(uppPartTypes.RingClip, uppProjectFamilies.uopFamMD, "", "", true)
        {

            InitializeProperties();
            Copy(aPartToCopy);
            Size = aPartToCopy.Size;
            _ForEndSupport = aPartToCopy._ForEndSupport;
            
        }

        private void InitializeProperties()
        {
          
            
            SparePercentage = 5;
            Category = "Tray To Ring";
         
            Size = uppRingClipSizes.ThreeInchRC;
        }
        private void SetSubPartType()
        {
            base.SubPartType = _Size;
            if (_Size == uppRingClipSizes.ThreeInchRC)
            {
                DescriptiveName = "Ring Clip (3 in.)";

                
            }
            else
            {
                DescriptiveName = "Ring Clip (4 in.)";
            }
        }
        public uopRingClip Clone() => new uopRingClip(this);

        public override uopPart Clone(bool aFlag = false) => (uopPart)this.Clone();

    
        public uppRingClipSizes Size { get => _Size;  set { _Size = value; base.SubPartType = _Size; SetSubPartType(); } }
     

        public double BigHeight => (Size == uppRingClipSizes.ThreeInchRC) ? 1 : 1.25;
        public double BigRadius => (Size == uppRingClipSizes.ThreeInchRC) ? 1.906 : 3;
        public double SmallHeight => (Size == uppRingClipSizes.ThreeInchRC) ? 0.625 : 0.75;
        public double SmallRadius => (Size == uppRingClipSizes.ThreeInchRC) ? 1.25 : 1.5;

        public bool ForEndSupport { get => _ForEndSupport;  set =>_ForEndSupport=value;  }

        public hdwHexBolt Bolt {
            get 
            {
                hdwHexBolt _rVal = new hdwHexBolt(this)
                {
                    WeldedInPlace = true,

                    IsVisible = IsVisible,
                    //_rVal.Material = goHardwareMaterialOptions.GetByFamily(SheetMetalFamily)


                    HardwareMaterial = RangeHardwareMaterial
                };
                if (Bolting == uppUnitFamilies.English)
                {

                    if (Size == uppRingClipSizes.ThreeInchRC)
                    {
                        _rVal.Size =uppHardwareSizes.ThreeEights;
                        _rVal.Length = 2;
                        _rVal.ThreadedLength = 1;
                    }
                    else
                    {
                        _rVal.Size = (!ForEndSupport) ?uppHardwareSizes.ThreeEights : uppHardwareSizes.OneHalf;
                        _rVal.Length = 2.5;
                        _rVal.ThreadedLength = 1.25;
                    }
                }
                else
                {

                    if (Size == uppRingClipSizes.ThreeInchRC)
                    {
                        _rVal.Size = uppHardwareSizes.M10;
                        _rVal.Length = 50 / 25.4;
                        _rVal.ThreadedLength = 25 / 25.4;
                    }
                    else
                    {

                    }
                    _rVal.Size = (!ForEndSupport) ? uppHardwareSizes.M10 : uppHardwareSizes.M12;

                    _rVal.Length = 65 / 25.4;
                    _rVal.ThreadedLength = 35 / 25.4;
                }
       


                _rVal.SparePercentage = 0;
                _rVal.DescriptiveName = "Ring Clip _rVal";
                _rVal.Quantity = Quantity;
                _rVal.SubPart( this, Category);
                return _rVal;
            }

        }

        public bool LargeBolt => Size == uppRingClipSizes.FourInchRC && ForEndSupport;

        public override double Length { get => (Size == uppRingClipSizes.ThreeInchRC)? 2.75:4; set { return; } }

        public double NibWidth => 0.125;

        public dxeHole BoltHole
        {
            get
            {
                dxeHole _rVal = new dxeHole(CenterDXF, mdGlobals.gsSmallHole, aDepth: Thickness)
                {
                    Inset = (Size == uppRingClipSizes.ThreeInchRC) ? 0.75 : 1,
                    Diameter = (!LargeBolt) ? mdGlobals.gsSmallHole : 13 / 25.4 // 0.531;
                };
                return _rVal;

            }

        }

        public dxePolygon View_Elevation(bool bShowBolt = false, bool bShowThreads = false, bool bIncludeCenterLine = false, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY")
         =>  uopPolygons.RingClip_View_Elevation(this, bShowBolt, bShowThreads, bIncludeCenterLine, aCenter, aRotation, aLayerName);
        

        public dxePolygon View_Profile(bool bShowBolt = false, bool bShowThreads = false, bool bIncludeCenterLine = false, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY")
         => uopPolygons.RingClip_View_Profile(this, bShowBolt, bShowThreads, bIncludeCenterLine, aCenter, aRotation, aLayerName);



        public dxePolygon View_Plan(bool bShowBolt = false, bool bShowBottom = false, bool bIncludeLengthCL = false, bool bIncludeWidthCL = false, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY")
        => uopPolygons.RingClip_View_Plan(this, bShowBolt, bShowBottom, bIncludeLengthCL, bIncludeWidthCL, aCenter, aRotation, aLayerName);

        public override void UpdatePartProperties() { return; }

        public override void UpdatePartWeight()
        {
            base.Weight = Weight();
        }

        public new double Weight()
        {
            double sArea = 2 * View_Elevation().Area + ((Width - 2 * Thickness) * Length);

            sArea *= SheetMetalWeightMultiplier;

            if (Size == uppRingClipSizes.ThreeInchRC)
            { sArea += 0.08; }  //the weight of a 3/8 x 2 bolt
            else
            {

                if (_ForEndSupport)
                { sArea += 0.17; } //the weight of a 1/2 x 2.25 bolt
                else
                { sArea += 0.09; } //the weight of a 3/8 x 2.25 bolt

            }
            return sArea;
        }
    


      
    }
}