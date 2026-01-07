using UOP.WinTray.Projects;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.UI.Messages;
using UOP.WinTray.UI.Commands;
using MvvmDialogs;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace UOP.WinTray.UI.ViewModels
{
    public class Edit_PropertyControlContainer_ViewModel : ViewModel_Base, IModalDialogViewModel
    {
        #region Constructors

        public Edit_PropertyControlContainer_ViewModel(ViewModel_Base parentVM, PropertyControlContainer container, string mode, string fieldname = "", string title = null, string headertext = null, string statustext = null)

        {

            Project = parentVM.Project;
            Mode = mode.Trim().ToUpper();

            FocusTarget = fieldname;
            if (MDProject == null || container == null) return;
            OKButtonText = (Mode == "ADD") ? "Save" : "OK";

            try
            {

                BusyMessage = "Loading Details..";
                IsEnabled = false;

                DisplayUnits = parentVM.DisplayUnits;

                Container = container;
                Visibility_UnitToggle = Visibility.Collapsed;
                GlobalProjectTitle = string.IsNullOrWhiteSpace(statustext) ? "" : statustext.Trim();

                HeaderText = string.IsNullOrWhiteSpace(headertext) ? "Edit Properties" : headertext.Trim();
                Title = string.IsNullOrWhiteSpace(title) ?  "Edit Properties" : title.Trim();
            }
            finally
            {
               



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
            set { base.IsEnglishSelected = value; NotifyPropertyChanges(); if (_Container == null) return; _Container.IsEnglishSelected = value; }
        }
        private PropertyControlContainer _Container = null;
        public PropertyControlContainer Container { get => _Container; set { _Container = value; NotifyPropertyChanged("Container"); } }
  
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
            get => Container == null ? new uopProperties() : Container.EditProps;
            set
            {
                if (Container != null) Container.EditProps = value;
            }
        }

        public override List<PropertyControlViewModel> PropertyControls
        {
            get => Container == null ? new List<PropertyControlViewModel>() : Container.PropertyControls;

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

                //uopProperties eprops = Container.EditProps;
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


                uopProperties changes = Container.GetEditedProperties();
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
            if (Container == null) return;
            List<PropertyControlViewModel> pcontrols = Container.PropertyControls;
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
            if (Container != null) Container.DefineLimits();

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
            if (!Activated || Container == null)
                return "";

            return Container.GetErrorString(aPropertyName);


        }
        private void Validate_ShowErrors()
        {
            if (Container != null) ErrorCollection = Container.ErrorCollection;

            if (ErrorCollection == null) { ErrorMessage = ""; return; }
            if (ErrorCollection.Count <= 0) { ErrorMessage = ""; return; }
            string msg = ErrorCollection[0].Item2;
            ErrorMessage = msg;
        }

        private string ValidateInput(out PropertyControlViewModel rErrorControl)
        {
            rErrorControl = null;
            if (!Activated || Container == null)
                return "";
            return Container.ValidateInput(out rErrorControl);

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

