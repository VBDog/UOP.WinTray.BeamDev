using UOP.WinTray.Projects;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Parts;
using MvvmDialogs;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows;
using UOP.WinTray.UI.Commands;
using UOP.WinTray.UI.Messages;
using System.Windows.Media;

namespace UOP.WinTray.UI.ViewModels
{
    public class Edit_MDDesignOptions_ViewModel : MDProjectViewModelBase, IModalDialogViewModel
    {

        #region Constructors
    
    public Edit_MDDesignOptions_ViewModel(mdProject project, string fieldname = "") : base()
        {


            MDProject = project;
            FocusTarget = fieldname;
            if (MDProject == null) return;

            try
            {
                BusyMessage = "Loading Part Details.."; ;
                IsEnabled = false;

                IsEnglishSelected = MDProject.DisplayUnits != uppUnitFamilies.Metric;

                uopProperties eprops = MDAssy.Deck.CurrentProperties();

                //set the categories so we know where to put the values back on save
                eprops.SetCategories("DECK", aPartType: uppPartTypes.Deck);
                eprops.Append(MDAssy.DesignOptions.CurrentProperties(), aCategory: "DESIGN_OPTIONS", aPartType: uppPartTypes.DesignOptions);

                EditProps = eprops;

                GlobalProjectTitle = $"{MDRange.Name(true)}.DesignOptions";



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

        public override bool IsEnglishSelected
        {
            get => base.IsEnglishSelected;
            set { base.IsEnglishSelected = value; NotifyPropertyChanges(); }
        }
        public double DefaultCDP => mdUtils.DefaultCDP(MDRange);


        private Visibility _Visiblity_APPans = Visibility.Visible ;
        public Visibility Visiblity_APPans
        {
            get => _Visiblity_APPans;
            set { _Visiblity_APPans = value; NotifyPropertyChanged("Visiblity_APPans"); }
        }


        public string CDP
        {
            get => EditProps.DisplayValueString("CDP", aZeroValue: DefaultCDP);
            set { EditProps.SetDisplayUnitValue("CDP", value); NotifyPropertyChanged("CDP"); NotifyPropertyChanged("ForegroundColor_CDP"); }
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
        public Brush ForegroundColor_RCSpaceMax
        {
            get
            {
                double dVal = DefaultRCSpacing;
                double aVal = (EditProps != null) ? EditProps.ValueD("MaxRingClipSpacing", 0) : 0;
                return (aVal != dVal && aVal != 0) ? Brushes.Blue : Brushes.Black;
            }
        }
        public Brush ForegroundColor_RCSpaceMoon
        {
            get
            {
                double dVal = DefaultRCSpacing;
                double aVal = (EditProps != null) ? EditProps.ValueD("MoonRingClipSpacing", 0) : 0;
                return (aVal != dVal && aVal != 0) ? Brushes.Blue : Brushes.Black;
            }
        }

        public Brush ForegroundColor_BoltSpacing
        { 
            get
            {
                double dVal = DefaultBoltSpacing;
                double aVal = (EditProps != null) ? EditProps.ValueD("JoggleBoltSpacing", 0) : 0;
                return (aVal != dVal && aVal != 0) ? Brushes.Blue : Brushes.Black;
            }
        }

        public Brush ForegroundColor_BaffleMount
        {
            get
            {
                double dVal = DefaultBaffleMountPercentage;
                double aVal = (EditProps != null) ? EditProps.ValueD("BaffleMountPercentage", 0) : 0;
                return (aVal != dVal && aVal != 0) ? Brushes.Blue : Brushes.Black;
            }
        }

        public string FEDorAPPHeight
        {
            get => EditProps.DisplayValueString("FEDorAPPHeight", true, aZeroValue: DefaultAPCleance);
            set { EditProps.SetDisplayUnitValue("FEDorAPPHeight", value); NotifyPropertyChanged("FEDorAPPHeight"); NotifyPropertyChanged("ForegroundColor_FEDorAPPHeight"); }

        }


        public string ToolTip_FEDorAPPHeight => $"Deck To Device Clearance - Default( {Units_Linear.UnitValueString(DefaultAPCleance, DisplayUnits) } )";
        public string ToolTip_RCSpacing => $"Ring Clip Spacing - Default( {Units_Linear.UnitValueString(DefaultRCSpacing, DisplayUnits) } )";
        public string ToolTip_BoltSpacing => $"Splice Bolt - Default( {Units_Linear.UnitValueString(DefaultBoltSpacing, DisplayUnits) } )";
        public string ToolTip_BaffleMount => $"Default Value = {DefaultBaffleMountPercentage} %";

        public Brush ForegroundColor_FEDorAPPHeight
        {
            get
            {
                double dVal = DefaultAPCleance;
                double aVal = (EditProps != null) ? EditProps.ValueD("FEDorAPPHeight", 0) : 0;
                return (aVal != dVal && aVal != 0) ? Brushes.Blue : Brushes.Black;
            }
        }

        public double DefaultRCSpacing => 9;

        public double DefaultBoltSpacing => 6;

        public double DefaultBaffleMountPercentage => 50;


        public double DefaultAPCleance => mdUtils.DefaultAPPanClearance(MDRange);


        private string _Caption_FEDorAPPHeight = "AP Pan Deck Clrc. : ";
        public string Caption_FEDorAPPHeight
        {
            get => _Caption_FEDorAPPHeight;
            set { _Caption_FEDorAPPHeight = value; NotifyPropertyChanged("Caption_FEDorAPPHeight"); }
        }
        /// <summary>
        /// Proeprty to get which punch direction the user selected.
        /// </summary>
        public uppFlowDevices FlowDeviceType
        {
            get => (uppFlowDevices)EditProps.ValueI("FlowDeviceType");
            set
            {
                EditProps.SetValue("FlowDeviceType", value);

                _Caption_FEDorAPPHeight = (value == uppFlowDevices.FED) ? "FED Deck Clrc. : " : "AP Pan Deck Clrc. : ";
                Visiblity_DeviceData = (value == uppFlowDevices.None) ? Visibility.Collapsed : Visibility.Visible;
                NotifyPropertyChanged("Caption_FEDorAPPHeight");
              
            }
        }


        private Visibility _Visiblity_DeviceData;
        public Visibility Visiblity_DeviceData
        {
            get => _Visiblity_DeviceData;
            set { _Visiblity_DeviceData = value; NotifyPropertyChanged("Visiblity_DeviceData"); }
        }

        public string MaxRingClipSpacing
        {
            get => EditProps.DisplayValueString("MaxRingClipSpacing", true, aZeroValue:DefaultRCSpacing);
            set { EditProps.SetDisplayUnitValue("MaxRingClipSpacing", value); NotifyPropertyChanges(); }
        }
        public string BottomDCHeight
        {
            get => EditProps.DisplayValueString("BottomDCHeight", true, aZeroValue: DefaultRCSpacing);
            set { EditProps.SetDisplayUnitValue("BottomDCHeight", value); NotifyPropertyChanges(); }
        }
        public string BaffleMountPercentage
        {
            get => EditProps.DisplayValueString("BaffleMountPercentage", true, aZeroValue: DefaultBaffleMountPercentage);
            set { EditProps.SetDisplayUnitValue("BaffleMountPercentage", value); NotifyPropertyChanges(); }

        }


        public string MoonRingClipSpacing
        {
            get => EditProps.DisplayValueString("MoonRingClipSpacing", true, aZeroValue: DefaultRCSpacing);
            set { EditProps.SetDisplayUnitValue("MoonRingClipSpacing", value); NotifyPropertyChanges(); }
        }

        public string JoggleBoltSpacing
        {
            get => EditProps.DisplayValueString("JoggleBoltSpacing", true, aZeroValue: DefaultBoltSpacing);
            set { EditProps.SetDisplayUnitValue("JoggleBoltSpacing", value); NotifyPropertyChanges(); }

        }

        public string JoggleAngle
        {
            get => EditProps.DisplayValueString("JoggleAngle", true);
            set { EditProps.SetDisplayUnitValue("JoggleAngle", value); NotifyPropertyChanged("JoggleAngle"); }

        }
        public string SpliceAngle
        {
            get => EditProps.DisplayValueString("SpliceAngle", true);
            set { EditProps.SetDisplayUnitValue("SpliceAngle", value); NotifyPropertyChanged("SpliceAngle"); }

        }
      
        public string APPanPerfDiameter
        {
          
             get => EditProps.DisplayValueString("APPanPerfDiameter", true);
            set { EditProps.SetDisplayUnitValue("APPanPerfDiameter", value); NotifyPropertyChanged("APPanPerfDiameter"); }

        }

        public string APPanPercentOpen
        {
            get => EditProps.DisplayValueString("APPanPercentOpen", true);
            set { EditProps.SetDisplayUnitValue("APPanPercentOpen", value); NotifyPropertyChanged("APPanPercentOpen"); }

        }


        public bool WeldedStiffeners 
        {
            get { return EditProps.ValueB("WeldedStiffeners"); }
            set { EditProps.SetValue("WeldedStiffeners", value); NotifyPropertyChanged("WeldedStiffeners"); }
            
        }


        public bool HasBubblePromoters
        {
            get => EditProps.ValueB("HasBubblePromoters");
            set { EditProps.SetValue("HasBubblePromoters", value); NotifyPropertyChanged("HasBubblePromoters"); }
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
        public ICommand Command_Cancel
        {
            get
            {
                if (_CMD_Cancel == null) _CMD_Cancel = new DelegateCommand(param => Execute_Cancel());
                return _CMD_Cancel;
            }
        }

        private DelegateCommand _CMD_OK;
        public ICommand Command_Ok
        {
            get
            {
                if (_CMD_OK == null) _CMD_OK = new DelegateCommand(param => Execute_Save());
                
                return _CMD_OK;
            }
        }

        #endregion Commands

        #region Methods


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

        private void Save()
        {

            Canceled = true;
            Message_Refresh refresh = null;
            try
            {

                if (EditProps.ValueD("CDP") <= 0) EditProps.SetValueD("CDP", DefaultCDP);
                if (FlowDeviceType == uppFlowDevices.None)
                {
                    EditProps.SetValueD("FEDorAPPHeight", 0);
                }
                else
                {
                    if (EditProps.ValueD("FEDorAPPHeight") <= 0) EditProps.SetValueD("FEDorAPPHeight", DefaultAPCleance);

                }

                if (EditProps.ValueD("MaxRingClipSpacing") <= 0) EditProps.SetValueD("MaxRingClipSpacing", DefaultRCSpacing);
                if (EditProps.ValueD("MoonRingClipSpacing") <= 0) EditProps.SetValueD("MoonRingClipSpacing", DefaultRCSpacing);
                if (EditProps.ValueD("JoggleBoltSpacing") <= 0) EditProps.SetValueD("JoggleBoltSpacing", DefaultBoltSpacing);
                if(VisibilityECMD == Visibility.Visible)
                {
                    if (EditProps.ValueD("BaffleMountPercentage") <= 0) EditProps.SetValueD("BaffleMountPercentage", DefaultBaffleMountPercentage);
                    
                }


                uopProperties changes = GetEditedProperties(); // EditProps.GetByValueChange(true);
                refresh = new Message_Refresh(bSuppressTree: true, aPartTypeList: new List<uppPartTypes>() { uppPartTypes.Deck, uppPartTypes.DesignOptions }, bCloseDocuments: true);
              
                if (changes.Count > 0)
                {
                    Canceled = false;
                    mdDeck deck = MDAssy.Deck;
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
                        if (propchange != null) refresh.Changes.Add(propchange);
                        pname = change.Name.ToUpper();

                        switch (pname)
                        {
                            case "HASBUBBLEPROMOTERS":
                                refresh.SuppressImage = false;
                                break;

                            case "WELDEDSTIFFENERS":
                            case "BAFFLEMOUNTPERCENTAGE":
                                refresh.SuppressTree = false;
                                break;

                            case "BOTTOMDCHEIGHT":
                                refresh.SuppressTree = false;
                                break;
                            default:
                                break;

                        }

                      
                      

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
                RefreshMessage = !Canceled ? refresh : null; 
                DialogResult = !Canceled;
                
            }

        }

     
        /// <summary>
        /// Close the Edit form with no changes
        /// </summary>
        private void Execute_Cancel()
        {
            DialogResult = false;
        }

        #endregion

        #region Validation of fields 

        private void Validation_DefineLimits()
        {


            List<ValueLimit> Limits = new() { };
            Limits.Add(new ValueLimit("MaxRingClipSpacing", (double)3, (double)20, bAllowZero: true));
            Limits.Add(new ValueLimit("MoonRingClipSpacing", (double)3, (double)20, bAllowZero: true));
            Limits.Add(new ValueLimit("JoggleBoltSpacing", (double)3, (double)10, bAllowZero: true));
            Limits.Add(new ValueLimit("JoggleAngle", (double)0.5, (double)6, bAllowZero: false));
            Limits.Add(new ValueLimit("SpliceAngle", (double)0.5, (double)6, bAllowZero: false));
           
            Limits.Add(new ValueLimit("APPanPerfDiameter", (double)0.125, (double)2, bAllowZero: false));
            Limits.Add(new ValueLimit("APPanPercentOpen", (double)3, (double)75, bAllowZero: false));
            

            if (VisibilityECMD == Visibility.Visible)
            {
                Limits.Add(new ValueLimit("BaffleMountPercentage", (double)25, (double)75, bAllowZero: true));
                Limits.Add(new ValueLimit("CDP", MDAssy.Downcomer().How + 2, MDRange.RingSpacing, bAllowZero: false));

            }
              Limits.Add(new ValueLimit("FEDorAPPHeight", (double)0.25, MDAssy.Downcomer().HeightAboveDeck - 0.25, bAllowZero: true));
            Limits.Add(new ValueLimit("BottomDCHeight", null, 1.5 * MDAssy.Downcomer().Height, bAllowZero: true));

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
                            dval = EditProps.ValueD(aPropertyName);
                            if (dval > 0)
                            {
                                dlimit = MDAssy.Downcomer().How + 2;
                                if (dval < dlimit)
                                {
                                    result = $"Baffle Height Must Be At Least { Units_Linear.UnitValueString(dlimit, DisplayUnits)}";
                                }
                                else
                                {
                                    dlimit = MDRange.RingSpacing;
                                    if (dval >= dlimit)
                                    {
                                        return ErrorMessage = $"Baffle Height Must Be Less Than The Tray Spacing In This\n Tray Section ({ Units_Linear.UnitValueString(dlimit, DisplayUnits)})";
                                    }

                                }

                            }
                        }
                        break;

                    case "BOTTOMDCHEIGHT":
                        {

                            dval = EditProps.ValueD(aPropertyName);
                            if (dval > 0)
                            {
                                dlimit = MDAssy.Downcomer().Height + 0.5;
                                if (dval < dlimit)
                                {
                                    result = $"Bottom Downcomer Height Must Be At Least { Units_Linear.UnitValueString(dlimit, DisplayUnits)} ( DC Height + {Units_Linear.UnitValueString(0.5, DisplayUnits)})";
                                }
                                else
                                {
                                   
                                    dlimit = 2.0 * MDAssy.Downcomer().Height;
                                    if (dval >= dlimit)
                                    {
                                        return ErrorMessage = $"Bottom Downcomer Height Must Be At Less Than or Equal To { Units_Linear.UnitValueString(dlimit, DisplayUnits)} ( 2.0 x DC Height ) ";
                                    }
                                }

                            }
                            break;
                        }


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

        /// <summary>
        /// Invokes on notify property changed for range properties.
        /// </summary>
        private void NotifyPropertyChanges()
        {
            //uopProperties props = EditProps;
            NotifyPropertyChanged("FEDorAPPHeight");
            NotifyPropertyChanged("CDP");
            NotifyPropertyChanged("ToolTip_FEDorAPPHeight");
            NotifyPropertyChanged("ToolTip_RCSpacing");
            NotifyPropertyChanged("ToolTip_BoltSpacing");
            NotifyPropertyChanged("ToolTip_BaffleMount");
     
            NotifyPropertyChanged("ForegroundColor_FEDorAPPHeight");
            NotifyPropertyChanged("ToolTip_CDP");
            NotifyPropertyChanged("ForegroundColor_CDP");
            NotifyPropertyChanged("ForegroundColor_RCSpaceMax");
            NotifyPropertyChanged("ForegroundColor_RCSpaceMoon");
            NotifyPropertyChanged("ForegroundColor_BoltSpacing");
            NotifyPropertyChanged("ForegroundColor_BaffleMount");


            NotifyPropertyChanged("ErrorMessage");
            NotifyPropertyChanged("VisibilityErrorMessage");
            NotifyPropertyChanged("MaxRingClipSpacing");
            NotifyPropertyChanged("MoonRingClipSpacing");
            NotifyPropertyChanged("JoggleBoltSpacing"); 
            NotifyPropertyChanged("JoggleAngle");
            NotifyPropertyChanged("SpliceAngle");
            NotifyPropertyChanged("BottomDCHeight");
            
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
        #endregion Validation of fields
    }
}
