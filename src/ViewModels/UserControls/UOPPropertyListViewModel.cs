using UOP.WinTray.Projects.Enums;
using UOP.WinTray.UI.Events.EventAggregator;
using UOP.WinTray.UI.Messages;
using MvvmDialogs;
using UOP.WinTray.UI.Behaviors;
using System.Windows.Controls;
using System.Collections.Generic;
using System;
using UOP.WinTray.Projects;

namespace UOP.WinTray.UI.ViewModels
{
    /// <summary>
    /// ViewModel for TrayRange properties
    /// </summary>
    public class UOPPropertyListViewModel : ViewModel_Base,
                                    IEventSubscriber<Message_RefreshControls>,
                                    IEventSubscriber<Message_UpdatePropertyList>,
                                    IEventSubscriber<Message_ClearData>
    {



        #region Constructors

        internal UOPPropertyListViewModel(IEventAggregator eventAggregator, IDialogService dialogService) : base( eventAggregator,dialogService)
        {
            EventAggregator?.Subscribe(this);
            Properties = new uopProperties();
        }

        #endregion Constructors

        #region Properties

        private uppPartTypes _PartType;
        public uppPartTypes PartType
        {
            get => _PartType;  
            set => _PartType = value; 
        }

        public uopProperties Properties
        {
            get => EditProps; 
            set { EditProps = value;  NotifyPropertyChanged("Properties"); }
        }

      
        public override uppUnitFamilies DisplayUnits { 
            get => base.DisplayUnits;
            set { base.DisplayUnits = value;  } 
        }

        /// <summary>
        /// The control that contains the Property values
        /// </summary>
        
        private WeakReference<ListView> _RefListView;
        public ListView PropertyListView 
        {
            get { if (_RefListView == null) return null; ListView _rVal = null; if (!_RefListView.TryGetTarget(out _rVal)) { _RefListView = null; return null; } else return _rVal; }
            set { if (value == null) { _RefListView = null; } else { _RefListView = new WeakReference<ListView>(value); } }
        }

        #endregion Properties

        #region Event Handlers
        /// <summary>
        ///  Event handler to clear all data
        /// </summary>
        /// <param name="message"></param>
        public void OnAggregateEvent(Message_ClearData message)
        { 
            if(message == null) return;
            if(message.PartType == PartType || message.PartType == uppPartTypes.Undefined) Properties = new uopProperties();
        }

        /// <summary>
        ///  Event handler to update the column widths
        /// </summary>
        /// <param name="message"></param>
        public void OnAggregateEvent(Message_RefreshControls message) => RefreshColumnWidths();

        /// <summary>
        ///  Event handler to update the property content
        /// </summary>
        /// <param name="message"></param>
        public void OnAggregateEvent(Message_UpdatePropertyList message)
        {
            DisplayUnits = message.Units;
            if (message.Properties != null)
            {
                if(message.PartType == PartType)
                    Properties = message.Properties;
            }
            else
            {
                if (message.PartType == uppPartTypes.Undefined || message.PartType == PartType)
                    Properties = message.GetProperties(PartType);
            }
        }

        #endregion Event Handlers

        #region Methods      

        public void RefreshColumnWidths() => ListViewBehaviors.UpdateColumnWidths(PropertyListView, new List<string>() { "Key" }); 
   
        
        #endregion
    }
}
