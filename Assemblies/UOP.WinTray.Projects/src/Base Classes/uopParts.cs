using UOP.DXFGraphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Interfaces;
using UOP.WinTray.Projects.Materials;
using UOP.WinTray.Projects.Parts;

using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using System.Reflection;

namespace UOP.WinTray.Projects
{
    public abstract class uopParts : uopPart, IEnumerable<uopPart>, IDisposable, System.Collections.Specialized.INotifyCollectionChanged
    {

        private List<uopPart> _Members;
        private bool _MaintainIndices = false;
        private bool _BaseOne = true;
        private bool _InvalidWhenEmpty = false;
     
        private bool disposedValue;

        public delegate void ValidityChange(bool bInvalidValue);
        public event ValidityChange EventValidityChange;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #region Constructors
        public uopParts(uppPartTypes aPartType, uppProjectFamilies aProjectFamily, bool bBaseOne = true, bool bMaintainIndices = false, bool bInvalidWhenEmpty = false) : base(aPartType,aProjectFamily) { _BaseOne = bBaseOne; _Members = new List<uopPart>(); _MaintainIndices = bMaintainIndices; _InvalidWhenEmpty = bInvalidWhenEmpty; PartFilter = uppPartTypes.Undefined; SetFilter(); }
    
        public uopParts(IEnumerable<uopPart> aPartsTopCopy, bool bDontCopyMembers = false, uopPart aParent = null) : base(uppPartTypes.Undefined, uppProjectFamilies.Undefined)
        {
            _Members = new List<uopPart>();

            if (aPartsTopCopy != null)
            {
                if (aPartsTopCopy.GetType().IsSubclassOf(typeof (uopParts)))
                {
                    uopParts uparts = (uopParts)aPartsTopCopy;
                    base.Copy(uparts);
                    _MaintainIndices = uparts.BaseOne;
                    _BaseOne = uparts.BaseOne;
                    _InvalidWhenEmpty = uparts.InvalidWhenEmpty;
                    PartFilter = uparts.PartFilter;
                    PartType = uparts.PartType;
                }


            }
            else { bDontCopyMembers = true; }

            // Copy(aPartTopCopy);
            Quantity = 0;

            if (!bDontCopyMembers) Append(aPartsTopCopy, bAddClones: true);
            Quantity = Count;
            SubPart(aParent);

            //uopPart aMem;
            //Collection < uopPart > mems = aPartTopCopy._Members;

            //for (int i = 0; i < mems.Count; i++)
            //{
            //    aMem = mems[i];
            //    if (aMem != null) aMem = aMem.Clone(false);
            //    aMem.SubPart(this);
            //    if (aMem != null) _Members.Add(aMem);



            //}

            Quantity = _Members.Count;

        }

        public uopParts(List<uopPart> aParts,  bool bCloneMembers = false, bool bBaseOne = true, bool bMaintainIndices = false, bool bInvalidWhenEmpty = false) : base(uppPartTypes.Undefined, uppProjectFamilies.Undefined)
        {
            _BaseOne = bBaseOne;
            _Members = aParts ?? new List<uopPart>();
            _MaintainIndices = bMaintainIndices;
            _InvalidWhenEmpty = bInvalidWhenEmpty;
            PartFilter = uppPartTypes.Undefined;
            SetFilter();
            //if (aParts != null) 
            //{
            //    for(int i = 0; i < aParts.Count; i++)
            //    {
            //        Add(aParts[i]);
            //    }
            //}



        }



        #endregion Constructors



        #region IEnumerable Implementation


        public IEnumerator<uopPart> GetEnumerator() => _Members.GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _Members.GetEnumerator();


        #endregion IEnumerable Implementation

        #region Properties

        public virtual bool HasAlternateDeckParts => FindAll(x => x.AlternateRingType != uppAlternateRingTypes.AllRings).Count > 0;

        public uppPartTypes PartFilter { get; internal set; }

        private int _SelID = 0;
        public virtual uopPart SelectedMember
        {
            get
            {

                _SelID = 0;
                uopPart aMem;
                uopPart _rVal = null;

                for (int i = 1; i <= Count; i++)
                {
                    aMem = _Members[i - 1];
                    if (aMem.Selected)
                    {
                        if (_rVal == null)
                        {
                            _SelID = i;
                            _rVal = aMem;
                        }
                        else { aMem.SetSelected(false); }
                    }
                }
                if (_rVal == null && Count > 0)
                {
                    _Members[0].SetSelected(true);
                    _SelID = 1;
                }

                return (_SelID > 0) ? Item(_SelID) : null;

            }

        }

        public int SelectedIndex
        {
            get
            {
                uopPart aMem = SelectedMember;
                return _SelID;
            }

            set => SetSelected(value);

        }

        public bool InvalidWhenEmpty { get => _InvalidWhenEmpty; set => _InvalidWhenEmpty = value; }

        public override bool SuppressEvents
        {
            get => base.SuppressEvents;
            set
            {
                base.SuppressEvents = value;
                for (int i = 1; i <= Count; i++) { _Members[i - 1].SuppressEvents = value; }
            }
        }

        public virtual int Count => _Members != null ? _Members.Count : 0;

        public bool MaintainIndices { get => _MaintainIndices; set { if (_MaintainIndices = value) return; _MaintainIndices = value; Reindex(); } }

        public bool BaseOne { get => _BaseOne; set { if (_BaseOne = value) return; _BaseOne = value; Reindex(); } }

        internal List<uopPart> Members => _Members;

        public override bool Invalid { get => _InvalidWhenEmpty ? (base.Invalid || Count <= 0) : base.Invalid; set { if (base.Invalid == value) return; base.Invalid = value; EventValidityChange?.Invoke(value); } }

        public override uopMaterial Material
        {
            get => base.Material;
            set
            {
                if (value == null) return;

                base.Material = value;
                uopPart aMem;
                for (int i = 1; i <= Count; i++)
                {
                    aMem = Item(i);
                    aMem.Material = value;
                }

            }
        }

        internal override TMATERIAL SheetMetalStructure
        {
            get => base.SheetMetalStructure;
            set
            {
                base.SheetMetalStructure = value;
                foreach (var item in _Members) { item.SheetMetalStructure = value; }
            }

        }

        public List<uopPart> CollectionObj
        {
            get { _Members ??= new List<uopPart>(); return _Members; }
            set { _Members = value ?? new List<uopPart>(); if (MaintainIndices) Reindex(); }
        }

        public virtual List<double> XOrdinates
        {
            get
            {
                List<double> _rVal = new List<double>();
                foreach (uopPart item in this)
                {
                    if (!_rVal.Contains(item.X)) _rVal.Add(item.X);
                }
                _rVal.Sort();

                return _rVal;
            }
        }
        public virtual List<double> YOrdinates
        {
            get
            {
                List<double> _rVal = new List<double>();
                foreach (uopPart item in this)
                {
                    if (!_rVal.Contains(item.Y)) _rVal.Add(item.Y);
                }
                _rVal.Sort();

                return _rVal;
            }
        }
        public ObservableCollection<uopPart> ToObservable()
        {
            ObservableCollection<uopPart> _rVal = new ObservableCollection<uopPart>();
            if (Count <= 0) return _rVal;
            foreach (uopPart item in _Members)
            {
                _rVal.Add(item);
            }
            return _rVal;
        }

        #endregion Properties

        #region Methods
        public virtual uopPart LastItem() => Item(Count, bSuppressIndexError: true);

        public virtual uopPart FirstItem() => Item(1, bSuppressIndexError: true);

        private void SetFilter()
        {
            PartFilter = base.PartType switch
            {
                uppPartTypes.TrayRanges => uppPartTypes.TrayRange,
                uppPartTypes.Constraints => uppPartTypes.Constraint,
                uppPartTypes.DeckSplices => uppPartTypes.DeckSplice,
                uppPartTypes.Downcomers => uppPartTypes.Downcomer,
                uppPartTypes.SpoutGroups => uppPartTypes.SpoutGroup,
                uppPartTypes.DeckPanels => uppPartTypes.DeckPanel,
                uppPartTypes.ChimneyTrays => uppPartTypes.ChimneyTray,
                uppPartTypes.Distributors => uppPartTypes.Distributor,
                _ => uppPartTypes.Undefined
            };

        }

        public override string ToString()
        {
            return $"{base.ToString()}[{Count}]";
        }

        public virtual uopPart Add(uopPart aPart)
        {
            return Add(aPart, false, null, -1);

        }

        public virtual uopPart Add(uopPart aPart, bool bAddClone, string aCategory = null, int aBeforeIndex = -1)
        {

            if (aPart == null) return null;

            if (aPart.PartType == uppPartTypes.Undefined) return null;
            if (PartFilter != uppPartTypes.Undefined && aPart.PartType != PartFilter) return null;

            if (_Members.IndexOf(aPart) >= 0) bAddClone = true;
            uopPart _rVal = (!bAddClone) ? aPart : aPart.Clone();
            if (_rVal == null) return null;

            if (!string.IsNullOrEmpty(aCategory)) _rVal.Category = Convert.ToString(aCategory);

            int aLocation = (aBeforeIndex <= 0 || aBeforeIndex > Count) ? Count + 1 : aBeforeIndex;
            return Insert(_rVal, aLocation);

        }

        /// <summary>
        /// Used to set all the ranges hardware material in one step
        /// </summary>
        /// <param name="aMaterial"></param>
        public void SetMaterial(uopMaterial aMaterial)

        {
            if (aMaterial == null) return;
            uopPart aMem;
            uppMaterialTypes typ = aMaterial.MaterialType;
            TMATERIAL strc = aMaterial.Structure;

            for (int i = 1; i <= Count; i++)
            {
                aMem = _Members[i - 1];
                switch (typ)
                {
                    case uppMaterialTypes.SheetMetal:
                        aMem.SheetMetalStructure = strc;
                        break;
                    case uppMaterialTypes.Hardware:
                        aMem.HardwareMaterial = (uopHardwareMaterial)aMaterial;
                        break;
                    case uppMaterialTypes.Tubing:
                        aMem.TubeMaterial = (uopTubeMaterial)aMaterial;
                        break;
                }
                _Members[i - 1] = aMem;
            }
        }

        /// <summary>
        ///returns the collection of center points of the member parts
        /// </summary>
        /// <param name="bReturnClones"></param>
        /// <param name="bReturnVirtuals"></param>
        /// <returns></returns>
        public virtual uopVectors Centers(bool bReturnClones = false, bool bReturnVirtuals = false, uppPartTypes? aPartType = null)
        {
            uopVectors _rVal = new uopVectors();

            for (int i = 1; i <= Count; i++)
            {
                uopPart aMem = Item(i);
                if (aMem.IsVirtual && !bReturnVirtuals) continue;
                if(aPartType.HasValue && aPartType != aMem.PartType) continue;
                _rVal.Add(aMem.Center, bAddClone: bReturnClones);
            }
            return _rVal;
        }
        public colDXFVectors CentersDXF(bool bReturnClones = false, uppPartTypes aPartType = uppPartTypes.Undefined, int aPanelID = -1, int aDowncomerID = -1, List<object> rIndices = null)
        {
            colDXFVectors _rVal = new colDXFVectors();
            uopPart aMem;

            for (int i = 1; i <= Count; i++)
            {
                aMem = _Members[i - 1];
                dxfVector v1 = aMem.CenterDXF;
                if (v1 != null)
                {
                    v1.Value = i;
                    bool bKeep = aPartType == uppPartTypes.Undefined || (aPartType != uppPartTypes.Undefined && aMem.PartType == aPartType);
                    if (bKeep && aPanelID > 0) bKeep = aMem.PanelIndex == aPanelID;
                    if (bKeep && aDowncomerID > 0) bKeep = aMem.DowncomerIndex == aDowncomerID;


                    if (bKeep)
                    {
                        if (rIndices != null) rIndices.Add(i);
                        _rVal.Add(v1);
                    }
                }

            }
            return _rVal;
        }



        public void Append(IEnumerable<uopPart> aPartsCol, bool bAddClones = false, string aCategory = null)
        {
            if (aPartsCol == null) return;

            foreach (var part in aPartsCol)
            {

                Add(part, bAddClones, aCategory);
            }
        }

        public void Populate(IEnumerable<uopPart> aPartsCol, bool bAddClones = false, string aCategory = null)
        {
            Clear();
            if (aPartsCol == null) return;
            Append(aPartsCol, bAddClones, aCategory);
        }

        public void AddRange(List<uopPart> aPartsCol)
        {
            if (aPartsCol == null) return;
            _Members.AddRange(aPartsCol.FindAll(x => x != null));
        }

        public void AddRange(uopParts aPartsCol)
        {
            if (aPartsCol != null) AddRange(aPartsCol.CollectionObj);
        }

   

        public int IndexOf(uopPart aMember)
        {
            if (aMember == null) return 0;
            return _Members.IndexOf(aMember) + 1;
        }

        public uopPart Insert(uopPart aPart, int aAtIndex)
        {
            if (aPart == null) return null;
            if (PartFilter != uppPartTypes.Undefined && aPart.PartType != PartFilter) return null;

            if (aAtIndex > Count || Count <= 0)
            {
                _Members.Add(aPart);
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, aPart));
                Quantity = Count;
                return SetMemberInfo(Count);
            }

            if (aAtIndex <= 1) aAtIndex = 1;
            _Members.Insert(aAtIndex - 1, aPart);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, aPart));
            Quantity = Count;
            return SetMemberInfo(aAtIndex);
        }

        public iuopBeam Beam(int aIndex, uppBeamTypes aBeamType = uppBeamTypes.Undefined)
        {
            int cnt = 0;
            uopPart aMem;
            iuopBeam aBeam;
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                if (aMem.IsBeam)
                {
                    aBeam = (iuopBeam)aMem;
                    if (aBeamType == uppBeamTypes.Undefined || (aBeam.BeamType == aBeamType))
                    {
                        cnt += 1;
                        if (cnt == aIndex) return aBeam;
                    }
                }
            }
            return null;
        }

        public uopPart Find(Predicate<uopPart> match) => _Members.Find(match);

        public int FindIndex(Predicate<uopPart> match) => _Members.FindIndex(match) + 1;
        public List<uopPart> FindAll(Predicate<uopPart> match) => _Members.FindAll(match);

        public List<uopPart> RemoveAll(Predicate<uopPart> match)
        {
            List<uopPart> _rVal = _Members.FindAll(match);
            if (_rVal == null || _rVal.Count <= 0) return _rVal;
            foreach (uopPart item in _rVal)
            {
                _Members.Remove(item);
            }

            if (MaintainIndices) Reindex();
            return _rVal;
        }

        /// <summary>
        ///returns a collection of the unique categories of the member parts
        /// </summary>
        /// <param name="aSearchCol"></param>
        /// <param name="aIncludeNullString"></param>
        /// <returns></returns>
        public List<string> Categories(List<uopPart> aSearchCol = null, bool aIncludeNullString = false)
        {
            List<String> _rVal = new List<string>();
            uopPart aPart = null;
            bool bAddIt = false;
            string aCat = string.Empty;
            List<uopPart> sCol = aSearchCol ?? CollectionObj;

            for (int i = 0; i < sCol.Count; i++)
            {
                aPart = sCol[i];
                aCat = aPart.Category;
                if (!string.IsNullOrWhiteSpace(aCat) || aIncludeNullString)
                {
                    bAddIt = true;
                    for (int j = 0; j < _rVal.Count; j++)
                    {
                        if (string.Compare(aCat, _rVal[j], StringComparison.CurrentCultureIgnoreCase) == 0)
                        {
                            bAddIt = false;
                            break;
                        }

                    }
                    if (bAddIt)
                    {
                        _rVal.Add(aCat);
                    }
                }
            }
            return _rVal;
        }



        public List<int> DowncomerIDs()
        {
            List<int> _rVal = new List<int>();
            uopPart aMem;
            int DID;
            bool bAddIt;
            try
            {
                for (int i = 1; i <= Count; i++)
                {
                    aMem = Item(i);
                    DID = aMem.DowncomerIndex;
                    if (DID > 0)
                    {
                        bAddIt = true;
                        for (int j = 1; j <= _rVal.Count; j++)
                        {
                            if (_rVal[j - 1] == DID) { bAddIt = false; break; }
                        }
                        if (bAddIt) _rVal.Add(DID);
                    }

                }
                _rVal.Sort();


            }
            catch (Exception ex)
            {
                ILoggerManager loggerManager = new LoggerManager();
                loggerManager.LogError(ex.Message);
            }

            return _rVal;
        }

        private uopPart SetMemberInfo(int aIndex, bool bSetIndex = false)
        {
            if (_Members == null) return null;
            if (aIndex < 1 || aIndex > _Members.Count) return null;
            if (_MaintainIndices || bSetIndex) _Members[aIndex - 1].Index = _BaseOne ? aIndex : aIndex - 1;
            _Members[aIndex - 1].SubPart(this);
            return _Members[aIndex - 1];
        }

        /// <summary>
        ///returns the item from the collection at the requested index ! Base 1 !
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
        /// 
        public virtual uopPart Item(int aIndex)
        {
            if (aIndex < 1 || aIndex > Count)
            {
                throw new IndexOutOfRangeException();
            }
            return SetMemberInfo(aIndex);
        }

        /// <summary>
        ///returns the item from the collection at the requested index ! Base 1 !
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
        /// 
        public virtual uopPart Item(int aIndex, bool bSuppressIndexError)
        {
            if (aIndex < 1 || aIndex > Count)
            {
                if (!bSuppressIndexError) throw new IndexOutOfRangeException(); else return null;
            }
            return SetMemberInfo(aIndex);
        }
        /// <summary>
        /// marks the indicated member as the selected part
        /// </summary>

        public virtual void SetSelected(int aIndex)
        {
            int j = SelectedIndex;
            for (int i = 1; i <= Count; i++)
            {
                _Members[i - 1].SetSelected(i == aIndex);

                if (i == aIndex)
                    j = i;
            }

            if (j == 0 && Count > 0)
            {
                _Members[0].SetSelected(true);
                j = 1;
            }

        }

        public void SetSelected(uopPart aMember)
        {
            int idx = _Members.IndexOf(aMember) + 1;

            int j = 0;
            for (int i = 1; i <= Count; i++)
            {
                _Members[i - 1].SetSelected(i == idx);

                if (i == idx)
                    j = i;
            }

            if (j == 0 && Count > 0)
            {
                _Members[0].SetSelected(true);
                j = 1;
            }

        }

        /// <summary>
        /// Get By Requested
        /// </summary>
        /// <param name="aRequestedValue"></param>
        /// <returns>returns the ranges whose requesed flag matches the passed value</returns>
        public List<uopPart> GetByRequested(bool aRequestedValue)
        {
            List<uopPart> _rVal = new List<uopPart>();
            uopPart aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                if (aMem.Requested == aRequestedValue) _rVal.Add(aMem);

            }
            return _rVal;
        }

        /// <summary>
        /// Sets the ranges requested flag to match the passed value
        /// </summary>
        /// <param name="aRequestedValue"></param>
        public void SetRequested(bool aRequestedValue)
        {
            for (int i = 1; i <= Count; i++)
            {
                _Members[i - 1].Requested = aRequestedValue;
            }
        }

        /// <summary>
        /// Get By Shell ID
        /// </summary>
        /// <param name="sShellID"></param>
        /// <returns>returns the ranges in the collection that have shell ID equal to the passed value</returns>
        public List<uopPart> GetByShellID(double sShellID)
        {
            List<uopPart> _rVal = null;
            uopPart aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                if (Math.Round(Math.Abs(aMem.ShellID - sShellID), 3) == 0) _rVal.Add(aMem);
            }
            return _rVal;
        }

        /// <summary>
        ///returns the _rVal from the collection at the requested index
        /// </summary>
        /// <param name="aIndex"></param>
        /// <param name="bReturnClone"></param>
        /// <param name="aPartIndex"></param>
        /// <returns></returns>
        public uopPart Item(int aIndex, bool bReturnClone = false, int? aPartIndex = null)
        {
            if (aIndex < 1 || aIndex > Count) throw new IndexOutOfRangeException();
            uopPart _rVal = SetMemberInfo(aIndex);
            if (_rVal == null) return null;
            if (bReturnClone) _rVal = _rVal.Clone();


            if (aPartIndex.HasValue) _rVal.PartIndex = aPartIndex.Value;

            return _rVal;
        }

        /// <summary>
        ///sets the item to the collection at the requested index ! Base 1 !
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
        /// 
        public virtual uopPart SetItem(int aIndex, uopPart aPart)
        {
            if (aIndex < 1 || aIndex > Count || aPart == null) throw new IndexOutOfRangeException();
            _Members[aIndex - 1] = aPart;
            if (_MaintainIndices) _Members[aIndex - 1].Index = _BaseOne ? aIndex : aIndex - 1;
            _Members[aIndex - 1].SubPart(this);
            return _Members[aIndex - 1];
        }

        public void Reindex() { for (int i = 1; i <= Count; i++) { SetMemberInfo(i, true); } }

        /// <summary>
        ///removes the item to the collection at the requested index ! Base 1 !
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
        /// 
        public virtual uopPart Remove(int aIndex)
        {
            if (aIndex < 1 || aIndex > Count) return null;
            uopPart _rVal = SetMemberInfo(aIndex);
            _Members.Remove(_rVal);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action: NotifyCollectionChangedAction.Remove, _rVal));
            if (_MaintainIndices) Reindex();
            Quantity = Count;
            return _rVal;
        }

        /// <summary>
        ///removes the items from the collection at the requested indices ! Base 1 !
        /// </summary>
        /// <param name="aIndices"></param>
        /// <returns></returns>
        /// 
        public virtual List<uopPart> Remove(List<int> aIndices)
        {
            List<uopPart> _rVal = new List<uopPart>();
            if (aIndices == null || _Members == null) return _rVal;
            if (aIndices.Count <= 0 || _Members.Count <= 0) return _rVal;

            List<uopPart> newmems = new List<uopPart>();
            uopPart mem;
            for (int i = 1; i <= Count; i++)
            {
                mem = Item(i);
                if (aIndices.IndexOf(i) >= 0)
                {
                    if (_rVal.IndexOf(mem) < 0) _rVal.Add(mem);
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action: NotifyCollectionChangedAction.Remove, mem));
                }
                else
                {
                    newmems.Add(mem);
                }
            }

            if (_rVal.Count > 0)
            {
                if (_MaintainIndices) Reindex();
                Quantity = Count;
            }

            _Members = newmems;
            return _rVal;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>

        public List<string> Paths()
        {
            List<string> _rVal = new List<string>();
            uopPart aMem;
            string aStr = string.Empty;
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                aStr = aMem.PartPath();
                _rVal.Add(aStr);
            }
            return _rVal;
        }

        /// <summary>
        ///returns a collection with all the part numbers of all the members
        /// </summary>
        /// <param name="bUniqueOnly"></param>
        /// <param name="rIndexes"></param>
        /// <param name="aSearchCol"></param>
        /// <returns></returns>
        public List<string> PartNumbersCol(bool bUniqueOnly, out List<int> rIndexes, uopParts aSearchCol = null)
        {
            List<string> _rVal = new List<string>();
            rIndexes = new List<int>();
            uopPart aMem;
            string aPn = string.Empty;
            bool bKeep = false;
            uopParts sCol = aSearchCol ?? this;

            for (int i = 1; i <= sCol.Count; i++)
            {
                aMem = Item(i);

                aPn = aMem.PartNumber;
                if (aPn !=  string.Empty)
                {
                    bKeep = true;
                    if (bUniqueOnly)
                    {
                        for (int j = 0; j < _rVal.Count; j++)
                        {
                            if (_rVal[j] == aPn)
                            {
                                bKeep = false;
                                break;
                            }
                        }

                        if (bKeep)
                        {
                            _rVal.Add(aPn);
                            rIndexes.Add(i);
                        }
                    }
                }
            }
            return _rVal;
        }

        public override void UpdatePartWeight()
        {
            double tot = 0;
            foreach (uopPart mem in _Members) { mem.UpdatePartWeight(); tot += mem.Weight; }
            base.Weight = tot;
        }

        public virtual void Clear(bool bSuppressEvents = false)
        {
            if (_Members == null) return;
            if (_Members.Count <= 0) return;
            if (!bSuppressEvents) CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action: NotifyCollectionChangedAction.Reset));
            _Members.Clear();
        }


        public void SetMembers(List<uopPart> aMembers, bool bSuppressEvents = false)
        {
            if (aMembers == null) { Clear(); return; }
            if (!bSuppressEvents) CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action: NotifyCollectionChangedAction.Reset));
            _Members = aMembers;
        }

        public override void SubPart(uopPart aPart, string aCategory = null, bool? bHidden = null)
        {
            if (aPart != null) base.SubPart(aPart, aCategory, bHidden);


            // TPART prt = aPart.Structure;
            // string cat = aCategory;
            // dynamic hid = bHidden;
            //base.SubPart(prt, cat);



        }

        public void WriteToFile(string aFileName = "", StreamWriter aStream = null)
        {

            try
            {

                if (string.IsNullOrWhiteSpace(aFileName)) throw new Exception("[uopParts.WriteToFile] Null filename detected");

                uopPart aMem;
                StreamWriter bStrm = aStream;
                bool bMyStream = false;
                string aStr = string.Empty;

                aFileName = aFileName.Trim();


                for (int i = 1; i <= Count; i++)
                {
                    aMem = Item(i);
                    aStr = aMem.PartPath();
                    if (bStrm != null)
                    {
                        bStrm.WriteLine(aStr);
                    }
                    else
                    {
                        Debug.Print(aStr);
                    }
                }

                if (bMyStream) bStrm.Close();


            }
            catch (Exception e) { throw e; }

        }

        /// <summary>
        /// Returns Ordinates
        /// </summary>
        /// <param name="aOrdinateType"></param>
        /// <param name="aPrecis"></param>
        /// <param name="bNoDupes"></param>
        /// <param name="aPartType"></param>
        /// <param name="aPanelID"></param>
        /// <param name="aDowncomerID"></param>
        /// <returns></returns>
        public mzValues Ordinates(dxxOrdinateDescriptors aOrdinateType,
            int aPrecis = -1, bool bNoDupes = false,
            uppPartTypes aPartType = uppPartTypes.Undefined,
            int aPanelID = 0, int aDowncomerID = 0)
         => new mzValues(uopParts.Ordinates(_Members, aOrdinateType, aPrecis, bNoDupes, aPartType, aPanelID, aDowncomerID));

        /// <summary>
        ///returns the requested ordinates of the indicated members.
        /// </summary>
        /// <param name="aOrdinateType">ordinate type type parameter</param>
        /// <param name="aPrecis">precision to round the return values to</param>
        /// <param name="bNoDupes">flag to return only the uniq values</param>
        /// <param name="aPartType"></param>
        /// <param name="aPanelID"></param>
        /// <param name="aDowncomerID"></param>
        /// <param name="aCollector"></param>
        /// <returns></returns>
        internal TVALUES OrdinatesV(dxxOrdinateDescriptors aOrdinateType,
             int aPrecis = -1, bool bNoDupes = false, uppPartTypes aPartType = uppPartTypes.Undefined,
             int aPanelID = 0, int aDowncomerID = 0, mzValues aCollector = null)
        {

            return uopParts.Ordinates(_Members, aOrdinateType, aPrecis, bNoDupes, aPartType, aPanelID, aDowncomerID, aCollector);

        }

        /// <summary>
        ///returns a memebr from the collection whose properties or position in the 
        /// collection match the passed control flag
        /// </summary>
        /// <param name="aFilter">flag indicating what type of vector to search for</param>
        /// <param name="aOrdinate">the ordinate to search for if the search is ordinate specific</param>
        /// <param name="aPrecis">precision for numerical comparison (1 to 8)</param>
        /// <param name="rIndex"></param>
        /// <param name="bRemove"></param>
        /// <param name="aPartType"></param>
        /// <param name="aPanelID"></param>
        /// <param name="aDowncomerID"></param>
        /// <param name="bReturnClone"></param>
        /// <returns></returns>
        public uopPart GetByPoint(dxxPointFilters aFilter,
            double aOrdinate = 0, int aPrecis = 3,
            bool bRemove = false, uppPartTypes aPartType = uppPartTypes.Undefined,
            int aPanelID = 0, int aDowncomerID = 0)
        {


            aPrecis = mzUtils.LimitedValue(aPrecis, 1, 10);

            // search for members at extremes
            /////returns the first one that satisfies

            colDXFVectors pCenters = CentersDXF(false, aPartType, aPanelID, aDowncomerID);
            dxfVector v1 = pCenters.GetVector(aFilter, aOrdinate, aPrecis: aPrecis);
            if (v1 == null) return null;
            int idx = (int)(v1.Value);
            return (!bRemove) ? Item(idx, bSuppressIndexError: true) : Remove(idx);

        }

        /// <summary>
        ///returns the members from the collection whose specified point match the passed search criteria
        /// </summary>
        /// <param name="aFilter">point type type parameter</param>
        /// <param name="aOrdinate">search type parameter</param>
        /// <param name="bOnIsIn">the ordinate to search for if the search is ordinate specific</param>
        /// <param name="aIndices">flag indicating if equal values should be returned</param>
        /// <param name="bReturnClones">returns the indices of the matches</param>
        /// <param name="bRemove">flag to return copies</param>
        /// <param name="aPrecis">flag to remove the matching set</param>
        /// <param name="aPartType"></param>
        /// <param name="aPanelID">precision for numerical comparison (1 to 8)</param>
        /// <param name="aDowncomerID"></param>
        /// <returns></returns>
        public List<uopPart> GetByPoints(dxxPointFilters aFilter,
            double aOrdinate = 0, bool bOnIsIn = true,
            List<int> rIndices = null, bool bReturnClones = false, bool bRemove = false, int aPrecis = 3,
            uppPartTypes aPartType = uppPartTypes.Undefined, int aPanelID = 0, int aDowncomerID = 0)
        {
            List<uopPart> _rVal = new List<uopPart>();

            colDXFVectors pCenters = null;
            List<dxfVector> rCenters = null;
            dxfVector v1 = null;
            int idx = 0;
            rIndices = new List<int>();
            pCenters = CentersDXF(false, aPartType, aPanelID, aDowncomerID);

            rCenters = pCenters.GetVectors(aFilter: aFilter, aOrdinate: aOrdinate, bOnIsIn: bOnIsIn, aPlane: null, aPrecis: aPrecis);
            uopPart aMem;
            for (int i = 1; i <= rCenters.Count; i++)
            {
                v1 = rCenters[i - 1];
                idx = mzUtils.VarToInteger(v1.Value);
                if (idx >= 1 && idx <= Count)
                {
                    aMem = SetMemberInfo(idx);
                    if (bReturnClones) aMem = aMem.Clone();

                    if (aMem != null) { rIndices.Add(idx); _rVal.Add(aMem); }

                }
            }

            if (bRemove & rIndices.Count > 0)
            {

                foreach (var item in _rVal)
                {
                    _Members.Remove(item);
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action: NotifyCollectionChangedAction.Remove, item));
                }

                Reindex();
            }


            return _rVal;
        }

        /// <summary>
        ///returns the members from the collection whose specified point match the passed search criteria
        /// </summary>
        /// <param name="aOrdinateType">ordinate type type parameter</param>
        /// <param name="aOrdinate">the ordinate to search for if the search is ordinate specific</param>
        /// <param name="aPartToSkip"></param>
        /// <param name="rIndices">returns the indices of the matches</param>
        /// <param name="bReturnClones">flag to return copies</param>
        /// <param name="bRemove">flag to remove the matching set</param>
        /// <param name="aPrecis">precision for numerical comparison (1 to 8)</param>
        /// <param name="aPartType"></param>
        /// <param name="aPanelID"></param>
        /// <param name="aDowncomerID"></param>
        /// <returns></returns>
        public List<uopPart> GetByOrdinate(dxxOrdinateDescriptors aOrdinateType,
            double aOrdinate, uopPart aPartToSkip = null,
            bool bReturnClones = false, bool bRemove = false, int aPrecis = 3, uppPartTypes aPartType = uppPartTypes.Undefined,
            int aPanelID = 0, int aDowncomerID = 0)
        {
            return GetByOrdinate(aOrdinateType, aOrdinate, aPartToSkip, out List<int> _, bReturnClones, bRemove, aPrecis, aPartType, aPanelID, aDowncomerID);
        }
        /// <summary>
        ///returns the members from the collection whose specified point match the passed search criteria
        /// </summary>
        /// <param name="aOrdinateType">ordinate type type parameter</param>
        /// <param name="aOrdinate">the ordinate to search for if the search is ordinate specific</param>
        /// <param name="aPartToSkip"></param>
        /// <param name="rIndices">returns the indices of the matches</param>
        /// <param name="bReturnClones">flag to return copies</param>
        /// <param name="bRemove">flag to remove the matching set</param>
        /// <param name="aPrecis">precision for numerical comparison (1 to 8)</param>
        /// <param name="aPartType"></param>
        /// <param name="aPanelID"></param>
        /// <param name="aDowncomerID"></param>
        /// <returns></returns>
        public List<uopPart> GetByOrdinate(dxxOrdinateDescriptors aOrdinateType,
        double aOrdinate, uopPart aPartToSkip, out List<int> rIndices,
        bool bReturnClones = false, bool bRemove = false, int aPrecis = 3, uppPartTypes aPartType = uppPartTypes.Undefined,
        int aPanelID = 0, int aDowncomerID = 0)
        {
            List<uopPart> _rVal = new List<uopPart>();
            rIndices = new List<int>();
            aPrecis = mzUtils.LimitedValue(aPrecis, 1, 8);

            uopPart aMem;

            double cmp = 0;
            bool bKeep = false;
            double dif = 0;

            cmp = Math.Round(aOrdinate, aPrecis);

            for (int i = 1; i <= Count; i++)
            {
                aMem = _Members[i - 1];
                if (aOrdinateType == dxxOrdinateDescriptors.Z)
                { dif = aMem.Z - aOrdinate; }
                else if (aOrdinateType == dxxOrdinateDescriptors.Y)
                { dif = aMem.Y - aOrdinate; }
                else
                { dif = aMem.X - aOrdinate; }
                bKeep = Math.Round(dif, aPrecis) == 0;
                if (bKeep)
                {
                    bKeep = bKeep && (aPartType == uppPartTypes.Undefined || aMem.PartType == aPartType);
                    bKeep = bKeep && (aPanelID <= 0 || aMem.PanelIndex == aPanelID);
                    bKeep = bKeep && (aDowncomerID <= 0 || aMem.DowncomerIndex == aDowncomerID);
                    if (bKeep && aPartToSkip != null)
                    {
                        if (aMem == aPartToSkip) bKeep = false;
                    }
                    //bKeep = (bKeep && (aPartToSkip == null ||(aPartToSkip !=null && aMem == aPartToSkip)));
                    if (bKeep) aMem = SetMemberInfo(i);
                    if (bReturnClones) { aMem = aMem.Clone(); bKeep = aMem != null; }
                    if (bKeep)
                        _rVal.Add(aMem);
                }

            }
            if (bRemove & rIndices.Count > 0)
            {
                foreach (var item in _rVal)
                {
                    _Members.Remove(item);
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action: NotifyCollectionChangedAction.Remove, item));
                }

                Reindex();
            }

            return _rVal;
        }


        /// <summary>
        ///returns the members from the collection whose specified point match the passed search criteria
        /// </summary>
        /// <param name="aMarkValue">ordinate type type parameter</param>
        /// <param name="aPartToSkip">the ordinate to search for if the search is ordinate specific</param>
        /// <param name="rIndices">returns the indices of the matches</param>
        /// <param name="bReturnClones">flag to return copies</param>
        /// <param name="bRemove">flag to remove the matching set</param>
        /// <param name="aPartType">a precision for numerical comparison (1 to 8)</param>
        /// <param name="aPanelID"></param>
        /// <param name="aDowncomerID"></param>
        /// <returns></returns>
        public List<uopPart> GetByMark(bool aMarkValue, uopPart aPartToSkip = null,
             bool bReturnClones = false, bool bRemove = false,
            uppPartTypes aPartType = uppPartTypes.Undefined, int aPanelID = 0, int aDowncomerID = 0)
        {
            List<uopPart> _rVal = new List<uopPart>();

            uopPart aMem = null;
            bool bKeep = false;

            for (int i = 1; i <= Count; i++)
            {
                aMem = _Members[i - 1];

                bKeep = aMem.Mark == aMarkValue;
                bKeep = bKeep && (aPartType == uppPartTypes.Undefined || aMem.PartType == aPartType);
                bKeep = bKeep && (aPanelID <= 0 || aMem.PanelIndex == aPanelID);
                bKeep = bKeep && (aDowncomerID <= 0 || aMem.DowncomerIndex == aDowncomerID);
                bKeep = bKeep && (aPartToSkip == null || aMem == aPartToSkip);
                if (bKeep) aMem = SetMemberInfo(i);
                if (bReturnClones) { aMem = aMem.Clone(); bKeep = aMem != null; }
                if (bKeep)
                {
                    _rVal.Add(aMem);

                }


            }
            if (bRemove)
            {
                foreach (var item in _rVal)
                {
                    _Members.Remove(item);
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action: NotifyCollectionChangedAction.Remove, item));
                }

                Reindex();
            }
            return _rVal;
        }


        /// <summary>
        /// reorders the members based on their ordinate values
        /// </summary>
        /// <param name="aOrdinateType">ordinate type type parameter</param>
        /// <param name="bLowToHigh">flag to control the sort order (high to low or Low to High)</param>
        /// <param name="bNoDupes">flag to remove any at the same ordinate</param>
        /// <param name="aPrecis">precision for numerical comparison (1 to 16)</param>
        public void SortByOrdinate(dxxOrdinateDescriptors aOrdinateType, bool bLowToHigh = false, bool bNoDupes = false, int aPrecis = -1)
        {
            if (Count <= 1) return;
            aPrecis = mzUtils.LimitedValue(aPrecis, 1, 15);
            TVALUES aOrds = new TVALUES();
            TVALUES aIds = new TVALUES();


            List<uopPart> _rVal = new List<uopPart>();

            aOrds = uopParts.Ordinates(_Members, aOrdinateType, aPrecis, bNoDupes);

            aIds = aOrds.SortWithIDs(bReturnBaseOne: true, !bLowToHigh);


            for (int i = 1; i <= aIds.Count; i++)
            {
                _rVal.Add(Item(aIds.Item(i)));
            }
            _Members = _rVal;

        }


        /// <summary>
        ///returns the members from the collection whose specified point match the passed search criteria
        /// </summary>
        /// <param name="aOrdinateType">ordinate type type parameter</param>
        /// <param name="aLowOrdinate">he low ordinate to search for if the search is ordinate specific</param>
        /// <param name="aHighOrdinate">the high ordinate to search for if the search is ordinate specific</param>
        /// <param name="bOnIsIn">flag indicating that equal values are considered matching</param>
        /// <param name="aPartToSkip"></param>
        /// <param name="rIndices">5returns the indices of the matches</param>
        /// <param name="bReturnClones">flag to return copies</param>
        /// <param name="bRemove">flag to remove the matching set</param>
        /// <param name="aPrecis">precision for numerical comparison (1 to 8)</param>
        /// <param name="aPartType"></param>
        /// <param name="aPanelID"></param>
        /// <param name="aDowncomerID"></param>
        /// <returns></returns>
        public List<uopPart> GetBetweenOrdinate(dxxOrdinateDescriptors aOrdinateType,
            double aLowOrdinate, double aHighOrdinate, bool bOnIsIn = false,
            uopPart aPartToSkip = null, bool bReturnClones = false, bool bRemove = false,
            int aPrecis = 3, uppPartTypes aPartType = uppPartTypes.Undefined,
            int aPanelID = 0, int aDowncomerID = 0)
        {
            return GetBetweenOrdinate(aOrdinateType, aLowOrdinate, aHighOrdinate, bOnIsIn, aPartToSkip, out List<int> _, bReturnClones, bRemove, aPrecis, aPartType, aPanelID, aDowncomerID);
        }

        /// <summary>
        ///returns the members from the collection whose specified point match the passed search criteria
        /// </summary>
        /// <param name="aOrdinateType">ordinate type type parameter</param>
        /// <param name="aLowOrdinate">he low ordinate to search for if the search is ordinate specific</param>
        /// <param name="aHighOrdinate">the high ordinate to search for if the search is ordinate specific</param>
        /// <param name="bOnIsIn">flag indicating that equal values are considered matching</param>
        /// <param name="aPartToSkip"></param>
        /// <param name="rIndices">5returns the indices of the matches</param>
        /// <param name="bReturnClones">flag to return copies</param>
        /// <param name="bRemove">flag to remove the matching set</param>
        /// <param name="aPrecis">precision for numerical comparison (1 to 8)</param>
        /// <param name="aPartType"></param>
        /// <param name="aPanelID"></param>
        /// <param name="aDowncomerID"></param>
        /// <returns></returns>
        public List<uopPart> GetBetweenOrdinate(dxxOrdinateDescriptors aOrdinateType,
        double aLowOrdinate, double aHighOrdinate, bool bOnIsIn,
        uopPart aPartToSkip, out List<int> rIndices, bool bReturnClones = false, bool bRemove = false,
        int aPrecis = 3, uppPartTypes aPartType = uppPartTypes.Undefined,
        int aPanelID = 0, int aDowncomerID = 0)
        {
            List<uopPart> _rVal = new List<uopPart>();
            rIndices = new List<int>();
            aPrecis = mzUtils.LimitedValue(aPrecis, 1, 8);
            uopPart aMem;
            double cpmto = 0;
            bool bKeep = false;
            double aLow = Math.Round(aLowOrdinate, aPrecis);
            double aHigh = Math.Round(aHighOrdinate, aPrecis);

            mzUtils.SortTwoValues(true, ref aLow, ref aHigh);

            for (int i = 1; i <= Count; i++)
            {
                aMem = _Members[i - 1];
                if (aOrdinateType == dxxOrdinateDescriptors.Z)
                { cpmto = Math.Round(aMem.Z, aPrecis); }
                else if (aOrdinateType == dxxOrdinateDescriptors.Y)
                { cpmto = Math.Round(aMem.Y, aPrecis); }
                else
                { cpmto = Math.Round(aMem.Z, aPrecis); }

                bKeep = bOnIsIn ? (cpmto >= aLow && cpmto <= aHigh) : (cpmto > aLow && cpmto < aHigh);

                bKeep = bKeep && (aPartType == uppPartTypes.Undefined || aMem.PartType == aPartType);
                bKeep = bKeep && (aPanelID <= 0 || aMem.PanelIndex == aPanelID);
                bKeep = bKeep && (aDowncomerID <= 0 || aMem.DowncomerIndex == aDowncomerID);
                bKeep = bKeep && (aPartToSkip == null || aMem == aPartToSkip);
                if (bKeep) aMem = SetMemberInfo(i);
                if (bReturnClones) { aMem = aMem.Clone(); bKeep = aMem != null; }
                if (bKeep) _rVal.Add(aMem);
            }
            if (bRemove & rIndices.Count > 0)
            {
                foreach (var item in _rVal)
                {
                    _Members.Remove(item);
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action: NotifyCollectionChangedAction.Remove, item));
                }

                Reindex();
            }

            return _rVal;
        }

        /// <summary>
        ///returns the members from the collection whose specified ordinate is greater than or equal to the passed value
        /// </summary>
        /// <param name="aOrdinateType">ordinate type type parameter</param>
        /// <param name="aOrdinate">the low ordinate to search for if the search is ordinate specific</param>
        /// <param name="bOnIsIn">the high ordinate to search for if the search is ordinate specific</param>
        /// <param name="aPartToSkip">flag indicating that equal values are considered matching</param>
        /// <param name="rIndices">returns the indices of the matches</param>
        /// <param name="bReturnClones">flag to return copies</param>
        /// <param name="bRemove">flag to remove the matching set</param>
        /// <param name="aPrecis"> precision for numerical comparison (1 to 8)</param>
        /// <param name="aPartType"></param>
        /// <param name="aPanelID"></param>
        /// <param name="aDowncomerID"></param>
        /// <returns></returns>
        public List<uopPart> GetAboveOrdinate(dxxOrdinateDescriptors aOrdinateType,
            double aOrdinate, bool bOnIsIn = true, uopPart aPartToSkip = null,
            List<int> rIndices = null, bool bReturnClones = false, bool bRemove = false,
            int aPrecis = 3, uppPartTypes aPartType = uppPartTypes.Undefined, int aPanelID = 0, int aDowncomerID = 0)
        {
            List<uopPart> _rVal = new List<uopPart>();
            rIndices = new List<int>();
            aPrecis = mzUtils.LimitedValue(aPrecis, 1, 8);

            uopPart aMem;

            double cpmto;
            double cmp = Math.Round(aOrdinate, aPrecis);
            bool bKeep = false;

            for (int i = 1; i <= Count; i++)
            {
                aMem = _Members[i - 1];
                if (aOrdinateType == dxxOrdinateDescriptors.Z)
                { cpmto = Math.Round(aMem.Z, aPrecis); }
                else if (aOrdinateType == dxxOrdinateDescriptors.Y)
                { cpmto = Math.Round(aMem.Y, aPrecis); }
                else
                { cpmto = Math.Round(aMem.Z, aPrecis); }
                bKeep = bOnIsIn ? (cpmto >= cmp) : (cpmto > cmp);

                bKeep = bKeep && (aPartType == uppPartTypes.Undefined || aMem.PartType == aPartType);
                bKeep = bKeep && (aPanelID <= 0 || aMem.PanelIndex == aPanelID);
                bKeep = bKeep && (aDowncomerID <= 0 || aMem.DowncomerIndex == aDowncomerID);
                bKeep = bKeep && (aPartToSkip == null || aMem == aPartToSkip);
                if (bKeep) aMem = SetMemberInfo(i);
                if (bReturnClones) { aMem = aMem.Clone(); bKeep = aMem != null; }
                if (bKeep) _rVal.Add(aMem);
            }
            if (bRemove & rIndices.Count > 0)
            {
                foreach (var item in _rVal)
                {
                    _Members.Remove(item);
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action: NotifyCollectionChangedAction.Remove, item));
                }

                Reindex();
            }
            return _rVal;
        }

        /// <summary>
        ///returns the members from the collection whose specified ordinate is greater than or equal to the passed value
        /// </summary>
        /// <param name="aOrdinateType">ordinate type type parameter</param>
        /// <param name="aOrdinate">the low ordinate to search for if the search is ordinate specific</param>
        /// <param name="bOnIsIn">the high ordinate to search for if the search is ordinate specific</param>
        /// <param name="aPartToSkip">flag indicating that equal values are considered matching</param>
        /// <param name="rIndices">returns the indices of the matches</param>
        /// <param name="bReturnClones">flag to return copies</param>
        /// <param name="bRemove">flag to remove the matching set</param>
        /// <param name="aPrecis"> precision for numerical comparison (1 to 8)</param>
        /// <param name="aPartType"></param>
        /// <param name="aPanelID"></param>
        /// <param name="aDowncomerID"></param>
        /// <returns></returns>
        public List<uopPart> GetBelowOrdinate(dxxOrdinateDescriptors aOrdinateType,
            double aOrdinate, bool bOnIsIn = true, uopPart aPartToSkip = null,
            List<int> rIndices = null, bool bReturnClones = false, bool bRemove = false,
            int aPrecis = 3, uppPartTypes aPartType = uppPartTypes.Undefined, int aPanelID = 0, int aDowncomerID = 0)
        {
            List<uopPart> _rVal = new List<uopPart>();
            rIndices = new List<int>();
            aPrecis = mzUtils.LimitedValue(aPrecis, 1, 8);

            uopPart aMem;

            double cpmto;
            double cmp = Math.Round(aOrdinate, aPrecis);
            bool bKeep = false;

            for (int i = 1; i <= Count; i++)
            {
                aMem = _Members[i - 1];
                if (aOrdinateType == dxxOrdinateDescriptors.Z)
                { cpmto = Math.Round(aMem.Z, aPrecis); }
                else if (aOrdinateType == dxxOrdinateDescriptors.Y)
                { cpmto = Math.Round(aMem.Y, aPrecis); }
                else
                { cpmto = Math.Round(aMem.Z, aPrecis); }
                bKeep = bOnIsIn ? (cpmto <= cmp) : (cpmto < cmp);

                bKeep = bKeep && (aPartType == uppPartTypes.Undefined || aMem.PartType == aPartType);
                bKeep = bKeep && (aPanelID <= 0 || aMem.PanelIndex == aPanelID);
                bKeep = bKeep && (aDowncomerID <= 0 || aMem.DowncomerIndex == aDowncomerID);
                bKeep = bKeep && (aPartToSkip == null || aMem == aPartToSkip);
                if (bKeep) aMem = SetMemberInfo(i);
                if (bReturnClones) { aMem = aMem.Clone(); bKeep = aMem != null; }
                if (bKeep) _rVal.Add(aMem);
            }
            if (bRemove & rIndices.Count > 0)
            {
                foreach (var item in _rVal)
                {
                    _Members.Remove(item);
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action: NotifyCollectionChangedAction.Remove, item));
                }

                Reindex();
            }
            return _rVal;
        }

        /// <summary>
        /// Returns hole count
        /// </summary>
        /// <param name="aHoleTag"></param>
        /// <param name="aHoleType"></param>
        /// <returns></returns>
        public int HoleCount(string aHoleTag, uppHoleTypes aHoleType = uppHoleTypes.Any)
        {
            int _rVal = 0;
            uopHoleArray memHoles;
            uopPart aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = _Members[i - 1];

                memHoles = aMem.HoleArray();
                if (memHoles != null) _rVal += memHoles.MemberCount(aHoleTag, aHoleType);
            }
            return _rVal;
        }

        /// <summary>
        /// Sets part index
        /// </summary>
        /// <param name="bReverseOrder"></param>
        public void SetPartIndexs(bool bReverseOrder = false)
        {


            if (!bReverseOrder)
            {
                for (int i = 1; i <= Count; i++) { _Members[i - 1].PartIndex = i; }
            }
            else
            {
                for (int i = Count; i >= 1; i--) { _Members[i - 1].PartIndex = i; }
            }
        }

        ///returns a member from the collection whose center is the passed point
        /// the point must actually be the member center not just equivalent to the member center coordinates
        /// </summary>
        /// <param name="aCenter">1the center to search for</param>
        /// <param name="rIndex"></param>
        /// <param name="aPartType"></param>
        /// <param name="aPrecis"></param>
        /// <returns></returns>
        public uopPart GetByCenter(iVector aCenter, uppPartTypes? aPartType = null, int aPrecis = 3, bool bReturnClones = false)
        {

           
            if (aCenter == null) return null;
            aPrecis = mzUtils.LimitedValue(aPrecis, 0, 6);
            
            double d1 = 0;
            foreach (var item in _Members)
            {
                if (aPartType.HasValue&& item.PartType != aPartType.Value) continue;
                    d1 = Math.Round(item.CenterDXF.DistanceTo(aCenter), aPrecis);
                    if (d1 <= 0) return SetMemberInfo(item.PartIndex);
               
            }
            
            return null;
        }

        ///returns a member from the collection whose center is the passed point
        /// the point must actually be the member center not just equivalent to the member center coordinates
        /// </summary>
        /// <param name="aCenter">1the center to search for</param>
        /// <param name="rIndex"></param>
        /// <param name="aPartType"></param>
        /// <param name="aPrecis"></param>
        /// <returns></returns>
        public uopPart GetByCenter(iVector aCenter, out int rIndex, uppPartTypes aPartType = uppPartTypes.Undefined, int aPrecis = 3, bool bReturnClones = false)
        {

            rIndex = 0;
            if (aCenter == null) return null;
            aPrecis = mzUtils.LimitedValue(aPrecis, 0, 6);
            uopPart aMem;
            dxfVector v1;
            double d1 = 0;
            for (int i = 1; i <= Count; i++)
            {
                aMem = _Members[i - 1];
                if (aPartType == uppPartTypes.Undefined || aMem.PartType == aPartType)
                {
                    v1 = aMem.CenterDXF;

                    d1 = Math.Round(v1.DistanceTo(aCenter), aPrecis);
                    if (d1 <= 0) { rIndex = i; return SetMemberInfo(i); }

                }
            }
            return null;
        }

     

        /// <summary>
        ///returns a stiffener from the collection whose center is nearest to the passed point
        /// </summary>
        /// <param name="aCenter">the center to search for</param>
        /// <param name="rIndex">an optional collection of parts to search other than the current collection</param>
        /// <param name="aPartType"></param>
        /// <returns></returns>
        public uopPart GetByNearestCenter(dxfVector aCenter, uppPartTypes aPartType = uppPartTypes.Undefined)
        { return (aCenter == null) ? null : GetByCenter(CentersDXF().NearestVector(aCenter), aPartType); }

        public uopPart GetEqualPart(uopPart aPart)
        {
            if (aPart == null || Count <= 0) return null;
            return _Members.Find(x => x.IsEqual(aPart));
        }

        /// <summary>
        ///returns the part from the collection whose center properties or position in the collection match the passed control flag
        /// </summary>
        /// <param name="aControlFlag">flag indicating what type of search to employ</param>
        /// <param name="aOrdinate">the ordinate to search for if the search is ordinate specific</param>
        /// <param name="aPrecis">an optional coordinate system to use</param>
        /// <param name="aPartType">part type filter</param>
        /// <param name="rIndex">returns the index of the matching part</param>
        /// <param name="bReturnClones">flag to return a clone</param>
        /// <param name="bRemove">flag to remove the member from the collection</param>
        /// <returns></returns>
        public uopPart GetPart(dxxPointFilters aControlFlag, double aOrdinate = 0,
            int aPrecis = 3, uppPartTypes? aPartType = null, 
            bool bReturnClones = false, bool bRemove = false)
        {
            uopVector v1 = Centers(aPartType:aPartType).GetVector(aControlFlag, aOrdinate: aOrdinate, aPrecis: aPrecis);

            if (v1 == null) return null;
            uopPart _rVal = GetByCenter(v1, aPartType);
            if (_rVal == null) return null;
            if (bRemove)  _Members.Remove(_rVal);
            if (bReturnClones)  _rVal = _rVal.Clone(); 

            return _rVal;
        }


        /// <summary>
        /// Gets first part
        /// </summary>
        /// <param name="aPartType"></param>
        /// <param name="aReturnClone"></param>
        /// <param name="aRangeID"></param>
        /// <returns></returns>
        public uopPart FirstPart(uppPartTypes aPartType, bool aReturnClone = false, int aRangeID = 0)
        {

            uopPart aMem;

            for (int i = 1; i <= Count; i++)
            {
                aMem = _Members[i - 1];
                if (aMem.PartType == aPartType)
                {
                    if (aRangeID <= 0 || (aRangeID > 0 & aMem.RangeIndex == aRangeID))
                    {
                        aMem = SetMemberInfo(i);
                        return (!aReturnClone) ? aMem : aMem.Clone();
                    }
                }

            }
            return null;
        }


        /// <summary>
        ///returns a member from the collection whose center is the passed point
        /// the point must actually be the member center not just equivalent to the member center coordinates
        /// </summary>
        /// <param name="aCenter">the center to search for</param>
        /// <param name="rIndex">an optional collection of member to search other than the current collection</param>
        /// <param name="aPartType"></param>
        /// <param name="aPrecis"></param>
        /// <returns></returns>
        public uopPart GetAtCenter(dxfVector aCenter, out int rIndex,
            uppPartTypes aPartType = uppPartTypes.Undefined, int aPrecis = 3, bool bIgnoreZ = false)
        {
            rIndex = 0;


            if (aCenter == null) return null;
            aPrecis = mzUtils.LimitedValue(aPrecis, 1, 8);
            uopPart aMem;
            dxfVector v1;
            dxfVector cp;
            dynamic zVal = null;
            if (bIgnoreZ)
            {
                zVal = 0;
                cp = new dxfVector(aCenter.X, aCenter.Y);
            }
            else { cp = aCenter; }

            for (int i = 1; i <= Count; i++)
            {
                aMem = _Members[i - 1];
                if (aPartType == uppPartTypes.Undefined ||
                    (aPartType != uppPartTypes.Undefined && aMem.PartType == aPartType))
                {
                    v1 = aMem.Center.ToDXFVector(aZ: zVal);
                    if (v1.IsEqual(cp, aPrecis))
                    {
                        rIndex = i;
                        return SetMemberInfo(i);

                    }

                }

            }
            return null;
        }

        /// <summary>
        ///returns all the members whose part category matches the passed string
        /// </summary>
        /// <param name="aCategory"></param>
        /// <returns></returns>
        public List<uopPart> GetByCategory(string aCategory)
        {
            List<uopPart> _rVal = new List<uopPart>();
            uopPart aMem;

            for (int i = 1; i <= Count; i++)
            {
                aMem = _Members[i - 1];
                if (string.Compare(aMem.Category, aCategory, StringComparison.OrdinalIgnoreCase) == 0) _rVal.Add(SetMemberInfo(i));

            }
            return _rVal;
        }

        /// <summary>
        ///returns all the members whose part number matches the passed string
        /// </summary>
        /// <param name="aPartNumber"></param>
        /// <param name="aSearchCol"></param>
        /// <returns></returns>
        public List<uopPart> GetByPartNumber(string aPartNumber, bool bJustOne = false)
        {
            List<uopPart> _rVal = new List<uopPart>();
            uopPart aMem;
            int i;

            for (i = 1; i <= Count; i++)
            {
                aMem = _Members[i - 1];
                if (string.Compare(aMem.PartNumber, aPartNumber, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    _rVal.Add(SetMemberInfo(i));
                    if (bJustOne) break;
                }

            }
            return _rVal;
        }

        /// <summary>
        ///returns the parts from the collection whose centers are members of the passed collection of centers
        /// </summary>
        /// <param name="aPartCenters">the centers to use in the search
        /// <returns></returns>
        public List<uopPart> GetByCenters(IEnumerable<iVector> aPartCenters, bool bReturnClones = false)
        {

            if (aPartCenters == null || Count <= 0) return null;
            List<uopPart> _rVal = new List<uopPart>();

            foreach (iVector v in aPartCenters)
            {
                uopPart part = GetByCenter(v, bReturnClones: bReturnClones);
                if (part == null) continue;
                _rVal.Add(part);
            }
            return _rVal;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Filter">the filter to apply to the current collection</param>
        /// <param name="Ordinate">the ordinate to use in the search</param>
        /// <param name="OnIsIn">flag indicating if parts with ordinates equal to the passed ordinate should be included or excluded in the </param>
        /// <returns>an optional collection of parts to search</returns>
        public List<uopPart> GetParts(dxxPointFilters Filter, double Ordinate = 0, bool OnIsIn = true, bool bReturnClones = false)
        => GetByCenters(Centers().GetVectors(aFilter: Filter, aOrdinate: Ordinate, bOnIsIn: OnIsIn), bReturnClones );

        /// <summary>
        ///returns the collection of members in the collection whose downcomer index equals the passed value
        /// </summary>
        /// <param name="aDCID"></param>
        /// <param name="bUnHiddenOnly"></param>
        /// <param name="bReturnClones"></param>
        /// <param name="aPartType"></param>
        /// <param name="aSide"></param>
        /// <param name="aRangeGUID"></param>
        /// <param name="rIndexes"></param>
        /// <param name="bRemove"></param>
        /// <returns></returns>
        public List<uopPart> GetByDowncomerIndex(dynamic aDCID,
            bool bUnHiddenOnly = false, bool bReturnClones = false,
            uppPartTypes aPartType = uppPartTypes.Undefined,
            uppSides aSide = uppSides.Undefined,
            string aRangeGUID = null, List<int> rIndices = null, bool bRemove = false)
        {
            List<uopPart> _rVal = new List<uopPart>();
            rIndices = new List<int>();
            uopPart aMem;

            bool bKeep = false;
            string GUIDfilter = string.IsNullOrWhiteSpace(aRangeGUID) ? string.Empty : aRangeGUID.Trim();

            for (int i = 1; i <= Count; i++)
            {
                aMem = _Members[i - 1];
                bKeep = aMem.DowncomerIndex == aDCID;
                bKeep = bKeep && (GUIDfilter ==  string.Empty || mzUtils.ListContains(GUIDfilter, aMem.RangeList));
                bKeep = bKeep && (aPartType == uppPartTypes.Undefined || aMem.PartType == aPartType);
                bKeep = bKeep && !bUnHiddenOnly || (bUnHiddenOnly && !aMem.Suppressed);
                if (bKeep) aMem = SetMemberInfo(i);
                if (bReturnClones) { aMem = aMem.Clone(); bKeep = aMem != null; }
                if (bKeep) _rVal.Add(aMem);
                if (bKeep) rIndices.Add(i);
            }
            if (bRemove & rIndices.Count > 0)
            {
                foreach (var item in _rVal)
                {
                    _Members.Remove(item);
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action: NotifyCollectionChangedAction.Remove, item));
                }

                Reindex();
            }
            return _rVal;
        }

        /// <summary>
        ///returns the collection of members in the collection whose downcomer index equals the passed value
        /// </summary>
        /// <param name="aRangeID"></param>
        /// <param name="bUnHiddenOnly"></param>
        /// <param name="bReturnClones"></param>
        /// <param name="aPartType"></param>
        /// <param name="aSide"></param>
        /// <returns></returns>
        public List<uopPart> GetByRangeIndex(int aRangeID,
            bool bUnHiddenOnly, bool bReturnClones,
            uppPartTypes aPartType = uppPartTypes.Undefined, uppSides aSide = uppSides.Undefined)
        {
            List<uopPart> _rVal = new List<uopPart>();
            uopPart aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = _Members[i - 1];

                if (aMem.RangeIndex == aRangeID)
                {
                    if (aPartType == uppPartTypes.Undefined || aMem.PartType == aPartType)
                    {
                        if (!bUnHiddenOnly || (bUnHiddenOnly && !aMem.Suppressed))
                        {
                            if (aSide == uppSides.Undefined || aMem.Side == aSide)
                            {
                                aMem = SetMemberInfo(i);
                                if (bReturnClones) aMem = aMem.Clone();

                                if (aMem != null) _rVal.Add(aMem);
                            }

                        }
                    }
                }
            }
            return _rVal;
        }

        /// <summary>
        ///returns the collection of members in the collection whose downcomer index equals the passed value
        /// </summary>
        /// <param name="aRangeGUID"></param>
        /// <param name="bUnHiddenOnly"></param>
        /// <param name="bReturnClones"></param>
        /// <param name="aPartType"></param>
        /// <param name="aSide"></param>
        /// <returns></returns>
        public List<uopPart> GetByRangeGUID(string aRangeGUID, bool bUnHiddenOnly = false, bool bReturnClones = false,
            uppPartTypes aPartType = uppPartTypes.Undefined, uppSides aSide = uppSides.Undefined)
        {
            List<uopPart> _rVal = new List<uopPart>();
            uopPart aMem;
            aRangeGUID ??= string.Empty;
            if (string.IsNullOrWhiteSpace(aRangeGUID)) return _rVal;
            aRangeGUID = aRangeGUID.ToUpper();
            for (int i = 1; i <= Count; i++)
            {
                aMem = _Members[i - 1];


                if (aMem.RangeList.ToUpper().Contains(aRangeGUID))
                {
                    if (aPartType == uppPartTypes.Undefined || aMem.PartType == aPartType)
                    {
                        if (!bUnHiddenOnly || (bUnHiddenOnly && !aMem.Suppressed))
                        {
                            if (aSide == uppSides.Undefined || aMem.Side == aSide)
                            {
                                aMem = SetMemberInfo(i);
                                if (bReturnClones) aMem = aMem.Clone();

                                if (aMem != null) _rVal.Add(aMem);
                            }
                        }
                    }
                }

            }
            return _rVal;
        }

        /// <summary>
        ///returns all the members whose hidden property matches the passed value
        /// </summary>
        /// <param name="HiddenValue">the value of the hidden flag to match</param>
        /// <returns></returns>
        public List<uopPart> GetByHidden(bool HiddenValue)
        {
            List<uopPart> _rVal = new List<uopPart>();
            uopPart aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = _Members[i - 1];
                if (aMem.Suppressed == HiddenValue) _rVal.Add(SetMemberInfo(i));
            }
            return _rVal;
        }

        /// <summary>
        ///returns all the members whose suppress property matches the passed value
        /// </summary>
        /// <param name="HiddenValue">the value of the hidden flag to match</param>
        /// <returns></returns>
        public List<uopPart> GetBySupressed(bool SupressedValue)
        {
            List<uopPart> _rVal = new List<uopPart>();
            uopPart aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = _Members[i - 1];
                if (aMem.Suppressed == SupressedValue) _rVal.Add(SetMemberInfo(i));
            }
            return _rVal;
        }

        /// <summary>
        ///returns all the members whose hidden property matches the passed value
        /// </summary>
        /// <param name="HiddenValue">the value of the hidden flag to match</param>
        /// <returns></returns>
        public List<uopPart> GetBySide(uppSides aSide)
        {
            List<uopPart> _rVal = new List<uopPart>();
            uopPart aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = _Members[i - 1];
                if (aMem.Side == aSide) _rVal.Add(SetMemberInfo(i));
            }
            return _rVal;
        }


        public virtual List<uopPart> ToList(bool bGetClones = false)
        {
            List<uopPart> _rVal = new List<uopPart>();
            uopPart aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                if (bGetClones) aMem = aMem.Clone();
                if (aMem != null) _rVal.Add(aMem);
            }

            return _rVal;
        }


        /// <summary>
        ///returns the parts in the collection whose material type matches then passed material type
        /// </summary>
        /// <param name="aMaterialType"></param>
        /// <returns></returns>
        public List<uopPart> GetByMaterialType(uppMaterialTypes aMaterialType)
        {
            List<uopPart> _rVal = new List<uopPart>();
            uopPart aMem;

            for (int i = 1; i <= Count; i++)
            {
                aMem = _Members[i - 1];
                if (aMem.MaterialType == aMaterialType) _rVal.Add(SetMemberInfo(i));

            }
            return _rVal;
        }

        public List<uopPart> GetByPanIndex(int aPanIndex, uppPartTypes aPartType = uppPartTypes.Undefined)
        {
            List<uopPart> _rVal = new List<uopPart>();

            uopPart aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = _Members[i - 1];
                if (aMem.PanIndex == aPanIndex)
                {
                    if (aPartType == uppPartTypes.Undefined || aMem.PartType == aPartType) _rVal.Add(SetMemberInfo(i));
                }
            }
            return _rVal;
        }

        public List<uopPart> GetByPanelIndex(int aPanelID, uppPartTypes aPartType = uppPartTypes.Undefined)
        {
            List<uopPart> _rVal = new List<uopPart>();

            uopPart aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = _Members[i - 1];
                if (aMem.PanelIndex == aPanelID)
                {
                    if (aPartType == uppPartTypes.Undefined || aMem.PartType == aPartType) _rVal.Add(SetMemberInfo(i));
                }

            }
            return _rVal;
        }

        /// <summary>
        ///returns the first part in the colleciton whose parent part path matches the passed path string
        /// </summary>
        /// <param name="aPath"></param>
        /// <returns></returns>
        public List<uopPart> GetByParentPath(string aPath, bool bJustOne = true)
        {
            List<uopPart> _rVal = new List<uopPart>();
            if (string.IsNullOrEmpty(aPath)) return _rVal;
            uopPart aMem;
            for (int i = 1; i <= Count; i++)
            {

                aMem = Item(i);
                if (string.Compare(aMem.ParentPath.Substring(0, aPath.Length), aPath, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    _rVal.Add(aMem);
                    if (bJustOne) break;
                }
            }
            return _rVal;
        }
        /// <summary>
        ///returns the first part in the colleciton whose part path matches the passed path string
        /// </summary>
        /// <param name="aPath"></param>
        /// <returns></returns>
        public List<uopPart> GetByPartPath(string aPath, bool bJustOne = true)
        {
            List<uopPart> _rVal = new List<uopPart>();
            if (string.IsNullOrEmpty(aPath)) return _rVal;
            uopPart aMem;
            for (int i = 1; i <= Count; i++)
            {

                aMem = Item(i);
                if (string.Compare(aMem.PartPath().Substring(0, aPath.Length), aPath, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    _rVal.Add(aMem);
                    if (bJustOne) break;
                }
            }
            return _rVal;
        }

        /// <summary>
        /// returns a subset of members that have the requested part type
        /// </summary>
        /// <param name="aPartType"></param>
        /// <param name="aRangeID"></param>
        /// <param name="bRemove"></param>
        /// <param name="rIndices"></param>
        /// <returns></returns>
        public List<uopPart> GetByPartType(uppPartTypes aPartType, int aRangeID = 0,
            bool bRemove = false, List<int> rIndices = null, Enum aSubPartType = null)
        {

            if (Count <= 0) return new List<uopPart>();



            List<uopPart> _rVal;

            _rVal = (aRangeID > 0) ? _Members.FindAll(x => x.RangeIndex == aRangeID) : _Members;
            if (aSubPartType == null)
            {
                _rVal = _Members.FindAll(x => x.PartType == aPartType);
            }
            else
            {
                _rVal = _Members.FindAll(x => x.PartType == aPartType && x.SubPartType == aSubPartType);
            }
            if (rIndices == null) rIndices = new List<int>();

            foreach (var item in _rVal)
            {
                item.Index = _Members.IndexOf(item) + 1;
                item.SubPart(this);
                if (rIndices != null) rIndices.Add(item.Index);
            }




            return bRemove ? Remove(rIndices) : _rVal;


        }

        /// <summary>
        /// returns a subset of members that have the requested tag
        /// </summary>
        /// <param name="aPartType"></param>
        /// <param name="aFlag"></param>
        /// <param name="ignoreCase"></param>
        /// <param name="aRangeID"></param>
        /// <param name="bJustOne"></param>
        /// <param name="bRemove"></param>
        /// <param name="rIndices"></param>
        /// <returns></returns>
        public List<uopPart> GetByTag(string aTag, string aFlag = null, bool bIgnoreCase = true, int aRangeID = 0, bool bJustOne = false,
            bool bRemove = false, List<int> rIndices = null)
        {
            List<uopPart> _rVal = new List<uopPart>();
            uopPart aMem;
            rIndices = new List<int>();
            bool testFlag = aFlag != null;

            for (int i = 1; i <= Count; i++)
            {
                aMem = _Members[i - 1];
                if (string.Compare(aMem.Tag, aTag, ignoreCase: bIgnoreCase) == 0)
                {
                    if (aRangeID <= 0 || (aRangeID > 0 & aMem.RangeIndex == aRangeID))
                    {
                        if (!testFlag)
                        {
                            _rVal.Add(SetMemberInfo(i));
                            rIndices.Add(i);
                            if (bJustOne) break;

                        }
                        else
                        {
                            if (string.Compare(aMem.Flag, aFlag, ignoreCase: bIgnoreCase) == 0)
                            {
                                _rVal.Add(SetMemberInfo(i));
                                rIndices.Add(i);
                                if (bJustOne) break;

                            }

                        }

                    }
                }

            }
            if (bRemove)
            {
                for (int i = rIndices.Count - 1; i > 0; i--) { Remove(rIndices[i]); }
            }
            return _rVal;
        }

        /// <summary>
        /// returns a the first member that has the requested tag
        /// </summary>
        /// <param name="aPartType"></param>
        /// <param name="aFlag"></param>
        /// <param name="ignoreCase"></param>
        /// <param name="aRangeID"></param>
        /// <param name="bJustOne"></param>
        /// <param name="bRemove"></param>
        /// <returns></returns>
        public uopPart GetTagged(string aTag, string aFlag = null, bool ignoreCase = true, int aRangeID = 0, bool bRemove = false)
        {
            List<uopPart> set = GetByTag(aTag, aFlag, ignoreCase, aRangeID, true, bRemove);
            return (set.Count > 0) ? set[0] : null;
        }

        /// <summary>
        ///returns all the members whose IsStainless property matches the passed value
        /// </summary>
        /// <param name="StainlessValue">the value of the stainless flag to match</param>
        /// <returns></returns>
        public List<uopPart> GetByStainless(bool StainlessValue)
        {
            List<uopPart> _rVal = new List<uopPart>();
            uopPart aMem;

            for (int i = 1; i <= Count; i++)
            {
                aMem = _Members[i - 1];
                if (aMem.IsStainless == StainlessValue) _rVal.Add(SetMemberInfo(Index));


            }
            return _rVal;
        }

        public List<uopPart> GetByValidity(bool aValidValue)
        {
            List<uopPart> _rVal = new List<uopPart>();
            uopPart aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = _Members[i - 1];
                if (aMem.Invalid == aValidValue) _rVal.Add(SetMemberInfo(i));
            }
            return _rVal;
        }

        public List<uopPart> GetByMark(bool aMarkValue)
        {
            List<uopPart> _rVal = new List<uopPart>();
            uopPart aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = _Members[i - 1];
                if (aMem.Mark == aMarkValue) _rVal.Add(SetMemberInfo(i));
            }
            return _rVal;
        }

        public List<uopPart> GetHardware(dynamic WeldedFlag = null)
        {
            List<uopPart> _rVal = new List<uopPart>();
            uopPart aMem = null;
            bool bTestit = false;
            bool bWFlag = false;

            if (WeldedFlag != null)
            {
                bTestit = true;
                bWFlag = mzUtils.VarToBoolean(WeldedFlag);
            }


            for (int i = 1; i <= Count; i++)
            {
                aMem = _Members[i - 1];

                if (aMem.IsHardware)
                {
                    if (bTestit)
                    { if (aMem.WeldedInPlace == bWFlag) _rVal.Add(aMem); }
                    else
                    { _rVal.Add(SetMemberInfo(i)); }
                }
            }
            return _rVal;
        }



        public uopPart GetPartByType(uppPartTypes aPartType, int aOccurance = 0) => GetPartByType(aPartType, aOccurance, out int _);

        public uopPart GetPartByType(uppPartTypes aPartType, int aOccurance, out int rIndex)
        {
            rIndex = 0;
            uopPart aMem;
            int ocr = 0;
            if (aOccurance <= 0) aOccurance = 1;
            if (aOccurance == 1) return Find(x => x.PartType == aPartType);

            for (int i = 1; i <= Count; i++)
            {
                aMem = _Members[i - 1];
                if (aMem.PartType == aPartType)
                {
                    ocr += 1;
                    if (ocr == aOccurance)
                    {
                        rIndex = i;
                        return SetMemberInfo(i);
                    }

                }
            }
            return null;
        }

        public virtual uopPart GetByName(string aName) => GetByName(aName, out int _);

        public virtual uopPart GetByName(string aName, out int rIndex)
        {
            rIndex = 0;
            uopPart aMem;
            if (string.IsNullOrEmpty(aName)) return null;

            for (int i = 1; i <= Count; i++)
            {
                aMem = _Members[i - 1];
                if (string.Compare(aMem.Name, aName, ignoreCase: true) == 0) { rIndex = i; return aMem; };

            }
            return null;
        }

        public virtual uopPart LongestMember(uppPartTypes aPartType = uppPartTypes.Undefined) => LongestMember(aPartType, out int _);

        public virtual uopPart LongestMember(uppPartTypes aPartType, out int rIndex)
        {
            uopPart aMem;
            rIndex = 0;
            double minval = Double.MinValue;

            for (int i = 1; i <= Count; i++)
            {
                aMem = _Members[i - 1];
                if (aPartType == uppPartTypes.Undefined || aMem.PartType == aPartType)
                {
                    if (aMem.Length > minval) { minval = aMem.Length; rIndex = i; }

                }

            }
            return (rIndex > 0) ? Item(rIndex) : null;
        }

        public virtual uopPart ShortestMember(uppPartTypes aPartType = uppPartTypes.Undefined) => ShortestMember(aPartType, out int _);
        public virtual uopPart ShortestMember(uppPartTypes aPartType, out int rIndex)
        {
            uopPart aMem;
            rIndex = 0;
            double maxval = double.MaxValue;

            for (int i = 1; i <= Count; i++)
            {
                aMem = _Members[i - 1];
                if (aPartType == uppPartTypes.Undefined || aMem.PartType == aPartType)
                {
                    if (aMem.Length < maxval) { maxval = aMem.Length; rIndex = i; }

                }

            }
            return (rIndex > 0) ? Item(rIndex) : null;
        }

        /// <summary>
        /// ^returns all the members whose part path is left equal to the past path
        /// </summary>
        /// <param name="aPath"></param>
        /// <param name="CollectionIndex"></param>
        /// <returns></returns>
        public List<uopPart> GetSubMembers(string aPath, out int rIndex)
        {
            List<uopPart> _rVal = new List<uopPart>();
            uopPart aMem = null;
            rIndex = 0;

            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                if (string.Compare(aMem.PartPath(), aPath, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    rIndex = i;
                    _rVal.Add(aMem);
                }
                else
                {
                    if (string.Compare(aMem.PartPath().Substring(0, aMem.PartPath().Length), aPath, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        _rVal.Add(aMem);
                    }
                }
            }
            return _rVal;
        }

        /// <summary>
        ///returns the parts of the requested type in a sub parts collection
        /// </summary>
        /// <param name="aPartType"></param>
        /// <returns></returns>
        public List<uopPart> GetSubParts(uppPartTypes aPartType)
        {
            List<uopPart> _rVal = new List<uopPart>();
            uopPart aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = _Members[i - 1];
                if (aMem.PartType == aPartType) _rVal.Add(SetMemberInfo(i));

            }
            return _rVal;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bWeldedFlag"></param>
        /// <param name="MatchNodeNames"></param>
        /// <returns></returns>
        public List<uopPart> Hardware(bool bWeldedFlag)
        {
            List<uopPart> _rVal = new List<uopPart>();
            uopPart aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = _Members[i - 1];
                if (aMem.PartType != uppPartTypes.Parts)
                {
                    if (aMem.IsHardware)
                    {
                        if (aMem.WeldedInPlace == bWeldedFlag) _rVal.Add(SetMemberInfo(i));

                    }
                }

            }
            return _rVal;
        }

        public void HideMembers(bool bFirstMemberVisible = true)
        {
            if (Count <= 0) return;
            for (int i = Count; i >= 1; i--)
            {

                _Members[i - 1].Suppressed = true;

                if (bFirstMemberVisible && Count > 0) _Members[0].Suppressed = false;
            }

            Suppressed = !bFirstMemberVisible;

        }

        /// <summary>
        /// returns true if the passed collection is equal to this one
        /// each member must be equal by position in collection
        /// </summary>
        /// <param name="As"></param>
        /// <param name=""></param>
        /// <returns></returns>
        public bool IsEqual(colUOPParts aPartsCol)
        {
            if (aPartsCol == null) return false;
            if (aPartsCol.Count != Count) return false;

            for (int i = 1; i <= Count; i++)
            {
                if (!_Members[i - 1].IsEqual(aPartsCol.Item(i))) return false;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aIndex"></param>
        /// <param name="aCollector"></param>
        /// <returns></returns>
        public uopDocuments Drawings(int aIndex = -1, uopDocuments aCollector = null)
        {
            uopDocuments _rVal = aCollector ?? new uopDocuments();
            uopPart aMem;

            if (aIndex >= 0)
            {
                if (aIndex <= Count - 1)
                {
                    aMem = Item(aIndex);
                    _rVal.Append(aMem.Drawings());
                }
            }
            else
            {
                for (int i = 1; i <= Count; i++)
                {
                    aMem = Item(i);
                    _rVal.Append(aMem.Drawings());
                }

            }
            return _rVal;
        }


        /// <summary>
        /// the holes of indicated member
        /// </summary>
        /// <param name="aIndex"></param>
        /// <param name="aPartType"></param>
        /// <param name="aTag"></param>
        /// <returns></returns>
        public uopHoleArray MemberHoles(int aIndex, uppPartTypes aPartType = uppPartTypes.Undefined, dynamic aTag = null)
        {

            uopPart aMem;
            int cnt = 0;
            if (aPartType == uppPartTypes.Undefined)
            {
                if (aIndex > 1 & aIndex <= Count)
                {
                    aMem = Item(aIndex);
                    return aMem.HoleArray(null, aTag);
                }
            }
            else
            {
                for (int i = 1; i <= Count; i++)
                {
                    aMem = Item(i);
                    if (aMem.PartType == aPartType)
                    {
                        cnt += 1;
                        if (cnt == aIndex) return aMem.HoleArray(null, aTag);


                    }
                }
            }
            return new uopHoleArray();
        }

        /// <summary>
        ///returns the count of members whose parttype property matches the passed value
        /// </summary>
        /// <param name="aPartType"></param>
        /// <param name="bIncludeOccurance"></param>
        /// <param name="bDoubleBYX"></param>
        /// <returns></returns>
        public int PartCount(uppPartTypes aPartType, bool bIncludeOccurance = true, bool bDoubleBYX = false)
        {
            int _rVal = 0;

            uopPart aMem;
            int acnt = 0;
            for (int i = 1; i <= Count; i++)
            {
                aMem = _Members[i - 1];
                if (aMem.PartType == aPartType)
                {
                    acnt = 0;
                    if (bIncludeOccurance)
                    { acnt = aMem.OccuranceFactor; }
                    else
                    {
                        acnt = 1;
                        if (bDoubleBYX)
                        {
                            if (Math.Round(aMem.X, 1) > 0) acnt = 2;

                        }
                    }
                    _rVal += acnt;
                }

            }
            return _rVal;
        }


        /// <summary>
        ///returns the first member part type property matches the passed value
        /// </summary>
        /// <param name="aPartType"></param>
        /// <param name="aIndex"></param>
        /// <returns></returns>
        public uopPart PartItem(uppPartTypes aPartType, int aIndex)
        {

            if (aIndex <= 0 || aIndex > Count) return null;
            uopPart aMem;
            int cnt = 0;
            for (int i = 1; i <= Count; i++)
            {
                aMem = _Members[i - 1];
                if (aMem.PartType == aPartType)
                {
                    cnt += 1;
                    if (cnt == aIndex) return SetMemberInfo(i);
                }
            }
            return null;
        }

        /// <summary>
        ///returns a list of all the part numbers of all the members
        /// </summary>
        /// <param name="bUniqueOnly"></param>
        /// <returns></returns>
        public List<string> PartNumbers(bool bUniqueOnly = true)
        {
            List<string> _rVal = new List<string>();
            uopPart aMem;

            string aPn = string.Empty;

            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);

                aPn = aMem.PartNumber;
                if (aPn !=  string.Empty)
                {
                    if (bUniqueOnly)
                    {
                        if (_rVal.IndexOf(aPn) < 0) _rVal.Add(aPn);
                    }
                    else
                    {
                        _rVal.Add(aPn);
                    }
                }



            }
            return _rVal;
        }

        /// <summary>
        ///returns a list of all the part types of all the members
        /// </summary>
        /// <param name="bUniqueOnly"></param>
        /// <returns></returns>
        public List<uppPartTypes> PartTypes(bool bUniqueOnly = true)
        {
            List<uppPartTypes> _rVal = new List<uppPartTypes>();
            uopPart aMem;

            uppPartTypes pt;

            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                pt = aMem.PartType;


                if (bUniqueOnly)
                {
                    if (_rVal.IndexOf(pt) < 0) _rVal.Add(pt);
                }
                else
                {
                    _rVal.Add(pt);
                }




            }
            return _rVal;
        }

        /// <summary>
        ///returns all the members whose part path is left equal to the past path
        /// </summary>
        /// <param name="aPartType"></param>
        /// <returns></returns>
        public virtual List<uopPart> RemoveByType(uppPartTypes aPartType)
        {
            List<uopPart> _rVal = new List<uopPart>();
            uopPart aMem;
            List<string> pPaths = null;
            pPaths = new List<string>();
            for (int i = Count; i >= 1; i--)
            {
                aMem = _Members[i - 1];
                if (aMem.PartType == aPartType)
                {
                    pPaths.Add(aMem.PartPath());
                    _rVal.Add(SetMemberInfo(i));

                }
            }
            foreach (var item in _rVal)
            {
                _Members.Remove(item);
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action: NotifyCollectionChangedAction.Remove, item));
            }

            return _rVal;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="aMemberPath"></param>
        public virtual List<uopPart> RemoveMemberAndChildren(string aMemberPath)
        {

            uopPart aMem;
            uopPart rPart = null;
            List<uopPart> _rVal = new List<uopPart>();
            for (int i = Count; i >= 1; i--)
            {
                aMem = Item(i);
                if (string.Compare(aMem.PartPath().Substring(0, aMemberPath.Length), aMemberPath, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (string.Compare(aMem.PartPath(), aMemberPath, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        rPart = aMem;
                    }
                    _rVal.Add(aMem);
                }

            }

            foreach (var item in _rVal)
            {
                _Members.Remove(item);
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action: NotifyCollectionChangedAction.Remove, item));
            }

            return _rVal;
        }


        /// <summary>
        /// ^returns all the members whose part path is left equal to the passed path
        /// </summary>
        /// <param name="aPath"></param>
        /// <param name="rCollectionIndex"></param>
        /// <returns></returns>
        public virtual List<uopPart> RemoveSubMembers(string aPath, out int rCollectionIndex)
        {
            List<uopPart> _rVal = new List<uopPart>();
            uopPart aMem;

            rCollectionIndex = 0;
            for (int i = Count; i >= 1; i--)
            {
                aMem = _Members[i - 1];
                if (string.Compare(aMem.PartPath(), aPath, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    rCollectionIndex = i;
                    _rVal.Add(aMem);
                }
                else
                {
                    if (string.Compare(aMem.PartPath().Substring(0, aPath.Length), aPath, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        _rVal.Add(aMem);
                    }
                }
            }

            foreach (var item in _rVal)
            {
                _Members.Remove(item);
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action: NotifyCollectionChangedAction.Remove, item));
            }

            return _rVal;
        }

        public virtual List<uopPart> RemoveByPartPath(string aPartPath, bool RemoveExactMatch, out int rExactMatchIndex)
        {
            List<uopPart> _rVal = new List<uopPart>();
            rExactMatchIndex = 0;
            uopPart aMem;
            int slen = 0;
            int plen = 0;
            slen = aPartPath.Length;
            for (int i = Count; i >= 1; i--)
            {
                aMem = _Members[i - 1];
                plen = aMem.PartPath().Length;
                if (plen >= slen)
                {
                    if (string.Compare(aMem.PartPath().Substring(0, slen), aPartPath, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        //the path contains the string
                        if (plen == slen)
                        {
                            rExactMatchIndex = i;
                            if (RemoveExactMatch)
                            {
                                _rVal.Add(aMem);
                            }
                        }
                        else
                        {
                            _rVal.Add(aMem);
                        }
                    }
                }
            }
            foreach (var item in _rVal)
            {
                _Members.Remove(item);
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action: NotifyCollectionChangedAction.Remove, item));
            }

            return _rVal;
        }


        /// <summary>
        /// sets the category of all the members to the passed value
        /// </summary>
        /// <param name="aCategory"></param>
        /// <param name="iStart"></param>
        /// <param name="iEnd"></param>
        public virtual int SetCategory(string aCategory, int iStart = 0, int iEnd = 0)
        {
            if (Count <= 0) return 0;
            if (iStart < 1) iStart = 1;
            if (iEnd <= 0) iEnd = Count;
            if (iEnd > Count) iEnd = Count;
            int _rVal = 0;
            mzUtils.SortTwoValues(true, ref iStart, ref iEnd);
            for (int i = iStart; i < iEnd; i++)
            {
                if (i > Count) break;
                if (_Members[i - 1].Category != aCategory) _rVal += 1;
                _Members[i - 1].Category = aCategory;

            }
            return _rVal;
        }

        /// <summary>
        /// sets the hidden property of all the members to the passed value
        /// </summary>
        /// <param name="SuppressedValue"></param>
        /// <param name="iStart"></param>
        /// <param name="iEnd"></param>
        public virtual int SetSuppressed(bool SuppressedValue, int iStart = 0, int iEnd = 0)
        {
            if (Count <= 0) return 0;
            if (iStart < 1) iStart = 1;
            if (iEnd <= 0) iEnd = Count;
            if (iEnd > Count) iEnd = Count;
            int _rVal = 0;
            mzUtils.SortTwoValues(true, ref iStart, ref iEnd);
            for (int i = iStart; i < iEnd; i++)
            {
                if (i > Count) break;
                if (_Members[i - 1].Suppressed != SuppressedValue) _rVal += 1;
                _Members[i - 1].Suppressed = SuppressedValue;

            }
            return _rVal;

        }

        /// <summary>
        /// sets the invalid property of all the members to the passed value
        /// </summary>
        /// <param name="InvalidValue"></param>
        /// <param name="iStart"></param>
        /// <param name="iEnd"></param>
        public virtual int SetInvalid(bool InvalidValue, int iStart = 0, int iEnd = 0)
        {
            if (Count <= 0) return 0;
            if (iStart < 1) iStart = 1;
            if (iEnd <= 0) iEnd = Count;
            if (iEnd > Count) iEnd = Count;
            int _rVal = 0;
            mzUtils.SortTwoValues(true, ref iStart, ref iEnd);
            for (int i = iStart; i < iEnd; i++)
            {
                if (i > Count) break;
                if (_Members[i - 1].Invalid != InvalidValue) _rVal += 1;
                _Members[i - 1].Invalid = InvalidValue;

            }
            return _rVal;

        }

        /// <summary>
        /// sets the hidden property of all the members to the passed value
        /// </summary>
        /// <param name="HiddenValue"></param>
        /// <param name="iStart"></param>
        /// <param name="iEnd"></param>
        public virtual int SetMark(bool MarkValue, int iStart = 0, int iEnd = 0)
        {
            if (Count <= 0) return 0;
            if (iStart < 1) iStart = 1;
            if (iStart > Count) iStart = Count;
            if (iEnd <= 0) iEnd = Count;
            if (iEnd > Count) iEnd = Count;
            int _rVal = 0;
            mzUtils.SortTwoValues(true, ref iStart, ref iEnd);
            for (int i = iStart; i < iEnd; i++)
            {
                if (i > Count) break;
                if (_Members[i - 1].Mark != MarkValue) _rVal += 1;
                _Members[i - 1].Mark = MarkValue;

            }
            return _rVal;

        }

        /// <summary>
        /// sets the hidden property of all the members to the passed value
        /// </summary>
        /// <param name="aQuantity"></param>
        /// <param name="iStart"></param>
        /// <param name="iEnd"></param>
        public virtual int SetQuantities(int aQuantity, int iStart = 0, int iEnd = 0)
        {
            if (Count <= 0) return 0;
            if (iStart < 1) iStart = 1;
            if (iEnd <= 0) iEnd = Count;
            if (iEnd > Count) iEnd = Count;
            int _rVal = 0;
            mzUtils.SortTwoValues(true, ref iStart, ref iEnd);
            for (int i = iStart; i < iEnd; i++)
            {
                if (i > Count) break;
                if (_Members[i - 1].Quantity != aQuantity) _rVal += 1;
                _Members[i - 1].Quantity = aQuantity;

            }
            return _rVal;

        }

        public int SetVisibility(bool VisVal, bool FirstMemberVisible = true, int iStart = 0, int iEnd = 0)
        {
            if (Count <= 0) return 0;

            if (iStart < 1) iStart = 1;
            if (iEnd <= 0) iEnd = Count;
            if (iEnd > Count) iEnd = Count;
            int _rVal = 0;
            mzUtils.SortTwoValues(true, ref iStart, ref iEnd);
            for (int i = iStart; i < iEnd; i++)
            {
                if (i > Count) break;
                if (_Members[i - 1].IsVisible != VisVal) _rVal += 1;
                _Members[i - 1].IsVisible = VisVal;
                if (FirstMemberVisible && i == iStart) _Members[i - 1].IsVisible = true;
            }
            return _rVal;

        }

        /// <summary>
        ///returns the tally of each members total quantity property
        /// </summary>
        /// <param name="bSuppressRangeMutiplier"></param>
        /// <returns></returns>
        public new int TotalQuantity(bool bSuppressRangeMutiplier = false)
        {
            int _rVal = 0;
            uopPart aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                if (!bSuppressRangeMutiplier)
                { _rVal += aMem.TotalQuantity; }
                else
                { _rVal += aMem.Quantity; }

            }
            return _rVal;
        }

        /// <summary>
        ///returns the tally of each members total quantity property including spare percentage
        /// </summary>
        public override int SpareQuantity()
        {
            int MinSprs = PropValI("MinSpares", uppPartTypes.Project, 2);
            return uopUtils.CalcSpares(TotalQuantity(false), base.SparePercentage, MinSprs);

        }


        /// <summary>
        /// the number of parts with occurance factors applied
        /// </summary>
        /// <param name="aPartType"></param>
        /// <returns></returns>
        public int TotalOccurances(uppPartTypes aPartType = uppPartTypes.Undefined)
        {
            int _rVal = 0;
            uopPart aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = _Members[i - 1];
                if (aPartType == uppPartTypes.Undefined ||
                    (aPartType != uppPartTypes.Undefined && aMem.PartType == aPartType))
                {
                    _rVal += aMem.OccuranceFactor;
                }
            }
            return _rVal;
        }



        public override void UpdatePartProperties()
        {
            for (int i = 1; i <= Count; i++)
            {
                _Members[i - 1].UpdatePartProperties();
            }
        }

        /// <summary>
        /// set the spare percentage of all the members to the passed value
        /// </summary>
        /// <param name="SpareVal"></param>
        /// <param name="aSkipType"></param>
        public void SetSparePercentage(double SpareVal, uppPartTypes aSkipType = uppPartTypes.Undefined)
        {

            uopPart aMem;

            for (int i = 1; i <= Count; i++)
            {
                aMem = _Members[i - 1];
                if (aSkipType == uppPartTypes.Undefined ||
                    (aSkipType != uppPartTypes.Undefined && aMem.PartType != aSkipType))
                {
                    aMem.SparePercentage = SpareVal;
                }
            }
        }



        /// <summary>
        ///the properties that are saved to file 
        /// </summary>
        public override uopPropertyArray SaveProperties(string aHeading = null)
        {
            uopProperties _rVal = base.CurrentProperties();
            aHeading = string.IsNullOrWhiteSpace(aHeading) ? PartName.ToUpper() : aHeading.Trim();
            uopPart aMem;
            uopProperties pProps;
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                aMem.UpdatePartProperties();
                pProps = aMem.SaveProperties($"{aHeading}({i})").Item(1);
                if (pProps != null && pProps.Count > 0) _rVal.Append(pProps);
            }

            return new uopPropertyArray(_rVal, aName: aHeading, aHeading: aHeading);
        }

        /// <summary>
        /// removes the last _rVal from the collection unitl the count is reduced to the requested number
        /// </summary>
        /// <param name="aCount"></param>
        public List<uopPart> ReduceTo(int aCount)
        {

            List<uopPart> _rVal = new List<uopPart>();

            if (aCount >= Count) return _rVal;


            while (_Members.Count > aCount)
            {
                _rVal.Add(_Members[_Members.Count - 1]);
            }

            foreach (var item in _rVal)
            {
                _Members.Remove(item);
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action: NotifyCollectionChangedAction.Remove, item));
            }

            Quantity = _Members.Count;
            return _rVal;
        }


        public void SortByPartNumber(bool bHighToLow = false, bool bRemoveDuplicates = false, List<uopPart> aWorkCol = null)
        {
            if (aWorkCol != null)
            {
                uopParts.SortPartsByPartNumber(aWorkCol, bHighToLow, false, bRemoveDuplicates);
                return;
            }

            uopParts.SortPartsByPartNumber(_Members, bHighToLow, MaintainIndices, bRemoveDuplicates);
        }

        public override void SubPart(uopProject aProject, string aCategory = null, bool? bHidden = null)
        {

            base.SubPart(aProject, aCategory, bHidden);
            foreach (var item in this)
            {
                item.SubPart(aProject, aCategory, bHidden);
            }

        }

        #endregion Methods



        #region Shared Methods

        /// <summary>
        /// reorders the members based on their ordinate values
        /// </summary>
        /// <param name="aOrdinateType">ordinate type type parameter</param>
        /// <param name="bLowToHigh">flag to control the sort order (high to low or Low to High)</param>
        /// <param name="bNoDupes">flag to remove any at the same ordinate</param>
        /// <param name="aPrecis">precision for numerical comparison (1 to 16)</param>
        public static List<uopPart> SortPartsByOrdinate(List<uopPart> aParts, dxxOrdinateDescriptors aOrdinateType, bool bLowToHigh = false, bool bNoDupes = false, int aPrecis = -1)
        {
            List<uopPart> _rVal = aParts;

            if (aParts == null || aParts.Count <= 0) return _rVal;
            aPrecis = mzUtils.LimitedValue(aPrecis, 1, 15);
            _rVal = new List<uopPart>();
            TVALUES aOrds = uopParts.Ordinates(aParts, aOrdinateType, aPrecis, bNoDupes);
            TVALUES aIds = aOrds.SortWithIDs(bReturnBaseOne: false, !bLowToHigh);


            for (int i = 1; i <= aIds.Count; i++) { _rVal.Add(aParts[aIds.Item(i)]); }
            return _rVal;

        }

        /// <summary>
        ///returns the requested ordinates of the indicated members.
        /// </summary>
        /// <param name="aOrdinateType">ordinate type type parameter</param>
        /// <param name="aPrecis">precision to round the return values to</param>
        /// <param name="bNoDupes">flag to return only the uniq values</param>
        /// <param name="aPartType"></param>
        /// <param name="aPanelID"></param>
        /// <param name="aDowncomerID"></param>
        /// <param name="aCollector"></param>
        /// <returns></returns>
        internal static TVALUES Ordinates(List<uopPart> aParts, dxxOrdinateDescriptors aOrdinateType,
             int aPrecis = -1, bool bNoDupes = false, uppPartTypes aPartType = uppPartTypes.Undefined,
             int aPanelID = 0, int aDowncomerID = 0, mzValues aCollector = null)
        {
            TVALUES _rVal = new TVALUES("");
            if (aParts == null) return _rVal;

            uopPart aMem;
            double aVal = 0;
            bool bKeep = false;

            aPrecis = mzUtils.LimitedValue(aPrecis, -1, 10);
            for (int i = 0; i < aParts.Count; i++)
            {
                aMem = aParts[i];
                bKeep = aPartType == uppPartTypes.Undefined || aMem.PartType == aPartType;

                if (bKeep && aPanelID > 0)
                {
                    if (aMem.PanelIndex != aPanelID) bKeep = false;

                }
                if (bKeep && aDowncomerID > 0)
                {
                    if (aMem.DowncomerIndex != aDowncomerID) bKeep = false;

                }
                if (bKeep)
                {
                    if (aOrdinateType == dxxOrdinateDescriptors.Z)
                    { aVal = aMem.Z; }
                    else if (aOrdinateType == dxxOrdinateDescriptors.Y)
                    { aVal = aMem.Y; }
                    else
                    { aVal = aMem.X; }
                    if (aPrecis >= 0) aVal = Math.Round(aVal, aPrecis);

                    if (_rVal.Add(aVal, bNoDupes: bNoDupes))
                    {
                        if (aCollector != null) aCollector.AddNumber(aVal);
                    }
                }
            }
            return _rVal;
        }

        public static void SortPartsByPartNumber(List<uopPart> aParts, bool bHighToLow = false, bool bReIndex = false, bool bRemoveDuplicates = false)
        {

            if (aParts == null) return;


            List<Tuple<string, uopPart>> sort = new List<Tuple<string, uopPart>>();

            foreach (uopPart part in aParts)
            {
                bool keep = true;
                string pn = part.PartNumber;
                if (bRemoveDuplicates)
                {
                    keep = sort.FindIndex(x => x.Item1 == pn) < 0;
                }
                if (keep) sort.Add(new Tuple<string, uopPart>(pn, part));

            }

            if (bHighToLow)
                sort = sort.OrderByDescending(t => t.Item1).ToList();
            else
                sort = sort.OrderBy(t => t.Item1).ToList();

            aParts.Clear();
            foreach (Tuple<string, uopPart> tpl in sort)
            {
                uopPart prt = tpl.Item2;
                if (bReIndex) prt.Index = aParts.Count + 1;
                aParts.Add(prt);
            }
        }

        #endregion Shared Methods


        #region IDisposable Implementation

        public virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach (var item in _Members)
                    {
                        item.Destroy();
                    }
                    _Members.Clear();
                    _Members = null;

                }

                disposedValue = true;
            }
        }



        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion  IDisposable Implementation

    }


}
