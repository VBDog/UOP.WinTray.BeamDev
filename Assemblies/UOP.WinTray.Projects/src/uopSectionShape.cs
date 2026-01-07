using System;
using System.Collections.Generic;
using System.Linq;
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
    public class uopSectionShape : uopPanelSectionShape, ICloneable
    {

        #region Constructors
        public uopSectionShape() => Init();
        public uopSectionShape(uopShape aShape, uopDeckSplice aTopSplice = null, uopDeckSplice aBottomSplice = null, uopTrayAssembly aAssy = null, bool bCloneSplices = true, IEnumerable<iVector> aVertices = null)
        {
            Init(aShape, aTopSplice, aBottomSplice, aAssy, bCloneSplices);
            if(aVertices != null)
            {
                Vertices.Populate(aVertices);
            }

            if (aShape.GetType() == typeof(uopPanelSectionShape))
            {
                base.BPSites = uopVectors.Zero;
                uopPanelSectionShape parent = (uopPanelSectionShape)aShape;

              
                uopVectors panelbpsites = parent.BPSites;
                if (panelbpsites.Count <= 0) return;
                List<uopDeckSplice> splices = Splices();
                if (splices.Count <= 0)
                {
                    base.BPSites.Populate(panelbpsites, true, Handle);
                }
                else
                {
                    uopVectors mysites = uopVectors.Zero;
                    URECTANGLE vbounds = BoundsV;
                    if (parent.IsHalfMoon)
                    {
                        mysites.Populate(panelbpsites.FindAll(x =>vbounds.ContainsOrd(x.X,bOrdIsY:false,bOnIsIn:false,aPrecis:4)) , true, Handle);
                    }
                    else
                    {
                        mysites.Populate(panelbpsites.FindAll(x => vbounds.ContainsOrd(x.Y, bOrdIsY: true, bOnIsIn: false, aPrecis: 4)), true, Handle);
                    }

                    double rad = mdGlobals.BPRadius;
                    foreach (var splice in splices)
                    {
                        URECTANGLE limits = uopDeckSplice.SpliceLimits(splice, bTrimToFlange: false, bMechanical: false, aAdder: 0.9 * rad);
                        foreach (var bp in mysites)
                        {
                            if (splice.Vertical)
                            {
                                if (bp.X >= limits.Left && bp.X <= limits.Right) bp.Suppressed = true;
                            }
                            else
                            {
                                if (bp.Y >= limits.Bottom && bp.Y <= limits.Top) bp.Suppressed = true;
                            }
                        }


                    }
                    base.BPSites = mysites;
                }
           


                
            }

        }


        private void Init(uopShape aShape, uopDeckSplice aTopSplice = null, uopDeckSplice aBottomSplice = null, uopTrayAssembly aAssy = null, bool bCloneSplices = true)
        {

            TopSplice = null;
            BottomSplice = null;
            _Depth = null;
            PanelSectionCount = 1;
            _SlotPoints = uopVectors.Zero;
            _SectionAboveRef = null;
            _SectionBelowRef = null;
            _JoggleAngleHeight = 0;
            Divider = null;
            SlotZone = null;
          
            base.Init(aShape, aAssy);
            
            if(aShape.GetType() == typeof(uopSectionShape))
            {
               
                uopSectionShape secshape = (uopSectionShape)aShape;
                TopSplice = uopDeckSplice.CloneCopy(secshape.TopSplice);
                BottomSplice = uopDeckSplice.CloneCopy(secshape.BottomSplice);
                _TopSpliceType = secshape._TopSpliceType;
                _BottomSpliceType = secshape._BottomSpliceType;
                PanelSectionCount = secshape.PanelSectionCount;
                _SlotPoints = uopVectors.CloneCopy( secshape._SlotPoints);
                SectionAbove = secshape.SectionAbove;
                SectionBelow = secshape.SectionBelow;
                DowncomerClearance = secshape.DowncomerClearance;
                BPSites = uopVectors.CloneCopy(secshape.BPSites);
                _JoggleAngleHeight = secshape.JoggleAngleHeight;
                Divider = DividerInfo.CloneCopy(secshape.Divider);
                SlotZone = mdSlotZone.CloneCopy(secshape.SlotZone);
                _Perimeter = (dxePolygon)dxfEntity.CloneCopy(secshape._Perimeter);
                _SimplePerimeter = (dxePolyline)dxfEntity.CloneCopy(secshape._SimplePerimeter);
                
            }

            if (aTopSplice != null) TopSplice = bCloneSplices ? new uopDeckSplice(aTopSplice) : aTopSplice;
            if (aBottomSplice != null) BottomSplice = bCloneSplices ? new uopDeckSplice(aBottomSplice) : aBottomSplice;

            RectifySplices();
        }
        #endregion Constructors


        #region Properties
        /// <summary>
        /// the panel index coupled with the section index like 1,2
        /// </summary>
        /// <remarks>If there are multible panel sections (MultiPanel) the panels section index is include as the middle item</remarks>
        public override string Handle { get => MultiPanel ? $"{PanelIndex},{PanelSectionIndex},{SectionIndex}" : $"{PanelIndex},{SectionIndex}"; set { } }

        public override string Name { get => $"SECTION SHAPE {Handle}"; set { } }

        public string Descriptor
        {
            get
            {
                string _rVal = $"{Name}";
                _rVal += IsHalfMoon ? $" X: {X:0.000}" : $" Y: {Y:0.000}";
                if (IsHalfMoon) _rVal += " Moon";
                if (IsManway) _rVal += $" Manway {ManwayHandle}";
                _rVal += $" OCCR: {OccuranceFactor}";
                if(IsRectangular) _rVal += $" SYM: {IsSymmetric}";
                return _rVal ;
            }
        }

        public string ManwayHandle =>SpliceCount < 2 ? string.Empty : TopSplice.ManwayHandle;
               
        public int SpliceCount => TopSplice == null && BottomSplice == null ? 0 : TopSplice != null && BottomSplice != null ? 2 : 1;

        /// <summary>
        /// the splice that defines the shape at the top or right for moon sections
        /// </summary>
        public uopDeckSplice TopSplice { get;set; }

        /// <summary>
        /// the splice that defines the shape at the bottom or left for moon sections
        /// </summary>
        public uopDeckSplice BottomSplice { get; set; }

      

        public bool IsManway => SpliceCount < 2 ? false : TopSplice.SupportsManway && BottomSplice.SupportsManway;


        public double FlangeHt
        {
            set
            {
                if(TopSplice != null) TopSplice.FlangeHt = value;
                if (BottomSplice != null) BottomSplice.FlangeHt = value;
            }
        }

        protected new double? _Depth;
        public new  double Depth
        {
            get
            {
                if(_Depth.HasValue) return _Depth.Value;
          
                double h1 = TopSplice != null ? TopSplice.JoggleAngleHeight : 0;
                double h2 = BottomSplice != null ? BottomSplice.JoggleAngleHeight : 0;
               return Thickness + Math.Max(h1,h2);
                
            }
            set
            {
                 _Depth = value;
            }
        }


        public new double FitWidth
        {
            get
            {

                URECTANGLE mechlimits = MechanicalBounds.Value.Limits;
                double wd = mechlimits.Width;
                double ht = mechlimits.Height;

                
                
                double _rVal =  Math.Min(wd, ht);
                GetSplices(out uopDeckSplice top, out uopDeckSplice bot);

                if (!IsHalfMoon && LapsRing && Math.Round(X,3) != 0 &&( (top==null && bot != null) || (top != null && bot == null)))
                {
                    

                    double d1 = -1;
                    uopArc trimArc = Segments.ArcSegments(aRadius: DeckRadius, aPrecis: 3).FirstOrDefault();
                    if (trimArc != null)
                    {
                        uopLine interceptor = new uopLine();
                        if (top == null && bot != null)
                        {
                            interceptor.ep = Vertices.GetVector(X > 0 ? dxxPointFilters.GetLeftBottom : dxxPointFilters.GetRightBottom);
                        }
                        else
                        {
                            interceptor.ep = Vertices.GetVector(X > 0 ? dxxPointFilters.GetLeftTop : dxxPointFilters.GetRightTop);
                        }
                        uopVector u1 = interceptor.Intersections(trimArc, aArcIsInfinite: false, aLineIsInfinite: true).Farthest(interceptor.sp);
                        if(u1 != null)
                        {
                            d1 = interceptor.ep.DistanceTo(u1);
                        }
                    }
                     
                   


                    if (d1 > 0 && d1 < _rVal) _rVal = d1;
                }

                return _rVal;
            }
            
        }
        public override double Radius { get => DeckRadius; set { DeckRadius = value; } }

        public int PanelSectionCount { get;set; }

  

        /// <summary>
        /// the distance between splice centers
        /// </summary>
        public double BaseHeight
        {
            get
            {

                RectifySplices(out uopDeckSplice top, out uopDeckSplice bot);
                if (!IsHalfMoon)
                {
                    double y1 = top == null ? Top : top.Ordinate;
                    double y2 = bot == null ? Bottom : bot.Ordinate ;
                    return y1 - y2;



                }
                else
                {
                    double x1 = top == null ? Right : top.Ordinate;
                    double x2 = bot == null ? Left : bot.Ordinate;
                    return x1 - x2;

                }

            }
        }

        public bool DoubleFemale 
        { get
            {
                RectifySplices(out uopDeckSplice top, out uopDeckSplice bot);
                if (top == null || bot == null) return false;
                return top.TabDirection == DXFGraphics.dxxOrthoDirections.Down && bot.TabDirection == DXFGraphics.dxxOrthoDirections.Up;
            }
            set
            {
                RectifySplices(out uopDeckSplice top, out uopDeckSplice bot);
                if (top == null || bot == null) return;
                 top.TabDirection =  value ? DXFGraphics.dxxOrthoDirections.Down : DXFGraphics.dxxOrthoDirections.Up;
                 bot.TabDirection = value ? DXFGraphics.dxxOrthoDirections.Up : DXFGraphics.dxxOrthoDirections.Down;
            }
        }

        private uopVectors _SlotPoints;
        public uopVectors SlotPoints
        {
            get { _SlotPoints ??= uopVectors.Zero; return _SlotPoints; }
            set  => _SlotPoints = value;
        }

        private uppSpliceIndicators? _TopSpliceType;
        public uppSpliceIndicators TopSpliceType 
        {
            get 
            {
                if (_TopSpliceType.HasValue) return _TopSpliceType.Value;
                return TopSplice != null ? TopSplice.SpliceIndicator : uppSpliceIndicators.ToRing;
              
            }
            
            set { _TopSpliceType = value; }

        }

        private uppSpliceIndicators? _BottomSpliceType;
        public uppSpliceIndicators BottomSpliceType
        {
            get
            {
                if (_BottomSpliceType.HasValue) return _BottomSpliceType.Value;
                return BottomSplice != null ? BottomSplice.SpliceIndicator :  uppSpliceIndicators.ToRing;
           
                
            }

            set { _BottomSpliceType = value; }

        }
        public new bool IsRectangular { get => !(HasArcs || LapsDivider); }

        public bool IsSymmetric 
        {
            get
            {
                if(IsHalfMoon || !IsRectangular) return false;
                if (IsManway && SpliceStyle == uppSpliceStyles.Tabs) return false;
                GetSplices(out uopDeckSplice top, out uopDeckSplice bot);
                if(top == null || bot == null) return false;
                if (top.SpliceType != bot.SpliceType) return false;
                if(top.SpliceType != uppSpliceTypes.SpliceWithAngle)
                {
                    if (top.Female != top.Female) return false;
                }
                if (!SuppressBubblePromoters)
                {
                    var sites = BPSites;
                    if (sites.Count > 0)
                    {
                        if(sites.Count == 1)
                        {
                            return Math.Round(sites.Item(1).Y, 3) == Math.Round(Y, 3);
                        }
                        var above = sites.GetVectors(dxxPointFilters.GreaterThanY, Y, bOnIsIn:true, aPrecis:3);
                        var below = sites.GetVectors(dxxPointFilters.LessThanY, Y, bOnIsIn: true, aPrecis: 3);
                        if (above.Count != below.Count) return false;
                        for (int i = 1; i <= above.Count; i++)
                        {
                            var u1 = above[i-1];
                            var u2 = below.Find(x => Math.Round(Math.Abs(x.Y - Y), 3) == Math.Round(Math.Abs(u1.Y - Y), 3));
                            if(u2 == null)
                                return false;
                            
                        }
                    }

                }

                return true;
            }
        }

        private WeakReference<uopSectionShape> _SectionAboveRef;
        public uopSectionShape SectionAbove
        {
            get
            {
                if (_SectionAboveRef == null) return null;
                if (!_SectionAboveRef.TryGetTarget(out uopSectionShape _rVal)) _SectionAboveRef = null;
                return _rVal;
            }
            set
            {
                if (value == null) { _SectionAboveRef = null; return; }
                _SectionAboveRef = new WeakReference<uopSectionShape>(value);
            }
        }


        private WeakReference<uopSectionShape> _SectionBelowRef;
        public uopSectionShape SectionBelow
        {
            get
            {
                if (_SectionBelowRef == null) return null;
                if (!_SectionBelowRef.TryGetTarget(out uopSectionShape _rVal)) _SectionBelowRef = null;
                return _rVal;
            }
            set
            {
                if (value == null) { _SectionBelowRef = null; return; }
                _SectionBelowRef = new WeakReference<uopSectionShape>(value);
            }
        }
        public double DowncomerLap => ShelfWidth - DowncomerClearance ;

      


public double LeftDowncomerFlangeInset
        {
            get
            {
                return LeftDowncomerInfo == null ? 0 : SpliceStyle!= uppSpliceStyles.Tabs ?   LeftDowncomerLap :  mdGlobals.DeckTabFlangeInset;
            }
        }

        public double RightDowncomerFlangeInset
        {
            get
            {
                return RightDowncomerInfo == null ? 0 : SpliceStyle != uppSpliceStyles.Tabs ? RightDowncomerInfo.ShelfWidth - DowncomerClearance + 0.125 : mdGlobals.DeckTabFlangeInset;
            }
        }

        
        public override uopVectors BPSites 
            { 
            get 
            {

                if (SuppressBubblePromoters) return uopVectors.Zero;

                uopRectangle bounds = this.Limits();
                uopVectors _rVal = ParentShape == null ?  base.BPSites : ParentShape.BPSites;

                if(_rVal.Count > 0 && GetSplices(out uopDeckSplice top, out uopDeckSplice bot))
                {
                     uopRectangles splicelims = uopDeckSplices.GetSpliceLimits(top, bot, 0.9 * mdGlobals.BPRadius);
                    foreach (uopVector ps in _rVal)
                    {
                        foreach (var psplicelims in splicelims)
                        {
                            if (psplicelims.Contains(ps, bOnIsOut: false))
                            {
                                ps.Suppressed = true;
                                break;
                            }
                        }
                    }


                }

                return _rVal.GetVectors(bounds, bOnIsIn:false, bReturnClones:true, bSuppressedVal: false);
             
             
            } 
            set => base.BPSites = value; }


        private double _JoggleAngleHeight;
        public double JoggleAngleHeight { get => _JoggleAngleHeight; set { _JoggleAngleHeight = value; if (TopSplice != null) TopSplice.FlangeHt = _JoggleAngleHeight; if (BottomSplice != null) BottomSplice.FlangeHt = _JoggleAngleHeight; } }


        /// <summary>
        /// one of two possible splices
        /// </summary>
        public string Splice1Handle => TopSplice != null ? $"{PanelIndex},{TopSplice.SpliceIndex}" : string.Empty;

        /// <summary>
        /// one of two possible splices
        /// </summary>
        public string Splice2Handle => BottomSplice != null ? $"{PanelIndex},{BottomSplice.SpliceIndex}" : string.Empty;

        public bool HasSplices => TopSplice != null || BottomSplice !=null;
        public bool RequiresSlotting => MDDesignFamily.IsEcmdDesignFamily();
        public string TopSpliceTypeName => TopSplice != null ? TopSplice.IndicatorStyleName : IsHalfMoon ? uopDeckSplice.IndicatorName(uppSpliceIndicators.ToRing): uopDeckSplice.IndicatorName(uppSpliceIndicators.ToRing);


        /// <summary>
        /// the hole used to splice the section to a manway angle or a splice angle
        /// size changes with the bolting property.
        /// </summary>
        internal UHOLE SpliceHole => new UHOLE(mdGlobals.SpliceHoleDiameter, 0, 0, aDepth: Thickness, aElevation: 0.5 * Thickness, aTag: "SPLICE HOLE");

        /// <summary>
        /// the hole in the angle that receives the bolts
        /// </summary>
        internal UHOLE BoltHole => new UHOLE(mdGlobals.gsSmallHole, X, Y, aDepth: Thickness, aElevation: 0.5 * Thickness, bWeldedBolt: true, aTag: "BOLT", aFlag: "HOLE");

        /// <summary>
        /// the hole in the angle that receives the bolts
        /// </summary>
        internal UHOLE LapHole => new UHOLE(aDiameter: mdGlobals.SpliceHoleDiameter, 0, 0, aDepth: Thickness, aElevation: 1.5 * Thickness, aTag: "LAP", aFlag: "HOLE");

        internal UHOLEARRAY GenHoles(uopTrayAssembly aAssy = null,  string aTag = "", string aFlag = "", bool bTrayWide = false)
        {
            aAssy ??= TrayAssembly;
          
            if (aAssy != null && aAssy.ProjectFamily == uppProjectFamilies.uopFamMD) return uopSectionShapes.GenMDHoles(this, (mdTrayAssembly)aAssy, aTag, aFlag, bTrayWide);
            return UHOLEARRAY.Null;
        }

        public override uopShapes GetSubShapes(uppSubShapeTypes aType, uopTrayAssembly aAssy = null)
        {
            aAssy ??= TrayAssembly;
            
            switch (aType)
            {
                case uppSubShapeTypes.BlockedAreas:
                    {
                        return uopSectionShapes.GenMDBlockedAreas(this, (mdTrayAssembly)aAssy);
                        
                    }
                case uppSubShapeTypes.SlotBlockedAreas:
                    {
                        return uopSectionShapes.GenMDBlockedAreas(this, (mdTrayAssembly)aAssy,false,true);
             
                    }
            }
            


            return new uopShapes(aType.Description());
        }

    
        public string BottomSpliceTypeName => BottomSplice != null ? BottomSplice.IndicatorStyleName : IsHalfMoon ? uopDeckSplice.IndicatorName(uppSpliceIndicators.ToDowncomer) : uopDeckSplice.IndicatorName(uppSpliceIndicators.ToRing);
        public bool SplicedOnTop => TopSplice != null;
        public bool SplicedOnBottom => BottomSplice != null;

        public mdSlotZone SlotZone { get; set; }
        private dxePolyline _SimplePerimeter;
        public dxePolyline SimplePerimeter { get => UpdateSimplePerimeter(false); set => _SimplePerimeter = value; }
        private dxePolygon _Perimeter;
        public new dxePolygon Perimeter { get => UpdatePerimeter(false); set => _Perimeter = value; }

        public bool HasMechanicalBounds => base.MechanicalBounds.HasValue;
        /// <summary>
        /// the shape that is  the mechanical shape of the secction without any splice details 
        /// </summary>
        internal override USHAPE? MechanicalBounds 
        {
            get 
            {
                UpdateSimplePerimeter(false);

                return base.MechanicalBounds;
            }
            set { base.MechanicalBounds = value; }
        }

        public uopRectangle MechanicalLimits => new uopRectangle(SimplePerimeter.BoundingRectangle());

        public List<double> TabOrdinates
        {
            get
            {
                if (!GetSplices(out uopDeckSplice top, out uopDeckSplice bot)) return new List<double>();
                if (top != null && top.SpliceStyle == uppSpliceStyles.Tabs && !top.Female) return top.FlangeLine.FastenerOrdinates;
                if (bot != null && bot.SpliceStyle == uppSpliceStyles.Tabs && !bot.Female) return bot.FlangeLine.FastenerOrdinates;
                if (top != null && top.SpliceStyle == uppSpliceStyles.Tabs && top.Female) return top.FlangeLine.FastenerOrdinates;
                if (bot != null && bot.SpliceStyle == uppSpliceStyles.Tabs && bot.Female) return bot.FlangeLine.FastenerOrdinates;
                return new List<double>();
            }
        }

     


        /// <summary>
        /// the fraction of the parent panels mechanical area that this section represents
        /// </summary>
        public double MechanicalPanelFraction
        {
            get
            {
                uopPanelSectionShape panel = ParentShape;
              
                if (panel == null) 
                    return 0;
                
                if (DeckRadius <= 0)
                    return 0;


                double fba = FreeBubblingArea.Area;
                if (fba <= 0)
                    return 0;

                if(panel.DeckRadius <= 0)
                {
                    panel.ShellRadius = ShellRadius;
                    panel.RingRadius = RingRadius;
                    panel.DeckRadius = DeckRadius;
                }

                uopShape parent = panel.FreeBubblingArea;

                
                if (parent == null) 
                    return 0;
                double parentfba = parent.Area;
                if (parentfba <= 0)
                    return 0;
                return parentfba / fba;
            }
        }
        #endregion Properties

        #region Methods

        public override uopFreeBubblingArea CreateFreeBubblingArea()
        {
            uopFreeBubblingArea _rVal = base.CreateFreeBubblingArea();
            _rVal.SectionIndex = SectionIndex;
            _rVal.SubShapes = GetSubShapes(uppSubShapeTypes.BlockedAreas);
            return _rVal;
        }
        public override uopCompoundShape SlotZoneBounds(out bool rUnSlottable)
        {
            rUnSlottable = true;
            mdTrayAssembly aAssy = MDTrayAssembly;
            DowncomerInfo dcleft = LeftDowncomerInfo;
            DowncomerInfo dcright = RightDowncomerInfo;

            rUnSlottable = false;
            double rad = RingRadius - mdGlobals.SlotDieClearance; //buffer for slot dies
            double wht = WeirHeight;

            GetSplices(out uopDeckSplice tSplc, out uopDeckSplice bSplc);

            if (wht <= 0 && aAssy != null) wht = aAssy.WeirHeight;
            if (wht < 1) wht = 1;

            double halfDieW = mdGlobals.SlotDieWidth / 2;
            double halfDieH = mdGlobals.SlotDieHeight / 2;
            double leftdcoffset = dcleft != null ? halfDieW + wht : 0;
            double rightdcoffset = dcright != null ? -(halfDieW + wht) : 0;
            double topspliceoffset = 0;
            double botspliceoffset = 0;
            double leftedgeoffset = 0;
            double rightedgeoffset = 0;
            double radoffset = -(RingLap + mdGlobals.SlotDieClearance);

            if(tSplc != null)
            {
                if (tSplc.Vertical)
                {
                    rightedgeoffset = ( X >= 0 ? -1 : 1) * (mdGlobals.SlotDieWidth * 0.501);
                    rightedgeoffset += (X >= 0 ? -1 : 1) *  tSplc.GapValue();
                }
                else
                {
                    topspliceoffset = -halfDieH - tSplc.GapValue();
                    if(tSplc.SpliceType == uppSpliceTypes.SpliceWithAngle || tSplc.SpliceType == uppSpliceTypes.SpliceManwayCenter)
                    {
                        topspliceoffset -= mdGlobals.SpliceAngleWidth / 2;
                    }
                    if (tSplc.SpliceType == uppSpliceTypes.SpliceWithTabs && !tSplc.Female)
                    {
                        topspliceoffset -=  mdGlobals.DeckTabFlangeHeight +tSplc.GapValue();
                    }
                }
            }
            if (bSplc != null)
            {
                if (bSplc.Vertical)
                {
                    leftedgeoffset = (X >= 0 ? 1 : -1) * (mdGlobals.SlotDieWidth *0.501);
                    leftedgeoffset += (X >= 0 ? 1 : -1) * mdGlobals.DeckTabFlangeHeight + bSplc.GapValue();
                }
                else
                {
                    botspliceoffset = halfDieH + bSplc.GapValue();
                    if (bSplc.SpliceType == uppSpliceTypes.SpliceWithAngle || bSplc.SpliceType == uppSpliceTypes.SpliceManwayCenter)
                    {
                        botspliceoffset += mdGlobals.SpliceAngleWidth / 2;
                    }
                    if (bSplc.SpliceType == uppSpliceTypes.SpliceWithTabs && !bSplc.Female)
                    {
                        botspliceoffset += mdGlobals.DeckTabFlangeHeight + bSplc.GapValue();
                    }
                }
            }

            uopCompoundShape _rVal = CreateSubShape($"SLOT ZONE BOUNDS {Handle}", aRadiusOffset: radoffset, aLeftDowncomerOffset: leftdcoffset, aRightDowncomerOffset: rightdcoffset, aTopSpliceOffset: topspliceoffset, aBottomSpliceOffset: botspliceoffset,aLeftEdgeOffset: leftedgeoffset, aRightEdgeOffset: rightedgeoffset);
            
            URECTANGLE limits = new URECTANGLE(Bounds);

            if (dcleft != null) limits.Left = dcleft.X_Outside_Right + halfDieW + wht;
            if (dcright != null) limits.Right = dcright.X_Outside_Left - (halfDieW + wht);
            limits.Top = Top;
            limits.Bottom = Bottom;

            //if (IsHalfMoon)
            //{
            //    if (X > 0)
            //    {
            //        limits.Left += bSplc == null ? halfDieW + wht : bSplc.FlowSlotClearance + halfDieW;
            //        limits.Right -= tSplc == null ? mdGlobals.SlotDieClearance : tSplc.FlowSlotClearance + halfDieW;

            //    }
            //    else
            //    {
            //        limits.Right -= bSplc == null ? halfDieW + wht : bSplc.FlowSlotClearance + halfDieW;
            //        limits.Left += tSplc == null ? mdGlobals.SlotDieClearance : tSplc.FlowSlotClearance + halfDieW;
            //    }

            //}
            //else
            //{
            //    limits.Left += halfDieW + wht;
            //    limits.Right -= (halfDieW + wht);

            //    limits.Bottom += bSplc == null ? halfDieH : bSplc.FlowSlotClearance + halfDieH;
            //    limits.Top -= tSplc == null ? mdGlobals.SlotDieClearance : tSplc.FlowSlotClearance + halfDieH;

            //}
            rUnSlottable = true;
            if (limits.Left < limits.Right && limits.Bottom < limits.Top)
            {
                if (limits.Right - limits.Left >= mdGlobals.SlotDieWidth && limits.Top - limits.Bottom >= mdGlobals.SlotDieHeight)
                    rUnSlottable = false;
            }

            _rVal.SubShapes = GetSubShapes(uppSubShapeTypes.SlotBlockedAreas, aAssy);

            return _rVal;
        }

        public void ResetMechanicaBounds() { base.MechanicalBounds = null;  _SimplePerimeter = null; }

        public bool GetSupportsManway(out bool rOnTop)
        {
            rOnTop = false;

            if (IsManway) return false;
            GetSplices(out uopDeckSplice top, out uopDeckSplice bot);

            if (top != null && top.SupportsManway) return true;
            if (bot != null && bot.SupportsManway) return true;

            return false;
        }

        /// <summary>
        /// the arcs and lines used to layout the ring clip holes on the section
        /// </summary>
        /// <param name="rTrimRectangle"> return</param>
        /// <returns></returns>
        public override List<uopRingClipSegment> RingClipSegments(out uopShape rRingClipBounds) => uopUtils.CreateRingClipSegments(this, out rRingClipBounds);

        public override uopShape RingClipArcBounds()
        {
  
            double radoffset = -(DeckRadius - TrimRadius);
            double dcoffsetL = LeftDowncomerInfo != null ? LeftDowncomerInfo.ShelfWidth + mdGlobals.MinRingClipClearance : 0;
            double dcoffsetR = RightDowncomerInfo != null ? -(RightDowncomerInfo.ShelfWidth+ mdGlobals.MinRingClipClearance) : 0;
            double spliceClearanceB = 0;
            double spliceClearanceT = 0;
            double leftSideOffset = 0;
            double rightSideOffset = 0;
            double washerRad = mdGlobals.HoldDownWasherRadius;
            GetSplices(out uopDeckSplice top, out uopDeckSplice bot);
            if (IsHalfMoon)
            {
                if(bot != null)
                {
                    uopFlangeLine flngLn = new uopFlangeLine(bot, true, this);
                    if (X > 0)
                        leftSideOffset = (flngLn.X() + mdGlobals.MinRingClipClearance) - Left;
                    else
                        rightSideOffset = -((flngLn.X() + mdGlobals.MinRingClipClearance) - Right);

                }
                if(top != null)
                {
                    
                    if (X > 0)
                        rightSideOffset = -(washerRad + 0.5); 
                    else
                        leftSideOffset = washerRad + 0.5; 
                }
           
            }
            

                return CreateSubShape("RING CLIP BOUNDS", aRadiusOffset: radoffset, aLeftDowncomerOffset: dcoffsetL, aRightDowncomerOffset: dcoffsetR, aTopSpliceOffset: spliceClearanceT, aBottomSpliceOffset: spliceClearanceB, aLeftEdgeOffset: leftSideOffset, aRightEdgeOffset: rightSideOffset, bIncludeQuadrantPoints:true);
          }
        
        public uopLines SplicesLines(bool bGetTop = true, bool bGetBottom = true, double aTopOffset = 0, double aBottomOffset = 0)
        {
            uopLines _rVal = new uopLines();
            if (TopSplice != null && bGetTop)
                _rVal.Add(TopSplice.SpliceLine(this, aTopOffset));
          
            
            if (BottomSplice != null && bGetBottom)
                _rVal.Add(BottomSplice.SpliceLine(this, aBottomOffset));

            
            return _rVal;
        }
        public  List<uopDeckSplice> Splices(bool bGetTop = true, bool bGetBottom = true, bool bGetClones = false)
        {
            List<uopDeckSplice> _rVal = new List<uopDeckSplice>();
            if (TopSplice != null && bGetTop) _rVal.Add(bGetClones ? new uopDeckSplice(TopSplice) : TopSplice);
            if (BottomSplice != null && bGetBottom) _rVal.Add(bGetClones ? new uopDeckSplice(BottomSplice) : BottomSplice);
            return _rVal;
        }

        public override string ToString() => Descriptor;
        public bool SupportsManway() => SupportsManway(out _);

        public bool SupportsManway(out bool rOnTop)
        {
            rOnTop = false;
           
            if (IsManway) return false;
            GetSplices(out uopDeckSplice top, out uopDeckSplice bot);

            if (top != null && top.SupportsManway) return true;
            if (bot != null && bot.SupportsManway) return true;

            return false;
        }
        public new void Copy(uopShape aShape)
        {
            if (aShape == null) return;
            base.Copy(aShape);

            if (aShape.GetType() == typeof(uopSectionShape))
            {
               
                uopSectionShape secshape = (uopSectionShape)aShape;
                TopSplice = uopDeckSplice.CloneCopy(secshape.TopSplice);
                BottomSplice = uopDeckSplice.CloneCopy(secshape.BottomSplice);
                _TopSpliceType = secshape._TopSpliceType;
                _BottomSpliceType = secshape._BottomSpliceType;
                PanelSectionCount = secshape.PanelSectionCount;
                _SlotPoints = uopVectors.CloneCopy(secshape._SlotPoints);
                SectionAbove = secshape.SectionAbove;
                SectionBelow = secshape.SectionBelow;
                DowncomerClearance = secshape.DowncomerClearance;
                BPSites = uopVectors.CloneCopy(secshape.BPSites);
                _JoggleAngleHeight = secshape.JoggleAngleHeight;
                Divider = DividerInfo.CloneCopy(secshape.Divider);
                SlotZone =  mdSlotZone.CloneCopy( secshape.SlotZone);
                _Perimeter = (dxePolygon)dxfEntity.CloneCopy(secshape._Perimeter);
                _SimplePerimeter = (dxePolyline)dxfEntity.CloneCopy(secshape._SimplePerimeter);
            }
        
        }
        
        public new uopSectionShape Clone() => new uopSectionShape(this);
        object ICloneable.Clone() => (object)new uopSectionShape(this);

        public new  bool FitsThruManhole(double? aManHoleID = null, double aClearance = 0.5)
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

        public List<uopFlangeLine> FlangeLines()
        {
            List<uopFlangeLine> _rVal = new List<uopFlangeLine>();
            GetSplices(out uopDeckSplice top, out uopDeckSplice bot);
            if (top != null) _rVal.Add(new uopFlangeLine(top.FlangeLine));
            if (bot != null) _rVal.Add(new uopFlangeLine(bot.FlangeLine));

            return _rVal;
        }

        /// <summary>
        /// returns true if either of the splices are truncated by the ring or the beam/divider
        /// </summary>
        /// <param name="bGetTop">if passed, the test will only include the top or bottom splice</param>
        /// <returns></returns>
        public bool TruncatedFlangeLine(bool? bGetTop = null)
        {
            GetSplices(out uopDeckSplice top, out uopDeckSplice bot);
            if (bGetTop.HasValue)
            {
                if (top != null && bGetTop.Value) return top.FlangeLine.IsTruncated;
                if (bot != null && !bGetTop.Value) return bot.FlangeLine.IsTruncated;

            }
            else
            {
                if (top != null && top.FlangeLine!= null && top.FlangeLine.IsTruncated) return true;
                if (bot != null && bot.FlangeLine != null && bot.FlangeLine.IsTruncated) return true;
            }
            return false;
        }



        public override bool GetSplices(out uopDeckSplice rTopSplice, out uopDeckSplice rBottomSplice,bool bGetClones = false)
        {
            rTopSplice = TopSplice; rBottomSplice = BottomSplice;
            if (rTopSplice != null) rTopSplice.Section = this;
            if(bGetClones && rTopSplice != null) rTopSplice = new uopDeckSplice(rTopSplice);
            if (rBottomSplice != null) rBottomSplice.Section = this;
            if (bGetClones && rBottomSplice != null) rBottomSplice = new uopDeckSplice(rBottomSplice);
            return rTopSplice != null || rBottomSplice != null;
        }

        public bool GetRelatives(out uopSectionShape rAbove, out uopSectionShape rBelow, List<uopSectionShape> aSource = null,  bool bGetClones = false)
        {
            rAbove = SectionAbove; rBelow = SectionBelow;
            if (aSource != null)
            {
                rAbove = aSource.Find(x => x.PanelIndex == PanelIndex && x.PanelSectionIndex ==  PanelSectionIndex && x.SectionIndex == SectionIndex -1);
                rBelow = aSource.Find(x => x.PanelIndex == PanelIndex && x.PanelSectionIndex == PanelSectionIndex && x.SectionIndex == SectionIndex + 1);
            }
            if (bGetClones && rAbove != null) rAbove = new uopSectionShape(rAbove);
            if (bGetClones && rBelow != null) rBelow = new uopSectionShape(rBelow);
            return rAbove != null || rBelow != null;
        }

        public bool RectifySplices(out uopDeckSplice rTopSplice, out uopDeckSplice rBottomSplice, bool? bMark  = null, uopSectionShape aParentShape = null)
        {
            rBottomSplice = BottomSplice ;
            rTopSplice = TopSplice;
            if (bMark.HasValue) Mark = bMark.Value;
            if (aParentShape != null) 
            {
                if (TopSplice != null) TopSplice.Section = aParentShape;
                if (BottomSplice != null) BottomSplice.Section = aParentShape;
            }
            else
            {
                if (TopSplice != null) TopSplice.Section = this;
                if (BottomSplice != null) BottomSplice.Section = this;

            }
            if (TopSplice == null || BottomSplice == null) return rBottomSplice != null || rTopSplice != null;
            bool swapem = false;
            
            if (IsHalfMoon)
            {
                if (TopSplice != null && BottomSplice != null)
                {
                    swapem = (X > 0 && TopSplice.X < BottomSplice.X) || (X < 0 && TopSplice.X > BottomSplice.X);

                }
                else if (BottomSplice != null && TopSplice == null)
                {
                    swapem = (X > 0 && BottomSplice.X > X) || (X < 0 && BottomSplice.X < X);
                }
                else if (BottomSplice == null && TopSplice != null)
                {
                    swapem = (X > 0 && TopSplice.X < X) || (X < 0 && TopSplice.X > X);
                }
            }
            else
            {
                if (TopSplice != null && BottomSplice != null)
                {
                    swapem = TopSplice.Y < BottomSplice.Y;

                }
                else if (BottomSplice != null && TopSplice == null)
                {
                    swapem = BottomSplice.Y > Y;
                }
                else if (BottomSplice == null && TopSplice != null)
                {
                    swapem = TopSplice.Y >Y;
                }
            }

            if (swapem)
            {
                uopDeckSplice s1 = TopSplice;
                TopSplice = BottomSplice;
                BottomSplice = s1;
            }

            if (TopSplice != null)
            {
                TopSplice.Side = IsHalfMoon ? PanelX >= 0 ? uppSides.Right : uppSides.Left : uppSides.Top;
                TopSplice.SpliceBoltCount = SpliceBoltCount;
            }
                if (BottomSplice != null) 
            {
                BottomSplice.Side = IsHalfMoon ? PanelX >= 0 ? uppSides.Left : uppSides.Right : uppSides.Bottom;
                BottomSplice.SpliceBoltCount = SpliceBoltCount;
            }

            rTopSplice = TopSplice;
            rBottomSplice = BottomSplice;
            return rBottomSplice != null || rTopSplice != null;
        }

        public void RectifySplices() => RectifySplices(out _, out _);

        public override void Move(double aX = 0, double aY = 0)
        {
            if(aX==0 && aY == 0) return;
            base.Move(aX, aY);
        
            _Perimeter?.Move(aX,aY);
            _SimplePerimeter?.Move(aX, aY);
                
          
        }

        public override bool Mirror(double? aX, double? aY)
        {
           bool _rVal =  base.Mirror(aX, aY);
            _Perimeter?.MirrorPlanar(aX, aY);
            _SimplePerimeter?.MirrorPlanar(aX, aY);
            return _rVal;
        }

        public dxePolygon UpdatePerimeter(bool bRegen, uopTrayAssembly aAssy = null, bool bVerbose = false)
        {
            if (bRegen) Perimeter = null;
            if (_Perimeter != null && !bRegen) return _Perimeter;
            _Perimeter = GeneratePerimeter(aAssy, bVerbose);
            return _Perimeter;

        }


        public dxePolygon GeneratePerimeter( uopTrayAssembly aAssy = null, bool bVerbose = false)
        {
           
            this.RectifySplices(out uopDeckSplice top, out uopDeckSplice bot);
            //if (SectionAbove != null && SectionAbove.DoubleFemale)
            //{
            //    Console.WriteLine(Handle);
            //}

            uopTrayAssembly assy = aAssy != null ? aAssy : TrayAssembly;
            if (bVerbose && assy !=null) assy.RaiseStatusChangeEvent($"Generating {assy.TrayName()} Section Shape {Handle} Perimeter");

            dxePolygon topPgon = top != null ? top.PerimeterPolygon(true,aSection: this) : null;
            dxePolygon botPgon = bot != null ? bot.PerimeterPolygon(true,aSection: this) : null;


            colDXFVectors topverts = topPgon != null ? new colDXFVectors(topPgon.Vertices) : colDXFVectors.Zero;
            colDXFVectors botverts = botPgon != null ? new colDXFVectors(botPgon.Vertices) : colDXFVectors.Zero;

            colDXFEntities topaddsegs = topPgon != null ? new colDXFEntities(topPgon.AdditionalSegments) : null;
            colDXFEntities botaddsegs = botPgon != null ? new colDXFEntities(botPgon.AdditionalSegments) : null;

            colDXFVectors pgverts = colDXFVectors.Zero;
            colDXFEntities addsegs = new colDXFEntities();
            dxePolygon _rVal = new dxePolygon(colDXFVectors.Zero, bClosed: true, aName: $"SECTION_SHAPE_{Handle.Replace(",", "_")}") { LayerName = "GEOMETRY" };
            //======================================== NO SPLCES
            if (topverts.Count <= 0 && botverts.Count <= 0)
            {

                pgverts.Append(Vertices.ToDXFVectors(), bAppendClones: false);
                if (IsHalfMoon)
                    TrimMoonPoints(pgverts);

            }
            //======================================== TWO SPLICES
            else if (topverts.Count > 0 && botverts.Count > 0)
            {
                pgverts.Append(topverts, bAppendClones: false);


                pgverts.Append(botverts, bAppendClones: false);
                if (!IsHalfMoon && !IsRectangular)
                {
                    uopVectors verts = new uopVectors(Vertices.FindAll(x => x.Y > bot.Ordinate + bot.SpliceType.GapValue() && x.Y < top.Ordinate - top.SpliceType.GapValue()));
                    if (verts.Count > 0)
                    {
                        verts.Sort(dxxSortOrders.CounterClockwise, null);
                        pgverts.Append(verts, bAppendClones: false);

                    }
                }

            }
            //======================================== TOP SPLICE ONLY
            else if (topverts.Count > 0 && botverts.Count <= 0)
            {
                if (!IsHalfMoon)
                {
                    botverts = new colDXFVectors(Vertices.FindAll(x => x.Y < top.Ordinate - top.SpliceType.GapValue()));
                    botverts.Sort(dxxSortOrders.LeftToRight);
                    if (botverts.Count <= 1)
                    {
                        botverts.Item(1).VertexRadius = DeckRadius;
                        topverts.Item(1).VertexRadius = 0;
                    }

                }
                else
                {
                    botverts = new colDXFVectors(Vertices.FindAll(x => x.X < top.Ordinate - top.SpliceType.GapValue()));
                    botverts.Sort(dxxSortOrders.TopToBottom);
                }

                pgverts.Append(botverts, bAppendClones: false);
                pgverts.Append(topverts, bAppendClones: false);


            }
            //======================================== BOTTOM SPLICE ONLY
            else if (topverts.Count <= 0 && botverts.Count > 0)
            {
                if (!IsHalfMoon)
                {
                    topverts = new colDXFVectors(Vertices.FindAll(x => x.Y > bot.Ordinate + bot.SpliceType.GapValue()));
                    topverts.Sort(dxxSortOrders.RightToLeft);
                    if (topverts.Count <= 1)
                        botverts.LastVector().VertexRadius = DeckRadius;
                }
                else
                {
                    topverts = new colDXFVectors(Vertices.FindAll(x => x.X > bot.Ordinate + bot.SpliceType.GapValue()));
                    topverts.Sort(dxxSortOrders.BottomToTop);
                }

                pgverts.Append(topverts, bAppendClones: false);
                pgverts.Append(botverts, bAppendClones: false);

            }


            //trim the point


            if (LapsRing && !IsHalfMoon & (top != null || bot != null))
            {
                pgverts.Circularize(DeckRadius, 3, null, bDontSort: true);
            }


            //to collect the additional segments
            _rVal.Vertices.AddRange(pgverts); //, bAddClones: false);
            _rVal.AdditionalSegments.AddRange(topaddsegs);
            _rVal.AdditionalSegments.AddRange(botaddsegs);

            List<uopVector> pts = BPSites.FindAll(x => !x.Suppressed);
            foreach (var p in pts)
                _rVal.AdditionalSegments.Add(new dxeArc(p, mdGlobals.BPRadius, aDisplaySettings: dxfDisplaySettings.Null("BUBBLE_PROMOTERS", dxxColors.ByLayer)) { Tag = "BUBBLE PROMOTER" });

            _rVal.InsertionPt = new dxfVector(X, Y);

            return _rVal;
        }

        public dxePolyline UpdateSimplePerimeter(bool bRegen, uopTrayAssembly aAssy = null, bool bVerbose = false)
        {
            if (bRegen) _SimplePerimeter = null;
            if (_SimplePerimeter != null && !bRegen) return _SimplePerimeter;
           
            _SimplePerimeter = GenerateSimplePerimeter(aAssy, bVerbose);
            base.MechanicalBounds = new USHAPE(_SimplePerimeter.Vertices, "SIMPLE_PERIMETER");
            return _SimplePerimeter;

        }

        internal dxePolyline GenerateSimplePerimeter(uopTrayAssembly aAssy = null, bool bVerbose = false)
        {
         
            this.GetSplices(out uopDeckSplice top, out uopDeckSplice bot);
            //if (SectionAbove != null && SectionAbove.DoubleFemale)
            //{
            //    Console.WriteLine(Handle);
            //}

            
            dxePolygon topPgon = top != null?  top.PerimeterPolygon(true,  aSection: this, bSimple:true) : null;
            dxePolygon botPgon = bot != null ? bot.PerimeterPolygon(true,aSection: this, bSimple: true) : null;


            colDXFVectors topverts = topPgon != null ? new colDXFVectors(topPgon.Vertices) : colDXFVectors.Zero;
            colDXFVectors botverts = botPgon != null ? new colDXFVectors(botPgon.Vertices) : colDXFVectors.Zero;

           
            colDXFVectors pgverts = colDXFVectors.Zero;
      
           
          //======================================== NO SPLCES
            if (topverts.Count <= 0 && botverts.Count <= 0)
            {
              
                pgverts.Append(Vertices.ToDXFVectors(), bAppendClones: false);
                if (IsHalfMoon)
                    TrimMoonPoints(pgverts);

            }
            //======================================== BOTTOM SPLICE ONLY
            else if (topverts.Count > 0 && botverts.Count > 0)
            {
                pgverts.Append(topverts, bAppendClones: false);
             
                    
                pgverts.Append(botverts, bAppendClones: false);
                if (!IsHalfMoon && !IsRectangular)
                {
                    uopVectors verts = new uopVectors(Vertices.FindAll(x => x.Y > bot.Ordinate + bot.SpliceType.GapValue() && x.Y < top.Ordinate - top.SpliceType.GapValue()));
                    if (verts.Count > 0)
                    {
                        verts.Sort(dxxSortOrders.CounterClockwise, null);
                        pgverts.Append(verts, bAppendClones: false);

                    }
                }

            }
            //======================================== TOP SPLICE ONLY
            else if (topverts.Count > 0 && botverts.Count <= 0)
            {
                if (!IsHalfMoon)
                {
                    botverts = new colDXFVectors(Vertices.FindAll(x => x.Y < top.Ordinate - top.SpliceType.GapValue()));
                    botverts.Sort(dxxSortOrders.LeftToRight);
                    if (botverts.Count <= 1)
                    {
                        botverts.Item(1).VertexRadius = DeckRadius;
                        topverts.Item(1).VertexRadius = 0;
                    }
                        
                }
                else
                {
                    botverts = new colDXFVectors(Vertices.FindAll(x => x.X < top.Ordinate - top.SpliceType.GapValue()));
                    botverts.Sort(dxxSortOrders.TopToBottom);
                }

                pgverts.AddRange(botverts);
                pgverts.AddRange(topverts);
        

            }
            //========================================
            else if (topverts.Count <= 0 && botverts.Count > 0)
            {
                if (!IsHalfMoon)
                {
                    topverts = new colDXFVectors(Vertices.FindAll(x => x.Y > bot.Ordinate + bot.SpliceType.GapValue()));
                    topverts.Sort(dxxSortOrders.RightToLeft);
                    if(topverts.Count <=1) 
                        botverts.LastVector().VertexRadius = DeckRadius;
                }
                else
                {
                    topverts = new colDXFVectors(Vertices.FindAll(x => x.X > bot.Ordinate + bot.SpliceType.GapValue() ) ) ;
                    topverts.Sort(dxxSortOrders.BottomToTop);
                }

                pgverts.AddRange(topverts);
                pgverts.AddRange(botverts);

            }


            //trim the point
          

            if (LapsRing && !IsHalfMoon & (top != null || bot !=null))
            {
                pgverts.Circularize(DeckRadius, 3,null, bDontSort:true);
            }

            dxePolyline _rVal = new dxePolyline(pgverts, true) { Tag = Handle};

           

            return _rVal;
        }

        public void GetSplicePolygons(out dxePolygon rTopSplice, out dxePolygon rBottomSplice)
        {
            rTopSplice = null;
            rBottomSplice = null;

            RectifySplices(out uopDeckSplice top, out uopDeckSplice bot);
            
            if(top != null) rTopSplice = top.PerimeterPolygon();
            if (bot != null) rBottomSplice = bot.PerimeterPolygon();

            if (rTopSplice != null && rBottomSplice != null) return;

            uopSegments segs = Segments;
            if(rTopSplice == null)
            {
                List<uopArc> arcs = Segments.ArcSegments(true);

            }

        }
       
        
        private void TrimMoonPoints(colDXFVectors aMoonPolygonVerticies)
        {
            try
            {
                if (aMoonPolygonVerticies == null || aMoonPolygonVerticies.Count < 3) return;
                colDXFVectors pgverts = aMoonPolygonVerticies;
                double clip = mdGlobals.MoonClipLength;
                if (X > 0)
                {
                    dxfVector u1 = pgverts.GetVector(dxxPointFilters.GetTopLeft);
                   
                    dxfVector u2 = u1.Moved(0, -clip);
                    uopLine trimmer = new uopLine(u2, u2.Moved(100, 0));
                    uopVector ip = trimmer.Intersections(Segments, false, true).GetVector(dxxPointFilters.AtMaxX);
                    if (ip != null)
                    {
                        u1.SetCoordinates(u2.X, u2.Y);
                        int idx = pgverts.IndexOf(u1);
                        if (idx == 1) pgverts.Add(ip); else pgverts.Insert(idx-1, new dxfVector(ip));

                    }


                    u1 = pgverts.GetVector(dxxPointFilters.GetBottomLeft);
                    u2 = u1.Moved(0, clip);
                    trimmer = new uopLine(u2, u2.Moved(100, 0));
                    ip = trimmer.Intersections(Segments, false, true).GetVector(dxxPointFilters.AtMaxX);
                    if (ip != null)
                    {
                        u1.SetCoordinates(u2.X, u2.Y);
                        int idx = pgverts.IndexOf(u1);
                        if (idx == 1) pgverts.Add(ip); else pgverts.Insert(idx , new dxfVector(ip));

                    }

                }

            }
            catch (Exception)  { }
        }


        #endregion Methods


        public override uopCompoundShape CreateSubShape(string aName, double aRadiusOffset = 0, double aLeftDowncomerOffset = 0, double aRightDowncomerOffset = 0, double aTopSpliceOffset = 0, double aBottomSpliceOffset = 0, double aLeftEdgeOffset = 0, double aRightEdgeOffset = 0, bool bIncludeQuadrantPoints = true)
        {

            GetSplices(out uopDeckSplice top, out uopDeckSplice bot);

            if (IsHalfMoon)
            {
                if (X >= 0)
                {
                    if (bot != null) aLeftDowncomerOffset = 0;
                    if (top != null) aRightDowncomerOffset = 0;
                }
                else
                {
                    if (bot != null) aRightDowncomerOffset = 0;
                    if (top != null) aLeftDowncomerOffset = 0;

                }
            }

            uopCompoundShape _rVal =  base.CreateSubShape(aName:aName, aRadiusOffset: aRadiusOffset, aLeftDowncomerOffset: aLeftDowncomerOffset,aRightDowncomerOffset: aRightDowncomerOffset,aTopSpliceOffset: aTopSpliceOffset,aBottomSpliceOffset: aBottomSpliceOffset, aLeftEdgeOffset: aLeftEdgeOffset, aRightEdgeOffset: aRightEdgeOffset, bIncludeQuadrantPoints: bIncludeQuadrantPoints);
            
            _rVal.Flag = Handle;
            return _rVal;
        }

        public uopVectors PerimeterPts( bool bIncludeBPSites = false, bool bIncludeSlotSites = false, bool bInvert = false, double aXOffset = 0, double aYOffset = 0)
        {
            uopVectors _rVal = new uopVectors(SimplePerimeter.Vertices);
       
       
            if (bIncludeBPSites && BPSites.Count > 0) _rVal.Append(BPSites, bAddClone:true, aTag:"BUBBLE PROMOTER POINTS");


            if (bIncludeSlotSites && MDDesignFamily.IsEcmdDesignFamily())
            {
                mdSlotZone aZone = SlotZone;
                if (aZone != null) _rVal.Append( aZone.PerimeterPts(MDTrayAssembly), bAddClone:true, aTag: "SLOT POINTS") ;
               
            }
            if (bInvert)
            { _rVal.RotateMove(Center, 180, aXOffset, aYOffset); }
            else
            { _rVal.RotateMove(Center, 0, aXOffset, aYOffset); }
            return _rVal;
        }
        #region Shared Methods
        public static uopSectionShape CloneCopy(uopSectionShape aShape) => aShape == null ? null : new uopSectionShape(aShape);

   
        

        #endregion Shared Methods
    }
}
