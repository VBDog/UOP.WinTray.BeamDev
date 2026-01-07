using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UOP.DXFGraphics;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Interfaces;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;


namespace UOP.WinTray.Projects
{
    /// <summary>
    /// Provides Rectangles functionalities
    /// </summary>
    public class uopRectangle : ICloneable, iShape, iRectangle, iArcRec
    {
  

        #region Constructors

        public uopRectangle()=> Init();

        public uopRectangle(iVector aCenter, double aWidth, double aHeight, string aTag = null, string aFlag = null, double aRotation = 0)
        {
            Init(aCenter: aCenter);
            Width = aWidth; Height = aHeight;
            if (aTag != null) Tag = aTag;
            if(aFlag != null) Flag = aFlag;
            Rotation = aRotation;
     
        }
        public uopRectangle(double aLeft = 0, double aTop = 0, double aRight = 0, double aBottom = 0, string aTag = "", string aName = "", string aFlag = "")
        {
            Init(aTag:aTag, aName : aName,aFlag:aFlag);
            Define(aLeft,aRight,aTop,aBottom);
        }

        public uopRectangle(IEnumerable<iVector> aVectors)
        {
            Init();
            URECTANGLE bounds = uopRectangle.ComputeBounds(aVectors, out double wd, out double ht);
            X = bounds.X; Y = bounds.Y; Rotation = 0;
            Width = wd;
            Height = ht;

        }
    

        internal uopRectangle(URECTANGLE aStructure, double aWidthAdder = 0, double aHeightAdder = 0, string aTag = null, int? aRow = null, int? aCol = null)
        {
            Init( bRectangle:aStructure);
       
            if (aWidthAdder != 0)
                Width += aWidthAdder/2;
            if (aHeightAdder != 0)
               Height += aHeightAdder/2;
                if (aTag != null) Tag = aTag;
            if(aRow.HasValue) Row = aRow.Value;
            if(aCol.HasValue) Col = aCol.Value;

        }

        public uopRectangle(iArcRec aArcRec)
        {
            Init(); 

            if (aArcRec == null) return;
            
              Width = aArcRec.Width;
            Height = aArcRec.Height;
            X = aArcRec.X; Y = aArcRec.Y;
            Tag = aArcRec.Tag;
            Flag = aArcRec.Flag;
            Rotation = aArcRec.Rotation;
            if (aArcRec is uopRectangle)
            {
                uopRectangle urec = (uopRectangle)aArcRec;
                Invalid = urec.Invalid;
                Tag = urec.Tag;
                Flag = urec.Flag;
                Index = urec.Index;
                Name = urec.Name;
                Row = urec.Row;
                Col = urec.Col;
                IsVirtual = urec.IsVirtual;
                Rotation = urec.Rotation;
                Suppressed = urec.Suppressed;
            }
        }

        public uopRectangle(uopShape aRectangularShape)
        {
            Init();

            if (aRectangularShape == null) return;
            Width = aRectangularShape.Width;
            Height = aRectangularShape.Height;
            Center = aRectangularShape.Center;
            Tag = aRectangularShape.Tag;
            Flag = aRectangularShape.Flag;
            Name = aRectangularShape.Name;
            if (!aRectangularShape.IsRectangular(true,3)) return;  //there are four vertices and and the distaance from vertex 1 and 3 is equal to th distaance from 2 to 4

            
        
            uopLines lines = aRectangularShape.Vertices.LineSegments(true, false, bLinked: true);
            if (lines.Count < 3) return;

            uopLine line1 = lines[0];
            
            
            if(line1.IsVertical(2) || line1.IsHorizontal(2))
            {
                return;  // this is a rectangle orthogonal to the world xy plane
            }
            else
            {
                Rotation = lines[1].AngleOfInclination;
                Height = line1.Length;
                Width = lines[2].Length;
            }

                

        }


        public uopRectangle(iRectangle aRectangle, iVector aCenter = null) => Init(aRectangle, aCenter: aCenter);
          
        private void Init(iRectangle aRectangle = null, URECTANGLE? bRectangle = null, iVector aCenter = null, string aTag = "", string aName = "", string aFlag = "", double aRotation = 0)
        {
            _Center = uopVector.Zero;
            Invalid = false;
            Width = 0;
            Height = 0;
            Tag =aTag;
            Flag =aFlag;
            Index = 0;
            Name = aName;
            Row = 0;
            Col = 0;
            IsVirtual = false;
            Rotation =aRotation;
            Suppressed = false;
            Selected = false;
            if (aRectangle != null)
            {
                _Center = new uopVector(aRectangle.Center);
                Width = aRectangle.Width;
                Height = aRectangle.Height;

                if (aRectangle is uopRectangle)
                {
                    uopRectangle urec = (uopRectangle)aRectangle;
                    Invalid = urec.Invalid;
                    Tag = urec.Tag;
                    Flag = urec.Flag;
                    Index = urec.Index;
                    Name = urec.Name;
                    Row = urec.Row;
                    Col = urec.Col;
                    IsVirtual = urec.IsVirtual;
                    Rotation = urec.Rotation;
                    Suppressed = urec.Suppressed;
                    Selected = urec.Selected;
                }
                else
                {
                    dxfPlane plane = new dxfPlane(aRectangle.Plane);
                    Rotation = plane.XDirection.XAngle;
                }
            }

            if (bRectangle.HasValue)
            {
                _Center = new uopVector(bRectangle.Value.Center);
                Width = bRectangle.Value.Width;
                Height = bRectangle.Value.Height;
                Invalid = bRectangle.Value.Invalid;
                Tag = bRectangle.Value.Tag;
                Flag = bRectangle.Value.Flag;
                Index = bRectangle.Value.Index;
                Name = bRectangle.Value.Name;
                Row = bRectangle.Value.Row;
                Col = bRectangle.Value.Col;
                IsVirtual = bRectangle.Value.IsVirtual;
                Rotation = bRectangle.Value.Rotation;
                Suppressed = bRectangle.Value.Suppressed;
                Selected = bRectangle.Value.Selected;
            }
            if (aCenter != null) Center = new uopVector(aCenter);
        }

        #endregion Constructors

        #region Properties
        public bool Suppressed { get => Center.Suppressed; set => Center.Suppressed = value; }
        public uopVectors Corners   =>   new uopVectors() { TopLeft, BottomLeft, BottomRight, TopRight }; 
    
        public double AspectRatio => Height == 0 ? 0 : Width / Height;

        public double Rotation { get; set; }

        private uopVector _Center;
        public uopVector Center
        {
            get
            {
                _Center ??= uopVector.Zero; _Center.Value = Rotation; return _Center;
            }
            set 
            {
                value ??= uopVector.Zero;
                _Center.X = value.X;
                _Center.Y = value.Y;
               _Center.Suppressed = value.Suppressed;
                _Center.Value = value.Value; //Capture rotation
            }
        }

        public uopVector BottomLeft
        {
            get
            {

                if (Rotation == 0) return new uopVector(Left, Bottom) { Tag = "BOTTOM LEFT" };
                uopVector xDir = XDir;
                uopVector  yDir = xDir.Rotated(-90);
                uopVector _rVal = Center + xDir * -(0.5 * Width)  + yDir * -(0.5 * Height);
                _rVal.Tag = "BOTTOM LEFT";
                return _rVal;
            }
        }

        public uopVector BottomRight
        {
            get
            {

                if (Rotation == 0) return new uopVector(Right, Bottom) { Tag = "BOTTOM RIGHT" };
                uopVector xDir = XDir;
                uopVector yDir = xDir.Rotated(-90);
                uopVector _rVal = Center + xDir * +(0.5 * Width) + yDir * -(0.5 * Height);
                _rVal.Tag = "BOTTOM RIGHT";
                return _rVal;
            }
        }
        public uopShape Shape => new uopShape(Corners, Name, Tag, false) { Flag = Flag, Row = Row, Col = Col};

        public uopVector TopRight
        {
            get
            {

                if (Rotation == 0) return new uopVector(Right, Top) { Tag = "TOP RIGHT" };
                uopVector xDir = XDir;
                uopVector yDir = xDir.Rotated(-90);
                uopVector _rVal = Center + xDir * +(0.5 * Width) + yDir * +(0.5 * Height);
                _rVal.Tag = "TOP RIGHT";
                return _rVal;
            }
        }
        public uopVector TopLeft
        {
            get
            {

                if (Rotation == 0) return new uopVector(Left, Top) { Tag = "TOP LEFT" };
                uopVector xDir = XDir;
                uopVector yDir = xDir.Rotated(-90);
                uopVector _rVal = Center + xDir * -(0.5 * Width) + yDir * +(0.5 * Height);
                _rVal.Tag = "TOP LEFT";
                return _rVal;
            }
        }
        public double Area { get { return Width * Height; } }

        private double _Width;
        public double Width
        {
            get =>_Width;
            set => _Width = Math.Abs(value);

        }
        private double _Height;
        public double Height
        {
            get => _Height;
            set => _Height = Math.Abs(value);

        }

     
        public int Index { get; set; }
        public bool Invalid { get; set; }

        public double Left 
        { 
            get => Rotation == 0 ? X - 0.5 * Width : Corners.GetExtremeOrd(bMin: true, bGetY: false, 15);  
         set 
            { 
                if (Rotation == 0) 
                { 
                    Width = Math.Abs(Right - value); 
                    X = value + 0.5 * Width; 
                } 
                else 
                    Center.Move(value - Left);  
            }  
        }



        public double Right { get => Rotation == 0 ? X + 0.5 * Width : Corners.GetExtremeOrd(bMin: false, bGetY: false, 15); set { if (Rotation == 0) { Width = Math.Abs( value-Left); X = value - 0.5 * Width; } else Center.Move(value - Right); } }

    
        public double Top { get => Rotation == 0 ? Y + 0.5 * Height : Corners.GetExtremeOrd(bMin: false, bGetY: true, 15); set { if (Rotation == 0) { Height = Math.Abs( value - Bottom); Y = value - 0.5 * Height; } else Center.Move(0,value - Top); } }

        public double Bottom { get => Rotation == 0 ? Y - 0.5 * Height : Corners.GetExtremeOrd(bMin: true, bGetY: true, 15); set { if (Rotation == 0)  { Height = Math.Abs(Top -value); Y = value + 0.5 * Height; } else Center.Move(0, value - Bottom); } }

        public string Tag { get => Center.Tag; set => Center.Tag = value; }
        public string Flag { get => Center.Flag; set => Center.Flag = value; }
        public string Name { get; set; }

        public double X {get => Center.X; set=> Center.X = value; }

        public double Y { get => Center.Y; set => Center.Y = value; }
        public int Row { get; set; }
        public int Col { get; set; }

        public bool IsVirtual { get; set; }

        public double Diagonal => Math.Sqrt(Math.Pow(Width, 2) + Math.Pow(Height, 2));

        public List<iSegment> Segments => new List<iSegment>()  { LeftEdge, BottomEdge, RightEdge, TopEdge };

        public uopLines Edges => new uopLines() { LeftEdge, BottomEdge, RightEdge, TopEdge };
        public uopLine LeftEdge => new uopLine(TopLeft, BottomLeft) { Tag = "LEFT", Side = uppSides.Left };
        public uopLine BottomEdge => new uopLine(BottomLeft, BottomRight) { Tag = "BOTTOM", Side = uppSides.Bottom };

        public uopLine RightEdge => new uopLine(BottomRight, TopRight) { Tag = "RIGHT", Side = uppSides.Right };
        public uopLine TopEdge => new uopLine(TopRight, TopLeft) { Tag = "TOP", Side = uppSides.Top };

        public bool IsNull { get => Width == 0 || Height == 0; }

        public uopVector XDir { get { uopVector _rVal = new uopVector(1, 0); if (Rotation != 0) { _rVal.Rotate(Rotation); } return _rVal; } }
        public uopVector YDir { get { uopVector _rVal = new uopVector(0, 1); if (Rotation != 0) { _rVal.Rotate(Rotation); } return _rVal; } }

        public bool Selected { get; set; }

        #endregion Properties

        #region Methods

        public void SetCenter(uopVector aCenter) { if(aCenter != null) _Center = aCenter; }
        public void Mirror(double? aX, double? aY) => Center.Mirror(aX, aY);
        


        public void Move(double aDX = 0, double aDY = 0) => Center.Move(aDX, aDY);

        public dxfRectangle ToDXFRectangle(dxfDisplaySettings aDisplaySettings = null) => new dxfRectangle(Center, Width, Height) { Tag = Tag, DisplaySettings = aDisplaySettings };

        public dxePolyline Perimeter(double aBuffer = 0, string aLayerName = "", dxxColors aColor = dxxColors.Undefined, string aLinetype = "") => new dxePolyline(Corners, bClosed: true, aDisplaySettings: new dxfDisplaySettings(aLayerName, aColor, aLinetype)) { Tag = Tag };

       
        public bool Contains(double aOrdinate, dxx2DOrdinateDescriptors aOrdType,bool bOnIsIn = true,int aPrecis = 3) 
        {
            aPrecis = mzUtils.LimitedValue(aPrecis, 0, 15);
            aOrdinate = Math.Round(aOrdinate, aPrecis);
                   double min = aOrdType == dxx2DOrdinateDescriptors.X ? Math.Round(Left, aPrecis) : Math.Round(Bottom, aPrecis);
                double max = aOrdType == dxx2DOrdinateDescriptors.X ? Math.Round(Right, aPrecis) : Math.Round(Top, aPrecis);

                return bOnIsIn ? aOrdinate >= min && aOrdinate <= max : aOrdinate > min && aOrdinate < max;

        }
  
        internal bool Contains(UVECTOR aVector, bool bOnIsOut = false, bool bHorizontalOnly = false, int aPrecis = 4)  => Contains(new uopVector( aVector), bOnIsOut, aPrecis,bHorizontalOnly);

        public bool Contains(IEnumerable<iVector> aVectors,  out int rCount, bool bOnIsOut = false, bool bHorizontalOnly = false, int aPrecis = 4, bool bJustOne =false)
        {
            rCount = 0;
            if (aVectors == null || aVectors.Count() <=0) return false;
            int tcnt = 0;
            foreach(var ivec  in aVectors)
            {
                if (ivec == null) continue;
                tcnt ++;
                if(Contains(  uopVector.FromVector(ivec), bOnIsOut,aPrecis, bHorizontalOnly))
                {
                    rCount++;
                    if (bJustOne) return true;
                }
            }
            return bJustOne ? false: rCount == tcnt;
        }

        public bool Contains(uopVector aVector, bool bOnIsOut = false, int aPrecis = 4, bool bHorizontalOnly = false)
        {
         
            bool _rVal = false;
            if (aVector == null) return false;
            aPrecis = mzUtils.LimitedValue(aPrecis, 0, 15, 4);
            if (Rotation == 0)
            {
                if (!bOnIsOut)
                {
                    if (Math.Round(aVector.X, aPrecis) >= Math.Round(Left, aPrecis) && Math.Round(aVector.X, aPrecis) <= Math.Round(Right, aPrecis))
                    {
                        if (bHorizontalOnly)
                        { _rVal = true; }
                        else
                        { _rVal = Math.Round(aVector.Y, aPrecis) >= Math.Round(Bottom, aPrecis) && Math.Round(aVector.Y, aPrecis) <= Math.Round(Top, aPrecis); }
                    }
                }
                else
                {
                    if (Math.Round(aVector.X, aPrecis) > Math.Round(Left, aPrecis) && Math.Round(aVector.X, aPrecis) < Math.Round(Right, aPrecis))
                    {
                        if (bHorizontalOnly)
                        { _rVal = true; }
                        else
                        { _rVal = Math.Round(aVector.Y, aPrecis) > Math.Round(Bottom, aPrecis) && Math.Round(aVector.Y, aPrecis) < Math.Round(Top, aPrecis); }
                    }
                }
            }
            else
            {
                double d1 = Math.Sqrt(Math.Pow(X - aVector.X, 2) + Math.Pow(Y - aVector.Y, 2));
                double d2 = Diagonal / 2;
                if (Math.Round(d1, aPrecis) > Math.Round(d2, aPrecis) ) return false; // proximity check
                uopVector xDir = XDir;
            
                uopVector dir = aVector.DirectionTo(LeftEdge, out d1);
                if (d1 == 0 && bOnIsOut) return false;
                if(!dir.IsEqual(xDir, aPrecis,false)) return false;
                if (d1 < Math.Round(0.5 * Width, aPrecis)) return true;

                dir = aVector.DirectionTo(RightEdge, out  d1);
                if (d1 == 0 && bOnIsOut) return false;
                if (!dir.IsEqual(xDir, aPrecis, false)) return false;
                if (d1 < Math.Round(0.5 * Width, aPrecis)) return true;

                uopVector yDir = YDir;
                dir = aVector.DirectionTo(BottomEdge, out d1);
                if (d1 == 0 && bOnIsOut) return false;
                if (dir.IsEqual(yDir, aPrecis, false)) return false;
                if (d1 < Math.Round(0.5 * Height, aPrecis)) return true;

          
                dir = aVector.DirectionTo(TopLeft, out d1);
                if (d1 == 0 && bOnIsOut) return false;
                if (!dir.IsEqual(yDir, aPrecis, false)) return false;
                if (d1 < Math.Round(0.5 * Height, aPrecis)) return true;

            }



            return _rVal;
        }
        public bool ContainsVector(iVector aVector, bool bOnIsIn= true, int aPrecis = 4, bool bInfinte = false) => ContainsVector(aVector, bOnIsIn, out _, out _, out _, aPrecis);
        
        public bool ContainsVector(iVector aVector, bool bOnIsIn, out bool rWithin, out bool rIsOn, out bool rIsCorner, int aPrecis = 4)
        {

            rWithin = false;
            rIsOn = false;
            rIsCorner = false;
            bool _rVal = false;
            if (aVector == null) return false;
            aPrecis = mzUtils.LimitedValue(aPrecis, 0, 15, 4);
            if (Rotation == 0)
            {
                rWithin = (Math.Round(aVector.X, aPrecis) >= Math.Round(Left, aPrecis) && Math.Round(aVector.X, aPrecis) <= Math.Round(Right, aPrecis) && Math.Round(aVector.Y, aPrecis) >= Math.Round(Bottom, aPrecis) && Math.Round(aVector.Y, aPrecis) <= Math.Round(Top, aPrecis) );
                rIsOn = (Math.Round(aVector.X, aPrecis) == Math.Round(Left, aPrecis) || Math.Round(aVector.X, aPrecis) == Math.Round(Right, aPrecis) || Math.Round(aVector.Y, aPrecis) == Math.Round(Bottom, aPrecis)|| Math.Round(aVector.Y, aPrecis) == Math.Round(Top, aPrecis));
                if (rIsOn)
                {
                    if (Math.Round(aVector.X, aPrecis) == Math.Round(Left, aPrecis) && (Math.Round(aVector.Y, aPrecis) == Math.Round(Top, aPrecis) || Math.Round(aVector.Y, aPrecis) == Math.Round(Bottom, aPrecis))) rIsCorner = true ;
                    if ( !rIsCorner && (Math.Round(aVector.X, aPrecis) == Math.Round(Right, aPrecis) && (Math.Round(aVector.Y, aPrecis) == Math.Round(Top, aPrecis) || Math.Round(aVector.Y, aPrecis) == Math.Round(Bottom, aPrecis)))) rIsCorner = true;
                    if (!rIsCorner && (Math.Round(aVector.Y, aPrecis) == Math.Round(Top, aPrecis) && (Math.Round(aVector.X, aPrecis) == Math.Round(Left, aPrecis) || Math.Round(aVector.X, aPrecis) == Math.Round(Right, aPrecis)))) rIsCorner = true;
                    if (!rIsCorner && (Math.Round(aVector.Y, aPrecis) == Math.Round(Bottom, aPrecis) && (Math.Round(aVector.X, aPrecis) == Math.Round(Left, aPrecis) || Math.Round(aVector.X, aPrecis) == Math.Round(Right, aPrecis)))) rIsCorner = true;

                }


                if (rIsOn) rWithin = false;
                return rWithin ? true : rIsOn && bOnIsIn; 
          
            }
            else
            {
                double d1 = Math.Sqrt(Math.Pow(X - aVector.X, 2) + Math.Pow(Y - aVector.Y, 2));
                double d2 = Diagonal / 2;
                if (Math.Round(d1, aPrecis) > Math.Round(d2, aPrecis)) return false; // proximity check
                uopVector xDir = XDir;
                uopVectors corners = Corners;
                rIsCorner = corners.FindAll(x => x.DistanceTo(aVector,aPrecis) == 0).Count > 0;
                rIsOn = rIsCorner;
                if(rIsOn  && bOnIsIn) return true;
                if (!rIsCorner)
                {
                    uopVector v1 = uopVector.FromVector(aVector);
                    uopVector dir = v1.DirectionTo(LeftEdge, out d1);
                    rIsOn = (d1 == 0);
                    rWithin = !dir.IsEqual(xDir, aPrecis, false) && !rIsOn && d1 < Math.Round(0.5 * Width, aPrecis);
                    if (rWithin || rIsOn) return rWithin ? true : rIsOn && bOnIsIn;

                    dir = v1.DirectionTo(RightEdge, out d1);
                    rIsOn = (d1 == 0);
                    rWithin = dir.IsEqual(xDir, aPrecis, false) && !rIsOn && d1 < Math.Round(0.5 * Width, aPrecis);
                    if (rWithin || rIsOn) return rWithin ? true : rIsOn && bOnIsIn;

                    uopVector yDir = YDir;
                    dir = v1.DirectionTo(BottomEdge, out d1);
                    rIsOn = (d1 == 0);
                    rWithin = !dir.IsEqual(xDir, aPrecis, false) && !rIsOn && d1 < Math.Round(0.5 * Height, aPrecis);
                    if (rWithin || rIsOn) return rWithin ? true : rIsOn && bOnIsIn;


                    dir = v1.DirectionTo(TopLeft, out d1);
                    rIsOn = (d1 == 0);
                    rWithin = dir.IsEqual(xDir, aPrecis, false) && !rIsOn && d1 < Math.Round(0.5 * Height, aPrecis);
                    if (rWithin || rIsOn) return rWithin ? true : rIsOn && bOnIsIn;
                }
          

            }



            return _rVal;
        }

        public bool Contains(double aX, double aY, bool bOnIsOut = false, int aPrecis = 4) =>Contains(new uopVector( aX, aY), bOnIsOut, aPrecis: aPrecis);

        public List<uopVector> ContainedVectors(IEnumerable<uopVector> aVectors,bool bOnIsOut = false, bool bHorizontalOnly = false, bool bJustOne = false)
        {
            List<uopVector> _rVal = new List<uopVector>();
            if (aVectors == null) return _rVal;
            foreach (uopVector v in aVectors)
            {
                if (this.Contains(v ,   bOnIsOut, bHorizontalOnly: bHorizontalOnly))
                {
                    _rVal.Add(v);
                    if (bJustOne) break;
                }
            }

            return _rVal;
        }

 

        public void Stretch(double? aWidthAdder = null, double? aHeightAdder = null) 
        {
            if (aWidthAdder.HasValue) Width += aWidthAdder.Value;
            if (aHeightAdder.HasValue) Height += aHeightAdder.Value;

        }

        public uopRectangle Stretched(double aWidthAdder, double aHeightAdder)
        {
            uopRectangle _rVal = new uopRectangle(this);
            _rVal.Stretch(aWidthAdder, aHeightAdder);
            return _rVal;
        }


        public uopRectangle Clone() => new uopRectangle(this);

        object ICloneable.Clone() => (object)this.Clone();

        public bool ExpandTo(uopRectangle aRectangle)
        {
            if (aRectangle == null) return false;
            bool _rVal = false;
            if(Rotation == 0)
            {
                 double ord = aRectangle.Left;
                if (ord < Left) { _rVal = true; Left = ord; }
                ord = aRectangle.Right;
                if (ord > Right) { _rVal = true; Right = ord; }
                ord = aRectangle.Top;
                if (ord > Top) { _rVal = true; Top = ord; }
                ord = aRectangle.Bottom;
                if (ord < Bottom) { _rVal = true; Bottom = ord; }
            }
      
            return _rVal;
        }
        
        public bool Define(double? aLeft = null, double? aRight = null, double? aTop = null, double? aBot = null, double? aRotation = null, int aPrecis = -1)
        {
            if(aRotation.HasValue) Rotation = aRotation.Value;
            double left = aLeft.HasValue ? aLeft.Value : Left;
            double right = aRight.HasValue ? aRight.Value : Right;
            double top = aTop.HasValue ? aTop.Value : Top;
            double bot = aBot.HasValue ? aBot.Value : Bottom;

            double wd = Math.Abs(right - left);
            double height = Math.Abs(top - bot);
            if (aPrecis >= 0)
            {
                wd = Math.Round(wd, aPrecis);
               height = Math.Round(height, aPrecis);
            }
            bool _rVal = Width != wd || Height != height;
            X = left + 0.5 * wd;
            Y = bot + 0.5 * height;
            Width = wd;
            Height = height;
            return _rVal;

        }


        public override string ToString()
        {
            string _rVal =  $"uopRectangle";
            if (!string.IsNullOrWhiteSpace(Name)) _rVal += $" {Name}";
            if (Row !=0) _rVal += $" Row:{Row}";
            if (Col != 0) _rVal += $" Col:{Col}";

            _rVal += $" [ L:{string.Format("{0:#,0.####}", Left)}  R:{ string.Format("{0:#,0.####}", Right) } T:{ string.Format("{0:#,0.####}", Top) } B:{ string.Format("{0:#,0.####}", Bottom) } ]";
            return _rVal;
        }

        #endregion Methods

        #region iArcRec Implementation
        public bool Contains(iArcRec aArcRec, bool bOnIsIn = true, int aPrecis = 4, bool bMustBeCompletelyWithin = false, bool bReturnTrueByCenter = false) => uopArcRecs.ArcRecContainsArcRec(this, aArcRec, out _, out _, bOnIsIn, aPrecis, bMustBeCompletelyWithin, bReturnTrueByCenter);

        public bool Contains(iArcRec aArcRec, out bool rCoincindent, out bool rIntersects, bool bOnIsIn = true, int aPrecis = 4, bool bMustBeCompletelyWithin = false, bool bReturnTrueByCenter = false) => uopArcRecs.ArcRecContainsArcRec(this, aArcRec, out rCoincindent, out rIntersects, bOnIsIn, aPrecis, bMustBeCompletelyWithin, bReturnTrueByCenter);

        
        uopArc iArcRec.Arc => null;
        uopRectangle iArcRec.Rectangle => this;
        uopShape iArcRec.Slot => null;

        double iArcRec.Radius => Diagonal/2;

        iArcRec iArcRec.Clone() => Clone();

        #endregion iArcRec Implementation

        #region iShape Implementation


        IEnumerable<iVector> iShape.Vertices { get => Corners;  }
        dxfPlane iShape.Plane { get => new dxfPlane(Center); }
        bool iShape.Closed { get => true; }


        #endregion iShape Implementation

        #region iRectangle Implementation
        uppArcRecTypes iArcRec.Type => uppArcRecTypes.Rectangle;
        dxfPlane iRectangle.Plane { get => new dxfPlane(Center); set { } }
        double iRectangle.Width { get => Width; set => Width = value; }
        double iRectangle.Height { get => Height; set => Height = value; }
        iVector iRectangle.Center { get => Center; set => Center = new uopVector(value); }

        #endregion iRectangle Implementation

        #region Shared Methods

        public static  uopRectangle FromRectangle(iRectangle aRectangle, bool bReturnClone = false)
        {
            if (aRectangle == null) return null;
            if (aRectangle is uopRectangle)
                return bReturnClone ? new uopRectangle((uopRectangle)aRectangle) : (uopRectangle)aRectangle;
            else
                return new uopRectangle(aRectangle);
        }

        public static uopRectangle CloneCopy(uopRectangle aRectangle) => aRectangle == null ? null : new uopRectangle(aRectangle);

        internal static URECTANGLE ComputeBounds(IEnumerable<iVector> aVectors, out double rWidth, out double rHeight)
        {
            rWidth = 0;
            rHeight = 0;

            if (aVectors == null) return new URECTANGLE(0, 0, 0, 0);

            List<iVector> vecs = aVectors.ToList();
            if (vecs.Count == 0) return new URECTANGLE(0, 0, 0, 0);


            uopVector u1 = uopVector.FromVector(vecs[0]);
            URECTANGLE _rVal = new URECTANGLE(u1.X, u1.Y, u1.X, u1.Y);


            if (vecs.Count <= 1) return _rVal;


            for (int i = 2; i <= vecs.Count; i++)
            {
                u1 = uopVector.FromVector(vecs[i - 1]);
                if (u1.X < _rVal.Left) _rVal.Left = u1.X;
                if (u1.X > _rVal.Right) _rVal.Right = u1.X;
                if (u1.Y > _rVal.Top) _rVal.Top = u1.Y;
                if (u1.Y < _rVal.Bottom) _rVal.Bottom = u1.Y;

            }

            rWidth = _rVal.Width;
            rHeight = _rVal.Height;
            return _rVal;
        }
        #endregion Shared Methods
    }
}