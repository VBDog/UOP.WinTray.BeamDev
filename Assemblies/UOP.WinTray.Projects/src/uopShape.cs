using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Media.Imaging;
using UOP.DXFGraphics;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Interfaces;
using UOP.WinTray.Projects.Parts;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;


namespace UOP.WinTray.Projects
{
    public class uopShape : ICloneable, iShape, iArcRec
    {

        #region iShape Implementation


        IEnumerable<iVector> iShape.Vertices { get => Vertices; }
        dxfPlane iShape.Plane { get => new dxfPlane(new dxfVector(Bounds.X, Bounds.Y)); }
        bool iShape.Closed { get => !Open; }

        #endregion iShape Implementation

        #region Constructors

        public uopShape() => Init();

        internal uopShape(USHAPE aShape, string aName = null, string aTag = null, string aFlag = null, string aHandle = null, int? aOccurance = null, int? aPartIndex = null, int? aRow = null, int? aCol = null)
        {
            Init(aShape);
            if (aTag != null) Tag = aTag;
            if (aFlag != null) Flag = aFlag;
            if (aName != null) Name = aName;
            if (aHandle != null) Handle = aHandle;
            if (aOccurance.HasValue) OccuranceFactor = aOccurance.Value;
            if (aPartIndex.HasValue) PartIndex = aPartIndex.Value;
            if (aRow.HasValue) Row = aRow.Value;
            if (aCol.HasValue) Col = aCol.Value;
        }

        internal uopShape(UARCREC aShape, string aName = null, string aTag = null, string aFlag = null, string aHandle = null, int? aOccurance = null, int? aPartIndex = null, int? aRow = null, int? aCol = null)
        {
            Init(new USHAPE( aShape));
            if (aTag != null) Tag = aTag;
            if (aFlag != null) Flag = aFlag;
            if (aName != null) Name = aName;
            if (aHandle != null) Handle = aHandle;
            if (aOccurance.HasValue) OccuranceFactor = aOccurance.Value;
            if (aPartIndex.HasValue) PartIndex = aPartIndex.Value;
            if (aRow.HasValue) Row = aRow.Value;
            if (aCol.HasValue) Col = aCol.Value;
        }
        public uopShape(uopShape aShape,  string aName = null, string aTag = null, string aFlag = null, string aHandle = null, int? aOccurance = null, int? aPartIndex = null, int? aRow = null, int? aCol = null)
        {

            Init(null, aShape);

            if (aTag != null) Tag = aTag;
            if (aFlag != null) Flag = aFlag;
            if (aName != null) Name = aName;
            if (aHandle != null) Handle = aHandle;
            if (aOccurance.HasValue) OccuranceFactor = aOccurance.Value;
            if (aPartIndex.HasValue) PartIndex = aPartIndex.Value;
            if (aRow.HasValue) Row = aRow.Value;
            if (aCol.HasValue) Col = aCol.Value;
        }

        public uopShape(dxfPolyline aPolyline) => Init(USHAPE.FromDXFPolyline(aPolyline));
        

        public uopShape(uopArc aArc, string aName = "")
        {
            uopVectors verts = null;
            if (aArc != null && aArc.Radius > 0)
            {
                verts = new uopVectors(new uopVector(aArc.X + aArc.Radius, aArc.Y, aRadius: aArc.Radius), new uopVector(aArc.X, aArc.Y + aArc.Radius, aRadius: aArc.Radius), new uopVector(aArc.X - aArc.Radius, aArc.Y, aRadius: aArc.Radius), new uopVector(aArc.X, aArc.Y - aArc.Radius, aRadius: aArc.Radius));
            }
            
            Init(new USHAPE(aName), aVertices: verts);
            
        }

        internal uopShape(URECTANGLE aRectangle, string aName = null, string aTag = null,string aFlag = null, string aHandle = null, int? aOccurance = null, int? aPartIndex = null, int? aRow = null, int? aCol = null)
        {
            Init(aRectangle: aRectangle);
            if (aTag != null) Tag = aTag;
            if (aFlag != null) Flag = aFlag;
            if (aName != null) Name = aName;
            if (aHandle != null) Handle = aHandle;
            if (aOccurance.HasValue) OccuranceFactor = aOccurance.Value;
            if (aPartIndex.HasValue)  PartIndex = aPartIndex.Value;
            if (aRow.HasValue) Row = aRow.Value;
            if (aCol.HasValue) Col = aCol.Value;

        }

        public uopShape(IEnumerable<iVector> aVerts, string aName = "", string aTag = "",bool? bOpen = null) => Init(new USHAPE(aName,Tag), aVertices: aVerts, bOpen:bOpen);


        internal void Init(USHAPE? aShape = null, uopShape bShape = null, IEnumerable<iVector> aVertices = null, bool? bOpen = null, URECTANGLE? aRectangle = null)
        {
            _IsRectangular = null;
            //Vertices = new
            // Segments = new USEGMENTS("");
            OccuranceFactor = 1;
            LinePair = new uopLinePair();
            _Vertices = new uopVectors();
            _Segments = null;
            Radius = 0;
            Tag = string.Empty;
            Value = 0;
            Elevation = 0;
            Depth = 0;
            Flag = string.Empty;
            Name = string.Empty;
            Factor = 1;
            Open = false;
            IsCircle = false;
            Index = 0;
            Handle = string.Empty;
            PartIndex = 0;
            IsVirtual = false;
            Row = 0;
            Col = 0;
            _Area = null;
            Suppressed = false;
            if (aShape.HasValue)
                Copy(aShape.Value);
            if (bShape != null)
                Copy(bShape);
           
            if (bOpen.HasValue) Open = bOpen.Value;
            if (aVertices != null)
            {
                Vertices.Populate(aVertices, true);
                Update();
            }
            if (aRectangle.HasValue)
            {
                Vertices.Clear();
                Vertices.Append(aRectangle.Value.Corners);
                Open = false;
                Row = aRectangle.Value.Row; Col = aRectangle.Value.Col;
                Name = aRectangle.Value.Name;
                Tag = aRectangle.Value.Tag;
                Flag = aRectangle.Value.Flag;
               IsVirtual = aRectangle.Value.IsVirtual;
            }

        }

        public void Copy(iShape bShape)
        {
            if (bShape == null) return;
            _Vertices = new uopVectors(bShape.Vertices, bCloneMembers: true);
            Open = !bShape.Closed;
            if(bShape is uopShape)
            {
                uopShape ushape = (uopShape)bShape;
                OccuranceFactor = ushape.OccuranceFactor;
                   LinePair = new uopLinePair(ushape.LinePair);
                Radius = ushape.Radius;
                Tag = ushape.Tag;
                Value = ushape.Value;
                Elevation = ushape.Elevation;
                Depth = ushape.Depth;
                Flag = ushape.Flag;
                Name = ushape.Name;
                Factor = ushape.Factor;
                Open = ushape.Open;
                IsCircle = ushape.IsCircle;
                _IsRectangular = ushape._IsRectangular;
                Index = ushape.Index;
                Handle = ushape.Handle;
                PartIndex = ushape.PartIndex;
                IsVirtual = ushape.IsVirtual;
                Row = ushape.Row;
                Col = ushape.Col;
                _Area = ushape._Area;

            }
            Update();

        }
        internal void Copy(USHAPE aShape)
        {
            LinePair = new uopLinePair(aShape.LinePair);
            _Vertices = new uopVectors(aShape.Vertices);
            Radius = aShape.Radius;
            Tag = aShape.Tag;
            Value = aShape.Value;
            Elevation = aShape.Elevation;
            Depth = aShape.Depth;
            Flag = aShape.Flag;
            Name = aShape.Name;
            Factor = aShape.Factor;
            Open = aShape.Open;
            IsCircle = aShape.IsCircle;
          
            Index = aShape.Index;
            Handle = aShape.Handle;
            PartIndex = aShape.PartIndex;
            IsVirtual = aShape.IsVirtual;
            Row = aShape.Row;
            Col = aShape.Col;
            Suppressed = aShape.Suppressed;
            Update();
  
        }

        #endregion Constructors

        #region Properties
        public bool Suppressed { get; set; }
        public virtual int OccuranceFactor { get; set; }

        public bool Open { get; set; }
        public double Elevation { get; set; }

        public uopLinePair LinePair { get; set; }

        internal uopSegments _Segments;

        public uopSegments Segments { get { if (_Segments == null || _Segments.Count != Vertices.Count -1) Update(); return _Segments; } set => _Segments = value; }



        protected double? _Area;
        public virtual double Area
        {
            get
            {
                if(_Area.HasValue) return _Area.Value;
                double _rVal = 0;


                if (IsCircle)
                {
                    _rVal = Math.PI * Math.Pow(Radius, 2);
                }
                else
                {
                    uopSegments segs = Segments;
                    if (segs.Count <= 0) return _rVal;
                    _rVal = Vertices.Area();
                    foreach (iSegment seg in segs)
                    {
                        if (seg.IsArc)
                        {
                            uopArc arc = seg as uopArc;
                            //arcs are assume to be covex from the shape
                            _rVal += uopUtils.ArcArea(arc.Radius, arc.StartAngle, arc.EndAngle);
                        }
                    }


                }
                return _rVal;
            }
            set => _Area = value;
        }



        public int PartIndex { get; set; }

        public virtual int Row { get; set; }
        public virtual int Col { get; set; }


        public bool IsVirtual { get; set; }

        public double Value { get; set; }

        public double Factor { get; set; }

        public double Depth { get; set; }

        public virtual double Radius { get; set; }


        private uopRectangle _Bounds;
        /// <summary>
        /// the bounding rectangle of the shape
        /// </summary>

        public uopRectangle Bounds
        {
            get
            {
                _Bounds??= new uopRectangle(Segments.ExtentPoints());
                return _Bounds;
            }
        }

        /// <summary>
        /// the bounding rectangle of the shape
        /// </summary>
        internal virtual URECTANGLE BoundsV => new URECTANGLE(Bounds);

        /// <summary>
        /// the center of the  bounding rectangle of the shape
        /// </summary>
        public virtual uopVector Center => new uopVector(Bounds.Center) { Tag = Tag, Flag = Flag };


        internal uopVectors _Vertices;

        public uopVectors Vertices { get => _Vertices; set { _Vertices = new uopVectors(value, bCloneMembers: true); Update(); } }


        /// <summary>
        /// the width of the bounding rectangle of the shape
        /// </summary>

        public double Width => Bounds.Width;

        /// <summary>
        /// the height of the bounding rectangle of the shape
        /// </summary>
        public double Height => Bounds.Height;

        public string Tag { get; set; }

        public virtual string Name { get; set; }
        public virtual string Handle { get; set; }


        public string Flag { get; set; }
        public bool Mark { get; set; }

        private bool _IsCircle;
        public bool IsCircle { get => _IsCircle && Radius > 0; set => _IsCircle = value; }

        public int Index { get; set; }
        public virtual double X => Bounds.X;

        public virtual double Y => Bounds.Y;


        //the X of the left edge of bounding rectangle of the shape
        public double Left => Bounds.Left;

        //the X of the right edge of bounding rectangle of the shape
        public double Right => Bounds.Right;

        //the Y of the top edge of bounding rectangle of the shape
        public double Top => Bounds.Top;

        //the Y of the bottom edge of bounding rectangle of the shape
        public double Bottom => Bounds.Bottom;

        public bool HasArcs => Vertices.FindAll((x) => x.Radius != 0).Count > 0;
        public bool IsDefined { get { bool _rVal = Vertices.Count > 1; if (_rVal && Segments.Count < Vertices.Count - 1) Update(); return _rVal; } }

        #endregion Properties

        #region Methods

        public uopVectors ExtentPoints(int? aCurveDivisions = null) => Segments.ExtentPoints(aCurveDivisions);
   
        /// <summary>
        /// moves the shape per the passed distances
        /// </summary>
        /// <param name="dX"></param>
        /// <param name="dY"></param>
        public virtual void Move(double dX = 0, double dY = 0)
        {
            if(dX == 0 && dY == 0) return;
            Vertices.Move(dX, dY);
            LinePair?.Move(dX, dY);
            Update();
        }

        public virtual bool Mirror(double? aX, double? aY)
        {
            if (!aX.HasValue && !aY.HasValue) return false;
             bool _rVal =   Vertices.Mirror(aX, aY);
            LinePair?.Mirror(aX, aY);
            if(_rVal) Update();
            return _rVal;
           
        }

        public virtual void Translate(uopVector aTranslation)
        {
            if (aTranslation == null || aTranslation.IsNull() ) return;
            Move(aTranslation.X, aTranslation.Y);

        }

        public void Project(uopVector aDirection, double aDistance, bool bSuppressNormalize = false, bool bInvertDirection = false)
        { 
            if(aDirection == null || aDistance == 0) return;
            if (!bSuppressNormalize) aDirection.Normalize();
            Translate( aDirection * aDistance);
            
        }
        /// <summary>
        /// moves the shape per the passed distances
        /// </summary>
        /// <param name="dX"></param>
        /// <param name="dY"></param>
        public uopShape Moved(double dX = 0, double dY = 0)
        {
            uopShape _rVal = new uopShape(this);
            _rVal.Move(dX, dY);
            return _rVal;
        }
        /// <summary>
        /// rotates the shape around the passed point
        /// </summary>
        /// <remarks>If null is passed the center of the shape is assummed</remarks>
        /// <param name="aOrigin">the point to rotate about</param>
        /// <param name="aAngle">the angle to apply</param>
        /// <param name="bInRadians">flag indicating the passed angle is expressed in radians</param>
        public void Rotate(iVector aOrigin, double aAngle, bool bInRadians = false)
        {
            if (Math.Abs(aAngle) <= 0.00000001) return;
            
            uopVector org = aOrigin == null ? Center : new uopVector(aOrigin);
            Vertices.Rotate(org, aAngle, bInRadians);
            LinePair?.Rotate(org, aAngle, bInRadians);
            Update();

        }


        /// <summary>
        /// returns a clone of the shape rotated around the passed point
        /// </summary>
        /// <remarks>If null is passed the center of the shape is assummed</remarks>
        /// <param name="aOrigin">the point to rotate about</param>
        /// <param name="aAngle">the angle to apply</param>
        /// <param name="bInRadians">flag indicating the passed angle is expressed in radians</param>
        public uopShape Rotated(iVector aOrigin, double aAngle, bool bInRadians = false)
        {
            uopShape _rVal = new uopShape(this);
            _rVal.Rotate(aOrigin, aAngle, bInRadians);
            return _rVal;

        }
        public override string ToString() => string.IsNullOrWhiteSpace(Name) ? $"uopShape[{Vertices.Count}]" : $"uopShape[{Vertices.Count}] '{Name}'";


        protected bool? _IsRectangular;
        public virtual  bool IsRectangular(bool bByDiagonal = false, int aPrecis = 3)
        {

            if(_IsRectangular.HasValue) return _IsRectangular.Value;
            aPrecis = mzUtils.LimitedValue(aPrecis, 0, 15)
;            uopVectors verts = Vertices;
            if (verts.Count != 4) return false;
            if (verts.FindAll((x) => x.Radius != 0).Count > 0) return false;

            double d1 = (verts.Item(1) - verts.Item(3)).Length(aPrecis);
            double d2 = (verts.Item(2) - verts.Item(4)).Length(aPrecis);
            bool _rVal = d1 == d2;
            if (!bByDiagonal && _rVal)
            {
                d1 = Math.Round(verts.Item(1).X - verts.Item(2).X, aPrecis);
                d2 = Math.Round(verts.Item(1).Y - verts.Item(2).Y, aPrecis);
                if (d1 != 0 & d2 != 0)
                { //the first edge is vertical or horizontal
                    return false;
                }
                d1 = Math.Round(verts.Item(2).X - verts.Item(3).X, aPrecis);
                d2 = Math.Round(verts.Item(2).Y - verts.Item(3).Y, aPrecis);
                if (d1 != 0 & d2 != 0)
                { //the second edge is vertical or horizontal
                    return false;
                }
                d1 = Math.Round(verts.Item(3).X - verts.Item(4).X, aPrecis);
                d2 = Math.Round(verts.Item(3).Y - verts.Item(4).Y, aPrecis);
                if (d1 != 0 & d2 != 0)
                { //the third edge is vertical or horizontal
                    return false;
                }
                d1 = Math.Round(verts.Item(4).X - verts.Item(1).X, aPrecis);
                d2 = Math.Round(verts.Item(4).Y - verts.Item(1).Y, aPrecis);
                if (d1 != 0 & d2 != 0)
                { //the forth edge is vertical or horizontal
                    return false;
                }
            }
            return _rVal;
        }

        public dxePolyline ToDXFPolyline(string aLayerName = "", dxxColors aColor = dxxColors.Undefined, string aLinetype = "", string aTag = null, string aFlag = null)
        {

            dxePolyline _rVal = new dxePolyline(Vertices, !Open);
            _rVal.LCLSet(aLayerName, aColor, aLinetype);

            if (aTag == null)
            {
                aTag = Tag;
                if (aTag ==  string.Empty) aTag = Name;
            }


            _rVal.TFVSet(aTag, aFlag, Value);
            if (Elevation != 0) _rVal.Move(0, 0, Elevation);

            return _rVal;
        }

        public dxfEntity ToDXFEntity(string aLayerName = "", dxxColors aColor = dxxColors.Undefined, string aLinetype = "")
        {
            if (!IsCircle) return ToDXFPolyline(aLayerName, aColor, aLinetype);
            dxeArc circle = new dxeArc(Center, Radius);
            circle.LCLSet(aLayerName, aColor, aLinetype);
            return circle;
        }



        public dxePolyline Perimeter(string aLayerName = "", dxxColors aColor = dxxColors.Undefined, string aLinetype = "") => ToDXFPolyline(aLayerName, aColor, aLinetype);


        public virtual uopRectangle Limits(double aWidthAdder = 0, double aHeightAdder = 0, string aTag = null) => new uopRectangle(BoundsV, aWidthAdder, aHeightAdder, aTag == null?Tag: aTag,  Row,  Col);


        public virtual uopShape Clone() => new uopShape(this);

        object ICloneable.Clone() => (object)new uopShape(this);

        public uopLines Lines(bool bGetClones = false, double? aLength = null, int? aPrecis = 4) => Segments.LineSegments(bGetClones, aLength, aPrecis);
       
        public int ArcCount(double? aRadius = null, int? aPrecis = 4) => Segments.ArcCount(aRadius,aPrecis);

        public int LineCount(double? aLength = null, int? aPrecis = 4) => Segments.LineCount(aLength,aPrecis);

        public List<uopArc> Arcs(bool bGetClones = false, double? aRadius = null, int? aPrecis = 4) => Segments.ArcSegments(bGetClones, aRadius,aPrecis);
        

        public  virtual void Update()
        {
            UpdateSegments();  
        }
        protected uopSegments UpdateSegments() 
        { 
            _Segments = Vertices.ToSegments(Open);  
            uopVectors exnts = _Segments.ExtentPoints(); 
            _Bounds = new uopRectangle(exnts); 
            return _Segments; 
        }

        public bool Contains(iArcRec aArcRec, bool bOnIsIn = true, int aPrecis = 4, bool bMustBeCompletelyWithin = false, bool bReturnTrueByCenter = false) => uopArcRecs.ArcRecContainsArcRec(this, aArcRec, out _, out _, bOnIsIn, aPrecis, bMustBeCompletelyWithin, bReturnTrueByCenter);

        public bool Contains(iArcRec aArcRec, out bool rCoincindent, out bool rIntersects, bool bOnIsIn = true, int aPrecis = 4, bool bMustBeCompletelyWithin = false, bool bReturnTrueByCenter = false) => uopArcRecs.ArcRecContainsArcRec(this, aArcRec, out rCoincindent, out rIntersects, bOnIsIn, aPrecis, bMustBeCompletelyWithin, bReturnTrueByCenter);

        /// <summary>
        /// returns true if the passed vector lies on the arc
        /// </summary>
        /// <param name="aVector"> the vector to test</param>
        /// <param name="aPrecis"> a precision apply</param>
        /// <param name="bTreatAsInfinite"> flag to treat the arc as 360 degrees</param>
        /// <returns></returns>
        public bool ContainsVector(iVector aVector, bool bOnIsIn = true, int aPrecis = 5, bool bTreatAsInfinite = false)
        => aVector == null ? false : ContainsVector(aVector, bOnIsIn, aPrecis, out bool _, out bool _, out bool _, out bool _, bTreatAsInfinite: bTreatAsInfinite);

        /// <summary>
        /// returns true if the passed vector lies on the arc or withing the arc
        /// </summary>
        /// <param name="aVector" >the vector to test</param>
        /// <param name="aPrecis">a precision apply</param>
        /// <param name="bOnIsIn" >flag treat vectors on the path as out of the circle </param>
        /// <param name="rWithin">returns true if the passed vector is within the arc. if true, the endpoint checks are not performed</param>
        /// <param name="rIsOn" >returns true of the vecor lies on the path of the arc</param>
        /// <param name="rIsStartPt"> returns true if the passed vector is the start vector of the arc</param>
        /// <param name="rIsEndPt" >returns true if the passed vector is the end vector of the arc</param>
        /// <param name="bTreatAsInfinite" >flag to treat the arc as 360 degrees . if true, the endpoint checks are not performed</param>
        public bool ContainsVector(iVector aVector, bool bOnIsIn, int aPrecis, out bool rWithin, out bool rIsOn, out bool rIsStartPt, out bool rIsEndPt, bool bTreatAsInfinite = true)
        {
         
            rIsStartPt = false;
            rIsEndPt = false;
            rIsOn = false;
            rWithin = false;
            if (Vertices.Count == 0) return false;

            iArcRec iMe = this as iArcRec;

            if(iMe.Type == uppArcRecTypes.Arc)
                return iMe.Arc.ContainsVector(aVector,bOnIsIn,aPrecis,out rWithin,out rIsOn,out rIsStartPt,out rIsEndPt,bTreatAsInfinite);
            else if (iMe.Type == uppArcRecTypes.Rectangle)
                return iMe.Rectangle.ContainsVector(aVector, bOnIsIn,  out rWithin, out rIsOn, out _, aPrecis);
          

            aPrecis = mzUtils.LimitedValue(aPrecis, 0, 15);
            

            double rad = Math.Round(Vertices.BoundingCircle().Radius, aPrecis);

            double d1 = Math.Round(Math.Sqrt(Math.Pow(X - aVector.X, 2) + Math.Pow(Y - aVector.Y, 2)), aPrecis);
          if(d1 > rad) return false;

          uopVector v1 = uopVector.FromVector(aVector);
            uopLine bisector = new uopLine() { sp = v1, ep = new uopVector(v1.X + 3 * rad, v1.Y)};
            uopVectors ips = bisector.Intersections(Segments, false, false);
            rWithin = ips.Count > 0 && mzUtils.IsOdd(ips.Count);

            return rWithin;

        }
        #endregion Methods

        #region iArcRec Implementation

        uppArcRecTypes iArcRec.Type => IsCircle ? uppArcRecTypes.Arc : IsRectangular(true, 3) ? uppArcRecTypes.Rectangle: uppArcRecTypes.Undefined ;

      
        uopArc iArcRec.Arc => IsCircle ? new uopArc(Radius, Center) { Tag = Tag, Flag = Flag} : null ;
        uopRectangle iArcRec.Rectangle => IsRectangular(true, 3) ? new uopRectangle(this) : null ;
        iArcRec iArcRec.Clone() => Clone();
        uopShape iArcRec.Slot => null;

        double iArcRec.Rotation => 0;

        #endregion iArcRec Implementation

        #region Shared Methods

        public static uopShape CloneCopy(uopShape aShape) => aShape == null ? null : new uopShape(aShape);
        public static uopShape Circle(iVector aCenter, double aRadius, string aTag = "",  string aFlag = "")
        {
            uopVector u0 = new uopVector(aCenter) { Radius = Math.Abs(aRadius)};
            uopVector u1 = u0.Moved( u0.Radius,0);
            uopVector u2 = u0.Moved(0, u0.Radius);
            uopVector u3 = u0.Moved(- u0.Radius,0);
            uopVector u4 = u0.Moved(0, -u0.Radius);

           return new uopShape(new uopVectors(u1, u2, u3, u4), bOpen: false) { Radius = u0.Radius, IsCircle = true, Tag = aTag, Flag = aFlag};
        }

        public static uopShape RoundSlot(iVector aCenter, double aRadius, double aLength, double aRotation, string aTag = "", string aFlag = "")
        {
            aLength = Math.Abs(aLength);
            aRadius = Math.Abs(aRadius);

            if (aLength <= aRadius * 2) 
            {
                uopVector u0 = new uopVector(aCenter) { Radius = Math.Abs(aRadius) };
                uopVector u1 = u0.Moved(u0.Radius, 0);
                uopVector u2 = u0.Moved(0, u0.Radius);
                uopVector u3 = u0.Moved(-u0.Radius, 0);
                uopVector u4 = u0.Moved(0, -u0.Radius);

                return new uopShape(new uopVectors(u1, u2, u3, u4), bOpen: false) { Radius = u0.Radius, IsCircle = true, Tag = aTag, Flag = aFlag };
            }
            else
            {
                uopVector xdir = new uopVector(1, 0);
                uopVector ydir = new uopVector(0, 1);
                if (aRotation != 0) { xdir.Rotate(aRotation); ydir.Rotate(aRotation); }

                double l1 = 0.5 * aLength;
                double l2 = l1 - aRadius;
                uopVector u0 = new uopVector(aCenter);
                uopVectors verts = new uopVectors() { u0 - xdir * l2 + ydir * aRadius, u0 - xdir * l1 , u0 - xdir * l2 - ydir * aRadius, u0 + xdir * l2 - ydir * aRadius, u0 + xdir * l1 , u0 + xdir * l2 + ydir * aRadius };

                verts[0].Radius = aRadius;
                verts[1].Radius = aRadius;
                verts[3].Radius = aRadius;
                verts[4].Radius = aRadius;

                return new uopShape(verts, bOpen: false) { Tag = aTag, Flag = aFlag };
            }
            
        }

        public static uopShape Rectangle(iVector aCenter, double aWidth, double aHeight, double aRotation = 0, string aTag = "", string aFlag = "")
        {
            uopVector u0 = new uopVector(aCenter);
            uopVector xdir = new uopVector(1, 0, 0);
            uopVector ydir = new uopVector(0, 1, 0);

            if(aRotation != 0) { xdir.Rotate(aRotation); ydir.Rotate(aRotation); }

            uopVector u1 = u0 + xdir * -(aWidth / 2) + ydir * (aHeight / 2);
            uopVector u2 = u0 + xdir * -(aWidth / 2) + ydir * -(aHeight / 2);
            uopVector u3 = u0 + xdir * (aWidth / 2) + ydir * -(aHeight / 2);
            uopVector u4 = u0 + xdir * (aWidth / 2) + ydir * (aHeight / 2);

            return new uopShape(new uopVectors(u1, u2, u3, u4), bOpen: false){ _IsRectangular = true, Tag = aTag, Flag = aFlag };
        }

        /// <summary>
        ///creates the vertices of a polyline that is a section of the indicated circle base on the passed rectangle.
        /// </summary>
        /// <param name="aCenter">the center of the circle</param>
        /// <param name="aRadius">the radius of the circle</param>
        /// <param name="aLimits">the rectangle to use to create the bounds of the circle section</param>
        /// <param name="aRotation">a rotation to apply</param>
        /// <returns></returns>
        public static uopVectors CircleSectionVertices(iVector aCenter, double aRadius, uopRectangle aLimits, double aRotation = 0) => CircleSectionVertices(new uopArc(aRadius,aCenter), aLimits,aBisector:null,bBisector: null,aPrecis: 3,bIncludeQuadrantPts: true);

        /// <summary>
        ///creates a shape section of the indicated circle base on the passed rectangle.
        /// </summary>
        /// <param name="aArc">the subect circle</param>
        /// <param name="aLimits">the rectangle to use to create the bounds of the circle section</param>
        /// <param name="aBisector">a line to add additional points which bisect the circle and the rectangle</param>
        /// <param name="bBisector">a line to add additional points which bisect the circle and the rectangle</param>
        /// <param name="aPrecis">the precision to apply for vector comparisons</param>
        /// <param name="bIncludeQuadrantPts">a flag to include the quadrant points of the circle if the fall withing the limits</param>
        /// <param name="aName">a name to assign to the returned vectors</param>
        /// <returns></returns>
        public static uopCompoundShape CircleSection(iArc aArc, iRectangle aLimits, iLine aBisector = null, iLine bBisector = null, int aPrecis = 3, bool bIncludeQuadrantPts = true, string aName = "")
        {

            try
            {
                if (aArc == null || aLimits == null) throw new ArgumentNullException("Invalid Input");

                uopArc arc = uopArc.FromArc(aArc, true, 0, 360);
                uopRectangle rectangle = uopRectangle.FromRectangle(aLimits);

                double rad = arc.Radius;

                if (rad == 0) { throw new Exception("Invalid Radius Passed."); }

                return new uopCompoundShape(CircleSectionVertices(aArc,aLimits,aBisector,bBisector,aPrecis,bIncludeQuadrantPts, aName:aName), aName, bOpen:false) ;
            }
            catch (Exception e)
            {
                throw new Exception("[uopShape.CircleSection] " + e.Message);
            }
        }
        /// <summary>
        ///creates the vertices of a polyline that is a section of the indicated circle base on the passed rectangle.
        /// </summary>
        /// <param name="aArc">the subect circle</param>
        /// <param name="aLimits">the rectangle to use to create the bounds of the circle section</param>
        /// <param name="aBisector">a line to add additional points which bisect the circle and the rectangle</param>
        /// <param name="bBisector">a line to add additional points which bisect the circle and the rectangle</param>
        /// <param name="aPrecis">the precision to apply for vector comparisons</param>
        /// <param name="bIncludeQuadrantPts">a flag to include the quadrant points of the circle if the fall withing the limits</param>
        /// <param name="aName">a name to assign to the returned vectors</param>
        /// <returns></returns>
        public static uopVectors CircleSectionVertices(iArc aArc,  iRectangle aLimits, iLine aBisector = null, iLine bBisector = null, int aPrecis = 3, bool bIncludeQuadrantPts = true, uopVector aBisector1TestDir = null, uopVector aBisector2TestDir = null, string aName = "")
        {
           
            try
            {
                if (aArc == null || aLimits == null) throw new ArgumentNullException("Invalid Input");

                uopArc arc =  uopArc.FromArc(aArc,true,0,360);
                uopRectangle rectangle = uopRectangle.FromRectangle(aLimits);

                double rad = arc.Radius;
                int prec = mzUtils.LimitedValue(aPrecis, 2, 15);

                if (rad == 0) { throw new ArgumentOutOfRangeException("Invalid Radius Passed."); }

                uopVectors corners = rectangle.Corners;
                uopLines edges = corners.LineSegments(true);
                uopVectors ipoints = uopVectors.Zero;
                double testrad = Math.Round(rad, prec);

                ipoints.Append(corners.FindAll(x => x.DistanceTo(arc.Center, prec) <= testrad));
                uopVectors edgeips = edges.Intersections(arc, aLinesAreInfinite: false, aArcIsInfinite: true);
                ipoints.Append(edgeips, aTag: "EDGES", bNoDupes: true, aNoDupesPrecis: prec);
//                ipoints.Append(edgeips.FindAll(x => x.DistanceTo(arc.Center, prec -1) <= testrad), aTag:"EDGES", bNoDupes:true, aNoDupesPrecis: prec);

                if (bIncludeQuadrantPts)
                {
                    ipoints.Append(arc.QuadrantPoints().FindAll(x => rectangle.Contains(x,bOnIsOut: false, aPrecis: prec)), aTag: "QUADRANT", bNoDupes:true, aNoDupesPrecis: prec);
                }
                uopLine line1 = uopLine.FromLine(aBisector);
                uopLine line2 = uopLine.FromLine(bBisector);
                if(aBisector1TestDir != null) aBisector1TestDir.Normalize();
                if (aBisector2TestDir != null) aBisector2TestDir.Normalize();

                uopVector dir1 = aBisector1TestDir == null ? rectangle.Center.DirectionTo(line1, out _): aBisector1TestDir;
                uopVector dir2 = aBisector2TestDir == null ? rectangle.Center.DirectionTo(line2, out double b2) : aBisector2TestDir;

                if (line1 != null)
                {
                    uopVectors ips = edges.Intersections(line1, true, true);
                    ipoints.Append(ips.FindAll(x => x.DistanceTo(arc.Center, prec) <= testrad), aTag: "BISECTOR-EDGES", bNoDupes: true, aNoDupesPrecis: prec);
                    ipoints.Append(line1.Intersections(arc, true, true).FindAll(x => rectangle.Contains(x, bOnIsOut: false, aPrecis: prec)), aTag: "BISECTOR-ARC", bNoDupes: true, aNoDupesPrecis: prec);
                }
                if (line2 != null)
                {
                    ipoints.Append(line2.Intersections(arc, true, true).FindAll(x => rectangle.Contains(x, bOnIsOut: false, aPrecis: prec)), aTag: "BISECTOR-ARC", bNoDupes: true, aNoDupesPrecis: prec);
                    ipoints.Append(edges.Intersections(line2, true, true).FindAll(x => x.DistanceTo(arc.Center, prec) <= testrad), aTag: "BISECTOR-EDGES", bNoDupes: true, aNoDupesPrecis: prec);
                }

                uopVectors discards = uopVectors.Zero;
                uopVectors internals = uopVectors.Zero;
                //List<uopVector> ips = ipoints.FindAll(x => x.DistanceTo(arc.Center, prec) <= testrad);
                //ips = ips.FindAll(x => x.DistanceTo(arc.Center, prec) <= testrad);
                foreach (var pt in ipoints)
                {
                    //double d1 = pt.DistanceTo(arc.Center, prec);
                    //if (d1 > testrad) continue;

                    pt.Flag = string.Empty;
                    if (!pt.Tag.Contains( "TOP") && !pt.Tag.Contains("BOTTOM")  && !rectangle.Contains(pt, bOnIsOut: false, aPrecis: prec))
                        pt.Flag = "RECTANGLE BOUNDS";

                    if (pt.Flag == string.Empty &&  line1 != null  && !pt.Tag.StartsWith("BISECTOR"))
                    {
                        uopVector dir = pt.DirectionTo(line1, out double b3);
                        if (!dir.IsNull(prec) &&  !dir1.IsEqual(dir, prec, false))
                            pt.Flag = "BISECTOR1_VIOLATION";
                    }
                    if (pt.Flag == string.Empty && line2 != null && !pt.Tag.StartsWith("BISECTOR"))
                    {
                        uopVector dir = pt.DirectionTo(line2, out double b3);
                        if (!dir.IsNull(prec) && !dir2.IsEqual(dir, prec, false)) 
                            pt.Flag = "BISECTOR2_VIOLATION";
                    }
                    if (pt.Flag == string.Empty)
                        internals.Add(pt);
                    else
                        discards.Add(pt);
                }

                if(internals.Count > 0)
                {
                    uopRectangle bounds = new uopRectangle(internals);
                    internals.Sort(dxxSortOrders.CounterClockwise, bounds.Center, prec);
                }
                

                internals.Circularize(rad, 1,arc.Center, bDontSort:true);

                internals.Name = aName;
                return internals;
            }
            catch (Exception e)
            {
                throw new Exception("[uopShape.CircleSectionVertices] " + e.Message);
            }
        }

           public static uopShape CircleSection(double aCenterX, double aCenterY, double aRadius, double aRotation = 0, double? aLeftEdge = null, double? aRightEdge = null, double? aTopEdge = null, double? aBottomEdge = null,  string aName = "")
        {
            uopVectors verts = null;
            uopShape _rVal = new uopShape() { Name = aName};
            try
            {

                aRadius = Math.Abs(aRadius);
                double rad = aRadius;

                if (rad == 0) { throw new Exception("Invalid Radius Passed."); }
                uopVector cp = new uopVector(aCenterX, aCenterY);
                uopRectangle limits = new uopRectangle(aLeftEdge.HasValue ? aLeftEdge.Value : cp.X - rad - 2, aTopEdge.HasValue ? aTopEdge.Value : cp.Y + rad + 2, aRightEdge.HasValue ? aRightEdge.Value : cp.X + rad + 2, aBottomEdge.HasValue ? aBottomEdge.Value : cp.Y - rad - 2);
                verts =  uopShape.CircleSectionVertices(cp, aRadius, limits, aRotation);
                _rVal.Vertices.AddRange(verts);
                _rVal.Update();

                return _rVal;
            }
            catch (Exception e)
            {
                throw new Exception("[uopShape.CircleSection] " + e.Message);
                //return _rVal;
            }

        }
        public static bool Compare(uopShape A, uopShape B, int? aPrecis = 4)
        {
            if (A == null && B == null) return true;
            if (A == null || B == null) return false;
            if (A == B) return true;
            int precis = !aPrecis.HasValue ? 15 : mzUtils.LimitedValue(aPrecis.Value, 0, 15);
            if ((Math.Round(A.Height, precis) != Math.Round(B.Height, precis)) || (Math.Round(A.Width, precis) != Math.Round(B.Width, precis))) return false;
            if (A.ArcCount() != B.ArcCount()) return false;
            List<iSegment> asegs = A.Segments.ToList();
            List<iSegment> bsegs = B.Segments.ToList();
            if (asegs.Count != asegs.Count) return false;

            for(int i = 1; i <= asegs.Count; i++)
            {
                iSegment aseg = asegs[i -1];
                iSegment bseg = bsegs[i - 1];
                if (!uopSegments.Compare(aseg, bseg, precis)) return false;
            }

            return true;
        }
        #endregion SharedMethods
    }
}
