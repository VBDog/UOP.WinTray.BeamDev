using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UOP.DXFGraphics;
using UOP.WinTray.Projects.Interfaces;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;

namespace UOP.WinTray.Projects
{
    public class uopSegments : List<iSegment>, IEnumerable<iSegment>, ICloneable
    {
        public uopSegments() { }

        #region Constructors
        public uopSegments(IEnumerable<uopLine> aLines)
        {
            if (aLines == null) return;
            foreach (uopLine aLine in aLines)
                Add(aLine);
        }
        public uopSegments(IEnumerable<uopArc> aArcs)
        {
            if (aArcs == null) return;
            foreach (uopArc aArc in aArcs)
                Add(aArc);
        }

        internal uopSegments(USEGMENTS aSegments)
        {
            for (int i = 1; i <= aSegments.Count; i++)
            {
                USEGMENT seg = aSegments.Item(i);
                if (seg.IsArc)
                {
                    Add(new uopArc(seg.ArcSeg));
                }
                else
                {
                    Add(new uopLine(seg.LineSeg));
                }
            }

        }

        public uopSegments(IEnumerable<iSegment> aSegments, bool bCloneMembers = true, bool bDontCopyMembers = false)
        {
            if (aSegments == null || bDontCopyMembers) return;

            foreach (iSegment seg in aSegments)
            {
                if (seg == null) continue;
                if (seg.IsArc)
                {
                    uopArc arc = (uopArc)seg;
                    Add(bCloneMembers ? new uopArc(arc) : arc);
                }
                else
                {
                    uopLine line = (uopLine)seg;
                    Add(bCloneMembers ? new uopLine(line) : line);
                }
            }

        }


        #endregion Constructors

        #region Methods

        public List<iSegment> ToList(bool bGetClones = false)
        {
            List<iSegment> _rVal = new List<iSegment>();
            foreach(var item in this) _rVal.Add(bGetClones ? item.Clone()  : item);
            return _rVal;
        }
        object ICloneable.Clone() => (object)Clone(false);

        public uopSegments Clone(bool bReturnEmpty = false) => new uopSegments(this, bCloneMembers: true, bDontCopyMembers: bReturnEmpty);

        public bool ContainsVector(iVector aVector, out uopVectors rIntercepts, bool bOnIsIn = true, int aPrecis = 3, double? aTestOrd = null)
        {
       

            rIntercepts = uopVectors.Zero;
            if (aVector == null) return false;

            uopVector v1 = uopVector.FromVector(aVector);
            if (bOnIsIn)
            {
                //check to see if the vector lies on one of the segments within a fudge factor
                foreach (iSegment seg in this)
                {
                    if (v1.LiesOn(seg, aPrecis: aPrecis))
                    {
                       if( rIntercepts.Add(v1, bNoDupes: true, aNoDupesPrecis: aPrecis) != null) 
                            return true;
                    }
                }
                
            }

            double tord = !aTestOrd.HasValue ? double.MaxValue / 2 : aTestOrd.Value;
            //get the intersections of a ray from the point to infinity X with the segments
            //an odd number of intercepts means the point lies within then segments
            uopLine iLine = new uopLine(new uopVector(v1), new uopVector(tord, v1.Y));

            foreach (iSegment seg in this)
            {
          
                uopVectors ips = iLine.Intersections(seg, aSegIsInfinite: false, aLineIsInfinite: false);
                if (ips.Count <= 0) continue;

                foreach(uopVector ip in ips)
                {
                    if (ip.IsEqual(v1, 3)) continue;
                    rIntercepts.Add(ip, bNoDupes: true,aNoDupesPrecis: aPrecis);
                }
            }


            return (rIntercepts.Count > 0) && mzUtils.IsOdd(rIntercepts.Count);
        }

        internal bool ContainsVector(UVECTOR aVector, out uopVectors rIntercepts, bool bOnIsIn = true, int aPrecis = 3, double? aTestOrd = null)
        {
            USEGMENT bSeg;

            rIntercepts = uopVectors.Zero;

            if (bOnIsIn)
            {
                //check to see if the vector lies on one of the segments within a fudge factor
                foreach (iSegment seg in this)
                {
                    bSeg = new USEGMENT(seg);

                    if (aVector.LiesOn(bSeg, aPrecis: aPrecis))
                    {
                        if (!rIntercepts.ContainsVector(aVector, aPrecis))
                            rIntercepts.Add(aVector);

                    }
                }
                if (rIntercepts.Count > 0) return true;
            }

            double tord = !aTestOrd.HasValue ? double.MaxValue / 2 : aTestOrd.Value;
            //get the intersections of a ray from the point to infinity X with the segments
            //an odd number of intercepts means the point lies within then segments
            ULINE iLine = new ULINE(new UVECTOR(aVector), new UVECTOR(tord, aVector.Y));

            foreach (iSegment seg in this)
            {
                bSeg = new USEGMENT(seg);
                UVECTORS ips = iLine.Intersections(bSeg, aSegIsInfinite: false, aLineIsInfinite: false);
                if (ips.Count <= 0) continue;


                for (int j = ips.Count; j >= 1; j--)
                {
                    UVECTOR u1 = ips.Item(j);
                    //don't count any intersections if they already found or if they lie on the test point
                    bool aFlg = !u1.Compare(aVector, 3);
                    if (aFlg)
                    {
                        if (rIntercepts.ContainsVector(u1)) aFlg = false;

                        if (aFlg)
                            rIntercepts.Add(u1);

                    }
                }
            }


            return (rIntercepts.Count > 0) && mzUtils.IsOdd(rIntercepts.Count);
        }

        public int ArcCount(double? aRadius = null, int? aPrecis = 4) => ArcSegments(bGetClones: false, aRadius, aPrecis).Count;
        public int LineCount(double? aLength = null, int? aPrecis = 4) => LineSegments(bGetClones: false, aLength, aPrecis).Count;

    
        public uopVectors ExtentPoints(int? aCurveDivisions = null, bool bNoDupes = true,int aPrecis = 4) => uopSegments.GetExtentPoints(this, aCurveDivisions, bNoDupes, aPrecis);
            
        public uopLines LineSegments(bool bGetClones = false, double? aLength = null, int? aPrecis = 4)
        {

            uopLines _rVal = new uopLines(FindAll((x) => !x.IsArc).OfType<uopLine>().ToList(), bAddClones: bGetClones);
            if (!aLength.HasValue) return _rVal;
            int prec = !aPrecis.HasValue ? 15 : mzUtils.LimitedValue(aPrecis.Value, 0, 15);
            return new uopLines(_rVal.FindAll(x => Math.Round(x.Length, prec) == Math.Round(aLength.Value, prec)), bAddClones: false);

        }
        public List<uopArc> ArcSegments(bool bGetClones = false, double? aRadius = null, int? aPrecis = 4)
        {
            List<uopArc> _rVal = new List<uopArc>();
            foreach (iSegment seg in this)
            {
                if (seg.IsArc)
                {
                    uopArc arc = (uopArc)seg;
                    _rVal.Add(bGetClones ? new uopArc(arc) : arc);
                }
            }
            if (!aRadius.HasValue) return _rVal;
            int prec = !aPrecis.HasValue ? 15 : mzUtils.LimitedValue(aPrecis.Value, 0, 15);
            return _rVal.FindAll(x => Math.Round(x.Radius, prec) == Math.Round(aRadius.Value, prec));

        }
        public uopVectors IntersectionPoints( IEnumerable<iSegment> aSegments, bool aSegmentsAreInfinite = false, bool bSegmentsAreInfinite = false, bool bNoDupes = false,int aNoDupesPrecis = 4)
        {
            if (aSegments == null) return uopVectors.Zero;
            uopVectors _rVal = new uopVectors();
            foreach(var seg1 in this)
            {
                foreach(var seg2 in aSegments)
                {
                    _rVal.Append(uopSegments.SegmentIntersectionPoints(seg1, seg2, aSegmentsAreInfinite, bSegmentsAreInfinite,bNoDupes,aNoDupesPrecis), bNoDupes: bNoDupes, aNoDupesPrecis: aNoDupesPrecis);
         
                }
            }

            return _rVal;

        }

        public uopVectors IntersectionPoints(bool bNoDupes = false, int aNoDupesPrecis = 4)
        {
            uopVectors _rVal = uopVectors.Zero;
            for (int i = 1; i <= Count; i++)
            {
                iSegment seg1 = this[i - 1];
                uopLine line1 = !seg1.IsArc ? (uopLine)seg1 : null;
                uopArc arc1 = seg1.IsArc ? (uopArc)seg1 : null;

                for (int j = 1; j <= Count; j++)
                {
                    if (j == i) continue;
                    uopVectors ips = null;
                    iSegment seg2 = this[j - 1];
              
                    _rVal.Append(uopSegments.SegmentIntersectionPoints(seg1, seg2), bNoDupes: bNoDupes, aNoDupesPrecis: aNoDupesPrecis);
                }
            }
                return _rVal;

        }

        #endregion Methods

        #region Shared Methods


        public static bool Compare(iSegment A, iSegment B, int? aPrecis = 4)
        {
            if (A == null && B == null) return true;
            if (A == null || B == null) return false;
            if (A == B) return true;
            if (A.IsArc != B.IsArc) return false;
            int precis = !aPrecis.HasValue ? 15 : mzUtils.LimitedValue(aPrecis.Value, 0, 15);
            if (A.IsArc)
            { 
                if (Math.Round(A.Radius, precis) != Math.Round(B.Radius, precis)) return false;
                if (Math.Round(A.Arc.SpannedAngle, precis) != Math.Round(B.Arc.SpannedAngle, precis)) return false;
            }
            else
            {
                if (Math.Round(A.Length, precis) != Math.Round(B.Length, precis)) return false;
            }
            
            return true;
        }


        public static uopVectors SegmentIntersectionPoints(iSegment aSegment, iSegment bSegment, bool aSegmentIsInfinite = false, bool bSegmentIsInfinite = false, bool bNoDupes = false, int aNoDupesPrecis = 4)
        {
            if (aSegment == null || bSegment == null) return uopVectors.Zero;

            uopVectors _rVal = uopVectors.Zero;
       
                uopLine line1 = !aSegment.IsArc ? (uopLine)aSegment : null;
                uopArc arc1 = aSegment.IsArc ? (uopArc)aSegment : null;

            uopLine line2 = !bSegment.IsArc ? (uopLine)bSegment : null;
            uopArc arc2 = bSegment.IsArc ? (uopArc)bSegment : null;


            if (line1 != null)
            {
                if(line2 != null)
                    _rVal.Add(line1.IntersectionPt(line2,aSegmentIsInfinite,bSegmentIsInfinite), bNoDupes: bNoDupes, aNoDupesPrecis: aNoDupesPrecis);
                else
                    _rVal.Append(line1.Intersections(arc2, bSegmentIsInfinite, aSegmentIsInfinite), bNoDupes: bNoDupes, aNoDupesPrecis: aNoDupesPrecis);

            }
            else
            {
                if (line2 != null)
                    _rVal.Append(arc1.Intersections(line2, bSegmentIsInfinite, aSegmentIsInfinite), bNoDupes: bNoDupes, aNoDupesPrecis: aNoDupesPrecis);
                else
                    _rVal.Append(arc1.Intersections(arc2, bSegmentIsInfinite, aSegmentIsInfinite), bNoDupes: bNoDupes, aNoDupesPrecis: aNoDupesPrecis);

            }
            return _rVal;

        }
        public static iSegment CloneCopy(iSegment aSegment) => aSegment == null ? null : aSegment.Clone();

        public static uopVectors GetExtentPoints(iSegment aSegment, int? aCurveDivisions = null) => aSegment == null ? uopVectors.Zero : aSegment.IsArc ? aSegment.Arc.PhantomPoints(aCurveDivisions, true) : aSegment.Line.EndPoints(bGetClones: true);

        public static uopVectors GetExtentPoints(IEnumerable<iSegment> aSegments, int? aCurveDivisions = null, bool bNoDupes = true, int aNoDupesPrecis = 4)
        {
            if (aSegments == null) return uopVectors.Zero;
            uopVectors _rVal = uopVectors.Zero;

            foreach (var item in aSegments)
            {
                if (item == null) continue;
           
                  _rVal.Append(uopSegments.GetExtentPoints(item, aCurveDivisions), bAddClone: false, bNoDupes: bNoDupes, aNoDupesPrecis: aNoDupesPrecis);

            }

            return _rVal;
        }
        #endregion Shared Methods

    }

}
