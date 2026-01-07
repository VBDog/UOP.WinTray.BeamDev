using UOP.WinTray.Projects;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Tables;
using UOP.WinTray.Projects.Parts;
using UOP.WinTray.UI.Events.EventAggregator;
using UOP.WinTray.UI.Messages;
using UOP.WinTray.UI.Model;
using UOP.WinTray.UI.Commands;
using MvvmDialogs;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace UOP.WinTray.UI.ViewModels
{
    /// <summary>
    /// View Model Class for Downcomers Data Grid of the MD Spout Project Form.
    /// </summary>
    public class MDDowncomersDataGridViewModel : MDProjectViewModelBase,
                                        IEventSubscriber<Message_RefreshDataGrid>,
                                        IEventSubscriber<Message_HighlightRowOnImageClick>,
                                        IEventSubscriber<Message_UnloadProject>,
                                       IEventSubscriber<Message_ClearData>
    {
        #region Constant

        private const string REFRESHING = "Refreshing Downcomer Grid Data";
        private const string RED = "Red";
        #endregion

       
        public event EventHandler DowncomerChanged;


        #region Constructors

        internal MDDowncomersDataGridViewModel(IEventAggregator eventAggregator, IDialogService dialogService) : base(dialogService: dialogService, eventAggregator: eventAggregator) { }


        #endregion Constructors

        #region Properties

        public uppPartTypes PartType => uppPartTypes.Downcomers;

        /// <summary>
        /// Selected downcomer
        /// </summary>
        private Downcomers _SelectedItem; 
        public Downcomers SelectedItem
        {
            get => this._SelectedItem;
            set { _SelectedItem = value; NotifyPropertyChanged("SelectedItem"); }
        }

        private ObservableCollection<Downcomers> _GridData;
        public ObservableCollection<Downcomers> GridData
        {
            get => _GridData; 
            set { _GridData = value; NotifyPropertyChanged("GridData"); }
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

      

        #endregion

        #region Event Handlers


        public void SelectionChanged(int index)
        {
            var project = MDProject;
            if (index > 0)
                EventAggregator.Publish<Message_HighlightImageFromGridClick>(new Message_HighlightImageFromGridClick(aIndex: index, bIsDowncomer: true, aMDProject: project));

        }

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
        /// OnAggregateEvent for Downcomer changes
        /// </summary>
        /// <param name="message"></param>
        public void OnAggregateEvent(Message_HighlightRowOnImageClick message) { OnDowncomerChanged(message.DowncomerIndex); }

        /// <summary>
        /// On Downcomer Changed
        /// </summary>
        /// <param name="index"></param>
        public void OnDowncomerChanged(int index) => DowncomerChanged?.Invoke(index, null);

        /// <summary>
        /// Clean up
        /// </summary>
        /// <param name="message"></param>
        public override void OnAggregateEvent(Message_UnloadProject message) { if (GridData != null) GridData.Clear(); }

        /// <summary>
        ///  Event handler called when edit operation is completed
        /// </summary>
        /// <param name="message"></param>
        public void OnAggregateEvent(Message_RefreshDataGrid message)
        {

            if (message.PartType == uppPartTypes.Undefined || message.PartType == PartType )
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
                //mainVM.SetStatusMessages(wuz, $"{action} Grid - {PartType.GetDescription()}");
                //DisplayUnits = message.UnitsToDisplay;
                //GridData = (!message.Clear) ? CurrentDowncomers() : new ObservableCollection<Downcomers>();
              
           
                //mainVM.SetStatusMessages(wuz, "");
            }
            

        }

        public void OnAggregateEvent(Message_ClearData message)
        {
            if (message == null) return;
            if (message.PartType == PartType || message.PartType == uppPartTypes.Undefined) GridData = new ObservableCollection<Downcomers>();
        }

        #endregion Event Handlers

        #region Methods

        private ObservableCollection<Downcomers> DefineByTable(uopTable aTable)
        {
            ObservableCollection<Downcomers> _rVal = new();

            if (aTable == null) return _rVal;

            uppUnitFamilies unit = DisplayUnits;
            int rows = aTable.Rows;

            try
            {

                uopProperties row;
                for (int i = 1; i <= rows; i++)
                {
                    var downcomer = new Downcomers() { Index = i };
                    row = aTable.PropertyRow(i);

                    //dc = DCs.Item(i);

                    bool noerrors = !uopUtils.RunningInIDE;
                    if(aTable.ProjectType == uppProjectTypes.MDSpout)
                    {
                        downcomer.No = new CellInfo(row.Item("No", noerrors), unit);
                        downcomer.WeirLength = new CellInfo(row.Item("Weir Length", noerrors), unit);
                        downcomer.IdealArea = new CellInfo(row.Item("Ideal Area", noerrors), unit);
                        downcomer.ActualArea = new CellInfo(row.Item("Actual Area", noerrors), unit);
                        downcomer.Err = new CellInfo(row.Item("Err", noerrors), unit, bZerosAsNullString: false);
                    }
                    else if (aTable.ProjectType == uppProjectTypes.MDDraw)
                    {
                        // Added by CADfx
                        downcomer.PN = new CellInfo(row.Item("PN", noerrors), unit);
                        downcomer.FldWeir = new CellInfo(row.Item("Fld. Weir", noerrors), unit);
                        downcomer.BoxLength = new CellInfo(row.Item("Box Lengh", noerrors), unit);
                        downcomer.SuplDefl = new CellInfo(row.Item("Supl. Defl.", noerrors), unit);
                        downcomer.Gussets = new CellInfo(row.Item("Gussets", noerrors), unit, bYesNoForBool: true);

                        // Added by CADfx
                    }



                    _rVal.Add(downcomer);
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

        /// <summary>
        /// Edit downcomer 
        /// </summary>
        /// <param name="index"></param>
        private void Execute_Edit(dynamic index)
        {
            MDProjectViewModel parent = (MDProjectViewModel)ParentVM;
            mdTrayAssembly assy = parent?.MDAssy;
            Downcomers si = SelectedItem;

            if (parent == null || assy == null || si == null) return;
            int idx = 0;
            if (si.No != null)
            {
                string dcname =si.No.Value.ToString();

                idx = mzUtils.LeadingInteger(dcname);
            }
            else if(si.PN!=null){
                string dcname = si.PN.Value.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(dcname))
                {
                    idx = mzUtils.VarToInteger(dcname.Substring(1,1));
                }
            }


            if (idx <= 0) return;
            assy.Downcomers.SelectedIndex = idx;
            parent.Edit_SelectedDowncomer();

            
        }

        /// <summary>
        /// Method to convert downcomer values with selected united.
        /// </summary>
        private ObservableCollection<Downcomers> CurrentDowncomers() => DefineByTable(mdUtils.GetDisplayTableProperties(MDProject, aTableType: uppDisplayTableTypes.DowncomerProperties));
      




        #endregion Methods
    }
}
