using MvvmDialogs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UOP.DXFGraphics;
using UOP.DXFGraphics.Utilities;
using UOP.DXFGraphicsControl;
using UOP.WinTray.Projects;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Parts;

using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;
using UOP.WinTray.UI.BusinessLogic;
using UOP.WinTray.UI.Commands;
using UOP.WinTray.UI.Messages;
using UOP.WinTray.UI.Views;
using WW.Math;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace UOP.WinTray.UI.ViewModels
{
    public class Edit_Startups_ViewModel : MDProjectViewModelBase, IModalDialogViewModel, INotifyPropertyChanged
    {
        #region Fields

        private readonly Color _HighlightColor = Color.FromArgb(213, 198, 0);

        #endregion Fields

        #region Constructors
        //public Edit_Startups_ViewModel(IStorageHelper<mdProject> storageHelper,
        //                                        IPropertyHelper propertyHelper,
        //                                        IEventAggregator eventAggregator,
        //                                        IProjectMDMain projectMDMain, 
        //                                        IDialogService dialogService,
        //                                        IContextMenu contextMenu) : base(propertyHelper, eventAggregator, storageHelper, dialogService, projectMDMain: projectMDMain)
        public Edit_Startups_ViewModel(IDialogService dialogService, mdProject project) : base(dialogService: dialogService)
        {

            try
            {
                BusyMessage = "Loading Details..";
               ShowProgress = true;
                Loading = true;

                MDProject = project;
                DisplayUnits = project.DisplayUnits;
                EditStartupTitle = $"Edit Start Spouts - { MDAssy.TrayName(true)}";
                _AllowPositioning = ProjectType == uppProjectTypes.MDDraw; NotifyPropertyChanged("AllowPositioning");

                if (MDAssy != null) Config = MDAssy.StartupConfiguration;
                Loading = false;

                Initialize();

                InitializeStartupSpout();

            }
            finally
            {
                ShowProgress = false;
                Loading = false;

                FinishedWaitingForClick = true;
                Execute_RefreshDisplay(true, true);
            }
        }
        #endregion Constructors

        #region Properties
        /// <summary>
        /// Is Cancelled.
        /// </summary>

        public bool InSpout { get; set; }

        public bool Loading { get; set; }

        public bool Dragging { get; set; }

        private mdStartupSpout SelectedStartupSpout { get; set; }

        public colDXFRectangles SURegions { get; set; }

        public string AlignMems { get; set; }

        public string AlignHandle { get; set; }

        public double AlignToY { get; set; }

        public string AlignToHandle { get; set; }

        private string _OverrideStartupLength = "";
        public string OverrideStartupLength
        {
            get => _OverrideStartupLength;
            set { _OverrideStartupLength = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private string _ToggleMenuHeader = "Toggle Spout";
        public string ToggleMenuHeader
        {
            get => _ToggleMenuHeader;
            set { _ToggleMenuHeader = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private colUOPParts _Stiffeners;
        public colUOPParts Stiffeners { get { _Stiffeners ??= colUOPParts.FromPartsList(mdPartGenerator.Stiffeners_ASSY(MDAssy, false)); return _Stiffeners; } set => _Stiffeners = value; }


        private bool _Config_None;
        public bool Config_None
        {
            get => _Config_None;
            set
            {
                _Config_None = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod());
                if (value)
                {
                    Config = uppStartupSpoutConfigurations.None;
                }
            }
        }

        private bool _ShowProgress;
        public bool ShowProgress
        {
            get => _ShowProgress;
            set
            {
                _ShowProgress = value;
                NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod());
                Visibility_Progress = _ShowProgress ? Visibility.Visible : Visibility.Collapsed;
                IsEnabled = !_ShowProgress;
                if (!value) BusyMessage = string.Empty;
            }
        }

        private bool _Config_TwoByTwo;
        public bool Config_TwoByTwo
        {
            get => _Config_TwoByTwo;
            set
            {
                _Config_TwoByTwo = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); 
                if (value)
                {
                    Config = uppStartupSpoutConfigurations.TwoByTwo;
                }
            }
        }


        private bool _Config_TwoByFour;
        public bool Config_TwoByFour
        {
            get => _Config_TwoByFour;
            set
            {
                _Config_TwoByFour = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); 
                if (value)
                {
                    Config = uppStartupSpoutConfigurations.TwoByFour;
                }
            }
        }


        private bool _Config_FourByFour;
        public bool Config_FourByFour
        {
            get => _Config_FourByFour;
            set
            {
                _Config_FourByFour = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); 
                if (value)
                {
                    Config = uppStartupSpoutConfigurations.FourByFour;
                }
            }
        }

        private List<string> PointerHandles { get; set; }

        public ZoomData CurrentView { get; set; }
        public double PointerWd { get; set; }

        private mdStartupSpouts _LastSpouts;
        public mdStartupSpouts LastSpouts { get => _LastSpouts; set { _LastSpouts = value; CanUndo = _LastSpouts != null; } }

        public mdStartupLine LimitLine { get; set; }
        public override dxfImage Image
        {
            get => base.Image;
            set
            {
                base._Image = value;
                //Viewer?.SetImage(value);
                NotifyPropertyChanged("Image");
            }
        }

        private string _SelectedSpoutLabel = "";
        public string SelectedSpoutLabel
        {
            get => _SelectedSpoutLabel;
            set
            {
                _SelectedSpoutLabel = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod());
            }
        }

        public mdStartupSpout AlignTo { get; set; }

        private mdStartupSpout _Selected;
        public mdStartupSpout Selected
        {
            get
            {
                if (_Selected == null)
                {
                    mdStartupSpouts spts = Spouts;
                    if (spts == null) { SpoutHandle = ""; _Selected = null; return null; }
                    if (spts.Count <= 0) { SpoutHandle = ""; _Selected = null; return null; }
                    if (!String.IsNullOrEmpty(SpoutHandle)) _Selected = spts.GetByHandle(SpoutHandle);
                    if (_Selected == null) _Selected = spts.SelectedMember;
                    if (_Selected == null)
                    {
                        spts.SelectedIndex = 1;
                        _Selected = spts.Item(1);
                    }

                }
                SpoutHandle = (_Selected != null) ? _Selected.Handle : "";
                if (_Selected != null) Spouts.SetSelected(_Selected);

                return _Selected;
            }
            set
            {
                _Selected = value;
                SpoutHandle = (_Selected != null) ? _Selected.Handle : "";
                if (_Selected != null)
                {
                    Spouts.SetSelected(_Selected);
                    UpdateToggle();
                }
            }

        }
        public dxfRectangle Region { get; set; }

        private mdStartupSpouts _Spouts;
        public mdStartupSpouts Spouts
        {
            get
            {
                if (_Spouts == null)
                {
                    LastSpouts = null;
                    mdTrayAssembly assy = MDAssy;
                    if (assy == null) return null;
                    _Spouts = assy.StartupSpouts.Clone();


                }
                return _Spouts;

            }
            set => _Spouts = value;
        }

        public string SpoutHandle { get; set; }



        private bool? _DialogResult;
        /// <summary>
        /// Dialogservice result
        /// </summary>
        public bool? DialogResult
        {
            get => _DialogResult;
            private set { _DialogResult = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }


        private string _IdealArea;
        public string IdealArea
        {
            get => _IdealArea;
            set { _IdealArea = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }
        private string _TotalArea;
        public string TotalArea
        {
            get => _TotalArea;
            set { _TotalArea = value; NotifyPropertyChanged("TotalArea"); }
        }
        private string _TotalSpout;
        public string TotalSpouts
        {
            get => _TotalSpout;
            set { _TotalSpout = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }
        private string _DeviationPct;
        public string Deviation
        {
            get => _DeviationPct;
            set { _DeviationPct = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }
        private string _DeviationForeColor = "Black";
        public string DeviationForeColor
        {
            get => _DeviationForeColor;
            set { _DeviationForeColor = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }
        private string _SpoutHeight;
        public string SpoutHeight
        {
            get => _SpoutHeight;
            set { _SpoutHeight = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private string _SpoutLength;
        public string SpoutLength
        {
            get => _SpoutLength;
            set { _SpoutLength = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }
        private string _SpoutArea;
        public string SpoutArea
        {
            get => _SpoutArea;
            set { _SpoutArea = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private string _MaxSite;
        public string MaxSite
        {
            get => _MaxSite;
            set { _MaxSite = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private string _MinSite;
        public string MinSite
        {
            get => _MinSite;
            set { _MinSite = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        /// <summary>
        /// Override Length.
        /// </summary>
        private double _OverrideLength = 0;
        public double OverrideLength
        {
            get => _OverrideLength;
            set { if (value < 0) value = 0; _OverrideLength = value; }
        }

        private string _YOrdinate = "";
        public string YOrdinate
        {
            get => _YOrdinate;
            set { _YOrdinate = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private string _MaxY = "";
        public string MaxY
        {
            get => _MaxY;
            set { _MaxY = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private string _MinY = "";
        public string MinY
        {
            get => _MinY;
            set { _MinY = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private bool _IsSpoutSelectionOn;
        public bool IsSpoutSelectionOn
        {
            get => _IsSpoutSelectionOn;
            set { _IsSpoutSelectionOn = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private bool _IsAllowDragEnabled = false;
        public bool IsAllowDragEnabled
        {
            get => _IsAllowDragEnabled;
            set
            {
                _IsAllowDragEnabled = value;
                NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod());
                AllowPositioning = value;


            }
        }

        private Visibility _Visibility_AllowDrag = Visibility.Collapsed;
        public Visibility Visibility_AllowDrag
        {
            get => _Visibility_AllowDrag;
            set { _Visibility_AllowDrag = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private Visibility _Visibility_Positioning = Visibility.Collapsed;
        public Visibility Visibility_Positioning
        {
            get => _Visibility_Positioning;

        }

        private bool _AllowConfiguration;
        public bool AllowConfiguration { get => _AllowConfiguration; set { _AllowConfiguration = value; NotifyPropertyChanged("AllowConfiguration"); _AllowPositioning = !value; NotifyPropertyChanged("AllowPositioning"); Visibility_Generation = (value == true) ? Visibility.Collapsed : Visibility.Visible; if (!Loading) Execute_RefreshDisplay(bRedraw: true); } }


        private bool _AllowPositioning;
        public bool AllowPositioning
        {
            get => _AllowPositioning;
            set
            {
                _AllowPositioning = value;
                NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod());
                _AllowConfiguration = !value; NotifyPropertyChanged("AllowConfiguration");
                Visibility_Generation = (value == true) ? Visibility.Collapsed : Visibility.Visible;

                //if (!Loading) RefreshDisplay(bRedraw: true);
            }
        }


        private bool _Aligning;
        public bool Aligning
        {
            get => _Aligning; set
            {
                if (_Aligning == value || Dragging || !AllowPositioning) return;

                _Aligning = value;
                if (_Aligning)
                {
                    AlignToHandle = "";
                    AlignMems = "";
                    AlignHandle = "";

                    if (Selected != null)
                    {
                        if (!Selected.Suppressed)
                        {
                            MousePoint = Selected.CenterDXF;

                        }
                        else
                        {
                            _Aligning = false;
                            return;
                        }
                    }
                    else
                    {
                        _Aligning = false;
                        return;
                    }
                    //TODO
                    //  MsgBox("Align Mode Activated: Select Spouts To Align To Spout " + Selected.Handle + "." + vbLf + vbLf + "Press Save or Enter when selection is complete" + vbLf + vbLf + "Press Escape or Execute_Cancel to cancel Align mode", vbInformation, "Align Mode");
                }
                else
                {
                    if (AlignToHandle != "")
                    {
                        int i = 0;

                        List<string> aAlignCol = null;

                        mdStartupSpout aSpt = null;

                        bool bCng = false;

                        Image.Screen.Entities.Delete(AlignToHandle);
                        if (AlignMems != "")
                        {
                            aAlignCol = mzUtils.StringsFromDelimitedList(AlignMems, ":", false);
                            for (i = 1; i < aAlignCol.Count; i++)
                            {
                                aSpt = Spouts.GetByHandle(aAlignCol[i]);
                                if (aSpt != null)
                                {
                                    if (aSpt.Y != AlignToY)
                                    {
                                        bCng = true;
                                    }
                                    aSpt.Y = AlignToY;
                                }
                            }
                            if (bCng)
                            {
                                Spouts.SetObscured(MDAssy, Stiffeners);
                                Spouts.UpdateYLimits(MDAssy, MDAssy.Downcomers);

                                Execute_RefreshDisplay();
                            }

                        }

                        Image.Display.Redraw();


                    }


                }

            }
        }

        private string _SpouLengthTxtColor = "Black";
        public string SpouLengthTxtColor
        {
            get { return _SpouLengthTxtColor; }
            set
            {
                _SpouLengthTxtColor = value;
                NotifyPropertyChanged("SpouLengthTxtColor");
            }
        }

        private string _TxtColor;
        public string TxtColor
        {
            get { return _TxtColor; }
            set
            {
                _TxtColor = value;
                NotifyPropertyChanged("TxtColor");
            }
        }

        public override DXFViewer Viewer
        {
            get => base.Viewer;
            set
            {
                if (value == null)
                {
                    CurrentView = null;
                    PersistentEntities = null;
                    Blocks = null;
                }

                if (value == null && base.Viewer != null)
                {
                    base.Viewer.OnViewerMouseDown -= Viewer_OnViewerMouseDown;
                    base.Viewer.MouseMove -= Viewer_MouseMove;
                }
                base.Viewer = value;
                if (base.Viewer != null)
                {
                    value.OnViewerMouseDown += Viewer_OnViewerMouseDown;
                    value.MouseMove += Viewer_MouseMove;

                }

            }
        }



        public dxfRectangle MaxViewRectangle { get; set; }


        private string _EditStartupTitle = "Edit Startup Spouts";
        public string EditStartupTitle { get => _EditStartupTitle; set { _EditStartupTitle = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); } }


        private bool WaitingForClick { get; set; }
        private string FinishWaitingForClick { get; set; }

        private bool FinishedWaitingForClick { get; set; }

        private readonly List<ulong> CursorHandles = new();

        private uppStartupSpoutConfigurations _Config;
        public uppStartupSpoutConfigurations Config
        {
            get => _Config;
            set
            {
                _Config = value;
                _Config_FourByFour = value == uppStartupSpoutConfigurations.FourByFour;
                _Config_None = value == uppStartupSpoutConfigurations.None;
                _Config_TwoByTwo = value == uppStartupSpoutConfigurations.TwoByTwo;
                _Config_TwoByFour = value == uppStartupSpoutConfigurations.TwoByFour;

                NotifyPropertyChanged("Config");
                NotifyPropertyChanged("Config_None");
                NotifyPropertyChanged("Config_FourByFour");
                NotifyPropertyChanged("Config_TwoByTwo");
                NotifyPropertyChanged("Config_TwoByFour");

            }

        }
        /// <summary>
        /// Validate can undo data
        /// </summary>
        /// 
        private bool _CanUndo = false;
        public bool CanUndo { get => _CanUndo; set { _CanUndo = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); } }

        private Visibility _Visibility_Generation;
        public Visibility Visibility_Generation
        {
            get => _Visibility_Generation;
            set
            {
                _Visibility_Generation = value;
                NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod());
                _Visibility_Positioning = value == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                NotifyPropertyChanged("Visibility_Positioning");
            }
        }
        private dxfEntities PersistentEntities { get; set; }

        private List<dxfBlock> Blocks { get; set; }
        #endregion Properties


        #region Overrides
        public override void ToggleUnits()
        {
            base.ToggleUnits();
            Execute_RefreshDisplay(bRedraw: false);
        }

        public override bool IsEnglishSelected
        {
            get => base.IsEnglishSelected;
            set { base.IsEnglishSelected = value; Execute_RefreshDisplay(); }
        }

        #endregion Overrides

        #region Methods

        private double ValidateY(double yval)
        {
            double ret = yval;
            if (Selected == null) return yval;
            if (yval > Selected.MaxY) ret = Selected.MaxY;
            if (yval < Selected.MinY) ret = Selected.MinY;
            return ret;
        }

        public void Viewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (Viewer == null)
            {
                return;
            }

            var position = e.GetPosition((System.Windows.IInputElement)sender);


            double xval = 0;
            double yval = 0;
            double multi;

            multi = DisplayUnits == uppUnitFamilies.English ? 1 : 25.4;

            (xval, yval) = Viewer.DeviceToWorld(position.X, position.Y);

            if (WaitingForClick)
            {
                switch (FinishWaitingForClick)
                {
                    case "DRAG":
                        {
                            ulong hnd = Viewer.overlayHandler.DrawHorzLine(new dxfVector(0, ValidateY(yval), 0), MDAssy.GetMDRange().ShellID * 2, dxxColors.Red, 20, "CENTER", CursorHandles.Count > 0 ? CursorHandles[0] : 0);
                            if (!CursorHandles.Contains(hnd))
                                CursorHandles.Add(hnd);
                            hnd = 0;
                            (xval, yval) = Viewer.DeviceToWorld(0, position.Y);

                            hnd = Viewer.overlayHandler.DrawText(string.Format("{0:0.000}", ValidateY(yval) * multi), new dxfVector(xval + 2, ValidateY(yval) - 2, 0), 2, CursorHandles.Count > 1 ? CursorHandles[1] : 0);
                            if (!CursorHandles.Contains(hnd))
                                CursorHandles.Add(hnd);
                            break;
                        }
                }
            }
        }

        public void LeftMouseButtonEventProcessor(System.Windows.Forms.MouseButtons mouseButtons, int x, int y)
        {
            if (Viewer == null)
            {
                return;
            }

            if (mouseButtons == System.Windows.Forms.MouseButtons.Left)
            {
                if (MousePoint == null) MousePoint = new dxfVector();


                //MousePoint.SetCoordinates( x, y );
                (double px, double py) = Viewer.DeviceToWorld((double)x, (double)y);
                MousePoint.SetCoordinates(px, py);

                if (WaitingForClick)
                {
                    Viewer.overlayHandler.RemoveByHandle(CursorHandles);
                    switch (FinishWaitingForClick)
                    {
                        case "DRAG":
                            WaitingForClick = false;
                            Command_DragY.Execute(null);
                            break;
                        case "ALIGN":
                            Command_AlignY.Execute(null);
                            break;
                    }
                    return;
                }
            }
        }


        internal void Window_Closing(object sender, CancelEventArgs e)
        {
            if (WaitingForClick)
            {
                ResetWaitingForPoint();
                e.Cancel = true;
                return;
            }
            Image = null;
            Viewer = null;

        }


        /// <summary>
        /// method for showing spout
        /// </summary>
        /// <param name="aMultiplier"></param>
        public void Execute_SpoutShow(mdStartupSpout aSU = null)
        {

            try
            {

                Loading = true;
                mdStartupSpout curspout = aSU ?? Selected;
                if (aSU != null) Selected = aSU;
                if (curspout == null)
                {
                    SelectedSpoutLabel = "Selected Spout";
                    YOrdinate = "";
                    MinY = "";
                    MaxY = "";
                    IsSpoutSelectionOn = false;
                    LimitLine = null;
                }
                else
                {
                    SelectedSpoutLabel = $"Selected Spout [{ curspout.Handle }]";
                    uopUnit lunit = uopUnits.GetUnit(uppUnitTypes.SmallLength);
                    YOrdinate = lunit.UnitValueString(curspout.Y, DisplayUnits, bAddLabel: true);
                    MinY = lunit.UnitValueString(curspout.MinY, DisplayUnits, bAddLabel: true);
                    MaxY = lunit.UnitValueString(curspout.MaxY, DisplayUnits, bAddLabel: true);
                    IsSpoutSelectionOn = !curspout.Suppressed;
                    LimitLine = curspout.LimitLine;
                }

            }
            catch { }
            finally { Loading = false; }





        }


        private void InitializeStartupSpout(bool bResetSpouts = false, mdStartupSpouts aSpouts = null)
        {
            if (aSpouts != null)
            {
                bResetSpouts = false;
                Spouts = aSpouts.Clone();

            }

            if (bResetSpouts) { Spouts = MDAssy.StartupSpouts.Clone(); Config = MDAssy.StartupConfiguration; LastSpouts = null; }

            Spouts.UpdateYLimits(MDAssy, MDAssy.Downcomers);
            OverrideLength = MDAssy.OverrideStartupLength;
            SelectedStartupSpout = Spouts.GetSpout(dxxPointFilters.GetLeftTop);
            if (SelectedStartupSpout != null)
            {

                IsSpoutSelectionOn = !SelectedStartupSpout.Suppressed;

            }

            if (Viewer != null) Execute_RefreshDisplay(bRedraw: true);
        }


        public void UpdateToggle()
        {

            if (Selected != null)
            {
                if (Selected.Suppressed)
                {
                    ToggleMenuHeader = "Toggle Spout: On";
                }
                else
                {
                    ToggleMenuHeader = "Toggle Spout: Off";
                }
            }
            else
            {
                ToggleMenuHeader = "Toggle Spout";
            }
        }

        /// <summary>
        /// Intialize Toggle Menu Default Values
        /// </summary>
        private void IntializeToggleMenuDefaultValues(System.Windows.Forms.ContextMenu menu)
        {
            if (menu != null)
            {
                var toggleSpoutStripMenuItem = menu.MenuItems[0];
                var dragStripMenuItem = menu.MenuItems[1];
                var alignStripMenuItem = menu.MenuItems[3];
                var setYStripMenuItem = menu.MenuItems[2];

                if (Spouts == null)
                {
                    dragStripMenuItem.Enabled = false;
                    toggleSpoutStripMenuItem.Enabled = false;
                    alignStripMenuItem.Enabled = false;
                    setYStripMenuItem.Enabled = false;
                    return;

                }

                if (!AllowPositioning)
                {
                    dragStripMenuItem.Enabled = false;
                    alignStripMenuItem.Enabled = false;

                }
                else
                {
                    if (Aligning)
                    {
                        dragStripMenuItem.Enabled = false;
                        toggleSpoutStripMenuItem.Enabled = false;
                        alignStripMenuItem.Enabled = false;
                        setYStripMenuItem.Enabled = false;
                    }
                    else
                    {
                        if (Selected != null)
                        {
                            dragStripMenuItem.Enabled = !Dragging && !Selected.Suppressed;
                            setYStripMenuItem.Enabled = !Dragging && !Selected.Suppressed;
                            alignStripMenuItem.Enabled = !Dragging && !Selected.Suppressed;

                            if (Selected.Suppressed)
                            {
                                toggleSpoutStripMenuItem.Text = "Toggle Spout On";
                            }
                            else
                            {
                                toggleSpoutStripMenuItem.Text = "Toggle Spout Off";
                            }
                        }
                        else
                        {
                            dragStripMenuItem.Enabled = false;
                            toggleSpoutStripMenuItem.Text = "Toggle Spout";
                            toggleSpoutStripMenuItem.Enabled = false;
                            alignStripMenuItem.Enabled = false;
                            setYStripMenuItem.Enabled = false;
                        }
                        if (!IsSpoutSelectionOn)
                        {
                            toggleSpoutStripMenuItem.Enabled = false;
                        }

                    }
                }
                setYStripMenuItem.Enabled = dragStripMenuItem.Enabled;
                menu.MenuItems.Clear();
                menu.MenuItems.Add(toggleSpoutStripMenuItem);
                menu.MenuItems.Add(dragStripMenuItem);
                menu.MenuItems.Add(setYStripMenuItem);
                menu.MenuItems.Add(alignStripMenuItem);
            }
        }


        /// <summary>
        /// To ddo Code Conversion need to do.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToggleSpoutStripMenuItem_Click(object sender, EventArgs e)
        {
            SpoutToggle();
        }

        public void Update_Viewer_Image(dxfImage aImage)
        {

            try
            {
                Edit_Startups_View myusc = (Edit_Startups_View)MyWindow();
                myusc.UdateViewerImage(aImage);


                //if (Viewer == null) return;
                //if (aImage != null)
                //{

                //    Viewer.SetImage(aImage);
                //    if (aImage.Display.HasLimits)
                //    { Viewer.ZoomWindow(aImage.Display.LimitsRectangle); }
                //    else
                //    { Viewer.ZoomExtents(); }
                //    Viewer.RefreshDisplay();
                //}
                //else { Viewer.Clear(); }

            }
            catch { }


        }


        public void SpoutToggle()
        {
            mdStartupSpout aSpout = Spouts.GetByHandle(SpoutHandle);
            if (aSpout == null) return;
            LastSpouts = Spouts.Clone();
            aSpout.Suppressed = !aSpout.Suppressed;
            Execute_ReGenerateSpouts(false);

        }


        /// <summary>
        /// Method for reseting data
        /// </summary>
        private void Execute_Reset()
        {
            if (DialogService.ShowMessageBox(this, "Reset To The Currently Saved Assembly Startup Spout Configuration?", "Reset ?", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                   ShowProgress = true;
                    InitializeStartupSpout(true);


                }
                finally
                {
                   ShowProgress = false;
                }
            }
        }

        /// <summary>
        /// Method for undoing data
        /// </summary>
        private void Execute_Undo()
        {
            if (LastSpouts != null)
            {
                if (DialogService.ShowMessageBox(this, "Reset To Last Spout Configuration?", "Undo ?", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    try
                    {
                       ShowProgress = true;
                        InitializeStartupSpout(false, LastSpouts);
                        LastSpouts = null;

                    }
                    finally
                    {
                       ShowProgress = false;
                    }
                }
            }
        }

        /// <summary>
        /// method for saving data
        /// </summary>
        private void Execute_Save()
        {
            if (!string.IsNullOrEmpty(ErrorMessage)) return;


            try
            {


                if (WaitingForClick)
                {
                    ResetWaitingForPoint();
                    return;
                }


                BusyMessage = "Saving Current Startup Spout Configuration..";
               ShowProgress = true;

                Edited = !Spouts.IsEqual(MDAssy.StartupSpouts) || MDAssy.OverrideStartupLength != OverrideLength;


                if (Edited)
                {
                    RefreshMessage = new Message_Refresh(bSuppressPropertyLists: false, bSuppressImage: false, bSuppressTree: false, aPartTypeList: new List<uppPartTypes> { uppPartTypes.StartupSpouts }) { SuppressDocumentClosure = false };
                    

                    MDAssy.PropValSet("OverrideStartupLength", OverrideLength, bSuppressEvnts: true);
                    MDAssy.PropValSet("StartupConfiguration", Config, bSuppressEvnts: true);

                    MDAssy.StartupSpouts = new mdStartupSpouts( Spouts);
                    //    MDProject.HasChanged = true;

                    //    MDAssy.PropValSet("OverrideStartupLength", OverrideLength, bSuppressEvnts: true);
                    //    MDAssy.PropValSet("StartupConfiguration", Config, bSuppressEvnts: true);

                    //    MDAssy.StartupSpouts = Spouts;

                }
            }
            finally
            {
               ShowProgress = false;
                BusyMessage = "";
                Canceled = !Edited;
                DialogResult = Edited;
                //WinTrayMainViewModel mainVM = WinTrayMainViewModel.WinTrayMainViewModelObj;
                //if(mainVM != null && Edited)
                //{
                //    Messages.Message_Refresh msg = new Messages.Message_Refresh(bSuppressPropertyLists: false, bSuppressImage: false, bSuppressTree: false, aPartTypeList: new List<uppPartTypes> { uppPartTypes.StartupSpouts }) { CloseAllDocuments = true } ;
                //    mainVM.RefreshDisplay(msg);
                //}

            }
        }

        /// <summary>
        /// Executed to refresh all or prt of the cureent display
        /// </summary>
        /// <param name="bRedraw"></param>
        /// <param name="bRefreshData"></param>
        /// <param name="bRefreshSelected"></param>
        /// <param name="aSpout"></param>
        public async void Execute_RefreshDisplay(bool bRedraw = false, bool bRefreshData = true, bool bRefreshSelected = true, mdStartupSpout aSpout = null)
        {

            if (!bRedraw && !bRefreshData && !bRefreshSelected) return;
            bool isBad = false;
           
            try
            {

               ShowProgress = true;
                BusyMessage = "Refreshing Display ...";
                Region = null;

                await Task.Run(() =>
                {
                    if (bRefreshData)
                    {



                        uppUnitFamilies dspunits = DisplayUnits;


                        mdStartupSpout basespout = (Spouts.Count > 0) ? Spouts.Item(1) : null;

                        double spoutlen = Spouts.Length;
                        double spoutheight = Spouts.Height;
                        double spoutrea = 0;
                        double spoutlenoride = OverrideLength;

                        if (basespout != null)
                        {
                            spoutlen = basespout.Length;
                            spoutheight = basespout.Height;
                            spoutrea = basespout.Area;
                        }

                        uopUnit ulinear = uopUnits.GetUnit(uppUnitTypes.SmallLength);
                        uopUnit uarea = uopUnits.GetUnit(uppUnitTypes.SmallArea);

                        Spouts.Descriptor(DisplayUnits, rDeviationPct: out double Pct, rIsDeviant: out isBad);


                        IdealArea = uarea.UnitValueString(MDAssy.IdealStartupArea, dspunits, bAddLabel: true);
                        TotalArea = uarea.UnitValueString(Spouts.TotalArea, dspunits, bAddLabel: true);
                        TotalSpouts = Spouts.TotalCount.ToString();

                        MinSite = ulinear.UnitValueString(Spouts.MinSiteLength, dspunits, bAddLabel: true);


                        MaxSite = ulinear.UnitValueString(Spouts.MaxSiteLength, dspunits, bAddLabel: true);
                        Deviation = Pct.ToString("0.00") + " %";
                        DeviationForeColor = Math.Abs(Pct) > mdStartUps.DeviationLimit ? "Red" : "Black";
                        OverrideStartupLength = (spoutlenoride > 0) ? ulinear.UnitValueString(spoutlenoride, dspunits, bAddLabel: true) : "";

                        SpoutLength = (basespout != null) ? ulinear.UnitValueString(spoutlen, dspunits, bAddLabel: true) : "";
                        SpoutHeight = (basespout != null) ? ulinear.UnitValueString(spoutheight, dspunits, bAddLabel: true) : "";
                        SpoutArea = (basespout != null) ? uarea.UnitValueString(spoutrea, dspunits, bAddLabel: true) : "";




                    }

                    if (bRefreshSelected) Execute_SpoutShow(aSpout);
                });

              
            }
            catch { }
            finally
            {
                TxtColor = isBad ? "Red" : "Black";

                SpouLengthTxtColor = (!string.IsNullOrWhiteSpace(OverrideStartupLength)) ? "Blue" : TxtColor;
               ShowProgress = false;
                BusyMessage = "";
               ShowProgress = false;
                aSpout ??= Selected;
                if (aSpout != null && Viewer != null && SURegions != null)
                {
                    var overlay = Viewer.overlayHandler;
                    overlay?.DrawRectangle(SURegions.GetTagged(aSpout.Handle), _HighlightColor, 75);
                    overlay?.RefreshDisplay();
                }

                if (bRedraw) Execute_DrawObjects();

            }


        }

        //Mathod for Cancel functionality
        private void Execute_Cancel()
        {
            if (MDAssy != null)
            {

                if (WaitingForClick)
                {
                    ResetWaitingForPoint();
                    return;
                }

                Edited = !Spouts.IsEqual(MDAssy.StartupSpouts) || MDAssy.OverrideStartupLength != OverrideLength;
                if (Edited)
                {
                    if (DialogService.ShowMessageBox(this, "Exit Without Saving Changes?", "Loss Current Changes?", button: MessageBoxButton.YesNo, icon: MessageBoxImage.Question) != MessageBoxResult.Yes)
                    {
                        return;
                    }

                }

            }

            Canceled = true;
            Edited = false;
            DialogResult = false;
        }

        /// <summary>
        /// Initialize startups
        /// </summary>

        /// <returns></returns>
        public void Initialize()
        {



            Loading = true;
            Visibility_AllowDrag = uopUtils.RunningInIDE ? Visibility.Visible : Visibility.Collapsed;


            IsAllowDragEnabled = ProjectType == uppProjectTypes.MDDraw;

            Loading = false;

            OverrideLength = MDAssy.OverrideStartupLength;

            Stiffeners = colUOPParts.FromPartsList(mdPartGenerator.Stiffeners_ASSY(MDAssy, false));
            Spouts = MDAssy.StartupSpouts.Clone();

            Spouts.UpdateYLimits(MDAssy, MDAssy.Downcomers);
            LastSpouts = null;


            TxtColor = "Black";
            SpouLengthTxtColor = "Black";




            //TODO: Need to verify whether main screen freshing is needed using below commented code. 
            //if (!bCancelSelected)
            //{
            //    rCanceled = false;
            //    editStartupLayout = oSpouts;
            //    rEditsMade = !MDAssy.StartupSpouts.IsEqual(oSpouts, true) || rOrideLength != sOverrideStartupLength;
            //    rOrideLength = sOverrideStartupLength;

            //}

            //ReleaseReferences();


        }

    


        /// <summary>
        /// Execute_ToggleSpout functionality
        /// </summary>
        public void Execute_ToggleSpout()
        {
            if (Loading) return;

            SpoutToggle();
            UpdateToggle();
        }

        public void Execute_Drag()
        {
            if (WaitingForClick || MousePoint == null) return;

            if (!FinishedWaitingForClick)
            {
                ResetWaitingForPoint();

                SetYordinate(ValidateY(MousePoint.Y));
            }
            else
            {

                mdStartupSpout spout = Selected;
                if (spout == null) return;
                if (spout.Suppressed) return;

                WaitingForClick = true;
                FinishedWaitingForClick = false;
                FinishWaitingForClick = "DRAG";
                Viewer.Cursor = Cursors.ScrollNS;
            }
        }

        public void Execute_Align()
        {
            if (MousePoint == null) return;

            if (!FinishedWaitingForClick)
            {
                //ResetWaitingForPoint();
                if (SURegions != null || AlignTo == null)
                {
                    dxfRectangle region = SURegions.GetByNearestCenter(MousePoint);
                    mdStartupSpout aSU = Spouts.GetByHandle(region.Tag);
                    SetYordinate(ValidateY(AlignToY), aSU, true);
                    Selected = AlignTo;
                    HighlightStartUp(MDAssy, AlignTo);
                }
            }
            else if (!WaitingForClick)
            {

                AlignTo = Selected;
                if (AlignTo == null) return;
                if (AlignTo.Suppressed)
                {
                    AlignTo = null; return;
                }

                AlignToY = ValidateY(MousePoint.Y);
                WaitingForClick = true;
                FinishedWaitingForClick = false;
                FinishWaitingForClick = "ALIGN";
                Viewer.Cursor = Cursors.Hand;
            }
        }

        /// <summary>
        /// Execute_InputYOrdinate functionality
        /// </summary>
        public void Execute_InputYOrdinate()
        {
            if (Loading) return;

            mdStartupSpout spout = Selected;
            if (spout == null) return;
            if (spout.Suppressed) return;

            double aVal = spout.Y * Multiplier_Linear;
            double max = spout.MaxY * Multiplier_Linear;
            double min = spout.MinY * Multiplier_Linear;
            uopUnit lunits = uopUnits.GetUnit(uppUnitTypes.SmallLength);

            string note = $"Min: {lunits.UnitValueString(spout.MinY, DisplayUnits)} Max: {lunits.UnitValueString(spout.MaxY, DisplayUnits)}";

            bool canceled = false;
            double newVal = dxfUtils.Input_Decimal(aVal, ref canceled, "Input Spout Y Ordinate", "Enter A New Y Ordinate:", note, aAllowZeroInput: true,
                aAllowNegatives: true, aMaxLimit: max, aMinLimit: min, aMaxWhole: 5, aMaxDeci: 3, aEqualReturnCancel: false);

            if (canceled) return;
            newVal /= Multiplier_Linear;
            if (newVal == spout.Y) return;

            SetYordinate(newVal, spout);

        }

        public void SetYordinate(double newVal, mdStartupSpout aSpout = null, bool bSuppressQuery = false)
        {
            mdStartupSpout spout = aSpout ?? Selected;
            try
            {

                if (spout == null) return;
                if (spout.Suppressed) return;
                if (newVal == spout.Y) return;

                LastSpouts = Spouts.Clone();
                List<mdStartupSpout> movers = new() { spout };

                if (!bSuppressQuery)

                {
                    movers = Spouts.FindAll(x => Math.Round(x.Y,3) == Math.Round( spout.Y,3));
                    if (movers.Count > 0)
                        movers.Remove(spout);
    
                    if (movers.Count > 0)
                    {
                        string query = $"Move All Spouts at Y = {Units_Linear.UnitValueString(spout.Y, DisplayUnits)} To = {Units_Linear.UnitValueString(newVal, DisplayUnits)} ";
                        if (DialogService.ShowMessageBox(this, query, "Move Aligned Spouts", button: MessageBoxButton.YesNo, icon: MessageBoxImage.Question) != MessageBoxResult.Yes)
                        {
                            movers.Clear();
                        }
                    }
                    movers.Add(spout);
                }


                foreach (var item in movers)
                {
                    item.Y = newVal;
                }
            }
            catch { }
            finally
            {
                if (spout != null) Execute_RefreshDisplay(bRedraw: true, bRefreshData: false, bRefreshSelected: false, aSpout: spout);
            }




        }


        /// <summary>
        /// method for terminating objects
        /// </summary>
        public override void ReleaseReferences()
        {
            Viewer = null;

            base.ReleaseReferences();

            _Spouts = null;
            AlignTo = null;
            MousePoint = null;
            _Selected = null;
            Project = null;
            
        }

        public override void Activate(System.Windows.Window myWindow)
        {
            if (Activated) return;
            base.Activate(myWindow);


        }

        /// <summary>
        ///  method used for draw objects
        /// </summary>

        public async void Execute_DrawObjects()
        {
          
            string selected = SpoutHandle;
            mdTrayAssembly assy = MDAssy;
            DXFViewer viewer = Viewer;
          
            dxfImage image = Image;
            bool firstDraw = image == null || PointerHandles == null || MaxViewRectangle == null || PersistentEntities ==null;
            SURegions ??= new colDXFRectangles();
            SURegions.Clear();
            
            if (assy == null) return;
           
            System.Drawing.Size sz = System.Drawing.Size.Empty;
           
            try
            {
                BusyMessage = "Refreshing Image";
                ShowProgress = true;
                if (viewer == null)
                {
                    Edit_Startups_View window = (Edit_Startups_View)MyWindow();
                    if (window != null)
                    {
                        Viewer = window.cadfxStartupSpoutDxfViewer;
                        viewer = Viewer;
                    }

                }
                else
                {
                     CurrentView = viewer.CurrentZoomData;
                }

                if (viewer != null) 
                    sz = viewer.Size;
                
             
              //  if(!AllowPositioning) viewer.overlayHandler?.Clear();

                await Task.Run(() =>
                {

                    //draw the persistent entities
                    if (firstDraw)
                    {
                        PersistentEntities = new dxfEntities();
                        Blocks = new List<dxfBlock>();
                           //initialize the image
                           image = new dxfImage("StartupSpouts", appApplication.SketchColor, sz);
                        image.Header.UCSMode = dxxUCSIconModes.None;
                        image.Header.LineTypeScale = 0.2 * image.Header.LineTypeScale;
                        dxoDrawingTool draw = image.Draw;

                        //set the pointer width
                        PointerWd = 0.5 * assy.Downcomer().BoxWidth;
                        if (PointerWd > 0.15 * assy.FunctionalPanelWidth) PointerWd = 0.15 * assy.FunctionalPanelWidth;

                        PersistentEntities.Add(draw.aArc(new uopArc(assy.RingID / 2), aDisplaySettings: dxfDisplaySettings.Null("CIRCLES")) );
                        PersistentEntities.Add(draw.aArc(new uopArc(assy.ShellID / 2), aDisplaySettings: dxfDisplaySettings.Null("CIRCLES")));

                        //draw the spout group perimeters
                        List<uopRectangle> sglimits = assy.SpoutGroups.SpoutLimits();
                        dxfDisplaySettings dsp = new dxfDisplaySettings(aLayer: "0", aColor: dxxColors.DarkGrey, aLinetype: dxfLinetypes.Continuous);
                        foreach (var sgrec in sglimits)
                            PersistentEntities.Add(image.Entities.AddShape(sgrec, dsp, aTag:sgrec.Tag));

                        //draw the dowwncomer edges
                        List<mdDowncomerBox> boxes = assy.Downcomers.Boxes();
                        foreach (var box in boxes)
                        {
                            dxfBlock block = image.Blocks.Add( mdBlocks.DowncomerBox_View_Plan(image, box, assy, bSetInstances: false, bOutLineOnly: false, bSuppressHoles: true, bIncludeSpouts: false, bShowObscured: false, aCenterLineLength: 0, aLayerName: "BOXES", bIncludeEndPlates: false, bIncludeEndSupports: false, bIncludeShelfAngles: false, bIncludeStiffeners: false, bIncludeBaffles: false, bIncludeSupDefs: false, bIncludeFingerClips: false, bIncludeEndAngles: true));
                            PersistentEntities.Add(draw.aInsert(block.Name, box.Center));
                            //colDXFEntities sEnts = box.Edges("0", dxxColors.BlackWhite, dxfLinetypes.Continuous, bQuickLine: true); // This needs to be modified to support multiple boxes
                            //PersistentEntities.AddRange(image.Entities.Append(sEnts, false, aTag: $"DOWNCOMER {box.DowncomerIndex}"));

                        }
                        MaxViewRectangle = dxfEntities.BoundingRectangle(PersistentEntities.FindAll((x) => x.LayerName != "CIRCLES" && x.LayerName != "BEAM"), aWidthAdder: 4 * PointerWd);
                        image.Display.ZoomOnRectangle(MaxViewRectangle, bSetFeatureScales: true);
                    
                        //draw the finger clips
                        if (AllowPositioning)
                        {
                            BusyMessage = "Drawing Finger Clips";
                            mdFingerClip aFC = assy.FingerClip;
                            uopVectors fcPts = assy.FingerClipPoints(bTrayWide:false,bReturnCenters: true);
                            List<dxfEntity> rectangles = draw.aRectangles(fcPts, aFC.Width, aFC.Length, aDisplaySettings: dxfDisplaySettings.Null("FINGER_CLIPS", dxxColors.BlackWhite, dxfLinetypes.Continuous)).OfType<dxfEntity>().ToList();
                            PersistentEntities.AddRange(rectangles );

                        
                        }

                        if (assy.DesignFamily.IsBeamDesignFamily())
                        {
                            
                            BusyMessage = "Drawing Support Beam";
                            dxfBlock block = mdBlocks.Beam_View_Plan(Image, assy.Beam, assy,bSetInstances: true, bObscured: false, bSuppressHoles: true, aLayerName:"BEAM");
                            Blocks.Add(block);
                            PersistentEntities.AddRange(draw.aInserts(block, null, bOverrideExisting: false,  aDisplaySettings: dxfDisplaySettings.Null(block.LayerName)));
                        }

                            
                        if(uopUtils.RunningInIDE)
                            PersistentEntities.Add(draw.aRectangle(MaxViewRectangle, aColor: dxxColors.Red));
                    }
                    else
                    {
                        BusyMessage = "Erasing Pointers";
                        image.Entities.RemoveByHandles(PointerHandles);

                 
                    }

                    //draw the pointers
        
                    PointerHandles = Execute_DrawPointers(image);
                });
            }
                
            catch { }
            finally
            {
                BusyMessage = string.Empty;
                ShowProgress = false;

                
                if (firstDraw)
                {
                    image.Display.LimitsRectangle = MaxViewRectangle;
                    Image = image;
                }

                if (viewer != null)
                {

                    // if (viewer.modelwh)
                    if (!firstDraw)
                    {

                        viewer.SetImage(image);
                        viewer.CurrentZoomData =  CurrentView;
                     
                    }
                    else
                    {
                        viewer.ZoomWindow(MaxViewRectangle);
                    }

                }

                if (!string.IsNullOrWhiteSpace(selected))
                    Selected = Spouts.GetByHandle(selected);

                HighlightStartUp(assy, Selected);

            }


        }

        /// <summary>
        /// Draw objects Location
        /// </summary>
        private List<string> Execute_DrawPointers(dxfImage image)
        {

            PointerHandles = new List<string>();

            
            mdTrayAssembly assy = MDAssy;
            if (assy == null || image == null) return PointerHandles;

            BusyMessage = "Drawing Pointers";
            if (Spouts == null) Spouts = assy.StartupSpouts.Clone();
            SURegions = new colDXFRectangles();
         

            Spouts.UpdateYLimits(assy, assy.Downcomers);
            double wd = image.Display.PaperScale * 0.095;

            PointerWd = wd;
            if (ProjectType == uppProjectTypes.MDDraw) Spouts.SetObscured(assy, Stiffeners);

            uopVectors centers = Spouts.Centers(aXOffset:0.5);

            //the centers Value =the rotation -90 for left side 90 for right
            //the centers Mark = obscured
            //the centers Tag = spouts handle
            //the centers Flag = spouts side LEFT or RIGHT

            dxfDisplaySettings dsp = new("0", dxxColors.BlackWhite, dxfLinetypes.Continuous);
            dxoDrawingTool draw = image.Draw;
            //================== DRAW THE UNOPPRESSED ONES =====================
            List<uopVector> subset = centers.FindAll((x) => !x.Mark && !x.Suppressed);
            Execute_DrawSpouts(draw, subset, dsp);

            //Execute_DrawSpouts(draw, centers.FindAll((x) => !x.Mark && !x.Suppressed && x.Flag == "LEFT"), dsp);
            //Execute_DrawSpouts(draw, centers.FindAll((x) => !x.Mark && !x.Suppressed && x.Flag == "RIGHT"), dsp);

            //================== DRAW THE SUPPRESSED ONES =====================
            dsp.Color = dxxColors.Blue;
            subset = centers.FindAll((x) => !x.Mark && x.Suppressed);
            Execute_DrawSpouts(draw, subset, dsp);
            //Execute_DrawSpouts(draw, centers.FindAll((x) => !x.Mark && x.Suppressed && x.Flag == "LEFT"), dsp);
            //Execute_DrawSpouts(draw, centers.FindAll((x) => !x.Mark && x.Suppressed && x.Flag == "RIGHT"), dsp);

            //================== DRAW THE OBSCURED ONES =====================

            dsp.Color = dxxColors.Red;
            subset = centers.FindAll((x) => x.Mark && !x.Suppressed);
            Execute_DrawSpouts(draw, subset, dsp);
            //Execute_DrawSpouts(draw, centers.FindAll((x) => x.Mark && !x.Suppressed && x.Flag == "LEFT"), dsp);
            //Execute_DrawSpouts(draw, centers.FindAll((x) => x.Mark && !x.Suppressed && x.Flag == "RIGHT"), dsp);

            return PointerHandles;

        }

        private  dxfEntities Execute_DrawSpouts(dxoDrawingTool aDraw,  List<uopVector> aCenters, dxfDisplaySettings aDiplaySettings)
        {
            dxfEntities  _rVal = new dxfEntities();
            if (aDraw == null || aCenters == null || aCenters.Count <= 0) return _rVal;

            uopVector u1 = aCenters[0];
            colDXFVectors dxfvecs = uopVectors.ConvertToDXFVectors(aCenters, bValueAsRotation: true, aZ: 0);
            _rVal = aDraw.aPointers(dxfvecs, PointerWd, aRotation: null, bReturnHollow: u1.Suppressed,  bSuppressInstances:false, aDisplaySettings: aDiplaySettings);

            foreach (var ent in _rVal) PointerHandles.Add(ent.Handle);
            foreach (uopVector ip in aCenters)
            {
                dxfRectangle region = new dxfRectangle(ip, PointerWd * 1.35, PointerWd * 1.35, aTag: ip.Tag,aFlag: ip.Flag);
                if (string.Compare(ip.Flag, "LEFT", true) == 0) region.Move(-region.Width / 2); else region.Move(region.Width / 2);
                //dxfRectangle region2 = new dxfRectangle(region);
                //region2.Translate(trans);
                SURegions.Add(region, aTag: ip.Tag);

            }



            //foreach (uopVector u1 in centers)
            //{

            //    uppSides side = string.Compare(u1.Flag, "LEFT", true) == 0 ? uppSides.Left : uppSides.Right;
            //    double ang = side == uppSides.Left ? 90 : -90;
            //    dxxColors aClr = u1.Mark ? dxxColors.Red : dxxColors.BlackWhite;   // the mark = the spout is obscured
            //    bool hollow = false;
            //    if (u1.Suppressed)
            //    {
            //        aClr = dxxColors.Blue;
            //        hollow = true;
            //    }

            //    u1.Move(side == uppSides.Left ? -1 : 1 * 0.5);

            //    dsp.Color = aClr;
            //    dxfEntity ent = draw.aPointer(u1, PointerWd, aRotation: ang, bReturnHollow: hollow, aDisplaySettings: dsp);
            //    dxfRectangle aRect = ent.BoundingRectangle().Expanded(1.2);
            //    aRect = SURegions.Add(aRect, aTag: u1.Tag);
            //    _rVal.Add(ent.Handle);
            //    if (AllowPositioning)
            //    {
            //        ent = draw.aLine(new dxfVector(u1.X, u1.Y + Spouts.Length / 2), new dxfVector(u1.X, u1.Y - Spouts.Length / 2), dsp);
            //        _rVal.Add(ent.Handle);
            //    }
            //}



            return _rVal;
        }


        public void InputOverrideLength()
        {

            double aVal = OverrideLength * Multiplier_Linear;

            bool canceled = false;
            double newVal = dxfUtils.Input_Decimal(aVal, ref canceled, "Override Calculated Startup Length", "Enter A New Override Length:", "Enter 0 to Clear", aAllowZeroInput: true,
                aAllowNegatives: false, aMaxLimit: 12 * Multiplier_Linear, aMinLimit: ( Spouts.Height) * Multiplier_Linear, aMaxWhole: 2, aMaxDeci: 5, aEqualReturnCancel: false);

            if (canceled) return;
            newVal /= Multiplier_Linear;
            if (newVal == Spouts.Length) newVal = 0;
            OverrideLength = newVal;
            Execute_ReGenerateSpouts(false);



            //if(_OverrideQuestion == null)
            //{
            //    _OverrideQuestion = new mzQuestions();
            //    _OverrideQuestion.AddNumeric("Enter New Override Length:", OverrideLength,LinearUnits,Multiplier_Linear,)
            //}


        }

        private void ResetWaitingForPoint()
        {
            WaitingForClick = false;
            FinishedWaitingForClick = true;
            FinishWaitingForClick = "";
            Viewer.overlayHandler.RemoveByHandle(CursorHandles);
            CursorHandles.Clear();
            Viewer.Cursor = Cursors.Arrow;
        }

        private async void Execute_ReGenerateSpouts(bool bNewConfig = true)
        {
            bool enabledwuz = ShowProgress;
            string curspout = null;
            try
            {

                mdTrayAssembly assy = MDAssy;
                if (assy == null || Spouts == null) return;

               ShowProgress = true;
                curspout = Selected?.Handle;
                uppStartupSpoutConfigurations config = Config;
                mdStartupSpouts oldspouts = bNewConfig ? null : Spouts;

                await Task.Run(() =>
                {
                    mdStartupSpouts newspouts = mdStartUps.Generate(assy, assy.IdealStartupArea, ref config, OverrideLength, oldspouts, ProjectType != uppProjectTypes.MDSpout);
                    LastSpouts = Spouts.Clone();



                    newspouts.UpdateYLimits(assy, assy.Downcomers);
                    Spouts = newspouts;

                   
                });




            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(curspout)) Selected = Spouts.GetByHandle(curspout);
                Execute_RefreshDisplay(bRedraw: true);
                ShowProgress = enabledwuz;
            }
        }


        #endregion Methods

        #region Eventhandlers

        private void Viewer_OnViewerMouseDown(object sender, MouseButtonEventArgs e)
        {

            UOP.DXFGraphicsControl.DXFViewer viewer = Viewer;

            if (viewer == null || MDAssy == null) return;
            System.Windows.Point mouseCoordinate = e.GetPosition((System.Windows.IInputElement)sender);


            var returnPoint = viewer.DeviceToWorld(mouseCoordinate.X, mouseCoordinate.Y);
            MousePoint = new dxfVector(returnPoint.Item1, returnPoint.Item2);
            colDXFRectangles suregions = SURegions;
            if (suregions != null)
            {
                dxfRectangle region = SURegions.GetByNearestCenter(MousePoint);
                if (region != null)
                {

                    mdStartupSpout aSU = Spouts.GetByHandle(region.Tag);
                    Spouts.SetSelected(aSU);

                    if (!WaitingForClick)
                        HighlightStartUp(MDAssy, aSU, region);
                }
            }


            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed && e.ClickCount == 1)
            {
                LeftMouseButtonEventProcessor(System.Windows.Forms.MouseButtons.Left, (int)mouseCoordinate.X, (int)mouseCoordinate.Y);
            }
            else
            {
                if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed && e.ClickCount == 2)
                {
                    Viewer_OnViewerMouseDownDoubleLeftClick(sender, e);
                }
            }
        }

        public void HighlightStartUp(mdTrayAssembly aAssy = null, mdStartupSpout aSU = null, dxfRectangle aSURegion = null)
        {

            SpoutHandle = "";

            aAssy ??= MDAssy;
            if (aAssy == null) return;
            aSU ??= Selected;
            Selected = aSU;
            if (aSU == null) return;
            if (SURegions == null) return;
            aSURegion ??= SURegions.GetTagged(aSU.Handle);
            if (aSURegion == null || Viewer == null) return;
            var overlay = Viewer.overlayHandler;
            overlay?.DrawRectangle(aSURegion, _HighlightColor, 75);
            overlay?.RefreshDisplay();
            Execute_SpoutShow(aSU);
        }

        private void Viewer_OnViewerMouseDownDoubleLeftClick(object sender, MouseButtonEventArgs e)
        {
            

            DXFViewer viewer = Viewer;

            if (viewer == null || MDAssy == null || SURegions == null) return;
            System.Windows.Point mouseCoordinate = e.GetPosition((System.Windows.IInputElement)sender);


            var returnPoint = viewer.DeviceToWorld(mouseCoordinate.X, mouseCoordinate.Y);
            MousePoint = new dxfVector(returnPoint.Item1, returnPoint.Item2);

            dxfRectangle region = SURegions.GetContainingMember(MousePoint, bReturnTheNearest: false, bSuppressProjection: true);
           
            if (region == null)
            {
                if (uopUtils.RunningInIDE)
                    Execute_RefreshDisplay(bRedraw: true, bRefreshData: false);
            }
            else
            {
                mdStartupSpout aSU = Spouts.GetByHandle(region.Tag);
                if (aSU == null) return;
                Spouts.SetSelected(aSU);
                if (AllowPositioning)
                {
                    Execute_InputYOrdinate();
                }
                else
                {
                    string query = aSU.Suppressed ? "On" : "Off";
                    query = $"Toggle Spout {aSU.Name} {query} ?";
                    if (MessageBox.Show(query, "Toggle Spout Suppresion", button: MessageBoxButton.YesNo, icon: MessageBoxImage.Question, defaultResult: MessageBoxResult.No) == MessageBoxResult.No) return;
                     Execute_ToggleSpout();
                }
            }
          

        }

        #endregion Eventhandlers

        #region Commands

        private DelegateCommand _CMD_Generate;
        public ICommand Command_Generate { get { if (_CMD_Generate == null) _CMD_Generate = new DelegateCommand(para => Execute_ReGenerateSpouts()); return _CMD_Generate; } }

        private DelegateCommand _CMD_Toggle;
        public ICommand Command_Toggle { get { if (_CMD_Toggle == null) _CMD_Toggle = new DelegateCommand(para => Execute_ToggleSpout()); return _CMD_Toggle; } }

        private DelegateCommand _CMD_SetY;
        public ICommand Command_SetY { get { if (_CMD_SetY == null) _CMD_SetY = new DelegateCommand(para => Execute_InputYOrdinate()); return _CMD_SetY; } }

        private DelegateCommand _CMD_DragY;
        public ICommand Command_DragY { get { if (_CMD_DragY == null) _CMD_DragY = new DelegateCommand(para => Execute_Drag()); return _CMD_DragY; } }

        private DelegateCommand _CMD_AlignY;
        public ICommand Command_AlignY { get { if (_CMD_AlignY == null) _CMD_AlignY = new DelegateCommand(para => Execute_Align()); return _CMD_AlignY; } }


        private DelegateCommand _CMD_Cancel;
        public ICommand Command_Cancel { get { if (_CMD_Cancel == null) _CMD_Cancel = new DelegateCommand(param => Execute_Cancel()); return _CMD_Cancel; } }

        private DelegateCommand _CMD_OK;
        public ICommand Command_OK { get { if (_CMD_OK == null) _CMD_OK = new DelegateCommand(param => Execute_Save()); return _CMD_OK; } }

        /// <summary>
        /// Reset Command.
        /// </summary>

        private DelegateCommand _CMD_Reset;
        public ICommand Command_Reset { get { if (_CMD_Reset == null) _CMD_Reset = new DelegateCommand(param => Execute_Reset()); ; return _CMD_Reset; } }

        /// <summary>
        /// Undo Command.
        /// </summary>
        private DelegateCommand _CMD_Undo;
        public ICommand Command_Undo { get { if (_CMD_Undo == null) _CMD_Undo = new DelegateCommand(param => Execute_Undo()); return _CMD_Undo; } }


        #endregion Commands
    }
}