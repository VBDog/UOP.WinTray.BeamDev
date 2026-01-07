using System;
using System.Collections.Generic;
using System.IO;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;


namespace UOP.WinTray.Projects.Documents
{
    /// <summary>
    /// uopDocReport Model
    /// </summary>
    public class uopDocReport : uopDocument, ICloneable
    {
        #region Constants

        private const string FILE_SEPARATOR = "\\";
        private const string PART_PATH = "PROJECT.REPORTS.RANGE({0})";
        private const string SPOUTING_DATA_REPORT = "Spouting Data Report";
        private const string SPACING_OPTIMIZATION_REPORT = "Spacing Optimization Report";
        private const string FEED_ZONE_REPORT = "Feed Zone Report";
        private const string TRAY_HARDWARE = "Tray Hardware";
        private const string DDM_DATA_REPORT = "DDM Data Report";
        private const string DISTRIBUTOR_DETAIL_REPORT = "Distributor Detail Report";

        #endregion

        #region Private Variables

        private uopDocuments _Pages = null;


        #endregion

        #region Constructors

        public uopDocReport() : base(uppDocumentTypes.Report) { _Pages = new uopDocuments(); _RequestedPages = new List<uopDocReportPage>(); _SelectedRanges = new List<int>(); }



        internal uopDocReport(uopDocReport aDocToCopy) : base(uppDocumentTypes.Report)
        {
            base.Copy(aDocToCopy);
            if (aDocToCopy == null) return;
            _Pages = aDocToCopy.Pages.Clone();
            _RequestedPages = new List<uopDocReportPage>();
            _SelectedRanges = new List<int>();
            RequestedPages = aDocToCopy.RequestedPages;
            SelectedRanges = aDocToCopy.SelectedRanges;
        }
        internal uopDocReport(TDOCUMENT aDoc, mzQuestions aOptions = null, mzQuestions aTrayQuery = null) : base(uppDocumentTypes.Report)
        {
            _Pages = new uopDocuments();
            _RequestedPages = new List<uopDocReportPage>();
            _SelectedRanges = new List<int>();
            aDoc.DocumentType = uppDocumentTypes.Report;
            Structure= aDoc.Clone();
            base.TrayQuery = aTrayQuery;
            base.Options = aOptions;
        }


        #endregion

        /// <summary>
        /// Get cloned object 
        /// </summary>
        /// <returns></returns>
        public uopDocReport Clone() => new uopDocReport(this);

        /// <summary>
        /// returns the Clone of the object
        /// </summary>
        public override uopDocument Clone(bool aFlag = false) => (uopDocument)this.Clone();

        /// <summary>
        /// returns the Clone of the object
        /// </summary>
        object ICloneable.Clone() => (object)this.Clone();



        #region Properties

        /// <summary>
        /// true if the report must contain all define report pages
        /// </summary>
        public bool AllPagesOnly { get => Props.ValueB("AllPagesOnly"); set => SetProp("AllPagesOnly", value); }
        /// <summary>
        /// true if the report must contain all define project tray ranges
        /// </summary>
        public bool AllRangesOnly { get => Props.ValueB("AllRangesOnly"); set => SetProp("AllRangesOnly", value); }

        public bool MechanicalTemplate { get => Props.ValueB("MechanicalTemplate"); set => SetProp("MechanicalTemplate", value); }

        public string ErrorString { get => base.Structure.String2; set { TDOCUMENT str = base.Structure; str.String2 = value; Structure= str; } }

        public override bool StandardFormat
        { get => mzUtils.ListContains(TemplateName, "MDSpoutReport,CrossFlowReport,MDSpoutReportXLT"); set => base.StandardFormat = value; }

        /// <summary>
        /// the base file name of the reports template
        /// </summary>
        public string TemplateName { get => Props.ValueS("TemplateName"); set { SetProp("TemplateName", value); base.StandardFormat = mzUtils.ListContains(value, "MDSpoutReport,CrossFlowReport,MDSpoutReportXLT"); } }
        /// <summary>
        /// the path to the reports template
        /// </summary>
        public string TemplatePath { get => Props.ValueS("TemplatePath"); set => SetProp("TemplatePath", value); }

        /// <summary>
        /// the name of the page to activate when the report is finished
        /// </summary>
        public string FirstTab { get => Props.ValueS("FirstTab"); set => SetProp("FirstTab", value); }
        /// <summary>
        /// toggles if the report file name property can be changed
        /// </summary>
        public bool FileNameLocked { get => Props.ValueB("FileNameLocked") && !string.IsNullOrWhiteSpace(FileName); set => SetProp("FileNameLocked", value); }
        /// <summary>
        /// the file location to create report in
        /// </summary>
        public string FilePath
        {
            get
            {
                string _rVal = string.Empty;
                if (!string.IsNullOrWhiteSpace(FolderName) && Directory.Exists(FolderName) && !string.IsNullOrWhiteSpace(FileName))
                {
                    _rVal = FolderName + FILE_SEPARATOR + FileName;
                    if (!File.Exists(_rVal)) _rVal = string.Empty;

                }
                return _rVal;
            }
        }
        /// <summary>
        /// the folder location to create report in
        /// </summary>
        public string FolderName { get => base.Structure.String1; set { TDOCUMENT str = base.Structure; str.String1 = value; Structure= str; } }

        /// <summary>
        /// toggles if the report folder name property can be changed
        /// </summary>
        public bool FolderNameLocked { get => Props.ValueB("FolderNameLocked") && FolderName != string.Empty; set => SetProp("FolderNameLocked", value); }

        /// <summary>
        /// true if the user has selected the page for inclusion in the report
        /// </summary>
        public bool IsRequested { get => Requested; set => Requested = value; }

        /// <summary>
        /// True tells the report writer to attempt to mark the changed lines in new revisions of the report
        /// </summary>
        public bool MaintainRevisionHistory { get => Props.ValueB("MaintainRevisionHistory"); set => SetProp("MaintainRevisionHistory", value); }


        /// <summary>
        /// the collection of all the pages (uopReportPages) in the report
        /// </summary>
        public uopDocuments Pages
        {
            get
            {
                if (_Pages == null) _Pages = new uopDocuments();
                return _Pages;
            }
            set => _Pages = value;
        }

        public string ProjectName { get => base.Structure.String3; set { TDOCUMENT str = base.Structure; str.String3 = value; Structure= str; } }

        /// <summary>
        /// True means the report writer will apply protection to the sheets in the report that have a Protected property = True
        /// </summary>
        public bool ProtectSheets { get => Protected; set => Protected = value; }


        public dynamic ReportForm { get; set; } = null;

        public string ReportName { get => SelectText; set => SelectText = value; }

        /// <summary>
        /// the type of report
        /// </summary>
        public uppReportTypes ReportType { get => (uppReportTypes)SubType;

            set
            {
                SubType = (int)value;
                //the type name of the report
                if (SelectText == string.Empty)
                {
                    switch (value)
                    {
                        case uppReportTypes.MDSpoutReport:
                            SelectText = SPOUTING_DATA_REPORT;
                            break;
                        case uppReportTypes.MDSpacingReport:
                            SelectText = SPACING_OPTIMIZATION_REPORT;
                            break;
                        case uppReportTypes.MDFeedZoneReport:
                            SelectText = FEED_ZONE_REPORT;
                            break;
                        case uppReportTypes.HardwareReport:
                            SelectText = TRAY_HARDWARE;
                            break;
                        case uppReportTypes.MDEDMReport:
                            SelectText = DDM_DATA_REPORT;
                            break;
                        case uppReportTypes.MDDistributorReport:
                            SelectText = DISTRIBUTOR_DETAIL_REPORT;
                            break;
                    }
                }
            }
        }

        private List<uopDocReportPage> _RequestedPages = null;
        /// <summary>
        /// the pages that have been requested prior to the generation of the report
        /// </summary>
        public List<uopDocReportPage> RequestedPages
        {
            get
            {
                if (_RequestedPages == null) _RequestedPages = new List<uopDocReportPage>();
                return _RequestedPages;
            }
            set
            {

                _RequestedPages = new List<uopDocReportPage>();
                if (value == null) return;

                uopDocReportPage aPage;

                for (int i = 1; i <= value.Count; i++)
                {
                    aPage = value[i-1];
                    aPage.Requested = true;
                    _RequestedPages.Add(aPage.Clone());
                }
            }
        }


        private List<int> _SelectedRanges = null;
        /// <summary>
        /// the list of tray range indices to include in the report
        /// </summary>
        public List<int> SelectedRanges
        {
            get
            {
                if (_SelectedRanges == null) _SelectedRanges = new List<int>();
                return _SelectedRanges;
            }
            set => _SelectedRanges = uopUtils.CopyCollection(value);

        }

        /// <summary>
        /// the number of steps to create the entire report
        /// </summary>
        public int StepCount
        {
            get
            {
                int _rVal = 0;
                uopDocReportPage aPage = null;
                List<uopDocReportPage> rPages = RequestedPages;
                for (int i = 0; i < rPages.Count; i++)
                {
                    aPage = rPages[i];
                    _rVal = _rVal + 2 + aPage.Drawings.Count + aPage.GraphCount;
                }

                return _rVal;
            }
        }

        /// <summary>
        /// the units that should be used in the report
        /// </summary>
        public uppUnitFamilies Units { get => DisplayUnits; set { if (!UnitsLocked) DisplayUnits = value; } }
        /// <summary>
        /// toggles if the report units property can be changed
        /// </summary>
        public bool UnitsLocked { get => Props.ValueB("UnitsLocked"); set => SetProp("UnitsLocked", value); }

        #endregion


        #region Methods

        public override string ToString() => $"uopDocReport[{ReportType.GetDescription()}]";
     

        /// <summary>
        /// Add Page to collection
        /// used to add a page to the report
        /// </summary>
        /// <param name="PageType"></param>
        /// <param name="IsRequired"></param>
        /// <param name="TabName"></param>
        /// <param name="Title"></param>
        /// <param name="SubTitle1"></param>
        /// <param name="SubTitle2"></param>
        /// <param name="bProtected"></param>
        /// <param name="aPassWord"></param>
        /// <returns></returns>
        public uopDocReportPage AddPage(uppReportPageTypes PageType, bool IsRequired, string TabName = "", string Title = "", string SubTitle1 = "", string SubTitle2 = "", bool bProtected = false, string aPassWord = "")
        {
            uopDocReportPage _rVal = new uopDocReportPage
            {
                ReportType = ReportType,
                PageType = PageType,
                IsRequired = IsRequired,
                TabName = TabName,
                Title = Title,
                SubTitle1 = SubTitle1,
                SubTitle2 = SubTitle2,
                Protected = bProtected,
                Password = aPassWord
            };

            _rVal = (uopDocReportPage)Pages.Add(_rVal);
            return _rVal;
        }

        /// <summary>
        /// Add Requested Page to Collection
        /// </summary>
        /// <param name="aBasePage"></param>
        /// <param name="aCount"></param>
        /// <param name="aTitle"></param>
        /// <param name="aTabName"></param>
        /// <param name="aProtected"></param>
        /// <returns></returns>
        public List<uopDocReportPage> AddRequestedPage(uopDocReportPage aBasePage, int aCount, dynamic aTitle, dynamic aTabName, bool aProtected)
        {
            _RequestedPages ??= new List<uopDocReportPage>();
            
            if (aBasePage == null) return _RequestedPages;
            

            for (int i = 0; i < aCount; i++)
            {
                uopDocReportPage aPage = aBasePage.Clone();
                aPage.ReportType = ReportType;
                if (!string.IsNullOrEmpty(aTitle)) aPage.Title = Convert.ToString(aTitle);
                
                if (!string.IsNullOrEmpty(aTabName)) aPage.TabName = Convert.ToString(aTabName);
                
                if (aProtected) aPage.Protected = true;

                aPage.SubPage = (aCount > 1) ? i : 0;
                _RequestedPages.Add(aPage);
            }
            return _RequestedPages;
        }

      

        /// <summary>
        /// Get Is Page Required Method
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public bool PageIsRequired(int pageIndex)
        {
            if (pageIndex <= 0 || pageIndex > Pages.Count)  return false;
            uopDocReportPage aPage = (uopDocReportPage)Pages[pageIndex - 1];
            return aPage.IsRequired || AllPagesOnly;
        }

        /// <summary>
        /// Return if selected Range is requested
        /// </summary>
        /// <param name="rangeID"></param>
        /// <returns></returns>
        public bool RangeIsRequested(int rangeID)
        {
            if (AllRangesOnly) return true;
            for (int i = 0; i < SelectedRanges.Count; i++)
            {
                if (SelectedRanges[i] == rangeID) return true;
            }
            return false;
        }

        /// <summary>
        /// Set Visible pages
        /// </summary>
        public void SetVisiblePages()
        {
            //^sets visibility for pages in the report

            // TODO (not supported):     On Error Resume Next

            uopParts aParts = null;
            uopProject proj = Project;
            uopDocuments pageCol = Pages;
            List<uopDocReportPage> requested = RequestedPages;
            for (int i = 0; i < pageCol.Count; i++)
            {
                uopDocReportPage aPage = (uopDocReportPage)pageCol[i];
                if(requested.FindIndex(x => x.PageType == aPage.PageType) >= 0)
                {
                    aPage.Requested = true;
                }
                if (aPage.PageType == uppReportPageTypes.MDChimneyTrayPage)
                {
                    aParts = proj.ChimneyTrays;
                    if (aParts == null)
                    { aPage.IsHidden = true; }
                    else
                    { aPage.IsHidden = aParts.Count == 0; }
                }
                else if (aPage.PageType == uppReportPageTypes.MDDistributorPage)
                {
                    aParts = proj.Distributors;
                    if (aParts == null)
                    { aPage.IsHidden = true; }
                    else
                    { aPage.IsHidden = aParts.Count == 0; }

                }
                else if (aPage.PageType == uppReportPageTypes.MDSpoutSketchPage)
                {
                    aPage.IsHidden = !proj.HasTriangularEndPlates;
                }
            }
        }

        /// <summary>
        /// Set Sub part
        /// </summary>
        /// <param name="aPart"></param>
        public void SubPart(ref uopPart aPart) => Part = aPart;
            


        /// <summary>
        /// Get Visible Pages
        /// the collection of all the pages (uopReportPages) in the report that are not hidded
        /// </summary>
        /// <param name="RequestedOnly"></param>
        /// <returns></returns>
        public List<uopDocReportPage> VisiblePages(bool RequestedOnly)
        {
            uopDocuments pageCol = Pages;
            SetVisiblePages();
            List<uopDocReportPage> _rVal = new List<uopDocReportPage>();
            for (int i = 1; i <= pageCol.Count; i++)
            {
                uopDocReportPage aPage = (uopDocReportPage)pageCol.Item(i);
                aPage.ReportType = ReportType;
                if (!aPage.IsHidden && (!RequestedOnly || (RequestedOnly && aPage.IsRequested)))
                {
                    _rVal.Add(aPage);
                }
            }
            return _rVal;
        }

        #endregion Methods

        internal override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            _Pages?.Dispose(true);
            _Pages = null;
            if (_RequestedPages != null)
            {
                foreach (var item in _RequestedPages)
                {
                    item.Dispose(true);
                }
                _RequestedPages.Clear();
            }
            _RequestedPages = null;
            _SelectedRanges?.Clear();
            _SelectedRanges = null;

        }
    }
}