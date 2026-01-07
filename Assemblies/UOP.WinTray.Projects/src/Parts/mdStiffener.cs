using UOP.DXFGraphics;
using System;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Structures;
using Newtonsoft.Json.Linq;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;
using System.Linq;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Constants;

namespace UOP.WinTray.Projects.Parts
{
    /// <summary>
    /// represents the stiffener component of an mdDowncomer
    /// </summary>
    public class mdStiffener : mdBoxSubPart
    {
        public override uppPartTypes BasePartType => uppPartTypes.Stiffener;

     
        #region Constructors

        public mdStiffener() : base(uppPartTypes.Stiffener) => Initialize();

        public mdStiffener(mdDowncomerBox aBox, double? aY = null) : base(uppPartTypes.Stiffener) => Initialize(aBox, aY: aY);

        public mdStiffener(mdStiffener aPartToCopy ) : base(uppPartTypes.Stiffener) => Initialize(aPartToCopy: aPartToCopy);
        
        private bool _Init;
        private void Initialize(mdDowncomerBox aBox = null, mdStiffener aPartToCopy = null, double? aY =null)
        {
            if (!_Init)
            {
                FlangeWidth = 1.5;
                MaxOrdinate = 0;
                BaffleMountHeight = 0;
                BaffleThickness = 0;
                Height = 1;
                Width = 4;
                SupplementalDeflectorHeight = 0;
                BaffleMountSlot = mdGlobals.BaffleMountSlot(null);
                TopZ = 0;
                FingerClipPts = new UVECTORS(false);

                ParentPartType = uppPartTypes.Downcomer;
                _Init = true;
            }
      

            if(aPartToCopy != null)
            {
                base.Copy(aPartToCopy);
                FlangeWidth = aPartToCopy.FlangeWidth;
                MaxOrdinate = aPartToCopy.MaxOrdinate;
                BaffleMountHeight = aPartToCopy.BaffleMountHeight;
                BaffleThickness = aPartToCopy.BaffleThickness;
                Height = aPartToCopy.Height;
                Width = aPartToCopy.Width;
                SupplementalDeflectorHeight = aPartToCopy.SupplementalDeflectorHeight;
                Center = aPartToCopy.Center;
                TopZ = aPartToCopy.TopZ;
                FingerClipPts = new UVECTORS(aPartToCopy.FingerClipPts);
            
                ParentPartType = aPartToCopy.ParentPartType;
                BaffleMountSlot = new UHOLE(aPartToCopy.BaffleMountSlot);
               aBox ??= aPartToCopy.DowncomerBox;
            }

            if (aBox != null)
            {

                SubPart(aBox);

                DesignFamily = aBox.DesignFamily;
             
                TopZ = aBox.WeirHeight;
                Height = TopZ + 1;
                Width = aBox.InsideWidth;
                SupplementalDeflectorHeight = aBox.SupplementalDeflectorHeight;
                SheetMetalStructure = aBox.SheetMetalStructure;
                DowncomerIndex = aBox.DowncomerIndex;
                BaffleThickness = aBox.DeckThickness;
                BaffleMountHeight = aBox.BaffleMountHeight;
                Z = aBox.DeckThickness + 0.625;
                X = aBox.X;
                AssociateToParent(aBox);
                OverridePartNumber = $"{aBox.PartNumber}";
              
                
                if (SupportsBaffle)
                {
                    TopZ = BaffleMountSlot.Elevation + BaffleMountSlot.DownSet;
                }
                mdTrayAssembly assy = aBox.GetMDTrayAssembly();
                if(assy != null)
                {
                    BaffleMountSlot = mdGlobals.BaffleMountSlot(assy, out double topz, out double mountht, out bool supbaf);
                    TopZ = topz;
                    //Height = assy.BaffleHeight;
                    BaffleMountHeight = mountht;
                }

            }

            if (aY.HasValue) Y = aY.Value;
        }

   
        internal override void Initialize(mdBoxSubPart aPartToCopy, mdDowncomerBox aBox)
        {
            mdStiffener copy = null;
            if (aPartToCopy != null && aPartToCopy.GetType() == typeof(mdStiffener)) copy = (mdStiffener)aPartToCopy;
            Initialize(aBox,copy );
        }
        
        
        #endregion Constructors

        #region Properties

        internal UHOLE BaffleMountSlot 
        { get; 
            set; 
        }

       

        internal UVECTORS FingerClipPts;

        /// <summary>
        /// the height of the flange that supports the baffle (if required)
        /// </summary>
        public double BaffleMountHeight { get; set; }
       

      

        /// <summary>
        /// the material thickness of the baffle that is attached to the stiffener
        /// </summary>
        public double BaffleThickness { get; set; }

        /// <summary>
        /// the bolt used to install the part
        /// </summary>
        public hdwHexBolt Bolt => base.SmallBolt("Stiffener Bolt",aQuantity : 2 * Quantity);

        public double TopZ { get; set; }

        #endregion Properties


        private double _FlangeWidth;
        /// <summary>
        /// the width of the mounting flange
        /// </summary>
        public double FlangeWidth { get => _FlangeWidth; set => _FlangeWidth = Math.Abs(value); }

        /// <summary>
        /// the distance the mounting bolt hole is inset from the lead eadge of the stiffener moufting flange
        /// </summary>
        public double MountHoleInset => 0.5;

        /// <summary>
        /// //^the height of the part
        /// </summary>
        public override double Height { get => base.Height; set => base.Height = Math.Abs(value); }

        private double _SupplementalDeflectorHeight;
        /// <summary>
        /// the height of the supplemental deflector
        /// </summary>
        public double SupplementalDeflectorHeight { get => _SupplementalDeflectorHeight; set => _SupplementalDeflectorHeight = Math.Abs(value); }
   
        /// <summary>
        /// the height of the part
        /// </summary>
        public double OverallHeight => SupportsBaffle ? Height + BaffleMountHeight + 0.25  : Height;

      
        /// <summary>
        /// the lock washer used to install the part
        /// </summary>
        public hdwLockWasher LockWasher => Bolt.GetLockWasher();
        
        /// <summary>
        ///the maximimum Y ordinate that this stiffner can be placed on its downcomer
        /// </summary>
        public double MaxOrdinate { get ; set ; }

        /// <summary>
        /// the flat washer used to install the part
        /// </summary>
        public hdwFlatWasher Washer => Bolt.GetWasher();
        
        /// <summary>
        ///the width of the downcomer box
        ///equal to the Width property of the parent downcomer

        public override double Width { get => base.Width; set => base.Width = Math.Abs(value); }

        /// <summary>
        ///True if the  stiffener includes a baffle mounting support
        ///ECMD designs
        /// </summary>
        public bool SupportsBaffle => DesignFamily.IsEcmdDesignFamily();

        #region Methods
        public override void SubPart(uopTrayAssembly aAssy, string aCategory = null, bool? bHidden = null)
        {
            if (aAssy == null) return;
            if (aAssy.ProjectFamily != uppProjectFamilies.uopFamMD) return;
            mdTrayAssembly assy = (mdTrayAssembly)aAssy;
            WeldedInPlace = assy.DesignOptions.WeldedStiffeners;
            BaffleThickness = assy.Deck.Thickness;
            base.SubPart(assy);
            BaffleMountSlot = mdGlobals.BaffleMountSlot(assy, out double s1, out double S2, out bool b1);
            Z = assy.Deck.Thickness + 0.625;
            Width = assy.Downcomer().Width;  //  inside 
            TopZ = s1;
            BaffleMountHeight = S2;
            Height = TopZ + 1;


        }

        public override string ToString()
        {
            return $"{uopEnums.Description(uppPartTypes.Stiffener)} [ {X:0.0000},{Y:0.0000} ]";
        }

        ///returns a new object that is an exact copy of this one
        /// </summary>
        /// <returns></returns>
        public mdStiffener Clone() => new mdStiffener(this);

        public override uopPart Clone(bool aFlag = false) => (uopPart)this.Clone();

        public override uopProperties CurrentProperties() { UpdatePartProperties(); return base.CurrentProperties(); }

        public override uopProperty CurrentProperty(string aPropertyName, bool bSupressNotFoundError = false)
        {
            UpdatePartProperties();
            return base.CurrentProperty(aPropertyName, bSupressNotFoundError);
        }

        public  override void SubPart(mdDowncomerBox aBox, string aCategory = null, bool? bHidden = null)
        {
            base.SubPart(aBox, aCategory, bHidden);
            if (aBox == null) return;
         
            BaffleMountSlot = new UHOLE(aBox.BaffleMountSlot)
            {
                Depth = Thickness,
                Tag = "BAFFLE MOUNT",
                Y = Y,

            };
        }

        /// <summary>
        /// executed internally to create the holes collection for the part
        /// </summary>
        /// <param name="aDC"></param>
        /// <param name="aAssy"></param>
        /// <param name="aTag"></param>
        /// <param name="aFlag"></param>
        /// <returns></returns>
        public uopHoleArray GenHoles(mdDowncomerBox aBox, string aTag = "", string aFlag = "")
        => new uopHoleArray(GenHolesV(aBox, aTag, aFlag));

        /// <summary>
        /// executed internally to create the holes collection for the part
        /// </summary>
        /// <param name="aDC"></param>
        /// <param name="aAssy"></param>
        /// <param name="aTag"></param>
        /// <param name="aFlag"></param>
        /// <returns></returns>
        UHOLEARRAY GenHolesV(mdDowncomerBox aBox,  string aTag = "", string aFlag = "")
        {
            UHOLEARRAY _rVal =UHOLEARRAY.Null;
            aBox ??= DowncomerBox;
            if (aBox == null) return _rVal;
            SubPart(aBox);
           
            _rVal.Invalid = false;
            aTag = aTag.ToUpper().Trim();
            aFlag = aFlag.ToUpper().Trim();
            double thk = Thickness;
            double wd = Width;
            double cx = aBox.X;
            if (aTag == "MOUNT" || aTag ==  string.Empty)
            {

                UHOLE aHole =  new UHOLE(aBox.FingerClipHole)
                 {
                     Center = new UVECTOR(cx - 0.5 * wd + 0.5 * thk, Y),
                     Inset = MountHoleInset,
                     Tag = "MOUNT",
                     Depth = thk
                 };

                UHOLES Hls = new UHOLES(aHole, new UVECTORS(false));
                if (aFlag == "LEFT" || aFlag ==  string.Empty) Hls.Centers.Add(aHole.Center);

                if (aFlag == "RIGHT" || aFlag ==  string.Empty)
                {
                    Hls.Centers.Add(new UVECTOR(cx + 0.5 * wd - 0.5 * thk, Y, aValue: 0, aRadius: 0, aInset: MountHoleInset), "RIGHT");
                }
                _rVal.Add(Hls, "MOUNT");
            }
            if (aTag == "BAFFLE MOUNT" || aTag ==  string.Empty)
            {
                if (SupportsBaffle)
                {

                    UHOLE aHole =  new UHOLE(aBox.BaffleMountSlot) { Y = Y} ;
               
                    if (Math.Round(cx, 3) >= 0 || !aBox.DesignFamily.IsStandardDesignFamily())
                    {
                        aHole.Center.X = cx - 0.5 * thk;
                        aHole.Center.X -= aBox.DeckThickness * 0.5;
                    }
                    else
                    {
                        aHole.Center.X = cx + 0.5 * thk;
                        aHole.Center.X += aBox.DeckThickness * 0.5;
                    }
                    UHOLES Hls = new UHOLES(aHole, new UVECTORS(false));
                    Hls.Centers.Add(aHole.Center);

                    _rVal.Add(Hls, "BAFFLE MOUNT");
                }
            }
            return _rVal;
        }


        public override void UpdatePartProperties()
        {
            DescriptiveName = $"Stiffener ({ Width:0.000}) x  {Height:0.000})";
        }

        public override void UpdatePartWeight() =>base.Weight = Weight();
        

        /// <summary>
        ///returns a dxePolygon that is used to draw the layout view of the stiffener
        /// </summary>
        /// <param name="bIncludeFillets"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="rPlane"></param>
        /// <returns></returns>

        public colDXFVectors Stiffener_Vertices(bool bIncludeFillets = false, iVector aCenter = null, double aRotation = 0, dxfPlane rPlane = null)
        => mdPolygons.Stiffener_Vertices(this, bIncludeFillets, aCenter, aRotation, out rPlane);
        
        /// <summary>
        ///returns a dxePolygon that is used to draw the elevation view of the stiffener
        /// </summary>
        /// <param name="bShowObscured"></param>
        /// <param name="bVisiblePartOnly"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <param name="bHoleCenterLines"></param>
        /// <returns></returns>

        public dxePolygon View_Elevation(bool bShowObscured = false, bool bVisiblePartOnly = false, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY", bool bHoleCenterLines = false)
         => mdPolygons.Stiffener_View_Elevation(this, bShowObscured, bVisiblePartOnly, aCenter, aRotation, aLayerName, bHoleCenterLines);

        /// <summary>
        ///returns a dxePolygon that is used to draw the layout view of the stiffener
        /// </summary>
        /// <param name="aDC"></param>
        /// <param name="aAssy"></param>
        /// <param name="bSuppressHoles"></param>
        /// <param name="bIncludeFillets"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <param name="bHoleCenterLines"></param>
        /// <returns></returns>
        public dxePolygon View_Plan(mdDowncomerBox aBox, mdTrayAssembly aAssy, bool bSuppressHoles = false, bool bIncludeFillets = false, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY", bool bHoleCenterLines = false)
       => mdPolygons.Stiffener_View_Plan(this, aBox, aAssy, bSuppressHoles, bIncludeFillets, aCenter, aRotation, aLayerName, bHoleCenterLines);
        

        /// <summary>
        ///returns a dxePolygon that is used to draw the side view of the stiffener
        /// </summary>
        /// <param name="bShowObscured">scale factor to apply to the returned polygon</param>
        /// <param name="bVisiblePartOnly"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <param name="bCenterBaffleMount"></param>
        /// <returns></returns>
        public dxePolygon View_Profile(bool bShowObscured = false, bool bVisiblePartOnly = false, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY", bool bCenterBaffleMount = false,bool bMirrored = false)
       => mdPolygons.Stiffener_View_Profile(this, bShowObscured, bVisiblePartOnly, aCenter, aRotation, aLayerName, bCenterBaffleMount, bMirrored);
   
        /// <summary>
        ///returns the weight of the stiffener in english pounds
        /// </summary>
        /// <param name="aDC"></param>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public new double Weight(mdDowncomerBox aBox = null)
        {
         
            double sArea = 0;
            double thk = Thickness;
            try
            {
                sArea = SupportsBaffle ? View_Elevation().Area - (thk * BaffleMountHeight) - (2 * Height * thk): Width * Height - (2 * Height * thk);
                sArea = sArea + (2 * Height * FlangeWidth) - GenHolesV(aBox).TotalArea();

                return sArea * SheetMetalWeightMultiplier;
            }
            catch (Exception e)
            {
                throw e;
            }
          
        }

        public bool IsEqual(mdStiffener aPart)
        {
            if (aPart == null) return false;

            if (aPart.SupportsBaffle != SupportsBaffle) return false;
            if (aPart.Height != Height) return false;
            if (aPart.Width != Width) return false;

            if (aPart.SupplementalDeflectorHeight != SupplementalDeflectorHeight) return false;
            if (aPart.BaffleThickness != BaffleThickness) return false;
            return aPart.Material.IsEqual(Material);
        }

        public override bool IsEqual(uopPart aPart)
        {
         
            if (aPart == null) return false;
            if (aPart.PartType != PartType) return false;
            return IsEqual((mdStiffener)aPart);
          
        }

        public colDXFVectors Vertices(bool bInclueFillets, iVector aCenter = null, double aRotation = 0)
        {
            return mdPolygons.Stiffener_Vertices(this, bInclueFillets, aCenter, aRotation, out dxfPlane PLN);
        }

        #endregion Methods
    }
}
