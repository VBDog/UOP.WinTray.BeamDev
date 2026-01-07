using ClosedXML.Excel;
using ClosedXML.Excel.Drawings;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Tables;
using UOP.WinTray.Projects.Generators;

using UOP.WinTray.Projects.Documents;
using UOP.WinTray.UI.Interfaces;
using UOP.WinTray.UI.ViewModels;
using MvvmDialogs;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using UOP.WinTray.UI.Messages;
using UOP.WinTray.UI.Events.EventAggregator;
using System.Drawing;
using UOP.DXFGraphics;
using ExcelIO = Microsoft.Office.Interop.Excel;
using System.Linq;
using System.Collections;
using UOP.WinTray.Projects;

namespace UOP.WinTray.UI.BusinessLogic
{
    /// <summary>
    /// Class for report writer operations
    /// </summary>
    public class ReportWriter : IReportWriter
    {

        #region Constructors

        /// <summary>
        /// Constructor for tree view 
        /// </summary>
        /// <param name="MainVM"></param>

        public ReportWriter(WinTrayMainViewModel parentVM) { MainVM = parentVM; Init(); }
        public ReportWriter() { Init(); }

        #endregion Constructors

        #region Properties

        private List<uopDocWarning> _Warnings;
        /// returns warnings
        /// </summary>
        public List<uopDocWarning> Warnings { get => _Warnings; set => _Warnings = value ?? new List<uopDocWarning>(); }

        private Dictionary<uppUnitTypes, string> Formats_English { get; set; }

        private Dictionary<uppUnitTypes, string> Formats_Metric { get; set; }


        private string DATE_FORMAT => "MM/dd/yy";

        private Dictionary<string, string> CellAddresses { get; set; }

        private string SheetPassword => StandardFormat ? "UOPMDUOP" : "";
        public bool UseClosedXML { get; set; }

        private bool? _CanMarkRevision = null;
        public bool CanMarkRevision => _CanMarkRevision.HasValue && _CanMarkRevision.Value;

        private IDialogService DialogService => MainVM?.DialogService;

        //Excel.Workbook  '@the previous run of the current report
        private XLWorkbook BackupXML {
            get
            {
                if (!BackupIsPresent) return null;

                return new XLWorkbook(BackupPath);

            }
        }

        private bool BackupIsPresent
        {
            get
            {
                if (string.IsNullOrWhiteSpace(BackupPath)) return false;
                if (!File.Exists(BackupPath)) { BackupPath = ""; return false; }

                return true;

            }
        }

        public bool MarkingRevisons { get; set; }
        private bool LastRevIsPresent
        {
            get
            {
                if (string.IsNullOrWhiteSpace(LastRevisionPath)) return false;
                if (!File.Exists(LastRevisionPath)) return false;

                return true;

            }
        }

        private ExcelIO.Workbook Backup
        {
            get
            {
                if (Excel == null) return null;
                if (string.IsNullOrWhiteSpace(BackupPath)) return null;
                if (!File.Exists(BackupPath)) { BackupPath = ""; return null; }

                ExcelIO.Workbook _rVal;
                try
                {
                    _rVal = Excel.Workbooks.Open(BackupPath);
                    return _rVal;

                }
                catch { BackupPath = ""; return null; }

            }
        }

        private string BackupPath { get; set; } = "";

        private XLWorkbook WorkBookXML { get; set; }

        private ExcelIO.Application Excel { get; set; }

        private ExcelIO.Workbook WorkBook { get; set; }

        private uppReportTypes _ReportType = uppReportTypes.Undefined;
        public uppReportTypes ReportType
        {
            get => _ReportType;
        }

        private uopDocReport _Report = null;
        public uopDocReport Report { get => _Report;
            set
            {
                _ReportType = uppReportTypes.Undefined;
                bool init = value != null && _Report == null;
                _Report = value;
                if (value != null) _ReportType = value.ReportType;
                if (init)
                {

                    Project ??= value.Project;
                    if (value.SuppressRevisionChecks || !Report.MaintainRevisionHistory) SuppressRevisions = true;
                    StandardFormat = Report.StandardFormat; //  mzUtils.ListContains(value.TemplateName, "MDSpoutReport,CrossFlowReport,MDSpoutReportXLT");
                    CreateAddressMap();

                    Warnings.Clear();
                    value.Warnings.Clear();
                    value.ErrorString = string.Empty;
                    ReportGeneratorVM = value.ReportForm;

                    WorkFolder = value.FolderName;
                    FileName = value.FileName;
                    RequestedPages = value.RequestedPages;
                    value.Warnings.Clear();

                    PageCount = RequestedPages.Count;

                    FatalError = false;
                    FileSpec = Path.Combine(WorkFolder, FileName);

                    _CanMarkRevision = null;

                    if (!SuppressRevisions)
                    {

                        if (ReportType == uppReportTypes.MDSpoutReport)
                        {
                            if (File.Exists(FileSpec) && File.Exists(LastRevisionPath)) _CanMarkRevision = true;
                        }
                    }


                }

            }
        }
        private bool NoRevisions { get; set; }

        public bool FatalError { get; set; }

        private bool StandardFormat { get; set; }

        private ExcelIO.Worksheet BlankPage { get; set; }

        private ExcelIO.Worksheet ActiveSheet { get; set; }

        private IXLWorksheet ActiveSheetXML { get; set; }
        private uopDocReportPage ActivePage { get; set; }
        private List<ExcelIO.Worksheet> Worksheets { get; set; }

        private List<IXLWorksheet> WorksheetsXML { get; set; }


        private IXLWorksheet BlankPageXML { get; set; }

        private bool SuppressRevisions { get; set; }
        private XLWorkbook LastRevisionXML { get; set; }


        private ExcelIO.Workbook LastRevision
        {
            get
            {

                if (Excel == null) return null;
                string fspec = LastRevisionPath;
                if (string.IsNullOrWhiteSpace(fspec)) return null;

                foreach (ExcelIO.Workbook wb in Excel.Workbooks)
                {
                    if (string.Compare(wb.Name, Path.GetFileName(fspec), true) == 0)
                    {
                        return wb;
                    }
                }

                return Excel.Workbooks.Open(fspec);

            }
        }

        private string LastRevisionPath
        {
            get
            {
                string fspec = FileSpec;
                if (string.IsNullOrWhiteSpace(fspec) || Project == null) return "";


                string ext = Path.GetExtension(FileSpec);
                string curname = Path.GetFileNameWithoutExtension(FileSpec);
                string lastname = curname.Replace($"-{Project.Revision} ", $"-{Project.Revision - 1} ");
                if (string.Compare(curname, lastname, true) == 0) return "";

                string path = Path.Combine(Path.GetDirectoryName(FileSpec), $"{lastname}{ext}");
                return File.Exists(path) ? path : "";
            }
        }

        private ReportGeneratorViewModel ReportGeneratorVM { get; set; }

        private uppUnitFamilies Units => (Report != null) ? Report.Units : uppUnitFamilies.English;

        //@the path to the output folder
        private string WorkFolder { get; set; }

        //@the name of the excel file to create
        private string FileName { get; set; }

        public string ReportPath { get => FileSpec; }

        //@the full path to the output document
        public string FileSpec { get; set; }
        //@the collection of requested pages from the current report
        private List<uopDocReportPage> RequestedPages { get; set; }

        //@holds the total number of pages that need to be created
        private int PageCount { get; set; }

        private string Template { get; set; }

        private string TemplateExtension
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Template)) return "";
                return System.IO.Path.GetExtension(Template);
            }
        }

        private bool TemplateIsTemplate
        { get
            {
                if (string.IsNullOrWhiteSpace(Template)) return false;
                return TemplateExtension.ToUpper().Contains("T");
            }
        }

        private bool TemplateHasMacros
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Template)) return false;
                return TemplateExtension.ToUpper().Contains("M");
            }
        }

        public string BaseFileName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(FileName)) return "";
                return System.IO.Path.GetFileNameWithoutExtension(FileName);
            }
        }

        public string SaveAsFileName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Template)) return "";
                if (string.IsNullOrWhiteSpace(FileName)) return "";
                return $"{ BaseFileName}{SaveAsExtension}";
            }
        }

        public string SaveAsExtension
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Template)) return "";

                return TemplateHasMacros ? ".xlsm" : ".xlsx";

            }
        }

        private string BasePage { get; set; }

        //@flag used to cancel the report generation procedure
        private bool CancelIt { get; set; }

        private string RevDate { get; set; }

        private string RevInits { get; set; }

        private WinTrayMainViewModel _MainVM;
        private WinTrayMainViewModel MainVM { get => _MainVM; set { _MainVM = value; Project = value?.Project; } }


        public uopProject Project { get; set; }

        private int ProjectRev => Project == null ? 0 : Project.Revision;

        public uppProjectTypes ProjectType => (Project == null) ? uppProjectTypes.Undefined : Project.ProjectType;

        private string _Status = string.Empty;

        public string Status { get => _Status; set { _Status = value?.Trim(); SetStatusMessages(null, _Status); } }


        public string ErrorMessage { get; set; }

        private bool Silent { get; set; }
        internal IEventAggregator EventAggregator => MainVM?.EventAggregator;


        #endregion Properties

        #region Methods
        private Dictionary<uppUnitTypes, string> NumberFormats(uppUnitFamilies aUnits)
        {
            return (aUnits == uppUnitFamilies.Metric) ? Formats_Metric : Formats_English;
        }
        private void Init()
        {
            Formats_English = new Dictionary<uppUnitTypes, string>
            {
                { uppUnitTypes.Undefined, "" },
                { uppUnitTypes.SmallLength, "0.0000" },
                { uppUnitTypes.BigLength, "0.000" },
                { uppUnitTypes.SmallArea, "0.000" },
                { uppUnitTypes.BigArea, "0.000" },
                { uppUnitTypes.Percentage, "0.00" },
                { uppUnitTypes.BigPercentage, "0.0" },
                { uppUnitTypes.BigMassRate, "#,0" },
                { uppUnitTypes.Density, "0.000" },
                { uppUnitTypes.VolumePercentage, "0.00" },
                { uppUnitTypes.VolumeRate, "#,0.0" }
            };


            Formats_Metric = new Dictionary<uppUnitTypes, string>
            {
                { uppUnitTypes.Undefined, "" },
                { uppUnitTypes.SmallLength, "0.0" },
                { uppUnitTypes.BigLength, "0.0" },
                { uppUnitTypes.SmallArea, "0.00" },
                { uppUnitTypes.BigArea, "0.00" },
                { uppUnitTypes.Percentage, "0.00" },
                { uppUnitTypes.BigPercentage, "0.0" },
                { uppUnitTypes.BigMassRate, "#,0" },
                { uppUnitTypes.Density, "0.0" },
                { uppUnitTypes.VolumePercentage, "0.00" },
                { uppUnitTypes.VolumeRate, "#,0.0" }
            };

            _Warnings = new List<uopDocWarning>();
        }
        public void ToggleAppEnabled(bool bEnabledVal, string aBusyMessage = null)
        {
            if (Silent) return;
            MainVM?.MenuItemViewModelHelper.ToggleAppEnabled(bEnabledVal,  aBusyMessage);
            
        }

        public void SetStatusMessages(string message1 = null, string message2 = null, bool? appEnabled = null)
        {
            if (Silent) return;

            PublishMessage(new Message_SetStatusMessages(message1, message2, appEnabled));
        }

        public void PublishMessage<T>(T message)
        {

            if (MainVM == null || message == null)
                return;

            EventAggregator.Publish<T>(message);
        }

        private void CreateAddressMap()
        {
            CellAddresses = new Dictionary<string, string>();



            if (StandardFormat)
            {
                CellAddresses.Add("Body".ToUpper(), "C12:AN61");
                CellAddresses.Add("Field".ToUpper(), "D12:AM61");
                CellAddresses.Add("Password".ToUpper(), "AP62:AU62");

                CellAddresses.Add("Notes".ToUpper(), "C29:AN60");
                CellAddresses.Add("SignatureBlock".ToUpper(), "V5:AO8");
                CellAddresses.Add("SignatureBlock1".ToUpper(), "V6:AE8");
                CellAddresses.Add("SignatureBlock2".ToUpper(), "AF6:AO8");
                CellAddresses.Add("SignatureBlock3".ToUpper(), "AF5:AO8");
                CellAddresses.Add("RevMarkCol".ToUpper(), "B");
                CellAddresses.Add("RevCells".ToUpper(), "B9:B61");
                CellAddresses.Add("RevBlock1".ToUpper(), "V5:V8");
                CellAddresses.Add("RevCol1".ToUpper(), "V");
                CellAddresses.Add("RevCol2".ToUpper(), "AF");
                CellAddresses.Add("DateCol1".ToUpper(), "X");
                CellAddresses.Add("DateCol2".ToUpper(), "AH");
                CellAddresses.Add("ReviewedCol1".ToUpper(), "AA");
                CellAddresses.Add("ReviewedCol2".ToUpper(), "AK");
                CellAddresses.Add("TitleBlock".ToUpper(), "C5:A7");
                CellAddresses.Add("Title1".ToUpper(), "C5");
                CellAddresses.Add("Title2".ToUpper(), "C6");
                CellAddresses.Add("Title3".ToUpper(), "C7");
                CellAddresses.Add("Apvd1".ToUpper(), "AC5:AE8");
                CellAddresses.Add("Apvd2".ToUpper(), "AM5:AO8");
                CellAddresses.Add("Sheet1".ToUpper(), "AM3");
                CellAddresses.Add("Sheet2".ToUpper(), "AO3");


                //                  private const string CellAddress("RevCol1") = "V";
                //    private const string CellAddress("RevCol2") = "AF";
                //    private const string CellAddress("DateCol1") = "X";
                //    private const string CellAddress("DateCol2") = "AH";
                //    private const string CellAddress("ReviewedCol1") = "AA";
                //    private const string CellAddress("ReviewedCol2") = "AK";

            }
        }

        private List<string> RevCellAddresses
        {
            get
            {
                if (!StandardFormat) return new List<string> { };
                List<string> _rVal = new();
                _rVal.Add("V5:AE5");
                _rVal.Add("V6:AE6");
                _rVal.Add("V7:AE7");
                _rVal.Add("V8:AE8");
                _rVal.Add("AF5:AO5");
                _rVal.Add("AF6:AO6");
                _rVal.Add("AF7:AO7");
                _rVal.Add("AF8:AO8");
                return _rVal;
            }
        }
        private string CellAddress(string aCellName)
        {
            return (string.IsNullOrWhiteSpace(aCellName) || CellAddresses == null) ? "" : CellAddresses.TryGetValue(aCellName.ToUpper(), out string _rVal) ? _rVal : "";


        }
        /// <summary>
        /// Method validates whether report can be generated
        /// </summary>
        /// <returns></returns>
        public bool PreValidateReportWriter(out string rBrief, out string rError)
        {
            //Type officeType = Type.GetTypeFromProgID("Excel.Application");

            //if (officeType == null)
            //{
            //    rError = @"This function is not currently available. MS Excel 2007 was not found installed on this PC. 
            //                    Contact your sytem administrator to have this application installed.";
            //    DialogService.ShowMessageBox(MainVM, rError, "MS Excel not found", MessageBoxButton.OK);
            //    return false;
            //}
            rBrief = string.Empty;
            rError = string.Empty;
            uopProject aProject = Project;
            if (aProject == null) return false;

            if (!aProject.HasReport) return false;

            //uopDocuments aReports = aProject.Reports;

            if (string.IsNullOrEmpty(aProject.LastSaveDate))
            {
                rError = "The Project Must Be Saved Prior To Executing The Report Function!";
                rBrief = "Unsaved Project Detected";

                return false;
            }

            if (aProject.TrayRanges.Count <= 0)
            {
                rError = "Reports Cannot Be Generated Until a Tray Section Is Added To The Project";

                rBrief = "No Defined Tray Sections Detected";

                return false;
            }

            if (aProject.HasChanged)
            {
                rError = "Unsaved Changes Detected";
                rBrief = "Unsaved Changes";
                return false;

            }
            return true;
        }
        /// <summary>
        /// the main entry point for the report reviosn marking functionality
        /// </summary>
        /// <param name="aReport">the report object that defines what report is bing requested</param>
        /// <returns>return is the error string if there is one. All warnings are added to the Report.Warnings collection.</returns>
        public void MarkRevisions(uopDocReport aReport)
        {
            MarkingRevisons = true;
            Report = null;
            if (aReport == null) return;
            Report = aReport;
            UseClosedXML = true;

            Project ??= aReport.Project;
            aReport.Project ??= Project;

            string fname1 = FileSpec;
            string fname2 = LastRevisionPath;

            if (!File.Exists(fname1)) { SaveWarning("MarkRevisions", "File Not Found", $"File '{fname1}' was not found.", uppWarningTypes.ReportFatal); return; }
            if (!File.Exists(fname2)) { SaveWarning("MarkRevisions", "File Not Found", $"File '{fname2}' was not found.", uppWarningTypes.ReportFatal); return; }
            if (uopUtils.FileIsOpen(fname1)) { SaveWarning("MarkRevisions", "File In Use", $"File '{fname1}' is locked another application.", uppWarningTypes.ReportFatal); return; }
            if (uopUtils.FileIsOpen(fname2)) { SaveWarning("MarkRevisions", "File In Use", $"File '{fname2}' is locked another application.", uppWarningTypes.ReportFatal); return; }

            try
            {

                if (WorkBookXML != null)
                {
                    WorkBookXML.Save();
                    WorkBookXML.Dispose();
                    WorkBookXML = null;
                }
                if (LastRevisionXML != null)
                {
                    LastRevisionXML.Dispose();
                    LastRevisionXML = null;
                }


                WorkBookXML = new XLWorkbook(fname1);
                LastRevisionXML = new XLWorkbook(fname2);

                WorksheetsXML = WorkBookXML.Worksheets.ToList();

                Status = $"Marking Revisions in '{Path.GetFileNameWithoutExtension(fname1)}' with Data From '{Path.GetFileNameWithoutExtension(fname2)}'";

                MarkRevisionsMD(WorkBookXML, LastRevisionXML);


            }
            catch (Exception e) { SaveWarning(System.Reflection.MethodBase.GetCurrentMethod().Name, $"Error Encounter Marhing Revision.", $"Marking Revisions in '{Path.GetFileNameWithoutExtension(fname1)}' with Data From '{Path.GetFileNameWithoutExtension(fname2)}'", uppWarningTypes.ReportFatal, e); }
            finally
            {
                if (aReport != null) aReport.CopyWarnings(Warnings);

                TerminateReferences(bRetainProject: false, bClearsErrorMsg: false, bAddressBackup: false, bProtectPages: true);

            }

        }
        /// <summary>
        /// the main entry point for the report generation functionality
        /// </summary>
        /// <param name="aReport">the report object that defines what report is bing requested</param>
        /// <returns>return is the error string if there is one. All warnings are added to the Report.Warnings collection.</returns>
        public string GenerateReport(uopDocReport aReport)
        {
            string _rVal = string.Empty;

            try
            {
                MarkingRevisons = false;
                TerminateReferences(bRetainProject: true, bClearsErrorMsg: true, bAddressBackup: false, bProtectPages: false);

                //trap
                if (aReport == null) { SaveWarning("GenerateReport", "Invalid Input", "The passed Report is null.", uppWarningTypes.ReportFatal); return ErrorMessage; ; }

                if (aReport.RequestedPages.Count <= 0) { SaveWarning("GenerateReport", "Invalid Input", "The passed Report doesn't contain any Requested Pages.", uppWarningTypes.ReportFatal); return ErrorMessage; ; }
                if (!Directory.Exists(aReport.FolderName)) { SaveWarning("GenerateReport", "Invalid Input", "The passed Report output FolderName does not exist.", uppWarningTypes.ReportFatal); return ErrorMessage; ; }
                if (string.IsNullOrWhiteSpace(aReport.FileName)) { SaveWarning("GenerateReport", "Invalid Input", "The passed Report Filename is undefined.", uppWarningTypes.ReportFatal); return ErrorMessage; ; }



                //initialize


                SetStatusMessages($"Creating Report - {aReport.ReportName}", "Inititializing");
                _Report = null;
                Report = aReport;

                bool bBackitUp = Report.ReportType == uppReportTypes.MDSpoutReport;

                if (File.Exists(FileSpec) && uopUtils.FileIsOpen(new FileInfo(FileSpec)))
                {

                    SaveWarning("GenerateReport", "File Is Open In System App", "Please close the existing report.", uppWarningTypes.ReportFatal); return ErrorMessage;
                }


                //make sure the report template is avaliable
                SetTemplatePath(Report, out string ext);
                if (Template == string.Empty || !File.Exists(Template))
                {

                    SaveWarning("GenerateReport", "Could not load template for report generation.", "", uppWarningTypes.ReportFatal); return ErrorMessage;

                }

                if (!UseClosedXML)
                {
                    WorkBook = CreateWorkBook(bBackitUp);

                    if (CancelIt) return ErrorMessage;


                    //run the code for the requested report type
                    ExecuteReport();
                    if (CancelIt) return ErrorMessage;

                    //add the page numbers and set the first activated tab
                    NumberSheets();
                    if (CancelIt) return ErrorMessage;



                    if (CancelIt) return ErrorMessage;

                    //close and save the work book
                    WorkBook.Save();


                    if (CancelIt)
                    {

                        SaveWarning("GenerateReport", "Report Generation Canceled Prematurely", "", uppWarningTypes.ReportFatal); return ErrorMessage;


                    }
                }
                else
                {
                    WorkBookXML = CreateWorkBookXML(bBackitUp);

                    if (CancelIt) return ErrorMessage;


                    //run the code for the requested report type
                    ExecuteReportXML();
                    if (CancelIt) return ErrorMessage;

                    //add the page numbers and set the first activated tab
                    NumberSheetsXML();
                    if (CancelIt) return ErrorMessage;


                    //apply protection to cells in the report
                    if (Report.ProtectSheets && !CancelIt)
                        ProtectReportPages();

                    if (CancelIt) return ErrorMessage;

                    //close and save the work book
                    WorkBookXML.Save(true);


                    if (CancelIt)
                    {
                        SaveWarning("GenerateReport", "Report Generation Canceled Prematurely", "", uppWarningTypes.ReportFatal); return ErrorMessage;
                    }



                }

                if (!CancelIt && (BackupIsPresent || LastRevIsPresent))
                    RecoverBackupData(UseClosedXML);



                ////try to recover any user data that might have been added since the last generation of this report
                //if (!CancelIt  && CanMarkRevision) 
                //MarkRevisions(FileSpec,LastRevisionPath);



            }
            catch (Exception e)
            {
                SaveWarning("GenerateReport", "Report GenerationUnhandled Error", "", uppWarningTypes.ReportFatal, e); return ErrorMessage;

            }
            finally
            {
                if (aReport != null) aReport.CopyWarnings(Warnings);
                TerminateReferences(bRetainProject: false, bClearsErrorMsg: false, bAddressBackup: true, bProtectPages: true);
                Status = string.Empty;
            }
            return _rVal;

        }


     
        public uopDocWarning SaveWarning(string aMethodName, string aBrief, string aTextString, uppWarningTypes wType, Exception e = null)
        {

            aMethodName = string.IsNullOrWhiteSpace(aMethodName) ? this.GetType().Name : $"{ this.GetType().Name }.{ aMethodName}";

            if (string.IsNullOrWhiteSpace(aBrief)) aBrief = aTextString;
            if (e != null)
            {
                aTextString = string.IsNullOrWhiteSpace(aTextString) ? e.Message : $"{aTextString.Trim()} - {e.Message}";
            }

            if (string.IsNullOrWhiteSpace(aTextString)) aTextString = aBrief;

            if (string.IsNullOrWhiteSpace(aTextString) && string.IsNullOrWhiteSpace(aBrief)) return null;

            aTextString = aTextString.Trim();
            aBrief = aBrief.Trim();

            ErrorMessage = aBrief;
            uopDocWarning _rVal = new()
            {
                Owner = aMethodName,
                Brief = aBrief,
                TextString = aTextString,
                WarningType = wType,

            };



            Warnings.Add(_rVal);

            if (wType == uppWarningTypes.ReportFatal)
            {
                CancelIt = true;
                FatalError = true;
                _CanMarkRevision = null;
            }

            return _rVal;
        }

        public void TerminateReferences()
        {
            TerminateReferences(false);
        }

        private void TerminateReferences(bool bRetainProject, bool bClearsErrorMsg = false, bool? bAddressBackup = null, bool bProtectPages = false)
        {
            string bacuppath = "";
            try
            {

                if (bProtectPages) ProtectReportPages();


                //delete or restore the backup
                if (bAddressBackup.HasValue)
                {
                    bacuppath = bAddressBackup.Value && File.Exists(BackupPath) ? bacuppath = BackupPath : "";

                }



                if (WorksheetsXML != null) WorksheetsXML.Clear();
                if (Report != null) Report.ErrorString = ErrorMessage;

                Warnings.Clear();
                if (bClearsErrorMsg) ErrorMessage = "";


                if (WorkBook != null)
                {
                    WorkBook.Save();
                    WorkBook.Close();
                }
                if (WorkBookXML! != null)
                {
                    WorkBookXML.Save();
                    WorkBookXML.Dispose();
                }
                if (LastRevisionXML! != null) LastRevisionXML.Dispose();


                if (Excel != null) Excel.Quit();
            }
            catch { }
            finally
            {
                ActiveSheet = null;
                ActiveSheetXML = null;
                BlankPage = null;

                WorksheetsXML = null;
                Worksheets = null;
                WorkBook = null;
                WorkBookXML = null;
                LastRevisionXML = null;
                Excel = null;
                if(!bRetainProject) Project = null;
                if (bacuppath != "")
                {

                    if (!CancelIt)
                        BackupDelete();
                    else
                        BackupRestore();
                }

            }
        }

        private void CreatePageWorksheets(bool bXML = false)
        {
            if (!bXML)
            {
                Worksheets = new List<ExcelIO.Worksheet>();
                WorksheetsXML = null;
                if (RequestedPages.Count <= 0) return;
                ExcelIO.Worksheet sheet;

                foreach (uopDocReportPage page in RequestedPages)
                {
                    page.Project ??= Project;
                    page.Units = Units;
                    sheet = CreatePageWorksheet(page);
                    if (sheet != null) Worksheets.Add(sheet);
                }
                if (Report.ReportType == uppReportTypes.MDDCStressReport)
                {
                    uopDocReportPage page = RequestedPages[0];
                    sheet = GetWorkSheet(page.TemplateTabName);
                    if (sheet != null) sheet.Visible = ExcelIO.XlSheetVisibility.xlSheetHidden;
                }
            }
            else
            {
                WorksheetsXML = new List<IXLWorksheet>();
                Worksheets = null;
                if (RequestedPages.Count <= 0) return;
                IXLWorksheet sheet;

                foreach (uopDocReportPage page in RequestedPages)
                {
                    page.Project ??= Project;
                    page.Units = Units;
                    sheet = CreatePageWorksheetXML(page);
                    if (sheet != null) WorksheetsXML.Add(sheet);
                }

            }


        }

        /// <summary>
        /// Create excel worksheet for each selected report page
        /// </summary>
        /// <param name="aPage"></param>
        /// <param name="bLastPage_UNUSED"></param>
        private ExcelIO.Worksheet CreatePageWorksheet(uopDocReportPage aPage)
        {
            if (WorkBook == null || aPage == null) return null;

            string tabname = aPage.TabName;
            int idx = 0;
            if (Report.ReportType == uppReportTypes.HardwareReport || !StandardFormat) NoRevisions = true;
            if (string.IsNullOrWhiteSpace(tabname)) tabname = "New Page";
            if (aPage.CopyCount > 1) tabname = $"{tabname}({aPage.CopyIndex})";
            if (aPage.SubPage > 0) tabname += $"{tabname}({aPage.SubPage})";
            //get the template page
            ExcelIO.Worksheet tmpltSht = GetWorkSheet(aPage.TemplateTabName, aDefault: BlankPage);

            Status = $"Creating Report WorkSheet - {tabname}";
            ExcelIO.Worksheet newSheet;


            if (tmpltSht == null)
            {
                newSheet = WorkBook.Worksheets.Add();
                newSheet.Name = tabname;
                newSheet = GetWorkSheet(tabname);
            }
            else
            {
                if (!aPage.NoTemplate)
                {
                    tmpltSht.Copy(After: GetWorkSheet(WorkBook.Worksheets.Count));
                    newSheet = GetWorkSheet(WorkBook.Worksheets.Count);
                    int tidx = 0;
                    string tname = tabname;
                    while(GetWorkSheet(tabname) != null)
                    {
                        tidx++;
                        tabname = $"{tname}_{tidx}";
                    }
                    newSheet.Name = tabname;
                }
                else
                {
                    newSheet = tmpltSht;
                }
                newSheet.Visible = ExcelIO.XlSheetVisibility.xlSheetVisible;
                newSheet.Activate();
                tabname = newSheet.Name;
            }

            if (!aPage.NoTemplate && !aPage.SuppressTabName)
            {
                List<ExcelIO.Worksheet> sheets = GetWorkSheets();

                for (int i = 1; i <= sheets.Count; i++)
                {
                    ExcelIO.Worksheet aSheet = sheets[i - 1];
                    if (!Object.ReferenceEquals(aSheet, newSheet))
                    {
                        if (string.Compare(aSheet.Name, tabname, true) == 0)
                        {
                            idx++;
                            if (!aPage.NoTemplate)
                            {
                                aSheet.Name = $"{aSheet.Name}({idx})";
                                tabname = aSheet.Name;
                            }
                        }
                    }

                }
            }



            if (NoRevisions && StandardFormat)
            {
                ActiveSheet = newSheet;
                ExcelIO.Range range = GetRange("AN9:AO62");
                range.Clear();
                BordersAround(range, ExcelIO.XlLineStyle.xlLineStyleNone);


                range = GetRange(CellAddress("RevCells"));
                range.Clear();
                BordersAround(range, ExcelIO.XlLineStyle.xlLineStyleNone);

                range = GetRange("A1:A61", bUnmerge: true);
                range.Clear();
                BordersAround(range, ExcelIO.XlLineStyle.xlLineStyleNone);

                range = GetRange("A61:AO62");
                BordersAround(range, ExcelIO.XlLineStyle.xlLineStyleNone);

                range = GetRange("B8");
                range.Clear();
                BordersAround(range, ExcelIO.XlLineStyle.xlContinuous);


                range = GetRange("B1AO8");
                BordersAround(range, ExcelIO.XlLineStyle.xlContinuous);

                ActiveSheet = null;
            }

            Excel.ActiveWindow.DisplayGridlines = false;
            aPage.TabName = newSheet.Name;
            if (aPage.Protected)
            {
                newSheet.Unprotect(aPage.Password);
            }

            return newSheet;
        }


        /// <summary>
        /// Create excel worksheet for each selected report page
        /// </summary>
        /// <param name="aPage"></param>
        /// <param name="bLastPage_UNUSED"></param>
        private IXLWorksheet CreatePageWorksheetXML(uopDocReportPage aPage)
        {
            if (WorkBookXML == null || aPage == null) return null;

            int idx = 0;
            if (Report.ReportType == uppReportTypes.HardwareReport || !StandardFormat) NoRevisions = true;


            string tbName = aPage.TabName;
            if (string.IsNullOrWhiteSpace(tbName)) tbName = "New Page";

            if (aPage.CopyCount > 1) tbName += string.Format("({0})", aPage.CopyIndex);

            if (aPage.SubPage > 0) tbName += string.Format("({0})", aPage.SubPage);


            IXLWorksheet tmpltSht = BlankPageXML;
            if (!string.IsNullOrWhiteSpace(aPage.TemplateTabName))
            {
                tmpltSht = WorkBookXML.Worksheet(aPage.TemplateTabName);
                tmpltSht ??= BlankPageXML;

            }

            if (!aPage.NoTemplate && !aPage.SuppressTabName)
            {
                if (string.IsNullOrWhiteSpace(tbName)) tbName = "New Page";
                if (aPage.CopyCount > 1) tbName += string.Format("({0})", aPage.CopyIndex);
                if (aPage.SubPage > 0) tbName += string.Format("({0})", aPage.SubPage);

                for (int i = 0; i < WorkBookXML.Worksheets.Count; i++)
                {
                    IXLWorksheet aSheet = WorkBookXML.Worksheets.Worksheet(i + 1);
                    if (aSheet.Name == tbName)
                    {
                        idx++;
                        if (!aPage.NoTemplate) aSheet.Name += string.Format("({0})", idx);

                    }
                }

                if (idx > 0)
                {
                    idx++;
                    tbName += string.Format("({0})", idx);
                }
            }


            IXLWorksheet newSheet;
            if (tmpltSht == null)
            {
                newSheet = WorkBookXML.Worksheets.Add(tbName);
            }
            else
            {
                newSheet = (!aPage.NoTemplate) ? tmpltSht.CopyTo(tbName) : tmpltSht;
                tbName = newSheet.Name;
            }
            newSheet.Visibility = XLWorksheetVisibility.Visible;

            Status = $"Creating Report WorkSheet - {tbName}";


            if (NoRevisions && StandardFormat)
            {
                IXLRange range = newSheet.Range("AN9:AO62");
                range.Clear();
                range.Style.Border.OutsideBorder = XLBorderStyleValues.None;

                range = newSheet.Range(CellAddress("RevCells"));
                range.Clear();
                range.Style.Border.OutsideBorder = XLBorderStyleValues.None;

                range = newSheet.Range("A1:A61");
                range.Unmerge();
                range.Clear();
                range.Style.Border.OutsideBorder = XLBorderStyleValues.None;

                range = newSheet.Range("A61:AO62");
                range.Style.Border.OutsideBorder = XLBorderStyleValues.None;

                range = newSheet.Range("B8");
                range.Clear();
                range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                range = newSheet.Range("B1AO8");
                range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }
            newSheet.ShowGridLines = false;

            aPage.TabName = newSheet.Name;
            if (aPage.Protected)
            {
                newSheet.Unprotect(aPage.Password);
            }
            return newSheet;
        }

        /// <summary>
        /// Set Template Path
        /// </summary>
        /// <param name="aReport"></param>
        private void SetTemplatePath(uopDocReport aReport, out string rExtenstion)
        {
            Template = string.Empty;
            BasePage = "Blank Page";
            rExtenstion = "";

            string fileName = aReport.TemplateName;
            if (string.IsNullOrWhiteSpace(fileName)) return;

            string fspec = appApplication.GetReportTemplatePath(aReport, ref fileName, out bool aFlg, out rExtenstion);

            fspec = appApplication.GetLocalReportTemplatePath(aFlg ? fspec : string.Empty, fileName);
            if (!string.IsNullOrEmpty(fspec) && File.Exists(fspec))
            {
                Template = fspec;

                rExtenstion = Path.GetExtension(Template).ToLower();
                string fname = Path.GetFileNameWithoutExtension(FileName);
                string ext = rExtenstion;
                if (ext == ".xltx") ext = ".xlsx";
                if (ext == ".xltm") ext = ".xlsm";
                FileName = fname + ext;





            }
            FileSpec = Path.Combine(WorkFolder, FileName);
        }

        /// <summary>
        /// creates and opens abackup version of the file passed
        /// </summary>
        /// <param name="aFileSpec"></param>


        /// <summary>
        /// creates and opens abackup version of the file passed
        /// </summary>
        /// <param name="aFileSpec"></param>
        private void BackupSave(string aFileSpec, bool bBackitUp)
        {
            aFileSpec = aFileSpec.Trim();
            if (bBackitUp)
            {
                BackupPath = "";

                if (aFileSpec == string.Empty) return;
                if (!File.Exists(aFileSpec)) return;
            }

            try
            {
                if (bBackitUp)
                {
                    string dir = Path.GetDirectoryName(aFileSpec);
                    string fname = Path.GetFileNameWithoutExtension(aFileSpec);
                    string ext = Path.GetExtension(aFileSpec);

                    BackupPath = Path.Combine(dir, $"{fname} [backup]{ext}");
                    if (File.Exists(BackupPath)) File.Delete(BackupPath);
                    File.Copy(aFileSpec, BackupPath, true);
                    File.Delete(aFileSpec);
                }
                else
                {
                    if (File.Exists(aFileSpec)) File.Delete(aFileSpec);
                }

            }
            catch (Exception e) { SaveWarning("BackupSave", "Error Encountered Creating Backup File ", BackupPath, uppWarningTypes.General, e); }
            finally
            {
                if (!File.Exists(BackupPath)) BackupPath = "";
            }


        }

        private ExcelIO.Workbook CreateWorkBook(bool bBackItUp)
        {
            ExcelIO.Workbook _rVal = null;

            //open the report page documents
            try
            {

                if (string.IsNullOrWhiteSpace(Template)) { SaveWarning("CreateWorkBook", "The template path is undefined.", "", uppWarningTypes.ReportFatal); return null; }

                if (!File.Exists(Template)) { SaveWarning("CreateWorkBook", $"The template '{Template}' was not found.", "", uppWarningTypes.ReportFatal); return null; }

                if (TemplateIsTemplate)
                    FileSpec = Path.Combine(WorkFolder, SaveAsFileName);


                //open the previous run of the current file and save it as a backup
                BackupSave(FileSpec, bBackItUp);

                //get a real instance of excel.  If this fails the user does not have excel installed on their machine.
                if (!GetExcel()) return null;

                if (TemplateIsTemplate)
                {

                    //File.Copy(Template, FileSpec, true);

                    _rVal = Excel.Workbooks.Add(Template);
                    _rVal.SaveAs(FileSpec, TemplateHasMacros ? ExcelIO.XlFileFormat.xlOpenXMLWorkbookMacroEnabled : ExcelIO.XlFileFormat.xlOpenXMLWorkbook);


                }
                else
                {
                    File.Copy(Template, FileSpec, true);
                    _rVal = Excel.Workbooks.Open(FileSpec);

                }

            }
            catch (Exception e) { SaveWarning(System.Reflection.MethodBase.GetCurrentMethod().Name, $"Could not load template '{Template}' for report generation.", "", uppWarningTypes.ReportFatal, e); }
            finally
            {


            }
            return _rVal;
        }

        private bool GetExcel()
        {
            bool _rVal = true;
            try
            {
                if (Excel == null) Excel = new ExcelIO.Application() { DisplayAlerts = false, Visible = false };
            }
            catch (Exception e)
            {
                SaveWarning("GetExcel", $"Unable To Start MS Excel.", "", uppWarningTypes.ReportFatal, e);
                _rVal = false;

            }
            return _rVal;


        }



        private XLWorkbook CreateWorkBookXML(bool bBackItUp)
        {
            XLWorkbook _rVal = null;

            ExcelIO.Application excel = null;
            ExcelIO.Workbook workbook = null;

            //open the report page documents
            try
            {

                if (string.IsNullOrWhiteSpace(Template))
                {
                    SaveWarning(System.Reflection.MethodBase.GetCurrentMethod().Name, "The template path is undefined.", "", uppWarningTypes.ReportFatal); return null;

                }

                if (!File.Exists(Template))
                {
                    SaveWarning(System.Reflection.MethodBase.GetCurrentMethod().Name, $"The template '{Template}' was not found.", "", uppWarningTypes.ReportFatal); return null;

                }

                if (TemplateIsTemplate)
                    FileSpec = Path.Combine(WorkFolder, SaveAsFileName);


                //open the previous run of the current file and save it as a backup
                BackupSave(FileSpec, bBackItUp);

                //get a real instance of excel.  If this fails the user does not have excel installed on their machine.


                if (TemplateIsTemplate)
                {
                    //File.Copy(Template, FileSpec, true);
                    excel = new ExcelIO.Application() { DisplayAlerts = false, Visible = false };
                    workbook = excel.Workbooks.Add(Template);
                    workbook.SaveAs(FileSpec, TemplateHasMacros ? ExcelIO.XlFileFormat.xlOpenXMLWorkbookMacroEnabled : ExcelIO.XlFileFormat.xlOpenXMLWorkbook);

                    workbook.Close();

                }
                else
                {
                    File.Copy(Template, FileSpec, true);


                }
                _rVal = new XLWorkbook(FileSpec);
            }
            catch (Exception e) { SaveWarning(System.Reflection.MethodBase.GetCurrentMethod().Name, $"Could not load template '{Template}' for report generation.", "", uppWarningTypes.ReportFatal, e); }
            finally
            {
                if (excel != null)
                {
                    excel.Quit();

                }
                excel = null;

            }
            return _rVal;
        }
        /// <summary>
        /// executed internally to generate the pages/worksheets of the report
        /// </summary>
        private void ExecuteReport()
        {

            if (WorkBook == null) throw new Exception("The workbook is not defined.");

            Status = $"Creating Report - {Report.FileName}";

            //get the blank page
            GetBlankPage();

            //fill in the rev block
            if (StandardFormat && ProjectType == uppProjectTypes.MDSpout)
                FillHeader_MD();

            //Create one workseet per requested page
            Status = "Creating Report WorkSheets";
            CreatePageWorksheets();


            //remove the blank page and any other unneed page
            RemoveNullPages();

            // fill in the worksheets based on the data in the request page

            Status = "Populating Report WorkSheets";
            foreach (uopDocReportPage page in RequestedPages)
            {
                CreatePage(page);
                if (CancelIt) break;
            }

            if (!CancelIt)
            {
                if (WorkBook.Worksheets.Count > 0)
                {
                    ExcelIO.Worksheet firstsheet = WorkBook.Worksheets.get_Item(1);
                    firstsheet.Activate();
                }



            }
        }

        /// <summary>
        /// executed internally to generate the pages/worksheets of the report
        /// </summary>
        private void ExecuteReportXML()
        {



            Status = $"Creating Report - {Report.FileName}";

            //get the last revision workbook
            if (Report.ReportType == uppReportTypes.MDSpoutReport)
                GetLastRevisionXML();

            //get the blank page
            GetBlankPageXML();

            if (StandardFormat)
            {
                if (ProjectType == uppProjectTypes.CrossFlow)
                {
                    //FillHeader_XF();
                }
                else
                {
                    FillHeader_MDXML();
                }
            }

            //Create one workseet per requested page
            CreatePageWorksheets(true);

            //remove the blank page and any other unneed page
            RemoveNullPagesXML();

            // fill in the worksheets based on the data in the request page

            Status = "Populating Report WorkSheets";
            foreach (uopDocReportPage page in RequestedPages)
            {

                if (Report.ReportType != uppReportTypes.MDDCStressReport) CreatePageXML(page);
                if (CancelIt) break;

            }


        }


        private void RecoverBackupData(bool bXML)
        {
            if (!BackupIsPresent && !LastRevIsPresent) return;
            if (bXML)
            {
                if (WorkBookXML == null) return;
                bool recovered;
                if (BackupIsPresent && LastRevIsPresent)
                {
                    recovered = RecoverMDSpoutDataXML(LastRevisionXML);
                } else if (BackupIsPresent)
                {
                    recovered = RecoverMDSpoutDataXML(BackupXML);
                }
                else
                {
                    recovered = RecoverMDSpoutDataXML(LastRevisionXML);
                }

                //close and save the work book
                if (recovered) WorkBookXML.Save();
            }
            else
            {
                if (WorkBook == null) return;

                bool recovered;
                ExcelIO.Workbook sourcefile = null;
                if (BackupIsPresent && LastRevIsPresent)
                {
                    sourcefile = LastRevision;
                    recovered = RecoverMDSpoutData(sourcefile);
                    if (sourcefile != null) sourcefile.Close();


                }
                else if (BackupIsPresent)
                {
                    sourcefile = Backup;
                    recovered = RecoverMDSpoutData(sourcefile);
                    if (sourcefile != null) sourcefile.Close();
                }
                else
                {
                    sourcefile = LastRevision;
                    recovered = RecoverMDSpoutData(sourcefile);
                    if (sourcefile != null) sourcefile.Close();
                }



                //close and save the work book
                if (recovered) WorkBook.Save();
            }
        }


        /// <summary>
        /// Returns number sheets
        /// </summary>
        private void NumberSheets()
        {
            if (WorkBook == null || Report == null || Worksheets == null)
                return;

            int i = 1;
            if (StandardFormat)
            {
                Status = "Numbering Sheets";
                foreach (ExcelIO.Worksheet sheet in Worksheets)
                {
                    ExcelIO.Range aRange = GetRange("Sheet1", sheet, bNamedRange: true);
                    aRange.Value = i;

                    aRange = GetRange("Sheet2", sheet, bNamedRange: true);
                    aRange.Value = Worksheets.Count;

                    i++;
                }
            }

            if (Worksheets.Count > 0)
            {
                ExcelIO.Worksheet aSheet = !string.IsNullOrWhiteSpace(Report.FirstTab) ? Worksheets.Find(x => string.Compare(x.Name, Report.FirstTab, true) == 0) : Worksheets[0];

                if (aSheet != null) aSheet.Activate();
            }
        }

        /// <summary>
        /// Returns number sheets
        /// </summary>
        private void NumberSheetsXML()
        {
            if (WorkBookXML == null || Report == null)
                return;

            int i = 1;
            if (StandardFormat)
            {
                Status = "Numbering Sheets";
                foreach (IXLWorksheet sheet in WorkBookXML.Worksheets)
                {
                    IXLRange aRange = sheet.Range("Sheet1");
                    aRange.Cell(1, 1).Value = i;

                    aRange = sheet.Range("Sheet2");
                    aRange.Cell(1, 1).Value = WorkBookXML.Worksheets.Count;

                    i++;
                }
            }

            if (WorkBookXML.Worksheets.Count > 0)
            {
                IXLWorksheet aSheet;
                aSheet = !string.IsNullOrWhiteSpace(Report.FirstTab) ? WorkBookXML.Worksheet(Report.FirstTab) : WorkBookXML.Worksheet(1);

                if (aSheet != null)
                {
                    aSheet.SetTabActive();
                }
            }
        }



        /// <summary>
        /// executed internally to set protection on individual sheets in the report
        /// </summary>
        private void ProtectReportPages()
        {
            Status = $"Setting Page Protections - {Report.FileName}";
            if (!UseClosedXML)
            {
                foreach (ExcelIO.Worksheet aSheet in Worksheets)
                {
                    try
                    {
                        uopDocReportPage sPage = RequestedPages.Find(x => string.Compare(x.TabName, aSheet.Name, true) == 0);


                        if (sPage == null) continue;

                        Status = $"Setting Page Protections - {sPage.PageName}";

                        if (!MarkingRevisons)
                        {


                            if (Report.ReportType == uppReportTypes.MDSpoutReport)
                            {
                                ExcelIO.Range aRange;
                                switch (sPage.PageType)
                                {
                                    case uppReportPageTypes.ColumnSketchPage:
                                        {
                                            aRange = GetRange("SignatureBlock", aSheet, bNamedRange: true, bProtection: false);
                                            aRange = GetRange("Body", aSheet, bNamedRange: true, bProtection: false);

                                            break;
                                        }
                                    case uppReportPageTypes.ProjectSummaryPage:
                                        {
                                            aRange = GetRange("Notes", aSheet, bNamedRange: true);
                                            aRange.Locked = false;

                                            //save the password to the sheet

                                            aRange = GetRange("Password", aSheet, bNamedRange: true);
                                            if (aRange != null)
                                            {

                                                aRange.Value = uopCryption.Crypt(sPage.Password, GlobalConstants.NETWORKKEY);
                                                aRange.Merge();
                                                aRange.Locked = true;
                                                aRange.Font.Size = 4;
                                                aRange.Font.Color = ColorTranslator.ToOle(Color.White);
                                            }
                                            break;
                                        }

                                }



                            }
                        }

                        try
                        {
                            if (sPage.Protected)
                            {
                                if (sPage.DontSaveWithPassword || string.IsNullOrWhiteSpace(sPage.Password))
                                    aSheet.Protect(DrawingObjects: false);
                                else
                                    aSheet.Protect(sPage.Password, DrawingObjects: false);

                            }
                        }
                        catch (Exception e) { SaveWarning(System.Reflection.MethodBase.GetCurrentMethod().Name, "Error encountered setting page protection", "", uppWarningTypes.General, e); }


                    }
                    catch (Exception e) { SaveWarning(System.Reflection.MethodBase.GetCurrentMethod().Name, "Error encountered setting page protection", "", uppWarningTypes.General, e); }
                }

            }
            else
            {
                foreach (IXLWorksheet aSheet in WorksheetsXML)
                {
                    try
                    {
                        uopDocReportPage sPage = RequestedPages.Find(x => string.Compare(x.TabName, aSheet.Name, true) == 0);


                        if (sPage == null) continue;

                        Status = $"Setting Page Protections - {sPage.PageName}";

                        if (!MarkingRevisons)
                        {


                            if (sPage.ReportType == uppReportTypes.MDSpoutReport)
                            {
                                IXLRange aRange;
                                switch (sPage.PageType)
                                {
                                    case uppReportPageTypes.ColumnSketchPage:
                                        {
                                            aRange = GetRangeXML("SignatureBlock", aSheet, bNamedRange: true, bProtection: false);
                                            aRange = GetRangeXML("Body", aSheet, bNamedRange: true, bProtection: false);

                                            break;
                                        }
                                    case uppReportPageTypes.ProjectSummaryPage:
                                        {
                                            aRange = GetRangeXML("Notes", aSheet, bNamedRange: true, bProtection: false);

                                            //save the password to the sheet

                                            aRange = GetRangeXML("Password", aSheet, bNamedRange: true, bProtection: false);
                                            if (aRange != null)
                                            {

                                                aRange.Value = uopCryption.Crypt(sPage.Password, GlobalConstants.NETWORKKEY);
                                                aRange.Merge();
                                                aRange.Style.Protection.Locked = true;
                                                aRange.Style.Font.FontSize = 4;
                                                aRange.Style.Font.FontColor = XLColor.White;
                                            }
                                            break;
                                        }

                                }



                            }
                        }

                        try
                        {
                            if (sPage.Protected)
                            {
                                //if (sPage.DontSaveWithPassword || !string.IsNullOrWhiteSpace(sPage.Password))
                                //    aSheet.Protect(allowedElements: XLSheetProtectionElements.EditObjects );
                                //else
                                //    aSheet.Protect(sPage.Password, XLProtectionAlgorithm.Algorithm.SimpleHash ,allowedElements: XLSheetProtectionElements.EditObjects);

                                if (sPage.Protected && !aSheet.IsProtected)
                                {

                                    IXLSheetProtection xLSheetProtection = sPage.DontSaveWithPassword ? aSheet.Protect() : aSheet.Protect(sPage.Password);
                                    xLSheetProtection.AllowElement(XLSheetProtectionElements.EditObjects, true);
                                }

                            }
                        }
                        catch (Exception e) { SaveWarning(System.Reflection.MethodBase.GetCurrentMethod().Name, "Error encountered setting page protection", "", uppWarningTypes.General, e); }


                    }
                    catch (Exception e) { SaveWarning(System.Reflection.MethodBase.GetCurrentMethod().Name, "Error encountered setting page protection", "", uppWarningTypes.General, e); }
                }
            }

        }


        private void GetBlankPage()
        {
            BlankPage = GetWorkSheet(BasePage);

        }

        private bool TryGetWorkSheet(string aTabName, ExcelIO.Workbook aWorkbook, out ExcelIO.Worksheet rSheet, bool bActivate = false)
        {
            bool _rVal = false;
            rSheet = null;
            try
            {
                rSheet = GetWorkSheet(aTabName, aWorkbook);
                _rVal = rSheet != null;
                if (_rVal & bActivate) rSheet.Activate();

            }
            catch { }

            return _rVal;
        }

        private bool TryGetWorkSheet(int aIndex, ExcelIO.Workbook aWorkbook, out ExcelIO.Worksheet rSheet, bool bActivate = false)
        {
            bool _rVal = false;
            rSheet = null;
            try
            {
                rSheet = GetWorkSheet(aIndex, aWorkbook);
                _rVal = rSheet != null;
                if (_rVal & bActivate) rSheet.Activate();

            }
            catch { }

            return _rVal;
        }

        private ExcelIO.Worksheet GetWorkSheet(string aTabName, ExcelIO.Workbook aWorkbook = null, ExcelIO.Worksheet aDefault = null)
        {
            aWorkbook ??= WorkBook;
            ExcelIO.Worksheet _rVal = aDefault;
            if (aWorkbook == null || string.IsNullOrWhiteSpace(aTabName)) return _rVal;

            foreach (ExcelIO.Worksheet item in aWorkbook.Worksheets)
            {
                if (string.Compare(item.Name, aTabName, true) == 0)
                {
                    _rVal = item;
                    break;
                }
            }


            return _rVal;
        }

        private ExcelIO.Worksheet GetWorkSheet(int aIndex, ExcelIO.Workbook aWorkbook = null, ExcelIO.Worksheet aDefault = null)
        {
            aWorkbook ??= WorkBook;
            ExcelIO.Worksheet _rVal = aDefault;
            if (aWorkbook == null) return _rVal;
            if (aIndex < 1 || aIndex > aWorkbook.Worksheets.Count)
            {
                SaveWarning("GetWorkSheet", "Worksheet Retrieval Error", $"The Requested Worksheet index {aIndex} does not exist in workbook '{aWorkbook}'", uppWarningTypes.General);
                return _rVal;
            }
            try
            {

            } catch (Exception e)
            {
                SaveWarning("GetWorkSheetXML", "Worksheet Retrieval Error", $"", uppWarningTypes.General, e);
            }
            _rVal = aWorkbook.Worksheets.get_Item(aIndex);

            return _rVal;
        }


        private IXLWorksheet GetWorkSheetXML(string aTabName, XLWorkbook aWorkbook = null)
        {
            return GetWorkSheetXML(aTabName, out int _, aWorkbook);
        }

        private IXLWorksheet GetWorkSheetXML(string aTabName, out int rIndex, XLWorkbook aWorkbook = null)
        {
            rIndex = 0;
            aWorkbook ??= WorkBookXML;
            IXLWorksheet _rVal = null;
            if (aWorkbook == null || string.IsNullOrWhiteSpace(aTabName)) return _rVal;
            int idx = 0;
            foreach (IXLWorksheet item in aWorkbook.Worksheets)
            {
                idx++;
                if (string.Compare(item.Name, aTabName, true) == 0)
                {
                    rIndex = idx;
                    _rVal = item;
                    break;
                }
            }

            if (_rVal == null)
                SaveWarning("GetWorkSheetXML", "Worksheet Retrieval Error", $"The Requested Worksheet{aTabName} does not exist in workbook '{aWorkbook}'", uppWarningTypes.General);

            return _rVal;
        }

        private IXLWorksheet GetWorkSheetXML(int aIndex, XLWorkbook aWorkbook = null)
        {
            aWorkbook ??= WorkBookXML;

            if (aWorkbook == null) return null;
            if (aIndex < 1 || aIndex > aWorkbook.Worksheets.Count)
            {
                SaveWarning("GetWorkSheetXML", "Worksheet Retrieval Error", $"The Requested Index{aIndex} Is Less than Or Greater Than the number of sheets in {aWorkbook}", uppWarningTypes.General);
                return null;
            }

            IXLWorksheet _rVal = null;
            try
            {
                _rVal = aWorkbook.Worksheet(aIndex);

            }
            catch (Exception e)
            {
                SaveWarning("GetWorkSheetXML", "Worksheet Retrieval Error", $"", uppWarningTypes.General, e);
            }

            return _rVal;
        }
        private List<ExcelIO.Worksheet> GetWorkSheets(ExcelIO.Workbook aWorkbook = null)
        {
            aWorkbook ??= WorkBook;
            if (aWorkbook == null) return new List<ExcelIO.Worksheet>();
            return aWorkbook.Worksheets.OfType<ExcelIO.Worksheet>().ToList();

        }

        private IXLRange GetRangeXML(string aAddress, IXLWorksheet aSheet = null, bool? bUnmerge = null, bool bNamedRange = false, bool? bProtection = null)
        {
            aSheet ??= ActiveSheetXML;
            if (aSheet == null || string.IsNullOrWhiteSpace(aAddress)) return null;
            aAddress = aAddress.Trim();
            if (bNamedRange)
            {
                aAddress = CellAddress(aAddress);
                if (string.IsNullOrWhiteSpace(aAddress)) return null;
            }
            IXLRange _rVal = null;
            try
            {
                _rVal = aSheet.Range(aAddress);
                if (_rVal == null)
                {
                    SaveWarning("GetRangeXML", $"Range Retrieval Error '{aAddress}'", "", uppWarningTypes.General);
                    return _rVal;
                }
                if (bUnmerge.HasValue)
                {
                    if (bUnmerge.Value == true)
                        _rVal.Unmerge();
                }
                if (bProtection.HasValue)
                {
                    _rVal.Style.Protection.Locked = bProtection.Value;
                }

            }
            catch (Exception e) { SaveWarning("GetRangeXML", $"Range Retrieval Error '{aAddress}'", "", uppWarningTypes.General, e); }



            return _rVal;
        }

        private ExcelIO.Range GetRange(string aAddress, ExcelIO.Worksheet aSheet = null, bool? bUnmerge = null, bool bNamedRange = false, bool? bProtection = null)
        {
            aSheet ??= ActiveSheet;
            if (aSheet == null || string.IsNullOrWhiteSpace(aAddress)) return null;
          
            aAddress = aAddress.Trim().Replace(" ","");
            if (bNamedRange)
            {
                aAddress = CellAddress(aAddress);
                if (string.IsNullOrWhiteSpace(aAddress)) return null;
                if (aAddress.Contains(" "))
                {
                    Console.WriteLine(aAddress);
                }
                aAddress = aAddress.Trim().Replace(" ", "");
            }
            else 
            {
                if (aAddress.Contains(" "))
                {
                    Console.WriteLine(aAddress);
                }
                aAddress = aAddress.Trim().Replace(" ", "");
            }
         
            ExcelIO.Range _rVal = null;
            try
            {
                _rVal = aSheet.Range[aAddress];
                if (_rVal == null)
                {
                    SaveWarning("GetRange", $"Range Retrieval Error '{aAddress}'", "", uppWarningTypes.General);
                    return _rVal;
                }

                if (bProtection.HasValue)
                {
                    _rVal.Locked = bProtection.Value;
                }

                if (bUnmerge.HasValue)
                {
                    if (bUnmerge.Value)
                        _rVal.UnMerge();
                }

            }
            catch (Exception e) { SaveWarning("GetRange", $"Range Retrieval Error '{aAddress}'", "", uppWarningTypes.General, e); }


            return _rVal;
        }

        private string GetRangeString(string aAddress, out bool rFound, ExcelIO.Worksheet aSheet = null, bool bTrim = true, bool? bUnmerge = null, bool bNamedRange = false)
        {
            rFound = false;
            ExcelIO.Range range = GetRange(aAddress, aSheet, bUnmerge, bNamedRange);

            if (range == null) return "";
            rFound = true;

            return GetRangeString(range, bTrim);
        }

        private string GetRangeStringXML(string aAddress, out bool rFound, IXLWorksheet aSheet = null, bool bTrim = true, bool? bUnmerge = null, bool bNamedRange = false)
        {
            rFound = false;
            IXLRange range = GetRangeXML(aAddress, aSheet, bUnmerge, bNamedRange);

            if (range == null) return "";
            rFound = true;

            return bTrim ? range.Cell(1, 1).GetString().Trim() : range.Cell(1, 1).GetString();
        }
        private string GetRangeStringXML(string aAddress,  IXLWorksheet aSheet = null, bool bTrim = true, bool? bUnmerge = null, bool bNamedRange = false)
        {
            return GetRangeStringXML(aAddress, out bool _, aSheet, bTrim, bUnmerge, bNamedRange);
        }

        private string GetRangeString(ExcelIO.Range aRange, bool bTrim = true)
        {
            if (aRange == null) return "";


            try
            {
                object celval = aRange.Value2;
                if (celval == null) return "";
                string _rVal = "";
                if (celval.GetType().IsArray)
                {
                    object celval2 = null;
                    foreach (object obj in (Array)celval)
                    {
                        celval2 = obj;
                        if (celval2 != null) break;
                    }
                    celval = celval2;
                }
                if (celval == null) return "";
                _rVal = Convert.ToString(celval);
                return bTrim ? _rVal.Trim() : _rVal;
            }
            catch
            {
                return "";
            }

        }

        private IXLCell GetCellXML(string aAddress, IXLWorksheet aSheet = null)
        {
            aSheet ??= ActiveSheetXML;
            if (aSheet == null || string.IsNullOrWhiteSpace(aAddress)) return null;
            aAddress = aAddress.Trim();

            return aSheet.Cell(aAddress);
        }

        private ExcelIO.Range GetCell(string aAddress, ExcelIO.Worksheet aSheet = null)
        {
            aSheet ??= ActiveSheet;
            if (aSheet == null || string.IsNullOrWhiteSpace(aAddress)) return null;
            aAddress = aAddress.Trim();

            return aSheet.Range[aAddress];
        }

        private IXLCell GetCellXML(IXLRange aRange, int aRow, int aCol)
        {
            if (aRange == null) return null;
         
            if (aRow < 1 || aRow > aRange.RowCount()) return null;
            if (aCol < 1 || aCol > aRange.ColumnCount()) return null;

            IXLCell _rVal = aRange.Cell(aRow, aCol);
            return _rVal;
        }

        private ExcelIO.Range GetCell(ExcelIO.Range aRange, int aRow, int aCol)
        {
            if (aRange == null) return null;
            if (aRange.Cells.Count <= 1) return aRange;
            if (aRow < 1 || aRow > aRange.Rows.Count) return null;
            if (aCol < 1 || aCol > aRange.Columns.Count) return null;

            ExcelIO.Range _rVal = aRange.Cells[aRow, aCol];
            return _rVal;
        }

        private IXLCell GetCellXML(IXLWorksheet aSheet, int aRow, int aCol)
        {
            aSheet ??= ActiveSheetXML;
            if (aSheet == null) return null;
            if (aRow < 1 || aRow > aSheet.RowCount()) return null;
            IXLRow row = aSheet.Row(aRow);
            if (aCol < 1 || aCol > row.CellCount()) return null;

            IXLCell _rVal = row.Cell(aCol);

            return _rVal;
        }

        private ExcelIO.Range GetCell(ExcelIO.Worksheet aSheet, int aRow, int aCol)
        {
            aSheet ??= ActiveSheet;
            if (aSheet == null) return null;
            if (aRow < 1 || aRow > aSheet.Rows.Count) return null;
            if (aCol < 1 || aCol > aSheet.Columns.Count) return null;
            ExcelIO.Range row = aSheet.Rows.get_Item(aRow);


            return GetCell(row, aRow, aCol);
        }


        private IXLCell GetCellXML(IXLWorksheet aSheet, string aAddress, int aRow = 1, int aCol = 1, bool bNamedRange = false )
        {
            return GetCellXML(GetRangeXML(aAddress, aSheet, bNamedRange: bNamedRange), aRow, aCol);

        }


        private void BordersAround(ExcelIO.Range aRange, ExcelIO.XlLineStyle aBorderStyle)
        {
            if (aRange == null) return;
            aRange.Borders[ExcelIO.XlBordersIndex.xlEdgeBottom].LineStyle = aBorderStyle;
            aRange.Borders[ExcelIO.XlBordersIndex.xlEdgeLeft].LineStyle = aBorderStyle;
            aRange.Borders[ExcelIO.XlBordersIndex.xlEdgeRight].LineStyle = aBorderStyle;
            aRange.Borders[ExcelIO.XlBordersIndex.xlEdgeTop].LineStyle = aBorderStyle;

        }

        private void SetBorders(ExcelIO.Range aRange, ExcelIO.XlBordersIndex aBorderIndex, ExcelIO.XlLineStyle aBorderStyle)
        {
            if (aRange == null) return;
            aRange.Borders[aBorderIndex].LineStyle = aBorderStyle;


        }


        private void GetBlankPageXML()
        {
            WorkBookXML.Worksheets.TryGetWorksheet(BasePage, out IXLWorksheet bp);
            BlankPageXML = bp ?? null;
        }


        /// <summary>
        /// attemps to mark the changes between revisions of the report
        /// </summary>
        private void GetLastRevisionXML()
        {
            LastRevisionXML = null;

            if (Report == null || WorkBookXML == null || Project == null) return;


            string fileSpec = WorkBookXML.ToString().Substring(WorkBookXML.ToString().IndexOf("(") + 1, WorkBookXML.ToString().Length - 12);
            string aStr = $"{Project.KeyNumber}-{ProjectRev} - ";
            string pname = $"{Project.KeyNumber}-{ProjectRev - 1} - ";
            fileSpec = fileSpec.Replace(aStr, pname);
            if (!File.Exists(fileSpec))
            {
                return;
            }

            LastRevisionXML = new XLWorkbook(fileSpec);
        }


        private void FillHeader_MD()
        {
            //^fills the template page with the current info for copy to all subsequent pages
            if (Project == null || BlankPage == null || !StandardFormat) return;


            string dt = Project.LastSaveDate;
            dt = mzUtils.IsDate(dt) ? Convert.ToDateTime(dt).ToString(DATE_FORMAT) : DateTime.Now.ToString(DATE_FORMAT);

            int rev = ProjectRev + 1;
            RevDate = DateTime.Now.ToString(DATE_FORMAT);

            RevInits = Project.Designer;

            //if (LastRevision != null && LastRevision.Worksheets.Count > 0)
            //{
            //    IXLWorksheet aSheet = LastRevision.Worksheets.Item(1);
            //    IXLRange aRange = aSheet.Range("V5:AO8");
            //    IXLRange bRange = BlankPage.Range["V5:AO8");
            //    aRange.CopyTo(bRange);
            //}

            BlankPage.Range["IDNumber"].Value = Project.IDNumber;
            BlankPage.Range["SAPNumber"].Value = Project.SAPNumber;
            BlankPage.Range["KeyNumber"].Value = Project.KeyNumber;
            BlankPage.Range["Customer"].Value = Project.Customer.Name;
            BlankPage.Range["Location"].Value = Project.Customer.Location;
            BlankPage.Range["ItemNumber"].Value = Project.Customer.Item;
            BlankPage.Range["Service"].Value = Project.Customer.Service;
            BlankPage.Range["Licensor"].Value = Project.ProcessLicensor;
            BlankPage.Range["Contractor"].Value = Project.Contractor;

            if (rev == 1 || rev > 8)
            {
                BlankPage.Range["RevNum1"].Value = $"'{ProjectRev}";
                BlankPage.Range["RevDate1"].Value = RevDate;
                BlankPage.Range["RevInitials1"].Value = Project.Designer;
            }
            else
            {
                BlankPage.Range[$"RevNum{rev}"].Value = $"'{ProjectRev}";
                BlankPage.Range[$"RevDate{rev}"].Value = dt;
                BlankPage.Range[$"RevInitials{rev}"].Value = Project.Designer;
            }
        }


        private void FillHeader_MDXML()
        {
            //^fills the template page with the current info for copy to all subsequent pages
            if (Project == null || BlankPageXML == null || !StandardFormat) return;


            string dt = Project.LastSaveDate;
            dt = mzUtils.IsDate(dt) ? Convert.ToDateTime(dt).ToString(DATE_FORMAT) : DateTime.Now.ToString(DATE_FORMAT);

            int rev = ProjectRev + 1;
            RevDate = DateTime.Now.ToString(DATE_FORMAT);

            RevInits = Project.Designer;

            //if (LastRevision != null && LastRevision.Worksheets.Count > 0)
            //{
            //    IXLWorksheet aSheet = LastRevision.Worksheets.Item(1);
            //    IXLRange aRange = aSheet.Range("V5:AO8");
            //    IXLRange bRange = BlankPageXML.Range("V5:AO8");
            //    aRange.CopyTo(bRange);
            //}

            BlankPageXML.Range("IDNumber").Value = Project.IDNumber;
            BlankPageXML.Range("SAPNumber").Value = Project.SAPNumber;
            BlankPageXML.Range("KeyNumber").Value = Project.KeyNumber;
            BlankPageXML.Range("Customer").Value = Project.Customer.Name;
            BlankPageXML.Range("Location").Value = Project.Customer.Location;
            BlankPageXML.Range("ItemNumber").Value = Project.Customer.Item;
            BlankPageXML.Range("Service").Value = Project.Customer.Service;
            BlankPageXML.Range("Licensor").Value = Project.ProcessLicensor;
            BlankPageXML.Range("Contractor").Value = Project.Contractor;

            if (rev == 1 || rev > 8)
            {
                BlankPageXML.Range("RevNum1").Value = ProjectRev;
                BlankPageXML.Range("RevDate1").Value = RevDate;
                BlankPageXML.Range("RevInitials1").Value = Project.Designer;
            }
            else
            {
                BlankPageXML.Range("RevNum" + rev).Value = ProjectRev;
                BlankPageXML.Range("RevDate" + rev).Value = dt;
                BlankPageXML.Range("RevInitials" + rev).Value = Project.Designer;
            }
        }

        private void RemoveNullPages()
        {

            string sname = string.Empty;

            if (BlankPage != null)
            {
                sname = BasePage;
                BlankPage = null;
            }

            for (int i = WorkBook.Worksheets.Count; i > 0; i--)
            {
                bool bKill = false;
                ExcelIO.Worksheet aSheet = GetWorkSheet(i);
                if (string.Compare(aSheet.Name, "Blank Page", true) == 0 || string.Compare(aSheet.Name, sname, true) == 0)
                {
                    bKill = true;
                }
                if (Report.ReportType == uppReportTypes.MDDCStressReport && string.Compare(aSheet.Name, "DCSTRENGTH", true) == 0)
                {
                    bKill = true;
                }
                if (bKill)
                {
                    Status = $"Removing - {aSheet.Name}";
                    try
                    {
                        aSheet.Delete();

                    }
                    catch { aSheet.Visible = ExcelIO.XlSheetVisibility.xlSheetHidden; }
                }
            }

        }


        private void RemoveNullPagesXML()
        {

            string sname = string.Empty;

            if (BlankPageXML != null)
            {
                sname = BasePage;
                BlankPageXML = null;
            }

            for (int i = WorkBookXML.Worksheets.Count; i > 0; i--)
            {
                bool bKill = false;
                IXLWorksheet aSheet = WorkBookXML.Worksheet(i);
                if (aSheet.Name == "Blank Page" || aSheet.Name == sname)
                {
                    bKill = true;
                }
                if (Report.ReportType == uppReportTypes.MDDCStressReport && aSheet.Name == "DCSTRENGTH")
                {
                    // bKill = true;
                }
                if (bKill)
                {
                    Status = $"Removing - {aSheet.Name}";
                    try
                    {
                        aSheet.Delete();

                    }
                    catch { aSheet.Visibility = XLWorksheetVisibility.Hidden; }
                }
            }

        }

        /// <summary>
        /// creates the passed page in excel
        /// </summary>
        /// <param name="aPage"></param>
        private void CreatePage(uopDocReportPage aPage)
        {
            ExcelIO.Range exRange;
            uopDocDrawing aDwg = null;
            //if (Report.ReportType == uppReportTypes.MDDCStressReport)
            //{
            //    return ;
            //}
            uopTableCell pCell = null;
            string reply = string.Empty;
            ExcelIO.Worksheet wSheet = Worksheets.Find(x => string.Compare(x.Name, aPage.TabName, true) == 0);
            if (wSheet == null) return;

            if (!string.IsNullOrWhiteSpace(aPage.Tag)) AddTagCell(aPage, aPage.Tag, aPage.TagAddress);
            bool protectit = wSheet.ProtectContents || !string.IsNullOrWhiteSpace(aPage.Password) || aPage.Protected;
            string imgfile = "";
            try
            {

                ActiveSheet = wSheet;
                ActivePage = aPage;

                if (wSheet.ProtectContents)
                {
                    if (!string.IsNullOrWhiteSpace(aPage.Password))
                        wSheet.Unprotect(aPage.Password);
                    else
                        wSheet.Unprotect();

                }


                //get the latest data
                Status = $"Populating Page - {aPage.TabName}";
                uopReportGenerator.PopulateReportPage(Project, aPage, Units, this.Warnings);
                if (Report.ReportType == uppReportTypes.HardwareReport)
                {
                    if (aPage.PageType == uppReportPageTypes.HardwareTotals)
                    {
                        aPage.Tables[0].SetValue("DESIGNER", appApplication.User.Initials);
                    }
                }


                if (string.IsNullOrWhiteSpace(aPage.SubTitle1) && !string.IsNullOrWhiteSpace(aPage.SpanName))
                {
                    aPage.SubTitle1 = $"Trays {aPage.SpanName}";
                }


                if (StandardFormat)
                {
                    GetRange(CellAddress("Title1"), wSheet).Value = aPage.Title;
                    GetRange(CellAddress("Title2"), wSheet).Value = aPage.SubTitle1;
                    GetRange(CellAddress("Title3"), wSheet).Value = aPage.SubTitle2;
                    //special case
                    if (Report.ReportType == uppReportTypes.MDSpoutReport && aPage.PageType == uppReportPageTypes.ProjectSummaryPage)
                    {
                        exRange = GetRange("C61:L61", wSheet, bUnmerge: true);

                        exRange.Value = $"WinTray Version {appApplication.AppVersion}";
                        exRange.Font.Name = "Arial";
                        exRange.Font.Size = 7;
                        exRange.Locked = true;
                        exRange.Merge();
                    }
                }


                Status = $"Writing Page - {aPage.TitleText}";

                //get the define cells in the page
                List<uopTableCell> pCells = aPage.DefinedCells(bAddressedOnly: false);


                for (int i = 0; i < pCells.Count; i++)
                {
                    try
                    {
                        pCell = pCells[i];

                        pCell.DisplayUnits = aPage.DisplayUnits;
                        //create the excel range for the cell
                        exRange = GetCellRange(pCell, wSheet, aPage.Units, out string cellerror, WorkBook);
                        if (!string.IsNullOrWhiteSpace(cellerror)) SaveWarning(System.Reflection.MethodBase.GetCurrentMethod().Name, "Range Retrieval Error", $"{aPage.TabName} - {cellerror}", uppWarningTypes.General);
                        if (exRange != null)
                        {
                            if (pCell.DataOnly)
                            {
                                try
                                {

                                    exRange.Value = pCell.Numeric ?
                                    Math.Round(pCell.ValueD, 6) :
                                    pCell.ValueS;
                                }
                                catch (Exception e)
                                {
                                    exRange = null;
                                    SaveWarning(System.Reflection.MethodBase.GetCurrentMethod().Name, "Range Data Error", $"{aPage.TabName}", uppWarningTypes.General, e);
                                    continue;
                                }



                            }
                            else
                            {


                                // set the cell content
                                SetCellValue(exRange, pCell);

                                //special case
                                if (aPage.PageType == uppReportPageTypes.MDDistributorPage && pCell.Col == 1 && (pCell.Row == 24 || pCell.Row == 30 || pCell.Row == 35))
                                {
                                    int j = Convert.ToString(exRange.Value).IndexOf($"rv{Convert.ToChar(178)}");
                                    if (j > 0)
                                    {
                                        //if(exRange.HasRichText) exRange.GetRichText().Substring(j, 1).SetFontName("Symbol");
                                    }


                                }

                                //special case
                                if (aPage.PageType == uppReportPageTypes.MDEDMPage1 && pCell.Row == 20 && pCell.Col == 3) exRange.Value = appApplication.User.NiceName;




                            }
                        }

                    }
                    catch (Exception e)
                    {
                        string err = $"Worksheet Cell Generation Error { aPage.TitleText}";
                        if (pCell != null) err = $"{err} - Cell[{pCell.Address}]";
                        SaveWarning(System.Reflection.MethodBase.GetCurrentMethod().Name, err, "", uppWarningTypes.ReportFatal, e);
                        continue;
                    }


                    if (CancelIt) return;

                }


                ////drawings
                dxfImage image = null;
                for (int j = 1; j <= aPage.Drawings.Count; j++)
                {

                    try
                    {
                        //tell the form to create the sketch and put it on the clipboard

                        aDwg = aPage.Drawings[j- 1];

                        if (aDwg.Project == null) aDwg.ProjectHandle = Project.Handle;

                        Status = $"Generating Drawing '{aDwg.DrawingName}'";

                        image = ReportGeneratorVM.CreateDrawing(aDwg, BackColor: Color.White);

                        //past the clipboard bitmap into the sheet if it was succesfully created
                        if (image != null)
                        {
                            // very important!!! black image results without this line
                            image.Display.ZoomExtents();
                            if (string.IsNullOrWhiteSpace(aDwg.PasteAddress)) aDwg.PasteAddress = "D13";
                            exRange = GetRange(aDwg.PasteAddress, wSheet);
                            //string currentPath = new Uri(Path.GetDirectoryName(Template)).LocalPath;
                            //string filePath = Path.Combine(currentPath, aDwg.DrawingName + ".jpg");
                            dxfBitmap dmap = image.Bitmap(false);
                            //dmap.FloodFill(Color.Red);
                            //dmap.CopyToClipBoard();
                            Bitmap bmap = (Bitmap)dmap.Image;
                            //bmap.Save(filePath, ImageFormat.Jpeg);
                            //System.Windows.Forms.Clipboard.GetImage().Save(filePath, ImageFormat.Jpeg);
                            //wSheet.AddPicture(filePath).MoveTo(exRange);

                            //MemoryStream bmapstream = new MemoryStream();
                            //bmap.Save(bmapstream, ImageFormat.Tiff);
                            imgfile = Path.Combine(Path.GetTempPath(), $"{Path.GetTempFileName()}.bmp");
                            bmap.Save(imgfile);
                            ExcelIO.Shape picture = wSheet.Shapes.AddPicture(imgfile, Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoTriState.msoTrue, exRange.Left, exRange.Top, bmap.Width * 72 / 96, bmap.Height * 72 / 96);

                        }
                        else
                        {
                            //exRange = GetRange(aDwg.PasteAddress, wSheet);
                            //exRange.Value = reply;
                        }

                        if (aDwg.Tag != string.Empty && aDwg.TagAddress != string.Empty)
                        {
                            AddTagCell(aPage, aDwg.Tag, aDwg.TagAddress);
                        }
                    }
                    catch (Exception e)
                    {
                        string err = $"Worksheet Cell Generation Error { aPage.TitleText}";
                        if (aDwg != null) err = $"{err} - Drawing[{aDwg.DrawingName}]";
                        SaveWarning(System.Reflection.MethodBase.GetCurrentMethod().Name, err, "", uppWarningTypes.ReportFatal, e);
                        continue;

                    }
                    finally
                    {

                        if (image != null) image.Dispose();
                        image = null;
                    }




                }

                //graphs
                //for (int j = 0; j < aPage.Graphs.Count; j++)
                //{
                //    //tell the form to create the sketch and put it on the clipboard
                //    aGraph = aPage.Graphs[j];

                //    if (aPage.SpanName != "")
                //    {
                //        Status = "Generating Drawing - Tray " + aPage.SpanName + " - " + aGraph.Title;
                //    }
                //    else
                //    {
                //        Status = "Generating Drawing - " + aGraph.Title;
                //    }
                //    if (aGraph.SeperateSheet)
                //    {
                //        //TODO
                //        //aGraph.Chart = WorkBook.Charts.Add(_, wSheet);
                //    }

                //    CreateChart(aPage, aGraph);
                //    
                //}

            }
            catch (Exception e)
            {
                SaveWarning(System.Reflection.MethodBase.GetCurrentMethod().Name, $"Page Generation Error '{ aPage.TabName}'", "", uppWarningTypes.ReportFatal, e);


            }
            finally
            {
                //if (protectit)
                //{
                //    if (!string.IsNullOrWhiteSpace(aPage.Password))
                //        wSheet.Protect(aPage.Password);
                //    else
                //        wSheet.Protect();

                //}

                ActiveSheet = null;
                ActivePage = null;
                if (!string.IsNullOrWhiteSpace(imgfile))
                {
                    if (File.Exists(imgfile)) File.Delete(imgfile);
                }
            }




        }


        /// <summary>
        /// creates the passed page in excel
        /// </summary>
        /// <param name="aPage"></param>
        private void CreatePageXML(uopDocReportPage aPage)
        {
            IXLRange exRange;
            uopDocDrawing aDwg = null;
            //if (Report.ReportType == uppReportTypes.MDDCStressReport)
            //{
            //    return ;
            //}

            string reply = string.Empty;
            IXLWorksheet wSheet = WorksheetsXML.Find(x => string.Compare(x.Name, aPage.TabName, true) == 0);
            if (wSheet == null) return;
            if (!string.IsNullOrWhiteSpace(aPage.Tag)) AddTagCellXML(aPage, aPage.Tag, aPage.TagAddress);

            if (!string.IsNullOrWhiteSpace(aPage.Password)) wSheet.Unprotect(aPage.Password);

            //get the latest data
            Status = $"Populating Page - {aPage.TabName}";
            uopReportGenerator.PopulateReportPage(Project, aPage, Units, this.Warnings);
            if (Report.ReportType == uppReportTypes.HardwareReport)
            {
                if (aPage.PageType == uppReportPageTypes.HardwareTotals)
                {
                    aPage.Tables[0].NamedCell("DESIGNER").Value = appApplication.User.Initials;
                }
            }


            if (string.IsNullOrWhiteSpace(aPage.SubTitle1) && !string.IsNullOrWhiteSpace(aPage.SpanName))
            {
                aPage.SubTitle1 = $"Trays {aPage.SpanName}";
            }


            if (StandardFormat)
            {
                wSheet.Range(CellAddress("Title1")).Value = aPage.Title;
                wSheet.Range(CellAddress("Title2")).Value = aPage.SubTitle1;
                wSheet.Range(CellAddress("Title3")).Value = aPage.SubTitle2;
            }

            string titl = aPage.Title;
            if (!string.IsNullOrWhiteSpace(aPage.SubTitle1))
            {
                if (titl != string.Empty) titl += " - ";
                titl += aPage.SubTitle1;
            }

            if (!string.IsNullOrWhiteSpace(aPage.SubTitle2))
            {
                if (!string.IsNullOrWhiteSpace(titl)) titl += " - ";
                titl += aPage.SubTitle2;
            }
            titl = string.IsNullOrWhiteSpace(titl) ? aPage.TabName : $"{aPage.TabName} / {titl}";

            Status = $"Writing Page - {titl}";

            //get the define cells in the page
            List<uopTableCell> pCells = aPage.DefinedCells(bAddressedOnly: false);

            if (aPage.PageType == uppReportPageTypes.DCStressPage) return;


            for (int i = 0; i < pCells.Count; i++)
            {
                try
                {
                    uopTableCell pCell = pCells[i];

                    pCell.DisplayUnits = aPage.DisplayUnits;
                    //create the excel range for the cell
                    exRange = GetCellRangeXML(pCell, wSheet, aPage.Units, out string cellerror, WorkBookXML);
                    if (!string.IsNullOrWhiteSpace(cellerror)) SaveWarning(System.Reflection.MethodBase.GetCurrentMethod().Name, "Range Retrieval Error", $"{aPage.TabName} - {cellerror}", uppWarningTypes.General);
                    if (exRange != null)
                    {
                        if (pCell.DataOnly)
                        {
                            try
                            {

                                exRange.Value = pCell.Numeric ?
                                XLCellValue.FromObject(Math.Round(pCell.ValueD, 6), null) :
                                XLCellValue.FromObject(pCell.ValueS, null);
                            }
                            catch (Exception e)
                            {
                                exRange = null;
                                SaveWarning(System.Reflection.MethodBase.GetCurrentMethod().Name, "Range Data Error", $"{aPage.TabName}", uppWarningTypes.ReportFatal, e);
                                continue;
                            }



                        }
                        else
                        {


                            // set the cell content
                            SetCellValueXML(exRange, pCell, bNonBlankCellsOnly: true);

                            //special case
                            if (aPage.PageType == uppReportPageTypes.MDDistributorPage && pCell.Col == 1 && (pCell.Row == 24 || pCell.Row == 30 || pCell.Row == 35))
                            {
                                int j = Convert.ToString(exRange.Cell(1, 1).Value).IndexOf("rv" + Convert.ToChar(178));
                                if (j > 0 && exRange.Cell(1, 1).HasRichText) exRange.Cell(1, 1).GetRichText().Substring(j, 1).SetFontName("Symbol");

                            }

                            //special case
                            if (aPage.PageType == uppReportPageTypes.MDEDMPage1 && pCell.Row == 20 && pCell.Col == 3) exRange.Cell(1, 1).Value = appApplication.User.NiceName;


                            //special case
                            if (Report.ReportType == uppReportTypes.MDSpoutReport && aPage.PageType == uppReportPageTypes.ProjectSummaryPage)
                            {
                                SetCellValueXML("C61:L61", $"WinTray Version {appApplication.AppVersion}", out exRange, bLocked: true, bMerge:true);
                                exRange.Style.Font.FontName = "Arial";
                            }

                        }
                    }

                }
                catch (Exception e)
                {
                    SaveWarning(System.Reflection.MethodBase.GetCurrentMethod().Name, $"Worksheet Image Generation Error { aDwg.DrawingName} -Trays { aPage.SpanName}", "", uppWarningTypes.ReportFatal, e);
                    continue;
                }


                if (CancelIt) return;

            }


            ////drawings
            dxfImage image = null;
            for (int j = 1; j <= aPage.Drawings.Count; j++)
            {

                try
                {
                    //tell the form to create the sketch and put it on the clipboard

                    aDwg = aPage.Drawings[j -1];

                    if (aDwg.Project == null) aDwg.ProjectHandle = Project.Handle;

                    Status = $"Generating Drawing '{aDwg.DrawingName}'";

                    image = ReportGeneratorVM.CreateDrawing(aDwg, BackColor: Color.White);

                    //past the clipboard bitmap into the sheet if it was succesfully created
                    if (image != null)
                    {
                        // very important!!! black image results without this line
                        image.Display.ZoomExtents();
                        if (string.IsNullOrWhiteSpace(aDwg.PasteAddress)) aDwg.PasteAddress = "D13";
                        exRange = wSheet.Range(aDwg.PasteAddress);
                        //string currentPath = new Uri(Path.GetDirectoryName(Template)).LocalPath;
                        //string filePath = Path.Combine(currentPath, aDwg.DrawingName + ".jpg");
                        dxfBitmap dmap = image.Bitmap(false);
                        //dmap.FloodFill(Color.Red);
                        //dmap.CopyToClipBoard();
                        Bitmap bmap = (Bitmap)dmap.Image;
                        //bmap.Save(filePath, ImageFormat.Jpeg);
                        //System.Windows.Forms.Clipboard.GetImage().Save(filePath, ImageFormat.Jpeg);
                        //wSheet.AddPicture(filePath).MoveTo(exRange.Cell(1, 1));

                        MemoryStream bmapstream = new();
                        bmap.Save(bmapstream, ImageFormat.Tiff);
                        wSheet.AddPicture(bmapstream).MoveTo(exRange.Cell(1, 1));
                    }
                    else
                    {
                        exRange = wSheet.Range(aDwg.PasteAddress);
                        exRange.Cell(1, 1).Value = reply;
                    }

                    if (aDwg.Tag != string.Empty && aDwg.TagAddress != string.Empty)
                    {
                        AddTagCellXML(aPage, aDwg.Tag, aDwg.TagAddress);
                    }
                }
                catch (Exception e)
                {
                    SaveWarning(System.Reflection.MethodBase.GetCurrentMethod().Name, $"Worksheet Image Generation Error { aDwg.DrawingName} -Trays { aPage.SpanName}", "", uppWarningTypes.ReportFatal, e);

                }
                finally
                {

                    if (image != null) image.Dispose();
                    image = null;
                }




            }

            //graphs
            //for (int j = 0; j < aPage.Graphs.Count; j++)
            //{
            //    //tell the form to create the sketch and put it on the clipboard
            //    aGraph = aPage.Graphs[j];

            //    if (aPage.SpanName != "")
            //    {
            //        Status = "Generating Drawing - Tray " + aPage.SpanName + " - " + aGraph.Title;
            //    }
            //    else
            //    {
            //        Status = "Generating Drawing - " + aGraph.Title;
            //    }
            //    if (aGraph.SeperateSheet)
            //    {
            //        //TODO
            //        //aGraph.Chart = WorkBook.Charts.Add(_, wSheet);
            //    }

            //    CreateChart(aPage, aGraph);
            //    
            //}

            aPage.TabName = wSheet.Name;
            //oWorkingExcel.ScreenUpdating = true;

            //=================================================================
            return;
        }

        private ExcelIO.Range GetCellRange(uopTableCell aCell, ExcelIO.Worksheet wSheet, uppUnitFamilies aUnits, out string rError, ExcelIO.Workbook aWorkBook = null)
        {
            //create the excel range for the cell
            ExcelIO.Range exRange = null;
            aWorkBook ??= WorkBook;
            wSheet ??= ActiveSheet;
            rError = "";
            if (aCell == null || wSheet == null || aWorkBook == null) return null;
            ExcelIO.Style style;

            try
            {
                if (aCell.HasName)
                {
                    exRange = GetRange(aCell.Name, wSheet);
                    if(aCell.Name == "MANFAST_HORIZONTAL")
                    {
                        Console.WriteLine(aCell.Signature());
                    }
                }
                else
                {
                    exRange = GetRange(aCell.CellAddress, wSheet);
                }
               // exRange = aCell.HasName
               //? GetRange(aCell.Name, wSheet)
               //: GetRange(aCell.CellAddress, wSheet);

                //define the range properties based on the cell properties
                if (!aCell.DataOnly)
                {
              

                   

                    //create the excel range for the cell
                    //exRange = GetRange(aCell.Address, wSheet);
                    exRange.Merge();


                    style = exRange.Style;


                    if (!string.IsNullOrWhiteSpace(aCell.FontName)) exRange.Font.Name = aCell.FontName;

                    if (aCell.FontSize > 0) exRange.Font.Size = aCell.FontSize;
                    exRange.Font.Bold = aCell.Bold;
                    if (aCell.FontColor != Color.Transparent)
                        exRange.Font.Color = ColorTranslator.ToOle(aCell.FontColor);

                    if (aCell.BackColor != Color.Transparent)
                        exRange.Interior.Color = ColorTranslator.ToOle(aCell.BackColor);



                    exRange.VerticalAlignment = (ExcelIO.XlVAlign)aCell.VerticalAlignmentIO;
                    exRange.HorizontalAlignment = (ExcelIO.XlHAlign)aCell.HorizontalAlignmentIO;

                    //if (aCell.Bold) exRange.Font.Bold = aCell.Bold;

                    exRange.WrapText = aCell.WrapText;
                    exRange.Orientation = (ExcelIO.XlOrientation)aCell.Orientation;

                    if (aCell.HasUnits && string.IsNullOrWhiteSpace(aCell.NumberFormat))
                    {
                        if (NumberFormats(aUnits).TryGetValue(aCell.UnitType, out string fmat)) exRange.NumberFormat = fmat;
                    }

                    //borders on individual cells
                    try
                    {
                        SetCellBorders(exRange, aCell);

                    }
                    catch (Exception e) { SaveWarning(System.Reflection.MethodBase.GetCurrentMethod().Name, $"Set Cell Borders Error", "", uppWarningTypes.General, e); }
                }



            }
            catch (Exception e)
            {
                rError = e.Message;
                SaveWarning(System.Reflection.MethodBase.GetCurrentMethod().Name, $"Get Cell Range Error", "", uppWarningTypes.General, e);

            }

            if (exRange == null)
            {
                rError = $"{aCell.Address} Was Not Found In Worksheet {wSheet.Name }";
            }

            return exRange;

        }

        private IXLRange GetCellRangeXML(uopTableCell aCell, IXLWorksheet wSheet, uppUnitFamilies aUnits, out string rError, XLWorkbook aWorkBook)
        {
            //create the excel range for the cell
            IXLRange exRange = null;
            rError = "";
            if (aCell == null || wSheet == null) return null;
            try
            {

                //define the range properties based on the cell properties
                if (aCell.DataOnly)
                {
                    if (!aCell.IsAdressed) return wSheet.Range(aCell.Address);
                    IXLNamedRange namerange = wSheet.NamedRange(aCell.Address);
                    namerange ??= aWorkBook.NamedRange(aCell.Address);
                    if (namerange != null)
                    {
                        IXLRanges ranges = namerange.Ranges;
                        foreach (var item in ranges)
                        {
                            exRange = item;
                            break;
                        }
                    }
                    exRange ??= wSheet.Range(aCell.Address);
                }
                else
                {

                    //create the excel range for the cell
                    exRange = wSheet.Range(aCell.Address);

                    IXLStyle cellstyle;
                    //borders on individual cells
                    SetCellBordersXML(exRange.Cells(), aCell);
                    exRange.Merge();

                    //borders
                    SetCellBordersXML(exRange, aCell);


                    cellstyle = exRange.Style;
                    cellstyle.Protection.Locked = false;
                    if (!string.IsNullOrWhiteSpace(aCell.FontName)) cellstyle.Font.FontName = aCell.FontName;

                    if (aCell.FontSize > 0) cellstyle.Font.FontSize = aCell.FontSize;
                    //cellstyle.Font.SetBold(aCell.Bold);
                    if (aCell.FontColor != Color.Transparent)
                        cellstyle.Font.FontColor = XLColor.FromColor(aCell.FontColor);

                    if (aCell.BackColor != Color.Transparent)
                        cellstyle.Fill.BackgroundColor = XLColor.FromColor(aCell.BackColor);

                    cellstyle.Font.Bold = aCell.Bold;

                    cellstyle.Alignment.Vertical = (XLAlignmentVerticalValues)aCell.VerticalAlignment;
                    cellstyle.Alignment.Horizontal = (XLAlignmentHorizontalValues)aCell.HorizontalAlignment;

                    if (aCell.Bold) cellstyle.Font.Bold = aCell.Bold;

                    cellstyle.Alignment.WrapText = aCell.WrapText;
                    cellstyle.Alignment.TextRotation = (int)aCell.Orientation;

                    if (aCell.HasUnits && string.IsNullOrWhiteSpace(aCell.NumberFormat))
                    {
                        if (NumberFormats(aUnits).TryGetValue(aCell.UnitType, out string fmat)) aCell.NumberFormat = fmat;
                    }


                }



            }
            catch (Exception e)
            {
                rError = e.Message;
                SaveWarning(System.Reflection.MethodBase.GetCurrentMethod().Name, $"Get Cell Range Error", "", uppWarningTypes.General, e);
            }

            if (exRange == null)
            {
                rError = $"{aCell.Address} Was Not Found In Worksheet {wSheet.Name }";
            }

            return exRange;

        }

        /// <summary>
        /// Set value to cell
        /// </summary>
        /// <param name="exRange"></param>
        /// <param name="pCell"></param>
        private void SetCellValue(ExcelIO.Range exRange, uopTableCell pCell)
        {
            if (!pCell.IsDefined || exRange == null) return;
            try
            {
                //ExcelIO.Style cellstyle = exRange.Style;

                //exRange.Locked = pCell.Locked;

                if (pCell.IsNullValue)
                {
                    exRange.Value = XLCellValue.FromObject("-");
                }
                else
                {

                    if (pCell.HasUnits)
                    {


                        exRange.Value = pCell.DisplayUnitValue;
                        if (!string.IsNullOrEmpty(pCell.NumberFormat))
                            exRange.NumberFormat = pCell.NumberFormat;
                    }
                    else
                    {

                        if (!pCell.Numeric)
                        {
                            exRange.Value = pCell.Property.ValueS;
                        }
                        else
                        {
                            exRange.Value = pCell.Property.ValueD;
                            if (!string.IsNullOrEmpty(pCell.NumberFormat)) exRange.NumberFormat = pCell.NumberFormat;
                        }


                    }
                }
            }
            catch (Exception e)
            {
                if (Report != null && ActivePage != null)
                {
                    string err = $"Set Cell Value Error Occured On Page '{ActivePage.TitleText}'";
                    if (pCell != null) err = $"{err} Cell '{pCell.Address}' ";

                    SaveWarning(System.Reflection.MethodBase.GetCurrentMethod().Name, err, "", uppWarningTypes.General, e);
                }
            }




        }

        /// <summary>
        /// Set value to cell
        /// </summary>
        /// <param name="aRange"></param>
        /// <param name="aProperty"></param>
        private bool SetCellValueXML(IXLRange aRange, object aValue, bool? bLocked = null, bool bMerge = false, int aRow = 1, int aCol  = 1)
        {
            aValue ??= "";
            if (aRange == null) return false;
            
          
            bool _rVal = false;
            try
            {
                if (bMerge && !aRange.IsMerged()) 
                    aRange =  aRange.Merge();
                if (bLocked.HasValue) aRange.Style.Protection.Locked = bLocked.Value;
                if (aRow < 1) aRow = 1;
                if (aCol < 1) aCol = 1;
                if (aRow > aRange.RowCount() || aCol > aRange.ColumnCount())
                    return false;
                return SetCellValueXML( aRange.Cell(aRow, aCol), aValue);


       
            }
            catch (Exception e)
            {
                SaveWarning("SetCellValueXML", "Error Setting Range Value", $"Cell:[{aRange.RangeAddress}]", uppWarningTypes.General, e);
            }
            return _rVal;
        }


        /// <summary>
        /// Set value to cell
        /// </summary>
        /// <param name="aRange"></param>
        /// <param name="aProperty"></param>
        private bool SetCellValueXML(IXLCell aCell, object aValue)
        {
            aValue ??= "";
            if (aCell == null) return false;


            bool _rVal = false;
            try
            {
               

                XLCellValue curval = aCell.Value;


                XLCellValue newval = XLCellValue.FromObject(aValue);
                _rVal = !newval.Equals(curval);
                aCell.Value = newval;
      
            }
            catch (Exception e)
            {
                SaveWarning("SetCellValueXML", "Error Setting Cell Value", $"Cell:[{aCell.Address}]", uppWarningTypes.General, e);
            }
            return _rVal;
        }

        /// <summary>
        /// Set value to cell
        /// </summary>
        /// <param name="aAddress"></param>
        /// <param name="aProperty"></param>
        private bool SetCellValueXML(string aAddress, uopProperty aProperty, IXLWorksheet aSheet = null, bool bNamedRange = false, int aRow = 1, int aCol = 1)
        {
            return SetCellValueXML(aAddress, aProperty, out IXLRange _, aSheet, bNamedRange, aRow,aCol);
        }

        /// <summary>
        /// Set value to cell
        /// </summary>
        /// <param name="exRange"></param>
        /// <param name="aProperty"></param>
        /// <param name="rRange"></param>
        /// <param name="aSheet"></param>
        /// <param name="bNamedRange"></param>
        private bool SetCellValueXML(string aAddress, uopProperty aProperty, out IXLRange rRange, IXLWorksheet aSheet = null, bool bNamedRange = false, int aRow = 1, int aCol = 1)
        {

            rRange = null;
            if (string.IsNullOrWhiteSpace(aAddress) || aProperty == null) return false;
            aSheet ??= ActiveSheetXML;
            rRange = GetRangeXML(aAddress, aSheet, bNamedRange);
            if (rRange == null) return false;
            bool _rVal = false;

            if (aProperty.IsNullValue)
            {
                _rVal = SetCellValueXML(rRange, "-", aRow: aRow, aCol: aCol);
            }
            else
            {

                if (aProperty.HasUnits)
                {

                    _rVal = SetCellValueXML(rRange, aProperty.DisplayUnitValue, aRow: aRow, aCol: aCol);

                }
                else
                {

                    _rVal = (!aProperty.Numeric) 
                        ? SetCellValueXML(rRange, aProperty.ValueS, aRow: aRow, aCol: aCol) 
                        : SetCellValueXML(rRange, aProperty.ValueD, aRow: aRow, aCol: aCol); ;
                }
            }

            return _rVal;
        }

        /// <summary>
        /// Set value to cell
        /// </summary>
        /// <param name="exRange"></param>
        /// <param name="aValue"></param>
        /// <param name="rRange"></param>
        /// <param name="aSheet"></param>
        /// <param name="bNamedRange"></param>
        private bool SetCellValueXML(string aAddress, object aValue, out IXLRange rRange, IXLWorksheet aSheet = null, bool? bLocked = null, bool bMerge = false, bool bNamedRange = false)
        {

            rRange = null;
            aValue ??= "";
            if (string.IsNullOrWhiteSpace(aAddress)) return false;
            aSheet ??= ActiveSheetXML;
            rRange = GetRangeXML(aAddress, aSheet, bNamedRange);
            if (rRange == null) return false;
            return SetCellValueXML(rRange, aValue,bLocked, bMerge);
        }

        /// <summary>
        /// Set value to cell
        /// </summary>
        /// <param name="exRange"></param>
        /// <param name="pCell"></param>
        private bool SetCellValueXML(IXLRange exRange, uopTableCell pCell, bool bNonBlankCellsOnly = false)
        {

          

            if (pCell == null) return false;
            if(bNonBlankCellsOnly)
            {
                if (!pCell.IsDefined) return false;
            }
           
            bool _rVal = false;
            uopProperty cellprop = pCell.Property;

            if (cellprop.IsNullValue)
            {
                _rVal = SetCellValueXML(exRange, "-", pCell.Locked);
            }
            else
            {

                if (cellprop.HasUnits)
                {

                    _rVal = SetCellValueXML(exRange, cellprop.DisplayUnitValue, pCell.Locked);

                }
                else
                {

                    _rVal = (!cellprop.Numeric) 
                        ? SetCellValueXML(exRange, cellprop.ValueS, pCell.Locked) 
                        : SetCellValueXML(exRange, cellprop.ValueD, pCell.Locked);


                }
            }

            return _rVal;

        }

        /// <summary>
        /// Set border to cell
        /// </summary>
        /// <param name="exRange"></param>
        /// <param name="pCell"></param>
        private void SetCellBordersXML(IXLCells exCells, uopTableCell pCell)
        {
            XLBorderStyleValues style;

            for (int j = 0; j < 4; j++)
            {
                pCell.GetBorderData(j, out mzBorderStyles aStyle, out mzBorderWeights aWeight);

                if (aStyle != mzBorderStyles.Undefined)
                {

                    style = GetIXLBorderStyle(aStyle, aWeight);
                    if (j == 0)
                    {
                        foreach (var item in exCells)
                        {
                            item.Style.Border.TopBorder = style;
                        }


                    }
                    else if (j == 1)
                    {
                        foreach (var item in exCells)
                        {
                            item.Style.Border.LeftBorder = style;
                        }

                    }
                    else if (j == 2)
                    {
                        foreach (var item in exCells)
                        {
                            item.Style.Border.BottomBorder = style;
                        }

                    }
                    else
                    {
                        foreach (var item in exCells)
                        {
                            item.Style.Border.RightBorder = style;
                        }

                    }
                }
            }
        }

        /// <summary>
        /// Set border to cell
        /// </summary>
        /// <param name="exRange"></param>
        /// <param name="pCell"></param>
        private void SetCellBordersXML(IXLRange exRange, uopTableCell pCell)
        {

            for (int j = 0; j < 4; j++)
            {
                pCell.GetBorderData(j, out mzBorderStyles aStyle, out mzBorderWeights aWeight);

                if (aStyle != mzBorderStyles.Undefined && aStyle != mzBorderStyles.None)
                {
                    if (j == 0)
                    {
                        exRange.Style.Border.TopBorder = GetIXLBorderStyle(aStyle, aWeight);
                    }
                    else if (j == 1)
                    {
                        exRange.Style.Border.LeftBorder = GetIXLBorderStyle(aStyle, aWeight);
                    }
                    else if (j == 2)
                    {
                        exRange.Style.Border.BottomBorder = GetIXLBorderStyle(aStyle, aWeight);
                    }
                    else
                    {
                        exRange.Style.Border.RightBorder = GetIXLBorderStyle(aStyle, aWeight);
                    }
                }
            }
        }


        /// <summary>
        /// Set border to cell
        /// </summary>
        /// <param name="exRange"></param>
        /// <param name="pCell"></param>
        private void SetCellBorders(ExcelIO.Range aRange, uopTableCell pCell)
        {
            ExcelIO.Style style = aRange.Style;

            for (int j = 0; j < 4; j++)
            {
                pCell.GetBorderData(j, out mzBorderStyles aStyle, out mzBorderWeights aWeight);

                if (aStyle != mzBorderStyles.Undefined && aStyle != mzBorderStyles.None)
                {
                    if (j == 0)
                    {
                        //style.Borders[ExcelIO.XlBordersIndex.xlEdgeTop].LineStyle = GetBorderStyle(aStyle, aWeight);
                        aRange.Borders[ExcelIO.XlBordersIndex.xlEdgeTop].LineStyle = GetBorderStyle(aStyle, aWeight);
                    }
                    else if (j == 1)
                    {
                        // style.Borders[ExcelIO.XlBordersIndex.xlEdgeLeft].LineStyle = GetBorderStyle(aStyle, aWeight);
                        aRange.Borders[ExcelIO.XlBordersIndex.xlEdgeLeft].LineStyle = GetBorderStyle(aStyle, aWeight);


                    }
                    else if (j == 2)
                    {
                        //style.Borders[ExcelIO.XlBordersIndex.xlEdgeBottom].LineStyle = GetBorderStyle(aStyle, aWeight);
                        aRange.Borders[ExcelIO.XlBordersIndex.xlEdgeBottom].LineStyle = GetBorderStyle(aStyle, aWeight);

                    }
                    else
                    {
                        // style.Borders[ExcelIO.XlBordersIndex.xlEdgeRight].LineStyle = GetBorderStyle(aStyle, aWeight);
                        aRange.Borders[ExcelIO.XlBordersIndex.xlEdgeRight].LineStyle = GetBorderStyle(aStyle, aWeight);
                    }
                }
            }
        }

        private void AddTagCell(uopDocReportPage aPage, string aTag, string aAddress, ExcelIO.Worksheet aWorksheet = null)
        {
            aPage ??= ActivePage;

            aWorksheet ??= ActiveSheet;

            if (string.IsNullOrEmpty(aTag) || string.IsNullOrEmpty(aAddress) || aPage == null || aWorksheet == null)
            {
                return;
            }
            aTag = aTag.Trim();
            ExcelIO.Range aRange = GetRange(aAddress.Trim(), aWorksheet);
            if (aRange == null) return;

            try
            {
                string cTag = Convert.ToString(aRange.Value);
                cTag ??= "";
                cTag = cTag.Trim();
                cTag = !string.IsNullOrWhiteSpace(cTag) ? $"{cTag} {Environment.NewLine}{ aTag}" : cTag = aTag;
                aRange.Value = cTag;
                aRange.Font.Color = ColorTranslator.ToOle(Color.White);
                //string pAddress = aPage.TagAddress;
                //mzUtils.ListAdd(ref pAddress, aAddress);
                //aPage.TagAddress = pAddress;
            }
            catch (Exception e) { SaveWarning(System.Reflection.MethodBase.GetCurrentMethod().Name, $"Add Tag Cell Error", "", uppWarningTypes.General, e); }

        }


        /// <summary>
        /// Add Tag to cell for the given address
        /// </summary>
        /// <param name="aPage"></param>
        /// <param name="aTag"></param>
        /// <param name="aAddress"></param>
        private void AddTagCellXML(uopDocReportPage aPage, string aTag, string aAddress)
        {
            aTag = aTag.Trim();
            aAddress = aAddress.Trim();
            if (string.IsNullOrEmpty(aTag) || string.IsNullOrEmpty(aAddress) || aPage == null) return;

            try
            {

                IXLWorksheet wSheet = WorksheetsXML.Find(x => string.Compare(x.Name, aPage.TabName, true) == 0);
                if (wSheet == null) return;
                IXLRange aRange = wSheet.Range(aAddress);
                if (aRange == null) return;

                string cTag = Convert.ToString(aRange.Cell(1, 1).Value);
                cTag ??= string.Empty;
                cTag = !string.IsNullOrWhiteSpace(cTag) ? $"{cTag.Trim()}{Environment.NewLine} {aTag}" : aTag;

                aRange.Cell(1, 1).Value = cTag.Trim();
                aRange.Style.Font.FontColor = XLColor.White;
                string pAddress = aPage.TagAddress;
                mzUtils.ListAdd(ref pAddress, aAddress);
                aPage.TagAddress = pAddress;
            } catch (Exception e) { SaveWarning(System.Reflection.MethodBase.GetCurrentMethod().Name, $"Add Tag Cell Error", "", uppWarningTypes.General, e); }

        }

        /// <summary>
        /// Get IXL Border Style
        /// </summary>
        /// <param name="borderStyle"></param>
        /// <returns></returns>
        private XLBorderStyleValues GetIXLBorderStyle(mzBorderStyles borderStyle, mzBorderWeights aWeight)
        {
            switch (borderStyle)
            {
                case mzBorderStyles.Undefined:
                case mzBorderStyles.None:
                    return XLBorderStyleValues.None;

                case mzBorderStyles.Dash:
                    return XLBorderStyleValues.Dashed;

                case mzBorderStyles.Double:
                    return XLBorderStyleValues.Double;

                case mzBorderStyles.Continous:
                    if (aWeight == mzBorderWeights.Thin)
                    {
                        return XLBorderStyleValues.Thin;
                    }
                    return XLBorderStyleValues.Medium;

                case mzBorderStyles.DashDot:
                    return XLBorderStyleValues.DashDot;

                case mzBorderStyles.DashDotDot:
                    return XLBorderStyleValues.DashDotDot;

                case mzBorderStyles.Dot:
                    return XLBorderStyleValues.Dotted;

                case mzBorderStyles.SlantDashDot:
                    return XLBorderStyleValues.SlantDashDot;

                default:
                    return XLBorderStyleValues.None;
            }
        }

        private ExcelIO.XlLineStyle GetBorderStyle(mzBorderStyles borderStyle, mzBorderWeights aWeight)
        {
            switch (borderStyle)
            {
                case mzBorderStyles.Undefined:
                case mzBorderStyles.None:
                    return ExcelIO.XlLineStyle.xlLineStyleNone;

                case mzBorderStyles.Dash:
                    return ExcelIO.XlLineStyle.xlDash;

                case mzBorderStyles.Double:
                    return ExcelIO.XlLineStyle.xlDouble;

                case mzBorderStyles.Continous:
                    if (aWeight == mzBorderWeights.Thin)
                    {
                        return ExcelIO.XlLineStyle.xlContinuous;
                    }
                    return ExcelIO.XlLineStyle.xlContinuous;

                case mzBorderStyles.DashDot:
                    return ExcelIO.XlLineStyle.xlDashDot;

                case mzBorderStyles.DashDotDot:
                    return ExcelIO.XlLineStyle.xlDashDotDot;

                case mzBorderStyles.Dot:
                    return ExcelIO.XlLineStyle.xlDot;

                case mzBorderStyles.SlantDashDot:
                    return ExcelIO.XlLineStyle.xlSlantDashDot;

                default:
                    return ExcelIO.XlLineStyle.xlLineStyleNone;
            }
        }

        /// <summary>
        /// deletes the current  back up file
        /// </summary>
        private void BackupDelete()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(BackupPath)) return;
                if (!File.Exists(BackupPath)) return;
                File.Delete(BackupPath);

            }
            catch (Exception e) { SaveWarning(System.Reflection.MethodBase.GetCurrentMethod().Name, $"Unable to delete backup file.", $"Unable to delete '{BackupPath}'", uppWarningTypes.General, e); }
            finally { BackupPath = ""; }


        }



        /// <summary>
        /// restores the backup to the current report filename
        /// </summary>
        private void BackupRestore()
        {


            string fspec = BackupPath;
            if (string.IsNullOrWhiteSpace(fspec) || string.IsNullOrWhiteSpace(FileSpec)) return;
            if (!File.Exists(fspec)) { BackupPath = ""; return; }
            try
            {
                File.Copy(fspec, FileSpec, true);
                File.Delete(fspec);

                BackupPath = "";
            }
            catch (Exception e)
            {
                SaveWarning(System.Reflection.MethodBase.GetCurrentMethod().Name, $"Unable to restore backup file.", $"Unable to restore '{BackupPath}'", uppWarningTypes.General, e);
            }
            finally
            {
                BackupPath = "";
            }



        }




        /// <summary>
        /// Recover MDSpoutData
        /// </summary>
        /// <param name="aBook"></param>
        /// <returns></returns>
        private bool RecoverMDSpoutData(ExcelIO.Workbook aBook)
        {
            if (Report == null || aBook == null || WorkBook == null) return false;

            bool _rVal = false;

            try
            {
                //retrieve the column sketch
                string aSheetName = "Column Sketch";

                //eventProcessStart?.Invoke("Recovering Data From Previous Report");
                ExcelIO.Worksheet curSheet;
                GetSheets(aBook, aSheetName, out List<ExcelIO.Worksheet> curSheets, out List<ExcelIO.Worksheet> lastRevSheets, out _, out _);
                ExcelIO.Worksheet lastRevSheet;
                ExcelIO.Range aRange;
                ExcelIO.Range bRange;

                for (int i = 0; i < curSheets.Count; i++)
                {
                    curSheet = curSheets[i];
                    Status = "Recovering Column Sketches";
                    if (lastRevSheets.Count >= i + 1)
                    {
                        lastRevSheet = lastRevSheets[i];
                        aRange = GetRange("D12:AM60", curSheet);
                        bRange = GetRange("D12:AM60", lastRevSheet);

                        bRange.Copy(aRange);


                        _rVal = true;

                    }
                }
            }
            catch (Exception e)
            {
                SaveWarning(System.Reflection.MethodBase.GetCurrentMethod().Name, $"Error encountered try to restore date", $"Unable to restore data from '{aBook.Name}'", uppWarningTypes.General, e);
            }


            //eventProcessEnd?.Invoke("Complete Recovering Data From Previous Report");
            return _rVal;
        }



        /// <summary>
        /// Recover MDSpoutData
        /// </summary>
        /// <param name="aBook"></param>
        /// <returns></returns>
        private bool RecoverMDSpoutDataXML(XLWorkbook aBook)
        {
            if (Report == null || aBook == null || WorkBookXML == null) return false;

            bool _rVal = false;


            //retrieve the column sketch
            string aSheetName = "Column Sketch";

            //eventProcessStart?.Invoke("Recovering Data From Previous Report");
            IXLWorksheet curSheet;
            GetSheetsXML(aBook, aSheetName, out List<IXLWorksheet> curSheets, out List<IXLWorksheet> lastRevSheets, out _, out _);
            IXLWorksheet lastRevSheet;
            IXLRange aRange;
            IXLRange bRange;

            for (int i = 0; i < curSheets.Count; i++)
            {
                curSheet = curSheets[i];
                Status = "Recovering Column Sketches";
                if (lastRevSheets.Count >= i)
                {
                    lastRevSheet = lastRevSheets[i];

                    aRange = curSheet.Range("D12:AM60");
                    bRange = lastRevSheet.Range("D12:AM60");
                    aRange.Unmerge();
                    bRange.CopyTo(aRange);
                    foreach (IXLPicture pic in lastRevSheet.Pictures)
                    {
                        if (pic.TopLeftCell != null && aRange.Contains(pic.TopLeftCell.ToString()))
                        {
                            try
                            {
                                pic.CopyTo(curSheet);
                            }
                            catch (Exception e)
                            {
                                SaveWarning(System.Reflection.MethodBase.GetCurrentMethod().Name, $"Error encountered try to restore date", $"Unable to restore data from '{aBook}'", uppWarningTypes.General, e);
                            }
                        }
                    }
                    _rVal = true;
                }
            }

            //eventProcessEnd?.Invoke("Complete Recovering Data From Previous Report");
            return _rVal;
        }

        /// <summary>
        /// Get sheets from workbook
        /// </summary>
        /// <param name="lastRevision"></param>
        /// <param name="sheetName"></param>
        /// <param name="curSheets"></param>
        /// <param name="lastRevSheets"></param>
        /// <param name="curSheet"></param>
        /// <param name="lastRevSheet"></param>
        private void GetSheetsXML(XLWorkbook lastRevision, string sheetName, out List<IXLWorksheet> curSheets, out List<IXLWorksheet> lastRevSheets, out IXLWorksheet curSheet, out IXLWorksheet lastRevSheet)
        {
            curSheet = WorkBookXML.Worksheet(sheetName);
            lastRevSheet = lastRevision.Worksheet(sheetName);

            curSheets = new List<IXLWorksheet>();
            lastRevSheets = new List<IXLWorksheet>();

            foreach (IXLWorksheet iterSht in WorkBookXML.Worksheets)
            {
                if (iterSht.Name.Length >= sheetName.Length && iterSht.Name.Substring(0, sheetName.Length) == sheetName)
                {
                    curSheets.Add(iterSht);
                }
            }

            foreach (IXLWorksheet iterSht in lastRevision.Worksheets)
            {
                if (iterSht.Name.Length >= sheetName.Length && iterSht.Name.Substring(0, sheetName.Length) == sheetName)
                {
                    lastRevSheets.Add(iterSht);
                }
            }
        }

        private void GetSheets(ExcelIO.Workbook lastRevision, string sheetName, out List<ExcelIO.Worksheet> curSheets, out List<ExcelIO.Worksheet> lastRevSheets, out ExcelIO.Worksheet curSheet, out ExcelIO.Worksheet lastRevSheet)
        {
            curSheet = GetWorkSheet(sheetName);
            lastRevSheet = GetWorkSheet(sheetName, lastRevision);

            curSheets = new List<ExcelIO.Worksheet>();
            lastRevSheets = new List<ExcelIO.Worksheet>();

            foreach (ExcelIO.Worksheet iterSht in WorkBook.Worksheets)
            {
                if (iterSht.Name.Length >= sheetName.Length && iterSht.Name.Substring(0, sheetName.Length) == sheetName)
                {
                    curSheets.Add(iterSht);
                }
            }

            foreach (ExcelIO.Worksheet iterSht in lastRevision.Worksheets)
            {
                if (iterSht.Name.Length >= sheetName.Length && iterSht.Name.Substring(0, sheetName.Length) == sheetName)
                {
                    lastRevSheets.Add(iterSht);
                }
            }
        }

  
        /// <summary>
        /// attempts to mark the changes between the current revisions of the report and the
        /// one one revision prior if it was found in the same output folder
        /// </summary>
        private void MarkRevisionsMD(XLWorkbook aWorkbook, XLWorkbook bWorkBook)
        {
            if (aWorkbook == null || bWorkBook == null) return;


           
            string aRowList = "5,6,7,8,10,11,13,15,17,18,19,20,21,22,32,33,36";
           
            Status = "Marking Revisions";
            PageResults results = CreateResultsArray( aWorkbook,  bWorkBook);

            try
            {
                //compare the workbooks in a sheet wise way
                foreach (PageResult result in results)
                {
                    try
                    {

                       
                        if (result.SheetCount < 2 || result.Page == null)  // both sheets were not found
                            continue;

                        if (result.Page.Protected && result.Sheet1.IsProtected)
                        {
                            result.Sheet1.Unprotect(result.Page.Password);
                        }

                        Status = $"Marking Revions - {result.SheetName }";


                        //check tagged cells
                        if (result.Page.TagAddress != string.Empty)
                        {
                            List<string> tagIDs = mzUtils.StringsFromDelimitedList(result.Page.TagAddress, ",", false);
                            foreach (string addr in tagIDs)
                            {
                                xCompareRange(result.Sheet1, result.Sheet2, addr, aRowIDs: result.RowIDs, bCompareValues: true);
                            }


                        }

                        for (int r = 9; r <= 61; r++)
                        {

                            if (result.CompareRowProps(r, 3, 37, bCompareAll: true, aPrecis: 4))
                            {
                                foreach (uopProperty prop in result.Differences)
                                {
                                    IXLCell aCell = GetCellXML(result.Sheet1, prop.Row, prop.Col);
                                    if (aCell != null) aCell.Style.Font.Bold = true;
                                }

                            }

                            if (CancelIt)
                            {
                                Status = "Marking Revisions Complete";
                                return;
                            }

                            if (result.MaxRevCheckRow > 0)
                            {
                                if (r + 1 > result.MaxRevCheckRow)
                                    break;
                            }
                        }

                        if (result.RowIDs.Count > 0)
                        {
                            MarkRevsMD(result);
                        }

                        Status = $"Revisions - {result.SheetName} Complete";

                        if (result.PageType == uppReportPageTypes.TraySketchPage && result.RowIDs.Count <= 0)
                        {
                            //save the tray sketch id's for processing at the end
                            results.SketchIDs.Add(result.Index);
                        }

                    }
                    catch (Exception e)
                    {
                        SaveWarning("MarkRevisionsMD", "Rev Marking Error", $"Sheet [{result.SheetName}]", uppWarningTypes.General, e);
                    }
                    finally
                    {
                        Status = "";

                        if (result.Page != null && result.Sheet1 != null)
                        {
                            if (result.Page.Protected && !result.Sheet1.IsProtected)
                            {

                                IXLSheetProtection xLSheetProtection = result.Page.DontSaveWithPassword ? result.Sheet1.Protect() : result.Sheet1.Protect(result.Page.Password);
                                xLSheetProtection.AllowElement(XLSheetProtectionElements.EditObjects, true);
                            }
                        }

                    }

                    if (CancelIt) return;
                }

                PageResult aResults = null;
                foreach (int skid in results.SketchIDs)
                {
                    bool bMarkIt = false;
                    try
                    {

                        aResults = results.Item(skid);

                        Status = $"Marking Sketch Revisions - {aResults.SheetName}";

                        //if the sheet has not already been flagged as revised
                        PageResult bResults = results.GetResultByType(uppReportPageTypes.MDSpoutDetailPage, aResults.SubTitle1);
                        bMarkIt = bResults.HasChanged;


                        if (!bMarkIt)
                        {
                            bResults = results.GetResultByType(uppReportPageTypes.MDOptimizationPage, aResults.SubTitle1);
                            if (bResults.HasChanged) bMarkIt = true;
                        }

                        //check the tray design summary sheet
                        if (aResults.SpanName != string.Empty && !bMarkIt)
                        {
                            bResults = results.GetResultByType(uppReportPageTypes.TraySummaryPage);
                            if (bResults.Index > 0)
                            {

                              
                                //get the tray summary props from the table on the summary page of the current workbook
                                uopProperties aProps = GetColumnProperties(bResults.Sheet1, 14, 45, 0, aRowList, aNameColumnIndex: 4, aMatchValue: aResults.SpanName);
                                if (aProps.Count > 0)
                                {
                                    //get the tray summary props from the table on the summary page of the last workbook
                                    IXLWorksheet bSheet = xGetSheetByNameXML(bWorkBook, bResults.Sheet1.Name, out _);
                                    uopProperties bProps = GetColumnProperties(bSheet, 14, 45, 0, aRowList, 4, 4, aResults.SpanName);
                                    if (bProps.Count > 0)
                                    {
                                        //get the properties that differ
                                        uopProperties dProps = aProps.GetDifferences(bProps, 4);
                                        if (dProps.Count > 0)
                                        {

                                            for (int k = 1; k < dProps.Count; k++)
                                            {
                                                uopProperty aProp = dProps.Item(k);
                                                IXLCell aCell = bResults.Sheet1.Cell(aProp.Row, aProp.Col);
                                                if (aCell != null) aCell.Style.Font.Bold = true;

                                            }
                                            bMarkIt = true;
                                        }
                                    }
                                }
                            }
                        }


                    }
                    catch (Exception e)
                    {
                        SaveWarning("MarkRevisionsMD", "Rev Marking Error", $"Sheet [{aResults.SheetName}]", uppWarningTypes.General, e);
                    }
                    finally
                    {
                        Status = "";

                    }


                    if (CancelIt) return;
                    if (bMarkIt)
                    {
                        MarkRevsMD(aResults);
                    }
                }

            }
            catch(Exception e)
            {
                SaveWarning("MarkRevisionsMD", "Rev Marking Error","", uppWarningTypes.General, e);
            }
            finally
            {
                results?.ReleaseReferences();
                aWorkbook?.Save();
                Status = "";
            }


           
        }

        /// <summary>
        /// mark the new revision
        /// </summary>
        /// <param name="aResult"></param>
        private void MarkRevsMD(PageResult aResult)
        {
            aResult.HasChanged = true;
            if (!aResult.RevMarked)
            {
                SetCellValueXML(GetCellXML(aResult.Sheet1, aResult.RevNumAddress), aResult.RevNum);
                SetCellValueXML(GetCellXML(aResult.Sheet1, aResult.RevDateAddress), RevDate);
                SetCellValueXML(GetCellXML(aResult.Sheet1, aResult.RevByAddress), RevInits);

           }
            aResult.RevMarked = true;
            if (aResult.RevBoxColumnLetter != string.Empty && aResult.RowIDs != null)
            {
                string revcol = CellAddress("RevMarkCol");
                foreach (int item in aResult.RowIDs)
                {
                    if (aResult.MaxRevMarkRow <= 0 || item <= aResult.MaxRevMarkRow)
                    {
                        IXLRange markrange = GetRangeXML($"{revcol}{ item}", aResult.Sheet1);
                        IXLCell markcell = GetCellXML($"{revcol}{ item}", aResult.Sheet1);
                        SetCellValueXML(markcell, aResult.RevNum);


                    }
                }

            }



        }



      
        /// <summary>
        /// attempts to mark the changes between the current revisions of the report and the
        /// one one revision prior if it was found in the same output folder
        /// </summary>
        /// <returns></returns>
        private PageResults CreateResultsArray(XLWorkbook aWorkbook, XLWorkbook bWorkBook)
        {
            PageResults _rVal = new();
            IXLRange range1;
            IXLRange range2;

            foreach (uopDocReportPage page in RequestedPages)
            {
                string sheetname = page.TabName;
                IXLWorksheet aSheet = GetWorkSheetXML(sheetname, aWorkbook);


                if (aSheet == null) continue;
                IXLWorksheet bSheet = GetWorkSheetXML(sheetname, bWorkBook);
                if (bSheet == null)
                {
                    //the a sheet is a new sheet so clear the revisions
                    aSheet.Range(CellAddress("SignatureBlock1")).Clear();
                    aSheet.Range(CellAddress("SignatureBlock2")).Clear();
                    SaveWarning("CreateResultsArray", "Sheet not found", $"{sheetname} was not found in the previous report.", uppWarningTypes.General);
                    continue;


                }
                //copy the revision block from the previous rev to the new page
                List<string> revcells = RevCellAddresses;
                for (int rv = 0; rv < revcells.Count; rv++)
                {
                    if (rv != ProjectRev)
                    {
                        range1 = GetRangeXML(revcells[rv], bSheet);
                        range2 = GetRangeXML(revcells[rv], aSheet);
                        range1.CopyTo(range2);


                    }
                    else
                    {
                        range1 = GetRangeXML(revcells[rv], aSheet);
                        range1.Clear(XLClearOptions.Contents);
                    }
                }
               


                //copy the left revision check boxes from the last rev
                range1 = GetRangeXML("RevCells", bSheet, bNamedRange: true);
                range2 = GetRangeXML("RevCells", aSheet, bNamedRange: true);


                range1.CopyTo(range2);
                WorkBookXML.Save();


                PageResult result = new()
                {
                    SheetName = aSheet.Name,
                    SpanName = page.SpanName,
                    Page = page,
                    PageType = page.PageType,
                    Title = GetRangeStringXML("Title1", aSheet, bNamedRange: true).ToUpper(),
                    SubTitle1 = GetRangeStringXML("Title2", aSheet, bNamedRange: true).ToUpper(),
                    SubTitle2 = GetRangeStringXML("Title3", aSheet, bNamedRange: true).ToUpper(),
                    Sheet1 = aSheet, 
                    Sheet2 = bSheet
                };




                result.IsSketch = result.PageType == uppReportPageTypes.ColumnSketchPage || result.PageType == uppReportPageTypes.MDSpoutSketchPage || result.PageType == uppReportPageTypes.TraySketchPage;
                result.RevBoxColumnLetter = CellAddress("RevMarkCol");
                if (result.IsSketch)
                    result.MaxRevCheckRow = 11;
                if (result.PageType == uppReportPageTypes.TraySketchPage)
                    result.MaxRevMarkRow = 11;

                IXLRange aRange = GetRangeXML("RevBlock1", bSheet, bNamedRange: true);
                int RevRow = 0;
                result.RevNum = 0;
                string sRev1 = string.Empty;
                for (int j = 1; j <= 4; j++)
                {
                    uopProperty p1 = CellToProperty(aRange.Cell(j, 1));
                   
                    if (p1.Numeric)
                    {
                        result.RevNum = p1.ValueI; 
                    }
                    else
                    {
                        sRev1 = CellAddress("RevCol1");
                        RevRow = j + 4;
                        break;
                    }
                }

                if (sRev1 == string.Empty)
                {
                    aRange = GetRangeXML("SignatureBlock3", bSheet, bNamedRange: true);
                   
                    sRev1 = string.Empty;
                    for (int j = 1; j <= 4; j++)
                    {
                        uopProperty p1 = CellToProperty(aRange.Cell(j, 1));
                 
                        if (p1.Numeric)
                        {
                            result.RevNum = p1.ValueI;
                        }
                        else
                        {
                            sRev1 = CellAddress("RevCol2");
                            RevRow = j + 4;
                            break;
                        }
                    }
                }

                result.RevNum++;

                if (sRev1 == CellAddress("RevCol1") && RevRow == 9)
                {
                    RevRow = 5;
                    sRev1 = CellAddress("RevCol2");
                }

                if (sRev1 == string.Empty)
                {
                    RevRow = 8;
                    sRev1 = CellAddress("RevCol2");
                }

                result.RevNumAddress = $"{sRev1}{RevRow}";

                if (sRev1 == CellAddress("RevCol1"))
                {
                    result.RevDateAddress = $"{CellAddress("DateCol1")}{ RevRow}";
                    result.RevByAddress = $"{CellAddress("ReviewedCol1")}{RevRow}";
                }
                else
                {
                    result.RevDateAddress = $"{CellAddress("DateCol2")}{ RevRow}";
                    result.RevByAddress = $"{CellAddress("ReviewedCol2")}{RevRow}";
                }

                _rVal.Add(result);
            }

            return _rVal;
        }
     
        /// <summary>
        /// 
        /// </summary>
        /// <param name="aSheet"></param>
        /// <param name="bSheet"></param>
        /// <param name="aRangeName"></param>
        /// <param name="bCompareAll"></param>
        /// <param name="rRowIDs"></param>
        /// <param name="bCompareValues"></param>
        /// <returns></returns>
        private bool xCompareRange(IXLWorksheet aSheet, IXLWorksheet bSheet, string aRangeName, bool bCompareAll = false, List<int> aRowIDs = null, bool bCompareValues = false)
        {
            GetRangesXML(aSheet, bSheet, aRangeName, out IXLRange aRange, out IXLRange bRange);
            if (aRange == null || bRange == null) return false;
          
            if (aRange.RowCount() != bRange.RowCount()) return false;


            bool _rVal = true;
            int iR;
            for (iR = 1; iR <= aRange.RowCount(); iR++)
            {
                IXLRangeRow aRow = aRange.Row(iR);
                IXLRangeRow bRow = bRange.Row(iR);
                int iC;
                for (iC = 1; iC <= aRow.CellCount(); iC++)
                {
                    if (iC > bRow.CellCount()) break;

                    IXLCell aCell = aRow.Cell(iC);
                    IXLCell bCell = bRow.Cell(iC);
                    if (!bCompareValues)
                    {
                        if (aCell.GetString() != bCell.GetString())
                        {
                            if (aRowIDs != null) aRowIDs.Add(aCell.WorksheetRow().RowNumber());
                            _rVal = false;
                            break;
                        }
                    }
                    else
                    {
                        if (string.Compare(aCell.GetString(), bCell.GetString()) != 0)
                        {
                            aRowIDs.Add(aCell.WorksheetRow().RowNumber());
                            _rVal = false;
                            break;
                        }
                    }

                    if (CancelIt) break;

                }
                if (!bCompareAll && !_rVal) break;

                if (CancelIt) break;

            }

            return _rVal;
        }


      
        /// <summary>
        /// 
        /// </summary>
        /// <param name="aWorkBook"></param>
        /// <param name="aName"></param>
        /// <param name="rIndex"></param>
        /// <returns></returns>
        private IXLWorksheet xGetSheetByNameXML(XLWorkbook aWorkBook, string aName, out int rIndex)
        {
            IXLWorksheet _rVal = null;
            rIndex = 0;
            for (int i = 1; i <= aWorkBook.Worksheets.Count; i++)
            {
                IXLWorksheet aSht = aWorkBook.Worksheet(i);
                if (aSht.Name == aName)
                {
                    _rVal = aSht;
                    rIndex = i;
                    break;
                }
            }
            return _rVal;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="aSheet"></param>
        /// <param name="bSheet"></param>
        /// <param name="aRangeName"></param>
        /// <param name="rARange"></param>
        /// <param name="rBRange"></param>
        private void GetRangesXML(IXLWorksheet aSheet, IXLWorksheet bSheet, string aRangeName, out IXLRange rARange, out IXLRange rBRange, bool bNamedRange = false)
        {

            rARange = aSheet != null ? GetRangeXML(aRangeName, aSheet, bNamedRange: bNamedRange) : null;
            rBRange = bSheet != null ? GetRangeXML(aRangeName, bSheet, bNamedRange: bNamedRange) : null;


        }


        private void GetRanges(ExcelIO.Worksheet aSheet, ExcelIO.Worksheet bSheet, string aRangeName, out ExcelIO.Range rARange, out ExcelIO.Range rBRange, bool bNamedRange = false)
        {
            rARange = aSheet != null ? GetRange(aRangeName, aSheet, bNamedRange: bNamedRange) : null;
            rBRange = bSheet != null ? GetRange(aRangeName, bSheet, bNamedRange: bNamedRange) : null;

        }

   
        internal static uopProperty CellToProperty(IXLCell aCell, int aPrecis = 4)
        {
            if (aCell == null) return null;
            XLCellValue val = aCell.Value;
            string addr = aCell.Address.ToStringFixed();
            string strval = aCell.GetString();
            uopProperty _rVal = null;
            if (val.IsBlank)
            {
                _rVal = new uopProperty(addr, "");
            }
            else if (val.IsBoolean)
            {
                _rVal = new uopProperty(addr, mzUtils.VarToBoolean(strval));
            }
            else if (val.IsNumber)
            {
                _rVal = new uopProperty(addr, mzUtils.VarToDouble(strval, aPrecis: aPrecis));
            }
            else
            {
                _rVal = new uopProperty(addr, strval);
            }
            _rVal.Row = aCell.WorksheetRow().RowNumber();
            _rVal.Col = aCell.WorksheetColumn().ColumnNumber();
            return _rVal;
        }

        private uopProperties xGetRowProperties(IXLWorksheet aSheet, int aRowIndex, int aStartCol, int aColCount, out uopProperty rDifProp, out uopProperties rBSheetProps, string aColList = null, int aPrecis = 4, int aNameRowIndex = -1, string aMatchValue = null, IXLWorksheet bSheet = null, int aMatchMaxSearch = 40, bool bBailOnFirstDifference = false)
        {
            uopProperties _rVal = new();
            rBSheetProps = new uopProperties();
            rDifProp = null;
            if (aSheet == null || aStartCol <= 0 || aColCount <= 0) return _rVal;
            aMatchMaxSearch = mzUtils.LimitedValue(aMatchMaxSearch, 1, 200, 40);
            aPrecis = mzUtils.LimitedValue(0, 8, 4);

            string aVal;
            IXLColumn aCol;
            IXLCell aCell;
            int pRow = aRowIndex;

            if (aMatchValue != null)
            {
                pRow = 0;
                if (aMatchValue != string.Empty)
                {
                    if (aMatchValue.Trim() != string.Empty)
                    {
                        
                     
                        aCol = aSheet.Column(aStartCol);

                        for (int iR = 1; iR <= aCol.CellCount(); iR++)
                        {
                            aCell = aCol.Cell(iR);
                            aVal = aCell.GetString();
                            if (!string.IsNullOrEmpty(aVal))
                            {
                                if (string.Compare( aVal, aMatchValue,true) == 0)
                                {
                                    pRow = iR;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            if (pRow == 0)
                 return _rVal;

            IXLRow nRow = aNameRowIndex > 0 ? aSheet.Row(aNameRowIndex) : null;
            IXLRow aRow = aSheet.Row(pRow);
            IXLRow bRow = bSheet?.Row(pRow);

            for (int i = 1; i <= aColCount; i++)
            {
                if (mzUtils.ListContains(i, aColList, bReturnTrueForNullList: true))
                {
                    int iC = aStartCol + (i - 1);
                    aCell = aRow.Cell(iC);
                    uopProperty aProp = CellToProperty(aCell,aPrecis);
                    _rVal.Add(aProp);

                

                    if (nRow != null) aProp.Name = CellToProperty(nRow.Cell(1)).Name;
                      
                
                    if (bRow != null)
                    {
                        aCell = bRow.Cell(iC);
                        uopProperty bProp = CellToProperty(aCell, aPrecis);

                        rBSheetProps.Add(bProp);
                        if (!aProp.IsEqual(bProp, aPrecis))
                            rDifProp ??= aProp;

                        if (bBailOnFirstDifference) break;
                        
                    }
                }
            }
            return _rVal;
        }


    
        private uopProperties GetColumnProperties(IXLWorksheet aSheet, int aStartRow, int aRowCount, int aColumnIndex, string aRowList, int aPrecis = 4, int aNameColumnIndex = -1, string aMatchValue = null, IXLWorksheet bSheet = null, uopProperties rBSheetProps = null, int aMatchMaxSearch = 40)
        {
            uopProperties _rVal = new();
            rBSheetProps = new uopProperties();
            if (aSheet == null || aStartRow <= 0 || aRowCount <= 0)
            {
                return _rVal;
            }

            if (aPrecis > 8)
            {
                aPrecis = 8;
            }

            IXLColumn bCol = null;//Excel.Range
            IXLColumn nCol = null;//Excel.Range

            int pCol = aColumnIndex;
            string aVal;
            IXLCell aCell;
            if (aMatchValue != null)
            {
                pCol = 0;
                if (aMatchValue != string.Empty)
                {
                    if (aMatchValue.Trim() != string.Empty)
                    {
                        if (aMatchMaxSearch < 1)
                        {
                            aMatchMaxSearch = 1;
                        }
                        if (aMatchMaxSearch > 200)
                        {
                        }

                        IXLRow aRow = aSheet.Row(aStartRow);

                        int iC;
                        for (iC = 1; iC < aRow.CellCount(); iC++)
                        {
                            aCell = aRow.Cell(iC);
                            aVal = aCell.GetString();
                            if (!string.IsNullOrEmpty(aVal))
                            {
                                if (aVal == aMatchValue)
                                {
                                    pCol = iC;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            if (pCol == 0)
            {
                return _rVal;
            }

            if (aNameColumnIndex > 0)
            {
                nCol = aSheet.Column(aNameColumnIndex);
            }

            IXLColumn aCol = aSheet.Column(pCol);
            if (bSheet != null)
            {
                bCol = bSheet.Column(pCol);
            }

            int i;
            for (i = 1; i <= aRowCount; i++)
            {
                if (mzUtils.ListContains(i, aRowList, bReturnTrueForNullList: true))
                {
                    int iR = aStartRow + (i - 1);
                    string pname;
                    if (nCol != null)
                    {
                        pname = nCol.Cell(iR).GetString().Trim();
                    }
                    else
                    {
                        pname = string.Empty;
                    }
                    if (pname == string.Empty)
                    {
                        pname = $"ROW {i}";
                    }

                    aCell = aCol.Cell(iR);
                    aVal = aCell.GetString();
                    if (string.IsNullOrEmpty(aVal))
                    {
                        aVal = string.Empty;
                    }
                    if (aPrecis >= 0)
                    {
                        if (mzUtils.IsNumeric(aVal))
                        {
                            _rVal.Add(pname, mzUtils.VarToDouble(aVal, aPrecis: aPrecis));
                        }
                        else
                        {
                            _rVal.Add(pname, aVal);
                        }

                    }
                    else { _rVal.Add(pname, aVal); }

                    _rVal.SetRowCol(_rVal.Count, pCol, iR);

                    if (bCol != null)
                    {
                        aCell = bCol.Cell(iR);
                        aVal = aCell.GetString();
                        if (string.IsNullOrEmpty(aVal))
                        {
                            aVal = string.Empty;
                        }
                        if (aPrecis >= 0)
                        {
                            if (mzUtils.IsNumeric(aVal))
                            {
                                rBSheetProps.Add(pname, mzUtils.VarToDouble(aVal, aPrecis: aPrecis));
                            }
                            else
                            {
                                rBSheetProps.Add(pname, aVal);
                            }

                        }
                        else { rBSheetProps.Add(pname, aVal); }

                        rBSheetProps.SetRowCol(rBSheetProps.Count, pCol, iR);
                    }
                }
            }
            return _rVal;
        }

        #endregion Methods
    }

    /// <summary>
    /// Model for PageResults for revision
    /// </summary>
    class PageResult
    {

        public PageResult()
        {
            Differences = new uopProperties("Differences") { Name = "Diffences" };
            RowIDs = new List<int>();
           
           
        }

        public int Index { get; set; } = 0;
        public string SheetName { get; set; } = string.Empty;
        public string SpanName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string SubTitle1 { get; set; } = string.Empty;
        public string SubTitle2 { get; set; } = string.Empty;
        public string TrayName { get; set; } = string.Empty;
        public bool HasChanged { get; set; } = false;
        public bool IsSketch { get; set; } = false;
      
        public int MaxRevCheckRow { get; set; } = 0;
        public int MaxRevMarkRow { get; set; } = 0;
        public uppReportPageTypes PageType { get; set; }
        public bool RevMarked { get; set; } = false;
        public string RevBoxColumnLetter { get; set; } = string.Empty;
        public int RevNum { get; set; } = 0;
        public string RevNumAddress { get; set; } = string.Empty;
        public string RevDateAddress { get; set; } = string.Empty;
        public string RevByAddress { get; set; } = string.Empty;

        public uopProperties Differences { get; set; }

        internal IXLWorksheet Sheet1 { get; set; } = null;
        internal IXLWorksheet Sheet2 { get; set; } = null;

        internal List<int> RowIDs { get; set; } = null;


        public int SheetCount { get { if (Sheet1 == null && Sheet2 == null) return 0; if (Sheet1 != null && Sheet2 != null) return 2; return 1; } }

        public uopDocReportPage Page { get; set; } = null;

        public void ReleaseReferences()
        {
            Sheet1 = null;
            Sheet2 = null;
            Page = null;
        }

        public bool CompareRowProps( int aRowIndex, int aStartCol, int aColCount, int aPropNameRow = 0, bool bCompareAll = false, int aPrecis = 4)
        {
            bool _rVal = true;

           
            Differences = new uopProperties();
            RowIDs ??= new List<int>();

            if (Sheet1 == null || Sheet2 == null) return _rVal;

            if (aRowIndex <= 0 || aStartCol <= 0 || aColCount < 1) return _rVal;
            uopProperties aRow = GetRowProperties(aRowIndex, aStartCol, aColCount, out uopProperty difProp, out uopProperties bRow, aPrecis: aPrecis, aNameRowIndex: aPropNameRow, bBailOnFirstDifference: !bCompareAll);
            if (aRow.Count <= 0) return _rVal;

            if (!bCompareAll)
            {

                if (difProp != null)
                {
                    _rVal = false;
                    Differences.Add(difProp);
                    RowIDs.Add(aRowIndex);
                }
            }
            else
            {


                Differences = aRow.GetDifferences(bRow, aPrecis, !bCompareAll);

                if (Differences.Count > 0)
                {
                    RowIDs.Add(aRowIndex);
                }
            }
            return Differences.Count > 0;
        }
        private uopProperties GetRowProperties(int aRowIndex, int aStartCol, int aColCount, out uopProperty rDifProp, out uopProperties rBSheetProps, string aColList = null, int aPrecis = 4, int aNameRowIndex = -1, string aMatchValue = null, int aMatchMaxSearch = 40, bool bBailOnFirstDifference = false)
        {
            uopProperties _rVal = new();
           
            rBSheetProps = new uopProperties();
            rDifProp = null;
            
            if (Sheet1 == null || aStartCol <= 0 || aColCount <= 0) return _rVal;
            aMatchMaxSearch = mzUtils.LimitedValue(aMatchMaxSearch, 1, 200, 40);
            aPrecis = mzUtils.LimitedValue(0, 8, 4);
            string aVal;
            IXLColumn aCol;
            IXLCell aCell;
            int pRow = 0;
            string pname = string.Empty;
            uopProperty bProp = null;

            pRow = aRowIndex;

            if (aMatchValue != null)
            {
                pRow = 0;
                if (aMatchValue != string.Empty)
                {
                    if (aMatchValue.Trim() != string.Empty)
                    {


                        aCol = Sheet1.Column(aStartCol);

                        for (int iR = 1; iR <= aCol.CellCount(); iR++)
                        {
                            aCell = aCol.Cell(iR);
                            aVal = aCell.GetString();
                            if (!string.IsNullOrEmpty(aVal))
                            {
                                if (string.Compare(aVal, aMatchValue, true) == 0)
                                {
                                    pRow = iR;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            if (pRow == 0)
                return _rVal;

            IXLRow nRow = aNameRowIndex > 0 ? Sheet1.Row(aNameRowIndex) : null;

            IXLRow aRow = Sheet1.Row(pRow);
            IXLRow bRow = Sheet2?.Row(pRow);


            for (int i = 1; i <= aColCount; i++)
            {
                if (mzUtils.ListContains(i, aColList, bReturnTrueForNullList: true))
                {
                    int iC = aStartCol + (i - 1);
                    aCell = aRow.Cell(iC);
                    uopProperty aProp = ReportWriter.CellToProperty(aCell, aPrecis);
                    _rVal.Add(aProp);



                    if (nRow != null) aProp.Name = ReportWriter.CellToProperty(nRow.Cell(1)).Name;


                    if (bRow != null)
                    {
                        aCell = bRow.Cell(iC);
                        bProp = ReportWriter.CellToProperty(aCell, aPrecis);

                        rBSheetProps.Add(bProp);
                        if (!aProp.IsEqual(bProp, aPrecis))
                            rDifProp ??= aProp;

                        if (bBailOnFirstDifference) break;

                    }
                }
            }
            return _rVal;
        }

    }


    /// <summary>
    /// Model for storing list of TPage results for revision
    /// </summary>
    class PageResults : IEnumerable<PageResult>
    {
        readonly List<PageResult> _Members;

        public PageResults() { _Members = new List<PageResult>(); SketchIDs = new List<int>(); }

        public int Count { get => _Members.Count; }


        public PageResult Item(int aIndex)
        {
            if (aIndex < 0 || aIndex > Count) return new PageResult();
            _Members[aIndex - 1].Index = aIndex;
            return _Members[aIndex - 1];
        }


        internal List<int> SketchIDs { get; set; } = null;

        public void SetItem(int aIndex, PageResult aResult)
        {
            if (aIndex < 0 || aIndex > Count) return;
            _Members[aIndex - 1] = aResult;
        }
        public void Add(PageResult aResult)
        {
            aResult.Index = _Members.Count + 1;
            _Members.Add(aResult);
        }

        public void ReleaseReferences()
        {
            foreach (PageResult item in this)
            {
                item.ReleaseReferences();
            }
        }

        public PageResult GetResultByType(uppReportPageTypes aPageType, string aSubTitle = null)
        {
            PageResult _rVal = null;


            for (int i = 1; i <= Count; i++)
            {
                PageResult aRes = Item(i);
                if (aRes.PageType == aPageType)
                {
                    if (string.IsNullOrEmpty(aSubTitle))
                    {

                        _rVal = aRes;
                        break;
                    }
                    else
                    {
                        if (aRes.SubTitle1 == aSubTitle)
                        {

                            _rVal = aRes;
                            break;
                        }
                    }
                }
            }
            _rVal ??= new PageResult();

            return _rVal;
        }

        public IEnumerator<PageResult> GetEnumerator()
        {
            return ((IEnumerable<PageResult>)_Members).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_Members).GetEnumerator();
        }
    }
}
