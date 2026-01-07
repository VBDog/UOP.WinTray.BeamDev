using UOP.DXFGraphics;
using MvvmDialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using UOP.WinTray.Projects;
using UOP.WinTray.Projects.Documents;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.UI.BusinessLogic;
using UOP.WinTray.UI.Commands;
using UOP.WinTray.UI.Interfaces;
using UOP.WinTray.UI.Model;

namespace UOP.WinTray.UI.ViewModels
{
    /// <summary>
    /// View Model for ReportGeneratorView
    /// </summary>
    public class ReportGeneratorViewModel : ViewModel_Base, IModalDialogViewModel
    {


        #region Constructor

        public ReportGeneratorViewModel(uopProject project, IReportWriter reportWriter, IDialogService dialogService) : base(null,dialogService)
        {
            Project = project;

            Reports = Project.Reports;
            foreach (var item in Reports)
            {
                SetTemplatePath(item as uopDocReport, out string ext);

            }
            ReportWriter = reportWriter;


            DrawingSource = (ProjectType == uppProjectTypes.MDSpout) ? new AppDrawingMDSHelper() : new AppDrawingMDDHelper();

            IsEnglishSelected = true;
            IsSelectAllPages = true;
            IsSelectAllTrays = true;
            SetUpForm();
        }

        #endregion

        /// <summary>
        /// Set Template Path
        /// </summary>
        /// <param name="aReport"></param>
        private void SetTemplatePath(uopDocReport aReport, out string rExtenstion)
        {


            rExtenstion = "";

            string fileName = aReport.TemplateName;
            if (string.IsNullOrWhiteSpace(fileName)) return;

            string fspec = appApplication.GetReportTemplatePath(aReport, ref fileName, out bool rFound, out rExtenstion);
            fspec = appApplication.GetLocalReportTemplatePath(rFound ? fspec : string.Empty, fileName);
            //if (File.Exists(fspec)) aReport.TemplatePath = fspec;
        }

        #region Private Properties


        private string Format_Date => "mm/dd/yy";
        private string Format_Tme => "hh:mm tt";
        private int DrawingPanelSize { get; set; }

        private uopDocuments Reports { get; set; }

        private IReportWriter ReportWriter { get; set; }

        public uopDocReport Report { get; set; }
        private string FileSpec { get; set; }

        private List<uopDocReportPage> Pages { get; set; }
        #endregion Private Properties

        #region Public Properties

        private bool _AllowPageSelection = false;
        public bool AllowPageSelection
        {
            get => _AllowPageSelection;
            set { _AllowPageSelection = value; NotifyPropertyChanged("AllowPageSelection"); Visibility_PageSelect = value ? Visibility.Visible : Visibility.Collapsed; }
        }

        private bool _AllowRangeSelection = false;
        public bool AllowRangeSelection
        {
            get => _AllowRangeSelection;
            set { _AllowRangeSelection = value; NotifyPropertyChanged("AllowRangeSelection"); Visibility_RangeSelect = value ? Visibility.Visible : Visibility.Collapsed; }
        }

        private Visibility _Visibility_PageSelect;
        public Visibility Visibility_PageSelect
        {
            get => _Visibility_PageSelect;
            set { _Visibility_PageSelect = value; NotifyPropertyChanged("Visibility_PageSelect"); }
        }

        private Visibility _Visibility_RangeSelect;
        public Visibility Visibility_RangeSelect
        {
            get => _Visibility_RangeSelect;
            set { _Visibility_RangeSelect = value; NotifyPropertyChanged("Visibility_RangeSelect"); }
        }


        private IappDrawingSource _DrawingSource;
        public IappDrawingSource DrawingSource
        {
            get => _DrawingSource;
            set => _DrawingSource = value;
        }

        private string _Status1 = "";
        public string Status1
        {
            get => _Status1; 
            set { _Status1 = value; NotifyPropertyChanged("Status1"); }
        }

        private string _Status2 = "";
        public string Status2
        {
            get => _Status2; 
            set { _Status2 = value; NotifyPropertyChanged("Status2"); }
        }

        private string _Status3;
        public string Status3
        {
           get => _Status3; 
            set { _Status3 = value; NotifyPropertyChanged("Status3"); }
        }

        private string _Status4;
        public string Status4
        {
            get => _Status4; 
            set { _Status4 = value; NotifyPropertyChanged("Status4"); }
        }

        private bool _IsMarkRevisions;
        public bool IsMarkRevisions
        {
            get => _IsMarkRevisions; 
            set { _IsMarkRevisions = value; NotifyPropertyChanged("IsMarkRevisions"); }
        }

        private bool _IsMarkRevisionsVisible;
        public bool IsMarkRevisionsVisible
        {
            get => _IsMarkRevisionsVisible; 
            set { _IsMarkRevisionsVisible = value; NotifyPropertyChanged("IsMarkRevisionsVisible"); }
        }

        private bool _IsSelectAllPages;
        public bool IsSelectAllPages
        {
            get => _IsSelectAllPages; 
            set { _IsSelectAllPages = value; NotifyPropertyChanged("IsSelectAllPages"); }
        }

        private bool _IsSelectAllTrays;
        public bool IsSelectAllTrays
        {
            get => _IsSelectAllTrays; 
            set { _IsSelectAllTrays = value; NotifyPropertyChanged("IsSelectAllTrays"); }
        }

        private bool _IsColumSketchCountVisible;
        public bool IsColumSketchCountVisible
        {
            get => _IsColumSketchCountVisible; 
            set { _IsColumSketchCountVisible = value; NotifyPropertyChanged("IsColumSketchCountVisible"); }
        }


        private string _ReportSelectedItem;

        public string ReportSelectedItem
        {
            get => _ReportSelectedItem; 
            set { _ReportSelectedItem = value; NotifyPropertyChanged("ReportSelectedItem"); SelectedReportChanged(); }
        }

        private string _FolderName;
        public string FolderName
        {
            get => _FolderName; 
            set { _FolderName = value.Trim(); NotifyPropertyChanged("FolderName"); }
        }

        private string _FileName;
        public string FileName
        {
            get => _FileName; 
            set { _FileName = value.Trim(); NotifyPropertyChanged("FileName"); }
        }

        private string _TemplateName;
        public string TemplateName
        {
            get => _TemplateName;
            set { _TemplateName = value.Trim(); NotifyPropertyChanged("TemplateName"); }
        }
        private bool? _DialogResult;
        /// <summary>
        /// Dialog service result
        /// </summary>
        public bool? DialogResult
        {
            get => _DialogResult;
            private set { _DialogResult = value; NotifyPropertyChanged("DialogResult"); }
        }

        ObservableCollection<MultiselectList> _PagesList = new();
        public ObservableCollection<MultiselectList> PagesList
        {
            get => _PagesList; 
            set { _PagesList = value; NotifyPropertyChanged("PagesList"); }
        }

        ObservableCollection<string> _ReportsList = new();
        public ObservableCollection<string> ReportsList
        {
            get => _ReportsList; 
            set { _ReportsList = value; NotifyPropertyChanged("ReportsList"); }
        }

        ObservableCollection<MultiselectList> _TrayRangeList = new();
        public ObservableCollection<MultiselectList> TrayRangeList
        {
            get => _TrayRangeList;
            set { _TrayRangeList = value; NotifyPropertyChanged("TrayRangeList"); }
        }

        private int _SketchCount = 1;
        public int SketchCount
        {
            get => _SketchCount; 
            set { _SketchCount = value; NotifyPropertyChanged("SketchCount"); }
        }

        #endregion Public Properties

        #region Commands
        private DelegateCommand _CMD_Cancel;
        /// <summary>
        /// Cancel command
        /// </summary>
        public ICommand Command_Cancel { get { if (_CMD_Cancel == null) _CMD_Cancel = new DelegateCommand(param => Execute_Cancel()); return _CMD_Cancel; } }

        private DelegateCommand _CMD_CheckBoxPages;
        /// <summary>
        /// Pages Check box command
        /// </summary>
        public ICommand Command_CheckBoxPages { get { if (_CMD_CheckBoxPages == null) _CMD_CheckBoxPages = new DelegateCommand(param => Execute_CheckBox_Pages()); return _CMD_CheckBoxPages; } }

        private DelegateCommand _CMD_CheckBoxTrays;
        /// <summary>
        /// TRays check box command
        /// </summary>
        public ICommand Command_CheckBoxTrays { get { if (_CMD_CheckBoxTrays == null) _CMD_CheckBoxTrays = new DelegateCommand(param => Execute_CheckBox_Trays()); return _CMD_CheckBoxTrays; } }

        private DelegateCommand _CMD_OK;
        /// <summary>
        /// Execute Button Command 
        /// </summary>
        public ICommand Command_Ok { get { if (_CMD_OK == null) _CMD_OK = new DelegateCommand(param => Execute_Report()); return _CMD_OK; } }

        #endregion Commands


        #region Private Methods
        /// <summary>
        /// Capture check change status for Select All Pages
        /// </summary>
        private void Execute_CheckBox_Pages()
        {
            PagesShow();
        }
        /// <summary>
        /// Capture check change status for Select All Trays
        /// </summary>
        private void Execute_CheckBox_Trays()
        {
            RangesShow();
        }

        private void Execute_Report()
        {
            try
            {
                if (ReportCanExecute())
                {
                    ErrorMessage =  CreateReport();

                    if (!string.IsNullOrWhiteSpace(ErrorMessage))
                    {
                        DialogService.ShowMessageBox(this, ErrorMessage, "Report Error");
                        return;
                    }

                    DialogResult = true;

                  
                }
            }
            finally
            {
           
            }
        }

        /// <summary>
        /// Close the ReportGenerator window
        /// </summary>
        private void Execute_Cancel()
        {
            this.DialogResult = false;
        }
        /// <summary>
        /// Method to setup the form initially
        /// </summary>
        private void SetUpForm()
        {

            Canceled = true;
            Edited = false;
            uopDocReport aReport = null;
            //opCanceled = false;
            //Reports = Project.Reports;
            DateTime projectDate = Convert.ToDateTime(Project.LastSaveDate);

            //chkApplyRevision.IsChecked = vbChecked;
            //chkApplyRevision.setVisible(false);

            IsMarkRevisions = true;
            IsMarkRevisionsVisible = false;

            //txtExtra.Text = "";
            //picSketch.setVisible(false);
            //fraWorking.setVisible(false);

            Status1 = Project.ProjectTypeName + " " + Project.ProjectName;
            Status2 = projectDate.ToString(Format_Date);
            Status3 = projectDate.ToString(Format_Tme);

            //txtFolder.Text = "";
            //txtName.Text = "";
            //optUnits.Item(0).Value = true;
            //optUnits.Item(0).Enabled = true;
            //optUnits.Item(1).Enabled = true;
            //txtFolder.Locked = true;
            //txtName.IsEnabled = true;

            ReportsList = new ObservableCollection<string>();
            TrayRangeList = new ObservableCollection<MultiselectList>();
            PagesList = new ObservableCollection<MultiselectList>();
            for (int i = 1; i <= Reports.Count; i++)
            {
                aReport = (uopDocReport)Reports.Item(i);
                aReport.FolderName = Project.OutputFolder;
                ReportsList.Add(aReport.ReportName);
            }
            ReportSelectedItem = ReportsList.FirstOrDefault();
        }

        /// <summary>
        /// Execute report method
        /// </summary>
        private bool ReportCanExecute()
        {
            if (Project == null || Report == null) return false;

            FileSpec = Path.Combine(FolderName, FileName);

            if (!FileSpec.ToLower().EndsWith(".xlsm") && !FileSpec.ToLower().EndsWith(".xlsx"))
            {
                FileSpec += ".xlsm";
            }

            if (TrayRangeList.All(x => !x.IsSelected.Value))
            {
                DialogService.ShowMessageBox(this, "At Least One Tray Section Must Be Selected!", "Report Error");
                return false;
            }
            if (string.IsNullOrEmpty(ReportSelectedItem))
            {
                DialogService.ShowMessageBox(this, "At Least One Report Page Must Be Selected!", "Report Error");
                return false;
            }

            if (string.IsNullOrEmpty(FolderName))
            {
                DialogService.ShowMessageBox(this, "Destination Folder Is Required", "Report Error");
                return false;
            }
            if (!Directory.Exists(FolderName))
            {
                Directory.CreateDirectory(FolderName);
            }

            if (!Directory.Exists(FolderName))
            {
                DialogService.ShowMessageBox(this, "Destination Folder Does Not Exist And Could Not Be Created", "Report Error");
                return false;
            }

            if (string.IsNullOrEmpty(FileName))
            {
                DialogService.ShowMessageBox(this, "File Name Is Required", "Report Error");
                return false;
            }

            SaveReportData();
            FormatReportPages();

            if (PagesList.All(x => !x.IsSelected.Value))
            {
                DialogService.ShowMessageBox(this,  "The Request Report Configuration Will Yield A Report With No Pages!", "Report Error");
                return false;
            }

            //confirm overwrite request
            if (File.Exists(FileSpec))
            {
                MessageBoxResult reply = DialogService.ShowMessageBox(this, $"Overwrite Existing Report {FileSpec} ?", "File Already Exists", MessageBoxButton.YesNo);
                if (reply == MessageBoxResult.No)
                {
                    return false;
                }
            }

            //opCanceled = false;
            //Hide();
            return true;
        }

        /// <summary>
        /// Format Report Pages
        /// </summary>
        private void FormatReportPages()
        {
            if (Report == null || Pages == null || Project == null) return;
            uopReportGenerator.FormatReport( Project, Report);
        }

        /// <summary>
        /// Save Report Data
        /// </summary>
        private void SaveReportData()
        {
            if (Report == null || Project == null) return;
            
            uopDocReportPage aPage;
            uopTrayRange aRange;
            colUOPTrayRanges ranges = Project.TrayRanges;
            Report.SelectedRanges = new List<int>();

            for (int i = 1; i <= TrayRangeList.Count; i++)
            {
                if(i <= ranges.Count)
                {
                    aRange = Project.TrayRanges.Item(i);
                    aRange.Requested = TrayRangeList[i - 1].IsSelected.Value;
                    if (aRange.Requested)
                    {
                        Report.SelectedRanges.Add(aRange.Index);
                    }

                }
            }
            Report.RequestedPages = new  List<uopDocReportPage>();
            for (int i = 1; i <= PagesList.Count ; i++)
            {
                aPage =  Pages[i - 1];
                aPage.IsRequested = PagesList[i -1].IsSelected.Value;
                if (aPage.IsRequested )
                    Report.RequestedPages.Add(aPage);
            }

            if (Report.ReportType == uppReportTypes.MDSpoutReport)
            {
                if (!mzUtils.IsNumeric(SketchCount)) SketchCount = Report.SketchCount;
                Report.SketchCount = SketchCount;
            }
            Report.Units = DisplayUnits;
            Report.FolderName = FolderName;
            Report.FileName = FileName;
            Report.ReportForm = this;
            
        }


        public override void ReleaseReferences()
        {
            base.ReleaseReferences();
            Report.Project = null;
            Report = null;
            
        }

        /// <summary>
        /// Create Report
        /// </summary>
        private string CreateReport()
        {
           
            if (Report == null) return "The Selectected Report Is Not Defined";
            if (Project == null) return "The Subject Project Is Not Defined";
            if (ReportWriter == null) return "The Report Writer Is Not Defined";

            try
            {
                //create the excel spreadsheets
                Report.ErrorString = string.Empty;
                Report.Project = Project;
                Report.SuppressRevisionChecks = IsMarkRevisionsVisible && !IsMarkRevisions;
                Report.FolderName = FolderName;
                Report.FileName = FileName;
                ReportWriter.Report = Report;

                if (Report.RequestedPages.Count <= 0) return "At Least One Page Type Needs to Be Selected!";
                if (string.IsNullOrWhiteSpace(Report.FolderName)) return "An Ouput Folder Path Must Be Provided!";
                if (!Directory.Exists(Report.FolderName)) return $"Ouput Folder Path ({Report.FolderName}) Can Not Be Found!";
                if (string.IsNullOrWhiteSpace(Report.FileName)) return "An Ouput FileName Must Be Provided!";

                Report.FileSpec = Path.Combine(Report.FolderName, Report.FileName);
                if (File.Exists(Report.FileSpec))
                {
                    if (uopUtils.FileIsOpen( Report.FileSpec)) return $"The Ouput File ({Report.FileSpec}) is In Use by Another Application.";
                }
                return "";
            }
            catch(Exception e)
            {
                return e.Message;
            }
      
           
        }

        /// <summary>
        /// Method to reload data on selected report change
        /// </summary>
        private void SelectedReportChanged()
        {
            uopDocReport aReport = null;

            aReport = (uopDocReport)Reports.GetBySelectName(ReportSelectedItem);
            if (aReport != null)
            {
                Reports.SetSelectedDocument(aReport.Index);
            }

            ShowReportInput();
        }

        /// <summary>
        /// Show Reports input
        /// </summary>
        private void ShowReportInput()
        {
            uopDocReport aReport =  (uopDocReport)Reports.GetSelectedDocument();

            AllowPageSelection = false;
            AllowRangeSelection =false;
            if (aReport == null)
            {
                TrayRangeList.Clear();
                PagesList.Clear();
                TemplateName ="";
                return;
            }

            //don't redisplay
            if (Report != null && aReport == Report)  return;
            if (!uopUtils.RunningInIDE)
            {
                AllowPageSelection = !aReport.AllPagesOnly;
                AllowRangeSelection = !aReport.AllRangesOnly;

            }
            else
            {
                AllowPageSelection = true;
                AllowRangeSelection = true;

            }

            Report = aReport;
            Pages = null;
            TemplateName = Report.TemplatePath;

            //bLoading = true;

            if (Report.ReportType == uppReportTypes.MDSpoutReport)
            {
                IsColumSketchCountVisible = true;
                SketchCount = Report.SketchCount;
            }
            else
            {
                IsColumSketchCountVisible = false;
            }

            IsMarkRevisionsVisible = Report.MaintainRevisionHistory && Project.Revision > 0;
            //if (Report.Units == uppUnitFamilies.Metric)
            //{
            //    optUnits.Item(1).Value = true;
            //    if (!ThisApp.RunningInIDE)
            //    {
            //        optUnits.Item(0).Enabled = !Report.UnitsLocked;
            //    }
            //    else
            //    {
            //        optUnits.Item(0).Enabled = true;
            //    }
            //}
            //else
            //{
            //    optUnits.Item(0).Value = true;
            //    if (!ThisApp.RunningInIDE)
            //    {
            //        optUnits.Item(1).Enabled = !Report.UnitsLocked;
            //    }
            //    else
            //    {
            //        optUnits.Item(1).Enabled = true;
            //    }
            //}

            if (!Directory.Exists(Report.FolderName))
            {
                Report.FolderName = Project.OutputFolder;
            }
            FolderName = Report.FolderName;
            ShowSpreadSheets();
            //if (Report.FileNameLocked)
            //{
            string ext = Path.GetExtension(Report.TemplatePath).ToLower();
            if (ext == ".xltm") ext = ".xlsm";
            if (ext == ".xltx") ext = ".xlsx";
            FileName = Project.ProjectName + " - " + Report.ReportName + ext;
            //}
            //else
            //{
            //    FileName = Report.FileName;
            //}

            //bLoading = false;

            RangesShow();
            PagesShow();
        }

        /// <summary>
        /// Show Pages for report
        /// </summary>
        private void PagesShow()
        {
            if (Report == null || Project == null) return;

            Report.Project = Project;
         
            Pages = Report.VisiblePages(false);
            PagesList.Clear();

            //bLoading = true;
            int cntItemsSelected = 0;
            for (int i = 1; i <= Pages.Count; i++)
            {
                uopDocReportPage aPage = Pages[i-1];
                PagesList.Add(new MultiselectList() { Text = aPage.PageName, IsSelected = aPage.IsRequested && IsSelectAllPages });
                cntItemsSelected++;
            }

            //if (cntItemsSelected == Pages.Count)
            //{
            //    IsSelectAllPages = true;
            //}
            //else
            //{
            //   IsSelectAllPages = false;
            //}
            //bLoading = false;

            //if (Report.AllPagesOnly && !ThisApp.RunningInIDE)
            //{
            //    chkAllPages.IsChecked = vbChecked;
            //    chkAllPages.IsEnabled = false;
            //}
            //else
            //{
            //    chkAllPages.IsEnabled = true;
            //}
        }

        /// <summary>
        /// Show Ranges for Report
        /// </summary>
        private void RangesShow()
        {
            if (Report == null)
            {
                return;

            }
            if ( Project.TrayRanges == null)
            {
                return;

            }
            TrayRangeList.Clear();
            uopTrayRange aRange = null;
            //int cnt = 0;
            //bLoading = true;
            for (int i = 1; i <=  Project.TrayRanges.Count; i++)
            {
                aRange =  Project.TrayRanges.Item(i);
                if (Report.AllRangesOnly)// && !ThisApp.RunningInIDE)
                {
                    aRange.Requested = IsSelectAllTrays;
                }
                TrayRangeList.Add(new MultiselectList() { Text =  Project.TrayRanges.Item(i).Name(true), IsSelected = IsSelectAllTrays });
                //if (lstRanges.Selected(i - 1))
                //{
                //    cnt = cnt + 1;
                //}
                //cnt++;
            }
            //if (cnt ==  Project.TrayRanges.Count)
            //{
            //    IsSelectAllTrays = true;
            //}
            //else
            //{
            //    IsSelectAllTrays = false;
            //}
            //bLoading = false;

            //if (Report.AllRangesOnly && !ThisApp.RunningInIDE)
            //{
            //    chkAllRanges.IsChecked = vbChecked;
            //    chkAllRanges.IsEnabled = false;
            //}
            //else
            //{
            //    chkAllRanges.IsEnabled = true;
            //}
        }

        /// <summary>
        /// Show Spreadsheets
        /// </summary>
        private void ShowSpreadSheets()
        {
            //string fspec = "";
            //File aFile = null;
            //Files aFiles = null;
            //ListItem itm = null;

            //DateTime fDate = DateTime.MinValue;

            //bool expire = false;

            //// TODO (not supported):     On Error Resume Next

            //FolderName = FolderName.Trim();
            //fspec = FolderName;


            //dynamic _WithVar_1416;
            //_WithVar_1416 = lstSpreadSheets.DefaultProperty;
            //_WithVar_1416.ListItems.Clear();
            //_WithVar_1416.GridLines = false;
            //_WithVar_1416.FullRowSelect = true;
            //_WithVar_1416.LabelEdit = lvwManual;
            //_WithVar_1416.ColumnHeaders.Clear();
            //_WithVar_1416.ColumnHeaders.Add();
            //_WithVar_1416.ColumnHeaders.Add();
            //_WithVar_1416.ColumnHeaders.Add();
            //_WithVar_1416.View = lvwReport;
            //_WithVar_1416.HideColumnHeaders = false;

            //if (fspec == "")
            //{
            //    return;
            //}
            //if (!Directory.Exists(fspec))
            //{
            //    return;
            //}

            //aFiles = goFSO.GetFolder(fspec).Files;
            //foreach (var aFile in aFiles)
            //{
            //    if (ListContains(goFSO.GetExtensionName(aFile.Name), "XLS,".xlsm",XLSX"))
            //    {
            //        fDate = aFile.DateLastModified;
            //        expire = fDate < ProjectDate;
            //        itm = _WithVar_1416.ListItems.Add(_, _, aFile.Name);
            //        itm.SubItems(1) = Format(fDate, "mm/dd/yy");
            //        itm.SubItems(2) = Format(fDate, "hh:mm AMPM");
            //    }
            //}
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Create Drawing
        /// </summary>
        /// <param name="aDrawing"></param>
        /// <returns></returns>
        public dxfImage CreateDrawing(uopDocDrawing aDrawing, Color? BackColor = null)
        {
            double sf = GetWindowsScreenScalingFactor();
            double screenWidth = 7.2 * sf;
            DrawingPanelSize = (int)screenWidth;
            
            if (aDrawing == null) 
                return null;

            dxfImage image;

           
            if (ProjectType == uppProjectTypes.MDSpout)
            {
                aDrawing.DrawingUnits = uppUnitFamilies.English;
            }
            if(DrawingSource == null)
            {
                DrawingSource = (ProjectType == uppProjectTypes.MDDraw) ? new AppDrawingMDDHelper() : new AppDrawingMDSHelper();
            }

            if (!BackColor.HasValue) BackColor = Color.White;

            if(aDrawing.DeviceSize == null)
            {
                aDrawing.DeviceSize = new System.Drawing.Size(DrawingPanelSize, DrawingPanelSize);
            }
            else
            {
                if(aDrawing.DeviceSize.IsEmpty) aDrawing.DeviceSize = new System.Drawing.Size(DrawingPanelSize, DrawingPanelSize);
            }
            
            aDrawing.ZoomExtents = true;
            try
            {
                image = DrawingSource.GenerateImage(aDrawing, true, new System.Drawing.Size(DrawingPanelSize, DrawingPanelSize),bUsingViewer:false, BackColor: BackColor);

                List<string> errs = image.ErrorCol;
                foreach (var item in errs)
                {
                    aDrawing.AddWarning(null, "Image Generation Error", item);
                }
            }
            finally
            {
                // send the drawing to the clipboard and reply as successful
               // image.Bitmap().CopyToClipBoard();
              //  _rVal = "OK";
            }

            
            
            return image;
        }

        /// <summary>
        /// Get Windows Screen Scaling Factor
        /// If required as percentage - convert it ScreenScalingFactor *= 100.0
        /// </summary>
        /// <param name="percentage"></param>
        /// <returns></returns>
        public static double GetWindowsScreenScalingFactor()
        {
            float dpiX;
            using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
            {
                dpiX = graphics.DpiX;
            }
            return dpiX;
        }
        #endregion

    }
}
