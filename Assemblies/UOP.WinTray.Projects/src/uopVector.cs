using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.Policy;
using UOP.DXFGraphics;
using UOP.WinTray.Projects.Interfaces;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;

namespace UOP.WinTray.Projects
{
    public class uopVector : ICloneable, iVector
    {

        #region Constructors

        public uopVector(double aX = 0, double aY = 0, double aValue = 0, double aRadius = 0, double aInset = 0, double aDownSet = 0, double? aElevation = null, bool bVirtual = false, uopVector aRotateAboutCenter = null, double aRotationAngle = 0)
        {
            X = aX;
            Y = aY;
            Value = aValue;
            Row = 0;
            Col = 0;
            PartIndex = 0;
            Index = 0;
            Tag = string.Empty;
            Flag = string.Empty;
            Suppressed = false;
            Radius = aRadius;
            Proximity = 0;
            Inset = aInset;
            DownSet = aDownSet;
            Elevation = aElevation;
            Virtual = bVirtual;

            if(aRotateAboutCenter != null && aRotationAngle != 0)  Rotate(aRotateAboutCenter,aRotationAngle);
        }
        
        internal uopVector(UVECTOR aVector) => Init(null, bVector: aVector);

        public uopVector(uopVectorProperties aProperties)  => Init(null, aProperties: aProperties);


        public uopVector(iVector aVector) => Init(aVector); 

        public uopVector(string aCoordinates, string aTag = "", string aFlag = "") => Init(null, aCoordinates: aCoordinates, aTag: aTag, aFlag: aFlag);

        private void Init(iVector aVector = null, UVECTOR? bVector = null,  uopVectorProperties aProperties = null, string aCoordinates = null, string aTag = null, string aFlag = null)
        {
            if (aVector != null)
            {

                X = aVector.X;
                Y = aVector.Y;

                if (aVector is uopVector)
                    Copy(new uopVectorProperties((uopVector)aVector));
                else if (aVector is dxfVector)
                    Copy(new uopVectorProperties((dxfVector)aVector));
                else if (aVector is uopHole)
                    Copy(new uopVectorProperties((uopHole)aVector));
                else
                    Copy(new uopVectorProperties(uopVector.FromVector(aVector)));
            }
            else if (bVector.HasValue)
            {
                X = bVector.Value.X;
                Y = bVector.Value.Y;
                Value = bVector.Value.Value;
                Row = bVector.Value.Row;
                Col = bVector.Value.Col;
                PartIndex = bVector.Value.PartIndex;
                Index = bVector.Value.Index;
                Tag = bVector.Value.Tag;
                Flag = bVector.Value.Flag;
                Suppressed = bVector.Value.Suppressed;
                Radius = bVector.Value.Radius;
                Proximity = bVector.Value.Proximity;
                Inset = bVector.Value.Inset;
                DownSet = bVector.Value.DownSet;
                Elevation = bVector.Value.Elevation;
                Mark = bVector.Value.Mark;
                Virtual = bVector.Value.Virtual;
            }
            else
            {
                X = 0;
                Y = 0;
                Value = 0;
                Row = 0;
                Col = 0;
                PartIndex = 0;
                Index = 0;
                Tag = string.Empty;
                Flag = string.Empty;
                Suppressed = false;
                Radius = 0;
                Proximity = 0;
                Inset = 0;
                DownSet = 0;
                Elevation = null;
                Virtual = false;
            }

            if (aTag != null) Tag = aTag;
            if (aFlag != null) Flag = aFlag;
             if(!string.IsNullOrWhiteSpace(aCoordinates)) Coordinates = aCoordinates;

            if (aProperties != null) Copy(aProperties);
        }
        
        #endregion Constructors

        #region Properties

        public double X { get; set; }
        public double Y { get; set; }

        public double Value { get; set; }
        public int Row { get; set; }
        public int Col { get; set; }
        public int PartIndex { get; set; }
        public int Index { get; set; }
        public string Tag { get; set; }
        public string Flag { get; set; }
        public bool Suppressed { get; set; }
        public double Radius { get; set; }
        public double Proximity { get; set; }
        public double Inset { get; set; }
        public double DownSet { get; set; }
        public double? Elevation { get; set; }

        public double Rotation => Value;
        /// <summary>
        ///  a  delimited string with the objects Tag and Flag
        /// </summary>
        public string Handle => uopUtils.Handle(Tag, Flag);
       
        public string Coordinates
        {
            get
            {
                string _rVal = $"({X},{Y}";
                 _rVal += Elevation != 0 ? $"{Elevation})" : ")";
                return _rVal ;
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    value = value.Replace("(", "").Trim();
                    value = value.Replace(")", "").Trim();

                }
                if (string.IsNullOrWhiteSpace(value))
                {
                    X = 0;
                    Y = 0;
                    Elevation = 0;
                }
                else
                {
                 
                    string[] svals = value.Split(',');
                    if(svals.Length >=1) X = mzUtils.VarToDouble(svals[0]);
                    if (svals.Length >= 2) Y = mzUtils.VarToDouble(svals[1]);
                    if (svals.Length >= 3) Elevation = mzUtils.VarToDouble(svals[2]);

                }
            }
        }
        /// <returns></returns>
        public double Sqrd => Math.Pow(X, 2) + Math.Pow(Y, 2);

        /// <summary>
        /// Vectors Angle
        /// </summary>
        /// <returns></returns>
        public double XAngle
        {
            get
            {
                //#1the from vector
                //^the angle between the X Axis and the passed vectors in degrees
                double dX = Math.Abs(Math.Round(X, 6));
                double dY = Math.Abs(Math.Round(Y, 6));
                if (dX == 0 && dY == 0) return 0;
                if (dX == 0 || dY == 0)
                {
                    if (dX == 0)
                    {
                        //lies on Y axis
                        return (Y >= 0) ? 90 : 270;
                    }
                    else
                    {
                        //lies on X axis
                        return (X >= 0) ? 0 : 180;
                    }
                }
                else
                {
                    double ang = Math.Atan(dY / dX) * 180 / Math.PI;
                    if (X >= 0)
                    {
                        return (Y >= 0) ? ang : 360 - ang;
                    }
                    else
                    {
                        return (Y >= 0) ? 180 - ang : 180 + ang;
                    }
                }
            }

        }

        public bool Mark { get; set; }

        public bool Virtual { get; set; }

        #endregion Properties

        #region Methods

        public bool SetCoordinates(double? aX = null, double? aY = null, double? aElevation = null)
        {
            bool _rVal = false;
            if (aX.HasValue)
            {
                if (X != aX.Value) _rVal = true;
                X = aX.Value;
            }
            if (aY.HasValue)
            {
                if (Y != aY.Value) _rVal = true;
                Y = aY.Value;
            }
            if (aElevation.HasValue)
            {
                if (Elevation != aElevation.Value) _rVal = true;
                Elevation = aElevation.Value;
            }

            return _rVal;
        }
        object ICloneable.Clone() => (object)Clone(true);

        public uopVector Clone(bool bCloneIndex = true) => new uopVector(this) { Index = !bCloneIndex ? 0 : Index };


        public void Copy(uopVector aVector, bool bIngnoreNUllVector = true)
        {
            if (aVector == null)
            {
                if (!bIngnoreNUllVector)
                    aVector = new uopVector();
                else
                    return;
            }

            int idx = Index;

            Value = aVector.Value;
            Row = aVector.Row;
            Col = aVector.Col;
            PartIndex = aVector.PartIndex;
            Index = idx;
            Tag = aVector.Tag;
            Flag = aVector.Flag;
            Suppressed = aVector.Suppressed;
            Radius = aVector.Radius;
            Proximity = aVector.Proximity;
            Inset = aVector.Inset;
            DownSet = aVector.DownSet;
            Elevation = aVector.Elevation;
        }

        public void Copy(uopVectorProperties aProperties)
        {
            if (aProperties == null)   return;

            if(aProperties.X.HasValue) X = aProperties.X.Value;
            if (aProperties.Y.HasValue) Y = aProperties.Y.Value;
            if (aProperties.Value.HasValue) Value = aProperties.Value.Value;
            if (aProperties.Row.HasValue) Row = aProperties.Row.Value;
            if (aProperties.Col.HasValue) Col = aProperties.Col.Value;
            if (aProperties.PartIndex.HasValue) PartIndex = aProperties.PartIndex.Value;
            if (aProperties.Index.HasValue) Index = aProperties.Index.Value;
            if (aProperties.Tag != null) Tag = aProperties.Tag;
            if (aProperties.Flag != null) Flag = aProperties.Flag;
            if (aProperties.Suppressed.HasValue) Suppressed = aProperties.Suppressed.Value;
            if (aProperties.Radius.HasValue) Radius = aProperties.Radius.Value;
            if (aProperties.Proximity.HasValue) Proximity = aProperties.Proximity.Value;
            if (aProperties.Inset.HasValue) Inset = aProperties.Inset.Value;
            if (aProperties.DownSet.HasValue) DownSet = aProperties.DownSet.Value;
            if (aProperties.Elevation.HasValue) Elevation = aProperties.Elevation.Value;
            if (aProperties.Virtual.HasValue) Virtual = aProperties.Virtual.Value;
        }

        public override string ToString() => $"uopVector ({X:0.00##},{Y:0.00##}) {uopUtils.Handle(Tag,Flag,"<>")}".Trim();
        

        public uopVector DirectionTo(iVector aVector, out double rDistance) => DirectionTo(aVector, false, out _, out rDistance, false);

        public uopVector DirectionTo(iVector aVector) => DirectionTo(aVector, false, out _, out _, false);


        public uopVector DirectionTo(iVector aVector, bool bReturnInverse, out bool rDirectionIsNull, out double rDistance, bool bDontNormalize = false)
        {
            rDirectionIsNull = false;
            rDistance = 0;
            uopVector v1 = aVector == null ? uopVector.Zero : uopVector.FromVector(aVector);
            uopVector _rVal = v1 - this;
            if (!bDontNormalize)
            { _rVal.Normalize(out rDirectionIsNull, out rDistance); }
            else
            {
                rDistance = _rVal.Length(15);
                rDirectionIsNull = rDistance == 0;
            }
            if (bReturnInverse) { _rVal *= -1; }

            return _rVal;
        }

        public uopVector DirectionTo(iLine aLine, out double rDistance) 
        {
            rDistance = 0;
            if (aLine == null) return uopVector.Zero;
            uopVector ip = ProjectedTo(aLine);
            return DirectionTo(ip, out rDistance);
} 


        public bool IsEqual(uopVector aVector, int aPrecis = 6, bool bCompareElevation = true)
        {
            if(!Elevation.HasValue || !aVector.Elevation.HasValue) bCompareElevation = false;
            if (aPrecis < 0)
            {
                if (bCompareElevation )
                    return X == aVector.X && Y == aVector.Y && Elevation.Value == aVector.Elevation.Value;
                else
                    return X == aVector.X && Y == aVector.Y;
            }
            else
            {
                aPrecis = mzUtils.LimitedValue(aPrecis, 0, 15);
                if (bCompareElevation)
                    return Math.Round(X, aPrecis) == Math.Round(aVector.X, aPrecis) && Math.Round(Y, aPrecis) == Math.Round(aVector.Y, aPrecis) && Math.Round(Elevation.Value, aPrecis) == Math.Round(aVector.Elevation.Value, aPrecis);
                else
                    return Math.Round(X, aPrecis) == Math.Round(aVector.X, aPrecis) && Math.Round(Y, aPrecis) == Math.Round(aVector.Y, aPrecis);

            }
        }

        public void Translate(double DX, double DY) { X += DX; Y += DY; }
        /// <summary>
        /// vectors distance
        /// </summary>
        /// <param name="bVector"></param>
        /// <param name="aPrecis"></param>
        /// <returns></returns>
        public double DistanceTo(iVector bVector, int aPrecis = 3)
        {
            if(bVector == null) return 0;
            uopVector v2 =  uopVector.FromVector( bVector);
            
            double _rVal = Math.Sqrt(Math.Pow(X - v2.X, 2) + Math.Pow(Y - v2.Y, 2));
            if (aPrecis >= 0)
                _rVal = Math.Round(_rVal, mzUtils.LimitedValue(aPrecis, 0, 15));

            return _rVal;
        }

        internal double DistanceTo(UVECTOR bVector, int aPrecis = 3)
        {
            double _rVal = Math.Sqrt(Math.Pow(X - bVector.X, 2) + Math.Pow(Y - bVector.Y, 2));
            if (aPrecis >= 0)
                _rVal = Math.Round(_rVal, mzUtils.LimitedValue(aPrecis, 0, 15));
            return _rVal;
        }



        /// <summary>
        /// returns the shortest distance to the passed line
        /// </summary>
        /// <param name="aLine" the line></param>
        /// <returns></returns>
        internal double DistanceTo(ULINE aLine)
        {
            UVECTOR v1 = new UVECTOR(this);
            v1.ProjectTo(aLine, out double _rVal);
            return _rVal;
        }

        /// returns the shortest distance to the passed line
        /// </summary>
        /// <param name="aLine" the line></param>
        /// <returns></returns>
        public double DistanceTo(iLine aLine)
        {
            return (aLine == null) ? 0 : DistanceTo(new ULINE(aLine));
        }

        /// <summary>
        /// vetcors update
        /// </summary>
        /// <param name="aNewX"></param>
        /// <param name="aNewY"></param>
        /// <param name="aPrecis"></param>
        public void Update(double? aNewX = null, double? aNewY = null, int aPrecis = -1)
        {
            if (aNewX.HasValue)
                X = aPrecis < 0 ? aNewX.Value : Math.Round(aNewX.Value, mzUtils.LimitedValue(aPrecis, 0, 15));
            if (aNewY.HasValue)
                Y = aPrecis < 0 ? aNewY.Value : Math.Round(aNewY.Value, mzUtils.LimitedValue(aPrecis, 0, 15));
        }

        public uopVector WithRespectToPlane(dxfPlane aPlane)
        {
            uopVector _rVal = new uopVector(this);
            if (aPlane != null) return _rVal;
            aPlane.VectorWithRespectTo(_rVal);
            //dxfVector v1 = new dxfVector(this).WithRespectToPlane(aPlane);
            //_rVal.X = v1.X; _rVal.Y = v1.Y;
            return _rVal;

        }

        /// <summary>
        /// returns true if the passed vectors are equal within the passed precision
        /// </summary>
        /// <param name="aVector"></param>
        /// <param name="bVector"></param>
        /// <param name="aPrecis"></param>
        /// <returns></returns>
        public bool Compare(uopVector bVector, int aPrecis = 4)
        {
            if (bVector is null) return false;
            aPrecis = mzUtils.LimitedValue(aPrecis, 0, 15);
            if (Math.Round(X, aPrecis) != Math.Round(bVector.X, aPrecis)) return false;
            if (Math.Round(Y, aPrecis) != Math.Round(bVector.Y, aPrecis)) return false;
            return true;
        }

        /// <summary>
        /// Vector Polar
        /// </summary>
        /// <param name="aVector"></param>
        /// <param name="aAngle"></param>
        /// <param name="aDistance"></param>
        /// <returns></returns>
        /// 
        public uopVector Polar(double aAngle, double aDistance)
        {
            uopVector _rVal = (uopVector)Clone();
            aAngle = mzUtils.NormAng(aAngle, false, true, true);
            if (aDistance == 0)
            {
                return _rVal;
            }

            if (aAngle == 0 || aAngle == 360)
            {
                _rVal.Update(_rVal.X + aDistance);
            }
            else if (aAngle == 90)
            {
                //_rVal.Update(_rVal.X, _rVal.Y);
            }
            else if (aAngle == 180)
            {
                _rVal.Update(_rVal.X - aDistance);
            }
            else if (aAngle == 270)
            {
                // _rVal.Update(_rVal.X, _rVal.Y);
            }
            else
            {
                uopVector aDir = new uopVector(1, 0, 0);
                aDir.Rotate(aAngle, false);
                _rVal += aDir * aDistance;
            }

            return _rVal;
        }


        /// <summary>
        ///#1the subject vector
        ///#2the structure of the plane
        ///^returns the coordinates of the passed vector with respect to the center and origin of the passed plane
        /// </summary>
        /// <param name="aPlane"></param>
        /// <param name="aScaler"></param>
        /// <returns></returns>
        public uopVector WithRespectToPlane(dxfPlane aPlane, double aScaler = 1)
        {
            uopVector _rVal = Clone(true);
            dxfVector v1 = ToDXFVector();


            if (aScaler != 1) { v1.Multiply(aScaler); }

            v1 = v1.WithRespectToPlane(aPlane);
            _rVal.Update(v1.X, v1.Y);

            return _rVal;
        }

       
        /// <summary>
        /// returns true if the vector lies on the passed segment
        /// </summary>
        /// <param name="aSegment">the segment to test</param>
        /// <param name="rIsStartPt">returns true if the vector is the start point of the vector</param>
        /// <param name="rIsEndPt">returns true if the vector is the end point of the segment </param>
        /// <param name="rWithin">returns true if the vector lies on th edefined path of the segment (not on it's infinite version)</param>
        /// <param name="aPrecis">the precision to apply</param>
        /// <param name="aSegmentIsInfinite">flag to treat the segment as infinite even if it isn't </param>
        /// <returns></returns>
        public bool LiesOn(iSegment aSegment, out bool rIsStartPt, out bool rIsEndPt, out bool rWithin, int aPrecis = 5, bool aSegmentIsInfinite = false)
        {
            rIsStartPt = false; rIsEndPt = false; rWithin = false;
            if (aSegment == null) return false;
            if (aSegment.IsArc)
            {
                bool test = aSegment.Arc.ContainsVector(this,  true,   aPrecis, out rWithin, out bool ison,out  rIsStartPt, out rIsEndPt, aSegmentIsInfinite);
                return ison;
            }
            else
            {
                return aSegment.Line.ContainsVector(this, aPrecis, out rIsStartPt, out rIsEndPt, out rWithin,   aSegmentIsInfinite);
                
            }
         
        }

        public uopVector WithRespectTo(dxfPlane aPlane , dxfPlane aTransferPlane = null, double? aTransferElevation = null, double? aTransferRotation  = null, double? aXScale = null, double? aYScale = null, bool bMaintainZ = false)
        {
            uopVector _rVal = new uopVector(this);
            if(aPlane == null) return _rVal;
            dxfVector v1 = dxfVector.VectorWithRespectToPlane(this, aPlane, aTransferPlane, aTransferElevation, aTransferRotation, aXScale, aYScale, bMaintainZ);
            _rVal.X = v1.X;
            _rVal.Y = v1.Y;
            _rVal.Elevation = v1.Z;
            return _rVal;
        }

        /// returns true if the vector lies on the passed segment
        /// </summary>
        /// <param name="aSegment">the segment to test</param>
        /// <param name="aPrecis">the precision to apply</param>
        /// <param name="aSegmentIsInfinite">flag to treat the segment as infinite even if it isn't </param>
        public bool LiesOn(iSegment aSegment, int aPrecis = 5, bool aSegmentIsInfinite = false) => LiesOn(aSegment, out _, out _, out _ ,aPrecis, aSegmentIsInfinite);
   
        /// <summary>
        /// vectors rotate
        /// </summary>
        /// <param name="aAngle"></param>
        /// <param name="bInRadians"></param>
        public uopVector Rotated(double aAngle, bool bInRadians = false)
        {
            uopVector _rVal = Clone();
            _rVal.Rotate(new uopVector(0, 0), aAngle, bInRadians);
            return _rVal;
        }

        /// <summary>
        /// vectors rotate
        /// </summary>
        /// <param name="aAngle"></param>
        /// <param name="bInRadians"></param>
        public void Rotate(double aAngle, bool bInRadians = false) => Rotate(new uopVector(0, 0), aAngle, bInRadians);
        

        /// <summary>
        /// vectors rotate
        /// </summary>
        /// <param name="aOrigin"></param>
        /// <param name="aAngle"></param>
        /// <param name="bInRadians"></param>
        public void Rotate(iVector aOrigin, double aAngle, bool bInRadians = false)
        {
            
            double a = aOrigin == null ? 0 : aOrigin.X;
            double B = aOrigin == null ? 0 : aOrigin.Y;
            double C = 0;
            double Z = 0;
            double u = 0;
            double V = 0;
            double W = 1;

            double VX = X;
            double VY = Y;
            if (!bInRadians)
            {
                aAngle -= (int)(aAngle / 360) * 360;
                aAngle = Math.Round(aAngle * Math.PI / 180, 6);
            }
            else
            {
                aAngle -= (int)(aAngle / (2 * Math.PI)) * (2 * Math.PI);
            }
            if (Math.Abs(aAngle) <= 0.00000001) return;


            double c1 = u * X + V * Y + W * Z;
            double c2 = Math.Sqrt(Math.Pow(u, 2) + Math.Pow(V, 2) + Math.Pow(W, 2));
            double denom = Math.Pow(u, 2) + Math.Pow(V, 2) + Math.Pow(W, 2);
            if (denom == 0) return;


            //the X component
            double t1 = a * (Math.Pow(V, 2) + Math.Pow(W, 2));
            double t2 = u * (-B * V - C * W + c1);
            double t3 = ((VX - a) * (Math.Pow(V, 2) + Math.Pow(W, 2)) + u * (B * V + C * W - V * VY - W * Z)) * Math.Cos(aAngle);
            double t4 = c2 * (-C * V + B * W - W * VY + V * Z) * Math.Sin(aAngle);
            X = (t1 + t2 + t3 + t4) / denom;

            //the Y component
            t1 = B * (Math.Pow(u, 2) + Math.Pow(W, 2));
            t2 = V * (-a * u - C * W + c1);
            t3 = ((VY - B) * (Math.Pow(u, 2) + Math.Pow(W, 2)) + V * (a * u + C * W - u * VX - W * Z)) * Math.Cos(aAngle);
            t4 = c2 * (C * u - a * W + W * VX - u * Z) * Math.Sin(aAngle);
            Y = (t1 + t2 + t3 + t4) / denom;
        }

        internal void Rotate(UVECTOR aOrigin, double aAngle, bool bInRadians = false) => Rotate(new uopVector(aOrigin.X, aOrigin.Y), aAngle, bInRadians);

        /// <summary>
        /// vectors dot product
        /// </summary>
        /// <param name="bVector"></param>
        /// <returns></returns>
        public double DotProduct(uopVector bVector) { return X * bVector.X + Y * bVector.Y; }

        /// <param name="aPrecis"></param>
        /// <returns></returns>
        /// <summary>
        /// vectors length
        /// </summary>
        /// <param name="aPrecis"></param>
        /// <returns></returns>
        public double Length(int aPrecis = 4)
        {
            double _rVal = Math.Sqrt(Sqrd);

            if (double.IsInfinity(_rVal))
            {
                _rVal = Double.MaxValue;
            }
            if (double.IsNaN(_rVal))
            {
                _rVal = 0;
            }




            if (aPrecis > 0)
            {
                aPrecis = mzUtils.LimitedValue(aPrecis, 0, 15);
                _rVal = Math.Round(_rVal, aPrecis);
            }
            if (double.IsInfinity(_rVal)) _rVal = double.MaxValue;
            return _rVal;
        }

        /// <summary>
        /// vectors mid point
        /// </summary>
        /// <param name="aVector"></param>
        /// <param name="bVector"></param>
        /// <returns></returns>
        public uopVector MidPt(uopVector bVector)
        {
            uopVector _rVal = new uopVector(this);
            _rVal.X += 0.5 * (bVector.X - X);
            _rVal.Y += 0.5 * (bVector.Y - Y);
            return _rVal;
        }

        /// <summary>
        /// vector normalize
        /// </summary>            
        /// <returns></returns>
        public void Normalize() => Normalize(out bool BNULL, out double LNGTH);



        /// <summary>
        /// vector normalize
        /// </summary>
        /// <param name="rVectorIsNull"></param>
        /// <returns></returns>
        public void Normalize(out bool rVectorIsNull) => Normalize(out rVectorIsNull, out double LNGTH);

        /// <summary>
        /// vector normalize
        /// </summary>
        /// <param name="rVectorIsNull"></param>
        /// <param name="rLength"></param>
        /// <returns></returns>
        public void Normalize(out double rLength) => Normalize(out bool BNULL, out rLength);


        /// <summary>
        /// vector normalize
        /// </summary>
        /// <param name="rVectorIsNull"></param>
        /// <param name="rLength"></param>
        /// <returns></returns>
        public void Normalize(out bool rVectorIsNull, out double rLength)
        {
            rVectorIsNull = false;
            rLength = Length(8);
            rVectorIsNull = rLength <= 0.000001;
            if (rVectorIsNull) { rLength = 0; return; }
            X /= rLength;
            Y = (Math.Abs(X) != 1) ? Y / rLength : 0;
        }
        /// <summary>
        /// vectors coord
        /// </summary>
        /// <param name="aPrecis"></param>
        /// <param name="aZValue"></param>
        /// <returns></returns>
        public string Coords(int aPrecis = 3, double aZValue = 0)
        {
            aPrecis = mzUtils.LimitedValue(aPrecis, 0, 8);
            return "(" + Math.Round(X, aPrecis) + "," + Math.Round(Y, aPrecis) + "," + Math.Round(aZValue, aPrecis) + ")";
        }


        /// <summary>
        ///^returns vector component of aVector along bVector 
        /// </summary>
        /// <param name="bVector"></param>
        /// <returns></returns>
        public uopVector ComponentAlong(uopVector bVector)
        {
            uopVector _rVal = (uopVector)bVector.Clone(); ;
            double numer = X * bVector.X + Y * bVector.Y;
            double denom = bVector.Sqrd;

            if (denom != 0) { _rVal *= numer / denom; }

            return _rVal;
        }

        /// <summary>
        /// vector normalize
        /// </summary>
        /// <returns></returns>
        public uopVector Normalized() => Normalized(out bool BNULL, out double LNGTH);

        /// <summary>
        /// vector normalize
        /// </summary>
        /// <param name="rVectorIsNull"></param>
        /// <returns></returns>
        public uopVector Normalized(out bool rVectorIsNull) => Normalized(out rVectorIsNull, out double LNGT);

        public bool IsNull(int aPrecis = 8)
        {

            if (aPrecis <= 0) return X == 0 && Y == 0;
            aPrecis = mzUtils.LimitedValue(aPrecis, 0, 15);
            return Math.Round(X, aPrecis) == 0 && Math.Round(Y, aPrecis) == 0;
        }

        /// <summary>
        /// vector normalize
        /// </summary>            
        /// <param name="rVectorIsNull"></param>
        /// <param name="rLength"></param>
        /// <returns></returns>
        public uopVector Normalized(out double rLength) => Normalized(out bool BNULL, out rLength);

        /// <summary>
        /// vector normalize
        /// </summary>
        /// <param name="rVectorIsNull"></param>
        /// <param name="rLength"></param>
        /// <returns></returns>
        public uopVector Normalized(out bool rVectorIsNull, out double rLength)
        {
            uopVector _rVal = Clone(true);
            _rVal.Normalize(out rVectorIsNull, out rLength);
            return _rVal;
        }

        public override bool Equals(object obj) => obj.GetType() == typeof(uopVector) ? this == (uopVector)obj : false;

        public dxfVector ToDXFVector(double aXChange, double aYChange, double? aRadius = null, double? aZ = null, string aTag = null, string aFlag = null, dxfPlane aPlane = null)
        {

            double elev = aZ.HasValue ?   aZ.Value : Elevation.HasValue ? Elevation.Value : 0;
            double rad = !aRadius.HasValue ? Radius : aRadius.Value;
            dxfVector _rVal = (aPlane == null) ? new dxfVector(X + aXChange, Y + aYChange, elev, aTag: Tag, aFlag: aFlag) : aPlane.Vector(X, Y, elev, aTag: Tag, aFlag: aFlag);

            _rVal.Row = Row;
            _rVal.Col = Col;
            _rVal.Suppressed = Suppressed;
            _rVal.Value = Value;
            if (aTag != null) _rVal.Tag = aTag;
            if (aFlag != null) _rVal.Flag = aFlag;
            _rVal.VertexRadius = rad;
            _rVal.Suppressed = Suppressed;
            return _rVal;
        }

        /// <summary>
        /// vectirs to vector
        /// </summary>
        /// <param name="bValueAsRotation"></param>
        /// <param name="bValueAsVertexRadius"></param>
        /// <param name="aZ"></param>
        /// <param name="aTag"></param>
        /// <param name="aFlag"></param>
        /// <param name="aPlane"></param>
        /// <returns></returns>
        public dxfVector ToDXFVector(bool bValueAsRotation = false, bool bValueAsVertexRadius = false, double? aZ = null, string aTag = null, string aFlag = null, dxfPlane aPlane = null)
        {

            double elev = aZ.HasValue ?  aZ.Value : Elevation.HasValue ? Elevation.Value: 0 ;
            dxfVector _rVal = (aPlane == null) ? new dxfVector(X, Y, elev, aTag: Tag, aFlag: Flag) : aPlane.Vector(X, Y, elev, aTag: Tag, aFlag: Flag);

            _rVal.Row = Row;
            _rVal.Col = Col;
            _rVal.Suppressed = Suppressed;
            _rVal.Value = Value;

            if (aTag != null) _rVal.Tag = aTag;
            if (aFlag != null) _rVal.Flag = aFlag;

            _rVal.VertexRadius = Radius;

            _rVal.Suppressed = Suppressed;
            if (bValueAsRotation) { _rVal.Rotation = Value; }
            if (bValueAsVertexRadius && Value != 0) { _rVal.VertexRadius = Value; }
            return _rVal;
        }

        /// <summary>
        /// vectors project
        /// </summary>
        /// <param name="aDirection"></param>
        /// <param name="aDistance"></param>
        /// <param name="bSuppressNormalize"></param>
        /// <param name="bInvertDirection"></param>
        public void Project(iVector aDirection, double aDistance, bool bSuppressNormalize = false, bool bInvertDirection = false)
        {
            if (aDistance == 0) return;

            uopVector aDir = new uopVector(aDirection);
            bool aFlg = false;
            if (!bSuppressNormalize)
                aDir.Normalize(out aFlg);
            if (aFlg) return;

            if (aDistance <= 0 || bInvertDirection) { aDir *= -1; }

            uopVector vector = aDir * Math.Abs(aDistance);
            X += vector.X;
            Y += vector.Y;

        }

        /// <param name="aDirection"></param>
        /// <param name="aDistance"></param>
        /// <param name="bSuppressNormalize"></param>
        /// <param name="bInvertDirection"></param>
        public uopVector Projected(iVector aDirection, double aDistance, bool bSuppressNormalize = false, bool bInvertDirection = false)
        {
            uopVector _rVal = new uopVector(this);
            _rVal.Project(aDirection, aDistance, bSuppressNormalize, bInvertDirection);
            return _rVal;
        }

        /// <summary>
        ///returns a clone of the vector projected orthogonally to the passed line
        /// </summary>
        /// <param name="aLine"></param>
        public uopVector ProjectedTo(iLine aLine) { uopVector _rVal = new uopVector(this); _rVal.ProjectTo(aLine); return _rVal; }


        /// <summary>
        ///projects the vector orthogonally to the passed line
        /// </summary>
        /// <param name="aLine"></param>
        public void ProjectTo(iLine aLine) => ProjectTo(aLine, out double _);
        

        /// <summary>
        ///projects the vector orthogonally to the passed line
        /// </summary>
        /// <param name="aLine"></param>
        /// <param name="rDistance"></param>
        public void ProjectTo(iLine aLine, out double rDistance) => ProjectTo(aLine, out uopVector _, out bool _, out bool _, out rDistance);
        

        /// <summary>
        ///projects the vector orthogonally to the passed line
        /// </summary>
        /// <param name="aLine"></param>
        /// <param name="rOrthoDirection"></param>
        /// <param name="rDistance"></param>
        public void ProjectTo(iLine aLine, out uopVector rOrthoDirection, out double rDistance)
        {
            ProjectTo(aLine, out rOrthoDirection, out bool _, out bool _, out rDistance);
        }

        /// <summary>
        ///projects the vector orthogonally to the passed line
        ///#1the subject line
        ///#2returns the orthogonal direction to the segment from the vector
        ///#3returns true if then returned vector lies on the passed segment (between the end points)
        ///#4returns true if the returned vector lies in the same direction from the start point as the end point (positive direction)
        ///#5returns then orthogal distance to the segment from the vector
        /// </summary>
        /// <param name="aLine"></param>
        /// <param name="rOrthoDirection"></param>
        /// <param name="rDistance"></param>
        /// <param name="rPointIsOnSegment"></param>
        /// <param name="rDirectionPositive"></param>
        internal void ProjectTo(iLine aLine, out uopVector rOrthoDirection, out bool rPointIsOnSegment, out bool rDirectionPositive, out double rDistance)
        {

            UVECTOR u1 = new UVECTOR(this);
            u1.ProjectTo(new ULINE(aLine), out UVECTOR ortho, out rPointIsOnSegment, out rDirectionPositive, out rDistance);
            rOrthoDirection = new uopVector(ortho);
            SetCoordinates(u1.X, u1.Y);

        }

        /// <summary>
        /// mirrors the point across the passed line
        /// </summary>
        /// <param name="aLine">the mirror line</param>
        public void Mirror(iLine aLine)
        {
            if (aLine == null) return;
            uopLine uline =  uopLine.FromLine(aLine);
            if (uline.Length <= 0) return;

            uopVector v1 = new uopVector(this);
            v1.ProjectTo(uline,out uopVector orthodir, out double d1);
            if (d1 == 0) return;
            Project(orthodir, 2 * d1);
        }

        /// <summary>
        /// mirrors the point across the passed line
        /// </summary>
        /// <param name="aLine">the mirror line</param>
        public uopVector Mirrored(iLine aLine)
        {
            uopVector _rVal = new uopVector(this);
            _rVal.Mirror(aLine);
            return _rVal;
        }


        /// <summary>
        /// mirrors the point across a vertical and/or horizontal lines
        /// </summary>
        /// <param name="aX">the x of the vertical line</param>
        /// <param name="aY">the y of the horizontal line</param>
        public void Mirror(double? aX, double? aY)
        {
            if (!aX.HasValue && !aY.HasValue) return;
            double dx = aX.HasValue ? X - aX.Value : 0;
            double dy = aY.HasValue ? Y - aY.Value : 0;
            if (dx == 0 && dy == 0) return;

            X -= 2 * dx;
            Y -= 2 * dy;


        }


        /// <summary>
        /// returns a clone of the point mirrored across a vertical and/or horizontal lines
        /// </summary>
        /// <param name="aX">the x of the vertical line</param>
        /// <param name="aY">the y of the horizontal line</param>
        public uopVector Mirrored(double? aX, double? aY)
        {
            uopVector _rVal = Clone();
            _rVal.Mirror(aX, aY);
            return _rVal;
        }

        public void Move(double aX = 0, double aY = 0)
        {
            X += aX;
            Y += aY;
        }

        public uopVector Moved(double aX = 0, double aY = 0)
        {
            uopVector _rVal = new uopVector(this);
            _rVal.Move(aX, aY);
            return _rVal;

        }

        public uopVectorProperties GetProperties() => new uopVectorProperties(this);
        public void SetProperties(uopVectorProperties aProperties) => Copy(aProperties);

        #endregion Methods

        #region Shared Methods

        public static uopVector Zero { get => new uopVector(0, 0); }
        double iVector.Z { get => Elevation.HasValue ? Elevation.Value : 0; set => Elevation = value; }

        /// <summary>
        /// Vector planar
        /// </summary>
        /// <param name="aPlane"></param>
        /// <param name="aX"></param>
        /// <param name="aY"></param>
        /// <param name="aRadius"></param>
        /// <returns></returns>
        internal static uopVector Planar(dxfPlane aPlane, dynamic aX = null, dynamic aY = null, double aRadius = 0)
        {

            if (aPlane == null)
            {
                return new uopVector(mzUtils.VarToDouble(aX, aDefault: 0), mzUtils.VarToDouble(aY, aDefault: 0), 0, mzUtils.VarToDouble(aRadius, aDefault: 0));
            }
            else
            {

                uopVector _rVal = new uopVector(aPlane.X, aPlane.Y, 0, mzUtils.VarToDouble(aRadius, aDefault: 0));
                double d1 = mzUtils.VarToDouble(aX);
                if (d1 != 0) { _rVal.Project(new uopVector(aPlane.XDirection.X, aPlane.XDirection.Y), d1, true); }
                d1 = mzUtils.VarToDouble(aY);
                if (d1 != 0) { _rVal.Project(new uopVector(aPlane.YDirection.X, aPlane.YDirection.Y), d1, true); }
                return _rVal;
            }

        }



        /// <summary>
        /// Vector planar
        /// </summary>
        /// <param name="aPlane"></param>
        /// <param name="aX"></param>
        /// <param name="aY"></param>
        /// <param name="aRadius"></param>
        /// <returns></returns>
        internal static uopVector PlaneVector(dxfPlane aPlane, dynamic aX = null, dynamic aY = null, double aRadius = 0)
        {
            uopVector _rVal;
            if (aPlane == null)
            {
                _rVal = new uopVector(mzUtils.VarToDouble(aX, aDefault: 0), mzUtils.VarToDouble(aY, aDefault: 0), 0, mzUtils.VarToDouble(aRadius, aDefault: 0));
            }
            else
            {


                _rVal = new uopVector(aPlane.X, aPlane.Y, 0, mzUtils.VarToDouble(aRadius, aDefault: 0));
                double d1 = mzUtils.VarToDouble(aX);
                if (d1 != 0)
                {
                    _rVal.Project(new uopVector(aPlane.XDirection.X, aPlane.XDirection.Y), d1, true);
                }
                d1 = mzUtils.VarToDouble(aY);
                if (d1 != 0)
                {
                    _rVal.Project(new uopVector(aPlane.YDirection.X, aPlane.YDirection.Y), d1, true);
                }
            }
            return _rVal;
        }

        public override int GetHashCode()
        { return base.GetHashCode(); }

        public static uopVector FromVector(iVector aVector, bool bCloneIt = false)
        {
            if (aVector == null) return null;

            uopVector _rVal = null;
            if (aVector.GetType() == typeof(uopVector))
            {
                _rVal = (uopVector)aVector;
            }
            else if (aVector.GetType() == typeof(dxfVector))
            {
                _rVal = new uopVector(new uopVectorProperties((dxfVector)aVector));
                bCloneIt = false;

            }
            else if (aVector.GetType() == typeof(uopHole))
            {
                _rVal = new uopVector(new uopVectorProperties((uopHole)aVector));
       
                bCloneIt = false;
            }
            else
            {
                _rVal = new uopVector() { X = aVector.X, Y = aVector.Y, Elevation = aVector.Z, Tag = aVector.Tag, Flag = aVector.Flag };

             
                bCloneIt = false; // we just did!
            }
           
            if (bCloneIt) { _rVal = new uopVector(_rVal); }
            return _rVal;
        }
        #endregion Shared Methods


        #region Operators

        public static uopVector operator +(uopVector A, uopVector B) { uopVector _rVal = A.Clone(true); _rVal.X += B.X; _rVal.Y += B.Y; return _rVal; }

        public static uopVector operator -(uopVector A, uopVector B) { uopVector _rVal = A.Clone(true); _rVal.X -= B.X; _rVal.Y -= B.Y; return _rVal; }
        public static uopVector operator *(uopVector A, double aScaler) { uopVector _rVal = A.Clone(true); _rVal.X *= aScaler; _rVal.Y *= aScaler; return _rVal; }
        public static bool operator ==(uopVector A, uopVector B) { return A is null ? B is null : !(B is null) && A.Compare(B, 4); }
        public static bool operator !=(uopVector A, uopVector B) => !(A == B);

        #endregion Operators




    }
}
