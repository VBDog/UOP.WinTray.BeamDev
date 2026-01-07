using System;
using System.Collections.Generic;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Interfaces;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Tables;
using UOP.WinTray.Projects.Utilities;


namespace UOP.WinTray.Projects.Documents
{
    /// <summary>
    /// uopDocReportPage Model
    /// </summary>
    public class uopDocReportPage : uopDocument, ICloneable
    {
        
        #region Private Variables

        private List<List<iCase>> _Cases = null;
        private List<uopTable> _Tables = null;
        private List<uopDocDrawing> _Drawings = null;
        private List<uopGraph> _Graphs = null;
        

        #endregion


        #region Constructors

        public uopDocReportPage() : base(uppDocumentTypes.ReportPage) { }



        internal uopDocReportPage(uopDocReportPage aDocToCopy) : base(uppDocumentTypes.ReportPage)
        {
            
            base.Copy(aDocToCopy);
            if (aDocToCopy == null) return;
            _Cases = Force.DeepCloner.DeepClonerExtensions.DeepClone<List<List<iCase>>>(aDocToCopy._Cases);
            _Tables = Force.DeepCloner.DeepClonerExtensions.DeepClone<List<uopTable>>(aDocToCopy._Tables);
            _Drawings = Force.DeepCloner.DeepClonerExtensions.DeepClone<List<uopDocDrawing>>(aDocToCopy._Drawings);
            _Graphs = Force.DeepCloner.DeepClonerExtensions.DeepClone<List<uopGraph>>(aDocToCopy._Graphs);
          


        }
        internal uopDocReportPage(TDOCUMENT aDoc, mzQuestions aOptions = null, mzQuestions aTrayQuery = null) : base(uppDocumentTypes.ReportPage)
        {
            aDoc.DocumentType = uppDocumentTypes.ReportPage;
            Structure = aDoc.Clone();
            base.TrayQuery = aTrayQuery;
            base.Options = aOptions;

        }

        #endregion Constructors


        /// <summary>
        /// Get Cloned object
        /// </summary>
        /// <returns></returns>
        public uopDocReportPage Clone() => new uopDocReportPage(this);

        /// <summary>
        /// returns the Clone of the object
        /// </summary>
        public override uopDocument Clone(bool aFlag = false) => (uopDocument)this.Clone();

        /// <summary>
        /// returns the Clone of the object
        /// </summary>
        object ICloneable.Clone() => (object)this.Clone();

        #region Public Properties

        public uppReportTypes ReportType { get => (uppReportTypes)Props.ValueI("ReportType"); set => SetProp("ReportType", value); }

        public string TemplateTabName { get => Props.ValueS("TemplateTabName"); set => SetProp("TemplateTabName", value); }
        
        public string MemberList { get => Props.ValueS("MemberList"); set => SetProp("MemberList", value); }

        public string Tag { get => Props.ValueS("Tag"); set => SetProp("Tag", value); }
        
        public string TagAddress { get => Props.ValueS("TagAddress"); set => SetProp("TagAddress", value); }

        public bool OnePerRange { get => Props.ValueB("OnePerRange"); set => SetProp("OnePerRange", value); }

        public int MaxMembers { get => Props.ValueI("MaxMembers"); set => SetProp("MaxMembers", value); }

        public int StartIndex { get => Props.ValueI("StartIndex"); set => SetProp("StartIndex", value); }

        public int EndIndex { get => Props.ValueI("EndIndex"); set => SetProp("EndIndex", value); }
        
        public int CopyCount { get => Props.ValueI("CopyCount"); set => SetProp("CopyCount", value); }

        public int CopyIndex { get => Props.ValueI("CopyIndex"); set => SetProp("CopyIndex", value); }
        
        public List<uopDocDrawing> Drawings
        {
            get
            {
                _Drawings ??= new List<uopDocDrawing>();
                return _Drawings;
            }
            set => _Drawings = value;
            
        }
        
        public bool Flag { get => Props.ValueB("Flag"); set => SetProp("Flag", value); }

        /// <summary>
        /// True indicates the page is not a copy of an existing template page in the workbook
        /// but is expected to already be in the workbook template
        /// </summary>
        public bool NoTemplate { get => Props.ValueB("NoTemplate"); set => SetProp("NoTemplate", value); }
        
        /// <summary>
        /// True indicates the even if the page has a password
        /// the final page is protected without a password
        /// </summary>
        public bool DontSaveWithPassword { get => Props.ValueB("DontSaveWithPassword"); set => SetProp("DontSaveWithPassword", value); }
        /// <summary>
        /// True indicates that the worksheet created for the page will be set by excel and should not be assigned by the report generator
        /// </summary>
        public bool SuppressTabName { get => Props.ValueB("SuppressTabName"); set => SetProp("SuppressTabName", value); }

        public int GraphCount
        {
            get
            {
                if (Graphs.Count > 0 && Props.ValueI("GraphCount") != Graphs.Count)
                {
                    SetProp("GraphCount", _Graphs.Count);
                }
                return Props.ValueI("GraphCount");
            }
            set => SetProp("GraphCount", value);
        }
        
        public List<uopGraph> Graphs
        {
            get
            {
                if (_Graphs == null) _Graphs = new List<uopGraph>();
                return _Graphs;
            }
            set
            {
                _Graphs = value;
            }
        }
        
        public bool IsHidden { get => Hidden; set => Hidden = value; }

        /// <summary>
        /// true if the user has selected the page for inclusion in the report
        /// </summary>
        public bool IsRequested { get => Requested; set => Requested = value; }
        
        /// <summary>
        /// true if the report must include the page
        /// </summary>
        public bool IsRequired { get => Required; set => Required = value; }
        public List<List<iCase>> Cases
        {
            get
            {
                if (_Cases == null) _Cases = new List<List<iCase>>();
                return _Cases;
            }
            set => _Cases = value;
            
        }
      
        /// <summary>
        /// the name of the report page
        /// </summary>
        public string PageName { get => Name; set => Name = value.Trim(); }

        /// <summary>
        /// the type of report page
        /// </summary>
        public uppReportPageTypes PageType
        {
            get =>  (uppReportPageTypes)SubType;
            
            set
            {
                SubType = (int)value;
                if (value == uppReportPageTypes.MDChimneyTrayPage || value == uppReportPageTypes.MDDistributorPage || value == uppReportPageTypes.MDOptimizationPage || value == uppReportPageTypes.MDSpoutConstraintPage || value == uppReportPageTypes.ColumnSketchPage || value == uppReportPageTypes.MDSpoutDetailPage || value == uppReportPageTypes.ProjectSummaryPage || value == uppReportPageTypes.TraySummaryPage || value == uppReportPageTypes.WarningPage)
                {
                    Protected = true;
                }
                if (string.IsNullOrEmpty(Name)) Name = uopEnums.Description(PageType);

            }
        }
        /// <summary>
        /// returns the span name
        /// </summary>
        public string SpanName { get => RangeName; set => RangeName = value; }


        /// <summary>
        /// the first page sub title
        /// </summary>
        public string SubTitle1 { get => Structure.String2; set { TDOCUMENT str = Structure; str.String2 = value; Structure = str; } }
      
        public string TitleText
        {
            get
            {
                string titl = Title;
                if (!string.IsNullOrWhiteSpace(SubTitle1))
                {
                    if (!string.IsNullOrWhiteSpace(titl)) titl += " - ";
                    titl += SubTitle1;
                }

                if (!string.IsNullOrWhiteSpace(SubTitle2))
                {
                    if (!string.IsNullOrWhiteSpace(titl)) titl += " - ";
                    titl += SubTitle2;
                }
                return string.IsNullOrWhiteSpace(titl) ? TabName : $"{TabName} / {titl}";
            }
        }

        /// <summary>
        /// the second page sub title
        /// </summary>
        public string SubTitle2 { get => Structure.String3; set { TDOCUMENT str = Structure; str.String3 = value; Structure = str; } }
        
        public  new string TabName { get => Props.ValueS("TabName"); set => SetProp("TabName", value); }

        public List<uopTable> Tables
        {
            get
            {
                if (_Tables == null) _Tables = new List<uopTable>();
                return _Tables;
            }
            set =>  _Tables = value;
            
        }
        
        /// <summary>
        /// the page title
        /// </summary>
        public string Title { get => Structure.String1; set { TDOCUMENT str = Structure; str.String1 = value; Structure = str; } }

        public uppUnitFamilies Units { get => DisplayUnits; set =>DisplayUnits = value; }

        

        #endregion Properties



        #region Public Methods

        public override string ToString()
        {
            return $"uopDocReportPage[{PageType.GetDescription()}]";
        }

        //TODO : This code will be required in future stories so keeping it for now
        ///// <summary>
        ///// Add Graph to Report Page
        ///// </summary>
        ///// <param name="aType"></param>
        ///// <param name="aTitle"></param>
        ///// <param name="aTabName"></param>
        ///// <param name="aSepSheet"></param>
        ///// <returns></returns>
        //public uopGraph AddGraph(ref uppGraphTypes aType, ref string aTitle, ref string aTabName, ref bool aSepSheet)
        //{
        //    uopGraph AddGraph = new uopGraph
        //    {
        //        ChartType = aType,
        //        TabName = aTabName,
        //        Title = aTitle,
        //        SeperateSheet = aSepSheet
        //    };
        //    _Graphs.Add(AddGraph);
        //    return AddGraph;
        //}

        /// <summary>
        /// Add Table to Report Page
        /// </summary>
        /// <param name="aName"></param>
        /// <returns></returns>
        public uopTable AddTable(string aName, int aStartRow = 1, int aStartCol = 1)
        {
            uopTable table = new uopTable(aName) { DisplayUnits = Units , StartColumn = aStartCol, StartRow = aStartRow, Protected = Protected };
            Tables.Add(table);
            return Tables[Tables.Count - 1];
        }

        /// <summary>
        /// get the cells from the defined tables
        /// </summary>
        /// <param name="bAddressedOnly"></param>
        /// <returns></returns>
        public List<uopTableCell> DefinedCells(bool bAddressedOnly)
        {
            List<uopTableCell> _rVal = new List<uopTableCell>();
            foreach (var item in Tables)
            {
                item.GetCells(ref _rVal, bAddressedOnly);
            }
          
            return _rVal;
        }

        /// <summary>
        /// Lock table
        /// </summary>
        /// <param name="TableIndex"></param>
        public void LockTable(int TableIndex = 0)
        {
            uopTable aTable;
           
            if (TableIndex < 0 || TableIndex > Tables.Count)  TableIndex = 0;
            if (TableIndex > 0)
            {
                aTable = Tables[TableIndex - 1];
                aTable.SetLocks("All", "All", true);
            }
            else
            {
                for (int i = 0; i < Tables.Count; i++)
                {
                    aTable = Tables[i];
                    aTable.SetLocks("All", "All", true);
                }
            }
        }

        ///// <summary>
        ///// Get Table for Report
        ///// </summary>
        ///// <param name="aNameOrIndex"></param>
        ///// <returns></returns>
        //public uopTable Table(ref dynamic aNameOrIndex)
        //{
        //    uopTable Table = null;
        //    int idx = 0;

        //    uopTable aTable = null;

        //    if (aNameOrIndex is string || !mzUtils.IsNumeric(aNameOrIndex))
        //    {
        //        for (idx = 1; idx < Tables.Count; idx++)
        //        {
        //            aTable = _Tables[idx];
        //            if (aTable.Name == aNameOrIndex)
        //            {
        //                Table = aTable;
        //                break;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        idx = Convert.ToInt32(aNameOrIndex);
        //        Table = _Tables[idx];
        //    }
        //    return Table;
        //}

        #endregion

        internal override void Dispose(bool disposing)
        {

            base.Dispose(disposing);
            _Drawings?.Clear();
            _Graphs?.Clear();
            _Cases?.Clear();

            _Drawings = null;
            _Graphs = null;
            _Cases = null;
     
    
        }
    }
}



