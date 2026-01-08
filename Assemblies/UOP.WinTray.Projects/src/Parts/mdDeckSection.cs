using UOP.DXFGraphics;
using System;
using System.Collections.Generic;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Materials;

using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;
using UOP.WinTray.Projects.Interfaces;
using System.Linq;

namespace UOP.WinTray.Projects.Parts
{
    /// <summary>
    /// represents a deck section section in a md tray assembly
    /// a deck section has a collection of sections if it is spliced
    /// </summary>
    public class mdDeckSection : uopPart
    {
        public override uppPartTypes BasePartType => uppPartTypes.DeckSection;
     
        #region Constructors
        public mdDeckSection() : base(uppPartTypes.DeckSection, uppProjectFamilies.uopFamMD, "", "", true) => Init();
          
        public mdDeckSection(uopSectionShape aBaseShape, mdDeckPanel aPanel) : base(uppPartTypes.DeckSection, uppProjectFamilies.uopFamMD, "", "", true)
        {
            Init( aPanel: aPanel);
            BaseShape = aBaseShape;
            if(aBaseShape != null)
            {
                SubPart(aBaseShape.MDTrayAssembly);
            }
            
        }

        internal mdDeckSection(mdDeckSection aPartToCopy) : base(uppPartTypes.DeckSection, uppProjectFamilies.uopFamMD, "", "", true) => Init(aPartToCopy);

        private void Init(mdDeckSection aPartToCopy = null, mdDeckPanel aPanel = null)
        {
           
       
         
            _PanelRef = null;
            
            UniqueIndex = -1;
            UniqueID = -1;
            ManholeID = 0;
            TotalRequiredSlotCount = 0;
            Height = 0;
            Width = 0;
            Depth = 0;
            Rotation = 0;
            Z = 0;
            MechanicalArea = 0;
            PanelOccuranceFactor = 1;
            Quantity = 1;
            Inverted = false;
            JoggleAngleHeight = 1;
            _AltSlotZone = null;
            if (aPartToCopy != null)
            {

                Copy(aPartToCopy);

                BaseShape = uopSectionShape.CloneCopy(aPartToCopy.BaseShape);
                PercentOpen = aPartToCopy.PercentOpen;
              

                UniqueIndex = aPartToCopy.UniqueIndex;
                UniqueID = aPartToCopy.UniqueID;
                ManholeID = aPartToCopy.ManholeID;
                TotalRequiredSlotCount = aPartToCopy.TotalRequiredSlotCount;
                Height = aPartToCopy.Height;
                Width = aPartToCopy.Width;
             
                Rotation = aPartToCopy.Rotation;
                SetCoordinates(aPartToCopy.X, aPartToCopy.Y, aPartToCopy.Z);
                _MechanicalArea = aPartToCopy._MechanicalArea;
                PanelOccuranceFactor = aPartToCopy.OccuranceFactor;
                Inverted = aPartToCopy.Inverted;
                SheetMetalStructure = aPartToCopy.SheetMetalStructure;
                if (aPartToCopy._AltSlotZone != null)
                    _AltSlotZone = new mdSlotZone(_AltSlotZone);

                aPanel ??= aPartToCopy.DeckPanel;
                
            }
            if (aPanel != null) _PanelRef = new WeakReference<mdDeckPanel> (aPanel);
          

        }
        #endregion Constructors

        #region Relationships

        public override bool IsVirtual { get => BaseShape == null ? true : BaseShape.IsVirtual; set { } }

        private WeakReference<mdDeckSection> _ParentRef;
        private WeakReference<mdDeckSection> _ChildRef;
        public void AssociateToParent(mdDeckSection aParent)
        {
            _ChildRef = null;
            if (aParent == null)
            {
                _ParentRef = null;
                IsVirtual = false;
                return;
            }

            _ParentRef = new WeakReference<mdDeckSection>(aParent);
            aParent._ChildRef = new WeakReference<mdDeckSection>(this);

            //IsVirtual = true;
        }

        public void AssociateToChild(mdDeckSection aChild)
        {
            _ParentRef = null;
            if (aChild == null)
            {
                _ChildRef = null;
                IsVirtual = false;
                return;
            }
            _ChildRef = new WeakReference<mdDeckSection>(aChild);
            aChild._ParentRef = new WeakReference<mdDeckSection>(this);
            // IsVirtual = false;
        }

        public mdDeckSection Parent
        {
            get

            {
                //IsVirtual = false;
                if (_ParentRef == null) return null;
                //IsVirtual = _ParentRef.TryGetTarget(out mdDeckSection _rVal);
                if (!_ParentRef.TryGetTarget(out mdDeckSection _rVal)) _ParentRef = null;
                //if (!IsVirtual) _ParentRef = null;
                return _rVal;
            }

            set => AssociateToParent(value);

        }

        public mdDeckSection Child
        {
            get
            {
                if (_ChildRef == null) return null;

                if (!_ChildRef.TryGetTarget(out mdDeckSection _rVal)) _ChildRef = null;
                return _rVal;
            }
            set => AssociateToChild(value);
        }

        public void CopyParentProperties()
        {
            mdDeckSection myparent = Parent;
            if (myparent == null) return;
            this.PropValsCopy(myparent.ActiveProperties, aSkipList: new List<string>() { "X" });
        }

        #endregion Relationships


        #region Properties

        public int PanelOccuranceFactor { get; set; }

        public override string NodeName 
        {
            get
            {
                string suffix = string.Empty;
                //sec.Quantity = sec.Instances.Count + 1;
                if (IsHalfMoon)
                    suffix = " (Moon)";
                else if (IsManway)
                    suffix = " (Manway)";
                

            return $"Deck Section {PartNumber}{suffix}";
            }
            set {} 
        }

        public double FitWidth => BaseShape == null ? 0 : BaseShape.FitWidth;

        /// <summary>
        /// the number of deck sections that reside on this sections panel
        /// </summary>
        public int PanelSectionCount => BaseShape == null ? 1 : BaseShape.PanelSectionCount;

        internal USHAPE PhysicalBoundaryV { get => new USHAPE(BaseShape); }


        public uopVectors ExtentPoints => BaseShape == null ? uopVectors.Zero : BaseShape.Segments.ExtentPoints();

        public uopRectangle Bounds => BaseShape == null ? new uopRectangle() : BaseShape.Bounds;

        internal UVECTORS BPSites => BaseShape == null ? UVECTORS.Zero : new UVECTORS( BaseShape.BPSites);
        


        public override uopRingRange RingRange { get => base.RingRange; set { base.RingRange = value; base.RingRanges = value == null ? null : new uopRingRanges(base.RingRange); } }


        public bool HasJoggleAngle
        { get => TopSpliceType == uppSpliceIndicators.JoggleMale || TopSpliceType == uppSpliceIndicators.TabFemale || BottomSpliceType == uppSpliceIndicators.JoggleMale || BottomSpliceType == uppSpliceIndicators.TabFemale; }

        public bool LapsRing  => BaseShape == null ? false : BaseShape.LapsRing;
        public  bool LapsDivider => BaseShape == null ? false : BaseShape.LapsDivider;

        public bool LapsBeam  => BaseShape == null ? false : DesignFamily.IsBeamDesignFamily() && LapsDivider;
        

        public uopShape MechanicalBounds => new uopShape(MechanicalBoundsV, aTag: Handle);

        internal USHAPE MechanicalBoundsV => BaseShape == null ? USHAPE.Null : BaseShape.MechanicalBounds.HasValue ?  BaseShape.MechanicalBounds.Value : USHAPE.Null;
        
    

        public uopVectors BaseVertices => BaseShape == null ? uopVectors.Zero : BaseShape.Vertices; 

        public override uopVector Center 
        { get 
            {
                uopVector _rVal = BaseShape == null ? uopVector.Zero : BaseShape.Center;
                _rVal.Tag = Handle;
                return  _rVal; 
            } 
            set 
            {
                if (BaseShape == null || value == null) return;
                BaseShape.Translate ( value - BaseShape.Center);
            }
        }
      
        /// <summary>
        /// the center used to define the section
        /// </summary>
        public override dxfVector CenterDXF { get => Center.ToDXFVector(false, false, Z, Handle); set => Center = new uopVector(value); }


        private double _MechanicalArea;
        public double MechanicalArea
        {
            get
            {
                if (_MechanicalArea <= 0)
                    _MechanicalArea = MechanicalBounds.Value;
                return _MechanicalArea;
            }
            set => _MechanicalArea = value;
        }

        internal URECTANGLE _MechanicalLimits;
        internal URECTANGLE MechanicalLimits  => BaseShape == null ? URECTANGLE.Null : BaseShape.MechanicalBounds.HasValue ? BaseShape.MechanicalBounds.Value.Limits : URECTANGLE.Null;

        private dxePolygon _Perimeter;

        public dxePolygon Perimeter { get => GeneratePerimeter(null, false); internal set => _Perimeter = value; }

    
        public override dxfPlane Plane => new dxfPlane(new dxfVector(X, Y, Z));


        public bool TruncatedFlangeLine(bool? bGetTop) => BaseShape == null ? false : BaseShape.TruncatedFlangeLine(bGetTop);

    
        public bool SupportsManway (out bool rOnTop)
        {
            rOnTop = false;
             return BaseShape == null ? false : BaseShape.SupportsManway(out rOnTop);
        }


   

        public List<double> TabOrdinates => BaseShape == null ? new List<double>() : BaseShape.TabOrdinates;

        public uopHole TabSlot
        {
            get
            {
                uopHole _rVal = new uopHole(new UHOLE(aDiameter: 0.375, 0, 0, aLength: 2.3625, aDepth: Thickness));
                _rVal.Diameter = (_rVal.Depth < 0.115) ? 0.375 : 0.472;
                if (PanelIndex == 1) _rVal.Rotation = 90;
                return _rVal;
            }
        }
        public uopHoles TabSlots => new uopHoles(new UHOLES(new UHOLE( TabSlot), new UVECTORS( SlotPoints)));

        public uppSpliceIndicators TopSpliceType { get => BaseShape == null ? uppSpliceIndicators.ToRing : BaseShape.TopSpliceType; }

        public bool SplicedOnTop => BaseShape == null ? false : BaseShape.SplicedOnTop;
        public bool SplicedOnBottom => BaseShape == null ? false : BaseShape.SplicedOnBottom;

        public uppSpliceIndicators BottomSpliceType { get => BaseShape == null ? uppSpliceIndicators.ToRing : BaseShape.BottomSpliceType; }




        /// <summary>
        /// the panel index coupled with the section index like 1,2
        /// </summary>
        /// <remarks>If there are multible panel sections (MultiPanel) the panels section index is include as the middle item</remarks>
        public string Handle => BaseShape == null ?"0,0" : BaseShape.Handle ;

        public override int SectionIndex { get { return BaseShape == null ? 0 : BaseShape.SectionIndex; } set { } }

        public bool Inverted { get; set; }

        /// <summary>
        /// returns True if the section is part of a center panel
        /// </summary>
        public bool IsCenterSection { get { return BaseShape == null ? false : BaseShape.IsCenterSection; }  }



        private uopSectionShape _BaseShape;
        public uopSectionShape BaseShape { get => _BaseShape; set => _BaseShape = value; }
        public uopPanelSectionShape ParentShape { get => BaseShape == null ? null : BaseShape.ParentShape; }
        /// <summary>
        /// returns true if the section is not a manway, and is not a half moon, and does not touch the ring
        /// </summary>
        public bool IsFieldSection => !IsHalfMoon && !LapsRing && !IsManway;

        /// <summary>
        /// True if the section is part of the moon panel
        /// </summary>
        public bool IsHalfMoon => BaseShape == null ? false : BaseShape.IsHalfMoon;
        /// <summary>
        /// a flag used to idincate the the section is a manway
        /// </summary>
        public bool IsManway => BaseShape == null ? false : BaseShape.IsManway;

        
        public double JoggleAngleHeight { get => BaseShape == null ? 0 : BaseShape.JoggleAngleHeight; set { if (BaseShape != null) BaseShape.JoggleAngleHeight = value;  } }

        /// <summary>
        /// the bolt used to install the part
        /// </summary>
        public hdwHexBolt JoggleBolt => base.SmallBolt(true, 1, "Joggle Bolt");

      

        public double LeftDowncomerClearance { get => BaseShape == null ? 0 : BaseShape.LeftDowncomerClearance; }

        /// <summary>
        /// The index of the downcomer that bounds the section on the left
        /// </summary>
        public int LeftDowncomerIndex { get => BaseShape == null ? 0 : BaseShape.LeftDowncomerIndex; }

        /// <summary>
        /// the lock washer used to install the part
        /// </summary>
        public hdwLockWasher LockWasher => JoggleBolt.GetLockWasher();



        /// <summary>
        /// the handle of the manway this section is associated to
        /// </summary>
        public string ManwayHandle => BaseShape == null ? string.Empty : BaseShape.ManwayHandle;


        public double Right => BaseShape == null ? 0 : BaseShape.Right;

        public double Top => BaseShape == null ? 0 : BaseShape.Top;

        public double Left => BaseShape == null ? 0 : BaseShape.Left;

        public double Bottom => BaseShape == null ? 0 : BaseShape.Bottom;
        /// <summary>
        /// the nut used to install the 
        /// </summary>

        public hdwHexNut Nut => JoggleBolt.GetNut();

        internal URECTANGLE Limits => BaseShape == null ? new URECTANGLE(0, 0, 0, 0) : BaseShape.BoundsV;

        /// <summary>
        /// the fraction of the parent panels mechanical area that this section represents
        /// </summary>
        public double MechanicalPanelFraction => BaseShape == null ? 0 : BaseShape.MechanicalPanelFraction;
          

        /// <summary>
        /// the index of the section if it exists on the left side of the centerline
        /// </summary>
        public int OpposingIndex
        {
            get
            {
                mdTrayAssembly aAssy = GetMDTrayAssembly();
                return (aAssy == null) ? 0 : uopUtils.OpposingIndex(PanelIndex, aAssy.PanelCount);
            }
        }

        /// <summary>
        /// the X ordinate for vertical splices and the Y ordinte for horizontal splices
        /// </summary>
        public double Ordinate => IsHalfMoon ? Math.Round(X, 4) : Math.Round(Y, 4);


        /// <summary>
        /// the gross area of the panel section
        /// the area of the boundary polyline
        /// </summary>
        public double PanelArea => BaseShape == null ? 0 : BaseShape.Area;


        private WeakReference<mdDeckPanel> _PanelRef;

        /// <summary>
        /// The deck panel that owns this section
        /// </summary>
        public new mdDeckPanel DeckPanel
        {
            get
            {
                if (_PanelRef == null) return null;
                if(!_PanelRef.TryGetTarget(out mdDeckPanel _rVal)) { _PanelRef = null; }
                return _rVal;
            }
            set
            {
                if(value == null) { _PanelRef = null; return; }
                _PanelRef = new WeakReference<mdDeckPanel>(value);
            }
        }

      

        /// <summary>
        /// The index of the deck panel that owns this section
        /// </summary>
        public override int PanelIndex { get   { return BaseShape == null ? 0 : BaseShape.PanelIndex; } set { } }
        public int PanelSectionIndex => BaseShape == null ? 1 : BaseShape.PanelSectionIndex;

        public int Quadrant
        {
            get
            {

                if (Math.Round(X, 1) >= 0)
                {
                    return (Math.Round(Y, 1) >= 0) ? 1 : 4;
                }
                else
                {
                    return (Math.Round(Y, 1) >= 0) ? 2 : 3;
                }
            }
        }

        /// <summary>
        /// the radius of the arc section of the deck section section
        /// </summary>
        public override double Radius { get => BaseShape == null? 0 :BaseShape.DeckRadius; set {; } }

        public int RequiredSlotCount
        {
            get
            {
                int occr = OccuranceFactor;
                return (occr > 1) ? Convert.ToInt32(uopUtils.RoundTo(TotalRequiredSlotCount / occr, dxxRoundToLimits.One, true)) : TotalRequiredSlotCount;
            }
        }

        /// <summary>
        /// true if this section is for an ECMD tray and requires slots
        /// </summary>
        public bool RequiresSlotting => DesignFamily.IsEcmdDesignFamily();

        public double RightDowncomerClearance { get => BaseShape == null ? 0 : BaseShape.RightDowncomerClearance; }

        /// <summary>
        /// The index of the downcomer that bounds the section on the right
        /// </summary>
        public int RightDowncomerIndex { get => BaseShape == null ? 0 : BaseShape.RightDowncomerIndex; }


        public override uopInstances Instances 
        { get 
            { 
                
                uopInstances _rVal = BaseShape != null ? BaseShape.Instances : base.Instances;
                _rVal.Owner = null;
                _rVal.BaseRotation = Rotation; 
                _rVal.BasePt.PartIndex = PanelIndex;
                _rVal.Owner = this;
                return _rVal; 
            } 
            set { if (BaseShape == null) base.Instances = value; else BaseShape.Instances = value; } 
        }

        /// <summary>
        /// the number of times a section occurs in a single tray assembly
        /// </summary>
        public override int OccuranceFactor  {  get => BaseShape == null ? 1 : BaseShape.OccuranceFactor; set {}  }
     

        /// <summary>
        /// the radius of the ring that supports this panel
        /// </summary>
        public new double RingRadius { get => BaseShape == null ? 0 : BaseShape.RingRadius; }

        public double TrimRadius { get => BaseShape == null ? 0 : BaseShape.TrimRadius; }

        /// <summary>
        /// Flag indicating that this section is the only section of its parent panel (no splices)
        /// </summary>
        public bool SingleSection => BaseShape == null ? true : !BaseShape.HasSplices; 

        /// <summary>
        /// one of two possible splices
        /// </summary>
        public string Splice1Handle => BaseShape == null ? string.Empty : BaseShape.Splice1Handle;

        /// <summary>
        /// one of two possible splices
        /// </summary>
        public string Splice2Handle => BaseShape == null ? string.Empty : BaseShape.Splice2Handle;

      
        /// <summary>
        /// the number of slots requied on this section
        /// </summary>
        public int TotalRequiredSlotCount { get; set; }

        /// <summary>
        /// the index of the section that relates it to other sections that are identical to this one
        /// </summary>
        public override int UniqueIndex { get; set; }

        /// <summary>
        /// the the sub index of the parts unique settings
        /// one means this is the first instance greater than one means
        /// it was found to be equal to another previous part
        /// </summary>
        public  new int UniqueID { get; set; }


        /// <summary>
        /// the width of the deck section as viewed from above  (includes all splice details)
        /// </summary>
        public override double Width { get => BaseShape == null ? 0 : BaseShape.SimplePerimeter.Width(); set { } }


        /// <summary>
        /// the height of the deck section as viewed from above (includes all splice details)
        /// </summary>
        public override double Height { get => BaseShape == null ? 0 : BaseShape.SimplePerimeter.Height(); set { } }

        /// <summary>
        /// the depth of the deck section as viewed in elevation  (includes all splice details)
        /// </summary>
        public double Depth { get => BaseShape == null ? Thickness : BaseShape.Depth; set {  } }

        public uopVectors SlotPoints  => BaseShape== null ? null : BaseShape.SlotPoints ;


        /// <summary>
        /// returns true if either of the splices are truncated by the ring or the beam/divider
        /// </summary>
        public bool TruncatedFlange => BaseShape == null ? false : BaseShape.TruncatedFlangeLine();

        /// <summary>
        /// the splice that defines the sections shape at the top or right for moon sections
        /// </summary>
        public uopDeckSplice TopSplice  => BaseShape?.TopSplice;

        /// <summary>
        /// the splice that defines the sections shape at the bottom or left for moon sections
        /// </summary>
        public uopDeckSplice BottomSplice => BaseShape?.BottomSplice;
        
        /// <summary>
        /// the info about the the downcomer that defines the section on the left
        /// </summary>
        public DowncomerInfo LeftDowncomerInfo => BaseShape?.LeftDowncomerInfo;

        /// <summary>
        /// the info about the the downcomer that defines the section on the right
        /// </summary>
        public DowncomerInfo RightDowncomerInfo => BaseShape?.RightDowncomerInfo;

        /// <summary>
        /// returns true if the parent panel has more than one non-virtual section
        /// </summary>
        public bool MultiPanel => BaseShape == null ? false : BaseShape.MultiPanel;
        #endregion Properties

        #region Methods

        public uppSpliceAngleTypes SpliceAngleType(bool bTop = false)
        {
            if (bTop && TopSplice != null) return TopSplice.SpliceAngleType;
            if (!bTop && BottomSplice != null) return BottomSplice.SpliceAngleType;
            return uppSpliceAngleTypes.Undefined;
        }
        public override mdTrayAssembly GetMDTrayAssembly(mdTrayAssembly aAssy = null)
        {
            if (aAssy == null) return aAssy;
            mdTrayAssembly _rVal = BaseShape == null ? null : BaseShape.MDTrayAssembly;
            if (_rVal != null) return _rVal;

            if (_rVal == null)
            {

                _rVal = base.GetMDTrayAssembly();
                if (_rVal != null && BaseShape != null) BaseShape.TrayAssembly = _rVal;
            }
            return _rVal;
        }

        public override uopProperties CurrentProperties() { UpdatePartProperties(); return base.CurrentProperties(); }

        public override void ClearInstances() { if (BaseShape != null) BaseShape.Instances = null; else base.ClearInstances(); }

        public override void SetCoordinates(double? aX = null, double? aY = null, double? aZ = null)
        {
            if (aX.HasValue) X = aX.Value; if (aY.HasValue) Y = aY.Value; if (aZ.HasValue) Z = aZ.Value;
        }

        public void Mirror(double? aX, double? aY)
        {
            if (!aX.HasValue && !aY.HasValue) return;

            uopInstances insts = Instances;
            uopVectors istpts = insts.MemberPoints(Center, false);

            BaseShape.RectifySplices(out uopDeckSplice topsplice, out uopDeckSplice botsplice);
            //double dx = aX.HasValue ? X - aX.Value : 0;
            double dy = aY.HasValue ? Y - aY.Value : 0;
            bool flip = false;
            if (aY.HasValue)
            {
                flip = true; // aY.Value == PanelV.Y;
     
            }
            

            Center.Mirror(aX, aY);
            SlotPoints.Mirror(aX, aY);

            if (topsplice!= null) topsplice.Mirror(aX, aY);
            if (botsplice != null) botsplice.Mirror(aX, aY);

            BPSites.Mirror(aX, aY);
            if (BaseShape != null) BaseShape.Mirror(aX, aY);
            if (flip)
            {
                
                //swap the splices
                uopDeckSplice s1 = botsplice;
                botsplice = topsplice;
                topsplice = s1;
                if (topsplice != null)
                {
                    if (topsplice.SupportsManway)
                    {
                        if (string.Compare(topsplice.ManTag, "TOP", true) == 0)
                            topsplice.ManTag = "BOTTOM";
                        else if (string.Compare(topsplice.ManTag, "BOTTOM", true) == 0)
                            topsplice.ManTag = "TOP";

                    }
      
                    //mdSplicing.SetSpliceMFTags(topsplice, true, GetMDTrayAssembly());
                }

                if (botsplice != null)
                {

                    if (botsplice.SupportsManway)
                    {
                        if (string.Compare(botsplice.ManTag, "TOP", true) == 0)
                            botsplice.ManTag = "BOTTOM";
                        else if (string.Compare(botsplice.ManTag, "BOTTOM", true) == 0)
                            botsplice.ManTag = "TOP";
                    }
                
                }

                if (PanelSectionCount > 0 && SectionIndex > 0)
                {
                    SectionIndex = uopUtils.OpposingIndex(SectionIndex, PanelSectionCount);
                }
            }


            uopVector cpt = Center;
            
            istpts.Mirror(aX, aY);
            if (flip)
            {
                insts.BaseRotation += 180;
            }


                for (int i = 1; i <= insts.Count; i++)
            {
                uopInstance inst = insts.Item(i);
                uopVector u1 = istpts.Item(i);

                inst.DX = u1.X - cpt.X;
                inst.DY = u1.Y - cpt.Y;
                if (flip)
                {
                    inst.Rotation += 180;
                }
            }

            Instances = insts;
        }

   
        /// <summary>
        /// the fasteners (clips or clamps) that are used to attach the section if it is a manway
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aTopSideOnly"></param>
        /// <param name="bTrayWide"></param>
        /// <returns></returns>
        public colUOPParts ManwayFasteners(mdTrayAssembly aAssy, bool aTopSideOnly = false, bool bTrayWide = false, colUOPParts aCollector = null)
        {
            colUOPParts _rVal = aCollector ?? new colUOPParts();
            _rVal.SubPart(this);
            if (!IsManway) return _rVal;
            aAssy ??= GetMDTrayAssembly(aAssy);
            if (aAssy == null) return _rVal;

            uopHoleArray sHoles = GenHoles(aAssy, "MANWAY");

            uopManwayClip aClip = null;
            uopManwayClamp aClamp = null;


            double aZ = Thickness;
            bool bClips = aAssy.DesignOptions.UseManwayClips;


            int ocf = OccuranceFactor;



            if (bClips)
            { aClip = aAssy.ManwayClip.Clone(); }
            else
            { aClamp = aAssy.ManwayClamp.Clone(); }

            foreach (uopHoles aHls in sHoles)
            {

                for (int i = 1; i <= aHls.Count; i++)
                {
                    uopHole aHole = aHls.Item(i);
                    if (bClips)
                    {
                        aClip.CenterDXF = aHole.CenterDXF;
                        aClip.Z = aZ;
                        aClip.Angle = aHole.Rotation;

                        aClip.OccuranceFactor = ocf;
                        aClip.Tag = Handle;
                        aClip.Flag = aHole.Flag;
                        _rVal.Add(aClip, true);
                    }
                    else
                    {
                        aClamp.CenterDXF = aHole.CenterDXF;
                        aClamp.Z = aZ;
                        aClamp.Angle = aHole.Rotation;
                        aClamp.OccuranceFactor = ocf;
                        aClamp.Tag = Handle;
                        aClamp.Flag = aHole.Flag;
                        _rVal.Add(aClamp, true);
                    }

                    //if (bTrayWide && ocf > 1)
                    //{
                    //    if (bClips)
                    //    {
                    //        uopManwayClip bClip = aClip.Clone();
                    //        bClip.X = -aClip.X;
                    //        bClip.Y = -aClip.Y;
                    //        if (aClip.Angle == 0)
                    //        {
                    //            bClip.Angle = 180;
                    //        }
                    //        else if (aClip.Angle == 180)
                    //        {
                    //            bClip.Angle = 0;
                    //        }
                    //        else if (aClip.Angle == 90)
                    //        {
                    //            bClip.Angle = 270;
                    //        }
                    //        else if (aClip.Angle == 270)
                    //        {
                    //            bClip.Angle = 90;
                    //        }
                    //        _rVal.Add(bClip);
                    //    }
                    //    else
                    //    {
                    //        uopManwayClamp bClamp = aClamp.Clone();
                    //        bClamp.X = -aClamp.X;
                    //        bClamp.Y = -aClamp.Y;
                    //        if (aClamp.Angle == 0)
                    //        {
                    //            bClamp.Angle = 180;
                    //        }
                    //        else if (aClamp.Angle == 180)
                    //        {
                    //            bClamp.Angle = 0;
                    //        }
                    //        else if (aClamp.Angle == 90)
                    //        {
                    //            bClamp.Angle = 270;
                    //        }
                    //        else if (aClamp.Angle == 270)
                    //        {
                    //            bClamp.Angle = 90;
                    //        }
                    //        _rVal.Add(bClamp);
                    //    }
                    //}

                    if (!bClips && !aTopSideOnly)
                    {
                        aClamp.BottomSide = true;
                        aClamp.Z = -2 * aZ;
                        _rVal.Add(aClamp, true);
                        //if (bTrayWide && ocf > 1)
                        //{
                        //    uopManwayClamp bClamp = aClamp.Clone();
                        //    bClamp.X = -aClamp.X;
                        //    bClamp.Y = -aClamp.Y;
                        //    if (aClamp.Angle == 0)
                        //    {
                        //        bClamp.Angle = 180;
                        //    }
                        //    else if (aClamp.Angle == 180)
                        //    {
                        //        bClamp.Angle = 0;
                        //    }
                        //    else if (aClamp.Angle == 90)
                        //    {
                        //        bClamp.Angle = 270;
                        //    }
                        //    else if (aClamp.Angle == 270)
                        //    {
                        //        bClamp.Angle = 90;
                        //    }
                        //    _rVal.Add(bClamp, true);
                        //}
                    }


                }
            }

            if (!bTrayWide & aAssy.IsSymmetric)
            {

            }

            _rVal.SubPart(this);
            return _rVal;
        }


        public override string ToString()
        {
            string _rVal = $"{uopEnums.Description(PartType) } { Handle}";
            int idx = PartIndex;

            if (idx > 0) _rVal += $"[{  idx }]";
            _rVal += $"OCCR: {OccuranceFactor}";
            return _rVal;
        }
        
        public dxfRectangle BoundingRectangle(dxfPlane aPlane = null) => BaseShape == null ? null : BaseShape.Vertices.ToDXFVectors().BoundingRectangle(aPlane);

        

        public uopVectors PerimeterPts( bool bIncludeBPSites = false, bool bIncludeSlotSites = false, bool bInvert = false, double aXOffset = 0, double aYOffset = 0)
        {
            return BaseShape == null ? uopVectors.Zero : BaseShape.PerimeterPts(bIncludeBPSites, bIncludeSlotSites, bInvert, aXOffset, aYOffset);
        }

        public string TopSpliceTypeName()
        {
            uopDeckSplice rSplice = TopSplice;
            return rSplice != null ? uopDeckSplice.IndicatorName(TopSpliceType, rSplice.SupportsManway) : uopDeckSplice.IndicatorName(TopSpliceType);
        }

     
        public string BottomSpliceTypeName() 
        {
            uopDeckSplice rSplice = BottomSplice;
            return (rSplice != null) ? uopDeckSplice.IndicatorName(BottomSpliceType, rSplice.SupportsManway) : uopDeckSplice.IndicatorName(BottomSpliceType);
        }

        public List<uopFlangeLine> FlangeLines() =>  BaseShape== null ? new List<uopFlangeLine>(): BaseShape.FlangeLines();

        public dxePolygon GeneratePerimeter(mdTrayAssembly aAssy, bool bRegen = false, bool bVerbose = true)

        {
            if (_Perimeter == null || bRegen)
            {

                aAssy ??= GetMDTrayAssembly();
                _Perimeter = new dxePolygon( BaseShape.UpdatePerimeter(true,aAssy, bVerbose:bVerbose)); //  mdSectionGenerator.CreateSectionPerimeter(this, aAssy);
                
                
            }
            return _Perimeter;
        }
       
        public dxePolygon View_Profile(mdTrayAssembly aAssy, bool bLongSide = false, bool bIncludePromoters = false, bool bSuppressHoles = false, bool bIncludeBolts = false, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY")
         => mdPolygons.DeckSection_View_Profile(this, aAssy, bLongSide, bIncludePromoters, bSuppressHoles, bIncludeBolts, aCenter, aRotation, aLayerName);
           
       
        /// <summary>
        ///returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>
        public mdDeckSection Clone() => new mdDeckSection(this);

        public override uopPart Clone(bool aFlag = false) => (uopPart)this.Clone();


        public override uopProperty CurrentProperty(string aPropertyName, bool bSupressNotFoundError = false)
        { UpdatePartProperties(); return base.CurrentProperty(aPropertyName, bSupressNotFoundError); }
        /// <summary>
        /// returns True if the section will fit through the passed manhole diameter
        /// If the manole ID passed <= 0 then then sections parent tray assemblies mahole ID (less 0.5 inches) is used.
        /// if the passed splices and sections are nothing then the sections parent tray asemmblies current section and splices are used.
        /// See FitsThruCircle for the calcs.
        /// </summary>
        /// <param name="aAssy">the manhole diameter to test</param>
        /// <param name="rClearance">the splices to use</param>
        /// <param name="rManId">the sections to use</param>
        /// <returns></returns>
        public bool FitsThroughManhole(mdTrayAssembly aAssy, ref double ioManID) { if (aAssy != null && ioManID <= 0) ioManID = aAssy.ManholeID; return uopUtils.FitsThruCircle(ioManID, FitWidth, FitWidth, Depth,0.5); }
        

        /// <summary>
        /// returns True if the section will fit through the passed manhole diameter
        /// If the manole ID passed <= 0 then then sections parent tray assemblies mahole ID (less 0.5 inches) is used.
        /// if the passed splices and sections are nothing then the sections parent tray asemmblies current section and splices are used.
        /// See FitsThruCircle for the calcs.
        /// </summary>
        /// <param name="aAssy">the manhole diameter to test</param>
        /// <param name="rClearance">the splices to use</param>
        /// <param name="rManId">the sections to use</param>
        /// <returns></returns>
        public bool FitsThroughManhole(mdTrayAssembly aAssy, ref double ioManID , out double rClearance )
        {

            rClearance = 0;
            if (ioManID <= 0)
            {
               aAssy ??= GetMDTrayAssembly(aAssy);
                if (aAssy == null) return true;
                ManholeID = aAssy.ManholeID;
                ioManID = ManholeID;
            }

            return uopUtils.FitsThruCircle(ioManID, FitWidth, FitWidth, Depth, out rClearance);
        }
        
        /// <summary>
        /// executed internally to create the holes in the section
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aTag"></param>
        /// <param name="aFlag"></param>
        /// <param name="bTrayWide"></param>
        /// <returns></returns>
        public uopHoleArray GenHoles(mdTrayAssembly aAssy = null, string aTag = "", string aFlag = "", bool bTrayWide = false) => new uopHoleArray(GenHolesV(aAssy, aTag, aFlag, bTrayWide));

        public override uopHoleArray HoleArray(uopTrayAssembly aAssy = null, string aTag = null)
            => GenHoles(aAssy != null && aAssy.ProjectFamily == uppProjectFamilies.uopFamMD ? (mdTrayAssembly)aAssy : null, aTag);

        public void GetSplices(out uopDeckSplice rTopSplice, out uopDeckSplice rBottomSplice)  {  rTopSplice = null; rBottomSplice = null;  if (BaseShape != null)  BaseShape.GetSplices(out rTopSplice, out rBottomSplice);  }

        /// <summary>
        /// executed internally to create the holes in the section
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aTag"></param>
        /// <param name="aFlag"></param>
        /// <param name="bTrayWide"></param>
        /// <returns></returns>
        internal UHOLEARRAY GenHolesV(mdTrayAssembly aAssy = null, string aTag = "", string aFlag = "", bool bTrayWide = false)
        {

            aAssy ??= GetMDTrayAssembly();

            UHOLEARRAY _rVal = BaseShape == null ? UHOLEARRAY.Null : BaseShape.GenHoles(aAssy,aTag,aFlag,bTrayWide);
            return _rVal;
        }


        /// <summary>
        /// returns true if the passed section is mechanically identical to this one
        /// </summary>
        /// <param name="aAssy">the parent tray assembly</param>
        /// <param name="aSection">the section to compare to this one</param>
        /// <returns></returns>
        public override bool IsEqual(uopPart aPart)
        {
            if (aPart == null) return false;
            if (aPart.GetType() != typeof(mdDeckSection)) return false;    //'aPart.PartType != uppPartTypes.DeckSection) return false;
            if (aPart.ProjectFamily != uppProjectFamilies.uopFamMD) return false;
            return CompareTo((mdDeckSection)aPart);
        }
        /// <summary>
        /// returns true if the passed section is mechanically identical to this one
        /// </summary>
        public bool CompareTo(mdDeckSection aSection)
        {
            if (aSection == null) return false;
           mdTrayAssembly aAssy = GetMDTrayAssembly();
            return mdSection_Generator.SectionsCompare(this, aSection, aAssy, null, true, out bool INVEQ);
        }

        /// <summary>
        /// the downcomer that bounds the section on the left
        /// </summary>
        public mdDowncomer LeftDowncomer(mdTrayAssembly aAssy = null) => GetMDDowncomer(aAssy,null, LeftDowncomerIndex);

        
        /// <summary>
        /// the downcomer that bounds the section on the right side
        /// </summary>
        public mdDowncomer RightDowncomer(mdTrayAssembly aAssy = null) => GetMDDowncomer(aAssy, null, RightDowncomerIndex);

        public List<uopRingClipSegment> RingClipSegments( ) => BaseShape == null ? new List<uopRingClipSegment>() : BaseShape.RingClipSegments(out _);

        public uopVectors RingClipCenters(out List<uopRingClipSegment> rSegments,  out uopShape rRingClipBounds, double? aSpacing = null) { rSegments = new List<uopRingClipSegment>(); rRingClipBounds = null; return BaseShape == null ? uopVectors.Zero : BaseShape.RingClipCenters(out rSegments, out rRingClipBounds, aSpacing); }

        /// <summary>
        /// used as the basis for creating the complex perimeters
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="bIncludePromoters"></param>
        /// <returns></returns>
        public dxePolygon SimplePerimeter( bool bIncludePromoters = false)
        {


            mdTrayAssembly aAssy = GetMDTrayAssembly();
            if (aAssy != null && bIncludePromoters && !aAssy.DesignOptions.HasBubblePromoters) bIncludePromoters = false;
            dxePolygon _rVal = new dxePolygon(BaseShape.SimplePerimeter )
            {
                Tag = Handle
            };
            if (bIncludePromoters && aAssy.DesignOptions.HasBubblePromoters)
            {
                UVECTORS BPs = BPSites;
                for (int i = 1; i <= BPs.Count; i++)
                {
                    UVECTOR aBP = BPs.Item(i);
                    _rVal.AdditionalSegments.AddArc(aBP.X, aBP.Y, mdGlobals.gsMDBubblePromoterDiameter / 2.0);
                }
            }
            if (IsManway)
            {
                _rVal.Flag = "MANWAY";
            }
            _rVal.InsertionPt.SetCoordinates(X, Y);
            return _rVal;
        }
        
        /// <summary>
        /// the area of the section that contains slots
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public mdSlotZone SlotZone(mdTrayAssembly aAssy)
        {
            if (_AltSlotZone != null)
                return _AltSlotZone;
           aAssy ??= GetMDTrayAssembly();
            if (aAssy == null) return new mdSlotZone(BaseShape, this, null, 0, 0, false);
            return   aAssy?.SlotZone(Handle) ; 
            
        }

        private mdSlotZone _AltSlotZone;
        internal mdSlotZone AltSlotZone
        {
            set => _AltSlotZone = value;
        }

        public bool LiesOnPanel(int aPanelIndex)
        {
            if (PanelIndex == aPanelIndex) return true;

            return Instances.FindAll((x) => x.PartIndex == PanelIndex).Count > 0;
        }

        public bool LiesOnPanel(List<int> aPanelIndexes)
        {
            if (aPanelIndexes == null) return false;
            foreach (int item in aPanelIndexes)
            {
                if (LiesOnPanel(item)) return true;
            }
            return false;
        }

        public uopVectors SlotCenters(mdTrayAssembly aAssy, out mdSlotZone rZone , bool bRegenSlots = false, bool bReturnClones = false, bool bInvert = false)
        {
            rZone = null;
            aAssy ??= GetMDTrayAssembly();
            if (aAssy == null) return uopVectors.Zero;
            if (!aAssy.IsECMD) return uopVectors.Zero;

            rZone = SlotZone(aAssy);
            if(rZone == null) return uopVectors.Zero;
            rZone.UpdateSlotPoints(aAssy, bMaintainSuppressed: true, bForceRegen: bRegenSlots);
            uopVectors _rVal = rZone.GridPts.GetBySuppressed(aSuppresedVal: false, bReturnClones: bReturnClones || bInvert);
            if (bInvert) _rVal.Mirror(null, Y, 0);
            return _rVal;
        }

        /// <summary>
        /// the splice angles that this section attaches to
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public colUOPParts SpliceAngles(mdTrayAssembly aAssy = null)
        {
            colUOPParts _rVal = new colUOPParts();
            try
            {
             
                if (aAssy == null) aAssy =GetMDTrayAssembly();
                
                if (aAssy == null) return _rVal;

                _rVal = mdUtils.GetAnglesBySpliceHandles(aAssy.SpliceAngles(), Splice1Handle, Splice2Handle);
            }
            catch (Exception exception)
            {
                LoggerManager log = new LoggerManager();
                log.LogError(exception.Message);
            }
            return _rVal;
        }

        /// <summary>
        /// the splices that define the section
        /// clones are returned
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public List<uopDeckSplice> Splices(bool bGetTop = true, bool bGetBottom = true, bool bGetClones = false) => BaseShape?.Splices(bGetTop, bGetBottom,bGetClones);
      




        public override void UpdatePartProperties()
        {
            DescriptiveName = IsManway?  $"Deck Section { Handle } (Manway)": $"Deck Section { Handle}";
        }

        public override void UpdatePartWeight() { base.Weight = Weight(); }

        /// <summary>
        ///returns a dxePolygon that is used to draw the elevation view of the deck section
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="bIncludePromoters"></param>
        /// <param name="bSuppressHoles"></param>
        /// <param name="bIncludeBolts"></param>
        /// <param name="rJoggleAngleIncluded"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <returns></returns>
        public dxePolygon View_Elevation(mdTrayAssembly aAssy, ref bool rJoggleAngleIncluded, bool bIncludePromoters = false, bool bSuppressHoles = false, bool bIncludeBolts = false, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY")
         => mdPolygons.DeckSection_View_Elevation(this, aAssy, ref rJoggleAngleIncluded, bIncludePromoters, bSuppressHoles, bIncludeBolts, aCenter, aRotation, aLayerName);
            
        
        /// <summary>
        ///returns a dxePolygon that is used to draw the layout view of the deck section
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="bObscured"></param>
        /// <param name="bIncludePromoters"></param>
        /// <param name="bIncludeHoles"></param>
        /// <param name="bIncludeSlotting"></param>
        /// <param name="bRegeneratePerimeter"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <param name="bHalfSideSlots"></param>
        /// <param name="bNoInstances"></param>
        /// <returns></returns>
        
        public dxePolygon View_Plan(mdTrayAssembly aAssy, bool bObscured, bool bIncludePromoters, bool bIncludeHoles, bool bIncludeSlotting, bool bRegeneratePerimeter, dxfVector aCenter, double aRotation = 0, string aLayerName = "GEOMETRY", bool bHalfSideSlots = false,  bool bRegenSlots = false, bool bSolidHoles = false)
        {
            return mdPolygons.DeckSection_View_Plan( this, aAssy, bObscured, bIncludePromoters, bIncludeHoles, bIncludeSlotting, bRegeneratePerimeter, aCenter, aRotation, aLayerName, bHalfSideSlots,  bRegenSlots, bSolidHoles);
            
        }
        
        /// <summary>
        ///returns the weight of the Section in english pounds
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="rPerfFraction"></param>
        /// <returns></returns>
        public new double Weight(mdTrayAssembly aAssy = null, double rPerfFraction = 0)
        {
            try
            {
                aAssy ??= GetMDTrayAssembly(aAssy);
                if (BaseShape == null) return 0;
                double sArea = BaseShape.Area - GenHoles(aAssy).TotalArea();
                if (rPerfFraction <= 0) rPerfFraction = (aAssy != null)?  aAssy.PerfFractionPercentOpen(out double FNXA, out double MECHA): 0;
                
                sArea -= rPerfFraction / 100 * sArea;
               return sArea * base.SheetMetalWeightMultiplier;
            }
            catch (Exception exception)
            {
                LoggerManager log = new LoggerManager();
                log.LogError(exception.Message);
            }
            return 0;
        }


        #endregion Methods



    }
}