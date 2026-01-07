
using System;
using System.Collections;
using System.Collections.Generic;
using UOP.WinTray.Projects.Structures;


namespace UOP.WinTray.Projects.Utilities
{
    public class mzValues : ICloneable
    {
        private TVALUES _Struc;

        #region Constructors
        public mzValues() => _Struc = new TVALUES("");

        public mzValues(string aName = "", dynamic aBaseVal = null) => _Struc = new TVALUES(aName, aBaseVal);

        internal mzValues(TVALUES aStructure) => _Struc = aStructure;

        public mzValues(List<double> aValues, bool bNoDupes = false)
        {
            _Struc = new TVALUES("");
            if (aValues == null) return;
            foreach (var item in aValues)
            {
                Add(item, bNoDupes);
            }

        }
        public mzValues(dynamic aValueString, bool bNoDupes = false, string aDelimitor = ",", bool bReturnNulls = false, bool bTrim = false, bool bNumbersOnly = false, int iPrecis = -1, bool bRemoveParens = false, string aSkipList = "")
        {
            _Struc = new TVALUES("");
            if (aValueString != null) AddByString(aValueString.ToString() , bNoDupes, aDelimitor, bReturnNulls, bTrim, bNumbersOnly, iPrecis, bRemoveParens, aSkipList);

        }

        #endregion Constructors

        internal TVALUES Structure { get => _Struc; set => _Struc = value; }

        public int Count => _Struc.Count;

        /// <summary>
        /// returns the indices of the members whos values falls within the passed limits
        /// </summary>
        /// <param name="aLower">the lower bound to apply</param>
        /// <param name="aUpper">the upper bound to apply</param>
        /// <param name="bOnisIn">flag indicating if(a value on a bound should be considered withing the reange</param>
        /// <param name="aPrecis">a precision to apply</param>
        /// <param name="rIndices"></param>
        /// <returns></returns>
        public mzValues GetInRange(dynamic aLower, dynamic aUpper, bool bOnisIn = true, int aPrecis = -1, mzValues rIndices = null)
         => new mzValues(_Struc.GetInRange(aLower, aUpper, bOnisIn, aPrecis));

        public void Invert() 
        {
            var temp = _Struc;
            temp.Invert();
            _Struc = temp;
        }

        public bool Add(dynamic aValue, bool bNoDupes = false)
        {
            TVALUES struc = _Struc;
            bool _rVal = struc.Add(aValue, bNoDupes);
            _Struc = struc;
            return _rVal;

        }

        public bool AddByString(string aValueString, bool bNoDupes = false, string aDelimitor = ",", bool bReturnNulls = false, bool bTrim = false, bool bNumbersOnly = false, int iPrecis = -1, bool bRemoveParens = false, string aSkipList = "")
        {
            TVALUES struc = _Struc;
            bool _rVal = struc.Append(TVALUES.FromDelimitedList(aValueString, aDelimitor, bReturnNulls, bTrim, bNoDupes, bNumbersOnly, iPrecis, bRemoveParens, aSkipList), bNoDupes: bNoDupes) > 0;
            _Struc = struc;
            return _rVal;

        }
        public void PrintToConsole(string aHeading = null, bool bIndexed = false) => _Struc.PrintToConsole();

        /// <summary>
        /// adds the passed value to the value at the indicated index
        /// </summary>
        /// <param name="aIndex">the index of the value to add too</param>
        /// <param name="aAdder">the value to add to the indicated member</param>
        /// <returns></returns>
        public void AddTo(int aIndex, dynamic aAdder)
        {
            TVALUES struc = _Struc;
            struc.AddTo(aIndex, aAdder);
            _Struc = struc;
        }

        /// <summary>
        /// multiplies the value of the indicated member by the passed value
        /// </summary>
        /// <param name="aIndex">the index of the value to add too</param>
        /// <param name="aMultiplier">the value to multiply the indicated member by</param>
        /// <returns></returns>
        public void MultiplyBy(int aIndex, double aMultiplier)
        {
            TVALUES struc = _Struc;
            struc.Multiply(aIndex, aMultiplier);
            _Struc = struc;
        }

        public bool Clear()
        {
            TVALUES struc = _Struc;
            bool _rVal = struc.Clear();
            _Struc = struc;
            return _rVal;
        }

        public mzValues Clone() => new mzValues(new TVALUES(_Struc));

        object ICloneable.Clone() => (object)this.Clone();

        public bool Append(mzValues aValues, bool bNoDupes = false)
        {
            if (aValues == null) return false;
            TVALUES struc = _Struc;
            bool _rVal = struc.Append(aValues.Structure, bNoDupes) > 0 ;
             _Struc = struc;
            return _rVal;
        }

        public bool ContainsValue(dynamic aValue, bool bStringCompare = false, bool ignoreCase = true) => _Struc.ContainsValue(aValue, bStringCompare, ignoreCase);

        public bool ContainsString(string aString) => ContainsValue(aString, true, true);


        public void SetMembers(List<dynamic> aMembers, int aStartIndex = -1, int aEndIndex = -1)
        {
            TVALUES struc = _Struc;
            try
            {
                var temp = _Struc;
                temp.Clear();
                int lb;
                int ub;
                //to do removed as of now
                //Dim vType As VbVarType
                //vType = VarType(aMembers) - vbArray
                lb = 0;
                ub = aMembers.Count - 1;
                if (ub - lb + 1 < 0) return;

                if (lb == 1 || aStartIndex >= 0 || aEndIndex >= 0) //vType<> vbVariant Then
                {
                    int si;
                    int ei;
                    int i;
                    si = lb;
                    ei = ub;
                    if (aStartIndex >= 0) si = aStartIndex;
                    if (aEndIndex >= 0) ei = aEndIndex;
                    mzUtils.SortTwoValues(true, ref si, ref ei);
                    for (i = si; i <= ei; i++)
                    {
                        if (i >= lb && i <= ub) temp.Add( aMembers[i]);
                    }
                    _Struc = temp;
                }
                else
                {
                    if (ub == 0 && lb == 0)
                    {
                        if (aMembers[0] == null) return;
                    }

                }
            }
            catch (Exception)
            {
            }
            finally
            {
                _Struc = struc;
            }
        }
        public double MinDifference(int aPrecis = -1) => _Struc.MinDifference(aPrecis);

        public double MaxDifference(int aPrecis = -1) => _Struc.MaxDifference(aPrecis);

        public List<double> ToNumericList(int aPrecis = -1, mzSortOrders Sort = mzSortOrders.None) => _Struc.ToNumericList(aPrecis, Sort);

        public string Name
        {
            get => _Struc.Name;
            set
            {
                var temp = _Struc;
                temp.Name = value;
                _Struc = temp;
            }
        }
        
        public dynamic BaseValue
        {
            get => _Struc.BaseValue;
            set {
                var temp = _Struc;
                temp.BaseValue = value;
                _Struc = temp;
            }
        }

        public dynamic Remove(int aIndex)
        {
            var temp = _Struc;
            dynamic removedObject = _Struc.Remove(aIndex);
            _Struc = temp;
            return removedObject;
        }

        public bool SetValues(dynamic aValue, string aSkipIDs = "", dynamic aNewCount = null)
        {
            var temp = _Struc;
            var boolResult = temp.SetValues(aValue, aSkipIDs, aNewCount);
            _Struc = temp;
            return boolResult;
        }

        public bool SetValue(int aIndex, dynamic aValue)
        {
            var temp = _Struc;
            var boolResult = temp.SetValue(aIndex, aValue);
            _Struc = temp;
            return boolResult;
        }

        //sorts the values in the array from low to high
        public mzValues Sort(bool bHighToLow = false, bool bConvertToDoubles = false, bool bRemoveDupes = false, bool bNumeric = false, int iPrecis = -1)
        {
            if (bConvertToDoubles) bNumeric = true;

            TVALUES aStruc = new TVALUES(_Struc);
            if (bNumeric) aStruc.SortNumeric(bHighToLow, bRemoveDupes, iPrecis); else aStruc.Sort(bHighToLow, bRemoveDupes);
            return new mzValues(aStruc);

        }
        public bool CompareStringWise(mzValues aValues, string aSkipList = "", bool bBailOnOne = false,string aDelimitor = ",",bool bIgnoreCase = true)
        {
            return (aValues != null) && TVALUES.CompareStrings(_Struc, aValues.Structure, aSkipList, aDelimitor, bIgnoreCase);
        }
        public bool CompareNumeric(mzValues aValues, int aPrecis = 6)
        {
            return (aValues != null) && TVALUES.CompareNumeric(_Struc, aValues.Structure,aPrecis);
        }

        public mzValues SubValues(int aStartIndex, int aEndIndex) => new mzValues(_Struc.SubValues(aStartIndex,aEndIndex ));

        public ArrayList ToArrayList() => TVALUES.ToArrayList(_Struc);

        public List<string> ToStringList(mzSortOrders Sort = mzSortOrders.None) => _Struc.ToStringList(Sort);


        public string ToList(string aDelim = ",", bool bNoNulls = false, string aLastDelim = null) => _Struc.ToDelimitedList(aDelim, bNoNulls, aLastDelim);

        public int FindStringValue(string aStringVal, string aLimitChar = "", int aOccur = 0) => _Struc.FindStringValue(aStringVal, aLimitChar, aOccur);

        /// <summary>
        /// pass index starting from 1 
        /// </summary>
        /// <param name="aIndex"></param>
        /// <returns></returns>
        public dynamic Item(int aIndex) => _Struc.Item(aIndex);

        public bool AddNumber(dynamic aValue, int rIndex = 0, bool bNoDupes = false, int aPrecis = 5) 
        {
            TVALUES struc = _Struc;
            bool _rVal = struc.AddNumber(aValue, bNoDupes, aPrecis);
            _Struc = struc;
            return _rVal;
        }


        public void PrintToDebug(bool bBaseZero = false) => _Struc.Print(bBaseZero);

        public dynamic ExtremeValue(bool bMax, bool bAbs , out int  rIndex ) => _Struc.ExtremeValue(bMax, bAbs, out rIndex);

        public mzValues UniqueValues(bool bNumeric = false, int aPrecis = -1) => new mzValues(_Struc.UniqueValues(bNumeric,aPrecis));

        public mzValues RemoveDuplicates(bool bNumeric = false, int aPrecis = -1)
        {
            TVALUES struc = _Struc;
            TVALUES _rVal = struc.RemoveDupes(bNumeric, aPrecis);
            _Struc = struc;
            return new mzValues(_rVal); 
        }
        
        public int ValueCount(dynamic aValue, bool bNumeric = false, int aPrecis = -1) => _Struc.ValueCount( aValue, bNumeric, aPrecis);


        public dynamic LastValue(string aDefault = "") => _Struc.LastValue(aDefault);

        public double Total(bool bAbsValue = false) => _Struc.Total(bAbsValue);


        #region Shared Methods
        public static mzValues FromList<T>( List<T>  aList, bool bNoDupes = false)
        {
            mzValues _rVal = new mzValues();
            if (aList == null) return _rVal;
            for(int i = 1; i <= aList.Count; i++) { _rVal.Add(aList[i - 1], bNoDupes); }


            return _rVal;
        }


        #endregion
    }
}
