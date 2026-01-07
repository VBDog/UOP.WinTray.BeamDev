
using UOP.WinTray.UI.BusinessLogic;
using UOP.WinTray.UI.Events.EventAggregator;
using UOP.WinTray.UI.Interfaces;
using UOP.WinTray.UI.Messages;
using UOP.WinTray.UI.Model;
using UOP.WinTray.UI.Utilities.Constants;
using UOP.WinTray.UI.Views;
using UOP.WinTray.UI.Views.Windows;
using MvvmDialogs;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Unity.Resolution;
using UOP.WinTray.Projects;
using UOP.WinTray.Projects.Documents;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Parts;
using UOP.WinTray.Projects.Utilities;
using System;
using System.Reflection;
using System.Windows.Forms;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Vml.Spreadsheet;

namespace UOP.WinTray.UI.ViewModels
{
    /// <summary>
    /// UI interaction logic for Win Tray Main View.
    /// </summary>
    public class WinTrayMainViewModel : ViewModel_Base,
                                        IEventSubscriber<Message_SetStatusMessages>,
                                        IEventSubscriber<Message_UpdateTreeView>,
                                        IEventSubscriber<Message_Refresh>,
                                        IEventSubscriber<Message_UpdatePropertyList>,
                                        IEventSubscriber<Message_ToggleAppEnabled>,
                                        IEventSubscriber<Message_DocumentRequest>,
                                        IEventSubscriber<Message_DocmentRequestComplete>,
                                        IEventSubscriber<Message_ShowWarnings>,
                                        IEventSubscriber<Message_PromptForRangeInput>,
                                        IEventSubscriber<Message_ClearData>
    {



        private static WinTrayMainViewModel _WinTrayMainViewModel;

        private readonly BackgroundWorker _Worker_Warnings = null;

        private readonly BackgroundWorker _Worker_Reports = null;

        private readonly BackgroundWorker _Worker_Revisions = null;
        //private object _CurrentViewModel;

        #region Constructors

        public WinTrayMainViewModel()
        {


            Mouse.OverrideCursor =  System.Windows.Input.Cursors.Wait;
            Utilities.Logger.SetDefaultLocation();

            //NewMDProjectViewModel = new NewMDProjectViewModel(this);
            //winTrayMainCurrentProjectViewModel = new WinTrayMainCurrentProjectViewModel(this);
            // default visibility for MD Spout form.
            VisibilityMDProject = Visibility.Hidden;
            Mouse.OverrideCursor = null;
            Title = "UOP WinTray";
            DialogService = ApplicationModule.Instance.Resolve<IDialogService>();
            EventAggregator = ApplicationModule.Instance.Resolve<IEventAggregator>();
            _StatusMessage1 = "";
            _StatusMessage2 = "";
            MenuItemViewModelHelper = new MenuItemViewModelHelper(this, ApplicationModule.Instance.Resolve<IDialogService>());
            MenuItemViewModelHelper.LoadMenuItem();

             _Worker_Reports = new BackgroundWorker();
            _Worker_Reports.DoWork += DoWork_GenerateReport;
            _Worker_Reports.RunWorkerCompleted += DoWork_GenerateReport_Complete;


            _Worker_Revisions = new BackgroundWorker();
            _Worker_Revisions.DoWork += DoWork_MarkRevions;
            _Worker_Revisions.RunWorkerCompleted += DoWork_MarkRevions_Complete;

            _Worker_Warnings = new BackgroundWorker();
            _Worker_Warnings.DoWork += DoWork_GetProjectWarnings;
            _Worker_Warnings.RunWorkerCompleted += DoWork_GetProjectWarnings_Complete;

            IsEnabled = true;

        }

        #endregion Constructors

        #region Worker Code
        private void DoWork_GetProjectWarnings(object sender, DoWorkEventArgs e)
        {
            Message_DocumentRequest request = (Message_DocumentRequest)e.Argument;
            if (request == null) return;
            request.Documents = new uopDocuments();
            if (request.Project == null) return;
            try
            {
                if(string.IsNullOrWhiteSpace(request.RangeGUID))
                {
                    request.ToggleAppEnabled(false, $"Generating Project Warnings");
                    request.Documents = request.Project.Warnings();

                }
                else
                {
                    uopTrayRange range = request.Project.TrayRanges.GetByGuid(request.RangeGUID);
                    if (range == null) return;
                    request.ToggleAppEnabled(false, $"Generating {range.TrayName(true)} Warnings");
                    request.Documents = range.GenerateWarnings( request.Project);

                }

            }
            catch
            {

            }
            finally
            {
                e.Result = request;
            }

        }

        private void DoWork_GetProjectWarnings_Complete(object sender, RunWorkerCompletedEventArgs e)
        {
            Message_DocumentRequest request = (Message_DocumentRequest)e.Result;

            request.ToggleAppEnabled(true);
            request.SetStatusMessages("", "");
            if (request == null) return;

            uopProject project = request.Project;
            request.Project = null;
            if (project == null || request.Documents == null) return;
            if (request.Documents.Count <= 0)
            {
                if(!string.IsNullOrWhiteSpace(request.RangeGUID))
                {
                    DialogService.ShowMessageBox(this, $" The Active Tray Range has no defined warnings", "No Warnings Found");
                }
                else
                {
                    DialogService.ShowMessageBox(this, $"Project {project.Name} has no defined warnings", "No Warnings Found");
                }
                return;
            }
            var inputParams = new ParameterOverride[] { new ParameterOverride("project", project), new ParameterOverride("warnings", request.Documents), new ParameterOverride("dialogService", DialogService) };

            WarningViewModel VM = ApplicationModule.Instance.Resolve<WarningViewModel>(inputParams);
            DialogService.ShowDialog<WarningView>(this, VM);

            
        }

        private void DoWork_GenerateReport(object sender, DoWorkEventArgs e)
        {
            Message_DocumentRequest request = (Message_DocumentRequest)e.Argument;
            if (request == null) return;
            try
            {
               
                ReportWriter writer = (ReportWriter)request.ReportWriter;
                if (writer == null) return;
                request.Document ??= writer.Report;
                request.ToggleAppEnabled(false, $"Generating Report - {writer.Report.ReportName }");
                string err = writer.GenerateReport(writer.Report);
                if (!string.IsNullOrEmpty(err))
                {
                    writer.Report.AddWarning(null, "Report Generation Error", err, uppWarningTypes.General, this.GetType().Name);
                }
            }
            catch
            {

            } finally
            {
                e.Result = request;
            }

        }

        private void DoWork_GenerateReport_Complete(object sender, RunWorkerCompletedEventArgs e)
        {
            Message_DocumentRequest request = (Message_DocumentRequest)e.Result;

            request.ToggleAppEnabled(true);
            request.SetStatusMessages("", "");
            if (request == null) return;
            ReportWriter writer = (ReportWriter)request.ReportWriter;
            if (writer == null) return;
            request.Document ??= writer.Report;

            EventAggregator.Publish<Message_DocmentRequestComplete>(new Message_DocmentRequestComplete(request));

        }

        private void DoWork_MarkRevions(object sender, DoWorkEventArgs e) 
        {
            Message_DocumentRequest request = (Message_DocumentRequest)e.Argument;

            try
            {

                IReportWriter writer = request.ReportWriter;
                if (writer == null) return;

                request.ToggleAppEnabled(false, $"Marking Report Revisions - {writer.Report.ReportName } ");
                writer.MarkRevisions(writer.Report);
            }
            catch
            {

            }
            finally
            {
                e.Result = request;
            }
        }
        private void DoWork_MarkRevions_Complete(object sender, RunWorkerCompletedEventArgs e)
        {
            Message_DocumentRequest request = (Message_DocumentRequest)e.Result;
            if (request == null) return;
            request.ToggleAppEnabled(true);
            request.SetStatusMessages("", "");

            ReportWriter writer = (ReportWriter)request.ReportWriter;

            if (writer.Warnings.Count > 0)
            {
                ShowWarnings(new uopDocuments(writer.Warnings));
            }


            if (request.FileToOpen != "")
            {
                if (File.Exists(request.FileToOpen))
                {
                    if (ShowMessageBox($"Open {request.DocumentType.Description() } File '{request.FileToOpen}' ?", $"{request.DocumentType.Description()} Complete", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes)
                    {
                        uopUtils.OpenFileInSystemApp(request.FileToOpen);

                    }

                }

            }

        }


        #endregion Worker Code

        #region Event Handlers


       
        /// <summary>
        /// Sets the enabled value of the main UI  
        /// </summary>
        /// <param name="message"></param>
        public void OnAggregateEvent(Message_ToggleAppEnabled message) => Execute_ToggleAppEnabled(message);
        
        public void OnAggregateEvent(Message_SetStatusMessages message)
        {

        
            if (message.AppEnable.HasValue)
            {
                Execute_ToggleAppEnabled(message.AppEnable.Value,  StatusMessage1, message.StatusMessage2);

            }
            else
            {
                Execute_SetStatusMessages(message);
             
            }
        }

        public void OnAggregateEvent(Message_DocmentRequestComplete message) 
        {

            Message_DocumentRequest request = message.Request;
            
            if (request == null) return;
            uopDocument doc = request.Document;

            if (doc == null) return;

         

            if (doc.Warnings.Count > 0)
            {
                ShowWarnings(new uopDocuments(doc.Warnings));
            }

            
            string doctype = doc.DocumentType.Description();
            string filetopen = "";
             
                       
            switch (request.DocumentType)
            {
                case uppDocumentTypes.Warning:
                    break;

                case uppDocumentTypes.Report:
                    {
                       
                        ReportWriter writer = (ReportWriter)request.ReportWriter;
                        uopDocReport report = writer.Report;
                        
                        if (writer.FatalError) return;


                        if (!writer.CanMarkRevision)
                        {
                            filetopen = writer.FileSpec;
                        }
                        else
                        {
                            if (ShowMessageBox($"Mark Revisions ?", "Report Complete", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.No) 
                            {
                                filetopen = writer.FileSpec;
                            }
                            else
                            {
                                request.FileToOpen = writer.FileSpec;
                            }


                        }


                        if (filetopen == "")
                        {
                            report.Project ??= Project;
                            writer.Project ??= Project;
                            bool async = true;

                            if (async)
                            {

                                _Worker_Revisions.RunWorkerAsync(request);
                            }
                            else
                            {

                                try
                                {


                                    DoWorkEventArgs e = new(request);
                                    DoWork_MarkRevions(null, e);
                                    RunWorkerCompletedEventArgs ec = new(e.Result, null, false);
                                    DoWork_MarkRevions_Complete(null, ec);

                                }
                                catch { }

                            }
                        }
                     
                    }

                   

                    break;
                default:
                    return;
            }


            if (filetopen != "")
            {
                if (File.Exists(filetopen))
                {
                    if (ShowMessageBox($"Open {doctype } File '{filetopen}' ?", $"{doctype} Complete", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes)
                    {
                        uopUtils.OpenFileInSystemApp(filetopen);

                    }

                }

            }


        }
        public void OnAggregateEvent(Message_PromptForRangeInput message)
        {
            if (message == null) return;
            if (message.RefreshMessage == null) return;
            mdProject mdproject = message.RefreshMessage.MDProject;
            if (mdproject == null) return;
            mdTrayRange lastmdrange = null;
            uppUnitFamilies units = mdproject.DisplayUnits;
            foreach (uopTrayRange range in mdproject.TrayRanges)
            {
             
                mdTrayRange mdrange = (mdTrayRange)range;
                string caption = $"Input Range Details for Stages {mdrange.StageList}";
                if (lastmdrange != null)
                {

                    mdrange.TrayAssembly.Beam.PropValsCopy(lastmdrange.TrayAssembly.Beam.ActiveProperties);
                }
                ParameterOverride[] inputParams = new ParameterOverride[]
                {
                         new ParameterOverride("project", mdproject),
                         new ParameterOverride("fieldName", "RingStart"),
                         new ParameterOverride("rangeGUID", range.GUID),
                         new ParameterOverride("caption", caption),
                         new ParameterOverride("allowLappingRingNumbers", true),
                         new ParameterOverride("isNewTray", true),
                         new ParameterOverride("unitsToDisplay", units),
                        };
                Edit_MDRange_ViewModel VM = ApplicationModule.Instance.Resolve<Edit_MDRange_ViewModel>(inputParams);
                bool? result = DialogService.ShowDialog<Edit_MDRange_View>(this, VM);
                units = VM.DisplayUnits;
                VM.ReleaseReferences();
                VM.Dispose();
                lastmdrange = mdrange;
            }

            MDProjectViewModel mdprojectVM = MDProjectViewModel;
            if (mdprojectVM != null)
            {
                mdprojectVM.SuppressEvents = true;
                mdprojectVM.MDProject = MDProject;
                VisibilityMDProject = mdproject.TrayRanges.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
                mdprojectVM.ParentVM = this;

                mdprojectVM.SuppressEvents = false;
            }

            VisibilityMDProject = Visibility.Visible;
            message.RefreshMessage.UpdateRangeList = true;
            message.RefreshMessage.Publish();
        }
        public async void OnAggregateEvent(Message_DocumentRequest request)
        {
            if (request == null || Project == null) return;
            switch (request.DocumentType)
            {
                case uppDocumentTypes.Warning:
                    ShowWarnings(request.Documents, request.RangeGUID);
                    break;

                case uppDocumentTypes.Report:
                    string err = "";
                    ReportWriter reportWriter = new(this);
                    bool doit = reportWriter.PreValidateReportWriter(out string brief, out err);
                   
                    if (!doit) 
                    { 
                        if(string.Compare(brief, "Unsaved Changes" , true) == 0) 
                        {
                           
                            if (ShowMessageBox("Unsaved Changes Detected. \n Select 'OK' to save the project and continue to the report generation screen", brief, MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
                            {
                                doit = await MenuItemViewModelHelper.Execute_SaveCurrentProjectAsyc();
                                
                            }
                        }
                        else
                        {
                            ShowMessageBox(err, brief, MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }

                    if (!doit) return;
                 
                    var inputParams2 = new ParameterOverride[] { new ParameterOverride("project", Project), new ParameterOverride("reportWriter", reportWriter), new ParameterOverride("dialogService", DialogService) };
                    ReportGeneratorViewModel VM2 = ApplicationModule.Instance.Resolve<ReportGeneratorViewModel>(inputParams2);
                    bool? result = DialogService.ShowDialog<ReportGeneratorView>(this, VM2);
                    if (result.HasValue && result.Value == true)
                    {
                           
                       
                        reportWriter.Report = VM2.Report;
                        reportWriter.Report.Project ??= Project;
                        reportWriter.Project ??= Project;
                        request.ReportWriter = reportWriter;
                            
                        bool async = true;

                        if (async)
                        {

                            _Worker_Reports.RunWorkerAsync(request);
                        }
                        else
                        {
                            try
                            {
                                DoWorkEventArgs e = new(request);
                                DoWork_GenerateReport(null, e);
                                RunWorkerCompletedEventArgs ec = new(e.Result, null, false);
                                DoWork_GenerateReport_Complete(null, ec);
                            }
                            catch { }
                        }
                       

                    }
                    
                    break;
                default:
                    return;
            }
          
           

        }

        public void ShowWarnings(uopDocuments aWarnings = null, string aRangeGUID = "")
        {
            IsEnabled = false;
            uopProject project = Project;
            if (project == null) return;
            bool passedwarnings = aWarnings != null;
          
            if (!passedwarnings)
            {
                //get the project warings
                
                Message_DocumentRequest request = new(uppDocumentTypes.Warning) { Project = project,  RangeGUID = aRangeGUID };
                _Worker_Warnings.RunWorkerAsync(request);
            }
            else
            {
                uopDocuments warnings = aWarnings.GetByDocumentType(uppDocumentTypes.Warning);
                if (warnings.Count > 0)
                {
                 
                    var inputParams = new ParameterOverride[] { new ParameterOverride("project", Project), new ParameterOverride("warnings", warnings), new ParameterOverride("dialogService", DialogService) };

                    WarningViewModel VM = ApplicationModule.Instance.Resolve<WarningViewModel>(inputParams);
                    DialogService.ShowDialog<WarningView>(this, VM);
                 
                }
                else
                {
                    if (!passedwarnings) DialogService.ShowMessageBox(this, $"Project {project.Name} has no defined warnings", "No Warnings Found");
                }
                IsEnabled = true;
            }
            
          
        }
        /// <summary>
        ///  Event handler to clear all data
        /// </summary>
        /// <param name="message"></param>
        public void OnAggregateEvent(Message_ClearData message)
        {
            if (message == null) return;
            if (message.PartType ==  uppPartTypes.Project || message.PartType == uppPartTypes.Undefined) 
            {
                TreeViewNodes = new ObservableCollection<TreeViewNode>();
            }
        }


        public void OnAggregateEvent(Message_Refresh message)
        {
            message ??= new Message_Refresh();
            //message.MainVM ??= this;

           // if (MenuItemViewModelHelper.Refreshing) return;

            MenuItemViewModelHelper.Execute_RefreshDisplay(message);
            //RefreshDisplay(message);

        }

        public void OnAggregateEvent(Message_ShowWarnings message)
        {
            if (message == null) return;
            if (message.Warnings.Count <= 0) return;
          

            if (MenuItemViewModelHelper.Refreshing) return;

            try
            {
                var inputParams = new ParameterOverride[] { new ParameterOverride("project", Project), new ParameterOverride("warnings", message.Warnings), new ParameterOverride("dialogService", DialogService) };

                WarningViewModel warningVM = ApplicationModule.Instance.Resolve<WarningViewModel>(inputParams);
                warningVM.UpdateWarningsMessage(message.Warnings);
                DialogService.ShowDialog<WarningView>(this, warningVM);

            }
            catch
            {

            }

        }

        public void OnAggregateEvent(Message_UpdatePropertyList message)
        {
            if (message.PartType == uppPartTypes.Project)
            {
                uopProperties props = message.Properties;
                props.DisplayUnits = message.Units;
                Props_Project = props;
            }
        }


        public void OnAggregateEvent(Message_UpdateTreeView message)
        {
            if (message == null) return;


            Message_Refresh refresh = message.RefreshMessage;
            if (refresh == null) return;
            if (message.RefreshMessage.TreeNodes == null) return;
            TreeViewNodes = message.RefreshMessage.TreeNodes;
          
            
            //refresh.SuppressTree = true;
            ////string action = (!refresh.Clear) ? "Refreshing" : "Clearing";
            ////refresh.SetStatusMessages($"{action} Project Tree", "");
            //var task = CreateTreeNodes_Async(refresh);
            //ObservableCollection<TreeViewNodeType> treenodes = task.Result;
            //refresh.SetStatusMessages("", "");
            //TreeViewNodes = treenodes;

            ////VM.ProjectCollection = new ObservableCollection<ProjectTreeViewModel>();
            //ObservableCollection<TreeViewNodeType> treenodes = TreeViewNodes;
            //ProjectTreeViewModel treeview = null;
            //if (!refresh.Clear)
            //{

            //    refresh.SetStatusMessages($"{action} Project Tree", "");

            //    treeview = new ProjectTreeViewModel(refresh.Project, DialogService);
            //    treenodes = treeview.GetTreeViewNodes(refresh.Project, this);

            //}
            //// to update the UI tree
            //TreeViewNodes = treenodes;

            //if (ProjectVM != null) ProjectVM.WarningColor = (treeview == null) ? "Black" : (treeview.WarningCount) <= 0 ? "Black" : "Red";

        }
        #endregion Event Handlers


        #region Properties

      

        public override IDialogService DialogService { get => base.DialogService; set => base.DialogService = value; }

        private static IEventAggregator _EventAggregator;
        internal override IEventAggregator EventAggregator { get => _EventAggregator; set { if (value != null) value.Subscribe(this); _EventAggregator = value; } }

        ///// <summary>
        ///// Tracks visibilility of MD Spout Child Form
        ///// </summary>
        //private Visibility _VisibilityMDProject = Visibility.Collapsed;
        //public Visibility VisibilityMDProject
        //{
        //    get => _VisibilityMDProject;
        //    set { _VisibilityMDProject = value; NotifyPropertyChanged("VisibilityMDProject");  }
        //}


        private UOPPropertyTreeViewModel _TreeViewModel;
        public UOPPropertyTreeViewModel TreeViewModel
        { 
            get => _TreeViewModel; 
            
            set 
            {
                if (_TreeViewModel != null) _TreeViewModel.NodeDoubleClickEvent -= TreeNodeDoubleClick;
                
                _TreeViewModel = value; NotifyPropertyChanged("TreeViewModel");
                if (_TreeViewModel != null) _TreeViewModel.NodeDoubleClickEvent += TreeNodeDoubleClick;
            } 
        }


        //private ObservableCollection<TreeViewNode> _TreeViewNodes = new ObservableCollection<TreeViewNode>();
        //public ObservableCollection<TreeViewNode> TreeViewNodes { get => _TreeViewNodes; set { value ??= new ObservableCollection<TreeViewNode>(); _TreeViewNodes = value; NotifyPropertyChanged("TreeViewNodes"); } }

        // private ObservableCollection<TreeViewNode> _TreeViewNodes;
        public ObservableCollection<TreeViewNode> TreeViewNodes
        {
            get { TreeViewModel ??= new UOPPropertyTreeViewModel(); return TreeViewModel.TreeViewNodes; }  //_TreeViewNodes;

            set
            {
                value ??= new ObservableCollection<TreeViewNode>();
                TreeViewModel ??= new UOPPropertyTreeViewModel();
                TreeViewModel.TreeViewNodes = value;
                NotifyPropertyChanged(MethodBase.GetCurrentMethod());
            }

            //_TreeViewNodes = value; NotifyPropertyChanged("TreeViewNodes"); }
        }

        public override uopProject Project
        {
            get => base.Project;
            set
            {
                base.Project = value;

                List<string> menuHeaders = new() { "Close", "Export" };

                if (value == null)
                {
                    if (!Unloading)
                        ClearAll(bSuppressImage: false, bSilent: true);
                }
                else
                { 
                    if (!Loading)
                        EventAggregator.Publish<Message_Refresh>(new Message_Refresh()); // RefreshDisplay();

                }
               if(base.Project != null)
                {
                    if (base.ProjectFamily == uppProjectFamilies.uopFamMD)
                        _MDProjectViewModel ??= new MDProjectViewModel(EventAggregator, DialogService);
                }
                ProjectName = (value != null) ? value.Name : "";
                FilePath = (value != null) ? value.ProjectFolder : "";
                UserId = $"{appApplication.User.NiceName } [{ appApplication.User.NetworkName }]";
                UpdateVisibily();
            }
        }


        private MenuItemViewModelHelper _MenuItemViewModelHelper;
        public MenuItemViewModelHelper MenuItemViewModelHelper
        {
            get
            {
                if (_MenuItemViewModelHelper != null)
                {
                    _MenuItemViewModelHelper.Project = Project;
                    _MenuItemViewModelHelper.DialogService = DialogService;
                    _MenuItemViewModelHelper.EventAggregator = EventAggregator;
                }
                return _MenuItemViewModelHelper;
            }
            set
            {

                _MenuItemViewModelHelper = value;
                if (_MenuItemViewModelHelper != null)
                {
                    _MenuItemViewModelHelper.DialogService = DialogService;
                    _MenuItemViewModelHelper.EventAggregator = EventAggregator;
                    //  EventAggregator.Subscribe(_MenuItemViewModelHelper);
                }
            }
        }

        public override bool IsEnabled 
        {
            get => base.IsEnabled;
            set
            {
                bool newval = base.IsEnabled != value;
                base.IsEnabled = value;
                NotifyPropertyChanged(nameof(IsEnabled));
                NotifyPropertyChanged(nameof(IsBusyVisible));
                NotifyPropertyChanged(nameof(IsCancelBtnEnable));
                NotifyPropertyChanged(nameof(IsOkBtnEnable));
                NotifyPropertyChanged(nameof(VisibilityStatusMessages));

                IProjectViewModel projVM = ProjectVM;
                if (projVM != null) projVM.Disabled = !value;
                   

                if (newval)
                {
                    if (base.IsEnabled)
                        Console.WriteLine($"MAIN VM - ENABLED");
                    else
                        Console.WriteLine($"MAIN VM - DISABLED  [{StatusMessage1}]");
                }

            }
        }
        private string _StatusMessage1 = "";
        public string StatusMessage1
        {
            get => _StatusMessage1;
            set
            {
                value ??= string.Empty;
                if(_StatusMessage1 != value) 
                {
                    _StatusMessage1 = value;
                }
                NotifyPropertyChanged("StatusMessage1");
            }
        }


        private string _StatusMessage2 = "";
        public string StatusMessage2
        {
            get => _StatusMessage2;
            set
            {
                _StatusMessage2 = value;
                NotifyPropertyChanged("StatusMessage2");
            }
        }

     
        public Visibility VisibilityStatusMessages  =>IsEnabled ? Visibility.Collapsed : Visibility.Visible; 
        
        public bool Unloading { get; set; }
        public bool Loading { get; set; }

        private System.WeakReference<TreeViewNode> _SelectedNodeRef;
        public TreeViewNode SelectedNode {
            get
            {
                TreeViewNode _rVal = null;

                if (_SelectedNodeRef != null)
                {
                    if (!_SelectedNodeRef.TryGetTarget(out _rVal))
                        _SelectedNodeRef = null;
                }
                return _rVal;
            }
            set { _SelectedNodeRef = (value == null) ? null : new System.WeakReference<TreeViewNode>(value); }
        }
      
        public IProjectViewModel ProjectVM
        {
            get => (Project == null) ? null : Project.ProjectFamily switch { uppProjectFamilies.uopFamMD => MDProjectViewModel, _ => null };
        }
        private MDProjectViewModel _MDProjectViewModel;
        public MDProjectViewModel MDProjectViewModel { get { _MDProjectViewModel ??= new MDProjectViewModel(EventAggregator, DialogService); return _MDProjectViewModel; } set => _MDProjectViewModel = value; }


      
        private ProjectTreeModel _TreeView;
        public ProjectTreeModel TreeView { get => _TreeView; set { _TreeView = value; NotifyPropertyChanged("TreeView"); } }



        private string _FilePath;
        public string FilePath { get => _FilePath; set { _FilePath = value; NotifyPropertyChanged("FilePath"); } }

        private string _FileName;
        public string FileName { get => _FileName; set { _FileName = value; NotifyPropertyChanged("FileName"); } }

        private string _UserId;
        public string UserId { get => _UserId; set { _UserId = value; NotifyPropertyChanged("UserId"); } }

        private string _ProjectName;
        public new string ProjectName { get => _ProjectName; set { _ProjectName = value; NotifyPropertyChanged("ProjectName"); } }

        ///// <summary>
        ///// get or set _CurrentViewModel for Data Template
        ///// </summary>
        //public object CurrentViewModel
        //{
        //    get => _CurrentViewModel; 
        //    set { _CurrentViewModel = value; NotifyPropertyChanged("CurrentViewModel"); }
        //}

        public bool Refreshing { get => MenuItemViewModelHelper.Refreshing; set => MenuItemViewModelHelper.Refreshing = value; }

        private uopProperties _Props_Project = new();
        public uopProperties Props_Project
        {
            get => _Props_Project;
            set
            {
                _Props_Project = value ?? new uopProperties();
                NotifyPropertyChanged("Props_Project");

                Prop_List_Project = PropertyListEntry.FromUOPProperties(_Props_Project);
            }
        }
        private List<PropertyListEntry> _Prop_List_Project = new();
        public List<PropertyListEntry> Prop_List_Project
        {
            get => _Prop_List_Project;
            set
            {
                _Prop_List_Project = value; NotifyPropertyChanged("Prop_List_Project");
            }
        }

        #endregion Properties



        #region Methods

        /// <summary>
        /// Sets the enabled value of the main UI  
        /// </summary>
        /// <param name="message"></param>
        public void Execute_ToggleAppEnabled(Message_ToggleAppEnabled message)
        {
            if (message == null) return;
            Execute_ToggleAppEnabled(message.EnabledValue, message.StatusMessage, message.SubStatusMessage);
       
        }
        /// <summary>
        /// Sets the enabled value of the main UI  
        /// </summary>
        /// <param name="message"></param>
        public void Execute_ToggleAppEnabled(bool bEnabledVal, string aBusyMessage = null, string aSubStatus = null)
        {
            if (!bEnabledVal)
            {

                if (string.IsNullOrWhiteSpace(aBusyMessage)) aBusyMessage = StatusMessage1;
                if (string.IsNullOrWhiteSpace(aBusyMessage)) aBusyMessage = "Working";
                if (aSubStatus == null) aSubStatus = StatusMessage2;

                IsEnabled = false;


                if (aBusyMessage != null)
                {
                    BusyMessage = aBusyMessage;
                    StatusMessage1 = aBusyMessage;
                }

                if (aSubStatus != null)
                    StatusMessage2 = aSubStatus;
            }
            else
            {

                IsEnabled = true;
                StatusMessage1 = "";
                StatusMessage2 = "";
                SuppressEvents = false;
               

            }
           
            UpdateVisibily();
            NotifyPropertyChanged("IsBusyVisible");
        }
        /// <summary>
        /// Update status messages for main screen
        /// </summary>

        public void Execute_SetStatusMessages(string aStatusMessage1 = null, string aStatusMessage2 = null)
        {


         bool supwuz = SuppressEvents;
            SuppressEvents = false;
            if (aStatusMessage1 == null) aStatusMessage1 = StatusMessage1;
            if (aStatusMessage2 == null) aStatusMessage1 = StatusMessage2;

            StatusMessage1 = aStatusMessage1;
            BusyMessage = StatusMessage1;
            StatusMessage2 = aStatusMessage2;
            SuppressEvents = supwuz;
        }

        /// <summary>
        /// Update status messages for main screen
        /// </summary>

        public void Execute_SetStatusMessages(WinTray.UI.Messages.Message_Base message)
        {

            if (message == null) return;
            Execute_SetStatusMessages(message.StatusMessage1, message.StatusMessage2);
        }
        /// <summary>
        /// Update caption for main screen
        /// </summary>

        /// <returns></returns>
        public void UpdateTitle()
        {
            string _rVal = "UOP WinTray";// Use uopGlobals.ApplicationVersion if required in future

            if (Project != null)
            {
                _rVal = string.Format("{0} - {1} - {2}", _rVal, Project.ProjectTypeName, Project.Name);
            }
            Title = _rVal;
        }

        public MessageBoxResult ShowMessageBox(string messageBoxText, string caption = "", MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None, MessageBoxResult defaultResult = MessageBoxResult.None)
        {

            return DialogService.ShowMessageBox(this, messageBoxText, caption, button, icon);
        }

        public override void Activate(Window myWindow)
        {
            if (!Activated)
            {
                base.Activate(myWindow);
            }
            else
            {
                if (myWindow != null) myWindow.Visibility = Visibility.Visible;
                UpdateVisibily();
            }
                

        }


        public void ClearAll(bool bSuppressPropertyLists = false, bool bSuppressDataGrids = false, bool bSuppressTree = false, bool bSuppressImage = false, bool bRangeDataOnly = false, bool bSilent = false)
        {
            if (Unloading) return;
            Message_Refresh message = new(bSuppressPropertyLists, bSuppressDataGrids, bSuppressTree, bSuppressImage, bRangeDataOnly, bClear: true, bSilent: bSilent);


            EventAggregator.Publish(message);

        }


        public ObservableCollection<TreeViewNode> CreateTreeNodes(Message_Refresh refresh)
        {

           
            ObservableCollection<TreeViewNode> treenodes = new();
            if (refresh != null)
            {
                try
                {
                    ProjectTreeViewModel treeview = null;
                    
                    if (!refresh.Clear)
                    {

                        treeview = new ProjectTreeViewModel(refresh.Project, DialogService);
                        treenodes = treeview.GetTreeViewNodes(refresh.Project, refresh);

                    }
                    // to update the UI tree
                    //TreeViewNodes = treenodes;

                    if (ProjectVM != null) ProjectVM.WarningColor = (treeview == null) ? "Black" : treeview.WarningCount <= 0 ? "Black" : "Red";
                }
                catch (Exception ex)
                {
                    refresh?.AddWarning(null, $"{System.Reflection.MethodBase.GetCurrentMethod()}", ex.Message);
                }

           
            }
            return treenodes;



        }


        /// <summary>
        /// method to show project properties dialogsbox
        /// </summary>
        public void Edit_ProjectProperties(string input = "")
        {
            if (Project == null) return;
            if(Project.ProjectFamily == uppProjectFamilies.uopFamMD)
            {
                MDProjectViewModel projVM = MDProjectViewModel;
                if (projVM == null) return;
                projVM.Edit_ProjectProperties(input);

            }

        }

        /// <summary>
        /// method to show project notes dialogsbox
        /// </summary>
        public void Edit_ProjectNotes()
        {
            if (Project == null) return;
            Edit_Notes_ViewModel VM = new(Project);
            bool? result = DialogService.ShowDialog<Edit_Notes_View>(this, VM);
            if (result.HasValue && result.Value == true)
            {
                Project.HasChanged = true;
                EventAggregator.Publish(new Message_Refresh(bSuppressTree: false));
            }
        }

        public void UpdateWarningsNode()
        {
            uopProject project = WinTrayMainViewModel.WinTrayMainViewModelObj.Project;
            if (project == null) return;
            TreeViewNode treeViewNode = TreeViewNodes?.FirstOrDefault()?.Members?.FirstOrDefault(y => y.NodeName.ToLower().Contains("warnings"));
            if (treeViewNode != null)
            {
                uopDocuments warnings = project.Warnings(bJustOne:true);
                if (warnings.Count > 0)
                {
                    //treeViewNode.NodeName = string.Format(CommonConstants.WARNINGSSTRING, warnings.Count);
                    treeViewNode.Colour = CommonConstants.RED;
                }
                else
                {
                    //treeViewNode.NodeName = CommonConstants.WARNINGS;
                    treeViewNode.Colour = CommonConstants.BLACK;
                }
            }

        }

       

        private void TreeNodeDoubleClick(TreeViewNode aNode)
        {
            //MessageBox.Show(aNode.Path);
            if (aNode == null)
                return;

            List<string> path = aNode.Path.Split('.').ToList();

            if (path.Count < 1) return;
            string level2 = path.Count >= 2 ? path[1].Trim().ToUpper() : "";
            string level3 = path.Count >= 3 ? path[2].Trim().ToUpper() : "";

            List<string> cats = new();
            if (level2.Contains('/'))
            {
                cats = level2.Split('/').ToList();
                level2 = cats[0];
                cats.RemoveAt(0);
                if (cats.Count > 0)
                {
                    level3 = cats[0];
                    cats.RemoveAt(0);
                }
            }

            uopProperties properties = null;
            uopDocument doc;
            uopProperties subproperties = null;
            string subproplabel = null;
            //the project aNode
            if (path.Count == 1)
            {
                uopPart part = aNode.Part;
                if (part == null) return;
                properties = Project.CurrentProperties();


            }
            else
            {
                switch (level2)
                {
                    case "NOTES":
                        Edit_ProjectNotes();
                        return;
                    case "WARNINGS":
                        ShowWarnings();
                        return;
                    case "DOCUMENTS":
                        switch (level3)
                        {
                            case "CALCULATIONS":
                                if (cats.Count <= 0) return;
                                return;
                            case "REPORTS":
                                if (cats.Count <= 0) return;
                                doc = aNode.Document;
                                EventAggregator.Publish<Message_DocumentRequest>(new Message_DocumentRequest(uppDocumentTypes.Report, doc));

                                return;
                            case "DRAWINGS":
                                if (cats.Count <= 0) return;
                                doc = aNode.Document;
                                if (doc == null)
                                    return;
                                if (doc.DocumentType != uppDocumentTypes.Drawing)
                                    return;

                                IProjectViewModel projVM = ProjectVM;
                                if (projVM != null) projVM.RespondToNodeClick(aNode);



                                return;
                            case "WARNINGS":
                                ShowWarnings();
                                return;
                            default:
                                return;
                        }


                    default:
                        uopPart part = aNode.Part;
                        if (part == null) return;
                         properties = part.CurrentProperties();
                        if (part.PartType == uppPartTypes.Stage)
                        {
                            mdStage stage = part as mdStage;
                            subproperties = new uopProperties(stage.SaveProperties($"{aNode.Path}.SubProperties").Item(1).ToList, properties);
                            subproplabel = "Definition Lines";
                        }
                        if (part.PartType == uppPartTypes.SpoutGroup)
                        {
                            mdSpoutGroup sg = part as mdSpoutGroup;

                            mdConstraint constr = sg.Constraints(null);
                            subproperties = constr.CurrentProperties();
                            subproplabel = $"Constraints({constr.Handle})";
                        }
                        break;

                }
            }
            if (properties != null)
            {
                if (properties.Count <= 0) return;
                var inputParams = new ParameterOverride[]
                {
                    new ParameterOverride("aProperties", properties),
                    new ParameterOverride("aDisplayUnits", Project.DisplayUnits) ,
                    new ParameterOverride("aFirstNodeName", aNode.Path) ,
                    new ParameterOverride("aSubProperties", subproperties),
                    new ParameterOverride("aSubPropLable", subproplabel),
                    new ParameterOverride("bSuppressNullProperties", !uopUtils.RunningInIDE)
                };

                UOPPropertyViewerViewModel uopTreePropertyVM = ApplicationModule.Instance.Resolve<UOPPropertyViewerViewModel>(inputParams);

                DialogService.ShowDialog<UOPPropertyViewerView>(this, uopTreePropertyVM);
            }
        }

        public void RespondToTreeNodeDoubleClick(string aNodePath)
        {
            if (string.IsNullOrWhiteSpace(aNodePath) || Project == null ) return;
            aNodePath = aNodePath.Trim().ToUpper();
            TreeViewNode aNode =  TreeViewModel.AllNodes.Find(x => string.Compare(x.Path, aNodePath, true) == 0);

            if (aNode == null) 
                aNode = ProjectTreeModel.FindTreeNode(TreeViewNodes, aNodePath);
           

            //}
            if (aNode == null)
                return;

            List<string> path = aNodePath.Split('.').ToList();
          
            if (path.Count < 1) return;
            string level2 = path.Count >= 2 ? path[1].Trim().ToUpper() : "";
            string level3 = path.Count >= 3 ? path[2].Trim().ToUpper() : "";

            List<string> cats = new();
            if (level2.Contains('/'))
            {
                cats = level2.Split('/').ToList();
                level2 = cats[0];
                cats.RemoveAt(0);
                if (cats.Count >0)
                {
                    level3 = cats[0];
                    cats.RemoveAt(0);
                }
            }

            uopDocument doc;
            uopProperties properties = null;
            uopProperties subproperties = null;
            string subproplabel = null;
            //the project aNode
            if (path.Count == 1)
            {
                uopPart part = aNode.Part;
                if (part == null) return;
                properties = Project.CurrentProperties();


            }
            else
            {
                switch (level2)
                {
                    case "NOTES":
                        return;
                    case "WARNINGS":
                        ShowWarnings();
                        return;
                    case "DOCUMENTS":
                        switch (level3)
                        {
                            case "CALCULATIONS":
                                if (cats.Count <= 0) return;
                                return;
                            case "REPORTS":
                                if (cats.Count <= 0) return;
                                doc = aNode.Document;
                                EventAggregator.Publish<Message_DocumentRequest>(new Message_DocumentRequest(uppDocumentTypes.Report, doc));

                                return;
                            case "DRAWINGS":
                                if (cats.Count <= 0) return;
                                doc = aNode.Document;
                                if (doc == null)
                                    return;
                                if (doc.DocumentType != uppDocumentTypes.Drawing)
                                    return;

                                IProjectViewModel projVM = ProjectVM;
                                if (projVM != null) projVM.RespondToNodeClick(aNode);

                              

                                return;
                            case "WARNINGS":
                                ShowWarnings();
                                return;
                            default:
                                return;
                        }
                       

                    default:
                        uopPart part = aNode.Part;
                        if (part == null) return;
                        properties = part.CurrentProperties();
                        if (part.PartType == uppPartTypes.Stage)
                        {
                            mdStage stage = part as mdStage;
                            subproperties = new uopProperties(stage.SaveProperties($"{aNode.Path}.SubProperties").Item(1).ToList, properties);
                            subproplabel = "Definition Lines";
                        }
                        if (part.PartType == uppPartTypes.SpoutGroup)
                        {
                            mdSpoutGroup sg = part as mdSpoutGroup;

                            mdConstraint constr = sg.Constraints(null);
                            subproperties = constr.CurrentProperties();
                            subproplabel = $"Constraints({constr.Handle})";
                        }
                        break;

                }
              
               
            }


            if(properties != null)
            {
                if (properties.Count <= 0) return;
                var inputParams = new ParameterOverride[] 
                {  
                    new ParameterOverride("aProperties", properties), 
                    new ParameterOverride("aDisplayUnits", Project.DisplayUnits) , 
                    new ParameterOverride("aFirstNodeName", aNode.Path) , 
                    new ParameterOverride("aSubProperties", subproperties), 
                    new ParameterOverride("aSubPropLable", subproplabel),
                    new ParameterOverride("bSuppressNullProperties", !uopUtils.RunningInIDE)
                };

                UOPPropertyViewerViewModel uopTreePropertyVM = ApplicationModule.Instance.Resolve<UOPPropertyViewerViewModel>(inputParams);
             
                DialogService.ShowDialog<UOPPropertyViewerView>(this, uopTreePropertyVM);
            }


        }

        public override void UpdateVisibily()
        {
  

            base.UpdateVisibily();
            if (ProjectFamily == uppProjectFamilies.uopFamMD) 
            {
                _MDProjectViewModel ??= new MDProjectViewModel(EventAggregator, DialogService);
                _MDProjectViewModel.Project = Project;
                _MDProjectViewModel.UpdateVisibily();
            }

            bool supevnt = SuppressEvents;
            SuppressEvents = false;
            UpdateTitle();
            NotifyPropertyChanged("VisibilityProject");
            NotifyPropertyChanged("VisibilityMDProject");

            SuppressEvents = supevnt;
        }

        #endregion Methods

        #region Static Methods

        public static WinTrayMainViewModel WinTrayMainViewModelObj
        {
            get
            {
                _WinTrayMainViewModel ??= new WinTrayMainViewModel();
                return _WinTrayMainViewModel;
            }
        }

        #endregion Static Methods
    }

}
