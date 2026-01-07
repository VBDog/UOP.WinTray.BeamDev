using UOP.DXFGraphicsControl;
using UOP.DXFGraphics;
using UOP.WinTray.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using UOP.WinTray.Projects;
using UOP.WinTray.Projects.Documents;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Parts;
using UOP.WinTray.Projects.Tables;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.UI.Events.EventAggregator;
using UOP.WinTray.UI.Interfaces;
using UOP.WinTray.UI.Model;
using System.ComponentModel;
using System.Windows;
using UOP.WinTray.UI.BusinessLogic;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Office2016.Excel;
using DocumentFormat.OpenXml.Spreadsheet;

namespace UOP.WinTray.UI.Messages
{
    public abstract class Message_Base
    {
        #region Constructors

        public Message_Base()
        {
            UnitsToDisplay = uppUnitFamilies.Undefined;
            Async = false;
            //MainVM = WinTrayMainViewModel.WinTrayMainViewModelObj;
        }


        #endregion Constructors

        #region Properties

        private uopTrayRange _Range;
        public uopTrayRange Range { get => _Range != null ? _Range : Project == null ? null : Project.SelectedRange; set => _Range = value; }
        public mdTrayRange MDRange { get { uopTrayRange range = Range; if (range == null) return null; return range.ProjectFamily == uppProjectFamilies.uopFamMD ? (mdTrayRange)range : null; } }
        public mdTrayAssembly MDAssy { get { mdTrayRange range = MDRange; return range == null ? null : range.TrayAssembly; } }

        public bool Silent { get; set; }
        public bool Async { get; set; }

        public string ResultString { get; set; }
        private uppUnitFamilies _UnitsToDisplay;
        public uppUnitFamilies UnitsToDisplay { get => _UnitsToDisplay; set => _UnitsToDisplay = value; }
        public string ErrorMessage { get; set; }

        public IProjectViewModel ProjectVM => MainVM?.ProjectVM;

        internal uopDocuments _Warnings = null;
        public uopDocuments Warnings { get => _Warnings; set => _Warnings = value; }

        public uopProject Project => MainVM.Project; // { get; set; }
        public mdProject MDProject => MainVM.MDProject; //{ get => (Project == null) ? null : (Project.ProjectFamily == uppProjectFamilies.uopFamMD) ? (mdProject)Project : null; set => Project = value; }

        public MenuItemViewModelHelper MainMenuHelper => MainVM.MenuItemViewModelHelper;
        public uppProjectTypes ProjectType => MainVM.ProjectType;
        public uppProjectFamilies ProjectFamily => MainVM.ProjectFamily; // //{ get => (Project == null) ? uppProjectFamilies.uopFamUndefined : Project.ProjectFamily; }

        internal IEventAggregator EventAggregator => MainVM?.EventAggregator;


        public WinTrayMainViewModel MainVM => WinTrayMainViewModel.WinTrayMainViewModelObj;


        public uopProperties Properties { get; set; }

        #endregion Properties

        public string StatusMessage { get; set; }
        public string StatusMessage1 { get; set; }
        public string StatusMessage2 { get; set; }
        #region Methods

        public MessageBoxResult ShowMessageBox(string messageBoxText, string caption = "", MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None, MessageBoxResult defaultResult = MessageBoxResult.None) 
         => MainVM.ShowMessageBox(messageBoxText,caption,button,icon,defaultResult);
        

        public void PublishMessage<T>(T message)
        {
            IEventAggregator ea = EventAggregator;
            if (ea == null || message == null)
                return;

            ea.Publish<T>(message);
        }
     
        public uopDocWarning AddWarning(uopPart aPart, string aBrief = "", string aTextString = "", uppWarningTypes wType = uppWarningTypes.General, string aOwnerName = "", string aCategory = "", string aSubCategory = "")
        {
            Warnings ??= new uopDocuments();
            SetStatusMessages(null, $"ERROR [{aBrief}] -  {aTextString}");
            return Warnings.AddWarning(aPart, aBrief, aTextString, wType, aOwnerName, aCategory, aSubCategory);
        }

        public void HandleException(System.Reflection.MethodBase aMethod, Exception aException)
        {
            if (aException == null) return;
            string brief = aMethod == null ? "" : $"An Error Occured In Method {aMethod.Name}";
            AddWarning(null, brief, aException.Message);
        }

        public void ToggleAppEnabled(bool bEnabledVal,  string aBusyMessage = null, string aSubStatus = null)
        {
          
            if (aBusyMessage != null && string.IsNullOrWhiteSpace(StatusMessage)) StatusMessage = aBusyMessage.Trim();
            if (Silent)
                MainVM?.Execute_ToggleAppEnabled(bEnabledVal, null, null);
            else
                MainVM?.Execute_ToggleAppEnabled(bEnabledVal, aBusyMessage, aSubStatus);

          //  return;

           // PublishMessage<Message_ToggleAppEnabled>(new Message_ToggleAppEnabled(aEnabledValue: bEnabledVal,  aStatusMessage: aBusyMessage, aSubStatusMessage: aSubStatus));

        }

        public virtual void ReleaseReferences()
        {
            Properties = null;
            _Range = null;
            ListenToProject = null;

        }

        public void SetStatusMessages(string message1 = null, string message2 = null, bool? appEnabled = null)
        {
            if (Silent) return;
            //if (message1 == null) 
            //    message1 = StatusMessage;
            if (!appEnabled.HasValue)
            {
              
                  MainVM?.Execute_SetStatusMessages(message1,message2);
              
            }
            else
            {
                ToggleAppEnabled(appEnabled.Value, aBusyMessage: message1, aSubStatus: message2);
            }
        }

        public void ShowWarnings()
        {
            if (_Warnings == null) return;
            if (_Warnings.Count > 0)  PublishMessage<Message_ShowWarnings>(new Message_ShowWarnings(_Warnings));
        }

     
        #endregion Methods

        #region Event Handlers

        private uopProject _ListenToProject;
        public uopProject ListenToProject
        {
            get => _ListenToProject;
            set
            {
                if (_ListenToProject != null)
                {
                    _ListenToProject.eventPartGeneration -= EventHandler_ProjectPartGenerationStatusChange;
                    _ListenToProject.eventReadStatusChange -= EventHandler_ProjectReadStatusChange;
                }
                _ListenToProject = value;

                if (_ListenToProject != null)
                { 
                    _ListenToProject.eventPartGeneration += EventHandler_ProjectPartGenerationStatusChange;
                    _ListenToProject.eventReadStatusChange += EventHandler_ProjectReadStatusChange;
                }                  

            }
        }

        private async void EventHandler_ProjectPartGenerationStatusChange(string aStatusString, bool? bBegin = null)
        {
            if(Silent) return;
            WinTrayMainViewModel mainVM = MainVM;
            if (mainVM == null) return;
            System.Windows.Threading.Dispatcher dispatcher = System.Windows.Application.Current?.Dispatcher;
            dispatcher ??= System.Windows.Threading.Dispatcher.CurrentDispatcher;
            await dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() =>
                {
                    mainVM.StatusMessage2 = aStatusString;
                })
            );
        }

        private async void EventHandler_ProjectReadStatusChange(string StatusString, int aIndex)
        {
            if (Silent) return;
            WinTrayMainViewModel mainVM = MainVM;
            if (mainVM == null) return;
            if (mainVM == null) return;
            System.Windows.Threading.Dispatcher dispatcher = System.Windows.Application.Current?.Dispatcher;
            dispatcher ??= System.Windows.Threading.Dispatcher.CurrentDispatcher;
            await dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() =>
                {
                    mainVM.StatusMessage2 = StatusString;
                })
            );
        }
        #endregion Event Handlers
    }




    public class Message_Refresh: Message_Base
    {
        #region Constructors

        public Message_Refresh(bool bSuppressPropertyLists = true, bool bSuppressDataGrids = true, bool bSuppressTree = true,
                                bool bSuppressImage = true, bool bRangeDataOnly = false, bool bClear = false, bool bSilent = false,
                                List<uppPartTypes> aPartTypeList = null, bool bCloseDocuments = false, List<uopProperty> aChanges = null, bool bUpdateParts = false)
        {
            _Changes = aChanges == null ? new List<uopProperty>() : aChanges;
            SuppressPropertyLists = bSuppressPropertyLists;
            SuppressDataGrids = bSuppressDataGrids;
            SuppressTree = bSuppressTree;
            SuppressImage = bSuppressImage;
            PartTypeList = aPartTypeList ?? new List<uppPartTypes>();
            Silent = bSilent;
            Clear = bClear;
            RangeDataOnly = bRangeDataOnly;
            SuppressDocumentClosure = !bCloseDocuments;
            UnitsToDisplay = uppUnitFamilies.Undefined;
            ForceDocumentRefresh = false;
            if (PartTypeList.Count > 0) SuppressPropertyLists = false;
            UpdateRangeList = false;
            Async = true;
            Message_ShowSpoutAreas = null;
            DirtyRange = "";
            UpdateParts = bUpdateParts;
            //MainVM = WinTrayMainViewModel.WinTrayMainViewModelObj;
        }
        public Message_Refresh()
        {
            _Changes = new List<uopProperty>();
            SuppressPropertyLists = false;
            SuppressDataGrids = false;
            SuppressTree = false;
            SuppressImage = false;
            PartTypeList = new List<uppPartTypes>();
            Silent = false;
            Clear = false;
            RangeDataOnly = false;
            Async = true;
            UpdateRangeList = false;
            ForceDocumentRefresh = false;
            UnitsToDisplay = uppUnitFamilies.Undefined;
            Message_ShowSpoutAreas = null;
            DirtyRange = "";
            //MainVM = WinTrayMainViewModel.WinTrayMainViewModelObj;
        }

        #endregion Constructors

        #region Properties

        public bool HideEditUI { get; set; }

        private List<uopProperty> _Changes;
        public List<uopProperty> Changes => _Changes;

        public UOPDocumentTab SelectedTab { get; set; }

        public List<uopProperties> PropertyListProperties { get; set; }
        public List<uopTable> DataTables { get; set; }

        public bool UpdateRangeList { get; set; }
        public string DirtyRange { get; set; }
        public uopDocDrawing Drawing { get; set; }
        public List<TreeViewNode> AllNodes { get; set; }
        public ObservableCollection<TreeViewNode> TreeNodes { get; set; }
        public bool ForceDocumentRefresh { get; set; }
        public bool UpdateParts { get; set; }
        public bool SuppressPropertyLists { get; set; }
        public bool SuppressDataGrids { get; set; }
        public bool SuppressTree { get; set; }
        public bool RangeNameChanged { get; set; }

        public DXFViewer Viewer { get; set; }

        public dxfImage Image { get; set; }
        public IProjectViewModel ProjectViewModel
        {
            get { return MainVM?.ProjectVM; }
            //set; 
        }

        public List<uopPart> Parts { get; set; }

        public bool SuppressDocumentClosure { get; set; }

        public bool SuppressImage { get; set; }

        public bool ResetComponents { get; set; }

        public bool Clear { get; set; }

        public bool RangeDataOnly { get; set; }

        private List<uppPartTypes> _PartTypeList = new();
        public List<uppPartTypes> PartTypeList { get => _PartTypeList; set { _PartTypeList = value ?? new List<uppPartTypes>(); if (_PartTypeList.Count > 0) SuppressPropertyLists = false; } }

        public uppPartTypes PartType { get; set; }


       

        public bool DisplayErrantAreas { get; set; }

        public string SelectedRangeGUID { get; set; }


        public System.Drawing.Size DeviceSize { get; set; }

        private IappDrawingSource _DrawingSource;
        public IappDrawingSource DrawingSource
        {
            get => _DrawingSource;
            set
            {
                if (_DrawingSource != null)
                {
                    _DrawingSource.StatusChange -= StatusChangeHandler;
                    if (value == null) _DrawingSource.TerminateObjects();
                }
                _DrawingSource = value;
                if (_DrawingSource != null)
                {
                    _DrawingSource.StatusChange += StatusChangeHandler;
                    _DrawingSource.Image = Image;
                }


            }

        }

        public Message_ShowSpoutAreaDistibution Message_ShowSpoutAreas { get; set; }

        #endregion Properties

        #region Methods

        public void Publish() => base.PublishMessage<Message_Refresh>(this);


        public List<uppPartTypes> GetPartTypes(bool bForPropertyLists)
        {
            List<uppPartTypes> _rVal = new();
            PartTypeList ??= new List<uppPartTypes>();
            if (ProjectFamily == uppProjectFamilies.uopFamMD)
            {
                if (bForPropertyLists)
                {
                    if ((PartTypeList.Count == 0 || PartTypeList.Contains(uppPartTypes.Project)) && !RangeDataOnly) _rVal.Add(uppPartTypes.Project);
                    if (PartTypeList.Count == 0 || PartTypeList.Contains(uppPartTypes.TrayRange)) _rVal.Add(uppPartTypes.TrayRange);
                    if (PartTypeList.Count == 0 || PartTypeList.Contains(uppPartTypes.Downcomer)) _rVal.Add(uppPartTypes.Downcomer);
                    if (PartTypeList.Count == 0 || PartTypeList.Contains(uppPartTypes.Deck)) _rVal.Add(uppPartTypes.Deck);
                    if (PartTypeList.Count == 0 || PartTypeList.Contains(uppPartTypes.DesignOptions)) _rVal.Add(uppPartTypes.DesignOptions);
                    if (PartTypeList.Count == 0 || PartTypeList.Contains(uppPartTypes.Materials)) _rVal.Add(uppPartTypes.Materials);
                    if (PartTypeList.Count == 0 || PartTypeList.Contains(uppPartTypes.StartupSpouts) || PartTypeList.Contains(uppPartTypes.StartupSpout)) _rVal.Add(uppPartTypes.StartupSpouts);
                    if (PartTypeList.Count == 0 || PartTypeList.Contains(uppPartTypes.TraySupportBeam)) _rVal.Add(uppPartTypes.TraySupportBeam);
                }
                else
                {
                    if (PartTypeList.Count == 0 || PartTypeList.Contains(uppPartTypes.SpoutGroup) || PartTypeList.Contains(uppPartTypes.SpoutGroups)) _rVal.Add(uppPartTypes.SpoutGroups);
                    if (PartTypeList.Count == 0 || PartTypeList.Contains(uppPartTypes.Downcomer) || PartTypeList.Contains(uppPartTypes.Downcomers)) _rVal.Add(uppPartTypes.Downcomers);
                    if (PartTypeList.Count == 0 || PartTypeList.Contains(uppPartTypes.DeckPanel) || PartTypeList.Contains(uppPartTypes.DeckPanels)) _rVal.Add(uppPartTypes.DeckPanels);

                }
            }
            return _rVal;
        }



        public List<uopProperties> GetProperties()
        {

            List<uopProperties> _rVal = new();
            try
            {
                List<uppPartTypes> ptypes = GetPartTypes(true);
                if (ProjectFamily == uppProjectFamilies.uopFamMD)
                {
                    uopProperties props;
                    mdProject mdproj = (mdProject)Project;

                    foreach (var item in ptypes)
                    {
                        try
                        {
                            SetStatusMessages(null, $"Building Properties For {item.GetDescription()} ");
                            props = Clear ? new uopProperties() : mdUtils.GetDisplayListProperties(mdproj, item, 0, UnitsToDisplay);
                            if (props != null)
                            {
                                props.PartType = item;
                                _rVal.Add(props);
                            }
                        }
                        catch (Exception ex)
                        {
                            HandleException(System.Reflection.MethodBase.GetCurrentMethod(), ex);
                        }

                        //Thread.Sleep(250);
                    }

                }

            }
            catch (Exception e) { HandleException(System.Reflection.MethodBase.GetCurrentMethod(), e); }
            finally { PropertyListProperties = _rVal; }
            
            return _rVal;



        }

        public List<uopTable> GetDataTables()
        {

            List<uopTable> _rVal = new();

            try
            {

                List<uppPartTypes> ptypes = GetPartTypes(false);
                if (ProjectFamily == uppProjectFamilies.uopFamMD)
                {
                    uopTable table;
                    mdProject mdproj = (mdProject)Project;

                    foreach (var item in ptypes)
                    {
                        SetStatusMessages(null, $"Building Properties For {item.GetDescription()} ");
                        table = Clear ? new uopTable() : mdUtils.GetDisplayTableProperties(mdproj, item, 0, UnitsToDisplay);
                        if (table != null)
                        {
                            table.PartType = item;
                            _rVal.Add(table);
                        }
                        //Thread.Sleep(250);
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(System.Reflection.MethodBase.GetCurrentMethod(), ex);
            }
            finally 
            {
                DataTables = _rVal;

            }
            return _rVal;



        }

        public void AddPartType(uppPartTypes aPartType)
        {
            PartTypeList ??= new List<uppPartTypes>();

            if (PartTypeList.FindIndex(x => x == aPartType) < 0) return;
            PartTypeList.Add(aPartType);
        }



        public void Publish_TreeView(bool bByMessage = false)
        {
            if (!bByMessage) 
            {
                MainVM.TreeViewNodes = TreeNodes == null ? new ObservableCollection<TreeViewNode>() : TreeNodes;
            }
            else
            {
                Message_UpdateTreeView message = new Message_UpdateTreeView(this);
                message.Publish();
            }

        }
  


        private void StatusChangeHandler(string StatusString)
        {
            SetStatusMessages(null, StatusString);
        }


        
     
    
        public override  void ReleaseReferences()
        {
                base.ReleaseReferences();
            DataTables = null;
            Parts = null;
            PropertyListProperties = null;
            TreeNodes = null;
            if (DrawingSource != null) { DrawingSource.TerminateObjects(); }
            DrawingSource = null;
            Image = null;
            Drawing = null;
        }

        #endregion Methods

    }


    public class Message_ShowWarnings : Message_Base
    {
        public Message_ShowWarnings(IEnumerable<uopDocument> aWarnings = null)
        {
            _Warnings = new uopDocuments();
            if (aWarnings == null) return;
            foreach (uopDocument aWarning in aWarnings) 
            { 
                if(aWarning == null) continue;
                if(aWarning.DocumentType == uppDocumentTypes.Warning)  _Warnings.Add(aWarning); 
            }
        
            
        }

        public void Publish() => base.PublishMessage<Message_ShowWarnings>(this);

    }

    public class Message_RefreshRangeList : Message_Base
    {
        public Message_RefreshRangeList( string aSelectedName, bool bSuppressRefresh)
        {

            _Ranges = null;
            SelectedTrayRangeName = aSelectedName;
            SuppressRefresh = bSuppressRefresh;
        }
        public string SelectedTrayRangeName { get; set; }
       
        public bool SuppressRefresh { get; set; }

        private colUOPTrayRanges _Ranges;
        public colUOPTrayRanges Ranges { get => _Ranges != null ? _Ranges : Project == null ? new colUOPTrayRanges() : Project.TrayRanges; set => _Ranges = value; }

    }


public class Message_HighlightRangeNodes : Message_Base
    {

        public Message_HighlightRangeNodes(string aRangeName)
        {
            RangeName = aRangeName;
        }
        public string RangeName { get; set; }
    }



    public class Message_RefreshControls : Message_Base
    { 
    
    } 


    public class Message_UpdatePropertyList : Message_Base
    {


        public Message_UpdatePropertyList()
        {
            PartType = uppPartTypes.Undefined;
            Units = uppUnitFamilies.English;
        }

        public Message_UpdatePropertyList(Message_Refresh aRefresh)
        {
            RefreshMessage = aRefresh ?? new Message_Refresh();
            PartType = RefreshMessage.PartType;
            Units = RefreshMessage.UnitsToDisplay;
      
        }
        public Message_UpdatePropertyList(uppPartTypes aPartType = uppPartTypes.Undefined) 
        {
            PartType = aPartType;
            Units = uppUnitFamilies.English;
        }

        public uppPartTypes PartType { get; set; }

        public Message_Refresh RefreshMessage { get; set; }

        public uopProperties GetProperties(uppPartTypes aPartType = uppPartTypes.Undefined,uopProject aProject = null )
        {
            uppPartTypes ptype = (aPartType == uppPartTypes.Undefined) ? PartType : aPartType;
            aProject ??= Project;

            if (aProject == null) return new uopProperties();
            uopProperties _rVal;
            switch (aProject.ProjectFamily)
            {
                case uppProjectFamilies.uopFamMD:
                    _rVal = mdUtils.GetDisplayListProperties((mdProject) aProject, ptype, aUnits: Units);
                    break;
                default:
                    _rVal = new uopProperties();
                    break;
            }
            return _rVal;

        }

        public bool Clear { get => Project == null; }
        public uppUnitFamilies Units { get; set; }

        public void Publish() => base.PublishMessage<Message_UpdatePropertyList>(this);

    }

    public class Message_ClearData : Message_Base
    {
        public Message_ClearData(uppPartTypes aPartType = uppPartTypes.Undefined) { PartType = aPartType; }
       public uppPartTypes PartType { get; set; }
    }

        public class Message_RefreshDataGrid : Message_Base
    {
        public Message_RefreshDataGrid()
        {
            PartType = uppPartTypes.Undefined;
            Properties = new uopProperties();
            Clear = false;
            UnitsToDisplay = uppUnitFamilies.English;
        }

        public Message_RefreshDataGrid(uppPartTypes aPartType = uppPartTypes.Undefined, uopProperties aProperties = null, bool bClear = false, uppUnitFamilies aUnitsToDisplay = uppUnitFamilies.English)
        {
            PartType = aPartType;
            Properties = aProperties;
            Clear = bClear;
            UnitsToDisplay = aUnitsToDisplay;
        }

        public Message_RefreshDataGrid(Message_Refresh aRefresh)
        {
            RefreshMessage = aRefresh ?? new Message_Refresh();
            PartType = RefreshMessage.PartType;
            UnitsToDisplay = RefreshMessage.UnitsToDisplay;
        }

        public uppPartTypes PartType { get; set; }

        public Message_Refresh RefreshMessage { get; set; }


        public bool Clear { get; set; }


        public uopTable DataTable { get; set; }

        public void Publish() => base.PublishMessage<Message_RefreshDataGrid>(this);

        public override void ReleaseReferences()
        {
            base.ReleaseReferences();
            DataTable = null;
            RefreshMessage = null;
        }

    }

  public class Message_OpenFile : Message_Base
    {

        public Message_OpenFile(string aFilename)
        {
            FileName = !string.IsNullOrWhiteSpace(aFilename) ? aFilename.Trim() : string.Empty;
            Error = string.Empty;
            IsImport = false;
            SelectedRangeGUID = string.Empty;
            Warnings = new uopDocuments();
            ProjectType = uppProjectTypes.Undefined;


        }

        public new uppProjectTypes ProjectType { get; set; } = uppProjectTypes.Undefined;

        public new uppProjectFamilies ProjectFamily => uopPart.GetProjectFamily(ProjectType);
        public string Error { get; set; }

        public bool IsImport { get; set; }

        private uopProject _Project;
        public new uopProject Project { get => _Project; set {_Project = value; ProjectType = value != null ? value.ProjectType : uppProjectTypes.Undefined; } }

        public new mdProject MDProject { get=> Project==null? null: Project.ProjectFamily == uppProjectFamilies.uopFamMD ?(mdProject)Project : null ; set => Project = value; }

        public string FileName { get; set; }

        public bool Convert { get; set; }

       
        public string SelectedRangeGUID { get; set; }

        internal async Task<uopProject> Task_ImportFileAsync()
        {
            Warnings = new uopDocuments();
            uopProject _rVal = null;
            WinTrayMainViewModel mainVM = MainVM;
            ErrorMessage = string.Empty;
            uopProject project = Project;
            if (project == null) return null;
            mdProject mdproject =  null;
    
            IsImport = true;
            mainVM.Loading = true;
            string filename = FileName;
            if (string.IsNullOrWhiteSpace(filename)) filename = project.ImportFileName;
                if (string.IsNullOrWhiteSpace(filename))
            {
                HandleException(System.Reflection.MethodBase.GetCurrentMethod(), new Exception("Null Filename Detected"));
                return project;
            }

            if (!System.IO.File.Exists(filename))
            {
                HandleException(System.Reflection.MethodBase.GetCurrentMethod(), new Exception($"File Not Found - '{filename}'"));
                return project;
            }


            try
            {
                await Task.Run(() =>
                {
                    if (project.ProjectFamily == uppProjectFamilies.uopFamMD)
                    {
                        mdproject = (mdProject)project;
                        _rVal = mdproject;
                        if (!Silent) ListenToProject = mdproject;

                        //set the messages and disable the app
                        ToggleAppEnabled(false, $"Importing WinTray File {System.IO.Path.GetFileNameWithoutExtension(filename)}", "");
                        string key = mdproject.KeyNumber;
                        int rev = mdproject.Revision;

                        // read the file data, catch the read statuss  and collect the warnings and listen for the read mdproject status changes

                        if (mdproject.ProjectType == uppProjectTypes.MDSpout)
                        {
                            mdProject.ImportMDHFile(mdproject, ref _Warnings, out _, false);

                        }
                        else
                        {
                            // read the file data, catch the read statuss  and collect the warnings

                            mdproject = new();
                            if (!Silent) ListenToProject = mdproject;
                            _rVal = mdproject;
                            mdproject.ReadFromFile(mdproject.ImportFileName, "WinTray", appApplication.AppVersion, out _Warnings);
                            mdproject.ConvertToMDD();
                            Project = mdproject;
                        }
                        //Execute_ImportMDH(mdproject);
                        mdproject.KeyNumber = key;
                        mdproject.Revision = rev;

                        mdproject.Column.TrayRanges.SortByRingStart();

                        SelectedRangeGUID = mdproject.SelectedRangeGUID;
                    }


                });


            }
            catch (Exception ex)
            {
                HandleException(System.Reflection.MethodBase.GetCurrentMethod(), ex);

            }
            finally
            {
                ToggleAppEnabled(true, "'", "");
                ListenToProject = null;
                if (mdproject != null)
                {
                    mdproject.SuppressEvents = false;
                    _rVal = mdproject;
                    SelectedRangeGUID = mdproject.SelectedRangeGUID;
                }


                mdproject = null;
                mainVM.VisibilityMDProject = Visibility.Visible;
        
            }

            return _rVal;


        }

    }

    public class Message_PromptForRangeInput : Message_Base
    {
        public Message_PromptForRangeInput(Message_Refresh aRefreshMessage)
        {
            RefreshMessage = aRefreshMessage;
        }


        public Message_Refresh RefreshMessage { get; set; }
    }
    
    public class Message_RefreshGraphics : Message_Base
    {
        public Message_RefreshGraphics(Message_Refresh aRefreshMessage)
        {
            RefreshMessage = aRefreshMessage;
        }

    
        public Message_Refresh RefreshMessage { get; set; }
    }
    
    public class Message_Save : Message_Base
    {

        public Message_Save(uopProject aProject)
        {
            Project = aProject;
            Warnings = new uopDocuments();
        }

        public bool SaveCompleted { get;  set; }

        public new uopProject Project { get; set; }

        public WinTrayMainViewModel ParentVM { get; set; }
        public string AppPath { get; set; }
        public string AppTempPath { get; set; }

       public string AppTempFilePath { get; set; }
        public string FullPath { get; set; }
   
        public string FileName { get; set; }
    }

   

    public class Message_ShowSpoutAreaDistibution : Message_Base
    {
        public Message_ShowSpoutAreaDistibution(string aRangeGUID )
        {
            RangeGUID = aRangeGUID;
        }

        public string RangeGUID { get; set; }
    }


    public class Message_ProjectChange : Message_Base
    {
        public Message_ProjectChange( uopProject aProject = null,bool bSuppressRefresh = false)
        {
            Project = aProject;
            SuppressRefresh = bSuppressRefresh;
        }
     
        public new uopProject Project { get; set; }

        public bool SuppressRefresh { get; set; }
    }

    public class Message_HighlightImage : Message_Base
    {
        public Message_HighlightImage(uppPartTypes aPartType = uppPartTypes.Undefined)
        {
            PartType = aPartType;
        }

        public uppPartTypes PartType { get; set; }
    }

    public class Message_DocumentRequest : Message_Base
    {
        public Message_DocumentRequest()
        {

            Document = null;
            Documents = null;
            Silent = false;
        }
        public Message_DocumentRequest(uppDocumentTypes aDocType, uopDocument aDocument = null )
        {

            DocumentType = aDocType;
            Documents = null;
            Document = aDocument;
            Silent = false;

        }

        public string RangeGUID { get; set; }

        private uopProject _Project;
        public new uopProject Project 
        { 
            get => _Project;
            set 
            { 
                if(value == null )
                {
                   if(_Project != null) _Project.eventReadStatusChange -= ProjObj_eventReadStatusChange;

                    
                }
                else
                {
                    value.eventReadStatusChange-= ProjObj_eventReadStatusChange;
                    value.eventReadStatusChange += ProjObj_eventReadStatusChange;
                }
                _Project = value;

            } 
        }

        private async void ProjObj_eventReadStatusChange(string StatusString, int aIndex)
        {
            if (MainVM == null) return;
            Dispatcher dispatcher = System.Windows.Application.Current?.Dispatcher;
            if (dispatcher == null)
            {
                dispatcher = Dispatcher.CurrentDispatcher;
            }

            await dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() =>
                {
                    MainVM.StatusMessage2 = StatusString;
                })
            );
        }

        public uopDocument Document { get; set; }
        public uopDocuments Documents { get; set; }
        public string FileToOpen { get; set; } = "";

        private uppDocumentTypes _DocumentType = uppDocumentTypes.Undefined;
        public uppDocumentTypes DocumentType { get => Document == null ? _DocumentType : Document.DocumentType; set => _DocumentType = value; }
        public uppReportTypes ReportType 
        {
            get 
            {
                if (Document == null) return uppReportTypes.ReportPlaceHolder;
                if (Document.DocumentType != uppDocumentTypes.Report) return uppReportTypes.ReportPlaceHolder;
                uopDocReport report = Document as uopDocReport;
                return report.ReportType;
            }

        }
        public string DocumentName { get => Document == null ? "": Document.Name; }

        public IReportWriter ReportWriter { get; set; }

    

    }


    public class Message_DocmentRequestComplete
    {

        public Message_DocmentRequestComplete(Message_DocumentRequest aRequest = null)
        {
            Request = aRequest;
        }
        public Message_DocumentRequest Request { get; set;  }
    }

    public class Message_ReleaseGraphics :Message_Base
    {
        
    }

    public class Message_UpdateTreeView : Message_Base
    {
        public Message_UpdateTreeView(uppPartTypes aPartType = uppPartTypes.Undefined)
        {

            PartType = aPartType;
        }
        public Message_UpdateTreeView(Message_Refresh aRefreshMessage)
        {
            RefreshMessage = aRefreshMessage ?? new Message_Refresh();
            PartType = RefreshMessage.PartType;

        }
        public uppPartTypes PartType { get; set; }
        public bool CloseAllDocuments { get; set; }

        public Message_Refresh RefreshMessage { get; set; }

        public override void ReleaseReferences()
        {
            base.ReleaseReferences();
            RefreshMessage = null;
        }
        public void Publish() => base.PublishMessage<Message_UpdateTreeView>(this);
    }



        public class Message_SetStatusMessages : Message_Base
    {
        public Message_SetStatusMessages()  {StatusMessage1 = null; StatusMessage2 = null; AppEnable = null; }
        public Message_SetStatusMessages(string stat1 = null, string stat2 = null, bool? bAppEnable = null)
        {
            StatusMessage1 = stat1; StatusMessage2 = stat2; AppEnable = bAppEnable;
        }
  
        public bool? AppEnable { get; set; }
    }


        public class Message_ToggleAppEnabled : Message_Base
    {
        public Message_ToggleAppEnabled(bool aEnabledValue,  string aStatusMessage = "",string aSubStatusMessage = "")
        {
            EnabledValue = aEnabledValue;
            StatusMessage = aStatusMessage == null? "" : aStatusMessage.Trim();
            SubStatusMessage = aSubStatusMessage == null ? "" : aSubStatusMessage.Trim();
        }
        public Message_ToggleAppEnabled()
        {
            EnabledValue = true;
            StatusMessage = "";
            SubStatusMessage = "";
        }
        public bool EnabledValue { get; set; }

    
        public string SubStatusMessage { get; set; }
    }

    /// <summary>
    /// class for Highlight Grids From Drawing functionality
    /// </summary>
    public class Message_HighlightRowOnImageClick : Message_Base
    {

        public Message_HighlightRowOnImageClick(int aSpoutIndex = 0, int aPanelIndex = 0, int aDowncomerIndex = 0, mdProject aMDProject = null)
        {
            SpoutIndex = aSpoutIndex;
            PanelIndex = aPanelIndex;
            DowncomerIndex = aDowncomerIndex;
            MDProject = aMDProject;
        }
        public int SpoutIndex { get; set; }
        public int PanelIndex { get; set; }
        public int DowncomerIndex { get; set; }
        public new mdProject MDProject { get; set; }
    }

    /// <summary>
    /// class for Highlight Grids From Drawing functionality
    /// </summary>
    public class Message_HighlightImageFromGridClick : Message_Base
    {

        public Message_HighlightImageFromGridClick(int aIndex = 0, bool bIsPanel = false, bool bIsDowncomer = false, bool bIsSpoutGroup = false, mdProject aMDProject = null)
        {
            Index = aIndex;
            IsPanel = bIsPanel;
            IsDowncomer = bIsDowncomer;
            IsSpoutGroup = bIsSpoutGroup;
            MDProject = aMDProject;
        }

        public int Index { get; set; }
        public bool IsPanel { get; set; }
        public bool IsDowncomer { get; set; }
        public bool IsSpoutGroup { get; set; }
        public new mdProject MDProject { get; set; }
    }

    /// <summary>
    /// Message to be passed to subscribers when project close is requested..
    /// </summary>
    public class Message_UnloadProject : Message_Base
    {
    }

}
