using UOP.WinTray.UI.Views.Windows;
using MvvmDialogs;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.UI.Commands;
using UOP.WinTray.UI.Events.EventAggregator;

namespace UOP.WinTray.UI.ViewModels
{
    /// <summary>
    /// Base class provides common functionalities for both new project and edit new project view models.
    /// </summary>
    public abstract class NewMDProjectViewModelBase : MDProjectViewModelBase
    {
        #region Variables

        protected bool? _DialogResult;
        #endregion

        #region Constants

       
        protected const string ERR_KEY_NO = "Key Number Is a Required Field";
        protected const string ERR_MANHOLE_ID = "Invalid Manhole I.D.";
        protected const string ERR_ID_REQUIRED = "ID Number Is a Required Field";
        protected const string ERR_ID_FORMAT = "ID Number Must Be Entered In The Format ##-###";
        protected const string SPOUT_AREA_DEVIATION_LOWERLIMIT = "Deviation Limit Must Be Greater Than Or Equal To 0.25%";
        protected const string SPOUT_AREA_DEVIATION_UPPERLIMIT = "Deviation Limit Must Be Less Than Or Equal To 100%";
        protected const string DISTRIBUTION_CONV_LOWERLIMIT = "Convergence Limit Must Be Greater Than Or Equal To 0.000001";
        protected const string DISTRIBUTION_CONV_UPPERLIMIT = "Convergence Limit Must Be Less Than Or Equal To 0.001";
        protected const string regPattern = "[0-9][0-9]-[0-9][0-9][0-9]";
        protected const string CALCULATION_IN_PROGRESS = "Calculation in progress...";

        #endregion

        #region Constructor


        internal NewMDProjectViewModelBase( IEventAggregator eventAggregator = null,
                                        IDialogService dialogService = null) : base(eventAggregator, dialogService: dialogService)
        {
            NotesHaveChanged = false;
        }

        #endregion

        #region Properties       

        public bool NotesHaveChanged { get; set; }

    

        private string _ProjectTitle;
        public string ProjectTitle
        {
            get => _ProjectTitle;
            set { _ProjectTitle = value; NotifyPropertyChanged("ProjectTitle"); }
        }

        private string _DisplayUnit;
        public string DisplayUnit
        {
            get => _DisplayUnit;
            set { _DisplayUnit = value; NotifyPropertyChanged("DisplayUnit"); }
        }

       

        public bool? DialogResult
        {
             get => _DialogResult;
            
            set
            {
                _DialogResult = value;
                NotifyPropertyChanged("DialogResult");
            }
        }

        private bool _EditProjectPropertyVisibilty;
        public bool EditProjectPropertyVisibilty
        {
            get => _EditProjectPropertyVisibilty; 
            set { _EditProjectPropertyVisibilty = value; NotifyPropertyChanged("EditProjectPropertyVisibilty"); }
        }

        public bool MetricSpouting
        {
            get => EditProps == null || EditProps.ValueB("MetricSpouting",aDefault:true);
            set { if (EditProps != null) { EditProps.SetProperty("MetricSpouting", value); NotifyPropertyChanged("MetricSpouting"); } }
        }

        public virtual int RevNo
        {
            get => (EditProps != null) ? EditProps.ValueI("Revision",aDefault:0) : 0;
            set { if (EditProps != null) { EditProps.SetProperty("Revision", value); NotifyPropertyChanged("RevNo"); } }
         
        }

        private bool _BoltingIsEnglish;
        public bool BoltingIsEnglish
        {
            get { return _BoltingIsEnglish; }
            set 
            { 
                _BoltingIsEnglish = value; 
                NotifyPropertyChanged("BoltingIsEnglish");
                EditProps?.SetProperty("Bolting", value ? uppUnitFamilies.English : uppUnitFamilies.Metric  );
                NotifyPropertyChanged("Bolting");
                _BoltingIsMetric = !value;
                NotifyPropertyChanged("BoltingIsMetric");
            }
        }
        private bool _BoltingIsMetric;
        public bool BoltingIsMetric
        {
            get { return _BoltingIsMetric; }
            set 
            { 
                _BoltingIsMetric = value; NotifyPropertyChanged("BoltingIsMetric");
                EditProps.SetProperty("Bolting", value ? uppUnitFamilies.English : uppUnitFamilies.Metric);
                NotifyPropertyChanged("Bolting");
                _BoltingIsEnglish = !value;
                NotifyPropertyChanged("BoltingIsEnglish");

            }
        }

        public virtual uppUnitFamilies Bolting
        {
            get =>(EditProps != null) ? (uppUnitFamilies)EditProps.ValueI("Bolting",aDefault: (int)uppUnitFamilies.Metric): uppUnitFamilies.Metric;
            set 
            { 
                if (EditProps != null) 
                { 
                    EditProps.SetProperty("Bolting", value); 
                    NotifyPropertyChanged("Bolting"); 
                    _BoltingIsEnglish = value == uppUnitFamilies.English;
                    _BoltingIsMetric = value != uppUnitFamilies.English;
                    NotifyPropertyChanged("BoltingIsEnglish");
                        NotifyPropertyChanged("BoltingIsMetric");
                } 
            }
        }


        public virtual bool MetricRings
        {
            get => (EditProps != null) && EditProps.ValueB("MetricRings",aDefault:false);
            set { if (EditProps != null) { EditProps.SetProperty("MetricRings", value); NotifyPropertyChanged("MetricRings"); } }
        
        }

        public virtual string KeyNumber
        {
            get => (EditProps != null) ? EditProps.ValueS("KeyNumber") : "";
            set
            {
                value = (!string.IsNullOrEmpty(value.Trim())) ? value.ToString().PadLeft(5, '0') : value.Trim();
                if (EditProps != null) { EditProps.SetProperty("KeyNumber", value); NotifyPropertyChanged("KeyNumber"); }
            }
        }


        public virtual double ManholeID
        {
            get => (EditProps != null) ? EditProps.ValueD("ManholeID", aMultiplier: Multiplier_Linear) : 0;
            set { if (EditProps != null) { EditProps.SetValueD("ManholeID", value, 1 / Multiplier_Linear); NotifyPropertyChanged("ManholeID"); } }
        }

        public virtual double RingThickness
        {
            get =>(EditProps != null) ? EditProps.ValueD("RingThickness", aMultiplier: Multiplier_Linear) : 0;
            set
            {
                if (EditProps != null) { EditProps.SetValueD("RingThickness", value, 1 / Multiplier_Linear); NotifyPropertyChanged("RingThickness"); }
            }
        }

        public virtual uppInstallationTypes InstallationType
        {
            get => (EditProps != null) ? (uppInstallationTypes)EditProps.ValueI("InstallationType"): uppInstallationTypes.GrassRoots ;
            set { if (EditProps != null) { EditProps.SetProperty("InstallationType", value); NotifyPropertyChanged("InstallationType"); } }
        }
        public virtual bool ReverseSort
        {
            get => (EditProps != null) && EditProps.ValueB("ReverseSort");
            set { if (EditProps != null) { EditProps.SetProperty("ReverseSort", value); NotifyPropertyChanged("ReverseSort"); } }
     
        }

        public virtual string IDNumber
        {
            get => (EditProps != null) ? EditProps.ValueS("IDNumber") : "";
            set { if (EditProps != null) { EditProps.SetProperty("IDNumber", value); NotifyPropertyChanged("IDNumber"); } }


        }
        public virtual string ColumnLetter
        {
            get =>(EditProps != null) ? EditProps.ValueS("ColumnLetter") : "";
            set { if (EditProps != null) { EditProps.SetProperty("ColumnLetter", value); NotifyPropertyChanged("ColumnLetter"); } }

        
        }

        public virtual string SAPNumber
        {
            get => (EditProps != null) ? EditProps.ValueS("SAPNumber") : "";
            set { if (EditProps != null) { EditProps.SetProperty("SAPNumber", value); NotifyPropertyChanged("SAPNumber"); } }
        }

        public virtual string ProcessLicensor
        {
            get => (EditProps != null) ? EditProps.ValueS("ProcessLicensor") : "";
            set { if (EditProps != null) { EditProps.SetProperty("ProcessLicensor", value); NotifyPropertyChanged("ProcessLicensor"); } }
          
        }

        public virtual string TrayVendor
        {
            get => (EditProps != null) ? EditProps.ValueS("TrayVendor") : "";
            set { if (EditProps != null) { EditProps.SetProperty("TrayVendor", value); NotifyPropertyChanged("TrayVendor"); } } 
          
        }
        public virtual string Service
        {
            get => (EditProps != null) ? EditProps.ValueS("Service") : "";
            set { if (EditProps != null) { EditProps.SetProperty("Service", value); NotifyPropertyChanged("Service"); } }
        }

        public virtual string Location
        {
            get => (EditProps != null) ? EditProps.ValueS("Location") : "";
            set { if (EditProps != null) { EditProps.SetProperty("Location", value); NotifyPropertyChanged("Location"); } }
          
        }
        public virtual string ItemNo
        {
            get => (EditProps != null) ? EditProps.ValueS("Item") : "";
            set { if (EditProps != null) { EditProps.SetProperty("Item", value); NotifyPropertyChanged("ItemNo"); } }
        }

        public virtual string PONo
        {
            get => (EditProps != null) ? EditProps.ValueS("PO") : "";
            set { if (EditProps != null) { EditProps.SetProperty("PO", value); NotifyPropertyChanged("PONo"); } }

          
        }
        public virtual bool ForExport
        {
            get => EditProps == null || EditProps.ValueB("ForExport") ;
            set { if (EditProps != null) { EditProps.SetProperty("ForExport", value); NotifyPropertyChanged("ForExport"); } }
        }
        public virtual string CustomerName
        {
            get => (EditProps != null) ? EditProps.ValueS("Name") : "";
            set
            {
                if (EditProps != null) { EditProps.SetProperty("Name", value); NotifyPropertyChanged("CustomerName"); }
            }
        }
        public virtual string Contractor
        {
            get => (EditProps != null) ? EditProps.ValueS("Contractor") : "";
            set
            {
                if (EditProps != null) { EditProps.SetProperty("Contractor", value); NotifyPropertyChanged("Contractor"); }
            }
        }

        public virtual double SpoutAreaDeviationLimit
        {
            get => (EditProps != null) ? EditProps.ValueD("ErrorLimit") : 2.5;
            set
            {
                if (EditProps != null) { EditProps.SetProperty("ErrorLimit", value); NotifyPropertyChanged("SpoutAreaDeviationLimit"); }
            }
        }

        public virtual double ConvergenceLimit
        {
            get => (EditProps != null) ? EditProps.ValueD("ConvergenceLimit",aDefault:0.00001) : 0.00001;
            set { EditProps.SetProperty("ConvergenceLimit", value); NotifyPropertyChanged("ConvergenceLimit"); }
        }

        public virtual uppMDSpacingMethods SpacingMethod
        {
            get => (EditProps != null) ? (uppMDSpacingMethods)EditProps.ValueI("SpacingMethod",aDefault: (int)uppMDSpacingMethods.Weighted) : uppMDSpacingMethods.Weighted;
            set { EditProps?.SetProperty("SpacingMethod", value); NotifyPropertyChanged("SpacingMethod"); }
        }

        private bool _IsCustomerDrawingUnitsEnglish = false;
        public virtual bool IsCustomerDrawingUnitsEnglish
        {
            get => _IsCustomerDrawingUnitsEnglish;
            set
            {
                _IsCustomerDrawingUnitsEnglish = value;
                CustomerDrawingUnitsName = value ? "English" : "Metric"; NotifyPropertyChanged("IsCustomerDrawingUnitsEnglish");
            }
        }

        private bool _IsManufacturingDrawingUnitsEnglish = false;
        public virtual bool IsManufacturingDrawingUnitsEnglish
        {
            get => _IsManufacturingDrawingUnitsEnglish;
            set
            {
                _IsManufacturingDrawingUnitsEnglish = value;
                ManufacturingDrawingUnitsName = value ? "English" : "Metric"; NotifyPropertyChanged("IsManufacturingDrawingUnitsEnglish");
            }
        }

        public virtual string CustomerDrawingUnitsName
        {
            get { if (EditProps == null) return "Metric"; return ((uppUnitFamilies)EditProps.ValueI("CustomerDrawingUnits") == uppUnitFamilies.English) ? "English" : "Metric"; }
            set
            {
                if (EditProps != null)
                {
                    uppUnitFamilies enumval = (string.Compare(value, "English", ignoreCase: true) == 0) ? uppUnitFamilies.English : uppUnitFamilies.Metric;
                    EditProps.SetProperty("CustomerDrawingUnits", enumval); NotifyPropertyChanged("CustomerDrawingUnitsName");

                }
            }
        }

        public virtual string ManufacturingDrawingUnitsName
        {
            get { if (EditProps == null) return "Metric"; return ((uppUnitFamilies)EditProps.ValueI("ManufacturingDrawingUnits") == uppUnitFamilies.English) ? "English" : "Metric"; }
            set
            {
                if (EditProps != null)
                {
                    uppUnitFamilies enumval = (string.Compare(value, "English", ignoreCase: true) == 0) ? uppUnitFamilies.English : uppUnitFamilies.Metric;
                    EditProps.SetProperty("ManufacturingDrawingUnits", enumval); NotifyPropertyChanged("ManufacturingDrawingUnitsName");

                }
            }
        }



        public virtual double HardwareSparePercentage
        {
            get => (EditProps != null) ? EditProps.ValueD("SparePercentage") : 5;
            set
            {
                if (EditProps != null) { EditProps.SetProperty("SparePercentage", value); NotifyPropertyChanged("HardwareSparePercentage"); }
            }

        }
        public virtual double ClipSparePercentage
        {
            get => (EditProps != null) ? EditProps.ValueD("ClipSparePercentage") : 2;
            set
            {
                if (EditProps != null) { EditProps.SetProperty("ClipSparePercentage", value); NotifyPropertyChanged("ClipSparePercentage"); }
            }

        }

        public virtual bool IsApplyMetricRingSpecs
        {
            get => MetricRings;
            set { MetricRings = value; NotifyPropertyChanged("IsApplyMetricRingSpecs"); }

        }

        private List<string> _Licensors;
        public List<string> Licensors
        {
            get => _Licensors;
            set { _Licensors = value; NotifyPropertyChanged("Licensors"); }
        }

        public string NewLicensor
        {
            get => ProcessLicensor;
            set
            {
                if (!string.IsNullOrEmpty(ProcessLicensor) && ProcessLicensor == value)  return;
                if (!string.IsNullOrEmpty(value)) ProcessLicensor = value;
             }
        }

        private List<string> _TrayVendors;
        public List<string> TrayVendors
        {
            get => _TrayVendors;
            set { _TrayVendors = value; NotifyPropertyChanged("TrayVendors"); }
        }

     

        public string NewTrayVendor
        {
            get => TrayVendor;
            
            set
            {
                if (!string.IsNullOrEmpty(TrayVendor) && TrayVendor == value) return;
                if (!string.IsNullOrEmpty(value)) TrayVendor = value;
            }
        }

        private List<string> _Services;
        public List<string> Services
        {
            get => _Services;
            set { _Services = value; NotifyPropertyChanged("Services"); }
        }

       

        public string NewService
        {
            get => Service;
            
            set
            {
                if (!string.IsNullOrEmpty(Service) && Service == value) return;
                if (!string.IsNullOrEmpty(value)) Service = value;
                
            }
        }


        private List<string> _Customers;
        public List<string> Customers
        {
            get => _Customers;
            set { _Customers = value; NotifyPropertyChanged("Customers"); }
        }

       
        public string NewCustomer
        {
            get=> CustomerName;
            
            set
            {
                if (!string.IsNullOrEmpty(CustomerName) && CustomerName == value) return;
                if (!string.IsNullOrEmpty(value)) CustomerName = value;
            }
        }
        public string NewContractor
        {
            get=>  Contractor;
            set 
            {
                if (!string.IsNullOrEmpty(Contractor) && Contractor == value) return;
                if (!string.IsNullOrEmpty(value)) Contractor = value;
            }
        }

        private List<string> _Contractors;
        public List<string> Contractors
        {
            get => _Contractors;
            
            set
            {
                // if selected constractor is not available in data base, add it
                _Contractors = value;
                NotifyPropertyChanged("Contractors");
            }
        }

     
      

        private string _SelectedProjectType;
        public string SelectedProjectType
        {
            get => _SelectedProjectType; 
            set { _SelectedProjectType = value; NotifyPropertyChanged("SelectedProjectType"); }
        }

        /// <summary>
        /// Is Manhole Id to focus.
        /// </summary>
        private bool _IsFocusedManholeId;
        public bool IsFocusedManholeId
        {
            get => _IsFocusedManholeId;
            set { _IsFocusedManholeId = value; NotifyPropertyChanged("IsFocusedManholeId"); }
        }
        private bool _IsFocusedHardwareSpares;
        public bool IsFocusedHardwareSpares
        {
            get => _IsFocusedHardwareSpares;
            set { _IsFocusedHardwareSpares = value; NotifyPropertyChanged("IsFocusedHardwareSpares"); }
        }

       
        private bool _IsFocusedRingThickness;
        public bool IsFocusedRingThickness
        {
            get => _IsFocusedRingThickness;
            set { _IsFocusedRingThickness = value; NotifyPropertyChanged("IsFocusedRingThickness"); }
        }
        /// <summary>
        /// Is KeyNumber to focus.
        /// </summary>
        private bool _IsFocusedKeyNo;
        public bool IsFocusedKeyNo
        {
            get => _IsFocusedKeyNo;
            set { _IsFocusedKeyNo = value; NotifyPropertyChanged("IsFocusedKeyNo"); }
        }

        /// <summary>
        /// Is ID no to focus.
        /// </summary>
        private bool _IsFocusedIDNo;
        public bool IsFocusedIDNo
        {
            get => _IsFocusedIDNo;
            set { _IsFocusedIDNo = value; NotifyPropertyChanged("IsFocusedIDNo"); }
        }

        /// <summary>
        /// Is Spout Area Deviation Limit to focus.
        /// </summary>
        private bool _IsFocusedSpoutAreaDeviationLimit;
        public bool IsFocusedSpoutAreaDeviationLimit
        {
            get => _IsFocusedSpoutAreaDeviationLimit; 
            set { _IsFocusedSpoutAreaDeviationLimit = value; NotifyPropertyChanged("IsFocusedSpoutAreaDeviationLimit"); }
        }

        /// <summary>
        /// Is Spout Area Deviation Limit to focus.
        /// </summary>
        private bool _IsFocusedDistributionConvLimit;
        public bool IsFocusedDistributionConvLimit
        {
            get => _IsFocusedDistributionConvLimit;
            set { _IsFocusedDistributionConvLimit = value; NotifyPropertyChanged("IsFocusedDistributionConvLimit"); }
        }

        public abstract DelegateCommand Command_ToggleUnits { get; }

        /// <summary>
        /// Open Notes command
        /// </summary> 
        protected DelegateCommand _CMD_Notes;
        public DelegateCommand Command_Notes { get { _CMD_Notes ??= new DelegateCommand(param => OpenNotes()); return _CMD_Notes; } }

        //cancel Command
        private DelegateCommand _CMD_Cancel;
        public DelegateCommand CancelCommand { get { _CMD_Cancel ??= new DelegateCommand(param => CloseNewProjectForm()); return _CMD_Cancel; } }

        public bool IsValid { get; protected set; }

        public MessageBoxResult MessageBoxResult { get; set; }

        #endregion



        #region Methods


        public abstract void NotifyPropertyChanges();


        /// <summary>
        /// Populates supported project types
        /// </summary>
        /// <param name="pType"></param>
        public List<string> PopulateProjectType(uppProjectTypes pType)
        {
            var projTypes = new List<string>();
            switch (pType)
            {
                case uppProjectTypes.MDSpout:
                    projTypes.Add("MD Spout");
                    break;
                case uppProjectTypes.MDDraw:
                    projTypes.Add("MD Draw");
                    break;
                case uppProjectTypes.CrossFlow:
                    projTypes.Add("Cross Flow");
                    break;
            }

            return projTypes;
        }

        /// <summary>
        /// Close Form
        /// </summary>
        protected virtual void CloseNewProjectForm()
        {
            DialogResult = false;
        }

        /// <summary>
        /// Clear the Focus of controls.
        /// </summary>
        protected virtual void ClearFocus()
        {
            IsFocusedKeyNo = false;
            IsFocusedManholeId = false;
            IsFocusedDistributionConvLimit = false;
            IsFocusedSpoutAreaDeviationLimit = false;
            IsFocusedIDNo = false;
        }

            /// <summary>
            /// Open Notes window.
            /// </summary>
            protected void OpenNotes()
        {
            if (Project == null) return; 
            Edit_Notes_ViewModel VM = new(Project);
            bool? result =  DialogService.ShowDialog<Edit_Notes_View>(this, VM);
            if (result.HasValue && result.Value == true) NotesHaveChanged = true;
        }

        #endregion

        #region Validation

        /// <summary>
        /// It will validate various NewProject Fields
        /// </summary>
        /// <param name="errString"></param>
        /// <returns></returns>
        public virtual bool ValidateInput(out string errString)
        {
            if (string.IsNullOrEmpty(KeyNumber))
            {
                errString = ERR_KEY_NO;
                IsFocusedKeyNo = true;
                return false;
            }
            else if (ManholeID <= 0)
            {
                errString = ERR_MANHOLE_ID;
                IsFocusedManholeId = true;
                return false;
            }
            else if (string.IsNullOrEmpty(IDNumber))
            {
                errString = ERR_ID_REQUIRED;
                IsFocusedIDNo = true;
                return false;
            }
            else if (!ValidateIDNo())
            {
                errString = ERR_ID_FORMAT;
                IsFocusedIDNo = true;
                return false;
            }
            else if (SpoutAreaDeviationLimit < 0.25)
            {
                errString = SPOUT_AREA_DEVIATION_LOWERLIMIT;
                IsFocusedSpoutAreaDeviationLimit = true;
                return false;
            }
            else if (SpoutAreaDeviationLimit > 100)
            {
                errString = SPOUT_AREA_DEVIATION_UPPERLIMIT;
                IsFocusedSpoutAreaDeviationLimit = true;
                return false;
            }
            else if (ConvergenceLimit < 0.000001)
            {
                errString = DISTRIBUTION_CONV_LOWERLIMIT;
                IsFocusedDistributionConvLimit = true;
                return false;
            }
            else if (ConvergenceLimit > 0.001)
            {
                errString = DISTRIBUTION_CONV_UPPERLIMIT;
                IsFocusedDistributionConvLimit = true;
                return false;
            }
            else
            {
                errString = string.Empty;
                return true;
            }
        }

        /// <summary>
        /// Validate ID No format "##-###" where # should be a number
        /// </summary>
        /// <returns></returns>
        protected bool ValidateIDNo()
        {
            bool isValid = Regex.IsMatch(IDNumber, regPattern);
            return isValid;
        }

        #endregion

        #region validation
        /// <summary>
        /// Validate ID No format "##-###" where # should be a number
        /// </summary>
        /// <returns></returns>
        protected virtual bool ValidateIDNo(string IDNumber,string regPattern)
        {
            bool isValid = Regex.IsMatch(IDNumber, regPattern);
            return isValid;
        }
        #endregion
    }
}
