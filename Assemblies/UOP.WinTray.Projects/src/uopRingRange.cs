using System;
using System.Collections.Generic;
using System.Linq;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Utilities;

namespace UOP.WinTray.Projects
{
    public class uopRingRange : ICloneable
    {

        public uopRingRange() { StackPattern = uppStackPatterns.Continuous; _Elevation = new uopElevation(); }

        public uopRingRange(uopRingRange aRangeToCopy) 
        { 
            StackPattern = uppStackPatterns.Continuous; 
            _Elevation = new uopElevation();
            if (aRangeToCopy == null) return;
            RingStart = aRangeToCopy.RingStart;
            RingEnd = aRangeToCopy.RingEnd;
            StackPattern = aRangeToCopy.StackPattern;
            Elevation = aRangeToCopy.Elevation.Clone();
            SortOrder = uppTraySortOrders.TopToBottom;
            Rectify();
        }


        public uopRingRange(uopTrayAssembly aAssy) 
        { 
            StackPattern = uppStackPatterns.Continuous;
            _Elevation = new uopElevation();
            if (aAssy == null) return;
             RingStart = aAssy.RingStart; 
            RingEnd = aAssy.RingEnd; 
            StackPattern = aAssy.StackPattern;
            Rectify();
            
            _Elevation = aAssy.Elevation.Clone();
            SortOrder = aAssy.TraySortOrder;
        }

        public uopRingRange(uopTrayRange aRange)
        {
            StackPattern = uppStackPatterns.Continuous;
            _Elevation = new uopElevation();
            if (aRange == null) return;
            RingStart = aRange.RingStart;
            RingEnd = aRange.RingEnd;
            StackPattern = aRange.StackPattern;
            Rectify();

            _Elevation = aRange.Elevation.Clone();
            SortOrder = aRange.TraySortOrder;
           
        }
        public uopRingRange(uopPart aPart)
        {
            StackPattern = uppStackPatterns.Continuous;
            _Elevation = new uopElevation();
            if (aPart == null) return;
            RingStart = aPart.RingStart;
            RingEnd = aPart.RingEnd;
            StackPattern = aPart.StackPattern;
            Rectify();

        }
        public uopRingRange(int aRingStart, int aRingEnd, uppStackPatterns aStackPattern = uppStackPatterns.Continuous)
        {
            _Elevation = new uopElevation();
            
            mzUtils.SortTwoValues(true, ref aRingStart, ref aRingEnd);
            StackPattern =aStackPattern; 
            RingStart = aRingStart; 
            RingEnd = aRingEnd;
            SortOrder = uppTraySortOrders.TopToBottom;
            Rectify();

        }

        public uopRingRange(string aRangeString, uppStackPatterns aStackPattern = uppStackPatterns.Continuous)
        {
            _Elevation = new uopElevation();
            SortOrder = uppTraySortOrders.TopToBottom;
            if (string.IsNullOrWhiteSpace(aRangeString)) return;
          if(aStackPattern == uppStackPatterns.Odd)
            {
                if (!aRangeString.ToUpper().Contains("ODD")) aRangeString += " ODD";
            }
            else if (aStackPattern == uppStackPatterns.Even)
            {
                if (!aRangeString.ToUpper().Contains("EVEN")) aRangeString += " EVEN";
            }

            uopUtils.SpanLimits(aRangeString, out int si, out int ei,  out aStackPattern);
            StackPattern = aStackPattern;
            RingStart = si;
            RingEnd = ei;
           
        }


        private uopElevation _Elevation;
        public uopElevation Elevation { get => _Elevation; set => _Elevation = value ?? new uopElevation(); }

        public int RingStart { get; set; }
        public int RingEnd { get; set; }

        /// <summary>
        /// ^the controls the which tray is the top tray in the columns Ranges
        /// </summary>
    
        public uppTraySortOrders SortOrder { get; set; }
        public uppStackPatterns StackPattern { get; set; }

        public uopRingRange FirstAlternatingRange => uopUtils.IsEven(RingStart) ? EvenRange: OddRange;
        public uopRingRange SecondAlternatingRange => uopUtils.IsEven(RingStart) ? OddRange : EvenRange;

        public uopRingRange TopRange(int aMinus)
        {
            uopRingRange _rVal = new uopRingRange(aRangeToCopy: this);
            if(SortOrder == uppTraySortOrders.TopToBottom)
            {
                if (_rVal.RingEnd - aMinus >= _rVal.RingStart) _rVal.RingEnd -= aMinus;
            else
                  if (_rVal.RingStart + aMinus >= _rVal.RingEnd) _rVal.RingStart += aMinus;
            }
            _rVal.Rectify();
            return _rVal;
        }
        

        public int TopRing => SortOrder != uppTraySortOrders.BottomToTop ? RingStart : RingEnd;
        public int BottomRing => SortOrder == uppTraySortOrders.BottomToTop ? RingStart : RingEnd;


        public uopRingRange OddRange
        {
            get
            {
                int si = RingStart;
                int ei = RingEnd;
                if (uopUtils.IsEven(si) && si + 1 <= ei)  si += 1;
                if (uopUtils.IsEven(ei) && ei - 1 >= si)  ei -= 1;
                return  new uopRingRange(si, ei, uppStackPatterns.Odd) { Elevation = Elevation.Clone() };
            }
        }
        public uopRingRange EvenRange
        {
            get
            {

            
            
                int si = RingStart;
                int ei = RingEnd;

                if (!uopUtils.IsEven(si) && si + 1 <= ei)  si += 1;
               
                if (!uopUtils.IsEven(ei) && ei - 1 >= si) ei -= 1;

                return new uopRingRange(si, ei, uppStackPatterns.Even) { Elevation = Elevation.Clone() };
             

                //return _rVal;
            }
        }


        public string SpanName => uopUtils.SpanName(RingStart, RingEnd, StackPattern);

        public int RingCount =>  uopUtils.TrayCount(RingStart, RingEnd, StackPattern);

        public List<int> RingNumbers
        {
            get
            {
                List<int> _rVal = new List<int>();

                Rectify();
               
                
                int step = StackPattern == uppStackPatterns.Odd || StackPattern == uppStackPatterns.Even ? 2 : 1;
                
                for(int i = RingStart; i <= RingEnd; i+=step)
                {
                    _rVal.Add(i);
                }
                return _rVal;
            }
        }

        public bool Rectify()
        {
         

            int si = RingStart;
            int ei = RingEnd;
            bool _rVal = uopRingRange.RectifySpan(ref si, ref ei, StackPattern);
            if (_rVal)
            {
                RingStart = si;
                RingEnd = ei;
            }
            return _rVal;
        }

        public bool Includes(uopRingRange aRingRange)
        {
            if (aRingRange == null) return false;

            List<int> mynums = RingNumbers;
            List<int> hernums = aRingRange.RingNumbers;
            foreach (int item in hernums)
            {
                if (mynums.IndexOf(item) >= 0) return true;
            }
            return false;
        }

        public bool Includes(int aRingIndex) => RingNumbers.Contains(aRingIndex);
       
        public uopRingRange Clone() => new uopRingRange(aRangeToCopy: this);

        object ICloneable.Clone() => (object)Clone();

        public uopRingRanges SubSpans(List<int> aRingsToOmit)
        {
            if (aRingsToOmit == null) return new uopRingRanges(Clone());

           
            List<int> myrings = RingNumbers;

            List<List<int>> spans = new List<List<int>>();
            List<int> span = new List<int>();
            int i = 0;
            bool skipped = false;
            foreach (int ring in myrings)
            {
               
                if (!aRingsToOmit.Contains(ring))
                {
                   
                    if(i== 0)
                    {
                        span = new List<int>();
                        spans.Add(span);
                    }
                    span.Add(ring);
                    i++;
                }
                else
                {
                    skipped = true;
                    i = 0;
                }
               
               
            }

            if(!skipped) return new uopRingRanges(Clone());

            uopRingRanges _rVal = new uopRingRanges();
            foreach (List<int> item in spans) 
            {
                int si = item.Min();
                int ei = item.Max();
                _rVal.Add(new uopRingRange(si, ei, StackPattern));

            }

            return _rVal;
        }

        public override string ToString()=>$"uopRingRange ({SpanName})";
        

        public static bool RectifySpan(ref int ringStart, ref int RingEnd, uppStackPatterns aStackPattern)
        {
       
            
            bool _rVal = mzUtils.SortTwoValues(true, ref ringStart, ref RingEnd);

            if (aStackPattern != uppStackPatterns.Odd && aStackPattern != uppStackPatterns.Even) return _rVal;

            if (aStackPattern == uppStackPatterns.Odd)
            {

                if (!mzUtils.IsOdd(ringStart))
                {
                    _rVal = true;
                    ringStart++;
                }
                   
                if (!mzUtils.IsOdd(RingEnd))
                {
                    _rVal = true;
                    RingEnd--;
                }
                 
            }
            else
            {

                if (mzUtils.IsOdd(ringStart))
                {
                    _rVal = true;
                    ringStart++;
                }
                   
                if (mzUtils.IsOdd(RingEnd))
                {
                    _rVal = true;
                    RingEnd--;
                }
                   
            }
            if (RingEnd < ringStart)
            {
                RingEnd = ringStart;
                _rVal = true;
            }
            return _rVal;
        }
    }

    public class uopRingRanges : List<uopRingRange>, ICloneable
    {
        public uopRingRanges() { base.Clear(); }
        public uopRingRanges(uopRingRange aRange) { base.Clear(); if (aRange != null) Add(aRange.RingStart, aRange.RingEnd, aRange.StackPattern); }

        public uopRingRanges(int aRingStart, int aRingEnd, uppStackPatterns aStackPattern = uppStackPatterns.Continuous) { base.Clear(); Add(aRingStart, aRingEnd, aStackPattern); }

        public uopRingRanges(string aRangeList)
        {
            base.Clear();
            if (string.IsNullOrWhiteSpace(aRangeList)) return;
            List<string> svals = mzUtils.StringsFromDelimitedList(aRangeList, aDelimitor: ",", bReturnNulls: false);
            foreach (string  rangename in svals)
            {
                Add(new uopRingRange(rangename));
            }
        }
        public uopRingRanges(uopRingRanges aRingRanges)
        {
            base.Clear();
            if (aRingRanges == null) return;
            foreach (var item in aRingRanges)
            {
                Add(new uopRingRange(aRangeToCopy: item));
            }
        }

        public uopRingRanges Clone() => new uopRingRanges(this);
        object ICloneable.Clone() => (object)Clone();

        public uopRingRange Add(int aRingStart, int aRingEnd, uppStackPatterns aStackPattern = uppStackPatterns.Continuous)
        {
            uopRingRange _rVal = new uopRingRange(aRingStart, aRingEnd, aStackPattern);
            base.Add(_rVal);
            return _rVal;
        }
        public List<string> SpanNames(uopRingRange aRangeToInclude = null)
        {       
            List<string> _rVal = new List<string>();
            foreach (var item in this)
            {
                if(aRangeToInclude == null)
                {
                    _rVal.Add(item.SpanName);
                }
                else
                {
                    if(item.Includes(aRangeToInclude)) _rVal.Add(item.SpanName);
                }
                    
            }
            return _rVal;
                
        }

        public List<int> RingCounts
        {
            get
            {
                List<int> _rVal = new List<int>();
                foreach (var item in this)
                {
                    _rVal.Add(item.RingCount);
                }
                return _rVal;
            }

        }

        public int TotalRingCount
        {
            get
            {
                int _rVal = 0;
                foreach (var item in this)
                {
                    _rVal += item.RingCount;
                }
                return _rVal;
            }

        }

        public string SpanNameList(string aDelimitor = ", ", string aFinalDelim = null, string aPrefix = null, uopRingRange aRangeToInclude = null) => mzUtils.ListToString(SpanNames(aRangeToInclude), aDelimitor, aFinalDelim: aFinalDelim, aPrefix: aPrefix);

        public uopRingRanges SubSpans(List<int> aRingsToOmit)
        {
            uopRingRanges _rVal = new uopRingRanges();

            foreach (uopRingRange item in this)
            {
                _rVal.AddRange(item.SubSpans(aRingsToOmit));
            }
            return _rVal;
        }

        public override string ToString()
        {
            return $"uopRingRanges ({Count})";
        }
    }
   
}
