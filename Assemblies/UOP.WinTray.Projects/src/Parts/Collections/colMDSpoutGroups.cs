using UOP.DXFGraphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Parts;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using static System.Math;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;

namespace UOP.WinTray.Projects.Parts
{
    /// <summary>
    /// a collection of mDSpoutGroup objects
    /// </summary>
    /// 
    public class colMDSpoutGroups : uopParts, IEnumerable<mdSpoutGroup>
    {
        public override uppPartTypes BasePartType => uppPartTypes.SpoutGroup;

        public delegate void SpoutGroupsGenerationEventHandler(bool aBegin, string aHandle);
        public event SpoutGroupsGenerationEventHandler eventSpoutGroupsGenerationEvent;
        //public delegate void SpoutGroupMemberChangedHandler(uopProperty aProperty);
        //public event SpoutGroupMemberChangedHandler eventSpoutGroupMemberChanged;
        public delegate void SpoutGroupsInvalidatedHandler();
        public event SpoutGroupsInvalidatedHandler eventSpoutGroupsInvalidated;


        #region Constructors

        public colMDSpoutGroups() : base(uppPartTypes.SpoutGroups, uppProjectFamilies.uopFamMD, bBaseOne: true, bMaintainIndices: true, true) { EventValidityChange += _BaseValidityChange; }

        internal colMDSpoutGroups(colMDSpoutGroups aPartToCopy, uopPart aParent = null, bool bDontCopyMembers = false) : base(aPartToCopy, bDontCopyMembers, aParent) { }

        #endregion

        #region IEnumerable Implementation

        public new IEnumerator<mdSpoutGroup> GetEnumerator() { foreach (uopPart part in Members) { yield return (mdSpoutGroup)part; } }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion IEnumerable Implementation

        #region Properties

        public double MaxPatternLength
        {
            get
            {
                double _rVal = 0;
                mdSpoutGroup aMem;

                for (int i = 1; i <= Count; i++)
                {
                    aMem = (mdSpoutGroup)base.Item(i);
                    if (aMem.IsVirtual) continue;
                    if (aMem.SpoutCount() > 0)
                    {
                        if (!aMem.LimitedBounds && aMem.GroupIndex > 1)
                        {

                            if (aMem.PatternLength > _rVal) _rVal = aMem.PatternLength;

                        }
                    }
                }
                return _rVal;

            }
        }

        public List<mdSpoutGroup> FieldGroups
        {
            get
            {
                List<mdSpoutGroup> _rVal = new List<mdSpoutGroup>();
                mdSpoutGroup aMem;

                for (int i = 1; i <= Count; i++)
                {
                    aMem = (mdSpoutGroup)base.Item(i);
                    if (aMem.IsVirtual) continue;
                    if (aMem.SpoutCount() > 0)
                    {
                        if (!aMem.LimitedBounds && aMem.GroupIndex > 1)
                        {

                            _rVal.Add(aMem);

                        }
                    }
                }
                return _rVal;

            }
        }

        public List<mdSpoutGroup> EndGroups
        {
            get
            {
                List<mdSpoutGroup> _rVal = new List<mdSpoutGroup>();
                mdSpoutGroup aMem;

                for (int i = 1; i <= Count; i++)
                {
                    aMem = (mdSpoutGroup)base.Item(i);
                    if (aMem.IsVirtual) continue;
                    if (aMem.SpoutCount() > 0)
                    {
                        if (aMem.LimitedBounds || aMem.GroupIndex > 1)
                        {

                            _rVal.Add(aMem);

                        }
                    }
                }
                return _rVal;

            }
        }

        /// <summary>
        /// the  number of spout groups for the whole tray
        /// </summary>
        public int CountPerTray
        {
            get
            {
                int _rVal = 0;
                mdSpoutGroup aMem;
                for (int i = 1; i <= Count; i++)
                {
                    aMem = (mdSpoutGroup)base.Item(i);
                    _rVal += aMem.CountPerTray;

                }
                return _rVal;
            }
        }

        /// <summary>
        /// the Errors that are available for this part
        /// </summary>
        public string Errors
        {
            get
            {
                string _rVal = string.Empty;
                mdSpoutGroup aMem;
                mdTrayAssembly aAssy = null;
                colMDConstraints Cns = null;
                aAssy = GetMDTrayAssembly();
                Cns = aAssy.Constraints;
                for (int i = 1; i <= Count; i++)
                {
                    aMem = (mdSpoutGroup)base.Item(i);
                    _rVal += aMem.ErrorPercentage.ToString("0.000");
                    if (i < Count) _rVal += ",";

                }
                return _rVal;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value) || value.IndexOf(",") == -1) return;

                value = value.Trim();
                int idx = 0;
                string[] vals = value.Split(',');
                mdSpoutGroup aMem;
                for (int i = 0; i < vals.Count(); i++)
                {
                    idx = i;
                    if (idx >= 0 & idx < Count)
                    {
                        aMem = (mdSpoutGroup)base.Item(i);
                        aMem.PropValSet("ErrorPercentage", mzUtils.VarToDouble(vals[i]), bSuppressEvnts: true);
                    }
                }
            }
        }

        /// <summary>
        ///^returns True if any members 'HasChanged' is currently true
        /// </summary>
        public bool HasChanged
        {
            get
            {
                mdSpoutGroup aMem;
                for (int i = 1; i <= Count; i++)
                {
                    aMem = (mdSpoutGroup)base.Item(i);
                    if (aMem.HasChanged) return true;
                }
                return false;
            }
            set
            {
                mdSpoutGroup aMem;
                for (int i = 1; i <= Count; i++)
                {
                    aMem = (mdSpoutGroup)base.Item(i);
                    aMem.HasChanged = value;
                    base.SetItem(i, aMem);
                }

                if (!value) { Invalid = false; }

            }
        }

        /// <summary>
        ///^returns the iNIPath that are available for this part
        /// </summary>
        public override string INIPath => $"COLUMN({ColumnIndex}).RANGE({RangeIndex}).TRAYASSEMBLY.SPOUTGROUPS";

        /// <summary>
        ///flag indicating that something has changed and the collection needs to be regenerated
        /// </summary>
        public override bool Invalid
        {
            get => Count == 0 || base.Invalid;

            set
            {
                if (base.Invalid != value)
                {
                    base.Invalid = value;
                    if (value) 
                        eventSpoutGroupsInvalidated?.Invoke();
                }

            }
        }


        /// <summary>
        ///returns the group marked as selected from the collection
        /// </summary>
        public new mdSpoutGroup SelectedMember => (mdSpoutGroup)base.SelectedMember;


        /// <summary>
        ///returns the index of the group marked as selected from the collection
        ///if none are marked as selected the first group is selected
        /// </summary>
        public string SelectedGroupHandle
        {
            get
            {
                mdSpoutGroup aGroup = (mdSpoutGroup)base.SelectedMember;
                return (aGroup != null) ? aGroup.Handle : "";
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value)) return;
                mdSpoutGroup aMem = GetByHandle(value.Trim(), null, out int idx);

                if (idx >= 0) base.SetSelected(idx);


            }
        }

        /// <summary>
        /// the index of the group marked as selected from the collection
        ///if none are marked as selected the first group is selected
        /// </summary>
        public int SelectedGroupIndex { get => base.SelectedIndex; set => base.SelectedIndex = value; }


        /// <summary>
        ///returns the total area of all the spouts in all the groups in the collection
        /// </summary>
        public double TotSpoutArea
        {
            get
            {
                double _rVal = 0;
                foreach(mdSpoutGroup mem in this)
                {
                    if (!mem.IsVirtual)
                        _rVal += mem.ActualArea * mem.OccuranceFactor;
                    else
                        continue;
                    
                }
                return _rVal;
            }
        }

        /// <summary>
        ///returns the total area of the spout group perimter polygons
        /// </summary>
        public double TotalPerimeterArea
        {
            get
            {
                double _rVal = 0;
                mdSpoutGroup aMem;
                USHAPE perim;
                for (int i = 1; i <= Count; i++)
                {
                    aMem = (mdSpoutGroup)base.Item(i);
                    perim = aMem.PerimeterV;
                    if (perim.IsDefined) _rVal += perim.Limits.Area * aMem.OccuranceFactor;

                }
                return _rVal;

            }
        }

        #endregion Properties

        #region Methods


        public void _BaseValidityChange(bool bInvalidValue)
        {
            if (bInvalidValue)
            {
                eventSpoutGroupsInvalidated?.Invoke();
                base.SetInvalid(true);
            }

        }

        public new List<mdSpoutGroup> ToList(bool bGetClones = false)
        {

            List<mdSpoutGroup> _rVal = new List<mdSpoutGroup>();

            foreach (var part in CollectionObj)
            {
                if (!bGetClones)
                    _rVal.Add((mdSpoutGroup)part);
                else
                    _rVal.Add((mdSpoutGroup)part.Clone());
            }
            return _rVal;
        }

        public override uopPart Item(int aIndex, bool bSuppressIndexError = false) => (uopPart)Item(aIndex, bSuppressIndexError);

        /// <summary>
        /// the item of a collection
        /// ~returns the specific value in a collection (mdspoutgroup) !Base 1!
        /// </summary>
        public mdSpoutGroup Item(dynamic aHandleOrIndex, bool bSuppressIndexError = false)
        {
            if (aHandleOrIndex == null) return null;
            if (aHandleOrIndex.GetType() == typeof(int))
            {
                int aIndex = (int)aHandleOrIndex;
                if (aIndex < 1 || aIndex > Count)
                    if (!bSuppressIndexError) { throw new IndexOutOfRangeException(); } else { return null; }
                return (mdSpoutGroup)base.Item(aIndex);
            }
            else
            {
                string hndl = Convert.ToString(aHandleOrIndex).Trim();
                mdSpoutGroup aMem;
                for (int i = 1; i <= Count; i++)
                {
                    aMem = (mdSpoutGroup)base.Item(i);
                    if (string.Compare(hndl, aMem.Handle, ignoreCase: true) == 0) return aMem;
                }
                return null;
            }
        }
        /// <summary>
        ///returns the objects properties in a collection
        /// </summary>
        public override uopProperties CurrentProperties() => base.CurrentProperties();

        public List<string> Names(string aPrefix = "Spout Group ")
        {
            if(string.IsNullOrEmpty(aPrefix)) aPrefix = string.Empty;
            List<string> _rVal = new List<string>();
            foreach(var part in this)
            {
                mdSpoutGroup sg = (mdSpoutGroup)part;
                _rVal.Add($"{aPrefix}{sg.Handle}");
            }
            return _rVal;
        }
        /// <summary>
        ///returns the defining perimeter polygons of the member spout groups
        /// </summary>
        internal UVECTORS PerimeterVertices(bool? aVirtualValue = null)
        {

            UVECTORS _rVal = UVECTORS.Zero;
            foreach (var item in this)
            {
                mdSpoutGroup mem = (mdSpoutGroup)item;
                if (!aVirtualValue.HasValue || (aVirtualValue.HasValue && mem.IsVirtual == aVirtualValue.Value))
                {
                    _rVal.Append(mem.PerimeterV.Vertices);
                }
            }

            return _rVal;

        }


        /// <summary>
        ///returns the properties required to save the spout groups to file
        /// </summary>
        public override uopPropertyArray SaveProperties(string aHeading = null)
        {
            aHeading = string.IsNullOrWhiteSpace(aHeading) ? INIPath : aHeading;

            uopPropertyArray _rVal = new uopPropertyArray();
            mdSpoutGroup aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = (mdSpoutGroup)base.Item(i);
                if (!aMem.IsVirtual) _rVal.Add(aMem.SaveProperties(aHeading).Item(1), aHeading, aHeading);

            }
            return _rVal;

        }

        /// <summary>
        /// the item to add to the collection
        /// used to add an item to the collection
        /// won't add "Nothing" (no error raised).
        /// </summary>
        public mdSpoutGroup Add(mdSpoutGroup aGroup, bool bAddClone = false)
        {
            if (aGroup == null) return null;

            uopPart _rVal = base.Add(aGroup, bAddClone);
            if (_rVal == null) return null;
            return (mdSpoutGroup)_rVal;


        }


        /// <summary>
        ///returns a rectangle that contains all of the spout groups in the collection
        /// </summary>
        public uopRectangle Bounds(bool? aVirtualValue = null) => new uopRectangle(PerimeterVertices(aVirtualValue).Bounds);


        /// <summary>
        ///returns a rectangle that contains all of the spout groups in the collection
        /// </summary>
        public dxfRectangle BoundingRectangle(dxfDisplaySettings aDisplaySettings = null, bool? aVirtualValue = false) => Bounds(aVirtualValue).ToDXFRectangle(aDisplaySettings);

        /// <summary>
        ///returns the centers of all the spout groups in the collection
        /// </summary>
        public colDXFVectors Centers(Collection<mdSpoutGroup> SearchCol = null)
        {
            colDXFVectors _rVal = new colDXFVectors();
            mdSpoutGroup aMem;
            if (SearchCol == null)
            {
                for (int i = 1; i <= Count; i++)
                {
                    aMem = (mdSpoutGroup)base.Item(i);
                    _rVal.Add(aMem.CenterDXF);
                }

            }
            else
            {
                for (int i = 0; i < SearchCol.Count; i++)
                {
                    aMem = SearchCol[i];
                    _rVal.Add(aMem.CenterDXF);
                }

            }
            return _rVal;
        }

        /// <summary>
        ///returns the MinimumBottomFraction
        /// </summary>
        public double MinimumBottomFraction(dynamic aDowcomerIndex, out int rIndex)
        {
            double _rVal = 1;
            int idx = 0;
            mdSpoutGroup aMem;
            rIndex = 0;
            idx = mzUtils.VarToInteger(aDowcomerIndex);
            for (int i = 1; i <= Count; i++)
            {
                aMem = (mdSpoutGroup)base.Item(i);
                if (aMem.DowncomerIndex == idx)
                {
                    if (aMem.BottomFraction < _rVal)
                    {
                        _rVal = aMem.BottomFraction;
                        rIndex = i;
                    }
                }
            }
            return _rVal;
        }

        /// <summary>
        ///returns an new object with the same properties as the cloned object
        /// </summary>
        public colMDSpoutGroups Clone() => new colMDSpoutGroups(this, null);

        public override uopPart Clone(bool aFlag = false) => (uopParts)this.Clone();

        /// <summary>
        /// flag to only regenerate if the spouts collection has not been initialize
        /// causes a regeneration of the spout groups spouts collections
        /// based on the current properties and constraints of the groups
        /// </summary>
        public void GenerateSpouts(mdTrayAssembly aAssy, bool bNonExistantOnly = false, bool? bSuppressEvnts = null)
        {
            aAssy ??= GetMDTrayAssembly(aAssy);
            mdSpoutGroup aMem;
            if(!bSuppressEvnts.HasValue) bSuppressEvnts = SuppressEvents;
            for (int i = 1; i <= Count; i++)
            {
                aMem = (mdSpoutGroup)base.Item(i);
                if (!aMem.IsVirtual)
                {
                    aMem.SuppressEvents = bSuppressEvnts.Value;
                    aMem.RegenerateSpouts(aAssy, bNonExistantOnly);
                    aMem.SuppressEvents = SuppressEvents;
                }
            }
        }


        /// <summary>
        /// the center to search for
        ///returns a group from the collection whose center is closest to the passed point
        /// </summary>
        public mdSpoutGroup GetByCenter(dxfVector GroupCenter, Collection<mdSpoutGroup> SearchCol = null)
        {
            mdSpoutGroup _rVal = null;
            if (GroupCenter == null) return null;

            colDXFVectors ctrs = null;
            dxfVector ctr = null;
            mdSpoutGroup aMem;
            ctrs = Centers(SearchCol);
            if (ctrs.Count == 0) return null;
            ctr = ctrs.NearestVector(GroupCenter);

            if (SearchCol == null)
            {
                for (int i = 1; i <= Count; i++)
                {
                    aMem = (mdSpoutGroup)base.Item(i);
                    if (aMem.CenterDXF.IsEqual(ctr, 2)) { _rVal = aMem; break; }

                }

            }
            else
            {
                for (int i = 0; i < SearchCol.Count; i++)
                {
                    aMem = SearchCol[i];
                    if (aMem.CenterDXF.IsEqual(ctr, 2)) { _rVal = aMem; break; }
                }

            }
            return _rVal;
        }

        /// <summary>
        /// #1the downcomer index to search for
        /// ^returns the spout group from the collection whose downcomer index matches the passed value
        /// </summary>
        public colMDSpoutGroups GetByDowncomerIndex(int aDowncomerIndex, bool bNonZero = false, bool bParentsOnly = false, int? aBoxIndex = null)
        {
            colMDSpoutGroups _rVal = new colMDSpoutGroups(this, null, true)
            {
                MaintainIndices = false
            };

            _rVal.SubPart(GetMDTrayAssembly());
            for (int i = 1; i <= Count; i++)
            {
                mdSpoutGroup aMem = (mdSpoutGroup)base.Item(i);
                if (bParentsOnly && aMem.IsVirtual) continue;
                
                if (aMem.DowncomerIndex == aDowncomerIndex )
                {
                    if (aBoxIndex.HasValue && aMem.BoxIndex != aBoxIndex.Value) continue;
                    if (!bNonZero || (bNonZero && aMem.SpoutCount() > 0))
                     _rVal.Add(aMem); 
                }
            }
            return _rVal;
        }

        /// <summary>
        /// #1the group index to search for
        /// ^returns the first spout group from the collection whose group index matches the passed value
        /// </summary>
        public mdSpoutGroup GetByGroupIndex(double aGroupIndex)
        {
            mdSpoutGroup aMem;

            for (int i = 1; i <= Count; i++)
            {
                aMem = (mdSpoutGroup)base.Item(i);
                if (aMem.GroupIndex == aGroupIndex) return aMem;
            }
            return null;
        }

        /// <summary>
        /// #1handle to search for
        /// #2a collection of spout groups to search other that this collection
        /// ^returns the spout group from the collection whose handle matches the passed value
        /// </summary>
        public bool TryGet(string aHandle, out mdSpoutGroup rGroup)
        {
            rGroup = GetByHandle(aHandle, null, out int aIndex);
            return rGroup != null;
        }

        /// <summary>
        /// #1handle to search for
        /// #2a collection of spout groups to search other that this collection
        /// ^returns the spout group from the collection whose handle matches the passed value
        /// </summary>
        public mdSpoutGroup GetByHandle(string aHandle, colMDSpoutGroups aSearchCol = null)
        {
            return GetByHandle(aHandle, aSearchCol, out int aIndex);
        }

        /// <summary>
        /// #1handle to search for
        /// #2a collection of spout groups to search other that this collection
        /// ^returns the spout group from the collection whose handle matches the passed value
        /// </summary>
        public mdSpoutGroup GetByHandle(string aHandle, colMDSpoutGroups aSearchCol, out int rIndex)
        {
            mdSpoutGroup aMem;
            colMDSpoutGroups sCol = aSearchCol ?? this;
            rIndex = 0;

            for (int i = 1; i <= sCol.Count; i++)
            {
                aMem = sCol.Item(i);
                if (string.Compare(aHandle, aMem.Handle, true) == 0)
                {
                    rIndex = i;
                    return aMem;

                }
            }
            return null;
        }

        /// <summary>
        /// #1the deck panel index to search for
        /// ^returns the spout group from the collection whose panel index matches the passed value
        /// </summary>
        public colMDSpoutGroups GetByPanelIndex(double aPanelIndex, bool bNonZero = false, bool bParentsOnly = false)
        {
            colMDSpoutGroups _rVal = new colMDSpoutGroups(this, null, true)
            {
                MaintainIndices = false
            };

            for (int i = 1; i <= Count; i++)
            {
                mdSpoutGroup aMem = (mdSpoutGroup)base.Item(i);
                if (bParentsOnly && aMem.IsVirtual) 
                    continue;

                if (aMem.PanelIndex == aPanelIndex)
                {
                    if (!bNonZero)
                    { _rVal.Add(aMem); }
                    else
                    {
                        if (aMem.SpoutCount() > 0) _rVal.Add(aMem);

                    }
                }
            }
            return _rVal;
        }

        /// <summary>
        ///returns the collection of members in the collection whose length property equals the passed value
        /// </summary>
        public List<mdSpoutGroup> GetByPatternLength(double aLength, int RoundTo = 4)
        {
            List<mdSpoutGroup> _rVal = new List<mdSpoutGroup>();
            RoundTo = mzUtils.LimitedValue(RoundTo, 0, 8);

            mdSpoutGroup aMem;
            mdTrayAssembly aAssy = GetMDTrayAssembly();

            for (int i = 1; i <= Count; i++)
            {
                aMem = (mdSpoutGroup)base.Item(i);
                if (Math.Round(aMem.PatternLength, RoundTo).Equals(Math.Round(aLength, RoundTo))) _rVal.Add(aMem);
            }
            return _rVal;
        }

        /// <summary>
        /// #1the panel index to search for
        /// #2the downcomer index to search for
        /// ^returns the spout group from the collection whose panel and downcomer indices match the passed values
        /// </summary>
        public mdSpoutGroup GetGroup(int PanelIndex, int DowncomerIndex)
        {
            mdSpoutGroup aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = (mdSpoutGroup)base.Item(i);
                if (aMem.PanelIndex == PanelIndex && aMem.DowncomerIndex == DowncomerIndex) return aMem;
            }
            return null;
        }

        /// <summary>
        /// returns onlt the spout groups that have the TreatAsGroup Flag = True
        /// </summary>
        public List<mdSpoutGroup> GetGroupedSpoutGroups(int aSpoutAreaGroupIndex)
        {
            List<mdSpoutGroup> _rVal = new List<mdSpoutGroup>();
            mdSpoutGroup aMem;
            mdTrayAssembly aAssy = GetMDTrayAssembly();
            if (aSpoutAreaGroupIndex <= 0) return _rVal;

            for (int i = 1; i <= Count; i++)
            {
                aMem = (mdSpoutGroup)base.Item(i);
                if (aMem.Constraints(aAssy).AreaGroupIndex == aSpoutAreaGroupIndex) _rVal.Add(aMem);
            }

            return _rVal;
        }

        /// <summary>
        /// #1the limit to apply to determine if a spout groups solution is invalid
        /// #2returns the spout groups that exceed the passed limit
        /// ^returns the spout group from the collection with the maximum error percentage.
        /// </summary>
        public mdSpoutGroup GetMaxError(mdTrayAssembly aAssy, double aErrorLimit, out List<mdSpoutGroup> rInvalidGroups)
        {
            rInvalidGroups = new List<mdSpoutGroup>();
            mdSpoutGroup _rVal = null;
            double max = -1000000;
            aAssy ??= GetMDTrayAssembly(aAssy);

            for (int i = 1; i <= Count; i++)
            {
                mdSpoutGroup aMem = (mdSpoutGroup)base.Item(i);
                if (!aMem.IsVirtual)
                {
                    double aVal = Math.Abs(aMem.ErrorPercentage);
                    if (aVal > Math.Abs(aErrorLimit)) rInvalidGroups.Add(aMem);

                    if (aVal > max)
                    {
                        _rVal = aMem;
                        max = aVal;
                    }
                }

            }
            return _rVal;
        }


        /// <summary>
        /// returns only the spout groups from the collection that have an actual area property greater than 0
        /// </summary>
        public List<mdSpoutGroup> GetSpoutedGroups(double aPanelIndex = 0, double aDowncomerIndex = 0)
        {
            List<mdSpoutGroup> _rVal = new List<mdSpoutGroup>();
            mdSpoutGroup aMem;
            mdTrayAssembly aAssy = GetMDTrayAssembly();

            for (int i = 1; i <= Count; i++)
            {
                aMem = (mdSpoutGroup)base.Item(i);
                if (aPanelIndex <= 0 || aMem.PanelIndex == aPanelIndex)
                {
                    if (aDowncomerIndex <= 0 || aMem.DowncomerIndex == aDowncomerIndex)
                    {
                        if (aMem.SpoutCount(aAssy) > 0) _rVal.Add(aMem);

                    }
                }
            }
            return _rVal;
        }

        /// <summary>
        ///returns the spout groups that have constraint property TreatAsGroup = True
        /// </summary>
        public uopMatrix GroupedAreas(mdTrayAssembly aAssy)
        {
            uopMatrix _rVal = new uopMatrix();
            _rVal.SetAllValues(-1);
            aAssy ??= GetMDTrayAssembly(aAssy);
            if (aAssy == null) return _rVal;

            int dcnt = aAssy.Downcomer().Count;
            _rVal.SetDimensions(dcnt + 2, dcnt + 1);
            _rVal.Precision = 1;
            bool symmetric = aAssy.IsSymmetric;
            for (int i = 1; i <= Count; i++)
            {
                mdSpoutGroup aMem = (mdSpoutGroup)base.Item(i);
                if (!aMem.IsVirtual)
                {
                    if (symmetric)
                        _rVal.SetMemberSymmetric(aMem.PanelIndex, aMem.DowncomerIndex, Convert.ToInt32(aMem.Constraints(aAssy).TreatAsGroup));
                    else
                        _rVal.SetValue(aMem.PanelIndex, aMem.DowncomerIndex, Convert.ToInt32(aMem.Constraints(aAssy).TreatAsGroup));

                }
            }
            return _rVal;
        }

        /// <summary>
        /// returns true if the value is equal
        /// </summary>
        public bool IsEqual(colMDSpoutGroups aSpoutCol = null)
        {
            bool isEqual = false;
            if (aSpoutCol == null || aSpoutCol.Count != Count)
            {
                return isEqual;

            }
            mdSpoutGroup aSG = null;

            mdTrayAssembly aAssy = GetMDTrayAssembly();


            isEqual = true;
            for (int i = 0; i < Count; i++)
            {
                aSG = Item(i);
                if (!aSG.IsEqual(aSpoutCol.Item(i), aAssy))
                {
                    isEqual = false;
                    break;
                }
            }
            return isEqual;
        }

        /// <summary>
        ///returns the spout groups that have constraint property TreatAsIdal = True
        /// </summary>
        public uopMatrix LockedAreas(mdTrayAssembly aAssy)
        {
            uopMatrix _rVal = new uopMatrix();

            aAssy ??= GetMDTrayAssembly(aAssy);
            if (aAssy == null) return _rVal;
            int dcnt = aAssy.Downcomer().Count;
            int pcnt = dcnt + 1;
            _rVal.SetDimensions(pcnt + 1, dcnt + 1);
            _rVal.SetAllValues(-1);
            _rVal.Precision = 6;
            bool symmetric = aAssy.IsSymmetric;

            for (int i = 0; i < Count; i++)
            {
                mdSpoutGroup aMem = (mdSpoutGroup)base.Item(i);
                if (!aMem.IsVirtual)
                {
                    if (aMem.Constraints(aAssy).TreatAsIdeal)
                    {
                        int didx = aMem.DowncomerIndex;
                        int pidx = aMem.PanelIndex;
                        double overd = aMem.Constraints(aAssy).OverrideSpoutArea;
                        if (symmetric)
                            _rVal.SetMemberSymmetric(pidx, didx, overd);
                        else
                            _rVal.SetValue(pidx, didx, overd);
                    }

                }

            }
            return _rVal;
        }

        /// <summary>
        /// reurns the current lock and group area matrices
        /// </summary>
        public void GetLockedAndGroupedAreaMatrices(mdTrayAssembly aAssy, out uopMatrix rLocks, out uopMatrix rGroups)
        {
            rLocks = new uopMatrix();
            rGroups = new uopMatrix();

            aAssy ??= GetMDTrayAssembly(aAssy);
            if (aAssy == null) return;

            int dcnt = aAssy.Downcomer().Count;
            int pcnt = dcnt + 1;
            string tname = aAssy.TrayName(false);
            rLocks = new uopMatrix(pcnt, dcnt, aPrecis: 6, $"Area Locks {tname}", -1);
            rGroups = new uopMatrix(pcnt, dcnt, aPrecis: 1, $"Group Indices {tname}", 0);


            bool? symmetric = null;
            if (aAssy.IsSymmetric) symmetric = true;
            for (int i = 1; i <= Count; i++)
            {
                mdSpoutGroup aMem = (mdSpoutGroup)base.Item(i);

                if (!aMem.IsVirtual)
                {
                    int didx = aMem.DowncomerIndex;
                    int pidx = aMem.PanelIndex;
                    mdConstraint mdcons = aMem.Constraints(aAssy);
                    bool bIdeal = mdcons.TreatAsIdeal;
                    bool bGroup = mdcons.TreatAsGroup;
                    double gVal = bGroup ? (double)mdcons.AreaGroupIndex : 0;
                    rGroups.SetValue(pidx, didx, gVal, symmetric);
                    double aVal = bIdeal && !bGroup ? mdcons.OverrideSpoutArea : -1;
                    rLocks.SetValue(pidx, didx, aVal, symmetric);
                }

            }
            //rGroups.PrintToConsole();
        }

        public virtual List<mdSpoutGroup> GetByVirtual(bool? aVirtualValue, bool bGetClones = false, bool bNonZeroOnly = false)
        {
            List<mdSpoutGroup> _rVal = new List<mdSpoutGroup>();
            foreach (var item in this)
            {
                if (aVirtualValue.HasValue)
                {
                    if (item.IsVirtual != aVirtualValue.Value) continue;
                }

                mdSpoutGroup mem = item;
                if (!bNonZeroOnly || (bNonZeroOnly && mem.SpoutCount() > 0))
                {
                    _rVal.Add(!bGetClones ? mem : mem.Clone());

                }
            }
            return _rVal;
        }

        /// <summary>
        /// this will generate notification
        /// </summary>
        public void NotifyGeneration(mdSpoutGroup aMember, bool bStart)
        {
            if (aMember == null)
                return;
            eventSpoutGroupsGenerationEvent?.Invoke(bStart, aMember.Handle);
        }

        /// <summary>
        ///returns the collection of unique length property values of the members in the collection
        /// </summary>
        public List<double> PatternLengths(int RoundTo = 4)
        {
            List<double> patternLengths = new List<double>();
            double aVal = 0;
            int i = 0;
            int j = 0;
            mdSpoutGroup aMem;
            mdTrayAssembly aAssy = GetMDTrayAssembly();
            RoundTo = Abs(RoundTo);
            if (RoundTo > 8)
            {
                RoundTo = 8;
            }
            for (i = 0; i < Count; i++)
            {
                aMem = (mdSpoutGroup)base.Item(i);
                aVal = Round(aMem.PatternLength, RoundTo);
                if (aVal > 0)
                {
                    for (j = 0; j < patternLengths.Count(); j++)
                    {
                        if (patternLengths[j] == aVal)
                        {
                            break;
                        }
                        else
                        {
                            patternLengths.Add(aVal);
                        }
                    }
                }
            }
            return patternLengths;
        }

        /// <summary>
        /// reassigns indices after a remove procedure
        /// </summary>
        private void ReIndex()
        {
            for (int i = 0; i < Count; i++)
            {
                Item(i).Index = i;
            }
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
        public  void ReadProperties(uopProject aProject, uopPropertyArray aFileProps, ref uopDocuments ioWarnings, double aFileVersion, string aFileSection = null, bool bIgnoreNotFound = false, uopTrayAssembly aAssy = null, uopPart aPartParameter = null, List<string> aSkipList = null, Dictionary<string, string> EqualNames = null, uopProperties aProperties = null, bool bRegenOnly = false)
        {

            try
            {
                ioWarnings ??= new uopDocuments();
                aFileSection = string.IsNullOrWhiteSpace(aFileSection) ? INIPath : aFileSection.Trim();
                if (aFileProps == null) throw new Exception("The Passed File Property Array is Undefined");
                if (string.IsNullOrWhiteSpace(aFileSection)) throw new Exception("File Section is Undefined");
                if (aProject != null) SubPart(aProject);
                if (aAssy == null) throw new Exception("Tray Assembly is Undefined");
                mdTrayAssembly myassy = (mdTrayAssembly)aAssy;
                SubPart(myassy);


                aProject?.ReadStatus($"Reading {TrayName(true)} Spout Group Properties");

                string FSec = mzUtils.ThisOrThat(aFileSection, INIPath).Trim().ToUpper();

                Reading = true;
                for (int i = 1; i <= Count; i++)
                {
                    mdSpoutGroup aMem = (mdSpoutGroup)base.Item(i);
                    if (aMem.IsVirtual) continue;
                  
                    aMem.ReadProperties(aProject, aFileProps, ref ioWarnings, aFileVersion, FSec, aAssy: myassy, bRegenOnly: bRegenOnly);

                }
                Reading = false;
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
        /// marks the indicated point (Z)
        /// </summary>
        public void SetZ(double aZOrd)
        {
            mdSpoutGroup aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = (mdSpoutGroup)base.Item(i);
                aMem.Z = aZOrd;
                base.SetItem(i, aMem);
            }
        }

        /// <summary>
        /// all the startup lines of all the members of the collection
        /// </summary>
        public mdStartupLines StartupLines(List<mdSpoutGroup> SearchCol = null)
        {
            List<mdSpoutGroup> sCol = SearchCol == null ? this.ToList() : SearchCol;

            mdStartupLines _rVal = new mdStartupLines();
            foreach(mdSpoutGroup aMem in sCol) 
                _rVal.Append(aMem.StartupLines);
            
            return _rVal;
        }

       
        public List<uopRectangle> SpoutLimits(List<mdSpoutGroup> SearchCol = null)
        {
            List<mdSpoutGroup> sCol = SearchCol == null ? this.ToList() : SearchCol;

            List<uopRectangle> _rVal = new List<uopRectangle>();
            foreach (mdSpoutGroup aMem in sCol) 
            {
                if(aMem.SpoutCount(null) > 0)
                _rVal.Add(aMem.SpoutLimits);
            }
            return _rVal;
        }

        /// <summary>
        ///returns the total theoretical area of all the groups in the collection
        /// </summary>
        public double TotTargetArea(mdTrayAssembly aAssy)
        {
            double _rVal = 0;
            mdSpoutGroup aMem;
            aAssy ??= GetMDTrayAssembly(aAssy);

            for (int i = 1; i <= Count; i++)
            {
                aMem = (mdSpoutGroup)base.Item(i);
                _rVal += aMem.TargetArea(aAssy) * aMem.OccuranceFactor;
            }
            return _rVal;
        }

        /// <summary>
        /// this will update the part properties
        /// </summary>
        public override void UpdatePartProperties()
        {
            mdSpoutGroup aMem;

            for (int i = 1; i <= Count; i++)
            {
                aMem = (mdSpoutGroup)base.Item(i);
                aMem.UpdatePartProperties();
                SubPartProperties();
            }
        }

        /// <summary>
        /// this will update the part weight
        /// </summary>
        public override void UpdatePartWeight() => base.Weight = 0;


        /// <summary>
        /// this will update the spouts
        /// </summary>
        public void UpdateSpouts(mdTrayAssembly aAssy)
        {
            aAssy ??= GetMDTrayAssembly(aAssy);
            if (aAssy == null) return;

            SubPart(aAssy);
            for (int i = 1; i <= Count; i++)
            {
                mdSpoutGroup aMem = (mdSpoutGroup)base.Item(i);
                aMem.UpdateSpouts(aAssy);
                base.SetItem(i, aMem);
            }
        }


        /// <summary>
        /// this will update the assembly level constraints
        /// </summary>
        public void UpdateConstraints(mdTrayAssembly aAssy)
        {
            aAssy ??= GetMDTrayAssembly();
            if (aAssy == null) return;

            SubPart(aAssy);
            for (int i = 1; i <= Count; i++)
            {
                mdSpoutGroup aMem = (mdSpoutGroup)base.Item(i);

                aMem.UpdateConstraints(aAssy);
                base.SetItem(i, aMem);
            }
        }

        /// <summary>
        ///returns a collection of strings that are warnings about possible problems with
        /// the current tray assembly design.
        /// these warnings may or may not be fatal problems.
        /// </summary>
        public uopDocuments GenerateWarnings(mdTrayAssembly aAssy, string aCategory = "", uopDocuments aCollector = null, bool bJustOne = false)
        {
            uopDocuments _rVal = aCollector ?? new uopDocuments();
            if (bJustOne && _rVal.Count > 0) return _rVal;
            aAssy ??= GetMDTrayAssembly(aAssy);
            if (aAssy == null) return _rVal;

            mdSpoutGroup aMem;
            string txt = string.Empty;
            bool bAP = aAssy.HasAntiPenetrationPans;

            mdDowncomer aDC;
            colMDConstraints Cns = aAssy.Constraints;
            mdConstraint sgCons;
            double aVal;
            double defClrc;
            TVALUES clrcLst = new TVALUES();
            TVALUES apLst = new TVALUES();
            TVALUES errLst = new TVALUES();
            int j = 0;

            double errLim = aAssy.ProjectType != uppProjectTypes.MDDraw ? aAssy.ErrorLimit : 0;

            aCategory = string.IsNullOrWhiteSpace(aCategory) ? TrayName(true) : aCategory.Trim();


            if (aAssy.ProjectType == uppProjectTypes.MDSpout)
            {

                aMem = GetMaxError(aAssy, aAssy.ErrorLimit, out List<mdSpoutGroup> sGroups);
                string errList = string.Empty;
                if (sGroups.Count > 0)
                {
                    mzUtils.ListAdd(ref errList, aMem.Handle, aDelimitor: uopGlobals.Delim);
                    txt = $"At Least 1 Spout Group Has a Spout Area Deviation That Exceeds The Project Limit of {aAssy.ErrorLimit:0.00} %";
                    _rVal.AddWarning(aAssy, "Spout Area Warning", txt, uppWarningTypes.ReportFatal);
                    if (bJustOne && _rVal.Count > 0) return _rVal;
                }

            }

            for (int i = 1; i <= Count; i++)
            {
                aMem = (mdSpoutGroup)base.Item(i);
                if (!aMem.IsVirtual)
                {
                    aDC = aMem.Downcomer(aAssy);

                    sgCons = Cns.GetByHandle(aMem.Handle);
                    defClrc = mdSpoutGroup.GetDefaultClearance(aDC);
                    if (aMem.TargetArea() > 0)
                    {
                        if (errLim > 0)
                        {
                            if (Math.Abs(aMem.ErrorPercentage) > errLim) errLst.Add(aMem.Handle);

                        }
                        if (Math.Round(aMem.ActualClearance - defClrc, 3) < -0.001) clrcLst.Add(aMem.Handle);


                        if (bAP && !aMem.LimitedBounds)
                        {
                            if (aMem.ActualMargin < 1) apLst.Add(aMem.Handle);

                        }
                    }
                    if (aMem != null && aMem.GroupIndex == 1)
                    {
                        aVal = sgCons.EndPlateClearance;
                        if (aVal <= 0) aVal = 0.25;

                        if (aVal < 0.25)
                        {
                            txt = $"Spout Group({aMem.Handle}) Has An End Plate Clearance Value Less Than The Recomended 0.25''";
                            _rVal.AddWarning(aAssy, "Spout Clearance", txt);
                            if (bJustOne && _rVal.Count > 0) return _rVal;
                        }
                    }

                }
            }

            if (errLst.Count > 0)
            {
                txt = errLst.ToDelimitedList(", ", out j, false, " & ");

                if (j > 1)
                {
                    txt = $"Spout Groups {txt} Have Spout Area Deviations That Exceeds The Project Limit of {string.Format(errLim.ToString(), "N2")} %";
                }
                else
                {
                    txt = $"Spout Group {txt} Has a Spout Area Deviations That Exceeds The Project Limit of {string.Format(errLim.ToString(), "N2")} %";
                }
                _rVal.AddWarning(aAssy, "Spout Area Warning", txt, uppWarningTypes.ReportFatal);
                if (bJustOne && _rVal.Count > 0) return _rVal;
            }
            if (apLst.Count > 0)
            {
                txt = errLst.ToDelimitedList(", ", out j, false, " & ");
                if (j > 1)
                {
                    txt = $"Spout Groups {txt} Have Margins That May Be Too Small To Allow Installation of AP Pans";

                }
                else
                {
                    txt = $"Spout Group {txt} Has a Margin That May Be Too Small To Allow Installation of AP Pans";
                }
                _rVal.AddWarning(aAssy, "AP Pan Warning", txt);
                if (bJustOne && _rVal.Count > 0) return _rVal;
            }
            if (clrcLst.Count > 0)
            {
                txt = clrcLst.ToDelimitedList(", ", out j, false, " & ");

                if (j > 1)
                {
                    txt = $"Spout Groups {txt} Have Clearance Values Which Violate The Minimum Spec. Spout Deformation May Occur.";
                }
                else
                {
                    txt = $"Spout Group {txt} Has a Clearance Values Which Violate The Minimum Spec. Spout Deformation May Occur."; ;
                }
                _rVal.AddWarning(aAssy, "Spout Clearance", txt);
                if (bJustOne && _rVal.Count > 0) return _rVal;
            }

            return _rVal;
        }

        #endregion Methods

        #region SharedMethods

        public static void GetSpoutEntities(IEnumerable<mdSpoutGroup> aGroups, double aErrorLimit, out List<dxeArc> rCirclesValid, out List<dxeArc> rCirclesInValid, out List<dxePolyline> rSlotsValid, out List<dxePolyline> rSlotsInValid, dxfDisplaySettings aBaseSets = null)
        {
            rCirclesValid = new List<dxeArc>();
            rCirclesInValid = new List<dxeArc>();
            rSlotsValid = new List<dxePolyline> { };
            rSlotsInValid = new List<dxePolyline> { };
            if (aGroups == null) return;
            aBaseSets ??= new dxfDisplaySettings("SPOUTS", dxxColors.ByLayer, dxfLinetypes.Continuous);
            dxxColors vcolor = aBaseSets.Color;
            foreach (var aGroup in aGroups)
            {
                if (aGroup == null) continue;
                double aVal = Math.Abs(aGroup.ErrorPercentage);
                bool invalid = (aErrorLimit != 0 && aVal > Math.Abs(aErrorLimit));
                if (aGroup.Grid.ViolatesSafeMargin) invalid = true;
                aBaseSets.Color = invalid ? dxxColors.Red : vcolor;
                colDXFEntities spouts = aGroup.BlockEntities(aBaseSets.LayerName, aBaseSets.Color, aBaseSets.Linetype, bSuppressInstances: true, bSuppressCenterPts: true);
                if (invalid)
                {
                    rCirclesInValid.AddRange(spouts.Arcs());
                    rSlotsInValid.AddRange(spouts.Polylines());
                }
                else
                {
                    rCirclesValid.AddRange(spouts.Arcs());
                    rSlotsValid.AddRange(spouts.Polylines());

                }

            }
        }

        #endregion SharedMethods
    }
}
