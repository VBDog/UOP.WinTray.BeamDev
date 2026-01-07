using UOP.DXFGraphics;
using System;
using System.Collections.Generic;
using System.Linq;
using UOP.WinTray.Projects.Documents;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Interfaces;
using UOP.WinTray.Projects.Parts;
using UOP.WinTray.Projects.Tables;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;

namespace UOP.WinTray.Projects.Generators
{
    public static class uopReportGenerator
    {


        #region Private Variables

        private static mdProject _MDProject = null;
        private static List<uopDocWarning> Warnings;
        #endregion

        #region Public Methods

        /// <summary>
        /// populates then reports requested pages collection based on the collection of requested ranges and the members of 
        /// its visible pages collection. each visible page can give rise to multiple requested pages.
        /// </summary>
        /// <param name="aProject">the subject project</param>
        /// <param name="aReport">the subject report</param>
        public static void FormatReport(uopProject aProject, uopDocReport aReport)
        {
            if (aReport == null || aProject == null) return;
            if (aProject.ProjectFamily == uppProjectFamilies.uopFamMD)
                FormatReport_MD((mdProject)aProject, aReport);
        }

        /// <summary>
        /// defines the tables of the passed page based on the data in the project
        /// </summary>
        /// <param name="aProject">the project to get the data from</param>
        /// <param name="aPage">the page to populate</param>
        /// <param name="aUnits">the index of the page to poplate</param>
        /// <returns></returns>
        public static dynamic PopulateReportPage(uopProject aProject, uopDocReportPage aPage, uppUnitFamilies aUnits, List<uopDocWarning> aWarnings)
        {
            if (aProject == null || aPage == null) return null;
            Warnings = aWarnings;
            try
            {
                if (aProject.ProjectFamily == uppProjectFamilies.uopFamMD)
                    return rpt_MD_PopulateReportPage((mdProject)aProject, aPage, aUnits);
            }
            catch (Exception e)
            {
                SaveWarning("PopulateReportPage", aPage.SelectText, e: e);
            }
            finally
            {
                Warnings = null;
            }


            return null;
        }

        private static uopDocWarning SaveWarning(string aMethodName, string aBrief, string aTextString = "", uppWarningTypes wType = uppWarningTypes.General, Exception e = null)
        {

            if (Warnings == null) return null;


            if (string.IsNullOrWhiteSpace(aBrief)) aBrief = aTextString;
            if (e != null)
            {
                aTextString = string.IsNullOrWhiteSpace(aTextString) ? e.Message : $"{aTextString.Trim()} - {e.Message}";
            }

            if (string.IsNullOrWhiteSpace(aTextString)) aTextString = aBrief;

            if (string.IsNullOrWhiteSpace(aTextString) && string.IsNullOrWhiteSpace(aBrief)) return null;

            aTextString = aTextString.Trim();
            aBrief = aBrief.Trim();


            uopDocWarning _rVal = new uopDocWarning()
            {
                Owner = aMethodName,
                Brief = aBrief,
                TextString = aTextString,
                WarningType = wType,

            };



            Warnings.Add(_rVal);



            return _rVal;
        }

        /// <summary>
        /// Sort Pages in Report
        /// </summary>
        /// <param name="aReport"></param>
        /// <param name="aProject"></param>
        public static void SortReportPages(uopDocReport aReport, uopProject aProject)
        {
            if (aReport == null) return;
            aProject ??= aReport.Project;

            if (aProject == null) return;
            if (aProject.ProjectFamily == uppProjectFamilies.uopFamMD) rpt_MD_SortReportPages(aReport, (mdProject)aProject);
        }

        #endregion Public Methods

        #region Private Methods
        /// <summary>
        /// Sort Pages in Report
        /// </summary>
        /// <param name="aReport"></param>
        /// <param name="aProject"></param>
        private static void rpt_MD_SortReportPages(uopDocReport aReport, mdProject aProject)
        {
            if (aReport == null) return;
            aProject ??= (mdProject)aReport.Project;

            if (aProject == null) return;

            if (aReport.ReportType != uppReportTypes.MDSpoutReport) return;
            List<uopDocReportPage> aPages = aReport.RequestedPages;
            if (aPages.Count <= 1) return;

            uopDocReportPage aPage;
            List<string> cGuids = aProject.TrayRanges.GUIDS;

            //extract the pages that are for a single tray range
            List<uopDocReportPage> rPages = new List<uopDocReportPage>();
            for (int i = aPages.Count - 1; i >= 0; i--)
            {
                aPage = aPages[i];
                string aGUID = aPage.RangeGUID;
                if (!string.IsNullOrEmpty(aGUID))
                {
                    rPages.Add(aPage);
                    aPages.RemoveAt(i);
                }
            }

            for (int i = 0; i < cGuids.Count; i++)
            {
                string aGUID = cGuids[i];
                for (int j = rPages.Count - 1; j >= 0; j--)
                {
                    aPage = rPages[j];
                    if (aPage.RangeGUID == aGUID)
                    {
                        aPages.Add(aPage);
                        rPages.RemoveAt(j);
                    }
                }
            }

            //    Set aReport.RequestedPages = aPages
        }

        /// <summary>
        /// populates then reports requested pages collection based on the collection of requested ranges and the members of 
        /// its visible pages collection. each visible page can give rise to multiple requested pages.
        /// </summary>
        /// <param name="aProject">the subject project</param>
        /// <param name="aReport">the subject report</param>
        private static void FormatReport_MD(mdProject aProject, uopDocReport aReport)
        {
            if (aReport == null || aProject == null) return;


            uopDocReportPage bPage;
            mdTrayRange aRange;
            List<uopDocReportPage> aAddedPages;

            mdTrayAssembly aAssy;


            //get the selected ranges. users mark the ranges to include
            //on the report generation form
            Dictionary<string, uopTrayRange> selRanges = aProject.TrayRanges.GetRequested(true);
            List<uopTrayRange> ranges = selRanges.Values.ToList();
            //get the visible pages of the report. users mark the pages to include
            //on the report generation form
            List<uopDocReportPage> vispages = aReport.VisiblePages(true);
            aReport.RequestedPages = new List<uopDocReportPage>();
            //ADD THE ONE PER REPORT PAGES
            foreach (uopDocReportPage page in vispages)
            {

                switch (page.PageType)
                {
                    //---------------------------------------------------------------------------------
                    case uppReportPageTypes.TestPage:
                        //---------------------------------------------------------------------------------
                        //one per selected range
                        //if (!page.OnePerRange)
                        //{
                        //    //one per selected range
                        //    xxOnePagePerRange(aProject, aReport, page, selRanges, "Test Page", aAddedPages);
                        //}
                        //else
                        //{
                        //    //one per visble page
                        //    aReport.AddRequestedPage(page, 1, "Test Page", "Test", page.Protected);
                        //}
                        //---------------------------------------------------------------------------------
                        break;
                    case uppReportPageTypes.DCSpacingOptimizationPage:
                        //---------------------------------------------------------------------------------
                        //one per selected range
                        //xxOnePagePerRange(aProject, aReport, page, selRanges, "Spacing Data", aAddedPages);
                        //---------------------------------------------------------------------------------
                        break;
                    case uppReportPageTypes.MDOptimizationPage://Spouting Data Report
                        //---------------------------------------------------------------------------------
                        //one per selected range
                        xxOnePagePerRange(aProject, aReport, page, ranges, "Optimization Results", out aAddedPages);
                        //---------------------------------------------------------------------------------
                        break;
                    case uppReportPageTypes.ProjectSummaryPage://Spouting Data Report
                        //---------------------------------------------------------------------------------
                        //one per visible page
                        aReport.AddRequestedPage(page, 1, "Column Summary", "Column Summary", true);
                        //---------------------------------------------------------------------------------
                        break;
                    case uppReportPageTypes.ColumnSketchPage://Spouting Data Report
                        //---------------------------------------------------------------------------------
                        //one per projects sketch count
                        xxAddPagesByMaxMembers(aReport, 1, aProject.ColumnSketchCount, page, null, "Column Sketch", "Column Sketch");
                        //---------------------------------------------------------------------------------
                        break;
                    case uppReportPageTypes.TraySummaryPage://Spouting Data Report
                        //---------------------------------------------------------------------------------
                        //6 ranges per page per selected range
                        xxAddPagesByMaxMembers(aReport, 6, selRanges.Count, page, null, "Tray Design Summary", "Design Summary");
                        //---------------------------------------------------------------------------------
                        break;
                    case uppReportPageTypes.MDChimneyTrayPage:
                        //---------------------------------------------------------------------------------
                        xxMDFormatCaseOwnerPages(false, aProject, aReport, page);
                        //---------------------------------------------------------------------------------
                        break;
                    case uppReportPageTypes.MDDistributorPage://Spouting Data Report
                        //---------------------------------------------------------------------------------
                        xxMDFormatCaseOwnerPages(true, aProject, aReport, page);
                        //---------------------------------------------------------------------------------
                        break;
                    case uppReportPageTypes.TraySketchPage://Spouting Data Report
                        //---------------------------------------------------------------------------------
                        //one per selected range
                        xxOnePagePerRange(aProject, aReport, page, ranges, "Tray Sketch", out aAddedPages, uppDrawingTypes.TraySketch, "D13:AM33");
                        //---------------------------------------------------------------------------------
                        break;
                    case uppReportPageTypes.MDEDMPage1:
                        //---------------------------------------------------------------------------------
                        //one per visible page
                        aReport.AddRequestedPage(page, 1, "Tray General And Process Information", "Tray General And Process Info", false);
                        //---------------------------------------------------------------------------------
                        break;
                    case uppReportPageTypes.MDEDMPage2:
                        //---------------------------------------------------------------------------------
                        //6 selected ranges per page
                        xxAddPagesByMaxMembers(aReport, 6, selRanges.Count, page, null, "Tray Mechanical Summary", "Tray Mechanical Summary");
                        //---------------------------------------------------------------------------------
                        break;
                    case uppReportPageTypes.MDEDMPage3:
                        //---------------------------------------------------------------------------------
                        //6 selected ranges per page
                        xxAddPagesByMaxMembers(aReport, 6, selRanges.Count, page, null, "Tray Hydraulic Parameters", "Tray Hydraulic Parameters");
                        //---------------------------------------------------------------------------------
                        break;
                    case uppReportPageTypes.MDSpoutDetailPage://Spouting Data Report
                        //---------------------------------------------------------------------------------
                        foreach (var item in selRanges)
                        {
                            aRange = (mdTrayRange)item.Value;
                            List<mdSpoutGroup> aSGs = aRange.TrayAssembly.SpoutGroups.GetByVirtual(aVirtualValue: false);
                            //45 per page per selected range (spout group details)
                            xxAddPagesByMaxMembers(aReport, 45, aSGs.Count, page, aRange, "Spouting Details", "Spout Details");

                            bPage = page.Clone();
                            bPage.PageType = uppReportPageTypes.MDSpoutConstraintPage;
                            //40 per page per selected range (constraint details)
                            xxAddPagesByMaxMembers(aReport, 40, aSGs.Count, bPage, aRange, "Spout Group Constraints", "Constraints");
                        }
                        //---------------------------------------------------------------------------------
                        break;
                    case uppReportPageTypes.MDFeedZonePage:
                        //---------------------------------------------------------------------------------
                        //22 max per page per selected range. drawing on all pages
                        foreach (var item in selRanges)
                        {
                            aRange = (mdTrayRange)item.Value;
                            aAssy = aRange.TrayAssembly;
                            List<mdFeedZone> zones = aAssy.DeckPanels.FeedZones(aAssy);
                            xxAddPagesByMaxMembers(aReport, 22, zones.Count, page, aRange, "Feed Zones", aDrawing: new uopDocDrawing(uppDrawingFamily.Sketch, "Feed Zones", uppDrawingTypes.FeedZones, aRange) { PasteAddress = "D13", DeviceSize = new System.Drawing.Size(300, 300) });
                        }
                        //---------------------------------------------------------------------------------
                        break;
                    case uppReportPageTypes.MDSpoutSketchPage:
                        //---------------------------------------------------------------------------------
                        //sketches of the spout groups that are affected by
                        //a downcomer with a triangular end plate
                        //two sketches per page

                        #region Commented Code for SpoutSketchImage from report - retaining the code as per Mike's suggestion so that we can use it in future
                        //colUOPTrayRanges coltrayranges = new colUOPTrayRanges();
                        //foreach (uopTrayRange tray in ranges)
                        //{
                        //    coltrayranges.Add(tray);
                        //}
                        //colMDSpoutGroups aCol1 = aProject.SpoutGroups(coltrayranges, true, true);

                        //for (j = 0; j < aCol1.Count; j += 2)
                        //{
                        //    m = 0;
                        //    aSG = aCol1.Item(j);
                        //    //Verify an dchange the logic when we have other projects available apart from MDSPoutProject
                        //    aSG.ProjectFamily = uppProjectFamilies.uopFamMD;
                        //    aDWG = aSG.Sketch(aPasteAddress: "D13:AM33");

                        //    bDWG = null;
                        //    if (j + 1 <= aCol1.Count)
                        //    {
                        //        bSG = aCol1.Item(j + 1);
                        //        if (bSG != null)
                        //        {
                        //            //Verify an dchange the logic when we have other projects available apart from MDSPoutProject
                        //            bSG.ProjectFamily = uppProjectFamilies.uopFamMD;
                        //            if (bSG.RangeGUID == aSG.RangeGUID)
                        //            {
                        //                bDWG = bSG.Sketch(aPasteAddress: D35_AM53);
                        //            }
                        //            else
                        //            {
                        //                //skip back one
                        //                m = -1;
                        //            }
                        //        }
                        //    }
                        //    aRange = (mdTrayRange)selRanges[aSG.RangeGUID];

                        //    bPage = page.Clone();
                        //    bPage.Range = aRange;
                        //    aDWG.Tag = aSG.Descriptor;
                        //    aDWG.TagAddress = AP14;
                        //    bPage.Drawings.Add(aDWG);
                        //    bPage.Protected = true;
                        //    bPage.Title = SPOUT_GROUPS_TRAYS + aRange.SpanName();
                        //    bPage.TabName = SPOUT_GROUPS + string.Format(WRAP_PARANTHESIS, aRange.SpanName());
                        //    bPage.SubTitle1 = SPOUT_GROUP + aSG.Handle;
                        //    bPage.SubTitle2 = String.Empty;
                        //    if (bDWG != null)
                        //    {
                        //        bDWG.Tag = bSG.Descriptor;
                        //        bDWG.TagAddress = AP36;
                        //        bPage.Drawings.Add(bDWG);
                        //        bPage.SubTitle2 = SPOUT_GROUP + bSG.Handle;
                        //    }
                        //    aReport.RequestedPages.Add(bPage);
                        //    //skip back one
                        //    j = j + m;

                        //}

                        #endregion
                        //---------------------------------------------------------------------------------
                        break;
                    case uppReportPageTypes.DCStressPage:
                        //---------------------------------------------------------------------------------
                        //one page per downcomer
                        mdPartMatrix parray = aProject.GetParts();
                        uopPartList<mdDowncomerBox> dcs = parray.Boxes();
                        foreach (uopTrayRange range in ranges)
                        {
                            List<mdDowncomerBox> rangedcs = dcs.FindAll((x) => x.IsAssociatedToRange(range.GUID));
                            foreach (uopPart item in rangedcs)
                            {
                                mdDowncomerBox rangedc = item as mdDowncomerBox;
                                mdTrayRange mdrng = range as mdTrayRange;
                                mdDowncomer mddc = rangedc.GetMDDowncomer(mdrng.TrayAssembly);
                                bPage = page.Clone();
                                bPage.Part = rangedc;
                                bPage.Range = range;
                                bPage.TemplateTabName = "DCSTRENGTH";
                                bPage.Title = $"Tray {rangedc.RingRange.SpanName} DC {mddc.Index}";
                                bPage.TabName = bPage.Title;
                                aReport.RequestedPages.Add(bPage);
                            }
                        }


                        //---------------------------------------------------------------------------------
                        break;
                    case uppReportPageTypes.HardwareTotals:
                        //---------------------------------------------------------------------------------
                        bPage = page.Clone();
                        bPage.TemplateTabName = "Totals";
                        bPage.NoTemplate = true;
                        bPage.Title = "Totals";
                        bPage.TabName = "Totals";
                        bPage.PageType = uppReportPageTypes.HardwareTotals;
                        bPage.DontSaveWithPassword = true;
                        bPage.SuppressTabName = true;
                        aReport.RequestedPages.Add(bPage);

                        //---------------------------------------------------------------------------------
                        break;
                    case uppReportPageTypes.TrayHardwarePage:
                        //---------------------------------------------------------------------------------


                        foreach (uopTrayRange item in ranges)
                        {
                            bPage = page.Clone();
                            bPage.PartIndex = item.Index;
                            bPage.Range = item;
                            bPage.TemplateTabName = "Trays";
                            bPage.Title = $"Trays {item.SpanName()}";
                            bPage.TabName = bPage.Title;
                            bPage.DontSaveWithPassword = true;
                            bPage.SuppressTabName = true;
                            aReport.RequestedPages.Add(bPage);

                        }

                        break;
                }

            }


            rpt_MD_SortReportPages(aReport, aProject);

        }

        /// <summary>
        /// Add pages based on members
        /// </summary>
        /// <param name="aReport"></param>
        /// <param name="aMaxMembers"></param>
        /// <param name="aColCount"></param>
        /// <param name="aBasePage"></param>
        /// <param name="aRange"></param>
        /// <param name="aTitle"></param>
        /// <param name="aTabName"></param>
        /// <param name="aDrawing"></param>
        /// <param name="aProtected"></param>
        private static void xxAddPagesByMaxMembers(uopDocReport aReport, int aMaxMembers, int aColCount, uopDocReportPage aBasePage, uopTrayRange aRange, string aTitle, string aTabName = "", uopDocDrawing aDrawing = null, bool aProtected = false)
        {
            if (aColCount <= 0 || aBasePage == null) return;

            uopDocReportPage cPage = aBasePage.Clone();
            uopDocReportPage dPage;
            int tot;
            int rmd;


            cPage.Range = aRange;

            aMaxMembers = Math.Abs(aMaxMembers);
            if (aMaxMembers == 0) aMaxMembers = 5;

            aTitle = string.IsNullOrWhiteSpace(aTitle) ? aTitle = cPage.Title : aTitle.Trim();
            if (string.IsNullOrWhiteSpace(aTitle)) aTitle = "Page";

            aTabName = string.IsNullOrWhiteSpace(aTabName) ? aTitle : aTabName.Trim();
            tot = aColCount / aMaxMembers;
            if (tot * aMaxMembers < aColCount)
            {
                rmd = aColCount - (tot * aMaxMembers);
                tot += 1;
            }


            if (aProtected) cPage.Protected = true;

            cPage.SubPage = 0;
            cPage.Title = aTitle;
            cPage.TabName = aTabName;
            if (aRange != null)
            {
                cPage.TabName = $"{cPage.TabName}({aRange.SpanName()})";
                cPage.SubTitle1 = aRange.Name(true);
            }

            if (aDrawing != null)
            {
                cPage.Drawings.Add(aDrawing);
            }
            cPage.MaxMembers = aMaxMembers;
            cPage.StartIndex = 1;
            cPage.EndIndex = aColCount;

            if (tot == 1)
            {
                aReport.RequestedPages.Add(cPage);
            }
            else
            {
                for (int i = 1; i <= tot; i++)
                {
                    dPage = cPage.Clone();

                    dPage.SubPage = i;
                    dPage.SubTitle2 = $"Page {i} of {tot}";
                    dPage.Range = aRange;
                    dPage.StartIndex = (i - 1) * aMaxMembers + 1;
                    dPage.EndIndex = dPage.StartIndex + aMaxMembers - 1;
                    if (dPage.EndIndex > aColCount)
                    {
                        dPage.EndIndex = aColCount;
                    }


                    aReport.RequestedPages.Add(dPage);
                }
            }
        }

        /// <summary>
        /// defines the tables of the passed page based on the data in the project
        /// </summary>
        /// <param name="aProject">the project to get the data from</param>
        /// <param name="aPage">the page to populate</param>
        /// <param name="aUnits">the index of the page to poplate</param>
        /// <returns></returns>
        private static dynamic rpt_MD_PopulateReportPage(mdProject aProject, uopDocReportPage aPage, uppUnitFamilies aUnits)
        {
            dynamic _rVal = null;
            if (aProject == null || aPage == null) return _rVal;

            _MDProject = aProject;
            //aPage.SubPart(aPage.Part);
            mdTrayRange aRange;

            aPage.Units = aUnits;


            switch (aPage.PageType)
            {
                case uppReportPageTypes.TrayHardwarePage:
                case uppReportPageTypes.HardwareTotals:
                    xMD_Page_MDHardware(aPage);
                    break;
                case uppReportPageTypes.TestPage:
                    xMD_Page_Test(aPage);
                    break;
                case uppReportPageTypes.DCSpacingOptimizationPage:
                    xMD_Page_SpacingOptimization(aPage);
                    break;
                case uppReportPageTypes.MDOptimizationPage:
                    xMD_Page_Optimization(aPage);
                    break;
                case uppReportPageTypes.MDSpoutDetailPage:
                    xMD_Page_SpoutDetails(aPage);
                    break;
                case uppReportPageTypes.MDSpoutConstraintPage:
                    xMD_Page_Constraints(aPage);
                    break;
                case uppReportPageTypes.MDFeedZonePage:
                    xMD_Page_FeedZones(aPage);
                    break;
                case uppReportPageTypes.MDEDMPage1:
                    xMD_Page_EDM1(aPage);
                    break;
                case uppReportPageTypes.MDEDMPage2:
                    xMD_Page_EDM2(aPage);
                    break;
                case uppReportPageTypes.MDEDMPage3:
                    xMD_Page_EDM3(aPage);
                    break;
                case uppReportPageTypes.ProjectSummaryPage:
                    xMD_Page_ProjectSummary(aPage);
                    break;
                case uppReportPageTypes.WarningPage:
                    //aPage.Title = "Project Warnings"
                    //aPage.Protected = true
                    break;
                case uppReportPageTypes.TraySummaryPage:
                    xMD_Page_TraySummary(aPage);
                    break;
                case uppReportPageTypes.MDDistributorPage:
                    xMD_Page_CaseOwners(true, aPage);
                    break;
                case uppReportPageTypes.MDChimneyTrayPage:
                    xMD_Page_CaseOwners(false, aPage);
                    break;
                case uppReportPageTypes.TraySketchPage:
                    aPage.Title = "Tray Sketch";
                    aRange = (mdTrayRange)aPage.Range;
                    if (aRange == null)
                    {
                        aPage.SubTitle1 = $"Trays {aPage.SpanName}";
                    }
                    else
                    {
                        aPage.SubTitle1 = aRange.Name(true);
                    }
                    aPage.Protected = true;
                    break;
                case uppReportPageTypes.DCStressPage:
                    xMD_Page_DCStress(aPage);
                    break;
            }

            _MDProject = null;

            return _rVal;
        }

        //private static dynamic rpt_XF_PopulateReportPage( xfProject aProject,  uopDocReportPage aPage,  uppUnitFamilies aUnits)
        //{
        //    dynamic rpt_XF_PopulateReportPage = null;
        //    //#1the project to get the data from
        //    //#2the page to populate
        //    //#3the index of the page to poplate
        //    //^defines the tables of the passed page based on the data in the project

        //    if (aProject == null || aPage == null)
        //    {
        //        return rpt_XF_PopulateReportPage;
        //    }

        //    oProjectXF = aProject;
        //    aPage.Project = aProject;

        //    aPage.Units = aUnits;


        //    if (aPage.PageType == uppReportPageTypes.TestPage)
        //    {
        //        xXF_Page_Test(aPage);
        //    }

        //    oProjectXF = null;

        //    return rpt_XF_PopulateReportPage;
        //}

        //private static void rpt_XF_SortReportPages( uopDocReport aReport,  xfProject aProject)
        //{
        //    // TODO (not supported):     On Error Resume Next
        //    if (aReport == null)
        //    {
        //        return;
        //    }
        //    if (aProject == null)
        //    {
        //        aProject = aReport.Project;
        //    }
        //    if (aProject == null)
        //    {
        //        return;
        //    }
        //    if (aReport.ReportType != uppReportTypes.MDSpoutReport)
        //    {
        //        return;
        //    }

        //    Collection aPages = null;

        //    Collection rPages = null;

        //    Collection nPages = null;

        //    uopDocReportPage aPage = null;

        //    int i = 0;

        //    int j = 0;

        //    string aGUID = string.Empty;

        //    Collection cGuids = null;

        //    bool bFlag = false;


        //    aPages = aReport.RequestedPages;
        //    if (aPages.Count <= 0)
        //    {
        //        return;
        //    }

        //    //extract the pages that are for a single tray range
        //    rPages = new Collection();
        //    cGuids = aProject.TrayRanges.GUIDS;
        //    for (i = aPages.Count; i > 1; i--)
        //    {
        //        aPage = aPages.Item(i);
        //        aGUID = aPage.RangeGUID;
        //        if (aGUID !=  string.Empty)
        //        {
        //            rPages.Add(aPage);
        //            aPages.Remove(i);
        //        }
        //    }

        //    for (i = 1; i < cGuids.Count; i++)
        //    {
        //        aGUID = cGuids.Item(i);
        //        for (j = rPages.Count; j > 1; j--)
        //        {
        //            aPage = rPages.Item(j);
        //            if (StrComp(aPage.RangeGUID, aGUID, vbTextCompare) == 0)
        //            {
        //                aPages.Add(aPage);
        //                rPages.Remove(j);
        //            }
        //        }
        //    }

        //    //    Set aReport.RequestedPages = aPages
        //}

        //private static void xConvertUnits(mzValues aUnits, mzValues aNumberFormats)
        //{
        //    int i = 0;
        //    string uni = string.Empty;
        //    string fmt = string.Empty;
        //    int decis = 0;

        //    for (i = 1; i < aUnits.Count; i++)
        //    {
        //        uni = aUnits.Item(i);

        //        if (i <= aNumberFormats.Count)
        //        {
        //            fmt = aNumberFormats.Item(i);
        //            decis = fmt.IndexOf('.');

        //            if (decis > 0)
        //            {
        //                decis = fmt.Length - decis;
        //                if (uni != "%" && uni !=  string.Empty)
        //                {
        //                    if (decis > 3)
        //                    {
        //                        decis = decis - 2;
        //                    }
        //                    else
        //                    {
        //                        if (decis == 2)
        //                        {
        //                            decis = 1;
        //                        }
        //                    }
        //                }
        //            }
        //            if (decis > 0)
        //            {
        //                fmt = "0." + String(decis, "0");
        //            }

        //            aNumberFormats.SetValue(i, fmt);
        //        }

        //        if (uni == scInches)
        //        {
        //uni = scMillimeters;
        //        }
        //        else if (uni == scPounds)
        //        {
        //            uni = sclKilos;
        //        }
        //        else if (uni == scInchesSqr)
        //        {
        //            uni = scCentimetersSqr;
        //        }
        //        else if (uni == scFeet)
        //        {
        //            uni = scMeters;
        //        }
        //        else if (uni == scFeetSqr)
        //        {
        //            uni = scMetersSqr;
        //        }
        //        else if (uni == scLBperHour)
        //        {
        //            uni = scKGperHour;
        //        }
        //        else if (uni == scLBperCubicFeet)
        //        {
        //            uni = scKGperCubicMeter;
        //        }
        //        aUnits.SetValue(i, uni);
        //        DoEvents();
        //    }
        //}



        //private static mzValues xGetHeaders(string aPage, ref int aTable, out mzValues aNumberFormats, out mzValues aUnits, ref bool aOption1)
        //{
        //    mzValues xGetHeaders = null;
        //    mzValues Headers = null;

        //    int i = 0;

        //    string fmt = string.Empty;


        //    Headers = new mzValues();
        //    aUnits = new mzValues();
        //    aNumberFormats = new mzValues();

        //    aPage = aPage.ToUpper().Trim();

        //    switch (aPage)
        //    {
        //        //=========================================
        //        case "EDM1":
        //            //=========================================

        //            mzValues _WithVar_Headers;
        //            _WithVar_Headers = Headers;
        //            _WithVar_Headers.Add("");
        //            _WithVar_Headers.Add("Key Number");
        //            _WithVar_Headers.Add("ID Number");
        //            _WithVar_Headers.Add("Project Number");
        //            _WithVar_Headers.Add("DDM Number");
        //            _WithVar_Headers.Add("Customer");
        //            _WithVar_Headers.Add("City");
        //            _WithVar_Headers.Add("State");
        //            _WithVar_Headers.Add("Country");
        //            _WithVar_Headers.Add("Service Name");
        //            _WithVar_Headers.Add("Item Number");
        //            _WithVar_Headers.Add("Installation Type");
        //            _WithVar_Headers.Add("Revamp Strategy");
        //            _WithVar_Headers.Add("Year");
        //            _WithVar_Headers.Add("Category");
        //            _WithVar_Headers.Add("Contractor");
        //            _WithVar_Headers.Add("Process Licensor");
        //            _WithVar_Headers.Add("Plant Type");
        //            _WithVar_Headers.Add("Type of Process");
        //            _WithVar_Headers.Add("Functional Engineer");
        //            _WithVar_Headers.Add("Number of Columns");
        //            _WithVar_Headers.Add("Diameter");
        //            _WithVar_Headers.Add("Number of Actual Trays");
        //            _WithVar_Headers.Add("Tray Type");
        //            _WithVar_Headers.Add("Tray Spacing");
        //            _WithVar_Headers.Add("Tan-Tan Length");
        //            _WithVar_Headers.Add("Number of Tray Manways");
        //            _WithVar_Headers.Add("Surrogate Rings");
        //            _WithVar_Headers.Add("Comments");
        //            _WithVar_Headers.Add("");
        //            _WithVar_Headers.Add("Operating Pressure");
        //            _WithVar_Headers.Add("Overhead Temperature");
        //            _WithVar_Headers.Add("Overhead Product Purity");
        //            _WithVar_Headers.Add("Ovhd. Product Impurity");
        //            _WithVar_Headers.Add("Bottom Impurity");
        //            _WithVar_Headers.Add("Feed Purity");
        //            _WithVar_Headers.Add("Basis");
        //            _WithVar_Headers.Add("Ovhd. Product Recovery");
        //            _WithVar_Headers.Add("Internal Reflux Ratio");
        //            _WithVar_Headers.Add("Total Reboiler Duty");
        //            _WithVar_Headers.Add("Design Turndown");
        //            Headers = _WithVar_Headers;

        //            mzValues _WithVar_aUnits;
        //            _WithVar_aUnits = aUnits;
        //            _WithVar_aUnits.Add(""); //""
        //            _WithVar_aUnits.Add(""); //"Key Number"
        //            _WithVar_aUnits.Add(""); //"ID Number"
        //            _WithVar_aUnits.Add(""); //"Project Number"
        //            _WithVar_aUnits.Add(""); //"DDM Number"
        //            _WithVar_aUnits.Add(""); //"Customer"
        //            _WithVar_aUnits.Add(""); //"City"
        //            _WithVar_aUnits.Add(""); //"State"
        //            _WithVar_aUnits.Add(""); //"Country"
        //            _WithVar_aUnits.Add(""); //"Service Name"
        //            _WithVar_aUnits.Add(""); //"Item Number"
        //            _WithVar_aUnits.Add(""); //"Installation Type"
        //            _WithVar_aUnits.Add(""); //"Revamp Strategy"
        //            _WithVar_aUnits.Add(""); //"Year"
        //            _WithVar_aUnits.Add(""); //"Category"
        //            _WithVar_aUnits.Add(""); //"Contractor"
        //            _WithVar_aUnits.Add(""); //"Process Licensor"
        //            _WithVar_aUnits.Add(""); //"Plant Type"
        //            _WithVar_aUnits.Add(""); //"Type of Process"
        //            _WithVar_aUnits.Add(""); //"Functional Engineer"
        //            _WithVar_aUnits.Add(""); //"Number of Columns"
        //            _WithVar_aUnits.Add(scInches$); //"Diameter"
        //            _WithVar_aUnits.Add(""); //"Number of Actual Trays"
        //            _WithVar_aUnits.Add(""); //"Tray Type"
        //            _WithVar_aUnits.Add(scInches$); //"Tray Spacing"
        //            _WithVar_aUnits.Add(scFeet$); //"Tan-Tan Length"
        //            _WithVar_aUnits.Add(""); //"Number of Tray Manways"
        //            _WithVar_aUnits.Add(""); //"Surrogate Rings"
        //            _WithVar_aUnits.Add(""); //"Comments"
        //            _WithVar_aUnits.Add(""); //""
        //            _WithVar_aUnits.Add(scPSIa$); //"Operating Pressure"
        //            _WithVar_aUnits.Add(sclDegreesF$); //"Overhead Temperature"
        //            _WithVar_aUnits.Add("Light Key"); //"Overhead Product Purity"
        //            _WithVar_aUnits.Add("Heavy Key"); //"Ovhd. Product Impurity"
        //            _WithVar_aUnits.Add("Light Key"); //"Bottom Impurity"
        //            _WithVar_aUnits.Add("Light Key"); //"Feed Purity"
        //            _WithVar_aUnits.Add(""); //"Basis"
        //            _WithVar_aUnits.Add("%"); //"Ovhd. Product Recovery"
        //            _WithVar_aUnits.Add(""); //"Internal Reflux Ratio"
        //            _WithVar_aUnits.Add("MMBtu/hr"); //"Total Reboiler Duty"
        //            _WithVar_aUnits.Add("%"); //"Design Turndown"

        //            aUnits = _WithVar_aUnits;

        //            for (i = 1; i < aUnits.Count; i++)
        //            {
        //                fmt = aUnits.Item(i);
        //                if (fmt == scInches || fmt == scRhoVSqr || fmt == scPascals)
        //                {
        //                    fmt = "0.0000";
        //                }
        //                else if (fmt == scFeet || fmt == scInchesSqr || fmt == scFeetSqr)
        //                {
        //                    fmt = "0.000";
        //                }
        //                else if (fmt == "%")
        //                {
        //                    fmt = "0.00";
        //                }
        //                else
        //                {
        //                    fmt = string.Empty;
        //                }
        //                aNumberFormats.Add(fmt);
        //                DoEvents();
        //            }

        //            break;
        //    }

        //    if (aPage.Units == uppUnitFamilies.Metric)
        //    {
        //        xConvertUnits(aUnits, aNumberFormats);
        //    }

        //    xGetHeaders = Headers;
        //    return xGetHeaders;
        //}

        private static void xDefineCellWithProperty(ref uopTableCell aCell, uopProperty aProp, uppUnitFamilies aUnits)
        {
            if (aProp == null || aCell == null) return;
            aCell.Value = aProp.UnitVariant(aUnits);
            if (aCell.Numeric) aCell.NumberFormat = aProp.UnitFormatString(aUnits);
            aCell.Locked = aProp.Protected;
        }

        /// <summary>
        /// one page per range
        /// </summary>
        /// <param name="aProject"></param>
        /// <param name="aReport"></param>
        /// <param name="aPage"></param>
        /// <param name="aRanges"></param>
        /// <param name="aTitle"></param>
        /// <param name="rRangePages"></param>
        /// <param name="aDrawingType"></param>
        /// <param name="aPasteAddress"></param>
        /// <param name="aGraphCount"></param>
        /// <param name="aProtected"></param>
        /// <param name="aTemplateTabName"></param>
        private static void xxOnePagePerRange(uopProject aProject, uopDocReport aReport, uopDocReportPage aPage, List<uopTrayRange> aRanges, string aTitle, out List<uopDocReportPage> rRangePages, uppDrawingTypes aDrawingType = uppDrawingTypes.Undefined, string aPasteAddress = "", int aGraphCount = 0, bool aProtected = false, string aTemplateTabName = "")
        {
            rRangePages = new List<uopDocReportPage>();

            for (int i = 0; i < aRanges.Count; i++)
            {
                uopTrayRange aRange = aRanges[i];
                uopDocReportPage cPage = aPage.Clone();
                if (aProtected) cPage.Protected = true;
                cPage.GraphCount = aGraphCount;
                cPage.Range = aRange;
                cPage.TemplateTabName = aTemplateTabName;
                cPage.Title = aTitle;
                cPage.TabName = aTitle + string.Format("({0})", aRange.SpanName());
                if (aDrawingType != uppDrawingTypes.Undefined)
                {
                    uopDocuments collector = new uopDocuments();
                    aRange.GetDrawing(collector, aDrawingType, cPage.DisplayUnits);

                    if (collector.Count > 0)
                    {
                        uopDocDrawing aDWG = (uopDocDrawing)collector.Item(1);
                        aDWG.PasteAddress = aPasteAddress;
                        cPage.Drawings.Add(aDWG);
                    }
                }

                aReport.RequestedPages.Add(cPage);
                rRangePages.Add(cPage);
            }
        }

        private static void xxMDFormatCaseOwnerPages(bool bDistributors, mdProject aProject, uopDocReport aReport, uopDocReportPage aBasePage)
        {
            uopDocReportPage cPage;
            uopParts aOwners;
            List<iCase> dCases;

            uppReportTypes pType = aReport.ReportType;
            int casecnt;
            int tblsperpage;
            if (bDistributors)
            {
                tblsperpage = pType == uppReportTypes.MDSpoutReport ? 2 : 1;
                aOwners = aProject.Distributors;
                dCases = aProject.Distributors.AllCases(true);
                casecnt = aProject.Distributors.MaxCaseCount;
            }
            else
            {
                tblsperpage = 2;
                aOwners = aProject.ChimneyTrays;
                dCases = aProject.ChimneyTrays.AllCases(true);
                casecnt = aProject.ChimneyTrays.MaxCaseCount;
            }
            bool bOnePerPage = aOwners.Count > 10;

            //break the cases into groups of 5 or less for the tables
            int tablecnt = 0;
            List<List<iCase>> caseSets = new List<List<iCase>>();
            for (int i = 1; i <= casecnt; i++)
            {
                List<iCase> cCases;
                if (bDistributors)
                {
                    cCases = aProject.Distributors.GetCasesByIndex(i, ref dCases);
                }
                else
                {
                    cCases = aProject.ChimneyTrays.GetCasesByIndex(i, ref dCases);
                }
                int casetablecnt = 0;
                if (cCases.Count > 0)
                {
                    iCase aCase;

                    List<iCase> tCases = new List<iCase>();
                    tablecnt++;
                    casetablecnt++;

                    caseSets.Add(tCases);
                    for (int j = 0; j < cCases.Count; j++)
                    {
                        aCase = cCases[j];
                        aCase.ProjectHandle = aProject.Handle;
                        if (tCases.Count == 5)
                        {
                            tCases = new List<iCase>();
                            caseSets.Add(tCases);
                            tablecnt++;
                            casetablecnt++;

                        }
                        tCases = caseSets[caseSets.Count - 1];
                        tCases.Add(aCase);
                    }

                    //to prevent laping add empty tables
                    if (bOnePerPage)
                    {
                        int remd = casetablecnt - (casetablecnt / tblsperpage * tblsperpage);
                        while (remd < tblsperpage)
                        {
                            remd++;
                            tCases = new List<iCase>();
                            caseSets.Add(tCases);
                            tablecnt++;
                        }
                    }
                }
            }

            int pagecnt = tablecnt / tblsperpage;
            if (tablecnt % tblsperpage > 0)
            {
                pagecnt++;
            }
            if (pagecnt <= 0) pagecnt = 1;

            //create the pages
            List<uopDocReportPage> oPages = new List<uopDocReportPage>();
            int k = 0;
            for (int i = 0; i < pagecnt; i++)
            {
                cPage = aBasePage.Clone();
                if (bDistributors)
                {
                    cPage.Title = (pType == uppReportTypes.MDDistributorReport) ? "Distributor Details" : "Distributors";

                }
                else
                {
                    cPage.Title = "Chimney Trays";
                }
                cPage.TabName = cPage.Title;
                if (pagecnt > 1)
                {
                    cPage.TabName += string.Format("({0})", i + 1);
                }
                oPages.Add(cPage);
                aReport.RequestedPages.Add(cPage);

                //assign the memebrs to be displayed on the pages
                List<List<iCase>> pCases = new List<List<iCase>>();

                for (int j = 0; j < tblsperpage; j++)
                {
                    if (k < caseSets.Count)
                    {
                        pCases.Add(caseSets[k]);
                    }
                    else
                    {
                        break;
                    }
                    k++;
                }
                cPage.Cases = pCases;
            }
        }

        /// <summary>
        /// executed internally to populate the distributors page of an MD Spout Report
        /// </summary>
        /// <param name="bDistribs"></param>
        /// <param name="aPage"></param>
        private static void xMD_Page_CaseOwners(bool bDistribs, uopDocReportPage aPage)
        {

            uppReportTypes pType;
            pType = aPage.ReportType;

            string titl;
            if (bDistribs)
            {
                titl = (pType == uppReportTypes.MDDistributorReport) ? "Distributor Details" : titl = "Distributors";
            }
            else
            {
                titl = "Chimney Trays";
            }

            if (string.IsNullOrWhiteSpace(aPage.Title)) aPage.Title = titl;

            aPage.Protected = true;
            aPage.Tables = new List<uopTable>();

            List<List<iCase>> pCases = aPage.Cases;
            int iR = 13;
            int tcnt = 0;
            string cNames = string.Empty;

            for (int i = 0; i < pCases.Count; i++)
            {
                List<iCase> aCases = pCases[i];

                if (aCases != null && aCases.Count > 0)
                {
                    iCase aCase = aCases[0];

                    string cname = aCase.Description;
                    if (!cNames.Contains(cname))
                    {
                        if (!string.IsNullOrEmpty(cNames)) cNames += " & ";
                        cNames += cname;
                    }

                    tcnt++;
                    aCase.ProjectHandle = aPage.ProjectHandle;
                    uopProperties Props = aCase.ReportProperties(null, pType);
                    uopTable aTable = aPage.AddTable($"Table{tcnt}");
                    aTable.StartColumn = 4;
                    aTable.StartRow = iR;
                    if (tcnt < 2)
                        iR += Props.Count + 2;
                    else
                        iR += Props.Count + 1;

                    aTable.SetDimensions(Props.Count, 7);
                    aTable.SetLocks(null, new List<int> { 1, 2 }, true);
                    aTable.SetWidth(0, 4);
                    aTable.SetWidth(1, 10);
                    aTable.SetWidth(2, 3);
                    aTable.SetTextFormat(0, 1, mzTextFormats.Bold);
                    aTable.SetTextFormat(1, 0, mzTextFormats.Bold);

                    aTable.SetAlignment(0, 0, uopAlignments.TopCenter);
                    if (bDistribs)
                    {
                        aTable.SetBorders(0, 0, mzSides.Right, mzBorderStyles.Continous, mzBorderWeights.Thin);
                        aTable.SetBorders(null, new List<int> { 1, 2 }, mzSides.Right, mzBorderStyles.Continous, mzBorderWeights.Medium);

                        if (pType == uppReportTypes.MDSpoutReport)
                        {
                            aTable.SetBorders(new List<int> { 1, 5 }, null, mzSides.Bottom, mzBorderStyles.Continous, mzBorderWeights.Medium);
                            aTable.SetBorders(new List<int> { 11, 17, 18 }, null, mzSides.Bottom, mzBorderStyles.Continous, mzBorderWeights.Thin);
                        }
                        else if (pType == uppReportTypes.MDDistributorReport)
                        {
                            aTable.SetBorders(new List<int> { 1, 5 }, null, mzSides.Bottom, mzBorderStyles.Continous, mzBorderWeights.Medium);
                            aTable.SetBorders(new List<int> { 11, 18, 19, 25, 38, 45 }, null, mzSides.Bottom, mzBorderStyles.Continous, mzBorderWeights.Thin);
                        }
                    }
                    else
                    {
                        aTable.SetBorders(0, 0, mzSides.Right, mzBorderStyles.Continous, mzBorderWeights.Thin);
                        aTable.SetBorders(null, new List<int> { 1, 2 }, mzSides.Right, mzBorderStyles.Continous, mzBorderWeights.Medium);
                        aTable.SetBorders(new List<int> { 1, 6 }, null, mzSides.Bottom, mzBorderStyles.Continous, mzBorderWeights.Medium);
                        aTable.SetBorders(13, 0, mzSides.Bottom, mzBorderStyles.Continous, mzBorderWeights.Thin);
                    }

                    aTable.BordersAround(mzBorderStyles.Continous, mzBorderWeights.Medium);

                    for (int j = 1; j <= aCases.Count; j++)
                    {
                        aCase = aCases[j - 1];
                        if (j > 0)
                        {
                            Props = aCase.ReportProperties(null, pType);
                        }
                        if (j == 1)
                        {
                            aTable.SetByCollection(1, true, Props.Names);
                            // aTable.ReplaceCellValues("Place Holder", string.Empty, false);
                            aTable.SetByCollection(2, true, Props.UnitCaptions(aPage.Units));
                            aTable.Cell(1, 2).Value = "Units";
                            aTable.SetAlignment(0, 1, uopAlignments.MiddleLeft);
                            aTable.Cell(1, 1).Alignment = uopAlignments.MiddleCenter;
                        }

                        //Collection Values = null;
                        for (int k = 1; k <= Props.Count; k++)
                        {
                            uopProperty aProp = Props.Item(k);
                            if (k == 1)
                            {
                                aProp.Value = Convert.ToString(aCase.OwnerIndex);
                            }
                            uopTableCell aCell = aTable.Cell(k, 2 + j);
                            aCell.Property = aProp.Clone();


                        }
                    }

                }
            }

            aPage.SubTitle1 = cNames;

        }

        static void xMD_Page_FeedZones(uopDocReportPage aPage)
        {
            //throw new NotImplementedException();
            //^executed internally to populate the feed zones page

            mdTrayRange aRange = (mdTrayRange)aPage.Range;

            mdTrayAssembly aAssy = aRange.TrayAssembly;
            List<mdFeedZone> Zones = aAssy.DeckPanels.FeedZones(aAssy);
            int si = aPage.StartIndex;
            int ei = aPage.EndIndex;
            int cnt = ei - si + 1;

            uopTable aTable = aPage.AddTable("Feed Zones", 32, 10);

            uopUnit area = uopUnits.GetUnit(uppUnitTypes.SmallArea);
            string arealabel = (aPage.Units == uppUnitFamilies.English) ? area.EnglishLabel : area.MetricLabel;


            aTable.SetDimensions(cnt + 3, 6, "4,4,4,4,4,4");

            aTable.SetTextFormat("1,2", "All", mzTextFormats.Bold);
            aTable.SetBorders("2,3", "All", mzSides.Bottom, mzBorderStyles.Continous, mzBorderWeights.Medium);
            aTable.SetBorders("All", "All", mzSides.Right, mzBorderStyles.Continous, mzBorderWeights.Thin);
            aTable.BordersAround(mzBorderStyles.Continous, mzBorderWeights.Medium);
            aTable.SetAlignment("All", "All", uopAlignments.TopCenter);
            aTable.SetByString(1, false, "Zone|Zone|FBA|Spout|Sa|Ratio", aDelimitor: "|");
            aTable.SetByString(2, false, "|FBA|Pct.|Area (Sa)|Pct.|FBA/Sa", aDelimitor: "|");


            aTable.SetByString(3, false, $"|{arealabel}|%|{arealabel}|%|", aDelimitor: "|");
            double multi = (aPage.Units == uppUnitFamilies.English) ? 1 : area.ConversionFactor(uppUnitFamilies.Metric);

            double FBATot = aAssy.FreeBubblingAreas.TotalFreeBubblingArea * multi;
            double TotSpoutArea = aAssy.TotalSpoutArea * multi;

            int j = 0;
            int k = 1;
            uopTableCell aCell;
            for (int i = si; i < ei; i++)
            {
                j++;
                k = j + 3;

                mdFeedZone aZone = Zones[i - 1];
                mdSpoutGroup sGroup = aZone.SpoutGroup(aAssy);

                double FBA = aZone.ZoneFBA * multi;
                double sArea = 0;
                double ratio = 0;
                double saPct = 0;
                double ocFact = aZone.OccuranceFactor;
                if (sGroup != null)
                {
                    //ocFact = sGroup.OccuranceFactor;
                    sArea = sGroup.ActualArea * ocFact * multi;
                }

                if (sArea > 0) ratio = FBA / sArea;


                if (TotSpoutArea > 0) saPct = sArea / TotSpoutArea * 100;


                double avg = ratio * (FBA / FBATot);

                aCell = aTable.Cell(k, 1);
                aCell.Property = new uopProperty("Handle", aZone.Handle);
                aCell.Numeric = false;
                aCell.NumberFormat = string.Empty;
                aCell = aTable.Cell(k, 2);
                aCell.Property = new uopProperty("FBA", FBA / ocFact, uppUnitTypes.SmallArea);
                aCell.NumberFormat = "#,0.000";

                aCell = aTable.Cell(k, 3);
                aCell.Property = new uopProperty("FBAPercentage", aZone.FBAPercentage / ocFact, uppUnitTypes.Percentage);
                aCell.NumberFormat = "0.00";

                aCell = aTable.Cell(k, 4);
                aCell.Property = new uopProperty("SGArea", sArea / ocFact, uppUnitTypes.SmallArea);
                aCell.NumberFormat = "#,0.000";

                aCell = aTable.Cell(k, 5);
                aCell.Property = new uopProperty("SGAreaPCT", saPct / ocFact, uppUnitTypes.Percentage);
                aCell.NumberFormat = "0.00";

                aCell = aTable.Cell(k, 6);
                aCell.Property = new uopProperty("SGAreaRatio", ratio);

                aCell.NumberFormat = "#,0.000";

            }

            uopTable bTable = aPage.AddTable("Totals", aTable.StartRow + aTable.Rows + 1, aTable.StartColumn);

            bTable.AddByString(true, "Total Tray FBA|Total Tray Spout Area|Overall Ratio");
            bTable.AddByString(true, arealabel + "|" + arealabel + "|");

            bTable.SetDimensions(3, 3, "7,5,2");

            aCell = bTable.Cell(1, 2);
            aCell.Property = new uopProperty("FBATot", FBATot, uppUnitTypes.SmallArea);
            aCell.Alignment = uopAlignments.TopRight;
            aCell.NumberFormat = "#,0.000";

            aCell = bTable.Cell(2, 2);
            aCell.Alignment = uopAlignments.TopRight;
            aCell.Property = new uopProperty("TotSpoutArea", TotSpoutArea, uppUnitTypes.SmallArea);

            aCell.NumberFormat = "#,0.000";

            aCell = bTable.Cell(3, 2);
            aCell.Alignment = uopAlignments.TopRight;
            aCell.Property = new uopProperty("FBARatio", 0d);
            if (TotSpoutArea != 0) aCell.Value = FBATot / TotSpoutArea;

            aCell.NumberFormat = "#,0.000";


        }

        private static void xMD_Page_EDM1(uopDocReportPage aPage)
        {

            //^executed internally to populate the First Page of an  MD DDM Report
            if (aPage.Project == null) return;
            mdProject project = (mdProject)aPage.Project;

            string hdrVal;

            double multi1 = (aPage.Units != uppUnitFamilies.English) ? 25.4d : 1;

            uopTable aTable = aPage.AddTable("Data", 13, 4);
            aPage.Title = "Tray General And Process Information";
            aPage.TabName = aPage.Title;
            List<string> Headers = new List<string>
            {
                "",
                "Key Number",
                "ID Number",
                "Project Number",
                "DDM Number",
                "Customer",
                "City",
                "State",
                "Country",
                "Service Name",
                "Item Number",
                "Installation Type",
                "Revamp Strategy",
                "Year",
                "Category",
                "Contractor",
                "Process Licensor",
                "Plant Type",
                "Type of Process",
                "Functional Engineer",
                "Number of Columns",
                "Diameter",
                "Number of Actual Trays",
                "Tray Type",
                "Tray Spacing",
                "Tan-Tan Length",
                "Number of Tray Manways",
                "Surrogate Rings",
                "Comments",
                "",
                "Operating Pressure",
                "Overhead Temperature",
                "Overhead Product Purity",
                "Ovhd. Product Impurity",
                "Bottom Impurity",
                "Feed Purity",
                "Basis",
                "Ovhd. Product Recovery",
                "Internal Reflux Ratio",
                "Total Reboiler Duty",
                "Design Turndown"
            };

            List<uopAlignments> algns = new List<uopAlignments> { uopAlignments.MiddleLeft, uopAlignments.MiddleCenter, uopAlignments.MiddleCenter };
            List<mzTextFormats> fmats = new List<mzTextFormats> { mzTextFormats.Bold, mzTextFormats.None, mzTextFormats.None };
            List<double> colwidths = new List<double> { 9, 4, 23 };
            for (int i = 1; i <= Headers.Count; i++)
            {
                hdrVal = Headers[i - 1];
                List<string> rowvals = new List<string> { hdrVal };

                switch (hdrVal.ToUpper())
                {
                    case "KEY NUMBER":
                        rowvals.Add(""); //units
                        rowvals.Add(project.KeyNumber);

                        break;
                    case "ID NUMBER":
                        rowvals.Add(""); //units
                        rowvals.Add(project.IDNumber);
                        break;
                    case "CUSTOMER":
                        rowvals.Add(""); //units
                        rowvals.Add(project.Customer.Name);
                        break;
                    case "SERVICE NAME":
                        rowvals.Add(""); //units
                        rowvals.Add(project.Customer.Service);
                        break;
                    case "ITEM NUMBER":
                        rowvals.Add(""); //units
                        rowvals.Add(project.Customer.Item);
                        break;
                    case "CONTRACTOR":
                        rowvals.Add(""); //units
                        rowvals.Add(project.Contractor);
                        break;
                    case "PROCESS LICENSOR":
                        rowvals.Add(""); //units
                        rowvals.Add(project.ProcessLicensor);
                        break;
                    case "FUNCTIONAL ENGINEER":
                        rowvals.Add(""); //units
                        rowvals.Add(""); //goUser.NiceName
                        break;
                    case "NUMBER OF COLUMNS":
                        rowvals.Add(""); //units
                        rowvals.Add("1");
                        break;
                    case "NUMBER OF ACTUAL TRAYS":
                        rowvals.Add(""); //units
                        rowvals.Add(project.TotalTrayCount.ToString());
                        break;
                    case "TRAY TYPE":
                        rowvals.Add(""); //units
                        rowvals.Add(project.TrayRanges.TrayTypeNames);
                        break;
                    case "DIAMETER":
                        rowvals.Add(uopUnits.GetUnitLabel(uppUnitTypes.SmallLength, aPage.Units)); //units
                        rowvals.Add(project.TrayRanges.ShellIDList(multi1));
                        break;
                    case "TRAY SPACING":
                        rowvals.Add(uopUnits.GetUnitLabel(uppUnitTypes.SmallLength, aPage.Units)); //units
                        rowvals.Add(project.TrayRanges.TraySpacingList(multi1));
                        break;
                    case "TAN-TAN LENGTH":
                        rowvals.Add(uopUnits.GetUnitLabel(uppUnitTypes.BigLength, aPage.Units)); //units
                        rowvals.Add("");

                        break;
                    case "BASIS":
                        rowvals.Add(""); //units
                        rowvals.Add("mass                mole                volume              not specified");

                        break;
                    case "NUMBER OF TRAY MANWAYS":
                        rowvals.Add(""); //units
                        rowvals.Add(project.TrayRanges.ManwayCountList);
                        break;
                    case "OVERHEAD TEMPERATURE":
                        rowvals.Add(uopUnits.GetUnitLabel(uppUnitTypes.Temperature, aPage.Units)); //units
                        rowvals.Add("");
                        break;
                    case "OPERATING PRESSURE":
                        rowvals.Add(uopUnits.GetUnitLabel(uppUnitTypes.Pressure, aPage.Units)); //units
                        rowvals.Add("");
                        break;
                    case "BOTTOM IMPURITY":
                    case "FEED PURITY":
                    case "OVERHEAD PRODUCT PURITY":
                        rowvals.Add("Light Key"); //units
                        rowvals.Add("");
                        break;
                    case "OVHD. PRODUCT IMPURITY":
                        rowvals.Add("Heavy Key"); //units
                        rowvals.Add("");
                        break;
                    case "DESIGN TURNDOWN":
                    case "OVHD. PRODUCT RECOVERY":
                        rowvals.Add("%"); //units
                        rowvals.Add("");
                        break;
                    case "TOTAL REBOILER DUTY":
                        rowvals.Add("MMBtu/hr"); //units
                        rowvals.Add("");
                        break;
                    default:
                        rowvals.Add(""); //units
                        rowvals.Add("");
                        break;
                }
                if (i == 1)
                    aTable.AddRow(rowvals, aAlignments: algns, aTextFormats: new List<mzTextFormats> { mzTextFormats.Bold, mzTextFormats.Bold, mzTextFormats.Bold }, aCellWidths: colwidths);
                else
                    aTable.AddRow(rowvals, aAlignments: algns, aTextFormats: fmats, aCellWidths: colwidths);
            }


            aTable.SetBorders(0, 0, mzSides.Right, mzBorderStyles.Continous, mzBorderWeights.Medium);
            aTable.SetBorders(new List<int> { 1, 14, 30 }, null, mzSides.Bottom, mzBorderStyles.Continous, mzBorderWeights.Medium);
            aTable.SetBorders(28, 0, mzSides.Bottom, mzBorderStyles.Continous, mzBorderWeights.Thin);
            aTable.BordersAround(mzBorderStyles.Continous, mzBorderWeights.Medium);


        }

        private static void xMD_Page_EDM2(uopDocReportPage aPage)
        {

            //^executed internally to populate the First Page of an  MD DDM Report
            if (aPage.Project == null) return;
            mdProject project = (mdProject)aPage.Project;

            int si = aPage.StartIndex;
            int ei = aPage.EndIndex;
            List<uopTrayRange> aRanges = project.TrayRanges.GetRanges(si, ei);
            mdTrayRange aRange;


            double multi1 = (aPage.Units != uppUnitFamilies.English) ? 25.4d : 1;

            uopTable aTable = aPage.AddTable("Data", 13, 4);
            aPage.Title = "Tray Mechanical Summary";

            aPage.TabName = aPage.Title;


            uopPropertyArray propa = new uopPropertyArray();
            uopProperties headers = new uopProperties("Headers")
            {
                new uopProperty("TopRow", ""),
                new uopProperty("Tray Numbers", "Tray Numbers"),
                new uopProperty("Tray Count", "Tray Count"),
                new uopProperty("Tray Type", "Tray Type"),
                new uopProperty("Number of Downcomers", "Number of Downcomers"),
                new uopProperty("Downcomer Type", "Downcomer Type"),
                new uopProperty("Column Inner Diameter", "Column Inner Diameter"),
                new uopProperty("Panel Width", "Panel Width"),
                new uopProperty("Weir Height", "Weir Height"),
                new uopProperty("Downcomer Width", "Downcomer Width"),
                new uopProperty("Downcomer Height", "Downcomer Height"),
                new uopProperty("Tray Spacing", "Tray Spacing"),
                new uopProperty("CDP Height", "CDP Height"),
                new uopProperty("Spout Area", "Spout Area"),
                new uopProperty("Deck Perforation Fraction", "Deck Perforation Fraction"),
                new uopProperty("Perforation Diameter", "Perforation Diameter"),
                new uopProperty("Fraction Slotting", "Fraction Slotting"),
                new uopProperty("Slot Type", "Slot Type"),
                new uopProperty("Additional Device", "Additional Device"),
                new uopProperty("Height Above Tray Deck", "Height Above Tray Deck")
            };
            propa.Add(headers, "Headers");



            uopProperties units = new uopProperties("Units")
            {
                new uopProperty("TopRow", "Units"),
                new uopProperty("Tray Numbers", ""),
                new uopProperty("Tray Count", 0),
                new uopProperty("Tray Count", 0),
                new uopProperty("Number of Downcomers", 0),
                new uopProperty("Downcomer Type", ""),
                new uopProperty("Column Inner Diameter", 0, uppUnitTypes.SmallLength),
                new uopProperty("Panel Width", 0, uppUnitTypes.SmallLength),
                new uopProperty("Weir Height", 0, uppUnitTypes.SmallLength),
                new uopProperty("Downcomer Width", 0, uppUnitTypes.SmallLength),
                new uopProperty("Downcomer Height", 0, uppUnitTypes.SmallLength),
                new uopProperty("Tray Spacing", 0, uppUnitTypes.SmallLength),
                new uopProperty("CDP Height", 0, uppUnitTypes.SmallLength),
                new uopProperty("Spout Area", 0, uppUnitTypes.SmallArea),
                new uopProperty("Deck Perforation Fraction", 0, uppUnitTypes.Percentage),
                new uopProperty("Perforation Diameter", 0, uppUnitTypes.SmallLength),
                new uopProperty("Fraction Slotting", 0),
                new uopProperty("Slot Type", ""),
                new uopProperty("Additional Device", ""),
                new uopProperty("Height Above Tray Deck", 0, uppUnitTypes.SmallLength)
            };
            units.DisplayUnits = aPage.Units;
            uopProperties props = new uopProperties("Units");
            foreach (var item in units)
            {
                props.Add(new uopProperty(item.Name, item.DisplayUnitLabel));
            }
            propa.Add(props, "Units");

            List<uopAlignments> algns = new List<uopAlignments> { uopAlignments.MiddleLeft, uopAlignments.MiddleCenter };
            List<mzTextFormats> fmats = new List<mzTextFormats> { mzTextFormats.Bold, mzTextFormats.None };
            List<double> colwidths = new List<double> { 9, 4, 4, 4, 4, 4, 4, 4 };


            for (int t = 1; t <= 6; t++)
            {
                props = new uopProperties();

                aRange = t <= aRanges.Count ? (mdTrayRange)aRanges[t - 1] : null;
                if (aRange == null)
                {
                    props = headers.Clone();
                    foreach (var item in props) { item.SetValue(""); }
                }
                else
                {
                    props = aRange.ReportProperties(2);
                }
                props.Item(1).SetValue(t);
                propa.Add(props, $"RANGE_{t}");
            }

            for (int i = 1; i <= headers.Count; i++)
            {

                List<uopProperty> row = new List<uopProperty>();
                foreach (var item in propa)
                {
                    row.Add(item.Item(i));
                }
                aTable.AddRow(row, aAlignments: algns, aTextFormats: fmats, aCellWidths: colwidths);
            }





            aTable.SetBorders(0, 0, mzSides.Right, mzBorderStyles.Continous, mzBorderWeights.Medium);
            aTable.SetBorders(1, 0, mzSides.Bottom, mzBorderStyles.Continous, mzBorderWeights.Medium);
            aTable.BordersAround(mzBorderStyles.Continous, mzBorderWeights.Medium);


        }

        private static void xMD_Page_EDM3(uopDocReportPage aPage)
        {

            //^executed internally to populate the First Page of an  MD DDM Report
            if (aPage.Project == null) return;
            mdProject project = (mdProject)aPage.Project;

            int si = aPage.StartIndex;
            int ei = aPage.EndIndex;
            List<uopTrayRange> aRanges = project.TrayRanges.GetRanges(si, ei);
            mdTrayRange aRange;


            double multi1 = (aPage.Units != uppUnitFamilies.English) ? 25.4d : 1;

            uopTable aTable = aPage.AddTable("Data", 13, 4);
            aPage.Title = "Tray Mechanical Summary";

            aPage.TabName = aPage.Title;


            uopPropertyArray propa = new uopPropertyArray();
            uopProperties headers = new uopProperties("Headers")
            {
                new uopProperty(@"Top Row", @""),
                new uopProperty(@"Tray Numbers", @"Tray Numbers"),
                new uopProperty(@"Weir Load (Ql/Bw)", @"Weir Load (Ql/Bw)"),
                new uopProperty(@"Weir Load Max", @"Weir Load Max"),
                new uopProperty(@"F-factor", @"F-factor"),
                new uopProperty(@"F-factor Max", @"F-factor Max"),
                new uopProperty(@"Froth Height", @"Froth Height"),
                new uopProperty(@"ECMD Froth Height", @"ECMD Froth Height"),
                new uopProperty(@"HfoVs", @"HfoVs"),
                new uopProperty(@"Margin of Safety", @"Margin of Safety"),
                new uopProperty(@"Z Height", @"Z Height"),
                new uopProperty(@"Entrainment", @"Entrainment"),
                new uopProperty(@"Downcomer Velocity", @"Downcomer Velocity"),
                new uopProperty(@"Tray Pressure Drop", @"Tray Pressure Drop"),
                new uopProperty(@"Liquid Density", @"Liquid Density"),
                new uopProperty(@"Vapor Density", @"Vapor Density"),
                new uopProperty(@"Density Difference", @"Density Difference"),
                new uopProperty(@"Surface Tension", @"Surface Tension"),
                new uopProperty(@"Applied Efficiency", @"Applied Efficiency"),
                new uopProperty(@"Calculated Efficiency", @"Calculated Efficiency"),
                new uopProperty(@"System Factor", @"System Factor"),
                new uopProperty(@"Alpha D", @"Alpha D"),
                new uopProperty(@"Alpha R", @"Alpha R"),
                new uopProperty(@"Stability Factor", @"Stability Factor"),
                new uopProperty(@"Fluidization Factor", @"Fluidization Factor"),
                new uopProperty(@"Stability @ Turndown", @"Stability @ Turndown"),
                new uopProperty(@"Spout Loss @ Turndown", @"Spout Loss @ Turndown"),
                new uopProperty(@"Weep @ Turndown", @"Weep @ Turndown")
            };

            propa.Add(headers, "Headers");



            uopProperties units = new uopProperties("Units")
            {
                new uopProperty(@"Top Row", @"Units"),
                new uopProperty(@"Tray Numbers", @""),
                new uopProperty(@"Weir Load (Ql/Bw)", @"CFS/ft"),
                new uopProperty(@"Weir Load Max", @"CFS/ft"),
                new uopProperty(@"F-factor", 0, uppUnitTypes.Velocity),
                new uopProperty(@"F-factor Max", 0, uppUnitTypes.Velocity),
                new uopProperty(@"Froth Height", 0, uppUnitTypes.SmallLength),
                new uopProperty(@"ECMD Froth Height", 0, uppUnitTypes.SmallLength),
                new uopProperty(@"HfoVs", @""),
                new uopProperty(@"Margin of Safety", 0, uppUnitTypes.SmallLength),
                new uopProperty(@"Z Height", 0, uppUnitTypes.SmallLength),
                new uopProperty(@"Entrainment", 0, uppUnitTypes.Percentage),
                new uopProperty(@"Downcomer Velocity", @"GPM/ft²"),
                new uopProperty(@"Tray Pressure Drop", 0, uppUnitTypes.Pressure),
                new uopProperty(@"Liquid Density", 0, uppUnitTypes.Density),
                new uopProperty(@"Vapor Density", 0, uppUnitTypes.Density),
                new uopProperty(@"Density Difference", 0, uppUnitTypes.Density),
                new uopProperty(@"Surface Tension", 0, uppUnitTypes.SurfaceTension),
                new uopProperty(@"Applied Efficiency", 0, uppUnitTypes.Percentage),
                new uopProperty(@"Calculated Efficiency", 0, uppUnitTypes.Percentage),
                new uopProperty(@"System Factor", @""),
                new uopProperty(@"Alpha D", @""),
                new uopProperty(@"Alpha R", @""),
                new uopProperty(@"Stability Factor", @""),
                new uopProperty(@"Fluidization Factor", @""),
                new uopProperty(@"Stability @ Turndown", @""),
                new uopProperty(@"Spout Loss @ Turndown", 0, uppUnitTypes.SmallLength),
                new uopProperty(@"Weep @ Turndown", 0, uppUnitTypes.Percentage)
            };

            units.DisplayUnits = aPage.Units;
            uopProperties props = new uopProperties("Units");
            foreach (var item in units)
            {
                if (item.HasUnits)
                    props.Add(new uopProperty(item.Name, item.DisplayUnitLabel));
                else
                    props.Add(item.Clone());
            }
            propa.Add(props, "Units");

            List<uopAlignments> algns = new List<uopAlignments> { uopAlignments.MiddleLeft, uopAlignments.MiddleCenter };
            List<mzTextFormats> fmats = new List<mzTextFormats> { mzTextFormats.Bold, mzTextFormats.None };
            List<double> colwidths = new List<double> { 9, 3, 4, 4, 4, 4, 4, 4 };


            for (int t = 1; t <= 6; t++)
            {
                props = new uopProperties();

                aRange = t <= aRanges.Count ? (mdTrayRange)aRanges[t - 1] : null;
                if (aRange == null)
                {
                    props = headers.Clone();
                    foreach (var item in props) { item.SetValue(""); }
                }
                else
                {
                    props = aRange.ReportProperties(3);
                }
                props.Item(1).SetValue(t);
                propa.Add(props, $"RANGE_{t}");
            }

            for (int i = 1; i <= headers.Count; i++)
            {

                List<uopProperty> row = new List<uopProperty>();
                foreach (var item in propa)
                {
                    row.Add(item.Item(i));
                }
                aTable.AddRow(row, aAlignments: algns, aTextFormats: fmats, aCellWidths: colwidths);
            }





            aTable.SetBorders(0, 0, mzSides.Right, mzBorderStyles.Continous, mzBorderWeights.Medium);
            aTable.SetBorders(1, 0, mzSides.Bottom, mzBorderStyles.Continous, mzBorderWeights.Medium);
            aTable.BordersAround(mzBorderStyles.Continous, mzBorderWeights.Medium);


        }

        private static void xMD_Page_DCStress(uopDocReportPage aPage)
        {
            mdTrayRange aRange = (mdTrayRange)aPage.Range;
            mdProject project = aRange.GetMDProject();
            if (aRange == null || project == null) return;
            mdTrayAssembly aAssy = aRange.TrayAssembly;


            mdDowncomerBox dcbox = aPage.Part as mdDowncomerBox;
            double ShellID = aRange.ShellID;
            bool ecmd = aAssy.DesignFamily.IsEcmdDesignFamily();
            int iR = 0;

            aPage.Protected = true;

            uopTable aTable = aPage.AddTable("Input Data");

            aTable.SetCellValue(++iR, 1, aPage.Units == uppUnitFamilies.Metric ? 1 : 0, "MetricFlag", bNumeric: true, bDataOnly: true);
            aTable.SetCellValue(++iR, 1, aRange.DesignFamily.IsEcmdDesignFamily() ? 1 : 0, "ECMDFlag", bNumeric: true, bDataOnly: true);
            string sval = project.ColumnName;
            if (String.IsNullOrWhiteSpace(sval)) sval = project.Name;
            aTable.SetCellValue(++iR, 1, sval, "Project_Name", bNumeric: false);
            aTable.SetCellValue(++iR, 1, $"{aRange.Name(true)} - Downcomer {aPage.PartIndex}", "Downcomer_Name", bNumeric: false);
            aTable.SetCellValue(++iR, 1, 10, "BottomPC", bNumeric: true, bDataOnly: true);



            if (ShellID > 216)
            {
                aTable.SetCellValue(++iR, 1, 750, "Load_Conc", bNumeric: true, bDataOnly: true);
            }
            else if (ShellID > 120 & ShellID <= 216)
            {
                aTable.SetCellValue(++iR, 1, 500, "Load_Conc", bNumeric: true, bDataOnly: true);
            }
            else
            {
                aTable.SetCellValue(++iR, 1, 250, "Load_Conc", bNumeric: true, bDataOnly: true);
            }

            if (dcbox == null) return;

            mdDowncomer pardc = aAssy.Downcomer(dcbox.Index);
            aTable.SetCellValue(++iR, 1, dcbox.Height - dcbox.Thickness, "DCHeight", bNumeric: true, bDataOnly: true);
            aTable.SetCellValue(++iR, 1, dcbox.Width, "DCWidth", bNumeric: true, bDataOnly: true);
            aTable.SetCellValue(++iR, 1, dcbox.How, "WeirHt", bNumeric: true, bDataOnly: true);
            aTable.SetCellValue(++iR, 1, dcbox.Thickness, "DCThk", bNumeric: true, bDataOnly: true);

            aTable.SetCellValue(++iR, 1, dcbox.LongAssemblyLength, "DCLength", bNumeric: true, bDataOnly: true);
            aTable.SetCellValue(++iR, 1, dcbox.LongLength, "BoxLength", bNumeric: true, bDataOnly: true);
            aTable.SetCellValue(++iR, 1, aAssy.DowncomerSpacing, "DCSpace", bNumeric: true, bDataOnly: true);
            aTable.SetCellValue(++iR, 1, aRange.RingWidth, "RingWd", bNumeric: true, bDataOnly: true);
            aTable.SetCellValue(++iR, 1, aAssy.Deck.Thickness, "DKThk", bNumeric: true, bDataOnly: true);
            aTable.SetCellValue(++iR, 1, aRange.ManholeID, "ManholeID", bNumeric: true, bDataOnly: true);


            aTable.SetCellValue(++iR, 1, dcbox.FoldOverHeight, "FldOvr", bNumeric: true, bDataOnly: true);
            aTable.SetCellValue(++iR, 1, ShellID, "COLUMNID", bNumeric: true, bDataOnly: true);
            aTable.SetCellValue(++iR, 1, dcbox.SupplementalDeflectorHeight, "SupDefHt", bNumeric: true, bDataOnly: true);
            aTable.SetCellValue(++iR, 1, ecmd ? aAssy.BaffleHeight : 0, "DefHt", bNumeric: true, bDataOnly: true);
            aTable.SetCellValue(++iR, 1, ecmd ? aAssy.GetSheetMetal(false).Thickness : 0, "DefThk", bNumeric: true, bDataOnly: true);

        }

        /// <summary>
        ///#1the project to get the data from
        ///#2the page to populate
        ///^defines the tables of the passed page based on the data in the project
        /// </summary>
        /// <param name="aPage"></param>
        private static void xMD_Page_ProjectSummary(uopDocReportPage aPage)
        {
            // string ADataString = "Column Inner Diameter||Total MD Trays|Total ECMD Trays|Additional Devices||Liquid Distributors|Vapor Distributors|Mixed Phase Distributors||Chimney Trays||Manhole Inner Diameter|Installation Type||Spouting Dimensions";
            List<string> headings = mzUtils.StringsFromDelimitedList("Additional Devices||Liquid Distributors|Vapor Distributors|Mixed Phase Distributors||Chimney Trays||Manhole Inner Diameter|Installation Type||Spouting Dimensions", "|", true);
            aPage.PageName = "Column Summary";
            aPage.Title = aPage.PageName;
            aPage.Tables = new List<uopTable>();

            uopTable aTable = aPage.AddTable("Summary", 14, 5);



            //aTable.Cell(1, 3).Value = uopUnits.UnitLabel(uppUnitTypes.BigLength, aPage.Units);
            //aTable.Cell(aTable.Rows - 3, 3).Value = uopUnits.UnitLabel(uppUnitTypes.SmallLength, aPage.Units);


            List<uppMDDesigns> types = _MDProject.TrayRanges.DesignFamilies(true);
            List<string> lables = new List<string> { "Column Inner Diameter" };
            foreach (uppMDDesigns item in types)
            {
                lables.Add($"Total {item.GetDescription()} Trays");
            }
            lables.AddRange(headings);


            aTable.AddColumn(lables, aAlignments: new List<uopAlignments> { uopAlignments.MiddleLeft }, aTextFormats: new List<mzTextFormats> { mzTextFormats.Bold });

            // second column
            List<string> bvals = new List<string>();

            foreach (string item in lables)
            {

                if (string.Compare(item, "column inner diameter", true) == 0)
                {
                    bvals.Add(uopUnits.UnitLabel(uppUnitTypes.BigLength, aPage.Units));
                    continue;
                }
                else if (string.Compare(item, "manhole inner diameter", true) == 0)
                {
                    bvals.Add(uopUnits.UnitLabel(uppUnitTypes.SmallLength, aPage.Units));
                    continue;
                }
                bvals.Add("");

            }
            aTable.SetColWidths(new List<double> { 9 });

            aTable.AddColumn(bvals, aAlignments: new List<uopAlignments> { uopAlignments.MiddleCenter });

            List<List<uopTrayRange>> UniqueRanges = _MDProject.GetUniqueRanges();


            // third column
            uopTableCell aCell;
            string aStr;
            mdTrayRange Range;
            uopDocWarning aWrn;
            uopProperty prop;
            List<uopProperty> colvals;

            string fmat = (aPage.Units == uppUnitFamilies.English) ? mzUtils.NumberFormatString(2, true) : mzUtils.NumberFormatString(1, true);
            List<string> fmats = new List<string> { fmat };

            List<uopAlignments> algns = new List<uopAlignments> { uopAlignments.MiddleCenter };

            List<uopTableCell> col;
            for (int i = 0; i < UniqueRanges.Count; i++)
            {
                List<uopTrayRange> Ranges = UniqueRanges[i];
                Range = (mdTrayRange)Ranges[0];
                if (Range != null)
                {
                    int cnt = colUOPTrayRanges.TablulateTrayCount(Ranges, Range.DesignFamily);
                    colvals = new List<uopProperty> { new uopProperty("Shell ID", Range.ShellID / 12, uppUnitTypes.BigLength) { DisplayUnits = aPage.Units } };
                    fmat = string.Empty;
                    for (int j = 2; j <= lables.Count; j++)
                    {
                        uopAlignments algnm = uopAlignments.MiddleCenter;
                        aStr = lables[j - 1];
                        prop = new uopProperty($"PROP_{j}", "");
                        if (!string.IsNullOrWhiteSpace(aStr))
                        {
                            prop.Name = aStr;
                            if (aStr.ToLower().StartsWith("total "))
                            {
                                if (string.Compare(aStr, $"Total {Range.DesignFamilyName} Trays", true) == 0)
                                {
                                    prop.SetValue(cnt);
                                }
                                else
                                {
                                    prop.SetValue("-");
                                }
                            }
                            else
                            {
                                if (string.Compare(aStr, "Additional Devices", true) == 0)
                                {
                                    prop.SetValue("-");
                                    if (Range.TrayAssembly.HasFlowEnhancement) prop.SetValue("FED");
                                    if (Range.TrayAssembly.HasAntiPenetrationPans) prop.SetValue("APP");

                                }
                                else if (string.Compare(aStr, "Manhole Inner Diameter", true) == 0)
                                {
                                    prop = new uopProperty(aStr, Range.ManholeID, uppUnitTypes.SmallLength) { DisplayUnits = aPage.Units };
                                    fmat = (aPage.Units == uppUnitFamilies.English) ? mzUtils.NumberFormatString(3, true) : mzUtils.NumberFormatString(1, true);
                                }

                            }

                            if (i == 0)
                            {
                                if (string.Compare(aStr, "Liquid Distributors", true) == 0)
                                {
                                    cnt = _MDProject.Distributors.LiquidDistributors().Count;
                                    if (cnt <= 0)
                                        prop.SetValue("-");
                                    else
                                        prop.SetValue(cnt);
                                    fmat = mzUtils.NumberFormatString(0, true);
                                }
                                else if (string.Compare(aStr, "Vapor Distributors", true) == 0)
                                {
                                    cnt = _MDProject.Distributors.VaporDistributors().Count;
                                    if (cnt <= 0)
                                        prop.SetValue("-");
                                    else
                                        prop.SetValue(cnt);
                                    fmat = mzUtils.NumberFormatString(0, true);
                                }
                                else if (string.Compare(aStr, "Mixed Phase Distributors", true) == 0)
                                {
                                    cnt = _MDProject.Distributors.MixedPhaseDistributors().Count;
                                    if (cnt <= 0)
                                        prop.SetValue("-");
                                    else
                                        prop.SetValue(cnt);
                                    fmat = mzUtils.NumberFormatString(0, true);
                                }
                                else if (string.Compare(aStr, "Chimney Trays", true) == 0)
                                {
                                    cnt = _MDProject.ChimneyTrays.Count;
                                    prop.SetValue(cnt <= 0 ? "-" : cnt.ToString());

                                }
                                else if (string.Compare(aStr, "Installation Type", true) == 0)
                                {
                                    algnm = uopAlignments.MiddleLeft;
                                    prop.SetValue(_MDProject.InstallationType.GetDescription());
                                }
                                else if (string.Compare(aStr, "Spouting Dimensions", true) == 0)
                                {
                                    algnm = uopAlignments.MiddleLeft;
                                    prop.SetValue(_MDProject.MetricSpouting ? "Metric" : "English");
                                }
                            }

                        }

                        fmats.Add(fmat);
                        algns.Add(algnm);


                        colvals.Add(prop);

                    }

                    col = aTable.AddColumn(colvals, aAlignments: algns, aNumbersFormats: fmats);

                }

            }

            aTable.SetColWidths(new List<double> { 9, 2, 4, 4 });
            //================== NOTES ===================================

            //============= PROJECT NOTES

            uopProperties Notes = _MDProject.Notes;
            if (Notes.Count > 0)
            {
                uopTable bTable = aPage.AddTable("Notes", 32, 3);
                aCell = bTable.SetValue(1, 1, "NOTES");
                aCell.Bold = true;
                aCell.Width = 38;
                aCell.SetBorderData((int)mzSides.Top + (int)mzSides.Bottom, mzBorderStyles.Continous, mzBorderWeights.Thin);
                int n = 1;

                for (int i = 1; i <= Notes.Count; i++)
                {
                    aStr = Notes.Value(i);
                    aCell = bTable.SetValue(n + 1, 1, n + " - ", "_", uopAlignments.TopRight);
                    aCell.Height = uopUtils.RoundTo(aStr.Length / 97d, dxxRoundToLimits.One, true);

                    aCell.Width = 2;
                    aCell.WrapText = true;

                    aCell = bTable.SetValue(n + 1, 2, aStr, "_", uopAlignments.TopLeft);
                    aCell.Height = uopUtils.RoundTo(aStr.Length / 97d, dxxRoundToLimits.One, true);
                    aCell.Width = 36;
                    aCell.WrapText = true;
                    n++;
                }

            }
            uopDocuments Warnings = _MDProject.Warnings().PlaceHolders(false);
            //================== WARNINGS ================
            if (Warnings.Count > 0)
            {
                uopTable cTable = aPage.AddTable("Warnings", Notes.Count == 0 ? 32 : 32 + Notes.Count + 3, 3);
                aCell = cTable.SetValue(1, 1, "WARNINGS");
                aCell.Bold = true;
                aCell.Width = 38;
                aCell.SetBorderData(mzSides.Top, mzBorderStyles.Continous, mzBorderWeights.Thin);
                aCell.SetBorderData(mzSides.Bottom, mzBorderStyles.Continous, mzBorderWeights.Thin);
                for (int i = 1; i <= Warnings.Count; i++)
                {
                    aWrn = (uopDocWarning)Warnings.Item(i);
                    aStr = aWrn.AbridgedText;
                    aCell = cTable.SetValue(i + 1, 1, i + " - ", "_", uopAlignments.TopRight);
                    aCell.Height = uopUtils.RoundTo(Convert.ToDouble(aStr.Length / 97), dxxRoundToLimits.One, true);
                    aCell.Width = 2;
                    aCell.WrapText = true;

                    aCell = cTable.SetValue(i + 1, 2, aStr, "_", uopAlignments.TopLeft);
                    aCell.Height = uopUtils.RoundTo(Convert.ToDouble(aStr.Length / 97), dxxRoundToLimits.One, true);
                    aCell.Width = 36;
                    aCell.WrapText = true;
                }
            }
            aPage.Protected = true;
            aPage.LockTable();
        }

        private static void xMD_Page_MDHardware(uopDocReportPage aPage)
        {
            mdTrayRange aRange = (mdTrayRange)aPage.Range;

            string sval;
            int iR = 0;
            if (aPage.PageType == uppReportPageTypes.HardwareTotals)
            {
                mdProject aProj = (mdProject)aPage.Project;
                if (aProj == null) return;
                uopTable aTable = aPage.AddTable("Input");

                aTable.SetCellValue(++iR, 1, aProj.Customer.Name, "CUSTOMER", bNumeric: false, bDataOnly: true);

                sval = aProj.ColumnName;
                if (string.IsNullOrWhiteSpace(sval)) sval = aProj.Name;
                aTable.SetCellValue(++iR, 1, sval, "PROJECTID", bNumeric: false, bDataOnly: true);
                aTable.SetCellValue(++iR, 1, "", "DESIGNER", bNumeric: false, bDataOnly: true);

                aTable.SetCellValue(++iR, 1, aProj.Bolting == uppUnitFamilies.Metric ? "M" : "E", "BOLTING", bNumeric: false, bDataOnly: true);
                aTable.SetCellValue(++iR, 1, aProj.SparePercentage, "SPAREPCT", bNumeric: true, bDataOnly: true);
                aTable.SetCellValue(++iR, 1, aProj.ClipSparePercentage, "SPARES_CLIP", bNumeric: false, bDataOnly: true);

            }
            else if (aPage.PageType == uppReportPageTypes.TrayHardwarePage)
            {
                if (aRange == null) return;
                uopTable aTable = aPage.AddTable("Input");

                mdTrayAssembly assy = aRange.TrayAssembly;
                List<mdDowncomer> dcs = assy.Downcomers.GetByVirtual(aVirtualValue: false); ;
                mdDeckSections dss = assy.DeckSections;
                List<mdDeckSection> manways = dss.Manways;

                mdDeckSection aManway = manways.Count > 0 ? manways[0] : null;
                uopHoleArray dsHoles = assy.DeckSections.GenHoles(assy, bTrayWide: true);
                uopHoleArray dcHoles = assy.Downcomers.GenHoles(assy, bSuppressSpouts: true, bTrayWide: true);
                colUOPParts SPangles = assy.GenerateSpliceAngles(bTrayWide: true);
                List<uopPart> MSpliceAngles = SPangles.GetByPartType( uppPartTypes.ManwaySplicePlate, bRemove:true);
                bool weldednuts = assy.Downcomers.ToList().FindIndex(x => x.WeldedBottomNuts) >= 0;

                aTable.SetCellValue(++iR, 1, "", "HIDDENFLAG", bNumeric: false, bDataOnly: true);
                aTable.SetCellValue(++iR, 1, assy.RingClipSize == uppRingClipSizes.ThreeInchRC ? 3 : 4, "RCSIZE", bNumeric: true, bDataOnly: true);
                aTable.SetCellValue(++iR, 1, aRange.RingStart, "RINGSTART", bNumeric: true, bDataOnly: true);
                aTable.SetCellValue(++iR, 1, aRange.RingEnd, "RINGEND", bNumeric: true, bDataOnly: true);
                aTable.SetCellValue(++iR, 1, assy.Downcomer().Count, "DCCOUNT", bNumeric: true, bDataOnly: true);
                aTable.SetCellValue(++iR, 1, assy.ManwayCount, "MANWAYS", bNumeric: true, bDataOnly: true);
                aTable.SetCellValue(++iR, 1, assy.SpliceStyle == uppSpliceStyles.Tabs ? "Y" : "N", "SLOTANDTAB", bNumeric: false, bDataOnly: true);
                aTable.SetCellValue(++iR, 1, assy.DesignOptions.BottomInstall ? "Y" : "N", "BOTTOMINSTALL", bNumeric: false, bDataOnly: true);
                aTable.SetCellValue(++iR, 1, weldednuts ? "Y" : "N", "WELDEDDCNUTS", bNumeric: false, bDataOnly: true); ;
                aTable.SetCellValue(++iR, 1, assy.DesignOptions.WeldedStiffeners ? "Y" : "N", "WELDEDSTF", bNumeric: false, bDataOnly: true);
                aTable.SetCellValue(++iR, 1, dsHoles.MemberCount("RING CLIP"), "RCCOUNT_DECK", bNumeric: true, bDataOnly: true);
                aTable.SetCellValue(++iR, 1, dcHoles.MemberCount("RING CLIP"), "RCCOUNT_DC", bNumeric: true, bDataOnly: true);
                aTable.SetCellValue(++iR, 1, assy.FingerClips(bBothSides: true).Count, "FCCOUNT", bNumeric: true, bDataOnly: true);
                aTable.SetCellValue(++iR, 1, dsHoles.MemberCount("BOLT"), "JOGGLE_BOLTS", bNumeric: true, bDataOnly: true);
                aTable.SetCellValue(++iR, 1, dcHoles.MemberCount("APPAN_HOLE"), "AP_BOLTS", bNumeric: true, bDataOnly: true);
                mdSpliceAngle Spangle = assy.SpliceAngle(uppSpliceAngleTypes.SpliceAngle);
                aTable.SetCellValue(++iR, 1, Spangle.BoltCount * 2, "SPLANGLE_BOLTS", bNumeric: true, bDataOnly: true);
                aTable.SetCellValue(++iR, 1, Spangle.BoltCount, "MANGLE_BOLTS", bNumeric: true, bDataOnly: true);
                aTable.SetCellValue(++iR, 1, Spangle.BoltCount * 2, "MANSPLICE_BOLTS", bNumeric: true, bDataOnly: true);
                aTable.SetCellValue(++iR, 1, SPangles.Count, "SPLANGLE_COUNT", bNumeric: true, bDataOnly: true);


                int f = 0;
                if (MSpliceAngles.Count > 0)
                {

                    Spangle = (mdSpliceAngle)MSpliceAngles[0];
                    f = Spangle.BoltCount * 2;
                }

                if (aManway != null)
                {
                    colDXFVectors pts = assy.ManwayFastenerCenters(false);
                    colDXFVectors bpts = pts.GetByFlagList("LEFT,RIGHT");

                    aTable.SetCellValue(++iR, 1, bpts.Count, "MANFAST_HORIZONTAL", bNumeric: true, bDataOnly: true);
                    bpts = pts.GetByFlagList("TOP,BOTTOM");
                    aTable.SetCellValue(++iR, 1, bpts.Count, "MANFAST_VERTICAL", bNumeric: true, bDataOnly: true);
                }
                f = 0;
                int cnt;



                if (assy.DesignFamily.IsEcmdDesignFamily())
                {

                    foreach (mdDowncomer dc in dcs)
                    {
                        cnt = dc.StiffenerYs.Count * dc.OccuranceFactor;
                        f += cnt;
                    }
                }
                aTable.SetCellValue(++iR, 1, f, "BAFFLE_MOUNT", bNumeric: true, bDataOnly: true);
                //f = 0;
                //string aStr = string.Empty;
                //cnt = 0;
                //if (assy.Downcomer().Width > 4)
                //{
                //    if (assy.DesignOptions.HasAntiPenetrationPans || assy.DesignOptions.CrossBraces)
                //    {
                //        foreach (mdDowncomer dc in dcs)
                //        {
                //            if (dc.WeldedBottomNuts)
                //            {
                //                mzUtils.ListAdd(ref aStr, dc.PartNumber);
                //                uopHoleArray holes = dc.GenHoles(assy, bSuppressSpouts: true);
                //                cnt += holes.MemberCount("CROSSBRACE,APPAN_HOLE");
                //                if (dc.OccuranceFactor >= 1) cnt *= dc.OccuranceFactor;
                //            }
                //        }
                //    }
                //}

                //if (cnt > 0)
                //{
                //    aTable.SetCellValue(++iR, 1, $"DCs {aStr} Welded Bottom Nuts", "M10NUT_ADDER_CMT", bNumeric: true, bDataOnly: true);
                //    aTable.SetCellValue(++iR, 1, -cnt, "M10NUT_ADDER", bNumeric: true, bDataOnly: true);
                //    aTable.SetCellValue(++iR, 1, aRange.TrayCount, "M10NUT_ADDER_MUTLI", bNumeric: true, bDataOnly: true);
                //}
            }



        }

        /// <summary>
        /// MD Page Optimization
        /// </summary>
        /// <param name="aPage"></param>
        private static void xMD_Page_Optimization(uopDocReportPage aPage)
        {


            mdTrayRange aRange = (mdTrayRange)aPage.Range;

            uopTable aTable;


            List<object> Counts = null;
            mdDeckPanel aDP;

            aPage.Title = "Optimization Results";
            aPage.SubTitle1 = string.Empty;
            if (aRange != null) aPage.SubTitle1 = aRange.Name(true);

            aPage.SubTitle2 = string.Empty;
            aPage.Protected = true;
            aPage.Tables = new List<uopTable>();

            try
            {
                if (aRange != null)
                {

                    aTable = aPage.AddTable("Downcomer Spouting1", 13, 4);

                    mdTrayAssembly aAssy = aRange.TrayAssembly;
                    List<mdDowncomer> DComers = aAssy.Downcomers.GetByVirtual(aVirtualValue: false);
                    List<mdDeckPanel> DPanels = aAssy.DeckPanels.GetByVirtual(aVirtualValue: false);


                    //get some numbers
                    mdSpacingData sdata = aAssy.Downcomers.FetchSpacingData(aAssy);



                    //the header row of the first actual table
                    // List<string> strings = mzUtils.StringsFromDelimitedList("DC|Box Length|Weir Length|Spout Area|Error|Downcomer/Tray", "|");
                    List<string> strings = new List<string>() { "DC", "Box Length", "Weir Length", "Spout Area", "Error", "Downcomer/Tray" };
                    List<double> colwidths = new List<double> { 3, 8, 8, 6, 3, 8 };
                    aTable.AddRow(strings, aTextFormats: new List<mzTextFormats> { mzTextFormats.Bold }, aAlignments: new List<uopAlignments> { uopAlignments.TopCenter }, aCellWidths: colwidths);
                    aTable.SetBorders(1, 0, mzSides.Outer, mzBorderStyles.Continous, mzBorderWeights.Medium);
                    aTable.SetBorders(1, 0, mzSides.Inner, mzBorderStyles.Continous, mzBorderWeights.Thin);
                    aTable.SetBorders(1, 0, mzSides.Bottom, mzBorderStyles.None, mzBorderWeights.Undefined);

                    //the body of the actual table
                    aTable = aPage.AddTable("Downcomer Spouting2", aTable.StartRow + 1, aTable.StartColumn);
                    colwidths = new List<double> { 3, 4, 4, 4, 4, 3, 3, 3, 4, 4 };
                    uopProperties Props;

                    for (int i = 1; i <= DComers.Count; i++)
                    {
                        Props = DComers[i - 1].ReportProperties(aAssy, 1);

                        if (i == 1)
                        {
                            strings = Props.DisplayNames;
                            aTable.AddRow(strings, aAlignments: new List<uopAlignments> { uopAlignments.MiddleCenter }, aTextFormats: new List<mzTextFormats> { mzTextFormats.Bold }, aCellWidths: colwidths);
                            strings = Props.UnitCaptions(aPage.Units);
                            aTable.AddRow(strings, aAlignments: new List<uopAlignments> { uopAlignments.MiddleCenter }, aCellWidths: colwidths);

                            aTable.SetValues("1", "1,8", string.Empty);

                        }

                        aTable.AddRow(Props, aAlignments: new List<uopAlignments> { uopAlignments.MiddleCenter }, aCellWidths: colwidths);

                    }
                    aTable.SetBorders(0, 0, mzSides.Outer, mzBorderStyles.Continous, mzBorderWeights.Medium);
                    aTable.SetBorders(1, 0, mzSides.Top, mzBorderStyles.None, mzBorderWeights.Undefined);
                    aTable.SetBorders(2, 0, mzSides.Outer, mzBorderStyles.Continous, mzBorderWeights.Medium);
                    aTable.SetBorders("All", "1,3,5,7,8", mzSides.Right, mzBorderStyles.Continous, mzBorderWeights.Thin);
                    aTable.SetNumberFormat($"3-{aTable.Rows}", "9,10", "0.0000");

                    //the header row of the second actual table
                    aTable = aPage.AddTable("Panel Spouting1", aTable.StartRow + aTable.Rows + 2, aTable.StartColumn);
                    colwidths = new List<double> { 3, 8, 4, 4, 6, 3, 8 };


                    strings = new List<string>() { "Panel", "Weir Length", "FBA", "FBA/WL", "Spout Area", "Error", "Panel/Tray" };
                    aTable.AddRow(strings, aAlignments: new List<uopAlignments> { uopAlignments.TopCenter }, aTextFormats: new List<mzTextFormats> { mzTextFormats.Bold }, aCellWidths: colwidths);
                    aTable.SetBorders(1, 0, mzSides.Outer, mzBorderStyles.Continous, mzBorderWeights.Medium);
                    aTable.SetBorders(1, 0, mzSides.Inner, mzBorderStyles.Continous, mzBorderWeights.Thin);
                    aTable.SetBorders(1, 0, mzSides.Bottom, mzBorderStyles.None, mzBorderWeights.Undefined);


                    //the body of the actual table
                    aTable = aPage.AddTable("Panel Spouting2", aTable.StartRow + 1, 4);
                    colwidths = new List<double> { 3, 4, 4, 4, 4, 3, 3, 3, 4, 4 };

                    uopFreeBubblingPanels assyFBAs = aAssy.FreeBubblingPanels;
                    for (int i = 1; i <= DPanels.Count; i++)
                    {
                        Props = DPanels[i - 1].ReportProperties(aAssy, 1, assyFBAs);
                        if (i == 1)
                        {
                            strings = Props.DisplayNames;
                            aTable.AddRow(strings, aAlignments: new List<uopAlignments> { uopAlignments.MiddleCenter }, aTextFormats: new List<mzTextFormats> { mzTextFormats.Bold }, aCellWidths: colwidths);
                            strings = Props.UnitCaptions(aPage.Units);
                            aTable.AddRow(strings, aAlignments: new List<uopAlignments> { uopAlignments.MiddleCenter }, aCellWidths: colwidths);

                            aTable.SetValues("1", "1,4,5,8", string.Empty);

                        }

                        aTable.AddRow(Props, aAlignments: new List<uopAlignments> { uopAlignments.MiddleCenter }, aCellWidths: colwidths);

                    }
                    aTable.SetNumberFormat($"3-{aTable.Rows}", "9,10", "0.0000");
                    aTable.SetBorders(0, 0, mzSides.Outer, mzBorderStyles.Continous, mzBorderWeights.Medium);
                    aTable.SetBorders(1, 0, mzSides.Top, mzBorderStyles.None, mzBorderWeights.Undefined);
                    aTable.SetBorders(2, 0, mzSides.Outer, mzBorderStyles.Continous, mzBorderWeights.Medium);
                    aTable.SetBorders("All", "1,3,4,5,7,8", mzSides.Right, mzBorderStyles.Continous, mzBorderWeights.Thin);


                    //the downcomer details table

                    aTable = aPage.AddTable("Downcomer Details", aTable.StartRow + aTable.Rows + 2, 15);
                    colwidths = new List<double> { 3, 3, 3, 4, 3, 3, 3, 3 };

                    for (int i = 1; i <= DComers.Count; i++)
                    {
                        Props = DComers[i - 1].ReportProperties(aAssy, 2);
                        if (i == 1)
                        {
                            strings = Props.DisplayNames;
                            aTable.AddRow(strings, aAlignments: new List<uopAlignments> { uopAlignments.MiddleCenter }, aTextFormats: new List<mzTextFormats> { mzTextFormats.Bold }, aCellWidths: colwidths);
                            strings = Props.UnitCaptions(aPage.Units);
                            strings[6] = strings[5];
                            aTable.AddRow(strings, aAlignments: new List<uopAlignments> { uopAlignments.MiddleCenter }, aCellWidths: colwidths);
                        }

                        aTable.AddRow(Props, aAlignments: new List<uopAlignments> { uopAlignments.MiddleCenter }, aCellWidths: colwidths);

                    }
                    aTable.SetNumberFormat($"3-{aTable.Rows}", "8", "0.0");
                    aTable.SetBorders(0, 0, mzSides.Right, mzBorderStyles.Continous, mzBorderWeights.Thin);
                    aTable.SetBorders(0, 0, mzSides.Outer, mzBorderStyles.Continous, mzBorderWeights.Medium);
                    aTable.SetBorders(2, 0, mzSides.Outer, mzBorderStyles.Continous, mzBorderWeights.Medium);


                    //    'the flow slot and VL table
                    aTable = aPage.AddTable("Slotting Details", aTable.StartRow, 4);
                    colwidths = new List<double> { 3, 4, 3 };
                    aTable.AddRow(new List<string> { "Panel", "V/L Error", "SLOTS" }, aTextFormats: new List<mzTextFormats> { mzTextFormats.Bold }, aCellWidths: colwidths, aAlignments: new List<uopAlignments> { uopAlignments.MiddleCenter });
                    aTable.AddRow(new List<string> { "", "%", "#" }, aCellWidths: colwidths, aAlignments: new List<uopAlignments> { uopAlignments.MiddleCenter });

                    bool aFlg = aRange.DesignFamily.IsEcmdDesignFamily();
                    if (aFlg)
                    {
                        Counts = aAssy.PanelSlotCounts(out int _, out double _, out double _, out double _);
                    }


                    uopProperty p1 = new uopProperty("COL1", "") { Protected = true };
                    uopProperty p2 = new uopProperty("COL2", "") { Protected = true };
                    uopProperty p3 = new uopProperty("COL3", "-") { Protected = true };

                    for (int i = 1; i <= DPanels.Count; i++)
                    {
                        aDP = DPanels[i - 1];
                        p1.SetValue(aDP.Label);
                        p2.SetValue("ERR");


                        if (sdata.FBA2WLRatio != 0)
                        {
                            if (aDP.TotalWeirLength(aAssy) > 0)
                            {
                                p2.SetValue(((aDP.FreeBubblingArea(aAssy) / aDP.TotalWeirLength(aAssy) / sdata.FBA2WLRatio) - 1) * 100);
                            }
                        }


                        if (aFlg)
                            p3.SetValue(Counts[i - 1]);

                        aTable.AddRow(new List<uopProperty> { p1.Clone(), p2.Clone(), p3.Clone() }, aCellWidths: colwidths, aAlignments: new List<uopAlignments> { uopAlignments.MiddleCenter });

                    }

                    aTable.SetNumberFormat($"3-{aTable.Rows}", "2", "0.000");
                    if (aFlg) aTable.SetNumberFormat($"3-{aTable.Rows}", "3", "#,0");

                    aTable.SetBorders(0, 0, mzSides.Right, mzBorderStyles.Continous, mzBorderWeights.Thin);
                    aTable.SetBorders(0, 0, mzSides.Outer, mzBorderStyles.Continous, mzBorderWeights.Medium);
                    aTable.SetBorders(2, 0, mzSides.Outer, mzBorderStyles.Continous, mzBorderWeights.Medium);

                    //the borderless table of the downcomer optimization info.

                    aTable = aPage.AddTable("Summary", aTable.StartRow + aTable.Rows + 1, aTable.StartColumn);


                    colwidths = new List<double> { 7, 4, 2, 7, 3, 2, 6, 3, 2 };

                    List<uopAlignments> alignments = new List<uopAlignments> { uopAlignments.MiddleRight, uopAlignments.MiddleRight, uopAlignments.MiddleLeft, uopAlignments.MiddleRight, uopAlignments.MiddleRight, uopAlignments.MiddleLeft, uopAlignments.MiddleRight, uopAlignments.MiddleRight, uopAlignments.MiddleLeft };

                    p1 = new uopProperty("IdealSpoutArea", aAssy.TheoreticalSpoutArea, aUnits: uppUnitTypes.SmallArea) { DisplayName = "Ideal Spout Area:", DisplayUnits = aPage.Units, Protected = true };
                    p2 = new uopProperty("FBA/WL", sdata.FBA2WLRatio, aUnits: uppUnitTypes.SmallLength) { DisplayName = "Overall FBA/WL:", DisplayUnits = aPage.Units, Protected = true };
                    p3 = new uopProperty("OptimizationMethod", aAssy.SpacingMethod.GetDescription()) { DisplayName = "Optim. Method:", DisplayUnits = aPage.Units, Protected = true };

                    List<uopProperty> props = CreatePropertyList(new List<uopProperty> { p1, p2, p3 }, true);
                    aTable.AddRow(props, aCellWidths: colwidths, aAlignments: alignments);

                    p1 = new uopProperty("ActualSpoutArea", aAssy.TotalSpoutArea, aUnits: uppUnitTypes.SmallArea) { DisplayName = "Actual Spout Area:", DisplayUnits = aPage.Units, Protected = true };
                    p2 = new uopProperty("OptimumSpacing", aAssy.Downcomers.OptimumSpacing, aUnits: uppUnitTypes.SmallLength) { DisplayName = "Ideal DC Spacing:", DisplayUnits = aPage.Units, Protected = true };
                    p3 = new uopProperty("StdDeviation", (aAssy.SpacingMethod == uppMDSpacingMethods.NonWeighted) ? sdata.StandardDeviation : sdata.WeightedDeviation) { DisplayName = "Std. Deviation:", DisplayUnits = aPage.Units, Protected = true };

                    props = CreatePropertyList(new List<uopProperty> { p1, p2, p3 }, true);
                    aTable.AddRow(props, aCellWidths: colwidths, aAlignments: alignments);
                    aTable.SetNumberFormat("2", aTable.Cols.ToString(), "0.0000");

                    p1 = new uopProperty("ErrorPercentage", aAssy.ErrorPercentage, aUnits: uppUnitTypes.Percentage) { DisplayName = "Error Percentage:", DisplayUnits = aPage.Units, Protected = true };
                    props = CreatePropertyList(new List<uopProperty> { p1 }, true);
                    aTable.AddRow(props, aCellWidths: colwidths, aAlignments: alignments);

                    if (Math.Abs(aAssy.ErrorPercentage) > aAssy.ErrorLimit)
                    {
                        aTable.SetFontColor("3", "1,2,3", System.Drawing.Color.Red);
                    }
                    aTable.SetNumberFormat(2, 8, "0.0000");
                    aTable.SetTextFormat("All", "1,4,7", mzTextFormats.Bold);
                }
            }
            catch (Exception e)
            {
                SaveWarning(System.Reflection.MethodBase.GetCurrentMethod().Name, aPage.SelectText, e: e);
            }


        }

        private static List<uopProperty> CreatePropertyList(List<uopProperty> aBaseProps, bool withUnits)
        {
            List<uopProperty> _rVal = new List<uopProperty>();
            if (aBaseProps == null) return _rVal;
            foreach (var item in aBaseProps)
            {
                if (item != null)
                {
                    _rVal.Add(new uopProperty($"{item.Name}_Caption", item.DisplayName) { Protected = item.Protected });
                    _rVal.Add(item.Clone());
                    if (withUnits) _rVal.Add(new uopProperty($"{item.Name}_Units", item.DisplayUnitLabel) { Protected = item.Protected });
                }
            }


            return _rVal;
        }

        /// <summary>
        /// executed internally to create the Tray Design Summary  page(s) for a MD Spout project
        /// </summary>
        /// <param name="aPage"></param>
        private static void xMD_Page_TraySummary(uopDocReportPage aPage)
        {

            int si = aPage.StartIndex;
            int ei = aPage.EndIndex;
            List<uopTrayRange> aRanges = _MDProject.TrayRanges.GetRanges(si, ei);
            mdTrayRange aRange;
            uopProperties Props;
            uopProperty aProp;
            uopTableCell aCell;
            int k = 0;
            List<uopTableCell> col;

            aPage.Title = "Tray Design Summary";
            aPage.SubTitle1 = string.Empty;
            aPage.SubTitle2 = string.Empty;
            aPage.Protected = true;
            aPage.Tables = new List<uopTable>();
            uopTable aTable = aPage.AddTable("Tray Summary", 13, 4);

            aTable.SetDimensions(48, 8);
            for (int i = 1; i <= 6; i++)
            {
                col = new List<uopTableCell>();
                if (i <= aRanges.Count)
                {
                    aRange = (mdTrayRange)aRanges[i - 1];
                    k = aRange.Index;
                    Props = aRange.ReportProperties(1);
                    Props.DisplayUnits = aPage.Units;
                    if (i == 1)
                    {
                        var values = Props.Names;
                        values[0] = string.Empty;
                        aTable.SetByCollection(1, true, values, aAlignments: new List<uopAlignments> { uopAlignments.MiddleLeft }, aTextFormats: new List<mzTextFormats> { mzTextFormats.Bold });
                        values = Props.UnitCaptions(aPage.Units);
                        values[0] = "Units";
                        aTable.SetByCollection(2, true, values, aAlignments: new List<uopAlignments> { uopAlignments.MiddleCenter });
                        aTable.SetLocks("All", "1,2", true);
                    }

                    for (int j = 1; j <= Props.Count; j++)
                    {
                        aProp = Props.Item(j);
                        aCell = aTable.Cell(j, i + 2);
                        col.Add(aCell);
                        if (aProp.HasUnits && aProp.ValueD <= 0) aProp = new uopProperty(aProp.Name, "-");
                        aCell.Property = aProp.Clone();
                        if (string.Compare(aProp.Name, "Tray Spacing", true) == 0)
                        {
                            aCell.NumberFormat = (aPage.Units == uppUnitFamilies.English) ? "0.00" : "0.0";

                        }
                        else if (string.Compare(aProp.Name, "Downcomer Spacing", true) == 0)
                        {
                            if (aRange.TrayAssembly.Downcomers.OptimumSpacing != aProp.ValueD)
                                aCell.FontColor = System.Drawing.Color.Blue;
                        }
                        aCell.Bold = j == 1;

                    }
                }
                else
                {
                    //k ++;
                    aTable.Cell(1, i + 2).Value = ++k;
                }
            }
            aTable.SetWidth("All", "4");

            aTable.SetWidth(1, 9);
            aTable.SetWidth(2, 3);
            aTable.SetTextFormat("All", "1", mzTextFormats.Bold);


            aTable.SetAlignment("All", "All", uopAlignments.TopCenter);
            aTable.SetAlignment("All", "1", uopAlignments.TopLeft);
            aTable.SetBorders("All", "All", mzSides.Right, mzBorderStyles.Continous, mzBorderWeights.Thin);
            aTable.SetBorders("All", "1,2", mzSides.Right, mzBorderStyles.Continous, mzBorderWeights.Medium);

            aTable.SetBorders("1,10,27,46", "All", mzSides.Bottom, mzBorderStyles.Continous, mzBorderWeights.Medium);
            aTable.SetBorders("22,40,44", "All", mzSides.Bottom, mzBorderStyles.Continous, mzBorderWeights.Thin);

            aTable.BordersAround(mzBorderStyles.Continous, mzBorderWeights.Medium);

        }

        /// <summary>
        /// MD Page SpoutDetails
        /// </summary>
        /// <param name="aPage"></param>
        private static void xMD_Page_SpoutDetails(uopDocReportPage aPage)
        {


            uopTableCell aCell;


            string dcStr = string.Empty;
            string dcRows = string.Empty;
            aPage.Title = "Spouting Details";
            aPage.Protected = true;
            int si = aPage.StartIndex;
            int ei = aPage.EndIndex;
            aPage.Tables = new List<uopTable>();
            uopTable aTable = aPage.AddTable("Spout Groups", 13, 4);
            mdTrayRange aRange = (mdTrayRange)aPage.Range;
            mdTrayAssembly aAssy = aRange.TrayAssembly;
            List<mdSpoutGroup> sGroups = aAssy.SpoutGroups.GetByVirtual(false);
            int k = 4;
            int iR = 1;

            try
            {

                aTable.SetDimensions(3 + ei - si + 1, 12, "3,3,3,3,3,3,3,3,3,3,3,3");
                aTable.SetAlignment(1, 0, uopAlignments.TopCenter); //0 == "All"
                aTable.SetTextFormat(1, 0, mzTextFormats.Bold);

                //the body of the Spouts table

                List<string> spoutLengthNotes = new List<string>();
                for (int i = si; i <= ei; i++)
                {
                    if (i > sGroups.Count) break;

                    mdSpoutGroup group = sGroups[i - 1];

                    uopProperties Props = group.ReportProperties(1, spoutLengthNotes);
                    if (iR == 1)
                    {
                        List<string> Values = Props.Names;

                        for (int j = 1; j <= Values.Count; j++)
                        {
                            aCell = aTable.Cell(1, j);
                            uopTableCell bCell = aTable.Cell(2, j);
                            string aStr = Values[j - 1];
                            string bStr = aStr;
                            int m = aStr.IndexOf("\n");
                            if (m != -1)
                            {
                                aStr = aStr.Substring(0, m).Trim();
                                bStr = bStr.Substring(m, bStr.Length - m).Trim();
                            }
                            else
                            {
                                bStr = string.Empty;
                            }
                            if (aStr != string.Empty)
                            {
                                aCell.Value = aStr;
                                aCell.Bold = true;
                                aCell.Alignment = uopAlignments.MiddleCenter;
                            }
                            if (bStr != string.Empty)
                            {
                                bCell.Value = bStr;
                                bCell.Bold = true;
                                bCell.Alignment = uopAlignments.MiddleCenter;
                            }
                            if (j == 12)
                            {
                                aCell.Address = "AE13:AM13";
                            }
                        }

                        Values = Props.UnitCaptions(aPage.Units);
                        for (int j = 1; j <= Values.Count; j++)
                        {
                            aCell = aTable.Cell(3, j);
                            aCell.Value = Values[j - 1];
                            aCell.Alignment = uopAlignments.MiddleCenter;
                        }
                    }

                    SetValueDetails(ref dcStr, aTable, iR, Props, aPage.Units);
                    uopProperty aProp = Props.Item(1);
                    if (aProp.Value != dcStr)
                    {
                        mzUtils.ListAdd(ref dcRows, iR + 2);
                        for (int j = iR + 2; j >= k + 1; j--)
                        {
                            aTable.SetValue(j, 1, string.Empty);
                        }
                        k = iR + 3;
                    }
                    dcStr = Convert.ToString(aProp.Value);

                    iR++;
                }
                for (int j = iR + 2; j >= k + 1; j--)
                {
                    aTable.SetValue(j, 1, string.Empty);
                }
                AddNotesTable(aPage, aTable.StartRow + 5 + ei - si, spoutLengthNotes);

                aTable.SetBorders(dcRows, "All", mzSides.Bottom, mzBorderStyles.Continous, mzBorderWeights.Medium);
                aTable.SetBorders(3, 0, mzSides.Top, mzBorderStyles.Continous, mzBorderWeights.Medium);
                aTable.SetBorders(3, 0, mzSides.Bottom, mzBorderStyles.Continous, mzBorderWeights.Medium);
                aTable.SetBorders("All", "1,2,9", mzSides.Right, mzBorderStyles.Continous, mzBorderWeights.Medium);
                aTable.BordersAround(mzBorderStyles.Continous, mzBorderWeights.Medium);
                aTable.SetLocks("All", "All", true);
            }
            catch (Exception e)
            {
                SaveWarning(System.Reflection.MethodBase.GetCurrentMethod().Name, aPage.SelectText, e: e);
            }


        }

        /// <summary>
        /// Set the value to cells
        /// </summary>
        /// <param name="dcStr"></param>
        /// <param name="aTable"></param>
        /// <param name="iR"></param>
        /// <param name="Props"></param>
        private static void SetValueDetails(ref string dcStr, uopTable aTable, int iR, uopProperties Props, uppUnitFamilies aUnits)
        {
            for (int j = 1; j <= Props.Count; j++)
            {
                uopProperty aProp = Props.Item(j);
                if (iR == 1 && j == 1)
                {
                    dcStr = Convert.ToString(aProp.Value);
                }
                if (j == 5 && (Convert.ToString(aProp.Value) == string.Empty || Convert.ToString(aProp.Value) == "0"))
                {
                    aProp.Value = string.Empty;
                }
                uopTableCell aCell = aTable.Cell(iR + 3, j);
                xDefineCellWithProperty(ref aCell, aProp, aUnits);

                aCell.Locked = true;
                aCell.Alignment = uopAlignments.MiddleCenter;
            }
        }

        private static void AddNotesTable(uopDocReportPage aPage, int startRow, List<string> spoutLengthNotes)
        {
            if (spoutLengthNotes.Count > 0)
            {
                uopTable bTable = aPage.AddTable("Notes");
                bTable.StartColumn = 4;
                bTable.StartRow = startRow;
                bTable.SetDimensions(spoutLengthNotes.Count, 1, "36");
                for (int i = 0; i < spoutLengthNotes.Count; i++)
                {
                    uopTableCell aCell = bTable.Cell(i + 1, 1);
                    aCell.Value = spoutLengthNotes[i];
                    aCell.Alignment = uopAlignments.MiddleLeft;
                }
            }
        }

        /// <summary>
        /// MD Page Constraints
        /// </summary>
        /// <param name="aPage"></param>
        private static void xMD_Page_Constraints(uopDocReportPage aPage)
        {
            const string STR33223333333311 = "3,3,2,2,3,3,3,3,3,3,3,3,1,1";
            int i = 0;
            int j = 0;
            mdTrayRange aRange = (mdTrayRange)aPage.Range;
            uopProperty aProp;

            uopProperties Props;
            uopTableCell aCell = null;

            List<string> Values;

            mdTrayAssembly aAssy = aRange.TrayAssembly;
            //colMDDowncomers DComers = null;
            //colMDDeckPanels DPanels = null;
            string aStr = string.Empty;
            string bStr = string.Empty;

            List<mdSpoutGroup> sGroups = aAssy.SpoutGroups.GetByVirtual(aVirtualValue: false);
            string dcStr = string.Empty;
            string dcRows = string.Empty;
            int si = aPage.StartIndex;
            int ei = aPage.EndIndex;


            aPage.SubTitle1 = string.Empty;
            aPage.SubTitle2 = string.Empty;
            aPage.Protected = true;
            aPage.Tables = new List<uopTable>();
            uopTable aTable = aPage.AddTable("Constraints", 13, 4);

            //the body of the constraints table
            aTable.LabelCell.Value = "Constraints:";
            aTable.LabelCell.Width = 9;
            aTable.LabelCell.Bold = true;

            aTable.SetDimensions(2 + ei - si + 1, 14, STR33223333333311);
            aTable.SetAlignment("1", string.Empty, uopAlignments.BottomCenter, 90);
            aTable.SetTextFormat("1", "All", mzTextFormats.Bold);
            aTable.SetHeight("1", 6);
            int iR = 3;
            List<string> spoutLengthNotes = new List<string>();
            for (i = si; i <= ei; i++)
            {
                Props = sGroups[i - 1].ReportProperties(2, spoutLengthNotes);
                if (i == si)
                {
                    Values = Props.Names;

                    for (j = 1; j <= Values.Count; j++)
                    {
                        aCell = aTable.Cell(1, j);
                        aStr = Values[j - 1];
                        if (aStr != string.Empty)
                        {
                            aCell.Value = aStr;
                            aCell.Bold = true;
                            aCell.Alignment = uopAlignments.BottomCenter;
                            aCell.Orientation = 90;
                            aCell.Locked = true;
                        }
                    }

                    Values = Props.UnitCaptions(aPage.Units);
                    for (j = 1; j <= Values.Count; j++)
                    {
                        aCell = aTable.Cell(2, j);
                        aStr = Values[j - 1];
                        if (aStr != string.Empty)
                        {
                            aCell.Value = aStr;
                            aCell.Alignment = uopAlignments.MiddleCenter;
                        }
                    }
                }

                for (j = 1; j <= Props.Count; j++)
                {
                    aProp = Props.Item(j);
                    if (i == si && j == 1)
                    {
                        dcStr = Convert.ToString(aProp.Value);
                    }
                    aCell = aTable.Cell(iR, j);
                    xDefineCellWithProperty(ref aCell, aProp, aPage.Units);
                    aCell.Locked = true;
                    aCell.Alignment = uopAlignments.MiddleCenter;
                }
                aProp = Props.Item(1);
                if (aProp.Value != dcStr)
                {
                    mzUtils.ListAdd(ref dcRows, i + 1);
                }
                dcStr = Convert.ToString(aProp.Value);
                iR++;
            }


            aTable.SetBorders(2, 0, mzSides.Top, mzBorderStyles.Continous, mzBorderWeights.Medium);
            aTable.SetBorders(2, 0, mzSides.Bottom, mzBorderStyles.Continous, mzBorderWeights.Medium);
            aTable.SetBorders(0, 0, mzSides.Right, mzBorderStyles.Continous, mzBorderWeights.Thin);
            aTable.SetBorders(dcRows, "All", mzSides.Bottom, mzBorderStyles.Continous, mzBorderWeights.Medium);
            aTable.BordersAround(mzBorderStyles.Continous, mzBorderWeights.Medium);
        }

        private static void xMD_Page_SpacingOptimization(uopDocReportPage aPage)
        {
            throw new NotImplementedException();
            //    //^executed internally to populate the spacing optimization report page

            //    mdTrayAssembly aAssy = null;
            //    decimal xSpace = 0;
            //    decimal std = 0;
            //    decimal wtstd = 0;
            //    uopTable aTable = null;
            //    mzValues Values = null;
            //    decimal multi = 0;
            //    uopGraph aGraph = null;
            //    uopGraphSeries aSeries = null;

            //    uopDocReportPage _WithVar_aPage;
            //    _WithVar_aPage = aPage;
            //    aAssy = _WithVar_aPage.Range.TrayAssembly;
            //    aTable = _WithVar_aPage.AddTable("Deviations");
            //    if (_WithVar_aPage.Units == English)
            //    {
            //        multi = 1;
            //    }
            //    else
            //    {
            //        multi = 25.4m;
            //    }

            //    aPage = _WithVar_aPage;
            //    xSpace = aAssy.Downcomers.Spacing - 1;

            //    uopTable aTable;
            //    aTable = aTable;
            //    aTable.StartColumn = 4;
            //    aTable.StartRow = 13;

            //    Values = new mzValues();

            //    Values.Add("DC Center To Center");
            //    Values.Add("Std. Deviation");
            //    Values.Add("Wtd. Std. Deviation");

            //    aTable.AddRow(Values);

            //    while (!(xSpace >= aAssy.Downcomers.Spacing + 1))
            //    {
            //        OptimizedSpace(aAssy, aAssy.Downcomers, xSpace, std);
            //        Values.Clear();

            //        Values.Add(xSpace * multi);
            //        Values.Add(std);
            //        Values.Add(wtstd);
            //        aTable.AddRow(Values);

            //        xSpace = xSpace + 0.0625m;
            //        DoEvents();
            //    }
            //    aTable.SetWidth("All", 9);
            //    aTable.SetAlignment("All", "All", mza_TopCenter);
            //    aTable.SetTextFormat(1, "All", mzBold);

            //    aTable.SetNumberFormats("All", "All", "0.000");
            //    aTable.SetNumberFormats("All", 1, "0.0000");

            //    //.BordersAround Continous, Thin
            //    aTable = aTable;

            //    aGraph = aPage.AddGraph(uopGraphXYScatterSmooth, "Trays " + aPage.SpanName + " Downcomer Spacing Optimization Graph", "Chart(" + aPage.SpanName + ")", true);
            //    uopGraph _WithVar_aGraph;
            //    _WithVar_aGraph = aGraph;
            //    _WithVar_aGraph.XAxisTitle = "Downcomer Center To Center Spacing";
            //    _WithVar_aGraph.HasLegend = true;
            //    _WithVar_aGraph.MajorUnits = 0.25m;
            //    _WithVar_aGraph.MinimumScale = aTable.Cell(3, 1, False).Value - 0.0625m;
            //    _WithVar_aGraph.MaximumScale = aTable.Cell(aTable.Rows, 1, False).Value + 0.0625m;
            //    aSeries = _WithVar_aGraph.AddSeries("Std. Deviation", aTable.Name, 1, 2, 3, aTable.Rows);
            //    aSeries = _WithVar_aGraph.AddSeries("Wtd. Std. Deviation", aTable.Name, 1, 3, 3, aTable.Rows);
            //    aGraph = _WithVar_aGraph;
        }

        private static void xMD_Page_Test(uopDocReportPage aPage)
        {
            throw new NotImplementedException();
            //    //^executed internally to populate the test page of an MD Spout Report

            //    mdTrayAssembly aAssy = null;
            //    mdUtilities mdUtils = null;
            //    decimal xSpace = 0;
            //    decimal std = 0;
            //    decimal wtstd = 0;
            //    uopTable aTable = null;
            //    mzValues Values = null;
            //    decimal multi = 0;
            //    uopGraph aGraph = null;
            //    uopGraphSeries aSeries = null;

            //    mdUtils = new mdUtilities();
            //    uopDocReportPage _WithVar_aPage;
            //    _WithVar_aPage = aPage;
            //    aAssy = _WithVar_aPage.Range.TrayAssembly;
            //    aTable = _WithVar_aPage.AddTable("Deviations");
            //    if (_WithVar_aPage.Units == English)
            //    {
            //        multi = 1;
            //    }
            //    else
            //    {
            //        multi = 25.4m;
            //    }

            //    aPage = _WithVar_aPage;
            //    xSpace = aAssy.Downcomers.Spacing - 1;

            //    uopTable aTable;
            //    aTable = aTable;
            //    aTable.StartColumn = 4;
            //    aTable.StartRow = 13;

            //    Values = new mzValues(); ;

            //    Values.Add("DC Center To Center");
            //    Values.Add("Std. Deviation");
            //    Values.Add("Wtd. Std. Deviation");

            //    aTable.AddRow(Values);

            //    while (!(xSpace >= aAssy.Downcomers.Spacing + 1))
            //    {
            //        mdUtils.OptimizedSpace(aAssy.Downcomers, xSpace, std);
            //        Values.Clear();
            //        Values.Add(xSpace * multi);
            //        Values.Add(std);
            //        Values.Add(wtstd);
            //        aTable.AddRow(Values);

            //        xSpace = xSpace + 0.0625m;
            //        DoEvents();
            //    }
            //    aTable.SetWidth("All", 9);
            //    aTable.SetAlignment("All", "All", mza_TopCenter);
            //    aTable.SetTextFormat(1, "All", mzBold);
            //    aTable.SetNumberFormats("All", "All", "0.000");
            //    aTable.SetNumberFormats("All", 1, "0.0000");

            //    //.BordersAround Continous, Thin
            //    aTable = aTable;

            //    aGraph = aPage.AddGraph(uppGraphTypes.XYScatterSmooth, "Trays " + aPage.SpanName + " Downcomer Spacing Optimization Graph", "Chart(" + aPage.SpanName + ")", true);
            //    uopGraph _WithVar_aGraph;
            //    _WithVar_aGraph = aGraph;
            //    _WithVar_aGraph.XAxisTitle = "Downcomer Center To Center Spacing";
            //    _WithVar_aGraph.HasLegend = true;
            //    _WithVar_aGraph.MajorUnits = 0.25m;
            //    _WithVar_aGraph.MinimumScale = aTable.Cell(2, 1, False).Value - 0.0625m;
            //    _WithVar_aGraph.MaximumScale = aTable.Cell(aTable.Rows, 1, False).Value + 0.0625m;
            //    aSeries = _WithVar_aGraph.AddSeries("Std. Deviation", aTable.Name, 1, 2, 2, aTable.Rows);
            //    aSeries = _WithVar_aGraph.AddSeries("Wtd. Std. Deviation", aTable.Name, 1, 3, 2, aTable.Rows);
            //    aGraph = _WithVar_aGraph;
            //}

            //private static void xXF_Page_Test(ref uopDocReportPage aPage)
            //{
            //    //&Empty_Function&
            //    //^executed internally to populate the test page of an MD Spout Report
            //}

            //private static string xYesNo(ref dynamic aValue, ref bool bOneOrZero)
            //{
            //    string xYesNo = string.Empty;
            //    if (!bOneOrZero)
            //    {
            //        if (CBool(aValue))
            //        {
            //            xYesNo = "Y";
            //        }
            //        else
            //        {
            //            xYesNo = "N";
            //        }
            //    }
            //    else
            //    {
            //        if (CBool(aValue))
            //        {
            //            xYesNo = "1";
            //        }
            //        else
            //        {
            //            xYesNo = "0";
            //        }
            //    }

            //    return xYesNo;
        }

        #endregion Private Methods

    }
}
