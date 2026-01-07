using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UOP.DXFGraphics;
using UOP.DXFGraphics.Utilities;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Interfaces;
using UOP.WinTray.Projects.Materials;
using UOP.WinTray.Projects.Parts;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;
using static System.Collections.Specialized.BitVector32;

namespace UOP.WinTray.Projects
{
    public class uopSectionShapes : List<uopSectionShape>, IEnumerable<uopSectionShape>, ICloneable
    {

        #region Events
        public delegate void DeckShapesInvalidatedHandler();
        public event DeckShapesInvalidatedHandler eventDeckShapesInvalidated;

        #endregion Events
        #region Constructors
        public uopSectionShapes() => Init();
        public uopSectionShapes(uopTrayAssembly aAssy, DowncomerDataSet aDataSet = null) => Init(aAssy: aAssy, aDataSet: aDataSet);
        public uopSectionShapes(IEnumerable<uopSectionShape> aShapes = null, bool bDontCloneMembers = false, uopTrayAssembly aAssy = null, bool bReturnEmpty = false) => Init(aShapes, bDontCloneMembers, aAssy, bReturnEmpty);

        private void Init(IEnumerable<uopSectionShape> aShapes = null, bool bDontCloneMembers = false, uopTrayAssembly aAssy = null, bool bReturnEmpty = false, DowncomerDataSet aDataSet = null)
        {
            RingClipSpacing = mdGlobals.DefaultRingClipSpacing;
            MoonRingClipSpacing = mdGlobals.DefaultRingClipSpacing;
            HasBubblePromoters = false;
            Divider = null;
            _DowncomerData = DowncomerDataSet.CloneCopy(aDataSet);
            TrayAssembly = aAssy;
            if (aDataSet != null) Divider = new DividerInfo(aDataSet.Divider);


                base.Clear();
            if (aShapes == null) return;
            if (aShapes.GetType() == typeof(uopSectionShapes))
            {
                Copy((uopSectionShapes)aShapes, bDontCloneMembers: bDontCloneMembers, bDontCopyMembers : bReturnEmpty);

            }
            else
            {
                if (!bReturnEmpty)
                {
                    foreach (var shape in aShapes)
                        Add(shape, !bDontCloneMembers);
                }
               
            }

        }
        #endregion Constructors

        #region Properties
        public string RangeGUID { get; private set; }

        public double RingRadius { get; set; }

        public double DeckRadius { get; set; }

        public double ShellRadius { get; set; }
        public double ShelfWidth { get; set; }

        public double SpliceAngleLength { get; set; }
        public int SpliceBoltCount { get; set; }

        public bool HasBubblePromoters { get; set; }

        public double RingClipSpacing { get; set; }
        public double MoonRingClipSpacing { get; set; }

        private WeakReference<uopTrayAssembly> _AssyRef;
        public uopTrayAssembly TrayAssembly
        {
            get
            {
                if (_AssyRef == null) return null;
                if (!_AssyRef.TryGetTarget(out uopTrayAssembly _rVal)) _AssyRef = null;
                return _rVal;

            }

            set
            {
                if (value == null) { _AssyRef = null; return; }
                _AssyRef = new WeakReference<uopTrayAssembly>(value);
                _SpliceStyle = value.SpliceStyle;
                ManholeID = value.ManholeID;
                RangeGUID = value.RangeGUID;
                JoggleAngleHeight = value.JoggleAngleHeight;
                ProjectFamily = value.ProjectFamily;
                TrayName = value.TrayName(true);
                RingRadius = value.RingRadius;
                DeckRadius = value.DeckRadius;
                ShellRadius = value.ShellRadius;
                if (value.ProjectFamily == uppProjectFamilies.uopFamMD)
                {
                    
                    mdTrayAssembly assy = (mdTrayAssembly)value;
                    _DowncomerData ??= new DowncomerDataSet(assy.DowncomerData);
                    MDDesignFamily = assy.DesignFamily;
                    ShelfWidth = assy.Downcomer(0).ShelfWidth;
                    SpliceAngleLength = assy.SpliceFlangeLength(out int bcnt);
                    SpliceBoltCount = bcnt;
                    RingClipSpacing = assy.DesignOptions.MaxRingClipSpacing;
                    MoonRingClipSpacing = assy.DesignOptions.MoonRingClipSpacing;
                     Divider = _DowncomerData == null ?  assy.Divider : new DividerInfo(_DowncomerData.Divider);
                    HasBubblePromoters = assy.DesignOptions.HasBubblePromoters;
                }

            }
        }
        private DowncomerDataSet _DowncomerData;
        public DowncomerDataSet DowncomerData
        {
            get
            {
                if (_DowncomerData != null) return _DowncomerData;
                mdTrayAssembly assy = GetMDTrayAssembly();
                return assy == null ? null : assy.DowncomerData;
            }
            set { _DowncomerData = value; }
        }

        private double _JoggleAngleHeight;
        public double JoggleAngleHeight
        {
            get => _JoggleAngleHeight;
            private set
            {
                _JoggleAngleHeight = value;
                foreach (var item in this) item.FlangeHt = value;
            }

        }

        public double ManholeID { get; set; }

        private double _ManwayHeight = 0;
        /// <summary>
        /// the gross height of all manways in the collection
        /// </summary>
        public double ManwayHeight
        {
            get
            {

                if (_ManwayHeight < 8)
                {
                    _ManwayHeight = GetManwayHt();
                    if (_ManwayHeight < 8)
                    {
                        mdTrayAssembly aAssy = GetMDTrayAssembly();
                        if (aAssy != null)
                        {
                            _ManwayHeight = mdUtils.IdealManwayHeight(aAssy,SpliceStyle);
                        }
                        else
                        {
                            return 24;
                        }
                    }
                }

                return _ManwayHeight;
            }
            set
            {
                _ManwayHeight = value;
            }
        }
        private uppSpliceStyles _SpliceStyle;
        /// <summary>
        /// the basic style that governs the configuration based on its ordinate
        /// </summary>
        public uppSpliceStyles SpliceStyle
        {
            get
            {
                if ((int)_SpliceStyle < 0 || (int)_SpliceStyle > 2)
                {
                    uopTrayAssembly aAssy = TrayAssembly;
                    if (aAssy != null) _SpliceStyle = aAssy.SpliceStyle;

                }
                return _SpliceStyle;
            }
            set
            {
                if (value >= 0 & (int)value <= 2)
                {
                    if (_SpliceStyle == value) return;
                    _SpliceStyle = value;

                }
            }
        }

        private bool _Invalid;
        public bool Invalid { get => _Invalid || Count <= 0; set { if (_Invalid == value) return; _Invalid = value; if (value) this.eventDeckShapesInvalidated?.Invoke(); } }




        public double IdealSectionHeight
        {
            get
            {
                double _rVal = ManholeID;
                if (_rVal <= 0) return 24;
                if (SpliceStyle == uppSpliceStyles.Tabs) _rVal -= 1.992 + 1.1050;
                if (SpliceStyle == uppSpliceStyles.Tabs) _rVal -= 1.1050;
                _rVal -= 0.5;
                return _rVal;
            }
        }

        public bool Reading { get; private set; }

        public uppProjectFamilies ProjectFamily { get; private set; }

        public uppMDDesigns MDDesignFamily { get; private set; }

        string TrayName { get; set; }

        public int MaxPanelIndex
        {
            get
            {
                if (Count == 0) return 0;
                int _rVal = 0;
                foreach (var item in this) if (item.PanelIndex > _rVal) _rVal = item.PanelIndex;
                return _rVal;
            }
        }

        public bool MultiPanel => Divider == null ? false : Divider.DividerType == uppTrayDividerTypes.Beam && Divider.Offset != 0;

        public DividerInfo Divider{ get ; set; }
        #endregion Properties

        #region Methods

        /// <summary>
        /// returns the parent panel sub-shape from the members whose panel index matchs the passed panel index
        /// </summary>
        /// <param name="aPanelIndex"></param>
        /// <param name="bGetClones"></param>
        /// <returns></returns>
        public uopPanelSectionShapes PanelShapes(int aPanelIndex, bool bGetClones = false)
        {
            uopPanelSectionShapes _rVal = new uopPanelSectionShapes(TrayAssembly, _DowncomerData);
            foreach (var item in this)
            {

                if (item.PanelIndex == aPanelIndex)
                {
                    uopPanelSectionShape panelshape = item.ParentShape;
                    if (panelshape == null) continue;
                    if (_rVal.FindIndex(x => uopShape.Compare(x, panelshape)) == -1)
                    {
                        _rVal.Add(panelshape, bGetClones);
                    }

                }
            }


            return _rVal;

        }



        /// <summary>
        /// returns the members whose panel index matchs the passed panel index
        /// </summary>
        /// <param name="aPanelIndex"></param>
        /// <param name="bGetClones"></param>
        /// <returns></returns>
        public uopSectionShapes PanelSectionShapes(int aPanelIndex, bool bGetClones = false)
        => new uopSectionShapes(FindAll(x => x.PanelIndex == aPanelIndex), !bGetClones, TrayAssembly);

   

        public uopShapes GetSubShapes(uppSubShapeTypes aType, uopTrayAssembly aAssy = null, int? aPanelIndex = null, int? aPanelSectionIndex = null, int? aSectionIndex = null)
        {
            uopShapes _rVal = new uopShapes(aType.Description());
            foreach (var item in this)
            {
                if (aPanelIndex.HasValue && item.PanelIndex != aPanelIndex) continue;
                if (aPanelSectionIndex.HasValue && item.PanelSectionIndex != aPanelSectionIndex) continue;
                if (aSectionIndex.HasValue && item.SectionIndex != aSectionIndex) continue;
                switch (aType)
                {
                    case uppSubShapeTypes.BlockedAreas:
                        {
                            _rVal.Append(item.GetSubShapes(aType));
                            break;
                        }
                }
            }
           

            return _rVal;
        }


        public mdTrayAssembly GetMDTrayAssembly(mdTrayAssembly aAssy = null)
        {
            if (aAssy != null) return aAssy;
            uopTrayAssembly assy = TrayAssembly;

            return assy == null ? null : assy.ProjectFamily == uppProjectFamilies.uopFamMD ? (mdTrayAssembly)assy : null;

        }


        /// <summary>
        /// returns the height of the current manways
        /// </summary>
        public double GetManwayHt()
        {
            double _rVal = 0;
            List<uopSectionShape> bCol = FindAll(x => x.IsManway);
            foreach (var item in bCol) if (item.Height > _rVal) _rVal = item.Height;
            return _rVal;
        }


        public void Copy(uopSectionShapes aShapes, bool bDontCloneMembers = false, bool bDontCopyMembers = false)
        {
            if (aShapes == null) return;

            uopTrayAssembly assy = aShapes.TrayAssembly;
            _Invalid = aShapes._Invalid;
            if (assy == null)
            {
                _ManwayHeight = aShapes._ManwayHeight;
                _SpliceStyle = aShapes._SpliceStyle;
                _JoggleAngleHeight = aShapes._JoggleAngleHeight;
                ProjectFamily = aShapes.ProjectFamily;
                MDDesignFamily = aShapes.MDDesignFamily;
                TrayName = aShapes.TrayName;
                ShelfWidth = aShapes.ShelfWidth;
                SpliceAngleLength = aShapes.SpliceAngleLength;
                SpliceBoltCount = aShapes.SpliceBoltCount;
                
                RingClipSpacing = aShapes.RingClipSpacing;
                MoonRingClipSpacing = aShapes.MoonRingClipSpacing;
                Divider  = DividerInfo.CloneCopy( aShapes.Divider);
                HasBubblePromoters  = aShapes.HasBubblePromoters;
            }
            else
            {
                TrayAssembly = assy;
            }
            if(aShapes._DowncomerData != null) _DowncomerData = new DowncomerDataSet(aShapes._DowncomerData);
            Clear();
            if (bDontCopyMembers) return;
            foreach (var aShape in aShapes)
                Add(aShape, !bDontCloneMembers);
        }

        public List<uopDeckSplice> GetSplices( bool bGetTops = true, bool bGetBottoms = true, bool bGetClones = false, int? aPanelIndex = null) => uopSectionShapes.GetSectionSplices(this, bGetTops, bGetBottoms, bGetClones, aPanelIndex);
        public new void Add(uopSectionShape aShape)
        {
            if (aShape == null) return;
            if (base.IndexOf(aShape) >= 0) return;
            SetMemberInfo(aShape);
            base.Add(aShape);

        }
        public void Add(uopSectionShape aShape, bool bAddClone)
        {
            if (aShape == null) return;
            Add(bAddClone ? new uopSectionShape(aShape) : aShape);

        }

        public uopSectionShapes Clone() => new uopSectionShapes(this);

        object ICloneable.Clone() => (object)new uopSectionShapes(this);
        public List<int> PanelIDs(List<uopSectionShape> aSubset = null)
        {
            int cnt = (aSubset == null) ? Count : aSubset.Count;
            List<int> _rVal = new List<int>();
            if (cnt <= 0) return _rVal;

            uopSectionShape aMem;
            int PID;
            try
            {
                for (int i = 1; i <= cnt; i++)
                {
                    aMem = (aSubset == null) ? Item(i) : aSubset[i - 1];
                    PID = aMem.PanelIndex;
                    if (PID > 0)
                    {
                        if (_rVal.IndexOf(PID) < 0) _rVal.Add(PID);
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

        /// <summary>
        ///returns the item from the collection at the requested index ! Base 1 !
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
        /// 
        public virtual uopSectionShape Item(int aIndex)
        {
            if (aIndex < 1 || aIndex > Count)
            {
                throw new IndexOutOfRangeException();
            }
            SetMemberInfo(base[aIndex - 1]);

            return base[aIndex - 1];
        }


        private void SetMemberInfo(uopSectionShape aMember, int? aIndex = null)
        {
            if (aMember != null)
            {
                if (aIndex.HasValue) aMember.SectionIndex = aIndex.Value;
                aMember.FlangeHt = JoggleAngleHeight;
                aMember.SpliceStyle = SpliceStyle;
                aMember.ProjectFamily = ProjectFamily;
                aMember.MDDesignFamily = MDDesignFamily;
                aMember.ShellRadius = ShellRadius;
                aMember.RingRadius = RingRadius;
                aMember.DeckRadius = DeckRadius;
                aMember.ShelfWidth = ShelfWidth;
                aMember.SpliceAngleLength = SpliceAngleLength;
                aMember.SpliceBoltCount = SpliceBoltCount;
                aMember.RingClipSpacing = aMember.IsHalfMoon ? MoonRingClipSpacing : RingClipSpacing;
                aMember.Divider = DividerInfo.CloneCopy(Divider);
                aMember.SuppressBubblePromoters = !HasBubblePromoters;
            }
        }
        public List<uopRingClipSegment> RingClipSegments(int? aPanelIndex = null)
        {
            List<uopRingClipSegment> _rVal = new List<uopRingClipSegment>();
            foreach (var member in this)
            {
                if (aPanelIndex.HasValue && member.PanelIndex != aPanelIndex.Value) continue;
                _rVal.AddRange(member.RingClipSegments(out _));
            }
            return _rVal;
        }
        public uopVectors BPSites(bool bGetClones = true)
        {
            uopVectors _rVal = uopVectors.Zero;
            foreach (var item in this) _rVal.Append(item.BPSites, bGetClones, item.Handle);
            return _rVal;
        }

        public void Populate(IEnumerable<uopSectionShape> aShapes, bool bCloneMembers = false)
        {
            Clear();
            if (aShapes == null) return;
            foreach (var aShape in aShapes) Add(bCloneMembers ? new uopSectionShape(aShape) : aShape);
        }

        public uopSectionShapes SubSet(Predicate<uopSectionShape> match, bool bGetClones = false)
        {
            uopSectionShapes _rVal = new uopSectionShapes(this, bDontCloneMembers: true, TrayAssembly, bReturnEmpty: true);
            if (match == null) return _rVal;
            _rVal.Populate(FindAll(match), bGetClones);

            return _rVal;
        }

        public List<uopDeckSplice> GetSplices(bool bGetClones = false, int? aPanelIndex = null, uppSides? aSide = null, bool? bSupportsManway = null)
         => GetSectionSplices(this,bGetClones,aPanelIndex,aSide, bSupportsManway);

        internal void PrintToConsole(int? aPanelIndex = null) => uopSectionShapes.PrintToShapesConsole(this, aPanelIndex);

        public List<uopSectionShape> UniqueMDSections(mdTrayAssembly aAssy,  int? aPanelIndex = null, List<uopSectionShape> aSearchList = null)
        {
            aAssy ??= GetMDTrayAssembly();
            aSearchList ??= this;
            List<uopSectionShape> _rVal = uopSectionShapes.GetUniqueMDShapes(aAssy, aSearchList, aPanelIndex); // new List<uopSectionShape>();

            return _rVal;

        }

        public uopShapes BlockedAreas(uopTrayAssembly aAssy = null)
        {
            aAssy ??= TrayAssembly;

            uopShapes _rVal = new uopShapes("BLOCKED AREAS");
            foreach(var mem in this)
            {
                if (aAssy != null && aAssy.ProjectFamily == uppProjectFamilies.uopFamMD)
                {
                    _rVal.AddRange(uopSectionShapes.GenMDBlockedAreas(mem, (mdTrayAssembly)aAssy));
                }
               
            }



            return _rVal;
        }

        #endregion Methods

        #region Shared Methods
        public static List<uopDeckSplice> GetSectionSplices(List<uopSectionShape> aSectionShapes, bool bGetClones = false, int? aPanelIndex = null, uppSides? aSide = null, bool? bSupportsManway = null)
        {
            List<uopDeckSplice> _rVal = new List<uopDeckSplice>();

            if (aSectionShapes == null || aSectionShapes.Count == 0) return _rVal;

            List<uopSectionShape> srch = aSectionShapes;
            if (aPanelIndex.HasValue) srch = srch.FindAll(x => x.PanelIndex == aPanelIndex.Value);
           
            uppSides side = aSide.HasValue ? aSide.Value : uppSides.Undefined;
            
            if (side != uppSides.Undefined)
            {
                if (side == uppSides.Right) side = uppSides.Top;
                if (side == uppSides.Left) side = uppSides.Bottom;

            }

            foreach (var item in srch)
            {
                item.GetSplices(out uopDeckSplice top, out uopDeckSplice bot);

                if (side == uppSides.Top) bot = null;
                if (side == uppSides.Bottom) top = null;
                
                if (top != null)
                {
                    if (!bSupportsManway.HasValue ||( bSupportsManway.Value == top.SupportsManway)) 
                    {
                        if (_rVal.IndexOf(top) == -1) _rVal.Add(top);
                    }
                        
                }
                if (bot != null)
                {
                    if (!bSupportsManway.HasValue || (bSupportsManway.Value == bot.SupportsManway)) 
                    {
                        if (_rVal.IndexOf(bot) == -1) _rVal.Add(bot);
                    }
                }

            }


            return _rVal;
        }

        public static List<uopDeckSplice> GetSectionSplices(List<mdDeckSection> aSections, bool bGetClones = false, int? aPanelIndex = null, uppSides? aSide = null, bool? bSupportsManway = null)
        {
            List<uopDeckSplice> _rVal = new List<uopDeckSplice>();

            if (aSections == null || aSections.Count == 0) return _rVal;

            List<mdDeckSection> srch = aSections;
            if (aPanelIndex.HasValue) srch = srch.FindAll(x => x.PanelIndex == aPanelIndex.Value);

            uppSides side = aSide.HasValue ? aSide.Value : uppSides.Undefined;

            if (side != uppSides.Undefined)
            {
                if (side == uppSides.Right) side = uppSides.Top;
                if (side == uppSides.Left) side = uppSides.Bottom;

            }

            foreach (var item in srch)
            {
                item.GetSplices(out uopDeckSplice top, out uopDeckSplice bot);

                if (side == uppSides.Top) bot = null;
                if (side == uppSides.Bottom) top = null;

                if (top != null)
                {
                    if (!bSupportsManway.HasValue || (bSupportsManway.Value == top.SupportsManway))
                    {
                        if (_rVal.IndexOf(top) == -1) _rVal.Add(top);
                    }

                }
                if (bot != null)
                {
                    if (!bSupportsManway.HasValue || (bSupportsManway.Value == bot.SupportsManway))
                    {
                        if (_rVal.IndexOf(bot) == -1) _rVal.Add(bot);
                    }
                }

            }


            return _rVal;
        }

        public static List<List<uopSectionShape>> GetPanelSections(List<uopSectionShape> aSectionShapes, bool bGetClones = false, bool? bMarkValue = null)
        {
            List<List<uopSectionShape>> _rVal = new List<List<uopSectionShape>>();
            if (aSectionShapes == null) return _rVal;

            foreach (var section in aSectionShapes)
            {
                if (section == null) continue;
                int p = section.PanelIndex;

                while (p > _rVal.Count - 1) _rVal.Add(new List<uopSectionShape>());

                List<uopSectionShape> sublist = _rVal[p];
                uopSectionShape rsection = bGetClones ? new uopSectionShape(section) : section;
                if (bMarkValue.HasValue) rsection.Mark = bMarkValue.Value;

                sublist.Add(rsection);

            }
            return _rVal;
        }

        /// <summary>
        /// returns the sections in the passed list that are relative to the passed section (above or below)
        /// </summary>
        /// <param name="aSectionShape">the section to get the relatives of</param>
        /// <param name="aSectionShapes">the list to get the relatives from</param>
        /// <param name="bAbove">flag to return the relatives above or below</param>
        /// <param name="bStopAtManway">flag to breaak the search if a manway is encounter</param>
        /// <param name="bReturnManway">flag to return the manway if one is encounter and we are stopping at manways</param>
        /// <param name="bGetClones">flag to return clones of the returned relatives</param>
        /// <returns></returns>
        public static List<uopSectionShape> GetRelativePanelSections(uopSectionShape aSectionShape, List<uopSectionShape> aSectionShapes, bool bAbove, bool bStopAtManway, bool bReturnManway, bool bGetClones = false)
        {
            List<uopSectionShape> _rVal = new List<uopSectionShape>();
            if (aSectionShape == null || aSectionShapes == null) return _rVal;
            if (bAbove)
            {

                aSectionShape.RectifySplices(out uopDeckSplice top, out uopDeckSplice bot);
                uopDeckSplice test = bAbove ? top : bot;
                if (test == null) return _rVal;
                uopSectionShape rel = null;
                while (rel == null)
                {
                    rel = test == null ? null : bAbove ? aSectionShapes.Find(x => uopDeckSplice.CompareSplices(x.BottomSplice, test)) : aSectionShapes.Find(x => uopDeckSplice.CompareSplices(x.TopSplice, test));
                    if (rel == null) break;
                    rel.RectifySplices(out top, out bot);
                    test = bAbove ? top : bot;
                    if (bStopAtManway && rel.IsManway)
                    {
                        if (bReturnManway) _rVal.Add(!bGetClones ? rel : new uopSectionShape(rel));
                        break;
                    }
                    else
                    {
                        _rVal.Add(!bGetClones ? rel : new uopSectionShape(rel));
                    }

                    rel = null;
                }


            }
            else
            {

            }

            return _rVal;
        }


        public static void SetMDSectionInstances(uopSectionShapes aSectionShapes, mdTrayAssembly aAssy = null,  bool bVerbose = false)
        {
            if (aSectionShapes == null) return;
            aAssy ??= aSectionShapes.GetMDTrayAssembly();
            if (aAssy == null) return;
            uppMDDesigns family = aSectionShapes.MDDesignFamily;
            if (family.IsDividedWallDesignFamily()) return;

            if (bVerbose) aAssy.RaiseStatusChangeEvent($"Setting {aAssy.TrayName()} Section Instances");

            DowncomerDataSet dcdata = aSectionShapes.DowncomerData;
            if(dcdata == null && aAssy != null)
            {
                aSectionShapes.DowncomerData = new DowncomerDataSet(aAssy.DowncomerData);
                dcdata = aSectionShapes.DowncomerData;
            }
            bool standard = family.IsStandardDesignFamily();
            int panelcount = dcdata.PanelCount;

            bool specialcase = standard && dcdata.OddDowncomers && dcdata.DowncomerCount > 1 ;
           int lastdc = dcdata.FindAll(x => !x.IsVirtual).Count();

            foreach (var section in aSectionShapes)
            {

                double dx = -2 * section.X;
                double dy = -2 * section.Y;
                double rot = 180;

                section.Instances.Clear();
                if (family.IsDividedWallDesignFamily()) continue;
                if (standard)
                {
                    if (!section.IsHalfMoon)
                    {
                        if (Math.Round(section.PanelX, 2) == 0.0) continue;

                        if (specialcase)
                        {
                            if (section.LeftDowncomerIndex == lastdc || section.RightDowncomerIndex == lastdc)
                                continue;
                        }

                        if (section.IsSymmetric && Math.Round(section.Y ,3) ==0) rot = 0;
                    }
                    
                    section.Instances.Add(dx, dy, aRotation: rot, bInverted: false, bLeftHanded: false, aPartIndex: aSectionShapes.IndexOf(section) + 1, aRow: section.PanelIndex, aCol: uopUtils.OpposingIndex(section.PanelIndex, panelcount), bVirtual: true);

                    
                }
                else if (family.IsBeamDesignFamily() )
                {
                   if(Math.Round(dx,2) !=0 || Math.Round(dy, 2) != 0)  section.Instances.Add(dx, dy, aRotation: rot, bInverted: false, bLeftHanded: false, aPartIndex: aSectionShapes.IndexOf(section) + 1, aRow: section.PanelIndex, aCol: uopUtils.OpposingIndex(section.PanelIndex, panelcount), bVirtual: true);
                }
            }

        }

        public static void SetMDSectionInstances(uopSectionShape aSection, mdTrayAssembly aAssy = null)
        {
            if (aSection == null) return;
            aAssy ??= aSection.MDTrayAssembly;
            if (aAssy == null) return;
            uppMDDesigns family = aAssy.DesignFamily;
            if (family.IsDividedWallDesignFamily()) return;

            DowncomerDataSet dcdata = aAssy.DowncomerData;
      
            bool standard = family.IsStandardDesignFamily();
            int panelcount = dcdata.PanelCount;

            bool specialcase = standard && dcdata.OddDowncomers && dcdata.DowncomerCount > 1;
            int lastdc = dcdata.FindAll(x => !x.IsVirtual).Count();
            double dx = -2 * aSection.X;
            double dy = -2 * aSection.Y;
            double rot = 180;

            aSection.Instances.Clear();
            if (family.IsDividedWallDesignFamily()) return;
            if (standard)
            {
                if (!aSection.IsHalfMoon)
                {
                    if (Math.Round(aSection.PanelX, 2) == 0.0) return;

                    if (specialcase)
                    {
                        if (aSection.LeftDowncomerIndex == lastdc || aSection.RightDowncomerIndex == lastdc)
                            return;
                    }

                    if (aSection.IsSymmetric && Math.Round(aSection.Y, 3) == 0) rot = 0;
                }

                aSection.Instances.Add(dx, dy, aRotation: rot, bInverted: false, bLeftHanded: false, aRow: aSection.PanelIndex, aCol: uopUtils.OpposingIndex(aSection.PanelIndex, panelcount), bVirtual: true);


            }
            else if (family.IsBeamDesignFamily())
            {
                if (Math.Round(dx, 2) != 0 || Math.Round(dy, 2) != 0) aSection.Instances.Add(dx, dy, aRotation: rot, bInverted: false, bLeftHanded: false,  aRow: aSection.PanelIndex, aCol: uopUtils.OpposingIndex(aSection.PanelIndex, panelcount), bVirtual: true);
            }
    

        }
        /// <summary>
        /// sets the section shapes splice properties and MFTags
        /// </summary>
        /// <param name="aSectionShapes">the subject sections hapes</param>
        /// <param name="aAssy">the parent tray assembly</param>
        public static void SetMDSpliceProperties(uopSectionShapes aSectionShapes, mdTrayAssembly aAssy = null, bool bVerbose= false)
        {
            if (aSectionShapes == null) return;
            aAssy ??= aSectionShapes.GetMDTrayAssembly();
            if (aAssy == null) return;

            if(bVerbose) aAssy.RaiseStatusChangeEvent($"Setting {aAssy.TrayName()} Splice Properties");
            uppMDDesigns family = aAssy.DesignFamily;
            bool standard = family.IsStandardDesignFamily();
         
            uppSpliceStyles style = aSectionShapes.SpliceStyle;
            List<List<uopSectionShape>> panelsections = uopSectionShapes.GetPanelSections(aSectionShapes, false);
            uopDeckSplice top = null; uopDeckSplice bot = null;
            for (int p = 1; p < panelsections.Count; p++)
            {
                List<uopSectionShape> psections = panelsections[p];

                //find the manways
                List<uopSectionShape> manways = psections.FindAll(x => x.IsManway);
                if (manways.Count <= 0) //no manways
                {

                    foreach (uopSectionShape section in psections)
                    {
                        section.MechanicalBounds = null;
                        section.RectifySplices(out top, out bot, bMark: true);
                        

                        if (standard)
                        {
                            if (p != 1)
                            {
                                if (section.Y >= 0)
                                {
                                    if (top != null) top.TabDirection = standard ? dxxOrthoDirections.Down : dxxOrthoDirections.Up;
                                    if (bot != null) bot.TabDirection = standard ? dxxOrthoDirections.Down : dxxOrthoDirections.Up;
                                }
                                else
                                {
                                    if (top != null) top.TabDirection = standard ? dxxOrthoDirections.Up : dxxOrthoDirections.Up;
                                    if (bot != null) bot.TabDirection = standard ? dxxOrthoDirections.Up : dxxOrthoDirections.Up;
                                }
                            }
                            else
                            {
                                if (top != null) top.TabDirection = dxxOrthoDirections.Up;
                                if (bot != null) bot.TabDirection = dxxOrthoDirections.Up;
                            }
                        }
                        else
                        {
                            if (top != null) top.TabDirection = dxxOrthoDirections.Up;
                            if (bot != null) bot.TabDirection = dxxOrthoDirections.Up;

                        }

                    }
                    if (psections.Count > 2 && p != 1 && standard && style== uppSpliceStyles.Tabs)
                    {
                        // make sure there is a double female
                        uopSectionShape section = psections.Find(x => x.DoubleFemale);
                        if (section == null)
                        {
                            int idx = (int)Math.Round((double)(psections.Count / 2), 0) - 1;
                            section = psections[idx];
                            section.DoubleFemale = true;

                        }
                    }
                } //no manways
                else   // panels with manways within
                {


                    for (int m = 1; m <= manways.Count; m++)
                    {

                        uopSectionShape manway1 = manways[m - 1];

                        manway1.RectifySplices(out top, out bot, bMark: true);
                        top.TabDirection = dxxOrthoDirections.Up;
                        bot.TabDirection = dxxOrthoDirections.Down;
                       
                        uopSectionShape manway2 = null;
                        switch (style)
                        {
                            case uppSpliceStyles.Tabs:
                                {
                                    if (m < manways.Count)
                                    {
                                        bot.ManTag = "CENTER";
                                        manway2 = manways[m];
                                        if (manway2.IsManway)
                                        {
                                            manway2.Mark = true;
                                            manway2.RectifySplices(out top, out bot, bMark: true);
                                            top.TabDirection = dxxOrthoDirections.Up;
                                            bot.TabDirection = dxxOrthoDirections.Down;
                                            top.ManTag = "CENTER";
                                            m++;

                                        }
                                    }
                                    break;
                                }
                            case uppSpliceStyles.Angle:
                            case uppSpliceStyles.Joggle:
                                {
                                    break;
                                }

                        }

                        List<uopSectionShape> relatives = uopSectionShapes.GetRelativePanelSections(manway1, psections, bAbove: true, bStopAtManway: true, bReturnManway: false);
                        foreach (uopSectionShape above in relatives)
                        {
                            above.RectifySplices(out top, out bot, bMark: true);

                            switch (style)
                            {
                                case uppSpliceStyles.Tabs:
                                    {
                                        if (above.SupportsManway())
                                        {
                                            above.DoubleFemale = true;
                                        }
                                        else
                                        {
                                            if (standard)
                                            {
                                                if (top != null) top.TabDirection = dxxOrthoDirections.Down;
                                                if (bot != null) bot.TabDirection = dxxOrthoDirections.Down;
                                            }
                                            else
                                            {
                                                if (top != null) top.TabDirection = dxxOrthoDirections.Up;
                                                if (bot != null) bot.TabDirection = dxxOrthoDirections.Up;
                                            }

                                        }
                                        break;
                                    }
                                case uppSpliceStyles.Angle:
                                case uppSpliceStyles.Joggle:
                                    {
                                        break;
                                    }
                            }


                        }


                        if (m == manways.Count)
                        {
                            relatives = uopSectionShapes.GetRelativePanelSections(manway2 == null ? manway1 : manway2, psections, bAbove: false, bStopAtManway: true, bReturnManway: false);
                            foreach (uopSectionShape below in relatives)
                            {
                                below.RectifySplices(out top, out bot, bMark: true);

                                switch (style)
                                {
                                    case uppSpliceStyles.Tabs:
                                        {
                                            if (below.SupportsManway())
                                            {
                                                if (top != null) top.TabDirection = dxxOrthoDirections.Up;
                                                if (bot != null) bot.TabDirection = standard ? dxxOrthoDirections.Down : dxxOrthoDirections.Up;
                                            }
                                            else
                                            {
                                                if (standard)
                                                {
                                                    if (top != null) top.TabDirection = dxxOrthoDirections.Down;
                                                    if (bot != null) bot.TabDirection = dxxOrthoDirections.Down;
                                                }
                                                else
                                                {
                                                    if (top != null) top.TabDirection = dxxOrthoDirections.Up;
                                                    if (bot != null) bot.TabDirection = dxxOrthoDirections.Up;
                                                }

                                                if (standard && top != null && bot != null)
                                                {
                                                    if (top.Y > 0 && bot.Y < 0)
                                                    {
                                                        top.TabDirection = dxxOrthoDirections.Down;
                                                        bot.TabDirection = dxxOrthoDirections.Up;
                                                    }
                                                }
                                            }
                                            break;
                                        }
                                    case uppSpliceStyles.Angle:
                                    case uppSpliceStyles.Joggle:
                                        {

                                            break;
                                        }
                                }
                            }
                        }
                    }// manway loop
                }// manways in the panel
            }
        }

        public static void SetMDMechanicalProperties(uopSectionShapes aSectionShapes, mdTrayAssembly aAssy = null,bool bVerbose = false)
        {
            if (aSectionShapes == null) return;
            aAssy ??= aSectionShapes.GetMDTrayAssembly();
            if(aAssy == null) return;
            if(bVerbose) aAssy.RaiseStatusChangeEvent($"Setting {aAssy.TrayName()} Splice Mechanical Properties");

            DowncomerDataSet dcdata = aAssy.DowncomerData;
            double jht = aAssy.JoggleAngleHeight;
            aSectionShapes.JoggleAngleHeight = jht;
      
            uopShape trimArc = uopShape.Circle(null, aAssy.BoundingRadius);
            double tabspace = dcdata.TabSpacing(false,out _, out int tabcount);
            double boltspc = dcdata.DeckBoltSpacing( out int boltcnt ,dcdata.DeckSectionSpan);

            for ( int s = 1; s<= aSectionShapes.Count; s++)
            {
                uopSectionShape section = aSectionShapes.Item(s);
                SetMDMechanicalProperties(section, aAssy,bVerbose, jht);

                uopSectionShape above = aSectionShapes.Find(x => x.PanelIndex == section.PanelIndex && x.PanelSectionIndex == section.PanelSectionIndex && x.SectionIndex == section.SectionIndex - 1);
                uopSectionShape below = aSectionShapes.Find(x => x.PanelIndex == section.PanelIndex && x.PanelSectionIndex == section.PanelSectionIndex && x.SectionIndex == section.SectionIndex + 1);
                section.SectionAbove = above;
                section.SectionBelow = below;
            }

        }

        public static void SetMDMechanicalProperties(uopSectionShape aSectionShape, mdTrayAssembly aAssy = null, bool bVerbose = false,double? aJoggleAngleHeight = null)
        {
            if (aSectionShape == null) return;
            aAssy ??= aSectionShape.MDTrayAssembly;
            if (aAssy == null) return;
            if (bVerbose) aAssy.RaiseStatusChangeEvent($"Setting {aAssy.TrayName()} - Section {aSectionShape.Handle} Splice Mechanical Properties");

            DowncomerDataSet dcdata = aAssy.DowncomerData;
        
          
            uopShape trimArc = uopShape.Circle(null, aAssy.BoundingRadius);
            double tabspace = dcdata.TabSpacing(false, out _, out int tabcount);
            double boltspc = dcdata.DeckBoltSpacing(out int boltcnt, dcdata.DeckSectionSpan);
            uopSectionShape section = aSectionShape;

            if (aJoggleAngleHeight.HasValue) section.JoggleAngleHeight = aJoggleAngleHeight.Value;
            double jht = aSectionShape.JoggleAngleHeight;
            // to break the links between the section splices and the splices that were used to create the shape
            section.TopSplice = uopDeckSplice.CloneCopy(section.TopSplice);
            section.BottomSplice = uopDeckSplice.CloneCopy(section.BottomSplice);

            bool manway = section.IsManway;
          
            //get the splices and set the connection reference back to the section shape
            section.RectifySplices(out uopDeckSplice top, out uopDeckSplice bot, bMark: true, section);

            //set the side and the gender
            if (top != null)
            {
                top.Side = section.IsHalfMoon ? section.PanelX >= 0 ? uppSides.Right : uppSides.Left : uppSides.Top;

            }
            if (bot != null)
            {
                bot.Side = section.IsHalfMoon ? section.PanelX >= 0 ? uppSides.Left : uppSides.Right : uppSides.Bottom;
            }


            //================ VERTICAL SPLICES ========================
            if (section.IsHalfMoon)
            {
                if (top != null)
                {
                    top.TabDirection = section.X >= 0 ? dxxOrthoDirections.Left : dxxOrthoDirections.Right;
                    top.TabSpacing = dcdata.TabSpacing(top.Vertical, out _, out int vtabcount);
                    top.TabCount = vtabcount;
                    top.SpliceType = uppSpliceTypes.SpliceWithTabs;
                    top.Vertical = true;
                    uopFlangeLine flngln = new uopFlangeLine(top);
                    if (top.RequiresFlange)
                    {
                        top.Depth = jht + section.Thickness;
                        top.FlangeHt = jht;
                    }
                    if (top.RequiresTabs)
                        top.Depth = 2 * section.Thickness;

                    top.FlangeLine = flngln;
                }

                if (bot != null)
                {

                    bot.TabDirection = section.X >= 0 ? dxxOrthoDirections.Left : dxxOrthoDirections.Right;
                    bot.TabSpacing = dcdata.TabSpacing(bot.Vertical, out _, out int vtabcount);
                    bot.TabCount = vtabcount;
                    bot.SpliceType = uppSpliceTypes.SpliceWithTabs;
                    uopFlangeLine flngln = new uopFlangeLine(bot);

                    if (bot.RequiresFlange)
                    {
                        bot.Depth = jht + section.Thickness;
                        bot.FlangeHt = jht;
                    }
                    if (bot.RequiresTabs)
                    {
                        bot.Depth = 2 * section.Thickness;
                    }

                    bot.FlangeLine = flngln;
                }
            }
            else //================ HORIZONTAL SPLICES ========================
            {

                if (top != null)
                {

                    top.TabCount = tabcount;
                    top.SpliceBoltCount = boltcnt;
                    top.TabSpacing = tabspace;
                    top.BoltSpacing = boltspc;


                    uopFlangeLine flngln = new uopFlangeLine(top,false,section);
                    if (top.RequiresFlange)
                    {

                        top.Depth = jht + section.Thickness;
                        top.FlangeHt = jht;
                    }
                    if (top.RequiresTabs)
                    {
                        top.Depth = 2 * section.Thickness;
                    }

                    top.FlangeLine = flngln;

                }

                if (bot != null)
                {
                    bot.TabCount = tabcount;
                    bot.SpliceBoltCount = boltcnt;
                    bot.TabSpacing = tabspace;
                    bot.BoltSpacing = boltspc;
                    uopFlangeLine flngln = new uopFlangeLine(bot);

                    if (bot.RequiresFlange)
                    {

                        bot.Depth = jht + section.Thickness;
                        bot.FlangeHt = jht;

                    }

                    if (bot.RequiresTabs)
                    {
                        bot.Depth = 2 * section.Thickness;
                    }

                    bot.FlangeLine = flngln;
                }
            }


          

        }

        public static List<uopDeckSplice> GetSectionSplices(IEnumerable<uopSectionShape> aShapes, bool bGetTops = true, bool bGetBottoms = true, bool bGetClones = false, int? aPanelIndex = null, uppSpliceTypes? aType = null) 
        {
            List<uopDeckSplice> _rVal = new List<uopDeckSplice>();
            if(aShapes != null)  return _rVal;
            foreach(uopSectionShape shape in aShapes)
            {
                if (aPanelIndex.HasValue && shape.PanelIndex != aPanelIndex.Value) continue;
                shape.GetSplices(out uopDeckSplice top, out uopDeckSplice bot);
                if (bGetTops && top != null)
                {
                    if(!aType.HasValue ||(aType.HasValue && top.SpliceType == aType.Value))
                    _rVal.Add(bGetClones ? new uopDeckSplice(top) : top);
                }
                if (bGetBottoms && bot != null)
                {
                    if (!aType.HasValue || (aType.HasValue && bot.SpliceType == aType.Value))
                        _rVal.Add(bGetClones ? new uopDeckSplice(bot) : bot);
                }
            }
            return _rVal;
        }
       
        internal static void PrintToShapesConsole(IEnumerable<uopSectionShape> aShapes, int? aPanelIndex = null)
        {
     
            if (aShapes != null) return ;
            foreach (uopSectionShape shape in aShapes)
            {
                if (aPanelIndex.HasValue && shape.PanelIndex != aPanelIndex.Value) continue;
                Console.WriteLine(shape.ToString());
            }
            return;
        }

        public static uopSectionShape FindEqualSection(uopSectionShape aSection, List<uopSectionShape> aSections, mdTrayAssembly aAssy, out bool rInverseEqual, bool bShapeCompare = false, bool bIgnoreSlots = false)
        {
            uopSectionShape _rVal = null;
            rInverseEqual = false;
            if (aSection == null || aSections == null) return _rVal;
            

            List<uopSectionShape> srch = aSections.ToList();
            srch.Remove(aSection);
            if (aSection.IsManway)
            {
                _rVal = srch.Find(x => x.IsManway);
                if(_rVal != null && _rVal.SpliceStyle == uppSpliceStyles.Tabs)
                {
                    rInverseEqual = aSection.BottomSpliceType != _rVal.BottomSpliceType;
                }
                return _rVal;
            }
            if (aSection.LapsRing)
            {
                if (aSection.X > 0)
                    srch.RemoveAll(x => x.X > 0);
                else if (aSection.X < 0)
                    srch.RemoveAll(x => x.X < 0);
                if (aSection.Y > 0)
                    srch.RemoveAll(x => x.Y > 0);
                else if (aSection.Y < 0)
                    srch.RemoveAll(x => x.Y < 0);
            }

            //naroww the search
            srch.RemoveAll(x => aSection.IsHalfMoon != x.IsHalfMoon ||  aSection.LapsRing != x.LapsRing || aSection.LapsDivider != x.LapsDivider || Math.Round(aSection.Area, 3) != Math.Round(x.Area, 3) );
            foreach (uopSectionShape item in srch)
            {


                if (uopSectionShapes.SectionsMDCompare(aSection, item, aAssy, aAssy, false, out rInverseEqual, bShapeCompare, bIgnoreSlots))
                {
                    _rVal = item;
                    break;
                }
            }
           
            return _rVal;


        }
        public static bool SectionsMDCompare(uopSectionShape aSection,  uopSectionShape bSection, mdTrayAssembly aAssy, mdTrayAssembly bAssy,  bool bCompareMaterial, out bool rInverseEqual, bool bShapeCompare = false, bool bIgnoreSlots = false)
        {
            rInverseEqual = false;
            if (aSection == null || bSection == null) return false;
            if (aSection.MDDesignFamily != bSection.MDDesignFamily) return false;
            if (aSection.HasSplices != bSection.HasSplices) return false;
            if (aSection.RequiresSlotting != bSection.RequiresSlotting) return false;
            if (aSection.IsHalfMoon != bSection.IsHalfMoon) return false;
            if (aSection.IsManway != bSection.IsManway) return false;
            if (aSection.LapsRing != bSection.LapsRing) return false;
            if (aSection.LapsDivider != bSection.LapsDivider) return false;
            if (aSection.LapsRing && aSection.X != 0 && !bShapeCompare)
            {
                if ((aSection.X > 0 & bSection.X > 0) || (aSection.X < 0 & bSection.X < 0)) return false;
                if ((aSection.Y > 0 & bSection.Y > 0) || (aSection.Y < 0 & bSection.Y < 0)) return false;
            }

            aAssy ??= aSection.MDTrayAssembly;
            if (aAssy == null) return false;

            bAssy ??= bSection.MDTrayAssembly;
            if (bAssy == null) return false;


            if (aAssy != bAssy)
            {
                if (aAssy.DesignFamily != bAssy.DesignFamily) return false;
                if (bCompareMaterial)
                {
                    if (!aAssy.Deck.Material.IsEqual(bAssy.Deck.Material)) return false;
                }

                if (aAssy.SpliceStyle != bAssy.SpliceStyle) return false;
            }
      
                bool haspromoters = aAssy.DesignOptions.HasBubblePromoters;
            uppMDDesigns family = aAssy.DesignFamily;

            bool bECMD = family.IsEcmdDesignFamily();
            if (!bECMD) bIgnoreSlots = true;

            dxePolygon perim1 = aSection.UpdatePerimeter(false,aAssy);
            dxePolygon perim2 = bSection.UpdatePerimeter(false,bAssy);

            if (!bShapeCompare)
            {
                if (aSection.TruncatedFlangeLine(null)) return false;
                if (aSection.TruncatedFlangeLine(null)) return false;

            }

            bool SlotAndTab =  aSection.SpliceStyle == uppSpliceStyles.Tabs;
            aSection.GetSplices(out uopDeckSplice top1, out uopDeckSplice bot1);
            bSection.GetSplices(out uopDeckSplice top2, out uopDeckSplice bot2);

            string aTpslc = aSection.TopSpliceTypeName;
            string aBsplc = aSection.BottomSpliceTypeName;
            string bTpslc = bSection.TopSpliceTypeName;
            string bBsplc = bSection.BottomSpliceTypeName;
            bool bInv = false;
            if (SlotAndTab)
            {
                if (aSection.IsManway && aAssy.RangeGUID == bAssy.RangeGUID)
                {
                    rInverseEqual = string.Compare(aTpslc, bTpslc, true) != 0;
                    return true;
                }

                //compare the splice names. if the inverse combo is equal then we compare the shapes inverted
                if (!uopUtils.CompareStrings(aTpslc, aBsplc, bTpslc, bBsplc, out bInv)) return false;
            }

            //special case
            if (!aSection.SplicedOnTop && !aSection.SplicedOnBottom && !bSection.SplicedOnTop && !bSection.SplicedOnBottom && !aSection.IsHalfMoon)
            {
                if ((aSection.X < 0 && bSection.X > 0) || (aSection.X > 0 && bSection.X < 0)) bInv = true;
            }

            //if the gross boundary dimensions don't match the sections cannot be the same
            if (!aSection.BoundsV.CompareDimensions(bSection.BoundsV, aPrecis: 3)) return false;

            if (SlotAndTab)
            {
                List<uopFlangeLine> aflngLns = aSection.FlangeLines();
                List<uopFlangeLine> bflngLns = bSection.FlangeLines();
                if (aflngLns.Count != bflngLns.Count)
                    return false;
                foreach (uopFlangeLine fline in aflngLns)
                {

                    if (bflngLns.FindIndex((x) => Math.Round(x.Length, 3) == Math.Round(fline.Length, 3)) < 0)
                        return false;

                }

            }

           
            bool _rVal = false;
            uopVectors aPts = uopVectors.Zero;
            uopVectors bPts = uopVectors.Zero;
            uopArc circleA = null;
            uopArc circleB = null;

            if (aSection.IsManway)
            {
                if (bECMD)
                {
                    //just compare the slotting
                    mdSlotZone aSlotZone = aSection.SlotZone;
                    mdSlotZone bSlotZone = aSection.SlotZone;
                    if (aSlotZone != null) aPts = aSlotZone.GridPts.GetBySuppressed(false, true);
                    if (bSlotZone != null) bPts = bSlotZone.GridPts.GetBySuppressed(false, true);
                    _rVal = uopVectors.MatchPlanar(aPts, bPts, aPrecis: 2);

                }
            }
            else
            {
                //compare the perimter points 
                aPts = aSection.PerimeterPts( bIncludeBPSites: true, bIncludeSlotSites: false);
                bPts = bSection.PerimeterPts( bIncludeBPSites: true, bIncludeSlotSites: false);
                _rVal = uopVectors.MatchPlanar(aPts, bPts, out circleA, out circleB, aPrecis: 2);
                if (!_rVal) return false;
                if (bECMD && !bIgnoreSlots)
                {
                    mdSlotZone aZone = aSection.SlotZone;
                    mdSlotZone bZone = bSection.SlotZone;
                    if (aZone == null || bZone == null)
                        return false;

                    uopArcRecs aIslands = aZone.Islands;
                    uopArcRecs bIslands = bZone.Islands;


                    if (aZone.PitchType != bZone.PitchType || Math.Round(aZone.HPitch - bZone.HPitch, 3) != 0 || Math.Round(aZone.VPitch - bZone.VPitch, 3) != 0 || aIslands.Count != bIslands.Count)
                        return false;
                    //_rVal = uopVectors.MatchPlanar(aZone.GridPts, bZone.GridPts);

                    _rVal = uopVectors.MatchPlanar(aZone.GridPts.GetBySuppressed(false), bZone.GridPts.GetBySuppressed(false), 2);


                }
            }

            rInverseEqual = false;
            if (_rVal)
            {
                rInverseEqual = string.Compare(aTpslc, bTpslc, true) != 0;
                //special case
                if (!aSection.SplicedOnTop && !aSection.SplicedOnBottom && !bSection.SplicedOnTop && !bSection.SplicedOnBottom && !aSection.IsHalfMoon)
                {
                    if ((aSection.X < 0 && bSection.X > 0) || (aSection.X > 0 && bSection.X < 0)) rInverseEqual = true;
                }
                else if (aSection.GetSupportsManway(out bool ontop1) && bSection.GetSupportsManway(out bool ontop2))
                {
                    rInverseEqual = ontop1 != ontop2;
                }
            }

         
            //check to see if the bubble promoters match up;
            if( _rVal && !rInverseEqual  && haspromoters  && !aSection.IsManway)
            {
                uopVectors bp1 = aPts.GetByTag("BUBBLE PROMOTER POINTS");
                uopVectors bp2 = bPts.GetByTag("BUBBLE PROMOTER POINTS");
                bp1.Sort(dxxSortOrders.TopToBottom);
                bp2.Sort(dxxSortOrders.TopToBottom);


                for (int i =1; i<= bp1.Count; i++)
                {
                    var v1 = bp1[i -1];
                    var v2 = bp2[i - 1];
                    if(!circleA.Center.DirectionTo(v1).IsEqual(circleB.Center.DirectionTo(v2),3,false))
                    {
                        rInverseEqual = true;
                        break;
                    }
                }

            }
            // _rVal = UVECTORS.CompareSet(aPts, bPts, false, bInv, false, false);
            //if (!_rVal && !aSection.IsManway)
            //{
            //    if (aTpslc == aBsplc)
            //    {
            //        bInv = !bInv;
            //        _rVal = UVECTORS.CompareSet(aPts, bPts, false, bInv, false, false);
            //    }
            //}
            //rInverseEqual = bInv && _rVal;
            return _rVal;
        }

        public static List<uopSectionShape> GetUniqueMDShapes(mdTrayAssembly aAssy,  IEnumerable<uopSectionShape> aShapes, int? aPanelIndex = null)
        {
            List<uopSectionShape> _rVal = new List<uopSectionShape>();
            if(aShapes == null) return _rVal;
        List<uopSectionShape> shapelist = aShapes.ToList();
            if (shapelist.Count <= 0) return _rVal;

            uppMDDesigns family = aAssy != null ? aAssy.DesignFamily : shapelist[0].MDDesignFamily;


            foreach (var shape in shapelist)
            {
             
                bool ignoreslots = true; // !aAssy.DesignFamily.IsEcmdDesignFamily();
                uopSectionShape match = uopSectionShapes.FindEqualSection(shape, _rVal, aAssy, out bool invEq, bShapeCompare: true, bIgnoreSlots: ignoreslots);
                if(match == null) 
                {
                    match = new uopSectionShape(shape);
                    //match.Instances = new uopInstances(shape.Instances);
                    _rVal.Add(match);
                }
                else
                {
                    
                  match.Instances.Add(shape.X - match.X, shape.Y - match.Y, aRotation: invEq ? 180 : 0, bInverted: false, bLeftHanded: false, aPartIndex: shapelist.IndexOf(shape) + 1, bVirtual: false, aRow: shape.Row, aCol: shape.Col);

                    //add instances for the far side
                    uopInstances insts = shape.Instances;
                    foreach (var inst in insts)
                    {
                        uopVector u1 = new uopVector(shape.X + inst.DX, shape.Y + inst.DY);

                        if (family.IsStandardDesignFamily()) 
                        { 
                        
                        }
                            double rot = inst.Rotation;
                        if ((Math.Round(match.Y, 3) > match.ParentShape.Y && Math.Round(shape.Y, 3) < shape.ParentShape.Y) || (Math.Round(match.Y, 3) < match.ParentShape.Y && Math.Round(shape.Y, 3) > shape.ParentShape.Y))
                            rot = mzUtils.NormAng(rot + 180, false, true, true);

                        match.Instances.Add(u1.X - match.X, u1.Y - match.Y, aRotation: rot, bInverted: false, bLeftHanded: false, aPartIndex: shapelist.IndexOf(shape) + 1, bVirtual: inst.Virtual, aRow: inst.Row, aCol: inst.Col);
                        //                        match.Instances.Add(u1.X - match.X, u1.Y - match.Y, aRotation: mzUtils.NormAng(invEq ? 180 : 0 + inst.Rotation, false, true, true), bInverted: false, bLeftHanded: false, aPartIndex: shapelist.IndexOf(shape) + 1, bVirtual: inst.Virtual, aRow: shape.Row, aCol: shape.Col);

                    }

                    //match.Instances.Add(shape.X - match.X, shape.Y - match.Y, aRotation: invEq ? 180 : 0 , bInverted: false, bLeftHanded: false, aPartIndex: shapelist.IndexOf(shape) + 1, bVirtual: false, aRow: shape.Row, aCol: shape.Col);

                }
            }

          

            return _rVal;

        }

        /// <summary>
        /// executed internally to create the holes in the section
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aTag"></param>
        /// <param name="aFlag"></param>
        /// <param name="bTrayWide"></param>
        /// <returns></returns>
        internal static UHOLEARRAY GenMDHoles(uopSectionShape aSection,  mdTrayAssembly aAssy = null, string aTag = "", string aFlag = "", bool bTrayWide = false)
        {
            UHOLEARRAY _rVal =UHOLEARRAY.Null;
            if (aSection == null) return _rVal;
            aAssy ??= aSection.MDTrayAssembly;

            aSection.GetSplices(out uopDeckSplice top, out uopDeckSplice bot);

            dxePolygon perimiter = aSection.Perimeter;

            bool splice1IsAngle = top == null ? false : top.SpliceType == uppSpliceTypes.SpliceWithAngle || top.SpliceType == uppSpliceTypes.SpliceWithJoggle;

            bool splice2IsAngle = bot == null ? false : bot.SpliceType == uppSpliceTypes.SpliceWithAngle || bot.SpliceType == uppSpliceTypes.SpliceWithJoggle;

            bool hasAngleSplice = splice1IsAngle || splice2IsAngle;

            if (aAssy == null || (perimiter != null && !perimiter.HasArcSegments && !hasAngleSplice && !aSection.LapsDivider)) return _rVal;

            UHOLE aHole;
            UHOLES aHls;
            double lng;
            double thk = aSection.Thickness;
            if(thk <= 0 && aAssy != null)
            {
                thk = aAssy.Deck.Thickness;
                aSection.Thickness = thk;
            }
            UVECTOR u1;

            aTag = string.IsNullOrWhiteSpace(aTag) ? string.Empty : aTag.ToUpper().Trim();
            aFlag = string.IsNullOrWhiteSpace(aFlag) ? string.Empty : aFlag.ToUpper().Trim();

            //add the ring clip holes
            if (mzUtils.ListContains("RING CLIP", aTag, bReturnTrueForNullList: true))
            {
                if (aSection.LapsRing || aSection.LapsDivider)
                {
                    List<iSegment> rcsegs = aSection.RingClipSegments(out _).OfType<iSegment>().ToList();

                    lng = aSection.IsHalfMoon ?  aAssy.DesignOptions.MoonRingClipSpacing : aAssy.DesignOptions.MaxRingClipSpacing;

                    _rVal.Add(mdUtils.LayoutRingClipHoles(rcsegs, aSection.RingRadius, aSpacing: lng, bSavePointsToSegments: true, aDepth: thk));
                }
            }
            if (aSection.IsManway)
            {
                if (mzUtils.ListContains("MANWAY", aTag, bReturnTrueForNullList: true))
                {
                    _rVal.Append(mdUtils.GenerateManwayFastenerHoles(aSection, aAssy));
                }
            }
            //splice holes
            if (mzUtils.ListContains("SPLICE", aTag, bReturnTrueForNullList: true))
            {
                aHls = new UHOLES("SPLICE");
                aHole = aSection.SpliceHole;
                aHls.Member = aHole;

                if (bot != null)
                {
                    if (bot.SpliceType == uppSpliceTypes.SpliceWithAngle)
                    {
                        if (!aSection.IsManway)
                        {
                            ULINE  aLn = new ULINE(bot.FlangeLine);
                            lng = aLn.Length;

                            aLn.sp.Y = bot.BaseLine(aSection, out _).Y() + 0.5; //+ bot.GapValue(false);
                            aLn.ep.Y = aLn.sp.Y;
                            aHls = mdUtils.SpliceBoltHoles(aLn, mdUtils.SpliceBoltCount(lng, thk), aHole, aHole.Elevation, aHls);
                        }
                    }
                }
                if (top != null)
                {
                    if (top.SpliceType == uppSpliceTypes.SpliceWithAngle)
                    {
                        if (!aSection.IsManway)
                        {
                            ULINE aLn = new ULINE(top.FlangeLine);
                            lng = aLn.Length;

                            aLn.sp.Y = top.BaseLine(aSection, out _).Y() - 0.5; //+ bot.GapValue(false);
                            aLn.ep.Y = aLn.sp.Y;
                            aHls = mdUtils.SpliceBoltHoles(aLn, mdUtils.SpliceBoltCount(lng, thk), aHole, aHole.Elevation, aHls);
                        }

                    }
                }
                if (aHls.Centers.Count > 0) _rVal.Add(aHls, "SPLICE");

            }

            //bolt holes
            if (mzUtils.ListContains("BOLT", aTag, bReturnTrueForNullList: true))
            {
                aHls = new UHOLES("BOLT");
                aHole = aSection.BoltHole;
                aHls.Member = aHole;
                if (bot != null)
                {
                    if (bot.SpliceType == uppSpliceTypes.SpliceWithJoggle && !bot.Female)
                    {
                        ULINE aLn = new ULINE(bot.FlangeLine);
                        lng = aLn.Length;

                        int cnt = !bot.Vertical ? mdUtils.SpliceBoltCount(lng, thk) : (int)Math.Floor((lng - 2) / aAssy.DesignOptions.JoggleBoltSpacing) + 1;

                        aHls = mdUtils.SpliceBoltHoles(aLn, cnt, aHole, aHole.Elevation, aHls);
                    }
                }
                if (top != null)
                {
                    if (top.SpliceType == uppSpliceTypes.SpliceWithJoggle && !top.Female)
                    {
                        ULINE aLn = new ULINE(top.FlangeLine);
                        lng = aLn.Length;
                        int cnt = !top.Vertical ? mdUtils.SpliceBoltCount(lng, thk) : (int)Math.Floor((lng - 2) / aAssy.DesignOptions.JoggleBoltSpacing) + 1;
                        aHls = mdUtils.SpliceBoltHoles(aLn, cnt, aHole, aHole.Elevation, aHls);
                    }
                }
                if (aHls.Centers.Count > 0) _rVal.Add(aHls, "BOLT");

            }
            if (mzUtils.ListContains("LAP", aTag, bReturnTrueForNullList: true))
            {
                aHls = new UHOLES("LAP");
                aHole = aSection.LapHole;
                aHls.Member = aHole;

                if (bot != null)
                {
                    if (bot.SpliceType == uppSpliceTypes.SpliceWithJoggle && bot.Female)
                    {
                        ULINE aLn = new ULINE(bot.FlangeLine);
                        lng = aLn.Length;
                        int cnt = !bot.Vertical ? mdUtils.SpliceBoltCount(lng, thk) : (int)Math.Floor((lng - 2) / aAssy.DesignOptions.JoggleBoltSpacing) + 1;
                        aHls = mdUtils.SpliceBoltHoles(aLn, cnt, aHole, aHole.Elevation, aHls);
                    }
                }
                if (top != null)
                {
                    if (top.SpliceType == uppSpliceTypes.SpliceWithJoggle && top.Female)
                    {
                        ULINE aLn = new ULINE(top.FlangeLine);
                        lng = aLn.Length;
                        int cnt = !top.Vertical ? mdUtils.SpliceBoltCount(lng, thk) : (int)Math.Floor((lng - 2) / aAssy.DesignOptions.JoggleBoltSpacing) + 1;

                        aHls = mdUtils.SpliceBoltHoles(aLn, cnt, aHole, aHole.Elevation, aHls);
                    }
                }
                if (aHls.Centers.Count > 0) _rVal.Add(aHls, "LAP");

            }
            if (bTrayWide && aSection.OccuranceFactor > 1)
            {

                for (int i = 1; i <= _rVal.Count; i++)
                {

                    UHOLES mem = _rVal.Item(i);
                    //Instances.ApplyTo(mems)

                    int cnt = mem.Count;
                    for (int j = 1; j <= cnt; j++)
                    {
                        u1 = new UVECTOR(mem.Centers.Item(j));
                        u1.X = -u1.X;
                        u1.Y = -u1.Y;
                        mem.Centers.Add(u1);
                    }
                    _rVal.SetItem(i, mem);

                }
            }
            return _rVal;
        }

        /// <summary>
        /// reates the blocked areas for the indicated panel
        /// </summary>
        /// <param name="aPanelIndex"> the subject panel</param>
        /// <param name="aBaseShapes">the group of all the deck sahpes for the rtray assembly</param>
        /// <param name="aAssy">the subject tray assembly</param>
        /// <returns></returns>
        public static uopShapes GenMDBlockedAreas(int aPanelIndex, List<uopSectionShape> aBaseShapes, mdTrayAssembly aAssy )
        {
            uopShapes _rVal = new uopShapes("BLOCKED AREAS");
            try
            {
                if (aBaseShapes == null && aAssy == null) return _rVal;

                aBaseShapes ??= aAssy != null ? aAssy.DeckSections.BaseShapes( aAssy:aAssy) : null;
                if (aBaseShapes == null) return _rVal;
                List<uopSectionShape> panelshapes = aBaseShapes.FindAll(x => x.PanelIndex == aPanelIndex);

                if(panelshapes.Count == 1)
                {
                    var section = panelshapes[0];
                    _rVal.AddRange(uopSectionShapes.GenMDBlockedAreas(section, aAssy, false));
                    return _rVal;
                }

                List<int> skiptops = new List<int>();
                for(int i = 1; i <= panelshapes.Count; i++)
                {
                    var section = panelshapes[i - 1];
                    var below = panelshapes.Find(x => x.SectionIndex == section.SectionIndex + 1);
                    _rVal.AddRange(uopSectionShapes.GenMDBlockedAreas(section, aAssy, true));
                    section.GetSplices(out uopDeckSplice top, out uopDeckSplice bot);
                    if (skiptops.Contains(i)) top = null;

                    //if (section.Handle == "2,3")
                    //    Console.WriteLine("HERE");

                    if(bot != null)
                    {
                        if(below != null & below.TopSplice != null)
                        {
                            string side = "BOTTOM";
                            string splicehandle = bot.Handle;
                            string sectionhandle = section.Handle;
                            int occurs = section.OccuranceFactor;

                            
                            uopShapes botshapes = bot.BlockedAreas(section);
                            uopShapes topshapes = below.TopSplice.BlockedAreas(below);
                            if(bot.SpliceType == uppSpliceTypes.SpliceWithAngle || bot.SpliceType == uppSpliceTypes.SpliceManwayCenter)
                            {
                                URECTANGLE lims = new URECTANGLE(botshapes[0].BoundsV);
                                lims.ExpandTo(topshapes[0].BoundsV);
                                _rVal.Add(new uopShape(lims, aName: $"{side} SPLICE {splicehandle}", aTag: bot.SpliceType == uppSpliceTypes.SpliceWithAngle ?"SPLICE ANGLE" : "MANWAY SPLICE", aFlag: splicehandle, aHandle: sectionhandle, aOccurance: occurs, aPartIndex: aPanelIndex, aRow: bot.Index, aCol: aPanelIndex));
                                skiptops.Add(panelshapes.IndexOf(below) + 1);
                                bot = null;
                            }
                            else if (bot.SpliceType == uppSpliceTypes.SpliceWithTabs )
                            {
                                if(!bot.Female)  // it's male
                                {
                                    botshapes = bot.BlockedAreas(section,bAddTabs: true); //get the flang with the tabs shape
                                    uopShape botshape = botshapes[0];
                                    _rVal.Add(new uopShape(botshape, aName: $"{side} SPLICE {splicehandle}",  aFlag: splicehandle, aHandle: sectionhandle, aOccurance: occurs, aPartIndex: aPanelIndex, aRow: bot.Index, aCol: aPanelIndex));
                                    skiptops.Add(panelshapes.IndexOf(below) + 1);
                                    bot = null;
                                }
                                else
                                {
                                    if(below != null)
                                    {
                                        botshapes = below.TopSplice.BlockedAreas(section, bAddTabs: true); //get the flang with the tabs shape
                                        uopShape botshape = botshapes[0];
                                        _rVal.Add(new uopShape(botshape, aName: $"{side} SPLICE {splicehandle}", aFlag: splicehandle, aHandle: sectionhandle, aOccurance: occurs, aPartIndex: aPanelIndex, aRow: bot.Index, aCol: aPanelIndex));
                                        skiptops.Add(panelshapes.IndexOf(below) + 1);
                                        bot = null;
                                    }
                                }
                              
                            }

                        }
                    }

                    if(bot != null) _rVal.AddRange(bot.BlockedAreas(section));
                    if (top != null) _rVal.AddRange(top.BlockedAreas(section));
                }


                return _rVal;
            }
            catch (Exception)
            {
                return _rVal;
            }
        }

        /// <summary>
        /// creates the blocked areas for the passed deck section
        /// </summary>
        /// <param name="aSection">the subject section</param>
        /// <param name="aAssy">the parent tray assembly</param>
        /// <param name="bOmmitSplices">flag to only return the ring clip washer and other blocked regions and ommit the splice regions</param>
        /// <returns></returns>
        public static uopShapes GenMDBlockedAreas(uopSectionShape aSection, mdTrayAssembly aAssy, bool bOmmitSplices = false, bool bForSlotZone = false)
        {
            uopShapes _rVal = new uopShapes(!bForSlotZone ?  uppSubShapeTypes.BlockedAreas  : uppSubShapeTypes.SlotBlockedAreas);
            try
            {
                if (aSection == null) return _rVal;

                aAssy ??= aSection.MDTrayAssembly;
                if (aAssy == null) return _rVal;

                aSection.GetSplices(out uopDeckSplice tSplice, out uopDeckSplice bSplice);
                UHOLEARRAY aHls =  GenMDHoles( aSection,aAssy, "RING CLIP,MANWAY");
               string hndl = aSection.Handle;
                int occurs = aSection.OccuranceFactor;
                URECTANGLE lims = URECTANGLE.Null;
                int pid = aSection.PanelIndex;
                double diag = !bForSlotZone ? 0 : Math.Sqrt(Math.Pow(mdGlobals.SlotDieWidth, 2) + Math.Pow(mdGlobals.SlotDieHeight, 2)) / 2;

                //============= BUBBLE PROMOTERS ======================
                if(bForSlotZone && aAssy.DesignOptions.HasBubblePromoters)
                {
                    List<uopVector> bpsites = aSection.BPSites.FindAll(x => !x.Suppressed);
                    foreach( uopVector v in bpsites)
                    {
                        USHAPE blockedarea = USHAPE.Circle(v.X, v.Y, 2,"BUBBLE PROMOTER");
                        blockedarea.PartIndex = pid;
                        blockedarea.Handle = hndl;

                        _rVal.Add(new uopShape(blockedarea) { Row = aSection.PanelSectionIndex, Col = aSection.PanelIndex, OccuranceFactor = occurs });
                    }
                }

                //============= RING CLIPS ======================
                if (aHls.TryGet("RING CLIP", out UHOLES Hls) && Hls.Count > 0)
                {
                    double rad = mdGlobals.HoldDownWasherRadius + diag;

                    for (int i = 1; i <= Hls.Count; i++)
                    {
                        UHOLE aHl = Hls.Item(i);
                        USHAPE blockedarea = USHAPE.Circle(aHl.X, aHl.Y, rad, aHl.Tag, aHl.Flag);
                        blockedarea.PartIndex = pid;
                        blockedarea.Handle = hndl;

                        _rVal.Add(new uopShape(blockedarea) { Row = aSection.PanelSectionIndex, Col = aSection.PanelIndex, OccuranceFactor = occurs });

                    }
                }

                //================ MANWAY CLAMPS ===========================
                if (aSection.IsManway)
                {
                    dxfVector v1 = dxfVector.Zero;
                    if (aHls.TryGet("MANWAY", out Hls, true) && Hls.Count > 0)
                    {
                         double sltwd = mdGlobals.SlotDieWidth/ 2;
                        double sltht = mdGlobals.SlotDieHeight/2;
                        UHOLE aHl = Hls.Item(1);
                        double d1 = 0;
                        double d2 = 0;
                        double d3 = 0;
                        if (bForSlotZone)
                        {
                            d1 = aHl.Length / 2;
                            d2 = d1 + 2.15625 + sltwd;
                            d3 = d1 + 2.15625 + sltht;
                        }
                        double mclamplapH =  2.5512; //0.0937   //these numbers match the bolcked area spreadsheet calcs
                        double mclamplapV = 2.8437; //0.0937
                        for (int j = 1; j <= aHls.Count; j++)
                            {
                           
                                Hls = aHls.Item(j);
                                for (int i = 1; i <= Hls.Count; i++)
                                {
                                string tag = "MANWAY CLAMP";
                                aHl = Hls.Item(i);
                                
                                if (!bForSlotZone)
                                {
                                    if (aHl.Rotation == 0 || aHl.Rotation == 180)  // to dowcomer
                                    {
                                        tag += " TO DOWNCOMER";
                                        if (aHl.X < aSection.X)
                                        {
                                            lims.Left = aSection.Left + aSection.LeftDowncomerLap;
                                            lims.Right = lims.Left + mclamplapH;

                                        }
                                        else
                                        {
                                            lims.Right = aSection.Right - aSection.RightDowncomerLap;
                                            lims.Left = lims.Right - mclamplapH;

                                        }
                                        lims.Top = aHl.Y + 0.5;
                                        lims.Bottom = aHl.Y - 0.5;
                                    }
                                    else // to manway angle
                                    {
                                        tag += " TO MANWAY ANGLE";

                                        if (aHl.Y < aSection.Y)
                                        {
                                            lims.Bottom = bSplice.Y + 0.5 * mdGlobals.SpliceAngleWidth;
                                            lims.Top = lims.Bottom + mclamplapV;
                                        }
                                        else
                                        {
                                            lims.Top = tSplice.Y - 0.5 * mdGlobals.SpliceAngleWidth;
                                            lims.Bottom = lims.Top - mclamplapV;
                                        }
                                        lims.Left = aHl.X - 0.5;
                                        lims.Right = aHl.X + 0.5;
                                    }
                                }
                                else
                                {
                                    double f1 = 1;
                                    if (aHl.Rotation == 0 || aHl.Rotation == 180)  // to dowcomer
                                    {
                                        f1 = (aHl.X < aSection.X) ? 1 : -1;
                                        tag += " TO DOWNCOMER";


                                        lims.Left = aHl.X - (d1 * f1);
                                        lims.Right = aHl.X + (f1 * d2);
                                        lims.Top = aHl.Y + (0.4375 + sltht);
                                        lims.Bottom = aHl.Y - (0.4375 + sltht);

                                    }
                                    else // to manway angle
                                    {
                                        tag += " TO MANWAY ANGLE";
                                        f1 = (aHl.Y < aSection.Y) ? -1 : 1;
                                        lims.Left = aHl.X + (0.4375 + sltwd);
                                        lims.Right = aHl.X - (0.4375 + sltwd);
                                        lims.Top = aHl.Y + (f1 * d1);
                                        lims.Bottom = aHl.Y - (f1 * d3);
                                    }

                                }
                                    _rVal.Add(new uopShape(lims, aName: tag, aTag: tag, aFlag: string.Empty, aOccurance: occurs, aPartIndex: pid, aRow: aSection.PanelSectionIndex, aCol: aSection.PanelIndex));


                            }

                            //}
                        }
                    }
                }

                //================ SPLICES ===========================
                if (!bOmmitSplices)
                {
                        if (bSplice != null)
                        {
                            double twidthadder = 0;
                            double theightadder = 0;
                        //if ((bSplice.Female && bSplice.SpliceType == uppSpliceTypes.SpliceWithTabs) || !bForSlotZone)
                        //{

                        //    if (bForSlotZone)
                        //    {
                        //        twidthadder = !bSplice.Vertical ?  mdGlobals.SlotDieWidth :  mdGlobals.SlotDieHeight;
                        //        theightadder = !bSplice.Vertical ?  mdGlobals.SlotDieHeight  :  mdGlobals.SlotDieWidth;
                        //    }
                        //}

                        if (!bForSlotZone || (bForSlotZone && bSplice.SpliceType == uppSpliceTypes.SpliceWithTabs  && bSplice.Female))
                        {

                            _rVal.AddRange(bSplice.BlockedAreas(aSection, bInverted: true, aTabWidthAdder: twidthadder, aTabHeightAdder: theightadder));
                        }


                    }



                        if (tSplice != null)
                        {
                        double twidthadder = 0;
                        double theightadder = 0;

                        //if ((tSplice.Female && tSplice.SpliceType == uppSpliceTypes.SpliceWithTabs) || !bForSlotZone)
                        //{
                        //    if (bForSlotZone)
                        //    {
                        //        twidthadder = !tSplice.Vertical ? mdGlobals.SlotDieWidth : mdGlobals.SlotDieHeight ;
                        //        theightadder = !tSplice.Vertical ? mdGlobals.SlotDieHeight : mdGlobals.SlotDieWidth ;
                        //        twidthadder = mdGlobals.SlotDieWidth;
                        //        theightadder = mdGlobals.SlotDieHeight;
                        //    }
                        //}
                        if(!bForSlotZone || (bForSlotZone && tSplice.SpliceType == uppSpliceTypes.SpliceWithTabs && tSplice.Female))
                        {
                           
                            _rVal.AddRange(tSplice.BlockedAreas(aSection, bInverted: true, aTabWidthAdder: twidthadder, aTabHeightAdder: theightadder));
                        }
                        
                    }

                        

                   // }

                }



                return _rVal;
            }
            catch (Exception)
            {
                return _rVal;
            }
        }
        #endregion Shared Methods
    }
}

