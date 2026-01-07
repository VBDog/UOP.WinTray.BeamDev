using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using UOP.DXFGraphics;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Interfaces;
using UOP.WinTray.Projects.Structures;

namespace UOP.WinTray.Projects
{
    public class uopLinePair : ICloneable
    {
        #region Constructors
        
        public uopLinePair() { 
            Line1 = null; 
            Line2 = null; 
            Tag = string.Empty;
            Value = 0; 
            PartIndex = 0; 
            Suppressed = false; 
            IntersectionType1 = uppIntersectionTypes.Undefined;
            IntersectionType2 = uppIntersectionTypes.Undefined;
            Row = 0;
            Col = 0;
            ZoneIndex = 1;
        }

        public uopLinePair(uopLinePair aPair ) 
        {
            Line1 = null;
            Line2 = null;
            Tag = string.Empty;
            Value = 0;
            PartIndex = 0;
            Suppressed = false;
            IntersectionType1 = uppIntersectionTypes.Undefined;
            IntersectionType2 = uppIntersectionTypes.Undefined;
            Row = 0;
            Col = 0;
            ZoneIndex = 0;
            if (aPair == null) return;
            PartIndex = aPair.PartIndex;
            Tag = aPair.Tag;
            Value = aPair.Value;
            IsVirtual = aPair.IsVirtual;
            Suppressed = aPair.Suppressed;
            Row = aPair.Row;
            Col = aPair.Col;
            IntersectionType1 = aPair.IntersectionType1;
            IntersectionType2 = aPair.IntersectionType2;
            ZoneIndex = aPair.ZoneIndex;
            if (aPair.Line1 != null) Line1 = new uopLine(aPair.Line1);
            if (aPair.Line2 != null) Line2 = new uopLine(aPair.Line2);

        }

        internal uopLinePair(ULINEPAIR aPair)
        {
            Line1 = null;
            Line2 = null;
            Tag = string.Empty;
            Value = 0;
            PartIndex = 0;
            Suppressed = false;
            IntersectionType1 = uppIntersectionTypes.Undefined;
            IntersectionType2 = uppIntersectionTypes.Undefined;
            Row = 0;
            Col = 0;
            ZoneIndex = 1;
            PartIndex = aPair.PartIndex;
            Tag = aPair.Tag;
            Value = aPair.Value;
            IsVirtual = aPair.IsVirtual;
            Suppressed = aPair.Suppressed;
            Row = aPair.Row;
            Col = aPair.Col;
            IntersectionType1 = aPair.IntersectionType1;
            IntersectionType2 = aPair.IntersectionType2;
            ZoneIndex = aPair.ZoneIndex;
            if (aPair.Line1.HasValue) Line1 =  new uopLine(aPair.Line1.Value);
            if (aPair.Line2.HasValue) Line2 = new uopLine(aPair.Line2.Value);

        }
        public uopLinePair(iLine aLine1, iLine aLine2)
        {
            IsVirtual = false; Tag = string.Empty;
            Line1 = null;
            Line2 = null;
            Value = 0;
            Suppressed = false;
            IntersectionType1 = uppIntersectionTypes.Undefined;
            IntersectionType2 = uppIntersectionTypes.Undefined;
            Col = 0;
            Row = 0;
            PartIndex = 0;
            ZoneIndex = 1;
            if (aLine1 != null)
            {
                if (aLine1 is uopLine)
                    Line1 = (uopLine) aLine1;
                else
                    Line1 = new uopLine(aLine1);
            }
            if (aLine2 != null)
            {
                if (aLine2 is uopLine)
                    Line2 = (uopLine)aLine2;
                else
                    Line2 = new uopLine(aLine2);
            }
        }

      
     
        internal uopLinePair(ULINE aLine, ULINE bLine)
        {
            Line1 = new uopLine(aLine); Line2 = new uopLine(bLine);
            Tag = string.Empty;
            Value = 0;
            PartIndex = 0;
            Suppressed = false;
            IntersectionType1 = uppIntersectionTypes.Undefined;
            IntersectionType2 = uppIntersectionTypes.Undefined;
            Row = 0;
            Col = 0;
            ZoneIndex = 1;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// a client assignable tag 
        /// </summary>
        public string Tag { get;set; }

  
        /// <summary>
        /// the first of the lines pair (can be null)
        /// </summary>
        public uopLine Line1 { get; set; }

        /// <summary>
        /// the second of the lines pair (can be null)
        /// </summary>
        public uopLine Line2 { get; set; }

        

        /// <summary>
        /// a client assignable virtual marker 
        /// </summary>
        public bool IsVirtual { get; set; }

        /// <summary>
        /// a client assignable suppressed marker 
        /// </summary>
        public bool Suppressed { get; set; }

        public int Row { get; set; }
        public int Col { get; set; }

        /// <summary>
        /// a client assignablel value 
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// a client assignable index 
        /// </summary>
        public int PartIndex { get; set; }

        public int ZoneIndex { get; set; }


        public double X => BoundsV().X;
        public double Y => BoundsV().Y;

        public uopVector Center 
        { 
            get => new uopVector(BoundsV().Center); 
            set 
            {
                UVECTOR curC = BoundsV().Center;
                UVECTOR newC = new UVECTOR(value);
                UVECTOR trans = newC - curC;
                Move(trans.X, trans.Y);

            }
        }

        public int Count
        {
            get
            {
                int _rVal = 0;
                if (Line1 != null) _rVal++;
                if (Line2 != null) _rVal++;
                return _rVal;
            }
        }

        public uppIntersectionTypes IntersectionType1 { get; set; }
        public uppIntersectionTypes IntersectionType2 { get; set; }

        /// <summary>
        /// returns the sum of the two line lengths
        /// </summary>
        public double TotalLength
        {
            get
            {
                bool do1 = Line1 != null;
                bool do2 = Line2 != null;
                if (do1 && do2)
                {
                    return Line1.Length +  Line2.Length;
                }
                else if (do1)
                {
                    return Line1.Length;
                }
                else if (do2)
                {
                    return Line2.Length;
                }

                return 0;
            }
        }

        /// <summary>
        /// returns the greater of the two line lengths
        /// </summary>
        public double MaxLength
        {
            get
            {
                bool do1 = Line1 != null;
                bool do2 = Line2 != null;
                if (do1 && do2)
                {
                    return Math.Max(Line1.Length, Line2.Length);
                }
                else if (do1)
                {
                    return Line1.Length;
                }
                else if (do2)
                {
                    return Line2.Length;
                }

                return 0;
            }
        }

        /// <summary>
        /// returns the lesser of the two line lengths
        /// </summary>
        public double MinLength

        {
            get
            {
                bool do1 = Line1 != null;
                bool do2 = Line2 != null;
                if (do1 && do2)
                {
                    return Math.Min(Line1.Length, Line2.Length);
                }
                else if (do1)
                {
                    return Line1.Length;
                }
                else if (do2)
                {
                    return Line2.Length;
                }
                return 0;

            }


        }
        #endregion Properties

        #region Methods

        public List<iSegment> LineSegments(bool bGetClones = false)
        {
            List<iSegment> _rVal = new List<iSegment>();
            if (Line1 != null) _rVal.Add(!bGetClones ? Line1 : new uopLine(Line1));
            if (Line2 != null) _rVal.Add(!bGetClones ? Line2 : new uopLine(Line2));

            return _rVal;
        }

        /// <summary>
        /// returns true if both lines are defined
        /// </summary>
        /// <param name="bBothLines">if false, true is returned if one line is defined</param>
        public bool IsDefined(bool bBothLines = true) => bBothLines ? Line1 != null && Line2 != null : Line1 != null || Line2 != null;
        /// <summary>
        /// extends this line to the passed line
        /// </summary>
        /// <param name="aLine">the subject line</param>
        /// <param name="bTrimTo">if the lines intersect and this flag is true the line will be trimmed back to the passed line  </param>
        /// <returns></returns>
        internal bool ExtendTo(ULINE aLine, bool bTrimTo = false)
        {

            bool t1 = Line1 != null ? Line1.ExtendTo(aLine,bTrimTo) : false;
            bool t2 = Line2 != null ? Line2.ExtendTo(aLine, bTrimTo) : false;
            return (t1 || t2);


        }


        /// <summary>
        /// extends this line to the passed line
        /// </summary>
        /// <param name="aLine">the subject line</param>
        /// <param name="bTrimTo">if the lines intersect and this flag is true the line will be trimmed back to the passed line  </param>
        /// <returns></returns>
        public bool ExtendTo(iLine aLine, bool bTrimTo = false)
        {
            if(aLine == null) return false;
            bool t1 = Line1 != null ? Line1.ExtendTo(aLine, bTrimTo) : false;
            bool t2 = Line2 != null ? Line2.ExtendTo(aLine, bTrimTo) : false;
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


        /// <summary>
        /// extends this line to the passed lines
        /// </summary>
        /// <param name="aLines">the subject lines</param>
        /// <param name="bTrimTo">if the lines intersect and this flag is true the line will be trimmed back to the passed line  </param>
        /// <returns></returns>

        public bool ExtendTo(uopLinePair aLines, bool bTrimTo = false)
        {
            if(aLines == null) return false;
            bool t1 = aLines.Line1 != null ? ExtendTo(aLines.Line1, bTrimTo) : false;
            bool t2 = aLines.Line2 != null ? ExtendTo(aLines.Line2, bTrimTo) : false;
            return (t1 || t2);
        }

        /// <summary>
        /// the rectangle that encloses the end points of the lines
        /// </summary>
        internal URECTANGLE BoundsV() => EndPoints(false).Bounds;
        /// <summary>
        /// the rectangle that encloses the end points of the lines
        /// </summary>
        public uopRectangle Bounds() => new uopRectangle(BoundsV());


        object ICloneable.Clone() => Clone();

        public uopLinePair Clone() => new uopLinePair(this);

        public override string ToString() => string.IsNullOrWhiteSpace(Tag) ? $"uopLinePair [{Count}]  Col:{Col} Row:{Row}" : $"uopLinePair [{Count}] - {Tag}  Col:{Col} Row:{Row} Zone:{ZoneIndex}";

        /// <summary>
        /// returns the londer o the start and end point connectors
        /// </summary>
        /// <returns></returns>
        public uopLine LongestConnector()
        {
            uopLine l1 = StartPtConnector();
            uopLine l2 = EndPtConnector();
            if (l1 == null && l2 == null) return new uopLine();
            if (l1 != null && l2 == null) return l1;
            if (l1 == null && l2 != null) return l2;

            return l1.Length >= l2.Length ? l1 : l2;
        }
        /// <summary>
        /// returns the shorter of the start and end point connectors
        /// </summary>
        /// <returns></returns>
        public uopLine ShortestConnector()
        {
            uopLine l1 = StartPtConnector();
            uopLine l2 = EndPtConnector();
            if (l1 == null && l2 == null) return new uopLine();
            if (l1 != null && l2 == null) return l1;
            if (l1 == null && l2 != null) return l2;
            return l1.Length <= l2.Length ? l1 : l2;
        }

        // <summary>
        /// the line that connects the start pt of the first line to the start pt of the second line
        /// </summary>
        /// <returns></returns>
        public uopLine StartPtConnector() => !IsDefined() ?null : new uopLine(Line1.sp, Line2.sp);
        // <summary>
        /// the line that connects the end pt of the first line to the end pt of the second line
        /// </summary>
        /// <returns></returns>
        public uopLine EndPtConnector() => !IsDefined() ? null : new uopLine(Line1.ep, Line2.ep);

        /// <summary>
        /// returns a pair containing the StartPtConnector and EndPtConnector
        /// </summary>
        /// <returns></returns>
        public uopLinePair Connectors(bool bSwap = false,bool bInvert = false) 
           {

            uopLinePair _rVal = Clone(); // to inherit properties
            _rVal.Line1 = StartPtConnector();
            _rVal.Line2 = EndPtConnector();
            if (bSwap) _rVal.Swap();
            if(bInvert) _rVal.Invert();
            return _rVal;

        }
    /// <summary>
    /// returns just the start and end points of the defined lines
    /// </summary>
    /// <returns></returns>
    public uopVectors EndPoints(bool bInvertLine2 = false)
        {
            uopVectors v = new uopVectors();
            if (Line1 != null) v.AddRange(new uopVectors( Line1.EndPoints(), bCloneMembers: false));
            if (Line2 != null)
            {
               if(bInvertLine2)
                    v.AddRange(Line2.EndPoints().Reversed(bGetClones:false));
               else
                    v.AddRange(Line2.EndPoints());
            }
                    return v;
        }

        /// <summary>
        /// returns the first line of the pair that has a designated side property equal to the passed side value
        /// </summary>
        /// <param name="aSide"></param>
        /// <param name="bGetClone"></param>
        /// <returns></returns>
        public uopLine GetSide(uppSides aSide, bool bGetClone = false, uopLine aDefault = null)
        {
            if (Line1 != null)
            {
                if (Line1.Side == aSide) return bGetClone ? Line1.Clone() : Line1;
            }
            if (Line2 != null)
            {
                if (Line2.Side == aSide) return bGetClone ? Line2.Clone() : Line2;
            }
     
            return aDefault;


        }

        public List<dxeLine> ToDXFLines(dxfDisplaySettings aDisplay = null)
        {
           List < dxeLine > _rVal = new List<dxeLine> ();

           if (Line1 != null) _rVal.Add(Line1.ToDXFLine(aDisplay));
            if (Line2 != null) _rVal.Add(Line2.ToDXFLine(aDisplay));

            return _rVal;
        }
        public void SetSide(uppSides aFromSide, uppSides aToSide, uppSides? aFromSide2 = null, uppSides? aToSide2 = null)
        {
            uopLine l1 = GetSide(aFromSide);
            if (l1 != null)  l1.Side = aToSide; 
            if(aFromSide2.HasValue && aToSide2.HasValue)
            {
                uopLine l2 = GetSide(aFromSide2.Value);
                if (l2 != null)  l2.Side =  aToSide2.Value; 
            }
        }

        public void Move(double aX = 0, double aY = 0)
        {
            if (Line1 != null)  Line1.Move(aX, aY);
            if (Line2 != null) Line2.Move(aX, aY);

        }
        
        public uopLinePair Moved(double aX = 0, double aY = 0)
        {
            uopLinePair _rVal = new uopLinePair(this);
            _rVal.Move(aX, aY);
            return _rVal;
        }
      
        /// <summary>
        /// switches the two lines
        /// </summary>
        public void Swap()
        {
            uopLine l1 = Line1;
            Line1 = Line2;
            Line2 = l1;
        }

        /// <summary>
        /// inverts the lines.  If an index is pased ( 1 or 2 ) just the indicated line is inverted.
        /// </summary>
        /// <param name="aIndex"></param>
        public void Invert(int? aIndex = null)
        {
            bool do1 = Line1 != null;
            if (do1 && aIndex.HasValue) do1 = aIndex.Value == 1;
            bool do2 = Line2 != null;
            if (do2 && aIndex.HasValue) do2 = aIndex.Value == 2;
            if (do1) Line1.Invert();
            if (do2) Line2.Invert();

        }

        /// <summary>
        /// moves the lines.  If an index is pased ( 1 or 2 ) just the indicated line is moved.
        /// </summary>
        /// <param name="aIndex"></param>
        public void Move(double aDX = 0, double aDY = 0,  int? aIndex = null)
        {
            bool do1 = Line1 != null;
            if (do1 && aIndex.HasValue) do1 = aIndex.Value == 1;
            bool do2 = Line2 != null;
            if (do2 && aIndex.HasValue) do2 = aIndex.Value == 2;
            if (do1) Line1.Move(aDX,aDY);
            if (do2) Line2.Move(aDX, aDY); ;

        }

        /// <summary>
        /// mirrors the lines.  If an index is pased ( 1 or 2 ) just the indicated line is mirrors.
        /// </summary>
        /// <param name="aIndex"></param>
        public void Mirror(double? aX , double? aY , int? aIndex = null)
        {
            if (!aX.HasValue && !aY.HasValue) return;
            bool do1 = Line1 != null;
            if (do1 && aIndex.HasValue) do1 = aIndex.Value == 1;
            bool do2 = Line2 != null;
            if (do2 && aIndex.HasValue) do2 = aIndex.Value == 2;
            if (do1) Line1.Mirror(aX, aY);
            if (do2) Line2.Mirror(aX, aY); ;

        }


        /// <summary>
        /// mirrors the lines.  If an Side is passed just the indicated line is mirrored.
        /// </summary>
        /// <param name="aSide"></param>
        public void MirrorSide(double? aX, double? aY, uppSides? aSide = null)
        {
            if (!aX.HasValue && !aY.HasValue) return;
            bool do1 = Line1 != null;
            if (do1 && aSide.HasValue) do1 = Line1.Side == aSide;
            bool do2 = Line2 != null;
            if (do2 && aSide.HasValue) do2 = Line2.Side == aSide;
            if (do1) Line1.Mirror(aX, aY);
            if (do2) Line2.Mirror(aX, aY); ;

        }

        /// <summary>
        /// moves the lines orthogonally.  
        /// </summary>
        /// <param name="aIndex"></param>
        public void MoveOrtho(double? aLine1Distance = null, double? aLine2Distance = null)
        {
            bool do1 = Line1 != null;
            if (do1 && !aLine1Distance.HasValue) do1 = false;
            bool do2 = Line2 != null;
            if (do2 && !aLine2Distance.HasValue) do1 = false;
            if (do1) Line1.MoveOrtho(aLine1Distance.Value);
            if (do2) Line2.MoveOrtho(aLine2Distance.Value);

        }

        /// <summary>
        /// returns the sum of the lengths of the lines. If a side is passed, just the indicated lines length is returned.
        /// </summary>
        /// <param name="aSide"></param>
        public double SideLength(uppSides? aSide= null)
        {
            double _rVal = 0;
            bool do1 = Line1 != null;
            if (do1 && aSide.HasValue) do1 = Line1.Side == aSide.Value;
            bool do2 = Line2 != null;
            if (do2 && aSide.HasValue) do2 = Line2.Side == aSide.Value;

            if (do1) _rVal += Line1.Length;
            if (do2) _rVal += Line2.Length;
            return _rVal;
        }
        /// <summary>
        /// returns the sum of the lengths of the lines. If an index is passed ( 1 or 2 ) just the indicated lines length is returned.
        /// </summary>
        /// <param name="aIndex"></param>

        public double Length(int aIndex)
        {
            double _rVal = 0;
            bool do1 = Line1 != null;
            if (do1 && aIndex>0) do1 = aIndex == 1;
            bool do2 = Line2 != null;
            if (do2 && aIndex > 0) do2 = aIndex == 2;

            if (do1) _rVal += Line1.Length;
            if (do2) _rVal += Line2.Length;
            return _rVal;
        }
        public double Length(uppSides aSide = uppSides.Undefined)
        {
            double _rVal = 0;
            bool do1 = Line1 != null;
            if (do1 && aSide != uppSides.Undefined) do1 = Line1.Side == aSide;
            bool do2 = Line2 != null;
            if (do2 && aSide != uppSides.Undefined) do2 = Line2.Side == aSide;
            if (do1) _rVal += Line1.Length;
            if (do2) _rVal += Line2.Length;
            return _rVal;
        }

      

        
        /// <summary>
        /// returns true if one of the lines is markerked with the passed side value and is marked as suppressed.
        /// </summary>
        /// <param name="aSide"></param>
        public bool SideIsSuppressed(uppSides aSide, bool aUndefinedValue = false)
        {
            uopLine l1 = GetSide(aSide);
            if(l1 == null) return aUndefinedValue;
            return l1.Suppressed;
        }

        /// <summary>
        /// returns true if one of the lines is markerked with the passed side value.
        /// </summary>
        /// <param name="aSide"></param>
        public bool SideIsIsDefined(uppSides aSide) => GetSide(aSide) != null;

        public void Rotate(iVector aOrigin, double aAngle, bool bInRadians = false) 
        {
            Line1?.Rotate(aOrigin, aAngle, bInRadians); Line2?.Rotate(aOrigin, aAngle, bInRadians);
        }

        public uopVectors Intersections(IEnumerable<iSegment> aSegments, bool aSegsAreInfinite = false, bool aLinesAreInfinite = false, int? aIndex = null)
        {
            uopVectors _rVal = new uopVectors();
            bool do1 = Line1 != null;
            if (do1 && aIndex.HasValue) do1 = aIndex.Value == 1;
            bool do2 = Line2 != null;
            if (do2 && aIndex.HasValue) do2 = aIndex.Value == 2;

            if (!do1 && !do2) return _rVal; // no lines to check


            foreach (iSegment aSegment in aSegments)
            {
                if (aSegment == null) continue;

               

                if (!aSegment.IsArc)
                {
                    if (do1)
                    {
                        uopVector ip = Line1.IntersectionPt(aSegment.Line, out bool PRL, out bool COINC, out bool ON1, out bool ON2, out bool EXST, out double T1, out double T2);
                        bool keep = ip != null;
                        if (!aSegsAreInfinite && !ON2) keep = false;
                        if (!aLinesAreInfinite && !ON1) keep = false;
                        if (keep) _rVal.Add(ip);
                    }
                    if (do2)
                    {
                        uopVector ip = Line2.IntersectionPt(aSegment.Line, out bool PRL, out bool COINC, out bool ON1, out bool ON2, out bool EXST, out double T1, out double T2);
                        bool keep = ip != null;
                        if (!aSegsAreInfinite && !ON2) keep = false;
                        if (!aLinesAreInfinite && !ON1) keep = false;
                        if (keep) _rVal.Add(ip);
                    }

                }
                else
                {
                    if (do1)
                    {
                        _rVal.Append(Line1.Intersections(aSegment.Arc, aSegsAreInfinite, aLinesAreInfinite));
                    }
                    if (do2)
                    {
                        _rVal.Append(Line2.Intersections(aSegment.Arc, aSegsAreInfinite, aLinesAreInfinite));
                    }
                }
          
          
            }
            return _rVal;
        }

        public uopVectors Intersections( uopLinePair aPair, bool aThisPairIsInfinite = false, bool aPairIsInfinite = false, int? aIndex = null)
        {
            uopVectors _rVal = new uopVectors();
            if(aPair==null) return _rVal; // no lines to check
            bool do1 = Line1 != null;
            if (do1 && aIndex.HasValue) do1 = aIndex.Value == 1;
            bool do2 = Line2 != null;
            if (do2 && aIndex.HasValue) do2 = aIndex.Value == 2;

            if (!do1 && !do2) return _rVal; // no lines to check
            uopVector ip = null;
            bool keep = false;
            bool ON1 = false;
            bool ON2= false;

            if (do1)
            {
                ip = Line1.IntersectionPt(aPair.Line1, out _, out _, out ON1, out ON2, out _, out _, out _);
                keep = ip != null;
                if (!aThisPairIsInfinite && !ON1) keep = false;
                if (!aPairIsInfinite && !ON2) keep = false;
                if (keep) _rVal.Add(ip);

                ip = Line1.IntersectionPt(aPair.Line2, out _, out _, out ON1, out ON2, out _, out _, out _);
                keep = ip != null;
                if (!aThisPairIsInfinite && !ON1) keep = false;
                if (!aPairIsInfinite && !ON2) keep = false;
                if (keep) _rVal.Add(ip);

            }
            if (do2)
            {
                ip = Line2.IntersectionPt(aPair.Line1, out _, out _, out ON1, out ON2, out _, out _, out _);
                keep = ip != null;
                if (!aThisPairIsInfinite && !ON1) keep = false;
                if (!aPairIsInfinite && !ON2) keep = false;
                if (keep) _rVal.Add(ip);

                ip = Line2.IntersectionPt(aPair.Line2, out _, out _, out ON1, out ON2, out _, out _, out _);
                keep = ip != null;
                if (!aThisPairIsInfinite && !ON1) keep = false;
                if (!aPairIsInfinite && !ON2) keep = false;
                if (keep) _rVal.Add(ip);

            }


            return _rVal;
        }
        public uopVectors Intersections(uopLine aLine, bool aThisPairIsInfinite = false, bool aLineIsInfinite = false, int? aIndex = null)
        {
            uopVectors _rVal = new uopVectors();
            if (aLine == null) return _rVal; // no lines to check
            bool do1 = Line1 != null;
            if (do1 && aIndex.HasValue) do1 = aIndex.Value == 1;
            bool do2 = Line2 != null;
            if (do2 && aIndex.HasValue) do2 = aIndex.Value == 2;

            if (!do1 && !do2) return _rVal; // no lines to check
            uopVector ip = null;
            bool keep = false;
            bool ON1 = false;
            bool ON2 = false;

            if (do1)
            {
                ip = Line1.IntersectionPt(aLine, out _, out _, out ON1, out ON2, out _, out _, out _);
                keep = ip != null;
                if (!aThisPairIsInfinite && !ON1) keep = false;
                if (!aLineIsInfinite && !ON2) keep = false;
                if (keep) _rVal.Add(ip);

            }
            if (do2)
            {
                ip = Line2.IntersectionPt(aLine, out _, out _, out ON1, out ON2, out _, out _, out _);
                keep = ip != null;
                if (!aThisPairIsInfinite && !ON1) keep = false;
                if (!aLineIsInfinite && !ON2) keep = false;
                if (keep) _rVal.Add(ip);

            }


            return _rVal;
        }
        public uopVectors Intersections(uopLine aLine, uppSides aSide, bool aThisPairIsInfinite = false, bool aLineIsInfinite = false)
        {
            uopVectors _rVal = new uopVectors();
            if(aLine == null) return _rVal; // no lines to check
            bool do1 = Line1 != null;
            if (do1) do1 = aSide == Line1.Side;
            bool do2 = Line2 != null;
            if (do2) do2 = aSide == Line2.Side;

            if (!do1 && !do2) return _rVal; // no lines to check
            uopVector ip = null;
            bool keep = false;
            bool ON1 = false;
            bool ON2 = false;

            if (do1)
            {
                ip = Line1.IntersectionPt(aLine, out _, out _, out ON1, out ON2, out _, out _, out _);
                keep = ip != null;
                if (!aThisPairIsInfinite && !ON1) keep = false;
                if (!aLineIsInfinite && !ON2) keep = false;
                if (keep) _rVal.Add(ip);

            }
            if (do2)
            {
                ip = Line2.IntersectionPt(aLine, out _, out _, out ON1, out ON2, out _, out _, out _);
                keep = ip != null;
                if (!aThisPairIsInfinite && !ON1) keep = false;
                if (!aLineIsInfinite && !ON2) keep = false;
                if (keep) _rVal.Add(ip);

            }


            return _rVal;
        }

        public uopVectors Intersections(uopLinePair aPair, uppSides aSide, bool aThisPairIsInfinite = false, bool aPairIsInfinite = false)
        {
            uopVectors _rVal = new uopVectors();
            if (aPair == null) return _rVal; // no lines to check
            bool do1 = Line1 != null;
            if (do1) do1 = aSide == Line1.Side;
            bool do2 = Line2 != null;
            if (do2) do2 = aSide == Line2.Side;


            if (!do1 && !do2) return _rVal; // no lines to check
            uopVector ip = null;
            bool keep = false;
            bool ON1 = false;
            bool ON2 = false;

            if (do1)
            {
                ip = Line1.IntersectionPt(aPair.Line1, out _, out _, out ON1, out ON2, out _, out _, out _);
                keep = ip != null;
                if (!aThisPairIsInfinite && !ON1) keep = false;
                if (!aPairIsInfinite && !ON2) keep = false;
                if (keep) _rVal.Add(ip);

                ip = Line1.IntersectionPt(aPair.Line2, out _, out _, out ON1, out ON2, out _, out _, out _);
                keep = ip != null;
                if (!aThisPairIsInfinite && !ON1) keep = false;
                if (!aPairIsInfinite && !ON2) keep = false;
                if (keep) _rVal.Add(ip);

            }
            if (do2)
            {
                ip = Line2.IntersectionPt(aPair.Line1, out _, out _, out ON1, out ON2, out _, out _, out _);
                keep = ip != null;
                if (!aThisPairIsInfinite && !ON1) keep = false;
                if (!aPairIsInfinite && !ON2) keep = false;
                if (keep) _rVal.Add(ip);

                ip = Line2.IntersectionPt(aPair.Line2, out _, out _, out ON1, out ON2, out _, out _, out _);
                keep = ip != null;
                if (!aThisPairIsInfinite && !ON1) keep = false;
                if (!aPairIsInfinite && !ON2) keep = false;
                if (keep) _rVal.Add(ip);

            }


            return _rVal;
        }
        public uopVectors Intersections(IEnumerable<iSegment> aSegments, uppSides aSide , bool aSegsAreInfinite = false, bool aLinesAreInfinite = false )
        {
            uopVectors _rVal = new uopVectors();
            bool do1 = Line1 != null;
            if (do1 ) do1 = aSide == Line1.Side;
            bool do2 = Line2 != null;
            if (do2 ) do2 = aSide == Line2.Side;

            if(!do1 && !do2) return _rVal; // no lines to check

            foreach (iSegment aSegment in aSegments)
            {
                if (aSegment == null) continue;


            
                if (!aSegment.IsArc)
                {
                    if (do1)
                    {
                        uopVector ip = Line1.IntersectionPt(aSegment.Line, out bool PRL, out bool COINC, out bool ON1, out bool ON2, out bool EXST, out double T1, out double T2);
                        bool keep = ip != null;
                        if (!aSegsAreInfinite && !ON2) keep = false;
                        if (!aLinesAreInfinite && !ON1) keep = false;
                        if (keep) _rVal.Add(ip);
                    }
                    if (do2)
                    {
                        uopVector ip = Line2.IntersectionPt(aSegment.Line, out bool PRL, out bool COINC, out bool ON1, out bool ON2, out bool EXST, out double T1, out double T2);
                        bool keep = ip != null;
                        if (!aSegsAreInfinite && !ON2) keep = false;
                        if (!aLinesAreInfinite && !ON1) keep = false;
                        if (keep) _rVal.Add(ip);
                    }

                }
                else
                {
                    if (do1)
                    {
                        _rVal.Append(Line1.Intersections(aSegment.Arc, aSegsAreInfinite, aLinesAreInfinite));
                    }
                    if (do2)
                    {
                        _rVal.Append(Line2.Intersections(aSegment.Arc, aSegsAreInfinite, aLinesAreInfinite));
                    }
                }


            }
            return _rVal;
        }

        #endregion Methods

        #region Shared Methods

        internal static List<uopLine> ToLineList(IEnumerable<ULINEPAIR> aPairs)
        {
            List<uopLine> _rVal = new List<uopLine>();
            if (aPairs == null) return _rVal;
            foreach (ULINEPAIR pair in aPairs) 
            { 
                if(pair.Line1.HasValue) _rVal.Add(new uopLine(pair.Line1.Value));
                if (pair.Line2.HasValue) _rVal.Add(new uopLine(pair.Line2.Value));

            }
            return _rVal;
        }
        internal static List<ULINE> ToULineList(IEnumerable<ULINEPAIR> aPairs)
        {
            List<ULINE> _rVal = new List<ULINE>();
            if (aPairs == null) return _rVal;
            foreach (ULINEPAIR pair in aPairs)
            {
                if (pair.Line1.HasValue) _rVal.Add(pair.Line1.Value);
                if (pair.Line2.HasValue) _rVal.Add(pair.Line2.Value);

            }
            return _rVal;
        }


        internal static List <uopLinePair> FromList(IEnumerable<ULINEPAIR> aPairs)
        {
            List<uopLinePair> _rVal = new List<uopLinePair>();
            if(aPairs == null) return _rVal;
            foreach(ULINEPAIR pair in aPairs) {_rVal.Add(new uopLinePair(pair)); }
            return _rVal;
        }

        public static List<uopLinePair> Copy(List<uopLinePair> aPairs)
        {
            if (aPairs == null) return null;
            List<uopLinePair> _rVal = new List<uopLinePair>();
            foreach (uopLinePair pair in aPairs) { _rVal.Add(pair.Clone()); }
            return _rVal;
        }

        public static uopLinePair CloneCopy(uopLinePair aPair)    => aPair == null ? null : new uopLinePair(aPair);
      
        #endregion Shared Methods


    }

    internal struct ULINEPAIR : ICloneable
    {
        #region Constructors

        public ULINEPAIR(string aTag)
        {
            IsVirtual = false; Line1 = null; Line2 = null; Tag = aTag != null ? aTag: ""; Value = 0; PartIndex =0; Col = 0; Row = 0; Suppressed = false; ZoneIndex = 1;
            IntersectionType1 = uppIntersectionTypes.Undefined;
            IntersectionType2 = uppIntersectionTypes.Undefined;

        }
       
        public ULINEPAIR(ULINEPAIR aPair)
        {
           Line1 = null; Line2 = null; Tag = aPair.Tag; Value = aPair.Value; PartIndex = aPair.PartIndex; Col = aPair.Col; Row = aPair.Row; Suppressed = aPair.Suppressed; IsVirtual = aPair.IsVirtual; ZoneIndex = aPair.ZoneIndex;
            IntersectionType1 = aPair.IntersectionType1;
            IntersectionType2 = aPair.IntersectionType2;
            if (aPair.Line1.HasValue) Line1 = new ULINE(aPair.Line1.Value);
            if (aPair.Line2.HasValue) Line2 = new ULINE(aPair.Line2.Value);
         
        }


        public ULINEPAIR(uopLinePair aPair)
        {
            IsVirtual = false; Line1 = null; Line2 = null; Tag =  ""; Value = 0; PartIndex = 0; Col = 0; Row = 0; Suppressed = false; ZoneIndex = aPair.PartIndex;
            IntersectionType1 = uppIntersectionTypes.Undefined;
            IntersectionType2 = uppIntersectionTypes.Undefined;

            if (aPair == null) return;
            Tag = aPair.Tag; Value = aPair.Value; PartIndex = aPair.PartIndex; Col = aPair.Col; Row = aPair.Row; Suppressed = aPair.Suppressed; IsVirtual = aPair.IsVirtual;
            IntersectionType1 = aPair.IntersectionType1;
            IntersectionType2 = aPair.IntersectionType2;
            Suppressed = aPair.Suppressed;

            if (aPair.Line1 != null) Line1 = new ULINE(aPair.Line1);
            if (aPair.Line2 != null) Line2 = new ULINE(aPair.Line2);
   
        }

        public ULINEPAIR(iLine aLine1, iLine aLine2, string aTag = "" )
        {
            IsVirtual = false; Line1 = null; Line2 = null; Tag = aTag == null ? string.Empty : aTag; ; Value = 0; PartIndex = 0; Col = 0; Row = 0; Suppressed = false; ZoneIndex = 1;
            IntersectionType1 = uppIntersectionTypes.Undefined;
            IntersectionType2 = uppIntersectionTypes.Undefined;

            if (aLine1 != null) Line1 = new ULINE(aLine1);
            if (aLine2 != null) Line2 = new ULINE(aLine2);
          
        }

        public ULINEPAIR(ULINE aLine1, ULINE aLine2, string aTag = "")
        {
            IsVirtual = false;
            Value = 0;
            Tag = aTag == null? string.Empty : aTag;
            Line1 = aLine1;
          Line2 = aLine2;
            PartIndex = 0;
            Suppressed = false;
            Col = 0; Row = 0;
            IntersectionType1 = uppIntersectionTypes.Undefined;
            IntersectionType2 = uppIntersectionTypes.Undefined;
            ZoneIndex = 1;
        }

        public ULINEPAIR(ULINE? aLine1, ULINE? aLine2, string aTag = "", uppSides? aLine1Side = null, uppSides? aLine2Side = null, bool bSuppressed = false)
        {
            IsVirtual = false;
            Value = 0;
            Tag = aTag == null ? string.Empty : aTag;
            Line1 = aLine1;
            Line2 = aLine2;
            PartIndex = 0;
            Suppressed = bSuppressed;
            Col = 0; Row = 0;
            IntersectionType1 = uppIntersectionTypes.Undefined;
            IntersectionType2 = uppIntersectionTypes.Undefined;
            ZoneIndex = 1;
            if (Line1.HasValue)
            {
                ULINE l1 = Line1.Value;
                if(Math.Round(l1.Length,6) == 4)  Line1 = null;
            }

            if (Line2.HasValue)
            {
                ULINE l1 = Line2.Value;
                if (Math.Round(l1.Length, 4) == 0)  Line2 = null;
            }


            if (aLine1Side.HasValue && Line1.HasValue)
            {
                ULINE l1 = Line1.Value;
                l1.Side = aLine1Side.Value;
                Line1 = l1;
            }
            if (aLine2Side.HasValue && Line2.HasValue)
            {
                ULINE l1 = Line2.Value;
                l1.Side = aLine2Side.Value;
                Line2 = l1;
            }
        }
        
        #endregion Constructors

        #region Properties

        /// <summary>
        /// returns true if both lines are defined
        /// </summary>
        public readonly bool IsDefined => Line1.HasValue && Line2.HasValue;

        /// <summary>
        /// a client assignable virtual marker 
        /// </summary>
        public bool IsVirtual { get; set; }

        /// <summary>
        /// a client assignable suppressed marker 
        /// </summary>
        public bool Suppressed { get; set; }

        public int Row { get; set; }
        public int Col { get; set; }

        /// <summary>
        /// a client assignablel value 
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// a client assignable index 
        /// </summary>
        public int PartIndex { get; set; }

        /// <summary>
        /// a client assignable index 
        /// </summary>
        public int ZoneIndex { get; set; }
        /// <summary>
        /// a client assignable tag 
        /// </summary>
        public string Tag { get; set; }
        /// <summary>
        /// the first of the lines pair (can be undefined)
        /// </summary>
        public ULINE? Line1 { get; set; }

        /// <summary>
        /// the second of the lines pair (can be undefined)
        /// </summary>
        public ULINE? Line2 { get; set; }

        public double X => BoundsV().X;
        public double Y => BoundsV().Y;

        internal UVECTOR Center => BoundsV().Center;
        public readonly int Count
        {
            get
            {
                int _rVal = 0;
                if (Line1.HasValue) _rVal++;
                if (Line2.HasValue) _rVal++;
                return _rVal;
            }
        }

        public uppIntersectionTypes IntersectionType1 { get; set; }
        public uppIntersectionTypes IntersectionType2 { get; set; }

        /// <summary>
        /// returns the sum of the two line lengths
        /// </summary>
        public readonly double TotalLength
        {
            get
            {
                bool do1 = Line1 != null;
                bool do2 = Line2 != null;
                if (do1 && do2)
                {
                    return Line1.Value.Length + Line2.Value.Length;
                }
                else if (do1)
                {
                    return Line1.Value.Length;
                }
                else if (do2)
                {
                    return Line2.Value.Length;
                }

                return 0;
            }
        }

        /// <summary>
        /// returns the greater of the two line lengths
        /// </summary>
        public readonly double MaxLength
        {
            get
            {
                bool do1 = Line1.HasValue;
                bool do2 = Line2.HasValue;
                if (do1 && do2)
                {
                    return Math.Max(Line1.Value.Length, Line2.Value.Length);
                }
                else if (do1)
                {
                    return Line1.Value.Length;
                }
                else if (do2)
                {
                    return Line2.Value.Length;
                }

                return 0;
            }
        }

        /// <summary>
        /// returns the lesser of the two line lengths
        /// </summary>
        public readonly double MinLength

        {
            get
            {
                bool do1 = Line1.HasValue;
                bool do2 = Line2.HasValue;
                if (do1 && do2)
                {
                    return Math.Min(Line1.Value.Length, Line2.Value.Length);
                }
                else if (do1)
                {
                    return Line1.Value.Length;
                }
                else if (do2)
                {
                    return Line2.Value.Length;
                }
                return 0;

            }


        }
        
        #endregion Properties

        #region Methods

        /// <summary>
        /// extends this line to the passed line
        /// </summary>
        /// <param name="aLine">the subject line</param>
        /// <param name="bTrimTo">if the lines intersect and this flag is true the line will be trimmed back to the passed line  </param>
        /// <returns></returns>
        public bool ExtendTo(ULINE aLine, bool bTrimTo = false)
        {

            bool t1 = false;
            bool t2 = false;
            if (Line1.HasValue)
            {
                ULINE l1 = Line1.Value;
                t1 = l1.ExtendTo(aLine, bTrimTo);
                Line1 = l1;
            }
            if (Line2.HasValue)
            {
                ULINE l2 = Line2.Value;
                t2 = l2.ExtendTo(aLine, bTrimTo);
                Line2 = l2;
            }
            return (t1 || t2);


        }

        /// <summary>
        /// extends this line to the passed lines
        /// </summary>
        /// <param name="aLines">the subject lines</param>
        /// <param name="bTrimTo">if the lines intersect and this flag is true the line will be trimmed back to the passed line  </param>
        /// <returns></returns>

        public bool ExtendTo(ULINEPAIR aLines, bool bTrimTo = false)
        {
            bool t1 = aLines.Line1.HasValue ? ExtendTo(aLines.Line1.Value, bTrimTo) : false;
            bool t2 = aLines.Line2.HasValue ? ExtendTo(aLines.Line2.Value, bTrimTo) : false;
            return (t1 || t2);
        }

        /// <summary>
        /// the rectangle that encloses the end points of the lines
        /// </summary>
        internal URECTANGLE BoundsV() => EndPoints().Bounds;

        /// <summary>
        /// returns the longer of the start and end point connectors
        /// </summary>
        /// <returns></returns>
        public ULINE LongestConnector()
        {
            ULINE l1 = StartPtConnector();
            ULINE l2 = EndPtConnector();
            return l1.Length >= l2.Length ? l1 : l2;
        }
        /// <summary>
        /// returns the shorter of the start and end point connectors
        /// </summary>
        /// <returns></returns>
        public ULINE ShortestConnector()
        {
            ULINE l1 = StartPtConnector();
            ULINE l2 = EndPtConnector();
            return l1.Length <= l2.Length ? l1 : l2;
        }

        /// <summary>
        /// the line that connects the start pt of the first line to the start pt of the second line
        /// </summary>
        /// <returns></returns>
        public ULINE StartPtConnector() => !IsDefined ? ULINE.Null : new ULINE(Line1.Value.sp, Line2.Value.sp);

        /// <summary>
        /// the line that connects the end pt of the first line to the end pt of the second line
        /// </summary>
        /// <returns></returns>
        public ULINE EndPtConnector() => !IsDefined ? ULINE.Null : new ULINE(Line1.Value.ep, Line2.Value.ep);

        /// <summary>
        /// returns a pair containing the StartPtConnector and EndPtConnector
        /// </summary>
        /// <returns></returns>
        public ULINEPAIR Connectors(bool bSwap = false, bool bInvert = false)
        {
            ULINE? l1 = null;
            if (IsDefined) l1 = StartPtConnector();
            ULINE? l2 = null;
            if (IsDefined) l2 = EndPtConnector();

            ULINEPAIR _rVal = Clone(); //to inherit properties
            _rVal.Line1 = l1;
            _rVal.Line2 = l2;
            if (bSwap) _rVal.Swap();
            if(bInvert) _rVal.Invert();
            
            
            return _rVal;

        }

        public readonly uopLine FirstLine()  => Line1.HasValue ? new uopLine(Line1.Value) : null;

        public readonly uopLine SecondLine() => Line2.HasValue ? new uopLine(Line2.Value) : null;

        internal readonly ULINE? GetSide(uppSides aSide ,bool bReturnSomething = false, uppSides? aNewSide = null)
        {
            ULINE? _rVal = null;

            if (Line1.HasValue)
            {
                if(Line1.Value.Side == aSide) _rVal =  Line1;
            }
            if (Line2.HasValue)
            {
                if (Line2.Value.Side == aSide) _rVal = Line2;
            }
            if (!bReturnSomething) return _rVal;
            if(_rVal == null)
            {
                if (Line1.HasValue) 
                    _rVal = Line1.Value;
                else if (Line2.HasValue) _rVal = Line2.Value;

            }
            if (_rVal != null && aNewSide.HasValue)
            {
                ULINE l1 = _rVal.Value;
                l1.Side = aNewSide.Value;
                _rVal = l1;
            }
            return _rVal;


        }

        internal readonly ULINE GetSideValue(uppSides aSide, uppSides? aNewSide = null)
        {
  

            if (Line1.HasValue)
            {
                if (Line1.Value.Side == aSide) return Line1.Value;
            }
            if (Line2.HasValue)
            {
                if (Line2.Value.Side == aSide)  return Line2.Value;
            }
            return ULINE.Null;

        }

        internal readonly ULINE? GetTagged(string aTag)
        {
            if(aTag == null) return null;
            if (Line1.HasValue)
            {
                if (string.Compare(Line1.Value.Tag, aTag, true) == 0) return Line1;
            }
            if (Line2.HasValue)
            {
                if (string.Compare(Line2.Value.Tag, aTag, true) == 0) return Line2;
            }
            return null;

        }

        internal ULINE GetTaggedValue(string aTag)
        {
            ULINE? r = GetTagged(aTag);
            return r.HasValue ? r.Value : ULINE.Null;
        }


        internal void SetSideValue(uppSides aSide, ULINE? aLine )
        {


            if (Line1.HasValue)
            {
                if (Line1.Value.Side == aSide) Line1 = aLine;
            }
            if (Line2.HasValue)
            {
                if (Line2.Value.Side == aSide) Line2 = aLine;
            }
            

        }


        object ICloneable.Clone() => Clone();

        public readonly ULINEPAIR Clone() => new ULINEPAIR(this);

        public readonly UVECTORS EndPoints()
        {
            UVECTORS v = new UVECTORS();
            if (Line1.HasValue) { v.Add(Line1.Value.sp); v.Add(Line1.Value.ep); }
            if (Line2.HasValue) { v.Add(Line2.Value.sp); v.Add(Line2.Value.ep); }
            return v;
        }

        public override string ToString() => string.IsNullOrWhiteSpace(Tag) ? $"ULINEPAIR [{Count}] Col:{Col} Row:{Row}" : $"ULINEPAIR [{Count}]- {Tag}  Col:{Col} Row:{Row}  Zone:{ZoneIndex}";

        public USHAPE? ToShape()
        {
            if(!IsDefined) return null;
            return new USHAPE(EndPoints(), Tag);
        }

        /// <summary>
        /// switches the two lines
        /// </summary>
        public void Swap()
        {
            ULINE? l1 = Line1;
            Line1 = Line2;
            Line2 = l1;
        }

        /// <summary>
        /// inverts the lines.  If an index is pased ( 1 or 2 ) just the indicated line is inverted.
        /// </summary>
        /// <param name="aIndex"></param>
        public void Invert(int? aIndex = null)
        {
            bool do1 = Line1.HasValue;
            if (do1 && aIndex.HasValue) do1 = aIndex.Value == 1;
            bool do2 = Line2.HasValue;
            if (do2 && aIndex.HasValue) do2 = aIndex.Value == 2;
            if (do1)  Line1 =  Line1.Value.Inverse();
            if (do2) Line2 = Line2.Value.Inverse();

        }

        /// <summary>
        /// returns the sum of the lengths of the lines..  If an index is passed ( 1 or 2 ) just the indicated lines length is returned.
        /// </summary>
        /// <param name="aIndex"></param>
        public readonly double Length(int aIndex)
        {
            double _rVal = 0;
            bool do1 = Line1.HasValue;
            if (do1 && aIndex >0) do1 = aIndex == 1;
            bool do2 = Line2.HasValue;
            if (do2 && aIndex > 0) do2 = aIndex == 2;
            if (do1) _rVal += Line1.Value.Length;
            if (do2) _rVal += Line2.Value.Length;
            return _rVal;
        }

        public readonly double Length(uppSides aSide = uppSides.Undefined)
        {
            double _rVal = 0;
            bool do1 = Line1.HasValue;
            if (do1 && aSide != uppSides.Undefined) do1 = Line1.Value.Side == aSide;
            bool do2 = Line2.HasValue;
            if (do2 && aSide != uppSides.Undefined) do2 = Line2.Value.Side == aSide;
            if (do1) _rVal += Line1.Value.Length;
            if (do2) _rVal += Line2.Value.Length;
            return _rVal;
        }
        /// <summary>
        /// returns the greater of the two line lengths
        /// </summary>
       

        /// <summary>
        /// returns the sum of the lengths of the lines.  If a side is passed,  just the indicated lines length is returned.
        /// </summary>
        /// <param name="aSide"></param>
        public readonly double SideLength(uppSides? aSide= null)
        {
            double _rVal = 0;
            bool do1 = Line1.HasValue;
            if (do1 && aSide.HasValue) do1 = Line1.Value.Side == aSide.Value;
            bool do2 = Line2.HasValue;
            if (do2 && aSide.HasValue) do2 = Line2.Value.Side == aSide.Value;

            if (do1) _rVal += Line1.Value.Length;
            if (do2) _rVal += Line2.Value.Length;
            return _rVal;
        }
        /// <summary>
        /// returns true if the one of the pairs is suppressed.  If a side is passed,  just the indicated lines suppressed property is returned.
        /// </summary>
        /// <param name="aSide"></param>
        public readonly bool SideIsSuppressed(uppSides? aSide = null)
        {
            bool _rVal = false;
            bool do1 = Line1.HasValue;
          
            if (do1 && aSide.HasValue) do1 = Line1.Value.Side == aSide.Value;
            bool do2 = Line2.HasValue;
            if (do2 && aSide.HasValue) do2 = Line2.Value.Side == aSide.Value;

            if (do1 && !do2) return Line1.Value.Suppressed ;
            if (do2 && !do1) return Line2.Value.Suppressed;
            if(!aSide.HasValue)
            {
                if (Line1.HasValue)
                {
                    if (Line1.Value.Suppressed) _rVal = true;
                }
                if (Line2.HasValue)
                {
                    if (Line2.Value.Suppressed) _rVal = true;
                }

            }
            return _rVal;
        }
        /// <summary>
        /// moves the lines.  If an index is pased ( 1 or 2 ) just the indicated line is moved.
        /// </summary>
        /// <param name="aIndex"></param>
        public void Move(double aDX = 0, double aDY = 0, int? aIndex = null)
        {
            bool do1 = Line1.HasValue;
            if (do1 && aIndex.HasValue) do1 = aIndex.Value == 1;
            bool do2 = Line2.HasValue;
            if (do2 && aIndex.HasValue) do2 = aIndex.Value == 2;
            if (do1) Line1 = Line1.Value.Moved(aDX,aDY);
            if (do2) Line2 = Line2.Value.Moved(aDX, aDY);

        }

        /// <summary>
        /// moves the lines orthogonally.  
        /// </summary>
        /// <param name="aIndex"></param>
        public void MoveOrtho(double? aLine1Distance = null, double? aLine2Distance = null)
        {
            bool do1 = Line1.HasValue;
            if (do1 && !aLine1Distance.HasValue) do1 = false;
            bool do2 = Line2.HasValue;
            if (do2 && !aLine2Distance.HasValue) do1 = false;
            if (do1) Line1 = Line1.Value.MovedOrtho(aLine1Distance.Value);
            if (do2) Line2 = Line2.Value.MovedOrtho(aLine2Distance.Value);

        }

        /// <summary>
        /// mirrors the lines.  If an index is pased ( 1 or 2 ) just the indicated line is mirrors.
        /// </summary>
        /// <param name="aIndex"></param>
        public void Mirror(double? aX, double? aY, int? aIndex = null)
        {
            if (!aX.HasValue && !aY.HasValue) return;
            bool do1 = Line1.HasValue;
            if (do1 && aIndex.HasValue) do1 = aIndex.Value == 1;
            bool do2 = Line2.HasValue;
            if (do2 && aIndex.HasValue) do2 = aIndex.Value == 2;
            if (do1)
            { 
                ULINE l1 = Line1.Value.Mirrored(aX, aY);
                if (aX.HasValue)
                {
                    l1.Side = l1.Side switch
                    {
                        uppSides.Left => uppSides.Right,
                        uppSides.Right => uppSides.Left,
                        _ => l1.Side
                    };
                }

                if (aY.HasValue)
                {
                    l1.Side = l1.Side switch
                    {
                        uppSides.Top => uppSides.Bottom,
                        uppSides.Bottom => uppSides.Top,
                        _ => l1.Side
                    };
                }
                Line1 = l1;
                
            }

            if (do2)
            {
                    ULINE l2 = Line2.Value.Mirrored(aX, aY);
                if (aX.HasValue)
                {
                    l2.Side = l2.Side switch
                    {
                        uppSides.Left => uppSides.Right,
                        uppSides.Right => uppSides.Left,
                        _ => l2.Side
                    };
                }

                if (aY.HasValue)
                {
                    l2.Side = l2.Side switch
                    {
                        uppSides.Top => uppSides.Bottom,
                        uppSides.Bottom => uppSides.Top,
                        _ => l2.Side
                    };
                }
                Line2 = l2;
                }
       
        }
        /// <summary>
        /// returns true if one of the lines is markerked with the passed side value.
        /// </summary>
        /// <param name="aSide"></param>
        public bool SideIsIsDefined(uppSides aSide) => GetSide(aSide).HasValue;

        public readonly UVECTORS Intersections(ULINEPAIR aLinePair, bool aLinesAreInfinite = false, bool bLinesAreInfinite = false)
        {
            UVECTORS _rVal = UVECTORS.Zero;
            if (Line1.HasValue) _rVal.Append(Line1.Value.Intersections(aLinePair, aLinesAreInfinite, bLinesAreInfinite));
            if (Line2.HasValue) _rVal.Append(Line2.Value.Intersections(aLinePair, aLinesAreInfinite, bLinesAreInfinite));
            return _rVal;
        }

        public UVECTORS Intersections(IEnumerable< ULINEPAIR> aLinePairs, bool aLinesAreInfinite = false, bool bLinesAreInfinite = false)
        {
            UVECTORS _rVal = UVECTORS.Zero;
            if (aLinePairs == null) return _rVal;
            foreach (var aLinePair in aLinePairs) {  _rVal.Append(Intersections(aLinePair, aLinesAreInfinite, bLinesAreInfinite));}
            return _rVal;
        }

        #endregion Methods

        #region Shared Methods

        public static ULINEPAIR Null   => new ULINEPAIR("");

      
        #endregion SharedMethods

    }

    public class uopLinePairs
    {
        public static void Rotate(IEnumerable<uopLinePair> aPairs, iVector aCenter, double aAngle, bool bInRadians = false)
        {
            if (aPairs == null) return;

            foreach (uopLinePair pair in aPairs)
            {
                pair.Rotate(aCenter, aAngle, bInRadians);
            }


        }

        public static void SetSide(IEnumerable<uopLinePair> aPairs, uppSides aFromSide, uppSides aToSide, uppSides? aFromSide2 = null, uppSides? aToSide2 = null)
        {
            if (aPairs == null) return;

            foreach (uopLinePair pair in aPairs)
            {
                pair.SetSide(aFromSide, aToSide, aFromSide2, aToSide2);
            }
        }

        /// <summary>
        /// mirrors the lines.  If an index is pased ( 1 or 2 ) just the indicated line is mirrors.
        /// </summary>
        /// <param name="aIndex"></param>
        public static void Mirror(IEnumerable<uopLinePair> aPairs, double? aX, double? aY, int? aIndex = null)
        {
            if (aPairs == null) return;

            foreach (uopLinePair pair in aPairs)
            {
                pair.Mirror(aX, aY, aIndex);
            }

        }

        /// <summary>
        /// mirrors the lines.  If an Side is passed just the indicated line is mirrored.
        /// </summary>
        /// <param name="aSide"></param>
        public static void MirrorSide(IEnumerable<uopLinePair> aPairs, double? aX, double? aY, uppSides? aSide = null)
        {
            if (aPairs == null) return;

            foreach (uopLinePair pair in aPairs)
            {
                pair.MirrorSide(aX, aY, aSide);
            }

        }
        /// <summary>
        /// sorts the pairs in the collection in the requested order
        /// </summary>
        /// <param name="aOrder"> the order to apply</param>
        /// <param name="aReferencePt" >the reference to use for relative orders</param>
        /// <param name="aPrecis" > the precision to apply for comparisons</param>
        /// <returns></returns>
        public static List<uopLinePair> Sort(List<uopLinePair> aPairs, dxxSortOrders aOrder, iVector aReferencePt = null, int aPrecis = 3)
        {
            List<uopLinePair> _rVal = new List<uopLinePair>();

            if (aPairs.Count <= 1)
            {
                if (aPairs.Count == 1) _rVal.Add(aPairs[0]);
                return _rVal;
            }

            uopVectors centers = uopVectors.Zero;
            for (int i = 1; i <= aPairs.Count; i++)
            {
                uopLinePair pair = aPairs[i - 1];
                if (pair == null) continue;
                centers.Add(new uopVector(pair.BoundsV().Center) { Value = (double)i });
            }
            centers.Sort(aOrder, aReferencePt, aPrecis);
            foreach (var ctr in centers)
            {
                int idx = (int)ctr.Value;
                _rVal.Add(aPairs[idx - 1]);
            }


            return _rVal;
        }

        /// <summary>
        /// sorts the pairs in the collection in the requested order
        /// </summary>
        /// <param name="aOrder"> the order to apply</param>
        /// <param name="aReferencePt" >the reference to use for relative orders</param>
        /// <param name="aPrecis" > the precision to apply for comparisons</param>
        /// <returns></returns>
        internal static List<ULINEPAIR> SortU(List<ULINEPAIR> aPairs, dxxSortOrders aOrder, iVector aReferencePt = null, int aPrecis = 3)
        {
            List<ULINEPAIR> _rVal = new List<ULINEPAIR>();

            if (aPairs.Count <= 1)
            {
                if (aPairs.Count == 1) _rVal.Add(aPairs[0]);
                return _rVal;
            }

            uopVectors centers = uopVectors.Zero;
            for (int i = 1; i <= aPairs.Count; i++)
            {
                ULINEPAIR pair = aPairs[i - 1];
               
                centers.Add(new uopVector(pair.BoundsV().Center) { Value = (double)i });
            }
            centers.Sort(aOrder, aReferencePt, aPrecis);
            foreach (var ctr in centers)
            {
                int idx = (int)ctr.Value;
                _rVal.Add(aPairs[idx - 1]);
            }


            return _rVal;
        }


        internal static List<uopLinePair> FromULinePairs(IEnumerable<ULINEPAIR> aPairs, dxxSortOrders? aSortOder = null, bool? bVirtual = null, int? aZoneIndex = null)
         => FromULinePairs(aPairs, aSortOder, bVirtual, out _, aZoneIndex);
           

        internal static List<uopLinePair> FromULinePairs(IEnumerable<ULINEPAIR> aPairs, dxxSortOrders? aSortOder , bool? bVirtual, out double rTotalLength, int? aZoneIndex = null)
        {
            rTotalLength = 0;
            List<uopLinePair> _rVal = new List<uopLinePair>();
            if (aPairs == null) return _rVal;
            foreach (ULINEPAIR pair in aPairs) 
            {
                if (aZoneIndex.HasValue && pair.ZoneIndex != aZoneIndex.Value) continue;
                if (bVirtual.HasValue && pair.IsVirtual != bVirtual.Value) continue;

                _rVal.Add(new uopLinePair(pair)); 
                rTotalLength += pair.Length(); 
            }

            if (aSortOder.HasValue && _rVal.Count > 1)
            {
                _rVal = uopLinePairs.Sort(_rVal, aSortOder.Value, null, 3);
            }
            return _rVal;
        }

        internal static List<ULINEPAIR> ToULinePairs(IEnumerable<uopLinePair> aPairs, dxxSortOrders? aSortOder = null, bool? bVirtual = null, int? aZoneIndex = null)
         => ToULinePairs(aPairs, aSortOder, bVirtual, out _, aZoneIndex);


        internal static List<ULINEPAIR> ToULinePairs(IEnumerable<uopLinePair> aPairs, dxxSortOrders? aSortOder, bool? bVirtual, out double rTotalLength, int? aZoneIndex = null)
        {
            rTotalLength = 0;
            List<ULINEPAIR> _rVal = new List<ULINEPAIR>();
            if (aPairs == null) return _rVal;
            foreach (uopLinePair pair in aPairs)
            {
                if (aZoneIndex.HasValue && pair.ZoneIndex != aZoneIndex.Value) continue;
                if (bVirtual.HasValue && pair.IsVirtual != bVirtual.Value) continue;

                _rVal.Add(new ULINEPAIR(pair));
                rTotalLength += pair.Length();
            }

            if (aSortOder.HasValue && _rVal.Count > 1)
            {
                _rVal = uopLinePairs.SortU(_rVal, aSortOder.Value, null, 3);
            }
            return _rVal;
        }
        internal static double TotalLength(IEnumerable<uopLinePair> aPairs, uppSides aSide = uppSides.Undefined)
        {
            double _rVal = 0;
            if (aPairs == null) return _rVal;
            foreach (uopLinePair pair in aPairs) { _rVal += aSide != uppSides.Undefined ? pair.Length(aSide) : pair.TotalLength; }

    
            return _rVal;
        }

        internal static double TotalLength(IEnumerable<ULINEPAIR> aPairs, uppSides aSide = uppSides.Undefined)
        {
            double _rVal = 0;
            if (aPairs == null) return _rVal;
            foreach (ULINEPAIR pair in aPairs) { _rVal += aSide != uppSides.Undefined ? pair.Length(aSide) : pair.TotalLength;  }


            return _rVal;
        }

        public static List<uopLinePair> Copy(List<uopLinePair> aPairs)
        {
            if (aPairs == null) return null;
            List<uopLinePair> _rVal = new List<uopLinePair>();
            foreach (uopLinePair pair in aPairs) { _rVal.Add(pair.Clone()); }
            return _rVal;
        }
        internal static List<ULINEPAIR> Copy(List<ULINEPAIR> aPairs)
        {
            if (aPairs == null) return null;
            List<ULINEPAIR> _rVal = new List<ULINEPAIR>();
            foreach (ULINEPAIR pair in aPairs) { _rVal.Add(pair.Clone()); }
            return _rVal;
        }
    }
}
