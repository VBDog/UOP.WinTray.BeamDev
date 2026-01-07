using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Parts;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;

namespace UOP.WinTray.Projects
{
    public class mdSpoutZone : List<mdSpoutArea>, IEnumerable<mdSpoutArea>, ICloneable
    {

        #region Constructors

        public mdSpoutZone()  => Initialize();
        public mdSpoutZone(mdTrayAssembly aAssy, int aZoneIndex = 1)
        {
            Initialize(aAssy: aAssy);
            Index = aZoneIndex;
         
        }

        public mdSpoutZone(mdSpoutZone aZone) => Initialize(aZone);

        bool _Init = false;
        private void Initialize(mdSpoutZone aZone = null, mdTrayAssembly aAssy = null) 
        {
            if (!_Init)
            {
                Index = 0;
                WeirFraction = 1;
                WeirLns = new List<ULINEPAIR>();
                base.Clear();
                TargetArea = 0;
                _Init = true;
                DowncomerCount = 0;
          
                _AssyRef = null;
                TrayName = string.Empty;
                DesignFamily = uppMDDesigns.Undefined;
                ConvergenceLimit = 0.00001;
                MultiPanel = false;
            }
            if (aZone != null) 
            {
                Index = aZone.Index;
                WeirFraction = aZone.WeirFraction;
                WeirLns = uopLinePairs.Copy(aZone.WeirLns);
                DesignFamily = aZone.DesignFamily;
                TrayName = aZone.TrayName;
                base.Clear();
                foreach (var  sa in aZone) Add(new mdSpoutArea(sa));
                TargetArea = aZone.TargetArea;
                ConvergenceLimit = aZone.ConvergenceLimit;
                DowncomerCount = aZone.DowncomerCount;
                MultiPanel  = aZone.MultiPanel;
                aAssy ??= aZone.TrayAssembly;
            }

            if(aAssy != null)
            {
                TrayAssembly = aAssy;
            }
            
        }
        #endregion Constructors

        #region Properties

        public int Index{get; set;}
        
        public double ConvergenceLimit{get; set;}
        public double WeirFraction{get; set;}

        private List<ULINEPAIR> _WeirLns;
        internal List<ULINEPAIR> WeirLns { get => _WeirLns; set => _WeirLns= value; }

        private List<double> _DowncomerWeirLengths;
        internal List<double> DowncomerWeirLengths
        {
            get 
            {
                if(_DowncomerWeirLengths == null && WeirLns != null )
                {
                    _DowncomerWeirLengths = new List<double>();
                    List<ULINEPAIR> weirs = WeirLns;
                    for(int d = 1; d<= DowncomerCount; d++)
                    {
                        double tot = 0;
                        List<ULINEPAIR> DCweirs = weirs.FindAll((x) => x.Col == d);
                       foreach(ULINEPAIR weir in DCweirs)
                        {
                            tot += weir.Length(uppSides.Undefined);
                        }
                        _DowncomerWeirLengths.Add(tot);
                    }
                }
                return _DowncomerWeirLengths;
            }
            
        }

        private List<double> _PanelWeirLengths;
        internal List<double> PanelWeirLengths
        {
            get
            {
                if (_PanelWeirLengths == null && WeirLns != null)
                {

                    _PanelWeirLengths = new List<double>();
                    List<ULINEPAIR> weirs = WeirLns;
                    for (int p = 1; p <= PanelCount; p++)
                    {
                        List<ULINEPAIR> weirsL = weirs.FindAll((x) => x.Col == p);
                        List<ULINEPAIR> weirsR = weirs.FindAll((x) => x.Col == p -1);
                        double tot = 0;
                       foreach(ULINEPAIR pair in weirsL)
                              tot += pair.Length(uppSides.Right);

                        foreach (ULINEPAIR pair in weirsR)
                            tot += pair.Length(uppSides.Left);
           
                        _PanelWeirLengths.Add(tot);
                    }
                    if (TreatAsHalfTray) _PanelWeirLengths.Reverse();
                }
                return _PanelWeirLengths;
            }
        }

        public List<uopLinePair> WeirLines => uopLinePairs.FromULinePairs(WeirLns);

        internal List<ULINEPAIR> PanelWeirLns
        {
            get
            {
                List<ULINEPAIR> _rVal = new List<ULINEPAIR>();
                if (WeirLns.Count <= 0) return _rVal;
                for (int i = 1; i <= WeirLns.Count +1; i++)
                {
                    ULINEPAIR dcpairR =  i <= WeirLns.Count ? WeirLns[i - 1]: ULINEPAIR.Null;
                    ULINEPAIR dcpairL = i > 1 ?  WeirLns[i - 2] : ULINEPAIR.Null;
                    ULINEPAIR panelPair = ULINEPAIR.Null;
                    if (i == 1)
                    {
                        panelPair.Line2 = new ULINE(dcpairR.GetSide(uppSides.Right), aSide: uppSides.Left);
                    }
                    else if (i == WeirLns.Count + 1)
                    {
                        panelPair.Line1 = new ULINE(dcpairL.GetSide(uppSides.Left), aSide: uppSides.Right);
                    }
                    else
                    {
                       if(i > 1) panelPair.Line1 = new ULINE(WeirLns[i - 2].GetSide(uppSides.Left), aSide: uppSides.Right);
                        panelPair.Line2 = new ULINE(WeirLns[i - 1].GetSide(uppSides.Right), aSide: uppSides.Left);
                    }

                    _rVal.Add(panelPair);
                     
                   
                }

                return _rVal;

            }
        }

        public double WeirLength => uopLinePairs.TotalLength(WeirLns);

        public double TargetArea { get; set; }

        public DowncomerDataSet  DCData { get; set; }

        public int DowncomerCount { get; set; }

        public int PanelCount => DowncomerCount > 0 ? DowncomerCount + 1 : 0;

        private WeakReference< mdSpoutAreaMatrix> _MatrixRef;
        public mdSpoutAreaMatrix SpoutAreaMatrix
        {
            get
            {
                mdSpoutAreaMatrix _rVal = null;
                if(_MatrixRef==null)
                {
                    _rVal = new mdSpoutAreaMatrix(this, false);
                    _MatrixRef = new WeakReference<mdSpoutAreaMatrix>(_rVal);
                }
                else
                {
                    if(!_MatrixRef.TryGetTarget(out _rVal)) _MatrixRef = null;
                }
                
                return _rVal;
            
            }

            set
            {
                if (value == null) { _MatrixRef = null; return; }
                _MatrixRef = new WeakReference<mdSpoutAreaMatrix>(value);
            }
        }

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
                TrayName = value.TrayName();
                DesignFamily = value.DesignFamily;
                DowncomerCount = value.Downcomer().Count;
                DCData = new DowncomerDataSet(value.DowncomerData);
                ConvergenceLimit = value.ConvergenceLimit;
                MultiPanel = DCData.MultiPanel;
            }
        }

        public bool TreatAsHalfTray => DesignFamily.IsBeamDesignFamily() && !MultiPanel;
        public bool MultiPanel { get;  set; }
        public string TrayName { get; set; }

       public  uppMDDesigns DesignFamily { get; set; }

        public bool IsSymmetric => DesignFamily.IsStandardDesignFamily(); // && !OddDowncomers;

        public mdSpoutArea SelectedArea
        {
            get
            {
                if( Count <= 0) return null;
                mdSpoutArea _rVal = Find((x) => x.Selected);
                if (_rVal == null) { _rVal = this[0]; _rVal.Selected = true; }

                return _rVal;
                ;
            }
        }

        #endregion Properties

        #region Methods

    
        public void SetSelected(string aHandle)
        {
            int idx = -1;
            foreach (var member in this)
            { member.Selected = member.Handle == aHandle; if (member.Selected && idx == -1) idx = IndexOf(member); }

            if (idx == -1 && Count > 0) this[0].Selected = true;
        }
        object ICloneable.Clone() => (object)this.Clone();
        public mdSpoutZone Clone()  =>new mdSpoutZone(this);

        public new void Add(mdSpoutArea aArea)
        {
            if (aArea == null) return;
            aArea.ZoneIndex = Index;
            base.Add(aArea);

        }

        public mdSpoutArea Item(int aPanelIndex, int aDowcomerIndex, bool bCheckInstances = false, bool bGetClone = false)
        {
            mdSpoutArea _rVal = Find((x) => x.PanelIndex == aPanelIndex && x.DowncomerIndex == aDowcomerIndex);

            if(_rVal == null && bCheckInstances)
            {
                foreach(var item in this)
                {
                    uopInstances insts = item.Instances;
                    if(insts.FindIndex((x) => x.Row == aPanelIndex && x.Col == aDowcomerIndex)>=0) 
                    {
                        _rVal = item;
                        break;
                    }
                }
            }
            if (bGetClone && _rVal != null) _rVal = new mdSpoutArea(_rVal);
            return _rVal;
        }
        public mdSpoutArea Item(string aHandle, bool bCheckInstances = false, bool bGetClone = false)
        {
            if (string.IsNullOrWhiteSpace(aHandle)) return null;
            mdSpoutArea _rVal = Find((x) => x.Handle == aHandle);

            //if (_rVal == null && bCheckInstances)
            //{
            //    foreach (var item in this)
            //    {
            //        uopInstances insts = item.Instances;
            //        if (insts.FindIndex((x) => x.Handle == aHandle) >= 0)
            //        {
            //            _rVal = item;
            //            break;
            //        }
            //    }
            //}
            if (bGetClone && _rVal != null) _rVal = new mdSpoutArea(_rVal);
            return _rVal;
        }

        public uopMatrix AvailableAreaMatrix(bool bAddTotalRowsAndColumns = false)
        {
            int iNd = DowncomerCount;
            if (iNd <= 0) return null;

            int iNp = PanelCount;

            uopMatrix _rVal = GetMatrix(uppSpoutAreaMatrixDataTypes.AvailableArea);

            if (_rVal == null) return null;

            if (bAddTotalRowsAndColumns)
            {
                _rVal.AddColumns(1);
                _rVal.AddRows(1);


                _rVal.TotalRows(iNd + 1, 1, iNd, 1, iNp);
                _rVal.TotalColumns(iNp + 1, 1, iNp, 1, iNd);
                _rVal.TotalRows(iNd + 1, 1, iNd, iNp + 1, iNp + 1);

            }

            return _rVal;

        }

        public void GetIdealMatrices(out uopMatrix rDCIdeals, out uopMatrix rDPIdeals, int aPrecis = 6)
        {
           
            rDCIdeals =GetMatrix(uppSpoutAreaMatrixDataTypes.DowncomerIdeals, aPrec:aPrecis);
            rDPIdeals = GetMatrix(uppSpoutAreaMatrixDataTypes.DeckPanelIdeals, aPrec: aPrecis);
        }

        public List<double> GetIdealAreas(bool bForDowncomers) 
        {
            List<double> _rVal = new List<double>();
            List<double> lengths = bForDowncomers ? DowncomerWeirLengths : PanelWeirLengths;
            if(lengths == null ) return _rVal;

            double ltotal = 0;
            double totalLength = WeirLength;
            foreach (double d in lengths) 
            {
                ltotal += d;
                if (TargetArea <=0 || totalLength <=0) 
                {
                    _rVal.Add(0);
                }
                else
                {
                    double frac = d / totalLength;
                    _rVal.Add( frac * TargetArea);
                }
                //Console.WriteLine($"{d}");
            }

            return _rVal;
        }

        public uopMatrix GetMatrix(uppSpoutAreaMatrixDataTypes aDataType ,string aNameSuffix = "" ,int aPrec = 6)
        {
            if(DowncomerCount <= 0) return null;
            uopMatrix _rVal = null;
         

            switch (aDataType)
            { 
                case uppSpoutAreaMatrixDataTypes.DowncomerIdeals:
                case uppSpoutAreaMatrixDataTypes.DeckPanelIdeals:
                    bool fordc = aDataType == uppSpoutAreaMatrixDataTypes.DowncomerIdeals;
                    string tstr = fordc ? "DC" : "DP"; 
                    _rVal = new uopMatrix(GetIdealAreas(bForDowncomers:fordc  ), bReturnColumn: false, aPrec, $"{uopEnums.Description(aDataType)} {TrayName} Zone:{Index} {aNameSuffix}");
                    
                    break;
                default:
                    bool bInvis = aDataType == uppSpoutAreaMatrixDataTypes.AvailableArea ;
                    _rVal = new uopMatrix(PanelCount, DowncomerCount, aPrecis: aPrec, aName: $"{uopEnums.Description(aDataType)} {TrayName} Zone:{Index} {aNameSuffix}", bInvisibleVal: bInvis);

                    foreach (mdSpoutArea item in this)
                    {
                        if (item.IsVirtual) continue;
                        List<uopMatrixCell> cells = item.GetMatrixCells(aDataType, bZeroInstances: TreatAsHalfTray);
                        _rVal.SetMemberCells(cells, bInVis: false);
                    }
                    break;
            }

            //_rVal.PrintToConsole();
            return _rVal;
            
        }

        public bool UpdateMatrix(ref uopMatrix aMatrix,uppSpoutAreaMatrixDataTypes aDataType, bool bGrowToInclude = false)
        {
            if(aMatrix == null) return false;
            bool _rVal = false;
            if(aDataType == uppSpoutAreaMatrixDataTypes.IdealSpoutArea)
            {
                mdSpoutAreaMatrix idealareas = SpoutAreaMatrix;
                if(idealareas == null) return false;
                List<uopMatrixCell> cells = idealareas.Cells();
                //idealareas.PrintToConsole();
                if (aMatrix.SetMemberCells(cells, bGrowToInclude:bGrowToInclude, bSuppressIndexErrors:true)) _rVal = true;
            }
            else
            {
                foreach (var item in this)
                {
                    if (item.IsVirtual) continue;

                    List<uopMatrixCell> cells = item.GetMatrixCells(aDataType);
                    if (aMatrix.SetMemberCells(cells)) _rVal = true;
                }

            }
            //rVal.PrintToConsole();
            return _rVal;

        }

        /// <summary>
        /// reurns the current lock and group area matrices
        /// </summary>
        public void GetLockedAndGroupedAreaMatrices(out uopMatrix rLocks, out uopMatrix rGroups)
        {
            rLocks = new uopMatrix();
            rGroups = new uopMatrix();
            mdTrayAssembly aAssy = TrayAssembly;
            if (aAssy != null)
            DowncomerCount = aAssy.Downcomer().Count;
            if (DowncomerCount <=0) return;

            

            int dcnt = DowncomerCount;
            int pcnt = PanelCount;
            string tname = aAssy?.TrayName(false);
            rLocks = new uopMatrix(pcnt, dcnt, aPrecis: 6, $"Area Locks {tname}", -1);
            rGroups = new uopMatrix(pcnt, dcnt, aPrecis: 1, $"Group Indices {tname}", 0);

            foreach (var item in this)
            {
                if (item.IsVirtual) continue;
                List<uopMatrixCell> cells = item.GetMatrixCells(uppSpoutAreaMatrixDataTypes.LockValue);
                rLocks.SetMemberCells(cells);

                //foreach (var cell in cells)
                //    rLocks.SetMember(cell.Row, cell.Col, cell.Value);
                
                cells = item.GetMatrixCells(uppSpoutAreaMatrixDataTypes.GroupIndex);
                rLocks.SetMemberCells(cells);

                //foreach (var cell in cells)
                //    rLocks.SetMember(cell.Row, cell.Col, cell.Value);

            }

            //bool? symmetric = null;
            //if (aAssy.IsSymmetric) symmetric = true;

            //for (int i = 1; i <= Count; i++)
            //{
            //    mdSpoutArea aMem = this[i-1];

            //    if (!aMem.IsVirtual)
            //    {
            //        int didx = aMem.DowncomerIndex;
            //        int pidx = aMem.PanelIndex;
            //        mdConstraint mdcons = aMem.Constraints(aAssy);
            //        bool bIdeal = aMem.TreatAsIdeal;
            //        bool bGroup = aMem.TreatAsGroup;
            //        double gVal = bGroup ? (double)aMem.GroupIndex : 0;
            //        rGroups.SetMember(pidx, didx, gVal, symmetric);
            //        double aVal = aMem.TreatAsIdeal && !aMem.TreatAsGroup && aMem.OverrideSpoutArea.HasValue ? aMem.OverrideSpoutArea.Value : -1;
            //        rLocks.SetMember(pidx, didx, aVal, symmetric);
            //    }

         //   }
            //rGroups.PrintToConsole();
        }

        public List<int> GetGroupIndices(out List<List<mdSpoutArea>> rGroupedAreas)
        {
            List<int> _rVal = new List<int>();
            List<int> groupIDS = new List<int>();
            rGroupedAreas = new List<List<mdSpoutArea>>();
            List<mdSpoutArea> allgroupers = new List<mdSpoutArea>();
            List<mdSpoutArea> group = new List<mdSpoutArea>();
            foreach (var area in this)
            {
                if (area.TreatAsIdeal || area.IsVirtual || area.GroupIndex <=0)
                {
                    area.GroupIndex = 0;
                    continue;
                }
                allgroupers.Add(area);

                int idx = groupIDS.IndexOf(area.GroupIndex);

                if(idx < 0)
                    groupIDS.Add(area.GroupIndex);
              
            }

            groupIDS.Sort();
            for(int i = 1; i<= groupIDS.Count; i++)
            {
                int gid = groupIDS[i -1];
                group = allgroupers.FindAll((x) => x.GroupIndex == gid);
                if (gid != i)
                {
                    foreach (var area in group) area.GroupIndex = i;
                }
                _rVal.Add(i);
                rGroupedAreas.Add(group);
            }

            return _rVal;
        }


        public List<string> GetGroupNames(out List<List<mdSpoutArea>> rGroupedAreas, bool bAddOne = false, bool bGetLetters = false)
        {
            List<string> _rVal = new List<string>();

            List<int> groupIDS = GetGroupIndices(out rGroupedAreas);
            if (bAddOne && groupIDS.Count + 1 <= mdSpoutAreaMatrix.MaxGroupings) groupIDS.Add(groupIDS.Count + 1);
            for(int i = 1; i<= groupIDS.Count; i++)
            {
                if (!bGetLetters) _rVal.Add($"Group {i}"); else _rVal.Add($"Group {mzUtils.ConvertIntegerToLetter(i)}");
            }

            return _rVal;
        }

        #endregion Methods
    }
}
