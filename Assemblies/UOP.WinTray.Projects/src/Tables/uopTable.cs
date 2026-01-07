using System;
using System.Collections.Generic;
using UOP.DXFGraphics;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Utilities;


namespace UOP.WinTray.Projects.Tables
{
    /// <summary>
    /// uopTable Class
    /// </summary>
    public class uopTable : List<uopTableCell> // IEnumerable<uopTableCell>
    {
   

        #region Constructors

        public uopTable()
        {
            Init();
        }

        public uopTable(string aName)
        {
            Init();
            Name = string.IsNullOrWhiteSpace(aName) ? string.Empty : aName;
        }
        public uopTable(uppDisplayTableTypes aType)
        {
            Init();
            Name = aType.GetDescription();
            TableType = aType;
        }

        public uopTable(int aRowCount, int aColCount, string aName = "")
        {
            Init();
            Name = string.IsNullOrWhiteSpace(aName) ? string.Empty : aName;
            SetDimensions(aRowCount, aColCount);
        }
        private void Init()
        {
            
           
            StartColumn = 1;
            StartRow = 1;
            Name = string.Empty;
            FontSize = 8;
            FontName = "Arial";
        }
      
        #endregion Constructors

        #region Properties

        public bool Protected { get; set; }

        private int _Cols;
        /// <summary>
        /// the number ic currently define columns
        /// </summary>
        public int Cols => _Cols;

        private int _Rows;
        public int Rows => _Rows;


        public uppDisplayTableTypes TableType { get; set; }

        public uppProjectFamilies ProjectFamily { get; set; }
        public uppProjectTypes ProjectType { get; set; }

        public string Name { get; set; }
        public double FontSize { get; set; }
        public string FontName { get; set; }
        public uppPartTypes PartType { get; set; }
        private uppUnitFamilies _DisplayUnits = uppUnitFamilies.Undefined;
        public uppUnitFamilies DisplayUnits
        {
            get => _DisplayUnits;
            set => _DisplayUnits = value;
        }

       public int SelectedRow { get; set; }

        private int _StartColumn;
        public int StartColumn
        {
            get => _StartColumn;
            set { if (value >= 1) _StartColumn = value; }
        }

        private int _StartRow;

        public int StartRow
        {
            get => _StartRow;
            set { if (value >= 1)  _StartRow = value; }
        }

        private uopTableCell _CaptionCell = null;
        public uopTableCell CaptionCell
        {
            get
            {
                _CaptionCell ??= new uopTableCell(0,0,new uopProperty("Caption",""), bLocked: Protected);
                return _CaptionCell;
            }
        }

        private uopTableCell _LabelCell = null;
        public uopTableCell LabelCell
        {
            get
            {
                _LabelCell ??= new uopTableCell(0, 0, new uopProperty("Label", ""), bLocked: Protected);
                return _LabelCell;
            }
        }


        #endregion Properties



        #region Methods

        /// <summary>
        /// Add Column
        ///#2the values to put in the table
        ///#1the column position to insert the passed values before
        ///#3the value to replace values that equate to an empty string with
        ///#4a collection of text formats to apply to the column cells
        ///#5a collection of number formats to apply to the column cells
        ///^creates or populates the indicated column with the passed values
        ///~returns the affected cells
        /// </summary>
        /// <param name="aColumn"></param>
        /// <param name="aBeforeColumn"></param>
        /// <param name="aEmptyValReplacer"></param>
        /// <param name="aTextFormats"></param>
        /// <param name="aNumbersFormats"></param>
        /// <returns></returns>
        public List<uopTableCell> AddColumn(List<string> aDataSet, dynamic aEmptyValReplacer = null, List<uopAlignments> aAlignments = null, List<string> aNumbersFormats = null, List<mzTextFormats> aTextFormats = null, List<double> aCellWidths = null, List<double> aCellHeights = null, bool bApplyLastFormats = true, int? aBeforeColumn = null, bool? aLockVal = null)
        {

            List<uopTableCell> _rVal = new List<uopTableCell>();
            if (aDataSet == null || aDataSet.Count <= 0) return _rVal;

            int count = Cols;

            //expand to add rows id the passed values are bigger

            if (aDataSet.Count > Rows) SetDimensions(aDataSet.Count, Cols, aPreserve: true);

            int iC = count + 1;
            if (aBeforeColumn != null && aBeforeColumn.HasValue)
            {
                int before = aBeforeColumn.Value;
                if (before > 2 && before <= count)
                {
                    InsertColumn(before - 1);
                    iC = before - 1;
                }

            }


            _rVal = ColumnValuesSet(iC, aDataSet);
            ApplyFormats(_rVal, aAlignments, aNumbersFormats, aTextFormats, aCellWidths, aCellHeights, bApplyLastFormats, aEmptyValReplacer, aLockVal);

            return _rVal;
        }

        /// <summary>
        /// Add Column
        ///#2the values to put in the table
        ///#1the column position to insert the passed values before
        ///#3the value to replace values that equate to an empty string with
        ///#4a collection of text formats to apply to the column cells
        ///#5a collection of number formats to apply to the column cells
        ///^creates or populates the indicated column with the passed values
        ///~returns the affected cells
        /// </summary>
        /// <param name="aColumn"></param>
        /// <param name="aBeforeColumn"></param>
        /// <param name="aTextFormats"></param>
        /// <param name="aNumbersFormats"></param>
        /// <returns></returns>
        public List<uopTableCell> AddColumn(List<uopProperty> aDataSet, List<uopAlignments> aAlignments = null, List<string> aNumbersFormats = null, List<mzTextFormats> aTextFormats = null, List<double> aCellWidths = null, List<double> aCellHeights = null, bool bApplyLastFormats = true, int? aBeforeColumn = null, bool? aLockVal = null)
        {

            List<uopTableCell> _rVal = new List<uopTableCell>();
            if (aDataSet == null || aDataSet.Count <= 0) return _rVal;

            int count = Cols;


            //expand to add rows id the passed values are bigger

            if (aDataSet.Count > Rows) SetDimensions(aDataSet.Count, Cols, aPreserve: true);
            bool added = Cols > count;


            int iC = count + 1;
            if (aBeforeColumn != null && aBeforeColumn.HasValue)
            {
                int before = aBeforeColumn.Value;
                if (before > 2 && before <= count)
                {
                    InsertColumn(before - 1);
                    iC = before - 1;
                }

            }


            _rVal = ColumnValuesSet(iC, aDataSet);

            ApplyFormats(_rVal, aAlignments, aNumbersFormats, aTextFormats, aCellWidths, aCellHeights, bApplyLastFormats,aLockVal :aLockVal);


            return _rVal;
        }

        public List<uopTableCell> AddColumn(uopProperties aDataSet, List<uopAlignments> aAlignments = null, List<string> aNumbersFormats = null, List<mzTextFormats> aTextFormats = null, List<double> aCellWidths = null, List<double> aCellHeights = null, bool bApplyLastFormats = true, int? aBeforeColumn = null, bool? aLockVal = null)
        {

            List<uopTableCell> _rVal = new List<uopTableCell>();
            if (aDataSet == null || aDataSet.Count <= 0) return _rVal;
            return AddColumn(aDataSet.ToList, aAlignments, aNumbersFormats, aTextFormats, aCellWidths, aCellHeights, bApplyLastFormats, aBeforeColumn,aLockVal);

        }

        public List<uopTableCell> AddColumn(List<double> aDataSet, dynamic aEmptyValReplacer = null, List<uopAlignments> aAlignments = null, List<string> aNumbersFormats = null, List<mzTextFormats> aTextFormats = null, List<double> aCellWidths = null, List<double> aCellHeights = null, bool bApplyLastFormats = true, int? aBeforeColumn = null, bool? aLockVal = null)
        {

            List<uopTableCell> _rVal = new List<uopTableCell>();
            if (aDataSet == null || aDataSet.Count <= 0) return _rVal;

            int count = Cols;

            //expand to add rows id the passed values are bigger

            if (aDataSet.Count > Rows) SetDimensions(aDataSet.Count, Cols, aPreserve: true);

            int iC = count + 1;
            if (aBeforeColumn != null && aBeforeColumn.HasValue)
            {
                int before = aBeforeColumn.Value;
                if (before > 2 && before <= count)
                {
                    InsertColumn(before - 1);
                    iC = before - 1;
                }

            }


            _rVal = ColumnValuesSet(iC, aDataSet);
            ApplyFormats(_rVal, aAlignments, aNumbersFormats, aTextFormats, aCellWidths, aCellHeights, bApplyLastFormats, aEmptyValReplacer, aLockVal);

            return _rVal;
        }


        public List<uopTableCell> AddRow(List<string> aDataSet, dynamic aEmptyValReplacer = null, List<uopAlignments> aAlignments = null, List<string> aNumbersFormats = null, List<mzTextFormats> aTextFormats = null, List<double> aCellWidths = null, List<double> aCellHeights = null, bool bApplyLastFormats = true, int? aBeforeRow = null, bool? aLockVal = null)
        {

            List<uopTableCell> _rVal = new List<uopTableCell>();
            if (aDataSet == null || aDataSet.Count <= 0) return _rVal;

            int count = Rows;

            //expand to add columns id the passed values are bigger

            if (aDataSet.Count > Cols) 
                SetDimensions(Cols, aDataSet.Count, aPreserve: true);

            int iR = count + 1;
            if (aBeforeRow != null && aBeforeRow.HasValue)
            {
                int before = aBeforeRow.Value;
                if (before > 2 && before <= count)
                {
                    InsertRow(before - 1);
                    iR = before - 1;
                }

            }
            
            _rVal = RowValuesSet(iR, aDataSet);
            ApplyFormats(_rVal, aAlignments, aNumbersFormats, aTextFormats, aCellWidths, aCellHeights, bApplyLastFormats, aEmptyValReplacer, aLockVal);

            return _rVal;
        }

        public List<uopTableCell> AddRow(List<double> aDataSet, dynamic aEmptyValReplacer = null, List<uopAlignments> aAlignments = null, List<string> aNumbersFormats = null, List<mzTextFormats> aTextFormats = null, List<double> aCellWidths = null, List<double> aCellHeights = null, bool bApplyLastFormats = true, int? aBeforeRow = null, bool? aLockVal = null)
        {

            List<uopTableCell> _rVal = new List<uopTableCell>();
            if (aDataSet == null || aDataSet.Count <= 0) return _rVal;

            int count = Rows;

            //expand to add columns id the passed values are bigger

            if (aDataSet.Count > Cols) SetDimensions(Cols, aDataSet.Count, aPreserve: true);

            int iR = count + 1;
            if (aBeforeRow != null && aBeforeRow.HasValue)
            {
                int before = aBeforeRow.Value;
                if (before > 2 && before <= count)
                {
                    InsertRow(before - 1);
                    iR = before - 1;
                }

            }


            _rVal = RowValuesSet(iR, aDataSet);
            ApplyFormats(_rVal, aAlignments, aNumbersFormats, aTextFormats, aCellWidths, aCellHeights, bApplyLastFormats, aEmptyValReplacer, aLockVal);

            return _rVal;
        }
        public List<uopTableCell> AddRow(uopProperties aDataSet, List<uopAlignments> aAlignments = null, List<string> aNumbersFormats = null, List<mzTextFormats> aTextFormats = null, List<double> aCellWidths = null, List<double> aCellHeights = null, bool bApplyLastFormats = true, int? aBeforeRow = null, bool? aLockVal = null)
        {
            List<uopTableCell> _rVal = new List<uopTableCell>();
            if (aDataSet == null || aDataSet.Count <= 0) return _rVal;
            return AddRow(aDataSet.ToList, aAlignments, aNumbersFormats, aTextFormats, aCellWidths, aCellHeights, bApplyLastFormats, aBeforeRow, aLockVal);
        }

        public List<uopTableCell> AddRow(List<uopProperty> aDataSet, List<uopAlignments> aAlignments = null, List<string> aNumbersFormats = null, List<mzTextFormats> aTextFormats = null, List<double> aCellWidths = null, List<double> aCellHeights = null, bool bApplyLastFormats = true, int? aBeforeRow = null, bool? aLockVal = null)
        {

            List<uopTableCell> _rVal = new List<uopTableCell>();
            if (aDataSet == null || aDataSet.Count <= 0) return _rVal;

            int count = Rows;


            //expand to add cols if the passed values are bigger

            if (aDataSet.Count > Cols) 
                SetDimensions(count, aDataSet.Count, aPreserve: true);
           
            int iR = count + 1;
            if (aBeforeRow != null && aBeforeRow.HasValue)
            {
                int before = aBeforeRow.Value;
                if (before > 2 && before <= count)
                {
                    InsertRow(before - 1);
                    iR = before - 1;
                }

            }


            _rVal = RowValuesSet(iR, aDataSet);

            ApplyFormats(_rVal, aAlignments, aNumbersFormats, aTextFormats, aCellWidths, aCellHeights, bApplyLastFormats, aLockVal:aLockVal);


            return _rVal;
        }


        /// <summary>
        /// Add Column
        ///#2the values to put in the table
        ///#1the column position to insert the passed values before
        ///#3the value to replace values that equate to an empty string with
        ///#4a collection of text formats to apply to the column cells
        ///#5a collection of number formats to apply to the column cells
        ///^creates or populates the indicated column with the passed values
        ///~returns the affected cells
        /// </summary>
        /// <param name="aCells"></param>
        /// <param name="aAlignments"></param>
        /// <param name="aNumbersFormats"></param>
        /// <param name="aTextFormats"></param>
        /// <param name="aCellWidths"></param>
        /// <param name="aCellHeights"></param>
        /// <param name="bApplyLastFormats"></param>
        /// <returns></returns>
        public void ApplyFormats(List<uopTableCell> aCells, List<uopAlignments> aAlignments = null, List<string> aNumbersFormats = null, List<mzTextFormats> aTextFormats = null, List<double> aCellWidths = null, List<double> aCellHeights = null, bool bApplyLastFormats = false, dynamic aEmptyValReplacer = null, bool? aLockVal = null)
        {


            if (aCells == null || aCells.Count <= 0) return;

            bool replace = aEmptyValReplacer != null;
            uopTableCell aCell;

            for (int iR = 1; iR <= aCells.Count; iR++)
            {
                aCell = aCells[iR - 1];

                if (aLockVal.HasValue) aCell.Locked = aLockVal.Value;
                if (replace)
                {
                    if (string.IsNullOrWhiteSpace(aCell.ValueS)) aCell.Value = aEmptyValReplacer;
                }

                if (aTextFormats != null)
                {
                    if (iR <= aTextFormats.Count)
                    {
                        aCell.TextFormat = aTextFormats[iR - 1];
                    }
                    else
                    {
                        if (aTextFormats.Count > 0 && bApplyLastFormats) aCell.TextFormat = aTextFormats[aTextFormats.Count - 1];
                    }

                }


                if (aNumbersFormats != null)
                {
                    if (iR <= aNumbersFormats.Count)
                    {
                        aCell.NumberFormat = aNumbersFormats[iR - 1];
                    }
                    else
                    {
                        if (aNumbersFormats.Count > 0 && bApplyLastFormats) aCell.NumberFormat = aNumbersFormats[aNumbersFormats.Count - 1];
                    }


                }


                if (aAlignments != null)
                {
                    if (iR <= aAlignments.Count)
                    {
                        aCell.Alignment = aAlignments[iR - 1];
                    }
                    else
                    {
                        if (aAlignments.Count > 0 && bApplyLastFormats) aCell.Alignment = aAlignments[aAlignments.Count - 1];
                    }

                }

                if (aCellWidths != null)
                {
                    if (iR <= aCellWidths.Count)
                    {
                        aCell.Width = aCellWidths[iR - 1];

                    }
                    else
                    {
                        if (aCellWidths.Count > 0 && bApplyLastFormats) aCell.Width = aCellWidths[aCellWidths.Count - 1];
                    }

                }


                if (aCellHeights != null)
                {
                    if (iR <= aCellHeights.Count)
                    {
                        aCell.Height = aCellHeights[iR - 1];
                    }
                    else
                    {
                        if (aCellHeights.Count > 0 && bApplyLastFormats) aCell.Height = aCellHeights[aCellHeights.Count - 1];
                    }

                }

            }

        }

       /// <summary>
        ////#1the index of the column to insert a column at
        ///^inserts a new column to the left of the passed index
        ///~if the passed index is greater than the current number of columns the new column is inserted at the end
        /// </summary>
        /// <param name="aBeforeIndex"></param>
        /// <param name="aInitValue"></param>
        /// <returns></returns>
        public List<uopTableCell> InsertColumn(dynamic aBeforeIndex, dynamic aInitValue = null)
        {
            List<uopTableCell> _rVal = null;
            List<uopTableCell> aCopyCol = null;
            int iC = 0;
            int iR = 0;
            int idx = 0;
            uopTableCell aCell = null;
            uopTableCell bCell = null;
            bool bInitVal = false;
            dynamic aVal = null;

            idx = mzUtils.VarToInteger(aBeforeIndex);
            if (idx <= 0) idx = 1;
            
            _rVal = new List<uopTableCell>();
            if (Rows < 1)  return _rVal;
         
            bInitVal = aInitValue != null;
            if (bInitVal)
            {
                aVal = aInitValue ?? string.Empty;
                
            }

            if (idx > Cols)
            {
                aCopyCol = Column(Cols);
                SetDimensions(Rows, Cols + 1);
                for (iR = 1; iR < aCopyCol.Count; iR++)
                {
                    aCell = aCopyCol[iR - 1];
                    bCell = Cell(iR, Cols);

                    bCell.Copy(aCell);
                    if (bInitVal)
                    {
                        bCell.Value = aVal;
                    }
                }
                _rVal = Column(Cols);
            }
            else
            {
                aCopyCol = Column(idx, true);
                SetDimensions(Rows, Cols + 1);

                for (iR = 1; iR <= Rows; iR++)
                {
                    for (iC = Cols; iC >= idx + 1; iC--)
                    {
                        aCell = Cell(iR, iC);
                        bCell = Cell(iR, iC - 1);
                        aCell.Copy(bCell);
                    }
                }

                for (iR = 1; iR <= Rows; iR++)
                {
                    aCell = aCopyCol[iR - 1];
                    bCell = Cell(iR, idx);

                    bCell.Copy(aCell);
                    if (bInitVal)
                    {
                        bCell.Value = aVal;
                    }
                }
            }
            return _rVal;
        }
        /// <summary>
        /// Column
        /// </summary>
        /// <param name="aCol"></param>
        /// <param name="bGetClones"></param>
        /// <returns></returns>
        public List<uopTableCell> Column(int aCol, bool bGetClones = false)
        {
            List<uopTableCell> _rVal =  new List<uopTableCell>();
            int iC = mzUtils.VarToInteger(aCol);
            foreach (uopTableCell item in this)
            {
                if (item.Col == iC)
                {
                    if (bGetClones)
                        _rVal.Add(item.Clone());
                    else
                        _rVal.Add(item);
                  
                }
            }
         
            return _rVal;
        }

        /// <summary>
        /// Row
        /// </summary>
        /// <param name="aRow"></param>
        /// <param name="bGetClones"></param>
        /// <returns></returns>
        public uopProperties PropertyRow(int aRow, bool bGetClones = false)
        {
            uopProperties _rVal = new uopProperties($"ROW {aRow}"); // { Name = $"ROW {aRow}" };
            List<uopTableCell> row = Row(aRow, false);
            foreach (var item in row)
            {
                if (bGetClones)
                    _rVal.Add(item.Property.Clone());
                else
                    _rVal.Add(item.Property);
            }
           
            return _rVal;
        }

        /// <summary>
        /// Row
        /// </summary>
        /// <param name="aRow"></param>
        /// <param name="bGetClones"></param>
        /// <returns></returns>
        public List<uopTableCell> Row(int aRow, bool bGetClones = false)
        {

            List<uopTableCell> _rVal = new List<uopTableCell>();
            foreach (var item in this)
            {
                if (item.Row == aRow)
                {
                    if (bGetClones)
                        _rVal.Add(item.Clone());
                    else
                        _rVal.Add(item);
                  
                }
            }

            return _rVal;
        }


        /// <summary>
        /// Removes the indicated row of cells and reindexs the rows
        /// </summary>
        /// <param name="aRow"></param>
        /// <returns>the removed cells</returns>
        public List<uopTableCell> RemoveRow(int aRow)
        {

            List<uopTableCell> _rVal = Row(aRow);
            if(_rVal.Count <=0) return _rVal;

            int mxrow = 0;
            foreach (var item in this)
            {
                if (item.Row != aRow)
                {
                    if (item.Row > aRow) item.Row -= 1;
                    if (item.Row > mxrow) mxrow = item.Row;
                }
            }
            _Rows = mxrow;
            foreach (var item in _rVal)
            {
                Remove(item);
            }


            return _rVal;
        }

        /// <summary>
        /// Removes the indicated column of cells and reindexs the collums
        /// </summary>
        /// <param name="aCol"></param>
        /// <returns>the removed cells</returns>
        public List<uopTableCell> RemoveCol(int aCol)
        {

            List<uopTableCell> _rVal = Column(aCol);
            if (_rVal.Count <= 0) return _rVal;

            int mxcol = 0;
            foreach (var item in this)
            {
                if (item.Col != aCol)
                {
                    if (item.Col > aCol) item.Col -= 1;
                    if (item.Col > mxcol) mxcol = item.Col;
                }
            }
            _Cols = mxcol;
            foreach (var item in _rVal)
            {
                Remove(item);
            }


            return _rVal;
        }


        /// <summary>
        ///#1the index of the row to insert a row at
        ///^inserts a new row to the left of the passed index
        ///~if the passed index is greater than the current number of columns the new row is inserted at the end 
        /// </summary>
        /// <param name="aBeforeIndex"></param>
        /// <param name="aInitValue"></param>
        /// <returns></returns>
        public List<uopTableCell> InsertRow(int aBeforeIndex, dynamic aInitValue = null)
        {

            List<uopTableCell> aCopyRow;
         
            int idx = 0;
            uopTableCell aCell;
            uopTableCell bCell;
            bool bInitVal = aInitValue != null;
            dynamic aVal = aInitValue ?? "";

            idx = mzUtils.VarToInteger(aBeforeIndex, true,Rows + 1,1,Rows+1);
            if (idx <= 0) idx = 1;

            List<uopTableCell> _rVal = new List<uopTableCell>(); ;
            if (Cols < 1) return _rVal;
          
          
            if (idx > Rows)
            {
                //add a new row and copy the settings from the previous row
                aCopyRow = Row(Rows);
                SetDimensions(Rows + 1, Cols);
                for (int iC = 1; iC < aCopyRow.Count; iC++)
                {
                    aCell = aCopyRow[iC];
                    bCell = Cell(Rows, iC);

                    bCell.Copy(aCell);
                    bCell.Value = aVal;
                    
                }
                _rVal = Row(Rows);

            }
            else
            {
                
                aCopyRow = Row(idx, true);
                SetDimensions(Rows + 1, Cols);
                for (int iR = Rows; iR >= idx + 1 ; iR += -1)
                {
                    for (int iC = 1; iC < Cols; iC++)
                    {
                        aCell = Cell(iR, iC);
                        bCell = Cell(iR - 1, iC);
                        aCell.Copy(bCell);
                    }
                }
                _rVal = Row(idx);

                for (int iC = 1; iC < Cols; iC++)
                {
                    aCell = aCopyRow[iC - 1];
                    bCell = Cell(idx, iC);

                    bCell.Copy(aCell);
                    if (bInitVal)
                    {
                        bCell.Value = aVal;
                    }
                }

               
            }
            return _rVal;
        }

        /// <summary>
        /// creates a row or column the passed values
        /// #1a flag to add a row or a column
        ///#2a string containing values to put in the table
        ///#3the value to replace values that equate to an empty string with
        ///#4the delimitor that seperates values in the passed string
        ///^creates a row or column the passed values
        /// </summary>
        /// <param name="bAddColumn">a flag to add a row or a column</param>
        /// <param name="aDataString">a string containing values to put in the table</param>
        /// <param name="aEmptyValReplacer">the value to replace values that equate to an empty string with</param>
        /// <param name="aDelimitor">the delimitor that seperates values in the passed string</param>
        /// <param name="aBeforeIndex"></param>
        /// <returns></returns>
        public List<uopTableCell> AddByString(bool bAddColumn, string aDataString, string aEmptyValReplacer = "", string aDelimitor = "|", int aBeforeIndex = 0)
        {

            List<string> aVals = mzUtils.StringsFromDelimitedList(aDataString, aDelimitor: aDelimitor, bReturnNulls: true);
            return (!bAddColumn) ? AddRow(aVals, aBeforeRow: aBeforeIndex, aEmptyValReplacer: aEmptyValReplacer) : AddColumn(aVals, aBeforeColumn: aBeforeIndex, aEmptyValReplacer: aEmptyValReplacer);

           
        }

        /// <summary>
        /// sets the tables overall height(rows)and width(cols) to the passed size
        /// </summary>
        /// <param name="aRowCount">the numbers of rows to set the tables to</param>
        /// <param name="aColumnCount">the numbers of columns to set the tables to</param>
        /// <param name="aColumnWidths">a collection of width to apply to the columns</param>
        /// <param name="aPreserve">flag to preserve the current cell values and settings for current cells already in the table</param>
        /// <param name="bDataOnly"></param>
        /// <param name="bNumeric"></param>
        /// <param name="aInitValue"></param>
        public void SetDimensions(int aRowCount, int aColumnCount, string aColumnWidths = null, bool aPreserve = true, bool? bDataOnly = null, bool? bNumeric = null, dynamic aInitValue = null)
        {
            aRowCount = mzUtils.VarToInteger(aRowCount, true, null, 1);
            aColumnCount = mzUtils.VarToInteger(aColumnCount, true, null, 1);
            uopTableCell aCell;
            if (!aPreserve)
            {
                Clear();
                for (int m = 1; m <= aRowCount; m++)
                {
                    for (int n = 1; n <= aColumnCount; n++)
                    {
                        aCell = new uopTableCell(m, n, new uopProperty($"CELL({Count + 1})", aInitValue), bLocked: Protected);
                        if (bDataOnly.HasValue)  aCell.DataOnly = bDataOnly.Value;
                        if (bNumeric.HasValue) aCell.Numeric = bNumeric.Value;
                       
                        Add(aCell);
                    }
                }
            }
            else
            {
                List<uopTableCell> nCells = new List<uopTableCell>();
                //if (aRowCount < Rows) aRowCount = Rows;
                //if (aColumnCount < Cols)  aColumnCount = Cols;
               
                for (int m = 1; m <= aRowCount; m++)
                {
                    for (int n = 1; n <= aColumnCount; n++)
                    {
                        aCell = Cell(m, n);
                        if (aCell == null)
                        {
                            aCell = new uopTableCell(m, n, new uopProperty($"CELL({Count + 1})", aInitValue), bLocked: Protected);
                            if (bDataOnly.HasValue) aCell.DataOnly = bDataOnly.Value;
                            if (bNumeric.HasValue) aCell.Numeric = bNumeric.Value;

                        }
                     
                        nCells.Add(aCell);
                    }
                }
                Clear();
                AddRange(nCells);
            }
            _Rows = aRowCount;
            _Cols = aColumnCount;

            if (!string.IsNullOrEmpty(aColumnWidths))
            {
                SetWidth("ALL", aColumnWidths);
            }
        }
        /// <summary>
        /// Define By String
        /// </summary>
        /// <param name="v"></param>
        /// <param name="i"></param>
        /// <param name="tlist"></param>
        internal List<uopTableCell> DefineByString(bool bDefineColumn, dynamic aRowOrColIndex, string aDataString, bool bIgnoreNullStrings = false, string aDelimitor = "|", string aSkipList = null)
        {
            // #1a flag to define a row or a column
            // #2the index of the row or column define
            // #3a string containing values to put in the table
            // #4flag to only transfer values that do not equate to a null string
            // #5the delimitor that seperates values in the passed string
            // #6a comma delimited list of indexs of items in the list to ignore
            // ^creates a row or column the passed values

            mzValues aVals;
            int idx;
            dynamic aVal;
            uopTableCell aCell;

            List<uopTableCell> defineByString = null;

            idx = mzUtils.VarToInteger(aRowOrColIndex);
            if (idx <= 0)
            {
                return defineByString;
            }
            aVals = new mzValues();
            aVals.AddByString(aDataString, aDelimitor: aDelimitor, bReturnNulls: true, aSkipList: aSkipList);

            if (bDefineColumn)
            {
                if (aVals.Count > Rows || idx > Cols)
                {
                    SetDimensions(aVals.Count, idx);
                }
                defineByString = Column(idx);
            }
            else
            {
                if (aVals.Count > Cols || idx > Rows)
                {
                    SetDimensions(idx, aVals.Count);
                }
                defineByString = Row(idx);
            }

            for (int i = 1; i <= aVals.Count; i++)
            {
                aVal = aVals.Item(i);
                if (i > defineByString.Count)
                {
                    break;
                }
                if (!bIgnoreNullStrings || (bIgnoreNullStrings && !string.IsNullOrWhiteSpace(((string)aVal).Trim())))
                {
                    aCell = defineByString[i - 1];
                    aCell.Value = aVal;
                }
            }

            return defineByString;
        }

        /// <summary>
        /// assigns the passed lock value to cells in the indicated cells
        /// passing "0" or "ALL"  or "" means to apply to all
        /// a range can be passed in the form 1-7 etc.
        /// </summary>
        /// <param name="aRowList">comma delimited list of row indexes</param>
        /// <param name="aColList">comma delimited list of column indexes</param>
        /// <param name="aLockValue">the lock value to apply</param>
        public List<uopTableCell> SetLocks(string aRowList, string aColList, bool aLockValue)
        {
            List<uopTableCell> _rVal  = GetCellsByList(aRowList, aColList);
            foreach (var item in _rVal) { item.Locked = aLockValue; }
            return _rVal;
        }
        /// <summary>
        /// assigns the passed lock value to cells in the indicated cells
        /// passing "0" or "ALL"  or "" means to apply to all
        /// a range can be passed in the form 1-7 etc.
        /// </summary>
        /// <param name="aRow">a row index</param>
        /// <param name="aCol">a column index</param>
        /// <param name="aLockValue">the lock value to apply</param>
        public List<uopTableCell> SetLocks(List<int> aRow, List<int> aCol, bool aLockValue)
        {
            List<uopTableCell> _rVal = GetCellsByList(aRow, aCol);
            foreach (var item in _rVal) { item.Locked = aLockValue; }
            return _rVal;
        }
        /// <summary>
        /// assigns the passed lock value to cells in the indicated cells
        /// passing "0" or "ALL"  or "" means to apply to all
        /// a range can be passed in the form 1-7 etc.
        /// </summary>
        /// <param name="aRow">a row index</param>
        /// <param name="aCol">a column index</param>
        /// <param name="aLockValue">the lock value to apply</param>
        public List<uopTableCell> SetLocks(int aRow, int aCol, bool aLockValue)
        {
            List<uopTableCell> _rVal = GetCellsByList(aRow, aCol);
            foreach (var item in _rVal) { item.Locked = aLockValue; }
            return _rVal;
        }

        /// <summary>
        /// assigns the passed width to the indicated cells
        /// passing "0" or "ALL"  or "" means to apply to all
        /// a range can be passed in the form 1-7 etc.
        /// the width can be a single calue or a list of values.
        /// if alist of values is passed the values are asummed to be in column order.
        /// </summary>
        /// <param name="aColList">comma delimited list of column indexes</param>
        /// <param name="aWidthList">width to apply</param>
        /// <param name="aRowList">comma delimited list of row indexes</param>
        /// <param name="bSnapToLast">flag to use the last with in the list for all cell whose column index is greater the the list length</param>
        /// <returns></returns>
        public List<uopTableCell> SetWidth(string aColList, string aWidthList, string aRowList = "ALL", bool bSnapToLast = true)
        {

            return SetWidth(aColList, mzUtils.ListToNumericCollection(aWidthList, ",", true), aRowList, bSnapToLast);
        }

        public List<uopTableCell> SetWidth(int aColIndex, double aWidth, int aRowIndex = 0)
        {
         
            List<uopTableCell> _rVal = GetCellsByList(aRowIndex, aColIndex);
            foreach (var item in _rVal){ item.Width = aWidth; }

            return _rVal;
        }


        public List<uopTableCell> SetWidth(List<int> aColIndex, double aWidth, List<int> aRowIndex = null)
        {

            List<uopTableCell> _rVal = GetCellsByList(aRowIndex, aColIndex);
            foreach (var item in _rVal) { item.Width = aWidth; }

            return _rVal;
        }
        /// <summary>
        /// assigns the passed width to the indicated cells
        /// passing "0" or "ALL"  or "" means to apply to all
        /// a range can be passed in the form 1-7 etc.
        /// the width can be a single calue or a list of values.
        /// if alist of values is passed the values are asummed to be in column order.
        /// </summary>
        /// <param name="aColList">comma delimited list of column indexes</param>
        /// <param name="aWidthList">width to apply</param>
        /// <param name="aRowList">comma delimited list of row indexes</param>
        /// <param name="bSnapToLast">flag to use the last with in the list for all cell whose column index is greater the the list length</param>
        /// <returns></returns>
        public List<uopTableCell> SetWidth(string aColList, List<double> aWidthList, string aRowList = "All", bool bSnapToLast = true)
        {
            if (aWidthList == null) return new List<uopTableCell>();
            if (aWidthList.Count <= 0) return new List<uopTableCell>();

            List<uopTableCell> _rVal = GetCellsByList(aRowList, aColList);
            foreach (var item in _rVal)
            {
                if (item.Col <= aWidthList.Count)
                {
                    item.Width = aWidthList[item.Col - 1];
                }
                else if (bSnapToLast)
                {
                    item.Width = aWidthList[aWidthList.Count - 1];
                }

            }
            return _rVal;
           
        }
        /// <summary>
        /// assigns the passed height to cells in the indicated row passing "0" or "ALL"  or "" means to apply to all
        /// a range can be passed in the form 1-7 etc.
        /// </summary>
        /// <param name="aRowList">a comma delimited list of row indexes</param>
        /// <param name="aHeight">a Height to a apply</param>
        /// <param name="aColList">a comma delimited list of column indexes</param>
        /// <returns></returns>
        public List<uopTableCell> SetHeight(string aRowList, double aHeight, string aColList = "All")
        {
         
            if (aHeight <= 0) return new List<uopTableCell>();

            List<uopTableCell> _rVal = GetCellsByList(aRowList, aColList);
            foreach (var item in _rVal) { item.Height = aHeight; }

            return _rVal;
        }


        /// <summary>
        /// assigns the passed height to cells in the indicated row passing "0" or "ALL"  or "" means to apply to all
        /// a range can be passed in the form 1-7 etc.
        /// </summary>
        /// <param name="aRow">the tagrget row</param>
        /// <param name="aHeight">a Height to a apply</param>
        /// <param name="aColList">a comma delimited list of column indexes</param>
        /// <returns></returns>
        public List<uopTableCell> SetHeight(int aRowIndex, double aHeight, int? aColIndex = null)
        {
            List<uopTableCell> _rVal = GetCellsByList(aRowIndex, aColIndex);
            foreach (var item in _rVal) { item.Height = aHeight; }
            return _rVal;
        }



        /// <summary>
        /// assigns the passed height to cells in the indicated row passing "0" or "ALL"  or "" means to apply to all
        /// a range can be passed in the form 1-7 etc.
        /// </summary>
        /// <param name="aRow">the tagrget row</param>
        /// <param name="aHeight">a Height to a apply</param>
        /// <param name="aColList">a comma delimited list of column indexes</param>
        /// <returns></returns>
        public List<uopTableCell> SetHeight(List<int> aRowList, double aHeight, List<int> aColList = null)
        {
            List<uopTableCell> _rVal = GetCellsByList(aRowList, aColList);
            foreach (var item in _rVal) { item.Height = aHeight; }
            return _rVal;
        }

        public List<uopTableCell> SetRowHeights(List<double> aValues, bool bSkipLessThanZero = true, bool bContinueWithLastValue = false)
        {

            List<uopTableCell> _rVal = new List<uopTableCell>();
            if (aValues == null) return _rVal;

            for(int i = 1; i <= aValues.Count; i++)
            {
                if (i > Rows) break;
                double dVal = aValues[i - 1];
                if (bSkipLessThanZero && dVal < 0) continue;
                List<uopTableCell> cells = FindAll(x => x.Row == i);
                foreach (uopTableCell cell in cells)
                {
                    cell.Height = dVal;
                    _rVal.Add(cell);
                }
                
            }
            if (aValues.Count < Rows && bContinueWithLastValue)
            {
                double dVal = aValues[aValues.Count - 1];
                if (bSkipLessThanZero && dVal < 0) return _rVal;
                for (int i = aValues.Count + 1; i <= Rows; i++)
                {
                    List<uopTableCell> cells = FindAll(x => x.Row == i);
                    foreach (uopTableCell cell in cells)
                    {
                        cell.Height = dVal;
                        _rVal.Add(cell);
                    }
                }

            }


            return _rVal;

        }

        public List<uopTableCell> SetColWidths(List<double> aValues, bool bSkipLessThanZero = true, bool bContinueWithLastValue = false)
        {

            List<uopTableCell> _rVal = new List<uopTableCell>();
            if (aValues == null) return _rVal;

            for (int i = 1; i <= aValues.Count; i++)
            {
                if (i > Cols) break;
                
                double dVal = aValues[i - 1];
              
                List<uopTableCell> cells = FindAll(x => x.Col == i);
                foreach (uopTableCell cell in cells)
                {
                    cell.Width = dVal;
                    _rVal.Add(cell);
                }

            }
            if(aValues.Count < Cols && bContinueWithLastValue)
            {
                double dVal = aValues[aValues.Count - 1];
                if (bSkipLessThanZero && dVal < 0)  return _rVal;
                for(int i = aValues.Count + 1; i <= Cols; i++)
                {
                    List<uopTableCell> cells = FindAll(x => x.Col == i);
                    foreach (uopTableCell cell in cells)
                    {
                        cell.Width = dVal;
                        _rVal.Add(cell);
                    }
                }

            }

            return _rVal;

        }

        public List<uopTableCell> SetColTextFormats(List<mzTextFormats> aValues, bool bContinueWithLastValue = false)
        {

            List<uopTableCell> _rVal = new List<uopTableCell>();
            if (aValues == null) return _rVal;

            for (int i = 1; i <= aValues.Count; i++)
            {
                if (i > Cols) break;

                mzTextFormats dVal = aValues[i - 1];

                List<uopTableCell> cells = FindAll(x => x.Col == i);
                foreach (uopTableCell cell in cells)
                {
                    cell.TextFormat = dVal;
                    _rVal.Add(cell);
                }

            }
            if (aValues.Count < Cols && bContinueWithLastValue)
            {
                mzTextFormats dVal = aValues[aValues.Count - 1];
              
                for (int i = aValues.Count + 1; i <= Cols; i++)
                {
                    List<uopTableCell> cells = FindAll(x => x.Col == i);
                    foreach (uopTableCell cell in cells)
                    {
                        cell.TextFormat = dVal;
                        _rVal.Add(cell);
                    }
                }

            }

            return _rVal;

        }

        public List<uopTableCell> SetRowTextFormats(List<mzTextFormats> aValues, bool bContinueWithLastValue = false)
        {

            List<uopTableCell> _rVal = new List<uopTableCell>();
            if (aValues == null) return _rVal;

            for (int i = 1; i <= aValues.Count; i++)
            {
                if (i > Rows) break;

                mzTextFormats dVal = aValues[i - 1];

                List<uopTableCell> cells = FindAll(x => x.Row == i);
                foreach (uopTableCell cell in cells)
                {
                    cell.TextFormat = dVal;
                    _rVal.Add(cell);
                }

            }
            if (aValues.Count < Rows && bContinueWithLastValue)
            {
                mzTextFormats dVal = aValues[aValues.Count - 1];

                for (int i = aValues.Count + 1; i <= Rows; i++)
                {
                    List<uopTableCell> cells = FindAll(x => x.Row == i);
                    foreach (uopTableCell cell in cells)
                    {
                        cell.TextFormat = dVal;
                        _rVal.Add(cell);
                    }
                }

            }

            return _rVal;

        }

        public List<uopTableCell> SetColTextAlignments(List<uopAlignments> aValues, bool bContinueWithLastValue = false)
        {

            List<uopTableCell> _rVal = new List<uopTableCell>();
            if (aValues == null) return _rVal;

            for (int i = 1; i <= aValues.Count; i++)
            {
                if (i > Cols) break;

                uopAlignments dVal = aValues[i - 1];

                List<uopTableCell> cells = FindAll(x => x.Col == i);
                foreach (uopTableCell cell in cells)
                {
                    cell.Alignment = dVal;
                    _rVal.Add(cell);
                }

            }
            if (aValues.Count < Cols && bContinueWithLastValue)
            {
                uopAlignments dVal = aValues[aValues.Count - 1];

                for (int i = aValues.Count + 1; i <= Cols; i++)
                {
                    List<uopTableCell> cells = FindAll(x => x.Col == i);
                    foreach (uopTableCell cell in cells)
                    {
                        cell.Alignment = dVal;
                        _rVal.Add(cell);
                    }
                }

            }

            return _rVal;

        }

        public List<uopTableCell> SetRowTextAlignments(List<uopAlignments> aValues, bool bContinueWithLastValue = false)
        {

            List<uopTableCell> _rVal = new List<uopTableCell>();
            if (aValues == null) return _rVal;

            for (int i = 1; i <= aValues.Count; i++)
            {
                if (i > Rows) break;

                uopAlignments dVal = aValues[i - 1];

                List<uopTableCell> cells = FindAll(x => x.Row == i);
                foreach (uopTableCell cell in cells)
                {
                    cell.Alignment = dVal;
                    _rVal.Add(cell);
                }

            }
            if (aValues.Count < Rows && bContinueWithLastValue)
            {
                uopAlignments dVal = aValues[aValues.Count - 1];

                for (int i = aValues.Count + 1; i <= Rows; i++)
                {
                    List<uopTableCell> cells = FindAll(x => x.Row == i);
                    foreach (uopTableCell cell in cells)
                    {
                        cell.Alignment = dVal;
                        _rVal.Add(cell);
                    }
                }

            }

            return _rVal;

        }

        /// <summary>
        /// assigns the passed values to the indicated rows and columns
        /// passing "0" or "ALL"  or "" means to apply to all
        /// a range can be passed in the form 1-7 etc.
        /// </summary>
        /// <param name="aRowList">a comma delimited list of row indexes</param>
        /// <param name="aColList">a comma delimited list of column indexes</param>
        /// <param name="aTextFormat">the format to apply</param>
        /// <param name="aAlignment"></param>
        /// <returns></returns>
        public List<uopTableCell> SetTextFormat(string aRowList, string aColList, mzTextFormats aTextFormat, uopAlignments aAlignment = uopAlignments.Undefined)
        {
            List<uopTableCell> _rVal = GetCellsByList(aRowList, aColList);
            foreach (var item in _rVal)
            {
                item.TextFormat = aTextFormat;
                if (aAlignment != uopAlignments.Undefined) item.Alignment = aAlignment;
              
            }

            return _rVal;
         
        }


        /// <summary>
        /// assigns the passed values to the indicated rows and columns
        /// passing "0" or "ALL"  or "" means to apply to all
        /// a range can be passed in the form 1-7 etc.
        /// </summary>
        /// <param name="aRowList">a comma delimited list of row indexes</param>
        /// <param name="aColList">a comma delimited list of column indexes</param>
        /// <param name="aColor">the color to apply</param>
        /// <returns></returns>
        public List<uopTableCell> SetForeColor(string aRowList, string aColList, string aColor)
        {
            aColor ??= string.Empty;
            List<uopTableCell> _rVal = GetCellsByList(aRowList, aColList);
            foreach (var item in _rVal)
            {
                item.ForegroundColor = aColor;
            }
            return _rVal;

        }

        /// <summary>
        /// assigns the passed values to the indicated rows and columns
        /// passing "0" or "ALL"  or "" means to apply to all
        /// a range can be passed in the form 1-7 etc.
        /// </summary>
        /// <param name="aRow">a row index</param>
        /// <param name="aCol">a column index</param>
        /// <param name="aTextFormat">the format to apply</param>
        /// <param name="aAlignment"></param>
        /// <returns></returns>
        public List<uopTableCell> SetTextFormat(int aRow, int aCol, mzTextFormats aTextFormat, uopAlignments aAlignment = uopAlignments.Undefined)
        {
            List<uopTableCell> _rVal = GetCellsByList(aRow, aCol);
            foreach (var item in _rVal)
            {
                item.TextFormat = aTextFormat;
                if (aAlignment != uopAlignments.Undefined) item.Alignment = aAlignment;

            }

            return _rVal;

        }

        /// <summary>
        /// assigns the passed values to the indicated rows and columns
        /// passing "0" or "ALL"  or "" means to apply to all
        /// a range can be passed in the form 1-7 etc.
        /// </summary>
        /// <param name="aRow">a row index</param>
        /// <param name="aCol">a column index</param>
        /// <param name="aTextFormat">the format to apply</param>
        /// <param name="aAlignment"></param>
        /// <returns></returns>
        public List<uopTableCell> SetTextFormat(List<int> aRowList, List<int> aColList, mzTextFormats aTextFormat, uopAlignments aAlignment = uopAlignments.Undefined)
        {
            List<uopTableCell> _rVal = GetCellsByList(aRowList, aColList);
            foreach (var item in _rVal)
            {
                item.TextFormat = aTextFormat;
                if (aAlignment != uopAlignments.Undefined) item.Alignment = aAlignment;

            }

            return _rVal;

        }

        /// <summary>
        /// assigns the passed values to the indicated rows and columns
        /// passing "0" or "ALL"  or "" means to apply to all
        /// a range can be passed in the form 1-7 etc.
        /// returns the affected cells in a  collection
        /// </summary>
        /// <param name="aRowList">a comma delimited list of row indexes</param>
        /// <param name="aColList">a comma delimited list of column indexes</param>
        /// <param name="aAlignment">the alignment to assign</param>
        /// <param name="aOrientation">a orientation to assign</param>
        /// <returns></returns>
        public List<uopTableCell> SetAlignment(string aRowList, string aColList, uopAlignments aAlignment, double? aOrientation = null)
        {
            List<uopTableCell> _rVal = GetCellsByList(aRowList, aColList);
            foreach (var item in _rVal)
            {
                item.Alignment = aAlignment;
                if(aOrientation != null && aOrientation.HasValue) item.Orientation = aOrientation.Value;
            }
            return _rVal;
        }

        /// <summary>
        /// assigns the passed values to the indicated rows and columns
        /// passing "0" or "ALL"  or "" means to apply to all
        /// a range can be passed in the form 1-7 etc.
        /// returns the affected cells in a  collection
        /// </summary>
        /// <param name="aRow">a row index</param>
        /// <param name="aCol">a column index</param>
        /// <param name="aAlignment">the alignment to assign</param>
        /// <param name="aOrientation">a orientation to assign</param>
        /// <returns></returns>
        public List<uopTableCell> SetAlignment(List<int> aRowList, List<int> aColList, uopAlignments aAlignment, double? aOrientation = null)
        {
            List<uopTableCell> _rVal = GetCellsByList(aRowList, aColList);
            foreach (var item in _rVal)
            {
                item.Alignment = aAlignment;
                if (aOrientation != null && aOrientation.HasValue) item.Orientation = aOrientation.Value;
            }

         
            return _rVal;
        }


        /// <summary>
        /// assigns the passed values to the indicated rows and columns
        /// passing "0" or "ALL"  or "" means to apply to all
        /// a range can be passed in the form 1-7 etc.
        /// returns the affected cells in a  collection
        /// </summary>
        /// <param name="aRow">a row index</param>
        /// <param name="aCol">a column index</param>
        /// <param name="aAlignment">the alignment to assign</param>
        /// <param name="aOrientation">a orientation to assign</param>
        /// <returns></returns>
        public List<uopTableCell> SetAlignment(int aRow, int aCol, uopAlignments aAlignment, double? aOrientation = null)
        {
            List<uopTableCell> _rVal = GetCellsByList(aRow, aCol);
            foreach (var item in _rVal)
            {
                item.Alignment = aAlignment;
                if (aOrientation != null && aOrientation.HasValue) item.Orientation = aOrientation.Value;
            }


            return _rVal;
        }


        /// <summary>
        /// assigns the passed values to the indicated rows and columns
        /// passing "0" or "ALL"  or "" means to apply to all
        /// a range can be passed in the form 1-7 etc.
        /// </summary>
        /// <param name="aRowList">a comma delimited list of row indexes</param>
        /// <param name="aColList">a comma delimited list of column indexes</param>
        /// <param name="aSide">the target cell side</param>
        /// <param name="aStyle">the line style to apply</param>
        /// <param name="aWeight">the line weight</param>
        /// <returns></returns>
        public List<uopTableCell> SetBorders(string aRowList, string aColList, mzSides aSide, mzBorderStyles aStyle, mzBorderWeights aWeight)
        {
            List<uopTableCell> _rVal = GetCellsByList(aRowList, aColList);
            List<int> bitvals = mzUtils.DecomposeBitCode(aSide);
            if (bitvals.Contains((int)mzSides.Outer))
            {
                List<uopTableCell> subset = new List<uopTableCell>();
                uopTable.GetExtremes(_rVal, out int rmin, out int rmax, out int cmin, out int cmax);
                subset.AddRange(uopTable.SetBorderData(_rVal.FindAll(x => x.Col == cmin), (int)mzSides.Left, aStyle, aWeight));
                subset.AddRange(uopTable.SetBorderData(_rVal.FindAll(x => x.Col == cmax), (int)mzSides.Right, aStyle, aWeight));
                subset.AddRange(uopTable.SetBorderData(_rVal.FindAll(x => x.Row == rmin), (int)mzSides.Top, aStyle, aWeight));
                subset.AddRange(uopTable.SetBorderData(_rVal.FindAll(x => x.Row == rmax), (int)mzSides.Bottom, aStyle, aWeight));
                return subset;
            }
            if (bitvals.Contains((int)mzSides.Outer))
            {
                List<uopTableCell> subset = new List<uopTableCell>();
                uopTable.GetExtremes(_rVal, out int rmin, out int rmax, out int cmin, out int cmax);
                uopTable.AppendCells(subset, uopTable.SetBorderData(_rVal.FindAll(x => x.Col <= cmax), (int)mzSides.Right, aStyle, aWeight));
                uopTable.AppendCells(subset, uopTable.SetBorderData(_rVal.FindAll(x => x.Row <= rmax), (int)mzSides.Top, aStyle, aWeight));


                return subset;
            }
            foreach (var item in _rVal)
            {
                item.SetBorderData(aSide, aStyle, aWeight);
            }
            return _rVal;
        }
        /// <summary>
        /// assigns the passed values to the indicated rows and columns
        /// passing "0" or "ALL"  or "" means to apply to all
        /// a range can be passed in the form 1-7 etc.
        /// </summary>
        /// <param name="aRowList">a list of row indexes. null or empty means all rows </param>
        /// <param name="aColList">a list of column indexes. null or empty means all columns </param>
        /// <param name="aSide">the target cell side</param>
        /// <param name="aStyle">the line style to apply</param>
        /// <param name="aWeight">the line weight</param>
        /// <returns></returns>
        public List<uopTableCell> SetBorders(List<int> aRowList, List<int> aColList, mzSides aSide, mzBorderStyles aStyle, mzBorderWeights aWeight)
        {
            List<uopTableCell> _rVal = GetCellsByList(aRowList, aColList);
            List<int> bitvals = mzUtils.DecomposeBitCode(aSide);
            if (bitvals.Contains((int)mzSides.Outer))
            {
                List<uopTableCell> subset = new List<uopTableCell>();
                uopTable.GetExtremes(_rVal, out int rmin, out int rmax, out int cmin, out int cmax);
                subset.AddRange(uopTable.SetBorderData(_rVal.FindAll(x => x.Col == cmin), (int)mzSides.Left, aStyle, aWeight));
                subset.AddRange(uopTable.SetBorderData(_rVal.FindAll(x => x.Col == cmax), (int)mzSides.Right, aStyle, aWeight));
                subset.AddRange(uopTable.SetBorderData(_rVal.FindAll(x => x.Row == rmin), (int)mzSides.Top, aStyle, aWeight));
                subset.AddRange(uopTable.SetBorderData(_rVal.FindAll(x => x.Row == rmax), (int)mzSides.Bottom, aStyle, aWeight));
                return subset;
            }
            if (bitvals.Contains((int)mzSides.Outer))
            {
                List<uopTableCell> subset = new List<uopTableCell>();
                uopTable.GetExtremes(_rVal, out int rmin, out int rmax, out int cmin, out int cmax);
                uopTable.AppendCells(subset, uopTable.SetBorderData(_rVal.FindAll(x => x.Col <= cmax), (int)mzSides.Right, aStyle, aWeight));
                uopTable.AppendCells(subset, uopTable.SetBorderData(_rVal.FindAll(x => x.Row <= rmax), (int)mzSides.Top, aStyle, aWeight));


                return subset;
            }
            foreach (var item in _rVal)
            {
                item.SetBorderData(aSide, aStyle, aWeight);
            }
            return _rVal;
        }
        /// <summary>
        /// assigns the passed values to the indicated rows and columns
        /// passing "0" or "ALL"  or "" means to apply to all
        /// a range can be passed in the form 1-7 etc.
        /// </summary>
        /// <param name="aRow">a row index</param>
        /// <param name="aCol">a column index</param>
        /// <param name="aSide">the target cell side</param>
        /// <param name="aStyle">the line style to apply</param>
        /// <param name="aWeight">the line weight</param>
        /// <returns></returns>
        public List<uopTableCell> SetBorders(int aRow, int aCol, mzSides aSide, mzBorderStyles aStyle, mzBorderWeights aWeight)
        {
            return SetBorders(aRow, aCol, (int)aSide, aStyle, aWeight);
        }

            /// <summary>
        /// assigns the passed values to the indicated rows and columns
        /// passing "0" or "ALL"  or "" means to apply to all
        /// a range can be passed in the form 1-7 etc.
        /// </summary>
        /// <param name="aRow">a row index</param>
        /// <param name="aCol">a column index</param>
        /// <param name="aSide">the target cell side</param>
        /// <param name="aStyle">the line style to apply</param>
        /// <param name="aWeight">the line weight</param>
        /// <returns></returns>
        public List<uopTableCell> SetBorders(int aRow, int aCol, int aSide, mzBorderStyles aStyle, mzBorderWeights aWeight)
        {
            List<uopTableCell> _rVal = GetCellsByList(aRow, aCol);
            List<int> bitvals = mzUtils.DecomposeBitCode(aSide);
            if (bitvals.Contains((int)mzSides.Outer))
            {
                List<uopTableCell> subset = new List<uopTableCell>();
               uopTable.GetExtremes(_rVal, out int rmin, out int rmax, out int cmin, out int cmax);
                uopTable.AppendCells(subset, uopTable.SetBorderData(_rVal.FindAll(x => x.Col == cmin), (int)mzSides.Left, aStyle, aWeight));
                uopTable.AppendCells(subset, uopTable.SetBorderData(_rVal.FindAll(x => x.Col == cmax), (int)mzSides.Right, aStyle, aWeight));
                uopTable.AppendCells(subset, uopTable.SetBorderData(_rVal.FindAll(x => x.Row == rmin), (int)mzSides.Top, aStyle, aWeight));
                uopTable.AppendCells(subset, uopTable.SetBorderData(_rVal.FindAll(x => x.Row == rmax), (int)mzSides.Bottom, aStyle, aWeight));
                return subset;
            }
            if(bitvals.Contains((int)mzSides.Inner))
            {
                List<uopTableCell> subset = new List<uopTableCell>();
                uopTable.GetExtremes(_rVal, out int rmin, out int rmax, out int cmin, out int cmax);
                uopTable.AppendCells(subset, uopTable.SetBorderData(_rVal.FindAll(x => x.Col < cmax), (int)mzSides.Right, aStyle, aWeight));
                uopTable.AppendCells(subset, uopTable.SetBorderData(_rVal.FindAll(x => x.Row < rmax), (int)mzSides.Bottom, aStyle, aWeight));

            
                return subset;
            }
            foreach (var item in _rVal)
            {
                item.SetBorderData(aSide, aStyle, aWeight);
            }
            return _rVal;
        }
        /// <summary>
        /// Set borders around
        /// </summary>
        /// <param name="aStyle">the style to set the exterior borders to</param>
        /// <param name="aWeight">the weight to set the exterior borders to</param>
        public void BordersAround(mzBorderStyles aStyle, mzBorderWeights aWeight)
        {
            SetBorders("1", "ALL", mzSides.Top, aStyle, aWeight);
            SetBorders(Rows, 0, mzSides.Bottom, aStyle, aWeight);
            SetBorders("ALL", "1", mzSides.Left, aStyle, aWeight);
            SetBorders(0, Cols, mzSides.Right, aStyle, aWeight);
        }

        /// <summary>
        /// populates the target row or column with the passed data
        /// </summary>
        /// <param name="aRowOrCol">the row or column to transfer the data to</param>
        /// <param name="bSetColumn">a flag to set a row or a column</param>
        /// <param name="aDataCol">a collection containing values to put in the table</param>
        /// <param name="bGrowToInclude">the delimitor that seperates values in the passed string</param>
        /// <param name="aStartIndex">the row or column to to sart inputing in (1 by default)</param>
        /// <returns></returns>
        public List<uopTableCell> SetByCollection(int aRowOrCol, bool bSetColumn, List<string> aDataSet, bool bGrowToInclude = true, int? aStartIndex = null, List<uopAlignments> aAlignments = null, List<string> aNumbersFormats = null, List<mzTextFormats> aTextFormats = null, List<double> aCellWidths = null, List<double> aCellHeights = null, bool bApplyLastFormats = true, bool? aLockVal = null, List<int> aSkipList = null)
        {
            List<uopTableCell> _rVal = new List<uopTableCell>();
            if (aDataSet == null || aDataSet.Count <= 0) return _rVal;

            if (!bSetColumn)
            {
                _rVal = RowValuesSet(aRowOrCol, aDataSet, aStartIndex, bGrowToInclude, aSkipList);
            }
            else
            {
                _rVal = ColumnValuesSet(aRowOrCol, aDataSet, aStartIndex, bGrowToInclude, aSkipList);
            }
            ApplyFormats(_rVal, aAlignments, aNumbersFormats, aTextFormats, aCellWidths, aCellHeights, bApplyLastFormats,aLockVal: aLockVal);
            return _rVal;
        }
        /// <summary>
                 /// replaces cells with the search value with the new value
                 /// </summary>
                 /// <param name="aSearchVal">the value to search for</param>
                 /// <param name="aReplaceValue">the value to replace the values of the target cells with</param>
                 /// <param name="bCaseSensitive">a flag to apply case sensitivity to the test</param>
                 /// <param name="aRowList">a list of row indexes</param>
                 /// <param name="aColList">a list of column indexes</param>
                 /// <returns></returns>
        public List<uopTableCell> ReplaceCellValues(dynamic aSearchVal, dynamic aReplaceValue, bool bCaseSensitive, string aRowList = "ALL", string aColList = "ALL")
        {
            List<uopTableCell> replaceCellValues = new List<uopTableCell>();
            if (aReplaceValue == null)
            {
                return replaceCellValues;
            }

            replaceCellValues = GetCellsByValue(aSearchVal, bCaseSensitive, aRowList, aColList);
            for (int i = 0; i < replaceCellValues.Count; i++)
            {
                uopTableCell aCell = replaceCellValues[i];
                aCell.Value = aReplaceValue;
            }
            return replaceCellValues;
        }

        /// <summary>
        /// returns the indicated cells in a collection
        /// passing "0" or "ALL"  or "" means to get all cells
        /// a range can be passed in the form "1-7" etc.
        /// </summary>
        /// <param name="aRowList">comma delimited list of row indexes</param>
        /// <param name="aColList">comma delimited list of column indexes</param>
        /// <returns></returns>
        public List<uopTableCell> GetCellsByList(string aRowList = null, string aColList = null)
        {
            List<uopTableCell> _rVal = new List<uopTableCell>();
            aRowList ??= string.Empty;
            aColList ??= string.Empty;

            aRowList = xCreateIndexList(aRowList);
            aColList = xCreateIndexList(aColList);
            for (int iR = 1; iR <= Rows; iR++)
            {
                if (mzUtils.ListContains(iR, aRowList, ",", true))
                {
                    for (int iC = 1; iC <= Cols; iC++)
                    {
                        if (mzUtils.ListContains(iC, aColList, ",", true))
                        {
                            uopTableCell aCell = Cell(iR, iC);
                            if (aCell != null)
                            {
                                _rVal.Add(aCell);
                            }
                        }
                    }
                }
            }
            return _rVal;
        }

        /// <summary>
        /// returns the indicated cells in a collection
        /// passing "0" or "ALL"  or "" means to get all cells
        /// a range can be passed in the form "1-7" etc.
        /// </summary>
        /// <param name="aRowList">a list of row indexes. null or empty means all rows </param>
        /// <param name="aColList">a list of column indexes. null or empty means all columns </param>
        /// <returns></returns>
        public List<uopTableCell> GetCellsByList(List<int> aRowList = null, List<int> aColList = null)
        {
            List<uopTableCell> _rVal = new List<uopTableCell>();
            if (Rows < 1 || Cols < 1) return _rVal;
            aRowList ??= new List<int>();
            aColList ??= new List<int>();
            List<int> rlist = new List<int>();
            List<int> clist = new List<int>();
            if (aRowList.Count <= 0)
            {
               for(int i = 1; i <= Rows; i++) {  rlist.Add (i); };  // all rows

            }
            else
            {
                foreach (var item in aRowList)
                {
                    if (item >= 1 && item <= Rows)
                    {
                        if (rlist.IndexOf(item) < 0) rlist.Add(item); // passed rows
                    }
                        
                }
             
            }
            if (aColList.Count <= 0)
            {
                for (int i = 1; i <= Cols; i++) {clist.Add(i); }; // all rows.

            }
            else
            {
                foreach (var item in aColList)
                {
                    if (item >= 1 && item <= Cols)
                    {
                        if (clist.IndexOf(item) < 0) clist.Add(item); // passed rows
                    }
                        
                }

            }


            foreach(var r in rlist)
            {
                foreach(var c in clist)
                {
                    _rVal.Add(xCell(r, c, AddToFit: true));
                }
            }

            return _rVal;

        }

        /// <summary>
        /// returns the indicated cells in a collection
        /// passing "0" or "ALL"  or "" means to get all cells
        /// a range can be passed in the form "1-7" etc.
        /// </summary>
        /// <param name="aRowList">comma delimited list of row indexes</param>
        /// <param name="aColList">comma delimited list of column indexes</param>
        /// <returns></returns>
        public List<uopTableCell> GetCellsByList(int? aRowList = null, int? aColList = null)
        {
            List<uopTableCell> _rVal = new List<uopTableCell>();
            aRowList ??= 0;
            aColList ??= 0;
            if(aRowList.Value <= 0 || aColList.Value <=0)
            {
                string rlist = aRowList.Value <= 0 ? "All" : aRowList.Value.ToString();
                string clist = aColList.Value <= 0 ? "All" : aColList.Value.ToString();
                return GetCellsByList(rlist, clist);
            }
            uopTableCell aCell = Cell(aRowList.Value, aColList.Value);
            if (aCell != null) _rVal.Add(aCell);


            return _rVal;
        }
        private string xCreateIndexList(string aIndexList)
        {
            string _rVal = string.Empty;
            if (string.IsNullOrEmpty(aIndexList))  aIndexList = string.Empty;
           
            aIndexList = aIndexList.ToUpper().Replace(" ", string.Empty).Trim();
            if (aIndexList == "0" || aIndexList == "ALL" || aIndexList == string.Empty)
            {
                return string.Empty;
            }
            else
            {
                _rVal = aIndexList;
                if (aIndexList != string.Empty)
                {
                    int i = aIndexList.IndexOf("-");

                    if (i > 0 && i < aIndexList.Length)
                    {
                        int si = mzUtils.VarToInteger(aIndexList.Substring(0, i).Trim());
                        int ei = mzUtils.VarToInteger(aIndexList.Substring(i + 1, aIndexList.Length - i - 1).Trim());
                        mzUtils.SortTwoValues(true, ref si, ref ei);
                        _rVal = string.Empty;
                        for (i = si; i <= ei; i++)
                        {
                            mzUtils.ListAdd(ref _rVal, i);
                        }
                    }
                }
            }

            return _rVal;
        }


        /// <summary>
        /// Sets the values of the target row with the passed values
        /// </summary>
        /// <param name="aRow">the target row</param>
        /// <param name="aValues">the values to put into the cells of the target row</param>
        /// <param name="aStartCol"></param>
        /// <param name="bGrowToInclude"></param>
        /// <returns>returns the affected cells</returns>
        public List<uopTableCell> RowValuesSet(int aRow, List<string> aValues, int? aStartCol = null, bool bGrowToInclude = true, List<int> aSkipList = null)
        {
            if (aValues == null) return new List<uopTableCell>();
            List<uopProperty> props = new List<uopProperty>();
            for (int i = 1; i <= aValues.Count; i++) { if (aValues[i - 1] != null) { props.Add(new uopProperty($"VALUE_{i}", aValues[i - 1])); } else { props.Add(new uopProperty($"VALUE_{i}", "")); } }
            return RowValuesSet(aRow, props, aStartCol, bGrowToInclude, aSkipList);

        }
        public List<uopTableCell> RowValuesSet(int aRow, List<double> aValues, int? aStartCol = null, bool bGrowToInclude = true, List<int> aSkipList = null)
        {
            if (aValues == null) return new List<uopTableCell>();
            List<uopProperty> props = new List<uopProperty>();
            for (int i = 1; i <= aValues.Count; i++) { props.Add(new uopProperty($"VALUE_{i}", aValues[i - 1])); }
            return RowValuesSet(aRow, props, aStartCol, bGrowToInclude, aSkipList);

        }
        public List<uopTableCell> RowValuesSet(int aRow, List<uopProperty> aValues, int? aStartCol = null, bool bGrowToInclude = true, List<int> aSkipList = null)
        {
            int iR = aRow;
            if (aValues == null || iR <= 0) return new List<uopTableCell>();
            List<uopTableCell> _rVal = new List<uopTableCell>();
            int si = aStartCol ?? 1;

            if (si <= 0) si = 1;
         
            aSkipList ??= new List<int>();
            int c = si;
            uopTableCell aCell;

            List<uopTableCell> data = Row(aRow);
            for (int i = 1; i <= aValues.Count; i++)
            {


                if (aSkipList.Count > 0 && aSkipList.FindIndex(x => x == i) >= 0) continue;
                
                aCell = data.Find(x => x.Col == c);

                if (aCell == null )
                {

                    if(!bGrowToInclude) break;
                    aCell = xCell(iR, c, true);
                }
                if (aCell == null) continue;
                
                aCell.Property = aValues[i - 1];
                _rVal.Add(aCell);
                
                c++;
                 
                
            }

            return _rVal;

        }

        public List<uopTableCell> ColumnValuesSet(int aCol, List<string> aValues, int? aStartRow = null, bool bGrowToInclude = true, List<int> aSkipList = null)
        {
            if (aValues == null) return new List<uopTableCell>();
            List<uopProperty> props = new List<uopProperty>();
            for (int i = 1; i <= aValues.Count; i++) { if (aValues[i - 1] != null) { props.Add(new uopProperty($"VALUE_{i}", aValues[i - 1])); } else { props.Add(new uopProperty($"VALUE_{i}", "")); } }
            return ColumnValuesSet(aCol, props, aStartRow, bGrowToInclude, aSkipList);

        }
        public List<uopTableCell> ColumnValuesSet(int aCol, List<double> aValues, int? aStartRow = null, bool bGrowToInclude = true, List<int> aSkipList = null)
        {
            if (aValues == null) return new List<uopTableCell>();
            List<uopProperty> props = new List<uopProperty>();
            for (int i = 1; i <= aValues.Count; i++) { props.Add(new uopProperty($"VALUE_{i}", aValues[i - 1])); }
            return ColumnValuesSet(aCol, props, aStartRow, bGrowToInclude, aSkipList);


        }
        public List<uopTableCell> ColumnValuesSet(int aCol, List<uopProperty> aValues, int? aStartRow = null, bool bGrowToInclude = true, List<int> aSkipList = null)
        {
            List<uopTableCell> _rVal = new List<uopTableCell>();

            if (aValues == null) return _rVal;

            int iC = mzUtils.VarToInteger(aCol);

            if (iC <= 0) return _rVal;

            int si = aStartRow ?? 1;

            if (si <= 0) si = 1;
           
            aSkipList ??= new List<int>();
            int r = si;
            uopTableCell aCell;

            List<uopTableCell> data = Column(aCol);

            for (int i = 1; i <= aValues.Count; i++)
            {

                if( aSkipList.Count > 0 && aSkipList.FindIndex(x => x == i) >= 0) continue;
                aCell = data.Find(x => x.Row == r);

                if (aCell == null)
                {

                    if (!bGrowToInclude) break;
                    aCell = xCell(r, iC, true);
                }
                if (aCell == null) break;
                aCell.Property = aValues[i - 1];
                _rVal.Add(aCell);
                r++;

            }


            return _rVal;

        }

        /// <summary>
        /// returns the cell described by the passed row and column address
        /// </summary>
        /// <param name="aRow">the row number of the cell to return</param>
        /// <param name="aCol">the column number or letter of the column to return</param>
        /// <param name="AddToFit">flag to grow the table to include the row and column requested if the passed values exceed the current dimensions</param>
        /// <param name="rRow">returns the Row index of the returned cell</param>
        /// <param name="rCol">returns the Column index of the returned cell</param>
        /// <returns></returns>
        private uopTableCell xCell(dynamic aRow, dynamic aCol , bool AddToFit, out int rRow , out int rCol, out bool rAdded)
        {
            rAdded = false;
            uopTableCell _rVal = null;
            rRow = 0;
            rCol = 0;

            if (aCol is string)
            {
                rCol = mzUtils.ConvertLetterToInteger(Convert.ToString(aCol));
            }
            else
            {
                rCol = mzUtils.VarToInteger(aCol, true, null, 1);
            }
            rRow = mzUtils.VarToInteger(aRow, true, null, 1);
            int r = rRow;
            int c = rCol;
            uopTableCell cell = null;
            if (r <= 0 || c <= 0 ) return null;
            if (r <= Rows && c <= Cols)
            {
                cell = base.Find(x => x.Row == r && x.Col == c);
                if(cell == null)
                {
                    AddToFit = true;

                }
                else { _rVal = cell; }
            }

            if (r > Rows || c > Cols || cell == null)
            {
                if (!AddToFit) return null;

                int mr = (r <= Rows) ? r : Rows + 1 ;
                if (r > Rows) 
                    r = mr;

                int mc = (c <= Cols) ? c : Cols + 1;
                if (c > Cols) 
                    c = mc;

                
                for (int iR = 1; iR <= mr; iR++){
                    for(int iC = 1; iC <= mc; iC++)
                    {
                        cell = base.Find(x => x.Row == iR && x.Col == iC);
                        if (cell == null)
                        {
                            cell = new uopTableCell(iR, iC, new uopProperty($"CELL({Count + 1})", ""), bLocked: Protected);
                            rAdded = true;
                            base.Add(cell);

                        }
                        if (iC > Cols) _Cols = iC;
                        if(iR == r && iC == c)
                        {
                            _rVal = cell;
                        }
                    }
                    if (iR > Rows) _Rows = iR;
                }
                

                
            }
            
            if (_rVal != null)
            { _rVal.DisplayUnits = DisplayUnits; rCol = _rVal.Col; rRow = _rVal.Row; }
            else
            { rCol = 0; rRow = 0; }
            return _rVal;
        }

        /// <summary>
        /// returns the cell described by the passed row and column address
        /// </summary>
        /// <param name="aRow">the row number of the cell to return</param>
        /// <param name="aCol">the column number or letter of the column to return</param>
        /// <param name="AddToFit">flag to grow the table to include the row and column requested if the passed values exceed the current dimensions</param>
    
        /// <returns></returns>
        private uopTableCell xCell(dynamic aRow, dynamic aCol, bool AddToFit = false)
        {

            return xCell(aRow, aCol, AddToFit, out int _, out int _, out bool _);
        }

        /// <returns></returns>
        private uopTableCell xCell(dynamic aRow, dynamic aCol, bool AddToFit, out bool rAdded)
        {
            rAdded = false;
            return xCell(aRow, aCol, AddToFit, out int _, out int _, out rAdded);
        }

        /// <summary>
        /// Get Cells by Value
        /// </summary>
        /// <param name="aValue">the value to search for</param>
        /// <param name="bCaseSensitive">a flag to apply case sensitivity to the test</param>
        /// <param name="aRowList">a list of row indexes</param>
        /// <param name="aColList">a list of column indexes</param>
        /// <returns>returns the cells whose value matches the passed value</returns>
        public List<uopTableCell> GetCellsByValue(dynamic aValue, bool bCaseSensitive, string aRowList = "All", string aColList = "All")
        {
            List<uopTableCell> _rVal = new List<uopTableCell>();
         
            List<uopTableCell> cellsList = GetCellsByList(aRowList, aColList);
            foreach (var item in cellsList)
            {
                dynamic aVal = item.Value;
                if (bCaseSensitive)
                {
                    if (aVal == aValue)
                    {
                        _rVal.Add(item);
                    }
                }
                else
                {
                    if (aVal is string)
                    {
                        if (Convert.ToString(aVal) == Convert.ToString(aValue))
                        {
                            _rVal.Add(item);
                        }
                    }
                    else
                    {
                        if (aVal == aValue)
                        {
                            _rVal.Add(item);
                        }
                    }
                }
            }


            return _rVal;
        }


        /// <summary>
        /// returns the cell described by the passed row and column address
        /// </summary>
        /// <param name="aRow">the row number of the cell to return</param>
        /// <param name="aCol">the column number or letter of the column to return</param>
        /// <returns></returns>
        public uopTableCell Cell(int aRow, int aCol) => Find(x => x.Row == aRow && x.Col == aCol);
       

        public bool SetCellValue(int aRow, int aCol, dynamic aValue, string aAddress = null, bool? bNumeric = null, bool? bDataOnly = null, bool bGrowToInclude = true)
        {
            uopTableCell cell = xCell(aRow, aCol, bGrowToInclude, out bool _);
            if (cell == null) return false;
            if (bNumeric.HasValue) cell.Numeric = bNumeric.Value;
            
             bool _rVal =    cell.SetValue(aValue);

            if (!string.IsNullOrWhiteSpace(aAddress))
                cell.Name = aAddress;

            if (bDataOnly.HasValue && cell != null) cell.DataOnly = bDataOnly.Value;
            return _rVal;
        }
        
        /// <summary>
        /// returns the cells from the table in the passed collection
        /// </summary>
        /// <param name="aCollector">a collection to add the cells to</param>
        /// <param name="bAddressedOnly">a flag to only return cells whose address has been assigned manually</param>
        /// <param name="aStartRow">a values to set as the start row adress</param>
        /// <param name="aStartCol">a value to assign to the start column address</param>
        public void GetCells(ref List<uopTableCell> aCollector, bool bAddressedOnly, int? aStartRow = null, int? aStartCol = null)
        {
            aCollector ??= new List<uopTableCell>();
          
            uopTableCell aCell;
            double wd = 0;

            if (aStartRow.HasValue)
            {
                StartRow = mzUtils.VarToInteger(aStartRow.Value, false, StartRow, null, null, uopValueControls.PositiveNonZero);
            }
            if (aStartCol.HasValue)
            {
                StartColumn = mzUtils.VarToInteger(aStartCol.Value, false, StartColumn, null, null, uopValueControls.PositiveNonZero);
            }

            int cell_rows = StartRow;

            if (_LabelCell != null)
            {
                _LabelCell.StartColumn = StartColumn;
                _LabelCell.StartRow = cell_rows;
                cell_rows ++;
                aCollector.Add(_LabelCell);
            }

            for (int iR = 1; iR <= Rows; iR++)
            {
                double mht = 1;
                int cell_cols = StartColumn;
                for (int iC = 1; iC <= Cols; iC++)
                {
                    aCell = xCell(iR, iC, true);
                    if (!bAddressedOnly || (bAddressedOnly && aCell.IsAdressed))
                    {
                        aCollector.Add(aCell);
                      
                        aCell.StartColumn = cell_cols;
                        aCell.StartRow = cell_rows;
                        wd = Math.Round(aCell.Width, 0);
                        if (wd < 1)  wd = 1;
                     

                        cell_cols += (int)wd;
                        if (aCell.Height > mht)
                        {
                            mht = aCell.Height;
                        }

                       
                    }

                }
                mht = Math.Round(mht, 0);
                if (mht < 1) mht = 1;
              
                cell_rows+= (int)mht;
            }

            if (_CaptionCell != null)
            {
                _CaptionCell.StartColumn = StartColumn;
                _CaptionCell.StartRow = cell_rows;
                aCollector.Add(_CaptionCell);
            }
        }
        /// <summary>
        ///   '#1the target row
        ///   '#2the target column
        ///   '#3the value to apply
        ///   '#4a value ot insert if the cell is numeric and is equal to zero
        ///   '#5a aligment to assign
        ///   '^sets the value of the target cell
        ///   '~if the passed indexes are outside of the current dimensions the table is grown to include the new cell
        /// </summary>
        /// <param name="aRow"></param>
        /// <param name="aCol"></param>
        /// <param name="aValue"></param>
        /// <param name="aZeroReplacer"></param>
        /// <param name="aAlignment"></param>
        /// <param name="aNumberFormat"></param>
        /// <returns></returns>
        public uopTableCell SetValue(int aRow, int aCol, string aValue, string aZeroReplacer = "", uopAlignments aAlignment = uopAlignments.Undefined, string aNumberFormat = "")
        {
            uopTableCell setValue = xCell(aRow, aCol, true);
            if (setValue != null)
            {
                setValue.Value = aValue;
                if (aAlignment != uopAlignments.Undefined)
                {
                    setValue.Alignment = aAlignment;
                }
                if (aZeroReplacer != null && setValue.Numeric && setValue.Value.ToString().Equals("0"))
                {
                    setValue.Value = aZeroReplacer;
                }
                if (aNumberFormat != null)
                {
                    setValue.NumberFormat = Convert.ToString(aNumberFormat);
                }
            }
            return setValue;
        }

        public uopTableCell NamedCell(string aCellName)
        {
            return Find(x => string.Compare(x.Name, aCellName, true) == 0);
        }

        public bool SetValue(string aNameOrAddress, dynamic newval, dynamic aDefaultValue = null, dynamic aNullValue = null)
        {
            uopTableCell cell = NamedCell(aNameOrAddress);
            if (cell == null) return false;
            bool _rVal = cell.SetValue(newval, aDefaultValue, aNullValue);
            return _rVal;
        }

        public bool SetValue(int aRow, int aCol, dynamic newval, dynamic aDefaultValue = null, dynamic aNullValue = null)
        {
            uopTableCell cell = Cell(aRow, aCol);
            if (cell == null) return false;
            bool _rVal = cell.SetValue(newval, aDefaultValue, aNullValue);
            return _rVal;
        }

        /// <summary>
        /// assigns the passed width to the indicated cells passing "0" or "ALL"  or "" means to apply to all
        /// a range can be passed in the form 1-7 etc.
        /// </summary>
        /// <param name="aRowList">a comma delimited list of column indexes</param>
        /// <param name="aColList">a comma delimited list of row indexes</param>
        /// <param name="aValue">the value to apply</param>
        /// <param name="aZeroReplacer">a value ot insert if the cell is numeric and is equal to zero</param>
        /// <param name="aAlignment">a aligment to assign</param>
        /// <param name="aNumberFormat"></param>
        /// <returns></returns>
        public List<uopTableCell> SetValues(string aRowList, string aColList, dynamic aValue, dynamic aZeroReplacer = null, uopAlignments aAlignment = uopAlignments.Undefined, dynamic aNumberFormat = null)
        {
            List<uopTableCell> tableCells = new List<uopTableCell>();
            if (aValue == null)
            {
                return tableCells;
            }

            tableCells = GetCellsByList(aRowList, aColList);
            for (int i = 0; i < tableCells.Count; i++)
            {
                uopTableCell aCell = tableCells[i];
                aCell.Value = aValue;
                if (aAlignment != uopAlignments.Undefined)
                {
                    aCell.Alignment = aAlignment;
                }
                if (aZeroReplacer != null && aCell.Numeric && aCell.Value == 0)
                {
                    aCell.Value = aZeroReplacer;
                }
                if (aNumberFormat != null)
                {
                    aCell.NumberFormat = Convert.ToString(aNumberFormat);
                }
            }
            return tableCells;
        }

        /// <summary>
        /// returns a collection of the row data delimited by pipes (or the passed delimitor)
        /// </summary>
        /// <param name="bColumn"></param>
        /// <param name="aRowOrColumnIDs"></param>
        /// <returns></returns>
        public List<uopTableCell> Cells(bool bColumn, string aRowOrColumnIDs)
        {
            return Cells(bColumn, aRowOrColumnIDs, out List<int> _);
        }


        /// <summary>
        /// returns a collection of the row data delimited by pipes (or the passed delimitor)
        /// </summary>
        /// <param name="bColumn"></param>
        /// <param name="aRowOrColumnIDs"></param>
        /// <returns></returns>
        public List<uopTableCell> Cells(bool bColumn, string aRowOrColumnIDs,out List<int> rIndexes)
        {
            rIndexes = new List<int>();
            string IDs = xCreateIndexList(aRowOrColumnIDs);
            List<uopTableCell> _rVal = new List<uopTableCell>();
            int cnt = bColumn ? Cols : Rows;
            List<uopTableCell> cells;
            for (int i = 1; i <= cnt; i++)
            {
                if (mzUtils.ListContains(i, IDs, ",", true))
                {
                    cells = bColumn ? base.FindAll(x => x.Col == i) : base.FindAll(x => x.Row == i);

                    if (cells != null)
                    {
                        if(cells.Count > 0)
                        {
                            rIndexes.Add(i);
                            _rVal.AddRange(cells);
                        }
                        
                    }
                }
            }

            return _rVal;
        }
        /// <summary>
        /// returns a collection of the row or column data as dynamic
        /// </summary>
        /// <param name="bColumnValues"></param>
        /// <param name="aRowOrColumnIDs"></param>
        /// <param name="aNullReplacer"></param>
        /// <returns></returns>
        public List<dynamic> Values(bool bColumnValues, string aRowOrColumnIDs, dynamic aNullReplacer = null)
        {
            List<dynamic> _rVal = new List<dynamic>();
            List<uopTableCell> cells = Cells(bColumnValues, aRowOrColumnIDs);
            foreach (var item in cells)
            {
                if (item.IsDefined)
                    _rVal.Add(item.IsNullValue ? aNullReplacer : item.Value);
                else
                    _rVal.Add(aNullReplacer);
            }
            return _rVal;
        }
        /// <summary>
        /// returns a collection of the row or column data as integer
        /// </summary>
        /// <param name="bColumnValues"></param>
        /// <param name="aRowOrColumnIDs"></param>
        /// <param name="aNullReplacer"></param>
        /// <returns></returns>
        public List<int> ValuesI(bool bColumnValues, string aRowOrColumnIDs, int aNullReplacer = 0)
        {
            List<int> _rVal = new List<int>();
            List<uopTableCell> cells = Cells(bColumnValues, aRowOrColumnIDs);
            foreach (var item in cells)
            {
                if (item.IsDefined)
                    _rVal.Add(item.IsNullValue ? aNullReplacer : item.ValueI);
                else
                    _rVal.Add(aNullReplacer);
            }
            return _rVal;
        }

        /// <summary>
        /// returns a collection of the row or column data as string
        /// </summary>
        /// <param name="bColumnValues"></param>
        /// <param name="aRowOrColumnIDs"></param>
        /// <param name="aNullReplacer"></param>
        /// <returns></returns>
        public List<string> ValuesStr(bool bColumnValues, string aRowOrColumnIDs, string aNullReplacer = "")
        {
            List<string> _rVal = new List<string>();
            List<uopTableCell> cells = Cells(bColumnValues, aRowOrColumnIDs);
            foreach (var item in cells)
            {
                if (item.IsDefined)
                    _rVal.Add(item.IsNullValue ? aNullReplacer : item.ValueS);
                else
                    _rVal.Add(aNullReplacer);
            }
            return _rVal;
        }

        public System.Data.DataTable ToDataTable_S()
        {
            System.Data.DataTable _rVal = new System.Data.DataTable();
            //add a column for every column in this table
            for (int c = 1; c <= Cols; c++)
            {
                _rVal.Columns.Add(Cell(1, c).ValueS, typeof(string));
            }

            for (int r = 2; r <= Rows; r++)
            {
                System.Data.DataRow row = _rVal.Rows.Add(Cell(r, 1).ValueS, typeof(string));

                List<uopTableCell> cells =Row(r);
                object[] rowdata = new object[cells.Count];
                for (int c = 1; c <= cells.Count; c++)
                {
                    uopTableCell cell = cells[c - 1];
                    rowdata[c - 1] = cell.ValueS;

                }
                row.ItemArray = rowdata;
            }

            return _rVal;
        }

        /// <summary>
        /// returns a collection of the row or column data as string
        /// </summary>
        /// <param name="bColumnValues"></param>
        /// <param name="aRowOrColumnIDs"></param>
        /// <param name="aNullReplacer"></param>
        /// <returns></returns>
        public List<string> ToStrings(bool bColumnValues, string aRowOrColumnIDs = null, string aNullReplacer = "", string aDelimitor = "|")
        {
            List<string> _rVal = new List<string>();
            List<List<uopTableCell>> subsets = bColumnValues ? GetCols(aRowOrColumnIDs) : GetRows(aRowOrColumnIDs);
            string sval;
            string slist;
            aNullReplacer ??= string.Empty;
            foreach (var set in subsets)
            {
                slist = string.Empty;
                int c = 0;
                foreach (var cell in set)
                {

                    if (cell.IsDefined)
                        sval = cell.IsNullValue ? aNullReplacer : cell.ValueS;
                    else
                         sval = aNullReplacer;

                    if (string.IsNullOrWhiteSpace(sval)) sval = aNullReplacer;
                    slist += sval;
                    //mzUtils.ListAdd(ref slist, sval, bSuppressTest: true, aDelimitor: aDelimitor, bAllowNulls: true);
                    c++;
                    if (c < set.Count) slist += aDelimitor;

                }
                _rVal.Add(slist);
            }


            return _rVal;
        }

       

        public int GetRowIndex(string aStringVal, int aColToSearch, bool bIgnoreCase = true, int aOccurs = 1)
        {
            if (Count <= 0 || aColToSearch <1 || aColToSearch > Cols) return 0;
            aStringVal ??= string.Empty;
            if (aOccurs <= 0) aOccurs = 1;

            List<uopTableCell> colvals = Column(aColToSearch);
            List<uopTableCell> matches = colvals.FindAll(x => string.Compare(x.ValueS, aStringVal, bIgnoreCase) ==0);
            return matches.Count < aOccurs ? 0 : matches[aOccurs - 1].Row; 

        }
        public List<List<uopTableCell>> GetRows(dynamic aRowIds)
        {
            List<List<uopTableCell>> _rVal = new List<List<uopTableCell>>();
            List<uopTableCell> cells = Cells(false, aRowIds, out List<int> ids);
            for (int i = 1; i <= ids.Count; i++)
            {
                _rVal.Add(cells.FindAll(x => x.Row == i));
            }
            return _rVal;
        }

           public int GetColIndex(string aStringVal, int aRowToSearch, bool bIgnoreCase = true, int aOccurs = 1)
        {
            if (Count <= 0 || aRowToSearch <1 || aRowToSearch > Rows) return 0;
            aStringVal ??= string.Empty;
            if (aOccurs <= 0) aOccurs = 1;

            List<uopTableCell> rowvals = Row(aRowToSearch);
            List<uopTableCell> matches = rowvals.FindAll(x => string.Compare(x.ValueS, aStringVal, bIgnoreCase) ==0);
            return matches.Count < aOccurs ? 0 : matches[aOccurs - 1].Col; 

        }
     

        public List<List<uopTableCell>> GetCols(dynamic aColIds)
        {
            List<List<uopTableCell>> _rVal = new List<List<uopTableCell>>();
            List<uopTableCell> cells = Cells(true, aColIds, out List<int> ids);
            for (int i = 1; i <= ids.Count; i++)
            {
                _rVal.Add(cells.FindAll(x => x.Row == i));
            }
            return _rVal;
        }

        /// <summary>
        /// returns a collection of the row or column data as bool
        /// </summary>
        /// <param name="bColumnValues"></param>
        /// <param name="aRowOrColumnIDs"></param>
        /// <param name="aNullReplacer"></param>
        /// <returns></returns>
        public List<bool> ValuesB(bool bColumnValues, string aRowOrColumnIDs, bool aNullReplacer = false)
        {
            List<bool> _rVal = new List<bool>();
            List<uopTableCell> cells = Cells(bColumnValues, aRowOrColumnIDs);
            foreach (var item in cells)
            {
                if (item.IsDefined)
                    _rVal.Add(item.IsNullValue ? aNullReplacer : item.ValueB);
                else
                    _rVal.Add(aNullReplacer);
            }
            return _rVal;
        }

        /// <summary>
        /// returns a collection of the row or column data as double
        /// </summary>
        /// <param name="bColumnValues"></param>
        /// <param name="aRowOrColumnIDs"></param>
        /// <param name="aNullReplacer"></param>
        /// <returns></returns>
        public List<double> ValuesB(bool bColumnValues, string aRowOrColumnIDs, double aNullReplacer = 0)
        {
            List<double> _rVal = new List<double>();
            List<uopTableCell> cells = Cells(bColumnValues, aRowOrColumnIDs);
            foreach (var item in cells)
            {
                if (item.IsDefined)
                    _rVal.Add(item.IsNullValue ? aNullReplacer : item.ValueD);
                else
                    _rVal.Add(aNullReplacer);
            }
            return _rVal;
        }

        /// <summary>
        /// populates the tartget row or column with the passed data
        /// </summary>
        /// <param name="aRowOrCol">the row or column to transfer the data to</param>
        /// <param name="bSetColumn">a flag to set a row or a column</param>
        /// <param name="aDataString">a string containing values to put in the table</param>
        /// <param name="bGrowToInclude"></param>
        /// <param name="aDelimitor">the delimitor that seperates values in the passed string</param>
        /// <param name="aStartIndex">the row or column to to sart inputing in (1 by default)</param>
        /// <returns></returns>
        public List<uopTableCell> SetByString(dynamic aRowOrCol, bool bSetColumn, string aDataString, bool bGrowToInclude = true, string aDelimitor = "|", int aStartIndex = 0)
        {
            List<string> aVals = mzUtils.StringsFromDelimitedList(aDataString, aDelimitor, true);

            if (!bSetColumn)
            {
                return RowValuesSet(aRowOrCol, aVals, aStartIndex, bGrowToInclude);
            }
            else
            {
                return ColumnValuesSet(aRowOrCol, aVals, aStartIndex, bGrowToInclude);
            }
        }

        /// <summary>
        /// assigns the passed values to the indicated rows and columns
        /// passing "0" or "ALL"  or "" means to apply to all
        /// a range can be passed in the form 1-7 etc.
        /// </summary>
        /// <param name="aRowList">a comma delimited list of row indexes</param>
        /// <param name="aColList">a comma delimited list of column indexes</param>
        /// <param name="aNumberFormat">the format to apply</param>
        /// <param name="aAlignment"></param>
        /// <returns></returns>
        public List<uopTableCell> SetNumberFormat(string aRowList, string aColList, string aNumberFormat, uopAlignments aAlignment = uopAlignments.Undefined)
        {
            List<uopTableCell> _rVal = GetCellsByList(aRowList, aColList);
            foreach (var item in _rVal)
            {
                item.NumberFormat = aNumberFormat;
                if (aAlignment != uopAlignments.Undefined) item.Alignment = aAlignment;
            }
            return _rVal;
        }

        /// <summary>
        /// assigns the passed values to the indicated rows and columns
        /// passing "0" or "ALL"  or "" means to apply to all
        /// a range can be passed in the form 1-7 etc.
        /// </summary>
        /// <param name="aRow">a row index</param>
        /// <param name="aCol">a column index</param>
        /// <param name="aNumberFormat">the format to apply</param>
        /// <param name="aAlignment"></param>
        /// <returns></returns>
        public List<uopTableCell> SetNumberFormat(int aRow, int aCol, string aNumberFormat, uopAlignments aAlignment = uopAlignments.Undefined)
        {
            List<uopTableCell> _rVal = GetCellsByList(aRow, aCol);
            foreach (var item in _rVal)
            {
                item.NumberFormat = aNumberFormat;
                if (aAlignment != uopAlignments.Undefined) item.Alignment = aAlignment;
            }
            return _rVal;
        }

        /// <summary>
        /// assigns the passed values to the indicated rows and columns passing "0" or "ALL"  or "" means to apply to all
        /// a range can be passed in the form 1-7 etc.
        /// </summary>
        /// <param name="aRowList">a comma delimited list of row indexes</param>
        /// <param name="aColList">a comma delimited list of column indexes</param>
        /// <param name="aColor">the color to apply</param>
        /// <returns></returns>
        public List<uopTableCell> SetFontColor(string aRowList, string aColList, System.Drawing.Color aColor)
        {
            List<uopTableCell> _rVal = GetCellsByList(aRowList, aColList);
            foreach (var item in _rVal)
            {
                item.FontColor = aColor;
            }
            return _rVal;
        }

        /// <summary>
        /// assigns the passed values to the indicated rows and columns passing "0" or "ALL"  or "" means to apply to all
        /// a range can be passed in the form 1-7 etc.
        /// </summary>
        /// <param name="aRow">a row index</param>
        /// <param name="aCol">a column index</param>
        /// <param name="aColor">the color to apply</param>
        /// <returns></returns>
        public List<uopTableCell> SetFontColor(int aRow, int aCol, System.Drawing.Color aColor)
        {
            List<uopTableCell> _rVal = GetCellsByList(aRow, aCol);
            foreach (var item in _rVal)
            {
                item.FontColor = aColor;
            }
            return _rVal;
        }

        public override string ToString()
        {
            string _rVal = $"uopTable [{Rows}x{Cols}]";
            if (!string.IsNullOrWhiteSpace(Name)) _rVal += $" - {Name}";
            return _rVal;
        }

 
        #endregion Methods

        #region Shared Methods
        public static void GetExtremes(List<uopTableCell> aCells, out int rMinRow, out int rMaxRow, out int rMinCol, out int rMaxCol)
        {
            rMinRow = 0;
            rMinCol = 0;
            rMaxRow = 0;
            rMaxCol = 0;
            if (aCells == null) return;
            if (aCells.Count <= 0 ) return;
            rMinRow = int.MaxValue;
            rMinCol = int.MaxValue;
            rMaxRow = int.MinValue;
            rMaxCol = int.MinValue;
            foreach (var item in aCells)
            {
                if (item.Row < rMinRow) rMinRow = item.Row;
                if (item.Col < rMinCol) rMinCol = item.Col;
                if (item.Row > rMaxRow) rMaxRow = item.Row;

                if (item.Col > rMaxCol) rMaxCol = item.Col;
            }

        }
        /// <summary>
        /// assigns the passed values to the indicated rows and columns
        /// passing "0" or "ALL"  or "" means to apply to all
        /// a range can be passed in the form 1-7 etc.
        /// </summary>
        /// <param name="aRow">a row index</param>
        /// <param name="aCol">a column index</param>
        /// <param name="aSide">the target cell side</param>
        /// <param name="aStyle">the line style to apply</param>
        /// <param name="aWeight">the line weight</param>
        /// <returns></returns>
        public static List<uopTableCell> SetBorderData(List<uopTableCell> aCells, int aSide, mzBorderStyles aStyle, mzBorderWeights aWeight)
        {
            List<uopTableCell> _rVal = new List<uopTableCell>();
            if (aCells == null) return _rVal;
            if (aCells.Count <= 0) return _rVal;
           
            
            foreach (var item in aCells)
            {
                item.SetBorderData(aSide, aStyle, aWeight);
                _rVal.Add(item);
            }

            return _rVal;
        }

        public static void AppendCells(List<uopTableCell> aCells, List<uopTableCell> bCells)
        {
            if (aCells == null || bCells == null) return;
            foreach (var item in bCells)
            {
                if (aCells.IndexOf(item) < 0) aCells.Add(item);
            }
        }
        #endregion Shared Methods
    }
}
