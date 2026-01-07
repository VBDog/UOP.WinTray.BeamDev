using System;
using System.Collections.Generic;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;

namespace UOP.WinTray.Projects.Tables
{
    public class uopTableCell : uopProperty, ICloneable
    {
        #region Private Variables

      
        private int[,] _BorderData;

        #endregion Private Variables

        #region Constructors

        internal uopTableCell()
        {
            Init();
        }
        internal uopTableCell(int aRow, int aCol, uopProperty aProperty = null, bool bLocked = false)
        {
            Init();
            if (aProperty != null) Copy(aProperty);
            Row = aRow;
            Col = aCol;
          
            Locked = bLocked;
        }

        internal uopTableCell(uopTableCell aCellToCopy)
        {
            Init();

            if (aCellToCopy == null) return;
            Width = aCellToCopy.Width;
            Height = aCellToCopy.Height;
            TextFormat = aCellToCopy.TextFormat;
            NumberFormat = aCellToCopy.NumberFormat;
            StartColumn = aCellToCopy.StartColumn;
            StartRow = aCellToCopy.StartRow;
            Alignment = aCellToCopy.Alignment;
            WrapText = aCellToCopy.WrapText;
            FontName = aCellToCopy.FontName;
            FontSize = aCellToCopy.FontSize;
            Locked = aCellToCopy.Locked;
            _Address = aCellToCopy._Address;
            Orientation = aCellToCopy.Orientation;
            HTMLFontSize = aCellToCopy.HTMLFontSize;
            FontColor = aCellToCopy.FontColor;
            BackColor = aCellToCopy.BackColor;
            DataOnly = aCellToCopy.DataOnly;
            _Locked = aCellToCopy._Locked;
            BoldText = aCellToCopy.BoldText;

            Copy(aCellToCopy);
        }

        private void Init()
        {
            //Row = 0;
            //Column = 0;
            _BorderData = new int[4, 2];
        
            TextFormat = mzTextFormats.None;
            FontSize = 10;
            FontColor = System.Drawing.Color.Transparent;
            BackColor = System.Drawing.Color.Transparent;
            _BorderData[0, 0] = Convert.ToInt32(mzBorderStyles.Undefined);
            _BorderData[0, 1] = Convert.ToInt32(mzBorderWeights.Thin);
            _BorderData[1, 0] = Convert.ToInt32(mzBorderStyles.Undefined);
            _BorderData[1, 1] = Convert.ToInt32(mzBorderWeights.Thin);
            _BorderData[2, 0] = Convert.ToInt32(mzBorderStyles.Undefined);
            _BorderData[2, 1] = Convert.ToInt32(mzBorderWeights.Thin);
            _BorderData[3, 0] = Convert.ToInt32(mzBorderStyles.Undefined);
            _BorderData[3, 1] = Convert.ToInt32(mzBorderWeights.Thin);
        }
        #endregion Constructors

        #region Public Properties


        public bool IsAdressed => !string.IsNullOrWhiteSpace(_Address);

        public override string CellAddress
        {
            get
            {
              string  _rVal = $"{mzUtils.ConvertIntegerToLetter(StartColumn)}{ StartRow}";
                double ht = Math.Round(Height, 0);
                if (ht < 1) ht = 1;

                double wd = Math.Round(Width, 0);
                if (wd < 1) wd = 1;

                if (wd > 1 || ht > 1)
                {
                    _rVal = $"{_rVal}:{mzUtils.ConvertIntegerToLetter(StartColumn + wd - 1)} {StartRow + (ht - 1)}";
                }
               

                return _rVal;
            }
            set
            {
                _Address = value;
            }
        }

        private string _Address;
        public string Address { get =>  string.IsNullOrEmpty(_Address) ? CellAddress : _Address; set => _Address = value; }

        private uopAlignments _Alignment = uopAlignments.MiddleLeft;
        public uopAlignments Alignment
        {
            get => _Alignment;
            set { if (Convert.ToInt32(value) >= 1) { _Alignment = value; } }
        }

        private string _FontName = "Arial";
        public string FontName
        {
            get => _FontName;
            set { if (!string.IsNullOrEmpty(value)) _FontName = value.Trim(); }
        }
        public double FontSize { get; set; }
        
        public uopProperty Property { get => base.Clone(); set => base.Copy(value,true,false); }

        public System.Drawing.Color FontColor{ get; set; }

        public bool HasName => !string.IsNullOrWhiteSpace(_Name);


        private string _Name = null;
        public override string Name { get => HasName ? _Name : Address ; set { _Name = value; if (HasName) base.Name = _Name; } }

        public System.Drawing.Color BackColor { get; set; }

        public bool BoldText { get; set; }

        public bool Bold
        {
            get => mzUtils.BitCodeContains(TextFormat, (int)mzTextFormats.Bold);

            set
            {
                if (value)
                {
                    if (!Bold) TextFormat += (int)mzTextFormats.Bold;
                }
                else
                {
                    if (Bold) TextFormat -= (int)mzTextFormats.Bold;
                }
            }
        }
        public bool Italic
        {
            get => mzUtils.BitCodeContains(TextFormat, (int)mzTextFormats.Italic);

            set
            {
                if (value)
                {
                    if (!Italic) TextFormat += (int)mzTextFormats.Italic;

                }
                else
                {
                    if (Italic) TextFormat -= (int)mzTextFormats.Italic;

                }

            }
        }
        public bool Underlined
        {
            get => mzUtils.BitCodeContains(TextFormat, (int)mzTextFormats.Underlined);
            set
            {
                if (value)
                {
                    if (!Underlined) TextFormat += (int)mzTextFormats.Underlined;

                }
                else
                {
                    if (Underlined) TextFormat -= (int)mzTextFormats.Underlined;

                }
            }
        }
        public bool DataOnly { get; set; }

        private bool _Locked = false;
        public override bool Locked { get => _Locked; set => _Locked = value; }

        //private int _Row;
        //public int Row { get =>_Row ; internal set { _Row = value; UpdatePropertyData(); } }

        //private int _Col;
        //public int Column { get => _Col; internal set { _Col = value; UpdatePropertyData(); } }

        private int _StartRow = 1;
        public int StartRow
        {
            get => _StartRow;
            set { if (value >= 1) _StartRow = value; }
        }
        private int _StartColumn = 1;
        public int StartColumn
        {
            get =>_StartColumn;
            set { if (value >= 1)_StartColumn = value; }
        }
        public string Handle  => $"{Row},{Col}";

        private double _Height = 1;
        public double Height
        {
            get => _Height;
            set => _Height = mzUtils.LimitedValue(mzUtils.VarToDouble(value, true, _Height, -1, aValueControl: uopValueControls.PositiveNonZero), 0, 1000);

        }
        private double _Width =1;
        public double Width
        {
            get => _Width;
            set => _Width = mzUtils.LimitedValue( mzUtils.VarToDouble(value, true, _Width, -1, aValueControl: uopValueControls.PositiveNonZero), 0, 1000);

        }
        public int HorizontalAlignment
        {
            get
            {
                //^the horizontal aligment of the cell text matches excel horizontal alignments
                if (Alignment == uopAlignments.TopLeft || _Alignment == uopAlignments.MiddleLeft || _Alignment == uopAlignments.BottomLeft)
                {
                    return 6; //XLAlignmentHorizontalValues.Left
                }
                else if (_Alignment == uopAlignments.TopRight || _Alignment == uopAlignments.MiddleRight || _Alignment == uopAlignments.BottomRight)
                {
                    return 7; //XLAlignmentHorizontalValues.Right
                }
                else
                {
                    return 0; //XLAlignmentHorizontalValues.Center
                }
            }
        }
        public int HorizontalAlignmentIO
        {
            get
            {
                //^the horizontal aligment of the cell text matches excel horizontal alignments
                if (Alignment == uopAlignments.TopLeft || _Alignment == uopAlignments.MiddleLeft || _Alignment == uopAlignments.BottomLeft)
                {
                    return -4131; //Microsoft.Office.Interop.Excel.xlHAlignLeft
                }
                else if (_Alignment == uopAlignments.TopRight || _Alignment == uopAlignments.MiddleRight || _Alignment == uopAlignments.BottomRight)
                {
                    return -4152; //Microsoft.Office.Interop.Excel.xlHAlignRight
                }
                else
                {
                    return -4108; //Microsoft.Office.Interop.Excel.xlHAlignCenter
                }
            }
        }

        public int VerticalAlignment
        {
            get
            {
                //^the horizontal vertical of the cell text matches excel vertical alignments
                if (_Alignment == uopAlignments.TopCenter || _Alignment == uopAlignments.TopLeft || _Alignment == uopAlignments.TopRight)
                {
                    return 4; //XLAlignmentVerticalValues.Top
                }
                else if (_Alignment == uopAlignments.BottomCenter || _Alignment == uopAlignments.BottomLeft || _Alignment == uopAlignments.BottomRight)
                {
                    return 0; //XLAlignmentVerticalValues.Bottom
                }
                else
                {
                    return 1; //XLAlignmentVerticalValues.Center
                }
            }
        }

        public int VerticalAlignmentIO
        {
            get
            {
                //^the horizontal vertical of the cell text matches excel vertical alignments
                if (_Alignment == uopAlignments.TopCenter || _Alignment == uopAlignments.TopLeft || _Alignment == uopAlignments.TopRight)
                {
                    return -4160; //Microsoft.Office.Interop.Excel.xlVAlignTop
                }
                else if (_Alignment == uopAlignments.BottomCenter || _Alignment == uopAlignments.BottomLeft || _Alignment == uopAlignments.BottomRight)
                {
                    return -4107; //Microsoft.Office.Interop.Excel.xlVAlignBottom
                }
                else
                {
                    return -4108; //Microsoft.Office.Interop.Excel.xlVAlignCenter
                }
            }
        }
        public bool IsDefined => !string.IsNullOrEmpty(ValueS);

        public string NumberFormat { get; set; }

        private double _Orientation;
        public double Orientation
        {
            get => _Orientation;
            set => _Orientation = mzUtils.NormAng(value);

        }
      
        public string StringValue => ValueS;
        

        public bool WrapText { get; set; }
        public mzTextFormats TextFormat { get; set; }
     
    
    
        public int HTMLFontSize { get; set; }
        #endregion



        #region Public Methods

        /// <summary>
        /// returns clone object of the current object
        /// </summary>
        /// <returns></returns>
        public new uopTableCell Clone() => new uopTableCell(this);
        
        /// <summary>
        /// copies the passed cell properties to this cell
        /// </summary>
        /// <param name="aCell"></param>
        public void Copy(uopTableCell aCell)
        {
            if (aCell == null) return;
            Width = aCell.Width;
            Height = aCell.Height;
            TextFormat = aCell.TextFormat;
            NumberFormat = aCell.NumberFormat;
            StartColumn = aCell.StartColumn;
            StartRow = aCell.StartRow;
            Alignment = aCell.Alignment;
            WrapText = aCell.WrapText;
            FontName = aCell.FontName;
            FontSize = aCell.FontSize;
            Locked = aCell.Locked;
            _Address = aCell.Address;
            Orientation = aCell.Orientation;
            HTMLFontSize = aCell.HTMLFontSize;
            Structure = new TPROPERTY(aCell.Property);
            FontColor = aCell.FontColor;
            BackColor = aCell.BackColor;
            DataOnly = aCell.DataOnly;
            _BorderData = Force.DeepCloner.DeepClonerExtensions.DeepClone<int[,]>(aCell._BorderData);

        }
      


        private int SideToBorderIndex(mzSides aSide)
        {
            int idx = aSide switch
            {
                mzSides.Left => 0,
                mzSides.Top => 1,
                mzSides.Right => 2,
                mzSides.Bottom => 3,
                _ => -1
            };
            return idx;
        }

        /// <summary>
        /// Sets Border data to out parameter for passed border index
        /// </summary>
        /// <param name="aBORDER"></param>
        /// <param name="aStyle"></param>
        /// <param name="aWeight"></param>
        public void GetBorderData(mzSides aSide, out mzBorderStyles aStyle, out mzBorderWeights aWeight)
        {
            aStyle = mzBorderStyles.None;
            aWeight = mzBorderWeights.Thin;
            int idx = SideToBorderIndex(aSide);
            if (idx < 0 || idx > 3)
            {
                return;
            }
           
              
            aStyle = (mzBorderStyles)_BorderData[idx, 0];
            aWeight = (mzBorderWeights)_BorderData[idx, 1];
        }

        /// <summary>
        /// Sets Border data to out parameter for passed border index
        /// </summary>
        /// <param name="aBORDER"></param>
        /// <param name="aStyle"></param>
        /// <param name="aWeight"></param>
        public void GetBorderData(int aSide, out mzBorderStyles aStyle, out mzBorderWeights aWeight)
        {
            aStyle = mzBorderStyles.None;
            aWeight = mzBorderWeights.Thin;
            int idx = aSide;
            if (idx < 0 || idx > 3)
            {
                return;
            }


            aStyle = (mzBorderStyles)_BorderData[idx, 0];
            aWeight = (mzBorderWeights)_BorderData[idx, 1];
        }
        /// <summary>
        /// Set Border Data
        /// </summary>
        /// <param name="aSide"></param>
        /// <param name="aStyle"></param>
        /// <param name="aWeight"></param>
        public void SetBorderData(mzSides aSide, mzBorderStyles aStyle, mzBorderWeights aWeight)
        {

            List<int> bitvals = mzUtils.DecomposeBitCode((int)aSide);
          
            if (bitvals.Contains((int)mzSides.None))
            {
                _BorderData[0, 0] = (int)mzBorderStyles.Undefined;
                _BorderData[0, 1] = (int)mzBorderWeights.Undefined;
            }
            else
            {
                if (bitvals.Contains((int)mzSides.Top))
                {
                    _BorderData[0, 0] = (int)aStyle;
                    _BorderData[0, 1] = (int)aWeight;
                }
                if (bitvals.Contains((int)mzSides.Left))
                {
                    _BorderData[1, 0] = (int)aStyle;
                    _BorderData[1, 1] = (int)aWeight;
                }
                if (bitvals.Contains((int)mzSides.Bottom))
                {
                    _BorderData[2, 0] = (int)aStyle;
                    _BorderData[2, 1] = (int)aWeight;
                }
                if (bitvals.Contains((int)mzSides.Right))
                {
                    _BorderData[3, 0] = (int)aStyle;
                    _BorderData[3, 1] = (int)aWeight;
                }
            }
        }

        /// <summary>
        /// Set Border Data
        /// </summary>
        /// <param name="aSide"></param>
        /// <param name="aStyle"></param>
        /// <param name="aWeight"></param>
        public void SetBorderData(dynamic aSide, mzBorderStyles aStyle, mzBorderWeights aWeight)
        {

            List<int> bitvals = mzUtils.DecomposeBitCode(aSide);
            if (bitvals.Contains((int)mzSides.None))
            {
                _BorderData[0, 0] = (int)mzBorderStyles.Undefined;
                _BorderData[0, 1] = (int)mzBorderWeights.Undefined;
            }
            else
            {
                if (bitvals.Contains((int)mzSides.Top))
                {
                    _BorderData[0, 0] = (int)aStyle;
                    _BorderData[0, 1] = (int)aWeight;
                }
                if (bitvals.Contains((int)mzSides.Left))
                {
                    _BorderData[1, 0] = (int)aStyle;
                    _BorderData[1, 1] = (int)aWeight;
                }
                if (bitvals.Contains((int)mzSides.Bottom))
                {
                    _BorderData[2, 0] = (int)aStyle;
                    _BorderData[2, 1] = (int)aWeight;
                }
                if (bitvals.Contains((int)mzSides.Right))
                {
                    _BorderData[3, 0] = (int)aStyle;
                    _BorderData[3, 1] = (int)aWeight;
                }
            }
      
         
        }

        public bool SetAddress(string aAddress, dynamic aValue = null)
        {
            bool _rVal = false;
            if (!string.IsNullOrWhiteSpace(aAddress))
            {
                aAddress = aAddress.Trim();
                if (string.Compare(aAddress, Address, true) != 0) _rVal = true;
                Address = aAddress;
            }
            if(aValue != null)
            {
                if (SetValue(aValue)) _rVal = true;
            }
            return _rVal;
        }

       
        public override string ToString()
        {
            string _rVal = $"uopTableCell[{ Handle }] - {base.ToString()}";
            if (!string.IsNullOrWhiteSpace(_Address)) _rVal += $" '{_Address}'";
            return _rVal;
        }

        object ICloneable.Clone()
        {
            return (object)Clone();
        }
        #endregion

    }
}
