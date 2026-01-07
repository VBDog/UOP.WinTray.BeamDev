using UOP.WinTray.Projects;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Interfaces;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.UI.Messages;
using System;
using System.Collections.Generic;
using System.Windows;

namespace UOP.WinTray.UI.ViewModels
{
    public class MDCaseViewModel : PropertyControlContainer
    {


        private readonly List<PropertyControlViewModel> _PropControls = new();

        #region Constructors

        public MDCaseViewModel(MDProjectViewModelBase aParentVM, iCaseOwner owner, iCase aCase, bool bReadOnly )
        {
            if(aParentVM != null)
            {
                MDProject = aParentVM.MDProject;
                DisplayUnits = aParentVM.DisplayUnits;
            }
            IsReadOnly = bReadOnly;
            Owner = owner;
            Case = aCase;
            EditProps = Case == null ? new uopProperties() : Case.CurrentProperties();
            EditProps.Add(new uopProperty("OwnerDescription", owner == null ? "" : owner.Description, aPartType: aCase == null ? uppPartTypes.Undefined : aCase.PartType));
            CreatePropertyControls(bReadOnly, aContainer: this);


        }

        #endregion Constructors

        #region Properties

        public double MaxMassRate => 100000000;
        public double MaxDensity => 200;

        public override bool IsEnglishSelected
        {
            get => base.IsEnglishSelected;
            set { base.IsEnglishSelected = value; NotifyPropertyChanges();}
        }

        private iCaseOwner _Owner = null;
        public iCaseOwner Owner { get => _Owner; set {  _Owner = value; NotifyPropertyChanged("Owner"); } }

        private iCase _Case = null;
        public iCase Case { get => _Case; 
            set 
            { _Case = value; 
            if(_Case == null)
                {
                    IsDistributor = false;
                    IsCT = false;
                    Visibility_Distributor =Visibility.Collapsed;
                    Visibility_ChimneyTray = Visibility.Collapsed;
                    Visibility = Visibility.Collapsed;
                   
                }
                else
                {
                    IsDistributor = value.OwnerType == uppCaseOwnerOwnerTypes.Distributor;
                    IsCT = value.OwnerType == uppCaseOwnerOwnerTypes.ChimneyTray;
                    Visibility_Distributor = IsDistributor ? Visibility.Visible : Visibility.Collapsed;
                    Visibility_ChimneyTray = IsCT ? Visibility.Visible : Visibility.Collapsed;
                 
                    Visibility_DistributorDependantProperties = IsDistributor && IsReadOnly ? Visibility.Visible : Visibility.Collapsed;
                    Visibility_ChimneyTrayDependantProperties = IsCT && IsReadOnly ? Visibility.Visible : Visibility.Collapsed;
                    Visibility = Visibility.Visible;
                }
                NotifyPropertyChanged("Case");
            } 
        }

        public uppCaseOwnerOwnerTypes OwnerType => Case == null ? uppCaseOwnerOwnerTypes.Undefined : Case.OwnerType;
        public int OwnerIndex => Owner == null ? 0 : Owner.Index;
        public int CaseIndex => Case == null ? 0 : Case.Index;

        private bool _IsDistributor = false;
        public bool IsDistributor { get => _IsDistributor; private set { _IsDistributor = value; NotifyPropertyChanged("IsDistributor"); } }

        private bool _IsCT = false;
        public bool IsCT { get => _IsCT; private set { _IsCT = value; NotifyPropertyChanged("IsCT"); } }


        private Visibility _Visibility_Edit = Visibility.Collapsed;
        public Visibility Visibility_Edit { get => _Visibility_Edit; set { _Visibility_Edit = value; NotifyPropertyChanged("Visibility_Edit"); NotifyPropertyChanged("Visibility_EditInverse"); } }
        public Visibility Visibility_EditInverse { get => _Visibility_Edit == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible; set { Visibility_Edit = value == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed; } }
     
        private Visibility _Visibility_Distributor = Visibility.Visible;
        public Visibility Visibility_Distributor { get => _Visibility_Distributor; set { _Visibility_Distributor = value; NotifyPropertyChanged("Visibility_Distributor"); } }

        private Visibility _Visibility_ChimneyTray = Visibility.Visible;
        public Visibility Visibility_ChimneyTray { get => _Visibility_ChimneyTray; set { _Visibility_ChimneyTray = value; NotifyPropertyChanged("Visibility_ChimneyTray"); } }

        private Visibility _Visibility_DistributorDependantProperties = Visibility.Visible;
        public Visibility Visibility_DistributorDependantProperties { get => _Visibility_DistributorDependantProperties; set { _Visibility_DistributorDependantProperties = value; NotifyPropertyChanged("Visibility_DistributorDependantProperties"); } }

        private Visibility _Visibility_ChimneyTrayDependantProperties = Visibility.Visible;
        public Visibility Visibility_ChimneyTrayDependantProperties { get => _Visibility_ChimneyTrayDependantProperties; set { _Visibility_ChimneyTrayDependantProperties = value; NotifyPropertyChanged("Visibility_ChimneyTrayDependantProperties"); } }

        private Visibility _Visibility = Visibility.Visible;
        public Visibility Visibility { get => _Visibility; set { _Visibility = value; NotifyPropertyChanged("Visibility"); } }

        private bool _IsReadOnly = false;
        public bool IsReadOnly { get => _IsReadOnly; set { _IsReadOnly = value; NotifyPropertyChanged("IsReadOnly");  } }
        #endregion Properties

        public string Name { get { string _rVal = _Owner != null ? _Owner.Description  : "Undefined"; _rVal += "::";  _rVal +=  Case != null ? Case.Description : "Undefined"; return _rVal; } }

        #region Editable DataSource Properties
        public PropertyControlViewModel OwnerDescription => GetPropControlModel("OwnerDescription", true, bFreeFrom: true, aDisplayName: mzUtils.UnPascalCase(OwnerType.GetDescription()));
        public PropertyControlViewModel Description => GetPropControlModel("Description", true, bFreeFrom: true);
        public PropertyControlViewModel MinimumOperatingRange => GetPropControlModel("MinimumOperatingRange", IsReadOnly, bFreeFrom: false, "Operating Range (min)");
        public PropertyControlViewModel MaximumOperatingRange => GetPropControlModel("MaximumOperatingRange", IsReadOnly, bFreeFrom: false,"Operating Range (max)");


        public PropertyControlViewModel LiquidRate => GetPropControlModel("LiquidRate", IsReadOnly, bFreeFrom: false, "Feed Liquid Rate" );
        
        public PropertyControlViewModel LiquidDensity =>  GetPropControlModel("LiquidDensity", IsReadOnly, bFreeFrom: false, "Density");
            
        public PropertyControlViewModel VaporRate => GetPropControlModel("VaporRate", IsReadOnly, bFreeFrom: false, "Feed Vapor Rate");
            
        public PropertyControlViewModel VaporDensity => GetPropControlModel("VaporDensity", IsReadOnly, bFreeFrom: false, "Density");

        public PropertyControlViewModel LiquidFromAbove => GetPropControlModel("LiquidFromAbove", IsReadOnly, bFreeFrom: false);
            
        public PropertyControlViewModel LiquidDensityFromAbove => GetPropControlModel("LiquidDensityFromAbove", IsReadOnly, bFreeFrom: false, "Density");

        public PropertyControlViewModel VaporFromBelow => GetPropControlModel("VaporFromBelow", IsReadOnly, bFreeFrom: false);
        public PropertyControlViewModel VaporDensityFromBelow => GetPropControlModel("VaporDensityFromBelow", IsReadOnly, bFreeFrom: false, "Density");

        public PropertyControlViewModel AdditionalTroughLiquidRate => GetPropControlModel("AdditionalTroughLiquidRate", IsReadOnly, bFreeFrom: false, "Additional Liquid to Troughs");
        public PropertyControlViewModel LiquidDrawRate => GetPropControlModel("LiquidDrawRate", IsReadOnly, bFreeFrom: false);

        #endregion Editable DataSource Properties

        #region Calculated DataSource Properties

        public PropertyControlViewModel DesignRate => GetPropControlModel("DesignRate", true, bFreeFrom: false,  aVisibility: !IsReadOnly ? Visibility.Collapsed : Visibility.Visible);
            
       
        public PropertyControlViewModel VaporPercentage => GetPropControlModel("VaporPercentage", true, bFreeFrom: false, aVisibility: !IsReadOnly ? Visibility.Collapsed : Visibility.Visible);

        public PropertyControlViewModel FeedVolumnRate => GetPropControlModel("FeedVolumnRate", true, bFreeFrom: false, aVisibility: !IsReadOnly ? Visibility.Collapsed : Visibility.Visible);
        public PropertyControlViewModel FeedDensity => GetPropControlModel("FeedDensity", true, bFreeFrom: false, aVisibility: !IsReadOnly ? Visibility.Collapsed : Visibility.Visible,aNullValueReplacer : "-", bZeroAsNullString: true);
        public PropertyControlViewModel FeedLiquidVolumeRate => GetPropControlModel("FeedLiquidVolumeRate", true, bFreeFrom: false, aVisibility: !IsReadOnly ? Visibility.Collapsed : Visibility.Visible);
        public PropertyControlViewModel FeedLiquidPercentage => GetPropControlModel("FeedLiquidPercentage", true, bFreeFrom: false, "Feed Liquid/Total Liquid", aVisibility: !IsReadOnly ? Visibility.Collapsed : Visibility.Visible);
        public PropertyControlViewModel FeedVaporVolumeRate => GetPropControlModel("FeedVaporVolumeRate", true, bFreeFrom: false, aVisibility: !IsReadOnly ? Visibility.Collapsed : Visibility.Visible);
        public PropertyControlViewModel FeedVaporPercentage => GetPropControlModel("FeedVaporPercentage", true, bFreeFrom: false, "Feed Vapor/Total Vapor", aVisibility: !IsReadOnly ? Visibility.Collapsed : Visibility.Visible);

        public PropertyControlViewModel DrawAmount => GetPropControlModel("DrawAmount", true, bFreeFrom: false, aVisibility: !IsReadOnly ? Visibility.Collapsed : Visibility.Visible);
        public PropertyControlViewModel LiquidToBelow => GetPropControlModel("LiquidToBelow", true, bFreeFrom: false, aVisibility: !IsReadOnly ? Visibility.Collapsed : Visibility.Visible);


        #endregion Calculated DataSource Properties

        #region Methods

        /// <summary>
        /// Invokes on notify property changed.
        /// </summary>
        private void NotifyPropertyChanges()
        {
          
            try
            {
                
                var props = this.GetType().GetProperties();
                foreach (var item in props)
                {
                    //System.Diagnostics.Debug.Print(item.Name);

                    //if (item.CanWrite)
                    //{
                    //System.Diagnostics.Debug.Print(item.Name);
                    try
                    {
                        NotifyPropertyChanged(item.Name, true);
                    }
                    catch 
                    {
                        Console.WriteLine($"{item.Name} CAUSED AN ERROR HERE");
                    }
                    //}

                }
                List < PropertyControlViewModel > pcontrols = PropertyControls;
                foreach (var item in pcontrols)
                {
                    item.NotifyPropertyChanges();
                }

            }
            catch { }



        }
        public override string ToString() => $"MDCaseViewModel [ {Name} ]";

        public override PropertyControlViewModel GetPropControlModel(string aPropertyName, bool bReadOnly, bool bFreeFrom, string aDisplayName = null, Visibility? aVisibility = null, string aSetVisibiltyName = null, string aNullValueReplacer = null, bool? bZeroAsNullString = null, List<string> aChoiceList = null)
        {
            PropertyControlViewModel _rVal = base.GetPropControlModel(aPropertyName, bReadOnly, bFreeFrom, aDisplayName, aVisibility, aSetVisibiltyName, aNullValueReplacer, bZeroAsNullString, aChoiceList);
            if (_rVal != null) 
            { 
                _rVal.Container = this;
                if (bReadOnly && _rVal.Property.UnitType == uppUnitTypes.Density && _rVal.Property.ValueD <= 0)
                    _rVal.Visibility = Visibility.Collapsed;

               
            }
            return _rVal;
        }
        #endregion Methods

        #region Validation of fields 

        public override void DefineLimits()
        {


            List<ValueLimit> Limits = new() { };
            Limits.Add(new ValueLimit("MinimumOperatingRange", 0.5, 1000, bAllowZero: false, bAbsValue: true));
            Limits.Add(new ValueLimit("MaximumOperatingRange", 0.5, 1000, bAllowZero: false, bAbsValue: true));

            //if(OwnerType == uppCaseOwnerOwnerTypes.ChimneyTray)
            //{
            //    Limits.Add(new ValueLimit("LiquidFromAbove", 0, MaxMassRate, bAllowZero: false, bAbsValue: true));
            //    Limits.Add(new ValueLimit("VaporFromBelow", 0, MaxMassRate, bAllowZero: false, bAbsValue: true));


            //}
            EditPropertyValueLimits = Limits;
        }

        public override string GetErrorString(string aPropertyName) => GetError(aPropertyName);
        

        protected override string GetError(string aPropertyName)
        {
         
            string result = null;
            uopProperties eprops = EditProps;
            uopProperty prop = eprops.Item(aPropertyName, true);
            if (prop == null)
                return "";
            uopProperty dprop = null;

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
                    case "LIQUIDDRAWRATE":
                        {
                            dprop = eprops.Item("LiquidFromAbove", true);
                            if (prop.ValueD > 0 && prop.ValueD > dprop.ValueD) result = $"'{prop.DisplayName}' Must Be Less Than Or Equal To '{ mzUtils.UnPascalCase(dprop.Name, dprop.DisplayName)}' ";
                            break;
                        }
                    case "LIQUIDRATE":
                        {
                            dprop = eprops.Item("LiquidDensity", true);
                            if (prop.ValueD > 0 && dprop.ValueD <= 0) result = $"'{prop.DisplayName}' Must Be Zero Or '{ mzUtils.UnPascalCase(dprop.Name, dprop.DisplayName)}' Must Be Entered";
                            break;
                        }
                    case "LIQUIDDENSITY":
                        {
                            if (OwnerType == uppCaseOwnerOwnerTypes.ChimneyTray)
                            {

                                if (prop.ValueD <= 0) result = $"'{mzUtils.UnPascalCase(prop.Name, prop.DisplayName)}' Must Must Be Entered";

                            }
                            else if (OwnerType == uppCaseOwnerOwnerTypes.Distributor)
                            {
                                dprop = eprops.Item("LiquidRate", true);
                                if (prop.ValueD > 0 && dprop.ValueD <= 0) result = $"'{mzUtils.UnPascalCase(prop.Name, prop.DisplayName)}' Must Be Zero Or '{ dprop.DisplayName}' Must Be Entered";
                            }
                            break;

                        }
                    case "VAPORRATE":
                        {
                            dprop = eprops.Item("VaporDensity", true);
                            if (prop.ValueD > 0 && dprop.ValueD <= 0) result = $"'{prop.DisplayName}' Must Be Zero Or '{ mzUtils.UnPascalCase(dprop.Name, dprop.DisplayName)}' Must Be Entered";
                            break;

                        }
                    case "VAPORDENSITY":
                        {
                            if (OwnerType == uppCaseOwnerOwnerTypes.ChimneyTray)
                            {

                                if (prop.ValueD <= 0) result = $"'{mzUtils.UnPascalCase(prop.Name, prop.DisplayName)}' Must Must Be Entered";

                            }
                            else if (OwnerType == uppCaseOwnerOwnerTypes.Distributor)
                            {
                                dprop = eprops.Item("VaporRate", true);
                                if (prop.ValueD > 0 && dprop.ValueD <= 0) result = $"'{mzUtils.UnPascalCase(prop.Name, prop.DisplayName)}' Must Be Zero Or '{ dprop.DisplayName}' Must Be Entered";
                            }
                            break;

                        }
                    case "LIQUIDFROMABOVE":
                        {
                            if (OwnerType == uppCaseOwnerOwnerTypes.ChimneyTray)
                            {

                                if (prop.ValueD <= 0) result = $"'{prop.DisplayName}' Must Must Be Entered";

                            }
                            else if (OwnerType == uppCaseOwnerOwnerTypes.Distributor)
                            {
                                dprop = eprops.Item("LiquidDensityFromAbove", true);
                                if (prop.ValueD > 0 && dprop.ValueD <= 0) result = $"'{prop.DisplayName}' Must Be Zero Or '{ mzUtils.UnPascalCase(dprop.Name, dprop.DisplayName)}' Must Be Entered";
                            }

                            break;

                        }
                    case "LIQUIDDENSITYFROMABOVE":
                        {
                            dprop = eprops.Item("LiquidFromAbove", true);
                            if (OwnerType == uppCaseOwnerOwnerTypes.ChimneyTray)
                            {
                                if (prop.ValueD <= 0) result = $"'{prop.DisplayName}' Must Must Be Entered";
                            }
                            else if (OwnerType == uppCaseOwnerOwnerTypes.Distributor)
                            {
                                if (prop.ValueD > 0 && dprop.ValueD <= 0) result = $"'{mzUtils.UnPascalCase(prop.Name, prop.DisplayName)}' Must Be Zero Or '{ dprop.DisplayName}' Must Be Entered";
                            }


                            break;

                        }
                    case "VAPORFROMBELOW":
                        {
                            if (OwnerType == uppCaseOwnerOwnerTypes.ChimneyTray)
                            {
                                if (prop.ValueD <= 0) result = $"'{prop.DisplayName}' Must Must Be Entered";
                            }
                            else if (OwnerType == uppCaseOwnerOwnerTypes.Distributor)
                            {
                                dprop = OwnerType == uppCaseOwnerOwnerTypes.ChimneyTray ? eprops.Item("VaporDensity", true) : eprops.Item("VaporDensityFromBelow", true);
                                if (prop.ValueD > 0 && dprop.ValueD <= 0) result = $"'{prop.DisplayName}' Must Be Zero Or '{ mzUtils.UnPascalCase(dprop.Name, dprop.DisplayName)}' Must Be Entered";

                            }



                            break;

                        }
                    case "VAPORDENSITYFROMBELOW":
                        {
                            if (OwnerType == uppCaseOwnerOwnerTypes.ChimneyTray)
                            {
                                if (prop.ValueD <= 0) result = $"'{prop.DisplayName}' Must Must Be Entered";
                            }
                            else if (OwnerType == uppCaseOwnerOwnerTypes.Distributor)
                            {

                                dprop = eprops.Item("VaporFromBelow", true);
                                if (prop.ValueD > 0 && dprop.ValueD <= 0) result = $"'{mzUtils.UnPascalCase(prop.Name, prop.DisplayName)}' Must Be Zero Or '{ dprop.DisplayName}' Must Be Entered";
                            }
                            break;

                        }
                }


            }
            if (result == null)
            {
                double limit = 0;
                switch (prop.UnitType)
                {
                    case uppUnitTypes.Density:
                        {
                            limit = MaxDensity;
                            break;
                        }
                    case uppUnitTypes.BigMassRate:
                    case uppUnitTypes.MassRate:
                        {
                            limit = MaxMassRate;
                            break;
                        }
                }
                if (limit > 0 && prop.ValueD > limit)
                {
                    result = $"'{mzUtils.UnPascalCase(prop.Name, prop.DisplayName)}' Must Be Less Than Or Equal To {  prop.Units.UnitValueString(limit, DisplayUnits, uppUnitFamilies.English)}";
                }

            }

            if (result == null)
            {
                switch (OwnerType)
                {
                    case uppCaseOwnerOwnerTypes.Distributor:
                        {
                            if (eprops.ValueD("LiquidRate") <= 0 && eprops.ValueD("VaporRate") <= 0) result = "Both Feed Liquid Rate and Feed Vapor Rate Cannot Be Zero";
                            break;
                        }
                    case uppCaseOwnerOwnerTypes.ChimneyTray:
                        {
                            break;
                        }
                }
                if (result == null)
                {
                    if (eprops.ValueD("MinimumOperatingRange") >= eprops.ValueD("MaximumOperatingRange")) result = "Minimum Operation Range Must Less Than Maximum Operating Range";
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
            string pname = "";
            FocusTarget = "";
            List<uopProperty> eProps = EditProps.ToList;
            List<PropertyControlViewModel> pcontrols = PropertyControls;

            foreach (uopProperty item in eProps)
            {
                pname = item.Name;
                err = GetError(pname);
                if (!string.IsNullOrWhiteSpace(err))
                {
                    rErrorControl = pcontrols.Find(x => string.Compare(x.Property.Name, pname, true) == 0);
                    break;
                }
            }

            //if (!string.IsNullOrWhiteSpace(err)) 
            //    SetFocus(pname);

            return err;
        }

        #endregion Validation of fields
    }
}
