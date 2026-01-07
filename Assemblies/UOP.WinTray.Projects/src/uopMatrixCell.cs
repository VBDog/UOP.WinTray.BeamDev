using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UOP.WinTray.Projects.Utilities;

namespace UOP.WinTray.Projects
{
    public class uopMatrixCell : ICloneable
    {
        #region Constructors
        public uopMatrixCell() => Init();

        public uopMatrixCell(int aRow, int aCol, double aVal)
        {
            Init();
            Row = aRow;
            Col = aCol;
            _Value = aVal;
            _LastValue = aVal;
           
        }
        public uopMatrixCell(uopMatrixCell aCell) 
        {
                Row = 0; Col = 0; _Value = 0; _LastValue = 0; Invisible = false; OverrideValue = null; GroupIndex = 0; Color = null; _Precision =10;
            if (aCell != null)  Copy(aCell);
        }


        public uopMatrixCell(int aRow, int aCol, double aVal, double? aLastVal = null, bool bInvisible = false, int? aPrecision = null)
        {
            Init();
            Row = aRow;
            Col = aCol;
            Value = aVal;
            LastValue = aLastVal.HasValue ? aLastVal.Value : aVal;
            Invisible = bInvisible;
            _Precision =  aPrecision.HasValue ? mzUtils.LimitedValue(aPrecision.Value,0,15,10) : 10;
        }

        private void Init()
        {
            Row = 0; Col = 0; _Value = 0; _LastValue = 0; OverrideValue = null; GroupIndex = 0; Color = null; IsVirtual = false;
        }

        #endregion Constructors

        #region Properties

        private double _LastValue;
        public double LastValue { get =>_LastValue; internal set => _LastValue = value; }

        private double _Value;
        public double Value { get => _Value; internal set { _LastValue = _Value; _Value = value; } } 
        public int Row { get; internal set; } 
        public int Col { get; internal set; }

        public string Handle { get => $"{Row},{Col}"; }
        public bool Invisible { get;  set; }
        public bool IsVirtual { get; set; }

        public double? OverrideValue { get;  set; }

        public int GroupIndex { get; set; }
        public System.Drawing.Color?  Color{ get; set; }

        private int _Precision;
        public int Precision
        {
            get => _Precision;
            set => _Precision = mzUtils.LimitedValue(value,0,15,_Precision);
        }

        #endregion Properties


        #region Methods

        public bool SetValue(double aValue, bool? bInvisible = null)
        {
            if (bInvisible.HasValue) Invisible = bInvisible.Value;
            if (double.IsNaN(aValue))
            {
                
                Console.WriteLine($"Cell[{Row},{Col} Set to NaN");
                return false;
            }
            bool _rVal = aValue != _Value;

            Value = aValue;
           
            return _rVal;
        }

        public override string ToString()
        {
            string fmat = Precision == 0 ? "#,0" : $"#,0.{new string('0', Precision)}"; 
            return $"[{Row},{Col}]={Value.ToString(fmat)}";
        }

        public uopMatrixCell Clone() => new uopMatrixCell(this);

        public void Copy(uopMatrixCell aCell)
        {
            if (aCell == null) return;
            Row = aCell.Row; Col = aCell.Col; _Value = aCell.Value; _LastValue = aCell.LastValue; Invisible = aCell.Invisible; OverrideValue = aCell.OverrideValue; GroupIndex = aCell.GroupIndex; Color = aCell.Color; _Precision = aCell.Precision; IsVirtual = aCell.IsVirtual;
        }

        object ICloneable.Clone() => (object)this.Clone();

        #endregion Methods

        #region Shared Methods

        public static double CellTotal(List<uopMatrixCell> aCells)
        {
            if (aCells == null) return 0;
            double _rVal = 0;
            foreach (uopMatrixCell item in aCells)
                _rVal += item.Value;
            return _rVal;
        }

        public static List<double> UniqueValues(List<uopMatrixCell> aCells, int aPrecis = 6)
        {
            List<double> _rVal = new List<double>();
            if (aCells == null) return  _rVal;
            int precis = mzUtils.LimitedValue(aPrecis, 0, 15);
            foreach (uopMatrixCell item in aCells)
            {
                double dval = Math.Round(item.Value, precis);
                if(_rVal.IndexOf(dval) ==-1)  _rVal.Add(dval);
            }
            return _rVal;
        }

        public static double CellAverage(List<uopMatrixCell> aCells)
        {
            if (aCells == null || aCells.Count <0) return 0;
            return CellTotal(aCells) / (double)aCells.Count;
        }

        public static List<uopMatrixCell> SetCellValues(List<uopMatrixCell> aCells, double aValue)
        {
            List < uopMatrixCell > _rVal = new List<uopMatrixCell >();
            if (aCells == null || aCells.Count < 0) return _rVal;
            foreach(uopMatrixCell cell  in aCells)
            {
                if (cell.SetValue(aValue)) _rVal.Add(cell);
            }
            return _rVal;
        }

        public static List<uopMatrixCell> SetMatchingCellValues(List<uopMatrixCell> aCells, List<uopMatrixCell> bCells, double aValue)
        {
            List<uopMatrixCell> _rVal = new List<uopMatrixCell>();
            if (aCells == null || aCells.Count < 0) return _rVal;
            if (bCells == null || bCells.Count < 0) return _rVal;
            foreach (uopMatrixCell matchcell in bCells)
            {
               List< uopMatrixCell> basecells = aCells.FindAll((x) => x.Row == matchcell.Row && x.Col == matchcell.Col); 
                foreach(var basecell in basecells)
                {
                    if (basecell.SetValue(aValue)) _rVal.Add(basecell);
                }
                
            }
            return _rVal;
        }

        public static List<uopMatrixCell> SetMatchingCellValues(uopMatrix aMatrix, List<uopMatrixCell> bCells, double aValue)
        => aMatrix == null || bCells == null ? new List<uopMatrixCell>() : uopMatrixCell.SetMatchingCellValues(aMatrix.Cells(), bCells, aValue);
           
        public static List<uopMatrixCell> SetCellData(List<uopMatrixCell> aCells, double? aOverrideValue, System.Drawing.Color? aColor, int? aGroupIndex = null)
        {
            List<uopMatrixCell> _rVal = new List<uopMatrixCell>();
            if (aCells == null || aCells.Count < 0) return _rVal;
            foreach (uopMatrixCell cell in aCells)
            {
                bool keep = cell.OverrideValue != aOverrideValue;
                if (cell.Color != aColor) keep = true;

                cell.OverrideValue = aOverrideValue;
                cell.Color = aColor;
                if (aGroupIndex != null) cell.GroupIndex = aGroupIndex.Value;
                if (keep) _rVal.Add(cell);
            }
            return _rVal;
        }

        public static List<uopMatrixCell> Sort(List<uopMatrixCell> aCells, bool bHighToLow = false)
        {
            if (aCells == null) return aCells;

            List<uopMatrixCell> _rVal = new List<uopMatrixCell>();
            List<Tuple<double, uopMatrixCell>> sortvals = new List<Tuple<double, uopMatrixCell>>();
            foreach (uopMatrixCell item in aCells)
            {
                sortvals.Add(new Tuple<double, uopMatrixCell>(item.Value, item));
            }
            sortvals = sortvals.OrderBy(t => t.Item1).ToList();
            if (bHighToLow) sortvals.Reverse();
            foreach (Tuple<double, uopMatrixCell> item in sortvals)
            {
                _rVal.Add(item.Item2);
            }
            return _rVal;
        }

        #endregion Shared Methods
    }
}
