using UOP.DXFGraphics;
using System;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Generators;

using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;

namespace UOP.WinTray.Projects.Parts
{
    public class mdAPPan : uopPart
    {
        public override uppPartTypes BasePartType => uppPartTypes.APPan;

        #region Constructors

        public mdAPPan() : base(uppPartTypes.APPan, uppProjectFamilies.uopFamMD, bIsSheetMetal: true) => Initialize();

        public mdAPPan(mdTrayAssembly aAssy) : base(uppPartTypes.APPan, uppProjectFamilies.uopFamMD, bIsSheetMetal: true) => Initialize(null,aAssy);


        public mdAPPan( mdAPPan aPartToCopy) : base(uppPartTypes.APPan, uppProjectFamilies.uopFamMD, bIsSheetMetal: true) => Initialize(aPartToCopy);
        

        private void Initialize(mdAPPan aPartToCopy = null, mdTrayAssembly aAssy = null)
        {
         
            FlangeLength = 31.8 / 25.4; // 1.25;
            TabWidth = 20 / 25.4; //  0.75;
            TabInset = 21.8 / 25.4; // 0.875;
            TabLength = 25 / 25.4; // 1;
            PerforationDiameter = 10 / 25.4;
            HoleInset = 14.3 / 25.4; // 0.653;
            DowncomerThickness = 0;
            Direction = dxxOrthoDirections.Up;
            OpenEnded = false;
            Length = 0;
            LongLength = 0;
            Height = 0;
            BoltCount = 1;
            OpenFaced = false;
            IsEndPan = false;
            SetCoordinates(0, 0, 0);

            ParentPartType = uppPartTypes.Downcomer;
            if (aPartToCopy != null)
            {
                base.Copy(aPartToCopy);
             
                FlangeLength = aPartToCopy.FlangeLength; // 1.25;
                TabWidth = aPartToCopy.TabWidth; //  0.75;
                TabInset = aPartToCopy.TabInset; // 0.875;
                TabLength = aPartToCopy.TabLength; // 1;
                PerforationDiameter = aPartToCopy.PerforationDiameter;
                HoleInset = aPartToCopy.HoleInset;
                DowncomerThickness = aPartToCopy.DowncomerThickness;
                Direction = aPartToCopy.Direction;
                OpenEnded = aPartToCopy.OpenEnded;
                Length = aPartToCopy.Length;
                _LongLength = aPartToCopy._LongLength;
                Height = aPartToCopy.Height;
                BoltCount = aPartToCopy.BoltCount;
                OpenFaced = aPartToCopy.OpenFaced;
                ParentPartType = aPartToCopy.ParentPartType;
                IsEndPan = aPartToCopy.IsEndPan;
                SetCoordinates(aPartToCopy.Center);
               
               
            }
            if(aAssy != null)
            {
                SheetMetalStructure = aAssy.GetSheetMetal(false).Structure;
                mdDowncomer dc = aAssy.Downcomer();
                mdDesignOptions ops = aAssy.DesignOptions;
                Width = dc.BoxWidth;
                PercentOpen = ops.APPanPercentOpen;
                PerforationDiameter = ops.APPanPerfDiameter;
                DowncomerThickness = dc.Thickness;
                Height = aAssy.APPanHeight;
                Z = dc.HeightBelowDeck - DowncomerThickness / 2;

                SubPart(aAssy);
            }
        }
        #endregion Constructors

        #region Properties

        public bool IsEndPan { get; set; }

        public override uopInstances Instances { get => base.Instances; set { base.Instances = value; Quantity = value == null ? 1 : value.Count + 1; } }

        //^the bolt used to install the part
        public hdwHexBolt Bolt { get => base.SmallBolt("AP Pan Bolt", "AP Pan Attachment"); }

        public uopHole Hole { get => new uopHole(HoleV); }

        public int BoltCount {get; set; }


        internal UHOLE HoleV => new UHOLE(aDiameter: mdGlobals.gsBigHole , aX: X, aY: Y, aDepth: Thickness, aElevation: Z, aTag: "APPAN_HOLE", aFlag: "LEFT", aInset: HoleInset);
      

       
        public override dxxOrthoDirections Direction
        { get =>base.Direction; set => base.Direction = (value == dxxOrthoDirections.Up || value == dxxOrthoDirections.Down) ? value : base.Direction; }
     
        public double DowncomerThickness { get; set; }
        public double TabHeight => DowncomerThickness > 0 ? DowncomerThickness + 1.6 / 25.4 : Thickness + 1.6 / 25.4;
        /// <summary>
        /// the width of the mounting flange as viewd from above
        /// default = 1.25
        /// </summary>
        public double FlangeLength { get; set; }

        /// <summary>
        /// the height of the pan
        /// the distance between the parent downcomer and the tray below less a clearance
        /// </summary>
        public override double Height { get => base.Height; set => base.Height = value;  }

        /// <summary>
        /// the inset of the mounting hole from the exterior edge of the pan flange
        /// default = 0.563
        /// </summary>
        public double HoleInset { get; set; }

        /// <summary>
        /// the pan length
        /// based on the spout group that spawned this pan
        /// </summary>
        public override double Length { get => base.Length; set => base.Length = value;  }
        /// the pan length
        /// based on the spout group that spawned this pan
        /// </summary>

        
        private double _LongLength;
        public double LongLength { get => _LongLength > Length ? _LongLength : Length; set => _LongLength = value; }

        public uopHole MountingSlot { get { return new uopHole(MountingSlotV); } }

        internal UHOLE MountingSlotV { get { return new UHOLE(aDiameter: 2.5 / 25.4, aX: 0, aY: 0, aLength: 25 / 25.4, aTag: "APPAN_SLOT", aZDirection: "0,0,1"); } }

        public bool OpenEnded { get; set; }

        public bool OpenFaced { get; set; }

        /// <summary>
        /// the diameter of the perforations in the pan bottom
        /// </summary>
        public override double PerforationDiameter { get => base.PerforationDiameter; set => base.PerforationDiameter = value;  }

        /// <summary>
        /// the inset of the mounting tabs from the exterior edge of the pan
        /// default = 0.875
        /// </summary>
        public double TabInset { get; set; }

        /// <summary>
        /// the length of the mounting tabs as viewed from above
        /// ^default = 1
        /// </summary>

        public double TabLength { get; set; }
        /// <summary>
        /// the height of the pan on the side with the tabs
        /// </summary>

        public double TabSideHeight { get => DowncomerThickness + Thickness + Height; }
        /// <summary>
        /// the width of the mounting tabs as viewd from above
        /// default = 0.75
        /// </summary>

        public double TabWidth { get; set; }

        public double OverallLength { get => Length + TabLength + FlangeLength; }
        /// <summary>
        ///returns the weight of the pan in english pounds
        /// </summary>
        public override double Weight
        {
            get
            {
                double sArea = Width * Length;
                double thk = Thickness;

                sArea -= PercentOpen / 100 * sArea;
                sArea += (Height - thk) * Length;
                sArea += (Height + thk) * Length;
                sArea += Width * FlangeLength;
                sArea += 2 * (TabLength * TabWidth);
                sArea -= GenHolesV().TotalArea();
                //multiply by the thickness and density
                base.Weight = sArea * base.SheetMetalWeightMultiplier;
                return base.Weight;
            }
        }


        #endregion Properties

        #region Methods


        public override string ToString() { return $"AP PAN({ PartNumber })"; }
     
       
        /// <summary>
        ///returns a new object that is an exact copy of this one
        /// </summary>
        /// <returns></returns>
        /// 
        public mdAPPan Clone() => new mdAPPan(this);
       
        public override uopPart Clone(bool aFlag = false)  => this.Clone();

      


        public override bool IsEqual(uopPart aPart)
        {
            
            if (aPart == null) return false;
            if (aPart.PartType != PartType) return false;

            try
            {
                
                return CompareTo((mdAPPan)aPart);

            }
            catch { return false; }
        }


        public bool CompareTo(mdAPPan aPart, bool bCompareMaterial = true)
        {

            if (aPart == null) return false;


            try
            {


                if (aPart.OpenEnded != OpenEnded) return false;
                if (aPart.OpenFaced != OpenFaced) return false;
                if (aPart.BoltCount != BoltCount) return false;
                if (!TVALUES.CompareNumeric(aPart.Width, Width, 3)) return false;
                if (!TVALUES.CompareNumeric(aPart.Height, Height, 3)) return false;
                if (!TVALUES.CompareNumeric(aPart.Length, Length, 3)) return false;
                if (!TVALUES.CompareNumeric(aPart.TabWidth, TabWidth, 3)) return false;
                if (!TVALUES.CompareNumeric(aPart.TabLength, TabLength, 3)) return false;
                if (!TVALUES.CompareNumeric(aPart.PerforationDiameter, PerforationDiameter, 3)) return false;

                if (bCompareMaterial)
                {
                    if (!aPart.Material.IsEqual(Material)) return false;
                }

                return true;

            }
            catch { return false; }
        }

        public override uopProperties CurrentProperties() { UpdatePartProperties(); return base.CurrentProperties(); }

     
        
        public override void UpdatePartProperties()
        {
            base.DescriptiveName = $"AP Pan ({ Math.Round(Length, 1) })";
          
        }
        
        public override void UpdatePartWeight() => base.Weight = Weight;
        
        /// <summary>
        ///returns a dxePolygon that is used to draw the elevation view of the pan
        /// </summary>
        /// <param name="aTabEnd"></param>
        /// <param name="bShowObscured"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <param name="aScale"></param>
        /// <returns></returns>
        public dxePolygon View_Elevation(bool aTabEnd = true, bool bShowObscured = false, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY", double aScale = 1)
        => mdPolygons.APPan_View_Elevation(this, aTabEnd, bShowObscured, aCenter, aRotation, aLayerName, aScale);
          
        /// <summary>
        ///returns a dxePolygon that is used to draw the layout view of the pan
        /// </summary>
        /// <param name="bShowObscured">scale factor to apply to the returned polygon</param>
        /// <param name="aCenter">flag to just return the outline of the pan as view in the layout drawing</param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <returns></returns>
        public dxePolygon View_Plan(bool bShowObscured = false, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY")
       => mdPolygons.APPan_View_Plan(this, bShowObscured, aCenter, aRotation, aLayerName);
          
        /// <summary>
        ///returns a dxePolygon that is used to draw the profile view of the pan
        /// </summary>
        /// <param name="aShowObscured"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="bLongSide"></param>
        /// <param name="aLayerName"></param>
        /// <returns></returns>
        public dxePolygon View_Profile(bool aShowObscured = false, iVector aCenter = null, double aRotation = 0, bool bLongSide = false, string aLayerName = "GEOMETRY")
        => mdPolygons.APPan_View_Profile(this, aShowObscured, aCenter, aRotation, bLongSide, aLayerName);
          
      
        public override uopHoleArray HoleArray(uopTrayAssembly aAssy = null, string aTag = null) => GenHoles();

        /// <summary>
        /// executed internally to create the holes collection for the pan
        /// </summary>
        /// <param name="aTag"></param>
        /// <param name="aFlag"></param>
        /// <param name="bReturnMountingSlots"></param>
        /// <returns></returns>
        public uopHoleArray GenHoles(string aTag = null, string aFlag = null, bool bReturnMountingSlots = false) => new uopHoleArray(GenHolesV(aTag, aFlag, bReturnMountingSlots));

        /// <summary>
        /// executed internally to create the holes collection for the pan
        /// </summary>
        /// <param name="aTag"></param>
        /// <param name="aFlag"></param>
        /// <param name="bReturnMountingSlots"></param>
        /// <returns></returns>
        internal UHOLEARRAY GenHolesV(string aTag = null, string aFlag = null, bool bReturnMountingSlots = false)
        {
            UHOLEARRAY _rVal =UHOLEARRAY.Null;;

            dxfVector v0 = CenterDXF;
            UHOLE aHole = HoleV;
            double thk = Thickness;
            UHOLES aHls = new UHOLES("APPAN_HOLE");
            UHOLES bHls = new UHOLES("APPAN_SLOT");
            UHOLE aSlot;
            aTag = string.IsNullOrWhiteSpace(aTag) ? string.Empty : aTag.Trim().ToUpper();
            aFlag = string.IsNullOrWhiteSpace(aFlag) ? string.Empty : aFlag.Trim().ToUpper();

            if (mzUtils.ListContains("APPAN_SLOT", aTag, ",", false)) bReturnMountingSlots = true;
            bool returnHoles = mzUtils.ListContains("APPAN_HOLE", aTag, ",", true);
            int hcount = mzUtils.LimitedValue(BoltCount, 1, 2);
            double xleft = v0.X - 0.5 * Width + TabInset + TabWidth / 2;
            double xright = v0.X + 0.5 * Width - TabInset - TabWidth / 2;
            double ytop = v0.Y;

            if (OpenEnded)
            {
                hcount = 2;
                bReturnMountingSlots = false;
            }

            aHole.X = v0.X;
            aHole.Y = ytop;
            aHole.InSet = HoleInset;
            aHole.Elevation = v0.Z;
            aHls.Member = aHole;

            if (hcount == 1)
            {
                aHls.Centers.Add(aHole.Center);

            }
            else
            {

                aHole.X = xleft;
                if (aFlag ==  string.Empty || aFlag == "LEFT") aHls.Centers.Add( xleft, ytop);
                if (aFlag ==  string.Empty || aFlag == "RIGHT") aHls.Centers.Add(xright, ytop); ;

            }


            if (bReturnMountingSlots)
            {
                aSlot = MountingSlotV;

                double f1 = v0.Rotation == 0 ? -1 : 1;
                double hole2slot = aHole.InSet - FlangeLength - Length - 0.5 * thk;
                aSlot.Y = v0.Y - f1 * hole2slot;
                aSlot.Rotation = f1 == 1 ? 180 : 0; 
                aSlot.Elevation = v0.Z + 1.5 * aHole.Depth;

                bHls.Member = aSlot;
                aSlot.Center = new UVECTOR(xleft, aSlot.Y);
                if (aFlag ==  string.Empty || aFlag == "LEFT") bHls.Centers.Add(aSlot.Center, "LEFT");

                aSlot.Center = new UVECTOR(xright, aSlot.Y);
                if (aFlag ==  string.Empty || aFlag == "RIGHT") bHls.Centers.Add(aSlot.Center, "RIGHT");

            }

            if (aHls.Count > 0 && returnHoles) _rVal.Add(aHls, "APPAN_HOLE");

            if (bHls.Count > 0 && bReturnMountingSlots) _rVal.Add(bHls, "APPAN_SLOT");
            return _rVal;
        }

        /// <summary>
        /// executed internally to create the holes collection for the pan
        /// </summary>
        /// <param name="aTag"></param>
        /// <param name="aFlag"></param>
        /// <param name="bReturnMountingSlots"></param>
        /// <returns></returns>
        internal UHOLEARRAY MountingHolesV(double aElevation, double aDepth, double? aRotation = null, uopVector aCenter = null)
        {
            UHOLEARRAY _rVal =UHOLEARRAY.Null;;

            dxfVector v0 = aCenter == null ? CenterDXF : new dxfVector(aCenter.X,aCenter.Y,Z) ;
            if (!aRotation.HasValue) aRotation = v0.Rotation;
            UHOLE aHole = HoleV;
            double thk = Thickness;
            UHOLES aHls = new UHOLES("APPAN_HOLE");
            UHOLES bHls = new UHOLES("APPAN_SLOT");
            UHOLE aSlot = MountingSlotV;

            int hcount = mzUtils.LimitedValue(BoltCount, 1, 2);
            double xleft = X - 0.5 * Width + TabInset + TabWidth / 2;
            double xright = X + 0.5 * Width - TabInset - TabWidth / 2;
            double ytop = v0.Y;

            if (OpenEnded) hcount = 2;
       
            
            aHole.X = hcount == 1 ? X : xleft ;
            aHole.Y = ytop;
            aHole.Depth = aDepth;
            aHole.InSet = HoleInset;
            aHole.Elevation = aElevation;
            aHls.Member = aHole;
            aHls.Centers.Add(aHole.Center);

            if (hcount == 2) aHls.Centers.Add(new UVECTOR(xright,aHole.Y));

            if (!OpenEnded)
            {
                double f1 = aRotation.Value == 0 ? -1 : 1;
                double hole2slot = aHole.InSet - FlangeLength - Length - 0.5 * thk;
                aSlot.Y = v0.Y - f1 * hole2slot;
                aSlot.Rotation = aRotation.Value;
                aSlot.Elevation = aElevation;
                aSlot.Depth = aDepth;
                bHls.Member = aSlot;
                aSlot.Center = new UVECTOR(xleft, aSlot.Y);
                bHls.Centers.Add(new UVECTOR(xleft, aSlot.Y), "LEFT");

                aSlot.Center = new UVECTOR(xright, aSlot.Y);
                bHls.Centers.Add(new UVECTOR(xright, aSlot.Y), "RIGHT");
            }

      

           

            if (aHls.Count > 0 ) _rVal.Add(aHls, "APPAN_HOLE");

            if (bHls.Count > 0 ) _rVal.Add(bHls, "APPAN_SLOT");
            return _rVal;
        }


     
        public double BottomLength(bool Inside)
        {
            return Inside ? Length : Length + 2 * Thickness;
        }

        #endregion Methods


    }
}
