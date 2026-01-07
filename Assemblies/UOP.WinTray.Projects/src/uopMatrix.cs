using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;

namespace UOP.WinTray.Projects
{
    /// <summary>
    /// represents a n by m matrix where n is the number of rows and m is the number of columns
    /// </summary>
    public class uopMatrix :  List<uopMatrixRow>, IEnumerable<uopMatrixRow>, ICloneable
    {


        #region Events
        public delegate void DimensionChangeHandler();//@raised when the dimensions of the current matrix changes
        public event DimensionChangeHandler eventDimensionChange;
        public delegate void MatrixValueChangeHandler();//@raised when a matrix value changes if MonitorChanges = True
        public event MatrixValueChangeHandler eventMatrixValueChange;

        public delegate void MatrixSetMemberChangeHandler(List<uopMatrixCell> changedCells);//@raised when a matrix value changes using SetMember or SetMemberSymmetric if MonitorChanges = True
        public event MatrixSetMemberChangeHandler eventMatrixSetMemberChange;

        #endregion Events


        #region Constructors

        public uopMatrix() => Initialize();

        public uopMatrix(int aRows, int aCols, int aPrecis = 10, string aName = "", double aInitVal = 0,List<string> aColumnHeaders= null, List<string> aRowHeaders = null, bool bInvisibleVal = false)  => Initialize(aRows: aRows, aCols: aCols, aPrecis: aPrecis, aName: aName, aInitVal: aInitVal,aColumnHeaders: aColumnHeaders, aRowHeaders: aRowHeaders, bInvisibleVal: bInvisibleVal);

   
        public uopMatrix(uopMatrix aMatrix, bool bDontCopyMembers = false) => Initialize(aMatrix: aMatrix, bDontCopyMembers: bDontCopyMembers);
        public uopMatrix(IEnumerable<double> aValues, bool bReturnColumn = false, int aPrecis = 10, string aName = "", double aInitVal = 0) 
        {
            int rows =  aValues == null ? 1 :  bReturnColumn ? aValues.Count() : 1;
            int cols = aValues == null ? 0 : bReturnColumn ? 1 : aValues.Count() ;
                Initialize(rows, cols, aPrecis,aName,aInitVal);
            if (aValues == null) return;
            if (bReturnColumn)
                PopulateCol(1, aValues.ToList());
            else
                PopulateRow(1, aValues.ToList());

        }


        internal  virtual void Initialize(int aRows =1, int aCols =1, int aPrecis = 10, string aName = "", double aInitVal = 0, uopMatrix aMatrix = null, List<string> aColumnHeaders = null, List<string> aRowHeaders = null, bool bInvisibleVal = false,bool bDontCopyMembers = false)
        {
            base.Clear();

            _UserVar1 = null;
            _UserVar2 = null;
            _TempStore = null;
            _Precision = mzUtils.LimitedValue(aPrecis, 0,15);
            Name = aName;
           _ColumnHeaders = aColumnHeaders;
            _RowHeaders = aRowHeaders;

            aRows = mzUtils.LimitedValue(aRows,1,uopMatrix.Maxsize);
            aCols = mzUtils.LimitedValue(aCols, 1, uopMatrix.Maxsize);
            Resize(aRows, aCols, aInitValue: aInitVal, bInvisibleVal: bInvisibleVal);


            if (aMatrix != null)
            {
                _UserVar1 = aMatrix.UserVar1;
                _UserVar2 = aMatrix.UserVar2;
                _Precision = aMatrix.Precision;
                Name = aMatrix.Name;
                Clear();
                if (!bDontCopyMembers)
                {
                    _TempStore = uopMatrix.Copy(aMatrix._TempStore);
                    if (aMatrix._ColumnHeaders != null)
                        _ColumnHeaders = new List<string>(aMatrix.ColumnHeaders);
                    if (aMatrix._RowHeaders != null)
                        _RowHeaders = new List<string>(aMatrix._RowHeaders);
                    _Cols = aMatrix.Cols;
                    foreach (var brow in aMatrix)
                    {
                        Add(new uopMatrixRow(brow));
                    }

                }
                
            }


        }
        #endregion Constructors;

        #region Properties

        private uopMatrix _TempStore { get; set; }//@used internally to store the current matrix for change comparison


        private List<string> _ColumnHeaders;
        public virtual List<string> ColumnHeaders
        {
            get =>  _ColumnHeaders;
            
            set
            {
                if(value != null)
                {
                    _ColumnHeaders = new List<string>();
                    _ColumnHeaders.AddRange(value);
                }
                else { _ColumnHeaders = null; }
            }
        }

        private List<string> _RowHeaders;
        public virtual List<string> RowHeaders
        {
            get
            {
                return _RowHeaders;
            }
            set
            {
                if (value != null)
                {
                    _RowHeaders = new List<string>();
                    _RowHeaders.AddRange(value);
                }
                else
                {
                    _RowHeaders = null;
                }
            }
        }


        private int _Cols;
        /// <summary>
        /// the number of columns in the matrix
        /// </summary>
        public int Cols
        {
            get => _Cols;

            set
            {
                if (value < 1) value = 1;
                if (value > uopMatrix.Maxsize) value = uopMatrix.Maxsize;
                if (value == _Cols) return;
                _Cols = value;
                Resize(Rows,_Cols );
            }
        }
        /// <summary>
        /// returns a string that contains all of the values in the matrix
        /// ~rows are seperated by colons with individual row values seperated by commas.
        /// ~see DefineByString
        /// </summary>
        public string Descriptor
        {
            get
            {
                string _rVal = string.Empty;

                for (int r = 1; r <= Rows; r++)
                {
                    uopMatrixRow row = Row(r);
                    if (r > 1) _rVal += "\n";
                    _rVal += row.Descriptor; ;
                }

                return _rVal;
            }
        }

        /// <summary>
        /// a string that describes the dimensions of the matrix
        /// ~like 2x3 etc.
        /// </summary>
        public string Dimensions => $"{Rows} x {Cols}";
        /// <summary>
        /// returns True if Rows = Cols
        /// </summary>
        public bool IsSquare => Rows == Cols;

        /// <summary>
        /// if set to True the MatrixValueChange event is raised when any of the values in the matrix change
        /// </summary>
        public bool MonitorChanges { get; set; }


        private int _Precision;
        /// <summary>
        /// the number if double places that all matrix values are rounded to
        /// ~default = 6, max = 15, min = 1
        /// </summary>
        public int Precision { get => _Precision; set { value = mzUtils.LimitedValue(value, 1, 15); if (_Precision != value) { _Precision = value; RoundValues(); } } }

        /// <summary>
        /// ^the number of rows in the matrix
        /// </summary>
        public int Rows
        {
            get => base.Count;

            set
            {
                //^the number of rows in the matrix
                Resize(value,Cols);

            }
        }


        /// <summary>
        /// the number of elements in the matrix
        /// </summary>
        public int Size => Rows * Cols;


        private dynamic _UserVar1;
        private dynamic _UserVar2;
        /// <summary>
        /// provided to carry addition information about the matrix
        /// </summary>
        public dynamic UserVar1 { get => _UserVar1; set => _UserVar1 = value; }

        /// <summary>
        /// provided to carry addition information about the matrix
        /// </summary>
        public dynamic UserVar2 { get => _UserVar2; set => _UserVar2 = value; }




public string Name { get; set; }
        #endregion Properties

        #region Methods

        public new  void Clear()
        {
            base.Clear();
            _TempStore = null;
            _Cols = 0;
        }
        /// <summary>
        ///returns a list containing lists of doubles which are the column values
        /// </summary>
        public List<List<double>> ToColList()
        {

            List<List<double>> _rVal = new List<List<double>>();
            for (int c = 1; c <= Cols; c++)
            {
                _rVal.Add(GetValues(c, true));
            }
            return _rVal;
        }

        /// <summary>
        ///returns a list containing lists of doubles which are the row values
        /// </summary>
        public List<List<double>> ToRowList()
        {

            List<List<double>> _rVal = new List<List<double>>();
            for (int r = 1; r <= Cols; r++)
            {
                _rVal.Add(GetValues(r, false));
            }
            return _rVal;
        }
        /// <summary>
        /// returns amatrix that is a subet of the current matrix values
        /// </summary>
        /// <returns></returns>
        public uopMatrix SubMatrix(int aStartRow, int aEndRow, int aStartCol, int aEndCol) 
        {

            if (aStartRow < 1 || aStartRow > Rows) throw new IndexOutOfRangeException();
            if (aEndRow < 1 || aEndRow > Rows) throw new IndexOutOfRangeException();
            if (aStartCol < 1 || aStartCol > Cols) throw new IndexOutOfRangeException();
            if (aEndCol < 1 || aEndCol > Cols) throw new IndexOutOfRangeException();

            mzUtils.LoopLimits(aStartRow, aEndRow, 1, Rows, out int sr, out int er);
            mzUtils.LoopLimits(aStartCol, aEndCol, 1, Cols, out int sc, out int ec);

            uopMatrix _rVal = new uopMatrix(this);
            _rVal.SetDimensions(er - sr + 1, ec - sc + 1);

            int nr = 1;
            int nc = 1;

            for (int r = sr; r <= er; r++)
            {
                nc = 1;
                for (int c = sc; c <= ec; c++)
                {
                 
                    _rVal.Cell(nr, nc).Copy(Cell(r, c));
                    nc++;
                }
                nr++;
            }


            return _rVal;

        }

        /// <summary>
        /// adds the passed matrix to this one
        /// #1 the matrix to add to this one
        /// </summary>
        /// <param name="aMatrix"></param>
        public bool Add(uopMatrix aMatrix)
        {
            bool _rVal = false;
            try
            {
                if (aMatrix == null) throw new Exception($"[{System.Reflection.MethodBase.GetCurrentMethod().Name}] The passed matrix is undefined");
                if (string.Compare(aMatrix.Dimensions, Dimensions) != 0) throw new Exception($"[{System.Reflection.MethodBase.GetCurrentMethod().Name}] The passed matrix dimensions do not match this matrix dimensions.");

                double nVal;

                for (int n = 1; n <= Rows; n++)
                {
                    uopMatrixRow row = Row(n);
                    for (int m = 1; m <= Cols; m++)
                    {
                        nVal = Math.Round(row.Value( m) + aMatrix.GetMember(n, m), _Precision);
                        if (row.SetValue(m,nVal)) _rVal = true;
                        
                    }
                }
                return _rVal;
            }
            catch (Exception ex)
            {
                return _rVal;
                throw ex;
            }
        }

        public List<double> UniqueValues(int aPrecis = -1)
        {
            List<double> _rVal = new List<double>();
            if (aPrecis < 0) aPrecis = Precision;
            aPrecis = mzUtils.LimitedValue(aPrecis,0, 15);
            for(int r = 1; r <= Rows; r++)
            {
                for(int c = 1 ; c <= Cols; c++)
                {
                    double dval = Math.Round(Value(r, c), aPrecis);
                    if(_rVal.IndexOf(dval) < 0 ) _rVal.Add(dval);
                }
            }
            return _rVal;
        }

        public List<uopMatrixCell> UniqueCells(int aPrecis = -1)
        {
            List<uopMatrixCell> _rVal = new List<uopMatrixCell>();
            if (aPrecis < 0) aPrecis = Precision;
            aPrecis = mzUtils.LimitedValue(aPrecis, 0, 15);
            for (int r = 1; r <= Rows; r++)
            {
                for (int c = 1; c <= Cols; c++)
                {
                    double dval = Math.Round(Value(r, c), aPrecis);

                    if (_rVal.FindIndex(x => x.Value == dval) < 0) _rVal.Add(new uopMatrixCell(r,c, dval));
                }
            }
            return _rVal;
        }

        /// <summary>
        /// returns all the cells whose current value matches the passed vaue
        /// </summary>
        /// <param name="aValue"></param>
        /// <param name="aPrecis">an override precision to apply for the comparison</param>
        /// <param name="aStartRow">the starting row index to scan. <=0 defaults to 1</param>
        /// <param name="aEndRow">the ending row index to scan. <=0 defaults to the current count</param>
        /// <param name="aStartCol">the starting column index to scan. <=0 defaults to 1</param>
        /// <param name="aEndCol">the ending column index to scan. <=0 defaults to the current column count</param>        /// <returns></returns>
        public List<uopMatrixCell> ValueCells(double aValue, int? aPrecis = null,  int aStartRow = 0, int aEndRow = 0, int aStartCol = 0, int aEndCol = 0  )
        {


            List<uopMatrixCell> cells = Cells(false, aStartRow, aEndRow, aStartCol, aEndCol);


            int prec = aPrecis ?? Precision ;
            prec = mzUtils.LimitedValue(prec, 0, 15);
            double dval = Math.Round(aValue, prec);
            return cells.FindAll(x => Math.Round(x.Value, prec) == dval);


        }

        /// <summary>
        /// returns all the cells that are not currently equal to zero
        /// </summary>
        /// <param name="aPrecis">the precision to apply for the comparison. If not passed, the current matrix precision is used</param>
        /// <param name="aStartRow">the starting row index to scan. <=0 defaults to 1</param>
        /// <param name="aEndRow">the ending row index to scan. <=0 defaults to the current count</param>
        /// <param name="aStartCol">the starting column index to scan. <=0 defaults to 1</param>
        /// <param name="aEndCol">the ending column index to scan. <=0 defaults to the current column count</param>
        /// <returns></returns>
        public List<uopMatrixCell> NonZeroCells(  int? aPrecis = null, int aStartRow = 0, int aEndRow = 0, int aStartCol = 0, int aEndCol = 0)
        {

            List<uopMatrixCell> cells = Cells(false,aStartRow,aEndRow,aStartCol,aEndCol);
            

            int prec = aPrecis ?? Precision ;
            prec = mzUtils.LimitedValue(prec, 0, 15);
           return  cells.FindAll(x => Math.Round(x.Value, prec) != 0);

            //for (int r = 1; r <= Rows; r++)
            //{
            //    if (aRow <= 0 || r == aRow)
            //    {
            //        for (int c = 1; c <= Cols; c++)
            //        {
            //            if (aCol <= 0 || c == aCol)
            //            {
            //                double dval = Math.Round(Value(r, c, 0));
            //                if (dval != 0) _rVal.Add(new uopMatrixCell(r, c, dval));
            //            }

            //            if (aCol > 0 & c >= aCol) break;
            //        }
            //    }

            //    if (aRow > 0 & r >= aRow) break;
            //}


            //return _rVal;
           
        }
        public bool SetMemberCells(List<uopMatrixCell> aCells, bool? bSymmetric = null, bool bSuppressEvents = false, double? aValue = null, bool bGrowToInclude = false, bool bSuppressIndexErrors = true, bool? bInVis = null)
        {
            if (aCells == null) return false;
            bool _rVal = false;

            foreach (uopMatrixCell cell in aCells)
            {
                if (SetValue(cell.Row, cell.Col, aValue ==null ? cell.Value : aValue.Value , bSymmetric, bSuppressEvents, bGrowToInclude, bSuppressIndexErrors, bInvis: bInVis.HasValue ? bInVis.Value: cell.Invisible)) _rVal = true;
            }
            return _rVal;
        }
        public virtual void PrintToConsole(int StartRow = 0, int EndRow = 0, int StartCol = 0, int EndCol = 0, string aHeading = null, int? aPrecis = null)
        {

            mzUtils.LoopLimits(StartRow, EndRow, 1, Rows, out int sr, out int er);
            


            System.Diagnostics.Debug.WriteLine("\n");
            if (!string.IsNullOrWhiteSpace(aHeading)) System.Diagnostics.Debug.WriteLine(aHeading);
            if (!string.IsNullOrWhiteSpace(Name)) System.Diagnostics.Debug.WriteLine(Name);

             string fmat = GetFMat(aPrecis);
            string val;
            string tRow;
            for (int r = sr; r <= er; r++)
            {
                uopMatrixRow row = Row(r);
                tRow = string.Empty;
                mzUtils.LoopLimits(StartCol, EndCol, 1, row.Cols, out int sc, out int ec);
                for (int c = sc; c <= ec; c++)
                {

                    val = row.Value(c).ToString(fmat);
                    if (row.Value(c) == 0)
                    {
                        val = new string(' ', val.Length);
                    }
                    tRow += val;
                    if (c < ec) tRow += " | ";
                }
                System.Diagnostics.Debug.WriteLine(tRow);
            }

        }

        private string GetFMat(int? aPrecis = null)
        {

            int rs = !aPrecis.HasValue ? Precision : mzUtils.LimitedValue(aPrecis.Value, 0, 15);
            int ls = 1;

            string fmat = $"0.{new string('0', rs)}"; // string.Format(Precision.ToString(), "0");

            for (int r = 1; r <= Rows; r++)
            {
                for (int c = 1; c <= Cols; c++)
                {
                    string aVal = Value(r, c).ToString(fmat); // string.Format("fmat", Values[r, c]);
                    while (!(mzUtils.Right(aVal, 1) != "0"))
                    {
                        aVal = mzUtils.Left(aVal, aVal.Length - 1);
                    }
                    if (mzUtils.Right(aVal, 1) == ".") aVal = mzUtils.Left(aVal, aVal.Length - 1);


                    int dt = aVal.IndexOf(".");
                    string v1;
                    string v2 = string.Empty;
                    if (dt > 0)
                    {
                        v1 = aVal.Substring(0, dt); // mzUtils.Left(aVal, dt - 1);
                        v2 = aVal.Substring(dt + 1, aVal.Length - dt - 1); // mzUtils.Right(aVal, aVal.Length - dt);
                    }
                    else
                    {
                        v1 = aVal;
                    }

                    if (v1.Length > ls) ls = v1.Length;

                    if (v2.Length > rs) rs = v2.Length;

                }
            }

            return $"{new string('0', ls)}.{new string('0', rs)}"; ;
        }

        /// <summary>
        /// used to add a specified number of columns to the matrix
        /// ~from 1 to 100 columns can be added at one time.
        /// ~values exceeding this limit are ignored.
        /// #1 the number of columns to add to matrix
        /// </summary>
        /// <param name="ColsToAdd"></param>
        public bool AddColumns(int ColsToAdd)
        {
            int wuz = Cols;
            Cols = wuz + ColsToAdd;
            bool _rVal = wuz != Cols;
            if (_rVal && MonitorChanges) eventDimensionChange?.Invoke();
            return _rVal;
        }

        /// <summary>
        /// used to add a specified number of rows to the matrix
        /// ~from 1 to 100 rows can be added at one time.
        /// ~values exceeding this limit are ignored.
        /// #1the number of rows to add to matrix
        /// </summary>
        /// <param name="RowsToAdd"></param>
        public bool AddRows(int RowsToAdd)
        {

            int wuz = Rows;
            Rows = wuz +RowsToAdd;
            bool _rVal = wuz != Rows;
            if (_rVal && MonitorChanges) eventDimensionChange?.Invoke();
            return _rVal;
        }

        /// <summary>
        /// used to add the columns of the passed matrix tto the right end of this matrix
        /// ~the passed matrix must have the same number of rows as the current matrix or an error is reaised
        /// #1 the matrix to Append the matrix
        /// </summary>
        /// <param name="aMatrix"></param>
        public void AppendColumns(uopMatrix aMatrix)
        {
            try
            {
                if (aMatrix == null) throw new Exception($"[{System.Reflection.MethodBase.GetCurrentMethod().Name}] The passed matrix is undefined");

                int cl = Cols + 1;
                int i = 0;
                AddColumns(aMatrix.Cols);

                for (int r = 1; r <= Rows; r++)
                {
                    uopMatrixRow row = Row(r);
                    i = 1;
                    for (int c = cl; c <= Cols; c++)
                    {
                        row.SetValue(c, aMatrix.Value(r, i));
                        i += 1;
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        /// <summary>
        /// used to add the rows of the passed matrix tto the bottom of this matrix
        /// ~the passed matrix must have the same number of columns as the current matrix or an error is reaised
        /// #1 the matrix to Append the matrix
        /// </summary>
        /// <param name="aMatrix"></param>
        public void AppendRows(uopMatrix aMatrix)
        {
            try
            {
                if (aMatrix == null) throw new Exception($"[{System.Reflection.MethodBase.GetCurrentMethod().Name}] The passed matrix is undefined");
                if (aMatrix.Cols != Cols) throw new Exception($"[{System.Reflection.MethodBase.GetCurrentMethod().Name}] The passed matrix column count does not match this matrix column count.");

                int rw = Rows + 1;
          
                AddRows(aMatrix.Rows);

                for (int r = rw; r <= Rows; r++)
                {
                    uopMatrixRow row = Row(r);
                    int i = 1;
                    for (int c = 1; c <= Cols; c++)
                    {
                        row.SetValue(c, aMatrix.Value(i, c));
                     
                    }
                    i += 1;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public int GetColIndex(string aStringVal, bool bIgnoreCase = true, int aOccurs = 1)
        {
            aStringVal ??= string.Empty;
            if (aOccurs <= 0 ) aOccurs = 1;
            if (ColumnHeaders == null) return 0;
            List<string> matches = ColumnHeaders.FindAll(x => string.Compare(x, aStringVal, true) == 0);
          
            return matches.Count < aOccurs ? 0 : ColumnHeaders.IndexOf( matches[aOccurs - 1]) + 1;

        }

        public int GetRowIndex(string aStringVal, bool bIgnoreCase = true, int aOccurs = 1)
        {
            aStringVal ??= string.Empty;
            if (aOccurs <= 0) aOccurs = 1;
            if (RowHeaders == null) return 0;
            List<string> matches = RowHeaders.FindAll(x => string.Compare(x, aStringVal, true) == 0);

            return matches.Count < aOccurs ? 0 : RowHeaders.IndexOf(matches[aOccurs - 1]) + 1;

        }

        /// <summary>
        /// returns an new matrix with the same properties and values as the cloned matrix
        /// </summary>
        /// <returns></returns>
        public uopMatrix Clone() => new uopMatrix(this);

        object ICloneable.Clone() => (object)this.Clone();

        /// <summary>
        /// returns the summation of column values from the indicated start row to the indicated end row
        /// ~the starting row is assummed to be 1 if not passed or invalid.
        /// ~the ending row is assummed to be the last row if not passed or invalid.
        /// ~an error is raised if the requested column is outside the bounds of the current matrix.
        /// #1 the column to perform the summation on
        /// #2 the row to start the summation
        /// #3 the row to end the summation
        /// </summary>
        /// <param name="aCol"></param>
        /// <param name="StartRow"></param>
        /// <param name="EndRow"></param>
        /// <returns></returns>

        public double ColumnTotal(int aCol, int StartRow = 0, int EndRow = 0)
        {

            double _rVal = 0;
            try
            {
                if (aCol <= 0 || aCol > Cols) throw new Exception($"[{System.Reflection.MethodBase.GetCurrentMethod().Name}] The requested column Is outside the bounds of the matrix");

                List<uopMatrixCell> column = Column(aCol, false, StartRow, EndRow);
                foreach (var cell in column) _rVal += cell.Value;

                return _rVal;
            }
            catch (Exception ex)
            {
                return _rVal;
                throw ex;
            }
        }

        public void CopyRow(uopMatrix aMatrix, int aCopyRow, int aSaveRow, dynamic StartCol = null, dynamic EndCol = null)
        {
            if (aMatrix == null) return;
            if (aSaveRow < 1 || aSaveRow > Rows) throw new IndexOutOfRangeException();

            mzUtils.LoopLimits(mzUtils.VarToInteger(StartCol), mzUtils.VarToInteger(EndCol), 1, aMatrix.Cols, out int sc, out int ec);
            for (int m = sc; m <= ec; m++)
            {
                SetValue(aSaveRow, m, aMatrix.Value(aCopyRow, m));
            }
        }
     

        public override string ToString()
        {
            return string.IsNullOrEmpty(Name) ? $"uopMatrix [{ Dimensions }]" : $"uopMatrix [{ Name }] [{ Dimensions }]";
        }


        public bool Resize(int aRows, int aCols, double aInitValue = 0, bool bInvisibleVal = false)
        {

            aRows = mzUtils.LimitedValue(aRows, 1, uopMatrix.Maxsize);
            aCols = mzUtils.LimitedValue(aCols, 1, uopMatrix.Maxsize);
            if (aRows == Rows && aCols == Cols) return false;

            int? rows = null;
            if(aRows !=Rows) rows = aRows;
            int? cols = aCols != Cols  ? aCols : Cols;


            List<uopMatrixRow> myrows = new List<uopMatrixRow>();
            myrows.AddRange(this);
            base.Clear();

            bool _rVal = uopMatrix.ResizeMatrix(ref myrows, rows, cols, aInitValue, bInvisibleVal, Precision);
            _Cols = cols.Value;
            base.AddRange(myrows);
            if (_rVal && MonitorChanges) eventDimensionChange?.Invoke();
            return _rVal;

        }

        /// <summary>
        /// populates the matrix with the values in the passed descriptor string
        /// #1 a matrix descriptor string
        /// </summary>
        /// <param name="DescriptorString"></param>
        public void DefineByString(string DescriptorString)
        {
            if (string.IsNullOrEmpty(DescriptorString)) return;

            string[] vals;

            string aVal = string.Empty;
            int cl = 0;
            DescriptorString = mzUtils.RemoveLeadChar(DescriptorString.Trim(), ':');
            DescriptorString = mzUtils.RemoveTrailingChar(DescriptorString.Trim(), ':');

            while (!(mzUtils.Left(DescriptorString, 1) != ":"))
            {
                DescriptorString = mzUtils.Right(DescriptorString, DescriptorString.Length - 1);
            }
            if (string.IsNullOrEmpty(DescriptorString)) return;

            string[] rws = DescriptorString.Split(':');


            Resize(rws.Length, 1);
            for (int n = 1; n <= Rows; n++)
            {
                aVal = rws[n - 1];
                if (aVal.IndexOf(",") > 0)
                {
                    vals = aVal.Split(',');
                    cl = vals.Length + 1;
                    if (cl > 0)
                    {
                        if (cl > Cols)
                        {
                            AddColumns(cl - Cols);
                        }
                        for (int m = 0; m < cl - 1; m++)
                        {
                            aVal = vals[m];
                            if (mzUtils.IsNumeric(aVal))
                            {
                                SetValue(n + 1, m + 1, mzUtils.VarToDouble( aVal));
                            }
                        }
                    }
                }
            }
            RoundValues();
        }

        //^rearranges the rows of the matrix such that the rows with the most leading
        //^zeros are at the bottom and the rows with the fewest leading zeros are at the top
        //~returns the column index of the column with the leftest leading entry
        //#1the row to begin at

        public int Diagonalize(int StartRow = 1)
        {
            if (MonitorChanges) _TempStore = new uopMatrix(this);

            int _rVal = 0;
            StartRow = mzUtils.LimitedValue(StartRow, 1, Rows);
            int zcnt = ZeroRowsToBottom(StartRow);
            if (zcnt == Rows) return _rVal;


            int tp = StartRow;
            int bt = Rows - zcnt;
            List<int> leads = new List<int> { };
            bool isDone = false;

            int swaps = 0;
            //only work to the bottom of the non-zero rows
            leads.Clear();

            //find the column position of the lead entries of all the non-zero rows

            _rVal = xxGetLeads(tp, bt, out leads);

            //bail since there is only one non-zero row
            if (bt <= StartRow) return _rVal;

            isDone = xxCheckForDone(tp, bt, leads);

            //find a row with the maximum lead entry in the leftest column
            //and swap it to the the diagonal row

            while (!isDone)
            {
                swaps = 0;
                for (int n = tp; n < bt - 1; n++)
                {
                    if (leads[n + 1] < leads[n])
                    {
                        SwapRow(n + 1, n);
                        swaps++;
                    }
                }
                xxGetLeads(tp, bt, out leads);
                isDone = swaps == 0;
                isDone = true;
                for (int i = bt; i < tp + 1; i++)
                {
                    isDone = leads[i - 1] < leads[i];
                    if (!isDone) break;

                }
            }



            if (MonitorChanges)
            {
                if (!IsEqual(_TempStore))
                {
                    eventMatrixValueChange?.Invoke();
                }
            }

            return _rVal;

        }


        /// <summary>
        /// returns the echelon form or reduced echelon form of the current matrix
        /// </summary>
        /// <param name="Reduced">flag to return the reduced echelon matrix (lead entries reduced to 1)</param>
        /// <returns></returns>
        public uopMatrix EchelonMatrix(bool Reduced = false)
        { 
            uopMatrix _rVal = Clone();

            //move zero rows to the bottom
            int zrows = _rVal.ZeroRowsToBottom();
            //bail if all rows are zero
            if (zrows >= Rows) return _rVal;


            int rw = 1;


            //get the leftest column with a non-zero entry
            //and diagonalize the matrix

            _rVal.Diagonalize(rw);
            int bt = Rows - zrows;

            while (rw <= bt)
            {
                //get the column of the row with the leading entry ang get its leading entry
                int cl = _rVal.LeadingRowEntry(rw, 1, out double x1);
                if (cl != 0)
                {
                    _rVal.MoveColumnMax(cl, rw, true, rw, bt);
                    cl = _rVal.LeadingRowEntry(rw, 1, out x1);
                }
                //clear all the subsequent rows below the leading column
                if (x1 != 0)
                {
                    for (int n = rw + 1; n < bt; n++)
                    {
                        int c2 = _rVal.LeadingRowEntry(n, 1, out double x2);
                        if (c2 == cl)
                        {
                            _rVal.MultiplyAndAddRow(rw, n, -x2 / x1);
                        }
                    }
                }
                //move to the next row
                rw++;

                if (rw < bt)
                {
                    _rVal.Diagonalize(rw);
                }
            }

            ///Done:

            if (Reduced)
            {
                //create only leading entries of 0
                for (int n = 0; n < bt; n++)
                {
                    int cl = _rVal.LeadingRowEntry(n, 1, out double x3);
                    if (x3 != 0 & x3 != 1)
                    {
                        _rVal.MultiplyRow(n, 1 / x3);
                    }
                }

                //now clear remaining entries in other rows that are non zero
                for (int n = 2; n < bt; n++)
                {
                    int cl = _rVal.LeadingRowEntry(n, 1, out double cVal);
                    if (cl != 0)
                    {
                        for (rw = n - 1; rw < 1; rw++)
                        {
                            double x1 = _rVal.GetMember(rw, cl);
                            if (x1 != 0)
                            {
                                _rVal.MultiplyAndAddRow(n, rw, -x1);
                            }
                        }
                    }
                }
            }
            return _rVal;
        }
        /// <summary>
        /// 1returns the number of time the passed value occurs in then matrix
        /// #1the value to search for
        /// #2a flag to mmake the comparison on an absoult value basis
        /// </summary>
        /// <param name="aValue"></param>
        /// <param name="AbsoluteVal"></param>
        /// <returns></returns>
        public int EntryCount(dynamic aValue, bool AbsoluteVal)
        {
            int _rVal = 0;
            if (!mzUtils.IsNumeric(aValue)) return _rVal;

            double comp = mzUtils.VarToDouble(aValue, AbsoluteVal, aPrecis: Precision);
            for (int n = 0; n < Rows; n++)
            {
                uopMatrixRow row = Row(n);
                for (int m = 0; m < Cols; m++)
                {
                    if (!AbsoluteVal)
                    {
                        if (row.Value( m) == comp) _rVal++;

                    }
                    else
                    {
                        if (Math.Abs(row.Value(m)) == Math.Abs(comp)) _rVal++;

                    }
                }
            }
            return _rVal;
        }


        /// <summary>
        /// returns a number from the matrix
        /// ~all array vaues are stored as doubles
        /// #1 the row position to return
        /// #2 the column position to return
        /// </summary>
        /// <param name="aRow"></param>
        /// <param name="aCol"></param>
        /// <returns></returns>
        public double GetMember(int aRow, int aCol, bool SuppressErrs = false)
        {
            try
            {
                if (aRow <= 0 || aRow > Rows)
                {
                    if (!SuppressErrs) throw new Exception($"[{System.Reflection.MethodBase.GetCurrentMethod().Name}] The requested row Is outside the bound of the matrix");
                    return 0;
                }

                if (aCol <= 0 || aCol > Cols)
                {
                    if (!SuppressErrs) throw new Exception($"[{System.Reflection.MethodBase.GetCurrentMethod().Name}] The requested column Is outside the bound of the matrix");
                    return 0;
                }
                return Row(aRow).Value(aCol);

            }
            catch (Exception e) { return 0; throw e; }
        }

        /// <summary>
        /// returns an identity matrix of the same dimensions as current matrix
        /// </summary>
        /// <returns></returns>
        public uopMatrix IdentityMatrix()
        {
            uopMatrix _rVal = new uopMatrix(this) { Name = "Identity" };
            for (int n = 1; n <= Rows; n++)
            {
                for (int m = 1; m <= Cols; m++)
                {
                    if (n == m) _rVal.SetValue(n, m, 1);

                }
            }

            return _rVal;
        }

        /// <summary>
        /// returns the inverse matrix of the current matrix
        /// ~an error is raised if the matrix is not invertible
        /// </summary>
        /// <returns></returns>
        public uopMatrix Inverse()
        {

            if (!IsSquare) throw new Exception($"[{System.Reflection.MethodBase.GetCurrentMethod().Name}] The matrix is not invertable because it is not square");
            //copy the current values to a new matrix
            uopMatrix aCopy = Clone();
            uopMatrix iMat = aCopy.IdentityMatrix();
            int m = 0;
            int n = 0;
            double aVal = 0;
            aCopy = Clone();

            //append the identity matrix to the copy
            aCopy.AppendColumns(iMat);

            //now get the reduced echelon
            iMat = aCopy.EchelonMatrix();

            //check to see if the left half was reduced to the identity matrix
            for (n = 0; n < Rows; n++)
            {
                m = iMat.LeadingRowEntry(n, 1, out aVal);
                if (m != n || aVal != 1)
                {
                    throw new Exception($"[{System.Reflection.MethodBase.GetCurrentMethod().Name }] The matrix is not invertable");
                }
            }

            //return the right side as the _rVal
            iMat.SplitVertically(Rows, out uopMatrix aMat, out aCopy);

            return aCopy;
        }

        /// <summary>
        /// returns True if the passed matrix is elementwise equal to the current matrix
        /// </summary>
        /// <param name="aMatrix">the matrix to compare</param>
        /// <returns></returns>
        public bool IsEqual(uopMatrix aMatrix, int? aPrec = null, bool bCompareGroupIDs = false, bool bCompareOverrides =  false)
         => uopMatrix.CompareTo(this, aMatrix, aPrec, bCompareGroupIDs, bCompareOverrides);
        

        /// <summary>
        /// searchs the indicated column for the the first row with a non zero entry
        /// </summary>
        /// <param name="SearchCol">the column to search</param>
        /// <param name="rEntry">returns the value found</param>
        /// <param name="StartRow">the row to start the search from (assummed 1 if not passed)</param>
        /// <returns></returns>
        public int LeadingColumnEntry(int SearchCol, out double rEntry, int? StartRow = null )
        {
           
            int _rVal = 0;
            rEntry = 0;
            if (SearchCol <= 0 || SearchCol > Cols) return _rVal;
            if(!StartRow.HasValue) StartRow =1;

            int sr = mzUtils.LimitedValue(StartRow.Value, 1, Rows);
            for (int r = sr; r <= Rows; r++)
            {

                uopMatrixRow row = this[r - 1];
                double aVal =  row.Value(SearchCol);
                if (aVal != 0)
                {
                    _rVal = r;
                    rEntry = aVal;
                    break;
                }
            }
            return _rVal;
        }

        /// <summary>
        /// returns a list of doubles that are the values of the requested row or colu
        /// ~reterns all zeros if the requested row is not withing the bounds of the matrix
        /// </summary>
        /// <returns></returns>
        public List<double> GetValues(int aRowOrCol, bool bSearchCols) => GetValues(aRowOrCol,bSearchCols, out _);

        /// <summary>
        /// returns a list of doubles that are the values of the requested row or colu
        /// ~reterns all zeros if the requested row is not withing the bounds of the matrix
        /// </summary>
        /// <returns></returns>
        public List<double> GetValues(int aRowOrCol, bool bSearchCols, out double rTotal) 
        {
            rTotal = 0;
            List<double> _rVal = new List<double>();
        
            List<uopMatrixCell> cells = null;
            if (!bSearchCols)
                cells = Row(aRowOrCol);
            else
                cells = Column(aRowOrCol);

                if (cells == null) return _rVal;
            
            foreach (uopMatrixCell cell in cells) { _rVal.Add(cell.Value); rTotal += cell.Value; }

            return _rVal;

        }


        /// <summary>
        /// returns a number from the matrix
        /// </summary>
        /// //<remarks>all array vaues are stored as doubles</remarks>
        /// <param name="aRow">1 the row position to return</param>
        /// <param name="aCol">the column position to return</param>
        /// <returns></returns>
        public double Value(int aRow, int aCol, double aDefault = 0, int? aPrecis = null) 
        { 
            uopMatrixCell cell = Cell(aRow, aCol);
            double _rVal = cell == null ? aDefault : cell.Value;
            int precis = aPrecis.HasValue ? mzUtils.LimitedValue(aPrecis.Value, 0, 15) : Precision;
            return Math.Round( _rVal,precis);
        }


        /// <summary>
        /// returns a cell from the matrix
        /// </summary>
        /// <param name="aRow"></param>
        /// <param name="aCol"></param>
        /// <returns></returns>
        public uopMatrixCell Cell(int aRow, int aCol, bool bGetClone = false) 
        {

            uopMatrixRow row = Row(aRow);
            return row == null ?null: row.Cell(aCol,bGetClone: bGetClone);
         
        }


        /// <summary>
        /// returns cells from the matrix
        /// </summary>
        /// <param name="aRow"></param>
        /// <param name="aCol"></param>
        /// <returns></returns>
        public List<uopMatrixCell> Cells(int aRow, int aCol,  uopInstances aInstances = null, bool bGetClone = false)
        {
            List<uopMatrixCell> _rVal = new List<uopMatrixCell>();
            uopMatrixCell cell = Cell(aRow,aCol, bGetClone: bGetClone);
            if(cell != null) _rVal.Add(cell);
            if (aInstances != null)
            {
                foreach (uopInstance inst in aInstances)
                {
                    cell = Cell(inst.Row, inst.Col, bGetClone: bGetClone);
                    if (cell != null) _rVal.Add(cell);
                }
            }
            return _rVal;
        }

        public uopMatrixRow Row(int aRow, bool bGetClones = false)
        {
            uopMatrixRow _rVal = aRow < 1 || aRow > Count ? null : !bGetClones ? base[aRow - 1] : new uopMatrixRow(base[aRow - 1]);
            if (_rVal != null)
            {
                _rVal.Row = aRow;
                _rVal.Cols = Cols;
                _rVal.Precision = Precision;
            }
            return _rVal;
        }

        public List<double> RowValues(int aRow,int StartCol= 0, int EndCol = 0)
        {
            List<double> _rVal = new List<double>();
            uopMatrixRow row = Row(aRow);
            if (row == null) return _rVal;
            mzUtils.LoopLimits(StartCol, EndCol, 1, row.Cols, out int sc, out int ec);
            for(int c = sc; c <= ec; c++)
            {
                _rVal.Add(row.Value(c));
            }
          
            return _rVal;
        }

        public List<uopMatrixCell> Column(int aCol, bool bGetClones = false,int StartRow = 0, int EndRow = 0)
        {
            List<uopMatrixCell> _rVal = new List<uopMatrixCell>();
            if (aCol > Cols) return _rVal;
            mzUtils.LoopLimits(StartRow, EndRow, 1, Rows, out int sr, out int er);
            for (int r = sr; r <= er; r++)
            {
                uopMatrixRow row = this[r - 1];
                if (row.Count < Cols) row.Resize(Cols);
                
                _rVal.Add(row.Cell(aCol, bGetClones));
                
            }

            return _rVal;
        }

        public List<double> ColumnValues(int aCol, int StartRow = 0, int EndRow = 0)
        {
            List<double> _rVal  = new List<double>();
            List<uopMatrixCell> column = Column(aCol, StartRow: StartRow, EndRow: EndRow);
            if (column == null)  return _rVal;
            foreach (var item in column)
            {
                _rVal.Add(item.Value);
            }
            return _rVal;
        }

        /// <summary>
        /// searchs the indicated row for the the first column with a non zero entry
        /// ~returns the column that contains the leading entry
        /// #1 the row to search
        /// #2 the column to start the search from (assummed 1 if not passed)
        /// #3 returns the value found
        /// </summary>
        /// <param name="SearchRow"></param>
        /// <param name="StartCol"></param>
        /// <param name="Entry"></param>
        /// <returns></returns>
        public int LeadingRowEntry(int SearchRow, int StartCol, out double rEntry)
        {
            int _rVal = 0;
            rEntry = 0;
            uopMatrixRow row = Row(SearchRow);
            if (row == null || row.Cols <=0) return 0;

            mzUtils.LoopLimits(StartCol, row.Cols, 1, row.Cols, out int si, out int ei);
            

            double aVal;
            for (int m = si; m <= ei; m++)
            {
                aVal = row.Value(m);
                if (aVal != 0)
                {
                    _rVal = m;
                    rEntry = aVal;
                    break;
                }
            }
            return _rVal;
        }


      ///<summary>
        /// searchs the indicated column for the maximum value within the search parameters
        /// </summary>
        /// <remarks>
        /// returns the maximum value found
        /// </remarks>
        /// <param name="SearchCol">the column to search</param>
        /// <param name="AbsoluteVal">flag to treat all values as absolute</param>
        /// <param name="StartRow">the row to start the search from (assumed 1 if not passed or is invalid)</param>
        /// <param name="EndRow">the row to end the search in (assumed = Cols if not passed or is invalid)</param>
   /// <returns></returns>
        public double MaximumColumnEntry(int SearchCol, bool AbsoluteVal = false, int StartRow = 0, int EndRow = 0)
       => MaximumColumnEntry(SearchCol, AbsoluteVal, StartRow, EndRow, out _, out _);


        /// <summary>
        /// searchs the indicated column for the maximum value within the search parameters
        /// </summary>
        /// <remarks>
        /// returns the maximum value found
        /// </remarks>
        /// <param name="SearchCol">the column to search</param>
        /// <param name="AbsoluteVal">flag to treat all values as absolute</param>
        /// <param name="StartRow">the row to start the search from (assumed 1 if not passed or is invalid)</param>
        /// <param name="EndRow">the row to end the search in (assumed = Cols if not passed or is invalid)</param>
        /// <param name="rRowLocation">returns the row index that contains the mimimum entry</param>
        /// <param name="rNonZeroCount">returns the number of nonzero entries in the column</param>
        /// <returns></returns>
        public double MaximumColumnEntry(int SearchCol, bool AbsoluteVal, int StartRow, int EndRow, out int rRowLocation, out int rNonZeroCount)
        {
            dynamic _rVal = 0;
            rRowLocation = 0;
            rNonZeroCount = 0;

            if (SearchCol <= 0 || SearchCol > Cols) return _rVal;

            mzUtils.LoopLimits(StartRow, EndRow, 1, Count, out int sr, out int er);

            _rVal = Value(sr, SearchCol);
            rRowLocation = sr;

            for (int m = sr; m <= er; m++)
            {
               double aVal = Value(m, SearchCol);
                if (AbsoluteVal) aVal = Math.Abs(aVal);

                if (aVal != 0) rNonZeroCount++;

                if (aVal > _rVal)
                {
                    rRowLocation = m;
                    _rVal = aVal;
                }
            }
            return _rVal;
        }



        /// <summary>
        /// searchs the indicated column for the mimimum value within the search parameters
        /// </summary>
        /// <remarks>
        /// returns the minimum value found
        /// </remarks>
        /// <param name="SearchCol">the column to search</param>
        /// <param name="AbsoluteVal">flag to treat all values as absolute</param>
        /// <param name="StartRow">the row to start the search from (assumed 1 if not passed or is invalid)</param>
        /// <param name="EndRow">the row to end the search in (assumed = Cols if not passed or is invalid)</param>
        /// <param name="IncludeZeros">flag to include zero in the comparison</param>
        /// <returns></returns>
        public double MimimumColumnEntry(int SearchCol, bool AbsoluteVal = false, int StartRow = 0, int EndRow = 0, bool IncludeZeros = false)
        { return MimimumColumnEntry(SearchCol, AbsoluteVal, StartRow, EndRow, out int LOC, out int NZCNT, IncludeZeros); }


        /// <summary>
        /// searchs the indicated column for the mimimum value within the search parameters
        /// </summary>
        /// <remarks>
        /// returns the minimum value found
        /// </remarks>
        /// <param name="SearchCol">the column to search</param>
        /// <param name="AbsoluteVal">flag to treat all values as absolute</param>
        /// <param name="StartRow">the row to start the search from (assumed 1 if not passed or is invalid)</param>
        /// <param name="EndRow">the row to end the search in (assumed = Cols if not passed or is invalid)</param>
        /// <param name="rRowLocation">returns the row index that contains the mimimum entry</param>
        /// <param name="rNonZeroCnt">returns the number of nonzero entries in the column</param>
        /// <param name="IncludeZeros">flag to include zero in the comparison</param>
        /// <returns></returns>
        public double MimimumColumnEntry(int SearchCol, bool AbsoluteVal, int StartRow, int EndRow, out int rRowLocation, out int rNonZeroCnt, bool IncludeZeros = false)
        {
            rNonZeroCnt = 0;
            double _rVal = 0;
            rRowLocation = 0;

            if (SearchCol <= 0 || SearchCol > Cols) return _rVal;
            if (StartRow <= 0 || StartRow > Rows) StartRow = 1;
            if (EndRow <= 0 || EndRow > Rows || EndRow < StartRow) EndRow = Rows;



            _rVal = MaximumColumnEntry(SearchCol, AbsoluteVal, StartRow, EndRow, out int rLoc, out int _);

            for (int n = StartRow; n <= EndRow; n++)
            {
                double aVal = Value(n, SearchCol);
                if (AbsoluteVal) aVal = Math.Abs(aVal);

                if (aVal != 0)
                {
                    rNonZeroCnt += 1;
                }
                if ((!IncludeZeros && aVal == 0) || (!IncludeZeros && aVal != 0))
                {
                    if (aVal < _rVal)
                    {
                        rLoc = n;
                        _rVal = aVal;
                    }
                }
            }
            rRowLocation = rLoc;

            return _rVal;
        }
        /// <summary>
        /// searchs the indicated range of columns and rows for the maximum value within the search parameters
        /// ~returns the maximum entry
        /// #1flag to treat all values as positive
        /// #2the row to start the search from (assumed 1 if not passed or is invalid)
        /// #3the row to end the search in (assumed = Rows if not passed or is invalid)
        /// #4the column to start the search from (assumed 1 if not passed or is invalid)
        /// #5the column to end the search in (assumed = Cols if not passed or is invalid)
        /// </summary>
        /// <param name="AbsoluteVal"></param>
        /// <param name="StartRow"></param>
        /// <param name="EndRow"></param>
        /// <param name="StartCol"></param>
        /// <param name="EndCol"></param>

        /// <summary>
        /// searchs the indicated range of columns and rows for the maximum value within the search parameters
        /// </summary>
        ///<remarks>returns the maximum entry</remarks>
        /// <param name="AbsoluteVal">flag to treat all values as positive</param>
        /// <param name="StartRow">the row to start the search from (assumed 1 if not passed or is invalid)</param>
        /// <param name="EndRow">the row to end the search in (assumed = Rows if not passed or is invalid)</param>
        /// <param name="StartCol">the column to start the search from (assumed 1 if not passed or is invalid)</param>
        /// <param name="EndCol">the column to end the search in (assumed = Cols if not passed or is invalid)</param>
        /// <returns></returns>
        public double MaximumEntry(bool AbsoluteVal = false, int StartRow = 0, int EndRow = 0, int StartCol = 0, int EndCol = 0)
        => MaximumEntry(AbsoluteVal, StartRow, EndRow, StartCol, EndCol, out _, out _, out _);


        /// <summary>
        /// searchs the indicated range of columns and rows for the maximum value within the search parameters
        /// </summary>
        ///<remarks>returns the maximum entry</remarks>
        /// <param name="AbsoluteVal">flag to treat all values as positive</param>
        /// <param name="StartRow">the row to start the search from (assumed 1 if not passed or is invalid)</param>
        /// <param name="EndRow">the row to end the search in (assumed = Rows if not passed or is invalid)</param>
        /// <param name="StartCol">the column to start the search from (assumed 1 if not passed or is invalid)</param>
        /// <param name="EndCol">the column to end the search in (assumed = Cols if not passed or is invalid)</param>
        /// <param name="AbsoluteVal"></param>
        /// <param name="StartRow"></param>
        /// <param name="EndRow"></param>
        /// <param name="StartCol"></param>
        /// <param name="EndCol"></param>
        /// <param name="rRowLocation">returns the row position of the maximum entry</param>
        /// <param name="rColLocation">returns the column position of the maximum entry</param>
        /// <param name="rValCount"> returns the number of locations that hold the maximum value</param>
        /// <returns></returns>
        public double MaximumEntry(bool AbsoluteVal, int StartRow, int EndRow, int StartCol, int EndCol, out int rRowLocation, out int rColLocation, out int rValCount)
        {

            rRowLocation = 0;
            rValCount = 0;
            rColLocation = 0;

            if (StartRow <= 0 || StartRow > Rows) StartRow = 1;
            if (EndRow <= 0 || EndRow > Rows || EndRow < StartRow) EndRow = Rows;
            if (StartCol <= 0 || StartCol > Cols) StartCol = 1;
            if (EndCol <= 0 || EndCol > Cols || EndCol < StartCol) EndCol = Cols;


            double aVal = 0;

            rRowLocation = StartRow;
            rColLocation = StartCol;
            double _rVal = Value(StartRow, StartCol);

            for (int n = StartRow; n <= EndRow; n++)
            {
                for (int m = StartCol; m <= EndCol; m++)
                {
                    aVal = Value(n, m);
                    if (AbsoluteVal)
                    {
                        aVal = Math.Abs(aVal);
                    }
                    if (aVal > _rVal)
                    {
                        rRowLocation = n;
                        rColLocation = m;
                        _rVal = aVal;
                    }
                }
            }

            rValCount = 0;
            for (int n = StartRow; n < EndRow; n++)
            {
                for (int m = StartCol; m < EndCol; m++)
                {
                    aVal = Value(n, m);
                    if (AbsoluteVal) aVal = Math.Abs(aVal);

                    if (aVal == _rVal) rValCount++;

                }
            }

            return _rVal;
        }

        /// <summary>
        /// searchs the indicated row for the maximum value within the search parameters
        /// ~returns the max value found
        /// #1the row to search
        /// #2flag to treat all values as positive
        /// #3the column to start the search from (assumed 1 if not passed or is invalid)
        /// #4the column to end the search in (assumed = Cols if not passed or is invalid)
        /// #5returns the column index that contains the maximum entry
        /// #6returns the number of nonzero entries in the row
        /// </summary>
        /// <param name="SearchRow"></param>
        /// <param name="AbsoluteVal"></param>
        /// <param name="StartCol"></param>
        /// <param name="EndCol"></param>
        /// <param name="ColLocation"></param>
        /// <param name="ValCount"></param>
        /// <returns></returns>
        public double MaximumRowEntry(int SearchRow, bool AbsoluteVal, int StartCol, int EndCol, out int rColLocation, out int rNonZeroCnt)
        {
            rColLocation = 0;
            rNonZeroCnt = 0;
            double _rVal = 0;
            uopMatrixRow row = Row(SearchRow);
            if(row == null)  return _rVal;

            mzUtils.LoopLimits(StartCol, EndCol, 0, row.Count, out int si, out int ei);
            _rVal = row.Value(si);
            rColLocation = si;
            for (int c = si + 1; c <= ei; c++)
            {
                {
                    double Val = row.Value(c);
                    if (AbsoluteVal) Val = Math.Abs(Val);

                    if (Val != 0) rNonZeroCnt++;

                    if (Val > _rVal)
                    {
                        rColLocation = c;
                        _rVal = Val;
                    }
                }
            }
            return _rVal;
        }
        /// <summary>
        /// searchs the indicated row for the maximum value within the search parameters
        /// ~returns the max value found
        /// #1the row to search
        /// #2flag to treat all values as positive
        /// #3the column to start the search from (assumed 1 if not passed or is invalid)
        /// #4the column to end the search in (assumed = Cols if not passed or is invalid)
        /// #5returns the column index that contains the maximum entry
        /// #6returns the number of nonzero entries in the row
        /// </summary>
        /// <param name="SearchRow"></param>
        /// <param name="AbsoluteVal"></param>
        /// <param name="StartCol"></param>
        /// <param name="EndCol"></param>
        /// <param name="ColLocation"></param>
        /// <param name="ValCount"></param>
        /// <returns></returns>
        public double MaximumRowEntry(int SearchRow, bool AbsoluteVal, int StartCol = 0, int EndCol = 0)
        => MaximumRowEntry(SearchRow, AbsoluteVal, StartCol, EndCol,out _, out _);

        /// <summary>
        /// searchs the indicated row for the mimimum value within the search parameters
        /// ~returns the minimum value found within the search criteria
        /// #1the row to search
        /// #2flag to treat all values as absolute
        /// #3the column to start the search from (assumed 1 if not passed or is invalid)
        /// #4the column to end the search in (assumed = Cols if not passed or is invalid)
        /// #5returns the column index that contains the mimimum entry
        /// #6returns the number of nonzero entries in the row
        /// #7flag to include zero in the comparison
        /// </summary>
        /// <param name="SearchRow"></param>
        /// <param name="AbsoluteVal"></param>
        /// <param name="StartCol"></param>
        /// <param name="EndCol"></param>
        /// <param name="ColLocation"></param>
        /// <param name="ValCount"></param>
        /// <param name="IncludeZeros"></param>
        /// <returns></returns>
        public dynamic MimimumRowEntry(int SearchRow, bool AbsoluteVal, int StartCol = 0, int EndCol = 0, int rColLocation = 0, int rNonZeroCnt = 0, bool IncludeZeros = false)
        {
            rColLocation = 0;
            rNonZeroCnt = 0;
            double _rVal = 0;
            uopMatrixRow row = Row(SearchRow);
            if (row == null) return _rVal;

            mzUtils.LoopLimits(StartCol, EndCol, 0, row.Count, out int si, out int ei);
            _rVal = row.Value(si);
            rColLocation = si;
            for (int c = si + 1; c <= ei; c++)
            {
                {
                    double Val = row.Value(c);
                    if (AbsoluteVal) Val = Math.Abs(Val);

                    if (Val != 0) rNonZeroCnt++;

                    if (Val < _rVal)
                    {
                        rColLocation = c;
                        _rVal = Val;
                    }
                }
            }
            return _rVal;
        }

        /// <summary>
        /// swaps the row of the column containing then greatest non-zero entry to the indicated row poistion
        /// </summary>
        /// <param name="SearchCol">the column to work on</param>
        /// <param name="DestinationRow">the row to move the column maximum to</param>
        /// <param name="AbsoluteVal">flag to treat all values as positive</param>
        /// <param name="StartRow">the row to start the search from (assumed 1 if not passed or is invalid)</param>
        /// <param name="EndRow">the row to end the search in (assumed = Cols if not passed or is invalid)</param>
        public void MoveColumnMax(int SearchCol, int DestinationRow, bool AbsoluteVal, int StartRow = 0, int EndRow = 0)
        {

            if (SearchCol <= 0 || SearchCol > Cols) return;
            if (DestinationRow <= 0 || DestinationRow > Rows) return;

            if (StartRow <= 0 || StartRow > Cols) StartRow = 1;

            if (EndRow <= 0 || EndRow > Rows || EndRow < StartRow) EndRow = Rows;



            double maxval = MaximumColumnEntry(SearchCol, AbsoluteVal, StartRow, EndRow, out int n, out int cnt);

            //bail if there are no non-zero entries
            if (cnt == 0) return;

            if (n != DestinationRow) SwapRow(n, DestinationRow);

        }


        /// <summary>
        /// multiplies this matrix with the passed matrix
        /// #1 the matrix to multiply this one with
        /// </summary>
        /// <param name="aMatrix"></param>
        public void Multiply(uopMatrix aMatrix)
        {
            if (aMatrix == null)
            {
                throw new Exception($"[{System.Reflection.MethodBase.GetCurrentMethod().Name}] The passed matrix is undefined");
            }
            if (aMatrix.Rows != Cols)
            {
                throw new Exception($"[{System.Reflection.MethodBase.GetCurrentMethod().Name}] The passed matrix rows dimension is not equal to this matrix column dimension");
            }

            double[,] newVals = new double[1000, 1000]; // TODO - Specified Minimum Array Boundary Not Supported:     Dim newVals() As Double
            bool Notify =  MonitorChanges;
            MonitorChanges = false;

            int m = Rows;
            int p = Cols;
            int n = aMatrix.Cols;
            if (Notify) _TempStore = new uopMatrix(this);
            bool changed = false;
            newVals = new double[1000, 1000];

            List<List<double>> mVals = aMatrix.Values();

            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    double val1 = 0;
                    for (int k = 0; k < p; k++)
                    {
                        val1 += Value(i, k) *  mVals[k][ j];
                    }
                    newVals[i, j] = Math.Round( val1,Precision);
                }
            }

            Resize(m, n);
       
            for (int i   = 1; i <= m; i++)
            {
                uopMatrixRow row = Row(i);
                for (int j = 1; j <= n; j++)
                {
                    if (row.SetValue(j,newVals[i -1, j -1])) changed = true;
                }
            }

            if (Notify && changed)
            {
                if (!IsEqual(_TempStore))
                {
                    eventMatrixValueChange?.Invoke();
                }
            }
            MonitorChanges = Notify;
        }

        /// <summary>
        /// multiplies the values in the first indicated row by the passed multiplier and adds the result to the second indicated row.
        /// ~if Row1 = Row2 the request is ignored.
        /// ~if Row1 or Row2 = 0 the request is ignored.
        /// ~if the requested rows exceed the bounds of the current matrix an error is raised.
        /// #1the row to multiply and add
        /// #2the row to add the result of the multiplication to
        /// #2the value to multiply the row by
        /// </summary>
        /// <param name="Row1"></param>
        /// <param name="Row2"></param>
        /// <param name="Multiplier"></param>
        public void MultiplyAndAddRow(int Row1, int Row2, double Multiplier)
        {

            bool changed = false;
            try
            {
            
                if (Row1 <= 0 || Row1 > Rows) throw new Exception($"[{System.Reflection.MethodBase.GetCurrentMethod().Name}] The first indicated row index is outside the bounds of the current matrix");
                if (Row2 <= 0 || Row2 > Rows) throw new Exception($"[{System.Reflection.MethodBase.GetCurrentMethod().Name}] The second indicated row index is outside the bounds of the current matrix");

                bool _rVal = false;

                int m = 0;
                double[,] multis = new double[1000, 1000];
                double multi = 0;

                for (m = 1; m <= Cols; m++)
                {
                    multi = Value(Row1, m) * Multiplier;
                    _rVal = Value(Row2, m) != Math.Round(Value(Row2, m) + multi, Precision);
                    changed  = Cell(Row2, m).SetValue( Math.Round(Value(Row2, m) + multi, Precision));
                }

            }
            catch (Exception e) { throw e; }
            finally
            {
             
                if (changed && MonitorChanges) eventMatrixValueChange?.Invoke();
            }
            //return _rVal;

        }

        

        /// <summary>
        /// multiplies the matrix by the passed scaler value
        /// </summary>
        /// <param name="Scaler"></param>
        public void MultiplyByScaler(double Scaler)
        {
            int n = 0;
            int m = 0;
            bool Notify = false;

            if (Scaler == 1) return;
            

            for (int r = 1; r <= Rows; r++)
            {
                uopMatrixRow row = Row(r);
                for (int c = 1; c <= Cols; c++)
                {
                        if( row.SetValue(c, Math.Round(Value(n, m) * Scaler, Precision))) Notify = true;
               
                }
            }
            if (Notify && MonitorChanges)
            {
                eventMatrixValueChange?.Invoke();
            }
        }

        /// <summary>
        /// multiplies the values in the indicated column by the passed multiplier
        /// </summary>
        /// <remarks>
        ///  returns the changed cells
        /// </remarks>
        /// <param name="aCol"></param>
        /// <param name="aMultiplier"></param>
        /// <param name="aStartRow"></param>
        /// <param name="aEndRow"></param>
        public List<uopMatrixCell> MultiplyColumn(int aCol, double aMultiplier, int aStartRow = 0, int aEndRow = 0, bool bReplaceWithOverride = false)
        {
            List<uopMatrixCell> _rVal = new List<uopMatrixCell>();
            if (double.IsNaN(aMultiplier)) return _rVal;
                        bool changes = false;
            try
            {
                if (aCol > Cols)
                    throw new Exception($"[{System.Reflection.MethodBase.GetCurrentMethod().Name}] The indicated column index is outside the bounds of the current matrix");

           
                mzUtils.LoopLimits(aStartRow, aEndRow, 1, Count, out int sr, out int er);
                for (int r = sr; r <= er; r++)
                {
                    uopMatrixRow row = Row(r);
                    uopMatrixCell cell = row.Cell(aCol);

                    double val =  0;
                    if (bReplaceWithOverride && cell.OverrideValue.HasValue)
                        val = cell.OverrideValue.Value;
                    else
                        val = cell.Value * aMultiplier;

                    if (cell.SetValue(val))
                    {
                        changes = true;
                        _rVal.Add(cell);
                    }
                }
            }
            catch (Exception e) { throw e; }
            finally
            {
                if (changes && MonitorChanges) eventMatrixValueChange?.Invoke();
            }
            if (changes && MonitorChanges)
                eventMatrixValueChange?.Invoke();
            return _rVal;
        }

        /// <summary>
        /// multiplies the values in the indicated row by the passed multiplier
        /// </summary>
        /// <remarks>
        ///  returns the changed cells
        /// </remarks>
        /// <param name="aRow">the row to multiply</param>
        /// <param name="aMultiplier"></param>
        /// <param name="aStartCol">the starting column index to scan. <=0 defaults to 1</param>
        /// <param name="aEndCol">the ending column index to scan. <=0 defaults to the current column count</param>
        /// <param name="bReplaceWithOverride">if true, any cell that has a defined override value will not be multiplied but its' current value will be set to it's current override value</param>
        public List<uopMatrixCell>  MultiplyRow(int aRow, double aMultiplier, int aStartCol = 0, int aEndCol = 0, bool bReplaceWithOverride = false)
        {
            List < uopMatrixCell > _rVal = new List<uopMatrixCell>();
            bool changes = false;
            if (double.IsNaN(aMultiplier)) 
                return _rVal;
            try
            {
                if (aRow > Rows)
                    throw new Exception($"[{System.Reflection.MethodBase.GetCurrentMethod().Name}] The indicated row index is outside the bounds of the current matrix");

                uopMatrixRow row = Row(aRow);
                List<uopMatrixCell> cells = row.Cells(false, aStartCol, aEndCol);
                foreach(var cell in cells)
                {
                    double val = 0;
                    if (bReplaceWithOverride && cell.OverrideValue.HasValue)
                        val = cell.OverrideValue.Value;
                    else
                        val = cell.Value * aMultiplier;

                        if (cell.SetValue(val))
                    {
                        _rVal.Add(cell);
                        changes = true;
                    }
                }
                
            }
            catch (Exception e) { throw e; }
            finally
            {
                if (changes && MonitorChanges) eventMatrixValueChange?.Invoke();
            }

            return _rVal;
        }

        /// <summary>
        /// searchs the indicated range of columns and rows and returns the number of non-zero entries
        /// 
        /// #1the row to start the search from (assumed 1 if not passed or is invalid)
        /// #2the row to end the search in (assumed = Rows if not passed or is invalid)
        /// #3the column to start the search from (assumed 1 if not passed or is invalid)
        /// #4the column to end the search in (assumed = Cols if not passed or is invalid)
        /// </summary>
        /// <param name="StartRow"></param>
        /// <param name="EndRow"></param>
        /// <param name="StartCol"></param>
        /// <param name="EndCol"></param>
        /// <returns></returns>
        public int NonZeroEntries(int StartRow = 0, int EndRow = 0, int StartCol = 0, int EndCol = 0)
        {
            int _rVal = 0;

            mzUtils.LoopLimits(StartRow, EndRow, 1, Rows, out int sr, out int er);
            mzUtils.LoopLimits(StartCol, EndCol, 1, Rows, out int sc, out int ec);


            for (int n = sr; n <= er; n++)
            {
                for (int m = sc; m <= ec; m++)
                {
                    double dval = Math.Round(Value(n, m), Precision);
                    if (dval != 0)
                        _rVal++;
                }
            }
            return _rVal;
        }

        /// <summary>
        /// used to remove a specific row from matrix
        /// ~values exceeding the limits are ignored. the call is ignored if the matrix has only 1 row
        /// #1the row to remove from the matrix
        /// </summary>
        /// <param name="RowToRemove"></param>
        public bool RemoveRow(int RowToRemove)
        {
            
            if (RowToRemove < 0 || RowToRemove > Rows) return false;

            try
            {
                base.RemoveAt(RowToRemove - 1);

            }
            finally
            {
                for (int r =1; r <= Count; r++)
                {
                    this[r - 1].Row = r;
                }
                    if (MonitorChanges) eventDimensionChange?.Invoke();
            }

            return true;


        }

        /// <summary>
        /// used to remove a specific column from matrix
        /// ~values exceeding the limits are ignored. the call is ignored if the matrix has only 1 column
        /// #1the column to remove from the matrix
        /// </summary>
        /// <param name="ColToRemove"></param>
        public bool RemoveCol(int ColToRemove)
        {
            bool _rVal = false;

            foreach(uopMatrixRow row in this)
            {
                if(row.RemoveCell(ColToRemove) != null) _rVal = true;
            }

            if (_rVal && MonitorChanges) eventDimensionChange?.Invoke();
            return _rVal;
        }


        

        /// <summary>
        /// used to remove a specified number of rows from matrix
        /// ~from 1 to (rows -1) rows can be removed at one time.
        /// ~values exceeding the limits are ignored.
        /// #1the number of rows to remove from the matrix
        /// </summary>
        /// <param name="RowsToRemove"></param>
        public List<uopMatrixRow> RemoveRows(int RowsToRemove, int StartRow = 1)
        {
            RowsToRemove = Math.Abs(RowsToRemove);
            List<uopMatrixRow> removers = new List<uopMatrixRow>();
            StartRow = mzUtils.LimitedValue(StartRow, 1, Rows);

            for (int r = StartRow; r <= StartRow + RowsToRemove; r++)
            {
                if (r < 1 || r > Count) continue;
                removers.Add(this[r-1]);
            }
            foreach (uopMatrixRow row in removers)  base.Remove(row);

            for(int r =1; r <= Count; r++)
            {
                this[r -1].Row = r;
            }

            if (removers.Count > 0 && MonitorChanges) eventDimensionChange?.Invoke();
            return removers;
        }

        /// <summary>
        /// used to remove a specified number of columns from matrix
        /// ~from 1 to cols - 1 can be removed at one time.
        /// ~values exceeding the limits are ignored.
        /// #1the number of columns to remove from the matrix
        /// </summary>
        /// <param name="ColsToRemove"></param>
        public List<uopMatrixCell> RemoveCols(int ColsToRemove, int StartCol = 1)
        {
            ColsToRemove = Math.Abs(ColsToRemove);
            List < uopMatrixCell > _rVal = new List<uopMatrixCell>();
            StartCol = mzUtils.LimitedValue(StartCol, 1, Cols);


            for (int r = 1; r <= Count; r++)
            {
                uopMatrixRow row = this[r-1];
                _rVal.AddRange(row.RemoveCols(ColsToRemove, StartCol));

            }

            if (_rVal.Count > 0 && MonitorChanges) eventDimensionChange?.Invoke();
            return _rVal;

       
        }

        /// <summary>
        /// re-orders the columns from last to first
        /// </summary>
        public void ReverseColumns()
        {

            if (MonitorChanges) _TempStore = new uopMatrix(this);

            foreach(var row in this)
            {
                row.Reverse();
            }
            if (MonitorChanges)
            {
                if (!IsEqual(_TempStore))
                {
                    eventMatrixValueChange?.Invoke();
                }
            }

        }


        /// <summary>
        /// re-orders the rows from last to first
        /// </summary>
        public void ReverseRows()
        {

            if (Count <= 1) return;
             _TempStore = new uopMatrix(this);
            bool changed = false;
            
            for (int r = 1; r <= Count; r++)
            {
                int r2 = uopUtils.OpposingIndex(r, Count);
                uopMatrixRow old = _TempStore.Row(r2);
                uopMatrixRow cur = this.Row(r);
                
                    for (int c = 1; c <= cur.Count; c++)
                    {
                        if  (cur.Cell(c).SetValue(old.Cell(c).Value)) changed = true;
                    }
                
                
            }
//            this.Reverse();
         
            if (MonitorChanges)
            {
                if (changed)
                {
                    eventMatrixValueChange?.Invoke();
                }
            }

        }

        /// <summary>
        /// rounds all the matrix values to the current precision setting
        /// </summary>
        private bool RoundValues(int? aPrecis = null)
        {
            bool _rVal = false;
            int prec = aPrecis.HasValue ? mzUtils.LimitedValue(aPrecis.Value, 0, 15) : Precision;
            for (int r = 1; r <= Rows; r++)
            {
                uopMatrixRow row = Row(r);
                row.Row = r;
                foreach (var c in row)  if(c.SetValue(Math.Round(c.Value,prec))) _rVal = true;
            }

                return _rVal;
        }

        /// <summary>
        /// returns the summation of row values from the indicated start column to the indicated end column
        /// ~the starting column is assummed to be 1 if not passed or invalid.
        /// ~the ending column is assummed to be the last column if not passed or invalid.
        /// ~an error is raised if the requested row is outside the bounds of the current matrix.
        /// #1the row to perform the summation on
        /// #2the column to start the summation
        /// #3the column to end the summation
        /// </summary>
        /// <param name="aRow"></param>
        /// <param name="StartCol"></param>
        /// <param name="EndCol"></param>
        /// <returns></returns>
        public double RowTotal(int aRow, int StartCol = 0, int EndCol = 0)
        {
            double _rVal = 0;
            try
            {
           

                if (aRow < 1 || aRow > Rows) throw new Exception($"[{System.Reflection.MethodBase.GetCurrentMethod().Name}] The requested row Is outside the bound of the matrix");

                return Row(aRow).Total(StartCol,EndCol);

            }
            catch (Exception e) { return _rVal; throw e; }
        }



        /// <summary>
        /// used to set all the members of the matrix to the passed value
        /// ~non-numeric values are ignored with no error raised
        /// #1a column to set the values for
        /// #1the value to set all matrix members to
        /// </summary>
        /// <param name="aValue"></param>
        public bool SetAllValues(double aValue)
        {
            bool _rVal = false;
            foreach (var row in this) if (row.SetAllValues(aValue)) _rVal = true;
            if (_rVal && MonitorChanges) eventMatrixValueChange?.Invoke();
            return _rVal;
        }

        /// <summary>
        /// used to set all the members of a particular column to the passed value
        /// ~invalid column numbers or non-numeric values are ignored with no error raised
        /// #1a column to set the values for
        /// #2the value to set the column members to
        /// </summary>
        /// <param name="aColumn"></param>
        /// <param name="aValue"></param>
        public bool SetColumnValues(int aColumn, double aValue)
        {
            bool _rVal = false;
            foreach (var row in this) if (row.SetValue(aColumn, aValue)) _rVal = true;

            if (_rVal & MonitorChanges) eventMatrixValueChange?.Invoke();
            return _rVal;

        }
        /// <summary>
        /// allows the setting of the matrix dimension sin a Double step
        /// #1the number of desired rows
        /// #2the number of desired columns
        /// </summary>
        /// <param name="aRows"></param>
        /// <param name="aColumns"></param>
        public bool SetDimensions(int aRows, int aColumns) => Resize(aRows, aColumns);

        /// <summary>
        /// populars the values in a row with the past list of doubles
        /// #1the target Row 
        /// #2a list of double values
        /// </summary>
        /// <param name="aRow"></param>
        /// <param name="aValues"></param>
        public bool PopulateRow(int aRow, List<double> aValues,bool bGrowToInclude = false, bool? bInvis = null)
        {

            if (aValues == null) return false;
            if (aRow < 1 )  return false;
            bool _rVal = false;
            if (aRow > Count) 
            {
                if (!bGrowToInclude) return false;
                _rVal = true;
                    Resize(aRow,Cols,0, bInvis.HasValue ? bInvis.Value : false) ; 
                
            }
            uopMatrixRow row = Row(aRow);
            if (row == null) return false;


            if (row.Populate(aValues, Precision, bInvis)) _rVal = true;
            if (_rVal && MonitorChanges) eventMatrixValueChange?.Invoke();
            return _rVal;
        }
     
        /// <summary>
        /// populars the values in a row with the past list of doubles
        /// #1the target Row 
        /// #2a list of double matrix cells
        /// </summary>
        /// <param name="aRow"></param>
        /// <param name="aValues"></param>
        public bool PopulateRow(int aRow, List<uopMatrixCell> aValues,bool bGrowToInclude = false, bool? bInvis = null)
        {

            if (aValues == null) return false;
            if (aRow < 1 )  return false;
            bool _rVal = false;
            if (aRow > Count) 
            {
                if (!bGrowToInclude) return false;
                _rVal = true;
                    Resize(aRow,Cols, 0,bInvis.HasValue ? bInvis.Value : false) ; 
                
            }

            uopMatrixRow row = Row(aRow);
            if (row == null) return false;


            if (row.Populate(aValues, Precision, bInvis)) _rVal = true;
            if (_rVal && MonitorChanges) eventMatrixValueChange?.Invoke();
            return _rVal;
        }


        /// <summary>
        /// populars the values in a column with the past list of doubles
        /// #1the target Column
        /// #2a list of double values
        /// </summary>
        /// <param name="aRow"></param>
        /// <param name="aValues"></param>
        public bool PopulateCol(int aCol, List<double> aValues, bool bGrowToInclude = false, bool? bInvis = null)
        {

            if (aValues == null) return false;
            if (aCol < 1) return false;
            bool _rVal = false;
            if (aCol > Cols)
            {
                if (!bGrowToInclude) return false;

                Resize(Rows, aCol,0, bInvis.HasValue ? bInvis.Value : false);
                _rVal = true;
            }

            List<uopMatrixCell> col = Column(aCol);
            if (col == null || aValues == null) return false;


            for (int r = 1; r <= col.Count; r++)
            {
                if (r > aValues.Count) break;

                if (col[r - 1].SetValue(Math.Round(aValues[r - 1], Precision), bInvis ))  _rVal = true;
            }

            if (_rVal && MonitorChanges) eventMatrixValueChange?.Invoke();
            return _rVal;

        }


        /// <summary>
        /// populars the values in a column with the past list of doubles
        /// #1the target Column
        /// #2a list of matrix cells
        /// </summary>
        /// <param name="aRow"></param>
        /// <param name="aValues"></param>
        public bool PopulateCol(int aCol, List<uopMatrixCell> aValues, bool bGrowToInclude = false, bool? bInvis = null)
        {

            if (aValues == null) return false;
            if (aCol < 1) return false;
            bool _rVal = false;
            if (aCol > Cols)
            {
                if (!bGrowToInclude) return false;
                _rVal = true;
                Resize(Rows, aCol,0, bInvis.HasValue ? bInvis.Value : false);

            }
            
            List<uopMatrixCell> col = Column(aCol);
            if (col == null || aValues == null) return false;
            

            for (int r = 1; r <= col.Count; r++)
            {
                if(r > aValues.Count) break;
                if (col[r - 1].SetValue(Math.Round(aValues[r - 1].Value, Precision), bInvis))  _rVal = true;
            }

            if (_rVal && MonitorChanges) eventMatrixValueChange?.Invoke();
            return _rVal;
        }
        /// <summary>
        /// used to set a vaue in the matrix
        /// </summary>
        /// <param name="aCell">the cells whose row, column and value will be used to find the cell to set a value on this matrix </param>
        /// <param name="bSymmetric"></param>
        public bool MatchMember(uopMatrixCell aCell, bool? bSymmetric = null, bool bSuppressEvents = false, bool bAddTo = false, bool bGrowToInclude = false, bool bSuppressIndexErrors = true)
         =>  aCell == null ? false : SetValue(aCell.Row, aCell.Col, aCell.Value, bSymmetric, bSuppressEvents, bAddTo, bGrowToInclude, bSuppressIndexErrors);

        /// <summary>
        /// used to set a vaue in the matrix
        /// ~all array vaues are stored as doubles
        /// #1the row position to set the value in
        /// #2the column position to set the value in
        /// #3the value to set the member to
        /// </summary>
        /// <param name="aRow"></param>
        /// <param name="aCol"></param>
        /// <param name="aValue"></param>
        /// <param name="bSymmetric"></param>
        public bool SetMember(int aRow, int aCol, double aValue, bool? bSymmetric = null, bool bSuppressEvents = false,bool bAddTo = false, bool bGrowToInclude = false, bool bSuppressIndexErrors = true, bool? bInvis = null)
        {
           
            bool _rVal = false;
            bool Notify = false;
            uopMatrixCell cellchange = null;

            try
            {

                if (!bGrowToInclude)
                {
                    if (bSuppressIndexErrors)
                    {
                        if ((aRow < 1 || aRow > Rows) || (aCol < 1 || aCol > Cols))
                            return false;
                    }
                    
                    if (aRow <1 || aRow > Rows) throw new Exception($"[{System.Reflection.MethodBase.GetCurrentMethod().Name}] The requested row Is outside the bound of the matrix");
                    if (aCol < 1 || aCol > Cols) throw new Exception($"[{System.Reflection.MethodBase.GetCurrentMethod().Name}] The requested column Is outside the bound of the matrix");
                }
                else 
                {
                    if ((aRow < 1 || aRow > Rows) || (aCol < 1 || aCol > Cols))
                    {
                        Resize(aRow, aCol);
                        _rVal = true;
                    }
                }

                uopMatrixRow row = Row(aRow);
                
            
                if (bSymmetric.HasValue)
                {
                    if (bSymmetric.Value == true)
                    {
                        return SetMemberSymmetric(aRow, aCol, aValue, HSymetry: true, VSymetry: true,bSuppressEvents: bSuppressEvents, bAddTo: bAddTo, bGrowToInclude ).Count > 0;
                    }
                }
                double dVal = !bAddTo ? Math.Round(aValue, Precision) : Math.Round(aValue + Value(aRow, aCol), Precision);
                cellchange = new uopMatrixCell(aRow, aCol, dVal, Value(aRow, aCol));
                if ( row.SetValue(aCol, dVal, bInvis)) _rVal  = true;
                Notify = MonitorChanges && _rVal && !bSuppressEvents;

            }
            finally
            {
                if (Notify)
                {
                    eventMatrixValueChange?.Invoke();
                    eventMatrixSetMemberChange?.Invoke(new List<uopMatrixCell>() { cellchange });
                }
            }




            return _rVal;
        }

        /// <summary>
        /// used to set a color in the matrix
        /// </summary>
        /// <param name="aRow">the target row</param>
        /// <param name="aCol">the target column</param>
        /// <param name="aColor">the color to assign to the target cell</param>
             /// <param name="bSuppressIndexErrors">flag to suppres the index out of bounds error if the passed address is outside the current matrix dimensions</param>
        /// <param name="bInvis">an optional invisible value to assign to the target cell(s)</param>
        /// <param name="aInstances">if passed, the instances row and column properties are used to define the related cells to the passed value. no exceptions thrown if the address is out of bounds</param>
        public bool SetColor(int aRow, int aCol, System.Drawing.Color? aColor,   bool bSuppressIndexErrors = true, bool? bInvis = null, uopInstances aInstances = null)
        {

            bool _rVal = false;
        

            try
            {

            
                if (bSuppressIndexErrors)
                {
                    if ((aRow < 1 || aRow > Rows) || (aCol < 1 || aCol > Cols))
                        return false;
                }

                if (aRow < 1 || aRow > Rows) throw new Exception($"[{System.Reflection.MethodBase.GetCurrentMethod().Name}] The requested row Is outside the bound of the matrix");
                if (aCol < 1 || aCol > Cols) throw new Exception($"[{System.Reflection.MethodBase.GetCurrentMethod().Name}] The requested column Is outside the bound of the matrix");
            
                uopMatrixCell cell = Cell(aRow,aCol);
                _rVal = cell.Color != aColor;
                cell.Color = aColor;
                if(bInvis.HasValue) cell.Invisible = bInvis.Value;
                if (aInstances != null)
                {
                    foreach (var aInstance in aInstances)
                    {
                       cell = Cell(aInstance.Row, aInstance.Col);
                        if (cell != null)
                        {
                            if (cell.Color != aColor) _rVal = true;
                            cell.Color = aColor;
                            if (bInvis.HasValue) cell.Invisible = bInvis.Value;
                        }

                    }
                }

               

            }
            catch(Exception e) 
            {
                throw e;
            }




            return _rVal;
        }


        /// <summary>
        /// used to set a vaue in the matrix
        /// </summary>
        /// <param name="aRow">the target row</param>
        /// <param name="aCol">the target column</param>
        /// <param name="aValue">the value to assign to the target cell</param>
        /// <param name="bSymmetric">flag to set set the value of the target cell and its symmetric relatives</param>
        /// <param name="bAddTo">flag to add the passed value to the target cells value rather than overriding the current value</param>
        /// <param name="bGrowToInclude">flag to resize the matrix if the passed address is outside the current matrix dimensions</param>
        /// <param name="bSuppressIndexErrors">flag to suppres the index out of bounds error if the passed address is outside the current matrix dimensions</param>
        /// <param name="bInvis">an optional invisible value to assign to the target cell(s)</param>
        /// <param name="aInstances">if passed, the instances row and column properties are used to define the related cells to the passed value. no exceptions thrown if the address is out of bounds</param>
        public bool SetValue(int aRow, int aCol, double aValue, bool? bSymmetric = null, bool bSuppressEvents = false, bool bAddTo = false, bool bGrowToInclude = false, bool bSuppressIndexErrors = true, bool? bInvis = null, uopInstances aInstances = null)
        {

            bool _rVal = false;
            bool Notify = false;
            uopMatrixCell cellchange = null;

            try
            {

                if (!bGrowToInclude)
                {
                    if (bSuppressIndexErrors)
                    {
                        if ((aRow < 1 || aRow > Rows) || (aCol < 1 || aCol > Cols))
                            return false;
                    }

                    if (aRow < 1 || aRow > Rows) throw new Exception($"[{System.Reflection.MethodBase.GetCurrentMethod().Name}] The requested row Is outside the bound of the matrix");
                    if (aCol < 1 || aCol > Cols) throw new Exception($"[{System.Reflection.MethodBase.GetCurrentMethod().Name}] The requested column Is outside the bound of the matrix");
                }
                else
                {
                    if ((aRow < 1 || aRow > Rows) || (aCol < 1 || aCol > Cols))
                    {
                        Resize(aRow, aCol);
                        _rVal = true;
                    }
                }

                uopMatrixRow row = Row(aRow);


                if (bSymmetric.HasValue)
                {
                    if (bSymmetric.Value == true)
                    {
                        if( SetMemberSymmetric(aRow, aCol, aValue, HSymetry: true, VSymetry: true, bSuppressEvents: bSuppressEvents, bAddTo: bAddTo, bGrowToInclude).Count > 0) _rVal = true;
                    }
                }

                if(aInstances != null)
                {
                    foreach(var aInstance in aInstances)
                    {
                        uopMatrixCell cell = Cell(aInstance.Row, aInstance.Col);
                        if(cell != null)
                        {
                            double d1 = !bAddTo ? Math.Round(aValue, Precision) : Math.Round(aValue + cell.Value, Precision);
                            if (cell.SetValue(d1, bInvis)) _rVal = true;
                        }

                    }
                }

                double dVal = !bAddTo ? Math.Round(aValue, Precision) : Math.Round(aValue + Value(aRow, aCol), Precision);
                cellchange = new uopMatrixCell(aRow, aCol, dVal, Value(aRow, aCol));
                if (row.SetValue(aCol, dVal, bInvis)) _rVal = true;
                Notify = MonitorChanges && _rVal && !bSuppressEvents;

            }
            finally
            {
                if (Notify)
                {
                    eventMatrixValueChange?.Invoke();
                    eventMatrixSetMemberChange?.Invoke(new List<uopMatrixCell>() { cellchange });
                }
            }




            return _rVal;
        }

        /// <summary>
        /// used to get the matrix cells based on symetric positions to the value
            /// </summary>
        /// <param name="aRow">the target row</param>
        /// <param name="aCol">the target column</param>
        /// <param name="HSymetry">flag indicating if the horizontally symetric position value should also be set</param>
        /// <param name="VSymetry">flag indicating if the vertically symetric position value should also be set</param>
        /// <param name="bSuppresErrs"></param>
        public List<uopMatrixCell> GetMembersSymmetric(int aRow, int aCol, bool HSymetry = true, bool VSymetry = true, bool bSuppresErrs = false, bool bGetClones = false)
        {
            List<uopMatrixCell> _rVal = new List<uopMatrixCell>();
            if (Size <= 0) return _rVal;

            if (Cols <= 1) HSymetry = false;
            if (Rows <= 1) VSymetry = false;

            try
            {

                if (aRow <= 0 || aRow > Rows)
                {
                    if (!bSuppresErrs)
                        throw new Exception($"[{System.Reflection.MethodBase.GetCurrentMethod().Name}] The requested row Is outside the bound of the matrix");
                    else
                        return _rVal;
                }
                if (aCol <= 0 || aCol > _Cols)
                {
                    if (!bSuppresErrs)
                        throw new Exception($"[{System.Reflection.MethodBase.GetCurrentMethod().Name}] The requested column Is outside the bound of the matrix");
                    else
                        return _rVal;

                }
        
                uopMatrixRow row = Row(aRow);

                uopMatrixCell cell = row.Cell(aCol,bGetClones);

                _rVal.Add(cell);
                if (!HSymetry && !VSymetry) return _rVal;
                if (HSymetry)
                {
                    int n = uopUtils.OpposingIndex(aCol, Cols, aCol);
                    if (n != aCol)
                    {

                        cell = row.Cell(n, bGetClones);

                        if (cell != null) _rVal.Add(cell);

                    }


                }

                if (VSymetry)
                {
                    int cnt = _rVal.Count;
                    for (int i = 1; i <= cnt; i++)
                    {
                       cell = _rVal[i - 1];
                        aRow = cell.Row;
                        aCol = cell.Col;
                        int n = uopUtils.OpposingIndex(aRow, Rows, aRow);

                        if (n != aRow)
                        {
                            row = Row(n);
                            cell = row.Cell(aCol, bGetClones);
                            if(cell != null) _rVal.Add(cell);

                        }
                    }

                }

            }
            catch (Exception e)
            {
                if (!bSuppresErrs) throw e;
            }


            return _rVal;
        }

        /// <summary>
        /// used to set a value in the matrix with the addition of setting the symetric positions to the value
        /// </summary>
        /// <remark> all array vaues are stored as doubles.</remark>
        /// <param name="aRow">the row position to set the value in</param>
        /// <param name="aCol">the column position to set the value in</param>
        /// <param name="aValue">the value to set the member to</param>
        /// <param name="HSymetry">flag indicating if the horizontally symetric position value should also be set</param>
        /// <param name="VSymetry">flag indicating if the vertically symetric position value should also be set</param>
        /// <param name="bSuppressEvents">flag to suppress any change events</param>
        /// <param name="bAddTo">flagto add the value to the current value rather than replacing the current value</param>

        public List<uopMatrixCell> SetMemberSymmetric(int aRow, int aCol, double aValue, bool HSymetry = true, bool VSymetry = true, bool bSuppressEvents = false, bool bAddTo = false, bool bGrowToInclude = false)
        {

            if (!bGrowToInclude)
            {
                if (aRow < 1 || aRow > Rows) throw new Exception($"[{System.Reflection.MethodBase.GetCurrentMethod().Name}] The requested row Is outside the bound of the matrix");
                if (aCol < 1 || aCol > Cols) throw new Exception($"[{System.Reflection.MethodBase.GetCurrentMethod().Name}] The requested column Is outside the bound of the matrix");
            }
            else
            {
                if ((aRow < 1 || aRow > Rows) || (aCol < 1 || aCol > Cols)) Resize(aRow, aCol);
            }
            uopMatrixRow row = Row(aRow);
            List<uopMatrixCell> cells = GetMembersSymmetric(aRow, aCol, HSymetry, VSymetry, true);
            double val = Math.Round(aValue, Precision);
            List<uopMatrixCell> _rVal = new List<uopMatrixCell> ();
            foreach (uopMatrixCell cell in cells) 
            {
                if (cell.SetValue(val)) _rVal.Add(cell);
                
            }
       
            if (_rVal.Count > 0 && MonitorChanges && !bSuppressEvents)
            {
                eventMatrixValueChange?.Invoke();
                eventMatrixSetMemberChange?.Invoke(_rVal);
            }
            return _rVal;

        }
        /// <summary>
        /// used internally to reinitialize the row order arrray
        /// </summary>
        private void SetRowOrder() => SetRowOrder();

        /// <summary>
        /// used to set all the members of a particular row to the passed value
        /// ~invalid row numbers or non-numeric values are ignored with no error raised
        /// #1a row to set the values for
        /// #2the value to set the row members to
        /// </summary>
        /// <param name="aRow"></param>
        /// <param name="aValue"></param>
        public bool SetRowValues(int aRow, double aValue)
        {
            if (aRow <= 0 || aRow > Rows) return false;
            bool _rVal = false;
            uopMatrixRow row = Row(aRow);
            if(row == null) return false;
            double dVal = Math.Round(aValue, Precision);
            foreach(var cell in row) if(cell.SetValue(dVal)) _rVal = true;

            if (_rVal & MonitorChanges) eventMatrixValueChange?.Invoke();
            return _rVal;
        }

        /// <summary>
        /// creates two matrices by splitting the current matrix after the indicated row
        /// ~the split row is included in the Top side matrix.
        /// #1the row to perform the split
        /// #2returns the top side of the split result
        /// #3returns the bottom side of the split result
        /// </summary>
        /// <param name="aRow"></param>
        /// <param name="TopSide"></param>
        /// <param name="BottomSide"></param>
        public void SplitHorizontally(dynamic aRow, out uopMatrix rTopSide, out uopMatrix rBottomSide)
        {
            {
                rTopSide = new uopMatrix(this, bDontCopyMembers: true);
                rBottomSide = new uopMatrix(this, bDontCopyMembers: true);


                if (Rows <= 1) throw new Exception($"[{System.Reflection.MethodBase.GetCurrentMethod().Name}] a matrix with 1 or less rows cannot be split");

                if (!mzUtils.IsNumeric(aRow)) throw new Exception($"[{System.Reflection.MethodBase.GetCurrentMethod().Name}] The indicated row index is not numeric");

                double rw = mzUtils.VarToDouble(aRow);
                int splt = (int)Math.Floor(rw);
                if (mzUtils.IsOdd(Rows) && rw > splt) splt += 1;

                if (splt <= 1 || splt >= Rows)
                {
                    throw new Exception($"[{System.Reflection.MethodBase.GetCurrentMethod().Name}] The indicated row index is outside the bounds of the current matrix");
                }

                rTopSide = SubMatrix(1, splt, 1, Cols);
                rTopSide = SubMatrix(splt + 1, Rows, 1, Cols);


            }
        }

        /// <summary>
        /// creates two matrices by splitting the current matrix after the indicated column
        /// ~the split column is included in the left side matrix.
        /// #1the column to perform the split
        /// #2returns the left side of the split result
        /// #3returns the right side of the split result
        /// </summary>
        /// <param name="aCol"></param>
        /// <param name="LeftSide"></param>
        /// <param name="RightSide"></param>
        public void SplitVertically(dynamic aCol, out uopMatrix rLeftSide, out uopMatrix rRightSide)
        {
       
            rLeftSide = new uopMatrix(this, bDontCopyMembers: true);
            rRightSide = new uopMatrix(this, bDontCopyMembers: true);
            try
            {

                if (Cols <= 1) throw new Exception($"[{System.Reflection.MethodBase.GetCurrentMethod().Name}] a matrix with 1 or less column cannot be split");

                if (!mzUtils.IsNumeric(aCol)) throw new Exception($"[{System.Reflection.MethodBase.GetCurrentMethod().Name}] The indicated column index is not numeric");

                double cl = mzUtils.VarToDouble(aCol);
                int splt = (int)Math.Floor(cl);
                if (mzUtils.IsOdd(Cols) && cl > splt) splt += 1;


                if (splt <= 1 || splt >= Cols)
                {
                    throw new Exception($"[{System.Reflection.MethodBase.GetCurrentMethod().Name}] The indicated column index is outside the bounds of the current matrix");
                }
                rLeftSide = SubMatrix(1, Rows, 1, splt);
                rRightSide = SubMatrix(1, Rows, splt + 1, Cols);

            }
            catch (Exception e) { throw e; }



        }



        /// <summary>
        /// subtracts the passed matrix to this one
        /// #1the matrix to subtract from this one
        /// </summary>
        /// <param name="aMatrix"></param>
        public bool Subtract(uopMatrix aMatrix)
        {
            bool _rVal = false;
            try
            {
                if (aMatrix == null) throw new Exception($"[{System.Reflection.MethodBase.GetCurrentMethod().Name}] The passed matrix is undefined");

                for (int r = 1; r <= Rows; r++)
                {
                    uopMatrixRow row = Row(r);
                    for (int c = 1; c <= row.Cols; c++)
                    {
                        if(row.SetValue(c,  Math.Round(row.Value(c) - aMatrix.GetMember(r, c,true), Precision)) )
                            _rVal = true;
                        
                    }
                }
                if (_rVal && MonitorChanges) eventMatrixValueChange?.Invoke();
            }
            catch (Exception e) { throw( e);  }

            return _rVal;

        }
        /// <summary>
        /// replaces the first indicated column with the values of the second and vis versa.
        /// ~if Col1 = Col2 the request is ignored.
        /// ~if Col1 or Col2 = 0 the request is ignored.
        /// ~if the requested rows exceed the bounds of the current matrix an error is raised.
        /// #1the first column to swap
        /// #2the second column to swap
        /// </summary>
        /// <param name="Col1"></param>
        /// <param name="Col2"></param>
        public bool SwapColumn(int Col1, int Col2)
        {

            if (Col1 <1 ||  Col1 > Cols) throw new Exception($"[{System.Reflection.MethodBase.GetCurrentMethod().Name}] The first indicated column index is outside the bounds of the current matrix");

            if (Col2 < 1 || Col2 > Cols) throw new Exception($"[{System.Reflection.MethodBase.GetCurrentMethod().Name}] The second indicated column index is outside the bounds of the current matrix");
            if (Col1 == Col2) return false;
            bool _rVal = false;
            try
            {
                List<uopMatrixCell> c1 = Column(Col1);
                List<uopMatrixCell> c2 = Column(Col2);
                for (int r = 1; r <= c1.Count; r++)
                {
                    double v1 = c1[r-1].Value;
                    double v2 = c2[r - 1].Value;

                    if(v1 != v2) _rVal = true;
                    c1[r - 1].SetValue(v2);
                    c2[r - 1].SetValue(v1);

                }

            }
            catch (Exception e) { throw e; }
            finally
            {
                if (MonitorChanges && _rVal) eventMatrixValueChange?.Invoke();
            }
            return _rVal;
            
        }

        /// <summary>
        /// replaces the first indicated row with the values of the second and vis versa.
        /// ~if Row1 = Row2 the request is ignored.
        /// ~if Row1 or Row2 = 0 the request is ignored.
        /// ~if the requested rows exceed the bounds of the current matrix an error is raised.
        /// #1the first row to swap
        /// #2the second row to swap
        /// </summary>
        /// <param name="Row1"></param>
        /// <param name="Row2"></param>
        public bool SwapRow(int Row1, int Row2)
        {
            bool _rVal = false;

            try
            {

                if (Row1 > Rows || Row2 > Rows || Row1 <= 0 || Row2 <= 0) return false;
                if (Row1 == Row2) return false;
                uopMatrixRow r1 = Row(Row1);
                uopMatrixRow r2 = Row(Row2);
                for (int c = 1; c <= Cols; c++)
                {
                    double v1 = r1.Value(c);
                    double v2 = r2.Value(c);

                    if (v1 !=  v2) _rVal = true;
                    r1.SetValue(c, v2);
                    r2.SetValue(c, v1);
                }

            }
            catch (Exception e) { throw e; }
            finally
            {
                if (MonitorChanges && _rVal) eventMatrixValueChange?.Invoke();

            }
            return _rVal;
        }

        /// <summary>
        /// takes the total of the columns in the indicated range from the start row to the end row and
        /// ^put the values for each column in the row position indicated
        /// #1the row to put the total values in
        /// #2the row to start in
        /// #3the row to end in
        /// #4the column to start in
        /// #5the column to end in
        /// </summary>
        /// <param name="OutputRow"></param>
        /// <param name="StartRow"></param>
        /// <param name="EndRow"></param>
        /// <param name="StartCol"></param>
        /// <param name="EndCol"></param>
        public void TotalColumns(int OutputRow, int StartRow = 0, int EndRow = 0, int StartCol = 0, int EndCol = 0)
        {
            if (OutputRow <= 0 || OutputRow > Rows)
            {
                throw new Exception($"[{System.Reflection.MethodBase.GetCurrentMethod().Name}] The indicated row index is outside the bounds of the current matrix");
            }


            mzUtils.LoopLimits(StartRow, EndRow, 1,Rows, out int sr, out int er);
            mzUtils.LoopLimits(StartCol, EndCol, 1, Cols, out int sc, out int ec);

            bool _rVal = false;
            double tot = 0;
            for (int c = sc; c <= ec; c++)
            {
                tot = ColumnTotal(c, sr, er);
                if (SetValue(OutputRow, c, tot)) _rVal = true;
            }

            if (_rVal && MonitorChanges) eventMatrixValueChange?.Invoke();
        }


        /// <summary>
        /// takes the total of the rows in the indicated range from the start col to the end col and
        /// ^put the values for each row in the column position indicated
        /// #1the row to put the total values in
        /// #2the column to start in
        /// #3the column to end in
        /// #4the row to start in
        /// #5the row to end in
        /// </summary>
        /// <param name="OutputCol"></param>
        /// <param name="StartCol"></param>
        /// <param name="EndCol"></param>
        /// <param name="StartRow"></param>
        /// <param name="EndRow"></param>
        public bool TotalRows(int OutputCol = 0, int StartCol = 0, int EndCol = 0, int StartRow = 0, int EndRow = 0)
        {

            if (OutputCol <= 0 || OutputCol > Cols)
            {
                throw new Exception($"[{System.Reflection.MethodBase.GetCurrentMethod().Name}] The indicated column index is outside the bounds of the current matrix");
            }
            mzUtils.LoopLimits(StartRow, EndRow, 1, Rows, out int sr, out int er);
            mzUtils.LoopLimits(StartCol, EndCol, 1, Cols, out int sc, out int ec);
            bool _rVal = false;
            double tot = 0;
            for (int r = sr; r <= er; r++)
            {
                tot = RowTotal(r, sc, ec);
                if (SetValue(r, OutputCol, tot)) _rVal = true;
            }

            if (_rVal && MonitorChanges) eventMatrixValueChange?.Invoke();

            if (_rVal && MonitorChanges) eventMatrixValueChange?.Invoke();
            return _rVal;
        }

        /// <summary>
        /// moves all zero rows (from the start row and below) to the bottom of the matrix and returns the count of the zero rows
        /// #1the row to begin at
        /// #2returns the new order of the matrix
        /// </summary>
        /// <param name="StartRow"></param>
        /// <returns></returns>
        public int ZeroRowsToBottom(int StartRow = 1)
        {
            //int _rVal = ZeroRowsToBottom(StartRow);
            mzUtils.LoopLimits(StartRow, Rows, 1, Rows, out int sr, out int er);
       
            TVALUES zrows = new TVALUES("");
            zrows.Fill(0, Rows);

            int zcnt = 0;
            for (int r = sr; r <= er; r++)
            {

                LeadingRowEntry(r, 1, out double lval);
                if (lval != 0)
                { zrows.SetItem(r, 1); }
                else
                { zcnt += 1; }
            }

            int _rVal = zcnt;
            if (zcnt == 0 || zcnt == Rows || Rows <= 1) return _rVal;



            for (int n = Rows; n >= StartRow; n--)
            {
                if (zrows.Item(n) != 0)
                {
                    for (int nn = n - 1; nn >= StartRow; nn--)
                    {
                        if (zrows.Item(nn) == 0)
                        {
                            SwapRow(n, nn);
                            zrows.SetValue(n, 0);
                            zrows.SetValue(nn, 1);
                            break;
                        }
                    }
                }
                if (n <= StartRow) break;

            }
         

            if (MonitorChanges && _rVal != 0) eventMatrixValueChange?.Invoke();

            return _rVal;
        }

        /// <summary>
        /// returns the requested cells
        /// </summary>
        /// <param name="bGetClones">flag to return clones of the member cells</param>
        /// <param name="aStartRow">the starting row index to scan. <=0 defaults to 1</param>
        /// <param name="aEndRow">the ending row index to scan. <=0 defaults to the current count</param>
        /// <param name="aStartCol">the starting column index to scan. <=0 defaults to 1</param>
        /// <param name="aEndCol">the ending column index to scan. <=0 defaults to the current column count</param>
        /// <param name="bInvisVal">an optional invisible value to assign to the returned cells</param>
        /// <returns></returns>
        public List<uopMatrixCell> Cells(bool bGetClones = false, int aStartRow =0, int aEndRow=0, int aStartCol=0, int aEndCol =0,  bool? bInvisVal = null)
        {
            List<uopMatrixCell> _rVal = new List<uopMatrixCell>();
            if (Size <= 0) return _rVal;
            mzUtils.LoopLimits(aStartRow, aEndRow, 1, Rows, out int sr, out int er);
            mzUtils.LoopLimits(aStartCol, aEndCol, 1, Cols, out int sc, out int ec);
            for(int r = sr; r <=er; r++)
            {
                for (int c = sc; c <= ec; c++) 
                {
                    uopMatrixCell cell = Cell(r, c, bGetClones);
                    if(cell != null)
                    {
                        if (bInvisVal.HasValue) cell.Invisible = bInvisVal.Value;
                        _rVal.Add(cell);
                    }
                }
            }

      
            return _rVal;
        }

        public uopMatrix GetGroupIndices(int aStartRow = 0, int aEndRow = 0, int aStartCol = 0, int aEndCol = 0)
        {
            uopMatrix _rVal = new uopMatrix(this,true);
            if (Size <= 0) return _rVal;
            mzUtils.LoopLimits(aStartRow, aEndRow, 1, Rows, out int sr, out int er);
            mzUtils.LoopLimits(aStartCol, aEndCol, 1, Cols, out int sc, out int ec);
            int rows = er - sr + 1;
            int cols = ec - sc + 1;

            _rVal.Resize(rows, cols);
            int irow = 0;
            
            for (int r = sr; r <= er; r++)
            {
                uopMatrixRow row = Row(r);
                if (row == null) continue;
                irow++;
                uopMatrixRow returnrow = _rVal.Row(r);

                int icol = 0;
                for (int c = sc; c <= ec; c++)
                {
                    uopMatrixCell cell = row.Cell(c);
                    if (cell != null)
                    {
                        icol++;
                        returnrow.SetValue(icol, cell.GroupIndex);
                    }
                        
                }
                
            }

            return _rVal;

        }

        public uopMatrix GetOverrideValues(double? aDefault = null, int aStartRow = 0, int aEndRow = 0, int aStartCol = 0, int aEndCol = 0)
        {
            uopMatrix _rVal = new uopMatrix(this, true);
            if (Size <= 0) return _rVal;
            mzUtils.LoopLimits(aStartRow, aEndRow, 1, Rows, out int sr, out int er);
            mzUtils.LoopLimits(aStartCol, aEndCol, 1, Cols, out int sc, out int ec);
            int rows = er - sr + 1;
            int cols = ec - sc + 1;

            _rVal.Resize(rows, cols);
            int irow = 0;

            for (int r = sr; r <= er; r++)
            {
                uopMatrixRow row = Row(r);
                if (row == null) continue;
                irow++;
                uopMatrixRow returnrow = _rVal.Row(r);

                int icol = 0;
                for (int c = sc; c <= ec; c++)
                {
                    uopMatrixCell cell = row.Cell(c);
                    if (cell != null)
                    {
                        icol++;
                        double dval =  cell.OverrideValue.HasValue ? cell.OverrideValue.Value : aDefault.HasValue ? aDefault.Value : 0;
                        returnrow.SetValue(icol, dval);
                    }

                }

            }

            return _rVal;

        }

        /// <summary>
        /// returns the values of the requested cells
        /// </summary>
        /// <param name="aStartRow">the starting row index to scan. <=0 defaults to 1</param>
        /// <param name="aEndRow">the ending row index to scan. <=0 defaults to the current count</param>
        /// <param name="aStartCol">the starting column index to scan. <=0 defaults to 1</param>
        /// <param name="aEndCol">the ending column index to scan. <=0 defaults to the current column count</param>
        /// <returns></returns>
        public List<List<double>> Values( int aStartRow = 0, int aEndRow = 0, int aStartCol = 0, int aEndCol = 0)
        {
            List<List<double>> _rVal = new List<List<double>>();
            if (Size <= 0) return _rVal;
            mzUtils.LoopLimits(aStartRow, aEndRow, 1, Rows, out int sr, out int er);
            mzUtils.LoopLimits(aStartCol, aEndCol, 1, Cols, out int sc, out int ec);
            for (int r = sr; r <= er; r++)
            {
                uopMatrixRow row = Row(r);
                if (row == null) continue;


                List<double> rowvals = new List<double>();
                for (int c = sc; c <= ec; c++)
                {
                    uopMatrixCell cell = row.Cell(c);
                    if (cell != null)
                        rowvals.Add(cell.Value);
                }
                _rVal.Add(rowvals);
            }


            return _rVal;
        }

        public uopMatrixCell FindCell(Predicate<uopMatrixCell> match, bool bGetClones = false, int aStartRow = 0, int aEndRow = 0, int aStartCol = 0, int aEndCol = 0) => Cells(bGetClones,aStartRow,aEndRow,aStartCol,aEndCol).Find(match);

        public List<uopMatrixCell> FindCells(Predicate<uopMatrixCell> match, bool bGetClones = false, int aStartRow = 0, int aEndRow = 0, int aStartCol = 0, int aEndCol = 0) => Cells(bGetClones, aStartRow, aEndRow, aStartCol, aEndCol).FindAll(match);

        public List<uopMatrixCell> SetCellValues(Predicate<uopMatrixCell> match, double aValue, int aStartRow = 0, int aEndRow = 0, int aStartCol = 0, int aEndCol = 0)
        {
            List<uopMatrixCell> _rVal = Cells(false, aStartRow, aEndRow, aStartCol, aEndCol).FindAll(match);
            foreach (var item in _rVal) item.SetValue(aValue);
            return _rVal;
        }
        public List<uopMatrixCell> SetCellOverrideValues(Predicate<uopMatrixCell> match, double? aValue, int aStartRow = 0, int aEndRow = 0, int aStartCol = 0, int aEndCol = 0)
        {
            List<uopMatrixCell> _rVal = Cells(false, aStartRow, aEndRow, aStartCol, aEndCol).FindAll(match);
            foreach (var item in _rVal) item.OverrideValue = aValue;
            return _rVal;
        }
        public List<uopMatrixCell> SetCellGroupIndices(Predicate<uopMatrixCell> match, int  aGroupIndex, int aStartRow = 0, int aEndRow = 0, int aStartCol = 0, int aEndCol = 0)
        {
            List<uopMatrixCell> _rVal = Cells(false, aStartRow, aEndRow, aStartCol, aEndCol).FindAll(match);
            foreach (var item in _rVal) item.GroupIndex = aGroupIndex;
            return _rVal;
        }
        public List<uopMatrixCell> RoundCellValues(int aPrecis, int aStartRow = 0, int aEndRow = 0, int aStartCol = 0, int aEndCol = 0)
        {
            List<uopMatrixCell> _rVal = Cells(false, aStartRow, aEndRow, aStartCol, aEndCol);
            int precis = mzUtils.LimitedValue(aPrecis, 0, 15); 
            foreach (var item in _rVal) item.SetValue(Math.Round( item.Value,precis));
            return _rVal;
        }

        public List<uopMatrixCell> GetMatchingCells(List<uopMatrixCell> aCellsToMatch, bool bGeClones = true)
        {
            List<uopMatrixCell> _rVal = new List<uopMatrixCell>();

            if (aCellsToMatch == null) return _rVal;

            foreach (uopMatrixCell cell in aCellsToMatch)
            {
                uopMatrixCell cellr = Cell(cell.Row, cell.Col, bGeClones);
                if (cellr != null) _rVal.Add(cellr);
            }

            return _rVal;
        }

        public void SetOverrideValues(double? aValue, int aStartRow = 0, int aEndRow = 0, int aStartCol = 0, int aEndCol = 0)
        {
            List<uopMatrixCell> cells = Cells(false, aStartRow, aEndRow, aStartCol, aEndCol);
            foreach (var cell in cells)
            {
                cell.OverrideValue = aValue;
            }
        }

        public void SetOverrideValue(int aRow, int aCol, double? aValue, uopInstances aInstances = null)
        {
            List<uopMatrixCell> cells = Cells(aRow, aCol, aInstances);
            foreach (var cell in cells)
            {
                cell.OverrideValue = aValue;

            }
        
            
        }


        public void SetGroupIndices(int aGroupIndex, int aStartRow = 0, int aEndRow = 0, int aStartCol = 0, int aEndCol = 0)
        {
            List<uopMatrixCell> cells = Cells(false, aStartRow, aEndRow, aStartCol, aEndCol);
            foreach (var cell in cells)
            {
                cell.GroupIndex = aGroupIndex;
            }
        }
        public void SetGroupIndex(int aRow, int aCol, int aGroupIndex)
        {
            uopMatrixCell cell = Cell(aRow, aCol);
            if (cell != null) cell.GroupIndex = aGroupIndex;

        }


        public List<uopMatrixCell> AverageGroupCells(int aGroupIndex, int aStartRow = 0, int aEndRow = 0, int aStartCol = 0, int aEndCol = 0)
        {
            List<uopMatrixCell> _rVal = this.FindCells((x) => x.GroupIndex == aGroupIndex  && !x.Invisible, false,aStartRow,aEndRow,aStartCol,aEndCol);
            if (_rVal.Count <= 1) return _rVal;
            double avg = uopMatrixCell.CellAverage(_rVal);
            uopMatrixCell.SetCellValues(_rVal,avg);
            return _rVal;
        } 
        private int xxGetLeads(int tp, int bt, out List<int> leads)
        {
            int _rVal = Cols;
            leads = new List<int>();

            int c;
            for (int r = tp; r <= bt; r++)
            {
                c = LeadingRowEntry(r, 1, out double leadval);
                leads.Add(c);
                if (c <= _rVal) _rVal = c;
            }
            return _rVal;
        }

        private bool xxCheckForDone(int tp, int bt, List<int> leads)
        {
            for (int r = bt; r <= tp + 1; r--)
            {
                if (leads[r - 2] < leads[r - 1]) return false;
            }
            return true;
        }

        
        #endregion Methods

        #region Shared Methods

        /// <summary>
        /// returns an new matrix with the same properties and values as the cloned matrix
        /// </summary>
        /// <returns></returns>
        public static uopMatrix Copy(uopMatrix aMatrix) { return aMatrix == null ? null : new uopMatrix(aMatrix); }

        public static uopMatrix CloneCopy(uopMatrix aMatrix) => aMatrix == null ? null : new uopMatrix(aMatrix, false);
        public static int Maxsize = 1000;

        internal static bool ResizeMatrix(ref List<uopMatrixRow> ioRowList, int? aRows = null, int? aCols = null, double? aInitVal = null, bool bInvisibleVal = false, int? aPrecis = null) 
        {
            if (ioRowList == null) return false;
            if(!aRows.HasValue && !aCols.HasValue) return false;
            bool rowchange = false;
            bool colchange = false;
            int  irows = ioRowList.Count;
            int icols = ioRowList.Count > 0 ? ioRowList[0].Cols : 0;
            if(aCols.HasValue && aCols.Value <=0) aCols = null;
            if (aCols.HasValue) icols = mzUtils.LimitedValue(aCols.Value, 0, uopMatrix.Maxsize, 1);
            
            if (aRows.HasValue && aRows.Value >= 0)
            {
                if (aRows.Value == 0)
                {
                    ioRowList.Clear();
                    return irows != 0;
                }
                if(aRows.Value != irows) rowchange = true  ;

                if (rowchange) 
                { 
                    if(aRows.Value < irows)
                    {
                         ioRowList.RemoveRange(aRows.Value - 1, irows - aRows.Value);
                    }
                    else
                    {
                       
                        for(int r = irows +1; r<= aRows.Value; r++)
                        {
                            uopMatrixRow row = new uopMatrixRow(r, icols, aInitVal, bInvisibleVal,  aPrecis );
                            ioRowList.Add(row);
                        }

                    }
                    
                }
                
            }
            for(int r = 1; r<= ioRowList.Count; r++) 
            {
                uopMatrixRow row = ioRowList[r - 1];
                row.Row = r;
                int cols = aCols.HasValue ? aCols.Value : icols;
                if (row.Resize(cols, aInitVal)) colchange = true;

            }


            return  rowchange || colchange;
        }

        /// <summary>
        /// returns True if the passed matrix is elementwise equal to the current matrix
        /// </summary>
        /// <param name="aMatrix">the matrix to compare</param>
        /// <param name="bMatrix">the matrix to compare</param>
        /// <returns></returns>
        public static bool CompareTo(uopMatrix aMatrix, uopMatrix bMatrix, int? aPrec = null, bool bCompareGroupIDs = false, bool bCompareOverrides = false)
        {
            if (aMatrix == null && bMatrix != null) return false;
            if (aMatrix != null && bMatrix == null) return false;

            if (string.Compare(aMatrix.Dimensions, bMatrix.Dimensions) != 0) return false;

            int prec = !aPrec.HasValue ? aMatrix.Precision : mzUtils.LimitedValue(aPrec.Value, 0, 15);
            bool _rVal = false;
            for (int r = 1; r <= aMatrix.Rows; r++)
            {

                for (int c = 1; c <= aMatrix.Cols; c++)
                {
                    uopMatrixCell mycell = aMatrix.Cell(r, c);
                    uopMatrixCell hercell = bMatrix.Cell(r, c);

                    if (Math.Round(mycell.Value - hercell.Value, prec) != 0)
                    {
                        _rVal = true;

                    }

                    if (bCompareGroupIDs && mycell.GroupIndex != hercell.GroupIndex) _rVal = true;
                    if (bCompareOverrides && mycell.OverrideValue != hercell.OverrideValue) _rVal = true;

                    if (!bCompareGroupIDs && !bCompareOverrides && _rVal) break;


                }
            }
            return _rVal;
        }

        #endregion Shared Methods
    }

 

 
}
