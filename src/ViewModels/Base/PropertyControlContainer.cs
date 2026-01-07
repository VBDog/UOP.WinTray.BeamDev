using System;
using System.Collections.Generic;

namespace UOP.WinTray.UI.ViewModels
{
    public abstract class PropertyControlContainer : ViewModel_Base
    {
        #region Events

        public delegate void DoubleClickEventHandler(PropertyControlViewModel aProperty);
        public event DoubleClickEventHandler eventDoubleClickEventHandler;

        public delegate void ChoiceValueChangeEventHandler(PropertyControlViewModel aProperty);
        public event ChoiceValueChangeEventHandler eventChoiceValueChangeEventHandler;
        
        public delegate void YesNoValueChangeEventHandler(PropertyControlViewModel aProperty);
        public event YesNoValueChangeEventHandler eventYesNoValueChangeEventHandler;

        #endregion Events

        #region Constructors
        public PropertyControlContainer() : base()
        {
            Controls.Clear();


        }

        #endregion Constructors

        #region Properties

        private string _SelectedControlName = "";
        public string SelectedControlName { get => _SelectedControlName; set => _SelectedControlName = value; }

        private readonly List<WeakReference<PropertyControlViewModel>> _Controls = new();
        public List<WeakReference<PropertyControlViewModel>> Controls
        {
            get => _Controls;

        }

        #endregion Properties

        #region Methods

        public void RespondToPropertyDoubleClick(PropertyControlViewModel aPropertyControl)
        {
            eventDoubleClickEventHandler?.Invoke(aPropertyControl);
        }

     

        public void RespondToChoiceValueChange(PropertyControlViewModel aPropertyControl)
        {
            eventChoiceValueChangeEventHandler?.Invoke(aPropertyControl);
        }
        public void RespondToYesNoValueChange(PropertyControlViewModel aPropertyControl)
        {
            eventYesNoValueChangeEventHandler?.Invoke(aPropertyControl);
        }
        public void RegisterPropertyControl(PropertyControlViewModel aPropertyControl)
        {
            if (aPropertyControl != null)
            {
                aPropertyControl.eventDoubleClickEventHandler += RespondToPropertyDoubleClick;
                aPropertyControl.eventChoiceValueChangeEventHandler += RespondToChoiceValueChange;
                Controls.Add(new WeakReference<PropertyControlViewModel>(aPropertyControl));
            }
        }


        #region Validation of fields 

        public abstract void DefineLimits();
        

        public abstract string GetErrorString(string aPropertyName);

        public abstract string ValidateInput(out PropertyControlViewModel rErrorControl);


        #endregion Validation of fields
        #endregion Methods
    }
}
