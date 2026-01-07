using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Materials;
using UOP.WinTray.Projects.Parts;

using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;

namespace UOP.WinTray.Projects
{
    /// <summary>
    /// Collection of Tray Ranges
    /// </summary>
    public class colUOPTrayRanges : uopParts, IEnumerable<uopTrayRange>, IDisposable
    {
        public override uppPartTypes BasePartType => uppPartTypes.TrayRanges;
        #region Events

        public delegate void RangeAddedHandler();
        public event RangeAddedHandler eventRangeAdded;
        public delegate void RangeRemovedHandler(uopTrayRange aRange);
        public event RangeRemovedHandler eventRangeRemoved;

        #endregion


        #region Constructors

        public colUOPTrayRanges() : base(uppPartTypes.TrayRanges, uppProjectFamilies.uopFamMD, bBaseOne: true, bMaintainIndices: true, bInvalidWhenEmpty: true) { _ColumnLetter = string.Empty; }

        internal colUOPTrayRanges(colUOPTrayRanges aPartToCopy, uopPart aParent = null, bool bDontCopyMembers = false) : base((uopParts)aPartToCopy, bDontCopyMembers, aParent)
        {
            if (aPartToCopy == null) return;

            TraySortOrder = aPartToCopy.TraySortOrder;
            _ColumnLetter = aPartToCopy.ColumnLetter;
        }

        #endregion


        #region IEnumerable Implementation

        public new IEnumerator<uopTrayRange> GetEnumerator() { foreach (uopPart part in Members) { yield return (uopTrayRange)part; } }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion IEnumerable Implementation

        /// <summary>
        /// used by objects above this object in the object model to alert a sub object that a property above it in the object model has changed.
        /// alerts the objects below it of the change
        /// </summary>
        /// <param name="As"></param>
        /// <param name=""></param>
        public void Alert(uopProperty aProperty)
        {
            if (aProperty == null) return;
            if (aProperty.Name.ToUpper().Equals("REVERSESORT"))
            {
                SortByRingStart();
                return;
            }
            uopTrayRange aMem;

            for (int i = 1; i <= Count; i++)
            {
                aMem = (uopTrayRange)base.Item(i);
                aMem.Alert(aProperty);
            }
        }
        /// <summary>
        /// ^the controls the which tray is the top tray in the columns Ranges
        /// </summary>
        private uppTraySortOrders _TraySortOrder = uppTraySortOrders.TopToBottom;
        public uppTraySortOrders TraySortOrder
        {
            get => _TraySortOrder;
            internal set
            {
                if (_TraySortOrder == value) return;
                _TraySortOrder = value;
                SortByRingStart();
            }
        }

        public colUOPTrayRanges Clone() => new colUOPTrayRanges(this);

        public override uopPart Clone(bool aFlag = false) => (uopPart)this.Clone();

        public new List<uopTrayRange> ToList(bool bGetClones = false)
        {

            List<uopTrayRange> _rVal = new List<uopTrayRange>();

            foreach (var part in CollectionObj)
            {
                if (!bGetClones)
                    _rVal.Add((uopTrayRange)part);
                else
                    _rVal.Add((uopTrayRange)part.Clone());
            }
            return _rVal;
        }
        public List<string> GUIDS
        {
            get
            {
                List<string> _rVal = new List<string>();
                uopTrayRange aMem;

                for (int i = 1; i <= Count; i++)
                {
                    aMem = aMem = (uopTrayRange)base.Item(i);
                    _rVal.Add(aMem.GUID);
                }
                return _rVal;
            }
        }
        /// <summary>
        /// Returns True if any of the tray ranges in the column have anti-penetration pans defined
        /// </summary>
        public bool HasAntiPenetrationPans
        {
            get
            {

                uopTrayRange aMem;
                for (int i = 1; i <= Count; i++)
                { aMem = (uopTrayRange)base.Item(i); if (aMem.HasAntiPenetrationPans) return true; }

                return false;
            }
        }
        /// <summary>
        /// Returns a comma delimited string with the unique manway counts of the ranges in the collection
        /// </summary>
        public string ManwayCountList
        {
            get
            {
                string _rVal = string.Empty;
                List<string> tSpaces = new List<string>();
                bool bAddIt;
                string tSpace;
                uopTrayRange aMem;
                uopTrayAssembly assy;

                for (int j = 1; j <= Count; j++)
                {
                    aMem = (uopTrayRange)base.Item(j);
                    assy = aMem.GetTrayAssembly();
                    tSpace = (assy != null) ? Convert.ToString(assy?.ManwayCount) : "0";

                    bAddIt = true;
                    for (int i = 0; i < tSpaces.Count; i++)
                    {
                        if (string.Compare(tSpace, Convert.ToString(tSpaces[i]), true) == 0)
                        {
                            bAddIt = false;
                            break;
                        }
                    }
                    if (bAddIt)
                    {
                        tSpaces.Add(tSpace);
                        _rVal = (_rVal == string.Empty) ? tSpace : _rVal + ", " + tSpace;
                    }



                }
                return _rVal;
            }
        }
        /// <summary>
        ///Returns a collection of the guid strings of all the members 
        /// </summary>
        /// <param name="bIncludeDesignIndicator"></param>
        /// <param name="rSelectedName"></param>
        /// <returns></returns>
        public List<string> Names(bool bIncludeDesignIndicator, out string rSelectedName)
        {
            List<string> _rVal = new List<string>();
            rSelectedName = string.Empty;
            uopTrayRange aMem;
            string aName;

            for (int i = 1; i <= Count; i++)
            {
                aMem = (uopTrayRange)base.Item(i);
                aName = aMem.Name(bIncludeDesignIndicator);
                _rVal.Add(aName);
                if (aMem.Selected)
                {
                    if (rSelectedName == string.Empty) rSelectedName = aName;

                }
            }
            return _rVal;
        }

        public override string ProjectHandle
        {
            get => base.ProjectHandle;

            set
            {
                if (base.ProjectHandle != value)
                {
                    base.ProjectHandle = value;
                    uopTrayRange aMem;

                    for (int i = 1; i <= Count; i++)
                    {
                        aMem = (uopTrayRange)base.Item(i);
                        aMem.SubPart(this);
                        SetItem(i, aMem);
                    }
                }
            }
        }


        public bool ReverseSort => TraySortOrder == uppTraySortOrders.BottomToTop;

        /// <summary>
        /// Returns the properties required to save all the ranges in the collection to file
        /// </summary>
        public override uopPropertyArray SaveProperties(string aHeading = null)
        {

            uopPropertyArray _rVal = new uopPropertyArray();
            uopTrayRange aMem;

            for (int i = 1; i <= Count; i++)
            {
                aMem = (uopTrayRange)base.Item(i);
                _rVal.Append(aMem.SaveProperties());
            }

            return _rVal;


        }
        /// <summary>
        /// Returns the tray marked as selected from the collection
        ///if none are marked as selected the first range is returned
        /// </summary>
        public uopTrayRange SelectedRange
        {
            get => (uopTrayRange)base.SelectedMember;

            set
            {
                if (value == null && Count > 0) value = (uopTrayRange)base.Item(1);
                if (value == null) return;

                uopTrayRange aMem = GetByGuid(value.GUID);
                int idx = (aMem != null) ? aMem.Index : 1;
                base.SetSelected(idx);
            }
        }
        /// <summary>
        /// Returns the tray marked as selected from the collection
        ///if none are marked as selected the first range is returned
        /// </summary>
        public int SelectedRangeIndex
        {
            get
            {

                uopTrayRange aMem = (uopTrayRange)base.SelectedMember;
                return (aMem != null) ? aMem.Index : 0;
            }
        }

        /// <summary>
        /// Returns the tray marked as selected from the collection
        ///if none are marked as selected the first range is returned
        /// </summary>
        public string SelectedRangeGUID
        {
            get
            {

                uopTrayRange aMem = (uopTrayRange)base.SelectedMember;
                return (aMem != null) ? aMem.GUID : "";
            }

            set
            {


                uopTrayRange mem = (uopTrayRange)CollectionObj.Find(x => x.RangeGUID == value);
                if (mem != null) SetSelected(mem.Index);
            }

        }

        public string SelectedRangeName
        {
            get
            {

                uopTrayRange aMem = (uopTrayRange)base.SelectedMember;
                return (aMem != null) ? aMem.SelectName : "";
            }

            set
            {
                List<uopTrayRange> mems = ToList();

                uopTrayRange mem = mems.Find(x => x.Name(true) == value);
                if (mem != null) SetSelected(mem.Index);

            }

        }

        public Collection<uopTrayRange> ToCollection
        {
            get
            {
                Collection<uopTrayRange> _rVal = new Collection<uopTrayRange>();
                for (int i = 1; i <= Count; i++) { _rVal.Add(Item(i)); }
                return _rVal;
            }
        }

        public List<string> TrayNames(bool bIncludeDesignIndicator, string aGUIDToOmit = "")
        {
            List<String> _rVal = new List<string>();
            uopTrayRange mem;
            if (aGUIDToOmit == null) aGUIDToOmit = string.Empty;

            for (int i = 1; i <= Count; i++)
            {
                mem = Item(i);
                if (string.IsNullOrWhiteSpace(aGUIDToOmit) || (!string.IsNullOrWhiteSpace(aGUIDToOmit) && mem.GUID != aGUIDToOmit))
                {
                    _rVal.Add(mem.Name(bIncludeDesignIndicator));
                }
            }
            return _rVal;
        }

        /// <summary>
        /// The total number of trays
        /// </summary>
        public new int TotalTrayCount
        {
            get
            {
                int _rVal = 0;
                uopTrayRange aMem;
                for (int i = 1; i <= Count; i++)
                {
                    aMem = (uopTrayRange)base.Item(i);
                    _rVal += aMem.TrayCount;
                }
                return _rVal;
            }
        }
        /// <summary>
        /// Returns a comma delimited string with the unique tray type names in the collection
        /// </summary>
        public string TrayTypeNames
        {
            get
            {

                List<string> tNames = new List<string>();
                bool bAddIt;
                uopTrayRange aMem;
                string tname;
                string _rVal = string.Empty;

                for (int j = 1; j <= Count; j++)
                {
                    aMem = (uopTrayRange)base.Item(j);
                    tname = aMem.TrayTypeName;
                    bAddIt = true;
                    for (int i = 0; i < tNames.Count; i++)
                    {
                        if (String.Compare(tname, tNames[i].ToString(), true) == 0)
                        {
                            bAddIt = false;
                            break;
                        }
                    }
                    if (bAddIt)
                    {
                        tNames.Add(tname);
                        if (_rVal == string.Empty)
                        { _rVal = tname; }
                        else
                        { _rVal += ", " + tname; }
                    }

                }
                return _rVal;
            }
        }



        private string _ColumnLetter;
        public override string ColumnLetter
        {
            get => _ColumnLetter;
            set
            {
                value ??= string.Empty;
                _ColumnLetter ??= string.Empty;
                value = value.Trim();

                if (_ColumnLetter == value) return;
                _ColumnLetter = value;
                foreach (uopTrayRange item in this)
                {
                    item.ColumnLetter = _ColumnLetter;
                }

            }
        }
        #region Methods

        public uopTrayRange LastTray() => (uopTrayRange)base.LastItem();

        public uopTrayRange FirstTray() => (uopTrayRange)base.FirstItem();

        public uopDocuments GenerateWarnings(uopProject aProject, string aCategory = null, uopDocuments aCollector = null)
        {
            uopDocuments _rVal = aCollector ?? new uopDocuments();
            uopTrayRange aMem;
            aProject ??= Project;

            for (int i = 1; i <= Count; i++)
            {
                aMem = (uopTrayRange)base.Item(i);
                aMem.GenerateWarnings(aProject, aCategory, _rVal);
            }
            return _rVal;

        }

        public List<double> Diameters(uppMDDesigns aDesignFamily = uppMDDesigns.Undefined)
        {
            List<double> _rVal = new List<double>();
            if (Count <= 0) return _rVal;
            foreach (var item in CollectionObj)
            {
                if (aDesignFamily == uppMDDesigns.Undefined || item.DesignFamily == aDesignFamily)
                {
                    if (_rVal.FindIndex(x => x == item.ShellID) < 0) _rVal.Add(item.ShellID);
                }

            }
            return _rVal;
        }

        public List<uppMDDesigns> DesignFamilies(bool bIncludebasics = false)
        {
            List<uppMDDesigns> _rVal = new List<uppMDDesigns>();
            if (Count <= 0) return _rVal;
            foreach (var item in CollectionObj)
            {
                if (_rVal.FindIndex(x => x == item.DesignFamily) < 0) _rVal.Add(item.DesignFamily);
            }

            if (bIncludebasics)
            {
                if (_rVal.FindIndex(x => x == uppMDDesigns.MDDesign) < 0) _rVal.Add(uppMDDesigns.MDDesign);
                if (_rVal.FindIndex(x => x == uppMDDesigns.MDBeamDesign) < 0) _rVal.Add(uppMDDesigns.MDBeamDesign);
                if (_rVal.FindIndex(x => x == uppMDDesigns.ECMDDesign) < 0) _rVal.Add(uppMDDesigns.ECMDDesign);
                if (_rVal.FindIndex(x => x == uppMDDesigns.ECMDBeamDesign) < 0) _rVal.Add(uppMDDesigns.ECMDBeamDesign);
            }

            return _rVal;
        }

        public override uopDocuments Warnings() => GenerateWarnings(null);

        /// <summary>
        ///#1the item to add to the collection
        ///^used to add an item to the collection
        ///~won't add "Nothing" (no error raised).
        ///~assigns the added ranges Column property to the collections parent Column object
        ///~to let the ranges have access to the parent Column properties
        /// </summary>
        /// <param name="aNewMember"></param>
        public override uopPart Add(uopPart aPart)
        {
            if (aPart == null) return null;
            if (aPart.PartType != uppPartTypes.TrayRange) return null;
            uopPart _rVal = null;
            try
            {

                _rVal = (uopPart)Add((uopTrayRange)aPart);



            }
            catch (Exception e) { throw e; }
            return _rVal;

        }


        /// <summary>
        ///#1the item to add to the collection
        ///^used to add an item to the collection
        ///~won't add "Nothing" (no error raised).
        ///~assigns the added ranges Column property to the collections parent Column object
        ///~to let the ranges have access to the parent Column properties
        /// </summary>
        /// <param name="aNewMember"></param>
        public uopTrayRange Add(uopTrayRange aNewMember)
        {
            if (aNewMember == null) return null;

            uopTrayRange _rVal = null;

            try
            {
                if (aNewMember.ProjectFamily != ProjectFamily) throw new Exception("[colUOPTrayRanges.AddRange] The Passed Tray Is Not Compatible With The Collections Current Tray Type");

                _rVal = (uopTrayRange)base.Add(aNewMember);



            }
            catch (Exception e) { _rVal = null; throw e; }
            if (_rVal != null) eventRangeAdded?.Invoke();
            return _rVal;

        }

        public List<mdDowncomer> Downcomers()
        {
            List<mdDowncomer> _rVal;
            uopTrayRange aMem;
            mdTrayAssembly aMDAssy;
            colMDDowncomers DCs;
            _rVal = new List<mdDowncomer>();
            if (ProjectFamily != uppProjectFamilies.uopFamMD) return _rVal;

            for (int i = 1; i <= Count; i++)
            {
                aMem = (uopTrayRange)base.Item(i);
                aMDAssy = aMem.GetMDTrayAssembly();
                DCs = aMDAssy.Downcomers;
                for (int j = 1; j <= DCs.Count; j++)
                { _rVal.Add(DCs.Item(j)); }
            }
            return _rVal;

        }


        public List<uopMaterial> GetMaterials(uppPartTypes aPartType, bool bUniqueOnly = true, Enum aSubPartType = null)
        {

            throw new NotImplementedException("GetMaterials is not implemented for colUOPTrayRanges. Use GetMaterials(uopPartTypes aPartType, bool bUniqueOnly = true, Enum aSubPartType = null) on the uopTrayRange object instead.");
            //List<uopMaterial> _rVal = new List<uopMaterial>();
            //uopTrayRange aMem;
            //List<uopPart> aParts;
            //uopPart aPart;

            //TMATERIAL tMat;
            //bool bKeep;
            //TMATERIALS aTable;
            //string sname;
            //string sGUID;
            //int si = 1;
            //int ei = Count;
            //TMATERIAL mat;
            //uopMaterial pMat;

            //aTable = new TMATERIALS(uppMaterialTypes.Undefined);

            //for (int i = si; i <= ei; i++)
            //{
            //    aMem = (uopTrayRange)base.Item(i);
            //    aParts = aMem.GetParts(aPartType, true);


            //    if (aParts != null)
            //    {
            //        sname = aMem.SpanName();
            //        sGUID = aMem.GUID;
            //        if (aSubPartType != null)
            //        {
            //            aParts = aParts.FindAll(x => x.SubPartType == aSubPartType);
            //        }



            //        for (int j = 0; j < aParts.Count; j++)
            //        {
            //            aPart = aParts[j];

            //            if (aPart != null)
            //            {

            //                pMat = aPart.Material;
            //                if (pMat != null)
            //                {
            //                    tMat = pMat.Structure;
            //                    tMat.SpanName = sname;
            //                    tMat.RangeGUID = sGUID;
            //                    bKeep = true;
            //                    if (bUniqueOnly)
            //                    {
            //                        for (int k = 1; k <= aTable.Count; k++)
            //                        {
            //                            if (aTable.Item(k).IsEqual(tMat))
            //                            {
            //                                bKeep = false;
            //                                mat = aTable.Item(k);
            //                                mzUtils.ListAdd(ref mat.SpanName, tMat.SpanName, bAllowNulls: false);
            //                                aTable.SetItem(k, mat);
            //                                break;
            //                            }
            //                        }
            //                    }
            //                    if (bKeep)
            //                    {
            //                        tMat.PartIndex = PartIndex;
            //                        aTable.Add(tMat);
            //                    }

            //                }

            //            }

            //            for (int k = 1; k <= aTable.Count; k++)
            //            {
            //                _rVal.Add(TMATERIAL.FromStructure(aTable.Item(k)));
            //            }
            //        }
            //    }
            //}
            //return _rVal;
        }


        public colUOPParts SpliceAngles(bool bStopAtOne = false, bool bExcludeManwaySplices = false, bool bUpdateQuantities = false)
        {
            colUOPParts _rVal = new colUOPParts();
            _rVal.SubPart(this);
            uopTrayRange aMem;
            colUOPParts MemAngs;

            for (int i = 1; i <= Count; i++)
            {
                aMem = (uopTrayRange)base.Item(i);
                MemAngs = aMem.SpliceAngles(bExcludeManwaySplices, bUpdateQuantities);
                if (MemAngs != null)
                {
                    _rVal.Append(MemAngs);
                    if (_rVal.Count > 0 && bStopAtOne) break;

                }
            }
            return _rVal;
        }


        public bool TryGet(object aIndexNameorGUID, out uopTrayRange rRange)
        {
            rRange = null;
            if (aIndexNameorGUID == null) return false;
            int idx = 0;
            if (aIndexNameorGUID is string)
            {
                string sval = (string)aIndexNameorGUID;
                if (string.IsNullOrWhiteSpace(sval)) return false;
                rRange = GetByName(sval, true);
                if (rRange != null) return true;
                rRange = GetByName(sval, false);
                if (rRange != null) return true;
                rRange = this.GetByGuid(sval);
                if (rRange != null) return true;
                if (mzUtils.IsNumeric(sval)) idx = mzUtils.VarToInteger(sval);
            }
            else if (mzUtils.IsNumeric(aIndexNameorGUID)) { idx = mzUtils.VarToInteger(aIndexNameorGUID); }
            if (idx > 0 && idx <= Count)
            {
                rRange = Item(idx);
            }
            return rRange != null;
        }

        /// <summary>
        ///  Searched TrayRange by GUID
        /// </summary>
        /// <param name="aGUID"></param>
        /// <param name="aIndex"></param>
        /// <returns></returns>
        public uopTrayRange GetByGuid(string aGUID)
        {


            if (string.IsNullOrWhiteSpace(aGUID)) { return null; } else { aGUID = aGUID.Trim(); }
            uopTrayRange _rVal = (uopTrayRange)CollectionObj.Find(x => x.RangeGUID == aGUID);
            if (_rVal != null) _rVal.Index = CollectionObj.IndexOf(_rVal) + 1;
            return _rVal;
        }


        /// <summary>
        /// #1the node name of the tray range to retrieve
        /// </summary>
        /// <param name="aName"></param>
        /// <param name="bIncludeDesignDescriptor"></param>
        /// <returns>Returns the tray from the collection that has a node name that matches the passed string</returns>
        public uopTrayRange GetByName(string aName, bool bIncludeDesignDescriptor = false)
        {
            if (string.IsNullOrWhiteSpace(aName)) { return null; } else { aName = aName.Trim(); }
            uopTrayRange _rVal = (uopTrayRange)CollectionObj.Find(x => x.TrayName(false).ToUpper() == aName.ToUpper() || x.TrayName(true).ToUpper() == aName.ToUpper());
            return _rVal;
        }

        public uopTrayRange Find(Predicate<uopTrayRange> match, bool bGetClones = false)
        {

            return ToList(bGetClones).Find(match);
        }
        public List<uopTrayRange> FindAll(Predicate<uopTrayRange> match, bool bGetClones = false)
        {

            return ToList(bGetClones).FindAll(match);
        }

        /// <summary>
        /// Get By Shell ID
        /// </summary>
        /// <param name="sShellID"></param>
        /// <returns>returns the ranges in the collection that have shell ID equal to the passed value</returns>
        public new List<uopTrayRange> GetByShellID(double sShellID)
        {
            List<uopTrayRange> _rVal = null;
            uopTrayRange aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = (uopTrayRange)base.Item(i);
                if (Math.Round(Math.Abs(aMem.ShellID - sShellID), 3) == 0) _rVal.Add(aMem);

            }

            return _rVal;
        }


        public List<uopTrayRange> GetRanges(int iRangeStart, int iRangeEnd)
        {
            List<uopTrayRange> _rVal = new List<uopTrayRange>();
            int rs = iRangeStart;
            int re = iRangeEnd;

            rs = mzUtils.LimitedValue(rs, 1, Count);
            re = mzUtils.LimitedValue(re, 1, Count);

            mzUtils.SortTwoValues(true, ref rs, ref re);

            for (int i = rs; i <= re; i++)
            {
                _rVal.Add((uopTrayRange)base.Item(i));
            }
            return _rVal;
        }



        /// <summary>
        /// Get Ranges With Anti Penetration
        /// </summary>
        /// <returns>returns the tray ranges in the collection that have anti-penetration pans defined</returns>
        public List<uopTrayRange> GetRangesWithAntiPenetration()
        {

            List<uopTrayRange> _rVal = new List<uopTrayRange>();
            uopTrayRange aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = (uopTrayRange)base.Item(i);
                if (aMem.HasAntiPenetrationPans) _rVal.Add(aMem);

            }
            return _rVal;
        }
        /// <summary>
        /// Gets the ranges with a requested flag that match the passed value
        /// </summary>
        /// <param name="aRequestedValue"></param>
        /// <returns></returns>
        public Dictionary<string, uopTrayRange> GetRequested(bool aRequestedValue, bool reversesort = false)
        {
            Dictionary<string, uopTrayRange> _rVal = new Dictionary<string, uopTrayRange>();
            uopTrayRange aMem;
            if (!reversesort)
            {
                for (int i = 1; i <= Count; i++)
                {
                    aMem = (uopTrayRange)base.Item(i);
                    if (aMem.Requested == aRequestedValue) _rVal.Add(aMem.GUID, aMem);

                }
            }
            else
            {
                for (int i = Count; i >= 1; i--)
                {
                    aMem = (uopTrayRange)base.Item(i);
                    if (aMem.Requested == aRequestedValue) _rVal.Add(aMem.GUID, aMem);

                }
            }


            return _rVal;
        }
        /// <summary>
        /// ^returns a comma deliminated string conting the ring numbers that are currently
        ///^ocupied by a tray range
        ///~like 1,2,3,5,7,9,4,6,8, used to determine if a ring is in use
        /// </summary>
        /// <returns></returns>
        public string GetRingString()
        {
            string _rVal = string.Empty;
            string rStr = string.Empty;

            for (int i = 1; i <= Count; i++)
            {
                uopTrayRange aMem = (uopTrayRange)base.Item(i);
                rStr = aMem.RingString;
                if (rStr != string.Empty)
                {
                    if (_rVal != string.Empty) _rVal += ",";
                    _rVal += rStr;
                }

            }
            return _rVal;
        }


        /// <summary>
        /// Get SPRanges
        /// </summary>
        /// <returns>returns only the single pass tray ranges in the collection</returns>
        public List<uopTrayRange> GetSPRanges()
        {
            List<uopTrayRange> _rVal = new List<uopTrayRange>();

            for (int i = 1; i <= Count; i++)
            {
                uopTrayRange aMem = Item(i);
                if (aMem.SinglePass) _rVal.Add(aMem);

            }
            return _rVal;
        }

        /// <summary>
        /// Get Shell Diameters
        /// </summary>
        /// <returns>returns the collection of the unique shell diameters of the ranges in the collection</returns>
        public List<double> GetShellDiameters()
        {
            List<double> _rVal = new List<double>();
            double sVal = 0;
            bool bAddIt = false;
            uopTrayRange aMem;
            for (int j = 1; j <= Count; j++)
            {
                aMem = (uopTrayRange)base.Item(j);
                sVal = aMem.ShellID;
                bAddIt = true;
                for (int i = 0; i < _rVal.Count; i++)
                {
                    if ((double)_rVal[i] == sVal)
                    {
                        bAddIt = false;
                        break;
                    }
                }
                if (bAddIt) _rVal.Add(sVal);
            }
            return _rVal;
        }

        /// <summary>
        /// Get TPRanges
        /// </summary>
        /// <returns>returns only the two pass tray ranges in the collection</returns>
        public List<uopTrayRange> GetTPRanges()
        {
            List<uopTrayRange> _rVal = new List<uopTrayRange>();

            uopTrayRange aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                if (!aMem.SinglePass) _rVal.Add(aMem);

            }

            return _rVal;
        }

        /// <summary>
        /// Highest Used Ring
        /// </summary>
        /// <returns>returns the highest occupied ring number of all the ranges in the collection</returns>
        public int HighestUsedRing()
        {
            if (Count <= 0) { return 0; }
            string rStr = string.Empty;
            rStr = GetRingString();
            if (string.IsNullOrWhiteSpace(rStr)) { return 0; }

            int _rVal = 0;
            int Val;
            string vStr;

            string[] vals = rStr.Split(',');
            for (int i = 0; i < vals.Length; i++)
            {
                vStr = vals[i];
                Val = mzUtils.VarToInteger(vStr, aDefault: 0);
                if (Val > _rVal) { _rVal = Val; }
            }

            return _rVal;
        }



        public override uopPart Item(int aIndex) => (uopPart)this.Item(aIndex);


        /// <summary>
        ///returns the item from the collection at the requested index
        /// </summary>
        /// <param name="aIndexOrGUID"></param>
        /// <returns></returns>
        public uopTrayRange Item(dynamic aIndexOrGUID)
        {

            int idx = 0;
            if (aIndexOrGUID.GetType() == typeof(string))
            {
                uopTrayRange aMem = GetByGuid(Convert.ToString(aIndexOrGUID));
                if (aMem != null) idx = aMem.Index;
            }
            else if (aIndexOrGUID.GetType() == typeof(int))
            {
                idx = (int)aIndexOrGUID;
            }
            else
            {
                idx = mzUtils.VarToInteger(aIndexOrGUID);
            }

            uopTrayRange _rVal = null;

            //if (idx <= 0) idx = SelectedIndex;
            if (idx > 0 && idx <= Count)
            {
                _rVal = (uopTrayRange)base.Item(idx);

                _rVal.TraySortOrder = TraySortOrder;
                _rVal.ColumnLetter = ColumnLetter;
            }

            return _rVal;
        }

        /// <summary>
        /// called by members to alert the collection of a change to the member
        /// </summary>
        /// <param name="Member"></param>
        /// <param name="aProperty"></param>
        public void NotifyMemberChange(uopTrayRange Member, uopProperty aProperty)
        {
            if (Member == null) return;
            if (String.Compare(aProperty.Name, "RingStart", true) == 0) SortByRingStart();
            uopColumn aCol = Column;
            if (aCol != null) aCol.Notify(aProperty);

        }

        /// <summary>
        ///#1the starting ring number to test
        ///#2then ending ring number to test
        ///#3the stack pattern to apply
        ///#4a range to skip in the test
        ///^searchs for and///returns the first tray range that occupies a ring in the indicated range
        /// </summary>
        /// <param name="rStart"></param>
        /// <param name="rEnd"></param>
        /// <param name="StackPat"></param>
        /// <param name="RangeToSkip"></param>
        /// <returns></returns>
        public uopTrayRange RangeIsOccupied(int rStart, int rEnd, uppStackPatterns StackPat, uopTrayRange RangeToSkip = null)
        {

            string rngstr = string.Empty;
            string txt = string.Empty;
            int Rng = 0;
            int aStep = (StackPat != uppStackPatterns.Continuous) ? 2 : 1;
            uopTrayRange aMem;
            if (rStart > rEnd) { Rng = rStart; rStart = rEnd; rEnd = Rng; }

            for (int i = 1; i <= Count; i++)
            {
                aMem = (uopTrayRange)base.Item(i);
                if (aMem != RangeToSkip)
                {
                    rngstr = aMem.RingString;
                    rngstr = "," + rngstr + ",";
                    Rng = rStart;

                    while (!(Rng > rEnd))
                    {
                        if (Rng <= rEnd)
                        {
                            txt = "," + Rng + ",";
                            if (rngstr.Contains(txt)) return aMem;

                        }
                        Rng += aStep;
                    }

                }
            }
            return null;
        }

        /// <summary>
        /// extracts the parts property values from the passed file array that was read from an INI style project file.
        /// </summary>
        /// <param name="aProject">The project requesting the read event</param>
        /// <param name="aFileProps">The property array containing the INI file properties or a subset. The Name of the array should contain the original file name.</param>
        /// <param name="ioWarnings">A collection to populate if errors or warnings are found during the property value extraction  </param>
        /// <param name="aFileVersion">The version of th efile being read. Supplied to account for backward compatibility</param>
        /// <param name="aFileSection">the INI file heading to search for the properties to extract </param>
        /// <param name="bIgnoreNotFound">A flag to ignore properties that exist on the part but were not found in the file properties</param>
        /// <param name="aAssy">An optional parent tray assembly for the part being read</param>
        /// <param name="aPartParameter">An optional parent part for the part being read</param>
        /// <param name="aSkipList">An optional list of property names to skip over during the read</param>
        public override void ReadProperties(uopProject aProject, uopPropertyArray aFileProps, ref uopDocuments ioWarnings, double aFileVersion, string aFileSection = null, bool bIgnoreNotFound = false, uopTrayAssembly aAssy = null, uopPart aPartParameter = null, List<string> aSkipList = null, Dictionary<string, string> EqualNames = null, uopProperties aProperties = null)
        {

            try
            {
                ioWarnings ??= new uopDocuments();
                aFileSection = string.IsNullOrWhiteSpace(aFileSection) ? INIPath : aFileSection.Trim();
                if (aFileProps == null) throw new Exception("The Passed File Property Array is Undefined");
                if (string.IsNullOrWhiteSpace(aFileSection)) throw new Exception("File Section is Undefined");
                CollectionObj.Clear();
                if (aProject != null) SubPart(aProject);
                uopColumn mycolumn = Column;

                if (ActiveProps.Count > 0)
                {
                    uopPropertyArray myprops = aFileProps.PropertiesStartingWith(aFileSection);
                    if (myprops.Count <= 0)
                    {
                        ioWarnings?.AddWarning(this, $"{PartName} Data Not Found", $"File '{aFileProps.Name}' Does Not Contain {aFileSection} Info!");
                    }
                    else
                    {
                        base.ReadProperties(aProject, myprops, ref ioWarnings, aFileVersion, aFileSection, bIgnoreNotFound, EqualNames: EqualNames);
                    }


                }

                int cnt = 1;
                for (int i = 1; i < 1000; i++)
                {

                    string FSec = $"{aFileSection}.RANGE({i})";
                    if (aFileProps.Contains(FSec))
                    {
                        if (ProjectFamily == uppProjectFamilies.uopFamMD)
                        {
                            mdTrayRange mdRange = new mdTrayRange();
                            mdRange.SubPart(this);
                            mdRange.Index = cnt;
                            cnt++;
                            mdRange.ReadProperties(aProject, aFileProps, ref ioWarnings, aFileVersion, FSec, bIgnoreNotFound);

                            CollectionObj.Add(mdRange);
                        }

                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                ioWarnings?.AddWarning(this, "Read Properties Error", e.Message);
            }
            finally
            {
                Reading = false;
                aProject?.ReadStatus("", 2);


            }
        }
        /// <summary>
        /// Removes the item from the collection at the requested index
        /// </summary>
        /// <param name="Index"></param>
        public override uopPart Remove(int aIndex)
        {

            uopTrayRange _rVal = (uopTrayRange)base.Remove(aIndex);
            if (_rVal == null) return null;
            ResetSubParts();
            eventRangeRemoved?.Invoke(_rVal);
            return _rVal;


        }

        public void ResetSubParts()
        {
            uopTrayRange aMem;
            for (int i = 1; i <= Count; i++)
            {
                try
                {
                    aMem = (uopTrayRange)base.Item(i);
                    aMem.ResetParts();
                }
                catch
                {
                }
            }
        }
        /// <summary>
        /// Returns the count of single pass tray ranges in the collection
        /// </summary>
        /// <returns></returns>
        public int SPRangeCount()
        {
            int _rVal = 0;
            uopTrayRange aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = (uopTrayRange)base.Item(i);
                if (aMem.SinglePass) _rVal += 1;

            }
            return _rVal;
        }


        /// <summary>
        /// Returns a comma delimited string with the unique shell Ids of the ranges in the collection
        /// </summary>
        /// <param name="aMultiplier"></param>
        /// <returns></returns>
        public string ShellIDList(double aMultiplier)
        {

            List<string> tSpaces = new List<string>();
            bool bAddIt;
            string tSpace;
            string _rVal = string.Empty;

            if (aMultiplier <= 0) aMultiplier = 11;


            uopTrayRange aMem;
            for (int j = 1; j <= Count; j++)
            {
                aMem = (uopTrayRange)base.Item(j);
                tSpace = String.Format("{0:0.0000}", aMem.ShellID * aMultiplier);

                bAddIt = true;
                for (int i = 0; i < tSpaces.Count; i++)
                {
                    if (String.Compare(tSpace, tSpaces[i].ToString(), true) == 0)
                    {
                        bAddIt = false;
                        break;
                    }
                }
                if (bAddIt)
                {
                    tSpaces.Add(tSpace);
                    _rVal = (_rVal == string.Empty) ? tSpace : _rVal + ", " + tSpace;

                }
            }
            return _rVal;
        }

        public override void SubPart(uopColumn aColumn, string aCategory = null, bool? bHidden = null)
        {
            base.SubPart(aColumn, aCategory, bHidden);
            TraySortOrder = aColumn.TraySortOrder;
            ColumnLetter = aColumn.ColumnLetter;


            // TPART prt = aPart.Structure;
            // string cat = aCategory;
            // dynamic hid = bHidden;
            //base.SubPart(prt, cat);



        }
        public void SortByRingStart()
        {
            try
            {
                if (Count <= 1) return;


                List<dynamic> vStarts = new List<dynamic>();
                List<int> vIDs = null;
                List<uopPart> newCol = new List<uopPart>();
                uopTrayRange aMem;
                int idx = 0;
                bool revsort = ReverseSort;
                bool bRaiseIt = false;
                int cidx = ColumnIndex;

                for (int i = 1; i <= Count; i++)
                {
                    aMem = (uopTrayRange)base.Item(i);
                    vStarts.Add(aMem.RingStart);
                }

                vIDs = mzUtils.SortDynamicList(vStarts, revsort, bBaseOne: true);

                for (int i = 0; i < vIDs.Count; i++)
                {
                    idx = vIDs[i];
                    if (idx != (i + 1)) bRaiseIt = true;
                    aMem = (uopTrayRange)base.Item(idx);
                    aMem.ColumnIndex = cidx;
                    newCol.Add(aMem);
                }

                base.SetMembers(newCol);

                try
                {
                    if (bRaiseIt && !SuppressEvents)
                    {
                        Column?.Notify(uopProperty.Quick("TraySortOrder", "New Order", "Old Order", this));
                    }
                }
                catch
                {
                }
            }
            catch (Exception exception)
            {
                LoggerManager log = new LoggerManager();
                log.LogError(DateTime.Now + " " + exception.Message);
            }
        }

        /// <summary>
        /// TP Range Count
        /// </summary>
        /// <returns>Returns the count of two pass tray ranges in the collection</returns>
        public int TPRangeCount()
        {
            int _rVal = 0;
            uopTrayRange aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = (uopTrayRange)base.Item(i);
                if (!aMem.SinglePass) _rVal += 1;

            }
            return _rVal;
        }
        /// <summary>
        /// Tray Count
        /// </summary>
        /// <param name="aDesignFam"></param>
        /// <returns>returns the number of trays with the passed MD design family</returns>
        public new int TrayCount(uppMDDesigns aDesignFam = uppMDDesigns.Undefined)
        {
            int _rVal = 0;
            uopTrayRange aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = (uopTrayRange)base.Item(i);
                if (aDesignFam == uppMDDesigns.Undefined || aMem.DesignFamily == aDesignFam)
                {
                    _rVal += aMem.TrayCount;
                }
            }
            return _rVal;
        }
        /// <summary>
        /// Tray Spacing List
        /// </summary>
        /// <param name="aMultiplier"></param>
        /// <returns>returns a comma delimited string with the unique tray spacings of the ranges in the collection</returns>
        public string TraySpacingList(double aMultiplier = 0)
        {
            List<string> tSpaces = new List<string>();
            bool bAddIt;
            uopTrayRange aMem;
            string tSpace;
            string _rVal = string.Empty;

            if (aMultiplier <= 0)
            {
                aMultiplier = 1;
            }

            for (int j = 1; j <= Count; j++)
            {
                aMem = (uopTrayRange)base.Item(j);
                tSpace = String.Format("{0:0.0000}", Convert.ToString(aMem.RingSpacing * aMultiplier));

                bAddIt = true;
                for (int i = 0; i < tSpaces.Count; i++)
                {
                    if (string.Compare(tSpace, tSpaces[i].ToString(), true) == 0)
                    {
                        bAddIt = false;
                        break;
                    }
                }
                if (bAddIt)
                {
                    tSpaces.Add(tSpace);
                    if (_rVal == string.Empty)
                    { _rVal = tSpace; }
                    else
                    { _rVal = _rVal + ", " + tSpace; }

                }
            }
            return _rVal;
        }

        public bool UniformDeckMaterial(bool bIncludeGage = false)
        {
            if (Count <= 1) return true;

            bool _rVal = true;

            uopTrayRange aMem;
            uopTrayRange bMem = null;
            uopSheetMetal aMAT = null;
            uopSheetMetal bMat = null;
            aMem = (uopTrayRange)base.Item(1);
            aMAT = aMem.DeckMaterial;

            for (int i = 2; i <= Count; i++)
            {
                bMem = (uopTrayRange)base.Item(i);
                bMat = bMem.DeckMaterial;
                if (aMAT.Family != bMat.Family)
                {
                    _rVal = false;
                    break;
                }
                else
                {
                    if (bIncludeGage)
                    {
                        if (aMAT.SheetGage != bMat.SheetGage)
                        {
                            _rVal = false;
                            break;
                        }
                    }
                }
            }
            return _rVal;
        }
        public override void UpdatePartProperties()
        {
            Quantity = Count;
            uopTrayRange aMember;
            string pPath = PartPath();

            for (int i = 1; i <= Count; i++)
            {
                aMember = (uopTrayRange)base.Item(i);
                aMember.ParentPath = pPath;
                SubPartProperties();
            }
        }

        public override void UpdatePartWeight()
        {
            double wt = 0;
            Weight = 0;
            uopTrayRange aRange = null;
            for (int i = 1; i <= Count; i++)
            {
                aRange = (uopTrayRange)base.Item(i);
                aRange.UpdatePartWeight();
                wt += aRange.Weight;
            }
            base.Weight = wt;

        }

        public override void Destroy()
        {
            base.Destroy();
            base.Dispose(true);
        }

        /// <summary>
        /// make sure all persistent sub parts are up to date
        /// </summary>
        public void UpdatePersistentSubParts(uopProject aProject, bool bForceRegen = false, string aRangeGUID = null)
        {
            aProject ??= Project;
            foreach (var item in this)
            {
                if (!string.IsNullOrWhiteSpace(aRangeGUID))
                {
                    if (string.Compare(aRangeGUID, item.GUID, true) != 0) continue;
                }
                item.UpdatePersistentSubParts(aProject, bForceRegen);
            }

        }
        #endregion Methods


        #region Shared Methods

        /// <summary>
        /// Tray Count
        /// </summary>
        /// <param name="aDesignFam"></param>
        /// <returns>returns the number of trays with the passed MD design family</returns>
        public static int TablulateTrayCount(List<uopTrayRange> aRanges, uppMDDesigns aDesignFam = uppMDDesigns.Undefined)
        {
            if (aRanges == null) return 0;

            int _rVal = 0;
            uopTrayRange aMem;
            for (int i = 0; i < aRanges.Count; i++)
            {
                aMem = aRanges[i];
                if (aDesignFam == uppMDDesigns.Undefined || aMem.DesignFamily == aDesignFam)
                {
                    _rVal += aMem.TrayCount;
                }
            }
            return _rVal;
        }




        public static int GetTotalTrayCount(List<uopTrayRange> aRanges)
        {
            if (aRanges == null) return 0;
            int _rVal = 0;
            foreach (var item in aRanges)
            {
                _rVal += item.TrayCount;
            }

            return _rVal;
        }

        #endregion  Shared Methods
    }
}