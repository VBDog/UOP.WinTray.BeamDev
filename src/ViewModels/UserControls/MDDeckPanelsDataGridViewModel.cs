using MvvmDialogs;
using System;
using System.Collections.ObjectModel;
using UOP.WinTray.Projects;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Parts;
using UOP.WinTray.Projects.Tables;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.UI.Events.EventAggregator;
using UOP.WinTray.UI.Messages;
using UOP.WinTray.UI.Model;

namespace UOP.WinTray.UI.ViewModels
{
    /// <summary>
    /// View Model Class for Deck Panels Data Grid of the MD Spout Project Form.
    /// </summary>
    public class MDDeckPanelsDataGridViewModel : MDProjectViewModelBase,
                                                             IEventSubscriber<Message_RefreshDataGrid>,
                                                            IEventSubscriber<Message_HighlightRowOnImageClick>,
                                                              IEventSubscriber<Message_UnloadProject>,
                                                            IEventSubscriber<Message_ClearData>

    {
        #region Variables
        private const string REFRESHING = "Refreshing Deck Section Grid";
        private const string RED = "Red";

        public event EventHandler DeckPanelChanged;

        #endregion

        #region Constructor

        internal MDDeckPanelsDataGridViewModel(IEventAggregator eventAggregator, IDialogService dialogService) : base(dialogService: dialogService, eventAggregator: eventAggregator) { }

        #endregion

        #region Properties

        public uppPartTypes PartType => uppPartTypes.DeckPanels;
        /// <summary>
        ///  Selected Deck Panels.
        /// </summary>

        private DeckPanels _SelectedItem;
        public DeckPanels SelectedItem
        {
            get => _SelectedItem;
            set { _SelectedItem = value; NotifyPropertyChanged("SelectedItem"); }
        }

        private ObservableCollection<DeckPanels> _GridData;
        public ObservableCollection<DeckPanels> GridData
        {
            get => _GridData;
            set { _GridData = value; NotifyPropertyChanged("GridData"); }
        }


        #endregion Properties

        #region Event Handlers
        /// <summary>
        ///  Event handler to clear all data
        /// </summary>
        /// <param name="message"></param>
        public void OnAggregateEvent(Message_ClearData message)
        {
            if (message == null) return;
            if (message.PartType == PartType || message.PartType == uppPartTypes.Undefined) GridData = new ObservableCollection<DeckPanels>();
        }

        public void OnAggregateEvent(Message_ProjectChange message)
        {
            if (message.Project == null)
            {
                MDProject = null;
                return;
            }
            if (message.Project.ProjectFamily == uppProjectFamilies.uopFamMD)
            {
                MDProject = (mdProject)message.Project;
            }
        }

        /// <summary>
        ///  Event handler called when the user changes from onr Tray Range to another
        /// </summary>
        /// <param name="message"></param>
        public void OnAggregateEvent(Message_RefreshDataGrid message)
        {
            if (message.PartType == uppPartTypes.Undefined || message.PartType == PartType)
            {
                MDProject = message.MDProject;
                if (message.DataTable != null)
                {
                    DisplayUnits = message.UnitsToDisplay;
                    GridData = DefineByTable(message.DataTable);
                    return;
                }
                //string action = (message.Clear) ? "Clearing" : "Refreshing";
                //WinTrayMainViewModel mainVM = (message.MainVM == null) ? WinTrayMainViewModel.WinTrayMainViewModelObj : message.MainVM ;
                //if (message.MDProject != null) MDProject = message.MDProject;
                //string wuz = mainVM.StatusMessage1;
                //mainVM.SetStatusMessages(wuz, $"{action} Grid - {PartType.GetDescription()}");
                //DisplayUnits = message.UnitsToDisplay;
                //GridData = (!message.Clear) ? CurrentDeckPanels() : new ObservableCollection<DeckPanels>();
                //mainVM.SetStatusMessages(wuz, "");

            }
        }
        /// <summary>
        /// Clean up
        /// </summary>
        /// <param name="message"></param>
        public override void OnAggregateEvent(Message_UnloadProject message)
        {
            if (GridData != null) GridData.Clear();

        }
        /// <summary>
        /// Downcomer Changed functionality
        /// </summary>
        /// <param name="message"></param>
        public void OnAggregateEvent(Message_HighlightRowOnImageClick message)
        {
            OnPanelChange(message.PanelIndex);

        }
        /// <summary>
        /// On Downcomer Changed functionality
        /// </summary>
        /// <param name="index"></param>
        public void OnPanelChange(int index)
        {
            if (DeckPanelChanged != null)
            {
                DeckPanelChanged.Invoke(index, null);
            }
        }

        /// <summary>
        /// Panel Selection Changed
        /// </summary>
        /// <param name="index"></param>
        public void SelectionChanged(int index)
        {

            if (index >= 0) EventAggregator.Publish<Message_HighlightImageFromGridClick>(new Message_HighlightImageFromGridClick(index, bIsPanel: true, aMDProject: MDProject));
        }

        #endregion Event Handlers

        #region Methods

        /// <summary>
        /// Method to convert deckpanel values with selected united.
        /// </summary>
        private ObservableCollection<DeckPanels> CurrentDeckPanels() => DefineByTable(mdUtils.GetDisplayTableProperties(MDProject, aTableType: uppDisplayTableTypes.DeckPanelsProperties));



        private ObservableCollection<DeckPanels> DefineByTable(uopTable aTable)
        {
            ObservableCollection<DeckPanels> _rVal = new();

            if (aTable == null) return _rVal;

            uppUnitFamilies unit = DisplayUnits;
            int rows = aTable.Rows;

            try
            {

                uopProperties row;
                for (int i = 1; i <= rows; i++)
                {
                    var panel = new DeckPanels();

                    row = aTable.PropertyRow(i);

                    //dc = DCs.Item(i);
                    if (aTable.ProjectType == uppProjectTypes.MDSpout)
                    {
                        panel.No = new CellInfo(row.Item("No", true), unit);
                        panel.FBA = new CellInfo(row.Item("FBA", true), unit);
                        panel.WL = new CellInfo(row.Item("FBA/WL", true), unit, aOverridePrecision: 3);

                        panel.VolumeErr = new CellInfo(row.Item("V/L Err", true), unit, aOverridePrecision: 3);
                        panel.IdealArea = new CellInfo(row.Item("Ideal Area", true), unit);
                        panel.ActualArea = new CellInfo(row.Item("Actual Area", true), unit);
                        panel.Err = new CellInfo(row.Item("Err", true), unit, bZerosAsNullString: false);

                    }
                    else if (aTable.ProjectType == uppProjectTypes.MDDraw)
                    {
                        panel.PN = new CellInfo(row.Item("PN", true), unit);
                        panel.Width = new CellInfo(row.Item("Width", true), unit);
                        panel.Height = new CellInfo(row.Item("Height", true), unit);
                        panel.Mnwy = new CellInfo(row.Item("Is Manway", true), unit);
                    }


                    _rVal.Add(panel);
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
                    if (aTable.SelectedRow > 0) SelectedItem = _rVal[aTable.SelectedRow - 1];
                }
            }
        }


        #endregion Methods
    }
}
