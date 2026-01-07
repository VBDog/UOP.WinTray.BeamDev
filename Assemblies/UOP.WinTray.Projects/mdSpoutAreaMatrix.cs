using ClosedXML.Excel;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using UOP.DXFGraphics;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Parts;
using UOP.WinTray.Projects.src.Utilities;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Tables;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;

namespace UOP.WinTray.Projects
{
    public class mdSpoutAreaMatrix : uopMatrix, ICloneable
    {
        #region Constructors


         public mdSpoutAreaMatrix(mdSpoutZone aZone, bool bDistributeArea = true) : base() => Initialize(aZone, bDistributeArea);
        
        public mdSpoutAreaMatrix(mdSpoutAreaMatrix aMatrix) : base()
        {
            Initialize(aMatrix == null? null : aMatrix.Zone != null ? new mdSpoutZone( aMatrix.Zone): null ,false);
            if (aMatrix == null) return;

            base.Resize(aMatrix.Rows, aMatrix.Cols);
            for(int r = 1; r <= Rows; r++)
            {
                uopMatrixRow myRow = Row(r);
                uopMatrixRow herRow = aMatrix.Row(r);

                for (int c = 1; c <= Cols; c++)
                {
                    uopMatrixCell mycell = myRow.Cell(c);
                    uopMatrixCell hercell = herRow.Cell(c);
                    mycell.Copy(hercell);

                }
            }

            TotalsColumn = aMatrix.TotalsColumn;
            TotalsRow = aMatrix.TotalsRow;
            DOF = aMatrix.DOF;
            IdealsColumn = aMatrix.IdealsColumn;
            IdealsRow = aMatrix.IdealsRow;
            DeviationsColumn = aMatrix.DeviationsColumn;
            DeviationsRow = aMatrix.DeviationsRow;
            VariableMarkers = uopMatrix.CloneCopy(aMatrix.VariableMarkers);
            MaxPanelDeviation = aMatrix.MaxPanelDeviation;
            MaxDowncomerDeviation = aMatrix.MaxDowncomerDeviation;
   
            _AvailableAreaMatrix = uopMatrix.CloneCopy( aMatrix._AvailableAreaMatrix);
            _DowncomerIdeals = uopMatrix.CloneCopy(aMatrix._DowncomerIdeals);
            _DeckPanelIdeals = uopMatrix.CloneCopy(aMatrix._DeckPanelIdeals);
            _SpoutGroupAreas = uopMatrix.CloneCopy(aMatrix._SpoutGroupAreas);

            if (aMatrix._RowHeaders != null) {_RowHeaders = new List<string>(); _RowHeaders.AddRange(aMatrix._RowHeaders); }
            if (aMatrix._ColumnHeaders != null) { _ColumnHeaders = new List<string>(); _ColumnHeaders.AddRange(aMatrix._ColumnHeaders); }

        }

        private  bool _Initializing = false;
        internal void Initialize(mdSpoutZone aZone, bool bDistribute = true)
        {

            try
            {
                _Initializing = true;
                Precision = 10;
                Name = "Area Matrix";
                Zone = aZone;
                if (Zone == null) return;
                List<string> dcheaders = new List<string>();
                int iNd = DowncomerCount;
                int iNp = PanelCount;
                dcheaders = ColumnHeaders;
                IsConverged = false;
                List<string> dpheaders = RowHeaders;
                

                //initialize the base matrix with extra columns and rows for totals and ideals and deviations
                base.Initialize(dpheaders.Count, dcheaders.Count, aColumnHeaders: dcheaders, aRowHeaders: dpheaders, bInvisibleVal: false);

                //Console.WriteLine($"{Cell(1, 1).Invisible}");
                UserVar1 = 0; UserVar2 = false;
                Name = $"Area Matrix {Zone.TrayName} Zone:{Zone.Index}";
                _AvailableAreaMatrix = Zone.AvailableAreaMatrix(true);
                double theo = Zone.TargetArea;
                List<uopLinePair> weirs = Zone.WeirLines;



                for (int r = 1; r <= iNp; r++)
                {
                    for (int c = 1; c <= iNd; c++)
                    {
                        uopMatrixCell cell = Cell(r, c);
                        List<uopLinePair> dcweirs = weirs.FindAll((x) => x.Col == c);
                        uopMatrixCell availcell = _AvailableAreaMatrix.Cell(r, c);
                        cell.Invisible = availcell.Invisible;
                        cell.IsVirtual = availcell.IsVirtual;

                        if (dcweirs.Count > 0 && availcell.Value != 0)
                        {
                            double denom = uopLinePairs.TotalLength(dcweirs);
                            double ideal = denom != 0 ? (availcell.Value / denom) * theo : 0;
                          
                            cell.SetValue(ideal, availcell.Invisible);
                            if (TreatAsHalfTray && cell.IsVirtual)
                            {
                                ideal = 0;
                                cell.Invisible = true;
                            }
                        }
                        else
                        {
                            cell.Invisible = true;
                        }

                    }
                }


                this.DeviationsColumn = ColumnHeaders.FindIndex((x) => string.Compare(x, "Deviation", true) == 0) + 1; // GetColIndex("Deviation");
                TotalsColumn = ColumnHeaders.FindIndex((x) => string.Compare(x, "Totals", true) == 0) + 1; //GetColIndex("Totals");
                IdealsColumn = ColumnHeaders.FindIndex((x) => string.Compare(x, "Ideals", true) == 0) + 1;  //GetColIndex("Ideals");
                DeviationsRow = RowHeaders.FindIndex((x) => string.Compare(x, "Deviation", true) == 0) + 1; // GetRowIndex("Deviation");
                TotalsRow = RowHeaders.FindIndex((x) => string.Compare(x, "Totals", true) == 0) + 1; // GetRowIndex("Totals");
                IdealsRow = RowHeaders.FindIndex((x) => string.Compare(x, "Ideals", true) == 0) + 1;  // GetRowIndex("Ideals");
                                                                                                      //PrintToConsole();
                _DowncomerIdeals = Zone.GetMatrix(uppSpoutAreaMatrixDataTypes.DowncomerIdeals, aPrec: Precision);
                    _DeckPanelIdeals = Zone.GetMatrix(uppSpoutAreaMatrixDataTypes.DeckPanelIdeals, aPrec: Precision);

                    //add the panel ideals into the last column
                PopulateCol(IdealsColumn, _DeckPanelIdeals.Row(1), bInvis: false);
                //add the dowmcomer ideals into the second last rows
                PopulateRow(IdealsRow, _DowncomerIdeals.Row(1), bInvis: false);
                //add the panel ideals into the second last column
                PopulateCol(TotalsColumn, _DeckPanelIdeals.Row(1), bInvis: false);
                //add the downcomer ideals into the second last row
                PopulateRow(TotalsRow, _DowncomerIdeals.Row(1), bInvis: false);

                // //add the dowmcomer ideals into the second last rows
                //PopulateRow(TotalsRow, _DowncomerIdeals.Row(1));
                ////add the panel ideals into the second last column
                //PopulateCol(totcol, _DeckPanelIdeals.Row(1));

                UserVar1 = 0; UserVar2 = false;

                if (bDistribute) DistributeSpoutArea();
            }
            finally
            {
                _Initializing = false;
            }
            
            //PrintToConsole();
        }



        #endregion Constructors

        #region Properties

        public mdTrayAssembly TrayAssembly => Zone == null ? null : Zone.TrayAssembly;

        private List<string> _ColumnHeaders;
        public override List<string> ColumnHeaders
        {
            get
            {if(_ColumnHeaders == null && Zone != null)
                {
                    _ColumnHeaders = new List<string>();
                    int iNd = DowncomerCount;
                 
                    for (int i = 1; i <= iNd; i++) { _ColumnHeaders.Add($"DC { i}"); }
                    _ColumnHeaders.Add("Totals");
                    _ColumnHeaders.Add("Ideals");
                    _ColumnHeaders.Add("Deviation");
                    base.ColumnHeaders = _ColumnHeaders;
                }

                return _ColumnHeaders;
            }

            set
            {
                _ColumnHeaders = value;
                base.ColumnHeaders = _ColumnHeaders;

            }
        }
        public double TargetArea  => Zone == null ? 0 : Zone.TargetArea;
           public uopMatrix LockedAreas
        {
            get
            {
                if (Zone == null) return null;
                uopMatrix _rVal = new uopMatrix(Np, Nd, 10, $"{uopEnums.Description(uppSpoutAreaMatrixDataTypes.LockValue)} {Label}", -1d);
                foreach (var area in Zone)
                {
                    if (area.TreatAsIdeal)
                    {
                        List<uopMatrixCell> cells = Cells(area.PanelIndex, area.DowncomerIndex, area.Instances);
                        foreach (var cell in cells)
                        {
                            _rVal.SetValue(cell.Row, cell.Col, area.OverrideSpoutArea.Value);
                        }
                    }
                }
                return _rVal;
            }
        }

        public uopMatrix GroupedAreas
        {
            get
            {
                if(Zone == null) return null;
                uopMatrix _rVal = new uopMatrix(Np,Nd,2, $"{uopEnums.Description(uppSpoutAreaMatrixDataTypes.GroupIndex)} {Label}");
                foreach(var area in Zone) 
                { 
                    if(area.GroupIndex > 0)
                    {
                        List<uopMatrixCell> cells = Cells(area.PanelIndex, area.DowncomerIndex, area.Instances);
                        foreach(var cell in cells)
                        {
                            _rVal.SetValue(cell.Row, cell.Col, area.GroupIndex);
                        }
                    }
                }
                return _rVal;
            }
        }


        public string Label => Zone == null ? string.Empty : $"{Zone.TrayName} Zone:{Zone.Index}";


        private List<string> _RowHeaders;
        public override List<string> RowHeaders
        {
            get
            {
               
                if (_RowHeaders == null && Zone != null)
                {
                    _RowHeaders = new List<string>();
                     
                    int iNp = PanelCount;
                    
                    for (int i = 1; i <= iNp; i++) 
                    {
                        int p = !OptionB ? i : uopUtils.OpposingIndex(i, iNp);

                        _RowHeaders.Add($"DP {p}"); 
                    }
                    _RowHeaders.Add("Totals");
                    _RowHeaders.Add("Ideals");
                    _RowHeaders.Add("Deviation");
                    base.RowHeaders = _RowHeaders;
                }
                return _RowHeaders;
            }
            set 
            { 
               _RowHeaders = value;
                base.RowHeaders = _RowHeaders;
            }
        }

        public mdSpoutZone Zone { get; set; }

        public int DowncomerCount => Zone == null ? 0 : Zone.DowncomerCount;
        public int PanelCount => Zone == null ? 0 : Zone.PanelCount;
        public bool TreatAsHalfTray => Zone == null ? false : Zone.TreatAsHalfTray;

        public bool MultiPanel => Zone == null ? false : Zone.MultiPanel;

        public int Nd => DowncomerCount;
        public int Np => PanelCount;

        private uopMatrix _DowncomerIdeals;
        public uopMatrix DowncomerIdeals
        {
            get
            {
                if (_DowncomerIdeals == null && Zone != null)
                {
                    _DowncomerIdeals = Zone.GetMatrix(Enums.uppSpoutAreaMatrixDataTypes.DowncomerIdeals);
                }
                return _DowncomerIdeals;
            }
        }
        private uopMatrix _DeckPanelIdeals;
        public uopMatrix DeckPanelIdeals
        {
            get
            {
                if (_DeckPanelIdeals == null && Zone != null)
                {
                    _DeckPanelIdeals = Zone.GetMatrix(Enums.uppSpoutAreaMatrixDataTypes.DeckPanelIdeals);
                }
                return _DeckPanelIdeals;
            }
        }
        private uopMatrix _AvailableAreaMatrix;
        public uopMatrix AvailableAreaMatrix
        {
            get
            {
                if (_AvailableAreaMatrix == null && Zone != null)
                {
                    _AvailableAreaMatrix = Zone.AvailableAreaMatrix(true);
                }
                return _AvailableAreaMatrix;
            }
        }

        public bool IsSymmetric => Zone == null ? false : Zone.IsSymmetric;

       public bool IsConverged { get;  set; }
        /// <summary>
        /// that percent deviation that the convergance routine uses to determine if
        /// the current solution is within acceptable limits
        /// ~max = 0.1 min = 0.0001 default = 0.001
        /// </summary>
        public double ConverganceLimit { get => Zone == null ? 0.001 : Zone.ConvergenceLimit; }

        public int DOF { get; set; }

        public int TotalsColumn { get; set; }
        public int TotalsRow { get; set; }


        public int DeviationsColumn { get; set; }
        public int DeviationsRow { get; set; }
        public int IdealsColumn { get; set; }
        public int IdealsRow { get; set; }

        public int Unknowns => VariableMarkers == null ? 0 : VariableMarkers.NonZeroCells(aPrecis: 2).Count;

        public uopMatrix VariableMarkers { get; set; }
        
        /// <summary>
        /// the max deviation computed for the deck panels
        /// </summary>
        public double MaxPanelDeviation { get; private set; }


        /// <summary>
        /// the max deviation computed for the downcomers
        /// </summary>
        public double MaxDowncomerDeviation { get; private set; }

        /// <summary>
        /// the rectangles that bound the spout areas in the array
        /// </summary>
        /// <remarks>the name of the rectangles matches the name of the non-virtual</remarks>
        public uopRectangles BoundingRectangles(bool bReturnVirtuals = true) 
        {
            if ( Zone == null) return new uopRectangles();
            uopRectangles _rVal = new uopRectangles();

            for (int r = 1; r <= Zone.PanelCount; r++)
            {
                for (int c = 1; c <= Zone.DowncomerCount; c++)
                {
                    mdSpoutArea area = Zone.Item(r, c);
                    if (area != null)
                    {
                        uopRectangle rect = area.Limits();
                        rect.Name = area.Handle;
                        uopInstances insts = area.Instances;
                        _rVal.Add(rect);
                        foreach (uopInstance inst in insts)
                        {
                            uopRectangle irect = new uopRectangle(rect) { Row = inst.Row, Col = inst.Col };
                            irect.Move(inst.DX, inst.DY);
                            irect.IsVirtual = true;
                            irect.Name = rect.Name;
                            _rVal.Add(irect);

                        }
                    }
                }
            }
                                
            return _rVal;
            
        }

        private uopMatrix _SpoutGroupAreas;
        public uopMatrix SpoutGroupAreas
        {
            get
            {
                if (_SpoutGroupAreas == null) UpdateSpoutGroupAreas(null);
                return _SpoutGroupAreas;
            }
            internal set
            {
                _SpoutGroupAreas = value;
            }
        }

        public bool OptionB { get; private set; }
        public bool HasConstraints => FindCells((x) => x.GroupIndex > 0 || x.OverrideValue.HasValue).Count > 0;
        #endregion Properties

        #region Methods

        public bool IsEqualTo(mdSpoutAreaMatrix aMatrix, int? aPrecis = null)
        {
            if (aMatrix == null) return false;
            if (aMatrix.Count != Count) return false;
            int prec = !aPrecis.HasValue ? Precision : mzUtils.LimitedValue(aPrecis.Value, 0, 15);

            return IsEqual(aMatrix,prec, true,true );
        }

        public new mdSpoutAreaMatrix Clone() => new mdSpoutAreaMatrix(this);

        object ICloneable.Clone() => (object)this.Clone();

        public void UpdateSpoutGroupAreas(mdTrayAssembly aAssy = null, colMDSpoutGroups aSpoutGroups = null, uopDocuments aWarnings = null)
        {
            _SpoutGroupAreas = null;
                aAssy ??= TrayAssembly;
            if (aAssy == null || Zone == null) return;
            aSpoutGroups ??= aAssy._SpoutGroups;
            if (aSpoutGroups == null) return ;

            try
            {
                _SpoutGroupAreas = new uopMatrix(PanelCount, DowncomerCount, aPrecis: 10, bInvisibleVal: true);
                foreach (var sg in aSpoutGroups)
                {
                    if ((sg.DowncomerIndex >= 1 && sg.DowncomerIndex <= DowncomerCount) && (sg.PanelIndex >= 1 && sg.PanelIndex <= PanelCount))
                    {
                        double area = sg.ActualArea;
                        _SpoutGroupAreas.SetValue(sg.PanelIndex, sg.DowncomerIndex, area, bInvis: false);
                        mdSpoutArea sgarea = sg.SpoutArea;
                        if (sgarea == null) continue;
                        uopInstances insts = sgarea.Instances;
                        foreach (var inst in insts) _SpoutGroupAreas.SetValue(inst.Row, inst.Col, area, bInvis: false);

                    }
                }
                _SpoutGroupAreas.Name = $"Spout Group Actual Areas {Zone.TrayName} Zone:{Zone.Index}";

            }
            catch (Exception ex) { if (aWarnings != null) aWarnings.AddWarning(ex, System.Reflection.MethodBase.GetCurrentMethod()); } 
        }

      

        public bool ClearConstraints(bool bClearLocks, bool bClearGroups)
        {
            if (!bClearLocks && !bClearLocks) return false;
            if (Zone == null) return false;
            bool _rVal = false;
            foreach (var item in Zone)
            {
                if (bClearGroups)
                {
                    if (item.GroupIndex > 0) _rVal = true;
                    item.GroupIndex = 0;

                }
                if (bClearLocks)
                {
                    if (item.TreatAsIdeal) _rVal = true;
                    item.OverrideSpoutArea = null;

                }
            }
            
            return _rVal;
        }

        public bool SetLockValue(int aPanelIndex, int aDowncomerIndex, bool aLockValue)
        {
            mdSpoutArea area = Area(aPanelIndex, aDowncomerIndex);
            if(area == null) return false;
            List<uopMatrixCell> cells = Cells(area.PanelIndex, area.DowncomerIndex, area.Instances);
            bool _rVal= false;
            uopMatrix spoutareas = aLockValue ? SpoutGroupAreas : new uopMatrix();
            double? lockval = null;
            if(aLockValue) lockval = spoutareas.Value(area.PanelIndex, area.DowncomerIndex)  ;
           area.GroupIndex = 0;
    
            area.OverrideSpoutArea = lockval;

            foreach(var cell in cells)
            {
                if (cell.OverrideValue != lockval) _rVal = true;
                if (cell.GroupIndex != 0) _rVal = true;
                cell.OverrideValue = lockval;
                cell.GroupIndex = 0;
            }

            return _rVal;
           
        }
        public bool SetLockValue(string aHandle, bool aLockValue)
        {
            mdSpoutArea area = Area(aHandle);
            if (area == null) return false;
            List<uopMatrixCell> cells = Cells(area.PanelIndex, area.DowncomerIndex, area.Instances);
            bool _rVal = false;
            uopMatrix spoutareas = aLockValue ? SpoutGroupAreas : new uopMatrix();
            double? lockval = null;
            if (aLockValue) lockval = spoutareas.Value(area.PanelIndex, area.DowncomerIndex);
            area.GroupIndex = 0;

            area.OverrideSpoutArea = lockval;

            foreach (var cell in cells)
            {
                if (cell.OverrideValue != lockval) _rVal = true;
                if (cell.GroupIndex != 0) _rVal = true;
                cell.OverrideValue = lockval;
                cell.GroupIndex = 0;
            }

            return _rVal;

        }
        public  bool SetGroupIndex(string aHandle, int aGroupIndex)
        {
            mdSpoutArea area = Area(aHandle);
            if (area == null) return false;

            
            
            aGroupIndex = mzUtils.LimitedValue(aGroupIndex, 0, Zone.DesignFamily.IsStandardDesignFamily() ? 1 : mdSpoutAreaMatrix.MaxGroupings);
            List<uopMatrixCell> cells = Cells(area.PanelIndex, area.DowncomerIndex, area.Instances);
            area.GroupIndex = aGroupIndex;
            area.OverrideSpoutArea = null;
            bool _rVal = false; 

            foreach (var cell in cells)
            {
                if (cell.GroupIndex != aGroupIndex) _rVal = true;
                if (cell.OverrideValue.HasValue) _rVal = true;
                cell.GroupIndex = aGroupIndex;
                cell.OverrideValue = null;
            }

            return _rVal;

        }

        public new bool SetGroupIndex(int aPanelIndex, int aDowncomerIndex, int aGroupIndex)
        {
            mdSpoutArea area = Area(aPanelIndex, aDowncomerIndex);
            if (area == null) return false;



            aGroupIndex = mzUtils.LimitedValue(aGroupIndex, 0, Zone.DesignFamily.IsStandardDesignFamily() ? 1 : mdSpoutAreaMatrix.MaxGroupings);
            List<uopMatrixCell> cells = Cells(area.PanelIndex, area.DowncomerIndex, area.Instances);
            area.GroupIndex = aGroupIndex;
            area.OverrideSpoutArea = null;
            bool _rVal = false;

            foreach (var cell in cells)
            {
                if (cell.GroupIndex != aGroupIndex) _rVal = true;
                if (cell.OverrideValue.HasValue) _rVal = true;
                cell.GroupIndex = aGroupIndex;
                cell.OverrideValue = null;
            }

            return _rVal;

        }

        public mdSpoutArea Area(int aPanelIndex, int aDowncomerIndex, bool bGetClone = false) 
        {
            return Zone == null ? null : Zone.Item(aPanelIndex, aDowncomerIndex, bCheckInstances: true, bGetClone: bGetClone);
        }
        public mdSpoutArea Area(string aHandle, bool bGetClone = false)
        {
            return Zone == null ? null : Zone.Item(aHandle, bCheckInstances: true, bGetClone: bGetClone);
        }
        public override void PrintToConsole(int StartRow = 0, int EndRow = 0, int StartCol = 0, int EndCol = 0, string aHeading = null, int? aPrecis = null)
            {
                 base.PrintToConsole(StartRow,EndRow,StartCol,EndCol,aHeading,aPrecis);
                if (Zone == null) return;
            DowncomerIdeals.PrintToConsole();
            DeckPanelIdeals.PrintToConsole();
            AvailableAreaMatrix.PrintToConsole();
        }
        public  void PrintToConsole(bool bVerbose, string aHeading = null, int ? aPrecis = null)
        {
            base.PrintToConsole(1, Rows, 1, Cols, aHeading, aPrecis);
            if (Zone == null || !bVerbose) return;
            DowncomerIdeals.PrintToConsole();
            DeckPanelIdeals.PrintToConsole();
            AvailableAreaMatrix.PrintToConsole();
        }

        

        public void DistributeSpoutArea( bool bVerbose = false, uopDocuments aWarnings = null) 
        {
            IsConverged = false;
            if (Zone == null) return;
            if ( !_Initializing) Initialize(Zone, bDistribute:false);

            int nochange = 0;
            bool converged = false;
            double  theo =this.TargetArea;
            if (theo <= 0)
            {
                SetAllValues(0);
                return;
            }

            //get the downcomer and deck panel ideals
            try
            {

           
                int totcol = TotalsColumn;  //ColumnHeaders.FindIndex((x) => string.Compare(x, "Totals", true) == 0) + 1; //GetColIndex("Totals");
                int idealcol = IdealsColumn; //ColumnHeaders.FindIndex((x) => string.Compare(x, "Ideals", true) == 0) + 1;  //GetColIndex("Ideals");
                int devcol = DeviationsColumn; // ColumnHeaders.FindIndex((x) => string.Compare(x, "Deviation", true) == 0) + 1; // GetColIndex("Deviation");

                int totrow = TotalsRow;  // RowHeaders.FindIndex((x) => string.Compare(x, "Totals", true) == 0) + 1; // GetRowIndex("Totals");
                int idealrow = IdealsRow; //RowHeaders.FindIndex((x) => string.Compare(x, "Ideals", true) == 0) + 1;  // GetRowIndex("Ideals");
                int devrow = DeviationsRow; // RowHeaders.FindIndex((x) => string.Compare(x, "Deviation", true) == 0) + 1; // GetRowIndex("Deviation");


                uopMatrix changesDC = new uopMatrix(Nd, 3, Precision, "Changes DC");
                uopMatrix changesDP = new uopMatrix(Np, 3, Precision, "Changes DP");

                double convergelimit = ConverganceLimit;

                if (bVerbose)
                {
                    PrintToConsole(bVerbose = true);


                }


                
                List<List<uopMatrixCell>> groups = ValidateGroupsAndLocks(out List<uopMatrixCell> lockcells, out List<int> groupindexs); // new List<List<uopMatrixCell>>();

                //foreach (uopMatrixCell cell in groupvals)
                //{
                //    if (cell.Value > 0)
                //        groups.Add(groupcells.FindAll(x => x.Value == cell.Value));
                //}


                //matAvails.PrintToConsole();

                
                GetVariableMarkers();


                int t = 0;
                for (t = 1; t <= mdSpoutAreaMatrix.MaxSmoothingPasses; t++)
                {
                    if (t > 1)
                    {


                        //smooth to deck panel ideals
                        for (int pidx = 1; pidx <= Np; pidx++)
                        {
                            //adjust for deviation from ideal

                            double ideal = Value(pidx, idealcol);

                            double curval = RowTotal(pidx, 1, Nd);
                            double ratio = ideal !=0 ? Math.Round(curval / ideal, Precision) :  0;
                            if (ratio != 0 && ratio != 1)
                                MultiplyRow(pidx, 1 / ratio, 1, Nd, bReplaceWithOverride: true);
                            //curval = RowTotal(pidx, 1, Nd);

                        }

                        //smooth to downcomer ideals
                        for (int didx = 1; didx <= Nd; didx++)
                        {

                            //adjust for deviation from ideal
                            double ideal = Value(idealrow, didx);

                            double curval = ColumnTotal(didx, 1, Np);
                            double ratio = ideal != 0 ? Math.Round(curval / ideal, Precision) : 0;
                            if (ratio != 0 && ratio != 1)
                                MultiplyColumn(didx, 1 / ratio, 1, Np, bReplaceWithOverride: true);
                            //curval = ColumnTotal(didx, 1, Np);

                        }


                    }

                    if (groupindexs.Count > 0)
                    {
                        //determines the average value of the areas that are grouped
                        //and sets the groups members aras to this value in the matrix
                        List<uopMatrixCell> groupcells = null;
                        foreach (int gidx in groupindexs)
                        {
                            groupcells  =   AverageGroupCells(gidx, 1, Np, 1, Nd);

                        }
                    }

                    //check for convergence
                    //convergance occurs when the maximum change
                    //of values between runs drops to a certain value
                    //compute changes from last run
                    //put last values in the first column

                    changesDP.SwapColumn(2, 1);
                    double maxChangeDP = 0;
                    double maxdeviation = 0;
                    double change = 0;
                    for (int p = 1; p <= Np; p++)
                    {
                        double last = changesDP.GetMember(p, 1);
                        //put the current totals for panels in the second column of the changes matrix
                        double dptotal = RowTotal(p, 1, Nd);
                        changesDP.SetValue(p, 2, dptotal);

                        //save the current panel totals in the totals column of the return matrix
                        SetValue(p, totcol, dptotal);
                        double ideal = Value(p, idealcol); //  IdealsDP.Value(p, 1);
                        double deviation = ideal != 0 ? Math.Abs((dptotal / ideal) - 1d) : 0;
                        //save the current panel deviation in the deviation column of the return matrix
                        SetValue(p, devcol, deviation);
                        maxdeviation = Math.Max(deviation, maxdeviation);
                        change = Math.Abs(last - dptotal);
                        maxChangeDP = Math.Max(change, maxChangeDP);
                        //compute the change between runs and put it in the 3rd column of the changes matrix
                        changesDP.SetValue(p, 3, change);
                    }

                    changesDC.SwapColumn(2, 1);
                    double maxChangeDC = 0;
                    for (int d = 1; d <= Nd; d++)
                    {
                        double last = changesDC.GetMember(d, 1);
                        //put the current totals for downcomers in the second column of the changes matrix
                        double dctotal = ColumnTotal(d, 1, Np);
                        changesDC.SetValue(d, 2, dctotal);

                        //save the current dcomer totals in the totals row of the return matrix
                        SetValue(totrow, d, dctotal);
                        double ideal = Value(idealrow, d); //  IdealsDC.Value(d ,1);
                        double deviation = ideal !=0 ? Math.Abs((dctotal / ideal) - 1d): 0;
                        //save the current dcomer deviation in the deviation row of the return matrix
                        SetValue(devrow, d, deviation);
                        
                        maxdeviation = Math.Max(deviation, maxdeviation);
                        change = Math.Abs(last - dctotal);
                        maxChangeDC = Math.Max(change, maxChangeDC);

                        //compute the change between runs and put it in the 3rd column of the changes matrix
                        changesDC.SetValue(d, 3, change);
                    }
                    //Console.WriteLine(mzUtils.ListToString(ColumnValues(devcol)));
                    //Console.WriteLine(mzUtils.ListToString(RowValues(devrow)));

                    //get the max change between runs (rounded)

                    double maxchange = Math.Round(Math.Max(maxChangeDC, maxChangeDP), 6);


                    //test convergence
                    if (maxdeviation < convergelimit)
                    {
                        converged = true;
                        break;
                    }
                    else
                    {
                        //bail if repeated iterations have not cause any change
                        nochange = maxchange == 0 ? nochange + 1 : 0;
                        if (nochange >= 25)
                        {
                            //  PrintToConsole();
                            break;

                        }
                    }
                }
                IsConverged = converged;
                
                if (bVerbose)
                {
                    Console.WriteLine($"t={t} - {mzUtils.ListToString(RowValues(devrow, 1, Nd), aPrecis: Precision, aPad: 1)}");
                    Console.WriteLine($"t={t} - {mzUtils.ListToString(ColumnValues(devcol, 1, Np), aPrecis: Precision, aPad: 1)}");

                }

                MaxDowncomerDeviation = MaximumRowEntry(devrow, true, 1, Nd);
                MaxPanelDeviation = MaximumColumnEntry(devcol, true, 1, Np);
                if (bVerbose)
                {
                    PrintToConsole(false, aHeading: $"{Dimensions} devrow:{devrow}  devcol:{devcol} max1:{MaxDowncomerDeviation}  max2:{MaxPanelDeviation}");
                }
                //format the return
                UserVar1 = t - 1;
                UserVar2 = converged;

                //return the matrix
                if (!converged)
                {
                    //PrintToConsole();
                }
                //

                //IdealsDC.PrintToConsole();

                //IdealsDP.PrintToConsole();

                //put the overall total in the lower right corner
                SetValue(GetRowIndex("Ideals"), GetColIndex("Ideals"), theo);

                List<uopMatrixCell> gcells = FindCells((x) => x.GroupIndex == 1);
                foreach (mdSpoutArea area in Zone)
                {
                    List<uopMatrixCell> areacells = Cells(area.PanelIndex, area.DowncomerIndex, area.Instances);
                    uopMatrixCell cell = areacells.Find((x) => x.Row == area.PanelIndex && x.Col == area.DowncomerIndex);
                    area.IdealSpoutArea = cell.Value != double.NaN ? cell.Value: 0;
                }


            }
            catch (Exception ex) { if (aWarnings != null) aWarnings.AddWarning(ex, System.Reflection.MethodBase.GetCurrentMethod()); }

        }

        public uopTable GetTable(uppUnitFamilies aUnits, bool bAddHeaders = true, bool bAddIdeals = false, int precisionadder = 0, bool bReverseColumns = false)
        {
            if (Size <=0) return new uopTable();
            if (aUnits != uppUnitFamilies.Metric) aUnits = uppUnitFamilies.English;

            // PrintToConsole();

            int rows = Rows + 1; // add one for the header 
            int cols = Cols + 1; // add one for the header 
            int dccnt = Nd;
            int pncnt = Np;
            uopUnit areas = uopUnits.GetUnit(uppUnitTypes.SmallArea);
            uopTable _rVal = new uopTable(rows, cols, Name);
            int precis = mzUtils.LimitedValue(areas.Precision(aUnits) + mzUtils.LimitedValue(precisionadder, 0, 3), 0, 15);
            try
            {
                int r;
                
                List<string> sdata = new List<string>();
                //List<List<double>> columndata = new List<List<double>>();

                for (int c = 1; c <= Cols; c++)
                {
                    string cheader = c <= ColumnHeaders.Count ? ColumnHeaders[c - 1] : string.Empty;
                   
                    //List<double> coldata = GetValues(c, bSearchCols: true);
                    //columndata.Add(coldata);

                    if (c == 1 && !bReverseColumns) sdata.Add(string.Empty);
                    sdata.Add(cheader);

                    if (bReverseColumns && c == DowncomerCount)
                     {
                        sdata.Reverse();
                        sdata.Insert(0, string.Empty);
                    }
                }
                //sdata.AddRange(ColumnHeaders);
                
                _rVal.RowValuesSet(1, sdata);


                sdata = new List<string>() { "" };

                
                sdata.AddRange(RowHeaders);
                _rVal.ColumnValuesSet(1, sdata);

                int devcol = GetColIndex("Deviation");
                int devrow = GetRowIndex("Deviation");
                int idealcol = GetColIndex("Ideals");
                int idealrow = GetRowIndex("Ideals");

                string devfmat = bAddIdeals ? "E2" : "E2";

             
                for (int c = 1; c <= Cols; c++)
                {
                    int dcidx = !bReverseColumns ? c : c <= dccnt ? uopUtils.OpposingIndex(c, dccnt) : c; 
                    List<uopMatrixCell> column = Column(dcidx);
      
                    //List<double> rvals = GetValues(dcidx, bSearchCols: true);
                    // rvals.Insert(rvals.Count - 1, -1d); // and a row for the ideals
                    sdata = new List<string>();
                    for (r = 1; r <= column.Count; r++)
                    {
                        mdSpoutArea area = r <= Np && c <= Nd ? Area(r, c) : null;
                        uopMatrixCell cell = column[r - 1];
                        cell.Color = area == null ? Color.Black : area.Color.Item2;
                        string sval = string.Empty;
                        double dval = 0;

                        if (!cell.Invisible)
                        {
                            dval = cell.Value;
                            if (c == devcol || (r == devrow))
                            {
                                sval = dval.ToString(devfmat);
                                if (r > pncnt)
                                {
                                    if (c > dccnt)
                                    {
                                        sval = string.Empty;
                                    }
                                }
                                //sval = r > pncnt || c > dccnt ? string.Empty : dval.ToString("0.00E00");
                            }
                            else
                            {
                                sval = dval >= 0 ? areas.UnitValueString(dval, aUnits, bAddLabel: false, bZeroAsNullString: false, aPrecis: precis) : "";
                                if (r > pncnt && c > dccnt)
                                {
                                    if (r != idealrow || c != idealcol) sval = string.Empty;

                                }
                            }

                        }

                        sdata.Add(sval);
                    }
                    int tcol = bAddHeaders ? c + 1 : c;
                    _rVal.ColumnValuesSet(tcol, sdata, bAddHeaders ? 2 : 1);
                }


                _rVal.SetFontColor("All", "All", System.Drawing.Color.Black);
                _rVal.SetCellValue(1, 1, "-");
              

          
                _rVal.SetFontColor(_rVal.GetRowIndex("Ideals", 1).ToString(), "All", System.Drawing.Color.DarkGray);
                _rVal.SetFontColor("All", _rVal.GetColIndex("Ideals", 1).ToString(), System.Drawing.Color.DarkGray);

                _rVal.SetFontColor(_rVal.GetRowIndex("Totals", 1).ToString(), "All", System.Drawing.Color.DarkGray);
                _rVal.SetFontColor("All", _rVal.GetColIndex("Totals", 1).ToString(), System.Drawing.Color.DarkGray);

                if (ConverganceLimit > 0)
                {
                    int c = _rVal.GetColIndex("Deviation", 1);

                    for (r = 1; r <= pncnt; r++)
                    {
                        double dval = Value(r, c - 1, aDefault: 0d);
                        _rVal.Cell(r + 1, c).FontColor = Math.Abs(dval) >= ConverganceLimit ? System.Drawing.Color.Red : System.Drawing.Color.Green;

                    }

                    r = _rVal.GetRowIndex("Deviation", 1);

                    for ( c = 1; c <= dccnt; c++)
                    {
                        double dval = Value(r - 1, c, aDefault: 0d);
                        _rVal.Cell(r, c + 1).FontColor = Math.Abs(dval) >= ConverganceLimit ? System.Drawing.Color.Red : System.Drawing.Color.Green;

                    }

                }


                for (r = 1; r <= pncnt; r++)
                {
                    for (int c = 1; c <= dccnt; c++)
                    {
                        mdSpoutArea area = Area(r, c);
                        if (area == null)
                            continue;
                        uopMatrixCell mycell = Cell(area.PanelIndex, area.DowncomerIndex);

                        int table_r = r + 1;
                        int table_c = bReverseColumns ? uopUtils.OpposingIndex(area.DowncomerIndex, DowncomerCount) : area.DowncomerIndex;
                        table_c++;
                        
                        uopTableCell cell = _rVal.Cell(table_r, table_c);
                        cell.BoldText = area.OverrideSpoutArea.HasValue;
                        cell.FontColor = area.Color.Item2;
                        uopInstances insts = area.Instances;
                        foreach (var item in insts) 
                        {
                            table_r = item.Row + 1;
                            table_c = bReverseColumns ? uopUtils.OpposingIndex(item.Col, DowncomerCount) : item.Col;
                            table_c++;
                            cell = _rVal.Cell(table_r, table_c);
                            cell.BoldText = area.OverrideSpoutArea.HasValue;
                            cell.FontColor = area.Color.Item2;

                        }
                        {

                        }
                    }
                }


                if (!bAddIdeals)
                {
                    _rVal.RemoveRow(idealrow);
                    _rVal.RemoveCol(idealcol);

                }

                if (!bAddHeaders)
                {
                    _rVal.RemoveRow(1);
                    _rVal.RemoveCol(1);
                }
                return _rVal;
            }
            catch
            {
                return _rVal;
            }




        }


        private void GetVariableMarkers()
        {
            VariableMarkers = new uopMatrix(Np, Nd, 2, "Variables", 1d);

            List<uopMatrixCell> vcells = VariableMarkers.Cells();
            if (Zone == null) return;
         
            int cmax = IsSymmetric ? Zone.DCData.FindAll((x) => !x.IsVirtual).Count : Nd;
            int rmax = cmax + 1;
            List<uopMatrixCell> changes;

            
            List<int> groupids = Zone.GetGroupIndices(out List<List<mdSpoutArea>> groupedareas);
            List<uopMatrixCell> cells = null;
            changes = new List<uopMatrixCell>();
            foreach (var group in groupedareas)
            {
                foreach (var sa in group) 
                {
                    cells = Cells(sa.PanelIndex, sa.DowncomerIndex, sa.Instances); // get the matrix cells at the same address as the group cells and mark the variable as 3
                    for(int i =1; i <= cells.Count; i++) 
                    { 
                        uopMatrixCell cell = cells[i -1];
                        if(i==1)
                        {
                            if(cell.SetValue(i==1 ?3d :0 )) changes.Add(cell);
                        }
                    }
                }
            }
            changes = uopMatrixCell.SetMatchingCellValues(vcells, FindCells((x) => x.Invisible == true), 0); // mark the out of diameter areas as constants

            List<mdSpoutArea> lockedareas = Zone.FindAll((x) => x.TreatAsIdeal);
            foreach (var sa in lockedareas)
            {
                cells = Cells(sa.PanelIndex, sa.DowncomerIndex, sa.Instances); // get the matrix cells set to ideal and mark them as constants
                changes.AddRange(uopMatrixCell.SetMatchingCellValues(vcells, cells, 3d));

            }



            DOF = Unknowns - (cmax + rmax - 1);

        }


        /// <summary>
        /// returns a collection of the row or column data as string
        /// </summary>
        /// <param name="aInvisibleReplacer"></param>
        /// <param name="aZeroReplacer"></param>
        /// <returns></returns>
        public List<string> ToStrings(int aRowStart = 0, int aRowEnd = 0, int aColStart = 0, int aColEnd = 0, string aInvisibleReplacer = "", string aZeroReplacer = null, int? aPrecis = null, string aDelimitor = "|")
        {

            List<string> _rVal = new List<string>();
            if(Size <=0) return _rVal;
            mzUtils.LoopLimits(aRowStart, aRowEnd, 1, Count, out int rs, out int re);
            mzUtils.LoopLimits(aColStart, aColEnd, 1, Cols, out int cs, out int ce);
            aInvisibleReplacer ??= string.Empty;
            int precis = aPrecis.HasValue ? mzUtils.LimitedValue(aPrecis.Value, 1, 10) : Precision;
            if (ColumnHeaders != null)
            {
              
                    string slist = string.Empty + aDelimitor;
                    for (int c = cs; c <= ce; c++)
                    {
                    string sval = string.Empty;
                    if(c<= ColumnHeaders.Count) sval = ColumnHeaders[c-1];
                        slist += sval;
                        if (c < ce) slist += aDelimitor;

                    }
                _rVal.Add(slist);

            }

            for (int r = rs; r <=re; r++) 
            {
                string slist = string.Empty;
                uopMatrixRow row = Row(r);
                if(RowHeaders != null && r <= RowHeaders.Count) slist = RowHeaders[r-1] + aDelimitor ;
                for (int c = cs; c<= ce; c++)
                {


                    uopMatrixCell cell = row[c - 1];
                    double dval = Math.Round(cell.Value, precis);
                    string sval = dval.ToString($"#,0.{new string('0', precis)}");
                    //if(c == ce) sval = dval.ToString($"E3");
                    if (cell.Invisible)
                    {
                        sval = aInvisibleReplacer;
                    } else if (dval == 0 && aZeroReplacer != null)
                    {
                        sval = aZeroReplacer;
                    }
                 

                    slist += sval;
                    if (c < ce ) slist += aDelimitor;

                }
                _rVal.Add(slist);

            }



            return _rVal;
        }

        private List<List<uopMatrixCell>> ValidateGroupsAndLocks(out List<uopMatrixCell> rLockedCells, out List<int> rGroupIndexs)
        {
            rGroupIndexs = new List<int>();
            rLockedCells = new List<uopMatrixCell>();
            List<List<uopMatrixCell>> _rVal = new List<List<uopMatrixCell>>();
            if (Zone == null) return _rVal;


            rGroupIndexs = Zone.GetGroupIndices(out List<List<mdSpoutArea>> groupedareas);  //this will bubble out missing groups and enforce the max

            List<uopMatrixCell> mycells = Cells();
            foreach (uopMatrixCell cell in mycells)
            {
                cell.GroupIndex = 0;
                cell.OverrideValue = null;
            }

            //assign group ids and override areas
            foreach (var area in Zone)
            {
                List<uopMatrixCell> cells = Cells(area.PanelIndex, area.DowncomerIndex, area.Instances);
                foreach (var cell in cells)
                {
                    cell.GroupIndex = area.GroupIndex;
                    if (area.TreatAsIdeal) cell.OverrideValue = area.OverrideSpoutArea;
                }
            }

            //return the group cells in a list of lists
            foreach (int groupid in rGroupIndexs) _rVal.Add(mycells.FindAll((x) => x.GroupIndex == groupid));

            //return all cells with an override area set 
            rLockedCells = mycells.FindAll((x) =>  !x.Invisible && x.OverrideValue.HasValue);
         
            return _rVal;
        }

     
        #endregion Methods

        #region Shared Methods

        public static int MaxSmoothingPasses => 25000;
        public static int MaxGroupings => 13;

        public static (dxxColors, System.Drawing.Color) Color_Group1 = (dxxColors.Magenta, dxfColors.ACLToWin64((int)dxxColors.Magenta));
        public static (dxxColors, System.Drawing.Color) Color_Group2 = (dxxColors.Red,  dxfColors.ACLToWin64((int)dxxColors.Red));
        public static (dxxColors, System.Drawing.Color) Color_Group3 = (dxxColors.Green, dxfColors.ACLToWin64((int)dxxColors.Green));
        public static (dxxColors, System.Drawing.Color) Color_Group4 = (dxxColors.Cyan, dxfColors.ACLToWin64((int)dxxColors.Cyan));
        public static (dxxColors, System.Drawing.Color) Color_Group5 = (dxxColors.Orange, dxfColors.ACLToWin64((int)dxxColors.Orange));
        public static (dxxColors, System.Drawing.Color) Color_Group6 = (dxxColors.LightMagenta, dxfColors.ACLToWin64((int)dxxColors.LightMagenta));
        public static (dxxColors, System.Drawing.Color) Color_Group7 = (dxxColors.LightRed, dxfColors.ACLToWin64((int)dxxColors.LightRed));
        public static (dxxColors, System.Drawing.Color) Color_Group8 = (dxxColors.LightGreen, dxfColors.ACLToWin64((int)dxxColors.LightGreen));
        public static (dxxColors, System.Drawing.Color) Color_Group9 = (dxxColors.LightCyan, dxfColors.ACLToWin64((int)dxxColors.LightCyan));
        public static (dxxColors, System.Drawing.Color) Color_Group10 = (dxxColors.LightBlue, dxfColors.ACLToWin64((int)dxxColors.LightBlue));
        public static (dxxColors, System.Drawing.Color) Color_Group11 = (dxxColors.LightBlue, dxfColors.ACLToWin64((int)dxxColors.Purple));
        public static (dxxColors, System.Drawing.Color) Color_Group12 = (dxxColors.LightBlue, dxfColors.ACLToWin64((int)dxxColors.LightPurple));
        public static (dxxColors, System.Drawing.Color) Color_Group13 = (dxxColors.LightBlue, dxfColors.ACLToWin64((int)dxxColors.DarkPurple));
        public static (dxxColors, System.Drawing.Color) Color_Ideal = (dxxColors.Blue, dxfColors.ACLToWin64((int)dxxColors.Blue));

        public static (dxxColors, System.Drawing.Color) GetGroupColor(int aGroupIndex)
        {
            if(aGroupIndex <1 || aGroupIndex > mdSpoutAreaMatrix.MaxGroupings) throw new IndexOutOfRangeException($"Group ID must be form 1 to {mdSpoutAreaMatrix.MaxGroupings}");
  
                   return aGroupIndex switch
                   {
                       1=> mdSpoutAreaMatrix.Color_Group1,
                       2 => mdSpoutAreaMatrix.Color_Group2,
                       3 => mdSpoutAreaMatrix.Color_Group3,
                       4 => mdSpoutAreaMatrix.Color_Group4,
                       5 => mdSpoutAreaMatrix.Color_Group5,
                       6 => mdSpoutAreaMatrix.Color_Group6,
                       7 => mdSpoutAreaMatrix.Color_Group7,
                       8 => mdSpoutAreaMatrix.Color_Group8,
                       9 => mdSpoutAreaMatrix.Color_Group9,
                       10 => mdSpoutAreaMatrix.Color_Group10,
                       11 => mdSpoutAreaMatrix.Color_Group11,
                       12 => mdSpoutAreaMatrix.Color_Group12,
                       13 => mdSpoutAreaMatrix.Color_Group13,
                       _ => (dxxColors.Yellow, dxfColors.ACLToWin64((int)dxxColors.Yellow))
                   };
        }
        #endregion Shared Methods
    }

    public class mdSpoutAreaMatrices : List<mdSpoutAreaMatrix>, IEnumerable<mdSpoutAreaMatrix>, ICloneable
    {

        #region Constructors

        public mdSpoutAreaMatrices() { }

        public mdSpoutAreaMatrices(mdTrayAssembly aAssy, bool bDistributeArea = true, mdSpoutZones aZones = null)
        {
            if (aAssy == null) { throw new ArgumentNullException("An MD Tray Assembly is required to create a valid mdSpoutAreaMatrices list"); }

            Zones = aZones ==null ? new mdSpoutZones(aAssy, aAssy._Constraints) : aZones;
            AddRange(  Zones.Matrices(bDistributeArea));

        }

        public mdSpoutAreaMatrices(mdSpoutAreaMatrices aMatrices)
        {
            if (aMatrices == null) return;
            foreach (var item in aMatrices) Add(new mdSpoutAreaMatrix(item));
        }

        #endregion Constructors

        #region Properties

        public mdSpoutZones Zones { get; set; }

        public colMDConstraints Constraints => Zones == null ? null : Zones.Constraints;
         


        public bool IsConverged 
        {
            get
            {
                if(Count == 0) return false;
                foreach(var matix in this)
                {
                    if(!matix.IsConverged) return false;
                }
                return true;
            }
        }

        public double MaximimDeviation => Math.Max(MaximumDowncomerDeviation, MaximumPanelDeviation);

        public double MaximumDowncomerDeviation
        {
            get
            {
                if (Count == 0) return 0;
                double _rVal = double.MinValue;
                foreach (var matrix in this)
                {
                    if (Math.Abs(matrix.MaxDowncomerDeviation) > _rVal) _rVal = Math.Abs(matrix.MaxDowncomerDeviation);
                }
                return _rVal;
            }
        }

        public double MaximumPanelDeviation
        {
            get
            {
                if (Count == 0) return 0;
                double _rVal = double.MinValue;
                foreach (var matrix in this)
                {
                    if (Math.Abs(matrix.MaxPanelDeviation) > _rVal) _rVal = Math.Abs(matrix.MaxPanelDeviation);
                }
                return _rVal;
            }
        }

        #endregion Properties

        #region Methods

        public mdSpoutAreaMatrices Clone() => new mdSpoutAreaMatrices(this);

        object ICloneable.Clone() => (object)this.Clone();

        public double MaxDeviation(int aZoneIndex, bool bReturnPanel)
        {
                if (Count == 0) return 0;
                if(aZoneIndex <1 || aZoneIndex > Count) return 0;
               
                 return   !bReturnPanel ? this[aZoneIndex - 1].MaxDowncomerDeviation : this[aZoneIndex - 1].MaxPanelDeviation;
            
         
        }

        public void UpdateSpoutGroupAreas(mdTrayAssembly aAssy = null)
        {
            for (int i =1; i <= Count; i++)
            {
                this[i - 1].UpdateSpoutGroupAreas(aAssy);
            }
        }

        public bool IsEqual(mdSpoutAreaMatrices aMatrices, int? aPrecis = null)
        {
            if(aMatrices == null) return false;
            if(aMatrices.Count != Count) return false;
            int prec = !aPrecis.HasValue ? 10 : mzUtils.LimitedValue(aPrecis.Value, 0, 15);

            
            for (int i = 1; i <= Count; i++) 
            {
                mdSpoutAreaMatrix mine = this[i - 1];
                mdSpoutAreaMatrix hers  = aMatrices[i -1];
                
                if (!mine.IsEqualTo(hers, prec)  ) return false;

            }
            return true;
        }

        public void DistributeSpoutArea(mdTrayAssembly aAssy, colMDSpoutGroups aSpoutGroups , bool bVerbose = false, uopDocuments aWarnings = null) 
        {

            List<mdSpoutGroup> groups = aSpoutGroups == null ? new List<mdSpoutGroup>() : aSpoutGroups.ToList();
            foreach(mdSpoutAreaMatrix matrix in this)
            {
                matrix.UpdateSpoutGroupAreas(aAssy, aSpoutGroups, aWarnings);
                //matrix.SpoutGroupAreas.PrintToConsole();

                matrix.DistributeSpoutArea( bVerbose, aWarnings);
                
                //matrix.LockedAreas.PrintToConsole(aPrecis:3);
                
                List<mdSpoutGroup> zgroups = groups.FindAll((x) => x.ZoneIndex == matrix.Zone.Index);
                foreach (mdSpoutGroup group in zgroups)
                {
                    mdSpoutArea sa = matrix.Area(group.PanelIndex, group.DowncomerIndex, true);
                    if (sa != null)
                    {
                        uopMatrixCell cell = matrix.Cell(sa.PanelIndex, sa.DowncomerIndex);
                        mdConstraint constrnt = group.Constraints(aAssy);
                        bool wuz = group.SuppressEvents;
                        group.SuppressEvents = true;
                        group.SpoutArea = sa;
                        group.TheoreticalArea = sa.IdealSpoutArea;
                        group._Grid.TargetArea = group.TheoreticalArea;
                        if (constrnt != null)
                        {
                            bool wuz1 = constrnt.SuppressEvents;
                            constrnt.SuppressEvents = true;
                            constrnt.GroupIndex = cell.GroupIndex;
                            constrnt.TreatAsIdeal = cell.OverrideValue.HasValue;
                            if (constrnt.TreatAsIdeal) 
                            {
                                constrnt.OverrideSpoutArea = cell.OverrideValue.Value;
                                group._Grid.TargetArea = cell.OverrideValue.Value;

                            }

                            constrnt.SuppressEvents = wuz1;
                        }
                        group.SuppressEvents = wuz;
                    }
                    else
                    {
                        Console.WriteLine($"Spout Area {group.Handle} Was Not Found");
                    }
                }
            }
        
        }

        #endregion Methods
    }

}
