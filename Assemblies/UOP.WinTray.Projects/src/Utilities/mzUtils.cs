using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Structures;

namespace UOP.WinTray.Projects.Utilities
{
    public enum mzEqualities
    {
        LessThan = 0,
        GreaterThan = 1,
        Equals = 2
    }

    public static class mzUtils { 

        public static mzEqualities CompareVal(double A, double B, int aPrecis = 5)
        {
            aPrecis = LimitedValue(aPrecis, 0, 15);
            double dif = Math.Round(A - B, aPrecis);
            if (dif == 0) return mzEqualities.Equals;
            return (dif > 0) ? mzEqualities.GreaterThan : mzEqualities.LessThan;
        }

        /// Creates new GUID
        /// </summary>
        /// <returns></returns>
        public static string CreateGUID() => Guid.NewGuid().ToString();
        
        public static string UnPascalCase(string aInput, string aDefault = "")
        {
            if (string.IsNullOrWhiteSpace(aInput)) return aDefault;
            char lastchar = '.';
            var sb = new StringBuilder();
            bool upper = true;

            for (var i = 0; i < aInput.Length; i++)
            {
                bool isUpperOrDigit = char.IsUpper(aInput[i]) || char.IsDigit(aInput[i]);
                // any time we transition to upper or digits, it's a new word
                if (!upper && isUpperOrDigit)
                {
                     if (lastchar != ' ') sb.Append(' ');
                }
                lastchar = aInput[i];
                sb.Append(aInput[i]);
                upper = isUpperOrDigit;
            }

            return sb.ToString();

        }
        /// <summary>
        /// Common method to convert Enum to specific type
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="val"></param>
        /// <returns></returns>
        public static TEnum GetEnumValue<TEnum>(dynamic val)
        {
            string enumToConv = string.Empty;
            if (mzUtils.IsNumeric(val))
            {
                enumToConv = Enum.GetName(typeof(TEnum), Convert.ToInt32(val));
            }
            else
            {
                enumToConv = val.ToString();
            }

            return (TEnum)Enum.Parse(typeof(TEnum), enumToConv.ToString());
        }

        /// <summary>
        /// Returns string value for the passed boolean
        /// </summary>
        /// <param name="aValue"></param>
        /// <param name="bYesNo"></param>
        /// <returns></returns>
        public static string BooleanToString(dynamic aValue, bool bYesNo = false)
        {
      
            if (bYesNo)
            {
                return mzUtils.VarToBoolean(aValue) ? "Yes" : "No";
            }
            else
            {
                return mzUtils.VarToBoolean(aValue) ? "True" : "False";
            }

        }

        public static string GetLastString(string aDelimitedString, char aDelimitor = '.')
        {
            if (string.IsNullOrEmpty(aDelimitedString)) return "";
            int i = aDelimitedString.LastIndexOf(aDelimitor);
            if (i < 0) return aDelimitedString;
            return aDelimitedString.Substring(i + 1);
        }

        public static bool CompareNumericList(List<double> List1, List<double> List2, int iPrecis = -1)
        {
   
            if (List1 == null || List2 == null) return false;
            if (List1.Count != List2.Count) return false;

            List<int> used = new List<int>();
            double val1;
            double val2;
            bool found = false;
            iPrecis = mzUtils.LimitedValue(iPrecis , -1, 15);

            for (int i = 0; i < List1.Count; i++)
            {
                val1 = List1[i];
                if (iPrecis >= 0) val1 = Math.Round(val1, iPrecis);
                found = false;
                for (int j = 0; j < List2.Count; j++)
                {
                    if (used.IndexOf(j) >= 0) continue;

                    val2 = (iPrecis >= 0) ? Math.Round(List2[j], iPrecis) : List2[j];
                    if(val1 == val2)
                    {
                        found = true;
                        used.Add(j);
                        break;
                    } 
                            
                }

                if (!found) 
                    break;
            }
            return used.Count == List1.Count;
       
        }

        public static void ResizeArray<T>(ref T[,] original, int rows, int cols)
        {

            var newArray = new T[rows, cols];
            int minRows = Math.Min(rows, original.GetLength(0));
            int minCols = Math.Min(cols, original.GetLength(1));
            for (int i = 0; i < minRows; i++)
            {
                for (int j = 0; j < minCols; j++)
                {
                    newArray[i, j] = original[i, j];
                }
                    

            }
            original = newArray;

            //var newArray = new T[newCoNum, newRoNum];
            //int columnCount = original.GetLength(1);
            //int columnCount2 = newRoNum;
            //int columns = original.GetUpperBound(0);
            //for (int co = 0; co <= columns; co++)
            //    Array.Copy(original, co * columnCount, newArray, co * columnCount2, columnCount);
            //original = newArray;
        }

        public static List<double> ValuesBetween(List<double> aValues, double aLowLim , double aHighLim , bool bOnIsIn = false, int iPrecis = 4)
        {
            List<double> _rVal = new List<double>();

            if (aValues is null) return _rVal;
            if (aValues.Count <= 0) return _rVal;

            mzUtils.SortTwoValues(true, ref aLowLim, ref aHighLim);
            iPrecis = mzUtils.LimitedValue(iPrecis, 0, 15);
            aLowLim = Math.Round(aLowLim, iPrecis);
            aHighLim = Math.Round(aHighLim, iPrecis);
            double comp;

            foreach (double dval in aValues)
            {
                comp = Math.Round(dval, iPrecis);
                if(comp >= aLowLim && comp <= aHighLim)
                {
                    if (comp == aLowLim || comp == aHighLim)
                    {
                        if (bOnIsIn) _rVal.Add(dval);
                    }
                    else
                    {
                        _rVal.Add(dval);
                    }

                }

            }


            return _rVal;
        }

        public static List<string> ListValues( string aList, string aDelimiter = ",", bool bReturnNulls = false, bool bTrim = false, bool bNoDupes = false, bool bNumbersOnly = false, int iPrecis = -1, bool bRemoveParens = false)
        {
            List<string> _rVal = new List<string>();
      
            string[] sVals;
            string sVal, aStr;
            bool bKeep = false;
            int cnt = 0;
            if (string.IsNullOrEmpty(aList)) return _rVal;
            aStr = aList.Trim();
            if (bRemoveParens)
            {
                if (aStr.Substring(0, 1).Equals("(")) aStr = aStr.Substring(1);
                if (aStr.Substring(aStr.Length - 1).Equals(")")) aStr = aStr.Substring(0, aStr.Length - 1);
                aStr = aStr.Trim();
            }
            if (string.IsNullOrEmpty(aStr)) return _rVal;
            if (aDelimiter == uopGlobals.Delim)
                aStr = mzUtils.FixGlobalDelimError(aStr);

            sVals = aStr.Split(Convert.ToChar(aDelimiter));
            if (bNumbersOnly) bReturnNulls = false;
            for (int i = 0; i < sVals.Length; i++)
            {
                sVal = sVals[i];
                if (bTrim || bNumbersOnly) sVal = sVal.Trim();
                bKeep = bReturnNulls || (!bReturnNulls && !string.IsNullOrEmpty(sVal.Trim()));
                if (bKeep)
                {
                    if (bNumbersOnly) sVal = mzUtils.VarToDouble(sVal, aPrecis: iPrecis).ToString();
                    if (bNoDupes)
                    {
                        for (int j = 0; j <= cnt - 1; j++)
                        {
                            if (string.Compare(_rVal[j], sVal, true) == 0)
                            {
                                bKeep = false;
                                break;
                            }
                        }
                    }
                }
                if (bKeep)
                {
                    cnt += 1;
                    _rVal.Add(sVal);
                }
            }
            
            return _rVal;
        }

        public static string FixGlobalDelimError(string aString)
        {
            if (string.IsNullOrWhiteSpace(aString)) return "";
            string _rVal = string.Empty;
            byte[] ASCIIValues = System.Text.Encoding.ASCII.GetBytes(aString);
            char[] schars = aString.ToCharArray();
            int i = 0;
            foreach (byte b in ASCIIValues)
            {
                if (b >= 32 && b <= 126 && b != 63)
                {
                    _rVal += schars[i];

                }
                else
                {
                    _rVal += uopGlobals.Delim;
                }
                i++;
            }
            return _rVal;
        }
        

        public static string ThisOrThat (string This, string That) { if (!string.IsNullOrWhiteSpace(This)) return This; return (!string.IsNullOrWhiteSpace(That))? That: "";  }

        public static double ThisOrThat(double This, double That,double CompareValue = 0) { return (This == CompareValue) ? That: This ; }

        public static dynamic ConvertDynamic(dynamic aValue,string aTypeName)
        {

            switch (aTypeName.ToUpper())
            {
                case "STRING":
                    return (aValue == null) ? string.Empty : Convert.ToString((object)aValue);

                case "INT":
                case "INTEGER":
                    return (aValue == null) ? 0 : Convert.ToInt64((object)aValue);

                case "BOOL":
                case "BOOLEAN":
                    return VarToBoolean(aValue);
                case null:
                    return (aValue == null) ? string.Empty : Convert.ToString((object)aValue);
            }
            if (IsNumeric(aValue))
            {
                return (aValue == null) ? 0 : VarToDouble (aValue);
            }
            else
            {
                return (aValue == null) ? string.Empty : Convert.ToString((object)aValue);
            }
            
        }
        public static dynamic ThisOrThat(object This, object That) => This ?? That; 

        public static byte VarToByte(dynamic aVariant)
        {
            return (byte)mzUtils.VarToInteger(aVariant, true, 0, 0, 255);
        }

        public static int VarToInteger(dynamic aVariant, bool bAbsoluteVal = false, dynamic aDefault = null, dynamic aMinVal = null, dynamic aMaxVal = null, uopValueControls aValueControl = uopValueControls.None)
        {

            if (aVariant is bool bval)
            {
                aVariant = bval ? -1 : 0;
            }

            double dVal = mzUtils.VarToDouble(aVariant, bAbsoluteVal, aDefault, 0, aMinVal, aMaxVal, aValueControl);
            if (dVal > int.MaxValue) dVal = (double)int.MaxValue;
            if (dVal < int.MinValue) dVal = (double)int.MinValue;
            return Convert.ToInt32(dVal);

        }


        public static long VarToLong(dynamic aVariant, bool bAbsoluteVal = false, dynamic aDefault = null, dynamic aMinVal = null, dynamic aMaxVal = null, uopValueControls aValueControl = uopValueControls.None)
        {


            double dVal = mzUtils.VarToDouble(aVariant, bAbsoluteVal, aDefault, 0, aMinVal, aMaxVal, aValueControl);
            if (dVal > long.MaxValue) dVal = (double)long.MaxValue;
            if (dVal < long.MinValue) dVal = (double)long.MinValue;
            return (long)Convert.ToInt64(dVal);

        }

        public static bool IsOdd(double spcs) =>  spcs % 2 != 0;

        public static bool IsOdd(int spcs) => spcs % 2 != 0;

        public static bool IsEven(int spcs) => spcs % 2 == 0;


        public static bool IsEven(double spcs) => spcs % 2 == 0;


        public static List<string> ListToCollection(string aList, string aDelimitor = ",", bool bReturnNulls = false)
        {
            TVALUES aVals = TVALUES.FromDelimitedList(aList, aDelimitor, bReturnNulls);
            List<string> _rVal = new List<string>();
            for(int i = 1; i <=aVals.Count; i++) { _rVal.Add(Convert.ToString(aVals.Item(i))); }
            return _rVal;
        }

        /// <summary>
        /// parses a delimited string based on the indicated delimitor and returns the values in a list
        /// </summary>
        /// <param name="aList"> the delimited list to parse</param>
        /// <param name="aDelimitor">the delimitor to split the string with</param>
        /// <param name="bReturnNulls">flag to indictate if null strings should be returned</param>
        /// <param name="bTrim">flag to indicate the members should be trimmed</param>
        /// <param name="bNoDupes">flag to disallow duplicate values in the return list. the test is not case sensitive</param>
        /// <param name="bNumbersOnly">flag to only return strings that are numbers</param>
        /// <param name="iPrecis">the precion to apply to the numbers if bNumbersOnly is true</param>
        /// <param name="bRemoveParens">flag to remove leading and trailing parren</param>
        /// <param name="aSkipList">a list of indexes to skip i.e. 1,4,7 </param>
        /// <returns></returns>

        public static  List<string> StringsFromDelimitedList(string aList, string aDelimitor = ",", bool bReturnNulls = false, bool bTrim = false, bool bNoDupes = false, bool bNumbersOnly = false, int iPrecis = -1, bool bRemoveParens = false, string aSkipList = null)
        {
            List<string> _rVal = new List<string>();
            if(string.IsNullOrWhiteSpace(aList))  return _rVal; 
             aList = aList.Trim();
            if (aDelimitor == uopGlobals.Delim)
                aList = mzUtils.FixGlobalDelimError(aList);

            if (bRemoveParens) { aList = aList.Replace("(", string.Empty).Replace(")", string.Empty).Trim(); }
            aSkipList = string.IsNullOrWhiteSpace(aSkipList) ? string.Empty : aSkipList.Trim();
            bool checkforskips = !string.IsNullOrWhiteSpace(aSkipList);
            string[] sVals = aList.Split(aDelimitor.ToCharArray());

            for (int i = 0; i < sVals.Length; i++)
            {
                if (checkforskips && mzUtils.ListContains($"{i + 1}" , aSkipList, bReturnTrueForNullList: false)) continue;
                string value = sVals[i];
                string trimval = value.Trim();
                if (bTrim || bNumbersOnly) value = trimval;

                if (bNumbersOnly && bReturnNulls && trimval == string.Empty) { value = "0"; trimval = "0"; }

                double dVal = 0;

                if (!bReturnNulls && trimval == string.Empty) continue;

                if (bNumbersOnly)
                {
                    if (value== string.Empty || !mzUtils.IsNumeric(value)) continue;

                    dVal = mzUtils.VarToDouble(value, aPrecis: iPrecis);
                    value = dVal.ToString();
                   
                }

                bool keep = true;

                if (bNoDupes)
                {
                    for (int j = 1; j <= _rVal.Count; j++)
                    {
                        string rVal = _rVal[j -1];
                        if (bNumbersOnly)
                        {
                            if (dVal == mzUtils.VarToDouble(rVal, aPrecis: iPrecis))
                            {
                                keep = false;
                                break;
                            }

                        }
                        else
                        {
                            if (value == rVal)
                            {
                                keep = false;
                                break;
                            }
                        }
                    }
                
                   
                }
                if (keep) _rVal.Add(value);
            }
            return _rVal;
        }

        /// <summary>
        /// parses a delimited string based on the indicated delimitor and returns the values in a list of doubles
        /// </summary>
        /// <param name="aList"> the delimited list to parse</param>
        /// <param name="aDelimitor">the delimitor to split the string with</param>
        /// <param name="bNullsAreZero">flag to indicate that null strings in the list should be interpreted as 0</param>
        /// <param name="bNoDupes">flag to disallow duplicate values in the return list</param>
        /// <param name="iPrecis">the precion to apply</param>
        /// <param name="aSkipList">a list of indexes to skip i.e. 1,4,7 </param>
        /// <returns></returns>

        public static List<double> DoublesFromDelimitedList(string aList, string aDelimitor = ",", bool bNullsAreZero = true, bool bNoDupes = false, int iPrecis = -1, string aSkipList = null)
        {
            if (string.IsNullOrWhiteSpace(aList)) return new List<double>();

            List<double> _rVal = new List<double>();
            List<string> stringvals = mzUtils.StringsFromDelimitedList(aList,aDelimitor,bReturnNulls: bNullsAreZero, bTrim:true, bNoDupes:false , bNumbersOnly:true, iPrecis: -1, bRemoveParens:true, aSkipList: aSkipList);
            if (stringvals.Count == 0) return new List<double>();

            foreach (var item in stringvals)
            {
                double dVal = mzUtils.VarToDouble(item, aPrecis: iPrecis);
                if (bNoDupes)
                {
                    if (_rVal.FindIndex((x) => x == dVal) >= 0) continue;
                }
                _rVal.Add(dVal);
            }

            return _rVal;

        }

        /// <summary>
        ///returns the smallest difference between any two adjacent members of the list
        /// </summary>
        /// <param name="aList"> the delimited list to parse</param>
        /// <param name="aDelimitor">the delimitor to split the string with</param>
        /// <param name="bNullsAreZero">flag to indicate that null strings in the list should be interpreted as 0</param>
        /// <param name="bNoDupes">flag to disallow duplicate values in the return list</param>
        /// <param name="iPrecis">the precion to apply</param>
        /// <param name="aSkipList">a list of indexes to skip i.e. 1,4,7 </param>
        /// <returns></returns>

        public static double MinDifference(string aList, string aDelimitor = ",", bool bNullsAreZero = true, bool bNoDupes = false, int iPrecis = -1, string aSkipList = null)
         => MinDifference(DoublesFromDelimitedList(aList, aDelimitor, bNullsAreZero, bNoDupes, -1, aSkipList), iPrecis);


        /// <summary>
        ///returns the largest difference between any two adjacent members of the list
        /// </summary>
        /// <param name="aList"> the delimited list to parse</param>
        /// <param name="aDelimitor">the delimitor to split the string with</param>
        /// <param name="bNullsAreZero">flag to indicate that null strings in the list should be interpreted as 0</param>
        /// <param name="bNoDupes">flag to disallow duplicate values in the return list</param>
        /// <param name="iPrecis">the precion to apply</param>
        /// <param name="aSkipList">a list of indexes to skip i.e. 1,4,7 </param>
        /// <returns></returns>

        public static double MaxDifference(string aList, string aDelimitor = ",", bool bNullsAreZero = true, bool bNoDupes = false, int iPrecis = -1, string aSkipList = null)
         => MaxDifference(DoublesFromDelimitedList( aList, aDelimitor,bNullsAreZero,bNoDupes, -1,aSkipList), iPrecis);
      

        /// <summary>
        /// returns the largest difference between any two adjacent members of the list
        /// </summary>
        /// <param name="aNumbers">a list of numbers to check</param>
        /// <param name="aPrecis">the precision to apply</param>
        /// <param name="bSortValues">flag to sort the numbers before doing the comparison</param>
        /// <returns></returns>

        public static double MaxDifference(List<double> aNumbers,  int aPrecis = -1, bool bSortValues = true)
        {
            if (aNumbers == null || aNumbers.Count <= 1) return 0;

            aPrecis = mzUtils.LimitedValue(aPrecis, -1, 15);

            List<double> dVals = new List<double>();
            dVals.AddRange(aNumbers); 

            if(bSortValues) dVals.Sort();

            double _rVal = 0;

            for (int i = dVals.Count - 1; i > 1; i--)
            {
                double aVal = dVals[i];
                double bVal = dVals[i - 1];

                if(aPrecis >= 0)
                {
                    aVal = Math.Round(aVal, aPrecis);
                    bVal = Math.Round(bVal, aPrecis);
                }

                double aDif = Math.Abs(aVal - bVal);
                if (aDif > _rVal) _rVal = aDif;
            }
            return _rVal;


        }
        /// <summary>
        /// returns the smalles difference between any two adjacent members of the list
        /// </summary>
        /// <param name="aNumbers">a list of numbers to check</param>
        /// <param name="aPrecis">the precision to apply</param>
        /// <param name="bSortValues">flag to sort the numbers before doing the comparison</param>
        /// <returns></returns>

        public static double MinDifference(List<double> aNumbers, int aPrecis = -1, bool bSortValues = true)
        {
            if (aNumbers == null || aNumbers.Count <= 1) return 0;

            aPrecis = mzUtils.LimitedValue(aPrecis, -1, 15);

            List<double> dVals = new List<double>();
            dVals.AddRange(aNumbers);

            if (bSortValues) dVals.Sort();

            double _rVal = double.MaxValue;

            for (int i = dVals.Count - 1; i > 1; i--)
            {
                double aVal = dVals[i];
                double bVal = dVals[i - 1];

                if (aPrecis >= 0)
                {
                    aVal = Math.Round(aVal, aPrecis);
                    bVal = Math.Round(bVal, aPrecis);
                }

                double aDif = Math.Abs(aVal - bVal);
                if (aDif < _rVal) _rVal = aDif;
            }
            return _rVal;


        }

        public static void AppendToStringList(ref List<string>  aStringList,  string aList, string aDelimitor = ",")
        {
            aStringList??= new List<string>();
            if (string.IsNullOrWhiteSpace(aList)) return;

            List<string> aVals = StringsFromDelimitedList(aList, aDelimitor, false);
            foreach (string s in aVals) { if (aStringList.FindIndex((x) => string.Compare(x, s, true) == 0) == -1) aStringList.Add(s); } 
        }
        public static void AppendToStringList(ref List<string> aStringList, List< string> aList)
        {
            aStringList ??= new List<string>();
            if (aList == null) return;

         
            foreach (string s in aList) { if (string.IsNullOrWhiteSpace(s)) continue; if (aStringList.FindIndex((x) => string.Compare(x, s, true) == 0) == -1) aStringList.Add(s); }
        }

        /// <summary>
        /// returns the passed delimited string as a list of doubles
        /// </summary>
        /// <param name="aList">the delimited list to parsre</param>
        /// <param name="aDelimitor">the delimitor used in the list</param>
        /// <param name="bZerosForNullValues">flag to interperet null strings in the list as zero</param>
        /// <param name="aPrecis">a precision to apply</param>
        /// <returns></returns>
        public static List<double> ListToNumericCollection(string aList, string aDelimitor = ",", bool bZerosForNullValues = false, int aPrecis = -1, bool bNoDupes = false )
        {
            TVALUES aVals = TVALUES.FromDelimitedList(aList, aDelimitor, bZerosForNullValues,true,bNoDupes);
            List<double> _rVal = new List<double>();
            for (int i = 1; i <= aVals.Count; i++) { _rVal.Add(mzUtils.VarToDouble(aVals.Item(i), aPrecis: aPrecis)); }
            return _rVal;
        }
        public static List<int> ListToIntegerCollection(string aList, string aDelimitor = ",")
        {
            List<int> _rVal = new List<int>();
            if (string.IsNullOrWhiteSpace(aList)) return _rVal;
            TVALUES aVals = TVALUES.FromDelimitedList(aList, aDelimitor, false);
            for (int i = 1; i <= aVals.Count; i++) { _rVal.Add(mzUtils.VarToInteger(aVals.Item(i))); }
            return _rVal;
        }

        public static string ListToString(List<string> aList, char aDelimitor = ',', bool bReturnNulls = false)
        {
            string _rVal = string.Empty;
            if (aList == null) return _rVal;
            string str;
            List<string> rvals = new List<string>();

            for (int i = 1; i <= aList.Count; i++) 
            {
                str = aList[i - 1];
                if (string.IsNullOrWhiteSpace(str)) str = string.Empty;
                if(bReturnNulls || (!bReturnNulls && !string.IsNullOrWhiteSpace(str))) rvals.Add(str);
           
             
                
            }

            for (int i = 1; i <= rvals.Count; i++)
            {
                str = rvals[i - 1];
                if (string.IsNullOrWhiteSpace(str)) str = string.Empty;
                if (bReturnNulls || (!bReturnNulls && !string.IsNullOrWhiteSpace(str)))
                {

                    if (_rVal !=  string.Empty) _rVal += aDelimitor;
                    _rVal += str;
                }

            }
            return _rVal;
        }

        /// <summary>
        /// returns the passed list of numbers as a delimited list
        /// </summary>
        /// <param name="aList"></param>
        /// <param name="aDelimitor"></param>
        /// <returns></returns>

        public static string ListToString(List<double> aList, char aDelimitor = ',', int aPrecis = 5, string aFmat = null, int aPad = 0 )
        {
            string _rVal = string.Empty;
            if (aList == null) return _rVal;
            string str;
            aPrecis = mzUtils.LimitedValue(aPrecis, 1, 15);
            string fmat = "0." + new string('0', aPrecis);
            if(!string.IsNullOrWhiteSpace(aFmat)) fmat = aFmat;
            string pad = string.Empty;
            if (aPad > 0) pad = new string(' ', aPad);
            List<string> rvals = new List<string>();

            for (int i = 1; i <= aList.Count; i++)
            {
                str = pad + aList[i - 1].ToString(fmat) + pad;
                
                rvals.Add(str);



            }

            for (int i = 1; i <= rvals.Count; i++)
            {
                str = rvals[i - 1];
               

                    if (_rVal !=  string.Empty) _rVal += aDelimitor;
                    _rVal += str;
            

            }
            return _rVal;
        }

        public static string ListToString(List<string> aList, string aDelimitor, string aFinalDelim = null, bool bReturnNulls = false, string aPrefix = null)
        {
            string _rVal = string.Empty;
            if (aList == null) return _rVal;
            string str;
            List<string> rvals = new List<string>();

            for (int i = 1; i <= aList.Count; i++)
            {
                str = aList[i - 1];
                if (string.IsNullOrWhiteSpace(str)) str = string.Empty;
                if (bReturnNulls || (!bReturnNulls && !string.IsNullOrWhiteSpace(str))) rvals.Add(str);
            }

            for (int i = 1; i <= rvals.Count; i++)
            {
                str = rvals[i - 1];
                if (string.IsNullOrWhiteSpace(str)) str = string.Empty;
                if (bReturnNulls || (!bReturnNulls && !string.IsNullOrWhiteSpace(str)))
                {

                    if (i == rvals.Count - 1 && aFinalDelim != null) aDelimitor = aFinalDelim;
                    if (_rVal !=  string.Empty) _rVal += aDelimitor;
                    _rVal += str;
                }

            }
            if (aPrefix != null) _rVal = aPrefix + _rVal;
            return _rVal;
        }

        public static string ListToString(List<int> aList, string aDelimitor, string aFinalDelim = null, bool bReturnNulls = false, string aPrefix = null)
        {
            string _rVal = string.Empty;
            if (aList == null) return _rVal;

            List<string> rvals = new List<string>();

            for (int i = 1; i <= aList.Count; i++)
            {
                rvals.Add(aList[i - 1].ToString());
            }

            return ListToString(rvals,aDelimitor,aFinalDelim,bReturnNulls, aPrefix);
        }
        public static bool SplitString(string aInput, string aDelimiter, out string rLeft, out string rRight)
        {
            rLeft = string.Empty;
            rRight = string.Empty;
            TVALUES sVals = TVALUES.FromDelimitedList(aInput, aDelimiter, true, true, false);
            if (sVals.Count >= 1) rLeft = Convert.ToString(sVals.Item(1));
            if (sVals.Count >= 2) rRight = Convert.ToString(sVals.Item(2));


            return sVals.Count >= 2;

        }

        public static int TrailingInteger(string aInput, string aDelimiter = " ",int aDefaultReturn = 0)
        {
            SplitString(aInput, aDelimiter, out string lval, out string rval);
            return mzUtils.IsNumeric(rval) ? mzUtils.VarToInteger(rval, aDefault: aDefaultReturn) : aDefaultReturn;
        }
        public static int LeadingInteger(string aInput, string aDelimiter = " ", int aDefaultReturn = 0)
        {
            SplitString(aInput, aDelimiter, out string lval, out string rval);
            return mzUtils.IsNumeric(lval) ? mzUtils.VarToInteger(lval, aDefault: aDefaultReturn) : aDefaultReturn;
        }
        /// <summary>
        /// sorts the passed two values as indicated
        /// returns True if the values were swapped
        /// </summary>
        /// <param name="LowToHigh"></param>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        public static bool SortTwoValues(bool LowToHigh, ref double val1, ref double val2)
        {
            if ((LowToHigh && val1 <= val2) || (!LowToHigh && val1 >= val2)) return false;
            double swap = val1;
            val1 = val2;
            val2 = swap;
            return true;
        }

        /// <summary>
        /// swaps the passed to values as indicated
        /// returns True if the values were swapped
        /// if a swap condition is passed and it evaluates to false the swap is aborted
        /// </summary>
        /// <param name="LowToHigh"></param>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        public static bool SwapTwoValues(ref double val1, ref double val2, bool? aCondition = null )
        {
            if (aCondition.HasValue) { if (aCondition.Value == false) return false; }
            double swap = val1;
            val1 = val2;
            val2 = swap;
            return true;
        }


        public static bool SortTwoValues(bool LowToHigh, ref int val1, ref int val2)
        {
            if ((LowToHigh && val1 <= val2) || (!LowToHigh && val1 >= val2)) return false;
            int swap = val1;
            val1 = val2;
            val2 = swap;
            return true;

        }


        public static bool SortTwoValues(bool LowToHigh, ref dynamic val1, ref dynamic val2)
        {
            if ((LowToHigh && val1 <= val2) || (!LowToHigh && val1 >= val2)) return false;
            dynamic swap = val1;
            val1 = val2;
            val2 = swap;
            return true;

        }

        public static void LoopLimits( int aLowIndex,  int aHighIndex, int aMin, int aMax,out int rStartIndex, out int rEndIndex)
        {
            rStartIndex = aLowIndex;
            rEndIndex = aHighIndex;
            if(aMax <= 0)
            {
                rStartIndex = 0;
                rEndIndex = 0;
                return;
            }
            if (aMin <= 0) aMin = 1;
            mzUtils.SortTwoValues(LowToHigh: true, ref aMin, ref aMax);
            if (rStartIndex < aMin || rStartIndex <= 0) rStartIndex = aMin;
            if (rStartIndex > aMax) rStartIndex = aMax;
            if (rEndIndex <= 0) rEndIndex = aMax;
            if (rEndIndex > aMax) rEndIndex = aMax;
            mzUtils.SortTwoValues(LowToHigh:true, ref rStartIndex, ref rEndIndex);

        }

        public static int LimitedValue(int aValue, int aMin, int aMax, int? aDefaultIfOutsideRange = null)
        {
            
            int _rVal = aMin;
            if (aMin > aMax) { aMin = aMax; aMax = _rVal; }
            _rVal = aValue;
            if(aDefaultIfOutsideRange.HasValue && (_rVal < aMin || _rVal > aMax))
            {
                return aDefaultIfOutsideRange.Value;
            }
            else
            {
                if (_rVal < aMin) _rVal = aMin;
                if (_rVal > aMax) _rVal = aMax;

                return _rVal;
            }



        }



        public static double LimitedValue(double aValue, double aMin, double aMax, double? aDefaultIfOutsideRange = null)
        {

            double _rVal = aMin;
            if (aMin > aMax) { aMin = aMax; aMax = _rVal; }
            _rVal = aValue;
            if (aDefaultIfOutsideRange.HasValue && (_rVal < aMin || _rVal > aMax))
            {
                return aDefaultIfOutsideRange.Value;
            }
            else
            {
                if (_rVal < aMin) _rVal = aMin;
                if (_rVal > aMax) _rVal = aMax;

                return _rVal;
            }

        }

        public static bool ListsStringCompare(string aList, string bList, string aSkipList , bool bBailOnOne , bool bTrim , out  mzValues rMistMatches, string aDelimitor = ",")
        {
            rMistMatches = new mzValues();
            bool _rVal = false;
            if (string.Compare(aList, bList, ignoreCase: true) ==0)
            {
                _rVal = true;
            }
            else
            {

                TVALUES aVals = TVALUES.FromDelimitedList(aList, aDelimitor, true, bTrim, false);
                TVALUES bVals = TVALUES.FromDelimitedList(bList, aDelimitor, true, bTrim, false);
                _rVal = TVALUES.CompareStrings(aVals, bVals, out TVALUES MSM, skipList: aSkipList, bBailOnOne: bBailOnOne, aDelimitor: aDelimitor);
                rMistMatches = new mzValues(MSM);
            }

            
            return _rVal;
        }

        public static bool ListContainsNumber(dynamic aValue, string aList, string aDelimitor = ",", bool bReturnTrueForNullList = false, int aPrecis = 4)
        {
            bool _rVal = false;
    
            if (string.IsNullOrWhiteSpace(aList)) return bReturnTrueForNullList;


            if (!mzUtils.IsNumeric(aValue)) return false;
            aPrecis = LimitedValue(aPrecis, 0, 10);

       
            double bVal = mzUtils.VarToDouble(aValue, aPrecis: aPrecis);
            string[] vList = aList.Split(aDelimitor.ToCharArray());
            for (int i = 0; i < vList.Count(); i++)
            {
                string aVal = vList[i];
                if (mzUtils.IsNumeric(aVal))
                {
                    if (Math.Round(Convert.ToDouble(aVal), aPrecis) == bVal)
                    {
                        _rVal = true;
              
                        break;
                    }
                }
            }
            return _rVal;
        }

        public static bool ListContainsNumber(dynamic aValue, string aList,  out int rStartID , string aDelimitor = ",", bool bReturnTrueForNullList = false, int aPrecis = 4)
        {
            bool _rVal = false;
          
            rStartID = 0;
            if (string.IsNullOrWhiteSpace(aList)) return bReturnTrueForNullList;


            if (!mzUtils.IsNumeric(aValue)) return false;
            aPrecis = LimitedValue(aPrecis, 0, 10);

            string aVal = string.Empty;
            double bVal  = mzUtils.VarToDouble(aValue, aPrecis: aPrecis);
            string[] vList = aList.Split(aDelimitor.ToCharArray());
            for (int i = 0; i < vList.Count(); i++)
            {
                aVal = vList[i];
                if (mzUtils.IsNumeric(aVal))
                {
                    if (Math.Round(Convert.ToDouble(aVal), aPrecis) == bVal)
                    {
                        _rVal = true;
                        rStartID = i + 1;
                        break;
                    }
                }
            }
            return _rVal;
        }

        public static double VersionToDouble(string aVersion, double aDefault = 0)
        {
            double _rVal = aDefault;
            if (string.IsNullOrWhiteSpace(aVersion)) return _rVal;

            string vers = string.Empty;
            string[] vparts = aVersion.Split('.');
            for (int i = 0; i < vparts.Length; i++)
            {
                if (i == 0)
                {
                    vers = vparts[i];
                }
                else if (i == 1)
                {
                    vers += "." + vparts[i];
                }
                else
                {
                    vers += vparts[i];
                }

            }
            _rVal = VarToDouble(vers, aDefault: aDefault);                
            return _rVal;
        }

        public static double VarToDouble(dynamic aVariant, bool bAbsoluteVal = false, dynamic aDefault = null, int aPrecis = -1, dynamic aMinVal = null, dynamic aMaxVal = null, uopValueControls aValueControl = uopValueControls.None)
        {
            double _rVal = 0;
            double iDef = 0;
            double dVal = 0;
            double defVal = 0;

            bool bLimits = (aMinVal != null) || (aMaxVal != null) || aValueControl > uopValueControls.None;
            bool numberPassed = aVariant != null;
            if (numberPassed)
            {
                if(aVariant is Enum)
                {
                    numberPassed = true;
                    dVal = (double)(int)aVariant;
                }
                else
                {
                    try
                    {
                        numberPassed = aVariant.ToString().Trim() != string.Empty && double.TryParse(aVariant.ToString(),out  dVal); // numberPassed = IsNumeric(aVariant, out dVal);
                        
                    }
                    catch
                    {
                        numberPassed = false;
                    }
                }
                
            }
            bool defaultPassed = aDefault != null;
            if (defaultPassed)
            {
                if (aDefault is Enum)
                {
                    defaultPassed = true;
                    defVal = (double)(int)aDefault;
                }
                else
                {
                    defaultPassed = aDefault.ToString().Trim() !=string.Empty && double.TryParse(aDefault.ToString(), out defVal);
                   // defaultPassed = IsNumeric(aDefault, out defVal);
                }
            }
          
                
            if (!numberPassed && !defaultPassed) return 0;

            try
            {

                _rVal = (!numberPassed && defaultPassed) ? defVal : dVal; 
                if (bAbsoluteVal) _rVal = Math.Abs(_rVal);
                if (aPrecis >= 0) _rVal = Math.Round(_rVal, mzUtils.LimitedValue(aPrecis, 0, 15));


                if (aMinVal != null || aMaxVal != null)
                { 
                    double lim = 0;
                    if (aMinVal != null)
                    {
                        lim = mzUtils.VarToDouble(aMinVal, aDefault: _rVal, aPrecis: aPrecis);
                        if (_rVal < lim) _rVal = lim;
                    }

                    if (aMaxVal != null)
                    {
                        if (mzUtils.IsNumeric(aMaxVal))
                        {
                            lim = mzUtils.VarToDouble(aMaxVal, aDefault: _rVal, aPrecis: aPrecis);
                            if (_rVal > lim) _rVal = lim;
                        }
                    }
                }

                if (aValueControl > uopValueControls.None)
                {


                    switch (aValueControl)
                    {
                        case uopValueControls.Positive:
                            if (_rVal > 0) break;
                            if (defaultPassed && defVal >= 0) _rVal = defVal;
                            _rVal = Math.Abs(_rVal);
                            break;
                        case uopValueControls.PositiveNonZero:
                            if (_rVal >= 0) break;
                            if (defaultPassed && defVal >= 0) _rVal = defVal;
                            if (_rVal == 0) _rVal = 1;
                            _rVal = Math.Abs(_rVal);
                            break;
                        case uopValueControls.Negative:
                            if (_rVal < 0) break;
                            if (defaultPassed && defVal <= 0) _rVal = defVal;
                            if (_rVal > 0) _rVal *= -1;

                            break;
                        case uopValueControls.NegativeNonZero:
                            if (_rVal <= 0) break;
                            if (defaultPassed && defVal < 0) _rVal = defVal;
                            if (_rVal == 0) _rVal = -1;
                            _rVal = Math.Abs(_rVal) * -1 ;

                            break;
                        case uopValueControls.NonZero:
                            if (_rVal != 0) break;
                            if (defaultPassed && defVal != 0) _rVal = defVal;
                            if (_rVal == 0) _rVal = 1;

                            break;
                    }



                }
                return _rVal;
            }
            catch (Exception)
            {
                return iDef;
            }
        }


        public static Single VarToSingle(Object aVariant, bool bAbsoluteVal = false, dynamic aDefault = null, int aPrecis = -1, dynamic aMinVal = null, dynamic aMaxVal = null, uopValueControls aValueControl = uopValueControls.None)
        {

            double dVal = mzUtils.VarToDouble(aVariant, bAbsoluteVal, aDefault, aPrecis, aMinVal, aMaxVal, aValueControl);
            if (dVal > float.MaxValue) dVal = float.MaxValue;
            if (dVal < float.MinValue) dVal = float.MinValue;
            return Convert.ToSingle(dVal);

        }

        public static string CreateIndexList(int aStart, int aEnd, string aItemPrefix = "", string aDelimitor = ",", bool bLowToHigh = true)
        {
            SortTwoValues(bLowToHigh, ref aStart, ref aEnd);
            string listitem = string.Empty;
            string _rVal = string.Empty;
            aItemPrefix ??= string.Empty;

            if (bLowToHigh)
            {
                for(int i = aStart; i <= aEnd; i++)
                {
                    listitem = $"{aItemPrefix}{i}";
                    mzUtils.ListAdd(ref _rVal, listitem, bSuppressTest: true, aDelimitor: aDelimitor);
                }
            }

            return _rVal;

        }

        public static bool ListAdd(ref string aList, dynamic aValue, string aDefaultValue = "", bool bSuppressTest = false, string aDelimitor = ",", int aPrecis = 0, bool bAllowNulls = false)
        {      
            if (string.IsNullOrWhiteSpace(aList)) bSuppressTest = true;
            string aVal = Convert.ToString(aValue);
            if (aPrecis > 0)
            {
                if (mzUtils.IsNumeric(aValue))  aVal = Convert.ToString(Math.Round(aValue, aPrecis));
                
            }
            if (aVal ==  string.Empty) aVal = aDefaultValue;

            if (string.IsNullOrWhiteSpace(aVal) && !bAllowNulls) return false;

            if (!bSuppressTest)
            {
                if (ListContains(aVal, aList, aDelimitor)) return false;
            }
            if (aList !=  string.Empty)
            {

                if(!aList.EndsWith(aDelimitor)) aList += aDelimitor;
            }
                

            aList += aVal;
            return true;
            
        }
        public static bool ListContains(dynamic aValue, string aList, string aDelimitor = ",", bool bReturnTrueForNullList = false) => ListContains(aValue, aList, out int _, aDelimitor, bReturnTrueForNullList);
        public static bool ListContains(dynamic aValue, string aList,  out int rIndex, string aDelimitor = ",", bool bReturnTrueForNullList = false)
        {
            bool _rVal = false;

            rIndex = -1;

            if (string.IsNullOrWhiteSpace(aList)) return bReturnTrueForNullList;

            string aVal = Convert.ToString(aValue);

            if (string.IsNullOrEmpty(aVal)) return bReturnTrueForNullList;
            if (String.IsNullOrEmpty(aDelimitor)) aDelimitor = ",";
            int lg = aDelimitor.Length;
            int rmv = 0;
                
            while (!(string.Compare(aList.Substring(0, lg), aDelimitor, true) != 0))
            {
                aList = aList.Substring(lg);
                rmv += lg;
                if (aList ==  string.Empty) return bReturnTrueForNullList;
                
            }

            lg = aVal.Length;
            if (lg == 0) return aList.IndexOf(aDelimitor + aDelimitor, StringComparison.OrdinalIgnoreCase) != -1;

            if (lg > aList.Length) return false;
            
            if (lg == aList.Length)
            {
                if (string.Compare(aList, aVal, true) == 0)
                {
                    rIndex = 1 + rmv;
                    return true;
                }
                return false;
            }
            if (string.Compare(aList.Substring(0, lg + 1), aVal + aDelimitor, true) == 0)
            {
                rIndex = 1 + rmv;
                return true;
            }
            else if (string.Compare(aList.Substring(aList.Length - lg - 1), aDelimitor + aVal, true) == 0)
            {
                rIndex = aList.Length - lg + 1 + rmv;
                _rVal = true;
            }
            else
            {
                rIndex = aList.IndexOf(aDelimitor + aVal + aDelimitor, StringComparison.OrdinalIgnoreCase);
                if (rIndex > 0)
                {
                    _rVal = true;
                    rIndex = rIndex + 1 + rmv;
                }
            }
            return _rVal;
        }
        
        /// <summary>
        /// IsNumeric Equivalent to VB6.0 IsNumeric which tells if value is numeric or not 
        /// </summary>
        /// <param name="aIndex"></param>
        /// <returns></returns>
        public static bool IsNumeric(dynamic aVariant, out double rDoubleVal )
        {
            rDoubleVal = 0;
            try
            {

                if (aVariant == null) return false;
                var t = aVariant.GetType();

                if (t == typeof(string))
                {
                    string sval = (string)aVariant;
                    if (string.IsNullOrWhiteSpace(sval)) return false;

                    return double.TryParse(sval, out rDoubleVal);
                }
                else if (t == typeof(double) || t == typeof(int) || t == typeof(float) || t == typeof(long))
                {
                    rDoubleVal = (double)aVariant;
                    return true;
                }
                else
                {
                    if (t == typeof(bool)) return false;

                    return double.TryParse(aVariant.ToString(), out rDoubleVal);

                }
            }
            catch (Exception)
            {
                rDoubleVal = 0;
                return false;
            }
        }

        /// <summary>
        /// IsNumeric Equivalent to VB6.0 IsNumeric which tells if value is numeric or not 
        /// </summary>
        /// <param name="aIndex"></param>
        /// <returns></returns>
        public static bool IsNumeric(dynamic aVariant)
        {
            double dbl = 0;

            try
            {

                if (aVariant == null) return false;
                if (aVariant is Enum) return true;
                var t = aVariant.GetType();
                
                if (t == typeof(string))
                {
                    string sval = (string)aVariant;
                    if (string.IsNullOrWhiteSpace(sval)) return false;
                   
                    return double.TryParse(sval, out dbl);
                }else if (t == typeof(double) || t == typeof(int) || t == typeof(float) || t == typeof(long))
                {
                    dbl = (double)aVariant;
                    return true;
                }
                else
                {
                    if (t == typeof(bool)) return false;

                    return double.TryParse(aVariant.ToString() , out dbl);

                }
            }
            catch (Exception)
            {
                return false;
            }
        }


        public static void GetLoopIndices( int aLowVal, int aHighVal, dynamic aLowInput, dynamic aHighInput, out int iStart,out int iEnd, bool bHighToLow = false )
        {
            iStart = Math.Abs(aLowVal);
            iEnd = Math.Abs(aHighVal);
            mzUtils.SortTwoValues(LowToHigh: true, ref iStart, ref iEnd);

            if (aLowInput != null && mzUtils.IsNumeric(aLowInput)) iStart = mzUtils.VarToInteger(aLowInput, aMinVal: iStart, aMaxVal: iEnd);

            if (aHighInput != null && mzUtils.IsNumeric(aHighInput)) iEnd = mzUtils.VarToInteger(aHighInput, aMinVal: iStart, aMaxVal: iEnd);

           if(bHighToLow)  mzUtils.SortTwoValues(LowToHigh:false, ref iStart, ref iEnd);


        }

        public static bool IsDate(dynamic aValue)
        {
           
            return  DateTime.TryParse(aValue, out DateTime _);
        }
        
        public static bool IsObject(dynamic val)
        {
            if (val.GetType() == typeof(int)) return false;
            else if (val.GetType() == typeof(double)) return false;
            else if (val.GetType() == typeof(string)) return false;
            else if (val.GetType() == typeof(bool)) return false;
            else return true;
        }

        public static bool VarToBoolean(Object aVariant, bool aDefault = false)
        {
            if (aVariant == null) return aDefault;


            try
            {
                if (aVariant.GetType() == typeof(bool)) return (bool)aVariant;
                if (mzUtils.IsNumeric(aVariant)) return mzUtils.VarToInteger(aVariant) == -1;
                string aStr = Convert.ToString(aVariant).ToUpper().Trim();
                return (aStr == "TRUE") || (aStr == "YES") || (aStr == "Y") || (aStr == "T");

            }
            catch (Exception)
            {
                return aDefault;
            }
        }

        internal static List<int> SortTwoLists(List<dynamic> aList, List<dynamic> bList, bool bHighToLow = false)
        {
            List<int> _rVal = new List<int>();
            if (aList == null) return _rVal;
            if (aList.Count <= 0) return _rVal;
            
            if (bList == null) bList = Force.DeepCloner.DeepClonerExtensions.DeepClone<List<dynamic>>(aList);

            List<dynamic> bListOrig =  Force.DeepCloner.DeepClonerExtensions.DeepClone<List<dynamic>>(bList);

            _rVal = mzUtils.SortDynamicList(aList, bHighToLow, bBaseOne: true);

            int idx;
            for (int i = 0; i < _rVal.Count; i++)
            {
                if (i + 1 > bList.Count) break;
                idx = _rVal[i];
                if(idx <= bList.Count)
                {
                    bList[i] = bListOrig[idx - 1];
                }
            }


            return _rVal;



        }

        public static string NumberFormatString(int aPrecision, bool bIncludeThousandSep = false)
        {
            aPrecision = mzUtils.LimitedValue(aPrecision, 0, 8);
            string slft = bIncludeThousandSep ? "#,#" : "#";
            string srgt =(aPrecision <= 0) ? string.Empty : "." + new string('0',aPrecision)  ;
            return slft + srgt;

        }

        public static List<int> SortDynamicList(List<dynamic> aList, bool bHighToLow = false, bool bBaseOne = false)
        {
            List<int> _rVal = new List<int>();
            if (aList == null) return _rVal;
            if (aList.Count <= 0) return _rVal;
            dynamic aVal;
            Dictionary<int, dynamic> keyValuePairs = new Dictionary<int, dynamic>();
      

            for (int i = 0; i < aList.Count; i++)
            {
                aVal = aList[i];
                if(aVal != null)
                {
                    if (bBaseOne)
                    { keyValuePairs.Add(i + 1, aVal); }
                    else
                    { keyValuePairs.Add(i, aVal); }

                }

            }
            aList?.Sort();

            if (bHighToLow) aList?.Reverse();
           

            for (int i = 0; i < aList?.Count; i++)
            {
                var aMem = aList[i];
                var dicItem =  keyValuePairs.First(item => item.Value == aMem);
               _rVal.Add((int)dicItem.Key);
               keyValuePairs.Remove(dicItem.Key);
                
            }

            return _rVal;
        }

        public static string ListAppend(string aList, dynamic aSubList, bool bUniqueValues = true, string aDelimitor = ",", bool bNoNulls = true)
        {
            string _rVal = aList;
            TVALUES aVals = TVALUES.FromDelimitedList(aSubList, aDelimitor, !bNoNulls, bNoDupes: bUniqueValues);
            if (aVals.Count <= 0) return _rVal;
            for (int i = 1; i <= aVals.Count; i++)
            {
                mzUtils.ListAdd(ref _rVal, aVals.Item(i), bSuppressTest: !bUniqueValues, aDelimitor: aDelimitor, bAllowNulls: !bNoNulls);
            }
            return _rVal;
        }


        /// <summary>
        /// To Do
        /// </summary>
        /// <param name="d1"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static string Format(double d1, string v)
        {
            return string.Format($"{{0:{v}}}", d1);
        }
        /// <summary>
        /// To Do
        /// </summary>
        /// <param name="CharCode"></param>
        /// <returns></returns>
        public static string SafeChr(int CharCode)
        {
            if (CharCode > 255)
            {
                throw new ArgumentOutOfRangeException("CharCode", CharCode,
                          "CharCode must be between 0 and 255.");
            }
            return string.Empty;
            //return System.Text.Encoding.Default.GetString()
            ///return System.Text.Encoding.ASCII.GetString({ CByte(CharCode)})
        }

        /// <summary>
        /// return 0 in value is False otherwise -1
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int CInt(bool value)
        {
            return value ? -1 : 0;
        }

    
        /// <summary>
        /// Extract left part of the string 
        /// need to be careful while passing arguments to this method if it is used with other inbuilt methods.
        /// eg.while calling this method with indexOf(),need to pass actual value+1 beacuse of indexing diffrence in C# and VB
        /// </summary>
        /// <param name="param">input string</param>
        /// <param name="length">lenght</param>
        /// <returns>length</returns>
        public static string Left(string param, int length)
        {
            string result = string.Empty;
            try
            {
                result = param?.Substring(0, length);
            }
            catch (Exception e)
            {
                throw e;
            }
            return result;
        }
        /// <summary>
        /// Extract Right part of the string
        /// need to be careful while passing arguments to this method if it is used with other inbuilt methods.
        /// eg.while calling this method with indexOf(),need to pass actual value+1 beacuse of indexing diffrence in C# and VB
        /// </summary>
        /// <param name="param"></param>
        /// <param name="length"></param>
        /// <returns>Right part of the string from length</returns>
        public static string Right(string param, int length)
        {
            string result = string.Empty;
            try
            {
                result = param.Substring(length);
                //can be acheived using below code
                //result = param.Substring(param.Length - length, length);
            }
            catch (Exception e)
            {
                throw e;
            }
            return result;
        }

        /// <summary>
        /// converts the passed number to it's relative alphabetic character
        /// i.e. 1 = "A",  2 = "B" etc.
        /// numbers greater than 26 are turned into a string of mutilple letters (27 = "AA")
        /// </summary>
        /// <param name="aNum">the number to convert</param>
        /// <returns></returns>
        public static string ConvertIntegerToLetter(dynamic aNum)
        {
            int pnum = mzUtils.VarToInteger(aNum, true);
            int nnum = pnum;
            string c1;
            if (pnum > 26)
            {
                int times = pnum / 26;
                c1 = $"{(char)(times - 1 + 65)}";
                nnum = pnum - (times * 26) - 1;
                if (nnum < 0)
                {
                    nnum = 0;
                }
            }
            else
            {
                c1 = $"{(char)(pnum - 1 + 65)}";
            }
            string _rVal;
            if (pnum > 26)
            {
                _rVal = c1 + $"{(char)(nnum + 65)}";
            }
            else
            {
                _rVal = c1;
            }

            return _rVal;
        }

        /// <summary>
        /// converts the passed character to a number
        /// i.e. "A" = 1 or "a" = 1 or "Z" = 26 OR "AA" = 27 or "BA" = 53
        /// </summary>
        /// <param name="aLetter">the character to convert to a number</param>
        /// <returns></returns>
        public static int ConvertLetterToInteger(string aLetter)
        {
            int _rVal = 0;
            char C;
            string ltr = aLetter.ToUpper().Trim();
            if (ltr.Length == 0)
            {
                return _rVal;
            }
            if (ltr.Length > 2)
            {
                ltr = ltr.Substring(0, 2);
            }

            C = ltr[0];
            int asci1 = C;
            if (asci1 < 65 || asci1 > 90)
            {
                asci1 = 65;
            }
            asci1 -= 64;
            if (ltr.Length == 1)
            {
                _rVal = asci1;
            }
            else
            {
                C = ltr[1];
                int asci2 = C;
                if (asci2 < 65 || asci2 > 90)
                {
                    asci2 = 65;
                }
                asci2 -= 64;
                _rVal = asci1 * 26 + asci2;
            }
            return _rVal;
        }

        /// <summary>
        /// used to convert an angle to a positive counterclockwise value <= 360
        /// </summary>
        /// <param name="aAngle">the angle to normalize</param>
        /// <param name="bInRadians">flag indicating if the passed value is in radians</param>
        /// <param name="ThreeSixtyEqZero"></param>
        /// <param name="bReturnPosive"></param>
        /// <returns></returns>
        public static double NormAng(double aAngle, bool bInRadians = false, bool ThreeSixtyEqZero = false, bool bReturnPosive = false)
        {
            double _rVal;
            try
            {
                int aSgn = 0;
                double aMax = 0;

                if (aAngle < 0)
                {
                    aSgn = -1;
                }
                else
                {
                    aSgn = 1;
                }
                aAngle = Math.Abs(aAngle);
                aAngle = Math.Round(aAngle, 6);
                if (bInRadians)
                {
                    aMax = 2 * Math.PI;
                }
                else
                {
                    aMax = 360;
                }

                //first get it down to less than 360
                while (aAngle > aMax)
                {
                    aAngle -= aMax;
                }

                if (ThreeSixtyEqZero && aAngle == aMax)
                {
                    aAngle = 0;
                }

                _rVal = aSgn * aAngle;
                if (bReturnPosive && aSgn == -1)
                {
                    _rVal = aMax + _rVal;
                }

                return _rVal;
            }
            catch (Exception)
            {
                _rVal = aAngle;
                if (bInRadians)
                {
                    _rVal = _rVal * Math.PI / 180;
                }
                return _rVal;
            }
        }

        /// <summary>
        /// return part of string using startindex and length
        /// </summary>
        /// <param name="param">input string</param>
        /// <param name="startIndex">strat index</param>
        /// <param name="length">number of character</param>
        /// <returns></returns>
        public static string Mid(string param, int startIndex, int length)
        {
            string result = string.Empty;
            try
            {
                result = param.Substring(startIndex, length);
            }
            catch (Exception e)
            {
                throw e;
            }
            return result;
        }

        public static string RemoveLeadChar(string aString, Char aChar)
        {
            string _rVal = aString;
            if (string.IsNullOrEmpty(aString)) return aString;
            while (_rVal.StartsWith(aChar.ToString())){
                _rVal = mzUtils.Right(_rVal, _rVal.Length - 1).Trim();
                if (_rVal.Length <= 0) break;
            }

            return _rVal;
        }


        public static string RemoveTrailingChar(string aString, Char aChar)
        {
            string _rVal = aString;
            if (string.IsNullOrEmpty(aString)) return aString;
            while (_rVal.EndsWith(aChar.ToString()))
            {
                _rVal = mzUtils.Left(_rVal, _rVal.Length - 1).Trim();
                if (_rVal.Length <= 0) break;
            }

            return _rVal;
        }

        public static bool BitCodeContains(dynamic aBitCode, dynamic aSearchVal)
        {
        
            mzUtils.DecomposeBitCode(aBitCode, out string sMEMS, aSearchVal, out bool _rVal);
            return _rVal;
        }

        public static bool SplitIntegers(string aString, out int rInt1, out int rInt2, char aDelimitor = '-', int aDefault1 = 0, int aDefault2 = 0)
        {
            rInt1 = aDefault1;
            rInt2 = aDefault2;
            if (string.IsNullOrWhiteSpace(aString)) return false;

            aString = aString.Trim();
            int dash = aString.IndexOf(aDelimitor);
            if (dash < 0) return false;
            string left = dash > 0 ? aString.Substring(0, dash).Trim() : "";
            string right =  dash +1 < aString.Length ? aString.Substring(dash+1, aString.Length -dash-1).Trim(): "";

            rInt1 = mzUtils.IsNumeric(left) ? mzUtils.VarToInteger(left) : aDefault1;

            rInt2 = mzUtils.IsNumeric(right) ? mzUtils.VarToInteger(right) : aDefault2;

            return mzUtils.IsNumeric(left) && mzUtils.IsNumeric(right);



        }

        public static int ExtractInteger(dynamic aStrng, bool bReturnNegatives, out string rSuffix)
        {
            return ExtractInteger(aStrng, out string _, out rSuffix, bReturnNegatives);
        }

        public static int ExtractInteger(dynamic aStrng,  bool bReturnNegatives = false)
        {
            return ExtractInteger(aStrng, out string _, out string _, bReturnNegatives);
        }
            public static int ExtractInteger(dynamic aStrng, out string rSuffix, out string rPrefix, bool bReturnNegatives = false) 
        {

            rSuffix = string.Empty;
            rPrefix = string.Empty;
            if (string.IsNullOrWhiteSpace(aStrng)) return 0;
            int i = 0;

            string astr = aStrng.Trim();
            astr = Regex.Replace(astr, @"s", "");
            if (string.IsNullOrWhiteSpace(astr)) return 0;
         
            bool bInNum = false;
            string sNum = string.Empty;
            char[] chars = astr.ToCharArray();
            char lCr = 'x';
            char nextchar = 'x';
            bool bNeg = false;

            int _rVal = 0;

            foreach (var item in chars)
            {
                i++;
                if (i < chars.Length) nextchar = chars[i];
                if (Char.IsDigit(item))
                {
                    if(!bInNum)
                    {
                        
                        sNum += item;
                        bInNum = true;

                    }
                    else
                    {
                        sNum += item;
                    }
                }
                else
                {
                   
                    if(!bInNum)
                    {
                        if (Char.IsDigit(nextchar))
                        {
                            if(item == '-' && bReturnNegatives) 
                            {
                                bNeg = true;
                            }
                            else
                            {
                                rPrefix += item;
                            }
                        }
                        else { rPrefix += item; }

                    } else
                    { 
                        bInNum = false; 
                        rSuffix += item; 
                    }
                }
                lCr = item;
            }

            if (!string.IsNullOrWhiteSpace(sNum))
            {
                _rVal = mzUtils.VarToInteger(sNum);
                if (bNeg) _rVal *= -1;
            }


            return _rVal;


        }
        internal static List<int> DecomposeBitCode(dynamic aValue) => DecomposeBitCode(aValue, out string _, null, out bool _, false);
        


        internal static List<int> DecomposeBitCode(dynamic aValue, out string rMemberList, dynamic aSearchVal , out bool rSearchValueFound, bool bReturnOnlySearchVal = false)
        {
            rSearchValueFound = false;
            rMemberList = string.Empty;
            List<int> _rVal = new List<int> ();
            try
            {
                
               
                if (!mzUtils.IsNumeric(aValue)) return _rVal;
                
                int lVal = mzUtils.VarToInteger(aValue);
                if (lVal <= 0) return _rVal;
                
                int i = 0;
                int mVal = 0;
                int aVal = 0;
                List<int> sVals = new List<int>();
                int svalidx = 0;
                aVal = 1;
                i = 1;
                sVals.Add(aVal);
                while (mVal < lVal)
                {
                    if (i > 1)
                    {
                        aVal *= 2;
                        sVals.Add(aVal);
                    }
                    mVal += aVal;
                    i += 1;
                }
                for (i = sVals.Count; i >= 1; i--)
                {
                    aVal = sVals[ i- 1];
                    if (lVal >= aVal)
                    {
                        _rVal.Add(aVal);
                        mzUtils.ListAdd(ref rMemberList, aVal, bSuppressTest: true);
                        if (aSearchVal != null)
                        {
                            if (aVal == aSearchVal) rSearchValueFound = true;
                            
                            svalidx = i;
                        }
                        while (lVal >= aVal) { lVal -= aVal; }
                    }
                }
                if (bReturnOnlySearchVal && aSearchVal != null)
                {
                    sVals = new  List<int>();
                    if (svalidx > 0) sVals.Add(_rVal[svalidx - 1]);
                    _rVal = sVals;
                }

                return _rVal;
            }
            catch (Exception)
            {
                return _rVal;
            }

        }
    }
}
