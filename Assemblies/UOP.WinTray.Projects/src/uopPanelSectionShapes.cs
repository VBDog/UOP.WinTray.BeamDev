using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Parts;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;
using static System.Collections.Specialized.BitVector32;

namespace UOP.WinTray.Projects
{
    public class uopPanelSectionShapes : List<uopPanelSectionShape>, IEnumerable<uopPanelSectionShape>, ICloneable
    {

        #region Events
        public delegate void PanelShapesInValidatedHandler();
        public event PanelShapesInValidatedHandler eventPanelShapesInValidated;

        #endregion Events
        #region Constructors
        public uopPanelSectionShapes() => Init();

        public uopPanelSectionShapes(uopTrayAssembly aAssy, DowncomerDataSet aDataSet = null) => Init(aAssy: aAssy, aDataSet:aDataSet);
        public uopPanelSectionShapes(List<uopPanelSectionShape> aShapes = null, bool bDontCloneMembers = false, uopTrayAssembly aAssy = null, bool bReturnEmpty = false) => Init(aShapes, bDontCloneMembers,aAssy,bReturnEmpty); 

        
        private void Init(IEnumerable<uopShape> aShapes = null, bool bDontCloneMembers = false, uopTrayAssembly aAssy = null, bool bReturnEmpty = false, DowncomerDataSet aDataSet = null)
        {
            RingClipSpacing = mdGlobals.DefaultRingClipSpacing;
            MoonRingClipSpacing = mdGlobals.DefaultRingClipSpacing;
            Divider = null;
            _DowncomerData = DowncomerDataSet.CloneCopy(aDataSet);
            SlotType = uppFlowSlotTypes.FullC;
            TrayAssembly = aAssy;
            if (aDataSet != null) Divider = new DividerInfo(aDataSet.Divider);

            base.Clear();
            if (aShapes == null) return;
            if (aShapes.GetType() == typeof(uopPanelSectionShapes))
            {
                Copy((uopPanelSectionShapes)aShapes, bDontCloneMembers: bDontCloneMembers, bDontCopyMembers: bReturnEmpty);

            }
            else
            {
                if (!bReturnEmpty)
                {
                    foreach (var shape in aShapes)
                        Add(new uopPanelSectionShape(shape));
                }
      
            }

        }

        public virtual uopPanelSectionShapes PanelShapes(int aPanelIndex, bool bGetClones = false)  => SubSet(x => x.PanelIndex == aPanelIndex,bGetClones: bGetClones);

        #endregion Constructors

        #region Properties
        public double ShelfWidth { get; set; }
        public double DowncomerClearance{get; set;}
        public string RangeGUID { get; private set; }

        public double RingRadius { get; set; }

        public double DeckRadius { get; set; }

        public double ShellRadius { get; set; }

        

        public int MaxPanelIndex
        {
            get
            {
                if(Count == 0) return 0;
                int _rVal = 0;
                foreach(var item in this) if(item.PanelIndex > _rVal) _rVal = item.PanelIndex;
                return _rVal;
            }
        }

        private DowncomerDataSet _DowncomerData;
        public DowncomerDataSet DowncomerData
        {
            get
            {
                if(_DowncomerData != null) return _DowncomerData;
                mdTrayAssembly assy = GetMDTrayAssembly();
                return assy == null ? null : assy.DowncomerData;
            }
            set { _DowncomerData = value; }
        }

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
                    MDDesignFamily = assy.DesignFamily;
                    DowncomerClearance = assy.PanelClearance(true);
                    ShelfWidth = assy.Downcomer().ShelfWidth;
                    RingClipSpacing = assy.DesignOptions.MaxRingClipSpacing;
                    MoonRingClipSpacing = assy.DesignOptions.MoonRingClipSpacing;
                    SlotType = assy.Deck.SlotType;
                    if (_DowncomerData == null)  Divider =  assy.Divider;
                }

            }
        }

        private double _JoggleAngleHeight;
        public double JoggleAngleHeight
        {
            get => _JoggleAngleHeight;
            private set
            {
                _JoggleAngleHeight = value;
        
            }

        }

        public double ManholeID { get; set; }

     
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
        public bool Invalid { get => _Invalid || Count <= 0; set { if (_Invalid == value) return; _Invalid = value; if (value) this.eventPanelShapesInValidated?.Invoke(); } }


        public uppFlowSlotTypes SlotType { get; set; }

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

        public uppProjectFamilies ProjectFamily { get; private set; }

        public uppMDDesigns MDDesignFamily { get; private set; }

        string TrayName { get; set; }

        public double RingClipSpacing { get; set; }
        public double MoonRingClipSpacing { get; set; }

 
        public bool MultiPanel => Divider == null ? false : Divider.DividerType == uppTrayDividerTypes.Beam && Divider.Offset != 0;

        public DividerInfo Divider { get; set; }

        #endregion Properties

        #region Methods
        public mdTrayAssembly GetMDTrayAssembly(mdTrayAssembly aAssy = null)
        {
            if (aAssy != null) return aAssy;
            uopTrayAssembly assy = TrayAssembly;

            return assy == null ? null : assy.ProjectFamily == uppProjectFamilies.uopFamMD ? (mdTrayAssembly)assy : null;

        }

        public void Populate(IEnumerable<uopPanelSectionShape> aShapes, bool bCloneMembers = false)
        {
            Clear();
            if(aShapes == null) return;
            foreach(var aShape in aShapes) Add(bCloneMembers ? new uopPanelSectionShape(aShape) : aShape);
        }

        public uopPanelSectionShapes SubSet(Predicate<uopPanelSectionShape> match, bool bGetClones = false)
        {
            uopPanelSectionShapes _rVal = new uopPanelSectionShapes(this,bDontCloneMembers:true,TrayAssembly, bReturnEmpty:true);
            if(match == null) return _rVal;
            _rVal.Populate(FindAll(match), bGetClones);

            return _rVal;
        }

        public void Copy(uopPanelSectionShapes aShapes, bool bDontCloneMembers = false, bool bDontCopyMembers = false)
        {
            if (aShapes == null) return;

            uopTrayAssembly assy = aShapes.TrayAssembly;

            _Invalid = aShapes._Invalid;
            if (assy == null)
            {
                 _SpliceStyle = aShapes._SpliceStyle;
                _JoggleAngleHeight = aShapes._JoggleAngleHeight;
                ProjectFamily = aShapes.ProjectFamily;
                MDDesignFamily = aShapes.MDDesignFamily;
                TrayName = aShapes.TrayName;
                ShelfWidth = aShapes.ShelfWidth;
                DowncomerClearance = aShapes.DowncomerClearance;
                RingClipSpacing = aShapes.RingClipSpacing;
                MoonRingClipSpacing = aShapes.MoonRingClipSpacing;
                SlotType = aShapes.SlotType;
                Divider = DividerInfo.CloneCopy(aShapes.Divider);
            }
            else
            {
                TrayAssembly = assy;
            }
            if (aShapes._DowncomerData != null) 
                _DowncomerData = new DowncomerDataSet(aShapes._DowncomerData);

            Clear();
            if (bDontCopyMembers) return;
            foreach (var aShape in aShapes)
                Add(aShape, !bDontCloneMembers);
        }


        public new void Add(uopPanelSectionShape aShape)
        {
            if (aShape == null) return;
            if (base.IndexOf(aShape) >= 0) return;
            SetMemberInfo(aShape);
            base.Add(aShape);

        }
        public void Add(uopPanelSectionShape aShape, bool bAddClone)
        {
            if (aShape == null) return;
            Add(bAddClone ? new uopPanelSectionShape(aShape) : aShape);

        }

        public uopPanelSectionShapes Clone() => new uopPanelSectionShapes(this);

        object ICloneable.Clone() => (object)new uopPanelSectionShapes(this);
        public List<int> PanelIDs(List<uopPanelSectionShape> aSubset = null)
        {
            int cnt = (aSubset == null) ? Count : aSubset.Count;
            List<int> _rVal = new List<int>();
            if (cnt <= 0) return _rVal;

            uopPanelSectionShape aMem;
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
        public virtual uopPanelSectionShape Item(int aIndex)
        {
            if (aIndex < 1 || aIndex > Count)
            {
                throw new IndexOutOfRangeException();
            }
            SetMemberInfo(base[aIndex - 1]);

            return base[aIndex - 1];
        }


        private void SetMemberInfo(uopPanelSectionShape aMember, int? aIndex = null)
        {
            if (aMember != null)
            {
                if (aIndex.HasValue) aMember.SectionIndex = aIndex.Value;
         
                aMember.SpliceStyle = SpliceStyle;
                aMember.ProjectFamily = ProjectFamily;
                aMember.MDDesignFamily = MDDesignFamily;
                aMember.ShellRadius = ShellRadius;
                aMember.RingRadius = RingRadius;
                aMember.DeckRadius = DeckRadius;
                aMember.DowncomerClearance = DowncomerClearance;
                aMember.RingClipSpacing = aMember.IsHalfMoon ? MoonRingClipSpacing : RingClipSpacing;
                aMember.Divider = DividerInfo.CloneCopy(Divider);
            }
        }

        public uopVectors BPSites(bool bGetClones = true)
        {
            uopVectors _rVal = uopVectors.Zero;
            foreach (var item in this) _rVal.Append(item.BPSites, bGetClones, item.Handle);
            return _rVal;
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
        #endregion Methods

        #region Shared Methods

        public static void SetMDPanelInstances(uopPanelSectionShapes aPanelShapes, mdTrayAssembly aAssy = null, bool bVerbose = false)
        {
            if (aPanelShapes == null) return;
            aAssy ??= aPanelShapes.GetMDTrayAssembly();
            if (aAssy == null) return;
            uppMDDesigns family = aAssy.DesignFamily;
            if (family.IsDividedWallDesignFamily()) return;

            if (bVerbose) aAssy.RaiseStatusChangeEvent($"Setting {aAssy.TrayName()} Section Instances");
            bool standard = family.IsStandardDesignFamily();
            DowncomerDataSet dcdata = aPanelShapes.DowncomerData;
            DividerInfo divider = !standard ? dcdata.Divider : null ;
            bool multipanel = family.IsBeamDesignFamily() ? divider.BeamOffsetFactor != 0 : false;
            int panelcount = dcdata.PanelCount;

            bool specialcase = aAssy.ProjectType == uppProjectTypes.MDDraw && standard && aAssy.OddDowncomers && aAssy.Downcomer().Count > 1;
            int lastdc = dcdata.FindAll(x => !x.IsVirtual).Count();

            foreach (var section in aPanelShapes)
            {

                double dx = -2 * section.X;
                double dy = -2 * section.Y;
                double rot = 180;

                section.Instances.Clear();
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

                        //if (section.IsSymmetric && Math.Round(section.Y, 3) == 0) rot = 0;
                    }

                    if(Math.Round( dx,1) !=0) section.Instances.Add(dx, dy, aRotation: rot, bInverted: false, bLeftHanded: false, aPartIndex: aPanelShapes.IndexOf(section) + 1, aRow: section.PanelIndex, aCol: uopUtils.OpposingIndex(section.PanelIndex, panelcount), bVirtual: true);


                }
                else if (family.IsBeamDesignFamily())
                {
                   
                    if (!multipanel)
                    {
                        section.Instances.Add(dx, dy, aRotation: rot, bInverted: false, bLeftHanded: false, aPartIndex: aPanelShapes.IndexOf(section) + 1, aRow: section.PanelIndex, aCol: uopUtils.OpposingIndex(section.PanelIndex, panelcount), bVirtual: true);
                    }
                    else
                    {
                        if(section.Row != 2)
                        {
                            section.Instances.Add(dx, dy, aRotation: rot, bInverted: false, bLeftHanded: false, aPartIndex: aPanelShapes.IndexOf(section) + 1, aRow: section.PanelIndex, aCol: uopUtils.OpposingIndex(section.PanelIndex, panelcount), bVirtual: true);
                        }
                        else
                        {
                            if (Math.Round(dy, 2) != 0)
                                section.Instances.Add(dx, dy, aRotation: rot, bInverted: false, bLeftHanded: false, aPartIndex: aPanelShapes.IndexOf(section) + 1, aRow: section.PanelIndex, aCol: uopUtils.OpposingIndex(section.PanelIndex, panelcount), bVirtual: true);
          
                        }
                        
                    }
                      
                                        
                }
            }

        }

        public static List<List<uopPanelSectionShape>> GetPanelSections(List<uopPanelSectionShape> aSectionShapes, bool bGetClones = false, bool? bMarkValue = null)
        {
            List<List<uopPanelSectionShape>> _rVal = new List<List<uopPanelSectionShape>>();
            if (aSectionShapes == null) return _rVal;

            foreach (var section in aSectionShapes)
            {
                if (section == null) continue;
                int p = section.PanelIndex;

                while (p > _rVal.Count - 1) _rVal.Add(new List<uopPanelSectionShape>());

                List<uopPanelSectionShape> sublist = _rVal[p];
                uopPanelSectionShape rsection = bGetClones ? new uopPanelSectionShape(section) : section;
                if (bMarkValue.HasValue) rsection.Mark = bMarkValue.Value;

                sublist.Add(rsection);

            }
            return _rVal;
        }

        /// <summary>
            #endregion Shared Methods
    }
}

