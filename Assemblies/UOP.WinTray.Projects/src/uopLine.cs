using UOP.DXFGraphics;
using System;
using System.Collections.Generic;
using UOP.WinTray.Projects.Structures;
using System.Reflection;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Interfaces;
using UOP.WinTray.Projects.Utilities;
using System.Linq;

namespace UOP.WinTray.Projects
{
    public class uopLine : ICloneable, iLine, iSegment
    {

        #region Constructors
        public uopLine() => Init();

        internal uopLine(UVECTOR aSP, UVECTOR aEP, string aTag = null, uppSides aSide = uppSides.Undefined)
        {
            Init();
            sp = new uopVector(aSP);
            ep = new uopVector(aEP);

            if (aTag != null) Tag = aTag;
            Side = aSide;
        
        }

        public uopLine(iVector aSP, iVector aEP, string aTag = null, uppSides aSide = uppSides.Undefined)
        {
            Init();
            sp = new uopVector(aSP);
            ep = new uopVector(aEP);
            Side = aSide;
             if (aTag != null) Tag = aTag;
            Side = aSide;
       
        }

        public uopLine(double aSPX, double aSPY, double aEPX, double aEPY, int aRow = 0, int aCol = 0)
        {
            Init();
            sp = new uopVector(aSPX, aSPY);
            ep = new uopVector(aEPX, aEPY);
            Row = aRow;
            Col = aCol;
         
        }

        internal uopLine(ULINE aLine)
        {
            Init();
            Copy(aLine);
        }
       
        public uopLine(iLine aLine) => Init(aLine);
            
     

        protected void Init(iLine aLine = null)
        {
            sp = uopVector.Zero;
            ep = uopVector.Zero;
            Row = 0;
            Col = 0;
            Index = 0;
            Points = new uopVectors();
            Suppressed = false;
            Side = uppSides.Undefined;
            if (aLine != null)
                Copy(aLine);

        }

        #endregion Constructors

        #region Properties

        private uopVector _sp;
        public uopVector sp { get { _sp ??= uopVector.Zero; return _sp; } set { _sp = value ?? uopVector.Zero; } }
        private uopVector _ep;
        public uopVector ep { get { _ep ??= uopVector.Zero; return _ep; } set { _ep = value ?? uopVector.Zero; } }
        public int Row { get; set; }
        public int Col { get; set; }

        public bool Suppressed { get; set; }

        public uppSides Side { get; set; }
        public int Index { get; set; }

        protected uopVectors _Points;
        public virtual uopVectors Points { get { _Points ??= new uopVectors(); return _Points; } set { _Points = value ?? uopVectors.Zero; } }

        public uopVector Direction { get => (ep - sp).Normalized(); }

        public double Value { get => sp.Value; set => sp.Value = value; }

        public string Tag { get => sp.Tag; set => sp.Tag = value; }

        public string Flag { get => sp.Flag; set => sp.Flag = value; }

        public double DeltaY => ep.Y - sp.Y;

        public double DeltaX => ep.X - sp.X;

        public double Slope => DeltaX != 0 ? DeltaY / DeltaX : Double.MaxValue;

        /// <summary>
        /// returns the angle from the world X axis to the line (0 to 360)
        /// </summary>
        public double AngleOfInclination
        {
            get
            {
                double dx = ep.X - sp.X;
                double dy = ep.Y - sp.Y;
                if (dx == 0 & dy == 0) return 0;
                if (dx == 0) return dy > 0 ? 90 : 270;
                if (dy == 0) return dx > 0 ? 0 : 180;
                double ang = Math.Atan(Math.Abs(dx / dy)) * 180 / Math.PI;
                if (dx > 0 && dy > 0) return dy > 0 ? ang : 360 - ang;
                if (dx < 0 && dy > 0) return dy > 0 ? 180 - ang : 180 + ang;
                return ang;
            }
        }
        public double Length => sp.DistanceTo(ep, 4);
        public uopVector MidPt => sp.MidPt(ep);

        internal ULINE Structure
        {
            get => new ULINE(this);
            set
            {
                Side = value.Side;
                sp = new uopVector(value.sp);
                ep = new uopVector(value.ep);
                Row = value.Row;
                Col = value.Col;
                Index = value.Index;
                Points = new uopVectors(value.Points);

            }
        }
        public  double MaxY => Math.Max(ep.Y, sp.Y);
        public double MinY => Math.Min(ep.Y, sp.Y);
        public double MaxX => Math.Max(ep.X, sp.X);
        public double MinX => Math.Min(ep.X, sp.X);

        internal URECTANGLE Limits => new URECTANGLE(MinX, MaxY, MaxX, MinY);
        public uopRectangle Bounds => new uopRectangle(MinX, MaxY, MaxX, MinY);

        /// <summary>
        ///  a  delimited string with the objects Tag and Flag
        /// </summary>
        public string Handle => uopUtils.Handle(Tag, Flag);

        #endregion Properties

        #region Methods



        public virtual double MaxYr(int? aPrecis = null) => uopUtils.MaxValue(sp.Y, ep.Y, aPrecis);
        public virtual double MinYr(int? aPrecis = null) => uopUtils.MinValue(sp.Y, ep.Y, aPrecis);
        public virtual double MaxXr(int? aPrecis = null) => uopUtils.MaxValue(sp.X, ep.X, aPrecis);
        public virtual double MinXr(int? aPrecis = null) => uopUtils.MinValue(sp.X, ep.X, aPrecis);


        public void SetCoordinates(double? aSPX = null, double? aSPY = null, double? aEPX = null, double? aEPY = null)
        {
            if(aSPX.HasValue) sp.X = aSPX.Value;
            if (aSPY.HasValue) sp.Y = aSPY.Value;
            if (aEPX.HasValue) ep.X = aEPX.Value;
            if (aEPY.HasValue) ep.Y = aEPY.Value;

        }

        public virtual bool TrimWithShape(uopShape aShape, bool bExtendTo = false)
        {
            if(aShape == null) return false;
            bool _rVal = false;
            //aShape.Segments = null;
            uopSegments segs = aShape.Segments;
            ULINE uline = new ULINE(this.sp,this.ep);
            foreach (var seg in segs)
            {
                if (seg.IsArc)
                {
                    uopArc arc = (uopArc)seg;
                    UARC uarc = new UARC(new UVECTOR(arc.Center), aRadius: arc.Radius, arc.StartAngle,arc.EndAngle);
                  
                    uopVectors ipts = new uopVectors(uline.Intersections(uarc, aArcIsInfinite: false, aLineIsInfinite: false));
                    if (ipts.Count <= 0)continue;

                    foreach (var ip in ipts)
                    {
                        bool onMe = ContainsVector(ip, aPrecis: 3, bTreatAsInfinite: false);
                        if (!onMe && !bExtendTo) continue;
                        double d1 = sp.DistanceTo(ip);
                        double d2 = ep.DistanceTo(ip);

                        if (d1 <= d2)
                        {
                            if (sp.SetCoordinates(ip.X, ip.Y)) _rVal = true;
                        }

                        else
                        {
                            if (ep.SetCoordinates(ip.X, ip.Y)) _rVal = true;
                        }

                    }
                        
                    

                }
                else
                {
                    uopVector ip = IntersectionPt((uopLine)seg, out _, out _, out bool onMe, out bool onHim, out bool exists);
                    if (!exists) continue;
                    if (!onMe && !bExtendTo) continue;
                    double d1 = sp.DistanceTo(ip);
                    double d2 = ep.DistanceTo(ip);

                    if (d1 <= d2)
                    {
                        if (sp.SetCoordinates(ip.X, ip.Y)) _rVal = true;
                    }

                    else
                    {
                        if (ep.SetCoordinates(ip.X, ip.Y)) _rVal = true;
                    }

                }

                
                
            }
                return _rVal;

        }

        /// <summary>
        /// extends this line to the passed arc
        /// </summary>
        /// <param name="aArc">the subject arc</param>
        /// <param name="bTrimTo">if the lines intersect and this flag is true the line will be trimmed back to the passed line  </param>
        /// <param name="bExtendTheArc">if this flag is true and the passed arc is a uopArc the process will also be applied to the passed arc  </param>
        /// <returns></returns>
        public virtual bool ExtendTo(iArc aArc, bool bTrimTo = false, bool bExtendTheArc = false, bool aArcIsInfinite = false)
        {

            if (aArc == null) return false;
            UARC uarc = new UARC(new UVECTOR( aArc.Center),aRadius: aArc.Radius, aArcIsInfinite ? 0: aArc.StartAngle, aArcIsInfinite ? 360 : aArc.EndAngle);
            ULINE line = new ULINE(this);
            uopVectors ipts = new uopVectors( line.Intersections(uarc, aArcIsInfinite: aArcIsInfinite, aLineIsInfinite: true));
            if (ipts.Count <=0) return false;
            bool _rVal = false;
            foreach(var ip in ipts)
            {
                bool onMe = ContainsVector(ip,aPrecis:3, bTreatAsInfinite:false)  ;
                double d1 = sp.DistanceTo(ip);
                double d2 = ep.DistanceTo(ip);
              
                if (!onMe || onMe && bTrimTo)
                {
                    if (d1 <= d2)
                        _rVal = sp.SetCoordinates(ip.X, ip.Y);
                    else
                        _rVal = ep.SetCoordinates(ip.X, ip.Y);

                }



                //if (bExtendTheLine && aLine.GetType() == typeof(uopLine))
                //{
                //    uopLine daline = (uopLine)aLine;
                //    d1 = daline.sp.DistanceTo(ip);
                //    d2 = daline.ep.DistanceTo(ip);
                //    if (d1 <= d2)
                //    { if (daline.sp.SetCoordinates(ip.X, ip.Y)) _rVal = true; }
                //    else
                //    { if (daline.ep.SetCoordinates(ip.X, ip.Y)) _rVal = true; }
                //}
            }
         


            return _rVal;
        }
        /// <summary>
        /// extends this line to the passed line
        /// </summary>
        /// <param name="aLine">the subject line</param>
        /// <param name="bTrimTo">if the lines intersect and this flag is true the line will be trimmed back to the passed line  </param>
        /// <param name="bExtendTheLine">if this flag is true and the passed line is a uopLIne the process will also be applied to the passed line  </param>
        /// <returns></returns>
        public virtual bool ExtendTo(iLine aLine, bool bTrimTo = false, bool bExtendTheLine = false)
        {

            if(aLine == null) return false;
            uopVector ip = IntersectionPt(new uopLine(aLine), out _, out _, out bool onMe, out bool onHim, out bool exists);
            if (!exists) return false;
            double d1 = sp.DistanceTo(ip);
            double d2 = ep.DistanceTo(ip);
            bool _rVal = false;
            if (!onMe ||  onMe && bTrimTo)
            {
                if (d1 <= d2)
                    _rVal = sp.SetCoordinates(ip.X, ip.Y);
                else
                    _rVal = ep.SetCoordinates(ip.X, ip.Y);

            }



            if (bExtendTheLine && aLine.GetType() == typeof(uopLine))
            {
                uopLine daline = (uopLine) aLine;
                d1 = daline.sp.DistanceTo(ip);
                d2 = daline.ep.DistanceTo(ip);
                if (d1 <= d2)
                { if (daline.sp.SetCoordinates(ip.X, ip.Y)) _rVal = true; }
                else
                { if (daline.ep.SetCoordinates(ip.X, ip.Y)) _rVal = true; }
            }


            return _rVal;
        }


        /// <summary>
        /// extends this line to the passed line
        /// </summary>
        /// <param name="aLine">the subject line</param>
        /// <param name="bTrimTo">if the lines intersect and this flag is true the line will be trimmed back to the passed line  </param>
        /// <returns></returns>
        internal bool ExtendTo(ULINE aLine, bool bTrimTo = false)
        {

            uopVector ip = IntersectionPt(new uopLine(aLine), out _, out _, out bool onMe, out bool onHim, out bool exists);
            if (!exists) return false;
            double d1 = sp.DistanceTo(ip);
            double d2 = ep.DistanceTo(ip);

            if (onMe && !bTrimTo) return false;

            if (d1 <= d2)
            {
                sp.SetCoordinates(ip.X, ip.Y);
                return d1 != 0;
            }

            else
            {
                ep.SetCoordinates(ip.X, ip.Y);
                return d2 != 0;
            }

        }

        public uopVector HandlePoint(uppSegmentPoints aPointType, bool bGetClone = false)
        {
            switch (aPointType)
            {
                case uppSegmentPoints.StartPt:
                    {
                        return bGetClone ? new uopVector(sp) : sp;
                    }
                case uppSegmentPoints.EndPt:
                    {

                        return bGetClone ? new uopVector(ep) :ep;
                    }
                case uppSegmentPoints.MidPt:
                    {
                        return MidPt;
                    }
                default:
                    return null;
            }
        }

        /// <summary>
        /// extends this line to the passed lines
        /// </summary>
        /// <param name="aLines">the subject lines</param>
        /// <param name="bTrimTo">if the lines intersect and this flag is true the line will be trimmed back to the passed line  </param>
        /// <returns></returns>

        public bool ExtendTo(uopLinePair aLines, bool bTrimTo = false, bool bExtendTheLine = false)
        {
            if(aLines == null) return false;
            bool t1 = aLines.Line1 != null ? ExtendTo(aLines.Line1, bTrimTo, bExtendTheLine) : false;
            bool t2 = aLines.Line2 != null ? ExtendTo(aLines.Line2, bTrimTo, bExtendTheLine) : false;
            return (t1 || t2);
        }


        /// <summary>
        /// extends this line to the passed lines
        /// </summary>
        /// <param name="aLines">the subject lines</param>
        /// <param name="bTrimTo">if the lines intersect and this flag is true the line will be trimmed back to the passed line  </param>
        /// <returns></returns>

        internal bool ExtendTo(ULINEPAIR aLines, bool bTrimTo = false)
        {
            bool t1 = aLines.Line1.HasValue ? ExtendTo(aLines.Line1.Value, bTrimTo) : false;
            bool t2 = aLines.Line2.HasValue ? ExtendTo(aLines.Line2.Value, bTrimTo) : false;
            return (t1 || t2);
        }

        public double X(bool bEndPt = false, int? aPrecis = null) => aPrecis == null ? !bEndPt ? sp.X : ep.X : !bEndPt ? Math.Round(sp.X,aPrecis.Value) : Math.Round(ep.X, aPrecis.Value);
        public double Y(bool bEndPt = false, int? aPrecis = null)  => aPrecis == null ? !bEndPt ? sp.Y : ep.Y : !bEndPt ? Math.Round(sp.Y,aPrecis.Value) : Math.Round(ep.Y, aPrecis.Value);

        public bool IsHorizontal(int aPrecis = 4) => Math.Round(DeltaY, mzUtils.LimitedValue(aPrecis, 1, 15)) == 0;
        public bool IsVertical(int aPrecis = 4) => Math.Round(DeltaX, mzUtils.LimitedValue(aPrecis, 1, 15)) == 0;

        object ICloneable.Clone() => Clone();

        public uopLine Clone() => new uopLine(this);

        internal void Copy(ULINE aLine)
        {
            Side = aLine.Side;
            Suppressed = aLine.Suppressed;
            sp = new uopVector(aLine.sp);
            ep = new uopVector(aLine.ep);
            Row = aLine.Row;
            Col = aLine.Col;
            Index = aLine.Index;
            Points = new uopVectors(aLine.Points);
        }


        public virtual void Copy(iLine aLine)
        {
            if (aLine == null) return;
            sp = new uopVector(aLine.StartPt);
            ep = new uopVector(aLine.EndPt);

            if (aLine is uopLine)
            {
                uopLine uline = (uopLine)aLine;
                Side = uline.Side;
                Row = uline.Row;
                Col = uline.Col;
                Index = uline.Index;
                Suppressed = uline.Suppressed;
                Tag = uline.Tag;
                Flag = uline.Flag;
                _Points = new uopVectors(uline._Points, bCloneMembers: true);
            }
            else if (aLine is dxeLine)
            {
                dxeLine uline = (dxeLine)aLine;

                Row = uline.Row;
                Col = uline.Col;
                Index = uline.Index;
                Tag = uline.Tag;
                Flag = uline.Flag;
            }
        }
        public override string ToString()
        {
            string _rVal = $"uopLine : {sp.X:0.00##},{sp.Y:0.00##} -> {ep.X:0.00##},{ep.Y:0.00##}";
            if (Row != 0) _rVal += $" Row: {Row}";
            if (Col != 0) _rVal += $" Col: {Col}";
            _rVal += $" {uopUtils.Handle(Tag, Flag, "<>")}";
            return _rVal.Trim();

        }

        public double PointValue(int aIndex = 1)
        { return (Points.Count > 0) ? Points.Item(aIndex).Value : 0; }


        public uopVectors GetPoints(Predicate<uopVector> match, bool bGetClones = false)
        {
            if (Points.Count <= 0) return uopVectors.Zero;
            List<uopVector> _rVal = Points.FindAll(match);
           return new uopVectors(_rVal, bCloneMembers: bGetClones);
           
        }

        public int PointCount(bool? bSuppressedVal = null)
        {
            if(!bSuppressedVal.HasValue) return Points.Count;
            return Points.FindAll((x) => x.Suppressed == bSuppressedVal.Value).Count;
        }

        public uopVector GetDirection(out bool rDirectionisNull, double aRotation = 0)
        { 
            uopVector _rVal = (ep - sp).Normalized(out rDirectionisNull);
            if(aRotation != 0) _rVal.Rotate(aRotation);
            return _rVal;
        }

        public uopVector GetDirection(out bool rDirectionisNull, out double rDistance) => (ep - sp).Normalized(out rDirectionisNull, out rDistance);

     

        public void Mirror(double? aX, double? aY)
        {
            if (!aX.HasValue && !aY.HasValue) return;
            sp.Mirror(aX, aY);
            ep.Mirror(aX, aY);
            Points.Mirror(aX, aY);
        }

        public uopLine Mirrored(double? aX, double? aY)
        {
            uopLine _rVal = new uopLine(this);
            _rVal.Mirror(aX, aY);
            return _rVal;
        }
        public dxeLine ToDXFLine(dxfDisplaySettings aDisplay = null) => new dxeLine(sp.ToDXFVector(), ep.ToDXFVector()) { Row = Row, Col = Col, DisplaySettings = aDisplay };

        internal bool ContainsVector(UVECTOR aVector, int aPrecis = 5, bool bTreatAsInfinite = false)
       => ContainsVector( new uopVector(aVector), aPrecis, out _, out _, out _, bTreatAsInfinite);

        public bool ContainsVector(iVector aVector, int aPrecis = 5, bool bTreatAsInfinite = false)
         => aVector == null ? false : ContainsVector(aVector, aPrecis, out _, out _, out _, bTreatAsInfinite);


        internal bool ContainsVector(iVector aVector, int aPrecis, out bool rIsStartPt, out bool rIsEndPt, out bool rWithin, bool bTreatAsInfinite = false)
        {

            rIsStartPt = false;
            rIsEndPt = false;
            rWithin = false;
            if (aVector == null) return false;
                double t1 = Math.Round(GetTValue(aVector, out rWithin), aPrecis);
            if (!rWithin) return false; // the point does not lie on the lines path or the line has ne length
            rIsStartPt = t1 == 0;
            rIsEndPt = t1 == 1;
            return bTreatAsInfinite || (t1 >= 0 && t1 <= 1);

        }

        internal uopVector IntersectionPt(ULINE bLine)
        { 
            UVECTOR ip =  Structure.IntersectionPt(bLine, out bool PRL, out bool COINC, out bool ON1, out bool ON2, out bool EXST, out double T1, out double T2);
            return EXST ? new uopVector(ip) : null;
        }
        /// <summary>
        /// returns the intersection vector of this line and the passed line
        /// </summary>
        /// <param name="aLine">1the Line to find intersections for</param>
        /// <param name="thisLine_IsInfinite">Flag to treat this line as infinite</param>
        /// <param name="aLine_IsInfinite">flag to treat the passed line as infinite</param>
        /// <returns></returns>
        public uopVector IntersectionPt(iLine aLine, bool thisLine_IsInfinite = true, bool aLine_IsInfinite = true)
        {
            if (aLine == null) return null;
            uopVector _rVal = IntersectionPt(aLine, out _, out _, out bool onme, out bool onher, out bool exists);
            if (!exists) return null;
            if (!thisLine_IsInfinite && !onme) return null;
            if (!aLine_IsInfinite && !onher) return null;
            return _rVal;
        }

        public uopVector IntersectionPt(iLine bLine)
        => IntersectionPt(bLine, out bool PRL, out bool COINC, out bool ON1, out bool ON2, out bool EXST, out double T1, out double T2); 

        public uopVector IntersectionPt(iLine bLine, out bool rLinesAreParallel, out bool rLinesAreCoincident, out bool rIsOnFirstLine, out bool rIsOnSecondLine, out bool rInterceptExists)
         =>  IntersectionPt(bLine, out rLinesAreParallel, out rLinesAreCoincident, out rIsOnFirstLine, out rIsOnSecondLine, out rInterceptExists, out double T1, out double T2); 

        public uopVector IntersectionPt(iLine bLine, out bool rLinesAreParallel, out bool rLinesAreCoincident, out bool rIsOnFirstLine, out bool rIsOnSecondLine, out bool rInterceptExists, out double rT1, out double rT2)
        {

            uopVector _rVal =null;
            rInterceptExists = false;
            rLinesAreParallel = false;
            rLinesAreCoincident = false;
            rIsOnFirstLine = false;
            rIsOnSecondLine = false;
            rT1 = -9999;
            rT2 = -9999;
            if (bLine == null) return null;
            uopLine uline = uopLine.FromLine( bLine);
            // get this lines direction and make sure it has a length
            uopVector aDir = this.GetDirection(out bool thisIsNULL);
            // get the passed lines direction and make sure it has a length
            uopVector bDir = uline.GetDirection(out bool sheIsNULL);
            if (thisIsNULL || sheIsNULL) return null;

            //see if they are parallel
            bool bParel = uopVectors.AreEqual(aDir, bDir, bCompareInverse: true, 6, out bool bInvsPar);
            rLinesAreParallel = bParel || bInvsPar;

            //get the shortest line between the two lines
            uopLine shortestConnector = uopLine.ShortestConnector(this, bLine, out _, out _);
            rLinesAreCoincident = shortestConnector == null;

            if (!rLinesAreCoincident)
            {
                double f1 = Math.Round(shortestConnector.Length, 6);
                rInterceptExists = f1 == 0;
                _rVal = new uopVector(shortestConnector.sp);
                rT1 = GetTValue(_rVal);
                rT2 = uline.GetTValue(_rVal);
                if (rInterceptExists)
                {
                    rIsOnFirstLine = rT1 >= 0 & rT1 <= 1;
                    rIsOnSecondLine = rT2 >= 0 & rT2 <= 1;
                }
            }
            return _rVal;
        }

        internal UVECTORS Intersections(UARC aArc, bool aArcIsInfinite = false, bool aLineIsInfinite = false) => Structure.Intersections(aArc, aArcIsInfinite, aLineIsInfinite);

        public uopVectors Intersections(uopArc aArc, bool aArcIsInfinite = false, bool aLineIsInfinite = false) => aArc == null ? new uopVectors() : new uopVectors(Structure.Intersections(aArc.Structure, aArcIsInfinite, aLineIsInfinite));

        internal UVECTORS Intersections(USEGMENT aSegment, bool aSegIsInfinite = false, bool aLineIsInfinite = false)
        {
            if (aSegment.IsArc)
            {
                return Structure.Intersections(aSegment.ArcSeg, aSegIsInfinite, aLineIsInfinite);
            }
            else
            {


                uopVector u1 = IntersectionPt(new uopLine(aSegment.LineSeg), out bool PAR, out bool COINC, out bool ON1, out bool ON2, out bool EXST);
                if (!EXST) return UVECTORS.Zero;
                if (aLineIsInfinite) ON1 = true;
                if (aSegIsInfinite) ON2 = true;

                return (ON1 && ON2) ? new UVECTORS(new UVECTOR(u1)) : UVECTORS.Zero;

            }
        }

        public uopVectors Intersections(iSegment aSegment, bool aSegIsInfinite = false, bool aLineIsInfinite = false)
        {
            if (aSegment.IsArc)
            {
                return Intersections( aSegment.Arc, aSegIsInfinite, aLineIsInfinite);
            }
            else
            {


                uopVector u1 = IntersectionPt(new uopLine(aSegment.Line), out _, out _, out bool ON1, out bool ON2, out bool EXST);
                if (!EXST) return uopVectors.Zero;
                if (aLineIsInfinite) ON1 = true;
                if (aSegIsInfinite) ON2 = true;

                return (ON1 && ON2) ? new uopVectors(new uopVectors(u1)) : uopVectors.Zero;

            }
        }

        public uopVectors Intersections(IEnumerable<iSegment> aSegments, bool aSegsAreInfinite = false, bool aLineIsInfinite = false, bool bNoDupes = false, int aNoDupesPrecis = 4)
        {
            uopVectors _rVal = new uopVectors();
            foreach (iSegment aSegment in aSegments)
            {
                if (aSegment == null) continue;
                USEGMENT seg = new USEGMENT(aSegment);
                UVECTORS ips = Intersections(seg, aSegsAreInfinite, aLineIsInfinite);
                _rVal.Append(ips, bNoDupes: bNoDupes,aNoDupesPrecis:aNoDupesPrecis);
            }
            return _rVal;
        }

        public uopVectors Intersections(List<uopLine> aLines, bool aLinesAreInfinite = false, bool aLineIsInfinite = false, bool  bNoDupes = false, int aNoDupesPrecis = 4)
        {
            uopVectors _rVal = new uopVectors();
            foreach (iLine aSegment in aLines)
            {
                if (aSegment == null) continue;
                uopLine intersect = uopLine.FromLine(aSegment);
                uopVector ip = IntersectionPt(intersect, out _, out _,out bool onMe, out bool onHim, out bool exists);
                if(!exists) continue;
                if (!onMe && !aLineIsInfinite) continue;
                if (!onHim && !aLinesAreInfinite) continue;

                _rVal.Add(ip, bNoDupes: bNoDupes,aNoDupesPrecis:aNoDupesPrecis);
            }
            return _rVal;
        }


        public uopVectors Intersections(uopLinePair aLinePair, bool aPairIsInfinite = false, bool aLineIsInfinite = false, int? aIndex = null)
        {
            if (aLinePair == null) return uopVectors.Zero;
            return aLinePair.Intersections(this, aPairIsInfinite,aLineIsInfinite,aIndex);

        }
        public uopVectors Intersections(uopLinePair aLinePair, uppSides aSide,  bool aPairIsInfinite = false, bool aLineIsInfinite = false)
        {
            if (aLinePair == null) return uopVectors.Zero;
            return aLinePair.Intersections(this, aSide,aPairIsInfinite, aLineIsInfinite);

        }
        public dxeLine ToDXFLineEX(dxeLine aExistingLine = null, string aLayerName = "", dxxColors aColor = dxxColors.Undefined, string aLinetype = "")
        {
            dxeLine _rVal;
            if (aExistingLine != null)
            { _rVal = aExistingLine; }
            else
            { _rVal = new dxeLine(); }

            _rVal.SetCoordinates2D(sp.X, sp.Y, ep.X, ep.Y);
            _rVal.LCLSet(aLayerName, aColor, aLinetype);
            return _rVal;
        }

        public bool Paramatize(out uopVector rSP, out uopVector rDir)
        {
            rSP =new uopVector( sp);
            rDir = Direction;
            return !rDir.IsNull(8);
        }

        internal double GetTValue(UVECTOR aLineVector) => GetTValue(new uopVector(aLineVector), out _);

        public double GetTValue(uopVector aLineVector) => GetTValue(aLineVector, out _);
        public uopLine Moved(double aX = 0, double aY = 0,bool bMovePoints = true)
        {
            uopLine _rVal = Clone();
            _rVal.Move(aX, aY, bMovePoints);
            return _rVal;
        }

        public void Move(double aX = 0, double aY = 0, bool bMovePoints = true)
        {
            sp.X += aX; ep.X += aX; sp.Y += aY; ep.Y += aY;
            if(bMovePoints) Points.Move(aX, aY);
        }

        public uopLine MovedOrtho(double aDistance, bool bInvert = false) => MovedOrtho(aDistance, out _,bInvert);

        public uopLine MovedOrtho(double aDistance, out bool rLineIsNull, bool bInvert = false)
        {
            uopLine _rVal = Clone();
            _rVal.MoveOrtho(aDistance, out rLineIsNull,bInvert);
            return _rVal;
        }

        public void MoveOrtho(double aDistance, bool bInvert = false) => MoveOrtho(aDistance, out _,bInvert);

        public void MoveOrtho(double aDistance, out bool rLineIsNull, bool bInvert = false)
        {

            uopVector aDir = GetDirection(out rLineIsNull);

            if (!rLineIsNull)
            {
                aDir.Rotate(!bInvert ?90 : -90);
                sp += aDir * aDistance;
                ep += aDir * aDistance;
                Points.Project(aDir, aDistance, bSuppressNormalize: true, bInvertDirection: false);
            }

        }
        public void Rotate(iVector aOrigin, double aAngle, bool bInRadians = false)
        {
            sp.Rotate(aOrigin, aAngle, bInRadians);
            ep.Rotate(aOrigin, aAngle, bInRadians);
        }

        /// <summary>
        ///offsets the line othogonally
        /// </summary>
        /// <param name="aDistance">the distance to offset the line</param>
        /// <param name="aOffsetPoint">the point used to determine which direction/side to offset the line</param>
        public void Offset(double aDistance, iVector aOffsetPoint)
        {
            if (Length == 0 || aDistance == 0) return;
            if (aOffsetPoint == null)
            {
                MoveOrtho(aDistance);
                return;
            }
            uopVector testpt = new uopVector(aOffsetPoint);
            uopVector dir1 = Direction;
            uopVector projpt = new uopVector(testpt);
            projpt.ProjectTo(this, out double d1);
            if(d1 <= 0)
            {
                MoveOrtho(aDistance);
                return;
            }
            uopVector dir2 = projpt.DirectionTo(testpt)  ;
            sp += dir2 * aDistance;
            ep += dir2 * aDistance;
            Points.Project(dir2, aDistance, bSuppressNormalize: true, bInvertDirection: false);


        }

        /// <summary>
        ///offsets the line othogonally
        /// </summary>
        /// <param name="aDistance">the distance to offset the line</param>
        /// <param name="aDirection">the direction to project the line</param>
        public void Offset(iVector aDirection,double aDistance )
        {
            if (Length == 0 || aDistance == 0 || aDirection== null) return;
            uopVector dir1 = new uopVector(aDirection);
            dir1.Normalize( out bool isnull);
            if (isnull) return;
            
            sp += dir1 * aDistance;
            ep += dir1 * aDistance;
            Points.Project(dir1, aDistance, bSuppressNormalize: true, bInvertDirection: false);


        }


        public uopLine Projected(uopVector aDirection, double aDistance, bool bSuppressNormalization = false)
        {
            uopLine _rVal = Clone();
            _rVal.Project(aDirection, aDistance, bSuppressNormalization);
            return _rVal;
        }

        public void Project(uopVector aDirection, double aDistance, bool bSuppressNormalization = false)
        {
            if (aDirection == null) return;
            uopVector d1 = aDirection.Clone();

            if (!bSuppressNormalization) { d1.Normalize(); }
            sp.Project(d1, aDistance, true);
            ep.Project(d1, aDistance, true);
            Points.Project(d1, aDistance, bSuppressNormalize: true, bInvertDirection: false);
        }

        public uopLine Inverse() { uopLine _rVal = new uopLine(this); _rVal.Invert(); return _rVal; }

        public double GetTValue(iVector aLineVector, out bool rIsOnLine)
        {
            double _rVal = 0;
            rIsOnLine = false;
            if (aLineVector == null) return _rVal;

            //get this lines direction and length; bail out if if there is no length
            uopVector aDir = GetDirection(out bool aFlg, out double d1);
            if (aFlg) return 0;

            //get the direction and distance from this lines start point to the passed point
            uopVector bDir = sp.DirectionTo(aLineVector, bReturnInverse: false, out aFlg, out double d2);

            //if the direction is null the point is this lines start point so return tvalue of 0 
            if (aFlg) return 0;

            //if the direction equal to this lines direction the point lies on the ine
            rIsOnLine = uopVectors.AreEqual(aDir, bDir, bCompareInverse: true, aPrecis: 2, out aFlg);


            // return the fraction of this lines length that it's start point must be projected
            // this lines direction to land on the the perpendicular intersect of the passed points
            // connecting line onto the line
            _rVal = aFlg ? -d2 / d1 : d2 / d1;

            // negative values indicate the point is off the line in the inverse direction of the line
            // positive values greater than 1 mean the point is off the line in the direction of the line
            // 0 means the point is coincident with the start pt of the line
            // 1 means the point is coincident with end pt of the line
            return _rVal;
        }

        /// <summary>
        /// swaps the end point vectors by the ordinates. i.e. if EP.Y > SP.Y switch them.
        /// </summary>
        public bool Rectify(bool bDoX = false, bool bInverse = false)
        {

            bool doIt;

            if (bDoX)
            {
                doIt = (!bInverse && ep.X > sp.X) || (bInverse && ep.X < sp.X);
            }
            else
            {
                doIt = (!bInverse && ep.Y > sp.Y) || (bInverse && ep.Y < sp.Y);


            }
            if (!doIt) return false;

            Invert();
            
            return true;

        }

        /// <summary>
        /// swaps the start point and end point coordinates
        /// </summary>
        public void Invert(bool bInvertPoints = false)
        {
            if (bInvertPoints && Points.Count > 0)
            {
                uopVectors mypoints = Points;
                uopVector mpt = MidPt;
                uopLine mirrline = new uopLine(mpt, mpt + Direction * 10);
                mirrline.Rotate(mpt, 90);
                uopVectors pts = uopVectors.Zero;
                foreach(var pt in mypoints)
                {
                    pts.Add(pt.Mirrored(mirrline));
                }
                Points = pts;
            }

            UVECTOR v1 = new UVECTOR(ep);
            ep.SetCoordinates(sp.X, sp.Y, sp.Elevation);
            sp.SetCoordinates(v1.X, v1.Y, v1.Elevation);
           
        }

        /// <summary>
        ///#1the subject line
        ///#2the structure of the plane
        ///^returns the coordinates of the passed vector with respect to the center and origin of the passed plane
        /// </summary>
        /// <param name="aLine"></param>
        /// <param name="aPlane"></param>
        /// <param name="aScaler"></param>
        /// <returns></returns>
        public uopLine WithRespectToPlane(dxfPlane aPlane, double aScaler = 1)
        {
            uopLine _rVal = Clone();
            _rVal.sp = sp.WithRespectToPlane(aPlane, aScaler);
            _rVal.ep = ep.WithRespectToPlane(aPlane, aScaler);
            return _rVal;
        }

        public uopVectors ArcIntersection(uopVector aArcCenter, double aRadius, bool bInfiniteLine = false)
        {
            uopVectors _rVal = uopVectors.Zero;
            aArcCenter ??= uopVector.Zero;



            double rad = Math.Round(aRadius, 15);
            if (rad <= 0) return _rVal;//null arc

            uopVector aDir = ep.DirectionTo(sp, false, out bool aFlg, out double dst);
            if (aFlg) return _rVal;


            uopVector u1 = sp - aArcCenter;
            uopVector u2 = ep - aArcCenter;
            uopVector P = u1;
            uopVector delta = P;
            uopVector C = aArcCenter;
            aDir = u2 - u1;

            List<double> t = new List<double>(new double[2]);
            double d1 = Math.Pow(aDir.Length(), 2);
            double d2 = Math.Pow(delta.Length(), 2) - Math.Pow(rad, 2);
            double d3 = 0;
            double omega = Math.Pow(aDir.DotProduct(delta), 2) - (d1 * d2);


            if (omega < 0) return _rVal;


            uopVector P1 = u1.Clone(true);
            uopVector P2 = uopVector.Zero;
            uopVector xDir = new uopVector(1, 0);
            uopVector yDir = new uopVector(0, 1);
            d3 = d1;
            d1 = (aDir * -1).DotProduct(delta);
            d2 = Math.Pow(omega, 2);

            t[0] = (d1 + d2) / d3;
            P1 = P + (aDir * t[0]);
            P2 = C + (xDir * P1.X);
            P2 += yDir * P1.Y;
            if ((Math.Round(t[0], 6) >= 0 & Math.Round(t[0], 6) <= 1) || bInfiniteLine)
            { _rVal.Add(P2); }

            if (omega != 0)
            { //zero means tangent
                t[1] = (d1 - d2) / d3;
                P1 = P + (aDir * t[1]);
                P2 = C + (xDir * P1.X);
                P2 += yDir * P1.Y;
                if ((Math.Round(t[1], 6) >= 0 & Math.Round(t[1], 6) <= 1) || bInfiniteLine)
                { _rVal.Add(P2); }
            }

            return _rVal;
        }
        public uopVectors EndPoints(bool bGetClones = false, bool bGetStart = true, bool bGetEnd = true)
        {
            if (!bGetStart && !bGetEnd) return uopVectors.Zero;
            if (bGetStart && bGetEnd) 
                return !bGetClones ? new uopVectors() { sp, ep } :new uopVectors() { new uopVector(sp),  new uopVector(ep) };
        if(bGetStart)
                return !bGetClones ? new uopVectors() { sp } : new uopVectors() { new uopVector(sp) };
            else
                return !bGetClones ? new uopVectors() { ep } : new uopVectors() { new uopVector(ep) };
        }

        public List<uopLine> PointSegments(bool bIncludeStartPt = true, bool bIncludeEndPt = true, bool bSortStartToEnd = true)
        {
            List<uopLine> _rVal = new List<uopLine>();

            List<iVector> pts = new List<iVector>();
            if (bIncludeStartPt) pts.Add(new uopVector(sp));
            for (int i = 1; i <= Points.Count; i++) { pts.Add(new uopVector(Points.Item(i))); }
            if (bIncludeEndPt) pts.Add(new uopVector(ep));

            if (bSortStartToEnd)
                pts = dxfVectors.Sort(pts, dxxSortOrders.NearestToFarthest, new uopVector(sp));

            for (int i = 1; i <= pts.Count - 1; i++)
            {
                if (i + 1 > pts.Count) break;
                uopLine l1 = new uopLine(pts[i - 1], pts[i]);
                if (l1.Length != 0) _rVal.Add(l1);


            }

            return _rVal;
        }

        public void Resize(double aNewLength, uppSegmentPoints aBasePt = uppSegmentPoints.StartPt)
        {
            uopVector dir = Direction;

            switch (aBasePt)
            {
                case uppSegmentPoints.MidPt:
                    {
                        double d1 = 0.5 * aNewLength;
                        uopVector u1 = MidPt;
                        uopVector u2 = u1 + dir * d1;
                        ep.X = u2.X; ep.Y = u2.Y;
                        u2 = u1 - dir * d1;
                        sp.X = u2.X; sp.Y = u2.Y;

                        break;
                    }
                case uppSegmentPoints.EndPt:
                    {
                        uopVector u1 = ep + dir * aNewLength;
                        sp.X = u1.X; sp.Y = u1.Y;
                        break;
                    }
                default:
                    {
                        uopVector u1 = sp - dir * aNewLength;
                        ep.X = u1.X; ep.Y = u1.Y;
                        break;
                    }
            }
        }

        public uopLine Resized(double aNewLength, uppSegmentPoints aBasePt = uppSegmentPoints.StartPt)
        {
            uopLine _rVal = new uopLine(this);
            _rVal.Resize(aNewLength, aBasePt);
            return _rVal;
        }

        public void Stretch(double aLengthAdder, uppSegmentPoints aBasePt = uppSegmentPoints.StartPt)
        {
            uopVector dir = Direction;

            switch (aBasePt)
            {
                case uppSegmentPoints.MidPt:
                    {
                        double d1 = 0.5 * (aLengthAdder + Length) ;
                        uopVector u1 = MidPt;
                        uopVector u2 = u1 + dir * d1;
                        ep.X = u2.X; ep.Y = u2.Y;
                        u2 = u1 - dir * d1;
                        sp.X = u2.X; sp.Y = u2.Y;

                        break;
                    }
                case uppSegmentPoints.EndPt:
                    {
                        uopVector u1 = ep + dir * (Length + aLengthAdder);
                        sp.X = u1.X; sp.Y = u1.Y;
                        break;
                    }
                default:
                    {
                        uopVector u1 = sp - dir * (Length + aLengthAdder);
                        ep.X = u1.X; ep.Y = u1.Y;
                        break;
                    }
            }
        }

        public uopLine Stretched(double aLengthAdder, uppSegmentPoints aBasePt = uppSegmentPoints.StartPt)
        {
            uopLine _rVal = new uopLine(this);
            _rVal.Resize(aLengthAdder, aBasePt);
            return _rVal;
        }

        #endregion Methods

        #region Shared Methods


        public static void MirrorLines(IEnumerable<uopLine> aLines, double? aX, double? aY)
        {
            if (aLines == null) return;
       
            foreach (var item in aLines) if(item != null) item.Mirror(aX,aY);
       
        }

        public static uopLine CloneCopy(uopLine aLine) => aLine == null ? null : new uopLine(aLine);


        public static uopVectors GetIntersections(List<uopLine> aLines, bool aLinesAreInfinite = true, bool bNoDupes = true, int aPrecis = 6)
        {
            uopVectors _rVal = new uopVectors();
            if(aLines == null) return _rVal;
            aPrecis = mzUtils.LimitedValue(aPrecis, 0, 15);

            for (int i = 1; i <= aLines.Count; i++)
            {
                uopLine l1 = aLines[i - 1];
                if (l1 == null) continue;
                for (int j = 1; j <= aLines.Count; j++)
                {
                    if (i == j) continue;
                    uopLine l2 = aLines[j - 1];
                    if (l2 == null) continue;

                    uopVector ip = l1.IntersectionPt(l2, out _, out _, out bool bOn1, out bool bOn2, out bool exists);
                    if (!exists) continue;
                    bool keep = aLinesAreInfinite;
                    if(!keep) keep = bOn1 && bOn2;
                    if(!keep) continue;
                    if (bNoDupes)
                    {
                        foreach(var u2 in _rVal)
                        {
                            if(ip.IsEqual(u2, aPrecis:aPrecis)) { keep = false; break; };
                        }
                    }
                    if(keep) _rVal.Add(ip);
                }
            }
            return _rVal;

        }

        public static uopLine FromLine(iLine aLine, bool bReturnClone = false)
        {
            if (aLine == null) return null;
            if (aLine is uopLine)
                return bReturnClone ? new uopLine((uopLine)aLine) : (uopLine)aLine;
            else
                return new uopLine(aLine);
        }

        /// <summary>
        /// computes and returns the shortest line between the two passed line.
        /// </summary>
        /// <remarks>It may not exist!.</remarks>
        /// <param name="aLine">the first line</param>
        /// <param name="bLine">the second line</param>
         /// <param name="rAIsNull">returns true if the first line is zero length or undefined</param>
        /// <param name="rBIsNull">returns true if the first line is zero length or undefined</param>
        /// <returns></returns>
        public static uopLine ShortestConnector(iLine aLine, iLine bLine, out bool rAIsNull, out bool rBIsNull)
        {

            rAIsNull = true;
            rBIsNull = false;
            if (aLine == null || aLine == null) return null;
           
            uopLine l1 = uopLine.FromLine(aLine, false);
            uopLine l2 = uopLine.FromLine(bLine, false);

            rAIsNull = !l1.Paramatize(out uopVector P1, out uopVector P21);
            rBIsNull = !l2.Paramatize(out uopVector p3, out uopVector P43);
            if (rAIsNull || rBIsNull) return null;


            uopVector P2 = l1.ep;
            uopVector p4 = l2.ep;
            uopVector P13 = P1 - p3;
            double d1343 = P13.X * P43.X + P13.Y * P43.Y;
            double d4321 = P43.X * P21.X + P43.Y * P21.Y;
            double d1321 = P13.X * P21.X + P13.Y * P21.Y;
            double d4343 = P43.X * P43.X + P43.Y * P43.Y;
            double d2121 = P21.X * P21.X + P21.Y * P21.Y;
            double denom = d2121 * d4343 - d4321 * d4321;
            if (denom <= 0.0000001) return null;

            uopLine _rVal = uopLine.FromLine(aLine, true);


            double numer = d1343 * d4321 - d1321 * d4343;
            double mua = numer / denom;
            double mub = (d1343 + d4321 * mua) / d4343;
            _rVal.sp.X = mzUtils.VarToDouble(P1.X + mua * P21.X);
            _rVal.sp.Y = mzUtils.VarToDouble(P1.Y + mua * P21.Y);
            //    proj_V2L rConnector.sp, aLine

            _rVal.ep.X = mzUtils.VarToDouble(p3.X + mub * P43.X);
            _rVal.ep.Y = mzUtils.VarToDouble(p3.Y + mub * P43.Y);
            //    proj_V2L rConnector.ep, bLine
            return _rVal;
        }

        #endregion Shared Methods

        #region iLine_Implementation

        public iVector StartPt { get => sp; set { if (value == null) { sp.SetCoordinates(0, 0, 0); } else { sp.SetCoordinates(value.X, value.Y, value.Z); } } }
        public iVector EndPt { get => ep; set { if (value == null) { ep.SetCoordinates(0, 0, 0); } else { ep.SetCoordinates(value.X, value.Y, value.Z); } } }
        iVector iLine.StartPt { get => StartPt; set => StartPt = value; }
        iVector iLine.EndPt { get => EndPt; set => EndPt = value; }
        #endregion iLine_Implementation


        #region iSegment_Implementation

        bool iSegment.IsArc => false;

        uopArc iSegment.Arc => null;

        uopLine iSegment.Line => this;

        double iSegment.Radius => 0;

        public uppSegmentTypes SegmentType => uppSegmentTypes.Line;

        iSegment iSegment.Clone() => new uopLine(this);
        
        #endregion  iSegment_Implementation
    }
}
