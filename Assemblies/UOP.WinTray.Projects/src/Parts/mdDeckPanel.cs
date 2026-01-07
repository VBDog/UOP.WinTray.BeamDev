using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UOP.DXFGraphics;
using UOP.DXFGraphics.Utilities;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Materials;

using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;

namespace UOP.WinTray.Projects.Parts
{
    /// <summary>
    /// represents a deck panel in a md tray assembly
    /// the deck panel is completely defined by the downcomers that bound it on the left and right
    /// </summary>
    public class mdDeckPanel : uopPart
    {
        public override uppPartTypes BasePartType => uppPartTypes.DeckPanel;
        
        #region Constructors

        public mdDeckPanel() : base(uppPartTypes.DeckPanel, uppProjectFamilies.uopFamMD, "", "", true) => Init();

                       
        public mdDeckPanel(mdTrayAssembly aAssy, uopPanelSectionShape aBaseShape) : base(uppPartTypes.DeckPanel, uppProjectFamilies.uopFamMD, "", "", true) => Init(aAssy, aBaseShape);
      
        



        public mdDeckPanel(mdDeckPanel aPanel) : base(uppPartTypes.DeckPanel, uppProjectFamilies.uopFamMD, "", "", true)
        {
            Init();
    
            if (aPanel != null)  Copy(aPanel);
       
        }

        private void Init(mdTrayAssembly aAssy = null, uopPanelSectionShape aBaseShape = null)
        {

            _BaseShape = aBaseShape;
     
            TotalRequiredSlotCount = 0;
     

            OccuranceFactor = 1;
            if (aAssy != null)
            {
                SubPart(aAssy);
                SheetMetalStructure = aAssy.Deck.SheetMetalStructure;

                PercentOpen = aAssy.Deck.Ap;
      
                
            }
        }
        #endregion Constructors


        #region Relationships

        public override bool IsVirtual { get => BaseShape == null ? true: BaseShape.IsVirtual ; set { } }

        private WeakReference<mdDeckPanel> _ParentRef;
        private WeakReference<mdDeckPanel> _ChildRef;
        public void AssociateToParent(mdDeckPanel aParent)
        {
            _ChildRef = null;
            if (aParent == null)
            {
                _ParentRef = null;
                IsVirtual = false;
                return;
            }

            _ParentRef = new WeakReference<mdDeckPanel>(aParent);
            aParent._ChildRef = new WeakReference<mdDeckPanel>(this);
            IsVirtual = true;
        }

        public void AssociateToChild(mdDeckPanel aChild)
        {
            _ParentRef = null;
            if (aChild == null)
            {
                _ChildRef = null;
                IsVirtual = false;
                return;
            }

            _ChildRef = new WeakReference<mdDeckPanel>(aChild);
            aChild._ParentRef = new WeakReference<mdDeckPanel>(this);
            IsVirtual = false;
        }

        public mdDeckPanel Parent
        {
            get

            {
                IsVirtual = false;
                if (_ParentRef == null) return null;
                IsVirtual = _ParentRef.TryGetTarget(out mdDeckPanel _rVal);
                if (!IsVirtual) _ParentRef = null;
                return _rVal;
            }

            set => AssociateToParent(value);

        }

        public mdDeckPanel Child
        {
            get
            {
                if (_ChildRef == null) return null;

                if (!_ChildRef.TryGetTarget(out mdDeckPanel _rVal)) _ChildRef = null;
                return _rVal;
            }
            set => AssociateToChild(value);
        }

        public void CopyParentProperties()
        {
            mdDeckPanel myparent = Parent;
            if (myparent == null) return;
            this.PropValsCopy(myparent.ActiveProperties, aSkipList: new List<string>() { "X" });
        }

        #endregion Relationships

        #region Properties

        

        private uopPanelSectionShape _BaseShape;
        public uopPanelSectionShape BaseShape { get => _BaseShape; set => _BaseShape = value; }

        public uopLinePair WeirLines  { get => BaseShape == null ? null : BaseShape.Weirs; set {} }

        public uppTrayConfigurations PanelType => uppTrayConfigurations.MultipleDowncomer;

        internal USHAPE Perimeter { get => new USHAPE(BaseShape); }

        public uopRectangle Bounds  => BaseShape == null ? new uopRectangle (): new uopRectangle( BaseShape.Bounds);  

        public override int OccuranceFactor { get => _FBP == null ? 1 : _FBP.OccuranceFactor; set {} }

        public override int PanelIndex { get => BaseShape == null ? 0 : BaseShape.PanelIndex ; set {  } }

        public override int Index { get => PanelIndex; set => PanelIndex = value; }

        public uopRectangle Limits => new uopRectangle(BaseShape == null ? null : BaseShape.Bounds);

        /// <summary>
        /// the info about the the downcomer that defines the section on the left
        /// </summary>
        public DowncomerInfo LeftDowncomerInfo => BaseShape?.LeftDowncomerInfo;

        /// <summary>
        /// the info about the the downcomer that defines the section on the right
        /// </summary>
        public DowncomerInfo RightDowncomerInfo => BaseShape?.RightDowncomerInfo;


        public override uopVector Center
        {
            get
            {
                uopVector _rVal = BaseShape == null ? uopVector.Zero : BaseShape.Center;
                _rVal.Tag = Handle;
                return _rVal;
            }
            set
            {
                if (BaseShape == null || value == null) return;
                BaseShape.Translate(value - BaseShape.Center);
            }
        }

        public string Handle => BaseShape == null ? "0,0" : BaseShape.Handle;

        /// <summary>
        /// returns True if the panel is the center panel
        /// </summary>
        public bool IsCenter { get => Math.Round(X,3) ==0; }

        /// <summary>
        /// returns True if the panels perimter has been defined
        /// </summary>
        public bool IsDefined => BaseShape != null;

        /// <summary>
        /// True if the panel is the outermost panel of the tray
        /// </summary>
        public bool IsHalfMoon { get => BaseShape == null ? false : BaseShape.IsHalfMoon; }

        public override double Weight
        {
            get
            {
                if(BaseShape == null) return 0;
                double aR = BaseShape.Area;
                aR -= PercentOpen / 100 * aR;
                return aR * Material.WeightMultiplier;
            }
        }

        public override uopInstances Instances
        {
            get
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
        /// the Z coordinate of the center of the part
        /// </summary>
        public override double Z => -Thickness;


        /// <summary>
        ///returns a string that includes the index of panel and includes the index opposing panel
        /// like "1 & 4" or "2 & 3" where ther are four deck panels in the assembly
        /// </summary>
        public string Label
        {
            get
            {
                string rVal = Index.ToString();
                int oidx = OpposingIndex;
                return (oidx > 0 && oidx != Index) ? rVal += $" & {oidx}" : rVal;

            }
        }

        public double LeftDowncomerClearance { get  { DowncomerInfo dcinfo = LeftDowncomerInfo; return dcinfo == null ? 0 : mdGlobals.DefaultPanelClearance; } }



        /// <summary>
        /// The index of the downcomer that bounds the panel on the left
        /// </summary>
        public int LeftDowncomerIndex { get { DowncomerInfo dcinfo = LeftDowncomerInfo; return dcinfo == null ? 0 : dcinfo.DCIndex; } }

        public double RightDowncomerClearance { get { DowncomerInfo dcinfo = RightDowncomerInfo; return dcinfo == null ? 0 : mdGlobals.DefaultPanelClearance; } }

        /// <summary>
        /// The index of the downcomer that bounds the panel on the right
        /// </summary>
        public int RightDowncomerIndex { get { DowncomerInfo dcinfo = RightDowncomerInfo; return dcinfo == null ? 0 : dcinfo.DCIndex; } }

        /// <summary>
        /// ^returns the long centerlines of the deck panel
        /// </summary>
        public dxeLine MidLine
        {
            get
            {

                double x1 = X;
                double rad = DeckRadius;
                double y1 = Math.Sqrt(Math.Abs(Math.Pow(rad, 2) - Math.Pow(x1, 2)));
                return new dxeLine(new dxfVector(x1, y1), new dxfVector(x1, -y1)) { Tag = Convert.ToString(Index) };
            }
        }


        /// <summary>
        /// the index of the panels left side opposing panel in the tray
        /// </summary>
        public int OpposingIndex
        {
            get
            {
                if (IsCenter) return 0;
                mdTrayAssembly aAssy = GetMDTrayAssembly();
                return (aAssy == null) ? 0 : uopUtils.OpposingIndex(Index, aAssy.PanelCount);
            }
        }

        /// <summary>
        /// the gross area of the panel
        /// the area of the simpleperimeter polygon
        /// </summary>
        public double PanelArea  => BaseShape == null? 0 : BaseShape.Area ;

        /// <summary>
        /// the radius of the arc section of the deck panel section
        /// </summary>
        public new double Radius => BaseShape == null ? 0 : BaseShape.DeckRadius;
        
        /// <summary>
        /// the radius of the ring that supports this panel
        /// </summary>
        public new double RingRadius => BaseShape == null ? 0 : BaseShape.RingRadius;


        /// <summary>
        /// the triming radius of the parent tray
        /// </summary>
        public double TrimRadius => BaseShape == null ? 0 : BaseShape.TrimRadius;


        /// <summary>
        /// the sections of the panel if there is any splicing
        /// </summary>
        public mdDeckSections Sections
        { get { mdTrayAssembly aAssy = GetMDTrayAssembly(); return (aAssy != null) ? aAssy.DeckSections.GetByPanelIndex(Index) : new mdDeckSections(); } }


        /// <summary>
        /// the list of splice centers for the panel
        /// </summary>
        public string SpliceLocations
        {
            get { mdTrayAssembly aAssy = GetMDTrayAssembly(); return (aAssy != null) ? aAssy.DeckSplices.OrdinateList(aPanelID: Index) : ""; }
        }

        /// <summary>
        ///returns the collection of splices defined on the deck panel
        /// </summary>
        public List<uopDeckSplice> Splices
        {
            get
            {
                mdTrayAssembly aAssy = GetMDTrayAssembly();
                return (aAssy != null) ? aAssy.DeckSplices.GetByPanelIndex(Index) : new List<uopDeckSplice>();
            }
        }


     
        /// <summary>
        /// the width of the deck section as viewed from above  (includes all splice details)
        /// </summary>
        public override double Width { get => BaseShape == null ? 0 : BaseShape.Width; set { } }
        /// <summary>
        /// the open area of the panel
        /// same as free bubbling area multiplied by the panels occurance factor
        /// </summary>
        public double TotalFreeBubblingArea => FreeBubblingArea() * OccuranceFactor;

        /// <summary>
        /// the number of slots requied on this panel tray wide
        /// </summary>
        public int TotalRequiredSlotCount { get; set; }



        #endregion Properties

        #region Methods    
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
        public void Copy(mdDeckPanel aPanel)
        {
            if (aPanel == null) return;
            base.Copy(aPanel);
            PercentOpen = aPanel.PercentOpen;
          
            _BaseShape = uopPanelSectionShape.CloneCopy(aPanel.BaseShape);
        }

        /// <summary>
        /// the maximum ordinate for the splice
        /// </summary>
        public double MinSpliceOrdinate { get => BaseShape == null ? double.MaxValue : BaseShape.MinSpliceOrdinate; }


        /// <summary>
        /// the maximum ordinate for a splice on the panel
        /// </summary>
        public double MaxSpliceOrdinate { get => BaseShape == null ? double.MaxValue : BaseShape.MaxSpliceOrdinate; }

        /// <summary>
        /// the collection of spout groups defined for the deck panel
        /// a deck panel retrieves its spout group from its parent tray assemblies collection of defined spout groups.
        /// see mdTrayAssembly.SpoutGroups
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public virtual colMDSpoutGroups SpoutGroups(mdTrayAssembly aAssy)
        {
            aAssy ??= GetMDTrayAssembly();
            if(aAssy == null) return new colMDSpoutGroups();
            colMDSpoutGroups _rVal = null;
            if (aAssy.DesignFamily.IsBeamDesignFamily() )
            {
                int oppindex = uopUtils.OpposingIndex(Index, aAssy.DowncomerData.PanelCount);
                if (!aAssy.DowncomerData.MultiPanel)
                {
                    _rVal = aAssy.SpoutGroups.GetByPanelIndex(oppindex, bNonZero: true, bParentsOnly: true);

                }
                else
                {
                    _rVal = aAssy.SpoutGroups.GetByPanelIndex(Index, bNonZero: true, bParentsOnly: true);
                    _rVal.AddRange(aAssy.SpoutGroups.GetByPanelIndex(oppindex, bNonZero: true, bParentsOnly: true));
                }

            }
            else
            {

                _rVal = aAssy.SpoutGroups.GetByPanelIndex(Index, bNonZero: true, bParentsOnly: true);
            }
            return _rVal;
        }
        public override string ToString() => $"MD DECK PANEL [{Index}] x {OccuranceFactor}";


        /// <summary>
        ///returns the ratio of the panels actual spout area to the total 
        /// actual spout area for the tray asembly
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public double ActualToActualRatio(mdTrayAssembly aAssy)
        {
            aAssy ??= GetMDTrayAssembly(aAssy);
            if (aAssy == null) return 0;
            return (aAssy.TotalSpoutArea > 0) ? ComputeSpoutArea(aAssy) / aAssy.TotalSpoutArea : 0;

        }


        /// <summary>
        /// ^returns the ratio of the panels actual spout area to the total actual spout area for the tray asembly
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public double VLError(mdTrayAssembly aAssy)
        {
            double rFBA2WLTo = 0;
            return VLError(aAssy, ref rFBA2WLTo);
        }
        /// <summary>
        /// ^returns the ratio of the panels actual spout area to the total actual spout area for the tray asembly
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="rFBA2WLTot"></param>
        /// <returns></returns>
        public double VLError(mdTrayAssembly aAssy, ref double rFBA2WLTot)
        {
            uopFreeBubblingPanel fbp = FreeBubblingPanel(aAssy);
            if (fbp == null) return 0;
            double WL = fbp.WeirLength;
            double fba = fbp.Area;
            if (rFBA2WLTot <= 0)
            {
                aAssy ??= GetMDTrayAssembly();
                rFBA2WLTot = aAssy == null ? 0 : aAssy.Downcomers.FBA2WLRatio;
            }
            return (rFBA2WLTot != 0 && WL != 0) ? ((fba / WL / rFBA2WLTot) - 1) * 100 : 0;

        }

        /// <summary>
        /// the actual spout area to ideal spout area ratio for the deck panel
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public double AreaRatio(mdTrayAssembly aAssy)
        {
            aAssy ??= GetMDTrayAssembly();
            if (aAssy == null) return 0;
            double ideal = IdealSpoutArea(aAssy);
            double actual = ComputeSpoutArea(aAssy,true);

            if (ideal != 0)
            { return actual / ideal; }
            else
            { return (ideal == 0 & actual == 0) ? 1 : 0; }

        }

        /// <summary>
        /// the absolute value of the difference of the area ratio (actual/theo) and 1
        /// represents the percentage over or under the required theoretical area
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="AbsVal">flag to return the ablsolute value</param>
        /// <returns></returns>
        public double AreaRatioDifferential(mdTrayAssembly aAssy) => AreaRatio(aAssy) - 1;



     

        /// <summary>
        /// the basic splice used to create the sections collection
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aOrdinate"></param>
        /// <param name="aSpliceFamily"></param>
        /// <returns></returns>
        public uopDeckSplice BasicDeckSplice(mdTrayAssembly aAssy, double? aOrdinate = null, uppSpliceStyles aSpliceFamily = uppSpliceStyles.Undefined)
        {
            aAssy ??= GetMDTrayAssembly();
            if (aAssy == null) return null;
            SubPart(aAssy);


            uppSpliceStyles sstyle = aSpliceFamily == uppSpliceStyles.Undefined ? aAssy.SpliceStyle : aSpliceFamily;
            uopDeckSplice _rVal = new uopDeckSplice()
            {
                PanelLimits = new URECTANGLE(Limits),
                 RangeGUID = aAssy.RangeGUID,
                ProjectHandle = aAssy.ProjectHandle,
                PanelIndex = Index,
                MaxOrdinate = MaxSpliceOrdinate,
                MinOrdinate = MinSpliceOrdinate,
                SpliceStyle = sstyle,
                JoggleSpliceLimit = JoggleSpliceLimit(sstyle),
                DeckRadius = aAssy.DeckRadius,
                ShellRadius = aAssy.ShellRadius,
                DeckThickness = aAssy.Deck.Thickness,
                LeftDowncomerClearance = LeftDowncomerClearance,
                RightDowncomerClearance = RightDowncomerClearance
            };


            if (IsHalfMoon)
            {

                _rVal.SpliceType = (sstyle == uppSpliceStyles.Tabs) ? uppSpliceTypes.SpliceWithTabs : uppSpliceTypes.SpliceWithJoggle;

                _rVal.Y = 0;
                if (aOrdinate.HasValue) _rVal.X = aOrdinate.Value;
                double x1 = (_rVal.SpliceType == uppSpliceTypes.SpliceWithJoggle) ? _rVal.X - 0.625 + Thickness : _rVal.X - 1.1975 + Thickness;
                _rVal.BaseFlangeLength = uopUtils.ComputeBeamLength(x1, aAssy.BoundingRadius, true);
                _rVal.FlangeLine = new uopFlangeLine(_rVal);
                _rVal.BaseFlangeLength = _rVal.FlangeLine.Length;
            }
            else
            {

                _rVal.X = X;
                _rVal.Y = aOrdinate ?? 0;
                _rVal.SpliceType = sstyle switch
                {
                    uppSpliceStyles.Angle => uppSpliceTypes.SpliceWithAngle,
                    uppSpliceStyles.Joggle => uppSpliceTypes.SpliceWithJoggle,
                    _ => uppSpliceTypes.SpliceWithTabs
                };


                if (Math.Abs(_rVal.Y) >= MaxSpliceOrdinate)
                    _rVal.Y = (_rVal.Y < 0) ? -MinSpliceOrdinate : MaxSpliceOrdinate;

                _rVal.BaseFlangeLength = aAssy.SpliceFlangeLength(out int _);
                if (X >= 0)
                    _rVal.FlangeLine = new uopFlangeLine(X - 0.5 * _rVal.BaseFlangeLength, _rVal.Y, X + 0.5 * _rVal.BaseFlangeLength, _rVal.Y,false);
                else
                    _rVal.FlangeLine = new uopFlangeLine(X + 0.5 * _rVal.BaseFlangeLength, _rVal.Y, X - 0.5 * _rVal.BaseFlangeLength, _rVal.Y, false);

                _rVal.BaseFlangeLength = _rVal.FlangeLine.Length;

                if (aOrdinate.HasValue)
                {
                    ULINE flngln = new ULINE( _rVal.FlangeLine.sp,_rVal.FlangeLine.ep) ;
                    double y1 = _rVal.Y;
                    //if(y1 >=0 && _rVal.SpliceType == uppSpliceTypes.SpliceWithTabs)

                    // Check intersection with the ring
                    double x1 = Math.Sqrt(Math.Pow(aAssy.BoundingRadius, 2) - Math.Pow(y1, 2));
                    if (X >= 0)
                    {
                        if (x1 < flngln.ep.X) flngln.ep.X = x1;
                    }
                    else
                    {
                        if (x1 < Math.Abs(flngln.ep.X)) flngln.ep.X = -x1;
                    }

                    // Check intersection with beam
                    var panelShape = mdDeckPanel.GetValidPanelShapeForDeckSpliceOrdinate(Index, y1, aAssy);

                    //// Improve the flange line
                    //// The initial value of the flange line at this stage could be very inaccurate.
                    //// In some cases, it is so long that not only intersects the closer side of the beam, but also intersects the farther side as well.
                    //// This can cause a lot of problems. So, here we find the portion of it that is inside the boundaries of the shape, first.
                    //uopLine horizontalLineConfinedInShape = GetHorizontalLineConfinedInShape(flngln.sp.Y, panelShape, flngln.sp.X <= flngln.ep.X);
                    //if (horizontalLineConfinedInShape != null)
                    //{
                    //    if (flngln.sp.X <= flngln.ep.X)
                    //    {
                    //        if (flngln.sp.X < horizontalLineConfinedInShape.sp.X)
                    //        {
                    //            flngln.sp.X = horizontalLineConfinedInShape.sp.X;
                    //        }

                    //        if (flngln.ep.X > horizontalLineConfinedInShape.ep.X)
                    //        {
                    //            flngln.ep.X = horizontalLineConfinedInShape.ep.X;
                    //        }
                    //    }
                    //    else
                    //    {
                    //        if (flngln.sp.X > horizontalLineConfinedInShape.sp.X)
                    //        {
                    //            flngln.sp.X = horizontalLineConfinedInShape.sp.X;
                    //        }

                    //        if (flngln.ep.X < horizontalLineConfinedInShape.ep.X)
                    //        {
                    //            flngln.ep.X = horizontalLineConfinedInShape.ep.X;
                    //        }
                    //    }
                    //}

                    //// Trim the flange line using the beam clearance
                    //uopLine uopFlangeLine = new uopLine(flngln.sp.X, flngln.sp.Y, flngln.ep.X, flngln.ep.Y);
                    //uopLine trimmedFlangeLine = GetHorizontalLineTrimmedByBeam(uopFlangeLine, panelShape, aAssy, aAssy.RingClearance);
                    //if (trimmedFlangeLine != null)
                    //{
                    //    flngln.sp.X = trimmedFlangeLine.sp.X;
                    //    flngln.ep.X = trimmedFlangeLine.ep.X;
                    //}

                    //_rVal.FlangeLine = new uopFlangeLine (flngln);
                }

                if (sstyle == uppSpliceStyles.Angle && IsShorterThanStandardAngleSplice(_rVal.FlangeLine.Length, aAssy))
                {
                    _rVal.SpliceType = uppSpliceTypes.SpliceWithJoggle;
                }

            }
            return _rVal;
        }
        private bool IsShorterThanStandardAngleSplice(double flangeLength, mdTrayAssembly assembly)
        {
            double standardFlangeLength = assembly.SpliceFlangeLength(out _);
            return Math.Round(flangeLength, 3) < Math.Round(standardFlangeLength, 3);
        }

        public static uopLine GetHorizontalLineTrimmedByBeam(uopLine originalLine, uopShape panelShape, mdTrayAssembly assembly, double beamOffset)
        {
            // Here is how it works: First, we find the first beam clearance line which has an intersection with the line and keep its intersection point.
            // This point is going to adjust either the start or end point of the original line. But to figure out which side, we need to perform more calculation.
            var beamClearanceLinePairs = uopLinePair.FromList(assembly.DowncomerData.CreateDividerLns(assembly.RingRadius, aOffset: beamOffset));
            var beamClearanceLines = beamClearanceLinePairs.SelectMany(lp => new[] { lp.Line1, lp.Line2 }).ToList();
            bool onSecondLine = false;
            uopVector beamClearanceIntersectionPoint = null;
            uopLine trimmedLine = null;

            foreach (var line in beamClearanceLines)
            {
                beamClearanceIntersectionPoint = line.IntersectionPt(originalLine, out _, out _, out _, out onSecondLine, out _);

                if (onSecondLine)
                {
                    break;
                }
            }

            if (onSecondLine)
            {
                if (panelShape != null)
                {
                    // To find the side that needs to be changed, we create an imaginary line from that point to the right and make sure it is long enough to intersect the panel shape on the right (by adding a small amount to the X of the rightmost vertex of the shape).
                    // If the line segment of the shape that intersects this line is not vertical, it means that the beam is on the right.

                    trimmedLine = new uopLine();
                    trimmedLine.sp.Y = originalLine.sp.Y;
                    trimmedLine.ep.Y = originalLine.ep.Y;
                    trimmedLine.sp.X = originalLine.sp.X;
                    trimmedLine.ep.X = originalLine.ep.X;

                    var shapeMaxX = panelShape.Vertices.Max(v => v.X);
                    var horizontalLineToRight = new uopLine(beamClearanceIntersectionPoint.X, beamClearanceIntersectionPoint.Y, shapeMaxX + 0.1, beamClearanceIntersectionPoint.Y);

                    foreach (var segment in panelShape.Segments)
                    {
                        if (!segment.IsArc)
                        {
                            var intersection = segment.Line.IntersectionPt(horizontalLineToRight, out _, out _, out bool onFirstLine, out onSecondLine, out _);
                            if (onFirstLine && onSecondLine)
                            {
                                if (segment.Line.IsVertical())
                                {
                                    // Beam is on the left side
                                    if (trimmedLine.sp.X < trimmedLine.ep.X)
                                    {
                                        trimmedLine.sp.X = beamClearanceIntersectionPoint.X;
                                    }
                                    else
                                    {
                                        trimmedLine.ep.X = beamClearanceIntersectionPoint.X;
                                    }
                                }
                                else
                                {
                                    // Beam is on the right side
                                    if (trimmedLine.sp.X < trimmedLine.ep.X)
                                    {
                                        trimmedLine.ep.X = beamClearanceIntersectionPoint.X;
                                    }
                                    else
                                    {
                                        trimmedLine.sp.X = beamClearanceIntersectionPoint.X;
                                    }
                                }

                                break;
                            }
                        }
                    }
                }
            }

            return trimmedLine;
        }

        public static uopLine GetHorizontalLineConfinedInShape(double ordinate, uopShape panelShape, bool sortLeftTRight)
        {
            if (panelShape == null) return null;
            var xLimits = GetPanelShapeMinMaxX(panelShape);

            uopLine horizontalLine = new uopLine(xLimits.minX - 1, ordinate, xLimits.maxX + 1, ordinate);

            var intersectionPoints = horizontalLine.Intersections(panelShape.Segments);

            if (intersectionPoints == null || intersectionPoints.Count == 0)
            {
                return null;
            }

            if (intersectionPoints.Count >= 2)
            {
                if (sortLeftTRight)
                {
                    intersectionPoints.Sort(dxxSortOrders.LeftToRight);
                }
                else
                {
                    intersectionPoints.Sort(dxxSortOrders.RightToLeft);
                }

                return new uopLine(intersectionPoints.Item(1), intersectionPoints.Item(2));
            }
            else
            {
                return new uopLine(intersectionPoints.Item(1), intersectionPoints.Item(1));
            }
        }




        /// <summary>
        ///returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>
        public mdDeckPanel Clone() => new mdDeckPanel( this);


        public override uopPart Clone(bool aFlag = false) => new mdDeckPanel(this);


      

        public override uopProperties CurrentProperties() { UpdatePartProperties(); return base.CurrentProperties(); }

        public override uopProperty CurrentProperty(string aPropertyName, bool bSupressNotFoundError = false)
        { UpdatePartProperties(); return base.CurrentProperty(aPropertyName, bSupressNotFoundError); }


        public colDXFEntities EdgeLines(bool aIncludeClearance = false, bool aRotated = false, dynamic aTopLim = null,
             dynamic aBotLim = null, string aLayer = "", dxxColors aColor = dxxColors.Undefined, string aLinetype = "")
        {
            colDXFEntities edgeLines =   new colDXFEntities();
            double lX =  Left(aIncludeClearance);
            double rx =  Right(aIncludeClearance);

            double rad = Radius;
            double lyt = 0;
            double lyb = 0;
            double ryt = 0;
            double ryb = 0;

            
            if (rad <= 0)
            {
                return edgeLines;

            }
            lyt = Math.Sqrt(Math.Pow(rad, 2) - Math.Pow(lX, 2));
            ryt = Math.Sqrt(Math.Pow(rad, 2) - Math.Pow(rx, 2));
            lyb = -lyt;
            ryb = -ryt;
            if (aTopLim != null)
            {
                if (mzUtils.IsNumeric(aTopLim))
                {
                    lyt = Math.Abs(Convert.ToSingle(aTopLim));
                    ryt = lyt;
                }
            }
            if (aBotLim != null)
            {
                if (mzUtils.IsNumeric(aBotLim))
                {
                    lyb = Math.Abs(Convert.ToSingle(aBotLim));
                    ryb = lyb;
                }
            }
            dxfDisplaySettings dsp = new dxfDisplaySettings(aLayer, aColor, aLinetype);
            if (!aRotated)
            {
                double lx = lX;
                double lybref = lyb;
                double lytref = lyt;
                EdgeLines().AddLine(lx, lybref, lx, lytref, dsp);

                double rx1 = rx;
                double rybref = lyb;
                double rytref = lyt;
                EdgeLines().AddLine(rx1, rybref, rx1, rytref, dsp);
            }
            else
            {
                double lx = lX;
                double lybref = lyb;
                double lytref = lyt;
                EdgeLines().AddLine(lybref, lx, lytref, lx, dsp);

                double rx1 = rx;
                double rybref = lyb;
                double rytref = ryt;
                EdgeLines().AddLine(ryb, rx1, rytref, rx1, dsp);
            }
            return edgeLines;
        }

        public static int ExtractPanelIndexFromPanelShapeName(string panelShapeName)
        {
            int panelIndex = 0;

            // We expect the name to be of this format: PANEL_<panel index>_<panel shape index>
            string[] components = panelShapeName.Split(new char[] { '_' });

            int.TryParse(components[1], out panelIndex);

            return panelIndex;
        }

        public static int ExtractPanelShapeIndexFromPanelShapeName(string panelShapeName)
        {
            int panelShapeIndex = 0;

            // We expect the name to be of this format: PANEL_<panel index>_<panel shape index>
            string[] components = panelShapeName.Split(new char[] { '_' });

            int.TryParse(components[2], out panelShapeIndex);

            return panelShapeIndex;
        }

        public static (double minX, double maxX) GetPanelShapeMinMaxX(uopShape panelShape)
        {
            var vertices = panelShape.Vertices;

            double maxX = vertices.GetOrdinate(dxxOrdinateTypes.MaxX);
            double minX = vertices.GetOrdinate(dxxOrdinateTypes.MinX);

            return (minX, maxX);
        }

        public static (double minY, double maxY) GetPanelShapeMinMaxY(uopShape panelShape)
        {
            var vertices = panelShape.Vertices;

            double maxY = vertices.GetOrdinate(dxxOrdinateTypes.MaxY);
            double minY = vertices.GetOrdinate(dxxOrdinateTypes.MinY);

            return (minY, maxY);
        }

        public static bool IsValidPanelShapeForDeckSpliceOrdinate(uopShape panelShape, double deckSpliceOrdinate, double? minDistanceFromTopAndBottom = null)
        {
            double minY, maxY;

            (minY, maxY) = GetPanelShapeMinMaxY(panelShape);

            if (deckSpliceOrdinate >= maxY || deckSpliceOrdinate <= minY)
            {
                return false;
            }
            else
            {
                if (minDistanceFromTopAndBottom.HasValue)
                {
                    // Returns false if the provided ordinate does not have enough distance from the top and the bottom of the panel shape
                    return !(deckSpliceOrdinate >= maxY - minDistanceFromTopAndBottom || deckSpliceOrdinate <= minY + minDistanceFromTopAndBottom);
                }
                else
                {
                    return true;
                }
            }
        }

        public static bool IsValidPanelShapeForVerticalDeckSpliceOrdinate(uopShape panelShape, double deckSpliceOrdinate, double? minDistanceFromLeftAndRight = null)
        {
            double minX, maxX;

            (minX, maxX) = GetPanelShapeMinMaxX(panelShape);

            if (deckSpliceOrdinate >= maxX || deckSpliceOrdinate <= minX)
            {
                return false;
            }
            else
            {
                if (minDistanceFromLeftAndRight.HasValue)
                {
                    // Returns false if the provided ordinate does not have enough distance from the left and the right of the panel shape
                    return !(deckSpliceOrdinate >= maxX - minDistanceFromLeftAndRight || deckSpliceOrdinate <= minX + minDistanceFromLeftAndRight);
                }
                else
                {
                    return true;
                }
            }
        }
        public static uopPanelSectionShapes GetPanelShapes(mdDeckPanel aPanel,  mdTrayAssembly aAssy = null)
        {
            if(aPanel == null)
                return null;
            if(aPanel.SubShapes != null) return new uopPanelSectionShapes(aPanel.SubShapes);
            aAssy ??= aPanel.GetMDTrayAssembly();
            if(aAssy == null) return null;

            aPanel.SetFBP(null, aAssy.DowncomerData.PanelShapes(bIncludeClearance: false,aPanel.Index ), false);
            return new uopPanelSectionShapes(aPanel.SubShapes);
        }

  

        public static uopShape GetPanelShapeUsingPanelAndShapeIndices(mdTrayAssembly assembly, int panelIndex, int panelShapeIndex, bool includeVirtualShapes = false)
        {
            if (assembly == null) return null;
            if (panelIndex < 1 || panelIndex > assembly.DeckPanels.Count) return null;

            var thisPanelShapes = mdDeckPanel.GetPanelShapes(assembly.DeckPanels.Item(panelIndex), assembly);
            if (thisPanelShapes == null) return null;

            //return thisPanelShapes.FirstOrDefault(s => ExtractPanelShapeIndexFromPanelShapeName(s.Name) == panelShapeIndex);
            return null;
        }

        public static uopShape GetValidPanelShapeForDeckSpliceOrdinate(int panelIndex, double deckSpliceOrdinate, mdTrayAssembly assembly)
        {
            if (assembly == null) return null;
            if(panelIndex <1 || panelIndex > assembly.DeckPanels.Count) return null;
            var thisPanelShapes = mdDeckPanel.GetPanelShapes(assembly.DeckPanels.Item(panelIndex), assembly);
            if(thisPanelShapes == null) return null;
            var validShapeForOrdinate = thisPanelShapes.FirstOrDefault(s => IsValidPanelShapeForDeckSpliceOrdinate(s, deckSpliceOrdinate));

            return validShapeForOrdinate;
        }

        public static double MinDistanceFromTopOrBottom => 20;

        /// <summary>
        /// the percentage that the current spout area deviates fron the ideal
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public double ErrorPercentage(mdTrayAssembly aAssy) => AreaRatioDifferential(aAssy) * 100;

        /// <summary>
        /// the zones of the panels FBA that are supplied from above by a single spout group
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public List<mdFeedZone> FeedZones(mdTrayAssembly aAssy) => mdPanelGenerator.CreatePanelFeedZones(this, aAssy);


     
        /// <summary>
        /// the ideal spout area for the deck panel
        /// equal to the weir length of the panel divided by the total weir length of the tray times the spout area of the tray
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aTotalWeirLength"></param>
        /// <param name="aTrayIdealSpoutArea"></param>
        /// <returns></returns>
        public double IdealSpoutArea(mdTrayAssembly aAssy, double aTotalWeirLength = 0, double aTrayIdealSpoutArea = 0)
        {
            if (aTrayIdealSpoutArea <= 0)
            {
                aAssy ??= GetMDTrayAssembly();
                if (aAssy == null) return 0;
                aTrayIdealSpoutArea = aAssy.TheoreticalSpoutArea;

            }

            if (aTrayIdealSpoutArea <= 0) return 0;
            double fract = WeirFraction(aAssy, aTotalWeirLength);

            return fract * aTrayIdealSpoutArea;
        }

        /// <summary>
        ///returns the ratio of the panels ideal spout area to the total ideal spout area for the tray asembly
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public double IdealToIdealRatio(mdTrayAssembly aAssy)
        { aAssy ??= GetMDTrayAssembly(aAssy); if (aAssy == null) return 0; return (aAssy.TheoreticalSpoutArea > 0) ? IdealSpoutArea(aAssy) / aAssy.TheoreticalSpoutArea : 0; }


        /// <summary>
        /// the maximum ordinate for a splice on the panel
        /// </summary>
        /// <param name="aSpliceStyle"></param>
        /// <returns></returns>
        public double JoggleSpliceLimit(uppSpliceStyles aSpliceStyle)
        {
            double _rVal = 2.6E+26d;
            if (IsHalfMoon) return _rVal;
            double rad = TrimRadius;

            if (rad <= 0) return _rVal;

            if (aSpliceStyle == uppSpliceStyles.Joggle) return MaxSpliceOrdinate;

            double x1 = (aSpliceStyle != uppSpliceStyles.Joggle) ? Right(false) - 1.125d : Right(false) - 1.125d;

            _rVal = Math.Sqrt(Math.Pow(rad, 2) - Math.Pow(x1, 2)) - 1.25d;

            return _rVal;
        }


        /// <summary>
        /// the downcomer that bounds the panel on the left
        /// </summary>
        /// <param name="DComers"></param>
        /// <returns></returns>
        public mdDowncomer LeftDowncomer(mdTrayAssembly aAssy = null) => GetMDDowncomer(aAssy, null, LeftDowncomerIndex);




        public double Right(bool bIncludeClearance = false)
        {
            if(BaseShape == null) return 0;
            uopRectangle limits = Limits;

            return (bIncludeClearance) ? limits.Right - RightDowncomerClearance : limits.Right;
        }

     

        public double Left(bool bIncludeClearance = false) 
          {
            if(BaseShape == null) return 0;
            uopRectangle limits = Limits;

            return (bIncludeClearance)? limits.Left - LeftDowncomerClearance : limits.Left;
        }


        public dxePolyline PerimeterMax(bool aIncludeClearance, bool aRotated)
        {
            return (dxePolyline)dxfPrimatives.CreateCircleStrip(dxfVector.Zero, Radius, bReturnAsPolygon: false, aRotation: aRotated ? 90 : 0, aLeftEdge: Left(aIncludeClearance), aRightEdge: Right(aIncludeClearance));
        }


        public uopProperties ReportProperties(mdTrayAssembly aAssy, int aTableIndex, uopFreeBubblingPanels FBAs = null)
        {

            aAssy ??= GetMDTrayAssembly(aAssy);
            if (aAssy == null) return null;

            uopProperties _rVal = null;


            if (aTableIndex == 1)
            {
                FBAs ??= aAssy.FreeBubblingPanels;

                _rVal = new uopProperties();
                uopFreeBubblingPanel fba = FBAs.Find(x => x.PanelIndex == Index);
                double fa = fba.Area;
                double wl = fba.WeirLength;
                _rVal.Add("DP", Label);
                _rVal.Add("Side 1", fba.WeirLength_Left, uppUnitTypes.SmallLength, aDisplayName: "Side 1");
                _rVal.Add("Side 2", fba.WeirLength_Right, uppUnitTypes.SmallLength, aDisplayName: "Side 2");
                _rVal.Add("FBA", fa, uppUnitTypes.SmallArea, aDisplayName: "FBA");
                _rVal.Add("FBA/WL", wl != 0 ? fa / wl : 0, uppUnitTypes.SmallLength, aDisplayName: "FBA/WL");
                _rVal.Add("Ideal", IdealSpoutArea(aAssy), uppUnitTypes.SmallArea, aDisplayName: "Ideal");
                _rVal.Add("Actual", ComputeSpoutArea(aAssy), uppUnitTypes.SmallArea, aDisplayName: "Actual");
                _rVal.Add("Error", ErrorPercentage(aAssy), uppUnitTypes.Percentage, aDisplayName: "Error");
                _rVal.Add("Ideal", IdealToIdealRatio(aAssy), aDisplayName: "Ideal");
                _rVal.Add("Actual", ActualToActualRatio(aAssy), aDisplayName: "Actual");
                _rVal.SetFormatString("0.0000", _rVal.Count - 1, _rVal.Count);
                _rVal.SetProtected(true);

            }

            return _rVal;
        }

        /// <summary>
        /// the number of slots required on this panel individually
        /// </summary>
        public int RequiredSlotCount
        {
            get
            {
                double tot = (double)TotalRequiredSlotCount;
                int occr = OccuranceFactor;
                int _rVal = (int)tot;
                if (occr > 1) _rVal = mzUtils.VarToInteger(uopUtils.RoundTo(tot / occr, dxxRoundToLimits.One, true));
                return _rVal;
            }
        }

        /// <summary>
        /// ^the length of the weir on the right side of the panel
        /// </summary>

        /// <returns></returns>
        public double RightWeirLength => _FBP == null ? 0 : _FBP.WeirLength_Right;


        /// <summary>
        /// the length of the weir on the left side of the panel/
        /// </summary>
        /// <returns></returns>
        public double LeftWeirLength => _FBP == null ? 0 : _FBP.WeirLength_Left;


        /// <summary>
        /// ^the FreeBUbbleArea associated to this panel
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>


        public uopFreeBubblingPanel FreeBubblingPanel(mdTrayAssembly aAssy = null)
        {
            if (_FBP != null) return _FBP;
            aAssy ??= GetMDTrayAssembly();
            if (aAssy == null) return null;
            mdFreeBubblingAreas fbas = aAssy.FreeBubblingAreas;
            if (fbas == null) return null;
            uopFreeBubblingPanel fbp = fbas.Panels.Find(x => x.PanelIndex == Index);
            SetFBP(fbp);
            return fbp;
        }

        private uopFreeBubblingPanel _FBP;
        public uopFreeBubblingPanel FBP => _FBP;

        private uopPanelSectionShapes _SubShapes; 
        public uopPanelSectionShapes SubShapes => _SubShapes;



        public double TrayFraction => FreeBubblingPanel() == null ? 0 : FreeBubblingPanel().TrayFraction;

        internal void SetFBP(uopFreeBubblingPanel aPanel, uopPanelSectionShapes aPanelShapes = null, bool bClear = true)
        {
            if (bClear)
            {
                _FBP = null;
       
            }
            if (aPanel != null)
            {
                    _FBP = new uopFreeBubblingPanel(aPanel);
              
            }

            if(aPanelShapes != null)
            {
                _SubShapes = aPanelShapes;
               
            }
        }


        /// <summary>
        /// the open area of the panel
        /// the area of the panel less the area blocked by the ring and the support angles
        /// </summary>
        public double FreeBubblingArea(mdTrayAssembly aAssy = null)
        {
            uopFreeBubblingPanel fbp = FreeBubblingPanel(aAssy);
            return fbp!=null ? fbp.Area : 0;
        }




        /// <summary>
        /// all the slot zones of the panel
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public List<mdSlotZone> SlotZones(mdTrayAssembly aAssy)
        { aAssy ??= GetMDTrayAssembly(aAssy); return aAssy != null ? aAssy.SlotZones.GetByPanelIndex(Index) : new List<mdSlotZone>(); }

       


        /// <summary>
        /// the total actual spout area of the spout groups in the deck panel
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public double TotalSpoutArea(mdTrayAssembly aAssy, bool bTrayWide = false) => ComputeSpoutArea(aAssy,bTrayWide);

        /// <summary>
        /// recomputes the current total spout area value
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public double ComputeSpoutArea(mdTrayAssembly aAssy, bool bTrayWide = false)
        {

            aAssy ??= GetMDTrayAssembly();
            if (aAssy == null) return 0;
            colMDSpoutGroups Groups = SpoutGroups(aAssy);
            if (Groups == null) return 0;

            double tally = 0;

            for (int i = 1; i <= Groups.Count; i++)
            {
                mdSpoutGroup aGroup = Groups.Item(i);
                if (aGroup.IsVirtual) continue;
                
                double tot = aGroup.ActualArea;
                double cnt = 1;
                if (bTrayWide)
                {
                    uopInstances insts = aGroup.Instances;
                    foreach (var inst in insts)
                        if (inst.DY == 0) cnt++;
                }
                //if (Math.Round(aGroup.X, 1) > 0) tot *= 2;
                tally += tot * cnt;

                
            }
            return tally;
        }

        /// <summary>
        /// ^the total theoretical spout area required for the spout groups in the deck panel
        /// </summary>
        public double TotalTheoreticalArea(mdTrayAssembly aAssy)
        {

            aAssy ??= GetMDTrayAssembly(aAssy);
            return (aAssy != null) ? SpoutGroups(aAssy).TotTargetArea(aAssy) / OccuranceFactor : 0.0;

        }

        /// <summary>
        /// the the fraction of traywide total weir length that this panel is exposed to. 
        /// equal to RightWeirLength + LeftWeirLength
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aTotalWeirLength"></param>
        /// <returns></returns>

        public double WeirFraction(mdTrayAssembly aAssy, double aTotalWeirLength = 0)
        {
            aAssy ??= GetMDTrayAssembly();
            if (aAssy == null) return 0;
            double totalweir = aTotalWeirLength <= 0 ? aAssy.TotalWeirLength : aTotalWeirLength;
            double mytotal = TotalWeirLength(aAssy);
            return (totalweir != 0) ? mytotal / totalweir : 0;
        }

        /// <summary>
        /// the total weir length for the panel
        /// equal to RightWeirLength + LeftWeirLength
        /// </summary>
        /// <returns></returns>
        public double TotalWeirLength(mdTrayAssembly aAssy = null)
        {
            uopFreeBubblingPanel fbp = FreeBubblingPanel(aAssy);
            return fbp != null ? fbp.WeirLength : 0;
        }

        /// <summary>
        /// ^changes the materials of the members and their sections
        /// </summary>
        /// <param name="aSheetMetal"></param>
        public void UpdateMaterial(uopSheetMetal aSheetMetal)
        {
            if (aSheetMetal == null) return;

            base.SheetMetalStructure = aSheetMetal.Structure;

            mdDeckSections Secs = Sections;
            Secs.Material = aSheetMetal;
            foreach (uopPart item in Secs)
            {
                mdDeckSection aMem = (mdDeckSection)item;
                aMem.SheetMetalStructure = aSheetMetal.Structure;
            }
        }

        public override void UpdatePartWeight() => base.Weight = Weight;

        public override void UpdatePartProperties() { }

        /// <summary>
        ///returns a polyline that is used to draw the layout view of the deck panel
        /// </summary>
        /// <param name="aLayerName"></param>
        /// <param name="aColor"></param>
        /// <param name="aLinetype"></param>
        /// <returns></returns>
        public dxePolyline View_Plan(string aLayerName = "", dxxColors aColor = dxxColors.Undefined, string aLinetype = "")
        => Perimeter.ToDXFPolyline(aLayerName, aColor, aLinetype);


  
        public override bool IsEqual(uopPart aPart)
        {

            if (aPart == null) return false;
            if (aPart.PartType != PartType) return false;
            return IsEqual((mdDeckPanel)aPart);
        }

        public bool IsEqual(mdDeckPanel aPanel)
        {
            if (aPanel == null) return false;
            if (aPanel.Width != Width) return false;
            if (aPanel.IsHalfMoon != IsHalfMoon) return false;
            if (aPanel.Perimeter.Vertices.Count != Perimeter.Vertices.Count) return false;
            if (aPanel.Perimeter.Area != Perimeter.Area) return false;
            return true;

        }

        public override mdDeckPanel GetMDPanel(mdTrayAssembly aAssy = null, mdDeckPanel aPanel = null, int aIndex = -1) => this;

        #endregion Methods

        #region Shared Methods
        
        #endregion Shared Methods
    }
}