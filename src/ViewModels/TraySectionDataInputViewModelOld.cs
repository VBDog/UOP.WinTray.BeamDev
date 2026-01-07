using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Shapes;
using Honeywell.UOP.WinTray.API.Calculations.Classes;
using Honeywell.UOP.WinTray.API.Calculations.Enums;
using Honeywell.UOP.WinTray.API.Calculations.Modules;
using Honeywell.UOP.WinTray.BusinessLogic;
using Honeywell.UOP.WinTray.BusinessLogic.Edit;
using Honeywell.UOP.WinTray.Events.EventAggregator;
using Honeywell.UOP.WinTray.Model;
using MvvmDialogs;

namespace Honeywell.UOP.WinTray.ViewModels
{
    public class TraySectionDataInputViewModelOld : MDSpoutViewModelBase, IModalDialogViewModel
    {
        #region Constants

        public static string Category = "RANGE";

        #endregion

        #region Variables
        private readonly int Min_Shell_ID = 32;
        private readonly int Max_shell_ID = 600;
        private readonly double Min_Ring_Width = 0.5;
        private readonly double Max_Ring_Width = 12;
        private readonly double Min_Ring_Bolt_Bar_Thickness = 0.125;
        private readonly double Max_Ring_Bolt_Bar_Thickness = 1;
        private readonly double Min_Ring_Spacing = 6;
        private readonly double Max_Ring_Spacing = 150;
        private readonly double Ring_Clearance_Factor = 1.5;
        private readonly double Tray_Diameter_Factor = 1;
        private bool? dialogResult;
        private string errorMessage;
        private DelegateCommand cancelCommand; //Cancel button delegate
        private DelegateCommand okCommand;     //Ok button delegate
        private DelegateCommand helpCommand;   //Help button delegate
        private List<TrayType> lstTrayType;
        private List<BoltingMaterial> lstBoltingMaterial;
        private TrayType selectedTrayType;
        private BoltingMaterial selectedBoltingMaterial;
        private IStorageHelper<mdProject> storageService;
        private mdTrayRange trayRange;
        private int currentSelectedIndex = -1;
        private IEditRangeHelper editHelper;
        private IRangeHelper rangeHelper;
        private bool useDefaultRingClearance;
        private uppMDDesigns rangeType;
        private double muliplier = 1;
        private bool isAddingNewRange;
        private uopProperties moreProperties;
        private IProjectMDMain projectMDMain;

        #endregion

        #region Properties
        /// <summary>
        /// Dialogservice result
        /// </summary>
        public bool? DialogResult
        {
            get
            {
                return this.dialogResult;
            }
            private set
            {
                this.dialogResult = value;
                NotifyPropertyChanged("DialogResult");
            }
        }
        public double ShellID 
        { 
            get => SelectedTrayRange.ShellID * muliplier;
            set
            {
                SelectedTrayRange.ShellID = value / muliplier;
                RingID = (value / muliplier) - (2 * SelectedTrayRange.RingWidth / muliplier);        
            }
        }        

        public int TrayCount { get => SelectedTrayRange.TrayCount; }

        public double TraySpacing 
        { 
            get => SelectedTrayRange.RingSpacing * muliplier; 
            set => SelectedTrayRange.RingSpacing = value / muliplier; 
        }

        public double RingWidth 
        { 
            get => SelectedTrayRange.RingWidth * muliplier; 
            set => SelectedTrayRange.RingWidth = value / muliplier; 
        }

        public double TrayOD 
        { 
            get => SelectedTrayRange.OverrideTrayDiameter * muliplier; 
            set => SelectedTrayRange.OverrideTrayDiameter = value / muliplier; 
        }

        public double RingID 
        {             
            get => SelectedTrayRange.RingID; 
            set => SelectedTrayRange.RingID = value; 
        }

        /// <summary>
        /// Validation Error message
        /// </summary>
        public string ErrorMessage
        {
            get
            {
                return this.errorMessage;
            }

            set
            {
                this.errorMessage = value;
                NotifyPropertyChanged("ErrorMessage");
            }
        }

        public double RingClearance 
        { 
            get
            {
                double aVal = SelectedTrayRange.OverrideRingClearance;
                double dVal = uopUtils.uop_BoundingClearance(SelectedTrayRange.ShellID);

                if(aVal == 0)
                {
                    aVal = dVal;
                }
                
                useDefaultRingClearance = aVal == dVal ? true : false;
                return aVal * muliplier;
            }
            
            set => SelectedTrayRange.RingClrc = value; 
        }

        public double RingThickness 
        { 
            get => SelectedTrayRange.RingThk * muliplier; 
            set => SelectedTrayRange.RingThk = value; 
        }   
        
        public int TrayRangeFrom { get => SelectedTrayRange.RingStart; set => SelectedTrayRange.RingStart = value; }

        public int TrayRangeTo { get => SelectedTrayRange.RingEnd; set => SelectedTrayRange.RingEnd = value; }

        public bool UseDefaultRingClearance 
        { 
            get => useDefaultRingClearance;
            set
            {
                if(useDefaultRingClearance != value)
                {
                    double aVal = SelectedTrayRange.OverrideRingClearance / muliplier;
                    double dVal = uopUtils.uop_BoundingClearance(SelectedTrayRange.ShellID);
                                        
                    if(value)
                    {
                        aVal = 0.0;
                    }
                    else
                    {
                        if (aVal == dVal)
                        {
                            aVal = 0;
                        }
                    }
                    SelectedTrayRange.OverrideRingClearance = aVal;
                    useDefaultRingClearance = value;
                    NotifyPropertyChanged("UseDefaultRingClearance");
                }
            }
        }

        public string TraySection { get => SelectedTrayRange.Name(true); }
        

        public uppMDDesigns SelectedRangeType
        {
            get { return SelectedTrayRange.TrayAssembly.DesignFamily; }
            set
            {
                SelectedTrayRange.DesignFamily = value;
                //NotifyPropertyChanged("SelectedRangeType");
            }
        }
        
        //public TrayType SelectedTrayType
        //{
        //    get
        //    {
        //        return SelectedTrayRange.DesignFamily;
        //    }
        //    set
        //    {
        //        if (value != selectedTrayType)
        //        {
        //            selectedTrayType = value;
        //            NotifyPropertyChanged("SelectedTrayType");
        //        }
        //    }
        //}

        public mdTrayRange SelectedTrayRange
        {
            get
            {
                if (currentSelectedIndex != this.storageService.SelectedRangeIndex || null == trayRange)
                {
                    trayRange = (mdTrayRange)this.storageService.MainObject.TrayRanges.Item(this.storageService.SelectedRangeIndex);               

                }

                return trayRange;
            }
        }
    
        public ICommand CancelCommand
        {
            get
            {
                if (cancelCommand == null)
                {
                    cancelCommand = new DelegateCommand(param => Cancel());
                }

                return cancelCommand;
            }
        }

        public ICommand OkCommand
        {
            get
            {
                if (okCommand == null)
                {
                    okCommand = new DelegateCommand(param => SaveTraySectionDataAsync());
                }

                return okCommand;
            }
        }

        public ICommand HelpCommand
        {
            get
            {
                if (helpCommand == null)
                {
                    // _Cancel = new DelegateCommand();
                }

                return helpCommand;
            }
        }        

        public BoltingMaterial SelectedBoltingMaterial
        {
            get
            {
                return selectedBoltingMaterial;
            }
            set
            {
                if (value != selectedBoltingMaterial)
                {
                    selectedBoltingMaterial = value;
                }
            }
        }

        public bool IsToShowRingWidth
        {
            get
            {
                return SelectedTrayRange.ProjectFamily == uppProjectFamilies.uopFamMD;
            }
        }

        public ObservableCollection<TrayType> TrayTypes { get => new ObservableCollection<TrayType>(lstTrayType); }

        public ObservableCollection<BoltingMaterial> BoltingMaterials { get => new ObservableCollection<BoltingMaterial>(lstBoltingMaterial); }

        #endregion

        #region Constrcutor

        public TraySectionDataInputViewModelOld(IStorageHelper<mdProject> storageHelper,
                                                IPropertyHelper propertyService,
                                                IEditRangeHelper editHelper,
                                                IEventAggregator eventAggregator,
                                                IRangeHelper rangeHelper,
                                                IProjectMDMain projectMDMain) : base(propertyService, eventAggregator)
        {
            this.storageService = storageHelper;
            this.projectMDMain = projectMDMain;
            this.editHelper = editHelper;
            this.editHelper.ToggleUnits();
            this.muliplier = this.editHelper.Multiplier;
            this.storageService.CurrentObject = this.storageService.MainObject.Clone();
            SelectedTrayRange.Part.GetProps().Members.ForEach(x =>
            {
                if (null != x)
                    x.ValueChanged = false;
            });
           
            this.rangeHelper = rangeHelper;

            lstBoltingMaterial = rangeHelper.GetBoltingMaterials();

            if (null != lstBoltingMaterial && lstBoltingMaterial.Count > 0)
            {
                SelectedBoltingMaterial = lstBoltingMaterial[0];
                Initialize();
            }
        }

        #endregion Constrcutor

        #region Methods

        private void SaveTraySectionDataAsync()
        {
            var bNewMat = string.Compare(SelectedTrayRange.HardwareMaterial.FamilySelectName, SelectedBoltingMaterial.Name, StringComparison.OrdinalIgnoreCase) != 0;
            var pname = "HardWareMaterial";
            var validationResult = ValidateInput();
            this.ErrorMessage = validationResult;
            if (string.IsNullOrEmpty(this.ErrorMessage))
            {
                //if (bNewMat)
                //{
                //   var pname = "HardWareMaterial";

                //if(!isAddingNewRange && storageService.MainObject.RangeCount > 1 Then

                //   reply = MsgBox("Apply New Hardware Material '" & cboStack.Text & "' To All Project Tray Sections?", vbQuestion + vbYesNo + vbDefaultButton2, "New Hardware Material Selected")
                //       If reply = vbYes Then pname = "HardWareMaterials"
                //    End If


                //moreProperties = new uopProperties();
                //moreProperties.AddProperty(pname, selectedBoltingMaterial.Name, aPartType: uppPartTypes.prtTrayRange);

                uopProperties uopProps = editHelper.GetEditedProps(SelectedTrayRange);
                uopProps.Category = "RANGE";
                this.projectMDMain.SetCurrentChanges(uopProps);
            }
        }

        private void Cancel()
        {
            this.DialogResult = true;
        }

        private void UpDateUnits()
        {
            double multi = editHelper.GetMultiplier();
            ShellID = ShellID * multi;

            if (IsToShowRingWidth)
            {
                RingWidth = RingWidth * multi;
            }
            else
            {
                RingID = RingID * multi;
            }

            RingThickness = RingThickness * multi;
            //Ring Spacing = RingSpacing * multi;
            //OverrideRingClearance = OverrideRingClearance * multi;
            //OverrideTrayDiameter = OverrideTrayDiameter * multi
        }

        private void Initialize()
        {
            //Need to think about Multiplier
            double aVal = SelectedTrayRange.OverrideRingClearance;
            double dVal = uopUtils.uop_BoundingClearance(SelectedTrayRange.ShellID);
            if (aVal == 0)
            {
                aVal = dVal;
            }

            SelectedTrayRange.OverrideRingClearance = aVal; // * Multiplier

            if (dVal == aVal)
            {
                UseDefaultRingClearance = true;
                //tBox.Enabled = False;
            }
            else
            {
                UseDefaultRingClearance = false;
                //tBox.Enabled = True;
            }
            //SelectedTrayTypeIn = SelectedTrayRange.TrayAssembly.DesignFamily;
            //            //SelectedTrayRange.OverrideTrayDiameter = SelectedTrayRange.OverrideTrayDiameter * Multiplier
            //'        If cboRevamp.Visible Then
            //        If oProject.InstallationType = uopRevamp Then
            //            cboRevamp.Text = .RevampStrategy
            //        End If
        }

        #endregion


        #region DisplayMethods

        /// <summary>
        /// Populates the combo box on the tray data design tab with all of the defined tray ranges
        /// in the project.
        /// </summary>
        public void ShowTrayRanges()
        {          
            //MSFlexGrid aGrid As MSFlexGrid
            string aGUID;
            PictureBox aPic;
            Shape aShape;


            //StatusText = "Refreshing Tray Section List"

            bool bLoadingTrays = true;

            //get all of the defined ranges
            var tRanges = rangeHelper.GetTrayRanges(true);


            //UNLOAD RANGE CONTROLS
            // For i = flxSpoutGroups.UBound To 1 Step - 1
            //    Set aGrid = flxSpoutGroups.Item(i)
            //    aGUID = aGrid.Tag
            //    If Ranges.GetByGuid(aGUID) Is Nothing Then


            //        Unload flxSpoutGroups(i)

            //        For j = 1 To flxDowncomers.UBound
            //            Set aGrid = flxDowncomers.Item(j)
            //            If aGrid.Tag = aGUID Then Unload flxDowncomers.Item(aGrid.Index)
            //        Next j

            //        For j = 1 To flxDeckPanels.UBound
            //            Set aGrid = flxDeckPanels.Item(j)
            //            If aGrid.Tag = aGUID Then Unload flxDeckPanels.Item(aGrid.Index)
            //        Next j


            //        Set aPic = picDisplay

            //    End If

            //Next i

            //ClearData

            //If Enabled And Visible Then
            //    SetFocus
            //    If cboTrays.Visible Then cboTrays.SetFocus
            //End If

            //bLoadingTrays = False
        }

        #endregion

        #region Private Methods

        private string ValidateInput()
        {
            string errorString = string.Empty;
            double ringId;
            double multiplier = 1;
            if (TrayRangeFrom <= 0)
            {
                errorString = "Inavlid Starting Tray Number";
            }
            else if (TrayRangeTo <= 0)
            {
                errorString = "Invalid Ending Tray Number";
            }
            else if (TrayRangeFrom > TrayRangeTo)
            {
                errorString = "Invalid Tray Number Range";
            }

            if (!string.IsNullOrEmpty(errorString))
            {
                return errorString;
            }

            var iuopTrayRange = this.storageService.MainObject.TrayRanges.RangeIsOccupied(TrayRangeFrom, TrayRangeTo, uppStackPatterns.stkContinuous, SelectedTrayRange);
            if (iuopTrayRange != null)
            {
                errorString = "Invalid Tray Number Range - One or More of the Entered Ring Numbers Are Already Occupied By Another Tray Section";

            }

            ringId = ShellID - 2 * RingWidth;
            if (ShellID / multiplier < 32)
            {
                errorString = "Minimum Shell I.D. = " + string.Format("0.000", Min_Shell_ID * multiplier);
            }
            else if (ShellID / multiplier > 600)
            {
                errorString = "Max Shell I.D. = " + string.Format("0.000", Max_shell_ID * multiplier);
            }
            else if (RingWidth / multiplier <= Min_Ring_Width)
            {
                errorString = "Minimum Ring Width = " + (Min_Ring_Width * multiplier);
            }
            else if (RingWidth / multiplier > 12)
            {
                errorString = "Maximum Ring Width = " + (12 * multiplier);
            }
            else if (SelectedTrayRange.ProjectType != uppProjectTypes.uopProjMDSpout)
            {
                if (RingThickness / multiplier < Min_Ring_Bolt_Bar_Thickness)
                {
                    errorString = "Ring/Bolting Bar Thickness Must Be Greater Than " + (Min_Ring_Bolt_Bar_Thickness * multiplier);
                }
                else if (RingThickness / multiplier > Max_Ring_Bolt_Bar_Thickness)
                {
                    errorString = "Ring/Bolting Bar Thickness Must Be Less Than or Equal To " + (Max_Ring_Bolt_Bar_Thickness * multiplier);
                }
            }
            else if (TraySpacing / multiplier < Min_Ring_Spacing)
            {
                errorString = "Ring Spacing Must Be Greater Than or Equal To " + (Min_Ring_Spacing * multiplier);
            }
            else if (TraySpacing / multiplier > Max_Ring_Spacing)
            {
                errorString = "Ring Spacing Must Be Less Than or Equal To " + (Max_Ring_Spacing * muliplier);
            }
            else if (UseDefaultRingClearance == false)
            {
                var minRingClearance = uopUtils.uop_BoundingClearance(ShellID / multiplier);
                var maxRingClearane = minRingClearance + Ring_Clearance_Factor;
                minRingClearance = minRingClearance - Ring_Clearance_Factor;
                if (RingClearance / multiplier < minRingClearance)
                {
                    errorString = "Ring Clearance Must Be Greater Than or Equal To " + (minRingClearance * multiplier);
                }
                else if (RingClearance / multiplier > maxRingClearane)
                {
                    errorString = "Ring Clearance Must Be Less Than or Equal To " + (maxRingClearane * multiplier);
                }
            }
            else if (TrayOD != 0)
            {
                var minTrayDiameter = (ringId / multiplier) + Tray_Diameter_Factor;
                var maxTrayDiameter = (ShellID / multiplier) - Tray_Diameter_Factor;
                if (TrayOD / multiplier < minTrayDiameter)
                {
                    errorString = "Override Tray Diameter Must Be Greater Than or Equal To " + (minTrayDiameter * multiplier);
                }
                else if (TrayOD / multiplier > maxTrayDiameter)
                {
                    errorString = "Override Tray Diameter Must Be Less Than or Equal To " + (maxTrayDiameter * multiplier);
                }
            }

            return errorString;
        }
        #endregion

    }
}
