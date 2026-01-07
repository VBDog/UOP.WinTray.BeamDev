using log4net.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UOP.DXFGraphics;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Interfaces;
using UOP.WinTray.Projects.Structures;

namespace UOP.WinTray.Projects
{
    public class uopLines : List<uopLine>, IEnumerable<uopLine>, ICloneable
    {

        #region Constructors
        public uopLines() { base.Clear(); Name = string.Empty; }
        public uopLines(string aName) { base.Clear(); Name = aName != null ? aName : string.Empty; }

        public uopLines(IEnumerable<iLine> aLines, bool bAddClones = true)
        {
            base.Clear(); Name = string.Empty;
            if (aLines == null) return;
            Populate(aLines, bAddClones);

            if (aLines is uopLines)
            {
                uopLines ulines = (uopLines)aLines;
                Name = ulines.Name;
            }
        }
        public uopLines(uopSegments aLines, bool bAddClones = true)
        {
            base.Clear(); Name = string.Empty;
            if (aLines == null) return;
        foreach(iSegment seg in aLines)
            {
                if(seg is uopLine)
                {
                    Add((uopLine)seg, bAddClones );
                }
            }

        }



        internal uopLines(ULINES aLines)
        {
            base.Clear(); Name = aLines.Name;
            for (int i = 0; i < aLines.Count; i++) base.Add(new uopLine(aLines.Item(i)));
        }

        #endregion Constructors

        #region Properties

        public string Name { get; set; }

        #endregion Properties


        #region Methods
        public uopLine Add(double aSPX, double aSPY, double aEPX, double aEPY, int aRow = 0, int aCol = 0, string aTag = "", string aFlag = "") => Add(new uopLine(aSPX, aSPY, aEPX, aEPY, aRow, aCol) { Tag = aTag, Flag = aFlag });

        public uopLine Add(iLine aLine, bool bAddClone = false, int? aRow = null, int? aCol = null)
        {
            if (aLine == null) return null;

            uopLine _rVal = null;
            if (aLine is uopLine)
                _rVal = bAddClone ? new uopLine(aLine) : (uopLine)aLine;
            else
                _rVal = new uopLine(aLine);

            if (aRow.HasValue) _rVal.Row = aRow.Value;
            if (aCol.HasValue) _rVal.Col = aCol.Value;
            base.Add(_rVal);
            return _rVal;
        }

        public uopVectors Intersections(IEnumerable<iSegment> aSegments, bool aLinesAreInfinite = false, bool aSegmentsAreInfinite = false, bool bNoDupes = false, int aNoDupesPrecis = 4)
        {
            uopVectors _rVal = uopVectors.Zero;
            if (aSegments == null) return _rVal;

            foreach (var line1 in this)
            {
                foreach (var seg in aSegments)
                {
                    if (seg == null) continue;
                    var ips = line1.Intersections(seg, aSegmentsAreInfinite, aLinesAreInfinite);
                   foreach(var ip in ips)
                        _rVal.Add(ip, bNoDupes: bNoDupes, aNoDupesPrecis: aNoDupesPrecis);
                }
            }

            return _rVal;

        }
        public uopVectors Intersections(IEnumerable<iLine> bLines, bool aLinesAreInfinite = false, bool bLinesAreInfinite = false,bool bNoDupes = false, int aNoDupesPrecis = 4)
        {
            uopVectors _rVal = uopVectors.Zero;
            if(bLines == null) return _rVal;

            foreach(var line1 in this)
            {
                foreach(var line2 in bLines)
                {
                    if(line2 == null)  continue;
                    
                    uopLine uline = uopLine.FromLine(line2);
                    uopVector ip = line1.IntersectionPt(uline, out _, out _, out bool isOnLine1, out bool isOnLine2, out bool exists);
                    if(!exists) continue;
                    if (!aLinesAreInfinite && !isOnLine1) continue;
                    if (!bLinesAreInfinite && !isOnLine2) continue;

                    _rVal.Add(ip, bNoDupes: bNoDupes, aNoDupesPrecis: aNoDupesPrecis);

                }
            }

            return _rVal;

        }

        public void Mirror(double? aX, double? aY)
        {
            if((!aX.HasValue && !aY.HasValue) || Count <=0) return;
            foreach(var line in this)
            {
                line.Mirror(aX, aY);
            }
        }


        public void Move(double DX = 0, double DY = 0)
        {

            if ((DX == 0 && DY == 0) || Count <= 0) return;
            foreach (var line in this)
            {
                line.Move(DX, DY);
            }
        }

        /// <summary>
        /// retruns the member lines whose points return at least one member that matches the search criteria 
        /// </summary>
        /// <param name="match">the predicate to apply</param>
        /// <param name="bGetClones">flag to return clones of the return set</param>
        /// <returns></returns>

        public uopLines GetByPoints(Predicate<uopVector> match, bool bGetClones = false)
        {
            uopLines _rVal = new uopLines() { Name = Name};
            if (Count <= 0) return _rVal;
            foreach (var line in this)
            {
                List<uopVector> pts = line.Points.FindAll(match);
               if(pts.Count > 0) _rVal.Add(line,bGetClones);
            }

            return _rVal;
        }

        public uopLine GetAtMaxOrdinate(bool bTestY, bool bTestEndPt = false)
        {
            if(Count <=0) return null;
            if (Count == 1) return Item(1);

            double minVal = double.MinValue;
            int idx = 1;
            int i = 0;
            foreach (var item in this)
            {
                i++;
                double ord = bTestY ? item.Y(bTestEndPt) : item.X(bTestEndPt);
                if(ord > minVal)
                {
                    idx = i; 
                    minVal = ord;
                }
            }

            return Item(idx);
        }

        public uopLine GetAtMinOrdinate(bool bTestY, bool bTestEndPt = false)
        {
            if (Count <= 0) return null;
            if (Count == 1) return Item(1);

            double maxVal = double.MaxValue;
            int idx = 1;
            int i = 0;
            foreach (var item in this)
            {
                i++;
                double ord = bTestY ? item.Y(bTestEndPt) : item.X(bTestEndPt);
                if (ord < maxVal)
                {
                    idx = i;
                    maxVal = ord;
                }
            }

            return Item(idx);
        }

        public uopVectors Points(Predicate<uopVector> match, bool bGetClones = false)
        {
            if (Count <= 0) return uopVectors.Zero;
            uopVectors _rVal = uopVectors.Zero;
            foreach (var line in this) _rVal.Append( line.Points.FindAll(match), bAddClone: bGetClones);

            return _rVal;
        }

        public uopVectors Points(bool ? bSuppressVal = null, bool bGetClones = false)
        {
            if(Count  <= 0) return uopVectors.Zero;
            uopVectors _rVal = uopVectors.Zero;
            foreach (var line in this) _rVal.Append(bSuppressVal.HasValue ? line.Points.GetBySuppressed(bSuppressVal.Value) : line.Points, bAddClone: bGetClones);

            return _rVal;

        }

        /// <summary>
        /// retrieves the handle points of the member lines
        /// </summary>
        /// <param name="aPointType">enum indicator of which point to retrieve form each member</param>
        /// <param name="bGetClones">flag to rerieve clones of the handle points (mid points are always new)</param>
        /// <param name="bNoDupes">flag to remove the coincident members and only return a unique set</param>
        /// <param name="aPrecis">the precision to apply if bNoDupes is true</param>
        /// <returns></returns>

       public uopVectors HandlePoints(uppSegmentPoints aPointType, bool bGetClones = false, bool bNoDupes = false, int aPrecis = -1) 
        {
            uopVectors _rVal = uopVectors.Zero;
            foreach(uopLine line in this)
            {
                _rVal.Add(line.HandlePoint(aPointType));
            }

            if(bNoDupes) _rVal.RemoveCoincidentVectors(aPrecis);

            return _rVal;

        }


        /// <summary>
        /// retrieves the end points (start & end) of the member lines
        /// </summary>
        /// <param name="bGetClones">flag to rerieve clones of the handle points (mid points are always new)</param>
        /// <param name="bGetStart">flag return the members start points</param>
        /// <param name="bGetEnd">flag return the members end points</param>
        /// <param name="bNoDupes">flag to remove the coincident members and only return a unique set</param>
        /// <param name="aNoDupesPrecis">the precision to apply if bNoDupes is true</param>
        /// <returns></returns>

        public uopVectors EndPoints( bool bGetClones = false, bool bGetStart = true, bool bGetEnd = true, bool bNoDupes = false, int aNoDupesPrecis = 4)
        {
            if ((!bGetStart && !bGetEnd) || Count <= 0) return uopVectors.Zero;
            uopVectors _rVal = uopVectors.Zero;
            foreach (uopLine line in this)
                _rVal.Append(line.EndPoints(bGetClones, bGetStart:bGetStart, bGetEnd:bGetEnd ), bNoDupes: bNoDupes, aNoDupesPrecis: aNoDupesPrecis) ;
         
         

            return _rVal;

        }

        public uopLine Item(int aIndex, bool bReturnClone = false, bool bSuppressIndexErrors = false)
        {

            if (aIndex < 1 || aIndex > Count)
            {
                if (bSuppressIndexErrors) return null; else throw new IndexOutOfRangeException();
            }
            uopLine _rVal = base[aIndex - 1];
            _rVal.Index = aIndex;
            if (bReturnClone) _rVal = new uopLine( _rVal);
            return _rVal;
        }

        public uopVectors Intersections(iLine bLine, bool aLinesAreInfinite = false, bool bLinesAreInfinite = false)
        {
            uopVectors _rVal = uopVectors.Zero;
            if (bLine == null) return _rVal;
            uopLine uline = uopLine.FromLine(bLine);

            foreach (var line1 in this)
            {
   
                    uopVector ip = line1.IntersectionPt(uline, out _, out _, out bool isOnLine1, out bool isOnLine2, out bool exists);
                    if (!exists) continue;
                    if (!aLinesAreInfinite && !isOnLine1) continue;
                    if (!bLinesAreInfinite && !isOnLine2) continue;

                    _rVal.Add(ip,aTag:line1.Handle, aFlag: uline.Handle);

                
            }

            return _rVal;

        }
        public uopVectors Intersections(iArc aArc, bool aLinesAreInfinite = false, bool aArcIsInfinite = false)
        {
            uopVectors _rVal = uopVectors.Zero;
            if (aArc == null) return _rVal;
            uopArc arc = uopArc.FromArc(aArc);

            foreach (var line1 in this)
                _rVal.AddRange(line1.Intersections(arc,aArcIsInfinite: aArcIsInfinite, aLineIsInfinite: aLinesAreInfinite));
            return _rVal;

        }
        public uopLines Clone() => new uopLines(this);

        object ICloneable.Clone() => new uopLines(this);

        public void Populate(IEnumerable<iLine> aLines, bool bAddClones = false)
        {
            Clear();
            if(aLines == null) return;
            foreach (iLine aLine in aLines)
            {
                if (aLine == null) continue;
                if (aLine is uopLine)
                    base.Add(bAddClones ? new uopLine(aLine) : (uopLine)aLine);
                else
                    base.Add(new uopLine(aLine));
            }
        }

        internal void Populate(ULINES aLines)
        {
            Clear();
            for(int i =1; i<= aLines.Count; i++) base.Add(new uopLine(aLines.Item(i)));
            
        }
        public void Append(IEnumerable<iLine> aLines, bool bAddClones = false)
        {
            if (aLines == null) return;
            foreach (iLine aLine in aLines)
            {
                if (aLine == null) continue;
                if (aLine is uopLine)
                    base.Add(bAddClones ? new uopLine(aLine) : (uopLine)aLine);
                else
                    base.Add(new uopLine(aLine));
            }
        }

        #endregion  Methods

        #region Shared Methods

        public static uopLines CloneCopy(uopLines aLines) => aLines == null ? null : new uopLines(aLines);
        
        #endregion Shared Methods
    }
}
