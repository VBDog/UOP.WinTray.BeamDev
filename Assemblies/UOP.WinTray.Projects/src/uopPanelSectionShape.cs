using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using UOP.DXFGraphics;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Interfaces;
using UOP.WinTray.Projects.Parts;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;
using static System.Collections.Specialized.BitVector32;

namespace UOP.WinTray.Projects
{
    public class uopPanelSectionShape : uopCompoundShape, ICloneable
    {

        #region Constructors
        public uopPanelSectionShape() => Init();

        public uopPanelSectionShape(uopVectors aVertices, uopTrayAssembly aAssy = null) => Init(null, aAssy,aVertices);
        public uopPanelSectionShape(uopShape aShape, uopTrayAssembly aAssy = null) => Init(aShape, aAssy);
           protected virtual void Init(uopShape aShape, uopTrayAssembly aAssy = null, uopVectors aVertices = null)
        {
            ManholeID = 0;
            _Depth = null;
            RingRadius = 0;
            DeckRadius = 0;
            PanelIndex = 0;
            SectionIndex = 0;
            PanelSectionIndex = 0;
            PanelX = 0;
            PanelY = 0;
            ProjectFamily = uppProjectFamilies.Undefined;
            MDDesignFamily = uppMDDesigns.Undefined;
            _SpliceStyle = uppSpliceStyles.Undefined;
            _BPSites = uopVectors.Zero;
            _Instances = new uopInstances();
            ParentShape = null;
            _FreeBubblingArea = null;
            LeftDowncomerInfo = null;
            RightDowncomerInfo = null;
            DowncomerClearance = 0;
            SpliceBoltCount = 0;
            SpliceAngleLength = 0;
            SlotType =  uppFlowSlotTypes.FullC;
            RingClipSpacing = mdGlobals.DefaultRingClipSpacing;
            SuppressBubblePromoters = true;
            if (aShape != null) Copy(aShape);

            if (aAssy != null) TrayAssembly = aAssy;
            if (aVertices != null) Vertices.Populate(aVertices);
        }
        
        #endregion Constructors


        #region Properties

        /// <summary>
        /// the shape that is  the mechanical shape of the secction without any splice details 
        /// </summary>
        internal virtual USHAPE? MechanicalBounds { get; set; }

        public uopPanelSectionShape ParentShape { get; set; }
        public override string Handle { get => $"{PanelIndex},{SectionIndex}"; set { } }


        public override string Name { get => $"PANEL SECTION SHAPE {Handle} x {OccuranceFactor}"; set { } }

        public uopLinePair Weirs => base.LinePair;

        public mdTrayAssembly MDTrayAssembly { get { uopTrayAssembly assy = TrayAssembly; return assy == null ? null : assy.ProjectFamily == uppProjectFamilies.uopFamMD ? (mdTrayAssembly)assy : null; } }

        private WeakReference<uopTrayAssembly> _AssyRef;
        public uopTrayAssembly TrayAssembly
        {
            get
            {
                if (_AssyRef == null) return null;
                if (!_AssyRef.TryGetTarget(out uopTrayAssembly _rVal)) _AssyRef = null;
                return _rVal;

            }

            set
            {
                if (value == null) { _AssyRef = null; return; }
                _AssyRef = new WeakReference<uopTrayAssembly>(value);

                ManholeID = value.ManholeID;
                Thickness = value.DeckMaterial.Thickness;
                RingRadius = value.RingRadius;
                DeckRadius = value.DeckRadius;
                ProjectFamily = value.ProjectFamily;

                if (value.ProjectFamily == uppProjectFamilies.uopFamMD)
                {
                    mdTrayAssembly assy = (mdTrayAssembly)value;
                    MDDesignFamily = assy.DesignFamily;
                    SpliceAngleLength = assy.SpliceFlangeLength(out int bcnt);
                    SpliceBoltCount = bcnt;
                    RingClipSpacing = IsHalfMoon ? assy.DesignOptions.MoonRingClipSpacing : assy.DesignOptions.MaxRingClipSpacing;
                    Divider = assy.Divider;
                    SuppressBubblePromoters = !assy.DesignOptions.HasBubblePromoters;
                    SlotType = assy.Deck.SlotType;
                }
            }
        }

        public double WeirHeight
        {
            get
            {
                if (LeftDowncomerInfo != null) return LeftDowncomerInfo.WeirHeight;
                if (RightDowncomerInfo != null) return RightDowncomerInfo.WeirHeight;
                return 0;
            }
        }
        public uppFlowSlotTypes SlotType { get; set; }

   

        /// <summary>
        /// the maximum ordinate for a splice on the panel
        /// </summary>
        public virtual double MaxSpliceOrdinate { get { if (IsHalfMoon) { if (Width <= 0) { return X >= 0 ? double.MinValue : double.MaxValue; } else { return X >= 0 ? Right - mdGlobals.MinSpliceProximity : Left + mdGlobals.MinSpliceProximity; } } else { return Height <= 0 ? double.MinValue : Top - mdGlobals.MinSpliceProximity; } } } 
        /// <summary>
        /// the minimum ordinate for a splice on the panel
        /// </summary>
        public virtual double MinSpliceOrdinate { get { if (IsHalfMoon) { if (Width <= 0) { return X >= 0 ? double.MaxValue : double.MinValue; } else { return X >= 0 ? Left + mdGlobals.MinSpliceProximity : Right + mdGlobals.MinSpliceProximity; } } else { return Height <= 0 ? double.MaxValue : Bottom + mdGlobals.MinSpliceProximity; } } }

        public bool IsHalfMoon
        {
            get
            {
                //if (LinePair == null || !LinePair.IsDefined) return false;
                return PanelIndex == 1; // !LinePair.SideIsIsDefined(Enums.uppSides.Left) || !LinePair.SideIsIsDefined(Enums.uppSides.Right);

            }
        }
        public uppProjectFamilies ProjectFamily { get; set; }

        public uppMDDesigns MDDesignFamily { get; set; }

        private uppSpliceStyles _SpliceStyle;
        /// <summary>
        /// the basic style that governs the configuration based on its ordinate
        /// </summary>
        public uppSpliceStyles SpliceStyle
        {
            get
            {
                if ((int)_SpliceStyle < 0 || (int)_SpliceStyle > 2)
                {
                    uopTrayAssembly aAssy = TrayAssembly;
                    if (aAssy != null) _SpliceStyle = aAssy.SpliceStyle;

                }
                return _SpliceStyle;
            }
            set
            {
                if (value >= 0 & (int)value <= 2)
                {
                    if (_SpliceStyle == value) return;
                    _SpliceStyle = value;

                }
            }
        }

        public double PanelX { get; set; }
        public double PanelY { get; set; }

        public int PanelIndex { get => base.PartIndex; set => base.PartIndex = value; }

        public int SectionIndex { get; set; }

        public uopLinePair PanelWeirLines => LinePair;


        public double ManholeID { get; set; }


        public bool LapsDivider =>  (!IsDefined || (ProjectFamily == uppProjectFamilies.uopFamMD && MDDesignFamily.IsStandardDesignFamily())) ? false:  Segments.LineSegments().FindIndex(x => !x.IsHorizontal(2) && !x.IsVertical(2)) >= 0;
            
        public bool LapsRing => (DeckRadius <= 0 || !IsDefined) ? false : Segments.FindIndex(x => Math.Round(x.Radius, 2) == Math.Round(DeckRadius, 2)) >= 0;
            
        private uopVectors _BPSites;
        public virtual uopVectors BPSites { get { _BPSites ??= uopVectors.Zero; return _BPSites; } set => _BPSites = value == null ? uopVectors.Zero : value; }


        private uopInstances _Instances;
        public virtual uopInstances Instances { get { _Instances ??= new uopInstances(); _Instances.BasePt = Center; return _Instances; } set => _Instances = value; }

        public override int OccuranceFactor
        {
            get => IsVirtual ? 0 :  _Instances == null ? 1 : _Instances.Count + 1;
            set { }
        }

        public double RingRadius { get; set; }

        public double TrimRadius { get => BoundingRadius; }
        /// <summary>
        /// returns the trim radius for the parent assemby which is the ring radius minus the assemblies clearance constant based on shell diameter
        /// </summary>
        public double BoundingRadius => RingRadius > 0 ? RingRadius - RingClearance : 0;


        public double DeckRadius { get; set; }

        public double ShellRadius { get; set; }

        public double RingLap => RingRadius > 0 && RingRadius < DeckRadius ? DeckRadius - RingRadius : 0;

        public double RingClearance => uopUtils.BoundingClearance(2 * ShellRadius);

        public double Thickness { get; set; }

        public int PanelSectionIndex { get; set; }


        protected double? _Depth;
        public new double Depth
        {
            get
            {
                if (_Depth.HasValue) return _Depth.Value;


                return Thickness;

            }
            set
            {
                _Depth = value;
            }
        }


        public double FitWidth
        {
            get
            {

                return Math.Min(Width, Height);

            }
        }
        public double ShelfWidth { get; set; }
        public double SpliceAngleLength { get; set; }
        public int SpliceBoltCount { get; set; }
        public double DowncomerClearance { get; set; }

        /// <summary>
        /// the info about the the downcomer that defines the section on the left
        /// </summary>

        public DowncomerInfo LeftDowncomerInfo { get; set; }

        /// <summary>
        /// the info about the the downcomer that defines the section on the right
        /// </summary>

        public DowncomerInfo RightDowncomerInfo { get; set; }

        public int LeftDowncomerIndex => LeftDowncomerInfo == null ? 0 : LeftDowncomerInfo.DCIndex;
        public int RightDowncomerIndex => RightDowncomerInfo == null ? 0 : RightDowncomerInfo.DCIndex;

        public double? LeftDowncomerX {get{ if(LeftDowncomerInfo == null )return null;  return LeftDowncomerInfo.X;} }
        public double? RightDowncomerX{get{ if(RightDowncomerInfo == null )return null;  return RightDowncomerInfo.X;} }

        public bool IsCenterSection
        {
            get
            {
                double? x1 = LeftDowncomerX;
                if(!x1.HasValue) return false;
                double? x2 = RightDowncomerX;
                if (!x2.HasValue) return false;
                return Math.Round(Math.Abs(x1.Value), 3) == Math.Round(Math.Abs(x2.Value), 3);
            }
        }
        public double LeftDowncomerClearance => LeftDowncomerInfo == null ? 0 : Right - LeftDowncomerInfo.X_Outside_Right;
        public double RightDowncomerClearance => RightDowncomerInfo == null ? 0 : RightDowncomerInfo.X_Outside_Left - Right;

        public double LeftDowncomerLap
        {
            get
            {
                if (LeftDowncomerInfo == null) return 0;
                return (LeftDowncomerInfo.X_Outside_Right + LeftDowncomerInfo.ShelfWidth) - Left;
            }
        }

        public double RightDowncomerLap
        {
            get
            {
                if (RightDowncomerInfo == null) return 0;
                return Right - (RightDowncomerInfo.X_Outside_Left - LeftDowncomerInfo.ShelfWidth);
            }
        }
        private uopFreeBubblingArea _FreeBubblingArea;
        public uopFreeBubblingArea FreeBubblingArea
        {
            get  => CreateFreeBubblingArea(); 
        }

        public double RingClipSpacing { get; set; }

        internal UHOLE RingClipHoleU()  => new UHOLE(aTag: "RING CLIP", aFlag: "HOLE")  {  Depth = Thickness,  Elevation = 0.5 * Thickness, Diameter = mdGlobals.gsBigHole };

        /// <summary>
        /// returns true if the parent panel has more than one non-virtual section
        /// </summary>
        public bool MultiPanel => Divider == null ? false : Divider.DividerType == uppTrayDividerTypes.Beam && Divider.Offset != 0;

        public DividerInfo Divider { get; set; }

        public bool SuppressBubblePromoters { get; set; }



        #endregion Properties


        #region Methods
        
        public virtual bool GetSplices(out uopDeckSplice rTopSplice, out uopDeckSplice rBottomSplice, bool bGetClones = false)   {  rTopSplice = null;  rBottomSplice = null;  return false; }

        public virtual uopCompoundShape SlotZoneBounds(out bool rUnSlottable)
        {

            uopSectionShape section = this.ToSectionShape(MDTrayAssembly);

            return section.SlotZoneBounds(out rUnSlottable);
        }

        public virtual uopShapes GetSubShapes(uppSubShapeTypes aType, uopTrayAssembly aAssy = null)
        {
            aAssy ??= TrayAssembly;

            uopSectionShape section = new uopSectionShape(this,null,null,aAssy,false,Vertices);

            switch (aType)
            {
                case uppSubShapeTypes.BlockedAreas:
                    {
                        return uopSectionShapes.GenMDBlockedAreas(section, (mdTrayAssembly)aAssy);

                    }
                case uppSubShapeTypes.SlotBlockedAreas:
                    {
                        return uopSectionShapes.GenMDBlockedAreas(section, (mdTrayAssembly)aAssy, true, true);

                    }
            }



            return new uopShapes(aType.Description());
        }
        public virtual  uopFreeBubblingArea CreateFreeBubblingArea()
        {
            double radoffset = -RingLap;
            double dcoffsetL = LeftDowncomerInfo != null ? LeftDowncomerInfo.ShelfWidth : 0;
            double dcoffsetR = RightDowncomerInfo != null ? -RightDowncomerInfo.ShelfWidth : 0;
            uopCompoundShape fba = CreateSubShape("FREE BUBBLING AREA", aRadiusOffset: radoffset, aLeftDowncomerOffset: dcoffsetL, aRightDowncomerOffset: dcoffsetR, aTopSpliceOffset : 0 , aBottomSpliceOffset: 0);

            uopFreeBubblingArea _rVal = new uopFreeBubblingArea(PanelIndex,Weirs,fba ){ PanelSectionIndex = PanelSectionIndex, Index = PanelSectionIndex};
            
            return _rVal;

        }

        public virtual uopCompoundShape CreateSubShape(string aName, double aRadiusOffset = 0, double aLeftDowncomerOffset = 0, double aRightDowncomerOffset = 0, double aTopSpliceOffset = 0, double aBottomSpliceOffset = 0, double aLeftEdgeOffset = 0, double aRightEdgeOffset = 0, bool bIncludeQuadrantPoints = true)
        {

            uopCompoundShape _rVal = new uopCompoundShape() { Name = aName };
            uopSegments segs = new uopSegments(Segments);
            List<uopLine> lines = segs.LineSegments();
            List<uopLine> vlines = lines.FindAll(x => x.IsVertical(2));
            List<uopLine> hlines = lines.FindAll(x => x.IsHorizontal(2));

            List<uopLine> alines = lines.FindAll(x => !x.IsVertical(2) && !x.IsHorizontal(2));
            List<uopArc> arcs = segs.ArcSegments();

            List<iSegment> trimarcs = new List<iSegment>();
            uopRectangle bounds = new uopRectangle(BoundsV);
            double rad = DeckRadius + aRadiusOffset;
            uopVectors ips = uopVectors.Zero;
           DowncomerInfo dcleft = LeftDowncomerInfo;
            DowncomerInfo dcright = RightDowncomerInfo;
            bool pointonright = false;
            bool pointonleft = false;

            if ((dcleft != null && !Weirs.SideIsIsDefined(uppSides.Left)) || dcleft == null)
            {
                dcleft = null;
                if (vlines.FindIndex(x => x.IsVertical(2) && x.X() < X) < 0)
                {
                    aLeftDowncomerOffset = 0;
                    aLeftEdgeOffset = 0;
                    bounds.Left = -rad - 1;
                    pointonleft = Vertices.Count == 3;
                }
            }
            if ((dcright != null && !Weirs.SideIsIsDefined(uppSides.Right)) || dcright ==null)
            {
                dcright = null;
                if (vlines.FindIndex(x => x.IsVertical(2) && x.X() > X) < 0)
                {
                    aRightDowncomerOffset = 0;
                    aRightEdgeOffset = 0;
                    bounds.Right = rad + 1;
                    pointonright = Vertices.Count ==3;

                }
            }


            if (dcleft != null && aLeftDowncomerOffset != 0)
                bounds.Left = dcleft.X_Outside_Right  + Math.Abs(aLeftDowncomerOffset);
            else if(aLeftEdgeOffset != 0) 
                bounds.Left += aLeftEdgeOffset;

            if (dcright != null && aRightDowncomerOffset != 0)
                bounds.Right = dcright.X_Outside_Left - Math.Abs(aRightDowncomerOffset);
            else if (aRightEdgeOffset != 0)
                bounds.Right  += aRightEdgeOffset;

            //if (MDDesignFamily.IsStandardDesignFamily())
            //{

                if (!IsHalfMoon)
                {
                    if (aTopSpliceOffset != 0 && hlines.FindIndex(x => x.Y() > Y) >= 0) bounds.Top  += aTopSpliceOffset;
                    if (aBottomSpliceOffset != 0 && hlines.FindIndex(x => x.Y() < Y) >= 0) bounds.Bottom += aBottomSpliceOffset;

                }
                else
                {
                    if(X > 0)
                    {
                        if (aTopSpliceOffset != 0 && vlines.FindIndex(x => x.X() > X) >= 0) bounds.Right += aTopSpliceOffset;
                        if (aBottomSpliceOffset != 0 && vlines.FindIndex(x => x.X() < X) >= 0) bounds.Left += aBottomSpliceOffset;

                    }
                    else
                    {
                        if (aBottomSpliceOffset != 0 && vlines.FindIndex(x => x.X() > X) >= 0) bounds.Right += aBottomSpliceOffset;
                        if (aTopSpliceOffset != 0 && vlines.FindIndex(x => x.X() < X) >= 0) bounds.Left += aTopSpliceOffset;

                    }

               }

            uopLine bisector1 = null;
            uopLine bisector2 = null;
            if(alines.Count >= 1)
            {
                bisector1 = new uopLine(alines[0]);

                bisector1.Rectify(false, true);
                if (aRadiusOffset != 0)
                {
                    uopVector offsetdir = Center.DirectionTo(bisector1, out _) * -1; // trimmer.Direction;
                    bisector1.Offset(offsetdir, Math.Abs(aRadiusOffset));
                }
            }
            if (alines.Count >= 2)
            {
                bisector2 = new uopLine(alines[1]);

                bisector2.Rectify(false, true);
                if (aRadiusOffset != 0)
                {
                    uopVector offsetdir = Center.DirectionTo(bisector2, out _) * -1; // trimmer.Direction;
                    bisector2.Offset(offsetdir, Math.Abs(aRadiusOffset));
                }
            }

            

            ips = uopShape.CircleSectionVertices(new uopArc(rad), bounds, bisector1, bisector2,aPrecis:3,bIncludeQuadrantPts: bIncludeQuadrantPoints, aName:$"{aName} - {Handle}");

           // ips = uopShape.CircleSectionVertices(null, aRadius: rad, bounds);
            if (ips.Count <= 0)
                ips.AddRange(bounds.Corners);
            else
                _rVal.Vertices.AddRange(ips);

            _rVal.Vertices.Populate(ips);
             _rVal.Update();
            _rVal.OccuranceFactor = OccuranceFactor;
          
            return _rVal;
        }

        public  uopVectors RingClipCenters(out List<uopRingClipSegment> rSegments, out uopShape rRingClipBounds,  double? aSpacing = null)
        {
            if(aSpacing.HasValue && aSpacing.Value > 0) RingClipSpacing = aSpacing.Value;
            rSegments = RingClipSegments(out rRingClipBounds);
            uopVectors _rVal = uopVectors.Zero;
            if (rSegments.Count <= 0) return _rVal;
            List<iSegment> rcSegs = rSegments.OfType<iSegment>().ToList();
            uopHoles holes = mdUtils.LayoutRingClipHoles(rcSegs, aRingRadius: RingRadius, aSpacing: RingClipSpacing, bAtLeastOne: false, bSavePointsToSegments: true, aDepth:Thickness);
            _rVal.Append(  holes.Centers, bAddClone:true);

            return _rVal;
        }

        public override void Move(double aX = 0, double aY = 0)
        {
            base.Move(aX, aY);
            if(MechanicalBounds.HasValue) MechanicalBounds = MechanicalBounds.Value.Moved(aX, aY);
        }

        public override bool Mirror(double? aX, double? aY)
        {
           bool _rVal =  base.Mirror(aX, aY);
            if (MechanicalBounds.HasValue && _rVal) MechanicalBounds = MechanicalBounds.Value.Mirrored(aX, aY);
            return _rVal;
        }
        public virtual List<uopRingClipSegment> RingClipSegments(out uopShape rRingClipBounds) => uopUtils.CreateRingClipSegments(this, out rRingClipBounds);

        public virtual uopShape RingClipArcBounds()
        {
            double radoffset = -(DeckRadius - TrimRadius);
            double dcoffsetL = LeftDowncomerInfo != null ? LeftDowncomerInfo.ShelfWidth + mdGlobals.MinRingClipClearance : 0;
            double dcoffsetR = RightDowncomerInfo != null ? -(RightDowncomerInfo.ShelfWidth + mdGlobals.MinRingClipClearance) : 0;
  
            return CreateSubShape("RING CLIP BOUNDS", radoffset, dcoffsetL, dcoffsetR, 0, 0);
            //uopSectionShape.CreateInternalBoundary(this, out _, RingRadius - TrimRadius, mdGlobals.MinRingClipClearance, mdGlobals.MinRingClipClearance, this.IsHalfMoon ? mdGlobals.HoldDownWasherRadius : 0.5);
        }
        public override string ToString() => string.IsNullOrWhiteSpace(Name) ? $"uopPanelSectionShape[{Vertices.Count}]" : $"uopPanelSectionShape[{Vertices.Count}] '{Name}'";

        public uopSectionShape ToSectionShape(uopTrayAssembly aAssy = null)
        {
            aAssy ??= TrayAssembly;
            return new uopSectionShape(this, aAssy: aAssy, aVertices: Vertices)
            {
                PanelIndex = PanelIndex,
                PanelSectionIndex = PanelSectionIndex,
                PanelSectionCount = 1,
                SectionIndex = 1,
                PanelX = PanelX,
                PanelY = PanelY,
                LinePair = new uopLinePair(Weirs),
                LeftDowncomerInfo = Weirs.SideIsIsDefined(uppSides.Left) ? DowncomerInfo.CloneCopy(LeftDowncomerInfo) : null,
                RightDowncomerInfo = Weirs.SideIsIsDefined(uppSides.Right) ? DowncomerInfo.CloneCopy(RightDowncomerInfo) : null,
                ParentShape = uopPanelSectionShape.CloneCopy(ParentShape),
            };
        }
        public void Copy(uopShape aShape)
        {
            if (aShape == null) return;
            base.Copy(aShape);

            if (aShape is uopPanelSectionShape || aShape is uopSectionShape)
            {

                uopPanelSectionShape pshape = (uopPanelSectionShape)aShape;
                ManholeID = pshape.ManholeID;
                _Depth = pshape._Depth;
                Thickness = pshape.Thickness;
                RingRadius = pshape.RingRadius;
                DeckRadius = pshape.DeckRadius;
                PanelIndex = pshape.PanelIndex;
                SectionIndex = pshape.SectionIndex;
                PanelSectionIndex = pshape.PanelSectionIndex;
                ProjectFamily = pshape.ProjectFamily;
                MDDesignFamily = pshape.MDDesignFamily;
                PanelX = pshape.PanelX;
                PanelY = pshape.PanelY;
                ShellRadius = pshape.ShellRadius;
                RingRadius = pshape.RingRadius;
                DeckRadius = pshape.DeckRadius;
                _Instances = uopInstances.CloneCopy(pshape._Instances);
                _BPSites = uopVectors.CloneCopy(pshape._BPSites);
                LeftDowncomerInfo = DowncomerInfo.CloneCopy(pshape.LeftDowncomerInfo);
                RightDowncomerInfo = DowncomerInfo.CloneCopy(pshape.RightDowncomerInfo);
                SlotType = pshape.SlotType;
                ParentShape = uopPanelSectionShape.CloneCopy(pshape.ParentShape);
                if (pshape.GetType() == typeof(uopSectionShape))
                {
                    uopSectionShape secshape = (uopSectionShape)pshape;
                    if (secshape.HasMechanicalBounds) MechanicalBounds = USHAPE.CloneCopy(pshape.MechanicalBounds);
                }
                else
                {
                    MechanicalBounds = USHAPE.CloneCopy(pshape.MechanicalBounds);
                }

                DowncomerClearance = pshape.DowncomerClearance;
                ShelfWidth = pshape.ShelfWidth;
                SpliceAngleLength = pshape.SpliceAngleLength;
                SpliceBoltCount = pshape.SpliceBoltCount;
                Divider = DividerInfo.CloneCopy(pshape.Divider);
                SuppressBubblePromoters = pshape.SuppressBubblePromoters;
                TrayAssembly = pshape.TrayAssembly;  // this sets thickness, ring rad, clearance, project type and design family

            }


        }

            public new uopPanelSectionShape Clone() => new uopPanelSectionShape(this);
            object ICloneable.Clone() => (object)new uopPanelSectionShape(this);

            public bool FitsThruManhole(double? aManHoleID = null, double aClearance = 0.5)
            {
                if (!aManHoleID.HasValue) aManHoleID = ManholeID;
                if (aManHoleID.Value < 10) aManHoleID = 10;
                double fitwd = FitWidth;
                bool _rVal = uopUtils.FitsThruCircle(aManHoleID.Value, fitwd, fitwd, Depth, aClearance);
                if (!_rVal && !IsHalfMoon)
                {
                    // ccheck for odd (not rectangular) shapes

                }
                return _rVal;

            }

        #endregion Methods

        #region Shared Methods
        public static uopPanelSectionShape CloneCopy(uopPanelSectionShape aShape) => aShape == null ? null : new uopPanelSectionShape(aShape); 

       
        #endregion Shared Methods
    }
}

