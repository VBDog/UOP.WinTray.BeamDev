using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UOP.DXFGraphics;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Materials;
using UOP.WinTray.Projects.Parts;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;

namespace UOP.WinTray.Projects.Utilities
{
    public class uopUtils
    {

        public static uppDrawingTypes PartTypeToDrawingType(uppPartTypes ptype, out string rPartTypeName, out string reSubCategory)
        {
            rPartTypeName = ptype.GetDescription();
            uppDrawingTypes _rVal = ptype switch
            {
                uppPartTypes.APPan => uppDrawingTypes.APPan,
                uppPartTypes.Stiffener => uppDrawingTypes.Stiffener,
                uppPartTypes.Deflector => uppDrawingTypes.DeflectorPlate,
                uppPartTypes.DowncomerBox => uppDrawingTypes.DowncomerBox,
                uppPartTypes.EndPlate => uppDrawingTypes.EndPlate,
                uppPartTypes.DeckSection => uppDrawingTypes.DeckPanel,
                uppPartTypes.EndSupport => uppDrawingTypes.EndSupport,
                uppPartTypes.EndAngle => uppDrawingTypes.EndAngles,
                uppPartTypes.SupplementalDeflector => uppDrawingTypes.SupplDeflectors,
                uppPartTypes.ManwaySplicePlate => uppDrawingTypes.ManwaySplicePlate,
                uppPartTypes.SpliceAngle => uppDrawingTypes.SpliceAngles,
                uppPartTypes.ManwayAngle => uppDrawingTypes.SpliceAngles,
                uppPartTypes.TraySupportBeam => uppDrawingTypes.BeamDetails,
                _ => uppDrawingTypes.Undefined
            };

            if (ptype == uppPartTypes.DowncomerBox)
            {
                reSubCategory = "DOWNCOMERS";
            }
            else if (ptype == uppPartTypes.DeckSection)
            {
                reSubCategory = "DECK SECTIONS";
            }
            else if (ptype == uppPartTypes.EndAngle || ptype == uppPartTypes.SpliceAngle || ptype == uppPartTypes.ManwayAngle || ptype == uppPartTypes.ManwaySplicePlate || ptype == uppPartTypes.SupplementalDeflector)
            {
                reSubCategory = "TABULAR PARTS";
            }
            else if (ptype == uppPartTypes.TraySupportBeam)
            {
                reSubCategory = "BEAMS";
            }
            else
            {
                reSubCategory = $"{rPartTypeName.ToUpper()}S";
            }

            return _rVal;
        }

        public static string Handle(string aTag, string aFlag, string aDelimitor = ",")
        {
                if (string.IsNullOrWhiteSpace(aTag) && string.IsNullOrWhiteSpace(aFlag)) return string.Empty;
                if (!string.IsNullOrWhiteSpace(aTag) && !string.IsNullOrWhiteSpace(aFlag)) return $"{aTag}{aDelimitor}{aFlag}";
                if (!string.IsNullOrWhiteSpace(aTag)) return $"{aTag}";
                if (!string.IsNullOrWhiteSpace(aFlag)) return $"{aFlag}";
                return string.Empty;
            
        }

        /// <summary>
        /// spaces points along the passed line using the indicated spacing
        /// </summary>
        /// <param name="aLine">the line to layout the points on</param>
        /// <param name="aTargetSpace">the requested distance between points</param>
        /// <param name="bCenterOnLine">flag to center the points on then midpoint of the line</param>
        /// <param name="aEndBuffer"></param>
        /// <param name="bAtLeastOne"></param>
        /// <param name="aMinSpace"></param>
        /// <param name="bSaveToLine">if true, the computed points are saved tot he lines points array</param>
        /// <returns></returns>
        public static uopVectors LayoutPointsOnLine(uopLine aLine, double aTargetSpace, bool bCenterOnLine = true, double aEndBuffer = 0, bool bAtLeastOne = false, double ? aMinSpace = null, bool bSaveToLine = false, string aTag = "", string aFlag = "")
         => LayoutPointsOnLine(aLine, out _, aTargetSpace, bCenterOnLine, aEndBuffer, bAtLeastOne, aMinSpace, bSaveToLine,aTag: aTag, aFlag:aFlag);
            
        /// <summary>
        /// spaces points along the passed line using the indicated spacing
        /// </summary>
        /// <param name="aLine">the line to layout the points on</param>
        /// <param name="aTargetSpace">the requested distance between points</param>
        /// <param name="bCenterOnLine">flag to center the points on then midpoint of the line</param>
        /// <param name="aEndBuffer"></param>
        /// <param name="bAtLeastOne"></param>
        /// <param name="aMinSpace"></param>
        /// <param name="bSaveToLine">if true, the computed points are saved tot he lines points array</param>
        /// <returns></returns>
        public static uopVectors LayoutPointsOnLine(uopLine aLine, out double rSpace, double aTargetSpace, bool bCenterOnLine = true, double aEndBuffer = 0, bool bAtLeastOne = false, double? aMinSpace = null, bool bSaveToLine = false, int? aTargetCount = null, bool bExactSpacing = false, string aTag = "", string aFlag = "")
        {
            rSpace = 0;
            if (aLine == null) return uopVectors.Zero;
            uopVectors _rVal = uopVectors.Zero;
            aTargetSpace = Math.Abs(aTargetSpace);

            if (aMinSpace.HasValue)
            {
                if (aTargetSpace < Math.Abs(aMinSpace.Value)) aTargetSpace = Math.Abs(aMinSpace.Value);
            }

            if (aTargetSpace == 0) bExactSpacing = false;
            double llen = aLine.Length - 2 * aEndBuffer;
            uopVector ctr = aLine.MidPt;
            uopVector linedir = aLine.Direction;
            uopVector sp = ctr + linedir * (-llen / 2);
            int numpt = 0;
            double spc = 0;
            int spaces = 0;
            bool exactcount = false;
            if (aEndBuffer <= 0) aEndBuffer = 0;
            if (aTargetCount.HasValue && aTargetCount.Value >= 1) exactcount = true; 
                if (llen <= 0)
            {
                if (bAtLeastOne) _rVal.Add(bCenterOnLine ? ctr: sp);
                return _rVal;
            }

            try
            {
                
                if (!exactcount)
                {
            
                    spc = aTargetSpace;
                    spaces = (int)Math.Truncate(llen / spc); // + 1;
                    if(spaces == 0 && !bExactSpacing)
                    {
                        double dif = llen -spc;
                        
                        if (dif < 0)
                        {
                            double redux = Math.Abs(dif / llen);
                            if(redux < 0.1)
                            {
                                spc -= redux * spc;
                                spaces = 1;
                                
                            }
                        }
                    }
                    numpt = spaces + 1;
               
                }
                else
                {

                       numpt = aTargetCount.Value;
                    spaces = numpt - 1;
                    if (bExactSpacing)
                        spc = aTargetSpace;
                    else
                        spc = llen / spaces;
                    

                    //if (!bCenterOnLine  && !bExactSpacing)
                    //    spc = llen / spaces;

                    
                }
              
            }
            finally
            {
                uopVector v1 = null;
                uopVector offset = linedir * spc;
                rSpace = spc;
                if (numpt == 0)
                {
                    if(bAtLeastOne) _rVal.Add(bCenterOnLine ? ctr : sp);

                }
                else if (numpt == 1)
                {
                    _rVal.Add(bCenterOnLine ? ctr : sp);

                }
                else
                {
                    if (bCenterOnLine)
                    {
                        double setback = 0.5 * spaces * spc;
                       for(int i = 1; i<= numpt; i++)
                        {
                            if (v1 == null)
                                v1 = new uopVector(ctr) {  Tag = aTag, Flag = aFlag }  + linedir * -setback;
                            else
                                v1 += offset;

                            if (!bExactSpacing && !exactcount)
                            {
                                if (v1.DistanceTo(ctr, 3) > Math.Round(llen / 2, 3))
                                    continue;

                            }
                            _rVal.Add(new uopVector(v1), aTag: aTag, aFlag:aFlag);
                        }

                    }
                    else
                    {



                        for (int i = 1; i <= numpt; i++)
                        {
                            if(v1 == null)
                                v1 = new uopVector(sp) { Tag= aTag, Flag =aFlag};
                            else
                                v1 += offset;
                            if (!bExactSpacing && !exactcount)
                            {
                                if (v1.DistanceTo(ctr, 3) > Math.Round(llen / 2, 3))
                                    continue;

                            }

                            _rVal.Add(new uopVector(v1), aTag: aTag, aFlag: aFlag);
                        }
                    }
                      
                    //if (bCenterOnLine)
                    //{
                    //    uopVector v2 = sp.MidPt(v1);  // midpoint of the return set
                    //    uopVector dir = v2.DirectionTo(ctr, false, out bool flag, out double d1);
                    //    if (!flag) _rVal.Translate(dir * d1);
                    //}
                }

              
                if (bSaveToLine) 
                    aLine.Points.Populate(_rVal, true);

            }
            return _rVal;

        }

        public static double DeckGapValue(uppSpliceTypes sType,bool bForManway)
        {
       
            if (sType == uppSpliceTypes.SpliceWithTabs)
            { return 0.0625; }
            else if (sType == uppSpliceTypes.SpliceWithAngle)
            {
                return (!bForManway) ? 0.0625 : 0.125;
            }
            else if (sType == uppSpliceTypes.SpliceManwayCenter)
            { return 0.314 / 2; }
            else if (sType == uppSpliceTypes.SpliceWithJoggle)
            { return 0.125; }

            return 0;
        }
        public static colUOPParts CombineEqualParts(uopParts aParts, uopParts bParts, uppPartTypes aPartType, int aCountMultiplyer = 1, string aProjectHandle = null)
        {
            colUOPParts _rVal = new colUOPParts(bBaseOne: true, bMaintainIndices: true) { PartType = aPartType };
            if (aParts == null || bParts == null) return _rVal;

            List<uopPart> a_parts = aParts.FindAll(x => x.PartType == aPartType);
            bParts.SetMark(false);
            List<uopPart> b_parts = bParts.FindAll(x => x.PartType == aPartType);
            if (a_parts.Count <= 0 || b_parts.Count <= 0) return _rVal;
            List<int> usedids = new List<int>();
            for (int i = 0; i < a_parts.Count; i++)
            {
                uopPart apart = a_parts[i];

                List<uopPart> matches = b_parts.FindAll(x => x.IsEqual(apart));
                if (matches.Count > 0)
                {
                    uopPart newpart = apart.Clone();
                    newpart.MergeRangeAssociations(apart.RangeList);
                    newpart.MergeParentAssociations(apart.ParentList);
                    if (!string.IsNullOrWhiteSpace(aProjectHandle)) newpart.ProjectHandle = aProjectHandle;
                    foreach (uopPart item in matches)
                    {
                        newpart.Mark = true;
                        newpart.Quantity += item.Quantity * aCountMultiplyer;
                        newpart.MergeRangeAssociations(item.RangeList);
                        newpart.MergeParentAssociations(item.ParentList);
                    }
                    newpart.PartIndex = _rVal.Count + 1;
                    _rVal.Add(newpart);
                }


            }
            b_parts = bParts.FindAll(x => x.Mark == false);
            foreach (uopPart item in b_parts)
            {
                uopPart newpart = item.Clone();
                if (!string.IsNullOrWhiteSpace(aProjectHandle)) newpart.ProjectHandle = aProjectHandle;
                newpart.Quantity = item.Quantity * aCountMultiplyer;
                newpart.RangeGUID = item.RangeGUID;
                newpart.MergeRangeAssociations(item.RangeList);
                newpart.MergeParentAssociations(item.ParentList);
                newpart.PartIndex = _rVal.Count + 1;
                _rVal.Add(newpart);
            }

            return _rVal;
        }

        public static bool PartListContainsPartType(IEnumerable<uppPartTypes> aPartTypes, IEnumerable<uppPartTypes> bPartTypes)
        {
            if (aPartTypes == null || bPartTypes == null) return false;
            List<uppPartTypes> baselist = aPartTypes.ToList();
            foreach (var aPartType in bPartTypes) { if (baselist.Contains(aPartType)) return true; }
            return false;
        }
        public static int TrayCount(int RingStart, int RingEnd, uppStackPatterns StackPattern)
        {
            SpanLimits(RingStart, RingEnd, StackPattern, out int istr, out int iend);


            if (iend <= istr) return 1;
            double qty = iend - istr + 1;
            if (StackPattern != uppStackPatterns.Odd && StackPattern != uppStackPatterns.Even)
            {
                return (int)qty;
            }

            return (int)(qty + 1) / 2;

            //return StackPattern == uppStackPatterns.Odd || StackPattern == uppStackPatterns.Even ? (iend - istr + 1) / 2 : (int)qty;


        }
        public static int TrayCount(string aTrayRange)
        {

            //converts comma delimeted strings like "Trays 1-25 ODD, 2-200,4,Trays 21-43 EVEN" to a numeric tray count 
            int i1;
            int i2;

            string t1;
            string t2;
            string[] sStrings = new string[1];
            int sCnt;
            bool bOdd = false;
            bool bEven = false;
            int subTot = 0;
            int idash = 0;
            int lTot = 0;
            string pre = string.Empty;
            string suf = string.Empty;
            try
            {
                //null string - return 0
                if (string.IsNullOrWhiteSpace(aTrayRange)) return 0;
                string trng = aTrayRange.Trim().ToUpper();


                if (!trng.Contains(","))
                {
                    //no commas so one item in the array

                    sStrings[0] = trng;
                    sCnt = 1;

                }
                else
                {
                    //split up comma delimited lists into a string array
                    sStrings = trng.Split(',');
                    sCnt = sStrings.Length;
                }


                //    //loop on strings in the array
                for (int i = 0; i < sStrings.Length; i++)
                {
                    trng = sStrings[i].Trim();
                    subTot = 0;
                    if (trng.Length > 0)
                    {
                        //remove all spaces
                        trng = trng.Replace(" ", "");
                        //remove the odd and even and capture odd or even
                        if (trng.Contains("ODD"))
                        {
                            if (trng.EndsWith("ODD")) { bOdd = true; bEven = false; trng = trng.Substring(0, trng.Length - 3); }
                            if (trng.EndsWith("(ODD)")) { bOdd = true; bEven = false; trng = trng.Substring(0, trng.Length - 5); }
                            trng = trng.Replace("(ODD)", "");
                            trng = trng.Replace("ODD", "");
                            trng = trng.Trim();

                        }
                        if (trng.Contains("EVEN"))
                        {
                            if (trng.EndsWith("EVEN")) { bOdd = false; bEven = true; trng = trng.Substring(0, trng.Length - 4); }
                            if (trng.EndsWith("(EVEN)")) { bOdd = false; bEven = true; trng = trng.Substring(0, trng.Length - 6); }
                            trng = trng.Replace("(EVEN)", "");
                            trng = trng.Replace("EVEN", "");
                            trng = trng.Trim();

                        }
                        if (bOdd && bEven) { bOdd = false; bEven = false; }
                        //remove leading dashes
                        while (trng.StartsWith("-")) { trng = trng.Substring(1, trng.Length - 1); }
                        //remove trailing dashes
                        while (trng.EndsWith("-")) { trng = trng.Substring(0, trng.Length - 1); }
                        //look for the dash
                        idash = trng.IndexOf('-');
                        if (idash >= 0)
                        {
                            //split the left and right strings across the dash
                            //extract numbers
                            t1 = trng.Substring(0, idash);
                            t2 = trng.Substring(idash + 1, trng.Length - (idash + 1));
                            i1 = mzUtils.ExtractInteger(t1, out suf, out pre);
                            i2 = mzUtils.ExtractInteger(t2, out suf, out pre);
                            if (i1 < 0) i1 = 0;
                            if (i2 < 0) i2 = 0;

                            mzUtils.SortTwoValues(true, ref i1, ref i2);
                            if (i1 == 0 && i2 == 0)
                            {
                                subTot = 0;
                            }
                            else if (i1 != 0 && i2 == 0)
                            {
                                subTot = 1;
                            }
                            else if (i1 == 0 && i2 != 0)
                            {
                                subTot = 1;


                            }
                            else
                            {
                                //compute the number of trays in a range
                                if (bOdd || bEven)
                                {
                                    if (!IsOdd(i1, bEven)) i1 += 1;
                                    if (!IsOdd(i2, bEven)) i2 -= 1;
                                    if (i2 == i1)
                                    {
                                        subTot = 1;
                                    }
                                    else if (i2 < i1)
                                    {
                                        subTot = 0;
                                    }
                                    else
                                    {
                                        subTot = (i2 - i1) / 2 + 1;
                                    }

                                }
                                else
                                {
                                    subTot = i2 - i1 + 1;
                                }
                            }


                        }
                        else
                        {
                            //no dash so just count 1
                            subTot = 1;
                        }

                    }
                    lTot += subTot;
                }


            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);

            }


            return lTot;


        }


        /// <summary>
        ///#1the value to test
        ///returns True if the passed value is numeric and is an even number
        /// </summary>
        /// <param name="aValue"></param>
        /// <returns></returns>
        public static bool IsEven(int aValue) => aValue % 2 == 0;


        /// <summary>
        ///#1the value to test
        ///returns True if the passed value is numeric and is an odd number
        /// </summary>
        /// <param name="aValue"></param>
        /// <returns></returns>
        public static bool IsOdd(int aValue, bool bReturnInverse = false)
        {
            int iVal = Math.Abs(aValue);
            bool _rVal = iVal % 2 != 0;
            if (bReturnInverse) _rVal = !_rVal;
            return _rVal;


        }

        public static void AddHolesToPGON(dxePolygon aPGon, colDXFEntities aHoles, uppPartViews aView)
        {
            dxeHole aH;
            dxfDirection zDir;
            dxfEntity aBnd;
            dxfRectangle aRec;
            double ht;
            double wd;
            double dpth;
            dxeLine aL;
            if (aHoles == null || aPGon == null) return;
            if (aView == uppPartViews.Top)
            {
                for (int i = 1; i <= aHoles.Count; i++)
                {
                    aH = (dxeHole)aHoles.Item(i, true);
                    aH.Z = 0;
                    zDir = aH.ZDirection;
                    aBnd = aH.BoundingEntity();
                    if (Math.Round(zDir.Z, 3) == 1)
                    {
                        aPGon.AdditionalSegments.Add(aBnd);
                    }
                    else
                    {
                        dpth = Math.Round(aH.Depth, 3);
                        if (dpth > 0)
                        {
                            aRec = aH.ExtentPoints.BoundingRectangle();
                            ht = aRec.Height;
                            wd = aRec.Width;
                            if (Math.Round(ht, 3) == dpth)
                            {
                                aL = new dxeLine(aRec.TopLeft, aRec.BottomLeft) { Tag = aH.Tag, Flag = aH.Flag };
                                aPGon.AdditionalSegments.Add(aL.Clone());
                                aL.SetVectors(aRec.TopRight, aRec.BottomRight);
                                aPGon.AdditionalSegments.Add(aL.Clone());

                            }
                            else
                            {
                                if (Math.Round(wd, 3) == dpth)
                                {
                                    aL = new dxeLine(aRec.TopLeft, aRec.TopRight) { Tag = aH.Tag, Flag = aH.Flag };
                                    aPGon.AdditionalSegments.Add(aL.Clone());
                                    aL.SetVectors(aRec.BottomLeft, aRec.BottomRight);
                                    aPGon.AdditionalSegments.Add(aL.Clone());

                                }
                            }
                        }
                    }

                }


                return;
            }
        }

        public static double HoleArea(double aRadius, double aMinorRadius, double aLength, bool bIsSquare)
        {
            double rad = aRadius;
            double mr = aMinorRadius;
            if (mr >= rad - 0.0001) mr = 0;
            double dia = 2 * rad;
            double lng = aLength;
            if (lng != dia) mr = 0;
            if (bIsSquare) return lng * dia;

            double _rVal = Math.PI * Math.Pow(rad, 2);
            if (mr == 0)
            {
                if (lng > dia) _rVal += (lng - dia) * dia;
            }
            else
            {
                double x1 = Math.Sqrt(Math.Pow(rad, 2) - Math.Pow(mr, 2));
                double ang = ArcCosine(x1 / rad) * Math.PI;
                double are = Math.Pow(2 * rad, 2) / 8;
                _rVal -= are * (ang - Math.Sin(ang));
            }
            return _rVal;
        }

        public static double ComputeBeamLength(double aX, double aRadius, bool bRoundDown, double? aReducer = null, dxxRoundToLimits aRoundTo = dxxRoundToLimits.Sixteenth)
        {
            double _rVal = 0;
            try
            {
                if (aRoundTo == dxxRoundToLimits.Undefined) bRoundDown = false;
                aX = Math.Abs(aX);
                aRadius = Math.Abs(aRadius);
                if (aRadius > 0 && (aX < aRadius)) _rVal = Math.Sqrt(Math.Pow(aRadius, 2) - Math.Pow(aX, 2));
                _rVal = Math.Round(2 * _rVal, 4);

                if (bRoundDown) _rVal = uopUtils.RoundTo(_rVal, aRoundTo, bRoundUp: false, bRoundDown: true);
                if (aReducer.HasValue)
                    _rVal -= aReducer.Value;
                return _rVal;
            }
            catch (Exception) { return _rVal; }
        }

        /// <summary>
        /// '^used to round a numeric value to the indicated limit.
        /// limits are equated to enums for convenience and clarity and are transformed to numeric values.
        /// i.e. Eighth = 1 means round to the nearest 0.125
        /// if Millimeter or Centimeter is passed then the passed number is
        /// assumed to be in inches and is returned in inches rounded to the metric equivaliet 
        /// </summary>
        /// <param name="aNum">the number to round</param>
        /// <param name="aNearest">the limit to round to</param>
        /// <param name="bRoundUp">flag to indicate that the value should only be rounded up</param>
        /// <param name="bRoundDown">flag to indicate that the value should only be rounded down</param>
        /// <returns></returns>
        public static double RoundTo(double aNum, dxxRoundToLimits aNearest, bool bRoundUp = false, bool bRoundDown = false)
        {
            if (aNearest == dxxRoundToLimits.Undefined) return aNum;


            if (double.IsNaN(aNum))
            {
                aNum = 0;
                return aNum;
            }

            double _rVal = aNum;
            double f1 = aNum < 0 ? -1 : 1;

            try
            {



                double Factor = 0;
                double remain;
                int whole;
                int multi;
                double multafter = 0;



                switch (aNearest)
                {

                    case dxxRoundToLimits.Eighth:
                        Factor = 0.125;
                        break;
                    case dxxRoundToLimits.Half:
                        Factor = 0.5;
                        break;
                    case dxxRoundToLimits.Quarter:
                        Factor = 0.25;
                        break;
                    case dxxRoundToLimits.Sixteenth:
                        Factor = 0.0625;
                        break;
                    case dxxRoundToLimits.ThirtySeconds:
                        Factor = 0.03125;
                        break;
                    case dxxRoundToLimits.One:
                        Factor = 1;
                        break;
                    case dxxRoundToLimits.Millimeter:
                        multafter = 1 / 25.4;
                        aNum *= 25.4;
                        aNearest = dxxRoundToLimits.One;
                        Factor = 1;
                        break;
                    case dxxRoundToLimits.Centimeter:
                        multafter = 1 / 2.54;
                        aNum *= 2.54;
                        aNearest = dxxRoundToLimits.One;
                        Factor = 1;
                        break;
                }
                double num = Math.Round(Math.Abs(aNum), 6);

                if (bRoundUp && bRoundDown) bRoundDown = false;
                if (Factor == 0) Factor = 0.125;
                whole = Convert.ToInt32(Math.Truncate(num));
                remain = num - whole;
                if (!bRoundDown && !bRoundUp)
                {
                    _rVal = RoundIt(num, Factor, aStyle: dxxRoundingTypes.Natural);
                }
                else
                {

                    multi = Convert.ToInt32(Math.Truncate(remain / Factor));
                    if (bRoundUp)
                        if (remain - (multi * Factor) != 0) multi += 1;

                    _rVal = whole + multi * Factor;
                }
                if (multafter != 0) _rVal *= multafter;
            }
            catch
            {
                _rVal = aNum;
            }


            return _rVal * f1;
        }

        /// <summary>
        /// used to round a numeric value to the indicated limit.
        /// </summary>
        /// <param name="num">the number to round</param>
        /// <param name="Fraction">the limit to round to</param>
        /// <param name="bRoundDown">flag to force only rounding down</param>
        /// <returns></returns>
        public static double RoundIt(double num, double Fraction, bool bRoundDown = false)
        {
            Fraction = Math.Abs(Fraction);
            if (Fraction == 0) return num;
            int sign = Math.Sign(num);
            num = Math.Abs(num);
            double whole = Math.Truncate(num); // Convert.ToInt32(Math.Round(num, 0));
            double remain = num - whole;
            double multy = Math.Truncate(remain / Fraction);
            remain -= (multy * Fraction);
            if (remain != 0)
            {
                if (!bRoundDown)
                {
                    if (remain >= 0.5 * Fraction)
                        multy += 1.0;
                }
            }
            return sign * (whole + multy * Fraction);

        }

        /// <summary>
        /// used to round a numeric value to the indicated limit.
        /// </summary>
        /// <param name="num">the number to round</param>
        /// <param name="Fraction">the limit to round to</param>
        /// <param name="bRoundDown">flag to force only rounding down</param>
        /// <returns></returns>
        public static double RoundIt(double num, double Fraction, dxxRoundingTypes aStyle  = dxxRoundingTypes.Natural)
        {
            Fraction = Math.Abs(Fraction);
            if (Fraction == 0) return num;
            int sign = Math.Sign(num);
            num = Math.Abs(num);
            double whole = Math.Truncate(num); // Convert.ToInt32(Math.Round(num, 0));
            double remain = num - whole;
            double multy = Math.Truncate(remain / Fraction);
            remain -= (multy * Fraction);
            if (remain != 0)
            {
                if (aStyle == dxxRoundingTypes.Natural)
                {
                    if (remain >= 0.5 * Fraction)
                        multy += 1.0;
                }
                else if (aStyle == dxxRoundingTypes.Up)
                {
                    //multy += 1.0;
                    if(sign ==1)
                    {
                        multy += 1.0;
                    }
                }
                else
                {
                    if (sign == -1)
                    {
                        multy += 1.0;
                    }
                }
                 
            }
            return sign * (whole + multy * Fraction);

        }

        public static string DesignFamilyName(uppProjectFamilies aFamily, uppTrayConfigurations aConfiguration, uppMDDesigns aDesignFamily)
        {
            //the type of MD design that this tray configured to in String Format default = "MD"
            if (aFamily == uppProjectFamilies.uopFamXF)
            {
                return aConfiguration switch
                {
                    uppTrayConfigurations.CenterToSide => "C2S",
                    uppTrayConfigurations.SideToCenter => "S2C",
                    _ => "S2S"
                };
            }
            else if (aFamily == uppProjectFamilies.uopFamMD)
            {
                return uopEnums.Description(aDesignFamily);

            }

            return "MD";
        }
        /// <summary>
        ///returns a string that can be used as a valid filename based on the project's name property
        /// </summary>
        /// <returns></returns>
        public static string FriendlyFileName(string aFileName)
        {
            string _rVal = string.Empty;
            if (string.IsNullOrWhiteSpace(aFileName)) return "";
            string fname = aFileName.Trim();
            for (int i = 0; i < fname.Length; i++)
            {
                bool goodchar = false;
                char C = fname[i];
                int asci = (int)C;

                if (asci >= 32 && asci <= 126) goodchar = true;// 'printable chars

                if (asci == 92) goodchar = false;// \
                if (asci == 47) goodchar = false;// /
                if (asci == 58) goodchar = false;// :
                if (asci == 42) goodchar = false;// *
                if (asci == 63) goodchar = false;// ?
                if (asci == 34) goodchar = false;// "
                if (asci == 60) goodchar = false;// <
                if (asci == 62) goodchar = false;// >
                if (asci == 124) goodchar = false;// |
                if (goodchar) _rVal += C;
            }
            if (_rVal.Length > 50) _rVal = _rVal.Substring(0, 50);
            return _rVal;


        }


        ///// <summary>
        ///// #1the liquid density
        ///// #2the vapor density
        ///// #3the liquid flow rate
        ///// #4then vapor flow rate
        ///// Function returns the average density, including two phase flows
        ///// </summary>
        ///// <param name="liquidDensity"></param>
        ///// <param name="vaporDensity"></param>
        ///// <param name="liquidRate"></param>
        ///// <param name="vaporRate"></param>
        ///// <returns></returns>
        //internal static dynamic AverageDensity(double liquidDensity, double vaporDensity, double liquidRate, double vaporRate)
        //{
        //    return AverageDensity(liquidDensity, vaporDensity, liquidRate, vaporRate);
        //}
        /// <summary>
        ///#1the liquid density
        ///#2the vapor density
        ///#3the liquid flow rate
        ///#4then vapor flow rate
        ///^Function returns the average density, including two phase flows
        /// </summary>
        /// <param name="aDenL"></param>
        /// <param name="aDenV"></param>
        /// <param name="aFlowL"></param>
        /// <param name="aFlowV"></param>
        /// <returns></returns>
        public static dynamic AverageDensity(double aDenL, double aDenV, double aFlowL, double aFlowV)
        {

            try
            {
                dynamic FlowT = null;


                if (aFlowV < 0 || aFlowL < 0 || aDenL < 0 || aDenV < 0) return "ERR";


                FlowT = aFlowV + aFlowL;
                if (FlowT == 0) return null;


                if (aFlowL > 0 & aDenL == 0) return "ERR";

                if (aFlowV > 0 & aDenV == 0) return "ERR";

                //AverageDensity = ((aFlowL / FlowT) * aDenL) + ((aFlowV / FlowT) * aDenV)

                double _rVal = (aFlowL / aDenL) + (aFlowV / aDenV);
                if (_rVal != 0)
                {
                    _rVal = (aFlowL + aFlowV) / _rVal;
                }
                else
                {
                    _rVal = 0;
                }
                return _rVal;
            }
            catch (Exception)
            {

                return "ERR";
            }
        }

        public static double MaxValue(double aVal, double bVal, int? aPrecis = null) { if (!aPrecis.HasValue) return Math.Max(aVal, bVal); int p = mzUtils.LimitedValue(aPrecis.Value, 0, 15); return Math.Max(Math.Round(aVal, p), Math.Round(bVal, p)); }

        public static double MinValue(double aVal, double bVal, int? aPrecis = null) { if (!aPrecis.HasValue) return Math.Min(aVal, bVal); int p = mzUtils.LimitedValue(aPrecis.Value, 0, 15); return Math.Min(Math.Round(aVal, p), Math.Round(bVal, p)); }

        /// <summary>
        ///double SpannedAngle = 0;
        ///^the angle spanned by the entity
        ///~dynamically calculated based on current entity properties
        ///return the angle covered by the entity
        ///useful in determining midpoint and entity length
        /// </summary>
        /// <param name="bClockwise"></param>
        /// <param name="aStartAngle"></param>
        /// <param name="aEndAngle"></param>
        /// <returns></returns>
        public static double SpannedAngle(bool bClockwise, double aStartAngle, double aEndAngle)
        {
            double _rVal = 0;
            try
            {
                double sa = aStartAngle;
                double ea = aEndAngle;
                while (sa < 0)
                {
                    sa += 360;
                }
                while (ea < 0)
                {
                    ea += 360;
                }
                if (!bClockwise)
                {
                    _rVal = Math.Round(ea - sa, 4);
                }
                else
                {
                    _rVal = Math.Round(sa - ea, 4);
                }
                while (_rVal < 0)
                {
                    _rVal += 360;
                }
                if (_rVal == 0 & (aEndAngle != aStartAngle))
                {
                    _rVal = 360;
                }
            }
            catch (Exception)
            {
                //Log exception
            }
            return _rVal;
        }

        public static int CalcSpares(int aCount, double aSparePercentage, int aMinAdd = 2)
        {
            int _rVal = aCount;
            if (_rVal <= 0 || aSparePercentage <= 0) return _rVal;
            if (aSparePercentage > 0.50) aSparePercentage /= 100;

            aMinAdd = Math.Abs(aMinAdd);
            int addcnt = (int)Math.Round(aSparePercentage * _rVal, 0);
            if (Math.Round((double)(addcnt / _rVal), 4) < aSparePercentage) addcnt += 1;
            if (addcnt < aMinAdd) addcnt = aMinAdd;
            _rVal += addcnt;
            return _rVal;
        }

        internal static string TrayName(uppProjectFamilies aFamily, int aRingStart, int aRingEnd, uppTrayConfigurations aConfiguration, uppMDDesigns aDesignFamily, uppStackPatterns aStackPattern, bool bIncludeDesignIndicator)
        {
            string _rVal = string.Empty;

            if (aFamily == uppProjectFamilies.uopFamXF)
            {
                if (aConfiguration == uppTrayConfigurations.SideToSide)
                {
                    bIncludeDesignIndicator = false;
                }
            }

            uopRingRange rngrange = new uopRingRange(aRingStart, aRingEnd, aStackPattern);
            string plural = rngrange.RingCount > 1 ? "s" : "";
            if (bIncludeDesignIndicator)
                _rVal = DesignFamilyName(aFamily, aConfiguration, aDesignFamily);

            if (_rVal !=  string.Empty)
                _rVal += $" Tray{plural} ";
            else
                _rVal = $"Tray{plural} ";

            _rVal += rngrange.SpanName; // SpanName(aRingStart, aRingEnd, aStackPattern, true);

            return _rVal;
        }



        internal static TPROPERTIES ReadINI_Strings(string aFileSpec, string aFileSection, string keyName)
        {
            TPROPERTIES _rVal = new TPROPERTIES();
            bool isFound = false;
            string currentResult = string.Empty;

            for (int i = 1; i <= 10000; i++)
            {
                isFound = false;
                currentResult = ReadValue<string>(aFileSection, keyName + i, "", aFileSpec, out isFound);
                if (!isFound) break;
                _rVal.AddProp(keyName + i, currentResult);
            }
            return _rVal;
        }

        /// <summary>
        /// Compares two strings nd returns result
        /// </summary>
        /// <param name="aString1"></param>
        /// <param name="aString2"></param>
        /// <param name="bString1"></param>
        /// <param name="bString2"></param>
        /// <param name="rInverted"></param>
        /// <returns></returns>
        public static bool CompareStrings(string aString1, string aString2, string bString1, string bString2, out bool rInverted)
        {
            bool _rVal = false;
            rInverted = false;
            if (string.Compare(aString1, bString1) == 0)
            {
                if (string.Compare(aString2, bString2) == 0)
                {
                    _rVal = true;
                }

            }
            else if (string.Compare(aString1, bString2) == 0)
            {
                if (string.Compare(aString2, bString1) == 0)
                {
                    _rVal = true;
                    rInverted = true;
                }
            }
            return _rVal;
        }

        public static string SpanName(int ringStart, int ringEnd, uppStackPatterns stackPattern, bool bIncludeOddEven = true, string aPrefix = null)
        {


            SpanLimits(ringStart, ringEnd, stackPattern, out int si, out int ei);

            string _rVal = si.ToString();
            if (ei > si)
            {

                if (stackPattern != uppStackPatterns.Odd && stackPattern != uppStackPatterns.Even) bIncludeOddEven = false;


                _rVal = $"{si}-{ei}";
                if (bIncludeOddEven)
                {
                    _rVal += stackPattern == uppStackPatterns.Odd ? " Odd" : " Even";

                }
            }

            if (!string.IsNullOrWhiteSpace(aPrefix)) _rVal = $"{aPrefix}{_rVal}";
            return _rVal;
        }
        internal static bool SpanContainsIndex(int testRing, int ringStart, int ringEnd, uppStackPatterns stackPattern, bool bIncludeOddEven = true)
        {

            SpanLimits(ringStart, ringEnd, stackPattern, out int si, out int ei);

            if (testRing < si || testRing > ei) return false;
            if (!bIncludeOddEven) return true;
            if (stackPattern == uppStackPatterns.Odd) return uopUtils.IsOdd(testRing);
            if (stackPattern == uppStackPatterns.Even) return !uopUtils.IsOdd(testRing);
            return true;

        }

        public static string SubSpan(string aRange, int aRemoveIndex)
        {
            SpanLimits(aRange, out int low, out int high, out uppStackPatterns stack);
            string _rVal = aRange;
            if (aRemoveIndex < low || aRemoveIndex > high)
            {
                _rVal = (low == high) ? low.ToString() : $"{low}-{high}";
                if (stack == uppStackPatterns.Odd || stack == uppStackPatterns.Even) _rVal += stack == uppStackPatterns.Odd ? " Odd" : " Even";
                return _rVal;
            }
            else if (aRemoveIndex == low || aRemoveIndex == high)
            {
                if (aRemoveIndex == low)
                {
                    low++;
                    if (stack == uppStackPatterns.Odd || stack == uppStackPatterns.Even)
                    {
                        if (stack == uppStackPatterns.Odd)
                            if (!mzUtils.IsOdd(low)) low++;
                            else
                            if (mzUtils.IsOdd(low)) low++;
                    }
                }

                else
                {
                    high--;
                    if (stack == uppStackPatterns.Odd || stack == uppStackPatterns.Even)
                    {
                        if (stack == uppStackPatterns.Odd)
                            if (!mzUtils.IsOdd(high)) high--;
                            else
                            if (mzUtils.IsOdd(high)) high--;
                    }
                }
                if (high < low) high = low;
                _rVal = (low == high) ? low.ToString() : $"{low}-{high}";
                if (stack == uppStackPatterns.Odd || stack == uppStackPatterns.Even) _rVal += $" {stack.GetDescription()}";
                return _rVal;
            }
            //the remove index is within the span so the return is two spans
            _rVal = $"{SpanName(low, aRemoveIndex - 1, stack)}-{SpanName(aRemoveIndex + 1, high, stack)} ";

            return _rVal;


        }


        public static void SpanLimits(string aRange, out int rLowIndex, out int rHighIndex, out uppStackPatterns rStackPattern)
        {
            rLowIndex = 0;
            rHighIndex = 0;
            rStackPattern = uppStackPatterns.Continuous;

            if (string.IsNullOrWhiteSpace(aRange)) return;
            string span = aRange.Trim().ToUpper();
            string suffix;

            int sidx = mzUtils.VarToInteger(span);

            if (aRange.Contains("-"))
            {
                string[] svals = span.Split('-');
                rLowIndex = mzUtils.VarToInteger(mzUtils.ExtractInteger(svals[0]));
                rHighIndex = mzUtils.VarToInteger(mzUtils.ExtractInteger(svals[1], false, out suffix));
                mzUtils.SortTwoValues(true, ref rLowIndex, ref rHighIndex);
            }
            else
            {
                rLowIndex = mzUtils.VarToInteger(mzUtils.ExtractInteger(span, false, out suffix));
                rHighIndex = rLowIndex;
            }
            if (string.IsNullOrWhiteSpace(suffix)) return;
            if (suffix.ToUpper().Contains("ODD"))
            {
                rStackPattern = uppStackPatterns.Odd;
                if (!mzUtils.IsOdd(rLowIndex))
                    rLowIndex++;
                if (!mzUtils.IsOdd(rHighIndex))
                    rHighIndex--;
            }
            else if (suffix.ToUpper().Contains("EVEN"))
            {
                rStackPattern = uppStackPatterns.Even;
                if (mzUtils.IsOdd(rLowIndex))
                    rLowIndex++;
                if (!mzUtils.IsOdd(rHighIndex))
                    rHighIndex--;
            }
            if (rHighIndex < rLowIndex) rHighIndex = rLowIndex;
        }

        public static void SpanLimits(int ringStart, int RingEnd, uppStackPatterns aStackPattern, out int rLowIndex, out int rHighIndex)
        {
            rLowIndex = ringStart;
            rHighIndex = RingEnd;
            uopRingRange.RectifySpan(ref rLowIndex, ref rHighIndex, aStackPattern);


        }

        private static bool _SuppressIDE = false;

        public static bool SuppressIDE { get => _SuppressIDE; set => _SuppressIDE = value; }

        public static bool RunningInIDE => !SuppressIDE && System.Diagnostics.Debugger.IsAttached;


        public static string ReadINI_String(string aFileSpec, string aFileSection, string aKeyName, string aDefault, out bool found)
        {
            string _rVal = ReadValue<string>(aFileSection, aKeyName, aDefault, aFileSpec, out found);
            if (!found || string.IsNullOrEmpty(_rVal)) _rVal = string.Empty;
            return _rVal;

        }

        /// <summary>
        ///^reads a uopsheet metal from a file in INI file format
        ///~included to account for changes in the way materials are saved
        /// </summary>
        /// <param name="aFileSpec"></param>
        /// <param name="fSec"></param>
        /// <param name="v"></param>
        /// <param name="CarbonSteel"></param>
        /// <param name="uopSheetGage10"></param>
        /// <param name="found"></param>
        /// <param name="mAdd"></param>
        /// <param name="bv"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        public static uopMaterial ReadMaterialFromFile(string aFileName, string aFileSection, string aKey = "Material", uppMetalFamilies aDefaultFamily = uppMetalFamilies.CarbonSteel, uppSheetGages aDefaultGage = uppSheetGages.Gage12, bool addIfNotFound = true, string fileString = "")
        {
            if (!File.Exists(aFileName)) return null;

            string matstring = string.Empty;
            string fname = string.Empty;
            string gName = string.Empty;

            bool found = false;
            uopMaterial aMaterial = null;
            uopSheetMetal bMaterial = null;
            List<string> sVals = new List<string> { };
            aKey = aKey.Trim();

            if (aKey !=  string.Empty)
            {
                matstring = ReadINI_String(aFileName, aFileSection, aKey, "", out found);
                found = found && matstring != string.Empty;
            }

            if (found)
            {
                sVals = mzUtils.ListValues(matstring, uopGlobals.Delim, false, true);
                if (sVals.Count >= 2)
                {
                    fname = sVals[0];
                    gName = sVals[1];
                }
                found = fname !=  string.Empty && gName != string.Empty;
            }

            if (found)
            {
                fileString = matstring;
                //see if me can find then material by Descriptor
                aMaterial = (uopSheetMetal)uopGlobals.goSheetMetalOptions().GetByDescriptor(uppMaterialTypes.SheetMetal, matstring);
                if (aMaterial == null)
                {
                    //see if me can find then material by name and gage
                    aMaterial = (uopSheetMetal)uopGlobals.goSheetMetalOptions().GetSheetMetalByStringVals(fname, gName);

                    if (aMaterial != null)
                    {
                        bMaterial = new uopSheetMetal(TMATERIAL.SheetMetalByDescriptor(matstring, out bool good));
                        if (good)
                        {
                            if (addIfNotFound)
                            {
                                uopGlobals.goSheetMetalOptions().Add(bMaterial);
                                aMaterial = bMaterial;

                            }
                        }
                    }
                }

            }

            //return the default if the material was not found
            if (aMaterial == null)
            {
                aMaterial = (uopSheetMetal)uopGlobals.goSheetMetalOptions().GetByFamilyAndGauge(aDefaultFamily, aDefaultGage);
            }
            if (aMaterial == null)
            {
                aMaterial = (uopSheetMetal)uopGlobals.goSheetMetalOptions().GetByFamilyAndGauge(uppMetalFamilies.CarbonSteel, uppSheetGages.Gage12);
            }
            if (aMaterial == null)
            {
                aMaterial = (uopSheetMetal)uopGlobals.goSheetMetalOptions().Item(1);
            }
            return aMaterial;
        }


        /// <summary>
        ///^reads a uopsheet metal from a file in INI file format
        ///~included to account for changes in the way materials are saved
        /// </summary>
        /// <param name="aFileSpec"></param>
        /// <param name="fSec"></param>
        /// <param name="v"></param>
        /// <param name="CarbonSteel"></param>
        /// <param name="uopSheetGage10"></param>
        /// <param name="found"></param>
        /// <param name="mAdd"></param>
        /// <param name="bv"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        public static uopMaterial ReadMaterialFromFile(uopPropertyArray aFileProps, string aFileSection, string aKey, out bool rFound, out bool rMaterialAdded, uppMetalFamilies aDefaultFamily = uppMetalFamilies.CarbonSteel, uppSheetGages aDefaultGage = uppSheetGages.Gage12, bool addIfNotFound = true)
        {
            rFound = false;
            rMaterialAdded = false;

            if (aFileProps == null) return null;
            uopProperties filesec = aFileProps.Item(aFileSection);
            if (filesec == null || filesec.Count <= 0) return null;

            string fname = string.Empty;
            string gName = string.Empty;
            rMaterialAdded = false;
            rFound = false;
            bool found = false;
            uopMaterial aMaterial = null;
            uopSheetMetal bMaterial = null;
            List<string> sVals = new List<string> { };
            aKey = aKey.Trim();


            string fileString = filesec.ValueS(aKey).Trim();
            found = !string.IsNullOrWhiteSpace(fileString);
            string matstring = mzUtils.FixGlobalDelimError(fileString);
            //matstring = found ? fileString.Trim() : "";

            if (found)
            {
                sVals = mzUtils.ListValues(matstring, uopGlobals.Delim, false, true);
                if (sVals.Count >= 2)
                {
                    fname = sVals[0];
                    gName = sVals[1];
                }
                found = fname !=  string.Empty && gName != string.Empty;
            }

            if (found)
            {
                fileString = matstring;
                //see if me can find then material by Descriptor
                aMaterial = (uopSheetMetal)uopGlobals.goSheetMetalOptions().GetByDescriptor(uppMaterialTypes.SheetMetal, matstring);
                if (aMaterial == null)
                {
                    //see if me can find then material by name and gage
                    aMaterial = (uopSheetMetal)uopGlobals.goSheetMetalOptions().GetSheetMetalByStringVals(fname, gName);
                    rFound = !(aMaterial == null);
                    if (!rFound)
                    {
                        bMaterial = new uopSheetMetal(TMATERIAL.SheetMetalByDescriptor(matstring, out bool good));
                        if (good)
                        {
                            if (addIfNotFound)
                            {
                                uopGlobals.goSheetMetalOptions().Add(bMaterial);
                                aMaterial = bMaterial;
                                rMaterialAdded = true;
                            }
                        }
                    }
                }
                else
                {

                    rFound = true;
                }
            }

            //return the default if the material was not found
            aMaterial ??= (uopSheetMetal)uopGlobals.goSheetMetalOptions().GetByFamilyAndGauge(aDefaultFamily, aDefaultGage);
            aMaterial ??= (uopSheetMetal)uopGlobals.goSheetMetalOptions().GetByFamilyAndGauge(uppMetalFamilies.CarbonSteel, uppSheetGages.Gage12);

            aMaterial ??= (uopSheetMetal)uopGlobals.goSheetMetalOptions().Item(1);

            return aMaterial;

        }
        public static double ArcArea(double aRadius, double aStartAngle, double aEndAngle)
        {
            double _rVal = 0;
            //^the area of the arc
            double ang = 0;
            double span = SpannedAngle(false, aStartAngle, aEndAngle);
            double tot = Math.PI * Math.Pow(aRadius, 2);



            if (span == 0) return _rVal;

            if (span == 180)
            {
                _rVal = tot / 2;
            }
            else if (span == 360)
            {
                _rVal = tot;
            }
            else
            {
                ang = span * Math.PI / 180;
                _rVal = Math.Pow(2 * aRadius, 2) / 8;
                _rVal *= ang - Math.Sin(ang);
                if (span > 180)
                {
                    _rVal = tot - _rVal;
                }
            }
            return _rVal;
        }
        /// <summary>
        /// Fits through circle
        /// </summary>
        /// <param name="aDiameter"></param>
        /// <param name="aHeight"></param>
        /// <param name="aWidth"></param>
        /// <param name="aDepth"></param>
        /// <param name="rClearance"></param>
        /// <returns></returns>
        public static bool FitsThruCircle(double aDiameter, double aHeight, double aWidth, double aDepth, out double rClearance, double aClearance = 0)
        {

            rClearance = 0;
            aClearance = Math.Abs(aClearance);
            double rad = 0.5 * (aDiameter + aClearance);

            if (rad <= 0)  return false; 
            double x1 = aHeight > 0 ? aHeight /2 : 0;
            double x2 = aWidth > 0 ? aWidth /2 :0;
            double y1 = aDepth > 0? aDepth/2 : 0;

            mzUtils.SortTwoValues(true, ref x1, ref x2);
            //increase the min width to account for the depth
            if (y1 > 0)
            {
                x1 = Math.Sqrt(Math.Pow(x1, 2) + Math.Pow(y1, 2));
            }
            if (x1 > 0)
                rClearance = (rad - x1) * 2;
            return rClearance > aClearance;
        }

        /// <summary>
        /// Fits through circle
        /// </summary>
        /// <param name="aDiameter"></param>
        /// <param name="aHeight"></param>
        /// <param name="aWidth"></param>
        /// <param name="aDepth"></param>

        /// <returns></returns>
        public static bool FitsThruCircle(double aDiameter, double aHeight, double aWidth, double aDepth, double aClearance = 0) => FitsThruCircle(aDiameter, aHeight, aWidth, aDepth, out double _,  aClearance);

        internal static string SpecTypeName(uppSpecTypes type)
        {


            return type switch
            {
                uppSpecTypes.SheetMetal => "Sheet",
                uppSpecTypes.Plate => "Plate",
                uppSpecTypes.Pipe => "Pipe",
                uppSpecTypes.Flange => "Flange",
                uppSpecTypes.Fitting => "Fitting",
                uppSpecTypes.Gasket => "Gasket",
                uppSpecTypes.Tube => "Tube",
                uppSpecTypes.Bolt => "Bolt",
                uppSpecTypes.Nut => "Nut",
                uppSpecTypes.FlatWasher => "Flat Washer",
                uppSpecTypes.LockWasher => "Lock Washer",
                uppSpecTypes.ThreadedStud => "Stud",
                uppSpecTypes.SetScrew => "Set Screw",
                _ => ""
            };

        }

        internal static List<string> ListValues(string aList, string aDelimitor = ",", bool bReturnNulls = false, bool bTrim = false, bool noDupes = false, bool bNumbersOnly = false, int iPrecis = -1, bool bRemoveParens = false)
        {

            List<string> _rVal = new List<string>();
            if (string.IsNullOrWhiteSpace(aList)) return _rVal;
            if (string.IsNullOrWhiteSpace(aDelimitor)) { _rVal.Add(aList); return _rVal; }

            string aStr = aList.Trim();
            char[] delim = aDelimitor.ToCharArray();

            bool keep;
            string[] sVals = aStr.Split(Convert.ToChar(aDelimitor));
            string sVal = string.Empty;
            for (int i = 0; i < sVals.Length; i++)
            {
                sVal = sVals[i];
                keep = true;
                if (bTrim) sVal = sVal.Trim();
                if (!bReturnNulls && string.IsNullOrWhiteSpace(sVal)) keep = false;
                if (keep && bNumbersOnly && !string.IsNullOrWhiteSpace(sVal))
                {
                    if (!mzUtils.IsNumeric(sVal))
                    { keep = false; }
                    else { sVal = mzUtils.VarToDouble(sVal, aPrecis: iPrecis).ToString(); }
                }

                if (keep && !bReturnNulls)
                {
                    for (int j = 0; j < _rVal.Count; j++)
                    {
                        if (string.Compare(_rVal[j], sVal, ignoreCase: true) == 0) { keep = false; break; }
                    }
                }
                if (keep && noDupes)
                {
                    keep = _rVal.FindIndex(x => string.Compare(x, sVal, true) == 0) < 0;
                }
                if (keep) _rVal.Add(sVal);
            }

            return _rVal;
        }

        internal static uppSpecTypes SpecType(string tname)
        {
            uppSpecTypes uop_SpecType;
            switch (tname.ToUpper())
            {
                case "SHEET":
                    uop_SpecType = uppSpecTypes.SheetMetal;
                    break;

                case "PLATE":
                    uop_SpecType = uppSpecTypes.Plate;
                    break;

                case "PIPE":
                    uop_SpecType = uppSpecTypes.Pipe;
                    break;

                case "FLANGE":
                    uop_SpecType = uppSpecTypes.Flange;
                    break;

                case "FITTING":
                    uop_SpecType = uppSpecTypes.Fitting;
                    break;

                case "BOLT":
                    uop_SpecType = uppSpecTypes.Bolt;
                    break;

                case "NUT":
                    uop_SpecType = uppSpecTypes.Nut;
                    break;

                case "LOCK WASHER":
                case "LOCKWASHER":
                    uop_SpecType = uppSpecTypes.LockWasher;
                    break;

                case "FLAT WASHER":
                case "FLATWASHER":
                    uop_SpecType = uppSpecTypes.FlatWasher;
                    break;

                case "STUD":
                case "THREADED STUD":
                    uop_SpecType = uppSpecTypes.ThreadedStud;
                    break;

                case "GASKET":
                    uop_SpecType = uppSpecTypes.Gasket;
                    break;
                case "SET SCREW":
                case "SETSCREW":
                    uop_SpecType = uppSpecTypes.SetScrew;
                    break;
                default:
                    uop_SpecType = uppSpecTypes.Undefined;
                    break;
            }

            return uop_SpecType;
        }

        internal static string ReadINI_String(string aFileSpec, string aSection, string aKey)
        {
            return ReadValue<string>(aSection, aKey, "", aFileSpec);
        }



        /// <summary>
        /// #1the INI file to write to
        /// #2the section in the iINI file to add a value to
        /// #3the key to add to the file
        /// #4the value to add to the file
        /// ^used to write a value into an INI formatted file
        /// ~uses the "WritePrivateProfileString" windows API call.
        /// ~returns False if the API call returns an error code.
        /// </summary>
        /// <param name="aFileSpec"></param>
        /// <param name="sSection"></param>
        /// <param name="sKey"></param>
        /// <param name="sValue"></param>
        /// <returns></returns>
        public static bool WriteINIString(string aFileSpec, string sSection, string sKey, string sValue)
        {
            long LR;
            sValue = sValue.Trim();
            StringBuilder sVal = new StringBuilder(sValue.Length * 2);
            sVal.Append(sValue);
            LR = PInvoker.WritePrivateProfileString(sSection, sKey, sVal, aFileSpec);
            return LR != 0;
        }


        /// <summary>
        /// Parses deliminated string and prepare the list of strings after splitting it with given delimitor.
        /// </summary>
        /// <param name="aDataString"></param>
        /// <param name="aDelimitor"></param>
        /// <param name="bRemoveQuotes"></param>
        /// <returns></returns>
        public static List<string> ParseDeliminatedString(string aDataString, char aDelimitor = ',', bool bRemoveQuotes = true)
        {
            bool isquote, isdelim;
            string dstr;
            string[] vals;
            char cR;
            int inquote;
            List<string> results = new List<string>();

            if (aDelimitor == ' ')
                aDelimitor = ',';

            inquote = -1;
            dstr = string.Empty;
            char[] aDataInChars = aDataString.ToCharArray();
            for (int i = 0; i < aDataInChars.Length; i++)
            {
                cR = aDataInChars[i];
                isquote = ((char)cR) == 34;
                isdelim = aDelimitor == cR;

                if (isquote)
                {
                    inquote = -inquote;
                }
                else
                {
                    if (inquote == 1 && isdelim)
                    {
                        cR = (char)231;
                    }

                    dstr += cR;
                }
            }

            vals = dstr.Split(aDelimitor);
            string cr1;

            for (int i = 0; i < vals.Length; i++)
            {
                cr1 = vals[i].Trim();

                if (bRemoveQuotes)
                {
                    if (cr1.Length > 0 && cr1[0] == (char)34)
                    {
                        cr1 = cr1.Substring(1, cr1.Length - 1);
                    }

                    if (cr1.Length > 0 && cr1[cr1.Length - 1] == (char)34)
                    {
                        cr1 = cr1.Substring(0, cr1.Length - 1);
                    }
                }

                cr1 = cr1.Replace((char)231, aDelimitor);
                results.Add(cr1.Trim());

            }

            return results;
        }

        internal static double ReadINI_Number(string aFileSpec, string fSec, string aKey, double aDefault = 0, bool bReturnZeroWhenNegative = false)
        {
            double _rVal = ReadValue<double>(fSec, aKey, aDefault.ToString(), aFileSpec);
            if (bReturnZeroWhenNegative && _rVal < 0) _rVal = 0;
            return _rVal;
        }


        internal static double ReadINI_Number(string aFileSpec, string fSec, string aKey, double aDefault, out bool found, bool bReturnZeroWhenNegative = false)
        {
            double _rVal = ReadValue<double>(fSec, aKey, aDefault.ToString(), aFileSpec, out found);
            if (bReturnZeroWhenNegative && _rVal < 0) _rVal = 0;
            return _rVal;
        }


        internal static int ReadINI_Integer(string aFileSpec, string fSec, string aKey, int aDefault = 0, bool bReturnZeroWhenNegative = false)
        {
            int _rVal = ReadValue<int>(fSec, aKey, aDefault.ToString(), aFileSpec);
            if (bReturnZeroWhenNegative && _rVal < 0) _rVal = 0;
            return _rVal;
        }


        internal static int ReadINI_Integer(string aFileSpec, string aFileSection, string aKeyName, int aDefault, out bool found, bool bReturnZeroWhenNegative = false)
        {
            int _rVal = ReadValue<int>(aFileSection, aKeyName, aDefault.ToString(), aFileSpec, out found);
            if (bReturnZeroWhenNegative && _rVal < 0) _rVal = 0;
            return _rVal;
        }


        internal static bool ReadINI_Boolean(string aFileSpec, string fSec, string aKey, bool aDefault = false)
        {

            return mzUtils.VarToBoolean(ReadValue<string>(fSec, aKey, aDefault.ToString(), aFileSpec));
        }



        internal static string ReadINI_String(string aFileSpec, string fSec, string aKey, string aDefault = "")
        {
            string _rVal = string.IsNullOrEmpty(aDefault) ? string.Empty : aDefault;
            _rVal = ReadValue<string>(fSec, aKey, _rVal, aFileSpec);
            return string.IsNullOrEmpty(_rVal) ? string.Empty : _rVal;
        }

        /// <summary>
        /// spaces points along the passed line using the indicated MAX spacing with the first being on the start pt and the last being on the end point of the line
        /// </summary>
        /// <remarks>the best fit space is computed based ont he passed max space. at least points 2 points are returned unless the length of the line is less that the line length less 2x the buffet.</remarks>
        /// <param name="aLine">the subject line</param>
        /// <param name="aHole">the subject hole</param>
        /// <param name="aTargetSpace">the max target space to apply</param>
        /// <param name="aEndBuffer">a end buffer to apply. the effectively shortens the line by 2x the passed value </param>
        /// <param name="aTag">a tag to apply to the returned holes</param>
        /// <param name="aFlag">a flag to apply to the points</param>
        /// <param name="bSaveToLine">a tag to save the points to the line</param>
        /// <returns></returns>
        internal static UHOLES LayoutHolesOnLine2(ULINE aLine, UHOLE aHole, double aTargetSpace, double aEndBuffer, string aTag = null, string aFlag = null, bool bSaveToLine = false)
        {
            
            UHOLES _rVal = UHOLES.Null;
            UHOLE basehole = new UHOLE(aHole);
            if (aTag != null) basehole.Tag = aTag;
            if (aFlag != null) basehole.Flag = aFlag;
            _rVal.Member = basehole;
            
            aTargetSpace = Math.Abs(aTargetSpace);
            if (aTargetSpace <= (2 * basehole.Radius) + 0.0625) aTargetSpace = (2 * basehole.Radius) + 0.0625;

            _rVal.Centers = LayoutPointsOnLine2(aLine, aTargetSpace, out double ang, aEndBuffer: aEndBuffer, aTag:basehole.Tag, aFlag: basehole.Flag,aRadius:basehole.Radius, bSaveToLine);
            if (_rVal.Centers.Count > 0) _rVal.Member.Center = _rVal.Centers.Item(1);
            _rVal.Rotation = ang;
            return _rVal;

        }


        /// <summary>
        /// spaces points along the passed line using the indicated MAX spacing with the first being on the start pt and the last being on the end point of the line
        /// </summary>
        /// <remarks>the best fit space is computed based ont he passed max space. at least points 2 points are returned unless the length of the line is less that the line length less 2x the buffet.</remarks>
        /// <param name="aLine">the subject line</param>
        /// <param name="aTargetSpace">the max target space to apply</param>
        /// <param name="rAngle">returns the angle of the line</param>
        /// <param name="aEndBuffer">a end buffer to apply. the effectively shortens the line by 2x the passed value </param>
        /// <param name="aTag">an pptional tag to apply to the returned points</param>
        /// <param name="aFlag">a an optional flag to apply to the returned points</param>
        /// <param name="aRadius">an optional radius to apply to the returned points</param>
        /// <param name="bSaveToLine">a tag to save the points to the line</param>
        /// <returns></returns>

        internal static UVECTORS LayoutPointsOnLine2(ULINE aLine, double aTargetSpace, out double rAngle, double aEndBuffer = 0, string aTag = null, string aFlag = null,  double?aRadius = null, bool bSaveToLine  = false)
        {

            rAngle = 0;
            UVECTORS _rVal = UVECTORS.Zero;
            aTargetSpace = Math.Abs(aTargetSpace);

            UVECTOR aDir = aLine.Direction(out bool isNull, out double llen);
            if (isNull) return _rVal;
            aEndBuffer = Math.Abs(aEndBuffer);
            if (aEndBuffer >= llen / 2) return _rVal;

            rAngle = aDir.XAngle;

            UVECTOR sp = new UVECTOR(aLine.sp);
            if (aTag != null) sp.Tag = aTag;
            if (aFlag != null) sp.Flag = aFlag;
            if (aRadius.HasValue) sp.Radius = aRadius.Value;
            UVECTOR ep = new UVECTOR(aLine.ep);
            if (aEndBuffer > 0)
            {
                sp += aDir * aEndBuffer;
                ep += aDir * -aEndBuffer;

                llen = sp.DistanceTo(ep);
            }

            int spaces = (int)Math.Floor(llen / aTargetSpace) + 1;
            double spac = llen / spaces;
            _rVal.Add(new UVECTOR(sp));
            for (int i = 1; i <= spaces; i++)
            {
                sp += aDir * spac;
                _rVal.Add(new UVECTOR(sp));
            }

            if(bSaveToLine ) aLine.Points.Append(_rVal);
            return _rVal;
        }

        public static double ArcStripArea(double aLeftX, double aRightX, double aRadius)
        {
            aRadius = Math.Abs(aRadius);
            if (aRadius <= 0 || aLeftX == aRightX) return 0;

            mzUtils.SortTwoValues(true, ref aLeftX, ref aRightX);
            if (aLeftX <= -aRadius || aLeftX >= aRadius) aLeftX = -3 * aRadius;

            if (aRightX <= -aRadius || aRightX >= aRadius) aRightX = 3 * aRadius;

            mzUtils.SortTwoValues(true, ref aLeftX, ref aRightX);

            // full circle
            if (aLeftX < -aRadius && aRightX > aRadius) return Math.PI * Math.Pow(aRadius, 2);


            double x2;
            double y1;
            double y2;
            double area1;
            double area2;
            double acirc = Math.PI * Math.Pow(aRadius, 2);
            double ang;

            if (aLeftX < -aRadius)
            {
                //semi-circle
                if (aRightX == 0) return acirc / 2;

                double x1 = Math.Abs(aRightX);
                y1 = Math.Sqrt(Math.Pow(aRadius, 2) - Math.Pow(x1, 2));
                ang = 2 * Math.Atan(y1 / x1);
                area1 = Math.Pow(2 * aRadius, 2) / 8;
                area1 *= ang - Math.Sin(ang);
                return (aRightX > 0) ? acirc - area1 : area1;
            }


            if (aRightX > aRadius)
            {
                //semi-circle
                if (aLeftX == 0) return acirc / 2;

                double x1 = Math.Abs(aLeftX);
                y1 = Math.Sqrt(Math.Pow(aRadius, 2) - Math.Pow(x1, 2));
                ang = 2 * Math.Atan(y1 / x1);
                area1 = Math.Pow(2 * aRadius, 2) / 8;
                area1 *= ang - Math.Sin(ang);
                return (aLeftX < 0) ? acirc - area1 : area1;


            }
            area1 = acirc / 2;

            if (aRightX != 0)
            {
                double x1 = Math.Abs(aRightX);
                y1 = Math.Sqrt(Math.Pow(aRadius, 2) - Math.Pow(x1, 2));
                ang = 2 * Math.Atan(y1 / x1);
                area1 = Math.Pow(2 * aRadius, 2) / 8;
                area1 *= ang - Math.Sin(ang);
            }

            area2 = acirc / 2;
            if (aLeftX != 0)
            {
                x2 = Math.Abs(aLeftX);
                y2 = Math.Sqrt(Math.Pow(aRadius, 2) - Math.Pow(x2, 2));
                ang = 2 * Math.Atan(y2 / x2);
                area2 = Math.Pow(2 * aRadius, 2) / 8;
                area2 *= ang - Math.Sin(ang);
            }

            return (aLeftX <= 0 & aRightX >= 0) ? Math.Abs(acirc - area1 - area2) : Math.Abs(area1 - area2);

        }




        public static List<uopRingClipSegment> CreateRingClipSegments(uopPanelSectionShape aShape, out uopShape rRingClipBounds)
        {

            rRingClipBounds = null;
            List<uopRingClipSegment> _rVal = new List<uopRingClipSegment>();
            if (aShape == null) return _rVal;


            bool doarcs = aShape.LapsRing;
            rRingClipBounds = aShape.RingClipArcBounds();
            if (rRingClipBounds == null) return _rVal;
            if (doarcs)
            {
                List<uopArc> arcs = rRingClipBounds.Arcs();
                foreach (var arc in arcs)
                {
                    _rVal.Add(new uopRingClipSegment(arc));

                }

            }

            doarcs = false;
            if (!aShape.MDDesignFamily.IsStandardDesignFamily() && aShape.LapsDivider)
            {
                List<uopLine> lines = rRingClipBounds.Lines().FindAll(x => !x.IsVertical(2) && !x.IsHorizontal(2));
                foreach (var line in lines)
                    if (line.Length >= mdGlobals.HoldDownWasherDiameter) _rVal.Add(new uopRingClipSegment(line));

            }


            return _rVal;
        }

        internal static UHOLES LayoutHolesOnArc(dxeArc anArc, double aTargetSpace, double aDiameter, bool bCenterOnArc, double elevation = 0)
        {
            UHOLES _rVal = UHOLES.Null;
            if (anArc == null || aDiameter <= 0) return _rVal;
            double arclen;


            if (aTargetSpace <= aDiameter + 0.0625) aTargetSpace = aDiameter + 0.0625;


            arclen = anArc.Length;
            if (arclen == 0) return _rVal;

            int NumSpc;
            double ang = anArc.SpannedAngle;
            double sa = anArc.StartAngle;
            double ea = anArc.EndAngle;
            double angstep;
            UHOLE aHole = new UHOLE(aDiameter, aElevation: elevation);
            double ang1;
            double rad = anArc.Radius;
            dxfVector v1;
            dxfPlane aPl;
            NumSpc = (int)Math.Truncate(arclen / aTargetSpace) + 1;
            angstep = ang / NumSpc;
            rad = anArc.Radius;
            //angstep = math_ArcSine(aTargetSpace / rad, True)
            aPl = anArc.Plane;

            ang1 = sa;
            v1 = aPl.VectorPolar(rad, ang1, false);
            aHole.Center = UVECTOR.FromDXFVector(v1);
            aHole.Center.Elevation = elevation;
            _rVal.Member = aHole;
            _rVal.Centers.Add(aHole.Center);




            for (int i = 1; i <= NumSpc; i++)
            {
                ang1 += angstep;
                v1 = aPl.VectorPolar(rad, ang1, false);
                aHole.Center = UVECTOR.FromDXFVector(v1);
                aHole.Center.Elevation = elevation;
                _rVal.Centers.Add(aHole.Center);
            }

            if (bCenterOnArc)
            {
                if (_rVal.Centers.Count > 1)
                {
                    ang1 = aPl.XDirection.AngleTo(aPl.Origin.DirectionTo(aHole.Center.ToDXFVector()));
                    if (Math.Round(ang1, 3) < Math.Round(ea, 3))
                    {
                        ang = (anArc.EndAngle - ang1) / 2;
                        _rVal.Centers.Rotate(UVECTOR.FromDXFVector(aPl.Origin), ang, false);
                    }
                }
                else if (_rVal.Centers.Count == 1)
                {
                    _rVal.Centers.SetItem(1, UVECTOR.FromDXFVector(anArc.MidPt));
                }

            }
            return _rVal;
        }

        public static uopVectors LayoutPointsOnArc(uopArc anArc, double aTargetSpace, double aDiameter, bool bCenterOnArc, double aElevation= 0, bool bSaveToArc = true, string aTag = "'", string aFlag= "")
        {
            uopVectors _rVal = uopVectors.Zero;
            if (anArc == null || aDiameter <= 0) return _rVal;
            double arclen;


            if (aTargetSpace <= aDiameter + 0.0625) aTargetSpace = aDiameter + 0.0625;


            arclen = anArc.Length;
            if (arclen == 0) return _rVal;

            int NumSpc;
            double ang = anArc.SpannedAngle;
            double sa = anArc.StartAngle;
            double ea = anArc.EndAngle;
            double angstep;
            //UHOLE aHole = new UHOLE(aDiameter, aElevation: elevation);
            double ang1 = sa;
            double rad = anArc.Radius;
          
                dxfPlane aPl = new dxfPlane(anArc.Center);
            NumSpc = (int)Math.Truncate(arclen / aTargetSpace) + 1;
            angstep = ang / NumSpc;
            rad = anArc.Radius;
            //angstep = math_ArcSine(aTargetSpace / rad, True)
           
             if (bCenterOnArc)
            {
               
                ang1 = aPl.AngleTo(anArc.MidPoint);
                if (NumSpc > 0)
                {
                    ang1 -= NumSpc / 2 * angstep;
                    if (mzUtils.IsOdd(NumSpc)) ang1 -= angstep / 2;
                }

            }
            uopVector u1 = new uopVector(aPl.VectorPolar(rad, ang1, false)) { Elevation = aElevation, Radius = rad, Tag = aTag, Flag = aFlag };
            _rVal.Add(u1);
             if (bSaveToArc) anArc.Points.Add(new uopVector(u1));

            for (int i = 1; i <= NumSpc; i++)
            {
                ang1 += angstep;
                u1 = new uopVector(aPl.VectorPolar(rad, ang1, false)) { Elevation = aElevation, Radius = rad, Tag = aTag, Flag = aFlag };
                _rVal.Add(u1);
                if (bSaveToArc) anArc.Points.Add(new uopVector(u1));

            }

               

            //if (bCenterOnArc)
            //{
            //    if (_rVal.Count > 1)
            //    {
            //        u1 = _rVal[0];
            //        uopVector u2 = _rVal[_rVal.Count - 1];
            //        dxfDirection normal = aPl.ZDirection;

            //        ang1 = dxfVectors.VectorAngle(aPl.Origin, u2, ref normal);
            //        double ang2 = dxfVectors.VectorAngle(aPl.Origin, u2, ref normal);
            //        uopArc arc1 = new uopArc(rad, anArc.Center, ang1, ang2);
            //        ang1 = dxfVectors.VectorAngle(aPl.Origin, anArc.MidPoint, ref normal);
            //        ang2 = dxfVectors.VectorAngle(aPl.Origin, arc1.MidPoint, ref normal);

            //        if (Math.Round(ang1, 3) != Math.Round(ang2, 3))
            //        {
            //            ang = mzUtils.NormAng (ang2 - ang1,false,false,true); // / 2;
            //            _rVal.Rotate(anArc.Center, ang, false);
            //        }
            //    }
            //    else if (_rVal.Count == 1)
            //    {
            //        u1 = _rVal[0];
            //        uopVector u2 =  anArc.MidPoint;
            //        u1.SetCoordinates(u2.X, u2.Y);
            //    }

            //}
            return _rVal;
        }
        /// <summary>
        /// spaces holes along the passed line using the indicated MAX spacing with the first being on the start pt and the last being on the end point of the line
        /// </summary>
        /// <remarks>the best fit space is computed based ont he passed max space. at least points 2 points are returned unless the length of the line is less that the line length less 2x the buffet.</remarks>
        /// <param name="aLine">the subject line</param>
        /// <param name="aTargetSpace">the max target space to apply</param>
        /// <param name="aEndBuffer">a end buffer to apply. the effectively shortens the line by 2x the passed value </param>
        /// <param name="aTag">an pptional tag to apply to the returned points</param>
        /// <param name="aFlag">a an optional flag to apply to the returned points</param>
        /// <param name="bSaveToLine">a tag to save the points to the line</param>
        public static uopHoles LayoutHolesOnLine( uopLine aLine, double aTargetSpace, double aDiameter, double aEndBuffer = 0, double aElevation = 0, string aTag = "", string aFlag = "", bool bSaveToLine = false)
        {
            uopHoles _rVal = uopHoles.Zero;
            if (aLine == null || aDiameter <= 0) return _rVal;
            double linelen;

            if (aTargetSpace <= aDiameter + 0.0625) aTargetSpace = aDiameter + 0.0625;

            linelen = aLine.Length;
            aEndBuffer = Math.Abs(aEndBuffer);
            if (aEndBuffer >= linelen / 2) return _rVal;

            if (linelen == 0) return _rVal;
            int NumSpc;
            double step;
            uopHole aHole = new uopHole( aDiameter, aElevation: aElevation, aTag: aTag, aFlag:aFlag );
            uopVector sp = new uopVector(aLine.StartPt) { Elevation= aElevation, Radius = aDiameter/2};
            uopVector ep = new uopVector(aLine.EndPt);
            NumSpc = (int)Math.Truncate( linelen / aTargetSpace ) + 1;
            step = linelen / NumSpc;
            uopVector dir = (ep - sp).Normalized();

            if (aEndBuffer > 0)
            {
                sp += dir * aEndBuffer;
                ep += dir * -aEndBuffer;

                linelen = sp.DistanceTo(ep);
            }
            if(bSaveToLine) { aLine.Points.Add(new uopVector(sp)); }
            aHole.Center =  sp  ;
            _rVal.Member = new uopHole(aHole);
            _rVal.Centers.Add( aHole.Center );

            for (int i = 1; i <= NumSpc; i++)
            {
                var v = sp + (dir * (step * i));
                v.Elevation = aElevation;
                v.Radius = aHole.Radius;
                if (bSaveToLine) { aLine.Points.Add(new uopVector(v)); }

                _rVal.Centers.Add( v );
            }

            return _rVal;
        }

        public static string TrayList(string aList, string aDelimeter, bool bNumbersOnly, bool bNoAmpersand = true)
        {
            string ret = string.Empty;
            if (aDelimeter == null) aDelimeter = ",";
            TVALUES aVals = new TVALUES();
            int i;
            int j;
            List<dynamic> rstarts = new List<dynamic>();
            List<dynamic> rends = new List<dynamic>();
            string aStr;
            dynamic aVal;
            dynamic bVal;
            dynamic cval;
            dynamic dVal;
            int k;
            int cnt = 0;
            bool bspans = false;


            ret = aList.Trim();
            if (ret ==  string.Empty) return ret;


            aVals = TVALUES.FromDelimitedList(ret, aDelimeter, false, true, true, false, -1, true);
            ret = string.Empty;

            for (i = 1; i <= aVals.Count; i++)
            {
                aStr = Convert.ToString(aVals.Item(i));
                j = aStr.IndexOf("-");
                if (j >= 0)
                {
                    aVal = aStr.Substring(0, j);
                    bVal = aStr.Substring(j + 1);

                    if (aVal.ToString() ==  string.Empty || bVal.ToString() ==  string.Empty)
                    {
                        continue;
                    }
                    int iVal1 = mzUtils.VarToInteger(aVal);
                    int iVal2 = mzUtils.VarToInteger(bVal);


                    mzUtils.SortTwoValues(true, ref iVal1, ref iVal2);
                    aVal = iVal1;
                    bVal = iVal2;
                }
                else
                {
                    aVal = aStr.Trim();
                    if (aVal.ToString() ==  string.Empty) continue;

                    aVal = mzUtils.VarToInteger(aVal, true);
                    bVal = aVal;
                }
                rstarts.Add(aVal);
                rends.Add(bVal);

            }

            mzUtils.SortTwoLists(rstarts, rends);
            for (i = 0; i < rstarts.Count; i++)
            {
                aVal = rstarts[i];
                bVal = rends[i];
                j = i;

                for (k = i + 1; k < rstarts.Count; k++)
                {
                    cval = rstarts[k];
                    dVal = rends[k];
                    if ((int)cval == (int)bVal + 1)
                    {
                        bVal = dVal;
                        j = k;
                    }
                    else
                    {
                        break;
                    }
                }

                if (aVal != rends[j])
                {
                    aStr = aVal + "-" + rends[j];
                    bspans = true;
                }
                else
                {
                    aStr = aVal.ToString();
                }
                cnt++;
                mzUtils.ListAdd(ref ret, aStr, "", true, aDelimeter);
                i = j;
            }

            if (ret !=  string.Empty)
            {
                if (!bNumbersOnly)
                {
                    if (cnt > 1 || bspans)
                    {
                        ret = "TRAYS " + ret;
                    }
                    else
                    {
                        ret = "TRAY " + ret;
                    }
                }

                if (bNoAmpersand)
                {
                    if (cnt > 1)
                    {
                        i = ret.LastIndexOf(",");
                        if (i > 0)
                        {
                            ret = ret.Substring(0, i - 1) + "&" + ret.Substring(i);
                        }
                    }
                }
            }
            return ret;
        }


        /// <summary>
        /// Reads the value of the keyname under specified section
        /// </summary>
        /// <typeparam name="T">Type of the return value</typeparam>
        /// <param name="sectionName">Section in the file to extract a value from</param>
        /// <param name="keyName">Name of the value to extract a value from</param>
        /// <param name="defaultValue">Optional default value to return if the key is not found</param>
        /// <param name="size"></param>
        /// <returns></returns>
        internal static T ReadValue<T>(string sectionName, string keyName, string defaultValue, string filePath)
        {
            return ReadValue<T>(sectionName, keyName, defaultValue, filePath, out bool _);
        }

        /// <summary>
        /// Reads the value of the keyname under specified section
        /// </summary>
        /// <typeparam name="T">Type of the return value</typeparam>
        /// <param name="sectionName">Section in the file to extract a value from</param>
        /// <param name="keyName">Name of the value to extract a value from</param>
        /// <param name="defaultValue">Optional default value to return if the key is not found</param>
        /// <param name="filePath">Name of the file</param>
        /// <param name="isKeyFound">Return whether specified key found</param>
        /// <param name="size">Size of the output</param>
        /// <returns></returns>
        internal static T ReadValue<T>(string sectionName, string keyName, string defaultValue, string filePath, out bool isKeyFound, int size = 25000)
        {
            T _rVal = default;
            StringBuilder returnedValue = new StringBuilder(size);
            int status = PInvoker.GetPrivateProfileString(sectionName,
                                                            keyName,
                                                            defaultValue,
                                                            returnedValue,
                                                            size,
                                                            filePath);
            isKeyFound = status > 0;

            if (status > 0)
            {
                _rVal = ConvertFrom<T>(returnedValue.ToString());
            }

            return _rVal;
        }

        /// <summary>
        /// Converts value to the specified type from string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static T ConvertFrom<T>(string value)
        {
            T result = default;

            if (!string.IsNullOrEmpty(value))
            {
                var conv = System.ComponentModel.TypeDescriptor.GetConverter(typeof(T));

                if (conv.CanConvertFrom(typeof(string)))
                {
                    try
                    {
                        result = (T)conv.ConvertFrom(value);
                    }
                    catch
                    {
                        //TODO: Need to find any best way to get rid of empty catch block..
                    }
                }
            }

            return result;
        }

        public static string GetFileHeadingsList(string aFileSpec, bool bCapsOnly, string aPrefix = null)
        {
            //extracts then strings in the passed files that are enclosed in brackets

            string headers = string.Empty;
            string filePath = aFileSpec.Trim();
            int prefixLen = string.IsNullOrEmpty(aPrefix) ? 0 : aPrefix.Trim().Length;

            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {



                foreach (string line in File.ReadAllLines(filePath))
                {
                    string currentLine = line.Trim();

                    if (currentLine.IndexOf('[') == 0 && currentLine.LastIndexOf(']') == currentLine.Length - 1)
                    {
                        string currentHeader = currentLine.Substring(1, currentLine.Length - 2);
                        currentHeader = bCapsOnly ? currentHeader.ToUpper() : currentHeader;

                        if (currentHeader != string.Empty)
                        {
                            if (prefixLen > 0)
                            {
                                if (string.Compare(currentHeader.Substring(0, prefixLen), aPrefix, StringComparison.OrdinalIgnoreCase) == 0 && currentHeader.Length > 0)
                                {
                                    mzUtils.ListAdd(ref headers, currentHeader, bSuppressTest: true);
                                }
                            }
                            else
                            {
                                mzUtils.ListAdd(ref headers, currentHeader, bSuppressTest: true);
                            }
                        }
                    }
                }
            }

            return headers;
        }


        public static List<string> GetINIFileHeadingsList(string aFileSpec, bool bCapsOnly, string aPrefix = null)
        {
            //extracts then strings in the passed files that are enclosed in brackets

            List<string> _rVal = new List<string>();

            string filePath = aFileSpec.Trim();
            int prefixLen = string.IsNullOrEmpty(aPrefix) ? 0 : aPrefix.Trim().Length;

            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                string currentLine = string.Empty;
                string currentHeader = string.Empty;

                foreach (string line in File.ReadAllLines(filePath))
                {
                    currentLine = line.Trim();

                    if (currentLine.IndexOf('[') == 0 && currentLine.LastIndexOf(']') == currentLine.Length - 1)
                    {
                        currentHeader = currentLine.Substring(1, currentLine.Length - 2);
                        currentHeader = bCapsOnly ? currentHeader.ToUpper() : currentHeader;

                        if (currentHeader != string.Empty)
                        {
                            if (prefixLen > 0)
                            {
                                if (string.Compare(currentHeader.Substring(0, prefixLen), aPrefix, StringComparison.OrdinalIgnoreCase) == 0 && currentHeader.Length > 0)
                                {
                                    _rVal.Add(currentHeader);
                                }
                            }
                            else
                            {
                                _rVal.Add(currentHeader);
                            }
                        }
                    }
                }
            }

            return _rVal;
        }

        /// <summary>
        ///#1the shell diameter to use in the calculation
        //^returns the beam or downcomer to ring clearance based on the passed shell diameter
        //~this is the limiting factor for the length of a beam or dowmcomer

        /// </summary>
        /// <param name="aShellDia"></param>
        /// <returns></returns>
        public static double BoundingClearance(double? aShellDia)
        {
            aShellDia = aShellDia == null ? 0 : aShellDia;
            aShellDia = Math.Abs(aShellDia.Value);
            if (aShellDia <= 36) return 0.75;
            if (aShellDia > 36 && aShellDia <= 84) return 0.875;
            if (aShellDia > 84 && aShellDia <= 156) return 0.875;
            if (aShellDia > 156 && aShellDia <= 240) return 1.125;
            if (aShellDia > 240 & aShellDia <= 300) return 1.25;
            if (aShellDia > 300 & aShellDia <= 420) return 1.5;

            //if (aShellDia > 420)
            //{
            return 1.625;
            //}

            // return 2;
        }

        public static uppGridAlignments GetGridAlignment(uppVerticalAlignments vAlignment, uppHorizontalAlignments hAlignment)
        {
            switch (vAlignment)
            {
                case uppVerticalAlignments.Top:
                    {
                        switch (hAlignment)
                        {
                            case uppHorizontalAlignments.Left:
                                {
                                    return uppGridAlignments.TopLeft;
                                }
                            case uppHorizontalAlignments.Center:
                                {
                                    return uppGridAlignments.TopCenter;
                                }
                            case uppHorizontalAlignments.Right:
                                {
                                    return uppGridAlignments.TopRight;
                                }
                            default:
                                {
                                    return uppGridAlignments.MiddleCenter;

                                }
                        }

                    }
                case uppVerticalAlignments.Center:
                    {
                        switch (hAlignment)
                        {
                            case uppHorizontalAlignments.Left:
                                {
                                    return uppGridAlignments.MiddleLeft;
                                }
                            case uppHorizontalAlignments.Center:
                                {
                                    return uppGridAlignments.MiddleCenter;
                                }
                            case uppHorizontalAlignments.Right:
                                {
                                    return uppGridAlignments.MiddleRight;
                                }
                            default:
                                {
                                    return uppGridAlignments.MiddleCenter;

                                }
                        }
                    }
                case uppVerticalAlignments.Bottom:
                    {
                        switch (hAlignment)
                        {
                            case uppHorizontalAlignments.Left:
                                {
                                    return uppGridAlignments.BottomLeft;
                                }
                            case uppHorizontalAlignments.Center:
                                {
                                    return uppGridAlignments.BottomCenter;
                                }
                            case uppHorizontalAlignments.Right:
                                {
                                    return uppGridAlignments.BottomRight;
                                }
                            default:
                                {
                                    return uppGridAlignments.MiddleCenter;

                                }
                        }
                    }
                default:
                    {
                        return uppGridAlignments.MiddleCenter;

                    }
            }
        }

        public static double IdealSpoutLength(double aTargetArea, int aTargetCount, double aRadius, bool bMetric, double? aMaxLength = null, bool bSuppressRound = false)
        {
            double _rVal = aRadius;

            if (aTargetArea <= 0 || aTargetCount <= 0 || aRadius <= 0) return 0;

            double len1 = aTargetArea / aTargetCount;
            len1 -= Math.PI * Math.Pow(aRadius, 2);
            len1 = len1 / (2 * aRadius) + 2 * aRadius;


            if (!bSuppressRound)
            {
                dxxRoundToLimits rnd2 = bMetric ? dxxRoundToLimits.Millimeter : dxxRoundToLimits.Sixteenth;
                len1 = uopUtils.RoundTo(len1, rnd2, false, false);
            }
            if (len1 < 0) len1 = 0;

            if (len1 < 2 * aRadius) len1 = 2 * aRadius;


            _rVal = len1; //+ 2 * aRadius
            if (aMaxLength.HasValue)
            {
                if (_rVal > aMaxLength.Value) _rVal = aMaxLength.Value;

            }
            return _rVal;
        }


        /// <summary>
        /// Reads version of the application
        /// </summary>
        /// <param name="aFileSpec"></param>
        /// <param name="sSection"></param>
        /// <param name="sKey"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static double ReadINI_Version(string aFileSpec, string sSection, string sKey, double defaultValue)
        {
            double result = defaultValue;
            string bStr = string.Empty;
            int cnt = 0;
            string aStr = ReadValue<string>(sSection, sKey, "", aFileSpec, out bool keyBound);

            if (keyBound && !string.IsNullOrEmpty(aStr))
            {
                char[] aDataInChars = aStr.ToCharArray();
                int charNumber;
                char aChar;
                for (int i = 0; i < aDataInChars.Length; i++)
                {
                    aChar = aDataInChars[i];
                    charNumber = (int)aDataInChars[i];

                    if (charNumber >= 48 && charNumber <= 57)
                    {
                        bStr += aChar;
                    }
                    else if (aChar == '.')
                    {
                        cnt++;
                        if (cnt == 1)
                            bStr += aChar;
                    }
                }

                bStr = bStr.Trim();
                if (!string.IsNullOrEmpty(bStr))
                    double.TryParse(bStr, out result);
            }
            return result;
        }
        /// <summary>
        ///#1the angle in radians to get the ArcCosine for
        ///#2flag to request the returned value in degrees 
        /// </summary>
        /// <param name="aVal"></param>
        /// <param name="bReturnDegrees"></param>
        /// <returns></returns>
        public static double ArcCosine(double aVal, bool bReturnDegrees = false)
        {
            double _rVal = 0;

            if (!mzUtils.IsNumeric(aVal)) return 0;

            //^used to calculate the ArcCosine of an angle expressed in radians

            if (Math.Abs(aVal) == 1)
            {
                if (aVal == 1)
                {
                    _rVal = Math.PI / 2;
                }
                else
                {
                    _rVal = -Math.PI / 2;
                }
            }
            else
            {
                double dVal = 0;
                double bVal = 0;

                dVal = aVal;
                bVal = -dVal * dVal + 1;

                if (Math.Round(bVal, 6) != 0)
                {
                    _rVal = Math.Atan(-dVal / Math.Sqrt(bVal)) + 2 * Math.Atan(1);
                }
            }

            if (bReturnDegrees)
            {
                _rVal = _rVal * 180 / Math.PI;
            }
            return _rVal;
        }



        /// <summary>
        /// Project Select Name
        /// </summary>
        /// <param name="keyNumber"></param>
        /// <param name="revision"></param>
        /// <param name="projectType"></param>
        /// <returns></returns>
        internal static string ProjectSelectName(string keyNumber, int revision, uppProjectTypes projectType)
        {
            string _rVal = string.Empty;
            _rVal = $"{keyNumber}-{revision}";
            if (projectType == uppProjectTypes.CrossFlow)
            {
                _rVal += "XF";
            }
            else if (projectType == uppProjectTypes.MDSpout)
            {
                _rVal += "MD";
            }
            else if (projectType == uppProjectTypes.MDDraw)
            {
                _rVal += "MDD";
            }
            return _rVal;
        }
        //    public static string ReadINI_String(string aFileSpec, string sSection, string sKey, string sDefault = "", BoolHelper bKeyFound = null)
        //    {
        //        //'#1the path to an INI formatted text file
        //        //'#2the section in the file to extract a value from
        //        //'#3the name of the value to extract a value from
        //        //'#4an optional default value to return if the key is not found
        //        //'#5returns false if the passed key is not found
        //        //'^used to extract an string value from the passed INI formatted file
        //        //'~does not raise an error if the file or key is not found just returns the default value.
        //        //'~uses the "GetPrivateProfileString" windows API call.
        //        if (bKeyFound == null) bKeyFound = new BoolHelper();
        //        string ReadINI_String = string.Empty;
        //        int LR;
        //        StringBuilder sReturnedValue = new StringBuilder(' ', 10000);
        //        LR = PInvoker.GetPrivateProfileString(sSection, sKey, sDefault, sReturnedValue, 100000, aFileSpec);
        //        if (LR == 0)
        //        {
        //            ReadINI_String = sDefault;
        //            bKeyFound.Flag = false;
        //        }
        //        else
        //        {
        //            ReadINI_String = sReturnedValue.ToString().Substring(0, LR);
        //            bKeyFound.Flag = true;
        //        }
        //        return ReadINI_String.Trim();
        //    }

        /// <summary>
        /// Copy collection to new collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="copyCol"></param>
        /// <returns></returns>
        public static List<T> CopyCollection<T>(List<T> copyCol)
        {
            //'^returns a new collection with the members of the passed colleciton added to it
            List<T> _rVal = new List<T>();
            if (copyCol == null) return _rVal;
            for (int i = 0; i < copyCol.Count; i++)
            {
                if (copyCol[i] != null) _rVal.Add(Force.DeepCloner.DeepClonerExtensions.DeepClone<T>(copyCol[i]));
            }
            return _rVal;
        }

        public static string PluralizeList(string aList, string aBase)
        {
            aBase ??= string.Empty;
            aList ??= string.Empty;
            string _rVal = aBase.Trim().ToUpper();
            if (string.IsNullOrWhiteSpace(_rVal)) return aList;

            if (ListIsPlural(aList))
                _rVal += "S";

            _rVal = $"{_rVal} {aList}";
            return _rVal;

        }

        public static bool ListIsPlural(string aList)
        {
            if (string.IsNullOrWhiteSpace(aList)) return false;
            return aList.Contains("-") || aList.Contains(",");
        }

        public static double DefaultTrayDiameter(double aShellID, double aRingID)
        {
            double sid = Math.Abs(aShellID);
            double rid = Math.Abs(aRingID);
            mzUtils.SortTwoValues(true, ref rid, ref sid);
            return (sid + rid) / 2;
        }
        public static double DefaultTrayDiameter(uopTrayRange aRange)
        {
            if (aRange == null) return 0;
            aRange.UpdateRangeProperties();
            return DefaultTrayDiameter(aRange.ShellID, aRange.RingID);

        }
        public static bool FileIsOpen(string aFilePath)
        {
            return !string.IsNullOrWhiteSpace(aFilePath) && FileIsOpen(new FileInfo(aFilePath));
        }
        public static bool FileIsOpen(FileInfo file)
        {

            return file != null && !CanReadFile(file.FullName);


        }



        const int ERROR_SHARING_VIOLATION = 32;
        const int ERROR_LOCK_VIOLATION = 33;

        private static bool IsFileLocked(Exception exception)
        {
            int errorCode = Marshal.GetHRForException(exception) & ((1 << 16) - 1);
            return errorCode == ERROR_SHARING_VIOLATION || errorCode == ERROR_LOCK_VIOLATION;
        }

        internal static bool CanReadFile(string filePath)
        {
            //Try-Catch so we dont crash the program and can check the exception
            try
            {
                //The "using" is important because FileStream implements IDisposable and
                //"using" will avoid a heap exhaustion situation when too many handles  
                //are left undisposed.
                using FileStream fileStream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                if (fileStream != null) fileStream.Close();  //This line is me being overly cautious, fileStream will never be null unless an exception occurs... and I know the "using" does it but its helpful to be explicit - especially when we encounter errors - at least for me anyway!
            }
            catch (IOException ex)
            {
                //THE FUNKY MAGIC - TO SEE IF THIS FILE REALLY IS LOCKED!!!
                if (IsFileLocked(ex))
                {
                    // do something, eg File.Copy or present the user with a MsgBox - I do not recommend Killing the process that is locking the file
                    return false;
                }
            }
            finally
            { }
            return true;
        }
        public static void OpenFileInSystemApp(string aFileSpec)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(aFileSpec)) return;
                if (!File.Exists(aFileSpec)) return;
                System.Diagnostics.Process.Start(aFileSpec);

            }
            catch { }
        }

        public static int OpposingIndex(int aIndex, int aCount, int aDefault = 0)
        {
            if (aCount <= 0 || aIndex <= 0 || aIndex > aCount) return aDefault;
            return aCount - aIndex + 1;
        }


        /// <summary>
        /// calculates the differences between the two passed areas and returns true if the percent difference is less than the passed limit
        /// </summary>
        /// <param name="aTargetArea">the target area</param>
        /// <param name="aCurrentArea">the actual area</param>
        /// <param name="rRatio">returns the ratio of the current area / the target </param>
        /// <returns></returns>
        public static double TabulateAreaDeviation(double aTargetArea, double aCurrentArea, out double rRatio)
        {
            rRatio = (aTargetArea != 0) ? aCurrentArea / aTargetArea : 0;

            double _rVal = 0;
            double dif = 0;

            if (rRatio > 0)
            {
                dif = rRatio - 1;
                _rVal = dif * 100;
            }
            else
            {
                _rVal = (aTargetArea != 0) ? -100 : 0;
            }
            return Math.Abs(_rVal);

        }

    }


}
