using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Security.Cryptography;
using UOP.DXFGraphics;
using UOP.DXFGraphics.Utilities;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Interfaces;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;

namespace UOP.WinTray.Projects
{
    public class uopVectors : List<uopVector>, ICloneable
    {
        public bool Suppressed;
        public bool Invalid;
        public string Name;


        #region Constructors

        public uopVectors()
        {
            base.Clear();
            Suppressed = false;
            Invalid = false;
            Name = string.Empty;

        }
        public uopVectors(uopShape aShape)
        {
            base.Clear();
            Suppressed = false;
            Invalid = false;
            Name = aShape != null? aShape.Name : string.Empty;
            if(aShape != null)  Populate(aShape.Vertices,true, aShape.Tag);

        }
        internal uopVectors(UVECTORS aVectors, string aName = null)
        {
            base.Clear();
            Suppressed = false;
            Invalid = false;
            Name = aVectors.Name;
            Invalid = aVectors.Invalid;
            Suppressed = aVectors.Suppressed;
            for (int i = 1; i <= aVectors.Count; i++)
            {
                Add(new uopVector(aVectors.Item(i)));
            }
            if (!string.IsNullOrWhiteSpace(aName)) Name = aName;
        }

        public uopVectors(bool bInvalid, string aName = "")
        {
            Suppressed = false;
            Invalid = bInvalid;

            base.Clear();
            Name = aName ?? string.Empty;
        }

        public uopVectors(IEnumerable<iVector> aMembers, bool bCloneMembers = true, bool bDontCopyMembers = false, bool? aSuppressedVal = null)
        {

            Suppressed = false;
            Invalid = false;
            Name = string.Empty;

            base.Clear();

            if (aMembers != null)
            {
                if (aMembers is uopVectors)
                {
                    uopVectors uvecs = (uopVectors)aMembers;
                    Suppressed = uvecs.Suppressed;
                    Name = uvecs.Name;
                    Invalid = uvecs.Invalid;
                }
            }


            if (aMembers != null && !bDontCopyMembers)
            {
                foreach (var item in aMembers)
                {
                    if (item == null) continue;
                
                    uopVector u1 = uopVector.FromVector(item, bCloneMembers);

                    if (aSuppressedVal.HasValue && u1.Suppressed != aSuppressedVal.Value) continue;

                        u1.Index = base.Count + 1;
                    base.Add(u1);
                }

            }
            
        }

        public uopVectors(iVector aVector)
        {
            Suppressed = false;
            Invalid = false;
            base.Clear();
            Name = string.Empty;
            if(aVector != null)  Add(uopVector.FromVector( aVector));
            

        }

        public uopVectors(iVector aVector, iVector bVector)
        {
            Suppressed = false;
            Invalid = false;
            base.Clear();
            Name = string.Empty;
            if (aVector != null) Add(uopVector.FromVector(aVector));
            if (bVector != null) Add(uopVector.FromVector(bVector));

        }

        public uopVectors(iVector aVector, iVector bVector, iVector cVector)
        {
            Suppressed = false;
            Invalid = false;
            base.Clear();
            Name = string.Empty;
            if (aVector != null) Add(uopVector.FromVector(aVector));
            if (bVector != null) Add(uopVector.FromVector(bVector));
            if (cVector != null) Add(uopVector.FromVector(cVector));

        }


        public uopVectors(iVector aVector, iVector bVector, iVector cVector, iVector dVector)
        {
            Suppressed = false;
            Invalid = false;
            base.Clear();
            Name = string.Empty;
            if (aVector != null) Add(uopVector.FromVector(aVector));
            if (bVector != null) Add(uopVector.FromVector(bVector));
            if (cVector != null) Add(uopVector.FromVector(cVector));
            if (dVector != null) Add(uopVector.FromVector(dVector));
        }

        public uopVectors(string aCoordinateString, char aDelimitor = '¸')
        {
            Name = string.Empty;
            Invalid = false;
            Suppressed = false;
            if (string.IsNullOrWhiteSpace(aCoordinateString)) return;
            if (string.IsNullOrWhiteSpace(aDelimitor.ToString())) aDelimitor = '¸';
            string[] svals = aCoordinateString.Split(aDelimitor);

            for(int i = 0; i < svals.Length; i++)
            {
                base.Add(new uopVector(svals[i]));
            }

        }

        #endregion Constructors

        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return (Count <= 6) ? $"uopVectors[{Coords()}]" : $"uopVectors[{Count}]";
            }
            else
            {
                return (Count <= 6) ? $"uopVectors '{Name}' [{Coords()}]" : $"uopVectors '{Name}' [{Count}]";
            }

        }

        public List<double> Radii(bool bUnique = true, int aPrec = -1)
        {
            List<double> _rVal = new List<double>();
            aPrec = mzUtils.LimitedValue(aPrec, -1, 15);
            foreach (var item in this)
            {
                double dval = aPrec < 0 ? item.Radius : Math.Round(item.Radius, aPrec);
                if (bUnique)
                {
                    if (_rVal.Contains(dval)) continue;
                }
                _rVal.Add(dval);
            }
            return _rVal;

        }
        public List<double> Values(bool bUnique = true, int aPrec = -1)
        {
            List<double> _rVal = new List<double>();
            aPrec = mzUtils.LimitedValue(aPrec, -1, 15);
            foreach (var item in this)
            {
                double dval = aPrec < 0 ? item.Value : Math.Round(item.Value, aPrec);
                if (bUnique)
                {
                    if (_rVal.Contains(dval)) continue;
                }
                _rVal.Add(dval);
            }
            return _rVal;
        }

        public uopVectors Reversed(bool bGetClones = true)
        {
            uopVectors _rVal = new uopVectors(this, bCloneMembers: bGetClones);
            _rVal.Reverse();
            return _rVal;
        }

        public uopVector Item(int aIndex, bool bReturnClone = false, bool bSuppressIndexErrors = false)
        {
           
            if (aIndex < 1 || aIndex > Count) 
            {
                if (bSuppressIndexErrors) return null; else throw new IndexOutOfRangeException();
            }
            uopVector _rVal = base[aIndex - 1];
            _rVal.Index = aIndex;
            if (bReturnClone) _rVal = _rVal.Clone();
            return _rVal;
        }

        /// <summary>
        /// vectors farthest value
        /// </summary>
        /// <param name="aVector"></param>
        /// <returns></returns>
        public uopVector Farthest(iVector aVector) => Farthest(aVector, out _);

        /// <summary>
        /// vectors farthest value
        /// </summary>
        /// <param name="aVector"></param>
        /// <param name="rIndex"></param>
        /// <returns></returns>
        public uopVector Farthest(iVector aVector, out int rIndex)
        {


            rIndex = 0;
            if(Count == 0) return null;
            if (Count == 1) { rIndex = 1; return Item(1); }

            double mx = double.MinValue;

            for (int i = 1; i <= Count; i++)
            {
                double d1 = Item(i).DistanceTo(aVector, 4);
                if (d1 > mx)
                {
                    rIndex = i;
                    mx = d1;
                }
            }
            return (rIndex >= 0) ? Item(rIndex) : null;

        }


        /// <summary>
        /// vectors nearest value
        /// </summary>
        /// <param name="aVector"></param>
        /// <returns></returns>
        public uopVector Nearest(iVector aVector, double? aMinDistance = null) => Nearest(aVector, out _, aMinDistance);

        /// <summary>
        /// vectors farthest value
        /// </summary>
        /// <param name="aVector"></param>
        /// <param name="rIndex"></param>
        /// <returns></returns>
        public uopVector Nearest(iVector aVector, out int rIndex, double? aMinDistance = null)
        {


            rIndex = 0;
            if (Count == 0) return null;
            if (Count == 1) { rIndex = 1; }

            double min = double.MaxValue;

            for (int i = 1; i <= Count; i++)
            {
                double d1 = Item(i).DistanceTo(aVector, 4);
                if (d1 < min)
                {
                    rIndex = i;
                    min = d1;
                }
            }
            uopVector _rVal =  rIndex >= 0 ? Item(rIndex) : null;
            if (_rVal != null && aMinDistance.HasValue && min > aMinDistance.Value) _rVal = null; 

                return _rVal;
        }

        public int SetSuppressed(  int? aIndex, bool aSuppressionVal, bool? aMark = null, double? aRadius = null) => uopVectors.SetSuppressedValue(this,aIndex,aSuppressionVal,aMark,aRadius);

        /// <summary>
        /// returns true of the this collection contain the same number of points and the members are equal within the precision  
        /// </summary>
        /// ///<remarks> the order does not mattter unless orderwise is true</remarks>
        /// <param name="aPtCol">the first comparitor</param>
        /// <param name="aPrecis">the precision to apply</param>
        /// <param name="bOrderWise">if true, the memebrs must be equal and in the same order</param>
        /// <returns></returns>
        public bool IsEqual(IEnumerable<iVector> aPtCol, int aPrecis = 4, bool bOrderWise = false) => dxfVectors.IsEqual(this, aPtCol, aPrecis, bOrderWise);

        internal UVECTOR ItemV(int aIndex) => new UVECTOR(Item(aIndex));

        public bool SetValue(int aIndex, dynamic aValue)
        {
            if (aIndex <= 0 || aIndex > Count || aValue == null) { return false; }
            bool _rVal = base[aIndex - 1].Value != aValue;
            base[aIndex - 1].Value = aValue;
            return _rVal;
        }

        /// <summary>
        /// Vectors set values
        /// </summary>
        /// <param name="aVectors"></param>
        /// <param name="aValue"></param>
        /// <param name="aStartID"></param>
        /// <param name="aEndID"></param>
        /// <returns></returns>
        public bool SetValues(double aValue, dynamic aStartID = null, dynamic aEndID = null)
        {
            if (Count <= 0) return false;
            bool _rVal = false;
            mzUtils.LoopLimits(mzUtils.VarToInteger(aStartID), mzUtils.VarToInteger(aEndID), 1, Count, out int si, out int ei);
            uopVector u1;

            for (int i = si; i <= ei; i++)
            {
                u1 = Item(i);
                if (u1.Value != aValue) _rVal = true;
                u1.Value = aValue;

            }

            return _rVal;
        }

        /// <summary>
        /// vectors coord
        /// </summary>
        /// <param name="aPrecis"></param>
        /// <param name="aZValue"></param>
        /// <returns></returns>
        public string Coords(int aPrecis = 3, double? aZValue = null,  string aDelimitor = "¸")
        {
            string _rVal = string.Empty;
            uopVector v1 = new uopVector();
            if (string.IsNullOrEmpty(aDelimitor)) aDelimitor = uopGlobals.Delim;
            aPrecis = mzUtils.LimitedValue(aPrecis, 0, 8);

            for (int i = 1; i <= Count; i++)
            {
                v1 = Item(i);
                if (_rVal !=  string.Empty)  _rVal += aDelimitor;
                if (aZValue.HasValue)
                {
                    _rVal = $"{_rVal}({Math.Round(v1.X, aPrecis)},{Math.Round(v1.Y, aPrecis)},{Math.Round(aZValue.Value, aPrecis)})";
                }
                else
                {
                    _rVal = $"{_rVal}({Math.Round(v1.X, aPrecis)},{Math.Round(v1.Y, aPrecis)})";
                }
            }
            return _rVal;
        }


        /// <summary>
        /// vectors ordinates
        /// </summary>
        /// <param name="bGetY"></param>
        /// <param name="aPrecis"></param>
        /// <returns></returns>
        public List<double> Ordinates(bool bGetY, int aPrecis = 6)
        {
            List<double> _rVal = new List<double>();
            double aVal = 0;

            aPrecis = mzUtils.LimitedValue(aPrecis, -1, 15);
            for (int i = 1; i <= Count; i++)
            {
                if (bGetY)
                { aVal = Item(i).Y; }
                else
                { aVal = Item(i).X; }
                if (aPrecis > 0)
                {
                    aVal = Math.Round(aVal, aPrecis);
                }
                if (_rVal.Contains(aVal)) continue;
                _rVal.Add(aVal); // only add the unique values
            }
            return _rVal;
        }

        public int SuppressedCount(bool aSuppresedVal = false)
        {
            return FindAll(x => x.Suppressed == aSuppresedVal).Count;

        }

        /// <summary>
        /// vectors by tag
        /// </summary>
        /// <param name="aTag"></param>
        /// <returns></returns>
        public uopVectors GetByTag(string aTag)
        {
            uopVectors _rVal = Clone(true);
            uopVector u1;
            for (int i = 1; i <= Count; i++)
            {
                u1 = Item(i);
                if (string.Compare(u1.Tag, aTag) == 0)
                { _rVal.Add(u1); }
            }
            return _rVal;
        }


        public uopVectors GetBySuppressed(bool aSuppresedVal = false, bool bReturnClones = false)
        {
            uopVectors _rVal = Clone(true);
            if (Count <= 0) return _rVal;
            List<uopVector> returners = this.FindAll(x => x.Suppressed == aSuppresedVal);
            if (returners.Count <= 0) return _rVal;
            if (!bReturnClones)
                _rVal.AddRange(returners);
            else
                _rVal.Populate(this.FindAll(x => x.Suppressed == aSuppresedVal), bReturnClones);

            return _rVal;
;
        }
        public uopVectors GetBySuppressed( bool aSuppresedVal , out uopVectors rTheOthers,  bool bReturnClones = false)
        {
            uopVectors _rVal = Clone(true);
            rTheOthers = Clone(true);
            for (int i = 1; i <= Count; i++)
            {
               uopVector u1 = Item(i);
                if (u1.Suppressed == aSuppresedVal)
                {
                    if (bReturnClones) u1 = u1.Clone();
                    _rVal.Add(u1);

                }
                else
                {
                    if (bReturnClones) u1 = u1.Clone();
                    rTheOthers.Add(u1);
                }
            }
            return _rVal;
        }
        /// <summary>
        /// vectors by tag
        /// </summary>
        /// <param name="aValue"></param>
        /// <returns></returns>
        public uopVectors GetByValue(double aValue, int aPrecis = 4)
        {
            aPrecis = mzUtils.LimitedValue(aPrecis, 0, 15);
            uopVector u1;
            uopVectors _rVal = Clone(true);
            double aVal = Math.Round(aValue, aPrecis);
            for (int i = 1; i <= Count; i++)
            {
                u1 = Item(i);

                if (Math.Round(u1.Value, aPrecis) == aVal)
                { _rVal.Add(u1); }
            }
            return _rVal;
        }


        public List<uopVector> GetAtOrdinates(List<double> aOrdinates, dxxOrdinateDescriptors aOrdType = dxxOrdinateDescriptors.X, int aPrecis = 3, bool bRemove = false)
        {
            List<uopVector> _rVal = new List<uopVector>();

            if (aOrdinates == null) return _rVal;
            aPrecis = mzUtils.LimitedValue(aPrecis, 0, 15);
            for (int i = 1; i <= Count; i++)
            {
                uopVector mp = Item(i);
                if (aOrdType == dxxOrdinateDescriptors.X)
                {
                    if (aOrdinates.FindIndex((x) => Math.Round(x, aPrecis) == Math.Round(mp.X, aPrecis)) >= 0)
                    {
                        _rVal.Add(Item(i));
                    }
                }
                else if (aOrdType == dxxOrdinateDescriptors.Y)
                {
                    if (aOrdinates.FindIndex((x) => Math.Round(x, aPrecis) == Math.Round(mp.Y, aPrecis)) >= 0)
                    {
                        _rVal.Add(Item(i));
                    }

                }

            }
            if (bRemove)
            {
                foreach (uopVector item in _rVal)
                {
                    Remove(item);
                }
            }


            return _rVal;
        }

        /// <summary>
        /// vectors get by ord
        /// </summary>
        /// <param name="aVectors"></param>
        /// <param name="aOrd"></param>
        /// <param name="bDoX"></param>
        /// <param name="aPrecis"></param>
        /// <returns></returns>
        public uopVectors GetByOrd(dynamic aOrd, bool bDoX = false, int aPrecis = 4)
        {
            uopVectors _rVal = Clone(true);
            double ord1 = 0;
            uopVector u1;
            aPrecis = mzUtils.LimitedValue(aPrecis, 0, 15);
            aOrd = mzUtils.VarToDouble(aOrd, aPrecis: aPrecis);
            for (int i = 1; i <= Count; i++)
            {
                u1 = Item(i);
                ord1 = bDoX ? Math.Round(u1.X, aPrecis) : Math.Round(u1.Y, aPrecis);

                if (ord1 == aOrd)
                {
                    _rVal.Add(u1);
                }
            }

            return _rVal;
        }

        /// <summary>
        /// vectors by row
        /// </summary>
        /// <param name="aRow"></param>
        /// <param name="aCol"></param>
        /// <returns></returns>
        public uopVectors GetByRow(int aRow, dynamic aCol = null)
        {
            uopVectors _rVal = Clone(true);
            uopVector u1;
            bool bTestCol = aCol != null;
            int col = 0;
            if (bTestCol) { col = mzUtils.VarToInteger(aCol); }
            for (int i = 1; i <= Count; i++)
            {
                u1 = Item(i);

                if (u1.Row == aRow)
                {
                    if (!bTestCol || (bTestCol && u1.Col == col))
                    { _rVal.Add(u1); }
                }
            }
            return _rVal;
        }
        /// <summary>
        /// vectors by part index
        /// </summary>
        /// <param name="aRow"></param>
        /// <param name="aCol"></param>
        /// <returns></returns>
        public uopVectors GetByPartIndex(int aPartIndex, bool bRemove = false)
        {
            uopVectors _rVal = Clone(true);

            for (int i = 1; i <= Count; i++)
            {
                uopVector u1 = Item(i);

                if (u1.PartIndex == aPartIndex) _rVal.Add(u1);
            }
            if (bRemove)
            {
                RemoveAll((x) => x.PartIndex == aPartIndex);
            }
            return _rVal;
        }
        /// <summary>
        /// vectors by row
        /// </summary>
        /// <param name="aCol"></param>
        /// <param name="aRow"></param>
        /// <returns></returns>
        public uopVectors GetByCol(int aCol, dynamic aRow = null)
        {
            uopVectors _rVal = Clone(true);
            uopVector u1;
            bool bTestRow = aRow != null;
            int row = 0;
            if (bTestRow) { row = mzUtils.VarToInteger(aRow); }
            for (int i = 1; i <= Count; i++)
            {
                u1 = Item(i);

                if (u1.Col == aCol)
                {
                    if (!bTestRow || (bTestRow && u1.Row == row))
                    { _rVal.Add(u1); }
                }
            }
            return _rVal;
        }

        public void MirrorX(double aX, double? aValueAdder = null)
        {
            int cnt = Count;
            for (int i = 1; i <= cnt; i++)
            {
                uopVector v1 = base[i - 1].Clone();
                double dx = v1.X - aX;
                v1.X -= 2 * dx;
                if (aValueAdder.HasValue) v1.Value += aValueAdder.Value;
                Add(v1);
            }
        }



        /// <summary>
        /// vectors nearest value to line
        /// </summary>
        /// <param name="aLine"></param>
        /// <param name="rDistance"></param>
        /// <returns></returns>
        internal uopVector NearestToLine(ULINE aLine, out double rDistance) => NearestToLine(aLine, out int rIndex, out rDistance);

        /// <summary>
        /// vectors nearest value to line
        /// </summary>
        /// <param name="aLine"></param>
        /// <param name="rIndex"></param>
        /// <param name="rDistance"></param>
        /// <returns></returns>
        internal uopVector NearestToLine(ULINE aLine, out int rIndex, out double rDistance)
        {
            uopVector _rVal = new uopVector();
            rIndex = -1;
            rDistance = 0;
            uopVector u1 = new uopVector();
            double d1 = 0;
            double d2 = 0;
            d1 = System.Double.MaxValue;
            for (int i = 1; i <= Count; i++)
            {
                u1 = Item(i);
                d2 = u1.DistanceTo(aLine);
                if (d2 < d1)
                {
                    rIndex = i;
                    d1 = d2;
                }
            }
            rDistance = d1;
            if (rIndex >= 0) { _rVal = Item(rIndex); }
            return _rVal;
        }


        public uopVector First { get => Count <= 0 ? null : Item(1); }


        public uopVector Last { get => Count <= 0 ? null : Item(Count); }


        /// <summary>
        /// computes the centroid of the points in the collection
        /// </summary>
        /// <param name="aPlane">the plane to use (world by default)</param>
        /// <returns></returns>
        public uopVector Centroid(dxfPlane aPlane = null)
        {
            double rArea = 0;
            colDXFVectors rPlanarVectors = null;
            dxfVector _rVal = dxfVectors.Centroid(this, aPlane, ref rArea, ref rPlanarVectors);
          return _rVal != null ? new uopVector(_rVal) : null;
        }

        /// <summary>
        /// computes the centroid of the points in the collection
        /// </summary>
        /// <param name="rArea">returns the area defined by the points</param>
        /// <param name="rPlanarVectors">returns the members projected to the working plane</param>
        /// <param name="aPlane">the plane to use (world by default)</param>
        /// <returns></returns>
        public uopVector Centroid( out double rArea, out uopVectors rPlanarVectors,  dxfPlane  aPlane = null)
        { 
            colDXFVectors pVecs = null;
            rArea = 0;
            double area = 0;
            dxfVector _rVal = dxfVectors.Centroid(this, aPlane, ref area, ref pVecs);
            rArea = area;
            rPlanarVectors =  pVecs == null ? null : new uopVectors(pVecs, bCloneMembers:true); 
            return _rVal != null ? new uopVector(_rVal) : null;

        }

        ///returns the 2D area summation of all the vectors in the collection
        /// </summary>
        /// <param name="rLimits"></param>
        /// <returns></returns>
        public double Area()
        {
            return Area(out URECTANGLE rLimits);
        }

        ///returns the 2D area summation of all the vectors in the collection
        /// </summary>
        /// <param name="rLimits"></param>
        /// <returns></returns>
        internal double Area(out URECTANGLE rLimits)
        {

            rLimits = uopVectors.ComputeBounds(this); 
            //return rLimits.Area;

            if (Count <= 1) return 0;


            if (rLimits.Width <= 0 || rLimits.Height <= 0) { return 0; }

            uopVector org = new uopVector(rLimits.Center);
            uopVector u1;
            uopVector u2;
            double sumation = 0;
            for (int i = 1; i <= Count; i++)
            {
                u1 = Item(i);

                u2 = (i < Count) ? Item(i + 1) : Item(1);
                u1 -= org;
                u2 -= org;
                sumation += Math.Abs(u1.X * u2.Y - u2.X * u1.Y);
            }

            return 0.5 * sumation;
        }

        /// <summary>
        ///returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>
        /// 

        object ICloneable.Clone() => (object)Clone(false);

        public uopVectors Clone(bool bReturnEmpty = false) => new uopVectors(this, bCloneMembers: true , bDontCopyMembers: bReturnEmpty) ;


        public List<uopVector> AppendMirrors(double? aX, double? aY, bool bReverseOrder = false, double? aValueAdder = null)
        {
            if (Count <= 0 || (!aX.HasValue && !aY.HasValue)) return uopVectors.Zero;
            List<uopVector> _rVal = new List<uopVector>();
            if (!bReverseOrder)
            {
                for (int i = 1; i <= Count; i++)
                {
                    uopVector mem = new uopVector(Item(i));
                    if (aX.HasValue)
                    {
                        double dx = mem.X - aX.Value;
                        mem.X -= 2 * dx;
                    }
                    if (aY.HasValue)
                    {
                        double dy = mem.Y - aY.Value;
                        mem.Y -= 2 * dy;
                    }
                    if (aValueAdder.HasValue)
                    {
                        mem.Value += aValueAdder.Value;
                    }
                    _rVal.Add(mem);
                }
            }
            else
            {
                for (int i = Count; i >=1; i--)
                {
                    uopVector mem = new uopVector(Item(i));
                    if (aX.HasValue)
                    {
                        double dx = mem.X - aX.Value;
                        mem.X -= 2 * dx;
                    }
                    if (aY.HasValue)
                    {
                        double dy = mem.Y - aY.Value;
                        mem.Y -= 2 * dy;
                    }
                    if (aValueAdder.HasValue)
                    {
                        mem.Value += aValueAdder.Value;
                    }
                    _rVal.Add(mem);
                }

            }

            Append(_rVal);
            return _rVal;
        }

        public bool Mirror(double? aX, double? aY, double? aValueAdder = null)
        {
            if (Count <= 0 || (!aX.HasValue && !aY.HasValue)) return false;
            bool _rVal = false;
            for (int i = 1; i <= Count; i++)
            {
                uopVector mem = Item(i);
                if (aX.HasValue)
                {
                    double dx = mem.X - aX.Value;
                    if(dx != 0)
                    {
                        mem.X -= 2 * dx;
                        _rVal = true;
                    }
                        
                }
                if (aY.HasValue)
                {
                    double dy = mem.Y - aY.Value;
                    if(dy != 0)
                    {
                        mem.Y -= 2 * dy;
                        _rVal = true;
                    }
                    
                }
                if (aValueAdder.HasValue)
                {
                    mem.Value += aValueAdder.Value;
                }

            }

            return _rVal;
        }

        /// <summary>
        /// Removes a member by 1 based index
        /// </summary>
        /// <param name="aIndex"></param>
        /// <returns></returns>
        public uopVector Remove(int aIndex)
        {

            if (aIndex < 1 || aIndex > Count) { return null; }

            uopVector _rVal = Item(aIndex);
            RemoveAt(aIndex - 1);
            return _rVal;
        }


        public void Remove(List<uopVector> aMembers)
        {
            if (aMembers == null) return;
            foreach(var mem  in aMembers) base.Remove(mem);

        }

        /// <summary>
        /// add vectors X,Y
        /// </summary>
        /// <param name="aX"></param>
        /// <param name="aY"></param>
        /// <param name="aRow"></param>
        /// <param name="aCol"></param>
        /// <param name="aValue"></param>
        /// <param name="aTag"></param>
        /// <param name="bSuppressed"></param>
        /// <param name="aRadius"></param>
        /// <param name="aElevation"></param>
        /// <returns></returns>
        public uopVector Add(double aX, double aY, int aRow = 0, int aCol = 0, double aValue = 0, string aTag = "", bool bSuppressed = false, double aRadius = 0, double aElevation = 0, string aFlag = "", bool bNoDupes = false, int aNoDupesPrecis = 4)
        {
            uopVector _rVal = new uopVector(aX, aY)
            {
                Row = aRow,
                Col = aCol,
                Value = aValue,
                Tag = aTag,
                Flag = aFlag,
                Suppressed = bSuppressed,
                Radius = aRadius,
                Elevation = aElevation
            };

            return Add(_rVal,bNoDupes: bNoDupes, aNoDupesPrecis: aNoDupesPrecis);
        }

        /// <summary>
        /// adds avector relative to the indicated existing member
        /// </summary>
        /// <remarks>if the passed index is null, the last vector in the array is used as the relative base</remarks>
        /// <param name="aIndex">the index of the current memebr to use as the starting point</param>
        /// <param name="aXOffset"></param>
        /// <param name="aYOffset"></param>
        /// <param name="aRow"></param>
        /// <param name="aCol"></param>
        /// <param name="aValue"></param>
        /// <param name="aTag"></param>
        /// <param name="bSuppressed"></param>
        /// <param name="aRadius"></param>
        /// <param name="aElevation"></param>
        /// <returns></returns>
        public uopVector AddRelative(int? aIndex, double aXOffset, double aYOffset, string aTag = null,  double? aValue = null,  string aFlag = null, double? aRadius = null, bool bNoDupes = false, int aNoDupesPrecis = 4)
        {
            uopVector basev = aIndex.HasValue ? Item(aIndex.Value, bSuppressIndexErrors:true) : Item(Count, bSuppressIndexErrors: true);
            uopVector _rVal = new uopVector(aXOffset, aYOffset);
            if (basev != null) _rVal += basev;

            return Add(_rVal, aTag: aTag, aCollector: null, bAddClone:false,aFlag:aFlag,aRadius:aRadius,bNoDupes:bNoDupes, aNoDupesPrecis:aNoDupesPrecis );
        }


        /// <summary>
        /// adds avector relative to the indicated existing member
        /// </summary>
        /// <remarks>if the passed index is null, the last vector in the array is used as the relative base</remarks>
        /// <param name="aBaseVector">the vector  to use as the starting point</param>
        /// <param name="aXOffset"></param>
        /// <param name="aYOffset"></param>
        /// <param name="aRow"></param>
        /// <param name="aCol"></param>
        /// <param name="aValue"></param>
        /// <param name="aTag"></param>
        /// <param name="bSuppressed"></param>
        /// <param name="aRadius"></param>
        /// <param name="aElevation"></param>
        /// <returns></returns>
        public uopVector AddRelativeTo(iVector aBaseVector,double aXOffset, double aYOffset, string aTag = null, double? aValue = null, string aFlag = null, double? aRadius = null, bool bNoDupes = false, int aNoDupesPrecis = 4)
        {
            uopVector _rVal = new uopVector(aXOffset, aYOffset);
           

            if (aBaseVector != null) _rVal += new uopVector(aBaseVector);

            return Add(_rVal, aTag: aTag, aCollector: null, bAddClone: false, aFlag: aFlag, aRadius: aRadius, bNoDupes: bNoDupes, aNoDupesPrecis: aNoDupesPrecis);
        
        }



        internal uopVector Add(UVECTOR aVector, double? aValue = null, int? aPartIndex = null, double? aRadius = null, bool bNoDupes = false, int aNoDupesPrecis = 4)
        {
            uopVector v1 = new uopVector(aVector);
            if (aValue.HasValue) v1.Value = aValue.Value;
            if (aPartIndex.HasValue) v1.PartIndex = aPartIndex.Value;
            if(aRadius.HasValue) v1.Radius = aRadius.Value;
            return Add(v1, bNoDupes: bNoDupes, aNoDupesPrecis: aNoDupesPrecis);
        }

      
        /// <summary>
        /// Append vectors
        /// </summary>
        /// <param name="aVectors"></param>
        /// <param name="bAddClone"></param>
        public void Append(IEnumerable<iVector> aVectors, bool bAddClone = false, string aTag = null, string aFlag = null, double? aRadius = null, bool bNoDupes = false, int aNoDupesPrecis = 4)
        {
            if (aVectors == null) return;
            foreach (iVector bVector in aVectors)
                Add(bVector, aTag: aTag, bAddClone: bAddClone, aFlag: aFlag, aRadius: aRadius, bNoDupes: bNoDupes, aNoDupesPrecis: aNoDupesPrecis);


        }

        /// <summary>
        /// Append vectors
        /// </summary>
        /// <param name="aVectors"></param>
        /// <param name="bAddClone"></param>
        public void Populate(IEnumerable<iVector> aVectors, bool bAddClone = false, string aTag = null)
        {
            Clear();
            if (aVectors == null || aVectors.Count() <= 0) return;

            if(aVectors is uopVectors)
            {
                if(!bAddClone && aTag == null)
                {
                    base.AddRange((uopVectors)aVectors);
                    return;
                }
            }
            if (aVectors is List<uopVector>)
            {
                if (!bAddClone && aTag == null)
                {
                    base.AddRange((List<uopVector>)aVectors);
                    return;
                }
            }
            Append(aVectors, bAddClone, aTag: aTag);
        }

        internal void Append(UVECTORS bVectors, double? aValue = null, int? aPartIndex = null, bool bNoDupes = false, int aNoDupesPrecis = 4) { for (int i = 1; i <= bVectors.Count; i++) { Add(bVectors.Item(i),aValue: aValue, aPartIndex:aPartIndex, bNoDupes: bNoDupes, aNoDupesPrecis: aNoDupesPrecis); } }

        public new void Clear() { base.Clear();  }

        /// <summary>
        /// add vectors
        /// </summary>
        /// <param name="aVectors"></param>

        /// <summary>
        /// Adds a vector to the array
        /// </summary>
        /// <param name="aVector"></param>
        /// <param name="aTag"></param>
        /// <param name="aCollector"></param>
        /// <param name="aValue"></param>
        public uopVector Add(iVector aVector, string aTag = null, colDXFVectors aCollector = null, double? aValue = null, bool bAddClone = false, string aFlag = null, double? aRadius = null, bool bNoDupes = false, int aNoDupesPrecis = 4)
        {
            if (aVector == null || Count + 1 > Int32.MaxValue) { return null; }

            uopVector vector = uopVector.FromVector(aVector,bAddClone);
         
            if (bNoDupes && FindIndex(x => x.IsEqual(vector, aNoDupesPrecis, false)) > -1) return null;

            if (aTag != null) vector.Tag = aTag;
            if (aFlag != null) vector.Flag = aFlag;
            if (aValue.HasValue) vector.Value = aValue.Value;
            if (aRadius.HasValue) vector.Radius = aRadius.Value;
            vector.Index = Count + 1;
            base.Add(vector);
            aCollector?.Add(new dxfVector( aVector));


            return Item(Count);

        }

        public List<uopVector> RemoveCoincidentVectors(int aPrecis = -1)
        {
            List<uopVector> _rVal = new List<uopVector> ();
            if (Count <= 1) return _rVal;
            aPrecis = mzUtils.LimitedValue(aPrecis, -1, 15);
            for (int i = 0; i < Count; i++)
            {
                uopVector u1 = this[i];
                if (_rVal.IndexOf(u1) >= 0) continue;

                //bool keep = true;
                List<uopVector> matches = aPrecis < 0 ? FindAll((x) => x.X == u1.X && x.Y == u1.Y && base.IndexOf(x) !=i ) : FindAll((x) => Math.Round(x.X, aPrecis) == Math.Round(u1.X, aPrecis) && Math.Round(x.Y, aPrecis) == Math.Round(u1.Y, aPrecis) && base.IndexOf(x) != i);

                //for(int j = 0; j < matches.Count; j++)
                //{
                //    int idx = base.IndexOf(matches[j]);
                //    if (idx == i)
                //    {
                //        matches.RemoveAt(j);
                //        break;
                //    }
                //}

                if(matches.Count > 0)
                {
                    if (_rVal.IndexOf(u1) ==-1) 
                        _rVal.Add(u1);
                }
                //for (int j = i + 1; j <= Count; j++)
                //{
                //    uopVector u2 = this[j - 1];
                //    if (aPrecis >= 0)
                //        keep = (Math.Round(u1.X, aPrecis) != Math.Round(u2.X, aPrecis)) || (Math.Round(u1.Y, aPrecis) != Math.Round(u2.Y, aPrecis));
                //    else
                //        keep = (u1.X != u2.X) || (u1.Y != u2.Y);


                //    if (!keep) 
                //    {

                //        if (_rVal.IndexOf(u1) == -1)
                //        {
                //            _rVal.Add(u1);
                            
                //        }
                //        break;
                //    }
                //}
            }

            if(_rVal.Count > 0)
            {
                foreach(var u in _rVal)
                {
                    base.Remove(u);
                }
            }
            return _rVal;
        }
  
        /// <summary>
        ///the bounding rectangle of the points in the collection
        /// </summary>
        /// <remarks> always returns a clone</remarks>
        public uopRectangle BoundingRectangle => new uopRectangle(uopVectors.ComputeBounds(this));
      

        /// <summary>
        ///the bounding rectangle of the points in the collection
        /// </summary>
        /// <remarks> always returns a clone</remarks>
    
        internal URECTANGLE Bounds =>  uopVectors.ComputeBounds(this);
        

        /// <summary>
        /// vectors to segments
        /// </summary>
        /// <param name="bOpen"></param>
        /// <returns></returns>
        public uopSegments ToSegments(bool bOpen = false)
        {
            uopSegments _rVal = new uopSegments();
            if (Count <= 1) return _rVal;
            //dxePolyline pline = new dxePolyline(this, !bOpen);
            //var dxsegs = pline.Segments;
            //foreach(var dxseg in dxsegs)
            //{

            //    if(dxseg.GraphicType == dxxGraphicTypes.Arc)
            //    {
            //        dxeArc darc = (dxeArc)dxseg;
            //        uopArc arc = new uopArc(darc);
            //        uopVector u1 = Nearest(dxseg.DefinitionPoint(dxxEntDefPointTypes.StartPt));
            //        arc.Tag = u1.Tag;
            //        arc.Flag = u1.Flag;

            //        _rVal.Add(arc);
            //    }
            //    else
            //    {
            //        uopLine line = new uopLine((dxeLine)dxseg);
            //        uopVector u1 = Nearest(dxseg.DefinitionPoint(dxxEntDefPointTypes.StartPt));
            //        line.Tag = u1.Tag;
            //        line.Flag = u1.Flag;

            //        _rVal.Add(line);
            //    }
            //}

            uopVector u2;
            for (int i = 1; i <= Count; i++)
            {
                USEGMENT seg = new USEGMENT();
                uopVector u1 = Item(i);
                if (i <= Count - 1)
                { u2 = Item(i + 1); }
                else
                {
                    if (bOpen) { break; }
                    u2 = Item(1);
                }
                if (u1.IsEqual(u2)) continue;
                seg.Tag = u1.Tag;
                seg.Value = u1.Value;

                if (u1.Radius == 0)
                {
                    seg.IsArc = false;
                    seg.LineSeg.sp = new UVECTOR(u1);
                    seg.LineSeg.ep = new UVECTOR(u2);
                }
                else
                {
                    dxeArc aArc = dxfUtils.ArcBetweenPoints(Math.Abs(u1.Radius), u1, u2, bSuppressErrors: true);
                    if (aArc == null)
                    {
                        seg.IsArc = false;
                        seg.LineSeg.sp = new UVECTOR(u1);
                        seg.LineSeg.ep = new UVECTOR(u2);
                    }
                    else
                    {
                        seg = USEGMENT.FromArc(aArc);
                    }
                }
                if (seg.IsArc)
                    _rVal.Add(new uopArc(seg.ArcSeg));
                else
                    _rVal.Add(new uopLine(seg.LineSeg));
            }
            return _rVal;
        }

        /// <summary>
        /// vectors to segments
        /// </summary>
        /// <param name="bOpen"></param>
        /// <returns></returns>
        internal USEGMENTS ToSegs(bool bOpen = false)
        {
            USEGMENTS _rVal = new USEGMENTS("SEGMENTS");
            uopVector u1;
            uopVector u2;
            USEGMENT aSeg;


            for (int i = 1; i <= Count; i++)
            {
                aSeg = new USEGMENT();
                u1 = Item(i);
                if (i <= Count - 1)
                { u2 = Item(i + 1); }
                else
                {
                    if (bOpen) { break; }
                    u2 = Item(1);
                }

                aSeg.Tag = u1.Tag;
                aSeg.Value = u1.Value;

                if (u1.Radius == 0)
                {
                    aSeg.IsArc = false;
                    aSeg.LineSeg.sp = new UVECTOR(u1);
                    aSeg.LineSeg.ep = new UVECTOR(u2);
                }
                else
                {
                    dxeArc aArc = dxfUtils.ArcBetweenPoints(Math.Abs(u1.Radius), u1.ToDXFVector(), u2.ToDXFVector());
                    if (aArc == null)
                    {
                        aSeg.IsArc = false;
                        aSeg.LineSeg.sp = new UVECTOR(u1);
                        aSeg.LineSeg.ep = new UVECTOR(u2);
                    }
                    else
                    {
                        aSeg = USEGMENT.FromArc(aArc);
                    }
                }
                _rVal.Add(aSeg);
            }
            return _rVal;
        }
        /// <summary>
        /// vectors line segments
        /// </summary>
        /// <param name="bClosed"></param>
        /// <param name="bSuppressNulls"></param>
        /// <param name="aNullLength"></param>
        /// <returns></returns>
        internal ULINES LineSegmentsU(bool bClosed = false, bool bSuppressNulls = false, double aNullLength = 0.001)
        {
            ULINES _rVal = new ULINES();
            if (Count <= 1) { return _rVal; }
            aNullLength = Math.Abs(aNullLength);

            UVECTOR u1;
            UVECTOR u2;

            for (int i = 1; i <= Count; i++)
            {
                u1 = new UVECTOR(Item(i));
                if (i < Count)
                { u2 = new UVECTOR(Item(i + 1)); }
                else
                {
                    if (!bClosed) break;

                    u2 = new UVECTOR(Item(1));
                }
                if (!bSuppressNulls)
                {
                    _rVal.Add(u1, u2);
                }
                else
                {
                    if (u1.DistanceTo(u2, 6) <= aNullLength)
                    {
                        _rVal.Add(u1, u2);
                    }
                }
            }
            return _rVal;
        }
        /// <summary>
        /// vectors line segments
        /// </summary>
        /// <param name="bClosed"></param>
        /// <param name="bSuppressNulls"></param>
        /// <param name="aNullLength"></param>
        /// <returns></returns>
        public List<uopLine> ToLines(bool bClosed = false, bool bSuppressNulls = false, double aNullLength = 0.001)
        {
            List<uopLine> _rVal = new List<uopLine>();
            if (Count <= 1) { return _rVal; }
            aNullLength = Math.Abs(aNullLength);

            uopVector u1;
           uopVector u2;

            for (int i = 1; i <= Count; i++)
            {
                u1 = Item(i);
                if (i < Count)
                    { u2 = Item(i + 1); }
                else
                {
                    if (!bClosed) break;

                    u2 =Item(1);
                }
                if (!bSuppressNulls)
                {
                    _rVal.Add( new uopLine( u1, u2));
                }
                else
                {
                    if (u1.DistanceTo(u2, 6) <= aNullLength)
                    {
                        _rVal.Add(new uopLine(u1, u2));
                    }
                }
            }
            return _rVal;
        }

        /// <summary>
        /// vectors line segments
        /// </summary>
        /// <param name="bClosed"></param>
        /// <param name="bSuppressNulls"></param>
        /// <param name="aNullLength"></param>
        /// <returns></returns>
        internal ULINES ToLinesV(bool bClosed = false, bool bSuppressNulls = false, double aNullLength = 0.001, List<ULINE> aCollector = null)
        {
            ULINES _rVal = new ULINES();
            if (Count <= 1) { return _rVal; }
            aNullLength = Math.Abs(aNullLength);

            uopVector u1;
            uopVector u2;

            for (int i = 1; i <= Count; i++)
            {
                u1 = Item(i);
                if (i < Count)
                { u2 = Item(i + 1); }
                else
                {
                    if (!bClosed) break;

                    u2 = Item(1);
                }
                if (!bSuppressNulls)
                {
                    _rVal.Add(new ULINE(u1, u2));
                    aCollector?.Add(new ULINE(u1, u2));
                }
                else
                {
                    if (u1.DistanceTo(u2, 6) <= aNullLength)
                    {
                        _rVal.Add(new ULINE(u1, u2));
                        aCollector?.Add(new ULINE(u1, u2));
                    }
                }
            }
            return _rVal;
        }

        /// <summary>
        /// returns the contiguous line segments using the points in the collection in order 
        /// </summary>
        /// <param name="bClosed">flag to return the closing segment</param>
        /// <param name="bSuppressNulls">flag to omit any segments that have no length</param>
        /// <param name="aNullLength">the limit to use to determine if a segment is null</param>
        /// <param name="bLinked">flag to return lines with their start and end points actually referenced to the points in the collection. moving the returns lines will actual move the points in the collection. </param>
        /// <returns></returns>
        public uopLines LineSegments(bool bClosed = false, bool bSuppressNulls = false, double aNullLength = 0.001, bool bLinked = false)
        {
            uopLines _rVal = new uopLines();
            if (Count <= 1)  return _rVal;
            aNullLength = Math.Abs(aNullLength);

            for (int i = 1; i <= Count; i++)
            {
                uopVector u1 = Item(i);
                uopVector u2 = null;

                if (i < Count)
                { u2 = Item(i + 1); }
                else
                {
                    if (!bClosed) break;

                    u2 = Item(1);
                }
                if (!bSuppressNulls)
                {
                    _rVal.Add( !bLinked ? new uopLine(u1, u2) : new uopLine() { sp = u1, ep = u2});  //if not linked the end points are clones otherwise the are the actual members (move the line/ move the member vector too'
                }
                else
                {
                    if (u1.DistanceTo(u2, 6) <= aNullLength)
                    {
                        _rVal.Add(!bLinked ? new uopLine(u1, u2) : new uopLine() { sp = u1, ep = u2 });  //if not linked the end points are clones otherwise the are the actual members (move the line/ move the member vector too'
                    }
                }
            }
            return _rVal;
        }
        /// <summary>
        /// vectors to vector
        /// </summary>
        /// <param name="bValueAsRotation"></param>
        /// <param name="bValueAsVertexRadius"></param>
        /// <param name="aMinX"></param>
        /// <param name="aMinY"></param>
        /// <param name="aCollector"></param>
        /// <param name="aTag"></param>
        /// <param name="aFlag"></param>
        /// <param name="aPlane"></param>
        /// <param name="bUnsuppressOnly"></param>
        /// <param name="rSuppressed"></param>
        /// <param name="aZ"></param>
        /// <returns></returns>
        public colDXFVectors ToDXFVectors(bool bValueAsRotation = false, bool bValueAsVertexRadius = false, double? aMinX = null, double? aMinY = null, colDXFVectors aCollector = null, string aTag = null, string aFlag = null, dxfPlane aPlane = null, bool bUnsuppressOnly = false, colDXFVectors rSuppressed = null, double? aZ = null)
        {
            return uopVectors.ConvertToDXFVectors(this, bValueAsRotation, bValueAsVertexRadius, aMinY, aMinY, aCollector, aTag, aFlag, aPlane, bUnsuppressOnly, rSuppressed, aZ);
        }



        /// <summary>
        /// Vectors to polyline
        /// </summary>
        /// <param name="aVectors"></param>
        /// <param name="bOpen"></param>
        /// <param name="aTag"></param>
        /// <param name="aFlag"></param>
        /// <param name="aValue"></param>
        /// <param name="aBase"></param>
        /// <returns></returns>
        public dxePolyline ToDXFPolyline(bool bOpen = false, dynamic aTag = null, dynamic aFlag = null, double? aValue = null, dxePolyline aBase = null)
        {
            dxePolyline _rVal;
            if (aBase == null)
            { _rVal = new dxePolyline(); }
            else
            {
                _rVal = aBase;
                _rVal.Vertices.Clear();
            }

            uopVector v1;

            _rVal.Closed = !bOpen;
            for (int i = 1; i <= Count; i++)
            {
                v1 = Item(i);

                _rVal.Vertices.Add(v1.ToDXFVector(false, false), aTag: v1.Tag);
            }
            if (aValue.HasValue) { _rVal.Value = aValue.Value; }
            if (!String.IsNullOrEmpty(aTag))
            {
                _rVal.Tag = Convert.ToString(aTag);
            }
            if (!String.IsNullOrEmpty(aFlag))
            {
                _rVal.Flag = Convert.ToString(aFlag);
            }
            return _rVal;
        }

        /// <summary>
        /// Vectors Get Extreme Ord
        /// </summary>
        /// <param name="bMin"></param>
        /// <param name="bGetY"></param>
        /// <param name="aPrecis"></param>
        /// <returns></returns>
        public double GetExtremeOrd(bool bMin = false, bool bGetY = false, int aPrecis = 4) => GetExtremeOrd(out int _, bMin, bGetY, aPrecis);



        /// <summary>
        /// Vectors Get Extreme Ord
        /// </summary>
        /// <param name="aVectors"></param>
        /// <param name="bMin"></param>
        /// <param name="bGetY"></param>
        /// <param name="aPrecis"></param>
        /// <param name="rIndex"></param>
        /// <returns></returns>
        public double GetExtremeOrd(out int rIndex, bool bMin = false, bool bGetY = false, int aPrecis = 4)
        {
            rIndex = 0;
            if (Count <= 0) { return 0; }
            uopVector u1 = Item(1);
            rIndex = 1;
            if (Count == 1)
            {
                if (bGetY)
                { return u1.Y; }
                else
                { return u1.X; }
            }

            aPrecis = mzUtils.LimitedValue(aPrecis, 1, 15);
            int i = 0;
            uopVector u2;
            double aOrd = 0;
            double bOrd = 0;

            if (bGetY)
            {
                aOrd = Math.Round(u1.Y, aPrecis);
            }
            else
            {
                aOrd = Math.Round(u1.X, aPrecis);
            }
            for (i = 1; i <= Count; i++)
            {
                u2 = (uopVector)Item(i).Clone();
                if (bGetY)
                { bOrd = Math.Round(u2.Y, aPrecis); }
                else
                { bOrd = Math.Round(u2.X, aPrecis); }
                if (bMin)
                {
                    if (bOrd < aOrd)
                    {
                        aOrd = bOrd;
                        rIndex = i;
                    }
                }
                else
                {
                    if (bOrd > aOrd)
                    {
                        aOrd = bOrd;
                        rIndex = i;
                    }
                }
            }

            u1 = Item(rIndex);
            if (bGetY) { return u1.Y; } else { return u1.X; }

        }

        /// <summary>
        /// Vectors rotate
        /// </summary>
        /// <param name="aOrigin"></param>
        /// <param name="aAngle"></param>
        /// <param name="bInRadians"></param>
        public void Rotate(iVector aOrigin, double aAngle, bool bInRadians = false, bool bAddToVectorRotation = false)
        {
            double degrees = mzUtils.NormAng(aAngle, bInRadians, true, true);

            if (Math.Abs(degrees) <= 0.001)  return;
            uopVector org= new uopVector(aOrigin);
            foreach (uopVector u1 in this)
            {
                u1.Rotate(org, degrees, false);
                if (bAddToVectorRotation)
                {
                    u1.Value = mzUtils.NormAng(u1.Value + degrees, false, true, true);
                }
            }
        
        }

        /// <summary>
        /// Vectors rotate move
        /// </summary>
        /// <param name="aVectors"></param>
        /// <param name="aOrigin"></param>
        /// <param name="aAngle"></param>
        /// <param name="aXChange"></param>
        /// <param name="aYChange"></param>
        /// <param name="bInRadians"></param>
        public void RotateMove(uopVector aOrigin, double aAngle, double aXChange, double aYChange, bool bInRadians = false)
        {
            aOrigin ??= new uopVector();
            bool bRotate = false;
            bRotate = Math.Abs(aAngle) > 0.00000001;
            if (!bRotate && aXChange == 0 & aYChange == 0) { return; }
            if (!bInRadians) { aAngle *= Math.PI / 180; }

            uopVector u1;
            for (int i = 1; i <= Count; i++)
            {
                u1 = Item(i);
                if (bRotate) { u1.Rotate(aOrigin, aAngle, true); }
                u1.X += aXChange;
                u1.Y += aYChange;
            }
        }


        /// <summary>
        /// move vectors
        /// </summary>
        /// <param name="aXChange"></param>
        /// <param name="aYChange"></param>
        public void Move(double aXChange = 0, double aYChange = 0)
        {
            if( (aXChange == 0 & aYChange == 0) ||Count ==0) return;
          
            for (int i = 1; i <= Count; i++)
            {
                uopVector u1 = Item(i);
                u1.X += aXChange;
                u1.Y += aYChange;
            }
        }

        /// <summary>
        /// move vectors
        /// </summary>
        /// <param name="aXChange"></param>
        /// <param name="aYChange"></param>
        public void Translate(iVector aTranslation)
        {
            if (aTranslation ==null || Count == 0) return;

            for (int i = 1; i <= Count; i++)
            {
                uopVector u1 = Item(i);
                u1.X += aTranslation.X;
                u1.Y += aTranslation.Y;
            }
        }

        public void ProjectTo(iLine aLine)
        {
            if(aLine == null || Count == 0) return;
            uopLine line = uopLine.FromLine(aLine);
            if (line.Length == 0) return;
            foreach(var mem in this)
                mem.ProjectTo(line);
           
        }

        public uopVectors ProjectedTo(iLine aLine)
        {
            if (aLine == null || Count == 0) return uopVectors.Zero;
            uopLine line = uopLine.FromLine(aLine);
            if (line.Length == 0) return uopVectors.Zero;

            uopVectors _rVal = new uopVectors(this, bCloneMembers:false, bDontCopyMembers: true);
            foreach (var mem in this)
            {
                uopVector v1 = new uopVector(mem);

                v1.ProjectTo(line);
                _rVal.Add(v1);
            }
            return _rVal;

        }

        public void Project(uopVector aDirection, double aDistance, bool bSuppressNormalize = false, bool bInvertDirection = false)
        {
            if (!mzUtils.IsNumeric(aDistance) || Count <= 0) { return; }
            if (aDistance == 0) { return; }

            uopVector aDir;
            bool aFlg = false;
            if (!bSuppressNormalize)
            { aDir = aDirection.Normalized(out aFlg); }
            if (aFlg) return;

            aDir = aDirection;


            if (aDistance <= 0 || bInvertDirection) { aDir *= -1; }
            aDistance = Math.Abs(aDistance);
            for (int i = 1; i <= Count; i++)
            {
                base[i - 1].Project(aDir, aDistance, bSuppressNormalize: true, bInvertDirection = false);
            }

        }

        /// <summary>
        ////#1the X coordinate to match
        ///#2the Y coordinate to match
        ///#3a precision for the comparison (1 to 16)
        ///^searchs for and returns vectors from the collection whose coordinates match the passed coordinates
        ///~if an any of the ordinates (X, Y or Z) are not passed or are not numeric they are not used in the comparison.
        ///~say ony an X value is passed, then all the vectors with the same X ordinate are returned regarless of their
        ///~respective Y and Z ordinate values.
        /// </summary>
        /// <param name="aVectors"></param>
        /// <param name="aX"></param>
        /// <param name="aY"></param>
        /// <param name="aPrecis"></param>
        /// <param name="bRemove"></param>
        /// <param name="bJustOne"></param>
        /// <returns></returns>
        public uopVectors GetAtCoordinate(dynamic aX = null, dynamic aY = null, int aPrecis = 3, bool bRemove = false, bool bJustOne = false)
        {
            uopVectors _rVal = Clone(true);
            bool bTestX = mzUtils.IsNumeric(aX);
            bool bTestY = mzUtils.IsNumeric(aY);
            if (!bTestX && !bTestY) return _rVal;
            if (Count <= 0) return _rVal;
            aPrecis = mzUtils.LimitedValue(aPrecis, 0, 15);

            uopVector u1;
            bool breturn = false;
            double xval = 0;
            double yVal = 0;

            TVALUES rIndices = new TVALUES("");
            List<uopVector> keepers = new List<uopVector>();
            if (bTestX) { xval = mzUtils.VarToDouble(aX); }
            if (bTestY) { yVal = mzUtils.VarToDouble(aY); }

            for (int i = 1; i <= Count; i++)
            {
                u1 = Item(i);
                breturn = false;
                if (bTestX)
                {
                    if (Math.Abs(Math.Round(u1.X - xval, aPrecis)) == 0) { breturn = true; }
                }
                if (bTestX && !breturn)
                {
                    if (Math.Abs(Math.Round(u1.Y - yVal, aPrecis)) == 0) { breturn = true; }
                }
                if (breturn)
                {
                    rIndices.Add(i);
                    _rVal.Add(u1);
                    if (bJustOne) break;
                }
                else
                {
                    if (bRemove)
                    {
                        keepers.Add(u1);

                    }
                }

            }

            if (bRemove)
            {
                base.Clear();
                base.AddRange(keepers);
            }

            return _rVal;
        }

        internal bool ContainsVector(UVECTOR aVector, int aPrecis = 4) => EqualVectors(new uopVector(aVector), aPrecis, BailOnOne: true).Count > 0;

        public uopVectors GetByHandle(string aHandle, bool bReturnJustOne = false, bool bReturnClones = false, bool bRemove = false)
        {
 
            aHandle ??= string.Empty;
            if (Count == 0) return uopVectors.Zero;
            uopVectors _rVal = uopVectors.Zero;
            if (bReturnJustOne)
                _rVal.Add(Find(x => string.Compare(x.Handle, aHandle, true) == 0));
            else
                _rVal.Append(FindAll(x => string.Compare(x.Handle, aHandle, true) == 0));

            uopVectors _rClones = uopVectors.Zero;
            if (bRemove || bReturnClones)
            {
                foreach (var u1 in _rVal)
                {
                    if(bRemove) base.Remove(u1);
                    if(bReturnClones) _rClones.Add(new uopVector(u1));
                }
            }
            return !bReturnClones ?_rVal: _rClones;
        }


        public bool ContainsVector(uopVector aVector, int aPrecis = 4) => EqualVectors(aVector, aPrecis, BailOnOne: true).Count > 0;


        public List<uopVector> EqualVectors(uopVector aVector, int aPrecis = 4, bool BailOnOne = false)
        {
            List<uopVector> _rVal = new List<uopVector>();
            uopVector mem;
            aPrecis = mzUtils.LimitedValue(aPrecis, 0, 15);


            for (int i = 1; i <= Count; i++)
            {
                mem = base[i - 1];
                if (Math.Round(mem.DistanceTo(aVector), aPrecis) == 0)
                {
                    _rVal.Add(mem);
                    if (BailOnOne) break;
                }
            }
            return _rVal;
        }


        public List<uopVector> GetAtOrdinate(double aOrdinate, dxxOrdinateDescriptors aOrdType, int aPrecis = 3, bool bGetClones = false)
        {
            List<uopVector> _rVal = new List<uopVector>();
            if (Count <= 0) return _rVal;
            aPrecis = mzUtils.LimitedValue(aPrecis, 0, 15);
            for (int i = 1; i <= Count; i++)
            {
                uopVector v1 = Item(i);
                double vord;
                switch (aOrdType)
                {
                    case dxxOrdinateDescriptors.Y:
                        {
                            vord = Math.Round(v1.Y, aPrecis);
                            break;
                        }
                    case dxxOrdinateDescriptors.Z:
                        {
                            vord = v1.Elevation.HasValue ? Math.Round(v1.Elevation.Value, aPrecis) : 0;
                            break;
                        }
                    default:
                        {
                            vord = Math.Round(v1.X, aPrecis);
                            break;
                        }
                }
                double aDif = Math.Round(vord - aOrdinate, aPrecis);
                if (aDif == 0)
                    if (!bGetClones)
                        _rVal.Add(v1);
                    else
                        _rVal.Add(v1.Clone());

            }

            return _rVal;

        }
        public double GetOrdinate(dxxOrdinateTypes aSearchParam, int aPrecis = -1) => dxfVectors.GetPlaneOrdinate(this, aSearchParam, aPrecis: aPrecis);
        
         /// <summary>
        /// sets the indicated ordinate of the vectors to the passed value
        /// </summary>
        /// <param name="aOrdinateType">the target ordinate X, Y or Z</param>
        /// <param name="aOrdinateValue">the value to assign to the vectors X, Y or Z ordinate</param>
        /// <param name="aMatchOrdinate">if passed only the members with ordiantes that currently match the match ordinate are affected</param>
        /// <param name="aPrecis">the precis to use to comare the match ordinates</param>
        /// <returns></returns>
        public List<uopVector> SetOrdinates(dxxOrdinateDescriptors aOrdType, double aOrdValue, double? aMatchOrdinate = null, int? aPrecis = null) => dxfVectors.SetMemberOrdinates(this,aOrdType,aOrdValue,aMatchOrdinate,aPrecis).OfType<uopVector>().ToList();


        /// <summary>
        /// sorts the vectors in the collection in the requested order
        /// </summary>
        /// <param name="aOrder"> the order to apply</param>
        /// <param name="aReferencePt" >the reference to use for relative orders</param>
        /// <param name="aPrecis" > the precision to apply for comparisons</param>
        /// <returns></returns>
        public bool Sort(dxxSortOrders aOrder, iVector aReferencePt = null, int aPrecis = 3)
        {
            bool _rVal = false;
            List<int> idxs = new List<int>();
            List<iVector> sorted = dxfVectors.Sort(this, aOrder, aReferencePt, null, ref _rVal, ref idxs, aPrecis);
            base.Clear();
            foreach (var item in sorted)
            {
                uopVector u1 = (uopVector)item;
                u1.Index = base.Count + 1;
                base.Add(u1);
            }
            return _rVal;
        }

        /// <summary>
        /// returns clones of the collection sorted in the requested order
        /// </summary>
        /// <param name="aOrder"> the order to apply</param>
        /// <param name="aReferencePt" >the reference to use for relative orders</param>
        /// <param name="aPrecis" > the precision to apply for comparisons</param>
        /// <returns></returns>
        public uopVectors Sorted(dxxSortOrders aOrder, iVector aReferencePt = null, int aPrecis = 3)
        {
            uopVectors _rVal = Clone();
            _rVal.Sort(aOrder, aReferencePt, aPrecis);
            return _rVal;
        }

        public uopVector SetVectorProperties(uopVectorProperties aProperties, dxxPointFilters aControlFlag, double aOrdinate = 0, int aPrecis = 3, bool bGetClone = false)
        {
            if (Count <= 0) return null;
            iVector u1 = dxfVectors.FindVector(this, aControlFlag, aOrdinate, aPrecis: aPrecis);
            if (u1 == null) return null;

            uopVector _rVal = bGetClone ? new uopVector(u1) : (uopVector)u1;
            _rVal.SetProperties(aProperties);
            return _rVal;

        }


        public uopVector GetVector(dxxPointFilters aControlFlag, double aOrdinate = 0, int aPrecis = 3, bool bGetClone = false)
        {
            if (Count <= 0) return null;
            iVector u1 = dxfVectors.FindVector(this, aControlFlag, aOrdinate, aPrecis: aPrecis);
            if (u1 == null) return null;

            uopVector _rVal = bGetClone ? new uopVector(u1) : (uopVector)u1;
   
            return _rVal;

        }
   /// <summary>
        /// returns a vector from the collection whose coordinate properties or position in the collection match the passed control flag
        /// </summary>
        /// <param name="aFilter">search type parameter</param>
        /// <param name="aOrdinate">the ordinate to search for if the search is ordinate specific</param>
        /// <param name="bOnIsIn">flag indicating if equal values should be returned</param>
        /// <param name="aRefPt">the reference to use to compute relative distances when the filter is NearestTo or FarthestFrom  </param>
        /// <param name="aPrecis">a precision for numerical comparison (0 to 15)</param>

        public uopVectors GetVectors(dxxPointFilters aFilter, double aOrdinate = 0.0, bool bOnIsIn = true,  int aPrecis = 3, iVector aRefPt =null,bool bReturnClones = false, bool bRemove = false)
        {
            uopVectors _rVal = uopVectors.Zero;
            if (Count <= 0) return _rVal;
            List<uopVector> removers = new List<uopVector>();
            List<iVector> iVecs = dxfVectors.FindVectors(this, aFilter, aOrdinate, bOnIsIn, aRefPt, null, aPrecis, false, null);
            foreach (var item in iVecs)
            {
                uopVector u1 = (uopVector)item;
                _rVal.Add(bReturnClones ? new uopVector(u1) : u1); 
                if(bRemove) removers.Add(u1);
            }

            if (bRemove && removers.Count > 0) Remove(removers);
            return _rVal;
        }


          /// <summary>
        /// returns the vectors in the vectors that are contained within the passed rectangle
        /// </summary>
        /// <param name="aRectangle">the subject rectangle</param>
        /// <param name="bOnIsIn">flag to the vectors on the bounds of the rectangle as withn the rectangle </param>
        /// <param name="aPrecis">the precision to apply for the conparison</param>
        /// <param name="bSuppressPlanes">flag to suppress plane project of the members for the test.</param>
        /// <param name="bReturnTheInverse">flag to return the members that ARE NOT contained within the passed rectangle </param>
        /// <param name="bReturnClones">flag to return clones </param>
        /// <param name="bRemove">flag to remove the return memebrs (or their souce) from this collection</param>
        /// <returns></returns>
                public uopVectors GetVectors(iRectangle aRectangle, bool bOnIsIn = true, int aPrecis = 3, bool bReturnTheInverse = false, bool bReturnClones = false, bool? bSuppressedVal = null, bool bSuppressPlanes = true, bool bReturnPlanarVectors = false, bool bRemove = false)
        {
            uopVectors _rVal = uopVectors.Zero;
            if (Count <= 0 || aRectangle == null) return _rVal;
            List<uopVector> removers = new List<uopVector>();
            int idx = 0;
            List<iVector> iVecs = dxfVectors.FindVectors(this, aRectangle, bOnIsIn, aPrecis, bReturnTheInverse, bSuppressPlanes, bReturnPlanarVectors);
            foreach (var item in iVecs)
            {
                idx++;
                uopVector u1 = (uopVector)item;
                if (bSuppressedVal.HasValue && u1.Suppressed != bSuppressedVal.Value) continue;
                _rVal.Add(bReturnClones ? new uopVector(u1) : u1);
                if (bRemove) removers.Add(base[idx - 1]);

            }
            if (bRemove && removers.Count > 0) Remove(removers);

            return _rVal;
        }

        /// <summary>
        /// returns the vectors in the vectors that are contained within the passed circle
        /// </summary>
        /// <param name="aCircle">the subject circle</param>
        /// <param name="bOnIsIn">flag to the vectors on the circumference of the circle as withn the circle </param>
        /// <param name="aPrecis">the precision to apply for the conparison</param>
        /// <param name="bSuppressPlanes">flag to suppress plane projection of the members for the test.</param>
        /// <param name="bReturnTheInverse">flag to return the members that ARE NOT contained within the passed circle </param>
        /// <param name="bReturnClones">flag to return clones </param>
        /// <param name="bRemove">flag to remove the return memebrs (or their souce) from this collection</param>
        /// <returns></returns>
        public uopVectors GetVectors(iArc aCircle, bool bOnIsIn = true, int aPrecis = 3, bool bReturnTheInverse = false, bool bReturnClones = false, bool? bSuppressedVal = null,  bool bSuppressPlanes = true, bool bReturnPlanarVectors = false, bool bRemove = false)
        {
            uopVectors _rVal = uopVectors.Zero;
            if (Count <= 0 || aCircle == null) return _rVal;
            List<uopVector> removers = new List<uopVector>();
            int idx = 0;
            List<iVector> iVecs = dxfVectors.FindVectors(this, aCircle, bOnIsIn,aPrecis, bReturnTheInverse, bSuppressPlanes, bReturnPlanarVectors);
            foreach (var item in iVecs)
            {
                idx++;
                uopVector u1 = (uopVector)item;
                if (bSuppressedVal.HasValue && u1.Suppressed != bSuppressedVal.Value) continue;
                _rVal.Add(bReturnClones ? new uopVector(u1) : u1);
                if (bRemove) removers.Add(base[idx - 1]);

            }
            if (bRemove && removers.Count > 0) Remove(removers);

            return _rVal;
        }


        public uopVectors UniqueMembers(int aPrecis = 4, bool? aMark = null)
        {
            uopVectors _rVal = uopVectors.Zero;
            if (Count <= 0) return _rVal;

            aPrecis = mzUtils.LimitedValue(aPrecis, 0, 15);

            foreach (uopVector item in this)
            {
                bool keep = true;
                foreach (uopVector ritem in _rVal)
                {
                    if (Math.Round(Math.Sqrt(Math.Pow(item.X - ritem.X, 2) + Math.Pow(item.Y - ritem.Y, 2)), aPrecis) == 0)
                    {
                        keep = false;
                        break;
                    }
                }
                if (keep)
                {
                    _rVal.Add(item);
                    if (aMark.HasValue) item.Mark = aMark.Value;
                }
            }
            return _rVal;

        }


        public void Circularize(double aRadius, int aPrecis = 4, iVector aCircleCenter = null,bool bDontSort = false)
        {
            if(Count <= 0 || aRadius <=0) return;

            uopVector v0 = new uopVector(aCircleCenter);
            int precis = mzUtils.LimitedValue(aPrecis, 1, 15, 4);
      
           if(!bDontSort) Sort( dxxSortOrders.CounterClockwise, new uopVector(Bounds.Center), precis);

            double rad = Math.Round(aRadius,precis);
            if (Count == 1)
            {
                uopVector v1 = this[0];
                double d1 = v0.DistanceTo(v1, precis);
                v1.Radius = d1 == rad? aRadius : 0;
                return;
            }
            if (this.Count == 2)
            {
                uopVector v1 = this[0];
                uopVector vnext =  this[1];
                v1.Radius = 0;
                vnext.Radius = 0;
                double d1 = v0.DistanceTo(v1, precis);
                double d2 = v0.DistanceTo(vnext, precis);
                if(d1==rad && d2 == rad)
                {
                    v1.Radius = 0;
                    vnext.Radius = aRadius;

                }
                else if (d1 == rad && d2 != rad)
                {
                    v1.Radius = aRadius;
                }
                else if (d1 != rad && d2== rad)
                {
                    vnext.Radius = aRadius;
                }
                return;
            }

            for (int i = 1; i <= Count; i++)
            {
                uopVector v1 = this[i - 1];
                uopVector vnext = i < Count ? this[i] : this[0];
                
                double d1 = v0.DistanceTo(v1, precis);
                double d2 = v0.DistanceTo(vnext, precis);
                uopVector dir = v1.DirectionTo(vnext);

                if (d1 == rad)
                {
                    v1.Radius = aRadius;

                    if (Math.Round(Math.Abs(dir.X), 3) == 1 || Math.Round(Math.Abs(dir.Y), 3) == 1) v1.Radius = 0;
                    if( d2 != rad)
                    {
                        uopVector dir2 = v1.DirectionTo(vnext);
                        double ang = Math.Atan(dir2.Y/ dir2.X) * 180/Math.PI;
                        if (Math.Abs(ang) > 20) v1.Radius = 0;
                    }
                }
                else
                {
                    if( Math.Round(v1.Radius,precis) == rad) v1.Radius = 0;
                }
            }
       

        }
        public uopVectors ConvexHull(dxfPlane aPlane = null, bool bOnBorder = false) => new uopVectors(dxfVectors.ConvexHull(this, aPlane, bOnBorder));

        ///<summary>
        /// returns a circle that encompasses all the member vectors projected to the working plane
        /// </summary>
        /// <param name="aPlane">the plane to use</param>
        /// <returns></returns>
        public dxeArc BoundingCircle(dxfPlane aPlane = null) => dxfVectors.BoundingCircle(this, aPlane);


        public int IndexOf(uopVector aMember,bool bBaseZero = false) => !bBaseZero? base.IndexOf(aMember) + 1 : base.IndexOf(aMember);


        internal UARC BoundingCircleV()
        {

            dxeArc circle = BoundingCircle();
            UARC _rVal = new UARC();
            if (circle == null) return new UARC(0);
            _rVal.Center = new UVECTOR(circle.Center);
            _rVal.Radius = circle.Radius;
            return _rVal;

        }


        /// <summary>
        /// returns the requested ordinates in a comma delimited string
        /// </summary>
        /// <param name="aOrdType"></param>
        /// <param name="bUniqueValues"></param>
        /// <param name="aPrecision"></param>
        /// <param name="aPlane"></param>
        public string GetOrdinateList(dxxOrdinateDescriptors aOrdType, bool bUniqueValues = true, int aPrecision = 6, dxfPlane aPlane = null)
        {
            return dxfVectors.GetOrdinateList(this, aOrdType, bUniqueValues, aPrecision, aPlane);

        }
        /// <summary>
        /// used to query the collection about the ordinates of the vectors in the passed collection
        /// </summary>

        /// <param name="rXVals">returns the X ordinates referred to by at least one of the vectors in the collection</param>
        /// <param name="rYVals">returns the Y ordinates referred to by at least one of the vectors in the collection</param>
        /// <param name="iPrecis">a precision to round the returned values to</param>
        /// <param name="bUnique">flag to return only the unique ordinates or all of them</param>
        public void GetOrdinates(out List<double> rXVals, out List<double> rYVals, int iPrecis = 6, bool bUnique = true)
        {
            rXVals = new List<double>();
            rYVals = new List<double>();
            List<double> z = new List<double>();
            dxfVectors.GetOrdinates(this, ref rXVals, ref rYVals, ref z, aPrecision: iPrecis, bUniqueValues: bUnique);

        }
        /// <summary>
        /// used to query the collection about the ordinates of the vectors in the current collection
        /// </summary>
        /// <param name="aOrdType"></param>
        /// <param name="bUniqueValues"></param>
        /// <param name="aPrecision"></param>
        /// <param name="aPlane"></param>
        /// <returns></returns>
        public List<double> GetOrdinates(dxxOrdinateDescriptors aOrdType, bool bUniqueValues = true, int aPrecision = -1, dxfPlane aPlane = null)
        {
            return dxfVectors.GetOrdinates(this, aOrdType, bUniqueValues, aPrecision, aPlane);
        }

        #region Operators
        public static uopVectors operator +(uopVectors A, uopVectors B) 
        {
            if (A == null && B == null) return null;
            if (A != null & B == null) return A;
            if (A == null & B != null) return B;

            uopVectors _rVal = A;
            _rVal.AddRange(B);
            return _rVal; 
        }

        public static uopVectors operator -(uopVectors A, uopVectors B)
        {
            if (A == null && B == null) return null;
            if (A != null & B == null) return A;
            if (A == null & B != null) return B;

            uopVectors _rVal = new uopVectors();
            foreach (uopVector item in A)
            {
                if(B.IndexOf(item) <0) _rVal.Add(item);
            }
            return _rVal;
        }
        #endregion Operators

        #region Shared Methods

        public static colDXFVectors ConvertToDXFVectors(IEnumerable<uopVector> aVectors, bool bValueAsRotation = false, bool bValueAsVertexRadius = false, double? aMinX = null, double? aMinY = null, colDXFVectors aCollector = null, string aTag = null, string aFlag = null, dxfPlane aPlane = null, bool bUnsuppressOnly = false, colDXFVectors rSuppressed = null, double? aZ = null)
        {
            colDXFVectors _rVal = aCollector ?? new colDXFVectors();
            if (aVectors == null) return _rVal;

            bool bTestX = aMinX.HasValue;
            double aX = bTestX ? aMinX.Value : 0;
            bool bTestY = aMinY.HasValue;
            double aY = bTestY ? aMinY.Value : 0;
           
            foreach (uopVector u1 in aVectors)
            {
                double elev = aZ.HasValue ? aZ.Value : u1.Elevation.HasValue ? u1.Elevation.Value : 0;
                if (!bUnsuppressOnly || (bUnsuppressOnly && !u1.Suppressed))
                {
                    if ((!bTestX || (bTestX && u1.X >= aX)) && (!bTestY || (bTestY && u1.Y >= aY)))
                    {

                        _rVal.Add(u1.ToDXFVector(bValueAsRotation, bValueAsVertexRadius, elev, aTag, aFlag, aPlane));
                    }
                }
                if (u1.Suppressed)
                {
                    if (rSuppressed != null)
                    {
                        rSuppressed.Add(u1.ToDXFVector(bValueAsRotation, bValueAsVertexRadius, elev, aTag, aFlag, aPlane));
                    }
                }
            }
            return _rVal;
        }
        internal static URECTANGLE ComputeBounds(IEnumerable<iVector> aVectors) => ComputeBounds(aVectors, out _, out _);


        internal static URECTANGLE ComputeBounds(IEnumerable<iVector> aVectors, out double rWidth, out double rHeight)
        {
            rWidth = 0;
            rHeight = 0;

            if(aVectors == null) return new URECTANGLE(0, 0, 0, 0);

            List<iVector> vecs = aVectors.ToList();
            if(vecs.Count == 0 ) return new URECTANGLE(0, 0, 0, 0);


            uopVector u1 =  uopVector.FromVector(vecs[0]);
            URECTANGLE _rVal = new URECTANGLE(u1.X, u1.Y, u1.X, u1.Y);


            if (vecs.Count <= 1)  return _rVal; 


            for (int i = 2; i <= vecs.Count; i++)
            {
                u1 = uopVector.FromVector(vecs[i-1]);
                if (u1.X < _rVal.Left)  _rVal.Left = u1.X; 
                if (u1.X > _rVal.Right)  _rVal.Right = u1.X; 
                if (u1.Y > _rVal.Top)  _rVal.Top = u1.Y; 
                if (u1.Y < _rVal.Bottom) _rVal.Bottom = u1.Y;

            }

            rWidth = _rVal.Width;
            rHeight = _rVal.Height;
            return _rVal;
        }

        /// <summary>
        /// returns true if the passed vectors are equal within the passed precision
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <param name="bCompareInverse"></param>
        /// <param name="aPrecis"></param>
        /// <param name="rInverseIsEqual"></param>
        /// <param name="bNormalize"></param>
        /// <returns></returns>
        public static bool AreEqual(uopVector A, uopVector B, int aPrecis = 0)
        {
            return AreEqual(A, B, false, aPrecis, out bool INVEQ, false);
        }

        public static uopVectors CloneCopy(uopVectors aVectors, bool? aSuppressedVal = null) => aVectors == null ? null : new uopVectors(aVectors,bCloneMembers:true, bDontCopyMembers:false, aSuppressedVal: aSuppressedVal ); 

        /// <summary>
        /// returns true if the passed vectors are equal within the passed precision
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <param name="bCompareInverse"></param>
        /// <param name="aPrecis"></param>
        /// <param name="rInverseIsEqual"></param>
        /// <param name="bNormalize"></param>
        /// <returns></returns>
        public static bool AreEqual(uopVector A, uopVector B, bool bCompareInverse, int aPrecis, out bool rInverseIsEqual, bool bNormalize = false)
        {
            rInverseIsEqual = false;

            uopVector d1 = A;
            uopVector d2 = B;

            if (bNormalize)
            {
                d1 = A.Normalized();
                d2 = B.Normalized();
            }


            bool _rVal = d1.Compare(d2, aPrecis);

            if (bCompareInverse && !_rVal)
            {
                rInverseIsEqual = d1.Compare(d2 * -1, aPrecis);
                if (rInverseIsEqual) _rVal = true;

            }
            return _rVal;
        }

        /// <summary>
        /// Vectors Compare Set
        /// </summary>
        /// <param name="aVectors"></param>
        /// <param name="bVectors"></param>
        /// <param name="bInvertA"></param>
        /// <param name="bInvertB"></param>
        /// <param name="bCompareValues"></param>
        /// <param name="bInOrderOnly"></param>
        /// <returns></returns>
        public static bool CompareSet(uopVectors aVectors, uopVectors bVectors, bool bInvertA = false, bool bInvertB = false, bool bCompareValues = false, bool bInOrderOnly = false)
        {

            if (aVectors == null || bVectors == null) return false;
            if (aVectors.Count != bVectors.Count) { return false; }
            if (aVectors.Count == 0) { return true; }


            uopVector u2;

            URECTANGLE aLims = aVectors.Bounds;
            URECTANGLE bLims = bVectors.Bounds;

            List<bool> aFlgs = new List<bool>();

            //get the bounding rectangles
            //if their dimensions don't match the points cant match

            if (Math.Round(Math.Abs(aLims.Width - bLims.Width), 3) != 0) { return false; }
            if (Math.Round(Math.Abs(aLims.Height - bLims.Height), 3) != 0) { return false; }

            bool _rVal = true;

            //get the rectangle centers and compute then offset between then
            UVECTOR cp1 = aLims.Center;
            UVECTOR cp2 = bLims.Center;
            uopVectors aVs = aVectors.Clone(false);
            uopVectors bVs = bVectors.Clone(false);

            UVECTOR trns = cp1 - cp2;
            for (int i = 1; i <= aVs.Count; i++) { aFlgs.Add(false); }

            for (int i = 1; i <= aVs.Count; i++)
            {
                uopVector u1 = aVs.Item(i);
                u1.Suppressed = false;
                if (bInvertA)
                {
                    u1.Rotate(cp1, 180);
                    //             aVs.Members[i ].Value = goDXFUtils.NormalizeAngle(aVs.Members[i ].Value + 180, , True)
                }

                //find a B point that is the same as the A point
                bool bFnd = false;
                int si = 0;
                int ei = bVs.Count - 1;

                if (bInOrderOnly)
                {
                    if (bInvertB)
                    {
                        si = i; //aVs.Count - 2 + 1
                    }
                    else
                    {
                        si = i;
                    }
                    ei = si;
                }

                for (int j = si; j <= ei; j++)
                {
                    if (!aFlgs[j])
                    {
                        aFlgs[j] = true;
                        u2 = bVs.Item(j + 1);
                        u2.Suppressed = false;
                        if (bInvertB)
                        {
                            u2.Rotate(cp2, 180);
                            //            bVs.Members[i ].Value = goDXFUtils.NormalizeAngle(bVs.Members[i ].Value + 180, , True)

                        }
                        //move the B vectors to a point relative to the A vectors center
                        u2.X += trns.X;
                        u2.Y += trns.Y;


                    }
                    else
                    {
                        u2 = bVs.Item(j + 1);
                    }

                    if (!u2.Suppressed)
                    { //only compare to points that have not already been matched
                        if (u1.DistanceTo(u2, 3) == 0)
                        { //see if they are at the same location
                            if (!bCompareValues || (bCompareValues && Math.Round(u1.Value - u2.Value, 3) == 0))
                            { //compare values if requested
                                bFnd = true;

                                u2.Suppressed = true; //mark the vector as matched

                                break;
                            }
                        }
                    }
                }
                if (!bFnd)
                {
                    _rVal = false; //bail if no match was found
                    return _rVal;
                }
            }
            return _rVal;
        }

        /// <summary>
        /// vectors from string
        /// ~a value string is X,Y, Value
        /// </summary>
        /// <param name="aValString"></param>
        /// <param name="aDelimiter"></param>
        /// <param name="bReturnValueList"></param>
        /// <param name="rValueList"></param>
        /// <param name="bUnsuppressedValsOnly"></param>
        /// <returns></returns>
        public static uopVectors FromString(string aValString, string aDelimiter, bool bReturnValueList, out string rValueList, bool bUnsuppressedValsOnly = false)
        {
            rValueList = string.Empty;
            uopVectors _rVal = new uopVectors();

            aDelimiter ??= string.Empty;
            aDelimiter = mzUtils.ThisOrThat(aDelimiter.Trim(), uopGlobals.Delim);

            List<string> aVals = mzUtils.ListValues(aValString, aDelimiter);
            int acnt = aVals.Count;

            for (int i = 0; i < acnt; i++)
            {
                string vStr = aVals[i];
                double vX = 0;
                double vY = 0;
                double vV = 0;
                int iSup = 0;

                if (vStr.StartsWith("(") && vStr.EndsWith(")")) vStr = vStr.Replace("(", "").Replace(")", "");

                List<string> vVals = mzUtils.ListValues(vStr, ",");
                int vcnt = vVals.Count;

                if (vcnt >= 1) vX = mzUtils.VarToDouble(vVals[0]);

                if (vcnt >= 2) vY = mzUtils.VarToDouble(vVals[1]);

                if (vcnt >= 3) vV = mzUtils.VarToDouble(vVals[2]);


                if (vcnt >= 4) iSup = mzUtils.VarToInteger(vVals[3]);



                uopVector u1 = _rVal.Add(vX, vY, 0, 0, vV, "", iSup == -1);
                if (bReturnValueList)
                {
                    if (!bUnsuppressedValsOnly || (bUnsuppressedValsOnly && !u1.Suppressed))
                    {
                        mzUtils.ListAdd(ref rValueList, string.Format("0.0####", u1.Value), bSuppressTest: true);
                    }
                }
            }
            return _rVal;
        }

        /// <summary>
        /// vectors from string
        /// ~a value string is X,Y, Value
        /// </summary>
        /// <param name="aValString"></param>
        /// <param name="aDelimiter"></param>
        /// <param name="bReturnValueList"></param>
        /// <param name="rValueList"></param>
        /// <param name="bUnsuppressedValsOnly"></param>
        /// <returns></returns>
        public static uopVectors FromString(string aValString, string aDelimiter)
        {
            return FromString(aValString, aDelimiter, false, out List<double> _, false);
        }
        /// <summary>
        /// vectors from string
        /// ~a value string is X,Y, Value
        /// </summary>
        /// <param name="aValString"></param>
        /// <param name="aDelimiter"></param>
        /// <param name="bReturnValueList"></param>
        /// <param name="rValueList"></param>
        /// <param name="bUnsuppressedValsOnly"></param>
        /// <returns></returns>
        public static uopVectors FromString(string aValString, string aDelimiter, bool bReturnValueList, out List<double> rValueList, bool bUnsuppressedValsOnly = false)
        {
            rValueList = new List<double>();
            uopVectors _rVal = new uopVectors();

            aDelimiter ??= string.Empty;
            aDelimiter = mzUtils.ThisOrThat(aDelimiter.Trim(), uopGlobals.Delim);

            List<string> aVals = mzUtils.ListValues(aValString, aDelimiter);
            int acnt = aVals.Count;

            for (int i = 0; i < acnt; i++)
            {
                string vStr = aVals[i];
                double vX = 0;
                double vY = 0;
                double vV = 0;
                int iSup = 0;

                if (vStr.StartsWith("(") && vStr.EndsWith(")")) vStr = vStr.Replace("(", "").Replace(")", "");

                List<string> vVals = mzUtils.ListValues(vStr, ",");
                int vcnt = vVals.Count;

                if (vcnt >= 1) vX = mzUtils.VarToDouble(vVals[0]);

                if (vcnt >= 2) vY = mzUtils.VarToDouble(vVals[1]);

                if (vcnt >= 3) vV = mzUtils.VarToDouble(vVals[2]);


                if (vcnt >= 4) iSup = mzUtils.VarToInteger(vVals[3]);



                uopVector u1 = _rVal.Add(vX, vY, 0, 0, vV, "", iSup == -1);
                if (bReturnValueList)
                {
                    if (!bUnsuppressedValsOnly || (bUnsuppressedValsOnly && !u1.Suppressed))
                    {
                        rValueList.Add(u1.Value);
                    }
                }
            }
            return _rVal;
        }


        /// <summary>
        /// returns true if there is a one for 1 equalilty between the members of the two vector sets.
        /// the comparison is the distance of the members from the center of the bounding circle. If the two sets have bounding circles of
        /// different radius they cannot be equal sets.
        /// </summary>
        /// <remarks> the test is not order specific</remarks>
        /// <param name="aVectors"></param>
        /// <param name="bVectors"></param>
        /// <param name="aPrecis"></param>
        /// <returns></returns>
        public static bool MatchPlanar(uopVectors aVectors, uopVectors bVectors, int aPrecis = 3) => MatchPlanar(aVectors, bVectors, out _, out _, aPrecis: aPrecis);


        /// <summary>
        /// returns true if there is a one for 1 equalilty between the members of the two vector sets.
        /// the comparison is the distance of the members from the center of the bounding circle. If the two sets have bounding circles of
        /// different radius they cannot be equal sets.
        /// </summary>
        /// <remarks> the test is not order specific</remarks>
        /// <param name="aVectors"></param>
        /// <param name="bVectors"></param>
        /// <param name="aPrecis"></param>
        /// <returns></returns>
        public static bool MatchPlanar(uopVectors aVectors, uopVectors bVectors,  out uopArc rBoundingCircleA, out uopArc rBoundingCircleB, int aPrecis = 3)
        {
            rBoundingCircleA = null;
            rBoundingCircleB = null;
            if (aVectors == null || bVectors == null) return false;

            if (aVectors.Count != bVectors.Count) return false;



            aPrecis = mzUtils.LimitedValue(aPrecis, 0, 15);
            List<uopVector> basevecs = aVectors.OfType<uopVector>().ToList();
            List<uopVector> testvecs = bVectors.OfType<uopVector>().ToList();


            UARC arc1 = aVectors.BoundingCircleV();
            UARC arc2 = bVectors.BoundingCircleV();
            rBoundingCircleA = new uopArc(arc1);
            rBoundingCircleB = new uopArc(arc2);

            if (Math.Round(arc1.Radius, 2) != Math.Round(arc2.Radius, 2)) return false;


            foreach (uopVector v1 in basevecs)
            {
                uopVector v2 = testvecs.Find(mem => Math.Round(mem.DistanceTo(arc2.Center), aPrecis) == Math.Round(v1.DistanceTo(arc1.Center), aPrecis));
                if (v2 == null)
                    return false;
                testvecs.Remove(v2);
            }

            return true;

            //'Dim r1 As dxfRectangle = aVectors.BoundingRectangle(aPlane)
            //'Dim r2 As dxfRectangle = bVectors.BoundingRectangle(aPlane)
            //'If (Math.Round(r1.Area, 2) <> Math.Round(r2.Area, 2)) Then Return False
            //'Dim p1 As dxfPlane = r1.Plane
            //'Dim p2 As dxfPlane = r2.Plane
            //'p1.Origin = r1.BottomLeft
            //'p2.Origin = r2.BottomLeft
            //'Dim basevecs As colDXFVectors = aVectors.WithRespectToPlane(p1)
            //'Dim testvecs As colDXFVectors = bVectors.WithRespectToPlane(p2)
            //'If (colDXFVectors.Match(basevecs, testvecs, aPrecis)) Then Return True
            //'p2 = New dxfPlane(r2.BottomRight, p1.XDirection.Inverse(), p1.YDirection)
            //'testvecs = bVectors.WithRespectToPlane(p2)
            //'If (colDXFVectors.Match(basevecs, testvecs, aPrecis)) Then Return True
            //'p2 = New dxfPlane(r2.TopRight, p1.XDirection.Inverse(), p1.YDirection.Inverse())
            //'testvecs = bVectors.WithRespectToPlane(p2)
            //'If (colDXFVectors.Match(basevecs, testvecs, aPrecis)) Then Return True
            //'p2 = New dxfPlane(r2.TopLeft, p1.XDirection, p1.YDirection.Inverse())
            //'testvecs = bVectors.WithRespectToPlane(p2)
            //'If (colDXFVectors.Match(basevecs, testvecs, aPrecis)) Then Return True
            //Return True

        }
        public static void Translate(IEnumerable<uopVector> aVectors, double aXChange = 0, double aYChange = 0)
        {
            if (aVectors == null || (aXChange == 0 & aYChange == 0)) return;
            foreach (uopVector u1 in aVectors)
            {
                u1.X += aXChange;
                u1.Y += aYChange;
            }

        }


        public static int  SetSuppressedValue(List<uopVector> aVectors, int? aIndex, bool aSuppressionVal, bool? aMark = null, double? aRadius = null)
        {
            if(aVectors == null || aVectors.Count == 0) return 0;
            int _rVal = 0;
            if (!aIndex.HasValue)
            {
                foreach (var mem in aVectors) { if (mem.Suppressed != aSuppressionVal) _rVal++; mem.Suppressed = aSuppressionVal; if (aMark.HasValue) mem.Mark = aMark.Value; if (aRadius.HasValue) mem.Radius = aRadius.Value; }
                return _rVal;
            }
            if (aIndex.Value < 0 || aIndex.Value > aVectors.Count) return _rVal;
            if (aVectors[aIndex.Value - 1].Suppressed != aSuppressionVal) _rVal = 1;
            aVectors[aIndex.Value - 1].Suppressed = aSuppressionVal;
            if (aMark.HasValue) aVectors[aIndex.Value - 1].Mark = aMark.Value;
            if (aRadius.HasValue) aVectors[aIndex.Value - 1].Radius = aRadius.Value;
            return _rVal;
        }



        public static uopVectors Zero { get => new uopVectors(); }


        #endregion Shared Methods
    }
}
