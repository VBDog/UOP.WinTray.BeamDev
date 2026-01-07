using UOP.WinTray.Projects.Enums;

namespace UOP.WinTray.Projects
{
    /// <summary>
    /// uopGraph Model
    /// </summary>
    public class uopGraph
    {
        #region Constants

        private const decimal DEFAULTDECIMALVALUE = -999;

        #endregion

        #region Private Variables

        //private Collection oSeries = null;
        private string strTitle = string.Empty;
        private string strTabName = string.Empty;
        private string strXAxisTitle = string.Empty;
        private bool bSeperateSheet = false;
        private dynamic oChart = null;
        private bool bHasLegend = false;
        private decimal sngMajorUnits = 0;
        private decimal sngMinimumScale = 0;
        private decimal sngMaximumScale = 0;
        private uppGraphTypes enuChartType;

        #endregion

        #region Public Properties

        public dynamic Chart
        {
            get
            {
                return oChart;
            }
            set
            {
                oChart = value;
            }
        }
        public uppGraphTypes ChartType
        {
            get
            {
                return enuChartType;
            }
            set
            {
                enuChartType = value;
            }
        }
        public bool HasLegend
        {
            get
            {
                return bHasLegend;
            }
            set
            {
                bHasLegend = value;
            }
        }
        public decimal MajorUnits
        {
            get
            {
                return sngMajorUnits;
            }
            set
            {
                sngMajorUnits = value;
            }
        }
        public decimal MaximumScale
        {
            get
            {
                return sngMaximumScale;
            }
            set
            {
                sngMaximumScale = value;
            }
        }
        public decimal MinimumScale
        {
            get
            {
                return sngMinimumScale;
            }
            set
            {
                sngMinimumScale = value;
            }
        }
        public bool SeperateSheet
        {
            get
            {
                return bSeperateSheet;
            }
            set
            {
                bSeperateSheet = value;
            }
        }
        //public Collection Series
        //{
        //    get
        //    {
        //        if (oSeries == null)
        //        {
        //            oSeries = new Collection();
        //        }

        //        return oSeries;
        //    }
        //    set
        //    {
        //        oSeries = value;
        //    }
        //}
        public string TabName
        {
            get
            {
                return strTabName;
            }
            set
            {
                strTabName = value;
            }
        }
        public string Title
        {
            get
            {
                return strTitle;
            }
            set
            {
                strTitle = value;
            }
        }
        public string XAxisTitle
        {
            get
            {
                return strXAxisTitle;
            }
            set
            {
                strXAxisTitle = value;
            }
        }

        #endregion

        #region Constructor

        public uopGraph()
        {
            bSeperateSheet = true;
            sngMajorUnits = DEFAULTDECIMALVALUE;
            sngMinimumScale = DEFAULTDECIMALVALUE;
            sngMaximumScale = DEFAULTDECIMALVALUE;
            enuChartType = uppGraphTypes.XYScatter;
        }

        #endregion

        //TODO : To be checked
        //public uopGraphSeries AddSeries(ref string aName, ref string aTableName, ref int aXColumnIndex, ref int aColumnIndex, ref int aRowStart, ref int aRowEnd)
        //{
        //    uopGraphSeries AddSeries = null;
        //    AddSeries = new uopGraphSeries();
        //    uopGraphSeries _WithVar_AddSeries;
        //    _WithVar_AddSeries = AddSeries;
        //    _WithVar_AddSeries.Name = aName;
        //    _WithVar_AddSeries.TableName = aTableName;
        //    _WithVar_AddSeries.RowStart = aRowStart;
        //    _WithVar_AddSeries.RowEnd = aRowEnd;
        //    _WithVar_AddSeries.ColumnIndex = aColumnIndex;
        //    _WithVar_AddSeries.XColumnIndex = aXColumnIndex;
        //    AddSeries = _WithVar_AddSeries;

        //    Series.Add(AddSeries);
        //    return AddSeries;
        //}
    }
}
