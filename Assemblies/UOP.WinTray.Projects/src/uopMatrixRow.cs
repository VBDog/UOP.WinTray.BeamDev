using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UOP.WinTray.Projects.Tables;
using UOP.WinTray.Projects.Utilities;

namespace UOP.WinTray.Projects
{
    public class uopMatrixRow : List<uopMatrixCell>, IEnumerable<uopMatrixCell> , ICloneable
    {
        #region Constructors
        internal uopMatrixRow(int aRow, int aCols, double? aInitVal = null, bool bInvisibleVal = false, int? aPrecis = null) 
        { 
            _Name = string.Empty;
         base.Clear();
            Row = Math.Abs(aRow);
            Precision= aPrecis.HasValue ? mzUtils.LimitedValue( aPrecis.Value,0,15,10) : 10;
            for (int c = 1; c <= aCols; c++) base.Add(new uopMatrixCell(Row, c, aInitVal.HasValue ? aInitVal.Value : 0 ,  bInvisible: bInvisibleVal, aPrecision:Precision));
        }
        internal uopMatrixRow(IEnumerable<uopMatrixCell>  aRow)
        {
            _Name = string.Empty;
            base.Clear();
            Row = 0;
            _Precision = 10;
            if (aRow == null) return;
           
            if (aRow.GetType() == typeof(uopMatrixCell)) 
            { 
                uopMatrixRow urow = (uopMatrixRow)aRow;
                Name = urow._Name;
                Row = urow.Row;
                Precision = urow.Precision;
            }
            List<uopMatrixCell> cells = aRow.ToList();
            for (int c = 1; c <= cells.Count; c++) base.Add(new uopMatrixCell(cells[c - 1]) { Col = c, Precision = Precision });
        
        }

        #endregion  Constructors

        #region Properties
        private int _Precision;
        public int Precision
        {
            get => _Precision;
            set => _Precision = mzUtils.LimitedValue(value, 0, 15, _Precision);
        }

        private string _Name;
        public string Name { get=> string.IsNullOrWhiteSpace(_Name) ?  $"Row {Row}": _Name ; set { _Name = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();  } }

        private int _Row;
        public int Row { get => _Row;
            internal set
            {
                if(_Row != value)
                {
                    _Row = value;
                    foreach (uopMatrixCell cell in this) cell.Row = value;

                }
            } 
        }

        public int Cols { get => Count;  internal set => Resize(value); }

        public string Descriptor
        {
            get
            {
                string _rVal = string.Empty;

                for (int c = 1; c <= Cols; c++)
                {
                    if (c > 1) _rVal += ",";
                    _rVal += Value(c);
                }
                return _rVal;
            }
        }

        #endregion Properties

                #region Methods

        public override string ToString()
        {
            return Name;
        }


        public double Total(int StartCol = 0, int EndCol = 0)
        {
            double _rVal = 0;
            try
            {


                mzUtils.LoopLimits(StartCol, EndCol, 1, Cols, out int sc, out int ec);
                for (int c = sc; c <= ec; c++) { _rVal += Value(c); }

                return _rVal;

            }
            catch (Exception e) { return _rVal; throw e; }
        }
        public bool SetAllValues(double aValue)
        {
            bool _rVal = false;
            for (int c = 1; c <= Count; c++) if (SetValue(c, aValue)) _rVal = true;
            return _rVal;
        }


        /// <summary>
        /// populars the values in a row with the pasted list of doubles
        /// </summary>
        /// <param name="aValues">a list of double values</param>
        public bool Populate(List<uopMatrixCell> aValues, int? aPrecis = null, bool? bInvis = null)
        {
            if (aValues == null ) return false;
            if (aValues.Count <= 0) return false;

            int prec = aPrecis.HasValue ? mzUtils.LimitedValue(aPrecis.Value, 1, 15) : -1; 
            bool _rVal = false;
            double dVal;
            for (int c = 1; c <= Cols; c++)
            {

                if (c > aValues.Count) break;
                dVal = prec >=0 ? Math.Round(aValues[c - 1].Value, prec) : aValues[c - 1].Value;
                if (SetValue(c,dVal, bInvis)) _rVal = true;
            }
            return _rVal;
        }
        /// <summary>
        /// populars the values in a row with the pasted list of doubles
        /// </summary>
        /// <param name="aValues">a list of double values</param>
        public bool Populate(List<double> aValues, int? aPrecis = null, bool? bInvis = null)
        {
            if (aValues == null) return false;
            if (aValues.Count <= 0) return false;

            int prec = aPrecis.HasValue ? mzUtils.LimitedValue(aPrecis.Value, 1, 15) : -1;
            bool _rVal = false;
            double dVal;
            for (int c = 1; c <= Cols; c++)
            {

                if (c > aValues.Count) break;
                dVal = prec >= 0 ? Math.Round(aValues[c - 1], prec) : aValues[c - 1];
                if (SetValue(c, dVal, bInvis)) _rVal = true;
            }
            return _rVal;
        }


        internal bool Resize(int aCols, double? aInitVal = null)
        {
            bool _rVal = false;
            try
            {
                if (aCols < 0) throw new ArgumentOutOfRangeException($"{System.Reflection.MethodBase.GetCurrentMethod().Name} - The passed value must be greater than equal to 0");
                if (aCols > uopMatrix.Maxsize) throw new ArgumentOutOfRangeException($"{System.Reflection.MethodBase.GetCurrentMethod().Name} - The passed value must be less than or equal to{uopMatrix.Maxsize}");
                if (aCols == 0)
                {
                    _rVal = Cols > 0;
                    Clear();
                }
                _rVal = aCols != Count;
                if (aCols < Count)
                   
                    base.RemoveAll((x) => IndexOf(x) + 1 > aCols);
                else
                {
                    int count = Count;
                    for (int c = 1; c <= aCols; c++) { if (c >= count + 1) { base.Add(new uopMatrixCell(Row, c, aInitVal.HasValue ? aInitVal.Value : 0, aPrecision:Precision)); } else { base[c - 1].Row = _Row; base[c - 1].Col = c; } };
                }

            }
            catch
            {
                _rVal = false;

            }
            return _rVal;        

        }

        public new void Add(uopMatrixCell aCell)
        {
            if(aCell == null) throw new ArgumentNullException($"{System.Reflection.MethodBase.GetCurrentMethod().Name} - The passed cell is null.");
            aCell.Col = Count +1;
            aCell.Precision = Precision;
            base.Add(aCell);

        }

        public bool SetValue(int aCol, double aValue, bool? bInvis = null)
        {
            if (aCol < 0 || aCol > Count) throw new ArgumentOutOfRangeException($"{System.Reflection.MethodBase.GetCurrentMethod().Name}.Add");
            return Cell(aCol).SetValue(aValue, bInvis);

        }
       
        object ICloneable.Clone() => (object)this.Clone();

        public uopMatrixRow Clone() => new uopMatrixRow(this);

        public double Value(int aCol) 
        {

            if(Count == 0) return 0;
            if (aCol < 1) throw new ArgumentOutOfRangeException($"{System.Reflection.MethodBase.GetCurrentMethod().Name} - The passed value must be greater than or equal to 1");
            if (aCol > Count) throw new ArgumentOutOfRangeException($"{System.Reflection.MethodBase.GetCurrentMethod().Name} - The passed value must be less than or equal to {Count}");

            return Cell(aCol).Value;
          
        }

        public double LastValue(int aCol)
        {

            if (Count == 0) return 0;
            if (aCol < 1) throw new ArgumentOutOfRangeException($"{System.Reflection.MethodBase.GetCurrentMethod().Name} - The passed value must be greater than or equal to 1");
            if (aCol > Count) throw new ArgumentOutOfRangeException($"{System.Reflection.MethodBase.GetCurrentMethod().Name} - The passed value must be less than or equal to {Count}");

            return Cell(aCol).LastValue;

        }

        /// <summary>
        /// used to remove a col column from matrix
        /// ~values exceeding the limits are ignored. the call is ignored if the matrix has only 1 column
        /// #1the column to remove from the matrix
        /// </summary>
        /// <param name="ColToRemove"></param>
        public uopMatrixCell RemoveCell(int ColToRemove)
        {
            uopMatrixCell _rVal = null;
            List<uopMatrixCell> cells = new List<uopMatrixCell>();
            cells.AddRange(this);
            base.Clear();
            int i = 1;
            for(int c  = 1; c <= cells.Count; c++)
            {
                if(c == ColToRemove) 
                { 
                    _rVal = cells[c -1];
                }
                else 
                {
                    cells[c - 1].Col = i;
                base.Add(cells[c - 1]);
                    i++;
                }
            }


            

            return _rVal;
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
            List<uopMatrixCell> _rVal = new List<uopMatrixCell>();
            StartCol = mzUtils.LimitedValue(StartCol, 1, Cols);

            List<uopMatrixCell> keepers = new List<uopMatrixCell>();

            int i  = 1;
            for (int c = 1; c <= Count; c++)
            {
                uopMatrixCell cell = this[c - 1];
                cell.Col = c;
                if (c < StartCol || c > StartCol + Cols)
                {
                    cell.Col = i;
                    i++;
                    keepers.Add(cell);
                }
                else
                {
                    _rVal.Add(cell);
                }
            }

            base.Clear();
            if (keepers.Count > 0) base.AddRange(keepers);
            return _rVal;
        }

        public new void Reverse() 
        { 
            base.Reverse();
            for(int c = 1; c <= Cols; c++) this[c-1].Col = c;
        }


        public uopMatrixCell Cell(int aCol, bool bGetClone = false)
        {
            if (Count == 0) return null;
            if (aCol < 1) throw new ArgumentOutOfRangeException($"{System.Reflection.MethodBase.GetCurrentMethod().Name} - The passed value must be greater than or equal to 1");
            if (aCol > Count) throw new ArgumentOutOfRangeException($"{System.Reflection.MethodBase.GetCurrentMethod().Name} - The passed value must be less than or equal to {Count}");
            uopMatrixCell _rVal = this[aCol - 1];
            _rVal.Row = Row;
            _rVal.Col = aCol;
            _rVal.Precision = Precision;
            return !bGetClone ?_rVal : new uopMatrixCell(_rVal);
        }

        /// <summary>
        /// returns the requested cells
        /// </summary>
        /// <param name="bGetClones">flag to return clones of the member cells</param>
        /// <param name="aStartCol">the starting column index to scan. <=0 defaults to 1</param>
        /// <param name="aEndCol">the ending column index to scan. <=0 defaults to the current column count</param>
        /// <param name="bInvisVal">an optional invisible value to assign to the returned cells</param>
        /// <returns></returns>
        public List<uopMatrixCell> Cells(bool bGetClones = false, int aStartCol = 0, int aEndCol = 0, bool? bInvisVal = null)
        {
            List<uopMatrixCell> _rVal = new List<uopMatrixCell>();
            if(Count<=0) return _rVal;
            mzUtils.LoopLimits(aStartCol, aEndCol, 1, Cols, out int cs, out int ce);
            for(int c= cs; c<= ce; c++)
            {
                uopMatrixCell cell = this[c - 1];
                cell.Row = Row;
                cell.Col = c;
                cell.Precision = Precision;
                if (bGetClones) cell = new uopMatrixCell(cell);
                if (bInvisVal.HasValue) cell.Invisible = bInvisVal.Value;
                _rVal.Add(cell);
            }

            return _rVal;
        }
            #endregion Methods
        }
}
