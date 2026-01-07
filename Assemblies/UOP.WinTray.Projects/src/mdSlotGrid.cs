using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
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
    public class mdSlotGrid : uopGrid, ICloneable
    {
        #region Constructors

        public mdSlotGrid() => Init();

        public mdSlotGrid(mdSlotGrid aGrid) => Init(aGrid);

        public mdSlotGrid(uopSectionShape aSection) => Init(null, aSection);

        private void Init(mdSlotGrid aGrid = null, uopSectionShape aSection = null)
        {

            _Angles = new List<double>();
            SlotType = uppFlowSlotTypes.FullC;
            PanelFraction = 0;
            _PitchType = dxxPitchTypes.Triangular;
            RegenSlots = true;
            ProjectHandle = string.Empty;
            RangeGUID = string.Empty;
            TotalRequiredSlotCount = 0;

            LeftDowncomerInfo = null;
            RightDowncomerInfo = null;
            UnSlottable = true;
            _MaxYLeft = null;
            _MaxYRight = null;
            _MinYLeft = null;
            _MinYRight = null;
            if (aGrid != null) Copy(aGrid);

            if (aSection != null) Section = aSection;



        }
        #endregion Constructors

        #region Properties

        private List<double> _Angles;
        public List<double> Angles { get { _Angles ??= new List<double>(); return _Angles; }  }
        public override double HPitch { get => base.HPitch; set { if (value > 0 && value < mdGlobals.MinSlotXPitch )value = mdGlobals.MinSlotXPitch; base.HPitch = value; } }
        public override double VPitch { get => base.VPitch; set { if (value > 0 && value < mdGlobals.MinSlotXPitch) value = mdGlobals.MinSlotYPitch; base.VPitch = value; } }


        public bool IsSymmetrical
        {
            get
            {
                if (BaseShape == null) return false;
                if (BaseShape.IsHalfMoon) return false;
                if (BaseShape.IsManway) return true;
                if (BaseShape.PanelX == 0 && !BaseShape.LapsDivider) return true;
                if (BaseShape.LapsRing || BaseShape.LapsDivider) return false;
                if (BaseShape.TruncatedFlangeLine(null)) return false;
                return true;
            }
        }
        public uopSectionShape Section
        {
            set
            {
                if (value == null) return;
                SlotType = value.SlotType;

                LeftDowncomerInfo = DowncomerInfo.CloneCopy(value.LeftDowncomerInfo);
                RightDowncomerInfo = DowncomerInfo.CloneCopy(value.RightDowncomerInfo);
                TrayAssembly = value.MDTrayAssembly;


                base.Copy(value.SlotZoneBounds(out bool unslot));   // to get the boundary
                UnSlottable = unslot;
            }
        }

        private WeakReference<mdTrayAssembly> _AssyRef;
        public mdTrayAssembly TrayAssembly
        {
            get
            {
                if (_AssyRef == null) return null;
                if (!_AssyRef.TryGetTarget(out mdTrayAssembly _rVal)) _AssyRef = null;
                return _rVal;

            }

            set
            {
                if (value == null) { _AssyRef = null; return; }
                _AssyRef = new WeakReference<mdTrayAssembly>(value);
                SlotType = value.Deck.SlotType;
                RangeGUID = value.RangeGUID;
                ProjectHandle = value.ProjectHandle;
            }
        }

        /// <summary>
        /// the info about the the downcomer that defines the section on the left
        /// </summary>
        public DowncomerInfo LeftDowncomerInfo { get; set; }

        /// <summary>
        /// the info about the the downcomer that defines the section on the right
        /// </summary>
        /// 
        public DowncomerInfo RightDowncomerInfo { get; set; }

        private bool _UnSlottable;
        public bool UnSlottable
        {
            get => _UnSlottable || !base.IsDefined;
            set => _UnSlottable = value;
        }

        public int PanelIndex { get => BaseShape == null ? 0 : BaseShape.PanelIndex; }

        public int SectionIndex { get => BaseShape == null ? 0 : BaseShape.SectionIndex; }

        public int PanelSectionIndex { get => BaseShape == null ? 0 : BaseShape.PanelSectionIndex; }

        public int PanelSectionCount { get => BaseShape == null ? 0 : BaseShape.PanelSectionCount; }

        public uppFlowSlotTypes SlotType { get; set; }

        public double PanelFraction { get; set; }

        public double WeirHeight
        {
            get
            {
                if (LeftDowncomerInfo != null) return LeftDowncomerInfo.WeirHeight;
                if (RightDowncomerInfo != null) return RightDowncomerInfo.WeirHeight;
                return 0;
            }
        }

        public double PanelCX { get => BaseShape == null ? 0 : BaseShape.PanelX; }

        public bool RegenSlots { get; set; }

        public string ProjectHandle { get; set; }

        public string RangeGUID { get; set; }

        public int TotalRequiredSlotCount { get; set; }

        public bool IsManway { get => BaseShape == null ? false : BaseShape.IsManway; }
        public bool IsHalfMoon { get => BaseShape == null ? false : BaseShape.IsHalfMoon; }

        public double ShellRadius { get => BaseShape == null ? 0 : BaseShape.ShellRadius; }

        public double DeckRadius { get => BaseShape == null ? 0 : BaseShape.DeckRadius; }

        public double RingRadius { get => BaseShape == null ? 0 : BaseShape.RingRadius; }
        public double RingLap => RingRadius > 0 && RingRadius < DeckRadius ? DeckRadius - RingRadius : 0;
        public double TrimRadius { get => BoundingRadius; }
        /// <summary>
        /// returns the trim radius for the parent assemby which is the ring radius minus the assemblies clearance constant based on shell diameter
        /// </summary>
        public double BoundingRadius => RingRadius > 0 ? RingRadius - RingClearance : 0;


        /// <summary>
        /// the Y value below witch the slots left of center in the zone must be angled
        /// </summary>
        public double MaxYLeft { get { uopLine weir = WeirLineLeft; return weir == null ? double.MaxValue : Math.Round(weir.MaxY, 4); } }

        /// <summary>
        /// the Y value below witch the slots right of center in the zone must be angled
        /// </summary>
        public double MaxYRight { get { uopLine weir = WeirLineRight; return weir == null ? double.MaxValue : Math.Round(weir.MaxY, 4); } }

        /// <summary>
        /// the Y value above which the slots left of center in the zone must be angled
        /// </summary>
        public double MinYLeft { get { uopLine weir = WeirLineLeft; return weir == null ? double.MinValue : Math.Round(weir.MinY, 4); } }

        /// <summary>
        /// the Y value above which the slots right of center in the zone must be angled
        /// </summary>
        public double MinYRight { get { uopLine weir = WeirLineRight; return weir == null ? double.MinValue : Math.Round(weir.MinY, 4); } }


        public double RingClearance => uopUtils.BoundingClearance(2 * ShellRadius);

        public mdSlot FlowSlot => new mdSlot(SlotType);

        private WeakReference<mdSlotZone> _ZoneRef;
        public mdSlotZone Zone
        { get
            {
                if (_ZoneRef == null) return null;
                if (!_ZoneRef.TryGetTarget(out mdSlotZone _rVal)) _ZoneRef = null;
                return _rVal;
            }
            set
            {

                _ZoneRef = new WeakReference<mdSlotZone>(value);
            }
        }

        public override int OccuranceFactor { get  => BaseShape == null ? 1 : BaseShape.OccuranceFactor;  set { } }

        /// <summary>
        /// the number of slots required on this zone individually
        /// </summary>
        public int RequiredSlotCount
        {
            get
            {
                int occr = OccuranceFactor;
                int _rVal = TotalRequiredSlotCount;
                return (occr > 1) ? mzUtils.VarToInteger(uopUtils.RoundTo(_rVal / occr, dxxRoundToLimits.One, true)) : _rVal;
            }
        }



        private uopSectionShape _BaseShape;
        public uopSectionShape BaseShape
        {
            get => _BaseShape;
            set
            {
                bool newgrid = (value != null && _BaseShape == null) || (value != null && _BaseShape != null && _BaseShape != value);
                _BaseShape = value;

                if (newgrid)
                {
                    //Clear();
                    uopCompoundShape bounds = value.SlotZoneBounds(out _UnSlottable);
                    SetBounds (bounds, bounds.SubShapes.ArcRecs(false));
                    LeftDowncomerInfo = DowncomerInfo.CloneCopy(value.LeftDowncomerInfo);
                    RightDowncomerInfo = DowncomerInfo.CloneCopy(value.RightDowncomerInfo);
                }
            }
        }

        public override bool OnIsIn { get => true;  set{} }
        
        public override int Row { get => BaseShape == null ? 0 : BaseShape.Row;  set { } }
        public override int Col { get => BaseShape == null ? 0 : BaseShape.Col; set { } }

        public override dxxPitchTypes PitchType { get => _PitchType; set { if (value != dxxPitchTypes.Triangular && value != dxxPitchTypes.InvertedTriangular) return;  if (_PitchType != value) _Invalid = true; _PitchType = value; } }
        public override string Handle { get => BaseShape == null ? string.Empty : BaseShape.Handle; set { } }

        public bool MultiPanel { get => BaseShape == null ? false : BaseShape.MultiPanel; }
        public uopLine WeirLineLeft
        {
            get
            {
                DowncomerInfo dcinfo = LeftDowncomerInfo;
                if (dcinfo == null) return null;
                List<ULINEPAIR> weirlns = dcinfo.WeirLns;
                if (weirlns == null || weirlns.Count <= 0) return null;
                int idx = dcinfo.DesignFamily.IsStandardDesignFamily() ? 0 : weirlns.FindIndex(x => x.Row == Row);
                if (idx < 0) return null;
                ULINEPAIR weirs = dcinfo.WeirLns[idx];
                return !weirs.SideIsIsDefined(uppSides.Right) ? null : new uopLine(weirs.GetSide(uppSides.Right).Value);

            }
        }
        public uopLine WeirLineRight
        {
            get
            {
                DowncomerInfo dcinfo = RightDowncomerInfo;
                if (dcinfo == null) return null;
                List<ULINEPAIR> weirlns = dcinfo.WeirLns;
                if (weirlns == null || weirlns.Count <= 0) return null;
                int idx = dcinfo.DesignFamily.IsStandardDesignFamily() ? 0 : weirlns.FindIndex(x => x.Row == Row);
                if (idx < 0) return null;
                ULINEPAIR weirs = dcinfo.WeirLns[idx];
                return !weirs.SideIsIsDefined(uppSides.Left) ? null : new uopLine(weirs.GetSide(uppSides.Left).Value);

            }
        }
        public uopLinePair WeirLines => new uopLinePair(WeirLineLeft,WeirLineRight);

        public override string Name { 
            get => $"SLOT GRID {Handle}";
            set { }
        }
        /// <summary>
        ///returns all the grid points including the suppressed locations
        ///for the unsuppressed grid points use GridPoints
        /// </summary>
        public uopVectors GridPts { get { UpdateGridPoints(); return _GridPts == null ? uopVectors.Zero: _GridPts; } }

        public int GridPointCount  {  get  { return  GridPts.SuppressedCount(false); }  }


        /// <summary>
        ///the total slot centers in the zone
        ///includes opposing side
        /// </summary>
        public int TotalSlotCount { get => OccuranceFactor * GridPointCount; }

        #endregion Properties

        #region Methods
        public void Copy( mdSlotGrid aGrid)
        {
            if (aGrid == null) return;
            base.Copy(aGrid);

            // BaseShape = uopSectionShape.CloneCopy(  aGrid.BaseShape);
            _BaseShape = uopSectionShape.CloneCopy(aGrid.BaseShape);
            if (_BaseShape != null)
            {
                uopCompoundShape bounds = _BaseShape.SlotZoneBounds(out _UnSlottable);
                SetBounds(bounds, bounds.SubShapes.ArcRecs(false));
                LeftDowncomerInfo = DowncomerInfo.CloneCopy(_BaseShape.LeftDowncomerInfo);
                RightDowncomerInfo = DowncomerInfo.CloneCopy(_BaseShape.RightDowncomerInfo);

            }
            else
            {
                LeftDowncomerInfo = null;
                RightDowncomerInfo = null;
            }

               

            SlotType = aGrid.SlotType;
            PanelFraction = aGrid.PanelFraction;
          
            RegenSlots = aGrid.RegenSlots;
            ProjectHandle = aGrid.ProjectHandle;
            RangeGUID = aGrid.RangeGUID;
            TotalRequiredSlotCount = aGrid.TotalRequiredSlotCount;
            _MaxYLeft = aGrid._MaxYLeft;
            _MaxYRight = aGrid._MaxYRight;
            _MinYLeft = aGrid._MinYLeft;
            _MinYRight = aGrid._MinYRight;
            Index = aGrid.Index;


        }
        /// <summary>
        ///returns all the slot rectangles  not including the suppressed locations by default
        /// </summary>
        public uopRectangles SlotRectangles(bool? aSuppressedVal = false, bool bGetClones = false)
        {
            UpdateGridPoints();
            uopRectangles _rVal = new uopRectangles() { };
            if (base.RectangleCollector == null) return _rVal;
            if (aSuppressedVal.HasValue)
            {
                _rVal.Append(base.RectangleCollector.FindAll(x => x.Suppressed == aSuppressedVal.Value), bGetClones);
            }
            else
            {
                _rVal.Append(base.RectangleCollector, bGetClones);
            }
            return _rVal;
        }
      
     
    
        public void GetYLimits(out double rMaxYL, out double rMinYL, out double rMaxYR, out double rMinYR)
        {

            uopLine weir = WeirLineLeft;

            rMaxYL = weir == null ? double.MaxValue : Math.Round(weir.MaxY, 4);
            rMinYL = weir == null ? double.MinValue : Math.Round(weir.MinY, 4);

            weir = WeirLineRight;
            rMaxYR = weir == null ? double.MaxValue : Math.Round(weir.MaxY, 4);
            rMinYR = weir == null ? double.MinValue : Math.Round(weir.MinY, 4);

            _MaxYLeft = rMaxYL;
            _MaxYRight = rMaxYR;
            _MinYLeft = rMinYL;
            _MinYRight = rMinYR;

        }
        public new mdSlotGrid Clone() => new mdSlotGrid(this);

        object ICloneable.Clone() => new mdSlotGrid(this);

        /// <summary>
        ///  generates the slot centers based on current properties
        /// </summary>
        public override void Generate()
        {

            mdSlotZone zone = Zone;
            if (zone == null) { Clear(); return; }
            
            mdTrayAssembly aAssy = zone.TrayAssembly;


           // if (aAssy == null) return;
            mdDeckSection section = zone.DeckSection;


            mdDeckSection ds = section ?? aAssy?.DeckSections.GetByHandle(zone.Handle);
            if (ds == null) { Clear(); return; }
            
            Generate(ds.BaseShape);

        }

        public override void Clear()
        {
            base.Clear();
            _MaxYLeft = null;
            _MaxYRight = null;
            _MinYLeft = null;
            _MinYRight = null;
            _Angles = new List<double>(); 
        }

        /// <summary>
        ///  generates the slot centers based on current properties
        /// </summary>

        private double? _MaxYLeft;
        private double? _MaxYRight;
        private double? _MinYLeft;
        private double? _MinYRight;
        private uopVectors _SuppressLocations;
       private uopArcRecs _GenIslands;
        public void Generate(uopSectionShape aBaseSectionShape, bool? bSymmetrical = null )
        {
           
                List<double> rAngles = new List<double>();
            _SuppressLocations = _GridPts != null ? new uopVectors(_GridPts.GetBySuppressed(true)) : uopVectors.Zero;

            Clear();
            if (aBaseSectionShape == null) return;
            BaseShape =  aBaseSectionShape;
            uopSectionShape baseshape = BaseShape;

          
            _GenIslands = uopSectionShapes.GenMDBlockedAreas(BaseShape, (mdTrayAssembly)BaseShape.TrayAssembly, false, true).ArcRecs(false); // this.SubShapes.ArcRecs(false);
           
            uopVectors grdPts = uopVectors.Zero;
          double cX = baseshape.PanelX;
            double wd = this.Width;
            double ht = this.Height;
            double xoset = 0;
            double yoset = 0;
            uppMDDesigns family = baseshape.MDDesignFamily;
            
            double pX = Math.Round(HPitch, 6);
            double pY = Math.Round(VPitch, 6);
            double ystep = pY;
            double cY = this.Center.Y;
            bool halfmoon = baseshape.IsHalfMoon;
            bool symmetrical = bSymmetrical.HasValue ? bSymmetrical.Value : IsSymmetrical;
                uopVectors gPts = uopVectors.Zero;
            Clear();
            //set the angles and save the rectangles of the slots

            baseshape.GetSplices(out uopDeckSplice top, out uopDeckSplice bot);

            GetYLimits(out double maxYL, out double minYL, out double maxYR, out double minYR);

            //set the grid props
            uopGrid aGrid = new uopGrid(new uopRectangles(), new uopRectangle(uopVector.Zero, mdGlobals.SlotDieWidth, mdGlobals.SlotDieHeight))
            {
                Tag = baseshape.Name,
                VPitch = pY,
                HPitch = pX,
                OnIsIn = true,
                Alignment = uppGridAlignments.MiddleCenter,
                PitchType = dxxPitchTypes.InvertedTriangular,
                SuppressInteriorCheck = true,
                TrimColumnLinesToBounds = true,
                TrimRowLinesToBounds = baseshape.IsHalfMoon 
            };

            aGrid.SetBounds(this, null); // we are doing the island checking Islands);
            aGrid.EventGridPointAdded += MyGridPointAddedHandler;
            aGrid.EventOriginCreated += MyOriginCreatedHandler;
            if (symmetrical || (!symmetrical && ! IsHalfMoon))
            {
                aGrid.MaxX = cX; //too only generate the left side
                aGrid.MirrorLine = new uopLine(new uopVector(cX, this.Bottom), new uopVector(cX, this.Top));
                aGrid.MirrorLine.TrimWithShape(baseshape);
                aGrid.ValidateMirrorPoints = (!symmetrical && !IsHalfMoon);
                //if (cX < 0 || (!family.IsStandardDesignFamily() && baseshape.LapsDivider))
                //{
                //    aGrid.MaxX = null;
                //    aGrid.MinX = cX;
                //}
               

            }
            ystep = 0.5 * pY;
           

            if (pX > 0 && pY > 0 && wd > 0 && ht > 0)
            {
                //this.UnSlottable = true;
                if (!halfmoon)
                {
                    //set the offsets so the first column is a half x step to the right of center
                    xoset = cX>=0  ? -0.5 * pX : 0.5 * pX;
                    if (cY>= 0 && bot != null && bot.Y > 0  && baseshape.LapsRing )
                        //change the aligment for the end sectons to radiate out to the ring edges
                        aGrid.Alignment = uppGridAlignments.BottomCenter;
                    else if (cY < 0 && top != null && top.Y < 0 && baseshape.LapsRing)
                        //change the aligment for the end sectons to radiate out to the ring edges
                        aGrid.Alignment = uppGridAlignments.TopCenter;
                }
                else
                {
                    //set the offsets so the first column lies on the left edge of the bound for half moon panels
                    aGrid.Alignment = pX > 0 ? uppGridAlignments.MiddleLeft : uppGridAlignments.MiddleRight;
      
                }
                if (aGrid.Alignment.VerticalAlignment() == uppVerticalAlignments.Center)
                {

                    if ((Math.Truncate(ht / ystep) + 1 % 2) != 0)
                    {
                        //start a half step off the for even row count
                        yoset = 0.5 * pY;
                    }
                  
                }

                if(aGrid.MirrorLine != null)
                {
                    if(aGrid.VerticalAlignment == uppVerticalAlignments.Center)
                    {
                        aGrid._OverrideOrigin = new UVECTOR(aGrid.MirrorLine.X() + xoset, aGrid.MirrorLine.MidPt.Y + yoset);
                        xoset = 0;
                        yoset = 0;
                    }
                    else
                    {
                        yoset = 0;
             
                    }
                 

                   
                }
        
                aGrid.XOffset = xoset;
                aGrid.YOffset = yoset;
              
                //create the grid points
                aGrid.Generate();
                  
                aGrid.EventGridPointAdded -= MyGridPointAddedHandler;
                aGrid.EventOriginCreated -= MyOriginCreatedHandler;
                //    _Angles.Clear() ;
                //         //set the grid props
                //    uopGrid bGrid = new uopGrid(new uopRectangles(), new uopRectangle(uopVector.Zero, mdGlobals.SlotDieWidth, mdGlobals.SlotDieHeight))
                //    {
                //        Tag = baseshape.Handle,
                //        VPitch = pY,
                //        HPitch = pX,
                //        OnIsIn = true,
                //        Alignment = uppGridAlignments.MiddleCenter,
                //        PitchType = dxxPitchTypes.Triangular,
                //    };

                //    bGrid.SetBounds(this, null); // we are doing the island checking Islands);
                //    bGrid.EventGridPointAdded += MyGridPointAddedHandler;
                //    bGrid.OriginCreated += MyOriginCreatedHandler;
                //    bGrid.XOffset = aGrid.XOffset;
                //    bGrid.YOffset = (bGrid.YOffset == 0) ? 0.5 * bGrid.VPitch : 0;
                //    if (symmetrical)
                //    {
                //        bGrid.MaxX = cX; //too only generate the left side
                //        bGrid.MirrorLine = new uopLine(new uopVector(cX, this.Bottom), new uopVector(cX, this.Top));
                //    }

                //    CopyInternals(bGrid);

                //    //create the alt grid points
                //    bGrid.Generate();
                //    uopVectors zPts = bGrid.GridPoints();
                //    bGrid.EventGridPointAdded -= MyGridPointAddedHandler;
                //    bGrid.OriginCreated -= MyOriginCreatedHandler;
                //    _Angles.Clear();
                //    //pick the better solution
                //    if (gPts.Count < TotalRequiredSlotCount / baseshape.OccuranceFactor)
                //    {
                //        if (zPts.Count > gPts.Count)
                //        {
                //            aGrid = bGrid;
                //             gPts = zPts;
                //        }
                //    }
                //    else if (gPts.Count > TotalRequiredSlotCount / baseshape.OccuranceFactor)
                //    {
                //        if (zPts.Count < gPts.Count && zPts.Count >= TotalRequiredSlotCount / baseshape.OccuranceFactor)
                //        {
                //            aGrid = bGrid;
                //            gPts = zPts;
                //        }
                //    }

            }

            
            CopyInternals(aGrid);
            Islands = _GenIslands;
            PitchType = dxxPitchTypes.Triangular;

            if (_SuppressLocations.Count > 0) 
            { 
                foreach(var suppt in _SuppressLocations)
                {
                    uopVector gp = _GridPts.Nearest(suppt, 0.1);
                    if (gp != null)
                        SetGridPointSuppression(_GridPts.IndexOf(gp) +1,true);
                }
            }
            
            // base.Copy(aGrid);

            RegenSlots = false;
            Invalid = false;
        }

        private double GetSlotAngle( double aX, double aY)
        {
            if(BaseShape == null) return 0;
            //set the rotations

            if (!_MaxYLeft.HasValue || !_MaxYRight.HasValue || !_MinYLeft.HasValue || !_MinYRight.HasValue) GetYLimits(out _, out _, out _, out _);

            double cX = BaseShape.X;
            if (BaseShape.IsHalfMoon)
            {
                return X > 0 ? 0 : 180;

            }
            else
            {
                //if (cX >= 0)
                //{

                if (aX < cX)
                {
                    //save the left side points


                    if (_MaxYLeft.HasValue && Math.Round(aY, 4) > _MaxYLeft.Value)
                        return 45;
                    else if (_MinYLeft.HasValue && Math.Round(aY, 4) < _MinYLeft.Value)
                        return 315;
                    else
                        return 0;

                }
                else
                {
                    //save the right side points
                    // dieshape.Center.Y += aGrid.VPitch / 2;

                    if (_MaxYRight.HasValue && Math.Round(aY, 4) > _MaxYRight.Value)
                        return 135;
                    else if (_MinYRight.HasValue && Math.Round(aY, 4) < _MinYRight.Value)
                        return 225;
                    else
                        return 180;

                }
            }

        }

        public override string ToString() => string.IsNullOrWhiteSpace(Name) ? $"mdSlotGrid {Handle} [{Vertices.Count}]" : $"mdSlotGrid {Handle} [{Vertices.Count}] '{Name}'";

  
        #endregion Methods

        #region Event Handlers

        private void MyOriginCreatedHandler( uopVector aOrigin, uopGrid aGrid)
        {
            //if(aGrid.MirrorLine != null  && aGrid.VerticalAlignment == uppVerticalAlignments.Center)
            //{
            //    aOrigin.Y = aGrid.MirrorLine.MidPt.Y + aGrid.YOffset;
            //}
        }
        private void MyGridPointAddedHandler(uopVector aGridPoint, uopVectors aGridPoints, ref bool ioKeep, ref bool ioSuppress, ref bool rStopProcessing, uopLine aRowLine, uopLine aColumnLine, ref uopRectangle aRectangleToSave, bool bIsMirrorPoint)
        {

            if (aRectangleToSave == null) return;
            aRectangleToSave.Rotation = GetSlotAngle(aGridPoint.X, aGridPoint.Y);
            aGridPoint.Value = aRectangleToSave.Rotation;

            uopArcRecs violators = !bIsMirrorPoint || base.ValidateMirrorPoints ? _GenIslands.GetContainers(aRectangleToSave, bOnIsIn: true, aPrecis: 3, bJustOne: true, bReturnTrueByCenter : true) : uopArcRecs.Null;

            if (violators.Count > 0)
                ioKeep = false; // !violator.ContainsVector(aGridPoint,bOnIsIn:true,aPrecis: 4);
       
          
       

            if (ioKeep)
            {
                //persisting suppressed location
                if (_SuppressLocations != null && _SuppressLocations.Count > 0 && !ioSuppress)
                {
                    uopVector s2 = _SuppressLocations.Nearest(aGridPoint, aMinDistance: 0.1);
                    if (s2 != null)
                    {
                        aGridPoint.Suppressed = true;
                        ioSuppress = true;

                    }

                }

                
            }

            aRectangleToSave.Suppressed = ioSuppress;

            //saving angles
            if (!ioSuppress)
            {
                _Angles ??= new List<double>();
                if (!_Angles.Contains(aRectangleToSave.Rotation)) _Angles.Add(aRectangleToSave.Rotation);
            }
        }


        #endregion Event Handlers
        #region Shared Methods

        public static mdSlotGrid CloneCopy(mdSlotGrid aGrid) => aGrid == null ? null : new mdSlotGrid(aGrid);

        

        #endregion Shared Methods
    }
}
