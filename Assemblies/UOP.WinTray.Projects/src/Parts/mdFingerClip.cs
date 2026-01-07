using UOP.DXFGraphics;
using System;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Materials;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Constants;

namespace UOP.WinTray.Projects.Parts
{
    public class mdFingerClip : uopPart
    {
        public override uppPartTypes BasePartType => uppPartTypes.FingerClip;

        //the end angles used to clamp down the deck panels at the ends of the downcomers
       

        #region Constructors

        public mdFingerClip(uopPart aParent = null) : base(uppPartTypes.FingerClip, uppProjectFamilies.uopFamMD, "","",true)
        {
            InitializeProperties();
            if (aParent != null) 
            {
                SubPart(aParent); 

                if(aParent.GetType() == typeof(mdTrayAssembly)) 
                {
                    mdTrayAssembly assy = (mdTrayAssembly)aParent;
                    Z = assy.Thickness;
                    Side = uppSides.Right;
                    SheetMetal = uopEventHandler.RetrieveFirstDeckMaterial(assy.ProjectHandle);
                    ProjectType = assy.ProjectType;
                }
            
            }
        }

        internal mdFingerClip(mdFingerClip aPartToCopy) : base(uppPartTypes.FingerClip, uppProjectFamilies.uopFamMD, "", "", true)
        {
            InitializeProperties(aPartToCopy);
           
            Copy(aPartToCopy);
        }

        private void InitializeProperties(mdFingerClip aPartToCopy = null)
        {
            try
            {


                HoleInset = 0.625;
                Width = 1;
                Height = 1;
                BendGap = 0.06;
                Length = mdFingerClip.DefaultLength;
                SlotWidth = 0.25;
                SlotSpan = 2;
                SlotLength = 0.75;
            
                OccuranceFactor = 1;
                Suppressed = false;
                SparePercentage = 5;
                if(aPartToCopy != null)
                {
                    Copy(aPartToCopy);

                }


            }
            catch (Exception )
            {

            }
        }


        #endregion


        public override string ToString() { return "FINGER CLIP"; }

        /// <summary>
        /// the downward bend gap for the fingers
        /// </summary>
        public double BendGap { get; set; }

        /// <summary>
        /// the bolt used to install the part
        /// </summary>
        public hdwHexBolt Bolt  => base.SmallBolt("Finger Clip Bolt", aQuantity: Quantity);

     
       
        /// <summary>
        ///returns a new object that is an exact copy of this one
        /// </summary>
        /// <returns></returns>
        public mdFingerClip Clone() =>new mdFingerClip(this);

        public override uopPart Clone(bool aFlag = false) => (uopPart)this.Clone();

        public override uopProperties CurrentProperties() { UpdatePartProperties(); return base.CurrentProperties(); }

        public override uopProperty CurrentProperty(string aPropertyName, bool bSupressNotFoundError = false)
        {
            UpdatePartProperties();
            return base.CurrentProperty(aPropertyName, bSupressNotFoundError);
        }
        /// <summary>
        /// the parent downcomer of this part
        /// </summary>
        public mdDowncomer Downcomer
        {
            get => GetMDDowncomer();
            set
            {
                //^the parent downcomer of this part
                if (value != null)
                {
                    SubPart(value);
                    DowncomerIndex = value.Index;
                }
            }
        }
        

        public dxfRectangle FootPrint(double ScaleFactor)
        {
            double scl = Math.Abs(ScaleFactor);
            if (scl == 0) scl = 1;
       
             double cx = X * scl;
            double cY = Y * scl;
            double wd = Width * scl;
            double lg = Length * scl;
            return (Side == uppSides.Left)?
                new dxfRectangle(new dxfVector(cx - 0.5 * wd, cY), wd, lg):
                new dxfRectangle(new dxfVector(cx + 0.5 * wd, cY), wd, lg);
        }
        /// <summary>
        /// executed internally to create the holes collection for the part
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public uopHoleArray GenHoles(mdTrayAssembly aAssy) => new uopHoleArray(GenHolesV(aAssy));

        /// <summary>
        /// executed internally to create the holes collection for the part
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        UHOLEARRAY GenHolesV(mdTrayAssembly aAssy, string aTag = null)
        {
            UHOLEARRAY _rVal =UHOLEARRAY.Null;;

            aAssy ??= GetMDTrayAssembly();
            if (aAssy == null) return _rVal;
            

         
            UHOLES Hls = UHOLES.Null;

            double thk = Thickness;
            //finger clip holes
            UHOLE aHole = mdGlobals.FingerClipHole( aAssy);
            aHole.Center.Y = Y;
            aHole.Center.X = (Side == uppSides.Left)? X - thk / 2:  X + thk / 2;
            aHole.Depth = thk;
            Hls.Member = aHole;

            Hls.Centers.Add(aHole.Center);
            _rVal.Add(Hls, aHole.Tag);
            return _rVal;
        }
        /// <summary>
        /// the height of the shelf
        /// default =1
        /// </summary>
        public override double Height   { get => base.Height; set =>base.Height = Math.Abs(value); }

    /// <summary>
    /// the distance the mout holes are inset from then end if the angle
    /// </summary>
    public double HoleInset { get; set; }


        public bool IsEqual(mdFingerClip aClip)
        {
            if (aClip == null) return false;
            return aClip.Material.IsEqual(Material);
        }

        /// <summary>
        /// flag indicating if the left is on the left or right side of it's owning downcomer
        /// </summary>
        public bool LeftSide { get=> Side == uppSides.Left; set
            { Side = value ? uppSides.Left: uppSides.Right; } }

        /// <summary>
        /// the length the finger clip
        /// </summary>
        public override double Length { get => base.Length; set =>base.Length = Math.Abs(value); }

        /// <summary>
        /// the lock washer used to install the part
        /// </summary>
        public hdwLockWasher LockWasher => Bolt.GetLockWasher();
        /// <summary>
        /// the material of construction for the part
        /// follows the parent tray assemblies deck material property
        /// </summary>
        public new uopMaterial Material
        {
            get => SheetMetal;

            set
            {
                if (value == null) return;
                if (value.MaterialType == uppMaterialTypes.SheetMetal) SheetMetal = (uopSheetMetal)value;

            }
        }


        /// <summary>
        /// the nut used to install the part
        /// </summary>
        public hdwHexNut Nut => Bolt.GetNut();
       
        public dxePolygon SimplePerimeter(double ScaleFactor)
        {
            dxePolygon _rVal = new dxePolygon();

            double scl = Math.Abs(ScaleFactor);
            if (scl == 0) scl = 1;
            int swap = LeftSide ? -1 : 1;
            double wd = Width * scl * swap;
            double cx = X * scl;
            double cY = Y * scl;
            double lg = Length * scl * swap;
            _rVal.Vertices.Add(cx, cY - 0.5 * lg);
            _rVal.Vertices.AddRelative(wd);
            _rVal.Vertices.AddRelative();
            _rVal.Vertices.AddRelative(-wd);
            return _rVal;
        }

        /// <summary>
        /// the length of the slots
        /// </summary>
        public double SlotLength { get; set; }
      
        /// <summary>
        /// the distance between the two fingers
        /// </summary>
        public double SlotSpan { get; set; }

        /// <summary>
        /// the width of the slots
        /// </summary>
        public double SlotWidth { get; set; }


        /// <summary>
        ///returns the area of the top flange
        /// </summary>
        public double TopArea => Length * Width;
        
      

        public override void UpdatePartProperties() { }

        public override void UpdatePartWeight() { base.Weight = Weight(); }

        /// <summary>
        ///returns a dxePolygon that is used to draw the end view of the clip
        /// </summary>
        /// <param name="bSuppressFillets"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <returns></returns>
        public dxePolygon View_Elevation(bool bSuppressFillets = false, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY")
        =>  mdPolygons.md_FingerClip_View_Elevation(this, bSuppressFillets, aCenter, aRotation, aLayerName);
     
        /// <summary>
        ///returns a dxePolygon that is used to draw the layout view of the part
        /// </summary>
        /// <param name="bSuppressHoles"></param>
        /// <param name="bSuppressOrientation"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <returns></returns>
        public dxePolygon View_Plan(bool bSuppressHoles = false, bool bSuppressOrientation = false, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY")
        => mdPolygons.md_FingerClip_View_Plan(this, bSuppressHoles, bSuppressOrientation, aCenter, aRotation, aLayerName);
       
        /// <summary>
        /// scale factor to apply to the returned polygon
        ///returns a dxePolygon that is used to draw the elevation view of the finger clip
        /// </summary>
        /// <param name="bShowTabsBent"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <returns></returns>
        public dxePolygon View_Profile(bool bShowTabsBent = false, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY")
       => mdPolygons.md_FingerClip_View_Profile(this, bShowTabsBent, aCenter, aRotation, aLayerName);
        
        /// <summary>
        /// the flat washer used to install the part
        /// </summary>
        public hdwFlatWasher Washer => Bolt.GetWasher();
        /// <summary>
        ///returns the weight of the part in english pounds
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public new double Weight(mdTrayAssembly aAssy = null)
        {
          
            double thk = Thickness;
            double sArea = 0;

            try
            {
                sArea = Length * Width + (Length * (Width - thk));
                sArea -= (((2 * SlotLength) - SlotWidth) * SlotWidth) - Math.PI * Math.Pow(SlotWidth / 2, 2);
                sArea -= GenHoles(aAssy).TotalArea();
             
                return sArea * base.SheetMetalWeightMultiplier;
            }
            catch 
            {
                return 0;
            }
        }
        /// <summary>
        /// if True the Clip is welded in place
        /// </summary>
        public bool Welded { get => WeldedInPlace; set => WeldedInPlace = value; }
            
        /// <summary>
        /// the top flange width of the angle
        /// default =1
        /// </summary>
        public override double Width { get => base.Width; set => base.Width = Math.Abs(value); }
       

        public override uopHoleArray HoleArray(uopTrayAssembly aAssy = null, string aTag = null) => new uopHoleArray(GenHolesV(aAssy != null && aAssy.ProjectFamily == uppProjectFamilies.uopFamMD ? (mdTrayAssembly)aAssy : null, aTag));

        public override bool IsEqual(uopPart aPart)
        {
            if (aPart == null) return false;
            if (aPart.PartType != PartType) return false;
            return IsEqual((mdFingerClip)aPart);
        }

        #region Shared Methods
        public static double DefaultLength => 3;
        #endregion Shared Methods
    }
}
