using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using UOP.DXFGraphics;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Parts;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;


namespace UOP.WinTray.Projects
{



    public class mdSpoutGrid : uopGrid, ICloneable
    {

        #region Fields
 
        internal URECTANGLE _Margins;
        internal UHOLE _Spout;
        internal UHOLES _Spouts;
        internal USHAPE? _MinBounds;
        internal USHAPE? _MaxBounds;
        internal double? _TopMargin;
        internal double? _BottomMargin;

        #endregion Fields

        private enum GenerationMethods
        {
            Undefined = 0,
            ByConstraint = 1,
            ByString = 2
        }


        #region Constructors

        public mdSpoutGrid() => Init();

        public mdSpoutGrid(mdSpoutGroup aGroup, mdConstraint aConstraints = null, mdTrayAssembly aAssy = null) => Init(aGroup, aConstraints, aAssy);

  
        public mdSpoutGrid(uopGrid aGrid, bool bDontCopyMembers = false)
        {
            Init();

            if (aGrid == null) return;
            Copy(aGrid);
            if (bDontCopyMembers) Clear();
        }

        protected void Init(mdSpoutGroup aGroup = null, mdConstraint aConstraints = null, mdTrayAssembly aAssy = null, uppSpoutPatterns aPattern = uppSpoutPatterns.Undefined, bool bExpandToMargins = true)
        {
            base.Init();
            _Generating = false;
            _ReGenerating = false;
            _Margins = URECTANGLE.Null;
            _SpoutCenter = UVECTOR.Zero;
            _Spout = new UHOLE(19 / 25.4, 0, 0);
            _Spouts = new UHOLES(_Spout, UVECTORS.Zero);
            SpoutArea = null;
            _ConstraintsRef = null;
            _SpoutGroupRef = null;
            Invalid = false;
            MarginsApplied = false;
            TotalArea = 0;
            GoodSolution = false; SPR = 0;
            Pattern = aPattern;
            AppliedEndPlateClearance = 0;
            ActualEndPlateClearance = 0;
            AppliedClearance = 0;
            PatternLength = 0;
            _TargetArea = 0;
         
            ErrorLimit = 0;
            TargetCount = 0;
            ErrorString = string.Empty;
            TargetRows = 0;
            PanelIndex = -1;
            GroupIndex = 1;
            Depth = 0;
            BottomFraction = 0;
            AppliedMargin = 0;
            SlotLength = 0;
       
            HPitchLock = false;
            VPitchLock = false;
            PatternLock = false;
            LengthLock = false;
            CountLock = false;
            ApplyMargins = false;
            Metric = false;
            LastLength = 0;
            YOffset = 0;
            BoxIndex = 0;
            _Margins = URECTANGLE.Null;
            _MinBounds = null;
            _MaxBounds = null;
            UsesMaxBounds = false;
            _TopMargin = null;
            _BottomMargin = null;
            DesignFamily = uppMDDesigns.Undefined;

            ExpandToMargins = bExpandToMargins;


            // this sets the spout group reference and the spout area
            if (aGroup == null)
                aGroup = SpoutGroup;
            else
                SpoutGroup = aGroup;

            if (aConstraints == null)
                aConstraints = Constraints;
            else
                Constraints = aConstraints;

            if (aGroup == null) return;
            DesignFamily = aGroup.DesignFamily;
            aAssy ??= aGroup.GetMDTrayAssembly();

            if (aAssy == null) return;
            DesignFamily = aAssy.DesignFamily;
            // if (!DesignFamily.IsStandardDesignFamily()) ExpandToMargins = false;
            ExpandToMargins = !SpoutArea.LimitedBounds;
            if (aConstraints == null)
            {
                aConstraints = aGroup.Constraints(aAssy);
                Constraints = aConstraints;
            }

            if (SpoutArea != null)
            {
               
                aConstraints?.PropValSet("TreatAsIdeal", SpoutArea.TreatAsIdeal, bSuppressEvnts: true);
                if (SpoutArea.TreatAsIdeal)
                {
                    TargetArea = SpoutArea.OverrideSpoutArea.Value;

                }
                aConstraints?.PropValSet("OverrideSpoutArea", SpoutArea.TreatAsIdeal ? SpoutArea.OverrideSpoutArea.Value : 0, bSuppressEvnts: true);
            }
            else
            {
                if (aConstraints != null)
                {
                    if (aConstraints.TreatAsIdeal)
                        TargetArea = aConstraints.OverrideSpoutArea;
                }

            }

            SetSpout();
            SetLimits(aGroup, aConstraints, aAssy);

        }

        #endregion Constructors

        #region Properties
        /// <summary>
        /// the bounding rectangle of the shape
        /// </summary>
        internal override URECTANGLE BoundsV
        {
            get
            {
                URECTANGLE _rVal = Vertices.Count >0 ? Vertices.Bounds : SpoutArea == null ? base.BoundsV : SpoutArea.BoundsV;
                return _rVal;

            }
        }
        private double SlotLength { get; set; }

        private GenerationMethods GenerationMethod
        {
            get;
            set;
        }
        internal UVECTOR _SpoutCenter;
        public uopVector SpoutCenter { get => new uopVector(_SpoutCenter); }

        public double TargetX => SpoutArea != null ? SpoutArea.X : 0;
        public double TargetY 
        { 
            get
            {
                mdSpoutArea sa = SpoutArea;
                if (sa == null) return 0;
                double top = sa.LimitedTop ? sa.Top : sa.PanelLims.Top;
                double bot = sa.LimitedBottom ? sa.Bottom : sa.PanelLims.Bottom;
                return bot + (top - bot) / 2;
            }
        }
        public uopLine LimitLine
        {
            get
            {
                if (SpoutArea == null) return null;
                //if (!_LimitLine.HasValue) return null;
                uopVectors pts = SpoutArea.BoxLimitLines.EndPoints();

                uopLine _rVal = null;
                if (Direction == dxxOrthoDirections.Up)
                    _rVal = new uopLine(pts.GetVector(dxxPointFilters.GetLeftTop), pts.GetVector(dxxPointFilters.GetRightTop));
                else
                    _rVal = new uopLine(pts.GetVector(dxxPointFilters.GetLeftBottom), pts.GetVector(dxxPointFilters.GetRightBottom));


                double clrc = AppliedEndPlateClearance;
                if (clrc != 0)
                {
                    if (Direction == dxxOrthoDirections.Up) clrc *= -1;
                    _rVal.MoveOrtho(clrc, bInvert: _rVal.DeltaX < 0);
                }

                return _rVal;
            }
        }

        public bool UsesMaxBounds { get; private set; }

        public bool ExpandToMargins { get; private set; }

        public uppMDDesigns DesignFamily { get; private set; }

        public uppSpoutPatterns Pattern { get; internal set; }

 

        public bool MarginsApplied { get; private set; }

        public bool GoodSolution { get; private set; }

        public double? MaxMargin
        {
            get
            {
                if (!_TopMargin.HasValue && !_BottomMargin.HasValue) return null;
                if (_TopMargin.HasValue && _BottomMargin.HasValue)
                {
                    return Math.Abs(_TopMargin.Value) > Math.Abs(_BottomMargin.Value) ? _TopMargin.Value : _BottomMargin.Value;
                }
                return _TopMargin.HasValue ? _TopMargin.Value : _BottomMargin.Value;
            }
        }

        public bool ViolatesSafeMargin
        {
            get
            {
                double? maxmarg = MaxMargin;
                if (!maxmarg.HasValue) return false;
                return maxmarg < mdSpoutGrid.MinSafeMargin;
            }

        }
        public int SPR { get; set; }

        public double AppliedEndPlateClearance { get; private set; }

        public double ActualEndPlateClearance { get; private set; }

        public double AppliedClearance { get; private set; }

        public double AppliedMargin { get; private set; }

        public double PatternLength { get; private set; }

        /// <summary>
        /// returns True if the spout groups spout bounds was effected by the endplate
        /// </summary>
        public bool LimitedBounds { get => SpoutArea == null ? false : SpoutArea.LimitedBounds; }


        public double MaxWidth
        {
            get
            {
                double _rVal = BoxInsideWidth;
                if (_rVal > 0)  _rVal -=( 2 * AppliedClearance + 2 * SpoutRadius);
                return _rVal < 0 ? 0 : _rVal;
            }
        }

        public double MinVPitch { get => _Spout.Diameter + Pattern.VerticalSpoutGap(); }

        public int TargetCount { get; set; }

        public int TargetRows { get; set; }

        private string _ErrorString;
        public string ErrorString { get => _ErrorString; internal set => _ErrorString = value; }

        public int PanelIndex { get; set; }

        public int GroupIndex { get; set; }

        public int DowncomerIndex => DCInfo == null ? 0 : DCInfo.DCIndex;

        public int BoxIndex { get; set; }

        public new double Depth { get; private set; }

        public double BottomFraction { get; set; }

        /// <summary>
        /// returns Total Area
        /// </summary>
        public double TotalArea { get; private set; }

        public bool HPitchLock { get; private set; }

        public bool VPitchLock { get; private set; }

        public bool LengthLock { get; private set; }

        public bool PatternLock { get; private set; }

        public bool CountLock { get; private set; }

        public bool ApplyMargins { get; set; }

        public bool Metric { get; set; }

        public double LastLength { get; set; }

        public bool TriangleEndPlate { get => !IsRectangular(); }

        public double BoxInsideWidth => DCInfo == null ? 0 : DCInfo.InsideWidth;

        public double BoxY => SpoutArea == null ? 0 : SpoutArea.BoxY;

        public DowncomerInfo DCInfo => SpoutArea == null ? null : SpoutArea.DCInfo;

        private double _ErrorLimit;
        public double ErrorLimit { get => _ErrorLimit <= 0 ? 2.5 : _ErrorLimit; set => _ErrorLimit = value; }

        /// <summary>
        /// the rectangle that encloses all of the spouts in the group
        /// </summary>
        /// <returns></returns>
        public uopRectangle SpoutLimits => new uopRectangle(_Spouts.BoundaryRectangle);

        /// <summary>
        /// the rectangle thats properties represent the margins of the spout group
        /// </summary>
        /// <returns></returns>
        public uopRectangle Margins => new uopRectangle(_Margins);

        private double _TargetArea;
        /// <summary>
        /// returns Target Area
        /// </summary>
        public double TargetArea
        {
            get => _TargetArea;

            set
            {
                if (double.IsNaN(value))
                    value = 0;

                _TargetArea = value;
            }
        }


        /// <summary>
        /// returns Error Percentage
        /// </summary>
        public double ErrorPercentage => (TargetArea > 0) ? ((TotalArea / TargetArea) - 1) * 100 : 0;

        public int SpoutCount => _Spouts.Count;

        /// <summary>
        /// returns Spouts
        /// </summary>
        public uopHoles Spouts { get { _Spouts.Centers = new UVECTORS(_Rows.Points(false)); return new uopHoles(_Spouts); } }

        /// <summary>
        /// returns Spout Diameter
        /// </summary>
        public double SpoutDiameter => SpoutRadius * 2;

        /// <summary>
        /// returns Spout Radius
        /// </summary>
        public double SpoutRadius { get => _Spout.Radius; internal set => _Spout.Radius = value; }

        /// <summary>
        /// returns Spout Length
        /// </summary>
        public double SpoutLength => _Spout.Length;


        /// <summary>
        /// the Y ordinate of the center of the grid points
        /// </summary>
        public override double Y => _SpoutCenter.Y;

        /// <summary>
        /// the X ordinate of the center of the grid points
        /// </summary>
        public override double X => _SpoutCenter.X;

        public double PanelY => SpoutArea == null ? 0 : SpoutArea.PanelY;

        public bool IsSlots => Pattern.UsesSlots();


        private mdSpoutArea _SpoutArea;
        public mdSpoutArea SpoutArea 
        { 
            get 
            { 
                if(_SpoutArea == null)
                {
                    mdSpoutGroup sg = SpoutGroup;
                    if (sg != null) _SpoutArea = mdSpoutArea.CloneCopy(sg.SpoutArea);
                }
                  return _SpoutArea; 
            } 
        
            internal set => _SpoutArea = value; 
        }

        private WeakReference<mdConstraint> _ConstraintsRef;
        
        public mdConstraint Constraints
        {
            get
            {
                if (_ConstraintsRef == null) return null;
                if (!_ConstraintsRef.TryGetTarget(out mdConstraint _rVal)) _ConstraintsRef = null;
                return _rVal == null ? new mdConstraint() : _rVal;
            }
        
            set
            {
                if (value == null) 
                { 
                    _ConstraintsRef = null;
                    PatternLock = false;
                    VPitchLock = false;
                    HPitchLock = false;
                    LengthLock = false;

                    CountLock = false;
                    ApplyMargins = false;

                }
                else
                {
                    _ConstraintsRef = new WeakReference<mdConstraint>(value);

                    //get any constraint property that is not currently set to its default ba
                    TPROPERTIES ndProps = value.ActiveProps.GetByDefaultStatus(false);
                    
                    //set the locks
                    PatternLock = ndProps.Contains("PatternType");
                    VPitchLock = ndProps.Contains("VerticalPitch");
                    HPitchLock = ndProps.Contains("HorizontalPitch");
                    LengthLock = ndProps.Contains("SpoutLength");
                    CountLock = ndProps.Contains("SpoutCount");
                    ApplyMargins = ndProps.Contains("Margin");
                    if (ApplyMargins) VPitchLock = false;

                    if (!PatternLock)
                    {
                        CountLock = false;
                        value.PropValSet("SpoutCount", -1);
                        LengthLock = false;
                        value.PropValSet("SpoutLength", 0);
                        HPitchLock = false;
                        value.PropValSet("HorizontalPitch", 0);
                    }
                }
             
                
            }
        }


        private WeakReference<mdSpoutGroup> _SpoutGroupRef;
        public mdSpoutGroup SpoutGroup
        {
            get
            {
                if (_SpoutGroupRef == null) return null;
                if (!_SpoutGroupRef.TryGetTarget(out mdSpoutGroup _rVal)) _SpoutGroupRef = null;
                return _rVal;
            }
            set
            {
                if (value == null) { _SpoutGroupRef = null;  SpoutArea = null;  return; }
                _SpoutGroupRef = new WeakReference<mdSpoutGroup>(value);
                
                SpoutArea = mdSpoutArea.CloneCopy(value.SpoutArea);
                PanelIndex = value.PanelIndex;
                BoxIndex = value.BoxIndex;
                TargetArea = value.TheoreticalArea;
                if(Constraints == null)
                {
                    Constraints = value.Constraints(null);
                }

            }
        }
        
        internal bool Unbounded { get => !_MinBounds.HasValue  ? true : !_MinBounds.Value.IsDefined ; }
        
        public bool LimitedTop => SpoutArea != null? SpoutArea.LimitedTop : false;

        public bool LimitedBottom => SpoutArea != null ? SpoutArea.LimitedBottom : false;

        public dxxOrthoDirections Direction => SpoutArea == null ? dxxOrthoDirections.Up : SpoutArea.Direction;

        /// <summary>
        /// the default boundary used for spout generation
        /// </summary>

        public uopShape MinBounds
        {
            get
            {
                if (!_MinBounds.HasValue) InitializeBound();
                return _MinBounds.HasValue ? new uopShape(_MinBounds.Value, "MIN BOUNDS") : null;
            }
        }

        /// <summary>
        /// the maimum boundary used for spout generation if the area can't be achieved using the min bounds
        /// </summary>
        public uopShape MaxBounds
        {
            get
            {
                if (!_MaxBounds.HasValue) InitializeBound();
                return _MaxBounds.HasValue ? new uopShape(_MaxBounds.Value, "MAX BOUNDS") : null;
            }
        }

        public double ActualClearance { get {  return (Math.Abs(Margins.Left) < Math.Abs(Margins.Right)) ? Margins.Left : Margins.Right; } }

        /// <summary>
        /// the actual margin of the spouts to bounding downcomer
        /// </summary>
        public double ActualMargin 
        { get 
            {  
                return (Math.Abs(Margins.Bottom) > Math.Abs(Margins.Top) && !(LimitedTop || LimitedBottom)) ? Margins.Top : Margins.Bottom; 
            } 
        }
        public override dxxPitchTypes PitchType { get => Pattern.PitchType() ; set => base._PitchType = Pattern.PitchType(); } 
        
        #endregion Properties

        #region Methods

        public List<uopLine> BoundaryEdges(bool? bMaxBound = null)
        {
            if (Unbounded) InitializeBound();
            if (Unbounded) return new List<uopLine>();
            if (!bMaxBound.HasValue) bMaxBound = UsesMaxBounds;
            return bMaxBound.Value ? _MaxBounds.Value.Vertices.ToLines(true) : _MinBounds.Value.Vertices.ToLines(true);

        }

        public override void Copy(uopGrid aGrid)
        {
            if (aGrid == null) return;
            base.Copy(aGrid);   

            if(aGrid.GetType() == typeof(mdSpoutGrid))
            {
                mdSpoutGrid sptGrid = (mdSpoutGrid)aGrid;
                _Margins = new URECTANGLE(sptGrid._Margins);
                _SpoutCenter = new UVECTOR(sptGrid._SpoutCenter);
                _Spout = new UHOLE(sptGrid._Spout);
                _Spouts = new UHOLES(sptGrid._Spouts);
           
                SpoutArea = mdSpoutArea.CloneCopy(sptGrid.SpoutArea);

                SpoutGroup = sptGrid.SpoutGroup;
                Constraints = sptGrid.Constraints;

                Invalid = sptGrid.Invalid;
                MarginsApplied = sptGrid.MarginsApplied;
                TotalArea = sptGrid.TotalArea;
                GoodSolution = sptGrid.GoodSolution;
                SPR = sptGrid.SPR;
                Pattern = sptGrid.Pattern;
                AppliedEndPlateClearance = sptGrid.AppliedEndPlateClearance;
                ActualEndPlateClearance = sptGrid.ActualEndPlateClearance;
                AppliedClearance = sptGrid.AppliedClearance;
                PatternLength = sptGrid.PatternLength;
                _TargetArea = sptGrid.TargetArea;
                ErrorLimit = sptGrid.ErrorLimit;
                TargetCount = sptGrid.TargetCount;
                ErrorString = sptGrid.ErrorString;
                TargetRows = sptGrid.TargetRows;
                PanelIndex = sptGrid.PanelIndex;
                GroupIndex = sptGrid.GroupIndex;
                Depth = sptGrid.Depth;
                BottomFraction = sptGrid.BottomFraction;
                AppliedMargin = sptGrid.AppliedMargin;

            
                HPitchLock = sptGrid.HPitchLock;
                VPitchLock = sptGrid.VPitchLock;
                PatternLock = sptGrid.PatternLock;
                LengthLock = sptGrid.LengthLock;
                CountLock = sptGrid.CountLock;
                ApplyMargins = sptGrid.ApplyMargins;
                Metric = sptGrid.Metric;
                LastLength = sptGrid.LastLength;
                YOffset = sptGrid.YOffset;
                BoxIndex = sptGrid.BoxIndex;
                ExpandToMargins = sptGrid.ExpandToMargins;
                DesignFamily = sptGrid.DesignFamily;
                UsesMaxBounds = sptGrid.UsesMaxBounds;
                _MinBounds = null;
                _MaxBounds = null;
                if (sptGrid._MinBounds.HasValue) _MinBounds = new USHAPE(sptGrid._MinBounds.Value);
                if (sptGrid._MaxBounds.HasValue) _MaxBounds = new USHAPE(sptGrid._MaxBounds.Value);
                _TopMargin = sptGrid._TopMargin;
                _BottomMargin = sptGrid._BottomMargin;
                GenerationMethod = sptGrid.GenerationMethod;
                SlotLength = sptGrid.SlotLength;
            }


        }
        
        object ICloneable.Clone() => (object)new mdSpoutGrid(this);

        public new mdSpoutGrid Clone() => new mdSpoutGrid(this);

        internal void SetLimits(mdSpoutGroup aSpoutGroup, mdConstraint aConstraints, mdTrayAssembly aAssy)
        {


            OnIsIn = true;
            aSpoutGroup ??= SpoutGroup;
            if (aSpoutGroup == null) return;
            aConstraints ??= Constraints;

            aAssy ??= aSpoutGroup.GetMDTrayAssembly();
            aConstraints ??= aSpoutGroup.Constraints(aAssy);

            DowncomerInfo info = aSpoutGroup.DCInfo;
            uopLinePair panelLimitLines = new uopLinePair(aSpoutGroup.SpoutArea.PanelLimitLines);

            Metric = aAssy != null ? aAssy.MetricSpouting : true;
            ErrorLimit = aAssy != null ? aAssy.ErrorLimit : 2.5;
            YOffset = aConstraints != null ? aConstraints.YOffset : 0;
            PanelIndex = aSpoutGroup.PanelIndex;
            GroupIndex = aSpoutGroup.GroupIndex;
            BoxIndex = aSpoutGroup.BoxIndex;
            Depth = info.Thickness;
   
            _Margins = new URECTANGLE(info.X_Inside_Left, panelLimitLines.GetSide(uppSides.Top).Y(), info.X_Inside_Right, panelLimitLines.GetSide(uppSides.Bottom).Y());
            aSpoutGroup.Limits = new URECTANGLE(_Margins);

            double dia = (!Metric) ? 0.75 : 19 / 25.4;
            SpoutRadius = aConstraints.SpoutDiameter / 2.0;
            if (SpoutRadius <= 0) SpoutRadius = dia / 2;

            //if (SpoutRadius <= 0)SpoutRadius = 0.375;

         
            TargetArea = aSpoutGroup.TheoreticalArea;
            if(SpoutArea != null)
            {

                aConstraints?.PropValSet("TreatAsIdeal", SpoutArea.TreatAsIdeal, bSuppressEvnts:true) ;
                if (SpoutArea.TreatAsIdeal)
                {
                    TargetArea = SpoutArea.OverrideSpoutArea.Value;

                }
                aConstraints?.PropValSet("OverrideSpoutArea", SpoutArea.TreatAsIdeal ? SpoutArea.OverrideSpoutArea.Value : 0, bSuppressEvnts: true);
            }
            else
            {
                if (aConstraints.TreatAsIdeal)
                    TargetArea = aConstraints.OverrideSpoutArea;

            }

            AppliedEndPlateClearance = aConstraints.EndPlateClearance;
            if (AppliedEndPlateClearance <= 0) AppliedEndPlateClearance = 0.25;

            //set the edge clearance
            if (GroupIndex != 1) aConstraints.PropValSet("EndPlateClearance", 0);

            AppliedClearance = aConstraints.Clearance;
            if (AppliedClearance <= 0)
            {
                AppliedClearance = info.SpoutGroupClearance;
                aConstraints.PropValSet("Clearance", 0);
            }
            else
            {
                if (AppliedClearance <= 0.0625)
                {
                    AppliedClearance = 0.0625;
                    aConstraints.PropValSet("Clearance", AppliedClearance);
                }
                if (mzUtils.CompareVal(AppliedClearance, info.SpoutGroupClearance, aPrecis: 3) == mzEqualities.Equals)
                {
                    aConstraints.PropValSet("Clearance", 0!);
                }
            }
            
            double dcWd = BoxInsideWidth / 2;
            double bWd = info.BoxWidth / 2.0;
            AppliedMargin = aConstraints.Margin;
            ApplyMargins = AppliedMargin != 0;

            _Margins.Bottom += AppliedMargin;
            if (PanelIndex > 0) _Margins.Top -= AppliedMargin;

           
        }

 
        /// <summary>
        /// creates the spout that will be used in the pattern (hole or slot)
        /// </summary>
        /// <param name="aDowncomer"></param>
        /// <param name="aConstraints"></param>
        public void SetSpout()
        {
          

            if (Pattern == uppSpoutPatterns.SStar)
            {
                _Spout.Length = 2 * SpoutRadius;
                LastLength = 0;
            }
            else
            {
                if (Pattern.UsesSlots()) //S1, S2 or S3
                {
                    
                    _Spout.Length = mdSpoutGrid.MaxSpoutLength(DCInfo, Pattern, Constraints.SpoutLength, true, Metric);
                }
            }
            _Spout.Tag = "SPOUT";
            _Spouts.Member = new UHOLE(_Spout);

            _Spouts.Centers = UVECTORS.Zero;
        }

        public new void Clear()
        {
            _TopMargin = null;
            _BottomMargin = null;
            TotalArea = 0;
            Pattern = uppSpoutPatterns.Undefined;
            base.Clear();
            _Spouts.Centers = UVECTORS.Zero;
            TargetCount = 0;
            TargetRows = 0;

            _MaxBounds = null;
            _MinBounds = null;

        }

        public void RecalculateError()
        {

       
          
            if (_Generating || _Rows == null) return;
            


            if (Pattern == uppSpoutPatterns.SStar)
            {
                TotalArea = _Spouts.Area();
                uopLines rows = _Rows;
                //for (int i = 1; i <= rows.Count; i++)
                //{
                //   uopLine uLn = rows.Item(i);

                //    uopVector u1 = uLn.Points.First;
                //    if (u1 == null) continue;
                //    //if(uLn.Value > 0 && u1.Value <=0)
                //    //    u1.Value = uLn.Value;
                //    //uLn.Points.SetItem(1, u1);
                //    //Grid.Rows.SetItem(i, uLn);

                //    TotalArea += mdSpoutGrid.SingleSpoutArea(_Spout, u1.Value);
                //}
            }
            else
            {
                TotalArea = _Spouts.Count * mdSpoutGrid.SingleSpoutArea(_Spout);
            }

            mdSpoutGrid.TabulateError(TargetArea, TotalArea, out _, out _);

            _SpoutCenter = (_Spouts.Centers.Count > 0) ? _Spouts.Centers.Bounds.Center : new UVECTOR( TargetX,TargetY);

        }


        /// <summary>
        /// tabulates the target spout count based on the theoretical area of the spout group and the spout area
        /// </summary>
        /// <param name="aConstraints"></param>
        /// <param name="aSpoutLength"></param>
        /// <returns></returns>
        private int ComputeRequiredCount( double aSpoutLength = 0)
        {

            if (Unbounded) InitializeBound();
            if (Unbounded) return 0 ;
            mdConstraint aConstraints = Constraints;
            int _rVal = 0;
            if (aSpoutLength == 0) aSpoutLength = _Spout.Length;

            double aArea = mdSpoutGrid.SingleSpoutArea(_Spout, aSpoutLength);
            bool bRoundUp = false;
            double sPtch = 0;


            if (Pattern != uppSpoutPatterns.SStar)
            {
                if (aArea <= 0) return _rVal;

                if (!CountLock)
                {
                    bRoundUp = IsSlots && !LengthLock;
                    _rVal = Convert.ToInt32(uopUtils.RoundTo(TargetArea / aArea, dxxRoundToLimits.One, bRoundUp));

                    if (_rVal == 0 & TargetArea > 0) _rVal = 1;

                }
                else
                {
                    _rVal = aConstraints.SpoutCount;
                }
            }
            else
            {
                sPtch = aConstraints.VerticalPitch;
                if (sPtch <= 0)
                {
                    sPtch = (2 * SpoutRadius) + Pattern.VerticalSpoutGap();
                    sPtch = uopUtils.RoundTo(sPtch, dxxRoundToLimits.Sixteenth);
                }
                _rVal = Convert.ToInt32(Math.Truncate(_MaxBounds.Value.Height / sPtch)) + 1;

                if (CountLock) _rVal = aConstraints.SpoutCount;

            }
            TargetCount = _rVal;

            SizeSpout();
            return _rVal;
        }


        private void SizeSpoutsSStar()
        {
            if (Unbounded || _Generating || _Rows == null) return;

             List<uopLine> bnds = BoundaryEdges();
       
           
            USEGMENTS aSegs =new USEGMENTS( Segments);
            URECTANGLE limits = BoundsV;
            int j = 0;
            uopVector u1;
            uopVectors nPts = new uopVectors( _Spouts.Centers);
            uopLine aLn;

            uopVector u3 = uopVector.Zero;
            double lX = 0;
            int idx = 0;
            double aArea = _Spouts.Area();;
            double longestArea = 0;
            double otherArea = 0;
            double dTarget = 0;
            double newLen = 0;
            double redux = 0;
            double rad = SpoutRadius;
            List<double> sLengths = _Spouts.Lengths();
            uopVector longestSpout;
            List<int> longestSpouts = new List<int>();
            List<int> otherSpouts = new List<int>();
            uopVector nextSpout = uopVector.Zero;
            uopLines nLns = new uopLines();

            uopLines rows = _Rows;

            //for (int i = 1; i <= rows.Count   ; i++)
            //{
            //    uopLine row = rows.Item(i);
            //    row.Row = i;

            //    if (row.Points.Count > 0)
            //    {
            //        u1 = row.Points.Item(1);
            //        if (ApplyMargins)
            //        {
            //            if (Math.Round(u1.Y, 4) < Math.Round(_MinBounds.Value.Bottom, 4)) break;

            //        }

            //        //if (Math.Round(u1.Y, 4) < Math.Round(BoundsV.Top, 4))
            //        //{
            //        //    uopVectors ips = row.Intersections(bnds, aLinesAreInfinite: false,aLineIsInfinite:true, bNoDupes:true   );
            //        //    if (ips.Count > 0)
            //        //    {
            //        //        u2 = ips.Farthest(u1);
            //        //        u3 = u1.MidPt(u2);
            //        //        u3.Value = u2.X - u1.X + 2 * rad;
            //        //        if (u3.Value < 2 * rad) 
            //        //            u3.Value = 2 * rad;

            //        //    }
            //        //else
            //        //{
            //        //    u3.Value = 2 * rad; 
            //        //}
            //            //}
            //            //else
            //            //{
            //            //    u2 = new UVECTOR(Vertices.Item(1));
            //            //    u3 = new UVECTOR(u1);
            //            //    u3.Value = u2.X - u1.X + 2 * rad;
            //            //    if (u3.Value < 2 * rad) u3.Value = 2 * rad;

            //            //    u3.X = u1.X + 0.5 * u3.Value - rad;
            //            //}

            //        //    u3.Index = i;
            //        aArea += mdSpoutGrid.SingleSpoutArea(_Spout, u3.Value);
            //        sLengths.Add(u3.Value);
            //        nPts.Add(u3);
            //        if (!CountLock)
            //        { if (Math.Round(aArea, 3) >= Math.Round(TargetArea, 3)) break; }
            //        else
            //        { if (nLns.Count >= TargetCount) break; }

            //    }
            //}
            //now shrink them to achieve the area
            TotalArea = aArea;
           // return;

            while (Math.Round(aArea, 3) > Math.Round(TargetArea, 3))
            {
                longestArea = 0;
                otherArea = 0;

                //find the longest spout
                lX = -1;
                idx = 0;
                for (int i = 1; i <= sLengths.Count; i++)
                {
                    if (sLengths[i - 1] > lX)
                    {
                        lX = sLengths[i - 1];
                        idx = i;
                    }
                }
                if (idx == 0) break;

                longestSpout = nPts.Item(idx, bReturnClone:true);
                if (longestSpout.Value <= 2 * rad) break;

                longestSpouts.Clear();
                otherSpouts.Clear();

                for (int i = 1; i <= nPts.Count; i++)
                {
                    u1 = nPts.Item(i);
                    if (u1.Value == longestSpout.Value)
                    {
                        longestSpouts.Add(i);
                        longestArea += mdSpoutGrid.SingleSpoutArea(_Spout, longestSpout.Value);
                    }
                    else
                    {
                        otherSpouts.Add(i);
                        otherArea += mdSpoutGrid.SingleSpoutArea(_Spout, nPts.Item(i).Value);
                    }
                }
                dTarget = TargetArea - otherArea;
                newLen = uopUtils.IdealSpoutLength(dTarget, longestSpouts.Count, rad, Metric, longestSpout.Value, true);
                if (otherSpouts.Count > 0)
                {
                    nextSpout = nPts.Item(otherSpouts[otherSpouts.Count - 1]);
                    if (newLen < nextSpout.Value) newLen = nextSpout.Value;

                    redux = longestSpout.Value - newLen;
                    aArea = otherArea;
                    if (Math.Round(redux, 3) == 0) break;

                    for (int i = 1; i <= longestSpouts.Count; i++)
                    {
                        j = longestSpouts[i - 1];
                        u1 = nPts.Item(j);

                        u1.Value = newLen;
                        u1.X -= redux / 2;
                        aArea += mdSpoutGrid.SingleSpoutArea(_Spout, newLen);
                        //nLns.SetValue(j, u1.Value);

                    }
                    if (Math.Round(aArea, 3) <= Math.Round(TargetArea, 3)) break;

                }
                else
                {
                    for (int i = 1; i <= nPts.Count; i++)
                    {
                        u1 = nPts.Item(i);
                        redux = u1.Value - newLen;

                        u1.Value = newLen;
                        u1.X -= redux / 2;
                        //nLns.SetValue(i, u1.Value);
                    }
                    break;//exit while loop
                }
            }

            aArea = 0;
            _Spouts.Centers = UVECTORS.Zero;
            dxxRoundToLimits rndTo = Metric ? dxxRoundToLimits.Millimeter : dxxRoundToLimits.Sixteenth;
            for (int i = 1; i <= nPts.Count; i++)
            {
                u1 = nPts.Item(i, bReturnClone:true);
                if (u1.Row > _Rows.Count) break;
                u1.Value = uopUtils.RoundTo(u1.Value, rndTo, false, true);
                aArea += mdSpoutGrid.SingleSpoutArea(_Spout, u1.Value);
                aLn = _Rows.Item(u1.Row);
                //aLn.Value = u1.Value;
                aLn.Points[0].SetCoordinates(u1.X, u1.Y);
                aLn.Points[0].Value = u1.Value;
                _Spouts.Centers.Add(u1);
            }
            
            //_Rows.Populate( nLns);
            //TotalArea = _Spouts.Area();
            RecalculateError();
        }


        private void SizeSpout()
        {
            SlotLength = _Spout.Radius * 2;
            _Spout.Length = SlotLength;
            if (Pattern == uppSpoutPatterns.SStar || !IsSlots) return;
            if ( IsSlots)
            {
                double maxlen = mdSpoutGrid.MaxSpoutLength(DCInfo, Pattern, Constraints.SpoutLength, true, Metric);
                double slotarea = SingleSpoutArea(_Spout, maxlen);
                if (!CountLock)
                {
                    TargetCount = (int)Math.Truncate(TargetArea / slotarea);
                    if (TargetCount * slotarea < TargetArea) TargetCount++;

                }

                SlotLength = LengthLock ? Constraints.SpoutLength : uopUtils.IdealSpoutLength(TargetArea, TargetCount, SpoutRadius, Metric, aMaxLength: maxlen, bSuppressRound: false);
            }
            _Spout.Length = SlotLength;


            if (LengthLock ||  TargetCount <= 0 )
                return;
       
            int cnt1 = TargetCount;
            double len2 = uopUtils.IdealSpoutLength(TargetArea, cnt1, SpoutRadius, Metric, aMaxLength: _Spout.Length, bSuppressRound: false);

            _Spout.Length = len2;
            TargetCount = cnt1;
            
            if (TargetCount > 1 && !CountLock)
            {
               int cnt2 = TargetCount - 1;
                 double len1= uopUtils.IdealSpoutLength(TargetArea, cnt2, SpoutRadius, Metric, aMaxLength: _Spout.Length, bSuppressRound: false);

                double sArea1 = mdSpoutGrid.SingleSpoutArea(_Spout, len1) * cnt1;
                double sArea2 = mdSpoutGrid.SingleSpoutArea(_Spout, len2) * cnt2;
                mdSpoutGrid.TabulateError(TargetArea, sArea1, out _, out double Err1);
                mdSpoutGrid.TabulateError(TargetArea, sArea2, out _, out double Err2);

                if (Math.Abs(Err2) < Math.Abs(Err1))
                {
                    _Spout.Length = len2;
                    TargetCount = cnt2;

                }
            }

            SlotLength = _Spout.Length;
            _Spouts.Member = new UHOLE(_Spout);
            SetHPitch();
        }

        /// <summary>
        /// sets constant distance between spouts in the X direction dictated by the pattern type
        /// </summary>
        /// <param name="aConstraints"></param>
        private void SetHPitch()
        {

            mdConstraint aConstraints = Constraints;
            
            if (Pattern == uppSpoutPatterns.S1 || Pattern == uppSpoutPatterns.SStar)
            {
                HPitch = 0;
            }
            else
            {
                //get the default horiz. pitch for the pattern
                double defVal = mdSpoutGrid.GetHorizontalPitch(Pattern, _Spout.Length);

                HPitch = HPitchLock ? aConstraints.HorizontalPitch : defVal;

                if (HPitch - _Spout.Length < 0.0625) HPitch = _Spout.Length + 0.0625;

                if (HPitch == defVal)
                {
                    aConstraints.PropValSet("HorizontalPitch", 0, bSuppressEvnts:true);
                    HPitchLock = false;
                }
            }
        }
        /// <summary>
        /// sets the intitial vertical pitch depending on the current pattern type
        /// </summary>
        /// <param name="aConstraints"></param>
        /// <param name="aRowTarget"></param>
        private void SetVPitch( int aRowTarget)
        {
            mdConstraint aConstraints = Constraints;

            SPR = SpoutsPerRow(this);

            if (VPitchLock)
            {
                VPitch = aConstraints.VerticalPitch;
                if (VPitch <= 0) VPitchLock = false;

            }
            if (Pattern == uppSpoutPatterns.SStar || TargetCount <= 0)
            {
                if (!VPitchLock) VPitch = MinVPitch;

            }
            else
            {

                if (Unbounded)
                {
                    VPitch = MinVPitch;
                    TargetRows = 0;
                }
                else
                {
                    if (!ExpandToMargins)
                    {
                        VPitch = !VPitchLock ? MinVPitch : aConstraints.VerticalPitch;
                    }
                    else
                    {
                        double ht = _MinBounds.Value.Height;
                       
                        if (Pattern == uppSpoutPatterns.A && aRowTarget <= 0)
                        {

                            TargetRows = TargetCount / (2 * SPR - 1);
                            double dif = TargetCount - (TargetRows * (2 * SPR - 1));
                            if (dif > 0)
                            {
                                if (dif <= SPR)
                                { TargetRows = (2 * TargetRows) + 1; }
                                else
                                { TargetRows = (2 * TargetRows) + 2; }
                            }
                        }
                        else
                        {
                            if (aRowTarget <= 0)
                            {
                                SPR = SpoutsPerRow(this);
                                TargetRows =(int)Math.Truncate((double)TargetCount / SPR);
                                double dif = TargetCount - (TargetRows * SPR);
                                if (dif > 0) TargetRows += 1;

                            }
                            else
                            { TargetRows = aRowTarget; }

                        }
                        if (!VPitchLock)
                        {
                            VPitch = uopUtils.RoundTo( (TargetRows > 1) ? ht / (uopUtils.IsOdd(TargetRows) ? TargetRows : TargetRows - 1) : 0, dxxRoundToLimits.Sixteenth,bRoundDown:true);
                        }
                    }

                    
                }
                
            }

            if (!VPitchLock)
            {
                if (VPitch < MinVPitch) VPitch = MinVPitch;

                VPitch = uopUtils.RoundTo(VPitch, dxxRoundToLimits.Sixteenth);
            }
        }

        bool _ReGenerating;

        private void ReGenerate()
        {
            if (_ReGenerating) return;

            _ReGenerating = true;
            try
            {
                Generate();
            }
            finally
            {
                _ReGenerating = false;
            }
        }
        

        protected override uopLines Create_Columns()
        {

            if (_Cols != null) _Cols.Clear(); else _Cols = new uopLines() { Name = "Columns" };
            URECTANGLE lims = BoundsV;
            if (Pattern == uppSpoutPatterns.SStar)
            {
                _Cols.Add(_Origin.X, lims.Top, _Origin.X, lims.Bottom, aCol:1);
                return _Cols;
            } 
            else if(Pattern == uppSpoutPatterns.S1)
            {
                MaxCols = 1;
                _Cols.Add(DCInfo.X, lims.Top, SpoutArea.X, lims.Bottom, aCol:Cols.Count + 1);
                return _Cols;
            }
            else
            {
              return   base.Create_Columns();
            }
        }

        public uppSides TallSide(out uopLine rTestLine)
        {
            rTestLine = null;
            
            uopVector u1 = null;
            uopVector u2 = null;

            if (Direction == dxxOrthoDirections.Up)
            {
                u1 = Vertices.GetVector(dxxPointFilters.GetLeftTop, aPrecis: 1, bGetClone: true);
                u2 = Vertices.GetVector(dxxPointFilters.GetRightTop, aPrecis: 1, bGetClone: true);
                rTestLine = u1.Y >= u2.Y ? new uopLine(u1, u2) : new uopLine(u2, u1);
            }
            else
            {
                u1 = Vertices.GetVector(dxxPointFilters.GetLeftBottom, aPrecis: 1, bGetClone: true);
                u2 = Vertices.GetVector(dxxPointFilters.GetRightBottom, aPrecis: 1, bGetClone: true);
                rTestLine = u1.Y <= u2.Y ? new uopLine(u1, u2) : new uopLine(u2, u1);

            }

            if (!IsRectangular())
            {

                if (Direction == dxxOrthoDirections.Up)
                {
                    return u1.Y >= u2.Y ? uppSides.Left : uppSides.Right;
                }
                else
                {
                    return u1.Y <= u2.Y ? uppSides.Left : uppSides.Right;

                }

            }
            else
            {

                return DCInfo.X >= 0 ? uppSides.Left : uppSides.Right;

            }
        }  

        protected override void Create_Origin()
        {
            _OverrideOrigin = null;
            OnIsIn = true;
            uopShape SA = SpoutArea;
            if (SA == null) return;

            if (Pattern == uppSpoutPatterns.SStar)
            {
                
                MaxRows = 0;
            
                uopVectors verts = Vertices; // the boundary verttices
                MaxCols = 1;


                //find the best Origin
                if(TallSide(out uopLine horizontal) == uppSides.Left)
                    Alignment = Direction == dxxOrthoDirections.Up ? uppGridAlignments.TopLeft : uppGridAlignments.BottomLeft;
                else
                    Alignment = Direction == dxxOrthoDirections.Up ? uppGridAlignments.TopRight : uppGridAlignments.BottomRight;

                    _OverrideOrigin = new UVECTOR(horizontal.sp);
                base.Create_Origin();
            }
            else 
            {
                if (Pattern.UsesSlots())
                {
                    if (Pattern == uppSpoutPatterns.S2)
                        XOffset = 0.5 * HPitch;
                }

           

                base.Create_Origin();

                if (Pattern == uppSpoutPatterns.S1)
                {
                    _Origin.X = DCInfo.X;

                }

                //push the limited nonsquare spouts as close to the limit line as they can be
                if (!IsRectangular())
                {

                    if(LimitedTop && LimitedBottom)
                    {
                        Alignment = uppGridAlignments.MiddleCenter;
                        _Origin = new UVECTOR(DCInfo.X, PanelY);
                        //'reset so they will get regenerated
                        _Cols = new uopLines() { Name = "Columns" };
                        _Rows = new uopLines() { Name = "Rows" };
                    }
                    else
                    {
                       if( LimitedTop || LimitedBottom)
                        {
                            if (DesignFamily.IsStandardDesignFamily())
                            {
                                List<uopLine> cols = base.Create_Columns();
                                if (cols.Count > 0)
                                {
                                    uopLine line = null;
                                    cols = cols.OrderByDescending(x => x.sp.X).ToList();
                                    if (TallSide(out _) == uppSides.Left)
                                    {
                                        line = cols.LastOrDefault();
                                        if (Pattern.UsesSlots())
                                        {
                                            line.Move(0.5 * SlotLength - SpoutRadius);
                                        }
                                    }
                                    else
                                    {
                                        line = cols.FirstOrDefault();
                                        if (Pattern.UsesSlots())
                                        {
                                            line.Move(-0.5 * SlotLength + SpoutRadius);
                                        }
                                    }


                                    uopVector ip = line.Intersections(Segments).GetVector(Direction == dxxOrthoDirections.Up ? dxxPointFilters.AtMaxY : dxxPointFilters.AtMinY);
                                    _Origin.Y = ip == null ? 0 : ip.Y;
                                }
                               



                                    //'reset so they will get regenerated
                                    
                            }
                            else
                            {
                                _Origin.Y = SpoutArea.PanelLimits.Y;
                            }
                        }
                        
                        _Cols = new uopLines() { Name = "Columns" };
                        _Rows = new uopLines() { Name = "Rows" };
                    }



                }
            }

      
            
        }

        public override void Generate()
        {

            if (_Generating)
                return;

            _Generating = true;
            base.EventGenerationComplete -= MyGenerationCompleteHandler;
            base.EventGenerationComplete += MyGenerationCompleteHandler;
            base.EventGenerationStart -= MyGenerationStartHandler;
            base.EventGenerationStart += MyGenerationStartHandler;
            base.EventGridPointAdded -= MyGridPointAddedHandler;
            base.EventGridPointAdded += MyGridPointAddedHandler;

            try
            {


                //PartialRowLocation = IsRectangular() ? Direction == dxxOrthoDirections.Up ? "BOTTOM" : "TOP" : string.Empty;
                // create the grid of points which are the spout grids sout centers 

                UVECTOR v1 = UVECTOR.Zero;
                ULINE l1 = ULINE.Null;
                URECTANGLE limits = BoundsV;

            
                if(Pattern.UsesSlots()) MaxCols = mdSpoutGrid.SpoutsPerRow(this);
                if (Pattern != uppSpoutPatterns.SStar)
                    MaxCount = TargetCount;

                if (Pattern == uppSpoutPatterns.A || Pattern == uppSpoutPatterns.Astar)
                {
                    if (Pattern == uppSpoutPatterns.Astar) XOffset = HPitch / 4;
                }

                uppVerticalAlignments valign = uppVerticalAlignments.Center;
                uppHorizontalAlignments halign = uppHorizontalAlignments.Center;
                if (LimitedTop || LimitedBottom)
                {
                    if (IsRectangular())
                    {
                        valign = LimitedTop ? uppVerticalAlignments.Top : uppVerticalAlignments.Bottom;
                    }
                    else
                    {
                        uopVector u1 = Vertices.GetVector(Direction == dxxOrthoDirections.Up ? dxxPointFilters.AtMaxY : dxxPointFilters.AtMinY);
                        if(u1 != null)
                        {
                            valign = LimitedTop ? uppVerticalAlignments.Top : uppVerticalAlignments.Bottom;
                            if (Math.Round(SpoutArea.Width, 2) < Math.Round(SpoutArea.DCInfo.InsideWidth, 2))
                                halign = u1.X < limits.X ? uppHorizontalAlignments.Left : uppHorizontalAlignments.Right;
                            else
                                halign = uppHorizontalAlignments.Center;
                        }
                       
                    }


                }
                if (!DesignFamily.IsStandardDesignFamily()) 
                {
                    valign = uppVerticalAlignments.Center;
                    halign = uppHorizontalAlignments.Center;
                }
                Alignment = uopUtils.GetGridAlignment(valign, halign);
                base.Generate();
               


            }
            catch (Exception ex) { _ErrorString = ex.Message; }
            finally
            {
                _Generating = false;
           
            }
           
        }

        public uopShape SpoutCenterBoundary(bool bMaxed = true, bool bIgnoreMargins = false) 
        {
            USHAPE? shape = SpoutCenterBounds(bMaxed, bIgnoreMargins);
            return shape.HasValue ? new uopShape(shape.Value) : null;
        }

        /// <summary>
        /// computes the boundary where the spout centers can reside without the spout perimeters violating clearances or margins
        /// </summary>
        /// <param name="bMaxed">flag to return the max boundary that laps into the downcomers below</param>
        /// <returns></returns>
        private USHAPE? SpoutCenterBounds(bool bMaxed = true, bool bIgnoreMargins = false)
        {

            if (SpoutArea == null) return null;

            double rad = SpoutRadius;
            double clrc = AppliedClearance + rad;
            double epClrc = AppliedEndPlateClearance + rad;
            double bxOutside = DCInfo == null ? 0 : DCInfo.BoxWidth / 2;
            double marg = ApplyMargins && !bIgnoreMargins ? Constraints.Margin : 0;
            if (marg > 0) bMaxed = false;
            if (LimitedTop && LimitedBottom) bMaxed = false;
            if (marg <= 0) marg = bMaxed ?  -(bxOutside + rad) : rad ;
            uopVectors saverts = new uopVectors(SpoutArea.Vertices, bCloneMembers: true);
            URECTANGLE limits = SpoutArea.BoundsV;
            uopVector ctr = new uopVector(limits.Center);
            uopVectors pts = SpoutArea.BoxLimitLines.EndPoints();
            
            //get the limit line
            uopLine limitline =  Direction == dxxOrthoDirections.Up ?
                new uopLine(pts.GetVector(dxxPointFilters.GetLeftTop), pts.GetVector(dxxPointFilters.GetRightTop)) :
                new uopLine(pts.GetVector(dxxPointFilters.GetLeftBottom), pts.GetVector(dxxPointFilters.GetRightBottom)) ;
         
            //get the lines with their endpoints linked to thr vertices so if we move them, the points move and the other segments that share endpoints will be affected
            uopLines segs = saverts.LineSegments(bClosed: true, bLinked : false);
            List<uopLine> topsANDbots = segs.FindAll((x) => !x.IsVertical(2));

            //get the top edge(s)
            uopLines tops =  new uopLines(topsANDbots.FindAll((x) =>  x.MaxYr(aPrecis: 2) >  Math.Round(SpoutArea.Y, 2)), bAddClones:false );
        uopLine top = tops.Find((x) => !x.IsVertical(2) && x.MaxYr(aPrecis: 2) == Math.Round(limits.Top, 2));
            uopLine top2 = tops.Count > 1 ?  tops.Find((x) => x != top) : null;
            if(tops.Count == 2)
            {
                if(!top.IsHorizontal(2) && top2.IsHorizontal(2))
                {
                    uopLine swap = top2;
                    top2 = top;
                    top = swap;
                }
            }

            //get the bottom edge(s)
            uopLines bots = new uopLines(topsANDbots.FindAll((x) =>  x.MidPt.Y < limits.Y) , bAddClones:false);
            uopLine bot = bots.Find((x) => !x.IsVertical(2) && x.MinYr(aPrecis: 2) == Math.Round(limits.Bottom, 2));
            uopLine bot2 = bots.Count > 1 ? bots.Find((x) => x != bot) : null;
            if (bots.Count == 2)
            {
                if (!bot.IsHorizontal(2) && bot2.IsHorizontal(2))
                {
                    uopLine swap = bot2;
                    bot2 = bot;
                    bot = swap;
                }
            }
            //apply the clearance the left edge 
            uopLine left = segs.Find((x) => x.IsVertical(2) && x.X(aPrecis: 2) == Math.Round(limits.Left, 2));
            left?.Move(clrc );
            //apply the clearance the right edge 
            uopLine right  = segs.Find((x) => x.IsVertical(2) && x.X(aPrecis: 2) == Math.Round(limits.Right, 2));
            right?.Move(-clrc );

            bool triangular = right == null || left == null;

            uopLinePair verticaledges = new uopLinePair() { Line1 = left, Line2 = right};
            uopLinePair horizontaledges = new uopLinePair() { Line1 = top, Line2 = bot }; 

             limits = segs.EndPoints().Bounds;
            ctr = new uopVector(limits.Center);

            if (top != null)
            {
                if (!LimitedTop && tops.Count == 1)
                {

                    //apply the margin to the top edge
                    top.Move(0, -marg);
                    top.ExtendTo(verticaledges, bTrimTo: true, bExtendTheLine: true);
                }
                else
                {
                    //apply the end plate clearance to the top edge
                    if(top2 == null)
                    {
                        if (top.IsHorizontal(2))
                            top.Move(0, -epClrc);
                        else
                            top.Offset(epClrc, ctr);

                        top.ExtendTo(verticaledges, bTrimTo: true, bExtendTheLine: true);
                        //left?.ExtendTo(horizontaledges, bTrimTo: true);
                        //right?.ExtendTo(horizontaledges, bTrimTo: true);
                    }
                    else
                    {
                        
                        top2.Offset(epClrc, ctr);
                        top2.ExtendTo(right, bTrimTo: true, bExtendTheLine: true);
                        top2.ExtendTo(left, bTrimTo: true, bExtendTheLine: true);
                        top.Move(0, -marg);
                       
                        if (top.Y() >= top2.MaxY)
                        {
                            segs.Remove(top);
                        }
                        else
                        {
                            top.ExtendTo(top2, bTrimTo: true, bExtendTheLine: true);
                            if (top.MinX < top2.MinX)
                                top.ExtendTo(left, bTrimTo: true, bExtendTheLine: true);
                            else
                                top.ExtendTo(right, bTrimTo: true, bExtendTheLine: true);
                        }
                           

                       

                    }
                }

                limits = segs.EndPoints().Bounds;
             
                ctr = new uopVector(limits.Center);
            }

            if (bot != null)
            {
                if (!LimitedBottom && bots.Count ==1)
                {
                    //apply the margin to the bottom edge
                    bot.Move(0, marg);
                    bot.ExtendTo(verticaledges, bTrimTo: true, bExtendTheLine:true);
                }
                else
                {
                    //apply the end plate clearance to the bottom edge
                    if (bot2 == null)
                    {
                        if (bot.IsHorizontal(2))
                        { bot.Move(0, epClrc); }
                        //else
                        //{ if(LimitedTop && LimitedBottom) bot.Offset(epClrc, ctr); }

                        bot.ExtendTo(verticaledges, bTrimTo: true, bExtendTheLine: true);
                        //left?.ExtendTo(horizontaledges, bTrimTo: true);
                        //right?.ExtendTo(horizontaledges, bTrimTo: true);
                    }
                    else
                    {
                        bot2.Offset(epClrc, ctr);
                        bot2.ExtendTo(right, bTrimTo: true, bExtendTheLine: true);
                        bot2.ExtendTo(left, bTrimTo: true, bExtendTheLine: true);
                        bot.Move(0, marg);

                        if (bot.Y() <= bot2.MinY)
                        {
                            segs.Remove(bot);
                        }
                        else
                        {
                            bot.ExtendTo(bot2, bTrimTo: true, bExtendTheLine: true);
                            if (bot.MinX < bot2.MinX)
                                bot.ExtendTo(left, bTrimTo: true, bExtendTheLine: true);
                            else
                                bot.ExtendTo(right, bTrimTo: true, bExtendTheLine: true);
                        }


                    }

                }

                limits = segs.EndPoints().Bounds;
                ctr = new uopVector(limits.Center);
            }

            //special cases
            if (triangular)
            {
                double ylimit = Direction == dxxOrthoDirections.Up ? limitline.MinY : limitline.MaxY;
                uopLine hline = Direction == dxxOrthoDirections.Up ? bot!= null? bot : top != null ? top: bot : null;
                uopLine nonhline = Direction == dxxOrthoDirections.Up ? top : bot;
               
                if (hline != null)
                {
                    mzEqualities compval = mzUtils.CompareVal(hline.Y(), ylimit) ;
                    uopLine limiter = right == null ? new uopLine(limitline.MaxX - clrc, hline.Y(), limitline.MaxX - clrc, limitline.MinY) : new uopLine(limitline.MinX - clrc, hline.Y(), limitline.MinX - clrc, limitline.MinY);

                    if (!bMaxed || (bMaxed && Direction == dxxOrthoDirections.Up && compval == mzEqualities.GreaterThan) || (bMaxed && Direction != dxxOrthoDirections.Up && compval == mzEqualities.LessThan))
                    {
                        if (bot != null) bot.ExtendTo(top, bTrimTo: true);
                        if (top != null) top.ExtendTo(bot, bTrimTo: true);
                    }
                    else
                    {
                        if (bot != null) bot.ExtendTo(limiter, bTrimTo: true);
                        if (bot != null) top.ExtendTo(limiter, bTrimTo: true);
                    }
                }
             


            }

            uopVectors verts = segs.EndPoints(bGetClones: true, bNoDupes: true, aNoDupesPrecis: 2);

            return new USHAPE(verts);

        }

        /// <summary>
        /// computes the current margins and returns the bounding rectangle of the spouts
        /// </summary>
        /// <returns></returns>

        public uopRectangle TabulateMargins()
        {
            _TopMargin = null;
            _BottomMargin = null;

            mdSpoutArea sa = SpoutArea;
            URECTANGLE margins = new URECTANGLE(0, 0, 0, 0);
            _Spouts.Centers = new UVECTORS(_Rows.Points(bSuppressVal: false, bGetClones: false));

            URECTANGLE holebounds = new URECTANGLE(new UVECTOR(TargetX,TargetY),0,0);
            ActualEndPlateClearance = 0;
            
            if (_Spouts.Count > 0 && sa != null)
            {
                holebounds = _Spouts.BoundaryRectangle;
           
                margins.Bottom = holebounds.Bottom - sa.Bottom;
                margins.Top = sa.Top - holebounds.Top ; // (PanelIndex > 0) ? _PanelLims.Top - SpoutLimits.Top : margins.Bottom;

                margins.Left = holebounds.Left - sa.Left;
                margins.Right = sa.Right - holebounds.Right;

                if (!sa.LimitedTop) _TopMargin = sa.PanelLims.Top  -holebounds.Top  ;
                if (!sa.LimitedBottom) _BottomMargin = holebounds.Bottom - sa.PanelLims.Bottom;

                if (sa.LimitedBounds)
                {
                    if (sa.BoxLimitLines != null)
                    {
                        uppSides side = sa.LimitedTop ? uppSides.Top : uppSides.Bottom;
                        uopLine aLn = sa.BoxLimitLines.GetSide(side);
                        _Spouts.ArcCenters.NearestToLine(aLn, out double d1);
                        ActualEndPlateClearance = d1 - SpoutRadius;
                    }
                }
            }
            else
            {
                
                holebounds = BoundsV;

            }

            _Margins = margins;
            return new uopRectangle(holebounds);
        }

        public override void Translate(double aXOffSet, double aYOffset, bool bJustMove = false)
        {
            if (aXOffSet == 0 && aYOffset == 0) return;
            base.Translate(aXOffSet, aYOffset, bJustMove);
            if(_Rows != null)
                _Spouts.Centers = new UVECTORS( _Rows.Points(bSuppressVal:false, bGetClones: false));

            _Spout = _Spouts.Member;
        }

        private void CenterSpouts()
        {
            _SpoutCenter = PointCount > 0 ? _Spouts.Centers.Bounds.Center : _MinBounds.HasValue ? new UVECTOR( _MinBounds.Value.X,_MinBounds.Value.Y) : new UVECTOR( SpoutArea.Center) ;
            if (PointCount <= 0 ) return;
            CenterPartialRows();
            if (VerticalAlignment != uppVerticalAlignments.Center) return;
            if (LimitedTop && LimitedBottom) return;

            _SpoutCenter = _Spouts.Centers.Bounds.Center;
            UVECTOR ctr = new UVECTOR(SpoutArea.DCInfo.X, SpoutArea.PanelY);
            double dY = ctr.Y - _SpoutCenter.Y;

            if (dY > 0)
            {
                double d1 = Top - _PointExtremes.Top;
                dY = Math.Min(Math.Abs(dY), Math.Abs(d1));
            }
            else
            {
                double d1 = Bottom - _Spouts.Centers.Bounds.Bottom; // _PointExtremes.Bottom;
                //if (Math.Abs(dY) > Math.Abs(d1)) dY = d1;
                dY = -Math.Min(Math.Abs(dY) , Math.Abs(d1));
                //dY = 0;
                //if(LimitedBottom && IsRectangular())
                //{
                //    dY = d1;
                //}

            }

       
            if (IsRectangular() && dY != 0)
            {
              this.Translate(0, dY, true);
              
            }
            else
            {
                if (_Generating || _ReGenerating) return;
                if (Math.Abs(dY) > 0) //move the spouts up
                {
                    mdSpoutGrid bGrid = new mdSpoutGrid(this);
                    bGrid.YOffset += dY;
                    bGrid.ReGenerate();

                    if (bGrid.GoodSolution)
                        {
                            Copy(bGrid);
                       
                        }
                    
                }
             }

            _Spouts.Centers = new UVECTORS(_Rows.Points(bSuppressVal: false, bGetClones: false));



        }

        public  override  bool IsRectangular(bool bByDiagonal = false, int aPrecis = 2)
        {
            if(SpoutArea == null )   return false;
            if (!SpoutArea.LimitedBounds) return SpoutArea.IsRectangular(true);
            uopLine limiter = LimitLine;
            return (limiter.IsHorizontal(aPrecis));
            //return base.IsRectangular(bByDiagonal, aPrecis);
        }

        public new void Mirror(double? aX, double? aY)
        {
            if (!aX.HasValue && !aY.HasValue) return;
            _Spouts.Centers.Mirror(aX, aY);
            base.Mirror(aX, aY);
        }
        private void CenterPartialRows()
        {
           
            if (_Generating || _ReGenerating) return;
            if (PointCount <= 0 || HorizontalAlignment != uppHorizontalAlignments.Center) return;
            if (Pattern.TriangularPitch() || Pattern == uppSpoutPatterns.S1 || Pattern == uppSpoutPatterns.SStar ) return;
            if (Rows.Count <= 1) return;
            if (LimitedTop && LimitedBottom) return;
            bool changed = false;

            uppSides partialside = Direction == dxxOrthoDirections.Up ? uppSides.Bottom : uppSides.Top;
            if (IsRectangular())
            {
                if (base.SwapPartialRow($"{partialside.Description().ToUpper()}" , out uopLine partialRowLine))
                    changed = true;

                if (partialRowLine != null )
                {
                    uopVectors cntrs = new uopVectors(partialRowLine.Points.FindAll((x) => !x.Suppressed),  bCloneMembers: false);
                    double cx =  cntrs.Bounds.X;
                    double dx = DCInfo.X - cx;
                    if (dx != 0)
                    {
                        changed = true;
                        partialRowLine.Points.Move(dx);
                    }
                        
                }

            }
            else
            {
                uopLine partialRowLine = partialside == uppSides.Top ? TopRow(true) : BottomRow(true);
                if(partialRowLine != null)
                {
                    uopVectors cntrs = new uopVectors(partialRowLine.Points.FindAll((x) => !x.Suppressed), bCloneMembers: false);
                    if (cntrs.Count < SpoutsPerRow(this))
                    {
                        double cx = cntrs.Bounds.X;
                        double dx = DCInfo.X - cx;
                        if (dx != 0)
                        {
                            changed = true;
                            partialRowLine.Points.Move(dx);
                        }
                    }
                }
     
            }


            if (changed)
            {


                _Spouts.Centers = new UVECTORS(_GridPts);

            }

            // //get the first & last row that have defined, upsupressed points
            // uopLine topRow = TopRow(bPopulated: true);
            // uopLine botRow = BottomRow( bPopulated :true);

            // int cntTop = topRow.PointCount(bSuppressedVal: false);
            // int cntBot = botRow.PointCount(bSuppressedVal: false);

            // //if (!_Generating)
            // //{
            // bool swaprows = false;
            // if (Direction == dxxOrthoDirections.Up)
            //     swaprows = cntBot < cntTop && IsRectangular();
            // else
            //     swaprows = cntTop > cntBot && IsRectangular();

            // if (swaprows)
            // {
            //     uopLine temprow = topRow.Clone();
            //     topRow.Copy(botRow);
            //     topRow.Row = temprow.Row;
            //     int idx = botRow.Index;
            //     botRow.Copy(temprow);
            //     botRow.Row = idx;
            //     cntTop = topRow.PointCount(bSuppressedVal:false);
            //     cntBot = botRow.PointCount(bSuppressedVal: false);
            // }

            //// }

            // uopLine targetrow = Direction == dxxOrthoDirections.Up ? botRow : topRow;
            // uopVectors rowpoints =targetrow.GetPoints ((x) => !x.Suppressed);

            // //see if there are any empty locations
            // int cnt1 = targetrow.PointCount(bSuppressedVal: false);

            // if (rowpoints.Count <= 0) return;
            // dxxPointFilters fltr = Direction == dxxOrthoDirections.Up ? HorizontalAlignment == uppHorizontalAlignments.Right ?  dxxPointFilters.GetRightTop : dxxPointFilters.GetLeftTop : HorizontalAlignment == uppHorizontalAlignments.Right ? dxxPointFilters.GetRightBottom : dxxPointFilters.GetLeftBottom;
            // if (Direction == dxxOrthoDirections.Up)
            // {
            //     if (!IsRectangular() && targetrow.Y() >= Vertices.GetVector(fltr).Y) return;
            // }
            // else
            // {
            //     if (!IsRectangular() && targetrow.Y() <= Vertices.GetVector(fltr).Y) return;
            // }




            //// align the points on the center of the boundary
            //double dX = DCInfo.X - rowpoints.Bounds.X;
            //if (dX != 0)
            //    targetrow.Points.Move(dX);

            //if (dX != 0 || swaprows)
            //{
            //    //double? uval = null;
            //    //List<double> uvals = null;
            //    //if (Pattern != uppSpoutPatterns.SStar) uval = _Spout.Length; else uvals = _Spouts.Centers.GetValues();
            //    //_Spouts.Centers = new UVECTORS(GridPoints(aValues: uvals, aUniformValue: uval));  //if (rGrid.Pattern == uppSpoutPatterns.SStar)
            //}
        }

        public override string ToString()
        {
            string _rVal = $"Spout Grid DC:{DowncomerIndex} PNL:{PanelIndex} PAT: {Pattern.Description()}";

            return _rVal;
        }


        /// <summary>
        /// generates the spouts based on the hole descriptor string that is passed
        /// </summary>
        /// <param name="aSpoutGroup"></param>
        /// <param name="aConstraints"></param>
        /// <param name="aDescriptor"></param>
        /// <returns>returns the number of spouts created</returns>
        public int Generate_ByString(string aDescriptor, mdSpoutGroup aSpoutGroup = null, mdConstraint aConstraints = null )
        {

            ErrorString = string.Empty;
            try
            {
                GenerationMethod = GenerationMethods.ByString;
                if (aSpoutGroup == null) aSpoutGroup = SpoutGroup;
                if (aConstraints == null) aConstraints = Constraints;

                // to initialize'
                Clear();
                SpoutGroup = aSpoutGroup;
                Constraints = aConstraints;
                InitializeBound();

                if (string.IsNullOrWhiteSpace(aDescriptor)) return 0;

                UVECTOR u1 = UVECTOR.Zero;
                UVECTORS rwPts;

                UVECTORS mxRow = UVECTORS.Zero;
                //strip the origin data

                int j = aDescriptor.IndexOf(uopGlobals.Delim, 0, StringComparison.OrdinalIgnoreCase);
                List<string> aVals = mzUtils.StringsFromDelimitedList(aDescriptor.Substring(0, j), ",", false, false, false, false, -1, true);
                //the rest are the hole descriptions
                aDescriptor = aDescriptor.Substring(j + 1);
                // decode the hole string
                _Spouts = UHOLES.FromDescriptor(aDescriptor);
                _Spouts.Name = "SPOUTS";
                _Spout = _Spouts.Item(1);
                
                Pattern = aConstraints.PatternType;
                _VPitch = aConstraints.VerticalPitch;
                _HPitch = aConstraints.HorizontalPitch;
               
                USHAPE aBound = new USHAPE(this);
                _PointExtremes = UVECTORS.ComputeBounds(_Spouts.Centers, out double d1, out double d2);
                PatternLength = d2 + 2 * SpoutRadius;
                _Origin = (aVals.Count >= 2) ? new UVECTOR(mzUtils.VarToDouble(aVals[0]), mzUtils.VarToDouble(aVals[1])) : _Spouts.Item(1).Center.Clone();

                UVECTORS.GetOrdinateLists(_Spouts.Centers, out List<double> xVals, out List<double> yVals, iPrecis: 6, bUnique: true);
                URECTANGLE aRect = aBound.Limits;
                _Rows.Clear();
                if (yVals.Count > 0)
                {

                    for (int i = 0; i < yVals.Count; i++)
                    {
                        d1 = yVals[i];
                        uopLine row =_Rows.Add(aRect.Left, d1, aRect.Right, d1, i + 1);
                        rwPts = _Spouts.Centers.getByOrd(d1, false, 2);
                        row.Points =  new uopVectors(rwPts);
                        if (rwPts.Count > 0)
                        {
                            if (rwPts.Count > mxRow.Count) mxRow = rwPts;

                        }
                    }
                    if (yVals.Count > 1)
                    {
                        yVals.Sort(); // ascending order
                        _VPitch = Math.Abs(yVals[1] - yVals[0]);
                    }
                }

                //set the HPitch
                if (mxRow.Count > 1)
                {
                    List<double> rxVals = UVECTORS.GetOrdinateList(mxRow, false, iPrecis: 6, bUnique: true);

                    if (rxVals.Count >= 2)
                    {
                        rxVals.Sort(); // ascending
                        _HPitch = Math.Abs(rxVals[1] - rxVals[0]);
                    }
                }

                _Cols.Clear();
                //set the VPitch
                if (xVals.Count > 0)
                {
                    xVals.Sort(); // ascending order
                    u1.X = xVals[0];
                    for (int i = 1; i < xVals.Count; i++)
                    {
                        d1 = xVals[i];
                        d2 = d1 - u1.X;
                        u1.Y = 0;
                        if (d2 == 0)
                        { u1.Y = 1; }
                        else
                        {
                            if (PitchType == dxxPitchTypes.Rectangular && HPitch > 0)
                            {
                                if (Math.Round(Convert.ToInt32(d2 / HPitch) - (d2 / HPitch), 2) == 0) u1.Y = 1;
                            }

                            else
                            { u1.Y = 1; }
                        }
                        if (u1.Y == 1)
                        {
                            _Cols.Add(d1, aBound.Limits.Top, d1, aBound.Limits.Bottom, aCol: i);
                          
                            if (_HPitch <= 0) break;

                        }
                    }
                }

            }
            catch (Exception e) { ErrorString = e.Message; }
            finally
            {
                _Spouts.Centers = new UVECTORS(_Rows.Points(bSuppressVal: false));
                Invalid = false;
                //wrap it up
                GenerationComplete();

            }

            return _Spouts.Count;

        }

     
        /// <summary>
        /// generates the spouts based on the constraints and the spout group properties
        /// </summary>
        /// <param name="aSpoutGroup"></param>
        /// <param name="aConstraints"></param>
        /// <returns>returns the number of spouts generated</returns>
        public int Generate_ByConstraints(mdSpoutGroup aSpoutGroup = null, mdConstraint aConstraints = null)
        {

            ErrorString = string.Empty;

            try
            {
                GenerationMethod = GenerationMethods.ByConstraint;
                double pstep = 1.0 / 16.0;
                _Spouts = new UHOLES(false, "", SpoutRadius);
                if (aSpoutGroup != null)
                    SpoutGroup = aSpoutGroup;
                else
                    aSpoutGroup = SpoutGroup;

                if (aSpoutGroup == null) return 0;

                if (aConstraints != null)
                    Constraints = aConstraints;
                else
                {

                    aConstraints = Constraints;
                    if (aConstraints == null)
                    {
                        aConstraints = aSpoutGroup.Constraints(null);
                        Constraints = aConstraints;
                    }
                }
                mdTrayAssembly aAssy = aSpoutGroup.GetMDTrayAssembly(null);

                InitializeBound();


               // if ((CountLock && aConstraints.SpoutCount == 0) || TargetArea < 0.5 * Math.PI * Math.Pow(SpoutRadius, 2)) return 0;

                //get the order of steps to
                List<uppSpoutPatterns> aSteps = GetSteps();

                int maxtry = ApplyMargins || (LimitedTop && LimitedBottom) ? 1 : 2;

                for (int boundindex = 1; boundindex <= maxtry; boundindex++)
                {

                    //create the grid boundary
                    CreateSpoutBounds(boundindex == 2 );
                    if (Unbounded) continue;

                    //loop on patterns in logical order until the mimimum pattern yields the required hole count
                    for (int step = 0; step < aSteps.Count; step++)
                    {
                        Pattern = aSteps[step];
                        //if(Pattern == uppSpoutPatterns.S1)
                        //{
                        //    Console.Beep();
                        //}
                        Init(SpoutGroup, Constraints, aAssy, aPattern: aSteps[step], false); // DesignFamily.IsStandardDesignFamily() && aSteps[step] != uppSpoutPatterns.SStar && !VPitchLock);

                        //determine how many of these spouts we need to achieve the area
                        ComputeRequiredCount();
                        if (TargetCount <= 0)
                            break;
                        // set the pitchs
                        SetHPitch();
                        SetVPitch(ExpandToMargins ? TargetRows : 0);

                        // create the spouts using the unsuppressed grid points within the bounds
                        Generate();

                        if (!ExpandToMargins)
                        {

                            //try to step up to the ideal pitch
                            if (GoodSolution && !VPitchLock && pstep != 0 && VPitch < 1.25 && !(LimitedTop || LimitedBottom)) 
                            {
                                mdSpoutGrid subgrid = new mdSpoutGrid(this);
                                mdSpoutGrid keepgrid = null;
                                while (subgrid.VPitch < 1.25)
                                {
                                    subgrid.VPitch += pstep;
                                    subgrid.ReGenerate();

                                    if (subgrid.GoodSolution)
                                        keepgrid = subgrid;
                                    else
                                        break;
                                }
                                if (keepgrid != null)
                                {
                                    keepgrid.GenerationComplete();
                                      Copy(keepgrid);
                                }
                            }
                        }
                        else
                        {
                            int rcnt = RowCount();
                            //shrink the vpitch to achieve the target row count
                            if (boundindex ==1 && rcnt < TargetRows & TargetRows > 0 && !VPitchLock)
                            {
                                mdSpoutGrid subgrid = new mdSpoutGrid(this);
                                mdSpoutGrid keepgrid = null;
                                double minpitch = MinVPitch;
                                double pitch = VPitch;
                                while (pitch >= minpitch)
                                {
                                    if (pitch - pstep < minpitch) break;
                                    subgrid.VPitch -= pstep;
                                    pitch = subgrid.VPitch;
                                    subgrid.ReGenerate();

                                    if (subgrid.RowCount() >= TargetRows )
                                    {
                                        keepgrid = subgrid;
                                        break;

                                    }

                                }
                                if (keepgrid != null)
                                {
                                    keepgrid.GenerationComplete();
                                    Copy(keepgrid);
                                }
                            }
                            //shrink the vpitch to achieve the target margin
                            if (boundindex == 1 && !VPitchLock && MaxMargin.HasValue && MaxMargin < mdSpoutGrid.MinSafeMargin && !Pattern.TriangularPitch() )
                            {
                                mdSpoutGrid subgrid = new mdSpoutGrid(this);
                                mdSpoutGrid keepgrid = null;
                                double minpitch = MinVPitch;
                                double pitch = VPitch;
                                while (pitch >= minpitch)
                                {
                                    if (pitch - pstep < minpitch) break;
                                    subgrid.VPitch -= pstep;
                                    pitch = subgrid.VPitch;
                                    subgrid.ReGenerate();

                                    if (subgrid.MaxMargin.HasValue && subgrid.MaxMargin.Value <=1)
                                    {
                                        keepgrid = subgrid;
                                        break;
                                    }
                                }
                                if (keepgrid != null)
                                {
                                    keepgrid.GenerationComplete();
                                      Copy(keepgrid);
                                }
                                else { GoodSolution = false; }
                            }

                            //if (rcnt < TargetRows && VPitch > MinVPitch)
                            //{
                            //    mdSpoutGrid bGrid = new mdSpoutGrid(this);
                            //    while (rcnt < TargetRows && bGrid.VPitch > MinVPitch)
                            //    {
                            //        bGrid.VPitch -= pstep;
                            //        if (bGrid.VPitch < MinVPitch) bGrid.VPitch = MinVPitch;

                            //        bGrid.ReGenerate();
                            //        rcnt = bGrid.RowCount();
                            //    }
                            //    bGrid.GenerationComplete();
                            //    Copy(bGrid);
                            //}
                            //TargetRows = RowCount();

                            //// shrink the vpitch to achieve the spout count
                            //rcnt = PointCount;
                            //sCnt = TargetCount;
                            //if (rcnt < sCnt && VPitch > MinVPitch)
                            //{
                            //    mdSpoutGrid bGrid = new mdSpoutGrid(this);
                            //    while (rcnt < sCnt && bGrid.VPitch > MinVPitch)
                            //    {
                            //        bGrid.VPitch -= pstep;
                            //        if (bGrid.VPitch < MinVPitch) bGrid.VPitch = MinVPitch;

                            //        bGrid.ReGenerate();
                            //        rcnt = bGrid.PointCount;
                            //    }
                            //    bGrid.GenerationComplete();
                            //    Copy(bGrid);
                            //    TargetRows = RowCount();
                            //}

                            //rcnt = RowCount();
                            //TargetRows = rcnt;
                            //y1 = SpoutLimits.Bottom;

                            ////shink/grow the pitch to fit in the margins
                            //if (VPitch > MinVPitch)
                            //{
                            //    if (y1 < _Margins.Bottom - MarginBuffer || y1 > _Margins.Bottom + MarginBuffer)
                            //    {
                            //        sCnt = PointCount;
                            //        mdSpoutGrid bGrid = new mdSpoutGrid(this);
                            //        //shrink into margins
                            //        if (y1 < _Margins.Bottom - MarginBuffer)
                            //        {
                            //            while (y1 < _Margins.Bottom - MarginBuffer && bGrid.VPitch > MinVPitch)
                            //            {
                            //                mdSpoutGrid cGrid = new mdSpoutGrid(bGrid);
                            //                cGrid.VPitch -= pstep;
                            //                if (cGrid.VPitch < MinVPitch)
                            //                {
                            //                    cGrid.VPitch = MinVPitch;
                            //                }
                            //                cGrid.ReGenerate();
                            //                if ((cGrid.IsRectangular() == true && cGrid.RowCount() == rcnt) || (cGrid.IsRectangular() == false && cGrid.PointCount >= sCnt))
                            //                {
                            //                    bGrid = new mdSpoutGrid(cGrid);
                            //                    y1 = bGrid.SpoutLimits.Bottom;
                            //                    if (y1 >= _Margins.Bottom - bGrid.MarginBuffer || bGrid.VPitch - pstep < MinVPitch || bGrid.VPitch == MinVPitch)
                            //                    {
                            //                        break;
                            //                    }
                            //                }
                            //                else
                            //                {
                            //                    cGrid.SetVPitch( cGrid.RowCount());
                            //                    cGrid.ReGenerate();
                            //                    bGrid = cGrid;
                            //                    break;
                            //                }
                            //            }
                            //        }
                            //        bGrid.GenerationComplete();
                            //        Copy(bGrid);

                            //        y1 = SpoutLimits.Bottom;
                            //        //grow to margins
                            //        rcnt = RowCount();
                            //        if ((y1 > _Margins.Bottom + MarginBuffer) && rcnt > 1)
                            //        {

                            //            while (y1 > _Margins.Bottom + MarginBuffer)
                            //            {
                            //                mdSpoutGrid cGrid = new mdSpoutGrid(this);
                            //                cGrid.VPitch += pstep;
                            //                cGrid.TargetRows = cGrid.RowCount();
                            //                cGrid.ReGenerate();
                            //                if (cGrid.RowCount() != rcnt)
                            //                {
                            //                    //bail if the row count changes
                            //                    break;
                            //                }
                            //                else
                            //                {
                            //                    //continue
                            //                    y1 = cGrid.SpoutLimits.Bottom;
                            //                    if (y1 <= _Margins.Bottom + cGrid.MarginBuffer) break;
                            //                    bGrid = cGrid;
                            //                }
                            //            }
                            //        }
                            //        bGrid.GenerationComplete();
                            //        Copy(bGrid);
                            //    }
                            // }
                        }
                        // see what we have
                        if (step < aSteps.Count)
                        {
                            if (GoodSolution) break; // if (y1 > _Margins.Bottom - MarginBuffer)
                        }

                        //if (!DesignFamily.IsStandardDesignFamily())
                        //    break;
                    } //loop on patterns

                    if (GoodSolution) break;

                } // loop on min and max bonds

            }
            catch (Exception e) { ErrorString = e.Message;}
            finally
            {
                //wrap it up
                //GenerationComplete();
                
            }

            if (_Spouts.Count <= 0) 
                GenerationComplete();
            return _Spouts.Count;
        }

        private void InitializeBound()
        {
            UsesMaxBounds = false;
            _MinBounds = null;
            _MaxBounds = null;
            mdSpoutGroup group = SpoutGroup;
            if (group == null) return;
            mdConstraint constraint = Constraints;
            SpoutArea ??= new mdSpoutArea(group.SpoutArea);

            mdSpoutArea SA = SpoutArea;

            LastLength = 0;
            if (_Spout.Length <= 0) _Spout.Length = 2 * SpoutRadius;
            if (_Spout.Length != LastLength) LastLength = 0;
      
            _MinBounds = SpoutCenterBounds(false);
            if (!_MinBounds.HasValue) return;
            _MaxBounds = ApplyMargins || (LimitedTop && LimitedBottom)  ?  _MaxBounds = new USHAPE(_MinBounds.Value) : SpoutCenterBounds(true); ;
            //PartialRowLocation = IsRectangular() ? Direction == dxxOrthoDirections.Up ? "BOTTOM" : "TOP" : string.Empty;
            _SpoutCenter = new UVECTOR(SA.Center);
        }
        public override bool ValidateVector(iVector aVector, bool bIsMirrorPt, out string rErrorString, bool? bAlt = null, double? aHStepFactor = null, double? aVStepFactor = null, bool bIgnoreMaxes = false, bool? bRectangularBounds = null)
        {
            bRectangularBounds = IsRectangular();
            bool _rVal = base.ValidateVector(aVector, bIsMirrorPt, out rErrorString, bAlt, aHStepFactor, aVStepFactor, bIgnoreMaxes, bRectangularBounds);

            if(!_rVal && Pattern == uppSpoutPatterns.SStar)
            {
                //if(rErrorString == "EXTERIOR TO BOUNDARY")
                //{
                    if (aVector.Y <= Top+0.00001 && aVector.Y >= Bottom - 0.00001)
                        _rVal = true;
                //}
            }

        if (Pattern.UsesSlots() && !bRectangularBounds.Value && _rVal)
            {
                if (SlotLength <= 0) SizeSpout();
                double halfspout = SlotLength / 2 - SpoutRadius;
                uopVector u2 = new uopVector(Math.Round(aVector.X + halfspout,6), aVector.Y);
                if (!base.ValidateVector(u2,  false, out rErrorString, bAlt, aHStepFactor, aVStepFactor, bIgnoreMaxes))
                    _rVal = false;

                else
                {
                    u2 = new uopVector(Math.Round(aVector.X - halfspout,6), aVector.Y );
                    if (!base.ValidateVector(u2, false, out rErrorString, bAlt, aHStepFactor, aVStepFactor, bIgnoreMaxes))
                        _rVal = false;
                }
            }
           
            
            return _rVal;
        }
    

        internal void CreateSpoutBounds(bool bMaxed)
        {
            UsesMaxBounds = false;

            if (ApplyMargins || (LimitedTop && LimitedBottom)) bMaxed = false;
            UsesMaxBounds = bMaxed;
            if (Unbounded || !_MinBounds.HasValue) InitializeBound();
            if( Unbounded ) return;
            
            //copy the max or min bounds to the base grid to use as it's bounds
            USHAPE shape = !bMaxed ? _MinBounds.Value : _MaxBounds.Value;
            Copy(shape);
            

            LastLength = _Spout.Length;
            
        }
        public void SetBottomFraction()
        {


            BottomFraction = 1;
            SPR = 0;
            if (_Spouts.Centers.Count <= 0 || RowCount() <= 0) return;



            uopRectangle aSpoutLimits = new uopRectangle(_Spouts.BoundaryRectangle);
            double dcWd = BoxInsideWidth;
            double cX = TargetX;
            URECTANGLE aLims = new URECTANGLE(cX - dcWd / 2.0, 0, cX + dcWd / 2.0, 0);
            
            for (int i = 1; i <= _Rows.Count; i++)
            {
                uopLine row = _Rows.Item(i);
                uopVectors rowpts = row.GetPoints((x) => !x.Suppressed);
                double frac = 1;
                if (rowpts.Count > 0)
                {
                    if (rowpts.Count >  SPR) SPR = rowpts.Count;
                    List<double> aVals = rowpts.Ordinates(false);
                    if (aVals.Count > 0)
                        aVals.Sort();
                    else
                        continue;

                    double d1 = aVals.First() - 0.5 * _Spout.Length - aLims.Left;
                    double d3 = aLims.Right - (aVals.Last() + 0.5 * _Spout.Length);
                    double d2 = 0;

                    if (rowpts.Count > 1)
                    {
                        d2 = Math.Abs(aVals.Last() - aVals.First()) - _Spout.Length;
                        d2 *= rowpts.Count - 1;
                    }
                    frac = Math.Round((d1 + d2 + d3) / dcWd, 4);

                    if (frac < BottomFraction)
                        BottomFraction = frac;
                }

                if (Pattern == uppSpoutPatterns.Undefined)
                    Pattern = uppSpoutPatterns.D;

                if (_Spouts.Centers.Count <= 0)
                {
                    PatternLength = 0;
                    _SpoutCenter = BoundsV.Center;
                }
                else
                {
                    PatternLength = aSpoutLimits.Height;
                    _SpoutCenter = new UVECTOR( aSpoutLimits.Center);
                }
            }
        }

        private void MyGenerationStartHandler() 
        {
            _Spouts.Centers = UVECTORS.Zero;
          

        }
        private void MyGenerationCompleteHandler(uopVectors aGridPoints)
        {
            GenerationComplete(aGridPoints);


        }

        private void MyGridPointAddedHandler(uopVector aGridPoint, uopVectors aGridPoints, ref bool ioKeep, ref bool ioSuppress, ref bool rStopProcessing, uopLine aRowLine, uopLine aColumnLine, ref uopRectangle aRectangleToSave, bool bIsMirrorPoint)
        {

            //when the grid points are added, we size the spouts by seting the value on the new grid point 

            if(TargetCount <= 0)
            {
                ioSuppress = true;
                rStopProcessing = true;
                return;
            }

            if (SlotLength <= 0)
                SizeSpout();

            aGridPoint.Radius = _Spout.Radius;
            double f1 = 1;
            if (ioKeep && !ioSuppress)
            {
                if(Pattern != uppSpoutPatterns.SStar)
                {
                    aGridPoint.Value = SlotLength;
                }
                else
                {
                    f1 = aColumnLine.X() < DCInfo.X ? 1 : -1;
                    uopLine hline = new uopLine(aGridPoint, aGridPoint.Moved(2*Width * f1));
                    uopVector ip = hline.Intersections(Segments, aSegsAreInfinite: false, aLineIsInfinite: false).GetVector(f1==1 ? dxxPointFilters.AtMaxX : dxxPointFilters.AtMinX );
                    dxxRoundToLimits rndTo = Metric ? dxxRoundToLimits.Millimeter : dxxRoundToLimits.Sixteenth;
                    if (ip != null)
                    {
                        aGridPoint.Value = uopUtils.RoundTo(ip.DistanceTo(aGridPoint)  +2 * SpoutRadius, rndTo,bRoundDown:true) ;
                        aGridPoint.X += (0.5 * aGridPoint.Value - SpoutRadius) * f1;
                    }
                    else
                    {
                        aGridPoint.Value = SpoutRadius;
                    }

                }
               
                    _Spouts.Centers.Add(aGridPoint);
                    TotalArea += mdSpoutGrid.SingleSpoutArea(_Spouts.Item(_Spouts.Count));
                if (Pattern == uppSpoutPatterns.SStar)
                {
                    if ((TotalArea >= TargetArea && !CountLock) || (CountLock && aGridPoints.Count + 1 >= Constraints.SpoutCount))
                        rStopProcessing = true;
                    
                }
                        //if (Pattern == uppSpoutPatterns.SStar)
                        //{
                        //    if (TotalArea >= TargetArea) 
                        //    {
                        //        rStopProcessing = true;

                        //        //shrink to best size for spouts

                        //        double err = uopUtils.TabulateAreaDeviation(TargetArea, TotalArea, out _);
                        //        int i1 = _Spouts.Count;
                        //        int i2 = i1-1;
                        //        while ( err > 2.5)
                        //        {
                        //            if (i2 < 1) break;

                        //            UHOLE spt1 = _Spouts.Item(i1);
                        //            UHOLE spt2 = _Spouts.Item(i2);

                        //            uopVector u1 = aGridPoints.Find((x) => x.Row == spt1.Center.Row);
                        //            uopVector u2 = aGridPoints.Find((x) => x.Row == spt2.Center.Row);

                        //            double dif = spt1.Length - spt2.Length;
                        //            if (dif >0)
                        //            {

                        //                spt1.Length = spt2.Length;
                        //                spt1.X = spt2.X;
                        //                //u1.X = spt1.X;

                        //            }
                        //            //u1.X = spt1.X;

                        //            _Spouts.SetItem(i1, spt1);
                        //            _Spouts.SetItem(i2, spt2);


                        //            i1--;
                        //            i2--;
                        //            TotalArea = _Spouts.Area();
                        //            err = uopUtils.TabulateAreaDeviation(TargetArea, TotalArea, out _);
                        //            break;
                        //        }

                        //    }

                        //}
                    }

        }

        private void GenerationComplete(uopVectors aGridPoints = null)
        {

            //tabulate the margins
            //attempt to center the spouts on the panel below and center the partial rows

            if (GenerationMethod == GenerationMethods.ByConstraint)
            {
                

                //set the good soultion flag
                if (Pattern != uppSpoutPatterns.SStar)
                {
                    TabulateMargins();
                    GoodSolution = PointCount >= TargetCount;
                    if (GoodSolution && !UsesMaxBounds  && !ExpandToMargins)
                    {
                        double? maxmarg = MaxMargin;
                        if (maxmarg.HasValue && maxmarg.Value < mdSpoutGrid.MinSafeMargin)
                            GoodSolution = false;
                    }

                }
                else
                {
                    SizeSpoutsSStar();
                    GoodSolution = true;

                }

                if (!_ReGenerating)
                {
                    //Center any partial rows and center the souts on the target Y
                    CenterSpouts();
                }
            }
             
            //compute the margins
            TabulateMargins();

            //set the error
            RecalculateError();

            //compute the bottom fraction
           SetBottomFraction();

            if(GenerationMethod == GenerationMethods.ByString && SpoutCount >0 && ! Unbounded)
            {
                UsesMaxBounds = _Spouts.BoundaryRectangle.Height > _MinBounds.Value.Height;
            }

            Invalid = false;
            _Generating = false;

            if (Pattern == uppSpoutPatterns.SStar)
            {
                _Spout.Length = _Spouts.Centers.Count > 0 ? _Spouts.Centers.First.Value : 2 * SpoutRadius;
                _Spouts.Member = _Spout;
            }

            _Spout.Depth = SpoutGroup != null ? SpoutGroup.Thickness : 0;
            _Spouts.Member.Depth = _Spout.Depth;
            if(GenerationMethod == GenerationMethods.ByConstraint)
            {
                base.EventGenerationComplete -= MyGenerationCompleteHandler;
                base.EventGridPointAdded -= MyGridPointAddedHandler;
                base.EventGenerationStart -= MyGenerationStartHandler;
            }

           //GenerationMethod = GenerationMethods.Undefined;
        }



        #endregion Methods

        #region Shared Methods

        public static double MinSafeMargin = 1;

        /// <summary>
        /// creates the groups spouts spout grid based on the current properties and constraints and the data read from the passed project file data
        /// </summary>
        /// <param name="aSpoutGroup">the target spout group</param>
        /// <param name="aConstraints">the constraints to update/apply</param>
        /// <param name="aAssy">the parent assembly</param>
        /// <param name="rErrorString">returns any detected errors</param>
        /// <param name="aFileData">a property array containind the file data</param>
        /// <param name="aFileSection">the file section to read the spout group info from</param>
        /// <param name="rDescriptor">returns the descriptor read from the file</param>
        /// <param name="bRegenSpouts">aflag to read the pattern info but to regenerate the spouts using the current generation rule</param>
        public static mdSpoutGrid GenerateSpouts(mdSpoutGroup aSpoutGroup, mdConstraint aConstraints, mdTrayAssembly aAssy, out string rErrorString, uopPropertyArray aFileData, string aFileSection, out string rDescriptor, bool bRegenSpouts = false)
        {
            rDescriptor = string.Empty;
            mdSpoutGrid _rVal = null;
            try

            {
                rErrorString = string.Empty;
                if (aSpoutGroup == null) return null;

                if (!aSpoutGroup.PerimeterV.IsDefined) return null;
                aAssy ??= aSpoutGroup.GetMDTrayAssembly();
                if (aAssy == null) return null;
                aConstraints ??= aSpoutGroup.Constraints(aAssy);
                if (aConstraints == null) return _rVal;

                bool bKeep = false;
                bool bRegen = true;
                double sErr = 0;

                //if (aSpoutGroup.Handle == "1,1" && aAssy.RingRange.SpanName == "30-48")
                //{
                //    Console.Beep();
                //}

                if (!aConstraints.TreatAsIdeal) aConstraints.PropValSet("OverrideSpoutArea", 0, bSuppressEvnts: true);

                if (!string.IsNullOrEmpty(aFileSection))
                {
                    bKeep = true;
                    rDescriptor = aFileData.ValueS(aFileSection, $"SpoutGroup{aSpoutGroup.Index}", "");
                    mdConstraint bConstraints = aConstraints.Clone();
                    if (!string.IsNullOrWhiteSpace( rDescriptor))
                    {
                        rDescriptor = mzUtils.FixGlobalDelimError(rDescriptor);
                        List<string> aVals = mzUtils.StringsFromDelimitedList(rDescriptor, uopGlobals.Delim, false, true, false, false, -1, true);

                        if (aVals.Count > 0)
                        {

                            bConstraints.PropValSet("PatternType",  uopEnums.SpoutPatternFromString(aVals[1],out string err), bSuppressEvnts: true);
                            bConstraints.PropValSet("VerticalPitch", mzUtils.VarToDouble(aVals[4]), bSuppressEvnts: true);
                            bConstraints.PropValSet("HorizontalPitch", mzUtils.VarToDouble(aVals[5]), bSuppressEvnts: true);
                       
                        }
                    }
                    rDescriptor = aFileData.ValueS(aFileSection, $"SpoutGroup{aSpoutGroup.Index}.Spouts", "");
                    if (string.IsNullOrWhiteSpace(rDescriptor)) rDescriptor = string.Empty;
                    rDescriptor = mzUtils.FixGlobalDelimError(rDescriptor);
                    if (!bRegenSpouts)
                    {
                        _rVal = GenSpouts(aAssy, aSpoutGroup, bConstraints, rDescriptor);
                        bRegen = !string.IsNullOrEmpty(_rVal.ErrorString);
                        if(_rVal.SpoutCount <=0 && bConstraints.SpoutCount >= 0)
                        {
                            bRegen = true;
                        }
                    }
                    else
                    {
                        bRegen = true;
                    }
                }
                if (bRegen)
                {
                    if (!aConstraints.TreatAsIdeal) aConstraints.PropValSet("OverrideSpoutArea", 0, bSuppressEvnts: true);

                    _rVal = GenSpouts(aAssy, aSpoutGroup, aConstraints);
                }
                rErrorString = _rVal.ErrorString;
                bKeep = string.IsNullOrWhiteSpace(rErrorString);
          
                //save the pattern stuff
                if (bKeep)
                {

                    _rVal.Invalid = false;
                    if (aConstraints.TreatAsIdeal)
                    {
                        aConstraints.PropValSet("OverrideSpoutArea", _rVal.TotalArea, bSuppressEvnts: true);
                        aSpoutGroup.SpoutArea.OverrideSpoutArea = _rVal.TotalArea;
                        _rVal.TargetArea = _rVal.TotalArea;
                    }
                    else
                    {
                        aConstraints.PropValSet("OverrideSpoutArea", 0, bSuppressEvnts: true);
                        aSpoutGroup.SpoutArea.OverrideSpoutArea = null;
                        _rVal.TargetArea = aSpoutGroup.TheoreticalArea;
                    }
                    sErr = (_rVal.TargetArea > 0) ? (_rVal.TotalArea / _rVal.TargetArea) - 1 : 0;

                   
                }

                aSpoutGroup.Invalid = true;
                aConstraints.Invalid = true;
                
            }
            catch (Exception ex)
            {
                rErrorString = ex.Message;
              
            }
            if (!string.IsNullOrWhiteSpace(rErrorString) && _rVal != null && string.IsNullOrWhiteSpace(_rVal.ErrorString))  _rVal.ErrorString = rErrorString;
            return _rVal;

        }


        /// <summary>
        /// used by spout groups to create their spout pattern and spouts collections
        /// </summary>
        /// <param name="aAssy">1the parent tray assembly</param>
        /// <param name="aSpoutGroup">the subject spout group</param>
        /// <param name="aConstraints">a constraint object that carries the variable constraints for the spout group</param>
        /// <param name="aDescriptor"></param>
        /// <returns></returns>
        public static mdSpoutGrid GenSpouts(mdTrayAssembly aAssy, mdSpoutGroup aSpoutGroup, mdConstraint aConstraints, string aDescriptor = "")
        {
            mdSpoutGrid _rVal = null;

            try
            {
                if (aSpoutGroup == null) return null;

                aAssy ??= aSpoutGroup.GetMDTrayAssembly();
                if (aAssy == null) return null;

                aConstraints ??=  aSpoutGroup.Constraints(aAssy);
                aConstraints ??=  new mdConstraint();
              
                _rVal = new mdSpoutGrid(aSpoutGroup, aConstraints, aAssy);

                if (!string.IsNullOrWhiteSpace(aDescriptor))
                {
                    _rVal.Generate_ByString(aDescriptor.Trim());
                }
                else
                {

                    // determine which case we have
                    // run the basic stuff with the natural bounds
                    _rVal.Generate_ByConstraints( );
                }
              
                
            }
            catch (Exception ex)
            {
                _rVal.ErrorString = ex.Message;
            }
            finally
            {
               if(_rVal !=null)  _rVal.Invalid = !string.IsNullOrWhiteSpace(_rVal.ErrorString);
                if(aSpoutGroup!= null) aSpoutGroup.StartupLines = null;
            }
            return _rVal;
        }

        /// <summary>
        /// sets the number of spouts that will be present in a complete row of spouts based
        /// on the pattern type and the width of the downcomer
        /// </summary>
        /// <param name="aGrid"></param>
        /// <returns></returns>
        public static int SpoutsPerRow(mdSpoutGrid aGrid)
        {

            if (aGrid == null) return 0;
            int _rVal = 0;
            if (!aGrid.Pattern.UsesSlots(true))
            {
                if (aGrid.HPitch > 0) 
                    _rVal = (int)Math.Truncate(aGrid.MaxWidth / aGrid.HPitch) ;

                if (_rVal >1 && (_rVal - 1) * _rVal <= aGrid.MaxWidth) _rVal++;
                //if (aGrid.MaxWidth - (_rVal * aGrid.HPitch) < aGrid.HPitch) _rVal++;
                //if (aGrid.MaxWidth - (_rVal * aGrid.HPitch) > aGrid.HPitch) _rVal--;

            }
            else
            {
                _rVal = aGrid.Pattern switch
                {
                    uppSpoutPatterns.S1 => 1,
                    uppSpoutPatterns.S2 => 2,
                    uppSpoutPatterns.S3 => 3,
                    _ => 1
                };

            }
            return (_rVal <= 0) ? 1 : _rVal;
        }

        /// <summary>
        /// sets the progression of pattern types that will be tried depending on the situation
        /// </summary>
        /// <param name="aGrid"></param>
        /// <param name="aConstraints"></param>
        /// <returns></returns>
        private  List<uppSpoutPatterns> GetSteps()
        {
            List<uppSpoutPatterns> _rVal = new List<uppSpoutPatterns>();
            mdConstraint aConstraints = Constraints;

            if (PatternLock)
            {
                _rVal.Add(aConstraints.PatternType);
            }
            else
            {
                _rVal.Add(uppSpoutPatterns.D);
                _rVal.Add(uppSpoutPatterns.C);
                _rVal.Add(uppSpoutPatterns.B);
                _rVal.Add(uppSpoutPatterns.A);
                _rVal.Add(uppSpoutPatterns.Astar);
                _rVal.Add(uppSpoutPatterns.S3);
                _rVal.Add(uppSpoutPatterns.S2);
                _rVal.Add(uppSpoutPatterns.S1);
                if(!IsRectangular())  _rVal.Add(uppSpoutPatterns.SStar);
            }
            return _rVal;
        }


        internal static ULINE ComputeLimitLine(mdSpoutGrid aGrid, mdSpoutGroup aSpoutGroup, mdConstraint aConstraints)
        {

            if (aGrid == null) return ULINE.Null;
            aSpoutGroup ??= aGrid.SpoutGroup;

            mdSpoutArea SA = aGrid.SpoutArea;
            mdDowncomerBox aBox = aSpoutGroup.DowncomerBox;
            if (aBox == null)
                return ULINE.Null;

            ULINE _rVal = aBox.SpoutGroupLimLine(aSpoutGroup, aConstraints, true);

            if (aGrid.IsSlots && aGrid.TriangleEndPlate)
            {
                UVECTOR u1 = _rVal.sp.Clone();
                u1.X -= aGrid._Spout.Length / 2 - aGrid.SpoutRadius;
                _rVal.MoveOrtho(u1.DistanceTo(_rVal));
            }
            return _rVal;
        }


        /// <summary>
        /// calculates the differences between the two passed areas and returns true if the percent difference is less than the passed limit
        /// </summary>
        /// <param name="aTargetArea">the target area</param>
        /// <param name="aCurrentArea">the actual area</param>
        /// <param name="rRatio">returns the ratio of the current area / the target </param>
        /// <param name="rErrPct">returns the percent difference betwee the two areas</param>
        /// <param name="aPercentLimit">the limit to apply which is applied to determine if the error percentage is less than the limits</param>
        /// <returns></returns>
        public static bool TabulateError(double aTargetArea, double aCurrentArea, out double rRatio, out double rErrPct, double aPercentLimit = 2.5)
        {
            rErrPct = uopUtils.TabulateAreaDeviation(aTargetArea,aCurrentArea, out rRatio);
            return Math.Abs(rErrPct) <= aPercentLimit;
        }

        /// <summary>
        /// the distance between spouts dictated by the pattern type
        /// read only
        /// </summary>
        /// <param name="aGrid"></param>
        /// <param name="aSpoutLength"></param>
        /// <returns></returns>
        public static double GetHorizontalPitch(uppSpoutPatterns aPatternType, double aSpoutLength)
        {
        
            switch (aPatternType)
            {
                case uppSpoutPatterns.A:
                case uppSpoutPatterns.Astar:
                case uppSpoutPatterns.B:
                    return 0.9375;

                case uppSpoutPatterns.C:
                    return 1.125;

                case uppSpoutPatterns.D:
                    return 1.75;

                case uppSpoutPatterns.S1:
                    return 0;

                case uppSpoutPatterns.S2:
                case uppSpoutPatterns.S3:
                    return 0.25 + aSpoutLength;

                default:
                    return 0.9375;

            }

        }


        public static double MaxSpoutLength(DowncomerInfo aDCInfo, uppSpoutPatterns aPatternType, double aOverrideClearance, bool aRoundDown, bool aMetric)
        {
            if (aDCInfo == null) return 0;

            double clrc = Math.Abs(aOverrideClearance);
            if (clrc == 0) clrc = aDCInfo.SpoutGroupClearance;
            double _rVal = aDCInfo.InsideWidth - (2 * clrc);


            if (aPatternType == uppSpoutPatterns.S2)
            { _rVal = (aDCInfo.InsideWidth - (2 * clrc) - 0.25) / 2; }
            else if (aPatternType == uppSpoutPatterns.S3)
            { _rVal = (aDCInfo.InsideWidth - (2 * clrc) - (2 * 0.25)) / 3; }
              return uopUtils.RoundTo(_rVal, aMetric ? dxxRoundToLimits.Millimeter : dxxRoundToLimits.Sixteenth, bRoundDown:aRoundDown); 
            
        }

        internal static double SingleSpoutArea( UHOLE aSpout,  double aLength = 0)
        {

            if (aLength == 0) aLength = aSpout.Length;
            return (aLength > (2 * aSpout.Radius)) ? Math.PI * Math.Pow(aSpout.Radius, 2) + (aLength - (2 * aSpout.Radius)) * (2 * aSpout.Radius) : Math.PI * Math.Pow(aSpout.Radius, 2); ;

        }
        #endregion Shared Methods

    }
}
