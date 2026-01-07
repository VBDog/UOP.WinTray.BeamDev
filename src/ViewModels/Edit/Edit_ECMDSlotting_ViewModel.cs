using DocumentFormat.OpenXml.Wordprocessing;
using MvvmDialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using UOP.DXFGraphics;
using UOP.DXFGraphics.Utilities;
using UOP.DXFGraphicsControl;
using UOP.WinTray.Projects;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Parts;

using UOP.WinTray.Projects.Tables;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;
using UOP.WinTray.UI.BusinessLogic;
using UOP.WinTray.UI.Commands;
using UOP.WinTray.UI.Messages;
using UOP.WinTray.UI.Views.Windows;
using UOP.WinTray.UI.Views.Windows.CADfx.Questions;

namespace UOP.WinTray.UI.ViewModels
{
    public class Edit_ECMDSlotting_ViewModel : MDProjectViewModelBase, IModalDialogViewModel
    {

        #region Fields

        private BackgroundWorker _Worker_Estimate;
        private SynchronizationContext _SyncCtx;
        readonly SolidColorBrush _Brush_Red = new(Colors.Red);
        readonly SolidColorBrush _Brush_Black = new(Colors.Black);
        readonly SolidColorBrush _Brush_GreyCustom = new SolidColorBrush(System.Windows.Media.Color.FromArgb((byte)0x80, 0x0, 0x0, 0x05)); // This was in VB code but not used. So we have not bound it to the UI
        readonly SolidColorBrush _Brush_Highlight = new SolidColorBrush(System.Windows.Media.Color.FromArgb(appApplication.HighlightColor.A, appApplication.HighlightColor.R, appApplication.HighlightColor.G, appApplication.HighlightColor.B)); // The value of A is 0 which mean totally transparent!!!
        private UOP.WinTray.UI.ViewModels.SuppressTracker _SuppressTracker;


        #endregion Fields

        private bool _Loading;

        #region Constructors
        public Edit_ECMDSlotting_ViewModel(mdProject aProject, mdTrayRange aRange, MDProjectViewModelBase aParentVM, uppUnitFamilies aDisplayUnits) : base()
        {
            _Loading = true;
            try
            {
                InitUIComponents();


                MDProject = aProject;
                if (MDProject == null) return;
                MDRange = aRange == null ? MDProject.SelectedRange : aRange;
                if (MDRange == null) return;

                if (aParentVM != null)
                {
                    Width = aParentVM.Width;
                    Height = aParentVM.Height;
                }
                Title = $"Edit {MDProject.Name} - {MDRange.Name(false)} Slot Configuration";
                AreaUnits = uopUnits.GetUnit(uppUnitTypes.SmallArea);
                LinearUnits = uopUnits.GetUnit(uppUnitTypes.SmallLength);

                DisplayUnits = aDisplayUnits != uppUnitFamilies.English && aDisplayUnits != uppUnitFamilies.Metric ? MDProject.DisplayUnits : aDisplayUnits;
                LeftStatusBarLabelText = $"{MDProject.ProjectName} ({MDRange.Name(true)})";
                SaveButtonEnabled = !MDProject.Locked;

              
                InitializeView(true);
              }
            catch { }
            finally
            {
                _Loading = false;   
            }
     
        }



        private void InitUIComponents()
        {
            SlotType = new ObservableCollection<string>();
            Target = new ObservableCollection<string>();

            _WindowEnabled = true;
            ControlsPanelEnabled = true;
            SlotGenerationPanelEnabled = true;

            RetainSuppressedLocationChecked = true;


            RedrawButtonEnabled = true;

            XPitchTextBoxFocused = false;
            YPitchTextBoxFocused = false;
        }
        #endregion Constructors


        #region INotifyPropertyChanged Implementation
        public override event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion INotifyPropertyChanged Implementation

        public override UOP.DXFGraphicsControl.DXFViewer Viewer
        {
            get => base.Viewer;
            set
            {
                if (base.Viewer != null)
                {
                    base.Viewer.MouseMove -= Viewer_MouseMove;
                    base.Viewer.MouseDown -= Viewer_MouseDown;
                    base.Viewer.MouseDoubleClick -= Viewer_MouseDoubleClick;

                }
                base.Viewer = value;

                if (base.Viewer != null)
                {
                    base.Viewer.MouseMove += Viewer_MouseMove;
                    base.Viewer.MouseDown += Viewer_MouseDown;
                    base.Viewer.MouseDoubleClick += Viewer_MouseDoubleClick;

                }
            }

        }

        private string _LeftStatusBarLabelText;
        public string LeftStatusBarLabelText
        {
            get => _LeftStatusBarLabelText;
            set { _LeftStatusBarLabelText = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); }
        }



        public override string StatusText
        {
            get => base.StatusText;
            set { base.StatusText = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); }
        }

        private string _SubStatusText;
        public string SubStatusText
        {
            get => _SubStatusText;
            set { _SubStatusText = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); }
        }

        private bool? _DialogResult;
        public bool? DialogResult
        {
            get => _DialogResult;
            private set { _DialogResult = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); }
        }

        private colMDDeckPanels _Panels;
        public colMDDeckPanels Panels
        {
            get
            {
                if (_Panels != null) return _Panels;
                if (MDAssy == null) return new colMDDeckPanels();
                _Panels = new colMDDeckPanels();
                _Panels.SubPart(MDAssy);
                List<mdDeckPanel> panels = MDAssy.DeckPanels.ActivePanels(MDAssy);


                foreach (var item in panels)
                {
                    _Panels.Add(item);
                }

                return _Panels;
            }
        }


        public bool Loading { get; set; }

        public int CurrentCount { get; set; }
        public int OriginalCount { get; set; }


        private double MaxX { get; set; }
        private double MaxY { get; set; }
        private double FunctionalActiveArea { get; set; }

        private uopUnit LinearUnits { get; set; }
        private uopUnit AreaUnits { get; set; }



      
     


        #region Ordinary Properties



        private colDXFVectors SuppressionPts { get; set; }

        private mdSlotZones LastRun { get; set; }
        private mdSlotZones OriginalZones { get; set; }
        mdSlotZones _CurrentZones;
        private mdSlotZones CurrentZones
        {
            get => _CurrentZones;
            set => _CurrentZones = value;
        }
        private dxfRectangle ZoomBox { get; set; }

        /// <summary>
        ///the units to display in th view
        ////// </summary>
        private uppUnitFamilies _DisplayUnits = uppUnitFamilies.Undefined;
        public override uppUnitFamilies DisplayUnits
        {
            get
            {
                if (_DisplayUnits == uppUnitFamilies.Undefined)
                { _DisplayUnits = uppUnitFamilies.English; UnitChange(); }
                return _DisplayUnits;
            }
            set
            {
                bool newval = value != _DisplayUnits;
                _DisplayUnits = value;

                if (newval) UnitChange();
                OnPropertyChanged();
            }
        }

        private bool _IsEnglishSelected = true;
        public override bool IsEnglishSelected
        {
            get => _IsEnglishSelected;
            set { DisplayUnits = (!value) ? uppUnitFamilies.Metric : uppUnitFamilies.English; }
        }


        public override dxfImage Image
        {
            get => base.Image;
            set
            {

                if (base.Image != null && value != null)
                {
                    base.Image.Dispose();
                    Viewer?.SetImage(null);

                }

                if (value != null)
                {
                    if (value != base.Image && Viewer != null)
                    {
                        if (value.GUID != Viewer.CADModels.GUID)
                        {
                            Viewer.Clear();
                            Viewer.SetImage(value, DisableZoom: false, DisablePan: false);
                            Viewer.EnableRightMouseZoom = true;

                        }


                    }


                }
                base.Image = value;
            }
        }

        private double IdealXPitch { get; set; }
        private double IdealYPitch { get; set; }

        ObservableCollection<string> _SlotType;
        public ObservableCollection<string> SlotType { get => _SlotType; set { _SlotType = value; OnPropertyChanged(); } }

        ObservableCollection<string> _Target;
        public ObservableCollection<string> Target { get => _Target; set { _Target = value; OnPropertyChanged(); } }

        private mdSlotZones _SlotZones;
        public mdSlotZones SlotZones { get => _SlotZones; } // this is the form's result

        public SolidColorBrush HighlightColor => _Brush_Highlight;

        #endregion Ordinary Properties


        #region Notifying Properties
        private string _ImageOverlayLabelText = "";
        public string ImageOverlayLabelText
        {
            get => _ImageOverlayLabelText;
            set { _ImageOverlayLabelText = value; OnPropertyChanged(); }
        }

        private string _FunctionalSlottingText = "";
        public string FunctionalSlottingText
        {
            get => _FunctionalSlottingText;

            set { _FunctionalSlottingText = value; OnPropertyChanged(); }
        }

        private string _FunctionalAreaText;
        public string FunctionalAreaText
        {
            get => _FunctionalAreaText;
            set { _FunctionalAreaText = value; OnPropertyChanged(); }
        }

        private string _SlotAreaText;
        public string SlotAreaText
        {
            get => _SlotAreaText;
            set { _SlotAreaText = value; OnPropertyChanged(); }
        }

        private string _ActualSlottingPercentageText;
        public string ActualSlottingPercentageText
        {
            get => _ActualSlottingPercentageText;
            set { _ActualSlottingPercentageText = value; OnPropertyChanged(); }
        }

        private string _RequiredSlotCountText;
        public string RequiredSlotCountText
        {
            get => _RequiredSlotCountText;
            set { _RequiredSlotCountText = value; OnPropertyChanged(); }
        }

        private string _ActualSlotCountText;
        public string ActualSlotCountText
        {
            get => _ActualSlotCountText;
            set { _ActualSlotCountText = value; OnPropertyChanged(); }
        }



        private string _CountDeviationText;
        public string CountDeviationText
        {
            get => _CountDeviationText;
            set { _CountDeviationText = value; OnPropertyChanged(); }
        }

        private string _PanelSlotCountErrorLimitText;
        public string PanelSlotCountErrorLimitText
        {
            get => _PanelSlotCountErrorLimitText;
            set { _PanelSlotCountErrorLimitText = value; OnPropertyChanged(); }
        }

        private string _TraySlotCountErrorLimitText;
        public string TraySlotCountErrorLimitText
        {
            get => _TraySlotCountErrorLimitText;
            set { _TraySlotCountErrorLimitText = value; OnPropertyChanged(); }
        }

        //private string _XPitchTextBlockText;
        //public string XPitchTextBlockText
        //{
        //    get => _XPitchTextBlockText; 
        //    set { _XPitchTextBlockText = value; OnPropertyChanged(); }
        //}

        //private string _YPitchTextBlockText;
        //public string YPitchTextBlockText
        //{
        //    get => _YPitchTextBlockText;
        //    set { _YPitchTextBlockText = value; OnPropertyChanged(); }
        //}

        private string _PitchTypeText;
        public string PitchTypeText
        {
            get => _PitchTypeText;
            set { _PitchTypeText = value; OnPropertyChanged(); }
        }

        private string _XPitchTextBoxText = "";
        public string XPitchTextBoxText
        {
            get => _XPitchTextBoxText;
            set { if (double.TryParse(value, out _)) { _XPitchTextBoxText = value; OnPropertyChanged(); } }
        }

        private string _YPitchTextBoxText = "";
        public string YPitchTextBoxText
        {
            get => _YPitchTextBoxText;

            set { if (double.TryParse(value, out _)) { _YPitchTextBoxText = value; OnPropertyChanged(); } else { _YPitchTextBoxText = ""; OnPropertyChanged(); } }
        }


        private bool _RetainSuppressedLocationChecked;
        public bool RetainSuppressedLocationChecked
        {
            get => _RetainSuppressedLocationChecked;
            set { _RetainSuppressedLocationChecked = value; OnPropertyChanged(); }
        }

        private ObservableCollection<PanelsDataGridViewModel> _PanelsDataGridItems;
        public ObservableCollection<PanelsDataGridViewModel> PanelsDataGridItems
        {
            get => _PanelsDataGridItems;
            set { _PanelsDataGridItems = value; OnPropertyChanged(); }
        }

        private bool _MaintainSymmetry = true;
        public bool MaintainSymmetry
        {
            get => _MaintainSymmetry;
            set { _MaintainSymmetry = value; OnPropertyChanged(); }
        }
        private bool _ClearSuppressedAllowed = false;
        public bool ClearSuppressedAllowed
        {
            get => _ClearSuppressedAllowed;
            set { _ClearSuppressedAllowed = value; OnPropertyChanged(); }
        }

        private Visibility _SuppressionIsVisible = Visibility.Collapsed;
        public Visibility SuppressionIsVisible
        {
            get => _SuppressionIsVisible;
            set
            {
                _SuppressionIsVisible = value;
                OnPropertyChanged();
                SlotGenerationPanelEnabled = value == Visibility.Collapsed;
            }
        }


        private bool _SuppressionIsActive = false;
        public bool SuppressionIsActive
        {
            get => _SuppressionIsActive;
            set
            {
                if (_SuppressionIsActive != value)
                {
                    if (value)
                    {
                        MessageBox.Show("Slot Suppression Is Now Active. Double Click a Slot to Toggle Its Suppression State.\n\nPress Cancel or Save or Uncheck Suppression To Save Your Changes.", "Suppression Mode Activated", MessageBoxButton.OK);
                        SuppressionPts = new colDXFVectors();
                        ClearSuppressedAllowed = false;
                        SlotGenerationPanelEnabled = false;
                        SuppressionIsVisible = Visibility.Visible;
                    }
                    else
                    {
                        SuppressionIsVisible = Visibility.Collapsed;
                        if (SuppressionPts.Count > 0)
                        {
                            bool savem = MessageBox.Show("Slot Suppression Is Deactivated. Select 'Yes' to Save Your Changes or 'No' To Abandon Them.", "Suppression Mode Disabled", MessageBoxButton.YesNo) == MessageBoxResult.Yes;

                            IsEnabled = false;

                            if (savem) LastRun = new mdSlotZones(CurrentZones);
                            List<string> znames =SuppressionPts.Tags();
                            
                            colDXFVectors suppts;
                            uopVector v2;
                            dxfVector s1;

                            foreach (var item in znames)
                            {
                                mdSlotZone zone = CurrentZones.Find(x => x.Handle ==item);
                                bool chnged = false;
                                if (zone != null)
                                {
                                    zone.UpdateSlotPoints(MDAssy);

                                    uopVectors gpts = zone.GridPts;

                                    suppts = SuppressionPts.GetByTag(zone.Handle, bRemove: true);
                                    for (int i = 1; i <= suppts.Count; i++)
                                    {

                                        s1 = suppts.Item(i);
                                        uopVector v1 = gpts.Nearest(s1);
                                        if (v1 != null)
                                        {
                                            if (savem)
                                            {
                                                v2 = zone.SetGridPointSuppression(v1.Index, s1.Suppressed);
                                                if (v2 != null) chnged = true;
                                            }
                                            else
                                            {
                                                s1.Suppressed = v1.Suppressed;
                                            }
                                            _SuppressTracker.SetRectangle(s1, s1.Suppressed, s1.Rotation, 0.76, 1.25, 30, zone.Name, redrawExisting: true);

                                        }


                                    } // loop on zone suppression pts

                                    if (chnged)
                                    {
                                        //CurrentZones.MemberUpdate(0, zone);
                                        //colDXFVectors sups = zone.GridPoints(MDAssy, false, false).GetByDisplayVariableValue(dxxDisplayProperties.Suppressed, true);
                                        DrawZoneSlots(Image, zone, _SuppressTracker);
                                    }



                                }
                            } // loop on zones


                        }
                    }

                    IsEnabled = true;
                    Redraw(true);
                    ClearSuppressedAllowed = true;
                    SlotGenerationPanelEnabled = true;
                    _SuppressionIsActive = value;
                    OnPropertyChanged();
                }
            }

        }


        private string _StatusBarRightLabelText;
        public string StatusBarRightLabelText
        {
            get => _StatusBarRightLabelText;
            set { _StatusBarRightLabelText = value; OnPropertyChanged(); }
        }


        private int _SelectedTargetIndex;
        public int SelectedTargetIndex
        {
            get => _SelectedTargetIndex;
            set { _SelectedTargetIndex = value; UpdateTargets(); OnPropertyChanged(); }
        }

        private int _SelectedSlotTypeIndex;
        public int SelectedSlotTypeIndex
        {
            get => _SelectedSlotTypeIndex;
            set
            {
                _SelectedSlotTypeIndex = value;
                if (!Loading && CurrentZones != null)
                {
                    CurrentZones.SlotType = SelectedSlotTypeIndex == 0 ? uppFlowSlotTypes.FullC : uppFlowSlotTypes.HalfC;
                    Redraw(true);
                }

                OnPropertyChanged();
            }
        }


        private Brush _DeviationForegroundColor;
        public Brush DeviationForegroundColor
        {
            get => _DeviationForegroundColor;
            set { _DeviationForegroundColor = value; OnPropertyChanged(); }
        }

        private int _SelectedPanelsDataGridIndex;
        public int SelectedPanelsDataGridIndex
        {
            get => _SelectedPanelsDataGridIndex;
            set { _SelectedPanelsDataGridIndex = value; OnPropertyChanged(); }
        }

        private string _TrayPercentPanelHeader;
        public string TrayPercentPanelHeader
        {
            get => _TrayPercentPanelHeader;
            set { _TrayPercentPanelHeader = value; OnPropertyChanged(); }
        }

        private string _XPitchPanelHeader;
        public string XPitchPanelHeader
        {
            get => _XPitchPanelHeader;
            set { _XPitchPanelHeader = value; OnPropertyChanged(); }
        }

        private string _YPitchPanelHeader;
        public string YPitchPanelHeader
        {
            get => _YPitchPanelHeader;
            set { _YPitchPanelHeader = value; OnPropertyChanged(); }
        }

        private string _PitchTypePanelHeader;
        public string PitchTypePanelHeader
        {
            get => _PitchTypePanelHeader;
            set { _PitchTypePanelHeader = value; OnPropertyChanged(); }
        }

        private string _RequiredCountPanelHeader;
        public string RequiredCountPanelHeader
        {
            get => _RequiredCountPanelHeader;
            set { _RequiredCountPanelHeader = value; OnPropertyChanged(); }
        }

        private string _Title = "Edit Tray Slot Configuration";
        public override string Title
        {
            get => _Title;
            set { _Title = value; OnPropertyChanged(); }
        }

        private string _ActualCountPanelHeader;
        public string ActualCountPanelHeader
        {
            get => _ActualCountPanelHeader;
            set { _ActualCountPanelHeader = value; OnPropertyChanged(); }
        }

        private string _CountErrorPanelHeader;
        public string CountErrorPanelHeader
        {
            get => _CountErrorPanelHeader;
            set { _CountErrorPanelHeader = value; OnPropertyChanged(); }
        }

        private bool _SlotGenerationPanelEnabled;
        public bool SlotGenerationPanelEnabled
        {
            get => _SlotGenerationPanelEnabled;
            set { _SlotGenerationPanelEnabled = value; OnPropertyChanged(); }
        }

        private bool _ControlsPanelEnabled;
        public bool ControlsPanelEnabled
        {
            get => _ControlsPanelEnabled;
            set { _ControlsPanelEnabled = value; OnPropertyChanged(); }
        }

        private Visibility _DxfViewerVisibility = Visibility.Visible;
        public Visibility DxfViewerVisibility
        {
            get => _DxfViewerVisibility;
            set { _DxfViewerVisibility = value; OnPropertyChanged(); }
        }

        private bool _SaveButtonEnabled;
        public bool SaveButtonEnabled
        {
            get => _SaveButtonEnabled;
            set { _SaveButtonEnabled = value; OnPropertyChanged(); }
        }

        private bool _RedrawButtonEnabled;
        public bool RedrawButtonEnabled
        {
            get => _RedrawButtonEnabled;
            set { _RedrawButtonEnabled = value; OnPropertyChanged(); }
        }

        private bool _WindowEnabled;
        public new bool IsEnabled
        {
            get => _WindowEnabled;
            set { _WindowEnabled = value; OnPropertyChanged(); }
        }

        private bool Refreshing { get; set; }
        private bool? _FunctionalSlottingTextBoxFocused;
        public bool? FunctionalSlottingTextBoxFocused
        {
            get => _FunctionalSlottingTextBoxFocused;
            set { _FunctionalSlottingTextBoxFocused = value; OnPropertyChanged(); }
        }

        private bool? _XPitchTextBoxFocused;
        public bool? XPitchTextBoxFocused
        {
            get => _XPitchTextBoxFocused;
            set { _XPitchTextBoxFocused = value; OnPropertyChanged(); }
        }

        private bool? _YPitchTextBoxFocused;
        public bool? YPitchTextBoxFocused
        {
            get => _YPitchTextBoxFocused;
            set { _YPitchTextBoxFocused = value; OnPropertyChanged(); }
        }

        private bool _ShowProgress;
        public bool ShowProgress
        {
            get => _ShowProgress;
            set
            {
                _ShowProgress = value;
                OnPropertyChanged();
                Visibility_Progress = _ShowProgress ? Visibility.Visible : Visibility.Collapsed;
                IsEnabled = !_ShowProgress;
            }
        }

        private Visibility _Visibility_Progress = Visibility.Collapsed;
        public override Visibility Visibility_Progress
        {
            get => _Visibility_Progress;
            set { _Visibility_Progress = value; OnPropertyChanged(); }
        }


        #endregion Notifying Properties

        #region Commands
        private DelegateCommand _CMD_Undo;
        public ICommand Command_Undo { get { if (_CMD_Undo == null) _CMD_Undo = new DelegateCommand(param => Execute_Undo()); return _CMD_Undo; } }


        private void Execute_Undo()
        {
            try
            {
                if (LastRun != null)
                {
                    var reply = MessageBox.Show("Undo Last Slot Generation Procedure?", "Undo Back", MessageBoxButton.YesNo);
                    if (reply == MessageBoxResult.No) return;

                    UndoLastChange();
                }

                //MousePointer = vbDefault // I am not sure if we need to do this here
                IsEnabled = true;
            }
            catch (Exception)
            {
            }
            finally { IsEnabled = true; }
        }

        private DelegateCommand _CMD_Apply;
        public ICommand Command_Apply { get { if (_CMD_Apply == null) _CMD_Apply = new DelegateCommand(param => Execute_Apply()); return _CMD_Apply; } }

        private void Execute_Apply()
        {
            try
            {
                ApplyPitches();
            }
            catch { }
            finally
            {
                IsEnabled = true;
            }
        }

        private DelegateCommand _CMD_EstimatePitches;
        public ICommand Command_EstimatePitches { get { if (_CMD_EstimatePitches == null) _CMD_EstimatePitches = new DelegateCommand(param => Execute_Estimate()); return _CMD_EstimatePitches; } }

        private void Execute_Estimate()
        {
            try
            {
                EstimatePitches();
            }
            catch { }
            finally
            {
                IsEnabled = true;
            }
        }

        private DelegateCommand _CMD_ClearSuppression;
        public ICommand Command_ClearSuppression { get { if (_CMD_ClearSuppression == null) _CMD_ClearSuppression = new DelegateCommand(param => Execute_ClearSuppressed()); return _CMD_ClearSuppression; } }


        private void Execute_ClearSuppressed()
        {
            try
            {
                ClearSuppressed();
            }
            catch { }
            finally
            {
                IsEnabled = true;
            }
        }

        private DelegateCommand _CMD_Redraw;
        public ICommand Command_Redraw { get { _CMD_Redraw ??= new DelegateCommand(param => Execute_Redraw()); return _CMD_Redraw; } }



        private void Execute_Redraw()
        {
            try
            {
                if (SuppressionIsActive)
                {
                    SuppressionIsActive = !SuppressionIsActive;
                }
                else
                {
                    Redraw(false, true);
                }
            }
            catch { }
            finally
            {
                IsEnabled = true;
            }
        }
        private DelegateCommand _CMD_Reset;
        public ICommand Command_Reset { get { _CMD_Reset ??= new DelegateCommand(param => Execute_Reset()); return _CMD_Reset; } }


        private void Execute_Reset()
        {
            try
            {
                if (SuppressionIsActive)
                {
                    SuppressionIsActive = !SuppressionIsActive;
                }
                else
                {
                    if (MessageBox.Show("Reset To Original Configuration?", "Reset Slots", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        InitializeView();
                    }
                }
            }
            catch { }
            finally
            {
                IsEnabled = true;
            }
        }

        private DelegateCommand _CMD_Cancel;
        public ICommand Command_Cancel { get { _CMD_Cancel ??= new DelegateCommand(param => Execute_Cancel()); return _CMD_Cancel; } }


        private void Execute_Cancel()
        {
            bool closeit = true;
            try
            {

                if (SuppressionIsActive)
                {
                    closeit = false;
                    SuppressionIsActive = !SuppressionIsActive;
                }
                else
                {
                    Edited = false;
                    Canceled = false;
                    if (LastRun != null)
                    {
                        if (!OriginalZones.IsEqual(CurrentZones))
                        {
                            MessageBoxResult reply = MessageBox.Show("Exit Without Saving Changes?", "Unsaved Changes Detected", MessageBoxButton.YesNo);
                            if (reply == MessageBoxResult.No) return;
                            _SlotZones = OriginalZones;

                        }

                        Canceled = true;
                        DialogResult = false;

                    }


                }
            }
            catch { }
            finally
            {
                IsEnabled = true;
                if (closeit)
                {
                    MyWindow().Close();
                    ReleaseReferences();


                }
            }
        }

        private DelegateCommand _CMD_Save;
        public ICommand Command_Save { get { if (_CMD_Save == null) _CMD_Save = new DelegateCommand(param => Execute_Save()); return _CMD_Save; } }


        private void Execute_Save()
        {
            bool closeit = true;
            try
            {
                if (SuppressionIsActive)
                {
                    closeit = false;
                    SuppressionIsActive = !SuppressionIsActive;
                }
                else
                {

                    Canceled = false;
                    Edited = OriginalCount != CurrentCount || !OriginalZones.IsEqual(CurrentZones);
                    if (Edited)
                    {

                        _SlotZones = CurrentZones;

                        MDAssy.SlotZones = SlotZones;

                        List<uppPartTypes> plist = new() { uppPartTypes.Deck };

                        RefreshMessage = new Message_Refresh(bSuppressTree: false, bSuppressImage: true, aPartTypeList: plist, bCloseDocuments: true);
                    }

                }
            }
            catch { }
            finally
            {
                IsEnabled = true;
                if (closeit)
                {
                    DialogResult = Edited;

                    MyWindow().Close();
                    ReleaseReferences();
                }
            }
        }

        private DelegateCommand _CMD_FunctionalSlottingTextBoxDoubleClick;
        public ICommand Command_FunctionalSlottingTextBoxDoubleClick { get { if (_CMD_FunctionalSlottingTextBoxDoubleClick == null) _CMD_FunctionalSlottingTextBoxDoubleClick = new DelegateCommand(param => Execute_FunctionalSlotting()); return _CMD_FunctionalSlottingTextBoxDoubleClick; } }


        private void Execute_FunctionalSlotting()
        {
            try
            {
                object tempSlottingPercentage = CurrentZones.SlottingPercentage;

                var q = new mzQuestions() { Title = "Enter Fs" };
                q.AddNumeric("Enter a Functional Slotting Percentage:", tempSlottingPercentage, aMaxValue: 1.0, aMinValue: 0.1, aMaxWholeDigits: 1, aValueControl: uopValueControls.PositiveNonZero);
                bool bCanc = !q.PromptForAnswers("Enter Fs", true, owner: (Window)Application.Current.Windows.OfType<System.Windows.Window>().SingleOrDefault(x => x.IsActive));

                if (!bCanc)
                {
                    double userAnswer = q.Item(1).AnswerD;
                    CurrentZones.SlottingPercentage = userAnswer;
                    UpdateTargets(new RefreshInput(""));
                }
            }
            catch { }
            finally
            {
                IsEnabled = true;
            }
        }
        #endregion Commands

        #region Worker Methods




        private void DoWork_Estimate(object sender, DoWorkEventArgs e)
        {

            if (IdealXPitch <= 0 || IdealYPitch <= 0  || uopUtils.RunningInIDE)
            {
                ShowProgress = true;
                IsEnabled = false;
                StatusBarRightLabelText = "Estimating Ideal Pitches";

                double pitchH = 0;
                double pitchV = 0;
                try
                {
                    mdSlotting.EstimatePitches(MDAssy, out pitchH, out pitchV);

                }
                catch { }
                finally
                {

                    IdealXPitch = pitchH;
                    IdealYPitch = pitchV;

                }

            }
            XPitchTextBoxText = LinearUnits.UnitValueString(IdealXPitch, DisplayUnits, bAddLabel: false);
            YPitchTextBoxText = LinearUnits.UnitValueString(IdealYPitch, DisplayUnits, bAddLabel: false);

        }

        private void DoWork_Complete(object sender, RunWorkerCompletedEventArgs e)
        {
            ShowProgress = false;
            IsEnabled = true;
            StatusBarRightLabelText = "";
            Loading = false;
        }

        #endregion Worker Methods

        #region Methods 




        public override void Activate(Window myWindow)
        {
            if (Activated) return;
            base.Activate(myWindow);
            try
            {

                if (Viewer == null)
                {
                    Edit_ECMDSlotting_View aView = myWindow as Edit_ECMDSlotting_View;
                    Viewer = aView?.cadfxDXFViewer;

                }

                //myWindow.Height = 920;
                //myWindow.Width = myWindow.Height * 1.3;

                myWindow.Top = 20;

                myWindow.Left = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width / 2 - myWindow.Width / 2;


                ShowProgress = true;
                Canceled = true;
                Edited = false;
                DialogResult = null;
                myWindow.Visibility = Visibility.Visible;
                DxfViewerVisibility = Visibility.Collapsed;
                _SuppressTracker = new SuppressTracker();
                _SyncCtx = SynchronizationContext.Current;
                _SuppressTracker.SetViewer(Viewer);
                _SuppressTracker.SetSynchronizationContext(_SyncCtx);
                XPitchTextBoxText = "";
                YPitchTextBoxText = "";
                ImageOverlayLabelText = "";
                SlotType = new ObservableCollection<string>() { "Full C", "Half C" };
                LastRun = null;

                Image = null;
                ZoomBox = null;
                Viewer.Clear();

                if (MDAssy == null) return;
                FunctionalActiveArea = MDAssy.FunctionalActiveArea;
                OriginalZones = new mdSlotZones( MDAssy.SlotZones);
                CurrentZones = OriginalZones.Clone();
                OriginalCount = OriginalZones.TotalSlotCount(MDAssy);
                CurrentZones.SlottingPercentage = MDAssy.Deck.SlottingPercentage;

                MaxX = MDAssy.DeckPanels.MaxPanelWidth();

                _Panels = new colMDDeckPanels();
                _Panels.SubPart(MDAssy);
                List<mdDeckPanel> panels = MDAssy.DeckPanels.ActivePanels(MDAssy);
                foreach (var item in panels)
                {
                    _Panels.Add(item);
                }

                Loading = true;
                ObservableCollection<string> targets = new() { "Entire Tray", "Field Panels", "Half Moon Panels", "Field Panels", "Half Moon Panels", "Manway Panels", "Field Panels Excl. Manways" };

                for (int i = 1; i <= panels.Count; i++)
                {
                    targets.Add($"Panel {i}");
                }
                Target = targets;


                SelectedTargetIndex = 0;

                LastRun = null;
                Image = null;
                ZoomBox = null;
                Viewer.Clear();

                DxfViewerVisibility = Visibility.Visible;

                _Worker_Estimate = new BackgroundWorker();
                _Worker_Estimate.DoWork += DoWork_Estimate;
                _Worker_Estimate.RunWorkerCompleted += DoWork_Complete;

                RefreshInput input = new(bApplyPitches: false, bRefreshData: true, bDrawSlots: true, bCreateView: true, aImage: Image, aTracker: _SuppressTracker);
                Execute_Refresh(input);
            }
            finally
            {

            }




        }

        internal void Window_Closing(object sender, CancelEventArgs e)
        {
            if (SuppressionIsActive)
            {

                e.Cancel = true;
                return;
            }
            //ReleaseReferences();

        }

        private async void Execute_Refresh(RefreshInput aInput)
        {

            if (Refreshing)
                return;
            try
            {
                StatusText = "Refreshing Display";
                Refreshing = true;
                ShowProgress = true;
                IsEnabled = false;
                aInput.Exceptions.Clear();
                if (Image == null) { aInput.CreateView = true; }
                if (aInput.ApplyPitches)
                {
                    SubStatusText = "Applying Pitches";
                    aInput.ApplyPitches = false;
                    await Execute_Refresh_ApplyPitchesLogicAsync(aInput);
                    aInput.RefreshData = true;
                    aInput.DrawSlots = true;
                    SubStatusText = string.Empty;
                }

                if (aInput.CreateView)
                {
                    aInput.CreateView = false;
                    aInput.Image = null;
                    SubStatusText = "Refreshing Image";
                    Image = await Execute_Refresh_CreateViewAsync(aInput);

                    aInput.DrawSlots = true;
                    SubStatusText = string.Empty;
                }
                if (aInput.DrawSlots)
                {
                    SubStatusText = "Refreshing Image";
                    aInput.Tracker = _SuppressTracker;
                    aInput.DrawSlots = false;
                    aInput.Image = Image;
                    await Execute_Refresh_DrawPanelSlotsAsync(aInput);
                    SubStatusText = string.Empty;
                }

                if (aInput.RefreshData)
                {
                    aInput.RefreshData = false;
                    SubStatusText = "Refreshing Display Data";
                    await Execute_RefreshDisplayDataAsync(aInput);
                    SubStatusText = string.Empty;
                }
            }
            finally
            {
                Refreshing = false;
                ShowProgress = false;
                StatusText = string.Empty;
                SubStatusText = string.Empty;
            }



        }

        private async Task Execute_Refresh_DrawPanelSlotsAsync(RefreshInput aInput)
        {
            if (_Loading) return;
            bool wuz1 = ShowProgress;
            bool wuz2 = IsEnabled;
            if (aInput.Image == null || aInput.Tracker == null) return;
            await Task.Run(() =>
            {

                try
                {
                    ShowProgress = true;
                    IsEnabled = false;

                    if (MDAssy == null || Viewer == null || CurrentZones == null) return;
                    if (aInput.Clear) aInput.Tracker.Clear();

                    List<mdSlotZone> aZones = CurrentZones.GetBySectionHandles(aInput.SectionHandles, bReturnAllForEmptyList: true);

                    foreach (var item in aZones)
                    {
                        DrawZoneSlots(aInput.Image, item, aInput.Tracker);
                    }

                }
                catch (Exception ex) { aInput.SaveException(ex); }
                finally
                {
                    ShowProgress = wuz1;
                    IsEnabled = wuz2;

                }
            });
        }

        private async Task Execute_RefreshDisplayDataAsync(RefreshInput aInput)
        {
            if (_Loading) return;
            bool wuz1 = ShowProgress;
            bool wuz2 = IsEnabled;

            await Task.Run(() =>
            {


                try
                {

                    if (CurrentZones == null) return;
                    ShowProgress = true;
                    IsEnabled = false;
                    CurrentCount = CurrentZones.TotalSlotCount(MDAssy);
                    // show the tray wide data
                    UpdateData(aInput);
                    ShowCurrentPitches(aInput);
                    UpdateTargets(aInput);

                }
                catch (Exception ex) { aInput.SaveException(ex); }
                finally
                {
                    ShowProgress = wuz1;
                    IsEnabled = wuz2;

                }
            });

        }


        private async Task<dxfImage> Execute_Refresh_CreateViewAsync(RefreshInput aInput)
        {
           
            dxfImage rImage = Image;
            if (_Loading) return rImage;
            bool wuz1 = ShowProgress;
            bool wuz2 = IsEnabled;

            await Task.Run(() =>
            {
                try
                {

                    ShowProgress = true;
                    IsEnabled = false;

                    if (rImage != null) rImage.Dispose();
                    rImage = new dxfImage(appApplication.SketchColor, Viewer?.Size);
                    rImage.Header.UCSMode = dxxUCSIconModes.None;

                    ZoomBox = Panels.Bounds(bExcludeVirtuals: true).ToDXFRectangle(); // new dxfRectangle(new dxfVector(d1 + wd / 2, 0), wd, ht);
                    ZoomBox.Expand(1.05, true, true);

                    rImage.Display.SetDisplayWindow(ZoomBox);
                    rImage.Display.SetFeatureScales(1 / rImage.Display.ZoomFactor / 2);

                    dxoDrawingTool draw = rImage.Draw;

                    //rImage.LinetypeLayers.Add("Center", dxxColors.Yellow, dxfLinetypes.Center, aImage: Image);
                    //rImage.LinetypeLayers.Setting = dxxLinetypeLayerFlag.ForceToColor;

                    // rImage.Screen.Entities.aScreenAxis(dxfPlane.World, dxxRectangularAlignments.BottomRight, 0.1, aColor: dxxColors.BlackWhite);

       
                    dxfDisplaySettings dsp = dxfDisplaySettings.Null(aLayer: "0", aColor: dxxColors.Yellow, aLinetype: dxfLinetypes.Center);
                    draw.aLine(new dxfVector(ZoomBox.Left, 0), new dxfVector(ZoomBox.Right, 0), aDisplaySettings: dsp);
                    draw.aLine(new dxfVector(0, ZoomBox.Top), new dxfVector(0, ZoomBox.Bottom), aDisplaySettings: dsp);
                    rImage.Display.ZoomExtents(1.05, true);
                    rImage.Header.UCSMode = dxxUCSIconModes.LowerLeft;

                    double tht = rImage.Display.PaperScale * 0.125;
                    dxfVector v1 = new(0, ZoomBox.Top - 0.1 * tht);
                    foreach (var item in Panels)
                    {
                        v1.X = item.X;
                        draw.aText(v1, item.Index, tht, dxxMTextAlignments.TopCenter);
                    }
                    //draw.aCircle(null, MDAssy.RingID / 2, "0", dxxColors.BlackWhite, "HIdden");
                    //draw.aCircleCenterLines(null, MDAssy.RingID / 2);
                    CurrentZones ??= new mdSlotZones(MDAssy.SlotZones);

                    

                    dxfDisplaySettings dsp1 = new dxfDisplaySettings("0", dxxColors.Blue, dxfLinetypes.Continuous);
                    dxfDisplaySettings dsp2 = new dxfDisplaySettings("0", dxxColors.Grey, dxfLinetypes.Continuous);
                    foreach (var zone in CurrentZones)
                    {
                        rImage.Entities.AddShape(zone.Boundary,dsp1);
                        rImage.Entities.AddShape(zone.BaseShape.SimplePerimeter, dsp2);
                        rImage.Entities.Append(zone.DXFIslands("0", dxxColors.Red, dxfLinetypes.Continuous));
                    }

                    uopLines weirs = MDAssy.DowncomerData.GetLines(uppDefinitionLineTypes.BoxWeirLine);
                    rImage.Entities.AddLines(weirs, dsp1);


                 dxeLine mirroraxis = ZoomBox.XAxis();

                    colMDDowncomers aDCs = MDAssy.Downcomers;
                    for (int i = 1; i <= aDCs.Count; i++)
                    {
                        mdDowncomer aDC = aDCs.Item(i);
                        if (aDC.IsVirtual) continue;

                        List<mdDowncomerBox> boxes = aDC.Boxes;

                        foreach (var box in boxes)
                        {
                            dxePolygon aPG = box.EndPlate(bTop: true).View_Plan(true);
                            aPG.LCLSet("0", dxxColors.BlackWhite, dxfLinetypes.Continuous);
                            dxePolyline pline = draw.aPolyline(aPG.Vertices, bClosed: true, aDisplaySettings: dxfDisplaySettings.Null(aLayer: "0", aColor: dxxColors.BlackWhite, aLinetype: dxfLinetypes.Continuous)).Clone();

                            aPG = box.EndPlate(bTop: false).View_Plan(true);
                            aPG.LCLSet("0", dxxColors.BlackWhite, dxfLinetypes.Continuous);
                            pline =draw.aPolyline(aPG.Vertices, bClosed: true, aDisplaySettings: dxfDisplaySettings.Null(aLayer: "0", aColor: dxxColors.BlackWhite, aLinetype: dxfLinetypes.Continuous)).Clone();
                        }
                    }

                    if (MDAssy.DesignFamily.IsBeamDesignFamily())
                    {
                        AppDrawingMDDHelper helper = new AppDrawingMDDHelper() { MDRange = MDRange, Image = rImage };
                        helper.Draw_Beams(uppViews.Plan, aLayerName: "0");
                    }
                }
                catch (Exception ex) { aInput.SaveException(ex); }
                finally
                {
                    aInput.Image = rImage;

                    
                    ShowProgress = wuz1;
                    IsEnabled = wuz2;

                }

                try
                {
                    _SyncCtx.Send(new System.Threading.SendOrPostCallback((o) =>
                    {
                        Viewer.SetImage(rImage, DisableZoom: false, DisablePan: false);
                        Viewer.EnableRightMouseZoom = true;
                        Viewer.SetBackgroundColor(rImage.Display.BackgroundColor);
                        Viewer.ZoomWindow(ZoomBox);
                        Viewer.RefreshDisplay();
                    }), null);
                }
                catch { }

            });

            return rImage;
        }

        private async Task Execute_Refresh_ApplyPitchesLogicAsync(RefreshInput aInput, List<string> aTargetSectionHandles = null)
        {
            if(_Loading) return;
            await Task.Run(() =>
            {
                //Message_Refresh request = RefreshMessage;
                bool wuz1 = ShowProgress;
                bool wuz2 = IsEnabled;
                try
                {
                    ShowProgress = true;
                    IsEnabled = false;


                    List<string> paneltargets = aTargetSectionHandles ?? GetTargetSectionHandles();
                    LastRun = new mdSlotZones( CurrentZones);
                    uppFlowSlotTypes slttype = _SelectedSlotTypeIndex == 0 ? uppFlowSlotTypes.FullC : uppFlowSlotTypes.HalfC;
                    if (CurrentZones.SlotType != slttype) CurrentZones.SlotType = slttype;
                    aInput.SectionHandles = paneltargets;
                    CurrentZones.SetPitches(aInput.XPitch, aInput.YPitch, paneltargets, true);
                }
                catch (Exception ex) { aInput.SaveException(ex); }
                finally
                {
                    ShowProgress = wuz1;
                    IsEnabled = wuz2;

                }
            });


        }

        protected void InitializeView( bool bDontRefresh = false)
        {

            _CurrentZones = new mdSlotZones( MDAssy.SlotZones);
            CurrentCount = _CurrentZones.TotalSlotCount(MDAssy);
            LastRun = null;
            Image = null;
            ZoomBox = null;
            if (!bDontRefresh)
            {
                Viewer.Clear();

                RefreshInput input = new(bApplyPitches: false, bRefreshData: true, bDrawSlots: true, bCreateView: true, aImage: Image, aTracker: _SuppressTracker);
                Execute_Refresh(input);

            }




        }


        protected void Redraw(bool bDataOnly = false, bool bClear = false)
        {

            if (bClear) _SuppressTracker?.Clear();
            RefreshInput input = new(false, true, !bDataOnly, false, Image, _SuppressTracker);
            Execute_Refresh(input);





        }


        private void UnitChange()
        {
            LinearUnits = uopUnits.GetUnit(uppUnitTypes.SmallLength);
            AreaUnits = uopUnits.GetUnit(uppUnitTypes.SmallArea);

            _IsEnglishSelected = _DisplayUnits == uppUnitFamilies.English;
            OnPropertyChanged("DisplayUnits");
            OnPropertyChanged("IsEnglishSelected");

            // NotifyPropertyChanged("DisplayUnits");
            // NotifyPropertyChanged("IsEnglishSelected");
            if (IsEnabled) Redraw(bDataOnly: true);



        }




        private void DrawZoneSlots(dxfImage aImage, mdSlotZone aZone, SuppressTracker aTracker)
        {

            bool wuz1 = ShowProgress;
            bool wuz2 = IsEnabled;
            if (aZone == null || aImage == null || aTracker == null) return;

            try
            {
                ShowProgress = true;
                IsEnabled = false;
                SubStatusText = $"Drawing Zone {aZone.SectionHandle} Slots";


                //colDXFVectors sPts = new colDXFVectors();

                if (aZone == null || aImage == null) return;

                aTracker.RemoveEntitiesUsingTag(aZone.Name, false);

                //if (aZone.VisualHandles != null)
                //{
                //    aTracker.RemoveEntitiesUsingHandle(aZone.VisualHandles, aZone.Name);

                //}


                //aImage.Screen.Entities.Delete(aZone.Tag);
                aZone.VisualHandles = new List<ulong>();
                aZone.UpdateSlotPoints(MDAssy, bMaintainSuppressed: RetainSuppressedLocationChecked, uopUtils.RunningInIDE);
                uopVectors gPts = aZone.GridPts;

                int drawn = 0;
                bool update = false;
                ulong handle;
                for (int i = 1; i <= gPts.Count; i++)
                {
                    drawn++;
                    update = drawn == 250 || i == gPts.Count;
                    if (update) drawn = 0;

                    uopVector v1 = gPts.Item(i);
                    if (!update)
                    {
                        handle = aTracker.SetRectangle(v1, v1.Suppressed, v1.Rotation, 0.76, 1.25, 30, aZone.Name, true, true);
                    }
                    else
                    {
                        handle = aTracker.SetRectangle(v1, v1.Suppressed, v1.Rotation, 0.76, 1.25, 30, aZone.Name, false, true);
                    }


                    if (handle != 0)
                    {
                        aZone.VisualHandles.Add(handle);
                    }
                }
                //drawn = 0;
                //for (int i = 1; i <= sPts.Count; i++)
                //{
                //    drawn++;
                //    update = drawn == 25 || i == sPts.Count;
                //    if (update) drawn = 0;

                //    v1 = sPts.Item(i);
                //    handle = aTracker.SetRectangle(v1, clr_Sup, v1.Rotation, 0.76, 1.25, 30, aZone.Name, !update, true,true);
                //    if (handle != 0)
                //    {
                //        aZone.VisualHandles.Add(handle);
                //    }
                //}


            }
            catch (Exception e) { throw e; }
            finally
            {
                //if (aZone != null)
                //{
                //    CurrentZones?.MemberUpdate(aZone.Index, aZone);
                //}
                ShowProgress = wuz1;
                IsEnabled = wuz2;
                SubStatusText = "";
            }

        }

        private dxxPitchTypes PitchType()
        {
            return dxxPitchTypes.Triangular; // TriangularRadioChecked ? dxxPitchTypes.Triangular : dxxPitchTypes.Rectangular;
        }

        private void EstimatePitches()
        {
            if (MDAssy == null || !IsEnabled) return;

            _Worker_Estimate.RunWorkerAsync();



        }

      

        private void ApplyPitches()
        {

            if (SuppressionIsActive)
            {
                MessageBox.Show("Slot Generation Is Disabled While Slot Suppression Is Active.", "Slot Generation Disabled", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            string ErrStr = ValidateInput(out double aXPitch, out double aYPitch, out List<string> sectionHandles);
            if (!string.IsNullOrWhiteSpace(ErrStr))
            {
                MessageBox.Show(ErrStr, "Input Error Detected", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //_SuppressTracker?.Reset();
            RefreshInput input = new(true, true, true, false, Image, _SuppressTracker, null, aXPitch, aYPitch, sectionHandles);
            Execute_Refresh(input);

        }

        private void ClearSuppressed()
        {
            if (SuppressionIsActive || _SuppressTracker == null || CurrentZones == null) return;
            List<dxfRectangle> suppressed = _SuppressTracker.SlotRectangles.GetBySuppressed(true);
            // suppressed = _SuppressTracker.SlotRectangles.CollectionObj.FindAll(mem => mem.Color == dxxColors.Yellow);
            if (suppressed.Count <= 0) return;
            if (MessageBox.Show($"Unsuppress {suppressed.Count} Slots?", "Clear Suppressed Slots", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.Cancel) return;
            bool updatedata = false;
            mdSlotZone zone;
            for (int i = 1; i <= CurrentZones.Count; i++)
            {
                zone = CurrentZones.Item(i);
                if (zone.ClearGridPointSuppression() > 0)
                {
                    updatedata = true;
                    DrawZoneSlots(Image, zone, _SuppressTracker);
                }
            }
            if (updatedata) Redraw(true);

        }



        public override void ReleaseReferences()
        {
            try
            {



                CurrentZones = null;
                OriginalZones = null;
                base.Image = null;

                if (Viewer != null)
                {
                    Viewer.Clear();
                    Viewer.MouseDown -= Viewer_MouseDown;
                    Viewer.MouseMove -= Viewer_MouseMove;
                    Viewer.MouseDoubleClick -= Viewer_MouseDoubleClick;
                    Viewer = null;
                }
                base.ReleaseReferences();

                if (_Worker_Estimate != null)
                {
                    _Worker_Estimate.DoWork -= DoWork_Estimate;
                    _Worker_Estimate.RunWorkerCompleted -= DoWork_Complete;
                    _Worker_Estimate = null;

                }

                LinearUnits = null;
                AreaUnits = null;

            }
            catch (Exception)
            {
            }
        }



        private void ShowCurrentPitches(RefreshInput aInput)
        {
            try
            {
                if (CurrentCount == 0)
                {
                    XPitchTextBoxText = $"{0}";
                    YPitchTextBoxText = $"{0}";
                    //TriangularRadioChecked = true;
                }
                else
                {


                    GetCurrentPitches(out double pX, out double pY, out int ptype);

                    XPitchTextBoxText = pX >= 0 ? LinearUnits.UnitValueString(pX, DisplayUnits) : "";
                    YPitchTextBoxText = pY >= 0 ? LinearUnits.UnitValueString(pY, DisplayUnits) : "";

                }

                ExecuteLostFocus();
            }
            catch (Exception ex) { aInput.SaveException(ex); }

        }

        private void GetCurrentPitches(out double XPitch, out double YPitch, out int PitchType)
        {



            XPitch = -1;
            YPitch = -1;
            PitchType = -1;
            if (CurrentZones == null) return;
            CurrentZones.GetPitchs(SelectedTargetIndex, 0, out dynamic pX, out dynamic pY, out dynamic ptype);

            if (pX is double)
            {
                XPitch = pX;
            }
            if (pY is double)
            {
                YPitch = pY;
            }
            //if (ptype is dxxPitchTypes)
            //{
            //    PitchType = (int)ptype;
            //}
        }

        private void ExecuteLostFocus()
        {
            // ^excutes the lost focus event on the data text boxes
            // ~the lost focus event formats the data based on the current application settings
            if (!IsEnabled) return;
            XPitchTextBoxText_LostFocus();
            YPitchTextBoxText_LostFocus();
        }


        // This is corresponding to "picPanel_MouseDown" in VB
        public void Viewer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {

                // if(!Viewer.InZoomWindow)  Viewer.ZoomWindow();



            }

        }


        public void Viewer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            if (e.RightButton == MouseButtonState.Pressed)
            {
                if (!Viewer.InZoomWindow) Viewer.ZoomExtents();
            }
            else
            {
                if (!SuppressionIsActive || _SuppressTracker == null || SuppressionPts == null || CurrentZones == null) return;
                Point point = e.GetPosition((System.Windows.IInputElement)sender);
                (double xval, double yval) = Viewer.DeviceToWorld(point.X, point.Y);
                dxfVector v0 = new(xval, yval);
                dxfRectangle slot = _SuppressTracker.SlotRectangles.GetContainingMember(v0);
                if (slot == null) return;
                mdSlotZone zone = CurrentZones.GetByName(slot.Tag);
                if (zone == null) return;
                dxfVector s1 = SuppressionPts.NearestVector(slot.Center);
                if (!v0.IsEqual(s1, 2)) s1 = null;
                if (s1 == null)
                {
                    s1 = slot.Center;
                    s1.Tag = zone.Handle;
                    s1.Suppressed = !slot.Suppressed;
                    SuppressionPts.Add(s1, aTag: zone.Handle);
                    s1.Rotation = slot.Value;
                }
                else
                {
                    s1.Suppressed = !s1.Suppressed;
                }
                _SuppressTracker.SetRectangle(s1, s1.Suppressed, s1.Rotation, 0.76, 1.25, 30, zone.Name, redrawExisting: true);


                if (MaintainSymmetry)
                {
                    dxfVector sym = new(s1.X, -s1.Y);
                    slot = _SuppressTracker.SlotRectangles.GetContainingMember(sym);
                    if (slot == null) return;
                    zone = CurrentZones.GetByName(slot.Tag);
                    if (zone == null) return;
                    dxfVector s2 = SuppressionPts.NearestVector(slot.Center);
                    if (!v0.IsEqual(s2, 2)) s2 = null;
                    if (s2 == null)
                    {
                        s2 = slot.Center;
                        s2.Tag = zone.Handle;
                        s2.Suppressed = s1.Suppressed;
                        SuppressionPts.Add(s2, aTag: zone.Handle);
                        s2.Rotation = slot.Value;
                    }
                    else
                    {
                        s2.Suppressed = s1.Suppressed;
                    }
                    _SuppressTracker.SetRectangle(s2, s2.Suppressed, s2.Rotation, 0.76, 1.25, 30, zone.Name, redrawExisting: true);


                }

            }
        }


        // It was "picPanel_MouseMove" in VB
        public void Viewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (Image == null || !IsEnabled)
            {
                return;
            }

            var position = e.GetPosition((System.Windows.IInputElement)sender);

            string coords;
            double xval = 0;
            double yval = 0;
            double multi;

            multi = LinearUnits.ConversionFactor(DisplayUnits);

            (xval, yval) = Viewer.DeviceToWorld(position.X, position.Y);

            coords = $"{string.Format("{0:0.000}", xval * multi)},{string.Format("{0:0.000}", yval * multi)}";
            ImageOverlayLabelText = coords;
        }


        public void UpdateTargets() => UpdateTargets(new RefreshInput(""));
        private void UpdateTargets(RefreshInput aInput)
        {
            //goControl.DisableApplication Me // may not be applicable here
            if (CurrentZones == null || LinearUnits == null || Target == null || MDAssy == null) return;
            try
            {
                string target = Target[SelectedTargetIndex];

                CurrentZones.GetZonePitches(MDAssy, GetTargetSectionHandles(), out List<double> pX, out List<double> pY, bUniqueVals: true);
                XPitchTextBoxText = (pX.Count == 1) ? LinearUnits.UnitValueString(pX[0], DisplayUnits, bAddLabel: false) : "";
                YPitchTextBoxText = (pY.Count == 1) ? LinearUnits.UnitValueString(pY[0], DisplayUnits, bAddLabel: false) : "";

            }
            catch (Exception ex) { aInput.SaveException(ex); }

        }

        public List<string> GetTargetSectionHandles()
        {
            List<string> _rVal = new();

            mdSlotZone zone;
            List<mdSlotZone> pzones;
            if (CurrentZones == null || LinearUnits == null || Target == null || MDAssy == null) return _rVal;
            string target = Target[SelectedTargetIndex].ToUpper();
            if (target.StartsWith("PANEL"))
            {
                pzones = CurrentZones.GetByPanelIndex(mzUtils.ExtractInteger(target, out _, out _)).ToList();
                foreach (mdSlotZone item in pzones) { _rVal.Add(item.SectionHandle); }

                return _rVal;
            }

            if (target.StartsWith("ENTIRE"))
            {
                pzones = CurrentZones;
                foreach (mdSlotZone item in pzones) { _rVal.Add(item.SectionHandle); }
                return _rVal;
            }

            if (target.StartsWith("MANWAY"))
            {

                List<string> manhandles = MDAssy.DeckSections.ManwayHandles();

                foreach (string item in manhandles)
                {
                    zone = CurrentZones.GetBySectionHandle(item);
                    if (_rVal != null) _rVal.Add(zone.SectionHandle);

                }

                return _rVal;
            }

            if (target.StartsWith("FIELD"))
            {
                bool excludemanways = target.Contains("MANWAY");
                List<string> manhandles = excludemanways ? MDAssy.DeckSections.ManwayHandles() : new List<string>();

                for (int i = 2; i <= Panels.Count; i++)
                {
                    pzones = CurrentZones.GetByPanelIndex(i);
                    foreach (mdSlotZone item in pzones)
                    {
                        if (manhandles.FindIndex((x) => x == item.SectionHandle) < 0)
                            _rVal.Add(item.SectionHandle);

                    }
                }
                return _rVal;

            }
            if (target.Contains("MOON"))
            {
                List<string> manhandles = MDAssy.DeckSections.MoonHandles();

                foreach (string item in manhandles)
                {
                    zone = CurrentZones.GetBySectionHandle(item);
                    if (_rVal != null) _rVal.Add(zone.SectionHandle);

                }

                return _rVal;
            }

            return _rVal;
        }

        // The name was "txtPitchX_LostFocus" in VB
        private void XPitchTextBoxText_LostFocus()
        {
            try
            {
                if (!IsEnabled) return;
                XPitchTextBoxText = string.IsNullOrWhiteSpace(XPitchTextBoxText) ? "" : XPitchTextBoxText.Trim();

                if (double.TryParse(XPitchTextBoxText, out double xPitchNumericValue))
                {
                    XPitchTextBoxText = string.Format("{0:0.0000}", xPitchNumericValue);
                }
                else
                {
                    XPitchTextBoxText = "";
                }
            }
            catch
            {
            }
        }

        // The name was "txtPitchY_LostFocus" in VB
        private void YPitchTextBoxText_LostFocus()
        {
            try
            {
                if (!IsEnabled) return;

                YPitchTextBoxText = string.IsNullOrWhiteSpace(YPitchTextBoxText) ? "" : YPitchTextBoxText.Trim();
                YPitchTextBoxText = double.TryParse(YPitchTextBoxText, out double yPitchNumericValue) ? string.Format("{0:0.0000}", yPitchNumericValue) : "";

            }
            catch
            {
            }
        }

        private void UpdateData(RefreshInput aInput)
        {
        
            try
            {
                if (CurrentZones == null) return;
                Loading = true;
                SelectedSlotTypeIndex = CurrentZones.SlotType == uppFlowSlotTypes.FullC ? 0 : 1;
                Loading = false;

                CurrentZones.SetSlotCounts(MDAssy, FunctionalActiveArea, false);

                PanelSlotCountErrorLimitText = "Panel Slot Count Error Limit = 5.0%";
                TraySlotCountErrorLimitText = "Tray Slot Count Error Limit = 2.0%";

                int cntAct = CurrentCount;
                int cntReq = CurrentZones.RequiredSlotCount;
                double dif = cntAct - cntReq;
                double dev = (cntReq > 0) ? dev = dif / cntReq * 100 : 100;


                Brush clr = Math.Abs(dev) > mdGlobals.MaxPanelSlotDeviation ? _Brush_Red : _Brush_Black;
                CountDeviationText = $"Count Deviation = {dev:0.0#}%";
                DeviationForegroundColor = clr;

                double pct1 = CurrentZones.SlottingPercentage; // target
                double pct2 = CurrentZones.ActualSlottingPercentage(MDAssy); // actual

                dif = pct2 - pct1;
                dev = pct1 != 0 ? dif / pct1 * 100 : 100;

                FunctionalAreaText = $"Functional Area = {AreaUnits.UnitValueString(FunctionalActiveArea, DisplayUnits)}";
                FunctionalSlottingText = $"{pct1:0.0#} %";
                ActualSlottingPercentageText = $"Actual Slotting Percentage = { pct2:0.0#} %";

                SlotAreaText = $"Slot Area = {AreaUnits.UnitValueString(CurrentZones.SlotArea, DisplayUnits)}";
                RequiredSlotCountText = $"Required Slot Count = {cntReq}";
                ActualSlotCountText = $"Actual Slot Count = {cntAct}";

                //CurrentZones.GetPitchs(0, 0, out dynamic pX, out dynamic pY, out dynamic ptype);
                //XPitchTextBlockText = $"X Pitch = {LinearUnits.UnitValueString(pX, DisplayUnits)}";
                //YPitchTextBlockText = $"Y Pitch = {LinearUnits.UnitValueString(pY, DisplayUnits)}";
                //if (ptype is int)
                //{
                //    PitchTypeText = ((int)ptype == (int)dxxPitchTypes.Rectangular) ?  "Pitch Type = Rectangular" :  "Pitch Type = Triangular";

                //}
                //else
                //{
                //    PitchTypeText = $"Pitch Type = {ptype}";
                //}

                UpdateGrids();
            }
            catch (Exception ex) { aInput.SaveException(ex); }

            finally
            {
                Loading = false;
            }

        }

        private void UpdateGrids()
        {

            bool wuz = Loading;
            Loading = true;
            try
            {
                int selectedRowIndex = SelectedPanelsDataGridIndex;
                uopTable dataTable = CurrentZones.GetTable(MDAssy, "PANELS", DisplayUnits);
                SetPanelHeaders(dataTable);
                PanelsDataGridItems = ExtractPanelData(dataTable);
                SelectedPanelsDataGridIndex = selectedRowIndex < PanelsDataGridItems.Count ? selectedRowIndex : 0;

            }
            catch (Exception e) { throw e; }
            finally
            {
                Loading = wuz;
            }

        }

        private void UndoLastChange()
        {
            if (LastRun != null)
            {
                CurrentZones = LastRun.Clone();
                LastRun = null; // I added this to make sure the "Undo" button gets disabled again!


                Redraw();

            }
        }


        private string ValidateInput(out double rXPitch, out double rYPitch, out List<string> rSectionHandles)
        {
            string _rVal = "";
            rXPitch = 0;
            rYPitch = 0;
            rSectionHandles = GetTargetSectionHandles();


            ExecuteLostFocus();

            if (!mzUtils.IsNumeric(XPitchTextBoxText))
            {
                _rVal = $"X Pitch Is Requited Input!";
                XPitchTextBoxFocused = true;
                return _rVal;
            }

            if (!mzUtils.IsNumeric(YPitchTextBoxText))
            {
                _rVal = $"Y Pitch Is Requited Input!";
                YPitchTextBoxFocused = true;
                return _rVal;
            }

            //If Not optPitch.Item(0) And Not optPitch.Item(1) Then optPitch.Item(0).Value = True // this won't happen here

            rXPitch = Math.Round(double.Parse(XPitchTextBoxText) / LinearUnits.ConversionFactor(DisplayUnits), 5);
            rYPitch = Math.Round(double.Parse(YPitchTextBoxText) / LinearUnits.ConversionFactor(DisplayUnits), 5);

            if (rXPitch < mdGlobals.MinSlotXPitch && rXPitch != 0)
            {
                _rVal = $"The Minimum Allowable X Pitch Is {LinearUnits.UnitValueString(mdGlobals.MinSlotXPitch, DisplayUnits)}";
                XPitchTextBoxFocused = true;
                return _rVal;
            }

            if (rXPitch > MaxX)
            {
                _rVal = $"The Maximum Allowable X Pitch Is {LinearUnits.UnitValueString(MaxX, DisplayUnits)}";
                XPitchTextBoxFocused = true;
                return _rVal;
            }

            if (rYPitch < mdGlobals.MinSlotYPitch && rYPitch != 0)
            {
                _rVal = $"The Minimum Allowable Y Pitch Is {LinearUnits.UnitValueString(mdGlobals.MinSlotYPitch, DisplayUnits)}";
                YPitchTextBoxFocused = true;
                return _rVal;
            }
            MaxY = 8 * rXPitch;
            if (rYPitch > MaxY)
            {
                _rVal = $"The Maximum Allowable Y Pitch Is {LinearUnits.UnitValueString(MaxY, DisplayUnits)}";
                YPitchTextBoxFocused = true;
                return _rVal;
            }

            if (CurrentZones.SlottingPercentage <= 0)
            {
                _rVal = "The Target Slotting Percentage Is Required!";
                FunctionalSlottingTextBoxFocused = true;
                return _rVal;
            }

            return _rVal;
        }

        // This method sets the headers for the condition that uopTable has 8 columns, means the mdSlotZones.GetTable was called with "PANELS". Calling it with "PANEL_ZONES" will break the code as it has 7 columns
        private void SetPanelHeaders(uopTable dataTable)
        {
            if (dataTable == null) return;


            var headerRow = dataTable.Row(1);
            if (headerRow == null) return;


            TrayPercentPanelHeader = headerRow[1].StringValue;
            XPitchPanelHeader = headerRow[2].StringValue;
            YPitchPanelHeader = headerRow[3].StringValue;
            //PitchTypePanelHeader = headerRow[4].StringValue;
            RequiredCountPanelHeader = headerRow[4].StringValue;
            ActualCountPanelHeader = headerRow[5].StringValue;
            CountErrorPanelHeader = headerRow[6].StringValue;
        }

        private ObservableCollection<PanelsDataGridViewModel> ExtractPanelData(uopTable dataTable)
        {
            ObservableCollection<PanelsDataGridViewModel> _rVal = new();

            if (dataTable == null) return _rVal;



            try
            {
                for (int i = 2; i <= dataTable.Rows; i++)
                {
                    List<uopTableCell> currentRow = dataTable.Row(i);
                    PanelsDataGridViewModel row = new()
                    {
                        RowNumber = int.Parse(currentRow[0].Value),
                        RowNumberColor = currentRow[0].FontColor == System.Drawing.Color.Red ? _Brush_Red : _Brush_Black,
                        TrayPercent = currentRow[1].StringValue,
                        XPitch = currentRow[2].StringValue,
                        YPitch = currentRow[3].StringValue,
                        PitchType = "tri.",
                        RequiredCount = mzUtils.VarToInteger(currentRow[4].Value),
                        ActualCount = mzUtils.VarToInteger(currentRow[5].Value),
                        CountError = currentRow[6].StringValue
                    };


                    _rVal.Add(row);
                }
            }
            catch (Exception)
            {
                _rVal = null;
            }

            return _rVal;
        }

        #endregion Methods



    }

    public class PanelsDataGridViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Implemantation
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        private int _RowNumber;
        public int RowNumber
        {
            get => _RowNumber;
            set { _RowNumber = value; OnPropertyChanged(); }
        }

        private string _TrayPercent;
        public string TrayPercent
        {
            get => _TrayPercent;
            set { _TrayPercent = value; OnPropertyChanged(); }
        }

        private string _XPitch;
        public string XPitch
        {
            get => _XPitch;
            set { _XPitch = value; OnPropertyChanged(); }
        }

        private string _YPitch;
        public string YPitch
        {
            get => _YPitch;
            set { _YPitch = value; OnPropertyChanged(); }
        }

        private string _PitchType;
        public string PitchType
        {
            get => _PitchType;
            set { _PitchType = value; OnPropertyChanged(); }
        }

        private int _RequiredCount;
        public int RequiredCount
        {
            get => _RequiredCount;
            set { _RequiredCount = value; OnPropertyChanged(); }
        }

        private int _ActualCount;
        public int ActualCount
        {
            get => _ActualCount;
            set { _ActualCount = value; OnPropertyChanged(); }
        }

        private string _CountError;
        public string CountError
        {
            get => _CountError;
            set { _CountError = value; OnPropertyChanged(); }
        }

        private Brush _RowNumberColor;
        public Brush RowNumberColor
        {
            get => _RowNumberColor;
            set { _RowNumberColor = value; OnPropertyChanged(); }
        }
    }

    public static class FocusExtension
    {
        public static readonly DependencyProperty IsFocusedProperty =
            DependencyProperty.RegisterAttached("IsFocused", typeof(bool?), typeof(FocusExtension), new FrameworkPropertyMetadata(IsFocusedChanged) { BindsTwoWayByDefault = true });

        public static bool? GetIsFocused(DependencyObject element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            return (bool?)element.GetValue(IsFocusedProperty);
        }

        public static void SetIsFocused(DependencyObject element, bool? value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            element.SetValue(IsFocusedProperty, value);
        }

        private static void IsFocusedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var fe = (FrameworkElement)d;

            if (e.OldValue == null)
            {
                fe.GotFocus += FrameworkElement_GotFocus;
                fe.LostFocus += FrameworkElement_LostFocus;
            }

            if (!fe.IsVisible)
            {
                fe.IsVisibleChanged += new DependencyPropertyChangedEventHandler(fe_IsVisibleChanged);
            }

            if (e.NewValue != null && (bool)e.NewValue)
            {
                fe.Focus();
            }
        }

        private static void fe_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var fe = (FrameworkElement)sender;
            if (fe.IsVisible && (bool)fe.GetValue(IsFocusedProperty))
            {
                fe.IsVisibleChanged -= fe_IsVisibleChanged;
                fe.Focus();
            }
        }

        private static void FrameworkElement_GotFocus(object sender, RoutedEventArgs e)
        {
            ((FrameworkElement)sender).SetValue(IsFocusedProperty, true);
        }

        private static void FrameworkElement_LostFocus(object sender, RoutedEventArgs e)
        {
            ((FrameworkElement)sender).SetValue(IsFocusedProperty, false);
        }
    }

    public class SuppressTracker
    {
        private DXFViewer _Viewer;
        private SynchronizationContext _SyncCtx;

        Dictionary<(double, double), (dxfVector, dxfRectangle, dxxColors, ulong, string)> _Rectangles;

        public colDXFRectangles SlotRectangles { get; set; }


        public SuppressTracker(DXFViewer aViewer = null, SynchronizationContext aSyncCtx = null)
        {
            this._Viewer = aViewer;
            this._SyncCtx = aSyncCtx;
            SlotRectangles = new colDXFRectangles();
            _Rectangles = new Dictionary<(double, double), (dxfVector, dxfRectangle, dxxColors, ulong, string)>();
        }

        private dxxColors Clr_Unsup => dxxColors.z_173;
        private dxxColors clr_Sup => dxxColors.Yellow;


        public void SetViewer(DXFViewer aViewer)
        {
            this._Viewer = aViewer;
        }

        public void SetSynchronizationContext(SynchronizationContext aSyncCtx)
        {
            this._SyncCtx = aSyncCtx;
        }


        public ulong SetRectangle(iVector vector, bool suppressed, double rotationInDegrees = 0, double width = 0.76, double height = 1.25, short lineWeight = 1, string aTag = "", bool suppressRefresh = false, bool redrawExisting = false)
        {
            if (_Viewer.overlayHandler == null) return 0;

            ulong handle = 0;
            dxxColors color = suppressed ? clr_Sup : Clr_Unsup;


            SlotRectangles ??= new colDXFRectangles();
            bool exists = _Rectangles.TryGetValue((vector.X, vector.Y), out (dxfVector vector, dxfRectangle rectangle, dxxColors color, ulong dxfHandle, string Tag) rectangleInfo);

            if (exists)
            {
                handle = rectangleInfo.dxfHandle;

                List<dxfRectangle> recs = SlotRectangles.GetByFlag(handle.ToString());
                dxfRectangle existingRectangle = recs.Count > 0 ? recs[0] : null;
                //List<string> flags = SlotRectangles.Flags();

                if (existingRectangle != null)
                {
                    existingRectangle.Suppressed = suppressed;
                    existingRectangle.Color = color;
                    rotationInDegrees = mzUtils.VarToDouble(existingRectangle.Value);
                    existingRectangle.Value = rotationInDegrees;
                    existingRectangle.Tag = aTag;
                    existingRectangle.Flag = handle.ToString();
                }
                else
                {
                    existingRectangle = new dxfRectangle(vector, width, height, rotationInDegrees) { Color = color, Flag = handle.ToString(), Tag = aTag, Suppressed = suppressed, Value = rotationInDegrees };
                    SlotRectangles.Add(existingRectangle);

                }


                if (rectangleInfo.color != color || redrawExisting) // If the color of the rectangle needs to be changed
                {
                    rectangleInfo = (new dxfVector(vector), rectangleInfo.rectangle, color, rectangleInfo.dxfHandle, aTag);
                    _Rectangles[(vector.X, vector.Y)] = rectangleInfo;



                    _SyncCtx.Send(new System.Threading.SendOrPostCallback((o) =>
                    {
                        handle = _Viewer.overlayHandler.DrawRectangleUsingHandle(rectangleInfo.rectangle, rectangleInfo.color, lineWeight, rotationInDegrees, rectangleInfo.dxfHandle, suppressRefresh: suppressRefresh);
                    }), null);


                }

            }
            else
            {
                dxfRectangle newRectangle = new(vector, width, height, rotationInDegrees) { Color = color, Flag = handle.ToString(), Tag = aTag, Suppressed = suppressed, Value = rotationInDegrees };

                _SyncCtx.Send(new System.Threading.SendOrPostCallback((o) =>
                {
                    handle = _Viewer.overlayHandler.DrawRectangleUsingHandle(newRectangle, color, lineWeight, rotationInDegrees, suppressRefresh: suppressRefresh);

                }), null);
                newRectangle.Flag = handle.ToString();

                SlotRectangles.Add(newRectangle);

                rectangleInfo = (new dxfVector(vector), newRectangle, color, handle, aTag);
                _Rectangles.Add((vector.X, vector.Y), rectangleInfo);
            }

            return handle;
        }


        public void RemoveEntitiesUsingTag(string aTag, bool bRefreshDisplay = true)
        {
            if (string.IsNullOrWhiteSpace(aTag)) return;
            Dictionary<(double, double), (dxfVector, dxfRectangle, dxxColors, ulong, string)> newrectangles = new();
            if (SlotRectangles != null)
            {
                SlotRectangles.GetByTag(aTag, bRemove: true);
            }

            List<ulong> handles = new();
            if (_Rectangles != null)
            {
                foreach (var item in _Rectangles)
                {
                    if (string.Compare(item.Value.Item5, aTag, true) != 0)
                    {
                        newrectangles.Add(item.Key, item.Value);
                    }
                    else
                    {
                        handles.Add(item.Value.Item4);
                    }
                }

            }
            _Rectangles = newrectangles;
            if (_Viewer == null || handles.Count <= 0) return;
            _SyncCtx.Send(new System.Threading.SendOrPostCallback((o) =>
            {
                _Viewer.overlayHandler.RemoveEntitiesUsingHandle(handles, bRefreshDisplay);
            }), null);

        }

        public void Clear()
        {

            List<ulong> handles;
            if (_Rectangles != null)
            {
                handles = new List<ulong>();
                foreach (var item in _Rectangles)
                {
                    handles.Add(item.Value.Item4);
                }
                if (_Viewer != null)
                {
                    _SyncCtx.Send(new System.Threading.SendOrPostCallback((o) =>
                    {
                        _Viewer.overlayHandler.RemoveEntitiesUsingHandle(handles, false);
                    }), null);
                    _SyncCtx.Send(new System.Threading.SendOrPostCallback((o) =>
                    {
                        _Viewer.RefreshDisplay();
                    }), null);

                }
            }
            _Rectangles = new Dictionary<(double, double), (dxfVector, dxfRectangle, dxxColors, ulong, string)>();
            if (SlotRectangles != null) SlotRectangles.Clear();
            SlotRectangles = new colDXFRectangles();
        }

        public void RemoveEntitiesUsingHandle(List<ulong> handles)
        {
            if (handles == null) return;
            Dictionary<(double, double), (dxfVector, dxfRectangle, dxxColors, ulong, string)> newrectangles = new();
            if (_Rectangles != null)
            {
                foreach (var item in _Rectangles)
                {
                    if (handles.IndexOf(item.Value.Item4) < 0)
                    {
                        newrectangles.Add(item.Key, item.Value);
                    }
                }

            }
            _Rectangles = newrectangles;
            if (_Viewer == null) return;
            _SyncCtx.Send(new System.Threading.SendOrPostCallback((o) =>
            {
                _Viewer.overlayHandler.RemoveEntitiesUsingHandle(handles, false);
            }), null);
            _SyncCtx.Send(new System.Threading.SendOrPostCallback((o) =>
            {
                _Viewer.RefreshDisplay();
            }), null);
        }



        public void Reset()
        {
            List<ulong> handles = _Rectangles.Select(kv => kv.Value.Item4).ToList();
            _SyncCtx.Send(new System.Threading.SendOrPostCallback((o) =>
            {
                _Viewer.overlayHandler.RemoveEntitiesUsingHandle(handles, false);
            }), null);

            _Rectangles.Clear();
            SlotRectangles = new colDXFRectangles();
        }
    }

    struct RefreshInput
    {

        public RefreshInput(string aDummy)
        {
            ApplyPitches = false;
            RefreshData = false;
            DrawSlots = false;
            CreateView = false;
            PanelIDs = new List<int>();
            Image = null;
            Tracker = null;
            XPitch = 0d;
            YPitch = 0d;
            SectionHandles = new List<string>();
            Clear = false;
            Exceptions = new List<Exception> { };
        }

        public RefreshInput(bool bApplyPitches, bool bRefreshData, bool bDrawSlots, bool bCreateView, dxfImage aImage, UOP.WinTray.UI.ViewModels.SuppressTracker aTracker, List<int> aPanelIDs = null, double aXPitch = 0, double aYPitch = 0, List<string> aSectionHandles = null, bool bClear = false)
        {
            ApplyPitches = bApplyPitches;
            RefreshData = bRefreshData;
            DrawSlots = bDrawSlots;
            CreateView = bCreateView;
            PanelIDs = aPanelIDs ?? new List<int>();
            Image = aImage;
            Tracker = aTracker;
            XPitch = aXPitch;
            YPitch = aYPitch;
            SectionHandles = aSectionHandles ?? new List<string>();
            Clear = bClear;
            Exceptions = new List<Exception> { };
        }

        public List<Exception> Exceptions { get; private set; }
        public bool ApplyPitches { get; set; }
        public bool RefreshData { get; set; }
        public bool CreateView { get; set; }
        public bool DrawSlots { get; set; }
        public double XPitch { get; set; }
        public double YPitch { get; set; }
        public List<int> PanelIDs { get; set; }
        public List<string> SectionHandles { get; set; }
        public dxfImage Image { get; set; }
        public SuppressTracker Tracker { get; set; }
        public bool Clear { get; set; }

        public void SaveException(Exception aException)
        {
            Exceptions ??= new List<Exception>();
            if (aException == null) Exceptions.Add(aException);
        }
    }

}
