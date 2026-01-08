using ClosedXML.Excel;
using ClosedXML.Excel.CalcEngine.Functions;
using Force.DeepCloner;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Security.AccessControl;
using System.Threading.Tasks;
using UOP.DXFGraphics;
using UOP.DXFGraphics.Utilities;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Events;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Interfaces;
using UOP.WinTray.Projects.Materials;
using UOP.WinTray.Projects.Parts;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;
using static System.Net.WebRequestMethods;

namespace UOP.WinTray.Projects.Structures
{


    internal struct TTABLECELL : ICloneable
    {
        public int Column;
        public int Row;
        public double Width;
        public double Height;
        public double Length;
        public double Size;
        public mzTextFormats TextFormat;
        public string NumberFormat;
        public int StartColumn;
        public int StartRow;
        public uopAlignments Alignment;
        public bool WrapText;
        public string FontName;
        public double FontSize;
        public bool Locked;
        public string Address;
        public double Orientation;
        public int HTMLFontSize;
        //public dynamic Value;
        public System.Drawing.Color FontColor;
        public System.Drawing.Color BackColor;
        public dynamic Numeric;
        public int[,] BorderData;
        public bool DataOnly;
        public TPROPERTY SourceProp;


        public TTABLECELL(int aRow = 1, int aCol = 1)
        {
            Column = aCol;
            Row = aRow;
            Width = 0;
            Height = 1;
            Length = 1;
            Size = 1;
            TextFormat = mzTextFormats.None;
            NumberFormat = string.Empty;
            StartColumn = 1;
            StartRow = 1;
            Alignment = uopAlignments.General;
            WrapText = false;
            FontName = string.Empty;
            FontSize = 0;
            Locked = false;
            Address = string.Empty;
            Orientation = 0;
            HTMLFontSize = 0;
            //Value = string.Empty;
            FontColor = System.Drawing.Color.Black;
            BackColor = System.Drawing.Color.Transparent;
            Numeric = false;
            BorderData = new int[4, 2];
            DataOnly = false;
            SourceProp = new TPROPERTY("CELLDATA", "");

        }



        public TTABLECELL Clone()
        {
            return new TTABLECELL(Row, Column)
            {

                Width = Width,
                Height = Height,
                Length = Length,
                Size = Size,
                TextFormat = TextFormat,
                NumberFormat = NumberFormat,
                StartColumn = StartColumn,
                StartRow = StartRow,
                Alignment = Alignment,
                WrapText = WrapText,
                FontName = FontName,
                FontSize = FontSize,
                Locked = Locked,
                Address = Address,
                Orientation = Orientation,
                HTMLFontSize = HTMLFontSize,
                FontColor = FontColor,
                BackColor = BackColor,
                Numeric = Numeric,
                BorderData = Force.DeepCloner.DeepClonerExtensions.DeepClone<int[,]>(BorderData),
                DataOnly = DataOnly,
                SourceProp = new TPROPERTY(SourceProp)
            };
        }

        public dynamic Value { get => SourceProp.Value; set => SourceProp.SetValue(value ?? "", SourceProp.Value); }

        public uppUnitTypes UnitType { get => SourceProp.UnitType; set => SourceProp.UnitType = value; }
        object ICloneable.Clone() => (object)Clone();
    }
    internal struct TMDFEEDZONE : ICloneable
    {
        public int PanelIndex;
        public int DowncomerIndex;
        public string RangeGUID;
        public string ProjectHandle;
        public USHAPE Bounds;

        public TMDFEEDZONE(int aDowncomerIndex = -1, int aPanelIndex = -1)
        {
            PanelIndex = aPanelIndex;
            DowncomerIndex = aDowncomerIndex;
            RangeGUID = string.Empty;
            ProjectHandle = string.Empty;
            Bounds = new USHAPE("BOUNDS");
        }
        public TMDFEEDZONE(TMDFEEDZONE aStructure)
        {
            PanelIndex = aStructure.PanelIndex;
            DowncomerIndex = aStructure.DowncomerIndex;
            RangeGUID = aStructure.RangeGUID;
            ProjectHandle = aStructure.ProjectHandle;
            Bounds = new USHAPE(aStructure.Bounds);
        }
        public TMDFEEDZONE Clone() => new TMDFEEDZONE(this);

        object ICloneable.Clone() => (object)Clone();


    }
 
    internal struct TCLIP : ICloneable
    {
        public double HoleSpan;
        public double SlotSpan;
        public double DownSet;
        public double Inset;
        public double Width;
        public double Height;
        public double Length;
        public double HoleDiameter;
        public double SlotLength;
        public string Owner;
        public double BendGap;
        public double BendAngle;
        public double SlotWidth;
        public double HoleInset;
        public double SlotInset;
        public dxxRadialDirections XDirection;
        public dxxRadialDirections YDirection;
        public double TabHeight;
        public double TabWidth;
        public double LipLength;
        public double Rotation;
        public double X;
        public double Y;
        public double Z;

        public TCLIP(double aHeight = 0, double aWidth = 0, double aLength = 0)
        {
            HoleSpan = 0;
            SlotSpan = 0;
            DownSet = 0;
            Inset = 0;
            Width = aWidth;
            Height = aHeight;
            Length = aLength;
            HoleDiameter = 0;
            SlotLength = 0;
            Owner = string.Empty;
            BendGap = 0;
            BendAngle = 0;
            SlotWidth = 0;
            HoleInset = 0;
            SlotInset = 0;
            XDirection = dxxRadialDirections.AwayFromCenter;
            YDirection = dxxRadialDirections.AwayFromCenter;
            TabHeight = 0;
            TabWidth = 0;
            LipLength = 0;
            Rotation = 0;
            X = 0;
            Y = 0;
            Z = 0;

        }

        public TCLIP Clone()
        {
            return new TCLIP
            {
                HoleSpan = HoleSpan,
                SlotSpan = SlotSpan,
                DownSet = DownSet,
                Inset = Inset,
                Width = Width,
                Height = Height,
                Length = Length,
                HoleDiameter = HoleDiameter,
                SlotLength = SlotLength,
                Owner = Owner,
                BendGap = BendGap,
                BendAngle = BendAngle,
                SlotWidth = SlotWidth,
                HoleInset = HoleInset,
                SlotInset = SlotInset,
                XDirection = XDirection,
                YDirection = YDirection,
                TabHeight = TabHeight,
                TabWidth = TabWidth,
                LipLength = LipLength,
                Rotation = Rotation,
                X = X,
                Y = Y,
                Z = Z,
            };
        }
        object ICloneable.Clone() => (object)Clone();
    }

    internal struct TSPOUTGROUP : ICloneable
    {
        internal TSTARTUPLINES SUEdges;
        internal TSTARTUPLINES SULines;
        internal USHAPE StartupBound;
        internal USHAPE Perimeter;
      
        internal URECTANGLE Limits;
        public int DowncomerIndex;
        public int PanelIndex;
        public int GroupIndex;
        public int ArrayIndex;

        public bool TriangularBounds;
        public bool HasChanged;
        public double ZOrd;
        public UVECTOR Center;


        public TSPOUTGROUP(int aDowncomerIndex = -1, int aPanelIndex = -1)
        {
            SUEdges = new TSTARTUPLINES();
            SULines = new TSTARTUPLINES();
            StartupBound = new USHAPE();
            Perimeter = new USHAPE();
     
            Limits = URECTANGLE.Null;
            DowncomerIndex = aDowncomerIndex;
            PanelIndex = aPanelIndex;
            GroupIndex = -1;
            ArrayIndex = -1;
            _TheoreticalArea = 0;
            TriangularBounds = false;
            HasChanged = false;
            ZOrd = 0;
            Center = UVECTOR.Zero;
        }

        object ICloneable.Clone() => (object)Clone();

        public TSPOUTGROUP Clone()
        {
            return new TSPOUTGROUP(DowncomerIndex, PanelIndex)
            {
                SUEdges = new TSTARTUPLINES(SUEdges),
                SULines = new TSTARTUPLINES(SULines),
                StartupBound = new USHAPE(StartupBound),
                Perimeter = new USHAPE(Perimeter),
         
                Limits = new URECTANGLE(Limits),
                DowncomerIndex = DowncomerIndex,
                PanelIndex = PanelIndex,
                GroupIndex = GroupIndex,
                ArrayIndex = ArrayIndex,
                _TheoreticalArea = _TheoreticalArea,
                TriangularBounds = TriangularBounds,
                HasChanged = HasChanged,
                ZOrd = ZOrd,
                Center = new UVECTOR(Center)
            };

        }

        private double _TheoreticalArea;
        public double TheoreticalArea
        {
            get => _TheoreticalArea;
            set
            {
                if (double.IsNaN(value))
                    value = 0;
                _TheoreticalArea = value;
            }
        }
        public double X { get => Center.X; set => Center.X = value; }
        public double Y { get => Center.Y; set => Center.Y = value; }

        public double PanelY => Limits.Bottom + (Limits.Height / 2);

      
        /// <summary>
        /// returns true if the startup edges are defined
        /// </summary>
        /// <param name="rHasBound"></param>
        /// <param name="rHasLines"></param>
        /// <returns></returns>
        public bool HasStartupObjects(out bool rHasBound, out bool rHasLines)
        {

            rHasBound = StartupBound.Vertices.Count > 0;
            rHasLines = SULines.Count > 0;
            return rHasLines && rHasBound;
        }


        public void Mirror(double? aX, double? aY)
        {
            if (!aX.HasValue && !aY.HasValue) return;
            double dx = aX.HasValue ? X - aX.Value : 0;
            double dy = aY.HasValue ? Y - aY.Value : 0;
            if (dx == 0 && dy == 0) return;

            Center.Mirror(aX, aY);
            Limits.Mirror(aX, aY);
            Perimeter.Mirror(aX, aY);
            SUEdges.Mirror(aX, aY);
            SULines.Mirror(aX, aY);
     
        }
    }

    internal struct THARDWARE : ICloneable
    {

        public double H;
        public double lt;
        public double dia;
        public double f;
        public double G;
        public double Length;
        public double Face;
        public double ThreadedLength;
        public double ObscuredLength;
        public bool DoubleNut;
        public double MinorDia;
        public double ID;
        public double OD;

        private uppWasherTypes _WasherType;
        private uppWasherSeries _WasherSeries;
        public uppHardwareTypes Type;
        public TMATERIAL Material;
        public UVECTOR Center;
        private uppHardwareSizes _Size;


        public THARDWARE(uppHardwareTypes aType, uppHardwareSizes aSize)
        {
            Type = aType;
            _Size = aSize;
            Material = new TMATERIAL(uppMaterialTypes.Hardware);
            Center = UVECTOR.Zero;
            _WasherType = uppWasherTypes.TypeB;
            _WasherSeries = uppWasherSeries.Regular;
            H = 0;
            lt = 0;
            dia = 0;
            f = 0;
            G = 0;
            Length = 0;
            Face = 0;
            ThreadedLength = 0;
            ObscuredLength = 0;
            DoubleNut = false;
            MinorDia = 0;
            ID = 0;
            OD = 0;
            UpdateDimensions();


        }

        public uppWasherTypes WasherType { get => _WasherType; set { _WasherType = value; UpdateDimensions(); } }
        public uppWasherSeries WasherSeries { get => _WasherSeries; set { _WasherSeries = value; UpdateDimensions(); } }


        public bool IsMetric
        {
            get => (int)Size >= 100;
            set
            {
                if ((int)Size >= 100)
                {
                    if (value == false)
                    {
                        if (Size == uppHardwareSizes.M10) { Size = uppHardwareSizes.ThreeEights; }
                        else if (Size == uppHardwareSizes.M12) { Size = uppHardwareSizes.OneHalf; }

                        UpdateDimensions();
                    }

                }
                else
                {
                    if (value == true)
                    {
                        if (Size == uppHardwareSizes.ThreeEights) { Size = uppHardwareSizes.M10; }
                        else if (Size == uppHardwareSizes.OneHalf) { Size = uppHardwareSizes.M12; }
                        UpdateDimensions();
                    }
                }

            }
        }
        public void UpdateDimensions()
        {
            if (IsWasher)
            {
                THARDWARE.SetWasherDimensions(ref this);
            }
            else
            {
                THARDWARE.SetHexDimensions(ref this);
            }

        }
        public double Thickness { get => Material.Thickness; set => Material.Thickness = value; }

        public double X { get => Center.X; set => Center.X = value; }
        public double Y { get => Center.Y; set => Center.Y = value; }
        public double Z { get => Center.Elevation ?? 0; set => Center.Elevation = value; }

        public uppHardwareSizes Size { get => _Size; set { _Size = value; } }

        public bool HasLength => Type == uppHardwareTypes.SetScrew || Type == uppHardwareTypes.HexBolt || Type == uppHardwareTypes.ShavedStud || Type == uppHardwareTypes.Stud;


        public bool IsWasher => Type == uppHardwareTypes.FlatWasher || Type == uppHardwareTypes.LockWasher;
        object ICloneable.Clone() => (object)Clone();

        public THARDWARE Clone()
        {
            return new THARDWARE()
            {
                Type = Type,
                _Size = _Size,
                Material = Material.Clone(),
                Center = Center.Clone(),
                _WasherType = _WasherType,
                _WasherSeries = _WasherSeries,
                H = H,
                lt = lt,
                dia = dia,
                f = f,
                G = G,
                Length = Length,
                Face = Face,
                ThreadedLength = ThreadedLength,
                ObscuredLength = ObscuredLength,
                DoubleNut = DoubleNut,
                MinorDia = MinorDia,
                ID = ID,
                OD = OD,

            };
        }

        #region Shared Methods
        /// <summary>
        /// internal function executed when the Size property is set. Sets the washer dimensions
        /// </summary>
        public static void SetWasherDimensions(ref THARDWARE aWasher)
        {


            if (aWasher.Type == uppHardwareTypes.LargeODWasher)
            {
                if (aWasher.IsMetric)
                {
                    aWasher.OD = 44 / 25.4;
                    aWasher.ID = 11 / 25.4;

                }
                else
                {
                    aWasher.OD = 1.75;
                    aWasher.ID = mdGlobals.gsSmallHole;
                }

            }

            if (aWasher.Size == uppHardwareSizes.ThreeEights)
            {
                if (aWasher.WasherType == uppWasherTypes.TypeA)
                {
                    if (aWasher.WasherSeries == uppWasherSeries.Regular)
                    {
                        aWasher.WasherSeries = uppWasherSeries.Narrow;
                    }
                    if (aWasher.WasherSeries == uppWasherSeries.Narrow)
                    {
                        aWasher.ID = mdGlobals.gsSmallHole;
                        aWasher.OD = 0.812;
                        aWasher.Material.Thickness = 0.065;
                    }
                    else
                    {
                        aWasher.ID = mdGlobals.gsBigHole;
                        aWasher.OD = 1;
                        aWasher.Material.Thickness = 0.083;
                    }
                }
                if (aWasher.WasherType == uppWasherTypes.TypeB)
                {
                    if (aWasher.WasherSeries == uppWasherSeries.Narrow)
                    {
                        aWasher.ID = mdGlobals.gsSmallHole;
                        aWasher.OD = 0.734;
                        aWasher.Material.Thickness = 0.063;
                    }

                    if (aWasher.WasherSeries == uppWasherSeries.Regular)
                    {
                        aWasher.ID = mdGlobals.gsSmallHole;
                        aWasher.OD = 1;
                        aWasher.Material.Thickness = 0.063;
                    }

                    if (aWasher.WasherSeries == uppWasherSeries.Wide)
                    {
                        aWasher.ID = mdGlobals.gsSmallHole;
                        aWasher.OD = 1.25;
                        aWasher.Material.Thickness = 0.1;
                    }

                }

            }

            if (aWasher.Size == uppHardwareSizes.OneHalf)
            {
                if (aWasher.WasherType == uppWasherTypes.TypeA)
                {
                    if (aWasher.WasherSeries == uppWasherSeries.Regular)
                    {
                        aWasher.WasherSeries = uppWasherSeries.Narrow;
                    }

                    if (aWasher.WasherSeries == uppWasherSeries.Narrow)
                    {
                        aWasher.ID = 0.531;
                        aWasher.OD = 1.062;
                        aWasher.Material.Thickness = 0.095;
                    }
                    else
                    {
                        aWasher.ID = 0.561;
                        aWasher.OD = 1.375;
                        aWasher.Material.Thickness = 0.109;
                    }
                }
                if (aWasher.WasherType == uppWasherTypes.TypeB)
                {
                    if (aWasher.WasherSeries == uppWasherSeries.Narrow)
                    {
                        aWasher.ID = 0.531;
                        aWasher.OD = 1.062;
                        aWasher.Material.Thickness = 0.095;
                    }

                    if (aWasher.WasherSeries == uppWasherSeries.Regular)
                    {
                        aWasher.ID = 0.531;
                        aWasher.OD = 1.062;
                        aWasher.Material.Thickness = 0.095;
                    }

                    if (aWasher.WasherSeries == uppWasherSeries.Wide)
                    {
                        aWasher.ID = 0.561;
                        aWasher.OD = 1.375;
                        aWasher.Material.Thickness = 0.109;
                    }

                }

            }

            if (aWasher.Size == uppHardwareSizes.M10)
            {
                if (aWasher.WasherType == uppWasherTypes.TypeB)
                {
                    aWasher.WasherType = uppWasherTypes.TypeA;
                }
                if (aWasher.WasherSeries == uppWasherSeries.Narrow)
                {
                    aWasher.ID = (((11.12 - 10.85) / 2) + 10.85) / 25.4;
                    aWasher.OD = (((20 - 19.48) / 2) + 19.48) / 25.4;
                    aWasher.Material.Thickness = (((2.3 - 1.6) / 2) + 1.6) / 25.4;
                }

                if (aWasher.WasherSeries == uppWasherSeries.Regular)
                {
                    aWasher.ID = (((11.12 - 10.85) / 2) + 10.85) / 25.4;
                    aWasher.OD = (((28 - 27.48) / 2) + 27.48) / 25.4;
                    aWasher.Material.Thickness = (((2.8 - 2) / 2) + 2) / 25.4;
                }

                if (aWasher.WasherSeries == uppWasherSeries.Wide)
                {
                    aWasher.ID = (((11.12 - 10.85) / 2) + 10.85) / 25.4;
                    aWasher.OD = (((39 - 38.38) / 2) + 38.38) / 25.4;
                    aWasher.Material.Thickness = (((3.5 - 2.5) / 2) + 2.5) / 25.4;
                }

            }

            if (aWasher.Size == uppHardwareSizes.M12)
            {
                if (aWasher.WasherType == uppWasherTypes.TypeB)
                {
                    aWasher.WasherType = uppWasherTypes.TypeA;
                }

                aWasher.ID = 13 / 25.4;
                aWasher.OD = 24 / 25.4;
                aWasher.Material.Thickness = 3 / 25.4;
                if (aWasher.WasherSeries == uppWasherSeries.Wide)
                {
                    aWasher.OD = 40 / 25.4;

                }


            }

        }

        public static void SetHexDimensions(ref THARDWARE aNut)
        {


            if (aNut.Size == uppHardwareSizes.ThreeEights)
            {
                aNut.H = 21d / 64d;
                aNut.dia = 0.375d;
                aNut.f = 9d / 16d;
                aNut.G = 0.639d;
            }
            else if (aNut.Size == uppHardwareSizes.M10)
            {
                aNut.H = 8.9d / 25.4d;
                aNut.dia = 10d / 25.4d;
                aNut.f = 17d / 25.4d;
                aNut.G = 19.63 / 25.4;
            }
            else if (aNut.Size == uppHardwareSizes.OneHalf)
            {
                aNut.H = 7d / 16d;
                aNut.dia = 0.5d;
                aNut.f = 0.75d;
                aNut.G = 0.846;
            }
            else if (aNut.Size == uppHardwareSizes.M12)
            {
                aNut.H = 9.5d / 25.4d;
                aNut.dia = 12d / 25.4d;
                aNut.f = 19d / 25.4d;
                aNut.G = 21.94d / 25.4d;
            }

        }

        public static THARDWARE ScaleDimensions(THARDWARE aHarware, double aScaleFactor = 1)
        {

            THARDWARE _rVal = aHarware.Clone();
            _rVal.H *= aScaleFactor;
            _rVal.dia *= aScaleFactor;
            _rVal.f *= aScaleFactor;
            _rVal.G *= aScaleFactor;
            _rVal.Length *= aScaleFactor;
            _rVal.ThreadedLength *= aScaleFactor;
            _rVal.ID *= aScaleFactor;
            _rVal.OD *= aScaleFactor;
            _rVal.MinorDia *= aScaleFactor;
            _rVal.Face *= aScaleFactor;
            _rVal.ObscuredLength *= aScaleFactor;

            return _rVal;
        }
        #endregion Shared Methods


    }



    internal struct TFLOWSLOT : ICloneable
    {
        public uppFlowSlotTypes SlotType;
        public double Height;
        public double Width;
        public double Angle;
        public int PanelIndex;
        public int SectionIndex;
        public double DieHeight;
        public double DieWidth;
        public string SlotAlignmentCode;
        public string SlotTypeCode;
        public UVECTOR Center;

        public TFLOWSLOT(uppFlowSlotTypes aType = uppFlowSlotTypes.HalfC)
        {
            SlotType = aType;
            Height = mdGlobals.SlotHeight; // 0.7567;
            Width = mdGlobals.SlotWidth; //0.3865;
            Angle = 0;
            PanelIndex = -1;
            SectionIndex = -1;
            DieHeight = mdGlobals.SlotDieHeight;
            DieWidth = mdGlobals.SlotDieWidth;
            SlotAlignmentCode = string.Empty;
            SlotTypeCode = string.Empty;
            Center = UVECTOR.Zero;

        }

        public TFLOWSLOT(TFLOWSLOT aSlot)
        {
            SlotType = aSlot.SlotType;
            Height = aSlot.Height;
            Width = aSlot.Width;
            Angle = aSlot.Angle;
            PanelIndex = aSlot.PanelIndex;
            SectionIndex = aSlot.SectionIndex;
            DieHeight = aSlot.DieHeight;
            DieWidth = aSlot.DieWidth;
            SlotAlignmentCode = aSlot.SlotAlignmentCode;
            SlotTypeCode = aSlot.SlotTypeCode;
            Center = new UVECTOR(aSlot.Center);

        }


        public double X { get => Center.X; set => Center.X = value; }
        public double Y { get => Center.Y; set => Center.Y = value; }



        /// <summary>
        ///returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>
        object ICloneable.Clone() => (object)Clone();

        public TFLOWSLOT Clone() => new TFLOWSLOT(this);

        public double DieArea => DieWidth * DieHeight;

        public string SlotTypeName => uopEnums.Description(SlotType);

    }

    internal struct TSTARTUPLINE : ICloneable
    {
        public int SpoutGroupID;
        public int SpoutGroupIndex;
        public int DowncomerIndex;
        public int BoxIndex;
        public double SpoutGroupArea;
        public int Occurs;
        public string SpoutGroupHandle;
        public ULINE Core;
        public UVECTOR ReferencePt;
        public int Index;
        public double MinLength;
        public string Tag;
        public string LineType;
        public double Z;
        public bool Suppressed;
        public bool Mark;
    
        public double? MirrorY;
        public double? MirrorX;

        public TSTARTUPLINE(int aDCindex = -1)
        {
            DowncomerIndex = aDCindex;
            BoxIndex = 0;
            SpoutGroupID = 0;
            SpoutGroupIndex = -1;
            SpoutGroupArea = 0;
            Occurs = 1;
            SpoutGroupHandle = string.Empty;
            Core =  ULINE.Null;
            ReferencePt = UVECTOR.Zero;
            Index = 0;
            MinLength = 0;
            Tag = string.Empty;
            LineType = string.Empty;
            Z = 0;
            Suppressed = false;
            MirrorY = null;
            MirrorX = null;
            Mark = false;
        }

        public TSTARTUPLINE(mdStartupLine aLine)
        {
            DowncomerIndex = -1;
            BoxIndex = 0;
            SpoutGroupID = 0;
            SpoutGroupIndex = -1;
            SpoutGroupArea = 0;
            Occurs = 1;
            SpoutGroupHandle = string.Empty;
            Core = ULINE.Null;
            ReferencePt = UVECTOR.Zero;
            Index = 0;
            MinLength = 0;
            Tag = string.Empty;
            LineType = string.Empty;
            Z = 0;
            Suppressed = false;
            MirrorY = null;
            MirrorX = null;
            Mark = false;
            if (aLine == null) return;
            DowncomerIndex = aLine.DowncomerIndex;
            BoxIndex = aLine.BoxIndex;
            SpoutGroupArea = aLine.SpoutGroupArea;
            Occurs = aLine.Occurs;
            SpoutGroupHandle = aLine.SpoutGroupHandle;
            Core = new ULINE(aLine);
            ReferencePt = new UVECTOR(aLine.ReferencePt);
            Index = aLine.Index;
            MinLength = aLine.MinLength;
            Tag = aLine.Tag;
            LineType = aLine.LineType;
            Z = aLine.Z;
            Suppressed = aLine.Suppressed;
    
            MirrorY = aLine.MirrorY;
            MirrorX = aLine.MirrorX;
            Mark = aLine.Mark;
        }
        public TSTARTUPLINE(TSTARTUPLINE aLine)
        {
            DowncomerIndex = aLine.DowncomerIndex;
            BoxIndex = aLine.BoxIndex;
            SpoutGroupID = aLine.SpoutGroupID;
            SpoutGroupIndex = aLine.SpoutGroupIndex;
            SpoutGroupArea = aLine.SpoutGroupArea;
            Occurs = aLine.Occurs;
            SpoutGroupHandle = aLine.SpoutGroupHandle;
            Core = new ULINE(aLine.Core);
            ReferencePt = new UVECTOR(aLine.ReferencePt);
            Index = aLine.Index;
            MinLength = aLine.MinLength;
            Tag = aLine.Tag;
            LineType = aLine.LineType;
            Z = aLine.Z;
            Suppressed = aLine.Suppressed;
       
            MirrorY = aLine.MirrorY;
            MirrorX = aLine.MirrorX;
            Mark = aLine.Mark;
        }

        object ICloneable.Clone() => (object)Clone();

        public TSTARTUPLINE Clone() => new TSTARTUPLINE(this);


        public UVECTOR EP { get => Core.ep; set => Core.ep = value; }
        public UVECTOR SP { get => Core.sp; set => Core.sp = value; }

        public void Mirror(double? aX, double? aY)
        {
            if (!aX.HasValue && !aY.HasValue) return;
            ReferencePt.Mirror(aX, aY);
            Core.Mirror(aX, aY);
        }


    }

    internal struct TSTARTUPLINES : ICloneable
    {
        public double SpoutLength;
        public double SpoutHeight;
        private int _Count;
        private TSTARTUPLINE[] _Members;
        private bool _Init;


        public TSTARTUPLINES(double aSpoutLength = 0, double aSpoutHeight = 0)
        {
            SpoutLength = aSpoutLength;
            SpoutHeight = aSpoutHeight;
            _Count = 0;
            _Members = new TSTARTUPLINE[0];
            _Init = true;
        }

        public TSTARTUPLINES(TSTARTUPLINES aLines)
        {
            SpoutLength = aLines.SpoutLength;
            SpoutHeight = aLines.SpoutHeight;
            _Count = 0;
            _Members = new TSTARTUPLINE[0];
            _Init = true;


            for (int i = 1; i < aLines.Count; i++)
            {
                Add(new TSTARTUPLINE(aLines.Item(i)));

            }
            _Count = _Members.Length;

        }

        private int Init() { _Count = 0; _Members = new TSTARTUPLINE[0]; _Init = true; return 0; }

        public int Count => (!_Init) ? Init() : _Count;

        public TSTARTUPLINES Clone() => new TSTARTUPLINES(this);

        public void Clear() { _Count = 0; _Members = new TSTARTUPLINE[0]; _Init = true; }

        public TSTARTUPLINE Item(int aIndex) { if (aIndex < 1 || aIndex > Count) { return new TSTARTUPLINE(); } _Members[aIndex - 1].Index = aIndex; return _Members[aIndex - 1]; }

        public void SetItem(int aIndex, TSTARTUPLINE aMember) { aMember.Index = 0; if (aIndex < 1 || aIndex > Count) { return; } aMember.Index = aIndex; _Members[aIndex - 1] = aMember; }
        public void Mirror(double? aX, double? aY)
        {
            if (!aX.HasValue && !aY.HasValue) return;
            if (Count <= 0) return;
            for (int i = 1; i <= Count; i++)
            {
                TSTARTUPLINE mem = Item(i);
                mem.Mirror(aX, aY);
                SetItem(i, mem);
            }

        }
        public TSTARTUPLINE Add(TSTARTUPLINE aLine)
        {

            { if (!_Init) { Init(); } }
            _Count += 1;
            Array.Resize<TSTARTUPLINE>(ref _Members, _Count);
            aLine.Index = _Count;
            _Members[_Count - 1] = aLine;
            return _Members[_Count - 1];
        }

        object ICloneable.Clone() => (object)Clone();
    }

    internal struct TSTARTUPSPOUT : ICloneable
    {
        public double Depth;
        public bool Suppressed;
        public bool Obscured;
        public string SpoutGroupHandle;
 
        public TSTARTUPLINE ControlLine;
        public UVECTOR Center;
        public double MaxY;
        public double MinY;
        public double Z;
        public double Height;
        public double Length;
        public int DowncomerIndex;
        public int BoxIndex;
        public int OccuranceFactor { get; set; }
        public string Tag;
        public int Index;
        /// <summary>
        ///returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>
        object ICloneable.Clone() => (object)Clone();

        public TSTARTUPSPOUT(double aHeight = 0.375, double aLength = 1)
        {
            Depth = 0;
            Suppressed = false;
            Obscured = false;
            SpoutGroupHandle = string.Empty;
            ControlLine = new TSTARTUPLINE(-1);
            Center = UVECTOR.Zero;
            MaxY = 0;
            MinY = 0;
            Z = 0;
            Length = aLength;
            Height = aHeight;
            DowncomerIndex = 0;
            BoxIndex = 0;
            OccuranceFactor = 1;
            Tag = string.Empty;
            Index = 0;
        }
        public TSTARTUPSPOUT(TSTARTUPSPOUT aSpout)
        {
            Depth = aSpout.Depth;
            Suppressed = aSpout.Suppressed;
            Obscured = aSpout.Obscured;
            SpoutGroupHandle = aSpout.SpoutGroupHandle;
            ControlLine = new TSTARTUPLINE(aSpout.ControlLine);
            Center = new UVECTOR(aSpout.Center);
            MaxY = aSpout.MaxY;
            MinY = aSpout.MinY;
            Z = aSpout.Z;
            Length = aSpout.Length;
            Height = aSpout.Height;
            DowncomerIndex = aSpout.DowncomerIndex;
            BoxIndex = aSpout.BoxIndex;
            OccuranceFactor = aSpout.OccuranceFactor;
            Tag = aSpout.Tag;
            Index = aSpout.Index;
        }


        public TSTARTUPSPOUT Clone() => new TSTARTUPSPOUT(this);
       
        /// <summary>
        ///  the handle of the startup spout
        /// like 2,4,UL
        /// </summary>
        public string Handle
        {
            get
            {
                string _rVal = SpoutGroupHandle;
                if (_rVal !=  string.Empty) _rVal += ",";

                _rVal += Tag;
                return _rVal;
            }
        }

        public override string ToString()
        {
            return $"TSTRARTUPSPOUT [{ Handle }]";
        }

    }


    internal struct TOBJECT : ICloneable
    {
        public string Name;
        public int Index;
        public TPROPERTIES Properties;

        public TOBJECT(string aName = "") { Index = 0; Name = aName; Properties = new TPROPERTIES(aName); }
        object ICloneable.Clone() => (object)Clone();

        public TOBJECT Clone() { return new TOBJECT() { Name = Name, Index = Index, Properties = Properties.Clone() }; }

    }

    internal struct TOBJECTS : ICloneable
    {
        public string Name;
        public string FileName;

        private TOBJECT[] _Members;
        private int _Count;
        private bool _Init;

        public TOBJECTS(string aName = "") { Name = aName; FileName = string.Empty; _Count = 0; _Members = new TOBJECT[0]; _Init = true; }

        public TOBJECTS(TOBJECTS aObjects, bool bClear = false)
        {
            Name = aObjects.Name;
            FileName = aObjects.FileName;
            _Init = true;
            _Members = new TOBJECT[0];
            _Count = bClear ? 0 : aObjects.Count;

            if (_Count > 0)
            {
                _Members = Force.DeepCloner.DeepClonerExtensions.DeepClone<TOBJECT[]>(aObjects._Members);
            }
        }
        private int Init() { Name = string.Empty; FileName = string.Empty; _Count = 0; _Members = new TOBJECT[0]; _Init = true; return 0; }

        public int Count => (!_Init) ? Init() : _Count;

        object ICloneable.Clone() => (object)Clone();

        public TOBJECTS Clone() => new TOBJECTS(this);

        public void Clear() { _Count = 0; _Members = new TOBJECT[0]; _Init = true; }


        public TOBJECT Item(int aIndex) { if (aIndex < 1 || aIndex > Count) { return new TOBJECT(); } _Members[aIndex - 1].Index = aIndex; return _Members[aIndex - 1]; }
        public TOBJECT Add(TOBJECT aMember)
        {
            if (Count + 1 > Int32.MaxValue) { return aMember; }

            _Count += 1;
            Array.Resize<TOBJECT>(ref _Members, _Count);
            _Members[_Count - 1] = (TOBJECT)aMember.Clone();
            _Members[_Count - 1].Index = _Count;
            return Item(Count);
        }


        public void ReadFromFile(string aFileSpec, string aObjectTypeName)
        {
            if (string.IsNullOrWhiteSpace(aFileSpec)) return;
            if (!System.IO.File.Exists(aFileSpec)) return;


            string pnames = string.Empty;
            TOBJECT aObj;
            TOBJECT bObj;
            int pcnt = 0;
            string pname = string.Empty;
            string strHds = string.Empty;
            int ocnt = 0;
            string oname = string.Empty;
            int nidx = 0;
            TVALUES hVals = new TVALUES();
            TVALUES sVals = new TVALUES();
            TPROPERTY prop;

            pnames = uopUtils.ReadINI_String(aFileSpec, aObjectTypeName, "PropNames");
            if (string.IsNullOrWhiteSpace(pnames)) return;



            strHds = uopUtils.GetFileHeadingsList(aFileSpec, true, aObjectTypeName);
            hVals = TVALUES.FromDelimitedList(strHds, ",");
            ocnt = hVals.Count;
            if (ocnt <= 0) return;


            FileName = aFileSpec;

            sVals = TVALUES.FromDelimitedList(pnames, ",");
            aObj = new TOBJECT("");
            pcnt = sVals.Count;
            for (int i = 1; i <= sVals.Count; i++)
            {
                pname = Convert.ToString(sVals.Item(i));
                if (!string.IsNullOrWhiteSpace(pname))
                {
                    aObj.Properties.Add(pname, "");
                    if (string.Compare(pname, "Name", ignoreCase: true) == 0) nidx = i;
                }

            }

            for (int i = 1; i <= hVals.Count; i++)
            {
                oname = Convert.ToString(hVals.Item(i));
                if (!string.IsNullOrWhiteSpace(oname))
                {
                    bObj = aObj.Clone();
                    bObj.Name = oname;
                    for (int j = 1; j <= bObj.Properties.Count; i++)
                    {
                        prop = bObj.Properties.Item(j);
                        pname = prop.Name;
                        prop.Value = uopUtils.ReadINI_String(aFileSpec, oname, pname);

                        if (j == nidx) bObj.Name = prop.ValueS;
                    }
                    Add(bObj);
                }
            }
        }

        public TOBJECT Member(string aName)
        {
            for (int i = 1; i <= Count; i++)
            {
                if (string.Compare(_Members[i - 1].Name, aName, ignoreCase: true) == 0) return _Members[i - 1];
            }
            return new TOBJECT("");
        }

    }

    internal struct TMATERIAL : ICloneable
    {
        public uppMaterialTypes Type;
        public uppMetalFamilies Family;
        public uppSheetGages SheetGage;

        public string Spec;
        public bool IsDefault;
        public dynamic Tag;
        public uppPartTypes PartType;
        public string MaterialName;
        public int Index;
        public uppPipeSchedules Schedule;
        public uppTubeSizes TubeSize;
        public string SpanName;
        public int PartIndex;
        public string RangeGUID;
        private double _Thickness;
        private string _GageName;
        private string _FamilyName;
        private bool _IsMetric;
        private bool _IsStainless;
        private double _Density;

        public TMATERIAL(uppMaterialTypes aType = uppMaterialTypes.Undefined)
        {
            Type = aType;
            _FamilyName = string.Empty;
            Family = uppMetalFamilies.CarbonSteel;
            SheetGage = uppSheetGages.Gage12;
            _GageName = string.Empty;
            Spec = string.Empty;
            _Thickness = 0;
            _Density = 0;
            IsDefault = false;
            Tag = string.Empty;
            PartType = uppPartTypes.Undefined;
            MaterialName = Family.Description();
            Index = 0;
            Schedule = uppPipeSchedules.uopStandard;
            TubeSize = uppTubeSizes.One;
            SpanName = string.Empty;
            PartIndex = 0;
            RangeGUID = string.Empty;
            _IsStainless = false;
            _IsMetric = false;

        }


        public TMATERIAL(TMATERIAL aMaterial)
        {
            Type = aMaterial.Type;
            Family = aMaterial.Family;
            SheetGage = aMaterial.SheetGage;
            _GageName = aMaterial._GageName;
            Spec = aMaterial.Spec;
            _Thickness = aMaterial._Thickness;
            _Density = aMaterial.Density;
            IsDefault = aMaterial.IsDefault;
            Tag = aMaterial.Tag;
            PartType = aMaterial.PartType;
            MaterialName = aMaterial.MaterialName;
            Index = aMaterial.Index;
            Schedule = aMaterial.Schedule;
            TubeSize = aMaterial.TubeSize;
            SpanName = aMaterial.SpanName;
            PartIndex = aMaterial.PartIndex;
            RangeGUID = aMaterial.RangeGUID;
            _IsStainless = aMaterial._IsStainless;
            _IsMetric = aMaterial._IsMetric;
            _FamilyName = aMaterial._FamilyName;
        }
        /// <summary>
        ///returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>
        object ICloneable.Clone() => (object)Clone();

        /// <summary>
        /// the thickness multiplied by the density
        ///~used to calculate weight given as surface are of the material
        /// </summary>
        /// <param name="aSMT"></param>
        /// <returns></returns>
        public double WeightMultiplier
        {
            get
            {

                return (Type == uppMaterialTypes.SheetMetal) ? Density * Thickness : 1;
            }
        }


        /// <summary>
        /// the density of the material given in lb per cubic inch
        /// </summary>
        /// <param name="aSMT"></param>
        /// <returns></returns>
        public double Density
        {
            get
            {
                double _rVal = _Density;

                if (Type == uppMaterialTypes.SheetMetal)
                {
                    if (Family == uppMetalFamilies.CarbonSteel || Family == uppMetalFamilies.CarbonSteel_Killed)
                    {
                        _rVal = 0.29039;

                    }
                    else if ((Family == uppMetalFamilies.Stainless_304) || (Family == uppMetalFamilies.Stainless_304L) || (Family == uppMetalFamilies.Stainless_316) || (Family == uppMetalFamilies.Stainless_410) || (Family == uppMetalFamilies.Stainless_316L))
                    {
                        _rVal = 0.28611;

                    }
                    else
                    {
                        _rVal = _Density;
                    }
                    _rVal = Math.Round(_rVal, 5);
                }


                return _rVal;
            }
            set => _Density = Math.Round(Math.Abs(value), 5);
        }

        public string SpecFilter => TMATERIAL.SheetMetalFamilyFilter(Family);

        public TMATERIAL Clone() => new TMATERIAL(this);


        public override string ToString() => Descriptor;

        /// <summary>
        /// flag indicating if the material is a stainless steel
        /// </summary>
        /// <returns></returns>
        public bool IsStainless
        {
            get
            {


                if (Type == uppMaterialTypes.SheetMetal)
                {


                    if (Family == uppMetalFamilies.CarbonSteel || Family == uppMetalFamilies.CarbonSteel_Killed)
                    { return false; }
                    else if ((Family == uppMetalFamilies.Stainless_304) || (Family == uppMetalFamilies.Stainless_304L) || (Family == uppMetalFamilies.Stainless_316) || (Family == uppMetalFamilies.Stainless_316L) || (Family == uppMetalFamilies.Stainless_410))
                    { return true; }
                    else
                    { return _IsStainless; }

                }


                return _IsStainless;
            }

            set => _IsStainless = value;
        }

        /// <summary>
        /// flag indicating if the material is a metric material
        /// </summary>
        /// <returns></returns>
        public bool IsMetric

        {
            get
            {

                if (Type == uppMaterialTypes.SheetMetal)
                {
                    switch (SheetGage)
                    {
                        case uppSheetGages.Gage14:
                        case uppSheetGages.Gage12:
                        case uppSheetGages.Gage10:
                            return false;

                        case uppSheetGages.Gage4mm:
                        case uppSheetGages.Gage3pt5mm:
                        case uppSheetGages.Gage3mm:
                        case uppSheetGages.Gage2pt7mm:
                        case uppSheetGages.Gage2pt5mm:
                        case uppSheetGages.Gage2mm:
                            return true;

                        default:
                            return _IsMetric;

                    }
                }
                return _IsMetric;

            }
            set => _IsMetric = value;
        }

        public void SubPart(uopPart aPart)
        {
            if (aPart == null) return;
            PartIndex = aPart.Index;
            PartType = aPart.PartType;
            RangeGUID = aPart.RangeGUID;
        }

        /// <summary>
        ///returns a string that describes the gage of the material based on the SheetGage Property
        /// like "10 ga","3mm" etc.  If the SheetGage is Undefined then value set is returned
        /// </summary>
        /// <param name="aMaterial"></param>
        /// <returns></returns>
        public string GageName
        {
            get
            {
                return ((int)SheetGage >= 0) ? SheetGage.GetDescription() : _GageName;
            }

            set
            {
                _GageName = value;
                int gage = uopEnumHelper.GetValueByDescription(typeof(uppSheetGages), value);

                if (gage > 0) { SheetGage = (uppSheetGages)gage; } else { SheetGage = uppSheetGages.GageUnknown; }

            }
        }

        /// <summary>
        /// The material thickness
        /// </summary>
        /// <param name="aMaterial"></param>
        /// <returns></returns>
        public double Thickness
        {
            get
            {


                double _rVal = 0.0;

                switch (Type)
                {
                    case uppMaterialTypes.SheetMetal:
                        {

                            return SheetGage switch
                            {
                                uppSheetGages.Gage10 => 0.135,
                                uppSheetGages.Gage12 => 0.105,
                                uppSheetGages.Gage14 => 0.075,
                                uppSheetGages.Gage2mm => 2 / 25.4,
                                uppSheetGages.Gage2pt5mm => 2.5 / 25.4,
                                uppSheetGages.Gage2pt7mm => 2.7 / 25.4,
                                uppSheetGages.Gage3mm => 3.0 / 25.4,
                                uppSheetGages.Gage3pt5mm => 3.5 / 25.4,
                                uppSheetGages.Gage4mm => 4.0 / 25.4,
                                _ => _Thickness
                            };
                        }


                    case uppMaterialTypes.Hardware:
                        _rVal = 0;
                        break;

                    case uppMaterialTypes.Tubing:
                        _rVal = _Thickness;
                        break;

                    default:
                        break;
                }

                return _rVal;
            }
            set => _Thickness = value;
        }
        /// <summary>
        /// ^the name of the material
        ///~like "Carbon Steel"
        /// </summary>
        /// <param name="aMaterial"></param>
        /// <returns></returns>
        public string FamilyName
        {
            get

            {
                return Family switch
                {
                    uppMetalFamilies.Stainless_304L => "304L Stainless",
                    uppMetalFamilies.Stainless_316L => "316L Stainless",
                    uppMetalFamilies.Stainless_316 => "316 Stainless",
                    uppMetalFamilies.Stainless_304 => "304 Stainless",
                    uppMetalFamilies.Stainless_410 => "410 Stainless",
                    uppMetalFamilies.CarbonSteel => "Carbon Steel",
                    uppMetalFamilies.CarbonSteel_Killed => "Killed Carbon Steel",
                    _ => _FamilyName
                };


            }

            set
            {
                _FamilyName = value.Trim();
                Family = uppMetalFamilies.Unknown;

                string fnam = _FamilyName.ToUpper();
                switch (fnam)
                {
                    case "304L STAINLESS":
                        Family = uppMetalFamilies.Stainless_304L;
                        break;

                    case "304 STAINLESS":
                        Family = uppMetalFamilies.Stainless_304;
                        break;

                    case "316 STAINLESS":
                        Family = uppMetalFamilies.Stainless_316;
                        break;
                    case "316L STAINLESS":
                        Family = uppMetalFamilies.Stainless_316L;
                        break;
                    case "410 STAINLESS":
                        Family = uppMetalFamilies.Stainless_410;
                        break;
                    case "CARBON STAINLESS":
                        Family = uppMetalFamilies.CarbonSteel;
                        break;
                    case "KILLED CARBON STAINLESS":
                        Family = uppMetalFamilies.CarbonSteel_Killed;
                        break;
                }

            }

        }

        /// <summary>
        ///returns a descriptive string for the material
        ///~like "Carbon Steel 10 ga."
        /// </summary>
        /// <param name="aMaterial"></param>
        /// <param name="bSuppressGageName"></param>
        /// <returns></returns>
        public string FriendlyName(bool bSuppressGageName = false)
        {
            string _rVal = string.Empty;

            switch (Type)
            {
                case uppMaterialTypes.SheetMetal:
                    _rVal = FamilyName;
                    return (!bSuppressGageName) ? _rVal + " " + GageName : _rVal;
                case uppMaterialTypes.Hardware:
                    return MaterialName;
                case uppMaterialTypes.Tubing:
                    _rVal = "Tube";
                    if (IsStainless) _rVal = "SS " + _rVal;
                    return _rVal + " " + Schedule.GetDescription();
            }

            return _rVal;
        }

        /// <summary>
        /// the string used to set the family by String
        /// </summary>
        public string FamilySelectName => Family.GetDescription(); // uopEnums.Description(Family);


        /// <summary>
        ///^the name of the material
        ///~like "CS" or "304 SS" etc.
        /// </summary>
        public string ShortName
        {
            get
            {
                return Family switch
                {
                    uppMetalFamilies.Stainless_304L => "304L SS",
                    uppMetalFamilies.Stainless_316L => "316L SS",
                    uppMetalFamilies.Stainless_316 => "316 SS",
                    uppMetalFamilies.Stainless_304 => "304 SS",
                    uppMetalFamilies.Stainless_410 => "410 SS",
                    uppMetalFamilies.CarbonSteel => "CS",
                    uppMetalFamilies.CarbonSteel_Killed => "Killed CS",
                    _ => FamilyName
                };


            }
        }

        public string Descriptor
        {
            get
            {
                string _rVal = string.Empty;
                switch (Type)
                {
                    case uppMaterialTypes.Tubing:
                        _rVal = MaterialName;
                        mzUtils.ListAdd(ref _rVal, IsStainless.ToString(), bSuppressTest: true, aDelimitor: uopGlobals.Delim, bAllowNulls: true);
                        mzUtils.ListAdd(ref _rVal, Spec.ToString(), bSuppressTest: true, aDelimitor: uopGlobals.Delim, bAllowNulls: true);
                        mzUtils.ListAdd(ref _rVal, Schedule.ToString(), bSuppressTest: true, aDelimitor: uopGlobals.Delim, bAllowNulls: true);
                        mzUtils.ListAdd(ref _rVal, TubeSize.ToString(), bSuppressTest: true, aDelimitor: uopGlobals.Delim, bAllowNulls: true);
                        break;

                    case uppMaterialTypes.SheetMetal:
                        System.Text.StringBuilder smtDescriptor = new System.Text.StringBuilder(FamilyName);
                        smtDescriptor.Append(uopGlobals.Delim + GageName);
                        smtDescriptor.Append(uopGlobals.Delim + Thickness);
                        smtDescriptor.Append(uopGlobals.Delim + Density);
                        smtDescriptor.Append(uopGlobals.Delim + mzUtils.VarToInteger(IsMetric));
                        smtDescriptor.Append(uopGlobals.Delim + mzUtils.VarToInteger(IsStainless));
                        smtDescriptor.Append(uopGlobals.Delim + Spec);
                        _rVal = smtDescriptor.ToString();

                        break;

                    case uppMaterialTypes.Hardware:
                        _rVal = MaterialName;
                        mzUtils.ListAdd(ref _rVal, IsStainless.ToString(), bSuppressTest: true, aDelimitor: uopGlobals.Delim, bAllowNulls: true);
                        mzUtils.ListAdd(ref _rVal, (int)Family, bSuppressTest: true, aDelimitor: uopGlobals.Delim, bAllowNulls: true);
                        mzUtils.ListAdd(ref _rVal, Spec, bSuppressTest: true, aDelimitor: uopGlobals.Delim, bAllowNulls: false);

                        break;

                }

                if (Type == uppMaterialTypes.Tubing)
                {
                    _rVal = MaterialName;
                    mzUtils.ListAdd(ref _rVal, IsStainless.ToString(), bSuppressTest: true, aDelimitor: uopGlobals.Delim, bAllowNulls: true);
                    mzUtils.ListAdd(ref _rVal, Spec.ToString(), bSuppressTest: true, aDelimitor: uopGlobals.Delim, bAllowNulls: true);
                    mzUtils.ListAdd(ref _rVal, Schedule.ToString(), bSuppressTest: true, aDelimitor: uopGlobals.Delim, bAllowNulls: true);
                    mzUtils.ListAdd(ref _rVal, TubeSize.ToString(), bSuppressTest: true, aDelimitor: uopGlobals.Delim, bAllowNulls: true);

                }
                else if (Type == uppMaterialTypes.SheetMetal)
                {

                }
                return _rVal;

            }
        }

        public uppSpecTypes SpecType
        {
            get
            {
                return Type switch
                {
                    uppMaterialTypes.SheetMetal => uppSpecTypes.SheetMetal,
                    uppMaterialTypes.Hardware => uopHardware.GetSpecType(PartType),
                    uppMaterialTypes.Tubing => uppSpecTypes.Tube,
                    uppMaterialTypes.Gasket => uppSpecTypes.Gasket,
                    uppMaterialTypes.Plate => uppSpecTypes.Plate,
                    uppMaterialTypes.Pipe => uppSpecTypes.Pipe,
                    uppMaterialTypes.Fitting => uppSpecTypes.Fitting,
                    uppMaterialTypes.Flange => uppSpecTypes.Flange,
                    _ => uppSpecTypes.Undefined
                };



            }
        }


        public bool IsEqual(TMATERIAL bMtrl, bool bCompareSpec, out bool bSpecDiffers)
        {
            bool _rVal = Type == bMtrl.Type;
            bSpecDiffers = false;

            if (_rVal)
            {
                if (Type == uppMaterialTypes.SheetMetal)
                {

                    string rADescriptor = Descriptor;
                    string rBDescriptor = bMtrl.Descriptor;
                    _rVal = true;

                    if (!mzUtils.ListsStringCompare(rADescriptor, rBDescriptor, "3¸7", true, bTrim: true, out _, aDelimitor: uopGlobals.Delim))
                    {
                        _rVal = false;
                    }
                    bSpecDiffers = string.Compare(Spec, bMtrl.Spec, ignoreCase: true) != 0;


                    if (bCompareSpec && bSpecDiffers) bCompareSpec = false;

                }
                else if (Type == uppMaterialTypes.Hardware)
                {
                    _rVal = string.Compare(FamilySelectName, bMtrl.FamilySelectName, ignoreCase: true) == 0;
                }
            }


            return _rVal;
        }

        public bool IsEqual(TMATERIAL bMtrl, bool bCompareSpec = false)
        {
            return IsEqual(bMtrl, bCompareSpec, out bool aFlag);
        }

        #region Shared Methods
        /// <summary>
        /// the material gage name if the sheet gage property is undefined
        /// </summary>
        /// <param name="aSMT"></param>
        /// <param name="newval"></param>
        /// <returns></returns>
        public static TMATERIAL SheetMetalByGageName(TMATERIAL aBase, string aSheetGageName)
        {

            TMATERIAL _rVal = (aBase.Type == uppMaterialTypes.SheetMetal) ? aBase.Clone() : new TMATERIAL(uppMaterialTypes.SheetMetal);

            if (string.IsNullOrEmpty(aSheetGageName)) { return _rVal; }


            switch (aSheetGageName.ToUpper())
            {
                case "10 GA.":
                    _rVal.SheetGage = uppSheetGages.Gage10;
                    break;
                case "12 GA.":
                    _rVal.SheetGage = uppSheetGages.Gage12;
                    break;
                case "14 GA.":
                    _rVal.SheetGage = uppSheetGages.Gage14;
                    break;
                case "2 MM":
                    _rVal.SheetGage = uppSheetGages.Gage2mm;
                    break;
                case "2.5 MM":
                    _rVal.SheetGage = uppSheetGages.Gage2pt5mm;
                    break;
                case "2.7 MM":
                    _rVal.SheetGage = uppSheetGages.Gage2pt7mm;
                    break;
                case "3 MM":
                    _rVal.SheetGage = uppSheetGages.Gage3mm;
                    break;
                case "3.5 MM":
                    _rVal.SheetGage = uppSheetGages.Gage3pt5mm;
                    break;
                case "4 MM":
                    _rVal.SheetGage = uppSheetGages.Gage4mm;
                    break;
                default:
                    _rVal.SheetGage = uppSheetGages.GageUnknown;
                    _rVal.GageName = aSheetGageName;
                    break;
            }
            return _rVal;
        }

        public static TMATERIAL SheetMetalByFamilyName(TMATERIAL aBase, string aFamName)
        {
            TMATERIAL _rVal = (aBase.Type == uppMaterialTypes.SheetMetal) ? aBase.Clone() : new TMATERIAL(uppMaterialTypes.SheetMetal);

            if (string.IsNullOrEmpty(aFamName)) { return _rVal; }

            _rVal.Family = aFamName.ToUpper() switch
            {
                "304L STAINLESS" => uppMetalFamilies.Stainless_304L,
                "304 STAINLESS" => uppMetalFamilies.Stainless_304,
                "316 STAINLESS" => uppMetalFamilies.Stainless_316,
                "316L STAINLESS" => uppMetalFamilies.Stainless_316L,
                "410 STAINLESS" => uppMetalFamilies.Stainless_410,
                "CARBON STEEL" => uppMetalFamilies.CarbonSteel,
                "KILLED CARBON STEEL" => uppMetalFamilies.CarbonSteel_Killed,
                _ => uppMetalFamilies.Unknown
            };


            return _rVal;
        }


        public static uopMaterial FromStructure(TMATERIAL aMaterial)
        {
            return aMaterial.Type switch
            {
                uppMaterialTypes.SheetMetal => new uopSheetMetal(aMaterial),
                uppMaterialTypes.Hardware => new uopHardwareMaterial(aMaterial),
                uppMaterialTypes.Tubing => new uopTubeMaterial(aMaterial),
                _ => null
            };

        }


        public static TMATERIAL DefineByDescriptor(string sDescriptor)

        { return SheetMetalByDescriptor(sDescriptor, out bool INVLD); }

        /// <summary>
        ///#1the descriptor string to define the material with
        ///^shorthand method to define the properties of a sheel metal object using a string
        ///~strings like Carbon Steel¸10 ga.¸0.1345¸0.2089¸-1,-1.
        ///~string deliminator is ascii character 184.
        ///~bad strings will not raise an error they just won't set the properties.
        /// </summary>
        /// <param name="sDescriptor"></param>
        /// <returns></returns>
        public static TMATERIAL SheetMetalByDescriptor(string sDescriptor)
        {
            return SheetMetalByDescriptor(sDescriptor, out bool INVLD);
        }

        /// <summary>
        ///#1the descriptor string to define the material with
        ///^shorthand method to define the properties of a sheel metal object using a string
        ///~strings like Carbon Steel¸10 ga.¸0.1345¸0.2089¸-1,-1.
        ///~string deliminator is ascii character 184.
        ///~bad strings will not raise an error they just won't set the properties.
        /// </summary>
        /// <param name="sDescriptor"></param>
        /// <param name="bValid"></param>
        /// <returns></returns>
        public static TMATERIAL SheetMetalByDescriptor(string sDescriptor, out bool bValid)
        {
            TMATERIAL _rVal = new TMATERIAL(uppMaterialTypes.Undefined);

            // dstr = FamilyName
            //dstr = dstr & Delim$ & GageName
            //dstr = dstr & Delim$ & Thickness
            //dstr = dstr & Delim$ & Density
            //dstr = dstr & Delim$ & CInt(IsMetric)
            //dstr = dstr & Delim$ & CInt(IsStainless)
            //dstr = dstr & Delim$ & Spec
            bValid = false;

            int iVal = 0;

            int cnt = 0;

            List<string> sVals = mzUtils.ListValues(sDescriptor, uopGlobals.Delim.ToString(), true, true);
            cnt = sVals.Count;

            if (cnt < 6) return _rVal;


            _rVal.Type = uppMaterialTypes.SheetMetal;
            _rVal.FamilyName = sVals[0];
            _rVal.GageName = sVals[1];

            if (_rVal.Family == uppMetalFamilies.Unknown)
            {
                if (mzUtils.IsNumeric(sVals[3]))
                {
                    _rVal.Density = Convert.ToDouble(sVals[3]);
                }
                if (mzUtils.IsNumeric(sVals[5]))
                {
                    iVal = Convert.ToInt32(sVals[5]);
                    _rVal.IsStainless = iVal == -1;
                }
                else
                {
                    _rVal.IsStainless = false;

                    if (_rVal.FamilyName.IndexOf("stainless", StringComparison.OrdinalIgnoreCase) > -1)  //Changed condition based on o/p diffrence in vb and c#
                    {
                        _rVal.IsStainless = true;
                    }

                }
            }

            if (_rVal.SheetGage == uppSheetGages.GageUnknown)
            {
                if (mzUtils.IsNumeric(sVals[2]))
                {
                    _rVal.Thickness = Convert.ToDouble(sVals[2]);
                }
                if (mzUtils.IsNumeric(sVals[4]))
                {
                    iVal = Convert.ToInt32(sVals[4]);
                    _rVal.IsMetric = iVal == -1;

                }
                else
                {
                    _rVal.IsMetric = _rVal.GageName.IndexOf("MM", StringComparison.OrdinalIgnoreCase) > -1;  //Changed condition based on o/p diffrence in vb and c#

                }
            }
            if (cnt >= 7)
            {
                _rVal.Spec = sVals[6].Trim();
            }

            bValid = true;
            return _rVal;
        }


        /// <summary>
        /// #1the defining string
        ///#2the string delimination character
        ///^shorthand method to define the properties of a sheel metal object using a string
        ///~strings like "Carbon Steel;10;0.1345;0.23445,False,True,ASTM A-240".
        ///~bad strings will not raise an error they just won't set the properties
        /// </summary>
        /// <param name="aDefString"></param>
        /// <param name="aDelim"></param>
        /// <param name="bValid"></param>
        /// <returns></returns>
        public static TMATERIAL SheetMetalByString(string aDefString, out bool rValid, string aDelim = ";")
        {
            TMATERIAL _rVal = new TMATERIAL();
            rValid = true;
            try
            {
                string nm = string.Empty;
                string ga = string.Empty;
                string thk = string.Empty;
                string dns = string.Empty;
                bool bISMet = false;
                bool bIS_SS = false;
                double bGage = 0;
                List<string> sVals = new List<string> { };
                int cnt = 0;

                aDefString = aDefString.Trim();
                if (aDefString.Length < 10)
                {
                    return _rVal;

                }


                aDelim = String.IsNullOrWhiteSpace(aDelim) ? ";" : aDelim.Trim();

                sVals = mzUtils.ListValues(aDefString, aDelim, true, true);
                cnt = sVals.Count;

                if (cnt < 6)
                {
                    throw new Exception("invalid material sptring detected");
                }
                if (!mzUtils.IsNumeric(sVals[2]))
                {
                    throw new Exception("invalid material sptring detected");
                }
                if (!mzUtils.IsNumeric(sVals[3]))
                {
                    throw new Exception("invalid material sptring detected");
                }
                if (!mzUtils.IsNumeric(sVals[4]))
                {
                    throw new Exception("invalid material sptring detected");
                }
                if (!mzUtils.IsNumeric(sVals[5]))
                {
                    throw new Exception("invalid material sptring detected");
                }

                nm = sVals[0];
                ga = sVals[1];
                thk = sVals[2];
                dns = sVals[3];
                bISMet = Boolean.TryParse(sVals[4], out bool bISMetvalue);
                bIS_SS = Boolean.TryParse(sVals[5], out bool bIS_SSvalue);//CInt(sVals[5])

                if (ga.IndexOf("GA", StringComparison.OrdinalIgnoreCase) > -1)
                {
                    bISMet = false;
                }
                if (ga.IndexOf("MM", StringComparison.OrdinalIgnoreCase) > -1)
                {
                    bISMet = true;
                }
                ga = ga.Replace("GA.", "");
                ga = ga.Replace("GA", "");
                ga = ga.Replace("mm", "");
                ga = ga.Trim();

                if (bISMet && !mzUtils.IsNumeric(ga))
                {
                    throw new Exception("invalid material sptring detected");
                }

                if (Convert.ToDouble(thk) <= 0)
                {
                    throw new Exception("invalid material sptring detected");
                }
                if (Convert.ToDouble(dns) <= 0)
                {
                    throw new Exception("invalid material sptring detected");
                }
                _rVal.Type = uppMaterialTypes.SheetMetal;
                _rVal.Thickness = Convert.ToDouble(thk);
                _rVal.Density = Convert.ToDouble(dns);
                bGage = Math.Round(Convert.ToDouble(ga), 0);
                //Changing condition check considering o/p differnce of IndexOf and Instr methods
                if (nm.IndexOf("Carbon", StringComparison.OrdinalIgnoreCase) > -1)
                {
                    if (nm.IndexOf("Kill", StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        _rVal.Family = uppMetalFamilies.CarbonSteel_Killed;
                    }
                    else
                    {
                        _rVal.Family = uppMetalFamilies.CarbonSteel;
                    }
                }
                else if (nm.IndexOf("316L", StringComparison.OrdinalIgnoreCase) > -1 || nm.IndexOf("316 L", StringComparison.OrdinalIgnoreCase) > -1)
                {
                    _rVal.Family = uppMetalFamilies.Stainless_316L;
                }
                else if (nm.IndexOf("316", StringComparison.OrdinalIgnoreCase) > -1)
                {
                    _rVal.Family = uppMetalFamilies.Stainless_316;
                }
                else if (nm.IndexOf("304L", StringComparison.OrdinalIgnoreCase) > -1 || nm.IndexOf("304 L", StringComparison.OrdinalIgnoreCase) > -1)
                {
                    _rVal.Family = uppMetalFamilies.Stainless_304L;
                }
                else if (nm.IndexOf("304", StringComparison.OrdinalIgnoreCase) > -1)
                {
                    _rVal.Family = uppMetalFamilies.Stainless_304;
                }
                else if (nm.IndexOf("410", StringComparison.OrdinalIgnoreCase) > -1)
                {
                    _rVal.Family = uppMetalFamilies.Stainless_410;
                }
                else
                {
                    _rVal.Family = uppMetalFamilies.Unknown;
                    _rVal.FamilyName = nm;
                }

                if (!bISMet)
                {
                    if (bGage == 10)
                    {
                        _rVal.SheetGage = uppSheetGages.Gage10;
                    }
                    else if (bGage == 12)
                    {
                        _rVal.SheetGage = uppSheetGages.Gage12;
                    }
                    else if (bGage == 14)
                    {
                        _rVal.SheetGage = uppSheetGages.Gage14;
                    }
                    else
                    {
                        _rVal.SheetGage = uppSheetGages.GageUnknown;
                        _rVal.GageName = string.Format(bGage.ToString(), "#0") + " ga.";
                    }

                }
                else
                {
                    if (Math.Abs(_rVal.Thickness - (3 / 25.4)) < 0.01)
                    {
                        _rVal.SheetGage = uppSheetGages.Gage3mm;
                    }
                    else if (Math.Abs(_rVal.Thickness - (2.7 / 25.4)) < 0.01)
                    {
                        _rVal.SheetGage = uppSheetGages.Gage2pt7mm;
                    }
                    else if (Math.Abs(_rVal.Thickness - (2.5 / 25.4)) < 0.01)
                    {
                        _rVal.SheetGage = uppSheetGages.Gage2pt5mm;
                    }
                    else if (Math.Abs(_rVal.Thickness - (2 / 25.4)) < 0.01)
                    {
                        _rVal.SheetGage = uppSheetGages.Gage2mm;
                    }
                    else
                    {
                        _rVal.GageName = string.Format("{0:0.0##}", _rVal.Thickness) + " mm";
                        _rVal.SheetGage = uppSheetGages.GageUnknown;
                    }
                }

                _rVal.IsStainless = bIS_SS;

                if (cnt >= 7)
                {
                    _rVal.Spec = sVals[6].Trim();
                }

                rValid = false;
            }
            catch (Exception e)
            {
                return _rVal;
                throw e;
            }

            return _rVal;
        }
        public static string SheetMetalFamilyFilter(uppMetalFamilies aFamily)
        {
            string _rVal = string.Empty;
            switch (aFamily)
            {
                case uppMetalFamilies.CarbonSteel:
                    break;
                case uppMetalFamilies.Stainless_316:
                    _rVal = "316";
                    break;
                case uppMetalFamilies.Stainless_316L:
                    _rVal = "316L";
                    break;
                case uppMetalFamilies.Stainless_304L:
                    _rVal = "304L";
                    break;
                case uppMetalFamilies.Stainless_304:
                    _rVal = "304";
                    break;
                case uppMetalFamilies.Stainless_410:
                    _rVal = "410";
                    break;
                case uppMetalFamilies.CarbonSteel_Killed:
                    _rVal = "KILLED";
                    break;
                case uppMetalFamilies.Stainless_188:
                    _rVal = "18-8";
                    break;
            }
            return _rVal;
        }

        internal static TMATERIAL DefaultHardware()
        {
            TMATERIAL _rVal = new TMATERIAL(uppMaterialTypes.Hardware)
            {
                MaterialName = "Carbon Steel",
                Family = uppMetalFamilies.CarbonSteel,
                Index = 1,
                IsMetric = true
            };
            return _rVal;
        }

        #endregion

    }

    internal struct TMATERIALS : ICloneable
    {
        public uppMaterialTypes Type;
        private int _Count;
        private TMATERIAL[] _Members;
        private bool _Init;
        public string Name;
        public TMATERIALS(uppMaterialTypes aType = uppMaterialTypes.Undefined, string aName = "")
        {
            Type = aType;
            Name = aName;
            _Count = 0;
            _Init = true;
            _Members = new TMATERIAL[0];

        }
        private int Init() { _Init = true; _Members = new TMATERIAL[0]; return 0; }


        public void Clear() { _Init = true; _Members = new TMATERIAL[0]; }

        public int Count => (!_Init) ? Init() : _Count;

        public List<TMATERIAL> ToList { get => new List<TMATERIAL>(_Members); }

        object ICloneable.Clone() => (object)Clone(false);

        public TMATERIALS Clone(bool bReturnEmpty = false)
        {
            if (!_Init) { Init(); }
            TMATERIAL[] mems = bReturnEmpty ? new TMATERIAL[0] : Force.DeepCloner.DeepClonerExtensions.DeepClone<TMATERIAL[]>(_Members);  //(TMATERIAL[])_Members.Clone();
            int cnt = bReturnEmpty ? 0 : _Count;
            return new TMATERIALS(Type) { Name = Name, _Members = mems, _Count = cnt, _Init = true };

        }

        public TMATERIAL Item(int aIndex)
        {
            if (aIndex < 1 || aIndex > Count) throw new IndexOutOfRangeException();
            _Members[aIndex - 1].Index = aIndex;
            return _Members[aIndex - 1];
        }

        public void SetItem(int aIndex, TMATERIAL aMember)
        {
            if (aIndex < 1 || aIndex > Count) throw new IndexOutOfRangeException();
            aMember.Index = aIndex;
            _Members[aIndex - 1] = aMember;
        }


        /// <summary>
        /// The material to search for///returns the index of the material in the collection that matches the passed one.
        /// match is performed against the friendly name which is the material family name with the gage name
        /// </summary>
        /// <param name="aSMTS"></param>
        /// <param name="aSMT"></param>
        /// <returns></returns>
        public int SheetMetalExists(TMATERIAL aSMT)
        {
            TMATERIAL aMem;
            int _rVal = 0;
            string aStr = aSMT.FriendlyName();

            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                if (string.Compare(aMem.FriendlyName(), aStr, ignoreCase: true) == 0)
                {
                    _rVal = i;
                    break;
                }
            }

            return _rVal;
        }

        public TMATERIALS GetByFamily(uppMaterialTypes aType, uppMetalFamilies aSearchFamily)
        {
            TMATERIALS _rVal = Clone(true);
            TMATERIAL aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                if (aMem.Type == aType)
                {
                    if (aMem.Family == aSearchFamily) _rVal.Add(aMem);
                }
            }


            return _rVal;
        }


        public TMATERIAL GetByFamilyAndThickness(uppMaterialTypes aType, uppMetalFamilies aSearchFamily, double aThickness)
        {
            TMATERIALS aFam = GetByFamily(aType, aSearchFamily);
            return aFam.GetByNearestThickness(aType, aThickness);
        }

        /// <summary>
        /// #1the thickness to search for
        /// ^returns the material in the collection whose thickness most closely matches the passed value
        /// </summary>
        /// <param name="type"></param>
        /// <param name="searchThk"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public TMATERIAL GetByNearestThickness(uppMaterialTypes type, double searchThk)
        {
            double maxdif = 100000;
            double dif;
            int index = 0;
            TMATERIAL aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                if (aMem.Type == type)
                {
                    dif = Math.Abs(searchThk - aMem.Thickness);
                    if (dif < maxdif)
                    {
                        index = i + 1;
                        maxdif = dif;
                    }
                }
            }
            return (index > 0) ? Item(index) : new TMATERIAL(uppMaterialTypes.Undefined);
        }

        /// <summary>
        /// returns the metal from the collection whose family and gage matches the passed values
        /// #1the family name to search for
        /// #2the sheet gage to search for
        /// </summary>
        /// <param name="aFamily"></param>
        /// <param name="aSheetGage"></param>
        /// <param name="aIndex"></param>
        /// <returns></returns>
        public TMATERIAL GetSheetMetalByStringVals(string aFamily, string aSheetGage)
        {

            TMATERIAL aMem;

            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                if (aMem.Type == uppMaterialTypes.SheetMetal)
                {
                    if (string.Compare(aMem.FamilyName, aFamily, ignoreCase: true) == 0)
                    {
                        if (string.Compare(aMem.GageName, aSheetGage, ignoreCase: true) == 0) return aMem;

                    }
                }

            }
            return new TMATERIAL(uppMaterialTypes.SheetMetal);
        }

        /// <summary>
        ///#1the family name to search for
        ///#2the sheet gage to search for
        ///#3the descriptor of the material
        ///#4the family to use if the material can't be found
        ///#5the gage to use if the material can't be found

        ///^returns the metal from the collection whose family and gage matches the passed values
        ///~if the material can't be found then the descriptor is used to create it and addit to the collection.
        ///~if the material can't be defined with the descriptor the material with the default family and gage is returned.
        ///~"Nothing" is never returned. 
        /// </summary>
        /// <param name="aFamily"></param>
        /// <param name="aSheetGage"></param>
        /// <param name="aDescriptor"></param>
        /// <param name="aDefaultFamily"></param>
        /// <param name="aDefaultGage"></param>
        /// <param name="aIndex"></param>
        /// <returns></returns>
        public TMATERIAL Retrieve(string aFamily, string aSheetGage, string aDescriptor = "", uppMetalFamilies aDefaultFamily = uppMetalFamilies.CarbonSteel, uppSheetGages aDefaultGage = uppSheetGages.Gage12)
        {
            TMATERIAL _rVal = new TMATERIAL(uppMaterialTypes.Undefined);

            int aIndex = 0;
            TMATERIAL aMem;
            int cnt = 0;

            aDescriptor = aDescriptor.Trim();

            //look fo the material by family and gage
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                if (string.Compare(aMem.FamilyName, aFamily) == 0)
                {
                    if (string.Compare(aMem.GageName, aSheetGage) == 0)
                    {
                        _rVal = aMem;
                        aIndex = i;

                        break;
                    }
                }
            }

            //if its not found create and add it based on the descriptor string
            if (aIndex <= 0 && aDescriptor !=  string.Empty)
            {
                cnt = Count;
                TMATERIAL mtrl = TMATERIAL.SheetMetalByDescriptor(aDescriptor, out bool bOK);
                if (bOK)
                {
                    mtrl = Add(mtrl);
                    if (mtrl.Index > 0)
                    {
                        aIndex = Count;
                        _rVal = Item(aIndex);
                    }
                }



            }

            //if its not found return the suggested default
            if (aIndex <= 0)
            {
                if (aDefaultFamily == uppMetalFamilies.Unknown)
                { aDefaultFamily = uppMetalFamilies.CarbonSteel; }
                if (aDefaultGage == uppSheetGages.GageUnknown) { aDefaultGage = uppSheetGages.Gage12; }

                for (int i = 1; i <= Count; i++)
                {
                    aMem = Item(i);
                    if (aMem.Family == aDefaultFamily && aMem.SheetGage == aDefaultGage)
                    {
                        aIndex = i;
                        _rVal = aMem;
                        break;
                    }
                }

            }

            //if its not found return the collection default
            if (aIndex <= 0)
            {
                if (Count > 1)
                {
                    _rVal = Item(2);
                }
                else
                {
                    _rVal = Item(1);
                }
            }

            _rVal.Index = aIndex;
            return _rVal;
        }

        public TMATERIAL GetByDescriptor(string aDescriptor)
        {

            aDescriptor = string.IsNullOrWhiteSpace(aDescriptor) ? string.Empty : aDescriptor.Trim();
            if (aDescriptor ==  string.Empty) return new TMATERIAL(uppMaterialTypes.Undefined);

            TMATERIAL aMem;
            TMATERIAL bMem = TMATERIAL.SheetMetalByDescriptor(aDescriptor, out bool aFlg);
            if (!aFlg) return new TMATERIAL(uppMaterialTypes.Undefined);

            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                if (aMem.IsEqual(bMem))
                {
                    return aMem;
                }

            }
            return new TMATERIAL(uppMaterialTypes.Undefined); ;
        }

        public TMATERIAL Add(TMATERIAL newMem)
        {
            newMem.Index = 0;
            if (Count + 1 > Int32.MaxValue) { return newMem; }
            if (newMem.Type == uppMaterialTypes.Undefined) { return newMem; }
            if (newMem.Type == uppMaterialTypes.SheetMetal)
            {
                if (SheetMetalExists(newMem) > 0) { return new TMATERIAL(uppMaterialTypes.Undefined); }
            }

            _Count += 1;
            Array.Resize<TMATERIAL>(ref _Members, _Count);
            newMem.Index = _Count;
            _Members[_Count - 1] = newMem;
            return Item(Count);

        }


        public bool AddSheetMetal(uppMetalFamilies aFamily, uppSheetGages aSheetGage, bool bSuppressTest = false, bool bMakeDefault = false, string aSpecFilter = "")
        {
            bool _rVal = false;
            TMATERIAL aMem = new TMATERIAL(uppMaterialTypes.SheetMetal)
            {
                Type = uppMaterialTypes.SheetMetal
            };
            if (string.IsNullOrWhiteSpace(aSpecFilter))
            {
                aSpecFilter = TMATERIAL.SheetMetalFamilyFilter(aFamily);
            }
            aMem.Family = aFamily;
            aMem.SheetGage = aSheetGage;
            aMem.IsDefault = bMakeDefault;

            if (!bSuppressTest)
            {

                _rVal = Add(aMem).Index > 0;
            }
            else
            {
                if (SheetMetalExists(aMem) <= 0)
                {
                    _rVal = true;
                    Add(aMem);
                }
            }

            return _rVal;
        }

        /// <summary>
        /// shorthand method for adding a material to the collection
        /// </summary>
        /// <param name="aMaterials"></param>
        /// <param name="MatName"></param>
        /// <param name="IsStainless"></param>
        /// <param name="Spec"></param>
        /// <param name="Family"></param>
        public void AddHarwareMaterial(string MatName, bool IsStainless, string Spec = "", uppMetalFamilies Family = uppMetalFamilies.Unknown, bool bMetric = true)
        {

            TMATERIAL aMem = TMATERIAL.DefaultHardware();
            aMem.MaterialName = MatName;
            aMem.IsStainless = IsStainless;
            aMem.Spec = Spec;
            aMem.Family = Family;
            aMem.IsMetric = bMetric;
            Add(aMem);

        }


        /// <summary>
        ///#1a family to filter the return list by
        ///^returns the gages names of the material in the collection
        ///~like "10 ga","3mm" etc.
        /// </summary>
        /// <param name="aSMTS"></param>
        /// <param name="aFamily"></param>
        /// <returns></returns>
        public List<string> GageNames(uppMetalFamilies aFamily = uppMetalFamilies.Unknown)
        {
            List<string> _rVal = new List<string>();
            string aStr = string.Empty;
            string bStr = string.Empty;
            TMATERIAL aMem = new TMATERIAL();
            int i = 0;
            for (i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                bStr = aMem.GageName;
                if (aFamily == uppMetalFamilies.Unknown || (aFamily != uppMetalFamilies.Unknown && aMem.Family == aFamily))
                {
                    if (aStr.IndexOf(bStr, StringComparison.OrdinalIgnoreCase) == -1)
                    {
                        aStr = aStr + "~" + bStr + "~";
                        _rVal.Add(bStr);
                    }
                }
            }
            return _rVal;
        }

        /// <summary>
        ///returns the metal from the collection whose family and gage matches the pased values
        /// #1the family name to search for
        /// #2the sheet gage to search for
        /// </summary>
        /// <param name="aFamily"></param>
        /// <param name="aSheetGage"></param>
        /// <param name="aType"></param>
        /// <returns></returns>
        public TMATERIAL GetByFamilyAndGauge(uppMetalFamilies aFamily, uppSheetGages aSheetGage, uppMaterialTypes aType = uppMaterialTypes.Undefined)
        {
            TMATERIAL aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                if (aType == uppMaterialTypes.Undefined || aType == aMem.Type)
                {
                    if (aMem.Family == aFamily && aMem.SheetGage == aSheetGage) return aMem;
                }
            }
            return new TMATERIAL(uppMaterialTypes.Undefined);
        }

        public TMATERIAL GetByMaterialName(uppMaterialTypes aType, string aSearchName)
        {

            TMATERIAL aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                if (aMem.Type == aType)
                {
                    if (string.Compare(aMem.MaterialName, aSearchName, true) == 0)
                    {

                        aMem.Index = i;
                        return aMem;
                    }
                }
            }
            return new TMATERIAL(uppMaterialTypes.Undefined);
        }

        public TMATERIAL GetByFamilyName(uppMaterialTypes aType, string aSearchName)
        {
            int rIndex = 0;
            TMATERIAL aMem;

            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                if (aMem.Type == aType)
                {
                    if (string.Compare(aMem.FamilyName, aSearchName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        rIndex = i;
                        return aMem;
                    }
                }
            }
            return new TMATERIAL(uppMaterialTypes.Undefined);
        }

        public TMATERIAL GetByFriendlyName(uppMaterialTypes aType, string aSearchName, bool bSuppressGageName = false)
        {
            int rIndex = 0;
            TMATERIAL aMem;

            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                if (aMem.Type == aType)
                {
                    if (string.Compare(aMem.FriendlyName(bSuppressGageName: bSuppressGageName), aSearchName, ignoreCase: true) == 0)
                    {
                        rIndex = i;
                        return aMem;
                    }
                }
            }
            return new TMATERIAL(uppMaterialTypes.Undefined);
        }

        /// <summary>
        /// Returns the metal from the collection whose friendly name matches the passed values
        /// </summary>
        /// <param name="aSelectName"></param>
        /// <param name="aType"></param>
        /// <returns></returns>
        public TMATERIAL GetBySelectName(string aSelectName, uppMaterialTypes aType = uppMaterialTypes.Undefined)
        {
            TMATERIAL aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                if (aMem.Type == aType || aType == uppMaterialTypes.Undefined)
                {
                    if (string.Compare(aSelectName, aMem.FamilySelectName, ignoreCase: true) == 0) return aMem;
                }
            }

            return new TMATERIAL(uppMaterialTypes.Undefined);
        }


        #region Shared Methods
        public static TMATERIALS DefautSheetMetals()
        {
            TMATERIALS _rVal = new TMATERIALS(uppMaterialTypes.SheetMetal, "Default Sheet Metals");



            _rVal.AddSheetMetal(uppMetalFamilies.CarbonSteel, uppSheetGages.Gage10, true, true);
            _rVal.AddSheetMetal(uppMetalFamilies.CarbonSteel, uppSheetGages.Gage12, true, true);
            _rVal.AddSheetMetal(uppMetalFamilies.CarbonSteel, uppSheetGages.Gage14, true, true);
            _rVal.AddSheetMetal(uppMetalFamilies.CarbonSteel, uppSheetGages.Gage2mm, true, true);
            _rVal.AddSheetMetal(uppMetalFamilies.CarbonSteel, uppSheetGages.Gage2pt5mm, true, true);
            _rVal.AddSheetMetal(uppMetalFamilies.CarbonSteel, uppSheetGages.Gage2pt7mm, true, true);
            _rVal.AddSheetMetal(uppMetalFamilies.CarbonSteel, uppSheetGages.Gage3mm, true, true);
            _rVal.AddSheetMetal(uppMetalFamilies.CarbonSteel, uppSheetGages.Gage3pt5mm, true, true);
            _rVal.AddSheetMetal(uppMetalFamilies.CarbonSteel, uppSheetGages.Gage4mm, true, true);

            _rVal.AddSheetMetal(uppMetalFamilies.Stainless_304, uppSheetGages.Gage10, true, true, "304");
            _rVal.AddSheetMetal(uppMetalFamilies.Stainless_304, uppSheetGages.Gage12, true, true, "304");
            _rVal.AddSheetMetal(uppMetalFamilies.Stainless_304, uppSheetGages.Gage14, true, true, "304");
            _rVal.AddSheetMetal(uppMetalFamilies.Stainless_304, uppSheetGages.Gage2mm, true, true, "304");
            _rVal.AddSheetMetal(uppMetalFamilies.Stainless_304, uppSheetGages.Gage2pt5mm, true, true, "304");
            _rVal.AddSheetMetal(uppMetalFamilies.Stainless_304, uppSheetGages.Gage2pt7mm, true, true, "304");
            _rVal.AddSheetMetal(uppMetalFamilies.Stainless_304, uppSheetGages.Gage3mm, true, true, "304");
            _rVal.AddSheetMetal(uppMetalFamilies.Stainless_304, uppSheetGages.Gage3pt5mm, true, true, "304");
            _rVal.AddSheetMetal(uppMetalFamilies.Stainless_304, uppSheetGages.Gage4mm, true, true, "304");

            _rVal.AddSheetMetal(uppMetalFamilies.Stainless_304L, uppSheetGages.Gage10, true, true, "304");
            _rVal.AddSheetMetal(uppMetalFamilies.Stainless_304L, uppSheetGages.Gage12, true, true, "304");
            _rVal.AddSheetMetal(uppMetalFamilies.Stainless_304L, uppSheetGages.Gage14, true, true, "304");
            _rVal.AddSheetMetal(uppMetalFamilies.Stainless_304L, uppSheetGages.Gage2mm, true, true, "304");
            _rVal.AddSheetMetal(uppMetalFamilies.Stainless_304L, uppSheetGages.Gage2pt5mm, true, true, "304");
            _rVal.AddSheetMetal(uppMetalFamilies.Stainless_304L, uppSheetGages.Gage2pt7mm, true, true, "304");
            _rVal.AddSheetMetal(uppMetalFamilies.Stainless_304L, uppSheetGages.Gage3mm, true, true, "304");
            _rVal.AddSheetMetal(uppMetalFamilies.Stainless_304L, uppSheetGages.Gage3mm, true, true, "304");
            _rVal.AddSheetMetal(uppMetalFamilies.Stainless_304L, uppSheetGages.Gage4mm, true, true, "304");

            _rVal.AddSheetMetal(uppMetalFamilies.Stainless_316, uppSheetGages.Gage10, true, true, "316");
            _rVal.AddSheetMetal(uppMetalFamilies.Stainless_316, uppSheetGages.Gage12, true, true, "316");
            _rVal.AddSheetMetal(uppMetalFamilies.Stainless_316, uppSheetGages.Gage14, true, true, "316");
            _rVal.AddSheetMetal(uppMetalFamilies.Stainless_316, uppSheetGages.Gage2mm, true, true, "316");
            _rVal.AddSheetMetal(uppMetalFamilies.Stainless_316, uppSheetGages.Gage2pt5mm, true, true, "316");
            _rVal.AddSheetMetal(uppMetalFamilies.Stainless_316, uppSheetGages.Gage2pt7mm, true, true, "316");
            _rVal.AddSheetMetal(uppMetalFamilies.Stainless_316, uppSheetGages.Gage3mm, true, true, "316");
            _rVal.AddSheetMetal(uppMetalFamilies.Stainless_316, uppSheetGages.Gage3pt5mm, true, true, "316");
            _rVal.AddSheetMetal(uppMetalFamilies.Stainless_316, uppSheetGages.Gage4mm, true, true, "316");

            _rVal.AddSheetMetal(uppMetalFamilies.Stainless_316L, uppSheetGages.Gage10, true, true, "316");
            _rVal.AddSheetMetal(uppMetalFamilies.Stainless_316L, uppSheetGages.Gage12, true, true, "316");
            _rVal.AddSheetMetal(uppMetalFamilies.Stainless_316L, uppSheetGages.Gage14, true, true, "316");
            _rVal.AddSheetMetal(uppMetalFamilies.Stainless_316L, uppSheetGages.Gage2mm, true, true, "316");
            _rVal.AddSheetMetal(uppMetalFamilies.Stainless_316L, uppSheetGages.Gage2pt5mm, true, true, "316");
            _rVal.AddSheetMetal(uppMetalFamilies.Stainless_316L, uppSheetGages.Gage2pt7mm, true, true, "316");
            _rVal.AddSheetMetal(uppMetalFamilies.Stainless_316L, uppSheetGages.Gage3mm, true, true, "316");
            _rVal.AddSheetMetal(uppMetalFamilies.Stainless_316L, uppSheetGages.Gage3pt5mm, true, true, "316");
            _rVal.AddSheetMetal(uppMetalFamilies.Stainless_316L, uppSheetGages.Gage4mm, true, true, "316");

            _rVal.AddSheetMetal(uppMetalFamilies.Stainless_410, uppSheetGages.Gage10, true, true, "410");
            _rVal.AddSheetMetal(uppMetalFamilies.Stainless_410, uppSheetGages.Gage12, true, true, "410");
            _rVal.AddSheetMetal(uppMetalFamilies.Stainless_410, uppSheetGages.Gage14, true, true, "410");
            _rVal.AddSheetMetal(uppMetalFamilies.Stainless_410, uppSheetGages.Gage2mm, true, true, "410");
            _rVal.AddSheetMetal(uppMetalFamilies.Stainless_410, uppSheetGages.Gage2pt5mm, true, true, "410");
            _rVal.AddSheetMetal(uppMetalFamilies.Stainless_410, uppSheetGages.Gage2pt7mm, true, true, "410");
            _rVal.AddSheetMetal(uppMetalFamilies.Stainless_410, uppSheetGages.Gage3mm, true, true, "410");
            _rVal.AddSheetMetal(uppMetalFamilies.Stainless_410, uppSheetGages.Gage3pt5mm, true, true, "410");
            _rVal.AddSheetMetal(uppMetalFamilies.Stainless_410, uppSheetGages.Gage4mm, true, true, "410");

            _rVal.AddSheetMetal(uppMetalFamilies.CarbonSteel_Killed, uppSheetGages.Gage10, true, true, "KILLED");
            _rVal.AddSheetMetal(uppMetalFamilies.CarbonSteel_Killed, uppSheetGages.Gage12, true, true, "KILLED");
            _rVal.AddSheetMetal(uppMetalFamilies.CarbonSteel_Killed, uppSheetGages.Gage14, true, true, "KILLED");
            _rVal.AddSheetMetal(uppMetalFamilies.CarbonSteel_Killed, uppSheetGages.Gage2mm, true, true, "KILLED");
            _rVal.AddSheetMetal(uppMetalFamilies.CarbonSteel_Killed, uppSheetGages.Gage2pt5mm, true, true, "KILLED");
            _rVal.AddSheetMetal(uppMetalFamilies.CarbonSteel_Killed, uppSheetGages.Gage2pt7mm, true, true, "KILLED");
            _rVal.AddSheetMetal(uppMetalFamilies.CarbonSteel_Killed, uppSheetGages.Gage3mm, true, true, "KILLED");
            _rVal.AddSheetMetal(uppMetalFamilies.CarbonSteel_Killed, uppSheetGages.Gage3pt5mm, true, true, "KILLED");
            _rVal.AddSheetMetal(uppMetalFamilies.CarbonSteel_Killed, uppSheetGages.Gage4mm, true, true, "KILLED");

            return _rVal;
        }

        /// <summary>
        /// adds the default collection of Hardware Materials on initialization
        /// </summary>
        /// <returns></returns>
        internal static TMATERIALS DefaultHardware()
        {
            TMATERIALS _rVal = new TMATERIALS(uppMaterialTypes.Hardware);
            _rVal.AddHarwareMaterial("Carbon Steel", IsStainless: false, Family: uppMetalFamilies.CarbonSteel);
            _rVal.AddHarwareMaterial("410 Stainless Steel", IsStainless: true, Family: uppMetalFamilies.Stainless_410);
            _rVal.AddHarwareMaterial("304 Stainless Steel", IsStainless: true, Family: uppMetalFamilies.Stainless_304);
            _rVal.AddHarwareMaterial("304L Stainless Steel", IsStainless: true, Family: uppMetalFamilies.Stainless_304L);
            _rVal.AddHarwareMaterial("316 Stainless Steel", IsStainless: true, Family: uppMetalFamilies.Stainless_316);
            _rVal.AddHarwareMaterial("316L Stainless Steel", IsStainless: true, Family: uppMetalFamilies.Stainless_316L);
            _rVal.AddHarwareMaterial("18-8 Stainless Steel", IsStainless: true, Family: uppMetalFamilies.Stainless_188);
            return _rVal;
        }

        #endregion

    }

    internal struct TANGLEIRON : ICloneable
    {
        public double Rotation;
        public double Height;
        public double Width;
        public double Length;
        public dxxRadialDirections Direction;
        public double HoleSpan;
        public double HoleInset;
        public double HoleDiameter;
        public UVECTOR Center;
        public double Z;
        public bool Chamfered;
        public int BoltCount;
        public string Tag;
        public bool Flag1;
        public bool Flag2;
        public double JoggleDepth;


        public TANGLEIRON(double aHeight = 0, double aWidth = 0, double aLength = 0)
        {
            Center = UVECTOR.Zero;
            Rotation = 0;
            Height = aHeight;
            Width = aWidth;
            Length = aLength;
            Direction = dxxRadialDirections.AwayFromCenter;
            HoleSpan = 0;
            HoleInset = 0;
            HoleDiameter = 0;
            Z = 0;
            Chamfered = false;
            BoltCount = 0;
            Tag = string.Empty;
            Flag1 = false;
            Flag2 = false;
            JoggleDepth = 0;

        }

        /// <summary>
        ///returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>
        public object Clone() => (object)Clone();

        public TANGLEIRON Clone(string aTag = null)
        {
            string tg = string.IsNullOrEmpty(aTag) ? Tag : aTag;
            return new TANGLEIRON
            {
                Center = Center.Clone(),
                Rotation = Rotation,
                Height = Height,
                Width = Width,
                Length = Length,
                Direction = Direction,
                HoleSpan = HoleSpan,
                HoleInset = HoleInset,
                HoleDiameter = HoleDiameter,
                Z = Z,
                Chamfered = Chamfered,
                BoltCount = BoltCount,
                Tag = tg,
                Flag1 = Flag1,
                Flag2 = Flag2,
                JoggleDepth = JoggleDepth
            };
        }


    }

    internal struct TSPECSET : ICloneable
    {
        private TMATERIALSPEC[] _Specs;
        public string Name;
        private bool _Init;
        public TSPECSET(string aName = "") { Name = aName; _Specs = new TMATERIALSPEC[3]; _Init = true; }

        private int Init() { _Specs = new TMATERIALSPEC[3]; _Init = true; return 3; }

        public int Count => (!_Init) ? Init() : 3;

        object ICloneable.Clone() => (object)Clone();

        public TSPECSET Clone() { if (!_Init) { Init(); } return new TSPECSET(Name) { _Specs = (TMATERIALSPEC[])_Specs.Clone() }; }

        public TMATERIALSPEC Item(int aIndex)
        {
            if (aIndex < 1 || aIndex > Count) { return new TMATERIALSPEC(uppSpecTypes.Undefined); }
            return _Specs[aIndex - 1];
        }

        public TMATERIALSPEC SetItem(int aIndex, TMATERIALSPEC aSpec)
        {
            if (aIndex < 1 || aIndex > Count) { return new TMATERIALSPEC(uppSpecTypes.Undefined); }
            _Specs[aIndex - 1] = aSpec;
            return _Specs[aIndex - 1];
        }
    }

    internal struct TPROJECTSPECS : ICloneable
    {
        public string ProjectHandle;
        private TSPECSET[] _SpecSets;
        private bool _Init;
        public TPROJECTSPECS(string aProjectHandle = "") { ProjectHandle = aProjectHandle; _SpecSets = new TSPECSET[12]; _Init = true; }


        private int Init() { _SpecSets = new TSPECSET[12]; _Init = true; return 12; }


        public int Count => (!_Init) ? Init() : 12;

        object ICloneable.Clone() => (object)Clone();

        public TPROJECTSPECS Clone() { if (!_Init) { Init(); } return new TPROJECTSPECS(ProjectHandle) { _SpecSets = (TSPECSET[])_SpecSets.Clone() }; }

        public TSPECSET Item(int aIndex)
        {
            if (aIndex < 1 || aIndex > Count) { return new TSPECSET(); }
            return _SpecSets[aIndex - 1];
        }

        public TSPECSET SetItem(int aIndex, TSPECSET aSpecs)
        {
            if (aIndex < 1 || aIndex > Count) { return new TSPECSET(); }
            _SpecSets[aIndex - 1] = aSpecs;
            return _SpecSets[aIndex - 1];
        }

    }

    internal struct TPROJECT : ICloneable
    {
        public TPROPERTIES Properties;
        public TPROPERTIES DrawingNumbers;
        public uppProjectTypes Type;
        public uppProjectFamilies Family;
        public string Handle;


        public double DLLVersion;
        public string AppVersion;
        public string AppName;

        #region Constructors

        public TPROJECT(uppProjectTypes aType = uppProjectTypes.Undefined, string aHandle = "")
        {
            Handle = aHandle;
            Properties = new TPROPERTIES
            {
                PartType = uppPartTypes.Project
            };
            DrawingNumbers = new TPROPERTIES("DRAWING NUMBERS");
            Type = aType;
            Family = uppProjectFamilies.Undefined;

            DLLVersion = 0;
            AppVersion = string.Empty;
            AppName = string.Empty;
        }

        public TPROJECT(TPROJECT aProject)
        {
            Handle = aProject.Handle;
            Properties = aProject.Properties; //.Clone();
            DrawingNumbers = aProject.DrawingNumbers;// .Clone();
            Type = aProject.Type;
            Family = aProject.Family;

            DLLVersion = aProject.DLLVersion;
            AppVersion = aProject.AppVersion;
            AppName = aProject.AppName;
        }

        #endregion

        object ICloneable.Clone() => (object)Clone();

        public TPROJECT Clone() => new TPROJECT(this);

        public string Name => Properties.ValueS("KeyNumber") + "-" + Properties.ValueS("Revision");

        public string OutputFolder
        {
            get
            {
                string dfile = Properties.ValueS("DataFilename");
                if (string.IsNullOrWhiteSpace(dfile)) dfile = Properties.ValueS("ImportFileName");

                return string.IsNullOrWhiteSpace(dfile) ? string.Empty : Path.GetDirectoryName(dfile);
            }
        }
    }

    internal struct TCOLUMN : ICloneable
    {
        public TPROPERTIES Properties;

        #region Constructors
        public TCOLUMN(string dummy = "")
        {
            Properties = new TPROPERTIES("COLUMN")
            {
                PartType = uppPartTypes.Column
            };
        }

        public TCOLUMN(TPROPERTIES aProperties)
        {
            Properties = aProperties;
        }

        #endregion region

        /// <summary>
        ///returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>
        /// 
        object ICloneable.Clone() => (object)Clone();

        public TCOLUMN Clone() => new TCOLUMN(Properties);

    }

    internal struct TTRAYRANGE : ICloneable
    {
        private string _GUID;
        public TPROPERTIES Properties;
        public TMATERIAL HardwareMaterial;

        #region Constructors

        public TTRAYRANGE(string aGUID = "")
        {
            _GUID = aGUID;
            Properties = new TPROPERTIES("")
            {
                PartType = uppPartTypes.TrayRange
            };
            HardwareMaterial = new TMATERIAL(uppMaterialTypes.Hardware);
        }

        public TTRAYRANGE(TTRAYRANGE aPartToCopy)
        {
            _GUID = string.Empty;
            Properties = aPartToCopy.Properties; //.Clone();
            HardwareMaterial = aPartToCopy.HardwareMaterial; // .Clone();
        }
        #endregion

        /// <summary>
        ///returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>
        object ICloneable.Clone() => (object)Clone();

        public TTRAYRANGE Clone()
        {
            return new TTRAYRANGE(this);

        }

        public string GUID
        {
            get => _GUID;
            set
            {
                if (value == null) return;


                //if(!string.IsNullOrWhiteSpace(_GUID) && string.IsNullOrWhiteSpace(value))
                //{
                //    if(!Reading) return;
                //}
                _GUID = value.Trim();

            }
        }
    }

    internal struct TPARTINDICES : ICloneable
    {
        public int ParentIndex;
        public int PartIndex;
        public int ColumnIndex;
        public int RangeIndex;
        public int DowncomerIndex;
        public int PanelIndex;
        public int PanIndex;
        public int DistributorIndex;
        public int ChimneyTrayIndex;
        public int SectionIndex;
        public int PartNumberIndex;
        public int GroupIndex;

        public TPARTINDICES(int aParentIndex = 0, int aPartIndex = 0, int aColIndex = 0, int aRangeIndex = 0, int aDCIndex = 0, int aPanelIndex = 0, int aPanIndex = 0, int aDistribIndex = 0, int aCTIndex = 0, int aSecIndex = 0, int aPNIndex = 0, int aGroupIndex = 0)
        {
            ParentIndex = aParentIndex;
            PartIndex = aPartIndex;
            ColumnIndex = aColIndex;
            RangeIndex = aRangeIndex;
            DowncomerIndex = aDCIndex;
            PanelIndex = aPanelIndex;
            PanIndex = aPanIndex;
            DistributorIndex = aDistribIndex;
            ChimneyTrayIndex = aCTIndex;
            SectionIndex = aSecIndex;
            PartNumberIndex = aPNIndex;
            GroupIndex = aGroupIndex;
        }

        public TPARTINDICES(TPARTINDICES aIndices)
        {
            ParentIndex = aIndices.ParentIndex;
            PartIndex = aIndices.PartIndex;
            ColumnIndex = aIndices.ColumnIndex;
            RangeIndex = aIndices.RangeIndex;
            DowncomerIndex = aIndices.DowncomerIndex;
            PanelIndex = aIndices.PanelIndex;
            PanIndex = aIndices.PanIndex;
            DistributorIndex = aIndices.DistributorIndex;
            ChimneyTrayIndex = aIndices.ChimneyTrayIndex;
            SectionIndex = aIndices.SectionIndex;
            PartNumberIndex = aIndices.PartNumberIndex;
            GroupIndex = aIndices.GroupIndex;
        }

        /// <summary>
        ///returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>
        object ICloneable.Clone() => (object)Clone(true);

        public TPARTINDICES Clone(bool bCloneIndex = true) => new TPARTINDICES(this) { PartIndex = bCloneIndex ? PartIndex : 0 };


        public void Copy(TPARTINDICES aIndices)
        {
            ParentIndex = aIndices.ParentIndex;
            PartIndex = aIndices.PartIndex;
            ColumnIndex = aIndices.ColumnIndex;
            RangeIndex = aIndices.RangeIndex;
            DowncomerIndex = aIndices.DowncomerIndex;
            PanelIndex = aIndices.PanelIndex;
            PanIndex = aIndices.PanIndex;
            DistributorIndex = aIndices.DistributorIndex;
            ChimneyTrayIndex = aIndices.ChimneyTrayIndex;
            SectionIndex = aIndices.SectionIndex;
            PartNumberIndex = aIndices.PartNumberIndex;
            GroupIndex = aIndices.GroupIndex;
        }

        public int GetIndex(uppPartTypes aPartType)
        {
            var idx = aPartType switch
            {
                uppPartTypes.Column => ColumnIndex,
                uppPartTypes.ChimneyTray => ChimneyTrayIndex,
                uppPartTypes.Distributor => DistributorIndex,
                uppPartTypes.TrayRange => RangeIndex,
                uppPartTypes.Downcomer => DowncomerIndex,
                uppPartTypes.DeckPanel => PanelIndex,
                uppPartTypes.DeckSection => SectionIndex,
                uppPartTypes.ReceivingPan => PanIndex,
                _ => PartIndex

            };
            return idx;

        }

        public bool SetIndex(uppPartTypes aPartType, int aIndex)
        {
            bool _rVal = PartIndex != aIndex; ;
            if (aPartType == uppPartTypes.ChimneyTray) { if (ChimneyTrayIndex != aIndex) _rVal = true; ChimneyTrayIndex = aIndex; }
            if (aPartType == uppPartTypes.DeckPanel)
            {
                if (PanelIndex != aIndex) _rVal = true; PanelIndex = aIndex;
            }
            if (aPartType == uppPartTypes.Column)
            {
                if (ColumnIndex != aIndex) _rVal = true; ColumnIndex = aIndex;
            }
            if (aPartType == uppPartTypes.Downcomer)
            {
                if (DowncomerIndex != aIndex) _rVal = true; DowncomerIndex = aIndex;
            }
            if (aPartType == uppPartTypes.TrayRange)
            {
                if (RangeIndex != aIndex) _rVal = true; RangeIndex = aIndex;
            }
            if (aPartType == uppPartTypes.ReceivingPan)
            {
                if (PanIndex != aIndex) _rVal = true; PanIndex = aIndex;
            }
            if (aPartType == uppPartTypes.DeckSection)
            {
                if (SectionIndex != aIndex) _rVal = true; SectionIndex = aIndex;
            }
            if (aPartType == uppPartTypes.Distributor)
            {
                if (DistributorIndex != aIndex) _rVal = true; DistributorIndex = aIndex;
            }
            PartIndex = aIndex;

            return _rVal;

        }

        public void CopyPositiveIndices(TPARTINDICES aIndices)
        {
            if (aIndices.ColumnIndex > 0) { ColumnIndex = aIndices.ColumnIndex; }
            if (aIndices.RangeIndex > 0) { RangeIndex = aIndices.RangeIndex; }
            if (aIndices.DowncomerIndex > 0) { DowncomerIndex = aIndices.DowncomerIndex; }
            if (aIndices.PanIndex > 0) { PanIndex = aIndices.PanIndex; }
            if (aIndices.PanelIndex > 0) { PanelIndex = aIndices.PanelIndex; }
            if (aIndices.DistributorIndex > 0) { DistributorIndex = aIndices.DistributorIndex; }
            if (aIndices.ChimneyTrayIndex > 0) { ChimneyTrayIndex = aIndices.ChimneyTrayIndex; }
        }

    }



    internal struct TMDDOWNCOMER : ICloneable
    {
        public int Index;
        public double X;
        public double Y;
        public double LongLength;
        public double ShortLength;
        public double How;
        public bool FoldOverWeirs;
        public bool TriangularEndplates;
        public double Width;
        public double Height;

        public TMDDOWNCOMER(double aX)
        {
            Index = -1;
            X = aX;
            Y = 0;
            LongLength = 0;
            ShortLength = 0;
            How = 0;
            TriangularEndplates = false;
            FoldOverWeirs = false;
            Width = 0;
            Height = 0;


        }

        object ICloneable.Clone() => (object)Clone();

        public TMDDOWNCOMER Clone()
        {
            return new TMDDOWNCOMER
            {
                Index = Index,
                X = X,
                Y = Y,
                LongLength = LongLength,
                ShortLength = ShortLength,
                How = How,
                TriangularEndplates = TriangularEndplates,
                FoldOverWeirs = FoldOverWeirs,
                Width = Width,
                Height = Height,
            };
        }

    }




    internal struct UHOLE : ICloneable
    {
        public double Radius;
        public double Length;
        public double MinorRadius;
        public double CornerRadius;
        public double Depth;
        public bool WeldedBolt;
        public bool IsSquare;
        public double Rotation;
        public UVECTOR Center;
        private string _Tag;
        private string _Flag;
        public string _ZDirection;
        private double _Elevation;
        public double Value;
        public string Name;
        public int Index;

        public UHOLE(double aDiameter = 0, double aX = 0, double aY = 0, double aLength = 0, string aTag = "", double aDepth = 0,
            double aRotation = 0, double aElevation = 0, string aZDirection = "0,0,1", string aFlag = "", double aInset = 0,
            double aDownSet = 0, double aMinorRadius = 0, bool bWeldedBolt = false, bool bIsSquare = false, double aCornerRadius = 0)

        {

            Center = new UVECTOR(aX, aY);
            _Tag = aTag;
            _Flag = aFlag;
            Rotation = aRotation;
            _Elevation = aElevation;
            Radius = Math.Abs(aDiameter) / 2;
            Length = Math.Abs(aLength);
            Depth = Math.Abs(aDepth);
            Name = string.Empty;
            Index = 0;
            _ZDirection = "0,0,1";
            Value = 0;
            WeldedBolt = bWeldedBolt;
            IsSquare = bIsSquare;
            MinorRadius = aMinorRadius;
            CornerRadius = aCornerRadius;
            DownSet = aDownSet;
            InSet = aInset;
            if (string.IsNullOrWhiteSpace(aZDirection)) aZDirection = _ZDirection;
            ZDirection = aZDirection;
            WeldedBolt = bWeldedBolt;
            Center.Elevation = _Elevation;


        }

        public UHOLE(string aName = "")
        {
            Center = UVECTOR.Zero;
            Rotation = 0;
            _Elevation = 0;
            Radius = 0;
            Length = 0;
            Depth = 0;
            Name = aName;
            Index = 0;
            _ZDirection = "0,0,1";
            Value = 0;
            WeldedBolt = false;
            IsSquare = false;
            MinorRadius = 0;
            CornerRadius = 0;
            _Tag = string.Empty;
            _Flag = string.Empty;
        }

        public UHOLE(UHOLE aHole)
        {
            Center = new UVECTOR(aHole.Center);
            Rotation = aHole.Rotation;
            _Elevation = aHole.Elevation;
            Radius = aHole.Radius;
            Length = aHole.Length;
            Depth = aHole.Depth;
            Name = aHole.Name;
            Index = aHole.Index;
            _ZDirection = aHole._ZDirection;
            Value = aHole.Value;
            WeldedBolt = aHole.WeldedBolt;
            IsSquare = aHole.IsSquare;
            MinorRadius = aHole.MinorRadius;
            CornerRadius = aHole.CornerRadius;

            _Tag = aHole.Tag;
            _Flag = aHole.Flag;
            Center.Elevation = _Elevation;
        }

        public UHOLE(uopHole aHole)
        {
            Center = UVECTOR.Zero;
            Rotation = 0;
            _Elevation = 0;
            Radius = 0;
            Length = 0;
            Depth = 0;
            Name = string.Empty;
            Index = 0;
            _ZDirection = "0,0,1";
            Value = 0;
            WeldedBolt = false;
            IsSquare = false;
            MinorRadius = 0;
            CornerRadius = 0;
            _Tag = string.Empty;
            _Flag = string.Empty;
            if (aHole == null) return;

            Center = new UVECTOR(aHole.Center);
            Rotation = aHole.Rotation;
            Elevation = aHole.Z;
            Radius = aHole.Radius;
            Length = aHole.Length;
            Depth = aHole.Depth;
            Name = aHole.Name;
            Index = aHole.Index;
            _ZDirection = aHole.ExtrusionDirection;
            Value = aHole.Value;
            WeldedBolt = aHole.WeldedBolt;
            IsSquare = aHole.IsSquare;
            MinorRadius = aHole.MinorRadius;
            CornerRadius = aHole.CornerRadius;
            _Tag = aHole.Tag;
            _Flag = aHole.Flag;
            Inset = aHole.Inset;
            DownSet = aHole.DownSet;
        }

        object ICloneable.Clone() => (object)Clone();

        /// <summary>
        ///returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>
        public UHOLE Clone() => new UHOLE(this);


        public string Tag { get => _Tag; set { _Tag = value; } }
        public string Flag { get => _Flag; set { _Flag = value; } }

        public double Elevation { get => _Elevation; set { _Elevation = value; Center.Elevation = value; } }
        public uppHoleTypes HoleType => (Length > Diameter) ? uppHoleTypes.Slot : uppHoleTypes.Hole;

        public override string ToString()
        {

            string _rVal;

            if (!IsElongated)
            {
                _rVal = IsSquare ? $"SQR. HOLE {Dimensions}" : $"HOLE {Dimensions}";
            }
            else
            {
                _rVal = IsSquare ? $"SQR. SLOT {Dimensions}" : $"SLOT {Dimensions}";

            }

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

            if (suf !=  string.Empty) _rVal = $"{_rVal} [{suf}]";

            return _rVal;
        }

        public string Dimensions
        {
            get
            {
                return IsElongated ? $"{Diameter.ToString("0.0###")} x {Length.ToString("0.0###")}" : Diameter.ToString("0.0###");
            }
        }

        public UVECTORS ArcCenters { get => (HoleType == uppHoleTypes.Hole) ? new UVECTORS(Center) : new UVECTORS(new UVECTOR(X - (Length / 2 - Radius), Y), new UVECTOR(X + (Length / 2 - Radius), Y)); }

        public double Diameter { get => 2 * Radius; set { if (value > 0) { Radius = value / 2; } } }

        public double Inset { get => Center.Inset; set => Center.Inset = value; }


        public bool Suppressed => Center.Suppressed;

        public colDXFEntities ToDXFEntities(string aLayerName = "", dxxColors aColor = dxxColors.Undefined, string aLinetype = "", double aHClineScale = 0, double aVClineScale = 0, double aDClineScale = 0, dxfImage aImage = null, dxxLinetypeLayerFlag aLTLSetting = dxxLinetypeLayerFlag.Undefined, bool bSuppressCenterPts = false)
        {

            colDXFEntities _rVal = new colDXFEntities();
            dxfEntity aEnt = BoundingDXFEntity(aLayerName, aColor, aLinetype, out dxfRectangle rBounds);
            aEnt.UpdatePath(aImage: aImage);
            if (aEnt == null) return _rVal;
            if (aImage != null) { aImage.LinetypeLayers.ApplyTo(aEnt, aLTLSetting, aImage); }


            if (Math.Abs(rBounds.ZDirection.Z) == 1)
            {
                _rVal.Add(aEnt);
                if (!bSuppressCenterPts)
                    _rVal.AddPoint(aEnt, aHandlePt: dxxEntDefPointTypes.Center);

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
                if (!bSuppressCenterPts) _rVal.AddPoint(bRec.Center, aLayerName, dxxColors.ByLayer, dxfLinetypes.Continuous);

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

        /// <summary>
        /// a string that defines a  hole on the XY plane
        /// </summary>
        /// <param name="aHole"></param>
        /// <returns></returns>
        public string SimpleDescriptor
        {
            get
            {

                if (Math.Round(Length, 3) > Math.Round(Radius, 3))
                {
                    return "(1," + Length + "," + 2 * Radius + ",0," + X + "," + Y + ")";
                }
                else
                {
                    return "(0," + 2 * Radius + "," + X + "," + Y + ")";
                }

            }
            set
            {


                if (string.IsNullOrWhiteSpace(value)) return;
                value = value.Trim();

                if (value.IndexOf(",") == -1) return;
                List<string> aMems = mzUtils.ListValues(value, ",", bRemoveParens: true);

                int cnt = aMems.Count;

                if (cnt < 2) return;

                if (mzUtils.VarToInteger(aMems[0], true, 0, 0, 1) == 1)
                {
                    Length = mzUtils.VarToDouble(aMems[1], true);
                    if (cnt >= 3) Radius = mzUtils.VarToDouble(aMems[2], true) / 2.0;
                    if (cnt >= 4) Rotation = mzUtils.VarToDouble(aMems[3]);
                    if (cnt >= 5) X = mzUtils.VarToDouble(aMems[4]);
                    if (cnt >= 6) Y = mzUtils.VarToDouble(aMems[5]);

                }
                else
                {
                    if (cnt >= 2) Radius = mzUtils.VarToDouble(aMems[1], true) / 2.0;
                    if (cnt >= 3) X = mzUtils.VarToDouble(aMems[2]);
                    if (cnt >= 4) Y = mzUtils.VarToDouble(aMems[3]);
                }
            }
        }

        public string ZDirection
        {
            get => _ZDirection;
            set
            {
                dxfDirection aDir = new dxfDirection(value);
                _ZDirection = aDir.Components;
                _ZDirection = _ZDirection.Replace("(", "");
                _ZDirection = _ZDirection.Replace(")", "");
                if (_ZDirection == "0,0,0")
                {
                    _ZDirection = "0,0,1";
                }
            }
        }

        public double X { get => Center.X; set => Center.X = value; }

        public double Y { get => Center.Y; set => Center.Y = value; }

        public dxfPlane Plane(bool Rotated = false) { dxfPlane _rVal = new dxfPlane(Center.ToDXFVector(), aZDir: new dxfVector(ZDirection)); if (Rotation != 0 && Rotated) { _rVal.Rotate(Rotation); } return _rVal; }

        public dxeHole ToDXFHole
        {
            get
            {
                dxeHole _rVal = new dxeHole(new dxfVector(X, Y, Elevation), Radius, Length, Tag, Depth, Rotation, Flag, InSet, DownSet, MinorRadius, WeldedBolt, IsSquare, Plane())
                {
                    MinorRadius = MinorRadius,
                    WeldedBolt = WeldedBolt
                };
                return _rVal;
            }
        }

        public double Area => uopUtils.HoleArea(Radius, MinorRadius, Length, IsSquare);

        public URECTANGLE BoundaryRectangle
        {
            get
            {
                URECTANGLE _rVal = URECTANGLE.Null;

                UHOLE.GetData(this, out double rad, out double mrad, out double lng);
                _rVal.Define(X - lng / 2, X + lng / 2, Y + rad, Y - rad);
                return _rVal;

            }
        }

        public bool IsElongated => Length > Diameter;

        public double DownSet { get => Center.DownSet; set => Center.DownSet = value; }

        public double InSet { get => Center.Inset; set => Center.Inset = value; }


        public string Descriptor
        {
            //^a string that completely defines the slots current property values
            //~see dxfSlot.DefineByString
            get
            {
                string _rVal;
                if (IsElongated) { _rVal = "(0"; } else { _rVal = "(1"; }
                _rVal += "," + Center.Coords(4, 0);
                _rVal += "," + Radius;
                _rVal += "," + Length;
                _rVal += "," + Rotation;
                _rVal += "," + Depth;
                _rVal += "," + DownSet;
                _rVal += "," + InSet;
                _rVal += "," + InSet;
                _rVal += "," + Convert.ToInt32(IsSquare);
                _rVal += "," + Tag;
                _rVal += "," + Flag;
                _rVal += "," + MinorRadius;
                _rVal += "," + ZDirection;
                _rVal += "," + ")";
                return _rVal;
            }

        }

        public void SetProps( double? aRadius = null, double? aLength = null, double? aDepth = null, double? aRotation = null, string aTag = null, string aExtrusionDirection = null, double? aMinorRadius = null, bool? bIsSquare = null, bool? bWeldedBolt = null)
        {
            if (aRadius.HasValue) Radius = Math.Abs(aRadius.Value);
            if (aLength.HasValue) Length = Math.Abs(aLength.Value);
            if (aMinorRadius.HasValue) MinorRadius = Math.Abs(aMinorRadius.Value);
            if (aDepth.HasValue) Depth = Math.Abs(aDepth.Value);

            if (bIsSquare.HasValue) IsSquare = bIsSquare.Value;
            if (bWeldedBolt.HasValue) WeldedBolt = bWeldedBolt.Value;
            if (aRotation.HasValue) Rotation = mzUtils.NormAng(aRotation.Value, false, true, true);

            if (!string.IsNullOrWhiteSpace(aExtrusionDirection)) ZDirection = aExtrusionDirection;
            
        }

        #region Shared Methods

        public static UHOLE SetProperties(UHOLE aHole, double? aRadius = null, double? aLength = null, double? aDepth = null, double? aRotation = null, string aTag = null, string aExtrusionDirection = null, double? aMinorRadius = null, bool? bIsSquare = null, bool? bWeldedBolt = null)
        {
            if (aRadius.HasValue) aHole.Radius = Math.Abs(aRadius.Value);
            if (aLength.HasValue) aHole.Length = Math.Abs(aLength.Value);
            if (aMinorRadius.HasValue) aHole.MinorRadius = Math.Abs(aMinorRadius.Value);
            if (aDepth.HasValue) aHole.Depth = Math.Abs(aDepth.Value);
         
            if (bIsSquare.HasValue) aHole.IsSquare = bIsSquare.Value;
            if (bWeldedBolt.HasValue) aHole.WeldedBolt = bWeldedBolt.Value;
            if (aRotation.HasValue) aHole.Rotation = mzUtils.NormAng(aRotation.Value,false, true, true);

            if (!string.IsNullOrWhiteSpace(aExtrusionDirection)) aHole.ZDirection = aExtrusionDirection;
            return aHole;
        }

        internal static UHOLE FromDXFHole(dxeHole aHole)
        {
            if (aHole == null) { return new UHOLE(""); }

            return new UHOLE(aDiameter: aHole.Diameter, aX: aHole.X, aY: aHole.Y, aLength: aHole.Length,
                aTag: aHole.Tag, aDepth: aHole.Depth, aRotation: aHole.Rotation, aElevation: aHole.Z, aZDirection: aHole.Plane.ExtrusionDirection,
                aInset: aHole.Inset, aDownSet: aHole.DownSet, aMinorRadius: aHole.MinorRadius, aFlag: aHole.Flag, bWeldedBolt: aHole.WeldedBolt, bIsSquare: aHole.IsSquare);

        }
        public static void GetData(UHOLE aHole, out double rRadius, out double rMinorRadius, out double rLength, double aScale = 1)
        {
            rRadius = Math.Round(Math.Abs(aHole.Radius * aScale), 6);
            rMinorRadius = Math.Round(Math.Abs(aHole.MinorRadius * aScale), 6);
            if (rMinorRadius >= rRadius) { rMinorRadius = 0; }
            rLength = Math.Round(Math.Abs(aHole.Length * aScale), 6);
            if (rLength < 2 * rRadius) { rLength = 2 * rRadius; }
            if (aHole.IsSquare) { rMinorRadius = 0; }
            if (rLength > 2 * rRadius) { rMinorRadius = 0; }
        }

        internal static UHOLE FromSimpleDescriptor(string aDescriptor)
        {
            UHOLE _rVal = new UHOLE("")
            {
                SimpleDescriptor = aDescriptor
            };
            return _rVal;
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
                    _rVal.Add(aPln, -0.5 * Length, Radius);
                    _rVal.Add(aPln, -0.5 * Length, -Radius);
                    _rVal.Add(aPln, 0.5 * Length, -Radius);
                    _rVal.Add(aPln, 0.5 * Length, Radius);
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


        public dxfEntity BoundingDXFEntity(string aLayerName, dxxColors aColor, string aLinetype, out dxfRectangle rBounds, double aScale = 1)
        {
            rBounds = new dxfRectangle();
            dxfEntity _rVal = null;

            UHOLE.GetData(this, out double rad, out double mrad, out double lng, aScale);
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

        public static UHOLE Null => new UHOLE("");
        #endregion Shared Methods
    }

    internal struct UHOLES : ICloneable
    {
        public double Rotation;
        public bool Invalid;
        public string Name;
        public int Index;
        public UHOLE Member;
        public UVECTORS Centers;

        public UHOLES(string aName = "")
        {
            Rotation = 0;
            Invalid = false;
            Member = new UHOLE("");
            Centers = UVECTORS.Zero;
            Name = aName;
            Index = 0;

        }

        public UHOLES(UHOLES aHoles, bool bClear = false)
        {
            Rotation = aHoles.Rotation;
            Invalid = aHoles.Invalid;
            Member = new UHOLE(aHoles.Member);
            Centers = bClear ? UVECTORS.Zero : new UVECTORS(aHoles.Centers);
            Name = aHoles.Name;
            Index = aHoles.Index;

        }

        public UHOLES(uopHoles aHoles, bool bClear = false)
        {
            Rotation = 0;
            Invalid = false;
            Member = new UHOLE("");
            Centers = UVECTORS.Zero;
            Name = string.Empty;
            Index = 0;

            if(aHoles != null)
            {
                Rotation = aHoles.Rotation;
                Invalid = aHoles.Invalid;
                Member = new UHOLE(aHoles.Member);
                Centers = bClear ? UVECTORS.Zero : new UVECTORS(aHoles.Centers);
                Name = aHoles.Name;
                Index = aHoles.Index;

            }

        }

        public UHOLES(UHOLE aBaseHole, UVECTORS aCenters)
        {
            Rotation = 0;
            Invalid = false;
            Member = new UHOLE(aBaseHole);
            Centers = new UVECTORS(aCenters);
            Name = string.Empty;
            Index = 0;

        }

        public UHOLES(bool bInvalid = true, string aTag = "", double aRadius = 0, double aLength = 0, double aDepth = 0, double aRotation = 0, double aElevation = 0, string aZDirection = "0,0,1")
        {
            Rotation = aRotation;
            Invalid = bInvalid;
            Member = new UHOLE("");
            Centers = UVECTORS.Zero;
            Name = string.Empty;
            Index = 0;
            Member.Elevation = aElevation;
            Member.Radius = Math.Abs(aRadius);
            Member.Length = Math.Abs(aLength);
            Member.Depth = Math.Abs(aDepth);
            Member.Tag = aTag;
            Member.ZDirection = aZDirection;

        }

        public UHOLES Clone(bool bReturnEmpty = false) => new UHOLES(this, bReturnEmpty);


        public URECTANGLE BoundaryRectangle

        {
            get
            {
                URECTANGLE _rVal = URECTANGLE.Null;
                UHOLE aHl;
                int j = 0;
                for (int i = 1; i <= Centers.Count; i++)
                {
                    if (!Centers.Item(i).Suppressed)
                    {
                        j += 1;
                        aHl = Item(i);
                        if (j == 1)
                        { _rVal = aHl.BoundaryRectangle; }
                        else
                        { _rVal.ExpandTo(aHl.BoundaryRectangle); }
                    }
                }
                return _rVal;

            }
        }

        public override string ToString() => string.IsNullOrWhiteSpace(Name) ? $"UHOLES[{Count}]" : $"UHOLES[{ Name }] [{ Count }]";

        public colDXFEntities ToDXFEntities(string aLayerName = "", dxxColors aColor = dxxColors.Undefined, string aLinetype = "", double aHClineScale = 0, double aVClineScale = 0, double aDClineScale = 0, dxfImage aImage = null, dxxLinetypeLayerFlag aLTLSetting = dxxLinetypeLayerFlag.Undefined, bool bSuppressInstances = false, double? aXMirrorOrd = null, bool bSuppressCenterPts = false)
        {
            colDXFEntities _rVal = new colDXFEntities();
            if (Count <= 0) return _rVal;


            UHOLE.GetData(Member, out double rad, out double mrad, out double lng);
            UHOLE aHl = Item(1);
            UVECTOR u1 = aHl.Center;

            colDXFEntities aEnts = aHl.ToDXFEntities(aLayerName, aColor, aLinetype, aHClineScale, aVClineScale, aDClineScale, aImage, aLTLSetting, bSuppressCenterPts);

            dxeLine aLn = null;
            UVECTOR u2;
            bool bXMir = false;
            double sXMir = 0;
            double f1 = 0;
            if (aXMirrorOrd != null)
            {
                bXMir = aXMirrorOrd.HasValue;
                if (bXMir) sXMir = aXMirrorOrd.Value;
            }

            UVECTORS oSets = UVECTORS.Zero;
            // get the center offsets ffrom the first 
            for (int i = 1; i <= Centers.Count; i++)
            {
                u2 = Centers.Item(i);
                if (!u2.Suppressed)
                {
                    f1 = 0;
                    double slng = u2.Value;
                    if (slng > 2 * rad)
                    {
                        if (Math.Round(slng, 6) != lng)
                        { f1 = (slng - lng) / 2; bSuppressInstances = true; }
                    }
                    oSets.Add(u2 - u1);
                    if (bXMir)
                    {
                        u2.Y += -2 * (u2.Y - sXMir);
                        oSets.Add(u2 - u1);
                    }
                }
            }


            if (bSuppressInstances)
            {
                if (bXMir) { aLn = new dxeLine(new dxfVector(sXMir, 0), new dxfVector(sXMir, 100)); }
                for (int i = 1; i <= Centers.Count; i++)
                {
                    aHl = Item(i);
                    aEnts = aHl.ToDXFEntities(aLayerName, aColor, aLinetype, aHClineScale, aVClineScale, aDClineScale, aImage, aLTLSetting, bSuppressCenterPts);
                    _rVal.Append(aEnts);
                    if (bXMir) { _rVal.AppendMirrors(aEnts, aLn); }
                }
            }
            else
            {
                for (int i = 2; i <= oSets.Count; i++)
                {
                    u1 = oSets.Item(i);
                    for (int j = 1; j <= aEnts.Count; j++)
                    {
                        dxfEntity aEnt = aEnts.Item(j);
                        aEnt.Instances.Add(u1.X, u1.Y);
                    }
                }
                _rVal.Append(aEnts);


            }
            _rVal.UpdatePaths();
            return _rVal;
        }

        object ICloneable.Clone() => (object)Clone(false);

        public void Clear() => Centers.Clear();

        public int Count => Centers.Count;

        public UHOLE Item(int aIndex)
        {
            UHOLE _rVal = new UHOLE(Member) { Index = 0 };
            if (aIndex < 1 || aIndex > Count) return _rVal;
            _rVal.Index = aIndex;
            UVECTOR ctr = Centers.Item(aIndex);

            if (ctr.Value > Member.Diameter)
            {
                if (Math.Round(ctr.Value - Member.Diameter, 4) > 0) { _rVal.Length = ctr.Value; }
            }
            _rVal.Flag = ctr.Tag;
            _rVal.Rotation = Rotation;
            _rVal.Elevation = mzUtils.VarToDouble(ctr.Elevation, aDefault: Member.Elevation);
            _rVal.Center = ctr;

            return _rVal;
        }

        public int GetCount(bool aSuppressedVal)
        {
            int _rVal = 0;
            for (int i = 1; i <= Centers.Count; i++)
            {
                if (Centers.Item(i).Suppressed == aSuppressedVal) _rVal += 1;

            }

            return _rVal;

        }

        public void SetItem(int aIndex, UHOLE aHole)
        {
            if (aIndex < 1 || aIndex > Count) { return; }

            Centers.SetItem(aIndex, aHole.Center);
        }

        public void SetItem(int aIndex, UVECTOR aVector)
        {
            if (aIndex < 1 || aIndex > Count) { return; }
            Centers.SetItem(aIndex, aVector);
        }
        //#1the X coordinate to match
        //#2the Y coordinate to match
        //#3a precision for the comparison (1 to 16)
        //^searchs for and returns vectors from the collection whose coordinates match the passed coordinates
        //~if an any of the ordinates (X, Y or Z) are not passed or are not numeric they are not used in the comparison.
        //~say ony an X value is passed, then all the vectors with the same X ordinate are returned regarless of their
        //~respective Y and Z ordinate values.
        public UHOLES GetAtCoordinate(dynamic aX = null, dynamic aY = null, int aPrecis = 3, bool bRemove = false, bool bJustOne = false)
        {
            UHOLES _rVal = new UHOLES(this, true)
            {
                Centers = Centers.GetAtCoordinate(aX, aY, aPrecis, bRemove, bJustOne)
            };
            return _rVal;
        }

        //'#1flag indicating what type of vector to search for
        //'#2the ordinate to search for if the search is ordinate specific
        //'#3a precision for numerical comparison (1 to 8)

        //'^returns a hole from the collection whose properties or position in the collection match the passed control flag

        //'~search for vectors at extremes
        //'~returns the first one that satisfies
        public UHOLE GetByPoint(dxxPointFilters aFilter, double aOrdinate = 0, int aPrecis = 3, bool bRemove = false)
        {
            UVECTOR u1 = Centers.GetVector(aFilter, aOrdinate, aPrecis);
            if (u1.Index <= 0) return new UHOLE("");
            UHOLE _rVal = Item(u1.Index);
            if (bRemove) Remove(u1.Index);
            return _rVal;

        }

        /// <summary>
        /// method to rotate
        /// </summary>
        /// <param name="aAngle"></param>
        /// <param name="aCenter"></param>
        public void Rotate(double aAngle, UVECTOR aCenter) => Centers.Rotate(aCenter, aAngle, false);

        /// <summary>
        /// method for setting properties
        /// </summary>
        /// <param name="aRadius"></param>
        /// <param name="aLength"></param>
        /// <param name="aDepth"></param>
        /// <param name="aRotation"></param>
        /// <param name="aTag"></param>
        /// <param name="aExtrusionDirection"></param>
        /// <param name="aMinorRadius"></param>
        /// <param name="bIsSquare"></param>
        /// <param name="bWeldedBolt"></param>
        public void SetProperties(double? aRadius = null, double? aLength = null,double? aDepth = null,double? aRotation = null, string aTag = null, string aExtrusionDirection = null, double? aMinorRadius = null, bool? bIsSquare = null, bool? bWeldedBolt = null)
        {
           Member.SetProps( aRadius, aLength, aDepth, aRotation, aTag, aExtrusionDirection, aMinorRadius, bIsSquare, bWeldedBolt);

        }


        public double Diameter { get => 2 * Member.Radius; set => Member.Radius = Math.Abs(value) / 2; }

        public double Radius { get => Member.Radius; set => Member.Radius = Math.Abs(value); }

        public double MinorRadius { get => Member.MinorRadius; set => Member.MinorRadius = Math.Abs(value); }

        public double Length { get => Member.Length; set => Member.Length = Math.Abs(value); }

        public double Depth { get => Member.Depth; set => Member.Depth = Math.Abs(value); }

        public bool WeldedBolt { get => Member.WeldedBolt; set => Member.WeldedBolt = value; }

        public bool IsSquare { get => Member.IsSquare; set => Member.IsSquare = value; }

        public string ZDirection { get => Member.ZDirection; set => Member.ZDirection = value; }


        public UVECTORS ArcCenters
        {
            get
            {
                UVECTORS _rVal = UVECTORS.Zero;
                UHOLE aHl;
                for (int i = 1; i <= Count; i++)
                {
                    aHl = Item(i);
                    _rVal.Append(aHl.ArcCenters);
                }
                return _rVal;

            }
        }

        public string SimpleDescriptor
        {
            get
            {
                string _rVal = string.Empty;
                //^a string that defines a  hole on the XY plane
                if (Count <= 0) { return _rVal; }


                UHOLE bHl;

                UHOLE.GetData(Member, out double rad, out double mrad, out double lng);
                UHOLE aHl = Item(1);
                UVECTOR u1 = aHl.Center;
                for (int i = 1; i <= Centers.Count; i++)
                {
                    u1 = Centers.Item(i);
                    if (!u1.Suppressed)
                    {
                        bHl = Item(i);
                        mzUtils.ListAdd(ref _rVal, bHl.SimpleDescriptor, bSuppressTest: true, aDelimitor: uopGlobals.Delim);
                    }
                }
                return _rVal;

            }
        }


        public string UniformDescriptor

        {
            //#1the subject holes
            //^returns a string that describes a set of holes with the same size and orientation
            //~the first dat set describes the hole. The rest describe the subsequent centers.
            get
            {
                string _rVal = string.Empty;
                if (Centers.Count <= 0)
                {
                    return _rVal;
                }
                UVECTOR u2;



                UHOLE.GetData(Member, out double rad, out double mrad, out double lng);
                UHOLE aHl = Item(1);
                UVECTOR u1 = aHl.Center;
                _rVal = "U" + aHl.SimpleDescriptor;

                for (int i = 2; i <= Centers.Count; i++)
                {
                    u2 = Centers.Item(i);
                    if (!u2.Suppressed)
                    {
                        mzUtils.ListAdd(ref _rVal, "(" + u2.X + "," + u2.Y + ")", bSuppressTest: true, aDelimitor: uopGlobals.Delim);
                    }
                }
                return _rVal;

            }
        }


        public List<double> Lengths(string aNamesList = "")
        {
            List<double> _rVal =  new List<double>();
            bool testit = !string.IsNullOrEmpty(aNamesList);
            for (int i = 1; i <= Count; i++)
            {
                UHOLE mem= Item(i);
                if (testit)
                {
                    if (!mzUtils.ListContains(mem.Name, aNamesList)) continue;

                }
                else 
                {
                    _rVal.Add(mem.Length);
                 }
            }

            return _rVal;
        }

        public double Area(string aNamesList = "")
        {
            double _rVal = 0;
            bool testit = !string.IsNullOrEmpty(aNamesList);
            
            for (int i = 1; i <= Count; i++)
            {
                UHOLE mem = Item(i);
                if (testit)
                {
                    if (mzUtils.ListContains(mem.Name, aNamesList)) 
                        _rVal += mem.Area; 

                }
                else { _rVal += mem.Area; }
            }

            return _rVal;
        }

        public UHOLE Nearest(double aX, double aY) => Nearest(aX, aY, out int _);


        public UHOLE Nearest(double aX, double aY, out int rIndex)

        {
            rIndex = 0;
            UHOLE _rVal = new UHOLE("");
            Centers.Nearest(new UVECTOR(aX, aY), out rIndex);
            if (rIndex >= 0) { _rVal = Item(rIndex); }
            return _rVal;
        }


        public void AppendMirrors(double? aX, double? aY)
        {
            if (Count <= 0 || (!aX.HasValue && !aY.HasValue)) return;


            Centers.Append(Centers.Mirrored(aX, aY));
        }


        public colDXFEntities DrawToImage(dxfImage aImage, string aLayerName = "", dxxColors aColor = dxxColors.Undefined, string aLinetype = "", dxxLinetypeLayerFlag aLTLSetting = dxxLinetypeLayerFlag.Undefined, double aHClineScale = 0, double aVClineScale = 0, double aDClineScale = 0, bool bSuppressInstances = false)
        {
            colDXFEntities _rVal = new colDXFEntities();

            colDXFEntities aEnts = ToDXFEntities(aLayerName, aColor, aLinetype, aHClineScale, aVClineScale, aDClineScale, aImage, aLTLSetting, bSuppressInstances);
            if (aImage == null || aEnts.Count <= 0) return aEnts;


            dxfEntity aEnt = null;
            double dX = aImage.X;
            double dY = aImage.Y;
            bool bTrans = false;
            bTrans = dX != 0 || dY != 0;
            for (int i = 1; i <= aEnts.Count; i++)
            {
                aEnt = aEnts.Item(i);
                if (bTrans) aEnt.Move(dX, dY);

                _rVal.Add(aImage.Entities.Add(aEnt));
            }
            return _rVal;
        }


        public UHOLE Remove(int aIndex)
        {
            UHOLE _rVal = new UHOLE("");
            if (aIndex < 1 || aIndex > Count) { return _rVal; }
            _rVal = Item(aIndex);

            Centers.Remove(aIndex);

            return _rVal;

        }

        public bool TryGetByFlag(string aFlag, out UHOLE rHole, string aTag = null)
        {
            rHole = new UHOLE("");
            if (aFlag == null) return false;
            rHole = GetFlagged(aFlag, out int idx, false, aTag);
            return idx > 0;

        }


        public UHOLE GetFlagged(string aFlag, bool bRemove = false, string aTag = null) => GetFlagged(aFlag, out int _, bRemove, aTag);


        public UHOLE GetFlagged(string aFlag, out int rIndex, bool bRemove = false, string aTag = null)
        {
            UHOLE _rVal = new UHOLE("");
            rIndex = 0;
            UHOLE mem;
            for (int i = 1; i <= Count; i++)
            {
                mem = Item(i);
                if (string.Compare(mem.Flag, aFlag, true) == 0)
                {
                    if (aTag != null)
                    {
                        if (string.Compare(mem.Tag, aTag, true) != 0) continue;
                    }

                    rIndex = i;
                    _rVal = mem;
                    break;
                }
            }

            if (bRemove && rIndex > 0) { Remove(rIndex); }
            return _rVal;
        }

        #region Shared Methods

        internal static UHOLES FromDescriptor(string aDescriptors)
        {
            UHOLES _rVal = UHOLES.Null;
            if (string.IsNullOrWhiteSpace(aDescriptors)) { return _rVal; }

            //#1a string that defines a holes array on the XY plane
            //^returns a hole array defined by the passed descriptor
            //~if th efirst character is a 'U' the string is a uniform set of holes otherwise each data set describes each hole.
            //~see hls_FromUniformDescriptor and hls_FromSimpleDescriptor
            aDescriptors = aDescriptors.Trim();

            if (string.Compare(aDescriptors[0].ToString(), "U", true) == 0)
            {
                _rVal = UHOLES.FromUniformDescriptor(aDescriptors);
            }
            else
            {
                _rVal = UHOLES.FromSimpleDescriptor(aDescriptors);
            }
            return _rVal;
        }


        /// <summary>
        /// a string that defines a hole on the XY plane
        /// </summary>
        /// <param name="aDescriptors"></param>
        /// <returns></returns>
        internal static UHOLES FromSimpleDescriptor(string aDescriptors)
        {
            UHOLES _rVal = UHOLES.Null;
            if (string.IsNullOrWhiteSpace(aDescriptors)) return _rVal;
            aDescriptors = aDescriptors.Trim();

            List<string> aMems = mzUtils.ListValues(aDescriptors, uopGlobals.Delim);
            int cnt  = aMems.Count;
            if (cnt <= 0) return _rVal;

            _rVal.Member = UHOLE.FromSimpleDescriptor(aMems[0]);

            for (int i = 0; i < cnt; i++)
            {
                UHOLE aHl = UHOLE.FromSimpleDescriptor(aMems[i]);
                UVECTOR u1 = aHl.Center;
                u1.Value = aHl.Length;
                _rVal.Centers.Add(u1.Clone(true));
            }
            return _rVal;
        }


        internal static UHOLES FromUniformDescriptor(string aDescriptors)
        {
            UHOLES _rVal = UHOLES.Null;
            //#1a string that defines a holes array on the XY plane
            //^returns a hole array with holes of the same size and orientant
            //~the first dat set describes the hole. The rest describe the subsequent centers.
            if (string.IsNullOrWhiteSpace(aDescriptors))  return _rVal; 
            aDescriptors = aDescriptors.Trim();

            int i = aDescriptors.IndexOf("(");
            if (i > 0) 
                aDescriptors = aDescriptors.Substring(i); 
            List<string> aMems = mzUtils.ListValues(aDescriptors, uopGlobals.Delim);
            int cnt = aMems.Count;

            if (cnt <= 0) return _rVal;

            UHOLE aHl = UHOLE.FromSimpleDescriptor(aMems[0]);
            _rVal.Member = aHl;
            _rVal.Centers.Add(aHl.Center);

            for (i = 2; i <= cnt; i++)
            {
                string vStr = aMems[i - 1];
                if (vStr[0].ToString() == "(") vStr = vStr.Substring(1);

                if (vStr[vStr.Length - 1].ToString() == ")") vStr = vStr.Substring(0, vStr.Length - 1);

               
                List<double> ords = mzUtils.ListToNumericCollection(vStr, ",");
                if (ords.Count == 0) continue;
        
                 _rVal.Centers.Add(new UVECTOR(ords.Count >= 1 ? ords[0] : 0, ords.Count >= 2 ?  ords[1] : 0));
                

            }
            return _rVal;
        }

        public static UHOLES Null => new UHOLES("");

        #endregion
    }

    internal struct UHOLEARRAY : ICloneable
    {
        #region Fields

        public bool Invalid;
        private UHOLES[] _Members;
        private int _Count;
        private bool _Init;

        #endregion Fields

        #region Constructors

        public UHOLEARRAY(bool bInvalid = false)
        {
            Invalid = bInvalid;
            _Members = new UHOLES[0];
            _Count = 0;
            _Init = true;
        }

        public UHOLEARRAY(UHOLEARRAY aArray, bool bClear = false)
        {
            Invalid = aArray.Invalid;
            _Members = new UHOLES[0];
            _Count = bClear ? 0 : aArray.Count;
            _Init = true;
            if (_Count > 0)
            {
                _Members = Force.DeepCloner.DeepClonerExtensions.DeepClone<UHOLES[]>(aArray._Members);


            }
        }

        private void Init() { _Members = new UHOLES[0]; _Init = true; _Count = 0; }

        #endregion Constructors

        #region Properties

        public int Count { get { if (!_Init) { Init(); } return _Count; } }

        public string MemberNamesList
        {
            get
            {
                string _rVal = string.Empty;
                for (int i = 1; i <= Count; i++)
                {
                    mzUtils.ListAdd(ref _rVal, Item(i).Name, bSuppressTest: true, aDelimitor: ", ", bAllowNulls: true);
                }
                return _rVal;
            }
        }

        public List<UHOLES> ToList { get => new List<UHOLES>(_Members); }


        #endregion Properties

        #region Methods

        public void Clear() { Init(); }

        public UHOLES Add(UHOLES aHoles, string aName = null, bool bAppendToExisting = false)
        {
            if (Count + 1 > Int32.MaxValue) return new UHOLES("");
            if (aHoles.Count <= 0) return aHoles;
            if (!string.IsNullOrWhiteSpace(aName)) aHoles.Name = aName;
            if (string.IsNullOrWhiteSpace(aHoles.Name)) aHoles.Name = aHoles.Member.Tag;
            if (string.IsNullOrWhiteSpace(aHoles.Name)) aHoles.Name = $"ARRAY_{_Count + 1}";

            if (bAppendToExisting)
            {

                if (Contains(aHoles.Name, out int idx))
                {
                    UHOLES mem = Item(idx);
                    mem.Centers.Append(aHoles.Centers);
                    SetItem(idx, mem);
                    return Item(idx);
                }

            }
            _Count += 1;
            Array.Resize<UHOLES>(ref _Members, _Count);
            _Members[_Count - 1] = aHoles;
            _Members[_Count - 1].Index = _Count;
            return Item(Count);
        }

        public UHOLES Add(uopHoles aHoles, string aName = null, bool bAppendToExisting = false)
            => Add(new UHOLES(aHoles), aName, bAppendToExisting);
      
        public bool TryGet(string aName, out UHOLES rMember, bool bNamesLike = false)
        {
            rMember = UHOLES.Null;

            bool _rVal = Contains(aName, out int idx, bNamesLike);
            if (_rVal) rMember = Item(idx); 
            return _rVal;
        }

        public bool Contains(string aName, bool bNamesLike = false)
        {
            return Contains(aName, out int _, bNamesLike);
        }


        public bool Contains(string aName, out int rIndex, bool bNamesLike = false)
        {
            rIndex = 0;
            if (!bNamesLike)
            {
                UHOLES mems = Member(aName, out rIndex, false);
                return rIndex > 0;

            }

            UHOLES aHoles;
            for (int i = 1; i <= Count; i++)
            {
                aHoles = Item(i);
                if (aHoles.Name.IndexOf(aName, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    rIndex = i;
                    break;
                }
            }
            return rIndex > 0;
        }

        public UHOLES Item(int aIndex)
        {
            if (aIndex < 1 || aIndex > Count || aIndex > _Members.Count()) throw new IndexOutOfRangeException($"Requested Index: {aIndex} - Max Valid: {_Members.Count()}");
            _Members[aIndex - 1].Index = aIndex;
            return _Members[aIndex - 1];
        }

        public void SetItem(int aIndex, UHOLES aHoles)
        {
            if (aIndex < 1 || aIndex > Count) throw new IndexOutOfRangeException();
            _Members[aIndex - 1] = aHoles;
            _Members[aIndex - 1].Index = aIndex;
        }


        public override string ToString() => $"UHOLEARRAY[{MemberNamesList}]";

        /// <summary>
        ///returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>
        public object Clone() => (object)Clone(false);

        public UHOLEARRAY Clone(bool bReturnEmpty = false) => new UHOLEARRAY(this, bReturnEmpty);

        public void AppendMirrors(double? aX, double? aY, string aNames = "")
        {
            if (Count <= 0 || (!aX.HasValue && !aY.HasValue)) return;
            aNames ??= string.Empty;
            for (int i = 1; i <= Count; i++)
            {
                UHOLES mem = Item(i);
                if (mzUtils.ListContains(mem.Name, aNames, bReturnTrueForNullList: true))
                {
                    mem.AppendMirrors(aX, aY);
                    SetItem(i, mem);
                }
            }

        }

        public void Append(UHOLEARRAY aArray, bool bAppendToExisting) { for (int i = 1; i <= aArray.Count; i++) { Add(aArray.Item(i), bAppendToExisting: bAppendToExisting); } }

        public void Append(uopHoleArray aArray, bool bAppendToExisting = true) { if (aArray != null) Append(aArray.Structure_Get(), bAppendToExisting); }

        public void Append(UHOLEARRAY bHoleArray, string aNamesList = "", bool bAppendToExisting = false)
        {

            for (int i = 1; i <= bHoleArray.Count; i++)
            {
                UHOLES aHls = bHoleArray.Item(i);
                bool addit = mzUtils.ListContains(aHls.Name, aNamesList, bReturnTrueForNullList: true);
                if (addit)
                {
                    string nm = aHls.Name;
                    addit = !string.IsNullOrWhiteSpace(nm);
                    if (bAppendToExisting && addit)
                    {
                        addit = true;

                        if (TryGet(nm, out UHOLES mems))
                        {
                            mems.Centers.Append(aHls.Centers);
                            SetItem(mems.Index, mems);
                            addit = false;
                        }
                    }

                    if (addit)
                    {
                        Add(aHls);
                    }

                }
            }
        }

        public colDXFVectors DXFCenters(string aNamesList = "", uppHoleTypes aHoleType = uppHoleTypes.Any, dynamic aSuppressVal = null)
        {
            colDXFVectors _rVal = new colDXFVectors();

            UHOLES aHls;
            bool bTestSup = aSuppressVal != null;
            bool aSup = false;
            bool bKeep = false;
            UHOLE aHl;
            if (bTestSup) { aSup = mzUtils.VarToBoolean(aSuppressVal); }

            for (int i = 1; i <= Count; i++)
            {
                aHls = Item(i);
                if (mzUtils.ListContains(aHls.Name, aNamesList, bReturnTrueForNullList: true))
                {
                    for (int j = 1; j <= aHls.Centers.Count; j++)
                    {
                        aHl = aHls.Item(j);

                        bKeep = aHoleType == uppHoleTypes.Any || aHl.HoleType == aHoleType;
                        if (bTestSup)
                        {
                            if (aHl.Center.Suppressed != aSup) { bKeep = false; }
                        }
                        if (bKeep)
                        {
                            _rVal.Add(aHl.Center.ToDXFVector());
                        }
                    }
                }
            }
            return _rVal;
        }

        public string Names(string aDelimiter = ",")
        {
            string _rVal = string.Empty;
            int i = 0;
            for (i = 1; i <= Count; i++)
            {
                mzUtils.ListAdd(ref _rVal, Item(i).Name, bSuppressTest: true, aDelimitor: aDelimiter, bAllowNulls: true);
            }
            return _rVal;
        }

        public int HoleCount(string aNamesList, uppHoleTypes aHoleType = uppHoleTypes.Any, dynamic aSuppressVal = null)
        {
            int _rVal = 0;
            UHOLES aHls;
            for (int i = 1; i <= Count; i++)
            {
                aHls = Item(i);
                if (mzUtils.ListContains(aHls.Name, aNamesList, bReturnTrueForNullList: true))
                {
                    _rVal += ItemCount(i, aHoleType, aSuppressVal);
                }
            }
            return _rVal;
        }

        public int ItemCount(int aIndex, uppHoleTypes aHoleType = uppHoleTypes.Any, dynamic aSuppressVal = null)
        {
            if (aIndex < 1 || aIndex > Count || Count <= 0) { return 0; }

            int _rVal = 0;
            UHOLES aHoles = UHOLES.Null;
            aHoles = Item(aIndex);

            if (aHoleType == uppHoleTypes.Any)
            {
                if (aSuppressVal == null)
                {
                    _rVal = aHoles.Centers.Count;
                }
                else
                {
                    _rVal = aHoles.Centers.SuppressedCount(mzUtils.VarToBoolean(aSuppressVal));
                }
            }
            else
            {
                UHOLE aHl;
                for (int i = 1; i <= aHoles.Count; i++)
                {
                    aHl = aHoles.Item(i);
                    if (aHl.HoleType == aHoleType)
                    {
                        if (aSuppressVal == null)
                        { _rVal += 1; }
                        else
                        {
                            if (Convert.ToBoolean(aSuppressVal) == aHl.Center.Suppressed)
                            {
                                _rVal += 1;
                            }
                        }
                    }
                }
            }
            return _rVal;
        }


        public double TotalArea(string aArrayName = "")
        {
            double _rVal = 0;
            UHOLES mem;
            for (int i = 1; i <= Count; i++)
            {
                mem = Item(i);
                if (mzUtils.ListContains(mem.Name, aArrayName, ",", bReturnTrueForNullList: true)) { _rVal += mem.Area(); }
            }
            return _rVal;
        }


        public UHOLES Member(dynamic aNameOrIndex, bool bRemove = false) => Member(aNameOrIndex, out int _, bRemove);


        public UHOLES Member(dynamic aNameOrIndex, out int rIndex, bool bRemove = false)
        {
            rIndex = 0;
            UHOLES _rVal = UHOLES.Null;
            if (aNameOrIndex == null || Count <= 0) { return _rVal; }
            string tname = TVALUES.GetDynamicTypeName(aNameOrIndex);
            UHOLES mem;

            if (tname == "STRING")
            {
                string memname = Convert.ToString(aNameOrIndex);
                for (int i = 1; i <= Count; i++)
                {
                    mem = Item(i);
                    if (string.Compare(mem.Name, memname, ignoreCase: true) == 0)
                    {
                        _rVal = mem;
                        rIndex = i;
                        break;
                    }
                }
            }
            if (rIndex == 0)
            {
                if (mzUtils.IsNumeric(aNameOrIndex))
                {
                    _rVal = Item(mzUtils.VarToInteger(aNameOrIndex));
                    rIndex = _rVal.Index;
                }
            }

            if (rIndex > 0 && bRemove) { Remove(rIndex); }

            return _rVal;
        }

        public UHOLES Remove(int aIndex)
        {
            UHOLES _rVal = UHOLES.Null;
            if (aIndex < 1 || aIndex > Count) return _rVal;
            if (Count == 1) { _rVal = Item(1); Clear(); return _rVal; }

            if (aIndex == Count)
            {
                _rVal = Item(Count);
                _Count -= 1;
                Array.Resize<UHOLES>(ref _Members, _Count);
                return _rVal;
            }


            int cnt = Count;
            int j = 0;
            UHOLES[] newMems = new UHOLES[0];
            Array.Resize<UHOLES>(ref newMems, _Count - 1);
            for (int i = 1; i <= cnt; i++)
            {
                if (i != aIndex)
                {
                    newMems[j] = Item(i);
                    newMems[j].Index = j + 1;
                    j += 1;
                }
                else { _rVal = Item(i); }
            }
            _Count -= 1;
            _Members = newMems;

            return _rVal;

        }

        public UHOLES AddHole(UHOLE aHole, string aArrayName = "") => AddHole(aHole, aArrayName, out int _);


        public UHOLES AddHole(UHOLE aHole, string aArrayName, out int rArrayIndex)
        {
            rArrayIndex = 0;
            if (aArrayName ==  string.Empty) { aArrayName = aHole.Tag; }
            if (aArrayName ==  string.Empty) { return new UHOLES(""); }


            UHOLES mems = Member(aArrayName, out rArrayIndex);
            if (rArrayIndex <= 0) { return new UHOLES(""); }

            mems.Centers.Add(aHole.Center, aHole.Flag);
            SetItem(rArrayIndex, mems);
            return Item(rArrayIndex);
        }

        public UHOLE GetHole(dynamic aArrayNameOrIndex, dynamic aMemberFlagOrIndex) => Hole(aArrayNameOrIndex, aMemberFlagOrIndex, out int _, out int _);

        public UHOLE Hole(dynamic aArrayNameOrIndex, dynamic aMemberFlagOrIndex) => Hole(aArrayNameOrIndex, aMemberFlagOrIndex, out int _, out int _);

        public UHOLE Hole(dynamic aArrayNameOrIndex, dynamic aMemberFlagOrIndex, out int rArrayIndex, out int rIndex)
        {
            rArrayIndex = 0;
            rIndex = 0;
            UHOLE _rVal = new UHOLE("");
            UHOLES aHoles = UHOLES.Null;
            int i = 0;
            UHOLE aHl = new UHOLE("");
            bool bByFlag = false;
            bByFlag = aMemberFlagOrIndex.GetType().Name.ToString().ToUpper() == "STRING";
            rIndex = 0;
            aHoles = Member(aArrayNameOrIndex, out rArrayIndex);
            if (rArrayIndex <= 0) return _rVal;
            if (bByFlag)
            {
                for (i = 1; i <= aHoles.Centers.Count; i++)
                {
                    aHl = aHoles.Item(i);
                    if (string.Compare(aHl.Flag, aMemberFlagOrIndex, true) == 0)
                    {
                        rIndex = i;
                        _rVal = aHl;
                        break;
                    }
                }
            }
            else
            {
                i = mzUtils.VarToInteger(aMemberFlagOrIndex);
                if (i > 0 & i <= aHoles.Centers.Count)
                {
                    rIndex = i;
                    _rVal = aHoles.Item(i);
                }
            }
            return _rVal;
        }

        public int MemberCount(string aName, uppHoleTypes aHoleType = uppHoleTypes.Any, dynamic aSuppressVal = null, bool bNamesLike = false)
        {

            if (!Contains(aName, out int idx, bNamesLike)) { return 0; }

            int _rVal = 0;
            UHOLES aHoles;

            UHOLE aHl;
            bool bSupTest = aSuppressVal != null;
            bool bSup = false;
            if (bSupTest) { bSup = mzUtils.VarToBoolean(aSuppressVal); }
            if (!bNamesLike)
            {
                aHoles = Item(idx);
                if (!bSupTest && aHoleType == uppHoleTypes.Any)
                {
                    _rVal += aHoles.Count;
                }
                else
                {
                    for (int i = 1; i <= aHoles.Count; i++)
                    {
                        aHl = aHoles.Item(i);

                        if (aHl.HoleType == aHoleType || aHoleType == uppHoleTypes.Any)
                        {

                            if (!bSupTest || (bSupTest && aHl.Center.Suppressed == bSup))
                            {
                                _rVal += 1;
                            }
                        }
                    }
                }

                return _rVal;
            }

            bool bAdd = false;

            for (int j = 1; j <= Count; j++)
            {
                aHoles = Item(j);
                bAdd = aHoles.Name.IndexOf(aName, StringComparison.OrdinalIgnoreCase) >= 0;

                if (bAdd)
                {
                    if (!bSupTest && aHoleType == uppHoleTypes.Any)
                    {
                        _rVal += aHoles.Count;
                    }
                    else
                    {
                        for (int i = 1; i <= aHoles.Count; i++)
                        {
                            aHl = aHoles.Item(i);
                            if (aHl.HoleType == aHoleType || aHoleType == uppHoleTypes.Any)
                            {
                                if (!bSupTest || (bSupTest && aHl.Center.Suppressed == bSup))
                                {
                                    _rVal += 1;
                                }
                            }
                        }
                    }
                }
            }
            return _rVal;
        }


        public colDXFEntities ToDXFEntities(string aMemberName, string aLayerName = "", dxxColors aColor = dxxColors.Undefined, string aLinetype = "", double aHClineScale = 0, double aVClineScale = 0, double aDClineScale = 0, dxfImage aImage = null, dxxLinetypeLayerFlag aLTLSetting = dxxLinetypeLayerFlag.Undefined, bool bSuppressInstances = false, double? aXMirrorOrd = null)
        {
            if (!string.IsNullOrWhiteSpace(aMemberName)) { return Member(aMemberName).ToDXFEntities(aLayerName, aColor, aLinetype, aHClineScale, aVClineScale, aDClineScale, aImage, aLTLSetting, bSuppressInstances, aXMirrorOrd); }
            UHOLES mem;
            colDXFEntities _rVal = new colDXFEntities();
            for (int i = 1; i <= Count; i++)
            {
                mem = Item(i);
                _rVal.Append(mem.ToDXFEntities(aLayerName, aColor, aLinetype, aHClineScale, aVClineScale, aDClineScale, aImage, aLTLSetting, bSuppressInstances, aXMirrorOrd));
            }
            return _rVal;
        }

        public List<double> GetOrdinates( dxxOrdinateDescriptors aOrdType, List<string> aMemberNames = null)
        {
            List<double> _rVal = new List<double>();
            for(int i =1; i<=Count; i++)
            {
                UHOLES mems = Item(i);
                if(aMemberNames != null)
                {
                    if (aMemberNames.FindIndex((x) => string.Compare(x, mems.Name, true) == 0) < 0) continue;


                }
                for (int j = 1; j <= mems.Count; j++)
                {
                    UVECTOR u1 = mems.Centers.Item(j);

                    double ord = aOrdType switch { dxxOrdinateDescriptors.X => u1.X, dxxOrdinateDescriptors.Y => u1.Y, dxxOrdinateDescriptors.Z => u1.Elevation.HasValue? u1.Elevation.Value :mems.Member.Elevation, _ => 0 };
                    _rVal.Add(ord); 
                }

            }
            return _rVal;
        }

        #endregion Methods

        #region Shared Methods

        public static UHOLEARRAY Null => new UHOLEARRAY(false);

        #endregion Shared Methods
    }

    internal struct URECTANGLE : ICloneable
    {
        public double Left;
        public double Top;
        public double Right;
        public double Bottom;
        public string Tag;
        public string Flag;
        public int Index;
        public string Name;
        public bool Invalid;
        public int Row;
        public int Col;
        public bool IsVirtual;
        public double Rotation;
        public bool Suppressed;
        public bool Selected;
        #region Constructors


        public URECTANGLE( string aName = "")
        {
            Invalid = true;
            Left = 0;
            Top = 0;
            Right = 0;
            Bottom = 0;
            Tag = string.Empty;
            Flag = string.Empty;
            Index = 0;
            Name =  aName;
            Row = 0;
            Col = 0;
            IsVirtual = false;
            Rotation = 0;
            Suppressed = false;
            Selected = false;
        }
        public URECTANGLE(double aLeft = 0, double aTop = 0, double aRight = 0, double aBottom = 0, string aTag = "", string aName = "", string aFlag = "")
        {
            mzUtils.SortTwoValues(true, ref aLeft, ref aRight);
            mzUtils.SortTwoValues(true, ref aBottom, ref aTop);
            Invalid = true;
            Left = aLeft;
            Top = aTop;
            Right = aRight;
            Bottom = aBottom;
            Tag = aTag;
            Flag = aFlag;
            Index = 0;
            Name = aName;
            Row = 0;
            Col = 0;
            IsVirtual = false;
            Rotation = 0;
            Suppressed = false;
            Selected = false;
        }
        
        public URECTANGLE(URECTANGLE aRectangle)
        {

            Invalid = aRectangle.Invalid;
            Left = aRectangle.Left;
            Top = aRectangle.Top;
            Right = aRectangle.Right;
            Bottom = aRectangle.Bottom;
            Tag = aRectangle.Tag;
            Flag = aRectangle.Flag;
            Index = aRectangle.Index;
            Name = aRectangle.Name;
            Row = aRectangle.Row;
            Col = aRectangle.Col;
            IsVirtual = aRectangle.IsVirtual;
            Rotation = aRectangle.Rotation;
            Suppressed = aRectangle.Suppressed;
            Selected = aRectangle.Selected;
        }

        public URECTANGLE(iRectangle aRectangle)
        {
            Invalid = false;
            Left = 0;
            Top = 0;
            Right = 0;
            Bottom = 0;
            Tag = string.Empty;
            Flag = string.Empty;
            Index = 0;
            Name = string.Empty;
            Row = 0;
            Col = 0;
            IsVirtual = false;
            Rotation = 0;
            Suppressed = false;
            Selected = false;
            if (aRectangle == null) return;
            if (aRectangle is uopRectangle)
            { 
                uopRectangle urec = (uopRectangle)aRectangle;
                Invalid = urec.Invalid;
                Left = urec.Left;
                Top = urec.Top;
                Right = urec.Right;
                Bottom = urec.Bottom;
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
                dxfPlane plane = aRectangle.Plane;
                UVECTOR cp =  plane != null ? new UVECTOR(plane.Origin) : UVECTOR.Zero;
                Left = cp.X - 0.5 * aRectangle.Width;
                Right = cp.X + 0.5 * aRectangle.Width;
               Bottom = cp.Y - 0.5 * aRectangle.Height;
                Top = cp.Y + 0.5 * aRectangle.Height;
                if(aRectangle is dxfRectangle)
                {
                    dxfRectangle drec = (dxfRectangle)aRectangle;
                    Tag = drec.Tag;
                    Flag = drec.Flag;
                    Name = drec.Name;
                    Suppressed = drec.Suppressed;
                }
                
                Rectify();

            }
        
        }
        public URECTANGLE(uopShape aShape)
        {
            Invalid = false;
            Left = 0;
            Top = 0;
            Right = 0;
            Bottom = 0;
            Tag = String.Empty;Flag = string.Empty;
            Index = 0;
            Name = string.Empty;
            Row = 0;
            Col = 0;
            IsVirtual = false;
            Rotation = 0;
            Suppressed = false;
            Selected = false;
            if (aShape == null) return;
        
            uopRectangle urec =  new uopRectangle(aShape.BoundsV);
            Invalid = urec.Invalid;
            Left = urec.Left;
            Top = urec.Top;
            Right = urec.Right;
            Bottom = urec.Bottom;
            Tag = aShape.Tag;
            Flag = aShape.Flag;
            Index = aShape.Index;
            Name = aShape.Name;
            Row = aShape.Row;
            Col = aShape.Col;
            IsVirtual = aShape.IsVirtual;
            Suppressed = urec.Suppressed;
            Rectify();

        }

        public URECTANGLE(USHAPE aShape)
        {
           

            uopRectangle urec = new uopRectangle(aShape.Vertices.Bounds);
            Invalid = urec.Invalid;
            Left = urec.Left;
            Top = urec.Top;
            Right = urec.Right;
            Bottom = urec.Bottom;
            Tag = aShape.Tag;
            Flag = aShape.Flag;
            Index = aShape.Index;
            Name = aShape.Name;
            Row = aShape.Row;
            Col = aShape.Col;
            IsVirtual = aShape.IsVirtual;
            Rotation = 0;
            Suppressed = urec.Suppressed;
            Selected = false;
            Rectify();

        }

        public URECTANGLE(UVECTOR aCenter, double aWidth, double aHeight, double aRotation = 0)
        {
            Name = string.Empty;
            Tag = string.Empty;
            Flag = string.Empty;
            Index = 0;
            Row = 0;
            Col = 0;
            Invalid = false;
            IsVirtual = false;
            Left = aCenter.X - 0.5 * aWidth;
            Right = aCenter.X + 0.5 * aWidth;
           Bottom = aCenter.Y - 0.5 * aHeight;
            Top = aCenter.Y + 0.5 * aHeight;
            Rotation = aRotation;
            Suppressed = false;
            Selected = false;
            Rectify();
        }
        #endregion Constructors


        #region Properties

        public double Width { get { Rectify(); return Math.Abs(Right - Left); }
            set
            {
                UVECTOR ctr = Center;
                Left = ctr.X - 0.5 * value;
                Right = ctr.X + 0.5 * value;
                Rectify();
            }
                
        }

        public double Height
        {
            get { Rectify(); return Math.Abs(Top - Bottom); }
            set
            {
                UVECTOR ctr = Center;
                Bottom = ctr.Y - 0.5 * value;
                Top = ctr.Y + 0.5 * value;
                Rectify();
            }
        }

        public UVECTOR Center { get { Rectify(); return new UVECTOR(Left + (Right - Left) / 2, Bottom + (Top - Bottom) / 2); } }


        public UVECTOR TopLeft { get { Rectify(); return new UVECTOR(Left, Top); } }

        public UVECTOR TopRight { get { Rectify(); return new UVECTOR(Right, Top); } }

        public UVECTOR BottomLeft { get { Rectify(); return new UVECTOR(Left, Bottom); } }

        public UVECTOR BottomRight { get { Rectify(); return new UVECTOR(Right, Bottom); } }

        public ULINE LeftEdge => new ULINE(Left, Top, Left, Bottom) { Tag = "LEFT", Side = uppSides.Left };
        public ULINE BottomEdge => new ULINE(Left, Bottom, Right, Bottom) { Tag = "BOTTOM", Side = uppSides.Bottom };

        public ULINE RightEdge => new ULINE(Right, Bottom, Right, Top) { Tag = "RIGHT", Side = uppSides.Right };
        public ULINE TopEdge => new ULINE(Right, Top, Left, Top) { Tag = "TOP", Side = uppSides.Top };

        public bool IsNull { get => Left == Right && Top == Bottom; }

        public double Area { get { Rectify(); return (Right - Left) * (Top - Bottom); } }

        public UVECTORS Corners => new UVECTORS(TopLeft, BottomLeft, BottomRight, TopRight);

        public double X { get { Rectify(); return Left + Math.Abs(Right - Left) / 2; } }

        public double Y { get { Rectify(); return Bottom + Math.Abs(Top - Bottom) / 2; } }

      
        #endregion Properties

        #region Methods

        object ICloneable.Clone() => (object)Clone(null);

        public URECTANGLE Clone(string aName = "")
        {
            if (string.IsNullOrWhiteSpace(aName)) { aName = Name; }

            return new URECTANGLE(this) { Name = aName };

        }

        public dxfRectangle ToDXFRectangle(dxfDisplaySettings aDisplaySettings = null) => new dxfRectangle(Center.ToDXFVector(), Width, Height) { Tag = Tag, DisplaySettings = aDisplaySettings };

        public void Clear() { Right = 0; Left = 0; Top = 0; Bottom = 0; }

        public override string ToString()
        {
            return $"UREC[ L:{string.Format("{0:#,0.####}", Left)} R:{string.Format("{0:#,0.####}", Right)} T:{string.Format("{0:#,0.####}", Top)} B:{string.Format("{0:#,0.####}", Bottom)} ]";

        }

        public void Mirror(double? aX, double? aY)
        {
            if (!aX.HasValue && !aY.HasValue) return;
            double dx = aX.HasValue ? X - aX.Value : 0;
            double dy = aY.HasValue ? Y - aY.Value : 0;

            Left -= 2 * dx;
            Right -= 2 * dx;
            Top -= 2 * dy;
            Bottom -= 2 * dy;
            Rectify();


        }
        public void Move(double aDX = 0, double aDY = 0) 
        {
            Left += aDX; Right += aDX; Top += aDY; Bottom += aDY;
            Rectify();
        }

        public void Rectify()
        {
            double s1;

            if (Left > Right) { s1 = Left; Left = Right; Right = s1; }
            if (Bottom > Top) { s1 = Bottom; Bottom = Top; Top = s1; }
        }

        public ULINES Edges( bool bCounterClockwise = false)
        {
            ULINES _rVal = ULINES.Null;
            if (!bCounterClockwise)
            {
                _rVal.Add(Edge(uppSides.Left, false));
                _rVal.Add(Edge(uppSides.Top, false));
                _rVal.Add(Edge(uppSides.Right, false));
                _rVal.Add(Edge(uppSides.Bottom, false));
            }

            return _rVal;
        }

        public ULINE Edge(uppSides aSide, bool bCounterClockwise = false)
        {
            if (aSide == uppSides.Top)
            {
                return (!bCounterClockwise) ? new ULINE(TopLeft, TopRight) : new ULINE(TopRight, TopLeft); ;

            }
            else if (aSide == uppSides.Right)
            {
                return (!bCounterClockwise) ? new ULINE(TopRight, BottomRight) : new ULINE(BottomRight, TopRight);

            }
            else if (aSide == uppSides.Left)
            {
                return (!bCounterClockwise) ? new ULINE(BottomLeft, TopLeft) : new ULINE(TopLeft, BottomLeft);
            }
            else { return (!bCounterClockwise) ? new ULINE(BottomRight, BottomLeft) : new ULINE(BottomLeft, BottomRight); }
        }


        /// <summary>
        /// Rectangle to Polyline
        /// </summary>
        /// <param name="aTag"></param>
        /// <param name="aFlag"></param>
        /// <param name="aLayerName"></param>
        /// <param name="aColor"></param>
        /// <param name="aLinetype"></param>
        /// <returns></returns>
        public dxePolyline ToDXFPolyline(string aTag = null, string aFlag = "", string aLayerName = "", dxxColors aColor = dxxColors.Undefined, string aLinetype = "")
        {
            dxePolyline _rVal = ToDXFRectangle().Perimeter(false, aColor, 0, aLayerName, aLinetype);
            _rVal.Tag = mzUtils.ThisOrThat(aTag, Tag);
            _rVal.Closed = true;
            _rVal.Flag = aFlag;
            _rVal.Value = Area;
            return _rVal;
        }


        public void Define(dynamic aLeft = null, dynamic aRight = null, dynamic aTop = null, dynamic aBot = null, int aPrecis = -1)
        {

            Left = mzUtils.VarToDouble(aLeft, false, Left);
            Right = mzUtils.VarToDouble(aRight, false, Right);
            Top = mzUtils.VarToDouble(aTop, false, Top);
            Bottom = mzUtils.VarToDouble(aBot, false, Bottom);

            if (aPrecis >= 0)
            {
                aPrecis = mzUtils.LimitedValue(aPrecis, 0, 15);
                Left = Math.Round(Left, aPrecis);
                Right = Math.Round(Right, aPrecis);
                Top = Math.Round(Top, aPrecis);
                Bottom = Math.Round(Bottom, aPrecis);
            }
            Rectify();
        }

        public bool TrimWithArc(double aRadius)
        {
            aRadius = Math.Round(Math.Abs(aRadius), 4);
            Rectify();

            if (aRadius == 0 || (Width == 0 && Height == 0)) return false;


            UVECTOR tr = new UVECTOR(Right, Top);
            double rad1 = tr.Length();
            UVECTOR tl = new UVECTOR(Left, Top);
            double rad2 = tl.Length();
            UVECTOR br = new UVECTOR(Right, Bottom);
            double rad3 = br.Length();
            UVECTOR bl = new UVECTOR(Left, Bottom);
            double rad4 = bl.Length();

            double radsqrd = Math.Pow(aRadius, 2);

            if (rad1 >= aRadius && rad2 >= aRadius && rad3 >= aRadius && rad4 >= aRadius) return false;
            if (rad1 < aRadius && rad2 < aRadius && rad3 < aRadius && rad4 < aRadius) return false;
            if (rad1 < aRadius && rad2 < aRadius && rad3 >= aRadius && rad4 >= aRadius) return false;
            if (rad1 >= aRadius && rad2 >= aRadius && rad3 < aRadius && rad4 < aRadius) return false;
            bool _rVal = false;

            if (rad1 > aRadius)
            {
                tr.X = tr.X >= 0 ? Math.Sqrt(radsqrd - Math.Pow(tr.Y, 2)) : -Math.Sqrt(radsqrd - Math.Pow(tr.Y, 2));
                _rVal = true;
                //br.X = tr.X;
                //rad3 = aRadius;
            }

            if (rad2 > aRadius)
            {
                tl.X = tl.X >= 0 ? Math.Sqrt(radsqrd - Math.Pow(tl.Y, 2)) : -Math.Sqrt(radsqrd - Math.Pow(tl.Y, 2));
                _rVal = true;
                //bl.X = tl.X;
                //rad4 = aRadius;
            }

            if (rad3 > aRadius)
            {
                br.X = br.X >= 0 ? Math.Sqrt(radsqrd - Math.Pow(br.Y, 2)) : -Math.Sqrt(radsqrd - Math.Pow(br.Y, 2));
                _rVal = true;
                //tr.X = br.X;
                //rad1 = aRadius;
            }

            if (rad4 > aRadius)
            {
                bl.X = bl.X >= 0 ? Math.Sqrt(radsqrd - Math.Pow(bl.Y, 2)) : -Math.Sqrt(radsqrd - Math.Pow(bl.Y, 2));
                _rVal = true;
                //tl.X = bl.X;
                //rad2 = aRadius;
            }
            Left = Math.Max(tl.X, bl.X);
            Right = Math.Min(tr.X, br.X);
            //Top = Math.Max(tl.Y, tr.Y);
            //Bottom = Math.Min(bl.Y, br.Y);

            return _rVal;


        }


        public USHAPE ToShape(string aTag = null)
        {
            USHAPE _rVal = new USHAPE(Name);
            Rectify();
            _rVal.Tag = Tag;
            _rVal.Vertices = Corners;
            _rVal.Update();
            if (aTag != null) _rVal.Tag = aTag;
            return _rVal;
        }


        public bool ContainsOrd(double aOrd, bool bOrdIsY, bool bOnIsIn, int aPrecis = 5) => ContainsOrd(aOrd, bOrdIsY, bOnIsIn, aPrecis, out bool ISON);

        public bool ContainsOrd(double aOrd, bool bOrdIsY, bool bOnIsIn, int aPrecis, out bool rIsOn)
        {

            Rectify();
            rIsOn = false;
            aPrecis = mzUtils.LimitedValue(aPrecis, 5, 15);
            double min = Left;
            double max = Right;
            if (bOrdIsY) { min = Bottom; max = Top; }
            if (mzUtils.CompareVal(aOrd, max, aPrecis) == mzEqualities.Equals)
            { rIsOn = true; return bOnIsIn; }
            if (mzUtils.CompareVal(aOrd, min, aPrecis) == mzEqualities.Equals)
            { rIsOn = true; return bOnIsIn; }
            return mzUtils.CompareVal(aOrd, min, aPrecis) == mzEqualities.GreaterThan && mzUtils.CompareVal(aOrd, max, aPrecis) == mzEqualities.LessThan;

        }

        public bool Contains(iVector aVector, bool bOnIsOut = false, bool bHorizontalOnly = false, int aPrecis = 4) => aVector != null && Contains(new UVECTOR(aVector.X, aVector.Y), bOnIsOut, bHorizontalOnly, aPrecis);

        internal bool Contains(double aX, double aY, bool bOnIsOut = false, bool bHorizontalOnly = false, int aPrecis = 4) => Contains(new UVECTOR(X, Y), bOnIsOut, bHorizontalOnly,aPrecis);

        public bool Contains(UVECTOR aVector, bool bOnIsOut = false, bool bHorizontalOnly = false, int aPrecis = 4)
        {
            Rectify();
            aPrecis = mzUtils.LimitedValue(aPrecis, 0, 15, 4);
            bool _rVal = false;
            if (!bOnIsOut)
            {
                if (Math.Round(aVector.X,aPrecis) >= Math.Round(Left, aPrecis) && Math.Round(aVector.X, aPrecis) <= Math.Round(Right, aPrecis))
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

            return _rVal;
        }

        public bool Contains(uopVector aVector, bool bOnIsOut = false, bool bHorizontalOnly = false, int aPrecis = 4)
        {
            Rectify();
            bool _rVal = false;
            if (aVector == null) return false;
            aPrecis = mzUtils.LimitedValue(aPrecis, 0, 15, 4);
            if(Rotation == 0)
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

            }



                return _rVal;
        }



        public bool SpansY(double aYVal, double aBuffer = 0, bool bOnIsIn = true)
        {
            if (bOnIsIn)
            {
                return aYVal <= Top + aBuffer && aYVal >= Bottom - aBuffer;
            }
            else
            {
                return aYVal < Top + aBuffer && aYVal > Bottom - aBuffer;
            }
        }

        public bool SpansX(double aXVal, double aBuffer = 0, bool bOnIsIn = true)
        {
            if (bOnIsIn)
            {
                return aXVal <= Right + aBuffer && aXVal >= Left - aBuffer;
            }
            else
            {
                return aXVal < Right + aBuffer && aXVal > Left - aBuffer;
            }
        }

        public bool ExpandTo(uopRectangle bRectangle)
        {
            
            if (bRectangle == null) return false;
            bool _rVal = false;
   
            if (IsNull)
            {
                _rVal = bRectangle.Left != Left || bRectangle.Right != Right || bRectangle.Top != Top || bRectangle.Bottom != Bottom;
                Define(bRectangle.Left, bRectangle.Right, bRectangle.Top, bRectangle.Bottom);
            }

            if (Update(bRectangle.Left, bRectangle.Top)) { _rVal = true; }

            if (Update(bRectangle.Right, bRectangle.Bottom)) { _rVal = true; }


            return _rVal;
        }

        public bool ExpandTo(URECTANGLE bRectangle)
        {
            bool _rVal = false;
            bRectangle.Rectify();
            if (IsNull)
            {
                _rVal = bRectangle.Left != Left || bRectangle.Right != Right || bRectangle.Top != Top || bRectangle.Bottom != Bottom;
                Define(bRectangle.Left, bRectangle.Right, bRectangle.Top, bRectangle.Bottom);
            }

            if (Update(bRectangle.Left, bRectangle.Top)) { _rVal = true; }

            if (Update(bRectangle.Right, bRectangle.Bottom)) { _rVal = true; }


            return _rVal;
        }

        public void Translate(double DX, Double DY) { Left += DX; Right += DX; Bottom += DY; Top += DY; }

        public bool Update(UVECTOR aVector, out bool rTopUpdated, out bool rBotUpdated, out bool rLeftUpdated, out bool rRightUpdated, int aPrecis = -1)
        {
            rTopUpdated = false;
            rBotUpdated = false;
            rLeftUpdated = false;
            rRightUpdated = false;

            return Update(aVector.X, aVector.Y, out rTopUpdated, out rBotUpdated, out rLeftUpdated, out rRightUpdated, aPrecis);
        }

        public bool Update(UVECTOR aVector, int aPrecis = -1) => Update(aVector.X, aVector.Y, out bool _, out bool _, out bool _, out bool _, aPrecis);


        public bool Update(double aXValue, double aYValue, int aPrecis = -1) => Update(aXValue, aYValue, out bool _, out bool _, out bool _, out bool _, aPrecis);


        public bool Update(double aXValue, double aYValue, out bool rTopUpdated, out bool rBotUpdated, out bool rLeftUpdated, out bool rRightUpdated, int aPrecis = -1)
        {

            //    Rectify();
            rTopUpdated = false;
            rBotUpdated = false;
            rLeftUpdated = false;
            rRightUpdated = false;

            if (aXValue < Left)
            {
                rLeftUpdated = true;
                Left = aXValue;
            }
            if (aXValue > Right)
            {
                rRightUpdated = true;
                Right = aXValue;
            }
            if (aYValue < Bottom)
            {
                rBotUpdated = true;
                Bottom = aYValue;
            }
            if (aYValue > Top)
            {
                rTopUpdated = true;
                Top = aYValue;
            }
            return rLeftUpdated || rRightUpdated || rBotUpdated || rTopUpdated;
        }


        /// <summary>
        /// Streatch the rectangle
        /// </summary>
        /// <param name="aRectangle"></param>
        /// <param name="aAdder"></param>
        /// <param name="bSuppressWidth"></param>
        /// <param name="bSuppressHeight"></param>        
        public void Stretch(double aAdder, bool bSuppressWidth = false, bool bSuppressHeight = false)
        {

            double wda = (!bSuppressWidth) ? aAdder : 0;
            double hta = (!bSuppressWidth) ? aAdder : 0;

            Stretch(wda, hta);

        }

        public void Stretch(double aWidthAdder, double aHeightAdder)
        {
            Rectify();

            if (aWidthAdder == 0 && aWidthAdder == 0) return;


            if (aWidthAdder != 0)
            {
                Left -= aWidthAdder;
                Right += aWidthAdder;
            }
            if (aHeightAdder != 0)
            {
                Bottom -= aHeightAdder;
                Top += aHeightAdder;
            }
            Rectify();

        }


        public bool CompareDimensions(URECTANGLE bRectangle, bool bSuppressWidth = false, bool bSuppressHeight = false, int aPrecis = 3)
        {
            bool _rVal = true;

            aPrecis = mzUtils.LimitedValue(aPrecis, 0, 10);
            Rectify();
            bRectangle.Rectify();
            if (!bSuppressWidth)
            {
                double d1 = Right - Left;
                double d2 = bRectangle.Right - bRectangle.Left;

                if (Math.Round(d1 - d2, aPrecis) != 0) { _rVal = false; }
            }
            if (_rVal && !bSuppressHeight)
            {
                double d1 = Top - Bottom;
                double d2 = bRectangle.Top - bRectangle.Bottom;

                if (Math.Round(d1 - d2, aPrecis) != 0) { _rVal = false; }
            }
            return _rVal;
        }

        #endregion  Methods

        #region Shared Methods

        public static URECTANGLE Null => new URECTANGLE("");

        internal static URECTANGLE FromDXFRectangle(dxfRectangle aRectangle)
        {
            return (aRectangle != null) ? new URECTANGLE(aRectangle.Left, aRectangle.Top, aRectangle.Right, aRectangle.Bottom, aTag:aRectangle.Tag, aName:aRectangle.Name, aFlag: aRectangle.Flag) : URECTANGLE.Null;
        }


        public static dxePolyline Perimeter(URECTANGLE aRectangle, double aBuffer = 0, string aLayerName = "", dxxColors aColor = dxxColors.Undefined, string aLinetype = "")
        {

            colDXFVectors verts = new colDXFVectors
            {
                { aRectangle.Left - aBuffer, aRectangle.Top + aBuffer },
                { aRectangle.Left - aBuffer, aRectangle.Bottom - aBuffer },
                { aRectangle.Right + aBuffer, aRectangle.Bottom - aBuffer },
                { aRectangle.Right + aBuffer, aRectangle.Top + aBuffer }
            };

            return new dxePolyline(verts, bClosed: true, aDisplaySettings: new dxfDisplaySettings(aLayerName, aColor, aLinetype)) { Tag = aRectangle.Tag };

        }
    
        #endregion Shared Methods

    }

    internal struct URECTANGLES : ICloneable
    {
        private bool _Init;
        private int _Count;
        private URECTANGLE[] _Members;
        public string Name;

        public URECTANGLES(string aName = "")
        {

            Name = aName;
            _Members = new URECTANGLE[0];
            _Count = 0;
            _Init = true;
        }

        public URECTANGLES(URECTANGLES aRectangles, bool bClear = false)
        {

            Name = aRectangles.Name;
            _Members = new URECTANGLE[0];
            _Count = bClear ? 0 : aRectangles.Count;
            _Init = true;
            if (_Count > 0)
            {
                _Members = Force.DeepCloner.DeepClonerExtensions.DeepClone<URECTANGLE[]>(aRectangles._Members);


            }
        }

        private int Init()
        {
            Name = string.Empty;
            _Members = new URECTANGLE[0];
            _Count = 0;
            _Init = true;
            return 0;
        }

        public int Count => (!_Init) ? Init() : _Count;

        public URECTANGLES Clone(bool bReturnEmpty = false) => new URECTANGLES(this, bReturnEmpty);

        public void Clear() { _Count = 0; _Members = new URECTANGLE[0]; _Init = true; }
        /// <summary>
        ///returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>

        public URECTANGLE Item(int aIndex) { return (aIndex < 1 || aIndex > Count) ? new URECTANGLE() : _Members[aIndex - 1]; }


        public void SetItem(int aIndex, URECTANGLE aRectang)
        {
            if (aIndex < 1 || aIndex > Count) { return; }
            aRectang.Index = aIndex;
            _Members[aIndex - 1] = aRectang;
        }


        public URECTANGLE Add(URECTANGLE aRectang)
        {
            if (Count + 1 > Int32.MaxValue) { return aRectang; }

            _Count += 1;
            Array.Resize<URECTANGLE>(ref _Members, _Count);
            _Members[_Count - 1] = aRectang;
            _Members[_Count - 1].Index = _Count;
            return Item(Count);

        }
        public int ContainsVector(uopVector aVector, bool bOnIsOut = false, bool bHorizontalOnly = false) => aVector == null ? 0 : ContainsVector(new UVECTOR(aVector), bOnIsOut, bHorizontalOnly);
        public int ContainsVector(UVECTOR aVector, bool bOnIsOut = false, bool bHorizontalOnly = false)
        {
            URECTANGLE aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                if (aMem.Contains(aVector, bOnIsOut, bHorizontalOnly)) return i;
            }
            return 0;
        }

        object ICloneable.Clone() => (object)Clone(false);

        public override string ToString() => string.IsNullOrWhiteSpace(Name) ? "URECTANGLES[" + Count + "]" : "URECTANGLES '" + Name + "' [" + Count + "]";
    }

    internal struct TDETAIL : ICloneable
    {
        public string Name;
        public string FileName;
        public string BlockName;
        public string Description;
        public string Title;
        public string GIFFile_English;
        public string GIFFile_Metric;
        public TATTRIBUTES Attributes;
        public int Index;
        public TDETAIL(string aName = "")
        {
            Name = aName;
            FileName = string.Empty;
            BlockName = string.Empty;
            Title = string.Empty;
            Description = string.Empty;
            GIFFile_English = string.Empty;
            GIFFile_Metric = string.Empty;
            Attributes = new TATTRIBUTES();
            Index = 0;
        }
        /// <summary>
        /// returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>
        /// 
        public TDETAIL Clone()
        {
            return new TDETAIL(Name)
            {
                FileName = FileName,
                BlockName = BlockName,
                Title = Title,
                Description = Description,
                GIFFile_English = GIFFile_English,
                GIFFile_Metric = GIFFile_Metric,
                Attributes = Attributes.Clone(),
                Index = Index
            };

        }

        object ICloneable.Clone() => (object)Clone();
    }

    internal struct TDETAILS : ICloneable
    {
        private int _Count;
        private TDETAIL[] _Members;
        private bool _Init;
        public string Name;
        public TDETAILS(string aName = "") { _Members = new TDETAIL[0]; _Init = true; _Count = 0; Name = aName; }

        private void Init() { _Members = new TDETAIL[0]; _Init = true; _Count = 0; Name = string.Empty; }

        public int Count { get { if (!_Init) { Init(); } return _Count; } }


        object ICloneable.Clone() => (object)Clone();

        public TDETAILS Clone(bool bReturnEmpty = false)
        {
            if (!_Init) { Init(); }
            TDETAILS _rVal = new TDETAILS(Name);
            if (!bReturnEmpty) { for (int i = 1; i <= Count; i++) { _rVal.Add(Item(i).Clone()); } }
            return _rVal;
        }

        public void Clear() { _Count = 0; _Members = new TDETAIL[0]; }

        public TDETAIL Item(int aIndex) { if (aIndex < 1 || aIndex > Count) { return new TDETAIL(); } else { _Members[aIndex - 1].Index = aIndex; return _Members[aIndex - 1]; } }

        public TDETAIL Add(TDETAIL aAttr)
        {
            if (Count + 1 > Int32.MaxValue) { return aAttr; }

            _Count += 1;
            Array.Resize<TDETAIL>(ref _Members, _Count);
            _Members[_Count - 1] = aAttr.Clone();
            _Members[_Count - 1].Index = _Count;
            return Item(Count);

        }


        public TDETAIL Item(string aName)
        {
            TDETAIL mem;
            for (int i = 1; i <= Count; i++) { mem = Item(i); if (string.Compare(mem.Name, aName, ignoreCase: true) == 0) { return mem; } }
            return new TDETAIL("");
        }
        public void SetItem(int aIndex, TDETAIL aAttr) { if (aIndex < 1 || aIndex > Count) { return; } _Members[aIndex - 1] = aAttr; }


    }

    internal struct TATTRIBUTE : ICloneable
    {
        public string Tag;
        public string EnglishValue;
        public string MetricValue;
        public int Index;

        public TATTRIBUTE(string aTag = "") { Tag = aTag; EnglishValue = string.Empty; MetricValue = string.Empty; Index = 0; }
        /// <summary>
        /// returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>
        /// 
        public TATTRIBUTE Clone()
        {
            return new TATTRIBUTE() { Tag = Tag, EnglishValue = EnglishValue, MetricValue = MetricValue, Index = Index };
        }

        object ICloneable.Clone() => (object)Clone();
    }

    internal struct TATTRIBUTES : ICloneable
    {
        private int _Count;
        private TATTRIBUTE[] _Members;
        private bool _Init;
        public string Name;
        public TATTRIBUTES(string aName = "") { _Members = new TATTRIBUTE[0]; _Init = true; _Count = 0; Name = aName; }

        private void Init() { _Members = new TATTRIBUTE[0]; _Init = true; _Count = 0; Name = string.Empty; }

        public int Count { get { if (!_Init) { Init(); } return _Count; } }


        object ICloneable.Clone() => (object)Clone();

        public TATTRIBUTES Clone(bool bReturnEmpty = false)
        {
            if (!_Init) { Init(); }
            TATTRIBUTES _rVal = new TATTRIBUTES(Name);
            if (!bReturnEmpty) { for (int i = 1; i <= Count; i++) { _rVal.Add(Item(i).Clone()); } }
            return _rVal;
        }

        public void Clear() { _Count = 0; _Members = new TATTRIBUTE[0]; }

        public TATTRIBUTE Item(int aIndex) { if (aIndex < 1 || aIndex > Count) { return new TATTRIBUTE(); } else { _Members[aIndex - 1].Index = aIndex; return _Members[aIndex - 1]; } }

        public TATTRIBUTE Add(TATTRIBUTE aAttr)
        {
            if (Count + 1 > Int32.MaxValue) { return aAttr; }

            _Count += 1;
            Array.Resize<TATTRIBUTE>(ref _Members, _Count);
            _Members[_Count - 1] = aAttr.Clone();
            _Members[_Count - 1].Index = _Count;
            return Item(Count);

        }


        public TATTRIBUTE Item(string aTag)
        {
            TATTRIBUTE mem;
            for (int i = 1; i <= Count; i++) { mem = Item(i); if (string.Compare(mem.Tag, aTag, ignoreCase: true) == 0) { return mem; } }
            return new TATTRIBUTE("");
        }
        public void SetItem(int aIndex, TATTRIBUTE aAttr) { if (aIndex < 1 || aIndex > Count) { return; } _Members[aIndex - 1] = aAttr; }


    }


    internal struct USEGMENT : ICloneable
    {
        #region Fields
        public bool IsArc;
        public ULINE LineSeg;
        public UARC ArcSeg;
        public string Tag;
        public dynamic Value;
        private int _Index;

        #endregion Fields

        #region Constructors

        public USEGMENT(string aTag = "")
        {
            IsArc = false;
            LineSeg = ULINE.Null;
            ArcSeg = new UARC();
            Tag = aTag;
            Value = null;
            _Index = 0;
        }

        public USEGMENT(USEGMENT aSegment)
        {
            IsArc = aSegment.IsArc;
            LineSeg = new ULINE(aSegment.LineSeg);
            ArcSeg = new UARC(aSegment.ArcSeg);
            Tag = aSegment.Tag;
            Value = aSegment.Value;
            _Index = aSegment._Index;
        }

        public USEGMENT(ULINE aLine, string aTag = null)
        {
            IsArc = false;
            LineSeg = aLine;
            ArcSeg = new UARC();
            Tag = aTag != null ? aTag : "";
            Value = null;
            _Index = 0;
        }

        public USEGMENT(UARC aArc, string aTag = null)
        {
            IsArc = true;
            LineSeg = ULINE.Null;
            ArcSeg = aArc;
            Tag = aTag != null ? aTag : "";
            Value = null;
            _Index = 0;
        }

        public USEGMENT(uopLine aLine, string aTag = null)
        {
            IsArc = false;
            LineSeg = new ULINE(aLine);
            ArcSeg = new UARC();
            Tag = aTag != null ? aTag : "";
            Value = null;
            _Index = 0;



        }

        public USEGMENT(uopArc aArc, string aTag = null)
        {
            IsArc = true;
            LineSeg = ULINE.Null;
            ArcSeg = new UARC(aArc);
            Tag = aTag != null ? aTag : "";
            Value = null;
            _Index = 0;
        }

        public USEGMENT(iSegment aSegment, string aTag = null)
        {
            IsArc = aSegment.IsArc;
            LineSeg = aSegment.IsArc ? ULINE.Null : new ULINE(aSegment.Line);
            ArcSeg = aSegment.IsArc ? new UARC(aSegment.Arc) : new UARC(0);
            Tag = aTag != null ? aTag : "";
            Value = null;
            _Index = 0;
        }


        #endregion Constructors

        #region Properties

        public int Index
        {
            get => _Index;
            set
            {
                _Index = value;
                LineSeg.Index = value;
                ArcSeg.Index = value;
            }
        }

        #endregion Properties

        #region Methods

        public override string ToString()
        {
            return IsArc ? ArcSeg.ToString() : LineSeg.ToString();
        }

        object ICloneable.Clone() => (object)Clone();

        public USEGMENT Clone() => new USEGMENT(this);


        public dxfEntity ToDXFEntity() => IsArc ? (dxfEntity)ArcSeg.ToDXFArc() : (dxfEntity)LineSeg.ToDXFLine();

        public dxfEntity ToDXFEntityEX(string aLayerName = "", dxxColors aColor = dxxColors.Undefined, string aLinetype = "")
        {
            if (IsArc)
            { return ArcSeg.ToDXFArcEX(null, aLayerName, aColor, aLinetype); }
            else
            { return LineSeg.ToDXFLineEX(null, aLayerName, aColor, aLinetype); }
        }


        public UVECTORS Intersections(USEGMENT bSegment, bool aSegInfinite = false, bool bSegInfinite = false)
        {
            UVECTORS _rVal = UVECTORS.Zero;
            UVECTOR u1 = UVECTOR.Zero;
            bool aFlg = false;
            bool bFlg = false;
            bool cFlg = false;
            double t1 = 0;
            double t2 = 0;

            if (!IsArc && !bSegment.IsArc)
            {
                //two lines
                u1 = LineSeg.IntersectionPt(bSegment.LineSeg, out bool PRL, out bool COINC, out aFlg, out bFlg, out cFlg, out t1, out t2);
                if (cFlg)
                {
                    if (aSegInfinite && bSegInfinite)
                    { _rVal.Add(u1); }
                    else
                    {
                        if (!aSegInfinite && !bSegInfinite)
                        {
                            if (aFlg && bFlg) _rVal.Add(u1);
                        }
                        else if (!aSegInfinite && bSegInfinite)
                        {
                            if (aFlg) _rVal.Add(u1);
                        }
                        else if (aSegInfinite && !bSegInfinite)
                        {
                            if (bFlg) _rVal.Add(u1);
                        }
                    }
                }
            }
            else if (!IsArc && bSegment.IsArc)
            {
                //line /arc
                _rVal.Append(LineSeg.Intersections(bSegment.ArcSeg, bSegInfinite, aSegInfinite));
            }
            else if (IsArc && !bSegment.IsArc)
            {
                //arc/line
                _rVal.Append(bSegment.LineSeg.Intersections(ArcSeg, aSegInfinite, bSegInfinite));
            }
            else
            {
                //arc/arc
            }
            return _rVal;
        }

        /// <summary>
        /// returns true if the passed vector lies on the segment
        /// </summary>
        /// <param name="aVector" the vector to test></param>
        /// <param name="aPrecis" a precision apply></param>
        /// <param name="bTreatAsInfinite" flag to treat the segment as  infinite></param>
        /// <returns></returns>
        public bool ContainsVector(UVECTOR aVector, int aPrecis = 5, bool bTreatAsInfinite = false)
        { return ContainsVector(aVector, aPrecis, out bool _, out bool _, out bool _, bTreatAsInfinite); }

        /// <summary>
        /// returns true if the passed vector lies on the segment
        /// </summary>
        /// <param name="aVector" the vector to test></param>
        /// <param name="aPrecis" a precision apply></param>
        /// <param name="rIsStartPt" returns true if the passed vector is the start vector of the segment></param>
        /// <param name="rIsEndPt" returns true if the passed vector is the end vector of the segment></param>
        /// <param name="rWithin" returns true if the passed vector is within the segment></param>
        /// <param name="bTreatAsInfinite" flag to treat the segment as  infinite></param>
        public bool ContainsVector(UVECTOR aVector, int aPrecis, out bool rIsStartPt, out bool rIsEndPt, out bool rWithin, bool bTreatAsInfinite = false)
        {
            if (IsArc)
            {
                return UARC.ArcContainsVector(ArcSeg.Center, ArcSeg.Radius, ArcSeg.StartAngle, ArcSeg.EndAngle, aVector, out rIsStartPt, out rIsEndPt, out rWithin, bTreatAsInfinite, aPrecis);
            }
            else
            {
                return LineSeg.ContainsVector(aVector, aPrecis, out rIsStartPt, out rIsEndPt, out rWithin, bTreatAsInfinite);
            }
        }

       

        #endregion Methods


        #region Shared Methods

        internal static USEGMENT FromArc(dxeArc aArc)
        {
            USEGMENT _rVal = new USEGMENT { IsArc = true };

            if (aArc == null) return _rVal;
            dxeArc bArc = aArc.ClockWise ? aArc.Inverse() : aArc;

            _rVal.ArcSeg = new UARC(new UVECTOR(bArc.Center), aArc.Radius, bArc.StartAngle, bArc.EndAngle);
            return _rVal;
        }

        #endregion
    }
    internal struct USHAPE : ICloneable
    {

        #region Fields

        public UVECTORS Vertices;
        public USEGMENTS Segments;
        public ULINEPAIR LinePair;
        public double Radius;
        public string Tag;
        public double Value;
        public double Elevation;
        public double Depth { get; set; }
        public string Flag;
        public bool Mark;
        public string Name;
        public double Factor;
        public bool Open;
        public bool IsCircle;
        public int Index;
        public int PartIndex;
        public string Handle;
        public bool IsVirtual;
        public int Row;
        public int Col;
        public bool Suppressed;
        #endregion Fields

        #region Constructors

        public USHAPE(string aName = "", string aTag = "")
        {
            Vertices = UVECTORS.Zero;
            Segments = new USEGMENTS("");
            LinePair = ULINEPAIR.Null;
            Radius = 0;
            Tag = aTag != null ? aTag : "";
            Value = 0;
            Elevation = 0;
            Depth = 0;
            Flag = string.Empty;
            Name = aName != null ? aName : "";
            Factor = 1;
            Open = false;
            IsCircle = false;
            Index = 0;
            Handle = string.Empty;
            PartIndex = 0;
            IsVirtual = false;
            Row = 0;
            Col = 0;
            Mark = false;
            Suppressed = false;
        }
        internal USHAPE(USHAPE aShape)
        {


            Vertices = new UVECTORS(aShape.Vertices);
            Segments = new USEGMENTS(aShape.Segments);
            LinePair = new ULINEPAIR(aShape.LinePair);
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
            Mark = aShape.Mark;
            Suppressed = aShape.Suppressed;
            Update();
        }

        internal USHAPE(UARCREC aShape, string aName = "")
        {
            Vertices = UVECTORS.Zero;
            Segments = new USEGMENTS("");
            LinePair = ULINEPAIR.Null;
            Radius = 0;
            Tag = aShape.Tag; 
            Value = 0;
            Elevation = 0;
            Depth = 0;
            Flag = aShape.Flag;
            Name = aName != null ? aName : "";
            Factor = 1;
            Open = false;
            IsCircle = aShape.IsArc && aShape.Radius >0;
            Index = 0;
            Handle = string.Empty;
            PartIndex = 0;
            IsVirtual = false;
            Row = 0;
            Col = 0;
            Mark = false;
            Suppressed = aShape.Suppressed;
            if (IsCircle)
            {                
                Vertices.Add(aShape.X + Radius, aShape.Y, aRadius: Radius);
                Vertices.Add(aShape.X, aShape.Y + Radius, aRadius: Radius);
                Vertices.Add(aShape.X - Radius, aShape.Y, aRadius: Radius);
                Vertices.Add(aShape.X, aShape.Center.Y - Radius, aRadius: Radius);

            }
            else
            {
                Vertices = aShape.Corners;
            }

                Radius = aShape.Radius;
        
            Update();
        }

        public USHAPE(uopShape aShape)
        {
            Vertices = UVECTORS.Zero;
            Segments = new USEGMENTS("");
            LinePair = ULINEPAIR.Null;
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
            Mark = false;
            Suppressed = false;
            if (aShape == null) return;

            Vertices = new UVECTORS(aShape.Vertices);
            Segments = new USEGMENTS(aShape.Segments);

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
            Mark = aShape.Mark;
            Suppressed = aShape.Suppressed;
            Update();
        }
        internal USHAPE(UVECTORS aVertices, string aName = "", bool bIsVirtual = false)
        {
            Vertices = new UVECTORS(aVertices);
            Segments = new USEGMENTS();
            LinePair = ULINEPAIR.Null;
            Radius = 0;
            Tag = string.Empty;
            Value = 0;
            Elevation = 0;
            Depth = 0;
            Flag = string.Empty;
            Name = aName ?? "";
            Factor = 1;
            Open = false;
            IsCircle = false;
            Index = 0;
            Handle = string.Empty;
            PartIndex = 0;
            IsVirtual = bIsVirtual;
            Row = 0;
            Col = 0;
            Mark = false;
            Suppressed = false;
            Update();


        }
        internal USHAPE(IEnumerable<iVector> aVertices, string aName = "", bool bIsVirtual = false)
        {
            Vertices = new UVECTORS(aVertices);
            Segments = new USEGMENTS();
            LinePair = ULINEPAIR.Null;
            Radius = 0;
            Tag = string.Empty;
            Value = 0;
            Elevation = 0;
            Depth = 0;
            Flag = string.Empty;
            Name = aName ?? "";
            Factor = 1;
            Open = false;
            IsCircle = false;
            Index = 0;
            Handle = string.Empty;
            PartIndex = 0;
            IsVirtual = bIsVirtual;
            Row = 0;
            Col = 0;
            Mark = false;
            Suppressed = false;
            Update();


        }

        /// <summary>
        /// Creates a new shape that is a circle section
        /// </summary>
        /// <param name="aCenterX"></param>
        /// <param name="aCenterY"></param>
        /// <param name="aRadius"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLeftEdge"></param>
        /// <param name="aRightEdge"></param>
        /// <param name="aTopEdge"></param>
        /// <param name="aBottomEdge"></param>
        /// <param name="sClipMoon"></param>
        /// <param name="aName"></param>
        /// <param name="aTag"></param>
        public USHAPE(double aCenterX, double aCenterY, double aRadius, double aRotation = 0, double? aLeftEdge = null, double? aRightEdge = null, double? aTopEdge = null, double? aBottomEdge = null, string aName = "", string aTag = "", bool bIsVirtual = false)
        {
            Vertices = USHAPE.CircleSectionVertices(aCenterX, aCenterY, aRadius, aRotation, aLeftEdge, aRightEdge, aTopEdge, aBottomEdge);
            Segments = new USEGMENTS("");
            LinePair = ULINEPAIR.Null;
            Radius = 0;
            Tag = aTag != null ? aTag : "";
            Value = 0;
            Elevation = 0;
            Depth = 0;
            Flag = string.Empty;
            Name = aName != null ? aName : string.Empty;
            Factor = 1;
            Open = false;
            IsCircle = false;
            Index = 0;
            Handle = string.Empty;
            PartIndex = 0;
            IsVirtual = bIsVirtual;
            Row = 0;
            Col = 0;
            Mark = false;
            Suppressed = false;
            Update();
        }
        #endregion Constructors

        #region Properties

        public URECTANGLE Limits => Vertices.Bounds;

        public double Top => Limits.Top;
        
        public double Bottom=> Limits.Bottom;
 
        public double Left=> Limits.Left;
        public double Right => Limits.Right;


        public double Width => Limits.Width;

        public double Height => Limits.Height;

        public UVECTOR Center => Limits.Center;

        public double X => Center.X;

        public double Y => Center.Y;


        public bool IsDefined { get { bool _rVal = Vertices.Count > 1; if (_rVal && Segments.Count < Vertices.Count - 1) Update(); return _rVal; } }


        public double Area
        {
            get
            {
                double _rVal = 0;


                if (IsCircle)
                {
                    _rVal = Math.PI * Math.Pow(Radius, 2);
                }
                else
                {

                    if (Segments.Count <= 0) Update();
                    if (Segments.Count <= 0) return _rVal;
                    USEGMENTS aSegs = Segments;
                    _rVal = Vertices.Area();

                    for (int i = 1; i <= aSegs.Count; i++)
                    {
                        USEGMENT aSeg = aSegs.Item(i);
                        if (aSeg.IsArc)
                        {
                            //arcs are assume to be covex from the shape
                            _rVal += uopUtils.ArcArea(aSeg.ArcSeg.Radius, aSeg.ArcSeg.StartAngle, aSeg.ArcSeg.EndAngle);
                        }
                    }
                }
                return _rVal;
            }
        }

        public bool HasArcs

        {
            get
            {
                for (int i = 1; i <= Vertices.Count; i++)
                {
                    if (Vertices.Item(i).Radius != 0)
                    { return true; }
                }
                return false;

            }
        }


        public uopVectors Vertexes
        {
            get => new uopVectors(Vertices);

            set
            {
                Vertices.Clear();
                if (value != null)
                {
                    foreach (var item in value)
                    {
                        Vertices.Add(new UVECTOR(item));
                    }
                }
                Update();
            }
        }

        #endregion Properties

        #region Methods
        public USEGMENTS UpdateSegments() { Segments = Vertices.ToSegments(Open); return Segments; }


        object ICloneable.Clone() => (object)Clone();

        public USHAPE Clone() => new USHAPE(this);

        public override string ToString() => string.IsNullOrWhiteSpace(Name) ? $"USHAPE[{Vertices.Count}]" : $"USHAPE[{Vertices.Count}] '{Name}'";

        public void Translate(double? dX = null, double? dY = null)
        {
            if (!dX.HasValue) dX = 0;
            if (!dY.HasValue) dY = 0;

            if (dX.Value == 0 && dY.Value == 0) return;
            for (int i = 1; i <= Vertices.Count; i++)
            {
                UVECTOR u1 = Vertices.Item(i);
                u1.X += dX.Value;
                u1.Y += dY.Value;
                Vertices.SetItem(i, u1);
            }

            for (int i = 1; i <= Segments.Count; i++)
            {
                USEGMENT seg = Segments.Item(i);
                seg.ArcSeg.Center.X += dX.Value;
                seg.ArcSeg.Center.Y += dY.Value;
                seg.LineSeg.sp.X += dX.Value;
                seg.LineSeg.sp.Y += dY.Value;
                seg.LineSeg.ep.X += dX.Value;
                seg.LineSeg.ep.Y += dY.Value;
                Segments.SetItem(i, seg);
            }

        }

        public void Update() { UpdateSegments(); Vertices.UpdateBounds(); }

        /// <summary>
        /// Move poly line from one to other
        /// </summary>
        /// <param name="aShape"></param>
        /// <param name="aLayerName"></param>
        /// <param name="aColor"></param>
        /// <param name="aLinetype"></param>
        /// <param name="aTag"></param>
        /// <param name="aFlag"></param>
        /// <returns></returns>
        public dxePolyline ToDXFPolyline(string aLayerName = "", dxxColors aColor = dxxColors.Undefined, string aLinetype = "", dynamic aTag = null, dynamic aFlag = null)
        {
            colDXFVectors verts = Vertices.ToDXFVectors();

            dxePolyline _rVal = new dxePolyline(verts, !Open);
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

        public dxePolyline ToDXFPolyline(dxfDisplaySettings aDisplaySettings, string aTag = null, string aFlag = null)
        {
            colDXFVectors verts = Vertices.ToDXFVectors();

            dxePolyline _rVal = new dxePolyline(verts, !Open, aDisplaySettings);

            if (aTag == null)
            {
                aTag = Tag;
                if (aTag ==  string.Empty) { aTag = Name; }
            }


            _rVal.TFVSet(aTag, aFlag, Value);
            if (Elevation != 0) { _rVal.Move(0, 0, Elevation); }

            return _rVal;
        }



        public bool IsRectangular(bool bByDiagonal = false)
        {

            if (Vertices.Count != 4) { return false; }
            if (HasArcs) { return false; }

            double d1 = (Vertices.Item(1) - Vertices.Item(3)).Length(3);
            double d2 = (Vertices.Item(2) - Vertices.Item(4)).Length(3);
            bool _rVal = d1 == d2;
            if (!bByDiagonal && _rVal)
            {
                d1 = Math.Round(Vertices.Item(1).X - Vertices.Item(2).X, 3);
                d2 = Math.Round(Vertices.Item(1).Y - Vertices.Item(2).Y, 3);
                if (d1 != 0 & d2 != 0)
                { //the first edge is vertical or horizontal
                    return false;
                }
                d1 = Math.Round(Vertices.Item(2).X - Vertices.Item(3).X, 3);
                d2 = Math.Round(Vertices.Item(2).Y - Vertices.Item(3).Y, 3);
                if (d1 != 0 & d2 != 0)
                { //the second edge is vertical or horizontal
                    return false;
                }
                d1 = Math.Round(Vertices.Item(3).X - Vertices.Item(4).X, 3);
                d2 = Math.Round(Vertices.Item(3).Y - Vertices.Item(4).Y, 3);
                if (d1 != 0 & d2 != 0)
                { //the third edge is vertical or horizontal
                    return false;
                }
                d1 = Math.Round(Vertices.Item(4).X - Vertices.Item(1).X, 3);
                d2 = Math.Round(Vertices.Item(4).Y - Vertices.Item(1).Y, 3);
                if (d1 != 0 & d2 != 0)
                { //the forth edge is vertical or horizontal
                    return false;
                }
            }
            return _rVal;
        }
        public void Move(double aX, double aY)
        {
            Vertices.Move(aX, aY);
            Update();
        }

        public USHAPE Moved(double aX, double aY)
        {
            USHAPE _rVal = new USHAPE(this);
            _rVal.Move(aX, aY);
            return _rVal;
        }

        public void Mirror(double? aX, double? aY)
        {
            Vertices.Mirror(aX, aY);
            Update();
        }

        public USHAPE Mirrored(double? aX, double? aY)
        {
            USHAPE _rVal = new USHAPE(this);
            _rVal.Mirror(aX, aY);
            return _rVal;
        }

        public List<ULINE> Lines() => Segments.Lines();


        public List<UARC> Arcs() => Segments.Arcs();

        #endregion Methods

        #region Shared Methods

        internal static USHAPE? CloneCopy(USHAPE? aShape) { if (aShape.HasValue) return new USHAPE(aShape.Value); else return null; }

        internal static USHAPE Circle(double aX, double aY, double aRadius, string aName = "", string aTag = "")
        {
            USHAPE _rVal = new USHAPE() { Radius = Math.Abs(aRadius) };
            _rVal.IsCircle = true;
            _rVal.Vertices.Add(aX + _rVal.Radius, aY, aRadius: _rVal.Radius);
            _rVal.Vertices.Add(aX, aY + _rVal.Radius, aRadius: _rVal.Radius);
            _rVal.Vertices.Add(aX - _rVal.Radius, aY, aRadius: _rVal.Radius);
            _rVal.Vertices.Add(aX, aY - _rVal.Radius, aRadius: _rVal.Radius);
            _rVal.Tag = aTag;
            _rVal.Name = aName;

            _rVal.Update();

            return _rVal;
        }

        /// </summary>
        /// <param name="aCenterX"></param>
        /// <param name="aCenterY"></param>
        /// <param name="aRadius"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLeftEdge"></param>
        /// <param name="aRightEdge"></param>
        /// <param name="aTopEdge"></param>
        /// <param name="aBottomEdge"></param>
        /// <param name="sClipMoon"></param>
        /// <param name="rClipped"></param>
        /// <param name="rMoon"></param>
        /// <returns></returns>
        internal static USHAPE CircleSection(double aCenterX, double aCenterY, double aRadius, double aRotation = 0, double? aLeftEdge = null, double? aRightEdge = null, double? aTopEdge = null, double? aBottomEdge = null, string aName = "")
        => new USHAPE(USHAPE.CircleSectionVertices(aCenterX, aCenterY, aRadius, aRotation, aLeftEdge, aRightEdge, aTopEdge, aBottomEdge), aName);

        ///#1the center X of the rectangle
        ///#2the center Y of the rectangle
        ///#2the Height the rectangle
        ///#2the Width of the rectangle
        /// </summary>
        /// <param name="aCenterX"></param>
        /// <param name="aCenterY"></param>
        /// <param name="aHeight"></param>
        /// <param name="aWidth"></param>
        /// <param name="aName"></param>
        /// <param name="aTag"></param>
        /// <returns></returns>
        internal static USHAPE Rectangle(double aCenterX, double aCenterY, double aHeight, double aWidth, string aName = "", string aTag = "")
        {
            UVECTORS vrts = new UVECTORS(new UVECTOR(aCenterX - Math.Abs(aWidth) / 2, aCenterY + Math.Abs(aHeight) / 2), new UVECTOR(aCenterX - Math.Abs(aWidth) / 2, aCenterY - Math.Abs(aHeight) / 2), new UVECTOR(aCenterX + Math.Abs(aWidth) / 2, aCenterY - Math.Abs(aHeight) / 2), new UVECTOR(aCenterX + Math.Abs(aWidth) / 2, aCenterY + Math.Abs(aHeight) / 2));
            return new USHAPE(vrts, aName) { Tag = aTag };

        }
 


        /// <summary>
        ///creates the vertices of a polyline that is a section of the indicated circle.
        ///the circle is assumed centered at the origin of a world coordinate system centered at CX,CY
        /// </summary>
        /// <param name="aCenterX"></param>
        /// <param name="aCenterY"></param>
        /// <param name="aRadius"></param>
        /// <param name="rClipped"></param>
        /// <param name="rMoon"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLeftEdge"></param>
        /// <param name="aRightEdge"></param>
        /// <param name="aTopEdge"></param>
        /// <param name="aBottomEdge"></param>
        /// <param name="sClipMoon"></param>
        /// <returns></returns>
        internal static UVECTORS CircleSectionVertices(double aCenterX, double aCenterY, double aRadius, double aRotation = 0, double? aLeftEdge = null, double? aRightEdge = null, double? aTopEdge = null, double? aBottomEdge = null)
        {
            UVECTORS _rVal = UVECTORS.Zero;

            try
            {

                aRadius = Math.Abs(aRadius);
                double rad = aRadius;

                if (rad == 0) { throw new Exception("Invalid Radius Passed."); }
                uopVector cp = new uopVector(aCenterX, aCenterY);
                uopRectangle limits = new uopRectangle(aLeftEdge.HasValue ? aLeftEdge.Value : cp.X - rad - 2, aTopEdge.HasValue ? aTopEdge.Value : cp.Y + rad + 2, aRightEdge.HasValue ? aRightEdge.Value: cp.X + rad + 2, aBottomEdge.HasValue ? aBottomEdge.Value:  cp.Y-rad - 2);
                _rVal.Append(uopShape.CircleSectionVertices(cp, aRadius, limits, aRotation));
        

                return _rVal;
            }
            catch (Exception e)
            {
                throw new Exception("[USHAPE.CircleSectionVertices] " + e.Message);
                //return _rVal;
            }

        }


        internal static USHAPE FromDXFPolyline(dxfPolyline aPolyline, string aTag = null)
        {
            USHAPE _rVal = new USHAPE();
            if (aPolyline == null) return _rVal;
            _rVal.Vertices = UVECTORS.FromDXFVectors(aPolyline.Vertices);
            if (aTag != null) _rVal.Tag = aTag;
            return _rVal;
        }

        internal static USHAPE TrimmedRectangle(URECTANGLE aRectangle, ULINE aTrimLine)
        {
            USHAPE _rVal = new USHAPE("");
            bool rTrimmed = false;

            ULINE left = aRectangle.Edge(uppSides.Left, true);
            ULINE bottom = aRectangle.Edge(uppSides.Bottom, true);
            ULINE right = aRectangle.Edge(uppSides.Right, true);
            ULINE top = aRectangle.Edge(uppSides.Top, true);

            UVECTOR u1;

            //left.Value = 1;
            //bottom.Value = 1;
            //right.Value = 1;
            //top.Value = 1;

            List<bool> flgs = new List<bool>(new bool[5]);


            u1 = left.IntersectionPt(aTrimLine, out bool bPar, out bool bCoinc, out bool bOnLn, out bool BONLN2, out bool aFlg);
            if (u1.Y < aRectangle.Bottom || !aFlg)
            {
                _rVal = USHAPE.Rectangle(aRectangle.X, aRectangle.Y, aRectangle.Height, aRectangle.Width, "RECTANGLE", "MISS BELOW");

            }
            else
            {
                if (aFlg && bOnLn)
                {
                    rTrimmed = true;
                    left.sp = u1.Clone();
                    flgs[1] = true;
                }
                u1 = bottom.IntersectionPt(aTrimLine, out bPar, out bCoinc, out bOnLn, out bool ON2, out aFlg);
                if (aFlg && bOnLn)
                {
                    rTrimmed = true;
                    bottom.ep = u1.Clone();
                    flgs[2] = true;
                }
                u1 = right.IntersectionPt(aTrimLine, out bPar, out bCoinc, out bOnLn, out bool ON3, out aFlg);
                if (aFlg && bOnLn)
                {
                    rTrimmed = true;
                    right.ep = u1.Clone();
                    flgs[3] = true;
                }
                u1 = top.IntersectionPt(aTrimLine, out bPar, out bCoinc, out bOnLn, out bool ON4, out aFlg);
                if (aFlg && bOnLn)
                {
                    rTrimmed = true;
                    top.ep = u1.Clone();
                    flgs[4] = true;
                }
                if (!rTrimmed)
                {
                    _rVal.Vertices.Add(left.sp);
                    _rVal.Vertices.Add(bottom.sp);
                    _rVal.Vertices.Add(right.sp);
                    _rVal.Vertices.Add(top.sp);
                    _rVal.Name = "RECTANGLE";
                }
                else
                {
                    if (flgs[1])
                    {
                        if (flgs[2])
                        {
                            //lower left corner triangle
                            _rVal.Vertices.Add(left.sp);
                            _rVal.Vertices.Add(bottom.sp);
                            _rVal.Vertices.Add(bottom.ep);
                            _rVal.Name = "TRIANGLE";

                        }
                        else if (flgs[3])
                        {
                            //rhombus
                            _rVal.Vertices.Add(left.sp);
                            _rVal.Vertices.Add(bottom.sp);
                            _rVal.Vertices.Add(bottom.ep);
                            _rVal.Vertices.Add(right.ep);
                            if (Math.Round(left.sp.Y, 3) != Math.Round(right.ep.Y, 3))
                            {
                                _rVal.Name = "RHOMBUS";
                            }
                            else
                            {
                                _rVal.Name = "RECTANGLE";
                            }

                        }
                    }
                    else if (flgs[4])
                    {
                        //clipped
                        _rVal.Vertices.Add(left.sp);
                        _rVal.Vertices.Add(bottom.sp);
                        _rVal.Vertices.Add(bottom.ep);
                        _rVal.Vertices.Add(right.ep);
                        _rVal.Vertices.Add(top.ep);
                        _rVal.Name = "CLIPPED";
                    }

                }


            }

            _rVal.Update();
            _rVal.Mark = rTrimmed;
            return _rVal;
        }

        public static USHAPE Null => new USHAPE("", "");
        #endregion Shared Methods

    }

    internal struct USHAPES : ICloneable
    {
        private int _Count;
        private USHAPE[] _Members;
        private bool _Init;
        public string Name;
        public USHAPES(string aName = "") { _Members = new USHAPE[0]; _Init = true; _Count = 0; Name = aName; }
        public USHAPES(USHAPES aShapes, bool bClear = false)
        {
            _Members = new USHAPE[0];
            _Init = true;
            _Count = bClear ? 0 : aShapes.Count;
            Name = aShapes.Name;
            if (_Count > 0)
            {
                _Members = Force.DeepCloner.DeepClonerExtensions.DeepClone<USHAPE[]>(aShapes._Members);


            }


        }
        public USHAPES(uopShapes aShapes)
        {
            _Members = new USHAPE[0];
            _Init = true;
            _Count = 0;
            Name = string.Empty;
            if (aShapes == null) return;

            Name = aShapes.Name;
            foreach (var item in aShapes)
            {
                Add(new USHAPE(item));
            }


        }
        private void Init()
        { _Members = new USHAPE[0]; _Init = true; _Count = 0; Name = string.Empty; }

        public int Count { get { if (!_Init) { Init(); } return _Count; } }

        object ICloneable.Clone() => (object)Clone();

        public USHAPES Clone(bool bReturnEmpty = false) => new USHAPES(this, bReturnEmpty);

        public void Clear() { _Count = 0; _Members = new USHAPE[0]; }

        public USHAPE Item(int aIndex) { if (aIndex < 1 || aIndex > Count) { return new USHAPE(); } else { _Members[aIndex - 1].Index = aIndex; return _Members[aIndex - 1]; } }


        public USHAPE Item(string aName)
        {
            USHAPE mem;
            for (int i = 1; i <= Count; i++) { mem = Item(i); if (string.Compare(mem.Name, aName, ignoreCase: true) == 0) { return mem; } }
            return new USHAPE();
        }
        public void Mirror(double? aX, double? aY)
        {
            for (int i = 1; i <= Count; i++)
            {
                USHAPE mem = Item(i);
                mem.Mirror(aX, aY);
                SetItem(i, mem);
            }
        }

        public USHAPE Add(USHAPE aShape, int? aPanelIndex = null)
        {
            if (Count + 1 > Int32.MaxValue) { return aShape; }

            _Count += 1;
            Array.Resize<USHAPE>(ref _Members, _Count);
            _Members[_Count - 1] = aShape;
            _Members[_Count - 1].Index = _Count;
            if (aPanelIndex.HasValue) _Members[_Count - 1].PartIndex = aPanelIndex.Value;
            return Item(Count);

        }

        public void Append(USHAPES aShapes, int? aPanelIndex = null)
        {
            for (int i = 1; i <= aShapes.Count; i++) { Add(aShapes.Item(i)); }
        }
        public void SetItem(int aIndex, USHAPE aShape) { if (aIndex < 1 || aIndex > Count) { return; } _Members[aIndex - 1] = aShape; }

        /// <summary>
        /// the tags of the shapes in the array
        /// </summary>
        /// <param name="bUnique"></param>
        /// <param name="rTagString"></param>
        /// <returns></returns>
        public List<string> Tags(bool bUnique, out string rTagString)
        {
            rTagString = string.Empty;
            for (int i = 1; i <= Count; i++)
            {
                mzUtils.ListAdd(ref rTagString, Item(i).Tag, bSuppressTest: !bUnique);
            }
            return mzUtils.StringsFromDelimitedList(rTagString, bReturnNulls: true);

        }

        /// <summary>
        /// the names of the shapes in the array
        /// </summary>
        /// <param name="bUnique"></param>
        /// <param name="rNameString"></param>
        /// <returns></returns>
        public List<string> Names(bool bUnique, out string rNameString)
        {
            rNameString = string.Empty;
            for (int i = 1; i <= Count; i++)
            {
                mzUtils.ListAdd(ref rNameString, Item(i).Name, bSuppressTest: !bUnique);
            }
            return mzUtils.StringsFromDelimitedList(rNameString, bReturnNulls: true);

        }

        public override string ToString() => string.IsNullOrWhiteSpace(Name) ? "USHAPES[" + Count + "]" : "USHAPES '" + Name + "' [" + Count + "]";
    }

    internal struct UARCREC : ICloneable
    {
        public UVECTOR Center;
        public bool IsArc;
        public double Rotation;
        public double Radius;
        public double Width;
        public double Height;
        public string Tag;
        public string Flag;
        public bool Proximity;
        public int Index;
        public bool Suppressed ;
        public UARCREC(double aX = 0, double aY = 0, bool bIsArc = false, double aRadius = 0, string aTag = "", string aFlag = "")
        {
            Index = 0;
            IsArc = bIsArc;
            Center = new UVECTOR(aX, aY);
            Rotation = 0;
            Radius = aRadius;
            Width = 0;
            Height = 0;
            Tag = aTag;
            Flag = aFlag;
            Proximity = false;
            Suppressed = false;
        }

        public UARCREC(UVECTOR aCenter, double aWidth, double aHeight, double aRotation = 0, string aTag = "", string aFlag = "")
        {
            Index = 0;
            IsArc = false;
            Center = new UVECTOR(aCenter);
            Rotation = aRotation;
            Radius = 0;
            Width = Math.Abs(aWidth);
            Height = Math.Abs(aHeight);
            Tag = aTag;
            Flag = aFlag;
            Proximity = false;
            Suppressed = false;
        }

        public double X => Center.X;
        public double Y => Center.Y;

        public UARCREC(iArcRec aArcRec)
        {
          
            IsArc = aArcRec.Type == uppArcRecTypes.Arc;
            Center = new UVECTOR(aArcRec.Center);
            Rotation = aArcRec.Rotation;
            Radius = aArcRec.Radius;
            Width = Math.Abs(aArcRec.Width);
            Height = Math.Abs(aArcRec.Height);
            Tag = aArcRec.Tag;
            Flag = aArcRec.Flag;
            Proximity = false;
            Index = 0;
            Suppressed = false;
        }

        public UARCREC(UARCREC aArcRec)
        {
            Index = aArcRec.Index;
            IsArc = aArcRec.IsArc;
            Center = new UVECTOR(aArcRec.Center);
            Rotation = aArcRec.Rotation;
            Radius = aArcRec.Radius;
            Width = Math.Abs(aArcRec.Width);
            Height = Math.Abs(aArcRec.Height);
            Tag = aArcRec.Tag;
            Flag = aArcRec.Flag;
            Proximity = aArcRec.Proximity;
            Suppressed = false;
        }

        object ICloneable.Clone() => (object)Clone();

        public UARCREC Clone() => new UARCREC(this);

        public UVECTORS Corners
        {
            get
            {
                UVECTORS _rVal = UVECTORS.Zero;


                _rVal.Add(X - Width / 2, Y + Height / 2);
                _rVal.Add(X - Width / 2, Y - Height / 2);
                _rVal.Add(X + Width / 2, Y - Height / 2);
                _rVal.Add(X + Width / 2, Y + Height / 2);
                if (Rotation != 0)
                {
                    _rVal.Rotate(Center, Rotation, false);
                }
                return _rVal;
            }


        }

        public dxfEntity ToDXFEntity(string aLayerName = "", dxxColors aColor = dxxColors.Undefined, string aLineType = "")
        {
            if (IsArc)
            {
                dxeArc _rVal = new dxeArc(Center.ToDXFVector(), aRadius: Radius);
                _rVal.LCLSet(aLayerName, aColor, aLineType);
                return _rVal;
            }
            else
            {
                dxePolyline _rVal = new dxePolyline(Corners.ToDXFVectors(), bClosed: true);
                _rVal.LCLSet(aLayerName, aColor, aLineType);
                return _rVal;

            }
        }

        /// <summary>
        /// check wether ArcRec contains points or not
        /// </summary>
        /// <param name="aPoint"></param>
        /// <param name="bOnIsIn"></param>
        /// <param name="aPrecis"></param>
        /// <returns></returns>
        public bool ContainsPoint(UVECTOR aPoint, bool bOnIsIn, int aPrecis = 4) => ContainsPoint(aPoint, bOnIsIn, out bool _, aPrecis);

        /// <summary>
        /// check wether ArcRec contains points or not
        /// </summary>
        /// <param name="aPoint"></param>
        /// <param name="bOnIsIn"></param>
        /// <param name="aPrecis"></param>
        /// <returns></returns>
        public bool ContainsPoint(iVector aPoint, bool bOnIsIn, int aPrecis = 4) => aPoint == null ? false : ContainsPoint(new UVECTOR(aPoint), bOnIsIn, out bool _, aPrecis);

        public void Mirror(double? aX, double? aY)
        {
            if (!aX.HasValue && !aY.HasValue) return;
            Center.Mirror(aX, aY);
        }

        /// <summary>
        /// check wether ArcRec contains points or not
        /// </summary>
        /// <param name="aPoint"></param>
        /// <param name="bOnIsIn"></param>
        /// <param name="aPrecis"></param>
        /// <param name="rIsOnEdge"></param>
        /// <returns></returns>
        public bool ContainsPoint(UVECTOR aPoint, bool bOnIsIn, out bool rIsOnEdge, int aPrecis = 4)
        {
            bool _rVal = false;
            aPrecis = mzUtils.LimitedValue(aPrecis, 1, 6);

            rIsOnEdge = false;
            double d1 = Center.DistanceTo(aPoint, aPrecis);
            double d2 = Math.Round(Radius, aPrecis);
            aPoint.Proximity = d2;

            if (d1 > d2) { return false; }

            if (IsArc)
            {

                if (d1 <= d2)
                {
                    if (d1 == d2)
                    {
                        rIsOnEdge = true;
                        _rVal = bOnIsIn;
                    }
                    else
                    {
                        _rVal = true;
                    }
                }

            }
            else
            {
                double dX;
                double dY;
                double wd = Math.Round(Width / 2, aPrecis);
                double ht = Math.Round(Height / 2, aPrecis);
                UVECTOR u1;
                UVECTOR aDir = new UVECTOR(1, 0);

                if (Rotation == 0)
                {
                    dX = Math.Round(Math.Abs(aPoint.X - Center.X), aPrecis);
                    dY = Math.Round(Math.Abs(aPoint.Y - Center.Y), aPrecis);
                }
                else
                {
                    aDir.Rotate(Rotation);
                    u1 = aPoint.Clone();
                    u1.ProjectTo(new ULINE(Center, Center.Projected(aDir, 10)), out double dst);

                    dY = Math.Round(dst, aPrecis);
                    dX = Center.DistanceTo(u1, aPrecis);

                }


                if (dX <= wd && dY <= ht)
                {
                    if (dX == wd || dY == ht)
                    {
                        rIsOnEdge = true;
                        _rVal = bOnIsIn;
                    }
                    else
                    {
                        _rVal = true;
                    }
                }


            }

            return _rVal;
        }

        /// <summary>
        /// check weather arcrec contains other arcrec
        /// </summary>
        /// <param name="aArcRec"></param>
        /// <param name="bArcRec"></param>
        /// <param name="bOnIsIn"></param>
        /// <param name="aPrecis"></param>
        /// <param name="bSuppressRectangleUpdate"></param>
        /// <returns></returns>
        public bool Contains(UARCREC bArcRec, bool bOnIsIn, int aPrecis = 4, bool bSuppressRectangleUpdate = false)
        {
            bool _rVal = false;


            if (!IsArc)
            {
                if (Radius <= 0)
                {
                    Radius = Math.Sqrt(Math.Pow(Width, 2) + Math.Pow(Height, 2)) / 2;
                }
            }

            if (!bArcRec.IsArc)
            {
                if (bArcRec.Radius <= 0)
                {
                    bArcRec.Radius = Math.Sqrt(Math.Pow(bArcRec.Width, 2) + Math.Pow(bArcRec.Height, 2)) / 2;
                }
            }

            if (aPrecis <= 0) { aPrecis = 1; }
            if (aPrecis > 6) { aPrecis = 6; }

            double dst = Center.DistanceTo(bArcRec.Center, aPrecis);
            if (dst == 0) { return false; } //they coincident
            double rad1 = Math.Round(Radius, aPrecis);
            double rad2 = Math.Round(bArcRec.Radius, aPrecis);

            if (dst > rad1 + rad2) { return false; } //they are too far away so bail

            if (IsArc || bArcRec.IsArc)
            {
                if (IsArc && bArcRec.IsArc)
                {
                    if (rad1 + rad2 == dst)
                    { return bOnIsIn; } //tangent arcs
                    else
                    { return true; }

                }
                else if (IsArc)
                {
                    if (dst <= rad1) { return true; } //the center of the secnd is within the first circle
                }
                else if (bArcRec.IsArc)
                {
                    if (dst <= rad2) { return true; } //the center of the first is within the second circle
                }
            }


            UARCREC aArc;


            UVECTORS ips;


            //at this point at least one is a rectangle



            if (!IsArc && !bArcRec.IsArc)
            {
                //two rectangles

                UARCREC aRec = this;
                UARCREC bRec = bArcRec;

                if (aRec.Rotation == 0 & aRec.Rotation == 0)
                {
                    double d1 = Math.Round(Math.Abs(aRec.Center.X - bRec.Center.X), aPrecis);
                    double wd = Math.Round(aRec.Width / 2 + bRec.Width / 2, aPrecis);
                    if (d1 <= wd)
                    {
                        double d2 = Math.Round(Math.Abs(aRec.Center.Y - bRec.Center.Y), aPrecis);
                        double ht = Math.Round(aRec.Height / 2 + bRec.Height / 2, aPrecis);
                        if (d2 <= ht)
                        {
                            if (d1 == wd)
                            { return bOnIsIn; } //tangent side to side
                            else
                            {
                                if (d2 == ht)
                                { return bOnIsIn; } //tangent side to side
                                else
                                { return true; }
                            }
                        }
                    }
                }
                else
                {
                    //at least one rectangle is rotated
                    ips = bRec.Corners;
                    for (int i = 1; i <= ips.Count; i++)
                    {
                        if (aRec.ContainsPoint(ips.Item(i), true, out bool aFlg, aPrecis))
                        {
                            if (aFlg)
                            { if (bOnIsIn) { return true; } }
                            else
                            { return true; }
                        }
                    }


                    if (!_rVal)
                    {
                        if (aRec.Corners.LineSegments(true).Intersections(bRec.Corners.LineSegments(true)).Count > 0)
                        { return true; }
                    }


                }

            }
            else
            {
                UARCREC aRec;
                //one is an arc the other is a rectangle
                if (!IsArc)
                {
                    aRec = this;
                    aArc = bArcRec;
                }
                else
                {
                    aRec = bArcRec;
                    aArc = this;
                }

                if (dst < aArc.Radius)
                {
                    return true;
                }
                else
                {
                    for (int i = 1; i <= aRec.Corners.Count; i++)
                    {
                        if (aArc.ContainsPoint(aRec.Corners.Item(i), true, out bool aFlg, aPrecis))
                        {
                            if (aFlg)
                            {
                                if (bOnIsIn) { return true; }
                            }
                            else
                            { return true; }
                        }
                    }
                    if (!_rVal)
                    {
                        if (aRec.Corners.LineSegments(true).ArcIntersections(aArc.Center, Math.Round(aArc.Radius, aPrecis)).Count > 0)
                        { return true; }
                    }

                }


            }

            return false;

        }

        #region Shared Methods

        internal static UARCREC FromDXFRectangle(dxfRectangle aRect)
        => new UARCREC(UVECTOR.FromDXFVector(aRect.Center), aRect.Width, aRect.Height, aRect.XDirection.AngleTo(new dxfDirection(1, 0, 0)), aRect.Tag, aRect.Flag);


        #endregion

    }

    internal struct UARCRECS : ICloneable
    {
        public UARCREC[] _Members;
        private int _Count;
        private bool _Init;
        public string Name;

        public UARCRECS(string aName = "")
        {
            Name = aName;
            _Count = 0;
            _Members = new UARCREC[0];
            _Init = true;
        }

        public UARCRECS(UARCRECS aArcRecs)
        {
            Name = aArcRecs.Name;
            _Count = aArcRecs.Count;
            _Members = new UARCREC[0];
            _Init = true;
            for (int i = 1; i <= aArcRecs.Count; i++)
            {
                Add(new UARCREC(aArcRecs.Item(i)));
            }
        }

        public UARCRECS(List<iArcRec> aArcRecs)
        {
            Name = string.Empty;
            _Count = aArcRecs.Count;
            _Members = new UARCREC[0];
            _Init = true;

            if (aArcRecs == null) return;
            foreach(var item in aArcRecs)
                Add(new UARCREC(item));
        }


        public void Clear() { _Count = 0; _Members = new UARCREC[0]; }

        private int Init() { _Count = 0; _Members = _Members = new UARCREC[0]; _Init = true; return 0; }

        object ICloneable.Clone() => (object)Clone(false);

        public UARCRECS Clone(bool bReturnEmpty = false)
        {
            ;
            if (!_Init) { Init(); }


            UARCRECS _rVal = new UARCRECS(this);

            if (!bReturnEmpty)
            {
                _rVal.Clear();
            }
            return _rVal;
        }

        public int Count => (!_Init) ? Init() : _Count;

        public List<UARCREC> ToList { get { if (!_Init) { Init(); } return new List<UARCREC>(_Members); } }


        public void Mirror(double? aX, double? aY)
        {
            if (Count <= 0 || (!aX.HasValue && !aY.HasValue)) return;
            for (int i = 1; i <= Count; i++)
            {
                UARCREC mem = Item(i);
                mem.Mirror(aX, aY);
                SetItem(i, mem);
            }
        }

        public void SetItem(int aIndex, UARCREC aArcRec)
        {
            if (aIndex < 1 || aIndex > Count) { return; }
            aArcRec.Index = aIndex;
            _Members[aIndex - 1] = aArcRec;
        }


        public UARCREC Item(int aIndex) { if (aIndex < 1 || aIndex > Count) { return new UARCREC(); } _Members[aIndex - 1].Index = aIndex; return _Members[aIndex - 1]; }

        public UARCREC GetTagged(string aTag, string aFlag = null)
        {
            UARCREC mem = new UARCREC();
            bool flags = !string.IsNullOrEmpty(aFlag);
            for (int i = 1; i <= Count; i++)
                mem = Item(i);
            if (string.Compare(mem.Tag, aTag, ignoreCase: true) == 0)
            {
                if (!flags)
                { return mem; }
                else
                {
                    if (string.Compare(mem.Flag, aFlag, ignoreCase: true) == 0) { return mem; }
                }

            }
            return new UARCREC();
        }

        public UARCREC Add(UARCREC aArcRec, string aTag = null, string aFlag = null)
        {

            { if (!_Init) { Init(); } }
            _Count += 1;
            aArcRec.Index = Count;
            if (!string.IsNullOrEmpty(aTag)) { aArcRec.Tag = aTag; }
            if (!string.IsNullOrEmpty(aFlag)) { aArcRec.Flag = aFlag; }
            Array.Resize<UARCREC>(ref _Members, _Count);
            _Members[_Count - 1] = aArcRec;
            return _Members[_Count - 1];
        }

        public UARCREC Remove(int aIndex)
        {

            if (aIndex < 1 || aIndex > Count) { return new UARCREC(0, 0); }

            UARCREC _rVal = new UARCREC();
            if (Count == 1) { _rVal = Item(1); Clear(); return _rVal; }
            if (aIndex == Count)
            {
                _rVal = Item(Count);
                _Count -= 1;
                Array.Resize<UARCREC>(ref _Members, _Count);
                return _rVal;
            }


            UARCREC[] newMems = new UARCREC[_Count - 2];
            int j = 0;


            for (int i = 1; i <= Count; i++)
            {
                if (i != aIndex)
                { j += 1; newMems[j - 1] = Item(i); newMems[j - 1].Index = j; }
                else
                { _rVal = Item(i); }

            }

            _Members = newMems;
            _Count -= 1;
            return _rVal;
        }

        public colDXFEntities ToDXFEntities(string aLayerName = "", dxxColors aColor = dxxColors.Undefined, string aLineType = "")
        {
            colDXFEntities _rVal = new colDXFEntities();
            for (int i = 1; i <= Count; i++) { _rVal.Add(Item(i).ToDXFEntity(aLayerName, aColor, aLineType)); }
            return _rVal;
        }

        public UARCREC AddArc(double aX, double aY, double aRadius, string aTag = "", string aFlag = "") => Add(new UARCREC(aX, aY, true, aRadius), aTag, aFlag);

        public UARCREC AddArc(UVECTOR aCenter, double aRadius, string aTag = "", string aFlag = "") => Add(new UARCREC(aCenter.X, aCenter.Y, true, aRadius), aTag, aFlag);

        public UARCREC AddRec(UVECTOR aCenter, double aWidth, double aHeight, double aRotation = 0, string aTag = "", string aFlag = "") => Add(new UARCREC(aCenter, aWidth, aHeight, aRotation, aTag, aFlag));

        public UARCREC AddRec(double aX, double aY, double aWidth, double aHeight, double aRotation = 0, string aTag = "", string aFlag = "") => Add(new UARCREC(new UVECTOR(aX, aY), aWidth, aHeight, aRotation, aTag, aFlag));

        public UARCREC AddRecLRTB(double aLeft, double aRight, double aTop, double aBottom, double aRotation = 0, string aTag = "", string aFlag = "")
        {
            double x1 = aLeft;
            double x2 = aRight;
            double y1 = aBottom;
            double y2 = aTop;

            mzUtils.SortTwoValues(true, ref x1, ref x2);
            mzUtils.SortTwoValues(true, ref y1, ref y2);
            return AddRec(x1 + (x2 - x1) / 2, y1 + (y2 - y1) / 2, x2 - x1, y2 - y1, aRotation, aTag, aFlag);

        }

        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(Name) ? "UARCRECS[" + Count.ToString() + "]" : "UARCRECS '" + Name + "' [" + Count.ToString() + "]";
        }


    }

    internal struct TFLANGE : ICloneable
    {
        public int DBID;
        public string Family;
        public string FlangeTypeName;
        public uppFlangeTypes FlangeType;
        public string Descriptor;
        public string GroupName;
        public int Rating;
        public double Weight;
        public double Size;
        public double OD;
        public double Bore;
        public double Thickness;
        public double R;
        public double X;
        public double L;
        public double H;
        public double BoltCircle;
        public int HoleCount;
        public double HoleDia;
        public string BoltSize_UNC;
        public double MinStud_UNC;
        public string BoltSize_Metric;
        public double MinStud_Metric;
        public int Quantity;
        public string Spec;
        public string Material;
        public string StudSize;
        public double StudLength;
        public int TotalStuds;
        public int Index;

        public TFLANGE(uppFlangeTypes aType = uppFlangeTypes.uop_UndefinedFlange)
        {
            DBID = 0;
            Family = string.Empty;
            FlangeTypeName = string.Empty;
            FlangeType = uppFlangeTypes.uop_UndefinedFlange;
            Descriptor = string.Empty;
            GroupName = string.Empty;
            Rating = 0;
            Weight = 0;
            Size = 0;
            OD = 0;
            Bore = 0;
            Thickness = 0;
            R = 0;
            X = 0;
            L = 0;
            H = 0;
            BoltCircle = 0;
            HoleCount = 0;
            HoleDia = 0;
            BoltSize_UNC = string.Empty;
            MinStud_UNC = 0;
            BoltSize_Metric = string.Empty;
            MinStud_Metric = 0;
            Quantity = 0;
            Spec = string.Empty;
            Material = string.Empty;
            StudSize = string.Empty;
            StudLength = 0;
            TotalStuds = 0;
            Index = 0;
        }

        public TFLANGE Clone()
        {
            return new TFLANGE
            {
                DBID = DBID,
                Family = Family,
                FlangeTypeName = FlangeTypeName,
                FlangeType = FlangeType,
                Descriptor = Descriptor,
                GroupName = GroupName,
                Rating = Rating,
                Weight = Weight,
                Size = Size,
                OD = OD,
                Bore = Bore,
                Thickness = Thickness,
                R = R,
                X = X,
                L = L,
                H = H,
                BoltCircle = BoltCircle,
                HoleCount = HoleCount,
                HoleDia = HoleDia,
                BoltSize_UNC = BoltSize_UNC,
                MinStud_UNC = MinStud_UNC,
                BoltSize_Metric = BoltSize_Metric,
                MinStud_Metric = MinStud_Metric,
                Quantity = Quantity,
                Spec = Spec,
                Material = Material,
                StudSize = StudSize,
                StudLength = StudLength,
                TotalStuds = TotalStuds,
                Index = Index
            };
        }

        object ICloneable.Clone() => (object)Clone();
    }

    internal struct TFLANGES : ICloneable
    {
        public string Family;
        public int Rating;
        public string GroupName;
        public string ShortGroupName;
        public string Comment;
        public string FlangeTypeName;
        public string SizeRange;

        private int _Count;
        private bool _Init;
        private TFLANGE[] _Members;

        public TFLANGES(string aFamily = "")
        {
            _Members = new TFLANGE[0];
            Family = aFamily;
            Rating = 0;
            GroupName = string.Empty;
            ShortGroupName = string.Empty;
            Comment = string.Empty;
            FlangeTypeName = string.Empty;
            SizeRange = string.Empty;
            _Count = 0;
            _Init = true;

        }

        private int Init() { _Count = 0; _Members = new TFLANGE[0]; _Init = true; return 0; }

        public int Count => (!_Init) ? Init() : _Count;

        public TFLANGE Item(int aIndex) { if (aIndex < 1 || aIndex > Count) return new TFLANGE(); _Members[aIndex - 1].Index = aIndex; return _Members[aIndex - 1]; }

        public TFLANGE Add(TFLANGE aFlange)
        {
            if (Count + 1 > Int32.MaxValue) { return aFlange; }

            _Count += 1;
            Array.Resize<TFLANGE>(ref _Members, _Count);
            _Members[_Count - 1] = aFlange.Clone();
            return Item(Count);
        }

        object ICloneable.Clone() => (object)Clone(false);

        public TFLANGES Clone(bool bReturnEmpty = false)
        {

            if (!_Init) { Init(); }
            TFLANGE[] mems = bReturnEmpty ? new TFLANGE[0] : Force.DeepCloner.DeepClonerExtensions.DeepClone<TFLANGE[]>(_Members); //(TFLANGE[])_Members.Clone();
            int cnt = bReturnEmpty ? 0 : _Count;

            return new TFLANGES()
            {
                Family = Family,
                Rating = Rating,
                GroupName = GroupName,
                ShortGroupName = ShortGroupName,
                Comment = Comment,
                FlangeTypeName = FlangeTypeName,
                SizeRange = SizeRange,
                _Count = cnt,
                _Members = mems,
                _Init = true
            };



        }

        public TFLANGE GetByDescriptor(string aDescriptor, out int rIndex)
        {
            rIndex = 0;
            TFLANGE _rVal = new TFLANGE();
            if (string.IsNullOrWhiteSpace(aDescriptor)) return _rVal;

            aDescriptor = aDescriptor.Trim();

            TFLANGE aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                if (string.Equals(aDescriptor, aMem.Descriptor, StringComparison.OrdinalIgnoreCase))
                {
                    rIndex = i;
                    _rVal = aMem;
                    break;
                }
                i++;
            }
            return _rVal;
        }
    }

    internal struct TFLANGE_ARRAY : ICloneable
    {

        private int _Count;
        private bool _Init;
        private TFLANGES[] _Members;


        private int Init() { _Count = 0; _Members = new TFLANGES[0]; _Init = true; return 0; }

        public int Count => (!_Init) ? Init() : _Count;

        public TFLANGES Item(int aIndex) { return (aIndex < 1 || aIndex > Count) ? new TFLANGES() : _Members[aIndex - 1]; }


        public TFLANGES Add(TFLANGES aFlanges)
        {
            if (Count + 1 > Int32.MaxValue) { return aFlanges; }

            _Count += 1;
            Array.Resize<TFLANGES>(ref _Members, _Count);
            _Members[_Count - 1] = aFlanges.Clone();
            return Item(Count);
        }

        public TFLANGE_ARRAY Clone(bool bReturnEmpty = false)
        {

            if (!_Init) { Init(); }
            TFLANGES[] mems = bReturnEmpty ? new TFLANGES[0] : Force.DeepCloner.DeepClonerExtensions.DeepClone<TFLANGES[]>(_Members); //(TFLANGES[])_Members.Clone();
            int cnt = bReturnEmpty ? 0 : _Count;

            return new TFLANGE_ARRAY()
            {

                _Count = cnt,
                _Members = mems,
                _Init = true
            };



        }

        object ICloneable.Clone() => (object)Clone(false);

        public void SetItem(int aIndex, TFLANGES aFlanges)
        {
            if (aIndex < 1 || aIndex > Count) { return; }
            _Members[aIndex - 1] = aFlanges;
        }
    }


    internal struct USEGMENTS : ICloneable
    {
        private USEGMENT[] _Members;
        private bool _Init;
        private int _Count;
        public string Name;

        #region Constructors

        public USEGMENTS(string aName = "")
        {
            _Members = new USEGMENT[0];
            _Count = 0;
            _Init = true;
            Name = aName;
        }

        public USEGMENTS(USEGMENT[] aMembers, string aName = "")
        {
            _Members = aMembers;
            _Count = 0;
            _Init = aMembers != null;
            Name = aName;
            if (_Init) _Count = aMembers.Length;

        }
        public USEGMENTS(USEGMENTS aMembers, bool bClear = false)
        {
            _Members = new USEGMENT[0];
            _Count = bClear ? 0 : aMembers.Count;
            _Init = true;
            Name = aMembers.Name;
            if (_Count > 0)
            {
                _Members = Force.DeepCloner.DeepClonerExtensions.DeepClone<USEGMENT[]>(aMembers._Members);
            };

        }

        public USEGMENTS(List<iSegment> aMembers)
        {
            _Members = new USEGMENT[0];
            _Count = 0;
            _Init = true;
            Name = string.Empty;
            if (aMembers == null) return;

            foreach (var item in aMembers)
            {
                if (item == null) continue;
                Add(new USEGMENT(item));

            }

        }
        #endregion Constructors

        #region Properties

        public int Count => (!_Init) ? Init() : _Count;

        #endregion Properties


        #region Methods

        private int Init()
        {
            _Members = new USEGMENT[0];
            _Count = 0;
            _Init = true;
            return 0;
        }

        public override string ToString() => string.IsNullOrWhiteSpace(Name) ? "USEGMENTS[" + Count + "]" : "USEGMENTS '" + Name + "' [" + Count + "]";

        object ICloneable.Clone() => (object)Clone(false);

        public USEGMENTS Clone(bool bReturnEmpty = false) => new USEGMENTS(this, bReturnEmpty);


        public void Clear() { _Count = 0; _Members = new USEGMENT[0]; }

        public USEGMENT Item(int aIndex)
        {
            if (aIndex < 1 || aIndex > Count) throw new IndexOutOfRangeException();
            _Members[aIndex - 1].Index = aIndex;
            return _Members[aIndex - 1];
        }

        public void SetItem(int aIndex, USEGMENT aSegment)
        {
            if (aIndex < 1 || aIndex > Count) throw new IndexOutOfRangeException();
            _Members[aIndex - 1] = aSegment;
        }

        public USEGMENT Add(USEGMENT aSegment)
        {
            if (Count + 1 > Int32.MaxValue) { return aSegment; }

            _Count += 1;
            Array.Resize<USEGMENT>(ref _Members, _Count);
            _Members[_Count - 1] = aSegment;
            return Item(Count);
        }

        public USEGMENT GetTagged(string aTag, out int rIndex)
        {
            USEGMENT _rVal = new USEGMENT();
            rIndex = 0;
            USEGMENT seg;

            for (int i = 1; i <= Count; i++)
            {
                seg = Item(i);
                if (string.Compare(aTag, seg.Tag, ignoreCase: true) == 0)
                {
                    _rVal = seg;
                    rIndex = i;
                    break;
                }
            }
            return _rVal;
        }

        public colDXFEntities ToDXFEntities(string aLayerName = "", dxxColors aColor = dxxColors.Undefined, string aLinetype = "")
        {
            colDXFEntities _rVal = new colDXFEntities();
            USEGMENT aSeg;

            for (int i = 1; i <= Count; i++)
            {
                aSeg = Item(i);
                if (aSeg.IsArc)
                { _rVal.Add(aSeg.ArcSeg.ToDXFArcEX(null, aLayerName, aColor, aLinetype)); }
                else
                { _rVal.Add(aSeg.LineSeg.ToDXFLineEX(null, aLayerName, aColor, aLinetype)); }
            }
            return _rVal;
        }


        public bool ContainsVector(UVECTOR aVector, out UVECTORS rIntercepts, bool bOnIsIn = true, int aPrecis = 3, dynamic aTestOrd = null)
        {
            USEGMENT bSeg;
            UVECTOR u1;

            bool aFlg = false;

            rIntercepts = UVECTORS.Zero;
            if (bOnIsIn)
            {
                //check to see if the vector lies on one of the segments within a fudge factor
                for (int i = 1; i <= Count; i++)
                {
                    bSeg = Item(i);
                    if (aVector.LiesOn(bSeg, aPrecis: aPrecis))
                    {
                        if (!rIntercepts.ContainsVector(aVector, aPrecis)) rIntercepts.Add(aVector);

                    }
                }
                if (rIntercepts.Count > 0) return true;
            }

            double tord = (aTestOrd == null) ? Double.MaxValue / 2 : mzUtils.VarToDouble(aTestOrd, true, Double.MaxValue / 2);
            //get the intersections of a ray from the point to infinity X with the segments
            //an odd number of intercepts means the point lies within then segments
            ULINE iLine = new ULINE(new UVECTOR(aVector), new UVECTOR(tord, aVector.Y));

            for (int i = 1; i <= Count; i++)
            {
                bSeg = Item(i);
                UVECTORS ips = iLine.Intersections(bSeg, aSegIsInfinite: false, aLineIsInfinite: false);
            
                if (ips.Count <= 0) continue;


                for (int j = ips.Count; j >= 1; j--)
                {
                    u1 = ips.Item(j);
                    //don't count any intersections if they already found or if they lie on the test point
                    aFlg = !u1.Compare(aVector, 3);
                    if (aFlg)
                    {
                        if (rIntercepts.ContainsVector(u1)) aFlg = false;

                        if (aFlg)
                            rIntercepts.Add(u1);

                    }
                }

            }


            return (rIntercepts.Count > 0) && mzUtils.IsOdd(rIntercepts.Count);
        }

        public UVECTORS Intersections(ULINE aLine, bool aLineIsInfinite = false, bool aSegsAreInfinite = false)
        {
            UVECTORS _rVal = UVECTORS.Zero;
            for (int i = 1; i <= Count; i++)
            {
                _rVal.Append(aLine.Intersections(Item(i), aSegIsInfinite: aSegsAreInfinite, aLineIsInfinite: aLineIsInfinite));
            }
            return _rVal;
        }
        public UVECTORS Intersections(ULINE? aLine, bool aLineIsInfinite = false) => (!aLine.HasValue) ? UVECTORS.Zero : Intersections(aLine.Value, aLineIsInfinite);

        public List<ULINE> Lines()
        {
            List<ULINE> _rVal = new List<ULINE>();
            for (int i = 1; i <= Count; i++)
            {
                USEGMENT seg = Item(i);
                if (!seg.IsArc) _rVal.Add(seg.LineSeg);
            }

            return _rVal;
        }

        public List<UARC> Arcs()
        {
            List<UARC> _rVal = new List<UARC>();
            for (int i = 1; i <= Count; i++)
            {
                USEGMENT seg = Item(i);
                if (seg.IsArc) _rVal.Add(seg.ArcSeg);
            }

            return _rVal;
        }
        #endregion  Methods
    }

    internal struct TUNIT : ICloneable
    {
        public string Name;
        public string SILabel;
        public string MetricLabel;
        public string EnglishLabel;
        public uppUnitFamilies UnitSystem;
        public double SIFactor;
        public double MetricFactor;
        private uppUnitTypes _UnitType;
        public int Index;
        public int EnglishPrecision;
        public int MetricPrecision;
        #region Constructors

        public TUNIT(uppUnitTypes aUnitType, uppUnitFamilies aUnitSystem = uppUnitFamilies.English)
        {
            Name = string.Empty;
            SILabel = string.Empty;
            MetricLabel = string.Empty;
            EnglishLabel = string.Empty;
            SIFactor = 1;
            MetricFactor = 1;
            Index = 0;
            EnglishPrecision = -1;
            MetricPrecision = -1;
            _UnitType = uppUnitTypes.Undefined;
            UnitSystem = aUnitSystem;
            SetType(aUnitType);

        }

        #endregion

        object ICloneable.Clone() => (object)Clone(true);

        public TUNIT Clone(bool bCloneIndex = true)
        {
            int idx = (!bCloneIndex) ? 0 : Index;
            return new TUNIT()
            {
                Name = Name,
                SILabel = SILabel,
                MetricLabel = MetricLabel,
                EnglishLabel = EnglishLabel,
                SIFactor = SIFactor,
                MetricFactor = MetricFactor,
                Index = idx,
                _UnitType = _UnitType,
                UnitSystem = UnitSystem,
                EnglishPrecision = EnglishPrecision,
                MetricPrecision = MetricPrecision
            };

        }

        public override string ToString() => "TUNITS [ " + Name + "]";

        public uppUnitTypes UnitType
        {
            get => _UnitType;
            set
            {
                if (_UnitType != value) SetType(value);
            }
        }

        public void SetType(uppUnitTypes aType)
        {
            _UnitType = aType;
            if (aType != uppUnitTypes.Undefined)
            {
                Name = uopEnums.Description(aType).Replace(" ", "");
                EnglishLabel = uopUnits.UnitLabel(aType, uppUnitFamilies.English);
                MetricLabel = uopUnits.UnitLabel(aType, uppUnitFamilies.Metric);
                SILabel = uopUnits.UnitLabel(aType, uppUnitFamilies.SI);
                MetricFactor = uopUnits.UnitFactor(aType, uppUnitFamilies.Metric);
                SIFactor = uopUnits.UnitFactor(aType, uppUnitFamilies.SI);
                MetricPrecision = uopUnits.UnitPrecision(aType, uppUnitFamilies.Metric);
                EnglishPrecision = uopUnits.UnitPrecision(aType, uppUnitFamilies.English);
                TUNIT gunits = uopUnits.gsUnits.GetByType(_UnitType);
                if (gunits.UnitType > 0) Index = gunits.Index;

            }
            else
            {
                Name = string.Empty;
                EnglishLabel = string.Empty;
                MetricLabel = string.Empty;
                SILabel = string.Empty;
                MetricFactor = 1;
                SIFactor = 1;
                Index = 0;
                EnglishPrecision = -1;
                MetricPrecision = -1;
            }

        }

        public bool IsDefined => UnitType > uppUnitTypes.Undefined;


        public string Label(uppUnitFamilies aUnitFamily, bool addleadSpace = false)
        {
            string _rVal = string.Empty;

            if ((int)aUnitFamily < (int)uppUnitFamilies.Default)
                aUnitFamily = uppUnitFamilies.English;

            switch (aUnitFamily)
            {
                case uppUnitFamilies.English:
                    _rVal = EnglishLabel;

                    break;
                case uppUnitFamilies.Metric:
                    _rVal = MetricLabel;

                    break;
                case uppUnitFamilies.SI:
                    _rVal = SILabel;
                    break;
            }
            if (addleadSpace && !string.IsNullOrEmpty(_rVal)) _rVal = " " + _rVal;
            return _rVal;
        }



        public int Precision(uppUnitFamilies aFamily) => aFamily == uppUnitFamilies.English ? EnglishPrecision : MetricPrecision;

        public void SetUnitPrecision(uppUnitFamilies aFamily, int aPrecis)
        {
            if (aFamily == uppUnitFamilies.English)
                EnglishPrecision = mzUtils.LimitedValue(aPrecis, 0, 8, uopUnits.UnitPrecision(UnitType, aFamily));
            else
                MetricPrecision = mzUtils.LimitedValue(aPrecis, 0, 8, uopUnits.UnitPrecision(UnitType, aFamily));
        }

        public string GetUnitLabel(uppUnitFamilies aUnitFamily)
        {
            if (aUnitFamily == uppUnitFamilies.English) return EnglishLabel;
            if (aUnitFamily == uppUnitFamilies.Metric) return MetricLabel;
            if (aUnitFamily == uppUnitFamilies.SI) return SILabel;
            return EnglishLabel;
        }

        public double GetUnitFactor(uppUnitFamilies aUnitFamily)
        {
            if (aUnitFamily == uppUnitFamilies.English) return 1d;
            if (aUnitFamily == uppUnitFamilies.Metric) return (MetricFactor != 0) ? MetricFactor : 0;
            if (aUnitFamily == uppUnitFamilies.SI) return (SIFactor != 0) ? SIFactor : 0;
            return 1d;
        }
        /// <summary>
        /// Format the string 
        /// ^the string used to format the values assigned to this unit object
        /// </summary>
        /// <param name="aUnits"></param>
        /// <param name="aUnitFamily"></param>
        /// <param name="aOverridePrecision"></param>
        /// <param name="bIncludeThousandsSeps"></param>
        /// <returns></returns>
        public string FormatString(uppUnitFamilies aUnitFamily = uppUnitFamilies.Default, int aOverridePrecision = -1, bool bIncludeThousandsSeps = false, bool bSuppressTrailingZeros = false)
        {

            if (aUnitFamily <= 0) { aUnitFamily = UnitSystem; }

            int prec = (aOverridePrecision >= 0) ? mzUtils.LimitedValue(aOverridePrecision, 0, 15) : uopUnits.UnitPrecision(UnitType, aUnitFamily);

            if (prec == 0)
            {
                return bIncludeThousandsSeps ? "#,##0" : "0";
            }
            else
            {
                if (prec == 1) bSuppressTrailingZeros = false;
                string decis = (!bSuppressTrailingZeros) ? new string('0', prec) : "0" + new string('#', prec - 1);
                return bIncludeThousandsSeps ? $"#,##0.{decis}" : $"0.{decis}";
            }

        }

        public double ConvertValue(dynamic aValue, uppUnitFamilies aFromUnits, uppUnitFamilies aToUnits = uppUnitFamilies.English, int aPrecis = -1)
        => ConvertValue(aValue, out double _, aFromUnits, aToUnits, aPrecis);


        public double ConvertValue(dynamic aValue, out double rEnglishValue, uppUnitFamilies aFromUnits, uppUnitFamilies aToUnits = uppUnitFamilies.English, int aPrecis = -1)
        {
            rEnglishValue = 0;
            if (!mzUtils.IsNumeric(aValue)) return 0;
            if (aPrecis <= 0) aPrecis = Precision(aToUnits);
            if (aPrecis >= 0) aPrecis = mzUtils.LimitedValue(aPrecis, 0, 6);
            double aVal = mzUtils.VarToDouble(aValue, aPrecis: 10);
            double rMultiplier = 1;

            if (UnitType <= 0 || UnitSystem < 0 || (aFromUnits == aToUnits))
            {
                if (aPrecis >= 0)
                    aVal = Math.Round(aVal, aPrecis, MidpointRounding.AwayFromZero);
                return aVal;
            }


            double aFactor;

            if (string.Compare("Temperature", Name, ignoreCase: true) == 0)
            {
                rMultiplier = 3.912363067;
                if (aFromUnits == uppUnitFamilies.English)
                {
                    rEnglishValue = aVal;
                    aVal = (aVal - 32) * (5 / 9);
                    rMultiplier = 1;
                }
                else
                {
                    aVal = (aVal * (9 / 5)) + 32;
                    rEnglishValue = aVal;
                    rMultiplier = MetricFactor;
                }
            }
            else
            {
                //make it English
                if (aFromUnits != uppUnitFamilies.English)
                {
                    aVal /= ConversionFactor(aFromUnits); ;
                    aVal = Math.Round(aVal, 10);
                }
                rEnglishValue = aVal;

                //make it the new units
                if (aToUnits != uppUnitFamilies.English)
                {
                    aFactor = ConversionFactor(aToUnits);


                    aVal *= aFactor;
                    aVal = Math.Round(aVal, 10);
                    rMultiplier = aFactor;
                }
            }
            if (aPrecis >= 0)
                aVal = Math.Round(aVal, aPrecis, MidpointRounding.AwayFromZero);


            return aVal;
        }

        public double ConversionFactor(uppUnitFamilies aFamily)
        {
            return aFamily switch
            {
                uppUnitFamilies.English => 1,
                uppUnitFamilies.SI => SIFactor,
                uppUnitFamilies.Metric => MetricFactor,
                _ => 1
            };


        }

        public string UnitValueString(dynamic aValue, uppUnitFamilies aToUnits, uppUnitFamilies aFromUnits = uppUnitFamilies.English, string aPrefix = null, bool bAddLabel = true, int aPrecis = -1, bool bIncludeThousandsSeps = true, bool bZeroAsNullString = false)
        {
            string _rVal;

            bool numberpassed = mzUtils.IsNumeric(aValue);
            if (numberpassed)
            {
                double rval = ConvertValue(aValue, aFromUnits, aToUnits, aPrecis);
                _rVal = rval.ToString(FormatString(aToUnits, bIncludeThousandsSeps: bIncludeThousandsSeps, aOverridePrecision: aPrecis));
                if (bZeroAsNullString && rval == 0)
                {
                    _rVal = string.Empty;
                }
                else
                {
                    if (bAddLabel) _rVal += Label(aToUnits, true);
                }



            }
            else
            {
                _rVal = (aValue != null) ? aValue.ToString() : "";
            }

            if (aPrefix != null) _rVal = aPrefix + _rVal;
            return _rVal;
        }
    }

    internal struct TUNITS : ICloneable
    {

        private bool _Init;
        private TUNIT[] _Members;
        private int _Count;
        public string Name;

        #region Constructors

        public TUNITS(string aName = "")
        {
            Name = aName;
            _Members = new TUNIT[0];
            _Init = true;
            _Count = 0;
        }

        public TUNITS(TUNIT[] aMembers, string aName = "")
        {
            _Members = aMembers;
            _Count = 0;
            _Init = aMembers != null;
            Name = aName;
            if (_Init) _Count = aMembers.Length;

        }

        #endregion

        public TUNITS Clone(bool bReturnEmpty = false)
        {

            if (!_Init) { Init(); }
            TUNIT[] mems = bReturnEmpty ? new TUNIT[0] : Force.DeepCloner.DeepClonerExtensions.DeepClone<TUNIT[]>(_Members); // (TUNIT[])_Members.Clone();
            int cnt = bReturnEmpty ? 0 : _Count;
            return new TUNITS(mems, Name);


        }

        object ICloneable.Clone() => (object)Clone();

        public int Count => (!_Init) ? Init() : _Count;

        private int Init() { _Count = 0; _Members = _Members = new TUNIT[0]; _Init = true; return 0; }

        public TUNIT Item(int aIndex)
        {
            return aIndex < 0 || aIndex > Count ? new TUNIT(uppUnitTypes.Undefined) : _Members[aIndex - 1];
        }
        public string GetUnitLabel(uppUnitTypes aType, uppUnitFamilies aUnitFamily) => GetByType(aType).GetUnitLabel(aUnitFamily);
        public double GetUnitFactor(uppUnitTypes aType, uppUnitFamilies aUnitFamily) => GetByType(aType).GetUnitFactor(aUnitFamily);

        public TUNIT GetByType(uppUnitTypes aType)
        {

            for (int i = 1; i <= Count; i++)
            {
                if (_Members[i - 1].UnitType == aType)
                { return _Members[i - 1]; }
            }
            return new TUNIT(uppUnitTypes.Undefined);
        }

        public dynamic ConvertUnits(dynamic aValue, uppUnitTypes aUnitType, uppUnitFamilies aToUnits, uppUnitFamilies aFromUnits = uppUnitFamilies.English, int aPrecis = -1)
        {
            if (aFromUnits <= uppUnitFamilies.Default)
                throw new Exception("uopUitilities.ConvertUnits. A Unit Family Must Be Passed");

            if (aToUnits <= uppUnitFamilies.Default)
                throw new Exception("uopUitilities.ConvertUnits. A Unit Family Must Be Passed");

            return GetByType(aUnitType).ConvertValue(aValue, aFromUnits, aToUnits, aPrecis);
        }

        public TUNIT Add(uppUnitTypes aType)
        {

            TUNIT aUnit;
            if ((int)aType <= (int)uppUnitTypes.Undefined) return new TUNIT(uppUnitTypes.Undefined);
            aUnit = GetByType(aType);
            if (aUnit.Index > 0) return aUnit;
            return Add(new TUNIT(aType));

        }

        public TUNIT Add(TUNIT aUnit)
        {

            { if (!_Init) { Init(); } }
            _Count += 1;
            aUnit.Index = Count;
            Array.Resize<TUNIT>(ref _Members, _Count);
            _Members[_Count - 1] = aUnit;
            return _Members[_Count - 1];
        }

        public List<TUNIT> ToList { get { if (!_Init) { Init(); } return new List<TUNIT>(_Members); } }

        #region Shared Methods

        /// <summary>
        /// Gets unit system name
        /// </summary>
        /// <param name="aFam"></param>
        /// <returns></returns>
        public static string UnitSystemName(uppUnitFamilies aFam)
        {
            string unitSysName = string.Empty;
            switch (aFam)
            {
                case uppUnitFamilies.English:
                    unitSysName = "English";
                    break;
                case uppUnitFamilies.SI:
                    unitSysName = "SI";
                    break;
                case uppUnitFamilies.Metric:
                    unitSysName = "Metric";
                    break;
            }

            return unitSysName;

        }

        public static TUNITS FromList(List<TUNIT> aList)
        {
            TUNITS _rVal = new TUNITS();
            if (aList == null) return _rVal;
            for (int i = 1; i <= aList.Count; i++) { _rVal.Add(aList[i - 1]); }
            return _rVal;
        }


        #endregion
    }


    internal struct TPIPE : ICloneable
    {
        public string Spec;
        public string Material;
        public string Schedule;
        public double WeightPerFoot;
        public string NominalDiameter;
        public double ID;
        public double OD;
        public int Count;
        public string ProjectHandle;
        public bool IsInlet;
        public int Index;

        public TPIPE(string aDescriptor, string aProject = "")
        {
            Spec = string.Empty;
            Material = string.Empty;
            Schedule = string.Empty;
            WeightPerFoot = 0;
            NominalDiameter = string.Empty;
            ID = 0;
            OD = 0;
            Count = 0;
            ProjectHandle = aProject;
            IsInlet = false;
            Index = 0;

            if (!string.IsNullOrWhiteSpace(aDescriptor)) Descriptor = aDescriptor;
        }

        public TPIPE Clone()
        {
            return new TPIPE
            {
                Spec = Spec,
                Material = Material,
                Schedule = Schedule,
                WeightPerFoot = WeightPerFoot,
                NominalDiameter = NominalDiameter,
                ID = ID,
                OD = OD,
                Count = Count,
                ProjectHandle = ProjectHandle,
                IsInlet = IsInlet,
                Index = Index
            };
        }

        /// <summary>
        ///returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>
        object ICloneable.Clone() => (object)Clone();


        public string Descriptor
        {
            get => Schedule + uopGlobals.Delim + NominalDiameter + uopGlobals.Delim + ID + uopGlobals.Delim + OD + uopGlobals.Delim + Spec + uopGlobals.Delim + Material;
            set
            {
                string aDescriptor = value?.Trim();
                if (!string.IsNullOrEmpty(aDescriptor))
                {
                    TVALUES aVals;

                    aVals = TVALUES.FromDelimitedList(aDescriptor, uopGlobals.Delim);

                    if (aVals.Count >= 1) Schedule = aVals.Item(1);


                    if (aVals.Count >= 2) NominalDiameter = aVals.Item(2);

                    if (aVals.Count >= 3) ID = mzUtils.VarToDouble(aVals.Item(3), true);

                    if (aVals.Count >= 4) OD = mzUtils.VarToDouble(aVals.Item(4), true);

                    if (aVals.Count >= 5) Spec = aVals.Item(5);

                    if (aVals.Count >= 6) Material = aVals.Item(6);

                }

            }
        }

        /// <summary>
        /// the reference name of the pipe
        /// contains nominal diameter, spec name and schedule
        /// </summary>
        public string IDDescriptor
        {
            get
            {
                string _rVal = string.Empty;
                _rVal += NominalDiameter;
                _rVal += " '' ";
                _rVal += " Nominal Pipe (";
                _rVal += ID;
                _rVal += " '' ";
                _rVal += " ID)";
                if (!string.IsNullOrEmpty(Schedule))
                {
                    _rVal += " {Schedule " + Schedule + "}";
                }
                return _rVal;
            }
        }

        public string SpecName
        {
            get
            {
                string _rVal = NominalDiameter + "''" + Spec;
                if (!string.IsNullOrWhiteSpace(Schedule)) _rVal += " (" + Schedule + ")";
                return _rVal;
            }
            set
            {
                int i = value.IndexOf("'", StringComparison.OrdinalIgnoreCase); //int i = InStr(1, value, "'", vbTextCompare);
                if (i != -1)
                {
                    NominalDiameter = mzUtils.Left(value, i).Trim();
                    value = mzUtils.Right(value, i + 2).Trim();
                }
                i = value.IndexOf("(", StringComparison.OrdinalIgnoreCase);//i = InStr(1, value, "(", vbTextCompare);

                if (i != -1)
                {
                    Spec = mzUtils.Left(value, i).Trim();
                    int j = value.IndexOf(")", StringComparison.OrdinalIgnoreCase);//InStr(1, value, ")", vbTextCompare);
                    if (j != -1)
                    {
                        Schedule = mzUtils.Mid(value, i + 1, j - i - 1).Trim();
                        Schedule = Schedule.Replace("Sch ", "");
                    }
                }

            }
        }

        public string SizeDescriptor
        => Schedule + uopGlobals.Delim + NominalDiameter + uopGlobals.Delim + ID + uopGlobals.Delim + OD;


        public static TPIPE ByDescriptor(TPIPE aBase, string aDescriptor)
        {
            TPIPE _rVal = aBase;
            aDescriptor = aDescriptor?.Trim();
            if (!string.IsNullOrEmpty(aDescriptor))
            {
                TVALUES aVals = TVALUES.FromDelimitedList(aDescriptor, uopGlobals.Delim);

                if (aVals.Count >= 1)
                {
                    _rVal.Schedule = aVals.Item(1);
                }

                if (aVals.Count >= 2)
                {
                    _rVal.NominalDiameter = aVals.Item(2);
                }
                if (aVals.Count >= 3)
                {
                    _rVal.ID = mzUtils.VarToDouble(aVals.Item(3), true);
                }
                if (aVals.Count >= 4)
                {
                    _rVal.OD = mzUtils.VarToDouble(aVals.Item(4), true);
                }
                if (aVals.Count >= 5)
                {
                    _rVal.Spec = aVals.Item(5);
                }
                if (aVals.Count >= 6)
                {
                    _rVal.Material = aVals.Item(6);
                }
            }
            return _rVal;
        }
    }

    internal struct TPIPES : ICloneable
    {


        private int _Count;
        private bool _Init;
        private TPIPE[] _Members;
        public string Family;
        public TPIPES(string aFamily = "")
        {
            _Members = new TPIPE[0];
            _Count = 0;
            _Init = true;
            Family = aFamily;
        }

        private int Init() { _Count = 0; _Members = new TPIPE[0]; _Init = true; return 0; }

        public int Count => (!_Init) ? Init() : _Count;

        public TPIPE Item(int aIndex) { return (aIndex < 1 || aIndex > Count) ? new TPIPE() : _Members[aIndex - 1]; }

        public TPIPE Add(TPIPE aPipe)
        {
            if (Count + 1 > Int32.MaxValue) { return aPipe; }

            _Count += 1;
            Array.Resize<TPIPE>(ref _Members, _Count);
            _Members[_Count - 1] = aPipe.Clone();
            return Item(Count);
        }

        object ICloneable.Clone() => (object)Clone(false);

        public void SetItem(int aIndex, TPIPE aPipe)
        {
            if (aIndex < 1 || aIndex > Count) { return; }
            _Members[aIndex - 1] = aPipe;
        }


        public TPIPES Clone(bool bReturnEmpty = false)
        {

            if (!_Init) { Init(); }
            TPIPE[] mems = bReturnEmpty ? new TPIPE[0] : Force.DeepCloner.DeepClonerExtensions.DeepClone<TPIPE[]>(_Members); //(TPIPE[])_Members.Clone();
            int cnt = bReturnEmpty ? 0 : _Count;

            return new TPIPES()
            {

                _Count = cnt,
                _Members = mems,
                _Init = true
            };



        }

        /// <summary>
        /// #1the nozzle name to search for
        /// </summary>
        /// <param name="aName"></param>
        /// <returns>returns the nozzle whose name matches the passed string</returns>
        public TPIPE GetPipeByName(string aName)
        {
            TPIPE _rVal = new TPIPE();
            if (string.IsNullOrWhiteSpace(aName)) return _rVal;
            aName = aName.Trim();

            TPIPE aMem;

            for (int i = 1; i <= Count; i++)
            {
                _Members[i - 1].Index = i;
                aMem = _Members[i - 1];
                if (string.Compare(aMem.SpecName.Substring(0, aName.Length), aName) == 0)
                {
                    return aMem;
                }
            }
            return _rVal;
        }

        public TPIPE GetByDescriptor(string aDescriptor)
        {
            int rIndex = 0;
            TPIPE _rVal = new TPIPE();
            if (string.IsNullOrWhiteSpace(aDescriptor)) return _rVal;

            aDescriptor = aDescriptor.Trim();

            TPIPE aMem;
            for (int i = 1; i <= Count; i++)
            {
                _Members[i - 1].Index = i;
                aMem = Item(i);
                if (string.Equals(aDescriptor, aMem.Descriptor, StringComparison.OrdinalIgnoreCase))
                {
                    rIndex = i;
                    _rVal = aMem;
                    break;
                }
                i++;
            }
            return _rVal;
        }

        public TPIPE GetBySizeDescriptor(string aDescriptor)
        {
            int rIndex = 0;
            TPIPE _rVal = new TPIPE();
            if (string.IsNullOrWhiteSpace(aDescriptor)) return _rVal;

            aDescriptor = aDescriptor.Trim();

            TPIPE aMem;
            for (int i = 1; i <= Count; i++)
            {
                _Members[i - 1].Index = i;
                aMem = Item(i);
                if (string.Equals(aDescriptor, aMem.SizeDescriptor, StringComparison.OrdinalIgnoreCase))
                {
                    rIndex = i;
                    _rVal = aMem;
                    break;
                }
                i++;
            }
            return _rVal;
        }

        //#1the nozzle size to search for
        //#1the nozzle schedule to search for
        //^returns the nozzle whose descriptor matches the passed string
        public TPIPE GetBySizeAndSchedule(string aSize, string aSchedule)
        {
            TPIPE _rVal = new TPIPE();
            if (string.IsNullOrWhiteSpace(aSize)) return _rVal;
            if (string.IsNullOrWhiteSpace(aSchedule)) return _rVal;
            aSize = aSize.Trim();
            aSchedule = aSchedule.Trim();

            TPIPE aMem;
            for (int i = 1; i <= Count; i++)
            {
                _Members[i - 1].Index = i;
                aMem = _Members[i - 1];
                if (string.Compare(aMem.NominalDiameter, aSize) == 0 & string.Compare(aMem.Schedule, aSchedule) == 0)
                {
                    _rVal = aMem;
                    break;
                }
            }
            return _rVal;

        }

        public TPIPES GetSubSet(string aSchedule, string aSpecName = "")
        {
            TPIPES _rVal = Clone(true);

            if (string.IsNullOrWhiteSpace(aSchedule)) return _rVal;
            aSpecName = string.IsNullOrWhiteSpace(aSpecName) ? string.Empty : aSpecName.Trim();

            TPIPE aMem;
            bool bTestSpec = false;
            aSpecName = aSpecName.Trim();
            aSchedule = aSchedule.Trim();

            bTestSpec = aSpecName != string.Empty;
            for (int i = 1; i <= Count; i++)
            {
                _Members[i - 1].Index = i;
                aMem = _Members[i - 1];
                if (!bTestSpec)
                {
                    if (string.Compare(aMem.Schedule, aSchedule, ignoreCase: true) == 0) _rVal.Add(aMem);

                }
                else
                {
                    if (string.Compare(aMem.Spec, aSpecName, ignoreCase: true) == 0)
                    {
                        if (string.Compare(aMem.Schedule, aSchedule, ignoreCase: true) == 0) _rVal.Add(aMem);

                    }
                }
            }
            return _rVal;
        }

        /// <summary>
        ///returns the unique schedules available in the collection
        /// </summary>
        /// <returns></returns>
        public List<string> GetSchedules()
        {
            List<string> _rVal = new List<string>();
            TPIPE aMem;
            string aStr = string.Empty;
            string bStr = string.Empty;
            for (int i = 1; i <= Count; i++)
            {
                aMem = _Members[i - 1];
                bStr = aMem.Schedule;
                if (bStr !=  string.Empty)
                {
                    if (!aStr.Contains("#" + bStr + "#"))
                    {
                        aStr = aStr + "#" + bStr + "#,";
                        _rVal.Add(bStr);
                    }
                }

            }
            return _rVal;
        }
    }

  

    internal struct TDOCUMENT : ICloneable
    {
        public uppDocumentTypes DocumentType;
        public int Index;
        public int SubType;
        public string Name;
        public string Category;
        public string SubCategory;
        public string RangeName;
        public string SelectText;
        public string FileName;
        public string TabName;
        public bool Invalid;
        public bool Selected;
        public bool Protected;
        public bool Requested;
        public bool Required;
        public bool IsPlaceHolder;
        public string String1;
        public string String2;
        public string String3;
        public int SketchCount;
        public bool Hidden;
        public TVALUES Data;
        public int SubPage;
        public uppUnitFamilies DisplayUnits;
        public string NodePath;
        public string NodeValue;
        public UVECTOR Center;
        public TPROPERTIES Properties;
        public string Password;
        public string ToolTip;
        public bool StandardFormat;
        public int SheetNumber;

        public TDOCUMENT(uppDocumentTypes aDocumentType = uppDocumentTypes.Undefined, uppPartTypes aPartType = uppPartTypes.Undefined)
        {
            DocumentType = aDocumentType;
            Index = 0;
            SubType = 0;
            Name = string.Empty;
            Category = string.Empty;
            SubCategory = string.Empty;
            RangeName = string.Empty;
            SelectText = string.Empty;
            FileName = string.Empty;
            Invalid = false;
            Selected = false;
            Protected = false;
            Requested = false;
            Required = false;
            String1 = string.Empty;
            String2 = string.Empty;
            String3 = string.Empty;
            SketchCount = 0;
            Hidden = false;
            Data = new TVALUES("DATA");
            SubPage = 0;
            DisplayUnits = uppUnitFamilies.English;
            NodePath = string.Empty;
            NodeValue = string.Empty;
            IsPlaceHolder = false;
            Properties = new TPROPERTIES("DOCUMENT PROPERTIES");
            Center = UVECTOR.Zero;
            Password = string.Empty;
            TabName = string.Empty;
            ToolTip = string.Empty;
            StandardFormat = false;
            SheetNumber = 1;
            if (DocumentType == uppDocumentTypes.Drawing)
            {
                Properties.AddProp("ProjectWide", false);
                Properties.AddProp("BorderScale", "");
                Properties.AddProp("Cancelable", false);
                Properties.AddProp("NoText", false);
                Properties.AddProp("OppositeHand", false);
                Properties.AddProp("Tag", "");
                Properties.AddProp("DrawTime", "");
                Properties.AddProp("WriteTime", "");
                Properties.AddProp("BorderSize", uppBorderSizes.Undefined);
                Properties.AddProp("TagAddress", "");
                Properties.AddProp("DrawingNumber", "");
                Properties.AddProp("Flag", "");
                Properties.AddProp("PasteAddress", "");
                Properties.AddProp("ExtentWidth", 0, uppUnitTypes.SmallLength);
                Properties.AddProp("SheetNumber", 0);
                Properties.AddProp("ZoomExtents", false);
                Properties.AddProp("SuppressBorder", false);




            }

            if (DocumentType == uppDocumentTypes.Report)
            {

                Properties.AddProp("AllPagesOnly", false);
                Properties.AddProp("AllRangesOnly", false);
                Properties.AddProp("MechanicalTemplate", false);
                Properties.AddProp("TemplateName", "");
                Properties.AddProp("FirstTab", "");
                Properties.AddProp("FileNameLocked", false);
                Properties.AddProp("FolderNameLocked", false);
                Properties.AddProp("MaintainRevisionHistory", false);
                Properties.AddProp("UnitsLocked", false);
                Properties.AddProp("TemplatePath", "");
            }




            if (DocumentType == uppDocumentTypes.ReportPage)
            {

                Requested = true;
                Properties.AddProp("ReportType", uppReportTypes.ReportPlaceHolder);
                Properties.AddProp("CopyCount", 0);
                Properties.AddProp("CopyIndex", 0);
                Properties.AddProp("GraphCount", 0);
                Properties.AddProp("Flag", false);
                Properties.AddProp("TabName", "");
                Properties.AddProp("TemplateTabName", "");
                Properties.AddProp("MemberList", "");
                Properties.AddProp("Tag", "");
                Properties.AddProp("TagAddress", "");
                Properties.AddProp("OnePerPage", false);
                Properties.AddProp("MaxMembers", 0);
                Properties.AddProp("StartIndex", 0);
                Properties.AddProp("EndIndex", 0);
                Properties.AddProp("NoTemplate", false);
                Properties.AddProp("DontSaveWithPassword", false);
                Properties.AddProp("SuppressTabName", false);


            }

        }

        object ICloneable.Clone() => (object)Clone();
        public TDOCUMENT Clone()
        {
            return new TDOCUMENT()
            {
                DocumentType = DocumentType,
                Index = Index,
                SubType = SubType,
                Name = Name,
                Category = Category,
                SubCategory = SubCategory,
                RangeName = RangeName,
                SelectText = SelectText,
                FileName = FileName,
                TabName = TabName,
                Invalid = Invalid,
                Selected = Selected,
                Protected = Protected,
                Requested = Requested,
                Required = Required,
                String1 = String1,
                String2 = String2,
                String3 = String3,
                SketchCount = SketchCount,
                Hidden = Hidden,
                Data = new TVALUES(Data),
                SubPage = SubPage,
                DisplayUnits = DisplayUnits,
                NodePath = NodePath,
                NodeValue = NodeValue,
                IsPlaceHolder = IsPlaceHolder,
                Center = new UVECTOR(Center),
                Properties = Properties.Clone(),
                Password = Password,
                ToolTip = ToolTip,
                StandardFormat = StandardFormat,
                SheetNumber = SheetNumber
            };
        }
    }

    public struct TVALUES : ICloneable
    {
        public string NodePath;
        public int Index;
        private int _Count;
        public string Name;
        public dynamic BaseValue;
        private bool _Init;
        private dynamic[] _Members;

        public TVALUES(string aName = "", dynamic aBaseVal = null)
        {
            Name = Convert.ToString(aName);
            Index = 0;
            _Count = 0;
            BaseValue = aBaseVal;
            _Init = true;
            NodePath = string.Empty;
            _Members = new dynamic[0];
        }
        public TVALUES(TVALUES aValues, bool bClear = false)
        {
            Name = aValues.Name;
            Index = aValues.Index;
            _Count = !bClear ? aValues.Count : 0;
            BaseValue = aValues.BaseValue;
            _Init = true;
            NodePath = aValues.NodePath;
            _Members = new dynamic[0];
            if (_Count > 0)
            {
                _Members = Force.DeepCloner.DeepClonerExtensions.DeepClone<dynamic[]>(aValues._Members);
            }

        }
        public TVALUES(List<dynamic> aList)
        {
            Name = String.Empty;
            Index = 0;
            _Count = 0;
            BaseValue = null;
            _Init = true;
            NodePath = string.Empty;
            _Members = new dynamic[0];
            if (aList != null) { DefineWithList(aList); }

        }
        public TVALUES(List<double> aList)
        {
            Name = String.Empty;
            Index = 0;
            _Count = 0;
            BaseValue = null;
            _Init = true;
            NodePath = string.Empty;
            _Members = new dynamic[0];
            if (aList != null) { DefineWithList(aList); }

        }
        public TVALUES(List<string> aList)
        {
            Name = String.Empty;
            Index = 0;
            _Count = 0;
            BaseValue = null;
            _Init = true;
            NodePath = string.Empty;
            _Members = new dynamic[0];
            if (aList != null) { DefineWithList(aList); }

        }

        public dynamic LastValue(string aDefault = "")
        {
            return Count > 0 ? Item(Count) : (dynamic)aDefault;
        }

        public int Count => (!_Init) ? Init() : _Count;

        public double MinDifference(int aPrecis = -1)
        {


            List<double> dVals = this.ToNumericList(aPrecis);

            if (dVals.Count <= 1) return 0;
            dVals.Sort();

            double _rVal = System.Double.MaxValue;
            double aVal = 0;
            double bVal = 0;
            double aDif = 0;

            for (int i = dVals.Count - 1; i >= 1; i--)
            {
                aVal = dVals[i];
                bVal = dVals[i - 1];
                aDif = Math.Abs(aVal - bVal);
                if (aDif < _rVal) _rVal = aDif;
            }
            return _rVal;
        }

        public double MaxDifference(int aPrecis = -1)
        {
            if (Count <= 1) return 0;


            List<double> dVals = this.ToNumericList(aPrecis);

            if (dVals.Count <= 1) return 0;
            dVals.Sort();

            double _rVal = 0;
            double aVal = 0;
            double bVal = 0;
            double aDif = 0;

            for (int i = dVals.Count - 1; i > 1; i--)
            {
                aVal = dVals[i];
                bVal = dVals[i - 1];
                aDif = Math.Abs(aVal - bVal);
                if (aDif > _rVal) _rVal = aDif;
            }
            return _rVal;


        }

        public double Total(bool bAbsValue = false)
        {
            double _rVal = 0;
            for (int i = 1; i <= Count; i++)
            {
                _rVal += mzUtils.VarToDouble(Value(i), bAbsValue);

            }
            return _rVal;
        }

        private int Init()
        {
            Name = string.Empty;
            Index = 0;
            _Count = 0;
            BaseValue = null;
            NodePath = string.Empty;
            _Members = new dynamic[0];
            _Init = true;
            return 0;
        }

        public bool Clear()
        {
            bool _rVal = Count > 0;
            _Count = 0;
            _Members = new dynamic[0];
            return _rVal;
        }

        public override string ToString() => "TVALUES[" + Count.ToString() + "]";


        public dynamic Remove(int aIndex)
        {
            int cnt = Count;
            if (aIndex <= 0 || aIndex > cnt || cnt <= 0) { return null; }
            dynamic _rVal = Item(aIndex);

            if (cnt == 1)
            {
                Clear();
                return _rVal;

            }
            if (cnt == aIndex)
            {
                _Count -= 1;
                Array.Resize<dynamic>(ref _Members, _Count);
                return _rVal;

            }

            _Count = 0;
            dynamic[] NewMems;
            NewMems = new dynamic[cnt - 1];

            for (int i = 1; i <= cnt; i++)
            {
                if (i != aIndex)
                {
                    _Count += 1;
                    NewMems[_Count - 1] = _Members[i - 1];
                }
            }

            return _rVal;

        }

        public TVALUES SubSet(int aMaxIndex, string aIndexList = "")
        {
            TVALUES _rVal = new TVALUES(this, true);
            for (int i = 1; i <= Count; i++)
            {
                if (i > aMaxIndex && aMaxIndex > 0) { break; }
                if (mzUtils.ListContains(i, aIndexList, bReturnTrueForNullList: true)) { _rVal.Add(Value(i)); }
            }
            return _rVal;
        }

        public TVALUES SubValues(int aStartIndex, int aEndIndex)
        {
            TVALUES _rVal = new TVALUES(this, true);
            if (Name != string.Empty) { _rVal.Name = Name + ".SubValues"; } else { _rVal.Name = "SubValues"; }
            mzUtils.LoopLimits(aStartIndex, aEndIndex, 1, Count, out int si, out int ei);
            for (int i = si; i <= ei; i++) { _rVal.Add(Value(i)); }
            return _rVal;
        }

        public void Invert()
        {
            int cnt = Count;
            if (cnt <= 1) { return; }
            dynamic[] oldMems = Force.DeepCloner.DeepClonerExtensions.DeepClone<dynamic[]>(_Members); // (dynamic[])_Members.Clone();

            int j = cnt - 1;
            for (int i = cnt; i >= 1; i--)
            {
                SetValue(i, oldMems[j]);
                j -= 1;
            }
        }

        public int Copy(TVALUES aValues, bool bShrinkTo = true, bool baddNewVals = true)
        {
            int cnt = Count;
            int iAdd = 0;
            for (int i = 1; i <= aValues.Count; i++)
            {
                if (i <= cnt)
                {
                    SetValue(i, aValues.Item(i));

                }
                else
                {
                    if (!baddNewVals) { break; }
                    if (Add(aValues.Item(i))) { iAdd += 1; }
                }
            }

            if (bShrinkTo && aValues.Count < cnt)
            {
                _Count = aValues.Count;
                Array.Resize<dynamic>(ref _Members, _Count);
            }

            return iAdd;

        }

        public void RemoveValue(dynamic aValue)
        {
            TVALUES bValues = new TVALUES(this, true);

            for (int i = 1; i <= Count; i++)
            {
                if (Value(i) != aValue)
                {
                    bValues.Add(Value(i));
                }
            }
            Copy(bValues, bShrinkTo: true);
        }

        public TVALUES RemoveDupes(bool bNumeric = false, int aPrecis = -1)
        {

            dynamic bVal = null;
            TVALUES _rVal = new TVALUES(this, true);
            TVALUES bValues = new TVALUES(this, true);

            for (int i = 1; i <= Count; i++)
            {
                dynamic aVal = Item(i);
                if (bNumeric) { aVal = mzUtils.VarToDouble(aVal, aPrecis: aPrecis); }
                bool bKeep = true;
                for (int j = i + 1; j <= Count; j++)
                {
                    bVal = Item(j);
                    if (bNumeric)
                    {
                        bVal = mzUtils.VarToDouble(bVal, aPrecis: aPrecis);
                        if (aVal == bVal)
                        {
                            bKeep = false;
                            break;
                        }
                    }
                    else
                    {
                        if (string.Compare(aVal, bVal, true) == 0)
                        {
                            bKeep = false;
                            break;
                        }

                    }
                }
                if (bKeep)
                {
                    bValues.Add(aVal);
                }
                else
                {
                    _rVal.Add(i);
                }
            }
            Copy(bValues, bShrinkTo: true);
            return _rVal;
        }

        public dynamic Value(int aIndex)
        {
            //Base ONE
            return aIndex < 1 || aIndex > Count ? (dynamic)null : _Members[aIndex - 1];
        }

        public dynamic Item(int aIndex)
        {
            //Base ONE
            return aIndex < 1 || aIndex > Count ? (dynamic)null : _Members[aIndex - 1];
        }

        public dynamic FirstVal { get => Item(1); set => SetValue(1, value); }

        public dynamic LastVal { get => Item(Count); set => SetValue(Count, value); }


        public void PrintToConsole(string aHeading = null, bool bIndexed = false)
        {

            string outs = string.Empty;
            if (!string.IsNullOrWhiteSpace(aHeading)) System.Diagnostics.Debug.WriteLine(aHeading);
            dynamic mem;
            for (int i = 1; i <= Count; i++)
            {
                mem = Item(i);
                outs = bIndexed ? i + " - " + mem.ToString() : mem.ToString();
                System.Diagnostics.Debug.WriteLine(outs);
            }
        }
        public void AddTo(int aIndex, dynamic aAdder)
        {
            if (aIndex > 0 && aIndex <= Count) { SetValue(aIndex, mzUtils.VarToDouble(Value(aIndex)) + mzUtils.VarToDouble(aAdder)); }

        }

        public void Multiply(int aIndex, double aMultiplyer)
        {
            if (aIndex > 0 && aIndex <= Count) { SetValue(aIndex, mzUtils.VarToDouble(Value(aIndex)) * aMultiplyer); }

        }

        public bool SetValue(int aIndex, dynamic aValue)
        {
            if (aIndex <= 0 || aIndex > Count || aValue == null) { return false; }
            string s1 = _Members[aIndex - 1].ToString();
            string s2 = aValue.ToString();

            bool _rVal = s1 != s2;
            _Members[aIndex - 1] = aValue;
            return _rVal;
        }


        public bool SetItem(int aIndex, dynamic aValue)
        {
            //!BASE ZERO

            if (aIndex < 0 || aIndex >= Count || aValue == null) { return false; }
            _Members[aIndex] = aValue;
            return true;
        }

        public bool SetCount(dynamic aCount)
        {
            if (aCount == null) { return false; }
            int aInt = mzUtils.VarToInteger(aCount);
            if (aInt == Count) { return false; }
            if (aInt <= 0)
            {
                Clear();
                return true;
            }

            if (aInt < Count)
            {
                _Count = aInt;
                Array.Resize<dynamic>(ref _Members, _Count);


            }
            else
            {
                int startIndex = Count + 1;

                for (int i = startIndex; i <= aInt; i++)
                {

                    Add("");
                }
            }
            return true;


        }


        public bool SetValues(dynamic aValue, string aSkipIDs = "", dynamic aNewCount = null)
        {
            bool _rVal = false;
            if (aNewCount != null) { SetCount(aNewCount); }
            for (int i = 1; i <= Count; i++)
            {
                if (!mzUtils.ListContains(i, aSkipIDs, bReturnTrueForNullList: true))
                {
                    if (Value(i) != aValue) { _rVal = true; }
                    SetValue(i, aValue);
                }

            }
            return _rVal;
        }

        public bool MoveValue(int aIndex, int aToIndex)
        {
            if (aIndex < 1 || aIndex > Count) { return false; }
            if (aToIndex < 1) { aToIndex = 1; }
            if (aToIndex > Count) { aToIndex = Count; }
            if (aIndex == aToIndex) { return false; }

            try
            {
                TVALUES nVals = new TVALUES(this);
                dynamic aVal = Item(aIndex);

                if (aIndex > aToIndex)
                { //moving up
                    for (int i = 1; i <= Count; i++)
                    {
                        if (i < aToIndex)
                        { nVals.SetValue(i, Item(i)); }
                        else
                        {
                            if (i == aToIndex)
                            { nVals.SetValue(i, aVal); }
                            else
                            { nVals.SetValue(i, Item(i - 1)); }
                        }
                    }
                }
                else
                { //moving down
                    for (int i = 1; i <= Count; i++)
                    {
                        if (i > aToIndex)
                        { nVals.SetValue(i, Item(i - 1)); }
                        else
                        {
                            if (i == aIndex)
                            { nVals.SetValue(i, aVal); }
                            else
                            { nVals.SetValue(i, Item(i)); }
                        }
                    }
                }
                DefineWithList(nVals.ToList);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public int Fill(dynamic aValue, int aTotalCount)
        {
            if (aValue == null || aTotalCount <= 0 || aTotalCount <= Count) return 0;
            int _rVal = 0;
            while (Count < aTotalCount) { Add(aValue); _rVal += 1; }
            return _rVal;
        }

        public bool Add(dynamic aValue, bool bNoDupes = false)
        {
            if (aValue == null) { return false; }
            if (Count + 1 >= Int32.MaxValue) { return false; }

            if (bNoDupes)
            {
                if (ContainsValue(aValue))
                    return false;
            }
            _Count += 1;
            Array.Resize<dynamic>(ref _Members, _Count);
            _Members[_Count - 1] = aValue;
            return true;
        }

        public bool AddNumber(dynamic aValue, bool bNoDupes = false, int aPrecis = 5)
        {


            double aVal = mzUtils.VarToDouble(aValue, false, aPrecis: aPrecis);


            if (bNoDupes)
            {
                if (FindNumericValue(aVal, aPrecis) > 0) { return false; }
            }

            return Add(aVal);

        }

        public bool AddByString(string aList, string aDelim = ",", bool bNoDupes = false, bool bTrim = true)
        {
            if (string.IsNullOrEmpty(aList)) { return false; }
            if (!_Init) { Init(); }
            return Append(TVALUES.FromDelimitedList(aList, aDelimitor: aDelim, bReturnNulls: false, bTrim: bTrim, bNoDupes: bNoDupes)) > 0;

        }

        public bool ContainsValue(dynamic aValue, bool bStringCompare = false, bool ignoreCase = true)
        {
            for (int i = 1; i <= Count; i++)
            {
                if (!bStringCompare)
                {
                    if (Value(i) == aValue) { return true; }

                }

                else
                {
                    if (String.Compare(aValue, Item(i), ignoreCase: ignoreCase)) { return true; }
                }
            }
            return false;
        }

        /// <summary>
        ///returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>
        object ICloneable.Clone() => (object)Clone(false);

        public TVALUES Clone(bool bReturnEmpty = false) => new TVALUES(this, bReturnEmpty);


        public int Append(TVALUES aValues, bool bNoDupes = false)
        {
            int _rVal = 0;
            if (aValues.Count <= 0) { return 0; }
            for (int i = 1; i <= aValues.Count; i++)
            {
                if (Add(aValues.Item(i), bNoDupes: bNoDupes)) { _rVal += 1; }
            }

            return _rVal;
        }

        public List<dynamic> ToList { get { if (!_Init) { Init(); } { return new List<dynamic>(_Members); } } }

        public List<double> ToNumericList(int aPrecis = -1, mzSortOrders Sort = mzSortOrders.None, bool bNoDupes = false, double? aAdder = null)
        {
            if (!_Init) Init();
            List<double> _rVal = new List<double>();
            dynamic aVal;
            int precis = aPrecis >= 0 ? mzUtils.LimitedValue(aPrecis, 0, 15): -1 ;
            for (int i = 1; i <= Count; i++)
            {
                aVal = _Members[i - 1];
                double dval = mzUtils.VarToDouble(aVal, aPrecis: precis);
                if(aAdder.HasValue)
                {
                    dval = precis >= 0 ? Math.Round(dval + aAdder.Value, precis) : dval + aAdder.Value;
                }
                if (bNoDupes && _rVal.FindIndex(x => x == dval) >= 0) continue;
                _rVal.Add(dval);
            }
            if (Sort == mzSortOrders.LowToHigh || Sort == mzSortOrders.HighToLow)
            {
                _rVal.Sort();
                if (Sort == mzSortOrders.HighToLow) _rVal.Reverse();
            }
            return _rVal;
        }

        public List<string> ToStringList(mzSortOrders Sort = mzSortOrders.None)
        {
            if (!_Init) Init();
            List<string> _rVal = new List<string>();
            string aVal;
            for (int i = 1; i <= Count; i++)
            {
                aVal = _Members[i - 1].ToString();
                if (!string.IsNullOrWhiteSpace(aVal)) _rVal.Add(aVal);
            }
            if (Sort == mzSortOrders.LowToHigh || Sort == mzSortOrders.HighToLow)
            {
                _rVal.Sort();
                if (Sort == mzSortOrders.HighToLow) _rVal.Reverse();
            }
            return _rVal;
        }

        public List<string> ToStringList(bool bNoNulls = true, bool bUniqueVals = true)
        {
            if (!_Init) Init();
            List<string> _rVal = new List<string>();
            dynamic aVal;
            string sVal;
            for (int i = 1; i <= Count; i++)
            {
                aVal = _Members[i - 1];
                sVal = (aVal != null) ? aVal.ToString() : "";
                if (!bNoNulls || (bNoNulls & !string.IsNullOrWhiteSpace(sVal)))
                {
                    if (bUniqueVals)
                    {
                        if (_rVal.FindIndex(x => string.Compare(x, sVal, ignoreCase: true) == 0) < 0) _rVal.Add(sVal);
                    }
                    else { _rVal.Add(sVal); }

                }
            }

            return _rVal;
        }

        public void AddToList(List<object> aList) { if (aList == null) { return; } for (int i = 1; i <= Count; i++) { aList.Add(Value(i)); } }


        public string ToDelimitedList(string aDelim, bool bNoNulls = false, string aLastDelim = "", int aMaxIndex = 0, string aIndexList = "", bool bNoDupes = false)
        {
            if (Count <= 0) { return ""; }

            return ToDelimitedList(aDelim, out int _, bNoNulls, aLastDelim, aMaxIndex, aIndexList, bNoDupes);
        }

        internal void Print(bool bBaseZero)
        {
            string sVal = string.Empty;
            dynamic aVal;
            for (int i = 1; i <= Count; i++)
            {
                aVal = Value(i);
                sVal = bBaseZero ? (i - 1).ToString() : i.ToString();
                if (aVal == null) sVal += " Is Null"; else sVal += Convert.ToString(aVal);
                System.Diagnostics.Debug.WriteLine(sVal);
            }

        }

        public string ToDelimitedList(string aDelim, out int rListCount, bool bNoNulls = false, string aLastDelim = "", int aMaxIndex = 0, string aIndexList = "", bool bNoDupes = false)
        {
            string _rVal = string.Empty;
            rListCount = 0;
            if (Count <= 0) { return ""; }


            if (!bNoNulls && aMaxIndex <= 0)
            { _rVal = string.Join(aDelim, ToList); }
            else
            {
                for (int i = 1; i <= Count; i++)
                {
                    if (i > aMaxIndex && aMaxIndex > 0) { break; }
                    if (mzUtils.ListContains(i, aIndexList, bReturnTrueForNullList: true))
                    {
                        if (mzUtils.ListAdd(ref _rVal, Item(i), bSuppressTest: !bNoDupes, aDelimitor: aDelim, bAllowNulls: !bNoNulls))
                        { rListCount += 1; }
                    }
                }
            }
            if (!string.IsNullOrWhiteSpace(aLastDelim) && rListCount > 1)
            {
                int i = _rVal.LastIndexOf(aDelim);
                if (i != -1) { _rVal = _rVal.Substring(0, i) + aLastDelim + _rVal.Substring(i + 3 + aLastDelim.Length); }
            }
            return _rVal;
        }

        public dynamic ExtremeValue(bool bMax, bool bAbs, out int rIndex)
        {
            dynamic _rVal = null;

            rIndex = 0;
            dynamic bVal = null;
            double dblVal = 0;

            for (int i = 1; i <= Count; i++)
            {

                dblVal = mzUtils.VarToDouble(Value(i), bAbsoluteVal: bAbs, aPrecis: 6);

                if (i == 1)
                {

                    bVal = dblVal;
                    rIndex = i;
                }
                else
                {


                    if (bMax)
                    {
                        if (dblVal > bVal) { bVal = dblVal; rIndex = i; }
                    }
                    else
                    {
                        if (dblVal < bVal) { bVal = dblVal; rIndex = i; }
                    }
                }
            }
            if (rIndex > 0) { _rVal = Item(rIndex); }
            return _rVal;
        }

        public int ExtremeValueIndex(bool bMax, bool bAbs = false)
        {
            ExtremeValue(bMax, bAbs, out int idx);
            return idx;
        }

        public TVALUES UniqueValues(bool bNumeric = false, int aPrecis = -1)
        {
            TVALUES _rVal = new TVALUES(this, true);
            if (Count <= 0) return _rVal;

            _rVal.Add(Value(1));
            if (Count <= 1) return _rVal;

            dynamic aVal;
            int idx = 0;

            for (int i = 2; i <= Count; i++)
            {
                aVal = Item(i);
                if (!bNumeric)
                {
                    idx = _rVal.FindStringValue(aVal, aOccur: 2);
                }
                else
                {
                    idx = _rVal.FindNumericValue(aVal, aPrecis, 2);
                }
                if (idx <= 0)
                {
                    _rVal.Add(aVal);
                }
            }
            return _rVal;
        }

        public void DefineWithList(List<dynamic> aList)
        {
            Clear();
            if (aList == null) { return; }
            for (int i = 1; i <= aList.Count; i++) { Add(aList[i - 1]); }

        }

        public void DefineWithList(List<double> aList)
        {
            Clear();
            if (aList == null) { return; }
            for (int i = 1; i <= aList.Count; i++) { Add(aList[i - 1]); }

        }

        public void DefineWithList(List<string> aList)
        {
            Clear();
            if (aList == null) { return; }
            for (int i = 1; i <= aList.Count; i++) { Add(aList[i - 1]); }
        }

        public int FindStringValue(dynamic aStringVal, string aLimitChar = "", int aOccur = 0, int iStart = 1, bool bStartsWith = false, bool ignoreCase = true)
        {
            if (aOccur <= 0) { aOccur = 1; }

            int cnt = 0;
            string bStr = string.Empty;
            int j = 0;
            string aStr = string.Empty;
            int llen = 0;
            aStr = Convert.ToString(aStringVal);
            if (bStartsWith)
            {
                llen = aStr.Length;
                if (llen <= 0)
                {
                    return 0;
                }
            }
            for (int i = iStart; i <= Count; i++)
            {
                bStr = Convert.ToString(Value(i));
                if (aLimitChar !=  string.Empty)
                {
                    j = bStr.IndexOf(aLimitChar, StringComparison.OrdinalIgnoreCase);
                    if (j != -1) { bStr = bStr.Substring(0, j); }
                }
                if (bStartsWith)
                {

                    if (bStr.Length >= llen)
                    {
                        if (string.Compare(bStr.Substring(0, llen), aStr, ignoreCase) == 0)
                        {
                            cnt++;
                            if (cnt == aOccur)
                            {
                                return i;
                            }
                        }
                    }
                }
                else
                {
                    if (string.Compare(bStr, aStr, ignoreCase) == 0)
                    {
                        cnt++;
                        if (cnt == aOccur)
                        {
                            return i;
                        }
                    }
                }
            }
            return 0;
        }

        public int FindNumericValue(dynamic aValue, int aPrecis = -1, int aOccur = 0, int iStart = 1)
        {
            if (aOccur <= 0) { aOccur = 1; }
            int cnt = 0;
            dynamic aVal = mzUtils.VarToDouble(aValue, aPrecis: aPrecis);
            dynamic bVal = null;

            for (int i = iStart; i <= Count; i++)
            {
                bVal = mzUtils.VarToDouble(Value(i), aPrecis: aPrecis);
                if (aVal == bVal)
                {
                    cnt++;
                    if (cnt == aOccur)
                    {
                        return i;
                    }
                }

            }
            return 0;
        }

        public void Sort(bool bHighToLow = false, bool bRemoveDupes = false)

        {
            if (Count <= 1) return;
            if (bRemoveDupes) RemoveDupes(false);
            List<dynamic> l1 = ToList;
            l1.Sort();
            if (bHighToLow) { l1.Reverse(); }
            DefineWithList(l1);
        }

        //^sorts the values in the array from low to high
        //~returns the indexs of the old values in the order they were moved to in the new order.
        public TVALUES SortWithIDs(bool bReturnBaseOne, bool bHighToLow = false, bool bConvertToDoubles = false, bool bRemoveDupes = false, dynamic bNumeric = null, int iPrecis = -1)
        {
            TVALUES _rVal = new TVALUES("Indices");

            if (Count <= 0) return _rVal;


            bool bNums = bConvertToDoubles;
            if (bNumeric != null && !bNums)
            { bNums = mzUtils.VarToBoolean(bNumeric); }
            else
            { if (Count > 0) bNums = mzUtils.IsNumeric(Value(1)); }
            if (bConvertToDoubles) ConvertToDoubles();
            if (bRemoveDupes) RemoveDupes(bNums);

            if (Count == 1)
            {
                if (bReturnBaseOne) { _rVal.Add(1); } else { _rVal.Add(0); }
                return _rVal;
            }


            List<int> lIds = TVALUES.SortIDs(ref this, bHighToLow: bHighToLow, bBaseOne: bReturnBaseOne);
            for (int i = 0; i < lIds.Count; i++)
            {
                _rVal.Add(lIds[i]);
            }

            return _rVal;
        }



        public void SortNumeric(bool bHighToLow = false, bool bRemoveDupes = false, int aPrecis = -1)
        {
            ConvertToDoubles(aPrecis, bRemoveDupes);
            Sort(bHighToLow);
        }

        public List<double> ConvertToDoubles(int aPrecis = -1, bool bRemoveDupes = false)
        {
            if (bRemoveDupes) RemoveDupes(true, aPrecis);
            List<double> _rVal = new List<double>();
            double dVal;
            for (int i = 1; i <= Count; i++)
            {
                dVal = mzUtils.VarToDouble(Value(i), aPrecis: aPrecis);
                _Members[i - 1] = dVal;
                _rVal.Add(dVal);
            }
            //if (bRemoveDupes) { RemoveDupes(true, aPrecis); _rVal = ConvertToDoubles(aPrecis, false); }

            return _rVal;
        }

        public void Reverse()
        {
            if (Count <= 1) { return; }
            List<dynamic> l1 = ToList;
            l1.Reverse();
            DefineWithList(l1);
        }

        public int ValueCount(dynamic aSearchVal, bool bNumeric = false, int aPrecis = 0)
        {
            int _rVal = 0;
            dynamic sVal;
            if (bNumeric)
            {
                aPrecis = mzUtils.LimitedValue(aPrecis, 0, 15);

                sVal = mzUtils.VarToDouble(aSearchVal, aPrecis: aPrecis);
            }
            else
            {
                sVal = Convert.ToString(aSearchVal);
            }
            dynamic aVal;

            for (int i = 1; i <= Count; i++)
            {
                aVal = Item(i);
                if (!bNumeric)
                {
                    if (string.Compare(aVal, sVal, true) == 0) { _rVal += 1; }
                }
                else
                {
                    aVal = mzUtils.VarToDouble(aVal, aPrecis: aPrecis);
                    if (aVal == sVal) { _rVal += 1; }
                }
            }
            return _rVal;
        }

        public TVALUES MatchingStringValues(TVALUES bValues, bool ignoreCase = true, bool bailOnOne = false)
        {
            TVALUES _rVal = new TVALUES("MatchingValues");
            for (int i = 1; i <= Count; i++)
            {
                if (i > bValues.Count) { break; }

                if (string.Compare(Value(i), bValues.Item(i), ignoreCase) == 0)
                {
                    _rVal.Add(Value(i));
                }
                else { if (bailOnOne) { break; } }

            }
            return _rVal;
        }

        /*
     '#1the subject values
     '#2the lower bound to apply
     '#3the upper bound to apply
     '#4flag indicating if a value on a bound should be considered withing the range
     '#5returns the indices of the members in the range
     '#6a precision to apply
     '^returns the values that fall within the passed limits*/
        /*
   '#1the subject values
   '#2the lower bound to apply
   '#3the upper bound to apply
   '#4flag indicating if a value on a bound should be considered withing the range
   '#5a precision to apply
   '^returns the values that fall within the passed limits*/

        public TVALUES GetInRange(dynamic aLower, dynamic aUpper, bool bOnisIn, int aPrecis = -1)
        {

            TVALUES rIndices = new TVALUES("INDICES");

            return GetInRange(aLower, aUpper, bOnisIn, out rIndices, aPrecis);
        }

        public TVALUES GetInRange(dynamic aLower, dynamic aUpper, bool bOnisIn, out TVALUES rIndices, int aPrecis = -1)
        {
            int i, lVal, uVal;
            double aVal;
            TVALUES _rVal = new TVALUES(this, true);
            rIndices = new TVALUES("INDICES");
            aPrecis = mzUtils.LimitedValue(aPrecis, 10, 15);
            uVal = mzUtils.VarToDouble(aUpper, false, aPrecis: aPrecis);
            lVal = mzUtils.VarToDouble(aLower, false, aPrecis: aPrecis);
            mzUtils.SortTwoValues(LowToHigh: true, ref lVal, ref uVal);


            for (i = 1; i <= Count; i++)
            {
                aVal = mzUtils.VarToDouble(Value(i), false, aPrecis: aPrecis);
                //suspicious logic
                if (bOnisIn)
                {
                    if (aVal >= lVal && aVal <= uVal)
                    {
                        rIndices.Add(i);
                        _rVal.Add(Value(i));
                    }
                }
                else
                {
                    if (aVal > lVal && aVal < uVal)
                    {
                        rIndices.Add(i);
                        _rVal.Add(Value(i));
                    }
                }
            }
            return _rVal;
        }


        public bool RemoveStringValue(dynamic aValue, bool bJustOne = false, bool ignoreCase = true)
        {
            bool _rVal = false;
            ;
            int i = 0;
            TVALUES aVals = new TVALUES(this, true);
            for (i = 1; i <= Count; i++)
            {
                if (string.Compare(Value(i), aValue, ignoreCase) != 0)
                {
                    aVals.Add(Value(i));
                    _rVal = true;
                    if (bJustOne) { break; }

                }
            }
            Clear();
            Append(aVals);
            return _rVal;
        }


        public int Occurances(dynamic aValue, bool bNumeric = false, int aPrecis = -1)
        {
            int _rVal = 0;
            int i = 0;
            if (bNumeric)
            {
                aValue = mzUtils.VarToDouble(aValue, aPrecis: aPrecis);
            }
            for (i = 1; i <= Count; i++)
            {
                if (!bNumeric)
                {
                    if (string.Compare(Value(i), aValue, true) == 0)
                    {
                        _rVal += 1;
                    }

                }
                else
                {
                    if (mzUtils.VarToDouble(Value(i), aPrecis: aPrecis) == aValue)
                    {
                        _rVal += 1;
                    }
                }
            }
            return _rVal;
        }


        #region "Shared Methods"
        public static TVALUES Null => new TVALUES("");

        public static List<string> ToStringList(TVALUES aValues)
        {
            List<string> _rVal = new List<string>();
            for (int i = 1; i <= aValues.Count; i++)
            { _rVal.Add(Convert.ToString(aValues.Item(i))); }
            return _rVal;
        }
        public static TVALUES FromStringList(List<string> aStrings, bool bSkipWhiteSpace = false)
        {
            TVALUES _rVal = new TVALUES();
            if (aStrings == null) return _rVal;
            string str;
            for (int i = 0; i < aStrings.Count; i++)
            {
                str = aStrings[i];
                if (bSkipWhiteSpace || (!bSkipWhiteSpace && !string.IsNullOrWhiteSpace(str)))
                {
                    _rVal.Add(str);
                }
            }
            return _rVal;
        }

        public static TVALUES FromList<T>(List<T> aValues, bool bNoDupes = false)
        {
            TVALUES _rVal = new TVALUES();
            if (aValues == null) return _rVal;
            dynamic dval;
            bool keep = false;
            for (int i = 0; i < aValues.Count; i++)
            {
                dval = aValues[i];
                keep = true;
                if (bNoDupes)
                {
                    for (int j = 1; j <= _rVal.Count; j++)
                    {
                        if (_rVal.Item(j) == dval)
                        {
                            keep = false; break;
                        }
                    }

                }

                if (keep) _rVal.Add(dval);

            }
            return _rVal;
        }

        public static bool CompareNumeric(TVALUES aValues, TVALUES bValues, int aPrecis = 6) => TVALUES.CompareNumeric(aValues, bValues, out TVALUES _, aPrecis, bBailOnOne: true);


        public static bool CompareNumeric(TVALUES aValues, TVALUES bValues, out TVALUES rMisMatches, int aPrecis = 6, bool bBailOnOne = false)
        {

            rMisMatches = new TVALUES("DIFFERENCES");
            if (aValues.Count <= 0) { return false; }
            if (aValues.Count != bValues.Count) { return false; }
            bool _rVal = true;
            double aVal;
            for (int i = 1; i <= aValues.Count; i++)
            {
                aVal = mzUtils.VarToDouble(aValues.Item(i), aPrecis: aPrecis);

                if (bValues.FindNumericValue(aVal, aPrecis) <= 0)
                {
                    _rVal = false;
                    rMisMatches.Add(i);
                    if (bBailOnOne) { break; }
                }
            }
            return _rVal;
        }
        public static bool CompareNumeric(double aValue, double bValue, int aPrecis = 6) => TVALUES.CompareNumeric(new List<double> { aValue }, new List<double> { bValue }, out List<double> _, aPrecis, bBailOnOne: true);

        public static bool CompareNumeric(List<double> aValues, List<double> bValues, int aPrecis = 6) => TVALUES.CompareNumeric(aValues, bValues, out List<double> _, aPrecis, bBailOnOne: true);

        public static bool CompareNumeric(List<double> aValues, List<double> bValues, out List<double> rMisMatches, int aPrecis = 6, bool bBailOnOne = false)
        {

            rMisMatches = new List<double>();
            if (aValues == null || bValues == null) return false;

            if (aValues.Count <= 0) return false;
            if (aValues.Count != bValues.Count) return false;
            bool _rVal = true;
            double aVal;
            aPrecis = mzUtils.LimitedValue(aPrecis, 0, 15, 15);
            for (int i = 1; i <= aValues.Count; i++)
            {
                aVal = Math.Round(aValues[i - 1], aPrecis);

                if (bValues.FindIndex(x => Math.Round(x, aPrecis) == aVal) < 0)
                {
                    _rVal = false;
                    rMisMatches.Add(i);
                    if (bBailOnOne) { break; }
                }
            }
            return _rVal;
        }

        public static bool CompareStrings(TVALUES aValues, TVALUES bValues, string skipList = "", string aDelimitor = ",", bool ignoreCase = true)
        {

            TVALUES rMisMatches = new TVALUES("DIFFERENCES");
            return CompareStrings(aValues, bValues, out rMisMatches, skipList: skipList, aDelimitor: aDelimitor, ignoreCase: ignoreCase, bBailOnOne: true);
        }

        public static bool CompareStrings(TVALUES aValues, TVALUES bValues, out TVALUES rMisMatches, string skipList = "", string aDelimitor = ",", bool ignoreCase = true, bool bBailOnOne = false)
        {

            rMisMatches = new TVALUES("DIFFERENCES");
            if (aValues.Count <= 0) { return false; }
            if (aValues.Count != bValues.Count) { return false; }
            bool _rVal = true;
            string aVal;
            for (int i = 1; i <= aValues.Count; i++)
            {
                aVal = Convert.ToString(aValues.Item(i));
                if (!mzUtils.ListContains(aVal, skipList, aDelimitor: aDelimitor, bReturnTrueForNullList: true))
                {
                    if (bValues.FindStringValue(aVal, ignoreCase: ignoreCase) <= 0)
                    {
                        _rVal = false;
                        rMisMatches.Add(i);
                        if (bBailOnOne) { break; }
                    }
                }

            }
            return _rVal;
        }

        public static TVALUES FromDelimitedList(dynamic aList, string aDelimitor = ",", bool bReturnNulls = false, bool bTrim = false, bool bNoDupes = false, bool bNumbersOnly = false, int iPrecis = -1, bool bRemoveParens = false, string aSkipList = null)
        {
            TVALUES _rVal = new TVALUES("LIST_VALUES");
            string aStr = aList?.ToString().Trim();
            if (bRemoveParens) { aStr = aStr.Replace("(", "").Replace(")", "").Trim(); }

            if (string.IsNullOrEmpty(aStr)) return _rVal;

            dynamic sVal;
            bool bKeep;
            double dVal = 0;
            dynamic rVal;
            if (aList == null) return _rVal;

            aSkipList = string.IsNullOrWhiteSpace(aSkipList) ? string.Empty : aSkipList.Trim();
            if (aDelimitor == uopGlobals.Delim)
                aStr = mzUtils.FixGlobalDelimError(aStr);

            string[] sVals = aStr.Split(aDelimitor.ToCharArray());

            if (bNumbersOnly) { bReturnNulls = false; }

            for (int i = 0; i < sVals.Length; i++)
            {
                bKeep = !mzUtils.ListContains(i + 1, aSkipList, bReturnTrueForNullList: false);

                if (bKeep)
                {
                    sVal = sVals[i];
                    if (bTrim || bNumbersOnly) { sVal = sVal.Trim(); }
                    if (!bReturnNulls && sVal.Trim() ==  string.Empty) { bKeep = false; }

                    if (bKeep && bNumbersOnly)
                    {
                        if (!mzUtils.IsNumeric(sVal) || sVal ==  string.Empty)
                        { bKeep = false; }
                        else
                        {
                            dVal = mzUtils.VarToDouble(sVal, aPrecis: iPrecis);
                            sVal = dVal;
                        }
                    }

                    if (bKeep && bNoDupes)
                    {
                        for (int j = 1; j <= _rVal.Count; j++)
                        {
                            rVal = _rVal.Item(j);
                            if (bNumbersOnly)
                            {
                                if (dVal == mzUtils.VarToDouble(rVal, aPrecis: iPrecis))
                                {
                                    bKeep = false;
                                    break;
                                }

                            }
                            else
                            {
                                if (sVal == rVal)
                                {
                                    bKeep = false;
                                    break;
                                }
                            }
                        }
                    }

                    if (bKeep) { _rVal.Add(sVal); }
                }
            }
            return _rVal;
        }

        public static List<int> SortIDs(ref TVALUES aValues, bool bHighToLow = false, bool bBaseOne = false)
        {
            List<dynamic> Mems = aValues.ToList;
            List<int> _rVal = mzUtils.SortDynamicList(Mems, bHighToLow, bBaseOne);

            aValues.DefineWithList(Mems);
            return _rVal;
        }

        public static TVALUES ParseCommaString(string aString)
        {
            TVALUES _rVal = new TVALUES("ParseCommaString");
            aString = aString.Trim();
            if (string.IsNullOrEmpty(aString)) return _rVal;
            int i = 0;
            string aStr = string.Empty;
            int iQuote = 0;
            string aCr = string.Empty;
            iQuote = 1;
            for (i = 0; i < aString.Length; i++)
            {
                aCr = aString.Substring(i, 1);
                if ((int)Convert.ToChar(aCr) == 34)
                {
                    iQuote++;
                }
                else if ((int)Convert.ToChar(aCr) == 44)
                {
                    if (iQuote % 2 == 1)
                    {
                        _rVal.Add(aStr.Trim());
                        aStr = string.Empty;
                        iQuote = 1;
                    }
                    else
                    {
                        aStr += aCr;
                    }

                }
                else
                {
                    aStr += aCr;
                }
            }
            if (_rVal.Count <= 0)
            {
                _rVal.Add(aString);
            }
            return _rVal;
        }

        public static string GetDynamicTypeName(dynamic aValue)
        {

            if (aValue == null) return "";
            string _rVal = mzUtils.GetLastString(aValue.GetType().ToString(), '.').ToUpper();
            if (_rVal == "BOOLEAN") _rVal = "BOOL";
            return _rVal;
        }



        public static ArrayList ToArrayList(TVALUES aValues)
        {
            ArrayList _rVal = new ArrayList();
            for (int i = 1; i <= aValues.Count; i++)
            { _rVal.Add(aValues.Item(i)); }
            return _rVal;
        }

        public static string GetTypeName(Type type)
        {

            string tname = type.Name;
            int i = tname.LastIndexOf(".");
            if (i >= 0) { tname = tname.Substring(i + 1); }
            return tname.ToUpper();

        }
        #endregion
    }

    internal struct UVECTOR : ICloneable
    {

        #region Fields

        public double X;
        public double Y;
        public double Value;
        public int Row;
        public int Col;
        public int PartIndex;
        public int Index;
        public string Tag;
        public string Flag;
        public bool Suppressed;
        public double Radius;
        public double Proximity;
        public double Inset;
        public double DownSet;
        public double? Elevation;
        public bool Mark;
        public bool Virtual;
        #endregion Fields

        #region Constructors

        public UVECTOR(double aX = 0, double aY = 0, double aValue = 0, double aRadius = 0, double aInset = 0, double aDownSet = 0, double? aElevation = null, bool bNormalize = false, bool bVirtual = false)
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
            Mark = false;
            Virtual = false;
            if (bNormalize) Normalize();
        }
        public UVECTOR(dxfDirection aDirection)
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
            Elevation = 0;
            Mark = false;
            Virtual = false;
            if (aDirection == null) return;

            X = aDirection.X;
            Y = aDirection.Y;
            Tag = aDirection.Tag;
            Flag = aDirection.Flag;
           
        }

        public UVECTOR(iVector aVector, bool bNormalize = false)
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
            Elevation = 0;
            Mark = false;
            Virtual = false;
            if (aVector == null) return;

            X = aVector.X;
            Y = aVector.Y;

            uopVector u1 = new uopVector(aVector);

            Value = u1.Value;
            Row = u1.Row;
            Col = u1.Col;
            PartIndex = u1.PartIndex;
            Index = u1.Index;
            Tag = u1.Tag;
            Flag = u1.Flag;
            Suppressed = u1.Suppressed;
            Radius = u1.Radius;
            Proximity = u1.Proximity;
            Inset = u1.Inset;
            DownSet = u1.DownSet;
            Elevation = u1.Elevation;
            Mark = u1.Mark;
            if (bNormalize) Normalize();

        }

        internal UVECTOR(UVECTOR aVector, bool bNormalize = false, double? aRadius = null)
        {

            X = aVector.X;
            Y = aVector.Y;
            Value = aVector.Value;
            Row = aVector.Row;
            Col = aVector.Col;
            PartIndex = aVector.PartIndex;
            Index = aVector.Index;
            Tag = aVector.Tag;
            Flag = aVector.Flag;
            Suppressed = aVector.Suppressed;
            Radius = aVector.Radius;
            Proximity = aVector.Proximity;
            Inset = aVector.Inset;
            DownSet = aVector.DownSet;
            Elevation = aVector.Elevation;
            Mark = aVector.Mark;
            Virtual = aVector.Virtual;
            if (bNormalize) Normalize();
            if (aRadius.HasValue) Radius = aRadius.Value;
        }

        internal UVECTOR(UVECTOR? aVector, bool bNormalize = false, double? aRadius = null)
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
            Elevation = 0;
            Mark = false;
            Virtual = false;

            if (aRadius.HasValue) Radius = aRadius.Value;
            if (!aVector.HasValue) return;

            X = aVector.Value.X;
            Y = aVector.Value.Y;

            uopVector u1 = new uopVector(aVector.Value);

            Value = u1.Value;
            Row = u1.Row;
            Col = u1.Col;
            PartIndex = u1.PartIndex;
            Index = u1.Index;
            Tag = u1.Tag;
            Flag = u1.Flag;
            Suppressed = u1.Suppressed;
            Radius = u1.Radius;
            Proximity = u1.Proximity;
            Inset = u1.Inset;
            DownSet = u1.DownSet;
            Elevation = u1.Elevation;
            Mark = u1.Mark;
            Virtual = u1.Virtual;
            if (bNormalize) Normalize();
        }
        internal UVECTOR(UHOLE aHole)
        {

            X = aHole.X;
            Y = aHole.Y;
            Value = aHole.Value;
            Row = aHole.Center.Row;
            Col = aHole.Center.Col;
            PartIndex = aHole.Center.PartIndex;
            Index = aHole.Index;
            Tag = aHole.Tag;
            Flag = aHole.Flag;
            Suppressed = aHole.Suppressed;
            Radius = aHole.Radius;
            Proximity = aHole.Center.Proximity;
            Inset = aHole.Inset;
            DownSet = aHole.DownSet;
            Elevation = aHole.Elevation;
            Mark = aHole.Center.Mark;
            Virtual = aHole.Center.Virtual;
        }

        public UVECTOR(dxfPlane aPlane, double aX  = 0, double aY = 0, string aTag = "", string aFlag = "",  double aRotation  = 0) 
        {
            X = 0;
            Y = 0;
            Value = 0;
            Row = 0;
            Col = 0;
            PartIndex = 0;
            Index = 0;
            Tag = aTag;
            Flag = aFlag;
            Suppressed = false;
            Radius = 0;
            Proximity = 0;
            Inset = 0;
            DownSet = 0;
            Elevation = 0;
            Mark = false;
            Virtual = false;

            if (!dxfPlane.IsNull(aPlane))
            {
                dxfVector v1 = new dxfVector(aPlane, aX, aY,0, aTag, aFlag, aRotation);
                Value = v1.Rotation;
                X = v1.X;
                Y = v1.Y;
            }
        }
      
        #endregion  Constructors

        #region Properties

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
        /// <summary>
        /// vectors sqaured
        /// </summary>
        /// <returns></returns>
        public double Sqrd => Math.Pow(X, 2) + Math.Pow(Y, 2);

        public double Z => Elevation ?? 0;

        #endregion Properties

        #region Methods
        object ICloneable.Clone() => (object)Clone(true);

        public UVECTOR Clone(bool bCloneIndex = true)
        {
            int idx = (!bCloneIndex) ? 0 : Index;


            return new UVECTOR(this) { Index = idx };
        }

        public override string ToString()
        {
            if (Elevation.HasValue)
                return Elevation.Value != 0 ? $"UVECTOR[{X:0.000###},{Y:0.000###},{Elevation.Value:0.000###}]" : $"UVECTOR[{X:0.000###},{Y:0.000###}]";
            else
                return $"UVECTOR[{X:0.000###},{Y:0.000###}]";



        }

        public UVECTOR DirectionTo(UVECTOR aVector) => DirectionTo(aVector, false, out bool _, out double _, false);


        public UVECTOR DirectionTo(UVECTOR aVector, bool bReturnInverse, out bool rDirectionIsNull, out double rDistance, bool bDontNormalize = false)
        {
            rDirectionIsNull = false;

            rDistance = 0;
            UVECTOR _rVal = aVector - this;
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

        public UVECTOR DirectionTo(ULINE aLine)
        {
            UVECTOR u1 = this.ProjectedTo(aLine, out UVECTOR orthodir, out double dst);
            return orthodir;
        }

        public bool IsEqual(UVECTOR aVector, int aPrecis = 6, bool bCompareElevation = true)
        {
            if (aPrecis < 0)
            {
                bool _rVal = X == aVector.X && Y == aVector.Y;

                if (bCompareElevation && _rVal)
                {
                    if (aVector.Elevation.HasValue && Elevation.HasValue)
                    {
                        _rVal = Elevation.Value == aVector.Elevation.Value;
                    }
                    else if (!aVector.Elevation.HasValue && Elevation.HasValue)
                    {
                        _rVal = Elevation.Value == 0;
                    }
                    else if (aVector.Elevation.HasValue && !Elevation.HasValue)
                    {
                        _rVal = aVector.Elevation.Value == 0;
                    }
                }

                return _rVal;
            }
            else
            {

                aPrecis = mzUtils.LimitedValue(aPrecis, 0, 15);
                bool _rVal = Math.Round(X, aPrecis) == Math.Round(aVector.X, aPrecis) && Math.Round(Y, aPrecis) == Math.Round(aVector.Y, aPrecis);

                if (bCompareElevation && _rVal)
                {
                    if (aVector.Elevation.HasValue && Elevation.HasValue)
                    {
                        _rVal = Math.Round(Elevation.Value, aPrecis) == Math.Round(aVector.Elevation.Value, aPrecis);
                    }
                    else if (!aVector.Elevation.HasValue && Elevation.HasValue)
                    {
                        _rVal = Math.Round(Elevation.Value, aPrecis) == 0;
                    }
                    else if (aVector.Elevation.HasValue && !Elevation.HasValue)
                    {
                        _rVal = Math.Round(aVector.Elevation.Value, aPrecis) == 0;
                    }
                }
                return _rVal;


            }
        }

        public void Translate(double DX, double DY) { X += DX; Y += DY; }
        /// <summary>
        /// vectors distance
        /// </summary>
        /// <param name="bVector"></param>
        /// <param name="aPrecis"></param>
        /// <returns></returns>
        public double DistanceTo(UVECTOR bVector, int aPrecis = 3)
        {
            double _rVal = Math.Sqrt(Math.Pow(X - bVector.X, 2) + Math.Pow(Y - bVector.Y, 2));
            if (aPrecis > 0)
            {
                aPrecis = mzUtils.LimitedValue(aPrecis, 0, 15);
                _rVal = Math.Round(_rVal, aPrecis);
            }
            return _rVal;
        }

        public double DistanceTo(uopVector bVector, int aPrecis = 3)
        {
            if (bVector == null) return 0;
            double _rVal = Math.Sqrt(Math.Pow(X - bVector.X, 2) + Math.Pow(Y - bVector.Y, 2));
            if (aPrecis > 0)
            {
                aPrecis = mzUtils.LimitedValue(aPrecis, 0, 15);
                _rVal = Math.Round(_rVal, aPrecis);
            }
            return _rVal;
        }

        /// <summary>
        /// returns the shortest distance to the passed line
        /// </summary>
        /// <param name="aLine" the line></param>
        /// <returns></returns>
        public double DistanceTo(ULINE aLine)
        {

            ProjectTo(aLine, out double _rVal);
            return _rVal;
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
            {
                X = aPrecis < 0 ? aNewX.Value : Math.Round(aNewX.Value, mzUtils.LimitedValue(aPrecis, 0, 15));
            }
            if (aNewY.HasValue)
            {
                Y = aPrecis < 0 ? aNewY.Value : Math.Round(aNewY.Value, mzUtils.LimitedValue(aPrecis, 0, 15));
            }

        }

        /// <summary>
        /// returns true if the passed vectors are equal within the passed precision
        /// </summary>
        /// <param name="aVector"></param>
        /// <param name="bVector"></param>
        /// <param name="aPrecis"></param>
        /// <returns></returns>
        public bool Compare(UVECTOR bVector, int aPrecis = 4)
        {
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
        public UVECTOR Polar(double aAngle, double aDistance)
        {
            UVECTOR _rVal = Clone();
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
                UVECTOR aDir = new UVECTOR(1, 0, 0);
                aDir.Rotate(aAngle, false);
                _rVal += aDir * aDistance;
            }

            return _rVal;
        }

        public void SetOrdinates(double? aX = null, double? aY = null) { if (aX.HasValue) X = aX.Value; if (aY.HasValue) Y = aY.Value; }

        /// <summary>
        ///#1the subject vector
        ///#2the structure of the plane
        ///^returns the coordinates of the passed vector with respect to the center and origin of the passed plane
        /// </summary>
        /// <param name="aPlane"></param>
        /// <param name="aScaler"></param>
        /// <returns></returns>
        public UVECTOR WithRespectToPlane(dxfPlane aPlane, double aScaler = 1)
        {
            UVECTOR _rVal = Clone(true);
            dxfVector v1 = ToDXFVector();


            if (aScaler != 1) { v1.Multiply(aScaler); }

            v1 = v1.WithRespectToPlane(aPlane);
            _rVal.Update(v1.X, v1.Y);

            return _rVal;
        }

        public bool LiesOn(UARC aArc, int aPrecis = 5, bool bArcIsInfinite = false) => aArc.ContainsVector(this, aPrecis, bArcIsInfinite);

        public bool LiesOn(ULINE aLine, int aPrecis = 5, bool bArcIsInfinite = false) => aLine.ContainsVector(this, aPrecis, bArcIsInfinite);


        public bool LiesOn(USEGMENT aSegment, int aPrecis = 5, bool bArcIsInfinite = false) => aSegment.ContainsVector(this, aPrecis, bArcIsInfinite);
        public double MultiSum(UVECTOR A)
        {
            //^returns the sum (A.X * X) + (A.Y * Y)
            return (A.X * X) + (A.Y * Y);

        }

        /// <summary>
        /// vectors rotate
        /// </summary>
        /// <param name="aAngle"></param>
        /// <param name="bInRadians"></param>
        public UVECTOR Rotated(double aAngle, bool bInRadians = false)
        {
            UVECTOR _rVal = (UVECTOR)Clone();
            _rVal.Rotate(new UVECTOR(0, 0), aAngle, bInRadians);
            return _rVal;
        }

        /// <summary>
        /// vectors rotate
        /// </summary>
        /// <param name="aAngle"></param>
        /// <param name="bInRadians"></param>
        public void Rotate(double aAngle, bool bInRadians = false)
        {
            Rotate(new UVECTOR(0, 0), aAngle, bInRadians);
        }

        /// <summary>
        /// vectors rotate
        /// </summary>
        /// <param name="aOrigin"></param>
        /// <param name="aAngle"></param>
        /// <param name="bInRadians"></param>
        public void Rotate(UVECTOR aOrigin, double aAngle, bool bInRadians = false)
        {

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

            double VX = X;
            double VY = Y;
            double a = aOrigin.X;
            double B = aOrigin.Y;
            double C = 0;
            double Z = 0;
            double u = 0;
            double V = 0;
            double W = 1;

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

        /// <summary>
        /// vectors dot product
        /// </summary>
        /// <param name="bVector"></param>
        /// <returns></returns>
        public double DotProduct(UVECTOR bVector) { return X * bVector.X + Y * bVector.Y; }


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
        public UVECTOR MidPt(UVECTOR bVector)
        {
            UVECTOR _rVal = Clone();
            _rVal.X += 0.5 * (bVector.X - X);
            _rVal.Y += 0.5 * (bVector.Y - Y);
            return _rVal;
        }


        /// <summary>
        /// vector normalize
        /// </summary>            
        /// <returns></returns>
        public void Normalize() => Normalize(out bool _, out double _);



        /// <summary>
        /// vector normalize
        /// </summary>
        /// <param name="rVectorIsNull"></param>
        /// <returns></returns>
        public void Normalize(out bool rVectorIsNull) => Normalize(out rVectorIsNull, out double _);

        /// <summary>
        /// vector normalize
        /// </summary>
        /// <param name="rVectorIsNull"></param>
        /// <param name="rLength"></param>
        /// <returns></returns>
        public void Normalize(out double rLength) => Normalize(out bool _, out rLength);


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
        public UVECTOR ComponentAlong(UVECTOR bVector)
        {
            UVECTOR _rVal = (UVECTOR)bVector.Clone(); ;
            double numer = X * bVector.X + Y * bVector.Y;
            double denom = bVector.Sqrd;

            if (denom != 0) { _rVal *= numer / denom; }

            return _rVal;
        }

        /// <summary>
        /// vector normalize
        /// </summary>
        /// <returns></returns>
        public UVECTOR Normalized() => Normalized(out bool _, out double _);

        /// <summary>
        /// vector normalize
        /// </summary>
        /// <param name="rVectorIsNull"></param>
        /// <returns></returns>
        public UVECTOR Normalized(out bool rVectorIsNull) => Normalized(out rVectorIsNull, out double _);

        public bool IsNull(int aPrecis = 8)
        {

            if (aPrecis <= 0) { return X == 0 && Y == 0; }
            aPrecis = mzUtils.LimitedValue(aPrecis, 0, 15);
            return Math.Round(X, aPrecis) == 0 && Math.Round(Y, aPrecis) == 0;
        }

        /// <summary>
        /// vector normalize
        /// </summary>            
        /// <param name="rVectorIsNull"></param>
        /// <param name="rLength"></param>
        /// <returns></returns>
        public UVECTOR Normalized(out double rLength) => Normalized(out bool _, out rLength);


        /// <summary>
        /// vector normalize
        /// </summary>
        /// <param name="rVectorIsNull"></param>
        /// <param name="rLength"></param>
        /// <returns></returns>
        public UVECTOR Normalized(out bool rVectorIsNull, out double rLength)
        {
            UVECTOR _rVal = Clone(true);
            rLength = Length(0);
            rVectorIsNull = rLength <= 0.000001;
            if (rVectorIsNull) { rLength = 0; return _rVal; }

            if (X == 0 && Y != 0)
            {
                _rVal.Y = (Y > 0) ? 1 : -1;
                return _rVal;
            }

            if (X != 0 && Y == 0)
            {
                _rVal.X = (X > 0) ? 1 : -1;
                return _rVal;
            }

            _rVal.X = X / rLength;

            _rVal.Y = (Math.Abs(_rVal.X) != 1) ? Y / rLength : _rVal.Y = 0;
            return _rVal;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(UVECTOR)) { return this == (UVECTOR)obj; } else { return false; };
        }

        public bool Equals(UVECTOR A, bool bCompareInverse, int aPrecis = 4)
        {
            aPrecis = mzUtils.LimitedValue(aPrecis, -1, 15);

            return (aPrecis >= 0) ? (Math.Round(A.X, aPrecis) == Math.Round(X, aPrecis)) && (Math.Round(A.Y, aPrecis) == Math.Round(Y, aPrecis)) : (A.X == X) && (A.Y == Y);
        }

        public bool Equals(UVECTOR A, bool bCompareInverse, int aPrecis, out bool rIsInverseEqual)
        {
            rIsInverseEqual = false;
            bool _rVal = Equals(A, aPrecis);
            if (bCompareInverse && !_rVal)
            {
                if (Equals(A * -1, aPrecis))
                {
                    _rVal = true;
                    rIsInverseEqual = true;
                }
            }
            return _rVal;
        }
        public dxfVector ToDXFVector(double aXChange, double aYChange, dynamic aRadius = null, dynamic aZ = null, string aTag = null, string aFlag = null, dxfPlane aPlane = null)
        {

            double elev = mzUtils.VarToDouble(Elevation);
            if (aZ != null) elev = mzUtils.VarToDouble(aZ, aDefault: elev);

            dxfVector _rVal = (aPlane == null) ? new dxfVector(X + aXChange, Y + aYChange, elev, aTag: Tag, aFlag: aFlag) : aPlane.Vector(X, Y, elev, aTag: Tag, aFlag: aFlag);

            _rVal.Row = Row;
            _rVal.Col = Col;
            _rVal.Suppressed = Suppressed;
            _rVal.Value = Value;
            if (aTag != null) _rVal.Tag = aTag;
            if (aFlag != null) _rVal.Flag = aFlag;

            _rVal.VertexRadius = Radius;

            _rVal.Suppressed = Suppressed;

            return _rVal;
        }

        public void Mirror(double? aX, double? aY)
        {
            if (!aX.HasValue && !aY.HasValue) return;
            double dx = aX.HasValue ? X - aX.Value : 0;
            double dy = aY.HasValue ? Y - aY.Value : 0;
            if (dx == 0 && dy == 0) return;

            X -= 2 * dx;
            Y -= 2 * dy;


        }

        public UVECTOR Mirrored(double? aX, double? aY)
        {
            UVECTOR _rVal = new UVECTOR(this);
            _rVal.Mirror(aX, aY);
            return _rVal;


        }


        public UVECTOR Moved(double aXAdder = 0, double aYAdder = 0)
        {
            UVECTOR _rVal = new UVECTOR(this);
            _rVal.Move(aXAdder, aYAdder);
            return _rVal;
        }

        public void Move(double aXAdder = 0, double aYAdder = 0)
        {
            if (aXAdder == 0 && aYAdder == 0) return;
            X += aXAdder;
            Y += aYAdder;

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
        public dxfVector ToDXFVector(bool bValueAsRotation = false, bool bValueAsVertexRadius = false, dynamic aZ = null, string aTag = null, string aFlag = null, dxfPlane aPlane = null)
        {

            double elev = Elevation.HasValue ? Elevation.Value : 0;
            if (aZ != null) elev = mzUtils.VarToDouble(aZ, aDefault: elev);

            dxfVector _rVal = aPlane == null ? new dxfVector(X, Y, elev, aTag: Tag, aFlag: aFlag) : aPlane.Vector(X, Y, elev, aTag: Tag, aFlag: aFlag);

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
        public void Project(UVECTOR aDirection, double aDistance, bool bSuppressNormalize = false, bool bInvertDirection = false)
        {
            if (!mzUtils.IsNumeric(aDistance)) { return; }
            if (aDistance == 0) { return; }

            UVECTOR aDir;
            bool aFlg = false;
            if (!bSuppressNormalize)
            { aDir = aDirection.Normalized(out aFlg); }
            if (aFlg) return;

            aDir = aDirection;


            if (aDistance <= 0 || bInvertDirection) { aDir *= -1; }

            UVECTOR vector = aDir * Math.Abs(aDistance);
            X += vector.X;
            Y += vector.Y;

        }

        /// <param name="aDirection"></param>
        /// <param name="aDistance"></param>
        /// <param name="bSuppressNormalize"></param>
        /// <param name="bInvertDirection"></param>
        public UVECTOR Projected(UVECTOR aDirection, double aDistance, bool bSuppressNormalize = false, bool bInvertDirection = false)
        {
            UVECTOR _rVal = new UVECTOR(this);

            if (aDistance == 0) return _rVal;

            UVECTOR aDir;
            bool aFlg = false;
            if (!bSuppressNormalize)
            { aDir = aDirection.Normalized(out aFlg); }
            if (aFlg) return _rVal;
            aDir = aDirection;


            if (aDistance <= 0 || bInvertDirection) { aDir *= -1; }

            _rVal += aDir * Math.Abs(aDistance);
            return _rVal;
        }

        /// <summary>
        ///projects the vector orthogonally to the passed line
        /// </summary>
        /// <param name="aLine" the subject line></param>
        public void ProjectTo(ULINE aLine) => ProjectTo(aLine, out UVECTOR rDir, out bool rOnSeg, out bool rDirPos, out double rDistance);


        /// <summary>
        ///projects the vector orthogonally to the passed line
        /// </summary>
        /// <param name="aLine" the subject line></param>
        /// <param name="rDistance"returns then orthogal distance to the segment from the vector></param>
        public void ProjectTo(ULINE aLine, out double rDistance) => ProjectTo(aLine, out UVECTOR rDir, out bool rOnSeg, out bool rDirPos, out rDistance);


        /// <summary>
        ///projects the vector orthogonally to the passed line
        /// </summary>
        /// <param name="aLine"></param>
        /// <param name="rOrthoDirection"></param>
        /// <param name="rDistance"></param>
        public void ProjectTo(ULINE aLine, out UVECTOR rOrthoDirection, out double rDistance) => ProjectTo(aLine, out rOrthoDirection, out bool _, out bool _, out rDistance);



        /// <summary>
        ///projects the vector orthogonally to the passed line
        /// </summary>
        /// <param name="aLine" the subject line></param>
        /// <param name="rDistance"returns then orthogal distance to the segment from the vector></param>
        public UVECTOR ProjectedTo(ULINE aLine, out double rDistance) => ProjectedTo(aLine, out UVECTOR rDir, out bool _, out bool _, out rDistance);

        /// <summary>
        ///projects the vector orthogonally to the passed line
        /// </summary>
        /// <param name="aLine"> the subject line</param>
        /// <param name="rOrthoDirection"> returns the orthogonal direction to the segment from the vector</param>
        /// <param name="rPointIsOnSegment"> returns true if the returned vector lies on the passed segment (between the end points)</param>
        /// <param name="rDirectionPositive" >returns true if the returned vector lies in the same direction from the start point as the end point (positive direction)</param>
        /// <param name="rDistance"> returns then orthogal distance to the segment from the vector</param>
        public void ProjectTo(ULINE aLine, out UVECTOR rOrthoDirection, out bool rPointIsOnSegment, out bool rDirectionPositive, out double rDistance)
        {
            rPointIsOnSegment = false;

            rDirectionPositive = true;
            UVECTOR org = Clone(true);


            rDistance = 0;


            //make sure the line has length
            UVECTOR lineDir = aLine.Direction(out bool aFlag, out double lineLen);
            if (aFlag)
            { Update(aLine.sp.X, aLine.sp.Y); }
            else
            {
                //see if the passed vector is the start pt
                if (aLine.sp.DistanceTo(this, 4) == 0)
                {
                    rPointIsOnSegment = true;
                    rDirectionPositive = false;
                    rOrthoDirection = new UVECTOR(1, 0, 0);
                    return;
                }

                //see if the passed vector is the end pt
                if (aLine.ep.DistanceTo(this, 4) == 0)
                {
                    rPointIsOnSegment = true;
                    rOrthoDirection = new UVECTOR(1, 0, 0);
                    return;
                }
            }


            if (!aFlag)
            {
                UVECTOR vComp = (this - aLine.sp).ComponentAlong(aLine.Direction());
                Update(aLine.sp.X + vComp.X, aLine.sp.Y + vComp.Y);
            }
            else
            {
                rPointIsOnSegment = true;
            }

            if (!aFlag)
            {
                UVECTOR aDir = aLine.sp.DirectionTo(this);

                rDirectionPositive = aDir.Compare(lineDir, 3);
                if (!rDirectionPositive)
                { rPointIsOnSegment = false; }
                else
                { rPointIsOnSegment = aLine.sp.DistanceTo(this, 8) < lineLen; }
            }
            rOrthoDirection = org.DirectionTo(this, false, out bool BNULL, out rDistance);
        }


        /// <summary>
        ///projects the vector orthogonally to the passed line
        /// </summary>
        /// <param name="aLine" the subject line></param>
        /// <param name="rOrthoDirection" returns the orthogonal direction to the segment from the vector></param>
        /// <param name="rDistance"returns then orthogal distance to the segment from the vector></param>
        public UVECTOR ProjectedTo(ULINE aLine, out UVECTOR rOrthoDirection, out double rDistance)
        {
            UVECTOR _rVal = new UVECTOR(this);
            _rVal.ProjectTo(aLine, out rOrthoDirection, out bool _, out bool _, out rDistance);
            return _rVal;
        }

        /// <summary>
        ///projects the vector orthogonally to the passed line
        /// </summary>
        /// <param name="aLine" the subject line></param>
        /// <param name="rOrthoDirection" returns the orthogonal direction to the segment from the vector></param>
        /// <param name="rPointIsOnSegment" returns true if the returned vector lies on the passed segment (between the end points)></param>
        /// <param name="rDirectionPositive" returns true if the returned vector lies in the same direction from the start point as the end point (positive direction)></param>
        /// <param name="rDistance"returns then orthogal distance to the segment from the vector></param>
        public UVECTOR ProjectedTo(ULINE aLine, out UVECTOR rOrthoDirection, out bool rPointIsOnSegment, out bool rDirectionPositive, out double rDistance)
        {
            UVECTOR _rVal = new UVECTOR(this);
            _rVal.ProjectTo(aLine, out rOrthoDirection, out rPointIsOnSegment, out rDirectionPositive, out rDistance);
            return _rVal;
        }

        /// <summary>
        ///  returns true if the lines are parallel and the orthogonal directions to the first line is the inverse of the orthogonal direction to the second line
        /// </summary>
        /// <param name="aLine"></param>
        /// <param name="bLine"></param>
        /// <param name="bOnIsIn" flag to consider the point between the lines if the vector is on one of the lines></param>
        /// <returns></returns>
        public bool LiesBetweenParallelLines(ULINE aLine, ULINE bLine, bool bOnIsIn = true)
        {
            if (aLine.Length == 0 || bLine.Length == 0) return false;
            UVECTOR dir1 = aLine.Direction();
            UVECTOR dir2 = bLine.Direction();
            if (!dir1.Equals(dir2, bCompareInverse: true)) return false;
            UVECTOR u1 = new UVECTOR(this);
            UVECTOR u2 = new UVECTOR(this);
            u1 = u1.ProjectedTo(aLine, out UVECTOR ortho1, out double d1);
            u2 = u2.ProjectedTo(bLine, out UVECTOR ortho2, out double d2);
            if ((d1 <= 0 || d2 <= 0) && bOnIsIn) return true;

            return !ortho1.Equals(ortho2, bCompareInverse: false);

        }

        /// <summary>
        ///  returns true if the lines are parallel and the orthogonal directions to the first line is the inverse of the orthogonal direction to the second line
        /// </summary>
        /// <param name="aLine"></param>
        /// <param name="bLine"></param>
        /// <param name="bOnIsIn" flag to consider the point between the lines if the vector is on one of the lines></param>
        /// <returns></returns>
        public bool LiesBetweenParallelLines(ULINE? aLine, ULINE? bLine, bool bOnIsIn = true)
        {
            return (!aLine.HasValue || !bLine.HasValue) ? false : LiesBetweenParallelLines(aLine.Value, bLine.Value, bOnIsIn);
        }

        #endregion Methods

        #region Shared Methods

        /// <summary>
        /// Vector planar
        /// </summary>
        /// <param name="aPlane"></param>
        /// <param name="aX"></param>
        /// <param name="aY"></param>
        /// <param name="aRadius"></param>
        /// <returns></returns>
        internal static UVECTOR Planar(dxfPlane aPlane, dynamic aX = null, dynamic aY = null, double aRadius = 0)
        {

            if (aPlane == null)
            {
                return new UVECTOR(mzUtils.VarToDouble(aX, aDefault: 0), mzUtils.VarToDouble(aY, aDefault: 0), 0, mzUtils.VarToDouble(aRadius, aDefault: 0));
            }
            else
            {

                UVECTOR _rVal = new UVECTOR(aPlane.X, aPlane.Y, 0, mzUtils.VarToDouble(aRadius, aDefault: 0));
                double d1 = mzUtils.VarToDouble(aX);
                if (d1 != 0) { _rVal.Project(new UVECTOR(aPlane.XDirection.X, aPlane.XDirection.Y), d1, true); }
                d1 = mzUtils.VarToDouble(aY);
                if (d1 != 0) { _rVal.Project(new UVECTOR(aPlane.YDirection.X, aPlane.YDirection.Y), d1, true); }
                return _rVal;
            }

        }

        /// <summary>
        /// vectors from vector
        /// </summary>
        /// <param name="aVector"></param>
        /// <returns></returns>
        internal static UVECTOR FromDXFVector(dxfVector aVector, bool bRotationsAsValues = false)
        {
            if (aVector == null) { return UVECTOR.Zero; }

            UVECTOR _rVal = new UVECTOR(aVector.X, aVector.Y, aVector.Radius)
            {
                Tag = aVector.Tag,
                Value = !bRotationsAsValues ? aVector.Value : aVector.Rotation,
                Radius = aVector.Radius,
                Row = aVector.Row,
                Col = aVector.Col,
                Suppressed = aVector.Suppressed,
                Elevation = aVector.Z

            };
            return _rVal;
        }
        /// <summary>
        /// Vectors by plane
        /// </summary>
        /// <param name="aPlane"></param>
        /// <param name="aVector"></param>
        /// <returns></returns>
        internal static UVECTOR ByPlane(dxfPlane aPlane, UVECTOR aVector)
        {
            if (aPlane != null) { return UVECTOR.FromDXFVector(aPlane.Vector(aVector.X, aVector.Y)); }
            return aVector;
        }

        /// <summary>
        /// Vector planar
        /// </summary>
        /// <param name="aPlane"></param>
        /// <param name="aX"></param>
        /// <param name="aY"></param>
        /// <param name="aRadius"></param>
        /// <returns></returns>
        internal static UVECTOR PlaneVector(dxfPlane aPlane, dynamic aX = null, dynamic aY = null, double aRadius = 0)
        {

            if (aPlane == null)
            {
                return new UVECTOR(mzUtils.VarToDouble(aX, aDefault: 0), mzUtils.VarToDouble(aY, aDefault: 0), 0, mzUtils.VarToDouble(aRadius, aDefault: 0));
            }
            else
            {


                UVECTOR _rVal = new UVECTOR(aPlane.X, aPlane.Y, 0, mzUtils.VarToDouble(aRadius, aDefault: 0));
                double d1 = mzUtils.VarToDouble(aX);
                if (d1 != 0)
                {
                    _rVal.Project(new UVECTOR(aPlane.XDirection.X, aPlane.XDirection.Y), d1, true);
                }
                d1 = mzUtils.VarToDouble(aY);
                if (d1 != 0)
                {
                    _rVal.Project(new UVECTOR(aPlane.YDirection.X, aPlane.YDirection.Y), d1, true);
                }
                return _rVal;
            }

        }

        public static UVECTOR Zero { get { return new UVECTOR(0, 0); } }

        public override int GetHashCode()
        { return base.GetHashCode(); }

        #endregion

        #region Operators

        public static UVECTOR operator +(UVECTOR A, UVECTOR B) { UVECTOR _rVal = A.Clone(true); _rVal.X += B.X; _rVal.Y += B.Y; return _rVal; }

        public static UVECTOR operator -(UVECTOR A, UVECTOR B) { UVECTOR _rVal = A.Clone(true); _rVal.X -= B.X; _rVal.Y -= B.Y; return _rVal; }
        public static UVECTOR operator *(UVECTOR A, double aScaler) { UVECTOR _rVal = A.Clone(true); _rVal.X *= aScaler; _rVal.Y *= aScaler; return _rVal; }
        public static bool operator ==(UVECTOR A, UVECTOR B) { return A.Compare(B, 4); }
        public static bool operator !=(UVECTOR A, UVECTOR B) => !(A == B);

        #endregion

    }



    internal struct UVECTORS : ICloneable
    {
        public bool Suppressed;
        public bool Invalid;
        private int _Count;
        private UVECTOR[] _Members;
        private bool _Init;
        internal URECTANGLE _Bounds;
        public string Name;

        #region Constructors
        public UVECTORS(bool bInvalid = false, string aName = "")
        {
            Suppressed = false;
            Invalid = bInvalid;
            _Count = 0;
            _Members = new UVECTOR[0];
            _Init = true;
            Name = aName;
            _Bounds = new URECTANGLE(0, 0, 0, 0);
            _DontUpdateBounds = false;
        }
        public UVECTORS(UVECTOR[] aMembers)
        {
            Suppressed = false;
            Invalid = false;
            _Count = 0;
            _Members = aMembers;
            _Init = aMembers != null;
            if (_Init) _Count = aMembers.Length;
            Name = string.Empty;
            _Bounds = new URECTANGLE(0, 0, 0, 0);
            _DontUpdateBounds = false;
            _Bounds = UVECTORS.ComputeBounds(this);
        }
        public UVECTORS(IEnumerable<iVector> aMembers)
        {
            Suppressed = false;
            Invalid = false;
            _Count = 0;
            _Members = new UVECTOR[0];
            _Init = true;
            Name = string.Empty;
            _Bounds = new URECTANGLE(0, 0, 0, 0);
            _DontUpdateBounds = false;
            if (aMembers == null) return;

            _DontUpdateBounds = true;
            foreach (iVector item in aMembers)
            {
                Add(new UVECTOR(item));
            }
            _DontUpdateBounds = false;
            _Bounds = UVECTORS.ComputeBounds(this);
        }
        public UVECTORS(UVECTORS aMembers, bool bClear = false,  iVector aDisplacement = null)
        {
            Suppressed = aMembers.Suppressed;
            Invalid = aMembers.Invalid;
            _Count = !bClear ? aMembers.Count : 0;
            _Init = true;
            _Members = new UVECTOR[0];
            _Bounds = new URECTANGLE(0, 0, 0, 0);
            Name = aMembers.Name;
            _DontUpdateBounds = false;
            if (_Count > 0)
            {
                if (aDisplacement != null)
                {
                    Array.Resize<UVECTOR>(ref _Members, _Count);

                    for (int i = 1; i <= _Count; i++)
                    {
                        UVECTOR v1 = new UVECTOR(aMembers.Item(i));
                        v1.Move(aDisplacement.X, aDisplacement.Y);
                        _Members[i - 1] = v1;
                        if (i == 1) { _Bounds = new URECTANGLE(v1.X, v1.Y, v1.X, v1.Y); } else { _Bounds.Update(v1); }
                    }
                }
                else
                {
                    _Members = Force.DeepCloner.DeepClonerExtensions.DeepClone<UVECTOR[]>(aMembers._Members);
                    UpdateBounds();
                }
                //UpdateBounds();
                //_DontUpdateBounds = false;
             
            }

        }


        public UVECTORS(UVECTOR aVector, iVector aDisplacement = null)
        {
            Suppressed = false;
            Invalid = false;
            _Count = 0;
            _Members = new UVECTOR[0];
            _Init = true;
            Name = string.Empty;
            _Bounds = new URECTANGLE(0, 0, 0, 0);
            _DontUpdateBounds = true;
            if (aDisplacement != null)
            {
                aVector = new UVECTOR(aVector);
                aVector.Move(aDisplacement.X, aDisplacement.Y);
            }
            Add(aVector);


            UpdateBounds();
            _DontUpdateBounds = false;
        }

        public UVECTORS(UVECTOR aVector, UVECTOR bVector)
        {
            Suppressed = false;
            Invalid = false;
            _Count = 0;
            _Members = new UVECTOR[0];
            _Init = true;
            _Bounds = new URECTANGLE(0, 0, 0, 0);
            Name = string.Empty;
            _DontUpdateBounds = true;
            Add(aVector);
            Add(bVector);
            UpdateBounds();
            _DontUpdateBounds = false;
        }


        public UVECTORS(UVECTOR aVector, UVECTOR bVector, UVECTOR cVector)
        {
            Suppressed = false;
            Invalid = false;
            _Count = 0;
            _Members = new UVECTOR[0];
            _Bounds = new URECTANGLE(0, 0, 0, 0);
            _Init = true;
            Name = string.Empty;
            _DontUpdateBounds = true;
            Add(aVector);
            Add(bVector);
            Add(cVector);
            UpdateBounds();
            _DontUpdateBounds = false;
            

        }


        public UVECTORS(UVECTOR aVector, UVECTOR bVector, UVECTOR cVector, UVECTOR dVector)
        {
            Suppressed = false;
            Invalid = false;
            _Count = 0;
            _Members = new UVECTOR[0];
            _Init = true;
            _Bounds = new URECTANGLE(0, 0, 0, 0);
            Name = string.Empty;
            _DontUpdateBounds = true;
            Add(aVector);
            Add(bVector);
            Add(cVector);
            Add(dVector);
            UpdateBounds();
            _DontUpdateBounds = false;

        
        }
        public UVECTORS(UVECTOR aVector, UVECTOR bVector, UVECTOR cVector, UVECTOR dVector, UVECTOR eVector)
        {
            Suppressed = false;
            Invalid = false;
            _Count = 0;
            _Members = new UVECTOR[0];
            _Init = true;
            _Bounds = new URECTANGLE(0, 0, 0, 0);
            Name = string.Empty;
            _DontUpdateBounds = true;
            Add(aVector);
            Add(bVector);
            Add(cVector);
            Add(dVector);
            Add(eVector);
            UpdateBounds();
            _DontUpdateBounds = false;
        }
        public UVECTORS(UVECTOR? aVector = null, UVECTOR? bVector = null, UVECTOR? cVector = null, UVECTOR? dVector = null, UVECTOR? eVector = null, UVECTOR? fVector = null)
        {
            Suppressed = false;
            Invalid = false;
            _Count = 0;
            _Members = new UVECTOR[0];
            _Init = true;
            _Bounds = new URECTANGLE(0, 0, 0, 0);
            Name = string.Empty;
            _DontUpdateBounds = true;
            if (aVector.HasValue) Add(aVector.Value);
            if (bVector.HasValue) Add(bVector.Value);
            if (cVector.HasValue) Add(cVector.Value);
            if (dVector.HasValue) Add(dVector.Value);
            if (eVector.HasValue) Add(eVector.Value);
            if (fVector.HasValue) Add(fVector.Value);
            UpdateBounds();
            _DontUpdateBounds = false;

        }

        #endregion

        private void Init()
        {
            Suppressed = false;
            Invalid = false;
            _Count = 0;
            _Members = new UVECTOR[0];
            _Bounds = new URECTANGLE(0, 0, 0, 0);
            _Init = true;
        }

        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return (_Count <= 6) ? $"UVECTORS[ {Coords()}]" : $"UVECTORS[{_Count}]";
            }
            else
            {
                return (_Count <= 6) ? $"UVECTORS '{Name}' [{Coords()}]" : $"UVECTORS ' {Name}' [{_Count}]";
            }

        }


        public int Count { get { if (!_Init) { Init(); } return _Count; } }

        public TVALUES Values
        {
            get
            {
                TVALUES _rVal = new TVALUES("Values");
                for (int i = 1; i <= Count; i++) { _rVal.Add(Item(i).Value); }
                return _rVal;
            }

            set
            {
                for (int i = 1; i <= value.Count; i++)
                {
                    if (i > Count) break;
                    UVECTOR u1 = Item(i);
                    u1.Value = mzUtils.VarToDouble(value.Item(i));
                    SetItem(i, u1);

                }
            }
        }


        public List<double> GetValues()
        {
            List<double> _rVal = new List<double>();
            for (int i = 1; i <= Count; i++) { _rVal.Add(Item(i).Value); }
            return _rVal;
           
        }
        public UVECTOR Item(int aIndex) { if (aIndex < 1 || aIndex > Count) { return UVECTOR.Zero; } _Members[aIndex - 1].Index = aIndex; return _Members[aIndex - 1]; }


        public void SetItem(int aIndex, UVECTOR aVector) { if (aIndex < 1 || aIndex > Count) { return; } if (_Members[aIndex - 1].X != aVector.X || _Members[aIndex - 1].Y != aVector.Y) _Bounds.Invalid = true; _Members[aIndex - 1] = aVector; _Members[aIndex - 1].Index = aIndex; }
        public void SetItem(int aIndex, UVECTOR? aVector) { if (aIndex < 1 || aIndex > Count || !aVector.HasValue) { return; } if (_Members[aIndex - 1].X != aVector.Value.X || _Members[aIndex - 1].Y != aVector.Value.Y) _Bounds.Invalid = true; _Members[aIndex - 1] = aVector.Value; _Members[aIndex - 1].Index = aIndex; }

        public void SetSuppressed(int aIndex, bool aSuppressionVal) { if (aIndex < 1 || aIndex > Count) return; _Members[aIndex - 1].Suppressed = aSuppressionVal; }

        public List<UVECTOR> ToList { get { if (!_Init) { Init(); } return new List<UVECTOR>(_Members); } }

        public bool SetValue(int aIndex, dynamic aValue)
        {
            if (aIndex <= 0 || aIndex > Count || aValue == null) { return false; }
            bool _rVal = _Members[aIndex - 1].Value != aValue;
            _Members[aIndex - 1].Value = aValue;
            return _rVal;
        }

        /// <summary>
        /// Vectors set values
        /// </summary>
        /// <param name="aVectors"></param>
        /// <param name="aValue"></param>
        /// <param name="aStartID"></param>
        /// <param name="aEndID"></param>
        /// <returns></returns>
        public bool SetValues(double aValue, dynamic aStartID = null, dynamic aEndID = null)
        {
            bool _rVal = false;
            mzUtils.LoopLimits(mzUtils.VarToInteger(aStartID), mzUtils.VarToInteger(aEndID), 1, Count, out int si, out int ei);
            UVECTOR u1;

            for (int i = si; i <= ei; i++)
            {
                u1 = Item(i);
                if (u1.Value != aValue) _rVal = true;
                u1.Value = aValue;
                SetItem(i, u1);
            }

            return _rVal;
        }

        /// <summary>
        /// vectors coord
        /// </summary>
        /// <param name="aPrecis"></param>
        /// <param name="aZValue"></param>
        /// <returns></returns>
        public string Coords(int aPrecis = 3, double? aZValue = null)
        {
            string _rVal = string.Empty;
            UVECTOR v1 = UVECTOR.Zero;

            aPrecis = mzUtils.LimitedValue(aPrecis, 0, 8);

            for (int i = 1; i <= Count; i++)
            {
                v1 = Item(i);
                if (_rVal !=  string.Empty) { _rVal += uopGlobals.Delim; }
                if (aZValue.HasValue)
                {
                    _rVal = $"{_rVal}({Math.Round(v1.X, aPrecis)},{Math.Round(v1.Y, aPrecis)},{Math.Round(aZValue.Value, aPrecis)})";
                }
                else
                {
                    _rVal = $"{_rVal}({Math.Round(v1.X, aPrecis)},{Math.Round(v1.Y, aPrecis)})";
                }

            }
            return _rVal;
        }

        /// <summary>
        /// vectors ordinates
        /// </summary>
        /// <param name="bGetY"></param>
        /// <param name="aPrecis"></param>
        /// <returns></returns>
        public List<double> Ordinates(bool bGetY, int aPrecis = 6)
        {
            List<double> _rVal = new List<double>();
            double aVal = 0;
            string aLst = string.Empty;

            for (int i = 1; i <= Count; i++)
            {
                if (bGetY)
                { aVal = Item(i).Y; }
                else
                { aVal = Item(i).X; }
                if (aPrecis >= 0)
                {
                    aPrecis = mzUtils.LimitedValue(aPrecis, 0, 15);
                    aVal = Math.Round(aVal, aPrecis);
                }
                if (mzUtils.ListAdd(ref aLst, aVal)) _rVal.Add(aVal); // only add the unique values
            }
            return _rVal;
        }

        public int SuppressedCount(bool aSuppresedVal = false)
        {
            int _rVal = 0;

            UVECTOR u1;
            for (int i = 1; i <= Count; i++)
            {
                u1 = Item(i);
                if (u1.Suppressed == aSuppresedVal) { _rVal++; }
            }
            return _rVal;
        }

        /// <summary>
        /// vectors by tag
        /// </summary>
        /// <param name="aTag"></param>
        /// <returns></returns>
        public UVECTORS GetByTag(string aTag, bool? bSuppressed = null)
        {
            UVECTORS _rVal = new UVECTORS(this, true);
            UVECTOR u1;
            for (int i = 1; i <= Count; i++)
            {
                u1 = Item(i);
                if (string.Compare(u1.Tag, aTag) == 0)
                {
                    if (!bSuppressed.HasValue)
                    {
                        _rVal.Add(u1);
                    }
                    else
                    {
                        if (bSuppressed.Value == u1.Suppressed) _rVal.Add(u1);
                    }

                }
            }
            return _rVal;
        }

        public UVECTORS GetBySuppressed(bool aSuppresedVal = false, bool bReturnClones = false)
        {
            UVECTORS _rVal = new UVECTORS(this, true);
            UVECTOR u1;
            for (int i = 1; i <= Count; i++)
            {
                u1 = Item(i);
                if (u1.Suppressed == aSuppresedVal)
                {
                    if (bReturnClones) u1 = u1.Clone();
                    _rVal.Add(u1);

                }
            }
            return _rVal;
        }

        /// <summary>
        /// vectors by tag
        /// </summary>
        /// <param name="aValue"></param>
        /// <returns></returns>
        public UVECTORS GetByValue(double aValue, int aPrecis = 4)
        {
            aPrecis = mzUtils.LimitedValue(aPrecis, 0, 15);
            UVECTOR u1;
            UVECTORS _rVal = new UVECTORS(this, true);
            double aVal = Math.Round(aValue, aPrecis);
            for (int i = 1; i <= Count; i++)
            {
                u1 = Item(i);

                if (Math.Round(u1.Value, aPrecis) == aVal)
                { _rVal.Add(u1); }
            }
            return _rVal;
        }

        /// <summary>
        /// vectors get by ord
        /// </summary>
        /// <param name="aVectors"></param>
        /// <param name="aOrd"></param>
        /// <param name="bDoX"></param>
        /// <param name="aPrecis"></param>
        /// <returns></returns>
        public UVECTORS getByOrd(dynamic aOrd, bool bDoX = false, int aPrecis = 4)
        {
            UVECTORS _rVal = new UVECTORS(this, true);
            double ord1 = 0;
            UVECTOR u1;
            aPrecis = mzUtils.LimitedValue(aPrecis, 0, 15);
            aOrd = mzUtils.VarToDouble(aOrd, aPrecis: aPrecis);
            for (int i = 1; i <= Count; i++)
            {
                u1 = Item(i);
                ord1 = bDoX ? Math.Round(u1.X, aPrecis) : Math.Round(u1.Y, aPrecis);

                if (ord1 == aOrd)
                {
                    _rVal.Add(u1);
                }
            }

            return _rVal;
        }
        /// <summary>
        /// vectors by row
        /// </summary>
        /// <param name="aRadius"></param>
        /// <returns></returns>
        public UVECTORS GetByRadius(double aRadius, int aPrecis = 6, bool bJustOne = false)
        {
            UVECTORS _rVal = new UVECTORS(this, true);
            UVECTOR u1;
            aPrecis = mzUtils.LimitedValue(aPrecis,0,15);
            double rad = Math.Round(aRadius, aPrecis);
            for (int i = 1; i <= Count; i++)
            {
                u1 = Item(i);

                if (Math.Round( u1.Radius,aPrecis) == rad)
                {
                     _rVal.Add(u1);
                    if (bJustOne) break;
                }
            }
            return _rVal;
        }
        /// <summary>
        /// vectors by row
        /// </summary>
        /// <param name="aRow"></param>
        /// <param name="aCol"></param>
        /// <returns></returns>
        public UVECTORS GetByRow(int aRow, int? aCol = null)
        {
            UVECTORS _rVal = new UVECTORS(this, true);
            UVECTOR u1;
            bool bTestCol = aCol.HasValue;
            int col = 0;
            if (bTestCol) { col = aCol.Value; }
            for (int i = 1; i <= Count; i++)
            {
                u1 = Item(i);

                if (u1.Row == aRow)
                {
                    if (!bTestCol || (bTestCol && u1.Col == col))
                    { _rVal.Add(u1); }
                }
            }
            return _rVal;
        }
        /// <summary>
        /// vectors by part index
        /// </summary>
        /// <param name="aRow"></param>
        /// <param name="aCol"></param>
        /// <returns></returns>
        public UVECTORS GetByPartIndex(int aPartIndex)
        {
            UVECTORS _rVal = new UVECTORS(this, true);
            UVECTOR u1;

            for (int i = 1; i <= Count; i++)
            {
                u1 = Item(i);

                if (u1.PartIndex == aPartIndex) _rVal.Add(u1);

            }
            return _rVal;
        }
        /// <summary>
        /// vectors by row
        /// </summary>
        /// <param name="aCol"></param>
        /// <param name="aRow"></param>
        /// <returns></returns>
        public UVECTORS GetByCol(int aCol, dynamic aRow = null)
        {
            UVECTORS _rVal = new UVECTORS(this, true);
            UVECTOR u1;
            bool bTestRow = aRow != null;
            int row = 0;
            if (bTestRow) { row = mzUtils.VarToInteger(aRow); }
            for (int i = 1; i <= Count; i++)
            {
                u1 = Item(i);

                if (u1.Col == aCol)
                {
                    if (!bTestRow || (bTestRow && u1.Row == row))
                    { _rVal.Add(u1); }
                }
            }
            return _rVal;
        }

        /// <summary>
        /// vectors nearest value
        /// </summary>
        /// <param name="aVector"></param>
        /// <param name="aDir">if passed, only vectors whose direction from the passed vector is equal to the passed direction </param>
        /// <returns></returns>
        public UVECTOR Nearest(UVECTOR aVector, UVECTOR? aDir = null) => Nearest(aVector, out int rIndex, aDir);

        /// <summary>
        /// vectors nearest value to line
        /// </summary>
        /// <param name="aLine"></param>
        /// <param name="rDistance"></param>
        /// <returns></returns>
        public UVECTOR NearestToLine(uopLine aLine, out double rDistance) => NearestToLine(new ULINE(aLine), out int rIndex, out rDistance);


        /// <summary>
        /// vectors nearest value to line
        /// </summary>
        /// <param name="aLine"></param>
        /// <param name="rDistance"></param>
        /// <returns></returns>
        public UVECTOR NearestToLine(ULINE aLine, out double rDistance) => NearestToLine(aLine, out int rIndex, out rDistance);

        /// <summary>
        /// vectors nearest value to line
        /// </summary>
        /// <param name="aLine"></param>
        /// <param name="rIndex"></param>
        /// <param name="rDistance"></param>
        /// <returns></returns>
        public UVECTOR NearestToLine(ULINE aLine, out int rIndex, out double rDistance)
        {
            UVECTOR _rVal = UVECTOR.Zero;
            rIndex = -1;
            rDistance = 0;
            UVECTOR u1 = UVECTOR.Zero;
            double d1 = 0;
            double d2 = 0;
            d1 = System.Double.MaxValue;
            for (int i = 1; i <= Count; i++)
            {
                u1 = Item(i);
                d2 = u1.DistanceTo(aLine);
                if (d2 < d1)
                {
                    rIndex = i;
                    d1 = d2;
                }
            }
            rDistance = d1;
            if (rIndex >= 0) { _rVal = Item(rIndex); }
            return _rVal;
        }

        public UVECTOR First { get => Item(1); set => SetItem(1, value); }

        public UVECTOR Last { get => Item(Count); set => SetItem(Count, value); }

        /// <summary>
        /// vectors nearest value
        /// </summary>
        /// <param name="aVector"></param>
        /// <param name="rIndex"></param>
        /// <param name="aDir">if passed, only vectors whose direction from the passed vector is equal to the passed direction </param>
        /// <returns></returns>
        public UVECTOR Nearest(UVECTOR aVector, out int rIndex, UVECTOR? aDir = null)
        {
            rIndex = -1;
            double mx = 0;
            double d1 = 0;
            mx = System.Double.MaxValue;

            for (int i = 1; i <= Count; i++)
            {
                UVECTOR mem = Item(i);
                if (aDir.HasValue)
                {
                    UVECTOR dir = aVector.DirectionTo(mem);
                    if (!aDir.Value.IsEqual(dir, 4))
                        continue;
                }
                d1 = mem.DistanceTo(aVector, 4);
                if (d1 < mx)
                {
                    rIndex = i;
                    mx = d1;
                }
            }
            return rIndex >= 0 ? Item(rIndex) : UVECTOR.Zero;

        }

        /// <summary>
        /// vectors farthest value
        /// </summary>
        /// <param name="aVector"></param>
        /// <returns></returns>
        public UVECTOR Farthest(UVECTOR aVector) => Farthest(aVector, out int rIndex);

        /// <summary>
        /// vectors farthest value
        /// </summary>
        /// <param name="aVector"></param>
        /// <param name="rIndex"></param>
        /// <returns></returns>
        public UVECTOR Farthest(UVECTOR aVector, out int rIndex)
        {
            UVECTOR _rVal = UVECTOR.Zero;
            rIndex = 0;
            double mx = 0;
            double d1 = 0;
            mx = System.Double.MinValue;

            for (int i = 1; i <= Count; i++)
            {
                d1 = Item(i).DistanceTo(aVector, 4);
                if (d1 > mx)
                {
                    rIndex = i;
                    mx = d1;
                }
            }
            if (rIndex >= 0) { _rVal = Item(rIndex); }

            return _rVal;
        }

        ///returns the 2D area summation of all the vectors in the collection
        /// </summary>
        /// <param name="rLimits"></param>
        /// <returns></returns>
        public double Area()
        {
            return Area(out URECTANGLE _);
        }

        ///returns the 2D area summation of all the vectors in the collection
        /// </summary>
        /// <param name="rLimits"></param>
        /// <returns></returns>
        public double Area(out URECTANGLE rLimits)
        {

            rLimits = Bounds;
            //return rLimits.Area;

            if (Count <= 1) return 0;


            if (rLimits.Width <= 0 || rLimits.Height <= 0) { return 0; }

            UVECTOR org = rLimits.Center;
            UVECTOR u1;
            UVECTOR u2;
            double sumation = 0;
            for (int i = 1; i <= Count; i++)
            {
                u1 = Item(i);

                u2 = (i < Count) ? Item(i + 1) : Item(1);
                u1 -= org;
                u2 -= org;
                sumation += Math.Abs(u1.X * u2.Y - u2.X * u1.Y);
            }

            return 0.5 * sumation;
        }

        /// <summary>
        ///returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>
        /// 

        object ICloneable.Clone() => (object)Clone(false);

        public UVECTORS Clone(bool bReturnEmpty = false) => new UVECTORS(this, bReturnEmpty);

        /// <summary>
        /// Remove vectors
        /// </summary>
        /// <param name="aVectors"></param>
        /// <param name="aIndex"></param>
        /// <returns></returns>
        public UVECTOR Remove(int aIndex)
        {

            if (aIndex < 1 || aIndex > Count) { return UVECTOR.Zero; }

            UVECTOR _rVal = UVECTOR.Zero;
            if (Count == 1) { _rVal = Item(1); Clear(); return _rVal; }
            _Bounds.Invalid = true;
            if (aIndex == Count)
            {
                _rVal = Item(Count);
                _Count -= 1;
                Array.Resize<UVECTOR>(ref _Members, _Count);
                return _rVal;
            }


            UVECTOR[] newMems = new UVECTOR[_Count - 1];
            int j = 0;


            for (int i = 1; i <= Count; i++)
            {
                if (i != aIndex)
                { j += 1; newMems[j - 1] = Item(i); newMems[j - 1].Index = j; }
                else
                { _rVal = Item(i); }

            }

            _Members = newMems;
            _Count -= 1;
            return _rVal;
        }

        /// <summary>
        /// add vectors X,Y
        /// </summary>
        /// <param name="aX"></param>
        /// <param name="aY"></param>
        /// <param name="aRow"></param>
        /// <param name="aCol"></param>
        /// <param name="aValue"></param>
        /// <param name="aTag"></param>
        /// <param name="bSuppressed"></param>
        /// <param name="aRadius"></param>
        /// <param name="aElevation"></param>
        /// <returns></returns>
        public UVECTOR Add(double aX, double aY, int aRow = 0, int aCol = 0, double aValue = 0, string aTag = "", bool bSuppressed = false, double aRadius = 0, double? aElevation = null)
        {
            UVECTOR _rVal = new UVECTOR(aX, aY)
            {
                Row = aRow,
                Col = aCol,
                Value = aValue,
                Tag = aTag,
                Suppressed = bSuppressed,
                Radius = aRadius,

            };

            if (aElevation.HasValue) _rVal.Elevation = aElevation.Value;
            return Add(_rVal);
        }

        /// <summary>
        /// add vectors X,Y
        /// </summary>
        /// <param name="aX"></param>
        /// <param name="aY"></param>
        /// <param name="aRow"></param>
        /// <param name="aCol"></param>
        /// <param name="aValue"></param>
        /// <param name="aTag"></param>
        /// <param name="bSuppressed"></param>
        /// <param name="aRadius"></param>
        /// <param name="aElevation"></param>
        /// <returns></returns>
        public UVECTOR AddXY(double aX, double aY, int aRow = 0, int aCol = 0, double aValue = 0, string aTag = "", bool bSuppressed = false, double aRadius = 0, double? aElevation = null)
        {
            UVECTOR _rVal = new UVECTOR(aX, aY)
            {
                Row = aRow,
                Col = aCol,
                Value = aValue,
                Tag = aTag,
                Suppressed = bSuppressed,
                Radius = aRadius,
            };

            if (aElevation.HasValue) _rVal.Elevation = aElevation.Value;

            return Add(_rVal);
        }

        public void Mirror(double? aX, double? aY)
        {
            if (Count <= 0 || (!aX.HasValue && !aY.HasValue)) return;
            for (int i = 1; i <= Count; i++)
            {
                UVECTOR mem = Item(i);
                if (aX.HasValue)
                {
                    double dx = mem.X - aX.Value;
                    mem.X -= 2 * dx;
                }
                if (aY.HasValue)
                {
                    double dy = mem.Y - aY.Value;
                    mem.Y -= 2 * dy;
                }

                SetItem(i, mem);
            }

        }

        public UVECTORS Mirrored(double? aX, double? aY)
        {
            UVECTORS _rVal = UVECTORS.Zero;
            if (Count <= 0 || (!aX.HasValue && !aY.HasValue)) return _rVal;
            for (int i = 1; i <= Count; i++)
            {
                UVECTOR mem = Item(i).Clone();
                if (aX.HasValue)
                {
                    double dx = mem.X - aX.Value;
                    mem.X -= 2 * dx;
                }
                if (aY.HasValue)
                {
                    double dy = mem.Y - aY.Value;
                    mem.Y -= 2 * dy;
                }

                _rVal.Add(mem);
            }
            return _rVal;
        }

        private bool _DontUpdateBounds;

        public void Append(IEnumerable<iVector> bVectors, string aTag = null,double? aValue = null, string aFlag = null, double? aRadius = null) { if (bVectors == null || bVectors.Count() ==0) return; foreach (var item in bVectors) { Add(item,aTag: aTag, aValue: aValue, aFlag: aFlag, aRadius: aRadius); } }

        /// <summary>
        /// Append vectors
        /// </summary>
        /// <param name="aVectors"></param>
        /// <param name="aValue"></param>
        public void Append(UVECTORS aVectors, double? aValue = null) 
        { 
            for (int i = 1; i <= aVectors.Count; i++) 
            { Add(aVectors.Item(i), aValue: aValue); } 
        
        }

        public void AppendMirrors(double? aX, double? aY, double? aValue = null)
        {
            if (Count <= 0 || (!aX.HasValue && !aY.HasValue)) return;
            Append(Mirrored(aX, aY), aValue);
        }

        public void Clear() { _Count = 0; _Members = new UVECTOR[0]; _Bounds = new URECTANGLE(0, 0, 0, 0); }

        /// <summary>
        /// add vectors
        /// </summary>
        /// <param name="aVectors"></param>

        /// <summary>
        /// Add a vector to the array
        /// </summary>
        /// <param name="aVector"></param>
        /// <param name="aTag"></param>
        /// <param name="aCollector"></param>
        /// <param name="aValue"></param>
        /// <param name="aFlag"></param>
        /// <param name="aRadius"></param>
        public UVECTOR Add(iVector aVector, string aTag = null, colDXFVectors aCollector = null, double? aValue = null, string aFlag = null, double? aRadius = null)
        {
            if (aVector == null) return UVECTOR.Zero;
             UVECTOR newv = new UVECTOR(aVector);
            if (aTag != null) newv.Tag = aTag;
            if (aFlag != null) newv.Flag = aTag;

            if (aValue.HasValue) newv.Value = aValue.Value;
            if (aRadius.HasValue) newv.Radius = aRadius.Value;
            if (aCollector != null) aCollector.Add(newv.ToDXFVector());
            return Add(newv);
        }


        /// <summary>
        /// Add a vector to the array
        /// </summary>
        /// <param name="aVector"></param>
        /// <param name="aTag"></param>
        /// <param name="aCollector"></param>
        /// <param name="aValue"></param>
        public UVECTOR Add(UVECTOR aVector, string aTag = null, colDXFVectors aCollector = null, double? aValue = null, double? aRadius = null, string aFlag = null)
        {
            if (Count + 1 > Int32.MaxValue) { return aVector; }

            _Count += 1;
            Array.Resize<UVECTOR>(ref _Members, _Count);
            _Members[_Count - 1] = new UVECTOR(aVector);
            if (aTag != null) _Members[_Count - 1].Tag = aTag;
            if (aFlag != null) _Members[_Count - 1].Flag = aFlag;
            if (aValue.HasValue) _Members[_Count - 1].Value = aValue.Value;
            if (aRadius.HasValue) _Members[_Count - 1].Radius = aRadius.Value;
            _Members[_Count - 1].Index = _Count;
            if (_Count == 1) { _Bounds = new URECTANGLE(aVector.X, aVector.Y, aVector.X, aVector.Y); } else { _Bounds.Update(aVector); }
            if (aCollector != null) aCollector.Add(aVector.ToDXFVector());


            return Item(Count);

        }

        public UVECTOR Add(UVECTOR? aVector, string aTag = null, colDXFVectors aCollector = null, double? aValue = null, double? aRadius = null)
        {
            if (Count + 1 > Int32.MaxValue || !aVector.HasValue) { return UVECTOR.Zero; }

            _Count += 1;
            Array.Resize<UVECTOR>(ref _Members, _Count);
            _Members[_Count - 1] = new UVECTOR(aVector.Value);
            if (!string.IsNullOrEmpty(aTag))
            {
                _Members[_Count - 1].Tag = aTag;

            }
            if (aValue.HasValue) _Members[_Count - 1].Value = aValue.Value;
            if (aRadius.HasValue) _Members[_Count - 1].Radius = aRadius.Value;
            _Members[_Count - 1].Index = _Count;
            if (_Count == 1) { _Bounds = new URECTANGLE(aVector.Value.X, aVector.Value.Y, aVector.Value.X, aVector.Value.Y); } else { _Bounds.Update(aVector.Value); }
            aCollector?.Add(aVector.Value.ToDXFVector());


            return Item(Count);

        }

        /// <summary>
        /// vetors bounds
        /// </summary>
        /// <param name="aVectors"></param>
        /// <param name="rWidth"></param>
        /// <param name="rHeight"></param>
        /// <returns></returns>
        public URECTANGLE Bounds
        {

            get
            {
                if (_Bounds.Invalid) UpdateBounds();
                return new URECTANGLE(_Bounds);
            }

        }

        public void UpdateBounds()
        {
            _Bounds = UVECTORS.ComputeBounds(this);
            _Bounds.Invalid = false;
        }

        /// <summary>
        /// vectors to segments
        /// </summary>
        /// <param name="aVectors"></param>
        /// <param name="bOpen"></param>
        /// <returns></returns>
        public USEGMENTS ToSegments(bool bOpen = false)
        {
            USEGMENTS _rVal = new USEGMENTS();
            UVECTOR u1;
            UVECTOR u2;
            USEGMENT aSeg;


            for (int i = 1; i <= Count; i++)
            {
                aSeg = new USEGMENT();
                u1 = Item(i);
                if (i <= Count - 1)
                { u2 = Item(i + 1); }
                else
                {
                    if (bOpen) { break; }
                    u2 = Item(1);
                }
                if (u1.IsEqual(u2)) continue;
                aSeg.Tag = u1.Tag;
                aSeg.Value = u1.Value;

                if (u1.Radius == 0)
                {
                    aSeg.IsArc = false;
                    aSeg.LineSeg.sp = new UVECTOR(u1);
                    aSeg.LineSeg.ep = new UVECTOR(u2);
                }
                else
                {
                    dxeArc aArc = dxfUtils.ArcBetweenPoints(Math.Abs(u1.Radius), u1.ToDXFVector(), u2.ToDXFVector(),  bSuppressErrors:true);
                    if (aArc == null)
                    {
                       
                        aSeg.IsArc = false;
                        aSeg.LineSeg.sp = new UVECTOR(u1);
                        aSeg.LineSeg.ep = new UVECTOR(u2);
                    }
                    else
                    {
                        aSeg = USEGMENT.FromArc(aArc);
                    }
                }
                _rVal.Add(aSeg);
            }
            return _rVal;
        }


        /// <summary>
        /// vectors line segments
        /// </summary>
        /// <param name="aVectors"></param>
        /// <param name="bClosed"></param>
        /// <param name="bSuppressNulls"></param>
        /// <param name="aNullLength"></param>
        /// <returns></returns>
        public ULINES LineSegments(bool bClosed = false, bool bSuppressNulls = false, double aNullLength = 0.001)
        {
            ULINES _rVal = new ULINES();
            if (Count <= 1) return _rVal;
            aNullLength = Math.Abs(aNullLength);

            UVECTOR u1 = UVECTOR.Zero;
            UVECTOR u2 = UVECTOR.Zero;

            for (int i = 1; i <= Count; i++)
            {
                u1 = Item(i);
                if (i < Count)
                { u2 = Item(i + 1); }
                else
                {
                    if (!bClosed)
                    {
                        break;
                    }
                    u2 = Item(1);
                }
                if (!bSuppressNulls)
                {
                    _rVal.Add(u1, u2);
                }
                else
                {
                    if (u1.DistanceTo(u2, 6) <= aNullLength)
                    {
                        _rVal.Add(u1, u2);
                    }
                }
            }
            return _rVal;
        }


        /// <summary>
        /// vectors to vector
        /// </summary>
        /// <param name="aVectors"></param>
        /// <param name="bValueAsRotation"></param>
        /// <param name="bValueAsVertexRadius"></param>
        /// <param name="aMinX"></param>
        /// <param name="aMinY"></param>
        /// <param name="aCollector"></param>
        /// <param name="aTag"></param>
        /// <param name="aFlag"></param>
        /// <param name="aPlane"></param>
        /// <param name="bUnsuppressOnly"></param>
        /// <param name="rSuppressed"></param>
        /// <param name="aZ"></param>
        /// <returns></returns>
        public colDXFVectors ToDXFVectors(bool bValueAsRotation = false, bool bValueAsVertexRadius = false, dynamic aMinX = null, dynamic aMinY = null, colDXFVectors aCollector = null, string aTag = null, string aFlag = null, dxfPlane aPlane = null, bool bUnsuppressOnly = false, colDXFVectors rSuppressed = null, double aZ = 0)
        {
            colDXFVectors _rVal = aCollector ?? new colDXFVectors();
            UVECTOR u1 = UVECTOR.Zero;
            bool bTestX = mzUtils.IsNumeric(aMinX);
            double aX = bTestX ? mzUtils.VarToDouble(aMinX) : 0;
            bool bTestY = mzUtils.IsNumeric(aMinY);
            double aY = bTestY ? mzUtils.VarToDouble(aMinY) : 0;

            if (bTestX) { aX = (double)aMinX; }
            if (bTestY) { aY = (double)aMinY; }

            for (int i = 1; i <= Count; i++)
            {
                u1 = Item(i);
                if (!bUnsuppressOnly || (bUnsuppressOnly && !u1.Suppressed))
                {
                    if ((!bTestX || (bTestX && u1.X >= aX)) && (!bTestY || (bTestY && u1.Y >= aY)))
                    {
                        _rVal.Add(u1.ToDXFVector(bValueAsRotation, bValueAsVertexRadius, aZ, aTag, aFlag, aPlane));
                    }
                }
                if (u1.Suppressed)
                {
                    if (rSuppressed != null)
                    {
                        rSuppressed.Add(u1.ToDXFVector(bValueAsRotation, bValueAsVertexRadius, aZ, aTag, aFlag, aPlane));
                    }
                }
            }
            return _rVal;
        }

        /// <summary>
        /// Vectors to polyline
        /// </summary>
        /// <param name="aVectors"></param>
        /// <param name="bOpen"></param>
        /// <param name="aTag"></param>
        /// <param name="aFlag"></param>
        /// <param name="aValue"></param>
        /// <param name="aBase"></param>
        /// <returns></returns>
        public dxePolyline ToDXFPolyline(bool bOpen = false, dynamic aTag = null, dynamic aFlag = null, double? aValue = null, dxePolyline aBase = null)
        {
            dxePolyline _rVal;
            if (aBase == null)
            { _rVal = new dxePolyline(); }
            else
            {
                _rVal = aBase;
                _rVal.Vertices.Clear();
            }



            _rVal.Closed = !bOpen;
            for (int i = 1; i <= Count; i++)
            {
                UVECTOR v1 = Item(i);

                _rVal.Vertices.Add(v1.ToDXFVector(false, false), aTag: v1.Tag);
            }
            if (aValue.HasValue) { _rVal.Value = aValue.Value; }
            if (!String.IsNullOrEmpty(aTag))
            {
                _rVal.Tag = Convert.ToString(aTag);
            }
            if (!String.IsNullOrEmpty(aFlag))
            {
                _rVal.Flag = Convert.ToString(aFlag);
            }
            return _rVal;
        }

        /// <summary>
        /// Vectors Get Extreme Ord
        /// </summary>
        /// <param name="bMin"></param>
        /// <param name="bGetY"></param>
        /// <param name="aPrecis"></param>
        /// <returns></returns>
        public double GetExtremeOrd(bool bMin = false, bool bGetY = false, int aPrecis = 4) => GetExtremeOrd(out int _, bMin, bGetY, aPrecis);

        /// <summary>
        /// Vectors Get Extreme Ord
        /// </summary>
        /// <param name="aVectors"></param>
        /// <param name="bMin"></param>
        /// <param name="bGetY"></param>
        /// <param name="aPrecis"></param>
        /// <param name="rIndex"></param>
        /// <returns></returns>
        public double GetExtremeOrd(out int rIndex, bool bMin = false, bool bGetY = false, int aPrecis = 4)
        {
            rIndex = 0;
            if (Count <= 0) { return 0; }
            UVECTOR u1 = Item(1);
            rIndex = 1;
            if (Count == 1)
            {
                if (bGetY)
                { return u1.Y; }
                else
                { return u1.X; }
            }

            aPrecis = mzUtils.LimitedValue(aPrecis, 1, 15);
            int i = 0;
            UVECTOR u2;
            double aOrd = 0;
            double bOrd = 0;

            if (bGetY)
            {
                aOrd = Math.Round(u1.Y, aPrecis);
            }
            else
            {
                aOrd = Math.Round(u1.X, aPrecis);
            }
            for (i = 1; i <= Count; i++)
            {
                u2 = (UVECTOR)Item(i).Clone();
                if (bGetY)
                { bOrd = Math.Round(u2.Y, aPrecis); }
                else
                { bOrd = Math.Round(u2.X, aPrecis); }
                if (bMin)
                {
                    if (bOrd < aOrd)
                    {
                        aOrd = bOrd;
                        rIndex = i;
                    }
                }
                else
                {
                    if (bOrd > aOrd)
                    {
                        aOrd = bOrd;
                        rIndex = i;
                    }
                }
            }

            u1 = Item(rIndex);
            if (bGetY) { return u1.Y; } else { return u1.X; }

        }

        /// <summary>
        /// Vectors rotate
        /// </summary>
        /// <param name="aOrigin"></param>
        /// <param name="aAngle"></param>
        /// <param name="bInRadians"></param>
        public void Rotate(UVECTOR aOrigin, double aAngle, bool bInRadians = false)
        {
            if (Math.Abs(aAngle) <= 0.00000001) { return; }
            if (!bInRadians) { aAngle *= Math.PI / 180; }

            UVECTOR u1;
            for (int i = 1; i <= Count; i++)
            {
                u1 = Item(i);
                u1.Rotate(aOrigin, aAngle, true);
                SetItem(i, u1);
            }
        }


        /// <summary>
        /// Vectors rotate
        /// </summary>
        /// <param name="aOrigin"></param>
        /// <param name="aAngle"></param>
        /// <param name="bInRadians"></param>
        internal UVECTORS Rotated(UVECTOR aOrigin, double aAngle, bool bInRadians = false)
        {

            if (Math.Abs(aAngle) <= 0.00000001) { return new UVECTORS(this); }
            if (!bInRadians) { aAngle *= Math.PI / 180; }
            UVECTORS _rVal = new UVECTORS(this, true);


            for (int i = 1; i <= Count; i++)
            {
                UVECTOR u1 = new UVECTOR(Item(i));
                u1.Rotate(aOrigin, aAngle, true);
                _rVal.Add(u1);
            }
            return _rVal;
        }


        /// <summary>
        /// Vectors rotate move
        /// </summary>
        /// <param name="aVectors"></param>
        /// <param name="aOrigin"></param>
        /// <param name="aAngle"></param>
        /// <param name="aXChange"></param>
        /// <param name="aYChange"></param>
        /// <param name="bInRadians"></param>
        public void RotateMove(UVECTOR aOrigin, double aAngle, double aXChange, double aYChange, bool bInRadians = false)
        {
            bool bRotate = false;
            bRotate = Math.Abs(aAngle) > 0.00000001;
            if (!bRotate && aXChange == 0 & aYChange == 0) { return; }
            if (!bInRadians) { aAngle *= Math.PI / 180; }

            UVECTOR u1;
            for (int i = 1; i <= Count; i++)
            {
                u1 = Item(i);
                if (bRotate) { u1.Rotate(aOrigin, aAngle, true); }
                u1.X += aXChange;
                u1.Y += aYChange;
                SetItem(i, u1);
            }
        }

        /// <summary>
        ///#1flag indicating what type of vector to search for
        ////#2the ordinate to search for if the search is ordinate specific
        ///#3an optional coordinate system to use
        ///#4a precision for numerical comparison (1 to 8)
        ///#5returns the index of the matching point
        ///#6flag to return a clone
        ///#7flag to remove the member from the collection
        ///^returns a vector from the collection whose properties or position in the collection match the passed control flag
        /// </summary>
        /// <param name="aControlFlag"></param>
        /// <param name="aOrdinate"></param>
        /// <param name="aPrecis"></param>
        /// <param name="rIndex"></param>
        /// <param name="bRemove"></param>
        /// <returns></returns>
        public UVECTOR GetVector(dxxPointFilters aControlFlag, double aOrdinate = 0, int aPrecis = 4, bool bRemove = false)
        {
            UVECTOR _rVal = UVECTOR.Zero;
            int rIndex = 0;

            if (Count <= 0) return _rVal;

            if (Count == 1) { rIndex = 1; }
            else
            {
                aPrecis = mzUtils.LimitedValue(aPrecis, 1, 15);

                double aOrd = 0;
                double d1 = 0;
                double d2 = 0;
                UVECTOR u1 = UVECTOR.Zero;
                double maxval = 0;
                double minval = 0;
                int id1 = 0;
                int ID2 = 0;

                //===================================================================
                if (aControlFlag >= dxxPointFilters.AtMaxX && aControlFlag <= dxxPointFilters.AtMinZ)
                {
                    //===================================================================
                    //search for vectors at extremes
                    //returns the first one that satisfies
                    if (aControlFlag == dxxPointFilters.AtMinX || aControlFlag == dxxPointFilters.AtMinY) { aOrdinate = System.Double.MaxValue; }
                    else
                    { aOrdinate = System.Double.MinValue; }

                    for (int i = 1; i <= Count; i++)
                    {
                        u1 = (UVECTOR)Item(i).Clone();

                        switch (aControlFlag)
                        {
                            case dxxPointFilters.AtMaxX:
                                if (u1.X > aOrdinate)
                                {
                                    rIndex = i;
                                    aOrdinate = u1.X;
                                }
                                break;
                            case dxxPointFilters.AtMaxY:
                                if (u1.Y > aOrdinate)
                                {
                                    rIndex = i;
                                    aOrdinate = u1.Y;
                                }
                                break;
                            case dxxPointFilters.AtMinX:
                                if (u1.X < aOrdinate)
                                {
                                    rIndex = i;
                                    aOrdinate = u1.X;
                                }
                                break;
                            case dxxPointFilters.AtMinY:
                                if (u1.Y < aOrdinate)
                                {
                                    rIndex = i;
                                    aOrdinate = u1.Y;
                                }
                                break;
                        }
                    }
                }
                else
                {
                    //search for vectors at nearest to the passed ordinate
                    //switch (aControlFlag)
                    //{
                    //===================================================================
                    // CONVERSION: Case was ToX To dxfFarthestFromZ
                    if (aControlFlag >= dxxPointFilters.NearestToX && aControlFlag <= dxxPointFilters.FarthestFromZ)
                    {
                        //===================================================================
                        rIndex = 1;
                        id1 = 1;
                        ID2 = 1;
                        u1 = (UVECTOR)Item(1).Clone();


                        switch (aControlFlag)
                        {
                            case dxxPointFilters.NearestToX:
                                d1 = Math.Abs(u1.X - aOrdinate);
                                break;
                            case dxxPointFilters.NearestToY:
                                d1 = Math.Abs(u1.Y - aOrdinate);
                                break;
                            case dxxPointFilters.NearestToZ:
                                return _rVal;
                        }
                        minval = d1;
                        maxval = d1;

                        for (int i = 2; i <= Count; i++)
                        {
                            u1 = (UVECTOR)Item(i).Clone();
                            switch (aControlFlag)
                            {
                                case dxxPointFilters.NearestToX:
                                    d2 = Math.Abs(u1.X - aOrdinate);
                                    break;
                                case dxxPointFilters.NearestToY:
                                    d2 = Math.Abs(u1.Y - aOrdinate);
                                    break;
                            }

                            if (d2 >= maxval)
                            {
                                id1 = i;
                                maxval = d2;
                            }
                            if (d2 <= minval)
                            {
                                ID2 = i;
                                minval = d2;
                            }
                        }
                        rIndex = id1;
                        if (aControlFlag == dxxPointFilters.NearestToX || aControlFlag == dxxPointFilters.NearestToY || aControlFlag == dxxPointFilters.NearestToZ)
                        {
                            rIndex = ID2;
                        }
                        //===================================================================
                        // break;
                    }
                    // CONVERSION: Case was dxfAtX To dxfAtZ
                    if (aControlFlag >= dxxPointFilters.AtX && aControlFlag <= dxxPointFilters.AtZ)
                    {
                        //===================================================================

                        //searching for a vector at a particular ordinate (ie at X = 10)
                        //returns the first one that satisfies
                        for (int i = 1; i <= Count; i++)
                        {
                            u1 = Item(i).Clone(true);
                            switch (aControlFlag)
                            {
                                case dxxPointFilters.AtX:
                                    d1 = Math.Abs(u1.X - aOrdinate);
                                    break;
                                case dxxPointFilters.AtY:
                                    d1 = Math.Abs(u1.Y - aOrdinate);
                                    break;
                                case dxxPointFilters.AtZ:
                                    return _rVal;
                            }
                            if (Math.Round(Math.Abs(d1), aPrecis) == 0)
                            {
                                rIndex = i;
                                break;
                            }
                        }


                        //===================================================================
                        // break;
                    }
                    // CONVERSION: Case was dxfGetTopLeft To dxfGetRightBottom
                    if (aControlFlag >= dxxPointFilters.GetTopLeft && aControlFlag <= dxxPointFilters.GetRightBottom)
                    {
                        //===================================================================

                        //searching for a relative vector (lower left, top right etc. etc.)
                        switch (aControlFlag)
                        {
                            case dxxPointFilters.GetBottomLeft:
                                aOrd = Math.Round(GetExtremeOrd(true, true, aPrecis), aPrecis);
                                d1 = System.Double.MaxValue;
                                for (int i = 1; i <= Count; i++)
                                {
                                    u1 = (UVECTOR)Item(i).Clone();
                                    if (Math.Round(u1.Y, aPrecis) == aOrd)
                                    {
                                        if (u1.X < d1)
                                        {
                                            d1 = u1.X;
                                            rIndex = i;
                                        }
                                    }
                                }
                                break;
                            case dxxPointFilters.GetLeftBottom:
                                aOrd = Math.Round(GetExtremeOrd(true, false, aPrecis), aPrecis);
                                d1 = System.Double.MaxValue;
                                for (int i = 1; i <= Count; i++)
                                {
                                    u1 = (UVECTOR)Item(i).Clone();
                                    if (Math.Round(u1.X, aPrecis) == aOrd)
                                    {
                                        if (u1.Y < d1)
                                        {
                                            d1 = u1.Y;
                                            rIndex = i;
                                        }
                                    }
                                }
                                break;
                            case dxxPointFilters.GetBottomRight:
                                aOrd = Math.Round(GetExtremeOrd(true, true, aPrecis), aPrecis);
                                d1 = System.Double.MinValue;
                                for (int i = 1; i <= Count; i++)
                                {
                                    u1 = (UVECTOR)Item(i).Clone();
                                    if (Math.Round(u1.Y, aPrecis) == aOrd)
                                    {
                                        if (u1.X > d1)
                                        {
                                            d1 = u1.X;
                                            rIndex = i;
                                        }
                                    }
                                }
                                break;
                            case dxxPointFilters.GetRightBottom:
                                aOrd = Math.Round(GetExtremeOrd(false, false, aPrecis), aPrecis);
                                d1 = System.Double.MaxValue;
                                for (int i = 1; i <= Count; i++)
                                {
                                    u1 = (UVECTOR)Item(i).Clone();
                                    if (Math.Round(u1.X, aPrecis) == aOrd)
                                    {
                                        if (u1.Y < d1)
                                        {
                                            d1 = u1.Y;
                                            rIndex = i;
                                        }
                                    }
                                }
                                break;
                            case dxxPointFilters.GetTopLeft:
                                aOrd = Math.Round(GetExtremeOrd(false, true, aPrecis), aPrecis);
                                d1 = System.Double.MaxValue;
                                for (int i = 1; i <= Count; i++)
                                {
                                    u1 = (UVECTOR)Item(i).Clone();
                                    if (Math.Round(u1.Y, aPrecis) == aOrd)
                                    {
                                        if (u1.X < d1)
                                        {
                                            d1 = u1.X;
                                            rIndex = i;
                                        }
                                    }
                                }
                                break;
                            case dxxPointFilters.GetLeftTop:
                                aOrd = Math.Round(GetExtremeOrd(true, false, aPrecis), aPrecis);
                                d1 = System.Double.MinValue;
                                for (int i = 1; i <= Count; i++)
                                {
                                    u1 = (UVECTOR)Item(i).Clone();
                                    if (Math.Round(u1.X, aPrecis) == aOrd)
                                    {
                                        if (u1.Y > d1)
                                        {
                                            d1 = u1.Y;
                                            rIndex = i;
                                        }
                                    }
                                }
                                break;
                            case dxxPointFilters.GetTopRight:
                                aOrd = Math.Round(GetExtremeOrd(false, true, aPrecis), aPrecis);
                                d1 = System.Double.MinValue;
                                for (int i = 1; i <= Count; i++)
                                {
                                    u1 = (UVECTOR)Item(i).Clone();
                                    if (Math.Round(u1.Y, aPrecis) == aOrd)
                                    {
                                        if (u1.X > d1)
                                        {
                                            d1 = u1.X;
                                            rIndex = i;
                                        }
                                    }
                                }
                                break;
                            case dxxPointFilters.GetRightTop:
                                aOrd = Math.Round(GetExtremeOrd(false, false, aPrecis), aPrecis);
                                d1 = System.Double.MinValue;
                                for (int i = 1; i <= Count; i++)
                                {
                                    u1 = (UVECTOR)Item(i).Clone();
                                    if (Math.Round(u1.X, aPrecis) == aOrd)
                                    {
                                        if (u1.Y > d1)
                                        {
                                            d1 = u1.Y;
                                            rIndex = i;
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }
            }

            if (rIndex > 0)
            {
                _rVal = Item(rIndex);
                if (bRemove) { Remove(rIndex); }
            }
            return _rVal;
        }

        /// <summary>
        /// move vectors
        /// </summary>
        /// <param name="aXChange"></param>
        /// <param name="aYChange"></param>
        public void Move(double aXChange = 0, double aYChange = 0)
        {
            if ((aXChange == 0 & aYChange == 0) || Count ==0) return; 
            
            for (int i = 1; i <= Count; i++)
            {
                UVECTOR u1 = Item(i);
                u1.X += aXChange;
                u1.Y += aYChange;
                SetItem(i, u1);
            }
            _Bounds.Move(aXChange, aYChange);
        }

        public void Project(UVECTOR aDirection, double aDistance, bool bSuppressNormalize = false, bool bInvertDirection = false)
        {
            if (!mzUtils.IsNumeric(aDistance) || Count <= 0) { return; }
            if (aDistance == 0) { return; }

            UVECTOR aDir;
            bool aFlg = false;
            if (!bSuppressNormalize)
            { aDir = aDirection.Normalized(out aFlg); }
            if (aFlg) return;

            aDir = aDirection;


            if (aDistance <= 0 || bInvertDirection) { aDir *= -1; }
            aDistance = Math.Abs(aDistance);
            for (int i = 1; i <= Count; i++)
            {
                _Members[i - 1].Project(aDir, aDistance, bSuppressNormalize: true, bInvertDirection = false);
            }

        }

        /// <summary>
        ////#1the X coordinate to match
        ///#2the Y coordinate to match
        ///#3a precision for the comparison (1 to 16)
        ///^searchs for and returns vectors from the collection whose coordinates match the passed coordinates
        ///~if an any of the ordinates (X, Y or Z) are not passed or are not numeric they are not used in the comparison.
        ///~say ony an X value is passed, then all the vectors with the same X ordinate are returned regarless of their
        ///~respective Y and Z ordinate values.
        /// </summary>
        /// <param name="aVectors"></param>
        /// <param name="aX"></param>
        /// <param name="aY"></param>
        /// <param name="aPrecis"></param>
        /// <param name="bRemove"></param>
        /// <param name="bJustOne"></param>
        /// <returns></returns>
        public UVECTORS GetAtCoordinate(dynamic aX = null, dynamic aY = null, int aPrecis = 3, bool bRemove = false, bool bJustOne = false)
        {
            UVECTORS _rVal = new UVECTORS(this, true);
            bool bTestX = mzUtils.IsNumeric(aX);
            bool bTestY = mzUtils.IsNumeric(aY);
            if (!bTestX && !bTestY) return _rVal;
            if (Count <= 0) return _rVal;
            aPrecis = mzUtils.LimitedValue(aPrecis, 0, 15);

            UVECTOR u1;
            bool breturn = false;
            double xval = 0;
            double yVal = 0;
            int cnt = 0;
            TVALUES rIndices = new TVALUES("");
            UVECTOR[] keepers = new UVECTOR[0];
            if (bTestX) { xval = mzUtils.VarToDouble(aX); }
            if (bTestY) { yVal = mzUtils.VarToDouble(aY); }

            for (int i = 1; i <= Count; i++)
            {
                u1 = Item(i);
                breturn = false;
                if (bTestX)
                {
                    if (Math.Abs(Math.Round(u1.X - xval, aPrecis)) == 0) { breturn = true; }
                }
                if (bTestX && !breturn)
                {
                    if (Math.Abs(Math.Round(u1.Y - yVal, aPrecis)) == 0) { breturn = true; }
                }
                if (breturn)
                {
                    rIndices.Add(i);
                    _rVal.Add(u1);
                    if (bJustOne) break;
                }
                else
                {
                    if (bRemove)
                    {
                        cnt += 1;
                        Array.Resize<UVECTOR>(ref keepers, cnt);
                        keepers[cnt - 1] = u1;

                    }
                }

            }

            if (bRemove && cnt > 0)
            {
                _Members = keepers;
                _Count = cnt;
            }

            return _rVal;
        }

        public bool ContainsVector(UVECTOR aVector, int aPrecis = 4) => EqualVectors(aVector, aPrecis, BailOnOne: true).Count > 0;


        public List<UVECTOR> EqualVectors(UVECTOR aVector, int aPrecis = 4, bool BailOnOne = false)
        {
            List<UVECTOR> _rVal = new List<UVECTOR>();
            UVECTOR mem;
            aPrecis = mzUtils.LimitedValue(aPrecis, 0, 15);


            for (int i = 1; i <= Count; i++)
            {
                mem = _Members[i - 1];
                if (Math.Round(mem.DistanceTo(aVector), aPrecis) == 0)
                {
                    _rVal.Add(mem);
                    if (BailOnOne) break;
                }
            }
            return _rVal;
        }

        public bool Sort(dxxSortOrders aOrder, iVector aReferencePt = null, int aPrecis = 3)
        {
            if (Count <= 1) return false;

            uopVectors mems = new uopVectors(this);
            bool _rVal = false;

            List<iVector> sorted = dxfVectors.Sort(mems, aOrder, aReferencePt, aPrecis);

            UVECTOR[] _newMems = new UVECTOR[sorted.Count];
            for (int i = 1; i <= sorted.Count; i++)
            {
                uopVector mem = (uopVector)sorted[i - 1];
                if (mem.Index != i) _rVal = true;
                _newMems[i - 1] = new UVECTOR(mem);
            }
            _Members = _newMems;

            return _rVal;
        }

        /// <summary>
        /// vectors line segments
        /// </summary>
        /// <param name="bClosed"></param>
        /// <param name="bSuppressNulls"></param>
        /// <param name="aNullLength"></param>
        /// <returns></returns>
        public List<uopLine> ToLines(bool bClosed = false, bool bSuppressNulls = false, double aNullLength = 0.001)
        {
            List<uopLine> _rVal = new List<uopLine>();
            if (Count <= 1) { return _rVal; }
            aNullLength = Math.Abs(aNullLength);

            UVECTOR u1;
            UVECTOR u2;

            for (int i = 1; i <= Count; i++)
            {
                u1 = Item(i);
                if (i < Count)
                { u2 = Item(i + 1); }
                else
                {
                    if (!bClosed) break;

                    u2 = Item(1);
                }
                if (!bSuppressNulls)
                {
                    _rVal.Add(new uopLine(u1, u2));
                }
                else
                {
                    if (u1.DistanceTo(u2, 6) <= aNullLength)
                    {
                        _rVal.Add(new uopLine(u1, u2));
                    }
                }
            }
            return _rVal;
        }

        #region Shared Methods

        public static UVECTORS Zero { get { return new UVECTORS(false, ""); } }

        public static void GetOrdinateLists(UVECTORS aVectors, out List<double> XVals, out List<double> YVals, int iPrecis = 6, bool bUnique = true)
        {
            XVals = new List<double>();
            YVals = new List<double>();
            UVECTOR u1;
            bool bAdd = false;
            double ord;

            iPrecis = mzUtils.LimitedValue(iPrecis, 0, 15);


            for (int i = 1; i <= aVectors.Count; i++)
            {
                u1 = aVectors.Item(i);
                bAdd = true;
                ord = Math.Round(u1.X, iPrecis);
                if (bUnique)
                {
                    for (int j = 0; j < XVals.Count; j++)
                    {
                        if (XVals[j] == ord)
                        {
                            bAdd = false;
                            break;
                        }
                    }
                }
                if (bAdd) XVals.Add(ord);

                ord = Math.Round(u1.Y, iPrecis);
                bAdd = true;
                if (bUnique)
                {
                    for (int j = 0; j < YVals.Count; j++)
                    {
                        if (YVals[j] == ord)
                        {
                            bAdd = false;
                            break;
                        }
                    }
                }
                if (bAdd) YVals.Add(ord);

            }

        }

        public static List<double> GetOrdinateList(UVECTORS aVectors, bool bGetY, int iPrecis = 6, bool bUnique = true)
        {
            List<double> _rVal = new List<double>();
            UVECTOR u1;
            bool bAdd = false;
            double ord;

            iPrecis = mzUtils.LimitedValue(iPrecis, 0, 15);


            for (int i = 1; i <= aVectors.Count; i++)
            {
                u1 = aVectors.Item(i);
                bAdd = true;
                ord = (!bGetY) ? Math.Round(u1.X, iPrecis) : Math.Round(u1.Y, iPrecis);
                if (bUnique)
                {
                    for (int j = 0; j < _rVal.Count; j++)
                    {
                        if (_rVal[j] == ord)
                        {
                            bAdd = false;
                            break;
                        }
                    }
                }
                if (bAdd) _rVal.Add(ord);


            }

            return _rVal;
        }
        internal static URECTANGLE ComputeBounds(UVECTORS aVectors) => ComputeBounds(aVectors, out _, out _);

        internal static URECTANGLE ComputeBounds(UVECTORS aVectors, out double rWidth, out double rHeight)
        {
            rWidth = 0;
            rHeight = 0;

            if (aVectors.Count <= 0) { return new URECTANGLE(0, 0, 0, 0); }
            UVECTOR u1 = aVectors.Item(1);
            URECTANGLE _rVal = new URECTANGLE(u1.X, u1.Y, u1.X, u1.Y);


            if (aVectors.Count <= 1) return _rVal;


            for (int i = 2; i <= aVectors.Count; i++)
            {
                u1 = aVectors.Item(i);
                if (u1.X < _rVal.Left) { _rVal.Left = u1.X; }
                if (u1.X > _rVal.Right) { _rVal.Right = u1.X; }
                if (u1.Y > _rVal.Top) { _rVal.Top = u1.Y; }
                if (u1.Y < _rVal.Bottom) { _rVal.Bottom = u1.Y; }

            }

            rWidth = _rVal.Width;
            rHeight = _rVal.Height;
            return _rVal;
        }

        /// <summary>
        /// returns true if the passed vectors are equal within the passed precision
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <param name="bCompareInverse"></param>
        /// <param name="aPrecis"></param>
        /// <param name="rInverseIsEqual"></param>
        /// <param name="bNormalize"></param>
        /// <returns></returns>
        public static bool AreEqual(UVECTOR A, UVECTOR B, int aPrecis = 0)
        {
            return AreEqual(A, B, false, aPrecis, out bool INVEQ, false);
        }


        /// <summary>
        /// returns true if the passed vectors are equal within the passed precision
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <param name="bCompareInverse"></param>
        /// <param name="aPrecis"></param>
        /// <param name="rInverseIsEqual"></param>
        /// <param name="bNormalize"></param>
        /// <returns></returns>
        public static bool AreEqual(UVECTOR A, UVECTOR B, bool bCompareInverse, int aPrecis, out bool rInverseIsEqual, bool bNormalize = false)
        {
            rInverseIsEqual = false;

            UVECTOR d1;
            UVECTOR d2;

            if (bNormalize)
            {
                d1 = A.Normalized();
                d2 = B.Normalized();
            }
            else
            {
                d1 = (UVECTOR)A.Clone();
                d2 = (UVECTOR)B.Clone(); ;
            }

            bool _rVal = d1.Compare(d2, aPrecis);

            if (bCompareInverse && !_rVal)
            {
                rInverseIsEqual = d1.Compare(d2 * -1, aPrecis);
                if (rInverseIsEqual) _rVal = true;

            }
            return _rVal;
        }



        /// <summary>
        /// Vectors Compare Set
        /// </summary>
        /// <param name="aVectors"></param>
        /// <param name="bVectors"></param>
        /// <param name="bInvertA"></param>
        /// <param name="bInvertB"></param>
        /// <param name="bCompareValues"></param>
        /// <param name="bInOrderOnly"></param>
        /// <returns></returns>
        public static bool CompareSet(UVECTORS aVectors, UVECTORS bVectors, bool bInvertA = false, bool bInvertB = false, bool bCompareValues = false, bool bInOrderOnly = false)
        {

            if (aVectors.Count != bVectors.Count) { return false; }
            if (aVectors.Count == 0) { return true; }

            URECTANGLE aLims = aVectors.Bounds;
            URECTANGLE bLims = bVectors.Bounds;

            //get the bounding rectangles
            //if their dimensions don't match the points cant match

            if (Math.Round(Math.Abs(aLims.Width - bLims.Width), 3) != 0) { return false; }
            if (Math.Round(Math.Abs(aLims.Height - bLims.Height), 3) != 0) { return false; }

            bool _rVal = true;


            //get the rectangle centers and compute then offset between then
            UVECTOR cp1 = aLims.Center;
            UVECTOR cp2 = bLims.Center;
            UVECTORS aVs = new UVECTORS(aVectors);
            UVECTORS bVs = new UVECTORS(bVectors);

            UVECTOR trns = cp1 - cp2;
            bool[] aFlgs = new bool[aVs.Count];


            for (int i = 1; i <= aVs.Count; i++)
            {
                UVECTOR u1 = aVs.Item(i);
                u1.Suppressed = false;
                if (bInvertA)
                {
                    u1.Rotate(cp1, 180);
                    aVs.SetItem(i, u1);
                    //             aVs.Members[i ].Value = goDXFUtils.NormalizeAngle(aVs.Members[i ].Value + 180, , True)
                }

                u1 = aVs.Item(i);
                //find a B point that is the same as the A point
                bool bFnd = false;
                int si = 0;
                int ei = bVs.Count - 1;

                if (bInOrderOnly)
                {
                    if (bInvertB)
                    {
                        si = i; //aVs.Count - 2 + 1
                    }
                    else
                    {
                        si = i;
                    }
                    ei = si;
                }


                for (int j = si; j <= ei; j++)
                {
                    UVECTOR u2 = bVs.Item(j + 1);
                    if (!aFlgs[j])
                    {
                        aFlgs[j] = true;

                        u2.Suppressed = false;
                        if (bInvertB)
                        {
                            u2.Rotate(cp2, 180);
                            //            bVs.Members[i ].Value = goDXFUtils.NormalizeAngle(bVs.Members[i ].Value + 180, , True)

                        }
                        //move the B vectors to a point relative to the A vectors center
                        u2.X += trns.X;
                        u2.Y += trns.Y;

                        bVs.SetItem(j + 1, u2);
                    }


                    if (!u2.Suppressed)
                    { //only compare to points that have not already been matched
                        if (u1.DistanceTo(u2, 3) == 0)
                        { //see if they are at the same location
                            if (!bCompareValues || (bCompareValues && Math.Round(u1.Value - u2.Value, 3) == 0))
                            { //compare values if requested
                                bFnd = true;

                                u2.Suppressed = true; //mark the vector as matched
                                bVs.SetItem(j + 1, u2);
                                break;
                            }
                        }
                    }
                }
                if (!bFnd)
                {
                    _rVal = false; //bail if no match was found
                    return _rVal;
                }
            }
            return _rVal;
        }

        /// <summary>
        /// vectors from string
        /// ~a value string is X,Y, Value
        /// </summary>
        /// <param name="aValString"></param>
        /// <param name="aDelimiter"></param>
        /// <param name="bReturnValueList"></param>
        /// <param name="rValueList"></param>
        /// <param name="bUnsuppressedValsOnly"></param>
        /// <returns></returns>
        internal static UVECTORS FromString(string aValString, string aDelimiter, bool bReturnValueList, out string rValueList, bool bUnsuppressedValsOnly = false)
        {
            rValueList = string.Empty;
            UVECTORS _rVal = UVECTORS.Zero;

            aDelimiter ??= string.Empty;
            aDelimiter = mzUtils.ThisOrThat(aDelimiter.Trim(), uopGlobals.Delim);

            List<string> aVals = mzUtils.ListValues(aValString, aDelimiter);
            int acnt = aVals.Count;

            for (int i = 0; i < acnt; i++)
            {
                string vStr = aVals[i];
                double vX = 0;
                double vY = 0;
                double vV = 0;
                int iSup = 0;

                if (vStr.StartsWith("(") && vStr.EndsWith(")")) vStr = vStr.Replace("(", "").Replace(")", "");

                List<string> vVals = mzUtils.ListValues(vStr, ",");
                int vcnt = vVals.Count;

                if (vcnt >= 1) vX = mzUtils.VarToDouble(vVals[0]);

                if (vcnt >= 2) vY = mzUtils.VarToDouble(vVals[1]);

                if (vcnt >= 3) vV = mzUtils.VarToDouble(vVals[2]);


                if (vcnt >= 4) iSup = mzUtils.VarToInteger(vVals[3]);



                UVECTOR u1 = _rVal.Add(vX, vY, 0, 0, vV, "", iSup == -1);
                if (bReturnValueList)
                {
                    if (!bUnsuppressedValsOnly || (bUnsuppressedValsOnly && !u1.Suppressed))
                    {
                        mzUtils.ListAdd(ref rValueList, string.Format("0.0####", u1.Value), bSuppressTest: true);
                    }
                }
            }
            return _rVal;
        }

        /// <summary>
        /// vectors from string
        /// ~a value string is X,Y, Value
        /// </summary>
        /// <param name="aValString"></param>
        /// <param name="aDelimiter"></param>
        /// <param name="bReturnValueList"></param>
        /// <param name="rValueList"></param>
        /// <param name="bUnsuppressedValsOnly"></param>
        /// <returns></returns>
        internal static UVECTORS FromString(string aValString, string aDelimiter)
        {
            return FromString(aValString, aDelimiter, false, out List<double> _, false);
        }
        /// <summary>
        /// vectors from string
        /// ~a value string is X,Y, Value
        /// </summary>
        /// <param name="aValString"></param>
        /// <param name="aDelimiter"></param>
        /// <param name="bReturnValueList"></param>
        /// <param name="rValueList"></param>
        /// <param name="bUnsuppressedValsOnly"></param>
        /// <returns></returns>
        internal static UVECTORS FromString(string aValString, string aDelimiter, bool bReturnValueList, out List<double> rValueList, bool bUnsuppressedValsOnly = false)
        {
            rValueList = new List<double>();
            UVECTORS _rVal = UVECTORS.Zero;

            aDelimiter ??= string.Empty;
            aDelimiter = mzUtils.ThisOrThat(aDelimiter.Trim(), uopGlobals.Delim);

            List<string> aVals = mzUtils.ListValues(aValString, aDelimiter);
            int acnt = aVals.Count;

            for (int i = 0; i < acnt; i++)
            {
                string vStr = aVals[i];
                double vX = 0;
                double vY = 0;
                double vV = 0;
                int iSup = 0;

                if (vStr.StartsWith("(") && vStr.EndsWith(")")) vStr = vStr.Replace("(", "").Replace(")", "");

                List<string> vVals = mzUtils.ListValues(vStr, ",");
                int vcnt = vVals.Count;
                if (vcnt >= 1) vX = mzUtils.VarToDouble(vVals[0]);
                if (vcnt >= 2) vY = mzUtils.VarToDouble(vVals[1]);
                if (vcnt >= 3) vV = mzUtils.VarToDouble(vVals[2]);
                if (vcnt >= 4) iSup = mzUtils.VarToInteger(vVals[3]);
                UVECTOR u1 = _rVal.Add(vX, vY, 0, 0, vV, "", iSup == -1);
                if (bReturnValueList)
                {
                    if (!bUnsuppressedValsOnly || (bUnsuppressedValsOnly && !u1.Suppressed))
                    {
                        rValueList.Add(u1.Value);
                    }
                }
            }
            return _rVal;
        }
        /// <summary>
        /// vectors from vectors
        /// </summary>
        /// <param name="aVectors"></param>
        /// <param name="bUnsuppressOnly"></param>
        /// <returns></returns>
        internal static UVECTORS FromDXFVectors(colDXFVectors aVectors, bool bUnsuppressOnly = false, bool bRotationsAsValues = false)
        {
            UVECTORS _rVal = UVECTORS.Zero;
            if (aVectors == null) return _rVal;

            dxfVector v1;

            for (int i = 1; i <= aVectors.Count; i++)
            {
                v1 = aVectors.Item(i);
                if (!bUnsuppressOnly || (bUnsuppressOnly && !v1.Suppressed))
                { _rVal.Add(UVECTOR.FromDXFVector(v1, bRotationsAsValues)); }
            }
            return _rVal;
        }

        /// <summary>
        /// vectors from vectors
        /// </summary>
        /// <param name="aVectors"></param>
        /// <param name="bUnsuppressOnly"></param>
        /// <returns></returns>
        internal static UVECTORS FromDXFVectors(List<dxfVector> aVectors, bool bUnsuppressOnly = false)
        {
            UVECTORS _rVal = UVECTORS.Zero;
            if (aVectors == null) return _rVal;

            dxfVector v1;

            for (int i = 1; i <= aVectors.Count; i++)
            {
                v1 = aVectors[i - 1];
                if (!bUnsuppressOnly || (bUnsuppressOnly && !v1.Suppressed))
                { _rVal.Add(UVECTOR.FromDXFVector(v1)); }
            }
            return _rVal;
        }

        /// <summary>
        /// vectors match
        /// </summary>
        /// <param name="aVectors"></param>
        /// <param name="bVectors"></param>
        /// <param name="aPrecis"></param>
        /// <returns></returns>
        public static bool Match(UVECTORS aVectors, UVECTORS bVectors, int aPrecis = 4)
        {
            if (aVectors.Count != bVectors.Count) { return false; }
            bool _rVal = true;
            UVECTOR u1;
            //UVECTOR u2;
            bool bFound = false;
            List<UVECTOR> vCol1 = aVectors.ToList;
            List<UVECTOR> vCol2 = bVectors.ToList;
            int idx = 0;
            aPrecis = mzUtils.LimitedValue(aPrecis, 0, 15);
            for (int i = 1; i <= vCol1.Count; i++)
            {
                u1 = vCol1[i - 1];
                idx = vCol2.FindIndex(v => Math.Round(v.X, aPrecis) == Math.Round(u1.X, aPrecis) && Math.Round(v.Y, aPrecis) == Math.Round(u1.Y, aPrecis));
                bFound = idx >= 0;
                if (!bFound)
                {
                    _rVal = false;
                    return _rVal;

                }
                else
                {
                    vCol2.RemoveAt(idx);
                }
            }
            _rVal = vCol2.Count == 0;
            return _rVal;
        }

        /// <summary>
        /// vectors match
        /// </summary>
        /// <param name="aVectors"></param>
        /// <param name="bVectors"></param>
        /// <param name="aPrecis"></param>
        /// <returns></returns>
        public static bool MatchPlanar(UVECTORS aVectors, UVECTORS bVectors, dxfPlane aPlane = null, int aPrecis = 3)
        {
            if (aVectors.Count != bVectors.Count) return false;

            colDXFVectors vcol1 = aVectors.ToDXFVectors(aZ: 0);
            colDXFVectors vcol2 = bVectors.ToDXFVectors(aZ: 0);
            aPlane ??= dxfPlane.World;
            return dxfVectors.MatchPlanar(vcol1, vcol2, aPlane, aPrecis);

        }

        #endregion Shared Methods


        #region Operators

        public static UVECTORS operator +(UVECTORS A, UVECTORS B) { UVECTORS _rVal = new UVECTORS(A); _rVal.Append(B); return _rVal; }

        //public static UVECTOR operator -(UVECTOR A, UVECTOR B) { UVECTOR _rVal = A.Clone(true); _rVal.X -= B.X; _rVal.Y -= B.Y; return _rVal; }
        //public static UVECTOR operator *(UVECTOR A, double aScaler) { UVECTOR _rVal = A.Clone(true); _rVal.X *= aScaler; _rVal.Y *= aScaler; return _rVal; }
        //public static bool operator ==(UVECTOR A, UVECTOR B) { return A.Compare(B, 4); }
        //public static bool operator !=(UVECTOR A, UVECTOR B) => !(A == B);

        #endregion

    }

    internal struct ULINE : ICloneable
    {

        #region Fields

        public UVECTOR sp;
        public UVECTOR ep;
        public int Row;
        public int Col;
        public int Index;
        public UVECTORS Points;
        public uppSides Side;
        public bool Suppressed;
       
        #endregion Fields

        #region Constructors

        public ULINE(int aIndex = -1)
        {
            Side = uppSides.Undefined;
            sp = UVECTOR.Zero;
            ep = UVECTOR.Zero;
            Row = 0;
            Col = 0;
            Index = aIndex;
            Points = UVECTORS.Zero;
            Suppressed = false;
            Points.Add(sp);
            Points.Add(ep);
        }
        public ULINE(string aTag)
        {
            Side = uppSides.Undefined;
            sp = UVECTOR.Zero;
            ep = UVECTOR.Zero;
            Row = 0;
            Col = 0;
            Index = -1;
            Suppressed = false;
            Points = UVECTORS.Zero;
            Tag = aTag;

            Points.Add(sp);
            Points.Add(ep);

        }
        public ULINE(iLine aLine, string aTag = null, uppSides aSide = uppSides.Undefined)
        {
            Side = aSide;
            sp = aLine == null ? UVECTOR.Zero : new UVECTOR(aLine.StartPt);
            ep = aLine == null ? UVECTOR.Zero : new UVECTOR(aLine.EndPt);
            Row = 0;
            Col = 0;
            Index = -1;
            Suppressed = false;
            Points = UVECTORS.Zero;
            if (aTag != null) Tag = aTag;
            Points.Add(sp);
            Points.Add(ep);
            if (aLine == null) return;


            if (aLine is uopLine)
            {
                uopLine uline = (uopLine)aLine;
                Side = uline.Side;
                Row = uline.Row;
                Col = uline.Col;
                Index = uline.Index;
                Suppressed = uline.Suppressed;
                Points = new UVECTORS(uline.Points);
            }
            else if (aLine is dxeLine)
            {
                dxeLine uline = (dxeLine)aLine;
                Tag = uline.Tag;
                Row = uline.Row;
                Col = uline.Col;
                Index = uline.Index;
            }
            if (aTag != null) Tag = aTag;
        }

        public ULINE(ULINE aLine, uppSides aSide = uppSides.Undefined)
        {
            Side = aLine.Side;
            sp = new UVECTOR(aLine.sp);
            ep = new UVECTOR(aLine.ep);
            Row = aLine.Row;
            Col = aLine.Col;
            Index = aLine.Index;
            Suppressed = aLine.Suppressed;
            Points = new UVECTORS(aLine.Points);
            if (aSide != uppSides.Undefined) Side = aSide;
        }

        public ULINE(ULINE? aLine, uppSides aSide = uppSides.Undefined)
        {

            Side = uppSides.Undefined;
            sp = UVECTOR.Zero;
            ep = UVECTOR.Zero;
            Row = 0;
            Col = 0;
            Index = -1;
            Points = UVECTORS.Zero;
            Suppressed = false;
            Points.Add(sp);
            Points.Add(ep);
            if (aLine.HasValue)
            {
                Side = aLine.Value.Side;
                sp = new UVECTOR(aLine.Value.sp);
                ep = new UVECTOR(aLine.Value.ep);
                Row = aLine.Value.Row;
                Col = aLine.Value.Col;
                Index = aLine.Value.Index;
                Suppressed = aLine.Value.Suppressed;
                Points = new UVECTORS(aLine.Value.Points);
                if (aSide != uppSides.Undefined) Side = aSide;
            }


        }
        public ULINE(UVECTOR aSP, UVECTOR aEP, string aTag = "", uppSides aSide = uppSides.Undefined, UARC? aTrimArc = null)
        {
            Side = aSide;
            sp = new UVECTOR(aSP);
            ep = new UVECTOR(aEP);
            Suppressed = false;
            Row = 0;
            Col = 0;
            Index = 0;
            Points = UVECTORS.Zero;
            if (aTag != null) Tag = aTag;
            if (!aTrimArc.HasValue)
            {
                Points.Add(sp);
                Points.Add(ep);
                return;
            }

            if (aTrimArc.Value.Radius == 0)
            {
                Points.Add(sp);
                Points.Add(ep);
                return;
            }
            UVECTORS ips = Intersections(aTrimArc.Value, true, true);
            if (ips.Count >= 2)
            {
                sp = ips.Item(1);
                ep = ips.Item(2);
            }
            Points.Add(sp);
            Points.Add(ep);

        }

        public ULINE(iVector aSP, iVector aEP, string aTag = "", uppSides aSide = uppSides.Undefined)
        {
            Side = aSide;
            sp = new UVECTOR(aSP);
            ep = new UVECTOR(aEP);
            Row = 0;
            Col = 0;
            Index = 0;
            Suppressed = false;
            Points = UVECTORS.Zero;
            if (aTag != null) Tag = aTag;

            Points.Add(sp);
            Points.Add(ep);
        }

        public ULINE(double aSPX, double aSPY, double aEPX, double aEPY, int aRow = 0, int aCol = 0, string aTag = "", uppSides aSide = uppSides.Undefined)
        {
            Side = aSide;
            sp = new UVECTOR(aSPX, aSPY);
            ep = new UVECTOR(aEPX, aEPY);
            Suppressed = false;
            Row = aRow;
            Col = aCol;
            Index = 0;
            Points = UVECTORS.Zero;
            if (aTag != null) Tag = aTag;
            Points.Add(sp);
            Points.Add(ep);
        }

        public ULINE(dxfPlane aPlane, double aLength, double aXOffset = 0, double aYOffset = 0, double aRotation = 0, UARC? aTrimArc = null)
        {
            Side = uppSides.Undefined;
            sp = UVECTOR.Zero;
            ep = UVECTOR.Zero;
            Row = 0;
            Col = 0;
            Index = -1;
            Points = UVECTORS.Zero;
            Suppressed = false;
            dxfPlane plane = new dxfPlane(aPlane: aPlane, aRotation: aRotation);
            UVECTOR mp = new UVECTOR(aPlane.Vector(aXOffset, aYOffset));
            UVECTOR xdir = new UVECTOR(plane.XDirection);
            sp = mp + xdir * -(aLength / 2);
            ep = mp + xdir * (aLength / 2);

            if (!aTrimArc.HasValue) return;
            if (aTrimArc.Value.Radius == 0) return;
            UVECTORS ips = Intersections(aTrimArc.Value, true, true);
            if (ips.Count >= 2)
            {
                sp = ips.Item(1);
                ep = ips.Item(2);
            }
        }

        #endregion Constructors

        #region Properties

        
        public double AngleOfInclination
        {
            get
            {
                double dx = ep.X - sp.X;
                double dy = ep.Y - sp.Y;
                if (dx == 0 & dy == 0) return 0;
                if (dx == 0) return dy > 0 ? 90 : 270;
                if (dy == 0) return dx > 0 ? 0 : 180;
                double ang = Math.Atan(Math.Abs(dx / dy)) * 180 / Math.PI;
                if (dx > 0 && dy > 0) return dy > 0 ? ang : 360 - ang;
                if (dx < 0 && dy > 0) return dy > 0 ? 180 - ang : 180 + ang;
                return ang;
            }
        }

        public double DeltaY => ep.Y - sp.Y;

        public double DeltaX => ep.X - sp.X;

        public double Slope => DeltaX != 0 ? DeltaY / DeltaX : Double.MaxValue;

        public string Tag { get => sp.Tag; set => sp.Tag = value; }
        public string Flag { get => sp.Flag; set => sp.Flag = value; }
        public double Length => sp.DistanceTo(ep, 4);
        public UVECTOR MidPt => sp.MidPt(ep);

        public List<UVECTOR> EndPts => new List<UVECTOR>() { sp, ep };

        public UVECTORS EndPoints => new UVECTORS(sp, ep);

        public double MaxY => Math.Max(ep.Y, sp.Y);
        public double MinY => Math.Min(ep.Y, sp.Y);
        public double MaxX => Math.Max(ep.X, sp.X);
        public double MinX => Math.Min(ep.X, sp.X);
        public double MidY => MinY + (MaxY - MinY) / 2;
        public double MidX => MinX + (MaxX - MinX) / 2;

        internal URECTANGLE Limits => new URECTANGLE(MinX, MaxY, MaxX, MinY);
        public uopRectangle Bounds => new uopRectangle(MinX, MaxY, MaxX, MinY);
        #endregion Properties

        #region Methods

        public double MaxYr(int? aPrecis = null) => uopUtils.MaxValue(sp.Y, ep.Y, aPrecis);
        public double MinYr(int? aPrecis = null) => uopUtils.MinValue(sp.Y, ep.Y, aPrecis);
        public double MaxXr(int? aPrecis = null) => uopUtils.MaxValue(sp.X, ep.X, aPrecis);
        public  double MinXr(int? aPrecis = null) => uopUtils.MinValue(sp.X, ep.X, aPrecis);

        /// <summary>
        /// extends this line to the passed line
        /// </summary>
        /// <param name="aLine">the subject line</param>
        /// <param name="bTrimTo">if the lines intersect and this flag is true the line will be trimmed back to the passed line  </param>
        /// <returns></returns>
        public bool ExtendTo(ULINE aLine, bool bTrimTo = false)
        {
            UVECTOR ip = IntersectionPt(aLine, out _, out _, out bool onMe, out bool onHim, out bool exists);
            if (!exists) return false;
            double d1 = sp.DistanceTo(ip);
            double d2 = ep.DistanceTo(ip);

            if (onMe && !bTrimTo) return false;
           
            if (d1 <= d2) 
            {
                sp.SetOrdinates(ip.X, ip.Y);
                return d1 != 0;
            }
                
            else
            {
                ep.SetOrdinates(ip.X, ip.Y);
                return d2 != 0;
            }
       
        }

        /// <summary>
        /// extends this line to the passed lines
        /// </summary>
        /// <param name="aLines">the subject lines</param>
        /// <param name="bTrimTo">if the lines intersect and this flag is true the line will be trimmed back to the passed line  </param>
        /// <returns></returns>

        public bool ExtendTo(ULINEPAIR aLines, bool bTrimTo = false)
        {
            bool t1 = aLines.Line1.HasValue ? ExtendTo(aLines.Line1.Value, bTrimTo): false ;
            bool t2 = aLines.Line2.HasValue ? ExtendTo(aLines.Line2.Value, bTrimTo) : false;
            return (t1 || t2);
        }


        public double Y(bool bGetEndPt = false) => !bGetEndPt ? sp.Y : ep.Y;
        
        public double X(bool bGetEndPt = false) => !bGetEndPt ? sp.X : ep.X;

        public bool IsHorizontal(int aPrecis = 4) => Math.Round(DeltaY, mzUtils.LimitedValue(aPrecis, 1, 15)) == 0;
        public bool IsVertical(int aPrecis = 4) => Math.Round(DeltaX, mzUtils.LimitedValue(aPrecis, 1, 15)) == 0;
        object ICloneable.Clone() => (object)Clone();

        public ULINE Clone() => new ULINE(this);

        public override string ToString() => string.IsNullOrWhiteSpace(Tag) ? $"ULINE - {sp.X:0.0000},{sp.Y:0.0000} -> {ep.X:0.0000},{ep.Y:0.0000}" : $"ULINE -  {sp.X:0.0000},{sp.Y:0.0000} -> {ep.X:0.0000},{ep.Y:0.0000} {Tag}";

        public UVECTOR Direction() => new UVECTOR(ep - sp, bNormalize: true);

        public double PointValue(int aIndex = 1)
        { return (Points.Count > 0) ? Points.Item(aIndex).Value : 0; }

        public UVECTOR Direction(out bool rDirectionisNull) => (ep - sp).Normalized(out rDirectionisNull);

        public UVECTOR Direction(out bool rDirectionisNull, out double rDistance)
        {
            rDirectionisNull = false; rDistance = 0;
            return (ep - sp).Normalized(out rDirectionisNull, out rDistance);
        }


        public void TrimToPair(ULINEPAIR aPair, bool bExtendTo = true)
        {
            UVECTORS ips = Intersections(aPair, aLineIsInfinite: bExtendTo, aLinesAreInfinite: true);
            if (ips.Count <= 0) return;
            UVECTOR u1 = ips.Nearest(sp);
            sp.SetOrdinates(u1.X, u1.Y);
            u1 = ips.Nearest(ep);
            ep.SetOrdinates(u1.X, u1.Y);

        }

        public void Mirror(double? aX, double? aY)
        {
            if (!aX.HasValue && !aY.HasValue) return;
            sp.Mirror(aX, aY);
            ep.Mirror(aX, aY);
            Points.Mirror(aX, aY);
        }

        public ULINE Mirrored(double? aX, double? aY)
        {
            ULINE _rVal = new ULINE(this);
            _rVal.Mirror(aX, aY);
            return _rVal;
        }

        public dxeLine ToDXFLine(dxfDisplaySettings aDisplay = null) => new dxeLine(sp.ToDXFVector(), ep.ToDXFVector()) { Row = Row, Col = Col, DisplaySettings = aDisplay };

        // <summary>
        /// returns true if the passed vector lies on the line
        /// </summary>
        /// <param name="aVector" the vector to test></param>
        /// <param name="aPrecis" a precision apply></param>
        /// <param name="bTreatAsInfinite" flag to treat the line as infinite></param>
        /// <returns></returns>
        public bool ContainsVector(UVECTOR aVector, int aPrecis = 5, bool bTreatAsInfinite = false)
        { return ContainsVector(aVector, aPrecis, out bool ISSP, out bool ISEP, out bool INSD, bTreatAsInfinite); }

        /// <summary>
        /// returns true if the passed vector lies on the line
        /// </summary>
        /// <param name="aVector" the vector to test></param>
        /// <param name="aPrecis" a precision apply></param>
        /// <param name="rIsStartPt" returns true if the passed vector is the start vector of the line></param>
        /// <param name="rIsEndPt" returns true if the passed vector is the end vector of the line></param>
        /// <param name="rWithin" returns true if the passed vector is within the line></param>
        /// <param name="bTreatAsInfinite" flag to treat the line as  infinite></param>
        public bool ContainsVector(UVECTOR aVector, int aPrecis, out bool rIsStartPt, out bool rIsEndPt, out bool rWithin, bool bTreatAsInfinite = false)
        {

            rIsStartPt = false;
            rIsEndPt = false;
            rWithin = false;
            double t1 = Math.Round(GetTValue(aVector, out rWithin), aPrecis);
            if (!rWithin) return false; // the point does not lie on the lines path or the line has ne length
            rIsStartPt = t1 == 0;
            rIsEndPt = t1 == 1;
            return bTreatAsInfinite || (t1 >= 0 && t1 <= 1);

        }
        /// <summary>
        ////returns true if this line intersects the passed line and the intersection point lies on both lines
        /// </summary>
        public bool Intersects(ULINE bLine, bool bMustBeOn1 = true, bool bMustBeOn2 = true)
        => Intersects(bLine, out UVECTOR _, bMustBeOn1, bMustBeOn2);

        /// <summary>
        ////returns true if this line intersects the passed line and the intersection point lies on both lines
        /// </summary>
        public bool Intersects(ULINE bLine, out UVECTOR rIntersectionPt, bool bMustBeOn1 = true, bool bMustBeOn2 = true)
        {
            rIntersectionPt = this.IntersectionPt(bLine, out bool _, out bool _, out bool on1, out bool on2, out bool exisits);
            if (!exisits) return false;
            if (bMustBeOn1 && bMustBeOn2) return on1 && on2;
            if (bMustBeOn1 && !on1) return false;
            if (bMustBeOn2 && !on2) return false;
            return true;
        }

        public UVECTOR IntersectionPt(ULINE bLine)
        { return IntersectionPt(bLine, out _, out _, out _, out _, out _, out _, out _); }


        public UVECTOR IntersectionPt(iLine bLine, out bool rLinesAreParallel, out bool rLinesAreCoincident, out bool rIsOnFirstLine, out bool rIsOnSecondLine, out bool rInterceptExists)
        { return IntersectionPt(new ULINE( bLine), out rLinesAreParallel, out rLinesAreCoincident, out rIsOnFirstLine, out rIsOnSecondLine, out rInterceptExists, out double T1, out double T2); }


        public UVECTOR IntersectionPt(ULINE bLine, out bool rLinesAreParallel, out bool rLinesAreCoincident, out bool rIsOnFirstLine, out bool rIsOnSecondLine, out bool rInterceptExists)
        { return IntersectionPt(bLine, out rLinesAreParallel, out rLinesAreCoincident, out rIsOnFirstLine, out rIsOnSecondLine, out rInterceptExists, out double T1, out double T2); }

        public UVECTOR IntersectionPt(ULINE bLine, out bool rLinesAreParallel, out bool rLinesAreCoincident, out bool rIsOnFirstLine, out bool rIsOnSecondLine, out bool rInterceptExists, out double rT1, out double rT2)
        {
            UVECTOR _rVal = UVECTOR.Zero;
            rInterceptExists = false;
            rLinesAreParallel = false;
            rLinesAreCoincident = false;
            rIsOnFirstLine = false;
            rIsOnSecondLine = false;
            rT1 = -9999;
            rT2 = -9999;


            // get this lines direction and make sure it has a length
            UVECTOR aDir = Direction(out bool bFlag);
            if (bFlag) return _rVal;

            // get the passed lines direction and make sure it has a length
            UVECTOR bDir = bLine.Direction(out bFlag);
            if (bFlag) return _rVal;

            //see if they are parallel
            bool bParel = UVECTORS.AreEqual(aDir, bDir, bCompareInverse: true, 6, out bool bInvsPar);
            rLinesAreParallel = bParel || bInvsPar;


            //get the shortest line between the two lines
            rLinesAreCoincident = !ULINES.ShortestConnector(this, bLine, out ULINE sL);

            if (!rLinesAreCoincident)
            {
                double f1 = Math.Round(sL.Length, 6);
                rInterceptExists = f1 == 0;
                _rVal = sL.sp;
                if (rInterceptExists)
                {
                    rT1 = GetTValue(_rVal, out rIsOnFirstLine);
                    rT2 = bLine.GetTValue(_rVal, out rIsOnSecondLine);

                    rIsOnFirstLine = rT1 >= 0 & rT1 <= 1;
                    rIsOnSecondLine = rT2 >= 0 & rT2 <= 1;
                }
            }
            return _rVal;
        }

        public UVECTORS Intersections(ULINEPAIR aLinePair, bool aLineIsInfinite = false, bool aLinesAreInfinite = false)
        {
            UVECTORS _rVal = UVECTORS.Zero;
            if (aLinePair.Line1.HasValue)
            {
                if (this.Intersects(aLinePair.Line1.Value, out UVECTOR u1, bMustBeOn1: aLineIsInfinite, bMustBeOn2: aLinesAreInfinite))
                {
                    _rVal.Add(u1);
                }
            }
            if (aLinePair.Line2.HasValue)
            {
                if (this.Intersects(aLinePair.Line2.Value, out UVECTOR u1, bMustBeOn1: aLineIsInfinite, bMustBeOn2: aLinesAreInfinite))
                {
                    _rVal.Add(u1);
                }
            }
            return _rVal;
        }
        public UVECTORS Intersections(IEnumerable<ULINEPAIR> aLinePairs, bool aLineIsInfinite = false, bool aLinesAreInfinite = false)
        {
            UVECTORS _rVal = UVECTORS.Zero;
            if (aLinePairs == null) return _rVal;
            foreach (var item in aLinePairs)
            {


                if (item.Line1.HasValue)
                {
                    if (this.Intersects(item.Line1.Value, out UVECTOR u1, bMustBeOn1: !aLineIsInfinite, bMustBeOn2: !aLinesAreInfinite))
                    {
                        _rVal.Add(u1);
                    }
                }
                if (item.Line2.HasValue)
                {
                    if (this.Intersects(item.Line2.Value, out UVECTOR u1, bMustBeOn1: !aLineIsInfinite, bMustBeOn2: !aLinesAreInfinite))
                    {
                        _rVal.Add(u1);
                    }
                }
            }

            return _rVal;
        }


        public UVECTORS Intersections(UARC aArc, bool aArcIsInfinite, bool aLineIsInfinite = false)
        {
            double sa = aArcIsInfinite ? 0 : aArc.StartAngle;
            double ea = aArcIsInfinite ? 360 : aArc.EndAngle;
            return  new UVECTORS( ULINE.ArcIntersection(this, aArc.Center,aArc.Radius,  bInfiniteLine: aLineIsInfinite, aArcIsInfinite: aArcIsInfinite,sa,ea));
        }

        public UVECTORS Intersections(USEGMENT aSegment, bool aSegIsInfinite, bool aLineIsInfinite = false)
        {
            UVECTORS _rVal = UVECTORS.Zero;
            if (aSegment.IsArc)
            {
                _rVal = Intersections(aSegment.ArcSeg, aSegIsInfinite, aLineIsInfinite);
            }
            else
            {
                UVECTOR u1 = IntersectionPt(aSegment.LineSeg, out bool _, out bool _, out bool ON1, out bool ON2, out bool EXST);
                if (EXST)
                {
                    if (aLineIsInfinite && aSegIsInfinite)
                    {
                        _rVal.Add(u1);
                    }
                    else if (aLineIsInfinite && !aSegIsInfinite)
                    {
                        if (ON2) _rVal.Add(u1);
                    }
                    else if (!aLineIsInfinite && aSegIsInfinite)
                    {
                        if (ON1) _rVal.Add(u1);
                    }
                    else
                    {
                        if (ON1 && ON2) _rVal.Add(u1);
                    }

                }


            }

            return _rVal;
        }

        public UVECTORS Intersections(ULINES bLines, bool bNoDupes = false, int aPrecis = 4, bool aLineIsInfinite = false, bool bLinesAreInfinite = false)
        {
            UVECTORS _rVal = UVECTORS.Zero;
       
           
            for (int i = 1; i <= bLines.Count; i++)
            {
                
                UVECTOR u1 = IntersectionPt(bLines.Item(i), out _, out _, out bool bOnA, out bool bOnB, out bool exists);
                if (!exists) continue;
               
                bool keep = (bOnA || (!bOnA && aLineIsInfinite)) && (bOnB || (!bOnB && bLinesAreInfinite));
                if (!keep) continue;
                if (bNoDupes )
                {
                    for (int j = 1; j <= _rVal.Count; j++)
                    {
                        if (UVECTORS.AreEqual(u1, _rVal.Item(j), aPrecis))
                        {
                            keep = false;
                            break;
                        }
                    }
                }
                if(keep)  _rVal.Add(u1);
            }
        
            return _rVal;
        }

        public UVECTORS Intersections(IEnumerable<iLine> bLines, bool bNoDupes = false, int aPrecis = 4, bool aLineIsInfinite = false, bool bLinesAreInfinite = false)
        {
            UVECTORS _rVal = UVECTORS.Zero;
            if (bLines == null) return _rVal;

            foreach (iLine  l1 in bLines )
            {

                UVECTOR u1 = IntersectionPt(l1, out _, out _, out bool bOnA, out bool bOnB, out bool exists);
                if (!exists) continue;

                bool keep = (bOnA || (!bOnA && aLineIsInfinite)) && (bOnB || (!bOnB && bLinesAreInfinite));
                if (!keep) continue;
                if (bNoDupes)
                {
                    for (int j = 1; j <= _rVal.Count; j++)
                    {
                        if (UVECTORS.AreEqual(u1, _rVal.Item(j), aPrecis))
                        {
                            keep = false;
                            break;
                        }
                    }
                }
                if (keep) _rVal.Add(u1);
            }

            return _rVal;
        }

        public dxeLine ToDXFLineEX(dxeLine aExistingLine = null, string aLayerName = "", dxxColors aColor = dxxColors.Undefined, string aLinetype = "")
        {
            dxeLine _rVal;
            if (aExistingLine != null)
            { _rVal = aExistingLine; }
            else
            { _rVal = new dxeLine(); }

            _rVal.SetCoordinates2D(sp.X, sp.Y, ep.X, ep.Y);
            _rVal.LCLSet(aLayerName, aColor, aLinetype);
            return _rVal;
        }

        public bool Paramatize(out UVECTOR rSP, out UVECTOR rDir)
        {
            rSP = new UVECTOR(sp);
            rDir = Direction();
            return !rDir.IsNull(8);
        }

        public double GetTValue(UVECTOR aLineVector) => GetTValue(aLineVector, out bool ONLN);

        public ULINE Moved(double aXAdder = 0, double aYAdder = 0, bool bDontMovePoints = false)
        {
            ULINE _rVal = new ULINE(this);
            _rVal.Move(aXAdder, aYAdder, bDontMovePoints);
            return _rVal;
        }

        public void Move(double aXAdder = 0, double aYAdder = 0, bool bDontMovePoints = false)
        {
            if (aXAdder == 0 && aYAdder == 0) return;
            sp.X += aXAdder;
            ep.X += aXAdder;
            sp.Y += aYAdder;
            ep.Y += aYAdder;
            if (!bDontMovePoints) Points.Move(aXAdder, aYAdder);
        }

        public ULINE MovedOrtho(double aDistance, bool bDontMovePoints = false) => MovedOrtho(aDistance, out bool _, bDontMovePoints: bDontMovePoints);

        public ULINE MovedOrtho(double aDistance, out bool rLineIsNull, bool bDontMovePoints = false)
        {
            ULINE _rVal = new ULINE(this);
            _rVal.MoveOrtho(aDistance, out rLineIsNull, bDontMovePoints: bDontMovePoints);
            return _rVal;
        }

        public void MoveOrtho(double aDistance, bool bDontMovePoints = false) => MoveOrtho(aDistance, out bool _, bDontMovePoints: bDontMovePoints);

        public void MoveOrtho(double aDistance, out bool rLineIsNull, bool bDontMovePoints = false)
        {
            rLineIsNull = false;

            UVECTOR aDir = Direction(out rLineIsNull);

            if (!rLineIsNull)
            {
                aDir.Rotate(90);
                sp += aDir * aDistance;
                ep += aDir * aDistance;
                if (!bDontMovePoints) Points.Project(aDir, aDistance, bSuppressNormalize: true, bInvertDirection: false);
            }

        }

        public ULINE Projected(UVECTOR aDirection, double aDistance, bool bSuppressNormalization = false)
        {
            ULINE _rVal = new ULINE(this);
            _rVal.Project(aDirection, aDistance, bSuppressNormalization);
            return _rVal;
        }

        public void Project(UVECTOR aDirection, double aDistance, bool bSuppressNormalization = false)
        {
            UVECTOR d1 = new UVECTOR(aDirection, !bSuppressNormalization);
            sp.Project(d1, aDistance, true);
            ep.Project(d1, aDistance, true);
            Points.Project(d1, aDistance, bSuppressNormalize: true, bInvertDirection: false);
        }

        public void Project(iVector aDirection, double aDistance, bool bSuppressNormalization = false)
        {
            if (aDirection == null) return;
            UVECTOR d1 = new UVECTOR(aDirection, !bSuppressNormalization);
            sp.Project(d1, aDistance, true);
            ep.Project(d1, aDistance, true);
            Points.Project(d1, aDistance, bSuppressNormalize: true, bInvertDirection: false);
        }

        /// <summary>
        /// swaps the start point and end point
        /// </summary>
        public void Invert() { UVECTOR u1 = new UVECTOR(sp); sp.SetOrdinates(ep.X, ep.Y); ep.SetOrdinates(u1.X, u1.Y); }
        /// <summary>
        ///  returns a clone witht he end points reverse
        /// </summary>
        public ULINE Inverse() => new ULINE(this) { sp = new UVECTOR(ep), ep = new UVECTOR(sp) };

        public double GetTValue(UVECTOR aLineVector, out bool rIsOnLine)
        {

            rIsOnLine = false;

            //get this lines direction and length; bail out if if there is no length
            UVECTOR aDir = Direction(out bool aFlg, out double d1);
            if (aFlg) return 0;

            //get the direction and distance from this lines start point to the passed point
            UVECTOR bDir = sp.DirectionTo(aLineVector, bReturnInverse: false, out aFlg, out double d2);

            //if the direction is null the point is this lines start point so return tvalue of 0 
            if (aFlg) return 0;

            //if the direction equal to this lines direction the point lies on the ine
            rIsOnLine = UVECTORS.AreEqual(aDir, bDir, bCompareInverse: true, aPrecis:6, out aFlg);


            // return the fraction of this lines length that it's start point must be projected
            // this lines direction to land on the the perpendicular intersect of the passed points
            // connecting line onto the line
            double _rVal =  aFlg ? -Math.Round(d2 / d1, 6) : Math.Round(d2 / d1, 6);

            

            return _rVal;
            // negative values indicate the point is off the line in the inverse direction of the line
            // positive values greater than 1 mean the point is off the line in the direction of the line
            // 0 means the point is coincident with the start pt of the line
            // 1 means the point is coincident with end pt of the line

        }

        /// <summary>
        /// swaps the vectors by the ordinates. i.e. if EP.Y > SP.Y switch them.
        /// </summary>
        public bool Rectify(bool bDoX = false, bool bInverse = false)
        {

            bool doIt;

            if (bDoX)
            {
                doIt = (!bInverse && ep.X > sp.X) || (bInverse && ep.X < sp.X);
            }
            else
            {
                doIt = (!bInverse && ep.Y > sp.Y) || (bInverse && ep.Y < sp.Y);


            }
            if (!doIt) return false;

            UVECTOR v1 = ep;
            ep = sp;
            sp = v1;
            return true;

        }

        /// <summary>
        ///#1the subject line
        ///#2the structure of the plane
        ///^returns the coordinates of the passed vector with respect to the center and origin of the passed plane
        /// </summary>
        /// <param name="aLine"></param>
        /// <param name="aPlane"></param>
        /// <param name="aScaler"></param>
        /// <returns></returns>
        public ULINE WithRespectToPlane(dxfPlane aPlane, double aScaler = 1)
        {
            ULINE _rVal = new ULINE(this)
            {
                sp = sp.WithRespectToPlane(aPlane, aScaler),
                ep = ep.WithRespectToPlane(aPlane, aScaler)
            };
            return _rVal;
        }

        public ULINES PointSegments(bool bIncludeStartPt = true, bool bIncludeEndPt = true, bool bSortStartToEnd = true)
        {
            ULINES _rVal = new ULINES("PointSegments");

            List<iVector> pts = new List<iVector>();
            if (bIncludeStartPt) pts.Add(new uopVector(sp));
            for (int i = 1; i <= Points.Count; i++) { pts.Add(new uopVector(Points.Item(i))); }
            if (bIncludeEndPt) pts.Add(new uopVector(ep));


            if (bSortStartToEnd)
                pts = dxfVectors.Sort(pts, dxxSortOrders.NearestToFarthest, new uopVector(sp));

            for (int i = 1; i <= pts.Count - 1; i++)
            {
                if (i + 1 > pts.Count) break;
                ULINE l1 = new ULINE(pts[i - 1], pts[i]);
                if (l1.Length != 0) _rVal.Add(l1);


            }



            return _rVal;
        }

        public void Resize(double aNewLength, uppSegmentPoints aBasePt = uppSegmentPoints.StartPt)
        {
            UVECTOR dir = Direction();

            switch (aBasePt)
            {
                case uppSegmentPoints.MidPt:
                    {
                        double d1 = 0.5 * aNewLength;
                        UVECTOR u1 = MidPt;
                        UVECTOR u2 = u1 + dir * d1;
                        ep.X = u2.X; ep.Y = u2.Y;
                        u2 = u1 - dir * d1;
                        sp.X = u2.X; sp.Y = u2.Y;

                        break;
                    }
                case uppSegmentPoints.EndPt:
                    {
                        UVECTOR u1 = ep + dir * aNewLength;
                        sp.X = u1.X; sp.Y = u1.Y;
                        break;
                    }
                default:
                    {
                        UVECTOR u1 = sp - dir * aNewLength;
                        ep.X = u1.X; ep.Y = u1.Y;
                        break;
                    }
            }
        }

        public ULINE Resized(double aNewLength, uppSegmentPoints aBasePt = uppSegmentPoints.StartPt)
        {
            ULINE _rVal = new ULINE(this);
            _rVal.Resize(aNewLength, aBasePt);
            return _rVal;
        }

        #endregion Methods

        #region Shared Methods

        public static ULINE Null => new ULINE("");


        public static UVECTORS ArcIntersection(ULINE aLine, UVECTOR aArcCenter, double aRadius, bool bInfiniteLine = false, bool aArcIsInfinite = true, double aStartAngle = 0, double aEndAngle = 360, int aPrecis = 5)
        {

            UVECTORS _rVal = UVECTORS.Zero;
            aPrecis = mzUtils.LimitedValue(aPrecis, 1, 15);
            double rad = Math.Abs(Math.Round(aRadius, aPrecis));
            if (rad <= 0) return _rVal;//null arc
   
            if (aArcIsInfinite)
            {
                aStartAngle = 0;
                aEndAngle = 360;
            }
            colDXFVectors pts = dxfIntersections.LineArc(new uopVector(aLine.sp), new uopVector(aLine.ep), new uopVector(aArcCenter), aRadius, bInfiniteLine, aArcIsInfinite, aStartAngle, aEndAngle, aPrecis);

            foreach (var item in pts)
            {
                UVECTOR ip = new UVECTOR(item.X, item.Y);
                UVECTOR dir = (ip - aArcCenter).Normalized();
                ip = aArcCenter +  dir * aRadius;
                _rVal.Add(ip ,aRadius: aRadius);

            }

            return _rVal;

        }

        internal static uopVectors LineArcIntersections(ULINE aLine, UARC aArc, bool bInfiniteLine = true, bool aArcIsInfinite = false, int aPrecis = 15)
        {
            uopVectors _rVal = uopVectors.Zero;
            if (aArc.Radius <= 0) return _rVal;
            UVECTOR dir = aLine.Direction(out bool nullLine);
            if ( nullLine) return _rVal;

            try
            {
                double dx = aLine.ep.X - aLine.sp.X;
                double dy = aLine.ep.Y - aLine.sp.Y;

                double a = dx * dx + dy * dy;
                double b = 2 * (dx * (aLine.sp.X - aArc.Center.X) + dy * (aLine.sp.Y - aArc.Center.Y));
                double c = (aLine.sp.X - aArc.Center.X) * (aLine.sp.X - aArc.Center.X) +
                           (aLine.sp.Y - aArc.Center.Y) * (aLine.sp.Y - aArc.Center.Y) -
                           aArc.Radius * aArc.Radius;

                double discriminant = b * b - 4 * a * c;

                if (discriminant < 0)
                {
                    // No intersection
                    return _rVal;
                }
                else if (discriminant == 0)
                {
                    // One intersection (tangent)
                    double t = -b / (2 * a);
                    _rVal.Add(new uopVector(aLine.sp.X + t * dx, aLine.sp.Y + t * dy));

                }
                else
                {
                    // Two intersections
                    double t1 = (-b + Math.Sqrt(discriminant)) / (2 * a);
                    double t2 = (-b - Math.Sqrt(discriminant)) / (2 * a);
                    _rVal.Add(new uopVector(aLine.sp.X + t1 * dx, aLine.sp.Y + t1 * dy));
                    _rVal.Add(new uopVector(aLine.sp.X + t2 * dx, aLine.sp.Y + t2 * dy));

                }
            }
            finally
            {
                if(_rVal.Count > 0 &&( !aArcIsInfinite || bInfiniteLine))
                {
                    for(int i = _rVal.Count; i >=1; i--)
                    {
                        UVECTOR u1 = new UVECTOR(_rVal.Item(i));
                        if ( !bInfiniteLine && !aLine.ContainsVector(u1, aPrecis: aPrecis, bTreatAsInfinite: false))
                        {
                            _rVal.RemoveAt(i - 1);
                            continue;
                        }
                        if (!aArcIsInfinite && !aArc.ContainsVector(u1, aPrecis: aPrecis, bTreatAsInfinite: false))
                        {
                            _rVal.RemoveAt(i - 1);
                            continue;
                        }
                    }
                }
            }
         
            return _rVal;
        }

        public static ULINE? CloneCopy(ULINE? aLine) { if (!aLine.HasValue) return null; return new ULINE(aLine.Value); }
        #endregion Shared Methods
    }

    internal struct ULINES : ICloneable
    {
        private bool _Init;
        private int _Count;
        private ULINE[] _Members;
        public string Name;

        public ULINES(string aName = "")
        {

            Name = aName;
            _Members = new ULINE[0];
            _Count = 0;
            _Init = true;
        }

        public ULINES(ULINES aLines, bool bClear = false)
        {

            Name = aLines.Name;
            _Members = new ULINE[0];
            _Count = bClear ? 0 : aLines.Count;
            _Init = true;
            if (_Count > 0)
            {
                _Members = Force.DeepCloner.DeepClonerExtensions.DeepClone<ULINE[]>(aLines._Members);
            }
        }

        public ULINES(IEnumerable<iLine>  aLines, bool bClear = false)
        {

            Name = string.Empty;
            _Members = new ULINE[0];
            _Count = bClear ? 0 :  aLines == null? 0 : aLines.Count();
            _Init = true;
            if (aLines == null) return;
            
                if(aLines is uopLines)
                {
                    uopLines ulines = (uopLines)aLines;
                    Name = ulines.Name;
                }

                if(_Count > 0)
            {
                foreach(var line in aLines)
                {
                    Add(new ULINE(line));
                }
            }
            
        }

        private int Init()
        {
            Name = string.Empty;
            _Members = new ULINE[0];
            _Count = 0;
            _Init = true;
            return 0;
        }

        public ULINES Clone(bool bReturnEmpty = false)
        {

            if (!_Init) { Init(); }
            ULINES _rVal = new ULINES(this);
            if (bReturnEmpty) _rVal.Clear();
            return _rVal;

        }

        object ICloneable.Clone() => (object)Clone();

        public void Clear() { _Count = 0; _Members = new ULINE[0]; _Init = true; }

        public UVECTORS Intersections(ULINES bLines)
        {
            UVECTORS _rVal = UVECTORS.Zero;
            bool aFlg = false;
            bool bFlg = false;
            bool bExst = false;
            UVECTOR u1;

            for (int i = 1; i <= Count; i++)
            {
                for (int j = 1; j <= bLines.Count; j++)
                {
                    u1 = Item(i).IntersectionPt(bLines.Item(j), out bool PRL, out bool CINC, out aFlg, out bFlg, out bExst);
                    if (bExst & aFlg && bFlg) _rVal.Add(u1);

                }
            }
            return _rVal;
        }

        public UVECTORS ArcIntersections(UVECTOR aArcCenter, double aRadius, bool bInfiniteLines = false)
        {
            UVECTORS _rVal = UVECTORS.Zero;
            UARC arc = new UARC(aArcCenter, aRadius);
            for (int i = 1; i <= Count; i++)
            { _rVal.Append(ULINE.ArcIntersection(Item(i), arc.Center,arc.Radius, bInfiniteLine: bInfiniteLines, aArcIsInfinite:true)); }
            return _rVal;
        }
        public void Move(double aXChange = 0, double aYChange = 0)
        {
            ULINE u1;

            for (int i = 1; i <= Count; i++)
            {
                u1 = Item(i);
                u1.Move(aXChange, aYChange);
                SetItem(i, u1);
            }

        }

        public ULINES GetByRow(int aRow)
        {
            ULINES _rVal = new ULINES(this, true);
            ULINE v1;

            for (int i = 1; i <= Count; i++)
            {
                v1 = Item(i);
                if (v1.Row == aRow) { _rVal.Add(v1); }
            }
            return _rVal;
        }

        public int Count => (!_Init) ? Init() : _Count;

        public ULINE Item(int aIndex) { return (aIndex < 1 || aIndex > Count) ? new ULINE() : _Members[aIndex - 1]; }

        public void SetItem(int aIndex, ULINE aLine)
        {
            if (aIndex < 1 || aIndex > Count) { return; }
            aLine.Index = aIndex;
            _Members[aIndex - 1] = aLine;
        }

        public void SetPoints(int aIndex, UVECTORS aPoints)
        {
            if (aIndex < 1 || aIndex > Count) { return; }
            _Members[aIndex - 1].Points = aPoints;
        }

        public void PrintToConsole()
        {
            for (int i = 1; i <= Count; i++)
            {
                System.Diagnostics.Debug.WriteLine($"{i}.) {Item(i)}");
            }
        }

        //public bool SetValue(int aIndex, dynamic aValue)
        //{
        //    if (aIndex <= 0 || aIndex > Count || aValue == null) { return false; }
        //    bool _rVal = _Members[aIndex - 1].Value != aValue;
        //    _Members[aIndex - 1].Value = aValue;
        //    return _rVal;
        //}

        public ULINE Add(double aSPX, double aSPY, double aEPX, double aEPY, int aRow = 0, int aCol = 0) => Add(new ULINE(aSPX, aSPY, aEPX, aEPY, aRow, aCol));

        public ULINE AddXY(double aSPX, double aSPY, double aEPX, double aEPY, int aRow = 0, int aCol = 0) => Add(new ULINE(aSPX, aSPY, aEPX, aEPY, aRow, aCol));


        public ULINE Add(UVECTOR aSP, UVECTOR aEP) => Add(new ULINE(aSP, aEP));

        public ULINE Add(ULINE aLine)
        {
            if (Count + 1 > Int32.MaxValue) { return aLine; }

            _Count += 1;
            Array.Resize<ULINE>(ref _Members, _Count);
            _Members[_Count - 1] = aLine;
            _Members[_Count - 1].Index = _Count;
            return Item(Count);

        }

        /// <summary>
        /// Remove a member
        /// </summary>
        /// <param name="aIndex"></param>
        /// <returns></returns>
        public ULINE Remove(int aIndex)
        {

            if (aIndex < 1 || aIndex > Count) { return new ULINE(UVECTOR.Zero, UVECTOR.Zero); }

            ULINE _rVal = ULINE.Null;
            if (Count == 1) { _rVal = Item(1); Clear(); return _rVal; }

            if (aIndex == Count)
            {
                _rVal = Item(Count);
                _Count -= 1;
                Array.Resize<ULINE>(ref _Members, _Count);
                return _rVal;
            }


            ULINE[] newMems = new ULINE[_Count - 2];
            int j = 0;


            for (int i = 1; i <= Count; i++)
            {
                if (i != aIndex)
                { j += 1; newMems[j - 1] = Item(i); newMems[j - 1].Index = j; }
                else
                { _rVal = Item(i); }

            }

            _Members = newMems;
            _Count -= 1;
            return _rVal;
        }

        public override string ToString() => string.IsNullOrWhiteSpace(Name) ? $"ULINES[{Count}]" : $"ULINES '{Name}' [{Count}]";

        public void Mirror(double? aX, double? aY)
        {
            if (Count <= 0 || (!aX.HasValue && !aY.HasValue)) return;
            for (int i = 1; i <= Count; i++)
            {
                ULINE mem = Item(i);
                mem.Mirror(aX, aY);
                SetItem(i, mem);
            }
        }

       

        #region Shared Methods

        public static ULINES Null => new ULINES("");

        public static colDXFEntities ToDXFLines(ULINES aLines, string aTag = "", string aFlag = "", string aLayerName = "", dxxColors aColor = dxxColors.Undefined, string aLinetype = "")
        {
            colDXFEntities _rVal = new colDXFEntities();
            ULINE aLn;

            for (int i = 1; i <= aLines.Count; i++)
            {
                aLn = aLines.Item(i);
                _rVal.AddLine(aLn.sp.X, aLn.sp.Y, aLn.ep.X, aLn.ep.Y, new dxfDisplaySettings(aLayerName, aColor, aLinetype), aTag, aFlag);
            }
            return _rVal;
        }

        internal static ULINES FromUVectors(UVECTORS aVectors, bool bClosed)
        {
            ULINES _rVal = new ULINES();
            UVECTOR u1;
            UVECTOR u2;

            if (aVectors.Count <= 1) return _rVal;

            for (int i = 1; i <= aVectors.Count; i++)
            {
                u1 = aVectors.Item(i);
                if (i < aVectors.Count)
                { u2 = aVectors.Item(i + 1); }
                else
                {
                    if (!bClosed) return _rVal;
                    u2 = aVectors.Item(1);
                }

                _rVal.Add(u1, u2);
            }

            return _rVal;
        }

        /// <summary>
        /// computes the shortest line between the two passed line. returns true if the line exists.
        /// </summary>
        /// <param name="aLine">the first line</param>
        /// <param name="bLine">the second line</param>
        /// <param name="rConnector">returns the shortest line between the two line</param>
        /// <param name="rAIsNull">returns true if the first line is zero length or undefined</param>
        /// <param name="rBIsNull">returns true if the first line is zero length or undefined</param>
        /// <returns></returns>
        public static bool ShortestConnector(ULINE aLine, ULINE bLine, out ULINE rConnector, bool rAIsNull = false, bool rBIsNull = false)
        {
            bool _rVal = false;
            rConnector = new ULINE(aLine);
            rAIsNull = !aLine.Paramatize(out UVECTOR P1, out UVECTOR P21);
            rBIsNull = !bLine.Paramatize(out UVECTOR p3, out UVECTOR P43);
            if (rAIsNull || rBIsNull) return _rVal;


            UVECTOR P2 = aLine.ep;
            UVECTOR p4 = bLine.ep;
            UVECTOR P13 = P1 - p3;
            double d1343 = P13.X * P43.X + P13.Y * P43.Y;
            double d4321 = P43.X * P21.X + P43.Y * P21.Y;
            double d1321 = P13.X * P21.X + P13.Y * P21.Y;
            double d4343 = P43.X * P43.X + P43.Y * P43.Y;
            double d2121 = P21.X * P21.X + P21.Y * P21.Y;
            double denom = d2121 * d4343 - d4321 * d4321;
            if (denom <= 0.0000001) return false;

            _rVal = true;

            double numer = d1343 * d4321 - d1321 * d4343;
            double mua = numer / denom;
            double mub = (d1343 + d4321 * mua) / d4343;
            rConnector.sp.X = mzUtils.VarToDouble(P1.X + mua * P21.X);
            rConnector.sp.Y = mzUtils.VarToDouble(P1.Y + mua * P21.Y);
            //    proj_V2L rConnector.sp, aLine

            rConnector.ep.X = mzUtils.VarToDouble(p3.X + mub * P43.X);
            rConnector.ep.Y = mzUtils.VarToDouble(p3.Y + mub * P43.Y);
            //    proj_V2L rConnector.ep, bLine
            return _rVal;
        }
        #endregion Shared Methods
    }

    internal struct UARC : ICloneable
    {

        #region Fields

        public UVECTOR Center;
        public double Radius;
        public double StartAngle;
        public double EndAngle;
        public int Index;
        public bool Suppressed;
        #endregion Fields

        #region Constructors


        public UARC(double aRadius = 0)
        {
            Center = UVECTOR.Zero;
            Radius = aRadius;
            StartAngle = 0;
            EndAngle = 360;
            Index = 0;
            Suppressed = false;
        }

        public UARC(UARC aArc)
        {
            Center = new UVECTOR(aArc.Center);
            Radius = aArc.Radius;
            StartAngle = aArc.StartAngle;
            EndAngle = aArc.EndAngle;
            Index = aArc.Index;
            Suppressed = aArc.Suppressed;
        }
        public UARC(uopArc aArc)
        {
            Center = UVECTOR.Zero;
            Radius = 0;
            StartAngle = 0;
            EndAngle = 360;
            Index = 0;
            Suppressed = false;
            if (aArc == null) return;
            Center = new UVECTOR(aArc.Center);
            Radius = aArc.Radius;
            StartAngle = aArc.StartAngle;
            EndAngle = aArc.EndAngle;
            Suppressed = aArc.Suppressed;
        }

        public UARC(UVECTOR aCenter, double aRadius = 0, double aStartAngle = 0, double aEndAngle = 360)
        {
            Center = aCenter;
            Radius = aRadius;
            StartAngle = aStartAngle;
            EndAngle = aEndAngle;
            Index = 0;
            Suppressed = false;
        }

        #endregion Constructors

        #region Properties

        public UVECTOR StartPt => new UVECTOR(Radius, 0).Rotated(StartAngle) + Center;

        public UVECTOR EndPt => new UVECTOR(Radius, 0).Rotated(EndAngle) + Center;

        public double SpannedAngle => uopUtils.SpannedAngle(false, StartAngle, EndAngle);

        #endregion Properties

        #region Methods

        object ICloneable.Clone() => (object)Clone();

        public UARC Clone() => new UARC(this);

        public override string ToString() => $"UARC - CP:{Center} RAD:{Radius} SA:{StartAngle} EA:{EndAngle}";

        /// <summary>
        /// returns true if the passed vector lies on the arc
        /// </summary>
        /// <param name="aVector" the vector to test></param>
        /// <param name="aPrecis" a precision apply></param>
        /// <param name="bTreatAsInfinite" flag to treat the arc as 360 degrees></param>
        public bool ContainsVector(UVECTOR aVector, int aPrecis = 5, bool bTreatAsInfinite = false)
       => UARC.ArcContainsVector(Center, Radius, StartAngle, EndAngle, aVector, out bool _, out bool _, out bool _, bTreatAsInfinite, aPrecis);

        /// <summary>
        /// returns true if the passed vector lies on the arc
        /// </summary>
        /// <param name="aVector" the vector to test></param>
        /// <param name="aPrecis" a precision apply></param>
        /// <param name="rIsStartPt" returns true if the passed vector is the start vector of the arc></param>
        /// <param name="rIsEndPt" returns true if the passed vector is the end vector of the arc></param>
        /// <param name="rWithin" returns true if the passed vector is within the arc></param>
        /// <param name="bTreatAsInfinite" flag to treat the arc as 360 degrees></param>
        public bool ContainsVector(UVECTOR aVector, out bool rIsStartPt, out bool rIsEndPt, out bool rWithin, bool bTreatAsInfinite = false, int aPrecis = 5)
       => UARC.ArcContainsVector(Center, Radius, StartAngle, EndAngle, aVector, out rIsStartPt, out rIsEndPt, out rWithin, bTreatAsInfinite, aPrecis);




        public dxeArc ToDXFArc() => new dxeArc(Center.ToDXFVector(), Radius, StartAngle, EndAngle);

        public dxeArc ToDXFArcEX(dxeArc aExistingArc = null, string aLayerName = "", dxxColors aColor = dxxColors.Undefined, string aLinetype = "")
        {
            dxeArc _rVal = aExistingArc != null ? aExistingArc : ToDXFArc();

            _rVal.Center = new dxfVector(new uopVector(Center));
            _rVal.Radius = Radius;
            _rVal.StartAngle = StartAngle;
            _rVal.EndAngle = EndAngle;

            _rVal.LCLSet(aLayerName, aColor, aLinetype);
            return _rVal;
        }


        public UVECTORS Intersections(UARC aArc, bool aArcIsInfinite, bool bArcIsInfinite = false) => UARC.ArcIntersections(this, aArc, aArcIsInfinite, bArcIsInfinite);

        public UVECTORS Intersections(ULINE aLine, bool aArcIsInfinite, bool aLineIsInfinite = false) => aLine.Intersections(this, aArcIsInfinite, aLineIsInfinite);


        public UVECTORS PhantomPts(int aCurveDivisions = 20, bool bIncludeEndPt = true) => UARC.ArcPhantomPts(this, aCurveDivisions, bIncludeEndPt);

        #endregion Methods

        #region Shared Methods
              internal static UVECTORS ArcPhantomPts(UARC aArc, int aCurveDivisions = 20, bool bIncludeEndPt = true)
        {
            UVECTORS _rVal = UVECTORS.Zero;
            aCurveDivisions = mzUtils.LimitedValue(aCurveDivisions, 2, 1000);

            UVECTOR spz = aArc.Center;

            UVECTOR aZDir = new UVECTOR(0, 0, 1);
            UVECTOR xdir = new UVECTOR(1, 0, 0);
            UVECTOR sp = spz + xdir * aArc.Radius;
            UVECTOR ep = spz + xdir * aArc.Radius;
            double sa = mzUtils.NormAng(aArc.StartAngle, ThreeSixtyEqZero: false, bReturnPosive: true);
            double ea = mzUtils.NormAng(aArc.EndAngle, ThreeSixtyEqZero: true, bReturnPosive: true);
            if (sa != 0) sp.Rotate(spz, sa);
            if (ea != 0) ep.Rotate(spz, ea);
            double span = dxfMath.SpannedAngle(false, sa, ea);

            int segs = aCurveDivisions;
            double angchange = span / segs;
            _rVal.Add(sp);

            UVECTOR v1 = sp;

            double remain = span;
            while (remain > angchange)
            {
                v1.Rotate(spz, angchange);
                _rVal.Add(v1);
                remain -= angchange;
            }


            if (bIncludeEndPt) _rVal.Add(ep);
            return _rVal;
        }
        public static UARC DefineWithVectors(UVECTOR aCenter, UVECTOR aSP, UVECTOR aEP)
        {
            UARC _rVal = new UARC(aCenter,0);
            UVECTOR d1 = aCenter.DirectionTo(aSP, false,  out bool flag1, out double rad1);
            UVECTOR d2 = aCenter.DirectionTo(aEP, false, out bool flag2, out double rad2);

            double rad = rad1;
            if(flag1 && flag2)
            {
                rad = 1;
                d1 = new UVECTOR(1, 0);
                d2 = new UVECTOR(1, 0);

            }
            else
            {
                if (flag1)
                {
                    rad = rad2;
                    d2 = d1;                
                }
                else if (flag2)
                {
                    rad = rad1;
                    d1 = d2;
                }

            }

            UVECTOR sp = aCenter + (d1 * rad);
            UVECTOR ep = aCenter + (d2 * rad);
            double sa = d1.XAngle;
            double ea = d2.XAngle;

            if(ea < sa)
            {
                ea = dxfUtils.NormalizeAngle(ea + 360, false, false, true);
            }

            _rVal.StartAngle = sa;
            _rVal.EndAngle = ea;
            _rVal.Radius = rad;
            return _rVal;
        }

        /// <summary>
        /// returns true if the passed vector lies on the arc
        /// </summary>
        /// <param name="aCenter" the circle center to test></param>
        /// <param name="aRadius" the circle radius></param>
        /// <param name="aStartAngle" the arc start angle></param>
        /// <param name="aEndAngle" the arc end angle></param>
        /// <param name="aVector" the vector to test></param>
        /// <param name="rIsStartPt" returns true if the passed vector is the start vector of the arc></param>
        /// <param name="rIsEndPt" returns true if the passed vector is the end vector of the arc></param>
        /// <param name="rWithin" returns true if the passed vector is within the arc></param>
        /// <param name="bTreatAsInfinite" flag to treat the arc as 360 degrees></param>
        /// <param name="aPrecis" a precision apply></param>
        public static bool ArcContainsVector(UVECTOR aCenter, double aRadius, double aStartAngle, double aEndAngle, UVECTOR aVector, out bool rWithin, out bool rIsStartPt, out bool rIsEndPt, bool bTreatAsInfinite = false, int aPrecis = 5)
        {
            rIsStartPt = false;
            rIsEndPt = false;
            rWithin = false;

            aPrecis = mzUtils.LimitedValue(aPrecis, 0, 15);
            double rad = Math.Abs(aRadius);
            double span = uopUtils.SpannedAngle(false, aStartAngle, aEndAngle);
            if (rad == 0 || span <= 0) return aCenter.IsEqual(aVector, aPrecis);

            //see if the point lies on the path of the arc with the fudge factor. If not,bail out
            rWithin = mzUtils.CompareVal(rad, aVector.DistanceTo(aCenter), aPrecis) == mzEqualities.Equals;
            if (span >= 359.99) bTreatAsInfinite = true;
            if (!rWithin) return false;


            UVECTOR StartPt = new UVECTOR(rad, 0).Rotated(aStartAngle) + aCenter;
            UVECTOR EndPt = new UVECTOR(rad, 0).Rotated(aEndAngle) + aCenter;

            rIsStartPt = mzUtils.CompareVal(0, aVector.DistanceTo(StartPt), aPrecis) == mzEqualities.Equals;
            if (!rIsStartPt) return true;

            rIsEndPt = mzUtils.CompareVal(0, aVector.DistanceTo(EndPt), aPrecis) == mzEqualities.Equals;
            if (!rIsEndPt) return true;

            //the point lies on the path of the arce and the arc 360 degrees so return true
            if (bTreatAsInfinite) return true;

            //get the angle from the positive X axis and compare to the arcs start and end angles
            double ang = (aVector - aCenter).XAngle;
            mzEqualities eqlt1 = mzUtils.CompareVal(ang, aStartAngle, 2);
            switch (eqlt1)
            {
                case mzEqualities.Equals:
                    rIsStartPt = true;
                    return true;
                case mzEqualities.LessThan:
                    return false;
                case mzEqualities.GreaterThan:
                    mzEqualities eqlt2 = mzUtils.CompareVal(ang, aEndAngle, 2);
                    if (eqlt2 == mzEqualities.GreaterThan) return false;
                    if (eqlt2 == mzEqualities.Equals) rIsEndPt = true;
                    return true;
            }
            return true;
        }


        internal static UVECTORS ArcIntersections(UARC aArc, UARC bArc, bool aArcIsInfinite = true, bool bArcIsInfinite = true)
        {
            // ^Finds the intersections of the two passed arcs
            // ~assume  Circular arcs
            UVECTORS _rVal = UVECTORS.Zero;
            double r0 = Math.Abs(aArc.Radius);
            double r1 = Math.Abs(bArc.Radius);
            if (r0 == 0 || r1 == 0) return _rVal;
            if (aArc.SpannedAngle >= 359.99d)
                aArcIsInfinite = true;
            if (bArc.SpannedAngle >= 359.99d)
                bArcIsInfinite = true;

            bool aFlg = false;

            double dst = 0;


            UVECTOR aDir = aArc.Center.DirectionTo(bArc.Center, false, out aFlg, out dst);
            if (aFlg)
                return _rVal; // centers are coincident
            if (Math.Round(dst, 15) > Math.Round(r0 + r1, 15))
                return _rVal; // too far apart
            UVECTOR v1;
            UVECTORS ips = UVECTORS.Zero;
            if (Math.Round(dst, 15) == Math.Round(r0 + r1, 15) | Math.Round(dst + r1, 15) == Math.Round(r0, 15))
            {
                // arcs are tangent one intersection
                v1 = aArc.Center + aDir * r0;
                ips.Add(v1.Clone());
            }
            else
            {
                var C = new UVECTOR(aArc.Center);

                UVECTOR xDir = new UVECTOR(1, 0);
                UVECTOR yDir = new UVECTOR(0, 1);
                UVECTOR c1 = new UVECTOR(dst, 0d, 0d);
                UVECTOR u1 = new UVECTOR(dst, 0d, 0d); // dir_Subtract(C1, C0)  'U
                UVECTOR u3 = new UVECTOR(u1.Y, -u1.X, 0d); // V
                double d1 = u1.Length();

                // C0 = dir_Input(0, 0, 0, , True)
                if (d1 != 0d)
                {
                    double d2 = 0.5d * ((Math.Pow(r0, 2d) - Math.Pow(r1, 2d)) / Math.Pow(d1, 2d) + 1d); // s
                    double d3 = Math.Pow(r0, 2d) / Math.Pow(d1, 2d);
                    if (d3 >= Math.Pow(d2, 2d))
                    {
                        d3 = Math.Sqrt(d3 - Math.Pow(d2, 2d)); // t
                        UVECTOR P1 = u1 * d2;
                        P1 += u3 * d3;
                        UVECTOR P2 = C + xDir * P1.X;
                        UVECTOR p3 = P2 + yDir * -P1.Y;
                        P2 += yDir * P1.Y;
                        ips.Add(P2.X, P2.Y);
                        ips.Add(p3.X, p3.Y);
                    }
                }
            }
            if (aArcIsInfinite & bArcIsInfinite)
            {
                _rVal = ips;

            }
            else
            {
                for (int i = 1; i <= ips.Count; i++)
                {
                    v1 = ips.Item(i);
                    aFlg = true;
                    if (!aArcIsInfinite)
                    {
                        if (!aArc.ContainsVector(v1, aPrecis: 5, bTreatAsInfinite: false))
                            aFlg = false;
                    }
                    if (aFlg & !bArcIsInfinite)
                    {
                        if (!bArc.ContainsVector(v1, aPrecis: 5, bTreatAsInfinite: false))
                            aFlg = false;
                    }

                    if (aFlg)
                        _rVal.Add(v1);


                }
            }
            return _rVal;
        }

        #endregion  Shared Methods
    }

    internal struct TPROPERTY : ICloneable
    {


        public string Name;

        public string Heading;
        public dynamic MaxValue;
        public dynamic MinValue;
        public string Category;
        public dynamic Increment;
        public bool ValueChanged;
        public bool Locked;
        public bool Protected;
        //public string Caption;

        public bool TypeLocked;
        public int Row;
        public int Col;
        public string DecodeString;
        public string Choices;
        public bool SelectOnly;
        public bool Hidden;
        public bool Optional;
        public TUNIT Units;
        public int Index;
        public string ProjectHandle;
        public int Precision;
        public uppDocumentTypes DocumentType;
        public bool IsShared;
        public string RangeGUID;
            public uppPartTypes PartType;
        private string _PartName;
        private int _PartIndex;
        private string _PartPath;


        private string _VariableTypeName;
        private string _FormatString;
        private string _DisplayName;
        private string _UnitCaption;
        private dynamic _Value;
        private dynamic _LastValue;
        private dynamic _DefaultValue;
        private dynamic _NullValue;
        private string _Color;
        private string _CellAddress;

        public TPROPERTY(string aName, dynamic aValue, uppUnitTypes aUnitType = uppUnitTypes.Undefined, uppPartTypes aPartType = uppPartTypes.Undefined, dynamic aLastValue = null, dynamic aDefaultValue = null, uopPart aPart = null, string aColor = null)
        {
            Units = new TUNIT(aUnitType);

            DocumentType = uppDocumentTypes.Undefined;
            Name = string.Empty;
            if (!Units.IsDefined) { _Value = null; } else { _Value = 0; }
            _DefaultValue = aDefaultValue;
            _LastValue = null;
            Increment = null;
            _NullValue = null;
            MaxValue = null;
            MinValue = null;

            Heading = string.Empty;
            ProjectHandle = string.Empty;
            Choices = string.Empty;
            _DisplayName = string.Empty;
            _FormatString = string.Empty;
            //Caption = string.Empty;
            _UnitCaption = string.Empty;
            _VariableTypeName = string.Empty;
            DecodeString = string.Empty;
            RangeGUID = string.Empty;

            SelectOnly = false;
            Locked = false;
            Protected = false;
            Hidden = false;
            TypeLocked = false;
            ValueChanged = false;
            IsShared = false;
            Row = 0;
            Col = 0;
            Precision = 0;
            Index = 0;
            DocumentType = uppDocumentTypes.Undefined;
            PartType = aPartType;
            _PartIndex = 0;
            _PartName = string.Empty;
            _PartPath = string.Empty;
            Category = string.Empty;
            _CellAddress = null;
            _Numeric = null;
            Name = aName.Trim();
            ProjectHandle = string.Empty;
            Optional = false;
            _Color = string.IsNullOrWhiteSpace(aColor) ? "Black" : aColor.Trim();
            SetValue(aValue);
            if (aLastValue == null) { LastValue = Value; } else { LastValue = aLastValue; }

            if (aPart != null) SubPart(aPart);
        }


        public TPROPERTY(string aName = "", uppUnitTypes aUnits = uppUnitTypes.Undefined)
        {
            Units = new TUNIT(aUnits);
            DocumentType = uppDocumentTypes.Undefined;
            Name = string.Empty;
            if (Units.IsDefined) _Value = 0; else _Value = null;
            _Color = "Black";

            _DefaultValue = null;
            _LastValue = null;
            _NullValue = null;
            Increment = null;
            MaxValue = null;
            MinValue = null;
            Heading = string.Empty;
            ProjectHandle = string.Empty;
            Choices = string.Empty;
            _DisplayName = string.Empty;
            _FormatString = string.Empty;
            //Caption = string.Empty;
            _UnitCaption = string.Empty;
            _VariableTypeName = string.Empty;
            DecodeString = string.Empty;
            RangeGUID = string.Empty;

            SelectOnly = false;
            Locked = false;
            Protected = false;
            Hidden = false;
            TypeLocked = false;
            ValueChanged = false;
            IsShared = false;
            Row = 0;
            Col = 0;
            Precision = 0;
            Index = 0;
            DocumentType = uppDocumentTypes.Undefined;
            PartType = uppPartTypes.Undefined;
            _PartIndex = 0;
            _PartName = string.Empty;
            _PartPath = string.Empty;
            _CellAddress = null;
            Category = string.Empty;

            Name = aName.Trim();
            ProjectHandle = string.Empty;
            _Numeric = null;
            Optional = false;
        }

        public TPROPERTY(uopProperty aProperty)
        {
            Units = new TUNIT();
            DocumentType = uppDocumentTypes.Undefined;
            Name = string.Empty;
            if (Units.IsDefined) _Value = 0; else _Value = null;
            _Color = "Black";

            _DefaultValue = null;
            _LastValue = null;
            _NullValue = null;
            Increment = null;
            MaxValue = null;
            MinValue = null;
            Heading = string.Empty;
            ProjectHandle = string.Empty;
            Choices = string.Empty;
            _DisplayName = string.Empty;
            _FormatString = string.Empty;
            //Caption = string.Empty;
            _UnitCaption = string.Empty;
            _VariableTypeName = string.Empty;
            DecodeString = string.Empty;
            RangeGUID = string.Empty;

            SelectOnly = false;
            Locked = false;
            Protected = false;
            Hidden = false;
            TypeLocked = false;
            ValueChanged = false;
            IsShared = false;
            Row = 0;
            Col = 0;
            Precision = 0;
            Index = 0;

            PartType = uppPartTypes.Undefined;
            _PartIndex = 0;
            _PartName = string.Empty;
            _PartPath = string.Empty;
            _CellAddress = null;
            Category = string.Empty;
            Name = string.Empty;
            ProjectHandle = string.Empty;
            _Numeric = null;
            Optional = false;
            if (aProperty == null) return;
            TPROPERTY pstruc = aProperty.Structure;

            Units = pstruc.Units;
            DocumentType = pstruc.DocumentType;
            Name = pstruc.Name;
            if (Units.IsDefined) _Value = 0; else _Value = null;
            _Color = pstruc._Color;
            _Value = pstruc._Value;
            _DefaultValue = pstruc._DefaultValue;
            _LastValue = pstruc._LastValue;
            _NullValue = pstruc._NullValue;
            Increment = pstruc.Increment;
            MaxValue = pstruc.MaxValue;
            MinValue = pstruc.MinValue;
            Heading = pstruc.Heading;
            ProjectHandle = pstruc.ProjectHandle;
            Choices = pstruc.Choices;
            _DisplayName = pstruc._DisplayName;
            _FormatString = pstruc._FormatString;
            //Caption = string.Empty;
            _UnitCaption = pstruc._UnitCaption;
            _VariableTypeName = pstruc._VariableTypeName;
            DecodeString = pstruc.DecodeString;
            RangeGUID = pstruc.RangeGUID;

            SelectOnly = pstruc.SelectOnly;
            Locked = pstruc.Locked;
            Protected = pstruc.Protected;
            Hidden = pstruc.Hidden;
            TypeLocked = pstruc.TypeLocked;
            ValueChanged = pstruc.ValueChanged;
            IsShared = pstruc.IsShared;
            Row = pstruc.Row;
            Col = pstruc.Col;
            Precision = pstruc.Precision;
            Index = pstruc.Index;

            PartType = pstruc.PartType;
            _PartIndex = pstruc._PartIndex;
            _PartName = pstruc._PartName;
            _PartPath = pstruc._PartPath;
            _CellAddress = pstruc._CellAddress;
            Category = pstruc.Category;

            Name = pstruc.Name;
            ProjectHandle = pstruc.ProjectHandle;
            _Numeric = pstruc._Numeric;

        }

        public TPROPERTY(TPROPERTY aProperty)
        {


            Units = aProperty.Units;
            DocumentType = aProperty.DocumentType;
            Name = aProperty.Name;
            if (Units.IsDefined) _Value = 0; else _Value = null;
            _Color = aProperty._Color;
            _Value = aProperty._Value;
            _DefaultValue = aProperty._DefaultValue;
            _LastValue = aProperty._LastValue;
            _NullValue = aProperty._NullValue;
            Increment = aProperty.Increment;
            MaxValue = aProperty.MaxValue;
            MinValue = aProperty.MinValue;
            Heading = aProperty.Heading;
            ProjectHandle = aProperty.ProjectHandle;
            Choices = aProperty.Choices;
            _DisplayName = aProperty._DisplayName;
            _FormatString = aProperty._FormatString;
            //Caption = string.Empty;
            _UnitCaption = aProperty._UnitCaption;
            _VariableTypeName = aProperty._VariableTypeName;
            DecodeString = aProperty.DecodeString;
            RangeGUID = aProperty.RangeGUID;

            SelectOnly = aProperty.SelectOnly;
            Locked = aProperty.Locked;
            Protected = aProperty.Protected;
            Hidden = aProperty.Hidden;
            TypeLocked = aProperty.TypeLocked;
            ValueChanged = aProperty.ValueChanged;
            IsShared = aProperty.IsShared;
            Row = aProperty.Row;
            Col = aProperty.Col;
            Precision = aProperty.Precision;
            Index = aProperty.Index;

            PartType = aProperty.PartType;
            _PartIndex = aProperty._PartIndex;
            _PartName = aProperty._PartName;
            _PartPath = aProperty._PartPath;
            _CellAddress = aProperty._CellAddress;
            Category = aProperty.Category;

            Name = aProperty.Name;
            ProjectHandle = aProperty.ProjectHandle;
            _Numeric = aProperty._Numeric;
            Optional = false;
        }

        // <summary>
        /// set the current property value back to its default value
        /// </summary>
        public bool ResetValue() { if (!HasDefaultValue) return false; Value = DefaultValue; return ValueChanged; }


        object ICloneable.Clone() => (object)Clone();

        public TPROPERTY Clone(bool bCloneIndex = true) => new TPROPERTY(this) { Index = bCloneIndex ? Index : 0 };

        public void Copy(TPROPERTY aProperty, bool bCloneIndex = true, bool bCloneRowCol = true)
        {
            int idx = bCloneIndex ? aProperty.Index : Index;


            Units = aProperty.Units;
            Name = aProperty.Name;
            _Value = aProperty._Value;
            _DefaultValue = aProperty._DefaultValue;
            _LastValue = aProperty._LastValue;
            Increment = aProperty.Increment;
            _NullValue = aProperty._NullValue;
            MaxValue = aProperty.MaxValue;
            MinValue = aProperty.MinValue;
            Heading = aProperty.Heading;
            ProjectHandle = aProperty.ProjectHandle;
            Choices = aProperty.Choices;
            _DisplayName = aProperty._DisplayName;
            _FormatString = aProperty._FormatString;
            _UnitCaption = aProperty._UnitCaption;
            _VariableTypeName = aProperty._VariableTypeName;
            DecodeString = aProperty.DecodeString;
            RangeGUID = aProperty.RangeGUID;
            _Color = aProperty._Color;
            _CellAddress = aProperty._CellAddress;
            SelectOnly = aProperty.SelectOnly;
            Locked = aProperty.Locked;
            Protected = aProperty.Protected;
            Hidden = aProperty.Hidden;
            TypeLocked = aProperty.TypeLocked;
            ValueChanged = aProperty.ValueChanged;
            IsShared = aProperty.IsShared;
            if (bCloneRowCol)
            {
                Row = aProperty.Row;
                Col = aProperty.Col;

            }
            Precision = aProperty.Precision;
            Index = idx;
            DocumentType = aProperty.DocumentType;
            PartType = aProperty.PartType;
            _PartIndex = aProperty._PartIndex;
            _PartName = aProperty._PartName;
            _PartPath = aProperty._PartPath;
            Category = aProperty.Category;


        }

        public string Color { get => _Color; set => _Color = (!string.IsNullOrWhiteSpace(value)) ? value.Trim() : "Black"; }


        public string CellAddress
        {
            get => _CellAddress ?? $"{mzUtils.ConvertIntegerToLetter(Col)}{Row}";

            set => _CellAddress = value;
        }

        public override string ToString() => $"{Name}={Convert.ToString(Value)}";


        /// <summary>
        /// the index of the part whose source is this property
        /// </summary>
        public int PartIndex { get => _PartIndex; set => _PartIndex = value; }

        /// <summary>
        /// the name of the part whose source is this property
        /// </summary>
        public String PartName { get => _PartName; set => _PartName = value; }

        /// <summary>
        /// the index of the parent part whose gave rsie to this property
        /// </summary>

        public string VariableTypeName { get => string.IsNullOrWhiteSpace(_VariableTypeName) ? TVALUES.GetDynamicTypeName(Value) : _VariableTypeName; set { value ??= string.Empty; _VariableTypeName = value; } }

        public string FormatString
        {
            get { return (Units.UnitType <= 0) ? _FormatString : Units.FormatString(); }
            set => _FormatString = value;
        }

        /// <summary>
        /// a string that shows the property value with the current format string applied
        /// </summary>
        /// <param name="aProp"></param>
        /// <returns></returns>
        public string FormatedString
        {
            get
            {
                if (!HasValue) { return ""; }
                string _rVal = string.Empty;
                dynamic aVar = Value;
                string fmat = FormatString;
                try
                {
                    if (mzUtils.IsNumeric(aVar) && fmat !=  string.Empty)
                    { _rVal = aVar.ToString(fmat); }
                    else
                    { _rVal = Convert.ToString(aVar); }
                }
                catch (Exception exception)
                {
                    LoggerManager log = new LoggerManager();
                    log.LogError(exception.Message);
                }
                return _rVal;
            }
        }

        public dynamic DecodedValue => TPROPERTY.DecodeValue(this, null);

        public bool IsNamed(string aNameList)
        {
            if (string.IsNullOrWhiteSpace(aNameList)) return false;
            bool _rVal = aNameList.Contains(",") ?
            mzUtils.ListContains(Name, aNameList) : string.Compare(Name, aNameList, ignoreCase: true) == 0;
            if (!_rVal)
            {
                _rVal = aNameList.Contains(",") ?
                mzUtils.ListContains(DisplayName, aNameList) : string.Compare(Name, aNameList, ignoreCase: true) == 0;

            }


            return _rVal;
        }

        public bool IsString
        {
            get
            {
                if (HasUnits) return false;
                if (!HasValue) return false;
                return Value.GetType() == typeof(string);
            }
        }

        public string DisplayName
        {
            get => string.IsNullOrWhiteSpace(_DisplayName) ? mzUtils.UnPascalCase(Name, _DisplayName) : _DisplayName;

            set => _DisplayName = (!string.IsNullOrWhiteSpace(value)) ? value.Trim() : "";

        }

        public double ValueD => mzUtils.VarToDouble(Value);


        public int ValueI => mzUtils.VarToInteger(Value);

        public bool ValueB => mzUtils.VarToBoolean(Value);

        public string ValueS => HasValue ? Convert.ToString(Value) : "";

        public bool HasValue => _Value != null;

        public bool HasNullValue => _NullValue != null;

        public bool HasDefaultValue => _DefaultValue != null;

        public double ConvertUnits(uppUnitFamilies aFromUnits, uppUnitFamilies aToUnits, dynamic aValue = null)
        {
            if (aValue != null) { SetValue(aValue, aDefaultValue: DefaultValue, aNullValue: NullValue); }
            if (Units.IsDefined) { Value = Units.ConvertValue(aValue, aFromUnits, aToUnits); Units.UnitSystem = aToUnits; }
            return ValueD;
        }





        public void SubPart(uopPart aPart)
        {
            if (aPart == null) return;
            ProjectHandle = aPart.ProjectHandle;
            RangeGUID = aPart.RangeGUID;
            PartType = aPart.PartType;
            _PartPath = aPart.GetPartPath();
            _PartIndex = aPart.GetPartIndex();

        }

        public void SubPart(TPROPERTIES aProps)
        {
            ProjectHandle = aProps.ProjectHandle;
            RangeGUID = aProps._RangeGUID;
            if (aProps.PartType != uppPartTypes.Undefined)
            {
                PartType = aProps.PartType;
                _PartPath = aProps._PartPath;
                _PartIndex = aProps._PartIndex;
                _PartName = aProps._PartName;
            }
        }

        public string PartPath { get { string _rVal = Name; return (!string.IsNullOrWhiteSpace(_PartPath)) ? _PartPath + "." + _rVal : _rVal; } set { _PartPath = value; } }


        public string UnitCaption(uppUnitFamilies aUnits = uppUnitFamilies.Default)
        {
            //a unit caption string assigned to the property
            if (Units.UnitType <= 0)
            { return _UnitCaption; }
            else
            {
                if (aUnits <= uppUnitFamilies.Default) { aUnits = Units.UnitSystem; }
                return Units.Label(aUnits);
            }
        }

        public uppUnitTypes UnitType
        {
            get => Units.UnitType;
            set
            {
                Units = new TUNIT(value);

                if (!Units.IsDefined) return;
                SetValue(mzUtils.VarToDouble(Value));
                LastValue = 0;

            }

        }

        public void SetUnitCaption(string aDisplayName)
        {
            //a unit caption string assigned to the property
            _UnitCaption = aDisplayName;
        }

        public bool HasUnits => Units.IsDefined;

        public dynamic Value { get => _Value; set { if (HasValue && value != null) LastValue = _Value; SetValue(value); } }


        public dynamic DefaultValue { get => _DefaultValue; set => _DefaultValue = value; }

        public dynamic NullValue { get => _NullValue; set => _NullValue = value; }

        public dynamic LastValue { get => _LastValue; set => _LastValue = value; }

        /// <summary>
        /// returns True if the current value is equal to the currently set default value
        /// </summary>
        /// <param name="aProp"></param>
        /// <returns></returns>
        public bool IsDefault
        {
            get
            {
                bool _rVal = HasDefaultValue;
                if (!_rVal) return false;
                if (string.Compare(Value.GetType().Name, "String", true) == 0)
                {
                    if (DefaultValue is Enum)
                    { _rVal = Value == ((int)DefaultValue).ToString(); }
                    else
                    { _rVal = string.Compare(Value, DefaultValue.ToString(), true) == 0; }
                }
                else
                {
                    if (DefaultValue is Enum && Value is Enum)
                    { _rVal = Value == DefaultValue; }
                    else if (DefaultValue is Enum)
                    { _rVal = Value == (int)DefaultValue; }
                    else if (DefaultValue is Boolean && Value is Int32)
                    { _rVal = Value == (DefaultValue ? 1 : 0); }
                    else
                    {
                        _rVal = Value == DefaultValue;
                    }
                }
                return _rVal;
            }
        }

        public bool IsNullValue
        {
            get
            {
                if (HasNullValue)
                {
                    { return ValueS == NullValue.ToString(); }
                }

                return false;

            }
        }

        private bool? _Numeric;

        public bool Numeric
        {
            get
            {
                if (HasUnits) return true;
                if (!HasValue) return false;
                return _Numeric ?? mzUtils.IsNumeric(ValueS);
            }
            set => _Numeric = value;

        }


        public bool IsEnum => _Value == null ? false : _Value.GetType().IsEnum;

        public string CaptionUnitSignature(uppUnitFamilies aUnitFamily = uppUnitFamilies.Default, bool bBoldLeftSide = false)
        {
            string _rVal = string.Empty;
            dynamic aVar = null;
            string aLable = string.Empty;
            string aFmat = string.Empty;
            string strVal = string.Empty;
            string LeftSide = string.Empty;
            try
            {
                if (aUnitFamily == uppUnitFamilies.Default) { aUnitFamily = uppUnitFamilies.English; }
                LeftSide = DisplayName;
                if (LeftSide ==  string.Empty) { LeftSide = Name; }


                if ((int)Units.UnitType == 1)
                {
                    aVar = Value;
                    aFmat = FormatString;
                }
                else
                {
                    aVar = Units.ConvertValue(Value, Units.UnitSystem, aUnitFamily);
                    aLable = Units.Label(aUnitFamily, addleadSpace: true);
                    aFmat = Units.FormatString(aUnitFamily);
                }
                if (aFmat !=  string.Empty)
                { strVal = aVar.ToString(aFmat); }
                else
                { strVal = Convert.ToString(aVar); }
                if (!bBoldLeftSide)
                { _rVal = LeftSide + ": " + strVal + aLable; }
                else
                { _rVal = "<B>" + LeftSide + ":</B> " + strVal + aLable; }
            }
            catch (Exception exception)
            {
                LoggerManager log = new LoggerManager();
                log.LogError(exception.Message);
            }
            return _rVal;
        }

        /// <summary>
        /// a string that shows unit lable of the property in the requested unit family
        /// </summary>
        /// <param name="aProp"></param>
        /// <param name="aUnitFamily"></param>
        /// <returns></returns>
        public string UnitString(uppUnitFamilies aUnitFamily = uppUnitFamilies.Default)
        {
            string _rVal = string.Empty;
            try
            {
                if (aUnitFamily == uppUnitFamilies.Default) { aUnitFamily = uppUnitFamilies.English; }
                _rVal = ((int)Units.UnitType <= 0) ? UnitCaption(aUnitFamily) : Units.Label(aUnitFamily);
            }
            catch (Exception exception)
            {
                LoggerManager log = new LoggerManager();
                log.LogError(exception.Message);
            }
            return _rVal;
        }

        /// <summary>
        ///returns the property value converted to the requested units
        /// </summary>
        /// <param name="aProp"></param>
        /// <param name="aToUnits"></param>
        /// <param name="aValue"></param>
        /// <param name="aFromUnits"></param>
        /// <returns></returns>
        public double UnitValue(uppUnitFamilies aToUnits = uppUnitFamilies.Default, dynamic aValue = null, uppUnitFamilies aFromUnits = uppUnitFamilies.Default)
        {

            try
            {
                if (!HasUnits) { return aValue != null ? (double)mzUtils.VarToDouble(aValue) : (double)mzUtils.VarToDouble(Value); }

                if (aValue == null) { aValue = Value; }
                aValue = mzUtils.VarToDouble(aValue);
                if (aToUnits == uppUnitFamilies.Default)
                { aToUnits = Units.UnitSystem; }
                if (aToUnits == uppUnitFamilies.Default)
                { aToUnits = uppUnitFamilies.English; }
                if (aFromUnits <= uppUnitFamilies.Default)
                { aFromUnits = Units.UnitSystem; }
                return aToUnits == aFromUnits ? (double)aValue : (double)Units.ConvertValue(aValue, aFromUnits, aToUnits);

            }
            catch (Exception exception)
            {
                LoggerManager log = new LoggerManager();
                log.LogError(exception.Message);
            }
            return 0;
        }


        /// <summary>
        ///returns the property converted to the requested units
        /// </summary>
        /// <param name="aProp"></param>
        /// <param name="aUnitFamily"></param>
        /// <param name="bZerosAsNullString"></param>
        /// <param name="bDefaultAsNullString"></param>
        /// <param name="aOverridePrecision"></param>
        /// <param name="bIncludeThousandsSeps"></param>
        /// <param name="bIncludeUnitString"></param>
        /// <param name="aFormatString"></param>
        /// <param name="aValue"></param>
        /// <returns></returns>
        public string UnitValueString(uppUnitFamilies aUnitFamily = uppUnitFamilies.Default, bool bZerosAsNullString = false,
            bool bDefaultAsNullString = false, int aOverridePrecision = -1, bool bIncludeThousandsSeps = false,
            bool bIncludeUnitString = false, string aFormatString = "", dynamic aValue = null,
            bool bYesNoForBool = false, bool bSuppressTrailingZeros = true, bool bAbsVal = false,
            dynamic aZeroValue = null)
        {

            string _rVal = string.Empty;
            string aFmat = string.Empty;
            string strVal = string.Empty;
            string strUni = string.Empty;
            double uval;

            TPROPERTY bProp = aValue != null ? new TPROPERTY(this) : this;
            if (aValue != null) bProp.SetValue(aValue);
            aValue = bProp.Value;
            if (bProp.IsNullValue) return _rVal;
            if (bIncludeUnitString)
            {
                strUni = bProp.UnitString(aUnitFamily);
                if (strUni !=  string.Empty) { strUni = " " + strUni; }
            }
            if (aUnitFamily == uppUnitFamilies.Default) { aUnitFamily = uppUnitFamilies.English; }
            if (bProp.Units.UnitType <= 0)
            {
                if (aValue.GetType() == typeof(bool))
                {
                    bool bval = (bool)aValue;
                    if (bYesNoForBool)
                    {
                        _rVal = bval ? "Yes" : "No";
                    }
                    else
                    {
                        _rVal = bval ? "True" : "False";
                    }

                }
                else
                {
                    _rVal = bProp.DecodedValue;
                }


            }
            else
            {
                double dval = bProp.ValueD;
                if (aZeroValue != null && aValue == 0)
                    dval = mzUtils.VarToDouble(aZeroValue);
                aZeroValue = null;
                if (bAbsVal) dval = Math.Abs(dval);
                uval = bProp.Units.ConvertValue(dval, bProp.Units.UnitSystem, aUnitFamily);
                aFmat = string.IsNullOrWhiteSpace(aFormatString) ? bProp.Units.FormatString(aUnitFamily, aOverridePrecision, bIncludeThousandsSeps, bSuppressTrailingZeros) : aFormatString.Trim();
                strVal = (aFmat !=  string.Empty) ? uval.ToString(aFmat) : Convert.ToString(uval);
                _rVal = strVal;
            }
            if (aZeroValue != null)
            {
                if (mzUtils.VarToDouble(_rVal) == 0) _rVal = mzUtils.VarToDouble(aZeroValue).ToString();
            }

            if (bZerosAsNullString && mzUtils.VarToDouble(_rVal) == 0) { _rVal = string.Empty; }

            if (_rVal !=  string.Empty && bDefaultAsNullString && bProp.IsDefault) { _rVal = string.Empty; }
            if (_rVal !=  string.Empty) { _rVal += strUni; }
            return _rVal;
        }

        public dynamic UnitVariant(uppUnitFamilies aToUnits = uppUnitFamilies.Default, dynamic aValue = null,
             uppUnitFamilies aFromUnits = uppUnitFamilies.Default, int aPrecis = -1)
        {

            TPROPERTY bProp = new TPROPERTY(this);
            if (aValue != null)
            {
                if (Units.IsDefined)
                {
                    if (mzUtils.IsNumeric(aValue)) { bProp.SetValue(mzUtils.VarToDouble(aValue)); }
                }
                else { bProp.Value = aValue; }
            }
            if (!Units.IsDefined) return bProp.Value;
            if (aToUnits == uppUnitFamilies.Default) { aToUnits = Units.UnitSystem; }
            if (aToUnits == uppUnitFamilies.Default) { aToUnits = uppUnitFamilies.English; }
            if (aFromUnits <= uppUnitFamilies.Default) { aFromUnits = Units.UnitSystem; }
            if (aToUnits == aFromUnits) return bProp.Value;
            return Units.ConvertValue(bProp.Value, aFromUnits, aToUnits, aPrecis);


        }

        /// <summary>
        /// the format string applied to the signature of numeric properties
        /// </summary>
        /// <param name="aProp"></param>
        /// <param name="aFamily"></param>
        /// <param name="aOverridePrecision"></param>
        /// <param name="bIncludeThousandsSeps"></param>
        /// <returns></returns>
        public string UnitFormatString(uppUnitFamilies aFamily, int aOverridePrecision = -1, bool bIncludeThousandsSeps = false)
        {

            if (Units.UnitType <= 0) return FormatString;

            if (aOverridePrecision < 0)
            {
                if (Precision > 0) aOverridePrecision = Precision;
            }

            if (Units.UnitType == uppUnitTypes.BigMassRate) bIncludeThousandsSeps = true;
            return Units.FormatString(aFamily, aOverridePrecision, bIncludeThousandsSeps);


        }

        /// <summary>
        /// a string that shows the name, units and value of the property
        /// </summary>
        /// <param name="aProp"></param>
        /// <param name="aUnitFamily"></param>
        /// <param name="bShowNulls"></param>
        /// <returns></returns>
        public string UnitSignature(uppUnitFamilies aUnitFamily = uppUnitFamilies.Default, bool bShowNulls = false)
        {
            string _rVal = string.Empty;

            try
            {
                if (aUnitFamily == uppUnitFamilies.Default) aUnitFamily = uppUnitFamilies.English;


                if (Units.UnitType <= 0)
                {
                    _rVal = Signature(true, bShowNulls, true);
                }
                else
                {
                    dynamic aVar = Units.ConvertValue(Value, Units.UnitSystem, aUnitFamily);
                    dynamic aVal = string.Empty;
                    bool bNull = IsNullValue;
                    string aLable = Units.Label(aUnitFamily, addleadSpace: true);
                    string aFmat = Units.FormatString(aUnitFamily, bIncludeThousandsSeps: true);

                    if (!bNull || bShowNulls)
                    {

                        if (aFmat !=  string.Empty)
                        {
                            aVal = aVar.ToString(aFmat);
                        }
                        else
                        {
                            aVal = Convert.ToString(aVar);
                        }
                    }
                    else { aLable = string.Empty; }
                    //_rVal = Name + "=" + (aVal + " " + aLable).ToString().Trim();
                    _rVal = $"{DisplayName}={aVal}{aLable}";
                }
            }
            catch (Exception exception)
            {
                LoggerManager log = new LoggerManager();
                log.LogError(exception.Message);
            }
            return _rVal;
        }


        /// <summary>
        /// #2flag to used the decoded value of the property value if there is a decode string
        /// a string that shows the name and value of the property
        /// like "Size=0.3213"
        /// </summary>
        /// <param name="aProp"></param>
        /// <param name="bShowDecodedValue"></param>
        /// <param name="bShowNulls"></param>
        /// <returns></returns>
        public string Signature(bool bShowDecodedValue = true, bool bShowNulls = false, bool bUseDisplayName = false)
        {
            string _rVal = string.Empty;
            dynamic aVar = Value;
            dynamic aVal = string.Empty;
            string fmt = string.Empty;
            bool bISNull = IsNullValue;
            try
            {
                if (!bISNull || (bISNull && bShowNulls))
                {
                    fmt = FormatString;
                    if (mzUtils.IsNumeric(aVar) && fmt != null && fmt !=  string.Empty)
                    { aVal = aVar.ToString(fmt); }
                    else
                    {
                        if (bShowDecodedValue)
                        { aVal = DecodedValue; }
                        else
                        { aVal = aVar; }
                    }
                }

                string name = bUseDisplayName ? DisplayName : Name;
                _rVal = $"{name}={aVal}";
            }
            catch (Exception exception)
            {
                LoggerManager log = new LoggerManager();
                log.LogError(exception.Message);
            }
            return _rVal;
        }

        public bool SetValue(dynamic aNewVal, dynamic aDefaultValue = null, dynamic aNullValue = null) => SetValue(aNewVal, out bool frst, aDefaultValue, aNullValue);

        public bool SetValue(dynamic aNewVal, out bool bFirstVal, dynamic aDefaultValue = null, dynamic aNullValue = null)
        {

            bool _rVal = false;
            try
            {

                _VariableTypeName = TVALUES.GetDynamicTypeName(_Value);

                bFirstVal = !HasValue;
                if (aNewVal != null)
                {
                    ValueChanged = false;

                    string newT = TVALUES.GetDynamicTypeName(aNewVal);
                    string tname = string.Empty;
                    bool typeislocked = TypeLocked;

                    if (Units.IsDefined)
                    {

                        if (!mzUtils.IsNumeric(aNewVal))
                        {
                            aNewVal = _Value;
                        }
                        typeislocked = true;
                        aNewVal = mzUtils.VarToDouble(aNewVal);
                        tname = "DOUBLE";
                    }

                    if (Locked && !bFirstVal) throw new Exception("This Property Is Currently Locked!");

                    if (tname ==  string.Empty) tname = !bFirstVal ? VariableTypeName : newT;
                    bool typechange = tname != newT;

                    if (typeislocked && !bFirstVal && typechange)
                    {

                        if (tname == "BOOL")
                        {
                            aNewVal = mzUtils.VarToBoolean(aNewVal);
                        }
                        else if (tname == "DOUBLE")
                            aNewVal = mzUtils.VarToDouble(aNewVal);
                        else
                        {
                            aNewVal = TPROPERTY.MatchToType(Value, aNewVal, tname);
                        }
                        typechange = false;
                    }

                    if (bFirstVal)
                    {
                        _VariableTypeName = tname;
                        _Value = aNewVal;
                        LastValue = _Value;
                        return true;

                    }
                    if (_Value.GetType().IsEnum && tname != newT)
                    {
                        if (!mzUtils.IsNumeric(aNewVal)) return false;
                        Enum enumval = uopEnumHelper.GetMatchingMember(_Value.GetType(), mzUtils.VarToInteger(aNewVal), out bool found);
                        if (!found) return false;
                        Enum valenum = _Value as Enum;
                        _rVal = string.Compare(enumval.ToString(), valenum.ToString()) != 0;
                        if (_rVal) { LastValue = _Value; ValueChanged = true; }
                        _Value = enumval;
                        _VariableTypeName = tname;
                        return _rVal;


                    }


                    if (!typechange)
                    {
                        _rVal = aNewVal != _Value;
                        if (_rVal) { LastValue = _Value; ValueChanged = true; }
                        _Value = aNewVal;
                        return _rVal;
                    }

                    // a rtype cchane has occured at this point

                    _VariableTypeName = newT;
                    if (mzUtils.IsNumeric(aNewVal))
                    {
                        _rVal = mzUtils.VarToDouble(_Value) != (double)aNewVal;
                    }
                    else
                    {
                        _rVal = string.Compare(_Value.ToString(), aNewVal.ToString()) != 0;
                    }
                    ValueChanged = true;
                    if (_rVal) { LastValue = TPROPERTY.MatchToType(aNewVal, Value, newT); ; ValueChanged = true; }
                    _Value = aNewVal;

                }

                if (aDefaultValue != null)
                    DefaultValue = MatchToType(_Value, aDefaultValue, null);

                if (aNullValue != null)
                    NullValue = MatchToType(_Value, aNullValue, null);


                return _rVal;


            }
            catch (Exception exception)
            {
                throw exception;

            }


        }



        public void LockType()
        {

            if (Value != null)
            {
                TypeLocked = true;
                if (string.IsNullOrWhiteSpace(_VariableTypeName)) _VariableTypeName = TVALUES.GetDynamicTypeName(Value);
            }

        }

        public bool ReadFromINIFile(string aFileSpec, string aFileSection, bool bRaiseNotFoundError = true)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(aFileSpec))
                { throw new Exception("The Passed File Name Is Invalid"); }
                if (aFileSection ==  string.Empty)
                { throw new Exception("The Passed File Section Is Invalid"); }
                if (!System.IO.File.Exists(aFileSpec))
                { throw new Exception("The Passed File Name Is Invalid"); }
                if (Name ==  string.Empty) { throw new Exception("Property Must Be Initialized Before It Can Be Read From File"); }


                if (!HasValue) { throw new Exception("Property '" + Name + "' Must Be Initialized Before It Can Be Read From File"); }
                string tname = string.Empty;
                string sVal = string.Empty;

                string defVal = string.Empty;
                if (DefaultValue != null)
                { defVal = Convert.ToString(DefaultValue); }
                else
                { defVal = ValueS; }

                sVal = uopUtils.ReadINI_String(aFileSpec, aFileSection, Name, defVal, out bool found);
                if (!found)
                {
                    if (bRaiseNotFoundError) { throw new Exception("'" + Name + "' Not Found in Section - '" + aFileSection + "'"); }
                    return false;
                }
                else
                {
                    if (!TypeLocked)
                    { tname = Value.GetType().ToString().ToUpper(); }
                    else
                    { tname = VariableTypeName; }
                    Value = TPROPERTY.SetType(tname, sVal);
                    return true;
                }


            }
            catch (Exception e)
            {
                throw e;

            }


        }

        /// <summary>
        /// sets the value of the property to the new value is it's current value is equal to the passed value
        /// returns true if the value changes
        /// </summary>
        /// <param name="aProp">the property</param>
        /// <param name="aValue">test value</param>
        /// <param name="aNewValue">the new value</param>
        /// <param name="bStringCompare">flag to test by a non-casesensitive string comparison</param>
        /// <param name="aPrecis">numerical precisions to apply to</param>
        /// <param name="aElseValue">value to apply if the current value does NOT match the test value</param>
        /// <returns></returns>
        public bool ThisThenThis(dynamic aValue, dynamic aNewValue, bool bStringCompare, int aPrecis = -1, dynamic aElseValue = null)
        {
            bool _rVal = false;
            bool bEqual = false;

            if (!bStringCompare)
            {
                if (aPrecis < 0)
                { bEqual = aValue == Value; }
                else
                {
                    bEqual = Math.Round(aValue, aPrecis) == Math.Round(Convert.ToDouble(Value), aPrecis);
                }
            }
            else
            {
                bEqual = string.Compare(aValue, Value, true) == 0;
            }
            if (bEqual)
            {
                if (SetValue(aNewValue))
                { _rVal = true; }
            }
            else
            {
                if (aElseValue != null)
                {
                    if (SetValue(aElseValue))
                    { _rVal = true; }
                }
            }
            return _rVal;
        }

        #region Shared Methods

        public static TPROPERTY Quick(string aName, dynamic aValue, dynamic aLastValue, uopPart aPart = null) => new TPROPERTY(aName, aValue, aLastValue: aLastValue, aPart: aPart);

        /// <summary>
        /// used to set the value if the variable type of the property is locked
        /// </summary>
        /// <param name="aProp"></param>
        /// <param name="aValue"></param>
        /// <param name="aTypeName"></param>
        /// <returns></returns>
        public static dynamic MatchToType(dynamic baseVal, dynamic aValue, string aDynamicTypeName)
        {
            if (baseVal == null || aValue == null) { return aValue; }

            try
            {
                string tname = aDynamicTypeName?.Trim().ToUpper();
                string newT = TVALUES.GetDynamicTypeName(aValue);
                if (string.IsNullOrEmpty(tname)) { tname = TVALUES.GetDynamicTypeName(baseVal); }
                return mzUtils.ConvertDynamic(aValue, tname);

            }
            catch (Exception)
            {
                return aValue;
            }

        }



        /// <summary>
        /// Takes PatternType as String and return corresponding Enum Value
        /// </summary>
        /// <param name="aProp"></param>
        /// <param name="aString"></param>
        /// <returns></returns>
        public static string GetCodedValue(TPROPERTY aProp, string aString)
        {
            string _rVal = string.Empty;
            if (aProp.DecodeString !=  string.Empty)
            {
                int i = 0;
                string comp1 = string.Empty;
                string comp2 = string.Empty;
                string dcdval = string.Empty;
                int j = 0;
                comp1 = aString.Trim();
                string[] dCodes = aProp.DecodeString.Split(',');
                //comp1 = comp1.Substring(10);
                for (i = 0; i < dCodes.Length; i++)
                {
                    comp2 = dCodes[i];
                    j = comp2.IndexOf("=");
                    if (j > 0)
                    {
                        dcdval = comp2.Substring(j + 1);
                        comp2 = comp2.Substring(0, j);
                        if (string.Compare(comp1, dcdval, true) == 0 || (dcdval.Equals("*S*") && comp1.ToUpper().Equals("SSTAR")) || (dcdval.Equals("*A*") && comp1.ToUpper().Equals("ASTAR")))
                        {
                            _rVal = comp2;
                            break;
                        }
                    }
                }
            }
            return _rVal;
        }


        public static dynamic DecodeValue(TPROPERTY aProp, dynamic aValue)
        {

            dynamic val = aValue ?? aProp.Value;
            if (val == null) return "";

            string _rVal = val.GetType().IsEnum ? Convert.ToInt32(val).ToString() : Convert.ToString(val);



            if (aProp.DecodeString ==  string.Empty) return _rVal;
            string comp1 = string.Empty;
            string comp2 = string.Empty;
            string dcdval = string.Empty;
            int j = 0;
            comp1 = _rVal;
            string[] dCodes = aProp.DecodeString?.Split(',');
            for (int i = 0; i < dCodes?.Length; i++)
            {
                comp2 = dCodes[i];
                j = comp2.IndexOf("=");
                if (j != -1)
                {
                    dcdval = comp2.Substring(j + 1);
                    comp2 = comp2.Substring(0, j);
                    if (string.Compare(comp1, comp2, true) == 0)
                    {
                        _rVal = dcdval;
                        break;
                    }
                }
            }
            return _rVal;


        }
        public static bool Compare(TPROPERTY aProp, TPROPERTY bProp, int aPrecis = 5, bool bCompareNames = false)
        {

            aPrecis = mzUtils.LimitedValue(aPrecis, 1, 10);


            if (bCompareNames)
            {
                if (string.Compare(aProp.Name, bProp.Name, ignoreCase: true) != 0) return false;
            }

            bool numbr = mzUtils.IsNumeric(aProp.Value);

            if (numbr != mzUtils.IsNumeric(bProp.Value)) return false;


            return !numbr
                ? (string.Compare(aProp.Value.ToString(), bProp.Value.ToString(), ignoreCase: true) == 0)
                : (mzUtils.VarToDouble(aProp.Value, aPrecis: aPrecis) == mzUtils.VarToDouble(bProp.Value, aPrecis: aPrecis));


        }


        public static dynamic SetType(string aTypeName, dynamic newval)
        {
            dynamic _rVal = newval;
            aTypeName ??= string.Empty;
            try
            {
                _rVal = newval;
                switch (aTypeName.ToUpper())
                {
                    case "SINGLE":
                        _rVal = mzUtils.VarToSingle(newval, aPrecis: 6);
                        break;
                    case "DOUBLE":
                        _rVal = mzUtils.VarToDouble(newval, aPrecis: 6);
                        break;
                    case "STRING":
                        _rVal = Convert.ToString(newval);
                        break;
                    case "SYSTEM.INT32":
                        _rVal = mzUtils.VarToInteger(newval);
                        break;
                    case "INTEGER":
                        _rVal = mzUtils.VarToInteger(newval);
                        break;
                    case "LONG":
                        _rVal = mzUtils.VarToLong(newval);
                        break;
                    case "SYSTEM.BOOLEAN":
                    case "BOOLEAN":
                        _rVal = mzUtils.VarToBoolean(newval);
                        break;
                    case "CURRENCY":
                        _rVal = Convert.ToDecimal(mzUtils.VarToSingle(newval));
                        break;
                    case "DECIMAL":
                        _rVal = mzUtils.VarToDouble(newval);
                        break;
                    case "DATE":
                        if (!mzUtils.IsDate(newval)) { newval = Convert.ToString(DateTime.Now); }
                        _rVal = Convert.ToDateTime(newval);

                        break;
                    case "VARIANT":
                    case "DYNAMIC":
                        _rVal = newval;
                        break;
                    default:
                        _rVal = newval;
                        break;
                }
                return _rVal;
            }
            catch (Exception exception)
            {
                LoggerManager log = new LoggerManager();
                log.LogError(exception.Message);
                return _rVal;
            }
        }

        public static TPROPERTY ReadFromINIFile(TPROPERTY aProp, string aFileSpec, string aFileSection)
        {
            TPROPERTY _rVal = new TPROPERTY(aProp);
            if (string.IsNullOrWhiteSpace(aFileSpec)) { throw new Exception("The Passed File Name Is Invalid"); }
            if (string.IsNullOrWhiteSpace(aFileSection)) { throw new Exception("The Passed File Section Is Invalid"); }
            if (!System.IO.File.Exists(aFileSpec)) { throw new Exception("The Passed File Name Is Invalid"); }
            if (string.IsNullOrWhiteSpace(aProp.Name) || !aProp.HasValue) { throw new Exception("Property '" + aProp.Name + "' Must Be Initialized Before It Can Be Read From File"); }


            string defVal = aProp.HasDefaultValue ? Convert.ToString(aProp.DefaultValue) : Convert.ToString(aProp.Value);
            string sVal = uopUtils.ReadINI_String(aFileSpec, aFileSection, aProp.Name, defVal, out bool found);
            string tname = aProp.VariableTypeName;
            _rVal.Value = TPROPERTY.SetType(tname, sVal);
            return _rVal;
        }



        /// <summary>
        /// #2the key in the registry to save the property under
        /// #3the aSection under the key to save the setting to
        /// the root key is always HKEY_CURRENT_USER\Software\VB and VBA Program Settings.
        /// wont save a property that is not named or whose value has not been initialized
        /// </summary>
        /// <param name="aProp"></param>
        /// <param name="aAppName"></param>
        /// <param name="aSection"></param>
        public static void SaveToRegistry(ref TPROPERTY aProp, string aAppName, string aSection)
        {
            if (aProp.Name ==  string.Empty) { return; }
            if (!aProp.HasValue) { return; }

            if (string.IsNullOrWhiteSpace(aAppName)) { return; }
            aAppName = aAppName.Trim();
            if (string.IsNullOrWhiteSpace(aSection)) { return; }
            aSection = aSection.Trim();
            throw new NotImplementedException();
            //TODO
            //SaveSetting(aAppName, aSection, aProp.Name, Convert.ToString(aProp.Value));
        }

        public static TPROPERTY Null => new TPROPERTY("") { Index = 0 };

        #endregion Shared Methods
    }

    internal struct TPROPERTIES //: ICloneable
    {
        public string NodeName;
        public string ProjectHandle;


        public string Category;
        public uppPartTypes PartType;
        internal string _RangeGUID;
        internal  string _PartName;
        internal string _PartPath;
        internal int _PartIndex;

        public TPROPERTIES[] StoredValues;
        private TPROPERTY[] _Members;
        private bool _Init;
        private int _Count;

        #region Constructors

        public TPROPERTIES(uppPartTypes aPartType = uppPartTypes.Undefined)
        {

            _Count = 0;
            StoredValues = new TPROPERTIES[0];
            NodeName = string.Empty;
            Category = string.Empty;
            _RangeGUID = string.Empty;
            ProjectHandle = string.Empty;
            _Members = new TPROPERTY[0];
            PartType = aPartType;
            _PartName = string.Empty;
            _PartPath = string.Empty;
            _PartIndex = 0;
            _Init = true;

        }

        public TPROPERTIES(uopProperties aProperties)
        {

            _Count = 0;
            StoredValues = new TPROPERTIES[0];
            NodeName = string.Empty;
            Category = string.Empty;
            _RangeGUID = string.Empty;
            ProjectHandle = string.Empty;
            _Members = new TPROPERTY[0];
            PartType = uppPartTypes.Undefined;
            _PartName = string.Empty;
            _PartPath = string.Empty;
            _PartIndex = 0;
            _Init = true;

            if (aProperties == null) return;

            NodeName = aProperties.NodeName;
            Category = aProperties.Category;
            _RangeGUID = aProperties.RangeGUID;
            ProjectHandle = aProperties.ProjectHandle;
            PartType = aProperties.PartType;
            PartName = aProperties.PartName;
            PartPath = aProperties.PartPath;
            PartIndex = aProperties.PartIndex;
            foreach (uopProperty item in aProperties)
            {
                Add(new TPROPERTY(item));
            }

            if (aProperties._StoredVals != null)
            {
                int cnt = aProperties._StoredVals.Count;
                Array.Resize<TPROPERTIES>(ref StoredValues, cnt);

                for (int i = 1; i <= aProperties._StoredVals.Count; i++)
                {
                    StoredValues[i - 1] = aProperties._StoredVals[i - 1].Clone();
                }
            }

        }


        public TPROPERTIES(String aCategory)
        {
            _Count = 0;
            StoredValues = new TPROPERTIES[0];
            NodeName = string.Empty;
            Category = aCategory;
            _RangeGUID = string.Empty;
            _PartName = string.Empty;
            _PartPath = string.Empty;
            ProjectHandle = string.Empty;
            _PartIndex = 0;
            _Members = new TPROPERTY[0];
            PartType = uppPartTypes.Undefined;
            _Init = true;

        }


        public TPROPERTIES(TPROPERTY[] aMembers)
        {
            StoredValues = new TPROPERTIES[0];
            NodeName = string.Empty;
            Category = string.Empty;
            _RangeGUID = string.Empty;
            _PartName = string.Empty;
            _PartPath = string.Empty;
            ProjectHandle = string.Empty;
            _PartIndex = 0;
            _Members = aMembers ?? (new TPROPERTY[0]);
            _Count = _Members.Length;
            PartType = uppPartTypes.Undefined;
            _Init = true;

        }

        #endregion

        /// <summary>
        ///returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>
       // object ICloneable.Clone() => (object)Clone(false);

        /// <summary>
        ///returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>
        public TPROPERTIES Clone(bool bReturnEmpty = false)
        {
            if (!_Init) { Init(); }
            int cnt = (bReturnEmpty || _Count == 0) ? 0 : _Count;
            TPROPERTY[] mems = new TPROPERTY[cnt];
            //TPROPERTY[] mems = (bReturnEmpty || _Count == 0) ? new TPROPERTY[0] : new TPROPERTY[Count];// (TPROPERTY[])_Members; //.Clone();// Force.DeepCloner.DeepClonerExtensions.ShallowClone<TPROPERTY[]>(_Members);      //(TPROPERTY[])_Members.Clone();
            for (int i = 0; i < cnt; i++) { mems[i] = new TPROPERTY(_Members[i]); }
            return new TPROPERTIES(mems)
            {
                Category = Category,
                NodeName = NodeName,
                ProjectHandle = ProjectHandle,
                _PartName = _PartName,
                _PartPath = _PartPath,
                _PartIndex = _PartIndex,
                _RangeGUID = _RangeGUID,
                PartType = PartType
            };
            //clones don't get to keep their stored values
            // StoredValues = Force.DeepCloner.DeepClonerExtensions.DeepClone<TPROPERTIES[]>(StoredValues),
        }


        public TPROPERTIES Clone(uopPart aParentPart, bool bReturnEmpty = false)
        {
            if (!_Init) { Init(); }

            SubPart(aParentPart);
            TPROPERTY[] mems = bReturnEmpty ? new TPROPERTY[0] : Force.DeepCloner.DeepClonerExtensions.DeepClone<TPROPERTY[]>(_Members);  //(TPROPERTY[])_Members.Clone();
            int cnt = bReturnEmpty ? 0 : _Count;

            return new TPROPERTIES(Category)
            {
                _Count = cnt,
                _Members = mems,
                NodeName = NodeName,
                ProjectHandle = ProjectHandle,
                StoredValues = Force.DeepCloner.DeepClonerExtensions.DeepClone<TPROPERTIES[]>(StoredValues),
                _RangeGUID = _RangeGUID,
                PartType = PartType,
                _PartName = _PartName,
                _PartPath = _PartPath,
                _PartIndex = _PartIndex,
                _Init = true
            };
        }



        public override string ToString()
        {
            return (!_Init) ? "TPROPERTIES{null}" : "TPROPERTIES [" + _Members.Length + "]";
        }

        private int Init()
        {

            _Members = new TPROPERTY[0];
            StoredValues = new TPROPERTIES[0];
            NodeName = string.Empty;
            Category = string.Empty;
            _RangeGUID = string.Empty;
            ProjectHandle = string.Empty;
            _PartIndex = 0;
            _Count = 0;
            PartType = uppPartTypes.Undefined;
            _PartName = string.Empty;
            _PartPath = string.Empty;
            _Init = true;
            return 0;
        }


        public int PartIndex { get => _PartIndex; set => _PartIndex = value; }



        public string PartName { get => _PartName; set => _PartName = value; }


        public int StoredValueCount => (StoredValues != null) ? StoredValues.Count() : 0;

        public bool StoredValuesClear() { bool _rVal = (StoredValues != null) && StoredValues.Count() > 0; StoredValues = new TPROPERTIES[0]; return _rVal; }


        /// <summary>
        /// the path to the properties collection
        /// like Column(1).Range(1).TrayAssembly.Deck.Properties
        /// </summary>
        public string PartPath { get { string _rVal = _PartPath; return (!string.IsNullOrWhiteSpace(_rVal)) ? _rVal + "Properties" : "Properties"; } set { _PartPath = value; } }

        public string RangeGUID
        {
            get => _RangeGUID;
            set
            {
                if (_RangeGUID != value)
                {
                    _RangeGUID = value;
                    TPROPERTY mem;
                    for (int i = 1; i <= Count; i++)
                    {
                        mem = Item(i);
                        mem.RangeGUID = value;
                        SetItem(i, mem);
                    }

                }
            }
        }

        /// <summary>
        /// resets the indexs of the properties in the collection
        /// </summary>
        /// <param name="aProps"></param>
        /// <returns></returns>
        public void ReIndex()
        {
            for (int i = 1; i <= Count; i++)
            {
                TPROPERTY mem = Item(i);
                mem.Index = i;
                SetItem(i, mem);
            }
        }

        public TPROPERTY Item(string aName, int aOccur = 1)
        {

            return TryGet(aName, out TPROPERTY mem, aOccur) ? mem : new TPROPERTY("");
        }


        public string StoreValues(bool bClearExisting = false)
        {
            string _rVal = string.Empty;
            try
            {
                if (bClearExisting) { StoredValues = new TPROPERTIES[0]; }
                int cnt = StoredValues.Count() + 1;
                _rVal = StringVals(uopGlobals.Delim, bIncludePropertyNames: true);
                Array.Resize<TPROPERTIES>(ref StoredValues, cnt);
                StoredValues[cnt - 1] = Clone();
            }
            catch (Exception exception)
            {
                LoggerManager log = new LoggerManager();
                log.LogError(exception.Message);
            }
            return _rVal;
        }

        public void SetValuesByString(string aValueString, bool bNamedList, string aDelimitor, out TPROPERTIES rChanges)
        {

            aValueString = aValueString.Trim();
            rChanges = Clone(bReturnEmpty: true);

            TPROPERTY aMem;
            string[] sVals;
            string sVal = string.Empty;
            string aName = string.Empty;
            int idx = 0;
            if (aDelimitor !=  string.Empty)
            {
                if (aValueString.IndexOf(aDelimitor, StringComparison.OrdinalIgnoreCase) != -1)
                {
                    sVals = aValueString.Split(aDelimitor.ToCharArray());
                }
                else
                {
                    sVals = new string[1];
                    sVals[0] = aValueString;
                }
            }
            else
            {
                sVals = new string[1];
                sVals[0] = aValueString;
            }
            for (int i = 0; i < sVals.Length; i++)
            {
                sVal = sVals[i].Trim();
                if (bNamedList)
                {
                    mzUtils.SplitString(sVal, "=", out aName, out sVal);
                    if (TryGet(aName, out aMem)) { idx = aMem.Index; } else { idx = 0; }
                }
                else
                {
                    idx = i + 1;
                }

                if (idx > 0 & idx <= Count)
                {
                    aMem = Item(idx);
                    if (aMem.SetValue(sVal))
                    {
                        rChanges.Add(aMem);
                    }
                    SetItem(i, aMem);
                }

            }

        }


        //Base ONE
        public TPROPERTY Item(int aIndex)
        {
            {
                if (aIndex <= 0 || aIndex > Count) return new TPROPERTY("");
                _Members[aIndex - 1].Index = aIndex;
                _Members[aIndex - 1].SubPart(this);
                return _Members[aIndex - 1];

            }
        }


        public void LockTypes(bool bSetDefaults = false)
        {
            TPROPERTY aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                aMem.TypeLocked = true;
                if (aMem.Value != null && aMem.VariableTypeName != null)
                {
                    var str = aMem.Value.GetType();
                    aMem.VariableTypeName = str.Name;
                }
                if (bSetDefaults)
                {
                    aMem.DefaultValue = aMem.Value;
                }
                SetItem(aMem.Index, aMem);
            }

        }

        public bool IsHidden(dynamic aNameOrIndex)
        {
            return TryGet(aNameOrIndex, out TPROPERTY member) ? member.Hidden : false;
        }

        public bool IsLocked(dynamic aNameOrIndex)
        {

            return TryGet(aNameOrIndex, out TPROPERTY member) ? member.Locked : false;
        }

        public void SetRowCol(int aIndex, dynamic aRow = null, dynamic aCol = null)
        {
            if (aIndex < 1 || aIndex > Count) return;
            if (aRow != null) _Members[aIndex - 1].Row = mzUtils.VarToInteger(aRow);
            if (aCol != null) _Members[aIndex - 1].Col = mzUtils.VarToInteger(aCol);
        }


        public void SetItem(int aIndex, TPROPERTY aMember)
        //Base ONE
        {
            {
                if (aIndex <= 0 || aIndex > Count) return;

                _Members[aIndex - 1] = aMember;
            }
        }
        public void Clear() { _Members = new TPROPERTY[0]; _Count = 0; StoredValues = new TPROPERTIES[0]; }

        public int SetMemberValueChange(bool newVal, string skipList = null)
        {
            int _rVal = 0;
            TPROPERTY mem;

            for (int i = 1; i <= Count; i++)
            {
                mem = Item(i);
                if (!mzUtils.ListContains(mem.Name, skipList))
                {
                    if (mem.ValueChanged != newVal) { _rVal += 1; }
                    mem.ValueChanged = newVal;
                    SetItem(i, mem);
                }
            }
            return _rVal;
        }

        public void SetUnitCaption(string aUnitCaption, int aStartIndex = 0, int aEndIndex = 0)
        {

            mzUtils.LoopLimits(aStartIndex, aEndIndex, 1, Count, out int si, out int ei);
            TPROPERTY prop;

            for (int i = si; i <= ei; i++)
            {
                prop = Item(i);
                prop.SetUnitCaption(aUnitCaption);
                SetItem(i, prop);
            }
        }

        public void SetHidden(dynamic aNameOrIndex, bool aHiddenValue)
        {
            if (TryGet(aNameOrIndex, out TPROPERTY member))
            {
                member.Hidden = aHiddenValue;
                SetItem(member.Index, member);
            }
        }


        /// <summary>
        /// assigns the passed hidden value to all the properties in the collection
        /// </summary>
        /// <param name="aHiddenValue"></param>
        /// <param name="aStartIndex"></param>
        /// <param name="aEndIndex"></param>
        /// <returns></returns>
        public void SetHidden(List<string> aHiddenNames, bool bUnHideNonMemebers, int aStartIndex = 0, int aEndIndex = 0)
        {
            if (Count <= 0 || aHiddenNames == null) return;
            mzUtils.LoopLimits(aStartIndex, aEndIndex, 1, Count, out int si, out int ei);
            TPROPERTY prop;


            for (int i = si; i <= ei; i++)
            {
                prop = Item(i);
                if (aHiddenNames.FindIndex(x => string.Compare(prop.Name, x, true) == 0) >= 0)
                {
                    prop.Hidden = true;
                }
                else
                {
                    if (bUnHideNonMemebers) prop.Hidden = false;
                }
                SetItem(i, prop);
            }


        }

        /// <summary>
        /// assigns the passed hidden value to all the properties in the collection
        /// </summary>
        /// <param name="aHiddenValue"></param>
        /// <param name="aStartIndex"></param>
        /// <param name="aEndIndex"></param>
        /// <returns></returns>
        public void SetHidden(bool aHiddenValue, int aStartIndex = 0, int aEndIndex = 0)
        {
            if (Count <= 0) return;
            mzUtils.LoopLimits(aStartIndex, aEndIndex, 1, Count, out int si, out int ei);
            TPROPERTY prop;


            for (int i = si; i <= ei; i++)
            {
                prop = Item(i);
                prop.Hidden = aHiddenValue;
                SetItem(i, prop);
            }


        }

        /// <summary>
        /// assigns the passed hidden value to all the properties in the collection whose values are current default (or not)
        /// </summary>
        /// <param name="aHiddenValue"></param>
        /// <param name="aDefaultValue"></param>
        /// <param name="aStartIndex"></param>
        /// <param name="aEndIndex"></param>
        /// <param name="bToggle"></param>
        /// <returns></returns>
        public void SetHiddenByDefault(bool aHiddenValue, bool aDefaultValue, int aStartIndex = 0, int aEndIndex = 0, bool bToggle = false)
        {
            if (Count <= 0) return;
            mzUtils.LoopLimits(aStartIndex, aEndIndex, 1, Count, out int si, out int ei);
            TPROPERTY prop;

            for (int i = si; i <= ei; i++)
            {
                prop = Item(i);
                if (prop.IsDefault == aDefaultValue)
                {

                    prop.Hidden = aHiddenValue;
                    SetItem(i, prop);
                }
                else
                {
                    if (bToggle)
                    {

                        prop.Hidden = !aHiddenValue;
                        SetItem(i, prop);
                    }
                }
            }




        }

        /// <summary>
        /// assigns the passed string to the heading property of all the properties in the collection
        /// </summary>
        /// <param name="aProps"></param>
        /// <param name="aHeadingString"></param>
        /// <param name="aStartIndex"></param>
        /// <param name="aEndIndex"></param>
        /// <returns></returns>
        public void SetHeadings(string aHeadingString, int aStartIndex = 0, int aEndIndex = 0)
        {
            if (Count <= 0) return;
            mzUtils.LoopLimits(aStartIndex, aEndIndex, 1, Count, out int si, out int ei);
            TPROPERTY prop;
            for (int i = si; i <= ei; i++)
            {
                prop = Item(i);
                prop.Heading = aHeadingString;
                SetItem(i, prop);
            }

        }

        /// <summary>
        /// assigns the passed hidden value to all the properties in the collection
        /// </summary>
        /// <param name="aProps"></param>
        /// <param name="aProtectedValue"></param>
        /// <param name="aStartIndex"></param>
        /// <param name="aEndIndex"></param>
        /// <returns></returns>
        public void SetProtected(bool aProtectedValue, int aStartIndex = 0, int aEndIndex = 0)
        {
            if (Count <= 0) return;
            mzUtils.LoopLimits(aStartIndex, aEndIndex, 1, Count, out int si, out int ei);
            TPROPERTY prop;

            for (int i = si; i <= ei; i++)
            {
                prop = Item(i);
                prop.Protected = aProtectedValue;
                SetItem(i, prop);
            }

        }


        /// <summary>
        /// assigns the passed string to the heading property of all the properties in the collection
        /// </summary>
        /// <param name="aPartType"></param>
        /// <param name="aStartIndex"></param>
        /// <param name="aEndIndex"></param>
        /// <returns></returns>
        public void SetPartTypes(uppPartTypes aPartType, int aStartIndex = 0, int aEndIndex = 0)
        {
            if (Count <= 0) return;
            mzUtils.LoopLimits(aStartIndex, aEndIndex, 1, Count, out int si, out int ei);
            TPROPERTY prop;
            for (int i = si; i <= ei; i++)
            {
                prop = Item(i);
                prop.PartType = aPartType;
                SetItem(i, prop);
            }
        }


        /// <summary>
        /// assigns the passed string to the Format string to the indicated properties
        /// </summary>
        /// <param name="aFormatString"></param>
        /// <param name="aStartIndex"></param>
        /// <param name="aEndIndex"></param>
        /// <returns></returns>
        public void SetFormatString(string aFormatString, int aStartIndex = 0, int aEndIndex = 0)
        {
            if (Count <= 0) return;
            mzUtils.LoopLimits(aStartIndex, aEndIndex, 1, Count, out int si, out int ei);
            TPROPERTY prop;

            for (int i = si; i <= ei; i++)
            {
                prop = Item(i);
                prop.FormatString = aFormatString;
                SetItem(i, prop);
            }

        }

        /// <summary>
        /// assigns the passed variant to the category property of all the properties in the collection
        /// </summary>
        /// <param name="aProps"></param>
        /// <param name="aCategory"></param>
        /// <param name="aStartIndex"></param>
        /// <param name="aEndIndex"></param>
        /// <returns></returns>
        public void SetCategories(string aCategory, int aStartIndex = 0, int aEndIndex = 0)
        {
            if (Count <= 0) return;
            mzUtils.LoopLimits(aStartIndex, aEndIndex, 1, Count, out int si, out int ei);
            TPROPERTY prop;
            for (int i = si; i <= ei; i++)
            {
                prop = Item(i);
                prop.Category = aCategory;
                SetItem(i, prop);
            }
        }

        /// <summary>
        /// used to reset all or some of the member properties back to their current default values
        /// </summary>
        /// <param name="aProps"></param>
        /// <param name="aStartIndex">the index to begin the procedure (default = 1)</param>
        /// <param name="aEndIndex">the index to end the procedure (default = count)</param>
        /// <returns></returns>
        public void ResetToDefaults(int aStartIndex = 0, int aEndIndex = 0)
        {
            if (Count <= 0) return;
            mzUtils.LoopLimits(aStartIndex, aEndIndex, 1, Count, out int si, out int ei);
            TPROPERTY aMem;


            for (int i = si; i <= ei; i++)
            {
                aMem = Item(i);
                if (aMem.HasDefaultValue)
                {
                    aMem.Value = aMem.DefaultValue;
                    SetItem(i, aMem);
                }
            }

        }

        public void ResetValueChange(int aStartIndex = 0, int aEndIndex = 0)
        {
            if (Count <= 0) return;
            mzUtils.LoopLimits(aStartIndex, aEndIndex, 1, Count, out int si, out int ei);
            TPROPERTY aMem;


            for (int i = si; i <= ei; i++)
            {
                aMem = Item(i);
                aMem.ValueChanged = false;
                SetItem(i, aMem);

            }

        }


        public void RestoreValues(out TPROPERTIES rProps)
        {
            rProps = Clone(bReturnEmpty: true);


            try
            {

                if (StoredValues == null) StoredValues = new TPROPERTIES[0];
                int cnt = StoredValues.Count();
                if (cnt <= 0) return;

                TPROPERTIES copies = StoredValues[cnt - 1];
                Array.Resize<TPROPERTIES>(ref StoredValues, cnt - 1);
                if (copies.Count != Count) return;
                TPROPERTY copy;
                TPROPERTY myProp;
                for (int i = 1; i <= copies.Count; i++)
                {
                    copy = copies.Item(i);
                    myProp = Item(i);
                    if (string.Compare(copy.Name, myProp.Name, ignoreCase: true) == 0)
                    {
                        if (myProp.SetValue(copy.Value))
                        {
                            rProps.Add(new TPROPERTY(myProp));
                            SetItem(i, myProp);
                        }
                    }
                }


            }
            catch (Exception exception)
            {
                LoggerManager log = new LoggerManager();
                log.LogError(exception.Message);
            }

        }


        /// <summary>
        /// sets the values of the members to the values in the passed list
        /// if the list includes names like "Color=Red,Size=3.2" then then names are used to define the values otherwise
        /// the valus are assigned as the occur in then current list like "Red,3.2" the first memebr will be set to "Red"
        /// </summary>
        /// <param name="aProps"></param>
        /// <param name="aValueString">string of values</param>
        /// <param name="bNamedList">flag indicating that the property names are in the passed list</param>
        /// <param name="aDelimitor">the delimitor that seperates the members of the list</param>
        /// <param name="rChanges"></param>
        /// <returns></returns>
        public bool SetValue(dynamic aNameOrIndex, dynamic aValue, int aOccur = 1)
        {

            if (TryGet(aNameOrIndex, out TPROPERTY mem, aOccur) == true)
            {
                bool _rVal = mem.SetValue(aValue);
                if (_rVal == true) SetItem(mem.Index, mem);
                return _rVal;
            }

            return false;
        }

        public bool SetValueD(dynamic aNameOrIndex, double aValue, dynamic aMultiplier = null, int aOccur = 1)
        {

            if (!TryGet(aNameOrIndex, out TPROPERTY mem, aOccur)) return false;
            if (aMultiplier != null) aValue *= mzUtils.VarToDouble(aMultiplier);
            bool _rVal = mem.SetValue(aValue);
            if (_rVal == true) SetItem(mem.Index, mem);
            return _rVal;

        }

        public int Count => (!_Init) ? Init() : _Count;

        /// <summary>
        /// shorthand method for adding a property to the collection
        /// </summary>
        /// <param name="aProps">1the subject properties</param>
        /// <param name="aPropName">the name of the property to add</param>
        /// <param name="aPart">the part to get the property from</param>
        /// <param name="aDisplayName">the caption assigned to the property</param>
        /// <param name="aChoices"></param>
        /// <returns></returns>
        public bool AddPartProperty(string aPropName, uopPart aPart, string aDisplyName = "", string aChoices = "")
        {
            try
            {

                aPropName = aPropName.Trim();
                if (aPart == null) return false;
                if (string.IsNullOrEmpty(aPropName)) return false;


                uopProperty aProp = aPart.CurrentProperty(aPropName, false);
                if (aProp == null) return false;

                if (aDisplyName !=  string.Empty) aProp.DisplayName = aDisplyName;
                Add(aProp.Structure);
                return true;
            }
            catch (Exception exception)
            {
                LoggerManager log = new LoggerManager();
                log.LogError(exception.Message);
                //Err.Raise Err.Number, Err.Source, Err.Description
            }
            return false;
        }

        /// <summary>
        /// shorthand method for adding a property to the collection
        /// won't add a property with no name (no error raised).
        /// </summary>
        /// <param name="aProps"></param>
        /// <param name="aName">the name of the property to add</param>
        /// <param name="aPropVal">the value to assign to the new property</param>
        /// <param name="aUnitType">the index of the units object to assign to the property</param>
        /// <param name="bIsHidden">flag to mark the new property as hidden</param>
        /// <param name="aHeading">the heading to assign to the new property</param>
        /// <param name="aDisplayName">the caption assigned to the property</param>
        /// <param name="aDecodeString"></param>
        /// <param name="bProtected"></param>
        /// <param name="aCategory"></param>
        /// <param name="aPartType"></param>
        /// <param name="bIsShared"></param>
        /// <param name="aPrecision"></param>
        /// <param name="aChoices"></param>
        /// <param name="aPreviousVal"></param>
        /// <returns></returns>
        public TPROPERTY Add(string aName, dynamic aPropVal,
                            uppUnitTypes aUnitType = uppUnitTypes.Undefined,
                            bool bIsHidden = false,
                            string aHeading = "",
                            string aDisplayName = "",
                            string aDecodeString = "",
                            bool bProtected = false,
                            string aCategory = "",
                            uppPartTypes aPartType = uppPartTypes.Undefined,
                            bool bIsShared = false, int aPrecision = 0,
                            string aChoice = "",
                            dynamic aPreviousVal = null,
                            dynamic aNullVal = null,
                            string aUnitCaption = null,
                            string aColor = null,
                            bool bSetDefault = false,
                            bool bSetNullValue = false,
                            bool bOptional = false)
        {

            if (!_Init) { Init(); }


            if (string.IsNullOrWhiteSpace(aName) || aPropVal == null) return new TPROPERTY("");
            TPROPERTY _rVal = new TPROPERTY()
            {
                Name = aName.Trim(),
                Heading = aHeading,
                Choices = aChoice?.Trim(),
                Units = new TUNIT(aUnitType),
                DisplayName = aDisplayName,
                IsShared = bIsShared,
                DecodeString = aDecodeString,
                Protected = bProtected,
                Category = aCategory,
                PartType = aPartType,
                Hidden = bIsHidden,
                Precision = aPrecision,
                Optional = bOptional
            };

            _rVal.SetValue(aPropVal);
            if (!string.IsNullOrWhiteSpace(aColor)) { _rVal.Color = aColor; }

            if (!string.IsNullOrWhiteSpace(aUnitCaption)) { _rVal.SetUnitCaption(aUnitCaption); }
            if (aNullVal != null) { _rVal.NullValue = aNullVal; }

            if (aPreviousVal != null) _rVal.LastValue = aPreviousVal;
            if (bSetDefault && _rVal.HasValue) _rVal.DefaultValue = _rVal.Value;
            if (bSetNullValue && _rVal.HasValue) _rVal.NullValue = _rVal.Value;


            _rVal = (Add(_rVal).Index > 0) ? Item(Count) : new TPROPERTY("");


            return _rVal;
        }



        /// <summary>
        /// shorthand method for adding a property to the collection
        /// </summary>
        /// <param name="aProps">the subject properties</param>
        /// <param name="aPropName">the name of the property to add</param>
        /// <param name="aValue">the value to assign to the new property</param>
        /// <param name="aUnits">the index of the units object to assign to the property</param>
        /// <param name="bProtected">flag to mark the new property as protected</param>
        /// <param name="NullValue">the value which is considered null for the property</param>
        /// <param name="aUnitString">unit string to assign to the property</param>
        /// <param name="aPartType"></param>
        /// <param name="aChoices"></param>
        /// <returns></returns>
        public TPROPERTY AddProp(string aName, dynamic aPropVal, uppUnitTypes aUnitType = uppUnitTypes.Undefined, bool bProtected = false, dynamic NullValue = null, string aUnitString = "", uppPartTypes aPartType = uppPartTypes.Undefined, string aChoice = "")
        {

            if (string.IsNullOrWhiteSpace(aName)) return new TPROPERTY("");

            if (aPropVal == null)
            {
                if (aUnitType <= uppUnitTypes.Undefined)
                { return new TPROPERTY(""); }
                else
                { aPropVal = 0; }
            }

            TPROPERTY _rVal = Add(aName: aName, aPropVal: aPropVal, aUnitType: aUnitType, bProtected: bProtected, aPartType: aPartType, aChoice: aChoice);

            if (_rVal.Index <= 0) return new TPROPERTY(aName);

            if (!string.IsNullOrWhiteSpace(aUnitString)) { _rVal.SetUnitCaption(aUnitString); }
            if (NullValue != null) { _rVal.NullValue = NullValue; }

            if (_rVal.Index > 0) { return Item(_rVal.Index); } else { return new TPROPERTY(_rVal.Name); }
        }

        /// <summary>
        /// #1a delimited string of property signatures
        /// #2the delimiter to look for
        /// Populates the collection with the properties defined in the passed string
        /// ~string like "Color=Red¸Count=240" etc.  Numerics are assumed to be doubles all others are strings.
        /// </summary>
        /// <param name="aPropString"></param>
        /// <param name="aDelimitor"></param>
        /// <param name="bClearExisting"></param>
        /// <returns></returns>
        public TPROPERTIES AddByString(string aPropString = "", string aDelimitor = "¸", bool bClearExisting = false)
        {
            TPROPERTIES _rVal = Clone(bClearExisting);

            int i = 0;
            string pStr = string.Empty;
            string pname = string.Empty;
            string pval = string.Empty;
            string[] pVals;
            string[] sVals = aPropString.Split(aDelimitor.ToCharArray());
            for (i = 0; i < sVals.Length; i++)
            {
                TPROPERTY aProp = TPROPERTY.Null;
                pStr = sVals[i];
                if (pStr.IndexOf("=") != -1)
                {
                    pVals = pStr.Split('=');
                    if (pVals.Length >= 1)
                    {
                        pname = pVals[0].Trim();
                        pval = pVals[1];
                        if (pname !=  string.Empty)
                        {
                            aProp.Name = pname;
                            aProp.SetValue(pval);
                            _rVal.Add(aProp);
                        }
                    }
                }
            }
            return _rVal;
        }


        /// <summary>
        /// Searchs this collection for properties that match the members of the passed collection.  If a match is found the value of the passed property is copied to the
        /// matching member of this collection.
        /// if there are members with the same name only the first member found receives the new value.
        /// If there is not a matching member for a member of the passed collection and the second argument is true
        /// a clone of the non-member property is added.
        /// </summary>
        /// <param name="aProps"></param>
        /// <param name="bProps"></param>
        /// <param name="bCopyNonMembers"></param>
        /// <param name="rChangeNames"></param>
        /// <returns></returns>
        public void CopyValues(TPROPERTIES bProps, out string rChangeNames, bool bCopyNonMembers = true, List<string> aSkipList = null)
        {

            rChangeNames = string.Empty;
            if (bProps.Count <= 0) return;
            aSkipList ??= new List<string>();
            try
            {
                TPROPERTY aProp;

                bool aFlag = false;
                for (int i = 1; i <= bProps.Count; i++)
                {
                    aProp = bProps.Item(i);
                    if (aSkipList.FindIndex((x) => string.Compare(x, aProp.Name, true) == 0) >= 0) continue;

                    if (TryGet(aProp.Name, out TPROPERTY myProp))
                    {
                        aFlag = myProp.SetValue(aProp.Value);
                        if (aFlag)
                        {
                            if (!string.IsNullOrEmpty(rChangeNames)) rChangeNames += ",";
                            SetItem(myProp.Index, myProp);
                            rChangeNames += aProp.Name;
                        }
                    }
                    else
                    {
                        if (bCopyNonMembers)
                        {
                            Add(aProp);
                            if (!string.IsNullOrEmpty(rChangeNames)) rChangeNames += ",";

                            rChangeNames += aProp.Name;

                        }
                    }
                }
            }
            catch (Exception exception)
            {
                LoggerManager log = new LoggerManager();
                log.LogError(exception.Message);
            }
        }


        /// <summary>
        /// Searchs this collection for properties that match the members of the passed collection.  If a match is found the value of the passed property is copied to the
        /// matching member of this collection.
        /// if there are members with the same name only the first member found receives the new value.
        /// If there is not a matching member for a member of the passed collection and the second argument is true
        /// a clone of the non-member property is added.
        /// </summary>
        /// <param name="aProps"></param>
        /// <param name="bProps"></param>
        /// <param name="bCopyNonMembers"></param>
        /// <param name="rChangeNames"></param>
        /// <returns></returns>
        public void CopyValues(uopProperties bProps, out string rChangeNames, bool bCopyNonMembers = true, List<string> aSkipList = null)
        {

            rChangeNames = string.Empty;
            if (bProps.Count <= 0) return;
            aSkipList ??= new List<string>();
            try
            {
                TPROPERTY aProp;

                bool aFlag = false;
                for (int i = 1; i <= bProps.Count; i++)
                {
                    aProp = new TPROPERTY(bProps.Item(i));
                    if (aSkipList.FindIndex((x) => string.Compare(x, aProp.Name, true) == 0) >= 0) continue;

                    if (TryGet(aProp.Name, out TPROPERTY myProp))
                    {
                        aFlag = myProp.SetValue(aProp.Value);
                        if (aFlag)
                        {
                            if (!string.IsNullOrEmpty(rChangeNames)) rChangeNames += ",";
                            SetItem(myProp.Index, myProp);
                            rChangeNames += aProp.Name;
                        }
                    }
                    else
                    {
                        if (bCopyNonMembers)
                        {
                            Add(aProp);
                            if (!string.IsNullOrEmpty(rChangeNames)) rChangeNames += ",";

                            rChangeNames += aProp.Name;

                        }
                    }
                }
            }
            catch (Exception exception)
            {
                LoggerManager log = new LoggerManager();
                log.LogError(exception.Message);
            }
        }

        public void ConvertUnits(uppUnitFamilies aFromUnits, uppUnitFamilies aToUnits)
        {

            TPROPERTY prop;

            for (int i = 1; i <= Count; i++)
            {
                prop = Item(i);
                prop.ConvertUnits(aFromUnits, aToUnits);

                SetItem(i, prop);
            }

        }

        public void CopyShared(TPROPERTIES bProps)
        {

            TPROPERTY bProp;
            for (int i = 1; i <= bProps.Count; i++)
            {
                bProp = bProps.Item(i);
                if (bProp.IsShared)
                {

                    if (TryGet(bProp.Name, out TPROPERTY aProp))
                    {

                        aProp.LastValue = aProp.Value;
                        aProp.Value = bProp.Value;

                        SetItem(aProp.Index, aProp);
                    }
                }
            }

        }

        public void RemoveLast()
        {
            if (Count <= 0) return;
            _Count -= 1;

            Array.Resize<TPROPERTY>(ref _Members, _Count);
        }

        public bool AddUnique(TPROPERTY aProp, bool bDontCheck = false, dynamic aCategory = null, dynamic aHeading = null)
        {

            aProp.Name = aProp.Name.Trim();
            if (aProp.Name ==  string.Empty) { aProp.Name = "Prop" + Count + 1; }

            if (aCategory != null) { aProp.Category = Convert.ToString(aCategory); }

            if (aHeading != null) { aProp.Heading = Convert.ToString(aHeading); }

            if (TryGet(aProp.Name, out TPROPERTY prop))
            {
                bool _rVal = string.Compare(prop.Value, aProp.Value, true) != 0;

                prop.Value = aProp.Value;
                prop.Category = aProp.Category;
                prop.Heading = aProp.Heading;
                SetItem(prop.Index, prop);
                return _rVal;
            }
            else
            {
                bool _rVal = true;
                Add(aProp);
                aProp.Index = Count;
                return _rVal;
            }

        }

        public TPROPERTY Add(TPROPERTY aProp, string aName = null, string aCategory = null, string aHeading = null, string aColor = null)
        {

            if (!string.IsNullOrWhiteSpace(aName)) aProp.Name = aName.Trim();
            if (!string.IsNullOrWhiteSpace(aCategory)) { aProp.Category = aCategory; }
            if (!string.IsNullOrWhiteSpace(aHeading)) { aProp.Heading = aHeading; }
            if (!string.IsNullOrWhiteSpace(aColor)) { aProp.Color = aColor; }

            return Add(aProp);

        }

        public TPROPERTY Add(TPROPERTY aProp)
        {
            if (string.IsNullOrWhiteSpace(aProp.Name)) return new TPROPERTY("");
            int cnt = Count;
            cnt += 1;
            _Count = cnt;
            Array.Resize<TPROPERTY>(ref _Members, cnt);
            _Members[_Count - 1] = aProp;
            return Item(Count);
        }

        public Boolean Contains(string aName)
        {

            if (string.IsNullOrWhiteSpace(aName)) return false;
            if (Count <= 0) return false;

            TPROPERTY aMem;
            aName = aName.Trim();

            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                if (string.Compare(aMem.Name, aName, true) == 0) return true;
            }
            return false;
        }


        public bool TryGet(string aName, out TPROPERTY rMember, int aOccur = 0)
        {
            rMember = TPROPERTY.Null;

            if (string.IsNullOrWhiteSpace(aName) || Count <= 0) return false;

            
            int cnt = 0;
            if (aOccur <= 0)  aOccur = 1; 
            aName = aName.Trim();

            for (int i = 1; i <= Count; i++)
            {
                _Members[i - 1].Index = i;
                if (string.Compare(_Members[i - 1].Name, aName, true) == 0)
                {
                    cnt += 1;
                    if (cnt == aOccur)
                    {
                        _Members[i - 1].SubPart(this);
                       rMember = _Members[i - 1];

                        return true;

                    }
                }
            }
            return false;
        }

        public Boolean TryGet(dynamic aNameOrIndex, out TPROPERTY rMember, int aOccur = 0)
        {
            rMember = TPROPERTY.Null;

            if (Count <= 0) { return false; }


            if (aNameOrIndex.GetType() == typeof(string))
            {
                return TryGet(Convert.ToString(aNameOrIndex), out rMember, aOccur);

            }
            else
            {
                if (!int.TryParse(aNameOrIndex.ToString(), out int idx)) return false;
                if (idx <= 0 || idx > Count) return false;
                rMember = Item(idx);
                rMember.Index = idx;
                return true;
            }


        }

        /// <summary>
        ///returns the values of properties in the collection in a comma (or other deliminator) string
        /// </summary>
        /// <param name="aProps">the subject properties</param>
        /// <param name="aDelimitor">an optional delimator</param>
        /// <param name="aStartIndex">the index to begin the procedure (default = 1)</param>
        /// <param name="aEndIndex">the index to end the procedure (default = count)</param>
        /// <param name="bIncludePropertyNames">flag to return the properties complete signatures in the returned string</param>
        /// <param name="aWrapper">optional string to put before anf after the values in the returned string</param>
        /// <param name="bShowDecodedValue"></param>
        /// <returns></returns>
        public string DeliminatedString(string aDelimitor = ",", int aStartIndex = 0, int aEndIndex = 0, bool bIncludePropertyNames = false, string aWrapper = "", bool bShowDecodedValue = false)
        {
            string _rVal = string.Empty;
            if (Count <= 0) return _rVal;

            mzUtils.LoopLimits(aStartIndex, aEndIndex, 1, Count, out int si, out int ei);

            TPROPERTY aProp;
            string dlm = string.Empty;
            string sVal;


            dlm = aDelimitor.Trim();
            if (dlm ==  string.Empty) dlm = ",";

            for (int i = si; i <= ei; i++)
            {
                aProp = Item(i);

                sVal = bShowDecodedValue ? aProp.DecodedValue : aProp.ValueS;

                if (!bIncludePropertyNames)
                {
                    if (aWrapper ==  string.Empty)
                    {
                        if (sVal.IndexOf(aDelimitor, StringComparison.OrdinalIgnoreCase) != -1)
                        {
                            sVal = (char)34 + sVal + (char)34;
                        }
                        _rVal += sVal;
                    }
                    else
                    {
                        _rVal = _rVal + aWrapper + sVal + aWrapper;
                    }
                }
                else
                {
                    _rVal = _rVal + aWrapper + aProp.Name + "=" + sVal + aWrapper;
                }
                if (i < ei) _rVal += dlm;


            }

            return _rVal;
        }

        public List<TPROPERTY> ToList
        {
            get
            {
                if (!_Init) { Init(); }
                return new List<TPROPERTY>(_Members);


            }
        }
        public List<uopProperty> ToPropertyList
        {
            get
            {
                if (!_Init) { Init(); }
                List<uopProperty> _rVal = new List<uopProperty>();
                for (int i = 1; i <= Count; i++) { _rVal.Add(new uopProperty(Item(i))); }
                return _rVal;


            }
        }
        public int Append(TPROPERTIES aProperties)
        {
            int _rVal = 0;
            for (int i = 1; i <= aProperties.Count; i++)
            {
                if (Add(new TPROPERTY(aProperties.Item(i))).Index > 0) { _rVal += 1; }
            }
            return _rVal;
        }


        /// <summary>
        ///returns the value of the item from the collection at the requested index or the passed name
        /// </summary>
        /// <param name="aNameOrIndex"></param>
        /// <param name="aDefault"></param>
        /// <returns></returns>
        public dynamic Value(dynamic aNameOrIndex, dynamic aDefault = null)
        {

            return TryGet(aNameOrIndex, out TPROPERTY mem) ? mem.Value : (dynamic)aDefault;
        }
        /// <summary>
        ///returns the 'double' value of the item from the collection at the requested index or the passed name
        /// </summary>
        /// <param name="aNameOrIndex"></param>
        /// <param name="aDefault"></param>
        /// <param name="aPrecis"></param>
        /// <returns></returns>

        public double UnitValue(dynamic aNameOrIndex, bool bReturnMetric, int aPrecis = -1)
        {

            double _rVal = TryGet(aNameOrIndex, out TPROPERTY mem) ? mem.ValueD : 0;
            if (!mem.Units.IsDefined) return _rVal;
            if (bReturnMetric)
            {
                _rVal = mem.UnitValue(uppUnitFamilies.Metric);
                if (aPrecis < 0) aPrecis = uopUnits.UnitPrecision(mem.UnitType, uppUnitFamilies.Metric);

            }
            else
            {
                mem.UnitValue(uppUnitFamilies.English);
                if (aPrecis < 0) aPrecis = uopUnits.UnitPrecision(mem.UnitType, uppUnitFamilies.English);
            }


            if (aPrecis >= 0) _rVal = Math.Round(_rVal, mzUtils.LimitedValue(aPrecis, 0, 15));
            return _rVal;
        }

        public bool SetUnitValue(dynamic aNameOrIndex, double aValue, bool bMetricPassed)
        {

            if (!TryGet(aNameOrIndex, out TPROPERTY mem)) return false;
            double multi = 1;
            if (mem.Units.IsDefined)
            {
                if (bMetricPassed)
                {
                    multi = uopUnits.UnitFactor(mem.Units.UnitType, uppUnitFamilies.Metric);
                    if (multi != 0) multi = 1 / multi;
                }
            }
            bool _rVal = mem.SetValue(aValue * multi);
            if (_rVal == true) SetItem(mem.Index, mem);
            return _rVal;

        }

        public double ValueD(dynamic aNameOrIndex, double aDefault = 0, dynamic aMultiplier = null, int aPrecis = -1) //, int aPrecis = -1)
        {

            double _rVal = TryGet(aNameOrIndex, out TPROPERTY mem) ? mem.ValueD : aDefault;
            if (aMultiplier != null && aMultiplier != 0) _rVal *= mzUtils.VarToDouble(aMultiplier);
            if (aPrecis >= 0) _rVal = Math.Round(_rVal, mzUtils.LimitedValue(aPrecis, 0, 15));
            return _rVal;
        }
        /// <summary>
        ///returns the 'integer' value of the item from the collection at the requested index or the passed name
        /// </summary>
        /// <param name="aNameOrIndex"></param>
        /// <param name="aDefault"></param>
        /// <returns></returns>

        public int ValueI(dynamic aNameOrIndex, int aDefault = 0)
        {

            return TryGet(aNameOrIndex, out TPROPERTY mem) ? mem.ValueI : aDefault;
        }

        /// <summary>
        ///returns the 'bool' value of the item from the collection at the requested index or the passed name
        /// </summary>
        /// <param name="aNameOrIndex"></param>
        /// <param name="aDefault"></param>
        /// <returns></returns>
        public bool ValueB(dynamic aNameOrIndex, bool aDefault = false)
        {

            return TryGet(aNameOrIndex, out TPROPERTY mem) ? mem.ValueB : aDefault;
        }

        /// <summary>
        ///returns the 'string' value of the item from the collection at the requested index or the passed name
        /// </summary>
        /// <param name="aNameOrIndex"></param>
        /// <param name="aDefault"></param>
        /// <returns></returns>
        public string ValueS(dynamic aNameOrIndex, string aDefault = "", bool formatted = false)
        {

            if (!TryGet(aNameOrIndex, out TPROPERTY mem)) return aDefault;

            return (!formatted) ? mem.ValueS : mem.FormatedString;


        }

        /// <summary>
        ///returns the first property in the collection whose part path matches the passed string
        /// </summary>
        /// <param name="aProps"></param>
        /// <param name="aNodePath"></param>
        /// <param name="aIndex"></param>
        /// <returns></returns>
        public TPROPERTY GetByPartPath(string aNodePath)
        {
            TPROPERTY aProp;
            for (int i = 1; i <= Count; i++)
            {
                aProp = Item(i);
                if (string.Compare(aProp.PartPath, aNodePath, true) == 0) return aProp;
            }

            return new TPROPERTY("");
        }



        /// <summary>
        ///returns all the properties in the collection that have a handle matching the search value
        /// </summary>
        /// <param name="aProps">the property handle to search for</param>
        /// <param name="aCategory"></param>
        /// <param name="bVisibleOnly">flag to only return the visible ones</param>
        /// <param name="bRemove">flag to remove the matches from the passed properties</param>
        /// <returns></returns>
        public TPROPERTIES GetByCategory(dynamic aCategory, bool bVisibleOnly = false, bool bRemove = false)
        {
            TPROPERTIES _rVal = Clone(true);

            TPROPERTY aMem;

            string IDXS = string.Empty;
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                if (string.Compare(aMem.Category, aCategory, ignoreCase: true) == 0)
                {
                    if (!bVisibleOnly || (bVisibleOnly && !aMem.Hidden))
                    {
                        _rVal.Add(aMem);
                        if (bRemove) { mzUtils.ListAdd(ref IDXS, i); }
                    }
                }
            }
            if (bRemove && IDXS !=  string.Empty)
            {
                _rVal = RemoveByIndices(IDXS);
            }
            return _rVal;
        }

        /// <summary>
        ///returns all the properties in the collection that have a handle matching the search value
        /// </summary>
        /// <param name="aProps">1the property handle to search for</param>
        /// <param name="aCategory">flag to only return the visible ones</param>
        /// <param name="bVisibleOnly">flag to remove the matches from the passed properties</param>
        /// <returns></returns>
        public TPROPERTIES RemoveByCategory(string aCategory, bool bVisibleOnly = false)
        {
            TPROPERTIES _rVal = Clone(true);
            TPROPERTY aMem;
            string IDXS = string.Empty;
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                if (string.Compare(aMem.Category, aCategory, ignoreCase: true) == 0)
                {
                    if (!bVisibleOnly || (bVisibleOnly && !aMem.Hidden))
                    {
                        _rVal.Add(aMem);
                        mzUtils.ListAdd(ref IDXS, i);
                    }
                }
            }
            if (IDXS !=  string.Empty)
            {
                RemoveByIndices(IDXS);
            }

            return _rVal;
        }

        public TPROPERTIES RemoveByIndices(string aIndices)
        {
            TPROPERTIES _rVal = Clone(true);


            int j = 0;
            _rVal.Clear();
            TPROPERTY[] newMems = new TPROPERTY[0];

            bool bKeep = false;


            for (int i = 1; i <= Count; i++)
            {
                bKeep = !mzUtils.ListContains(i, aIndices);
                if (!bKeep)
                {
                    j += 1;
                    Array.Resize<TPROPERTY>(ref newMems, j);
                    newMems[j - 1] = Item(i);
                    newMems[j - 1].Index = j;
                }
                else
                {
                    _rVal.Add(new TPROPERTY(Item(i)));

                }

            }
            _Count = j;
            _Members = newMems;
            return _rVal;
        }


        /// <summary>
        ///returns the properties from the collection that have a heading property matching the passed string
        /// search is not case sensitive
        /// </summary>
        /// <param name="aProps">the subject properties</param>
        /// <param name="aHeading">the heading to search form</param>
        /// <returns></returns>
        public TPROPERTIES GetByHeading(string aHeading)
        {
            TPROPERTIES _rVal = Clone(true);
            TPROPERTY aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                if (string.Compare(aMem.Heading, aHeading, true) == 0)
                { _rVal.Add(aMem); }
            }
            return _rVal;
        }


        public TPROPERTIES RemoveByPartType(uppPartTypes aPartType, uppDocumentTypes aDocumentType)
        {

            TPROPERTY aMem;
            bool bKeep = false;
            string IDXS = string.Empty;


            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                bKeep = true;
                if (aMem.PartType == aPartType)
                {
                    if (aDocumentType == uppDocumentTypes.Undefined || aMem.DocumentType == aDocumentType)
                    { bKeep = false; }
                }
                if (!bKeep) { mzUtils.ListAdd(ref IDXS, i); }

            }

            return RemoveByIndices(IDXS);
        }

        public TPROPERTIES RemoveRange(int aStartID, int aEndID)
        {

            int aSID = aStartID;
            int aEID = aEndID;
            string IDXS = string.Empty;


            mzUtils.SortTwoValues(true, ref aSID, ref aEID);
            for (int i = 1; i <= Count; i++)
            {
                if (i >= aSID && i <= aEID) { mzUtils.ListAdd(ref IDXS, i); }
            }
            return RemoveByIndices(IDXS);
        }



        public TPROPERTIES GetByHidden(bool bHiddenVal)
        {
            TPROPERTIES _rVal = Clone(true);

            TPROPERTY aProp;

            for (int i = 1; i <= Count; i++)
            {
                aProp = Item(i);
                if (aProp.Hidden == bHiddenVal) { _rVal.Add(aProp); }
            }
            return _rVal;
        }




        public TPROPERTIES GetMembers(string aNamesList, string aDelimitor = ",")
        {
            TPROPERTIES _rVal = Clone(true);
            TVALUES nms = TVALUES.FromDelimitedList(aNamesList, aDelimitor: aDelimitor, false, true, true, bRemoveParens: true);
            TPROPERTY aMem = TPROPERTY.Null;
            string sname = string.Empty;
            for (int i = 1; i <= nms.Count; i++)
            {
                sname = Convert.ToString(nms.Item(i));
                if (TryGet(sname, out aMem)) _rVal.Add(new TPROPERTY(aMem));
            }

            return _rVal;
        }

        /// <summary>
        ///returns the properties from the collection that have a value matching the passed 
        /// </summary>
        /// <param name="aProps">the subject properties</param>
        /// <param name="aValue">the value to search form</param>
        /// <returns></returns>
        public TPROPERTIES GetByValue(dynamic aValue)
        {
            TPROPERTIES _rVal = Clone(true);

            TPROPERTY aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                if (aMem.Value == aValue)
                { _rVal.Add(aMem); }
            }
            return _rVal;
        }

        /// <summary>
        ///returns the properties from the collection that have a ValueChanged value matching the passed value
        /// </summary>
        /// <param name="aValue">the value to search form</param>
        /// <returns></returns>
        public TPROPERTIES GetByValueChange(bool aValue)
        {
            TPROPERTIES _rVal = Clone(true);

            TPROPERTY aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                if (aMem.ValueChanged == aValue) _rVal.Add(aMem);
            }
            return _rVal;
        }

        public TPROPERTIES GetByProtected(bool bProtectedVal)
        {
            TPROPERTIES _rVal = Clone(true);
            TPROPERTY aProp;

            for (int i = 1; i <= Count; i++)
            {
                aProp = Item(i);
                if (aProp.Protected == bProtectedVal)
                { _rVal.Add(aProp); }
            }
            return _rVal;
        }


        public void PrintToConsole(string aHeading = null, bool bIndexed = false, bool bPrintHeadings = false)
        {

            string outs = string.Empty;
            string cHeading = string.Empty;
            string hd1 = string.Empty;

            if (!string.IsNullOrWhiteSpace(aHeading)) System.Diagnostics.Debug.WriteLine(aHeading);
            TPROPERTY mem;
            for (int i = 1; i <= Count; i++)
            {
                mem = Item(i);
                if (bPrintHeadings)
                {

                    hd1 = mem.Heading.Trim();
                    if (hd1 !=  string.Empty)
                    {
                        if (i == 1)
                        {
                            cHeading = hd1;
                            System.Diagnostics.Debug.WriteLine($"[{ cHeading }]");
                        }
                        else
                        {
                            if (hd1 != cHeading)
                            {
                                cHeading = hd1;
                                System.Diagnostics.Debug.WriteLine($"[{ cHeading }]");
                            }

                        }
                    }
                }

                outs = bIndexed ? i + " - " + mem.ToString() : mem.ToString();
                System.Diagnostics.Debug.WriteLine(outs);
            }
        }


        public int ReadFromINIFile(string aFileSpec, string aFileSection, bool bRaiseNotFoundError = false)
        {
            int _rVal = 0;
            try
            {
                if (string.IsNullOrWhiteSpace(aFileSpec)) { throw new Exception("The Passed File Name Is Invalid"); }
                if (aFileSection ==  string.Empty) { throw new Exception("The Passed FileSectionIs Invalid"); }
                if (!System.IO.File.Exists(aFileSpec)) { throw new Exception("file Not Found '" + aFileSpec + "'"); }

                int i = 0;
                TPROPERTY aMem;
                TPROPERTY aReadProp;
                for (i = 1; i <= Count; i++)
                {
                    aMem = Item(i);
                    aReadProp = aMem;
                    if (aReadProp.ReadFromINIFile(aFileSpec, aFileSection, false))
                    {
                        _rVal += 1;
                        aMem.SetValue(aReadProp.Value); SetItem(i, aMem);
                    }

                    else
                    {
                        if (bRaiseNotFoundError) { throw new Exception("'" + aMem + "' Not Found in Section - '" + aFileSection + "'"); }
                    }

                }
                return _rVal;
            }
            catch (Exception exception)
            {
                LoggerManager log = new LoggerManager();
                log.LogError(exception.Message);
                //Err.Raise(Err().Number, "props_ReadFromFile", sErr);
                return _rVal;
            }
        }

        public List<string> Names
        {
            get
            {
                List<string> _rVal = new List<string>();
                for (int i = 1; i <= Count; i++)
                {
                    _rVal.Add(Item(i).Name);

                }
                return _rVal;
            }

        }

        /// <summary>
        ///returns all the names of all the properties
        /// </summary>
        /// <param name="bUnique"></param>
        /// <param name="aDelimiter"></param>
        /// <returns></returns>
        public string NameList(bool bUnique = false, string aDelimitor = ",")
        {
            string _rVal = string.Empty;
            for (int i = 1; i <= Count; i++)
            {
                mzUtils.ListAdd(ref _rVal, Item(i).Name, "", bSuppressTest: !bUnique, aDelimitor: aDelimitor);
            }
            return _rVal;
        }


        /// <summary>
        ///sets all the memebrs ProjectHandle to the passed value
        /// </summary>
        /// <param name="aProps"></param>
        /// <param name="aHandle"></param>
        /// <returns></returns>
        public int SetProjectHandle(string aHandle)
        {
            int _rVal = 0;
            ProjectHandle = aHandle;
            for (int i = 1; i <= Count; i++)
            {
                TPROPERTY prop = Item(i);
                if (string.Compare(prop.ProjectHandle, aHandle, ignoreCase: true) != 0) { _rVal += 1; }
                prop.ProjectHandle = aHandle;
                SetItem(i, prop);
            }

            return _rVal;
        }

        public TPROPERTIES RemoveByName(string aNamesList, char aDelimitor = ',')
        {
            TPROPERTIES _rVal = Clone(bReturnEmpty: true);

            if (string.IsNullOrWhiteSpace(aNamesList) || Count <= 0) return _rVal;
            string[] pNames = aNamesList.Split(aDelimitor);
            TPROPERTY mem;
            bool keep = true;
            int cnt = 0;
            TPROPERTY[] newMembers = new TPROPERTY[0];
            for (int j = 1; j <= Count; j++)
            {
                mem = Item(j);
                keep = true;
                for (int i = 0; i < pNames.Length; i++)
                {
                    if (string.Compare(pNames[i], mem.Name, ignoreCase: true) == 0)
                    {
                        keep = false;
                        break;
                    }
                }

                if (!keep)
                {
                    _rVal.Add(mem);
                }
                else
                {
                    cnt++;
                    Array.Resize<TPROPERTY>(ref newMembers, cnt);
                    newMembers[cnt - 1] = mem;
                }
            }
            _Members = newMembers;
            _Count = cnt;

            return _rVal;
        }

        public int Remove(dynamic aNameOrIndex)
        {

            TPROPERTIES newMems = Clone(false);
            newMems = TPROPERTIES.Removed(this, aNameOrIndex, out int _rVal);

            if (_rVal <= 0) return 0;

            _Members = newMems._Members;
            _Count = newMems._Count;

            return _rVal;
        }
        /// <summary>
        ///returns all the properties from the collection whose values are not currently equal to their set default  values
        /// </summary>
        /// <param name="aStatus">the default status to search for</param>
        /// <returns></returns>
        public TPROPERTIES GetByDefaultStatus(bool aStatus, string aNameList = "", TPROPERTY? aEmptyProp = null, int aStartIndex = 0, int aEndIndex = 0)
         => GetByDefaultStatus(aStatus, aStartIndex, aEndIndex, aNameList, aEmptyProp: aEmptyProp, out string NMS);
        

        /// <summary>
        ///returns all the properties from the collection whose values are not currently equal to their set default  values
        /// </summary>
        /// <param name="aStatus">the default status to search for</param>
        /// <param name="aStartIndex"></param>
        /// <param name="aEndIndex"></param>
        /// <param name="aNameList"></param>
        /// <param name="rNameList"></param>
        /// <returns></returns>
        public TPROPERTIES GetByDefaultStatus(bool aStatus, int aStartIndex, int aEndIndex, string aNameList, TPROPERTY? aEmptyProp, out string rNames)
        {
            TPROPERTIES _rVal =  Clone(true);
            rNames = string.Empty;
            if (Count <= 0) return _rVal;
            bool testnames = !string.IsNullOrWhiteSpace(aNameList);

            List<string> searchlist =   testnames ? mzUtils.StringsFromDelimitedList(aNameList) : new List<string>();
            aNameList = aNameList.Trim();
            if (testnames) { aNameList = "," + aNameList + ","; }

            mzUtils.LoopLimits(aStartIndex, aEndIndex, 1, Count, out int si, out int ei);

            for (int i = si; i <= ei; i++)
            {
                TPROPERTY aMem = Item(i);
               bool keep = false;
                if (!testnames)
                {
                    keep = aMem.IsDefault == aStatus;
                }
                else
                {
                    if (searchlist.FindIndex((x) => string.Compare(x , aMem.Name, true) == 0) >=0 )
                    {
                        keep = aMem.IsDefault == aStatus;
                    }
                }

                if (keep)
                {
                    _rVal.Add(aMem);
                    if (rNames != string.Empty)  rNames += ","; 
                    rNames += aMem.Name;
                }
            }
            if (_rVal.Count <= 0 && aEmptyProp.HasValue) _rVal.Add(aEmptyProp.Value);
            return _rVal;
        }


        public void SetMinMax(dynamic aNameOrIndex, dynamic aMin = null, dynamic aMax = null, dynamic aDefault = null, bool bSetToDefault = false, bool bApplyLimits = false, dynamic aIncrement = null)
        {

            if (!TryGet(aNameOrIndex, out TPROPERTY aProp)) { return; }

            TPROPERTY bProp = aProp;
            double? min = null;
            double? max = null;
            if (aMin != null) min =  mzUtils.VarToDouble(aMin );
            if (aMax != null) max = mzUtils.VarToDouble(aMax);
          
            if(min.HasValue && max.HasValue)
            {
                double d1 = min.Value;
                double d2 = max.Value;
                if(d1 > d2)
                {
                    min = d2;
                    max = d1;
                }
            }

            if (min.HasValue)  bProp.MinValue = min.Value; 
            if (max.HasValue)  bProp.MaxValue = max.Value; 
            if (aDefault != null)  bProp.DefaultValue = aDefault; 
            if (aIncrement != null) bProp.Increment = aIncrement;   
            if (bSetToDefault)
            {
                if (bProp.DefaultValue != null)  bProp.SetValue(bProp.DefaultValue); 
            }
            if ((bProp.Value != aProp.DefaultValue) && aProp.DefaultValue != null)
            {
                if (bApplyLimits)
                {
                    if (bProp.MinValue != null)
                    {
                        double d1 = mzUtils.VarToDouble(bProp.MinValue);
                        if (bProp.Value != null)
                        {
                            double d2 = mzUtils.VarToDouble(bProp.Value);
                            if (d2 < d1)
                            {
                                bProp.SetValue(bProp.MinValue);
                            }
                        }
                    }
                    if (bProp.MaxValue != null)
                    {
                        double d1 = mzUtils.VarToDouble(bProp.MaxValue);
                        if (bProp.Value != null)
                        {
                            double d2 = mzUtils.VarToDouble(bProp.Value);
                            if (d2 > d1)
                            {
                                bProp.SetValue(bProp.MaxValue);
                            }
                    }
                    }
                }

                SetItem(aProp.Index, bProp);
            }
        }

        /// <summary>
        ///returns all the unit captions of of all the properties
        /// </summary>
        /// <param name="aProps"></param>
        /// <param name="aUnits"></param>
        /// <returns></returns>
        public List<string> UnitCaptions(uppUnitFamilies aUnits = uppUnitFamilies.Default)
        {
            List<string> _rVal = new List<string>();
            for (int i = 1; i <= Count; i++)
            {
                _rVal.Add(Item(i).UnitCaption(aUnits));
            }
            return _rVal;
        }

        /// <summary>
        /// sets the value of the indicated members to the new value is it's current value is equal to the passed value
        /// returns true if the value changes
        /// </summary>
        /// <param name="aNameOrIndex">the name or index of the subject propert</param>
        /// <param name="aValue">test value</param>
        /// <param name="aNewValue">the new value</param>
        /// <param name="bStringCompare">flag to test by a non-casesensitive string comparison</param>
        /// <param name="aPrecis">numerical precisions to apply to</param>
        /// <param name="aElseValue">value to apply if the current value does NOT match the test value</param>
        /// <param name="rIndex"></param>
        /// <returns></returns>
        public bool ThisThenThis(dynamic aNameOrIndex, dynamic aValue, dynamic aNewValue, bool bStringCompare, int aPrecis, dynamic aElseValue, out int rIndex)
        {
            bool _rVal = false;
            rIndex = 0;
            if (Count <= 0) { return false; }

            if (!TryGet(aNameOrIndex, out TPROPERTY aProp)) { return false; }
            rIndex = aProp.Index;
            if (aProp.ThisThenThis(aValue, aNewValue, bStringCompare, aPrecis, aElseValue))
            {
                _rVal = true;
                SetItem(rIndex, aProp);
            }
            return _rVal;
        }


        public bool SortByName(bool bReverseOrder = false)
        {
            bool _rVal = false;
            if (Count <= 1)
            {
                return _rVal;
            }
            int i = 0;
            int idx = 0;
            List<dynamic> aNames = new List<dynamic> { };
            List<int> IDXS = new List<int> { };
            TPROPERTY[] newProps = new TPROPERTY[_Count];

            TPROPERTIES bProps = Clone(false);
            for (i = 1; i <= Count; i++)
            {
                aNames.Add(Item(i).Name);
            }
            IDXS = mzUtils.SortDynamicList(aNames, bReverseOrder, bBaseOne: true);

            for (i = Count; i <= 1; i--)
            {
                if (IDXS[i - 1] != i) _rVal = true;

                idx = IDXS[i - 1];
                newProps[idx - 1] = Item(idx);

            }

            _Members = newProps;
            return _rVal;
        }
        public TPROPERTIES SubSet(int aStartID, int aEndID)
        {
            TPROPERTIES _rVal = Clone(true);
            int aSID = aStartID;
            int aEID = aEndID;
            int idx = 0;
            mzUtils.SortTwoValues(true, ref aSID, ref aEID);
            for (int i = 1; i <= Count; i++)
            {
                idx = i + 1;
                if (idx >= aSID && idx <= aEID)
                { _rVal.Add(Item(i)); }
            }
            return _rVal;
        }

        public void SubPart(uopPart aPart)
        {
            if (aPart == null) return;
            ProjectHandle = aPart.ProjectHandle;
            _RangeGUID = aPart.RangeGUID;
            PartType = aPart.PartType;
            _PartName = aPart.PartName;
            _PartPath = aPart.GetPartPath();
            _PartIndex = aPart.Index;
            
         
        }




        /// </summary>
        /// eturns the values of properties in the collection in a comma (or other deliminator) string
        /// <param name="aDeliminator">an optional delimator</param>
        /// <param name="aStartIndex">the index to begin the procedure (default = 1)</param>
        /// <param name="aEndIndex">the index to end the procedure (default = count)</param>
        /// <param name="bIncludePropertyNames">flag to return the properties complete signatures in the returned string</param>
        /// <param name="aWrapper">an optional string to put before anf after the values in the returned string </param>
        /// <param name="bShowDecodedValue"></param>
        /// <returns></returns>
        public string StringVals(string aDeliminator = ",", int aStartIndex = 0, int aEndIndex = 0, bool bIncludePropertyNames = false, string aWrapper = "", bool bShowDecodedValue = false)
        {
            string _rVal = string.Empty;
            if (Count <= 0) return _rVal;


            mzUtils.LoopLimits(aStartIndex, aEndIndex, 1, Count, out int si, out int ei);


            TPROPERTY aProp;

            string dlm = string.Empty;
            dynamic aVal = null;
            string sVal = string.Empty;
            dlm = aDeliminator.Trim();
            if (dlm ==  string.Empty) dlm = ",";

            for (int i = si; i <= ei; i++)
            {
                aProp = Item(i);
                aVal = aProp.Value;
                sVal = bShowDecodedValue ? aProp.DecodedValue : Convert.ToString(aVal);

                if (bIncludePropertyNames) sVal = aProp.Name + "=" + sVal;

                if (aWrapper !=  string.Empty)
                {
                    if (sVal.IndexOf(aWrapper, StringComparison.OrdinalIgnoreCase) != -1)
                    {
                        sVal = sVal.Replace(aWrapper, "");
                    }
                    sVal = aWrapper + sVal + aWrapper;
                }
                mzUtils.ListAdd(ref _rVal, sVal, dlm, true, ",", bAllowNulls: true);
            }

            return _rVal;
        }

        #region Shared Methods


        public static TPROPERTIES Removed(TPROPERTIES aProps, dynamic aNameOrIndex, out int aIndex)
        {
            TPROPERTIES _rVal = (TPROPERTIES)aProps.Clone(bReturnEmpty: true);

            aIndex = 0;
            if (!aProps.TryGet(aNameOrIndex, out TPROPERTY prop) || aProps.Count <= 0) { return _rVal; }
            aIndex = prop.Index;
            int cnt = aProps.Count;

            for (int i = 1; i <= cnt; i++)
            {
                if (i != aIndex) _rVal.Add(aProps.Item(i));
            }
            return _rVal;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aProps">the subject properties</param>
        /// <param name="aFileSpec">the file name to create</param>
        /// <param name="bSuppressHiddenProperties">causes any property marked as hidden to be ommited from the file</param>
        /// <param name="aDefaultHeading">optional default section heading text to write properties without headings to</param>
        /// <param name="bForceToDefault">flag to force all properties to be written to the default heading</param>
        /// <param name="bSuppressErrors">writes the collection of properties to an INI file formated text file</param>
        public static void WriteToINIFile(TPROPERTIES aProps, string aFileSpec, bool bSuppressHiddenProperties = true, string aDefaultHeading = "DATA", bool bForceToDefault = false, bool bSuppressErrors = false, bool bEnumsToInts = true)
        {
            TPROPERTY aMem;
            string dhead = string.Empty;
            string phead = string.Empty;
            int i = 0;
            dynamic pval;

            bool doneit = false;
            dhead = aDefaultHeading.Trim();
            if (dhead ==  string.Empty)
            {
                dhead = "DATA";
            }
            int z = 0;
            for (i = 1; i <= aProps.Count; i++)
            {
                aMem = aProps.Item(i);
                pval = aMem.Value;

                if (!bSuppressHiddenProperties || (bSuppressHiddenProperties && !aMem.Hidden))
                {
                    if (!bForceToDefault)
                    {
                        phead = aMem.Heading;
                        if (phead ==  string.Empty)
                        {
                            phead = dhead;
                        }
                        phead = phead.ToUpper();
                    }
                    else
                    {
                        phead = dhead;
                    }
                    if (pval is Enum && bEnumsToInts)
                    {
                        pval = (int)aMem.Value;
                    }


                    string value = aMem.Name == "ConvergenceLimit" ? pval.ToString("F5") : Convert.ToString(pval);

                    doneit = uopUtils.WriteINIString(aFileSpec, phead.ToUpper(), aMem.Name, !string.IsNullOrEmpty(value) ? value : string.Empty);
                    z++;
                    if (!doneit && !bSuppressErrors)
                    {
                        throw new Exception("[TPROPERTIES.WriteToINIFile] INI Write failed.");
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aProps">the subject properties</param>
        /// <param name="aTStream">the text stream to wite the properties to</param>
        /// <param name="aHeader">optional Heading text to write to the file before the properties</param>
        /// <param name="bShowDecodedValue">writes the signatures of the properties to the passed text file</param>
        /// <param name="bShowNulls"></param>
        public static void WriteToFile(TPROPERTIES aProps, System.IO.StreamWriter aTStream, string aHeader = "", bool bShowDecodedValue = true, bool bShowNulls = false)
        {
            if (aTStream == null)
            {
                return;
            }
            TPROPERTY aMem;
            int i = 0;
            if (aHeader !=  string.Empty)
            {
                aTStream.WriteLine(aHeader);
            }
            for (i = 1; i <= aProps.Count; i++)
            {
                aMem = aProps.Item(i);
                aTStream.WriteLine(aMem.Signature(bShowDecodedValue, bShowNulls));
            }
        }


        public static TPROPERTIES ConCat(TPROPERTIES aProps, TPROPERTIES bProps)
        {
            TPROPERTIES _rVal = (TPROPERTIES)aProps.Clone();
            for (int i = 1; i <= bProps.Count; i++)
            {
                _rVal.Add(bProps.Item(i));
            }
            return _rVal;
        }

        //writes the signatures of the properties to the VB debug window
        public static void WriteToDebug(TPROPERTIES aProps, string aHeader, bool bShowHeadings = false, bool bShowDecodedValue = true)
        {
            if (aHeader !=  string.Empty)
            {
                System.Diagnostics.Debug.WriteLine(aHeader);
            }
            TPROPERTY aMem;
            string lstHead = string.Empty;
            for (int i = 1; i <= aProps.Count; i++)
            {
                aMem = aProps.Item(i);

                if (bShowHeadings)
                {
                    if (aMem.Heading !=  string.Empty)
                    {
                        if (aMem.Heading != lstHead)
                        {
                            lstHead = aMem.Heading;
                            System.Diagnostics.Debug.WriteLine($"[{ aMem.Heading }]");
                        }
                    }
                }
                System.Diagnostics.Debug.WriteLine(aMem.Signature(bShowDecodedValue, true));
            }
        }

        /// <summary>
        /// updates the indicated member with the data from the passed property
        /// </summary>
        /// <param name="aProps">the subject properties</param>
        /// <param name="aNameOrIndex">the name of the property to update</param>
        /// <param name="aReplacement">the property to copy data from</param>
        /// <param name="bRename">flag to rename the original property to the name of the replacement</param>
        /// <param name="aIndex"></param>
        /// <param name="rChanged">returns true if a new value is stored</param>
        /// <returns></returns>
        public static TPROPERTIES ReplaceProperty(TPROPERTIES aProps, dynamic aNameOrIndex, TPROPERTY aReplacement, out int aIndex, out bool rChanged, out TPROPERTY rMember, bool bRename = false)
        {
            rChanged = false;
            aIndex = 0;
            TPROPERTIES _rVal = (TPROPERTIES)aProps.Clone();
            if (string.IsNullOrEmpty(aReplacement.Name)) { bRename = false; }
            string aNm;
            TPROPERTY mem;

            if (aProps.TryGet(aNameOrIndex, out rMember))

            {
                aIndex = rMember.Index;
                mem = new TPROPERTY(rMember);
                aNm = rMember.Name;
                rChanged = mem.Value != aReplacement.Value;
                mem.SetValue(aReplacement.Value);
                if (!bRename)
                {
                    if (string.Compare(aNm, aReplacement.Name, true) == 0)
                    {
                        rChanged = true;
                        mem.Name = aReplacement.Name;
                    }
                }


                _rVal.SetItem(aIndex, mem);

            }

            return _rVal;
        }


        public static TPROPERTIES Insert(TPROPERTIES aProps, TPROPERTIES bProps, int aInsertAfterID)
        {
            TPROPERTIES _rVal = aProps.Clone(false);
            if (aInsertAfterID <= 0 || aInsertAfterID >= aProps.Count)
            {
                _rVal = TPROPERTIES.ConCat(aProps, bProps);
            }
            else
            {
                int idx = 0;
                int j = 0;
                //_rVal.Count = 0;
                _rVal.Clear();
                for (int i = 1; i <= aProps.Count; i++)
                {
                    idx = i - 1;
                    if (idx == aInsertAfterID)
                    {
                        _rVal.Add(aProps.Item(i));
                        for (j = 1; j <= bProps.Count; j++)
                        {
                            _rVal.Add(bProps.Item(j));
                        }
                    }
                    else
                    {
                        _rVal.Add(aProps.Item(i));
                    }
                }
            }
            return _rVal;
        }
    
        /// <summary>
        ///returns a collection of properties from the passed collection that have the same name but different values
        /// </summary>
        /// <param name="aProps">1the first properties to compare to</param>
        /// <param name="bProps">the second properties to compare to </param>
        /// <param name="rDifIndices"></param>
        /// /// <param name="aPrecis"></param>
        /// <param name="bBailOnFistDifference"></param>

        /// <returns></returns>
        public static TPROPERTIES GetDifferences(TPROPERTIES aProps, TPROPERTIES bProps, out List<string> rDifIndices, int aPrecis = 5, bool bBailOnFistDifference = false)
        {
            rDifIndices = new List<string>();
            TPROPERTIES _rVal = aProps.Clone(true);
            if (bProps.Count <= 0) { return _rVal; }
            TPROPERTY hisProp;

            for (int i = 1; i <= bProps.Count; i++)
            {
                hisProp = bProps.Item(i);
                if (aProps.TryGet(hisProp.Name, out TPROPERTY myProp))
                {
                    if (!TPROPERTY.Compare(myProp, hisProp, aPrecis))
                    {
                        rDifIndices.Add(hisProp.Name);
                        _rVal.Add(hisProp);
                        if (bBailOnFistDifference) { return _rVal; }

                    }
                }
            }
            return _rVal;
        }

        /// <summary>
        /// used to compare two property collections and deterime how they differ from each other
        /// </summary>
        /// <param name="aProps">the first properties to compare to</param>
        /// <param name="bProps">the second properties to compare to</param>
        /// <param name="aDifferences">returns a collection of properties from the passed collection that have the same name but different values</param>
        /// <param name="aLikes">returns a collection of properties from the passed collection that have the same name and equal values</param>
        /// <param name="aNotMembers">returns a collection of properties from the passed collection that are not found in the current collection</param>
        /// <param name="aPrecis"></param>
        /// <param name="bBailOnFistDifference"></param>
        public static void GetIntersections(TPROPERTIES aProps, TPROPERTIES bProps, out TPROPERTIES aDifferences, out TPROPERTIES aLikes, out TPROPERTIES aNotMembers, int aPrecis = 5, bool bBailOnFistDifference = false)
        {
            aDifferences = aProps.Clone(true);
            aLikes = aProps.Clone(true);
            aNotMembers = aProps.Clone(true);

            if (bProps.Count <= 0) { return; }
      


            for (int i = 1; i <= bProps.Count; i++)
            {
                TPROPERTY hisProp = bProps.Item(i);
                if (aProps.TryGet(hisProp.Name, out TPROPERTY myProp))
                {
                    if (TPROPERTY.Compare(myProp, hisProp, aPrecis))
                    { aLikes.Add(hisProp); }
                    else
                    {
                        aDifferences.Add(hisProp);
                        if (bBailOnFistDifference)
                        { break; }
                    }
                }
                else
                {
                    aNotMembers.Add(hisProp);
                }
            }
        }


        /// <summary>
        /// returns True if the the properties are equal
        /// </summary>
        /// <param name="aProps">the first to compare</param>
        /// <param name="bProps">the second to compare</param>
        /// <param name="aPrecis">precision for numerical comparisons</param>
        /// <returns></returns>
        public static bool Compare(TPROPERTIES aProps, TPROPERTIES bProps, int aPrecis = 5, List<string> aSkipList = null)
        {
            if (aProps.Count != bProps.Count) { return false; }
            aPrecis = mzUtils.LimitedValue(aPrecis, 0, 8);

            for (int i = 1; i <= aProps.Count; i++)
            {
                if (aSkipList != null)
                {
                    if (aSkipList.FindIndex((x) => string.Compare(x, aProps.Item(i).Name, true) == 0) >= 0)
                        continue;
                }
                if (!TPROPERTY.Compare(aProps.Item(i), bProps.Item(i), aPrecis))
                    return false;
            }
            return true;
        }


        /// <summary>
        ///returns the properties from the collection whose names match the on in the passed list
        /// </summary>
        /// <param name="aProps"></param>
        /// <param name="aPropNames">the property names to search for</param>
        /// <param name="aDelimitor">the deliminator of the  passed string</param>
        /// <param name="rProps"></param>
        /// <returns></returns>
        public static int ContainsNames(TPROPERTIES aProps, string aPropNames, string aDelimitor, out List<uopProperty> rProps)
        {
            rProps = new List<uopProperty>();
            int _rVal = 0;
            try
            {

                aPropNames = aPropNames.Trim();
                aDelimitor = aDelimitor.Trim();
                if (aPropNames ==  string.Empty) { return _rVal; }
                if (aDelimitor ==  string.Empty) { return _rVal; }
                string pname = string.Empty;

                TVALUES pnms = TVALUES.FromDelimitedList(aPropNames, aDelimitor);

                for (int i = 1; i <= pnms.Count; i++)
                {
                    pname = Convert.ToString(pnms.Item(i));
                    if (aProps.TryGet(pname, out TPROPERTY aMem)) { rProps.Add(new uopProperty(aMem)); }

                }
                _rVal = rProps.Count;
            }
            catch (Exception exception)
            {
                LoggerManager log = new LoggerManager();
                log.LogError(exception.Message);
            }
            return _rVal;
        }


        #endregion
    }

    internal struct TQUESTION : ICloneable
    {
        public int Index;
        public uopQueryTypes QType;
        public string Prompt;
        public string Tag;
        public string Suffix;
        public int ColCount;
        public dynamic Answer;
        public dynamic LastAnswer;
        public double DisplayMultiplier;
        public bool AnswerRequired;
        public string ChoiceDelimiter;
        public string ChoiceSubDelimiter;
        public int MinChoiceCount;
        public int MaxChoiceCount;
        public int MaxChars;
        public TVALUES Choices;
        public TVALUES Headers;
        public TVALUES ColumnWidths;
        public int MaxWhole;
        public int MaxDecimals;
        public uopValueControls ValueControl;
        public double? MaxAnswer;
        public double? MinAnswer;
        public int MinValues;
        public bool ShowAllDigits;
        public bool AddMirrors;
        public double MinDifference;
        public string ToolTip;
        public TQUESTION(string aPrompt, uopQueryTypes aType = uopQueryTypes.YesNo)
        {
            Index = 0;
            QType = aType;
            Prompt = aPrompt;
            Tag = string.Empty;
            Suffix = string.Empty;
            ColCount = 0;
            Answer = null;
            LastAnswer = null;
            DisplayMultiplier = 1;
            AnswerRequired = false;
            ChoiceDelimiter = ",";
            ChoiceSubDelimiter = ":";
            MinChoiceCount = 0;
            MaxChoiceCount = 0;
            MaxChars = 50;
            Choices = new TVALUES();
            Headers = new TVALUES();
            ColumnWidths = new TVALUES();
            MaxWhole = 0;
            MaxDecimals = 6;
            ValueControl = uopValueControls.None;
            MaxAnswer = null;
            MinAnswer = null;
            MinValues = 0;
            ShowAllDigits = true;
            AddMirrors = false;
            MinDifference = 0;
            ToolTip = string.Empty;
        }
        public TQUESTION(TQUESTION aQuestion)
        {
            Index = 0;
            QType = aQuestion.QType;
            Prompt = aQuestion.Prompt;
            Tag = string.Empty;
            Suffix = string.Empty;
            ColCount = 0;
            Answer = null;
            LastAnswer = null;
            DisplayMultiplier = 1;
            AnswerRequired = false;
            ChoiceDelimiter = ",";
            ChoiceSubDelimiter = ":";
            MinChoiceCount = 0;
            MaxChoiceCount = 0;
            MaxChars = 50;
            Choices = new TVALUES();
            Headers = new TVALUES();
            ColumnWidths = new TVALUES();
            MaxWhole = 0;
            MaxDecimals = 6;
            ValueControl = uopValueControls.None;
            MaxAnswer = null;
            MinAnswer = null;
            MinValues = 0;
            ShowAllDigits = true;
            AddMirrors = false;
            MinDifference = 0;
            ToolTip = string.Empty;
        }
        object ICloneable.Clone() => (object)Clone();

        public TQUESTION Clone()
        {
            return new TQUESTION
            {


                Index = Index,
                QType = QType,
                Prompt = Prompt,
                Tag = Tag,
                Suffix = Suffix,
                ColCount = ColCount,
                Answer = Answer,
                LastAnswer = LastAnswer,
                DisplayMultiplier = DisplayMultiplier,
                AnswerRequired = AnswerRequired,
                ChoiceDelimiter = ChoiceDelimiter,
                ChoiceSubDelimiter = ChoiceSubDelimiter,
                MinChoiceCount = MinChoiceCount,
                MaxChoiceCount = MaxChoiceCount,
                MaxChars = MaxChars,
                Choices = new TVALUES(Choices),
                Headers = new TVALUES(Headers),
                ColumnWidths = new TVALUES(ColumnWidths),
                MaxWhole = MaxWhole,
                MaxDecimals = MaxDecimals,
                ValueControl = ValueControl,
                MaxAnswer = MaxAnswer,
                MinAnswer = MinAnswer,
                MinValues = MinValues,
                ShowAllDigits = ShowAllDigits,
                AddMirrors = AddMirrors,
                MinDifference = MinDifference,
                ToolTip = ToolTip
            };

        }

        public string NumberFormat(bool bShowAllPrecision = false)
        {
            string qst_NumberFormat = "0";
            if (MaxDecimals > 0)
            {
                qst_NumberFormat = $"{qst_NumberFormat}.0";
                if (MaxDecimals > 1)
                {
                    if (bShowAllPrecision)
                    {
                        string zeros = new string('0', MaxDecimals - 1);
                        qst_NumberFormat = $"{qst_NumberFormat}{zeros}";
                    }
                    else
                    {
                        string zeros = new string('#', MaxDecimals - 1);
                        qst_NumberFormat = $"{qst_NumberFormat}{zeros}";
                    }
                }
            }
            return qst_NumberFormat;
        }

        public bool ValidateNumber(out string rError, dynamic aNumber = null, string aNumericList = null)
        {
            bool qst_ValidateNumber = false;
            rError = string.Empty;
             aNumber ??= Answer;
            
            double doubleNumber = mzUtils.VarToDouble(aNumber);

            switch (ValueControl)
            {
                case uopValueControls.Positive:
                    if (doubleNumber < 0)
                        rError = " Must Be a Positive Number !";
                    break;
                case uopValueControls.Negative:
                    if (doubleNumber > 0)
                        rError = " Must Be a Negative Number!";
                    break;
                case uopValueControls.NonZero:
                    if (doubleNumber == 0)
                        rError = " Zero Is Invalid !";
                    break;
                case uopValueControls.PositiveNonZero:
                    if (doubleNumber <= 0)
                        rError = " Must Greater Than Zero !";
                    break;
                case uopValueControls.NegativeNonZero:
                    if (doubleNumber > 0)
                        rError = " Must Less Than Zero !";
                    break;
                default:
                    break;
            }

            if (string.IsNullOrWhiteSpace(rError))
            {
                if (MaxAnswer.HasValue)
                {
                    double limit = MaxAnswer.Value * DisplayMultiplier;
                    if (MaxDecimals > 0 && MaxDecimals < 15) limit = Math.Round(limit, MaxDecimals);
                    if (aNumber > limit)
                    {
                        rError = $" Must Be Less Than or Equal To {limit} !";
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(rError))
            {
                if (MinAnswer.HasValue)
                {
                    double limit = MinAnswer.Value * DisplayMultiplier;
                    if (MaxDecimals > 0 && MaxDecimals < 15) limit = Math.Round(limit, MaxDecimals);

                    if (aNumber <limit)
                    {
                        rError = $" Must Be Greater Than or Equal To {limit} !";
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(rError))
            {
                if (QType == uopQueryTypes.NumericList)
                {
                    if (MinDifference > 0)
                    {
                        TVALUES aVals;
                        double aDif;
                        if (aNumericList != null)
                        {
                            aVals = new TVALUES(mzUtils.ListValues(aNumericList, bNumbersOnly: true));
                        }
                        else
                        {
                            aVals = new TVALUES(mzUtils.ListValues(Answer, bNumbersOnly: true));
                        }

                        if (aVals.Count > 1)
                        {
                            //aVals.Add(aNumber);
                            aDif = aVals.MinDifference();
                            if (aDif < MinDifference)
                            {
                                rError = $" A Minumum Difference of {mzUtils.VarToDouble(MinDifference * DisplayMultiplier, aPrecis: MaxDecimals)} Between The Values Is Required !";
                            }
                        }
                    }
                }
            }

            qst_ValidateNumber = rError == string.Empty;
            return qst_ValidateNumber;
        }

        public static bool ValidateNumbers(TQUESTION aQuestion, out string rError)
        {
            bool _rVal = true;
            // On Error Resume Next <-> No such construct in C#
            rError = string.Empty;
            if (aQuestion.QType != uopQueryTypes.NumericList)  return true;
           
            TVALUES aVals = TVALUES.FromDelimitedList(aQuestion.Answer, ",", false, true, true, true, aQuestion.MaxDecimals);
            aVals.SortNumeric(true);
            aQuestion.Answer =   aVals.ToList;


            for (int i = 1; i <= aVals.Count; i++)
            {
          
               if(! aQuestion.ValidateNumber(out rError, aVals.Item(i)))
                {
                    _rVal = false;
                    break;
                }

            }

            if (_rVal)
            {
                if (aQuestion.MinChoiceCount > 0)
                {
                    if (aVals.Count < aQuestion.MinChoiceCount)
                    {
                        rError = $" A Minumum of {aQuestion.MinChoiceCount} Values Are Required !";
                        _rVal = false;
                    }
                }
            }

            if (_rVal)
            {
                if (aQuestion.MinDifference > 0)
                {
                    if (aVals.Count > 1)
                    {
                        if (aVals.MinDifference() < aQuestion.MinDifference)
                        {
                            rError = $" A Minumum Difference of {mzUtils.VarToDouble(aQuestion.MinDifference * aQuestion.DisplayMultiplier, aPrecis: aQuestion.MaxDecimals)} Between The Values Is Required !";
                            _rVal = false;
                        }
                    }
                }
            }

            return _rVal;
        }
        public override string ToString() => $"TQUESTION [{QType.GetDescription()}] '{  Prompt }'";


        /// <summary>
        /// Add new Questions
        /// </summary>
        /// <param name="aPrompt"></param>
        /// <param name="aType"></param>
        /// <param name="aInitialAnswer"></param>
        /// <param name="aChoices"></param>
        /// <param name="bAnswerRequired"></param>
        /// <param name="aChoiceDelimiter"></param>
        /// <param name="aChoiceSubDelimiter"></param>
        /// <param name="aHeaders"></param>
        /// <param name="aMinChoiceCount"></param>
        /// <param name="aMaxLengthOrWholeDigits"></param>
        /// <param name="aMaxDecis"></param>
        /// <param name="aValueControl"></param>
        /// <param name="aMaxValue"></param>
        /// <param name="aMinValue"></param>
        /// <param name="aColumnWidths"></param>
        /// <param name="aSuffix"></param>
        /// <param name="aDisplayMultiplier"></param>
        /// <param name="bShowAllDigits"></param>
        /// <param name="bAddMirrors"></param>
        /// <param name="aMinDifference"></param>
        /// <param name="aTag"></param>
        public static TQUESTION Create(string aPrompt, uopQueryTypes aType, dynamic aInitialAnswer = null, string aChoices = "", bool bAnswerRequired = false,
                                        string aChoiceDelimiter = ",", string aChoiceSubDelimiter = "", string aHeaders = "", int aMinChoiceCount = -1, int aMaxLengthOrWholeDigits = 0,
                                        int aMaxDecis = 4, uopValueControls aValueControl = new uopValueControls(), double? aMaxValue = null,
                                        double? aMinValue = null, string aColumnWidths = "", string aSuffix = "", double? aDisplayMultiplier = null, bool bShowAllDigits = false,
                                        bool bAddMirrors = false, double aMinDifference = 0, string aTag = "", string aToolTip = "")
        {
            aInitialAnswer ??= string.Empty;

            aChoiceDelimiter ??= ",";
            aChoiceSubDelimiter ??= string.Empty;
            aHeaders ??= string.Empty;
            aSuffix ??= string.Empty;
            aColumnWidths ??= string.Empty;
            aChoices ??= string.Empty;
            aTag ??= string.Empty;
            aToolTip ??= string.Empty;


            if (string.IsNullOrWhiteSpace(aPrompt))
                aPrompt = "Question ?";
            aPrompt = aPrompt.Trim();

            aColumnWidths = aColumnWidths.Trim();
            aTag = aTag.Trim();
            if (string.IsNullOrEmpty(aTag))
                aTag = aPrompt;
            TQUESTION aQst;
            TVALUES aChcs = new TVALUES();
            TVALUES aVals;
            TVALUES bVals;
            TVALUES aHdrs;

            string aStr;
            int ccnt;
            int idx;

            TQUESTION _rVal = new TQUESTION("", uopQueryTypes.Undefined);
            ccnt = 1;
            aQst = new TQUESTION("");
            aHdrs = aQst.Headers;

            aChoices = aChoices.Trim();
            // ================================================
            if (aType == uopQueryTypes.SingleSelect)
            {
                // ================================================
                aChcs = TVALUES.FromDelimitedList(aChoices, aChoiceDelimiter, false, true, true);
                aInitialAnswer = aInitialAnswer.Trim();
                if (!string.IsNullOrEmpty(aInitialAnswer))
                {
                    if (!mzUtils.ListContains(aInitialAnswer, aChoices))
                        mzUtils.ListAdd(ref aChoices, aInitialAnswer);
                }
                if (string.IsNullOrEmpty(aChoices))
                    return _rVal;
            }
            else if (aType == uopQueryTypes.MultiSelect)
            {
                // ================================================


                aChoiceDelimiter = aChoiceDelimiter.Trim();
                if (string.IsNullOrEmpty(aChoiceDelimiter))
                    aChoiceDelimiter = ",";

                aChcs = TVALUES.FromDelimitedList(aChoices, aChoiceDelimiter, false, true, true);

                aChoiceSubDelimiter = aChoiceSubDelimiter.Trim();
                aHeaders = aHeaders.Trim();

                if (!string.IsNullOrEmpty(aChoiceSubDelimiter))
                {
                    for (int i = 1; i <= aChcs.Count; i++)
                    {
                        aStr = aChcs.Item(i);
                        aVals = TVALUES.FromDelimitedList(aStr, aChoiceSubDelimiter, true, true, false);
                        aStr = aVals.ToDelimitedList(aChoiceSubDelimiter, out int j, bNoNulls: false);
                        if (j > ccnt)
                            ccnt = j;
                        aChcs.SetValue(i, aStr);
                    }
                }



                aInitialAnswer = aInitialAnswer.Trim();
                if (!string.IsNullOrEmpty(aInitialAnswer))
                {
                    if (string.Compare(aInitialAnswer, "All", true) == 0)
                    {
                        aInitialAnswer = aChoices;
                    }
                    else
                    {
                        bVals = new TVALUES("");
                        aVals = TVALUES.FromDelimitedList(aInitialAnswer, aChoiceDelimiter, false, true, true);
                        for (int i = 1; i <= aVals.Count; i++)
                        {
                            aStr = aVals.Item(i);

                            if (!string.IsNullOrEmpty(aStr))
                            {
                                idx = aChcs.FindStringValue(aStr, aChoiceSubDelimiter);
                                if (idx <= 0)
                                    aChcs.Add(aStr);
                                bVals.Add(aStr);
                            }
                        }
                        aInitialAnswer = bVals.ToDelimitedList(aChoiceDelimiter);
                    }

                }


                if (!string.IsNullOrEmpty(aHeaders))
                {
                    aHdrs = TVALUES.FromDelimitedList(aHeaders, aChoiceDelimiter, true, true, false);
                    if (aHdrs.Count > ccnt)
                        ccnt = aHdrs.Count;
                }
                else
                    aHdrs = new TVALUES("Headers");
                aHdrs.Add("");
            }
            else if (aType == uopQueryTypes.StringChoice)
            {
                // ================================================


                aChoiceDelimiter = aChoiceDelimiter.Trim();
                if (string.IsNullOrEmpty(aChoiceDelimiter))
                    aChoiceDelimiter = ",";

                aChcs = TVALUES.FromDelimitedList(aChoices, aChoiceDelimiter, false, true, true);

                aInitialAnswer = aInitialAnswer.Trim();
            }
            else if (aType == uopQueryTypes.DualStringChoice)
            {
                // ================================================


                aChoiceDelimiter = aChoiceDelimiter.Trim();
                if (string.IsNullOrEmpty(aChoiceDelimiter))
                    aChoiceDelimiter = ",";

                aChoiceSubDelimiter = aChoiceSubDelimiter.Trim();
                if (string.IsNullOrEmpty(aChoiceSubDelimiter))
                    aChoiceSubDelimiter = "|";
                if (aChoiceSubDelimiter == aChoiceDelimiter)
                    aChoiceSubDelimiter = "|";

                aChcs = TVALUES.FromDelimitedList(aChoices, aChoiceDelimiter, false, true, true);

                aInitialAnswer = aInitialAnswer.Trim();
            }
            else if (aType == uopQueryTypes.CheckVal)
                // ================================================
                aInitialAnswer = mzUtils.VarToBoolean(aInitialAnswer);
            else if (aType == uopQueryTypes.YesNo)
            {
                // ================================================
                if (aInitialAnswer != null)
                    aInitialAnswer = mzUtils.VarToBoolean(aInitialAnswer);
            }
            else if (aType == uopQueryTypes.StringValue)
            {
                // ================================================
                aMaxLengthOrWholeDigits = Math.Abs(aMaxLengthOrWholeDigits);
                aInitialAnswer = aInitialAnswer.Trim();
            }
            else if (aType == uopQueryTypes.NumericValue)
            {
                // ================================================
                aMaxLengthOrWholeDigits = mzUtils.LimitedValue(aMaxLengthOrWholeDigits, 1, 12, 12);

                aMaxDecis = mzUtils.LimitedValue((int)Math.Abs(aMaxDecis), 0, 6, 6);

                aInitialAnswer = mzUtils.VarToDouble(aInitialAnswer, false, null, aMaxDecis);
            }
            else if (aType == uopQueryTypes.NumericList)
            {
                // ================================================
                aMaxLengthOrWholeDigits = mzUtils.LimitedValue((int)Math.Abs(aMaxLengthOrWholeDigits), 1, 12);

                if (string.IsNullOrWhiteSpace(aChoiceDelimiter)) aChoiceDelimiter = ",";
                aMaxDecis = mzUtils.LimitedValue((int)Math.Abs(aMaxDecis), 0, 12);


                aVals = TVALUES.FromDelimitedList(aInitialAnswer, aChoiceDelimiter, false, true, true, true, aMaxDecis);
                aVals.Sort(bHighToLow: true);
                aInitialAnswer = aVals.ToDelimitedList(aChoiceDelimiter);
            }
            else if (aType == uopQueryTypes.Folder)
            {
                // ================================================

                aInitialAnswer = aInitialAnswer.Trim();
                if (!string.IsNullOrEmpty(aInitialAnswer) && !System.IO.File.Exists(aInitialAnswer))
                {
                    // aInitialAnswer = App.Path;//TODO
                }
            }
            else
            { return _rVal; }


            _rVal.QType = (uopQueryTypes)aType;
            _rVal.Prompt = aPrompt;
            _rVal.Tag = aTag;
            _rVal.Suffix = aSuffix.Trim();
            _rVal.Answer = aInitialAnswer;
            _rVal.Choices = aChcs;
            _rVal.ToolTip = aToolTip;
            if (aDisplayMultiplier.HasValue)
            {
                _rVal.DisplayMultiplier = Math.Round(aDisplayMultiplier.Value, 5);
                if (_rVal.DisplayMultiplier <= 0) _rVal.DisplayMultiplier = 1;
            }

            _rVal.ChoiceDelimiter = aChoiceDelimiter;
            _rVal.ChoiceSubDelimiter = aChoiceSubDelimiter;
            _rVal.AnswerRequired = bAnswerRequired;
            if ((aType == uopQueryTypes.MultiSelect || aType == uopQueryTypes.SingleSelect) && aChcs.Count <= 0)
            {
                _rVal.AnswerRequired = false;
            }
            _rVal.ColCount = ccnt;
            _rVal.Headers = aHdrs;


            _rVal.MinChoiceCount = aMinChoiceCount;
            if (_rVal.QType == uopQueryTypes.MultiSelect)
            {
                if (_rVal.MinChoiceCount > _rVal.Choices.Count)
                    _rVal.MinChoiceCount = _rVal.Choices.Count;
                if (_rVal.MinChoiceCount > 0)
                    _rVal.AnswerRequired = true;
            }

            if (_rVal.QType == uopQueryTypes.StringValue)
            {
                _rVal.MaxChars = aMaxLengthOrWholeDigits;
                if (_rVal.MaxChars > 0 && Convert.ToString(_rVal.Answer).Length > _rVal.MaxChars)
                    _rVal.Answer = Convert.ToString(_rVal.Answer).SubString(_rVal.MaxChars);
            }
            _rVal.ShowAllDigits = bShowAllDigits;
            _rVal.ValueControl = aValueControl;

            if (_rVal.QType == uopQueryTypes.NumericValue || _rVal.QType == uopQueryTypes.NumericList)
            {
                _rVal.MaxWhole = aMaxLengthOrWholeDigits;
                _rVal.MaxDecimals = aMaxDecis;

               _rVal.MaxAnswer = aMaxValue;

                 _rVal.MinAnswer = aMinValue;

                if(_rVal.MaxAnswer.HasValue && _rVal.MinAnswer.HasValue)
                {
                    if (_rVal.MaxAnswer.Value == _rVal.MinAnswer.Value)
                    {
                        _rVal.MaxAnswer = null;
                        _rVal.MinAnswer = null;
                    }
                }
               
            }

            if (!string.IsNullOrEmpty(aColumnWidths))
                _rVal.ColumnWidths = TVALUES.FromDelimitedList(aColumnWidths, ",", false, true, false, true);
            _rVal.LastAnswer = _rVal.Answer;
            _rVal.AddMirrors = bAddMirrors;
            if (aMinDifference > 0)
                _rVal.MinDifference = aMinDifference;

            return _rVal;
        }

    }

    internal struct TQUESTIONS
    {
        public string Title;
        private int _Count;
        private TQUESTION[] _Members;
        private bool _Init;

        public TQUESTIONS(string aTitle = "")
        {
            Title = aTitle;
            _Count = 0;
            _Members = new TQUESTION[0];
            _Init = true;
        }

        private int Init() { _Members = new TQUESTION[0]; _Count = 0; return 0; }

        public int Count => (!_Init) ? Init() : _Count;

        public void Clear() => Init();

        public List<TQUESTION> ToList { get => new List<TQUESTION>(_Members); }

        public TQUESTIONS Clone(bool bReturnEmpty = false)
        {
            if (!_Init) { Init(); }
            TQUESTION[] mems = bReturnEmpty ? new TQUESTION[0] : Force.DeepCloner.DeepClonerExtensions.DeepClone<TQUESTION[]>(_Members);  //(TQUESTION[])_Members.Clone();
            int cnt = bReturnEmpty ? 0 : _Count;
            return new TQUESTIONS(Title) { _Members = mems, _Count = cnt, _Init = true };
        }

        public TQUESTION Item(int aIndex)
        {
            if (aIndex < 1 || aIndex > Count) { return new TQUESTION(""); }
            _Members[aIndex - 1].Index = aIndex;
            return _Members[aIndex - 1];
        }


        public TQUESTION Item(string aPrompt)
        {
            for (int i = 1; i <= Count; i++) { if (string.Compare(Item(i).Prompt, aPrompt, ignoreCase: true) == 0) { return Item(i); } }
            return new TQUESTION("");
        }

        public bool TryGet(dynamic aIndexOrPrompt, out TQUESTION rMember)
        {
            rMember = new TQUESTION("");

            if (mzUtils.IsNumeric(aIndexOrPrompt))
            { rMember = Item(mzUtils.VarToInteger(aIndexOrPrompt)); }
            else
            { rMember = Item(Convert.ToString(aIndexOrPrompt)); }
            return rMember.Index > 0;
        }

        public void SetItem(int aIndex, TQUESTION aMember)
        {
            if (aIndex < 1 || aIndex > Count) { return; }
            aMember.Index = aIndex;
            _Members[aIndex - 1] = aMember;

        }

        /// <summary>
        /// Remove a member
        /// </summary>
        /// <param name="aIndex"></param>
        /// <returns></returns>
        public TQUESTION Remove(int aIndex)
        {

            if (aIndex < 1 || aIndex > Count) { return new TQUESTION(""); }

            TQUESTION _rVal = new TQUESTION();
            if (Count == 1) { _rVal = Item(1); Clear(); return _rVal; }

            if (aIndex == Count)
            {
                _rVal = Item(Count);
                _Count -= 1;
                Array.Resize<TQUESTION>(ref _Members, _Count);
                return _rVal;
            }


            TQUESTION[] newMems = new TQUESTION[_Count - 2];
            int j = 0;


            for (int i = 1; i <= Count; i++)
            {
                if (i != aIndex)
                { j += 1; newMems[j - 1] = Item(i); newMems[j - 1].Index = j; }
                else
                { _rVal = Item(i); }

            }

            _Members = newMems;
            _Count -= 1;
            return _rVal;
        }

        /// <summary>
        /// Add questions
        /// </summary>
        /// <param name="aQuestions"></param>
        /// <param name="aQuestion"></param>
        public TQUESTION Add(TQUESTION newMem)
        {
            if (Count + 1 > Int32.MaxValue) { return newMem; }

            _Count += 1;
            Array.Resize<TQUESTION>(ref _Members, _Count);
            newMem.Index = _Count;
            _Members[_Count - 1] = newMem;
            return Item(Count);
        }


        /// <summary>
        /// Add new Questions
        /// </summary>
        /// <param name="aPrompt"></param>
        /// <param name="aType"></param>
        /// <param name="aInitialAnswer"></param>
        /// <param name="aChoices"></param>
        /// <param name="bAnswerRequired"></param>
        /// <param name="aChoiceDelimiter"></param>
        /// <param name="aChoiceSubDelimiter"></param>
        /// <param name="aHeaders"></param>
        /// <param name="aMinChoiceCount"></param>
        /// <param name="aMaxLengthOrWholeDigits"></param>
        /// <param name="aMaxDecis"></param>
        /// <param name="aValueControl"></param>
        /// <param name="aMaxValue"></param>
        /// <param name="aMinValue"></param>
        /// <param name="aColumnWidths"></param>
        /// <param name="aSuffix"></param>
        /// <param name="aDisplayMultiplier"></param>
        /// <param name="bShowAllDigits"></param>
        /// <param name="bAddMirrors"></param>
        /// <param name="aMinDifference"></param>
        /// <param name="aTag"></param>
        public TQUESTION Add(string aPrompt, uopQueryTypes aType, dynamic aInitialAnswer = null, string aChoices = "", bool bAnswerRequired = false,
                                        string aChoiceDelimiter = ",", string aChoiceSubDelimiter = "", string aHeaders = "", int aMinChoiceCount = -1, int aMaxLengthOrWholeDigits = 0,
                                        int aMaxDecis = 4, uopValueControls aValueControl = new uopValueControls(), double? aMaxValue = null,
                                        double? aMinValue = null, string aColumnWidths = "", string aSuffix = "", double? aDisplayMultiplier = null, bool bShowAllDigits = false,
                                        bool bAddMirrors = false, double aMinDifference = 0, string aTag = "", string aToolTip = "")
        {


            if (string.IsNullOrEmpty(aPrompt))
                aPrompt = "Question " + Count + 1;

            TQUESTION aQst = TQUESTION.Create(aPrompt, aType, aInitialAnswer, aChoices, bAnswerRequired, aChoiceDelimiter, aChoiceSubDelimiter, aHeaders, aMinChoiceCount, aMaxLengthOrWholeDigits, aMaxDecis, aValueControl, aMaxValue, aMinValue, aColumnWidths, aSuffix, aDisplayMultiplier, bShowAllDigits, bAddMirrors, aMinDifference, aTag, aToolTip);
            if (aQst.QType == uopQueryTypes.Undefined) return new TQUESTION("", uopQueryTypes.Undefined);
            aQst.Index = Count + 1;

            return Add(aQst);
        }


        public dynamic AnswerByIndex(int aIndex, dynamic aDefaultAnswer, out dynamic rLastAnswer, out int rIndex)
        {
            rIndex = 0;

            rLastAnswer = aDefaultAnswer;
            TQUESTION mem = Item(aIndex);

            if (mem.Index <= 0) return aDefaultAnswer;
            rIndex = mem.Index;
            dynamic _rVal = mem.Answer;
            rLastAnswer = mem.LastAnswer;
            if (mem.QType == uopQueryTypes.YesNo)
            {
                if (_rVal == null) _rVal = false;

            }

            if (_rVal == null || (object)_rVal == default) _rVal = string.Empty;
            if (rLastAnswer == null || (object)rLastAnswer == default) rLastAnswer = string.Empty;
            return _rVal;



        }


        public dynamic AnswerByIndex(int aIndex, dynamic aDefaultAnswer) => AnswerByIndex(aIndex, aDefaultAnswer, out dynamic _, out int _);

        public TQUESTION GetByPrompt(string aPrompt) { return GetByPrompt(aPrompt, out int IDX); }

        public TQUESTION GetByPrompt(string aPrompt, out int rIndex)
        {
            rIndex = 0;
            TQUESTION aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);

                if (string.Compare(aMem.Prompt, aPrompt, true) == 0)
                {
                    rIndex = i;
                    if ((object)aMem.Answer == default) aMem.Answer = null;

                    if ((object)aMem.LastAnswer == default) aMem.LastAnswer = aMem.Answer;

                    if (aMem.QType == uopQueryTypes.YesNo)
                    {
                        if (aMem.Answer is bool) aMem.Answer = mzUtils.VarToBoolean(aMem.Answer);

                        if (aMem.LastAnswer is bool) aMem.Answer = mzUtils.VarToBoolean(aMem.LastAnswer);

                    }
                    return aMem;
                }
            }
            return new TQUESTION();
        }

        public dynamic AnswerByPrompt(string aPrompt, dynamic aDefaultAnswer = null)
        {

            return AnswerByPrompt(aPrompt, out dynamic LSTANS, out int IDX, aDefaultAnswer);
        }

        public dynamic AnswerByPrompt(string aPrompt, out dynamic rLastAnswer, out int rIndex, dynamic aDefaultAnswer = null)
        {
            dynamic _rVal = aDefaultAnswer;
            rLastAnswer = aDefaultAnswer;
            GetByPrompt(aPrompt, out rIndex);
            TQUESTION mem;

            if (rIndex >= 0)
            {
                mem = Item(rIndex);
                _rVal = mem.Answer;
                rLastAnswer = mem.LastAnswer;

                if (mem.QType == uopQueryTypes.YesNo || mem.QType == uopQueryTypes.CheckVal)
                {
                    if (_rVal == null) _rVal = false;
                    mem.Answer = mzUtils.VarToBoolean(_rVal);
                    SetItem(rIndex, mem);
                    _rVal = mem.Answer;
                }
            }

            if (string.IsNullOrEmpty(Convert.ToString(_rVal))) _rVal = string.Empty;


            if (string.IsNullOrEmpty(Convert.ToString(rLastAnswer))) rLastAnswer = string.Empty;


            return _rVal;
        }

        public bool SetAnswerByIndex(int aIndex, dynamic aAnswer, out int rIndex)
        {
            bool qsts_SetAnswerByIndex = false;
            GetByIndex(aIndex, out rIndex);
            if (rIndex >= 0)
            {
                TQUESTION question = Item(rIndex);
                if (question.QType == uopQueryTypes.MultiSelect)
                {
                    TVALUES tvalues = TVALUES.FromDelimitedList(aAnswer, question.ChoiceDelimiter, false, true, true);
                    aAnswer = tvalues.ToDelimitedList(question.ChoiceDelimiter, true);
                }
                else
                {
                    if (question.QType == uopQueryTypes.YesNo)
                    {
                        aAnswer = mzUtils.VarToBoolean(aAnswer);
                    }
                }
                question.LastAnswer = question.Answer;
                qsts_SetAnswerByIndex = question.Answer != aAnswer;
                question.Answer = aAnswer;
                SetItem(rIndex, question);
            }

            return qsts_SetAnswerByIndex;
        }

        public bool SetAnswerByPrompt(dynamic aPrompt, dynamic aAnswer, out int rIndex)
        {
            bool qsts_SetAnswerByPrompt = false;
            GetByPrompt(aPrompt, out rIndex);
            if (rIndex >= 0)
            {
                TQUESTION question = Item(rIndex);
                if (question.QType == uopQueryTypes.MultiSelect)
                {
                    TVALUES tvalues = TVALUES.FromDelimitedList(aAnswer, question.ChoiceDelimiter, false, true, true);
                    aAnswer = tvalues.ToDelimitedList(question.ChoiceDelimiter, true);
                }
                else
                {
                    if (question.QType == uopQueryTypes.YesNo)
                    {
                        aAnswer = mzUtils.VarToBoolean(aAnswer);
                    }
                }
                question.LastAnswer = question.Answer;
                qsts_SetAnswerByPrompt = question.Answer != aAnswer;
                question.Answer = aAnswer;
                SetItem(rIndex, question);
            }

            return qsts_SetAnswerByPrompt;
        }

        public TQUESTION GetByIndex(int aIndex, out int rIndex)
        {
            TQUESTION qsts_GetByIndex = new TQUESTION(); // !!! In VB code it may set to null
            // On Error Resume Next
            rIndex = -1;
            if (aIndex > 0 && aIndex <= Count)
            {
                rIndex = aIndex;
                if (Item(rIndex).Answer == null)
                {
                    TQUESTION question = Item(rIndex);
                    question.Answer = string.Empty;
                    SetItem(rIndex, question);
                }

                if (Item(rIndex).LastAnswer == null)
                {
                    TQUESTION question = Item(rIndex);
                    question.LastAnswer = question.Answer;
                    SetItem(rIndex, question);
                }

                if (Item(rIndex).QType == uopQueryTypes.YesNo)
                {
                    if (Item(rIndex).Answer.GetType() == typeof(bool))
                    {
                        TQUESTION question = Item(rIndex);
                        question.Answer = null;
                        SetItem(rIndex, question);
                    }
                    if (Item(rIndex).LastAnswer.GetType() == typeof(bool))
                    {
                        TQUESTION question = Item(rIndex);
                        question.LastAnswer = null; // In VB code it is .Answer = null
                        SetItem(rIndex, question);
                    }
                }

                qsts_GetByIndex = Item(rIndex);
            }

            return qsts_GetByIndex;
        }

        public TQUESTION GetByPrompt(dynamic aPrompt, out int rIndex)
        {
            TQUESTION qsts_GetByPrompt = new TQUESTION(); // !!! In VB code it may set to null
            // On Error Resume Next
            rIndex = -1;
            for (int i = 1; i <= Count; i++)
            {
                if (string.Compare(Item(i - 1).Prompt, aPrompt) == 0)
                {
                    rIndex = i - 1;

                    if (Item(rIndex).Answer == null)
                    {
                        TQUESTION question = Item(rIndex);
                        question.Answer = string.Empty;
                        SetItem(rIndex, question);
                    }

                    if (Item(rIndex).LastAnswer == null)
                    {
                        TQUESTION question = Item(rIndex);
                        question.LastAnswer = question.Answer;
                        SetItem(rIndex, question);
                    }

                    if (Item(rIndex).QType == uopQueryTypes.YesNo)
                    {
                        if (Item(rIndex).Answer.GetType() == typeof(bool))
                        {
                            TQUESTION question = Item(rIndex);
                            question.Answer = null;
                            SetItem(rIndex, question);
                        }
                        if (Item(rIndex).LastAnswer.GetType() == typeof(bool))
                        {
                            TQUESTION question = Item(rIndex);
                            question.LastAnswer = null; // In VB code it is .Answer = null
                            SetItem(rIndex, question);
                        }
                    }

                    qsts_GetByPrompt = Item(rIndex);
                    break;
                }
            }

            return qsts_GetByPrompt;
        }


        public override string ToString() => "TQUESTIONS[" + Count + "]";
    }

    internal struct TMATERIALSPEC : ICloneable
    {
        public uppSpecTypes Type;
        public bool Stainless;
        public bool Metric;
        public string Comment;
        public string Name;
        public string Filter;
        public int Index;
        public TMATERIALSPEC(uppSpecTypes aType = uppSpecTypes.Undefined, string aName = "")
        {
            Type = aType;
            Stainless = false;
            Metric = false;
            Comment = string.Empty;
            Name = aName;
            Filter = string.Empty;
            Index = 0;
        }

        object ICloneable.Clone() => (object)Clone();

        public TMATERIALSPEC Clone()
        {
            return new TMATERIALSPEC()
            {
                Type = Type,
                Stainless = Stainless,
                Metric = Stainless,
                Comment = Comment,
                Name = Name,
                Filter = Filter,
                Index = Index
            };

        }

        public bool AppliesTo(TMATERIAL aMaterial)
        {

            if (aMaterial.SpecType != Type) { return false; }
            if (aMaterial.IsStainless != Stainless) { return false; }
            if (!string.IsNullOrWhiteSpace(Filter) && !string.IsNullOrWhiteSpace(aMaterial.SpecFilter))
            {
                if (Filter != aMaterial.SpecFilter) { return false; }
            }

            return true;

        }



        #region Shared Methods



        #endregion

    }

    internal struct TMATERIALSPECS : ICloneable
    {
        private int _Count;
        private TMATERIALSPEC[] _Members;
        private bool _Init;
        public string Name;
        public string TypeNames;
        public TMATERIALSPECS(string aName = "")
        {
            Name = aName;
            _Count = 0;
            _Init = true;
            _Members = new TMATERIALSPEC[0];
            TypeNames = "Sheet,Plate,Pipe,Flange,Fitting,Bolt,Nut,Lock Washer,Flat Washer,Stud,Gasket";
        }

        private int Init() { TypeNames = string.Empty; _Init = true; _Members = new TMATERIALSPEC[0]; return 0; }

        public void Clear() => Init();

        public int Count => (!_Init) ? Init() : _Count;

        public List<TMATERIALSPEC> ToList { get => new List<TMATERIALSPEC>(_Members); }

        object ICloneable.Clone() => (object)Clone(false);

        public TMATERIALSPECS Clone(bool bReturnEmpty = false)
        {
            if (!_Init) { Init(); }
            TMATERIALSPEC[] mems = bReturnEmpty ? new TMATERIALSPEC[0] : Force.DeepCloner.DeepClonerExtensions.DeepClone<TMATERIALSPEC[]>(_Members); // (TMATERIALSPEC[])_Members.Clone();
            int cnt = bReturnEmpty ? 0 : _Count;
            string tnames = bReturnEmpty ? string.Empty : TypeNames;
            return new TMATERIALSPECS() { Name = Name, _Members = mems, _Count = cnt, _Init = true, TypeNames = tnames };

        }

        public TMATERIALSPEC Item(int aIndex)
        {
            if (aIndex < 1 || aIndex > Count) { return new TMATERIALSPEC(uppSpecTypes.Undefined); }
            _Members[aIndex - 1].Index = aIndex;
            return _Members[aIndex - 1];
        }

        public TMATERIALSPEC Add(TMATERIALSPEC newMem)
        {
            if (Count + 1 > Int32.MaxValue) { return newMem; }

            _Count += 1;
            Array.Resize<TMATERIALSPEC>(ref _Members, _Count);
            newMem.Index = _Count;
            _Members[_Count - 1] = newMem;
            return Item(Count);

        }

        public bool TryGet(uppSpecTypes aType, bool bStainless, bool bMetric, out TMATERIALSPEC rSpec)
        {
            rSpec = new TMATERIALSPEC(uppSpecTypes.Undefined);

            TMATERIALSPEC mem;
            bool bTest = false;
            int rIndex = -1;
            for (int i = 1; i <= Count; i++)
            {
                mem = Item(i);
                bTest = mem.Type == aType;
                if (bTest && aType != uppSpecTypes.Gasket)
                { bTest = mem.Stainless == bStainless; }
                if (bTest && (int)aType >= 100)
                { bTest = mem.Metric == bMetric; }
                if (bTest)
                {
                    rIndex = i;
                    rSpec = mem;
                    break;
                }
            }
            return rSpec.Index > 0;
        }


        public string FirstName(uppSpecTypes aType)
        {
            TMATERIALSPEC mem;

            for (int i = 1; i <= Count; i++)
            {
                mem = Item(i);
                if (mem.Type == aType) { return mem.Name; }
            }
            return "";
        }


        public TMATERIALSPEC GetByName(uppSpecTypes aType, string aName, bool bAddNewIfNotFound = false)
        {

            TMATERIALSPEC _rVal = new TMATERIALSPEC(uppSpecTypes.Undefined);


            TMATERIALSPEC mem;
            for (int i = 1; i <= Count; i++)
            {
                mem = Item(i);
                if (string.Compare(mem.Name, aName) == 0) { _rVal = mem; break; }
            }

            if (_rVal.Index <= 0 && bAddNewIfNotFound)
            {
                _rVal = new TMATERIALSPEC(aType, aName);
                Add(_rVal);
                _rVal = Item(Count);
            }
            return _rVal;
        }

        public string GetMaterialDefault(TMATERIAL aMaterial)
        {
            string _rVal = string.Empty;
            int i = 0;
            TMATERIALSPEC mem;
            uppSpecTypes sType;
            uppMaterialTypes mtype;
            bool bTest = false;
            sType = aMaterial.SpecType;
            mtype = aMaterial.Type;
            for (i = 1; i <= Count; i++)
            {
                mem = Item(i);
                bTest = mem.Type == sType;
                if (bTest && sType != uppSpecTypes.Gasket) { bTest = mem.Stainless == aMaterial.IsStainless; }
                if (bTest && mtype == uppMaterialTypes.Hardware) { bTest = mem.Metric == aMaterial.IsMetric; }
                if (bTest)
                {
                    if (mem.AppliesTo(aMaterial))
                    {
                        _rVal = mem.Name;
                        break;
                    }
                }
            }
            return _rVal;
        }

        public TMATERIALSPEC GetMaterialDefaultStruc(TMATERIAL aMaterial)
        {
            TMATERIALSPEC _rVal = new TMATERIALSPEC(uppSpecTypes.Undefined);
            int i = 0;
            TMATERIALSPEC mem;
            uppSpecTypes sType;
            uppMaterialTypes mtype;
            bool bTest = false;
            sType = aMaterial.SpecType;
            mtype = aMaterial.Type;
            for (i = 1; i <= Count; i++)
            {
                mem = Item(i);
                bTest = mem.Type == sType;
                if (bTest && sType != uppSpecTypes.Gasket) { bTest = mem.Stainless == aMaterial.IsStainless; }
                if (bTest && mtype == uppMaterialTypes.Hardware) { bTest = mem.Metric == aMaterial.IsMetric; }
                if (bTest)
                {
                    if (mem.AppliesTo(aMaterial))
                    {
                        _rVal = mem;
                        break;
                    }
                }
            }
            return _rVal;
        }

        public string SpecList(uppSpecTypes aType, dynamic bStainless = null, dynamic bMetric = null)
        {
            string _rVal = string.Empty;
            TMATERIALSPECS aSet = SubSet(aType, bStainless, bMetric);
            for (int i = 1; i <= aSet.Count; i++)
            {
                mzUtils.ListAdd(ref _rVal, aSet.Item(i).Name);
            }
            return _rVal;
        }

        public TMATERIALSPECS GetByTypeName(string aName, dynamic bStainless = null, dynamic bMetric = null)
        => SubSet(uopUtils.SpecType(aName), bStainless, bMetric);

        public TMATERIALSPECS SubSet(uppSpecTypes aType, dynamic bStainless = null, dynamic bMetric = null)
        {
            TMATERIALSPECS _rVal = Clone(true);
            bool bChkStn = bStainless != null;
            bool bStns = false;
            bool bChkMetr = (int)aType >= 100 && (bMetric != null);
            bool bMetr = false;
            if (bChkStn) { bStns = mzUtils.VarToBoolean(bStainless); }
            if (bChkMetr) { bMetr = mzUtils.VarToBoolean(bMetric); }

            bool bKeep = false;
            TMATERIALSPEC mem;

            _rVal.TypeNames = aType.ToString();
            for (int i = 1; i <= Count; i++)
            {
                mem = Item(i);
                bKeep = mem.Type == aType;
                if (bChkStn && bKeep) { bKeep = mem.Stainless == bStns; }
                if (bChkMetr && bKeep) { bKeep = mem.Metric == bMetr; }
                if (bKeep) { _rVal.Add(mem); }

            }
            return _rVal;
        }


        public TMATERIALSPEC GetDefault(uppSpecTypes aType, bool bStainless, bool bMetric)
        {
            TMATERIALSPEC _rVal = new TMATERIALSPEC(uppSpecTypes.Undefined);

            TMATERIALSPEC aMem;
            bool bTest = false;
            int rIndex = -1;
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                bTest = aMem.Type == aType;
                if (bTest && aType != uppSpecTypes.Gasket)
                {
                    bTest = aMem.Stainless == bStainless;
                }
                if (bTest && (int)aType >= 100)
                {
                    bTest = aMem.Metric == bMetric;
                }
                if (bTest)
                {
                    rIndex = i - 1;
                    _rVal = aMem;
                    break;
                }
            }
            return _rVal;
        }

        public TMATERIALSPECS GetMaterialSpecs(TMATERIAL aMaterial)
        {
            TMATERIALSPECS _rVal = Clone(true);

            TMATERIALSPEC mem;
            uppSpecTypes sType;
            uppMaterialTypes mtype;
            bool bTest = false;

            sType = aMaterial.SpecType;
            mtype = aMaterial.Type;
            for (int i = 1; i <= Count; i++)
            {
                mem = Item(i);
                bTest = mem.Type == sType;
                if (bTest && sType != uppSpecTypes.Gasket)
                {
                    bTest = mem.Stainless == aMaterial.IsStainless;
                }
                if (bTest && mtype == uppMaterialTypes.Hardware)
                {
                    bTest = mem.Metric == aMaterial.IsMetric;
                }
                if (bTest)
                {
                    if (mem.AppliesTo(aMaterial)) { _rVal.Add(mem); }
                }
            }
            return _rVal;
        }


    }

    internal struct TINSTANCE
    {
        public double DX;
        public double DY;
        public double Rotation;
        public int Index;
        public bool Inverted;
        public bool LeftHanded;
        public double ScaleFactor;
        public string Tag;
        public int PartIndex;
        public uppAlternateRingTypes AltRingType;
        public bool Virtual;
        public int Row;
        public int Col;

        public TINSTANCE(double aDX, double aDY, double aRotation = 0, bool bInverted = false, double aScaleFactor = 1, bool bLeftHanded = false, int aPartIndex = 0, uppAlternateRingTypes aAltRingType = uppAlternateRingTypes.AllRings, bool bVirtual = false)
        {
            DX = aDX;
            DY = aDY;
            Rotation = aRotation;
            Index = 0;
            Inverted = bInverted;
            LeftHanded = bLeftHanded;
            ScaleFactor = aScaleFactor;
            Tag = string.Empty;
            PartIndex = aPartIndex;
            AltRingType = aAltRingType;
            Virtual = bVirtual;
            Row = 0;
            Col = 0;

        }
        public TINSTANCE(uopInstance aInstance)
        {
            DX = 0;
            DY = 0;
            Rotation = 0;
            Index = 0;
            Inverted = false;
            LeftHanded = false;
            ScaleFactor = 1;
            Tag = string.Empty;
            PartIndex = 0;
            AltRingType = uppAlternateRingTypes.AllRings;
            Virtual = false;
            Row = 0;
            Col = 0;

            if (aInstance == null) return;

            DX = aInstance.DX;
            DY = aInstance.DY;
            Rotation = aInstance.Rotation;
            Index = aInstance.Index;
            Inverted = aInstance.Inverted;
            LeftHanded = aInstance.LeftHanded;
            ScaleFactor = aInstance.ScaleFactor;
            Tag = aInstance.Tag;
            PartIndex = aInstance.PartIndex;
            AltRingType = aInstance.AltRingType;
            Virtual = aInstance.Virtual;
            Row = aInstance.Row;
            Col = aInstance.Col;
        }


        internal TINSTANCE(TINSTANCE aInstance)
        {
            DX = aInstance.DX;
            DY = aInstance.DY;
            Rotation = aInstance.Rotation;
            Index = aInstance.Index;
            Inverted = aInstance.Inverted;
            LeftHanded = aInstance.LeftHanded;
            ScaleFactor = aInstance.ScaleFactor;
            Tag = aInstance.Tag;
            PartIndex = aInstance.PartIndex;
            AltRingType = aInstance.AltRingType;
            Virtual = aInstance.Virtual;
            Row = aInstance.Row;
            Col = aInstance.Col;
        }
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
            return _rVal;
        }
        public TINSTANCE Clone() => new TINSTANCE(this);

        public bool Equals(TINSTANCE aInstance)
        {
            if (aInstance.DX != DX) return false;
            if (aInstance.DY != DY) return false;
            if (aInstance.Rotation != Rotation) return false;
            if (aInstance.LeftHanded != LeftHanded) return false;
            if (aInstance.Inverted != Inverted) return false;
            if (aInstance.ScaleFactor != ScaleFactor) return false;

            return true;

        }


    }


    internal struct TINSTANCES : ICloneable
    {
        private bool _Init;
        private int _Count;
        private TINSTANCE[] _Members;
        public string Name;
        public UVECTOR BasePt;
        public double BaseRotation;
        #region Constructors

        public TINSTANCES(string aName = "")
        {
            Name = String.IsNullOrWhiteSpace(aName) ? string.Empty : Name = aName.Trim();
            _Init = true;
            _Count = 0;
            _Members = new TINSTANCE[0];
            BasePt = UVECTOR.Zero;
            BaseRotation = 0;
        }

        public TINSTANCES(TINSTANCE[] aMembers)
        {
            BasePt = UVECTOR.Zero;
            _Count = 0;
            Name = string.Empty;
            BaseRotation = 0;
            _Members = aMembers;
            _Init = aMembers != null;
            if (_Init) { _Count = aMembers.Length; }

        }

        public TINSTANCES(uopInstances aInstances)
        {
            Name = string.Empty;
            _Init = true;
            _Count = 0;
            _Members = new TINSTANCE[0];
            BasePt = UVECTOR.Zero;
            BaseRotation = 0;
            if (aInstances == null) return;
            BasePt = new UVECTOR(aInstances.BasePt);
            Name = aInstances.Name;
            BaseRotation = aInstances.BaseRotation;
            foreach (uopInstance item in aInstances)
            {
                Add(new TINSTANCE(item));
            }

        }

        public TINSTANCES(TINSTANCES aInstances)
        {
            
            _Init = true;
            _Count = 0;
            _Members = new TINSTANCE[0];
            BasePt = new UVECTOR(aInstances.BasePt);
            Name = aInstances.Name;
            BaseRotation = aInstances.BaseRotation;

            for (int i = 1 ; i<=  aInstances.Count;i++)
            {
                Add(new TINSTANCE(aInstances.Item(i)));
            }

        }
        #endregion Constructors

        #region Properties
        
        public int Count { get { if (!_Init) { Init(); } return _Count; } }

        public int PartIndex { get => BasePt.PartIndex; set => BasePt.PartIndex = value; }

        #endregion Properties

        #region Methods
        public override string ToString() => $"TINSTANCES [{Count}]";

        private void Init()
        {
            _Init = true;
            _Count = 0;
            _Members = new TINSTANCE[0];
            BasePt = UVECTOR.Zero;
        }

        object ICloneable.Clone() => (object)Clone(false);

        public TINSTANCES Clone(bool bReturnEmpty = false)
        {
            if (!_Init) { Init(); }

            TINSTANCE[] mems = bReturnEmpty ? new TINSTANCE[0] : Force.DeepCloner.DeepClonerExtensions.DeepClone<TINSTANCE[]>(_Members);
            TINSTANCES _rVal = new TINSTANCES(mems) { Name = Name, BaseRotation = BaseRotation };

            return _rVal;
        }
        public bool Clear()
        {
            bool _rVal = Count > 0;

            _Init = true;
            _Count = 0;
            _Members = new TINSTANCE[0];
            BasePt = UVECTOR.Zero;
            return _rVal;
        }

        public TINSTANCE Item(int aIndex)
        {
            if (aIndex < 1 || aIndex > Count) return new TINSTANCE(0, 0);
            _Members[aIndex - 1].Index = aIndex;
            return _Members[aIndex - 1];
        }

        public bool Update(int aIndex, TINSTANCE aMember)
        {
            if (aIndex < 1 || aIndex > Count) return false;
            bool _rVal = !_Members[aIndex - 1].Equals(aMember);
            _Members[aIndex - 1] = aMember;
            _Members[aIndex - 1].Index = aIndex;
            return _rVal;

        }

        public void Add(double aDX, double aDY, double aRotation = 0, bool bInverted = false, double aScaleFactor = 1, bool bLeftHanded = false)
        {
            Add(new TINSTANCE(aDX, aDY, aRotation, bInverted, aScaleFactor, bLeftHanded));
        }

        public void Add(TINSTANCE aInstance)
        {
            if (Count + 1 > Int32.MaxValue) return;

            _Count += 1;
            Array.Resize<TINSTANCE>(ref _Members, _Count);
            _Members[_Count - 1] = new TINSTANCE(aInstance);
        }



        #endregion Methods
    }

}


