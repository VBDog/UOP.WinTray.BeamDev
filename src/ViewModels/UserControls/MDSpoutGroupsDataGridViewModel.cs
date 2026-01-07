using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using UOP.WinTray.Projects;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Tables;
using UOP.WinTray.Projects.Parts;
using UOP.WinTray.UI.Events.EventAggregator;
using UOP.WinTray.UI.Messages;
using UOP.WinTray.UI.Commands;
using UOP.WinTray.UI.Model;
using MvvmDialogs;


namespace UOP.WinTray.UI.ViewModels
{
    /// <summary>
    /// View Model Class for Spout Groups Data Grid of the MD Spout Project Form
    /// </summary>
    public class MDSpoutGroupsDataGridViewModel : MDProjectViewModelBase,
                                                                IEventSubscriber<Message_RefreshDataGrid>,
                                                                IEventSubscriber<Message_HighlightRowOnImageClick>,
                                                                IEventSubscriber<Message_UnloadProject>, 
                                                                IEventSubscriber<Message_ClearData>

    {
        #region Constants

        private const string REFRESHING = "Refreshing Spout Group Data";
        private const string RED = "Red";
        private const string BLUE = "Blue";
        #endregion

        #region Variables

       
        public event EventHandler DowncomerChanged;


        #endregion Variables

        #region Properties
        public uppPartTypes PartType => uppPartTypes.SpoutGroups;

        private SpoutGroup _SelectedItem;
        public SpoutGroup SelectedItem
        {
            get => _SelectedItem;
            set { _SelectedItem = value; NotifyPropertyChanged("SelectedItem"); }
        }

        private ObservableCollection<SpoutGroup> _GridData;
        public ObservableCollection<SpoutGroup> GridData
        {
            get =>_GridData;
            set { _GridData = value; NotifyPropertyChanged("GridData"); }
        }

        #endregion Properties

        #region Constructor

        internal MDSpoutGroupsDataGridViewModel(IEventAggregator eventAggregator, IDialogService dialogService) : base(dialogService: dialogService,  eventAggregator: eventAggregator)
        {
           
            _GridData = new ObservableCollection<SpoutGroup>();
        }

        #endregion

        #region Event Handlers

        public void OnAggregateEvent(Message_ProjectChange message)
        {
            if (message.Project == null)
            {
                MDProject = null;
                ReleaseReferences();
                return;
            }
            if (message.Project.ProjectFamily == uppProjectFamilies.uopFamMD)
            {
                MDProject = (mdProject)message.Project;
            }
        }

        /// <summary>
        /// Event handler called when edit operation completed message
        /// </summary>
        /// <param name="message"></param>
        public void OnAggregateEvent(Message_RefreshDataGrid message)
        {
            if(message.PartType == uppPartTypes.Undefined || message.PartType == PartType)
            {
                MDProject = message.MDProject;
                if (message.DataTable != null)
                {
                    DisplayUnits = message.UnitsToDisplay;
                    GridData = DefineByTable(message.DataTable);
                    return;
                }
                
                //string action = (message.Clear) ? "Clearing" : "Refreshing";
                //WinTrayMainViewModel mainVM = (message.MainVM == null) ? WinTrayMainViewModel.WinTrayMainViewModelObj : message.MainVM;
                //if (message.MDProject != null) MDProject = message.MDProject;
                //string wuz = mainVM.StatusMessage1;
                //mainVM.SetStatusMessages(wuz, $"{ action} Grid - { PartType.GetDescription()}");
                //DisplayUnits = message.UnitsToDisplay;
                //GridData = (!message.Clear) ? CurrentSpoutGroups() : new ObservableCollection<SpoutGroup>();
                //mainVM.SetStatusMessages(wuz, "");


            }


        }

        private DelegateCommand _CMD_Edit;
        public ICommand Command_Edit
        {
            get
            {
                if (_CMD_Edit == null) _CMD_Edit = new DelegateCommand(param => Execute_Edit((dynamic)param));
                return _CMD_Edit;
            }
        }

        /// <summary>
        /// event for Downcomer Changed functionalty
        /// </summary>
        /// <param name="message"></param>
        public void OnAggregateEvent(Message_HighlightRowOnImageClick message)
        {
            OnDowncomerChanged(message.SpoutIndex);
        }

        /// <summary>
        /// Clean up
        /// </summary>
        /// <param name="message"></param>
        public override void OnAggregateEvent(Message_UnloadProject message)
        {
            if (GridData != null) GridData.Clear();
          
        }

        public void OnAggregateEvent(Message_ClearData message)
        {
            if (message == null) return;
            if (message.PartType == PartType || message.PartType == uppPartTypes.Undefined) GridData = new ObservableCollection<SpoutGroup>();
        }


        /// <summary>
        /// Downcomer Changed functionalty
        /// </summary>
        /// <param name="index"></param>
        public void OnDowncomerChanged(int index) => DowncomerChanged?.Invoke(index, null);

        /// <summary>
        /// SpoutS selection changed
        /// </summary>
        /// <param name="index"></param>
        public void SelectionChanged(int index)
        {
            if (index > 0) EventAggregator.Publish<Message_HighlightImageFromGridClick>(new Message_HighlightImageFromGridClick(aIndex: index, bIsSpoutGroup: true, aMDProject: MDProject));

        }

        public void SelectionChanged(string aHandle)
        {
            if(string.IsNullOrWhiteSpace(aHandle) || MDAssy == null) return;
            if (aHandle.IndexOf(',') <0) return;
            mdSpoutGroup sg = MDAssy.SpoutGroups.GetByHandle(aHandle, null, out int idx);
            if(sg == null) return;
            MDAssy.SpoutGroups.SetSelected(sg);
            EventAggregator.Publish<Message_HighlightImageFromGridClick>(new Message_HighlightImageFromGridClick(aIndex: idx, bIsSpoutGroup: true, aMDProject: MDProject));

        }
        #endregion

        #region Methods

        /// <summary>
        /// pops up MDSpout properties edit screens
        /// </summary>
        /// <param name="param"></param>
        private void Execute_Edit(SpoutGroup param)
        {
            if (null != param)
            {

                MDProjectViewModel parent = (MDProjectViewModel)ParentVM;
                if (parent == null) return;
                mdTrayAssembly assy = parent.MDAssy;
                if (assy == null) return;
                assy.SpoutGroups.SelectedIndex = param.Index;
                parent.Edit_SelectedSpoutGroup();

            }
        }

        private uopTable _DataTable;
        public uopTable DataTable 
        { 
            get => _DataTable;
            set { _DataTable = value; }
        }

     
        /// <summary>
        /// Method to convert spout group values with selected united.
        /// </summary>
        private ObservableCollection<SpoutGroup> CurrentSpoutGroups()=> DefineByTable(mdUtils.GetDisplayTableProperties(MDProject, uppDisplayTableTypes.SpoutGroupsProperties, aUnits: DisplayUnits));
        

        private ObservableCollection<SpoutGroup> DefineByTable(uopTable aTable)
        {
            ObservableCollection<SpoutGroup> _rVal = new();

            if (aTable == null) return _rVal;

            uppUnitFamilies unit = DisplayUnits;
            int rows = aTable.Rows;
           
            try
            {

                uopProperties row;
            
                for (int i = 1; i <= rows; i++)
                {


                    row = aTable.PropertyRow(i);


                    //var item = _GridData[i - 1];
                    SpoutGroup spoutGroup = new();
                    spoutGroup.Index = i;

                    spoutGroup.No = new CellInfo(row.Item("No."), unit);
                    spoutGroup.Handle = new CellInfo(row.Item("Handle"), unit);
                    spoutGroup.SpoutCount = new CellInfo(row.Item("Spout Count"), unit);
                    spoutGroup.PatType = new CellInfo(row.Item("Pat. Type"), unit);
                    spoutGroup.SpoutLength = new CellInfo(row.Item("Spout Length"), unit);
                    spoutGroup.SPR = new CellInfo(row.Item("Spouts Per Row"), unit);
                    spoutGroup.VertPitch = new CellInfo(row.Item("Vertical Pitch"), unit);
                    spoutGroup.PatLength = new CellInfo(row.Item("Pat. Length"), unit);
                    spoutGroup.ActualMargin = new CellInfo(row.Item("Actual Margin"), unit);
                    spoutGroup.ActualArea = new CellInfo(row.Item("Actual Area"), unit);
                    spoutGroup.IdealArea = new CellInfo(row.Item("Ideal Area"), unit);

                    spoutGroup.Err = new CellInfo(row.Item("Err"), unit,  bZerosAsNullString: false);
                    _rVal.Add(spoutGroup);


                }
                return _rVal;
            }
            catch
            {
                return _rVal;
            }
            finally
            {
                if (rows > 0)
                {
                    if (aTable.SelectedRow > 0 && _rVal.Count > 0)
                    {
                        if (aTable.SelectedRow > _rVal.Count) aTable.SelectedRow = _rVal.Count;

                            SelectedItem = _rVal[aTable.SelectedRow - 1];
                    }
                }
            }
        }

        #endregion
    }

}
