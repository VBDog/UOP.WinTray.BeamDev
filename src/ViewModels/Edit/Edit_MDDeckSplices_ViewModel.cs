using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2010.PowerPoint;
using DocumentFormat.OpenXml.Office2016.Excel;
using DocumentFormat.OpenXml.Vml;
using DocumentFormat.OpenXml.Wordprocessing;
using MvvmDialogs;
using Ookii.Dialogs.WinForms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using UOP.DXFGraphics;
using UOP.DXFGraphics.Utilities;
using UOP.DXFGraphicsControl;
using UOP.WinTray.Projects;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Parts;

using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;
using UOP.WinTray.UI.BusinessLogic;
using UOP.WinTray.UI.Commands;
using UOP.WinTray.UI.Messages;
using UOP.WinTray.UI.Views.Windows.CADfx.Questions;

namespace UOP.WinTray.UI.ViewModels
{



    public class Edit_MDDeckSplices_ViewModel : MDProjectViewModelBase, IModalDialogViewModel
    {

        #region INotifyPropertyChanged Implementation

        public override event PropertyChangedEventHandler PropertyChanged;


        public override bool NotifyPropertyChanged(System.Reflection.MethodBase aMethod, bool? bNoErrors = null)
        {
            // Verify if the property is valid
            if (aMethod == null || SuppressEvents) return false;
            string propname = aMethod.Name.Replace("set_", "");
            try
            {
                if (!bNoErrors.HasValue) bNoErrors = !uopUtils.RunningInIDE;
                if (VerifyPropertyName(propname, bNoErrors.Value))
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));

                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch { return false; }


        }

        #endregion INotifyPropertyChanged Implementation

        #region Fields
        readonly SolidColorBrush _RedSolidBrush = new(Colors.Red);
        readonly SolidColorBrush _BlackSolidBrush = new(Colors.Black);

        private dxfRectangle _DisplayRectangle;
        private uopSectionShapes _Shapes;
        private uopDeckSplice _SelectedSplice;
        private List<uopDeckSplice> _Sisters;
        private uopVectors _FreePoints;
        private uopVectors _ManPts;
        private List<string> _Tray_List;
        private mzQuestions _Questions_TRAY;
        private mzQuestions _Questions_Uniform;
        private mzQuestions _Questions_Stradle;
        private colDXFRectangles _SplicePerims;


        private uopDeckSplices _LastRun = null;


        #endregion Fields


        #region Constructors
        public Edit_MDDeckSplices_ViewModel(mdProject project, MDProjectViewModel parentVM) : base(project: project, parentVM: parentVM)
        {

            FinishedWaitingForClick = true;
            SuppressEvents = false;
            SketchColor = appApplication.SketchColor;

            IsEnabled = true;
            ObservableCollection<string> styles = uopEnumHelper.GetDescriptionsObs(typeof(uppSpliceStyles), true);
            if (styles.Contains("Joggles")) styles.Remove("Joggles");

            SpliceStyles = styles;
            _SelectedSpliceStyle = (int)MDAssy.DesignOptions.SpliceStyle;


            ListFieldPanels = string.Empty;
            ListAllPanels = string.Empty;
            int pcnt = MDAssy.DeckPanels.ActivePanels(MDAssy).Count;
            for (int i = 1; i <= pcnt; i++)
            {

                if (i > 1 && (!MDAssy.DesignFamily.IsBeamDesignFamily() || i < pcnt))
                {
                    if (!string.IsNullOrWhiteSpace(ListFieldPanels)) ListFieldPanels += $",";
                    ListFieldPanels += $"Panel {i}";
                }
                if (!string.IsNullOrWhiteSpace(ListAllPanels)) ListAllPanels += ",";
                ListAllPanels += $"Panel {i}";

            }

            if (MDProject == null || MDRange == null) return;
             EditSplices();
        }

        #endregion Constructors;

        #region Properties


        private List<uopDeckSplices> _Runs;
        public List<uopDeckSplices> Runs
        {
            get => _Runs;
            set => _Runs = value;
        }
        private List<uopDeckSplices> _Redos;
        public List<uopDeckSplices> Redos
        {
            get => _Redos;
            set => _Redos = value;
        }




        private uopDeckSplices _Splices;
        public uopDeckSplices Splices
        {
            get {  if(_Splices!= null) _Splices.TrayAssembly = MDAssy;  return _Splices; }
            set
            {
                if (_Splices == null && value != null)
                {
                    _Runs = new List<uopDeckSplices>();
                    _Redos = new List<uopDeckSplices>();

                    _Splices = value;
                    return;
                }

                if (value == null)
                {
                    _Runs = null;
                    _Redos = null;

                }
                else
                {
                    value.TrayAssembly = MDAssy;

                    _ManPts = value.ManwayPoints;
                }
                    IsEnabled_Reset = value != null;
                _Splices = value;

            }
        }

        private System.Drawing.Color SketchColor { get; set; }
        private int Whole => DisplayUnits == uppUnitFamilies.Metric ? 5 : 3;

        private int Deci => DisplayUnits == uppUnitFamilies.Metric ? 1 : 5;

        private int ManCnt { get; set; }

        private double Radius { get => (MDRange != null) ? MDRange.TrayAssembly.DeckRadius : 0; }

        private string Format => DisplayUnits == uppUnitFamilies.Metric ? "{0:0.0}" : "{0:0.0###}";

        private uppSpliceStyles OriginalStyle { get; set; }

        public override bool IsEnglishSelected
        {
            get => base.IsEnglishSelected;
            set
            {
                bool newval = base.IsEnglishSelected != value;
                base.IsEnglishSelected = value;
                if (newval && Activated)
                {

                    Execute_Refresh(true);
                }

            }
        }


        private double Multiplier { get => (LinearUnits == null) ? 1 : LinearUnits.ConversionFactor(DisplayUnits); }

        private double ManholeID { get; set; }

        private double MaxManway { get; set; }

        private string ListFieldPanels { get; set; }

        private string ListAllPanels { get; set; }

        private string LastHighlight { get; set; }

        private double PanelWidth { get; set; }

        private bool WaitingForClick { get; set; }

        private string FinishWaitingForClick { get; set; }

        private bool FinishedWaitingForClick { get; set; }

        private readonly List<ulong> CursorHandles = new();

        private uopVectors PanelCenters 
        {
            get
            {
                if (Panels == null) return new uopVectors();
                uopVectors _rVal = new uopVectors();
                foreach (var item in _Panels)
                {
                    _rVal.Add(new uopVector(item.X, item.Y));
                }
                return _rVal;
            } 
        }

        private List<mdDeckPanel> _Panels;
        public List<mdDeckPanel> Panels
        {
            get
            {
                if (_Panels != null) return _Panels;
                if (MDAssy == null) return new List<mdDeckPanel>();
                //                _Panels = new colMDDeckPanels();
                _Panels = MDAssy.DeckPanels.ActivePanels(MDAssy);
                //_Panels.SubPart(MDAssy);
                //List<mdDeckPanel> panels = MDAssy.DeckPanels.ActivePanels(MDAssy);


                //foreach (var item in panels)
                //{
                //    _Panels.Add(item);
                //}

                return _Panels;
            }
        }

        private uopUnit _LinearUnits;
        private uopUnit LinearUnits { get { _LinearUnits ??= uopUnits.GetUnit(uppUnitTypes.SmallLength); return _LinearUnits; } }

        #endregion Properties

        #region Notifying Property


        private bool _IsEnabled_Redo;
        public bool IsEnabled_Redo
        {
            get => _IsEnabled_Redo;
            set { _IsEnabled_Redo = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); }
        }

        private bool _IsEnabled_Undo;
        public bool IsEnabled_Undo
        {
            get => _IsEnabled_Undo;
            set { _IsEnabled_Undo = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); }
        }
        private bool _IsEnabled_Reset;
        public bool IsEnabled_Reset
        {
            get => _IsEnabled_Reset;
            set { _IsEnabled_Reset = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); }
        }
        private bool? _DialogueResult;
        /// <summary>
        /// Dialogservice result
        /// </summary>
        public bool? DialogResult
        {
            get => _DialogueResult;

            private set { _DialogueResult = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); }
        }

        private ObservableCollection<string> _SpliceStyles;

        public ObservableCollection<string> SpliceStyles
        {
            get => _SpliceStyles;
            set { _SpliceStyles = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); }
        }

        private int _SelectedSpliceStyle;
        public int SelectedSpliceStyle
        {
            get { return _SelectedSpliceStyle; }
            set { if (_SelectedSpliceStyle == value) return; _SelectedSpliceStyle = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); Splices.ChangeSpliceStyle((uppSpliceStyles)_SelectedSpliceStyle, MDAssy); Execute_Refresh(); }
        }

        private bool _ImageOverlayCheckBoxIsChecked;
        public bool ImageOverlayCheckBoxIsChecked
        {
            get => _ImageOverlayCheckBoxIsChecked;
            set { _ImageOverlayCheckBoxIsChecked = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); }
        }

        private string _ImageOverlayLocationLabelText;
        public string ImageOverlayLocationLabelText
        {
            get => _ImageOverlayLocationLabelText;
            set { _ImageOverlayLocationLabelText = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); }
        }

        private string _ManwayCountText;
        public string ManwayCountText
        {
            get => _ManwayCountText;
            set { _ManwayCountText = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); }
        }

        private string _ImageOverlayLabelText;
        public string ImageOverlayLabelText
        {
            get => _ImageOverlayLabelText;
            set { _ImageOverlayLabelText = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); }
        }


        private ObservableCollection<SplicesDataGridViewModel> _SplicesDataGridItems;
        public ObservableCollection<SplicesDataGridViewModel> SplicesDataGridItems
        {
            get => _SplicesDataGridItems;
            set { _SplicesDataGridItems = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); }
        }

        private SplicesDataGridViewModel _SelectedSpliceItem;
        public SplicesDataGridViewModel SelectedSpliceItem
        {
            get => _SelectedSpliceItem;
            set
            {
                _SelectedSpliceItem = value;
                if (value != null && Splices != null)
                {
                    _SelectedSplice = Splices.Find(x => x.Handle == value.SpliceID);
                    Splices.SelectedMember = _SelectedSplice;
                    _SelectedSplice = Splices.SelectedMember;
                    if (_SelectedSplice == null) return;

                    MousePoint = new dxfVector(_SelectedSplice.Center);
                    HighlightSplices(_SelectedSplice.Handle);
                }

                NotifyPropertyChanged(MethodBase.GetCurrentMethod());
            }
        }

        private ObservableCollection<DeckSectionDataGridViewModel> _DeckSectionsDataGridItems;
        public ObservableCollection<DeckSectionDataGridViewModel> DeckSectionsDataGridItems
        {
            get => _DeckSectionsDataGridItems;
            set { _DeckSectionsDataGridItems = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); }
        }

        private string _ManholeIDText;
        public string ManholeIDText
        {
            get => _ManholeIDText;
            set { _ManholeIDText = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); }
        }

        private string _PanelFitLimitText;
        public string PanelFitLimitText
        {
            get => _PanelFitLimitText;
            set { _PanelFitLimitText = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); }
        }

        private string _IdealManwayHeightText;
        public string IdealManwayHeightText
        {
            get => _IdealManwayHeightText;
            set { _IdealManwayHeightText = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); }
        }

        private string _ManwayHeightText;
        public string ManwayHeightText
        {
            get => _ManwayHeightText;
            set { _ManwayHeightText = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); }
        }

        private string _DcSpaceText;
        public string DcSpaceText
        {
            get => _DcSpaceText;
            set { _DcSpaceText = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); }
        }

        private string _PanelWidthText;
        public string PanelWidthText
        {
            get => _PanelWidthText;
            set { _PanelWidthText = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); }
        }
        private string _LeftStatusBarLabelText;
        public string LeftStatusBarLabelText
        {
            get => _LeftStatusBarLabelText;
            set { _LeftStatusBarLabelText = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); }
        }

        private string _RightStatusBarLabelText;
        public string RightStatusBarLabelText
        {
            get => _RightStatusBarLabelText;
            set { _RightStatusBarLabelText = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); }
        }

        private string _SubStatusText;
        public string SubStatusText
        {
            get => _SubStatusText;
            set { _SubStatusText = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); }
        }

        private bool _ShowProgress;
        public bool ShowProgress
        {
            get => _ShowProgress;
            set { _ShowProgress = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); }
        }


        private Brush _manwayHeightForegroundColor;
        public Brush ManwayHeightForegroundColor
        {
            get => _manwayHeightForegroundColor;

            set { _manwayHeightForegroundColor = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); }
        }

        private Brush _ManwayCountForegroundColor;
        public Brush ManwayCountForegroundColor
        {
            get => _ManwayCountForegroundColor;
            set { _ManwayCountForegroundColor = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); }
        }

        public override DXFViewer Viewer
        {
            get => base.Viewer;
            set
            {
                if (value == null)
                {
                    if (base.Viewer != null)
                    {
                        base.Viewer.OnViewerMouseDown -= Viewer_OnViewerMouseDown;
                        base.Viewer.MouseMove -= Viewer_MouseMove;
                        base.Viewer.MouseDoubleClick -= Viewer_MouseDoubleClick;
                        base.Viewer.KeyDown -= Viewer_KeyDown;

                    }
                    base.Viewer = null;
                }
                else
                {
                    base.Viewer = value;
                    value.OnViewerMouseDown -= Viewer_OnViewerMouseDown;
                    value.MouseMove -= Viewer_MouseMove;
                    value.MouseDoubleClick -= Viewer_MouseDoubleClick;
                    base.Viewer.KeyDown -= Viewer_KeyDown;

                    value.OnViewerMouseDown += Viewer_OnViewerMouseDown;
                    value.MouseMove += Viewer_MouseMove;
                    value.MouseDoubleClick += Viewer_MouseDoubleClick;
                    base.Viewer.KeyDown += Viewer_KeyDown;

                }
            }

        }

        public override bool IsEnabled
        {
            get => base.IsEnabled;
            set { base.IsEnabled = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); }
        }

        private bool _Hidden;
        public bool Hidden
        {
            get => _Hidden;
            set { _Hidden = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); }

        }

        #endregion Notifying Property

        #region Worker Code



        private async void AssyStatusChangeHandler(string StatusString, string aSubStatus = null)
        {
            Dispatcher dispatcher = System.Windows.Application.Current?.Dispatcher;
            dispatcher ??= Dispatcher.CurrentDispatcher;

            await dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() =>
                {
                    bool sewuz = SuppressEvents;
                    SuppressEvents = false;
                    SubStatusText = StatusString;
                    SuppressEvents = sewuz;
                })
            );
            //Console.Write(StatusString);

        }



        private async Task Execute_DrawInputAsync(Message_Refresh request)
        {

            LastHighlight = "";
            SubStatusText = "";
            if (request == null) return;
            if (request.MDRange == null || request.Image == null) return;
            await Task.Run(() =>
            {
                dxfImage image = request.Image;
                mdTrayAssembly assy = request.MDAssy;
                if (assy == null) return;
                ShowProgress = true;
                SubStatusText = "Creating Section Shapes";
                if (_Shapes == null)
                {
                    _Shapes = assy.DowncomerData.CreateSectionShapes(Splices, bSetInstances:false);
                }
                    

                SubStatusText = "Formating Image";
                if (request.Viewer != null) image.Display.Size = request.Viewer.Size;
                image.Header.UCSMode = dxxUCSIconModes.None;
                image.TextStyle().FontName = "Txt.shx";
                image.TextStyle().WidthFactor = 0.8;

                List<mdDeckPanel> panels = Panels;
                colMDDowncomers DComers = assy.Downcomers;

                dxfVector v1 = dxfVector.Zero;
                dxoDrawingTool draw = image.Draw;
                mdDeckPanel aDP = panels.Last();
                double rRad = assy.RingID / 2.0;
                double wd = rRad - aDP.Left();

                double xLeft = aDP.Left();

                SubStatusText = "Defining Extents";
                _DisplayRectangle = new dxfRectangle(new dxfVector(xLeft + 0.5 * wd, 0), aDP.Radius - xLeft, 2 * aDP.Radius);
                _DisplayRectangle.ScaleDimensions(1.1, 1.1);
                image.Display.SetDisplayRectangle(_DisplayRectangle, bSetFeatureScales: true, bNoRedraw: false, bSaveAsLimits: true);

                double paperscale = image.Display.PaperScale;

                //draw.aRectangle(_DisplayRectangle, aColor: dxxColors.Red);

                dxeLine hcline = draw.aCenterlines(_DisplayRectangle, aLengthAdder: 0.25 * paperscale, aSuppressHorizontal: false, aSuppressVertical: true, aDisplaySettings: dxfDisplaySettings.Null(aColor: dxxColors.Red, aLinetype: dxfLinetypes.Center))[0];

                double leftlim = image.Display.ExtentRectangle.Left;
                //draw the downcomers below
                SubStatusText = "Drawing Downcomers Below";
                for (int i = 1; i <= DComers.Count; i++)
                {
                    mdDowncomer dc = DComers.Item(i);
                    if (dc.IsVirtual) continue;

                    foreach (var box in dc.Boxes)
                    {
                        if (box.IsVirtual) continue;

                        colDXFVectors verts = box.ExtractDowncomerBelowVertices(trimOrder: true);

                        if (verts.Item(1).X < leftlim) verts.Item(1).X = leftlim;
                        if (verts.Item(4).X < leftlim) verts.Item(4).X = leftlim;

                        image.Entities.Add(new dxePolyline(verts, true) { Color = dxxColors.Orange, Linetype = dxfLinetypes.Hidden });
                        if (assy.DesignFamily.IsStandardDesignFamily() && dc.OccuranceFactor > 1)
                        {
                            verts.MirrorPlanar(aMirrorY: 0);
                            image.Entities.Add(new dxePolyline(verts, true) { Color = dxxColors.Orange, Linetype = dxfLinetypes.Hidden });

                        } 
                    }
                }
                SubStatusText = "Labeling Panels";
                // put labels indicating the panel at the top and bottom of the drawing
                double txtY = _DisplayRectangle.Top - 0.1 * paperscale * 0.75;
                for (int i = 1; i <= panels.Count; i++)
                {
                    aDP = panels[i - 1];
                    v1.SetCoordinates(aDP.X, txtY);
                    dxeText tx = draw.aText(v1, i, 0.15 * paperscale, dxxMTextAlignments.MiddleCenter, aColor: dxxColors.Blue);
                    v1.Y *= -1;
                    tx = draw.aText(v1, i, 0.15 * paperscale, dxxMTextAlignments.MiddleCenter, aColor: dxxColors.Blue);
                }

                SubStatusText = "Drawing Section Shapes";
                double mandia = assy.ManholeID;
                dxfDisplaySettings dsp = null;
                if (_Shapes != null)
                {
                    foreach (var shape in _Shapes)
                    {
                        bool bFits = shape.FitsThruManhole(mandia, 0.5);
                        dxePolyline aPl = (dxePolyline)image.Entities.Add(shape.Perimeter("0", (!bFits) ? dxxColors.Red : dxxColors.BlackWhite, dxfLinetypes.Continuous));
                        if (shape.IsManway) 
                            draw.aHatch_ByFillStyle(dxxFillStyles.UpwardDiagonal, aPl, aScaleFactor:paperscale, aDisplaySettings: dxfDisplaySettings.Null(aColor: dxxColors.LightGrey));



                        if (assy.DesignOptions.HasBubblePromoters)
                        {
                            SubStatusText = "Creating Bubble Promoter Centers";
                            uopVectors BPCenters = shape.BPSites;
                            dsp = new dxfDisplaySettings("0", dxxColors.BlackWhite, dxfLinetypes.Continuous);
                            SubStatusText = "Drawing Bubble Promoters";
                            draw.aCircles(BPCenters.FindAll(x => !x.Suppressed), mdGlobals.BPRadius, dsp);
                            dsp.Color = dxxColors.Red;
                            draw.aCircles(BPCenters.FindAll(x => x.Suppressed), mdGlobals.BPRadius, dsp);

                        }
                    }
                }
               

                _SplicePerims = new colDXFRectangles();
                if (Splices.Count > 0)
                {
                    dsp = new dxfDisplaySettings("0", dxxColors.LightGrey, dxfLinetypes.Hidden);
                    SubStatusText = "Drawing Splices";
                    foreach (uopDeckSplice splice in Splices)
                    {
                        uopLine cl = splice.GetCenterLine(assy);
                        dxeLine aLn = draw.aLine(cl, aDisplaySettings:dsp);
                        uopRectangle srec = splice.SelectionBounds(assy, aWidth: 0.5);
                        _SplicePerims.Add(srec.ToDXFRectangle());
                    }
                }
                LastHighlight = "";



                SubStatusText = "";
            });

            //image.SaveToFile(true, @"C:\Junk\Test.DXF", aFileType: dxxFileTypes.DXF);
            // Viewer.SaveDwg(@"C:\Junk\Test.DWG",out string msg);

        }

        private void Execute_DrawInput_Complete(Message_Refresh request)
        {

            try
            {
                RightStatusBarLabelText = "Loading Drawing";
                SubStatusText = "";
                if (request.Viewer != null && request.Image != null)
                {
                    request.Viewer.Clear();
                    request.Viewer.SetImage(request.Image);
                    // Viewer.ZoomWindow( DXFImage.Display.LimitsRectangle );
                    request.Viewer.ZoomExtents();

                }
                request.StatusMessage = "";
                ShowProgress = false;
                FreePointsSet();
                Execute_RefreshData();
            }
            catch { }
            finally
            {
                ShowProgress = false;
                //HightlightSplices();
                //ShowData();
                //Viewer.Execute_Refresh();
                IsEnabled = true;
                RightStatusBarLabelText = "";
                IsEnabled_Redo = Redos.Count > 0;
                IsEnabled_Undo = Runs.Count > 0;
                IsEnabled_Reset = IsEnabled_Undo;

            }


        }

        

        public override void Dispose()
        {
            base.Dispose();
            Viewer = null;
            ReleaseReferences();
        }


        #endregion Worker Code

        #region Event Handlers

        public void LeftMouseButtonEventProcessor(System.Windows.Forms.MouseButtons mouseButtons, int x, int y)
        {
            if (Viewer == null) return;

            if (mouseButtons == System.Windows.Forms.MouseButtons.Left)
            {
                if (MousePoint == null) MousePoint = new dxfVector();


                MousePoint.SetCoordinates(x, y);
                (double px, double py) = Viewer.DeviceToWorld(MousePoint.X, MousePoint.Y);
                MousePoint.SetCoordinates(px, py);

                if (WaitingForClick)
                {
                    WaitingForClick = false;
                    Viewer.overlayHandler.RemoveByHandle(CursorHandles);
                    switch (FinishWaitingForClick)
                    {
                        case "ADDSPLICE":
                            Command_Add.Execute(null);
                            break;
                        case "MANADD":
                            Command_ManwayAdd.Execute(null);
                            break;
                        case "MANREMOVE":
                            Command_ManwayRemove.Execute(null);
                            break;
                    }
                    return;
                }

                dxfRectangle rec = _SplicePerims.GetByNearestCenter(MousePoint);
                if (rec == null) return;

                foreach (SplicesDataGridViewModel item in SplicesDataGridItems)
                {
                    if (item.SpliceID == rec.Tag)
                    {
                        SelectedSpliceItem = item;
                        break;
                    }
                }
            }
        }

        public void LefMouseDoubleClickEventProcessor(int x, int y) { }

        public void SplicesDataGrid_SelectionChanged(object selectedItem)
        {
            if (selectedItem is SplicesDataGridViewModel)
            {
                SplicesDataGridViewModel drow = (SplicesDataGridViewModel)selectedItem;
                _SelectedSplice = Splices.Find(x => x.Handle ==drow.SpliceID);
                MousePoint = _SelectedSplice.Center.ToDXFVector();
                HighlightSplices();
            }
        }

        #endregion Event Handlers



        #region Commands

        private void FinishCommand(bool bExecuted, string aErrorString = null)
        {
            if (bExecuted)
            {
                _Shapes = null;
                SaveRun(_LastRun);
                _Shapes = null;
                Execute_Refresh();
                Execute_RefreshData();
            }
            if (!string.IsNullOrWhiteSpace(aErrorString))
            {
                System.Windows.MessageBox.Show("Error : " + aErrorString, "Error Detected", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            IsEnabled_Redo = Redos.Count > 0;
            IsEnabled_Undo = Runs.Count > 0;
            IsEnabled_Reset = IsEnabled_Undo;
            _LastRun = null;
            // CommandManager.InvalidateRequerySuggested();
        }

        private DelegateCommand _CMD_Generate_Uniform;
        public ICommand Command_GenerateUniform { get { _CMD_Generate_Uniform ??= new DelegateCommand(param => Execute_Generate_Uniform()); return _CMD_Generate_Uniform; } }


        private DelegateCommand _CMD_Generate_Stradle;
        public ICommand Command_GenerateStradle { get { _CMD_Generate_Stradle ??= new DelegateCommand(param => Execute_Generate_Stradle()); return _CMD_Generate_Stradle; } }

        private DelegateCommand _CMD_Copy_Tray;
        public ICommand Command_CopyTray { get { _CMD_Copy_Tray ??= new DelegateCommand(param => Execute_CopyFromOtherTray()); return _CMD_Copy_Tray; } }

        private DelegateCommand _CMD_Add;
        public ICommand Command_Add { get { _CMD_Add ??= new DelegateCommand(param => Execute_Add()); return _CMD_Add; } }


        private DelegateCommand _CMD_Manway_Add;
        public ICommand Command_ManwayAdd { get { _CMD_Manway_Add ??= new DelegateCommand(param => Execute_ManwayAdd()); return _CMD_Manway_Add; } }


        private DelegateCommand _CMD_Manway_Remove;
        public ICommand Command_ManwayRemove { get { _CMD_Manway_Remove ??= new DelegateCommand(param => Execute_ManwayRemove()); return _CMD_Manway_Remove; } }

        private DelegateCommand _CMD_Manways_Remove;
        public ICommand Command_ManwaysRemove { get { _CMD_Manways_Remove ??= new DelegateCommand(param => Execute_ManwaysRemove()); return _CMD_Manways_Remove; } }

        private DelegateCommand _CMD_Manways_Resize;
        public ICommand Command_ManwaysResize { get { _CMD_Manways_Resize ??= new DelegateCommand(param => Execute_ManwayResize()); return _CMD_Manways_Resize; } }

        private DelegateCommand _CMD_Moon_Split;
        public ICommand Command_MoonSplit { get { _CMD_Moon_Split ??= new DelegateCommand(param => Execute_VerticalSpliceToggle()); return _CMD_Moon_Split; } }



        private DelegateCommand _CMD_Set_Ordinate;

        public ICommand Command_SetOrdinate { get { _CMD_Set_Ordinate ??= new DelegateCommand(param => Execute_SetOrd()); return _CMD_Set_Ordinate; } }



        private DelegateCommand _CMD_Delete;
        public ICommand Command_Delete { get { _CMD_Delete ??= new DelegateCommand(param => Execute_Delete()); return _CMD_Delete; } }



        private DelegateCommand _CMD_Clear;
        public ICommand Command_Clear { get { _CMD_Clear ??= new DelegateCommand(param => Execute_Clear()); return _CMD_Clear; } }


        private DelegateCommand _CMD_Undo;
        public ICommand Command_Undo { get { _CMD_Undo ??= new DelegateCommand(param => Execute_Undo()); return _CMD_Undo; } }


        private DelegateCommand _CMD_Redo;
        public ICommand Command_Redo { get { _CMD_Redo ??= new DelegateCommand(param => Execute_Redo()); return _CMD_Redo; } }


        private DelegateCommand _CMD_Reset;
        public ICommand Command_Reset { get { _CMD_Reset ??= new DelegateCommand(param => Execute_Reset()); return _CMD_Reset; } }



        private DelegateCommand _CMD_Refresh;
        public ICommand Command_Refresh { get { _CMD_Refresh ??= new DelegateCommand(param => Execute_Refresh()); return _CMD_Refresh; } }


        private DelegateCommand _CMD_Cancel;
        public ICommand Command_Cancel { get { _CMD_Cancel ??= new DelegateCommand(param => Execute_Cancel()); return _CMD_Cancel; } }


        private DelegateCommand _CMD_Save;
        public ICommand Command_Save { get { _CMD_Save ??= new DelegateCommand(param => Execute_Save()); return _CMD_Save; } }


        #endregion Commands

        #region Methods

        private void Execute_Undo()
        {
            if (!IsEnabled_Undo || Runs.Count <= 0) return;
            if (!IsEnabled) return;
            if (WaitingForClick) ResetWaitingForPoint();

            Redos.Add(Splices.Clone());
            Splices = Runs.Last();
            SaveRun(null, true);
            FinishCommand(true);
        }


        private void Execute_Redo()
        {
            if (!IsEnabled_Redo || Redos.Count <= 0) return;
            if (!IsEnabled) return;
            if (WaitingForClick) ResetWaitingForPoint();

            _LastRun = Splices.Clone();
            Splices = Redos.Last();
            Redos.RemoveAt(Redos.Count - 1);
            FinishCommand(true);
        }
        private void Execute_Reset()
        {
            if (!IsEnabled_Reset || MDAssy == null) return;
            if (!IsEnabled) return;
            if (WaitingForClick) ResetWaitingForPoint();


           
            if( System.Windows.MessageBox.Show("Reset Splices To Original Configuration ?", "Reset ?", button: MessageBoxButton.YesNo, icon: MessageBoxImage.Question, defaultResult: MessageBoxResult.No) != MessageBoxResult.Yes) return;
            
            uopDeckSplices aSplices = new uopDeckSplices(MDAssy.DeckSplices); 
            _LastRun = null;
            _Runs = new List<uopDeckSplices>();
            _Redos = new List<uopDeckSplices>();

            SelectedSpliceStyle = (int)aSplices.SpliceStyle;
            Splices = aSplices;

            FinishCommand(true);
        }

        private void Execute_Add()
        {
            if (WaitingForClick || !IsEnabled) return;

            if (!FinishedWaitingForClick)
            {
                ResetWaitingForPoint();
                _LastRun = Splices.Clone();
                MousePoint ??= new dxfVector();

                FinishCommand(Execute_AddComplete(MousePoint.X, MousePoint.Y));
            }
            else
            {
                WaitingForClick = true;
                FinishedWaitingForClick = false;
                FinishWaitingForClick = "ADDSPLICE";
                Viewer.Cursor = System.Windows.Input.Cursors.ScrollNS;
            }


        }
        private bool Execute_AddComplete(double aX, double ordinate)
        {
            mdTrayAssembly assy = MDAssy;
            if (assy == null ) return false;
            bool ret = default;
            uopVectors pcenters = PanelCenters;
            if (pcenters.Count == 0) return false;
            uopVector u1 = pcenters.Nearest(new uopVector(aX, ordinate));

            mdDeckPanel aDP = Panels.Find((x) => Math.Round(x.X, 3) == Math.Round(u1.X, 3));
           
            if (aDP == null || aDP.IsHalfMoon) return false;


            List<int> dpIDs;
           

            ordinate = DisplayUnits == uppUnitFamilies.Metric ? uopUtils.RoundTo(ordinate * 25.4, dxxRoundToLimits.Millimeter) : uopUtils.RoundTo(ordinate, dxxRoundToLimits.Eighth);
            if (DisplayUnits == uppUnitFamilies.Metric) ordinate /= 25.4;

            mzQuestions aQuestions = new();
            aQuestions.AddNumeric("Enter Splice Ordinate:", ordinate, LinearUnits.Label(DisplayUnits), Multiplier, assy.RingRadius - 2, -assy.RingRadius + 2, aMaxWholeDigits: Whole, aMaxDecimals: Deci, bShowAllDigits: true);
            aQuestions.AddMultiSelect("Select Panels:", ListFieldPanels, "All", 1, aChoiceDelimiter: ",");

            if (!assy.DesignFamily.IsBeamDesignFamily())
            {
                aQuestions.AddCheckVal("Add Mirrors?", true); 
            }
            bool bCanc = !aQuestions.PromptForAnswers("Confirm Splice Location", true, owner: MyWindow());

            if (bCanc) return false;
            _LastRun = Splices.Clone();

            string pnllist = aQuestions.AnswerS("Select Panels:", string.Empty);

            dpIDs = GetPanelIndices(pnllist, aQuestions.Item(2).Choices.ToList());
            if (dpIDs.Count <= 0) return false;
            ordinate = (double)aQuestions.Item(1).Answer;
            bool addmirrors = assy.DesignFamily.IsBeamDesignFamily() ? false : (bool)aQuestions.Item(3).Answer; 


            HashSet<int> violatingPanelIds = new HashSet<int>();
         
            List<uopDeckSplice> added = new List<uopDeckSplice>();
            HashSet<string>  errors = new HashSet<string>();

            foreach (int i in dpIDs)
            {


                Splices.AddHorizontalSpliceMD(MDAssy, i, ordinate, out string err, out uopDeckSplice splice1, out uopDeckSplice splice2, aSnapToMax: true, bAvoidManways: true, bAddMirror: addmirrors);
                if (splice1 != null) added.Add(splice1);
                if (splice2 != null) added.Add(splice2);

         
                if ( !string.IsNullOrWhiteSpace(err))
                {
                    violatingPanelIds.Add(i);
                    errors.Add(err);
                }

            }

            if ( violatingPanelIds.Count > 0)
            {
                string validationText = $"The selected ordinate should be within, and not closer than {mdDeckPanel.MinDistanceFromTopOrBottom} to the top or bottom of the containing panel shape.\r\n\r\nViolating panel ID(s): \r\n{string.Join(", ", violatingPanelIds)}. \r\n\r\nErrors: \\r\\n{string.Join(", ", errors )}";
                System.Windows.MessageBox.Show(validationText, "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
            }
             ret = added.Count > 0;
            if (ret)
            {
                Splices.SetSelected(added[0]);
                _Shapes = null;

            }


            return ret;
        }

        private void Execute_ManwayAdd()
        {

            if (WaitingForClick || !IsEnabled) return;

            if (!FinishedWaitingForClick)
            {
                ResetWaitingForPoint();
                _LastRun = Splices.Clone();
                string mnHdl = "";

                var pt = _FreePoints.Nearest(MousePoint);
                if (pt != null)
                {
                    mnHdl = pt.Handle;
                    if (Splices.GetByManwayHandle(mnHdl).Count > 0) mnHdl = "";
                }
                bool ret = Execute_ManwayAddComplete(mnHdl, out string rErr);
                FinishCommand(ret, rErr);
            }
            else
            {
                WaitingForClick = true;
                FinishedWaitingForClick = false;
                FinishWaitingForClick = "MANADD";
                Viewer.Cursor = System.Windows.Input.Cursors.Cross;
            }
        }
        private bool Execute_ManwayAddComplete(string aHandle, out string rErr)
        {


            bool ret = default;
            rErr = "";

            mdTrayAssembly assy = MDAssy;
            if (assy == null) return false;


            double manht;
            aHandle = aHandle.Trim();
            if (string.IsNullOrEmpty(aHandle)) return ret;

            if (Splices.ManwayCount(false) <= 0)
            {
                mzQuestions query = new mzQuestions("Enter Manway Height");
                mzQuestion q1 = query.AddNumeric("Manway Height:", Splices.ManwayHeight, LinearUnits.Label(DisplayUnits), Multiplier, MaxManway, 12, uopValueControls.PositiveNonZero, Whole, Deci);
                if (query.PromptForAnswers(null)) Splices.ManwayHeight = q1.AnswerD;
            }
            manht = Splices.ManwayHeight;
            Splices.SpliceStyle = (uppSpliceStyles)SelectedSpliceStyle;
            if (Splices.AddManway(assy, aHandle, _FreePoints, Splices.SpliceStyle, ref manht, out int _, out rErr))
            {
                _ManPts = Splices.ManwayPoints;
                _Shapes = null;
                ret = true;
            }

            return ret;
        }

        private void Execute_ManwayRemove()
        {
            bool ret = false;
            if (WaitingForClick || !IsEnabled) return;

            if (!FinishedWaitingForClick)
            {
                ResetWaitingForPoint();
                _LastRun = Splices.Clone();
                string mnHdl = "";
                var pt = _FreePoints.Nearest(MousePoint);
                if (pt != null)
                {
                    mnHdl = pt.Handle;
                    if (Splices.GetByManwayHandle(mnHdl).Count == 0)
                    {
                        mnHdl = "";
                    }
                    else
                    {
                        if (System.Windows.MessageBox.Show($"Delete Manway {mnHdl}?", "Remove Manway", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.No)
                            mnHdl = "";
                    }
                }
          
                FinishCommand(Execute_ManwayRemoveComplete(mnHdl));
            }
            else
            {
                WaitingForClick = true;
                FinishedWaitingForClick = false;
                FinishWaitingForClick = "MANREMOVE";
                Viewer.Cursor = System.Windows.Input.Cursors.Cross;
            }
        }

        private bool Execute_ManwayRemoveComplete(string aHandle)
        {
            mdTrayAssembly assy = MDAssy;
            if (assy == null) return false;
            bool rBackFill = true;
            if (string.IsNullOrEmpty(aHandle)) return false;
            return Splices.RemoveManway(aHandle, assy, out int _, ref rBackFill);
       
        }

        private void Execute_ManwaysRemove()
        {
            if (WaitingForClick || !IsEnabled) return;
            bool ret = default;
            bool bCanc;
            mdTrayAssembly assy = MDAssy;
            if (assy == null) return;
            mzQuestions aQuestions;

            List<string> manIDs = Splices.ManwayHandles();

            if (manIDs.Count <= 0) return;
            if (manIDs.Count == 1)
            {

                if (System.Windows.MessageBox.Show($"Delete Manway {manIDs[0]}?", "Delete Manway", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.No)
                    return;

                Splices.RemoveManway(manIDs[0], assy);
                ret = true;
                FinishCommand(ret);
                return;
            }

            manIDs.Insert(0, "All");
            aQuestions = new mzQuestions();
            mzQuestion question1 = aQuestions.AddMultiSelect("Manways", manIDs, "All", aChoiceDelimiter: ";");
            bCanc = !aQuestions.PromptForAnswers("Select Manways To Remove", true, owner: MyWindow());
            if (bCanc) return;
            _LastRun = Splices.Clone();
            string selects = question1.Answers.ToList(aDelim: ";");
            if (selects == "") return;
            if (selects.Contains("All"))
            {
                Splices.RemoveManways(assy,  ioBackFill: false);
                ret = true;
            }
            else
            {
                string[] svals = selects.Split(';');
                for (int i = 1; i <= svals.Length; i++)
                {
                    selects = svals[i - 1];
                    if (string.Compare(selects, "All", ignoreCase: true) != 0)
                    {
                        if (Splices.RemoveManway(selects, assy)) ret = true;
                    }
                }

            }

            FinishCommand(ret);
        }

        private void Execute_ManwayResize()
        {
            if (!IsEnabled) return;
            if (WaitingForClick) ResetWaitingForPoint();


            mdTrayAssembly assy = MDAssy;
            if (assy == null) return;

            bool ret = default;

            uopDeckSplices aSplices;

            mzQuestions aQuestions = new("Enter a New Manway Height");
            double aHt = Splices.ManwayHeight;
            if (aHt <= 0)
            {
                ret = true;
                aHt = mdUtils.IdealManwayHeight(assy, Splices.SpliceStyle);
                Splices.ManwayHeight = aHt;
            }

            mzQuestion question1 = aQuestions.AddNumeric("Manway Height:", aHt, LinearUnits.Label(DisplayUnits), Multiplier, MaxManway, 8, uopValueControls.PositiveNonZero, Whole, Deci);
            if (!aQuestions.PromptForAnswers("", true, owner: null)) return;
            _LastRun = Splices.Clone();
            aHt = question1.AnswerD;
            aSplices = Splices.Clone();
            if (aHt != Splices.ManwayHeight) ret = true;
            aSplices.ManwayHeight = aHt;
            if (aSplices.ManwayCount() > 0)
            {
                aSplices.RegenerateManways(assy, aHt);

                ret = EditsMade(aSplices, Splices);
                if (ret) Splices = aSplices;
            }
            else
            {
                if (ret)
                {
                    _Shapes = null;
                    Splices = aSplices;
                }
                ret = false;
            }

            FinishCommand(ret);

        }

        private void Execute_VerticalSpliceToggle()
        {
            if (!IsEnabled) return;
            if (WaitingForClick) ResetWaitingForPoint();

            mdTrayAssembly assy = MDAssy;
            if (assy == null) return;
            bool ret = default;
            mdDeckPanel sPanel;
            uopDeckSplice aSplice;
            bool bRemoving;
            string err = "";
            bRemoving = Splices.FindIndex(x => x.Vertical) >=0;

            if (bRemoving)
            {
                Splices.RemoveVerticalSplices();
                FinishCommand(true);
                return;
            }

            sPanel = Panels.First();

            if (sPanel.Width < ManholeID - 0.5)
            {

                if (System.Windows.MessageBox.Show("Vertical Splices Not Required.\nAdd Vertical Splices?", "SPlice Not Required", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.No)
                    return;
            }

            aSplice = sPanel.BasicDeckSplice(assy, aSpliceFamily: Splices.SpliceStyle);
            if (aSplice.MinOrdinate >= aSplice.MaxOrdinate)
            {
                err = "The Half Moon Panels Are Too Narrow To Accommodate Splicing";
                ret = false;
            }
            else
            {
                ret = true;
            }
            if (ret) _LastRun = Splices.Clone();
            Splices.AddVerticalSplices(assy, Splices.SpliceStyle);
            FinishCommand(ret, err);

        }

        private void Execute_SetOrd(uopDeckSplice aSplice = null)
        {
            if (!IsEnabled) return;
            if (WaitingForClick) ResetWaitingForPoint();

            mdTrayAssembly assy = MDAssy;
            if (assy == null) return;
            aSplice ??= Splices.GetSelected();
            if (aSplice == null) return;
            if (!string.IsNullOrWhiteSpace(aSplice.ManwayHandle)) return;

            bool vertical = aSplice.Vertical;
            double aOrd = aSplice.Ordinate;
            bool ret = false;
            mzQuestions aQuestions;
            aQuestions = new mzQuestions();

            double maxOrd = aSplice.MaxOrdinate.HasValue ? aSplice.MaxOrdinate.Value : double.MaxValue ;
            double minOrd = vertical ? aSplice.MinOrdinate.HasValue ? aSplice.MinOrdinate.Value : double.MinValue : -maxOrd;
            string aPmt = vertical ? "Enter New X Ordinate" : "Enter New Y Ordinate";


            List<string> pNames = new();
            List<int> pIDs;
            double bOrd;
            uopDeckSplice bSplc;
            List<uopDeckSplice> bSplcs;

            bool addmirrors = false;
            pIDs = new List<int> { aSplice.PanelIndex };

            for (int i = 1; i <= Splices.Count; i++)
            {
                bSplc = Splices.Item(i);
                if (!bSplc.IsEqual(aSplice))
                {
                    if (bSplc.ManwayHandle == "")
                    {
                        if (bSplc.Vertical == vertical)
                        {
                            if (Math.Abs(Math.Round(bSplc.Ordinate, 1)) == Math.Abs(Math.Round(aOrd, 1)))
                            {
                                if (pIDs.FindIndex(x => x == bSplc.PanelIndex) < 0) pIDs.Add(bSplc.PanelIndex);
                                if (Math.Round(aOrd, 1) != 0)
                                {
                                    if (Math.Round(bSplc.Ordinate, 1) == -Math.Round(aOrd, 1)) addmirrors = true;
                                }
                            }
                        }
                    }
                }
            }

            mzQuestion question2 = null;
            mzQuestion question3 = null;
            mzQuestion question4 = null;

            aQuestions.Title = aPmt;
            mzQuestion question1 = aQuestions.AddNumeric("Splice Ordinate:", aSplice.Ordinate, LinearUnits.Label(DisplayUnits), Multiplier, maxOrd, minOrd, uopValueControls.None, Whole, Deci);

            if (!vertical && !assy.DesignFamily.IsBeamDesignFamily())
            {
                if (addmirrors)
                    question2 = aQuestions.AddCheckVal("Move Mirrors?", true);
                else
                    question4 = aQuestions.AddCheckVal("Add Mirrors?", true);
            }


            if (!vertical)
            {
                pIDs.Sort();
                foreach (int item in pIDs) { pNames.Add($"Panel {item}"); }
                question3 = aQuestions.AddMultiSelect("Select Panels:", pNames, "All", 1);

            }

            bool bCanc = !aQuestions.PromptForAnswers(aPmt, true, owner: MyWindow(), aSaveButtonText: "Apply");
            if (bCanc) return;

            _LastRun = Splices.Clone();

            bOrd = question1.AnswerD;
            if (bOrd == aOrd) return;
            if (question2 != null) addmirrors = question2.AnswerB;
            List<int> panelIDS = new();
            if (question3 == null)
                panelIDS.Add(aSplice.PanelIndex);
            else
                panelIDS = DPIndexes(question3.Answers);

            List<uopDeckSplice> moved = new();
            List<int> violatedPanels = new List<int>(); // Panel IDs of the panels for which the selected ordinate was invalid.

            for (int i = 1; i <= panelIDS.Count; i++)
            {
                mdDeckPanel panel = MDAssy.DeckPanels.Item(panelIDS[i - 1]);
                var panelShapes = mdDeckPanel.GetPanelShapes(panel,assy);
                if (vertical)
                {
                    if (!panelShapes.Any(ps => mdDeckPanel.IsValidPanelShapeForVerticalDeckSpliceOrdinate(ps, bOrd)))
                    {
                        violatedPanels.Add(panelIDS[i - 1]);
                        continue;
                    }
                }
                else
                {
                    if (!panelShapes.Any(ps => mdDeckPanel.IsValidPanelShapeForDeckSpliceOrdinate(ps, bOrd, mdDeckPanel.MinDistanceFromTopOrBottom)))
                    {
                        violatedPanels.Add(panelIDS[i - 1]);
                        continue;
                    }
                }

                bSplcs = Splices.GetAtOrdinate(aOrd, true, vertical, false, panelIDS[i - 1]);
                for (int j = 1; j <= bSplcs.Count; j++)
                {
                    bSplc = bSplcs[j - 1];

                    if (Splices.MoveSpliceTo(assy, bSplc.Handle, bOrd, false))
                    {
                        ret = true;
                        moved.Add(Splices.GetByHandle(bSplc.Handle));
                    }
                }
            }

            if (violatedPanels.Count > 0)
            {
                System.Windows.MessageBox.Show($"The selected ordinate is invalid for these panels:\r\n{string.Join(", ", violatedPanels)}", "Invalid Splice Ordinate", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            if (moved.Count > 0 && question4 != null)
            {
                if (question4.AnswerB)
                {
                    foreach (uopDeckSplice item in moved)
                    {
                        Splices.AddHorizontalSpliceMD(assy, item.PanelIndex, -item.Ordinate);
                    }
                }
            }


            if (addmirrors)
            {

                for (int i = 1; i <= panelIDS.Count; i++)
                {
                    bSplcs = Splices.GetAtOrdinate(-aOrd, true, vertical, false, panelIDS[i - 1]);
                    for (int j = 1; j <= bSplcs.Count; j++)
                    {
                        bSplc = bSplcs[j - 1];

                        if (Splices.MoveSpliceTo(assy, bSplc.Handle, -bOrd, false))
                        {
                            ret = true;
                        }
                    }
                }
            }


            FinishCommand(ret);

        }
        public void Execute_Delete(uopDeckSplice aSplice = null)
        {
            if (!IsEnabled) return;
            if (WaitingForClick) ResetWaitingForPoint();

            mdTrayAssembly assy = MDAssy;
            if (assy == null) return;
            bool ret = default;
            aSplice ??= Splices.SelectedMember;

            if (aSplice == null) return;
            double ord = aSplice.Ordinate;
            if (aSplice.Vertical)
            {
                if (System.Windows.MessageBox.Show("Delete Vertical Splices?", "Delete Splice", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.No)
                {
                    return;
                }
                else
                {
                    _LastRun = Splices.Clone();
                    if (Splices.GetAtOrdinate(ord, bSuppressManways: true, bVerticals: true, bReturnMirrors: true, aPanelIndex: aSplice.PanelIndex, bRemove: true).Count > 0)
                    {
                        _SelectedSplice = null;
                        MousePoint = null;
                        FinishCommand(true);
                        return;

                    }
                }
            }
            else if (aSplice.SupportsManway)
            {
                if (System.Windows.MessageBox.Show($"Delete Manway {aSplice.ManwayHandle}?", "Delete Splice", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.No)
                {
                    return;
                }
                else
                {
                    bool BackFill = false;
                    _LastRun = Splices.Clone();
                    ret = Splices.RemoveManway(aSplice.ManwayHandle, assy, out int _, ref BackFill);
                    FinishCommand(ret);
                    return;
                }

            }

      
          
            int cnt2 = 0;
              List<int> Idxs = new();
            mzQuestions aQuestions;
            aQuestions = new mzQuestions();

            List<double> ds = Splices.Ordinates(5);
            ord = aSplice.Ordinate;
            List<uopDeckSplice> targets1 = Splices.GetAtOrdinate(ord, bSuppressManways: true, bVerticals: false, bReturnMirrors: false);
            int cnt1 = targets1.Count;
            List<uopDeckSplice> targets2 = new();
            if (Math.Round(ord, 4) != 0 && assy.DesignFamily.IsStandardDesignFamily())
            {
                targets2 = Splices.GetAtOrdinate(-ord, bSuppressManways: true, bVerticals: false, bReturnMirrors: false);
                cnt2 = targets2.Count;
            }
            if (cnt1 <= 0 && cnt2 <= 0) return;
            if (cnt1 + cnt2 == 1)
            {
                ret = Splices.DeleteSplice(aSplice.Handle, bDeleteAllAtOrdinate: false);
                FinishCommand(ret);

            }


            aQuestions.Title = $"Delete Splices At Y = {LinearUnits.UnitValueString(ord, DisplayUnits, aPrecis: DisplayUnits == uppUnitFamilies.Metric ? 1 : 4)} ?";

            string strpnls = ""; //(cnt1 + cnt2 > 1) ? "ALL" : "";

            foreach (uopDeckSplice splc in targets1)
            {
                string str1 = $"Panel {splc.PanelIndex}";
                if (!strpnls.Contains(str1))
                    mzUtils.ListAdd(ref strpnls, str1);
            }

            string defans = (cnt1 + cnt1 > 1) ? "All" : $"Panel {aSplice.PanelIndex}";
            mzQuestion q1 = aQuestions.AddMultiSelect("Select Panels:", strpnls, defans, 1);
            mzQuestion q2 = (assy.DesignFamily.IsBeamDesignFamily() || cnt2 == 0) ? null : aQuestions.AddCheckVal("Delete Mirrors?", true);

            bool bCanc = !aQuestions.PromptForAnswers("", true, owner: null, aSaveButtonText: "Delete");
            if (bCanc) return;

            _LastRun = Splices.Clone();
            string pnllist = aQuestions.Item(1).Answers.ToList();

            List<int> dpIDs = GetPanelIndices(pnllist, aQuestions.Item(1).Choices.ToList());

            if (dpIDs.Count <= 0) return;

           bool bMirs = q2 == null ? false : q2.AnswerB;

            foreach (int i in dpIDs)
            {
                List<uopDeckSplice> removers = Splices.GetAtOrdinate(ord, bSuppressManways: true, bVerticals: false, bReturnMirrors: false, aPanelIndex: i, bRemove: false);
                if (removers.Count > 0)
                {
                    Splices.RemoveMembers(removers);
                    ret = true;
                }
                    
                if (bMirs)
                {
                    removers = Splices.GetAtOrdinate(-ord, bSuppressManways: true, bVerticals: false, bReturnMirrors: false, aPanelIndex: i, bRemove: false);
                    if (removers.Count > 0)
                    {
                        Splices.RemoveMembers(removers);
                        ret = true;
                    }

                }
            }


            if (ret)
            {
                _SelectedSplice = null;
                MousePoint = null;
            }
            FinishCommand(ret);
        }

        private async Task Execute_Generate_Uniform()
        {
            if (!IsEnabled) return;
            if (WaitingForClick) ResetWaitingForPoint();

            bool ret = default;
            mdTrayAssembly assy = MDAssy;
            if (assy == null || Splices == null) return;
            double manht = Splices.ManwayHeight;

            double fldht = Splices.PanelHeight;
            uopDeckSplices aSplices;
            bool bCanc;

            mzQuestion question1;
            mzQuestion question2;
            mzQuestion question3;

            Splices.SpliceStyle = (uppSpliceStyles)SelectedSpliceStyle;


            if (_Questions_Uniform == null)
            {
                _Questions_Uniform = new mzQuestions();

                double multi = Multiplier_Linear;
                if (manht <= 0) manht = mdUtils.IdealManwayHeight(MDAssy, Splices.SpliceStyle);
                string suffix = LinearUnits.Label(DisplayUnits);
                //_Questions_Uniform.AddSingleSelect("Select Splice Style:", uopEnumHelper.GetDescriptionsList(typeof(uppSpliceStyles), SkipNegatives: true), Splices.SpliceStyle.GetDescription(), true, aInputWidthPct: "65");
                question1 = _Questions_Uniform.AddNumeric("Panel Height:", fldht, suffix, multi, aMaxValue: Radius * 0.75,aMinValue: 4, aValueControl: uopValueControls.Positive,Whole, Deci, aInputWidthPct: "25");
                question2 = _Questions_Uniform.AddNumeric("Manway Height:", manht,suffix, multi, aMaxValue: MaxManway, aMinValue: 0, aValueControl: uopValueControls.Positive, Whole, Deci, aInputWidthPct: "25");
                question3 = _Questions_Uniform.AddCheckVal("Retain Current Manways?", true);
                //question1.SetAnswer(fldht);
                //question2.SetAnswer(manht);

            }
            else
            {
                question1 = _Questions_Uniform.Item(1);
                question2 = _Questions_Uniform.Item(2);
                question3 = _Questions_Uniform.Item(3);
            }



            //_Questions_Uniform.SetAnswer(1, Splices.SpliceStyle.GetDescription());
            question1.SetFormats(Multiplier, Whole, Deci, LinearUnits.Label(DisplayUnits));
            question2.SetFormats(Multiplier, Whole, Deci, LinearUnits.Label(DisplayUnits));
            bCanc = !_Questions_Uniform.PromptForAnswers("Enter Generation Control Data Uniform Ht.", true, owner: null);

            if (bCanc) return;

            uopVectors manpts = Splices.ManwayPoints;

            _LastRun = Splices.Clone();
            //txt = _Questions_Uniform.Item(1).Answer.ToString().ToUpper().Trim();
            //astly = (uppSpliceStyles)uopEnumHelper.GetValueByDescription(typeof(uppSpliceStyles), txt);

            fldht = question1.AnswerD;
            manht = question2.AnswerD;
            if (manht < 8)
            {
                manht = mdUtils.IdealManwayHeight(assy, Splices.SpliceStyle);
            }

            aSplices = Splices.Clone();
            if (!question3.AnswerB) manpts.Clear();

            aSplices.RemoveManways(assy, false);
            aSplices.ManwayHeight = manht;

            RightStatusBarLabelText = "Generating splices using uniform strategy ...";
            var syncContext = SynchronizationContext.Current;
            var detailStatusUpdater = (string status) =>
            {
                syncContext.Post(new SendOrPostCallback((_) => { SubStatusText = status; }), null);
            };

            aSplices = await mdSplicing.GenerateSplices_UNIFORM(assy, aSpliceStyle: Splices.SpliceStyle, aPanelHeight: fldht, aOldSplices: aSplices, aManwayHeight: manht, statusUpdater: detailStatusUpdater);
            foreach (var manpt in manpts)
            {
                aSplices.AddManway(MDAssy, manpt.Handle, _FreePoints, Splices.SpliceStyle, ref manht, out int _, out string rErr);
            }


            ret = EditsMade(Splices, aSplices);
            if (ret) Splices = aSplices;
            RightStatusBarLabelText = "";
            FinishCommand(ret);

        }

        private async Task Execute_Generate_Stradle()
        {
            if (!IsEnabled) return;
            if (WaitingForClick) ResetWaitingForPoint();

            bool ret = default;
            mdTrayAssembly assy = MDAssy;
            if (assy == null) return;

            double manht = Splices.ManwayHeight;
            uopVectors manpts = Splices.ManwayPoints;
            uppSpliceStyles styl = (uppSpliceStyles)SelectedSpliceStyle;
            double fldht = mdUtils.IdealSectionHeight( assy,styl);


            Splices.SpliceStyle = styl;
            mzQuestion question1;
            mzQuestion question2;
            mzQuestion question3;

            if (_Questions_Stradle == null)
            {
                _Questions_Stradle = new mzQuestions();

                fldht =  mdUtils.IdealSectionHeight(assy,Splices.SpliceStyle);
                if (manht <= 0) manht = mdUtils.IdealManwayHeight(assy, Splices.SpliceStyle);
                question1 = _Questions_Stradle.AddNumeric("Panel Height:", fldht, LinearUnits.Label(DisplayUnits), Multiplier, Radius * 0.75, 4, uopValueControls.Positive, Whole, Deci, aInputWidthPct: "25");
                question2 = _Questions_Stradle.AddNumeric("Manway Height:", manht, LinearUnits.Label(DisplayUnits), Multiplier, MaxManway, 0, uopValueControls.Positive, Whole, Deci, aInputWidthPct: "25");
                question3 = _Questions_Stradle.AddCheckVal("Retain Current Manways?", true);
            }
            else
            {
                question1 = _Questions_Stradle.Item(1);
                question2 = _Questions_Stradle.Item(2);
                question3 = _Questions_Stradle.Item(3);

            }

            // _Questions_Stradle.SetAnswer(1, assy.IdealSectionHeight(Splices.SpliceStyle));
            //_Questions_Stradle.SetAnswer(2, manht);
            // _Questions_Stradle.SetAnswer(3, true);
            //_Questions_Stradle.SetFormats(1, Multiplier, Whole, Deci,  LinearUnits.Label(DisplayUnits));
            question1.SetAnswer(fldht);
            question1.SetFormats(Multiplier, Whole, Deci, LinearUnits.Label(DisplayUnits));
            question2.SetAnswer(manht);
            question1.SetFormats(Multiplier, Whole, Deci, LinearUnits.Label(DisplayUnits));
            bool bCanc = !_Questions_Stradle.PromptForAnswers($"Enter Generation Control Data Straddle DCs", true, owner: null);

            if (bCanc) return;
            _LastRun = new uopDeckSplices( Splices);
            //txt = _Questions_Stradle.Item(1).Answer.ToString().ToUpper().Trim();
            //astly = (uppSpliceStyles)uopEnumHelper.GetValueByDescription(typeof(uppSpliceStyles), txt);


            fldht = question1.AnswerD;
            manht = question2.AnswerD;
            if (manht < 8)
            {
                manht = mdUtils.IdealManwayHeight(assy, styl);
            }

            uopDeckSplices aSplices = new uopDeckSplices(Splices);
            if (question3.AnswerB == false) manpts.Clear();
            bool backfiil = false;
            aSplices.RemoveManways(assy, ref backfiil, 0, out List<string> mhandles);
            aSplices.ManwayHeight = manht;

            RightStatusBarLabelText = "Generating splices using straddle strategy ...";
            var syncContext = SynchronizationContext.Current;
            var detailStatusUpdater = (string status) =>
            {
                syncContext.Post(new SendOrPostCallback((_) => { SubStatusText = status; }), null);
            };

            aSplices = await mdSplicing.GenerateSplices_STRADLE(assy, aSpliceStyle: Splices.SpliceStyle, aPanelHeight: fldht, aOldSplices: aSplices, aManwayHeight: manht, statusUpdater: detailStatusUpdater);
            foreach (var manpt in manpts)
            {
                aSplices.AddManway(MDAssy, manpt.Handle, _FreePoints, Splices.SpliceStyle, ref manht, out int _, out string rErr);
            }

            ret = EditsMade(Splices, aSplices);

            if (ret) Splices = aSplices;

            RightStatusBarLabelText = "";
            FinishCommand(ret);

        }

        private void Execute_CopyFromOtherTray()
        {
            if (!IsEnabled) return;
            if (WaitingForClick) ResetWaitingForPoint();

            mdTrayAssembly assy = MDAssy;
            if (assy == null) return;


            string tname;

            if (_Questions_TRAY == null)
            {
                _Questions_TRAY = new mzQuestions();
                _Questions_TRAY.AddSingleSelect("Select a Tray:", _Tray_List, aInitialAnswer: "", bAnswerRequired: true);
            }

            if (!_Questions_TRAY.PromptForAnswers("Select a Tray To Copy Splice Locations From", true, owner: MyWindow())) return;
            tname = _Questions_TRAY.Item(1).Answer.ToString().Trim();

            colUOPTrayRanges aTrays = MDProject.TrayRanges;

            mdTrayAssembly otherassy = null;

          
            List<mdTrayRange> mdtrays = MDProject.GetMDTrays();


            mdTrayRange aTray = mdtrays.Find(x => string.Compare(x.TrayName(false), tname, true) == 0);

            if (aTray == null) return;


            _LastRun = Splices.Clone();
            otherassy = aTray.TrayAssembly;

            uopDeckSplices bSplices = otherassy.DeckSplices;
            uopDeckSplices aSplices = new uopDeckSplices(Splices);

            int maxpid = assy.PanelCount;
            Splices.Clear(false);
            uopVectors mnPts = bSplices.ManwayPoints;
            Splices.SpliceStyle = bSplices.SpliceStyle;
            SelectedSpliceStyle = (int)Splices.SpliceStyle;

            var groupByPanelIndex = bSplices.ToList().GroupBy(s => s.PanelIndex);
            foreach (var panelGroup in groupByPanelIndex)
            {
                if (panelGroup.Key > maxpid)
                {
                    continue;
                }
                mdDeckPanel panel = assy.DeckPanels.Item(panelGroup.Key);

                var panelShapes = mdDeckPanel.GetPanelShapes(panel, assy);

                foreach (var splice in panelGroup)
                {
                    if (!splice.SupportsManway)
                    {
                        if (panelGroup.Key > 1)
                        {
                            if (panelShapes.Any(ps => mdDeckPanel.IsValidPanelShapeForDeckSpliceOrdinate(ps, splice.Ordinate, mdDeckPanel.MinDistanceFromTopOrBottom)))
                            {
                                Splices.AddHorizontalSpliceMD(assy, splice.PanelIndex, splice.Ordinate);
                            }
                        }
                        else
                        {
                            if (panelShapes.Any(ps => mdDeckPanel.IsValidPanelShapeForVerticalDeckSpliceOrdinate(ps, splice.Ordinate, mdDeckPanel.MinDistanceFromTopOrBottom)))
                            {
                                if (splice.SpliceIndex == 1)
                                {
                                    Splices.AddVerticalSplices(assy);
                                }
                            }
                        }
                    }
                }
            }

            for (int i = 1; i <= mnPts.Count; i++)
            {
                Splices.AddManway(assy, bSplices.ManwayHeight, mnPts.Item(i).Handle, out string _);
            }


            Splices.ManwayHeight = bSplices.ManwayHeight;
            FinishCommand(true);

        }


        private void Execute_Clear()
        {
            if (!IsEnabled) return;
            if (WaitingForClick) ResetWaitingForPoint();

            bool ret = default;
            if (Splices.Count <= 0) return;

            mzQuestions aQuestions = new();
            int i;
            List<string> dpIDs = new();
            bool bMnys = false;

            for (i = 1; i <= Panels.Count; i++)
            {
                if (Splices.PanelSpliceCount(i) > 0) dpIDs.Add($"Panel {i}");
            }
            if (dpIDs.Count > 0)
            {
                mzQuestion question2 = null;

                mzQuestion question1 = aQuestions.AddMultiSelect("Select Panels To Clear:", mzUtils.ListToString(dpIDs), "All", 1);
                if (Splices.ManwayCount() > 0) question2 = aQuestions.AddCheckVal("Retain Manways?:", true);
                double manht = Splices.ManwayHeight;

                if (!aQuestions.PromptForAnswers("Clear Splices", true, owner: MyWindow(), aSaveButtonText: "Apply")) return;


                _LastRun = Splices.Clone();
                string pnllist = aQuestions.Item(1).Answers.ToList();
                List<int> dpIDXs = GetPanelIndices(pnllist, question1.Choices.ToList());

                if (dpIDXs.Count <= 0) return;
                uopVectors manpts = question2 == null ? uopVectors.Zero : question2.AnswerB == false ?  uopVectors.Zero : Splices.ManwayPoints;

                if (question2 != null) bMnys = !question2.AnswerB;

                Splices.RemoveManways(MDAssy, false);

                for (i = 1; i <= dpIDXs.Count; i++)
                {
                    if (Splices.GetByPanelIndex(dpIDXs[i - 1], true, bRemove: true).Count > 0) ret = true;
                }


                foreach (var item in manpts)
                {
                    Splices.AddManway(MDAssy, item.Handle, _FreePoints, Splices.SpliceStyle, ref manht, out int _, out string rErr);
                }
                _ManPts = Splices.ManwayPoints;
                _Shapes = null;

                //dpIDs.Insert(0,"All");
            }
            else
            {
                ret = true;
                Splices.Clear();
            }

            if (ret)
            {
                _SelectedSplice = null;
                MousePoint = null;
                _Shapes = null;
            }
            FinishCommand(ret);

        }


        private void Execute_Cancel()
        {

            if (WaitingForClick)
            {
                ResetWaitingForPoint();
                return;
            }
            if (MDAssy != null)
            {
                if (EditsMade(MDAssy.DeckSplices, Splices))
                {
                    MessageBoxResult reply = System.Windows.MessageBox.Show("Exit Without Saving Changes?", "Unsaved Changes Detected", MessageBoxButton.YesNo);
                    if (reply == MessageBoxResult.No)
                        return;
                }

            }

            Canceled = true;
            DialogResult = false;

        }


        private async void Execute_Save()
        {
            if (!IsEnabled) return;
            if (MDAssy == null) { Execute_Cancel(); return; }
            Canceled = false;
            if (WaitingForClick)
            {
                ResetWaitingForPoint();
                return;
            }
            uopDeckSplices save = Splices.Clone();
            Edited = EditsMade(MDAssy.DeckSplices, save);
            //mdProject proj = MDProject;
            //mdTrayAssembly assy = MDAssy;
            mdTrayRange range = MDRange;
            if (Edited)
            {

                RefreshMessage = new Message_Refresh(bSuppressPropertyLists: false)
                {
                    PartType = uppPartTypes.DeckSplices,
                    SuppressDataGrids = false,
                    DirtyRange = MDRange.GUID,
                    Range = MDRange,
                    SuppressImage = false,
                    Parts = save.OfType<uopPart>().ToList(),
                    HideEditUI = Hidden,
                    Properties = new uopProperties(new uopProperty("Style", (int)Splices.SpliceStyle)),
                   
                };
                RefreshMessage.PartTypeList.Add(uppPartTypes.DesignOptions);
                await Task.Run(() =>
                {
                    //Message_Refresh RefreshMessage = RefreshMessage;
                    try
                    {


                        mdProject proj = MDProject;
                        mdTrayRange range = MDRange;
                        mdTrayAssembly assy = range?.TrayAssembly ?? null;
                        if (assy != null)
                        {
                            SuppressEvents = false;
                            IsEnabled = false;
                            assy.eventStatusChange -= AssyStatusChangeHandler;
                            if (!Hidden)
                            {

                                ShowProgress = true;
                                RightStatusBarLabelText = "Saving Splices ...";
                                assy.eventStatusChange += AssyStatusChangeHandler;
                            }
                            else
                            {
                                RefreshMessage.ToggleAppEnabled(false, "Saving Splices ...");
                            }
                            //SuppressEvents = true;

                            assy.Invalidate(uppPartTypes.DeckSection);
                          
                            uppSpliceStyles style = Splices.SpliceStyle;
                            assy.DesignOptions.PropValSet("SpliceStyle", style, bSuppressEvnts: true);
                   
                            assy.SetDeckSplices(Splices, false);
                            if (!Hidden)
                            {
                                SuppressEvents = false;
                                RightStatusBarLabelText = $"Updating {assy.TrayName()} Deck Sections ...";
                                SuppressEvents = true;
                            }
                            else
                            {
                                RefreshMessage.ToggleAppEnabled(false, $"Updating{assy.TrayName()} Deck Sections ...");
                            }
                            assy.UpdatePersistentSubParts(proj, range);
                        }
                        assy.eventStatusChange -= AssyStatusChangeHandler;


                    }
                    catch (Exception ex)
                    {

                        RefreshMessage.ErrorMessage = ex.Message;
                    }
                });
                
                try
                {

                    //RefreshMessage.ToggleAppEnabled(bEnabledVal: true);
                    RefreshMessage.DirtyRange = "";
                    RefreshMessage.Parts = null;
                    RefreshMessage.Properties = null;
                }
                catch { }
                finally
                {

                    ResetWaitingForPoint();
                    SuppressEvents = false;
                    if (RefreshMessage.HideEditUI)
                    {
                        RefreshMessage.ToggleAppEnabled(true);
                    }
                    RightStatusBarLabelText = "";
                    IsEnabled = true;
                    ShowProgress = false;
                    RefreshMessage.PartType = uppPartTypes.DeckSplices;
                    RefreshMessage = RefreshMessage;
                    MDProject.HasChanged = true;
                    MDAssy.eventStatusChange -= AssyStatusChangeHandler;
                    Hidden = false;
                    Edited = true;
                    Canceled = false;
                    DialogResult = true;
                    SuppressEvents = true;

                    MDProjectViewModel vm = ParentVM != null ? (MDProjectViewModel)ParentVM : RefreshMessage.MainVM.MDProjectViewModel;
                    if (vm != null) vm.RespondToEdits(this, true, uppPartTypes.DeckSplices);
                }


            }
            else
            {
                Canceled = true;
                DialogResult = false;
            }

        }



        public void EditSplices()
        {
            if (MDProject == null || MDRange == null) return;
            mdTrayAssembly assy = MDAssy;
            if (assy == null) return;

            // Activated = false;

            try
            {
                Splices = new uopDeckSplices(assy.DeckSplices);

                //uopProperties p1 = assy.DeckSplices.CurrentProperties(assy);
                //uopProperties p2 = Splices.CurrentProperties(assy);


                Splices.SpliceStyle = assy.DesignOptions.SpliceStyle;



                _Tray_List = new List<string>();

                ManCnt = (assy != null) ? assy.Deck.ManwayCount : 0;

                _ManPts = Splices?.ManwayPoints?.Clone();

                mdTrayAssembly otherassy;

                for (int i = 1; i <= MDProject.TrayRanges.Count; i++)
                {
                    otherassy = MDProject.TrayRanges.Item(i).GetMDTrayAssembly();
                    if (string.Compare(assy.RangeGUID, otherassy.RangeGUID, true) != 0)
                    {
                        if (otherassy.DeckSplices.Count > 0)
                        {
                            _Tray_List.Add(otherassy.TrayName(false));
                        }
                    }
                }


                ManholeID = MDRange.ManholeID;
                //show tray parent properties in status bar
                LeftStatusBarLabelText = $"{MDProject.ProjectName} ({MDRange.Name(true)})";

                _DisplayRectangle = new dxfRectangle();
                mdDeckPanel aDP = assy.DeckPanels.LastPanel();
                _DisplayRectangle.Height = aDP.Radius * 2;

                _DisplayRectangle.Width = aDP.Radius - aDP.Left();
                _DisplayRectangle.X = aDP.Left() + 0.5 * _DisplayRectangle.Width;

                IsEnglishSelected = MDProject.DisplayUnits == uppUnitFamilies.English;

                MaxManway = assy.Downcomer().BoxWidth * 2;
                PanelWidth = assy.DowncomerSpacing - MaxManway / 2;

                MaxManway += PanelWidth;

            }
            finally
            {
                //Activated = true;
                //Execute_Refresh();
            }




        }

        private void ClearGrids(bool HideLines = true)
        {

            SplicesDataGridItems = new ObservableCollection<SplicesDataGridViewModel>();
            DeckSectionsDataGridItems = new ObservableCollection<DeckSectionDataGridViewModel>();


        }

        private bool EditsMade(uopDeckSplices aSplices, uopDeckSplices bSplices = null)
        {
            bool ret = default;

            if (MDAssy == null) return false;
            if (bSplices == null)
            {
                ret = aSplices.SpliceStyle != MDAssy.DesignOptions.SpliceStyle;
                if (MDAssy.DesignOptions.HasTiledDecks != aSplices.HasHorizontalMembers(false)) ret = true;
                if (!ret)
                {
                    if (!aSplices.IsEqual(MDAssy.DeckSplices, MDAssy)) ret = true;
                }
            }
            else
            {
                ret = aSplices.SpliceStyle != bSplices.SpliceStyle;
                if (bSplices.HasHorizontalMembers(false) != aSplices.HasHorizontalMembers(false)) ret = true;
                if (!ret)
                {
                    if (!aSplices.IsEqual(bSplices, MDAssy)) ret = true;
                }
            }
            if (ret) _Shapes = null;
            return ret;
        }

        private void SectionsShow()
        {
            ObservableCollection<DeckSectionDataGridViewModel> grid = new ObservableCollection<DeckSectionDataGridViewModel>();
           _Shapes ??= MDAssy.DowncomerData.CreateSectionShapes(Splices);
            _ManPts = Splices.ManwayPoints;

            for (int i = 1; i <= _Shapes.Count; i++)
            {
                uopSectionShape aShape = _Shapes[i - 1];
                grid.Add(new DeckSectionDataGridViewModel()
                {
                    Section = aShape.Name,
                    ManholeFitWidth = LinearUnits.UnitValueString(aShape.FitWidth, DisplayUnits, bAddLabel: false),
                    Depth = LinearUnits.UnitValueString(aShape.Depth, DisplayUnits, bAddLabel: false),
                    BaseHeight = LinearUnits.UnitValueString(aShape.BaseHeight, DisplayUnits, bAddLabel: false)
                });
            }
            DeckSectionsDataGridItems = grid;
        }

        private void SplicesShow()
        {
            SplicesDataGridItems = new ObservableCollection<SplicesDataGridViewModel>();
            _SelectedSpliceStyle = (int)Splices.SpliceStyle;
            NotifyPropertyChanged("SelectedSpliceStyle");
            uopDeckSplice aSplice;

            for (int j = 1; j <= Panels.Count; j++)
            {
                var pSplices = Splices.GetByPanelIndex(j, true);
                for (int i = 1; i <= pSplices.Count; i++)
                {
                    aSplice = pSplices[i - 1];
                    SplicesDataGridItems.Add(new SplicesDataGridViewModel()
                    {
                        SpliceID = aSplice.Handle,
                        SpliceType = aSplice.SpliceTypeName(),
                        MOrF = aSplice.MFTag,
                        Ordinate = LinearUnits.UnitValueString(aSplice.Ordinate, DisplayUnits, bAddLabel: false),
                        ManwayID = aSplice.SupportsManway ? $"{aSplice.ManwayHandle}({aSplice.ManTag.Substring(0, 1)})" : "",
                    });
                }
            }
        }

        private void InfoShow()
        {
            mdTrayAssembly assy = MDAssy;
            if (assy == null) return;
            uopUnit lunit = LinearUnits;

            ManholeIDText = $"Manhole ID = {lunit.UnitValueString(ManholeID, DisplayUnits, aPrecis: DisplayUnits == uppUnitFamilies.Metric ? 1 : 4)}";
            PanelFitLimitText = $"Panel Fit Limit = {lunit.UnitValueString(ManholeID - 0.5, DisplayUnits, aPrecis: DisplayUnits == uppUnitFamilies.Metric ? 1 : 4)}";
            IdealManwayHeightText = $"Ideal Manway Ht. = {lunit.UnitValueString(mdUtils.IdealManwayHeight(assy, Splices.SpliceStyle), DisplayUnits, aPrecis: 4)}";
            ManwayHeightText = $"Manway Height = {lunit.UnitValueString(Splices.ManwayHeight, DisplayUnits, aPrecis: DisplayUnits == uppUnitFamilies.Metric ? 1 : 4)} ";
            DcSpaceText = $"DC Space = {lunit.UnitValueString(assy.DowncomerSpacing, DisplayUnits, aPrecis: DisplayUnits == uppUnitFamilies.Metric ? 1 : 4)}";
            PanelWidthText = $"Panel Width = {lunit.UnitValueString(PanelWidth, DisplayUnits)}";

            if (Splices.ManwayHeight < 12)
                ManwayHeightForegroundColor = new SolidColorBrush(Colors.Red);
            else
                ManwayHeightForegroundColor = new SolidColorBrush(Colors.Black);

            var mcnt = Splices.ManwayCount(true);
            if (mcnt != ManCnt)
            {
                ManwayCountText = $"Manways: {mcnt} of {ManCnt}";
                ManwayCountForegroundColor = new SolidColorBrush(Colors.Red);
            }
            else
            {
                ManwayCountText = $"Manways: {mcnt}";
                ManwayCountForegroundColor = new SolidColorBrush(Colors.Black);
            }

            var aSplice = Splices.GetSelected();
            ImageOverlayLocationLabelText = "";
            if (aSplice == null) return;

            if (!aSplice.Vertical)
                ImageOverlayLocationLabelText = $"Y={lunit.UnitValueString(aSplice.Ordinate, DisplayUnits, aPrecis: DisplayUnits == uppUnitFamilies.Metric ? 1 : 4)}";
            else
                ImageOverlayLocationLabelText = $"X={lunit.UnitValueString(aSplice.Ordinate, DisplayUnits, aPrecis: DisplayUnits == uppUnitFamilies.Metric ? 1 : 4)}";

            //OnPropertyChanged();
        }

        private void FreePointsSet()
        {
            _FreePoints = MDAssy.DowncomerData.FreeCenters();
        }

        public override void ReleaseReferences()
        {
            try
            {

                Viewer = null; // this needs to be first;

                base.ReleaseReferences();

                _Splices = null;
                _Sisters = null;
                _SelectedSplice = null;
                _FreePoints = null;
                _ManPts = null;
                MousePoint = null;
                _LastRun = null;
                _DisplayRectangle = null;
                _Panels = null;
                _Runs = null;
                _Redos = null;

                _CMD_Add = null;
                _CMD_Cancel = null;
                _CMD_Clear = null;
                _CMD_Copy_Tray = null;
                _CMD_Delete = null;
                _CMD_Generate_Stradle = null;
                _CMD_Generate_Uniform = null;
                _CMD_Manways_Remove = null;
                _CMD_Manways_Resize = null;
                _CMD_Manway_Add = null;
                _CMD_Manway_Remove = null;
                _CMD_Moon_Split = null;
                _CMD_Redo = null;
                _CMD_Refresh = null;
                _CMD_Save = null;
                _CMD_Reset = null;
                _CMD_Undo = null;



            }
            catch (Exception) { }
        }

        private void Execute_RefreshData()
        {
            
            SectionsShow();
            SplicesShow();
            InfoShow();
            HighlightSplices();
        }

        private async void Execute_Refresh(bool bDataOnly = false)
        {
            if (!IsEnabled) return;
            if (WaitingForClick) ResetWaitingForPoint();

            mdTrayAssembly assy = MDAssy;

            if (assy == null) return;
            if (bDataOnly)
            {
                Execute_RefreshData();
                return;
            }

            _Shapes = null;
            FreePointsSet();
            _ManPts = Splices?.ManwayPoints?.Clone();

            Splices?.Verify(assy);
            _SelectedSpliceStyle = (int)Splices.SpliceStyle;
            ClearGrids();

            RightStatusBarLabelText = "Generating Splice Drawing...";
            if (Image != null) Image.Dispose();
            Image = null;
            Viewer?.Clear();
            Image = new dxfImage(SketchColor);

            Message_Refresh request = new()
            {
                Image = Image,
                Viewer = Viewer,
                StatusMessage = "Generating Splice Drawing..."
            };
            await Execute_DrawInputAsync(request);
            Execute_DrawInput_Complete(request);



        }


        private void SaveRun(uopDeckSplices aRun, bool bRemoveLast = false)
        {
            if (_Runs != null)
            {
                if (bRemoveLast && _Runs != null) _Runs.RemoveAt(_Runs.Count - 1);

                if (aRun != null)
                {
                    if (_Runs.IndexOf(aRun) >= 0)
                        return;

                    if (_Runs.Count > 40)
                    {
                        _Runs.RemoveAt(0);
                    }
                    _Runs.Add(aRun.Clone());
                }
            }



            IsEnabled_Undo = _Runs != null && _Runs.Count > 0;


        }

        private List<int> GetPanelIndices(string aPanelList, string aChoiceList)
        {
            List<int> _rVal = new();
            if (string.IsNullOrWhiteSpace(aPanelList)) return _rVal;

            List<string> lstvals = mzUtils.StringsFromDelimitedList(aPanelList);
            List<string> lstchoices = mzUtils.StringsFromDelimitedList(aChoiceList);
            lstchoices.Remove("All");
            foreach (string lval in lstvals)
            {

                if (string.Compare(lval, "All", ignoreCase: true) == 0)
                {
                    _rVal.Clear();
                    foreach (string cval in lstchoices)
                    {

                        _rVal.Add(mzUtils.TrailingInteger(cval));
                    }
                    return _rVal;
                }
                else
                {
                    _rVal.Add(mzUtils.TrailingInteger(lval));

                }

            }


            return _rVal;
        }

        private List<int> DPIndexes(mzValues aDPList)
        {
            var ret = new List<int>();
            if (aDPList == null) return ret;

            string aStr;
            int j;
            int dpidx;
            for (int i = 1; i <= aDPList.Count; i++)
            {
                aStr = aDPList.Item(i);
                j = aStr.LastIndexOf(" ");
                aStr = aStr.Substring(j + 1).Trim();
                if (int.TryParse(aStr, out dpidx))
                {
                    if (dpidx > 0 && dpidx <= Panels.Count)
                    {
                        ret.Add(dpidx);
                    }
                }
            }

            return ret;
        }

     



        #endregion Methods

        #region form and mouse control


        public override void Activate(Window myWindow)
        {
            if (Activated) return;
            SuppressEvents = false;
            base.Activate(myWindow);
            Activated = true;
            if (myWindow != null)
                myWindow.Top = 20;
            Execute_Refresh();
            // EditSplices();

        }

        public void FormLoad(UOP.DXFGraphicsControl.DXFViewer aViewer)
        {
            Canceled = true;
            RightStatusBarLabelText = $"Initializing...";
            Viewer = aViewer;
            RightStatusBarLabelText = "";

        }

        public void Viewer_MouseDoubleClick(object sender, System.Windows.Input.MouseEventArgs e)
        {

            if (MDAssy == null || Image == null || !IsEnabled || WaitingForClick || _SelectedSplice == null) return;
            e.Handled = true;
            Execute_SetOrd(_SelectedSplice);
        }

        public void Viewer_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            mdTrayAssembly assy = MDAssy;
            if (assy == null || Image == null || !IsEnabled) return;

            var position = e.GetPosition((System.Windows.IInputElement)sender);

            string coords;
            double xval = 0;
            double yval = 0;
            double multi;

            multi = DisplayUnits == uppUnitFamilies.English ? 1 : 25.4;

            //xval = picPanel.ScaleX(X, picPanel.ScaleMode, vbPixels); // I don't know what this should do
            //yval = picPanel.ScaleX(Y, picPanel.ScaleMode, vbPixels// I don't know what this should do
            //DXFImage.Display.DeviceToWorld(xval, yval, , True); // The last argument's type does not match the signature
            (xval, yval) = Viewer.DeviceToWorld(position.X, position.Y);

            coords = $"{string.Format("{0:0.000}", xval * multi)},{string.Format("{0:0.000}", yval * multi)}";
            ImageOverlayLabelText = coords;

            if (WaitingForClick)
            {
                switch (FinishWaitingForClick)
                {
                    case "ADDSPLICE":
                        {
                            ulong hnd = Viewer.overlayHandler.DrawHorzLine(new dxfVector(0, yval, 0), assy.GetMDRange().ShellID * 2, dxxColors.Red, 20, "CENTER", CursorHandles.Count > 0 ? CursorHandles[0] : 0);
                            if (!CursorHandles.Contains(hnd))
                                CursorHandles.Add(hnd);
                            hnd = 0;
                            (xval, yval) = Viewer.DeviceToWorld(0, position.Y);

                            hnd = Viewer.overlayHandler.DrawText(string.Format("{0:0.000}", yval * multi), new dxfVector(xval + 2, yval - 2, 0), 2, CursorHandles.Count > 1 ? CursorHandles[1] : 0);
                            if (!CursorHandles.Contains(hnd))
                                CursorHandles.Add(hnd);
                            break;
                        }
                    case "MANADD":
                        {
                            uopVector v1 = new(xval, yval, 0);
                            var clr = dxxColors.LightGreen;

                            v1 = _FreePoints.Nearest(v1);
                            bool manExists = Splices.GetByManwayHandle(v1.Handle, false, true).Count > 0;
                            if (manExists) clr = dxxColors.Red;
                            ulong hnd = Viewer.overlayHandler.DrawFilledCircle(v1, 7, clr, CursorHandles.Count > 0 ? CursorHandles[0] : 0);
                            CursorHandles.Clear();
                            CursorHandles.Add(hnd);
                            break;
                        }
                    case "MANREMOVE":
                        {
                            uopVector v1 = _FreePoints.Nearest(new uopVector(xval, yval));
                            bool manExists = Splices.GetByManwayHandle(v1.Handle, false, true).Count > 0;
                            if (manExists)
                            {
                                ulong hnd = Viewer.overlayHandler.DrawFilledCircle(v1, 7, dxxColors.Red, CursorHandles.Count > 0 ? CursorHandles[0] : 0);
                                CursorHandles.Clear();
                                CursorHandles.Add(hnd);
                            }
                            else
                            {
                                Viewer.overlayHandler.RemoveByHandle(CursorHandles);
                                CursorHandles.Clear();
                            }
                            break;
                        }
                }
            }
        }


        private void Viewer_OnViewerMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var position = e.GetPosition((System.Windows.IInputElement)sender);
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed && e.ClickCount == 1)
            {
                LeftMouseButtonEventProcessor(System.Windows.Forms.MouseButtons.Left, (int)position.X, (int)position.Y);
            }
            else
            {
                if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed && e.ClickCount == 2)
                {
                    LefMouseDoubleClickEventProcessor((int)position.X, (int)position.Y);
                }
            }
        }

        public void Viewer_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            uopDeckSplice selected = (uopDeckSplice)Splices.SelectedMember;
            if (selected == null || e == null) return;
            if (e.Key == Key.Delete) Execute_Delete(selected);
        }

        private void HighlightSplices(string aSpliceToHightlight = "")
        {

            _Sisters = new List<uopDeckSplice>();
            if (_SplicePerims == null || Splices == null) return;



            dxfRectangle rec = null;
            var selectedSplices = new List<string>();
            var selectedSisterSplices = new List<string>();
            var manwaySplices = new List<string>();
            if (!string.IsNullOrWhiteSpace(aSpliceToHightlight)) rec = _SplicePerims.GetTagged(aSpliceToHightlight);

            if (MousePoint != null && rec == null) rec = _SplicePerims.GetByNearestCenter(MousePoint);
            if (rec == null) rec = _SplicePerims.Item(_SplicePerims.Count);


            _SelectedSplice = (rec != null) ? Splices.GetByHandle(rec.Tag) : null;

            if (_SelectedSplice == null && Splices.Count > 0) _SelectedSplice = Splices.Item(1);
            if (_SelectedSplice == null)
            {
                Splices.SelectedMember = _SelectedSplice;
                Viewer.overlayHandler.ClearRectangles();
                ImageOverlayLocationLabelText = "";
                for (int i = 1; i <= _SplicePerims.Count; i++)
                {
                    _SplicePerims.Item(i).Color = dxxColors.BlackWhite;
                }
                //appApplication.DoEvents();
                return;
            }
            else
            {
                int selectedSpliceIndex = Splices.IndexOf(_SelectedSplice);
                Splices.SetSelected(selectedSpliceIndex);
                if (string.Compare(LastHighlight, _SelectedSplice.Handle, ignoreCase: true) == 0) return;
                LastHighlight = _SelectedSplice.Handle;
                Viewer.overlayHandler.ClearRectangles();
                ImageOverlayLocationLabelText = "";
                for (int i = 1; i <= _SplicePerims.Count; i++)
                {
                    _SplicePerims.Item(i).Color = dxxColors.BlackWhite;
                }
                //appApplication.DoEvents();


            }


            _Sisters = Splices.GetRelatives(_SelectedSplice, bReturnParent: false, bReturnMirrors: MDAssy.DesignFamily.IsStandardDesignFamily());
            selectedSplices.Add(_SelectedSplice.Handle);

            foreach (uopDeckSplice splc in _Sisters)  selectedSisterSplices.Add(splc.Handle); 


            if (!_SelectedSplice.Vertical)
                ImageOverlayLocationLabelText = $"Y={LinearUnits.UnitValueString(_SelectedSplice.Ordinate, DisplayUnits)}";
            else
                ImageOverlayLocationLabelText = $"X={LinearUnits.UnitValueString(_SelectedSplice.Ordinate, DisplayUnits)}";




            for (int i = 1; i <= _SplicePerims.Count; i++)
            {

                rec = _SplicePerims.Item(i).Clone();
                rec.ScaleDimensions(0.95, 0.95);

                dxxColors clr = dxxColors.BlackWhite;
                if (selectedSplices.Contains(rec.Tag))
                {
                    clr = dxxColors.Yellow;
                    Viewer.overlayHandler.DrawRectangle(rec, clr, 70, 0, true, 0);
                }
                else if (selectedSisterSplices.Contains(rec.Tag))
                {
                    clr = dxxColors.LightGreen;
                    Viewer.overlayHandler.DrawRectangle(rec, clr, 70, 0, true, 0);
                }



            }
        }

        private void ResetWaitingForPoint()
        {
            WaitingForClick = false;
            FinishedWaitingForClick = true;
            FinishWaitingForClick = "";
            if (Viewer != null) Viewer.overlayHandler.RemoveByHandle(CursorHandles);

            CursorHandles.Clear();
            if (Viewer != null) Viewer.Cursor = System.Windows.Input.Cursors.Arrow;
        }



        #endregion form and mouse control

        internal void Window_Closing(object sender, CancelEventArgs e)
        {
            if (WaitingForClick)
            {
                ResetWaitingForPoint();
                e.Cancel = true;
                return;
            }
            ReleaseReferences();

        }

        private bool IsValidSplicerOrdinate(double ordinate, int panelIndex, double minDistance, mdTrayAssembly assembly)
        {
            if (assembly == null) return false;
            if(panelIndex <1 || panelIndex > assembly.DeckPanels.Count) return false;

            var panelShapes = mdDeckPanel.GetPanelShapes(assembly.DeckPanels.Item(panelIndex) ,assembly);

            // For these shapes, the ordinate is between min Y and max Y of the shape
            var containingPanelShapes = panelShapes.Where(ps => mdDeckPanel.IsValidPanelShapeForDeckSpliceOrdinate(ps, ordinate, minDistance)).ToArray();
            if (containingPanelShapes.Length != 1)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }


    public class SplicesDataGridViewModel : BindingObject, INotifyPropertyChanged
    {
        #region INotifyPropertyChagned Implementation

        public override event PropertyChangedEventHandler PropertyChanged;

        public override bool NotifyPropertyChanged(System.Reflection.MethodBase aMethod, bool? bNoErrors = null)
        {
            // Verify if the property is valid
            if (aMethod == null) return false;
            string propname = aMethod.Name.Replace("set_", "");
            try
            {
                if (!bNoErrors.HasValue) bNoErrors = !uopUtils.RunningInIDE;
                if (VerifyPropertyName(propname, bNoErrors.Value))
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
                    //OnPropertyChanged(propname);
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch { return false; }


        }
        #endregion  INotifyPropertyChagned Implementation


        #region Notifying Properties

        private string _SpliceID;
        public string SpliceID
        {
            get => _SpliceID;
            set { _SpliceID = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); }
        }

        private string _SpliceType;
        public string SpliceType
        {
            get => _SpliceType;
            set { _SpliceType = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); }
        }

        private string _MOrF;
        public string MOrF
        {
            get => _MOrF;
            set { _MOrF = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); }
        }

        private string _Ordinate;
        public string Ordinate
        {
            get => _Ordinate;
            set { _Ordinate = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); }
        }

        private string _ManwayID;
        public string ManwayID
        {
            get => _ManwayID;
            set { _ManwayID = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); }
        }

        #endregion Notifying Properties
    }

    public class DeckSectionDataGridViewModel : BindingObject, INotifyPropertyChanged
    {
        #region INotifyPropertyChagned Implementation

        public override event PropertyChangedEventHandler PropertyChanged;

        public override bool NotifyPropertyChanged(System.Reflection.MethodBase aMethod, bool? bNoErrors = null)
        {
            // Verify if the property is valid
            if (aMethod == null) return false;
            string propname = aMethod.Name.Replace("set_", "");
            try
            {
                if (!bNoErrors.HasValue) bNoErrors = !uopUtils.RunningInIDE;
                if (VerifyPropertyName(propname, bNoErrors.Value))
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
                    //OnPropertyChanged(propname);
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch { return false; }


        }
        #endregion INotifyPropertyChagned Implementation


        #region Notifying Properties

        private string _Section;
        public string Section
        {
            get => _Section;
            set { _Section = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); }
        }

        private string _ManholeFitWidth;
        public string ManholeFitWidth
        {
            get => _ManholeFitWidth;
            set { _ManholeFitWidth = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); }
        }

        private string _Depth;
        public string Depth
        {
            get => _Depth;
            set { _Depth = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); }
        }

        private string _BaseHeight;
        public string BaseHeight
        {
            get => _BaseHeight;
            set { _BaseHeight = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); }
        }

        #endregion INotifyPropertyChagned Implementation
    }

}
