using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Security.Policy;
using UOP.DXFGraphics;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Interfaces;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;


namespace UOP.WinTray.Projects
{
    /// <summary>
    /// Provides grid functionalities
    /// </summary>
    public class uopGrid : uopCompoundShape, ICloneable
    {
        public delegate void GenerationStartHandler();
        public event GenerationStartHandler EventGenerationStart;


        public delegate void GenerationCompleteHandler(uopVectors aGridPoints );
        public event GenerationCompleteHandler EventGenerationComplete;

        public delegate void OriginCreatedHandler(uopVector aOrigin, uopGrid aGrid);
        public event OriginCreatedHandler EventOriginCreated;

        public delegate void RowLinesCreatedHandler(uopLines aRowLines);
        public event RowLinesCreatedHandler EventRowLinesCreated;

        public delegate void ColumnLinesCreatedHandler(uopLines aColLines);
        public event ColumnLinesCreatedHandler EventColumnLinesCreated;


        /// <summary>
        /// Raised prior to adding a grid point during point generation
        /// </summary>
        /// <remarks>allows clients to decide if the point should be saved or to save it and suppress it</remarks>
        /// <param name="aGridPoint">the point that has been deemed  legitimate grid point</param>
        /// <param name="aGridPoints">the currently saved grid points  before the new point is added</param>
        /// <param name="ioKeep">the io flag indicating if the new point should be saved </param>
        /// <param name="ioSuppress">the io flag indication that if the point is saved that it should be marked as suppred</param>
        public delegate void GridPointAddedHandler(uopVector aGridPoint, uopVectors aGridPoints,  ref bool ioKeep, ref bool ioSuppress, ref bool rStopProcessing, uopLine aRowLine, uopLine aColumnLine, ref uopRectangle aRectangleToSave, bool bIsMirrorPoint );
        public event GridPointAddedHandler  EventGridPointAdded;

        #region Fields

        internal UVECTOR _Origin;

        internal UVECTOR? _OverrideOrigin;

        internal URECTANGLE _Extremes;

        internal URECTANGLE _PointExtremes;

        internal URECTANGLE _PointLimits;

        #endregion Fields

        #region Constructors

        public uopGrid() => Init();

        public uopGrid(uopGrid aGrid) => Init(aGrid);

        internal uopGrid(uopRectangles aRectangleCollector, uopRectangle aBaseRectangle)
        {
            Init();
            RectangleCollector = uopRectangles.CloneCopy(aRectangleCollector);
            RectangleToSave = uopRectangle.CloneCopy(aBaseRectangle);
        }
        public uopGrid(iShape aShape) { Init(null); base.Copy(aShape); }

        internal uopGrid(UGRID aGrid) => Copy(aGrid);
        
        internal void Init(uopGrid aGrid = null)
        {

            if(aGrid == null)
            {
                RectangleCollector = null;
                RectangleToSave = null;
                _Invalid = true;
                _OverrideOrigin = null;
                _Origin = UVECTOR.Zero;
             
                Clear();
                _Extremes = URECTANGLE.Null;
                _PointExtremes = URECTANGLE.Null;
                _PointLimits = new URECTANGLE(double.MinValue / 2, double.MaxValue / 2, double.MaxValue / 2, double.MinValue / 2);

                _Alignment = uppGridAlignments.Undefined;
                _PitchType = dxxPitchTypes.Rectangular;
                 _OnIsIn = false;
                _ReverseRowGeneration = false;
                _ReverseColumnGeneration = false;
                PartialRowLocation = string.Empty;
                Tag = string.Empty;
                _VPitch = 0;
                _HPitch = 0;
                _XOffset = 0;
                _YOffset = 0;
                _MaxRows = 0;
                _MaxCols = 0;
                _MaxCount = 0;
                _PointCount = 0;
                _MaxX = null;
                _MirrorLine = null;
                _Invalid = true;
                Islands = null;
                ValidateMirrorPoints = false;
                SuppressInteriorCheck = false;
                TrimRowLinesToBounds = false;
                TrimColumnLinesToBounds = false;
            }
            else
            {
                Copy(aGrid);
            }
        }



        #endregion Constructors

        #region Properties

      
        public bool ValidateMirrorPoints { get; set; }
        public uopArcRecs Islands { get; set; }

        internal uopRectangles RectangleCollector { get; set; }

        internal uopRectangle RectangleToSave { get; set; }

        protected bool _Invalid;
        public bool Invalid { get => _Invalid || _Rows == null || _Cols == null; set => _Invalid = value; }

        //the center of the bounding rectangle of the grid boundary
        public override double X => BoundsV.X;

        //the center of the bounding rectangle of the grid boundary
        public override double Y => BoundsV.Y;



        protected uppGridAlignments _Alignment;
        public uppGridAlignments Alignment { get => _Alignment; set { if (_Alignment != value) Invalid = true; _Alignment = value; } }

        public uppVerticalAlignments VerticalAlignment => Alignment.VerticalAlignment();
        public uppHorizontalAlignments HorizontalAlignment => Alignment.HorizontalAlignment();

        protected int _PointCount;
        public int PointCount { get { if (Invalid) Generate(); return _PointCount; } }

        protected int _FreePointCount;
        public int FreePointCount
        {
            get { if (Invalid) Generate(); return _FreePointCount; }
        }

        protected dxxPitchTypes _PitchType;
        public virtual dxxPitchTypes PitchType { get => _PitchType; set { if (_PitchType != value) Invalid = true; _PitchType = value; } }

        bool _OnIsIn;
        public virtual  bool OnIsIn { get => _OnIsIn; set { if (_OnIsIn != value) Invalid = true; _OnIsIn = value; } }

        bool _ReverseRowGeneration;
        public bool ReverseRowGeneration { get => _ReverseRowGeneration; set { if (_ReverseRowGeneration != value) Invalid = true; _ReverseRowGeneration = value; } }

        bool _ReverseColumnGeneration;
        public bool ReverseColumnGeneration { get => _ReverseColumnGeneration; set { if (_ReverseColumnGeneration != value) Invalid = true; _ReverseColumnGeneration = value; } }

        string _PartialRowLocation;
        public string PartialRowLocation { get => _PartialRowLocation; set { if (_PartialRowLocation != value) Invalid = true; _PartialRowLocation = value; } }
        
        protected double _VPitch;
        public virtual double VPitch { get => _VPitch; set { if (_VPitch != value) Invalid = true; _VPitch = value; } }

        protected double _HPitch;
        public virtual double HPitch { get => _HPitch; set { if (_HPitch != value) Invalid = true; _HPitch = value; } }

        protected double _XOffset;
        public double XOffset { get => _XOffset; set { if (_XOffset != value) Invalid = true; _XOffset = value; } }

        protected double _YOffset;
        public double YOffset { get => _YOffset; set { if (_YOffset != value) Invalid = true; _YOffset = value; } }

        protected int _MaxRows;
        public int MaxRows { get => _MaxRows; set { if (_MaxRows != value) Invalid = true; _MaxRows = value; } }

        protected int _MaxCols;
        public int MaxCols { get => _MaxCols; set { if (_MaxCols != value) Invalid = true; _MaxCols = value; } }

        protected int _MaxCount;
        public int MaxCount { get => _MaxCount; set { if (_MaxCount != value) Invalid = true; _MaxCount = value; } }

        protected double? _MaxX;
        public double? MaxX { get => _MaxX; set { if (_MaxX != value) Invalid = true; _MaxX = value; } }


        protected double? _MinX;
        public double? MinX { get => _MinX; set { if (_MinX != value) Invalid = true; _MinX = value; } }
        protected uopLine _MirrorLine;
        public uopLine MirrorLine { get => _MirrorLine; set { if ((_MirrorLine != null && value == null) || (_MirrorLine == null && value != null)) Invalid = true; _MirrorLine = value; } }


        internal void SetBounds(uopCompoundShape aShape, uopArcRecs aIslands)
        {
            if (aShape == null) return;
            this._Vertices = aShape._Vertices;
            this._SubShapes = aShape._SubShapes;
            this._Segments = aShape._Segments;
            this.Islands = aIslands;

        }
     
        public uopCompoundShape Boundary { get => new uopCompoundShape(this);
            set
            {
                Clear();
                base.Copy(value ?? new uopCompoundShape());
                

            }
        }
        public virtual uopVector Origin { get { if (_Invalid && !_Generating) Create_Origin(); return new uopVector(_Origin); } }


     

        private double HStepFactor => PitchType == dxxPitchTypes.Rectangular ? 1.0 : PitchType == dxxPitchTypes.InvertedTriangular ? 1.0 : 0.5;
        private double VStepFactor => PitchType == dxxPitchTypes.Rectangular ? 1.0 : PitchType == dxxPitchTypes.InvertedTriangular ? 0.5 : 1.0;

        public bool TriangularPitch => PitchType == dxxPitchTypes.Triangular || PitchType == dxxPitchTypes.InvertedTriangular;

        public bool TrimColumnLinesToBounds { get; set; }
        public bool TrimRowLinesToBounds { get; set; }
        protected uopLines _Rows;
        protected uopLines Rows 
        {
            get
            {
                UpdateGridPoints();
                return _Rows;
            }
         
        }

        protected uopLines _Cols;
        protected uopLines Cols
        {
            get
            {
                UpdateGridPoints();
                return _Cols;
            }
          
        }

        public bool SuppressInteriorCheck { get; set; }
        #endregion Properties

        #region Methods
        public override string ToString() => string.IsNullOrWhiteSpace(Name) ? $"uopSlotGrid[{Vertices.Count}]" : $"uopSlotGrid[{Vertices.Count}] '{Name}'";

        public int RowCount() {  return _Rows == null ? 0 :  _Rows.GetByPoints((x) => !x.Suppressed).Count;  }

        public int ColCount() {  return _Cols == null ? 0 :  _Cols.Count;  }

        /// <summary>
        /// returns top row of the grid (with more than zero grid points defined)
        /// </summary>
        /// <param name="bPopulated"></param>
        /// <returns></returns>
        public uopLine TopRow(bool bPopulated = false)
        {


            uopLines rows = bPopulated ?  Rows.GetByPoints((x) => !x.Suppressed ) : Rows;
            uopLine _rVal = rows.GetAtMaxOrdinate(bTestY: true, bTestEndPt: false);
            if (_rVal != null) _rVal.Index = _Rows.IndexOf(_rVal) + 1;
            return _rVal;
        
        }

        public void InvertRows()
        {
            if (_Rows.Count <= 1) return;
            uopLines oldrows = new uopLines(_Rows, bAddClones: false);
             _Rows.Clear();

            for(int r = oldrows.Count; r >=1; r--)
            {
                uopLine newrow = oldrows.Item(r);
                newrow.Index = _Rows.Count + 1;
                newrow.Row = newrow.Index;
                _Rows.Add(newrow);
            }
        }

        public void InvertCols()
        {
           
            if (_Cols.Count <= 1) return;
            uopLines oldcols = new uopLines(_Cols, bAddClones:false);
            _Cols.Clear();

            for (int c = oldcols.Count; c >= 1; c--)
            {
                uopLine newcol = oldcols.Item(c);
                newcol.Index = _Cols.Count + 1;
                newcol.Col = newcol.Index;
                _Cols.Add(newcol);
            }

        }
        /// <summary>
        /// returns bottom row of the grid (with more than zero grid points defined)
        /// </summary>
        /// <param name="bPopulated"></param>
        /// <returns></returns>
        internal uopLine BottomRow(bool bPopulated = false)
        {

            uopLines rows = bPopulated ? Rows.GetByPoints((x) => !x.Suppressed) : Rows;
            uopLine _rVal = rows.GetAtMinOrdinate(bTestY: true, bTestEndPt: false);
            if (_rVal != null) _rVal.Index = _Rows.IndexOf(_rVal) + 1;
            return _rVal;


        }

        public new void Mirror(double? aX, double? aY)
        {
            base.Mirror(aX, aY);
            if (!aX.HasValue && !aY.HasValue) return;
            if (_MirrorLine != null) _MirrorLine.Mirror(aX, aY);
       
            _PointLimits.Mirror(aX, aY);
            _PointExtremes.Mirror(aX, aY);
            _Extremes.Mirror(aX, aY);
            _Rows?.Mirror(aX, aY);
            _Cols?.Mirror(aX, aY);
            _Origin.Mirror(aX, aY);
            if (MaxX.HasValue && aX.HasValue)
            {
                double dx = MaxX.Value - aX.Value;
                MaxX = MaxX.Value - 2 * dx;
            }
            
        }


        /// <summary>
        /// Re -generate grids
        /// </summary>
        /// <param name="aXOffset"></param>
        /// <param name="aYOffset"></param>
        public virtual void ReGenerate(double aXOffset = 0, double aYOffset = 0)
        {
            if (aXOffset == 0 & aYOffset == 0) { return; }


            _PointCount = 0;
        
          
            Update();

            if (Segments.Count <= 0)
                return;
            
            if (Width <= 0 ||   Height <= 0)
            { return; }

            Origin.X += aXOffset;
            Origin.Y += aYOffset;

            //Create_Origin
            Create_RowsAndColumns();

            Create_GridPoints();
           
        }


        /// <summary>
        /// method for translating the grid
        /// </summary>
        /// <returns></returns>
        public virtual void Translate(double aXOffSet, double aYOffset, bool bJustMove = false)
        {
            if (aXOffSet == 0 && aYOffset == 0)  return; 


            if (!bJustMove)
            { 
                ReGenerate(aXOffSet, aYOffset); 
            }
            else
            {

                _PointExtremes.Translate(aXOffSet, aYOffset);
                _Extremes.Translate(aXOffSet, aYOffset);
                _PointLimits.Translate(aXOffSet, aYOffset);
                _Origin.Translate(aXOffSet, aYOffset);
                if (_OverrideOrigin.HasValue) _OverrideOrigin = _OverrideOrigin.Value.Moved(aXOffSet, aYOffset);
                _Rows?.Move(aXOffSet, aYOffset);
                _Cols?.Move(aXOffSet, aYOffset);

            }
            if (_Rows != null)
                _GridPts = _Rows.Points(bSuppressVal: false, bGetClones:false);
        }
        public virtual void Copy(uopGrid aGrid)
        {

            if(aGrid == null) return;
            base.Copy(aGrid);
            _Origin = new UVECTOR(aGrid._Origin);
     
            if (_Rows != null) _Rows.Clear(); else _Rows = new uopLines() { Name = "Rows" };
            if (_Cols != null) _Cols.Clear(); else _Cols = new uopLines() { Name = "Columns" };

            _Rows.Populate(aGrid._Rows,true);
            _Cols.Populate(aGrid._Cols, true);
            Tag = aGrid.Tag;
            ValidateMirrorPoints = aGrid.ValidateMirrorPoints;
            _Extremes = new URECTANGLE(aGrid._Extremes);
            _PointExtremes = new URECTANGLE(aGrid._PointExtremes);
            _PointLimits = new URECTANGLE(aGrid._PointExtremes);

            _Alignment = aGrid._Alignment;
            _PitchType = aGrid._PitchType;
            _OnIsIn = aGrid._OnIsIn;
            _ReverseRowGeneration = aGrid._ReverseRowGeneration;
            _ReverseColumnGeneration = aGrid._ReverseColumnGeneration;
            _PartialRowLocation = aGrid._PartialRowLocation;
            _VPitch = aGrid._VPitch;
            _HPitch = aGrid._HPitch;
            _XOffset = aGrid._XOffset;
            _YOffset = aGrid._YOffset;
            _MaxRows = aGrid._MaxRows;
            _MaxCols = aGrid._MaxCols;
            _MaxCount = aGrid._MaxCount;
            _MaxCount = aGrid._MaxCount;
            _PointCount = aGrid._PointCount;
            _MaxX = aGrid.MaxX;
            _MirrorLine = uopLine.CloneCopy(aGrid.MirrorLine);
            _OverrideOrigin = null;
            _Invalid = aGrid._Invalid;
            if (aGrid._OverrideOrigin.HasValue )_OverrideOrigin =  new UVECTOR(aGrid._OverrideOrigin.Value);
            _GridPts = uopVectors.CloneCopy(aGrid._GridPts);
            RectangleCollector = uopRectangles.CloneCopy(aGrid.RectangleCollector);
            RectangleToSave = uopRectangle.CloneCopy(aGrid.RectangleToSave);
            Islands = uopArcRecs.CloneCopy(aGrid.Islands);
            ValidateMirrorPoints = aGrid.ValidateMirrorPoints;
            SuppressInteriorCheck = aGrid.SuppressInteriorCheck;
            TrimColumnLinesToBounds = aGrid.TrimColumnLinesToBounds;
            TrimRowLinesToBounds = aGrid.TrimRowLinesToBounds;
        }

        protected virtual void CopyInternals(uopGrid aGrid)
        {

            if (aGrid == null) return;
          
            _Origin = aGrid._Origin;
            _Rows = aGrid._Rows;
            _Cols = aGrid._Cols;

            
            Tag = aGrid.Tag;

            _Extremes =aGrid._Extremes;
            _PointExtremes = aGrid._PointExtremes;
            _PointLimits = aGrid._PointExtremes;

            _Alignment = aGrid._Alignment;
            _PitchType = aGrid._PitchType;
            _OnIsIn = aGrid._OnIsIn;
            _ReverseRowGeneration = aGrid._ReverseRowGeneration;
            _ReverseColumnGeneration = aGrid._ReverseColumnGeneration;
            _PartialRowLocation = aGrid._PartialRowLocation;
            _VPitch = aGrid._VPitch;
            _HPitch = aGrid._HPitch;
            _XOffset = aGrid._XOffset;
            _YOffset = aGrid._YOffset;
            _MaxRows = aGrid._MaxRows;
            _MaxCols = aGrid._MaxCols;
            _MaxCount = aGrid._MaxCount;
            _MaxCount = aGrid._MaxCount;
            _PointCount = aGrid._PointCount;
            _MaxX = aGrid.MaxX;
            _MirrorLine = aGrid._MirrorLine;
            _OverrideOrigin = null;
            _Invalid = aGrid._Invalid;
            _OverrideOrigin = aGrid._OverrideOrigin;
            _GridPts = aGrid._GridPts;
            RectangleCollector = aGrid.RectangleCollector;
            RectangleToSave = aGrid.RectangleToSave;
            Islands = aGrid.Islands;
            ValidateMirrorPoints = aGrid.ValidateMirrorPoints;
            SuppressInteriorCheck = aGrid.SuppressInteriorCheck;
            TrimColumnLinesToBounds = aGrid.TrimColumnLinesToBounds;
            TrimRowLinesToBounds = aGrid.TrimRowLinesToBounds;
        }

        internal void Copy(UGRID aGrid)
        {

            base.Copy(aGrid.Boundary);
            _Origin = new UVECTOR(aGrid.Origin);
           

            _Rows ??= new uopLines() { Name = "ROWS" };
            _Cols ??= new uopLines() { Name = "COLS" };

            _Rows.Populate(aGrid.Rows);
            _Cols.Populate(aGrid.Cols);

            _Extremes = new URECTANGLE(aGrid.Extremes);
            _PointExtremes = new URECTANGLE(aGrid.PointExtremes);
            _PointLimits = new URECTANGLE(aGrid.PointExtremes);

            Alignment = aGrid.Alignment;
            PitchType = aGrid.PitchType;
            OnIsIn = aGrid.OnIsIn;
            ReverseRowGeneration = aGrid.ReverseRowGeneration;
            ReverseColumnGeneration = aGrid.ReverseColumnGeneration;
            PartialRowLocation = aGrid.PartialRowLocation;
            Tag = aGrid.Tag;
            VPitch = aGrid.VPitch;
            HPitch = aGrid.HPitch;
            XOffset = aGrid.XOffset;
            YOffset = aGrid.YOffset;
            MaxRows = aGrid.MaxRows;
            MaxCols = aGrid.MaxCols;
            MaxCount = aGrid.MaxCount;
            _PointCount = aGrid.PointCount;
            MaxX = aGrid.MaxX;
            MirrorLine = null;
           if(aGrid.MirrorLine.HasValue) MirrorLine = new uopLine( aGrid.MirrorLine.Value);
        }

        object ICloneable.Clone() => new uopGrid(this);

        public new uopGrid Clone() => new uopGrid(this);


        /// <summary>
        /// Method for adding Rec or Arc based on condition
        /// </summary>
        /// <param name="aX"></param>
        /// <param name="aY"></param>
        /// <param name="bCircular"></param>
        /// <param name="aWidthOrRadius"></param>
        /// <param name="aHeight"></param>
        /// <param name="aRotation"></param>
        /// <param name="aTag"></param>
        /// <param name="aFlag"></param>
        public void AddIsland(double aX, double aY, bool bCircular, double aWidthOrRadius, double aHeight = 0, double aRotation = 0, string aTag = "", string aFlag = "")
        {
            double wd = mzUtils.VarToDouble(aWidthOrRadius, true);

            double ht = mzUtils.VarToDouble(aHeight, true);
            if (wd <= 0) return;

            if (!bCircular && ht <= 0) return;
            if (!bCircular)
            {
                SubShapes.Add(uopShape.Circle(new uopVector(aX, aY), aRadius: aWidthOrRadius, aTag, aFlag) );
            }
            else
            {
                SubShapes.Add(uopShape.Rectangle(new uopVector(aX, aY), aWidth:aWidthOrRadius, aHeight:aHeight, aRotation, aTag, aFlag) );
            }


        }

        public virtual bool ValidateVector(iVector aVector, bool bIsMirrorPt, out string rErrorString, bool? bAlt = null, double? aHStepFactor = null, double? aVStepFactor = null, bool bIgnoreMaxes = false, bool? bRectangularBounds = null)
        {
            rErrorString = string.Empty;
            if (aVector == null)
            {
                rErrorString = "NULL VECTOR PASSED";
                return false;

            }
        
            //if(Math.Round(aVector.X,2) == -6.23 && Math.Round(aVector.Y, 2) == -167.25)
            //{
            //    Console.WriteLine("HERE");
            //}
            if (!bAlt.HasValue) bAlt = TriangularPitch;
            if (!aHStepFactor.HasValue) aHStepFactor = HStepFactor;
            if (!aVStepFactor.HasValue) aVStepFactor = VStepFactor;


            //gross check that the point is within bounds
            if (!_PointLimits.ContainsOrd(aVector.X, bOrdIsY: false, bOnIsIn: OnIsIn, aPrecis: 5) && _PointLimits.ContainsOrd(aVector.Y, bOrdIsY: true, bOnIsIn: OnIsIn, aPrecis: 5))
                rErrorString = "POINT LIMITS";

            if (!bIgnoreMaxes && rErrorString == string.Empty && MaxX.HasValue && !bIsMirrorPt)
            {
                if (Math.Round(aVector.X,4) > MaxX.Value)
                    rErrorString = "MAX X VIOLATION";

            }

            if (!bIgnoreMaxes && rErrorString == string.Empty && MinX.HasValue && !bIsMirrorPt)
            {
                if (Math.Round(aVector.X, 4) < MinX.Value)
                    rErrorString = "MIN X VIOLATION";

            }

            if (bAlt.Value && rErrorString == string.Empty )
            {
                double dX = aVector.X - _Origin.X;
                double dY = aVector.Y - _Origin.Y;
                double xsteps = Math.Round(dX / (aHStepFactor.Value * HPitch), 0) + 1;
                double ysteps = Math.Round(dY / (aVStepFactor.Value * VPitch), 0) + 1;
                bool altskip = false;
                if (PitchType == dxxPitchTypes.InvertedTriangular )
                {
                    
                    altskip = mzUtils.IsOdd(xsteps) ? mzUtils.IsOdd(ysteps) : mzUtils.IsEven(ysteps);
                    if (!altskip)
                        rErrorString = "ALTERNATE COLUMN";

                }
                else
                {

                    altskip = mzUtils.IsOdd(ysteps) ? mzUtils.IsOdd(xsteps) : mzUtils.IsEven(xsteps);
                    if (!altskip)
                        rErrorString = "ALTERNATE ROWS";

                }
            }
            if (!bRectangularBounds.HasValue) bRectangularBounds = IsRectangular();
            //bool supcheck = SuppressInteriorCheck && !bIsMirrorPt;
            if (rErrorString == string.Empty && !bRectangularBounds.Value  && !SuppressInteriorCheck)
            {
                double maxX = this.Right + this.Width;
                if (!Segments.ContainsVector(aVector, out uopVectors ips, OnIsIn, aPrecis: 3, aTestOrd: maxX))
                    rErrorString = "EXTERIOR TO BOUNDARY";

            }
            if (rErrorString == string.Empty && Islands != null && Islands.Count > 0 && (!bIsMirrorPt || (bIsMirrorPt && ValidateMirrorPoints)))
            {

                uopArcRecs violators = RectangleToSave == null ? Islands.GetContainers(aVector, true, 3, bJustOne: true) : Islands.GetContainers(new uopRectangle(RectangleToSave, aVector), bOnIsIn: true, aPrecis: 3, bJustOne: true, bReturnTrueByCenter: true);

                if (violators.Count > 0)
                    rErrorString = "ISLAND VIOLATION";
                
            }

            if (rErrorString == string.Empty && Islands != null && RectangleToSave != null)
            {
             
                uopRectangle rec2save = new uopRectangle(RectangleToSave, aVector);
                int violatorid = Islands.FindIndex(x => x.Contains(rec2save, true, 4, false));
                if( violatorid >= 0 )
                { 
                        rErrorString = "ISLAND VIOLATION";
                    
                }
            }
            

            return string.IsNullOrWhiteSpace(rErrorString);

       
        
        }

        public virtual void Clear()
        {

            if (_Rows != null) _Rows.Clear(); else _Rows = new uopLines() { Name = "Rows" };
            if (_Cols != null) _Cols.Clear();  else _Cols = new uopLines() { Name = "Columns" };
            _PointCount = 0;
            _FreePointCount = 0;
               Invalid = true;
            _GridPts = null;
           
         if (RectangleCollector != null) RectangleCollector.Clear();
        }

        public colDXFEntities DXFIslands(string aLayerName = "", dxxColors aColor = dxxColors.Undefined, string aLinetype = "")
        => Islands == null ? new colDXFEntities() :  Islands.ToDXFEntities(aLayerName, aColor, aLinetype);

        /// <summary>
        /// Methods for creating grids points
        /// </summary>
        /// <param name="aHPitch"></param>
        /// <param name="aVPitch"></param>
        /// <param name="aXOffset"></param>
        /// <param name="aYOffset"></param>
        /// <param name="aPitchType"></param>
        /// <param name="aAlignment"></param>
        /// <param name="bOnIsIn"></param>
        /// <returns></returns>
        public uopVectors CreateGridPts(dynamic aHPitch, dynamic aVPitch, double? aXOffset = null, double? aYOffset = null, dxxPitchTypes aPitchType = dxxPitchTypes.Rectangular, uppGridAlignments aAlignment = uppGridAlignments.MiddleCenter, bool bOnIsIn = false)
        {
            Invalid = true;

            PitchType = (dxxPitchTypes)mzUtils.VarToInteger(aPitchType, true, dxxPitchTypes.Rectangular, 0, 2);
            Alignment = (uppGridAlignments)mzUtils.VarToInteger(aAlignment, true, uppGridAlignments.MiddleCenter, 1, 9);
            VPitch = mzUtils.VarToDouble(aVPitch, true, 6);
            HPitch = mzUtils.VarToDouble(aHPitch, true, 6);
            if(aXOffset.HasValue) XOffset =  Math.Round(aXOffset.Value,6);
            if (aYOffset.HasValue) YOffset = Math.Round(aYOffset.Value, 6);
            OnIsIn = bOnIsIn;
           
             Generate();
            
            return GridPoints();
        }

        protected bool _Generating;
        /// <summary>
        /// method for generating grids
        /// </summary>
        public  virtual void Generate()
        {
          
            _Generating = true;
            EventGenerationStart?.Invoke();
            try
            {
                Clear();
                PartialRowLocation = string.IsNullOrWhiteSpace(PartialRowLocation) ? string.Empty : PartialRowLocation.ToUpper().Trim();
                if (!IsDefined) return;
                if (Area <= 0) return;
                Create_Origin();
                Create_RowsAndColumns();
                 Create_GridPoints();

            }
            finally
            {
                _Generating = false;
                _Invalid = false;
            }


        }


        /// <summary>
        /// locate partail row
        /// </summary>
        /// <param name="aGrid"></param>
        public  virtual bool LocatePartialRow(string aPartialRowLocation)
        {
            if (string.IsNullOrWhiteSpace(aPartialRowLocation)) aPartialRowLocation = PartialRowLocation;
            if (string.IsNullOrWhiteSpace(aPartialRowLocation)) return false;
            aPartialRowLocation = aPartialRowLocation.ToUpper().Trim();
            if (aPartialRowLocation != "BOTTOM" && aPartialRowLocation != "TOP") return false;
            if (_FreePointCount <= 0 || _Rows == null || _Rows.Count <= 1 ) return false;

           
            uopLine tRow = TopRow(bPopulated:true);
            uopLine bRow = BottomRow(bPopulated:true);
            
            
            if (tRow == null || bRow == null) return false;
           if (tRow == bRow) return false;

            int pCnt_T = uopGrid.GridRowCount(this, tRow.Index, false, out int FILLDT, out int fCnt_T, out bool XST_T, out double YT);
            int pCnt_B = uopGrid.GridRowCount(this, bRow.Index, false, out int FILLDB, out int fCnt_B, out bool XST_B, out double YB);
            if (aPartialRowLocation == "BOTTOM" && fCnt_T <= 0) return false;
            if (aPartialRowLocation == "TOP" && fCnt_B <= 0) return false;
            if (aPartialRowLocation == "BOTTOM")
            {
                while (!(fCnt_T <= 0 || pCnt_B <= 0))
                {
                    int idx = 0;

                    for (int i = bRow.Points.Count; i >= 1; i--)
                    {
                        uopVector z1 = bRow.Points.Item(i);
                        if (!z1.Suppressed)
                        {
                            idx = i;
                            z1.Suppressed = true;
                            pCnt_B -= 1;
                            break;
                        }
                    }
                    if (idx == 0) break;
                    

                    idx = 0;
                    for (int i = tRow.Points.Count; i >= 1; i--)
                    {
                        uopVector z1 = tRow.Points.Item(i);
                        if (z1.Suppressed)
                        {
                            idx = i;
                            z1.Suppressed = false;
                            fCnt_T -= 1;
                            break;
                        }
                    }
                    if (idx == 0)
                        break;
                }
            }
            else
            {
                if (fCnt_B <= 0 || pCnt_T <= 0)
                    return false;

                while (!(fCnt_B <= 0 || pCnt_T <= 0))
                {
                    int idx = 0;
                    for (int i = tRow.Points.Count; i >= 1; i--)
                    {
                        uopVector z1 = tRow.Points.Item(i);
                        if (!z1.Suppressed)
                        {
                            idx = i;
                            z1.Suppressed = true;
                            pCnt_T -= 1;
                            break;
                        }
                    }
                    if (idx == 0)
                        break;
                   
                    idx = 0;
                    for (int i = bRow.Points.Count; i >= 1; i--)
                    {
                        uopVector z1 = bRow.Points.Item(i);
                        if (z1.Suppressed)
                        {
                            idx = i;
                            z1.Suppressed = false;
                            fCnt_B -= 1;
                            break;
                        }
                    }
                    if (idx == 0)
                        break;
                }
            }

            _GridPts = _Rows.Points(bSuppressVal: false);
            return true;
            
        }

        /// <summary>
        /// locate partail row
        /// </summary>
        /// <param name="aGrid"></param>
        public virtual bool SwapPartialRow(string aPartialRowLocation, out uopLine rPartialRow)
        {
            rPartialRow = null;
            if (string.IsNullOrWhiteSpace(aPartialRowLocation)) aPartialRowLocation = PartialRowLocation;
            if (string.IsNullOrWhiteSpace(aPartialRowLocation)) return false;
            aPartialRowLocation = aPartialRowLocation.ToUpper().Trim();
            if (aPartialRowLocation != "BOTTOM" && aPartialRowLocation != "TOP") return false;
            if ( _Rows == null || _Rows.Count <= 1) return false;


            uopLine tRow = TopRow(bPopulated:true);
            uopLine bRow = BottomRow(bPopulated: true);

            if(tRow == bRow  ) 
            {
                if (aPartialRowLocation == "BOTTOM")
                    tRow = null;
                else
                    bRow = null;
            }

            int pCnt_T = tRow == null ?  0 : tRow.Points.Count;
            int pCnt_B = bRow == null ? 0 : bRow.Points.Count;
            int fCnt_T = tRow == null ? 0 : tRow.Points.Count(x => x.Suppressed);
            int fCnt_B = bRow == null ? 0 : bRow.Points.Count(x => x.Suppressed);

            
            bool swap =  (pCnt_T == pCnt_B && pCnt_B > 0 && tRow != null && bRow != null);
            if (aPartialRowLocation == "BOTTOM" && fCnt_T <= 0) swap = false;
            if (aPartialRowLocation == "TOP" && fCnt_B <= 0) swap = false;
            
            if (swap)
            {
                uopVectors row1 = aPartialRowLocation == "TOP" ? tRow.Points : bRow.Points;
                uopVectors row2 = aPartialRowLocation == "TOP" ? bRow.Points : tRow.Points;
                uopVectors source = new uopVectors(row1, bCloneMembers: true);
                for(int i = 1; i <= row1.Count; i++)
                {
                    var u1 = row1[i -1];
                    var u2 = row2[i - 1];
                    var u3 = source[i - 1];
                    //u1.X = u2.X;
                    u1.Suppressed = u2.Suppressed;
                    //u1.Value = u2.Value;
                    //u2.X = u3.X;
                    u2.Suppressed = u3.Suppressed;
                    //u2.Value = u3.Value;
                }

                fCnt_T = tRow == null ? 0 : tRow.Points.Count(x => x.Suppressed);
                fCnt_B = bRow == null ? 0 : bRow.Points.Count(x => x.Suppressed);

            }

            rPartialRow = fCnt_T > fCnt_B ? tRow : bRow;

            _GridPts = _Rows.Points(bSuppressVal: false);
            return swap;

        }

        public virtual void GridGenerationComplete()
        {

            _Generating = false;
            _Invalid = false;
            EventGenerationComplete?.Invoke(_GridPts);
        }

        /// <summary>
        /// calucaltes grid points
        /// </summary>
        /// <param name="aGrid"></param>
        /// <returns></returns>
        public virtual uopVectors Create_GridPoints()
        {
            _GridPts = uopVectors.Zero;
            List<uopGridLine> rows = new List<uopGridLine>();
            List<uopGridLine> cols = new List<uopGridLine>();

            _FreePointCount = 0;
            _PointCount = 0;
            _PointExtremes = BoundsV;
            _PointExtremes.Define(Center.X, Center.X, Center.Y, Center.Y);


            if (!IsDefined) return uopVectors.Zero;
            if (_Rows == null) Create_Rows();
            if (_Cols == null) Create_Columns();

            if (_Rows == null || _Cols == null)
                return _GridPts;
            if (_Rows.Count == 0 || _Cols.Count == 0)
                return _GridPts;

            for (int i = 1; i <= _Rows.Count; i++)
            {
                uopGridLine gline = _Rows[i - 1] is uopGridLine ? (uopGridLine)_Rows[i - 1] : new uopGridLine(_Rows[i - 1]) { IsRow = true, GridOrigin = new uopVector(_Origin) };
                gline.IsRow = true;
                gline.Row = i;
                gline.Points = uopVectors.Zero;
                gline.PitchType = this.PitchType;
                gline.VPitch = _VPitch;
                gline.HPitch = _HPitch;
                gline.GridOrigin ??= new uopVector(_Origin);
                gline.GridOrigin.SetCoordinates(_Origin.X, _Origin.Y);
                rows.Add(gline);
            }

            for (int i = 1; i <= _Cols.Count; i++)
            {
                uopGridLine gline = _Cols[i - 1] is uopGridLine ? (uopGridLine)_Cols[i - 1] : new uopGridLine(_Rows[i - 1]) { IsRow = false, GridOrigin = new uopVector(_Origin) };
                gline.IsRow = false;
                gline.Col = i;
                gline.Points = uopVectors.Zero;
                gline.PitchType = this.PitchType;
                gline.VPitch = _VPitch;
                gline.HPitch = _HPitch;
                gline.GridOrigin ??= new uopVector(_Origin);
                gline.GridOrigin.SetCoordinates(_Origin.X, _Origin.Y);
                cols.Add(gline);
            }

            try
            {

              
                //if (this.Tag.Contains( "2,1"))
                //{
                //    Console.WriteLine("HERE");
                //}
                double hStepFactor = HStepFactor;
                double vStepFactor = VStepFactor;
                bool mirroring = _MirrorLine != null;
                   if (Segments.Count <= 0 || (HPitch  ==0 &&  VPitch ==0) ) return _GridPts;
                
                if (mirroring && _MirrorLine.Length <= 0) mirroring = false;

                //_PointExtremes.Define(u1.X, u1.X, System.Double.MaxValue/2, System.Double.MinValue/2);
                bool bAlt = TriangularPitch;
              
                bool stopprocessing = false;
                bool rectangular = IsRectangular(true, aPrecis: 3);
                for (int r = 1; r <= rows.Count; r++)
                {
                    uopGridLine rLn = rows[r - 1];
                  
                    for (int c = 1; c <= cols.Count; c++)
                    {

                        int altc = uopUtils.OpposingIndex(c, cols.Count);
                        uopGridLine cLn = cols[c - 1] ;
                   

                        uopVector gridpt = new uopVector(cLn.X(), rLn.Y());
                        bool onRow = TrimRowLinesToBounds ?  Math.Round( gridpt.X,2) >= Math.Round(rLn.MinX,2) && Math.Round(gridpt.X,2) <= Math.Round(rLn.MaxX,2): true;
                        bool onCol = TrimColumnLinesToBounds ? Math.Round(gridpt.Y,2) >= Math.Round(cLn.MinY, 2) && Math.Round(gridpt.Y,2) <= Math.Round(cLn.MaxY,2) : true;
                        bool keep = false;
                        bool suppressit = false;
                        string err = string.Empty;
                        gridpt.Row = r;
                        gridpt.Col = c;
                        bool doit = true;
                        if ( MaxX.HasValue)
                        {
                            if (Math.Round(gridpt.X, 4) > MaxX.Value)
                                doit = false;

                        }

                        if ( MinX.HasValue )
                        {
                            if (Math.Round(gridpt.X, 4) < MinX.Value)
                                doit = false;

                        }
                        if (bAlt)
                        {
                          
                            bool altskip = false;
                            if (PitchType == dxxPitchTypes.InvertedTriangular)
                            {

                                altskip = mzUtils.IsOdd(cLn.XStep) ? mzUtils.IsOdd(rLn.YStep) : mzUtils.IsEven(rLn.YStep);
                                if (!altskip)
                                {
                                    err = "ALTERNATE COLUMN";
                                    doit = false;
                                }
                            }
                            else
                            {
                                altskip = mzUtils.IsOdd(rLn.YStep) ? mzUtils.IsOdd(cLn.XStep) : mzUtils.IsEven(cLn.XStep);
                                if (!altskip)
                                {
                                    err = "ALTERNATE ROWS";
                                    doit = false;
                                }
                            }
                        }
                        

                        if (doit)
                        {
                            if (onRow && onCol)
                                SaveGridPoint(gridpt, false, rLn, cLn, out keep, out suppressit, out stopprocessing, out err, false, hStepFactor, vStepFactor, rectangular);
                        }
                            
                        if (stopprocessing) break;

                        if (!mirroring || !doit) continue;

               
                        uopVector mgridpt = new uopVector(gridpt);
                        uopVector dir = mgridpt.DirectionTo(_MirrorLine, out double d1);
                        if (Math.Round(d1, 2) == 0) continue;

                        mgridpt += dir * (2 * d1);
                        uopGridLine colLine = cols.Find(x=> Math.Round(x.X(),1) == Math.Round(mgridpt.X,1));
                        bool supcheck = SuppressInteriorCheck;
                        if(colLine != null)
                        {
                            onCol = Math.Round(mgridpt.Y, 2) >= Math.Round(colLine.MinY, 2) && Math.Round(mgridpt.Y, 2) <= Math.Round(colLine.MaxY, 2);
                            mgridpt.Col = colLine.Col;
                            mgridpt.X = colLine.X();
                        }
                        else
                        {
                            //continue;
                            SuppressInteriorCheck = false;
                            //Console.WriteLine(Boundary.Tag);
                        }

                        if (onCol)
                            SaveGridPoint(mgridpt, true, rLn, cLn, out keep, out suppressit, out stopprocessing, out err, false, hStepFactor, vStepFactor, rectangular);

                        SuppressInteriorCheck = supcheck;
                        if (stopprocessing) break;
                    } // loop on column lines

                    if (stopprocessing) break;
                }// loop on row lines

                
                if (_PointCount <= 0) { uopRectangle limits = Bounds; _PointExtremes.Define(limits.X, limits.X, limits.Y, limits.Y); }

            }
            finally
            {
                _PointCount = _GridPts.Count;
                _Invalid = false;
                _Generating = false;
                if (! string.IsNullOrWhiteSpace(PartialRowLocation)) LocatePartialRow(PartialRowLocation);

                _Rows.Populate(rows);
                _Cols.Populate(cols);
                GridGenerationComplete();
                
            }
       
            
            return  _GridPts ;

        }

        private void SaveGridPoint(uopVector aGridPt,  bool bMirroring, uopLine aRowLine, uopLine aColLine, out bool rKeep, out bool rSuppressIt,out bool rStopProcessing, out string rInvalidationError, bool bAlt,double hStepFactor, double vStepFactor, bool? bRectangularBounds = null)
        {
            rStopProcessing = false;
             //check that the point is within bounds
             rKeep = ValidateVector(aGridPt, bMirroring, out rInvalidationError, bAlt, hStepFactor, vStepFactor, bRectangularBounds:bRectangularBounds);
            rSuppressIt = MaxCount <= 0 ? false : _PointCount + 1 > MaxCount;

            if (rKeep)
            {
                uopRectangle rec = null;
                if (RectangleCollector != null && RectangleToSave != null)
                    rec = new uopRectangle(RectangleToSave, aGridPt);
                    
                EventGridPointAdded?.Invoke(aGridPt, _GridPts, ref rKeep, ref rSuppressIt, ref rStopProcessing, aRowLine, aColLine, ref rec, false);
                if (rKeep && rec != null)
                {
                    rec.Rotation = aGridPt.Rotation;
                    rec.Suppressed = aGridPt.Suppressed;
                    rec.SetCenter(aGridPt);
                    RectangleCollector?.Add(rec);
                }

            }

            if (rKeep)
            {
                aGridPt.Suppressed = rSuppressIt;
                if (!rSuppressIt)
                {
                    _PointCount++;
                    aGridPt.Index = _PointCount;
                
                    _PointExtremes.Update(aGridPt.X, aGridPt.Y);
                    _GridPts.Add(aGridPt);
                }
                else
                {
                    _FreePointCount++;
                    aGridPt.Tag = rInvalidationError;
                    aGridPt.Index = _FreePointCount;
                    _GridPts.Add(aGridPt);
                }
                aRowLine.Points.Add(aGridPt);

            }
        }

        protected uopVectors _GridPts;
        public uopVectors GridPoints(bool? bSuppressVal = false, bool bGetClones = false, List<double> aValues = null, double? aUniformValue = null)
        {
            UpdateGridPoints();
            if (_GridPts == null) return uopVectors.Zero;
            uopVectors _rVal = _GridPts.Clone(true);
            if (bSuppressVal.HasValue)
            {
                _rVal = _GridPts.GetBySuppressed(bSuppressVal.Value, bReturnClones: bGetClones);
            }
            else
            {
                _rVal.Populate (_GridPts, bAddClone: bGetClones);
      
            }



            if (aValues != null || aUniformValue.HasValue)
            {
                for (int i = 1; i <= _rVal.Count; i++)
                {
                    uopVector u1 = _rVal[i - 1];
                    if (aValues != null)
                    {
                        if (i <= aValues.Count)
                            u1.Value = aValues[i - 1];
                    }

                    if (aUniformValue.HasValue) u1.Value = aUniformValue.Value;
                }
            }

            return _rVal;
        }

        public int ClearGridPointSuppression()
        {
            UpdateGridPoints();
            return _GridPts.SetSuppressed(null, false);

        }

        public string GridPointString(int aPrecis = 5)
        {
            string _rVal = string.Empty;
            aPrecis = mzUtils.LimitedValue(aPrecis, 1, 8);
            UpdateGridPoints();

            foreach (var item in _GridPts)
            {
                mzUtils.ListAdd(ref _rVal, $"({Math.Round(item.X, aPrecis)},{Math.Round(item.Y, aPrecis)},{Math.Round(item.Value, 1)},{Convert.ToInt32(item.Suppressed)})");
            }



            return _rVal;
        }

        public virtual uopVector SetGridPointSuppression(int aIndex, bool aSuppressionVal, bool bReturnClone = false) => aIndex < 1 || aIndex > _GridPts.Count ? null : SetGridPointSuppression (_GridPts[aIndex -1], aSuppressionVal);
      

        public virtual uopVector SetGridPointSuppression(uopVector aGridPoint, bool aSuppressionVal, bool bReturnClone = false)
        {
            UpdateGridPoints();
            if (_GridPts == null) return null;
            int aIndex = _GridPts.IndexOf(aGridPoint);
            if (aIndex < 0) return null;
            uopVector u1 = _GridPts[aIndex - 1];
            bool _rVal = _GridPts[aIndex - 1].Suppressed != aSuppressionVal;
            _GridPts[aIndex - 1].Suppressed = aSuppressionVal;

            if (this.RectangleCollector != null && aIndex > 0 && aIndex <= RectangleCollector.Count)
                RectangleCollector[aIndex - 1].SetCenter(_GridPts[aIndex - 1]);

            return bReturnClone ? new uopVector(_GridPts[aIndex - 1]) : _GridPts[aIndex - 1];
        }
        /// <summary>
        /// updates the current grid points if the need updating
        /// </summary>
        /// <param name="bRegen">flag to force a regen</param>
        /// <returns></returns>
        public virtual bool UpdateGridPoints(bool bRegen = false)
        {
            if ((bRegen || Invalid) && !_Generating) { Generate(); return true; }

            return false;
        }

        public uopVectors RowPoints(int aRowIndex, bool bSuppressVal = false, bool bGetClones = false)
        {
            UpdateGridPoints();
            if(aRowIndex <1 || aRowIndex > Rows.Count) return uopVectors.Zero;
            uopVectors allPoints = Rows.Item(aRowIndex).Points;
            List<uopVector> gridPts = allPoints.FindAll((x) => !x.Suppressed);
            return bSuppressVal ? new uopVectors(allPoints.FindAll((x) => x.Suppressed), bCloneMembers: bGetClones) : new uopVectors(gridPts, bCloneMembers: bGetClones);
        }

        public uopVectors ColumnPoints(int aColumnIndex, bool bSuppressVal = false, bool bGetClones = false)
        {
            UpdateGridPoints();
            if (aColumnIndex < 1 || aColumnIndex > _Cols.Count) return uopVectors.Zero;
            uopVectors allPoints = new uopVectors( _Rows.Points().FindAll((x) => x.Col == aColumnIndex), bCloneMembers: bGetClones);
            return bSuppressVal ? new uopVectors(allPoints.FindAll((x) => x.Suppressed), bCloneMembers: false) : new uopVectors(allPoints, bCloneMembers: false);
        }

        private void Create_RowsAndColumns()
        {
            _Extremes.Define(double.MaxValue / 2, double.MinValue / 2);
            Create_Columns();
            EventColumnLinesCreated?.Invoke(_Cols);
            Create_Rows();
            EventRowLinesCreated?.Invoke(_Rows);
        }

        /// <summary>
        /// get origin co-ordinats
        /// </summary>
        protected virtual void Create_Origin()
        {
            if (!_OverrideOrigin.HasValue)
            {
                uopRectangle limits = Vertices.BoundingRectangle;
                double X =HorizontalAlignment switch
                {
                    uppHorizontalAlignments.Left => limits.Left,
                    uppHorizontalAlignments.Right=> limits.Right,
                    _ => limits.X
                };
                double Y = VerticalAlignment switch
                {
                    uppVerticalAlignments.Top => limits.Top,
                    uppVerticalAlignments.Bottom => limits.Bottom,
                    _ => limits.Y
                };
                _Origin = new UVECTOR(X + XOffset, Y + YOffset);
            }
            else
            {
                _Origin = new UVECTOR(_OverrideOrigin.Value.X + XOffset, _OverrideOrigin.Value.Y + YOffset);

            }
            uopVector v1 = new uopVector(_Origin);
            EventOriginCreated?.Invoke( v1,this);
            _Origin.SetOrdinates(v1.X, v1.Y);


        }

        /// <summary>
        /// calculate grids row 
        /// </summary>
        protected virtual uopLines Create_Rows()
        {
            if (_Rows != null) _Rows.Clear(); else _Rows = new uopLines() { Name = "Rows" };

            URECTANGLE limits = BoundsV;
            double step = VPitch;
            int mx = MaxRows;

            if (PitchType == dxxPitchTypes.InvertedTriangular)
            {
                step *= 0.5;
                mx *= 2;
            }


            uopVector origin = new uopVector(_Origin);  
            double yord = _Origin.Y; //start at the current origin
            if (step > 0)
            {
                //step into range
                while (yord > limits.Top) { yord -= step; }
                while (yord < limits.Bottom) { yord += step; }
            }


            if (step == 0)
            {
                if (limits.ContainsOrd(yord, true, OnIsIn, aPrecis: 5))
                {
                    if (mx <= 0 ||  _Rows.Count + 1 <= mx)
                    {
                        //single row if there is no VPitch
                         _Rows.Add(new uopGridLine(true, limits.Left, yord, limits.Right, yord, aRow: _Rows.Count + 1, aGridOrigin: origin) { PitchType = PitchType, VPitch = VPitch, HPitch = HPitch });
                        _Rows.Add(new uopGridLine(true, limits.Left, yord, limits.Right, yord, aRow: _Rows.Count + 1, aGridOrigin: origin) { PitchType = PitchType, VPitch = VPitch, HPitch = HPitch });
                        _Extremes.Update(_Extremes.X, yord);
                    }
                }
            }
            else
            {
                if ( VerticalAlignment == uppVerticalAlignments.Top) // Alignment == uppGridAlignments.TopCenter || Alignment == uppGridAlignments.TopLeft || Alignment == uppGridAlignments.TopRight)
                {
                    //radiate down so start at the top
                    //this will prevent top side lines from being added
                    if (yord < limits.Top)
                    {
                        yord += (Math.Truncate((limits.Top - yord) / step) + 2) * step;
                    }
                }
                else if (VerticalAlignment == uppVerticalAlignments.Bottom) // Alignment == uppGridAlignments.BottomLeft || Alignment == uppGridAlignments.BottomCenter || Alignment == uppGridAlignments.BottomRight)
                {
                    //radiate up so start at the bottom
                    //this will prevent bottom side lines from being added
                    if (yord > limits.Bottom)
                    {
                        yord -= (Math.Truncate((yord - limits.Bottom) / step) + 2) * step;
                    }
                }
                double y1 = yord;
                double y2 = yord;
                while (!(y1 > limits.Top && y2 < limits.Bottom))
                {

                    if (mx > 0 && _Rows.Count + 1 > mx) break;

                    if (!ReverseRowGeneration)
                    {
                        if (limits.ContainsOrd(y1, true, OnIsIn, aPrecis: 5))
                        {
                            //add a top side line
                            _Rows.Add(new uopGridLine(true, limits.Left, y1, limits.Right, y1, aRow: _Rows.Count + 1, aGridOrigin: origin) { PitchType = PitchType, VPitch = VPitch, HPitch = HPitch });
                         
                            _Extremes.Update(_Extremes.X, y1);
                            if (mx > 0 && _Rows.Count + 1 > mx) break;
                        }
                        if (y1 != y2)
                        {
                            if (limits.ContainsOrd(y2, true, OnIsIn, aPrecis: 5))
                            {
                                //add a bottom side line
                                _Rows.Add(new uopGridLine(true,   limits.Left, y2, limits.Right, y2, aRow: _Rows.Count + 1, aGridOrigin: origin) { PitchType = PitchType, VPitch = VPitch, HPitch = HPitch });
                     
                                _Extremes.Update(_Extremes.X, y2);
                                if (mx > 0 && _Rows.Count + 1 > mx) break;
                            }
                        }
                    }
                    else
                    {
                        if (limits.ContainsOrd(y2, true, OnIsIn, aPrecis: 5))
                        {
                            //add abottom side line
                                _Rows.Add(new uopGridLine(true, limits.Left, y2, limits.Right, y2, aRow: _Rows.Count + 1, aGridOrigin: origin) { PitchType = PitchType, VPitch = VPitch, HPitch = HPitch });
                        
                                _Extremes.Update(_Extremes.X, y2);
                                if (mx > 0 && _Rows.Count + 1 > mx) break;
                        }
                        if (y1 != y2)
                        {
                            if (limits.ContainsOrd(y1, true, OnIsIn, aPrecis: 5))
                            {
                                //add a top side line
                                _Rows.Add(new uopGridLine(true, limits.Left, y1, limits.Right, y1, aRow: _Rows.Count + 1, aGridOrigin: origin) { PitchType = PitchType, VPitch = VPitch, HPitch = HPitch });
                             
                                _Extremes.Update(_Extremes.X, y1);
                                if (mx > 0 && _Rows.Count + 1 > mx) break;
                            }
                        }
                    }
                    y1 += step; //step up
                    y2 -= step; //step down
                }
            }

            if (TrimRowLinesToBounds && !IsRectangular())
            {
                uopSegments bounds = Segments;
                foreach (var rowline in _Rows)
                {

                    uopVectors ips = rowline.Intersections(bounds, false, false, true, 4);
                    if (ips.Count == 2)
                    {
                        uopVector ip1 = ips.Nearest(rowline.sp);
                        uopVector ip2 = ips.Nearest(rowline.ep);
                        rowline.sp.SetCoordinates(ip1.X, ip1.Y);
                        rowline.ep.SetCoordinates(ip2.X, ip2.Y);
                    }

                }
            }
            return _Rows;

        }


        
        /// <summary>
        ///create the grid column lines
        /// </summary>
 
        protected virtual uopLines Create_Columns()
        {
            if (_Cols != null) _Cols.Clear(); else _Cols = new uopLines() { Name = "Columns" };
            URECTANGLE aLims = BoundsV; //get the limits to know when we exceed them on the left or right

            double step = HPitch;

            int mx = MaxCols;

            //init array and extremes rectangle left and right

            uopVector origin = new uopVector(_Origin);
            double xord = _Origin.X; //start at the current origin X
                                 //step into the bounds to start
            if (step > 0)
            {
                if (mx <= 0)
                {
                    mx = (int)Math.Truncate(aLims.Width / step) + 1;
                }
                if (PitchType == dxxPitchTypes.Triangular)
                {
                    //step is half for triangular pitchs
                    step *= 0.5;
                    mx *= 2;
                }
                while (xord > aLims.Right) { xord -= step; }
                while (xord < aLims.Left) { xord += step; }
            }

            if (step == 0)
            {
                if (aLims.ContainsOrd(xord, false, OnIsIn, aPrecis: 5))
                {
                    //one column cause there is no HPitch
                    if (mx <= 0 || _Cols.Count + 1 <= mx)
                    {
                        _Cols.Add(new uopGridLine(false,xord, aLims.Top, xord, aLims.Bottom, aCol: _Cols.Count + 1, aGridOrigin: origin) { PitchType = PitchType, VPitch = VPitch, HPitch = HPitch });
                        _Extremes.Update(xord, _Extremes.Y);
                    }
                }
            }
            else
            {
                if (HorizontalAlignment == uppHorizontalAlignments.Right)
                {
                    //radiate to the left so start at the far right
                    //this will prevent right side lines from being added
                    if (xord < aLims.Right) xord += (Math.Truncate((aLims.Right - xord) / step) + 2) * step;
                }
                else if (HorizontalAlignment == uppHorizontalAlignments.Left) 
                {
                    //radiate to the right so start at the far left
                    //this will prevent left side lines from being added
                    if (xord > aLims.Left) xord -= (Math.Truncate((xord - aLims.Left) / step) + 2) * step;
                }

                double x1 = xord;
                double x2 = xord;
                while (!(x1 < aLims.Left && x2 > aLims.Right))
                {
                    //flip flop back and forth around the origin until we exceed the limits on both sides
                    if (mx >= 0 && _Cols.Count + 1 > mx) break;
                    if (!ReverseColumnGeneration)
                    {
                        if (aLims.ContainsOrd(x1, false, OnIsIn, aPrecis: 5))
                        {
                            //add a left side line
                            _Cols.Add(new uopGridLine(false,x1, aLims.Top, x1, aLims.Bottom, aCol: _Cols.Count + 1, aGridOrigin: origin) { PitchType = PitchType, VPitch = VPitch, HPitch = HPitch });
                            _Extremes.Update(x1, _Extremes.Y);
                            if (mx >= 0 && _Cols.Count + 1 > mx) break;
                        }
                        if (x1 != x2)
                        {
                            if (aLims.ContainsOrd(x2, false, OnIsIn, aPrecis: 5))
                            {
                                //add a right side line
                                _Cols.Add(new uopGridLine(false,x2, aLims.Top, x2, aLims.Bottom, aCol: _Cols.Count + 1, aGridOrigin: origin) { PitchType = PitchType, VPitch = VPitch, HPitch = HPitch });
                                _Extremes.Update(x2, _Extremes.Y);
                                if (mx >= 0 && _Cols.Count + 1 > mx) break;
                            }
                        }
                    }
                    else
                    {
                        if (aLims.ContainsOrd(x2, false, OnIsIn, aPrecis: 5))
                        {
                            //add a right side line
                            _Cols.Add(new uopGridLine(false,x2, aLims.Top, x2, aLims.Bottom, aCol: _Cols.Count + 1, aGridOrigin: origin) { PitchType = PitchType, VPitch = VPitch, HPitch = HPitch });
                            _Extremes.Update(x2, _Extremes.Y);
                           if (mx >= 0 && _Cols.Count + 1 > mx) break;
                            if (x1 != x2)
                            {
                                if (aLims.ContainsOrd(x1, false, OnIsIn, aPrecis: 5))
                                {
                                    //add a left side line
                                    _Cols.Add(new uopGridLine(false,x1, aLims.Top, x1, aLims.Bottom, aCol: _Cols.Count + 1, aGridOrigin: origin) { PitchType = PitchType, VPitch = VPitch, HPitch = HPitch });
                                    _Extremes.Update(x1, _Extremes.Y);
                                    if (mx >= 0 && _Cols.Count + 1 > mx) break;
                                }
                            }
                        }
                    }
                    x1 -= step; //step to the left
                    x2 += step; //step to the right
                }
            }

            if (TrimColumnLinesToBounds && !IsRectangular())
            {
                uopSegments bounds = Segments;
                foreach (var colline in _Cols)
                {

                    uopVectors ips = colline.Intersections(bounds, false, false, true, 4);
                    if (ips.Count == 2)
                    {
                        uopVector ip1 = ips.Nearest(colline.sp);
                        uopVector ip2 = ips.Nearest(colline.ep);
                        colline.sp.SetCoordinates(ip1.X, ip1.Y);
                        colline.ep.SetCoordinates(ip2.X, ip2.Y);
                    }

                }
            }
            


            return _Cols;
        }


       

        /// <summary>
        /// returns the row lines used to generte the grid points
        /// </summary>
        /// <returns></returns>
        public uopLines RowLines(bool bPopulatedOnly = false)
        {
            UpdateGridPoints();
            uopLines _rVal = new uopLines();
            if (_Rows == null) return _rVal;
            uopLines rows = bPopulatedOnly ? _Rows.GetByPoints((x) => !x.Suppressed) :  _Rows;
           
            for (int i = 1; i <= rows.Count; i++)
            {
                _rVal.Add(new uopLine(rows.Item(i)) { Row = i } );
            }
            return _rVal;
        }

        /// <summary>
        /// returns the column lines used to generte the grid points
        /// </summary>
        /// <returns></returns>
        public uopLines ColumnLines()
        {
            UpdateGridPoints();
            uopLines _rVal = new uopLines();
            if (_Cols== null || _Cols.Count <= 0) return _rVal;

            for (int i = 1; i <= _Cols.Count; i++)
            {
                _rVal.Add(new uopLine(_Cols.Item(i)) { Col = i });
            }
            return _rVal;
        }

 



        #endregion Methods

        #region Shared Methods

        internal static int GridRowCount(uopGrid aGrid, int aRowID, bool bReportFreePts, out int rFilledCount, out int rFreeCount, out bool rRowExists, out double rRowY)
        {
            
            rFreeCount = 0;
            rFilledCount = 0;
            rRowExists = false;
            rRowY = 0;
            if (aGrid == null) return 0;

            rRowExists = aRowID > 0 & aRowID <= aGrid.Rows.Count;
            rRowY = aGrid.Center.Y;
            if (!rRowExists)  return 0;
            uopLine aRow = aGrid.Rows[aRowID -1] ;
            rRowY = aRow.Y();
            rFreeCount = aRow.Points.FindAll((x) => !x.Suppressed).Count ;
            rFilledCount = aRow.Points.Count - rFreeCount ;
            return bReportFreePts ? rFreeCount : rFilledCount  ;

        }

        #endregion Shared Methods
    }
}