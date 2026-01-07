using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UOP.DXFGraphics;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Parts;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using static System.Collections.Specialized.BitVector32;

namespace UOP.WinTray.Projects
{
    public class uopFlangeLine : uopLine, ICloneable
    {

        #region Constructors

        public uopFlangeLine() => Init();

        public uopFlangeLine(iLine aLine = null)  => Init(aLine);
        public uopFlangeLine(double aSPX, double aSPY, double aEPX, double aEPY,  bool bVertical, int aRow = 0, int aCol = 0)
        {
            Init();
            sp = new uopVector(aSPX, aSPY);
            ep = new uopVector(aEPX, aEPY);
            Row = aRow;
            Col = aCol;
            Vertical = bVertical;
        }

        public uopFlangeLine(uopDeckSplice aSplice, bool bOppositeGender = false, uopSectionShape aSection = null, uopTrayAssembly aAssy = null) =>  Init(null, aSplice, bOppositeGender, aSection, aAssy);
         

        internal uopFlangeLine(ULINE aLine)
        {
            Init();
            sp = new uopVector(aLine.sp);
            ep = new uopVector(aLine.ep);

        }
        private void Init(iLine aLine = null, uopDeckSplice aSplice = null, bool bOppositeGender = false, uopSectionShape aSection = null, uopTrayAssembly aAssy = null)
        {
            Row = 0;
            Col = 0;
            Index = 0;
            Suppressed = false;
            Points = uopVectors.Zero;
            TruncatedOnRight = false;
            TruncatedOnLeft = false;
            Vertical = false;
            SpliceType = uppSpliceTypes.Undefined;
            FastenerSpacing = 0;
            FastenerType = uppSpliceFastenerTypes.Bolts;
            FastenerCount = 0;
            IsTop = false;
            LeftDowncomerInfo = null;
            RightDowncomerInfo = null;
            if (aLine != null) Copy(aLine);

            Splice = aSplice;
            uopSectionShape section = aSection == null ? aSplice == null ? null : aSplice.Section : aSection;
            if (section != null)
            {
                LeftDowncomerInfo = DowncomerInfo.CloneCopy(section.LeftDowncomerInfo);
                RightDowncomerInfo = DowncomerInfo.CloneCopy(section.RightDowncomerInfo);
            }
            
            if (aSplice != null) 
            {
                FastenerCount = aSplice.FastenerCount;
                FastenerType = aSplice.FastenerType;
                FastenerSpacing = aSplice.FastenerSpacing;


                //if(FastenerType == uppSpliceFastenerTypes.Bolts)
                //{
                //    Console.WriteLine($"{section.Handle} uses {FastenerType.GetDescription()}");
                //}
                double gap = aSplice.GapValue(); // section == null ? false : section.IsManway);
                var bounds = section != null ? section.BoundsV : new URECTANGLE(MinX, MaxY, MaxX, MinY);
                bool rectangular = section != null ? section.IsRectangular : false;

                double corrY = 0;
                double corrX = 0;
                double f1 = !aSplice.IsTop ? -1 : 1;
                double deckrad = section != null ? section.DeckRadius : aSplice.DeckRadius;
                double trimrad = section != null ? section.TrimRadius : aSplice.TrimRadius;

                double dy = 0;

                if (Vertical)
                {
                    gap = aSplice.GapValue();
                    double x = Math.Abs(aSplice.Ordinate);
                    x -= gap;
                    x += mdGlobals.DeckTabFlangeHeight;
                    bool female = aSplice.Female;
                    if (bOppositeGender)
                        female = !female;
                    if (!female) corrX = -(mdGlobals.DeckTabFlangeHeight + mdGlobals.DeckTabHeight - 2 * gap);

                    double y = Math.Sqrt(Math.Pow(trimrad, 2) - Math.Pow(x, 2));
                    if (aSplice.X < 0) { x *= -1; corrX *= -1; }

                    sp.SetCoordinates(x, y);
                    ep.SetCoordinates(x, -y);

                    this.Rectify(bDoX: false, bInverse: !IsTop);




                }
                else
                {
                    double isetL = aSplice.FlangeInset;
                    if (isetL == 0)
                        isetL = section != null ? section.LeftDowncomerLap + mdGlobals.DefaultSpliceAngleClearance : (mdGlobals.DefaultShelfAngleWidth - mdGlobals.DefaultPanelClearance) + mdGlobals.DefaultSpliceAngleClearance;

                    double isetR = aSplice.FlangeInset;
                    if (isetR == 0)
                        isetR = section != null ? section.RightDowncomerLap + mdGlobals.DefaultSpliceAngleClearance : (mdGlobals.DefaultShelfAngleWidth - mdGlobals.DefaultPanelClearance) + mdGlobals.DefaultSpliceAngleClearance;

                    if (aSplice.RequiresFlange)
                    {
                        if (section != null)
                        {
                            if (LeftDowncomerInfo != null) isetL = section.LeftDowncomerLap + mdGlobals.FormedFlangeDeckGap;
                            if (RightDowncomerInfo != null) isetR = (section.RightDowncomerLap + mdGlobals.FormedFlangeDeckGap);
                        }
                        if (!bOppositeGender)
                            dy = (aSplice.IsTop ? 1 : -1) * (mdGlobals.DeckTabFlangeHeight - gap);
                        else
                            dy = (aSplice.IsTop ? -1 : 1) * (mdGlobals.DeckTabHeight - gap);
                    }
                    else if (aSplice.RequiresTabs)
                    {
                        if (section != null)
                        {
                            if (LeftDowncomerInfo != null) isetL = section.LeftDowncomerLap + mdGlobals.FormedFlangeDeckGap;
                            if (RightDowncomerInfo != null) isetR = (section.RightDowncomerLap + mdGlobals.FormedFlangeDeckGap);
                        }

                        if (!rectangular)
                        {
                            if (!bOppositeGender)
                            {
                                dy = (aSplice.IsTop ? -1 : 1) * (mdGlobals.DeckTabFlangeHeight - gap);
                                corrY = -dy + (aSplice.IsTop ? 1 : -1) * (mdGlobals.DeckTabHeight - gap);
                            }
                            else
                            {

                                dy = (aSplice.IsTop ? -1 : 1) * (mdGlobals.DeckTabFlangeHeight - gap);
                            }

                        }
                        else
                             if (!bOppositeGender)
                            dy = (aSplice.IsTop ? 1 : -1) * (mdGlobals.DeckTabHeight - gap);
                        else
                            dy = (aSplice.IsTop ? -1 : 1) * (mdGlobals.DeckTabFlangeHeight - gap);

                    }

                    sp.SetCoordinates(bounds.Right - isetR, aSplice.Ordinate + dy);
                    ep.SetCoordinates(bounds.Left + isetL, sp.Y);
                    if (!aSplice.IsTop) Invert();
                    TrimWithShape(uopShape.Circle(null, aSplice.TrimRadius), true);
                    if (section != null && section.LapsDivider)
                    {
                        uopLine trimmer = section.Segments.LineSegments(bGetClones: true).Find(x => !x.IsVertical(1) && !x.IsHorizontal(1));
                        if (trimmer != null)
                        {
                            uopVector u1 = trimmer.IntersectionPt(this, false, false);
                            if (u1 != null)
                            {
                                double offset = section == null ? aSplice.RingClearance : section.RingClearance;
                                uopVector mp = MidPt;
                                if (u1.X < mp.X)
                                {
                                    if (sp.X < mp.X) sp.X = u1.X + offset; else ep.X = u1.X - offset;
                                }
                                else
                                {
                                    if (sp.X > mp.X) sp.X = u1.X - offset; else ep.X = u1.X + offset;
                                }
                            }
                        }

                    }
                }


                if (corrX != 0 || corrY != 0)
                    Move(corrX, corrY);


            }

            if (FastenerSpacing > 0) 
                UpdateFastenerPoints(true,aAssy,aSplice);
         



        }


        #endregion Constructors


        #region Properties
        public DowncomerInfo LeftDowncomerInfo { get; set; }
        public DowncomerInfo RightDowncomerInfo { get; set; }

        public List<double> FastenerOrdinates
        {
            get
            {
                if (Points.Count < 0 && FastenerSpacing > 0)
                    UpdateFastenerPoints();

                return Points.Ordinates(bGetY: Vertical, 10);
            }
        }


        private WeakReference<uopDeckSplice> _SpliceRef;
        public uopDeckSplice Splice{
            get
            {
                if (_SpliceRef == null) return null;
                if (!_SpliceRef.TryGetTarget(out uopDeckSplice _rVal)) _SpliceRef = null;
                return _rVal;
            }
            set
            {
                if (value == null) { _SpliceRef = null; return; }
                _SpliceRef = new WeakReference<uopDeckSplice>(value);
                if (value == null) return;
                FastenerSpacing = value.FastenerSpacing;
                Vertical = value.Vertical;
                SpliceType = value.SpliceType;
                FastenerType = value.FastenerType;
                FastenerCount = FastenerType == uppSpliceFastenerTypes.Bolts ? value.SpliceBoltCount : 0;
                IsTop = value.IsTop;

            }
        }
    

        public bool IsTruncated => TruncatedOnLeft || TruncatedOnRight;
        public bool TruncatedOnRight { get; set; }
        public bool TruncatedOnLeft { get; set; }
        public bool Vertical { get; set; }
        public bool IsTop { get; set; }
        public uppSpliceTypes SpliceType { get; set; }
        public uppSpliceFastenerTypes FastenerType { get; set; }
        public int FastenerCount 
        { get; 
            set; 
        }
        public double FastenerSpacing { get; set; }

        public double FastenerHoleInset => SpliceType == uppSpliceTypes.SpliceWithTabs ? mdGlobals.DeckTabSlotInset : 1;

        public double FasternerHoleLength
        {
            get
            {
                return SpliceType == uppSpliceTypes.SpliceWithTabs  ? mdGlobals.DeckTabSlotLength : 0.5;
            }
        }
        public bool LeftToRight => Vertical ? sp.Y < ep.Y : sp.X < ep.X;

        public override uopVectors Points { get { if (base._Points == null || base._Points.Count <= 0) UpdateFastenerPoints(); return base.Points; } set => base.Points = value; }

        public double Ordinate => Vertical ? X() : Y();
        #endregion Properties

        #region Methods

        public new void Copy(iLine aLine)
        {
            if (aLine == null) return;
            base.Copy(aLine);
            if (aLine is uopFlangeLine)
            {
                uopFlangeLine fline = (uopFlangeLine)aLine;
                TruncatedOnLeft = fline.TruncatedOnLeft;
                TruncatedOnRight = fline.TruncatedOnRight;
                Vertical = fline.Vertical;
                FastenerSpacing += fline.FastenerSpacing;
                SpliceType = fline.SpliceType;
                IsTop = fline.IsTop;
                LeftDowncomerInfo = DowncomerInfo.CloneCopy( fline.LeftDowncomerInfo);
                RightDowncomerInfo = DowncomerInfo.CloneCopy(fline.RightDowncomerInfo);
            }
        }

        public new  uopFlangeLine Clone() => new uopFlangeLine(this);

        object ICloneable.Clone() => new uopFlangeLine(this);


        public override string ToString()
        {
            string _rVal = $"uopFlangeLine - {sp.X:0.0000},{sp.Y:0.0000} -> {ep.X:0.0000},{ep.Y:0.0000}";
            if (TruncatedOnLeft) _rVal += $" TRUNC: LEFT";
            if (TruncatedOnRight) _rVal += $" TRUNC: RIGHT";
            return _rVal;

        }

        public void UpdateFastenerPoints(bool bRegen = false, uopTrayAssembly aAssy = null, uopDeckSplice aSplice = null)
        {
            uopVectors pts = base._Points;
            if ((pts == null ||pts.Count <= 0) && FastenerSpacing > 0 || bRegen)
            {
                int? tcount = null;
                //if( FastenerType == uppSpliceFastenerTypes.Bolts && FastenerCount > 0) 
                if (FastenerCount > 0)
                {
                    if (!IsTruncated)
                        tcount = this.FastenerCount;
                    else
                        tcount = null;
                }
                double space = 0;


                if (SpliceType == uppSpliceTypes.SpliceManwayCenter)
                {
                    uopDeckSplice splice = aSplice == null ? Splice : aSplice;
                    aAssy ??= splice.Section?.TrayAssembly;
                    if(splice != null && aAssy != null && aAssy.ProjectFamily == uppProjectFamilies.uopFamMD)
                    {
                        mdTrayAssembly assy = (mdTrayAssembly)aAssy;
                        
                        mdSpliceAngle ang = assy.SpliceAngle(uppSpliceAngleTypes.ManwaySplicePlate,splice.X,splice.Y);
                        uopHoleArray holes = ang.GenHoles();

                        List<double> ords = holes[0].Centers.Ordinates(bGetY: false, aPrecis: 6);
                        
                        foreach (var x in ords)
                            pts.Add(x, sp.Y);
                    }
                    
                    
                }
                else
                {
                    pts = uopUtils.LayoutPointsOnLine(this, out space, FastenerSpacing, bCenterOnLine: true, aEndBuffer: FastenerHoleInset, bAtLeastOne: true, aMinSpace: FasternerHoleLength / 2 + 0.5, bSaveToLine: false, aTargetCount: tcount, bExactSpacing: !IsTruncated);
                }

                if (space != FastenerSpacing)
                {
                    FastenerSpacing = space;
                }

                if (Vertical)
                {
                    pts.Sort(!IsTop ? dxxSortOrders.TopToBottom : dxxSortOrders.BottomToTop);
                } 
                else
                {
                    pts.Sort(!IsTop ? dxxSortOrders.RightToLeft : dxxSortOrders.LeftToRight);
                }
                    base._Points = pts;

               

            }
            else
            {
                double ord = Vertical ? X() : Y();
                foreach (var pt in pts)
                {
                    if (Vertical) pt.X = ord; else pt.Y = ord;
                }
            }

        }
        public  override bool TrimWithShape(uopShape aShape, bool bSetTruncation = false)
        {
            if (aShape == null) return false;

            double length = Length;
            uopVector sp1 = new uopVector(sp);
            uopVector ep1 = new uopVector(ep);
            bool _rVal = base.TrimWithShape(aShape,false);
            if(!_rVal) return _rVal ;
            if (bSetTruncation && _rVal && Length < length)
            {
                if (!sp1.IsEqual(sp, 6))
                {
                    if (LeftToRight) TruncatedOnLeft = true; else TruncatedOnRight = true;
                }
                if (!ep1.IsEqual(ep, 6))
                {
                    if (LeftToRight) TruncatedOnRight = true; else TruncatedOnLeft = true;
                }


            }
      
            return _rVal;

        }

        public bool ExtendTo(iArc aArc, bool bTrimTo = false, bool bExtendTheArc = false, bool aArcIsInfinite = false, bool bSetTruncation = false)
        {
            double length = Length;
            uopVector sp1 = new uopVector(sp);
            uopVector ep1 = new uopVector(ep);

            bool _rVal = base.ExtendTo(aArc, bTrimTo, bExtendTheArc, aArcIsInfinite);
            if (bSetTruncation && _rVal && Length < length)
            {
                if(!sp1.IsEqual(sp, 6))
                {
                    if (LeftToRight) TruncatedOnLeft = true; else TruncatedOnRight = true;
                }
                if (!ep1.IsEqual(ep, 6))
                {
                    if (LeftToRight) TruncatedOnRight = true; else TruncatedOnLeft = true;
                }


            }


            return _rVal;
        }
        public  bool ExtendTo(iLine aLine, bool bTrimTo = false, bool bExtendTheLine = false, bool bSetTruncation = false)
        {

            double length = Length;
            uopVector sp1 = new uopVector(sp);
            uopVector ep1 = new uopVector(ep);

            bool _rVal = base.ExtendTo(aLine, bTrimTo, bExtendTheLine);
            if (bSetTruncation && _rVal && Length < length)
            {
                if (!sp1.IsEqual(sp, 6))
                {
                    if (LeftToRight) TruncatedOnLeft = true; else TruncatedOnRight = true;
                }
                if (!ep1.IsEqual(ep, 6))
                {
                    if (LeftToRight) TruncatedOnRight = true; else TruncatedOnLeft = true;
                }


            }


            return _rVal;
        }
        #endregion Methods

        #region Shared Methods

        public static uopFlangeLine CloneCopy(uopFlangeLine aLine) => aLine == null? null : new uopFlangeLine(aLine); 

        #endregion Shared Methods
    }
}
