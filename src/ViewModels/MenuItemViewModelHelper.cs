using UOP.WinTray.UI.Logger;
using UOP.WinTray.UI.BusinessLogic;
using UOP.WinTray.Projects;
using UOP.WinTray.Projects.Tables;
using UOP.WinTray.Projects.Parts;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.UI.Interfaces;
using UOP.WinTray.UI.Events.EventAggregator;
using UOP.WinTray.UI.Messages;
using UOP.WinTray.UI.Commands;
using UOP.WinTray.UI.ViewModels;
using UOP.WinTray.UI.Views;
using UOP.WinTray.UI.Views.Windows;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.OpenFile;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Unity.Resolution;
using UOP.WinTray.UI.Views.Windows.CADfx.Questions;
using DocumentFormat.OpenXml.Office2016.Excel;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Drawing;
using System.Reflection;
using UOP.WinTray.UI.Model;
using DocumentFormat.OpenXml.Spreadsheet;
using Dynamitey.Internal.Optimization;
using System.Threading;

namespace UOP.WinTray.UI
{
    /// <summary>
    /// This class enables menu bar to be dynamically added to the application. Also provides functionality for menu user interaction.
    /// </summary>
    public class MenuItemViewModelHelper : ViewModel_Base, IModalDialogViewModel
    {

        #region Fields

        private bool? _DialogResult;
        private readonly IDialogService _DialogService;


        #endregion Fields

        #region Properties

        public string AppTitle => "UOP WinTray";

        public override string ErrorMessage { get; set; }

        private bool _Refreshing = false;
        public bool Refreshing { get => _Refreshing; set { _Refreshing = value; } }

        public bool IsProjectLoaded => Project != null;
        public virtual bool DontKill { get; set; }

        //private override uopProject Project => (ParentVM != null) ? ParentVM.Project : null;


        private new mdProject MDProject => ParentVM?.MDProject;

        public override Visibility VisibilityMDProject
        {
            get => ParentVM.VisibilityMDProject;
            set { ParentVM.VisibilityMDProject = value; }

        }

        #endregion Properties

        #region Constructor

        public MenuItemViewModelHelper(WinTrayMainViewModel parentVM,
                                        IDialogService dialogService)
        {
            _DialogService = dialogService;
            ParentVM = parentVM;

        }


        #endregion Constructor

        #region Event Handlers


        #endregion Event Handlers


        #region Methods

        public void LoadMenuItem()
        {
            Menu.Clear();
            /* File */
            Menu.Add(Init_Menu_File());
            /*Trays*/
            Menu.Add(Init_Menu_Trays());
            /*View*/
            Menu.Add(Init_Menu_View());
            /*Developer*/
            if (System.Diagnostics.Debugger.IsAttached)
                Menu.Add(Init_Menu_Developer());
            /*Help*/
            Menu.Add(Init_Menu_Help());
        }

        public MenuItemViewModel Init_Menu_File()
        {
            var fileMenuItem = new MenuItemViewModel("File", isenabled: true);
            var newMenuItem = new MenuItemViewModel("New", "", null, "/UOP.WinTray.UI;component/Utilities/Images/New.ico", true);

            //newMenuItem.AddChildMenuItem("Cross FLow Project", CrossFlowProjectCommand);
            newMenuItem.AddChildMenuItem("MD Draw Project", Command_New_MDDrawProject);
            newMenuItem.AddChildMenuItem("MD Spout Project", Command_New_MDSpoutProject);

            fileMenuItem.Children = new ObservableCollection<HierarchicalViewModel<MenuItemViewModel>>
            {
                newMenuItem
            };
            fileMenuItem.AddChildMenuItem("Open", Command_Open, "/UOP.WinTray.UI;component/Utilities/Images/Open.ico");
            fileMenuItem.AddChildMenuItem("Close", Command_Close, "/UOP.WinTray.UI;component/Utilities/Images/Delete.ico");


            //Will be implemented when loading multiple projects
            //fileMenuItem.AddChildMenuItem("Close All", CloseAllCommand);

            fileMenuItem.AddChildMenuItem("separator", null);

            fileMenuItem.AddChildMenuItem("Import", Command_Import, "/UOP.WinTray.UI;component/Utilities/Images/Import.JPG");
            fileMenuItem.AddChildMenuItem("Export", Command_Export);

            fileMenuItem.AddChildMenuItem("separator", null);

            fileMenuItem.AddChildMenuItem("Save", Command_Save, "/UOP.WinTray.UI;component/Utilities/Images/Save.ico");
            fileMenuItem.AddChildMenuItem("Save As", Command_SaveAs, "/UOP.WinTray.UI;component/Utilities/Images/SaveAs.ico");


            fileMenuItem.AddChildMenuItem("separator", null);

            fileMenuItem.AddChildMenuItem("Exit", Command_Exit);

            return fileMenuItem;

        }


        public MenuItemViewModel Init_Menu_Trays()
        {
            var traysMenuItem = new MenuItemViewModel("Trays", isenabled: true);


            //traysMenuItem.AddChildMenuItem("Add", Command_Add_Trays, "", MDProject != null && MDProject.TotalTrayCount > 0);
            //traysMenuItem.AddChildMenuItem("Edit", Command_Edit_Trays, "", MDProject != null && MDProject.TotalTrayCount > 0);
            traysMenuItem.AddChildMenuItem("Delete", Command_Delete_Trays);
            //Add below code once we have Import implemented
            //traysMenuItem.AddChildMenuItem("Import", ImportTraysCommand,"",false);

            return traysMenuItem;
        }


        public MenuItemViewModel Init_Menu_View()
        {
            var viewMenuItem = new MenuItemViewModel("View", isenabled: true);

            viewMenuItem.AddChildMenuItem("Refresh", Command_Refresh);

            //viewMenuItem.AddChildMenuItem("separator", null);

            //viewMenuItem.AddChildMenuItem("Project Explorer", ProjectExplorerCommand, "/UOP.WinTray.UI;component/Utilities/Images/Checked.bmp");
            //viewMenuItem.AddChildMenuItem("Show Working Times", ShowWorkingTimesCommand);

            return viewMenuItem;
        }

        public MenuItemViewModel Init_Menu_Help()
        {
            var _rVal = new MenuItemViewModel("Help", isenabled: true);

            _rVal.AddChildMenuItem("HelpContext", Command_Help_Contents);
            _rVal.AddChildMenuItem("HelpAbout", Command_Help_About);
            //helpMenuItem.AddChildMenuItem("Known Issues", KnownIssuesCommand);
            return _rVal;
        }
        public MenuItemViewModel Init_Menu_Developer()
        {
            var _rVal = new MenuItemViewModel("Developer", isenabled: true) { IsCheckable = true };

            var firstChild = _rVal.AddChildMenuItem("Suppress IDE", Command_DeveloperToggleIDE);
            firstChild.IsCheckable = true;

            //_rVal.AddChildMenuItem("Known Issues", KnownIssuesCommand);
            return _rVal;
        }
        #endregion

        #region Commands

        private DelegateCommand _CMD_MDDrawProject;
        public ICommand Command_New_MDDrawProject => _CMD_MDDrawProject ?? (_CMD_MDDrawProject = new DelegateCommand(param => Execute_NewProject(uppProjectTypes.MDDraw), CanMDDrawProject));

        private DelegateCommand _CMD_MDSpoutProject;
        public ICommand Command_New_MDSpoutProject => _CMD_MDSpoutProject ?? (_CMD_MDSpoutProject = new DelegateCommand(param => Execute_NewProject(uppProjectTypes.MDSpout), CanMDSpoutProject));

        private DelegateCommand _CMD_Open;
        public ICommand Command_Open => _CMD_Open ?? (_CMD_Open = new DelegateCommand(param => Execute_Open(), CanOpen));

        private DelegateCommand _CMD_Close;
        public ICommand Command_Close => _CMD_Close ?? (_CMD_Close = new DelegateCommand(param => Execute_Close(), CanClose));

        private DelegateCommand _CMD_Import;
        public ICommand Command_Import => _CMD_Import ?? (_CMD_Import = new DelegateCommand(param => Execute_Import(), CanImport));

        private DelegateCommand _CMD_Export;
        public ICommand Command_Export => _CMD_Export ?? (_CMD_Export = new DelegateCommand(param => Execute_Export(), CanExport));


        private DelegateCommand _CMD_Save;
        public ICommand Command_Save => _CMD_Save ?? (_CMD_Save = new DelegateCommand(param => Execute_Save(), CanSave));


        private DelegateCommand _CMD_SaveAs;
        public ICommand Command_SaveAs => _CMD_SaveAs ?? (_CMD_SaveAs = new DelegateCommand(param => Execute_SaveAs(), CanSaveAs));

        private DelegateCommand _CMD_Exit;
        public ICommand Command_Exit => _CMD_Exit ?? (_CMD_Exit = new DelegateCommand(param => Execute_Exit(), CanExit));


        private DelegateCommand _CMD_Add_Trays;
        public ICommand Command_Add_Trays => _CMD_Add_Trays ?? (_CMD_Add_Trays = new DelegateCommand(param => Execute_Add_Trays(), CanAdd));


        private DelegateCommand _CMD_Delete_Trays;
        public ICommand Command_Delete_Trays { get { _CMD_Delete_Trays ??= new DelegateCommand(param => Execute_Delete_Trays()); return _CMD_Delete_Trays; } }


        private DelegateCommand _CMD_Refresh;
        public ICommand Command_Refresh => _CMD_Refresh ?? (_CMD_Refresh = new DelegateCommand(param => Execute_Refresh(), CanRefresh));

        private DelegateCommand _CMD_HelpCentents;
        public ICommand Command_Help_Contents => _CMD_HelpCentents ?? (_CMD_HelpCentents = new DelegateCommand(param => Execute_HelpContext(), CanContents));

        private DelegateCommand _CMD_HelpAbout;
        public ICommand Command_Help_About => _CMD_HelpAbout ?? (_CMD_HelpAbout = new DelegateCommand(param => Execute_HelpAbout(), CanAbout));

        private DelegateCommand _CMD_DeveloperToggleIDE;
        public ICommand Command_DeveloperToggleIDE => _CMD_DeveloperToggleIDE ?? (_CMD_DeveloperToggleIDE = new DelegateCommand(param => Execute_DeveloperToggleIDE(), CanAlways));

        #endregion Commands

        #region Properties
        /// <summary>
        /// Dialogservice result
        /// </summary>
        public bool? DialogResult
        {
            get => _DialogResult;
            private set { _DialogResult = value; NotifyPropertyChanged("DialogResult"); }
        }

        private readonly ObservableCollection<MenuItemViewModel> _Menu = new();
        public ObservableCollection<MenuItemViewModel> Menu => _Menu;


        public new WinTrayMainViewModel ParentVM { get; }
        public string Caption { get; set; }


        public bool IsProjectClosed { get; private set; }

        private bool CanMDDrawProject() { return true; }

        private bool CanMDSpoutProject() { return true; }

        private bool CanOpen() { return true; }

        private bool CanClose() { return true; }

        private bool CanCloseAll() { return true; }

        private bool CanImport() { return true; }

        private bool CanExport() { return true; }

        private bool CanSave() { return true; }

        private bool CanSaveAs() { return true; }

        private bool CanExit() { return WinTrayMainViewModel.WinTrayMainViewModelObj.IsEnabled; }

        private bool CanAdd() { return true; }

        private bool CanRefresh() { return true; }

        private bool CanContents() { return true; }

        private bool CanAbout() { return true; }

        private bool CanAlways() { return true; }

        #endregion Properties

        /// <summary>
        /// Opens the file to be loaded 
        /// </summary>
        public async void Execute_Open(string filePath = "")
        {
            if (Project != null)
            {
                string question = string.Empty;
                MessageBoxResult reply = MessageBoxResult.Cancel;
                if (Project.HasChanged)
                {
                    question = $"The current project has unsaved changes.\n\n Select  'Yes' To save the changes before closing it.\n Select 'No' to abandon the changes and close it.\nSelect 'Cancel' to do nothing.";
                    reply = ParentVM.ShowMessageBox(question, "Unsaved Changes Detected", button: MessageBoxButton.YesNoCancel, icon: MessageBoxImage.Question, defaultResult: MessageBoxResult.Cancel);
                    if (reply == MessageBoxResult.Yes)
                    {
                        bool saved = await Execute_SaveCurrentProjectAsyc(false);
                        if (!saved) return;
                    }
                    else if (reply == MessageBoxResult.No)
                    {
                        Execute_UnloadProject();
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    question = $"Close the current project?";
                    reply = ParentVM.ShowMessageBox(question, "Close Curent Project", button: MessageBoxButton.YesNo, icon: MessageBoxImage.Question, defaultResult: MessageBoxResult.No);
                    if (reply == MessageBoxResult.Yes)
                    {
                        Execute_UnloadProject();
                    }
                    else
                    {
                        return;
                    }
                }
            }
            WinTrayMainViewModel mainVM = ParentVM;

            OpenFileDialogSettings setting = new() 
            {
                Title = "Open WinTray Projects",
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = "WinTray Data Files|*.MDP;*.XFP;*.MDD|Cross Flow Project Files|*.XFP|MD Spout Project Files|*.MDP|MD Drawing Project Files|*.MDD",
        };

            
            _DialogResult = _DialogService.ShowOpenFileDialog(mainVM, setting);
            if (!(bool)_DialogResult) return;

            string filename = setting.FileName;

            if (string.IsNullOrWhiteSpace(filename)) return;
            if (!File.Exists(filename)) return;
            Message_OpenFile message = new(filename);

            string ptypestr = uopUtils.ReadINI_String(filename, "PROJECT", "ProjectType", "", out bool found).ToUpper();
            string err = (string.IsNullOrWhiteSpace(ptypestr) || !found) ? "Invalid WinTay File Detect" : "";
            bool converttoMDD = false;
            uopProject newproject = null;

            if (ptypestr.Contains("MD SPOUT"))
                message.ProjectType = uppProjectTypes.MDSpout;
            else if (ptypestr.Contains("MD DRAW"))
                message.ProjectType = uppProjectTypes.MDDraw;
           

            try
            {
                if (message.ProjectType == uppProjectTypes.Undefined)
                {
                    message.ErrorMessage ="The Selected File Does Not Contain WinTray Data";
                }
                else
                {
                    if (message.ProjectType == uppProjectTypes.MDSpout && appApplication.User.IsDesigner)
                    {
                        converttoMDD = mainVM.ShowMessageBox("Convert Project to MD Draw?", "Convert Project?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
                        if (!converttoMDD && !uopUtils.RunningInIDE && !appApplication.User.IsEngineer)
                            message.ErrorMessage = "Designers Cannot Open/Edit MD Spout Project Files. Access Denied.";
                        else
                            message.Convert = converttoMDD;
                    }
                    if (string.IsNullOrWhiteSpace(message.ErrorMessage))
                    {
                        newproject = await Task_OpenFileAsync(message);
                        message.ToggleAppEnabled(true, "", "");
                     
                    }

                    else
                    {
                        message.HandleException(MethodBase.GetCurrentMethod(), new Exception(message.ErrorMessage));
                    }
                        
                }
            }
            catch (Exception ex) 
            {
                message.ErrorMessage = ex.Message;
                message.HandleException(MethodBase.GetCurrentMethod(), ex);
            }
            finally 
            { 
                if(newproject != null)
                {
                    mainVM.Loading = true;
                    mainVM.Project = newproject;
                    mainVM.Loading = false;
                    mainVM.SuppressEvents = false;
                    mainVM.UpdateVisibily();
               
                    Message_Refresh refresh = new Message_Refresh() { Warnings = message.Warnings.Clone(), UpdateRangeList = true};
                    mainVM.MenuItemViewModelHelper.Execute_RefreshDisplay(refresh,false,true);
                    //refresh.Publish();
                }
                else
                {
                    message.ShowWarnings();
                }
                
            }

        }

         private void DoWork_OpenFile_Complete(object sender, RunWorkerCompletedEventArgs e)
        {
            Message_OpenFile message = (Message_OpenFile)e.Result;
            WinTrayMainViewModel mainVM = message.MainVM;

            mdProject mdproject = message.MDProject;
            message.ListenToProject = null;

            Message_Refresh refreshmsg = new()
            {
                Async = true,
                SelectedRangeGUID = message.SelectedRangeGUID,
                Warnings = message.Warnings,
                DisplayErrantAreas = !message.IsImport && mdproject.ProjectType == uppProjectTypes.MDSpout,
                ErrorMessage = message.Error,
                SuppressTree = false,
                UpdateRangeList = true,
            };


            //refreshmsg.ToggleAppEnabled(true);
            refreshmsg.SetStatusMessages("", "", appEnabled:true);
            
            mainVM.Loading = true; //so it doesn't react

            mainVM.Project = mdproject;
            mainVM.Loading = false;

            if (mdproject.ProjectFamily == uppProjectFamilies.uopFamMD)
            {
         
               

                if (message.IsImport && mdproject.ProjectType == uppProjectTypes.MDSpout)
                {
                    Message_PromptForRangeInput promptmsg = new(refreshmsg);
                    mainVM.EventAggregator.Publish<Message_PromptForRangeInput>( promptmsg );
                    return;

                   
                }
                
                MDProjectViewModel mdprojectVM = mainVM.MDProjectViewModel;
                if (mdprojectVM != null)
                {
                    mdprojectVM.SuppressEvents = true;
                    mdprojectVM.MDProject = mainVM.MDProject;
                    mainVM.VisibilityMDProject = mdproject.TrayRanges.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
                    mdprojectVM.ParentVM = mainVM;

                    mdprojectVM.SuppressEvents = false;
                }

                mainVM.VisibilityMDProject = Visibility.Visible;
            }
            else 
            {
                mainVM.VisibilityMDProject = Visibility.Collapsed;
            }



            //refresh the display
            refreshmsg.Publish();
          
       
        }

        private void DoWork_SaveFile(object sender, DoWorkEventArgs e)
        {
            Message_Save message = (Message_Save)e.Argument;
            if (message == null) return;
            message.ErrorMessage = "";
            try
            {
                if (!string.IsNullOrWhiteSpace(message.FullPath))
                {
                    if (message.Project == null) return;
                    if (message.ParentVM != null)
                    {
                        message.ParentVM.IsEnabled = false;
                        message.SetStatusMessages("Saving ...", message.FullPath);
                    }
                    
                    

                    message.Project.DataFileName = message.FullPath;
                    if (string.IsNullOrWhiteSpace(message.AppPath)) message.AppPath = System.Configuration.ConfigurationManager.AppSettings.Get("AppResourcePath");

                    if (!Directory.Exists(message.AppPath))
                    {
                        Directory.CreateDirectory(message.AppPath);
                    }

                    if (string.IsNullOrWhiteSpace(message.AppTempFilePath)) message.AppTempFilePath = System.IO.Path.Combine(System.Configuration.ConfigurationManager.AppSettings.Get("AppResourcePath"), $"{message.Project.KeyNumber}-{message.Project.Revision}");

                    if (File.Exists(message.AppTempFilePath))
                    {
                        File.Delete(message.AppTempFilePath);
                    }
                    message.Project.SaveToFile(message.AppTempFilePath, appApplication.AppName, appApplication.AppVersion, message.FullPath);

                    if (File.Exists(message.FullPath))
                        File.Delete(message.FullPath);

                    File.Copy(message.AppTempFilePath, message.FullPath,true);
                    message.FileName = System.IO.Path.GetFileName(message.FullPath);

                    message.Project.DataFileName = message.FullPath;



                }

            }
            catch (Exception exp)
            {
                message.ErrorMessage = exp.Message;
            }


            e.Result = message;

        }

        private void DoWork_SaveFile_Complete(object sender, RunWorkerCompletedEventArgs e)
        {
            Message_Save message = (Message_Save)e.Result;
            if (message == null) return;
            
            message.ToggleAppEnabled(true,"","");

            message.MainVM.UpdateTitle();
            if (message.Project == null) return;

            message.Project.Designer = appApplication.User.Initials;
            message.Project.LastSaveDate = DateTime.Now.ToString();
            if (message.ParentVM != null)
            {
             
                message.ParentVM.ProjectName = MDProject?.Name;

                message.ParentVM.FilePath = message.Project.OutputFolder;
            }
            message.Project.PropValSet("DataFileName", message.FullPath, bSuppressEvnts: true);
            message.Project.HasChanged = false;

            if (!string.IsNullOrWhiteSpace(message.ErrorMessage))
            {
                message.HandleException(MethodBase.GetCurrentMethod(),new Exception(message.ErrorMessage));
            }

        }



        public void RefressMessage_Format(ref Message_Refresh aRefreshControl, bool ?bProjectHasChanged = null)
        {
            if (aRefreshControl == null) return;

            WinTrayMainViewModel mainVM = aRefreshControl.MainVM;

            //aRefreshControl.MainMenuHelper ??= this;
            //if(mainVM == null)
            //{
            //    mainVM = aRefreshControl.MainVM;
            //    if (mainVM == null) mainVM = WinTrayMainViewModel.WinTrayMainViewModelObj;
            //}

            //aRefreshControl.MainVM ??= mainVM;

            if (mainVM.Project == null) aRefreshControl.Clear = true;
            uopProject project = (!aRefreshControl.Clear) ? mainVM.Project : null;
            if (bProjectHasChanged.HasValue && project != null) project.HasChanged =  bProjectHasChanged.Value;

            IProjectViewModel projvm = aRefreshControl.ProjectVM;


            if (project == null && !aRefreshControl.Clear) project = mainVM.Project;

            if (projvm != null)
            {
                MDProjectViewModelBase projvmbase = (MDProjectViewModelBase)projvm;
                aRefreshControl.UnitsToDisplay = projvmbase.DisplayUnits;

            }

            if (aRefreshControl.UnitsToDisplay == uppUnitFamilies.Undefined)
                aRefreshControl.UnitsToDisplay = (project != null) ? project.DisplayUnits : uppUnitFamilies.English;

            bool mainIsBusy = mainVM.IsBusyVisible;

            if (project == null)
                aRefreshControl.Silent = true;
            if (project == null || aRefreshControl.Clear)
            {
                aRefreshControl.Clear = true;
                aRefreshControl.Silent = true;
            }
            //aRefreshControl.ProjectViewModel = projvm;
            if (string.IsNullOrWhiteSpace(aRefreshControl.SelectedRangeGUID) && project != null && !aRefreshControl.Clear)
                aRefreshControl.SelectedRangeGUID = project.TrayRanges.SelectedRangeGUID;
            //aRefreshControl.ProjectVM = projvm;
            if (projvm != null)
            {

                if (!aRefreshControl.SuppressDocumentClosure || aRefreshControl.Clear)
                    projvm.CloseAllDocuments();

                //if (aRefreshControl.RangeNameChanged && !aRefreshControl.Clear && project != null)
                //    projvm.UpdateTrayList();
            }
            else
            {
                aRefreshControl.SuppressDataGrids = true;
                aRefreshControl.SuppressPropertyLists = true;
                aRefreshControl.SuppressImage = true;

            }
        }



        public async void Execute_RefreshDisplay(Message_Refresh aRefreshControl = null, bool? bProjectHasChanged = null , bool? bRefreshTrayLists = null)
        {
            Message_Refresh refresh = aRefreshControl == null ? new Message_Refresh() : aRefreshControl;
            WinTrayMainViewModel mainVM = refresh.MainVM;
            if (mainVM.Unloading || mainVM.Loading || Refreshing)
                return;

            if (bRefreshTrayLists.HasValue) refresh.UpdateRangeList = bRefreshTrayLists.Value;
      
            Refreshing = true;

            // get the project view model
            IProjectViewModel projectVM = mainVM.ProjectVM;
            MDProjectViewModel mdVM = mainVM.ProjectFamily == uppProjectFamilies.uopFamMD ? (MDProjectViewModel)projectVM: null;
            if(projectVM == null)
            {
                refresh.UpdateRangeList = false;
                refresh.SuppressImage = true;
                refresh.SuppressDocumentClosure = true;
            }
            if (refresh.RangeNameChanged) 
            {
                refresh.UpdateRangeList = true;
                refresh.SuppressTree = false;
            }
               
            uopProject project = mainVM.Project;
            refresh.ToggleAppEnabled(true, "", "");
            try
            {
                if (project == null || refresh.Clear)
                {
                    // just clear all the controls
                    refresh.PublishMessage<Message_ClearData>( new Message_ClearData());
                    refresh.PartTypeList.Clear();
                }
                else
                {
                    projectVM?.ToggleViewerVisibility(false, true);
                    if (bProjectHasChanged.HasValue) project.HasChanged = bProjectHasChanged.Value;
                    //listen for project events 
                    if (!refresh.Silent) refresh.ListenToProject = project;
                    mainVM.UpdateVisibily();

                    //================= CLOSE THE OPEN DOCS ============================
                    if (!refresh.SuppressDocumentClosure)
                    {
                        projectVM.CloseAllDocuments();
                        refresh.SuppressDocumentClosure = true;
                    }
                     
                    RefressMessage_Format(ref refresh, bProjectHasChanged);
                    Refreshing = true;
                  
                    string action = refresh.Clear ? "Clearing" : "Updating";
                    string basemsg = $"{action} Project Data";
                    refresh.StatusMessage = basemsg;
                    refresh.ToggleAppEnabled(false, aBusyMessage: refresh.StatusMessage);

                    //================= UPDATE PROJECT PARTS ============================
                    try
                    {
                        refresh.ToggleAppEnabled(false, "Updating Project Parts", "");
                        await Task_UpdateProjectPartsAsync(refresh);
                    }
                    catch (Exception ex) { refresh.HandleException(MethodBase.GetCurrentMethod(), ex); }
                    finally { refresh.ToggleAppEnabled(false, basemsg, ""); }
                    
                    bool doitall = !refresh.SuppressTree && !refresh.SuppressPropertyLists && !refresh.SuppressDataGrids && !refresh.SuppressImage;
                    if (doitall) 
                    {
                        if(refresh.PartTypeList.Count <=0)
                        refresh.PublishMessage<Message_ClearData>(new Message_ClearData());

                    }


                    //================= UPDATE THE RANGE LIST ============================
                    if (refresh.UpdateRangeList || doitall || refresh.RangeNameChanged)
                    {
                        bool newlist = false;
                        try
                        {
                            newlist = projectVM.UpdateTrayList(project.SelectedRangeGUID);
                            //refresh.PublishMessage<Message_RefreshRangeList>(new Message_RefreshRangeList(rangename, bSuppressRefresh: true) { });
                        }
                        catch (Exception ex) { refresh.HandleException(MethodBase.GetCurrentMethod(), ex); }
                        finally { refresh.UpdateRangeList = false; if (newlist) refresh.SuppressTree = false; };
                    }
                    //================= UPDATE THE TREE VIEW ============================
                    if (!refresh.SuppressTree || doitall)
                    {
                        //asyncronise part
                        try
                        {
                            refresh.ToggleAppEnabled(false, "Build Tree View Nodes", "");
                            await Task_CollectTreeNodesAsync(refresh);
                        }
                        catch (Exception ex) { refresh.HandleException(MethodBase.GetCurrentMethod(), ex); }
                        //update UI cotnrols
                        try
                        {
                            refresh.ToggleAppEnabled(false, $"Loading Project Tree View", "");
                            refresh.Publish_TreeView(false);
                        }
                        catch (Exception ex) { refresh.HandleException(MethodBase.GetCurrentMethod(), ex); }
                        finally
                        { refresh.ToggleAppEnabled(false, basemsg, ""); refresh.TreeNodes = null; refresh.SuppressTree = true; }
                    
                 
                    }
                    //================= UPDATE THE PROPERTY LISTS ============================
                    if (!refresh.SuppressPropertyLists || doitall)
                    {
                        //asyncronise part
                        try
                        {
                            refresh.ToggleAppEnabled(false, aBusyMessage: $"{action} Project Property Lists", aSubStatus: "");
                            await Task_CollectPropertyListsAsync(refresh);
                        }
                        catch(Exception ex) { refresh.HandleException(MethodBase.GetCurrentMethod(), ex); }
                        //update UI cotnrols
                        try
                        {
                            refresh.ToggleAppEnabled(false, $"Loading Property Lists");
                            if (refresh.PropertyListProperties != null)
                            {
                                List<uopProperties> props = refresh.PropertyListProperties;
                                foreach (var item in props)
                                {
                                    try
                                    {
                                        refresh.ToggleAppEnabled(false, null, $"Loading {item.PartType.GetDescription()} Property List");
                                        Message_UpdatePropertyList message = new(refresh) { PartType = item.PartType, Properties = item };
                                        message.Publish();
                                    }
                                    catch (Exception ex) { refresh.HandleException(MethodBase.GetCurrentMethod(), ex); }
                                }
                            }
                        }
                        catch (Exception ex) { refresh.HandleException(MethodBase.GetCurrentMethod(), ex); }
                        finally { refresh.PropertyListProperties = null;  refresh.ToggleAppEnabled(false, basemsg, ""); }
                    }

                    //================= UPDATE THE DATA TABLES ============================
                    if (!refresh.SuppressDataGrids || doitall)
                    {
                        //asyncronise part
                        try
                        {
                            refresh.ToggleAppEnabled(false, aBusyMessage: $"{action} Data Tables", aSubStatus: "");
                            await Task_CollectDataGridDataAsync(refresh);
                        }
                        catch (Exception ex) { refresh.HandleException(MethodBase.GetCurrentMethod(), ex); }


                        //update UI cotnrols
                        try
                        {
                            if (refresh.DataTables != null)
                            {
                                refresh.ToggleAppEnabled(false, $"Loading Data Table");
                                List<uopTable> tables = refresh.DataTables;
                                foreach (var item in tables)
                                {
                                    try
                                    {
                                        refresh.ToggleAppEnabled(false, null, $"Loading {item.PartType.GetDescription()} Table");
                                        Message_RefreshDataGrid message = new(refresh) { PartType = item.PartType, DataTable = item };
                                        message.Publish();
                                    }
                                    catch (Exception ex) { refresh.HandleException(MethodBase.GetCurrentMethod(), ex); }
                                }
                            }


                        }
                        catch (Exception ex) { refresh.HandleException(MethodBase.GetCurrentMethod(), ex); }
                        finally { refresh.ToggleAppEnabled(false, basemsg, ""); refresh.DataTables = null; refresh.SuppressDataGrids = true; }
                    }

                    mainVM.UpdateVisibily();

                    bool doimage = !refresh.SuppressImage || doitall;
                    //================= UPDATE THE INPUT SKETCH ============================

                    if (doimage)
                    {
                        basemsg = $"Generating Project Input Sketch";
                        refresh.StatusMessage = basemsg;
                       
                        //asyncronise part
                        try
                        {
                            refresh.ToggleAppEnabled(false, basemsg, "");
                            IProjectViewModel projVM = await Task_CreateInputSketchAsync(refresh);

                        }
                        catch (Exception ex) { refresh.HandleException(MethodBase.GetCurrentMethod(), ex); }


                        //update UI cotnrols
                        try
                        {
                            if (mdVM != null)
                            {
                                refresh.ToggleAppEnabled(false, $"Displaying Project Input Sketch", "");
                                mdVM.Task_ShowInputSketch(refresh);
                                
                            }
                        }
                        catch (Exception ex) { refresh.HandleException(MethodBase.GetCurrentMethod(), ex); }
                        finally { refresh.ToggleAppEnabled(false, basemsg, "");}
                    }
                        
              


                }
            }
            catch (Exception ex) { refresh.HandleException(System.Reflection.MethodBase.GetCurrentMethod(), ex); }

            finally
            {
                Refreshing = false;
                projectVM?.ToggleViewerVisibility(true, false);
                mainVM.SuppressEvents = false;
                refresh.ToggleAppEnabled(true, "", "");

                mainVM.UpdateTitle();
                mainVM.UpdateVisibily();

                refresh.ShowWarnings();
                refresh.ReleaseReferences();
            }
  
        }
        
        private async Task Task_CollectPropertyListsAsync(Message_Refresh refresh)
        {

            if (refresh == null) return;
            refresh.PropertyListProperties ??= new List<uopProperties>();

            uopProject project = refresh.Project;
            if (project == null)
            {
                refresh.PropertyListProperties = null;
                return;
            }


            //if (refresh.PropertyListProperties.Count == 0) refresh.SuppressPropertyLists = false;

            if (refresh.SuppressPropertyLists) return;

            await Task.Run(() =>
            {

                try
                {

                    List<uopProperties> props = refresh.GetProperties();
                    
                }
                catch (Exception ex)
                {

                    refresh.HandleException(MethodBase.GetCurrentMethod(), ex);
                   
                }
                finally
                {

                    refresh.SuppressPropertyLists = true;
                }



            });

        }

        private async Task<IProjectViewModel> Task_CreateInputSketchAsync(Message_Refresh refresh)
        {
            IProjectViewModel _rVal = null;
            if (refresh == null) return null;
            uopProject project = refresh.Project;
            if (project == null)
            {
                refresh.Image = null;
                return null;
            }

            if (refresh.SuppressImage) return null;
            await Task.Run(async () =>
            {

                if (refresh.Image != null) refresh.Image.Dispose();
                refresh.Image = null;

                try
                {
                    WinTrayMainViewModel mainVM = refresh.MainVM;
                    if (project.ProjectFamily == uppProjectFamilies.uopFamMD)
                    {
                        refresh.Range ??= project.TrayRanges.SelectedRange;
                    
                        MDProjectViewModel projVM = mainVM.MDProjectViewModel;
                        _rVal = projVM;
                        await projVM.Task_GenerateInputSketch(refresh);
                    }

                }
                catch (Exception ex)
                {

                    refresh.HandleException(System.Reflection.MethodBase.GetCurrentMethod(), ex);
                }
                finally 
                {
                    refresh.SuppressImage = true;
                }
                

            });

            return _rVal;
        }

        private async Task Task_CollectDataGridDataAsync(Message_Refresh refresh)
        {

            if (refresh == null) return;
            refresh.DataTables ??= new List<uopTable>();

            uopProject project = refresh.Project;
            if (project == null)
            {
                refresh.DataTables = null;
                return;
            }


            

            if (refresh.SuppressDataGrids) return;
            
            await Task.Run(() =>
            {

                try
                {
                    List<uopTable> tables = refresh.GetDataTables();
                    
                }
                catch (Exception ex)
                {

                    refresh.HandleException(MethodBase.GetCurrentMethod(), ex);

                }
                finally
                {

                    refresh.SuppressDataGrids = true;
                }



            });

        }


        private async Task Task_CollectTreeNodesAsync(Message_Refresh refresh)
        {

            if (refresh == null) return;
            refresh.TreeNodes ??= new ObservableCollection<TreeViewNode>();

            uopProject project = refresh.Project;
            if (project == null) 
                return;

       
            if (refresh.TreeNodes.Count == 0) refresh.SuppressTree = false;

            if (refresh.SuppressTree) return;

            //string statuswuz = refresh.StatusMessage;
            //if (string.IsNullOrWhiteSpace(statuswuz)) statuswuz = refresh.MainVM.StatusMessage1;

            await Task.Run(() =>
            {

                try
                {

                    WinTrayMainViewModel mainVM = refresh.MainVM;

                    refresh.TreeNodes = mainVM.CreateTreeNodes(refresh);

                }
                catch (Exception ex)
                {

                    refresh.HandleException(MethodBase.GetCurrentMethod(), ex);

                }
                finally
                {
                    refresh.SuppressTree = true;

                }



            });

        }

        private async Task<bool> Task_SaveFileAsync(Message_Save message)
        {
            if (message == null) return false;
            if (message.Project == null) return false;
            bool _rVal = true;


                await Task.Run(() =>
            {
                message.Warnings = new uopDocuments();
                try
                {

                    message.ToggleAppEnabled(false, "Saving ...", message.FullPath);


                    message.SaveCompleted = true;
                    message.Project.DataFileName = message.FullPath;
                    if (string.IsNullOrWhiteSpace(message.AppPath)) message.AppPath = System.Configuration.ConfigurationManager.AppSettings.Get("AppResourcePath");

                    if (!Directory.Exists(message.AppPath))
                    {
                        Directory.CreateDirectory(message.AppPath);
                    }

                    if (string.IsNullOrWhiteSpace(message.AppTempFilePath)) message.AppTempFilePath = System.IO.Path.Combine(System.Configuration.ConfigurationManager.AppSettings.Get("AppResourcePath"), $"{message.Project.KeyNumber}-{message.Project.Revision}");

                    if (File.Exists(message.AppTempFilePath))
                    {
                        File.Delete(message.AppTempFilePath);
                    }
                    message.Project.SaveToFile(message.AppTempFilePath, appApplication.AppName, appApplication.AppVersion, message.FullPath);

                    if (File.Exists(message.FullPath))
                        File.Delete(message.FullPath);

                    File.Copy(message.AppTempFilePath, message.FullPath, true);
                    message.FileName = System.IO.Path.GetFileName(message.FullPath);

                    message.Project.DataFileName = message.FullPath;
                  

                }
                catch (Exception exp)
                {
                    message.HandleException(MethodBase.GetCurrentMethod(), exp);
                    
                    _rVal = false;
                }
                finally
                {
                    message.ToggleAppEnabled(true, "", "");
                    message.SaveCompleted = _rVal;
                }
             
               

                return _rVal;
            });

            return _rVal;

        }

        private async Task <uopProject>  Task_OpenFileAsync(Message_OpenFile message)
        {
            if (message == null) return null;
            if (message.Warnings.Count > 0) return null;
                WinTrayMainViewModel mainVM = message.MainVM;
            uopProject _rVal = null;

            await Task.Run(() =>
            {
                uopProject project = message.Project;
                uopDocuments readwarnings = new();
                string filename = message.FileName;
                try
                {


                    if (message.ProjectFamily == uppProjectFamilies.uopFamMD)
                    {

                        string fname = System.IO.Path.GetFileNameWithoutExtension(filename);
                        //set the messages and disable the app
                        message.ToggleAppEnabled(false, $"Opening WinTray File {fname}", filename);


                        message.Project = new mdProject(); ;
                        message.ListenToProject = message.Project;

                        mainVM.Loading = true;
                        bool converttoMDD = message.Convert;
                        // read the file data, catch and collect the warnings and the message.Project read status changes
                        message.Project.ReadFromFile(filename, "WinTray", appApplication.AppVersion, out readwarnings);


                        if (converttoMDD)
                        {
                            message.ToggleAppEnabled(false, $"Converting MD Spout Project {fname}  to MD Draw", "");

                            message.MDProject.ConvertToMDD();
                            message.Project.PropValSet("ImportFileName", filename, bSuppressEvnts: true);

                            message.ToggleAppEnabled(false, null, $"{fname} Conversion Complete");
                        }
                        message.SelectedRangeGUID = message.Project.SelectedRangeGUID;

                    }



                }
                catch (Exception ex)
                {
                    message.HandleException(MethodBase.GetCurrentMethod(), ex);

                }
                finally
                {
                    message.ListenToProject = null;
                    _rVal = message.Project;
                    message.Warnings.Append(readwarnings);


                }
                return _rVal;
            });

            return _rVal;



        }
        private async Task Task_UpdateProjectPartsAsync(Message_Refresh refresh) 
        {

            if (refresh == null) return;
            uopProject project = refresh.Project;
            if (project == null) return;
            await Task.Run(() =>
            {
            try
            {
                    uopTrayRange range = null;
                    if (refresh.ResetComponents)
                    {
                        range = project.SelectedRange;
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(refresh.DirtyRange))   range = project.TrayRanges.Item(refresh.DirtyRange);
                    }
                    if (range != null)
                    {
                        refresh.SetStatusMessages(null, $"Reseting Range {range.TrayName()} Persistent Sub Parts");
                        range.Assembly.ResetComponents();
                        refresh.SetStatusMessages(null, $"Updating Range {range.TrayName()} Persistent Sub Parts");
                        project.UpdatePersistentSubParts(aRangeGUID: range.GUID);
                    }

                    project.UpdateParts(refresh.UpdateParts);
                   
                }
                catch (Exception ex)
                {

                    refresh.HandleException(MethodBase.GetCurrentMethod(), ex);
                    throw;
                }
                finally
                {

                }
            


            });
        
        }

     

        /// <summary>
        /// Close the current Project.
        /// </summary>
        public void Execute_Close()
        {
            if (!WinTrayMainViewModel.WinTrayMainViewModelObj.IsEnabled) return;
            IsProjectClosed = false;
            if (IsProjectLoaded)
            {
                if (Project.HasChanged)
                {
                    if (MessageBoxResult.No == ParentVM.ShowMessageBox("Do you want to close the Project? \n\n All the unsaved changes will be lost!",  "Close Project", MessageBoxButton.YesNo, MessageBoxImage.Question))
                    {
                        return;
                    }
                }
                Execute_UnloadProject();


            }
            //else
            //{
            //    if (!Convert.ToBoolean(param))
            //    {
            //        ParentVM.ShowMessageBox("No Project Loaded", "Wintray", MessageBoxButton.OK, MessageBoxImage.Information);
            //    }
            //    else { IsProjectClosed = true; }
            //}
        }

        /// <summary>
        /// Unload the MDProject properties.
        /// </summary>
        public void Execute_UnloadProject()
        {
            WinTrayMainViewModel mainVM = ParentVM;
            IEventAggregator eventAggtor = mainVM == null ? ApplicationModule.Instance.Resolve<IEventAggregator>() : mainVM.EventAggregator;
            try
            {

                uopProject project = mainVM.Project;

                mainVM.Project = null;
                
                ToggleAppEnabled(false,"","");
                mainVM.UpdateVisibily();
                Refreshing = false;
                mainVM.ClearAll(bSuppressImage: false, bSilent: true);
                mainVM.Unloading = true;

                //this should cause the project view models to release reference to the project
                eventAggtor.Publish<Message_UnloadProject>(new Message_UnloadProject());

                project?.Destroy();

                if (mainVM.ProjectVM != null) mainVM.ProjectVM.Dispose();


            }
            catch
            {

            }
            finally
            {

                mainVM.MDProject = null;
                //mainVM.ProjectCollection?.Clear();

                ParentVM.ProjectName = string.Empty;
                mainVM.FilePath = string.Empty;


                //NotifyPropertyChanged("ProjectCollection");
                mainVM.Unloading = false;

                IsProjectClosed = true;
                ToggleAppEnabled(true);
            }

        }

        public void ToggleAppEnabled(bool bEnabledVal,  string aBusyMessage = null, string aSubStatus = null)
        {

            EventAggregator?.Publish<Message_ToggleAppEnabled>(new Message_ToggleAppEnabled(bEnabledVal,  aBusyMessage, aSubStatus));
          
        }
        public void CloseAll()
        {
        }

        /// <summary>
        /// Import data from selected file type
        /// </summary>
        private async void Execute_Import()
        {
          
            Message_OpenFile message = new("");
            
            WinTrayMainViewModel mainVM = message.MainVM;
            if (mainVM == null) return;
            uopProject project = mainVM.Project;
       
            try
            {

              
                if (project == null)
                {
                    message.AddWarning(null, "Import Warning", $"A new project must be created before data can be imported!");
                    return;
                }
                if(project.ProjectFamily != uppProjectFamilies.uopFamMD)
                {
                    message.AddWarning(null, "Import Warning", $"Project Type '{project.ProjectTypeName}' Does Not Support Importation!");
                    return;
                }
                
                if (project == null)
                {
                    message.AddWarning(project, project.ProjectType == uppProjectTypes.MDSpout ? "Stage Import Warning" : "MDP Import Warning", "A new MD Project must be created before data can be imported!");
                    return;
                }
                if (project.TrayRanges.Count > 0)
                {
                    message.AddWarning(project, project.ProjectType == uppProjectTypes.MDSpout ? "Stage Import Warning" : "MDP Import Warning", "Data can only be imported into projects With no defined tray sections!");
                    return;
                }

                message.ToggleAppEnabled(false, "Connecting to Q drive...", "");
             
                string folderPath = string.IsNullOrWhiteSpace(project.ImportFileName) ? project.ProjectType == uppProjectTypes.MDSpout ? appApplication.GetInquiryFolder(project) : appApplication.MechanicalPath: System.IO.Path.GetDirectoryName(project.ImportFileName);
                message.ToggleAppEnabled(true, "", "");
               

                OpenFileDialogSettings setting = new()
                {
                    Title = string.Format($"Select File To Import"),
                    CheckFileExists = true,
                    Filter = project.ImportFileFilter,  // "MD Hydraulics Files (*.MDH)|*.mdh",
                    InitialDirectory = !string.IsNullOrWhiteSpace(folderPath) ? folderPath : "",
                };
                bool? result = _DialogService.ShowOpenFileDialog(mainVM, setting);

                if ((bool)result == false) return;

              
                project.ImportFileName = setting.FileName;
                message.Project = project;
                message.FileName = setting.FileName;
                await message.Task_ImportFileAsync();
             

            }
            catch (Exception e)
            {
                message.HandleException(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }
            finally
            {
                message.ListenToProject = null;
                mainVM.Loading = false;
                    message.ToggleAppEnabled(true,"", "");
                mainVM.UpdateVisibily();

                Message_Refresh refresh = new Message_Refresh() { Warnings = message.Warnings };
                


                if (project.ProjectType == uppProjectTypes.MDSpout)
                {
                    Message_PromptForRangeInput promptmsg = new(refresh);
                    mainVM.EventAggregator.Publish<Message_PromptForRangeInput>(promptmsg);
                   

                }
                Execute_RefreshDisplay(refresh, true, true);
                
                }


            }
        


        /// <summary>
        /// Export to selected file type
        /// </summary>
        public void Execute_Export()
        {
            uopProject project = Project;
            if (project == null)
                return;

            if (project.HasChanged)
            {
                ParentVM.ShowMessageBox($"Project {project.ProjectName} Must Be Saved Before It Can Be Exported", "Unsaved Changes Detected", icon: MessageBoxImage.Information);
                return;
            }
            string defname = project.DefaultFileName;
            string fullPath = string.Empty;
            string fileSpec = project.ImportFileName;
            string curName = string.Empty;
            string curFolder = string.Empty;


            if (!string.IsNullOrEmpty(fileSpec))
            {
                if (File.Exists(fileSpec))
                {
                    curName = System.IO.Path.GetFileName(fileSpec);
                    curFolder = System.IO.Path.GetDirectoryName(fileSpec);
                    defname = curName;
                }
            }
            if (string.IsNullOrEmpty(curName)) curName = defname;
            //get the folder
            if (string.IsNullOrEmpty(curFolder))
            {
                curFolder = project.OutputFolder;
                if (!string.IsNullOrEmpty(curFolder))
                    if (!Directory.Exists(curFolder)) curFolder = string.Empty;
            }
            if (string.IsNullOrEmpty(curFolder))
            {
                curFolder = project.ImportFileName;
                if (!string.IsNullOrEmpty(curFolder))
                    curFolder = System.IO.Path.GetDirectoryName(curFolder);
                if (!Directory.Exists(curFolder)) curFolder = string.Empty;
            }
            if (!string.IsNullOrEmpty(curFolder))
            {
                if (!Directory.Exists(curFolder)) curFolder = "C:";

            }
            else
            {
                curFolder = "C:";
            }

            bool SaveIt = false;
            using var dlg = new Ookii.Dialogs.WinForms.VistaFolderBrowserDialog();
            dlg.SelectedPath = curFolder;
            dlg.Description = $"Select Output Folder For '{ curName }'";
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                curFolder = dlg.SelectedPath;
                fullPath = System.IO.Path.Combine(curFolder, curName);
                SaveIt = true;
                //confirm overwrite
                if (File.Exists(fullPath))
                {
                    if (ParentVM.ShowMessageBox("Overwrite Existing Project File?", "Overwrite File?", MessageBoxButton.YesNo) == MessageBoxResult.No)
                    {
                        SaveIt = false;
                    }
                }
            }

            if (!SaveIt) return;

            if (project.ProjectType == uppProjectTypes.MDSpout)
            {
                ExportMDH(fullPath, (mdProject)project);

            }


        }

        /// <summary>
        /// Export to MDH file type
        /// </summary>
        private void ExportMDH(string fullPath, mdProject aProject)
        {
            try
            {
               
                List<string> lines;
                string txt;

                List<mdStage> stages = aProject.UpdatedStages();
                if (stages.Count <= 0)
                {
                    ParentVM.ShowMessageBox("Unable To Create MDH File.  No Hydraulic Stage Definitions Detected in Current Project.", "Export", icon: MessageBoxImage.Information);
                    return;
                }

                using StreamWriter sw = File.CreateText(fullPath);
                foreach (mdStage stage in stages)
                {
                    if (stage == null) continue;
                    lines = stage.SaveLines;
                    for (int j = 0; j < lines.Count; j++)
                    {
                        txt = lines[j];
                        sw.WriteLine(txt);
                    }
                }
                ParentVM.ShowMessageBox($"Project {aProject.ProjectName} Data Exported To {fullPath}", "Data Export Successfull", icon: MessageBoxImage.Information);
            }
            catch (IOException ioExptn)
            {
                LoggerManager log = new();
                log.LogError(DateTime.Now + " " + ioExptn.Message);
            }
            catch (Exception exception)
            {
                LoggerManager log = new();
                log.LogError(DateTime.Now + " " + exception.Message);
            }
        }

        public  async void Execute_Save()
        {
            bool _rVal = await Execute_SaveCurrentProjectAsyc(false);
     
        }

        public async void Execute_SaveAs()
        {
            bool _rVal = await Execute_SaveCurrentProjectAsyc(true);
        
        }

   
        /// <summary>
        /// saves the current MDProject (or the passed MDProject) to a MDProject text file
        /// </summary>
        /// <param name="Saveas">flag indicating if the user should be prompted for a filename</param>
        /// <returns></returns>
        public async Task<bool> Execute_SaveCurrentProjectAsyc(bool bSaveAs = false)
        {
      
            WinTrayMainViewModel mainVM = ParentVM;
            if (mainVM == null) return false;
            uopProject project = mainVM.Project;
            if (project is null) return false;

            if (project.TrayRanges.Count <= 0)
            {
                mainVM.ShowMessageBox("A Project Must Have At Least One Tray Section Defined Before It Can Be Saved", "No Defined Tray Sections Detected", icon: MessageBoxImage.Error);
                return false;
            }


            Message_Save message = new(project);
            bool _rVal = false;
            bool SaveIt = false;
            string fileSpec = project.DataFileName;
            string defname = project.DefaultFileName;
            bool forcename = project.ProjectType == uppProjectTypes.MDSpout;
            string fullPath = string.Empty;
            string curName = string.Empty;
            string curFolder = project.OutputFolder;
            if (!curFolder.EndsWith("\\")) curFolder += "\\";

            string temppath = System.IO.Path.GetTempPath();
            string defaultfolder = System.Configuration.ConfigurationManager.AppSettings.Get("AppResourcePath");
            if (curFolder.StartsWith(temppath, StringComparison.OrdinalIgnoreCase))
            {
                curFolder = defaultfolder;
                bSaveAs = true;
                fileSpec = System.IO.Path.Combine(curFolder, defname);
            }
            temppath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), defname);
            try
            {
                if (string.IsNullOrEmpty(project.ProjectName))
                {
                    message.ShowMessageBox("Wintray Projects must have a UOP Key Number assigned before they can be saved.", "Key Number Required", icon: MessageBoxImage.Error);
                    return false;
                }
                // create the file name

                bool nopath = string.IsNullOrWhiteSpace(fileSpec);
                if (nopath) bSaveAs = true;
                {
                    if (nopath && !string.IsNullOrWhiteSpace(curFolder))
                        fileSpec = System.IO.Path.Combine(curFolder, defname);
                }

                //get the file name
                if (!string.IsNullOrWhiteSpace(fileSpec))
                {
                    if (File.Exists(fileSpec))
                    {
                        curName = System.IO.Path.GetFileName(fileSpec);
                        curFolder = System.IO.Path.GetDirectoryName(fileSpec);
                        defname = curName;
                    }
                }
                if (string.IsNullOrWhiteSpace(curName) || forcename) curName = defname;

                //get the folder
                if (string.IsNullOrEmpty(curFolder))
                {
                    bSaveAs = true;
                    curFolder = project.OutputFolder;
                    if (!string.IsNullOrEmpty(curFolder))
                        if (!Directory.Exists(curFolder)) curFolder = string.Empty;
                }
                if (string.IsNullOrEmpty(curFolder))
                {
                    curFolder = project.ImportFileName;
                    if (!string.IsNullOrEmpty(curFolder))
                        curFolder = System.IO.Path.GetDirectoryName(curFolder);
                    if (!Directory.Exists(curFolder)) curFolder = string.Empty;
                }
                if (string.IsNullOrEmpty(curFolder))
                {
                    //if (project.ProjectType == uppProjectTypes.MDDraw || project.ProjectType == uppProjectTypes.CrossFlow)
                    //{
                    //    curFolder = ThisApp.MechanicalPath;
                    //}
                    //else if (project.ProjectType == uppProjectTypes.MDSpout)
                    //{
                    //    curFolder = ThisApp.FunctionalPath;
                    //}

                    //if (!string.IsNullOrEmpty(curFolder))
                    //    if (!Directory.Exists(curFolder)) curFolder = string.Empty;
                    //if (string.Compare(curFolder, ThisApp.FunctionalPath, true) == 0)
                    //    if (goUser.IsEngineer || project.ProjectType = uppProjectTypes.MDSpout)
                    //        curFolder = InquiryFolder(project);
                }
                if (!string.IsNullOrEmpty(curFolder))
                {
                    if (!Directory.Exists(curFolder))
                    {
                        curFolder = string.Empty;
                        bSaveAs = true;
                        curFolder = "C:";
                    }
                }
                else
                {
                    bSaveAs = true;
                    curFolder = "C:";
                }
                //prompt for folder
                if (bSaveAs)
                {
                    if (appApplication.User.IsDesigner)
                    {
                        using var dlg = new Ookii.Dialogs.WinForms.VistaSaveFileDialog();
                        dlg.Filter = $"Project File (*.{project.ProjectFileExtension})|*.{project.ProjectFileExtension}";
                        dlg.DefaultExt = $"*.{project.ProjectFileExtension}";
                        dlg.FileName = System.IO.Path.Combine(curFolder, curName);
                        dlg.OverwritePrompt = true;
                        dlg.Title = "Save As";
                        if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            fullPath = dlg.FileName;
                            curFolder = System.IO.Path.GetDirectoryName(dlg.FileName);
                            curName = System.IO.Path.GetFileName(dlg.FileName);
                            SaveIt = true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        using var dlg = new Ookii.Dialogs.WinForms.VistaFolderBrowserDialog();
                        dlg.SelectedPath = curFolder;
                        dlg.Description = $"Select Project Output Folder For File '{ curName }'";
                        if (dlg.ShowDialog().ToString() == "OK")
                        {
                            curFolder = dlg.SelectedPath;
                            fullPath = System.IO.Path.Combine(curFolder, curName);
                            SaveIt = true;
                            //confirm overwrite
                            if (File.Exists(fullPath))
                            {
                                if (message.ShowMessageBox("Overwrite Existing Project File?", "Overwrite File?", MessageBoxButton.YesNo) == MessageBoxResult.No)
                                    return false;
                            }
                        }
                    }

                }
                else
                {
                    //good path so save with no prompt
                    fullPath = System.IO.Path.Combine(curFolder, curName);
                    if (project.DataFileName.ToUpper() != fullPath.ToUpper() && File.Exists(fullPath))
                    {
                        if (message.ShowMessageBox("Overwrite Existing Project File?", "Overwrite File?", MessageBoxButton.YesNo) == MessageBoxResult.No)
                            return false;
                    }

                    SaveIt = true;
                }

                if (SaveIt)
                {
                    uopGlobals.LastOpenedFile = fullPath;
                    message.Project = project;
                    message.FullPath = fullPath;
                    message.AppTempPath = System.IO.Path.GetTempPath(); // System.Configuration.ConfigurationManager.AppSettings.Get("AppResourcePath"),
                    message.AppTempFilePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), curName);

                    _rVal  =await Task_SaveFileAsync(message);

                }

            }
            catch (IOException ioExptn)
            {
                ApplicationLogger.Instance.LogError(ioExptn);
                 message.HandleException( MethodBase.GetCurrentMethod(), new Exception("The file could not be saved successfully. Please check access to the folder and try again."));
            }
            catch (Exception exception)
            {
                message.HandleException(MethodBase.GetCurrentMethod(), new Exception("The file could not be saved successfully"));
                ApplicationLogger.Instance.LogError(exception);
            }
            finally
            {
                if (SaveIt)
                {
                   
                    message.ToggleAppEnabled(true, "", "");
                    message.ShowWarnings();

                }

            }
        
            return SaveIt;
        }

        /// <summary>
        /// Close the application
        /// </summary>
        public void Execute_Exit(System.ComponentModel.CancelEventArgs e = null)
        {
            if (!ParentVM.IsEnabled) return;
            if (IsProjectLoaded)
            {
                string msg = "Do you want to close the Application?";
                if (Project.HasChanged)
                {
                    msg += "\n\n All the unsaved changes will be lost!";

                    if (ParentVM.ShowMessageBox(msg, "Exit Application", MessageBoxButton.YesNo, MessageBoxImage.Question, defaultResult: MessageBoxResult.No) == MessageBoxResult.No) 
                    {
                        if (e != null) e.Cancel = true;
                        return;
                    }
                        


                }
                ParentVM.Project = null;
                Application.Current?.Shutdown();

            }
        }

            /// <summary>
            ///  #1the MDProject to add a range to
            /// ^shows the wizard to add a tray range to the MDProject
            /// </summary>
            /// <returns></returns>
            public bool Execute_Add_Trays()
        {
            var newSectionVM = ApplicationModule.Instance.Resolve<Edit_MDRange_ViewModel>(new ParameterOverride[] {
                                                                                                        new ParameterOverride("fieldName", "RingStart"),
                                                                                                        new ParameterOverride("isNewTray", true)});
            var res = _DialogService.ShowDialog<Edit_MDRange_View>(this.ParentVM, newSectionVM);

            if (res == true)
            {
                var mainVM = ParentVM;
            

                NotifyPropertyChanged("Project Collection");
                Mouse.OverrideCursor = null;
               
                Message_Refresh refresh = new(bSuppressTree: false);
                refresh.Publish();



            }
            MDProject?.ReadStatus(string.Empty);
            return true;
        }

        /// <summary>
        /// Delete selected tray ranges
        /// </summary>
        public void Execute_Delete_Trays()
        {

            if (Project == null) return;
            colUOPTrayRanges pRanges = Project.TrayRanges;
            if (pRanges.Count <= 0) return;

            bool refreshrequired = false;


            //ParameterOverride[] inputParams = new ParameterOverride[] 
            //{ 
            //    new ParameterOverride("selectionSet", Project.TrayRanges), 
            //    new ParameterOverride("dialogueCaption", "Select Trays To Delete"),
            //    new ParameterOverride("eventAggregator", EventAggregator),
            //    new ParameterOverride("dialogService",DialogService)

            //};

            List<string> traynames = new();

            foreach (uopTrayRange item in pRanges)
            {
                traynames.Add(item.SelectName);
            }

            mzQuestions query = new("Select Trays To Delete");
            mzQuestion q = query.AddMultiSelect("Select Trays", traynames, "");
            bool cancelit =!query.PromptForAnswers("");

            traynames = q.AnswersList();
            if (traynames.Count <= 0) return;



            try
            {

                if (ParentVM.ShowMessageBox($"Delete {traynames.Count} Trays?", "Confirm Tray Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }

                pRanges.SuppressEvents = true;


                List<uopTrayRange> aCol = new();
                foreach (string item in traynames)
                {
                    aCol.Add(pRanges.Find((x) => x.SelectName == item));
                }



                int indexToRemove;



                string selectedRangeGUID = pRanges.SelectedRangeGUID;

                foreach (uopTrayRange item in aCol)
                {
                    indexToRemove = pRanges.IndexOf(item);
                    if (indexToRemove > 0)
                    {

                        if (item.GUID == selectedRangeGUID) selectedRangeGUID = "";
                        pRanges.Remove(indexToRemove);
                        refreshrequired = true;
                    }
                }




            }
            catch { refreshrequired = true; }
            finally
            {
                pRanges.SuppressEvents = false;
                if (refreshrequired)
                {



                    Execute_RefreshDisplay(null, true, true);
                }



            }





        }
        /// <summary>
        /// Enable or disable tray menu
        /// </summary>
        /// <param name="isEnable"></param>

        private mzQuestions _RefreshQuery = null;

        public void Execute_Refresh()
        {

            try
            {
                WinTrayMainViewModel mainVM = ParentVM;

                uopProject proj = mainVM.Project;
                if (proj == null) return;


                System.Diagnostics.Debug.WriteLine("REFRESH");
               
               
                //proj?.InvalidateDocuments();
                //range?.TrayAssembly.ResetComponents();
                //mainVM.RefreshAll(bSuppressImage:true);
                Message_Refresh message = new() { SuppressDocumentClosure = false, RangeNameChanged = true, ForceDocumentRefresh = true , UpdateRangeList = true, UpdateParts = true};
                if (uopUtils.RunningInIDE)
                {
                    mzQuestion q1 = null;
                    mzQuestion q2 = null;
                    mzQuestion q3 = null;
                    mzQuestion q4 = null;
                    mzQuestion q5 = null;

                    if (_RefreshQuery == null)
                    {
                        _RefreshQuery = new mzQuestions("Toggle Refresh Settings");
                        q1 = _RefreshQuery.AddCheckVal("Force Document Refresh?", true, aToolTip: "Force a rebuild of the project tree view and the displayed documents list");
                        q2 = _RefreshQuery.AddCheckVal("Update Range List?", true, aToolTip: "Force an update to the range selector combo box on the project view");
                        q3 = _RefreshQuery.AddCheckVal("Rebuild Project Parts?", true, aToolTip: "Force the current project to rengenerate its part matrix");
                        q4 = _RefreshQuery.AddCheckVal("Refresh Input Sketch?", true, aToolTip: "Force the regenerations of the currently selected tray ranges input sketch");
                        q5 = _RefreshQuery.AddCheckVal("Reset Components?", false, aToolTip:"Force the currently selected tray range to regenerate its persistent sub parts including downcomers, decks and spout groups");

                    }
                    else 
                    {
                        q1 = _RefreshQuery.Item(1);
                        q2 = _RefreshQuery.Item(2);
                        q3 = _RefreshQuery.Item(3 );
                        q4 = _RefreshQuery.Item(4);
                        q5 = _RefreshQuery.Item(5);


                    }
                    if (!_RefreshQuery.PromptForAnswers("")) return;
                    message.ForceDocumentRefresh = q1.AnswerB;
                    message.UpdateRangeList = q2.AnswerB;
                    message.UpdateParts = q3.AnswerB;
                    message.SuppressImage = !q4.AnswerB;
                    message.ResetComponents = q5.AnswerB;
                        
                }

                Execute_RefreshDisplay(message);
                //message.Publish();
            }
            catch
            {

            }

        }

        /// <summary>
        /// Show help content
        /// Help document is available on main screen, once we finaize the content for help file then we shall include help file specific to the screen
        /// </summary>
        public void Execute_HelpContext()
        {
            uopGlobals.ShowHelp();
        }

        public void Execute_HelpAbout()
        {
            _DialogService.ShowDialog<AboutView>(this.ParentVM, ApplicationModule.Instance.Resolve<AboutViewModel>());
        }

        public void Execute_DeveloperToggleIDE()
        {
            uopUtils.SuppressIDE = uopUtils.SuppressIDE ? false : true ;
        }

  

        /// <summary>
        /// Create a new MDProject
        /// </summary>
        /// <param name="projectType"></param>
        public virtual void Execute_NewProject(uppProjectTypes projectType)
        {

          

            WinTrayMainViewModel mainVM = ParentVM;
            if (mainVM == null) return;
            if (projectType != uppProjectTypes.MDSpout && projectType != uppProjectTypes.MDDraw)
            {
                mainVM.ShowMessageBox($"Project Type {projectType.Description()} Is Not Currently Support", "Unsupported Project Type", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (Project != null)
            {
                mainVM.ShowMessageBox("Please Close The Existing Project", "Project Has Data", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (projectType == uppProjectTypes.MDSpout)
            {
                if (!string.IsNullOrEmpty(SecurityHelper.Instance.ValidateVersion()))
                {
                    mainVM.ShowMessageBox(string.Format($"WinTray Version {SecurityHelper.Instance.AprovedVersion} Must Be Installed Before a New Project Can Be Started."), "Superceded Version Violation");
                    return;
                }
            }

            mdProject project = new() { ProjectType = projectType };
            project.DisplayUnits = uppUnitFamilies.English;
            ParameterOverride[] iputParams = new ParameterOverride[] 
            { new ParameterOverride("project", project), 
                new ParameterOverride("fieldName", "KeyNo"), 
                new ParameterOverride("isnewproject", true) 
            };


            Edit_MDProject_ViewModel VM = ApplicationModule.Instance.Resolve<Edit_MDProject_ViewModel>(iputParams);

            bool? result = mainVM.DialogService.ShowDialog<Edit_MDProject_View>(mainVM, VM);


            //bool? result = mainVM.DialogService.ShowDialog<NewMDProjectView>(mainVM, newmdProject);
            if (result != null && result.Value)
            {


                mainVM.Project = project;
                Execute_RefreshDisplay(null, true, true);


            }
           

        }

    }



    public class MenuEventArg
    {

        public MenuEventArg() { }
        public MenuEventArg(string aFileName, bool bFlag1 = false)
        {
            FileName = aFileName;
            Flag1 = bFlag1;
        }

        public string FileName { get; set; }
        public bool Flag1 { get; set; }
        

    }

}
