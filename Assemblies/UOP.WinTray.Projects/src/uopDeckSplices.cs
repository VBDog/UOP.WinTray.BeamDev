using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UOP.DXFGraphics;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Parts;

using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;

namespace UOP.WinTray.Projects
{
    public class uopDeckSplices : List<uopDeckSplice>, IEnumerable<uopDeckSplice>, ICloneable
    {

        #region Events
        public delegate void DeckSplicesInvalidatedHandler();
        public event DeckSplicesInvalidatedHandler eventDeckSplicesInvalidated;

        #endregion Events

        #region Constructors

        public uopDeckSplices() => Init();

        public uopDeckSplices(uopTrayAssembly aAssy) => Init(aAssy: aAssy);
        public uopDeckSplices(uopTrayAssembly aAssy, IEnumerable<uopDeckSplice> aSplices) => Init(aSplices: aSplices, aAssy: aAssy);

        public uopDeckSplices(uopTrayAssembly aAssy, IEnumerable<uopDeckSplice> aSplices, bool bDontCloneMembers ) => Init(aSplices: aSplices, bDontCloneMembers: bDontCloneMembers, aAssy: aAssy);

        public uopDeckSplices(IEnumerable<uopDeckSplice> aSplices) =>  Init(aSplices:aSplices);
           
        private void Init(IEnumerable<uopDeckSplice> aSplices = null, bool bDontCloneMembers = false, uopTrayAssembly aAssy = null)
        {
            base.Clear();
            _AssyRef = null;
            _ManwayHeight = 0;
            ManholeID = 0;
            _JoggleAngleHeight = 0;
            ProjectFamily = uppProjectFamilies.Undefined;
            MDDesignFamily = uppMDDesigns.Undefined;
            RingRadius = 0;
            DeckRadius = 0;
            ShellRadius = 0;
            _FemaleFlange_SAndT = null;
            TrayAssembly = aAssy;
            if (aSplices == null) return;

            if (aSplices.GetType() == typeof(uopDeckSplices))
            {
                Copy((uopDeckSplices)aSplices, bDontCloneMembers);

            }
            else
            {
                foreach (var aSplice in aSplices)
                    Add(aSplice, !bDontCloneMembers);
            }



        }

        #endregion Constructors

        public string RangeGUID { get; private set; }
        #region Properties

        /// <summary>
        /// the coordinate of the points that are the centers of the manways
        /// </summary>
        public string ManwayCenters
        {
            get
            {
                ManwayCount(out uopVectors _rVal);
                return _rVal.Coords(5);
            }
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
                _JoggleAngleHeight = value.JoggleAngleHeight;
                ProjectFamily = value.ProjectFamily;
                TrayName = value.TrayName(true);
                RingRadius = value.RingRadius;
                DeckRadius = value.DeckRadius;
                ShellRadius = value.ShellRadius;
                DeckThickness = value.DeckMaterial.Thickness;
                if (value.ProjectFamily == uppProjectFamilies.uopFamMD)
                {
                    mdTrayAssembly assy = (mdTrayAssembly)value;
                    MDDesignFamily = assy.DesignFamily;
                }
            }
        }

        public mdTrayAssembly MDTrayAssembly { get { uopTrayAssembly assy = TrayAssembly; return assy == null ? null : assy.ProjectFamily == uppProjectFamilies.uopFamMD ? (mdTrayAssembly)assy : null; } }
        private double _JoggleAngleHeight;
        public double JoggleAngleHeight
        {
            get => _JoggleAngleHeight;
            private set
            {
                _JoggleAngleHeight = value;
                foreach (var item in this) if(item!=null) item.FlangeHt = value;
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
                    ChangeSpliceStyle(_SpliceStyle);
                }
            }
        }

        private bool _Invalid;
        public bool Invalid { get => _Invalid || Count <= 0; set { if (_Invalid == value) return; _Invalid = value; if (value) this.eventDeckSplicesInvalidated?.Invoke(); } }

        public double FastenerSpace { get; set; }

        public double RingRadius { get; set; }

        public double DeckRadius { get; set; }

        public double ShellRadius { get; set; }

        public double RingClearance => uopUtils.BoundingClearance(2 * ShellRadius);

        /// <summary>
        /// the height used to force the panel height during regeneration
        /// </summary>
        public double PanelHeight
        {
            get
            {
                double _rVal = 0;
                TVALUES aSpaces = new TVALUES("");
                TVALUES uSpaces = new TVALUES("");



                int idx = 0;
                int cnt = 0;
                int vcnt = 0;


                bool bSkip = false;

                List<int> pidxs = PanelIndexes();
                for (int i = 0; i < pidxs.Count; i++)
                {

                    int pid = pidxs[i];

                    if (pid != 1)
                    {
                        List<uopDeckSplice> aMems = GetByPanelIndex(pid);
                        for (int j = 1; j <= aMems.Count - 1; j++)
                        {
                            uopDeckSplice aMem = aMems[j - 1];
                            bSkip = aMem.ManwayHandle !=  string.Empty && string.Compare(aMem.ManTag, "BOTTOM", StringComparison.OrdinalIgnoreCase) != 0;
                            if (!bSkip)
                            {
                                uopDeckSplice bMem = aMems[j];
                                aSpaces.AddNumber(Math.Abs(aMem.Ordinate - bMem.Ordinate));
                            }
                        }
                    }
                }
                if (aSpaces.Count > 0)
                {
                    if (aSpaces.Count == 1)
                    { _rVal = aSpaces.Item(1); }
                    else
                    {
                        uSpaces = aSpaces.UniqueValues(true, 5);
                        cnt = 0;
                        idx = 0;
                        for (int i = 1; i <= uSpaces.Count; i++)
                        {
                            vcnt = aSpaces.Occurances(uSpaces.Item(i), true, 5);
                            if (vcnt > cnt)
                            {
                                cnt = vcnt;
                                idx = i;
                            }
                        }
                        _rVal = uSpaces.Item(idx);
                    }
                }
                if (_rVal <= 0) _rVal = IdealSectionHeight;
                return _rVal;
            }
        }


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

        double DeckThickness { get; set; }

        /// <summary>
        /// the points that are currently set as the centers of manways
        /// </summary>
        public uopVectors ManwayPoints
        {
            get
            {

                try
                {
                    ManwayCount(out uopVectors _rVal);
                    return _rVal;
                }
                catch (Exception e)
                { throw e; }

            }
        }


        public uopDeckSplice SelectedMember
        {
            get
            {

                if (Count <= 0) return null;
                uopDeckSplice _rVal = null;
                foreach (var mem in this)
                {
                    if (mem.Selected && _rVal == null) _rVal = mem; else mem.Selected = false;

                }

                if (_rVal == null && Count > 0)
                {
                    this[0].Selected = true;
                    _rVal = this[0];

                }

                return _rVal;

            }
            set
            {
                if (value == null) return;
                int idx = IndexOf(value);
                if (idx <= 0) idx = FindIndex(x => string.Compare(x.Handle, value.Handle, true) == 0) + 1;
                if (idx <= 0) return;
                SelectedIndex = idx;

            }
        }

        public int SelectedIndex
        {
            get
            {
                uopDeckSplice aMem = SelectedMember;
                return aMem == null ? 0 : base.IndexOf(aMem) + 1;
            }

            set => SetSelected(value);

        }

        private dxePolygon _FemaleFlange_SAndT;
        public dxePolygon FemaleFlange_SAndT
        {

            get
            {
                if (_FemaleFlange_SAndT != null) return _FemaleFlange_SAndT;

                uopTrayAssembly assy = TrayAssembly;
                if (assy == null) return null;


                double wd = assy.PanelWidth();
                colDXFVectors verts = colDXFVectors.Zero;

                dxfVector u1 = verts.Add(-wd / 2, 0);
                verts.AddRelative(mdGlobals.DeckTabFlangeInset - 0.24, 0);
                verts.AddRelative(0.24 / 2, -0.125, aVertexRadius: 0.125);
                verts.AddRelative(0.24 / 2, 0.125, aVertexRadius: 0.125);
                u1 = verts.AddRelative(0, mdGlobals.DeckTabFlangeHeight, aTag: "FLANGE1");
                verts.AppendMirrors(0, null, bReverseOrder: true);
                dxfVector u2 = verts.Item(5);
                u2.Tag = "FLANGE2";
                _FemaleFlange_SAndT = new dxePolygon(verts, dxfVector.Zero);

                double thk = DeckThickness;
                _FemaleFlange_SAndT.AdditionalSegments.Add(new dxeLine(u1.Moved(0, -thk), u2.Moved(0, -thk)) { Linetype = dxfLinetypes.Hidden });

                return _FemaleFlange_SAndT;
            }

        }
        public  string INIPath
        {
            get
            {
                uopTrayAssembly assy = TrayAssembly;
                return assy !=null ? $"COLUMN({assy.ColumnIndex}).RANGE({assy.RangeIndex}).TRAYASSEMBLY.DECKSPLICES" : "TRAYASSEMBLY.DECKSPLICES";
            }
            
        }
        #endregion Properties

        #region Methods

        public uopRectangles GetLimits(double aAdder = 0) => uopDeckSplices.GetSpliceLimits(this, aAdder);

        public new int IndexOf(uopDeckSplice aSplice) => aSplice == null ? 0 : base.IndexOf(aSplice) + 1;

        public virtual void SetSelected(int aIndex)
        {
            int j = SelectedIndex;
            for (int i = 1; i <= Count; i++)
            {
                this[i - 1].Selected = i == aIndex;

                if (i == aIndex)
                    j = i;
            }

            if (j == 0 && Count > 0)
            {
                this[0].Selected = true;
                j = 1;
            }

        }

        public void SetSelected(uopDeckSplice aMember)
        {
            int idx = IndexOf(aMember);

            int j = 0;
            for (int i = 1; i <= Count; i++)
            {
                this[i - 1].Selected = i == idx;

                if (i == idx)
                    j = i;
            }

            if (j == 0 && Count > 0)
            {
                this[0].Selected = true;
                j = 1;
            }

        }

        public void Copy(uopDeckSplices aSplices, bool bDontCloneMembers = false)
        {
            if (aSplices == null) return;

            uopTrayAssembly assy = aSplices.TrayAssembly;

            _Invalid = aSplices._Invalid;
            if (assy == null)
            {
                _ManwayHeight = aSplices._ManwayHeight;
                _SpliceStyle = aSplices._SpliceStyle;
                _JoggleAngleHeight = aSplices._JoggleAngleHeight;
                ProjectFamily = aSplices.ProjectFamily;
                MDDesignFamily = aSplices.MDDesignFamily;
                TrayName = aSplices.TrayName;
                RingRadius = aSplices.RingRadius;
                DeckRadius = aSplices.DeckRadius;
                ShellRadius = aSplices.ShellRadius;
                DeckThickness = aSplices.DeckThickness;
                _FemaleFlange_SAndT = (dxePolygon)dxfEntity.CloneCopy(aSplices._FemaleFlange_SAndT);
            }
            else
            {
                TrayAssembly = assy;
            }

            Clear();
            foreach (var aSplice in aSplices)
                Add(aSplice, !bDontCloneMembers);
        }

        public void SubPart(uopTrayAssembly aAssy)
        {
            if (aAssy != null) TrayAssembly = aAssy;
        }

        public mdTrayAssembly GetMDTrayAssembly(mdTrayAssembly aAssy = null)
        {
            if (aAssy != null) return aAssy;
            uopTrayAssembly assy = TrayAssembly;

            return assy == null ? null : assy.ProjectFamily == uppProjectFamilies.uopFamMD ? (mdTrayAssembly)assy : null;

        }

        public new void Add(uopDeckSplice aSplice)
        {
            if (aSplice == null) return;
            SetMemberInfo(aSplice, Count + 1);
            base.Add(aSplice);

        }
        public void Add(uopDeckSplice aSplice, bool bAddClone)
        {
            if (aSplice == null) return;
            Add(bAddClone ? new uopDeckSplice(aSplice) : aSplice);

        }

        /// <summary>
        /// adds clones of the passed collection to this collection
        /// </summary>
        /// <param name="aSplices"></param>
        public void Append(IEnumerable<uopDeckSplice> aSplices)
        {
            if (aSplices == null) return;
            foreach (var item in aSplices) Add(item, true);

        }

        public uopDeckSplices Clone() => new uopDeckSplices(this);
        object ICloneable.Clone() => (object)new uopDeckSplices(this);

        /// <summary>
        ///returns the first splice with a manway index that matches the passed value
        /// </summary>
        /// <param name="aManID"></param>
        /// <param name="bRemove"></param>
        /// <param name="bJustOne"></param>
        /// <param name="rPanelIndex"></param>
        /// <param name="rIndexes"></param>
        /// <returns></returns>
        public List<uopDeckSplice> GetByManwayHandle(string aManID, bool bRemove = false, bool bJustOne = false, List<uopDeckSplice> aSubset = null)
        {
            int cnt = (aSubset == null) ? Count : aSubset.Count;
            List<uopDeckSplice> _rVal = new List<uopDeckSplice>();

            if (cnt <= 0) return _rVal;


            for (int i = 1; i <= cnt; i++)
            {
                uopDeckSplice aMem = (aSubset == null) ? Item(i) : aSubset[i - 1];
                if (string.Compare(aMem.ManwayHandle, aManID, ignoreCase: true) == 0)
                {
                    _rVal.Add(aMem);
                    if (bJustOne) break;

                }
            }
            if (bRemove) RemoveMembers(_rVal, aSubset);

            return _rVal;
        }
        public uopPropertyArray SaveProperties(mdTrayAssembly aAssy) => new uopPropertyArray(CurrentProperties(aAssy), aName: INIPath, aHeading: INIPath);
    
        /// <summary>
        /// Gets Panel Indexs
        /// </summary>
        /// <returns></returns>
        public List<int> PanelIndexes(List<uopDeckSplice> aSubset = null)
        {
            int cnt = (aSubset == null) ? Count : aSubset.Count;
            uopDeckSplice aMem;
            List<int> _rVal = new List<int>();
            bool keep;
            int pid;
            for (int i = 1; i <= cnt; i++)
            {
                aMem = (aSubset == null) ? Item(i) : aSubset[i - 1];
                keep = true;
                pid = aMem.PanelIndex;

                for (int j = 0; j < _rVal.Count; j++)
                {
                    if (_rVal[j] == pid) { keep = false; break; }
                }
                if (keep) _rVal.Add(pid);
            }
            _rVal.Sort();
            return _rVal;
        }

        public int SpliceCount(uppSpliceTypes aSpliceType, bool bTrayWide = false)
        {
            int _rVal = 0;
            for (int i = 1; i <= Count; i++)
            {
                var aMem = Item(i);
                if (aMem.SpliceType == aSpliceType)
                {
                    _rVal += 1;
                    if (bTrayWide)
                    {
                        if (Math.Round(aMem.X, 1) > 0) _rVal += 1;

                    }
                }
            }
            return _rVal;
        }

        // TODO: Arguments not allowed on properties: Optional rManCount As Integer
        //       Public Property Get SpliceTypes(Optional rManCount As Integer) As mzValues
        public mzValues SpliceTypes(out int rManCount)
        {
            mzValues _rVal = new mzValues("SPLICE TYPES");
            rManCount = ManwayCount();

            uopDeckSplice aMem;

            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                _rVal.Add(aMem.SpliceType, true);
            }

            return _rVal;

        }

        public List<uopDeckSplice> GetManwaySplices(int aPanelIndex = 0, bool bRemove = false, List<uopDeckSplice> aSubset = null)
        {
            int cnt = (aSubset == null) ? Count : aSubset.Count;
            List<uopDeckSplice> _rVal = new List<uopDeckSplice>();
            for (int i = 1; i <= cnt; i++)
            {
                uopDeckSplice aMem = (aSubset == null) ? this[i - 1] : aSubset[i - 1];
                if (!string.IsNullOrWhiteSpace(aMem.ManwayHandle))
                {
                    if (aPanelIndex <= 0 || aMem.PanelIndex == aPanelIndex) _rVal.Add(aMem);

                }

            }
            if (bRemove) RemoveMembers(_rVal);
            return _rVal;
        }

        /// <summary>
        ///returns the item from the collection at the requested index ! Base 1 !
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
        /// 
        public virtual uopDeckSplice Item(int aIndex)
        {
            if (aIndex < 1 || aIndex > Count)
            {
                throw new IndexOutOfRangeException();
            }
            SetMemberInfo(base[aIndex - 1], aIndex);

            return base[aIndex - 1];
        }


        private void SetMemberInfo(uopDeckSplice aMember, int? aIndex = null)
        {
            if (aMember != null)
            {
                if (aIndex.HasValue) aMember.SpliceIndex = aIndex.Value;
                aMember.FlangeHt = JoggleAngleHeight;
                aMember.SpliceStyle = SpliceStyle;
                aMember.ProjectFamily = ProjectFamily;
                aMember.MDDesignFamily = MDDesignFamily;
                aMember.ShellRadius = ShellRadius;
                aMember.RingRadius = RingRadius;
                aMember.DeckRadius = DeckRadius;
                aMember.DeckThickness = DeckThickness;
            }
        }

        /// <summary>
        /// removes the members of the passed colleciton from this collection
        /// </summary>
        /// <param name="aMembers"></param>
        /// <param name="aSubset"></param>
        /// <returns></returns>
        public int RemoveMembers(List<uopDeckSplice> aMembers, List<uopDeckSplice> aSubset = null) => uopDeckSplices.RemoveSplices(aSubset == null ? this : aSubset, aMembers);

        /// <summary>
        /// returns the height of the current manways
        /// </summary>
        public double GetManwayHt()
        {
            double _rVal = 0;
            List<uopDeckSplice> bCol = GetManwaySplices();
            List<uopDeckSplice> cCol = null;

            if (bCol.Count > 0)
            {
                cCol = GetByManwayHandle(bCol[0].ManwayHandle, aSubset: bCol);

                uopVectors ctrs = Centers(bGetClones: true, aSubset: cCol);
                _rVal = ctrs.Bounds.Height;

            }
            return _rVal;
        }
        /// <summary>
        /// empties the collection
        /// </summary>
        /// <param name="bRetainManways"></param>
        /// <returns></returns>
        public int Clear(bool bRetainManways = false)
        {
            int cnt = Count;
            List<uopDeckSplice> MnSplices = bRetainManways ? GetManwaySplices(bRemove: true) : null;
            cnt = this.Count;

            base.Clear();
            if (bRetainManways) Append(MnSplices);
            return cnt - Count;
        }
        public void ChangeSpliceStyle(uppSpliceStyles? aStyle, uopTrayAssembly aAssy = null)
        {
            if (!aStyle.HasValue) aStyle = SpliceStyle;
            if ((int)aStyle.Value < 0) return;
            aAssy ??= TrayAssembly;
            _SpliceStyle = aStyle.Value;
            if (aAssy == null || Count <= 0) return;
            if (aAssy.ProjectFamily != uppProjectFamilies.uopFamMD) return;
            mdTrayAssembly assy = (mdTrayAssembly)aAssy;

            double ManHt = GetManwayHt();
            uopVectors freecenters = assy.DowncomerData.FreeCenters();
            int cnt = ManwayCount(out uopVectors mpts, bTrayWide: false);
            List<uopDeckSplice> curmems = new List<uopDeckSplice>(this);
            Clear();
         
            foreach (uopDeckSplice cur in curmems)
            {
                if (cur.SupportsManway) continue;
                if (!cur.Vertical)
                    AddHorizontalSpliceMD(assy, cur.PanelIndex, cur.Ordinate, aSpliceStyle: _SpliceStyle);
                else
                    AddVerticalSplices(aAssy, aSpliceStyle: _SpliceStyle, cur.Ordinate);

            }
            foreach (var mpt in mpts)
            {
                AddManway(aAssy, mpt.Handle, freecenters, _SpliceStyle, ref ManHt, out int rPanelIndex, out string rErrorString);
            }
            Invalid = true;
        }



        /// <summary>
        /// shorthand method for adding horizontal splice
        /// </summary>
        /// <param name="aAssy">the parent Tray</param>
        /// <param name="aPanelIndex">the index of the panel to add a splice for</param>
        /// <param name="aOrdinate">the y ordinate of the new splice</param>
        /// <param name="rErrorString">returns an error string if the ordinate is invalid</param>
        /// <param name="rFirstSplice">returns the first added splice</param>
        /// <param name="rSecondSplice">returns the mirrored splice if it was created</param>
        /// <param name="aSnapToMax">flag to add a splice at the max y if the passed Y is greater</param>
        /// <param name="bAvoidManways">flag to not add the splice if it lies within a manway</param>
        /// <param name="bAddMirror"></param>
        /// <param name="bAddJoggles">flag to add a joggle splice at the passed Y if is greater that the max allowable</param>
        /// <param name="bDontVerify"></param>
        /// <returns></returns>
        public bool ValidateSpliceOrdinateMD(mdTrayAssembly aAssy, int aPanelIndex,  ref double ioOrdinate, ref bool ioAddMirror, out string rErrorString,  bool aSnapToMax = true, bool bAvoidManways = true,  bool bAddJoggles = false,  uppSpliceStyles aSpliceStyle = uppSpliceStyles.Undefined)
        {
        
            rErrorString = string.Empty;

            if (aAssy != null) TrayAssembly = aAssy;
            aAssy ??= MDTrayAssembly;

            if (aPanelIndex <= 1 || aAssy == null) { rErrorString = "Null Input"; return false; }


            colMDDeckPanels aDeckPanels = aAssy.DeckPanels;


            if (aDeckPanels.Count <= 1 || aPanelIndex > aDeckPanels.Count) { rErrorString = $"Invalid Panel ID {aPanelIndex}"; return false; }

            mdDeckPanel aDP = aDeckPanels.Item(aPanelIndex);

            if (_SpliceStyle <= uppSpliceStyles.Undefined) _SpliceStyle = aAssy.SpliceStyle;
            if (aSpliceStyle <= uppSpliceStyles.Undefined) aSpliceStyle = _SpliceStyle;

            uopFreeBubblingPanel panel = aAssy.FreeBubblingPanels.Find(x => x.PanelIndex == aPanelIndex);
            if (panel == null) { rErrorString = $"Invalid Panel ID {aPanelIndex}"; return false; }

            uopFreeBubblingArea panelsection = null;
            //get the sub panel
            foreach (uopFreeBubblingArea fba in panel)
            {
                if (fba.IsHalfMoon)
                {
                    if (ioOrdinate >= fba.Bottom || ioOrdinate <= fba.Top)
                    {
                        panelsection = fba; break;
                    }
                }
                else
                {
                    if (ioOrdinate >= fba.Left || ioOrdinate <= fba.Right)
                    {
                        panelsection = fba; break;
                    }
                }
                if (panelsection == null) { rErrorString = $"Invalid Ordinate For Panel {aPanelIndex} Requested"; return false; }


                double y2 = panelsection.MaxSpliceOrdinate;
                double y1 = panelsection.MinSpliceOrdinate;
                if (panelsection.IsHalfMoon)
                {
                    if (ioOrdinate > y1 || ioOrdinate < y2)
                    {
                        if (!aSnapToMax)
                            rErrorString = $"Invalid Ordinate For Panel Section {panelsection.PanelIndex},{panelsection.SectionIndex}";
                        else
                            if (ioOrdinate > y1) ioOrdinate = y1; else ioOrdinate = y2;
                    }
                }
                else
                {
                    if (ioOrdinate > y1 || ioOrdinate < y2)
                    {

                        if (!aSnapToMax)
                            rErrorString = $"Invalid Ordinate For Panel Section {panelsection.PanelIndex},{panelsection.SectionIndex}";
                        else
                            if (ioOrdinate > y1) ioOrdinate = y1; else ioOrdinate = y2;
                    }
                }

            }
            bool addit = string.IsNullOrWhiteSpace(rErrorString);

            if (bAvoidManways && addit)
            {

                if (LiesWithinManway(aPanelIndex, ioOrdinate)) { addit = false; rErrorString = $"The Requested Ordinate Is Within a Manway"; }


            }

            if (!addit) return false;

          


            if (ioAddMirror)
            {
                if (panelsection.IsHalfMoon)
                {
                    ioAddMirror = false;
                }
                else
                {
                    addit = Math.Round(ioOrdinate, 2) != Math.Round(panelsection.Y, 2);
                    {
                        ioOrdinate = ioOrdinate > panelsection.Y ? panelsection.Y - Math.Abs(ioOrdinate) : panelsection.Y + Math.Abs(ioOrdinate);

                    }

                    if (!addit) return false;

                }
            }


            return true;
        }



        /// <summary>
        /// shorthand method for adding horizontal splice
        /// </summary>
        /// <param name="aAssy">the parent Tray</param>
        /// <param name="aPanelIndex">the index of the panel to add a splice for</param>
        /// <param name="aOrdinate">the y ordinate of the new splice</param>
        /// <param name="rErrorString">returns an error string if the ordinate is invalid</param>
        /// <param name="rFirstSplice">returns the first added splice</param>
        /// <param name="rSecondSplice">returns the mirrored splice if it was created</param>
        /// <param name="aSnapToMax">flag to add a splice at the max y if the passed Y is greater</param>
        /// <param name="bAvoidManways">flag to not add the splice if it lies within a manway</param>
        /// <param name="bAddMirror"></param>
        /// <param name="bAddJoggles">flag to add a joggle splice at the passed Y if is greater that the max allowable</param>
        /// <param name="bDontVerify"></param>
        /// <returns></returns>
        public bool AddHorizontalSpliceMD(mdTrayAssembly aAssy, int aPanelIndex, double aOrdinate,  out string rErrorString, out uopDeckSplice rFirstSplice, out uopDeckSplice rSecondSplice, bool aSnapToMax = true, bool bAvoidManways = true, bool bAddMirror = false, bool bAddJoggles = false, bool bDontVerify = false, uppSpliceStyles aSpliceStyle = uppSpliceStyles.Undefined)
        {
            rFirstSplice = null;
            rSecondSplice = null;
            rErrorString = string.Empty;
          
            if (aAssy != null) TrayAssembly = aAssy;
            aAssy ??= MDTrayAssembly;

            if (aPanelIndex <= 1 || aAssy == null) { rErrorString = "Null Input";return false; }


            colMDDeckPanels aDeckPanels = aAssy.DeckPanels;


            if (aDeckPanels.Count <= 1 || aPanelIndex > aDeckPanels.Count) { rErrorString = $"Invalid Panel ID {aPanelIndex}";return false; }

            mdDeckPanel aDP = aDeckPanels.Item(aPanelIndex);

            if (_SpliceStyle <= uppSpliceStyles.Undefined) _SpliceStyle = aAssy.SpliceStyle;
            if (aSpliceStyle <= uppSpliceStyles.Undefined) aSpliceStyle = _SpliceStyle;

           uopFreeBubblingPanel panel = aAssy.FreeBubblingPanels.Find(x => x.PanelIndex == aPanelIndex);
            if(panel == null) { rErrorString = $"Invalid Panel ID {aPanelIndex}";return false; }

            uopFreeBubblingArea panelsection = null;
            //get the sub panel
            foreach (uopFreeBubblingArea fba in panel)
            {
                if (fba.IsHalfMoon)
                {
                    if(aOrdinate >= fba.Bottom || aOrdinate <= fba.Top)
                    {
                        panelsection = fba; break;
                    }
                }
                else
                {
                    if (aOrdinate >= fba.Left || aOrdinate <= fba.Right)
                    {
                        panelsection = fba; break;
                    }
                }
                if (panelsection == null) { rErrorString = $"Invalid Ordinate For Panel {aPanelIndex} Requested";return false; }

                
                double y2 = panelsection.MaxSpliceOrdinate;
                double y1 = panelsection.MinSpliceOrdinate;
                if (panelsection.IsHalfMoon)
                {
                    if (aOrdinate > y1 || aOrdinate < y2)
                    {
                        if (!aSnapToMax)
                            rErrorString = $"Invalid Ordinate For Panel Section {panelsection.PanelIndex},{panelsection.SectionIndex}";
                        else
                            if (aOrdinate > y1) aOrdinate = y1; else aOrdinate = y2;
                    }
                }
                else
                {
                    if (aOrdinate > y1 || aOrdinate < y2)
                    {

                        if (!aSnapToMax)
                            rErrorString = $"Invalid Ordinate For Panel Section {panelsection.PanelIndex},{panelsection.SectionIndex}";
                        else
                            if (aOrdinate > y1) aOrdinate = y1; else aOrdinate = y2;
                    }
                }

            }
            bool addit = string.IsNullOrWhiteSpace(rErrorString);

            if (bAvoidManways && addit)
            {

                if (LiesWithinManway(aPanelIndex, aOrdinate)) { addit = false; rErrorString = $"The Requested Ordinate Is Within a Manway"; }


            }

            if (!addit)return false;

            RemoveNearbySplices(panelsection.PanelIndex, aOrdinate);
            rFirstSplice = aDP.BasicDeckSplice(aAssy, aOrdinate , aSpliceStyle);
            
            if (rFirstSplice != null)
                Add(rFirstSplice);
            else
            {
                addit = false; bAddMirror = false;
                rErrorString = $"Unable To Add the Requested Splice at {aOrdinate:0.0000}";
            }
                    
           
            if (!addit) return false;
            if(bAddMirror) 
            {
                if (panelsection.IsHalfMoon)
                {
                    aOrdinate = -aOrdinate;
                }
                else
                {
                    addit = Math.Round(aOrdinate, 2) != Math.Round(panelsection.Y, 2);
                    {
                        aOrdinate = aOrdinate > panelsection.Y  ?  panelsection.Y - Math.Abs(aOrdinate) : panelsection.Y + Math.Abs(aOrdinate);

                    }

                    if (!addit) return false;

                    RemoveNearbySplices(panelsection.PanelIndex, aOrdinate);
                    rSecondSplice = aDP.BasicDeckSplice(aAssy, aOrdinate, aSpliceStyle);

                    if (rSecondSplice != null)
                        Add(rSecondSplice);
                    else
                    {
                        addit = false; bAddMirror = false;
                        rErrorString = $"Unable To Add the Requested Splice at {aOrdinate:0.0000}";
                    }
                }
            }

            if (!bDontVerify) Verify(aAssy);

            return true;
        }



        /// <summary>
        /// shorthand method for adding horizontal splice
        /// </summary>
        /// <param name="aAssy">the parent Tray</param>
        /// <param name="aPanelIndex">the index of the panel to add a splice for</param>
        /// <param name="aOrdinate">the y ordinate of the new splice</param>
        /// <param name="aSnapToMax">flag to add a splice at the max y if the passed Y is greater</param>
        /// <param name="bAvoidManways">flag to not add the splice if it lies within a manway</param>
        /// <param name="bAddMirror"></param>
        /// <param name="bAddJoggles">flag to add a joggle splice at the passed Y if is greater that the max allowable</param>
        /// <param name="bDontVerify"></param>
        /// <returns></returns>
        public uopDeckSplice AddHorizontalSpliceMD(mdTrayAssembly aAssy, int aPanelIndex, double aOrdinate,  bool aSnapToMax = true, bool bAvoidManways = true, bool bAddMirror = false, bool bAddJoggles = false, bool bDontVerify = false, uppSpliceStyles aSpliceStyle = uppSpliceStyles.Undefined)
        {
           
            AddHorizontalSpliceMD(aAssy, aPanelIndex, aOrdinate, out _, out uopDeckSplice _rVal, out _,aSnapToMax,bAvoidManways,bAddMirror,bAddJoggles,bDontVerify );
            return _rVal;

        }
        public bool AddVerticalSplicesMD(mdTrayAssembly aAssy, out string rErrorString, out uopDeckSplice rFirstSplice, out uopDeckSplice rSecondSplice, uppSpliceStyles aSpliceStyle = uppSpliceStyles.Undefined, double aOrdinate = 0)
        {
            rErrorString = string.Empty;
            rFirstSplice = null;
            rSecondSplice = null;

            if (aAssy != null) TrayAssembly = aAssy;
            aAssy ??= MDTrayAssembly;
            if (aAssy == null || aAssy.ProjectFamily != uppProjectFamilies.uopFamMD) return false;

            mdTrayAssembly assy = (mdTrayAssembly)aAssy;
            mdDeckPanel aPanel = assy.DeckPanels.Item(1);

            if (aPanel == null) return false;
            if (aSpliceStyle == uppSpliceStyles.Undefined) aSpliceStyle = aAssy.SpliceStyle;

            List<uopDeckSplice> aCol = GetVertical(bRemove: true, aPanelndex: aPanel.Index);
            double x1 = (aOrdinate > 0) ? aOrdinate : 0;

            if (x1 <= aPanel.Left(true) + 4)
            {
                x1 = aPanel.Left(true);
                x1 = x1 + (aPanel.Radius - x1) / 2 + 0.079;
            }

            uopDeckSplice splice = aPanel.BasicDeckSplice(assy, x1, aSpliceStyle);
            splice.Side = uppSides.Right;
            Add(splice);
            return true;
        }




        public uopDeckSplice AddVerticalSplices(uopTrayAssembly aAssy, uppSpliceStyles aSpliceStyle = uppSpliceStyles.Undefined, double aOrdinate = 0)
        {

            if (aAssy != null) TrayAssembly = aAssy;
            aAssy ??= TrayAssembly;
            if (aAssy == null || aAssy.ProjectFamily != uppProjectFamilies.uopFamMD) return null;

            mdTrayAssembly assy = (mdTrayAssembly)aAssy;
            mdDeckPanel aPanel = assy.DeckPanels.Item(1);

            if (aPanel == null) return null;
            if (aSpliceStyle == uppSpliceStyles.Undefined) aSpliceStyle = aAssy.SpliceStyle;

            List<uopDeckSplice> aCol = GetVertical(bRemove: true, aPanelndex: aPanel.Index);
            double x1 = (aOrdinate > 0) ? aOrdinate : 0;

            if (x1 <= aPanel.Left(true) + 4)
            {
                x1 = aPanel.Left(true);
                x1 = x1 + (aPanel.Radius - x1) / 2 + 0.079;
            }

            uopDeckSplice splice = aPanel.BasicDeckSplice(assy, x1, aSpliceStyle);
            splice.Side = uppSides.Right;
            Add(splice);
            return splice;
        }

                /// <summary>
        /// returns only the splices with the passed panel index
        /// </summary>
        /// <param name="aPanelIndex"></param>
        /// <param name="bIncludeManways"></param>
        /// <param name="bRemove"></param>
        /// <param name="aSubset"></param>
        ///  /// <param name="bSort"></param>
        /// <returns></returns>
        public List<uopDeckSplice> GetByPanelIndex(int aPanelIndex, bool bIncludeManways = true, bool bRemove = false, List<uopDeckSplice> aSubset = null, bool bSort = false)
        {
            int cnt = (aSubset == null) ? Count : aSubset.Count;
            List<uopDeckSplice> _rVal = new List<uopDeckSplice>();
            if (cnt <= 0) return _rVal;

            for (int i = 1; i <= cnt; i++)
            {
                uopDeckSplice aMem = (aSubset == null) ? Item(i) : aSubset[i - 1];
                if (aSubset != null) aMem.SpliceIndex = i;
                if (aMem.PanelIndex == aPanelIndex)
                {
                    if (bIncludeManways || (!bIncludeManways && !aMem.SupportsManway)) _rVal.Add(aMem);
                }
            }

            if (_rVal.Count <= 0) return _rVal;
            if (bSort)
                Sort(aPanelIndex, aSubset: _rVal);

            if (bRemove)
            {

                RemoveMembers(_rVal);

            }

            return _rVal;
        }

        public bool AddManway(mdTrayAssembly aAssy, double aManwayHt, string aManHandle, out string rErrorString)
        {
            rErrorString = string.Empty;
            return AddManway(aAssy, aManHandle, null, uppSpliceStyles.Undefined, ref aManwayHt, out int PIPD, out rErrorString);

        }

        public bool AddManway(uopTrayAssembly aAssy, string aManHandle, uopVectors aFreePoints, uppSpliceStyles aSpliceStyle, ref double aManHeight, out int rPanelIndex, out string rErrorString)
        {
            bool _rVal = false;
            rPanelIndex = 0;
            rErrorString = string.Empty;
            if (string.IsNullOrWhiteSpace(aManHandle)) return _rVal;
            aAssy ??= TrayAssembly;
            if (aAssy == null || aAssy.ProjectFamily != uppProjectFamilies.uopFamMD) return _rVal;

            mdTrayAssembly assy = (mdTrayAssembly)aAssy;

            if (aSpliceStyle < 0) aSpliceStyle = assy.SpliceStyle;


            uopDeckSplice aDS;
            uopDeckSplice bDS;
            uopDeckSplice cDS = null;

            colMDDeckPanels aDPs = assy.DeckPanels;
            if (aFreePoints == null) aFreePoints = assy.DowncomerData.FreeCenters();
            uopVectors find = aFreePoints.GetByHandle(aManHandle, true, true);
            if (find.Count <= 0) return _rVal;
            uopVector v1 = find.Item(1);
            string hndl = v1.Handle;

            //get the panel

            int DPID = mzUtils.VarToInteger(v1.Tag);
            if (DPID <= 0 || DPID > aDPs.Count) return _rVal;
            mdDeckPanel aDP = aDPs.Item(DPID);
            rPanelIndex = aDP.Index;

            //set the height
            double ManHt = Math.Abs(aManHeight);
            if (ManHt <= 0) ManHt = GetManwayHt();

            ManHt = Math.Round(mzUtils.LimitedValue(ManHt, 12, 2 * assy.DowncomerSpacing, mdUtils.IdealManwayHeight(assy,aSpliceStyle)), 5);
            aManHeight = ManHt;

            //set the ordinates
            double cY = v1.Y;
            double y1 = cY + 0.5 * ManHt;
            double y2 = cY - 0.5 * ManHt;
            bool bCtrSplice = aSpliceStyle == uppSpliceStyles.Tabs;

            // Check if the manway fits inside the panel shape
            if (!ManwayFitsInPanelShape(aDP, y1, cY, y2, assy))
            {
                return _rVal;
            }

            //get rid of the existing manway (if it's there)
            RemoveManway(hndl, aAssy);
            bool bAddIt = true;
            List<uopDeckSplice> mSplices = GetBetweenOrdinates(y1 + 4, y2 - 4, aDP.Index);

            if (mSplices.Find((x) => !string.IsNullOrWhiteSpace(x.ManwayHandle)) != null)
            {
                //can't add the manway cause it will lap another
                return _rVal;
            }


            //delete any splices in the manways y range on it's panel
            if (mSplices.Count > 0)
                RemoveMembers(mSplices);

            //the top splice
            aDS = AddHorizontalSpliceMD(assy, aDP.Index, aOrdinate: y1, aSnapToMax: false, bAvoidManways: false, bAddMirror: false, bAddJoggles: false, bDontVerify: true);

            if (bCtrSplice)
            {
                //the center splice
                cDS = AddHorizontalSpliceMD(assy, aDP.Index, aOrdinate: cY, aSnapToMax: false, bAvoidManways: false, bAddMirror: false, bAddJoggles: false, bDontVerify: true);
            }
            //the bottom splice
            bDS = AddHorizontalSpliceMD(assy, aDP.Index, aOrdinate: y2, aSnapToMax: false, bAvoidManways: false, bAddMirror: false, bAddJoggles: false, bDontVerify: true);

            if (aDS != null)
            {
                aDS.Direction = dxxOrthoDirections.Down;
                aDS.ManwayHandle = hndl;
                aDS.ManTag = "TOP";

                if (aSpliceStyle == uppSpliceStyles.Joggle) aDS.SpliceType = uppSpliceTypes.SpliceWithAngle;


            }
            else
            {
                bAddIt = false;
            }
            if (cDS != null)
            {
                cDS.Direction = dxxOrthoDirections.Down;
                cDS.ManwayHandle = hndl;
                cDS.SpliceType = uppSpliceTypes.SpliceManwayCenter;
                cDS.ManTag = "CENTER";

            }
            else
            {
                if (bCtrSplice) bAddIt = false;

            }
            if (bDS != null)
            {
                bDS.Direction = dxxOrthoDirections.Up;
                bDS.ManwayHandle = hndl;
                bDS.ManTag = "BOTTOM";
                if (aSpliceStyle == uppSpliceStyles.Joggle)
                {
                    bDS.SpliceType = uppSpliceTypes.SpliceWithAngle;
                }

            }
            else
            {
                bAddIt = false;
            }

            if (!bAddIt)
            {
                RemoveMember(aDS);
                RemoveMember(bDS);
                RemoveMember(cDS);
                rErrorString = "The Selected Site Cannot Be Used As a Manway";
            }
            else
            {
                if (aDS != null) SetItem(IndexOf(aDS), aDS);
                if (bDS != null) SetItem(IndexOf(bDS), bDS);
                if (cDS != null) SetItem(IndexOf(cDS), cDS);

                _rVal = true;
            }
            return _rVal;
        }

        public void SetItem(int aIndex, uopDeckSplice aSplice)
        {
            if (aIndex < 1 || aIndex > Count || aSplice == null) throw new IndexOutOfRangeException();
            SetMemberInfo(aSplice, aIndex);
            base[aIndex - 1] = aSplice;

        }

        /// <summary>
        /// removes the splices that are associated to the passed manway handle
        /// </summary>
        /// <param name="aManHandle"></param>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public bool RemoveManway(string aManHandle, uopTrayAssembly aAssy, List<uopDeckSplice> aSubset = null)
        {


            int cnt = (aSubset == null) ? Count : aSubset.Count;
            if (cnt <= 0) return false;

            List<uopDeckSplice> mansplices = GetByManwayHandle(aManHandle, bRemove: false, bJustOne: false, aSubset: aSubset);

            bool _rVal = false;


            aAssy ??= TrayAssembly;
            bool rBackFill = false;
            _rVal = RemoveManway(aManHandle: aManHandle, aAssy: aAssy, out int PID, ref rBackFill, aSubset: aSubset);
            if (rBackFill) Verify(aAssy);

            return _rVal;
        }
        /// <summary>
        /// returns True if the passed Y ordinate lies within a manway on the passed panel
        /// </summary>
        /// <param name="aPanelIndex"></param>
        /// <param name="aYOrdinate"></param>
        /// <param name="aSubset"></param>
        /// <returns></returns>
        public bool LiesWithinManway(int aPanelIndex, double aYOrdinate, List<uopDeckSplice> aSubset = null)
        {

            if (aPanelIndex <= 1) return false;

            int cnt = (aSubset == null) ? Count : aSubset.Count;
            if (cnt <= 0) return false;

            double y1;
            double y2;
            List<uopDeckSplice> pSplices;

            pSplices = GetByPanelIndex(aPanelIndex, aSubset: aSubset);
            for (int i = 1; i <= pSplices.Count; i++)
            {
                uopDeckSplice aDS = pSplices[i - 1];
                uopDeckSplice bDS = (i + 1 <= pSplices.Count) ? pSplices[i] : null;


                if (aDS != null && bDS != null)
                {
                    if (aDS.SupportsManway && bDS.SupportsManway)
                    {
                        if (aDS.ManwayHandle == bDS.ManwayHandle)
                        {
                            y1 = aDS.Y + 0.03125;
                            y2 = bDS.Y - 0.03125;
                            if (aYOrdinate <= y1 && aYOrdinate >= y2)
                            {
                                return true;

                            }
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// removes any slice withing 4 inches of the passed ordinate
        /// </summary>
        /// <param name="aPanelIndex"></param>
        /// <param name="aOrdinate"></param>
        /// <param name="aBuffer"></param>
        /// <returns></returns>
        public bool RemoveNearbySplices(int aPanelIndex, double aOrdinate, double aBuffer = 4, List<uopDeckSplice> aSubset = null)
        {
            bool _rVal = false;
            int cnt = (aSubset == null) ? Count : aSubset.Count;

            if (aPanelIndex == 1 || cnt <= 0) return false;
            List<uopDeckSplice> pSplices = GetByPanelIndex(aPanelIndex, aSubset: aSubset);

            pSplices.RemoveAll((x) => x.SupportsManway);

            aBuffer = Math.Abs(aBuffer);
            if (aBuffer < 1) aBuffer = 1;

            List<uopDeckSplice> removers = new List<uopDeckSplice>();
            removers = pSplices.FindAll((x) => (x.Ordinate <= aOrdinate + 0.5 * aBuffer) && (x.Ordinate >= aOrdinate - 0.5 * aBuffer));
            _rVal = removers.Count > 0;
            if (!_rVal) return false;
            if (aSubset != null)
            {
                foreach (var item in removers)
                {
                    if (aSubset != null)
                        aSubset.Remove(item);
                    else
                        Remove(item);
                }
            }

            return _rVal;
        }
        /// <summary>
        /// Returns Panel's ID
        /// </summary>
        /// <returns></returns>
        public List<int> PanelIDs(List<uopDeckSplice> aSubset = null)
        {
            int cnt = (aSubset == null) ? Count : aSubset.Count;
            List<int> _rVal = new List<int>();
            if (cnt <= 0) return _rVal;

            uopDeckSplice aMem;
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
        /// returns the list of iordinates for the splices in the collection
        /// </summary>
        /// <param name="aPanelID"></param>
        /// <param name="bExcludeManwayCenters"></param>
        /// <param name="aSubset"></param>
        /// <returns></returns>
        public List<double> Ordinates(int aPanelID = 0, bool bExcludeManwayCenters = false, List<uopDeckSplice> aSubset = null)
        {
            List<double> _rVal = new List<double>();
            int cnt = (aSubset == null) ? Count : aSubset.Count;

            if (cnt <= 0) return _rVal;

            uopDeckSplice aMem;
            for (int i = 1; i <= cnt; i++)
            {
                aMem = (aSubset == null) ? Item(i) : aSubset[i - 1];
                if (!bExcludeManwayCenters || (bExcludeManwayCenters && aMem.SpliceType != uppSpliceTypes.SpliceManwayCenter))
                {
                    if (aPanelID <= 0 || (aPanelID > 0 & aMem.PanelIndex == aPanelID)) _rVal.Add(aMem.Ordinate);
                }
            }
            return _rVal;
        }

        /// <summary>
        /// returns only the splices with the passed panel index
        /// </summary>
        /// <param name="aPanelIndex"></param>
        /// <param name="bIncludeManways"></param>
        /// <param name="aSubset"></param>
        /// <param name="bBailOnOne"></param>
        /// <returns></returns>
        public int PanelSpliceCount(int aPanelIndex, bool bIncludeManways = false, List<uopDeckSplice> aSubset = null, bool bBailOnOne = false)
        {
            int cnt = (aSubset == null) ? Count : aSubset.Count;
            int _rVal = 0;
            if (cnt <= 0) return _rVal;

            for (int i = 1; i <= cnt; i++)
            {
                uopDeckSplice aMem = (aSubset == null) ? Item(i) : aSubset[i - 1];
                if (aMem.PanelIndex == aPanelIndex)
                {
                    if (bIncludeManways || (!bIncludeManways && string.IsNullOrWhiteSpace(aMem.ManTag)))
                    {
                        _rVal++;
                        if (bBailOnOne) break;
                    }
                }
            }



            return _rVal;
        }

        /// <summary>
        /// sorts the splices in the collection in the requested order
        /// </summary>
        /// <param name="aPanelIndex">the order to sort the collection in</param>
        /// <param name="aAssignIndices">the reference point to use for sorting clockwise or counter-clockwise or nearest to farthest </param>
        /// <param name="aSubset"></param>
        public void Sort(int aPanelIndex = 0, bool aAssignIndices = true, List<uopDeckSplice> aSubset = null)
        {
            int cnt = (aSubset == null) ? Count : aSubset.Count;

            if (cnt <= 1) return;

            List<uopDeckSplice> rCol = new List<uopDeckSplice>();
            List<int> pIDs = PanelIDs(aSubset);
            List<uopDeckSplice> panelMems = null;
            List<uopDeckSplice> pAdd;
            uopDeckSplice pmem;
            double ord = 0;

            for (int p = 0; p < pIDs.Count; p++)
            {

                int PID = pIDs[p];

                if (aPanelIndex <= 0 || aPanelIndex == PID)
                {
                    panelMems = GetByPanelIndex(PID, bIncludeManways: true, bRemove: false, aSubset: aSubset, bSort: false);

                    List<double> aOrds = this.Ordinates(PID, bExcludeManwayCenters: false, aSubset: panelMems);
                    aOrds.Sort();
                    aOrds.Reverse();
                    pAdd = new List<uopDeckSplice>();


                    for (int i = 0; i < aOrds.Count; i++)
                    {

                        ord = aOrds[i];
                        pmem = panelMems.Find(x => x.Ordinate == ord);
                        if (pmem != null)
                        {
                            if (aAssignIndices) pmem.SpliceIndex = pAdd.Count + 1;
                            pAdd.Add(pmem);
                            panelMems.Remove(pmem);
                        }


                    }
                }
                else
                {
                    pAdd = panelMems;
                }


                for (int i = 1; i <= pAdd.Count; i++)
                {
                    rCol.Add(pAdd[i - 1]);
                }




            }
            if (aSubset == null)
            {
                base.Clear();
                base.AddRange(rCol);
            }
            else
            {
                aSubset.Clear();
                for (int i = 1; i <= rCol.Count; i++)
                {
                    aSubset.Add((uopDeckSplice)rCol[i - 1]);
                }
            }

        }

        public void Populate(List<uopDeckSplice> aSplices, bool bAddClones = false)
        {
            Clear();
            if (aSplices == null) return;
            foreach (var item in aSplices)
            {
                Add(item, bAddClones);
            }
        }
      
        private bool ManwayFitsInPanelShape(mdDeckPanel aPanel, double topSpliceOrdinate, double middleSpliceOrdinate, double bottomSpliceOrdinate, mdTrayAssembly assembly)
        {
            var panelShapes = mdDeckPanel.GetPanelShapes(aPanel, assembly);
            if (panelShapes == null) return false;
            var shapeForMiddleOrdinate = panelShapes.FirstOrDefault(ps => mdDeckPanel.IsValidPanelShapeForDeckSpliceOrdinate(ps, middleSpliceOrdinate, mdDeckPanel.MinDistanceFromTopOrBottom));
            if (shapeForMiddleOrdinate == null)
            {
                return false;
            }

            bool fitsInPanelShape = mdDeckPanel.IsValidPanelShapeForDeckSpliceOrdinate(shapeForMiddleOrdinate, topSpliceOrdinate, mdDeckPanel.MinDistanceFromTopOrBottom) && mdDeckPanel.IsValidPanelShapeForDeckSpliceOrdinate(shapeForMiddleOrdinate, bottomSpliceOrdinate, mdDeckPanel.MinDistanceFromTopOrBottom);

            return fitsInPanelShape;
        }
        /// <summary>
        /// removes the splices that are associated to the passed manway handle
        /// </summary>
        /// <param name="aManHandle"></param>
        /// <param name="aAssy"></param>
        /// <param name="rPanelIndex"></param>
        /// <param name="rBackFill"></param>
        /// <returns></returns>
        public bool RemoveManway(string aManHandle, uopTrayAssembly aAssy, out int rPanelIndex, ref bool ioBackFill, List<uopDeckSplice> aSubset = null)
        {
            rPanelIndex = 0;

            bool _rVal = false;
            if (string.IsNullOrWhiteSpace(aManHandle)) return _rVal;
            aManHandle = aManHandle.Trim();
            aAssy ??= TrayAssembly;
            if (aAssy == null) ioBackFill = false;
            if (aAssy == null || aAssy.ProjectFamily != uppProjectFamilies.uopFamMD) return _rVal;
            mdTrayAssembly assy = (mdTrayAssembly)aAssy;

            uopDeckSplices workCol = new uopDeckSplices(uopDeckSplices.CloneCopy(aSubset == null ? this : aSubset, bClone: true)) { TrayAssembly = assy };

            List<uopDeckSplice> rSplices = workCol.GetByManwayHandle(aManHandle, bRemove: true, bJustOne: false);
            double ht = 0;

            uopDeckSplice bSplc = null;
            int cnt = 0;

            colMDDeckPanels aDPs = assy.DeckPanels;

            if (rSplices.Count > 0)
            {
                _rVal = true;
                if (rSplices.Count > 1 && ioBackFill)
                {
                    uopDeckSplice tp = rSplices[0];
                    rPanelIndex = tp.PanelIndex;
                    uopDeckSplice bt = rSplices[rSplices.Count - 1];
                    ht = Math.Abs(tp.Ordinate - bt.Ordinate);
                    double ctr = bt.Ordinate + ht / 2;
                    double cX = tp.X;
                    List<uopDeckSplice> bSplices = new List<uopDeckSplice>();
                    if (Math.Round(ctr, 2) != 0)
                    {
                        bSplices = workCol.GetBetweenOrdinates(-ctr + ht / 2 + 4, -ctr - ht / 2 - 4, rPanelIndex, bSuppressManways: true, bReturnClones: true);

                        for (int i = bSplices.Count - 1; i >= 0; i--)
                        {
                            bSplc = bSplices[i];
                            if (workCol.GetBetweenOrdinates(-bSplc.Ordinate + 2, -bSplc.Ordinate - 2, rPanelIndex).Count == 0)
                            {
                                bSplc.Ordinate = -bSplc.Ordinate;
                            }
                            else
                            {
                                bSplices.RemoveAt(i);
                            }
                        }
                    }
                    if (bSplices.Count == 0)
                    {
                        bSplices = workCol.GetBetweenOrdinates(ctr + ht / 2 + 2, ctr - ht / 2 - 2, rPanelIndex + 1, bSuppressManways: true, bReturnClones: true);

                        if (bSplices.Count == 0 & (rPanelIndex - 1) > 1)
                        {
                            bSplices = workCol.GetBetweenOrdinates(ctr + ht / 2 + 2, ctr - ht / 2 - 2, rPanelIndex - 1, bSuppressManways: true, bReturnClones: true);
                        }

                    }
                    if (bSplices.Count > 0)
                    {
                        for (int i = 1; i <= bSplices.Count; i++)
                        {
                            bSplc = bSplices[i - 1];
                            if (workCol.AddHorizontalSpliceMD(aAssy: assy, aPanelIndex: rPanelIndex, aOrdinate: bSplc.Y, aSnapToMax: false, aSpliceStyle: uppSpliceStyles.Undefined) != null) cnt++;


                        }
                    }
                }
            }

            if (cnt == 0)
            {
                ioBackFill = false;
            }
            else
            {

                workCol.Verify(aAssy);
            }

            if (aSubset != null)
            {
                workCol.ToList(aCollector: aSubset, bClearCollector: true);
            }
            return _rVal;
        }


        public bool RemoveManways(mdTrayAssembly aAssy, bool ioBackFill) => RemoveManways(aAssy, ref ioBackFill, 0, out List<string> _);

        /// <summary>
        /// removes the splices that are associated to the manways
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="bBackFill"></param>
        /// <param name="aPanelIndex"></param>
        /// <param name="rManwayHandles"></param>
        /// <returns></returns>
        public bool RemoveManways(mdTrayAssembly aAssy, ref bool ioBackFill, int aPanelIndex, out List<string> rManwayHandles)
        {
            bool _rVal = false;
            rManwayHandles = ManwayHandles(aPanelIndex);
            if (rManwayHandles.Count <= 0) return _rVal;

            aAssy ??= GetMDTrayAssembly();
            for (int i = 1; i <= rManwayHandles.Count; i++)
            {
                if (RemoveManway(rManwayHandles[i - 1], aAssy, out int PID, ref ioBackFill)) _rVal = true;

            }
            return _rVal;



        }

        public bool RegenerateManways(mdTrayAssembly aAssy, double aManwayHeight = 0)
        {

            aAssy ??= GetMDTrayAssembly(aAssy);
            if (aAssy == null) return false;

            uppSpliceStyles sstyle = SpliceStyle;
            if (sstyle < 0) { sstyle = aAssy.DesignOptions.SpliceStyle; _SpliceStyle = sstyle; }

            double ManHt = Math.Abs(aManwayHeight);
            if (ManHt <= 0) ManHt = mdUtils.IdealManwayHeight(aAssy,sstyle);
            ManHt = mzUtils.LimitedValue(ManHt, aMin: 12, aMax: 2 * aAssy.DowncomerSpacing, aDefaultIfOutsideRange: GetManwayHt());

            int mcnt = ManwayCount(out uopVectors mpts, bTrayWide: true);
            if (mcnt == 0) return false;


            uopVectors fpts = aAssy.DowncomerData.FreeCenters();
            uopDeckSplices.CreateManways(this, ref sstyle, aAssy, aAssy.DeckPanels, ManHt, mpts, aFreeCenters: fpts);
            return true;
        }

        /// <summary>
        /// removes the splices that are associated to the passed manway handle
        /// </summary>
        /// <param name="aManHandle"></param>
        /// <param name="aAssy"></param>
        /// <param name="rPanelIndex"></param>
        /// <param name="ioBackFill"></param>
        /// <returns></returns>
        public bool RemoveManway(string aManHandle, mdTrayAssembly aAssy, out int rPanelIndex, ref bool ioBackFill, List<uopDeckSplice> aSubset = null)
        {
            rPanelIndex = 0;
            ioBackFill = false;
            bool _rVal = false;
            if (string.IsNullOrWhiteSpace(aManHandle)) return _rVal;
            aManHandle = aManHandle.Trim();
            uopDeckSplices workCol = new uopDeckSplices(aSubset == null ? this : aSubset);

            List<uopDeckSplice> rSplices = workCol.GetByManwayHandle(aManHandle, bRemove: false, bJustOne: false);
            foreach(var splice in rSplices) 
            { 
                if (aSubset == null)
                {
                    int idx = base.FindIndex(x => x.Handle == splice.Handle);
                    base.RemoveAt(idx);
                }
                    
                else
                {
                    aSubset.RemoveAt(aSubset.IndexOf(splice));

                }
            }
                 
            int cnt = 0;

            if (aAssy == null) ioBackFill = false;
            if (!ioBackFill) return rSplices.Count > 0;

            if (rSplices.Count > 0)
            {
                if (rSplices.Count >= 2 )
                {
                    _rVal = true;
                    uopDeckSplice tp = rSplices[0];
                    rPanelIndex = tp.PanelIndex;
                    uopDeckSplice bt = rSplices[rSplices.Count - 1];
                    double ht = Math.Abs(tp.Ordinate - bt.Ordinate);
                    double ctr = bt.Ordinate + ht / 2;
                    double cX = tp.X;
                    List<uopDeckSplice> bSplices = new List<uopDeckSplice>();
                    if (Math.Round(ctr, 2) != 0)
                    {
                        bSplices = workCol.GetBetweenOrdinates(-ctr + ht / 2 + 4, -ctr - ht / 2 - 4, rPanelIndex, bSuppressManways: true, bReturnClones: true);

                        for (int i = bSplices.Count - 1; i >= 0; i--)
                        {
                            uopDeckSplice bSplc = bSplices[i];
                            if (workCol.GetBetweenOrdinates(-bSplc.Ordinate + 2, -bSplc.Ordinate - 2, rPanelIndex).Count == 0)
                            {
                                bSplc.Ordinate = -bSplc.Ordinate;
                            }
                            else
                            {
                                bSplices.RemoveAt(i);
                            }
                        }
                    }
                    if (bSplices.Count == 0)
                    {
                        bSplices = workCol.GetBetweenOrdinates(ctr + ht / 2 + 2, ctr - ht / 2 - 2, rPanelIndex + 1, bSuppressManways: true, bReturnClones: true);

                        if (bSplices.Count == 0 & (rPanelIndex - 1) > 1)
                        {
                            bSplices = workCol.GetBetweenOrdinates(ctr + ht / 2 + 2, ctr - ht / 2 - 2, rPanelIndex - 1, bSuppressManways: true, bReturnClones: true);
                        }

                    }
                    if (bSplices.Count > 0 )
                    {
                        for (int i = 1; i <= bSplices.Count; i++)
                        {
                            uopDeckSplice bSplc = bSplices[i - 1];
                           
                           if (workCol.AddHorizontalSpliceMD(aAssy: aAssy, aPanelIndex: rPanelIndex, aOrdinate: bSplc.Y, aSnapToMax: false, aSpliceStyle: uppSpliceStyles.Undefined) != null) cnt++;
                           
                        }
                    }
                }
            }

            if (cnt == 0)
            {
                ioBackFill = false;
            }
            else
            {

                workCol.Verify(aAssy);
            }

            if (aSubset != null)
            {
                workCol.ToList(aCollector: aSubset, bClearCollector: true);
            }
            return _rVal;
        }


        /// <summary>
        /// returns the all the splices that have an ordinate between the passed two Y values
        /// <param name="aOrd1"></param>
        /// <param name="aOrd2"></param>
        /// <param name="aPanelIndex"></param>
        /// <param name="bSuppressManways"></param>
        /// <param name="bReturnClones"></param>
        /// <returns></returns>
        public List<uopDeckSplice> GetBetweenOrdinates(double aOrd1, double aOrd2, int aPanelIndex, bool bSuppressManways = false, bool bReturnClones = false, List<uopDeckSplice> aSubset = null)
        {
            List<uopDeckSplice> _rVal = new List<uopDeckSplice>();
            int cnt = (aSubset == null) ? Count : aSubset.Count;
            if (cnt <= 0) return _rVal;

            double o1 = Math.Round(aOrd1, 1);
            double o2 = Math.Round(aOrd2, 1);
            double o3 = 0;
            mzUtils.SortTwoValues(true, ref o1, ref o2);

            for (int i = 1; i <= cnt; i++)
            {
                uopDeckSplice aMem = (aSubset == null) ? Item(i) : aSubset[i - 1];
                if (aPanelIndex <= 0 || aMem.PanelIndex == aPanelIndex)
                {
                    if (!bSuppressManways || (bSuppressManways && !aMem.SupportsManway))
                    {

                        o3 = Math.Round(aMem.Ordinate, 1);
                        if (o3 >= o1)
                        {
                            if (o3 <= o2)
                            {
                                _rVal.Add(!bReturnClones ? aMem : new uopDeckSplice(aMem));
                            }
                        }
                    }
                }
            }
            return _rVal;
        }
        public List<uopDeckSplice> ToList(bool bReturnClones = false, int aPanelID = 0, List<uopDeckSplice> aCollector = null, bool bClearCollector = false)
        {
            List<uopDeckSplice> _rVal;
            if (aCollector != null)
            {
                if (bClearCollector) aCollector.Clear();
                _rVal = aCollector;
            }
            else
            {
                _rVal = new List<uopDeckSplice>();
            }



            for (int i = 1; i <= Count; i++)
            {
                uopDeckSplice mem = Item(i);
                if (aPanelID <= 0 || mem.PanelIndex == aPanelID)
                {
                    if (bReturnClones) mem = mem.Clone();
                    _rVal.Add(mem);

                }
            }

            return _rVal;
        }

        /// <summary>
        /// returns only the vertical splices from the collection
        /// </summary>
        /// <returns></returns>
        public List<uopDeckSplice> GetVertical(bool bReturnClones = false, bool bRemove = false, int? aPanelndex = null, List<uopDeckSplice> aSubset = null)
        {


            List<uopDeckSplice> _rVal = uopDeckSplices.GetByVertical(aSubset == null ? this : aSubset, true, aPanelndex);

            if (bRemove) RemoveMembers(_rVal, aSubset == null ? this : aSubset);
            if (bReturnClones)
                _rVal = uopDeckSplices.CloneCopy(_rVal, true);

            return _rVal;
        }


        public void GetVars(out double rManHt, out double rPanelHt, out uppSpliceStyles rSpliceStyle)
        {

            rManHt = _ManwayHeight;
            rPanelHt = PanelHeight;
            rSpliceStyle = _SpliceStyle;

        }

        /// <summary>
        ///returns the centers of all the splices in the collection
        /// </summary>
        /// <param name="bGetClones"></param>
        /// <param name="aSpliceType"></param>
        /// <param name="aSubset"></param>
        /// <returns></returns>
        public uopVectors Centers(bool bGetClones, uppSpliceTypes aSpliceType = uppSpliceTypes.Undefined, List<uopDeckSplice> aSubset = null)
        {
            uopVectors _rVal = new uopVectors();

            List<uopDeckSplice> srch = aSubset == null ? this : aSubset;
            foreach (var item in srch)
            {
                if (aSpliceType != uppSpliceTypes.Undefined && item.SpliceType != aSpliceType) continue;
                _rVal.Add(bGetClones ? new uopVector(item.Center) : item.Center);
            }

            return _rVal;
        }


        public int ManwayCount(out uopVectors rManwayPoints, int aPanelIndex = 0, bool bTrayWide = true) => ManwayCount(out rManwayPoints, out _, bTrayWide, aPanelIndex);

        public int ManwayCount(bool bTrayWide = true) => ManwayCount(out _, out _, bTrayWide, 0);


        public int ManwayCount(out uopVectors rManwayPoints, out List<string> rHandles, bool bTrayWide = false, int aPanelID = 0)
        {
            int _rVal = 0;
            rManwayPoints = uopVectors.Zero;
            rHandles = new List<string>();
            bool mirrors = false;
            mdTrayAssembly assy = GetMDTrayAssembly();
            if (assy != null)
            {
                mirrors = assy.IsStandardDesign && !assy.OddDowncomers;
            }
            for (int i = 1; i <= Count; i++)
            {
                uopDeckSplice aMem = Item(i);

                if (aPanelID == 0 || aMem.PanelIndex == aPanelID)
                {
                    string hndl = aMem.ManwayHandle;
                    if (hndl !=  string.Empty)
                    {
                        if (!rHandles.Contains(hndl))
                        {
                            rHandles.Add(hndl);
                            List<uopDeckSplice> aCol = GetByManwayHandle(hndl);
                            if (aCol.Count > 0)
                            {
                                if (aCol.Count == 1)
                                {
                                    aMem = aCol[0];
                                    aMem.ReleaseManway();
                                }
                                else
                                {
                                    aMem = aCol[0];
                                    string[] sVals = hndl.Split(',');
                                    uopVectors ctrs = Centers(bGetClones: true, aSubset: aCol).Sorted(dxxSortOrders.TopToBottom);

                                    dxfVector v1 = new dxfVector(ctrs.Item(1).MidPt(ctrs.Last()));
                                    v1.Tag = sVals[0];
                                    v1.Flag = sVals.Last();
                                    v1.X = aMem.X;
                                    rManwayPoints.Add(v1);
                                    _rVal++;
                                    if (bTrayWide && mirrors)
                                    {
                                        if (Math.Round(aMem.X, 1) > 0)
                                        {
                                            _rVal++;
                                        }
                                    }
                                }
                            }
                        }
                    }

                }

            }

            return _rVal;
        }

        public uopDeckSplice GetSelected()
        {
            uopDeckSplice _rVal = null;

            for (int i = 1; i <= Count; i++)
            {
                uopDeckSplice aMem = Item(i);
                if (aMem.Selected)
                { _rVal = aMem; }
                else
                { aMem.Selected = false; }
            }
            if (_rVal == null && Count > 0) _rVal = Item(1);
            if (_rVal != null) _rVal.Selected = true;
            return _rVal;
        }

        /// <summary>
        /// returns only the horizontal splices from the collection
        /// </summary>
        /// <returns></returns>
        public List<uopDeckSplice> GetHorizontal()
        {
            List<uopDeckSplice> _rVal = new List<uopDeckSplice>();

            for (int i = 1; i <= Count; i++)
            {
                uopDeckSplice aMem = Item(i);
                if (!aMem.Vertical) _rVal.Add(aMem);

            }
            return _rVal;
        }

        public void RemoveMember(uopDeckSplice aSplice, List<uopDeckSplice> aSubset = null)
        {
            if (aSplice == null) return;
            int cnt = (aSubset == null) ? Count : aSubset.Count;

            for (int i = cnt; i >= 1; i--)
            {
                uopDeckSplice aMem = aSubset == null ? Item(i) : aSubset[i - 1];

                if (aMem.IsEqual(aSplice))
                {
                    if (aSubset == null)
                    {
                        RemoveAt(i - 1);
                    }
                    else
                    {
                        aSubset.RemoveAt(i - 1);
                    }
                    break;
                }
            }
        }



        /// <summary>
        ///returns the list of ordinates for the splices in the collection
        /// </summary>
        /// <param name="aPanelID"></param>
        /// <param name="bExcludeManwayCenters"></param>
        /// <returns></returns>
        public string OrdinateList(int aPanelID = 0, bool bExcludeManwayCenters = false, char aDelimitor = ',', List<uopDeckSplice> aSubset = null)
        {
            int cnt = (aSubset == null) ? Count : aSubset.Count;
            string _rVal = string.Empty;
            if (cnt <= 0) return _rVal;


            for (int i = 1; i <= cnt; i++)
            {
                uopDeckSplice aMem = aSubset == null ? Item(i) : aSubset[i - 1];
                if (!bExcludeManwayCenters || (bExcludeManwayCenters && aMem.SpliceType != uppSpliceTypes.SpliceManwayCenter))
                {
                    if (aPanelID <= 0 || (aPanelID > 0 && aMem.PanelIndex == aPanelID))
                    {
                        if (_rVal !=  string.Empty) _rVal += aDelimitor;
                        _rVal += $"{aMem.Ordinate:0.0###}";


                    }
                }
            }
            return _rVal;
        }
        public List<string> ManwayHandles(int aPanelIndex = 0, List<uopDeckSplice> aSubset = null)
        {
            List<uopDeckSplice> srch = new List<uopDeckSplice>();
            srch.AddRange(aSubset == null ? this : aSubset);
            int cnt = srch.Count;
            List<string> _rVal = new List<string>();
            if (srch.Count <= 0) return _rVal;



            for (int i = 1; i <= srch.Count; i++)
            {
                uopDeckSplice aMem = srch[i - 1];
                if (!string.IsNullOrWhiteSpace(aMem.ManwayHandle))
                {
                    if (aPanelIndex <= 0 || aMem.PanelIndex == aPanelIndex)
                    {


                        string hndl = aMem.ManwayHandle;
                        bool keep = _rVal.FindIndex(x => string.Compare(hndl, x, true) == 0) == -1;
                        if (keep) _rVal.Add(hndl);
                    }
                }
            }
            return _rVal;
        }

        /// <summary>
        /// the properties of the current collection
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public uopProperties CurrentProperties(uopTrayAssembly aAssy = null)
        {
            uopProperties _rVal = new uopProperties();
            aAssy ??= TrayAssembly;

            if (aAssy == null || aAssy.ProjectFamily != uppProjectFamilies.uopFamMD) return new uopProperties();

            mdTrayAssembly assy = (mdTrayAssembly)aAssy;
            colMDDeckPanels DPs = assy.DeckPanels;
           
            TPROPERTIES aProps  = new TPROPERTIES(uppPartTypes.DeckSplices);
            aProps.Add("ManwayCenters", ManwayCenters);
            aProps.Add("ManwayHeight", ManwayHeight);
            for (int pidx = 1; pidx <= DPs.Count; pidx++)
            {
                List<uopDeckSplice> sCol = GetByPanelIndex(pidx);
                if (sCol.Count > 0)
                {
                    aProps.Add($"Panel{pidx}", OrdinateList(aDelimitor: ',', aSubset: sCol)); //   sCol.SplicesOrdinates(bIncludeMFTags:true).ToDelimitedList(","));;
                }
                else
                {
                    aProps.Add($"Panel{pidx}", "");
                }
            }
            aProps.SubPart(assy);
            aProps.SetHeadings($"{assy.INIPath}.DECKSPLICES");
            return new uopProperties(aProps);

        }

        public bool HasHorizontalMembers(bool bCountManways = false) => bCountManways ? FindIndex((x) => !x.Vertical) > -1 : FindIndex((x) => !x.Vertical && string.IsNullOrWhiteSpace(x.ManwayHandle)) > -1;



        /// <summary>
        /// ensures that the splices are symmetric across both X and Y
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="bSuppressMFTags"></param>
        public void Verify(uopTrayAssembly aAssy)
        {
            try
            {
                if (Count <= 0) return;
                aAssy ??= TrayAssembly;
                if (aAssy == null || aAssy.ProjectFamily != uppProjectFamilies.uopFamMD) return;
                List<uopDeckSplice> members = ToList(true);
                uopDeckSplices.VerifySplices(members, aAssy);
                Populate(members);


            }
            catch (Exception exception)
            {
                throw exception;
            }
        }


        internal void PrintToConsole(int aPanelIndex = 0)
        {
            try
            {

                if (Count <= 0) return;

                for (int i = 1; i <= Count; i++)
                {
                    uopDeckSplice aDS = Item(i);
                    if (aPanelIndex <= 0 || aDS.PanelIndex == aPanelIndex)
                    {
                        if (string.IsNullOrEmpty(aDS.ManwayHandle))
                        {
                            System.Diagnostics.Debug.WriteLine($"{i}=,{aDS.Handle}, {aDS.Ordinate}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"{i}=,{aDS.Handle}, {aDS.Ordinate} , {aDS.ManwayHandle} ({aDS.ManTag})");
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        ///  #1the splice collection to compare this one too
        ///^returns True if the passed collection is exactly the same as this one
        /// </summary>
        /// <param name="aSpliceCol"></param>
        /// <returns></returns>
        public bool IsEqual(uopDeckSplices aSpliceCol, uopTrayAssembly aAssy = null)
        {

            if (aSpliceCol == null) return false;

            if (aSpliceCol.Count != Count) return false;

            uopProperties herProps = aSpliceCol.CurrentProperties(aAssy);
            if (herProps == null) return false;

            uopProperties props = CurrentProperties(aAssy);

            if (!props.IsEqual(herProps)) return false;


            for (int i = 1; i <= aSpliceCol.Count; i++)
            {
                uopDeckSplice aSplice = aSpliceCol.Item(i);
                uopDeckSplice bSplice = Find(s => string.Compare(s.Handle, aSplice.Handle, true) == 0);
                if (bSplice == null) return false;

                if (!aSplice.IsEqual(bSplice))
                    return false;
            }

            return true;
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
        public void ReadProperties(uopProject aProject, uopPropertyArray aFileProps, ref uopDocuments ioWarnings, double aFileVersion, string aFileSection = null, bool bIgnoreNotFound = false, uopTrayAssembly aAssy = null, uopPart aPartParameter = null, List<string> aSkipList = null, Dictionary<string, string> EqualNames = null, uopProperties aProperties = null)
        {

            try
            {
                aAssy ??= TrayAssembly;
                if (aAssy == null) throw new Exception("Tray Assembly is Undefined");
                Clear();
                ioWarnings ??= new uopDocuments();
                aFileSection = string.IsNullOrWhiteSpace(aFileSection) ? $"{aAssy.INIPath}.DECKSPLICES" : aFileSection.Trim();
                if (aFileProps == null) throw new Exception("The Passed File Property Array is Undefined");
                if (string.IsNullOrWhiteSpace(aFileSection)) throw new Exception("File Section is Undefined");

                mdTrayAssembly myassy = (mdTrayAssembly)aAssy;
                SubPart(aAssy);
                colMDDeckPanels aPanels = null;
                if (aPartParameter != null)
                {
                    if (aPartParameter.PartType != uppPartTypes.DeckSplices)
                        aPartParameter = null;
                    else
                        aPanels = (colMDDeckPanels)aPartParameter;
                }
                aPanels ??= myassy.DeckPanels;


                uopProperties myprops = aFileProps.Item(aFileSection);
                if (myprops == null || myprops.Count <= 0)
                {
                    ioWarnings?.AddWarning(aAssy, $"Deck Splice Data Not Found", $"File '{aFileProps.Name}' Does Not Contain {aFileSection.ToUpper()} Info!");
                    return;
                }

                aProject?.ReadStatus($"Reading {myassy.TrayName(true)} Deck Splices Data");

                uopDeckSplice aSplice = null;
                List<string> subVals;
                int cnt = 0;
                string sVal = string.Empty;
                string sMFTag = string.Empty;
                int iMFCnt = 0;
                SubPart(myassy);
                Reading = true;


                double mht = aFileProps.ValueD(aFileSection, "ManwayHeight", 0);
                uopVectors mpts = new uopVectors(aFileProps.ValueS(aFileSection, "ManwayCenters", ""));

                _SpliceStyle = myassy.SpliceStyle;
                uopDeckSplices.CreateManways(this, ref _SpliceStyle, myassy, aPanels, mht, mpts);
                this.ManwayHeight = mht;

                for (int i = 1; i <= aPanels.Count; i++)
                {
                    mdDeckPanel aPanel = aPanels.Item(i);
                    string ky = $"Panel{i}";
                    string ords = aFileProps.ValueS(aFileSection, ky, "");
                    if (string.IsNullOrWhiteSpace(ords)) continue;
                    List<string> sVals = mzUtils.ListValues(ords);
                    cnt = sVals.Count;
                    for (int j = 1; j <= cnt; j++)
                    {
                        sVal = sVals[j - 1];
                        int k = sVal.IndexOf(";");
                        //sMFTag = string.Empty;
                        if (k >= 0)
                        {
                            subVals = mzUtils.ListValues(sVal, ";");
                            sVal = subVals[0];
                            //sMFTag = subVals[1];
                        }
                       // if (!string.IsNullOrWhiteSpace(sMFTag)) iMFCnt += 1;

                        if (!mzUtils.IsNumeric(sVal))
                            continue;
                        if (aPanel.IsHalfMoon)
                        {
                            aSplice = aPanel.BasicDeckSplice(myassy, mzUtils.VarToDouble(sVal), SpliceStyle);
                            //aSplice.MFTag = sMFTag;
                            Add(aSplice);
                        }
                        else
                        {
                            aSplice = AddHorizontalSpliceMD(myassy, aPanel.Index, mzUtils.VarToDouble(sVal), false, true, false, true, true, aSpliceStyle: SpliceStyle);
                            if (aSplice != null)
                            {
                           
                                //aSplice.MFTag = sMFTag;

                            }
                        }

                    }
                }
                this.Verify(myassy);
                Invalid = false;
                Reading = false;



            }
            catch (Exception e)
            {
                ioWarnings?.AddWarning(aAssy, "Read Properties Error", e.Message);
            }
            finally
            {
                Reading = false;
                aProject?.ReadStatus("", 2);
            }
        }

        public uopVectors GetCenters(int? aPanelIndex = null, bool bGetClones = false) => uopDeckSplices.GetSpliceCenters(this, aPanelIndex, bGetClones);
        #endregion Methods

        #region Shared Methods

        public static uopDeckSplices ReGenerateSplices(uopTrayAssembly aAssy, uopDeckSplices aOldSplices, uppSpliceStyles aSpliceStyle = uppSpliceStyles.Undefined)
        {
            uopDeckSplices _rVal = new uopDeckSplices(aAssy);

            if (aAssy == null || aOldSplices == null) return _rVal;
            if (aAssy.ProjectFamily != uppProjectFamilies.uopFamMD) return _rVal;

            mdTrayAssembly assy = (mdTrayAssembly)aAssy;
            uopDeckSplice aDS = null;
            colMDDeckPanels aDPs = assy.DeckPanels;
            mdDeckPanel aDP = null;

            uppSpliceStyles sstyle = (aSpliceStyle == uppSpliceStyles.Undefined) ? aAssy.SpliceStyle : aSpliceStyle;
            bool bSplit = false;
            double vSplit = 0;

            uopVectors fpts = null;



            aOldSplices.ManwayCount(out uopVectors mpts, bTrayWide: false);
            _rVal.SpliceStyle = sstyle;
            _rVal.SubPart(aAssy);
            _rVal.ManwayHeight = aOldSplices.ManwayHeight;
            double mht = _rVal.ManwayHeight;

            for (int i = 1; i <= mpts.Count; i++)
            {
                _rVal.AddManway(aAssy, mpts.Item(i).Handle, fpts, _rVal.SpliceStyle, ref mht, out int PID, out string SERR);
            }
            _rVal.ManwayHeight = mht;

            for (int i = 1; i <= aOldSplices.Count; i++)
            {
                aDS = aOldSplices.Item(i);
                if (!aDS.Vertical)
                {
                    if (!aDS.SupportsManway)
                    {
                        aDP = aDPs.Item(aDS.PanelIndex);
                        if (aDP != null) _rVal.AddHorizontalSpliceMD(assy, aDP.Index, aDS.Ordinate, false, true, false, false);

                    }
                }
                else
                {
                    bSplit = true;
                    if (vSplit <= 0) vSplit = aDS.X;

                }
            }

            if (bSplit)
            {
                _rVal.AddVerticalSplices(aAssy, aSpliceStyle: sstyle);
            }
            _rVal.Invalid = false;
            _rVal.Verify(aAssy);
            return _rVal;
        }

        /// <summary>
        /// returns only the vertical splices from the collection
        /// </summary>
        /// <returns></returns>
        public static List<uopDeckSplice> GetByVertical(IEnumerable<uopDeckSplice> aSplices, bool aVerticalVal, int? aPanelIndex = null, bool bReturnClones = false)
        {

            List<uopDeckSplice> _rVal = new List<uopDeckSplice>();
            if (aSplices == null || aSplices.Count() <= 0) return _rVal;

            foreach (var splice in aSplices)
            {
                if (!aPanelIndex.HasValue || (aPanelIndex.HasValue && splice.PanelIndex == aPanelIndex.Value))
                {
                    if (splice.Vertical == aVerticalVal)
                        _rVal.Add(!bReturnClones ? splice : new uopDeckSplice(splice));
                }
            }

            return _rVal;
        }

        public static List<uopDeckSplice> CloneCopy(IEnumerable<uopDeckSplice> aSplices, bool bClone)
        {
            List<uopDeckSplice> _rVal = new List<uopDeckSplice>();
            if (aSplices == null || aSplices.Count() <= 0) return _rVal;
            foreach (var splice in aSplices)
            {
                _rVal.Add(!bClone ? splice : new uopDeckSplice(splice));
            }
            return _rVal;
        }
        public static int RemoveSplices(List<uopDeckSplice> aSplices, List<uopDeckSplice> aRemoveSplices)
        {
            if (aSplices == null || aSplices.Count <= 0) return 0;
            if (aRemoveSplices == null || aRemoveSplices.Count <= 0) return 0;
            int _rVal = 0;
            foreach (var rsplice in aRemoveSplices)
            {
                for (int i = aSplices.Count - 1; i >= 0; i--)
                {
                    if (rsplice.IsEqual(aSplices[i]))
                    {
                        _rVal++;
                        aSplices.RemoveAt(i);
                    }
                }


            }
            return _rVal;
        }


        /// <summary>
        /// ensures that the splices are symmetric across both X and Y
        /// </summary>
        /// <param name="aSplices"></param>
        /// <param name="aAssy"></param>
        public static void VerifySplices(List<uopDeckSplice> aSplices, uopTrayAssembly aAssy)
        {

            List<uopDeckSplice> _rVal = new List<uopDeckSplice>();
            try
            {
                if (aSplices == null || aAssy == null || aAssy.ProjectFamily != uppProjectFamilies.uopFamMD) return;

                if (aSplices.Count <= 0) return;
                mdTrayAssembly assy = (mdTrayAssembly)aAssy;

                uopDeckSplices.Sort(aSplices, aAssignIndices: true);

                colMDDeckPanels DPs = assy.DeckPanels;
                uppSpliceStyles aStyle = aAssy.SpliceStyle;
                List<uopDeckSplice> members = aSplices;

                List<uopDeckSplice> panelmembers;
                List<Tuple<double, uopDeckSplice>> sortlist;
                uopDeckSplice splicebefore;
                uopDeckSplice spliceafter;
                uopDeckSplice splice;
                bool keep;
                if (aStyle < 0) aStyle = aAssy.SpliceStyle;

                foreach (mdDeckPanel panel in DPs)
                {

                    panelmembers = members.FindAll(x => x.PanelIndex == panel.Index);
                    sortlist = new List<Tuple<double, uopDeckSplice>>();
                    //filter for bounds
                    for (int i = 1; i <= panelmembers.Count; i++)
                    {

                        splice = panelmembers[i - 1];
              
                        //save only valid members

                        keep = true;
                        if (panel.IsHalfMoon)
                        {
                            if (splice.MinOrdinate.HasValue && splice.MaxOrdinate.HasValue)
                            {
                                if (Math.Round(Math.Abs(splice.X) - splice.MinOrdinate.Value, 1) < 0 || Math.Round(splice.MaxOrdinate.Value - Math.Abs(splice.X), 1) < 0)
                                    keep = false;
                            }

                        }
                        else
                        {
                            if (splice.MaxOrdinate.HasValue)
                            {
                                if (Math.Round(splice.MaxOrdinate.Value - Math.Abs(splice.Y), 1) < 0) keep = false;
                            }


                        }
                        if (keep) sortlist.Add(new Tuple<double, uopDeckSplice>(splice.Ordinate, splice));

                    }


                    //sort high to low for field panels and low to high for moon panels                    
                    if (panel.IsHalfMoon && panel.X >0)
                        if (sortlist.Count > 1) sortlist = sortlist.OrderBy(t => t.Item1).ToList();
                        else
                        if (sortlist.Count > 1) sortlist = sortlist.OrderByDescending(t => t.Item1).ToList();
                    


                    //set splice indices and filter by proximity
                    panelmembers = new List<uopDeckSplice>();
                    for (int i = 1; i <= sortlist.Count; i++)
                    {
                        splice = sortlist[i - 1].Item2;
                        keep = true;
                        splicebefore = i <= 1 ? null : sortlist[i - 2].Item2;
                        spliceafter = i >= sortlist.Count ? null : sortlist[i].Item2;
                        if (panel.IsHalfMoon)
                        {
                            if (splicebefore != null && Math.Abs(splicebefore.Ordinate - splice.Ordinate) <= 2) keep = false;
                        }
                        else
                        {
                            if (splicebefore != null && Math.Abs(splicebefore.Ordinate - splice.Ordinate) <= 2) keep = splice.SupportsManway;
                        }


                        if (keep)
                        {
                            splice.SpliceIndex = panelmembers.Count + 1;
                            panelmembers.Add(splice);
                        }
                    }



                    _rVal.AddRange(panelmembers);

                }


            }
            catch (Exception exception)
            {
                throw exception;
            }
            finally
            {
                aSplices.Clear();
                foreach (uopDeckSplice item in _rVal)
                {
                    aSplices.Add(item);
                }
            }


        }

        public List<uopDeckSplice> RemoveVerticalSplices(List<uopDeckSplice> aSubset = null)
        {
            int cnt = (aSubset == null) ? Count : aSubset.Count;
            List<uopDeckSplice> _rVal = new List<uopDeckSplice>();
            for (int i = cnt; i >= 1; i--)
            {
                uopDeckSplice aMem = (aSubset == null) ? Item(i) : aSubset[i - 1];

                if (aMem.Vertical) _rVal.Add(aMem);
            }

            RemoveMembers(_rVal, aSubset);

            return _rVal;
        }


        /// <summary>
        ///#1the X or Y Ordinate to search for
        ///#3flag to return verical splices
        ///#4flag to return the members at the negative ordinate to
        ///^returns the splices with and X or Y ordinate matching the passed value
        /// </summary>
        /// <param name="aXorY"></param>
        /// <param name="bSuppressManways"></param>
        /// <param name="bVerticals"></param>
        /// <param name="bReturnMirrors"></param>
        /// <param name="aPanelIndex"></param>
        /// <param name="rIndexes"></param>
        /// <param name="rPanelIDS"></param>
        /// <returns></returns>
        public List<uopDeckSplice> GetAtOrdinate(double aXorY, bool bSuppressManways = false, bool bVerticals = false, bool bReturnMirrors = false, int aPanelIndex = 0, List<uopDeckSplice> aSubset = null, bool bJustOne = false, bool bRemove = false)
        {
            List<uopDeckSplice> _rVal = new List<uopDeckSplice>();
            List<uopDeckSplice> srch = aSubset == null ? this : aSubset;
            //int cnt = (aSubset == null) ? Count : aSubset.Count;

            if (srch.Count <= 0) return _rVal;

            if (bJustOne) bReturnMirrors = false;


            aXorY = Math.Round(aXorY, 3);

            if (aXorY == 0) bReturnMirrors = false;
            srch = bReturnMirrors ? srch.FindAll(x => Math.Abs(Math.Round(x.Ordinate, 3)) == Math.Abs(aXorY)) : srch.FindAll(x => Math.Round(x.Ordinate, 3) == aXorY);
            srch.RemoveAll(x => x.Vertical != bVerticals);
            if (aPanelIndex > 0) srch.RemoveAll(x => x.PanelIndex != aPanelIndex);
            if (bSuppressManways) srch.RemoveAll(x => !string.IsNullOrWhiteSpace(x.ManwayHandle));

            if (bJustOne && srch.Count > 0)
                _rVal.Add(srch[0]);
            else
                _rVal.AddRange(srch);



            if (bRemove && _rVal.Count > 0)
                RemoveMembers(_rVal, aSubset);


            return _rVal;
        }

        /// <summary>
        /// the panel to add the splice to
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aSpliceHandle"></param>
        /// <param name="aOrdinate"></param>
        /// <param name="bMoveMirror"></param>
        /// <returns></returns>
        public bool MoveSpliceTo(mdTrayAssembly aAssy, string aSpliceHandle, double aOrdinate, bool bMoveMirror = true)
        {
            aAssy ??= GetMDTrayAssembly(aAssy);

            if (Count <= 0) return false;

            uopDeckSplice aDS = null;
            uopDeckSplice bDS = null;
            double ordReq = 0;
            int pid = 0;
            double dY = 0;
            double dX = 0;

            double memOrd = 0;
            bool _rVal = false;

            aDS = Find(x => x.Handle == aSpliceHandle);
            if (aDS == null) return false;

            memOrd = aDS.Ordinate;
            ordReq = aOrdinate;
            pid = aDS.PanelIndex;
            if (aDS.Vertical)
            {
                if (aOrdinate > aDS.MaxOrdinate) aOrdinate = aDS.MaxOrd;

                if (aOrdinate < aDS.MinOrdinate) aOrdinate = aDS.MinOrd;

                dX = aOrdinate - memOrd;
                _rVal = dX != 0;
                if (_rVal)
                {
                    aDS.Move(memOrd + dX - aDS.Ordinate);

                }
            }
            else
            {
                if (Math.Abs(aOrdinate) > aDS.MaxOrdinate)
                {
                    aOrdinate = (aOrdinate < 0) ? -aDS.MaxOrd : aDS.MaxOrd;

                }
                if (!LiesWithinManway(pid, aOrdinate)) dY = aOrdinate - memOrd;

                _rVal = dY != 0;
                if (_rVal)
                {
                    RemoveMember(aDS);
                    //remove any splices in the way
                    if (RemoveNearbySplices(aDS.PanelIndex, aOrdinate))
                    {
                        dY = 0;
                    }
                    aDS.Move(memOrd + dY - aDS.Ordinate);
                    Add(aDS);
                    if (bMoveMirror)
                    {

                        bDS = GetByOrdinate(-memOrd, bVerticals: false, aPanelIndex: pid);
                        if (bDS != null)
                        {
                            dY = -dY;
                            if (bDS != aDS)
                            {
                                RemoveMember(bDS);
                                RemoveNearbySplices(bDS.PanelIndex, -aOrdinate);
                                bDS.Move(dY);
                                Add(bDS);
                            }
                        }
                        else
                        {
                            if (memOrd == 0)

                            {
                                bDS = AddHorizontalSpliceMD(aAssy: aAssy, aPanelIndex: pid, aOrdinate: -aOrdinate, aSnapToMax: false, bAvoidManways: true, aSpliceStyle: aDS.SpliceStyle);
                            }
                        }
                    }
                }
            }
            //to sort
            Sort();

            return _rVal;


        }


        /// <summary>
        ///#1the X or Y Ordinate to search for
        ///#2flag to return verical splices
        ///#3a subset of splices to search
        ///#4rteurns all the splices at the passed ordinate
        ///^returns the splices with and X or Y ordinate matching the passed value
        /// </summary>
        /// <param name="aXorY"></param>
        /// <param name="bVerticals"></param>
        /// <param name="aPanelIndex"></param>
        /// <returns></returns>
        public uopDeckSplice GetByOrdinate(double aXorY, bool bVerticals, int aPanelIndex = 0, List<uopDeckSplice> aSubset = null)
        {
            List<uopDeckSplice> rAllAtOrd = GetAtOrdinate(aXorY, bSuppressManways: false, bVerticals, bReturnMirrors: false, aPanelIndex, aSubset, bJustOne: true);
            return (rAllAtOrd.Count > 0) ? rAllAtOrd[0] : null;


        }

        public bool DeleteSplice(string aSpliceHandle, bool bDeleteMirror = false, bool bDeleteAllAtOrdinate = false, List<uopDeckSplice> aSubset = null)
        {

            if (string.IsNullOrWhiteSpace(aSpliceHandle)) return false;

            // Dim aDS As uopDeckSplice
            // Dim bDS As uopDeckSplice
            // Dim bSplices As Collection
            // Dim mCtrs As colDXFVectors
            // Dim v1 As dxfVector
            // Dim ord As Single

            uopDeckSplice aDS = GetByHandle(aSpliceHandle, aSubset: aSubset);
            List<uopDeckSplice> bSplices;

            if (aDS == null) return false;
            double ord = aDS.Ordinate;

            if (aDS.Vertical)
            {
                RemoveVerticalSplices(aSubset: aSubset);
                return true;
            }

            if (!string.IsNullOrWhiteSpace(aDS.ManwayHandle))
            { RemoveManway(aDS.ManwayHandle, GetMDTrayAssembly(), aSubset: aSubset); } //    mdSplicing.SplicesRemoveManway(this, aDS.ManwayHandle, null, out int PID, out bool BFILL); }
            else
            { RemoveMember(aDS, aSubset: aSubset); }


            if (bDeleteAllAtOrdinate)
            {
                bSplices = GetAtOrdinate(ord, aSubset: aSubset);
                if (bSplices.Count > 0) RemoveMembers(bSplices, aSubset: aSubset);
            }

            RemoveMember(aDS, aSubset: aSubset);



            if (bDeleteMirror)
            {
                GetAtOrdinate(-ord, bSuppressManways: false, bVerticals: false, aPanelIndex: aDS.PanelIndex, bRemove: true, aSubset: aSubset);

                if (bDeleteAllAtOrdinate)
                {
                    GetAtOrdinate(-ord, bSuppressManways: false, bVerticals: false, aPanelIndex: 0, bRemove: true, aSubset: aSubset);

                }
                else
                {
                    GetAtOrdinate(-ord, bSuppressManways: false, bVerticals: false, aPanelIndex: aDS.PanelIndex, bRemove: true, aSubset: aSubset);
                }

            }

            return true;
        }


        public List<uopDeckSplice> GetRelatives(uopDeckSplice aSplice, bool bReturnParent = true, int aPanelID = 0, bool bReturnMirrors = true)
        {
            List<uopDeckSplice> _rVal = new List<uopDeckSplice>();
            if (aSplice == null) return _rVal;

            uopDeckSplice parent = GetByHandle(aSplice.Handle);
            if (parent == null) parent = aSplice;
            if (parent != null && bReturnParent) _rVal.Add(parent);

            if (parent.Vertical) return _rVal;

            if (parent.SupportsManway)
            {
                string mnhndl = parent.ManwayHandle;
                foreach (var mem in this)
                {
                    if (mem != parent)
                    {
                        if (string.Compare(mnhndl, mem.ManwayHandle, ignoreCase: true) == 0)
                        {
                            _rVal.Add(mem);
                        }

                    }

                }
                return _rVal;
            }

            if (!MDDesignFamily.IsStandardDesignFamily()) bReturnMirrors = false;
            double ord = parent.Ordinate;
            bool pos = ord >= 0;
            foreach (var mem in this)
            {
                if (!mem.SupportsManway && !mem.Vertical && mem != parent)
                {

                    if (Math.Round(Math.Abs(mem.Ordinate) - Math.Abs(ord), 3) == 0)
                    {
                        if (bReturnMirrors)
                        {
                            if (aPanelID <= 0 | (aPanelID > 0 && mem.PanelIndex == aPanelID)) _rVal.Add(mem);
                        }
                        else
                        {
                            if ((pos && mem.Ordinate >= 0) || (!pos && mem.Ordinate < 0))
                            {
                                if (aPanelID <= 0 | (aPanelID > 0 && mem.PanelIndex == aPanelID)) _rVal.Add(mem);
                            }


                        }


                    }
                }
            }

            return _rVal;

        }

        public uopDeckSplice GetByHandle(string aHandle, List<uopDeckSplice> aSubset = null) => aSubset == null ? Find(x => x.Handle == aHandle) : aSubset.Find(x => x.Handle == aHandle);

        /// <summary>
        /// creates the manways based on the current manway centers collection
        /// </summary>
        /// <param name="aSplices"></param>
        /// <param name="aSpliceStyle"></param>
        /// <param name="aAssy"></param>
        /// <param name="aDeckPanels"></param>
        /// <param name="aManwayHeight"></param>
        /// <param name="aManCenters"></param>
        /// <param name="aPanelIndex"></param>
        /// <param name="aFreeCenters"></param>
        internal static void CreateManways(uopDeckSplices aSplices, ref uppSpliceStyles aSpliceStyle, mdTrayAssembly aAssy, colMDDeckPanels aDeckPanels, double aManwayHeight, uopVectors aManCenters, int aPanelIndex = 0, uopVectors aFreeCenters = null)
        {
            if (aAssy == null || aSplices == null || aManCenters == null || aDeckPanels == null) return;
            if (aManCenters.Count <= 0) return;

            double ManHt = Math.Abs(aManwayHeight);
            string hndls = string.Empty;
            aSplices.GetManwaySplices(aPanelIndex, bRemove: true);  // remove existing manway splices
            if (aSpliceStyle < 0) aSpliceStyle = aAssy.DesignOptions.SpliceStyle;
            if (aFreeCenters == null || aFreeCenters.Count <= 0)
                aFreeCenters = aAssy.DowncomerData.FreeCenters();

            if (ManHt <= 0) ManHt = aSplices.GetManwayHt();

            ManHt = mzUtils.LimitedValue(ManHt, 12, 2 * aAssy.DowncomerSpacing, mdUtils.IdealManwayHeight(aAssy,aSpliceStyle));

            if (aPanelIndex <= 0)
            {
                foreach (uopVector vector in aManCenters)
                {
                    uopVector v2 = aFreeCenters.Nearest(vector);
                    if (mzUtils.ListAdd(ref hndls, v2.Handle, "", false, "#"))
                    {
                        mdDeckPanel aDP = aDeckPanels.Item(mzUtils.VarToInteger(v2.Tag));
                        if (aDP != null)
                        {
                            aSplices.AddManway(aAssy, v2.Handle, aFreeCenters, aSpliceStyle, ref ManHt, out int PID, out string SERR);
                        }
                    }
                }


            }
            else
            {
                uopVectors bCtrs = aFreeCenters.GetByTag(Convert.ToString(aPanelIndex));
                if (bCtrs.Count > 0)
                {
                    mdDeckPanel aDP = aDeckPanels.Item(aPanelIndex);
                    if (aDP != null)
                    {
                        if (!aDP.IsHalfMoon)
                        {
                            foreach (var vector in bCtrs)
                            {
                                aSplices.AddManway(aAssy, vector.Handle, aFreeCenters, aSpliceStyle, ref ManHt, out _, out _);
                            }

                        }
                    }
                }
            }
        }


        public static List<int> GetPanelIDS(List<uopDeckSplice> aSplices, bool bHighToLow = false)
        {
            List<int> _rVal = new List<int>();
            if (aSplices == null) return _rVal;
            foreach (uopDeckSplice item in aSplices)
            {
                if (item != null && !_rVal.Contains(item.PanelIndex)) _rVal.Add(item.PanelIndex);
            }
            _rVal.Sort();
            if (bHighToLow) _rVal.Reverse();
            return _rVal;
        }
        /// <summary>
        /// sorts the splices in the collection in the requested order
        /// </summary>
        /// <param name="aSplices"></param>
        /// <param name="aAssignIndices">the reference point to use for sorting clockwise or counter-clockwise or nearest to farthest </param>
        public static void Sort(List<uopDeckSplice> aSplices, bool aAssignIndices = true)
        {
            if (aSplices == null) return;

            List<int> pids = GetPanelIDS(aSplices);

            List<uopDeckSplice> keep = new List<uopDeckSplice>();

            foreach (var pid in pids)
            {
                List<uopDeckSplice> panel = aSplices.FindAll((x) => x!=null && x.PanelIndex == pid);
                panel = uopDeckSplices.SortSet(panel, bLowToHigh: pid == 1);
                foreach (var splice in panel)
                {
                    
                    if (aAssignIndices) splice.SpliceIndex = keep.Count + 1;
                    keep.Add(splice);
                }
            }

            aSplices.Clear();
            aSplices.AddRange(keep);


        }
        /// <summary>
        /// sorts the splices in the collection in the requested order
        /// </summary>
        /// <param name="aSplices"></param>
        /// <param name="aAssignIndices">the reference point to use for sorting clockwise or counter-clockwise or nearest to farthest </param>
        public static List<uopDeckSplice> SortSet(List<uopDeckSplice> aSplices, bool bLowToHigh)
        {
            if (aSplices == null) return null;

            List<uopDeckSplice> keep = new List<uopDeckSplice>();
            List<Tuple<double, uopDeckSplice>> sort;

            uopDeckSplice splice;

            sort = new List<Tuple<double, uopDeckSplice>>();
            foreach (var splc in aSplices)
            {
                if(splc != null)
                    sort.Add(new Tuple<double, uopDeckSplice>(splc.Ordinate, splc));

            }

            if (!bLowToHigh)
            {
                sort = sort.OrderByDescending(t => t.Item1).ToList();
            }
            else
            {
                sort = sort.OrderBy(t => t.Item1).ToList();
            }

            foreach (Tuple<double, uopDeckSplice> tpl in sort)
            {
                splice = tpl.Item2;

                if(splice != null)
                keep.Add(splice);

            }

            return keep;
        }

        public static uopVectors GetSpliceCenters(List<uopDeckSplice> aSplices, int? aPanelIndex = null, bool bGetClones = false)
        {
            uopVectors _rVal = uopVectors.Zero;
            if (aSplices == null || aSplices.Count <= 0) return _rVal;
            List<uopDeckSplice> srch = aSplices;
            if (aPanelIndex.HasValue) srch = srch.FindAll(x => x.PanelIndex == aPanelIndex.Value);
            foreach (var splice in srch)
            {
                _rVal.Add(bGetClones ? new uopVector(splice.Center) : splice.Center);
            }
            return _rVal;
        }

        /// <summary>
        /// main entry point to build the polygon parts for the passed splice based on it's gender, orientation and splice type
        /// </summary>
        /// <remarks>
        /// the splice polygons are used to build the parent sections perimeter polygon (plan view) to include the splice details
        /// </remarks>
        /// <param name="aSplice"></param>
        /// <param name="aBaseShape"></param>
        /// <param name="bSimple"></param>
        /// <returns></returns>

        public static dxePolygon GetSplicePolygon(uopDeckSplice aSplice, uopSectionShape aBaseShape = null, bool bSimple = false)
        {
            if (aSplice == null) return null;
            uopSectionShape baseshape = aBaseShape == null ? aSplice.Section : aBaseShape;
            if (baseshape == null) return null;
            dxePolygon _rVal = null;
            if (aSplice.SpliceType == uppSpliceTypes.SpliceWithTabs)
            {
                //if (aSplice.SupportsManway)
                //{
                //    Console.WriteLine(aSplice.ManTag);
                //}
                _rVal = aSplice.Female ? xSpliceFemale_SlotAndTab(baseshape, aSplice, bSimple) : xSpliceMale_SlotAndTab(baseshape, aSplice, bSimple);
            }
            else if (aSplice.SpliceType == uppSpliceTypes.SpliceManwayCenter)
            {


                _rVal = xSpliceManwayCenter(baseshape, aSplice, bSimple);

            }
            else if (aSplice.SpliceType == uppSpliceTypes.SpliceWithAngle)
            {
                _rVal = xSpliceFlange(baseshape,aSplice, bSimple);
            }

                return _rVal;
        }
        private static dxePolygon xSpliceManwayCenter(uopSectionShape aSection, uopDeckSplice aSplice, bool bSimple = false)
        {
            dxePolygon _rVal = new dxePolygon() { Closed = false };
      
            try
            {

                double gap = aSplice.SpliceType.GapValue();
              
                uopFlangeLine flngLn = aSplice.FlangeLine;
              
                uopLine baseline = aSplice.BaseLine(aSection, out dxfPlane plane);
              
                double radswap = baseline.sp.X < baseline.ep.X ? -1 : 1;
                double rad = mdGlobals.DeckManwayAngleSlotRadius;
                double y3 = mdGlobals.DeckManwayAngleSlotLength;
                double y2 =y3 - rad;
                double filletrad = mdGlobals.DeckManwayCornerRadius;
                //baseline.Rectify(bDoX: true, bLeftToRight);
                List<double> tOrds = flngLn.FastenerOrdinates;
                tOrds.Sort();
                if (baseline.sp.X >baseline.ep.X)
                    tOrds.Reverse();

                colDXFVectors verts = colDXFVectors.Zero;
                radswap = -1;
                if (!bSimple)
                {
                    verts.Add(plane, 0, filletrad, aVertexRadius: radswap * -filletrad);
                    verts.Add(plane, filletrad, aVertexRadius: 0);
                }
                else
                {
                    verts.Add(plane, 0, 0);
                }

                foreach (var ord in tOrds)
                {
                    plane.X = ord;
                    if (!bSimple)
                    {
                        verts.Add(plane, -rad, 0, aVertexRadius: 0, aTag: "MANGLE", aFlag: "NODIM");
                        verts.Add(plane, -rad, y2, aVertexRadius: radswap * rad, aTag: "MANGLE", aFlag: "NODIM");
                        verts.Add(plane, 0, y3, aVertexRadius: radswap * rad, aTag: "MANGLE", aFlag: "NODIM");
                        verts.Add(plane, rad, y2, aVertexRadius: 0, aTag: "MANGLE", aFlag: "NODIM");
                        verts.Add(plane, rad, 0, aVertexRadius: 0, aTag: "MANGLE", aFlag: "NODIM");
                    }
                    else
                    {
                        verts.Add(plane, -rad, 0, aVertexRadius: 0, aTag: "MANGLE", aFlag: "NODIM");
                        verts.Add(plane, -rad, y3, aVertexRadius: 0, aTag: "MANGLE", aFlag: "NODIM");
                        verts.Add(plane, rad, y3, aVertexRadius: 0, aTag: "MANGLE", aFlag: "NODIM");
                        verts.Add(plane, rad, 0, aVertexRadius: 0, aTag: "MANGLE", aFlag: "NODIM");
                    }
                }
                plane.X = baseline.sp.X;

                if (!bSimple)
                {
                    verts.Add(plane, baseline.Length - filletrad, 0, aVertexRadius: radswap * -filletrad);
                    verts.Add(plane, baseline.Length, filletrad, aVertexRadius: 0);
                }
                else
                {
                    verts.Add(plane, baseline.Length, 0);
                }


                    _rVal.Vertices = verts;
                _rVal.InsertionPt = new dxfVector(aSplice.Center);
                _rVal.Value = baseline.sp.X > baseline.ep.X ? 0 : 1;


                return _rVal;
            }
            catch (Exception)
            {
                return _rVal;
            }
        }


        private static dxePolygon xSpliceFlange(uopSectionShape aSection, uopDeckSplice aSplice, bool bSimple = false)
        {
            dxePolygon _rVal = new dxePolygon() { Closed = false, InsertionPt = new dxfVector(aSplice.Center) };
            uopRectangle limits = aSection.Limits();
            try
            {

               // double gap = aSplice.GapValue(aSplice.SupportsManway);
                 uopLine baseline = aSplice.BaseLine(aSection, out dxfPlane plane);
                //uopFlangeLine flngLn = aSplice.FlangeLine;
                dxfVector startpt1 = new dxfVector(baseline.sp).WithRespectToPlane(plane);
                dxfVector endpt1 = new dxfVector(baseline.ep).WithRespectToPlane(plane);

                //if (flngLn.Direction != baseline.Direction) flngLn.Invert();
                //dxfVector startpt2 = null;//  new dxfVector(flngLn.sp).WithRespectToPlane(plane);
                //dxfVector endpt2 = null; // new dxfVector(flngLn.ep).WithRespectToPlane(plane);
               

                colDXFVectors verts = colDXFVectors.Zero;
                if(bSimple || aSplice.Vertical || !aSplice.SupportsManway || !aSection.IsManway )
                {
                    verts.Add(plane, startpt1.X, startpt1.Y);
                    verts.Add(plane, endpt1.X, endpt1.Y);  // the baseline start , aVertexRadius: radswap * -filletrad);

                }
                else
                {
                    bool left2right = !aSplice.IsTop;

                    double rad = mdGlobals.ManwayFilletRadius;
                    verts.Add(plane, startpt1.X, startpt1.Y-rad,aVertexRadius:rad);
                    verts.Add(plane, startpt1.X + rad, startpt1.Y );
                    verts.Add(plane, endpt1.X - rad, endpt1.Y,aVertexRadius:rad);
                    verts.Add(plane, endpt1.X, endpt1.Y -rad);
                }

                    _rVal.Vertices.AddRange(verts);
              
                _rVal.Value = baseline.sp.X > baseline.ep.X ? 0 : 1;


                return _rVal;
            }
            catch (Exception)
            {
                return _rVal;
            }
        }

        private static dxePolygon xSpliceMale_SlotAndTab(uopSectionShape aSection, uopDeckSplice aSplice, bool bSimple = false)
        {
            dxePolygon _rVal = new dxePolygon() { Closed = false, InsertionPt = new dxfVector(aSplice.Center) };
            
            try
            {
                
                uopLine baseline = aSplice.BaseLine(aSection, out dxfPlane plane); // the planes y direction indicates the tab direction
                double deckrad = aSection.DeckRadius;

                _rVal.InsertionPt.SetCoordinates(aSplice.X, aSplice.Y);
                

                uopFlangeLine flngLn = aSplice.FlangeLine;
                dxfVector startpt1 = new dxfVector(baseline.sp).WithRespectToPlane(plane);
                dxfVector endpt1 = new dxfVector(baseline.ep).WithRespectToPlane(plane);

                if (flngLn.Direction != baseline.Direction) flngLn.Invert();
                dxfVector startpt2 = null;//  new dxfVector(flngLn.sp).WithRespectToPlane(plane);
                dxfVector endpt2 = null; // new dxfVector(flngLn.ep).WithRespectToPlane(plane);
                bool top2bottom = false;
                bool left2right = false;
                List<double> tOrds = flngLn.FastenerOrdinates;
                colDXFVectors baseverts = null;
                bool vertical = aSplice.Vertical;
                List<dxfEntity> bendlines = new List<dxfEntity> ();
                colDXFVectors pgverts = colDXFVectors.Zero;
                double clip = mdGlobals.MoonClipLength;

                //====== VERTICAL ======
                if (vertical)
                {
                    top2bottom = !aSplice.IsTop;

                    tOrds.Sort();
                    if ((aSection.X >= 0 && !aSplice.IsTop) || (aSection.X < 0 && aSplice.IsTop))
                        tOrds.Reverse();

                   
                    startpt1 = plane.Vector(startpt1.X + clip, startpt1.Y, aVertexRadius: 0, aTag: "TAB_LINE", aFlag: "NODIM");
                    startpt2 = new dxfVector(Math.Sqrt(Math.Pow(deckrad, 2) - Math.Pow(startpt1.Y, 2)), startpt1.Y, aTag: "TAB_LINE", aFlag: "NODIM");
                       
                    startpt2 = pgverts.Add(startpt2, aVertexRadius: 0);
                    startpt1 = pgverts.Add(startpt1, aVertexRadius: !top2bottom ? deckrad : 0);

              
                    _rVal.Value = top2bottom ? 0 : 1;
                }
                else //====== HORIZONTAL ========
                {

                  
                    left2right = !aSplice.IsTop;
                    tOrds.Sort();
                    if (!left2right) tOrds.Reverse();

                    if(  aSection.IsManway && !bSimple)
                    {
                        double filletrad = mdGlobals.ManwayFilletRadius;
                        pgverts.Add(plane, 0, -filletrad, aVertexRadius: filletrad);
                        pgverts.Add(plane, filletrad,0, aVertexRadius: 0);
                    }
                    else
                    {
                        pgverts.Add(plane, 0, 0, aTag: "TAB_LINE", aFlag: "NODIM", aVertexRadius: 0);
                    }
                    _rVal.Value = left2right ? 0 : 1;

                }

                //add the tabs
                foreach (var ord in tOrds)
                {
                    if (vertical) plane.Y = ord; else  plane.X = ord;
                    xxAddTabVertices(pgverts, aSplice, plane, ref baseverts, ref bendlines, bSimple);
                }

                plane.SetCoordinates(baseline.sp.X, baseline.sp.Y);
                if (!vertical)
                {
                    if (aSection.IsManway && !bSimple)
                    {
                        double filletrad = mdGlobals.ManwayFilletRadius;
                        pgverts.Add(plane, baseline.Length -filletrad, 0, aVertexRadius: filletrad);
                        pgverts.Add(plane, baseline.Length, -filletrad, aVertexRadius: 0);
                    }
                    else
                    {
                        pgverts.Add(plane, baseline.Length, 0, aTag: "TAB_LINE", aFlag: "NODIM", aVertexRadius: 0);
                    }
                }
                else
                {
                    endpt1 = plane.Vector(endpt1.X - clip, endpt1.Y, aVertexRadius: 0, aTag: "TAB_LINE", aFlag: "NODIM");
                    endpt2 = new dxfVector(Math.Sqrt(Math.Pow(deckrad, 2) - Math.Pow(startpt1.Y, 2)), endpt1.Y, aTag: "TAB_LINE", aFlag: "NODIM");

                    endpt1 = pgverts.Add(endpt1, aVertexRadius: 0);
                    endpt2 = pgverts.Add(endpt2, aVertexRadius: top2bottom ? deckrad : 0);
                }



                    _rVal.Vertices.AddRange(pgverts); //, bAddClones:false);
                if(!bSimple)  _rVal.AdditionalSegments.AddRange(bendlines);
                
                _rVal.InsertionPt = new dxfVector(aSplice.Center);

                return _rVal;
            }
            catch (Exception)
            {
                return _rVal;
            }
        }
        private static dxePolygon xSpliceFemale_SlotAndTab(uopSectionShape aSection, uopDeckSplice aSplice, bool bSimple = false)
        {
            dxePolygon _rVal = new dxePolygon() { Closed = false, InsertionPt = new dxfVector(aSplice.Center) };

            try
            {

              
                double thk = aSection.Thickness;
                double gap = aSplice.GapValue(aSection.IsManway);
                colDXFVectors verts = colDXFVectors.Zero;
                uopFlangeLine flngLn = aSplice.FlangeLine;
                uopLine baseline = aSplice.BaseLine(aSection, out dxfPlane plane); // the planes y direction indicates the tab direction
                double radswap = 1;
                double deckrad = aSection.DeckRadius;
                uopVectors tSlotCtrs = uopVectors.Zero;
                dxfVector startpt1 = new dxfVector( baseline.sp).WithRespectToPlane(plane);
                dxfVector endpt1 = new dxfVector(baseline.ep).WithRespectToPlane(plane);
               
                if (flngLn.Direction != baseline.Direction) flngLn.Invert();
                dxfVector startpt2 = new dxfVector(flngLn.sp).WithRespectToPlane(plane);
                dxfVector endpt2 = new dxfVector(flngLn.ep).WithRespectToPlane(plane);
                bool top2bottom = false;
                bool left2right = false;
                double relieferad = 0.125;
                double lastrad = 0;
                dxfVector tip1 = null;
                dxfVector tip2 = null;

                

                //================ VERTICAL ================================
                if (aSplice.Vertical)
                {

                    top2bottom = !aSplice.IsTop;
                    if (top2bottom && plane.XDirection.Y < 0) radswap = -1;
                    if (!top2bottom && plane.XDirection.Y > 0) radswap = -1;
                    _rVal.Value = top2bottom ? 0 : 1;
                    if (aSection.X > 0 && !top2bottom)   lastrad= deckrad;
                    
                }
                else//================ HORIZONTAL ================================
                {
                   
                    left2right = !aSplice.IsTop;
                    if (left2right && plane.YDirection.Y > 0) radswap = -1;
                    if (!left2right && plane.YDirection.Y < 0) radswap = -1;
                    _rVal.Value = left2right ? 0 : 1;
                }
                //create the flange out line
                if (!bSimple)
                {
                    verts.Add(plane, aX:0, aY:0, aTag: "SLOT_FLANGE", aFlag: "NODIM", aVertexRadius: 0);
                    verts.Add(plane, aX: startpt2.X - 0.24, aY: 0, aVertexRadius: radswap * relieferad, aTag: "SLOT_FLANGE", aFlag: "NODIM");
                    verts.Add(plane, aX: startpt2.X - 0.24 / 2, aY: relieferad, aVertexRadius: radswap * relieferad, aTag: "SLOT_FLANGE", aFlag: "NODIM");
                    verts.Add(plane, aX: startpt2.X, aY: 0, aVertexRadius: 0, aTag: "SLOT_FLANGE", aFlag: "NODIM");
                    verts.Add(plane, aX: startpt2.X, aY: -2 * gap, aVertexRadius: 0, aLineType: dxfLinetypes.Hidden, aTag: "SLOT_FLANGE", aFlag: "NODIM");
                    tip1 = verts.Add(plane, aX: startpt2.X, aY: startpt2.Y, aVertexRadius: 0, aLineType: dxfLinetypes.Hidden, aTag: "SLOT_FLANGE", aFlag: "NODIM");

                    tip2 = verts.Add(plane, aX: endpt2.X, aY: endpt2.Y, aVertexRadius: 0, aLineType: dxfLinetypes.Hidden, aTag: "SLOT_FLANGE", aFlag: "NODIM");
                    verts.Add(plane, aX: endpt2.X, aY: -2 * gap, aVertexRadius: 0, aLineType: string.Empty, aTag: "SLOT_FLANGE", aFlag: "NODIM");
                    verts.Add(plane, aX: endpt2.X, aY: 0, aVertexRadius: radswap * relieferad, aTag: "SLOT_FLANGE", aFlag: "NODIM");
                    verts.Add(plane, aX: endpt2.X + 0.24 / 2, aY: relieferad, aVertexRadius: radswap * relieferad, aTag: "SLOT_FLANGE", aFlag: "NODIM");
                    verts.Add(plane, aX: endpt2.X + 0.24, aY: 0, aVertexRadius: 0, aTag: "SLOT_FLANGE", aFlag: "NODIM");
                }
                else
                {
                    verts.Add(plane, aX: 0, aY: 0, aVertexRadius: 0, aTag: "SLOT_FLANGE", aFlag: "NODIM");
                    verts.Add(plane, aX: startpt2.X, aY: 0, aVertexRadius: 0, aTag: "SLOT_FLANGE", aFlag: "NODIM");
                    tip1 = verts.Add(plane, aX: startpt2.X, aY: startpt2.Y, aVertexRadius: 0, aLineType: dxfLinetypes.Hidden, aTag: "SLOT_FLANGE", aFlag: "NODIM");
                    tip2 = verts.Add(plane, aX: endpt2.X, aY: endpt2.Y, aVertexRadius: 0, aLineType: dxfLinetypes.Hidden, aTag: "SLOT_FLANGE", aFlag: "NODIM");
                    verts.Add(plane, aX: endpt2.X, aY: 0, aVertexRadius: radswap * relieferad, aTag: "SLOT_FLANGE", aFlag: "NODIM");
                }


                verts.Add(plane, baseline.Length, 0, aTag: "SLOT_FLANGE", aFlag: "NODIM", aVertexRadius: lastrad);
               
                _rVal.Vertices.AddRange(verts);
                if(!bSimple)_rVal.AdditionalSegments.Add(new dxeLine(tip1 + plane.YDirection * thk, tip2 + plane.YDirection * thk, dxfDisplaySettings.Null(aLinetype: dxfLinetypes.Hidden)) { Tag = "Thickness" });

                


                //add the slots and bend lines
                if(!bSimple) xxAddSlotEntities(_rVal, baseline, flngLn, aSplice, plane, out tSlotCtrs);

                //aSplice.FlangeLine = flngLn;
                if (!bSimple && tSlotCtrs.Count > 0)
                    aSection.SlotPoints.Append(tSlotCtrs);

                _rVal.InsertionPt = new dxfVector(aSplice.Center);
                return _rVal;
            }
            catch (Exception)
            {
                return _rVal;
            }

        }



        private static void xxAddSlotEntities(dxePolygon aSplicePolygon, uopLine aBaseLine, uopFlangeLine aFlangeLine, uopDeckSplice aSplice, dxfPlane aPlane, out uopVectors rSlotCenters)
        {
            rSlotCenters = uopVectors.Zero;
            try
            {

                aFlangeLine ??= new uopFlangeLine(aSplice.FlangeLine);

                double thk = aSplice.DeckThickness;

                double rad = uopDeckSplice.TabSlotRadius(aSplice.DeckThickness);
                dxfVector v1 = null;
                double gap = uppSpliceTypes.SpliceWithTabs.GapValue();
                dxfPlane plane = aPlane;
                uopLine baseline = aBaseLine;
                if (plane == null || baseline == null) baseline = aSplice.BaseLine(null, out plane);
                rSlotCenters = new uopVectors(aFlangeLine.Points);

                rSlotCenters.SetOrdinates(aSplice.Vertical ? dxxOrdinateDescriptors.X : dxxOrdinateDescriptors.Y, aSplice.Vertical ? baseline.X() : baseline.Y());
                
                if (rSlotCenters.Count <= 0) return;

                if (!aSplice.Vertical)
                {
                    rSlotCenters.Sort(aSplice.IsTop ? dxxSortOrders.RightToLeft : dxxSortOrders.LeftToRight);
                }
                else
                {
                    rSlotCenters.Sort(aSplice.IsTop ? dxxSortOrders.BottomToTop: dxxSortOrders.TopToBottom);
                }

                    double slen = mdGlobals.DeckTabSlotLength - 2 * rad;

                uopVector center = rSlotCenters[0];
                plane = new dxfPlane(plane, center);
                dxfPlane baseplane = new dxfPlane(plane);
                //create the slot
                uopArc leftEnd = new uopArc(rad, new uopVector(plane.Vector(-slen / 2, 0)));

               // uopShape arc = new uopShape(new uopVectors() { new uopVector(plane.Vector(-slen / 2, rad)) { Radius = rad }, new uopVector(plane.Vector(-mdGlobals.DeckTabSlotLength / 2, 0)) { Radius = rad }, new uopVector(plane.Vector(-slen / 2, -rad)) }, bOpen: true);
                uopLine line = new uopLine(new uopVector(plane.Vector(-100, -2 * gap)), new uopVector(plane.Vector(100, -2 * gap)));
                uopVectors ips = line.Intersections(new uopArc(rad, new uopVector(plane.Vector(-slen / 2, 0))), true, true);
                uopVector u1 = ips.Nearest(baseline.StartPt);
                v1 = new dxfVector(u1).WithRespectToPlane(plane);
                
                //aSplicePolygon.AdditionalSegments.Add(new dxeArc(new dxfVector(ips.Item(1)), 0.0625, aDisplaySettings: dxfDisplaySettings.Null(aColor: dxxColors.Blue)));
                //aSplicePolygon.AdditionalSegments.Add(new dxeArc(new dxfVector(ips.Item(2)), 0.0625, aDisplaySettings: dxfDisplaySettings.Null(aColor: dxxColors.Yellow)));
                //aSplicePolygon.AdditionalSegments.Add(new dxeLine(line , aDisplaySettings: dxfDisplaySettings.Null(aColor: dxxColors.Blue)));


                colDXFEntities segs = new colDXFEntities();
                colDXFVectors verts = colDXFVectors.Zero;


                verts.Add(plane, -slen / 2, rad, aVertexRadius: rad, aTag: "SLOT_FLANGE", aFlag: "NODIM");
                verts.Add(plane, -mdGlobals.DeckTabSlotLength / 2, 0, aVertexRadius: rad, aTag: "SLOT_FLANGE", aFlag: "NODIM");
                verts.Add(plane, v1.X, v1.Y, aVertexRadius: rad, aLineType: dxfLinetypes.Hidden, aTag: "TRANSITION", aFlag: "LEFT");
                verts.Add(plane, -slen / 2, -rad, aLineType: dxfLinetypes.Hidden);
                verts.Add(plane, slen / 2, -rad, aVertexRadius: rad, aLineType: dxfLinetypes.Hidden);
               
                verts.Add(plane, -v1.X, v1.Y, aVertexRadius: rad, aTag: "TRANSITION", aFlag: "RIGHT");
                verts.Add(plane, mdGlobals.DeckTabSlotLength / 2, 0, aVertexRadius: rad, aLineType: string.Empty, aTag: "SLOT_FLANGE", aFlag: "NODIM");
                verts.Add(plane, slen / 2, rad, aVertexRadius: 0, aLineType: string.Empty);
                dxePolygon slot = new dxePolygon(verts, aInsertPt: plane.Origin, bClosed: true) { Tag = "TABSLOT" };
                uopShape uslot1 = new uopShape(verts);
                colDXFEntities bendlines = new colDXFEntities();
                uopLine line1 = new uopLine(new uopVector(plane.Vector(-100, thk)), new uopVector(plane.Vector(100, thk)));
                uopLine line2 = new uopLine(new uopVector(plane.Vector(-100, -thk)), new uopVector(plane.Vector(100, -thk)));
                uopVectors ips1 = line1.Intersections(aSegments: uslot1.Segments, aLineIsInfinite: true);
                u1 = ips1.Nearest(baseline.StartPt);
                dxfVector offset = new dxfVector(u1).WithRespectToPlane(plane);
                dxfVector sp = new dxfVector(aFlangeLine.sp).WithRespectToPlane(plane);
                dxfVector ep = new dxfVector(aFlangeLine.ep).WithRespectToPlane(plane);

                dxeLine bendline1 = (dxeLine)bendlines.Add(new dxeLine(plane.Vector(offset.X, offset.Y), plane.Vector(sp.X, offset.Y)));
                dxeLine bendline2 = (dxeLine)bendlines.Add(new dxeLine(plane.Vector(offset.X, -offset.Y), plane.Vector(sp.X, -offset.Y)));

                segs.Append(slot.SubEntities(), bAddClones: false);
                segs.AddPoint(plane.Origin, aTag: "TABSLOT", aFlag: "CENTER");
                aSplicePolygon.AdditionalSegments.Append(segs, bAddClones: true);
                uopVector xdir = new uopVector(plane.XDirection.X, plane.XDirection.Y);
                for (int i = 2; i <= rSlotCenters.Count; i++)
                {
                    
                    bendline1 = (dxeLine)bendlines.Add(new dxeLine(plane.Vector(-offset.X, offset.Y), plane.Vector(offset.X, offset.Y)));
                    bendline2 = (dxeLine)bendlines.Add(new dxeLine(plane.Vector(-offset.X, -offset.Y), plane.Vector(offset.X, -offset.Y)));

                    u1 = rSlotCenters.Item(i);
                    xdir = center.DirectionTo(u1, out double d1);

                    plane.SetCoordinates(u1.X, u1.Y);

                    segs.Project(xdir, d1);
                    aSplicePolygon.AdditionalSegments.Append(segs, bAddClones: true);

                    bendline1.EndPt = plane.Vector(offset.X, offset.Y);
                    bendline2.EndPt = plane.Vector(offset.X, -offset.Y);

                    bendlines.Add(bendline1);
                    bendlines.Add(bendline2);
                    center = u1;

                    if (i == rSlotCenters.Count)
                    {
                        bendline1 = (dxeLine)bendlines.Add(new dxeLine(plane.Vector(-offset.X, offset.Y), baseplane.Vector(ep.X, offset.Y)));
                        bendline2 = (dxeLine)bendlines.Add(new dxeLine(plane.Vector(-offset.X, -offset.Y), baseplane.Vector(ep.X, -offset.Y)));

                    }
                }

                aSplicePolygon.AdditionalSegments.Append(bendlines, bAddClones: false, aTag: "TABBEND", aFlag: "BEND LINE");
                //}


            }
            catch (Exception)
            {
            }
        }


        private static double _CurThk;
        private static double _CurGap;
        private static dxePolygon _SlotPolyline;
        private static dxePolygon _TabPolyline;

        /// <summary>
        /// adds the tab vertices to the passed splice polygon
        /// </summary>
        /// <param name="aSplicePolygon"></param>
        /// <param name="aSplice"></param>
        /// <returns></returns>
        private static void xxAddTabVertices(colDXFVectors aVertices, uopDeckSplice aSplice, dxfPlane aPlane, ref colDXFVectors ioBaseVerts, ref List<dxfEntity> ioBendLines, bool bSimple = false)
        {
            //double gap = uppSpliceTypes.SpliceWithTabs.GapValue();
            try
            {
                colDXFVectors verts = aVertices;
                if(ioBendLines == null) ioBendLines  = new List<dxfEntity>();
                double rad = uopDeckSplice.TabSlotRadius(aSplice.DeckThickness);
           
                dxfVector v1 = null;
                dxfVector v2 = null;
                dxfVector v3 = null;
                dxfVector v4 = null;
                dxfVector v5 = null;
                dxfVector v6 = null;
                dxfVector v7 = null;
                dxfVector v8 = null;

                dxfPlane plane = aPlane == null ? new dxfPlane(aSplice.Center) : aPlane;
                if (!bSimple)
                {
                    if (ioBaseVerts == null)
                    {
                        double topY = +uppSpliceTypes.SpliceWithTabs.GapValue() + mdGlobals.TabSlotOffset + rad;

                        double f1 = 1;
                        if (aSplice.Vertical)
                        {
                            f1 = (plane.XDirection.Y == -1 && plane.YDirection.X == -1) || (plane.XDirection.Y == -1 && plane.YDirection.X == 1) ? -1 : 1;
                        }
                        else
                        {
                            f1 = (plane.XDirection.X == 1 && plane.YDirection.Y == -1) || (plane.XDirection.X == -1 && plane.YDirection.Y == 1) ? -1 : 1;
                        }


                        dxfVector transition = new dxfVector(-0.5 * mdGlobals.DeckTabWidth, topY);


                        ioBaseVerts = new colDXFVectors();
                        v1 = ioBaseVerts.Add(new dxfVector(transition.X - 0.125, 0) { VertexRadius = f1 * 0.125 });
                        v2 = ioBaseVerts.Add(new dxfVector(v1.X + 0.125, v1.Y + 0.125));
                        v3 = ioBaseVerts.Add(new dxfVector(transition.X, transition.Y) { Linetype = dxfLinetypes.Hidden });
                        v4 = ioBaseVerts.Add(new dxfVector(transition.X, mdGlobals.DeckTabHeight) { Linetype = dxfLinetypes.Hidden });
                        v5 = ioBaseVerts.Add(new dxfVector(-v4.X, v4.Y) { Linetype = dxfLinetypes.Hidden });
                        v6 = ioBaseVerts.Add(new dxfVector(-v4.X, topY));
                        v7 = ioBaseVerts.Add(new dxfVector(-v2.X, v2.Y) { VertexRadius = f1 * 0.125 });
                        v8 = ioBaseVerts.Add(new dxfVector(-v1.X, v1.Y) { VertexRadius = f1 * 0.125 });

                    }
                    else
                    {
                        v1 = ioBaseVerts[0];
                        v2 = ioBaseVerts[1];
                        v3 = ioBaseVerts[2];
                        v4 = ioBaseVerts[3];
                        v5 = ioBaseVerts[4];
                        v6 = ioBaseVerts[5];
                        v7 = ioBaseVerts[6];
                        v8 = ioBaseVerts[7];

                    }

                    v1 = verts.Add(plane.Vector(v1, aTag: "TAB", aFlag: "NODIM"));
                    v2 = verts.Add(plane.Vector(v2, aTag: "TAB", aFlag: "NODIM"));
                    v3 = verts.Add(plane.Vector(v3, aTag: "TAB", aFlag: "NODIM"));
                    v4 = verts.Add(plane.Vector(v4, aTag: "TAB", aFlag: "NODIM"));
                    v5 = verts.Add(plane.Vector(v5, aTag: "TAB", aFlag: "NODIM"));
                    v6 = verts.Add(plane.Vector(v6, aTag: "TAB", aFlag: "NODIM"));
                    v7 = verts.Add(plane.Vector(v7, aTag: "TAB", aFlag: "NODIM"));
                    v8 = verts.Add(plane.Vector(v8, aTag: "TAB", aFlag: "NODIM"));

                    dxfVector sp = v2.WithRespectToPlane(plane);
                    dxfVector ep = v7.WithRespectToPlane(plane);

                    dxeLine bendline = new dxeLine(plane.Vector(sp), plane.Vector(ep)) { Tag = "TABBEND" };
                    ioBendLines.Add(bendline);
                    ioBendLines.Add(bendline.Projected(plane.YDirection, 0.1));
                    ioBendLines.Add(new dxePoint(plane.Vector(0, mdGlobals.DeckTabHeight)) { Tag = "TABSLOT" });

                }
                else
                {
                    if (ioBaseVerts == null)
                    {
                        
                     
                        

                        ioBaseVerts = new colDXFVectors();
                        v1 = ioBaseVerts.Add(new dxfVector(-0.5 * mdGlobals.DeckTabWidth, 0));
                        v2 = ioBaseVerts.Add(new dxfVector(v1.X , mdGlobals.DeckTabHeight));
                        v3 = ioBaseVerts.Add(new dxfVector(0.5 * mdGlobals.DeckTabWidth, mdGlobals.DeckTabHeight));
                        v4 = ioBaseVerts.Add(new dxfVector(v3.X, 0));

                    }
                    else
                    {
                        v1 = ioBaseVerts[0];
                        v2 = ioBaseVerts[1];
                        v3 = ioBaseVerts[2];
                        v4 = ioBaseVerts[3];

                    }

                    v1 = verts.Add(plane.Vector(v1, aTag: "TAB", aFlag: "NODIM"));
                    v2 = verts.Add(plane.Vector(v2, aTag: "TAB", aFlag: "NODIM"));
                    v3 = verts.Add(plane.Vector(v3, aTag: "TAB", aFlag: "NODIM"));
                    v4 = verts.Add(plane.Vector(v4, aTag: "TAB", aFlag: "NODIM"));
             

                }


            }
            catch (Exception)
            {
            }

        }

        /// <summary>
        /// returns the plan view of a deck section tab
        /// </summary>
        /// <param name="aDeckThickness"></param>
        /// <param name="aGap"></param>
        /// <returns></returns>
        public static dxePolygon TabPolygon(double aDeckThickness, double aGap)
        {
            if (aGap <= 0) aGap = uopUtils.DeckGapValue(uppSpliceTypes.SpliceWithTabs, false);
            dxePolygon _rVal = null;
            if ((_CurThk != aDeckThickness && aDeckThickness > 0) && _CurGap != Math.Abs(aGap))
                _TabPolyline = null;

            if (_TabPolyline != null) return _TabPolyline;
            _CurThk = aDeckThickness;
            _CurGap = Math.Abs(aGap);


            try
            {
                colDXFVectors verts = colDXFVectors.Zero;

                dxePolygon aPline = SlotPolygon(_CurThk, _CurGap);
                double rad = aPline.Vertex(1).Radius;

                double topY = aPline.Vertices.GetOrdinate(dxxOrdinateTypes.MaxY);
                dxfVector v1 = aPline.Vertices.GetByTag(aTag: "TRANSITION").GetVector(dxxPointFilters.AtMinX);
                dxfVector transition = new dxfVector(-0.5 * mdGlobals.DeckTabWidth, topY);

                v1 = verts.Add(v1.X, -_CurGap, aTag: "TAB", aFlag: "NODIM");
                dxfVector v2 = null;
                //v2 = verts.Add(transition.X , topY, aVertexRadius: 0, aTag: "TAB", aFlag: "NODIM");
                dxfVector v3 = null;
                dxfVector v4 = null;
                dxfVector v5 = null;
                dxfVector v6 = null;
                if (transition.Y > v1.Y + 0.125)
                {
                    v2 = verts.Add(transition.X - 0.125, v1.Y, aVertexRadius: 0.125, aTag: "TAB", aFlag: "NODIM");
                    v3 = verts.Add(v2.X + 0.125, v2.Y + 0.125, aVertexRadius: 0, aLineType: string.Empty, aTag: "TAB", aFlag: "NODIM");
                    v4 = verts.Add(transition.X, transition.Y, aVertexRadius: 0, aLineType: dxfLinetypes.Hidden, aTag: "TAB", aFlag: "NODIM");
                    v5 = verts.Add(transition.X, v1.Y + mdGlobals.DeckTabHeight, aVertexRadius: 0, aLineType: dxfLinetypes.Hidden, aTag: "TAB", aFlag: "NODIM");
                    v6 = verts.Add(-v5.X, v5.Y, aVertexRadius: 0, aLineType: dxfLinetypes.Hidden, aTag: "TAB", aFlag: "NODIM");
                    verts.Add(-v5.X, topY, aVertexRadius: 0, aTag: "TAB", aFlag: "NODIM");
                    verts.Add(-v3.X, v3.Y, aVertexRadius: 0.125, aTag: "TAB", aFlag: "NODIM");
                    verts.Add(-v2.X, v2.Y, aVertexRadius: 0.125, aTag: "TAB", aFlag: "NODIM");
                    verts.Add(-v1.X, v1.Y, aVertexRadius: 0, aTag: "TAB", aFlag: "NODIM");
                }
                else
                {
                    v2 = verts.Add(transition.X, transition.Y, aVertexRadius: 0, aLineType: dxfLinetypes.Hidden, aTag: "TAB", aFlag: "NODIM");
                    v3 = verts.Add(transition.X, v1.Y + mdGlobals.DeckTabHeight, aVertexRadius: 0, aLineType: dxfLinetypes.Hidden, aTag: "TAB", aFlag: "NODIM");
                    v5 = verts.Add(-v3.X, v3.Y, aVertexRadius: 0, aLineType: dxfLinetypes.Hidden, aTag: "TAB", aFlag: "NODIM");
                    v6 = verts.Add(-v3.X, v2.Y, aVertexRadius: 0.125, aTag: "TAB", aFlag: "NODIM");
                    verts.Add(-v2.X, v2.Y, aVertexRadius: 0, aTag: "TAB", aFlag: "NODIM");
                    verts.Add(-v1.X, v1.Y, aVertexRadius: 0, aTag: "TAB", aFlag: "NODIM");
                }




                double epy = v1.Y + 0.125;

                _TabPolyline = new dxePolygon(verts, aInsertPt: new dxfVector(0, mdGlobals.DeckTabHeight - _CurGap), bClosed: false) { Tag = "TAB" };
                _rVal = _TabPolyline;

                _TabPolyline.AdditionalSegments.AddLine(transition.X, epy, -transition.X, epy, aTag: "TABBEND");
                epy = v1.Y + 0.125 + 0.1;
                _TabPolyline.AdditionalSegments.AddLine(transition.X, epy, -transition.X, epy, aTag: "TABBEND");

                _TabPolyline.AdditionalSegments.AddPoint(_TabPolyline.InsertionPt, aTag: "TABPOINT");



                //double rad = aSplice.TabSlotRadius;
                //double slen = mdGlobals.DeckTabSlotLength - 2 * rad;

                //double x = -slen / 2;
                //dxeArc aArc = new dxeArc(new dxfVector(-slen / 2, 0), rad, aStartAngle: 90, aEndAngle: 270);
                //dxeLine aL = new dxeLine(new dxfVector(-3 * mdGlobals.DeckTabSlotLength, -_CurGap), new dxfVector(3 * mdGlobals.DeckTabSlotLength, -_CurGap));
                //dxfVector transition = aL.Intersections(aArc, true, false).Item(1);

                //verts.Add(-slen / 2, rad, aVertexRadius: rad, aTag: "SLOT_FLANGE", aFlag: "NODIM");
                //verts.Add(transition.X, transition.Y, aVertexRadius: rad, aTag: "TRANSITION", aFlag: "RIGHT");
                //verts.Add(-slen / 2, -rad);
                //verts.Add(slen / 2, -rad, aVertexRadius: rad);
                //verts.Add(-transition.X, transition.Y, aVertexRadius: rad, aTag: "TRANSITION", aFlag: "LEFT");
                //verts.Add(slen / 2, rad, aVertexRadius: rad);


            }
            catch (Exception)
            {
                return _rVal;
            }

            return _rVal;  //.Clone();
        }

        /// <summary>
        /// returns the plan view of a deck section slot & tab slot
        /// </summary>
        /// <param name="aDeckThickness"></param>
        /// <param name="aGap"></param>
        /// <returns></returns>
        public static dxePolygon SlotPolygon(double aDeckThickness, double aGap)
        {
            dxePolygon _rVal = null;
            //if ((_CurThk != aDeckThickness && aDeckThickness > 0) && _CurGap != Math.Abs(aGap))
            _SlotPolyline = null;

            if (_SlotPolyline != null) return _SlotPolyline;
            _CurThk = aDeckThickness;
            _CurGap = Math.Abs(aGap);

            try
            {
                colDXFVectors verts = colDXFVectors.Zero;

                double rad = uopDeckSplice.TabSlotRadius(_CurThk);
                double slen = mdGlobals.DeckTabSlotLength - 2 * rad;
                double x = -slen / 2;
                dxeArc aArc = new dxeArc(new dxfVector(-slen / 2, 0), rad, aStartAngle: 90, aEndAngle: 270);
                dxeLine aL = new dxeLine(new dxfVector(-3 * mdGlobals.DeckTabSlotLength, -_CurGap - mdGlobals.TabSlotOffset), new dxfVector(3 * mdGlobals.DeckTabSlotLength, -_CurGap - mdGlobals.TabSlotOffset));
                colDXFVectors ips = aL.Intersections(aArc, true, false);
                dxfVector v1 = aL.Intersections(aArc, true, false).Item(1);

                verts.Add(-slen / 2, rad, aVertexRadius: rad, aTag: "SLOT_FLANGE", aFlag: "NODIM");
                verts.Add(-mdGlobals.DeckTabSlotLength / 2, 0, aVertexRadius: rad, aTag: "SLOT_FLANGE", aFlag: "NODIM");
                verts.Add(v1.X, v1.Y, aVertexRadius: rad, aLineType: dxfLinetypes.Hidden, aTag: "TRANSITION", aFlag: "LEFT");
                verts.Add(-slen / 2, -rad, aLineType: dxfLinetypes.Hidden);
                verts.Add(slen / 2, -rad, aVertexRadius: rad, aLineType: dxfLinetypes.Hidden);
                verts.Add(-v1.X, v1.Y, aVertexRadius: rad, aTag: "TRANSITION", aFlag: "RIGHT");
                verts.Add(mdGlobals.DeckTabSlotLength / 2, 0, aVertexRadius: rad, aLineType: string.Empty, aTag: "SLOT_FLANGE", aFlag: "NODIM");
                verts.Add(slen / 2, rad, aVertexRadius: 0, aLineType: dxfLinetypes.Hidden);

                _SlotPolyline = new dxePolygon(verts, aInsertPt: dxfVector.Zero, bClosed: true) { Tag = "TABSLOT" };
                _rVal = _SlotPolyline;

            }
            catch (Exception)
            {
                return _rVal;
            }

            return _rVal;  //.Clone();
        }

        public static uopRectangles GetSpliceLimits( uopDeckSplice aSplice, uopDeckSplice bSplice, double aAdder = 0)
        {
            uopRectangles _rVal = new uopRectangles();
            if (aSplice == null && bSplice == null) return _rVal;
            try
            {
                if (aSplice != null)
                {

                        URECTANGLE aRec = aSplice.Limits();
                        aRec.Row = aSplice.PanelIndex;
                        if (aAdder != 0)
                            aRec.Stretch(aAdder);
                        _rVal.Add(aRec);
                    }

                if (bSplice != null)
                {

                    URECTANGLE aRec = bSplice.Limits();
                    aRec.Row = bSplice.PanelIndex;
                    if (aAdder != 0)
                        aRec.Stretch(aAdder);
                    _rVal.Add(aRec);
                }
            }
            catch { }



            return _rVal;
        }

        public static uopRectangles GetSpliceLimits(List<uopDeckSplice> aSplices, double aAdder = 0)
        {
            uopRectangles _rVal = new uopRectangles();
            if (aSplices == null || aSplices.Count <=0) return _rVal;
            try
            {
                foreach (uopDeckSplice item in aSplices)
                {

                    if (item != null)
                    {
                        URECTANGLE aRec = item.Limits();
                        aRec.Row = item.PanelIndex;
                        if (aAdder != 0)  
                            aRec.Stretch(aAdder); 
                        _rVal.Add(aRec);
                    }
                }
            }
            catch { }
         


            return _rVal;
        }


        /// <summary>
        /// executed  to create the splice angles collection based on the current splices
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="bTrayWide"></param>
        /// <returns></returns>
        public colUOPParts GenerateSpliceAngles(mdTrayAssembly aAssy, bool bTrayWide = false)
        {
            colUOPParts _rVal = new colUOPParts();
            try
            {

                aAssy ??= GetMDTrayAssembly(aAssy);
                if (aAssy == null) return _rVal;

                _rVal.SubPart(aAssy);
                List<mdDeckPanel> DPs = aAssy.DeckPanels.ActivePanels(aAssy, out bool specialcase, out _);

                mdSpliceAngle aSA = null;
                mdSpliceAngle SAng = aAssy.SpliceAngle(uppSpliceAngleTypes.SpliceAngle);
                mdSpliceAngle MAng = aAssy.SpliceAngle(uppSpliceAngleTypes.ManwaySplicePlate);
                bool mirrors = aAssy.IsStandardDesign && !aAssy.OddDowncomers;

                double dkthk = aAssy.Deck.Thickness;

                for (int pid = 1; pid <= DPs.Count; pid++)
                {
                    List<uopDeckSplice> pSplices = FindAll(x => x.PanelIndex == pid);
                    for (int j = 1; j <= pSplices.Count; j++)
                    {
                        uopDeckSplice aDS = pSplices[j - 1];
                        if (aDS.SpliceType == uppSpliceTypes.SpliceManwayCenter)
                        {
                            //manwya centers for slot and tab
                            aSA = new mdSpliceAngle(MAng) { PanelIndex = aDS.PanelIndex, SpliceHandle = aDS.Handle, Center = new uopVector(aDS.Center), Direction = dxxRadialDirections.TowardsCenter };

                            aSA.Z = 0.5 * dkthk;
                            aSA.PartType = uppPartTypes.ManwaySplicePlate;
                            _rVal.Add(aSA);
                            if (bTrayWide && Math.Round(aDS.X, 1) > 0 && mirrors)
                            {
                                aSA = aSA.Clone();
                                aSA.X = -aSA.X;
                                aSA.Y = -aSA.Y;
                                aSA.PartIndex = _rVal.Count + 1;
                                _rVal.Add(aSA);
                            }
                        }
                        else if (aDS.SpliceType == uppSpliceTypes.SpliceWithAngle)
                        {
                            aSA = new mdSpliceAngle(SAng) { PanelIndex = aDS.PanelIndex, SpliceHandle = aDS.Handle, Center = new uopVector(aDS.Center) };
                            aSA.PartType = uppPartTypes.SpliceAngle;
                            if (aSA.SupportsManway)
                            {
                                aSA.PartType = uppPartTypes.ManwayAngle;
                                if (aDS.Y > 0)
                                {
                                    aSA.Direction = (aDS.Direction == dxxOrthoDirections.Down) ? dxxRadialDirections.TowardsCenter : dxxRadialDirections.AwayFromCenter;
                                }
                                else
                                {
                                    aSA.Direction = (aDS.Direction == dxxOrthoDirections.Up) ? dxxRadialDirections.TowardsCenter : dxxRadialDirections.AwayFromCenter;
                                }

                                aSA.Y = (aDS.Direction == dxxOrthoDirections.Down) ? aDS.Y + 0.1875 : aDS.Y - 0.1875;
                            }
                            else
                            {
                                
                                aSA.Direction = dxxRadialDirections.TowardsCenter;
                                aSA.AngleType = uppSpliceAngleTypes.SpliceAngle;
                            }
                            aSA.PartIndex = _rVal.Count + 1;
                            _rVal.Add(aSA);
                            if (bTrayWide && Math.Round(aDS.X, 1) > 0 && mirrors)
                            {
                                bool addit = !specialcase;
                                if (specialcase)
                                {
                                    addit = pid! < DPs.Count - 1;
                                }
                                if (addit)
                                {
                                    aSA = aSA.Clone();
                                    aSA.X = -aSA.X;
                                    aSA.Y = -aSA.Y;
                                    _rVal.Add(aSA);
                                }


                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return _rVal;
        }


        public static uopShapes GenMDBlockedAreas(uopDeckSplice aSplice, uopSectionShape aSection = null, bool bInverted = false, double aTabWidthAdder = 0, double aTabHeightAdder = 0) 
        { 
            uopShapes _rVal = new uopShapes("BLOCKED AREAS");
            if(aSplice == null ) return _rVal;

            aSection ??= aSplice.Section;
            int occurs = aSection == null ? 1 : aSection.OccuranceFactor;
            int pid = aSection == null ? aSplice.PanelIndex : aSection.PanelIndex;
            string sectionhandle = aSection == null ? string.Empty : aSection.Handle;
            int secindex = aSection == null ? 1 : aSection.SectionIndex;

            URECTANGLE lims = URECTANGLE.Null;

            bool vertical = aSplice.Vertical;

            //================ SPLICES ===========================
            uopFlangeLine flngLn = new uopFlangeLine(aSplice, bInverted, aSection);
            string splicehandle = aSplice.Handle;
            URECTANGLE splLims = aSplice.Limits(true);
           
            bool isTop = aSplice.IsTop;
            if (bInverted) isTop = !isTop;
            string side = aSplice.IsTop ? "TOP" : "BOTTOM";
            if (vertical)
            {
                        
                if (aSplice.SpliceType == uppSpliceTypes.SpliceWithTabs)
                {
                if (isTop)
                {
                    lims = new URECTANGLE(aLeft: aSplice.X, aTop: flngLn.MaxY, aRight: flngLn.Ordinate, flngLn.MinY);
                    _rVal.Add(new uopShape(lims, aName: $"{side} SPLICE {splicehandle}", aTag: "VFLANGE", aFlag: splicehandle, aHandle: sectionhandle, aOccurance: occurs, aPartIndex: pid, aRow: aSection.PanelSectionIndex, aCol: aSection.PanelIndex));
                }
                else
                {
                    List<double> tOrds = flngLn.Points.Ordinates(bGetY: true);
                    lims = new URECTANGLE(aLeft: flngLn.X() +(bInverted ? -1 : 1) * (0.5 * aTabHeightAdder ), aRight: aSplice.X);
                    foreach (double ord in tOrds)
                    {
                        lims.Define(aTop: ord + 0.5 * mdGlobals.DeckTabWidth + 0.5 * aTabWidthAdder, aBot: ord - 0.5 * mdGlobals.DeckTabWidth - 0.5 * aTabWidthAdder);
                        _rVal.Add(new uopShape(lims, aName: $"{side} SPLICE {splicehandle}", aTag: "VTAB", aFlag: splicehandle, aHandle: sectionhandle, aOccurance: occurs, aPartIndex: pid, aRow: secindex, aCol: pid));

                    }
                }
                     
                }
            }
            else
            {

                 double f1 = aSplice.TabDirection == dxxOrthoDirections.Up ? 1 : -1;
                if (aSplice.SpliceType == uppSpliceTypes.SpliceWithTabs)
                {
                    if (aSplice.Female)
                    {
                        List<double> tOrds = flngLn.Points.Ordinates(bGetY: false);
                        lims = new URECTANGLE(aTop: aSplice.Y + f1 * (- aSplice.GapValue() + mdGlobals.DeckTabHeight + 0.5 * aTabHeightAdder), aBottom: aSplice.Y);
                        foreach (double ord in tOrds)
                        {
                            lims.Define(aLeft: ord - 0.5 * mdGlobals.DeckTabWidth -0.5 * aTabWidthAdder , aRight: ord + 0.5 * mdGlobals.DeckTabWidth + 0.5 * aTabWidthAdder);
                            _rVal.Add(new uopShape(lims,aName: $"{side} SPLICE {splicehandle}", aTag: "TAB", aFlag: splicehandle, aHandle: sectionhandle, aOccurance: occurs, aPartIndex: pid, aRow : secindex, aCol : pid));
                        }
                    }
                    else
                    {
                        lims = new URECTANGLE(flngLn.MinX, flngLn.Ordinate, flngLn.MaxX, aSplice.Y);
                        _rVal.Add(new uopShape(lims,aName: $"{side} SPLICE {splicehandle}", aTag: vertical ? "VFLANGE" : "FLANGE", aFlag: splicehandle, aHandle: sectionhandle, aOccurance: occurs, aPartIndex: pid, aRow : secindex, aCol : pid));
                    }
                }
                else if (aSplice.SpliceType == uppSpliceTypes.SpliceWithJoggle)
                {
                    if (aSplice.Female)
                    {
                        lims = new URECTANGLE(splLims.Left, splLims.Top, splLims.Right, aSplice.Y);
                        _rVal.Add(new uopShape(lims,aName: $"{side} SPLICE {splicehandle}", aTag: "JOGGLE", aFlag: splicehandle, aHandle: sectionhandle, aOccurance: occurs, aPartIndex: pid, aRow : secindex, aCol : pid));
                    }
                }
                else if (aSplice.SpliceType == uppSpliceTypes.SpliceWithAngle)
                {

                    f1 = isTop ? 1 : -1;
                    lims = new URECTANGLE(flngLn.MinX, aSplice.Y, flngLn.MaxX, aSplice.Y + f1 * mdGlobals.SpliceAngleWidth / 2);
                    _rVal.Add(new uopShape(lims,aName: $"{side} SPLICE {splicehandle}", aTag: "SPLICE ANGLE", aFlag: splicehandle, aHandle: sectionhandle, aOccurance: occurs, aPartIndex: pid, aRow : secindex, aCol : pid));
                }
                else if (aSplice.SpliceType == uppSpliceTypes.SpliceManwayCenter)
                {
                    f1 = isTop ? 1 : -1;
                    lims = new URECTANGLE(flngLn.MinX, aSplice.Y, flngLn.MaxX, aSplice.Y + f1* mdGlobals.SpliceAngleWidth / 2);
                    _rVal.Add(new uopShape(lims,aName: $"{side} SPLICE {splicehandle}", aTag: "MANWAY SPLICE", aFlag: splicehandle, aHandle: sectionhandle, aOccurance: occurs, aPartIndex: pid, aRow : secindex, aCol : pid));

                }
            }
            

            return _rVal;
        } 
        #endregion Shared Methods
    }
}

