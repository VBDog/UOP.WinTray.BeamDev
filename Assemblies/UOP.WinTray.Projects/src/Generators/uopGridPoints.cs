
using UOP.DXFGraphics;
using System;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using System.Collections.Generic;

namespace UOP.WinTray.Projects.Generators
{
    class uopGridPoints
    {

    }

    internal struct UGRID : ICloneable
    {

        #region Fields

        public USHAPE Boundary;
        public UVECTOR Origin;
        public ULINES Cols;
        public ULINES Rows;
        public URECTANGLE Extremes;
        public URECTANGLE PointExtremes;
        public URECTANGLE PointLimits;
        public UARCRECS Islands { get; set; }
        public uppGridAlignments Alignment;
        public dxxPitchTypes PitchType;
        public bool OnIsIn;
        public bool RectangularBounds;
        public bool ReverseRowGeneration;
        public bool ReverseColumnGeneration;
        public string PartialRowLocation;
        public string Tag;
        public double VPitch;
        public double HPitch;
        public double XOffset;
        public double YOffset;
        public int MaxRows;
        public int MaxCols;
        public int MaxCount;
        public int RowCount;
        public int ColCount;
        public int PointCount;
        public int FreePointCount;
        public double? MaxX;
        public double? MinX;
        public ULINE? MirrorLine;

        #endregion Fields

        #region Constructors

        public UGRID(dxxPitchTypes aPType = dxxPitchTypes.Triangular, uppGridAlignments aAlignment = uppGridAlignments.Undefined, string aTag = "")
        {
            Boundary = new USHAPE();
            Origin = UVECTOR.Zero;
            Cols = new ULINES();
            Rows = new ULINES();
            Extremes = URECTANGLE.Null;
            PointExtremes = URECTANGLE.Null;
            PointLimits = URECTANGLE.Null;
            Islands = new UARCRECS();
            Alignment = aAlignment;
            PitchType = aPType;
            OnIsIn = false;
            RectangularBounds = false;
            ReverseRowGeneration = false;
            ReverseColumnGeneration = false;
            PartialRowLocation = string.Empty;
            Tag = aTag;
            VPitch = 0;
            HPitch = 0;
            XOffset = 0;
            YOffset = 0;
            MaxRows = 0;
            MaxCols = 0;
            MaxCount = 0;
            RowCount = 0;
            ColCount = 0;
            PointCount = 0;
            FreePointCount = 0;
            MaxX = null;
            MinX = null;
            MirrorLine = null;
            PointLimits.Define(double.MinValue / 2, double.MaxValue / 2, double.MaxValue / 2, double.MinValue / 2);

        }

        public UGRID(UGRID aGrid)
        {
            Boundary = new USHAPE(aGrid.Boundary);
            Origin = new UVECTOR(aGrid.Origin);
            Cols = new ULINES(aGrid.Cols);
            Rows = new ULINES(aGrid.Rows);
            Extremes = new URECTANGLE(aGrid.Extremes);
            PointExtremes = new URECTANGLE(aGrid.PointExtremes);
            PointLimits = new URECTANGLE(aGrid.PointLimits);
            Islands = new UARCRECS(aGrid.Islands);
            Alignment = aGrid.Alignment;
            PitchType = aGrid.PitchType;
            OnIsIn = aGrid.OnIsIn;
            RectangularBounds = aGrid.RectangularBounds;
            ReverseRowGeneration = aGrid.ReverseRowGeneration;
            ReverseColumnGeneration = aGrid.ReverseColumnGeneration;
            PartialRowLocation = aGrid.PartialRowLocation;
            Tag = aGrid.Tag;
            VPitch = aGrid.VPitch;
            HPitch = aGrid.HPitch;
            XOffset = aGrid.XOffset;
            YOffset = aGrid.YOffset;
            MaxRows = aGrid.MaxCols;
            MaxCols = aGrid.MaxCols;
            MaxCount = aGrid.MaxCount;
            RowCount = aGrid.RowCount;
            ColCount = aGrid.ColCount;
            PointCount = aGrid.PointCount;
            FreePointCount = aGrid.FreePointCount;
            MaxX = aGrid.MaxX;
            MinX = aGrid.MinX;
            MirrorLine = null;
            if (aGrid.MirrorLine.HasValue) MirrorLine = new ULINE(aGrid.MirrorLine.Value);


        }

        public UGRID(uopGrid aGrid)
        {

            //================ INIT
            Boundary = new USHAPE();
            Origin = UVECTOR.Zero;
            Cols = new ULINES();
            Rows = new ULINES();
            Extremes = URECTANGLE.Null;
            PointExtremes = URECTANGLE.Null;
            PointLimits = URECTANGLE.Null;
            Islands = new UARCRECS();
            Alignment = uppGridAlignments.Undefined;
            PitchType = dxxPitchTypes.Rectangular;
            OnIsIn = false;
            RectangularBounds = false;
            ReverseRowGeneration = false;
            ReverseColumnGeneration = false;
            PartialRowLocation = string.Empty;
            Tag = string.Empty;
            VPitch = 0;
            HPitch = 0;
            XOffset = 0;
            YOffset = 0;
            MaxRows = 0;
            MaxCols = 0;
            MaxCount = 0;
            RowCount = 0;
            ColCount = 0;
            PointCount = 0;
            FreePointCount = 0;
            MaxX = null;
            MinX = null;
            MirrorLine = null;
            PointLimits.Define(double.MinValue / 2, double.MaxValue / 2, double.MaxValue / 2, double.MinValue / 2);

            if (aGrid == null) return;

            Boundary = new USHAPE(aGrid);
            Origin = new UVECTOR(aGrid.Origin);
            Cols = new ULINES(aGrid.ColumnLines());
            Rows = new ULINES(aGrid.RowLines());
            Extremes = new URECTANGLE(aGrid._Extremes);
            PointExtremes = new URECTANGLE(aGrid._PointExtremes);
            PointLimits = new URECTANGLE(aGrid._PointLimits);
            Islands = new UARCRECS(aGrid.Islands);
            Alignment = aGrid.Alignment;
            PitchType = aGrid.PitchType;
            OnIsIn = aGrid.OnIsIn;
            RectangularBounds = aGrid.IsRectangular();
            ReverseRowGeneration = aGrid.ReverseRowGeneration;
            ReverseColumnGeneration = aGrid.ReverseColumnGeneration;
            PartialRowLocation = aGrid.PartialRowLocation;
            Tag = aGrid.Tag;
            VPitch = aGrid.VPitch;
            HPitch = aGrid.HPitch;
            XOffset = aGrid.XOffset;
            YOffset = aGrid.YOffset;
            MaxRows = aGrid.MaxCols;
            MaxCols = aGrid.MaxCols;
            MaxCount = aGrid.MaxCount;
            RowCount = aGrid.RowCount();
            ColCount = aGrid.ColCount();
            PointCount = aGrid.PointCount;
            FreePointCount = aGrid.FreePointCount;
            MaxX = aGrid.MaxX;
            MinX = aGrid.MinX;
            if (aGrid.MirrorLine != null) MirrorLine = new ULINE(aGrid.MirrorLine);


        }

        #endregion Constructors

        public readonly UGRID Clone() => new UGRID(this);


        object ICloneable.Clone() => (object)Clone();

        public readonly string VerticalAlignment
        {
            get
            {
                if (Alignment == uppGridAlignments.TopLeft || Alignment == uppGridAlignments.TopCenter || Alignment == uppGridAlignments.TopRight) return "TOP";
                if (Alignment == uppGridAlignments.MiddleLeft || Alignment == uppGridAlignments.MiddleCenter || Alignment == uppGridAlignments.MiddleRight) return "MIDDLE";
                if (Alignment == uppGridAlignments.BottomLeft || Alignment == uppGridAlignments.BottomCenter || Alignment == uppGridAlignments.BottomRight) return "BOTTOM";

                return "UNDEFINED";
            }
        }

        public readonly string HorizontalAlignment
        {
            get
            {
                if (Alignment == uppGridAlignments.TopLeft || Alignment == uppGridAlignments.MiddleLeft || Alignment == uppGridAlignments.BottomLeft) return "LEFT";
                if (Alignment == uppGridAlignments.TopCenter || Alignment == uppGridAlignments.MiddleCenter || Alignment == uppGridAlignments.BottomCenter) return "CENTER";
                if (Alignment == uppGridAlignments.TopRight || Alignment == uppGridAlignments.MiddleRight || Alignment == uppGridAlignments.BottomRight) return "RIGHT";

                return "UNDEFINED";
            }
        }


        public void Mirror(double? aX, double? aY)
        {
            if (!aX.HasValue && !aY.HasValue) return;
            if (MirrorLine.HasValue) MirrorLine.Value.Mirror(aX, aY);
            Islands.Mirror(aX, aY);
            PointLimits.Mirror(aX, aY);
            PointExtremes.Mirror(aX, aY);
            Extremes.Mirror(aX, aY);
            Rows.Mirror(aX, aY);
            Cols.Mirror(aX, aY);
            Origin.Mirror(aX, aY);
            if (MaxX.HasValue && aX.HasValue)
            {
                double dx = MaxX.Value - aX.Value;
                MaxX = MaxX.Value - 2 * dx;
            }
        }


        public void Clear()
        {
            Rows = new ULINES("ROWS");
            Cols = new ULINES("COLS");
            RowCount = 0;
            ColCount = 0;
            PointCount = 0;
            FreePointCount = 0;
        }

        /// <summary>
        /// method for generating grids
        /// </summary>
        public void Generate()
        {
            Clear();
            PartialRowLocation = string.IsNullOrWhiteSpace(PartialRowLocation) ? string.Empty : PartialRowLocation.ToUpper().Trim();
            if (!Boundary.IsDefined) return;
            RectangularBounds = Boundary.IsRectangular(false);
            if (Boundary.Limits.Area <= 0) return;
            xOrigin();
            xColumnsAndRows();
            PointExtremes = xPoints();
            if (PartialRowLocation !=  string.Empty) xLocatePartialRow();

        }

        /// <summary>
        /// method for generating grids
        /// </summary>
        public void Generate(out uopVectors rGridPoints)
        {
            rGridPoints = new uopVectors();
            Clear();
            PartialRowLocation = string.IsNullOrWhiteSpace(PartialRowLocation) ? string.Empty : PartialRowLocation.ToUpper().Trim();
            if (!Boundary.IsDefined) return;
            RectangularBounds = Boundary.IsRectangular(false);
            if (Boundary.Limits.Area <= 0) return;
            xOrigin();
            xColumnsAndRows();
            PointExtremes = xPoints(rGridPoints);
            if (!string.IsNullOrWhiteSpace(PartialRowLocation))
            {
                xLocatePartialRow();
                rGridPoints = new uopVectors(GetGridPoints());
            }


        }


        /// <summary>
        /// Re -generate grids
        /// </summary>
        /// <param name="aXOffset"></param>
        /// <param name="aYOffset"></param>
        public void ReGenerate(double aXOffset = 0, double aYOffset = 0)
        {
            if (aXOffset == 0 & aYOffset == 0) { return; }


            PointCount = 0;
            FreePointCount = 0;
            RowCount = 0;
            ColCount = 0;

            if (Boundary.Segments.Count <= 0) Boundary.UpdateSegments();
            if (Boundary.Segments.Count <= 0)
            { return; }
            RectangularBounds = Boundary.IsRectangular(false);

            if (Boundary.Width <= 0 || Boundary.Height <= 0)
            { return; }

            Origin.X += aXOffset;
            Origin.Y += aYOffset;

            //xOrigin
            xColumnsAndRows();

            PointExtremes = xPoints();
            if (PartialRowLocation !=  string.Empty) { xLocatePartialRow(); }
        }

        /// <summary>
        /// method for translating y co-ordinate
        /// </summary>
        /// <param name="aGrid"></param>
        /// <param name="aOffset"></param>
        /// <param name="bJustMove"></param>
        /// <returns></returns>
        public void Translate(double aXOffSet, double aYOffset, bool bJustMove = false)
        {
            if (aXOffSet == 0 && aYOffset == 0) { return; }


            if (!bJustMove)
            { ReGenerate(aXOffSet, aYOffset); }
            else
            {

                PointExtremes.Translate(aXOffSet, aYOffset);
                Extremes.Translate(aXOffSet, aYOffset);
                PointLimits.Translate(aXOffSet, aYOffset);
                Origin.Translate(aXOffSet, aYOffset);
                Rows.Move(aXOffSet, aYOffset);
                Cols.Move(aXOffSet, aYOffset);
            }
        }
        private void xColumnsAndRows()
        {
            Extremes.Define(double.MaxValue / 2, double.MinValue / 2);
            xColumns();
            xRows();
        }

        /// <summary>
        /// get origin co-ordinats
        /// </summary>
        /// <param name="aGrid"></param>
        private void xOrigin()
        {
            double X = HorizontalAlignment.ToUpper() switch
            {
                "LEFT" => Boundary.Limits.Left,
                "RIGHT" => Boundary.Limits.Right,
                _ => Boundary.Limits.X
            };
            double Y = VerticalAlignment.ToUpper() switch
            {
                "TOP" => Boundary.Limits.Top,
                "BOTTOM" => Boundary.Limits.Bottom,
                _ => Boundary.Limits.Y
            };


            Origin = new UVECTOR(X + XOffset, Y + YOffset);
        }

        /// <summary>
        /// calculate grids row 
        /// </summary>
        /// <param name="aGrid"></param>
        private void xRows()
        {
            URECTANGLE aLims = Boundary.Limits;


            double step = VPitch;
            int mx = MaxRows;

            if (PitchType == dxxPitchTypes.InvertedTriangular)
            {
                step *= 0.5;
                mx *= 2;
            }

            Rows = ULINES.Null;
            double Y = Origin.Y; //start at the current origin
            if (step > 0)
            {
                while (Y > aLims.Top) { Y -= step; }
                while (Y < aLims.Bottom) { Y += step; }
            }
            if (step == 0)
            {
                if (aLims.ContainsOrd(Y, true, OnIsIn, aPrecis: 5))
                {
                    if (mx <= 0 || Rows.Count + 1 <= mx)
                    {
                        //single row if there is no VPitch
                        Rows.Add(aLims.Left, Y, aLims.Right, Y, Rows.Count + 1);
                        Extremes.Update(Extremes.X, Y);
                    }
                }
            }
            else
            {
                if (VerticalAlignment == "TOP") // Alignment == uppGridAlignments.TopCenter || Alignment == uppGridAlignments.TopLeft || Alignment == uppGridAlignments.TopRight)
                {
                    //radiate down so start at the top
                    //this will prevent top side lines from being added
                    if (Y < aLims.Top)
                    {
                        Y += (Math.Truncate((aLims.Top - Y) / step) + 2) * step;
                    }
                }
                else if (VerticalAlignment == "BOTTOM") // Alignment == uppGridAlignments.BottomLeft || Alignment == uppGridAlignments.BottomCenter || Alignment == uppGridAlignments.BottomRight)
                {
                    //radiate up so start at the bottom
                    //this will prevent bottom side lines from being added
                    if (Y > aLims.Bottom)
                    {
                        Y -= (Math.Truncate((Y - aLims.Bottom) / step) + 2) * step;
                    }
                }
                double y1 = Y;
                double y2 = Y;
                while (!(y1 > aLims.Top && y2 < aLims.Bottom))
                {
                    if (!ReverseRowGeneration)
                    {
                        if (aLims.ContainsOrd(y1, true, OnIsIn, aPrecis: 5))
                        {
                            //add a top side line
                            if (mx <= 0 || Rows.Count + 1 <= mx)
                            {
                                Rows.Add(aLims.Left, y1, aLims.Right, y1, Rows.Count + 1);
                                Extremes.Update(Extremes.X, y1);
                            }
                        }
                        if (y1 != y2)
                        {
                            if (aLims.ContainsOrd(y2, true, OnIsIn, aPrecis: 5))
                            {
                                //add a bottom side line
                                if (mx <= 0 || Rows.Count + 1 <= mx)
                                {
                                    Rows.Add(aLims.Left, y2, aLims.Right, y2, Rows.Count + 1);
                                    Extremes.Update(Extremes.X, y2);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (aLims.ContainsOrd(y2, true, OnIsIn, aPrecis: 5))
                        {
                            //add abottom side line
                            if (mx <= 0 || Rows.Count + 1 <= mx)
                            {
                                Rows.Add(aLims.Left, y2, aLims.Right, y2, Rows.Count + 1);
                                Extremes.Update(Extremes.X, y2);
                            }
                        }
                        if (y1 != y2)
                        {
                            if (aLims.ContainsOrd(y1, true, OnIsIn, aPrecis: 5))
                            {
                                //add a top side line
                                if (mx <= 0 || Rows.Count + 1 <= mx)
                                {
                                    Rows.Add(aLims.Left, y1, aLims.Right, y1, Rows.Count + 1);
                                    Extremes.Update(Extremes.X, y1);
                                }
                            }
                        }
                    }
                    y1 += step; //step up
                    y2 -= step; //step down
                }
            }
        }

        /// <summary>
        ///create the grid column lines
        /// </summary>
        /// <param name="aGrid"></param>
        private void xColumns()
        {

            double step = HPitch;
            int mx = MaxCols;

            if (PitchType == dxxPitchTypes.Triangular)
            {
                //step is half for triangular pitchs
                step *= 0.5;
                mx *= 2;
            }
            //init array and extremes rectangle left and right
            Cols = new ULINES("COLS");

            URECTANGLE aLims = new URECTANGLE(Boundary.Limits) { Name = "" }; //get the limits to know when we exceed them on the left or right
            double X = Origin.X; //start at the current origin X
                                 //step into the bounds to start
            if (step > 0)
            {
                while (X > aLims.Right) { X -= step; }
                while (X < aLims.Left) { X += step; }
            }

            ULINE l1;

            if (step == 0)
            {
                if (aLims.ContainsOrd(X, false, OnIsIn, aPrecis: 5))
                {
                    //one column cause there is no HPitch
                    if (mx <= 0 || Cols.Count + 1 <= mx)
                    {
                        l1 = Cols.Add(X, aLims.Top, X, aLims.Bottom, aCol: Cols.Count + 1);
                        Extremes.Update(X, Extremes.Y);
                    }
                }
            }
            else
            {
                if (HorizontalAlignment == "RIGHT") //  Alignment == uppGridAlignments.TopRight || Alignment == uppGridAlignments.MiddleRight || Alignment == uppGridAlignments.BottomRight)
                {
                    //radiate to the left so start at the far right
                    //this will prevent right side lines from being added
                    if (X < aLims.Right) X += (Math.Truncate((aLims.Right - X) / step) + 2) * step;
                }
                else if (HorizontalAlignment == "LEFT") // Alignment == uppGridAlignments.TopLeft || Alignment == uppGridAlignments.MiddleLeft || Alignment == uppGridAlignments.BottomLeft)
                {
                    //radiate to the right so start at the far left
                    //this will prevent left side lines from being added
                    if (X > aLims.Left) X -= (Math.Truncate((X - aLims.Left) / step) + 2) * step;
                }

                double x1 = X;
                double x2 = X;
                while (!(x1 < aLims.Left && x2 > aLims.Right))
                {
                    //flip flop back and forth around the origin until we exceed the limits on both sides

                    if (!ReverseColumnGeneration)
                    {
                        if (aLims.ContainsOrd(x1, false, OnIsIn, aPrecis: 5))
                        {
                            //add a left side line
                            if (mx <= 0 || Cols.Count + 1 <= mx)
                            {
                                l1 = Cols.Add(x1, aLims.Top, x1, aLims.Bottom, aCol: Cols.Count + 1);
                                Extremes.Update(x1, Extremes.Y);
                            }
                        }
                        if (x1 != x2)
                        {
                            if (aLims.ContainsOrd(x2, false, OnIsIn, aPrecis: 5))
                            {
                                //add a right side line
                                if (mx <= 0 || Cols.Count + 1 <= mx)
                                {
                                    Cols.Add(x2, aLims.Top, x2, aLims.Bottom, aCol: Cols.Count + 1);
                                    Extremes.Update(x2, Extremes.Y);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (aLims.ContainsOrd(x2, false, OnIsIn, aPrecis: 5))
                        {
                            //add a right side line
                            if (mx <= 0 || Cols.Count + 1 <= mx)
                            {
                                l1 = Cols.Add(x2, aLims.Top, x2, aLims.Bottom, aCol: Cols.Count + 1);
                                Extremes.Update(x2, Extremes.Y);
                            }
                        }
                        if (x1 != x2)
                        {
                            if (aLims.ContainsOrd(x1, false, OnIsIn, aPrecis: 5))
                            {
                                //add a left side line
                                if (mx <= 0 || Cols.Count + 1 <= mx)
                                {
                                    l1 = Cols.Add(x1, aLims.Top, x1, aLims.Bottom, aCol: Cols.Count + 1);
                                    Extremes.Update(x1, Extremes.Y);
                                }
                            }
                        }
                    }
                    x1 -= step; //step to the left
                    x2 += step; //step to the right
                }
            }
        }

        private readonly double HStepFactor => PitchType == dxxPitchTypes.InvertedTriangular ? 1.0 : 0.5;
        private readonly double VStepFactor => PitchType == dxxPitchTypes.InvertedTriangular ? 0.5 : 1.0;

        public readonly bool TriangularPitch => PitchType == dxxPitchTypes.Triangular || PitchType == dxxPitchTypes.InvertedTriangular;

        /// <summary>
        /// calucaltes grid points
        /// </summary>
        /// <param name="aGrid"></param>
        /// <returns></returns>
        private URECTANGLE xPoints(uopVectors rGridPoints = null)
        {
            URECTANGLE _rVal = URECTANGLE.Null;
            USEGMENTS aSegs = Boundary.Segments;
            UVECTOR u0 = Origin;
            double f1 = HStepFactor;
            double f2 = VStepFactor;


            if (aSegs.Count <= 0)
            {
                Boundary.Update();
                aSegs = Boundary.Segments;
            }

            PointLimits = Boundary.Limits;
            RowCount = 0;
            ColCount = 0;
            FreePointCount = 0;
            PointCount = 0;
            _rVal.Define(Boundary.Center.X, Boundary.Center.X, Boundary.Center.Y, Boundary.Center.Y);
            if (aSegs.Count <= 0) return _rVal;
            uopSegments boundarysegs = new uopSegments(aSegs);
            bool mirroring = MirrorLine.HasValue;
            ULINE mirrline = !mirroring ? new ULINE() : MirrorLine.Value;
            if (mirroring && mirrline.Length <= 0) mirroring = false;

            //_rVal.Define(u1.X, u1.X, System.Double.MaxValue/2, System.Double.MinValue/2);
            bool bAlt = TriangularPitch;

            for (int i = 1; i <= Rows.Count; i++)
            {
                ULINE rLn = Rows.Item(i);
                rLn.Points = UVECTORS.Zero;
                for (int j = 1; j <= Cols.Count; j++)
                {
                    ULINE cLn = Cols.Item(j);
                    UVECTOR u1 = new UVECTOR(cLn.sp.X, rLn.sp.Y) { Row = i, Col = j };

                    //check that the point is within bounds
                    bool keep = xValidateVector(ref u1, u0, boundarysegs, bAlt, f1, f2);

                    if (keep)
                    {

                        if (MaxCount <= 0 || PointCount + 1 <= MaxCount)
                        {
                            PointCount += 1;
                            u1.Index = PointCount;
                            u1.Suppressed = false;
                            _rVal.Update(u1);
                            if (u1.Row > RowCount) RowCount = u1.Row;
                            if (u1.Col > ColCount) ColCount = u1.Col;
                        }
                        else
                        {
                            u1.Suppressed = true;
                            FreePointCount++;
                            u1.Index = FreePointCount;
                        }
                        rLn.Points.Add(u1);


                        if (mirroring)
                        {
                            UVECTOR u2 = u1.Clone();
                            u2.ProjectTo(mirrline, out UVECTOR dir, out bool ison, out bool dpos, out double d1);
                            if (d1 == 0) continue;
                            u2 += dir * d1;

                            if (xValidateVector(ref u2, u0, boundarysegs, false, f1, f2, true))
                            {
                                if (MaxCount <= 0 || PointCount + 1 <= MaxCount)
                                {
                                    PointCount += 1;
                                    u2.Index = PointCount;
                                    u2.Suppressed = false;
                                    _rVal.Update(u2);
                                    if (u2.Row > RowCount) RowCount = u2.Row;
                                    if (u2.Col > ColCount) ColCount = u2.Col;
                                }
                                else
                                {
                                    u2.Suppressed = true;
                                    FreePointCount++;
                                    u2.Index = FreePointCount;
                                }
                                rLn.Points.Add(u2);

                            }

                        }
                    }
                    else
                    {
                        //if (!string.IsNullOrEmpty(u1.Tag))
                        //{
                        //    if(string.Compare(u1.Tag, "EXTERIOR TO BOUNDARY",true) == 0)
                        //    {
                        //        System.Console.WriteLine($"{u1.Row},{u1.Col}-{u1.Tag} - {u1}");
                        //    }

                        //}

                    }


                } // loop on column lines

                if (rGridPoints != null) rGridPoints.Append(rLn.Points);
                Rows.SetItem(i, rLn);
            }// loop on row lines
            if (PointCount <= 0) { _rVal.Define(Boundary.X, Boundary.X, Boundary.Y, Boundary.Y); }
            return _rVal;

        }


        private bool xValidateVector(ref UVECTOR aVector, UVECTOR u0, USEGMENTS aSegs, bool bAlt, double f1, double f2, bool bIgnoreMaxes = false)
        {
            aVector.Tag = string.Empty;
            //gross check that the point is within bounds
            bool _rVal = PointLimits.ContainsOrd(aVector.X, bOrdIsY: false, bOnIsIn: OnIsIn, aPrecis: 5) && PointLimits.ContainsOrd(aVector.Y, bOrdIsY: true, bOnIsIn: OnIsIn, aPrecis: 5);
            if (!_rVal)
            {
                aVector.Tag = "POINT LIMITS";
                return _rVal;
            }

            if (!bIgnoreMaxes)
            {
                if (_rVal && MaxX.HasValue)
                {
                    if (aVector.X > MaxX.Value)
                    {
                        _rVal = false;
                        aVector.Tag = "MAXX VIOLATION";
                        return _rVal;
                    }
                }

            }

            if (bAlt && _rVal)
            {
                double dX = aVector.X - u0.X;
                double dY = aVector.Y - u0.Y;
                double xsteps = Math.Round(dX / (f1 * HPitch), 0) + 1;
                double ysteps = Math.Round(dY / (f2 * VPitch), 0) + 1;
                bool altskip = false;
                if (PitchType == dxxPitchTypes.InvertedTriangular)
                {

                    altskip = mzUtils.IsOdd(xsteps) ? mzUtils.IsOdd(ysteps) : mzUtils.IsEven(ysteps);
                    if (!altskip)
                    {
                        _rVal = false;
                        aVector.Tag = "ALTERNATE COLUMN";
                        return _rVal;
                    }
                }
                else
                {
                    altskip = mzUtils.IsOdd(ysteps) ? mzUtils.IsOdd(xsteps) : mzUtils.IsEven(xsteps);
                    if (!altskip)
                    {
                        _rVal = false;
                        aVector.Tag = "ALTERNATE ROWS";
                        return _rVal;
                    }
                }
            }
            if (_rVal && !RectangularBounds)
            {
                _rVal = aSegs.ContainsVector(aVector, out UVECTORS ips, OnIsIn, aPrecis: 3, PointLimits.Right + PointLimits.Width);
                if (!_rVal)
                {
                    aVector.Tag = "EXTERIOR TO BOUNDARY";

                    return _rVal;
                }

            }
            if (_rVal && Islands.Count > 0)
            {
                for (int k = 1; k <= Islands.Count; k++)
                {
                    UARCREC aIslnd = Islands.Item(k);
                    if (aIslnd.ContainsPoint(aVector, OnIsIn, 5))
                    {
                        aVector.Tag = "ISLAND VIOLATION";
                        _rVal = false;
                        return _rVal;

                    }
                }
            }
            return _rVal;
        }

        public bool ValidateVector(uopVector aVector, out string rErrorString)
        {
            UVECTOR u1 = new UVECTOR(aVector);
            bool _rVal = xValidateVector(ref u1, Origin, null);
            rErrorString = u1.Tag;
            return _rVal;
        }

        internal bool xValidateVector(ref UVECTOR aVector, UVECTOR u0, uopSegments aSegs, bool? bAlt = null, double? f1 = null, double? f2 = null, bool bIgnoreMaxes = false)
        {
            aVector.Tag = string.Empty;
            if (!bAlt.HasValue) bAlt = TriangularPitch;
            if (!f1.HasValue) f1 = HStepFactor;
            if (!f2.HasValue) f2 = VStepFactor;


            //gross check that the point is within bounds
            bool _rVal = PointLimits.ContainsOrd(aVector.X, bOrdIsY: false, bOnIsIn: OnIsIn, aPrecis: 5) && PointLimits.ContainsOrd(aVector.Y, bOrdIsY: true, bOnIsIn: OnIsIn, aPrecis: 5);
            if (!_rVal)
            {
                aVector.Tag = "POINT LIMITS";
                return _rVal;
            }

            if (!bIgnoreMaxes)
            {
                if (_rVal && MaxX.HasValue)
                {
                    if (aVector.X > MaxX.Value)
                    {
                        _rVal = false;
                        aVector.Tag = "MAXX VIOLATION";
                        return _rVal;
                    }
                }

            }

            if (bAlt.Value && _rVal)
            {
                double dX = aVector.X - u0.X;
                double dY = aVector.Y - u0.Y;
                double xsteps = Math.Round(dX / (f1.Value * HPitch), 0) + 1;
                double ysteps = Math.Round(dY / (f2.Value * VPitch), 0) + 1;
                bool altskip = false;
                if (PitchType == dxxPitchTypes.InvertedTriangular)
                {

                    altskip = mzUtils.IsOdd(xsteps) ? mzUtils.IsOdd(ysteps) : mzUtils.IsEven(ysteps);
                    if (!altskip)
                    {
                        _rVal = false;
                        aVector.Tag = "ALTERNATE COLUMN";
                        return _rVal;
                    }
                }
                else
                {
                    altskip = mzUtils.IsOdd(ysteps) ? mzUtils.IsOdd(xsteps) : mzUtils.IsEven(xsteps);
                    if (!altskip)
                    {
                        _rVal = false;
                        aVector.Tag = "ALTERNATE ROWS";
                        return _rVal;
                    }
                }
            }
            if (_rVal && !RectangularBounds)
            {
                if (aSegs == null) aSegs = new uopSegments(Boundary.Segments);

                _rVal = aSegs.ContainsVector(aVector, out uopVectors ips, OnIsIn, aPrecis: 3, PointLimits.Right + 4 * PointLimits.Width);
                if (!_rVal)
                {
                    aVector.Tag = "EXTERIOR TO BOUNDARY";

                    return _rVal;
                }

            }
            if (_rVal && Islands.Count > 0)
            {
                for (int k = 1; k <= Islands.Count; k++)
                {
                    UARCREC aIslnd = Islands.Item(k);
                    if (aIslnd.ContainsPoint(aVector, OnIsIn, 5))
                    {
                        aVector.Tag = "ISLAND VIOLATION";
                        _rVal = false;
                        return _rVal;

                    }
                }
            }
            return _rVal;
        }

        /// <summary>
        /// locate partail row
        /// </summary>
        /// <param name="aGrid"></param>
        private void xLocatePartialRow()
        {
            PartialRowLocation = PartialRowLocation.ToUpper().Trim();
            if (PartialRowLocation != "BOTTOM" && PartialRowLocation != "TOP") return;
            if (FreePointCount <= 0 || RowCount <= 1) return;
            UVECTOR z1;

            int i = 0;
            ULINE tRow = ULINE.Null;
            ULINE bRow = ULINE.Null;
            tRow.sp.Y = System.Double.MinValue;
            bRow.sp.Y = System.Double.MaxValue;
            for (i = 1; i <= RowCount; i++)
            {
                ULINE aRow = Rows.Item(i);
                aRow.Index = i;
                if (aRow.sp.Y < bRow.sp.Y) bRow = new ULINE(aRow);

                if (aRow.sp.Y > tRow.sp.Y) tRow = new ULINE(aRow);

                Rows.SetItem(i, aRow);
            }
            if (tRow.Index == 0 || bRow.Index == 0) return;

            if (tRow.Index == bRow.Index) return;

            int pCnt_T = UGRID.GridRowCount2(this, tRow.Index, false, out int FILLDT, out int fCnt_T, out bool XST_T, out double YT);
            int pCnt_B = UGRID.GridRowCount2(this, bRow.Index, false, out int FILLDB, out int fCnt_B, out bool XST_B, out double YB);
            int idx = 0;
            if (PartialRowLocation == "BOTTOM" && fCnt_T <= 0) return;
            if (PartialRowLocation == "TOP" && fCnt_B <= 0) return;
            if (PartialRowLocation == "BOTTOM")
            {
                while (!(fCnt_T <= 0 || pCnt_B <= 0))
                {
                    idx = 0;

                    for (i = bRow.Points.Count; i >= 1; i--)
                    {
                        z1 = bRow.Points.Item(i);
                        if (!z1.Suppressed)
                        {
                            idx = i;
                            z1.Suppressed = true;
                            bRow.Points.SetItem(i, z1);
                            pCnt_B -= 1;
                            break;
                        }
                    }
                    if (idx == 0)
                    {
                        break;
                    }
                    idx = 0;
                    for (i = tRow.Points.Count; i >= 1; i--)
                    {
                        z1 = tRow.Points.Item(i);
                        if (z1.Suppressed)
                        {
                            idx = i;
                            z1.Suppressed = false;
                            tRow.Points.SetItem(i, z1);
                            fCnt_T -= 1;
                            break;
                        }
                    }
                    if (idx == 0)
                    {
                        break;
                    }
                }
            }
            else
            {
                if (fCnt_B <= 0 || pCnt_T <= 0)
                    return;

                while (!(fCnt_B <= 0 || pCnt_T <= 0))
                {
                    idx = 0;
                    for (i = tRow.Points.Count; i >= 1; i--)
                    {
                        z1 = tRow.Points.Item(i);
                        if (!z1.Suppressed)
                        {
                            idx = i;
                            z1.Suppressed = true;
                            tRow.Points.SetItem(i, z1);
                            pCnt_T -= 1;
                            break;
                        }
                    }
                    if (idx == 0)
                    {
                        break;
                    }

                    idx = 0;
                    for (i = bRow.Points.Count; i >= 1; i--)
                    {
                        z1 = bRow.Points.Item(i);
                        if (z1.Suppressed)
                        {
                            idx = i;
                            z1.Suppressed = false;
                            bRow.Points.SetItem(i, z1);
                            fCnt_B -= 1;
                            break;
                        }
                    }
                    if (idx == 0)
                    {
                        break;
                    }
                }
            }
            Rows.SetItem(tRow.Index, tRow);
            Rows.SetItem(bRow.Index, bRow);
        }

        /// <summary>
        /// returns bottom row of the grid (with more than zero grid points defined)
        /// </summary>
        /// <param name="aGrid"></param>
        /// <param name="bPopulated"></param>
        /// <returns></returns>
        public ULINE TopRow(bool bPopulated = false)
        {
            ULINE _rVal = ULINE.Null;

            double y1 = 0;
            int idx = 0;
            ULINE aRow;
            y1 = Double.MinValue;
            for (int i = 1; i <= Rows.Count; i++)
            {
                aRow = Rows.Item(i);
                aRow.Index = i;
                if (bPopulated)
                {
                    if (aRow.Points.GetBySuppressed(false).Count > 0)
                    {
                        if (aRow.sp.Y > y1)
                        {
                            idx = i;
                            y1 = aRow.sp.Y;
                        }
                    }
                }
                else
                {
                    if (aRow.sp.Y > y1)
                    {
                        idx = i;
                        y1 = aRow.sp.Y;
                    }
                }
                Rows.SetItem(i, aRow);
            }
            if (idx > 0) { _rVal = Rows.Item(idx); }//condition need to check while debugging 

            return _rVal;
        }


        /// <summary>
        /// returns bottom row of the grid (with more than zero grid points defined)
        /// </summary>
        /// <param name="bPopulated"></param>
        /// <returns></returns>
        public ULINE BottomRow(bool bPopulated = false)
        {
            ULINE _rVal = ULINE.Null;

            double y1 = Double.MaxValue;
            int idx = 0;
            ULINE aRow;
            for (int i = 1; i <= Rows.Count; i++)
            {
                aRow = Rows.Item(i);
                aRow.Index = i;
                if (bPopulated)
                {
                    if (aRow.Points.GetBySuppressed(false).Count > 0)
                    {
                        if (aRow.sp.Y < y1)
                        {
                            idx = i;
                            y1 = aRow.sp.Y;
                        }
                    }
                }
                else
                {
                    if (aRow.sp.Y < y1)
                    {
                        idx = i;
                        y1 = aRow.sp.Y;
                    }
                }
                Rows.SetItem(i, aRow);
            }
            if (idx > 0) { _rVal = Rows.Item(idx); }//condition need to check while debugging 

            return _rVal;
        }



        /// <summary>
        /// returns grid points
        /// </summary>
        /// <param name="bFreePts"></param>
        /// <param name="aRowID"></param>
        /// <returns></returns>
        public UVECTORS GetGridPoints(bool bFreePts = false, int aRowID = 0)
        {
            UVECTORS _rVal = UVECTORS.Zero;

            if (aRowID > 0)
            {
                if (aRowID <= Rows.Count)
                {
                    ULINE aLn = Rows.Item(aRowID);
                    _rVal = aLn.Points.GetBySuppressed(bFreePts, true);
                }


            }
            else
            {
                for (int i = 1; i <= Rows.Count; i++)
                {
                    ULINE aLn = Rows.Item(i);
                    UVECTORS rowpts = aLn.Points.GetBySuppressed(bFreePts, true);
                    _rVal.Append(rowpts);
                }

            }
            return _rVal;
        }


        /// <summary>
        /// returns grid points
        /// </summary>
        /// <param name="bFreePts"></param>
        /// <param name="aRowID"></param>
        /// <returns></returns>
        public UVECTORS GetGridPoints(TVALUES aValues, bool bFreePts = false, int aRowID = 0)
        {
            UVECTORS _rVal = UVECTORS.Zero;
            ULINE aLn;
            if (aRowID > 0)
            {
                if (aRowID <= Rows.Count)
                {
                    aLn = Rows.Item(aRowID);
                    _rVal = aLn.Points.GetBySuppressed(bFreePts, true);
                }


            }
            else
            {
                int k = 0;
                int m = 0;
                UVECTOR u1;
                for (int i = 1; i <= Rows.Count; i++)
                {
                    aLn = Rows.Item(i);
                    m = 0;
                    for (int j = 1; j <= aLn.Points.Count; j++)
                    {
                        u1 = aLn.Points.Item(j);
                        if (bFreePts == u1.Suppressed)
                        {
                            k++;
                            m++;
                            if (k <= aValues.Count)
                            {
                                u1.Value = mzUtils.VarToDouble(aValues.Item(k));
                                //if (m == 1 && !u1.Suppressed)
                                //aLn.Value = u1.Value;
                            }

                            aLn.Points.SetItem(j, u1);

                            _rVal.Add(u1);



                        }
                    }


                    Rows.SetItem(i, aLn);
                }

            }


            return _rVal;
        }

        #region Shared Methods

        /// <summary>
        /// Return row count
        /// </summary>
        /// <param name="aGrid"></param>
        /// <param name="aRowID"></param>
        /// <param name="rFilledCount"></param>
        /// <param name="rFreeCount"></param>
        /// <param name="bReportFreePts"></param>
        /// <param name="rRowExists"></param>
        /// <param name="rRowY"></param>
        /// <returns></returns>
        public static int GridRowCount(UGRID aGrid, int aRowID, out int rFilledCount, out int rFreeCount, bool bReportFreePts, out bool rRowExists, out double rRowY)
        {
            int _rVal = 0;
            rRowExists = aRowID > 0 & aRowID <= aGrid.Rows.Count;
            rFreeCount = 0;
            rFilledCount = 0;
            rRowY = aGrid.Boundary.Center.Y;
            if (!rRowExists)
            {
                return _rVal;

            }

            ULINE aRow = aGrid.Rows.Item(aRowID);
            rRowY = aRow.sp.Y;
            for (int i = 1; i <= aRow.Points.Count; i++)
            {
                if (aRow.Points.Item(i).Suppressed)
                { rFreeCount++; }
                else
                { rFilledCount++; }
            }
            if (!bReportFreePts)
            { _rVal = rFilledCount; }
            else
            { _rVal = rFreeCount; }

            return _rVal;
        }

        public static int GridRowCount2(UGRID aGrid, int aRowID, bool bReportFreePts, out int rFilledCount, out int rFreeCount, out bool rRowExists, out double rRowY)
        {
            int _rVal = 0;
            rRowExists = aRowID > 0 & aRowID <= aGrid.Rows.Count;
            rFreeCount = 0;
            rFilledCount = 0;
            rRowY = aGrid.Boundary.Center.Y;
            if (!rRowExists)
            {
                return _rVal;

            }

            ULINE aRow = aGrid.Rows.Item(aRowID);
            rRowY = aRow.sp.Y;
            for (int i = 1; i <= aRow.Points.Count; i++)
            {
                if (aRow.Points.Item(i).Suppressed)
                { rFreeCount++; }
                else
                { rFilledCount++; }
            }
            if (!bReportFreePts)
            { _rVal = rFilledCount; }
            else
            { _rVal = rFreeCount; }

            return _rVal;
        }

        #endregion
    }

}
