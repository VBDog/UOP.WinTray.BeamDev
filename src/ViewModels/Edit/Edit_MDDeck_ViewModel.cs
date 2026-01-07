using UOP.WinTray.Projects;
using UOP.WinTray.Projects.Enums;
using MvvmDialogs;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using UOP.WinTray.UI.Messages;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Parts;

using System.Windows.Media;
using UOP.WinTray.UI.Commands;

namespace UOP.WinTray.UI.ViewModels
{
    /// <summary>
    /// edit class to have the methods required while performing Deck edit functionality. 
    /// </summary>
    public class Edit_MDDeck_ViewModel : MDProjectViewModelBase, IModalDialogViewModel
    {

        #region Constructor

        public Edit_MDDeck_ViewModel(mdProject project, string fieldname = "") : base()
        {
           
            FocusTarget = fieldname;
            MDProject = project;
            if (MDProject == null) return;

            try
            {
                BusyMessage = "Loading Deck Details.."; ;
                IsEnabled = false;

                DisplayUnits = MDProject.DisplayUnits;

                uopProperties eprops = MDAssy.Deck.CurrentProperties();

                //set the categories so we know where to put the values back on save
                eprops.SetCategories("DECK",aPartType:uppPartTypes.Deck);
                  eprops.Append(MDAssy.DesignOptions.CurrentProperties(), aCategory: "DESIGN_OPTIONS", aPartType: uppPartTypes.DesignOptions);
                

               EditProps = eprops;

                GlobalProjectTitle = $"{MDRange.Name(true)}.Deck";



            }
            finally
            {
                Validation_DefineLimits();
                IsEnabled = true;
                IsOkBtnEnable = true;

            }
        }

        #endregion Constructors

        #region Properties

        public override Visibility VisibilityECMD { get { return (ProjectType == uppProjectTypes.MDSpout) ? base.VisibilityECMD : Visibility.Collapsed ; } set => base.VisibilityECMD = value; }

        public override bool IsEnglishSelected
        {
            get => base.IsEnglishSelected;
            set { base.IsEnglishSelected = value; NotifyPropertyChanges(); }
        }
        
        public bool HasTiledDecks
        { 
            get => EditProps.ValueB("HasTiledDecks"); 
            set { EditProps.SetValue("HasTiledDecks", value); NotifyPropertyChanged("HasTiledDecks"); }  
        }

        public bool HasBubblePromoters
        {
            get => EditProps.ValueB("HasBubblePromoters");
            set { EditProps.SetValue("HasBubblePromoters", value); NotifyPropertyChanged("HasBubblePromoters"); }
        }

        public string Fp
        {
            get => EditProps.DisplayValueString("Fp");
            set { EditProps.SetDisplayUnitValue("Fp", value); NotifyPropertyChanged("Fp"); }
        }
        public string SlottingPercentage
        {
            get => EditProps.DisplayValueString("SlottingPercentage");
            set { EditProps.SetDisplayUnitValue("SlottingPercentage", value); NotifyPropertyChanged("SlottingPercentage"); }
        }
        public string PerfDiameter
        {
            get => EditProps.DisplayValueString("PerfDiameter");
            set { EditProps.SetDisplayUnitValue("PerfDiameter", value); NotifyPropertyChanged("PerfDiameter"); }
        }
        public string FEDorAPPHeight
        {
            get => EditProps.DisplayValueString("FEDorAPPHeight",true,aZeroValue:DefaultAPCleance);
            set { EditProps.SetDisplayUnitValue("FEDorAPPHeight", value); NotifyPropertyChanged("FEDorAPPHeight");  NotifyPropertyChanged("ForegroundColor_FEDorAPPHeight"); }
            
        }

        private string _Caption_FEDorAPPHeight = "AP Pan Deck Clrc. : ";
        public string Caption_FEDorAPPHeight
        {
            get => _Caption_FEDorAPPHeight;
            set { _Caption_FEDorAPPHeight= value; NotifyPropertyChanged("Caption_FEDorAPPHeight"); }
        }

        public string CDP
        {
            get => EditProps.DisplayValueString("CDP",aZeroValue: DefaultCDP);
            set { EditProps.SetDisplayUnitValue("CDP", value); NotifyPropertyChanged("CDP");   NotifyPropertyChanged("ForegroundColor_CDP"); }
        }
        public string ManwayCount
        {
            get => EditProps.DisplayValueString("ManwayCount", true);
            set { EditProps.SetDisplayUnitValue("ManwayCount", value); NotifyPropertyChanged("ManwayCount"); }
        }


        private readonly List<string> _SpliceStyles = new() { "Slot & Tab", "Angles" };
        public List<string> SpliceStyles => _SpliceStyles;

        private string _SelectedSpliceStyle = "Angles";
        public string SelectedSpliceStyle
        {
            get => _SelectedSpliceStyle;
            set { _SelectedSpliceStyle = value; NotifyPropertyChanged("SelectedSpliceStyle"); }
        }

        public bool AntiPenerationPans => FlowDeviceType == uppFlowDevices.APPans;
          
        
        public bool FlowEnhancementDevices => FlowDeviceType != uppFlowDevices.None; 
        

        /// <summary>
        /// Proeprty to get which punch direction the user selected.
        /// </summary>
        public uppPunchDirections PunchDirection
        {
            get => (uppPunchDirections)EditProps.ValueI("PunchDirection");
            set { EditProps.SetValue("PunchDirection", value); }
        }
        /// <summary>
        /// Proeprty to get which punch direction the user selected.
        /// </summary>
        public uppFlowDevices FlowDeviceType
        {
            get => (uppFlowDevices)EditProps.ValueI("FlowDeviceType");
            set {
                EditProps.SetValue("FlowDeviceType", value); 
             
                _Caption_FEDorAPPHeight = (value == uppFlowDevices.FED) ? "FED Deck Clrc. : " : "AP Pan Deck Clrc. : ";
                _Visiblity_DeviceData = (value == uppFlowDevices.None) ? Visibility.Collapsed : Visibility.Visible;
                NotifyPropertyChanged("Caption_FEDorAPPHeight");
                NotifyPropertyChanged("Visiblity_DeviceData");

            }
        }

        public string ToolTip_FEDorAPPHeight =>  $"Deck To Device Clearance - Default( {Units_Linear.UnitValueString(DefaultAPCleance, DisplayUnits) } )";

        public Brush ForegroundColor_FEDorAPPHeight
        {
            get
            {
                double dVal = DefaultAPCleance;
                double aVal = (EditProps != null) ? EditProps.ValueD("FEDorAPPHeight", 0) : 0;
                return (aVal != dVal && aVal != 0) ? Brushes.Blue : Brushes.Black;
            }
        }

        public string ToolTip_CDP => $" Default CDP Height = ( {Units_Linear.UnitValueString(DefaultCDP, DisplayUnits) } )";

        public Brush ForegroundColor_CDP
        {
            get
            {
                double dVal = DefaultCDP;
                double aVal = (EditProps != null) ? EditProps.ValueD("CDP", 0) : 0;
                return (aVal != dVal && aVal != 0) ? Brushes.Blue : Brushes.Black;
            }
        }

        public double DefaultAPCleance => mdUtils.DefaultAPPanClearance(MDRange);


        public double DefaultCDP => mdUtils.DefaultCDP(MDRange);


        private Visibility _Visiblity_DeviceData;
        public Visibility Visiblity_DeviceData
        {
            get => (VisibilityMDSpout == Visibility.Visible) ? _Visiblity_DeviceData : Visibility.Collapsed;
            set { _Visiblity_DeviceData = value; NotifyPropertyChanged("Visiblity_DeviceData"); }
        }

       
        /// <summary>
        /// Property to get and set the Slot Type
        /// </summary>
        public uppFlowSlotTypes SlotType
        {
            get => (uppFlowSlotTypes)EditProps.ValueI("SlotType"); 
            set  { EditProps.SetValue("SlotType", value); NotifyPropertyChanged("SlotType"); }
        }



        /// <summary>
        /// Dialogservice result
        /// </summary>
        private bool? _DialogResult;
        public bool? DialogResult
        {
            get => _DialogResult;
            private set { _DialogResult = value; NotifyPropertyChanged("DialogResult"); }
        }

        #endregion Properties


        #region Commands

        private DelegateCommand _CMD_Cancel;
        public ICommand Command_Cancel { get { if (_CMD_Cancel == null)  _CMD_Cancel = new DelegateCommand(param => Execute_Cancel()); return _CMD_Cancel; } }

        private DelegateCommand _CMD_OK;
        public ICommand Command_Ok { get { if (_CMD_OK == null) _CMD_OK = new DelegateCommand(param => Execute_Save()); return _CMD_OK; } }



        /// <summary>
        /// Close the Edit form with no changes
        /// </summary>
        private void Execute_Cancel()
        {
            DialogResult = false;
        }

        /// <summary>
        /// Save the edited properties back to the project object
        /// </summary>
        /// <returns></returns>
        private void Execute_Save()
        {
            try
            {

                IsOkBtnEnable = false;
                IsEnabled = false;
                BusyMessage = "Calculation in progress..";
                IsEnabled = false;
                if (MDAssy == null) return;
           
                ErrorMessage = ValidateInput();
                if (string.IsNullOrEmpty(ErrorMessage))
                {
                    Save();
                }
            }
            finally
            {
                IsOkBtnEnable = true;
                IsEnabled = true;
                BusyMessage = "";
                IsEnabled = true;
            }
        }



        #endregion Commands

        #region Methods


        /// <summary>
        /// Invokes on notify property changed for range properties.
        /// </summary>
        private void NotifyPropertyChanges()
        {
            //uopProperties props = EditProps;
            NotifyPropertyChanged("FEDorAPPHeight");
            NotifyPropertyChanged("CDP");
            NotifyPropertyChanged("PerfDiameter");
            NotifyPropertyChanged("ToolTip_FEDorAPPHeight");
            NotifyPropertyChanged("ForegroundColor_FEDorAPPHeight");
            NotifyPropertyChanged("ToolTip_CDP");
            NotifyPropertyChanged("ForegroundColor_CDP");
            NotifyPropertyChanged("ErrorMessage");
            NotifyPropertyChanged("VisibilityErrorMessage");
            try
            {
                var props = this.GetType().GetProperties();
                foreach (var item in props)
                {
                    //System.Diagnostics.Debug.Print(item.Name);

                    if (item.CanWrite)
                    {
                        //System.Diagnostics.Debug.Print(item.Name);
                        NotifyPropertyChanged(item.Name);
                    }

                }
            }
            catch { }

        }
    
        private void Cancel() => DialogResult = true;
        
        /// <summary>
        /// Save the updated values
        /// </summary>
        private void Save()
        {
          
            Canceled = true;
            Message_Refresh refresh = null;
            try
            {
                
                if (EditProps.ValueD("CDP") <= 0) EditProps.SetValueD("CDP", DefaultCDP);
                if(FlowDeviceType == uppFlowDevices.None)
                {
                    EditProps.SetValueD("FEDorAPPHeight", 0);
                }
                else
                    {
                    if (EditProps.ValueD("FEDorAPPHeight") <= 0) EditProps.SetValueD("FEDorAPPHeight", DefaultAPCleance);

                }


                uopProperties changes = GetEditedProperties(); // EditProps.GetByValueChange(true);
                refresh = new Message_Refresh(bSuppressTree: true, aPartTypeList: new List<uppPartTypes>() { uppPartTypes.Deck , uppPartTypes.DesignOptions }, bCloseDocuments: true);
                
                if (changes.Count > 0)
                {
                    Canceled = false;
                    mdDeck deck  = MDAssy.Deck;
                    mdDesignOptions dopt = MDAssy.DesignOptions;
                    bool invalidateall = false;
                    string pname;
                    foreach (var change in changes)
                    {
                        uopProperty propchange = null;
                        pname = change.Name.ToUpper();
                        switch (change.PartType)
                        {
                            case uppPartTypes.Deck:
                                propchange = deck.PropValSet(change.Name, change.Value, bSuppressEvnts: true);
                                if (propchange == null) break; 
                                 MDAssy.Notify(change);
                                
                                    break;
                        
                            case uppPartTypes.DesignOptions:
                                propchange = dopt.PropValSet(change.Name, change.Value, bSuppressEvnts: true);
                                if (propchange == null) break;
                                MDAssy.Notify(change);
                                break;
                          }
                        if (propchange != null)  refresh.Changes.Add(propchange);
                    }
                    if (invalidateall)
                    {
                        refresh.PartTypeList.Clear();
                        refresh.SuppressImage = false;
                        MDAssy.ResetSubComponents();
                    }
                }
                else
                {
                    Canceled = true;
                }

            }
            catch { }
            finally
            {
                IsEnabled = true;
                DialogResult = !Canceled;
                IsEnabled = true;
                IsOkBtnEnable = true;
            }
        }

      

        /// <summary>
      

        
        #endregion

        public override void SetFocus(string aControlName)
        {
            if (string.IsNullOrWhiteSpace(aControlName)) aControlName = FocusTarget;
            if (string.IsNullOrWhiteSpace(aControlName)) return;


            string pname = aControlName.Trim();

         
            if (!EditProps.TryGet(pname, out uopProperty prop))
            {
                EditProps.TryGet("Fp", out prop);
            }
            if (prop != null) base.SetFocus(prop.Name);


        }
        #region Validation

        /// <summary>
        /// Validates a field based on its property
        /// name and returns a list of errors
        /// </summary>
        /// <param name="propertyName">Name of the field</param>
        /// <param name="validationErrors">List of Validation Errors</param>
        /// <returns>Wheather any error exists or not</returns>
        public override bool ValidateProperty(string propertyName, out ICollection<CustomErrorType> validationErrors)
        {
            validationErrors = new List<CustomErrorType>();
            switch (propertyName)
            {
                // Check for ShellId
                case "Fp":
                
                    break;
                case "PerfDiameter":
                    validationErrors = ValidateDeckPerforationDiameter();
                    break;
                default:
                    break;
            }
            return validationErrors.Count == 0;
        }

       

        /// <summary>
        /// Checks whether DeckPerforation Diameter is in Valid Range or Not
        /// </summary>
        /// <returns>List of validation errors</returns>
        public List<CustomErrorType> ValidateDeckPerforationDiameter()
        {
            List<CustomErrorType> _rVal = new();
            if (EditProps.ValueD("PerfDiameter") > 2 || EditProps.ValueD("PerfDiameter") < 0.25)
            {
                _rVal.Add(new CustomErrorType("Min Value: " + 0.25 + ", Max Value: " + 2, Severity.ERROR));
            }
            return _rVal;
        }

        #endregion

        #region Validation of fields 

        private void Validation_DefineLimits()
        {


            List<ValueLimit> Limits = new() { };
            Limits.Add(new ValueLimit("Fp", (double)0, (double)35, bAllowZero: false));
            Limits.Add(new ValueLimit("PerfDiameter", (double)0, (double)1, bAllowZero: false));
            Limits.Add(new ValueLimit("SlottingPercentage", (double)0.1, (double)1,bAllowZero:VisibilityECMD == Visibility.Collapsed));
            Limits.Add(new ValueLimit("ManwayCount", (int)-1, (int)Math.Pow(MDAssy.PanelCount - 2, 2),bAllowZero:true ));
            Limits.Add(new ValueLimit("FEDorAPPHeight", (double)0.25, MDAssy.Downcomer().HeightAboveDeck - 0.25 , bAllowZero: true));
           EditPropertyValueLimits = Limits;
        }
        public override void Activate(Window myWindow)
        {
            if (Activated || MDAssy == null) return;
            base.Activate(myWindow);
            if (EditPropertyValueLimits == null) Validation_DefineLimits();
            if (!string.IsNullOrWhiteSpace(FocusTarget)) SetFocus(FocusTarget);
        }

        protected override string GetError(string aPropertyName)
        {
            if (!Activated)
                return "";
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
                double dlimit;
                
                double dval;
              
                switch (aPropertyName.ToUpper())
                {

                    case "CDP":

                        if (VisibilityECMD == Visibility.Visible) 
                        {
                            dval = EditProps.ValueD("");
                            if (dval > 0)
                            {
                                dlimit = MDAssy.Downcomer().How + 2;
                                if (dval < dlimit) { 
                                result = $"Baffle Height Must Be At Least { Units_Linear.UnitValueString(dlimit,DisplayUnits)}";
                                }
                                else
                                {
                                    dlimit = MDRange.RingSpacing;
                                    if (dval >= dlimit)
                                    {
                                        return ErrorMessage = $"Baffle Height Must Be Less Than The Tray Spacing In This\n Tray Section ({ Units_Linear.UnitValueString(dlimit,DisplayUnits)})";
                                    }

                                }

                            }
                        }
                        break;

                    case "FEDORAPPANHEIGHT":
              break;
                }
            }


            idx = ErrorCollection.FindIndex(x => string.Compare(x.Item1, aPropertyName, true) == 0);
            if (idx >= 0) ErrorCollection.RemoveAt(idx);
            if (!string.IsNullOrWhiteSpace(result)) ErrorCollection.Add(new Tuple<string, string>(aPropertyName, result));

            NotifyPropertyChanged("ErrorCollection");
            Validate_ShowErrors();
            return result;
        }
        private void Validate_ShowErrors()
        {

            if (ErrorCollection == null) { ErrorMessage = ""; return; }
            if (ErrorCollection.Count <= 0) { ErrorMessage = ""; return; }
            string msg = ErrorCollection[0].Item2;
            ErrorMessage = msg;
        }

        private string ValidateInput()
        {
            if (EditProps == null) return "";
            string err = "";
            uopProperty item = null;
            for (int i = 1; i <= EditProps.Count; i++)
            {
                item = EditProps.Item(i);
                err = GetError(item.Name);
                if (!string.IsNullOrWhiteSpace(err)) break;
            }
            //if (!string.IsNullOrWhiteSpace(err)) SetFocus(item.Name, null);
            return err;
        }


        #endregion Validation of fields
    }
}
