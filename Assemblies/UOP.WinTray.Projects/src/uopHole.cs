using System;
using System.Runtime.CompilerServices;
using System.Windows.Media.Media3D;
using UOP.DXFGraphics;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Interfaces;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;


namespace UOP.WinTray.Projects
{
    public class uopHole : iVector, iArcRec
    {
     
        private uopVector _Center;
        #region Constructors

        public uopHole() => Init();
        

        public uopHole(uopHole aHoleToCopy)  { Init(); Copy(aHoleToCopy);   }

        internal uopHole(UHOLE aHole) 
        {
            Init();
            _Center = new uopVector(aHole.Center) { Tag = aHole.Tag };
            Rotation = aHole.Rotation;
            Elevation = aHole.Elevation;
            Radius = Math.Abs(aHole.Radius);
            Length = Math.Abs(aHole.Length);
            Depth = Math.Abs(aHole.Depth);
            Name = aHole.Name;
            Index = aHole.Index;
            ExtrusionDirection = aHole.ZDirection;
            Value = aHole.Value;
            Tag = aHole.Tag;
            Flag = aHole.Flag;
            WeldedBolt = aHole.WeldedBolt;
            IsSquare = aHole.IsSquare;
            MinorRadius = aHole.MinorRadius;
            CornerRadius = aHole.CornerRadius;
            Inset = aHole.Inset;
            DownSet = aHole.DownSet;
        }

        public uopHole(double aDiameter = 0, double aX = 0, double aY = 0, double aLength = 0, string aTag = "", double aDepth = 0,
    double aRotation = 0, double? aElevation = null, string aZDirection = "0,0,1", string aFlag = "", double aInset = 0,
    double aDownSet = 0, double aMinorRadius = 0, bool bWeldedBolt = false, bool bIsSquare = false, double aCornerRadius = 0)

        {
            Init();
        
            _Center = new uopVector(aX, aY, aElevation: aElevation);
            Rotation = aRotation;
            Elevation = aElevation;
            Radius = Math.Abs(aDiameter) / 2;
            Length = Math.Abs(aLength);
            Depth = Math.Abs(aDepth);
            Name = string.Empty;
            Index = 0;
            ExtrusionDirection = aZDirection;
            Value = 0;
            Tag = aTag;
            Flag = aFlag;
            WeldedBolt = bWeldedBolt;
            IsSquare = bIsSquare;
            MinorRadius = aMinorRadius;
            CornerRadius = aCornerRadius;
            Inset = aInset;
            DownSet = aDownSet;
        
            WeldedBolt = bWeldedBolt;
        }

        private void Init()
        {
            _Center = uopVector.Zero;

            Rotation = 0;
            Elevation = null;
            Radius = 0;
            Length = 0;
            Depth = 0;
            Tag = string.Empty;
            Name = string.Empty;
            Index = 0;
            _ZDirection = "0,0,1";
            Value = 0;
            Flag = string.Empty;
            WeldedBolt = false;
            IsSquare = false;
            MinorRadius = 0;
            CornerRadius = 0;
        }
        #endregion Constructors

        #region Properties

        public double? Elevation { get => _Center.Elevation; set => _Center.Elevation = value; }

        public double Area => uopUtils.HoleArea(Radius, MinorRadius, Length, IsSquare);



        public uppHoleTypes HoleType => (Length > Diameter) ? uppHoleTypes.Slot : uppHoleTypes.Hole;

        public string Name { get; set; }

        public int Index { get; set; }

        public double X { get => _Center.X; set => _Center.X = value; }

        public double Y { get => _Center.Y; set => _Center.Y = value; }

        public double Z { get => Elevation.HasValue ? Elevation.Value : 0; set => _Center.Elevation = value; }

        public double Diameter { get => 2 * Radius; set => Radius = value/2; }

        private double _Length;
        public double Length { get => _Length; set => _Length = Math.Abs(value); }
     
        public uopVector Center
        {
            get { _Center ??= uopVector.Zero; _Center.Tag = Tag; _Center.Flag = Flag; return _Center; }
            set
            {
                if (value == null)
                {
                    _Center.SetCoordinates(0, 0, 0);
                }
                else
                {
                    _Center.SetCoordinates(value.X, value.Y, value.Elevation);
                }
            }
        }

        public dxfVector CenterDXF
        {
            get => _Center.ToDXFVector(aZ: Elevation, aTag: Tag, aFlag: Flag);

            set
            {
                if (value != null)
                {
                    _Center = new uopVector( UVECTOR.FromDXFVector(value));
                   
                }
                else
                {
                    X = 0;
                    Y = 0;
                    Elevation = 0;
                }

            }
        }


        private double _CornerRadius;
        public double CornerRadius { get => _CornerRadius; set => _CornerRadius = Math.Abs(value); }


        private double _MinorRadius;
        public double MinorRadius { get => _MinorRadius; set => _MinorRadius = Math.Abs(value); }

        private double _Rotation;
        public double Rotation { get => _Rotation; set => _Rotation = mzUtils.NormAng( value, ThreeSixtyEqZero: true); }

        private double _Depth;
        public double Depth { get => _Depth; set => _Depth = Math.Abs(value); }

        public double Inset { get; set; }

        public double DownSet { get; set; }

      
        public string Tag { get; set; }

        public double Value { get; set; }

        public string Flag { get; set; }

        public bool WeldedBolt { get; set; }

        public bool IsSquare { get; set; }
        public bool Suppressed { get => _Center.Suppressed; set => _Center.Suppressed = value; }

        public dxfDirection ZDirection
        {
            get => new dxfDirection(ExtrusionDirection);
            set => ExtrusionDirection = value == null ? "0,0,1" : $"{value.X},{ value.Y },{ value.Z}";
        }

        private double _Radius;
        public double Radius { get => _Radius; set => _Radius = Math.Abs(value); }
        public bool IsElongated => Length > Diameter;


        private string _ZDirection;
        public string ExtrusionDirection
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_ZDirection)) _ZDirection = "0,0,1";
                return _ZDirection;
            }
            set
            {
                value ??= string.Empty;
                value = value.Trim();
                if (string.IsNullOrWhiteSpace(value)) value = "0,0,1";
                
                dxfDirection v1 = new dxfDirection(value);
                _ZDirection = $"{v1.X },{ v1.Y },{ v1.Z}";
            }
        }
     
        internal URECTANGLE BoundaryRectangle
        {
            get
            {
                URECTANGLE _rVal = URECTANGLE.Null;

                uopHole.GetData(this, out double rad, out double mrad, out double lng);
                _rVal.Define(X - lng / 2, X + lng / 2, Y + rad, Y - rad);
                return _rVal;

            }
        }

        public dxeHole ToDXFHole
        {
            get
            {
                return new dxeHole
                {
                    Center = CenterDXF,
                    Radius = Radius,
                    MinorRadius = MinorRadius,
                    Depth = Depth,
                    Rotation = Rotation,
                    DownSet = DownSet,
                    Inset = Inset,
                    IsSquare = IsSquare,
                    Length = Length,
                    WeldedBolt = WeldedBolt,
                    Tag = Tag,
                    Flag = Flag,

                    Value = Value
                };

            }

        }
        public string Dimensions
        {
            get
            {
                return IsElongated ? $"{Diameter:0.0###} x { Length:0.0###}" : $"{Diameter:0.0###}";
            }
        }

        /// <summary>
        ///returns the vertices that define the bounds of a hole on its plane
        /// </summary>
        /// <returns></returns>
        public colDXFVectors DXFVertices

        {
            get
            {
                colDXFVectors _rVal = new colDXFVectors();
                if (Radius <= 0) { return _rVal; }

                dxfPlane aPln = Plane(true);

                double dia = Diameter;
                double l1 = 0.5 * Length - Radius;
                if (IsSquare)
                {
                    //square hole
                    double rad = CornerRadius;
                    if (rad >= Length / 2) rad = 0;
                    double w = Length / 2;
                    double h = Diameter / 2;
                    if (rad <= 0)
                    {
                        _rVal.Add(aPln, -w, h);
                        _rVal.Add(aPln, -w, -h);
                        _rVal.Add(aPln, w, -h);
                        _rVal.Add(aPln, w, h);

                    }
                    else
                    {
                        _rVal.Add(aPln, -w + rad, h, aVertexRadius: rad);
                        _rVal.Add(aPln, -w, h - rad);
                        _rVal.Add(aPln, -w, -h + rad, aVertexRadius: rad);
                        _rVal.Add(aPln, -w + rad, -h);
                        _rVal.Add(aPln, w - rad, -h, aVertexRadius: rad);
                        _rVal.Add(aPln, w, -h + rad);
                        _rVal.Add(aPln, w, h - rad, aVertexRadius: rad);
                        _rVal.Add(aPln, w - rad, h);


                    }
                }
                else
                {
                    if (MinorRadius == 0)
                    {
                        if (Length == dia)
                        {
                            double aRad = Radius;
                            //Round hole
                            _rVal.Add(aPln, -Radius, aVertexRadius: aRad);
                            _rVal.Add(aPln, 0, -Radius, aVertexRadius: aRad);
                            _rVal.Add(aPln, Radius, aVertexRadius: aRad);
                            _rVal.Add(aPln, 0, Radius, aVertexRadius: aRad);
                        }
                        else
                        {
                            double aRad = Radius;
                            //Round end slot
                            dxfVector v1 = _rVal.Add(aPln, -l1, Radius, aVertexRadius: aRad);
                            _rVal.Add(aPln, -0.5 * Length, aVertexRadius: aRad);
                            _rVal.Add(aPln, -l1, -Radius);
                            _rVal.Add(aPln, l1, -Radius, aVertexRadius: aRad);
                            _rVal.Add(aPln, 0.5 * Length, aVertexRadius: aRad);
                            _rVal.Add(aPln, l1, Radius);
                        }
                    }
                    else
                    {
                        //Round hole with a flat
                        double ang = Math.Atan(Math.Sqrt(Math.Pow(Radius, 2) - Math.Pow(MinorRadius, 2)) / MinorRadius) * 180 / Math.PI;
                        double aRad = Radius;
                        _rVal.AddPlaneVectorPolar(aPln, ang, Radius, aVertexRadius: aRad);
                        _rVal.Add(aPln, 0, Radius, aVertexRadius: aRad);
                        _rVal.Add(aPln, -Radius, aVertexRadius: aRad);
                        _rVal.Add(aPln, 0, -Radius, aVertexRadius: aRad);
                        _rVal.AddPlaneVectorPolar(aPln, -ang, Radius, 0);
                    }
                }
                return _rVal;

            }
        }

        #endregion Properties

        #region Methods

        public dxfPlane Plane(bool Rotated = false) { dxfPlane _rVal = new dxfPlane(CenterDXF, aZDir: ZDirection); if (Rotation != 0 && Rotated) { _rVal.Rotate(Rotation); } return _rVal; }

        public void SetProps(double? aRadius = null, double? aLength = null, double? aDepth = null, double? aRotation = null, string aTag = null, string aExtrusionDirection = null, double? aMinorRadius = null, bool? bIsSquare = null, bool? bWeldedBolt = null)
        {
            if (aRadius.HasValue) Radius = Math.Abs(aRadius.Value);
            if (aLength.HasValue) Length = Math.Abs(aLength.Value);
            if (aMinorRadius.HasValue) MinorRadius = Math.Abs(aMinorRadius.Value);
            if (aDepth.HasValue) Depth = Math.Abs(aDepth.Value);

            if (bIsSquare.HasValue) IsSquare = bIsSquare.Value;
            if (bWeldedBolt.HasValue) WeldedBolt = bWeldedBolt.Value;
            if (aRotation.HasValue) Rotation = mzUtils.NormAng(aRotation.Value, false, true, true);

            if (!string.IsNullOrWhiteSpace(aExtrusionDirection)) ZDirection = new dxfDirection( aExtrusionDirection);

        }

        public void Copy(uopHole aHole)
        {
            if (aHole == null) return;
              _Center = new uopVector(aHole.X, aHole.Y);
            Rotation = aHole.Rotation;
            Elevation = aHole.Elevation;
            Radius = aHole.Radius;
            Length = aHole.Length;
            Depth = aHole.Depth;
            Name = aHole.Name;
            Index = aHole.Index;
            ExtrusionDirection = aHole.ExtrusionDirection;
            Value = aHole.Value;
            Tag = aHole.Tag;
            Flag = aHole.Flag;
            WeldedBolt = aHole.WeldedBolt;
            IsSquare = aHole.IsSquare;
            MinorRadius = aHole.MinorRadius;
            CornerRadius = aHole.CornerRadius;
            Inset = aHole.Inset;
            DownSet = aHole.DownSet;
            WeldedBolt = aHole.WeldedBolt;

        }


        public void SetCoordinates(double? aNewX = null, double? aNewY = null) { _Center.Update(aNewX, aNewY); }

        public override string ToString()
        {
            string _rVal;

            if (!IsElongated)
            {
                _rVal = IsSquare ? $"SQR. HOLE { Dimensions}" : $"HOLE { Dimensions }";
            }
            else
            {
                _rVal = IsSquare ? $"SQR. SLOT { Dimensions }" : $"SLOT { Dimensions}";

            }

            _rVal += $" X:{X:0.000}";
            _rVal += $" Y:{Y:0.000}";
if(Elevation.HasValue) _rVal += $" ELEV:{Elevation.Value:0.000}";

            string suf = string.Empty;
            if (!string.IsNullOrWhiteSpace(Name)) suf = Name.Trim();
            if (!string.IsNullOrWhiteSpace(Tag))
            {
                if (suf !=  string.Empty) suf += " :: ";
                suf += Tag.Trim();
            }

            if (!string.IsNullOrWhiteSpace(Flag))
            {
                if (suf !=  string.Empty) suf += " :: ";
                suf += Flag.Trim();
            }

            if (suf !=  string.Empty) _rVal += $" [{ suf }]";

            return _rVal;
        }

        /// <summary>
        /// returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>
        public uopHole Clone() => new uopHole(this);

        public colDXFEntities ToDXFEntities(string aLayerName = "", dxxColors aColor = dxxColors.Undefined, string aLinetype = "", double aHClineScale = 0, double aVClineScale = 0, double aDClineScale = 0, dxfImage aImage = null, dxxLinetypeLayerFlag aLTLSetting = dxxLinetypeLayerFlag.Undefined, bool bSuppressCenterPoints = false)
        {



            colDXFEntities _rVal = new colDXFEntities();
            dxfEntity aEnt = BoundingDXFEntity(aLayerName, aColor, aLinetype, out dxfRectangle rBounds);
            aEnt.UpdatePath(aImage: aImage);
            if (aEnt == null) return _rVal;
            if (aImage != null) { aImage.LinetypeLayers.ApplyTo(aEnt, aLTLSetting, aImage); }


            if (Math.Abs(rBounds.ZDirection.Z) == 1)
            {
                _rVal.Add(aEnt);
                if(!bSuppressCenterPoints) _rVal.AddPoint(aEnt, aHandlePt: dxxEntDefPointTypes.Center);

                if (aHClineScale > 0 || aVClineScale > 0)
                {
                    dxeLine cLn = new dxeLine() { LayerName = aLayerName, Color = dxxColors.ByLayer, Linetype = dxfLinetypes.Hidden };
                    if (aImage != null) { aImage.LinetypeLayers.ApplyTo(cLn, aLTLSetting, aImage); }
                    if (aHClineScale > 0)
                    {
                        double lng = rBounds.Width * aHClineScale / 2;
                        cLn.SetVectors(rBounds.Vector(-lng), rBounds.Vector(lng));

                        _rVal.Add(cLn, bAddClone: true);
                    }
                    if (aVClineScale > 0)
                    {
                        double lng = rBounds.Height * aVClineScale / 2;
                        cLn.SetVectors(rBounds.Vector(aY: -lng), rBounds.Vector(aY: lng));
                        _rVal.Add(cLn, bAddClone: true);
                    }
                }
            }
            else
            {
                if (Depth == 0) { return _rVal; }
                dxfPlane aPln = rBounds.Plane;
                dxfRectangle bRec = aEnt.ExtentPoints.BoundingRectangle(dxfPlane.World);
                _rVal.AddPoint(bRec.Center, aLayerName, dxxColors.ByLayer, dxfLinetypes.Continuous);

                dxeLine aLn = new dxeLine() { LayerName = aLayerName, Color = dxxColors.ByLayer, Linetype = dxfLinetypes.Hidden };

                if (aImage != null) { aImage.LinetypeLayers.ApplyTo(aLn, aLTLSetting, aImage); }
                double ht = Depth / 2;
                if (Math.Abs(aPln.ZDirection.X) == 1)
                {
                    double lng = bRec.Height / 2;
                    aLn.SetVectors(bRec.Vector(-ht, lng), bRec.Vector(ht, lng));
                    _rVal.Add(aLn, bAddClone: true);
                    aLn.SetVectors(bRec.Vector(-ht, -lng), bRec.Vector(ht, -lng));
                    _rVal.Add(aLn, bAddClone: true);
                    if (aDClineScale > 0)
                    {
                        lng = Depth * aDClineScale / 2;
                        dxeLine cLn = new dxeLine(bRec.Vector(-lng), bRec.Vector(lng)) { LayerName = aLayerName, Color = dxxColors.ByLayer, Linetype = dxfLinetypes.Hidden };
                        if (aImage != null) { aImage.LinetypeLayers.ApplyTo(cLn, aLTLSetting, aImage); }
                        _rVal.Add(cLn);
                    }
                }
                else if (Math.Abs(aPln.ZDirection.Y) == 1)
                {
                    double lng = bRec.Width / 2;
                    aLn.SetVectors(bRec.Vector(-lng, ht), bRec.Vector(-lng, -ht));
                    _rVal.Add(aLn, bAddClone: true);
                    aLn.SetVectors(bRec.Vector(lng, ht), bRec.Vector(lng, -ht));
                    _rVal.Add(aLn);
                    if (aDClineScale > 0)
                    {


                        lng = Depth * aDClineScale / 2;
                        dxeLine cLn = new dxeLine(bRec.Vector(aY: -lng), bRec.Vector(aY: lng)) { LayerName = aLayerName, Color = dxxColors.ByLayer, Linetype = dxfLinetypes.Hidden };
                        if (aImage != null) { aImage.LinetypeLayers.ApplyTo(cLn, aLTLSetting, aImage); }
                        _rVal.Add(cLn);
                    }
                }
            }
            return _rVal;
        }

        public dxfEntity BoundingEntity(string aLayerName = "", dxxColors aColor = dxxColors.Undefined, string aLinetype = "", double aScale = 1) => BoundingDXFEntity(aLayerName, aColor, aLinetype, out dxfRectangle Bnds, 1);

        public dxfEntity BoundingDXFEntity(string aLayerName, dxxColors aColor, string aLinetype, out dxfRectangle rBounds, double aScale = 1)
        {
            rBounds = new dxfRectangle();
            dxfEntity _rVal = null;

            uopHole.GetData(this, out double rad, out double mrad, out double lng, aScale);
            if (rad <= 0) return _rVal;
            double dia = 2 * rad;
            dxfPlane aPln = Plane(true);

            rBounds = aPln.Rectangle();

            if (mrad == 0 & lng == dia && !IsSquare)
            {
                dxeArc aAr = new dxeArc(new dxfVector(X, Y), rad, 0, 360, false, aPln);
                rBounds.SetDimensions(dia, dia);

                _rVal = aAr;
            }
            else
            {
                colDXFVectors verts = DXFVertices;
                dxePolyline aPl = new dxePolyline(verts, bClosed: true, aPlane: aPln);

                _rVal = aPl;
                if (mrad == 0)
                {
                    rBounds.SetDimensions(lng, dia);
                }
                else
                {
                    rBounds.Project(rBounds.XDirection, -0.5 * (rad - mrad));
                    rBounds.SetDimensions(rad + mrad, dia);
                }

            }
            _rVal.LCLSet(aLayerName, aColor, aLinetype);
            _rVal.TFVSet(Tag, Flag, Value);
            return _rVal;
        }


        public bool Contains(iArcRec aArcRec, bool bOnIsIn = true, int aPrecis = 4, bool bMustBeCompletelyWithin = false, bool bReturnTrueByCenter = false) => uopArcRecs.ArcRecContainsArcRec(this, aArcRec, out _, out _, bOnIsIn, aPrecis, bMustBeCompletelyWithin, bReturnTrueByCenter);

        public bool Contains(iArcRec aArcRec, out bool rCoincindent, out bool rIntersects, bool bOnIsIn = true, int aPrecis = 4, bool bMustBeCompletelyWithin = false, bool bReturnTrueByCenter = false) => uopArcRecs.ArcRecContainsArcRec(this, aArcRec, out rCoincindent, out rIntersects, bOnIsIn, aPrecis, bMustBeCompletelyWithin, bReturnTrueByCenter);

        public bool ContainsVector(iVector aVector, bool bOnIsIn = true, int aPrecis = 4, bool bInfinte = false) => ContainsVector(aVector, bOnIsIn, out _, out _, out _, aPrecis);

        public bool ContainsVector(iVector aVector, bool bOnIsIn, out bool rWithin, out bool rIsOn, out bool rIsCorner, int aPrecis = 4)
        {

            rWithin = false;
            rIsOn = false;
            rIsCorner = false;
        
            if (aVector == null) return false;
            aPrecis = mzUtils.LimitedValue(aPrecis, 0, 15, 4);
            iArcRec iMe = this as iArcRec;
            switch (iMe.Type)
            {
                case uppArcRecTypes.Arc:
                    return iMe.Arc.ContainsVector(aVector, bOnIsIn, aPrecis, out rWithin, out rIsOn, out _, out _, true);
                case uppArcRecTypes.Rectangle:
                    return iMe.Rectangle.ContainsVector(aVector, bOnIsIn, out rWithin, out rIsOn, out rIsCorner, aPrecis);
                case uppArcRecTypes.Slot:
                    {
                        uopShape slot = uopShape.RoundSlot(Center, Radius, Length, Rotation, Tag, Flag);
                        rIsCorner = slot.Vertices.FindIndex(x => x.IsEqual(uopVector.FromVector(aVector), aPrecis, false)) >= 0;
                        return slot.ContainsVector(aVector,bOnIsIn,aPrecis, out rWithin,out rIsOn, out _,out _,true);
                    }
                default:
                    return false;

            }

        }

        #endregion Methods

        #region Shared Methods

        public static void GetData(uopHole aHole, out double rRadius, out double rMinorRadius, out double rLength, double aScale = 1)
        {
            rRadius = aHole == null ? 0 : Math.Round(Math.Abs(aHole.Radius * aScale), 6);
            rMinorRadius = aHole == null ? 0 : Math.Round(Math.Abs(aHole.MinorRadius * aScale), 6);
            if (rMinorRadius >= rRadius) { rMinorRadius = 0; }
            rLength = aHole == null ? 0 : Math.Round(Math.Abs(aHole.Length * aScale), 6);
            if (rLength < 2 * rRadius) { rLength = 2 * rRadius; }
            if (aHole !=null && aHole.IsSquare) { rMinorRadius = 0; }
            if (rLength > 2 * rRadius) { rMinorRadius = 0; }
        }

        public static uopHole Null => new uopHole() { Index = -1};

        #endregion Shared Methods

        #region iArcRec Implementation

        uppArcRecTypes iArcRec.Type => !IsSquare && !IsElongated ? uppArcRecTypes.Arc : IsSquare ? uppArcRecTypes.Rectangle : uppArcRecTypes.Slot;
      
      
        uopArc iArcRec.Arc => !IsSquare && !IsElongated ? new uopArc(Radius,Center) { Tag = Tag, Flag = Flag} : null;
        uopRectangle iArcRec.Rectangle => IsSquare? new uopRectangle(Center,Length,Diameter,Tag,Flag,Rotation) :  null;
        uopShape iArcRec.Slot => uopShape.RoundSlot(Center, Radius, Length, Rotation, Tag, Flag);
        double iArcRec.Height { get =>Diameter; }
        double iArcRec.Width { get => Length; }
        double iArcRec.Rotation { get => Rotation; }
        iArcRec iArcRec.Clone() => Clone();
        #endregion iArcRec Implementation
    }
}
