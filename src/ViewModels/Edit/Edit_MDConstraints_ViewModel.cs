using Microsoft.Office.Core;
using MvvmDialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using UOP.WinTray.Projects;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Parts;

using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.UI.BusinessLogic;
using UOP.WinTray.UI.Commands;
using UOP.WinTray.UI.Messages;

namespace UOP.WinTray.UI.ViewModels
{
    /// <summary>
    /// ViewModel for TrayRange properties
    /// </summary>
    public class Edit_MDConstraints_ViewModel : MDProjectViewModelBase, IModalDialogViewModel
    {
       
        private const string REG_NUMERIC_ONLY = @"\d+";


        #region Variables

        private List<mdConstraint> _ApplyConstrs;
       
        #endregion

        #region Constructors

        public Edit_MDConstraints_ViewModel(mdProject project) : base()
        {

            Project = project;
            SpoutPatterns = new ObservableCollection<string>();
            if (MDProject == null) return;
            if (MDAssy == null) return;
            IsEnglishSelected = MDProject.DisplayUnits == uppUnitFamilies.English;

            EditProps = new uopProperties()
            {
                { "Clearance", 0, uppUnitTypes.SmallLength },
                { "Margin", 0, uppUnitTypes.SmallLength },
                { "SpoutLength", 0, uppUnitTypes.SmallLength },
                { "VerticalPitch", 0, uppUnitTypes.SmallLength },
                { "PatternType", uppSpoutPatterns.Undefined }
            };
            

            Validation_DefineLimits();
            this.IsOkBtnEnable = true;

            
            List<string> names = mdUtils.SpoutPatternNames();
            //SpoutPatterns.Add("Varies");
            for (int i = 0; i < names.Count; i++)
            {
                if (names[i] != "*S*") SpoutPatterns.Add(names[i]);
     
            }
                

         
            ApplyTo = GetApplyToOptions();

            if (ApplyTo.Count > 0)
            {
                SelectedApplyTo = ApplyTo.Contains(appApplication.LastGlobalSelect) ? appApplication.LastGlobalSelect : ApplyTo[0];
                NotifyPropertyChanged("SelectedApplyTo");
            }

          
            ShowCommomProperties();
        }

      

        /// <summary>
        /// Show options
        /// </summary>
        public ObservableCollection<string> GetApplyToOptions()
        {

            //       private const string ALL_SPOUT_GROUPS = "All Spout Groups";
            //private const string GROUPED_ONLY = "Grouped Only";
            //private const string END_GROUPS = "End Groups";
            //private const string SPOUT_GROUPS = " Spout Groups";
            //private const string FIELD_GROUPS = "Field Groups";

            if (Constraints == null && MDAssy != null) Constraints = MDAssy.Constraints.Clone();
            colMDDowncomers downcomers = MDAssy.Downcomers;
            colMDDeckPanels deckPanels = MDAssy.DeckPanels;
            ObservableCollection<string> ListApplyTo = new();
            ListApplyTo.Add("All Spout Groups");
            if (Constraints.HasGroupFlags)
            {
                ListApplyTo.Add("Grouped Only");
            }
            ListApplyTo.Add("End Groups");
            ListApplyTo.Add("Field Groups");
            for (int i = 1; i <= downcomers.Count; i++)
            {
                ListApplyTo.Add($"Downcomer { i} Spout Groups");
            }
            for (int i = 1; i <= deckPanels.Count; i++)
            {
                ListApplyTo.Add($"Deck Panel { i} Spout Groups");

            }
            return ListApplyTo;
        }

        #endregion Constructors

        #region Properties
        private uopProperties CommonProps { get; set; }
        private colMDConstraints Constraints { get; set; }
        public bool ClearanceFocused { get; set; }
        public bool MarginFocused { get; set; }
        public bool VPitchFocused { get; set; }
        public bool LengthFocused { get; set; }

        public virtual ObservableCollection<string> ApplyTo { get; set; }

        public ObservableCollection<string> SpoutPatterns { get; set; }

        public override bool IsEnglishSelected
        {
            get => base.IsEnglishSelected;
            set { base.IsEnglishSelected = value; NotifyPropertyChanges(); }
        }

        private string _SelectedSpoutPattern;
        public string SelectedSpoutPattern
        {
            get { if (string.IsNullOrWhiteSpace(_SelectedSpoutPattern)) _SelectedSpoutPattern = ""; return _SelectedSpoutPattern; }
            set
            {
                if (_SelectedSpoutPattern != value)
                {
                    _SelectedSpoutPattern = value;
                    SelectedSpoutPatternChanged();
                    NotifyPropertyChanged("SelectedSpoutPattern");
                }
            }
        }

        public uppSpoutPatterns SelectedPattern { 
            get  => (uppSpoutPatterns)EditProps.ValueI("PatternType");
            set { EditProps.SetValue("PatternType", value); NotifyPropertyChanged("SelectedPattern"); }
        }

        private string _SelectedApplyTo;

        public string SelectedApplyTo
        {
            get => _SelectedApplyTo;
            set
            {
                if (value != _SelectedApplyTo)
                {

                    _SelectedApplyTo = value;
                    Execute_Clear();
                    ShowCommomProperties();
                    
                }
                NotifyPropertyChanged("SelectedApplyTo");
            }
        }
        public new mdTrayRange SelectedTrayRange => MDProject.SelectedRange;
            

        private DelegateCommand _CMD_Cancel;
        public ICommand Command_Cancel
        {
            get
            {
                if (_CMD_Cancel == null)
                {
                    _CMD_Cancel = new DelegateCommand(param => Execute_Cancel());
                }
                return _CMD_Cancel;
            }
        }

        private DelegateCommand _CMD_OK;
        public ICommand Command_Ok { get { if (_CMD_OK == null) _CMD_OK = new DelegateCommand(param => Execute_Save()); return _CMD_OK; } }




        private DelegateCommand _CMD_Clear;

        public ICommand Command_Clear
        {
            get
            {
                if (_CMD_Clear == null) _CMD_Clear = new DelegateCommand(param => Execute_Clear());
       
                return _CMD_Clear;
            }
        }

          private bool? _DialogueResult;
        /// <summary>
        /// Dialogservice result
        /// </summary>
        public bool? DialogResult
        {
            get => _DialogueResult;
            
            private set { _DialogueResult = value; NotifyPropertyChanged("DialogResult"); }
        }




        public string Clearance
        {
            get => EditProps.DisplayValueString("Clearance", true);
            set { EditProps.SetDisplayUnitValue("Clearance", value); NotifyPropertyChanged("Clearance"); }
        }


        public string SpoutMargin
        {
            get => EditProps.DisplayValueString("Margin", true);
            set { EditProps.SetDisplayUnitValue("Margin", value); NotifyPropertyChanged("SpoutMargin"); }
        }

        public string VerticalPitch
        {
            get => EditProps.DisplayValueString("VerticalPitch", true);
            set { EditProps.SetDisplayUnitValue("VerticalPitch", value); NotifyPropertyChanged("VerticalPitch"); }
        }

        public string SpoutLength
        {
            get => EditProps.DisplayValueString("SpoutLength", true);
            set { EditProps.SetDisplayUnitValue("SpoutLength", value); NotifyPropertyChanged("SpoutLength"); }
        }





        public bool IsSPattern
        {
            get
            {
                uppSpoutPatterns pat = SelectedPattern;
                return pat == uppSpoutPatterns.S1 || pat == uppSpoutPatterns.S2 || pat == uppSpoutPatterns.S3;
            }
        }

        private bool _IsSpoutLength;
        public virtual bool IsSpoutLength
        {
            get => _IsSpoutLength;
            set { _IsSpoutLength = value; NotifyPropertyChanged("IsSpoutLength"); Visibility_SpoutLength = _IsSpoutLength ? Visibility.Visible : Visibility.Collapsed; }
        }

        private Visibility _Visibility_SpoutLength = Visibility.Visible;
        public Visibility Visibility_SpoutLength
        {
            get => _Visibility_SpoutLength;
            set { _Visibility_SpoutLength = value; NotifyPropertyChanged("Visibility_SpoutLength"); }
        }

        #endregion

        #region Methods

        public uppSpoutPatterns GetSelectedPattern()
        {
            switch (SelectedSpoutPattern.ToUpper())
            {
                case "A":
                    return uppSpoutPatterns.A;
                case "*A*":
                    return uppSpoutPatterns.Astar;
                case "B":
                    return uppSpoutPatterns.B;
                case "C":
                    return uppSpoutPatterns.C;
                case "D":
                    return uppSpoutPatterns.D;
                case "S1":
                    return uppSpoutPatterns.S1;
                case "S2":
                    return uppSpoutPatterns.S2;
                case "S3":
                    return uppSpoutPatterns.S3;
                default:
                    return uppSpoutPatterns.Undefined;
            }
            
        }
        /// <summary>
        /// Cancel constrains
        /// </summary>
        private void Execute_Cancel() => DialogResult = false;
            

        /// <summary>
        /// Clear constrains
        /// </summary>
        private void Execute_Clear()
        {
            SelectedSpoutPattern = string.Empty;
            EditProps.SetValue("Margin", 0);
            EditProps.SetValue("Clearance", 0);
            EditProps.SetValue("VerticalPitch", 0);
            EditProps.SetValue("SpoutLength", 0);

          
            NotifyPropertyChanges();



        }

        private void NotifyPropertyChanges()
        {

            NotifyPropertyChanged("SelectedSpoutPattern");
            NotifyPropertyChanged("Visibility_SpoutLength");
            NotifyPropertyChanged("SpoutMargin");
            NotifyPropertyChanged("Clearance");
            NotifyPropertyChanged("SpoutLength");
            NotifyPropertyChanged("VerticalPitch");
            NotifyPropertyChanged("SelectedPattern");
        }
        /// <summary>
        /// Save constrains
        /// </summary>
        private void Execute_Save()
        {
            bool changes = false;
            bool executed = false;
            try
            {
                BusyMessage = "Calculation in progress..";
                IsOkBtnEnable = false;
                IsEnabled = false;
                var validationResult = ValidateInput();
                ErrorMessage = validationResult;
                if (string.IsNullOrEmpty(ErrorMessage))
                {
                    executed = true;
                    changes = SaveEditConstraints();
                }
            }
            finally
            {
                MDAssy.SpoutGroups.UpdateConstraints(MDAssy);
                if (changes)
                {
                    MDAssy.Invalidate(uppPartTypes.SpoutGroups);
                    MDAssy.RegenerateStartupSpouts();
                    RefreshMessage = new Message_Refresh(bSuppressPropertyLists: false, bSuppressDataGrids: false, bSuppressTree: false, bSuppressImage: false);
                }

                BusyMessage = "";
                IsEnabled = true;
                IsOkBtnEnable = true;

                if (executed) DialogResult = changes;
            }

        }

        /// <summary>
        /// Save the Constrains
        /// </summary>
        private bool SaveEditConstraints()
        {
            bool bChanged = false;
            
            appApplication.LastGlobalSelect = SelectedApplyTo;
            uopProperties aProps = GetApplicableProperties();
            List<mdConstraint> colApplyTo = GetApplicableConstraints();
            List<mdSpoutGroup> sgs = MDAssy.SpoutGroups.ToList();
            MDAssy.SpoutGroups.Invalid = true;
            colMDSpoutGroups aSGs = MDAssy.SpoutGroups;

            if (colApplyTo == null || aProps == null || aProps.Count <= 0) return false;
            foreach(var sg in sgs)
            {
                sg.SuppressEvents = false;
                mdConstraint cnstr = colApplyTo.Find((x) => x == sg.Constraints(MDAssy));
                if (cnstr == null) continue;
                foreach(var prop in aProps)
                {
                    if (prop.Name.Contains("Pitch") && cnstr.PatternType == uppSpoutPatterns.Undefined)
                    {
                        if (cnstr.SetProperty("PatternType",sg.PatternType))
                        {
                            bChanged = true;
                            sg.Invalid = true;
                        }

                    }

                    if (cnstr.SetProperty(prop.Name, prop.Value))
                    {
                        bChanged = true;
                        sg.Invalid = true;

                    }

                    if (sg.Invalid)
                    {
                        sg.SetConstraints(cnstr);
                        sg.UpdateConstraints(MDAssy);
                        sg.UpdateSpouts(MDAssy);
                         
                    }

                    sg.UpdateConstraints(MDAssy);
                }
            }    
            

            if (bChanged) 
                MDAssy.Invalidate(uppPartTypes.SpoutGroups);


            return bChanged;
         
           


        }

      

        /// <summary>
        /// get the sub set of assy constraints to apply the changes to
        /// </summary>
        /// <returns></returns>
        private List<mdConstraint> GetApplicableConstraints()
        {
            int index = 0;
            List<mdConstraint> _rVal = new();
            if (MDAssy == null) return _rVal;
            uppConstraintApplications applyTo = GetApplyTo(ref index);
            switch (applyTo)
            {
                case uppConstraintApplications.uopApplyToAll:
                    _rVal =  MDAssy.Constraints.ToList();
                    break;
                case uppConstraintApplications.uopApplyToPanelGroups:
                    _rVal = MDAssy.Constraints.GetByPanelIndex(index);
                    break;
                case uppConstraintApplications.uopApplyToDowncomerGroups:
                    _rVal = MDAssy.Constraints.GetByDowncomerIndex(index);
                    break;
                case uppConstraintApplications.uopApplyToGrouped:
                    _rVal = MDAssy.Constraints.GetGroupedConstraints(SelectedTrayRange.TrayAssembly);
                    break;
                case uppConstraintApplications.uopApplyToEndGroups:
                    _rVal = MDAssy.Constraints.GetEndGroupConstraints(SelectedTrayRange.TrayAssembly);
                    break;
                case uppConstraintApplications.uopApplyToFieldGroups:
                    _rVal = MDAssy.Constraints.GetFieldGroupConstraints(SelectedTrayRange.TrayAssembly);
                    break;
            }

            return _rVal; 
        }

        /// <summary>
        /// Get the Apply to list
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public virtual uppConstraintApplications GetApplyTo(ref int aIndex)
        {
            uppConstraintApplications applyTo = uppConstraintApplications.uopApplyToNone;
            if (SpoutPatterns.Count == 0 || SelectedApplyTo == "All Spout Groups")
            {
                applyTo = uppConstraintApplications.uopApplyToAll;
            }
            else
            {
                string apply = SelectedApplyTo.ToUpper();
                string[] splittedVals = apply.Split(' ');

                if (apply.StartsWith("DECK"))
                {
                    if (splittedVals.Length > 2)
                    {
                        if (!int.TryParse(splittedVals[2], out aIndex))
                        {
                            aIndex =0;
                        }
                    }
                    applyTo = uppConstraintApplications.uopApplyToPanelGroups;
                }
                else if (apply.StartsWith("DOWNCOMER"))
                {
                    if (splittedVals.Length > 1)
                    {
                        if (!int.TryParse(splittedVals[1], out aIndex))
                        {
                            aIndex =0;
                        }
                    }
                    applyTo = uppConstraintApplications.uopApplyToDowncomerGroups;
                }
                else if (apply.StartsWith("GROUPED"))
                {
                    applyTo = uppConstraintApplications.uopApplyToGrouped;
                }
                else if (apply.StartsWith("END GROUP"))
                {
                    applyTo = uppConstraintApplications.uopApplyToEndGroups;
                }
                else if (apply.StartsWith("FIELD GROUP"))
                {
                    applyTo = uppConstraintApplications.uopApplyToFieldGroups;
                }
            }

            return applyTo;
        }

        /// <summary>
        /// update the SpoutLength on Selected Spout Pattern changes
        /// </summary>
        private void SelectedSpoutPatternChanged()
        {
            
            
            if (Constraints == null && MDAssy != null) Constraints = MDAssy.Constraints.Clone();
            if (Constraints == null) return;
             SelectedPattern = GetSelectedPattern();
            IsSpoutLength = IsSPattern;

            if (!string.IsNullOrEmpty(SelectedSpoutPattern))
            {
                

                GetAllIndexsFrom(SelectedApplyTo, out int? downcomerIndex, out int? panelIndex, out int? excludePanelIndex);
                //if (IsSPattern || Constraints.IsPatternOfTypeSExist(downcomerIndex, panelIndex, excludePanelIndex))
                //{
                //    IsSpoutLength = true;
                //}
                if(SelectedSpoutPattern.ToUpper() == "BY DEFAULT")
                {
                    
                    EditProps.SetValue("VerticalPitch",0);
                    EditProps.SetValue("SpoutLength", 0);
                   
                    NotifyPropertyChanged("VerticalPitch");
                    NotifyPropertyChanged("SpoutLength");
                }
                else
                {
           
                    EditProps.SetValue("SpoutLength", 0);
                    NotifyPropertyChanged("SpoutLength");
                }

                
            }
            NotifyPropertyChanges();
        }


        /// <summary>
        /// ^returns the Values from the form that have numeric input in them
        /// </summary>
        /// <returns></returns>
        private uopProperties GetApplicableProperties()
        {
            try
            {
                uopProperties _rVal = new();
                SelectedPattern = GetSelectedPattern();
         
                if (EditProps.ValueD("Margin") > 0) _rVal.Add(EditProps.Item("Margin"), bAddClone: true);
                if (EditProps.ValueD("VerticalPitch") > 0) _rVal.Add(EditProps.Item("VerticalPitch"), bAddClone: true);

                if (EditProps.ValueD("Clearance") > 0) _rVal.Add(EditProps.Item("Clearance"), bAddClone: true);
                if (IsSPattern && EditProps.ValueD("SpoutLength") > 0) _rVal.Add(EditProps.Item("SpoutLength"), bAddClone: true);

                if (SelectedPattern != uppSpoutPatterns.Undefined)
                {
                    _rVal.Add("PatternType", SelectedPattern);

                }
              


                foreach (uopProperty Prop in CommonProps)
                {
                    if (_rVal.TryGet(Prop.Name, out uopProperty upProp))
                    {
                        if (Prop.Name == "PatternType")
                        {
                            if (mzUtils.GetEnumValue<uppSpoutPatterns>(Prop.Value) == mzUtils.GetEnumValue<uppSpoutPatterns>(upProp.Value))
                            {
                                _rVal.Remove(Prop.Name);
                            }
                        }
                        else if (null != upProp.Value && null != Prop.Value && upProp.Value.ToString() == Prop.Value.ToString())
                        {
                            _rVal.Remove(Prop.Name);
                        }
                    }
                }  

                return _rVal;
            }
            catch (Exception ex)
            {
                Utilities.Logger.HandleException(ex, "GetApplicableProperties", "Edit_MDConstraints_ViewModel");
                return null;
            }
        }
        /// <summary>
        /// Show common properties
        /// </summary>
        public void ShowCommomProperties()
        {

            if (!Activated) return;
            Execute_Clear();
        
            uopProperty uopProp = null;

          
            GetAllIndexsFrom(SelectedApplyTo, out int? downcomerIndex, out int? panelIndex, out int? excludePanelIndex);
            Constraints = MDAssy.Constraints.Clone();

             _ApplyConstrs = GetApplicableConstraints();
            CommonProps = Constraints.GetCommonProperties(downcomerIndex, panelIndex, excludePanelIndex);
            for (int i = 1; i <= CommonProps.Count; i++)
            {
                uopProp = CommonProps.Item(i);
                
                switch (uopProp.Name)
                {
                    case "SpoutLength":
                    case "Margin":
                    case "VerticalPitch":
                    case "Clearance":
                        EditProps.SetValue(uopProp.Name, uopProp.ValueD);
                        break;


                    case "PatternType":

                        EditProps.SetValue(uopProp.Name, uopProp.ValueI);
                        uppSpoutPatterns pattern = mzUtils.GetEnumValue<uppSpoutPatterns>(uopProp.ValueI);
                        SelectedSpoutPattern = pattern.Description();
                
                        break;

                    default:
                        break;
                }
            }

            //if (Clearance <= 0) Clearance = null;
            //if (VerticalPitch <= 0) VerticalPitch = null;
            //if (Margin <= 0) Margin = null;

            NotifyPropertyChanges();
            IsEnabled = true;
            
        }

      
        /// <summary>
        /// Get all downcomer, panel a
        /// </summary>
        /// <param name="selectedApplyTo"></param>
        /// <param name="downcomerIndex"></param>
        /// <param name="panelIndex"></param>
        /// <param name="excludePanelIndex"></param>
        private void GetAllIndexsFrom(string selectedApplyTo, out int? downcomerIndex, out int? panelIndex, out int? excludePanelIndex)
        {
            downcomerIndex = null;
            panelIndex = null;
            excludePanelIndex = null;

        
            string selectedApplyToWithoutSpaces = Regex.Replace(selectedApplyTo, @"\s+", "").ToUpper();
            
            int? idx = mzUtils.ExtractInteger(selectedApplyToWithoutSpaces, out string _, out string prefix);


            if (prefix.StartsWith("DOWNCOMER"))
            {
                downcomerIndex = idx;
            }
            else if (prefix.StartsWith("DECK PANEL"))
            {
                panelIndex = idx;
            }
            else if (prefix.StartsWith("ENDGROUPS"))
            {
                panelIndex = 1;
            }
            else if (selectedApplyToWithoutSpaces.StartsWith("FIELDGROUPS"))
            {
                excludePanelIndex = 1;
            }
        }

        #endregion Methods



        #region Validation of fields 

        private void Validation_DefineLimits()
        {


            List<ValueLimit> Limits = new() { };
            Limits.Add(new ValueLimit("VerticalPitch", (double)0.9375, (double)6, bAllowZero: true));
            Limits.Add(new ValueLimit("Margin", (double)0.125, (double)0.25 * MDAssy.FunctionalPanelWidth, bAllowZero: true));
            Limits.Add(new ValueLimit("Clearance", (double)0.0625, (double)0.25 * MDAssy.Downcomer().Width, bAllowZero: true));
            Limits.Add(new ValueLimit("SpoutLength", (double)MDAssy.Downcomer().SpoutDiameter, (double)MDAssy.Downcomer().Width -0.125, bAllowZero: true));
            
                EditPropertyValueLimits = Limits;
        }
        public override void Activate(Window myWindow)
        {
            if (Activated || MDAssy == null) return;
            base.Activate(myWindow);
            if (EditPropertyValueLimits == null) Validation_DefineLimits();
            ShowCommomProperties();
        }

        /// <summary>
        /// Validate input fields
        /// </summary>
        /// <returns></returns>
        public string Validate_Margin()
        {
         

            if (EditProps.ValueD("Margin") > 0 && EditProps.ValueD("VerticalPitch") > 0)
            {
                return "A Margin Cannot Be Entered If Vertical Pitch Is Specified";
            }

            return string.Empty;
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
               
                switch (aPropertyName.ToUpper())
                {
                    case "VERTICALPITCH":
                    case "MARGIN":

                        result = Validate_Margin();
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
        #endregion Validation of fields 

    }


}
