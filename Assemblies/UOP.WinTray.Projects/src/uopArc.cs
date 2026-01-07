using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Windows;
using UOP.DXFGraphics;
using UOP.DXFGraphics.Utilities;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Interfaces;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using static System.Net.Mime.MediaTypeNames;

namespace UOP.WinTray.Projects
{
    public class uopArc : ICloneable, iSegment, iArc, iArcRec
    {
        #region Constructors
        public uopArc() => Init() ;

        public uopArc(double aRadius, iVector aCenter = null, double aStartAngle = 0, double aEndAngle = 360)
        {
            Init();
            Center = new uopVector(aCenter);
            Radius = Math.Abs(aRadius);
            StartAngle = aStartAngle;
            EndAngle = aEndAngle;
           
        }
        public uopArc(double aRadius, iVector aCenter, iVector aStartPt, iVector aEndPt)
        {
            Init();
            DefineWithPoints(aRadius, aCenter, aStartPt,aEndPt);
        
        }
        internal uopArc(UARC aArc) => Init(bArc: aArc);

        public uopArc(iArcRec aArcRec) => Init(aArcRec: aArcRec);


        public uopArc(uopArc aArc) =>Init(aArc);
        
        public uopArc(iSegment aArc)
        {
            if (aArc == null)
            {
                Init(); return;
            }
            if(aArc.IsArc) Init(aArc.Arc);
        }

        public uopArc(iArc aArc) =>Init(aArc);

        private void Init(iArc aArc = null, UARC? bArc = null, iArcRec aArcRec = null)
        {
            Center = uopVector.Zero;
            Radius = 0;
            StartAngle = 0;
            EndAngle = 360;
            Tag = string.Empty;
            Flag = string.Empty;
            _Points = uopVectors.Zero;
            Suppressed = false;
            if (bArc.HasValue)
            {
                Center = new uopVector(bArc.Value.Center);
                Radius = bArc.Value.Radius;
                StartAngle = bArc.Value.StartAngle;
                EndAngle = bArc.Value.EndAngle;
                Suppressed = bArc.Value.Suppressed;
            }

            if (aArc != null)
            {

                Center = new uopVector(aArc.Center);
                Radius = aArc.Radius;
                StartAngle = aArc.StartAngle;
                EndAngle = aArc.EndAngle;
                if (aArc is uopArc)
                {
                    uopArc uarc = (uopArc)aArc;
                    Tag = uarc.Tag;
                    Flag = uarc.Flag;
                    Suppressed = uarc.Suppressed;
                    _Points = new uopVectors(uarc._Points);
                }
                else if (aArc.GetType() == typeof(dxeArc))
                {
                    dxeArc dxarc = (dxeArc)aArc;
                    Tag = dxarc.Tag;
                    Flag = dxarc.Flag;
                   
                }
            }

            if (aArcRec != null)
            {
                Center = new uopVector(aArcRec.Center);
                Radius = aArcRec.Radius;
                Suppressed = aArcRec.Suppressed;
            }

        }
        #endregion Constructors

        #region Properties

        protected uopVectors _Points;
        public virtual uopVectors Points { get { _Points ??= new uopVectors(); return _Points; } set { _Points = value ?? uopVectors.Zero; } }

        public string Tag { get; set; }
        public string Flag { get; set; }

        public bool Suppressed { get; set; }
        /// <summary>
        ///  a  delimited string with the objects Tag and Flag
        /// </summary>
        public string Handle => uopUtils.Handle(Tag, Flag);

        private uopVector _Center;
        public uopVector Center { get { _Center ??= uopVector.Zero; return _Center; } set { _Center = value != null ? value : uopVector.Zero; } }

        public double X => Center.X;
        public double Y => Center.Y;

        public double Radius { get; set; }
        public double StartAngle { get; set; }

        public double EndAngle { get; set; }

        public double SpannedAngle => uopUtils.SpannedAngle(false, StartAngle, EndAngle);

        public uopVector StartPoint => new uopVector(StartPt);

        public uopVector EndPoint => new uopVector(EndPt);

        public uopVector MidPoint => new uopVector(MidPt);

        internal UVECTOR StartPt => new UVECTOR(Radius, 0).Rotated(StartAngle) + new UVECTOR(Center);

        internal UVECTOR EndPt => new UVECTOR(Radius, 0).Rotated(EndAngle) + new UVECTOR(Center);

        internal UVECTOR MidPt => new UVECTOR(Radius, 0).Rotated(StartAngle + 0.5 * SpannedAngle) + new UVECTOR(Center);
        
        public double Length => (SpannedAngle  * Math.PI) / 180 * Radius;
        
        internal UARC Structure
        {
            get => new UARC(new UVECTOR(Center), Radius, StartAngle, EndAngle);
            set
            {
                Center = new uopVector(value.Center);
                Radius = value.Radius;
                StartAngle = value.StartAngle;
                EndAngle = value.EndAngle;

            }


        }
        #endregion Properties

        #region Methods

        public void DefineWithPoints(double aRadius, iVector aCenter, iVector aStartPt, iVector aEndPt)
        {
            Center = new uopVector(aCenter);
            Radius = Math.Abs(aRadius);
            StartAngle = 0;
            EndAngle = 360;
            if (aStartPt != null && Math.Round(aStartPt.X, 3) == Math.Round(X, 3) && Math.Round(aStartPt.Y, 3) == Math.Round(Y, 3)) aStartPt = null;
            if (aEndPt != null && Math.Round(aEndPt.X, 3) == Math.Round(X, 3) && Math.Round(aEndPt.Y, 3) == Math.Round(Y, 3)) aEndPt = null;

            if (aStartPt != null)
            {
                UVECTOR sp = new UVECTOR(aStartPt);
                double dx = sp.X - X;
                double dy = sp.Y - Y;
                double ang = dx == 0 ? 90 : dy == 0 ? 0 : Math.Atan2(Math.Abs(dy), Math.Abs(dx)) * 180 / Math.PI;
                if (dx > 0)
                    StartAngle = dy > 0 ? ang : 360 - ang;
                else
                    StartAngle = dy > 0 ? 180 - ang : 180 + ang;
            }
            if (aEndPt != null)
            {
                UVECTOR ep = new UVECTOR(aEndPt);
                double dx = ep.X - X;
                double dy = ep.Y - Y;
                double ang = dx == 0 ? 90 : dy == 0 ? 0 : Math.Atan2(Math.Abs(dy), Math.Abs(dx)) * 180 / Math.PI;
                if (dx > 0)
                    EndAngle = dy > 0 ? ang : 360 - ang;
                else
                    EndAngle = dy > 0 ? 180 - ang : 180 + ang;
            }

        }

        public virtual double MaxX(int? aPrecis = null) => PhantomPoints().GetExtremeOrd(bMin: false, bGetY: false, aPrecis: aPrecis.HasValue ? aPrecis.Value : 4);
        public virtual double MinX(int? aPrecis = null) => PhantomPoints().GetExtremeOrd(bMin: true, bGetY: false, aPrecis: aPrecis.HasValue ? aPrecis.Value : 4);

        public virtual double MaxY(int? aPrecis = null) => PhantomPoints().GetExtremeOrd(bMin: false, bGetY: true, aPrecis: aPrecis.HasValue ? aPrecis.Value : 4);
        public virtual double MinY(int? aPrecis = null) => PhantomPoints().GetExtremeOrd(bMin: true, bGetY: true, aPrecis: aPrecis.HasValue ? aPrecis.Value : 4);

        public uopArc Moved(double aX = 0, double aY = 0, bool bMovePoints = true)
        {
            uopArc _rVal = new uopArc(this);
            _rVal.Move(aX, aY, bMovePoints);
            return _rVal;
        }

        public void Move(double aX = 0, double aY = 0, bool bMovePoints = true)
        {
            Center.Move(aX, aY);
            if (bMovePoints) Points.Move(aX, aY);
        }
        public uopArc Clone() => new uopArc(this);
        object ICloneable.Clone() => (object)new uopArc(this);

       
        public uopArc ExtendedTo(uopLinePair aLines, double? aNewRadius = null)
        {
            uopArc _rVal = new uopArc(this);
            if(aNewRadius.HasValue && aNewRadius.Value > 0)  _rVal.Radius = aNewRadius.Value;
            if (aLines == null) return _rVal;
            uopVector sp = StartPoint;
            uopVector ep = EndPoint;
            if (aLines.Line1 != null)
            {
                uopVectors ips = _rVal.Intersections(aLines.Line1, true, true);
                if (ips.Count >= 2)
                {
                    if (ips.Count > 1)
                    {
                        sp = ips.Nearest(sp);
                        ep = ips.Nearest(ep);
                    }
                    else
                    {
                        if (ips[0].DistanceTo(sp) < ips[0].DistanceTo(ep)) sp = ips[0]; else ep = ips[0];
                    }

                }
            }
            if (aLines.Line2 != null)
            {
                uopVectors ips = this.Intersections(aLines.Line1, true, true);
                if (ips.Count >= 2)
                {
                    if (ips.Count > 1)
                    {
                        sp = ips.Nearest(sp);
                        ep = ips.Nearest(ep);
                    }
                    else
                    {
                        if (ips[0].DistanceTo(sp) < ips[0].DistanceTo(ep)) sp = ips[0]; else ep = ips[0];
                    }
                }

                
            
            }
            _rVal.DefineWithPoints(_rVal.Radius, Center, sp, ep);
            return _rVal;
        }

        public uopVectors Intersections(uopArc aArc, bool aArcIsInfinite = false, bool bArcIsInfinite = false) => aArc == null ? uopVectors.Zero : new uopVectors(Structure.Intersections(aArc.Structure, aArcIsInfinite, bArcIsInfinite));
        public uopVectors Intersections(uopLine aLine, bool aArcIsInfinite = false, bool aLineIsInfinite = false) => aLine == null ? uopVectors.Zero : new uopVectors(Structure.Intersections(new ULINE(aLine), aArcIsInfinite, aLineIsInfinite));

        public uopVectors Intersections(uopLinePair aLines, bool aArcIsInfinite = false, bool aLinesAreInfinite = false)
        {
            uopVectors _rVal = uopVectors.Zero;
            if (aLines == null) return _rVal;
            if(aLines.Line1 != null)
            {
                uopVectors ips =this.Intersections(aLines.Line1, aArcIsInfinite, aLinesAreInfinite);
                if (ips.Count > 0) _rVal.AddRange(ips);
            }
            if (aLines.Line2 != null)
            {
                uopVectors ips = Intersections(aLines.Line2, aArcIsInfinite, aLinesAreInfinite);
                if (ips.Count > 0) _rVal.AddRange(ips);
            }
            return _rVal;
        }

        public uopVectors Intersections(IEnumerable <iSegment> aSegments, bool aArcIsInfinite = false, bool aSegmentIsInfinite = false)
        {
                uopVectors _rVal = new uopVectors();
            if (aSegments == null) return _rVal;
            foreach (iSegment aSegment in aSegments) 
            {
                if (aSegment == null) continue; 
                    if (aSegment.IsArc)
                        _rVal.AddRange(Intersections(aSegment.Arc ,aArcIsInfinite, aSegmentIsInfinite));
                    else
                        _rVal.AddRange(Intersections(aSegment.Line, aArcIsInfinite, aSegmentIsInfinite));

               
            }
                return _rVal;
            }


        public override string ToString() => $"uopArc - CP:{Center.X:0.00##},{Center.Y:0.00##} RAD:{Radius:0.00##} SA:{StartAngle:0.0} EA:{EndAngle:0.0} {uopUtils.Handle(Tag,Flag,"<>")}".Trim();



        

        /// <summary>
        /// returns true if the passed vector lies on the arc
        /// </summary>
        /// <param name="aVector" the vector to test></param>
        /// <param name="aPrecis" a precision apply></param>
        /// <param name="bTreatAsInfinite" flag to treat the arc as 360 degrees></param>
        /// <returns></returns>
        public bool ContainsVector(iVector aVector, bool bOnIsIn = true, int aPrecis = 5,  bool bTreatAsInfinite = false)
        => aVector == null ? false : ContainsVector(aVector, bOnIsIn, aPrecis,  out bool _, out bool _, out bool _, out bool _,  bTreatAsInfinite: bTreatAsInfinite);

        /// <summary>
        /// returns true if the passed vector lies on the arc or withing the arc
        /// </summary>
        /// <param name="aVector" >the vector to test</param>
        /// <param name="aPrecis">a precision apply</param>
        /// <param name="bOnIsIn" >flag treat vectors on the path as out of the circle </param>
        /// <param name="rWithin">returns true if the passed vector is within the arc. if true, the endpoint checks are not performed</param>
        /// <param name="rIsOn" >returns true of the vecor lies on the path of the arc</param>
        /// <param name="rIsStartPt"> returns true if the passed vector is the start vector of the arc</param>
        /// <param name="rIsEndPt" >returns true if the passed vector is the end vector of the arc</param>
        /// <param name="bTreatAsInfinite" >flag to treat the arc as 360 degrees . if true, the endpoint checks are not performed</param>
        public bool ContainsVector(iVector aVector, bool bOnIsIn,int aPrecis,out bool rWithin, out bool rIsOn,  out bool rIsStartPt, out bool rIsEndPt, bool bTreatAsInfinite = true)
        {
            rIsStartPt = false;
            rIsEndPt = false;
     
            aPrecis = mzUtils.LimitedValue(aPrecis, 0, 15);
            if (SpannedAngle >= 359.99) bTreatAsInfinite = true;

            double rad = Math.Round(Radius, aPrecis);

            double d1 = Math.Round(Math.Sqrt(Math.Pow(X - aVector.X, 2) + Math.Pow(Y - aVector.Y, 2)), aPrecis);
            //see if the point lies on the path of the arc with the fudge factor. If not,bail out
            rIsOn = rad == d1;
            rWithin = d1 < rad;
            if ((!rWithin && !rIsOn) || (rIsOn && !bOnIsIn)) return false;

            if (!rIsOn && rWithin) return true;
            UVECTOR u1 = new UVECTOR(aVector);
            rIsStartPt = mzUtils.CompareVal(0, u1.DistanceTo(StartPt), aPrecis) == mzEqualities.Equals;
            rIsEndPt = mzUtils.CompareVal(0, u1.DistanceTo(EndPt), aPrecis) == mzEqualities.Equals;
            if (rIsStartPt || rIsEndPt || bTreatAsInfinite) return true;
            //the point lies on the path of the arce and the arc 360 degrees so return true
            
            //get the angle from the positive X axis and compare to the arcs start and end angles
            double ang = (u1 - new UVECTOR(X,Y)).XAngle;
            mzEqualities eqlt1 = mzUtils.CompareVal(ang, StartAngle, 2);
            switch (eqlt1)
            {
                case mzEqualities.Equals:
                    rIsStartPt = true;
                    return true;
                case mzEqualities.LessThan:
                    return false;
                case mzEqualities.GreaterThan:
                    mzEqualities eqlt2 = mzUtils.CompareVal(ang, EndAngle, 2);
                    if (eqlt2 == mzEqualities.GreaterThan) return false;
                    if (eqlt2 == mzEqualities.Equals) rIsEndPt = true;
                    return true;
            }

            return true;
        }

      
   
        public List<uopArc> TrimWithRectangle(uopRectangle aRectangle, bool bReturnInteriors = true, bool bReturnExteriors = true)
        {
            List<uopArc> _rVal = new List<uopArc>();
            if (aRectangle == null) return new List<uopArc>();

            try 
            {
                if (!bReturnInteriors && !bReturnExteriors) return _rVal;

                double rad = Math.Round(Radius, 8);
               if( rad == 0 ) return _rVal;

                UVECTOR org = new UVECTOR(Center);
                uopVectors vrts = RectangleIntersections(aRectangle.Left,aRectangle.Right, aRectangle.Top,aRectangle.Bottom , false, false,out uopVectors inside,out uopRectangle trect);
                uopArc dArc = new uopArc(this);
            //    Dim arc As New TARC(plane, plane.Origin, rad)
            //    Dim inside As New TVERTICES(0)
            //    Dim trect As dxfRectangle = Nothing
            //    Dim vrts As TVERTICES = arc.RectangleIntersections(aLeftLim, aRightLim, aTopLim, aBottomLim, False, False, inside, trect)
            //    //the rectangle contains the full circle so return it
                if (vrts.Count <= 0 && inside.Count <= 0)
                {
                    dArc = new uopArc(this) { StartAngle = 0, EndAngle = 360, Tag = "FULL CIRCLE" }; 
                        _rVal.Add(dArc);
                }
                else 
                {
                    if (vrts.Count <= 1) return _rVal;
                    for (int i = 1; i <= vrts.Count; i++) 
                    {
                        uopVector sp = vrts[i - 1];
                        uopVector ep = i < vrts.Count ? vrts[i] : vrts[0] ;
                        if (Math.Round(sp.X, 8) == Math.Round(ep.X, 8) && Math.Round(sp.Y, 8) == Math.Round(ep.Y, 8)) continue;
                        UARC arc = UARC.DefineWithVectors(org,new UVECTOR(sp), new UVECTOR( ep));
                        dArc = new uopArc(arc) { Tag = $"SEGMENT {i}", Radius = Math.Abs(Radius) };
                        bool keep = true;
                        if (!bReturnInteriors || !bReturnExteriors)
                        {
                            UVECTOR mpt = dArc.MidPt;
                            if (!bReturnInteriors)
                                keep = !trect.Contains(mpt);
                            else
                                keep = trect.Contains(mpt);
                        }
                        if (keep) _rVal.Add(dArc);
                    }

                }
                //_rVal.Add(dArc)
                //    Else
                //        If vrts.Count <= 1 Then Return _rVal
                //        Dim sp As TVERTEX
                //        Dim ep As TVERTEX
                //        Dim bounds As dxfRectangle = plane.ToRectangle()
                //        For i As Integer = 1 To vrts.Count
                //            sp = vrts.Item(i)
                //            If i<vrts.Count Then ep = vrts.Item(i + 1) Else ep = vrts.Item(1)
                //            arc = TARC.DefineWithPoints(plane, plane.Origin, sp.Vector, ep.Vector, False, True)
                //            dArc = New dxeArc(arc, aDisplaySettings) With {.Name = $"SEGMENT {i}"}
                //If Math.Round(sp.X, 8) = Math.Round(ep.X, 8) And Math.Round(sp.Y, 8) = Math.Round(ep.Y, 8) Then
                //    Continue For
                //End If
                //Dim keep As Boolean = True
                //            If Not bReturnInteriors Or Not bReturnExteriors Then
                //                Dim mpt As dxfVector = dArc.MidPt

                //                If Not bReturnInteriors Then
                //                    keep = Not trect.ContainsVector(mpt)
                //                Else
                //                    keep = trect.ContainsVector(mpt)
                //                End If

                //            End If
                //            If keep Then _rVal.Add(dArc)
                //        Next
                //    End If
                
            }
            catch (Exception ex)
            { throw ex; }
            
            
            //Return dxfPrimatives.CreateCircleSegments(Radius, rec.Left, rec.Right, rec.Top, rec.Bottom, aPlane:= New dxfPlane(myplane), aDisplaySettings:= aDisplaySettings, bReturnInteriors, bReturnExteriors)
            return _rVal;
        }


        public uopVectors RectangleIntersections(double? aLeftLim = null, double? aRightLim = null, double? aTopLim = null, double? aBottomLim = null, bool bReturnInteriorCormers =false, bool bReturnQuadrantPt= false)
         => RectangleIntersections(aLeftLim,aRightLim,aTopLim,aBottomLim,bReturnInteriorCormers,bReturnQuadrantPt,out _, out _);
        
        public uopVectors RectangleIntersections(double? aLeftLim,double? aRightLim, double? aTopLim, double? aBottomLim, bool bReturnInteriorCormers, bool bReturnQuadrantPt, out uopVectors rInsides, out uopRectangle rTrimRectangle)
        {
            rInsides = new uopVectors();
            rTrimRectangle = new uopRectangle();
            uopVectors _rVal = uopVectors.Zero;
            double rad = Math.Round(Radius, 8);
            if (rad == 0) return _rVal;

            UVECTOR org = new UVECTOR(Center);
            double left = aLeftLim.HasValue ? aLeftLim.Value :  X - rad - 10;
            double right = aRightLim.HasValue ? aRightLim.Value : X + rad + 10;
            double top= aTopLim.HasValue ? aTopLim.Value : Y + rad + 10;
            double bot = aBottomLim.HasValue ? aBottomLim.Value : Y - rad - 10;
            mzUtils.SortTwoValues(true, ref left, ref right);
            mzUtils.SortTwoValues(true, ref top, ref bot);
            if(left< X-rad-10) left = X-rad-10;
            if (right > X + rad + 10) right= X + rad + 10;
            if (bot < Y - rad - 10) bot = Y - rad - 10;
            if (top > Y + rad + 10) top = Y + rad + 10;
            rTrimRectangle = new uopRectangle(left,top,right,bot);
            int inside = 0;
            UVECTOR ul = new UVECTOR(left, top) { Tag = "UL" };
            UVECTOR ll = new UVECTOR(left, bot) { Tag = "LL" };
            UVECTOR ur = new UVECTOR(right, top) { Tag = "UR" };
            UVECTOR lr = new UVECTOR(right, bot) { Tag = "LR" };

            if (org.DistanceTo(ul) < rad) inside++;
            if (org.DistanceTo(ur) < rad) inside++;
            if (org.DistanceTo(ll) < rad) inside++;
            if (org.DistanceTo(lr) < rad) inside++;

            if (inside == 4) return _rVal; // all corners are within the circle so we cant

            uopVectors ipts = Intersections(rTrimRectangle.Segments, true, false);
            foreach (uopVector v in ipts)
            {
                if (!_rVal.ContainsVector(v))
                    _rVal.Add(new uopVector(v) { Radius = rad, Tag="IP"});
            }

            if (bReturnQuadrantPt)
            {
                uopVector q = new uopVector(X, Y + rad, aRadius: rad) { Tag = "Q1" };
                if (rTrimRectangle.Contains(q)) _rVal.Add(q);
                q = new uopVector(X-rad, Y , aRadius: rad) { Tag = "Q2" };
                if (rTrimRectangle.Contains(q)) _rVal.Add(q);
                q = new uopVector(X , Y-rad, aRadius: rad) { Tag = "Q3" };
                if (rTrimRectangle.Contains(q)) _rVal.Add(q);
                q = new uopVector(X + rad, Y, aRadius: rad) { Tag = "Q4" };
                if (rTrimRectangle.Contains(q)) _rVal.Add(q);

            }
             _rVal.Sort(dxxSortOrders.CounterClockwise, Center);

            if (bReturnInteriorCormers)
            {
                for (int i = 1; i <= 4; i++)
                {
                    UVECTOR u1 = i switch
                    {
                        1 => ul,
                        2 => ll,
                        3 => lr,
                        4 => ur,
                        _ => UVECTOR.Zero,
                    };
                    if(org.DistanceTo(u1) < rad)
                    {
                        _rVal.Add(u1);
                        rInsides.Add(u1);
                    }

                }
            }

            return _rVal;
        }


        public uopVectors QuadrantPoints()
        {
            return new uopVectors() { new uopVector(X + Radius, Y), new uopVector(X , Y + Radius), new uopVector(X - Radius, Y), new uopVector(X , Y -Radius) };
        }

        public uopArc QuadrantArc(int aQuadrant)
        {
            aQuadrant = mzUtils.LimitedValue(aQuadrant, 1, 4, 1);
            uopArc _rVal = new uopArc(this);
            switch (aQuadrant)
            { 
                case 1:
                {
                        _rVal.StartAngle = 0;
                        _rVal.EndAngle = 90;
                    break;
                }
                case 2:
                    {
                        _rVal.StartAngle = 90;
                        _rVal.EndAngle = 180;
                        break;
                    }
                case 3:
                    {
                        _rVal.StartAngle = 180;
                        _rVal.EndAngle = 270;
                        break;
                    }
                case 4:
                    {
                        _rVal.StartAngle = 270;
                        _rVal.EndAngle = 360;
                        break;
                    }
            }
            return _rVal;
        }
        public uopVectors PhantomPoints(int? aCurveDivisions = null, bool bIncludeEndPt = true) => uopArc.ArcPhantomPts(this, aCurveDivisions, bIncludeEndPt);
        public uopRectangle BoundingRectangle(int? aCurveDivisions = null) => ArcPhantomPts(this, aCurveDivisions, true).BoundingRectangle;

        public bool Contains(iArcRec aArcRec, bool bOnIsIn = true, int aPrecis = 4, bool bMustBeCompletelyWithin = false, bool bReturnTrueByCenter = false) => uopArcRecs.ArcRecContainsArcRec(this, aArcRec, out _, out _, bOnIsIn, aPrecis, bMustBeCompletelyWithin, bReturnTrueByCenter);

        public bool Contains(iArcRec aArcRec, out bool rCoincindent, out bool rIntersects, bool bOnIsIn = true, int aPrecis = 4, bool bMustBeCompletelyWithin = false, bool bReturnTrueByCenter = false) => uopArcRecs.ArcRecContainsArcRec(this,aArcRec,out rCoincindent,out rIntersects,bOnIsIn,aPrecis,bMustBeCompletelyWithin, bReturnTrueByCenter);
        #endregion Methods

        #region iArcRec Implementation

        uppArcRecTypes iArcRec.Type =>  uppArcRecTypes.Arc;
        uopArc iArcRec.Arc => this;
        uopRectangle iArcRec.Rectangle => null;
        double iArcRec.Height { get => 2 * Radius; }
        double iArcRec.Width { get => 2 * Radius; }
        double iArcRec.Rotation { get => 0; }
        uopShape iArcRec.Slot => null;

        #endregion iArcRec Implementation

        #region iSegment Implementation

        public uppSegmentTypes SegmentType => uppSegmentTypes.Arc;
        bool iSegment.IsArc => true;
        uopArc iSegment.Arc => this;

        uopLine iSegment.Line => null;

        iSegment iSegment.Clone() => new uopArc(this);
        #endregion  iSegment Implementation

        #region iArc Implementation

        iVector iArc.Center { get => Center; set => Center = new uopVector(value); }

        dxfPlane iArc.Plane => new dxfPlane(Center);

        double iArc.StartAngle { get => StartAngle; set => StartAngle = value; }
        double iArc.EndAngle { get => EndAngle; set => EndAngle = value; }

        double iArc.Radius { get => Radius; set => Radius = value; }

        iArcRec iArcRec.Clone() => Clone();

        #endregion iArc Implementation


        #region Shared Methods
        public static uopVectors ArcPhantomPts(uopArc aArc, int? aCurveDivisions = null, bool bIncludeEndPt = true)
        {
            uopVectors _rVal = uopVectors.Zero;
            if (aArc == null) return _rVal;
            if (!aCurveDivisions.HasValue)
            {
                if (aArc.SpannedAngle <= 90)
                    aCurveDivisions = 5;
                else if (aArc.SpannedAngle <= 180)
                    aCurveDivisions = 10;
                else if (aArc.SpannedAngle <= 270)
                    aCurveDivisions = 15;
                else
                    aCurveDivisions = 20;
            }
            aCurveDivisions = mzUtils.LimitedValue(aCurveDivisions.Value, 2, 1000);

            UVECTOR spz = new UVECTOR(aArc.Center);

            UVECTOR aZDir = new UVECTOR(0, 0, 1);
            UVECTOR xdir = new UVECTOR(1, 0, 0);
            UVECTOR sp = spz + xdir * aArc.Radius;
            UVECTOR ep = spz + xdir * aArc.Radius;
            double sa = mzUtils.NormAng(aArc.StartAngle, ThreeSixtyEqZero: false, bReturnPosive: true);
            double ea = mzUtils.NormAng(aArc.EndAngle, ThreeSixtyEqZero: true, bReturnPosive: true);
            if (sa != 0) sp.Rotate(spz, sa);
            if (ea != 0) ep.Rotate(spz, ea);
            double span = dxfMath.SpannedAngle(false, sa, ea);

            int segs = aCurveDivisions.Value;
            double angchange = span / segs;
            _rVal.Add(sp);

            UVECTOR v1 = sp;

            double remain = span;
            while (remain > angchange)
            {
                v1.Rotate(spz, angchange);
                _rVal.Add(v1);
                remain -= angchange;
            }


            if (bIncludeEndPt) _rVal.Add(ep);
            return _rVal;
        }
      

        public static uopVectors ArcIntersections(uopArc aArc, uopArc bArc, bool aArcIsInfinite = true, bool bArcIsInfinite = true)
        {
            // ^Finds the intersections of the two passed arcs
            // ~assume  Circular arcs

            if (aArc == null || bArc == null) return uopVectors.Zero;
            UVECTORS intpts = UARC.ArcIntersections(aArc.Structure, bArc.Structure, aArcIsInfinite, bArcIsInfinite);
            return new uopVectors(intpts);
        }

        internal static UVECTORS ArcIntersections(uopArc aArc, UARC bArc, bool aArcIsInfinite = true, bool bArcIsInfinite = true)
        {
            // ^Finds the intersections of the two passed arcs
            // ~assume  Circular arcs

            if (aArc == null || bArc.Radius <= 0) return UVECTORS.Zero;
            return UARC.ArcIntersections(aArc.Structure, bArc, aArcIsInfinite, bArcIsInfinite);
        }

        public static uopArc FromArc(iArc aArc, bool bReturnClone = false, double? aStartAngle = null, double? aEndAngle = null)
        {
            if (aArc == null) return null;
            uopArc _rVal = null;
            if (aArc is uopArc)
                _rVal= bReturnClone ? new uopArc((uopArc)aArc) : (uopArc)aArc;
            else
                _rVal= new uopArc(aArc);

            if (aStartAngle.HasValue || aEndAngle.HasValue)
            {
                double sa = aStartAngle.HasValue ? mzUtils.NormAng(aStartAngle.Value, false, true, true) : _rVal.StartAngle;
                double ea = aEndAngle.HasValue ? mzUtils.NormAng(aEndAngle.Value, false, true, true) : _rVal.EndAngle;
                mzUtils.SortTwoValues(true, ref sa, ref ea);
                _rVal.StartAngle = sa;
                _rVal.EndAngle = ea;
            }

            return _rVal;

        }

     
        #endregion  Shared Methods
    }
}
