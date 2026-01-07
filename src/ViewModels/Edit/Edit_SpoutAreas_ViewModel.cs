using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using iText.StyledXmlParser.Css.Media;
using Microsoft.Office.Interop.Excel;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.MessageBox;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using UOP.DXFGraphics;
using UOP.DXFGraphicsControl;
using UOP.WinTray.Projects;
using UOP.WinTray.Projects.Documents;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Parts;

using UOP.WinTray.Projects.src.Utilities;
using UOP.WinTray.Projects.Tables;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;
using UOP.WinTray.UI.BusinessLogic;
using UOP.WinTray.UI.Commands;
using UOP.WinTray.UI.Messages;
using UOP.WinTray.UI.Views;
using UOP.WinTray.UI.Views.Windows.CADfx.Questions;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskBand;
using media = System.Windows.Media;

namespace UOP.WinTray.UI.ViewModels
{
    public class Edit_SpoutAreas_ViewModel : MDProjectViewModelBase, IModalDialogViewModel
    {

        
        #region Events

        public event EventHandler Event_ColorChanged;
        public event EventHandler Event_IndexChanged;


        #endregion Events

        #region Constructors




        internal Edit_SpoutAreas_ViewModel(IDialogService dialogService
                                             ) : base(null, dialogService)
        {


            Edited = false;
            Canceled = true;


            Image = null;
            //if(project == null)
            //{
            WinTrayMainViewModel main = WinTrayMainViewModel.WinTrayMainViewModelObj;

            MDProject = main.MDProject;// project;

            //}
            //else
            //{
            //    MDProject = project;
            //}
            if (MDRange == null) return;


            IsOkBtnEnable = true;
            Title = $"Edit Spout Area Distribution - {MDProject.Name} ({MDRange.Name(true)})";

            IsEnglishSelected = MDProject.DisplayUnits == uppUnitFamilies.English;

            MenuItems = new ObservableCollection<System.Windows.Controls.MenuItem>();
      
            

        }


        #endregion Constructors

        #region mouse event response

        private System.Windows.Point DownPoint { get; set; }
        private System.Windows.Point CurPoint { get; set; }
        private bool Init { get; set; }
        public string Control { get; set; }
        #endregion  mouse event response

        #region Properties 

        private string LastGroupSelected { get; set; }
        
         
        public ObservableCollection<System.Windows.Controls.MenuItem> MenuItems { get; set; }
        

        private mdSpoutAreaMatrices _Matrices;
        private mdSpoutAreaMatrices Matrices 
        { get => _Matrices;
            set
            { 
                    _Matrices = value;
                if (_Matrices != null) 
                {
                    mdSpoutAreaMatrix matrix = CurrentMatrix;
                    
                }
            }
        }

        private int _ZoneIndex = 1;
        public int ZoneIndex
        {
            get => _ZoneIndex;
            set 
            {
                bool newvalue = _ZoneIndex != value;
                _ZoneIndex = value; 
                NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod());
                _Zone1Selected = value == 1;
                NotifyPropertyChanged("Zone1Selected");
                _Zone2Selected = ! _Zone1Selected;
                NotifyPropertyChanged("Zone2Selected");

                if (newvalue && !_Refreshing) Execute_Refresh(bSuppressRecalc: true, bRedraw: true);
            }
        }
        private bool _Zone1Selected = true;
        public bool Zone1Selected
        {
            get => _Zone1Selected;
            set
            {
                bool newvalue = _Zone1Selected != value;
                _Zone1Selected = value;
                NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod());
                _Zone2Selected = !value;
                NotifyPropertyChanged("Zone2Selected");
                _ZoneIndex = value ? 1 : 2;
                NotifyPropertyChanged("ZoneIndex");
                if (newvalue && !_Refreshing) Execute_Refresh(bSuppressRecalc: true, bRedraw: true);
            }
        }
        private bool _Zone2Selected = true;
        public bool Zone2Selected
        {
            get => _Zone2Selected;
            set
            {
                bool newvalue = _Zone2Selected != value;
                _Zone2Selected = value;
                NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod());
                _Zone1Selected = !value;
                NotifyPropertyChanged("Zone1Selected");
                _ZoneIndex = value ? 2 : 1;
                NotifyPropertyChanged("ZoneIndex");
                if (newvalue && !_Refreshing) Execute_Refresh(bSuppressRecalc: true, bRedraw: true);
            }
        }

        public mdSpoutZone CurrentZone
        {
            get
            {
                if(Matrices == null) return null;
                if(ZoneIndex <= 0) ZoneIndex =1;
                if (ZoneIndex > Matrices.Count) ZoneIndex = Matrices.Count;
                return Matrices[ZoneIndex - 1].Zone;
            }
        }

        public mdSpoutAreaMatrix CurrentMatrix
        {
            get
            {
                if (Matrices == null) return null;
                if (ZoneIndex <= 0) ZoneIndex = 1;
                if (ZoneIndex > Matrices.Count) ZoneIndex = Matrices.Count;
                return Matrices[ZoneIndex - 1];
            }
        }

        public bool IsSymmetric => MDAssy == null ? false : MDAssy.IsSymmetric;

        public bool? Symmetric => IsSymmetric ? true : null;

        private int _ColumnIndex;
        public int ColumnIndex
        {
            get => _ColumnIndex;
            set
            {
                _ColumnIndex = value;
   
            }
        }
        private int _RowIndex;
        public int RowIndex
        {
            get => _RowIndex;
            set
            {
                _RowIndex = value;
    
            }
        }

        public List<mdSpoutArea> SelectedAreas
        {
            get
            {
                List<mdSpoutArea> _rVal = new List<mdSpoutArea>();
                mdSpoutAreaMatrix matrix = CurrentMatrix;
                if(matrix == null)  return _rVal;
                mdSpoutArea area = matrix.Area(RowIndex,ColumnIndex);
                if (area != null)
                {
                    _rVal.Add(area);
                    uopInstances insts = area.Instances;
                    foreach (var inst in insts) 
                    {
                        mdSpoutArea sister = area = new mdSpoutArea(area) { Row = inst.Row, Col = inst.Col, PanelIndex = inst.Row, DowncomerIndex = inst.Col, IsVirtual = true };
                        sister.Move(inst.DX, inst.DY);
                        _rVal.Add(sister);
                    }
                    
                }
                return _rVal;
            }
        } 

        private string _Converged;
        public string Converged
        {
            get => _Converged;
            set { _Converged = value; NotifyPropertyChanged("Converged"); }
        }

        private string _BColor;
        public string BColor
        {
            get => _BColor;
            set { _BColor = value; NotifyPropertyChanged("BColor"); }
        }

        public int DCTot => Matrices == null ? 0 : CurrentMatrix.DowncomerCount;
        public int DPTot => DCTot > 0 ? DCTot + 1 : 0;

       
        public bool ChangesDisplayed { get; set; }

        public bool DontTrackGroup { get; set; }
        public bool Loading { get; set; }
        public bool GoodSolution { get; set; }

        private bool _IsEnabled;
        public override bool IsEnabled
        {
            get => _IsEnabled;
            set { _IsEnabled = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        public override bool IsEnglishSelected
        {
            get => base.IsEnglishSelected;
            set { base.IsEnglishSelected = value; DataTable = null; if (!_Refreshing) Execute_Refresh(true); }
        }

        public List<dxePolyline> DXFBounds { get; set; }

        /// <summary>
        /// //the areas in the display that respond to clicks on spoutareas
        /// </summary>
        private uopRectangles _AreaRegions;
        public uopRectangles AreaRegions
        {
            get 
            {   if(_AreaRegions == null)
                {
                    mdSpoutAreaMatrix matrix = CurrentMatrix;
                    _AreaRegions = uopRectangles.Copy( matrix == null ?null : matrix.BoundingRectangles(true));

                }
                return _AreaRegions;
            }
        }

        public uopRectangles NonVirtualAreaRegions
        {
            get
            {
                uopRectangles _rVal = new uopRectangles(AreaRegions.FindAll((x) => !x.IsVirtual));
               
                return _rVal;
        
            }
        }

        private uopRectangle _SelectedRegion = null;

        public uopRectangle SelectedRegion 
        { 
            get => _SelectedRegion;
            set 
            {
                _SelectedRegion = value;
                if(_SelectedRegion != null) 
                {
                   
                    if (_SelectedRegion.IsVirtual) 
                        _SelectedRegion = AreaRegions.Find((x) => x.Name == value.Name &&  !x.IsVirtual);

                    CurrentZone.SetSelected(_SelectedRegion.Name);
                }
            
            }
        
        }


        private string _ConvergeLimit;
        public string ConvergeLimit
        {
            get => _ConvergeLimit;
            set { _ConvergeLimit = value; NotifyPropertyChanged("ConvergeLimit"); }
        }

        private string _DOF;
        public string Dof
        {
            get => _DOF;
            set { _DOF = value; NotifyPropertyChanged("Dof"); }
        }

        private bool? _DialogResult;

        /// <summary>
        /// Dialogservice result
        /// </summary>
        public bool? DialogResult
        {
            get => _DialogResult;
            private set { _DialogResult = value; NotifyPropertyChanged("DialogResult"); }
        }

        public uopTable DataTable { get; set; }

  
        private System.Data.DataTable _DataTableCollection;
        public System.Data.DataTable DataTableCollection
        {
            get => _DataTableCollection;
            set { _DataTableCollection = value; NotifyPropertyChanged("DataTableCollection"); }

        }

        public override DXFViewer Viewer
        {
            get => base.Viewer;
            set
            {
                bool newviewer = base.Viewer != value;

                if (base.Viewer != null)
                {
                    base.Viewer.OnViewerMouseDown -= Viewer_OnViewerMouseDown;
                    base.Viewer.MouseMove -= Viewer_MouseMove;
                    base.Viewer.MouseUp -= Viewer_MouseUp;
                    base.Viewer.ContextMenuOpening -= Viewer_ContextMenuOpening;
                }
                base.Viewer = value;

                if (base.Viewer == null) 
                    return;

                base.Viewer.ZoomIsDisabled = false;
                base.Viewer.PanIsDisabled = true;
                base.Viewer.OnViewerMouseDown += Viewer_OnViewerMouseDown;
                base.Viewer.MouseMove += Viewer_MouseMove;
                base.Viewer.MouseUp += Viewer_MouseUp;
                base.Viewer.ContextMenuOpening += Viewer_ContextMenuOpening;
            }

        }


        /// <summary>
        /// calcultes DOF
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns>dof count</returns>
        public int DOF
        {
            get
            {


                if (Matrices == null) return 0;

                return CurrentMatrix.DOF;

            }


        }

        private dxfBlock PanelBlock { get; set; }


        public override dxfImage Image
        {
            get => _Image;
            set
            {
                if (_Image != null && value == null) _Image.Dispose();
                if (_Image != null && value != null)
                {
                    if (_Image.GUID != value.GUID) { _Image.Dispose(); }
                }
                ;
                _Image = value;

                NotifyPropertyChanged("Image");
            }
        }
        
        private  mdSpoutAreaMatrices OriginalMatrices { get; set; }

        private mzQuestions _Query_Action;

        public mzQuestions Query_Action
        {
            get
            {
                if (_Query_Action == null)
                {
                    _Query_Action = new mzQuestions();
                    _Query_Action.AddStringChoice("Select The Action To Perform", new List<string> { "Clear Selection Set","Clear Constraints", "Make Ideal", "Add To Group" }, "Add To Group", aToolTip: "Actions");
                }
                    

                return _Query_Action;
            }
        }

        private mzQuestions _Query_Clear;

        public mzQuestions Query_Clear
        {
            get
            {
                if (_Query_Clear == null)
                {
                
                    _Query_Clear = new("Clear Constraints?");
                    _Query_Clear.AddCheckVal("Clear Ideal Areas?", aInitialAnswer: true);
                    _Query_Clear.AddCheckVal("Clear Ideal Groups?", aInitialAnswer: true);

       
                }

                return _Query_Clear;
            }
        }

        private Visibility _Visibility_Group1;
        public Visibility Visibility_Group1
        {
            get => _Visibility_Group1;
            set { _Visibility_Group1 = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private Visibility _Visibility_Group2;
        public Visibility Visibility_Group2
        {
            get => _Visibility_Group2;
            set { _Visibility_Group2 = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private Visibility _Visibility_Group3;
        public Visibility Visibility_Group3
        {
            get => _Visibility_Group3;
            set { _Visibility_Group3 = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private Visibility _Visibility_Group4;
        public Visibility Visibility_Group4
        {
            get => _Visibility_Group4;
            set { _Visibility_Group4 = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private Visibility _Visibility_Group5;
        public Visibility Visibility_Group5
        {
            get => _Visibility_Group5;
            set { _Visibility_Group5 = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private Visibility _Visibility_Group6;
        public Visibility Visibility_Group6
        {
            get => _Visibility_Group6;
            set { _Visibility_Group6 = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private Visibility _Visibility_Group7;
        public Visibility Visibility_Group7
        {
            get => _Visibility_Group7;
            set { _Visibility_Group7 = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private Visibility _Visibility_Group8;
        public Visibility Visibility_Group8
        {
            get => _Visibility_Group8;
            set { _Visibility_Group8 = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private Visibility _Visibility_Group9;
        public Visibility Visibility_Group9
        {
            get => _Visibility_Group9;
            set { _Visibility_Group9 = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private Visibility _Visibility_Group10;
        public Visibility Visibility_Group10
        {
            get => _Visibility_Group10;
            set { _Visibility_Group10 = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }
        private Visibility _Visibility_Group11;
        public Visibility Visibility_Group11
        {
            get => _Visibility_Group11;
            set { _Visibility_Group11 = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private Visibility _Visibility_Group12;
        public Visibility Visibility_Group12
        {
            get => _Visibility_Group12;
            set { _Visibility_Group12 = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private Visibility _Visibility_Group13;
        public Visibility Visibility_Group13
        {
            get => _Visibility_Group13;
            set { _Visibility_Group13 = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }
        private Visibility _Visibility_MultiMatrices;
        public Visibility Visibility_MultiMatrices
        {
            get => _Visibility_MultiMatrices;
            set { _Visibility_MultiMatrices = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        public media::SolidColorBrush Ideal_Color
        {
            get => ColorToBrush(mdSpoutAreaMatrix.Color_Ideal.Item2);
        }


        public media::SolidColorBrush Group1_Color
        {
            get => ColorToBrush(mdSpoutAreaMatrix.Color_Group1.Item2);
        }
       
        public media::SolidColorBrush Group2_Color
        {
            get => ColorToBrush(mdSpoutAreaMatrix.Color_Group2.Item2);
        }
        
        public media::SolidColorBrush Group3_Color
        {
            get => ColorToBrush(mdSpoutAreaMatrix.Color_Group3.Item2);
        }
        
        public media::SolidColorBrush Group4_Color
        {
            get => ColorToBrush(mdSpoutAreaMatrix.Color_Group4.Item2);
        }

        public media::SolidColorBrush Group5_Color
        {
            get => ColorToBrush(mdSpoutAreaMatrix.Color_Group5.Item2);
        }

        public media::SolidColorBrush Group6_Color
        {
            get => ColorToBrush(mdSpoutAreaMatrix.Color_Group6.Item2);
        }

        public media::SolidColorBrush Group7_Color
        {
            get => ColorToBrush(mdSpoutAreaMatrix.Color_Group7.Item2);
        }

        public media::SolidColorBrush Group8_Color
        {
            get => ColorToBrush(mdSpoutAreaMatrix.Color_Group8.Item2);
        }

        public media::SolidColorBrush Group9_Color
        {
            get => ColorToBrush(mdSpoutAreaMatrix.Color_Group9.Item2);
        }

        public media::SolidColorBrush Group10_Color
        {
            get => ColorToBrush(mdSpoutAreaMatrix.Color_Group10.Item2);
        }
        public media::SolidColorBrush Group11_Color
        {
            get => ColorToBrush(mdSpoutAreaMatrix.Color_Group11.Item2);
        }
        public media::SolidColorBrush Group12_Color
        {
            get => ColorToBrush(mdSpoutAreaMatrix.Color_Group12.Item2);
        }
        public media::SolidColorBrush Group13_Color
        {
            get => ColorToBrush(mdSpoutAreaMatrix.Color_Group13.Item2);
        }

        public bool InvertDowncomerColumns => true;

        private bool _LeftShiftDown = false;
        private bool _RightShiftDown = false;
        private bool _LeftControlDown = false;
        private bool _RightControlDown = false;

        public bool ShiftIsDown => _LeftShiftDown || _RightShiftDown;
        public bool ControlIsDown => _LeftControlDown || _RightControlDown;


        #endregion Properties

        #region Commands
        private DelegateCommand _CMD_Cancel;
        public ICommand Command_Cancel { get { _CMD_Cancel ??= new DelegateCommand(param => Execute_Cancel()); return _CMD_Cancel; } }


        private DelegateCommand _CMD_Clear;
        public ICommand Command_Clear { get { _CMD_Clear ??= new DelegateCommand(param => Execute_Clear()); return _CMD_Clear; } }


        private DelegateCommand _CMD_Reset;
        public ICommand Command_Reset { get { _CMD_Reset ??= new DelegateCommand(param => Execute_Reset()); return _CMD_Reset; } }


        private DelegateCommand _CMD_Refresh;
        public ICommand Command_Refresh { get { _CMD_Refresh ??= new DelegateCommand(param => Execute_Refresh()); return _CMD_Refresh; } }


        private DelegateCommand _CMD_Ok;
        public ICommand Command_Ok { get { _CMD_Ok ??= new DelegateCommand(param => Execute_Save()); return _CMD_Ok; } }


        private DelegateCommand _CMD_ClearConstraints;
        public ICommand Command_ClearConstraints { get { _CMD_ClearConstraints ??= new DelegateCommand(param => Execute_ClearConstraints()); return _CMD_ClearConstraints; } }

        private DelegateCommand _CMD_MakeIdeal;
        public ICommand Command_MakeIdeal { get { _CMD_MakeIdeal ??= new DelegateCommand(param => Execute_MakeIdeal()); return _CMD_MakeIdeal; } }

        private DelegateCommand _CMD_AddToGroup;
        public ICommand Command_AddToGroup { get { _CMD_AddToGroup ??= new DelegateCommand(param => Execute_AddToGroup()); return _CMD_AddToGroup; } }

        #endregion Commands



        #region Methods

        public void Execute_ClearConstraints()
        {

            
            mdSpoutAreaMatrix matrix = CurrentMatrix;
            if (matrix == null) return;
            List<mdSpoutArea> areas = SelectedAreas;
            if (areas == null) return;
            mdSpoutArea area = areas.Find((x) => !x.IsVirtual);
            if (area == null) return;
            //int pidx = area.PanelIndex;
            //int didx = area.DowncomerIndex;

            bool refresh =matrix.SetLockValue(area.Handle, false) ;
            if (matrix.SetGroupIndex(area.Handle, 0)) refresh = true;

            //if (MDAssy.DesignFamily.IsBeamDesignFamily())
            //{
            //           bool? symmetric = Symmetric;

            //    int currentGroup = (int)matrix.GroupedAreas.GetMember(pidx, didx);

            //    (bool groupFlagsRes, _) = AssignToGroup(pidx, didx, currentGroup, 0, symmetric, true);
            //    bool idealFlagsRes = refresh; 

            //    bool requiresGroupRenumbering = RequiresGroupReNumbering();
            //    if (requiresGroupRenumbering)
            //    {
            //        MakeGroupNumbersSequential();
            //    }

            //    refresh = groupFlagsRes || idealFlagsRes || requiresGroupRenumbering;
            //}
            //else
            //{

            //    bool? symmetric = Symmetric;

            //    refresh = refresh ||  matrix.SetGroupIndex(pidx, didx, 0);
            //    //if (IdealFlags.SetMember(pidx, didx, -1, symmetric, true)) refresh = true;
            //}
            // if (refresh)
            if (!_Refreshing) Execute_Refresh();

        }

        public void Execute_MakeIdeal()
        {
            List<mdSpoutArea> areas = SelectedAreas;
            if (areas.Count <= 0) return;
            mdSpoutAreaMatrix matrix = CurrentMatrix;

            mdSpoutArea area = areas.Find((x) => !x.IsVirtual);
            if(area == null) return;

            matrix.SetLockValue(area.Handle, true);
            if (!_Refreshing) Execute_Refresh();

            //if (MDAssy.DesignFamily.IsBeamDesignFamily())
            //{
            //    if (SelectedRegion == null) return;
            //    if (!ExtractIndices(SelectedRegion.Name, out int didx, out int pidx)) return;

            //    bool refresh;
            //    bool? symmetric = Symmetric;

            //    int currentGroup = (int)GroupFlags.GetMember(pidx, didx);

            //    (bool groupFlagsRes, _) = AssignToGroup(pidx, didx, currentGroup, 0, symmetric, true);
            //    bool idealFlagsRes = IdealFlags.SetMember(pidx, didx, 0, symmetric, true);

            //    bool requiresGroupRenumbering = RequiresGroupReNumbering();
            //    if (requiresGroupRenumbering)
            //    {
            //        MakeGroupNumbersSequential();
            //    }

            //    refresh = groupFlagsRes || idealFlagsRes || requiresGroupRenumbering;
            //    if (refresh) Execute_Refresh();
            //}
            //else
            //{
            //    if (SelectedRegion == null) return;
            //    if (!ExtractIndices(SelectedRegion.Name, out int didx, out int pidx)) return;

            //    bool refresh;
            //    bool? symmetric = Symmetric;

            //    refresh = GroupFlags.SetMember(pidx, didx, 0, symmetric, true);
            //    if (IdealFlags.SetMember(pidx, didx, 0, symmetric, true)) refresh = true;

            //    if (refresh) Execute_Refresh();
            //}
        }

        public void Execute_Reset()
        {
            try
            {
                if (MDAssy == null) return;


                MessageBoxSettings settings = new()
                {
                    Caption = "Reset Current Constraints?",
                    MessageBoxText = "Reset To Original Constraints?",
                    Button = MessageBoxButton.OKCancel,
                    Icon = MessageBoxImage.Question,
                    DefaultResult = MessageBoxResult.Cancel
                };

                if (DialogService.ShowMessageBox(this, settings) == MessageBoxResult.Cancel) return;

                IsEnabled = false;
                BusyMessage = "Resetting...";

                Loading = true;

                Matrices = new mdSpoutAreaMatrices(MDAssy);
                //Areas = DataObject.GenerateAreaMatrix(MDAssy, Locks,Groups);

                ChangesDisplayed = false;

                if (!_Refreshing) Execute_Refresh();


            }
            finally
            {

            }
        }

        public void Execute_AddToGroup()
        {

            mdSpoutAreaMatrix matrix = CurrentMatrix;
            if (matrix == null) return;


            
            if (SelectedRegion == null) return;
           

            mdSpoutArea area = matrix.Area(SelectedRegion.Name);
           
            if (area == null) return;

            int selectedGroupId = 1;
            if (!MDAssy.DesignFamily.IsStandardDesignFamily())
            {
                mzQuestions query = new();
                mzQuestion groupQuestion = query.AddStringChoice("Please select the group to which you like to add:", matrix.Zone.GetGroupNames(out _, true), LastGroupSelected, false, ",");
                bool isCanceled = !query.PromptForAnswers("Add to Group?", aSaveButtonText: "Select");
                if (isCanceled || string.IsNullOrWhiteSpace(groupQuestion.AnswerS)) return;
                selectedGroupId = int.Parse(groupQuestion.AnswerS.Replace("Group ", ""));
                LastGroupSelected = groupQuestion.AnswerS;
            }

            matrix.SetGroupIndex(area.Handle, selectedGroupId);
            Execute_Refresh();
        }

        private bool _Refreshing = false;
        internal async void Execute_Refresh(bool bSuppressRecalc = false, bool bRedraw = false)
        {

            if(_Refreshing) 
                return;
  
            mdSpoutAreaMatrix matrix = CurrentMatrix;
            if (matrix == null) return;
            

            Warnings = new uopDocuments();

            try
            {
                _Refreshing = true;

          
             
                if (bRedraw)  // the zone has changed
                {
                    PanelBlock = null;
                    _AreaRegions = null;
                    Execute_DrawImage();

                }

                IsOkBtnEnable = false;
                //e.Result = arg;
                Loading = true;
                Visibility_Progress = Visibility.Visible;
                IsEnabled = false;
                StatusText = "Refreshing Data";

                await Task.Run(() =>
                {

                    try
                    {
                        if (!bSuppressRecalc)
                        {
                            StatusText = "Applying Constraints";
                            Execute_RedistributeArea();
                            ChangesDisplayed = false;
                        }
                    }
                    catch (Exception ex) { SaveWarning(ex, System.Reflection.MethodBase.GetCurrentMethod()); }
                    finally
                    {
                        StatusText = string.Empty;
                    }

                });

                try
                {
                    StatusText = "Loading Table";

                    //load the table
                    Execute_ShowMatrix();

                    if (DXFBounds != null)
                    {
                        StatusText = "Updating Image";
                        
                        //update the perimeter shape colors
                        foreach (dxePolyline aPl in DXFBounds)
                        {
                            try
                            {

                           
                                mdSpoutArea area = CurrentMatrix.Area(aPl.Flag);

                                bool updateit = false;
                                if (area != null)
                                {
                                    updateit = aPl.Color != area.Color.Item1;
                                    if (updateit)
                                    {
                                        aPl.Color = area.Color.Item1;
                                        aPl.UpdateImage(false);
                                        Viewer.UpdateEntity(aPl);
                                    }
                                }
                            }
                            catch (Exception ex) { SaveWarning(ex, System.Reflection.MethodBase.GetCurrentMethod()); }

                        }
                    }
                }
                catch (Exception ex) { SaveWarning(ex, System.Reflection.MethodBase.GetCurrentMethod()); }


            }
            finally
            {
                StatusText = string.Empty;

                Execute_UpdateControlVisibility();
                Execute_HighlightSelectedRegions();
                IsEnabled = true;
                IsOkBtnEnable = true;
                Loading = false;
                Visibility_Progress = Visibility.Collapsed;
                _Refreshing = false;

                if (Warnings.Count > 0) ShowWarnings();

            }

        
            //_Worker_Refresh.RunWorkerAsync(new RefreshInput_SpoutAreas(bSuppressRecalc));
        }

        /// <summary>
        /// method for cancel
        /// </summary>
        private void Execute_Cancel()
        {
            Canceled = true;

            DialogResult = false;
        }

        /// <summary>
        /// method for clear
        /// </summary>
        private void Execute_Clear()
        {
            bool canceld = false;
            try
            {
                
                mdSpoutAreaMatrix matrix = CurrentMatrix;
                if(matrix == null) return;
                mdSpoutZone zone = matrix.Zone;

                bool refresh = zone.FindAll((x) => x.TreatAsIdeal).Count > 0;
                if ( zone.FindAll((x) => x.GroupIndex >0).Count > 0) refresh = true;
                if (!refresh && !uopUtils.RunningInIDE) return;

                //mzQuestions query = new("Clear Current Constraints?");
                //mzQuestion q1 = query.AddCheckVal("Clear Ideal Areas?", aInitialAnswer: true);
                //mzQuestion q2 = query.AddCheckVal("Clear Ideal Groups?", aInitialAnswer: true);
                 canceld = !Query_Clear.PromptForAnswers("Clear Current Constraints?", aSaveButtonText: "Apply");
                if (!Query_Clear.AnswerB(1,false) && !Query_Clear.AnswerB(2, false)) canceld = true;


                if (canceld) return;
                if(!matrix.ClearConstraints(Query_Clear.AnswerB(1, false), Query_Clear.AnswerB(2, false))) canceld =  true;

            }
            finally
            {
                if(!canceld && !_Refreshing) Execute_Refresh();
            }



        }

        private void Execute_Save()
        {
            IsOkBtnEnable = false;
            if (MDAssy == null)
            {
                Execute_Cancel();
                return;
            }
            //if (!ChangesDisplayed) Execute_RedistributeArea();


            //check for good solution
            if (!GoodSolution)
            {
                System.Windows.MessageBox.Show("The Current Result Matrix Yields Spout Area Deviations That Exceed The Maximum Limits.", "Invalid Solution Set");
                IsOkBtnEnable = true;
                return;

            }


           Edited = MDAssy.SaveSpoutAreaFlags(Matrices);
            Canceled = false;
            IsOkBtnEnable = true;
            DialogResult = true;

        }

        /// <summary>
        /// method for Setting Shape Location
        /// </summary>
  
        public void Execute_HighlightSelectedRegions()
        {

            try
            {
                if (Viewer == null) return;
                var overlay = Viewer.overlayHandler;
                overlay.ClearRectangles();

                if (AreaRegions == null || AreaRegions.Count <=0) return;
                List<uopRectangle> regions = AreaRegions.FindAll(x => x.Selected);
                if(regions.Count == 0)
                {
                    return;
                    AreaRegions[0].Selected = true;
                    regions = AreaRegions.FindAll(x => x.Selected);
                }
                mdSpoutAreaMatrix matrix = CurrentMatrix;

                
                foreach (uopRectangle box in regions)
                {
                    mdSpoutArea area = matrix.Zone.Find((x) => x.Handle == box.Name);
                    if (area == null)
                    {
                        box.Selected = false;
                        continue;
                    }
                 
                    area.Selected = true;
                    ColumnIndex = area.DowncomerIndex;
                    RowIndex = area.PanelIndex;

                    OnIndexChanged();
                    double paperscale = Image.Display.PaperScale;
                    dxfRectangle rec = box.ToDXFRectangle();
                    rec = rec.Stretched(0.1 * paperscale, true, true);
                    overlay.DrawRectangle(rec, System.Drawing.Color.Yellow, 100, ignoreExisting: true);

                    if (matrix.Zone.DesignFamily.IsStandardDesignFamily())
                    {
                        uopInstances insts = area.Instances;
                        foreach (var inst in insts)
                        {
                            area = matrix.Area(inst.Row, inst.Col);

                            dxfRectangle box2 = new dxfRectangle(rec);
                            box2.Move(inst.DX, inst.DY);

                            if (box2 != null)
                            {

                                overlay.DrawRectangle(box2, System.Drawing.Color.LightSalmon, 100, ignoreExisting: true);
                            }
                        }
                    }
                }

                //mdSpoutZone zone = CurrentZone;
                //if (aRegion == null && zone != null)
                //{
                //    mdSpoutArea sa = zone.SelectedArea;
                //    if (sa != null) aRegion = new uopRectangle(sa.Limits()) { Tag = $"{sa.DowncomerIndex},{sa.PanelIndex}", Name = sa.Handle };
                //}

                //if (aRegion == null)
                //    return;

                //zone.SetSelected(aRegion.Name);

                //SetShapeLocation(aRegion);
            }
            catch (Exception ex) { SaveWarning(ex, System.Reflection.MethodBase.GetCurrentMethod()); }


        }


        public void Execute_ExecuteAction()
        {
            
            mdSpoutAreaMatrix matrix = CurrentMatrix;
            if (AreaRegions == null || matrix == null) return;
            mdSpoutZone zone = matrix.Zone;

            List<uopRectangle> regions = AreaRegions.FindAll((x) => !x.IsVirtual && x.Selected);    //just get the non virtual regions
            if (regions.Count <= 0) return;


            bool bChanges = false;
            try
            {


                bool canceled = !Query_Action.PromptForAnswers($"Select Action For {regions.Count} Spout Regions", true, aSaveButtonText: "Select");
                if (canceled) return;
                string action = Query_Action.AnswerS(1, string.Empty).ToUpper();
                if (string.IsNullOrWhiteSpace(action)) return;


                SelectedRegion = regions[0];
                if (action.Contains("GROUP")) // add to group
                {
                    int selectedGroupId = 1;
                    string selectedGroupLetter = "A";
                    if (!MDAssy.DesignFamily.IsStandardDesignFamily())
                    {
                        mzQuestions query = new();
                        mzQuestion groupQuestion = query.AddStringChoice("Please select the group to which you like to add:", zone.GetGroupNames(out _,bAddOne: true,bGetLetters:true), LastGroupSelected, false, ",");
                        bool isCanceled = !query.PromptForAnswers("Add to Group?", aSaveButtonText: "Select");

                        if (isCanceled || string.IsNullOrWhiteSpace(groupQuestion.AnswerS)) return;
                        LastGroupSelected = groupQuestion.AnswerS;
                        selectedGroupLetter = LastGroupSelected.Replace("Group ", "").Trim();
                        selectedGroupId = mzUtils.ConvertLetterToInteger(selectedGroupLetter);
                        LastGroupSelected = groupQuestion.AnswerS;
                    }
                    foreach (uopRectangle aRect in regions)
                    {
                        if (matrix.SetGroupIndex(aRect.Name, selectedGroupId)) bChanges = true;
                    }


                }
                else if (action.Contains("CLEAR")) // remove constraints
                {

                        if (action.Contains("SELECT"))
                        {
                            uopRectangles.SetSelectedValue(AreaRegions, null, false, false);
                        Execute_HighlightSelectedRegions();
                    }
                    else
                    {
                        //    bool canceld = false; // !Query_Clear.PromptForAnswers("Clear Current Constraints?", aSaveButtonText: "Apply");
                        //bool clearIdeals = true; // Query_Clear.AnswerB(1, false);
                        //bool clearGroups = true; //Query_Clear.AnswerB(2, false);
                        //if (!clearIdeals && !clearGroups) canceld = true;

                        //if (canceld) return;
                        foreach (uopRectangle aRect in regions)
                        {

                            mdSpoutArea area = matrix.Area(aRect.Name);
                            if (area == null) continue;
                          
                                if (area.GroupIndex > 0)
                                {
                                    bChanges = true;
                                    area.GroupIndex = 0;
                                }

                         
                                if (area.TreatAsIdeal)
                                {
                                    bChanges = true;
                                    area.OverrideSpoutArea = null;
                                }

                            

                        }
                    }

               


                }
                else if (action.Contains("IDEAL"))
                {

                    foreach (uopRectangle aRect in regions)
                    {
                        if (matrix.SetLockValue(aRect.Name, true)) bChanges = true;
                    }


                }
            }
            finally
            {
                if (bChanges && !_Refreshing) Execute_Refresh();
            }



        }


        public void Execute_LassoEnd(uopRectangle rect)
        {
            if ( AreaRegions == null) return;
            List<uopRectangle> regions = AreaRegions.FindAll((x) => !x.IsVirtual);    //just get the non virtual regions
            if (regions.Count <=0 || rect == null) return;


            bool bChanges = false;
            try
            {
       
                //find the regions that are incled in the lasso rectangle
                List<uopRectangle> selectedregions = uopRectangles.RectanglesWithinRectangle( regions, rect,false);
                uopRectangles.SetSelectedValue(selectedregions, null, true, true);
                Execute_HighlightSelectedRegions();
                //if (selectedregions.Count == 0) return;
                //foreach (var region in selectedregions)
                //    region.Selected == true;
            
                //bool canceled = !Query_Action.PromptForAnswers($"Select Action For {selectedregions.Count} Spout Regions", true, aSaveButtonText: "Select");
                //if (canceled) return;
                //string action = Query_Action.AnswerS(1, string.Empty).ToUpper();
                //if (string.IsNullOrWhiteSpace(action)) return;


                //SelectedRegion = selectedregions[0];
                //if (action.Contains("GROUP")) // add to group
                //{
                //    int selectedGroupId = 1;
                //    if (!MDAssy.DesignFamily.IsStandardDesignFamily())
                //    {
                //        mzQuestions query = new();
                //        mzQuestion groupQuestion = query.AddStringChoice("Please select the group to which you like to add:", zone.GetGroupNames(out _, true), LastGroupSelected, false, ",");
                //        bool isCanceled = !query.PromptForAnswers("Add to Group?", aSaveButtonText: "Select");
                //        if (isCanceled || string.IsNullOrWhiteSpace(groupQuestion.AnswerS)) return;
                //        selectedGroupId = int.Parse(groupQuestion.AnswerS.Replace("Group ", ""));
                //        LastGroupSelected = groupQuestion.AnswerS;
                //    }
                //    foreach (uopRectangle aRect in selectedregions)
                //    {
                //        if (matrix.SetGroupIndex(aRect.Name, selectedGroupId)) bChanges = true;
                //    }


                //}
                //else if (action.Contains("CLEAR")) // remove constraints
                //{

                //    bool canceld = false; // !Query_Clear.PromptForAnswers("Clear Current Constraints?", aSaveButtonText: "Apply");
                //    bool clearIdeals = true; // Query_Clear.AnswerB(1, false);
                //    bool clearGroups = true; //Query_Clear.AnswerB(2, false);
                //    if (!clearIdeals && !clearGroups) canceld = true;

                //    if (canceld) return;
                //    foreach (uopRectangle aRect in selectedregions)
                //    {

                //        mdSpoutArea area = matrix.Area(aRect.Name);
                //        if (area == null) continue;
                //        if (clearGroups)
                //        {
                //            if (area.GroupIndex > 0)
                //            {
                //                bChanges = true;
                //                area.GroupIndex = 0;
                //            }

                //        }
                //        if (clearIdeals)
                //        {
                //            if (area.TreatAsIdeal)
                //            {
                //                bChanges = true;
                //                area.OverrideSpoutArea = null;
                //            }

                //        }

                //    }


                //}
                //else if (action.Contains("IDEAL"))
                //{

                //    foreach (uopRectangle aRect in selectedregions)
                //    {
                //        if (matrix.SetLockValue(aRect.Name, true)) bChanges = true;
                //    }


                //}
            }
            finally
            {
                if (bChanges && !_Refreshing) Execute_Refresh();
            }



        }



        public void Execute_UpdateControlVisibility()
        {
            try 
            {
                if (MDAssy == null) return;

                mdSpoutAreaMatrix matrix = CurrentMatrix;

                if (MDAssy.DesignFamily.IsStandardDesignFamily())
                {
                    Visibility_Group1 = Visibility.Visible;
                    Visibility_Group2 = Visibility.Collapsed;
                    Visibility_Group3 = Visibility.Collapsed;
                    Visibility_Group4 = Visibility.Collapsed;
                    Visibility_Group5 = Visibility.Collapsed;
                    Visibility_Group6 = Visibility.Collapsed;
                    Visibility_Group7 = Visibility.Collapsed;
                    Visibility_Group8 = Visibility.Collapsed;
                    Visibility_Group9 = Visibility.Collapsed;
                    Visibility_Group10 = Visibility.Collapsed;
                    Visibility_Group11 = Visibility.Collapsed;
                    Visibility_Group12 = Visibility.Collapsed;
                    Visibility_Group13 = Visibility.Collapsed;
                }
                else
                {
                    List<int> GroupIDs = matrix.Zone.GetGroupIndices(out _);
                    Visibility_Group1 = GroupIDs.IndexOf(1) > -1 ? Visibility.Visible : Visibility.Collapsed;
                    Visibility_Group2 = GroupIDs.IndexOf(2) > -1 ? Visibility.Visible : Visibility.Collapsed;
                    Visibility_Group3 = GroupIDs.IndexOf(3) > -1 ? Visibility.Visible : Visibility.Collapsed;
                    Visibility_Group4 = GroupIDs.IndexOf(4) > -1 ? Visibility.Visible : Visibility.Collapsed;
                    Visibility_Group5 = GroupIDs.IndexOf(5) > -1 ? Visibility.Visible : Visibility.Collapsed;
                    Visibility_Group6 = GroupIDs.IndexOf(6) > -1 ? Visibility.Visible : Visibility.Collapsed;
                    Visibility_Group7 = GroupIDs.IndexOf(7) > -1 ? Visibility.Visible : Visibility.Collapsed;
                    Visibility_Group8 = GroupIDs.IndexOf(8) > -1 ? Visibility.Visible : Visibility.Collapsed;
                    Visibility_Group9 = GroupIDs.IndexOf(9) > -1 ? Visibility.Visible : Visibility.Collapsed;
                    Visibility_Group10 = GroupIDs.IndexOf(10) > -1 ? Visibility.Visible : Visibility.Collapsed;
                    Visibility_Group11 = GroupIDs.IndexOf(11) > -1 ? Visibility.Visible : Visibility.Collapsed;
                    Visibility_Group12 = GroupIDs.IndexOf(12) > -1 ? Visibility.Visible : Visibility.Collapsed;
                    Visibility_Group13 = GroupIDs.IndexOf(13) > -1 ? Visibility.Visible : Visibility.Collapsed;
                }

            }
            catch (Exception ex) { SaveWarning(ex, System.Reflection.MethodBase.GetCurrentMethod()); }
            
        }


        public void Execute_AssignGridColor(Edit_SpoutAreas_View aView)
        {
            if (aView == null) return;
            mdSpoutAreaMatrix matrix = CurrentMatrix;
            if (matrix == null) return;

            uopTable aDataTable = DataTable;
            if (aDataTable == null) return;



            try
            {
                int iNd = matrix.Nd;
                int iNp = iNd + 1;
                mdSpoutZone zone = matrix.Zone;

                foreach (mdSpoutArea area in zone)
                {
                    int r = area.PanelIndex; //- 1;
                    int c = !InvertDowncomerColumns ? area.DowncomerIndex : uopUtils.OpposingIndex(area.DowncomerIndex, iNd);
                    //c--;
                    System.Windows.Controls.DataGridCell dgcell = aView.GetCell(r-1, c);
                    if (dgcell == null) continue;

                    dgcell.FontWeight = area.OverrideSpoutArea.HasValue ? System.Windows.FontWeights.Bold : System.Windows.FontWeights.Normal;
                    dgcell.Foreground = aView.ToMediaColor(area.Color.Item2);
             
                    uopInstances insts = area.Instances;
                    foreach (var item in insts)
                    {
                        r = item.Row; // - 1;
                        c = !InvertDowncomerColumns ? item.Col : uopUtils.OpposingIndex(item.Col, iNd);
                        //c--;
                        System.Windows.Controls.DataGridCell dgcell1 = aView.GetCell(r-1 , c);
                        if (dgcell1 == null) continue;
                        dgcell1.FontWeight = dgcell.FontWeight;
                        dgcell1.Foreground = dgcell.Foreground;
                    }

                }

                double lim = matrix.ConverganceLimit;

                for(int p = 1; p<= matrix.Rows; p++) 
                {
                    uopMatrixCell cell1 = matrix.Cell(p, iNd + 1); // dc total
                    uopMatrixCell cell2 = matrix.Cell(p, iNd + 2); // dc ideal
                    uopMatrixCell cell3 = matrix.Cell(p, iNd + 3); // deviation
                    System.Windows.Controls.DataGridCell dgcell1 = aView.GetCell(cell1.Row - 1, iNd + 1);
                    System.Windows.Controls.DataGridCell dgcell2 = aView.GetCell(cell1.Row - 1, iNd + 2);
                    System.Windows.Controls.DataGridCell dgcell3 = aView.GetCell(cell1.Row - 1, iNd + 3);

                    double dif = Math.Abs(cell3.Value);

                    dgcell1.Foreground = aView.ToMediaColor(System.Drawing.Color.Gray);
                    dgcell2.Foreground = aView.ToMediaColor(System.Drawing.Color.Gray);
                    dgcell3.Foreground = aView.ToMediaColor(dif < lim ? System.Drawing.Color.Green : System.Drawing.Color.Red);


                }

                for(int d=1; d<= iNd; d++)
                {
                    uopMatrixCell cell1 = matrix.Cell(iNp + 1, d); // dp total
                    uopMatrixCell cell2 = matrix.Cell(iNp + 2, d); // dp ideal
                    uopMatrixCell cell3 = matrix.Cell(iNp + 3, d); // deviation
                    System.Windows.Controls.DataGridCell dgcell1 = aView.GetCell(iNp + 1 -1,d);
                    System.Windows.Controls.DataGridCell dgcell2 = aView.GetCell(iNp + 2 -1, d);
                    System.Windows.Controls.DataGridCell dgcell3 = aView.GetCell(iNp + 3 - 1, d);

                    double dif = Math.Abs(cell3.Value);

                    dgcell1.Foreground = aView.ToMediaColor(System.Drawing.Color.Gray);
                    dgcell2.Foreground = aView.ToMediaColor(System.Drawing.Color.Gray);
                    dgcell3.Foreground = aView.ToMediaColor(dif < lim ? System.Drawing.Color.Green : System.Drawing.Color.Red);

                }

                //for (int p = 1; p <= iNp; p++)
                //{
                //    for (int d = 1; d <= iNd; d++)
                //    {
                //        double dctotal = 0;
                //        mdSpoutArea area = matrix.Area(p, d);
                //        if (area == null) continue;
                //        int r = p; //- 1;
                //        int c = !InvertDowncomerColumns ? d : uopUtils.OpposingIndex(d, iNd);
                //        //c--;
                //        System.Windows.Controls.DataGridCell dgcell = aView.GetCell(r, c);
                //        if (dgcell == null) continue;

                //        dgcell.FontWeight = area.OverrideSpoutArea.HasValue ? System.Windows.FontWeights.Bold : System.Windows.FontWeights.Normal;
                //        dgcell.Foreground = aView.ToMediaColor(area.Color.Item2);

                //        uopInstances insts = area.Instances;
                //        foreach (var item in insts)
                //        {
                //            r = item.Row; // - 1;
                //            c = !InvertDowncomerColumns ? item.Col : uopUtils.OpposingIndex(item.Col, iNd);
                //            //c--;
                //            System.Windows.Controls.DataGridCell dgcell1 = aView.GetCell(r, c);
                //            if (dgcell1 == null) continue;
                //            dgcell1.FontWeight = dgcell.FontWeight;
                //            dgcell1.Foreground = dgcell.Foreground;
                //        }
                //        if (area != null && dgcell != null && area.Color.Item1 != DXFGraphics.dxxColors.BlackWhite)
                //        {
                //            Console.Beep();
                //            break;
                //        }
                //    }
                //    break;
                //}


            }
           catch (Exception ex) { SaveWarning(ex, System.Reflection.MethodBase.GetCurrentMethod()); }


        }

        /// <summary>
        /// Initialize method for edit spout area drawing
        /// </summary>
        /// <param name="mDSpoutProjectViewModel"></param>
        /// <param name="aRange"></param>
        /// <returns></returns>
        public bool Initialize(MDProjectViewModel mDSpoutProjectViewModel, mdTrayRange aRange)
        {
            bool _rVal = false;
            _Query_Action = null;
            MDRange = aRange;
            Canceled = true;
            DialogService =   mDSpoutProjectViewModel.DialogService;
            Image = null;
            Project = mDSpoutProjectViewModel.MDProject;
            _Refreshing = false;
            if (MDRange == null) return _rVal;
            
            Matrices = new mdSpoutAreaMatrices(MDAssy);
            Matrices.UpdateSpoutGroupAreas(MDAssy);
            OriginalMatrices = new mdSpoutAreaMatrices(Matrices);
            LastGroupSelected = "Group A";
            Visibility_MultiMatrices = Matrices.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
            ZoneIndex = 1;
           IsOkBtnEnable = true;

            MenuItems.Clear();

            System.Windows.Controls.MenuItem clearConstraints = new System.Windows.Controls.MenuItem() { Header = "Clear Constraints", Command = Command_ClearConstraints };
            MenuItems.Add(clearConstraints);

            System.Windows.Controls.MenuItem makeIdeal = new System.Windows.Controls.MenuItem() { Header = "Make Ideal", Command = Command_MakeIdeal };
            MenuItems.Add(makeIdeal);

            System.Windows.Controls.MenuItem addtogrp = new System.Windows.Controls.MenuItem() { Header = "Add To Group", Command = Command_AddToGroup };
            MenuItems.Add(addtogrp);


            Title = $"Edit Spout Area Distribution - {MDProject.Name} ({MDRange.Name(true)})";

       
            IsEnglishSelected = MDProject.DisplayUnits == uppUnitFamilies.English;

            if (DialogService.ShowDialog<Edit_SpoutAreas_View>(mDSpoutProjectViewModel, this) == true) { }
            // Added by CADfx
            DialogService.Close(this);
            // Added by CADfx

            RefreshMessage = null;
            if (!Canceled)
            {
                Edited = CheckForChanges();
                _rVal = Edited;

                if (Edited)
                {
                    RefreshMessage = new Messages.Message_Refresh(bSuppressPropertyLists:false,bSuppressDataGrids:false, bSuppressTree: false,bSuppressImage:false,bCloseDocuments:true);
                }

            }

            return _rVal;

        }

        /// <summary>
        /// Edit spout areas
        /// </summary>
        /// <param name="editLayout"></param>
        public void EditSpoutAreas()
        {
            if (Image == null)
            {
                try
                {
                    BusyMessage = "Loading Data..";
                    IsEnabled = false;
                    Loading = true;
                    Execute_DrawImage();

                    Loading = false;

                }
                finally
                {
                    IsEnabled = true;
                }
            }
        }
       
        /// <summary>
        /// Enter cell event functionaity of datagrid
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <param name="RowIndex"></param>
        public void EnterCellEventOfDataGrid(int columnIndex, int RowIndex)
        {
            mdSpoutAreaMatrix matrix = CurrentMatrix;
            if (!DontTrackGroup|| matrix == null)
            {
                
                int didx = columnIndex;
                int pidx = RowIndex;
                if (didx < 1 || pidx < 1 || didx > matrix.DowncomerCount || pidx > matrix.PanelCount) return;

                mdSpoutArea area = matrix.Area(pidx, didx);
                if (area == null) return;

                uopRectangle region =  AreaRegions.Find((x) => x.Name == area.Handle);
                if (region == null) return;
                if (ControlIsDown)
                {
                    AreaRegions.SetSelected(region, false, true);
                }
                else
                {
                    AreaRegions.SetSelected(region, true, ShiftIsDown);
                }

         
                Execute_HighlightSelectedRegions();
                
            }

        }

        public void SetShapeLocation(uopRectangle aBox)
        {

            try
            {
                mdSpoutAreaMatrix matrix = CurrentMatrix;
                if (Viewer == null || matrix == null) return;

                uopRectangle box = aBox == null ? SelectedRegion : aBox;

                var overlay = Viewer.overlayHandler;
                overlay.ClearRectangles();
                // overlay.DrawRectangle(box, Color.Yellow, 100,ignoreExisting:true);
                if (box == null) return;

                //            if (!ExtractIndices(box.Tag, out int dcindex, out int plindex)) return;
                mdSpoutArea area = matrix.Zone.Find((x) => x.Handle == box.Name);
                if (area == null) return;
                uopRectangles regions = AreaRegions;

                SelectedRegion = regions.Find((x) => x.Name == area.Handle);
                box = SelectedRegion;
                if (box == null) return;

                ColumnIndex = area.DowncomerIndex;
                RowIndex = area.PanelIndex;

                OnIndexChanged();
                double paperscale = Image.Display.PaperScale;
                dxfRectangle rec = box.ToDXFRectangle();
                rec = rec.Stretched(0.1 * paperscale, true, true);
                overlay.DrawRectangle(rec, System.Drawing.Color.Yellow, 100, ignoreExisting: true);

                if (matrix.Zone.DesignFamily.IsStandardDesignFamily())
                {
                    uopInstances insts = area.Instances;
                    foreach (var inst in insts)
                    {
                        area = matrix.Area(inst.Row, inst.Col);

                        dxfRectangle box2 = new dxfRectangle(rec);
                        box2.Move(inst.DX, inst.DY);

                        if (box2 != null)
                        {

                            overlay.DrawRectangle(box2, System.Drawing.Color.LightSalmon, 100, ignoreExisting: true);
                        }
                    }
                }
             
            }
            catch (Exception ex) { SaveWarning(ex, System.Reflection.MethodBase.GetCurrentMethod()); }

        }

   


        /// <summary>
        /// Function to envoke ColorChnged event
        /// </summary>
        public void OnColorChanged()
        {
            if (Event_ColorChanged != null)
            {
                Event_ColorChanged.Invoke(this, null);
            }
        }
        
        /// <summary>
        /// Function to envoke ColorChnged event
        /// </summary>
        public void OnIndexChanged()
        {
            if (Event_IndexChanged != null)
            {
                Event_IndexChanged.Invoke(this, null);
            }
        }

        public override void Activate(System.Windows.Window myWindow)
        {
            if (Activated) return;
            base.Activate(myWindow);
            _Visibility_Group1 = Visibility.Collapsed;
            _Visibility_Group2 = Visibility.Collapsed;
            _Visibility_Group3 = Visibility.Collapsed;
            _Visibility_Group4 = Visibility.Collapsed;
            _Visibility_Group5 = Visibility.Collapsed;
            _Visibility_Group6 = Visibility.Collapsed;
            _Visibility_Group7 = Visibility.Collapsed;
            _Visibility_Group8 = Visibility.Collapsed;
            _Visibility_Group9 = Visibility.Collapsed;
            _Visibility_Group10 = Visibility.Collapsed;
            EditSpoutAreas();
        }

        /// <summary>
        /// check for the changes
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public bool CheckForChanges()
        {

            if(Matrices == null || OriginalMatrices == null) return false;
            return Matrices.IsEqual(OriginalMatrices, 6);

        }

/// <summary>
        /// Recalculates spout areas
        /// </summary>
        /// <param name="viewModel"></param>
        public void Execute_RedistributeArea()
        {

            if (MDAssy == null) return;
            try
            {
                if (Matrices == null) Matrices = new mdSpoutAreaMatrices(MDAssy);
                mdSpoutAreaMatrix matrix = CurrentMatrix;

                matrix.DistributeSpoutArea();
                DataTable = matrix.GetTable(DisplayUnits, bAddHeaders: true, bAddIdeals: true, precisionadder: uopUtils.RunningInIDE ? 1 : 0, bReverseColumns: true);

            }
            catch (Exception ex) {SaveWarning(ex,System.Reflection.MethodBase.GetCurrentMethod());  }

        }

        /// <summary>
        /// show Area matrix
        /// </summary>
        /// <param name="viewModel"></param>
        public void Execute_ShowMatrix()
        {
            IsOkBtnEnable = false;
            if (MDAssy == null) return;

            try
            {
                Matrices ??= new mdSpoutAreaMatrices(MDAssy);

                mdSpoutAreaMatrix matrix = CurrentMatrix;

                double daLimit = MDAssy.ConvergenceLimit;

                DontTrackGroup = true;

                //if(DataTable == null)
                DataTable = matrix.GetTable(DisplayUnits, bAddHeaders: true, bAddIdeals: true, precisionadder: uopUtils.RunningInIDE ? 1 : 0, bReverseColumns: InvertDowncomerColumns);

                DataTableCollection = DataTable.ToDataTable_S();

                OnColorChanged();
                DontTrackGroup = false;
                GoodSolution = Math.Max(matrix.MaxPanelDeviation, matrix.MaxDowncomerDeviation) < daLimit;

                if (GoodSolution)
                {
                    Converged = "Converged";
                    BColor = "Green";
                    IsOkBtnEnable = true;
                }
                else
                {
                    Converged = "Not Converged";
                    BColor = "Pink";
                }
                Dof = $"Degrees of Freedom = {DOF}";
                ConvergeLimit = $"Covergence Limit = {daLimit:0.00E+00}";
            }
            catch(Exception ex) { SaveWarning(ex, System.Reflection.MethodBase.GetCurrentMethod()); }

        }

        /// <summary>
        /// Method to draw spout area
        /// </summary>

        public void Execute_DrawImage()
        {

            try
            {

           

                if (MDAssy == null || Viewer == null) return;
                if (Matrices == null) Matrices = new mdSpoutAreaMatrices(MDAssy);
                mdSpoutAreaMatrix matrix = CurrentMatrix;

                DowncomerDataSet dcdata = MDAssy.DowncomerData;

                mdSpoutZone zone = matrix.Zone;

                DXFBounds = new List<dxePolyline>();
                if (Image != null) Image.Dispose();
                //Viewer.Clear();
                Viewer.Init();


                Image = new dxfImage(appApplication.SketchColor);
                Image.Display.Size = Viewer.Size;
                dxoDrawingTool draw = Image.Draw;
                Image.Display.BackgroundColor = Properties.Settings.Default.SketchColor;
                Image.Header.Linetype = dxfLinetypes.Continuous;
                Image.Header.UCSMode = dxxUCSIconModes.None;
                Image.Display.Size = Viewer.Size;
                Image.LinetypeLayers.Add(dxfLinetypes.Hidden, "HIDDEN", dxxColors.Green);

                draw.aCircle(null, MDAssy.RingRadius, dxfDisplaySettings.Null(aLinetype: dxfLinetypes.Hidden)); //  new(null, MDAssy.RingID / 2); //, 0, 90);
                Image.Display.ZoomExtents(bSetFeatureScale: true);

                //dxfRectangle ViewRectang = MDAssy.SpoutGroups.BoundingRectangle().Expanded(1.05);

                List<uopLinePair> boxlines = zone.WeirLines; // dcdata.BoxLines;
                dxfDisplaySettings dsp = new dxfDisplaySettings("0", dxxColors.Grey, dxfLinetypes.Continuous);

                foreach (var pair in boxlines)
                {
                    //if (pair.IsVirtual) continue;
                    draw.aLine(pair.Line1, aDisplaySettings: dsp);
                    draw.aLine(pair.Line2, aDisplaySettings: dsp);
                }
                PanelBlock ??= Image.Blocks.Add(mdBlocks.DeckPanels_View_Plan(Image, MDAssy, "0", dxxColors.z_71, dxfLinetypes.Continuous, bBothSides: true, bForTrayBelow: true));
                double xscale = MDAssy.DesignFamily.IsStandardDesignFamily() ? 1 : -1;
                draw.aInsert(PanelBlock.Name, dxfVector.Zero, 0, aScaleFactor: xscale, aYScale:1);
                DXFBounds = new List<dxePolyline>();
                foreach (var sa in zone)
                {
                    DXFBounds.Add((dxePolyline)Image.Entities.Add(sa.ToDXFPolyline(aColor: sa.Color.Item1), aTag: (matrix.Zone.IndexOf(sa) + 1).ToString(), aFlag: sa.Handle));
                }

                if (MDAssy.DesignFamily.IsBeamDesignFamily())
                {
                    dxfBlock beam = mdBlocks.Beam_View_Plan(Image, MDAssy.Beam, MDAssy, true, bObscured: false, bSuppressHoles: true, aLayerName: "BEAMS");

                    draw.aInserts(beam, null, bOverrideExisting: false, aLTLSetting: dxxLinetypeLayerFlag.ForceToColor);
                }

                Viewer.SetImage(Image, false, true);
                Viewer.RefreshDisplay();
            } catch(Exception ex) { SaveWarning(ex, System.Reflection.MethodBase.GetCurrentMethod());}

        }

      
        public override void ReleaseReferences()
        {
            base.ReleaseReferences();
            SelectedRegion = null;
            DataTableCollection = null;
            _AreaRegions = null;
            Matrices = null;
            OriginalMatrices = null;
            DataTable = null;
            
        }
        
        private media::SolidColorBrush ColorToBrush(System.Drawing.Color color)
        {
            return new media::SolidColorBrush(media::Color.FromArgb(color.A, color.R, color.G, color.B));
        }

        #endregion Methods

        #region Event Handlers
        public void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e == null) return;
            if(e.Key == Key.LeftShift) 
                _LeftShiftDown = true;
            else if (e.Key == Key.RightShift)
                _RightShiftDown = true;
            else if (e.Key == Key.LeftCtrl)
                _LeftControlDown = true;
            else if (e.Key == Key.RightCtrl)
                _RightControlDown = true;
            else if(e.Key == Key.Escape)
            {
                if(AreaRegions != null)
                {
                    uopRectangles.SetSelectedValue(AreaRegions, null, false, true);
                    Execute_HighlightSelectedRegions();
                }
            }

        }
        public void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e == null) return;
            if (e.Key == Key.LeftShift)
                _LeftShiftDown = false;
            else if (e.Key == Key.RightShift)
                _RightShiftDown = false;
            else if (e.Key == Key.LeftCtrl)
                _LeftControlDown = false;
            else if (e.Key == Key.RightCtrl)
                _RightControlDown = false;
        }
        public void Viewer_MouseDown(MouseButtons button, double x, double y)
        {
            if (Image == null || Matrices == null) return;


            if (button == MouseButtons.Left)
            {
                int cnt = 0;
                MousePoint = Viewer.DeviceToWorld(new dxfVector(x, y, 0));
                if (AreaRegions.ContainsVector(MousePoint, out List<uopRectangle> containers, bReturnJustOne: true))
                {
                    if (ControlIsDown)
                    {
                        cnt = AreaRegions.SetSelected(containers[0], false, true);
                    }
                    else
                    {
                        cnt = AreaRegions.SetSelected(containers[0], true, ShiftIsDown);
                    }

                    //SelectedRegion = containers[0];

                }
                else
                {
                    //if (MousePoint.Length > MDAssy.ShellRadius)
                    //    uopRectangles.SetSelectedValue(AreaRegions, null, false, false);
                }
               if(cnt > 0) Execute_HighlightSelectedRegions();

            }
            else if (button == MouseButtons.Right)
            {
                mdSpoutAreaMatrix matrix = CurrentMatrix;
                if (AreaRegions == null || matrix == null) return;
                mdSpoutZone zone = matrix.Zone;

                List<uopRectangle> regions = AreaRegions.FindAll((x) => !x.IsVirtual && x.Selected);    //just get the non virtual regions
                if (regions.Count <= 0) return;
                Execute_ExecuteAction();
            }

        }


        private void Viewer_OnViewerMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        { 
            Viewer.SetDragSelectionBorderColor( ColorToBrush(System.Drawing.Color.Black));
            Init = true;
            DownPoint = e.GetPosition((System.Windows.IInputElement)sender);
            Viewer.ShowDragSelectionRect();
      
            System.Windows.Forms.MouseButtons button = System.Windows.Forms.MouseButtons.None;
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed && e.ClickCount == 1)
                button = System.Windows.Forms.MouseButtons.Left;
            if (e.RightButton == System.Windows.Input.MouseButtonState.Pressed && e.ClickCount == 1)
                button = System.Windows.Forms.MouseButtons.Right;

            var p = e.GetPosition((System.Windows.IInputElement)sender);
            Viewer_MouseDown(button, p.X, p.Y);
        }

        private void Viewer_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (Init)
            {
                CurPoint = e.GetPosition((System.Windows.IInputElement)sender);
                var rect = new Rect(DownPoint, CurPoint);

                Viewer.SetDragBoxLeft(rect.Left);
                Viewer.SetDragBoxTop(rect.Top);
                Viewer.dragSelectionBorder.Width = rect.Width;
                Viewer.dragSelectionBorder.Height = rect.Height;
            }
        }


        private void Viewer_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Init)
            {
                var p1 = Viewer.DeviceToWorld(new dxfVector(DownPoint.X, DownPoint.Y, 0));
                var p2 = Viewer.DeviceToWorld(new dxfVector(CurPoint.X, CurPoint.Y, 0));
                var topleft = new uopVector(Math.Min(p1.X, p2.X), Math.Max(p1.Y, p2.Y), 0);
                var bottomright = new uopVector(Math.Max(p1.X, p2.X), Math.Min(p1.Y, p2.Y), 0);
                var rect = new uopRectangle(topleft.X, topleft.Y,bottomright.X,bottomright.Y);
                
                if (rect.Width > 0 && rect.Height > 0)
                    Execute_LassoEnd(rect);

                Viewer.HideDragSelectionRect();
            }
            Init = false;
            Control = "";
        }

        private void Viewer_ContextMenuOpening(object sender, System.Windows.Controls.ContextMenuEventArgs e)
        {
            if (SelectedRegion == null) e.Handled = true;
        }


        internal void Window_Closing(object sender, CancelEventArgs e)
        {

            Image = null;
            Viewer = null;

        }

        #endregion Event Handlers
    }

    struct RefreshInput_SpoutAreas
    {



        public RefreshInput_SpoutAreas(bool bSuppressRecalc)
        {
            SuppressRecalc = bSuppressRecalc;

        }
        public bool SuppressRecalc { get; set; }

    }

    public class SpoutGroupForSpoutAreasViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int Id { get; set; }
        public string Name { get; set; }

        private int _count = 0;
        public int Count
        {
            get { return _count; }
            set
            {
                if (value < 0)
                {
                    value = 0;
                }

                if (_count != value)
                {
                    _count = value;
                    OnPropertyChanged("Visibility");
                }
            }
        }

        public Visibility Visibility
        {
            get { return _count > 0 ? Visibility.Visible : Visibility.Collapsed; }
        }
    }
}