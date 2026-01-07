using UOP.DXFGraphics;
using UOP.WinTray.UI.Views;
using MvvmDialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UOP.WinTray.Projects;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Parts;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.UI.Events.EventAggregator;
using UOP.WinTray.UI.Messages;
using UOP.WinTray.UI.Utilities;
using Unity;
using UOP.WinTray.Projects.Documents;

namespace UOP.WinTray.UI.ViewModels
{
    /// <summary>
    /// This class provides base class for all View Models
    /// </summary>
    public abstract class ViewModel_Base : BindingObject
    {


        protected const string NA = "N/A";
        protected const string IS = "Is ";
        protected const string FOCUSED = " Focused";


        #region Constructors

        internal ViewModel_Base()
        {
            Edited = false;
            Canceled = true;
            DialogService = null;

            EventAggregator = null;


            Units_Linear = uopUnits.GetUnit(uppUnitTypes.SmallLength);
            Units_Area = uopUnits.GetUnit(uppUnitTypes.SmallArea);
        }

        internal ViewModel_Base(IEventAggregator eventAggregator = null, IDialogService dialogService = null, uopProject project = null, ViewModel_Base parentVM = null)
        {
            Edited = false;
            Canceled = true;
            DialogService = dialogService;
            _ParentVM_Ref = new WeakReference<ViewModel_Base>(parentVM);
            Project = project;
            EventAggregator = eventAggregator;

            EventAggregator?.Subscribe(this);
            Units_Linear = uopUnits.GetUnit(uppUnitTypes.SmallLength);
            Units_Area = uopUnits.GetUnit(uppUnitTypes.SmallArea);
            VisibilityDeveloper = uopUtils.RunningInIDE ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion Constructors


        #region Properties

        private IDialogService _DialogService;
        public virtual IDialogService DialogService { get { return _DialogService;  } set => _DialogService = value; }


        internal virtual IEventAggregator EventAggregator { get; set; }

        public bool Activated { get; set; }

        protected dxfImage _Image = null;
        public virtual dxfImage Image
        {
            get => _Image;
            set
            {
                if (_Image != null && value == null) _Image.Dispose();
                if (_Image != null && value != null)
                {
                    if (_Image.GUID != value.GUID) { _Image.Dispose(); }
                };
                _Image = value;

                NotifyPropertyChanged("Image");
            }
        }

        private Visibility _VisibilityDeveloper;
        public virtual Visibility VisibilityDeveloper
        {
            get => _VisibilityDeveloper;
            set { _VisibilityDeveloper = value; NotifyPropertyChanged("VisibilityDeveloper"); }

        }

        private Visibility _VisibilityViewer = Visibility.Collapsed;
        public virtual Visibility Visibility_Viewer
        {
            get => _VisibilityViewer;
            set { _VisibilityViewer = value; NotifyPropertyChanged("Visibility_Viewer"); }
        }


        private Visibility _VisibilityViewerProgress = Visibility.Collapsed;
        public Visibility Visibility_ViewerProgress
        {
            get => _VisibilityViewerProgress;
            set
            {
                _VisibilityViewerProgress = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod());
            }
        }

        public virtual UOP.DXFGraphicsControl.DXFViewer Viewer { get; set; }


        public virtual dxfVector MousePoint { get; set; }

        public uopUnit Units_Linear { get; set; }
        public uopUnit Units_Area { get; set; }

        private bool _IsEnabled;
        public virtual bool IsEnabled
        {
            get => _IsEnabled;
            set
            {

                _IsEnabled = value;
                NotifyPropertyChanged("IsEnabled");
                NotifyPropertyChanged("IsBusyVisible");
                NotifyPropertyChanged("IsCancelBtnEnable");
                NotifyPropertyChanged("IsOkBtnEnable");

            }
        }

        private WeakReference<ViewModel_Base> _ParentVM_Ref;

        public virtual ViewModel_Base ParentVM
        {
            get
            {
                if (_ParentVM_Ref == null) return null;
                if (!_ParentVM_Ref.TryGetTarget(out ViewModel_Base _rVal)) _ParentVM_Ref = null;
                return _rVal;
            }
            set => _ParentVM_Ref = value != null ? new WeakReference<ViewModel_Base>(value) : null;

        }

        /// <summary>
        /// the window heading
        /// </summary>       
        private string _Title = "";
        public virtual string Title
        {
            get => _Title;
            set { _Title = value; NotifyPropertyChanged("Title"); }
        }

        /// <summary>
        /// Property to get/set Global title which is a combination of Project Type, KeyNumber and Rev
        /// </summary>       
        private string _GlobalProjectTitle = "";
        public string GlobalProjectTitle
        {
            get => _GlobalProjectTitle;
            set { _GlobalProjectTitle = value; NotifyPropertyChanged("GlobalProjectTitle"); }
        }

        private bool _IsDirty = false;
        public virtual bool IsDirty
        {
            get => _IsDirty;
            set { _IsDirty = value; if (ParentVM != null) ParentVM.IsDirty = true; }
        }

        private Visibility _VisibilityMDProject = Visibility.Collapsed;
        public virtual Visibility VisibilityMDProject
        {
            get => _VisibilityMDProject;
            set { _VisibilityMDProject = value; NotifyPropertyChanged("VisibilityMDProject"); }

        }

        private Visibility _VisibilityProject = Visibility.Collapsed;
        public virtual Visibility VisibilityProject
        {
            get => _VisibilityProject;
            set { _VisibilityProject = value; NotifyPropertyChanged("VisibilityProject"); }

        }

        private uopProject _Project = null;
        public virtual uopProject Project
        {
            get => _Project;
            set
            {
                if (_Project == value)
                {
                    _Trays = null;
                    return;
                }
                _Project = value; NotifyPropertyChanged("Project");
                if (_Project != null)
                {
                    Trays = _Project.TrayRanges;
                    VisibilityProject = Visibility.Visible;
                }
                else
                {
                    Trays = null;
                    VisibilityProject = Visibility.Collapsed;
                }


            }


        }
        public virtual mdProject MDProject
        {
            get => (Project != null) ? (Project.ProjectFamily == uppProjectFamilies.uopFamMD) ? (mdProject)Project : null : null;
            set => Project = value;


        }

        private colUOPTrayRanges _Trays;
        public virtual colUOPTrayRanges Trays
        {
            get {  if(_Trays ==null && Project != null)Trays = Project.TrayRanges; return     _Trays;}
            set { _Trays = value; NotifyPropertyChanged("Trays"); TrayList = _Trays == null ? new ObservableCollection<uopPart>() : _Trays.ToObservable();  }
        }

        private ObservableCollection<uopPart> _TrayList;
        public ObservableCollection<uopPart> TrayList
        {
            get => _TrayList;
            set
            {
                _TrayList = value;
                NotifyPropertyChanged("TrayList");
            }
        }

        public string ProjectName { get => _Project != null ? _Project.Name : ""; }

        public uppProjectTypes ProjectType
        {
            get => _Project != null ? _Project.ProjectType : uppProjectTypes.Undefined;

        }

        /// <summary>
        /// Validation Error message
        /// </summary>
        private string _ErrorMessage = "";
        public virtual string ErrorMessage
        {
            get => _ErrorMessage;
            set { _ErrorMessage = value; NotifyPropertyChanged("ErrorMessage"); VisibilityErrorMessage = string.IsNullOrWhiteSpace(value) ? Visibility.Collapsed : Visibility.Visible; }
        }

        private Visibility _VisibilityErrorMessage = Visibility.Collapsed;
        public virtual Visibility VisibilityErrorMessage { get => _VisibilityErrorMessage; set { _VisibilityErrorMessage = value; NotifyPropertyChanged("VisibilityErrorMessage"); } }

        /// <summary>
        /// Validation Error message
        /// </summary>
        private string _ErrorString = "";
        public string ErrorString
        {
            get => _ErrorString;
            set { _ErrorString = value; NotifyPropertyChanged("ErrorString"); }
        }

        private bool _Canceled = false;
        public bool Canceled { get => _Canceled; set { _Canceled = value; NotifyPropertyChanged("Canceled"); } }

        private bool _Edited = false;
        public bool Edited { get => _Edited; set { _Edited = value; NotifyPropertyChanged("Edited"); }
}

public uppProjectFamilies ProjectFamily {  get => (_Project != null) ? _Project.ProjectFamily : uppProjectFamilies.Undefined;  }


        public bool IsSpoutProject => ProjectType == uppProjectTypes.MDSpout;

        /// <summary>
        ///the display units of the underlying mdProject
        ////// </summary>
        public uppUnitFamilies ProjectDisplayUnits => (_Project != null) ? _Project.DisplayUnits : uppUnitFamilies.English;

        /// <summary>
        /// dispay the busy indicator
        /// </summary>
    
        public virtual bool IsBusyVisible
        {
            get => !IsEnabled;
            
        }

       
        /// <summary>
        /// Busy message to show
        /// </summary>
        private string _BusyMessage = "";
        public virtual string BusyMessage
        {
            get => _BusyMessage; 
            set { _BusyMessage = value ?? string.Empty; NotifyPropertyChanged("BusyMessage"); }
        }

        public Message_Refresh RefreshMessage { get; set; }

        /// <summary>
        ///the uopProperties curenntly being edited
        ////// </summary>
        private uopProperties _EditProperties;
        public virtual uopProperties EditProps
        {
            get { if (_EditProperties != null) _EditProperties.DisplayUnits = DisplayUnits; return _EditProperties; }
            set
            {
                _EditProperties = value;
                _PropControls = null;

                if (_EditProperties != null) 
                {
                    _EditProperties.DisplayUnits = DisplayUnits;
                    _EditProperties.ResetValueChange(); 
                    _EditProperties.StoredValuesClear(); 
                    _EditProperties.StoreValues(true);
                   
                } 
            }
        }

        private List<PropertyControlViewModel> _PropControls = null;
        public virtual List<PropertyControlViewModel> PropertyControls 
        { 
            get 
            {   if(_PropControls == null && _EditProperties != null)
                {
                    _PropControls = new List<PropertyControlViewModel>();
                    foreach (var item in _EditProperties)
                    {
                        _PropControls.Add(new PropertyControlViewModel(item));
                    }
                }
                return _PropControls ?? new List<PropertyControlViewModel>(); 
            } 
        }

        public virtual void CreatePropertyControls(bool bReadOnly, uopProperties aProperties = null, PropertyControlContainer aContainer = null)
        {
            _PropControls = new List<PropertyControlViewModel>();
            aProperties ??= _EditProperties;

            if (aProperties == null) return;
            foreach (var item in _EditProperties)
            {
                uopProperty prop = item;
                if (bReadOnly)
                {
                    prop = prop.Clone();
                    if(prop.VariableTypeName == "BOOL")
                    {
                        prop = new uopProperty(prop.Name, prop.ValueB ? "Yes" : "No", aPartType: prop.PartType);
                    }
                    prop.Protected = true;
                }
                
                _PropControls.Add(new PropertyControlViewModel(prop,bReadOnly,aContainer:aContainer));
            }

        }

        public void ExecutePropertyControlsLostFocus()
        {
            List<PropertyControlViewModel> pcontrols = PropertyControls;
            foreach (var item in pcontrols) 
            {
               
                item.ExectuteLostFocus(); 
            }
        }

        public virtual PropertyControlViewModel GetPropControlModel(string aPropertyName, bool bReadOnly, bool bFreeFrom, string aDisplayName = null, Visibility? aVisibility = null, string aSetVisibiltyName = null, string aNullValueReplacer = null, bool? bZeroAsNullString = null, List<string> aChoiceList = null)
        {
            uopProperty eprop = EditProps.Find(x => string.Compare(x.Name, aPropertyName, true) == 0);
            PropertyControlViewModel _rVal;

            if (eprop == null)
            {
                eprop = new uopProperty(aPropertyName, "") { DisplayName = "Not Found" } ;
                _rVal = new PropertyControlViewModel(eprop, true) { Visibility = Visibility.Collapsed };
                return _rVal;
                //throw new Exception($"Edit Property Control '{aPropertyName}' Was Not Found On '{ToString()}'");

            }

            if (eprop.HasUnits) bFreeFrom = false;

            bool bIsYesNo = eprop.VariableTypeName == "BOOL";

            if (!bZeroAsNullString.HasValue) bZeroAsNullString = !bReadOnly;
            if (!aVisibility.HasValue && !string.IsNullOrWhiteSpace(aSetVisibiltyName))
            {
                aVisibility = (bReadOnly && EditProps.IsNullValue(aSetVisibiltyName, true)) ? Visibility.Collapsed : Visibility.Visible;
            }
            if (!aVisibility.HasValue) aVisibility = Visibility.Visible;
         
            _rVal = PropertyControls.Find(x => string.Compare(x.Property.Name, aPropertyName, true) == 0);
                     
            _rVal.DisplayUnits = DisplayUnits;
            _rVal.IsReadOnly = bReadOnly;
            _rVal.FreeForm = bFreeFrom;
            _rVal.Visibility = aVisibility.Value;

            if (!_rVal.IsYesNo)
            {
                if (aChoiceList == null)
                {
                    if (!bIsYesNo)
                    {
                        if (_rVal.Property.UnitType == uppUnitTypes.Density)
                        {
                            bZeroAsNullString = true;
                            if (bReadOnly) aNullValueReplacer ??= "-";
                            if (bReadOnly && _rVal.Property.Value <= 0) aVisibility = Visibility.Collapsed;
                        }
                        _rVal.ZerosAsNullString = bZeroAsNullString.Value;
                        _rVal.NullValueReplacer = aNullValueReplacer;

                    }


                }
                else
                {
                    string sval = _rVal.Property.DisplayValueString;
                    if (!bReadOnly)
                    {
                        _rVal.Choices = aChoiceList;
                        _rVal.ChoiceIndex = aChoiceList.FindIndex(x => string.Compare(x, sval, true) == 0);

                    }
                }
            }
            


            if (!string.IsNullOrWhiteSpace(aDisplayName)) _rVal.DisplayName = aDisplayName;
            
            return _rVal;
        }

        private Visibility _Visibility_UnitToggle = Visibility.Visible;
        public Visibility Visibility_UnitToggle { get => _Visibility_UnitToggle; set { _Visibility_UnitToggle = value; NotifyPropertyChanged("Visibility_UnitToggle"); } }

        /// <summary>
        ///the orignal uopProperties curenntly being edited
        ///stored to provided the changed values on save
        ////// </summary>

        public virtual uopProperties GetEditedProperties(bool bGetJustOne = false)
        {
            if (_EditProperties == null) return new uopProperties();
            uopProperties orig = _EditProperties.StoreValuesGet(1, false);
            return orig.GetDifferences(_EditProperties, bBailOnFistDifference: bGetJustOne);
        }

        public List<ValueLimit> EditPropertyValueLimits { get; set; }


      

        public uopProperties Notes { get; set; }


        /// <summary>
        ///the units to display in th view
        ////// </summary>
        private uppUnitFamilies _DisplayUnits = uppUnitFamilies.Undefined;
        public virtual uppUnitFamilies DisplayUnits
        {
            get
            {
                if (_DisplayUnits == uppUnitFamilies.Undefined)
                { _DisplayUnits = ProjectDisplayUnits; UnitChange(); }
                return _DisplayUnits;
            }
            set
            {
                bool newval = value != _DisplayUnits;
                _DisplayUnits = value;
                if (EditProps != null) EditProps.DisplayUnits = _DisplayUnits;
              


                if (newval) UnitChange();
                NotifyPropertyChanged("DisplayUnits");
            }
        }


        public string LinearUnitsLabel => Units_Linear.Label(DisplayUnits);
        public string AreaUnitsLabel => Units_Area.Label(DisplayUnits);

        private bool _IsEnglishSelected = true;
        public virtual bool IsEnglishSelected
        {
            get => _IsEnglishSelected;
            set { _IsEnglishSelected = value;  DisplayUnits = (!value) ? uppUnitFamilies.Metric : uppUnitFamilies.English; }
        }


        /// <summary>
        /// returns the conversion factor for small linear units (in / mm) from the current Display units to english units
        /// </summary>        
        public double Multiplier_Linear => Units_Linear.ConversionFactor(DisplayUnits);


        /// <summary>
        /// returns the conversion factor for small area units (in^2 & cm^2) from the current Display units to english units
        /// </summary>  
        public double Multiplier_SmallArea => Units_Area.ConversionFactor(DisplayUnits);

        /// <summary>
        /// busy indicator for all screen
        /// </summary>        
        private bool _IsOkBtnEnable = true;
        public bool IsOkBtnEnable
        {
            get => _IsOkBtnEnable && IsEnabled;
            set { _IsOkBtnEnable = value; NotifyPropertyChanged("IsOkBtnEnable"); }
        }

        /// <summary>
        /// busy indicator for all screen
        /// </summary>        
        private bool _IsCancelBtnEnable = true;
        public bool IsCancelBtnEnable
        {
            get => _IsCancelBtnEnable && IsEnabled;
            set { _IsCancelBtnEnable = value; NotifyPropertyChanged("IsCancelBtnEnable"); }
        }

        public WeakReference<Window> WindowReference { get; set; }
        public WeakReference<UserControl> ControlReference { get; set; }

        public string FocusTarget { get; set; }

        public virtual uopDocuments Warnings { get; set;}


        #endregion Properties


        #region Methods
        
        public uopDocWarning SaveWarning(Exception aException, System.Reflection.MethodBase aMethod = null, string aBrief = null, string aTextSuffix = "")
        {
            if (aException == null) return null;
            Warnings ??=new uopDocuments();
            return Warnings.AddWarning(aException, aMethod,aBrief,aTextSuffix);
        }

        public void ShowWarnings(List<uopDocument> aWarnings = null)
        {
            Warnings ??= new uopDocuments();
            if (aWarnings != null) Warnings.Append(aWarnings, bAddClones: true, uppDocumentTypes.Warning);
            if (Warnings.Count > 0) PublishMessage<Message_ShowWarnings>(new Message_ShowWarnings(Warnings));
        }

        public void PublishMessage<T>(T message)
        {
            IEventAggregator ea = EventAggregator;
            if(ea ==null) 
                ea = WinTrayMainViewModel.WinTrayMainViewModelObj?.EventAggregator;
            if (ea == null || message == null)
                return;

            ea.Publish<T>(message);
        }


        public virtual void UpdateVisibily()
        {
            uopProject project = Project;
            bool supevnt = SuppressEvents;
            SuppressEvents = false;
            if (project == null)
            {
                VisibilityProject = Visibility.Collapsed;
                VisibilityMDProject = Visibility.Collapsed;
            }
            else 
            { 
                if(project.ProjectFamily == uppProjectFamilies.uopFamMD)
                {
                    VisibilityMDProject =  project.TrayRanges.Count >0 ? Visibility.Visible : Visibility.Collapsed;
                }
                else
                {
                    VisibilityMDProject = Visibility.Collapsed;
                }
            }

            SuppressEvents = supevnt;
        }


        public virtual void Activate(Window myWindow)
        {
            Activated = true;
            WindowReference = (myWindow == null) ? null : new WeakReference<Window>(myWindow);
        }


        public virtual void Activate(UserControl myUserControl)
        {
            Activated = true;
            ControlReference = (myUserControl == null) ? null : new WeakReference<UserControl>(myUserControl);
        }


        public System.Windows.Window MyWindow()
        {
            if (WindowReference == null) return null;

       
            if (!WindowReference.TryGetTarget(out Window _rVal)) WindowReference = null ;
            return _rVal;
        }

        public UserControl MyUserControl()
        {
            if (ControlReference == null) return null;

            if (!ControlReference.TryGetTarget(out UserControl _rVal)) ControlReference = null;
            return _rVal;
        }
        public virtual void ToggleUnits()
        {
            DisplayUnits = (DisplayUnits == uppUnitFamilies.Metric) ? uppUnitFamilies.English : uppUnitFamilies.Metric;
        }

        private void UnitChange()
        {
           
            _IsEnglishSelected = _DisplayUnits == uppUnitFamilies.English;

            if (_EditProperties != null) _EditProperties.DisplayUnits = DisplayUnits;

            //if (_IsEnglishSelected)
            //{
            //    PreDecimalDigitCountOtherProp = 4;
            //    DecimalDigitCountOtherField = 5;
            //    PreDecimalDigitCountArea = 5;
            //    DecimalDigitCountArea = 5;
            //}
            //else
            //{
            //    PreDecimalDigitCountOtherProp = 7;
            //    DecimalDigitCountOtherField = 1;
            //    PreDecimalDigitCountArea = 8;
            //    DecimalDigitCountArea = 1;
            //}

            NotifyPropertyChanged("DisplayUnits");
            NotifyPropertyChanged("IsEnglishSelected");
            NotifyPropertyChanged("AreaUnitsLabel");
            NotifyPropertyChanged("LinearUnitsLabel");
            if (_PropControls != null)
            {
                foreach (var item in _PropControls)
                {
                    item.Property.DisplayUnits = _DisplayUnits;
                }
            }

        }
        public virtual void SetFocus(string aControlName)
        {
            if (string.IsNullOrWhiteSpace(aControlName)) aControlName = FocusTarget;
            if (string.IsNullOrWhiteSpace(aControlName)) return;
            Window window = MyWindow();
            if (window == null) return;

            try
            {
                IEnumerable<PropertyControl> elements = WinTrayUtilies.FindVisualChildren<PropertyControl>(window).Where(x => (x.Tag != null && string.Compare(x.Tag.ToString(), aControlName, true) == 0 || string.Compare(x.Name, aControlName, true) == 0) && x.Visibility == Visibility.Visible);

                if (elements.Count() <= 0) return;

                object element = elements.First(); //    window.FindName(aControlName);
                //Control cntrl = element as Control;
                if (element is PropertyControl)
                {
                    
                    PropertyControl pcontrol = element as PropertyControl;
                    element = pcontrol.InputControl();
                
                      
                }


                if (element != null)
                    FocusManager.SetFocusedElement(window, (FrameworkElement)element);

            }


            catch { }
      

        }

        public virtual void ReleaseReferences()
        {
            DialogService = null;
           
           
            EditProps = null;
          
            EventAggregator?.Unsubscribe(this);
            EventAggregator = null;
            
            if (_Image != null) _Image.Dispose();
            _Image = null;
            _Trays = null;
            Viewer = null;
            Project = null;
            _ParentVM_Ref = null;
            WindowReference = null;
            ControlReference = null;
        }

        public override void Dispose()
        {
            base.Dispose();
            ReleaseReferences();
        }
        #endregion Methods

        #region Message Handlers

        /// <summary>
        /// clean up handler
        /// </summary>
        /// <param name="message"></param>
        public virtual void OnAggregateEvent(Message_UnloadProject message)
        {
            if (_EditProperties != null) _EditProperties.Clear();
            _EditProperties = null;
           _Project = null;

        }

        #endregion Message Handlers

        #region Shared Methods
     

      
        #endregion Shared Methods
    }
}
