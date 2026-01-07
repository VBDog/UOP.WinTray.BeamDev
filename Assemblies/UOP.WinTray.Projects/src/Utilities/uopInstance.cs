using System;
using UOP.DXFGraphics;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Structures;

namespace UOP.WinTray.Projects.Utilities
{


    public class uopInstance : dxoInstance,  ICloneable
    {

        #region Constructors

        public uopInstance() { Init(); }
        public uopInstance(double aDX, double aDY, double aRotation = 0, bool bInverted = false, double aScaleFactor = 1, bool bLeftHanded = false, int aPartIndex = 0, uppAlternateRingTypes altRing = uppAlternateRingTypes.AllRings, bool bVirtual = false, int aRow = 0, int aCol = 0)
        {
            Init();
            DX = aDX;
            DY = aDY;
            Rotation = aRotation;
            Inverted = bInverted;
            LeftHanded = bLeftHanded;
            ScaleFactor = aScaleFactor;
            PartIndex = aPartIndex;
            AltRingType = altRing;
            Virtual = bVirtual;
            Row = aRow;
            Col = aCol;
        }

        internal uopInstance(TINSTANCE aStructure)
        {
            Init();

            DX = aStructure.DX;
            DY = aStructure.DY;
            Rotation = aStructure.Rotation;
            Inverted = aStructure.Inverted;
            LeftHanded = aStructure.LeftHanded;
            ScaleFactor = aStructure.ScaleFactor;
            Tag = aStructure.Tag;
            Index = aStructure.Index;
            PartIndex = aStructure.PartIndex;
            AltRingType = aStructure.AltRingType;
            Virtual = aStructure.Virtual;
            Row = aStructure.Row;
            Col = aStructure.Col;

        }

        public uopInstance(iInstance aInstance)
        {
            Init();
            if (aInstance == null) return;
            base.Copy(aInstance);
            if (aInstance.GetType() == typeof(uopInstance))
            {
                uopInstance uinstance = (uopInstance)aInstance;
                PartIndex = uinstance.PartIndex;
                BasePt = uinstance.BasePt.Clone();
                AltRingType = uinstance.AltRingType;
                Virtual = uinstance.Virtual;
                Row = uinstance.Row;
                Col = uinstance.Col;

            }
           
        }
        private void Init()
        {
            DX = 0;
            DY = 0;
            Rotation = 0;
            Inverted = false;
            LeftHanded = false;
            ScaleFactor = 0;
            Tag = string.Empty;
            Index = 0;
            PartIndex = 0;
            AltRingType = uppAlternateRingTypes.AllRings;
            Virtual = false;
            Row = 0;
            Col = 0;
        }

        #endregion Constructors

        #region Properties

        public bool ExistsOnRing1 => AltRingType == uppAlternateRingTypes.AllRings || AltRingType == uppAlternateRingTypes.AtlernateRing1;
        public bool ExistsOnRing2 => AltRingType == uppAlternateRingTypes.AllRings || AltRingType == uppAlternateRingTypes.AtlernateRing2;


        public uopVector InverseDisplacement { get => new uopVector(DY, DX); set { if (value == null) { DX = 0; DY = 0; } else { DY = value.X; DX = value.Y; } } }

        public uopVector Displacement { get => new uopVector(DX, DY, aValue: Rotation); set { if (value == null) { DX = 0; DY = 0; } else { DX = value.X; DY = value.Y; } } }
        public double DX { get => base.XOffset; set => base.XOffset = value; }
        public double DY { get => base.YOffset; set => base.YOffset = value; }

        public int Row { get; set; }
        public int Col { get; set; }

        public uppAlternateRingTypes AltRingType { get; set; }

        public bool Virtual { get; set; }

        public int PartIndex { get; internal set; }

        internal UVECTOR BasePt { get; set; }

        public double X => BasePt.X + DX;

        public double Y => BasePt.Y + DY;


        #endregion Properties

        #region Methods
        public override string ToString()
        {
            string _rVal;
            if (Rotation != 0)
            {
                _rVal = $"TINSTANCE [DX: {DX:0.0####} DY: {DY:0.0####} R: {Rotation:0.0####}";
            }
            else
            {
                _rVal = $"TINSTANCE [DX: {DX:0.0####} DY: {DY:0.0####}";
            }
            if (Inverted) _rVal += " Inverted";
            if (LeftHanded) _rVal += " LeftHanded";
            if (!string.IsNullOrWhiteSpace(Tag)) _rVal += $" TAG:{Tag}";
            if (PartIndex > 0) _rVal += $" PART INDEX:{PartIndex}";
            return _rVal;
        }

        public uopVectors ApplyTo(uopVectors aVectors, uopVector aBasePt)
        {
            if (aVectors == null) return null;
            aBasePt ??= uopVector.Zero;
            uopVectors _rVal = new uopVectors(aVectors);
            uopVector center = aBasePt == null ? new uopVector(_rVal.Bounds.Center) : aBasePt;

            if (Rotation != 0)
                _rVal.Rotate(center, Rotation);

            if (LeftHanded)
                _rVal.Mirror(center.X, null);
            if (Inverted)
                _rVal.Mirror(null,center.Y);

            _rVal.Move(DX, DY);
      
            return _rVal;
        }

        public uopVector ApplyTo(uopVector aVector, uopVector aBasePt)
        {
            if (aVector == null) return null;
            aBasePt ??= uopVector.Zero;
            uopVector _rVal = new uopVector(aVector)
            {
                Value = mzUtils.NormAng(aBasePt.Value + Rotation, false, true),
                X = aVector.X + DX,
                Y = aVector.Y + DY,

            };
            return _rVal;
        }
        internal UVECTOR ApplyTo(UVECTOR aVector, uopVector aBasePt)
        {
            if (aVector == null) return UVECTOR.Zero;
            aBasePt ??= uopVector.Zero;
            UVECTOR _rVal = new UVECTOR(aVector)
            {
                Value = mzUtils.NormAng(aBasePt.Value + Rotation, false, true),
                X = aVector.X + DX,
                Y = aVector.Y + DY,

            };
            return _rVal;
        }
        public new uopInstance Clone() => new uopInstance(this);

        object ICloneable.Clone() => (object)this.Clone();
        public dxoInstance ToDXFInstance() => new dxoInstance(this);

        #endregion Methods

    }

}
