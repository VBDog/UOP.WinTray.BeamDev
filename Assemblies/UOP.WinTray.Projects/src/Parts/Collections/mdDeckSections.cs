using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UOP.DXFGraphics;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Events;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Materials;
using UOP.WinTray.Projects.Parts;

using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;

namespace UOP.WinTray.Projects.Parts
{
    public class mdDeckSections : List<mdDeckSection>, IEnumerable<mdDeckSection>, ICloneable
    {
        public delegate void DeckSectionsInvalidatedHandler();
        public event DeckSectionsInvalidatedHandler eventDeckSectionsInvalidated;

        #region Constructors
        public mdDeckSections() => Init();

        public mdDeckSections(IEnumerable<mdDeckSection> aPartToCopy, bool bDontCopyMembers = false, bool bDontCloneMembers = false) => Init(aPartToCopy,bDontCopyMembers, bDontCloneMembers);
       
        private void Init(IEnumerable<mdDeckSection> aPartToCopy = null, bool bDontCopyMembers = false, bool bDontCloneMembers = false)
        {
            _AssyRef = null;
            _InValid= false;
            _InvalidWhenEmpty= false;

            if (aPartToCopy != null)
            {
                if(aPartToCopy is mdDeckSections)
                {
                    mdDeckSections secs = (mdDeckSections)aPartToCopy;
                    HasAlternateSections = secs.HasAlternateSections;
                    _InValid = secs._InValid;
                    _InvalidWhenEmpty = secs._InvalidWhenEmpty;
                }
                if (!bDontCopyMembers) Populate(aPartToCopy, bAddClones: !bDontCloneMembers);
            }

        }

        #endregion Constructors

       

        #region Properties


        private WeakReference<mdTrayAssembly> _AssyRef;
        public mdTrayAssembly TrayAssembly
        {
            get
            {
                if (_AssyRef == null) return null;
                if (!_AssyRef.TryGetTarget(out mdTrayAssembly _rVal)) _AssyRef = null;
                return _rVal;

            }

            set
            {
                if (value == null) { _AssyRef = null; return; }
                _AssyRef = new WeakReference<mdTrayAssembly>(value);
                if (value != null)
                {
                    HasAlternateSections = value.HasAlternateDeckParts;
                    RangeGUID = value.RangeGUID;
                }

            }
        }

        /// <summary>
        ///returns the section marked as selected from the collection
        /// </summary>
        private int _SelID = 0;
        public virtual mdDeckSection SelectedMember
        {
            get
            {

                _SelID = 0;
                mdDeckSection _rVal = null;

                for (int i = 1; i <= Count; i++)
                {
                    mdDeckSection aMem = this[i - 1];
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
                    this[0].SetSelected(true);
                    _SelID = 1;
                }

                return (_SelID > 0) ? Item(_SelID,true) : null;

            }

        }

        public int SelectedIndex
        {
            get
            {
                mdDeckSection aMem = SelectedMember;
                return _SelID;
            }

            set => SetSelected(value);

        }

        private bool _InvalidWhenEmpty = false;
        public bool InvalidWhenEmpty { get => _InvalidWhenEmpty; set => _InvalidWhenEmpty = value; }


        public string RangeGUID { get; set; }
        private bool _InValid;
        /// <summary>
        /// flag indicating that something has changed and the sections need to be regenerated
        /// </summary>
        public  bool Invalid
        {
            get => _InValid || (InvalidWhenEmpty &&  Count <= 0);
            set
            {
                if (_InValid == value) return;
                _InValid = value;
                if (value)
                    eventDeckSectionsInvalidated?.Invoke();
            }
        }

        /// <summary>
        ///returns the number of actual manways in the collection of sections
        /// </summary>
        /// <returns></returns>
        public int ManwayCount
        {
            get
            {
                int _rVal = 0;
                foreach (mdDeckSection item in this)
                    if (item.IsManway) _rVal++;
                return _rVal;

            }
        }


        /// <summary>
        /// the total number of ring clips needed to attach the section to the ring
        /// accounts for sections occurance factors
        /// </summary>
        public int RingClipCount
        {
            get
            {
                int _rVal = 0;

                mdTrayAssembly aAssy = GetMDTrayAssembly();
                if (aAssy == null) return 0;
                foreach (mdDeckSection item in this)
                {
             
                    _rVal += item.GenHoles(aAssy, "RING CLIP").Item(1).Count * item.OccuranceFactor;
                }

                return _rVal;
            }
        }


        public List<mdDeckSection> Manways

        {
            get
            {
                return FindAll(x => x.IsManway);
            }
        }

        public bool HasAlternateSections { get; private set; }


        public uopSheetMetal Material 
        {
            get
            {
                 return Count == 0 ?  null : this[0].SheetMetal;
            }

            set
            {
                if (value == null) return;
                TMATERIAL struc = value.Structure;
                foreach (mdDeckSection item in this) item.SheetMetalStructure = struc;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// marks the indicated member as the selected part
        /// </summary>

        public virtual void SetSelected(int aIndex)
        {
            int j = SelectedIndex;
            for (int i = 1; i <= Count; i++)
            {
                this[i - 1].SetSelected(i == aIndex);

                if (i == aIndex)
                    j = i;
            }

            if (j == 0 && Count > 0)
            {
                this[0].SetSelected(true);
                j = 1;
            }

        }

        public void SetSelected(mdDeckSection aMember)
        {
            int idx = this.IndexOf(aMember) + 1;

            int j = 0;
            for (int i = 1; i <= Count; i++)
            {
                this[i - 1].SetSelected(i == idx);

                if (i == idx)
                    j = i;
            }

            if (j == 0 && Count > 0)
            {
                this[0].SetSelected(true);
                j = 1;
            }

        }


        public mdTrayAssembly GetMDTrayAssembly(mdTrayAssembly aAssy = null)
        {
            if (aAssy != null)
            {
                TrayAssembly = aAssy;
                return aAssy;
            }
            mdTrayAssembly _rVal = TrayAssembly;
            if (_rVal == null && !string.IsNullOrWhiteSpace(RangeGUID))
            {
                uopTrayRange range = uopEvents.RetrieveRange(RangeGUID);
                if(range != null && range.ProjectFamily == uppProjectFamilies.uopFamMD)
                {
                    mdTrayRange mdrange = (mdTrayRange)range;
                    _rVal = mdrange.TrayAssembly;
                }
             
                if (_rVal != null) TrayAssembly = _rVal;

            }
            return _rVal;
        }

        public uopSectionShapes BaseShapes(bool bGetClones = false, mdTrayAssembly aAssy = null, int? aPanelIndex = null, int? aPanelSectionIndex = null, bool bGetUniques = false, bool bRegenShapes = false)
        {
            aAssy ??= GetMDTrayAssembly();
            uopSectionShapes _rVal = new uopSectionShapes() { TrayAssembly = aAssy };
            uopSectionShapes deckshapes = null;
            if (aAssy == null) bRegenShapes = false;
            if (bRegenShapes)
                deckshapes = aAssy.DowncomerData.CreateSectionShapes(aAssy.DeckSplices, null, true, true, true);

            foreach (uopPart item in this)
            {
                mdDeckSection section = (mdDeckSection)item;
                if (bRegenShapes) section.BaseShape = deckshapes.Find(x => x.Handle == section.Handle);
                uopSectionShape shape = section.BaseShape;
                if (shape == null) continue;
                if (aPanelIndex.HasValue && shape.PanelIndex != aPanelIndex.Value) continue;
                if (aPanelSectionIndex.HasValue && shape.PanelSectionIndex != aPanelSectionIndex.Value) continue;
                _rVal.Add(bGetClones ? new uopSectionShape(shape) : shape);
            }
            uopSectionShapes.SetMDSectionInstances(_rVal, aAssy, false);
            if (bGetUniques)
            {
                uopSectionShapes _rVal2 = new uopSectionShapes() { TrayAssembly = aAssy };
                _rVal2.AddRange(uopSectionShapes.GetUniqueMDShapes(aAssy, aShapes: _rVal));
                _rVal = _rVal2;
            }
            return _rVal;

        }


        /// <summary>
        /// returns True if the all the members will fit through the manhole
        /// </summary>
        public bool MembersFitThroughManhole(mdTrayAssembly aAssy, double ManID, mdDeckSections SearchCol, out mdDeckSections rBadSections)
        {
            mdDeckSections sCol = SearchCol ?? this;
            rBadSections = new mdDeckSections(sCol, bDontCopyMembers: true);
            aAssy ??= GetMDTrayAssembly(aAssy);
            if (aAssy == null) return true;

            if (ManID <= 0) ManID = aAssy.ManholeID;
            if (ManID <= 0) return true;
            foreach (uopPart item in sCol)
            {
                mdDeckSection mem = (mdDeckSection)item;
                if (!mem.FitsThroughManhole(aAssy, ref ManID)) rBadSections.Add(mem);
            }


            return rBadSections.Count == 0;
        }
        public void Populate(IEnumerable<mdDeckSection> aSections, bool bAddClones = false, string aCategory = null)
        {
            Clear();
            Invalid = false;
            if (aSections == null) return;
            foreach (mdDeckSection item in aSections)
            {
                mdDeckSection newmem =  Add(item, bAddClones);
                if (aCategory != null) newmem.Category = aCategory;
            }

        }

        public List<mdDeckSection> GetByAlternateRing1()
        {
            List<mdDeckSection> _rVal = FindAll(x => !x.IsVirtual && x.InstalledOnAlternateRing1).OfType<mdDeckSection>().ToList();
            return _rVal;
        }

        public List<mdDeckSection> GetByAlternateRing2()
        {
            List<mdDeckSection> _rVal = FindAll(x => !x.IsVirtual && x.InstalledOnAlternateRing2).OfType<mdDeckSection>().ToList();
            return _rVal;
        }

        public colDXFVectors BPCenters(bool bReturnBothSides = false)
        {
            colDXFVectors _rVal = new colDXFVectors();
            UVECTORS aBPs;
            dxfVector v1;

            foreach (uopPart item in this)
            {
                mdDeckSection mem = (mdDeckSection)item;
                aBPs = mem.BPSites;
                for (int j = 1; j <= aBPs.Count; j++)
                {
                    v1 = aBPs.Item(j).ToDXFVector();
                    _rVal.Add(v1);
                    if (bReturnBothSides)
                    {

                        if (Math.Round(mem.X, 1) > 0)
                        {
                            _rVal.Add(-v1.X, -v1.Y);
                        }
                    }
                }
            }

            return _rVal;
        }

        /// <summary>
        /// used to add an item to the collection
        /// won't add "Nothing" (no error raised).
        /// </summary>
        /// <param name="aSection">the item to add to the collection</param>
        /// <param name="bAddClone"></param>
        /// <returns></returns>

        public mdDeckSection Add(mdDeckSection aSection, bool bAddClone )
        {
            if (aSection == null) return null;

             base.Add(bAddClone ? new mdDeckSection(aSection) : aSection);
            return base[base.Count - 1];
        }

        /// <summary>
        /// used to add an item to the collection
        /// won't add "Nothing" (no error raised).
        /// </summary>
        /// <param name="aSection">the item to add to the collection</param>
     
        /// <returns></returns>

        public new mdDeckSection Add(mdDeckSection aSection)
        {
            if (aSection == null) return null;

            base.Add( aSection);
            return base[base.Count - 1];
        }
        /// <summary>
        /// add the members of the passed collection of sections to this on
        /// </summary>
        /// <param name="SecCol"></param>

        public void Append(IEnumerable<mdDeckSection> SecCol, bool bAddClones = false)
        {
            if (SecCol == null) return;
            foreach (mdDeckSection item in SecCol)
            {
                if (item == null) continue;
                Add(item,bAddClones);
            }

        }

        public colDXFVectors Centers(bool bSuppressField = false, bool bSuppressMoons = false, bool bSuppressManways = false, bool bSuppressEndSections = false, bool bReturnNegativeXs = false)
        {
            colDXFVectors _rVal = new colDXFVectors();

            foreach (uopPart item in this)
            {
                mdDeckSection mem = (mdDeckSection)item;
                bool bKeep = true;
                if (bSuppressField && mem.IsFieldSection) bKeep = false;

                if (bSuppressMoons && mem.IsHalfMoon) bKeep = false;

                if (bSuppressMoons && !mem.IsFieldSection) bKeep = false;

                if (mem.IsManway) bKeep = !bSuppressManways;

                if (bKeep)
                {
                    _rVal.Add(mem.CenterDXF);
                    if (bReturnNegativeXs)
                    {
                        if (Math.Round(mem.X, 1) > 0)
                        {
                            _rVal.Add(mem.CenterDXF);
                            _rVal.LastVector().X = -_rVal.LastVector().X;
                        }
                    }
                }
            }


            return _rVal;
        }

        public colDXFVectors GetSections(bool bSuppressField = false, bool bSuppressMoons = false, bool bSuppressManways = false, bool bSuppressEndSections = false, bool bReturnNegativeXs = false)
        {
            colDXFVectors _rVal = new colDXFVectors();

            foreach (uopPart item in this)
            {

                mdDeckSection aMem = (mdDeckSection)item;


                if (bSuppressField && aMem.IsFieldSection) continue;
                if (bSuppressMoons && aMem.IsHalfMoon) continue;
                if (bSuppressMoons && !aMem.IsFieldSection) continue;
                if (aMem.IsManway && bSuppressManways) continue;


                _rVal.Add(aMem.CenterDXF);
                if (bReturnNegativeXs)
                {
                    if (Math.Round(aMem.X, 1) > 0)
                    {
                        _rVal.Add(aMem.CenterDXF);
                        _rVal.LastVector().X = -_rVal.LastVector().X;
                    }
                }

            }
            return _rVal;
        }

        public uopRectangles Limits(int aPanelIndex = 0, List<mdDeckSection> aSearchList = null)
        {
            aSearchList ??= ToList(aPanelIndex);
            return mdDeckSections.GetLimits(aSearchList, aPanelIndex);
        }

        public colDXFVectors RingClipCenters(mdTrayAssembly aAssy, bool bBothSides = false, colDXFVectors aCollector = null)
        {

            aAssy ??= GetMDTrayAssembly(aAssy);
            if (aAssy == null || Count <= 0) return new colDXFVectors();

            colDXFVectors _rVal = mdDeckSections.GetHolesV(ToList(), aAssy, "RING CLIP", bTrayWide: bBothSides).Item(1).Centers.ToDXFVectors();

            if (aCollector != null) aCollector.Append(_rVal);

            double rad = mdGlobals.HoldDownWasherRadius;
            foreach (dxfVector item in _rVal)
            {
                item.Radius = rad;
            }

            //uopHole aHl;
            //mdDeckSection aMem;
            //uopHoles aHls;
            //dxfVector v1;

            //for (int i = 1; i <= Count; i++)
            //{
            //    aMem = Item(i);
            //    if (aMem.LapsRing)
            //    {
            //        aHls = aMem.GenHoles(aAssy, "RING CLIP").Item(1);
            //        for (int j = 0; j < aHls.Count; j++)
            //        {
            //            aHl = aHls.Item(j + 1);
            //            v1 = aHl.Center;
            //            double radius = rad;
            //            v1.Radius = radius;
            //            _rVal.Add(v1);
            //            if (bBothSides)
            //            {
            //                if (!aMem.IsCenterSection)
            //                {
            //                    v1 = v1.Clone();
            //                    v1.X = (-v1.X);
            //                    _rVal.Add(v1);
            //                }
            //            }
            //        }
            //    }
            //}
            return _rVal;
        }

        public uopRectangles SpliceLimits(mdTrayAssembly aAssy, double aAdder = 0) => mdSection_Generator.GetSpliceLimits(ToList(), aAssy, aAdder);


        /// <summary>
        ///returns an new collection whose members are clones of the members of this collection
        /// </summary>
        /// <param name="bReturnEmpty"></param>
        /// <returns></returns>
        public mdDeckSections Clone() => new mdDeckSections(this);

        object ICloneable.Clone() => (object)new mdDeckSections(this);

        public List<mdDeckSection> ToList(int aPanelIndex = 0, bool bReturnClones = false)
        {
            List<mdDeckSection> _rVal = new List<mdDeckSection>();
            foreach (var item in this)
            {
                mdDeckSection section = item;
                if (section.PanelIndex == aPanelIndex || aPanelIndex <= 0) _rVal.Add(!bReturnClones ? section : new mdDeckSection(section));
            }
            return _rVal;
        }

        public List<uopPart> ToParts(int aPanelIndex = 0, bool bReturnClones = false)
        {
            List<uopPart> _rVal = new List<uopPart>();
            foreach (var item in this)
            {
                mdDeckSection section = item;
                if (section.PanelIndex == aPanelIndex || aPanelIndex <= 0) _rVal.Add(!bReturnClones ? section : new mdDeckSection(section));
            }
            return _rVal;
        }

        public List<int> ManwayPanelIndexes()
        {
            List<int> _rVal = new List<int>();
            List<mdDeckSection> manways = this.Manways;
            if (manways.Count <= 0) return _rVal;

            foreach (mdDeckSection manway in manways)
            {
                if (!_rVal.Contains(manway.PanelIndex)) _rVal.Add(manway.PanelIndex);
            }
            return _rVal;
        }

        public List<int> PanelIndexes(out List<int> rPanelSectionCounts)
        {
            List<int> _rVal = new List<int>();
            rPanelSectionCounts = new List<int>();

            foreach (mdDeckSection mem in this)
            {
                if (!_rVal.Contains(mem.PanelIndex)) _rVal.Add(mem.PanelIndex);
            }
            _rVal.Sort();
            foreach (int item in _rVal)
            {
                rPanelSectionCounts.Add(FindAll((x) => x.PanelIndex == item).Count);
            }

            return _rVal;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aAssy">the fit value to match</param>
        /// <param name="aFits">the splices the define the sections</param>
        /// <param name="SearchCol"></param>
        /// <returns></returns>
        public mdDeckSections GetByFit(mdTrayAssembly aAssy, bool aFits, mdDeckSections SearchCol = null)
        {

            mdDeckSections sCol = SearchCol ?? this;
            aAssy ??= GetMDTrayAssembly(aAssy);
            mdDeckSections _rVal = new mdDeckSections(sCol, bDontCopyMembers: true);

            double manid = aAssy.ManholeID;

            foreach (uopPart item in sCol)
            {

                mdDeckSection aMem = (mdDeckSection)item;
                if (aMem.FitsThroughManhole(aAssy, ref manid) == aFits) _rVal.Add(aMem);

            }
            return _rVal;
        }
        /// <summary>
        ///returns the first section with a handle that matches the passed value
        /// </summary>
        /// <param name="aHandle"></param>
        /// <returns></returns>
        public mdDeckSection GetByHandle(string aHandle)
        {

            foreach (uopPart item in this)
            {

                mdDeckSection aMem = (mdDeckSection)item;
                if (aMem.Handle == aHandle) return aMem;
            }
            return null;
        }
        /// <summary>
        /// returns only the sections with the passed panel index
        /// </summary>
        /// <param name="aPanelIndex"></param>
        /// <param name="aSearchCol"></param>
        /// <returns></returns>
        public mdDeckSections GetByPanelIndex(int aPanelIndex)
        {
            mdDeckSections _rVal  = new mdDeckSections(this, bDontCopyMembers: true);
           _rVal.Populate(FindAll(x => x.PanelIndex == aPanelIndex));
            return _rVal;
        }


        /// <summary>
        /// returns only the sections with the passed panel index
        /// </summary>
        /// <param name="aPanelIndex"></param>
        /// <param name="bGetClones"></param>
        /// <returns></returns>
        public List<mdDeckSection> GetByPanelID(int aPanelIndex, bool bGetClones = false, bool bGetVirtuals = false)
        {
            List<mdDeckSection> _rVal = new List<mdDeckSection>();
            foreach (mdDeckSection item in this)
            {
                if (!bGetVirtuals && item.IsVirtual) continue;
                if (item.PanelIndex == aPanelIndex)
                {
                    if (!bGetClones)
                        _rVal.Add(item);
                    else
                        _rVal.Add(item.Clone());
                }
            }

            return _rVal;
        }


        /// <summary>
        /// returns lists of sections grouped by panel index
        /// </summary>
        /// <param name="bGetClones"></param>
        /// <returns></returns>
        public List<List<mdDeckSection>> GetSortedByPanelID(bool bGetClones = false, bool bGetVirtuals = false, bool? bISOnAlternameRing1 = null, bool? bISOnAlternameRing2 = null)
        {
            List<List<mdDeckSection>> _rVal = new List<List<mdDeckSection>>();

            foreach (mdDeckSection item in this)
            {
                if (!bGetVirtuals && item.IsVirtual) continue;
                if (bISOnAlternameRing1.HasValue && item.InstalledOnAlternateRing1 != bISOnAlternameRing1.Value)
                    continue;
                if (bISOnAlternameRing2.HasValue && item.InstalledOnAlternateRing2 != bISOnAlternameRing2.Value)
                    continue;

                int pid = item.PanelIndex;
                while (pid > _rVal.Count)
                {
                    _rVal.Add(new List<mdDeckSection>());
                }

                if (!bGetClones)
                    _rVal[pid - 1].Add(item);
                else
                    _rVal[pid - 1].Add(item.Clone());
            }

            return _rVal;
        }



        /// <summary>
        /// returns lists of sections grouped by panel index
        /// </summary>
        /// <param name="bGetClones"></param>
        /// <returns></returns>
        public List<List<mdDeckSection>> GetByAlternateRingSection(bool bGetTopRing, bool bGetClones = false)
        {
            List<List<mdDeckSection>> _rVal = new List<List<mdDeckSection>>();

            foreach (mdDeckSection item in this)
            {

                if (item.IsVirtual) continue;
                if (bGetTopRing)
                {
                    if (!item.InstalledOnAlternateRing1) continue;
                    if (item.IsManway && item.Parent != null) continue;
                }
                else
                {
                    if (!item.InstalledOnAlternateRing2) continue;
                    if (item.IsManway && item.Parent == null) continue;
                }


                int pid = item.PanelIndex;
                while (pid > _rVal.Count)
                {
                    _rVal.Add(new List<mdDeckSection>());
                }

                if (!bGetClones)
                    _rVal[pid - 1].Add(item);
                else
                    _rVal[pid - 1].Add(item.Clone());
            }

            return _rVal;
        }

        /// <summary>
        ///returns all the sections in the requested quadrant
        /// </summary>
        /// <param name="Quad">the quadrant to look for</param>
        /// <param name="SearchCol"></param>
        /// <returns></returns>
        public mdDeckSections GetByQuadrant(int Quad = 1, mdDeckSections SearchCol = null)
        {
            mdDeckSections sCol = SearchCol ?? this;
            Quad = mzUtils.LimitedValue(Quad, 1, 4);

            mdDeckSections _rVal = new mdDeckSections(sCol, bDontCopyMembers: true);

            foreach (uopPart item in sCol)
            {

                mdDeckSection aMem = (mdDeckSection)item;
                if (aMem.Quadrant == Quad)
                {
                    _rVal.Add(aMem);
                }
            }
            return _rVal;
        }
        /// <summary>
        ///returns all the sections that have a unique index matching the passed value
        /// </summary>
        /// <param name="aUniqueIndex">the unique index to search for</param>
        /// <param name="aSearchCol"></param>
        /// <returns></returns>
        public List<mdDeckSection> GetByUniqueIndex(int aUniqueIndex, mdDeckSections aSearchCol = null)
        {
            mdDeckSections sCol = aSearchCol ?? this;
            List<mdDeckSection> srch = sCol.ToList();
            List<mdDeckSection> _rVal = new List<mdDeckSection>();

            foreach (uopPart item in sCol)
            {

                mdDeckSection aMem = (mdDeckSection)item;
                if (aMem.UniqueIndex == aUniqueIndex) _rVal.Add(aMem);

            }
            return _rVal;
        }
        /// <summary>
        ///returns all the sections that have a unique ID matching the passed value
        /// </summary>
        /// <param name="aUniqueID">the unique ID to search for</param>
        /// <param name="aPanelIndex"></param>
        /// <returns></returns>
        public List<mdDeckSection> GetByUniqueID(int aUniqueID, int aPanelIndex = 0)
        {
            List<mdDeckSection> _rVal = new List<mdDeckSection>();

            foreach (uopPart item in this)
            {

                mdDeckSection aMem = (mdDeckSection)item;
                if (aPanelIndex <= 0 || aMem.PanelIndex == aPanelIndex)
                {
                    if (aMem.UniqueID == aUniqueID) _rVal.Add(aMem);

                }
            }
            return _rVal;
        }

        public mdDeckSections GetByY(double aY, double aBuffer = 0, int aPanelIndex = 0, mdDeckSections aSearchCol = null)
        {


            mdDeckSections sCol = aSearchCol ?? this;
            mdDeckSections _rVal = new mdDeckSections(sCol, bDontCopyMembers: true);
            aBuffer = Math.Abs(aBuffer);
            foreach (uopPart item in sCol)
            {

                mdDeckSection aMem = (mdDeckSection)item;
                if (aPanelIndex == 0 || (aPanelIndex != 0 & aMem.PanelIndex == aPanelIndex))
                {
                    if (aY <= aMem.Top + aBuffer && aY >= aMem.Bottom - aBuffer) _rVal.Add(aMem);

                }
            }
            return _rVal;
        }

        public virtual List<mdDeckSection> GetByVirtual(bool? aVirtualValue, bool bGetClones = false)
        {
            List<mdDeckSection> _rVal = new List<mdDeckSection>();
            foreach (var item in this)
            {
                if (aVirtualValue.HasValue)
                {
                    if (item.IsVirtual != aVirtualValue.Value) continue;
                }
                mdDeckSection mem = (mdDeckSection)item;
                _rVal.Add(!bGetClones ? mem : mem.Clone());
            }
            return _rVal;
        }

        /// <summary>
        ///returns the section with the matching panel index and section index
        /// </summary>
        /// <param name="PanelID"></param>
        /// <param name="SectionID"></param>
        /// <returns></returns>
        public mdDeckSection GetSection(int PanelID, int SectionID)
        {

            mdDeckSections pSections = GetByPanelIndex(PanelID);
            foreach (uopPart item in pSections)
            {

                mdDeckSection aMem = (mdDeckSection)item;
                if (aMem.SectionIndex == SectionID) return aMem;
            }

            return null;
        }

        /// <summary>
        /// all the holes of all the sections in the collection
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aTag"></param>
        /// <param name="aFlag"></param>
        /// <param name="bTrayWide"></param>
        /// <returns></returns>
        public uopHoleArray GenHoles(mdTrayAssembly aAssy, string aTag = "", string aFlag = "", bool bTrayWide = false)
       => mdDeckSections.GetHoles(FindAll(x => x.InstalledOnAlternateRing1).OfType<mdDeckSection>().ToList(), aAssy, aTag, aFlag, bTrayWide);

        /// <summary>
        ///returns the item from the collection at the requested index
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>

        /// <summary>
        ///returns the item from the collection at the requested index ! Base 1 !
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
        /// 
        public  mdDeckSection Item(int aIndex, bool bSuppressIndexError)
        {
            if (aIndex < 1 || aIndex > Count)
            {
                if (!bSuppressIndexError) throw new IndexOutOfRangeException(); else return null;
            }
            return this[aIndex -1];
        }

        /// <summary>
        ///returns the item from the collection at the requested index ! Base 1 !
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
        /// 
        public virtual mdDeckSection Item(int aIndex)
        {
            if (aIndex < 1 || aIndex > Count)
                throw new IndexOutOfRangeException();
            return this[aIndex - 1];
        }


        /// <summary>
        /// returns the handles of the manways in the collection
        /// </summary>
        /// <returns></returns>
        public List<string> ManwayHandles()
        {
            List<string> _rVal = new List<string>();

            foreach (uopPart item in this)
            {
                mdDeckSection aMem = (mdDeckSection)item;
                if (aMem.IsManway) _rVal.Add(aMem.Handle);
            }

            return _rVal;
        }

        /// <summary>
        /// returns the handles of the half moons in the collection
        /// </summary>
        /// <returns></returns>
        public List<string> MoonHandles()
        {
            List<string> _rVal = new List<string>();
            foreach (uopPart item in this)
            {
                mdDeckSection aMem = (mdDeckSection)item;
                if (aMem.IsHalfMoon) _rVal.Add(aMem.Handle);

            }
            return _rVal;
        }


        /// <summary>
        /// the manway fasteners of the 
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aTopSideOnly"></param>
        /// <param name="aBothSides"></param>
        /// <returns></returns>
        public colUOPParts ManwayFasteners(mdTrayAssembly aAssy, bool aTopSideOnly = false, bool aBothSides = false)
        {
            colUOPParts _rVal = new colUOPParts();

            aAssy ??= GetMDTrayAssembly(aAssy);

            if (aAssy == null) return _rVal;

            foreach (uopPart item in this)
            {
                mdDeckSection aMem = (mdDeckSection)item;
                if (aMem.IsManway)
                    aMem.ManwayFasteners(aAssy, aTopSideOnly, aBothSides, _rVal);

            }
            _rVal.SubPart(aAssy);
            return _rVal;
        }

        public  uopDocuments Warnings() => GenerateWarnings(null);

        /// <summary>
        ///returns a collection of strings that are warnings about possible problems with
        /// the current tray assembly design.
        /// these warnings may or may not be fatal problems.
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aCategory"></param>
        /// <returns></returns>
        public uopDocuments GenerateWarnings(mdTrayAssembly aAssy, string aCategory = "", uopDocuments aCollector = null, bool bJustOne = false)
        {
            uopDocuments _rVal = aCollector ?? new uopDocuments();
            if (bJustOne && _rVal.Count > 0) return _rVal;
            aAssy ??= GetMDTrayAssembly();

            if (aAssy == null) return _rVal;

            aCategory = string.IsNullOrWhiteSpace(aCategory) ? aAssy.TrayName(true) : aCategory.Trim();
            mdTrayRange Rng = null;

            string txt = string.Empty;
            double mhid = 0;

            int cnt = 0;
            int cnt2 = 0;
            double lim = 0;
            bool bLengthViol = false;
            double dev = 0;
            double dif = 0;

            uopVectors fCPts = uopVectors.Zero;


            TVALUES noFit = new TVALUES("");
            TVALUES clrcFit = new TVALUES("");
            TVALUES noClips = new TVALUES("");


            try
            {
                if (aAssy.DesignOptions.SpliceStyle == uppSpliceStyles.Tabs && aAssy.ProjectType == uppProjectTypes.MDDraw)
                {
                    if (aAssy.DesignOptions.SpliceStyle == uppSpliceStyles.Tabs)
                        fCPts = mdUtils.FingerClipPoints(aAssy, null, null, bGetEndAnglePts: true);

                }
                mhid = aAssy.ManholeID;
                Rng = aAssy.GetMDRange();
                foreach (uopPart item in this)
                {
                    mdDeckSection aMem = (mdDeckSection)item;
                    if (!aMem.FitsThroughManhole(aAssy, ref mhid, out double clrc))
                    {
                        noFit.Add(aMem.Handle);
                    }
                    else
                    {
                        if (clrc > 0 & clrc < 0.5) clrcFit.Add(aMem.Handle);

                    }
                    if (fCPts.Count > 0)
                    {
                        if (!aMem.IsManway && !aMem.IsHalfMoon)
                        {
                            if (!mdSection_Generator.SectionIsHeldDown(aAssy, aMem, fCPts))
                            {
                                noClips.Add(aMem.Handle);
                                txt = $"Deck Section {aMem.Handle} Is Not Held Down By a Finger Clip or End Angle";
                                _rVal.AddWarning(aAssy, "Panel Hold Down Warning", txt);
                                if (bJustOne && _rVal.Count > 0) return _rVal;
                            }
                        }
                    }
                    if (!bLengthViol)
                    {
                        if (aMem.Bounds.Height > 150)
                        {
                            bLengthViol = true;
                            txt = $"Deck Section {aMem.Handle} Is Longer than {150} Inches";
                            _rVal.AddWarning(aAssy, "Deck Section Length Warning", txt);
                            if (bJustOne && _rVal.Count > 0) return _rVal;
                        }
                    }
                }
                cnt = aAssy.ManwayCount;
                cnt2 = aAssy.Deck.ManwayCount;
                if (cnt < cnt2)
                {
                    txt = $"The Number Of Designated Manways ({cnt}) Is Less Than The Number Of Manways ({cnt2})  Requested By Functional Engineer";
                    _rVal.AddWarning(aAssy, "Manway Count Warning", txt);
                    if (bJustOne && _rVal.Count > 0) return _rVal;
                }
                if (aAssy.DesignFamily.IsEcmdDesignFamily())
                {
                    lim = 2;
                    cnt = aAssy.SlotZones.TotalSlotCount(aAssy);
                    cnt2 = aAssy.RequiredSlotCount;
                    dif = Math.Abs(cnt - cnt2);
                    dev = (cnt2 > 0) ? dif / cnt2 * 100 : 100;

                    txt = $"The Actual ECMD Slot Count Deviates From The Required Slot Count By More Than {string.Format("{0:0.00}", lim)}%";
                    if (dev > lim)
                    {
                        _rVal.AddWarning(aAssy, "Slot Count Warning", txt);
                        if (bJustOne && _rVal.Count > 0) return _rVal;
                    }

                }
                if (noFit.Count > 0)
                {
                    txt = $"Deck Sections {noFit.ToDelimitedList(", ", aLastDelim: "&")} Will Not Fit Through ({string.Format("{0:0.0000}", mhid)}'') Manhole";
                    _rVal.AddWarning(aAssy, "Deck Section Manhole Warning", txt);
                    if (bJustOne && _rVal.Count > 0) return _rVal;
                }
                if (clrcFit.Count > 0)
                {
                    txt = $"Deck Sections {clrcFit.ToDelimitedList(", ", aLastDelim: " & ")} Will Fit Through ({string.Format("{0:0.0000}", mhid)}'') Manhole But The Clearance Is Less Than 0.5''";
                    _rVal.AddWarning(aAssy, "Deck Section Manhole Warning", txt);
                    if (bJustOne && _rVal.Count > 0) return _rVal;
                }
            }
            catch (Exception exception)
            {
                LoggerManager log = new LoggerManager();
                log.LogError(exception.Message);
            }
            return _rVal;
        }


        /// <summary>
        /// all the holes of all the sections in the collection
        /// </summary>
        /// <param name="aSections"></param>
        /// <param name="aAssy"></param>
        /// <param name="aTag"></param>
        /// <param name="aFlag"></param>
        /// <param name="bTrayWide"></param>
        /// <returns></returns>
        public static uopHoleArray GetHoles(List<mdDeckSection> aSections, mdTrayAssembly aAssy, string aTag = "", string aFlag = "", bool bTrayWide = false)
       => new uopHoleArray(GetHolesV(aSections, aAssy, aTag, aFlag, bTrayWide));


        /// <summary>
        /// executed internally to create the holes in the section
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aTag"></param>
        /// <param name="aFlag"></param>
        /// <param name="bTrayWide"></param>
        /// <returns></returns>
        internal static UHOLEARRAY GetHolesV(List<mdDeckSection> aSections, mdTrayAssembly aAssy, string aTag = "", string aFlag = "", bool bTrayWide = false)
        {
            UHOLEARRAY _rVal = UHOLEARRAY.Null; ;

            if (aAssy == null || aSections == null) return _rVal;
            foreach (var item in aSections)
            {
                UHOLEARRAY memHls = item.GenHolesV(aAssy, aTag, aFlag, bTrayWide);
                _rVal.Append(memHls, bAppendToExisting: true);
            }

            return _rVal;
        }

        public static uopRectangles GetLimits(List<mdDeckSection> aSections, int aPanelIndex = 0)
        {
            uopRectangles _rVal = new uopRectangles();
            if (aSections == null) return _rVal;
            mdDeckSection aMem;
            for (int i = 1; i <= aSections.Count; i++)
            {
                aMem = aSections[i - 1];
                if (aMem.IsVirtual) continue;
                //to do need to check condition
                if (aPanelIndex <= 0 || aMem.PanelIndex == aPanelIndex)
                {
                    uopRectangle rec = new uopRectangle(aMem.Limits)
                    {
                        Tag = aMem.Handle
                    };
                    _rVal.Add(rec);
                }
            }
            return _rVal;
        }

        /// <summary>
        ///returns the number of items in the collection
        /// </summary>
        /// <param name="rPanelList"></param>
        /// <param name="rMaxPanelID"></param>
        /// <returns></returns>
        public static int GetCount(IEnumerable<mdDeckSection> aSections, out string rPanelList, out int rMaxPanelID)
        {

            rPanelList = string.Empty;
            rMaxPanelID = 0;

            if (aSections == null) return 0;
            int _rVal = aSections.Count();
            foreach (var item in aSections)
            {
                if (item.PanelIndex > rMaxPanelID) rMaxPanelID = item.PanelIndex;

                mzUtils.ListAdd(ref rPanelList, item.PanelIndex);
            }

            return _rVal;
        }
 
        public void SubPart(mdTrayAssembly aAssy)
        {
            if (aAssy == null)  return; 
           
            TrayAssembly = aAssy;
        }


        /// <summary>
        /// executed  to create the splice angles collection based on the current splices
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="bSetInstances"></param>
        /// <returns></returns>
        public List<mdSpliceAngle> GenerateSpliceAngles(mdTrayAssembly aAssy, bool bSetInstances = false, uppSpliceAngleTypes? aFilter = null) => mdDeckSections.GenerateSectionSpliceAngles(ToList(), GetMDTrayAssembly(aAssy), bSetInstances, aFilter);

        /// <summary>
        /// creates the subset of the assemblies deck sections which are the unique members with their instances set for each occurance in the assembly
        /// </summary>
        /// <param name="aAssy">the parent assembly</param>
        /// <param name="rAltRingSections">returns the deck sections for the alternate ring if the assembly has alternating deck section arrangements</param>
        /// <returns></returns>
        public List<mdDeckSection> UniqueSections(mdTrayAssembly aAssy, out List<mdDeckSection> rAltRingSections)
        {
            rAltRingSections = new List<mdDeckSection>();
            List<mdDeckSection> _rVal = new List<mdDeckSection>();
            mdTrayAssembly assy = GetMDTrayAssembly(aAssy);
            if (assy == null) return _rVal;

            HasAlternateSections = aAssy.HasAlternateDeckParts;
            assy.SetPartGenerationStatus($"Generating {assy.TrayName()} - Unique Deck Sections", true);
            uopDeckSplices splices = new uopDeckSplices(assy, assy.DeckSplices);
            _rVal = mdSection_Generator.UniqueSections(this, assy, splices);
            if (HasAlternateSections)
            {
                assy.SetPartGenerationStatus($"Generating {assy.TrayName()} - Alternate Ring Unique Deck Sections", true);

                rAltRingSections = mdSection_Generator.UniqueAltRingSections(this, _rVal, assy, splices);
            }

                return _rVal;
        }



        public List<uopDeckSplice> GetSplices(bool bGetTops = true, bool bGetBottoms = true, bool bGetClones = false, int? aPanelIndex = null, uppSpliceTypes? aType = null, uppSpliceTypes? bType = null, List<mdDeckSection> aCollector = null) => mdDeckSections.GetSectionSplices(this, bGetTops, bGetBottoms, bGetClones, aPanelIndex, aType, bType, aCollector);


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
            
            List<uopPart> asparts = uopParts.SortPartsByOrdinate(ToParts(),aOrdinateType,bLowToHigh,bNoDupes,aPrecis);
            base.Clear();
            foreach(var part in asparts)
            {
                base.Add((mdDeckSection)part);
            }
            
        }

        #endregion Methods

        #region Shared Methods
        public static List<uopDeckSplice> GetSectionSplices(IEnumerable<mdDeckSection> aSections, bool bGetTops = true, bool bGetBottoms = true, bool bGetClones = false, int? aPanelIndex = null, uppSpliceTypes? aType = null, uppSpliceTypes? bType = null, List<mdDeckSection> aCollector = null)
        {
            List<uopDeckSplice> _rVal = new List<uopDeckSplice>();
            if (aSections == null) return _rVal;
            foreach (mdDeckSection section in aSections)
            {
                if (aPanelIndex.HasValue && section.PanelIndex != aPanelIndex.Value) continue;
                section.GetSplices(out uopDeckSplice top, out uopDeckSplice bot);
                if (bGetTops && top != null)
                {
                    bool keep = true;
                    if (aType.HasValue || bType.HasValue)
                    {
                        keep = false;
                        if (aType.HasValue && top.SpliceType == aType.Value) keep = true;
                        if (bType.HasValue && top.SpliceType == bType.Value) keep = true;
                    }
                    if (keep)
                    {
                        if (aCollector != null) aCollector.Add(section);
                        _rVal.Add(bGetClones ? new uopDeckSplice(top) : top);
                    }
                }

                if (bGetBottoms && bot != null)
                {
                    bool keep = true;
                    if (aType.HasValue || bType.HasValue)
                    {
                        keep = false;
                        if (aType.HasValue && bot.SpliceType == aType.Value) keep = true;
                        if (bType.HasValue && bot.SpliceType == bType.Value) keep = true;
                    }
                    if (keep)
                    {
                        if (aCollector != null) aCollector.Add(section);
                        _rVal.Add(bGetClones ? new uopDeckSplice(bot) : bot);
                    }
                }

            }
            return _rVal;
        }

        /// <summary>
        /// executed  to create the splice angles collection based on the current splices
        /// </summary>
        /// <param name="aSections"></param>
        /// <param name="aAssy"></param>
        /// <param name="bSetInstances"></param>
        /// <param name="aFilter"></param>
        /// /// <returns></returns>
        public static List<mdSpliceAngle> GenerateSectionSpliceAngles(List<mdDeckSection> aSections, mdTrayAssembly aAssy, bool bSetInstances = false, uppSpliceAngleTypes? aFilter = null)
        {
            List<mdSpliceAngle> _rVal = new List<mdSpliceAngle>();
            if (aSections == null || aSections.Count <= 0) return _rVal;
            try
            {
                List<mdDeckSection> owners = new List<mdDeckSection>();
                mdDeckSection section = null;
                mdSpliceAngle angle = null;
                List<mdSpliceAngle> splangles = null;
                //if (!aFilter.HasValue || (aFilter.HasValue && aFilter.Value == uppSpliceAngleTypes.SpliceAngle ))
                //{
                //    owners = aSections.FindAll(x => x.SpliceAngleType(false) == uppSpliceAngleTypes.SpliceAngle);
                //    if(owners.Count > 0)
                //    {
                //        splangles = new List<mdSpliceAngle>();
                //        for (int i = 1; i <=  owners.Count; i++) 
                //        {
                //            section = owners[i -1];
                //            angle = new mdSpliceAngle(aAssy, section.BottomSplice, section);
                //            uopVectors sectionips = section.Instances.MemberPoints(section.Center, true);

                //        }
                //    }

                List<uopDeckSplice> splices = GetSectionSplices(aSections, bGetTops: false, bGetBottoms: true, aType: uppSpliceTypes.SpliceWithAngle, bType: uppSpliceTypes.SpliceManwayCenter, aCollector: owners);


                for (int i = 1; i <= splices.Count; i++)
                {
                    uopDeckSplice splice = splices[i - 1];
                    section = owners[i - 1];
                    angle = new mdSpliceAngle(aAssy, splice, section);
                    if (!aFilter.HasValue || (aFilter.HasValue && angle.AngleType == aFilter.Value))
                    {
                        //if(section.Instances.Count > 0)
                        //{
                        //    uopVector offset = section.Center - splice.Center;
                        //    uopInstances.Combine(angle.Instances, section.Instances, offset);
                        //}


                        _rVal.Add(angle);
                    }
                }
                if (bSetInstances)
                {

                    uopInstances insts = null;
                    splangles = _rVal.FindAll(x => x.AngleType == uppSpliceAngleTypes.SpliceAngle);
                    if (splangles.Count > 0)
                    {
                        _rVal.RemoveAll(x => x.AngleType == uppSpliceAngleTypes.SpliceAngle);
                        angle = splangles[0];
                        insts = angle.Instances;
                        section = angle.DeckSection;
                        for (int i = 2; i <= splangles.Count; i++)
                        {
                            mdSpliceAngle ang = splangles[i - 1];
                            uopVector delta = ang.Center - angle.Center;
                            insts.Add(delta);
                            if (ang.Instances.Count > 0)
                                insts.AppendWithPoints(ang.Instances.MemberPoints(ang.Center, aReturnBasePt: false), angle.Center);
                        }
                        _rVal.Add(angle);
                    }
                    splangles = _rVal.FindAll(x => x.AngleType == uppSpliceAngleTypes.ManwayAngle);
                    if (splangles.Count > 0)
                    {
                        _rVal.RemoveAll(x => x.AngleType == uppSpliceAngleTypes.ManwayAngle);
                        angle = splangles[0];
                        insts = angle.Instances;
                        insts.Clear();
                        uopDeckSplice splice = angle.Splice;

                        double rot = 180;
                        insts.BaseRotation = rot;
                        for (int i = 2; i <= splangles.Count; i++)
                        {
                            mdSpliceAngle ang = splangles[i - 1];
                            uopVector delta = ang.Center - angle.Center;
                            insts.Add(delta, rot);
                            if (ang.Instances.Count > 0)
                                insts.AppendWithPoints(ang.Instances.MemberPoints(ang.Center, aReturnBasePt: false), angle.Center);

                            rot = rot == 180 ? 0 : 180;
                        }
                        _rVal.Add(angle);
                    }
                    splangles = _rVal.FindAll(x => x.AngleType == uppSpliceAngleTypes.ManwaySplicePlate);
                    if (splangles.Count > 0)
                    {
                        _rVal.RemoveAll(x => x.AngleType == uppSpliceAngleTypes.ManwaySplicePlate);
                        angle = splangles[0];
                        insts = angle.Instances;
                        for (int i = 2; i <= splangles.Count; i++)
                        {
                            mdSpliceAngle ang = splangles[i - 1];
                            uopVector delta = ang.Center - angle.Center;
                            insts.Add(delta);
                            if (ang.Instances.Count > 0)
                                insts.AppendWithPoints(ang.Instances.MemberPoints(ang.Center, aReturnBasePt: false), angle.Center);
                        }
                        _rVal.Add(angle);
                    }
                }

                //List<mdDeckPanel> DPs = aAssy.DeckPanels.ActivePanels(aAssy, out bool specialcase, out _);

                //mdSpliceAngle aSA = null;
                //mdSpliceAngle SAng = aAssy.SpliceAngle(false);
                //mdSpliceAngle MAng = aAssy.SpliceAngle(true);
                //bool mirrors = aAssy.IsStandardDesign && !aAssy.OddDowncomers;

                //double dkthk = aAssy.Deck.Thickness;

                //for (int pid = 1; pid <= DPs.Count; pid++)
                //{
                //    List<uopDeckSplice> pSplices = FindAll(x => x.PanelIndex == pid);
                //    for (int j = 1; j <= pSplices.Count; j++)
                //    {
                //        uopDeckSplice aDS = pSplices[j - 1];
                //        if (aDS.SpliceType == uppSpliceTypes.SpliceManwayCenter)
                //        {
                //            //manwya centers for slot and tab
                //            aSA = new mdSpliceAngle(MAng) { PanelIndex = aDS.PanelIndex, SpliceHandle = aDS.Handle, Center = new uopVector(aDS.Center), SupportsManway = false, Direction = dxxRadialDirections.TowardsCenter, IsManwaySplice = true };

                //            aSA.Z = 0.5 * dkthk;
                //            aSA.PartType = uppPartTypes.ManwaySplicePlate;
                //            _rVal.Add(aSA);
                //            if (bTrayWide && Math.Round(aDS.X, 1) > 0 && mirrors)
                //            {
                //                aSA = aSA.Clone();
                //                aSA.X = -aSA.X;
                //                aSA.Y = -aSA.Y;
                //                aSA.PartIndex = _rVal.Count + 1;
                //                _rVal.Add(aSA);
                //            }
                //        }
                //        else if (aDS.SpliceType == uppSpliceTypes.SpliceWithAngle)
                //        {
                //            aSA = new mdSpliceAngle(SAng) { PanelIndex = aDS.PanelIndex, SpliceHandle = aDS.Handle, Center = new uopVector(aDS.Center), SupportsManway = aDS.SupportsManway };
                //            aSA.PartType = uppPartTypes.SpliceAngle;
                //            if (aSA.SupportsManway)
                //            {
                //                aSA.PartType = uppPartTypes.ManwayAngle;
                //                if (aDS.Y > 0)
                //                {
                //                    aSA.Direction = (aDS.Direction == dxxOrthoDirections.Down) ? dxxRadialDirections.TowardsCenter : dxxRadialDirections.AwayFromCenter;
                //                }
                //                else
                //                {
                //                    aSA.Direction = (aDS.Direction == dxxOrthoDirections.Up) ? dxxRadialDirections.TowardsCenter : dxxRadialDirections.AwayFromCenter;
                //                }

                //                aSA.Y = (aDS.Direction == dxxOrthoDirections.Down) ? aDS.Y + 0.1875 : aDS.Y - 0.1875;
                //            }
                //            else
                //            {
                //                aSA.SupportsManway = false;
                //                aSA.Direction = dxxRadialDirections.TowardsCenter;
                //                aSA.IsManwaySplice = false;
                //                aSA.PartType = uppPartTypes.SpliceAngle;
                //            }
                //            aSA.PartIndex = _rVal.Count + 1;
                //            _rVal.Add(aSA);
                //            if (bTrayWide && Math.Round(aDS.X, 1) > 0 && mirrors)
                //            {
                //                bool addit = !specialcase;
                //                if (specialcase)
                //                {
                //                    addit = pid! < DPs.Count - 1;
                //                }
                //                if (addit)
                //                {
                //                    aSA = aSA.Clone();
                //                    aSA.X = -aSA.X;
                //                    aSA.Y = -aSA.Y;
                //                    _rVal.Add(aSA);
                //                }


                //            }
                //        }
                //    }
                // }
            }
            catch (Exception e)
            {
                throw e;
            }
            return _rVal;
        }
        #endregion Shared Methods
    }
}
