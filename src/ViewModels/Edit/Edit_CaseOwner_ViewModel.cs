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
    class Edit_CaseOwner_ViewModel : ViewModel_Base, IModalDialogViewModel
    {
        #region Constructors

        public Edit_CaseOwner_ViewModel(MDProjectViewModel parentVM, iCaseOwner owner, string mode, string fieldname = "", string title = null, string headertext = null, string statustext = null)

        {

            Project = parentVM.Project;
            Owner = owner;
            FocusTarget = fieldname;
            if (MDProject == null || owner == null) return;
            Mode = mode.Trim().ToUpper();
            OKButtonText = (Mode == "ADD") ? "Save" : "OK";

            try
            {
                IsEnabled = false;
                BusyMessage = "Loading Part Details..";

                DisplayUnits = MDProject.DisplayUnits;
              
               OwnerViewModel= new MDCaseOwnerViewModel(MDProject, Owner, bReadOnly: false) ;
                if (string.IsNullOrWhiteSpace(statustext))
                    GlobalProjectTitle = $"MD Project({MDProject.Name}).{OwnerType.GetDescription()}s({owner.Description})";
                else
                    GlobalProjectTitle = statustext.Trim();

                HeaderText = string.IsNullOrWhiteSpace(headertext) ? $"MD {OwnerType.GetDescription()} Data Input" : headertext.Trim();
                Title = string.IsNullOrWhiteSpace(title) ? $"Edit {OwnerType.GetDescription()} Properties" : title.Trim();

                Visibility_UnitToggle = Visibility.Collapsed;
            
            }
            finally
            {
                Validation_DefineLimits();
                IsEnabled = true;
                IsOkBtnEnable = true;
                BusyMessage = "";
            }
        }
        #endregion Constructors

        #region Properties
        public override bool IsEnglishSelected
        {
            get => base.IsEnglishSelected;
            set { base.IsEnglishSelected = value; NotifyPropertyChanges(); if (_OwnerViewModel == null) return; _OwnerViewModel.IsEnglishSelected = value; }
        }

        private MDCaseOwnerViewModel _OwnerViewModel = new(null, null, bReadOnly: false );
        public MDCaseOwnerViewModel OwnerViewModel { get => _OwnerViewModel; set { _OwnerViewModel = value; NotifyPropertyChanged("OwnerViewModel"); } }
        public iCaseOwner Owner { get; set; } = null;

        public uppCaseOwnerOwnerTypes OwnerType => Owner == null ? uppCaseOwnerOwnerTypes.Undefined : Owner.OwnerType;

        public int OwnerIndex => Owner == null ? 0 : Owner.Index;

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
            get => OwnerViewModel == null ? new uopProperties() : OwnerViewModel.EditProps;
            set
            {
                if (OwnerViewModel != null) OwnerViewModel.EditProps = value;
            }
        }
        #endregion Properties
        #region Commands


        private DelegateCommand _CMD_Cancel;
        public ICommand Command_Cancel { get { _CMD_Cancel ??=  new DelegateCommand(param => Execute_Cancel()); return _CMD_Cancel; } }

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

               


                uopProperties changes =  OwnerViewModel.GetEditedProperties();
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
            if (OwnerViewModel == null) return;
            List<PropertyControlViewModel> pcontrols = OwnerViewModel.PropertyControls;
            foreach (var item in pcontrols)
            {
                item.ExectuteLostFocus();
            }
        }

        #endregion Methods

        #region Validation of fields 

       

        private void Validation_DefineLimits()
        {

          
            List<ValueLimit> Limits = new() { };
          
            EditPropertyValueLimits = Limits;
            if (OwnerViewModel != null) OwnerViewModel.DefineLimits();
        }
        public override void Activate(Window myWindow)
        {
            if (Activated || MDProject == null) return;
            base.Activate(myWindow);
            if (EditPropertyValueLimits == null) Validation_DefineLimits();
            if (!string.IsNullOrWhiteSpace(FocusTarget)) SetFocus(FocusTarget);
            ExecuteLostFocus();
        }

        protected override string GetError(string aPropertyName)
        {
            if (!Activated || OwnerViewModel == null)
                return "";

            return OwnerViewModel.GetErrorString(aPropertyName);
      
        }

        private void Validate_ShowErrors()
        {
            if (OwnerViewModel != null) ErrorCollection = OwnerViewModel.ErrorCollection;
            if (ErrorCollection == null) { ErrorMessage = ""; return; }
            if (ErrorCollection.Count <= 0) { ErrorMessage = ""; return; }
            string msg = ErrorCollection[0].Item2;
            ErrorMessage = msg;
        }

        public string ValidateInput(out PropertyControlViewModel rErrorControl )
        {
            rErrorControl = null;
            if (!Activated || OwnerViewModel == null)
                return "";
            return OwnerViewModel.ValidateInput(out rErrorControl);

       
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
