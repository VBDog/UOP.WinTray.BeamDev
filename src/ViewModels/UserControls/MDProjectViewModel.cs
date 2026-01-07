using DocumentFormat.OpenXml.Drawing.Diagrams;
using iText.Layout.Properties;
using MvvmDialogs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Unity.Resolution;
using UOP.DXFGraphics;
using UOP.DXFGraphics.Utilities;
using UOP.DXFGraphicsControl;
using UOP.WinTray.Projects;
using UOP.WinTray.Projects.Documents;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Interfaces;
using UOP.WinTray.Projects.Parts;

using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;
using UOP.WinTray.UI.BusinessLogic;
using UOP.WinTray.UI.Commands;
using UOP.WinTray.UI.Events.EventAggregator;
using UOP.WinTray.UI.Interfaces;
using UOP.WinTray.UI.Messages;
using UOP.WinTray.UI.Model;
using UOP.WinTray.UI.ViewModels.CADfx.Windows;
using UOP.WinTray.UI.Views;
using UOP.WinTray.UI.Views.UserControls;
using UOP.WinTray.UI.Views.Windows;
using UOP.WinTray.UI.Views.Windows.CADfx;
using UOP.WinTray.UI.Views.Windows.CADfx.Questions;

namespace UOP.WinTray.UI.ViewModels
{



    /// <summary>
    /// ViwModel for MDSpout project. TODO://Alot of existing methods are remove, as those are not required
    /// at this moment. We need get necessary chnages required as and when we need.
    /// </summary>
    public class MDProjectViewModel : MDProjectViewModelBase, IProjectViewModel,
                                            IEventSubscriber<Message_RefreshGraphics>,
                                            IEventSubscriber<Message_ReleaseGraphics>,
                                            IEventSubscriber<Message_ProjectChange>,
                                            IEventSubscriber<Message_UnloadProject>,
                                            IEventSubscriber<Message_HighlightImageFromGridClick>,
                                            IEventSubscriber<Message_RefreshRangeList>,
                                            IEventSubscriber<Message_HighlightImage>,
                                            IEventSubscriber<Message_ShowSpoutAreaDistibution>,
                                            IEventSubscriber<Message_ClearData>

    {
        #region Constants


        private const string NO_VALID_SCALE_ITEM_TEXT = "---";

        #endregion



        public delegate void Update_Viewer_Image_Callback(dxfImage aImage);



        #region Variables

        private readonly BackgroundWorker _Worker_DrawingGenerate;
        private readonly BackgroundWorker _Worker_SketchGenerate;
        private readonly BackgroundWorker _Worker_DrawingRegenerate;


        private readonly Color _DowncomerHighlightColor = Color.FromArgb(0, 217, 42);
        private readonly Color _SpoutGroupHighlightColor = Color.FromArgb(213, 198, 0);

        #endregion Variables

        #region Constructor


   

        internal MDProjectViewModel(IEventAggregator eventAggregator,
                                        IDialogService dialogService) : base(eventAggregator: eventAggregator, dialogService: dialogService)
        {


            try
            {
                IsEnglishSelected = true;
                IsEnabled = false;
                _Worker_DrawingGenerate = new BackgroundWorker();
                _Worker_DrawingGenerate.DoWork += DoWork_Worker_DrawingGenerate;
                _Worker_DrawingGenerate.RunWorkerCompleted += DoWork_Worker_DrawingGenerate_Complete;

                _Worker_DrawingRegenerate = new BackgroundWorker();
                _Worker_DrawingRegenerate.DoWork += DoWork_Worker_DrawingReGenerate;
                _Worker_DrawingRegenerate.RunWorkerCompleted += DoWork_Worker_DrawingReGenerate_Complete;

                _Worker_SketchGenerate = new BackgroundWorker();
                _Worker_SketchGenerate.DoWork += DoWork_Worker_SketchGenerate;
                _Worker_SketchGenerate.RunWorkerCompleted += DoWork_Worker_SketchGenerate_Complete;

                this.PropertyChanged += MDProjectViewModel_PropertyChanged;
                DrawingScales = new ObservableCollection<string>();
                DrawingScale_Current = NO_VALID_SCALE_ITEM_TEXT;
                EventAggregator?.Subscribe(this);
            }
            catch { }
          
    
        }
        private void MDProjectViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "DrawingTab_SelectedIndex")
            {
                ProcessTabChangeEvent();
            }
        }

        #endregion

        #region Properties

        public bool Disabled
        {
            get => !base.IsEnabled;
            set => base.IsEnabled = !value;
        }

        public override bool IsEnglishSelected
        {
            get => base.IsEnglishSelected;
            set
            {
                base.IsEnglishSelected = value;
                NotifyPropertyChanged("IsEnglishSelected");

                Message_Refresh msg = new(bSuppressDataGrids: false, bSuppressPropertyLists: false, bSuppressTree: true, bSuppressImage: true);
                msg.UnitsToDisplay = DisplayUnits;
                if (MDProject != null)
                    MDProject.DisplayUnits = DisplayUnits;
                msg.Publish();
            }
        }

        private bool DrawingTab_Loading { get; set; }

        public override dxfVector MousePoint
        {
            get { if (base.MousePoint == null) base.MousePoint = new dxfVector(); return base.MousePoint; }
            set { base.MousePoint = value; }
        }

        private Visibility _Visibility_CurrentDistributor = Visibility.Collapsed;
        public Visibility Visibility_CurrentDistributor
        {
            get => _Visibility_CurrentDistributor; set { _Visibility_CurrentDistributor = value; NotifyPropertyChanged("Visibility_CurrentDistributor"); }
        }

        private Visibility _Visibility_CurrentCT = Visibility.Collapsed;
        public Visibility Visibility_CurrentCT
        {
            get => _Visibility_CurrentCT; set { _Visibility_CurrentCT = value; NotifyPropertyChanged("Visibility_CurrentCT"); }
        }

        private Visibility _Visibility_DrawingScaleControl = Visibility.Visible;
        public Visibility Visibility_DrawingScaleControl
        {
            get => _Visibility_DrawingScaleControl; set { _Visibility_DrawingScaleControl = value; NotifyPropertyChanged("Visibility_DrawingScaleControl"); }
        }

        public bool RefreshingInputSketch { get; set; }

        public IappDrawingSource DrawingHelper(dxfImage image = null)
        {


            IappDrawingSource _rVal = ProjectType switch
            {
                uppProjectTypes.MDDraw => new AppDrawingMDDHelper() { Project = Project, Range = MDRange, BlockSource = appApplication.BlockSource },
                uppProjectTypes.MDSpout => new AppDrawingMDSHelper() { Project = Project, Range = MDRange },
                _ => null
            };

            if (_rVal != null)
            {
                _rVal.Image = image;
            }

            return _rVal;
        }

        /// <summary>
        /// get the areas in the display that respond to clicks on spoutgroups
        /// </summary>
        /// <returns></returns>

        private colDXFRectangles _SpoutRegions;
        public colDXFRectangles SpoutRegions
        {
            get
            {
                if (_SpoutRegions == null)
                {
                    mdTrayAssembly assy = MDAssy;
                    if (assy != null) _SpoutRegions = assy.SpoutRegions;
                }
                return _SpoutRegions;
            }
            set
            {
                _SpoutRegions = value;
            }
        }

        private colDXFRectangles _DowncomerRegions;
        public colDXFRectangles DowncomerRegions
        {
            get
            {
                if (_DowncomerRegions == null)
                {
                    mdTrayAssembly assy = MDAssy;
                    if (assy != null) _DowncomerRegions = assy.DowncomerRegions(false);
                }
                return _DowncomerRegions;
            }
            set
            {
                _DowncomerRegions = value;
            }
        }


        public override mdProject MDProject
        {
            get => base.MDProject;
            set
            {
                base.MDProject = value;
                VisibilityMDDraw = (value != null && value.ProjectType == uppProjectTypes.MDDraw) ? Visibility.Visible : Visibility.Collapsed;
                DesignOptionsLabel = (value != null && value.ProjectType == uppProjectTypes.MDDraw) ? "Design Options" : "Design Info";
                Activated = false;
            }
        }

        private bool _IsEnabled = true;
        public override bool IsEnabled
        {
            get => _IsEnabled;
            set { _IsEnabled = value; NotifyPropertyChanged("IsEnabled"); }
        }

        private string _DesignOptionsLabel;
        public string DesignOptionsLabel
        {
            get => _DesignOptionsLabel;
            set { _DesignOptionsLabel = value; NotifyPropertyChanged("DesignOptionsLabel"); }
        }

        public dxfImage InputSketch
        {
            set
            {
                if (value != null && Viewer != null)
                {
                    Viewer.SetImage(value);
                    if (value.Display.HasLimits)
                    { Viewer.ZoomWindow(value.Display.LimitsRectangle); }
                    else
                    { Viewer.ZoomExtents(); }
                    Viewer.RefreshDisplay();
                }
            }
        }

        public override DXFViewer Viewer
        {
            get => base.Viewer;
            set
            {
                if (base.Viewer != null) base.Viewer.OnViewerMouseDown -= InputSketch_OnViewerMouseDown;
                base.Viewer = value;
                if (base.Viewer != null) base.Viewer.OnViewerMouseDown += InputSketch_OnViewerMouseDown;

                //if(Viewer != null) Viewer.SizeChanged += ViewerSizeChangeHandler();

                if (base.Viewer != null) base.Viewer.EnableRightMouseZoom = true;
            }
        }

        readonly ObservableCollection<UOPDocumentTab> _UOPDocumentTabs = new();
        public ObservableCollection<UOPDocumentTab> UOPDocumentTabs => _UOPDocumentTabs;

        private int _DrawingTab_SelectedIndex;
        public int DrawingTab_SelectedIndex
        {
            get => _DrawingTab_SelectedIndex;
            set { _DrawingTab_SelectedIndex = value; NotifyPropertyChanged("DrawingTab_SelectedIndex"); }
        }

        public int DrawingTab_Index { get; set; } // This is the index of "Drawings" tab in its containing TabControl



        private ObservableCollection<string> _DrawingScales;
        public ObservableCollection<string> DrawingScales
        {
            get => _DrawingScales;
            set { _DrawingScales = value; NotifyPropertyChanged("DrawingScales"); }
        }

        private string _DrawingScale_Current;
        public string DrawingScale_Current
        {
            get => _DrawingScale_Current;

            set
            {
                if (_DrawingScale_Current != value)
                {
                    _DrawingScale_Current = value;
                    NotifyPropertyChanged("DrawingScale_Current");
                }
            }
        }

        private bool _DrawingScale_Enabled;
        public bool DrawingScale_Enabled
        {
            get
            {
                return _DrawingScale_Enabled;
            }

            set
            {
                if (_DrawingScale_Enabled != value)
                {
                    _DrawingScale_Enabled = value;
                    NotifyPropertyChanged("DrawingScale_Enabled");
                }
            }
        }



        public uopTrayRange SelectedRange
        {
            get => Trays?.SelectedRange;

            set
            {
                SelectedRangeGUID = (value != null) ? value.GUID : "";
                NotifyPropertyChanged("SelectedRange");
            }
        }



        public override colUOPTrayRanges Trays
        {
            get => base.Trays;
            set
            {
                base.Trays = value;
                if (value == null)
                {
                    SelectedRangeGUID = "";

                }
                else
                {
                    SelectedRangeGUID = value.SelectedRangeGUID;

                }
            }
        }

        private string _SelectedRangeName;
        public string SelectedRangeName
        {
            get => _SelectedRangeName;
            set
            {
                if (_settingRange || MDProject == null) return;
                _SelectedRangeName = value == null ? "" : value.Trim();
                NotifyPropertyChanged(MethodBase.GetCurrentMethod());
                if (!string.IsNullOrWhiteSpace(_SelectedRangeName))
                {
                    uopTrayRange range = MDProject.TrayRanges.Find((x) => string.Compare(x.SelectName, _SelectedRangeName, true) == 0);
                    if (range != null) SetSelectedRange(range.GUID);
                }

            }
        }

        private int _SelectedRangeIndex;
        public int SelectedRangeIndex
        {
            get => _SelectedRangeIndex;
            set
            {
                if (_settingRange || MDProject == null) return;
                _SelectedRangeIndex = value;
                NotifyPropertyChanged(MethodBase.GetCurrentMethod());
                if (value >= 0)
                {
                    uopTrayRange range = MDProject.TrayRanges.Item(value + 1);
                    if (range != null) SetSelectedRange(range.GUID);
                }


            }
        }



        private string _SelectedRangeGUID;
        public string SelectedRangeGUID
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_SelectedRangeGUID))
                {
                    string guid = (Trays != null) ? Trays.SelectedRangeGUID : "";
                    if (string.Compare(guid, _SelectedRangeGUID, true) != 0)
                    {
                        SetSelectedRange(guid);

                    }
                }
                return _SelectedRangeGUID;

            }
            set
            {
                if (_settingRange) return;
                SetSelectedRange(value, string.IsNullOrWhiteSpace(value));
                NotifyPropertyChanged(nameof(IsBeamPropertiesVisible));
            }
        }


        private bool _settingRange;
        private void SetSelectedRange(string aGUID, bool bSuppressRefresh = false)
        {
            _settingRange = true;
            try
            {

                _SelectedRangeGUID = "";
                _SelectedRangeIndex = -1;
                _SelectedRangeName = string.Empty;
                TrayList = new ObservableCollection<uopPart>();

                colUOPTrayRanges mytrays = Trays;

                if (mytrays != null)
                {
                    TrayList = mytrays.ToObservable();
                    if (!string.IsNullOrWhiteSpace(aGUID)) mytrays.SelectedRangeGUID = aGUID;
                    uopTrayRange range = mytrays.SelectedRange;
                    if (range != null)
                    {
                        _SelectedRangeIndex = range.Index - 1;
                        _SelectedRangeGUID = range.GUID;
                        _SelectedRangeName = range.SelectName;
                    }



                }

            }
            catch { }
            finally
            {
                _settingRange = false;
                UpdateProjectVisibility();

                NotifyPropertyChanged(nameof(TrayList));
                NotifyPropertyChanged(nameof(SelectedRangeGUID));
                NotifyPropertyChanged(nameof(SelectedRangeIndex));
                NotifyPropertyChanged(nameof(SelectedRangeName));
                NotifyPropertyChanged(nameof(IsBeamPropertiesVisible));
                if (!bSuppressRefresh && !SuppressEvents)
                {
                    Message_Refresh msg = new() { SuppressTree = true, RangeDataOnly = true, UnitsToDisplay = DisplayUnits, SuppressDocumentClosure = true };
                    msg.UnitsToDisplay = DisplayUnits;

                    msg.Publish();
                }
            }




        }

        private int _SelectedTabIndex;
        public int SelectedTabIndex
        {
            get => _SelectedTabIndex;

            set
            {
                int prevValue = _SelectedTabIndex;
                _SelectedTabIndex = value;
                if (_SelectedTabIndex == 1)
                {
                    UpdateProjectVisibility();
                    Show_CaseOwners(true);

                }
                if (_SelectedTabIndex == 2)
                {
                    Show_CaseOwners(false);
                }

                NotifyPropertyChanged("SelectedTabIndex");
            }
        }
        private MDCaseOwnerViewModel _Selected_Distributor;
        public MDCaseOwnerViewModel Selected_Distributor
        {
            get => _Selected_Distributor;
            set
            {

                if (_Selected_Distributor != null && IsSpoutProject)
                    _Selected_Distributor.eventDoubleClickEventHandler -= RespondToDoubleClick_SelectedCaseOrOwner;

                if (value != null)
                {
                    if (IsSpoutProject)
                        value.eventDoubleClickEventHandler += RespondToDoubleClick_SelectedCaseOrOwner;

                    if (value.Distributor != null) value.Distributor.Cases.SetSelected(Selected_DistributorCaseIndex + 1);
                }
                _Selected_Distributor = value; NotifyPropertyChanged("Selected_Distributor");
                _Selected_DistributorIndex = _Selected_Distributor == null ? -1 : _Selected_Distributor.OwnerIndex - 1;
                NotifyPropertyChanged("Selected_DistributorIndex");
                Selected_DistributorCaseIndex = _Selected_Distributor == null ? -1 : _Selected_Distributor.SelectedCaseIndex - 1;


            }
        }


        private MDCaseViewModel _Selected_DistributorCase;
        public MDCaseViewModel Selected_DistributorCase
        {
            get => _Selected_DistributorCase;
            set
            {
                if (_Selected_DistributorCase != null && IsSpoutProject)
                    _Selected_DistributorCase.eventDoubleClickEventHandler -= RespondToDoubleClick_SelectedCaseOrOwner;

                _Selected_DistributorCase = value;
                if (value != null)
                {
                    value.Visibility_DistributorDependantProperties = VisibilityMDSpout;
                    value.Visibility_ChimneyTrayDependantProperties = Visibility.Collapsed;
                    if (IsSpoutProject)
                        value.eventDoubleClickEventHandler += RespondToDoubleClick_SelectedCaseOrOwner;
                }
                NotifyPropertyChanged("Selected_DistributorCase");

            }
        }

        private int _Selected_DistributorIndex = 0;
        public int Selected_DistributorIndex
        {
            get => _Selected_DistributorIndex;
            set
            {
                _Selected_DistributorIndex = value; NotifyPropertyChanged("Selected_DistributorIndex");
                mdDistributor owner = null;
                if (MDProject != null)
                {
                    MDProject.Distributors.SelectedIndex = value + 1;
                    owner = MDProject.Distributors.SelectedMember;
                    if (owner != null) owner.Cases.SelectedIndex = Selected_DistributorCaseIndex + 1;


                }
                MDCaseOwnerViewModel ownerview = new(MDProject, owner, bReadOnly: true);

                Selected_Distributor = ownerview;
                Visibility_CurrentDistributor = Selected_Distributor.Visibility;
                if (owner != null)
                    Selected_DistributorCaseIndex = owner.Cases.SelectedIndex - 1;
                else
                    Selected_DistributorCaseIndex = -1;

            }
        }
        private int _Selected_DistributorCaseIndex = 0;
        public int Selected_DistributorCaseIndex
        {
            get => _Selected_DistributorCaseIndex;
            set
            {
                _Selected_DistributorCaseIndex = value; NotifyPropertyChanged("Selected_DistributorCaseIndex");
                iCaseOwner owner = (MDProject != null) ? (iCaseOwner)MDProject.Distributors.SelectedMember : null;
                iCase ocase = null;

                if (owner != null)
                {
                    owner.Cases.SetSelected(value + 1);
                    ocase = (mdDistributorCase)owner.Cases.SelectedMember;
                }
                Selected_DistributorCase = new MDCaseViewModel(this, owner, ocase, true) { Visibility_DistributorDependantProperties = VisibilityMDSpout, Visibility_ChimneyTrayDependantProperties = Visibility.Collapsed };

            }
        }

        public int Selected_CaseOwnerIndex
        {
            get => SelectedTabIndex == 1 ? Selected_DistributorIndex : Selected_ChimneyTrayIndex;
            set
            {

                if (SelectedTabIndex == 1) Selected_DistributorIndex = value;
                if (SelectedTabIndex == 2) Selected_ChimneyTrayIndex = value;

            }

        }

        public int Selected_CaseIndex
        {
            get => SelectedTabIndex == 1 ? Selected_DistributorCaseIndex : Selected_ChimneyTrayCaseIndex;
            set
            {

                if (SelectedTabIndex == 1) Selected_DistributorCaseIndex = value;
                if (SelectedTabIndex == 2) Selected_ChimneyTrayCaseIndex = value;

            }

        }
        private List<string> _OwnerListD = new();
        public List<string> OwnerListD { get => _OwnerListD; set { value ??= new List<string>(); _OwnerListD = value; NotifyPropertyChanged("OwnerListD"); } }

        private List<string> _CaseListD = new();
        public List<string> CaseListD { get => _CaseListD; set { value ??= new List<string>(); _CaseListD = value; NotifyPropertyChanged("CaseListD"); } }

        private MDCaseOwnerViewModel _Selected_ChimneyTray;
        public MDCaseOwnerViewModel Selected_ChimneyTray
        {
            get => _Selected_ChimneyTray;
            set
            {
                if (_Selected_ChimneyTray != null && IsSpoutProject)
                    _Selected_ChimneyTray.eventDoubleClickEventHandler -= RespondToDoubleClick_SelectedCaseOrOwner;

                if (value != null)
                {
                    if (IsSpoutProject)
                        value.eventDoubleClickEventHandler += RespondToDoubleClick_SelectedCaseOrOwner;

                    if (value.ChimneyTray != null) value.ChimneyTray.Cases.SetSelected(Selected_ChimneyTrayCaseIndex + 1);
                }
                _Selected_ChimneyTray = value; NotifyPropertyChanged("Selected_ChimneyTray");
                _Selected_ChimneyTrayIndex = _Selected_ChimneyTray == null ? -1 : _Selected_ChimneyTray.OwnerIndex - 1;
                NotifyPropertyChanged("Selected_ChimneyTrayIndex");
                Selected_ChimneyTrayCaseIndex = _Selected_ChimneyTray == null ? -1 : _Selected_ChimneyTray.SelectedCaseIndex - 1;


            }
        }

        private MDCaseViewModel _Selected_ChimneyTrayCase;
        public MDCaseViewModel Selected_ChimneyTrayCase
        {
            get => _Selected_ChimneyTrayCase;
            set
            {
                if (_Selected_ChimneyTrayCase != null && IsSpoutProject)
                    _Selected_ChimneyTrayCase.eventDoubleClickEventHandler -= RespondToDoubleClick_SelectedCaseOrOwner;
                _Selected_ChimneyTrayCase = value;

                if (value != null)
                {
                    value.Visibility_DistributorDependantProperties = Visibility.Collapsed;
                    value.Visibility_ChimneyTrayDependantProperties = Visibility.Visible;
                    if (IsSpoutProject)
                        value.eventDoubleClickEventHandler += RespondToDoubleClick_SelectedCaseOrOwner;
                }

                NotifyPropertyChanged("Selected_ChimneyTrayCase");

            }
        }

        private int _Selected_ChimneyTrayIndex = 0;
        public int Selected_ChimneyTrayIndex
        {
            get => _Selected_ChimneyTrayIndex;
            set
            {
                _Selected_ChimneyTrayIndex = value; NotifyPropertyChanged("Selected_ChimneyTrayIndex");
                mdChimneyTray owner = null;
                if (MDProject != null)
                {
                    MDProject.ChimneyTrays.SelectedIndex = value + 1;
                    owner = MDProject.ChimneyTrays.SelectedMember;

                }
                Selected_ChimneyTray = new MDCaseOwnerViewModel(MDProject, owner, bReadOnly: true);
                Visibility_CurrentCT = Selected_ChimneyTray.Visibility;
                if (owner != null)
                {
                    owner.Cases.SelectedIndex = Selected_ChimneyTrayCaseIndex + 1;
                    Selected_ChimneyTrayCaseIndex = owner.Cases.SelectedIndex - 1;
                }
                else
                {
                    Selected_ChimneyTrayCaseIndex = -1;
                }


            }
        }

        private void RespondToDoubleClick_SelectedCaseOrOwner(PropertyControlViewModel aProperty)
        {
            if (aProperty == null || !IsSpoutProject) return;
            uppPartTypes ptype = aProperty.Property.PartType;
            switch (aProperty.PartType)
            {
                case uppPartTypes.ChimneyTray:
                case uppPartTypes.Distributor:
                    {
                        Edit_CaseOwner(aProperty.PartType == uppPartTypes.Distributor, aProperty.Tag, "EDIT");
                        break;
                    }
                case uppPartTypes.ChimneyTrayCase:
                case uppPartTypes.DistributorCase:
                    {
                        Edit_Case(aProperty.PartType == uppPartTypes.DistributorCase, aProperty.Tag, "EDIT");
                        break;
                    }

            }



        }

        private int _Selected_ChimneyTrayCaseIndex = 0;
        public int Selected_ChimneyTrayCaseIndex
        {
            get => _Selected_ChimneyTrayCaseIndex;
            set
            {
                _Selected_ChimneyTrayCaseIndex = value; NotifyPropertyChanged("Selected_ChimneyTrayCaseIndex");
                iCaseOwner owner = (MDProject != null) ? (iCaseOwner)MDProject.ChimneyTrays.SelectedMember : null;
                iCase ocase = null;

                if (owner != null)
                {
                    owner.Cases.SetSelected(value + 1);
                    ocase = (mdChimneyTrayCase)owner.Cases.SelectedMember;
                }
                Selected_ChimneyTrayCase = new MDCaseViewModel(this, owner, ocase, true) { Visibility_DistributorDependantProperties = Visibility.Collapsed, Visibility_ChimneyTrayDependantProperties = VisibilityMDSpout }; ;
            }
        }
        private List<string> _OwnerListC = new();
        public List<string> OwnerListC { get => _OwnerListC; set { value ??= new List<string>(); _OwnerListC = value; NotifyPropertyChanged("OwnerListC"); } }

        private List<string> _CaseListC = new();
        public List<string> CaseListC { get => _CaseListC; set { value ??= new List<string>(); _CaseListC = value; NotifyPropertyChanged("CaseListC"); } }


        private string _WarningButtonColor = "Black";
        public string WarningButtonColor
        {
            get => _WarningButtonColor;
            set { _WarningButtonColor = value; NotifyPropertyChanged("WarningButtonColor"); }

        }

        private static List<string> _EnglishScales;
        private List<string> Scales_English
        {
            get
            {
                if (_EnglishScales == null)
                {
                    _EnglishScales = new List<string>()
                {
                    NO_VALID_SCALE_ITEM_TEXT,
                    "1:1",
                    "1:2",
                    "1:4",
                    "1:5",
                    "1:8",
                    "1:10",
                    "1:12",
                    "1:16",
                    "1:20",
                    "1:30",
                    "1:40",
                    "1:50",
                    "2:1",
                    "4:1",
                    "8:1",
                };
                }

                return _EnglishScales;
            }

        }

        private List<string> _MetricScales;
        private List<string> Scales_Metric
        {
            get
            {
                if (_MetricScales == null)
                {
                    _MetricScales = new List<string>()
                {
                    NO_VALID_SCALE_ITEM_TEXT,
                    "1:1",
                    "1:2",
                    "1:3",
                    "1:4"
                };

                    for (int i = 0; i < 8; i++)
                    {
                        _MetricScales.Add($"1:{5 * (i + 1)}");
                    }
                }

                return _MetricScales;

            }
        }

        private UOPPropertyTreeViewModel _Tree_Distributors;
        public UOPPropertyTreeViewModel Tree_Distributors
        {
            get => _Tree_Distributors;
            set
            {
                if (_Tree_Distributors != null)
                {

                    _Tree_Distributors.NodeGotFocusEvent -= DistributorNodeGotFocus;
                }
                _Tree_Distributors = value;
                NotifyPropertyChanged("Tree_Distributors");
                if (_Tree_Distributors != null)
                {

                    _Tree_Distributors.NodeGotFocusEvent += DistributorNodeGotFocus;

                }
            }
        }

        private UOPPropertyTreeViewModel _Tree_ChimneyTrays;
        public UOPPropertyTreeViewModel Tree_ChimneyTrays
        {
            get => _Tree_ChimneyTrays;
            set
            {
                if (_Tree_ChimneyTrays != null) _Tree_ChimneyTrays.NodeGotFocusEvent -= ChimneyTrayNodeGotFocus;

                _Tree_ChimneyTrays = value;
                NotifyPropertyChanged("Tree_ChimneyTrays");
                if (_Tree_ChimneyTrays != null) _Tree_ChimneyTrays.NodeGotFocusEvent += ChimneyTrayNodeGotFocus;


            }
        }

        public bool IsBeamPropertiesVisible
        {
            get
            {
                bool isVisible =  SelectedTrayRange?.Assembly?.DesignFamily.IsBeamDesignFamily() ?? false;
                return isVisible;
            }
        }
        #endregion Properties


        #region Methods



        private void DistributorNodeGotFocus(TreeViewNode aNode)
        {
            //if (MDProject == null) return;
            //mdDistributor distrib = null;
            //mdDistributorCase dcase = null;

            //if (aNode.Part == null) return;
            //if(aNode.Part.PartType == uppPartTypes.Distributor)
            //{
            //    distrib = (mdDistributor)aNode.Part;

            //}
            //else if (aNode.Part.PartType == uppPartTypes.DistributorCase)
            //{
            //    dcase = (mdDistributorCase)aNode.Part;
            //    distrib = dcase.Distributor();
            //    distrib.Cases.SelectedIndex = dcase.Index;
            //}


            //if (distrib != null) 
            //{
            //    Selected_DistributorIndex = distrib.Index - 1;

            //}


        }

        private void ChimneyTrayNodeGotFocus(TreeViewNode aNode)
        {
            //if (MDProject == null) return;
            //mdChimneyTray chimney = null;
            //mdChimneyTrayCase dcase = null;

            //if (aNode.Part == null) return;
            //if (aNode.Part.PartType == uppPartTypes.ChimneyTray)
            //{
            //    chimney = (mdChimneyTray)aNode.Part;

            //}
            //else if (aNode.Part.PartType == uppPartTypes.ChimneyTrayCase)
            //{
            //    dcase = (mdChimneyTrayCase)aNode.Part;
            //    chimney = dcase.ChimneyTray();

            //}


            //if (chimney != null)
            //{
            //    MDProject.ChimneyTrays.SetSelected(chimney);
            //    Selected_ChimneyTray = new MDCaseOwnerViewModel(MDProject, MDProject.ChimneyTrays.SelectedMember, bReadOnly: true);
            //}


        }
        public bool UpdateTrayList(string aSelectedGUID = null)
        {
            bool _rVal = true;
            ObservableCollection<uopPart> oldlist = TrayList;
            oldlist ??= new ObservableCollection<uopPart>();
            List<string> oldnames = new List<string>();
            foreach (var item in oldlist)
            {
                oldnames.Add(item.SelectName);
            }
            ObservableCollection<uopPart> newlist = new ObservableCollection<uopPart>();

            string guid = string.IsNullOrWhiteSpace(aSelectedGUID) ? SelectedRangeGUID : aSelectedGUID;
            _SelectedRangeGUID = ""; NotifyPropertyChanged("SelectedRangeGUID");
            mdProject proj = MDProject;
            mdTrayRange range = null;
            int idx = -1;
            string selname = string.Empty;
            if (proj != null)
            {

                newlist = proj.TrayRanges.ToObservable();
                if (!string.IsNullOrWhiteSpace(guid)) proj.TrayRanges.SelectedRangeGUID = guid;
                guid = proj.TrayRanges.SelectedRangeGUID;
                range = (mdTrayRange)proj.TrayRanges.SelectedRange;
                idx = range == null ? -1 : range.Index - 1;
                selname = range == null ? string.Empty : range.SelectName;
            }
            if (oldlist.Count != newlist.Count)
            {
                _rVal = true;
            }
            else
            {
                List<uopPart> newparts = newlist.ToList();
                foreach (var item in oldnames)
                {
                    if (newparts.FindIndex((x) => string.Compare(item, x.SelectName, true) == 0) == -1)
                    {
                        _rVal = false;
                        break;
                    }
                }
            }


            bool supwuz = SuppressEvents;
            SuppressEvents = false;
            bool enabwuz = IsEnabled;
            IsEnabled = true;
            TrayList = newlist;

            _SelectedRangeGUID = guid; NotifyPropertyChanged("SelectedRangeGUID");
            _SelectedRangeIndex = idx; NotifyPropertyChanged("SelectedRangeIndex");
            _SelectedRangeName = selname; NotifyPropertyChanged("SelectedRangeName");
            SetSelectedRange(guid, bSuppressRefresh: true);
            SuppressEvents = supwuz;
            IsEnabled = enabwuz;
            return _rVal;
        }

        private void Show_CaseOwners(bool bDistributors, bool bDontRefreshLists = false)
        {
            uopCaseOwners owners = MDProject == null ? new colMDDistributors() : bDistributors ? MDProject.Distributors : MDProject.ChimneyTrays;
            Visibility_CurrentDistributor = (bDistributors && owners.Count > 0) ? Visibility.Visible : Visibility.Collapsed;
            Visibility_CurrentCT = (!bDistributors && owners.Count > 0) ? Visibility.Visible : Visibility.Collapsed;

            UOPPropertyTreeViewModel dvm = new(EventAggregator, DialogService);
            TreeViewNode node;
            ObservableCollection<TreeViewNode> nodes = new();
            node = bDistributors ? new TreeViewNode("DISTRIBUTORS") : new TreeViewNode("CHIMNEY TRAYS");
            uopProperties props;
            uopProperty prop;
            TreeViewNode propnode;
            iCase memcase;
            uopPart part;
            string subpath = bDistributors ? "Distributors" : "ChimneyTrays";
            uopDocuments warnings = null;
            colMDDistributors distribs = bDistributors ? (colMDDistributors)owners : null;
            colMDChimneyTrays cts = !bDistributors ? (colMDChimneyTrays)owners : null;

            bool showmechanical = false; // MDProject == null ? false : ProjectType == uppProjectTypes.MDDraw;
            for (int i = 1; i <= owners.Count; i++)
            {
                iCaseOwner owner = owners.Item(i);
                part = (uopPart)owner;
                props = part.Properties;
                props.DisplayUnits = DisplayUnits;
                warnings = bDistributors ? distribs.Item(i).GenerateWarnings(Project) : cts.Item(i).GenerateWarnings(Project); ;


                TreeViewNode childNode = new(owner.DescriptiveName, part, node, i) { Path = $"Project.{subpath}.Item({i})", IsBold = true, Colour = warnings.Count > 0 ? "Red" : "Black" };

                for (int p = 1; p <= props.Count; p++)
                {
                    prop = props.Item(p);
                    if (showmechanical || string.Compare(prop.Category, "MECHANICAL", true) != 0)
                    {
                        propnode = new TreeViewNode(prop.DisplaySignature, part, childNode, i) { Path = $"Project.{subpath}.Item({i}).Property({p})" };
                    }

                }

                if (warnings.Count > 0)
                {
                    TreeViewNode topnode = new($"Warnings", part, childNode) { Path = $"{childNode.Path}.Warnings", IsBold = true, Colour = "Red" };
                    for (int j = 1; j <= warnings.Count; j++)
                    {
                        uopDocWarning doc = (uopDocWarning)warnings.Item(j);
                        TreeViewNode warnnode = new($"{doc.TextString}", part, topnode) { Path = $"{childNode.Path}.Warnings({j})", IsBold = false, Colour = "Red" };

                    }
                }


                for (int j = 1; j <= owner.Cases.Count; j++)
                {
                    memcase = (iCase)owner.Cases.Item(j);
                    part = (uopPart)memcase;

                    TreeViewNode casenode = new($"Case - {memcase.Description}", part, childNode, j) { Path = $"{childNode.Path}.Case({j})", IsBold = true, Colour = "Blue" };
                    props = part.Properties;
                    props.DisplayUnits = DisplayUnits;
                    for (int p = 1; p <= props.Count; p++)
                    {
                        prop = props.Item(p);
                        if (!prop.IsNamed("Description"))
                        {
                            propnode = new TreeViewNode(prop.DisplaySignature, part, casenode, i) { Path = $"Project.{subpath}.Item({i}.Case({j}).Property({p})" };

                        }
                    }
                }

            }
            nodes.Add(node);

            dvm.TreeViewNodes = nodes;
            dvm.TreeViewNodes[0].IsExpanded = true;

            if (bDontRefreshLists) return;
            if (bDistributors)
            {
                OwnerListD = owners.Descriptions;
                CaseListD = owners.CaseDescriptions;
                Tree_Distributors = dvm;


            }
            else
            {
                OwnerListC = owners.Descriptions;
                CaseListC = owners.CaseDescriptions;
                Tree_ChimneyTrays = dvm;

            }

            if (Selected_CaseOwnerIndex < 0 || Selected_CaseOwnerIndex > owners.Count - 1)
            {
                iCaseOwner activeowner = owners.SelectedMember;
                Selected_CaseOwnerIndex = activeowner != null ? activeowner.Index - 1 : -1;
            }
            else { Selected_CaseOwnerIndex = Selected_CaseOwnerIndex; }


        }

        public void ShowErrors(List<string> aErrors, string drawingName)
        {
            if (aErrors == null || aErrors.Count <= 0) return;
            System.Diagnostics.Debug.Write(string.Join("\\n", aErrors));
            dxfUtils.DisplayErrors(aErrors, drawingName + " Drawing Errors");
        }

        public void DrawingTab_Show(string tabName, string drawingName = "", uopDocDrawing selectedDrawing = null)
        {
            if (selectedDrawing != null) selectedDrawing.Project = Project;


            UOPDocumentTab drawingTabItemData = _UOPDocumentTabs.Where(ctid => ctid.Header == tabName).FirstOrDefault();
            uopDocuments drawings;

            if (drawingTabItemData != null)
                selectedDrawing = (uopDocDrawing)drawingTabItemData.Document;

            if (selectedDrawing == null)
            {
                drawings = Project.Drawings();
                if (selectedDrawing == null && !string.IsNullOrWhiteSpace(tabName))
                {
                    selectedDrawing = drawings.Where(d => ((uopDocDrawing)d).TabName == tabName).OfType<uopDocDrawing>().FirstOrDefault();
                    if (selectedDrawing == null)
                    {
                        selectedDrawing = drawings.Where(d => ((uopDocDrawing)d).Name == tabName).OfType<uopDocDrawing>().FirstOrDefault();

                    }
                }
                if (selectedDrawing == null && !string.IsNullOrWhiteSpace(drawingName))
                {
                    selectedDrawing = drawings.Where(d => ((uopDocDrawing)d).Name == drawingName).OfType<uopDocDrawing>().FirstOrDefault();

                }
            }


            if (selectedDrawing == null) return;
            tabName = selectedDrawing.TabName;


            SelectedTabIndex = DrawingTab_Index;
            if (drawingTabItemData == null) //new drawing
                DrawingTab_CreateItemData(selectedDrawing);
            else
                DrawingTab_ShowDrawing(drawingTabItemData);


        }

        private void DrawingTab_ShowDrawing(UOPDocumentTab tabItemData)
        {
            // Bring up the tab for the selected drawing

            SelectedTabIndex = DrawingTab_Index; // Activate the Drawings tab
            DrawingTab_SelectedIndex = _UOPDocumentTabs.IndexOf(tabItemData); // Activate the corresponding drawing's tab

            // Bring up the tab for the selected drawing
            // Set the dxfImage that the DXFViewer user control should display

            // The CADfxViewer may be null for the first run when it has not been initialized yet.
            // The code that sets this reference, does the same operation when the Drawings TabControl content section is getting loaded for the first time.
            if (DXFViewerViewModel.CADfxViewer != null && (tabItemData.ViewModel.Image != null || tabItemData.ViewModel.CADModels != null))
            {
                if (tabItemData.ViewModel.CADModels == null && tabItemData.ViewModel.Image != null)
                {
                    DXFViewerViewModel.CADfxViewer.SetImage(tabItemData.ViewModel.Image);
                    if (tabItemData.ViewModel.titleBlockHelper != null)
                    {
                        tabItemData.ViewModel.titleBlockHelper.Insert(DXFViewerViewModel.CADfxViewer);
                    }
                    //DXFViewerViewModel.CADfxViewer?.Regenerate(tabItemData.ViewModel.Image, true);
                    if (!tabItemData.HasBeenInitialized)
                    {
                        DXFViewerViewModel.CADfxViewer.ZoomExtents();
                        tabItemData.HasBeenInitialized = true;
                    }
                    //save a clone of drawing model to the tabs item data
                    tabItemData.ViewModel.CADModels = DXFViewerViewModel.CADfxViewer.CADModels.Clone();
                    DrawingScale_Set(tabItemData);

                }
                else
                {
                    //show the saved model in the viewer
                    DXFViewerViewModel.CADfxViewer.SetModel(tabItemData.ViewModel.CADModels.Clone());
                    DrawingScale_Set(tabItemData, true);
                }

                // show the drawings scale

            }


        }

        private void DrawingTab_CreateItemData(uopDocDrawing drawing)
        {
            mdProject project = MDProject;
            if (project == null || _Worker_DrawingGenerate.IsBusy || DrawingTab_Loading) return;
            DrawingTab_Loading = true;
            mdTrayRange range = project.SelectedRange;

            WinTrayMainViewModel mainVM = (WinTrayMainViewModel)ParentVM;
            UOPDocumentTab newtab = new(drawing);
            newtab.DocMenu = Menu_GetDrawingMenu(newtab, out DelegateCommand closecmd);
            newtab.Command_Close = closecmd;


            drawing.BorderScale = "";

            newtab.ViewModel = new DXFViewerViewModel()
            {
                ZoomExtentCommand = Command_Drawing_ZoomExtents,
                ZoomInCommand = Command_Drawing_ZoomIn,
                ZoomOutCommand = Command_ZoomOut,
                ZoomWindowCommand = Command_Drawing_ZoomWindow,
                ZoomPreviousCommand = Command_Drawing_ZoomPrevious,
                SaveCommand = Command_Drawing_Save,
                SaveCommandAndOpen = Command_Drawing_SaveAndOpen,
                CloseCommand = Command_Drawing_Close,
                CloseAllCommand = Command_Drawing_CloseAll,
                RegenerateCommand = Command_Drawing_Regenerate,


            };
            //DXFViewer viewer = newtab.Viewer;
            //important to init true type fonts
            dxoFonts.Initialize();
            dxfImage image = new(appApplication.SketchColor);
            image.TextStyle().FontName = "Arial Narrow.ttf";
            //dxfUtils.ShowImageProperties(ref image);
            Message_Refresh request = new()
            {
                Silent = false,
                SuppressDocumentClosure = true,
                Drawing = drawing,
                SelectedTab = newtab,
                Image = image,
                StatusMessage = $"Generating Drawing '{drawing.Name}'"
            };

            //request.Viewer = viewer;
            //if (!async)
            //{
            //    //mainVM.MenuItemViewModelHelper.ToggleAppEnabled(false, true, request.StatusMessage);
            //    request.ToggleAppEnabled(false,aBusyMessage:request.StatusMessage));
            //    DoWorkEventArgs e = new DoWorkEventArgs(request);
            //    DoWork_Worker_DrawingGenerate(null, e);
            //    RunWorkerCompletedEventArgs ec = new RunWorkerCompletedEventArgs(e.Result, null, false);
            //    DoWork_Worker_DrawingGenerate_Complete(null, ec);

            //    request.ToggleAppEnabled());
            //}
            //else
            //{

            _Worker_DrawingGenerate.RunWorkerAsync(request);
            //}

        }

        public void DrawingTab_Close()
        {
            int curIndex = DrawingTab_SelectedIndex;
            //To prevent damage to the viewer, switch to another tab before the close operation
            if (DrawingTab_SelectedIndex > 0)
                DrawingTab_SelectedIndex -= 1;
            else if (DrawingTab_SelectedIndex == 0 && _UOPDocumentTabs.Count > 1)
                DrawingTab_SelectedIndex += 1;

            _UOPDocumentTabs[curIndex].ViewModel.Image = null;
            _UOPDocumentTabs[curIndex].ViewModel.CADModels = null;
            _UOPDocumentTabs.RemoveAt(curIndex);
            if (_UOPDocumentTabs.Count == 0)
            {
                //Clear the viewer if there are no remaining open drawings
                DXFViewerViewModel.CADfxViewer.Clear();
                DXFViewerViewModel.CADfxViewer = null;
            }
        }

        public void DrawingTab_Close(UOPDocumentTab aTab)
        {
            if (aTab == null) return;

            int curIndex = _UOPDocumentTabs.IndexOf(aTab);
            //To prevent damage to the viewer, switch to another tab before the close operation
            if (DrawingTab_SelectedIndex > 0)
                DrawingTab_SelectedIndex -= 1;
            else if (DrawingTab_SelectedIndex == 0 && _UOPDocumentTabs.Count > 1)
                DrawingTab_SelectedIndex += 1;

            _UOPDocumentTabs[curIndex].ViewModel.Image = null;
            _UOPDocumentTabs[curIndex].ViewModel.CADModels = null;
            _UOPDocumentTabs.RemoveAt(curIndex);
            if (_UOPDocumentTabs.Count == 0)
            {
                //Clear the viewer if there are no remaining open drawings
                DXFViewerViewModel.CADfxViewer.Clear();
                DXFViewerViewModel.CADfxViewer = null;
            }
        }

        public void DrawingTab_CloseAll()
        {
            if (_UOPDocumentTabs == null || _UOPDocumentTabs.Count <= 0) return;
            foreach (UOPDocumentTab tabItem in _UOPDocumentTabs)
            {
                tabItem.ViewModel.Image = null;
                tabItem.ViewModel.CADModels = null;
            }
            DXFViewerViewModel.CADfxViewer.Clear();
            DXFViewerViewModel.CADfxViewer = null;
            if (_UOPDocumentTabs.Count > 0) _UOPDocumentTabs.Clear();
            SelectedTabIndex = 0;
        }

        private string DrawingTab_SetName(uopDocDrawing aDrawing)
        {
            if (aDrawing == null) return string.Empty;
            string _rVal = aDrawing.Name;
            aDrawing.TabName = _rVal;
            mdProject project = MDProject;
            bool bCanc = false;
            if (project == null) return string.Empty;
            mdTrayRange range = project.SelectedRange;
            if (range == null) return string.Empty;
            mzQuestion question1 = aDrawing.TrayQuery.Item(1);
            string dname = string.Empty;
            string[] answers = null;
            if (aDrawing.RequiresTraySelection)
            {
                
                if (aDrawing.DrawingType == uppDrawingTypes.DowncomerDesign)
                {
                    bCanc = !aDrawing.TrayQuery.PromptForAnswers("Select Downcomer", true, aSaveButtonText: "Select");
                    if (bCanc) return null;
                    question1 = aDrawing.TrayQuery.Item(1);
                    string dcpn = aDrawing.TrayQuery.AnswerS(1, "");
                    mdDowncomerBox box = MDProject.GetParts().Boxes().Find((x) => string.Compare(x.PartNumber, dcpn, true) == 0);

                    if (box == null) return null;
                    aDrawing.TrayName = box.TrayName();
                    aDrawing.Range = box.GetMDRange();
                    aDrawing.Part = box;
                    dname = $"Downcomer Assembly {dcpn}";

                    

                }
                else
                {

                    if (question1.QuestionType == uopQueryTypes.SingleSelect)
                        question1.SetAnswer(range.TrayName(false));


                    bCanc = !aDrawing.TrayQuery.PromptForAnswers("Select Tray Range", true, aSaveButtonText: "Select");
                    if (bCanc) return null;
                    question1 = aDrawing.TrayQuery.Item(1);

                    string answerval = (string)question1.Answer;
                    answers = answerval.Split('|');

                    string rangeName = answers[0];
                    range = (mdTrayRange)project.TrayRanges.GetByName(rangeName);

                    if (range == null)
                    {
                        WinTrayMainViewModel mainVM = WinTrayMainViewModel.WinTrayMainViewModelObj;
                        mainVM.ShowMessageBox("The selected aDrawing requires a tray range to be defined.", "Missing Data", MessageBoxButton.OK, MessageBoxImage.Stop);
                        return string.Empty;
                    }


                    aDrawing.Range = range;
                    aDrawing.RangeGUID = range.GUID;
                    dname = aDrawing.Name;
                    string tname = range.TrayName(false);
                    if (!dname.EndsWith(tname))
                    {
                        int i = dname.IndexOf('-');
                        if (i >= 0)
                            dname = dname.Substring(0, i).Trim();

                        dname = $"{dname} - {tname}";

                    }

                    if(aDrawing.DrawingType == uppDrawingTypes.SGInputSketch)
                    {
                        aDrawing.Options.Clear();
                        List<string> sgnames = range.TrayAssembly.SpoutGroups.Names();
                        aDrawing.Options.AddSingleSelect("Select Spout Group", sgnames, bAnswerRequired: true, aChoiceDelimiter:"|");
                    }
                }





                //aDrawing.Name = dname;
                aDrawing.TabName = dname;
                if (aDrawing.RequiresDowncomerSelection & range != null)
                {
                    string answerval = (string)question1.Answer;
                    answers = answerval.Split('|');

                    int dcidx = (answers.Length >= 2) ? mzUtils.VarToInteger(mzUtils.TrailingInteger(answers[1])) : 0;
                    if (dcidx <= 0 || dcidx > range.Downcomers.Count)
                    {

                        WinTrayMainViewModel mainVM = WinTrayMainViewModel.WinTrayMainViewModelObj;
                        mainVM.ShowMessageBox("The selected aDrawing requires a downcomer index to be defined.", "Missing Data", MessageBoxButton.OK, MessageBoxImage.Stop);
                        return null;

                    }
                    mdDowncomer dc = range.Downcomers.Item(dcidx);
                    aDrawing.PartIndex = dc.Index;
                    aDrawing.TabName += $" - {dc.PartNumber}";
                }


            }

            if (aDrawing.RequiresOptionSelection)
            {
                bCanc = !aDrawing.Options.PromptForAnswers("Drawing Options", true);
                if(!bCanc && aDrawing.DrawingType == uppDrawingTypes.SGInputSketch)
                {
                  //  bCanc = true;

                    string sgname = aDrawing.Options.AnswerS(1,string.Empty).Split(' ')[2].Trim();
                    mdSpoutGroup sg = aDrawing.MDAssy.SpoutGroups.GetByHandle(sgname);
                    sg ??= aDrawing.MDAssy.SpoutGroups.Item(1);
                    aDrawing.Part = sg;
                    aDrawing.TabName += $" - {sg.Handle.Replace(",","-")}";
                    aDrawing.PartIndex = sg.Index;
                    aDrawing.Options.Clear();
                }

            }

            return aDrawing.TabName;
        }

        public void DrawingTab_Regenerate()
        {

            mdProject project = MDProject;

            if (project == null || DrawingTab_Loading || _Worker_DrawingRegenerate.IsBusy) return;

            UOPDocumentTab selectedTab = _UOPDocumentTabs[DrawingTab_SelectedIndex];
            if (selectedTab == null) return;

            string selectedDrawingName = selectedTab.Header;

            WinTrayMainViewModel mainVM = (WinTrayMainViewModel)ParentVM;

            if (selectedTab == null)
            {
                mainVM.ShowMessageBox($"The drawing name \"{selectedDrawingName}\" could not be found", "Drawing Regeneration Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            uopDocDrawing drawing = (uopDocDrawing)selectedTab.Document;
            DXFViewer viewer = DXFViewerViewModel.CADfxViewer;
            System.Drawing.Size siz = (viewer != null) ? viewer.Size : new System.Drawing.Size();
            dxoFonts.Initialize();
            dxfImage image = null;
            try
            {
                image = new(appApplication.SketchColor, siz);

            }
            catch { }
            image.TextStyle().FontName = "Arial Narrow.ttf";
            drawing.Name = selectedDrawingName;
            drawing.BorderScale = string.IsNullOrWhiteSpace(DrawingScale_Current) || DrawingScale_Current == NO_VALID_SCALE_ITEM_TEXT ? "" : DrawingScale_Current;


            Message_Refresh request = new()
            {
                Viewer = viewer,
                Silent = false,
                SuppressDocumentClosure = false,
                Drawing = drawing,
                SelectedTab = selectedTab,
                Image = image,
                StatusMessage = $"Regenerating Drawing '{drawing.Name}'",
            };


            try
            {

                if (drawing.RequiresOptionSelection)
                {
                    bool bCanc = !drawing.Options.PromptForAnswers("Drawing Options", true);
                    if (bCanc) return;
                }
                request.ToggleAppEnabled(false, request.StatusMessage);

                selectedTab.ViewModel.Image = null;
                DXFViewerViewModel.CADfxViewer?.Clear();

                bool async = true;
                if (!async)
                {
                    DoWorkEventArgs e = new(request);
                    DoWork_Worker_DrawingReGenerate(null, e);
                    RunWorkerCompletedEventArgs ec = new(e.Result, null, false);
                    DoWork_Worker_DrawingReGenerate_Complete(null, ec);
                }
                else
                {

                    _Worker_DrawingRegenerate.RunWorkerAsync(request);

                }
                return;
                //dxfImage regeneratedImage = GenerateImage(drawing);



            }
            catch (Exception e)
            {
                request.ErrorMessage = $"Exception thrown when regenerating drawing \"{selectedDrawingName}\":\r\n{e}";
            }
            finally
            {


                //if (!string.IsNullOrWhiteSpace(request.ErrorMessage))
                //    mainVM.ShowMessageBox(request.ErrorMessage, "Drawing Regeneration Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }


        }

        public bool DrawingTab_SaveDWG(string filePath, out string rMessage, bool bOpenOnSucces = false)
        {
            rMessage = "";
            if (dxfUtils.FileIsInUse(filePath))
            {

                rMessage = "File is in use!! Close it and try again.";
                return false;
            }

            bool _rVal = false;

            uopDocDrawing doc = UOPDocumentTabs[DrawingTab_SelectedIndex].Document as uopDocDrawing;

            bool scaleToMetric = doc.DrawingUnits == uppUnitFamilies.Metric;

            if (filePath.EndsWith(".dwg", StringComparison.OrdinalIgnoreCase))
            {
                _rVal = DXFViewerViewModel.CADfxViewer.SaveDwg(filePath, out rMessage, scaleToMetric);
            }
            else
            {
                if (filePath.EndsWith(".dxf", StringComparison.OrdinalIgnoreCase))
                {
                    _rVal = DXFViewerViewModel.CADfxViewer.SaveDxf(filePath, out rMessage, scaleToMetric);
                }
                else
                {
                    if (filePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                    {
                        _rVal = DXFViewerViewModel.CADfxViewer.SavePdf(filePath, out rMessage);
                    }
                }
            }

            if (_rVal && bOpenOnSucces)
            {
                try
                {
                    System.Diagnostics.Process.Start(filePath);
                }
                catch (Exception e)
                {
                    rMessage = e.Message;
                }

            }

            return _rVal;
        }

        public string ShowFileSelectionDialog()
        {
            Microsoft.Win32.SaveFileDialog fileDialog = new();
            fileDialog.Filter = "DWG files (*.dwg)|*.dwg|DXF files (*.dxf)|*.dxf|PDF files (*.pdf)|*.pdf";
            fileDialog.FileName = UOPDocumentTabs[DrawingTab_SelectedIndex].Header;
            fileDialog.Title = "Save Drawing To File";
            fileDialog.CheckFileExists = false;
            fileDialog.CheckPathExists = false;
            bool? result = fileDialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                return fileDialog.FileName;
            }
            else
            {
                return null;
            }
        }

        private void ProcessTabChangeEvent()
        {
            if (DrawingTab_SelectedIndex < 0 || DrawingTab_SelectedIndex >= UOPDocumentTabs.Count)
            {
                DrawingScales = new ObservableCollection<string>() { NO_VALID_SCALE_ITEM_TEXT };
                DrawingScale_Current = NO_VALID_SCALE_ITEM_TEXT;
                DrawingScale_Enabled = false;
                return;
            }

            UOPDocumentTab currentTab = UOPDocumentTabs[DrawingTab_SelectedIndex];

            DrawingScale_Set(currentTab, true);
        }

        private void DrawingScale_Set(UOPDocumentTab currentTab, bool suppressRedraw = false)
        {
            uopDocDrawing drawing = currentTab.Document as uopDocDrawing;
            string borderScale = drawing.BorderScale;
            bool borderScaleIsValid = DrawingScale_IsValid(borderScale);

            List<string> possibleScales = DrawingScale_GetScaleList(drawing);

            DrawingScale_Enabled = possibleScales.Count > 0;

            DrawingScales = new ObservableCollection<string>(possibleScales);
            DrawingScale_Save(borderScale);

            bool wuz = DrawingTab_Loading;
            DrawingTab_Loading = suppressRedraw;

            DrawingScale_Current = borderScaleIsValid ? borderScale : NO_VALID_SCALE_ITEM_TEXT;
            DrawingTab_Loading = wuz;
        }

        private List<string> DrawingScale_GetScaleList(uopDocDrawing drawing)
        {
            List<string> possibleScales = drawing.DrawingUnits switch
            {
                uppUnitFamilies.English => Scales_English,
                uppUnitFamilies.Metric => Scales_Metric,
                _ => new List<string>() { $"The \"{drawing.DrawingUnits}\" is not suppoerted" }
            };

            return possibleScales;
        }

        private void DrawingScale_Change(string selectedScale, uopDocDrawing drawing)
        {
            if (DrawingTab_Loading || drawing == null) return;
            if (selectedScale == NO_VALID_SCALE_ITEM_TEXT)
                selectedScale = "";
            else
            {
                if (!DrawingScale_IsValid(selectedScale)) selectedScale = drawing.BorderScale;
            }

            if (selectedScale != drawing.BorderScale)
            {
                drawing.BorderScale = selectedScale;
                DrawingTab_Regenerate();
            }



            //DrawingTab_Regenerate();
        }

        private bool DrawingScale_IsValid(string scaleString)
        {
            if (string.IsNullOrWhiteSpace(scaleString) || scaleString.Contains(".") || !scaleString.Contains(":"))
            {
                return false;
            }

            string[] parts = scaleString.Split(':');
            if (parts.Length != 2) return false;

            for (int i = 0; i < 2; i++)
            {
                if (!int.TryParse(parts[i], out int temp) || temp <= 0)
                {
                    return false;
                }
            }

            return true;
        }

        private (string, bool) DrawingScale_PromptForScale()
        {
            mzQuestions question = new("Enter Drawing Scale");

            string currentscale = DrawingScale_Current;
            if (!currentscale.Contains(':')) currentscale = "1:1";
            question.AddString("Input a New Drawing Scale: (Example 3:64)", currentscale, true);

            bool canceled = !question.PromptForAnswers("Enter Drawing Scale");

            string answer = canceled ? "" : (string)question.Item(1).Answer;

            return (answer, canceled);
        }

        public override void Activate(UserControl myUserControl)
        {
            if (Activated) return;
            base.Activate(myUserControl);
            //EventAggregator?.Subscribe(myUserControl);
            NotifyPropertyChanged("DesignOptionsLabel");

        }


        private void DrawingScale_Save(string newScaleString)
        {
            if (DrawingScale_IsValid(newScaleString) && !DrawingScales.Contains(newScaleString))
            {
                DrawingScales.Insert(1, newScaleString); // Keeps the newer ones at the beginning of the list, right after the first item.
            }
        }

        #endregion Methods

        #region Worker Code

        private void DoWork_Worker_DrawingGenerate(object sender, DoWorkEventArgs e)
        {
            Message_Refresh request = (Message_Refresh)e.Argument;
            if (request == null) return;
            uopDocDrawing drawing = request.Drawing;
            mdProject project = request.MDProject;
            if (drawing == null || project == null) return;
            drawing.UpdateDrawingUnits(project);

            System.Drawing.Size dwgsize = System.Drawing.Size.Empty;
            if (UOPDocumentTab.Viewer != null) request.Viewer = UOPDocumentTab.Viewer;
            if (request.Viewer != null) dwgsize = UOPDocumentTab.Viewer.Size;

            request.Drawing.DeviceSize = dwgsize;


            MDProjectViewModel projVM = (MDProjectViewModel)request.ProjectViewModel;
            request.Image ??= new dxfImage(appApplication.SketchColor, dwgsize);
            projVM ??= this;

            try
            {
                string stat1 = $"Generating Drawing '{request.Drawing.Name}'";
                request.ToggleAppEnabled(false, aBusyMessage: stat1);

                request.SetStatusMessages(stat1, "");


                request.DrawingSource ??= projVM.DrawingHelper(request.Image);
                request.DrawingSource.Image ??= request.Image;
                request.DrawingSource.Project = project;

                // create the drawing
                request.DrawingSource.GenerateImage(request.Drawing);

                request.SetStatusMessages($"Drawing Generation'{request.Drawing.Name}' Complete", "");


            }
            catch (Exception exp)
            {

                request.ErrorMessage = $"Exception thrown when generating drawing \"{request.Drawing.Name}\":\r\n{exp}";

            }
            finally
            {
                e.Result = request;


            }


        }

        private void DoWork_Worker_DrawingGenerate_Complete(object sender, RunWorkerCompletedEventArgs e)
        {
            Message_Refresh request = (Message_Refresh)e.Result;

            UOPDocumentTab tab = request.SelectedTab;
            IappDrawingSource source = request.DrawingSource;
            uopDocDrawing drawing = request.Drawing;
            List<string> errors = new();
            try
            {
                if (tab == null) return;

                if (drawing != null)
                {
                    tab.Range = drawing.Range;
                    tab.Part = drawing.Part;

                }


                request.ToggleAppEnabled(false, aBusyMessage: request.StatusMessage);

                Visibility_DrawingScaleControl = request.Drawing.BorderSize == uppBorderSizes.Undefined ? Visibility.Collapsed : Visibility.Visible;
                request.SetStatusMessages("Loading Drawing Viewer Image", "");

                tab.ViewModel.Image = request.Image;

                if (tab.ViewModel.Image != null && source != null)
                {

                    if (source.TitleHelper != null)
                    {
                        request.SetStatusMessages("Loading Drawing Viewer Image", "Adding Border");
                        tab.ViewModel.titleBlockHelper = source.TitleHelper.Clone();

                    }

                    _UOPDocumentTabs.Add(tab);

                    errors = request.Image.ErrorCol;

                    request.SetStatusMessages("Loading Drawing Viewer Image", "Updating Tab Viewer Control");



                    DrawingTab_ShowDrawing(tab);

                    source.TerminateObjects();
                }

            }

            catch (Exception ex)
            {
                errors.Add($"Drawing Generate : {ex.Message}");
            }
            finally
            {

                //if (request.Image != null)
                //{
                //    dxoTable styles = request.Image.Styles;
                //    dxoStyle style;
                //    for (int i = 1; i <= styles.Count; i++)
                //    {
                //        style = (dxoStyle)styles.get_Item(i);
                //        System.Diagnostics.Debug.WriteLine($"{style.Name} Font:{style.FontName} :: {style.FontStyle}");
                //    }


                //}


                request.SetStatusMessages("Loading Drawing Viewer Image", "Disposing DXF Image");
                request.Image = null;
                request.DrawingSource = null;
                request.ToggleAppEnabled(true);


                ShowErrors(errors, tab?.Header);
                DrawingTab_Loading = false;

            }
        }

        public void Task_ShowInputSketch(Message_Refresh refresh)
        {
            if (refresh == null) return;

            //await System.Threading.Tasks.Task.Run(() =>
            //{
            try
            {

                if (refresh.Image == null)
                    Viewer.Clear(); //saving the image will update the viewer
                else
                    Image = refresh.Image;

                SpoutRegions = null;
                DowncomerRegions = null;
                RefreshingInputSketch = false;
                IsEnabled = true;

                UpdateProjectVisibility();


            }
            catch (Exception ex)
            {
                refresh.HandleException(System.Reflection.MethodBase.GetCurrentMethod(), ex);
            }
            finally
            {
                UpdateProjectVisibility();
                //refresh.ReleaseReferences();
                //refresh.ToggleAppEnabled(true);

                HighlightSpoutGroup();


                //if (refresh.Message_ShowSpoutAreas != null)
                //{
                //    Message_ShowSpoutAreaDistibution msgsa = refresh.Message_ShowSpoutAreas;
                //    refresh.Message_ShowSpoutAreas = null;
                //    refresh.PublishMessage<Message_ShowSpoutAreaDistibution>(msgsa);
                //}

            }

            //});
        }


        public async System.Threading.Tasks.Task Task_GenerateInputSketch(Message_Refresh refresh)
        {

            if (refresh == null) return;
            await System.Threading.Tasks.Task.Run(() =>
            {

                refresh.Range ??= MDRange;
                mdTrayRange range = refresh.MDRange;

                refresh.Image = new dxfImage(appApplication.SketchColor, refresh.DeviceSize);
                if (range == null) return;
                refresh.ToggleAppEnabled(false, aBusyMessage: !refresh.Clear ? $"Refreshing Tray Sketch '{range.TrayName(false)}'" : "", aSubStatus: "");


                refresh.Image.TextStyle().FontName = "Arial Narrow.ttf";
                refresh.DrawingSource = DrawingHelper(refresh.Image);

                try
                {
                    //ToggleViewerVisibility(false, bShowWorking: true);

                    // recreate the image of the tray
                    refresh.Drawing = new uopDocDrawing(uppDrawingFamily.Sketch, "Input Sketch", uppDrawingTypes.InputSketch, range, bZoomExtents: false, bNoText: true);
                    if (refresh.DeviceSize != null) refresh.Drawing.DeviceSize = refresh.DeviceSize;

                    // this draws the input image  the tray to the current dxf Viewer
                    RefreshingInputSketch = true;
                    string stat1 = string.IsNullOrWhiteSpace(refresh.StatusMessage) ? $"Generating Drawing '{refresh.Drawing.Name}'" : refresh.StatusMessage;
                    refresh.ToggleAppEnabled(false, aBusyMessage: string.IsNullOrWhiteSpace(refresh.StatusMessage) ? $"Generating Drawing '{refresh.Drawing.Name}'" : refresh.StatusMessage);
                    refresh.SetStatusMessages(stat1, "");

                    if (refresh.Drawing.DeviceSize.IsEmpty)
                    {
                        refresh.Drawing.DeviceSize = (refresh.DeviceSize != null) ? refresh.DeviceSize : new System.Drawing.Size(0, 0);
                    }


                    refresh.DrawingSource ??= DrawingHelper(refresh.Image);

                    if (refresh.DrawingSource != null)
                    {

                        refresh.DrawingSource.Image = refresh?.Image;
                        try
                        {
                            RefreshingInputSketch = true;
                            //ToggleViewerVisibility(false, bShowWorking: true);
                            refresh.DrawingSource.GenerateImage(refresh.Drawing);
                        }
                        catch (Exception ex) { refresh.HandleException(MethodBase.GetCurrentMethod(), ex); }


                    }

                }
                catch (Exception ex)
                {
                    refresh.HandleException(MethodBase.GetCurrentMethod(), ex);
                }
                finally
                {
                    refresh.Image ??= new dxfImage(appApplication.SketchColor, refresh.DeviceSize);

                    //refresh.SetStatusMessages("", "");
                    //ToggleViewerVisibility(true, false);
                }
            });

        }
        private void DoWork_Worker_SketchGenerate(object sender, DoWorkEventArgs e)
        {
            Message_Refresh refresh = (Message_Refresh)e.Argument;
            refresh.Range ??= MDRange;
            mdTrayRange range = refresh.MDRange;
            refresh.ToggleAppEnabled(false, aBusyMessage: !refresh.Clear ? $"Refreshing Tray Sketch '{range.TrayName(false)}'" : "", aSubStatus: "");

            refresh.Image = new dxfImage(appApplication.SketchColor, refresh.DeviceSize);
            refresh.Image.TextStyle().FontName = "Arial Narrow.ttf";
            refresh.DrawingSource = DrawingHelper(refresh.Image);

            try
            {
                if (refresh.MDRange != null)
                {



                    // recreate the image of the tray
                    refresh.Drawing = new uopDocDrawing(uppDrawingFamily.Sketch, "Input Sketch", uppDrawingTypes.InputSketch, refresh.MDRange, bZoomExtents: false, bNoText: true);
                    if (refresh.DeviceSize != null) refresh.Drawing.DeviceSize = refresh.DeviceSize;

                    // this draws the input image  the tray to the current dxf Viewer
                    RefreshingInputSketch = true;
                    string stat1 = string.IsNullOrWhiteSpace(refresh.StatusMessage) ? $"Generating Drawing '{refresh.Drawing.Name}'" : refresh.StatusMessage;
                    refresh.ToggleAppEnabled(false, aBusyMessage: string.IsNullOrWhiteSpace(refresh.StatusMessage) ? $"Generating Drawing '{refresh.Drawing.Name}'" : refresh.StatusMessage);
                    refresh.SetStatusMessages(stat1, "");

                    if (refresh.Drawing.DeviceSize.IsEmpty)
                    {
                        refresh.Drawing.DeviceSize = (refresh.DeviceSize != null) ? refresh.DeviceSize : new System.Drawing.Size(0, 0);
                    }
                    refresh.Image ??= new dxfImage(appApplication.SketchColor, refresh.DeviceSize);

                    refresh.DrawingSource ??= DrawingHelper(refresh.Image);

                    if (refresh.DrawingSource != null)
                    {
                        refresh.DrawingSource.Image = refresh?.Image;
                        try { refresh.DrawingSource.GenerateImage(refresh.Drawing); } catch { }

                    }

                    //Thread.Sleep(250);
                    //GenerateImage(refresh);

                }
            }
            catch
            {

            }
            finally
            {

                refresh.SetStatusMessages("", "");
                e.Result = refresh;

            }
        }

        private void DoWork_Worker_SketchGenerate_Complete(object sender, RunWorkerCompletedEventArgs e)
        {
            Message_Refresh refresh = (Message_Refresh)e.Result;
            if (refresh == null) return;
            try
            {
                //saving the image will update the viewer
                Image = refresh.Image;
                if (Image == null) Viewer.Clear();


                refresh.SetStatusMessages("Drawing Complete", "Disposing Image");

                SpoutRegions = null;
                DowncomerRegions = null;
                RefreshingInputSketch = false;
                IsEnabled = true;
                if (!refresh.SuppressDocumentClosure) DrawingTab_CloseAll();
                UpdateProjectVisibility();
                if (refresh.Warnings != null)
                {
                    if (refresh.Warnings.Count > 0)
                    {
                        refresh.PublishMessage<Message_ShowWarnings>(new Message_ShowWarnings(refresh.Warnings));

                    }

                }


            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            finally
            {
                refresh.ReleaseReferences();
                refresh.ToggleAppEnabled(true);

                HighlightSpoutGroup();


                //if (refresh.Message_ShowSpoutAreas != null)
                //{
                //    Message_ShowSpoutAreaDistibution msgsa = refresh.Message_ShowSpoutAreas;
                //    refresh.Message_ShowSpoutAreas = null;
                //    refresh.PublishMessage<Message_ShowSpoutAreaDistibution>(msgsa);
                //}
                
            }


        }

        private void DoWork_Worker_DrawingReGenerate(object sender, DoWorkEventArgs e)
        {
            Message_Refresh request = (Message_Refresh)e.Argument;
            if (request == null) return;
            uopDocDrawing drawing = request.Drawing;
            mdProject project = request.MDProject;
            if (drawing == null || project == null) return;

            drawing.UpdateDrawingUnits(project);

            System.Drawing.Size dwgsize = System.Drawing.Size.Empty;

            if (UOPDocumentTab.Viewer != null) request.Viewer = UOPDocumentTab.Viewer;
            if (request.Viewer != null) dwgsize = UOPDocumentTab.Viewer.Size;


            request.Drawing.DeviceSize = dwgsize;


            MDProjectViewModel projVM = (MDProjectViewModel)request.ProjectViewModel;
            request.Image ??= new dxfImage(appApplication.SketchColor, dwgsize);
            projVM ??= this;

            try
            {
                string stat1 = $" Regenerating Drawing '{request.Drawing.Name}'";
                request.ToggleAppEnabled(false, aBusyMessage: stat1);

                request.SetStatusMessages(stat1, "");


                request.DrawingSource ??= projVM.DrawingHelper(request.Image);
                request.DrawingSource.Image ??= request.Image;
                request.DrawingSource.Project = project;

                // create the drawing
                request.DrawingSource.GenerateImage(request.Drawing);

                request.SetStatusMessages($"Drawing Regeneration'{request.Drawing.Name}' Complete", "");


            }
            catch (Exception exp)
            {

                request.ErrorMessage = $"Exception thrown when regenerating drawing \"{request.Drawing.Name}\":\r\n{exp}";

            }
            finally
            {
                e.Result = request;


            }


        }
        private void DoWork_Worker_DrawingReGenerate_Complete(object sender, RunWorkerCompletedEventArgs e)
        {
            Message_Refresh request = (Message_Refresh)e.Result;
            WinTrayMainViewModel mainVM = request.MainVM;
            mdProject project = MDProject;
            mdTrayRange range = MDRange;
            DXFViewer viewer = request.Viewer;
            UOPDocumentTab tab = request.SelectedTab;
            bool setisbusy = !request.Silent;
            bool closeDrawings = !request.SuppressDocumentClosure;
            //dxfImage image = request.Image;
            IappDrawingSource source = request.DrawingSource;
            List<string> errors = new();

            try
            {
                //mainVM.MenuItemViewModelHelper.ToggleAppEnabled(false,aBusyMessage:"Drawing Regeneration Complete");
                request.ToggleAppEnabled(false, aBusyMessage: "Drawing Regeneration Complete");
                if (tab != null)
                {
                    tab.ViewModel.Image = request.Image;
                    errors = tab.ViewModel.Image.ErrorCol;

                    if (viewer == null) viewer = DXFViewerViewModel.CADfxViewer;
                    request.SetStatusMessages("Loading Drawing Viewer Image", "SetImage");
                    if (viewer != null) viewer.SetImage(tab.ViewModel.Image);

                    if (source != null)
                    {
                        if (source.TitleHelper != null)
                        {
                            if (!source.SuppressBorder)
                            {
                                request.SetStatusMessages("Loading Drawing Viewer Image", "Adding Border");

                                tab.ViewModel.titleBlockHelper = source.TitleHelper.Clone();
                                tab.ViewModel.titleBlockHelper.Insert(DXFViewerViewModel.CADfxViewer);
                            }
                        }

                    }



                    request.SetStatusMessages("Loading Drawing Viewer Image", "Zooming TO Extents");
                    if (viewer != null) viewer.ZoomExtents();
                    request.SetStatusMessages("Loading Drawing Viewer Image", "Cloning CADModels");


                    tab.ViewModel.CADModels = DXFViewerViewModel.CADfxViewer.CADModels.Clone();


                }
            }
            catch (Exception ex)
            {
                errors.Add($"Drawing Generate : {ex.Message}");
            }
            finally
            {
                DrawingScale_Set(tab);

                request.SetStatusMessages("Loading Drawing Viewer Image", "Disposing DXF Image");


                if (tab != null) tab.ViewModel.Image = null;
                request.Image = null;

                //mainVM.MenuItemViewModelHelper.ToggleAppEnabled(true);
                request.ToggleAppEnabled(true);
                if (!string.IsNullOrEmpty(request.ErrorMessage))
                {
                    mainVM.ShowMessageBox(request.ErrorMessage, "Drawing Generation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                ShowErrors(errors, tab?.Header);

            }


        }

        #endregion Worker Code


        #region Commands



        private DelegateCommand _CMD_Drawing_ZoomExtents;
        public ICommand Command_Drawing_ZoomExtents
        {
            get
            {
                if (_CMD_Drawing_ZoomExtents == null)
                {
                    Action<object> executeAction = (object obj) =>
                    {
                        DXFViewerViewModel.CADfxViewer?.ZoomExtents();
                    };

                    _CMD_Drawing_ZoomExtents = new DelegateCommand(executeAction, (param) => true);
                }

                return _CMD_Drawing_ZoomExtents;
            }
        }

        DelegateCommand _CMD_Drawing_ZoomIn;
        public ICommand Command_Drawing_ZoomIn
        {
            get
            {
                if (_CMD_Drawing_ZoomIn == null)
                {
                    Action<object> executeAction = (object obj) => DXFViewerViewModel.CADfxViewer?.ZoomIn();
                    _CMD_Drawing_ZoomIn = new DelegateCommand(executeAction, (param) => true);
                }

                return _CMD_Drawing_ZoomIn;
            }
        }

        DelegateCommand Command_Drawing_ZoomOut;
        public ICommand Command_ZoomOut
        {
            get
            {
                if (Command_Drawing_ZoomOut == null)
                {
                    Action<object> executeAction = (object obj) => DXFViewerViewModel.CADfxViewer?.ZoomOut();
                    Command_Drawing_ZoomOut = new DelegateCommand(executeAction, (param) => true);
                }

                return Command_Drawing_ZoomOut;
            }
        }

        DelegateCommand _CMD_Drawing_ZoomWindow;
        public ICommand Command_Drawing_ZoomWindow
        {
            get
            {
                if (_CMD_Drawing_ZoomWindow == null)
                {
                    Action<object> executeAction = (object obj) => DXFViewerViewModel.CADfxViewer?.ZoomWindow();
                    _CMD_Drawing_ZoomWindow = new DelegateCommand(executeAction, (param) => true);
                }

                return _CMD_Drawing_ZoomWindow;
            }
        }

        DelegateCommand _CMD_Drawing_ZoomPrevious;
        public ICommand Command_Drawing_ZoomPrevious
        {
            get
            {
                if (_CMD_Drawing_ZoomPrevious == null)
                {
                    Action<object> executeAction = (object obj) =>
                    {
                        DXFViewerViewModel.CADfxViewer?.ZoomPrevious();
                    };

                    _CMD_Drawing_ZoomPrevious = new DelegateCommand(executeAction, (param) => true);
                }

                return _CMD_Drawing_ZoomPrevious;
            }
        }

        DelegateCommand _CMD_Drawing_Save;
        public DelegateCommand Command_Drawing_Save
        {
            get
            {
                if (_CMD_Drawing_Save == null)
                {


                    Action<object> executeAction = (object obj) =>
                    {
                        string selectedFilePath = ShowFileSelectionDialog();
                        if (!string.IsNullOrWhiteSpace(selectedFilePath))
                        {
                            if (!DrawingTab_SaveDWG(selectedFilePath, out string msg))
                            {
                                if (!string.IsNullOrWhiteSpace(msg))
                                {
                                    MessageBox.Show(msg, "Save Operation Status", MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                            }

                        }
                    };

                    _CMD_Drawing_Save = new DelegateCommand(executeAction, (param) => true);
                }

                return _CMD_Drawing_Save;
            }
        }


        DelegateCommand _CMD_Drawing_SaveAndOpen;
        public DelegateCommand Command_Drawing_SaveAndOpen
        {
            get
            {
                if (_CMD_Drawing_SaveAndOpen == null)
                {


                    Action<object> executeAction = (object obj) =>
                    {
                        string selectedFilePath = ShowFileSelectionDialog();
                        if (!string.IsNullOrWhiteSpace(selectedFilePath))
                        {
                            if (!DrawingTab_SaveDWG(selectedFilePath, out string msg, true))
                            {
                                if (!string.IsNullOrWhiteSpace(msg))
                                {
                                    MessageBox.Show(msg, "Save Operation Status", MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                            }

                        }
                    };

                    _CMD_Drawing_SaveAndOpen = new DelegateCommand(executeAction, (param) => true);
                }

                return _CMD_Drawing_SaveAndOpen;
            }
        }

        DelegateCommand _CMD_Drawing_Close;
        public DelegateCommand Command_Drawing_Close
        {
            get
            {
                if (_CMD_Drawing_Close == null)
                {
                    Action<object> executeAction = (object obj) =>
                    {
                        DrawingTab_Close();
                    };

                    _CMD_Drawing_Close = new DelegateCommand(executeAction, (param) => true);
                }

                return _CMD_Drawing_Close;
            }
        }

        DelegateCommand _CMD_Drawing_CloseAll;
        public DelegateCommand Command_Drawing_CloseAll
        {
            get
            {
                if (_CMD_Drawing_CloseAll == null)
                {
                    Action<object> executeAction = (object obj) =>
                    {
                        DrawingTab_CloseAll();
                    };

                    _CMD_Drawing_CloseAll = new DelegateCommand(executeAction, (param) => true);
                }

                return _CMD_Drawing_CloseAll;
            }
        }

        private DelegateCommand _CMD_Drawing_Regenerate;
        public DelegateCommand Command_Drawing_Regenerate
        {
            get
            {
                if (_CMD_Drawing_Regenerate == null)
                {
                    Action<object> executeAction = (object obj) =>
                    {
                        if (!DrawingTab_Loading) DrawingTab_Regenerate();
                    };

                    _CMD_Drawing_Regenerate = new DelegateCommand(executeAction, (param) => true);
                }

                return _CMD_Drawing_Regenerate;
            }
        }

        DelegateCommand _CMD_BatchProcessing;
        public ICommand Command_OpenBatchProcessingWindow
        {
            get
            {
                if (_CMD_BatchProcessing == null)
                {
                    Action<object> executeAction = (object param) =>
                    {
                        OpenBatchProcessingWindow();
                    };

                    _CMD_BatchProcessing = new DelegateCommand(executeAction, () => true);
                }

                return _CMD_BatchProcessing;
            }
        }

        DelegateCommand _Command_DrawingScale_Change;
        public ICommand Command_DrawingScale_Change
        {
            get
            {
                if (_Command_DrawingScale_Change == null)
                {
                    Action<object> executeAction = (object param) =>
                    {
                        if (DrawingTab_SelectedIndex < 0 || DrawingTab_SelectedIndex >= UOPDocumentTabs.Count || DrawingTab_Loading)
                        {
                            return;
                        }

                        uopDocDrawing drawing = UOPDocumentTabs[DrawingTab_SelectedIndex].Document as uopDocDrawing;

                        DrawingScale_Change(DrawingScale_Current, drawing);
                    };

                    _Command_DrawingScale_Change = new DelegateCommand(executeAction, () => true);
                }

                return _Command_DrawingScale_Change;
            }
        }

        DelegateCommand _Command_DrawingScale_Custom;
        public ICommand Command_DrawingScale_Custom
        {
            get
            {
                if (_Command_DrawingScale_Custom == null)
                {
                    Action<object> executeAction = (object param) =>
                    {
                        (string userEnteredScale, bool canceled) = DrawingScale_PromptForScale();

                        if (canceled) return;


                        if (!DrawingScale_IsValid(userEnteredScale))
                        {
                            MessageBox.Show("The entered scale string is not valid.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        if (DrawingTab_SelectedIndex < 0 || DrawingTab_SelectedIndex >= UOPDocumentTabs.Count)
                        {
                            return;
                        }

                        uopDocDrawing drawing = UOPDocumentTabs[DrawingTab_SelectedIndex].Document as uopDocDrawing;

                        DrawingScale_Save(userEnteredScale);

                        DrawingScale_Change(userEnteredScale, drawing);

                        DrawingScale_Current = userEnteredScale;
                    };

                    _Command_DrawingScale_Custom = new DelegateCommand(executeAction, () => true);
                }

                return _Command_DrawingScale_Custom;
            }
        }

        private DelegateCommand _CMD_Edit_Splicing;
        public ICommand Command_Edit_Splicing { get { _CMD_Edit_Splicing ??= new DelegateCommand(param => Edit_Splicing()); return _CMD_Edit_Splicing; } }

        private DelegateCommand _CMD_EditSlotting;
        public ICommand Command_EditSlotting { get { _CMD_EditSlotting ??= new DelegateCommand(param => Edit_Slotting()); return _CMD_EditSlotting; } }

        DelegateCommand _CMD_EditStiffeners;
        public ICommand Command_EditStiffeners { get { _CMD_EditStiffeners ??= new DelegateCommand(param => Edit_Stiffeners()); return _CMD_EditStiffeners; } }

        private DelegateCommand _CMD_EditProjectProperties;
        public ICommand Command_EditProperties_Project { get { _CMD_EditProjectProperties ??= new DelegateCommand(param => Edit_ProjectProperties("")); return _CMD_EditProjectProperties; } }

        private DelegateCommand _CMD_EditConstraints;
        public ICommand Command_EditConstraints { get { _CMD_EditConstraints ??= new DelegateCommand(param => Edit_Constraints()); return _CMD_EditConstraints; } }

        private DelegateCommand _CMD_CaseOwner_Add;
        public ICommand Command_CaseOwner_Add { get { _CMD_CaseOwner_Add ??= new DelegateCommand(param => Edit_CaseOwner(SelectedTabIndex == 1, "", "Add")); return _CMD_CaseOwner_Add; } }

        private DelegateCommand _CMD_CaseOwner_Edit;
        public ICommand Command_CaseOwner_Edit { get { _CMD_CaseOwner_Edit ??= new DelegateCommand(param => Edit_CaseOwner(SelectedTabIndex == 1, "", "Edit")); return _CMD_CaseOwner_Edit; } }

        private DelegateCommand _CMD_CaseOwner_Delete;
        public ICommand Command_CaseOwner_Delete { get { _CMD_CaseOwner_Delete ??= new DelegateCommand(param => Edit_CaseOwner(SelectedTabIndex == 1, "", "Delete")); return _CMD_CaseOwner_Delete; } }

        private DelegateCommand _CMD_CaseOwner_Copy;
        public ICommand Command_CaseOwner_Copy { get { _CMD_CaseOwner_Copy ??= new DelegateCommand(param => Edit_CaseOwner(SelectedTabIndex == 1, "", "Copy")); return _CMD_CaseOwner_Copy; } }

        private DelegateCommand _CMD_CaseOwner_Sort;
        public ICommand Command_CaseOwner_Sort { get { _CMD_CaseOwner_Sort ??= new DelegateCommand(param => Edit_CaseOwner(SelectedTabIndex == 1, "", "Sort")); return _CMD_CaseOwner_Sort; } }

        private DelegateCommand _CMD_CaseOwner_Rename;
        public ICommand Command_CaseOwner_Rename { get { _CMD_CaseOwner_Rename ??= new DelegateCommand(param => Edit_CaseOwner(SelectedTabIndex == 1, "", "Rename")); return _CMD_CaseOwner_Rename; } }



        private DelegateCommand _CMD_Case_Add;
        public ICommand Command_Case_Add { get { _CMD_Case_Add ??= new DelegateCommand(param => Edit_Case(SelectedTabIndex == 1, "", "Add")); return _CMD_Case_Add; } }

        private DelegateCommand _CMD_Case_Edit;
        public ICommand Command_Case_Edit { get { _CMD_Case_Edit ??= new DelegateCommand(param => Edit_Case(SelectedTabIndex == 1, "", "Edit")); return _CMD_Case_Edit; } }

        private DelegateCommand _CMD_Case_Delete;
        public ICommand Command_Case_Delete { get { _CMD_Case_Delete ??= new DelegateCommand(param => Edit_Case(SelectedTabIndex == 1, "", "Delete")); return _CMD_Case_Delete; } }

        private DelegateCommand _CMD_Case_Copy;
        public ICommand Command_Case_Copy { get { _CMD_Case_Copy ??= new DelegateCommand(param => Edit_Case(SelectedTabIndex == 1, "", "Copy")); return _CMD_Case_Copy; } }

        private DelegateCommand _CMD_Case_Sort;
        public ICommand Command_Case_Sort { get { _CMD_Case_Sort ??= new DelegateCommand(param => Edit_Case(SelectedTabIndex == 1, "", "Sort")); return _CMD_Case_Sort; } }
        private DelegateCommand _CMD_Case_Rename;
        public ICommand Command_Case_Rename { get { _CMD_Case_Rename ??= new DelegateCommand(param => Edit_Case(SelectedTabIndex == 1, "", "Rename")); return _CMD_Case_Rename; } }


        /// <summary>
        /// Edit spout area command.
        /// </summary>
        private DelegateCommand _CMD_EditSpoutArea;
        public ICommand Command_EditSpoutArea { get { _CMD_EditSpoutArea ??= new DelegateCommand(param => Edit_SpoutAreaDistribution()); return _CMD_EditSpoutArea; } }


        /// <summary>
        /// Command to show warnings window
        /// </summary>
        private DelegateCommand _CMD_ShowWarnings;
        public ICommand Command_ShowWarnings { get { _CMD_ShowWarnings ??= new DelegateCommand(param => ShowWarnings()); return _CMD_ShowWarnings; } }


        /// <summary>
        /// Command to Show reports
        /// </summary>
        private DelegateCommand _CMD_Reports;
        public ICommand Command_Reports { get { _CMD_Reports ??= new DelegateCommand(param => ShowReports()); return _CMD_Reports; } }

        #endregion Commands

        #region ContextMenus

        private DelegateMenu _Menu_DrawingCommands;
        public DelegateMenu Menu_DrawingCommands
        {
            get
            {
                if (_Menu_DrawingCommands == null)
                {
                    _Menu_DrawingCommands = new DelegateMenu("CM_Menu_DrawingCommands");
                    _Menu_DrawingCommands.Items.Add(new DelegateMenuItem("Regenerate", Command_Drawing_Regenerate));
                    _Menu_DrawingCommands.Items.Add(new Separator());
                    _Menu_DrawingCommands.Items.Add(new DelegateMenuItem("Close Tab", Command_Drawing_Close));
                    _Menu_DrawingCommands.Items.Add(new DelegateMenuItem("Close All Tabs", Command_Drawing_CloseAll));

                    _Menu_DrawingCommands.Items.Add(new Separator());
                    _Menu_DrawingCommands.Items.Add(new DelegateMenuItem("Save", Command_Drawing_Save));
                    _Menu_DrawingCommands.Items.Add(new DelegateMenuItem(@"Save & Open", Command_Drawing_SaveAndOpen));
                }
                return _Menu_DrawingCommands;
            }
        }

        public DelegateMenu Menu_GetDrawingMenu(UOPDocumentTab aTab, out DelegateCommand rCloseCommand)
        {

            Action<object> executeAction = (object obj) =>
            {
                DrawingTab_Close(aTab);
            };

            rCloseCommand = new DelegateCommand(executeAction, (param) => true) { DocumentTab = aTab };



            DelegateMenu _rVal = new("CM_Menu_DrawingCommands");
            _rVal.Items.Add(new DelegateMenuItem("Regenerate", Command_Drawing_Regenerate));
            _rVal.Items.Add(new Separator());
            _rVal.Items.Add(new DelegateMenuItem("Close", rCloseCommand));
            _rVal.Items.Add(new DelegateMenuItem("Close All Tabs", Command_Drawing_CloseAll));

            _rVal.Items.Add(new Separator());
            _rVal.Items.Add(new DelegateMenuItem("Save", Command_Drawing_Save));
            _rVal.Items.Add(new DelegateMenuItem(@"Save & Open", Command_Drawing_SaveAndOpen));
            return _rVal;

        }
        #endregion ContextMenus


        #region Event Handlers
        public void RespondToNodeClick(TreeViewNode selectedNode)
        {
            if (selectedNode == null || Project == null) return;
            if (selectedNode.DocumentType == uppDocumentTypes.Drawing)
            {

                uopDocDrawing selectedDrawing = (uopDocDrawing)selectedNode.Document;

                if (selectedDrawing == null) return;
                string tabName = DrawingTab_SetName(selectedDrawing);
                if (string.IsNullOrWhiteSpace(tabName)) return;
                DrawingTab_Show(tabName, selectedDrawing.Name, selectedDrawing);
            }

        }
        private void InputSketch_OnViewerMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && e.ClickCount == 1)
            {
                mdTrayRange range = MDRange;
                DXFViewer viewer = Viewer;
                if (range == null || viewer == null) return;

                mdTrayAssembly assy = range.TrayAssembly;
                colMDSpoutGroups spoutgroups = assy.SpoutGroups;
                dxfRectangle SGRegion = null;
                dxfRectangle DCRegion = null;
                mdSpoutGroup spoutgroup;
                colDXFRectangles spoutregions = SpoutRegions; //  assy.SpoutRegions;


                System.Windows.Point mouseCoordinate = e.GetPosition((System.Windows.IInputElement)sender);
                MousePoint = new dxfVector(mouseCoordinate.X, mouseCoordinate.Y, 0);

                (double, double) returnPoint = viewer.DeviceToWorld(mouseCoordinate.X, mouseCoordinate.Y);

                colDXFRectangles dcregions = DowncomerRegions; // assy.DowncomerRegions(true);

                MousePoint.SetCoordinates(returnPoint.Item1, returnPoint.Item2);
                if (spoutregions.EnclosesPoint(MousePoint, true, ref SGRegion))
                {

                    // set visibility of shape control
                    spoutgroups.SelectedIndex = SGRegion.Index >= 1 ? SGRegion.Index : 1;
                    spoutgroup = spoutgroups.SelectedMember;
                    assy.Downcomers.SetSelected(spoutgroup.DowncomerIndex);

                    HighlightSpoutGroup(assy, spoutgroup, SGRegion, dcregions.Item(spoutgroup.DowncomerIndex));
                    return;
                }
                if (dcregions.EnclosesPoint(MousePoint, true, ref DCRegion))
                {
                    assy.Downcomers.SetSelected(DCRegion.Index);
                    mdDowncomer dc = assy.Downcomers.SelectedMember;

                    spoutgroup = spoutgroups.SelectedMember;
                    if (spoutgroup.DowncomerIndex != dc.Index)
                    {
                        SGRegion = spoutregions.GetByNearestCenter(MousePoint);
                        spoutgroups.SelectedIndex = SGRegion.Index >= 1 ? SGRegion.Index : 1;
                        spoutgroup = spoutgroups.SelectedMember;
                        DCRegion = dcregions.Item(spoutgroup.DowncomerIndex);
                        // set visibility of shape control
                        HighlightSpoutGroup(assy, spoutgroup, SGRegion, DCRegion);
                    }

                    //if (spoutgroup != null && DCRegion.Index  != _SpoutGroup.DowncomerIndex)
                    //{
                    //    aDowncomer = Assy.Downcomers.Item(Region.Index);
                    //    dSG = aDowncomer.SpoutGroups(Assy);
                    //    aSpoutGroup = (dSG.Count > 0) ? dSG.Item(1) : null;

                    //    if (aSpoutGroup != null)
                    //    {
                    //        Assy.SpoutGroups.SelectedGroupHandle = aSpoutGroup.Handle;
                    //        HighlightSpoutGroup();
                    //    }
                    //}
                }
            }
            else if (e.LeftButton == MouseButtonState.Pressed && e.ClickCount == 2)
            {
                System.Windows.Point mouseCoordinate = e.GetPosition((System.Windows.IInputElement)sender);

                InvokeEditSpoutGroup(mouseCoordinate.X, mouseCoordinate.Y);

            }
        }

        public void PropertyList_DoubleClick(uopProperty aProperty, uppPartTypes aPartType)
        {
            if (aProperty == null) return;

            string pname = aProperty.Name;

            switch (aPartType)

            {

                case uppPartTypes.TrayRange:
                case uppPartTypes.TraySupportBeam:
                    Edit_Range(pname);
                    break;

                case uppPartTypes.Downcomer:
                    if (aProperty.IsNamed("Thickness"))
                    {
                        Edit_Materials("Downcomer");
                    }
                    else
                    {
                        Edit_Downcomer(pname);
                    }

                    break;

                case uppPartTypes.Deck:
                    if (aProperty.IsNamed("Thickness"))
                    {
                        Edit_Materials("Deck");
                    }
                    else
                    {
                        if (ProjectType == uppProjectTypes.MDSpout)
                        {
                            Edit_Deck(pname);
                        }
                        else
                        {
                            if (aProperty.IsNamed("SlottingPercentage,SlotType,Required Slot Count,Actual Slot Count"))
                            {
                                Edit_Slotting();
                            }
                            else
                            {
                                Edit_Deck(pname);
                            }

                        }


                    }

                    break;

                case uppPartTypes.DesignOptions:
                    if (ProjectType == uppProjectTypes.MDDraw)
                    {
                        if (aProperty.IsNamed("SpliceStyle,Splice Style"))
                        {
                            Edit_Splicing();
                        }
                        else
                        {
                            Edit_DesignOptions(pname);
                        }
                    }
                    else
                    {
                        if (aProperty.IsNamed("SA Distrib. Dev."))
                        {
                            Edit_SpoutAreaDistribution();
                        }
                    }
                    break;

                case uppPartTypes.Materials:
                    Edit_Materials(pname);
                    break;

                case uppPartTypes.StartupSpouts:
                    Edit_StartupSpouts(pname);
                    break;

            }


        }


        /// <summary>
        /// Handles updating the input sketch.
        /// </summary>
        /// <param name="message"></param>
        public void OnAggregateEvent(Message_ReleaseGraphics message)
        {
            ReleaseGraphics();
        }


        /// <summary>
        /// Handles updating the input sketch.
        /// </summary>
        /// <param name="message"></param>
        public void OnAggregateEvent(Message_RefreshGraphics message) => RefreshTraySketch(message.RefreshMessage);
        public void OnAggregateEvent(Message_ShowSpoutAreaDistibution message)
        {
            Edit_SpoutAreaDistribution();
        }


        public void OnAggregateEvent(Message_HighlightImageFromGridClick message)
        {
            HighlightDrawing(message);
        }

        public void OnAggregateEvent(Message_HighlightImage message)
        {
            if (message.PartType == uppPartTypes.SpoutGroup)
            {
                HighlightSpoutGroup();
            }
        }

        public void OnAggregateEvent(Message_ProjectChange message)
        {

            if (message.Project == null)
            {

                MDProject = null;
                ReleaseReferences();
                Message_Refresh refresh = new(bSuppressImage: false) { Clear = true };
                refresh.Publish();


                return;
            }

            if (message.Project.ProjectFamily == uppProjectFamilies.uopFamMD)
            {
                string curGUID = SelectedRangeGUID;
                bool sameproj = message.Project == MDProject;
                MDProject = (mdProject)message.Project;
                Execute_RefreshRangeCombo(bReselectCurentRange: true, message.SuppressRefresh);
                if (MDProject != null && curGUID != MDProject.SelectedRangeGUID)
                    if (!message.SuppressRefresh && MDProject != null) SetSelectedRange(MDProject.SelectedRangeGUID, message.SuppressRefresh);


            }

        }



        public void Execute_SpoutAreaCheck()
        {
            if (MDAssy == null) return;
            if (Math.Abs(MDAssy.MaximumDistributionDeviation) > MDProject.ConvergenceLimit)
            {
                Edit_SpoutAreaDistribution();
            }

        }
        /// <summary>
        /// Event handler called when reading of MPD file is completed 
        /// </summary>
        /// <param name="message"></param>
        public void OnAggregateEvent(Message_RefreshRangeList message)
        {
            if (message == null) return;

            Execute_RefreshRangeCombo(bReselectCurentRange: true, message.SuppressRefresh);
        }

        /// <summary>
        ///  Event handler to clear all data
        /// </summary>
        /// <param name="message"></param>
        public void OnAggregateEvent(Message_ClearData message)
        {
            if (message == null) return;
            if (message.PartType == uppPartTypes.Project || message.PartType == uppPartTypes.Undefined)
            {
                DrawingTab_CloseAll();
                TrayList = new ObservableCollection<uopPart>();
                Image?.Dispose();
                Image = null;
            }
        }

        /// <summary>
        /// handles for clean up
        /// </summary>
        /// <param name="message"></param>
        public override void OnAggregateEvent(Message_UnloadProject message)
        {
            IsEnabled = false;

            Project = null;
            MDRange = null;

            UpdateVisibily();
            //RangeData = new ObservableCollection<WintrayRange>();
            ReleaseGraphics();

            // Added by CADfx
            _UOPDocumentTabs?.Clear();
            DrawingTab_SelectedIndex = 0;
            SelectedTabIndex = 0;
            DXFViewerViewModel.CADfxViewer?.Clear();
            DXFViewerViewModel.CADfxViewer = null;
            // Added by CADfx

            Viewer?.Clear();

        }



        #endregion Event Handlers

        #region Functions

        public void ReleaseGraphics()
        {
            SpoutRegions = null;
            DowncomerRegions = null;

            DXFViewer viewer = Viewer;
            if (viewer != null) viewer.Clear();
            DrawingTab_CloseAll();
        }

        public void ShowReports()
        {

            EventAggregator.Publish<Message_DocumentRequest>(new Message_DocumentRequest(uppDocumentTypes.Report));


        }

        public void ShowWarnings(uopDocuments aWarnings = null)
        {

            EventAggregator.Publish<Message_DocumentRequest>(new Message_DocumentRequest(uppDocumentTypes.Warning) { Documents = aWarnings, RangeGUID = SelectedRangeGUID });



        }

        public void HighlightDrawing(Message_HighlightImageFromGridClick message)
        {

            try
            {
                if (message == null || MDProject == null) return;
                mdTrayRange aRange = MDRange;
                if (aRange == null) return;
                mdTrayAssembly assy = aRange.TrayAssembly;
                colMDSpoutGroups spoutgroups = assy.SpoutGroups;
                mdSpoutGroup spoutgroup = spoutgroups.SelectedMember;

                if (spoutgroup == null) return;



                if (message.IsSpoutGroup && message.Index >0)
                {
                    if (message.Index <= 0 || message.Index > spoutgroups.Count) return;
                    spoutgroup = spoutgroups.Item(message.Index, bSuppressIndexError:true);
                    if (spoutgroup == null) return;
                    dxfRectangle dcregion = DowncomerRegions.Item(spoutgroup.DowncomerIndex);
                    dxfRectangle sgregion = SpoutRegions.Item(spoutgroup.Index);
                    HighlightSpoutGroup(assy, spoutgroup, sgregion, dcregion);
                }
                else if (message.IsDowncomer && message.Index>0)
                {
                    mdDowncomer dc = assy.Downcomers.Item(message.Index, bSuppressIndexError:true);
                    if (dc == null) return;
                    assy.Downcomers.SetSelected(dc.Index);
                    if (spoutgroup != null && spoutgroup.DowncomerIndex != dc.Index) spoutgroup = null;
                    if(spoutgroup == null) spoutgroup = spoutgroups.GetByDowncomerIndex(dc.Index).Item(1, bSuppressIndexError: true);
                    if (spoutgroup == null) return;
                    spoutgroups.SetSelected(spoutgroup.Index);
                    dxfRectangle dcregion = DowncomerRegions.Item(dc.Index);
                    dxfRectangle sgregion = spoutgroup == null ? null : SpoutRegions.Item(spoutgroup.Index);
                    HighlightSpoutGroup(assy, spoutgroup, sgregion, dcregion);
                }
                else if (message.IsPanel && message.Index != spoutgroup.PanelIndex)
                {
                    spoutgroup = (mdSpoutGroup)spoutgroups.GetByPanelIndex(message.Index, bNonZero: true, bParentsOnly: true).GetByNearestCenter(spoutgroup.CenterDXF);
                    if (spoutgroup == null) return;
                    dxfRectangle dcregion = DowncomerRegions.Item(spoutgroup.DowncomerIndex);
                    dxfRectangle sgregion = SpoutRegions.Item(spoutgroup.Index);
                    HighlightSpoutGroup(assy, spoutgroup, sgregion, dcregion);
                }

            }
            catch
            {
                ReleaseReferences();
            }

        }


        public void HighlightSpoutGroup(mdTrayAssembly aAssy = null, mdSpoutGroup aSpoutGroup = null, dxfRectangle aSGRegion = null, dxfRectangle aDCRegion = null)
        {
            if (RefreshingInputSketch)
                return;

            aAssy ??= MDAssy;
            if (aAssy == null) return;
            aSpoutGroup ??= aAssy.SpoutGroups.SelectedMember;
            if (aSpoutGroup == null) return;

            aSGRegion ??= SpoutRegions.Item(aSpoutGroup.Index);
            if (aSGRegion == null || Viewer == null) return;
            OverlayHandler overlay = Viewer.overlayHandler;
            overlay?.DrawRectangle(aSGRegion, _SpoutGroupHighlightColor, 50);

            aDCRegion ??= DowncomerRegions.Item(aSpoutGroup.DowncomerIndex);
            if (aDCRegion != null)
            {
                overlay?.DrawRectangle(aDCRegion, _DowncomerHighlightColor, 50);
            }




            EventAggregator.Publish<Message_HighlightRowOnImageClick>(new Message_HighlightRowOnImageClick() { SpoutIndex = aSpoutGroup.Index, PanelIndex = aSpoutGroup.PanelIndex, DowncomerIndex = aSpoutGroup.DowncomerIndex });


        }


        public override void UpdateVisibily()
        {
            base.UpdateVisibily();

            bool supevnt = SuppressEvents;
            SuppressEvents = false;
            //if(!RefreshingInputSketch) 
            //    Visibility_Viewer = MDProject == null ? Visibility.Collapsed : Visibility.Visible;
            SuppressEvents = supevnt;
        }

        public void ToggleViewerVisibility(bool bVisible, bool? bShowWorking = null)
        {
            bool supevnts = SuppressEvents;
            SuppressEvents = false;

            if (Project == null)
            {

                Visibility_Viewer = Visibility.Collapsed;
                Visibility_ViewerProgress = Visibility.Collapsed;
                return;
            }
            Visibility_Viewer = bVisible ? Visibility.Visible : Visibility.Collapsed;
            if (bShowWorking.HasValue)
            {
                Visibility_ViewerProgress = bShowWorking.Value ? Visibility.Visible : Visibility.Collapsed;
            }
            SuppressEvents = supevnts;

        }

        private void Execute_RefreshRangeCombo(bool bReselectCurentRange = true, bool bSuppressRefresh = false)
        {
            bool enabl = IsEnabled;
            IsEnabled = true;
            bool supevnts = SuppressEvents;
            SuppressEvents = false;
            if (MDProject != null)
            {
                Trays = MDProject.TrayRanges;
                TrayList = MDProject.TrayRanges.ToObservable();
                SetSelectedRange(MDProject.SelectedRangeGUID, bSuppressRefresh);

            }
            else
            {
                TrayList = new ObservableCollection<uopPart>();
            }



            NotifyPropertyChanged("TrayList");
            NotifyPropertyChanged("SelectedRangeIndex");
            SuppressEvents = supevnts;
            IsEnabled = enabl;

        }



        /// <summary>
        /// To Display MD Spout Drawings
        /// </summary>
        /// <param name="aProject"></param>
        public void RefreshTraySketch(Message_Refresh aMessage)
        {

            if (aMessage == null || Viewer == null) return;

            aMessage.DeviceSize = Viewer.Size;
            mdTrayRange range = null;
            if (!aMessage.Clear)
            {
                range = MDRange;
                if (range == null) aMessage.Clear = true;
            }

            SpoutRegions = null;
            DowncomerRegions = null;

            if (_Worker_SketchGenerate.IsBusy)
                return;

            //dxoFonts.Initialize();

            //to dispose the old image
            Image = null;


            _Worker_SketchGenerate.RunWorkerAsync(aMessage);

        }





        /// <summary>
        /// To show the Edit SpoutGroup form.
        /// </summary>
        public void InvokeEditSpoutGroup(double x, double y)
        {
            mdTrayAssembly assy = MDAssy;
            if (assy == null) return;

            dxfRectangle Region = new();

            //oMousePt.SetCoordinates(x, y, 0);
            //double xMouseDc = x;
            //double yMouseDc = y;
            //double zMouseDc = 0.0;

            MousePoint = Viewer.DeviceToWorld(new dxfVector(x, y));


            if (SpoutRegions.EnclosesPoint(MousePoint, true, ref Region))
            {
                assy.SpoutGroups.SelectedIndex = Region.Index;

                Edit_SelectedSpoutGroup();
            }
            else if (DowncomerRegions.EnclosesPoint(MousePoint, true, ref Region))
            {
                assy.Downcomers.SelectedIndex = Region.Index;
                Edit_SelectedDowncomer();
            }
        }

        public void RespondToEdits(ViewModel_Base aEditForm, bool? result, uppPartTypes aPartType)
        {
            if (aEditForm == null || MDAssy == null || !result.HasValue) return;

            aEditForm.ParentVM = null;
            aEditForm.SuppressEvents = true;
            Message_Refresh refresh = null;
            if (result.Value == true)
            {
                MDProject.HasChanged = true;
                refresh = aEditForm.RefreshMessage;
            }
            aEditForm.ReleaseReferences();
            aEditForm.Dispose();
            IsEnabled = true;
            if (refresh != null)
            {
                refresh.PartType = aPartType;
                refresh.SetStatusMessages("", "");
                refresh.ToggleAppEnabled(true);
                refresh.Publish();
            }

        }

        /// <summary>
        /// Opens edit material screen 
        /// </summary>
        /// <param name="input"></param>
        public void Edit_Materials(string input)
        {
            if (MDProject == null) return;
            ParameterOverride[] inputParams = [new ParameterOverride("project", Project), new ParameterOverride("selectedFieldName", input)];
            Edit_Materials_ViewModel VM = ApplicationModule.Instance.Resolve<Edit_Materials_ViewModel>(inputParams);
            bool? result = DialogService.ShowDialog<Edit_Materials_View>(this, VM);
            RespondToEdits(VM, result, uppPartTypes.Materials);
        }

        /// <summary>
        /// method to show project properties dialogsbox
        /// </summary>
        public void Edit_ProjectProperties(string input)
        {
            if (MDProject == null) return;


            DialogService.ShowDialog<Edit_MDProject_View>(this, ApplicationModule.Instance.Resolve<Edit_MDProject_ViewModel>([new ParameterOverride("project", MDProject), new ParameterOverride("parentVM", this)]));


        }
        /// <summary>

        public void Edit_Stiffeners()
        {
            if (MDProject == null || MDAssy == null) return;
            if (ProjectType != uppProjectTypes.MDDraw) return;

            try
            {

                DialogService.ShowDialog<Edit_MDStiffeners_View>(this, ApplicationModule.Instance.Resolve<Edit_MDStiffeners_ViewModel>([new ParameterOverride("project", MDProject), new ParameterOverride("parentVM", this)]));


            }
            catch (Exception e)
            {
                MessageBox.Show($"An Error Occured:\n {e.Message}", caption: $"{System.Reflection.MethodBase.GetCurrentMethod()}", button: MessageBoxButton.OK, icon: MessageBoxImage.Error);
            }

        }

        /// <summary>
        /// Popup edit screen of Range properties
        /// </summary>
        /// <param name="input"></param>
        public void Edit_Range(string aPropertyName)
        {

            if (MDRange == null) return;
            string rangeGUID = SelectedRangeGUID;
            if (string.IsNullOrWhiteSpace(rangeGUID))
            {
                rangeGUID = MDProject?.SelectedRangeGUID;
            }
            if (string.IsNullOrWhiteSpace(rangeGUID)) return;

            ParameterOverride[] inputParams =
            [
                 new ParameterOverride("project", MDProject),
                 new ParameterOverride("fieldName", aPropertyName),
                 new ParameterOverride("rangeGUID", SelectedRangeGUID),
                  new ParameterOverride("allowLappingRingNumbers", true)
            ];
            Edit_MDRange_ViewModel VM = ApplicationModule.Instance.Resolve<Edit_MDRange_ViewModel>(inputParams);
            bool? result = DialogService.ShowDialog<Edit_MDRange_View>(this, VM);

            RespondToEdits(VM, result, uppPartTypes.TrayRange);


            //if (!string.IsNullOrWhiteSpace(rangeGUID)) { SelectedRangeGUID = rangeGUID; } else { SelectedRangeGUID = MDProject.SelectedRangeGUID; }
        }

        public void Edit_SelectedDowncomer()
        {
            mdTrayAssembly assy = MDAssy;
            if (assy == null) return;
            mdDowncomer dc = assy.Downcomers.SelectedMember;
            if (dc == null) return;
            ParameterOverride[] inputParams =
            [
                new ParameterOverride("project", MDProject),
                new ParameterOverride("dcindex", dc.Index),
                new ParameterOverride("fieldname", "")
            ];

            Edit_MDDowncomer_ViewModel VM = ApplicationModule.Instance.Resolve<Edit_MDDowncomer_ViewModel>(inputParams);
            bool? result = DialogService.ShowDialog<Edit_MDDowncomer_View>(this, VM);
            RespondToEdits(VM, result, uppPartTypes.Downcomer);

        }

        /// <summary>
        /// Popup edit screen of  Range.Assemby.Donwcmer properties
        /// </summary>
        /// <param name="input"></param>
        public void Edit_Downcomer(string input)
        {
            if (MDRange == null) return;
            ParameterOverride[] inputParams = [new ParameterOverride("project", MDProject), new ParameterOverride("dcindex", 0), new ParameterOverride("fieldname", input)];

            Edit_MDDowncomer_ViewModel VM = ApplicationModule.Instance.Resolve<Edit_MDDowncomer_ViewModel>(inputParams); // new Edit_MDDowncomer_ViewModel(MDProject, 0,input);

            bool? result = DialogService.ShowDialog<Edit_MDDowncomer_View>(this, VM);
            RespondToEdits(VM, result, uppPartTypes.Downcomer);


        }
        /// Popup edit screen of Range.Assemby.Deck properties
        /// </summary>
        /// <param name="input"></param>
        public void Edit_Deck(string input)
        {
            if (MDRange == null) return;
            ParameterOverride[] inputParams = [new ParameterOverride("project", MDProject), new ParameterOverride("fieldname", input)];

            Edit_MDDeck_ViewModel VM = ApplicationModule.Instance.Resolve<Edit_MDDeck_ViewModel>(inputParams);  //new Edit_MDDeck_ViewModel(MDProject, input);

            bool? result = DialogService.ShowDialog<Edit_MDDeck_View>(this, VM);
            RespondToEdits(VM, result, uppPartTypes.Deck);
        }


        /// <summary>
        /// Popup edit screen of Range.Assemby.Deck properties
        /// </summary>
        /// <param name="input"></param>
        public void Edit_DesignOptions(string input)
        {

            if (MDRange == null || ProjectType == uppProjectTypes.MDSpout) return;

            ParameterOverride[] inputParams = [new ParameterOverride("project", MDProject), new ParameterOverride("fieldname", input)];

            Edit_MDDesignOptions_ViewModel VM = ApplicationModule.Instance.Resolve<Edit_MDDesignOptions_ViewModel>(inputParams);  //new Edit_MDDesignOptions_ViewModel(MDProject, input);

            bool? result = DialogService.ShowDialog<Edit_MDDesignOptions_View>(this, VM);
            RespondToEdits(VM, result, uppPartTypes.DesignOptions);


        }


        /// <summary>
        /// Popup edit screen of Project.Distributor properties
        /// </summary>
        /// <param name="input"></param>
        public void Edit_CaseOwner(bool bDistributor, string input, string mode, iCaseOwner aOwnerToEdit = null)
        {

            if (MDProject == null) return;
            if (string.IsNullOrWhiteSpace(mode)) return;
            mode = mode.Trim().ToUpper();
            uopCaseOwners owners = bDistributor ? MDProject.Distributors : MDProject.ChimneyTrays;
            iCaseOwner owner = aOwnerToEdit ?? owners.SelectedMember;


            string description = owner != null ? owner.Description : "";
            bool editowner = false;
            bool refreshtree = false;
            string ownertypename = owners.OwnerType.GetDescription();
            List<string> descriptions;
            bool reloadcombos = false;
            int selectedownerindex = owners.SelectedIndex;
            int selectedcaseindex = bDistributor ? Selected_DistributorCaseIndex + 1 : Selected_ChimneyTrayCaseIndex + 1;
            mzQuestion question2 = null;
            mzQuestion question1 = null;
            List<int> caseindextoedit = new();
            bool editallcases = false;
            switch (mode)
            {
                case "ADD":
                case "EDIT":
                    {
                        if (mode == "EDIT" && owner == null) return;

                        if (mode == "ADD")
                        {
                            descriptions = new List<string> { "" };
                            descriptions.AddRange(owners.Descriptions);
                            mzQuestions questions = new();
                            description = bDistributor ? $"Distrib {owners.Count + 1}" : $"Tray {owners.Count + 1}";
                            question1 = questions.AddSingleSelect($"Select {ownertypename} To Copy", descriptions);
                            question2 = questions.AddString($"Enter a {ownertypename} Description", description, true);
                            question2.UnacceptableAnswers = descriptions;
                            question2.UnacceptableAnswerMessage = $"Names for {ownertypename}s must be unique. ";
                            bool canc = !questions.PromptForAnswers($"Add {ownertypename}", aSaveButtonText: "Add");

                            if (canc) return;
                            description = question2.AnswerS;
                            owner = null;
                            question1 ??= new mzQuestion(uopQueryTypes.StringValue, "");
                            if (!string.IsNullOrWhiteSpace(question1.AnswerS))
                            {
                                uopPart ownerpart = owners.Find(x => string.Compare(x.Description, question1.AnswerS, true) == 0);
                                if (ownerpart != null)
                                {
                                    owner = (iCaseOwner)ownerpart;
                                    owner = owner.Clone();
                                    owner.Description = description;

                                }
                                else
                                {
                                    question1.Answer = "";
                                }
                            }

                            owner ??= bDistributor ? new mdDistributor() { Description = description } : new mdChimneyTray() { Description = description };

                        }
                        if (string.IsNullOrWhiteSpace(input))
                        {
                            input = "NozzleLabel";
                        }

                        ParameterOverride[] inputParams =
                       [
                            new ParameterOverride("parentVM", this),
                            new ParameterOverride("owner", owner),
                            new ParameterOverride("mode", mode),
                            new ParameterOverride("fieldname", input),
                            new ParameterOverride("title", $"Edit {ownertypename} Properties" ) ,
                            new ParameterOverride("headertext",  $"MD {ownertypename} Data Input" ) ,
                            new ParameterOverride("statustext", $"MD Project({MDProject.Name}).{ownertypename}s({owner.Description})" ) ,

                       ];
                        Edit_CaseOwner_ViewModel VM = ApplicationModule.Instance.Resolve<Edit_CaseOwner_ViewModel>(inputParams);
                        bool? result = DialogService.ShowDialog<Edit_CaseOwner_View>(this, VM);
                        // PropertyControlContainer container = new MDCaseOwnerViewModel(MDProject, owner, false);
                        //  ParameterOverride[] inputParams = new ParameterOverride[]
                        //{
                        //     new ParameterOverride("parentVM", this),
                        //     new ParameterOverride("container", container),
                        //     new ParameterOverride("mode", mode),
                        //     new ParameterOverride("fieldname", input),
                        //     new ParameterOverride("title", $"Edit {ownertypename} Properties" ) ,
                        //     new ParameterOverride("headertext",  $"MD {ownertypename} Data Input" ) ,
                        //     new ParameterOverride("statustext", $"MD Project({MDProject.Name}).{ownertypename}s({owner.Description})" ) ,

                        //};
                        // Edit_PropertyControlContainer_ViewModel VM = ApplicationModule.Instance.Resolve<Edit_PropertyControlContainer_ViewModel>(inputParams);
                        // bool? result = DialogService.ShowDialog<Edit_PropertyControlContainer_View>(this, VM);

                        uopProperties changes = VM.Mode == "EDIT" ? VM.GetEditedProperties() : VM.EditProps;

                        if (result.HasValue && VM.Mode == "ADD")
                        {
                            if (!VM.Canceled) result = true;
                        }
                        if (result.HasValue)
                        {


                            if (result == true)
                            {




                                if (VM.Mode == "EDIT")
                                {

                                    refreshtree = owners.SelectedMember.SetCurrentProperties(changes);
                                    reloadcombos = true;

                                }
                                else if (VM.Mode == "ADD")
                                {
                                    question1 ??= new mzQuestion(uopQueryTypes.StringValue, "");
                                    iCaseOwner newowner = bDistributor ? new mdDistributor() : new mdChimneyTray();
                                    if (!string.IsNullOrWhiteSpace(question1.AnswerS))
                                    {
                                        owner = (iCaseOwner)owners.Find(x => string.Compare(x.Description, question1.AnswerS, true) == 0);

                                    }

                                    newowner.SetCurrentProperties(changes);
                                    uopPart prt = (uopPart)newowner;
                                    prt.SubPart(owners);

                                    owners.Add(newowner);
                                    reloadcombos = true;
                                    refreshtree = true;
                                    selectedownerindex = owners.Count;

                                    editallcases = true;
                                }

                            }

                        }

                        break;
                    }
                case "DELETE":
                    {
                        if (owners.Count <= 0) return;
                        descriptions = owners.Descriptions;
                        mzQuestions questions = new();
                        string prompt = $"Select {ownertypename}s To Delete";

                        question1 = questions.AddMultiSelect(prompt, descriptions, owner.Description, aChoiceDelimiter: "~");

                        bool canceled = !questions.PromptForAnswers($"Select  Project {ownertypename}s To Delete", aSaveButtonText: "Delete");
                        if (canceled) return;
                        descriptions = question1.AnswersList();
                        foreach (string item in descriptions)
                        {

                            owner = owners.Remove(item);
                            if (owner != null) refreshtree = true;

                        }
                        selectedownerindex = selectedownerindex > 1 ? selectedownerindex - 1 : owners.Count;
                        reloadcombos = refreshtree;
                        break;
                    }

                case "COPY":
                    {
                        if (owners.Count <= 0) return;

                        descriptions = owners.Descriptions;

                        //if (DialogService.ShowMessageBox(this, $"Copy Distributor '{distributor.Description}' ?", "Copy Project Distributor", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.No) return;
                        mzQuestions questions = new();
                        question1 = questions.AddSingleSelect($"Select {ownertypename} To Copy", descriptions, owner.Description);
                        question2 = questions.AddString("Enter New Description", bDistributor ? $"Distrib {owners.Count + 1}" : $"Tray {owners.Count + 1}", true, 25);
                        question2.UnacceptableAnswers = descriptions;
                        question2.UnacceptableAnswerMessage = $"Names for {ownertypename}s must be unique. ";
                        bool canceled = !questions.PromptForAnswers($"Copy Project {ownertypename}", aSaveButtonText: "Copy");
                        if (canceled) return;

                        List<iCaseOwner> dmembers = owners.ToList();
                        owner = dmembers.Find(x => string.Compare(x.Description, question1.AnswerS, true) == 0);
                        if (owner == null) return;
                        iCaseOwner newowner = owner.Clone();
                        newowner.Description = question2.AnswerS;
                        newowner = owners.Add(newowner);
                        owners.SetSelected(newowner);
                        Edit_CaseOwner(bDistributor, "NozzleLable", "EDIT", newowner);

                        refreshtree = true;
                        //editowner = true;
                        reloadcombos = true;
                        selectedownerindex = owners.Count;
                        editallcases = true;

                        break;
                    }

                case "SORT":
                    {
                        if (owners.Count <= 1) return;
                        List<string> outputlist;

                        string title = $"Sort {ownertypename}s";

                        descriptions = owners.Descriptions;
                        ParameterOverride[] inputParams = [new ParameterOverride("aListToSort", descriptions), new ParameterOverride("aTitle", title)];
                        SortListViewModel VM = ApplicationModule.Instance.Resolve<SortListViewModel>(inputParams);
                        bool? result = DialogService.ShowDialog<SortListView>(this, VM);
                        if (result.HasValue && result == true)
                        {


                            List<iCaseOwner> ownerslist = owners.ToList();
                            List<iCaseOwner> newowners = new();
                            outputlist = VM.OutputList;
                            foreach (string item in outputlist)
                            {
                                owner = ownerslist.Find(x => string.Compare(x.Description, item, true) == 0);
                                if (owner != null) newowners.Add(owner);
                            }
                            owners.Populate(newowners);
                            selectedownerindex = owners.ToList().FindIndex(x => string.Compare(x.Description, description, true) == 0) + 1;

                            refreshtree = true;
                            reloadcombos = true;
                        }


                        break;
                    }
                case "RENAME":
                    {
                        if (owner == null) return;
                        descriptions = owners.Descriptions;
                        description = owner.Description;
                        descriptions.Remove(description);
                        mzQuestions questions = new();
                        question1 = questions.AddString($"Enter New {ownertypename} Description", description, true, 25);
                        question1.UnacceptableAnswers = descriptions;
                        question1.UnacceptableAnswerMessage = $"Names for {ownertypename}s must be unique. ";
                        bool canceled = !questions.PromptForAnswers($"Rename {ownertypename} '{description}'", aSaveButtonText: "Rename");
                        description = question1.AnswerS.Trim();
                        if (canceled || string.IsNullOrWhiteSpace(description)) return;
                        refreshtree = string.Compare(owner.Description, description, false) != 0;
                        if (refreshtree) owner.Description = description;
                        reloadcombos = refreshtree;
                        break;
                    }
            }

            if (editallcases)
            {
                for (int i = 1; i <= owners.MaxCaseCount; i++)
                {
                    caseindextoedit.Add(i);
                }
            }

            if (refreshtree)
            {
                MDProject.HasChanged = true;
                Show_CaseOwners(bDistributor, !reloadcombos);
                if (reloadcombos)
                    ReloadCaseAndCaseOwnerCommbos(bDistributor, selectedownerindex, selectedcaseindex, caseindextoedit);
                Message_Refresh refresh = new(true, true, false, true, bCloseDocuments: false);
                refresh.Publish();

                if (editowner) Edit_CaseOwner(bDistributor, "", "EDIT");
            }



        }

        private void ReloadCaseAndCaseOwnerCommbos(bool bDistributors, int? aOwnerIndex = null, int? aCaseIndex = null, List<int> aCaseListToEdit = null)
        {
            if (MDProject == null) return;
            uopCaseOwners owners = bDistributors ? MDProject.Distributors : MDProject.ChimneyTrays;

            if (aOwnerIndex.HasValue) owners.SelectedIndex = aOwnerIndex.Value;
            if (aCaseIndex.HasValue) owners.SetSelectedCase(aCaseIndex.Value);
            aCaseListToEdit ??= new List<int>();
            iCaseOwner owner = owners.SelectedMember;
            iCase ocase = owner == null ? null : (iCase)owner.Cases.SelectedMember;
            int casecnt = 0;
            List<string> casenames = new();
            if (bDistributors)
            {


                OwnerListD = owners.Descriptions;
                Selected_DistributorIndex = (owner != null) ? owner.Index - 1 : -1;

                CaseListD = owners.CaseDescriptions;
                Selected_DistributorCaseIndex = ocase != null ? ocase.Index - 1 : -1;
                casecnt = CaseListD.Count;
                casenames = CaseListD;
            }
            else
            {


                OwnerListC = owners.Descriptions;
                Selected_ChimneyTrayIndex = (owner != null) ? owner.Index - 1 : -1;

                CaseListC = owners.CaseDescriptions;
                Selected_ChimneyTrayCaseIndex = ocase != null ? ocase.Index - 1 : -1;
                casecnt = CaseListC.Count;
                casenames = CaseListC;

            }
            foreach (int item in aCaseListToEdit)
            {
                if (item > 0 & item <= casecnt)
                {
                    iCase editcase = owner.GetCase(item);
                    if (editcase != null)
                    {
                        Edit_Case(bDistributors, "", "EDIT", editcase);
                    }


                }
            }

        }
        /// <summary>
        /// Popup edit screen of Project.Distributor properties
        /// </summary>
        /// <param name="input"></param>
        public void Edit_Case(bool bDistributor, string input, string mode, iCase aCaseToEdit = null)
        {

            if (MDProject == null) return;
            if (string.IsNullOrWhiteSpace(mode)) return;
            mode = mode.Trim().ToUpper();

            uopCaseOwners owners = bDistributor ? MDProject.Distributors : MDProject.ChimneyTrays;
            iCaseOwner owner = owners.SelectedMember;
            bool editowner = false;
            bool refreshtree = false;
            string ownertypename = owners.OwnerType.GetDescription();
            List<string> descriptions;

            List<iCase> cases = owner.CaseList;
            iCase curcase = (iCase)owner.Cases.SelectedMember;

            if (aCaseToEdit != null)
            {
                curcase = aCaseToEdit;
                owner = owners.Item(curcase.OwnerIndex);
            }

            bool reloadcombos = false;
            int selectedownerindex = owners.SelectedIndex;
            int selectedcaseindex = curcase != null ? curcase.Index : 1;
            string description = curcase != null ? curcase.Description : "";
            mzQuestion question2 = null;
            mzQuestion question1 = null;

            switch (mode)
            {
                case "ADD":
                case "EDIT":
                    {


                        if (mode == "ADD")
                        {
                            descriptions = new List<string> { "" };
                            descriptions.AddRange(owners.CaseDescriptions);
                            mzQuestions questions = new();
                            description = $"Case {owners.MaxCaseCount + 1}";
                            descriptions.Add("");

                            question1 = questions.AddSingleSelect($"Select {ownertypename} Case To Copy", descriptions);
                            question2 = questions.AddString($"Enter a {ownertypename} Case Description", description, true);
                            question2.UnacceptableAnswers = descriptions;
                            question2.UnacceptableAnswerMessage = $"Names for {ownertypename} Cases must be unique. ";
                            bool canc = !questions.PromptForAnswers($"Add {ownertypename} Case", aSaveButtonText: "Add");
                            if (canc) return;

                            curcase = null;
                            description = question2.AnswerS;
                            if (!string.IsNullOrWhiteSpace(question1.AnswerS))
                            {
                                uopPart casepart = owner.Cases.Find(x => string.Compare(x.Description, question1.AnswerS, true) == 0);
                                if (casepart != null)
                                {
                                    curcase = (iCase)casepart;
                                    curcase = curcase.Clone();
                                    curcase.Description = description;
                                    casepart.SubPart((uopPart)owner);
                                }
                            }

                            curcase ??= bDistributor ? new mdDistributorCase() { Description = description } : new mdChimneyTrayCase() { Description = description };
                            if (uopUtils.RunningInIDE)
                            {
                                curcase.MinimumOperatingRange = 50;
                                curcase.MaximumOperatingRange = 100;
                            }
                        }
                        if (curcase == null) return;

                        ParameterOverride[] inputParams = [
                            new ParameterOverride("parentVM", this) ,
                            new ParameterOverride("owner", owner),
                            new ParameterOverride("aCase", curcase),
                            new ParameterOverride("mode", mode),
                            new ParameterOverride("fieldname", input) ,
                            new ParameterOverride("title", $"Edit {ownertypename} Case Properties" ) ,
                            new ParameterOverride("headertext",  $"MD {ownertypename} Case Data Input" ) ,
                            new ParameterOverride("statustext", $"MD Project({MDProject.Name}).{ownertypename}s({owner.Description}).Case({curcase.Description})" ) ,

                        ];
                        Edit_Case_ViewModel VM = ApplicationModule.Instance.Resolve<Edit_Case_ViewModel>(inputParams);
                        bool? result = DialogService.ShowDialog<Edit_Case_View>(this, VM);
                        if (result.HasValue && VM.Mode == "ADD")
                        {
                            result = true;
                        }

                        if (result.HasValue && result == true)
                        {
                            uopProperties changes = VM.Mode == "EDIT" ? VM.GetEditedProperties() : VM.EditProps;
                            if (changes.Count <= 0) return;
                            refreshtree = curcase.SetCurrentProperties(changes);
                            reloadcombos = true;
                            //caseindex = curcase.Index;

                            if (VM.Mode == "ADD")
                            {

                                if (owners.AddCase(curcase))
                                {

                                    reloadcombos = true;
                                    refreshtree = true;
                                    selectedcaseindex = owners.MaxCaseCount;
                                }
                            }

                        }

                        break;
                    }
                case "DELETE":
                    {
                        int casecount = owners.MaxCaseCount;

                        if (casecount <= 0) return;
                        if (casecount == 1)
                        {
                            DialogService.ShowMessageBox(this, $"Case '{curcase}' Cannot Be Deleted", $"Minumum Case Count Is 1", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        description = curcase != null ? curcase.Description : "";
                        descriptions = owners.CaseDescriptions;
                        mzQuestions questions = new();
                        question1 = questions.AddMultiSelect($"Select {ownertypename}s Cases To Delete", descriptions, description, aChoiceDelimiter: "~");
                        question1.MaxChoiceCount = descriptions.Count - 1;
                        bool canceled = !questions.PromptForAnswers($"Select  Project {ownertypename}s Cases To Delete", aSaveButtonText: "Delete");
                        if (canceled) return;
                        descriptions = question1.AnswersList();
                        if (owners.RemoveCases(descriptions))
                        {
                            refreshtree = true;
                            reloadcombos = true;
                            selectedcaseindex = selectedcaseindex > 1 ? selectedcaseindex - 1 : owners.MaxCaseCount;
                        }

                        break;
                    }

                case "COPY":
                    {
                        if (owners.Count <= 0) return;


                        descriptions = owners.CaseDescriptions;
                        if (descriptions.Count <= 0) return;
                        mzQuestions questions = new();
                        question1 = questions.AddSingleSelect($"Select {ownertypename} Case To Copy", descriptions, curcase.Description);
                        question2 = questions.AddString("Enter New Case Description", $"Case {owners.MaxCaseCount + 1}", true, 25);
                        question2.UnacceptableAnswers = descriptions;
                        question2.UnacceptableAnswerMessage = $"Names for {ownertypename} Cases must be unique. ";
                        bool canceled = !questions.PromptForAnswers($"Copy Project {ownertypename} Case", aSaveButtonText: "Copy");
                        if (canceled) return;
                        description = question1.AnswerS;
                        string newdescription = question2.AnswerS;
                        int idx = descriptions.FindIndex(x => string.Compare(x, description, true) == 0) + 1;

                        if (!owners.CloneCase(idx, newdescription)) return;

                        selectedcaseindex = owners.MaxCaseCount;
                        reloadcombos = true;
                        refreshtree = true;

                        break;
                    }

                case "SORT":
                    {
                        if (owners.Count <= 0) return;
                        descriptions = owners.CaseDescriptions;
                        if (descriptions.Count <= 1) return;

                        List<string> outputlist;
                        string title = $"Sort {ownertypename}s Cases";

                        ParameterOverride[] inputParams = [new ParameterOverride("aListToSort", descriptions), new ParameterOverride("aTitle", title)];
                        SortListViewModel VM = ApplicationModule.Instance.Resolve<SortListViewModel>(inputParams);
                        bool? result = DialogService.ShowDialog<SortListView>(this, VM);

                        if (result.HasValue && result == true)
                        {

                            outputlist = VM.OutputList;
                            foreach (uopPart item in owners)
                            {
                                List<uopPart> newcases = new();
                                owner = (iCaseOwner)item;
                                foreach (string descr in outputlist)
                                {
                                    uopPart casepart = owner.Cases.Find(x => string.Compare(x.Description, descr, true) == 0);
                                    if (curcase != null) newcases.Add(casepart);
                                }
                                owner.Cases.Populate(newcases);
                            }
                            refreshtree = true;
                        }
                        selectedcaseindex = owner.Cases.FindIndex(x => string.Compare(x.Description, description, true) == 0) + 1;
                        reloadcombos = true;
                        break;
                    }

                case "RENAME":
                    {
                        if (curcase == null) return;
                        descriptions = owners.CaseDescriptions;
                        description = curcase.Description;
                        descriptions.Remove(description);
                        mzQuestions questions = new();
                        question1 = questions.AddString("Enter New Case Description", description, true, 25);
                        question1.UnacceptableAnswers = descriptions;
                        question1.UnacceptableAnswerMessage = $"Names for {ownertypename} Cases must be unique. ";
                        bool canceled = !questions.PromptForAnswers($"Rename {ownertypename} Case '{description}'", aSaveButtonText: "Rename");
                        description = question1.AnswerS.Trim();
                        if (canceled || string.IsNullOrWhiteSpace(description)) return;
                        if (string.Compare(curcase.Description, description, false) == 0) return;
                        refreshtree = owners.RenameCase(curcase.Description, description);
                        reloadcombos = true;
                        break;
                    }
            }

            if (refreshtree)
            {
                MDProject.HasChanged = true;
                Show_CaseOwners(bDistributor, !reloadcombos);
                Message_Refresh refresh = new(true, true, false, true, bCloseDocuments: false);
                refresh.Publish();
                if (reloadcombos)
                    ReloadCaseAndCaseOwnerCommbos(bDistributor, selectedownerindex, selectedcaseindex);

                if (editowner) Edit_CaseOwner(bDistributor, "", "EDIT");

            }

        }

        /// <summary>
        /// Edit Spout Area Distribution
        /// </summary>
        /// <param name="param"></param>
        public void Edit_SpoutAreaDistribution()
        {
            if (MDProject == null) return;
            if (MDAssy == null) return;
            ParameterOverride[] inputParams = [new ParameterOverride("project", MDProject)];
            Edit_SpoutAreas_ViewModel VM;
            //VM = ApplicationModule.Instance.Resolve<Edit_SpoutAreas_ViewModel>(inputParams);
            //bool? result = DialogService.ShowDialog<Edit_SpoutAreas_View>(this, VM);
            //RespondToEdits(VM, result, uppPartTypes.SpoutAreas);

            Edit_SpoutAreas_View spoutAreaPropertiesView = new();
            // spoutAreaPropertiesView.DataContext = VM;
            VM = spoutAreaPropertiesView.DataContext as Edit_SpoutAreas_ViewModel;
            //bool rCanceled = false;
            //bool rEditsMade = false;
            //uopMatrix rGroupFlags;
            //uopMatrix rIdealFlags;
            VM.Initialize(this, MDRange);
            RespondToEdits(VM, !VM.Canceled, uppPartTypes.SpoutAreas);


            //if (!rCanceled)
            //{
            //    if (rEditsMade)
            //    {
            //        MDAssy.SaveSpoutAreaFlags(rGroupFlags, rIdealFlags);
            //        RefreshAll();

            //    }
            //}
        }

        public void Edit_StartupSpouts(string input)
        {
            if (MDRange == null) return;
            ParameterOverride[] inputParams = [new ParameterOverride("project", MDProject)];

            Edit_Startups_ViewModel VM = ApplicationModule.Instance.Resolve<Edit_Startups_ViewModel>(inputParams);
            bool? result = DialogService.ShowDialog<Edit_Startups_View>(this, VM);
            RespondToEdits(VM, result, uppPartTypes.StartupSpouts);



        }


        private void Edit_Slotting()
        {
            if (MDAssy == null) return;
            ParameterOverride[] inputParams = [new ParameterOverride("aProject", MDProject), new ParameterOverride("aRange", MDRange), new ParameterOverride("aParentVM", this), new ParameterOverride("aDisplayUnits", DisplayUnits)];
            Edit_ECMDSlotting_ViewModel VM = ApplicationModule.Instance.Resolve<Edit_ECMDSlotting_ViewModel>(inputParams);

            bool? result = DialogService.ShowDialog<Edit_ECMDSlotting_View>(this, VM);
            RespondToEdits(VM, result, uppPartTypes.FlowSlot);


        }

        /// <summary>
        /// Edit Constrains
        /// </summary>
        private void Edit_Constraints()
        {
            if (MDProject == null) return;
            if (MDAssy == null) return;
            ParameterOverride[] inputParams = [new ParameterOverride("project", MDProject)];

            Edit_MDConstraints_ViewModel VM = ApplicationModule.Instance.Resolve<Edit_MDConstraints_ViewModel>(inputParams);
            bool? result = DialogService.ShowDialog<Edit_MDConstraints_View>(this, VM);
            RespondToEdits(VM, result, uppPartTypes.Constraints);

        }

        private void Edit_Splicing()
        {
            if (MDProject == null || MDAssy == null || ProjectType != uppProjectTypes.MDDraw) return;
            try
            {
                DialogService.ShowDialog<Edit_MDDeckSplices_View>(this, ApplicationModule.Instance.Resolve<Edit_MDDeckSplices_ViewModel>([new ParameterOverride("project", MDProject), new ParameterOverride("parentVM", this)]));
            }
            catch (Exception e)
            {
                MessageBox.Show($"An Error Occured: {e.Message}", $"{System.Reflection.MethodBase.GetCurrentMethod()}", button: MessageBoxButton.OK, icon: MessageBoxImage.Error);
            }


        }

        public void Edit_SelectedSpoutGroup()
        {
            mdTrayAssembly assy = MDAssy;
            if (assy == null) return;
            mdSpoutGroup sg = assy.SpoutGroups.SelectedMember;
            if (sg == null) return;
            assy.Downcomers.SelectedIndex = sg.DowncomerIndex;
            ParameterOverride[] inputParams = [new ParameterOverride("project", MDProject), new ParameterOverride("spoutgroupindex", sg.Index)];
            Edit_SpoutGroup_ViewModel VM = ApplicationModule.Instance.Resolve<Edit_SpoutGroup_ViewModel>(inputParams);
            bool? result = DialogService.ShowDialog<Edit_SpoutGroup_View>(this, VM);
            RespondToEdits(VM, result, uppPartTypes.SpoutGroup);
        }

        private void OpenBatchProcessingWindow()
        {
            mdProject project = MDProject;
            if (project == null) return;



            List<uopDocDrawing> allDrawings = new();
            uopDocuments dwgs = project.Drawings();

            mzQuestions questions = new();
            mzQuestion q1 = questions.AddSingleSelect("Select Drawing Category To List", dwgs.Categories(true, false, bReturnUpperCase: true), "MANUFACTURING");
            bool canc = !questions.PromptForAnswers("Select Drawing Type", aSaveButtonText: "Select");
            if (canc) return;
            if (string.IsNullOrWhiteSpace(q1.AnswerS)) return;

            List<uopDocDrawing> projectdwgs = project.Drawings().OfType<uopDocDrawing>().ToList();
            List<uopDocDrawing> subset = projectdwgs.FindAll(x => string.Compare(x.Category, q1.AnswerS, true) == 0);
            List<string> subcats = dwgs.SubCategories(true, true, subset.OfType<uopDocument>().ToList());
            foreach (string subcat in subcats)
            {
                allDrawings.AddRange(subset.FindAll(x => string.Compare(x.SubCategory, subcat, true) == 0));
            }



            DrawingsBatchProcessViewModel viewModel = new(allDrawings, q1.AnswerS, project, project.ProjectFolder);

            DrawingsBatchProcessView view = new();
            viewModel.CloseForm = () =>
            {
                view.Close();
            };
            view.DataContext = viewModel;

            view.ShowDialog();
        }

        public override void ReleaseReferences()
        {
            base.ReleaseReferences();
            _SpoutRegions = null;
            _DowncomerRegions = null;
        }


        void IProjectViewModel.CloseAllDocuments()
        {
            DrawingTab_CloseAll();
        }

        bool IProjectViewModel.UpdateTrayList(string aSelectedGUID) => this.UpdateTrayList(aSelectedGUID);



        DXFViewer IProjectViewModel.DXFViewer { get => Viewer; set => Viewer = value; }

        string IProjectViewModel.WarningColor { get => WarningButtonColor; set => WarningButtonColor = value; }


        #endregion Functions

    }

}
