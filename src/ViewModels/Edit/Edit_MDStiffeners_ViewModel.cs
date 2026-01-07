using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using UOP.DXFGraphicsControl;
using UOP.DXFGraphics;
using UOP.WinTray.UI.BusinessLogic;
using UOP.WinTray.UI.Messages;
using UOP.WinTray.UI.Commands;
using UOP.WinTray.Projects;
using UOP.WinTray.Projects.Parts;

using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Generators;

using UOP.WinTray.UI.Views.Windows.CADfx.Questions;
using MvvmDialogs;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;
using UOP.WinTray.Projects.Documents;

using System.Reflection;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Drawing;
using System.Threading.Tasks;

namespace UOP.WinTray.UI.ViewModels
{
    public class Edit_MDStiffeners_ViewModel : MDProjectViewModelBase,  IModalDialogViewModel
    {
        #region Fields

        private readonly BackgroundWorker _Worker_DrawObjects;
        private readonly BackgroundWorker _SaveWorker;
        #endregion Fields


        #region INotifyPropertyChanged Implementation

        public override event PropertyChangedEventHandler PropertyChanged;
        public override bool NotifyPropertyChanged(System.Reflection.MethodBase aMethod, bool? bNoErrors = null)
        {
            // Verify if the property is valid
          
            if (aMethod == null ||SuppressEvents) return false;
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
        public override bool NotifyPropertyChanged(string propertyName, bool? bNoErrors = null)
        {

            try
            {
                if (SuppressEvents) return false;
                if (!bNoErrors.HasValue) bNoErrors = !uopUtils.RunningInIDE;
                // Verify if the property is valid
                if (VerifyPropertyName(propertyName, bNoErrors.Value))
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

        #region Constructors

        public Edit_MDStiffeners_ViewModel(mdProject project , MDProjectViewModel parentVM) : base(project: project, parentVM: parentVM)
        {


            if (MDProject == null) return;
            if (MDAssy == null) return;
            SuppressEvents = false;
            _Worker_DrawObjects = new BackgroundWorker();
            _Worker_DrawObjects.DoWork += DoWork_DrawObjects;
            _Worker_DrawObjects.RunWorkerCompleted += DoWork_DrawObjects_Complete;

            _SaveWorker = new BackgroundWorker();
            _SaveWorker.DoWork += DoWork_Save;
            _SaveWorker.RunWorkerCompleted += DoWork_Save_Complete;

            try
            {

                FinishedWaitingForClick = true;
                IsEnglishSelected = MDProject.DisplayUnits == uppUnitFamilies.English;
                Downcomers = MDAssy.Downcomers.GetByVirtual(aVirtualValue: false);
                DCCount = Downcomers.Count();
                ManwaySections = MDAssy.DeckSections.Manways;

                    Spacing = MDAssy.DesignOptions.StiffenerSpacing;
             
                StartUpLength = MDAssy.Downcomer().StartupLength;
                FCLength = mdFingerClip.DefaultLength;
                OriginalStiffeners =    colUOPParts.FromPartsList(mdPartGenerator.Stiffeners_ASSY(MDAssy, bApplyInstances: false, bTrayWide: false), bGetClones: false);
                Stiffeners = OriginalStiffeners.Clone();
                
                StartUps = MDAssy.Downcomers.StartupSpouts(MDAssy, true);
                TrayNames = MDProject.TrayRanges.TrayNames(true, MDAssy.RangeGUID);
                DCList = "";
                MaxOrds = new List<double>();
                MinOrds = new List<double>();
                Runs = new List<colUOPParts>(); // the first item in the List, is the oldest one
                Redos = new List<colUOPParts>(); // the first item in the List, is the oldest one
                SaveRun(Stiffeners.Clone());


                if (MDAssy.DesignOptions.SpliceStyle == uppSpliceStyles.Angle) Straddle = true;

                //show tray parent properties in status bar
                GlobalProjectTitle = $"{MDProject.Name} ({MDAssy.TrayName(true)})";

                Inited = true;
                
                IsEnabled = true;
                
                RefreshDisplay();

            }
            catch { }
            
           
        
        }

        #endregion Constructors


        #region Worker Code

        private void DoWork_DrawObjects_Complete( object sender, RunWorkerCompletedEventArgs e )
        {

            try
            {
                RightStatusBarLabelText = "Loading Drawing";
                Viewer.SetImage(Image, DisableZoom: false, DisablePan: false);
                Viewer.EnableRightMouseZoom = true;
                if ((bool)e.Result)
                {
                    Viewer.ZoomWindow(Image.Display.LimitsRectangle);
                }
                else
                {
                    Viewer.CurrentZoomData =CurrentView;
                }
                Viewer.RefreshDisplay();

            }
            finally
            {

                RightStatusBarLabelText = "Updating Data";
                HighLight();
                ShowData();
                RightStatusBarLabelText = "";
                ShowProgress = false;
                IsEnabled = true;
            }
                

            //IsEnabled = true;
        }


        private async void Execute_RefreshImage()
        {
            ShowProgress = true;
            bool complete = false;
            await Task.Run(() => 
            {
                if (BaseImage == null)
                {
                    RightStatusBarLabelText = "Generating Persistent Entities...";
                    Execute_CreatePersistentEntities();
                }
                RightStatusBarLabelText = "Generating Stiffeners Drawing...";
                complete = Execute_DrawObjects();
            });
         
           
        }

        private void DoWork_DrawObjects( object sender, DoWorkEventArgs e )
        {

            IsEnabled = false;
            ShowProgress = true;

            if (BaseImage == null)
            {
                RightStatusBarLabelText = "Generating Persistent Entities...";
                Execute_CreatePersistentEntities();
            }
            RightStatusBarLabelText = "Generating Stiffeners Drawing...";

            e.Result = Execute_DrawObjects();
            

        }

        private void DoWork_Save(object sender, DoWorkEventArgs e)
        {
            Message_Refresh request = (Message_Refresh)e.Argument;
            mdProject proj = request.MDProject;
            mdTrayRange range = request.MDRange;
            mdTrayAssembly assy = request.MDAssy;
            SuppressEvents = false;
            IsEnabled = false;
            ShowProgress = true;
            RightStatusBarLabelText = "Saving Stiffener Locations ...";
            SuppressEvents = true;

            if (assy != null)
            {
                colUOPParts save = new colUOPParts(request.Parts );
                assy.SetStiffeners(save, request.Properties.ValueD("Spacing"), bSuppressEvnts: true);
            }

            e.Result = request;
        }

        private void DoWork_Save_Complete(object sender, RunWorkerCompletedEventArgs e)
        {

            Message_Refresh request = (Message_Refresh)e.Result;
            try
            {
                //request.ToggleAppEnabled(bEnabledVal: true);
                RightStatusBarLabelText = "";
                ShowProgress = false;
                request.DirtyRange = "";
                request.Parts = null;
                request.Properties = null;
            }
            catch { }
            finally
            {

                ResetWaitingForPoint();
                IsEnabled = true;
                request.PartType = uppPartTypes.Stiffener;
                RefreshMessage = request;
                MDProject.HasChanged = true;
                Edited = true;
                Canceled = false;
                SuppressEvents = false;
                DialogResult = true;
                SuppressEvents = true;
                MDProjectViewModel vm = ParentVM != null ? (MDProjectViewModel)ParentVM : request.MainVM.MDProjectViewModel;
                
                if (vm != null) vm.RespondToEdits(this, true, uppPartTypes.Stiffener);
            }


        }
        #endregion Worker Code

        #region Event Handlers

        public void Viewer_OnViewerMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1) { Viewer_MouseClick(sender, e); } else { Viewer_MouseDoubleClick(sender, e); }
        }

            public void Viewer_MouseClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var position = e.GetPosition((System.Windows.IInputElement)sender);

            if (!WaitingForClick)
            {
                NotifyPropertyChanged("IsEnabled");
                NotifyPropertyChanged("IsCancelBtnEnable");
                NotifyPropertyChanged("IsOkBtnEnable");
                MousePt ??= new dxfVector();
                (double px, double py) = Viewer.DeviceToWorld(position.X, position.Y);
                MousePt.SetCoordinates(px, py);

                HighLight();

            }
            else
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    if (e.ClickCount == 1)
                    {
                        LeftMouseButtonEventProcessor(System.Windows.Forms.MouseButtons.Left, (int)position.X, (int)position.Y);
                    }
                    else
                    {
                        if (e.ClickCount == 2)
                        {
                            LefMouseDoubleClickEventProcessor((int)position.X, (int)position.Y);
                        }
                    }

                }
                else if (e.RightButton == MouseButtonState.Pressed)
                {

                    //if (!Viewer.InZoomWindow) Viewer.ZoomWindow();

                }

            }
        }

        public void Viewer_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (Selected == null || e == null) return;
            if(e.Key == Key.Delete) Execute_Delete(null);
        }


        public void Viewer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition((System.Windows.IInputElement)sender);

            if (e.LeftButton == MouseButtonState.Pressed)
            {

                
                dxfRectangle aRect = new();
                LastRun = Stiffeners.Clone();
                if (Regions.EnclosesPoint(MousePt, true, ref aRect))
                { Execute_SetY(); e.Handled = true; }
                else if (DCRegions.EnclosesPoint(MousePt, true, ref aRect))
                { Execute_Edit(int.Parse(aRect.Tag)); e.Handled = true; }
                
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                if (e.Handled)
                    return;
                //IsEnabled = false;
                //ShowProgress = true;
                //Viewer.EnableRightMouseZoom = false;
                //Viewer.ZoomExtents();
                //Viewer.RefreshDisplay();
                //Viewer.EnableRightMouseZoom = true;
                //IsEnabled = true;
                //ShowProgress = false;


            }
        }

        public void Viewer_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            mdTrayAssembly assy = MDAssy;
            if (assy == null || Image == null || !IsEnabled) return;

            var position = e.GetPosition((System.Windows.IInputElement)sender);
            double xval = 0;
            double yval = 0;
         

            //xval = picPanel.ScaleX(X, picPanel.ScaleMode, vbPixels); // I don't know what this should do
            //yval = picPanel.ScaleX(Y, picPanel.ScaleMode, vbPixels// I don't know what this should do
            //DXFImage.Display.DeviceToWorld(xval, yval, , True); // The last argument's type does not match the signature
            (xval, yval) = Viewer.DeviceToWorld(position.X, position.Y);

            //ImageOverlayLocationLabelText = $"Y = {LinearUnits.UnitValueString(yval, DisplayUnits)}";

            if (WaitingForClick)
            {
                switch (FinishWaitingForClick)
                {
                    case "ADD":
                        {
                            ulong hnd = Viewer.overlayHandler.DrawHorzLine(new dxfVector(0, yval, 0), assy.GetMDRange().ShellID * 2, dxxColors.Red, 20, "CENTER", CursorHandles.Count > 0 ? CursorHandles[0] : 0);
                            if (!CursorHandles.Contains(hnd))
                                CursorHandles.Add(hnd);
                            hnd = 0;
                            (xval, yval) = Viewer.DeviceToWorld(0, position.Y);

                            hnd = Viewer.overlayHandler.DrawText(LinearUnits.UnitValueString(yval, DisplayUnits, bAddLabel: false), new dxfVector(xval + 2, yval - 2, 0), 2, CursorHandles.Count > 1 ? CursorHandles[1] : 0);
                            if (!CursorHandles.Contains(hnd))
                                CursorHandles.Add(hnd);
                            break;
                        }


                    default:
                        {
                            ResetWaitingForPoint();
                            break;
                        }
                }
            }
        }

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
                        case "ADD":
                            Command_Add.Execute(null);
                            break;
                        default:
                            ResetWaitingForPoint();
                            HighLight();
                            break;
                    }
                    return;
                }
                else
                {
                    HighLight();
                }

            }
        }

        public void LefMouseDoubleClickEventProcessor(int x, int y) { }


        #endregion Event Handlers


        #region Ordinary Properties

        public List<colUOPParts> Redos { get; set; }

        public List<colUOPParts> Runs { get; set; }

        private string DCList { get; set; }

        List<mdDeckSection> ManwaySections { get; set; }
        private dxePolyline StiffenerShape { get; set; }
        private List<dxfEntity> StiffenerEnts { get; set; }

        private List<string> TrayNames { get; set; }

        private List<double> MaxOrds { get; set; }

        private List<double> MinOrds { get; set; }

        private bool Straddle { get; set; }
     
        private double PixPerInch { get; set; }
        private colUOPParts LastRun { get; set; }
        public double Spacing { get; set; }
        private double StartUpLength { get; set; }
        private int DCCount { get; set; }
        private double FCLength { get; set; }
        private mdStartupSpouts StartUps{ get; set; }
        private mdStiffener Selected { get; set; }

        private double Multiplier { get => (LinearUnits == null) ? 1 : LinearUnits.ConversionFactor(DisplayUnits); }

        private dxfVector MousePt { get; set; }
        private colDXFRectangles Regions { get; set; }
        private colDXFRectangles DCRegions { get; set; }
        private dxfRectangle DCRegion { get; set; }
        private mzQuestions Questions_ADD { get; set; }
        private mzQuestions Questions_TRAY { get; set; }
        private mzQuestions Questions_UNI { get; set; }
        private mzQuestions Questions_YVALS { get; set; }
        private mzQuestions Questions_STRADDLE { get; set; }
        private mzQuestions Questions_SPACING { get; set; }
     
        public bool BestFit { get; set; }
        public bool Inited { get; set; }
       
        public  override DXFViewer Viewer 
        { 
            get => base.Viewer ;
            set 
            { 
               if(base.Viewer != null)
                {
                    base.Viewer.OnViewerMouseDown -= Viewer_OnViewerMouseDown;
                    base.Viewer.MouseDoubleClick -= Viewer_MouseDoubleClick;
                    base.Viewer.MouseMove -= Viewer_MouseMove;
                    base.Viewer.KeyDown -= Viewer_KeyDown;
                }
                base.Viewer = value;
                if (base.Viewer != null)
                {
                    base.Viewer.OnViewerMouseDown -= Viewer_OnViewerMouseDown;
                    base.Viewer.MouseDoubleClick -= Viewer_MouseDoubleClick;
                    base.Viewer.KeyDown -= Viewer_KeyDown;
                    base.Viewer.MouseMove -= Viewer_MouseMove;

                    base.Viewer.OnViewerMouseDown += Viewer_OnViewerMouseDown;
                    base.Viewer.MouseDoubleClick += Viewer_MouseDoubleClick;
                    base.Viewer.KeyDown += Viewer_KeyDown;
                    base.Viewer.MouseMove += Viewer_MouseMove;
                }

            } 
        }

        private bool WaitingForClick { get; set; }

        private string FinishWaitingForClick { get; set; }

        private bool FinishedWaitingForClick { get; set; }

        private readonly List<ulong> CursorHandles = new();



        private colUOPParts _Stiffeners;
        public colUOPParts Stiffeners 
        { 
            get => _Stiffeners;
            set { if (value != null) { value.MaintainIndices = true;  value.Reindex(); }   _Stiffeners = value; }
        }

        public colUOPParts OriginalStiffeners { get; set;  }
        
        private List<mdDowncomer> Downcomers { get; set; }
        private ZoomData CurrentView { get; set; }
      
        public dxfImage BaseImage { get; set; }

        public override dxfImage Image
        {
            get => base.Image; 
            set 
            {
                base.Image = value;
            }
        }

        private uopUnit _LinearUnits;
        private uopUnit LinearUnits { get { _LinearUnits ??= uopUnits.GetUnit(uppUnitTypes.SmallLength); return _LinearUnits; } }

        private System.Drawing.Size DisplayWindowSize => (Viewer != null) ? Viewer.Size : new System.Drawing.Size();

        public bool SymmetryIsEnabled => !MDAssy.DesignFamily.IsBeamDesignFamily();

        #endregion  Ordinary Properties


        #region Notifying Properties



        private bool? _DialogResult;
        public bool? DialogResult
        {
            get => _DialogResult;
            private set { _DialogResult = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); }
        }

        public override bool IsEnabled
        {
            get => base.IsEnabled;
            set { base.IsEnabled = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); }
        }

        private ObservableCollection<DowncomerLimitDataGridViewModel> _DowncomerLimits;
        
        public ObservableCollection<DowncomerLimitDataGridViewModel> DowncomerLimits
        {
            get => _DowncomerLimits;
            set { _DowncomerLimits = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); }
        }

        private bool _ImageOverlayCheckBoxIsChecked;
        public bool MaintainSymmetryIsChecked
        {
            get => _ImageOverlayCheckBoxIsChecked;
            set
            {
                _ImageOverlayCheckBoxIsChecked = value;
                NotifyPropertyChanged(MethodBase.GetCurrentMethod());
                if (value == true)
                    Execute_Symmetry(false);
            }
        }

        private string _ImageOverlayLocationLabelText;
        public string ImageOverlayLocationLabelText
        {
            get =>  _ImageOverlayLocationLabelText;
            set { _ImageOverlayLocationLabelText = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); }
        }

        private string _ImageOverlayTotalLabelText;
        public string ImageOverlayTotalLabelText
        {
            get => _ImageOverlayTotalLabelText;
            set { _ImageOverlayTotalLabelText = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); }
        }

       

        private string _RightStatusBarLabelText;
        public string RightStatusBarLabelText
        {
            get  => _RightStatusBarLabelText;
            set { _RightStatusBarLabelText = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); }
        }
      

        private bool _ShowProgress;
        public bool ShowProgress
        {
            get => _ShowProgress;
            set { _ShowProgress = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); IsEnabled = !_ShowProgress; }
        }

        public override bool IsEnglishSelected
        {
            get => base.IsEnglishSelected;
            set { bool newval = value != base.IsEnglishSelected; base.IsEnglishSelected = value; if (newval) UnitChange(); }
        }

        private bool _Hidden;
        public bool Hidden
        {
            get => _Hidden;
            set { _Hidden = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); }

        }



        #endregion Notifying Properties


        #region Commands
        private bool CanExec() { return IsEnabled; }

        private DelegateCommand _CMD_EditDCStiffeners;
        public ICommand Command_EditDCStiffeners { get { _CMD_EditDCStiffeners ??= new DelegateCommand(param1 => Execute_Edit(), param2 => CanExec(), isAutomaticRequeryDisabled: true); return _CMD_EditDCStiffeners; } }

        private DelegateCommand _CMD_InputYValues;
        public ICommand Command_InputYValues { get { _CMD_InputYValues ??= new DelegateCommand(param1 => Execute_InputY(), param2 => CanExec(), isAutomaticRequeryDisabled: true); return _CMD_InputYValues; } }
     
        private DelegateCommand _CMD_SetY;
        public ICommand Command_SetY { get { _CMD_SetY ??= new DelegateCommand(param1 => Execute_SetY(), param2 => CanExec(), isAutomaticRequeryDisabled: true); return _CMD_SetY; } }
       

        private DelegateCommand _CMD_Add;
        public ICommand Command_Add { get { _CMD_Add ??= new DelegateCommand(param1 => Execute_Add(), param2 => CanExec(), isAutomaticRequeryDisabled: true); return _CMD_Add; } }

        private DelegateCommand _CMD_Delete;
        public ICommand Command_Delete { get { _CMD_Delete ??= new DelegateCommand(param1 => Execute_Delete(), param2 => CanExec(), isAutomaticRequeryDisabled: true); return _CMD_Delete; } }
        private DelegateCommand _CMD_UniformSpace;
        public ICommand Command_UniformSpacing { get { _CMD_UniformSpace ??= new DelegateCommand(param1 => Execute_UniformSpace(), param2 => CanExec(), isAutomaticRequeryDisabled: true); return _CMD_UniformSpace; } }
      
        private DelegateCommand _CMD_StraddleDowncomers;
        public ICommand Command_StraddleDowncomers { get { _CMD_StraddleDowncomers ??= new DelegateCommand(param1 => Execute_Straddle(), param2 => CanExec(), isAutomaticRequeryDisabled: true); return _CMD_StraddleDowncomers; } }
      

        private DelegateCommand _CMD_CopyTrayStiffeners;
        public ICommand Command_CopyTrayStiffeners { get { _CMD_CopyTrayStiffeners ??= new DelegateCommand(param1 => Execute_CopyTray(), param2 => CanExec(), isAutomaticRequeryDisabled: true); return _CMD_CopyTrayStiffeners; } }
        

        private DelegateCommand _CMD_EnforceSymmetry;
        public ICommand Command_EnforceSymmetry { get { _CMD_EnforceSymmetry ??= new DelegateCommand(param1 => Execute_Symmetry(), param2 => CanExec(), isAutomaticRequeryDisabled: true); return _CMD_EnforceSymmetry; } }
      
        private DelegateCommand _CMD_Spacing;
        public ICommand Command_Spacing { get { _CMD_Spacing ??= new DelegateCommand(param1 => Execute_Spacing(), param2 => CanExec(), isAutomaticRequeryDisabled: true); return _CMD_Spacing; } }
    

        private DelegateCommand _CMD_Undo;
        public ICommand Command_Undo { get { _CMD_Undo ??= new DelegateCommand(param1 => Execute_Undo(), param2 => CanExec(), isAutomaticRequeryDisabled: true); return _CMD_Undo; } }
     
        private DelegateCommand _CMD_Redo;
        public ICommand Command_Redo { get { _CMD_Redo ??= new DelegateCommand(param1 => Execute_Redo(), param2 => CanExec(), isAutomaticRequeryDisabled: true); return _CMD_Redo; } }
     
        private DelegateCommand _CMD_Reset;
        public ICommand Command_Reset { get { _CMD_Reset ??= new DelegateCommand(param1 => Execute_Reset(), param2 => CanExec(), isAutomaticRequeryDisabled: true); return _CMD_Reset; } }
     

        private DelegateCommand _CMD_Redraw;
        public ICommand Command_Redraw { get { _CMD_Redraw ??= new DelegateCommand(param1 => Execute_Redraw(), param2 => CanExec(), isAutomaticRequeryDisabled: true); return _CMD_Redraw; } }
   

        private DelegateCommand _CMD_Cancel;
        public ICommand Command_Cancel { get {  _CMD_Cancel ??= new DelegateCommand(param1 => Execute_Cancel(), param2 => CanExec(), isAutomaticRequeryDisabled: true); return _CMD_Cancel; } }

        private DelegateCommand _CMD_Save;
        public ICommand Command_Save { get { _CMD_Save ??= new DelegateCommand(param1 => Execute_Save(), param2 => CanExec(), isAutomaticRequeryDisabled: true); return _CMD_Save; } }

        #endregion Commands

        #region Methods

        private void Execute_Cancel()
        {
            if (!IsEnabled || MDAssy == null) return;

            if (WaitingForClick)
            {
                ResetWaitingForPoint();
                return;
            }

            bool close = true;
            try
            {
                Edited = !Stiffeners.Centers().IsEqual(OriginalStiffeners.Centers(), aPrecis: 3) || Spacing != MDAssy.DesignOptions.StiffenerSpacing;
                

                if (Edited)
                {
                    MessageBoxResult reply = MessageBox.Show("Exit Without Saving Changes?", "Unsaved Changes Detected", MessageBoxButton.YesNo);
                    if (reply == MessageBoxResult.No)
                    {
                        close = false;
                        return;
                    }
                }
            }
            finally
            {
                if (close)
                {
                    ReleaseReferences();
                    IsOkBtnEnable = true;
                    BusyMessage = "";
                    IsEnabled = true;
                    Edited = false;
                    Canceled = true;
                    DialogResult = false;  // close the view window

                }
            }





        }

        private void Execute_Save()
        {

            if (!IsEnabled || MDAssy == null) return;
            Canceled = false;
            if (WaitingForClick)
            {
                ResetWaitingForPoint();
                return;
            }
            colUOPParts save = Stiffeners.Clone();
            Edited = ! OriginalStiffeners.Centers().IsEqual(save.Centers(), aPrecis: 3) ;
            //mdProject proj = MDProject;
            //mdTrayAssembly assy = MDAssy;
            mdTrayRange range = MDRange;
            if (Edited)
            {
                //IsEnabled = false;
                ////Hidden = true;
                //RightStatusBarLabelText = "Saving Stiffeners";
                //ShowProgress = true;
                //Viewer = null;
                //SuppressEvents = true;
                Message_Refresh refresh = new Message_Refresh() { PartType = uppPartTypes.Stiffener, SuppressDataGrids = true ,SuppressTree= false, DirtyRange = range.GUID, Range = range, SuppressImage = false, Parts = save.OfType<uopPart>().ToList() };
                refresh.Properties = new uopProperties(new uopProperty("Spacing", Spacing, uppUnitTypes.SmallLength));
                _SaveWorker.RunWorkerAsync(refresh);

            }
            else
            {
              
                Canceled = true;
                DialogResult = false;
            }


        }


        private void Execute_Edit(int aDCID = 0)
        {


            if (WaitingForClick || !IsEnabled) return ;

            bool result = false;
            if (MDAssy == null) return;
            if (aDCID <= 0)
            {
                aDCID = Selected == null ? 1 : Selected.DowncomerIndex;
            }
            if (aDCID < 1 || aDCID > Downcomers.Count) return;

          
            mzQuestions aQuestions;
            double mx;
        
            List<double> yvals;
            List<double> newVals;
            mdStiffener aStf = null;
            mdDowncomer aDC;

            aDC = Downcomers[aDCID- 1];
            if (aDC == null) return;
            mx = MaxOrds[aDCID -1];

            mzValues initVals =  Stiffeners.Ordinates(dxxOrdinateDescriptors.Y, aDowncomerID: aDC.Index);
            yvals = initVals.ToNumericList(5, mzSortOrders.HighToLow);
            aQuestions = new mzQuestions() { Title = $"DC { aDC.Index } Stiffener Locations" };
            aQuestions.AddNumericList("Enter Y Values:", initVals.ToList(), LinearUnits.Label(DisplayUnits) , LinearUnits.ConversionFactor(DisplayUnits), -1, null, null, aMinDifference: 3,  aMaxDecimals: 4, bAddMirrors: !MDAssy.DesignFamily.IsBeamDesignFamily()); // aMinAnswers value was -mx in original VB code which is not an int
            bool bCanc  = !aQuestions.PromptForAnswers( "", true, owner: (System.Windows.Window)Application.Current.Windows.OfType<System.Windows.Window>().SingleOrDefault( x => x.IsActive )); // rChangeList has not been used
            if (bCanc)  return;
           
            newVals = aQuestions.Item(1).Answers.ToNumericList(5, mzSortOrders.HighToLow);

            var invalidStiffenerValues = newVals.Where(s => aDC.FindDowncomerBoxContainingStiffenerOrdinate(s) == null).ToArray();
            if (invalidStiffenerValues.Length > 0)
            {
                MessageBox.Show($"Invalid stiffener ordinate: {invalidStiffenerValues[0]}", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (mzUtils.CompareNumericList( yvals,newVals, 5)) return;

            LastRun = Stiffeners.Clone();

            result = true;

            Stiffeners.GetByDowncomerIndex(aDC.Index, bRemove: true);
            for (int i = 0; i < newVals.Count; i++)
            {
               Stiffeners.Add( aDC.Stiffener( newVals[i]) );
               if (i == 0) aStf = (mdStiffener)Stiffeners.Last();
               
            }
            
            mdUtils.StiffenersSort(Stiffeners, MDAssy);
            if (aStf != null)
            {
                MousePt.SetCoordinates(aStf.X, aStf.Y);
                Selected = aStf;
            }

            FinishCommand( result);
        }

        private void Execute_Spacing()
        {
            if (WaitingForClick || !IsEnabled) return ;

            bool result = false;

            if (Questions_SPACING == null)
            {
                Questions_SPACING = new mzQuestions();
                Questions_SPACING.AddNumeric( "Input a Spacing Change Value:", 3, LinearUnits.Label(DisplayUnits) , LinearUnits.ConversionFactor(DisplayUnits), MDAssy.RingID / 4, -MDAssy.RingID / 4, uopValueControls.NonZero, aMaxDecimals:4, bShowAllDigits: true );
                Questions_SPACING.AddMultiSelect( "Select Downcomers:", DCList, "All", 1 );
            }
            if (!Questions_SPACING.PromptForAnswers("Increase/Decrease Spacing", true, owner: MyWindow())) return ;

            LastRun = Stiffeners.Clone();

            int k = 0;
            int idx = 0;
            int dcidx;
            double aspc = double.Parse(Questions_SPACING.Item(1).Answer.ToString());
            List<int> aDCList = DCIndices(Questions_SPACING.Item(2).Answers);

            List<uopPart> aStfs;
            mdStiffener aStf;
            mdStiffener bStf;
            bool bStrad = false;
            mdDowncomer aDC;

            for (int i = 0; i < aDCList.Count; i++)
            {
                dcidx = aDCList[i];

                aDC = Downcomers[dcidx -1];
                aStfs = Stiffeners.GetByDowncomerIndex(dcidx);
                if (aStfs.Count > 1)
                {
                    aStfs = uopParts.SortPartsByOrdinate(aStfs,dxxOrdinateDescriptors.Y, false);

                    for (int j = 0; j < aStfs.Count - 2; j++)
                    {
                        aStf = (mdStiffener)aStfs[j];
                        bStf = (mdStiffener)aStfs[j + 1];
                        if (Math.Round(bStf.Y, 3) <= 0)
                        {
                            if (Math.Round(bStf.Y, 3) == 0)
                            {
                                idx = j;
                            }
                            else
                            {
                                bStrad = true;
                                idx = j - 1;
                                bStf.Y -= 0.5 * aspc;
                                aStf.Y += 0.5 * aspc;
                                result = true;
                            }
                            break;
                        }
                    }

                    if (idx > 0)
                    {
                        result = true;
                        k = 1;
                        for (int j = idx; j > 0; j--)
                        {
                            aStf = (mdStiffener)aStfs[ j ];
                            if (!bStrad)
                            {
                                aStf.Y += aspc * k;
                            }
                            else
                            {
                                if (k != idx)
                                {
                                    aStf.Y += aspc * k;
                                }
                            }
                            k ++;
                        }

                        k = 1;
                        if (!bStrad)
                        {
                            idx += 2;
                        }
                        for (int j = idx; j < aStfs.Count; j++)
                        {
                            aStf = (mdStiffener)aStfs[ j ];
                            if (!bStrad)
                            {
                                aStf.Y -= aspc * k;
                            }
                            else
                            {
                                if (k > idx + 1)
                                {
                                    aStf.Y -= aspc * k;
                                }
                            }
                            k++;
                        }
                    }
                }
            }
            mdUtils.StiffenersSort(Stiffeners, MDAssy);

            FinishCommand( result);
        }

        private void Execute_Symmetry(bool bAlertIfCurrentlySymmetric = true)
        {
            if (WaitingForClick || !IsEnabled) return ;
            bool result = false;
            colDXFVectors NonSym = new();
            if (mdUtils.StiffenersAreSymetric(MDAssy, Stiffeners, NonSym))
            {
                if (bAlertIfCurrentlySymmetric) MessageBox.Show( "The current layout is symmetric", "No Changes Requires", button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
                return ;
            }

          
            if (MessageBox.Show("Force Symmetry?", "Enforce Symmetry", button: MessageBoxButton.YesNo, icon: MessageBoxImage.Question) == MessageBoxResult.No) return ;
            
         
            LastRun = Stiffeners.Clone();

            for (int i = 1; i <= NonSym.Count; i++)
            {
                dxfVector v1 = NonSym.Item(i);
                mdStiffener aStf = (mdStiffener)Stiffeners.GetByCenter(v1, out _);
                if (aStf != null)
                {
                    mdStiffener bStf = (mdStiffener)aStf.Clone();
                    if (aStf.Y != 0)
                    {
                        bStf.Y = -aStf.Y;
                        Stiffeners.Add(bStf);
                    }

                    if (Stiffeners.GetBetweenOrdinate(dxxOrdinateDescriptors.Y, -aStf.Y - 3, -aStf.Y + 3, bOnIsIn: false, aPartToSkip: bStf, bRemove: true, aDowncomerID: aStf.DowncomerIndex).Count > 0)
                    {
                        result = true;
                    }
                }
            }
            mdUtils.StiffenersSort(Stiffeners, MDAssy);

            FinishCommand( result);
        }

     
        private bool Execute_Add()
        {

            bool ret = false;
            if (WaitingForClick || !IsEnabled) return ret;

            if (!FinishedWaitingForClick)
            {
                ResetWaitingForPoint();
            //    _LastRun = Splices.Clone();
                MousePoint ??= new dxfVector();

                ret = Execute_AddComplete(MousePoint.X, MousePoint.Y);
                FinishCommand(ret);
            }
            else
            {
                WaitingForClick = true;
                FinishedWaitingForClick = false;
                FinishWaitingForClick = "ADD";
                Viewer.Cursor = System.Windows.Input.Cursors.ScrollNS;
            }
            return ret;

            //dxxOrdinateDescriptors dxfOrdinate_Y = dxxOrdinateDescriptors.Y;
            //if (Questions_ADD == null)
            //{
            //    Questions_ADD = new mzQuestions("Enter Stiffener Y Location") ;
            //    Questions_ADD.AddNumeric( "Y Value:", 0, LinearUnits.Label(DisplayUnits) , LinearUnits.ConversionFactor(DisplayUnits), MaxOrds[MaxOrds.Count - 1], -MaxOrds[MaxOrds.Count - 1], uopValueControls.None, aMaxDecimals: 4, bShowAllDigits: true );
            //    Questions_ADD.AddMultiSelect( "Select Downcomers:", DCList, "All", 1);
            //    Questions_ADD.AddCheckVal( "Add Mirrors?", true );
            //}

            //bool ret = false;
            
            //Questions_ADD.PromptForAnswers( out bool bCanc, out string _, "", true, 0.5, true, true, true, owner:  MyWindow()  ); 
            //if (bCanc) return false;


            //LastRun = Stiffeners.Clone();

            //List<int> aDCIDs;
            //int i = 0;
            //mdDowncomer aDC = null;
            //double aYVal = 0.0;
            //mdStiffener aStf;
            //bool bMirrors = false;

            //aDCIDs = DCIndices(Questions_ADD.Item(2).Answers);
            //if (aDCIDs.Count <= 0) return false;
            //aYVal = (double)Questions_ADD.Item(1).Answer;
            //bMirrors = (bool)Questions_ADD.Item(3).Answer;


            //for (i = 0; i < aDCIDs.Count; i++)
            //{
            //    aDC = Downcomers[ aDCIDs[i] -1];
            //    if (Math.Abs(aYVal) <= MaxOrds[aDC.Index - 1])
            //    {
            //        if (Stiffeners.GetByOrdinate( dxfOrdinate_Y, aYVal, aDowncomerID: aDC.Index ).Count <= 0)
            //        {
            //            aStf = MDAssy.Stiffener( aDC, aYVal );
            //            Stiffeners.Add( aStf );
            //            Stiffeners.GetBetweenOrdinate( dxfOrdinate_Y, aYVal + 3, aYVal - 3, aPartToSkip: aStf,  bRemove: true, aDowncomerID: aDC.Index );
            //            ret = true;
            //        }
            //    }
            //    if (bMirrors && aYVal != 0)
            //    {
            //        aYVal = -aYVal;
            //        if (Stiffeners.GetByOrdinate( dxfOrdinate_Y, aYVal, aDowncomerID: aDC.Index ).Count <= 0)
            //        {
            //            aStf = MDAssy.Stiffener( aDC, aYVal );
            //            Stiffeners.Add( aStf );
            //            Stiffeners.GetBetweenOrdinate( dxfOrdinate_Y, aYVal + 3, aYVal - 3,bOnIsIn: false, aPartToSkip: aStf, bRemove: true, aDowncomerID: aDC.Index );
            //       }
            //    }
            //}

            //if (ret)
            //{
            //    List<uopPart> aPrts = Stiffeners.GetByOrdinate(dxfOrdinate_Y, (double)Questions_ADD.Item(1).Answer, aDowncomerID: aDCIDs[0]);
            //    if (aPrts.Count <= 0)
            //    {
            //        aPrts = Stiffeners.GetByOrdinate(dxfOrdinate_Y, (double)Questions_ADD.Item(1).Answer);
            //    }
            //    if (aPrts.Count > 0)
            //    {
            //        Selected = (mdStiffener)aPrts[ 0 ];
            //    }
            //}

            //mdUtils.StiffenersSort( Stiffeners, MDAssy );
            //return ret;
        }


        private bool Execute_AddComplete(double aX, double aY)
        {
         

            mzQuestion q1 = null;
            mzQuestion q2 = null;
            mzQuestion q3 = null;

            aY = DisplayUnits == uppUnitFamilies.Metric ? uopUtils.RoundTo(aY * 25.4, dxxRoundToLimits.Millimeter) : uopUtils.RoundTo(aY, dxxRoundToLimits.Eighth);
            if (DisplayUnits == uppUnitFamilies.Metric) aY /= 25.4;

            if (Questions_ADD == null)
            {
                Questions_ADD = new mzQuestions("Enter Stiffener Y Location");
                q1 = Questions_ADD.AddNumeric("Y Value:", aY, LinearUnits.Label(DisplayUnits), LinearUnits.ConversionFactor(DisplayUnits), MaxOrds[MaxOrds.Count - 1], -MaxOrds[MaxOrds.Count - 1], uopValueControls.None, aMaxDecimals: DisplayUnits == uppUnitFamilies.English ? 4 : 1, bShowAllDigits: true);
                q2 = Questions_ADD.AddMultiSelect("Select Downcomers:", DCList, "All", 1);
                q3 =Questions_ADD.AddCheckVal("Add Mirrors?", !MDAssy.DesignFamily.IsBeamDesignFamily());
            }
            else
            {
                q1 = Questions_ADD.Item(1);
                q1.Suffix = LinearUnits.Label(DisplayUnits);
                q1.DisplayMultiplier = LinearUnits.ConversionFactor(DisplayUnits);
                q1.MaxDecimals = DisplayUnits == uppUnitFamilies.English ? 4 : 1;
                q1.SetAnswer(aY);
                q2 = Questions_ADD.Item(2);
                q3 = Questions_ADD.Item(3);
            }

        

            bool bCanc =!Questions_ADD.PromptForAnswers(  "", true,  true,  owner: MyWindow());
            if (bCanc) return false;


            
            List<int> aDCIDs = DCIndices(q2.Answers);
            if (aDCIDs.Count <= 0) return false;

            LastRun = Stiffeners.Clone();

            int i = 0;
            mdDowncomer aDC = null;
            double ord =  q1.AnswerD;
            mdStiffener aStf;
            bool bMirrors =  q3.AnswerB;

            List<mdStiffener> added = new List<mdStiffener>();
            for (i = 0; i < aDCIDs.Count; i++)
            {
                aDC = Downcomers[aDCIDs[i] - 1];
                double max = Math.Abs(MaxOrds[aDC.Index - 1]);
                if (Math.Abs(ord) <= max)
                {
                    if (Stiffeners.GetByOrdinate( dxxOrdinateDescriptors.Y, ord, aDowncomerID: aDC.Index).Count <= 0)
                    {
                     
                        Stiffeners.GetBetweenOrdinate( dxxOrdinateDescriptors.Y, ord + 3, ord - 3, aPartToSkip: null, bRemove: true, aDowncomerID: aDC.Index);
                        aStf = (mdStiffener)Stiffeners.Add(MDAssy.Stiffener(aDC, ord));
                        if(aStf != null) added.Add(aStf);
                      
                    }
                    if (bMirrors && ord != 0)
                    {
                        ord = -ord;
                        if (Stiffeners.GetByOrdinate( dxxOrdinateDescriptors.Y, ord, aDowncomerID: aDC.Index).Count <= 0)
                        {

                            aStf = aDC.Stiffener(ord);
                            
                            Stiffeners.GetBetweenOrdinate( dxxOrdinateDescriptors.Y, ord + 3, ord - 3, bOnIsIn: false, aPartToSkip: null, bRemove: true, aDowncomerID: aDC.Index);
                            aStf = (mdStiffener)Stiffeners.Add(aDC.Stiffener(ord));
                            if (aStf != null) added.Add(aStf);
                        }
                    }

                }
          
            }

            if (added.Count > 0)
            {
                mdUtils.StiffenersSort(Stiffeners, MDAssy);
                Selected = added[0];
              
            }

          
            return added.Count > 0;
        }


        private void Execute_SetY()
        {
            if (!IsEnabled) return;
            if (WaitingForClick) ResetWaitingForPoint();

            bool result = false;
            if (Selected == null) return;


            string dclist = GetSelectedDCListByOrdinate(out double ordinate, out List<uopPart> selected, out List<uopPart> sisters);
            if (string.IsNullOrWhiteSpace(dclist)) return;

            mdDowncomer aDC = Downcomers[Selected.DowncomerIndex - 1];
            if (aDC == null) return;
            
            double aMax = aDC.MaxStiffenerOrdinate(MDAssy);

            mzQuestions query = new();

            mzQuestion q1 = query.AddNumeric("Enter Y Value:", ordinate, LinearUnits.Label(DisplayUnits), LinearUnits.ConversionFactor(DisplayUnits), null, null, aMaxDecimals: 4);
            mzQuestion q2 = query.AddMultiSelect("Select Target Downcomers:",  dclist, aInitialAnswer:dclist, bAnswerRequired: true);
            mzQuestion q3 = sisters.Count <= 0 ? null : query.AddCheckVal($"Move Stiffeners At Y = { LinearUnits.UnitValueString(-1 * ordinate, DisplayUnits, bAddLabel: true)}?", false);
            
            bool canc = !query.PromptForAnswers( $"Move Stiffeners At Y = { LinearUnits.UnitValueString(ordinate, DisplayUnits, bAddLabel: true)}?", owner:MyWindow(), aSaveButtonText: "Move");

            if (canc) return;
           
            double aY = q1.AnswerD;
            if (Math.Abs(aY) == Math.Abs(ordinate)) return;

            List<int> dcids = GetDCIndices(q2.AnswerS);
            if (dcids.Count <= 0) return ; 

            LastRun = Stiffeners.Clone();

            List<uopPart> movers = new();
            foreach (var item in selected)
            {
                if (dcids.IndexOf(item.DowncomerIndex) >= 0) 
                {
                    double y = aY;
                    mdStiffener stiff = (mdStiffener)item;
                    aDC = Downcomers[stiff.DowncomerIndex - 1];

                    var closestBox = aDC.Boxes.Find((x) =>x.IsValidStiffenerOrdinate(y));
                    if (closestBox != null)
                    {
                        var temp = closestBox.TopMandatoryStiffenerRangeOrdinates().MaxY;
                        if (y > temp)
                        {
                            y = temp;
                        }
                        else
                        {
                            temp = closestBox.BottomMandatoryStiffenerRangeOrdinates().MinY;
                            if (y < temp)
                            {
                                y = temp;
                            }
                        }

                        stiff.Y = y;
                        result = true;
                    }
                }
            }
            if (q3 != null)
            {
                if (q3.AnswerB == true)
                {
                    foreach (var item in sisters)
                    {
                        if (dcids.IndexOf(item.DowncomerIndex) >= 0)
                        {
                            double y = -aY;

                            mdStiffener stiff = (mdStiffener)item;

                            var closestBox = aDC.FindClosestBoxToStiffenerOrdinate(y);
                            if (closestBox != null)
                            {
                                var temp = closestBox.TopMandatoryStiffenerRangeOrdinates().MaxY;
                                if (y > temp)
                                {
                                    y = temp;
                                }
                                else
                                {
                                    temp = closestBox.BottomMandatoryStiffenerRangeOrdinates().MinY;
                                    if (y < temp)
                                    {
                                        y = temp;
                                    }
                                }

                                stiff.Y = y;
                                result = true;
                            }

                        }
                    }

                }
            }


            if(result) mdUtils.StiffenersSort(Stiffeners, MDAssy);
            FinishCommand( result);
        }

        private void Execute_InputY()
        {
            if (WaitingForClick || !IsEnabled) return ;
            bool ret = false;
            double mx = MaxOrds.Max();
            mzQuestion q1;
            mzQuestion q2;
           
            string uname = DisplayUnits == uppUnitFamilies.Metric ? "Millimeters" : "Inches";
            if (Questions_YVALS == null)
            {
                Questions_YVALS = new mzQuestions();
                q1 = Questions_YVALS.AddMultiSelect("Select Downcomers:", DCList, "All", 1);
                q2 = Questions_YVALS.AddNumericList("Enter Y Values:", null, LinearUnits.Label(DisplayUnits), LinearUnits.ConversionFactor(DisplayUnits), -1, null, null, 3, uopValueControls.None, aMaxDecimals: 4, bAddMirrors: !MDAssy.DesignFamily.IsBeamDesignFamily(), bShowAllDigits: true);
            }
            else
            {
                q1 = Questions_YVALS.Item(1);
                q2 = Questions_YVALS.Item(2);
            }

            q2.Suffix = LinearUnits.Label(DisplayUnits);
            q2.DisplayMultiplier = LinearUnits.ConversionFactor(DisplayUnits);
            q2.MaxDecimals = DisplayUnits == uppUnitFamilies.Metric ? 1 : 4;
            //q2.SetAnswer("");
            
            Questions_YVALS.Title = $"Enter Stiffener Y Locations (UNITS = {uname})";

            bool bCanc = !Questions_YVALS.PromptForAnswers(  "", true,  owner:MyWindow());
            mzValues aDCList = q1.Answers;
            List<double> aYVals = q2.Answers.ToNumericList();


            if (bCanc || aDCList.Count <= 0) return;

            LastRun = Stiffeners.Clone();

            List<int> DCIDs = DCIndices(aDCList);

            for (int i = 1; i <= aDCList.Count; i++)
            {
                int dcidx = DCIDs[i - 1];
                mdDowncomer aDC = Downcomers[dcidx - 1];
                double aMax = MaxOrds[dcidx - 1];
                if (Stiffeners.GetByDowncomerIndex(aDC.Index, bRemove: true).Count > 0) ret = true;


                for (int j = 0; j < aYVals.Count; j++)
                {

                    double aYVal = aYVals[j];
                    bool bkeep = true;

                    var closestBox = aDC.FindClosestBoxToStiffenerOrdinate(aYVal);
                    if (closestBox != null)
                    {
                        if (MDAssy.DesignFamily.IsEcmdDesignFamily())
                        {
                            var temp = closestBox.TopMandatoryStiffenerRangeOrdinates().MaxY;
                            if (aYVal > temp)
                            {
                                aYVal = temp;
                            }
                            else
                            {
                                temp = closestBox.BottomMandatoryStiffenerRangeOrdinates().MinY;
                                if(aYVal < temp)
                                {
                                    aYVal = temp;
                                }
                            }
                        }
                        else
                        {
                            bkeep = closestBox.IsValidStiffenerOrdinate(aYVal);
                        }
                    }
                    else
                    {
                        bkeep = false;
                    }

                    if (bkeep)
                    {
                        Stiffeners.Add(aDC.Stiffener(aYVal));
                        ret = true;
                    }
                }
            }

            mdUtils.StiffenersSort(Stiffeners, MDAssy);
           FinishCommand(ret);
        }

        private void Execute_UniformSpace()
        {
            if (!IsEnabled) return;
            if (WaitingForClick) ResetWaitingForPoint();

            List<double> aVals;
            double mx;
            List<int> aDCList;
            int dcidx;
            mdDowncomer aDC;
            double aYVal;
            mdStiffener aStf;

            if (Questions_UNI == null)
            {
                Questions_UNI = new mzQuestions();

                mx = MDAssy.RingID / 2;


                Questions_UNI.Title = "Enter Stiffener Spacing";
                Questions_UNI.AddNumeric("Current Spacing:", Spacing, LinearUnits.Label(DisplayUnits) , LinearUnits.ConversionFactor(DisplayUnits) , MDAssy.RingID / 2, 3.5, uopValueControls.PositiveNonZero, aMaxDecimals: 4, bShowAllDigits: true);
                Questions_UNI.AddCheckVal("Straddle Center Line?", Straddle);
                Questions_UNI.AddCheckVal("Exact Space?", !BestFit);
                Questions_UNI.AddMultiSelect("Select Downcomers:", DCList, "All", 1);
            }

            bool bCanc = !Questions_UNI.PromptForAnswers( "", true,  owner:MyWindow());
            if (bCanc) return;

            LastRun = Stiffeners.Clone();

            Spacing = double.Parse(Questions_UNI.Item(1).Answer.ToString());
            Straddle = bool.Parse(Questions_UNI.Item(2).Answer.ToString());
            BestFit = !bool.Parse(Questions_UNI.Item(3).Answer.ToString());
            aDCList = DCIndices(Questions_UNI.Item(4).Answers);

            for (int i = 0; i < aDCList.Count; i++)
            {
                dcidx = aDCList[i];
                aDC = Downcomers[dcidx -1];
                Stiffeners.GetByDowncomerIndex(dcidx, bRemove: true);
                aVals = mdUtils.StiffenersGenerate(Spacing, aDC, MDAssy, Straddle, BestFit);
                for (int j = 0; j < aVals.Count; j++)
                {
                    aYVal = aVals[j];
                    aStf = aDC.Stiffener(aYVal);
                    Stiffeners.Add(aStf);
                }
            }

            FinishCommand( true);
        }

        private void Execute_Straddle()
        {

            if (!IsEnabled) return;
            if (WaitingForClick) ResetWaitingForPoint();

            //if (Questions_STRADDLE != null)
            {
                Questions_STRADDLE = new mzQuestions("Input Function Parameters");
                Questions_STRADDLE.AddNumeric("Downcomer Clearance:", 1, LinearUnits.Label(DisplayUnits) , LinearUnits.ConversionFactor(DisplayUnits), 0.5 * MDAssy.DeckSectionWidth(false) - 1.5, null, uopValueControls.PositiveNonZero, aMaxDecimals: 4, bShowAllDigits: true);
            }

            bool bCanc = !Questions_STRADDLE.PromptForAnswers("", true, owner: MyWindow());

            if (bCanc) return;
            LastRun = Stiffeners.Clone();
            Stiffeners = mdUtils.StiffenersStraddleDowncomers(Downcomers, MDAssy, double.Parse(Questions_STRADDLE.Item(1).Answer.ToString()));

            FinishCommand(true);
        }

        private void Execute_CopyTray()
        {

            if (WaitingForClick || !IsEnabled) return ;
            if (TrayNames.Count <= 0) return ;
            if (Questions_TRAY == null)
            {
                Questions_TRAY = new mzQuestions();
                Questions_TRAY.AddSingleSelect("Select a Tray:", mzUtils.ListToString(TrayNames), TrayNames[0], true);
            }

            bool bCanc = !Questions_TRAY.PromptForAnswers( "Select a Tray To Copy Stiffener Locations From", true, owner:MyWindow());
            if (bCanc) return;

            LastRun = Stiffeners.Clone();

            string tname = Questions_TRAY.Item(1).Answer.ToString().Trim();

            colUOPTrayRanges aTrays = MDProject.TrayRanges;
            

            uopTrayRange range = aTrays.Find(x => string.Compare(x.TrayName(true), tname, true) == 0);
            if (range == null) return;
          
            mdTrayRange aTray = (mdTrayRange)range;

            Stiffeners = mdUtils.StiffenersCopy(MDAssy, aTray.TrayAssembly);
            FinishCommand( true);
        }

        public void Execute_Delete (List<uopPart> aParts = null )
        {
            if (!IsEnabled) return;
            if (WaitingForClick) ResetWaitingForPoint();

            if (aParts == null)
            {
                string dclist = GetSelectedDCListByOrdinate(out double ordinate, out List<uopPart> selected, out List<uopPart> sisters);
                if (string.IsNullOrWhiteSpace(dclist)) return;

                mzQuestion q1 = null;
                mzQuestion q2 = null;
                mzQuestions query = new();
                q1 =query.AddMultiSelect("Select Target Downcomers:", dclist, aInitialAnswer: "All", bAnswerRequired: true);
                if (sisters.Count > 0)
                {
                    q2 = query.AddCheckVal($"Delete Stiffeners At Y = { LinearUnits.UnitValueString(-1 * ordinate, DisplayUnits, bAddLabel: true)}?", !MDAssy.DesignFamily.IsBeamDesignFamily());
                }
                bool canc = !query.PromptForAnswers($"Delete Stiffeners At Y = {LinearUnits.UnitValueString(ordinate, DisplayUnits, bAddLabel: true)}?", owner: MyWindow(), aSaveButtonText: "Delete");

                if (canc) return;
                

                List<int> dcids = GetDCIndices(q1.AnswerS);
                if (dcids.Count  <= 0) return;

                aParts = new List<uopPart>();
                foreach (var item in selected)
                {
                    if (dcids.IndexOf(item.DowncomerIndex) >= 0) aParts.Add(item);
                }
                if (q2 != null)
                {
                    if (q2.AnswerB == true)
                    {
                        foreach (var item in sisters)
                        {
                            if (dcids.IndexOf(item.DowncomerIndex) >= 0) aParts.Add(item);
                        }

                    }
                }
            }

            if (aParts.Count <= 0) return;

            int idx;
            mdStiffener stiff;
           
            List<int> delete = new();

            for (int i = 0; i < aParts.Count; i++)
            {
                stiff = (mdStiffener)aParts[i];
                idx = stiff.Index;
                if(idx > 0 && idx <= Stiffeners.Count)
                {
                    if (delete.IndexOf(idx) < 0) delete.Add(idx);

                }
            }
            if (delete.Count <= 0) return;
           
            LastRun = Stiffeners.Clone();
            FinishCommand( Stiffeners.Remove(delete).Count > 0);
        }

        private void Execute_Redraw()
        {
            if (!IsEnabled) return;
            if (WaitingForClick) ResetWaitingForPoint();

            Viewer.Clear();
            Image = null;

            BaseImage?.Dispose();
            BaseImage = null;
            RefreshDisplay();
        }

        private void Execute_Undo()
        {
            if (Runs.Count <= 0) return;
            if (!IsEnabled) return;
            if (WaitingForClick) ResetWaitingForPoint();

            Redos.Add(Stiffeners.Clone());
            Stiffeners = Runs.Last();
            if (Runs.Count > 1) Runs.RemoveAt(Runs.Count - 1);
            FinishCommand(true);
        }

        private void Execute_Redo()
        {
            if (Redos.Count <= 0) return;
            if (!IsEnabled) return;
            if (WaitingForClick) ResetWaitingForPoint();

            LastRun = Stiffeners.Clone();
            Stiffeners = Redos.Last();
            Redos.RemoveAt(Redos.Count - 1);

            FinishCommand(true);
        }


        private void Execute_Reset()
        {
            if (!IsEnabled) return;
            if (WaitingForClick) ResetWaitingForPoint();


            bool edited = !Stiffeners.Centers().IsEqual(OriginalStiffeners.Centers(), aPrecis: 3) || Spacing != MDAssy.DesignOptions.StiffenerSpacing;
            if (edited)
            {
                if (MessageBox.Show("Reset Stiffeners to the original arrangement?", "Reset", button: MessageBoxButton.YesNo, icon: MessageBoxImage.Question, defaultResult: MessageBoxResult.Yes) != MessageBoxResult.Yes) return;
                Stiffeners = OriginalStiffeners.Clone();
                Runs = new List<colUOPParts>();
                Redos = new List<colUOPParts>();
                Spacing = MDAssy.DesignOptions.StiffenerSpacing;
                SaveRun(OriginalStiffeners.Clone());
                FinishCommand(true);
        
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
        private void FinishCommand(bool bExecuted)
        {
            if (bExecuted)
            {
                SaveRun(LastRun);

                RefreshDisplay();
            }
        }


        private List<int> DCIndices(mzValues aDCList)
        {
            var ret = new List<int>();
            if (aDCList == null) return ret;

            
            string aStr;
            int j;
          
            for (int i = 1; i <= aDCList.Count; i++)
            {
                aStr = aDCList.Item(i);
                j = aStr.LastIndexOf(" ");
                aStr = aStr.Substring(j).Trim();

                if (int.TryParse(aStr, out int dcidx))
                {
                    if (dcidx > 0 && dcidx <= Downcomers.Count)
                        ret.Add(dcidx);
                }
            }
            return ret;
        }

        private void UnitChange()
        {

            if (WaitingForClick || !IsEnabled) return ;

            NotifyPropertyChanged("DisplayUnits");
            NotifyPropertyChanged("IsEnglishSelected");
            Questions_YVALS = null;
            Questions_SPACING = null;
            Questions_STRADDLE = null;
            Questions_ADD = null;
            Questions_UNI = null;
            if (Inited) RefreshDisplay();
            // NotifyPropertyChanged("DisplayUnits");
            // NotifyPropertyChanged("IsEnglishSelected");
            //if (IsEnabled) Redraw(bDataOnly: true);



        }

        public override void Activate(System.Windows.Window myWindow)
        {
            if (Activated) return;
            base.Activate(myWindow);
            SuppressEvents = false;
            DialogResult = null;
            myWindow.Top = 20;
            myWindow.Left = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width / 2 - myWindow.Width / 2;
            myWindow.Visibility = Visibility.Visible;

            Edited = false;
            //IsEnabled = true;
            Canceled = true; //myWindow.Dispatcher.Invoke(InitializeView, DispatcherPriority.Render);
        }



        private void Execute_CreatePersistentEntities()
        {

            uopDocDrawing dwg = new uopDocDrawing(uppDrawingFamily.Sketch, "Stiffener Edit Sketch", uppDrawingTypes.StiffenerEdit , MDRange, bZoomExtents: false, bNoText: true);
                  
            AppDrawingMDDHelper helper = new AppDrawingMDDHelper();
            BaseImage = new dxfImage();
            helper.Image = BaseImage;
            helper.GenerateImage(dwg);
            DCRegions = new colDXFRectangles();
            double paperscale = BaseImage.Display.PaperScale;

            foreach (dxfEntity ent in BaseImage.Entities) 
            {
                switch (ent.LayerName.ToUpper())
                {
                    case "DOWNCOMERS":
                        {
                            DCRegions.Add(ent.BoundingRectangle().Stretched(paperscale * 0.05 * 2));
                            break;
                        }
                }
            }

  
            MaxOrds = new List<double>();
            MinOrds = new List<double>();

            for (int i = 1; i <= Downcomers.Count; i++)
            {
                mdDowncomer dc = Downcomers[i - 1];
            

                DCList = (i > 1) ? $"{DCList},Downcomer {i}" : $"Downcomer { i}";
             

                MaxOrds.Add(dc.MaxStiffenerOrdinate(MDAssy));
                MinOrds.Add(dc.MinStiffenerOrdinate(MDAssy));
            }

            StiffenerShape = new dxePolyline(MDAssy.Downcomer().Stiffener().Vertices(bInclueFillets: false, aCenter: dxfVector.Zero), false, new dxfDisplaySettings("0", dxxColors.Blue, dxfLinetypes.Continuous));

        }

        public bool Execute_DrawObjects()
        {
            bool firstImage = Image == null;
            Downcomers ??= MDAssy.Downcomers.GetByVirtual(aVirtualValue: false);
            StiffenerShape ??= new dxePolyline(MDAssy.Downcomer().Stiffener().Vertices(bInclueFillets: false, aCenter: dxfVector.Zero), false, new dxfDisplaySettings("0", dxxColors.Blue, dxfLinetypes.Continuous));
            StartUps ??= MDAssy.Downcomers.StartupSpouts(MDAssy, bReturnMirrored: true, bGetClones: true);

            Regions = new colDXFRectangles();
            if (Selected == null)
                MousePt = null;
            else
                MousePt.MoveTo(Selected.CenterDXF);
     
            if (!firstImage)
                   CurrentView = Viewer.CurrentZoomData;
        
            if (firstImage)
            {

                Image = new dxfImage(appApplication.SketchColor);
                Image.Header.UCSMode = dxxUCSIconModes.None;

                
                dxoLayers lyrs = Image.Layers;
                lyrs.Append(BaseImage.Layers,true);

                Image.UsingDxfViewer = true;
                Image.Layers.Add("STIFFENERS", dxxColors.Blue);

              
                dxoStyle tstyle = Image.Screen.TextStyle;
                dxoStyle basetstyle = BaseImage.TextStyle();
                tstyle.WidthFactor = basetstyle.WidthFactor;
                tstyle.FontName = basetstyle.FontName;
                //tstyle =Image.TextStyle();
                //tstyle.WidthFactor = 0.8;
                //tstyle.FontName = "RomanS.shx";

                bool bBafLines = MDAssy.DesignFamily.IsEcmdDesignFamily();
                double d1 = MDAssy.Downcomer().BoxWidth + 3;

                //static stuff
                Image.Entities.Append(BaseImage.Entities, bAddClones: true);
                Image.Display.ZoomExtents(bSetFeatureScale: true);
                dxoDisplay display = Image.Display;
                display.Size = DisplayWindowSize;
                display.ZoomToLimits();
                PixPerInch = display.PixelsPerInch(true);

            }

            List<dxePolyline> sus = Image.Entities.Polylines().FindAll(x => string.Compare(x.LayerName, "STARTUPS", true) == 0);

            //=========== the changing  stuff
            dxoDrawingTool draw = Image.Draw;
            //dxoScreenEntities screenents = Image.Screen.Entities;
            //screenents.Clear();
            double  paperscale = Image.Display.PaperScale;

           Image.Entities.RemoveMembers(StiffenerEnts);
            StiffenerEnts = new List<dxfEntity>();


            dxfRectangle stfrec =  StiffenerShape.BoundingRectangle();
            stfrec.Height = 3 * stfrec.Height;
            stfrec.Width = 1.5 * stfrec.Width;

      

            foreach (mdDowncomer dc in Downcomers)
            {
                List<uopPart> dcStfs = Stiffeners.GetByDowncomerIndex(dc.Index);
                //========= set startup colors based on obscuration by a stiffener 
              
                StartUps.SetObscured(MDAssy, Stiffeners);
                mdStartupSpouts aSUSs = StartUps.GetByDowncomerIndex(dc.Index, 0);
                for (int i = 1; i <= aSUSs.Count; i++)
                {
                    mdStartupSpout aSU = aSUSs.Item(i);
                    dxePolyline subound = sus.Find(x => x.Tag == dc.Index.ToString() && x.Flag == i.ToString());  //i = downcomer index i = index inside the downcomer suset
                    if (subound != null) subound.Color = !aSU.Obscured ? dxxColors.BlackWhite : dxxColors.Red;
                }

                dxfDisplaySettings dsp = new("STIFFENERS", dxxColors.ByLayer, dxfLinetypes.Continuous);


                //========= stiffeners
                for (int i = 1; i <= dcStfs.Count; i++)
                {
                    RightStatusBarLabelText = $"Generating Stiffeners {i} for Downcomer {dc.Index}...";
                    mdStiffener stiffener = (mdStiffener)dcStfs[i - 1];
                    stfrec.SetCoordinates(stiffener.X, stiffener.Y);
                    stfrec.Value = stiffener.Y;
                    stfrec.Tag = dc.Index.ToString();
                    stfrec.Flag = i.ToString();


                    if (MousePt == null)
                    {
                        MousePt = new dxfVector();
                        MousePt.SetCoordinates(stiffener.X, stiffener.Y);
                    }

                    uopVector u1 = new uopVector(stiffener.Center);

                    dxePolyline pl = StiffenerShape.Clone();
                    pl.Translate(u1);

                    dxfEntity ent = Image.Entities.Add(pl, bAddClone: false);
                    ent.Tag = $"DOWCOMER {dc.Index}";
                    StiffenerEnts.Add(ent);

                    
                    Regions.Add(stfrec, 0, 0, true);
                    u1 = new uopVector(stfrec.Left, stfrec.Y);
                    u1.X -= 0.08 * paperscale;



                    string txt = (stiffener.Y * LinearUnits.ConversionFactor(DisplayUnits)).ToString(DisplayUnits == uppUnitFamilies.Metric ? "#,0.0" : "#,0.0###");
                    ent = draw.aText(u1, txt, 0.09 * paperscale, aAlignment: dxxMTextAlignments.MiddleRight);
                    ent.Tag = $"DOWCOMER {i}";

                    StiffenerEnts.Add(ent);

                    stiffener.Tag = $"{stiffener.Tag},{txt}";
                }

            }
            RightStatusBarLabelText = $"Generated {Stiffeners.PartCount( uppPartTypes.Stiffener, true )} Stiffeners. Rendering Drawing...";
            ImageOverlayTotalLabelText = $"Total={Stiffeners.PartCount(uppPartTypes.Stiffener, true)}";

            return firstImage;
        }


        private void SaveRun(colUOPParts aRun)
        {
            if (Runs == null)  return;
            
            if (aRun != null)
            {
                if (Runs.Count > 40)
                {
                    Runs.RemoveAt(0);
                }
                Runs.Add(aRun);
            }

            CommandManager.InvalidateRequerySuggested();
        }

        private void HighLight()
        {

            if (Regions == null || DCRegions == null || Viewer == null) return;

          
            Viewer.overlayHandler.ClearRectangles();

            Selected = null;
            ImageOverlayLocationLabelText = "";
            DCRegion = null;

            uopPart selpart = Stiffeners.GetByNearestCenter(MousePt, aPartType: uppPartTypes.Stiffener);
            if (selpart == null && Stiffeners.Count > 0) selpart = Stiffeners.Item(1);
            if (selpart == null) return;



            int recID = Stiffeners.IndexOf(selpart);

            Selected = (mdStiffener)selpart;
            ImageOverlayLocationLabelText = $"Location: {LinearUnits.UnitValueString(Selected.Y, DisplayUnits, bAddLabel: false)}";

            dxfRectangle stfrec = Regions.Item(recID);
            DCRegion = FindSelectedStiffenerRegion(Selected, DCRegions);
             Viewer.overlayHandler.DrawRectangle( DCRegion, clr: System.Drawing.Color.Orange, lineWeight: 50, ignoreExisting: true );
             Viewer.overlayHandler.DrawRectangle(stfrec, clr: System.Drawing.Color.Yellow, lineWeight: 50, ignoreExisting: true);


            List<dxfRectangle> sisters = Regions.FindAll((x) => Math.Abs(x.Value) == Math.Abs(stfrec.Value));
            foreach (dxfRectangle rect in sisters)
            {
                if(Regions.IndexOf(rect) != recID)
                {
                    Viewer.overlayHandler.DrawRectangle(rect, clr: System.Drawing.Color.LightYellow, lineWeight: 50, ignoreExisting:true);
                }
            }

            

            //dxfVector v1 = new dxfVector(Selected.X,-Selected.Y);

            //if (v1.Y != 0)
            //{
            //    IntHelper recIDhelper = new IntHelper();
            //    Sister = (mdStiffener)Stiffeners.GetAtCenter(v1, recIDhelper,aPrecis:2, bIgnoreZ:true);

            //    if (Sister != null) =
            //    {
            //        stfrec = Regions.Item(recIDhelper.Index);
            //        SisterScreenHandle = Viewer.overlayHandler.DrawRectangle( stfrec, System.Drawing.Color.Yellow, 50, SisterScreenHandle );
            //    }
            //}
            //else
            //{
            //    //overlap the selected rectangle to effectively hide it without resetting the handle
            //    SisterScreenHandle = Viewer.overlayHandler.DrawRectangle( stfrec, System.Drawing.Color.Yellow, 50, SisterScreenHandle );
            //}
                
            
            CommandManager.InvalidateRequerySuggested();
        }

        private dxfRectangle FindSelectedStiffenerRegion(mdStiffener stiffener, colDXFRectangles regions)
        {
            var groupByDowncomers = regions.GroupBy(r => r.X);

            int dcIndex = 0;
            foreach (var dcRegions in groupByDowncomers)
            {
                dcIndex++;
                if (dcIndex == stiffener.DowncomerIndex)
                {
                    foreach (var dcBoxRegion in dcRegions)
                    {
                        if (dcBoxRegion.Bottom <= stiffener.Y && dcBoxRegion.Top >= stiffener.Y)
                        {
                            return dcBoxRegion;
                        }
                    }
                }
            }

            return null;
        }
      
        private void RefreshDisplay()
        {
            if (_Worker_DrawObjects.IsBusy) return;
            ShowProgress = true;
            _Worker_DrawObjects.RunWorkerAsync();
        }


        private string GetSelectedDCListByOrdinate(out double rOrdinate, out List<uopPart> rSelected, out List<uopPart> rSisters)
        {
            rOrdinate = 0;
            string _rVal = "";
            rSelected = new List<uopPart>();
            rSisters = new List<uopPart>();

            if (Selected == null) return "";
            double ordinate = Selected.Y;

            rSelected = Stiffeners.GetByOrdinate(dxxOrdinateDescriptors.Y, ordinate);

            if (rSelected.Count <= 0) return "";
            rSisters = Stiffeners.GetByOrdinate(dxxOrdinateDescriptors.Y, -ordinate);
            List<int> dcids = new();
            foreach (var item in rSelected)
            {
                if (dcids.IndexOf(item.DowncomerIndex) < 0) dcids.Add(item.DowncomerIndex);
            }
            foreach (var item in rSisters)
            {
                if (dcids.IndexOf(item.DowncomerIndex) < 0) dcids.Add(item.DowncomerIndex);
            }



            dcids.Sort();
            foreach (var item in dcids) { mzUtils.ListAdd(ref _rVal, $"Downcomer {item}"); }
            rOrdinate = ordinate;
            return _rVal;
        }

        
        private List<int> GetDCIndices(string aInput)
        {
            List<int> _rVal = new();
            if (string.IsNullOrWhiteSpace(aInput)) return _rVal;
            List<string> dclist = mzUtils.StringsFromDelimitedList(aInput);
            int idx = 0;
            foreach (var item in dclist)
            {
                idx = mzUtils.ExtractInteger(item,out _,out _);
                if(idx > 0 && idx <= Downcomers.Count)
                {
                    if (_rVal.IndexOf(idx) < 0) _rVal.Add(idx);
                }
            }
            _rVal.Sort();
            return _rVal;
        }

        private void ShowData()
        {


            MaintainSymmetryIsChecked = mdUtils.StiffenersAreSymetric(MDAssy, Stiffeners);

            DowncomerLimits?.Clear();
            var dclimits = new ObservableCollection<DowncomerLimitDataGridViewModel>();

            foreach (var downcomer in MDAssy.Downcomers)
            {
                var boxLimits = downcomer.Boxes.Where(b => !b.IsVirtual).Select(b =>
                {
                    (var topMaxY, var topMinY) = b.TopMandatoryStiffenerRangeOrdinates();
                    (var bottomMaxY, var bottomMinY) = b.BottomMandatoryStiffenerRangeOrdinates();

                    return (b.Index, topMaxY, bottomMinY);
                }).ToArray();

                foreach (var boxLimit in boxLimits)
                {
                    dclimits.Add(new DowncomerLimitDataGridViewModel() { LimitId = $"{downcomer.Index}-{boxLimit.Index}", MaximumY = LinearUnits.UnitValueString(boxLimit.topMaxY, DisplayUnits, bAddLabel: false), MinimumY = LinearUnits.UnitValueString(boxLimit.bottomMinY, DisplayUnits, bAddLabel: false) });
                }
            }
            
            DowncomerLimits = dclimits;




        }

        public override void ReleaseReferences()
        {
            try
            {
                Viewer = null;
                base.ReleaseReferences();
                Stiffeners = null;
                Downcomers = null;
                Regions = null;
                DCRegions = null;
                Selected = null;
                ManwaySections = null;
                  MousePt = null;
                StartUps = null;
           
                _Worker_DrawObjects.DoWork -= DoWork_DrawObjects;
                _Worker_DrawObjects.RunWorkerCompleted -= DoWork_DrawObjects_Complete;

                BaseImage?.Dispose();
                BaseImage = null;
                StiffenerShape = null;
            }
            catch (Exception) { }
        }

        #endregion Methods

    }

    public class DowncomerLimitDataGridViewModel : BindingObject, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Implementation
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

                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch { return false; }


        }
       
        #endregion  INotifyPropertyChanged Implementation

        private string _LimitId;
        public string LimitId
        {
            get => _LimitId;
            set { _LimitId = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); }
        }

        private string _MaximumY;
        public string MaximumY
        {
            get => _MaximumY;
            set { _MaximumY = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); }
        }

        private string _MinimumY;
        public string MinimumY
        {
            get => _MinimumY;
            set { _MinimumY = value; NotifyPropertyChanged(MethodBase.GetCurrentMethod()); }
        }
    }

}
