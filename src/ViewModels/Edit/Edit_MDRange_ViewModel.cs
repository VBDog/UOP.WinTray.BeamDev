using MvvmDialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using UOP.WinTray.Projects;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Parts;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;
using UOP.WinTray.UI.Commands;
using UOP.WinTray.UI.Messages;

namespace UOP.WinTray.UI.ViewModels
{
    /// <summary>
    /// This is the View model for edit range properties view
    /// </summary>
    public class Edit_MDRange_ViewModel : MDProjectViewModelBase, IModalDialogViewModel
    {

        //private const double _MinimumBeamWidth = 6.0;
        //private const double _MaximumBeamWidth = 36.0;


        private const double _MinimumBeamOffset = 0.0;
        private const double _MaximumBeamOffsetCoefficient = 0.25;


        //private const double _MinimumBeamFlangeThickness = 0.375;
        //private const double _MaximumBeamFlangeThickness = 3.0;
        private const double _MaximumBeamFlangeThicknessCoefficient = 0.25;


        //private const double _MinimumBeamWebThickness = 0.375;
        //private const double _MaximumBeamWebThickness = 3.0;
        private const double _MaximumBeamWebThicknessCoefficient = 0.25;

        private bool widthIsRed = false;
        private bool flangeThicknessIsRed = false;
        private bool webThicknessIsRed = false;


        #region Constructors

        public Edit_MDRange_ViewModel(mdProject project, string fieldName = "", string rangeGUID = "", string caption = "", bool allowLappingRingNumbers = false, bool isNewTray = false, uppUnitFamilies? unitsToDisplay = null) : base()
        {

            IsOkBtnEnable = true;
            MDProject = project;
            AllowLappingRingNumbers = allowLappingRingNumbers;
            FocusTarget = fieldName;
            if (MDProject == null) return;
            if (string.IsNullOrWhiteSpace(rangeGUID)) rangeGUID = MDProject.SelectedRangeGUID;
            MDRange = (mdTrayRange)MDProject.TrayRanges.Find((x) => string.Compare(x.GUID, rangeGUID, true) == 0);
            Caption = string.IsNullOrWhiteSpace(caption) ? "Tray Section Data Input" : caption;
            DisplayUnits = unitsToDisplay.HasValue ? unitsToDisplay.Value : MDProject.DisplayUnits;
            IsNewTray = isNewTray;

            ErrorMessage = "";

            VisiblilityRevampType = (MDProject.InstallationType == uppInstallationTypes.Revamp) ? Visibility.Visible : Visibility.Collapsed;

            try
            {
                BusyMessage = "Loading Tray Details..";
                IsEnabled = false;
                DisplayUnits = MDProject.DisplayUnits;

                IsOkBtnEnable = true;
                VisibilityBeamHeight = Visibility.Visible;
                Enable_BeamHeight = false;
                Enable_BeamOffset = uopUtils.RunningInIDE;
                uopProperty prop = null;
                mdTrayRange range = MDRange;
                if (range != null)
                {
                    uopProperties eprops = range.CurrentProperties();
                    if (eprops.ValueD("RingThk") == 0)
                    {
                        eprops.SetValue("RingThk", MDProject.Column.RingThickness);
                    }

                    if (ProjectType == uppProjectTypes.MDSpout)
                    {

                        if (range.CurrentProperties().TryGet("OverrideTrayDiameter", out prop))
                        {
                            eprops.Remove(prop);
                        }
                    }

                    eprops.Append(MDAssy.Beam.Properties);
                    //AddBeamProperties(eprops);


                    EditProps = eprops;

                    if (EditProps.ValueD("WebOpeningSize") == 0 && eprops.ValueD("Height") != 0)
                    {
                        EditProps.SetValue("WebOpeningSize", eprops.ValueD("Height") * 0.5);
                    }

                    RevampStrategies = new ObservableCollection<string>(uopEnumHelper.GetDescriptions(typeof(uppRevampStrategies), SkipNegatives: true));
                    BeamOffsetFactors = new ObservableCollection<double>(mdBeam.OffsetStepValues(MDAssy.ShellRadius, MDAssy.Beam.Rotation, MDAssy.DowncomerSpacing));

                    RevampStrategy = range.RevampStrategy;
                    SelectedRangeTypeName = range.DesignFamily.Description();

                    GlobalProjectTitle = range.Name(true);

                }


            }
            finally
            {

                Validation_DefineLimits();
                IsEnabled = true;
            }

        }

        #endregion Constructors

        #region Commands

        private DelegateCommand _CMD_Cancel; //Cancel button delegate
        public ICommand Command_Cancel { get { if (_CMD_Cancel == null) _CMD_Cancel = new DelegateCommand(param => Execute_Cancel()); return _CMD_Cancel; } }


        private DelegateCommand _CMD_OK;     //Ok button delegate
        public ICommand Command_Ok { get { if (_CMD_OK == null) _CMD_OK = new DelegateCommand(param => Execute_Save()); return _CMD_OK; } }


        /// <summary>
        /// Close the Edit form with no changes
        /// </summary>
        private void Execute_Cancel()
        {
            Canceled = true;
            DialogResult = false;
        }



        #endregion Commands


        #region Properties

        private bool AllowLappingRingNumbers { get; set; }

        public bool IsNewTray { get; set; }

        public override bool IsEnglishSelected
        {
            get => base.IsEnglishSelected;
            set { base.IsEnglishSelected = value; NotifyPropertyChanges(); }
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


        private Visibility _VisiblilityRevampType;
        public Visibility VisiblilityRevampType
        {
            get => _VisiblilityRevampType;
            set { _VisiblilityRevampType = value; NotifyPropertyChanged("VisiblilityRevampType"); }
        }

        public string ShellID
        {
            get => EditProps.DisplayValueString("ShellID", true);
            set
            {
                double clrc = DefaultRingClearance;

                if (EditProps.SetDisplayUnitValue("ShellID", value))
                {
                    if (DefaultRingClearance != clrc) EditProps.SetValue("OverrideRingClearance", 0);
                    EditProps.SetValue("OverrideTrayDiameter", 0);

                }

                NotifyPropertyChanged("ShellID");
                NotifyPropertyChanges();
            }
        }

        public string ManholeID
        {
            get => EditProps.DisplayValueString("ManholeID", true, aZeroValue: DefaultManholeID);
            set { EditProps.SetDisplayUnitValue("ManholeID", value); NotifyPropertyChanges(); }
        }

        public int TrayCount => EditProps.ValueI("RingEnd") - EditProps.ValueI("RingStart") + 1;

        public string RingSpacing
        {
            get => EditProps.DisplayValueString("RingSpacing", true);
            set
            {
                double newspace = mzUtils.VarToDouble(value);
                EditProps.SetDisplayUnitValue("RingSpacing", newspace);
                NotifyPropertyChanged("RingSpacing");
                EditProps.SetValueD("Height", EditProps.ValueD("RingSpacing") + EditProps.ValueD("FlangeThickness"));
                NotifyPropertyChanged("Height");


            }

        }

        /// <summary>
        ///  the width of the ring in this tray range
        /// </summary>
        public string RingWidth
        {
            get => EditProps.DisplayValueString("RingWidth", true);

            set
            {
                EditProps.SetDisplayUnitValue("RingWidth", value); NotifyPropertyChanged("RingWidth"); NotifyPropertyChanged("RingID");


            }
        }

        public string OverrideTrayDiameter
        {
            get => EditProps.DisplayValueString("OverrideTrayDiameter", true, aZeroValue: DefaultTrayDiameter);


            set { EditProps.SetDisplayUnitValue("OverrideTrayDiameter", value); NotifyPropertyChanges(); }

        }

        public double RingID
        {
            get => EditProps.ValueD("ShellID") - (2 * Math.Abs(EditProps.ValueD("RingWidth")));

        }

        public string OverrideRingClearance
        {
            get => EditProps.DisplayValueString("OverrideRingClearance", false, aZeroValue: DefaultRingClearance);

            set
            {
                EditProps.SetDisplayUnitValue("OverrideRingClearance", value);
                NotifyPropertyChanges();
            }
        }

        public double DefaultRingClearance => uopUtils.BoundingClearance(EditProps.ValueD("ShellID"));

        public double DefaultTrayDiameter => uopUtils.DefaultTrayDiameter(EditProps.ValueD("ShellID"), RingID);

        public double DefaultManholeID => (MDProject != null) ? MDProject.Column.ManholeID : 0;

        public double DefaultRingThk => (MDProject != null) ? MDProject.Column.RingThickness : 0;

        private Brush _ForegroundColor_RingClearance = Brushes.Black;
        public Brush ForegroundColor_RingClearance
        {
            get
            {
                if (Activated)
                {
                    double dVal = DefaultRingClearance;
                    double aVal = (EditProps != null) ? EditProps.ValueD("OverrideRingClearance", 0) : 0;
                    _ForegroundColor_RingClearance = (aVal != dVal && aVal != 0) ? Brushes.Blue : Brushes.Black;
                }

                return _ForegroundColor_RingClearance;
            }
        }

        private Brush _ForegroundColor_RingThk = Brushes.Black;
        public Brush ForegroundColor_RingThk
        {
            get
            {
                if (Activated)
                {
                    double dVal = DefaultRingThk;
                    double aVal = (EditProps != null) ? EditProps.ValueD("RingThk", 0) : 0;
                    _ForegroundColor_RingThk = (aVal != dVal && aVal != 0) ? Brushes.Blue : Brushes.Black;

                }
                return _ForegroundColor_RingThk;
            }
        }

        private Brush _ForegroundColor_TrayOD = Brushes.Black;
        public Brush ForegroundColor_TrayOD
        {
            get
            {
                if (Activated)
                {
                    double dVal = DefaultTrayDiameter;
                    double aVal = (EditProps != null) ? EditProps.ValueD("OverrideTrayDiameter", 0) : 0;
                    _ForegroundColor_TrayOD = (aVal != dVal && aVal != 0) ? Brushes.Blue : Brushes.Black;

                }
                return _ForegroundColor_TrayOD;
            }
        }

        private Brush _ForegroundColor_ManholeID = Brushes.Black;
        public Brush ForegroundColor_ManholeID
        {
            get
            {
                if (Activated)
                {
                    double dVal = DefaultManholeID;
                    double aVal = (EditProps != null) ? EditProps.ValueD("ManholeID", 0) : 0;
                    _ForegroundColor_ManholeID = (aVal != dVal && aVal != 0) ? Brushes.Blue : Brushes.Black;
                }
                return _ForegroundColor_ManholeID;
            }
        }

        public string ToolTip_RingClearance => $"Default Clearance: {Units_Linear.UnitValueString(DefaultRingClearance, DisplayUnits)}";

        public string ToolTip_TrayOD => $"Default Tray OD: {Units_Linear.UnitValueString(DefaultTrayDiameter, DisplayUnits)}";

        public string ToolTip_ManholeID => $"Column Manhole ID: {Units_Linear.UnitValueString(DefaultManholeID, DisplayUnits)}";

        public string ToolTip_RingThk => $"Column Ring Thickness: {Units_Linear.UnitValueString(DefaultRingThk, DisplayUnits)}";
        public string ToolTip_BeamOffset => $"A multiplier used the set the beam offset as a multiple of the trays downcomer spaceing ";


        public override void Activate(Window myWindow)
        {
            if (Activated || MDAssy == null) return;
            base.Activate(myWindow);
            if (EditPropertyValueLimits == null) Validation_DefineLimits();
            if (!string.IsNullOrWhiteSpace(FocusTarget)) SetFocus(FocusTarget);
        }

        public string RingThk
        {
            get => EditProps.DisplayValueString("RingThk", true, aZeroValue: DefaultRingThk);
            set { EditProps.SetDisplayUnitValue("RingThk", value); NotifyPropertyChanges(); }

        }

        public string RingStart
        {
            get => EditProps.ValueI("RingStart").ToString();
            set { EditProps.SetValue("RingStart", mzUtils.VarToInteger(value)); NotifyPropertyChanged("RingStart"); }
        }

        public string RingEnd
        {
            get => EditProps.ValueI("RingEnd").ToString();
            set { EditProps.SetValue("RingEnd", mzUtils.VarToInteger(value)); NotifyPropertyChanged("RingEnd"); }
        }

        public uppMDDesigns DesignFamily
        {
            get => (uppMDDesigns)EditProps.ValueI("DesignFamily");
            set { EditProps.SetValue("DesignFamily", value); NotifyPropertyChanged("DesignFamily"); NotifyPropertyChanged("SelectedRangeTypeName"); }
        }

        private string _SelectedRangeTypeName = "";
        public string SelectedRangeTypeName
        {
            get => _SelectedRangeTypeName;
            set
            {
                _SelectedRangeTypeName = value;
                NotifyPropertyChanged("SelectedRangeTypeName");
                DesignFamily = (uppMDDesigns)uopEnumHelper.GetValueByDescription(typeof(uppMDDesigns), value);
                VisibilityBeamDesign = DesignFamily.IsBeamDesignFamily() ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// the avaialble revamp types
        /// </summary>
        private ObservableCollection<string> _RevampStrategies;
        
        public ObservableCollection<string> RevampStrategies
        {
            get => _RevampStrategies;
            set { _RevampStrategies = value; NotifyPropertyChanged("RevampStrategies"); }
        }

        /// <summary>
        /// selected value for RevampStrategies 
        /// </summary>
        public string RevampStrategy
        {
            get => EditProps.ValueS("RevampStrategy");
            set { EditProps.SetValue("RevampStrategy", value); NotifyPropertyChanged("RevampStrategy"); }
        }


        /// <summary>
        /// the avaialble beam offset factors
        /// </summary>
        private ObservableCollection<double> _BeamOffsetFactors;

        public ObservableCollection<double> BeamOffsetFactors
        {
            get => _BeamOffsetFactors;
            set { _BeamOffsetFactors = value; NotifyPropertyChanged("BeamOffsetFactors"); }
        }

        /// <summary>
        /// selected value for BeamOffsetFactor 
        /// </summary>
        public double BeamOffsetFactor
        {
            get => EditProps.ValueD("OffsetFactor");
            set { EditProps.SetValue("OffsetFactor", value); NotifyPropertyChanged("BeamOffsetFactor"); }
        }


        public bool IsTrayTypeEnabled { get { return IsNewTray; } }

        // Beam properties
        private Visibility _VisibilityBeamDesign;
        
        public Visibility VisibilityBeamDesign
        {
            get => _VisibilityBeamDesign;
            set { _VisibilityBeamDesign = value; NotifyPropertyChanged("VisibilityBeamDesign"); }
        }
        
        private Visibility _VisibilityBeamHeight;
        
        public Visibility VisibilityBeamHeight
        {
            get => _VisibilityBeamHeight;
            set { _VisibilityBeamHeight = value; NotifyPropertyChanged("VisibilityBeamHeight"); }
        }
        
        public new string Width
        {
            get => EditProps.DisplayValueString("Width", true);

            set
            {
                EditProps.SetDisplayUnitValue("Width", mzUtils.VarToDouble(value));
                NotifyPropertyChanged("Width");
            }
        }

        private bool _Enable_BeamHeight;
        public bool Enable_BeamHeight
        {
            get => _Enable_BeamHeight;

            set
            {
                _Enable_BeamHeight = value;
                NotifyPropertyChanged("Enable_BeamHeight");
            }
        }
        private bool _Enable_BeamOffset;
        public bool Enable_BeamOffset
        {
            get => _Enable_BeamOffset;

            set
            {
                _Enable_BeamOffset = value;
                NotifyPropertyChanged("Enable_BeamOffset");
            }
        }
        /*
         * For now, Height is simply the sum of flange thickness and tray spacing. So, we are not validating it and do not allow user to edit it. The reason we dedicate a property for it is that we want to show it in the UI and keep it in project file. There is a possibility that we will allow user to edit it in the future.
         */
        public new string Height
        {
            get => EditProps.DisplayValueString("Height", true);

            set
            {
                EditProps.SetDisplayUnitValue("Height", mzUtils.VarToDouble(value));
                NotifyPropertyChanged("Height");
            }
        }

        public string Offset
        {
            get => EditProps.DisplayValueString("Offset", true);

            set
            {
                EditProps.SetDisplayUnitValue("Offset", mzUtils.VarToDouble(value));
                NotifyPropertyChanged("Offset");
            }
        }

        public string FlangeThickness
        {
            get => EditProps.DisplayValueString("FlangeThickness", true);

            set
            {
                double flangeThickness = mzUtils.VarToDouble(value);
                EditProps.SetDisplayUnitValue("FlangeThickness", flangeThickness);
                NotifyPropertyChanged("FlangeThickness");
                EditProps.SetValueD("Height", EditProps.ValueD("RingSpacing") + EditProps.ValueD("FlangeThickness"));
                NotifyPropertyChanged("Height");
            }
        }

        public string WebThickness
        {
            get => EditProps.DisplayValueString("WebThickness", true);

            set
            {
                EditProps.SetDisplayUnitValue("WebThickness", mzUtils.VarToDouble(value));
                NotifyPropertyChanged("WebThickness");
            }
        }

        //public string WebOpeningSize
        //{
        //    get => EditProps.DisplayValueString("WebOpeningSize", true);

        //    set
        //    {
        //        EditProps.SetDisplayUnitValue("WebOpeningSize", mzUtils.VarToDouble(value));
        //        NotifyPropertyChanged("WebOpeningSize");
        //    }
        //}

        //public string WebOpeningCount
        //{
        //    get => EditProps.DisplayValueString("WebOpeningCount", true);

        //    set
        //    {
        //        EditProps.SetDisplayUnitValue("WebOpeningCount", mzUtils.VarToInteger(value));
        //        NotifyPropertyChanged("WebOpeningCount");
        //    }
        //}
        // Beam properties

        #endregion Properties



        #region Methods

        /// <summary>
        /// Execute_SaveComplete the edited properties back to the project object
        /// </summary>
        /// <returns></returns>
        private void Execute_Save()
        {
            if (!string.IsNullOrWhiteSpace(ErrorMessage))
            {
                return;
            }

            try
            {
                IsOkBtnEnable = false;
                IsEnabled = false;
                BusyMessage = "Calculation in progress..";
                IsEnabled = false;
                var validationResult = ValidateInput();
                ErrorMessage = validationResult;
                if (string.IsNullOrEmpty(ErrorMessage))
                {
                    Execute_SaveComplete();
                }
            }
            finally
            {
                IsOkBtnEnable = true;
                IsEnabled = true;
                BusyMessage = "";

            }
        }

        public override void SetFocus(string aControlName)
        {
            if (string.IsNullOrWhiteSpace(aControlName)) aControlName = FocusTarget;
            if (string.IsNullOrWhiteSpace(aControlName)) return;


            string pname = aControlName.Trim();

            if (string.Compare("RingID", pname, true) == 0) pname = "RingWidth";
            if (string.Compare("TrayOD", pname, true) == 0) pname = "OverrideTrayDiameter";

            uopProperty prop;
            if (!EditProps.TryGet(pname, out prop))
            {
                EditProps.TryGet("RingStart", out prop);
            }
            if (prop != null) base.SetFocus(prop.Name);


        }

        private void Execute_SaveComplete()
        {


            bool applyHoleIDToColumn = false;
            bool applyRingThicknessToColumn = false;
            mdTrayRange range = MDRange;
            Message_Refresh refresh = null;
            try
            {

                refresh = new Message_Refresh(bSuppressTree: true, aPartTypeList: new List<uppPartTypes>() { uppPartTypes.TrayRange, uppPartTypes.Project }, bCloseDocuments: true);


                int changecount = 0;
                double dval;
                double aval;
                bool zeroit = false;
                uopProperty prop;
                bool invalidateall = false;
                uopProperties changes = GetEditedProperties();
                bool namechange = changes.ContainsProperties("RingStart,RingEnd", ",").Count > 0;

                if (!DesignFamily.IsBeamDesignFamily())
                    changes.RemoveAll((x) => x.PartType == uppPartTypes.TraySupportBeam);
                else
                    changes.RemoveAll((x) => x.PartType == uppPartTypes.TraySupportBeam && x.IsNamed("Height"));

                if (string.IsNullOrEmpty(ErrorMessage))
                {
                    prop = EditProps.Item("OverrideRingClearance");
                    aval = prop.ValueD;
                    dval = DefaultRingClearance;
                    zeroit = aval == dval;
                    if (zeroit) prop.SetValue(0);

                    prop = EditProps.Item("OverrideTrayDiameter");
                    aval = prop.ValueD;
                    dval = DefaultTrayDiameter;
                    zeroit = aval == dval;
                    if (zeroit) prop.SetValue(0);

                    prop = EditProps.Item("ManholeID");
                    aval = prop.ValueD;
                    dval = DefaultManholeID;
                    zeroit = dval > 0 && aval == dval;
                    if (zeroit) prop.SetValue(0);
                    if (EditProps.TryGet("RingThk", out prop))
                    {
                        aval = prop.ValueD;
                        dval = DefaultRingThk;
                        zeroit = dval > 0 && aval == dval;
                        if (zeroit)
                            prop.SetValue(0);
                    }

                    changecount = changes.Count;
                    if (changes.TryGet("ManholeID", out prop))
                    {
                        dval = MDProject.Column.ManholeID;
                        aval = prop.ValueD;
                        if (MDProject.Column.ManholeID <= 0 && aval > 0)
                        {
                            MDProject.Column.PropValSet("ManholeID", aval, bSuppressEvnts: true);
                            MDProject.HasChanged = true;
                            dval = aval;
                            aval = 0;
                            prop.SetValue(0);
                        }

                        if (aval != 0)
                        {
                            applyHoleIDToColumn = (MDProject.TrayRanges.Count <= 1) || MessageBox.Show("Apply Override Manhole ID to All Tray Ranges?", "Manhole ID Change", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes;
                        }
                        else { if (MDRange.ManholeID == prop.ValueD) { changes.Remove(prop); changecount--; } }
                    }
                    if (changes.TryGet("RingThk", out prop))
                    {
                        dval = MDProject.Column.RingThickness;
                        aval = prop.ValueD;

                        if (dval <= 0 && aval > 0)
                        {
                            MDProject.Column.PropValSet("RingThickness", aval, bSuppressEvnts: true);
                            MDProject.HasChanged = true;
                            dval = aval;
                            aval = 0;
                            prop.SetValue(0);
                        }
                        if (aval != 0)
                        {

                            applyRingThicknessToColumn = (MDProject.TrayRanges.Count <= 1) || MessageBox.Show("Apply Override Ring Thinkness to All Tray Ranges?", "Ring Thickness Change", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes;
                        }
                        else { if (MDRange.RingThickness == prop.ValueD) { changes.Remove(prop); changecount--; } }
                    }

                    //{ uppPartTypes.TrayRange, uppPartTypes.Project }

                    if (changes.TryGet("RINGWIDTH", out prop))
                    {
                        if (!changes.Contains("RINGID"))
                        {

                            double rwd = prop.ValueD;
                            prop = EditProps.Item("RINGID");
                            double ringid = EditProps.ValueD("SHELLID") - rwd * 2;
                            prop.SetValue(ringid);
                            changes.Add(prop);
                            //     changes.Remove("RINGWIDTH");

                        }

                    }


                    if (changecount > 0)
                    {
                        Canceled = false;


                        foreach (uopProperty change in changes)
                        {
                            uopProperty propchange;
                            if (change.PartType == uppPartTypes.TraySupportBeam)
                            {
                                propchange = MDAssy.Beam.PropValSet(change.Name, change.Value, bSuppressEvnts: true);
                                if (propchange != null)
                                {
                                    invalidateall = true;
                                    MDAssy.Alert(propchange);
                                }
                            }
                            else
                            {
                                string pname = change.Name.ToUpper();
                                switch (pname)
                                {
                                    case "RINSTART":
                                    case "RINGEND":
                                        refresh.SuppressTree = false;
                                        refresh.UpdateRangeList = true;
                                        refresh.RangeNameChanged = true;
                                        invalidateall = true;
                                        break;
                                    case "RINGWIDTH":
                                        invalidateall = true;
                                        break;

                                    case "SHELLID":
                                        invalidateall = true;
                                        break;
                                    case "OVERRIDERINGCLEARANCE":
                                    case "OVERRIDETRAYDIAMETER":
                                        refresh.SuppressImage = false;
                                        invalidateall = true;

                                        MDAssy.ResetComponents(true);
                                        break;

                                    default:
                                        break;

                                }
                                propchange = MDRange.PropValSet(change.Name, change.Value, bSuppressEvnts: true);
                                if (propchange != null)
                                {
                                    MDAssy.Alert(propchange);

                                }
                            }

                            if (propchange != null) refresh.Changes.Add(propchange);

                        }


                    }
                    if (invalidateall)
                    {
                        refresh.PartTypeList.Clear();
                        refresh.SuppressImage = false;
                        MDAssy.ResetComponents();
                    }
                }
                else
                {
                    Canceled = true;
                }

                if (!Canceled)
                {
                    if (applyRingThicknessToColumn)
                    {
                        MDProject.Column.SetRingThickness(EditProps.ValueD("RingThk"), bApplyToAllRanges: true, GUIDSToSkip: new List<string>() { range.GUID }, bSuppressEvents: false);
                    }
                    ;
                    if (applyHoleIDToColumn)
                    {
                        MDProject.Column.SetManholeID(EditProps.ValueD("ManholeID"), bApplyToAllRanges: true, GUIDSToSkip: new List<string>() { range.GUID }, bSuppressEvents: false);
                    }
                    ;


                }


                if (namechange)
                {
                    refresh.UpdateRangeList = true;
                    refresh.RangeNameChanged = true;
                }
            }
            catch { }
            finally
            {

                if (IsNewTray)
                {
                    MDAssy.DesignOptions.PropValSet("HasTiledDecks", MDAssy.FunctionalPanelWidth >= MDRange.ManholeID - 0.5);
                }
                if (!Canceled)
                {
                    RefreshMessage = refresh;
                    MDProject.HasChanged = true;
                }
                else
                {
                    RefreshMessage = null;
                }

                ReleaseReferences();
                DialogResult = !Canceled;
            }
        }

        public new void ReleaseReferences()
        {
            base.ReleaseReferences();


        }


        private void Cancel() => DialogResult = false;


        /// <summary>
        /// On toggle units.
        /// </summary>
        public override void ToggleUnits()
        {
            base.ToggleUnits();
            NotifyPropertyChanges();
        }



        /// <summary>
        /// Invokes on notify property changed for range properties.
        /// </summary>
        private void NotifyPropertyChanges()
        {
            //uopProperties props = EditProps;
            NotifyPropertyChanged("RingStart");
            NotifyPropertyChanged("RingEnd");

            NotifyPropertyChanged("SelectedRangeTypeName");
            NotifyPropertyChanged("RevampStrategy");
            NotifyPropertyChanged("ShellID");
            NotifyPropertyChanged("RingWidth");
            NotifyPropertyChanged("RingThk");
            NotifyPropertyChanged("RingSpacing");
            NotifyPropertyChanged("Height");
            NotifyPropertyChanged("Width");
            NotifyPropertyChanged("WebThickness");
            NotifyPropertyChanged("FlangeThickness");
            NotifyPropertyChanged("Offset");
            NotifyPropertyChanged("OverrideTrayDiameter");
            NotifyPropertyChanged("ManholeID");
            NotifyPropertyChanged("ToolTip_TrayOD");
            NotifyPropertyChanged("ToolTip_ManholeID");
            NotifyPropertyChanged("VisibilityMDDraw");
            NotifyPropertyChanged("VisibilityMDSpout");
            NotifyPropertyChanged("ToolTip_RingClearance");
            NotifyPropertyChanged("OverrideRingClearance");
            NotifyPropertyChanged("OverrideTrayDiameter");
            NotifyPropertyChanged("ForegroundColor_RingClearance");
            NotifyPropertyChanged("ForegroundColor_TrayOD");
            NotifyPropertyChanged("ForegroundColor_ManholeID");
            NotifyPropertyChanged("ToolTip_RingThk");
            NotifyPropertyChanged("ForegroundColor_RingThk");
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
        #endregion Methods




        #region Validation of fields 

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

        private void Validation_DefineLimits()
        {


            List<ValueLimit> Limits = new() { };
            Limits.Add(new ValueLimit("RingStart", 0, 999));
            Limits.Add(new ValueLimit("RingEnd", 0, 999));
            Limits.Add(new ValueLimit("ShellID", 32.0d, 600.0d));
            Limits.Add(new ValueLimit("RingWidth", 0.5d, 12.0d));
            Limits.Add(new ValueLimit("RingThk", 0.25d, 2.0d, bAllowZero: true)); ;
            Limits.Add(new ValueLimit("RingSpacing", 6.0d, 150.0d));
            Limits.Add(new ValueLimit("OverrideTrayDiameter", null, null, bAllowZero: true));
            Limits.Add(new ValueLimit("ManholeID", 12.0d, 52.0d, bAllowZero: true));
            Limits.Add(new ValueLimit("Width", 6.0d, 36.0d, bAllowZero: false));
            Limits.Add(new ValueLimit("FlangeThickness", 0.375, 3.0, bAllowZero: false));
            Limits.Add(new ValueLimit("WebThickness", 0.375, 3.0, bAllowZero: false));

            EditPropertyValueLimits = Limits;
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
                result = limits.ValidateProperty(prop, (prop.Units.UnitType == uppUnitTypes.SmallLength) ? Units_Linear : Units_Area, DisplayUnits);
            }

            int idx;
            //check inter-dependant values
            if (result == null)
            {
                double dlimit;
                ValueLimit test;
                double dval;
                double defval;
                string suffix;
                switch (aPropertyName.ToUpper())
                {

                    case "RINGSTART":
                    case "RINGEND":
                        int i1 = EditProps.ValueI("RingStart");
                        int i2 = EditProps.ValueI("RingEnd");

                        idx = ErrorCollection.FindIndex(x => string.Compare(x.Item1, "RingStart", true) == 0);
                        if (idx >= 0) ErrorCollection.RemoveAt(idx);
                        idx = ErrorCollection.FindIndex(x => string.Compare(x.Item1, "RingEnd", true) == 0);
                        if (idx >= 0) ErrorCollection.RemoveAt(idx);

                        if (i1 > i2)
                        {
                            result = "Invalid Tray Number Range";

                            break;
                        }
                        if (!AllowLappingRingNumbers)
                        {
                            if (MDProject.TrayRanges.RangeIsOccupied(i1, i2, uppStackPatterns.Continuous, (uopTrayRange)MDRange) != null)
                            {
                                result = "Invalid Tray Number Range - One or More of the Entered Ring Numbers Are Already Occupied By Another Tray Section";

                                break;
                            }
                        }

                        break;
                    case "OVERRIDETRAYDIAMETER":
                        dval = prop.ValueD;
                        defval = DefaultTrayDiameter;
                        if (dval == defval) dval = 0;
                        if (dval > 0)
                        {

                            dlimit = 0.5 * EditProps.ValueD("RingWidth"); ;
                            suffix = $" ({Units_Linear.UnitValueString(defval, DisplayUnits)} +/- {Units_Linear.UnitValueString(dlimit, DisplayUnits)})";
                            test = new ValueLimit(prop.Name, defval - dlimit, defval + dlimit, suffix, suffix, bAllowZero: true);
                            result = test.ValidateProperty(prop, Units_Linear, DisplayUnits);


                        }

                        break;

                    case "OVERRIDERINGCLEARANCE":



                        dval = prop.ValueD;
                        defval = DefaultRingClearance;

                        if (dval != defval)
                        {

                            dlimit = 1.5;
                            test = new ValueLimit(prop.Name, defval, defval + dlimit, bAllowZero: true);
                            result = test.ValidateProperty(prop, Units_Linear, DisplayUnits);
                            break;
                        }
                        break;

                    case "WIDTH":
                        if (DesignFamily.IsBeamDesignFamily())
                        {
                            dval = prop.ValueD;
                            //test = new ValueLimit("Width", _MinimumBeamWidth, _MaximumBeamWidth);
                            //result = test.ValidateProperty(prop, Units_Linear, DisplayUnits);
                            //if (string.IsNullOrWhiteSpace(result))
                            //{
                            uopProperty flangeThicknessProp = EditProps.Item("FlangeThickness", true);
                            uopProperty webThicknessProp = EditProps.Item("WebThickness", true);
                            if (dval * _MaximumBeamFlangeThicknessCoefficient <= flangeThicknessProp.ValueD)
                            {
                                result = $"Flange Thickness must be less than {_MaximumBeamFlangeThicknessCoefficient} of the Beam Width.";
                                widthIsRed = true;
                            }
                            else
                            {
                                if (dval * _MaximumBeamWebThicknessCoefficient <= webThicknessProp.ValueD)
                                {
                                    result = $"Web Thickness must be less than {_MaximumBeamWebThicknessCoefficient} of the Beam Width.";
                                    widthIsRed = true;
                                }
                                else
                                {
                                    widthIsRed = false;
                                    if (flangeThicknessIsRed)
                                    {
                                        // This is to remove the error indicator on flange thickness input, if any.
                                        FlangeThickness = FlangeThickness;
                                    }
                                    if (webThicknessIsRed)
                                    {
                                        // This is to remove the error indicator on web thickness input, if any.
                                        WebThickness = WebThickness;
                                    }
                                }
                                //}
                            }
                        }
                        break;
                    case "HEIGHT":
                        //if (DesignFamily.IsBeamDesignFamily())
                        //{
                        //    // For now, we are not validating the height of the beam because it is simply the sum of flange thickness and tray spacing.
                        //}
                        break;
                    case "OFFSET":
                        if (DesignFamily.IsBeamDesignFamily())
                        {
                            dval = prop.ValueD;
                            test = new ValueLimit("Offset", _MinimumBeamOffset, _MaximumBeamOffsetCoefficient * EditProps.ValueD("ShellID"), bAllowZero: true);
                            result = test.ValidateProperty(prop, Units_Linear, DisplayUnits);
                        }
                        break;
                    case "FLANGETHICKNESS":
                        if (DesignFamily.IsBeamDesignFamily())
                        {
                            if (ProjectType == uppProjectTypes.MDDraw || prop.ValueD != 0) // Setting this is required only for MDDraw projects
                            {
                                dval = prop.ValueD;
                                //test = new ValueLimit("FlangeThickness", _MinimumBeamFlangeThickness, _MaximumBeamFlangeThickness);
                                //result = test.ValidateProperty(prop, Units_Linear, DisplayUnits);
                                //if (string.IsNullOrWhiteSpace(result))
                                //{
                                uopProperty beamWidthProp = EditProps.Item("Width", true);
                                if (dval >= _MaximumBeamFlangeThicknessCoefficient * beamWidthProp.ValueD)
                                {

                                    result = $"Flange Thickness must be less than {(_MaximumBeamFlangeThicknessCoefficient * 100):0.00} % of the Beam Width.";
                                    flangeThicknessIsRed = true;
                                }
                                else
                                {
                                    flangeThicknessIsRed = false;
                                    if (widthIsRed && !webThicknessIsRed)
                                    {
                                        // This is to remove the error indicator on beam width input, if any.
                                        Width = Width;
                                    }
                                }
                                //}
                            }
                        }
                        break;
                    case "WEBTHICKNESS":
                        if (DesignFamily.IsBeamDesignFamily()) // Setting this is required only for MDDraw projects
                        {
                            if (ProjectType == uppProjectTypes.MDDraw || prop.ValueD != 0)
                            {
                                dval = prop.ValueD;
                                //test = new ValueLimit("WebThickness", _MinimumBeamWebThickness, _MaximumBeamWebThickness);
                                //result = test.ValidateProperty(prop, Units_Linear, DisplayUnits);
                                //if (string.IsNullOrWhiteSpace(result))
                                //{
                                uopProperty beamWidthProp = EditProps.Item("Width", true);
                                if (dval >= _MaximumBeamWebThicknessCoefficient * beamWidthProp.ValueD)
                                {
                                    result = $"Web Thickness must be less than {(_MaximumBeamWebThicknessCoefficient * 100):0.00} % of the Beam Width.";
                                    webThicknessIsRed = true;
                                }
                                else
                                {
                                    webThicknessIsRed = false;
                                    if (widthIsRed && !flangeThicknessIsRed)
                                    {
                                        // This is to remove the error indicator on beam width input, if any.
                                        Width = Width;
                                    }
                                }
                                // }
                            }
                        }
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


        #endregion Validation of fields
    }
}
