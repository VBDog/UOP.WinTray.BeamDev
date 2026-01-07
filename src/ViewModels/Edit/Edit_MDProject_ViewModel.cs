using DocumentFormat.OpenXml.Office2016.Excel;
using MvvmDialogs;
using System.Collections.Generic;
using System.Windows;
using UOP.WinTray.Projects;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Materials;
using UOP.WinTray.Projects.Parts;
using UOP.WinTray.UI.BusinessLogic;
using UOP.WinTray.UI.Commands;
using UOP.WinTray.UI.Messages;

namespace UOP.WinTray.UI.ViewModels
{
    /// <summary>
    /// ViewModel for edit project properties
    /// </summary>
    public class Edit_MDProject_ViewModel : NewMDProjectViewModelBase, IModalDialogViewModel
    {
        #region Constants


        private const string MATERIAL_410_SS = "410 SS";
        private const string MATERIAL_304_SS = "304 SS";
        private const string CONVERGENCE_LIMIT_CHANGE = "Convergance Limit Change";
        private const string METRIC_BOLTING_REQUESTED = "Metric Bolting Requested";
        private const string EDIT_PROJECT_PROPERTIES = "Edit Project Properties";
        private const string MATERIAL_410_2_304_MESSAGE = "A change of the project bolting to metric has been detected." +
                                                        "\n" + "This change will cause the project tray ranges with 410 SS bolting material to change to 304 SS bolting." +
                                                        "\n" + "Select 'Yes' to accept the change or 'No' to discard the change.";

        private const string CONVERGENCE_LIMIT_CHANGE_MESSAGE = "A change to the projects spout area convergence limit has been requested." +
                                                        "\n" + "This change may affect some or all of the spout area distributions and spout groups of the tray ranges in the project." +
                                                        "\n" + "Select 'Yes' to accept the change or 'No' to discard the change.";



        #endregion Constants

        

        #region Constructors

        public Edit_MDProject_ViewModel(mdProject project, string fieldName = "",bool isnewproject = false): base()
        {

            this.EditProjectPropertyVisibilty = true;
            IsNewProject = isnewproject;
            FocusTarget = fieldName;
            DialogService = WinTrayMainViewModel.WinTrayMainViewModelObj.DialogService;

            uopProperties eProps = new();
            MDProject = project;
            uppProjectTypes pType = uppProjectTypes.MDSpout;
            BoltingIsEnglish = false;
            NotesHaveChanged = false;
        

            if (project != null)
            {
                BoltingIsEnglish = project.Bolting == uppUnitFamilies.English;

#if DEBUG
                if (IsNewProject)
                {
                    MDProject.KeyNumber = "12345";
                    MDProject.Column.ManholeID = 24;
                    MDProject.IDNumber = "11-999";
                }
#endif

                pType = project.ProjectType;
                eProps.Append(project.CurrentProperties(), aCategory: "PROJECT", aPartType: uppPartTypes.Project);
                eProps.Append(project.Customer.CurrentProperties(), aCategory: "CUSTOMER", aPartType: uppPartTypes.Customer);
                eProps.Append(project.Column.CurrentProperties(), aCategory: "COLUMN", aPartType: uppPartTypes.Column);
                eProps.Append(project.DrawingNumbers, aCategory: "PROJECT", aPartType: uppPartTypes.Document);
                DisplayUnits = project.DisplayUnits;
            }
            else
            {
                DisplayUnits = uppUnitFamilies.Metric;
            }

            EditProps = eProps;


            Customers = appApplication.Customers;
            Services = appApplication.Services;
            Licensors = appApplication.Liscensors;
            Contractors = appApplication.Contractors;
            TrayVendors = appApplication.TrayVendors;
            
            SelectedProjectType = pType.GetDescription(); 
            ProjectTitle = $"Edit Project Properties";
            if (MDProject != null) 
            {
                base.IsCustomerDrawingUnitsEnglish = MDProject.CustomerDrawingUnits == uppUnitFamilies.English;
                base.IsManufacturingDrawingUnitsEnglish = MDProject.ManufacturingDrawingUnits == uppUnitFamilies.English;


            }



        }

        #endregion Constructors


        #region Properties      

        private bool _IsNewProject = false;

        public bool IsNewProject
        {
            get => _IsNewProject;
            set { _IsNewProject = value; NotifyPropertyChanged("IsNewProject"); NotifyPropertyChanged("Visibility_NewProject"); }
        }

        public new bool NotesHaveChanged { get; set; }
        public Visibility Visibility_NewProject
        {
            get => (!IsNewProject) ? Visibility.Visible : Visibility.Collapsed;
        }
       
        public override bool IsEnglishSelected 
        { 
            get => base.IsEnglishSelected;
            set { base.IsEnglishSelected = value; NotifyPropertyChanges(); }
        }


        public string DNFunctional
        {
            get => EditProps.ValueS("Functional");
            set { EditProps.SetValue("Functional", value); NotifyPropertyChanged("DNFunctional"); }
        }

        public string DNInstallation
        {
            get => EditProps.ValueS("Installation");
            set { EditProps.SetValue("Installation", value); NotifyPropertyChanged("DNInstallation"); }
        }

        public string DNManufacturing
        {
            get => EditProps.ValueS("Manufacturing");
            set { EditProps.SetValue("Manufacturing", value); NotifyPropertyChanged("DNManufacturing"); }
        }
        #endregion Properties

        #region Commands

        protected DelegateCommand _CMD_Save;
        public DelegateCommand Command_Save { get { if (_CMD_Save is null) _CMD_Save = new DelegateCommand(parm => Execute_Save()); return _CMD_Save; } }



        private DelegateCommand _CMD_ToggleUnits;
        public override DelegateCommand Command_ToggleUnits {get { if (_CMD_ToggleUnits is null) _CMD_ToggleUnits = new DelegateCommand(param => ToggleUnits()); return _CMD_ToggleUnits; } }

        #endregion Commands

        #region Methods       

        /// <summary>
        /// Toggle Units from English to Metric and vice-versa
        /// </summary>
        public override void ToggleUnits()
        {
            base.ToggleUnits();
            NotifyPropertyChanges();

        }

        public override void NotifyPropertyChanges()
        {
            NotifyPropertyChanged("ManholeID");
            NotifyPropertyChanged("RingThickness");
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

                IsValid = ValidateInput(out string errString);
                if (!IsValid || !string.IsNullOrEmpty(errString))
                {
                    
                    ErrorMessage = errString;
                    MessageBox.Show(errString, "Invalid Data", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                Save();
            }
            finally
            {
                IsOkBtnEnable = true;
                IsEnabled = true;
                BusyMessage = "";
                IsEnabled = true;
            }
        }

        /// <summary>
        /// Saved the modified data into underlying properties and then performs calculations
        /// </summary>
        public void Save()
        {
            int changecount = 0;
            ClearFocus();
           
            uopProperty prop;
            bool invalidateall = false;
            uopProperties changes;
            uopProperties uopProps = EditProps;
            mdProject project = MDProject;
            Message_Refresh refresh = null;

            if (uopProps == null || project == null) return;
          

            try
            {
                changes = EditProps;
                changes = GetEditedProperties();
              if(string.Compare(KeyNumber,Project.KeyNumber,true) != 0)
                {
                    if (!changes.Contains("KeyNumber")) changes.Add(new uopProperty("KeyNumber", KeyNumber));
                }
                if (string.Compare(IDNumber, Project.IDNumber, true) != 0)
                {
                    if (!changes.Contains("IDNumber")) changes.Add(new uopProperty("IDNumber", IDNumber));
                }

                changecount = changes.Count;
                if (changecount <= 0) return;
                colUOPTrayRanges ranges = Project.TrayRanges;
                mdTrayRange range;

                this.IsEnabled = false;
                this.BusyMessage = CALCULATION_IN_PROGRESS;

               
                uppUnitFamilies selectedBoltingFamily = uppUnitFamilies.English;
                bool bChangeBolting = false;

                if (Bolting == uppUnitFamilies.Metric && !IsNewProject)
                {
                    uopMaterials bMats = MDProject.RangeBoltingMaterials;

                    if (null != bMats.GetByFriendlyName(uppMaterialTypes.Hardware, "410 SS"))
                    {
                     
                        if (MessageBox.Show(MATERIAL_410_2_304_MESSAGE, METRIC_BOLTING_REQUESTED, MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK)
                        {
                            selectedBoltingFamily = uppUnitFamilies.Metric;
                            bChangeBolting = true;
                        }
                        else
                        {
                            selectedBoltingFamily = uppUnitFamilies.English;
                        }
                    }
                }

                if (bChangeBolting)
                {
                    
                    if (!changes.TryGet("Bolting", out prop))
                    {
                        prop = EditProps.GetProperty("Bolting");
                        prop.SetValue(selectedBoltingFamily);

                        changes.Add(prop);
                        changecount++;
                    }
                    prop = changes.GetProperty("Bolting");
                    

                    prop.RangeGUID = ranges.SelectedRangeGUID;

                    //change 410 SS ranges to 304 SS
                    uopHardwareMaterial hMat = (uopHardwareMaterial)uopGlobals.goHardwareMaterialOptions().GetByFriendlyName(uppMaterialTypes.Hardware, "304 SS");

                    foreach (uopTrayRange item in ranges)
                    {
                    
                        if (string.Compare(item.HardwareMaterial.MaterialName, "410 SS", true) == 0)
                            item.HardwareMaterial = hMat;
                    }
                 
                }

                if (EditProps.ValueD("ConvergenceLimit") != project.ConvergenceLimit && !IsNewProject)
                {
                 

                    if (MessageBox.Show(CONVERGENCE_LIMIT_CHANGE_MESSAGE, CONVERGENCE_LIMIT_CHANGE, MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK)
                    {
                        if (!changes.TryGet("ConvergenceLimit", out prop))
                        {
                            prop = EditProps.GetProperty("ConvergenceLimit");
                            prop.SetValue(ConvergenceLimit);
                           
                            changes.Add(prop);
                            changecount++;
                            invalidateall = true;
                        }
                    }
                    else
                    {
                        changes.Remove("ConvergenceLimit");
                        changecount = changes.Count;
                    }
                }


                if (changecount > 0 || NotesHaveChanged)
                {
                    refresh = new Message_Refresh(bSuppressTree: true, aPartTypeList: new List<uppPartTypes>() { uppPartTypes.Project }, bCloseDocuments: true);
                    project.HasChanged = true;
                    foreach (uopProperty item in changes)
                    {
                        if (item.IsNamed("RingThickness,ManholeID")) item.PartType = uppPartTypes.Column;
                            switch (item.PartType)
                        {
                            case uppPartTypes.Project:

                                if (item.IsNamed("ConvergenceLimit,MetricSpouting,ReverseSort")) invalidateall = true;
                                if (item.Name.ToUpper().Contains("UNITS")) refresh.SuppressTree = false;
                                project.PropValSet(item.Name, item.Value);

                                foreach (uopTrayRange urange in ranges)
                                {
                                    range = (mdTrayRange)urange;
                                    if (item.IsNamed("ConvergenceLimit,MetricSpouting")) range.TrayAssembly.ResetComponents();
                                    range.Alert(item);
                                }
                              
                           

                                break;
                            case uppPartTypes.Customer:
                                project.Customer.PropValSet(item.Name, item.Value);
                                break;
                            case uppPartTypes.Column:
                                { 
                                    project.Column.PropValSet(item.Name, item.Value);
                                    foreach (uopTrayRange urange in ranges)
                                    {
                                        urange.Alert(item);
                                    }
                                    if (item.IsNamed("RingThickness,ManholeID"))
                                    {
                                        if (refresh.PartTypeList.IndexOf(uppPartTypes.TrayRange) <= 0) refresh.PartTypeList.Add(uppPartTypes.TrayRange);
                                    }
                                    break;
                                }
                            case uppPartTypes.Document:
                                project.DrawingNumbers.PropValSet(item.Name, item.Value);

                                break;

                        }
                    }
                }
            }

            finally
            {
                this.ClearFocus();
                if (IsValid && refresh != null)
                {
                    if (invalidateall) refresh = new Message_Refresh();

                    if (NotesHaveChanged) refresh.SuppressTree = false;

                    project.Properties.CopyValues(EditProps);
                    RefreshMessage = refresh;

                    bool changed = IsNewProject || changecount > 0 || NotesHaveChanged;
                    DialogResult = changed;
                    if (changed)
                    {

                        MDProjectViewModel vm = ParentVM != null ? (MDProjectViewModel)ParentVM : refresh.MainVM.MDProjectViewModel;
                        if (vm != null) vm.RespondToEdits(this, true, uppPartTypes.Project);
                    }

                }

            }
           
        }

        public override void Activate(Window myWindow)
        {
            if (Activated || MDAssy == null) return;
            base.Activate(myWindow);
            //if (EditPropertyValueLimits == null) Validation_DefineLimits();
            if (!string.IsNullOrWhiteSpace(FocusTarget)) SetFocus(FocusTarget);
        }
        #endregion 


    }
}
