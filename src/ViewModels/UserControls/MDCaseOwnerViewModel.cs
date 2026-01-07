using System;
using System.Collections.Generic;
using System.Windows;
using UOP.WinTray.Projects;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Interfaces;
using UOP.WinTray.Projects.Parts;
using UOP.WinTray.UI.Messages;

namespace UOP.WinTray.UI.ViewModels
{
    public class MDCaseOwnerViewModel : PropertyControlContainer
    {

      

        #region Constructors

        public MDCaseOwnerViewModel(mdProject project, iCaseOwner aOwner, bool bReadOnly)
        {
            MDProject = project;
            IsReadOnly = bReadOnly;
            AllowEdits = !IsReadOnly;
            Owner = aOwner;
            EditProps = aOwner == null ? new uopProperties() : aOwner.CurrentProperties();
            CreatePropertyControls(bReadOnly, aContainer: this);
            if (MDProject == null && Owner != null) MDProject = aOwner.MDProject;
      
         }

        #endregion Constructors

       

        #region Properties
        public string Name => Owner != null ? Owner.Description : "Undefined";

        private iCaseOwner _Owner = null;
        public iCaseOwner Owner
        {
            get => _Owner;
            set
            {
                _Owner = value;
                NotifyPropertyChanged("Owner");
                if(value == null)
                {
                    IsDistributor = false;
                    IsCT = false;
                    EditProps = new uopProperties();
                    Visibility_Distributor = Visibility.Collapsed;
                    Visibility_ChimneyTray = Visibility.Collapsed;
                    Visibility = Visibility.Collapsed;
                }
                else
                {
                    IsDistributor = value.OwnerType == uppCaseOwnerOwnerTypes.Distributor;
                    IsCT = value.OwnerType == uppCaseOwnerOwnerTypes.ChimneyTray;
                    EditProps = value.CurrentProperties();
                    Visibility_Distributor = IsDistributor ? Visibility.Visible : Visibility.Collapsed;
                    Visibility_ChimneyTray = IsCT ? Visibility.Visible : Visibility.Collapsed;
                    Visibility = Visibility.Visible;
                }
            }
        }

        public uppCaseOwnerOwnerTypes OwnerType => Owner == null ? uppCaseOwnerOwnerTypes.Undefined : Owner.OwnerType;
        public int OwnerIndex => Owner == null ? 0 : Owner.Index;

        public int SelectedCaseIndex => Owner == null ? 0 : Owner.Cases.SelectedIndex;

        private bool _IsDistributor = false;
        public bool IsDistributor { get => _IsDistributor; private set { _IsDistributor = value; NotifyPropertyChanged("IsDistributor"); } }

        private bool _IsCT = false;
        public bool IsCT { get => _IsCT; private set { _IsCT = value; NotifyPropertyChanged("IsCT"); } }

        public int OptionIndex1 { get; set; } = 0;
        public mdDistributor Distributor
        {
            get => OwnerType == uppCaseOwnerOwnerTypes.Distributor ? (mdDistributor)Owner : null;
            set
            {
                Owner = value; NotifyPropertyChanged("Distributor");
                if (value != null)
                {
                    switch (value.ContainsHFTubes.ToUpper())
                    {
                        case @"N\A":
                            OptionIndex1 = 0;
                            break;
                        case "TRUE":
                        case "YES":
                            OptionIndex1 = 1;
                            break;
                        case "FALSE":
                        case "NO":
                            OptionIndex1 = 2;
                            break;
                        default:
                            OptionIndex1 = 0;
                            break;

                    }

                }

            }
        }
        public mdChimneyTray ChimneyTray
        {
            get => OwnerType == uppCaseOwnerOwnerTypes.ChimneyTray ? (mdChimneyTray)Owner : null;
            set { Owner = value; NotifyPropertyChanged("ChimneyTray"); }
        }

        private Visibility _Visibility_Edit = Visibility.Collapsed;
        public Visibility Visibility_Edit { get => _Visibility_Edit; set { _Visibility_Edit = value; NotifyPropertyChanged("Visibility_Edit"); NotifyPropertyChanged("Visibility_EditInverse"); } }
        public Visibility Visibility_EditInverse { get => _Visibility_Edit == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible; set { Visibility_Edit = value == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed; } }

        private bool _AllowEdits = false;
        public bool AllowEdits { get => _AllowEdits; set { _AllowEdits = value; NotifyPropertyChanged("AllowEdits"); Visibility_Edit = value ? Visibility.Visible : Visibility.Collapsed; _IsReadOnly = !value; NotifyPropertyChanged("IsReadOnly"); } }
           private Visibility _Visibility_Distributor = Visibility.Visible;
        public Visibility Visibility_Distributor { get => _Visibility_Distributor; set { _Visibility_Distributor = value; NotifyPropertyChanged("Visibility_Distributor"); } }

        private Visibility _Visibility_ChimneyTray = Visibility.Visible;
        public Visibility Visibility_ChimneyTray { get => _Visibility_ChimneyTray; set { _Visibility_ChimneyTray = value; NotifyPropertyChanged("Visibility_ChimneyTray"); } }

        private Visibility _Visibility = Visibility.Visible;
        public Visibility Visibility { get => _Visibility; set { _Visibility = value; NotifyPropertyChanged("Visibility"); } }
        private bool _IsReadOnly = false;
        public bool IsReadOnly { get => _IsReadOnly; set { _IsReadOnly = value; NotifyPropertyChanged("IsReadOnly"); } }

        #region Editable DataSource Properties

        public PropertyControlViewModel Description => GetPropControlModel("Description", true, bFreeFrom: true);
        public PropertyControlViewModel NozzleLabel => GetPropControlModel("NozzleLabel", IsReadOnly, bFreeFrom: true);
        public PropertyControlViewModel TrayAbove => GetPropControlModel("TrayAbove", IsReadOnly, bFreeFrom: true);
        public PropertyControlViewModel TrayBelow => GetPropControlModel("TrayBelow", IsReadOnly, bFreeFrom: true);
        public PropertyControlViewModel DistributorRequired
        {
            get
            {
                PropertyControlViewModel _rVal = GetPropControlModel("DistributorRequired", IsReadOnly, bFreeFrom: true, "Distributor Required?", aVisibility: Visibility_Distributor);
                return _rVal;
            }
        }
        public PropertyControlViewModel MinimizePressureDrop => GetPropControlModel("MinimizePressureDrop", IsReadOnly, bFreeFrom: true, " Minimize Pressure Drop?", aVisibility: Visibility_Distributor, aChoiceList: new List<string> { "Yes", "No" });
        public PropertyControlViewModel ContainsHFTubes => GetPropControlModel("ContainsHFTubes", IsReadOnly, bFreeFrom: true, "Reboiler Contains HF Tubes?", aVisibility: Visibility_Distributor, aChoiceList: ContainsHFTubesOptions);
      
        private readonly List<string> _ContainsHFTubesOptions = new() { @"N\A", "Yes", "No" };
        public List<string> ContainsHFTubesOptions => _ContainsHFTubesOptions;
        public int ContainsHFTubesOptionIndex { get => OptionIndex1; set => OptionIndex1 = value; }


        #endregion Editable DataSource Properties

        #endregion Properties

        #region Methods
        public override string ToString() => $"MDCaseOwnerViewModel [ {Name} ]";
        public override PropertyControlViewModel GetPropControlModel(string aPropertyName, bool bReadOnly, bool bFreeFrom, string aDisplayName = null, Visibility? aVisibility = null, string aSetVisibiltyName = null, string aNullValueReplacer = null, bool? bZeroAsNullString = null, List<string> aChoiceList = null)
        {
            PropertyControlViewModel _rVal = base.GetPropControlModel(aPropertyName, bReadOnly, bFreeFrom, aDisplayName, aVisibility, aSetVisibiltyName, aNullValueReplacer, bZeroAsNullString, aChoiceList);
            _rVal.Container = this;

            if (_rVal.Property.IsNamed("NozzleLabel,TrayAbove,TrayBelow"))
            {
                _rVal.MaxLength = 5;
            }
            
            return _rVal;
        }
        #endregion Methods

        #region Validation of fields 

        public override void DefineLimits()
        {


            List<ValueLimit> Limits = new() { };

            EditPropertyValueLimits = Limits;
        }

        public override string GetErrorString(string aPropertyName) => GetError(aPropertyName);
       

        protected override string GetError(string aPropertyName)
        {
            string result = null;
            uopProperty prop = EditProps.Item(aPropertyName, true);
            if (prop == null)
                return "";


            //check for limits
            ValueLimit limits = EditPropertyValueLimits?.Find(x => string.Compare(x.PropertyName, aPropertyName, true) == 0);
            if (limits != null)
            {
                result = limits.ValidateProperty(prop, null, DisplayUnits);
            }

            int idx;
            //check inter-dependant values
            if (result == null)
            {
                switch (aPropertyName.ToUpper())
                {
                    case "TRAYABOVE":
                    case "TRAYBELOW":
                    case "NOZZLELABEL":

                        if (string.IsNullOrWhiteSpace(prop.ValueS)) result = $"{prop.DisplayName} Is Required Input";

                        break;
                }
            }


            idx = ErrorCollection.FindIndex(x => string.Compare(x.Item1, aPropertyName, true) == 0);
            if (idx >= 0) ErrorCollection.RemoveAt(idx);
            if (!string.IsNullOrWhiteSpace(result)) ErrorCollection.Add(new Tuple<string, string>(aPropertyName, result));

            NotifyPropertyChanged("ErrorCollection");
           
            return result;
        }


        public override string ValidateInput(out PropertyControlViewModel rErrorControl)
        {
            rErrorControl = null;
            string err = "";
            uopProperty item;
            for (int i = 1; i <= EditProps.Count; i++)
            {
                item = EditProps.Item(i);
                err = GetError(item.Name);
                if (!string.IsNullOrWhiteSpace(err)) break;
            }
            //if (!string.IsNullOrWhiteSpace(err)) SetFocus(item.Name, null);
            string description = Description.Property.ValueS;
            List<iCaseOwner> owners = OwnerType == uppCaseOwnerOwnerTypes.Distributor ? MDProject.Distributors.ToOwnerList() : MDProject.ChimneyTrays.ToOwnerList();
            for (int i = 1; i <= owners.Count; i++)
            {
                if (i != OwnerIndex && OwnerIndex > 0)
                {
                    iCaseOwner owner = owners[i - 1];
                    if (string.Compare(owner.Description, description, true) == 0)
                    {
                        err = $"{owner.OwnerType.GetDescription() } Descriptions Must Be Unique";
                        break;
                    }
                }
            }
            List<uopProperty> eProps = EditProps.ToList;
            List<PropertyControlViewModel> pcontrols = PropertyControls;

            foreach (uopProperty prop in eProps)
            {
                string pname = prop.Name;
                err = GetError(pname);
                if (!string.IsNullOrWhiteSpace(err))
                {
                    rErrorControl = pcontrols.Find(x => string.Compare(x.Property.Name, pname, true) == 0);
                    break;
                }
            }
            return err;
        }

       
        #endregion Validation of fields
    }
}
