using MvvmDialogs;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using UOP.WinTray.Projects;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Interfaces;
using UOP.WinTray.UI.Commands;
using UOP.WinTray.UI.Messages;

namespace UOP.WinTray.UI.ViewModels
{
    class Edit_Case_ViewModel : MDProjectViewModelBase, IModalDialogViewModel
    {
        #region Constructors

        public Edit_Case_ViewModel(MDProjectViewModel parentVM, iCaseOwner owner, iCase aCase, string mode, string fieldname = "", string title = null, string headertext = null, string statustext = null)

        {

            Project = parentVM.Project;
            Mode = mode.Trim().ToUpper();
            
            FocusTarget = fieldname;
            if (MDProject == null || aCase == null) return;
              OKButtonText = (Mode == "ADD") ? "Save" : "OK";
          
            try
            {
                
                BusyMessage = "Loading Part Details..";
                IsEnabled = false;

                DisplayUnits = parentVM.DisplayUnits;

                CaseViewModel = new MDCaseViewModel(this, owner, aCase, bReadOnly: false);
                Visibility_UnitToggle = Visibility.Collapsed;
                if (string.IsNullOrWhiteSpace(statustext))
                    GlobalProjectTitle = owner == null ? $"MD Project({MDProject.Name}).{OwnerType.GetDescription()}s.Cases({aCase.Description})" : $"MD Project({MDProject.Name}).{OwnerType.GetDescription()}({ owner.Description }).Cases({aCase.Description})";
                else
                    GlobalProjectTitle = statustext.Trim();

                HeaderText = string.IsNullOrWhiteSpace(headertext) ?  $"MD {OwnerType.GetDescription()} Case '{Case.Description}' Data Input" : headertext.Trim();
                Title = string.IsNullOrWhiteSpace(title) ? $"Edit {OwnerType.GetDescription()} Case Properties" : title.Trim();
            }
            finally
            {
                if (string.IsNullOrWhiteSpace(FocusTarget))
                {
                    if (OwnerType == uppCaseOwnerOwnerTypes.Distributor) FocusTarget = "LiquidRate";
                    if (OwnerType == uppCaseOwnerOwnerTypes.ChimneyTray) FocusTarget = "LiquidFromAbove";

                }



                Validation_DefineLimits();
              

               IsEnabled = true;
                IsOkBtnEnable = true;
                BusyMessage = "";
                Visibility_UnitToggle = Visibility.Visible;
            }
        }
        #endregion Constructors


        #region Properties

        public double MaxMassRate => 100000000;
        public double MaxDensity => 200;

        public override bool IsEnglishSelected
        {
            get => base.IsEnglishSelected;
            set { base.IsEnglishSelected = value; NotifyPropertyChanges(); if (_CaseViewModel == null) return; _CaseViewModel.IsEnglishSelected = value; }
        }
        private MDCaseViewModel _CaseViewModel = new(null, null, null, bReadOnly: false);
        public MDCaseViewModel CaseViewModel { get => _CaseViewModel; set { _CaseViewModel = value; NotifyPropertyChanged("CaseViewModel"); } }
        public iCase Case => _CaseViewModel?.Case;

        public uppCaseOwnerOwnerTypes OwnerType => Case == null ? uppCaseOwnerOwnerTypes.Undefined : Case.OwnerType;

        public int OwnerIndex => Case == null ? 0 : Case.Index;

        private string _OKButtomText = "OK";
        public string OKButtonText { get => _OKButtomText; set { value ??= "OK"; _OKButtomText = value.Trim(); NotifyPropertyChanged("OKButtonText"); } }

        private string _HeaderText = "MD Distributor Data Input";
        public string HeaderText { get => _HeaderText; set { value ??= ""; _HeaderText = value.Trim(); NotifyPropertyChanged("HeaderText"); } }

        public string Mode { get; set; }
        /// <summary>
        /// Dialogservice result
        /// </summary>
        private bool? _DialogResult;
        public bool? DialogResult
        {
            get => _DialogResult;
            private set { _DialogResult = value; NotifyPropertyChanged("DialogResult"); }
        }


        private string _Title = "Edit Distributor Properties";
        public override string Title { get => _Title; set { value ??= ""; _Title = value; NotifyPropertyChanged("Title"); } }

        /// <summary>
        ///the uopProperties curenntly being edited
        ////// </summary>
        public override uopProperties EditProps
        {
            get => CaseViewModel == null ? new uopProperties() : CaseViewModel.EditProps;
            set
            {
                if (CaseViewModel != null) CaseViewModel.EditProps = value;
            }
        }

        public override List<PropertyControlViewModel> PropertyControls
        {
            get => CaseViewModel == null ? new List<PropertyControlViewModel>() : CaseViewModel.PropertyControls;
          
        }
        #endregion Properties
        #region Commands


        private DelegateCommand _CMD_Cancel;
        public ICommand Command_Cancel { get { _CMD_Cancel ??= new DelegateCommand(param => Execute_Cancel()); return _CMD_Cancel; } }

        private DelegateCommand _CMD_OK;
        public ICommand Command_Ok { get { _CMD_OK ??= new DelegateCommand(param => Execute_Save()); return _CMD_OK; } }

        #endregion Commands

        #region Methods
        /// <summary>
        ///the orignal uopProperties curenntly being edited
        ///stored to provided the changed values on save
        ////// </summary>

        public override uopProperties GetEditedProperties(bool bGetJustOne = false)
        {
            if (EditProps == null) return new uopProperties();
            uopProperties orig = EditProps.StoreValuesGet(1, false);
            return orig.GetDifferences(EditProps, bBailOnFistDifference: bGetJustOne);
        }

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
            PropertyControlViewModel rErrorControl = null;
            try
            {

                IsOkBtnEnable = false;
                IsEnabled = false;
                BusyMessage = "Calculation in progress..";
                IsEnabled = false;

                ErrorMessage = ValidateInput(out rErrorControl);

                if (string.IsNullOrEmpty(ErrorMessage))
                    Save();
                else
                    Validate_ShowErrors();
            }

            finally
            {
                IsOkBtnEnable = true;
                IsEnabled = true;
                BusyMessage = "";
                IsEnabled = true;
                if (rErrorControl != null)
                {

                    SetFocus(rErrorControl.Tag);
                }
            }
        }

        private void Save()
        {

            Canceled = true;
            Message_Refresh refresh = null;
            try
            {

                //uopProperties eprops = CaseViewModel.EditProps;
                //uopProperties orig = eprops.StoreValuesGet(1, false);
                //foreach (var item in eprops)
                //{
                //    if(item.HasUnits && item.HasNullValue)
                //    {
                //        if (item.ValueD <= 0)
                //        {
                //            var oprop = orig.Item(item.Name);
                //            if(oprop.IsNullValue) item.Value = item.NullValue;
                //        }
                //    }
                //}


                uopProperties changes = CaseViewModel.GetEditedProperties();
                refresh = new Message_Refresh(bSuppressTree: true, aPartTypeList: new List<uppPartTypes>() { uppPartTypes.Distributor });

                if (changes.Count > 0)
                {
                    Canceled = false;
                    Edited = true;
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

        public void ExecuteLostFocus()
        {
            if (CaseViewModel == null) return;
            List<PropertyControlViewModel> pcontrols = CaseViewModel.PropertyControls;
            foreach (var item in pcontrols)
            {

                if (item.Property.HasUnits && item.Property.IsNullValue)
                    item.Property.SetValue(0d);
              
                item.ExectuteLostFocus();
            }
        }

        #endregion Methods

        #region Validation of fields 

        private void Validation_DefineLimits()
        {


            List<ValueLimit> Limits = new() { };
          
            EditPropertyValueLimits = Limits;
            if (CaseViewModel != null)
            {
                CaseViewModel.DefineLimits();
            }
        }
        public override void Activate(Window myWindow)
        {
            if (Activated || MDProject == null) return;
            base.Activate(myWindow);
            if (EditPropertyValueLimits == null) Validation_DefineLimits();
            ExecuteLostFocus();
            if (!string.IsNullOrWhiteSpace(FocusTarget))
            { 
              SetFocus(FocusTarget); 
            }

          
         
       
        }

        protected override string GetError(string aPropertyName)
        {
            if (!Activated || CaseViewModel == null)
                return "";

            return CaseViewModel.GetErrorString(aPropertyName);

         
        }
        private void Validate_ShowErrors()
        {
            if (CaseViewModel != null) ErrorCollection = CaseViewModel.ErrorCollection;

            if (ErrorCollection == null) { ErrorMessage = ""; return; }
            if (ErrorCollection.Count <= 0) { ErrorMessage = ""; return; }
            string msg = ErrorCollection[0].Item2;
            ErrorMessage = msg;
        }

        private string ValidateInput(out PropertyControlViewModel rErrorControl )
        {
            rErrorControl = null;
            if (!Activated || CaseViewModel == null)
                return "";
            return CaseViewModel.ValidateInput(out rErrorControl);

        }

        /// <summary>
        /// Invokes on notify property changed for range properties.
        /// </summary>
        private void NotifyPropertyChanges()
        {

            try
            {
                if (!Activated) return;
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

            }
            catch { }




        }
        #endregion Validation of fields
    }
}
