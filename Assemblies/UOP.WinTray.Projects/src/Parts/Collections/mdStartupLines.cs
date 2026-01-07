using UOP.DXFGraphics;
using System;
using System.Collections.Generic;
using System.Linq;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;

namespace UOP.WinTray.Projects.Parts
{
    public class mdStartupLines : List<mdStartupLine>, IEnumerable<mdStartupLine>
    {

        /// <summary>
        /// a collection of mdStartupLine objects
        /// </summary>
        /// 
        public mdStartupLines() { base.Clear(); }

        internal mdStartupLines(TSTARTUPLINES aStructure, bool bSetRelations = false)
        {
            SpoutHeight = aStructure.SpoutHeight;
            SpoutLength = aStructure.SpoutLength;
            base.Clear();
            for (int i = 1; i <= aStructure.Count; i++)
            {
                Add(new mdStartupLine(aStructure.Item(i)));
            }
            if (bSetRelations)
            {
                mdStartupLine UL = Find((x) => x.Tag == "UL");
                mdStartupLine LL = Find((x) => x.Tag == "LL");
                mdStartupLine UR = Find((x) => x.Tag == "UR");
                mdStartupLine LR = Find((x) => x.Tag == "LR");

                if (UL != null && LL != null)
                {
                    UL.NeightborBelow = LL;
                    LL.NeightborAbove = UL;
                }
                if (UR != null && LR != null)
                {
                    UR.NeightborBelow = LR;
                    LR.NeightborAbove = UR;
                }

            }
        }

        public mdStartupLines(bool bCloneMembers, mdStartupLines aColToCopy, bool? bSpressed = null)
        {
            base.Clear();
            if (aColToCopy == null) return;
            SpoutLength = aColToCopy.SpoutLength;
            SpoutHeight = aColToCopy.SpoutHeight;

            foreach (mdStartupLine item in aColToCopy)
            {
                mdStartupLine mem = !bCloneMembers ? item : new mdStartupLine(item);
                if (bSpressed.HasValue) mem.Suppressed = bSpressed.Value;
                Add(mem);
            }

        }

        public mdStartupLines(List<mdStartupLine> aColToCopy, bool bCloneMembers = false)
        {
            base.Clear();
            if (aColToCopy == null) return;
            foreach (mdStartupLine item in aColToCopy)
            {
                Add(item, bCloneMembers);
            }

        }

        /// <summary>
        ///returns the item from the collection at the requested index
        /// </summary>
        /// 
        public mdStartupLine Item(int aIndex)
        {


            mdStartupLine _rVal = null;

            int idx = aIndex - 1;

            if (aIndex >= 1 & aIndex <= Count)
            {
                base[idx].Index = aIndex;
                _rVal = base[idx];

            }
            else
            {
                throw new IndexOutOfRangeException();
            }
            return _rVal;
        }

        /// <summary>
        ///returns the structure 
        /// </summary>
        /// 

        internal TSTARTUPLINES Structure_Get()
        {
            TSTARTUPLINES _rVal = new TSTARTUPLINES() { SpoutHeight = SpoutHeight, SpoutLength = SpoutLength };

            for (int i = 1; i <= Count; i++) { mdStartupLine mem = Item(i); _rVal.Add(new TSTARTUPLINE(mem)); }
            return _rVal;

        }


        /// <summary>
        ///returns the height iof the spout 
        /// </summary>
        /// 
        public double SpoutHeight { get; set; }

        /// <summary>
        ///returns the length of the spout 
        /// </summary>
        /// 
        public double SpoutLength { get; set; }


        /// <summary>
        /// #1the item to add to the collection
        /// ^used to add an item to the collection
        /// ~won't add "Nothing" (no error raised).
        /// </summary>
        public mdStartupLine Add(mdStartupLine aLine, bool bAddClones = false)
        {

            if (aLine == null) return null;
            base.Add(bAddClones ? new mdStartupLine(aLine) : aLine);
            return Item(Count);

        }

        /// <summary>
        /// #1a collection of lines to add to the current collection
        /// ^appends the members of the passed lines to the current collection
        /// </summary>
        public void Append(mdStartupLines newLines, bool bAddClones = false, string bSuppressedVal = null)
        {
            if (newLines == null) return;

            if (newLines.Count == 0) return;
            mdStartupLine nline = null;
            bool bSetSup = !string.IsNullOrWhiteSpace(bSuppressedVal);
            bool supVal = false;
            //
            // If Not IsMissing(aSuppressedVal) Then
            //Changed as per vb code
            if (bSetSup) supVal = mzUtils.VarToBoolean(bSuppressedVal);


            for (int i = 1; i <= newLines.Count; i++)
            {
                nline = newLines.Item(i);
                Add(nline, bAddClones);
                if (bSetSup) base[Count - 1].Suppressed = supVal;

            }

        }

        public override string ToString() => $"mdStartupLines[{Count}]";




        /// <summary>
        /// Sort mdstratupline collection 
        /// </summary>
        private List<mdStartupLine> zSort(List<mdStartupLine> aLines, mzSortOrders aSortOrder, bool? bSupVal = null, bool? bMarkVal = null)
        {
            List<mdStartupLine> _rVal = new List<mdStartupLine>();
            if (aLines == null) return _rVal;

            bool bSetSup = bSupVal.HasValue;
            bool bSetMark = bMarkVal.HasValue;
            double yord = 0;
            List<double> ords = new List<double>();
            mdStartupLine aMem;

            List<Tuple<double, int>> srt = new List<Tuple<double, int>>();

            for (int i = 0; i < aLines.Count; i++)
            {
                aMem = aLines[i];
                if (bSetSup) aMem.Suppressed = bSupVal.Value;
                if (bSetMark) aMem.Mark = bMarkVal.Value;
                yord = Math.Round(aMem.SPT.MidPt(aMem.EPT).Y, 3);
                if (!ords.Contains(yord))
                {
                    srt.Add(new Tuple<double, int>(yord, i));
                    ords.Add(yord);
                }


            }

            srt = srt.OrderBy(t => t.Item1).ToList();
            if (aSortOrder == mzSortOrders.HighToLow) srt.Reverse();

            foreach (Tuple<double, int> tpl in srt) { _rVal.Add(aLines[tpl.Item2]); }

            return _rVal;
        }

        /// <summary>
        /// Clears it and make the structure count to zero
        /// </summary>
        /// 
        public new bool Clear()
        {
            bool _rVal = Count > 0;

            base.Clear();
            return _rVal;
        }

        /// <summary>
        /// ^returns an new object with the same properties as the cloned object
        /// </summary>
        public mdStartupLines Clone(bool? bSpressed = null) => new mdStartupLines(true, this, bSpressed);

        /// <summary>
        /// ^returns the mdStartupLine collection memebers on the downcomer Index
        /// </summary>
        public List<mdStartupLine> GetByDowncomerIndex(int aIndex, string aSide, bool? bReturnSuppressed = null, bool bReturnClone = true, mzSortOrders aSortOrder = mzSortOrders.None)
        {
            List<mdStartupLine> _rVal = new List<mdStartupLine>();
            aSide = aSide.ToUpper().Trim();
            List<mdStartupLine> mems = FindAll((x) => x.DowncomerIndex == aIndex);

            foreach (mdStartupLine item in mems)
            {
                mdStartupLine aMem = bReturnClone ? new mdStartupLine(item) : item;
                if (aSide == "L")
                {
                    if (aMem.LeftSide)
                    {
                        if (bReturnSuppressed.HasValue) aMem.Suppressed = bReturnSuppressed.Value;

                        _rVal.Add(aMem);
                    }
                }
                else if (aSide == "R")
                {
                    if (!aMem.LeftSide)
                    {
                        if (bReturnSuppressed.HasValue) aMem.Suppressed = bReturnSuppressed.Value;

                        _rVal.Add(aMem);
                    }
                }
                else
                {
                    if (bReturnSuppressed.HasValue) aMem.Suppressed = bReturnSuppressed.Value;

                    _rVal.Add(aMem);
                }
            }

            if (aSortOrder != mzSortOrders.None) _rVal = zSort(_rVal, aSortOrder);

            return _rVal;

        }

        /// <summary>
        /// ^returns the first line that ends at the passed point
        /// </summary>
        public mdStartupLine GetByEndPoint(dxfVector aPoint)
        {
            if (aPoint == null) return null;
            mdStartupLine aMem;


            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                if (aMem.StartPt.Equals(aPoint))
                {
                    return aMem;

                }
            }
            return null;
        }

        /// <summary>
        /// #1the handle to search for
        /// #2returns all then lines with the passed handle
        /// ^returns the first line with a handle matching the passed value
        /// </summary>
        public mdStartupLine GetByHandle(string aSpoutGroupHandle, out List<mdStartupLine> rHandleLines)
        {

            rHandleLines = FindAll((x) => string.Compare(x.Handle, aSpoutGroupHandle, true) == 0);
            return rHandleLines.Count > 0 ? rHandleLines[0] : null;
        }


        /// <summary>
        /// returns "L" or "R" for left or right side
        /// </summary>
        public List<mdStartupLine> GetBySide(string aSide)
        {
            List<mdStartupLine> _rVal = new List<mdStartupLine>();
            if (string.IsNullOrWhiteSpace(aSide)) return _rVal;
            aSide = aSide.ToUpper().Trim().Substring(0, 1);
            List<mdStartupLine> sCol = FindAll((x) => x.Handle.Length >= 2);
            return sCol.FindAll((x) => string.Compare(x.Handle.Substring(1, 1), aSide, true) == 0);
        }




        /// <summary>
        ///returns and removes the lines that have a suppressed flag equal to the passed flag
        /// </summary>
        public mdStartupLines GetBySuppressed(bool bSuppressedVal)
        {
            return new mdStartupLines(FindAll((x) => x.Suppressed == bSuppressedVal), bCloneMembers: false);
        }

        /// <summary>
        /// #1the tag to search for
        /// #2returns all the members that have the passed tag
        /// ^returns the first line that has the passed tag
        /// </summary>
        public List<mdStartupLine> GetByTag(string aTag)
        {
            return string.IsNullOrWhiteSpace(aTag) ? new List<mdStartupLine>() : FindAll((x) => string.Compare(x.Tag, aTag, true) == 0);

        }

        /// <summary>
        /// #1the tag to search for
        /// #2a suppression flag to apply
        /// ^returns the first line that has the passed tag
        /// </summary>
        public mdStartupLine GetTagged(string aTag, bool? bSuppressedVal = null)
        {
            if (string.IsNullOrWhiteSpace(aTag)) return null;
            aTag = aTag.Trim();
            mdStartupLine _rVal = Find((x) => string.Compare(x.Tag, aTag, true) == 0);
            if (_rVal != null && bSuppressedVal.HasValue) _rVal.Suppressed = bSuppressedVal.Value;
            return _rVal;
        }

        /// <summary>
        /// #1flag to only return the occurance of the unsupressed lines
        /// #2a length limit to apply
        /// ^returns the total of the Occurs properties of all the lines in the collection
        /// </summary>
        public int GetTotalOccurance(bool bUnspressedOnly, double aLongerThan = 0)
        {
            int _rVal = 0;


            foreach (mdStartupLine mem in this)
            {
                if ((!bUnspressedOnly || (bUnspressedOnly && !mem.Suppressed)) && mem.MaxLength >= aLongerThan)
                    _rVal += mem.Occurs;
            }


            return _rVal;
        }

        /// <summary>
        ///returns the longest member in the collection
        /// </summary>
        public mdStartupLine LongestMember(bool bUnsupressedOnly = false)
        {
            mdStartupLine _rVal = null;
            double cval = 0;


            for (int i = 1; i <= Count; i++)
            {
                mdStartupLine aMem = Item(i);
                if (!bUnsupressedOnly || (bUnsupressedOnly && !aMem.Suppressed))
                {
                    double lVal = aMem.MaxLength;
                    if (lVal > cval)
                    {
                        _rVal = aMem;
                        cval = lVal;
                    }
                }
            }
            return _rVal;
        }

        /// <summary>
        /// #1a tag to filter for
        /// ^returns the mid points of all then lines in the collection
        /// </summary>
        public uopVectors MidPoints(string aTag = "")
        {
            uopVectors _rVal = new uopVectors();
            List<mdStartupLine> scol = string.IsNullOrWhiteSpace(aTag) ? this : GetByTag(aTag);
            foreach (mdStartupLine item in scol)
            {
                _rVal.Add(item.MidPt);
            }
            return _rVal;
        }

        /// <summary>
        /// the method to re index
        /// </summary>
        public void ReIndex()
        {
            mdStartupLine aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                aMem.Index = i;
            }
        }

        /// <summary>
        /// removes the item from the collection at the requested index
        /// </summary>
        public void Remove(int Index)
        {
            if (Index > 0 & Index <= Count)
            {
                base.RemoveAt(Index - 1);
                ReIndex();
            }
        }



        /// <summary>
        /// #1the linetype to assign to all of the line in the collection
        /// ^used to assign to all of the lines in the collection
        /// </summary>
        public void SetLineTypes(string LineType, List<mdStartupLine> SearchCol = null, string SearchType = "")
        {

            List<mdStartupLine> sCol = SearchCol ?? this;
            if (!string.IsNullOrWhiteSpace(SearchType)) sCol = sCol.FindAll((x) => string.Compare(x.LineType, SearchType, true) == 0);
            foreach (mdStartupLine item in sCol)
            {
                item.LineType = LineType;
            }

        }

        /// <summary>
        /// #1flag to only return the occurance of the unsupressed lines
        /// #2a length limit to apply
        /// ^returns the total of the Occurs properties of all the lines in the collection
        /// </summary>
        public void SetMinLength(double aMinLength, bool bSuppressTheLessThan)
        {
            mdStartupLine aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                aMem.MinLength = aMinLength;
                if (bSuppressTheLessThan && aMem.MaxLength < aMinLength)
                {

                    aMem.Suppressed = true;
                }

            }
        }

        /// <summary>
        ///returns the shortest member in the collection
        /// </summary>
        public mdStartupLine ShortestMember(bool bUnsupressedOnly)
        {
            mdStartupLine _rVal = null;
            double cval = double.MaxValue;

            for (int i = 1; i <= Count; i++)
            {
                mdStartupLine aMem = Item(i);
                if (!bUnsupressedOnly || (bUnsupressedOnly && !aMem.Suppressed))
                {
                    double lVal = aMem.Length;
                    if (lVal < cval)
                    {
                        _rVal = aMem;
                        cval = lVal;
                    }
                }
            }
            return _rVal;
        }

        /// <summary>
        /// check whether the value can be suppresed or not
        /// </summary>
        public void Suppress(string TagToSuppress = "ALL")
        {
            TagToSuppress = TagToSuppress.ToUpper();
            mdStartupLine aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                if (TagToSuppress == "ALL")
                { aMem.Suppressed = true; }
                else
                {
                    if (aMem.Tag.ToUpper() == TagToSuppress)
                    {
                        aMem.Suppressed = true;
                    }
                }
            }
        }

        /// <summary>
        ///returns the count of the suppresed values
        /// </summary>
        public int SuppressedCount(bool aSuppressVal)
        {

            return FindAll((x) => x.Suppressed == aSuppressVal).Count;
        }

        /// <summary>
        ///returns the mdStartupLines members on the configuration
        /// </summary>
        public mdStartupLines ToggleByConfiguration(uppMDDesigns aDesignFamily, uppStartupSpoutConfigurations aConfiguration, mdSpoutGroup aSpoutGroup, mdSpoutGroup aSGBelow)
        {

            mdStartupLines _rVal = Clone(bSpressed: true);
            int sgidx = 0;
            int trgcnt = 0;
            dxxOrthoDirections direction = dxxOrthoDirections.Up;
            bool endgroup = false;
            bool limitedtop = false;
            bool limitedbot = false;
            double sgx = 0;
            if (aSpoutGroup != null)
            {
                sgx = aSpoutGroup.X;
                sgidx = aSpoutGroup.GroupIndex;
                endgroup = aSpoutGroup.LimitedBounds;
                direction = aSpoutGroup.Direction;
                if (endgroup)
                {
                    limitedtop = aSpoutGroup.LimitedTop;
                    limitedbot = aSpoutGroup.LimitedBottom;
                }
            }

            if (aConfiguration >= uppStartupSpoutConfigurations.None && aConfiguration <= uppStartupSpoutConfigurations.FourByFour)
            {
                if (aConfiguration > uppStartupSpoutConfigurations.None)
                {
                    trgcnt = (aConfiguration == uppStartupSpoutConfigurations.TwoByTwo) ? 2 : 4;
                    if (aConfiguration == uppStartupSpoutConfigurations.TwoByFour && endgroup) trgcnt = 2;
                }
                else
                {
                    trgcnt = 0;
                    _rVal.Suppress("ALL");
                }


                if (trgcnt == 2 && endgroup && aConfiguration == uppStartupSpoutConfigurations.TwoByFour)
                {
                    if (aSpoutGroup != null && aSGBelow != null)
                    {
                        if (aSpoutGroup.TheoreticalArea >= (0.75 * aSGBelow.TheoreticalArea)) trgcnt = 4;

                    }
                }
            }


            if (trgcnt == 0) return _rVal;

            mdStartupLine UL = _rVal.Find((x) => x.End == uppSides.Top && x.Side == uppSides.Left);
            mdStartupLine LL = _rVal.Find((x) => x.End == uppSides.Bottom && x.Side == uppSides.Left);
            mdStartupLine UR = _rVal.Find((x) => x.End == uppSides.Top && x.Side == uppSides.Right);
            mdStartupLine LR = _rVal.Find((x) => x.End == uppSides.Bottom && x.Side == uppSides.Right);



            if (trgcnt == 2)
            {
                if (UL != null || LL != null)
                {

                    if (UL != null && LL != null)
                    {
                        if (endgroup)
                        {
                            UL.Suppressed = UL.Length <= LL.Length;
                        }
                        else
                        {
                            UL.Suppressed = false;
                        }

                        LL.Suppressed = !UL.Suppressed;


                        //if (aDesignFamily.IsStandardDesignFamily())
                        //{
                        //    if (direction == dxxOrthoDirections.Up) UL.Suppressed = false; else LL.Suppressed = false;
                        //}
                        //else
                        //{
                        //    if (direction == dxxOrthoDirections.Up || !endgroup)
                        //    {
                        //        UL.Suppressed = false;
                        //    }
                        //    else
                        //    {
                        //        if (sgx >= 0)
                        //            UL.Suppressed = false;
                        //        else
                        //            LL.Suppressed = false;
                        //    }
                        //}

                    }
                    else
                    {
                        if (UL != null) UL.Suppressed = false; else LL.Suppressed = false;
                    }
                    //if (aSpoutGroup.Handle == "7,12")
                    //{
                    //    if (UL.Suppressed && !LL.Suppressed)
                    //    {
                    //        LL.sp.Y = UL.MaxY();
                    //    }
                    //    else if (!UL.Suppressed && !LL.Suppressed)
                    //    {
                    //        UL.sp.Y = LL.MinY();
                    //    }
                    // }
                }



                if (UR != null || LR != null)
                {
                    if (UR != null && LR != null)
                    {
                        if (endgroup)
                        {
                            LR.Suppressed = LR.Length <= UR.Length;
                        }
                        else
                        {
                            LR.Suppressed = false;
                        }

                        UR.Suppressed = !LR.Suppressed;

                        //if (aDesignFamily.IsStandardDesignFamily())
                        //{
                        //    if (direction == dxxOrthoDirections.Up) LR.Suppressed = false; else UR.Suppressed = false;
                        //}
                        //else
                        //{
                        //    if (direction == dxxOrthoDirections.Up || !endgroup) 
                        //    { 
                        //        LR.Suppressed = false; 
                        //    }
                        //    else
                        //    {
                        //        if (sgx >= 0)
                        //            UR.Suppressed = false;
                        //        else
                        //            LR.Suppressed = false;
                        //    }
                        //}


                    }
                    else
                    {
                        if (UR != null) UR.Suppressed = false; else LR.Suppressed = false;
                    }
                }

            }
            else
            {

                if (UL != null) UL.Suppressed = false;
                if (LL != null) LL.Suppressed = false;
                if (UR != null) UR.Suppressed = false;
                if (LR != null) LR.Suppressed = false;

            }


            if (UL != null && LL != null)
            {
                UL.NeightborBelow = LL;
                LL.NeightborAbove = UL;
            }
            if (UR != null && LR != null)
            {
                UR.NeightborBelow = LR;
                LR.NeightborAbove = UR;
            }

            return _rVal;
        }

        /// <summary>
        ///returns the mdStartupLines members on the downcomers
        /// </summary>
        public mdStartupLines ToggleByDowncomers(colMDSpoutGroups aSpoutGroups, colMDDowncomers aDowncomers, double aMinLength)
        {

            mdStartupLines _rVal = new mdStartupLines();
            double tY = 0;
            double bY = 0;
            double y1 = 0;
            mdStartupLine aMem = null;
            for (int i = 1; i <= aDowncomers.Count; i++)
            {
                mdDowncomer aDC = aDowncomers.Item(i);
                if (aDC.IsVirtual) continue;
                List<double> dcOrds;
                List<mdStartupLine> dcLines;
                for (int loop = 1; loop <= 2; loop++)
                {
                    if (loop == 1) // swap from right to left on the dc
                    {
                        // get the Y values of the SUS on the right side of the downcomer
                        dcOrds = aDC.StartupOrdinates(bLeft: false);
                        // get the startup lines on the right side of the downcomer
                        dcLines = GetByDowncomerIndex(aDC.Index, "R", bReturnSuppressed: true, bReturnClone: false);
                    }
                    else
                    {
                        // get the Y values of the SUS on the left side of the downcomer
                        dcOrds = aDC.StartupOrdinates(bLeft: true);
                        // get the startup lines on the left side of the downcomer
                        dcLines = GetByDowncomerIndex(aDC.Index, "L", bReturnSuppressed: true, bReturnClone: false);
                    }

                    if (dcOrds.Count > 0)
                    {

                        dcOrds.Sort();
                        //sort the left or right su lines by ordinate low to high and set the suppressed value to true and set the mark to false
                        //alos remove any duplicates
                        zSort(dcLines, mzSortOrders.LowToHigh, true, true);

                        //loop on the dc startup site ordinates and unsupress the dc left/right members that contain the ordinate
                        for (int j = 1; j <= dcOrds.Count; j++)
                        {
                            y1 = dcOrds[j - 1];

                            aMem = dcLines.Find(x => x.CrossesY(y1) == true);
                            if (aMem != null)
                            {
                                aMem.Suppressed = false;
                                tY = aMem.SPT.Y;
                                bY = aMem.EPT.Y;
                                if (y1 > aMem.sp.Y) aMem.sp.Y = y1;
                                if (y1 < aMem.ep.Y) aMem.ep.Y = y1;
                                aMem.ReferencePt.Y = y1;
                                aMem.MinLength = aMinLength;

                            }
                        }
                    } //search for unsupressed y ordinates

                    // set the min length and save the results saving the highest ones first
                    for (int j = dcLines.Count - 1; j >= 0; j += -1)
                    {
                        aMem = dcLines[j];
                        aMem.MinLength = aMinLength;
                        _rVal.Add(new mdStartupLine(aMem));
                    }
                } //left right loop
            } //dc loop


            //strectch the left and right spout group lines if one is suppressed and the other is not 
            for (int i = 1; i <= aSpoutGroups.Count; i++)
            {
                mdSpoutGroup aSG = aSpoutGroups.Item(i);
                if (aSG.IsVirtual) continue;

                List<mdStartupLine> sgLines = _rVal.FindAll((x) => string.Compare(x.SpoutGroupHandle, aSG.Handle, true) == 0);

                mdStartupLine UL = sgLines.Find((x) => x.Tag == "UL");
                mdStartupLine LL = sgLines.Find((x) => x.Tag == "LL");
                if (UL != null && LL != null)
                {
                    if (!UL.Suppressed && LL.Suppressed)
                    {
                        UL.EPT = LL.EPT;
                    }
                    else if (UL.Suppressed && !LL.Suppressed)
                    {
                        LL.SPT = UL.SPT;
                    }
                }

                mdStartupLine UR = sgLines.Find((x) => x.Tag == "UR");
                mdStartupLine LR = sgLines.Find((x) => x.Tag == "LR");
                if (UR != null && LR != null)
                {
                    if (!UR.Suppressed && LR.Suppressed)
                    {
                        UR.EPT = LR.EPT;
                    }
                    else if (UR.Suppressed && !LR.Suppressed)
                    {
                        LR.SPT = UR.SPT;
                    }
                }
            }


            return _rVal;
        }

        /// <summary>
        ///returns the mdStartupLines members on the startups
        /// </summary>
        public mdStartupLines ToggleByStartups(colMDSpoutGroups aSpoutGroups, mdStartupSpouts aStartups)
        {
            mdStartupLines _rVal = new mdStartupLines();

            for (int i = 1; i <= aSpoutGroups.Count; i++)
            {
                mdSpoutGroup aSG = aSpoutGroups.Item(i);
                if (!aSG.IsVirtual)
                {
                    List<mdStartupLine> sgLines = FindAll((x) => string.Compare(x.SpoutGroupHandle, aSG.Handle, true) == 0);
                    List<mdStartupSpout> sgStartups = aStartups.FindAll((x) => string.Compare(x.SpoutGroupHandle, aSG.Handle, true) == 0);
                    mdStartupLine UL = sgLines.Find((x) => x.Tag == "UL");
                    mdStartupLine LL = sgLines.Find((x) => x.Tag == "LL");
                    if (UL != null) UL.Suppressed = mdStartUps.GetTaggedSupression(sgStartups, UL.Tag, UL.Suppressed);

                    if (LL != null) LL.Suppressed = mdStartUps.GetTaggedSupression(sgStartups, LL.Tag, LL.Suppressed);

                    if (UL != null && LL != null)
                    {
                        if (!UL.Suppressed && LL.Suppressed)
                        { UL.EPT = LL.EPT; }
                        else if (UL.Suppressed && !LL.Suppressed)
                        { LL.SPT = UL.SPT; }
                    }
                    mdStartupLine UR = sgLines.Find((x) => x.Tag == "UR");
                    mdStartupLine LR = sgLines.Find((x) => x.Tag == "LR");
                    if (UR != null) UR.Suppressed = mdStartUps.GetTaggedSupression(sgStartups, UR.Tag, UR.Suppressed);

                    if (LR != null) LR.Suppressed = mdStartUps.GetTaggedSupression(sgStartups, LR.Tag, LR.Suppressed);

                    if (UR != null && LR != null)
                    {
                        if (!UR.Suppressed && LR.Suppressed)
                        { UR.EPT = LR.EPT; }
                        else if (UR.Suppressed && !LR.Suppressed)
                        { LR.SPT = UR.SPT; }
                    }
                    _rVal.Add(UL);
                    _rVal.Add(LL);
                    _rVal.Add(UR);
                    _rVal.Add(LR);

                }
            }


            return _rVal;
        }
    }
}
