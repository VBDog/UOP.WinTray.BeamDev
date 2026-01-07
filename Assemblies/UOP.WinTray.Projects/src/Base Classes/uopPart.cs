using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using UOP.DXFGraphics;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Events;
using UOP.WinTray.Projects.Materials;
using UOP.WinTray.Projects.Parts;

using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;
using static Unity.Storage.RegistrationSet;

namespace UOP.WinTray.Projects
{

    public class PartEventArgs : EventArgs
    {

        public PartEventArgs(uppPartEventTypes aType, dynamic aValue = null, dynamic bValue = null, uopMaterial aMaterial = null)
        {
            _EventType = aType;
            AValue = aValue;
            BValue = bValue;
            _Material = aMaterial;
        }

        public PartEventArgs(uopProperty aChangedProperty)
        {
            _EventType = uppPartEventTypes.PropertyChange;
            _Property = aChangedProperty;
        }
        readonly uppPartEventTypes _EventType;
        public uppPartEventTypes EventType { get => _EventType; }
        public dynamic AValue { get; set; }
        public dynamic BValue { get; set; }

        private uopPart _Part;
        public uopPart Part { get => _Part; set => _Part = value; }

        readonly uopMaterial _Material;
        public uopMaterial Material { get => _Material; }
        readonly uopProperty _Property;
        public uopProperty Property { get => _Property; }
    }



    public delegate void PartEvent(PartEventArgs e);



    /// <summary>
    /// the parent object for parts
    /// </summary>
    /// 
    public abstract class uopPart : INotifyPropertyChanged, iVector, ICloneable
    {




        #region Fields


        private WeakReference<uopProject> _Reference_Project;
        private WeakReference<uopColumn> _Reference_Column;
        private WeakReference<uopTrayRange> _Reference_Range;
        internal TPROPERTIES _Properties;
        internal TPROPERTIES _Notes;
        internal List<string> _RangeIDS;

        //private TTRAYRANGE _Range;
        //private TPROJECT _Project;
        //private TCOLUMN _Column;

        #endregion Fields
        static readonly Version App = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;


        public event EventHandler<PartEventArgs> PartEvent;

        public event PropertyChangedEventHandler PropertyChanged;


        //The event-invoking method that derived classes can override.
        protected virtual void OnPartChange(PartEventArgs e)
        {
            // Safely raise the event for all subscribers
            if (e == null) return;
            if (e.Property != null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(e.Property.Name));
                e.Property.SubPart(this);
            }
            else
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(e.EventType.GetDescription()));
            }

            e.Part = this;

            PartEvent?.Invoke(this, e);
        }

        #region Constructors

        public virtual void Destroy() { }


        public uopPart(uppPartTypes partType, uppProjectFamilies aProjectFamily, string aProjectHandle = "", string aRangeGUID = null, bool bIsSheetMetal = false)
        {

            Init(partType);
            ProjectType = aProjectFamily.ProjectType();
            if (!string.IsNullOrEmpty(aRangeGUID)) _RangeGUID = aRangeGUID;
            _ProjectHandle = aProjectHandle;
            IsSheetMetal = bIsSheetMetal;
            uopPart.SetPartDefaults(this, _RangeGUID);
           
            _Instances = new TINSTANCES("");

        }
      
        public uopPart(uopPart aPartToCopy, string aProjectHandle = "", string aRangeGUID = null, bool bIsSheetMetal = false)
        {

            Init(aPartToCopy != null ? aPartToCopy.PartType : uppPartTypes.Undefined);
            if (!string.IsNullOrEmpty(aRangeGUID)) _RangeGUID = aRangeGUID;
            _ProjectHandle = aProjectHandle;

            if (aPartToCopy != null)
            {
                Copy(aPartToCopy);

            }
            IsSheetMetal = bIsSheetMetal;
            _Instances = new TINSTANCES("");
        }



        private void Init(uppPartTypes partType)
        {

            DesignFamily = uppMDDesigns.Undefined;
            _RangeGUID = string.Empty;
            AlternateRingType = uppAlternateRingTypes.AllRings;
            _ProjectHandle = string.Empty;
            _Indices = new TPARTINDICES();
            _Properties = new TPROPERTIES(partType);
            _Notes = new TPROPERTIES(partType);
            _HardwareMaterial = TMATERIAL.DefaultHardware();
            _SheetMetal = new TMATERIAL(uppMaterialTypes.SheetMetal);
            _TubeMaterial = new TMATERIAL(uppMaterialTypes.Tubing);
            _RangeIDS = new List<string>();
            _ParentPartNumbers = new List<string>();
            _Center = uopVector.Zero;

            _StackPattern = uppStackPatterns.Undefined;
            ParentPartType = uppPartTypes.Undefined;
            _ParentPath = string.Empty;
            _ManholeID = null;
            ParentPartIndex = 0;
            _RangeHardwareMaterial = TMATERIAL.DefaultHardware();
            PartType = partType;

            _PartNumber = string.Empty;
            _OverridePartNumber = string.Empty;
            SuppressEvents = false;
            LastSaveDate = string.Empty;
            _Reading = false;
            Category = string.Empty;

            DescriptiveName = string.Empty;
            IsCommon = false;
            Suppressed = false;
            Invalid = false;
            WeldedInPlace = false;
            Selected = false;
            IsVisible = true;
            Requested = false;
            RadialDirection = dxxRadialDirections.TowardsCenter;
            Direction = dxxOrthoDirections.Up;
            _DisplayUnits = uppUnitFamilies.Metric;
            _DrawingUnits = uppUnitFamilies.Metric;
            _Weight = 0;
            _Name = string.Empty;
            Tag = string.Empty;
            Flag = string.Empty;
            SubType = 0;
            UniqueIndex = 0;
            _OccuranceFactor = 1;
            _Quantity = 1;
            _TotalQuantity = 0;
            Side = uppSides.Undefined;
            End = uppSides.Undefined;
            Length = 0;
            Height = 0;
            Width = 0;
            Radius = 0;
            Diameter = 0;
            bSupWuz = false;
            Angle = 0;
            SparePercentage = 0;
            _PercentOpen = 0;
            NodePath = string.Empty;
            Mark = false;
            IsSheetMetal = false;
            IsMetric = false;
            
            ProjectType = uppProjectTypes.Undefined;
            DesignFamily = uppMDDesigns.Undefined;
            Col = 1;
            Row = 1;
        }

        #endregion Constructors

        #region Properties


        public virtual int Col { get; set; }

        public virtual int Row { get; set; }

        public virtual uopProject Project
        {
            get
            {
                uopProject _rVal = null;
                if (_Reference_Project == null && !string.IsNullOrWhiteSpace(ProjectHandle))
                {
                    _rVal = uopEvents.RetrieveProject(ProjectHandle);
                    if (_rVal != null)
                        _Reference_Project = new WeakReference<uopProject>(_rVal);
                    else
                        ProjectHandle = string.Empty;
                    return _rVal;
                }
                if (_Reference_Project != null)
                {
                    if (!_Reference_Project.TryGetTarget(out _rVal))
                    { _Reference_Project = null; ProjectHandle = string.Empty; }
                    else
                    { ProjectHandle = _rVal.Handle; }
                }

                return _rVal;
            }
        }

        public virtual uopColumn Column
        {
            get
            {
                uopColumn _rVal = null;
                if (_Reference_Column == null && !string.IsNullOrWhiteSpace(ColumnHandle))
                {
                    _rVal = uopEvents.RetrieveColumn(ColumnHandle);
                    if (_rVal != null)
                        _Reference_Column = new WeakReference<uopColumn>(_rVal);
                    else
                        ColumnHandle = string.Empty;

                    return _rVal;
                }

                if (_Reference_Column != null)
                {
                    if (!_Reference_Column.TryGetTarget(out _rVal))
                    { _Reference_Column = null; ColumnHandle = string.Empty; }
                    else
                    { ColumnHandle = _rVal.Handle; }
                }

                return _rVal;
            }
        }

        public virtual uopTrayRange TrayRange
        {
            get
            {
                uopTrayRange _rVal = null;
                if (_Reference_Range == null && !String.IsNullOrWhiteSpace(RangeGUID))
                {
                    _rVal = uopEvents.RetrieveRange(RangeGUID);
                    if (_rVal != null)
                        _Reference_Range = new WeakReference<uopTrayRange>(_rVal);
                    else
                        RangeGUID = string.Empty; ;
                    return _rVal;

                }

                if (_Reference_Range != null)
                {
                    if (!_Reference_Range.TryGetTarget(out _rVal))
                    { _Reference_Range = null; RangeGUID = string.Empty; }
                    else
                    { RangeGUID = _rVal.GUID; }
                }
                return _rVal;
            }

            set => _Reference_Range = value == null ? null : new WeakReference<uopTrayRange>(value);
        }


        internal TINSTANCES _Instances;
        public virtual uopInstances Instances { get => new uopInstances(_Instances, this); set => _Instances = new TINSTANCES(value); }

        public uopVector ParentCenter { get => new uopVector(ParentCenterV); set => ParentCenterV = new UVECTOR(value); }
        internal UVECTOR ParentCenterV { get; set; }

        //the weight of the part in english pounds

        private double _Weight;
        public virtual double Weight { get => _Weight; set => _Weight = mzUtils.VarToDouble(value, bAbsoluteVal: false, aPrecis: 4); }

        //flag indicating that the part is welded onto its parent
        public virtual bool WeldedInPlace { get; set; }


        //the width of the Part
        public virtual double Width { get; set; }

        public virtual int UniqueIndex { get; set; }

        internal TMATERIAL _TubeMaterial;
        //the tube material assigned to the part
        public virtual uopTubeMaterial TubeMaterial { get => new uopTubeMaterial(_TubeMaterial, this); set => _TubeMaterial = value != null ? value.Structure : new TMATERIAL(uppMaterialTypes.Tubing); }


        public virtual int SubType { get; set; }

        public virtual bool Suppressed { get; set; }

        private bool _SuppressEvents;
        public virtual bool SuppressEvents { get => _SuppressEvents || Reading; set => _SuppressEvents = value; }

        //provided to carry additional info about the part
        public virtual string Tag { get; set; }

        private int _TotalQuantity;
        //the parts quantity multipled by its parent traysection tray count
        public virtual int TotalQuantity { get { int _rVal = _TotalQuantity; if (_rVal == 0) _rVal = Quantity * TotalTrayCount; return _rVal; } set { _TotalQuantity = value; } }

        //the numbers of trays in the parent range

        public virtual int TrayCount => uopUtils.TrayCount(RingStart, RingEnd, StackPattern);

        //a string of range GUIDS that this part is associated to
        public string RangeList
        {
            get
            {
                List<string> guids = new List<string>();
                guids.AddRange(_RangeIDS);
                if (!string.IsNullOrWhiteSpace(RangeGUID))
                {
                    if (guids.FindIndex((x) => string.Compare(RangeGUID, x, true) == 0) == -1)
                    {
                        guids.Add(RangeGUID);
                    }
                }
                return mzUtils.ListToString(guids, ",");
            }
        }

        public virtual uppPartTypes ParentPartType { get; set; }

        public int ParentPartIndex { get; set; }

        private uopRingRanges _RingRanges;
        public uopRingRanges RingRanges { get => _RingRanges ?? new uopRingRanges(RingRange); set => _RingRanges = value; }
        public bool HasRingRanges => _RingRanges != null && _RingRanges.Count > 0;

        public virtual uopRingRange RingRange { get => new uopRingRange(RingStart, RingEnd, StackPattern); set { if (value == null) return; RingStart = value.RingStart; RingEnd = value.RingEnd; StackPattern = value.StackPattern; } }

        //the angle of the Part
        public virtual double Angle { get; set; }


        public string Category { get; set; }


        //the index of the chimney tray that this part belongs to
        public virtual int ChimneyTrayIndex { get => _Indices.ChimneyTrayIndex; set => _Indices.ChimneyTrayIndex = value; }

        public virtual bool Mark { get; set; }

        public virtual string INIPath => PartName.ToUpper();
        public virtual int ParentPartCount => _ParentPartNumbers.Count;

        public abstract uopPart Clone(bool aFlag = false);

        public virtual int PartNumberIndex { get => _Indices.PartNumberIndex; set => _Indices.PartNumberIndex = value; }

        //returns a copy of the current properties
        public uopProperties Properties => new uopProperties(ActiveProps, aPart:this);




        //the index of the column that this part is associated to
        public virtual int ColumnIndex { get => _Indices.ColumnIndex; set => _Indices.ColumnIndex = value; }

        //the index of the column that this part is associated to
        public virtual string ColumnLetter { get { uopColumn aColumn = Column; return (aColumn != null) ? aColumn.ColumnLetter : ""; } set { uopColumn aColumn = Column; if (aColumn != null) aColumn.ColumnLetter = value; } }


        private string _DisplayName;
        public virtual string DisplayName
        {
            get => string.IsNullOrWhiteSpace(_DisplayName) ? PartName : _DisplayName;
            set => _DisplayName = value;
        }

        public virtual string UniqueID { get; set; }



        /// <summary>
        ///returns a string that contains the the ring numbers occupied by the range
        //~I.E. 1,2,3,4,5 or 1,3,5,7 or 6,8,10,12 etx.
        /// </summary>
        public virtual string RingString
        {
            get
            {
                string _rVal = string.Empty;
                int Rng = RingStart;
                int lastrng = RingEnd;
                int step = (StackPattern == uppStackPatterns.Continuous) ? 1 : 2;

                while (!(Rng > lastrng))
                {
                    if (_rVal !=  string.Empty) _rVal += ",";
                    _rVal += Rng.ToString();

                    Rng += step;
                }


                return _rVal;
            }
        }


        public mdProject MDProject { get { return (ProjectFamily == uppProjectFamilies.uopFamMD) ? uopEvents.RetrieveMDProject(ProjectHandle) : null; } }


        public string SheetMetalSpec { get => _SheetMetal.Spec; set => _SheetMetal.Spec = value.Trim(); }

        public string HardwareSpec
        {
            get
            {
                if (string.IsNullOrEmpty(_HardwareMaterial.Spec)) _HardwareMaterial.Spec = uopMaterialSpecs.GetMaterialDefaultV(_HardwareMaterial);
                return _HardwareMaterial.Spec;
            }
            set
            {
                _HardwareMaterial.Spec = value.Trim();
                if (!string.IsNullOrEmpty(_HardwareMaterial.Spec)) _HardwareMaterial.Spec = uopMaterialSpecs.GetMaterialDefaultV(_HardwareMaterial);
            }
        }

        private uppStackPatterns _StackPattern;
        public virtual uppStackPatterns StackPattern
        {
            get => _StackPattern == uppStackPatterns.Undefined ? (uppStackPatterns)PropValI("StackPattern", uppPartTypes.TrayRange, aDefault: (int)uppStackPatterns.Continuous) : _StackPattern;
            set => _StackPattern = value;
        }
        public virtual double DLLVersion
        {
            get
            {
                uopProject proj = Project;

                return proj == null ? 0 : proj.DLLVersion;
            }
            set { uopProject proj = Project; if (proj != null) proj.DLLVersion = value; }
        }

        public virtual uopSheetMetal DeckMaterial { get { uopTrayAssembly aAssy = GetTrayAssembly(); return (aAssy != null) ? (uopSheetMetal)aAssy.DeckObj.Material : uopGlobals.goSheetMetalOptions().GetByFamilyAndGauge(uppMetalFamilies.CarbonSteel, uppSheetGages.Gage12); } }

        //returns the downcomer material of the first tray range in the project
        public virtual uopSheetMetal DowncomerMaterial { get { uopTrayAssembly aAssy = GetTrayAssembly(); return (aAssy != null) ? (uopSheetMetal)aAssy.DowncomerObj.Material : uopGlobals.goSheetMetalOptions().GetByFamilyAndGauge(uppMetalFamilies.CarbonSteel, uppSheetGages.Gage12); } }


        public virtual double RingThickness
        {
            get
            {
                double colval = PropValD("RingThickness", uppPartTypes.Column);
                double _rVal = PropValD("RingThk", uppPartTypes.TrayRange);
                if (colval == 0 && _rVal == 0) return 0;
                if (_rVal <= 0)
                {
                    _rVal = colval;

                }
                else
                {
                    if (_rVal == colval) PropValSet("RingThk", 0);
                }
                return _rVal;
            }

        }


        public virtual double ShellID { get => PropValD("ShellID", uppPartTypes.TrayRange); set => PropValSet("ShellID", value); }
        public virtual double ShellRadius => ShellID / 2;

        //the ring ID associated to the part
        public virtual double RingID { get => PropValD("RingID", uppPartTypes.TrayRange); set => PropValSet("RingID", value); }
        public virtual double RingRadius => RingID / 2; 

        public virtual double SparePercentage { get; set; }

        public uppRingClipSizes RingClipSize { get { return (ShellID <= uopGlobals.RingClipSizeChangeLimit) ? uppRingClipSizes.ThreeInchRC : uppRingClipSizes.FourInchRC; } }

        public virtual double DefaultRingClipClearance { get { return (RingClipSize == uppRingClipSizes.FourInchRC) ? 1.375 : 1.125; } }

        public virtual string DescriptiveName { get; set; }

        public virtual string Description { get; set; }
        /// <summary>
        /// the type of MD design that this tray configured to
        /// ~default = uopMDDesign
        /// </summary>

        public virtual uppMDDesigns DesignFamily { get; set; }


        //the type of MD design that this tray configured to in String Format
        //default = "MD"
        public string DesignFamilyName => uopUtils.DesignFamilyName(ProjectFamily, Configuration, DesignFamily);

        //the diameter of the Part
        //this is not associatied to the radius property
        public virtual double Diameter { get; set; }

        private uppUnitFamilies _DisplayUnits;
        public uppUnitFamilies DisplayUnits { get => _DisplayUnits; set { if (_DisplayUnits == value) { return; } _DisplayUnits = value; if (!_SuppressEvents && !_Reading) { OnPartChange(new PartEventArgs(uppPartEventTypes.DisplayUnitChange, _DisplayUnits, string.Empty)); } } }

        private uppUnitFamilies _DrawingUnits;
        public virtual uppUnitFamilies DrawingUnits { get => _DrawingUnits; set { if (_DrawingUnits == value) { return; } _DrawingUnits = value; if (!_SuppressEvents && !_Reading) { OnPartChange(new PartEventArgs(uppPartEventTypes.DrawingUnitChange, _DrawingUnits, string.Empty)); } } }


        private dxxOrthoDirections _Direction;
        //the direction of the part
        public virtual dxxOrthoDirections Direction { get => _Direction; set { if ((int)value >= 0 && (int)value <= 3) _Direction = value; } }


        //the index of the distributor that this part belongs to

        public virtual int DistributorIndex { get => _Indices.DistributorIndex; set => _Indices.DistributorIndex = value; }

        internal List<string> _ParentPartNumbers;
        public string ParentList { get => mzUtils.ListToString(_ParentPartNumbers, ","); set => _ParentPartNumbers = mzUtils.StringsFromDelimitedList(value); }

        private uppUnitFamilies? _Bolting;
        public virtual uppUnitFamilies Bolting
        {
            get
            {
                if (!_Bolting.HasValue)
                {
                    uopProject myProj = Project;
                    return (myProj != null) ? myProj.Bolting : uppUnitFamilies.Metric;

                }
                return _Bolting.Value;
            }

            set => _Bolting = value;
        }

        public virtual uppProjectTypes ProjectType { get; set;  }


        public virtual uppProjectFamilies ProjectFamily => ProjectType.Family();

        public virtual uppTrayConfigurations Configuration
        {
            get
            {
                TTRAYRANGE range = RangeStructure();

                return (range.Properties.Count > 0 && ProjectFamily == uppProjectFamilies.uopFamXF) ? uopEnums.GetEnumValue<uppTrayConfigurations>(range.Properties.ValueI("Configuration")) : uppTrayConfigurations.MultipleDowncomer;
            }

        }


        //the height of the Part
        public virtual double Height { get; set; }


        //the rotation of the Part
        public virtual double Rotation { get => Center.Value; set => Center.Value = value; }
        //flag indicating that the part is no longer valid
        private bool _Invalid;
        public virtual bool Invalid { get { return _Invalid; } set { if (_Invalid == value) { return; } { _Invalid = value; if (_Invalid) { OnPartChange(new PartEventArgs(uppPartEventTypes.PartInvalidated, string.Empty, string.Empty)); } } } }



        public virtual bool IsBeam => (PartType == uppPartTypes.DeckBeam) || (PartType == uppPartTypes.IntegralBeam) || (PartType == uppPartTypes.DowncomerBeam);

        //flag indicating that the part can be shared among several different trays
        public virtual bool IsCommon { get; set; }

        public virtual bool IsDesignObject => (int)PartType >= 3000;

        //true if the part is hardware
        public virtual bool IsHardware => (int)PartType < 0;

        private bool _IsMetric;
        public virtual bool IsMetric
        {
            get { if (!_IsMetric) { uopMaterial aMAT = Material; if (aMAT != null) { return aMAT.IsMetric; } } return _IsMetric; }
            set { _IsMetric = value; _HardwareMaterial.IsMetric = value; }
        }

        /// <summary>
        /// flag indicating that the part is constructed of sheet metal
        /// </summary>

        public virtual bool IsSheetMetal { get; set; }

        /// <summary>
        /// indicates that the part has no physical part definition or is a mirror copy of a physical part
        /// </summary>
        public virtual bool IsVirtual { get; set; }

        //indicates that the part is the base property source for children of the part
        public virtual bool IsGlobal { get; set; }

        //returns true if the parts material is stainless steel
        public virtual bool IsStainless => Material.IsStainless;

        //the length of the Part
        public virtual double Length { get; set; }

        //the manhole ID of the parent ray range
        private double? _ManholeID;
        public virtual double ManholeID { get => _ManholeID ?? PropValD("ManholeID", uppPartTypes.TrayRange); set => _ManholeID = value; }

        //returns the material of the part
        public virtual uopMaterial Material
        {
            get
            {
                if ((int)PartType < 0 && !IsSheetMetal) { return HardwareMaterial; }
                return (PartType == uppPartTypes.SpacerTube) ? (uopMaterial)TubeMaterial : (uopMaterial)SheetMetal;
            }
            set
            {
                if (value == null) return;
                if (value.MaterialType == uppMaterialTypes.SheetMetal) SheetMetalStructure = value.Structure;
                if (value.MaterialType == uppMaterialTypes.Hardware) HardwareMaterial = (uopHardwareMaterial)value;
                if (value.MaterialType == uppMaterialTypes.Tubing) _TubeMaterial = value.Structure;


            }
        }


        //provided to carry additional info about the part
        public virtual string Flag { get; set; }

        internal TMATERIAL _SheetMetal;
        //the sheet metal assigned to the part
        internal virtual TMATERIAL SheetMetalStructure { get { _SheetMetal.PartType = PartType; return _SheetMetal; } set { _SheetMetal = value; _SheetMetal.PartType = PartType; } }

        public virtual double Thickness => Material.Thickness;

        //determines if a part exists on all rings or on alternate rings
        public virtual uppAlternateRingTypes AlternateRingType { get; set; }

        public virtual bool InstalledOnAlternateRing1 => AlternateRingType == uppAlternateRingTypes.AllRings || AlternateRingType == uppAlternateRingTypes.AtlernateRing1;

        public virtual bool InstalledOnAlternateRing2 => AlternateRingType == uppAlternateRingTypes.AllRings || AlternateRingType == uppAlternateRingTypes.AtlernateRing2;


        //the thickness of sheet metal assigned to the part
        public virtual double SheetMetalThickness => _SheetMetal.Thickness;

        //the metal family of sheet metal assigned to the part
        public virtual uppMetalFamilies SheetMetalFamily => _SheetMetal.Family;

        //the metal family of sheet metal assigned to the part
        public string SheetMetalFamilyName => _SheetMetal.FamilyName;

        //the thickness multiplied by the density
        //used to calculate weight given as surface are of the material
        public virtual double SheetMetalWeightMultiplier => _SheetMetal.WeightMultiplier;

        public virtual uppSides Side { get; set; }
        public virtual uppSides End { get; set; }

        //the date that the file was last saved to file
        public string LastSaveDate { get; set; }

        public virtual bool IsVisible { get; set; }

    


        //the sheet metal assigned to the part
        public uopSheetMetal SheetMetal
        {
            get { return new uopSheetMetal(_SheetMetal) { PartType = PartType }; }
            set
            {
                if (value == null) return;
                TMATERIAL newMAT = value.Structure;
                string aWas = _SheetMetal.Descriptor;
                string aNew = newMAT.Descriptor;
                if (string.Compare(aWas, aNew, ignoreCase: true) == 0) { return; }
                bool bDifSpec = string.Compare(_SheetMetal.Spec, newMAT.Spec, ignoreCase: true) != 0;

                if (PropertyCount() > 0)
                {
                    PropValSet("Material", aNew, bSuppressEvnts: true);
                }

                _SheetMetal = newMAT;
                if (!SuppressEvents && !Reading) OnPartChange(new PartEventArgs(uppPartEventTypes.SheetMetalChange1, aWas, aNew, SheetMetal));

            }
        }


        /// <summary>
        ///the nominal shell radius for the tray range
        ///used in beam length calcs.
        /// </summary>
        public virtual double NominalShellRadius => 0.99 * ShellID / 2;

        internal int? _RingEnd;
        public virtual int RingEnd { get => _RingEnd ?? PropValI("RingEnd", uppPartTypes.TrayRange); set => PropValSet("RingEnd", value); }

        public virtual double RingSpacing
        {
            get => PropValD("RingSpacing", uppPartTypes.TrayRange); set => PropValSet("RingSpacing", value);
        }

        internal int? _RingStart;
        public virtual int RingStart { get => _RingStart ?? PropValI("RingStart", uppPartTypes.TrayRange); set => PropValSet("RingStart", value); }

        /// <summary>
        /// the ring width associated to the part
        /// </summary>
        public virtual double RingWidth
        {
            get
            {
                double _rVal = 0;

                double sID, rid;
                sID = ShellID;
                rid = RingID;
                if (sID > 0 && rid > 0) _rVal = (sID - rid) / 2;
                return _rVal;
            }
        }

        private int _OccuranceFactor;
        /// <summary>
        /// the number of times the part occurs in the entire tray
        /// </summary>
        public virtual int OccuranceFactor { get => _OccuranceFactor; set =>_OccuranceFactor = value; }

        private string _OverridePartNumber;
        /// <summary>
        /// returns the override part number of the part
        /// </summary>
        public string OverridePartNumber 
        {
            get { _OverridePartNumber ??= string.Empty; return _OverridePartNumber; }
            set 
            { 
                _OverridePartNumber = value == null ? string.Empty : value.Trim();
                if (_OverridePartNumber.Contains("Stiff"))
                {
                    Console.WriteLine("HERE");
                }
            } 
        }

        public virtual int PanIndex { get => _Indices.PanIndex; set => _Indices.PanIndex = value; }

        /// <summary>
        /// The index of the deck panel that owns this part
        /// </summary>
        public virtual int PanelIndex { get => _Indices.PanelIndex; set => _Indices.PanelIndex = value; }




        private string _ParentPath;
        public string ParentPath { get => _ParentPath; set => _ParentPath = value.Replace("Ranges.Range", "Range"); }


        /// <summary>
        /// the index of the part if it is a collection member
        /// </summary>
        public virtual int PartIndex { get => GetPartIndex(); set => SetPartIndex(value); }

        public virtual dxfPlane Plane { get => new dxfPlane(Center); }

        public virtual int GroupIndex { get => _Indices.GroupIndex; set => _Indices.GroupIndex = value; }

        public virtual Enum SubPartType { get; set; }

        /// <summary>
        /// returns the name of the part
        /// </summary>
        public string PartName
        {
            get
            {
                string _rVal = uopEnums.Description(PartType);
                int idx = PartIndex;

                if (idx > 0) _rVal += "[" + idx + "]";
                return _rVal;

            }
        }
        
        /// <summary>
        /// returns the part number of the part
        /// </summary>
        internal string _PartNumber;
        public string PartNumber
        {
            get
            {

                string _rVal = OverridePartNumber;
                if (string.IsNullOrWhiteSpace(_rVal))
                    _rVal = _PartNumber;
                else
                    return _rVal;

                if (string.IsNullOrWhiteSpace(_rVal))
                    _rVal = uopPart.DefaultPartNumber(this);

                if (_rVal !=  string.Empty)
                {
                    if (PartType != uppPartTypes.FingerClip && PartType != uppPartTypes.RingClip)
                    {
                        _rVal += PropValS("ColumnLetter", uppPartTypes.Column);
                    }

                }

                return _rVal;
            }

            set
            {
                _PartNumber = value;
            }
        }

        public string PanPath => $"{TrayAssemblyPath}.ReceivingPans.Item({_Indices.PanIndex})";

        public string TrayAssemblyPath => RangePath + ".TrayAssembly";

        /// <summary>
        /// the index of the Range that this part is associated to
        /// </summary>
        public virtual int RangeIndex { get => _Indices.RangeIndex; set => _Indices.RangeIndex = value; }

        public string RangePath => $"Project.Column( {ColumnIndex}).Range({RangeIndex})";

        public uopProperties ActiveProperties => new uopProperties(ActiveProps, aPart: this);

        internal TPROPERTIES ActiveProps
        {
            get
            {

                _Properties.PartType = PartType;
                _Properties.RangeGUID = _RangeGUID;
                return _Properties;
            }

            set
            {
                _Properties = value;
                //    if (PartType == uppPartTypes.Project) { _Project.Properties = value; return; }
                //    else if (PartType == uppPartTypes.Column) { _Column.Properties = value; return; }
                //    else if (PartType == uppPartTypes.TrayRange) { _Range.Properties = value; return; }
                //    else _Properties = value;
            }
        }
        
        /// <summary>
        /// the required quantity of the part
        /// </summary>
        private int _Quantity;
        public virtual int Quantity { get => _Quantity; set => _Quantity = Math.Abs(value); }

        public virtual int Index { get => GetPartIndex(); set => SetPartIndex(value); }


        internal TMATERIAL _HardwareMaterial;

        /// <summary>
        /// the hardware material assigned to the part
        /// </summary>
        public virtual uopHardwareMaterial HardwareMaterial
        {
            get => new uopHardwareMaterial(_HardwareMaterial, this);

            set => _HardwareMaterial = value == null ? TMATERIAL.DefaultHardware() : value.Structure;
        }


        public virtual string ColumnHandle { get; set; }

        /// <summary>
        /// the hardware material assigned to the range that owns this part
        /// </summary>
        private TMATERIAL _RangeHardwareMaterial;
        public uopHardwareMaterial RangeHardwareMaterial
        {
            get
            {

                uopTrayRange range = TrayRange;
                return range == null ? HardwareMaterial : range.HardwareMaterial;
            }

            //set { if (value != null) _RangeHardwareMaterial = value.Structure; }
        }



        public virtual bool DoubleNuts { get { uopTrayAssembly aAssy = GetTrayAssembly(); return aAssy != null && aAssy.DoubleNuts; } }

        public virtual double ProjectSparePercentage { get { uopProject aProj = Project; return (aProj != null) ? aProj.SparePercentage : 5; } }

        public virtual string ProjectTypeName
        {
            get
            {
                if (ProjectFamily == uppProjectFamilies.uopFamXF) return "Cross Flow Project";
                else if (ProjectFamily == uppProjectFamilies.uopFamMD)
                {
                    if (ProjectType == uppProjectTypes.MDDraw) return "MD Draw Project";
                    else return "MD Spout Project";
                }
                else return "";
            }
        }


        /// <summary>
        /// the index of the downcomer that this part belongs to
        /// </summary>
        public virtual int DowncomerIndex { get => _Indices.DowncomerIndex; set => _Indices.DowncomerIndex = value; }

        /// <summary>
        /// the index of the deck section that this part belongs to
        /// </summary>
        public virtual int SectionIndex { get => _Indices.SectionIndex; set => _Indices.SectionIndex = value; }

        public string DowncomerPath => $"{TrayAssemblyPath}.Downcomers.Item({_Indices.DowncomerIndex})";

        /// <summary>
        /// the family of the sheet metal that the part is constructed of
        /// </summary>
        public string MaterialFamily
        {
            get => _SheetMetal.FamilyName;

            set => SheetMetal = (uopSheetMetal)uopGlobals.goSheetMetalOptions().Retrieve(value, _SheetMetal.GageName, _SheetMetal.Descriptor, _SheetMetal.Family, _SheetMetal.SheetGage);

        }
        
        /// <summary>
        /// the gage of the sheet metal that the part is constructed of
        /// </summary>
        public string MaterialGage
        {
            get => _SheetMetal.GageName;
            set => SheetMetal = (uopSheetMetal)uopGlobals.goSheetMetalOptions().Retrieve(_SheetMetal.FamilyName, value, _SheetMetal.Descriptor, _SheetMetal.Family, _SheetMetal.SheetGage);

        }

        public string MaterialName => Material.FriendlyName();

        //returns the type of material of the part
        //Sheet metal, hardware or tube material
        public uppMaterialTypes MaterialType
        {
            get
            {
                if ((int)PartType < 0) return uppMaterialTypes.Hardware;
                else if (PartType == uppPartTypes.SpacerTube) return uppMaterialTypes.Tubing;
                else return uppMaterialTypes.SheetMetal;
            }
        }

        /// <summary>
        /// returns the spec currently assigned to the parts material form the parent project material specs
        /// </summary>
        public string MaterialSpecName
        {
            get
            {
                return MaterialType switch
                {
                    uppMaterialTypes.Hardware => _HardwareMaterial.Spec,
                    uppMaterialTypes.Tubing => _TubeMaterial.Spec,
                    uppMaterialTypes.SheetMetal => _SheetMetal.Spec,
                    _ => uopMaterialSpecs.GetMaterialDefault(Material)
                };


            }
        }
        public virtual string SelectName { get => Name; }

        internal string _Name;
        public virtual string Name { get => _Name; set => _Name = value; }

        public string NodePath { get; set; }

        private string _NodeName;
        public virtual string NodeName { get => string.IsNullOrWhiteSpace(_NodeName) ? $"{PartType.Description()} {PartNumber}".Trim() : _NodeName; set => _NodeName = value; }

        /// <summary>
        /// the density of sheet metal assigned to the part
        /// </summary>
        public virtual double SheetMetalDensity => _SheetMetal.Density;

        public string SheetMetalDescriptor
        {
            get => _SheetMetal.Descriptor;

            set
            {
                value = value.Trim();
                if (!string.IsNullOrEmpty(value))
                {
                    uopSheetMetal aSMT = uopGlobals.goSheetMetalOptions().GetByDescriptor(uppMaterialTypes.SheetMetal, value);
                    if (aSMT != null)
                        SheetMetal = aSMT;
                }
            }
        }
        /// <summary>
        /// the part type of the part
        /// </summary>
        public virtual uppPartTypes PartType { get; internal set; }
        public abstract uppPartTypes BasePartType { get; }
        public void SetPartType(uppPartTypes aPartType, string aPN = "")
        {
            if (PartType == aPartType) return;
            uppPartTypes oVal = PartType;

            PartType = aPartType;
            PartNumber = aPN;
            IsCommon = false;
            _PartNumber = string.Empty;
            uopPart.SetPartDefaults(this, RangeGUID);

            if (oVal != uppPartTypes.Undefined) OnPartChange(new PartEventArgs(uppPartEventTypes.PartTypeChange, oVal, PartType, null));

        }


        /// <summary>
        /// the perforation open area for the part
        /// </summary>
        internal double _PercentOpen;
        public virtual double PercentOpen { get => _PercentOpen; set => _PercentOpen = value; }

        public uppUnitFamilies ProjectDisplayUnits { get { uopProject aProj = Project; return (aProj != null) ? aProj.DisplayUnits : _DisplayUnits; } }

        public uppUnitFamilies ProjectBoltingUnits { get { uopProject aProj = Project; return (aProj != null) ? aProj.Bolting : uppUnitFamilies.Metric; } }

        public string ProjectFileExtension
        {
            get
            {
                if (ProjectFamily == uppProjectFamilies.uopFamXF)
                { return "XFP"; }
                else if (ProjectFamily == uppProjectFamilies.uopFamMD)
                {
                    return (ProjectType == uppProjectTypes.MDDraw) ? "MDD" : "MDP";
                }
                else return "";
            }
        }

        public string ProjectFolder { get => ProjectStructure().OutputFolder; }


        private string _ProjectHandle;
        /// <summary>
        /// the handle of the project that owns this object
        /// </summary>
        public virtual string ProjectHandle { get => _ProjectHandle; set => _ProjectHandle = value; }

        public virtual string ProjectName => ProjectStructure().Name;


        //the bend allowance used in the flat plate drawing of the beam
        //Default = 1.7 * material thickness.
        public virtual double BendAllowance => uopGlobals.goGlobalVars().ValueD("BendFactor", 1.7, SheetMetalThickness);

        /// <summary>
        /// the radial direction of the part
        /// </summary>
        public virtual dxxRadialDirections RadialDirection { get; set; }

        /// <summary>
        /// the radius of the Part
        /// </summary>
        public virtual double Radius { get; set; }

        private string _RangeGUID;
        public virtual string RangeGUID
        {
            get => _RangeGUID;
            set => _RangeGUID = value;
        }

        public virtual bool Requested { get; set; }

        private bool _Selected;
        public virtual bool Selected
        {
            get => _Selected;

            set
            {
                if (_Selected != value)
                {
                    _Selected = value;
                    int idx = PartIndex;
                    if (idx > 0 && !string.IsNullOrEmpty(ProjectHandle))
                    {
                        uopParts myCol = Collection();
                        myCol?.SetSelected(idx);

                    }
                }
            }
        }


        private bool _Reading; private bool bSupWuz;

        /// <summary>
        /// flag indicating if the part should notify its parent of changes to its properties
        /// </summary>
        public virtual bool Reading
        {
            get => _Reading;
            set
            {
                if (_Reading != value)
                {
                    _Reading = value;
                    if (value)
                    {
                        bSupWuz = SuppressEvents;
                        SuppressEvents = value;
                    }
                    else
                    {
                        SuppressEvents = bSupWuz;
                    }
                }
            }
        }

        /// <summary>
        /// the receiving pan object of the parent range
        /// </summary>
        public virtual uopPart ReceivingPan => (ProjectType != uppProjectTypes.CrossFlow) ? null : uopEventHandler.RetrieveReceivingPan(ProjectHandle, _Indices.ColumnIndex, RangeGUID, _Indices.PanIndex);

        public int TotalTrayCount
        {
            get
            {
                int _rVal = 0;
                List<string> guids = !string.IsNullOrWhiteSpace(RangeGUID) ? new List<string> { RangeGUID } : new List<string>(); guids.AddRange(_RangeIDS);

                string guid = RangeGUID;
                if (!string.IsNullOrWhiteSpace(guid))
                {
                    if (guids.FindIndex(x => string.Compare(x, guid, ignoreCase: true) == 0) < 0) guids.Add(guid);
                }

                uopTrayRange range;
                for (int i = 1; i <= guids.Count; i++)
                {
                    guid = guids[i - 1];
                    range = uopEvents.RetrieveRange(guid);
                    if (range != null) _rVal += range.TrayCount;
                }

                return _rVal;
            }
        }
        public string AbridgedParentPath
        {
            get
            {
                string _rVal = ParentPath.Replace($"Project.Column({ _Indices.ColumnIndex })." , "~");
                _rVal = _rVal.Replace(".TrayAssembly.", ".");
                _rVal = _rVal.Replace(".Item(", "(");
                return _rVal;
            }
        }


        public string AbridgedPath
        {
            get
            {
                string _rVal = GetPartPath().Replace($"Project.Column({ _Indices.ColumnIndex }).", "~");
                _rVal = _rVal.Replace(".TrayAssembly.", ".");
                _rVal = _rVal.Replace(".Item(", "(");
                return _rVal;
            }
        }
        public virtual double X { get => Center.X; set => Center.X = value; }

        public virtual double Y { get => Center.Y; set => Center.Y = value; }


        public virtual double Z { get => Center.Elevation.HasValue ? Center.Elevation.Value : 0; set => Center.Elevation = value; }


        internal uopVector _Center;
        public virtual uopVector Center { get { _Center ??= uopVector.Zero; return _Center; } set { if (value == null) { _Center.SetCoordinates(0, 0, 0); } else { _Center.SetCoordinates(value.X, value.Y, value.Elevation); } } }
        public virtual dxfVector CenterDXF
        {
            get => _Center.ToDXFVector(aTag: Tag, aFlag: Flag);

            set  => _Center = new uopVector(value) { Tag = Tag, Flag = Flag} ;
        }

        internal virtual UVECTOR CenterV { get => new UVECTOR(Center); set => _Center = new uopVector(value); }
        public virtual double PerforationDiameter { get; set; }



        /// <summary>
        /// the radius of the parts parent tray deck panels
        /// </summary>
        public virtual double DeckRadius
        {
            get
            {
                double _rVal = 0;
                TPROPERTIES rProps = GetParentProps(uppPartTypes.TrayRange);
                if (rProps.Count <= 0) return _rVal;

                double oride = rProps.ValueD("OverrideTrayDiameter");
                double sID = ShellID; double rid = RingID;
                if (sID <= 0 || rid <= 0) { UpdateRangeProperties(); sID = ShellID; rid = RingID; }
                if (sID > 0 && rid > 0)
                {
                    if (Math.Abs(oride) > 0)
                    {
                        if (oride < rid + 1) { oride = 0; }
                        if (oride > sID - 1) { oride = 0; }
                        if (oride == 0)
                        {
                            if (PartType == uppPartTypes.TrayRange) { this.PropValSet("OverrideTrayDiameter", 0); }
                            _rVal = (sID + rid) / 2 / 2;
                        }
                        else
                        { _rVal = oride / 2; }
                    }
                    else
                    { _rVal = (sID + rid) / 2 / 2; }
                }
                return _rVal;
            }
        }


        /// <summary>
        /// the ring radius less the ringclearance
        /// </summary>
        ///<remarks>this is also the ring clip bolt circle radius</remarks>
        public virtual double BoundingRadius { get { double _rVal = (RingID / 2) - RingClearance; return (_rVal < 0) ? 0 : _rVal; } }

        public virtual double RingClearance
        {
            get
            {
                double _rVal;
                TPROPERTIES rProps = GetParentProps(uppPartTypes.TrayRange);

                if (rProps.Count > 0)
                {
                    _rVal = rProps.ValueD("OverrideRingClearance");
                    if (_rVal <= 0) _rVal = uopUtils.BoundingClearance(ShellID);
                }
                else
                {
                    uopTrayRange range = TrayRange;
                    _rVal = (range != null) ? range.RingClearance : 0;
                }
                return _rVal;
            }
        }

        #endregion Properties

        #region Methods


        public virtual void ClearInstances() { _Instances.Clear(); }


        internal virtual TPROJECT ProjectStructure()
        {
            uopProject project = Project;
            return project == null ? new TPROJECT() : project.ProjectStructure();

        }
        internal virtual TCOLUMN ColumnStructure()
        {
            uopColumn column = Column;
            return column == null ? new TCOLUMN() : column.ColumnStructure();

        }


        internal virtual TTRAYRANGE RangeStructure()
        {
            uopTrayRange range = TrayRange;
            return range == null ? new TTRAYRANGE() : range.RangeStructure();

        }

        public uopParts Collection()
        {
            int idx = this.PartIndex;
            if (idx <= 0 || string.IsNullOrEmpty(ProjectHandle)) return null;
            uopParts _rVal = null;
            mdProject mdProj;
            mdTrayAssembly mdAssy;

            switch (PartType)
            {

                case uppPartTypes.TrayRange:
                    uopColumn aCol = Column;
                    _rVal = aCol?.TrayRanges;
                    break;
                case uppPartTypes.ChimneyTray:
                    mdProj = uopEvents.RetrieveMDProject(ProjectHandle);
                    _rVal = mdProj?.ChimneyTrays;
                    break;
                case uppPartTypes.Distributor:
                    mdProj = uopEvents.RetrieveMDProject(ProjectHandle);
                    _rVal = mdProj?.Distributors;
                    break;
                case uppPartTypes.Stage:
                    mdProj = uopEvents.RetrieveMDProject(ProjectHandle);
                    _rVal = mdProj?.Stages;
                    break;
                case uppPartTypes.DeckPanel:
                    if (ProjectFamily == uppProjectFamilies.uopFamMD)
                    {
                        mdAssy = GetMDTrayAssembly();
                        _rVal = mdAssy?.DeckPanels;
                    }
                    break;
                case uppPartTypes.DeckSections:
                    //if (ProjectFamily == uppProjectFamilies.uopFamMD)
                    //{
                    //    mdAssy = GetMDTrayAssembly();
                    //    _rVal = mdAssy?.DeckSections;
                    //}
                    //break;
                case uppPartTypes.DeckSplice:
                    //if (ProjectFamily == uppProjectFamilies.uopFamMD)
                    //{
                    //    mdAssy = GetMDTrayAssembly();
                    //    _rVal = mdAssy?.DeckSplices;
                    //}
                    break;

                case uppPartTypes.Downcomer:
                    if (ProjectFamily == uppProjectFamilies.uopFamMD)
                    {
                        _rVal = (colMDDowncomers)uopEvents.RetrieveDowncomers(RangeGUID);

                    }
                    break;
                case uppPartTypes.SpoutGroup:
                    if (ProjectFamily == uppProjectFamilies.uopFamMD)
                    {
                        _rVal = uopEvents.RetrieveSpoutGroups(RangeGUID);

                    }
                    break;

                case uppPartTypes.Constraints:
                    if (ProjectFamily == uppProjectFamilies.uopFamMD)
                    {
                        mdAssy = GetMDTrayAssembly();
                        _rVal = mdAssy?.Constraints;

                    }
                    break;

                default:
                    _rVal = null;
                    break;
            }
            return _rVal;

        }


        public hdwHexBolt SmallBolt(bool bWeldedInPlace, int aQuantity = 1, string aDescriptiveName = "", dynamic aObscuredLength = null)
        {
            hdwHexBolt _rVal = (Bolting == uppUnitFamilies.Metric) ? new hdwHexBolt(uppHardwareSizes.M10, RangeHardwareMaterial) : new hdwHexBolt(uppHardwareSizes.ThreeEights, RangeHardwareMaterial);
            _rVal.SubPart(this);
            _rVal.WeldedInPlace = bWeldedInPlace;
            _rVal.DoubleNut = DoubleNuts;
            _rVal.SparePercentage = SparePercentage;
            _rVal.IsVisible = this.IsVisible;
            _rVal.ObscuredLength = mzUtils.VarToDouble(aObscuredLength, aDefault: SheetMetalThickness);

            if (Bolting == uppUnitFamilies.Metric)
            { _rVal.Length = (!_rVal.DoubleNut) ? 25 / 25.4 : 30 / 25.4; }
            else
            { _rVal.Length = (!_rVal.DoubleNut) ? 1 : 1.25; }

            _rVal.Quantity = aQuantity;
            _rVal.DescriptiveName = aDescriptiveName;
            //_rVal.IsMetric = Bolting == uppUnitFamilies.Metric;
            _rVal.UpdateDimensions();
            return _rVal;

        }


        public virtual void UpdatePartProperties() { PropValSet("RangeGUID", RangeGUID, bSuppressEvnts: true); }

        public void UpdateRangeProperties()
        {
            uopTrayRange range = TrayRange;


        }
        public virtual double ComputeBeamLength(double aCenterOffset, bool bRoundDown)
        => uopUtils.ComputeBeamLength(aCenterOffset, BoundingRadius, bRoundDown);

        /// <summary>
        /// the tube material assigned to the part
        /// </summary>
        /// <returns></returns>
        public uopTubeMaterial GetTubeMaterial() => new uopTubeMaterial(_TubeMaterial);

        public virtual bool IsAssociatedToRange(string aGUID)
        {

            if (string.IsNullOrWhiteSpace(aGUID)) return false;
            if (!string.IsNullOrWhiteSpace(RangeGUID))
            {
                if (string.Compare(aGUID.Trim(), RangeGUID, true) == 0) 
                    return true;
            }
            if (_RangeIDS == null || _RangeIDS.Count <= 0) return false;
            return _RangeIDS.FindIndex((x) => string.Compare(aGUID, x, true) == 0) >= 0;

        }

        public virtual bool IsAssociatedToParent(string aParentHandle)
        {

            if (string.IsNullOrWhiteSpace(aParentHandle) || _ParentPartNumbers == null) return false;

            return _ParentPartNumbers.FindIndex((x) => string.Compare(x, aParentHandle, true) == 0) >= 0;

        }
        /// <summary>
        /// the parts quantity multipled by its parent traysection tray count and including the spares
        /// </summary>
        /// <returns></returns>
        public virtual int SpareQuantity()
        {
            int MinSprs = this.PropValI("MinSpares", uppPartTypes.Project, 2);
            return uopUtils.CalcSpares(TotalQuantity, SparePercentage, MinSprs);
        }

        public virtual void SubPart(uopProject aProject, string aCategory = null, bool? bHidden = null)
        {

            _ProjectHandle = aProject == null ? string.Empty : aProject.ProjectHandle;
            _Reference_Project = aProject == null ? null : new WeakReference<uopProject>(aProject);
            if (!string.IsNullOrWhiteSpace(aCategory)) { Category = aCategory; }
            if (bHidden.HasValue) Suppressed = bHidden.Value;
            if (aProject == null) return;
            ProjectType = aProject.ProjectType;

        }

        public virtual void SubPart(uopTrayRange aRange, string aCategory = null, bool? bHidden = null)
        {

            _ProjectHandle = aRange == null ? string.Empty : aRange.ProjectHandle;
            _RangeGUID = aRange == null ? string.Empty : aRange.GUID;
            _Reference_Range = aRange == null ? null : new WeakReference<uopTrayRange>(aRange);
            if (!string.IsNullOrWhiteSpace(aCategory)) { Category = aCategory; }
            if (bHidden.HasValue) Suppressed = bHidden.Value;
            if (aRange == null) return;
            SuppressEvents = aRange.SuppressEvents;
            bool spevnts = SuppressEvents;
            SuppressEvents = true;

            ColumnIndex = aRange.ColumnIndex;
            RangeIndex = aRange.Index;
            ColumnHandle = aRange.ColumnHandle;
            ProjectType = aRange.ProjectType;
            ParentPartType = aRange.PartType;
            ParentPartIndex = aRange.Index;
            DesignFamily = aRange.DesignFamily;
            RingStart = aRange.RingStart;
            RingEnd = aRange.RingEnd;
            StackPattern = aRange.StackPattern;
           
            DesignFamily = aRange.DesignFamily;
            _ManholeID = aRange.ManholeID;
            SuppressEvents = spevnts;
        }

        public virtual void SubPart(uopTrayAssembly aAssy, string aCategory = null, bool? bHidden = null)
        {

            if (aAssy != null)
            {
                SubPart(aAssy.TrayRange);
                _ManholeID = aAssy.ManholeID;

            }
            
        }

        public virtual void SubPart(uopColumn aColumn, string aCategory = null, bool? bHidden = null)
        {

            SubPart(aColumn.Project, aCategory, bHidden);
            ColumnHandle = aColumn == null ? string.Empty : aColumn.Handle;
            ColumnIndex = aColumn == null ? 0 : aColumn.Index;
            _Reference_Column = aColumn == null ? null : new WeakReference<uopColumn>(aColumn);
            if (aColumn == null) return;
            ProjectType = aColumn.ProjectType;

        }


        public virtual void SubPart(uopPart aPart, string aCategory = null, bool? bHidden = null)
        {
            if (aPart == null) return;
            try
            {

                _SuppressEvents = aPart._SuppressEvents;
                if (bHidden.HasValue) Suppressed = bHidden.Value;

                if (!string.IsNullOrWhiteSpace(aPart.ProjectHandle)) _ProjectHandle = aPart.ProjectHandle;
                if (!string.IsNullOrWhiteSpace(aPart.RangeGUID)) _RangeGUID = aPart.RangeGUID;
                if (!string.IsNullOrWhiteSpace(aPart.ColumnHandle)) ColumnHandle = aPart.ColumnHandle;
                if (!string.IsNullOrWhiteSpace(aCategory)) { Category = aCategory; }
                if (aPart.ProjectType != uppProjectTypes.Undefined)
                {
                    ProjectType = aPart.ProjectType;
                    
                }

                if (aPart.PartType != PartType)
                    ParentPartType = aPart.PartType;
                ParentPartIndex = aPart.Index;

                if (aPart.DesignFamily != uppMDDesigns.Undefined) DesignFamily = aPart.DesignFamily;
                _Indices.CopyPositiveIndices(aPart._Indices);

                if (aPart._RingStart.HasValue) _RingStart = aPart._RingStart.Value;
                if (aPart._RingEnd.HasValue) _RingEnd = aPart._RingEnd.Value;


            }
            catch (Exception e)
            {
                Debug.Fail(e.Message);
            }
            finally
            {


            }

        }


        public void SubPartProperties()
        {
            if (_Properties.Count <= 0) return;
            _Properties.SubPart(this);
            _Notes.SubPart(this);

        }

        public string TrayName(bool bIncludeDesignIndicator = false) => uopUtils.TrayName(ProjectFamily, RingStart, RingEnd, Configuration, DesignFamily, StackPattern, bIncludeDesignIndicator);

        /// <summary>
        /// sets the property in the collection with the passed name or index to the passed value
        /// </summary>
        /// <param name="aName">he name or key of the property to set</param>
        /// <param name="aPropVal"> the value to set the property value to</param>
        /// <param name="aLastVal">of passed, the new properties last value is set to this value</param>
        /// <returns></returns>
        public uopProperty CreateProperty(string aName, dynamic aPropVal, dynamic aLastVal = null)
        {
            TPROPERTY aProp = new TPROPERTY(aName, aValue: aPropVal, aUnitType: uppUnitTypes.Undefined, aPartType: PartType, aLastValue: aLastVal);
            aProp.SubPart(this);
            return new uopProperty(aProp);
        }

        /// <summary>
        /// raises the passed status string up to the project which will fire it's read status change event
        /// </summary>
        /// <param name="aStatus"></param>
        /// <param name="aIndex"></param>
        public void SetReadStatus(string aStatus, int aIndex = 0) { if (ProjectHandle !=  string.Empty) uopEvents.SetProjectReadStatus(ProjectHandle, aStatus, aIndex); }

        /// <summary>
        ///  raises the passed status string up to the project which will fire it's part generations status change event
        /// </summary>
        /// <param name="aStatus"></param>
        /// <param name="bBegin"></param>
        public void SetPartGenerationStatus(string aStatus, bool? bBegin = null) { if (ProjectHandle !=  string.Empty) uopEvents.SetPartGenenerationStatus(ProjectHandle, aStatus, bBegin); }

        //'#1flag to include the the stack pattern in the return
        //'^returns a descriptive name string for a span of a tray range
        //'~like "1-15 Odd"
        public string SpanName(bool bIncludeOddEven = true, string aPrefix = null) => uopUtils.SpanName(RingStart, RingEnd, StackPattern, bIncludeOddEven, aPrefix);




        //a collection of notes assigned to the part
        public uopProperty SetNotes(uopProperties newObj, bool bSuppressEvnts = false)
        {
            TPROPERTIES oldNotes = _Notes.Clone();

            TPROPERTIES newNotes = new TPROPERTIES(newObj);
            _Notes.Clear();
            string aNote;
            TPROPERTY aProp;
            for (int i = 1; i <= newNotes.Count; i++)
            {
                aProp = newNotes.Item(i);
                aNote = aProp.ValueS.Trim();
                if (!string.IsNullOrWhiteSpace(aNote)) _Notes.Add($"Note{_Notes.Count + 1}", aNote);


            }

            if (!TPROPERTIES.Compare(_Notes, oldNotes))
            {
                uopProperty SetNotes = uopProperty.Quick("NOTES", "NEW NOTES", "OLD NOTES", this);

                if (!_SuppressEvents && !bSuppressEvnts)
                {
                    OnPartChange(new PartEventArgs(SetNotes));
                    uopEvents.NotifyProject(ProjectHandle, SetNotes);
                }
                return SetNotes;
            }
            return null;
        }

        public virtual bool AddParentPartNumber(string aPartNum)
        {
            if (string.IsNullOrWhiteSpace(aPartNum)) return false;
            _ParentPartNumbers ??= new List<string>();
            if (_ParentPartNumbers.FindIndex((x) => string.Compare(x, aPartNum, true) == 0) >= 0) return false;
            _ParentPartNumbers.Add(aPartNum.Trim()); return true;
        }

        public virtual colUOPTrayRanges TrayRanges() => uopEvents.RetrieveRanges(ColumnHandle);


        internal virtual TINSTANCES Instances_Get() => _Instances;
        internal virtual void Instances_Set(TINSTANCES value) => _Instances = value;

        public int PropertyCount(uppPartTypes aPartType = uppPartTypes.Undefined)
        {


            if (aPartType == uppPartTypes.Undefined) aPartType = PartType;
            //if (aPartType == PartType)
            //{
            //    return _Properties.Count;
            //}
            //else
            //{
            switch (aPartType)
            {
                case uppPartTypes.Column:

                    return ColumnStructure().Properties.Count;

                case uppPartTypes.Project:

                    return ProjectStructure().Properties.Count;

                case uppPartTypes.TrayRanges:
                case uppPartTypes.TrayRange:
                    return RangeStructure().Properties.Count;



                default:
                    return _Properties.Count;

            }
            //}

        }

        //returns the objects properties in a collection
        //signatures like "Name=Project1"
        public virtual uopProperties CurrentProperties()
        {
            UpdatePartProperties();

            return new uopProperties(ActiveProps, aPart:this);
            //}
            //set { if (value != null) SetProps(value.Structure); }
        }

        public virtual void SetCoordinates(double? aX = null, double? aY = null, double? aZ = null)
        {
            if (aX.HasValue) X = aX.Value; if (aY.HasValue) Y = aY.Value; if (aZ.HasValue) Z = aZ.Value;
        }
        public virtual void SetCoordinates(uopVector aVector)
        {
            if (aVector != null) SetCoordinates(aVector.X, aVector.Y, aVector.Elevation);
        }
        public virtual bool SetCurrentProperties(uopProperties value)
        {
            return SetProps(value, true);

        }


        public string GetPartPath(bool bRangeOnly = false, string aSuffix = null)
        {
            return uopPart.BuildPartPath(PartType, _Indices, PartName, bRangeOnly, aSuffix);

        }
        internal void SetSelected(bool aSelectedVal) { _Selected = aSelectedVal; }


        public virtual bool IsEqual(uopPart aPart) { return aPart != null && aPart.PartType == PartType; }

        public override string ToString()
        {
            return $"{uopEnums.Description(PartType)}";


        }

        public virtual void UpdatePartWeight() { PartEvent?.Invoke(this, new PartEventArgs(uppPartEventTypes.UpdateBaseWeight)); }


        public int GetPartIndex(uppPartTypes aPartType = uppPartTypes.Undefined)
        {

            if (aPartType == uppPartTypes.Undefined) aPartType = PartType;
            return _Indices.GetIndex(aPartType);
        }


        public void SetPartIndex(int aIndex, uppPartTypes aPartType = uppPartTypes.Undefined)
        {

            if (aPartType == uppPartTypes.Undefined) aPartType = PartType;
            if(_Indices.SetIndex(aPartType, aIndex))
           {
                SubPartProperties();
              
            }

        }




        /// <summary>
        ///returns the properties required to save the object to file
        /// signatures like "COLOR=RED"
        /// </summary>
        public virtual uopPropertyArray SaveProperties(string aHeading = null)
        {
            uopProperties _rVal = CurrentProperties();
            aHeading = string.IsNullOrWhiteSpace(aHeading) ? this.PartPath() : aHeading.Trim();
            return new uopPropertyArray(_rVal, aName: aHeading, aHeading: aHeading);
        }



        private WeakReference<mdDowncomer> _DCRef;
        public virtual mdDowncomer GetMDDowncomer(mdTrayAssembly aAssy = null, mdDowncomer aDC = null, int aIndex = -1)
        {
            if (aDC != null) return aDC;
            if (_DCRef != null)
            {
                if (!_DCRef.TryGetTarget(out aDC)) _DCRef = null; else return aDC;
            }

            aAssy ??= GetMDTrayAssembly(aAssy);
            aIndex = (aIndex < 0) ? DowncomerIndex : aIndex;
            if (aAssy == null) return null;
            if (aIndex < 1 || aIndex > aAssy.Downcomer().Count) return null;

            aDC = aAssy.Downcomers.Item(aIndex);
            _DCRef = new WeakReference<mdDowncomer>(aDC);
            return aDC;
        }

        public virtual mdDeckPanel GetMDPanel(mdTrayAssembly aAssy = null, mdDeckPanel aPanel = null, int aIndex = -1)
        {
            if (aPanel != null) return aPanel;
            aAssy ??= GetMDTrayAssembly(aAssy);
            aIndex = (aIndex <= 0) ? PanelIndex : aIndex;
            if (aAssy == null) return null;
            if (aIndex < 1 || aIndex > aAssy.DeckPanels.Count) return null;
            return aAssy.DeckPanels.Item(aIndex);
        }

        public mdTrayRange GetMDRange(mdTrayRange aRange = null)
        {
            if (aRange != null) return aRange;
            uopTrayRange myrange = TrayRange;
            return myrange == null ? null : myrange.ProjectFamily == uppProjectFamilies.uopFamMD ? (mdTrayRange)myrange : null;


        }


        public virtual mdTrayAssembly GetMDTrayAssembly(mdTrayAssembly aAssy = null)
        {
            if (aAssy != null) return aAssy;
            uopTrayAssembly assy = GetTrayAssembly();

            return assy == null ? null : assy.ProjectFamily == uppProjectFamilies.uopFamMD ? (mdTrayAssembly)assy : null;

        }

        public mdProject GetMDProject(mdProject aProject = null) { if (aProject != null) return aProject; return (ProjectFamily != uppProjectFamilies.uopFamMD) ? null : this.MDProject; }

        public virtual uopTrayAssembly GetTrayAssembly(uopTrayAssembly aAssy = null)
        {
            try
            {
                if (aAssy != null) return aAssy;
                uopTrayRange range = TrayRange;

                return range?.Assembly;
            }
            catch { return null; }


        }

        public colUOPTrayRanges GetTrayRanges(colUOPTrayRanges aRanges = null)
        { if (aRanges != null) { return aRanges; } else { aRanges = this.TrayRanges(); return aRanges; } }

        public virtual uopDocuments Documents(uppDocumentTypes aType = uppDocumentTypes.Undefined) => new uopDocuments();

        public virtual uopDocuments Calculations() => new uopDocuments();


        public virtual uopDocuments Drawings() => new uopDocuments();

        public virtual void GetDrawing(uopDocuments aCollector, uppDrawingTypes aType, uppUnitFamilies aUnits) { }

        public virtual uopDocuments Warnings() => new uopDocuments();



        internal bool SetProps(uopProperties aProperties, bool bCopyMatchingNameVals = false)
        {
            if (aProperties == null) return false;
            if (aProperties.Count <= 0) return false;


            if (_Properties.Count <= 0) bCopyMatchingNameVals = false;
            if (bCopyMatchingNameVals)
            {
                _Properties.CopyValues(aProperties, out string changes, bCopyNonMembers: false);
                return !string.IsNullOrWhiteSpace(changes);

            }
            else
            {
                _Properties.CopyValues(aProperties, out string changes, bCopyNonMembers: true);
                return !string.IsNullOrWhiteSpace(changes);
            }

        }


        public virtual bool PropValB(dynamic aNameOrIndex, uppPartTypes aPartType = uppPartTypes.Undefined, bool aDefault = false)
        {
            if (aPartType == uppPartTypes.Undefined || aPartType == PartType) return _Properties.ValueB(aNameOrIndex, aDefault);
            TPROPERTIES props = GetParentProps(aPartType);
            var _rVal = props.ValueB(aNameOrIndex, aDefault);
            return _rVal;



        }

        public string PropValS(dynamic aNameOrIndex, uppPartTypes aPartType = uppPartTypes.Undefined, string aDefault = "", bool formatted = false)
        {
            if (aPartType == uppPartTypes.Undefined || aPartType == PartType) return _Properties.ValueS(aNameOrIndex, aDefault, formatted);
            TPROPERTIES props = GetParentProps(aPartType);
            var _rVal = props.ValueS(aNameOrIndex, aDefault, formatted);
            return _rVal;


        }
        public virtual int PropValI(dynamic aNameOrIndex, uppPartTypes aPartType = uppPartTypes.Undefined, int aDefault = 0)
        {
            if (aPartType == uppPartTypes.Undefined || aPartType == PartType) return _Properties.ValueI(aNameOrIndex, aDefault);

            TPROPERTIES props = GetParentProps(aPartType);
            int _rVal = props.ValueI(aNameOrIndex, aDefault);
            return _rVal;
        }
        public virtual double PropValD(dynamic aNameOrIndex, uppPartTypes aPartType = uppPartTypes.Undefined, double aDefault = 0, dynamic aMultiplier = null)
        {
            if (aPartType == uppPartTypes.Undefined || aPartType == PartType) return _Properties.ValueD(aNameOrIndex, aDefault, aMultiplier);


            TPROPERTIES props = GetParentProps(aPartType);
            double _rVal = props.ValueD(aNameOrIndex, aDefault, aMultiplier);
            if (_rVal <= 0)
            {
                if (aPartType == uppPartTypes.TrayRange)
                {
                    if (aNameOrIndex.GetType() == typeof(string))
                    {
                        if (string.Compare((string)aNameOrIndex, "ManholeID", true) == 0)
                        {
                            return PropValD(aNameOrIndex, uppPartTypes.Column, aDefault, aMultiplier);
                        }
                    }

                }
            }
            return _rVal;


        }

        public uopPart GetParent(uppPartTypes aPartType)
        {
            uopPart parent = aPartType switch
            {
                uppPartTypes.Column => Column,
                uppPartTypes.Project => Project,
                uppPartTypes.TrayRanges => GetTrayRanges(),
                uppPartTypes.TrayRange => TrayRange,
                _ => null
            };
            return parent;
        }

        internal TPROPERTIES GetParentProps(uppPartTypes aPartType)
        {
            uopPart parent = GetParent(aPartType);
            return parent == null ? new TPROPERTIES() : parent._Properties;
        }
        public dynamic PropValGet(dynamic aNameOrIndex, out bool rExists, bool bDecodedValue = false, uppPartTypes aPartType = uppPartTypes.Undefined)
        {
            rExists = true;
            if (aPartType == uppPartTypes.Undefined) { aPartType = PartType; }
            TPROPERTIES aProps = (aPartType == uppPartTypes.Undefined || aPartType == PartType) ? ActiveProps : GetParentProps(aPartType);

            rExists = aProps.TryGet(aNameOrIndex, out TPROPERTY myProp);
            if (!rExists)
            {
                TPROPERTIES.WriteToDebug(aProps, $"UOPPART PROPVALGET NOT FOUND - {aNameOrIndex}");
                return "";
            }

            dynamic _rVal = !bDecodedValue ? myProp.Value : myProp.DecodedValue;
            if (aPartType == uppPartTypes.TrayRange && rExists)
            {
                _rVal = myProp.ValueD;

                if (string.Compare(myProp.Name, "RingThk", true) == 0 && _rVal <= 0)
                { _rVal = PropValD("RingThk", uppPartTypes.Column); }
            }
            return _rVal;
        }

        /// <summary>
        /// the parent panel of this part
        /// </summary>
        /// <returns></returns>
        public uopPart DeckPanel(uopTrayAssembly aAssy = null) => uopEvents.RetrieveDeckPanel(RangeGUID, _Indices.PanelIndex, aAssy);

        /// <summary>
        /// the deck panels of the range of this part
        /// </summary>
        public virtual uopParts DeckPanelParts(uopTrayAssembly aAssy = null) => uopEvents.RetrieveDeckPanels(RangeGUID, aAssy);


        /// <summary>
        /// sets the property in the collection with the passed name or index to the passed value
        //returns the property if the property value actually changes.
        /// </summary>
        //public virtual uopProperty PropValSet(dynamic aNameOrIndex, dynamic aPropVal,  bool bSuppressErrors = false, bool? bSuppressEvnts = null, bool? bHiddenVal = null)
        //{

        //    uopProperty _rVal = null;
        //    ////TPROPERTIES aProps;

        //    //bool myprops = (aPartType == uppPartTypes.Undefined || aPartType == PartType);
        //    //aProps = myprops ? ActiveProps :  GetProps(aPartType);
        //    int aOccur = 0;
        //    //string alertName = "SpliceStyle";

        //    bool supEvnts = bSuppressEvnts ?? Reading || SuppressEvents  ;
        //    bool rChanged = false;
        //    bool propfound = _Properties.TryGet(aNameOrIndex, out TPROPERTY aProp, aOccur);


        //    if (propfound)
        //    {

        //        aProp.ValueChanged = false;
        //        rChanged = aProp.SetValue(aPropVal);
        //        if (bHiddenVal.HasValue)
        //        {
        //            if(aProp.Hidden != bHiddenVal.Value)
        //            {
        //                aProp.Hidden = bHiddenVal.Value;
        //                _Properties.SetItem(aProp.Index, aProp);
        //            }
        //        }
        //        if (rChanged)
        //        {


        //            aProp.ValueChanged = true;
        //            aProp.SubPart(this);
        //            _Properties.SetItem(aProp.Index, aProp);
        //            if(PartType == uppPartTypes.Project)
        //            {
        //                if (aProp.IsNamed("Revision,KeyNumber"))
        //                {
        //                    string path = ProjectFolder;
        //                    if (!string.IsNullOrWhiteSpace(path))
        //                    {
        //                        path = $"{path} \\{ ProjectName}.{ProjectFileExtension}";
        //                        _Properties.SetValue("DatafileName", path );
        //                    }
        //                }
        //            }



        //            _rVal = new uopProperty(aProp);



        //        }
        //        if (!supEvnts && _rVal != null)
        //            OnPartChange(new PartEventArgs(_rVal));

        //        return _rVal;
        //    }




        //    if (aNameOrIndex.GetType() == typeof(string))
        //    {

        //        string pname = (string)aNameOrIndex;


        //        if (string.Compare(pname, "SheetMetal.Spec", true) == 0)
        //        {
        //            string strlast = _SheetMetal.Spec;
        //            rChanged = string.Compare(strlast, _SheetMetal.Spec, true) != 0;
        //            if (rChanged)
        //            {
        //                _SheetMetal.Spec = aPropVal.ToString();
        //                _rVal = new uopProperty(pname, aPropVal, aLastValue: strlast);
        //            }
        //        } else if (string.Compare(pname, "RingStart", true) == 0)
        //        {
        //            int ilast = _RingStart ?? 0;
        //            int ival = mzUtils.VarToInteger(aPropVal, aDefault: ilast);

        //            rChanged = ilast != ival;
        //            if (rChanged)
        //            {
        //                _RingStart = aPropVal;
        //                _rVal = new uopProperty(pname, aPropVal, aLastValue: ilast);
        //            }
        //        } else if (string.Compare(pname, "RingEnd", true) == 0)
        //        {
        //            int ilast = _RingEnd ?? 0;
        //            int ival = mzUtils.VarToInteger(aPropVal, aDefault: ilast);

        //            rChanged = ilast != ival;
        //            if (rChanged)
        //            {
        //                _RingEnd = aPropVal;
        //                _rVal = new uopProperty(pname, aPropVal, aLastValue: ilast);
        //            }
        //        }
        //        if (!supEvnts  && rChanged) OnPartChange(new PartEventArgs(_rVal));

        //        return _rVal;
        //    }


        //    string msg = $"Property [{ aNameOrIndex }] Not Found On Part '{ PartPath() }'";
        //    //Debug.Fail(msg);
        //    if (!bSuppressErrors)
        //    {


        //        throw new Exception(msg);
        //    }


        //    return _rVal;
        //}

        /// <summary>
        // sets the parts property with the passed name to the passed value
        //returns the property if the property value actually changes.
        /// </summary>
        public virtual uopProperty PropValSet(string aName, object aPropVal, int aOccur = 0, bool? bSuppressEvnts = null, bool? bHiddenVal = null)
        {
            if (string.IsNullOrWhiteSpace(aName) || aPropVal == null) return null;
            uopProperty _rVal = null;
            bool supevnts = bSuppressEvnts.HasValue ? bSuppressEvnts.Value : SuppressEvents || Reading;
            bool rChanged = false;
            bool propfound = _Properties.TryGet(aName, out TPROPERTY aProp, aOccur);

            if (propfound)
            {
                aProp.ValueChanged = false;
                rChanged = aProp.SetValue(aPropVal);

                if (rChanged)
                {

                    aProp.ValueChanged = true;
                    aProp.SubPart(this);
                    _Properties.SetItem(aProp.Index, aProp);
                    if (PartType == uppPartTypes.Project)
                    {
                        if (aProp.IsNamed("Revision,KeyNumber"))
                        {
                            string path = ProjectFolder;
                            if (!string.IsNullOrWhiteSpace(path))
                            {
                                path = $"{path} \\{ProjectName}.{ProjectFileExtension}";
                                _Properties.SetValue("DatafileName", path);
                            }
                        }
                    }

                    _rVal = new uopProperty(aProp);

                }
            }
            int ilast = 0;
            int ival = 0;
            double dlast = 0;
            double dval = 0;

            switch (aName.ToUpper())
            {
                case "OFFSETFACTOR":
                    Console.WriteLine(aPropVal.ToString());
                    break;
                case "SHEETMETAL.SPEC":
                    string strlast = _SheetMetal.Spec;
                    rChanged = string.Compare(strlast, _SheetMetal.Spec, true) != 0;
                    if (rChanged)
                    {
                        _SheetMetal.Spec = aPropVal.ToString();
                        _rVal ??= new uopProperty(aName, aPropVal, aLastValue: strlast);
                    }
                    break;
                case "RINGSTART":
                    ilast = _RingStart ?? 0;
                    ival = mzUtils.VarToInteger(aPropVal, aDefault: ilast);

                    rChanged = ilast != ival;
                    if (rChanged)
                    {
                        _RingStart = ival;
                        _rVal ??= new uopProperty(aName, ival, aLastValue: ilast);
                    }
                    break;
                case "RINGEND":
                    ilast = _RingEnd ?? 0;
                    ival = mzUtils.VarToInteger(aPropVal, aDefault: ilast);

                    rChanged = ilast != ival;
                    if (rChanged)
                    {
                        _RingEnd = ival;
                        _rVal ??= new uopProperty(aName, ival, aLastValue: ilast);
                    }
                    break;

                case "X":
                    if (_rVal == null)
                    {
                        dval = mzUtils.VarToDouble(aPropVal);
                        dlast = Center.X;
                        _rVal = new uopProperty(aName, dval, aLastValue: dlast);

                    }
                    else
                    {
                        dval = _rVal.ValueD;
                    }
                    Center.X = dval;

                    break;
                case "Y":
                    if (_rVal == null)
                    {
                        dval = mzUtils.VarToDouble(aPropVal);
                        dlast = Center.Y;
                        _rVal = new uopProperty(aName, dval, aLastValue: dlast);

                    }
                    else
                    {
                        dval = _rVal.ValueD;
                    }
                    Center.Y = dval;
                    break;
            }



            if (_rVal == null) return _rVal;
            _rVal.SubPart(this);

            if (supevnts) return _rVal;

            OnPartChange(new PartEventArgs(_rVal));
            return _rVal;
        }

        /// <summary>
        /// extracts the parts property values from the passed file array that was read from an INI style project file.
        /// </summary>
        /// <param name="aProject">The project requesting the read event</param>
        /// <param name="aFileProps">The property array containing the INI file properties or a subset. The Name of the array should contain the original file name.</param>
        /// <param name="ioWarnings">A collection to populate if errors or warnings are found during the property value extraction  </param>
        /// <param name="aFileVersion">The version of th efile being read. Supplied to account for backward compatibility</param>
        /// <param name="aFileSection">the INI file heading to search for the properties to extract </param>
        /// <param name="bIgnoreNotFound">A flag to ignore properties that exist on the part but were not found in the file properties</param>
        /// <param name="aAssy">An optional parent tray assembly for the part being read</param>
        /// <param name="aPartParameter">An optional parent part for the part being read</param>
        /// <param name="aSkipList">An optional list of property names to skip over during the read</param>
        /// <param name="EqualNames">An optional dictionary of names aliases to test</param>
        /// <param name="aProperties">aset of properties to define my propertieswith rather than reading the from the passed property array</param>
        public virtual void ReadProperties(uopProject aProject, uopPropertyArray aFileProps, ref uopDocuments ioWarnings, double aFileVersion, string aFileSection = null, bool bIgnoreNotFound = false, uopTrayAssembly aAssy = null, uopPart aPartParameter = null, List<string> aSkipList = null, Dictionary<string, string> EqualNames = null, uopProperties aProperties = null)
        {
            TPROPERTIES myprops = ActiveProps;
            if (myprops.Count <= 0) return;

            try
            {
                uopProperties props = null;
                ioWarnings ??= new uopDocuments();
                if (aProperties == null)
                {
                    aFileSection = string.IsNullOrWhiteSpace(aFileSection) ? INIPath : aFileSection.Trim();
                    if (aFileProps == null) throw new Exception("The Passed File Property Array is Undefined");
                    if (string.IsNullOrWhiteSpace(aFileSection)) throw new Exception("The Passed File Section is Undefined");
                    if (!aFileProps.TryGet(aFileSection, out props)) throw new Exception($"The Passed File Property Array Does Not Contain '{aFileSection}'");

                }
                else
                {
                    props = aProperties;
                }

                aProject?.ReadStatus($"Reading '{aFileSection}' Properties", 2);

                Reading = true;


                for (int i = 1; i <= myprops.Count; i++)
                {
                    TPROPERTY prop = myprops.Item(i);
                    if (aSkipList != null)
                    {
                        if (aSkipList.FindIndex(x => string.Compare(x, prop.Name, true) == 0) >= 0)
                            continue;
                    }
                    if (prop.Protected)
                        continue;

                    if (!props.TryGet(prop.Name, out uopProperty fprop, EqualNames))
                    {

                        if (!bIgnoreNotFound)
                            if (!prop.Optional)
                            {
                                ioWarnings?.AddWarning(this, $"Property Not Found", $"Property '{prop.Name}' Was Not Found ");

                            }

                        continue;
                    }

                    if (prop.IsEnum)
                    {
                        prop.SetValue(fprop.ValueI);
                    }
                    else if (prop.HasUnits)
                    {

                        prop.SetValue(fprop.ValueD);

                    }
                    else
                    {
                        switch (prop.VariableTypeName.ToUpper())
                        {
                            case "BOOLEAN":
                            case "BOOL":
                                {
                                    prop.SetValue(fprop.ValueB);
                                    break;
                                }
                            case "STRING":
                                {
                                    prop.SetValue(fprop.ValueS);
                                    break;
                                }
                            case "SINGLE":
                            case "DOUBLE":
                                {
                                    prop.SetValue(fprop.ValueD);
                                    break;
                                }
                            case "LONG":
                            case "INT32":
                            case "INT64":
                            case "INT":
                                {
                                    prop.SetValue(fprop.ValueI);
                                    break;
                                }
                            default:

                                {

                                    prop.SetValue(fprop.ValueS);
                                    ioWarnings?.AddWarning(this, $"Property Variable Type Conversion Trouble", $"Property '{prop.Name}' Of type '{prop.VariableTypeName}");

                                    break;

                                }
                        }

                        //SPECIAL CASES

                        //material properties
                        if (prop.IsNamed("HardwareMaterial") && PartType == uppPartTypes.TrayRange)
                        {
                            uopHardwareMaterial hmat = null;

                            string sval = fprop.ValueS;
                            if (sval !=  string.Empty)
                            {
                                hmat = (uopHardwareMaterial)uopGlobals.goHardwareMaterialOptions().GetByMaterialName(uppMaterialTypes.Hardware, sval);
                                hmat ??= (uopHardwareMaterial)uopGlobals.goHardwareMaterialOptions().GetByFamilyName(uppMaterialTypes.Hardware, sval);
                                if (hmat == null)
                                {
                                    ioWarnings?.AddWarning(this, $"Unknown Hardware Material", $"Material '{sval}' Was Not found!");
                                }
                            }
                            hmat ??= HardwareMaterial;
                            hmat ??= (uopHardwareMaterial)uopGlobals.goHardwareMaterialOptions().Item(1);
                            HardwareMaterial = hmat;
                            prop.Value = RangeHardwareMaterial.MaterialName;

                        }
                        if (prop.IsNamed("Material"))
                        {
                            if (IsSheetMetal)
                            {
                                SheetMetal = (uopSheetMetal)uopUtils.ReadMaterialFromFile(aFileProps, aFileSection, prop.Name, out bool matfound, out bool matadded, uppMetalFamilies.CarbonSteel, uppSheetGages.Gage12);
                                if (!matfound && !matadded)
                                    ioWarnings.AddWarning(this, "Saved Material Warning", $"The Material '{prop.ValueS}' Could Not Be Found", uppWarningTypes.General);


                            }
                        }

                    }

                    myprops.SetItem(i, prop);

                }


            }
            catch (Exception e)
            {
                ioWarnings?.AddWarning(this, "Read Properties Error", e.Message);
            }
            finally
            {
                Reading = false;
                aProject?.ReadStatus("", 2);
                ActiveProps = myprops;

            }




        }





        /*'#1the name of the property to add
       '#2the value to assign to the new property
       '#3the index of the units object to assign to the property
       '#4flag to mark the new property as hidden
       '#5the heading to assign to the new property
       '#6the caption assigned to the property

       '^shorthand method for adding a property to the collection
       '~won't add a property with no name(no error raised).
       */


        internal TPROPERTY AddProperty(string aPropName, dynamic aPropVal, uppUnitTypes aUnitType = uppUnitTypes.Undefined, bool bIsHidden = false,
          string aHeading = "", string aDisplayName = "", string aDecodeString = "", bool bProtected = false, string aCategory = "", bool bIsShared = false,
          dynamic aNullVal = null, string aUnitString = "", bool bSetDefault = false, bool bOptional = false)
        {
            return _Properties.Add(aPropName, aPropVal, aUnitType, bIsHidden, aHeading, aDisplayName, aDecodeString, bProtected, aCategory, PartType, bIsShared, aNullVal: aNullVal, aUnitCaption: aUnitString, bSetDefault: bSetDefault, bOptional: bOptional);
        }

        //'#1the index to begin the procedure (default = 1)
        //'#2the index to end the procedure (default = count)
        //'^used to reset all or some of the member properties back to their current default values
        public void PropsReset(int aStartIndex = 0, int aEndIndex = 0)
        {
            _Properties.ResetToDefaults(aStartIndex, aEndIndex);
        }

        public void PropsSetHidden(bool bHidden)
        {
            _Properties.SetHidden(bHidden);

        }

        public void ReadNotes(string aFileSpec, string aFileSection) { _Notes = uopUtils.ReadINI_Strings(aFileSpec, aFileSection, "Note"); }

        //a string of range span name that this part is associated to
        public string RangeSpanNames(uopProject aProject, string aDelimiter = ",")
        {

            if (HasRingRanges)
                return RingRanges.SpanNameList(aDelimiter);

            List<string> rGUIDs = mzUtils.StringsFromDelimitedList(RangeList, ",");
            string _rVal = string.Empty;
            if (rGUIDs.Count > 0)
            {
                aProject ??= Project;
                if (aProject == null) return "";


                colUOPTrayRanges aRngs = aProject.TrayRanges;
                for (int i = 1; i <= rGUIDs.Count; i++)
                {
                    uopTrayRange aRng = aRngs.GetByGuid(rGUIDs[i - 1]);
                    if (aRng != null) mzUtils.ListAdd(ref _rVal, aRng.SpanName(), bSuppressTest: true, aDelimitor: aDelimiter);
                }
            }
            return _rVal;
        }


        public void PropsLockTypes(bool bSetDefaults = false)
        {
            TPROPERTIES props = ActiveProps;
            props.LockTypes(bSetDefaults);
            ActiveProps = props;
        }
        //public object WeldedInPlace { get; internal set; }
        //public uopSheetMetal SheetMetal { get;  set; }
        //public virtual int OccuranceFactor { get; internal set; }
        //public mdProject Project { get; internal set; }
        //public string RangeGUID { get; internal set; }
        //public virtual double SheetMetalThickness { get; internal set; }
        //public virtual bool IsVisible { get; internal set; }
        //public virtual bool DoubleNuts { get; internal set; }
        //public uppUnitFamilies Bolting { get; internal set; }
        //public uopHardwareMaterial RangeHardwareMaterial { get; internal set; }
        //public virtual int DowncomerIndex { get; internal set; }
        //public object ProjectSparePercentage { get; internal set; }
        //public virtual double SheetMetalWeightMultiplier { get; internal set; }
        public void PropsReadINI(string aFileSpec, string aFileSection, bool bRaiseNotFoundError = false)
        {
            try
            {
                TPROPERTIES myProps = ActiveProps;

                myProps.ReadFromINIFile(aFileSpec, aFileSection, bRaiseNotFoundError);
                ActiveProps = myProps;
            }
            catch (Exception)
            {
            }
        }




        private WeakReference<mdDowncomer> _MDDowncomerRef;
        public virtual mdDowncomer MDDowncomer
        {
            get
            {
                if (_MDDowncomerRef == null) return null;
                if (!_MDDowncomerRef.TryGetTarget(out mdDowncomer _rVal)) _MDDowncomerRef = null;
                return _rVal;
            }
            set
            {
                if (value == null)
                {
                    _MDDowncomerRef = null;
                    return;
                }
                _MDDowncomerRef = new WeakReference<mdDowncomer>(value);
            }
        }

        //a string of range GUIDS that this part is associated to
        public void AssociateToRange(string aGUID, bool bClear = false)

        {
            _RangeIDS ??= new List<string>();
            if (bClear) _RangeIDS.Clear();
            if (string.IsNullOrWhiteSpace(aGUID)) return;
            if (!string.IsNullOrWhiteSpace(RangeGUID) && string.Compare(RangeGUID, aGUID, true) == 0)
                return;
            if (_RangeIDS.FindIndex((x) => string.Compare(x, aGUID, true) == 0) == -1) { _RangeIDS.Add(aGUID); }
            if (bClear) RangeGUID = aGUID;
        }
        public void AssociateToRange(uopTrayRange aRange) { if (aRange != null) AssociateToRange(aRange.GUID); }
        public string AssociatedRangeList(uopProject aProject = null, string aPrefix = null)
        {
            aProject ??= Project;
            if (aProject == null) return "";
            string _rVal = string.Empty;
            List<string> spannames = new List<string>();
            string spanname;
            List<string> guids = mzUtils.StringsFromDelimitedList(RangeList);
            foreach (string item in guids)
            {
                uopTrayRange range = aProject.TrayRanges.Find(x => string.Compare(x.GUID, item, true) == 0);
                if (range != null)
                {
                    spanname = range.SpanName(true);
                    if (spannames.FindIndex(x => x == spanname) < 0)
                    {
                        spannames.Add(spanname);
                    }
                }
            }
            spannames.Sort();
            _rVal = mzUtils.ListToString(spannames, ", ");
            if (!string.IsNullOrWhiteSpace(aPrefix)) _rVal = aPrefix + _rVal;

            return _rVal;
        }
        public string AssociatedParentList(string aPrefix = null)
        {

            string _rVal = string.Empty;

            List<string> parentpns = mzUtils.StringsFromDelimitedList(ParentList);
            foreach (string item in parentpns)
            {
                if (!string.IsNullOrWhiteSpace(item))
                {
                    mzUtils.ListAdd(ref _rVal, $" {item}", bSuppressTest: false, aDelimitor: ",");

                }

            }

            if (!string.IsNullOrWhiteSpace(aPrefix)) _rVal = aPrefix + _rVal;

            return _rVal;
        }
        //a string of parent ids that this part is associated to
        public void AssociateToParent(string aPNorID, string aDelimitor = null, bool bClear = false)
        {
            if (bClear || _ParentPartNumbers == null) _ParentPartNumbers = new List<string>();
            if (string.IsNullOrWhiteSpace(aPNorID)) return;

            if (aDelimitor == null)
                _ParentPartNumbers.Add(aPNorID.Trim());
            else
                mzUtils.AppendToStringList(ref _ParentPartNumbers, aPNorID, aDelimitor);
        }
        public void AssociateToParent(uopPart aPart, bool bClear = false) { if (aPart != null) { AssociateToParent(aPart.PartNumber, bClear: bClear); ParentPartType = aPart.PartType; } }

        public void MergeRangeAssociations(List<string> aGUIDS) => mzUtils.AppendToStringList(ref _RangeIDS, aGUIDS);
        public void MergeRangeAssociations(string aGUIDS) => mzUtils.AppendToStringList(ref _RangeIDS, aGUIDS, ",");
        public void MergeParentAssociations(string aPNorIDs) => mzUtils.AppendToStringList(ref _ParentPartNumbers, aPNorIDs, ",");

        public void MergeParentAssociations(List<string> aPNorIDs) => mzUtils.AppendToStringList(ref _ParentPartNumbers, aPNorIDs);


        public hdwHexBolt SmallBolt(string aDescription = "", string aCategory = "", int aQuantity = 0)
        {
            hdwHexBolt _rVal = new hdwHexBolt((Bolting == uppUnitFamilies.English) ? uppHardwareSizes.ThreeEights : uppHardwareSizes.M10, RangeHardwareMaterial)
            {
                IsVisible = IsVisible,
                Quantity = Quantity,
                DoubleNut = DoubleNuts,
                ObscuredLength = SheetMetalThickness
            };
            if (Bolting == uppUnitFamilies.English)
            {
                _rVal.Length = (!_rVal.DoubleNut) ? 1 : 1.25;
            }
            else
            {
                _rVal.Length = (!_rVal.DoubleNut) ? 25 / 25.4 : 30 / 25.4;
            }

            _rVal.DescriptiveName = aDescription;
            aCategory = string.IsNullOrWhiteSpace(aCategory) ? Category : aCategory.Trim();
            _rVal.Quantity = (aQuantity <= 0) ? Quantity : aQuantity;

            _rVal.SubPart(this, aCategory);
            return _rVal;

        }
        public void Copy(uopPart aPart)
        {
            if (aPart == null) return;


            _Center = new uopVector(aPart._Center);
            _Properties = aPart._Properties.Clone();

            //_Column = aPart.Column;
            //_Project = aPart.Project;
            //_Range = aPart.Range;
            _HardwareMaterial = new TMATERIAL(aPart._HardwareMaterial);
            _SheetMetal = new TMATERIAL(aPart._SheetMetal);
            _TubeMaterial = new TMATERIAL(aPart._TubeMaterial);
            _RangeIDS = new List<string>(aPart._RangeIDS);
            _ParentPartNumbers = new List<string>(aPart._ParentPartNumbers);
            _Indices = new TPARTINDICES(aPart._Indices);
            if (aPart.HasRingRanges)
                _RingRanges = aPart.RingRanges.Clone();


            ParentPartType = aPart.ParentPartType;
            ParentPath = aPart.ParentPath;
            ParentPartIndex = aPart.ParentPartIndex;
            PartType = aPart.PartType;
            _PartNumber = aPart._PartNumber;
            _OverridePartNumber = aPart._OverridePartNumber;
            SuppressEvents = aPart.SuppressEvents;
            LastSaveDate = aPart.LastSaveDate;
            _Reading = aPart._Reading;
            Category = aPart.Category;
            IsMetric = aPart.IsMetric;
            DescriptiveName = aPart.DescriptiveName;
            IsCommon = aPart.IsCommon;
            IsGlobal = aPart.IsGlobal;
            Suppressed = aPart.Suppressed;
            Invalid = aPart.Invalid;
            WeldedInPlace = aPart.WeldedInPlace;
            Selected = aPart.Selected;
            IsVisible = aPart.IsVisible;
            Requested = aPart.Requested;
            RadialDirection = aPart.RadialDirection;
            Direction = aPart.Direction;
            DisplayUnits = aPart.DisplayUnits;
            DrawingUnits = aPart.DrawingUnits;
            Weight = aPart.Weight;
            Tag = aPart.Tag;
            Flag = aPart.Flag;
            SubType = aPart.SubType;
            UniqueIndex = aPart.UniqueIndex;
            _OccuranceFactor = aPart._OccuranceFactor;
            _Quantity = aPart._Quantity;
            TotalQuantity = aPart.TotalQuantity;
            Side = aPart.Side;
            End = aPart.End;
            Length = aPart.Length;
            Height = aPart.Height;
            Width = aPart.Width;
            Radius = aPart.Radius;
            Diameter = aPart.Diameter;
            _Name = aPart._Name;
            Angle = aPart.Angle;
            SparePercentage = aPart.SparePercentage;
            _PercentOpen = aPart._PercentOpen;
            NodePath = aPart.NodePath;
            _NodeName = aPart._NodeName;
            Mark = aPart.Mark;
            IsSheetMetal = aPart.IsSheetMetal;
            IsVirtual = aPart.IsVirtual;
            AlternateRingType = aPart.AlternateRingType;
            SubPartType = aPart.SubPartType;
            ColumnHandle = aPart.ColumnHandle;

            ParentCenterV = aPart.ParentCenterV;
            _StackPattern = aPart._StackPattern;
            _Indices.PartIndex = 0;
            _RingStart = aPart._RingStart;
            _RingEnd = aPart._RingEnd;
            _Bolting = aPart._Bolting;
            _ManholeID = aPart._ManholeID;
            Col = aPart.Col;
            Row = aPart.Row;

        }

        internal TPARTINDICES _Indices;

        //the value of the property
        public virtual uopProperty CurrentProperty(string aPropertyName, bool bSupressNotFoundError = false)
        {


            TPROPERTIES aProps = ActiveProps;
            if (aProps.TryGet(aPropertyName, out TPROPERTY aProp))
            {
                aProp.SubPart(this);
                aProps.SetItem(aProp.Index, aProp);
                ActiveProps = aProps;
                return new uopProperty(aProp);

            }
            else
            {
                if (!bSupressNotFoundError)
                {
                    throw new Exception("Property '" + aPropertyName + "' Not Found.");
                }
            }
            return null;
        }



        public string PartPath(bool bTruncate = false, string aSuffix = null) => uopPart.BuildPartPath(PartType, _Indices, PartName, bTruncate, aSuffix);

        internal TPROPERTY Prop(dynamic aNameOrIndex)
        {

            if (ActiveProps.TryGet(aNameOrIndex, out TPROPERTY _rVal)) _rVal.SubPart(this);
            return _rVal;
        }

        public void PropRemove(dynamic aNameOrIndex)
        {
            TPROPERTIES props = ActiveProps;
            props.Remove(aNameOrIndex);
            ActiveProps = props;
        }

        //the value of the property
        //while calling pass 3rd parameter to 2nd place
        public string PropStringVal(dynamic aNameOrIndex, bool bFormated = false)
        {

            if (!ActiveProps.TryGet(aNameOrIndex, out TPROPERTY prop)) { return ""; }

            return bFormated ? prop.FormatedString : prop.ValueS;

        }

        // ^returns the property converted to the requested units
        //while calling pass lastparamater to 2nd place
        public string PropUnitValueString(dynamic aNameOrIndex, uppUnitFamilies aUnitFamily = uppUnitFamilies.Default, bool bZerosAsNullString = false, bool bDefaultAsNullString = false, int aOverridePrecision = -1, bool bIncludeThousandsSeps = false, bool bIncludeUnitString = false, string aFormatString = "")
        => Prop(aNameOrIndex).UnitValueString(aUnitFamily, bZerosAsNullString, bDefaultAsNullString, aOverridePrecision, bIncludeThousandsSeps, bIncludeUnitString, aFormatString);

        // ^returns the property converted to the requested units
        public virtual double PropUnitValue(dynamic aNameOrIndex, uppUnitFamilies aUnitFamily = uppUnitFamilies.Default) => Prop(aNameOrIndex).UnitValue(aUnitFamily);

        public virtual bool PropValsCopy(uopProperties aProperties, bool bCopyNewMembers = false, List<string> aSkipList = null)
        {
            if (aProperties == null) return false;

            TPROPERTIES myProps = ActiveProps;
            string sval = string.Empty;
            myProps.CopyValues(aProperties, out sval, bCopyNewMembers, aSkipList);
            if (sval !=  string.Empty)
            {
                ActiveProps = myProps;
                return true;

            }
            return false;

        }
        internal virtual bool PropValsCopy(TPROPERTIES aProperties, bool bCopyNewMembers = false, List<string> aSkipList = null)
        {

            TPROPERTIES myProps = ActiveProps;
            string sval = string.Empty;
            myProps.CopyValues(aProperties, out sval, bCopyNewMembers, aSkipList);
            if (sval !=  string.Empty)
            {
                ActiveProps = myProps;
                return true;

            }
            return false;

        }
        public void PropSetMinMax(dynamic aNameOrIndex, dynamic aMin = null, dynamic aMax = null, dynamic aDefault = null, bool bSetToDefault = false, bool bApplyLimits = false, dynamic aIncrement = null)
        {
            TPROPERTIES aProps = ActiveProps;
            aProps.SetMinMax(aNameOrIndex, aMin, aMax, aDefault, bSetToDefault, bApplyLimits, aIncrement);
            ActiveProps = aProps;
        }
        /*
        '#1an optional delimator
        '#2the index to begin the procedure (default = 1)
        '#3the index to end the procedure (default = count)
        '#4flag to return the properties complete signatures in the returned string
        '#5an optional string to put before anf after the values in the returned string
        '^returns the values of properties in the collection in a comma (or other deliminator) string
        */
        public string PropString(string aDeliminator = ",", int aStartIndex = 0, int aEndIndex = 0, bool bIncludePropertyNames = false, string aWrapper = "", bool bShowDecodedValue = false)
         => ActiveProps.StringVals(aDeliminator, aStartIndex, aEndIndex, bIncludePropertyNames, aWrapper, bShowDecodedValue);

        public uopProperty PropSetAttributes(dynamic aNameOrIndex, bool? bOptional = null, bool? bHidden = null, bool? bProtected = null, bool bReset = false) => PropSetAttributes(aNameOrIndex, bOptional, bHidden, bProtected, bReset, out bool _);
        public uopProperty PropSetAttributes(dynamic aNameOrIndex, bool? bOptional, bool? bHidden, bool? bProtected, bool bReset, out bool rExists)
        {

            TPROPERTIES aProps = ActiveProps;
            rExists = aProps.TryGet(aNameOrIndex, out TPROPERTY aProp);
            if (!rExists) return null;


            if (bOptional.HasValue) aProp.Optional = bOptional.Value;
            if (bHidden.HasValue) aProp.Hidden = bHidden.Value;
            if (bProtected.HasValue) aProp.Protected = bProtected.Value;
            if (bReset)
            {
                aProp.Value = aProp.DefaultValue;
            }
            aProps.SetItem(aProp.Index, aProp);
            ActiveProps = aProps;
            return new uopProperty(aProp);

        }

        //a collection of notes assigned to the object
        public uopProperties GetNotes(string aLeadNote = "")
        {
            _Notes.SubPart(this);
            TPROPERTIES _rVal = _Notes.Clone(true);
            if (aLeadNote !=  string.Empty) _rVal.AddProp("LeadNote", aLeadNote);
            _rVal.Append(_Notes);
            return new uopProperties(_rVal);
        }

        public void NotifyProject(uopProperty aProperty)
        {
            if (aProperty != null)
            {
                if (ProjectHandle !=  string.Empty && !_SuppressEvents && !_Reading)
                {
                    uopProject aProject = uopEvents.RetrieveProject(ProjectHandle);
                    if (aProject != null) aProject.Notify(aProperty);
                }
            }
        }

        //retrieves the requested part property
        public uopProperty GetProperty(dynamic aNameOrIndex, uppPartTypes aPartType = uppPartTypes.Undefined, bool bSupressNotFoundError = false)
        {

            TPROPERTIES Props = (aPartType == PartType || aPartType == uppPartTypes.Undefined) ? ActiveProps : GetParentProps(aPartType);
            if (Props.TryGet(aNameOrIndex, out TPROPERTY aProp)) return new uopProperty(aProp);
            if (!bSupressNotFoundError)
            {
                string msg = "Property [" + aNameOrIndex.ToString() + "] Not Found On Part '" + PartPath() + "'";
                Debug.Fail(msg); // throw new Exception("Property '" + aNameOrIndex + "' Not Found."); 
            }
            return null;

        }

        //the value of the property
        public uppUnitTypes GetPropertyUnits(dynamic aNameOrIndex, uppPartTypes aPartType = uppPartTypes.Undefined, bool bSupressNotFoundError = false)
        {

            TPROPERTIES Props = (aPartType == PartType || aPartType == uppPartTypes.Undefined) ? ActiveProps : GetParentProps(aPartType);
            if (Props.TryGet(aNameOrIndex, out TPROPERTY aProp)) return aProp.Units.UnitType;
            if (!bSupressNotFoundError)
            { throw new Exception($"Property '{aNameOrIndex}' Not Found."); }
            return uppUnitTypes.Undefined;

        }



        public virtual uopHoleArray HoleArray(uopTrayAssembly aAssy = null, string aTag = null) => new uopHoleArray();

        #endregion

        #region Shared Methods
        //virtual parts are parts that have not physical objects or have no part number etc.
        public static bool IsVirtualPart(uopPart aPart)
        {
            if (aPart.PartType == uppPartTypes.Project) { return true; }
            if (aPart.PartType == uppPartTypes.Column) { return true; }
            if (aPart.PartType == uppPartTypes.TrayRange) { return true; }
            if (aPart.PartType == uppPartTypes.TrayAssembly) { return true; }
            if (aPart.PartType == uppPartTypes.BeamSupport || aPart.PartType == uppPartTypes.Ring) { return true; }
            return false;

        }



        public static uppProjectFamilies GetProjectFamily(uppProjectTypes aProjectType)
        {

            return aProjectType switch
            {
                uppProjectTypes.CrossFlow => uppProjectFamilies.uopFamXF,
                uppProjectTypes.MDSpout => uppProjectFamilies.uopFamMD,
                uppProjectTypes.MDDraw => uppProjectFamilies.uopFamMD,
                _ => uppProjectFamilies.Undefined
            };


        }

        public static string DefaultPartNumber(uopPart aPart, int aAdder = 0)
        {
            string _rVal = string.Empty;
            int pnumint = 0;

            if (aPart.ProjectFamily == uppProjectFamilies.uopFamMD)
            {
                switch (aPart.PartType)
                {
                    case uppPartTypes.TraySupportBeam:
                        pnumint = 850 + aPart.RangeIndex;
                        break;
                    case uppPartTypes.DowncomerBox:
                        mdDowncomerBox box = (mdDowncomerBox)aPart;
                        var dc = box.Downcomer;
                        if (box.IsVirtual && dc != null)
                        {
                            if (dc.Parent != null) dc = dc.Parent;
                            int correspondingDcIndex = dc.Index;
                            int correspondingBoxIndex = dc.Boxes.Count - box.Index + 1;

                            aAdder += (correspondingBoxIndex - 1) * 10;
                            pnumint = (aPart.RangeIndex * 1000) + (correspondingDcIndex * 100);
                            
                        }
                        else
                        {
                            aAdder += (aPart.Index - 1) * 10;
                            pnumint = (aPart.RangeIndex * 1000) + (aPart.DowncomerIndex * 100);
                        }
                        break;
                    case uppPartTypes.Downcomer:
                        pnumint = (aPart.RangeIndex * 1000) + (aPart.Index * 100);
                        break;
                    case uppPartTypes.EndAngle:
                        if (aPart.Direction == dxxOrthoDirections.Right)
                        {
                            pnumint = (aPart.RangeIndex * 100) + (aPart.DowncomerIndex * 10);
                        }
                        else
                        {
                            pnumint = (aPart.RangeIndex * 100) + (aPart.DowncomerIndex * 10) + 5;
                        }
                        break;
                    case uppPartTypes.SpliceAngle:
                        pnumint = (aPart.RangeIndex * 100) + 60;
                        break;
                    case uppPartTypes.ManwaySplicePlate:
                    case uppPartTypes.ManwayAngle:
                        pnumint = (aPart.RangeIndex * 100) + 80;

                        break;
                    case uppPartTypes.CrossBrace:
                        pnumint = aPart.RangeIndex + 70;
                        break;
                    case uppPartTypes.DeckSection:
                        if (aPart._Indices.PartNumberIndex <= 0)
                        {
                            pnumint = (aPart.RangeIndex * 1000) + (aPart.PanelIndex * 100) + aPart._Indices.SectionIndex;
                        }
                        else
                        {
                            pnumint = (aPart.RangeIndex * 1000) + aPart._Indices.PartNumberIndex;
                        }

                        break;
                    case uppPartTypes.FingerClip:
                        pnumint = 50;

                        break;
                    case uppPartTypes.DeckBeam:
                        pnumint = (aPart.RangeIndex * 1000) + 200 + aPart.Index;
                        break;
                }

            }
            else if (aPart.ProjectFamily == uppProjectFamilies.uopFamXF)
            {
                switch (aPart.PartType)
                {
                    case uppPartTypes.Downcomer:
                        break;
                    case uppPartTypes.SpliceAngle:
                        pnumint = (aPart.RangeIndex * 100) + 60 + aPart.Index - 1;
                        break;
                    case uppPartTypes.ManwayAngle:
                        pnumint = (aPart.RangeIndex * 100) + 80;

                        break;
                    case uppPartTypes.CrossBrace:
                        pnumint = aPart.RangeIndex + 70;
                        break;
                    case uppPartTypes.DeckSection:
                        pnumint = (aPart.RangeIndex * 1000) + aPart.Index;
                        break;
                    case uppPartTypes.FingerClip:
                        pnumint = 50;

                        break;
                    case uppPartTypes.DeckBeam:
                        pnumint = (aPart.RangeIndex * 1000) + 200 + aPart.Index;
                        break;
                }

            }
            if (pnumint > 0)
            {
                pnumint += aAdder;
                _rVal = Convert.ToString(pnumint);
            }

            return _rVal;
        }

        internal static string BuildPartPath(uppPartTypes pType, TPARTINDICES aIndices, string aPartName, bool bRangeOnly = false, string aSuffix = null)
        {

            string rVal = string.Empty;


            if (!bRangeOnly) { rVal = "PROJECT"; }

            if (pType != uppPartTypes.Project)
            {
                if (aIndices.ColumnIndex > 0 & pType != uppPartTypes.Column && !bRangeOnly) { rVal += $".COLUMN({aIndices.ColumnIndex})"; }

                if (aIndices.RangeIndex > 0 & pType != uppPartTypes.TrayRange)
                {
                    if (!bRangeOnly)
                    { rVal += $".RANGE({aIndices.RangeIndex})"; }
                    else
                    { rVal += $"TRAY({aIndices.RangeIndex})"; }

                    if (pType != uppPartTypes.TrayAssembly && pType != uppPartTypes.SlotFile && !bRangeOnly) { rVal += ".TRAYASSEMBLY"; }
                }

            }


            if (aIndices.PanelIndex > 0 & pType != uppPartTypes.DeckPanel) { rVal += $".PANEL({aIndices.PanelIndex})"; }
            if (aIndices.DowncomerIndex > 0 & pType != uppPartTypes.Downcomer) { rVal += $".DOWNCOMER({aIndices.DowncomerIndex})"; }

            if (aIndices.PanIndex > 0 & pType != uppPartTypes.ReceivingPan) { rVal += $".PAN({aIndices.PanIndex})"; }
            if (aIndices.DistributorIndex > 0 && pType != uppPartTypes.Distributor) { rVal += $".DISTRIBUTOR({aIndices.DistributorIndex})"; }
            if (aIndices.ChimneyTrayIndex > 0 & pType != uppPartTypes.ChimneyTray) { rVal += $".CHIMNEYTRAY({aIndices.ChimneyTrayIndex})"; }

            if (!string.IsNullOrWhiteSpace(aPartName))
            {
                rVal += "." + aPartName.ToUpper();

            }

            if (!string.IsNullOrEmpty(aSuffix)) { rVal += $".{aSuffix}"; }

            return rVal;
        }

        /// <summary>
        /// Set default values to Part
        /// </summary>
        /// <param name="aPart"></param>
        /// <param name="aType"></param>
        /// <returns></returns>
        public static void SetPartDefaults(uopPart aPart, string aRangeGUID = null)
        {


            //.PartName = "Undefined"
            aPart.IsCommon = false;


            //=============================================
            switch (aPart.PartType)
            {
                case uppPartTypes.RingClip:
                    aPart.IsCommon = true;

                    //=============================================
                    break;
                case uppPartTypes.TrayAssembly:

                    //=============================================
                    break;
                case uppPartTypes.TrayRange:
                    aPart.RangeGUID = string.IsNullOrWhiteSpace(aRangeGUID) ? mzUtils.CreateGUID() : aRangeGUID;
                    //=============================================
                    break;
                case uppPartTypes.TrayRanges:


                    //=============================================
                    break;
                case uppPartTypes.Project:

                    //=============================================
                    break;

                case uppPartTypes.Distributor:
                    //.PartName = "Distributor"

                    //=============================================
                    break;
                case uppPartTypes.DistributorCase:

                    //=============================================
                    break;
                case uppPartTypes.DeckPanel:
                    break;
                case uppPartTypes.ChimneyTray:
                    break;
                case uppPartTypes.ChimneyTrayCase:
                    break;
                case uppPartTypes.StartupSpout:
                    break;
                case uppPartTypes.Stage:
                    break;
                case uppPartTypes.Constraint:
                    break;
                case uppPartTypes.SpoutGroup:
                    break;
                case uppPartTypes.MDBubblePromoter:
                    break;
                case uppPartTypes.XFBubblePromoter:
                    //.PartName = "Bubble Promoter"


                    //=============================================
                    break;
                case uppPartTypes.Column:
                    //.PartName = "Column"
                    aPart.DescriptiveName = "Column";


                    //=============================================
                    break;
                case uppPartTypes.Columns:
                    //.PartName = "Columns"
                    aPart.DescriptiveName = "Columns";
                    //=============================================
                    break;
                case uppPartTypes.Customer:
                    //.PartName = "Customer"

                    //=============================================
                    break;
                case uppPartTypes.Ring:
                    //.PartName = "Support Ring"

                    //=============================================
                    break;
                case uppPartTypes.MaterialSpec:
                    //.PartName = "Material Spec."

                    //=============================================
                    break;
                case uppPartTypes.Deflector:
                    //.PartName = "Deflector Plate"
                    //'.PartNumber = "900"

                    //=============================================
                    break;
                case uppPartTypes.SupplementalDeflector:
                    //.PartName = "Supplemental Deflector"


                    //=============================================
                    break;
                case uppPartTypes.SupportClip:
                    //.PartName = "Support Clip"


                    //=============================================
                    break;
                case uppPartTypes.SpliceAngle:
                    //.PartName = "Splice Angle"


                    //=============================================
                    break;
                case uppPartTypes.FingerClip:
                    //.PartName = "Finger Clip"
                    //'.PartNumber = "50"
                    aPart.IsCommon = true;


                    //=============================================
                    break;
                case uppPartTypes.SpacerTube:
                    //.PartName = "Spacer Tube"

                    //            .IsCommon = True
                    //'.PartNumber = "80"

                    //=============================================
                    break;
                case uppPartTypes.Parts:
                    //.PartName = "Common Part"


                    //=============================================
                    break;
                case uppPartTypes.SpacerPlate:
                    //.PartName = "Spacer Plate"

                    //=============================================
                    break;
                case uppPartTypes.SealTab:
                    //.PartName = "Seal Tab"


                    //=============================================
                    break;
                case uppPartTypes.SealPlate:
                    //.PartName = "Seal Plate"
                    //'.PartNumber = "100"

                    aPart.IsCommon = true;

                    //=============================================
                    break;
                case uppPartTypes.IntegralBeam:
                    //.PartName = "Integral Beam"


                    //=============================================
                    break;
                case uppPartTypes.WeirAngle:
                    //.PartName = "Outlet Weir"
                    //'.PartNumber = "140"


                    //=============================================
                    break;
                case uppPartTypes.ReceivingPan:
                    //.PartName = "Receiving Pan"
                    //'.PartNumber = "130"



                    //=============================================
                    break;
                case uppPartTypes.DowncomerBeam:
                    //.PartName = "Downcomer Beam"
                    //'.PartNumber = "101"


                    //=============================================
                    break;
                case uppPartTypes.HoldDownClip:
                    //.PartName = "Hold Down Clip"
                    //'.PartNumber = "90"

                    aPart.IsCommon = true;

                    //=============================================
                    break;
                case uppPartTypes.FeedZone:
                    //.PartName = "Feed Zone"

                    //=============================================
                    break;
                case uppPartTypes.EndPlate:
                    //.PartName = "End Plate"
                    //'.PartNumber = ""


                    //=============================================
                    break;
                case uppPartTypes.EndSupport:
                    //.PartName = "End Support"
                    //'.PartNumber = ""


                    //=============================================
                    break;
                case uppPartTypes.EndAngle:
                    //.PartName = "End Angle(" & .SubType & ")"

                    aPart.IsCommon = true;

                    //=============================================
                    break;
                case uppPartTypes.DowncomerBox:
                    //.PartName = "Downcomer Box"


                    //=============================================
                    break;
                case uppPartTypes.APPan:
                    //.PartName = "Anti-Pena return _rVal;tration Pan"
                    //'.PartNumber = ""


                    //=============================================
                    break;
                case uppPartTypes.DowncomerExtension:
                    //.PartName = "Downcomer Extension"


                    //=============================================
                    break;
                case uppPartTypes.CrossBrace:
                    //.PartName = "Cross Brace"


                    //=============================================
                    break;
                case uppPartTypes.ManwayAngle:
                    //.PartName = "Manway Angle"

                    //=============================================
                    break;
                case uppPartTypes.ManwaySplicePlate:
                    //.PartName = "Manway Splice Plate"


                    //=============================================
                    break;
                case uppPartTypes.ShelfAngle:
                    //.PartName = "Shelf Angle"
                    //.PartNumber = ""


                    //=============================================
                    break;
                case uppPartTypes.Downcomer:
                    //.PartName = "Downcomer Assembly"
                    //.PartNumber = "100"


                    //=============================================
                    break;
                case uppPartTypes.Deck:
                    //.PartName = "Deck Object"
                    aPart.DescriptiveName = "Deck Design Data";
                    //=============================================
                    break;
                case uppPartTypes.DesignOptions:
                    //.PartName = "Design Options Object"

                    //=============================================
                    break;
                case uppPartTypes.TrayLayout:
                    //.PartName = "Tray Object"

                    //=============================================
                    break;
                case uppPartTypes.SlotFile:
                    //.PartName = "Slotting File"


                    //=============================================
                    break;
                case uppPartTypes.FlowSlot:
                    //.PartName = "Flow Slot"

                    //=============================================
                    break;
                case uppPartTypes.FlowSlotZone:
                    //.PartName = "Flow Slot Zone"

                    //=============================================
                    break;
                case uppPartTypes.BeamSupport:
                    //.PartName = "Beam Support"

                    //=============================================
                    break;
                case uppPartTypes.CarriageBolt:
                    //.PartName = "Carriage Bolt"

                    //=============================================
                    break;

                case uppPartTypes.LockWasher:
                    //.PartName = "Lock Washer"

                    //=============================================
                    break;
                case uppPartTypes.Stiffener:
                    //.PartName = "Stiffener"
                    //.PartNumber = "80"


                    //=============================================
                    break;
                case uppPartTypes.DeckSection:
                    //.PartName = "Deck Section"
                    //.PartNumber = "1101"


                    //=============================================
                    break;
                case uppPartTypes.Stud:
                    //.PartName = "Stud"
                    //=============================================
                    break;
                case uppPartTypes.ShavedStud:
                    //.PartName = "Shaved Stud"

                    //=============================================
                    break;
                case uppPartTypes.ManwayClamp:
                    //.PartName = "Manway Clamp"
                    //.PartNumber = "62"
                    aPart.IsCommon = true;


                    //=============================================
                    break;
                case uppPartTypes.ManwayClip:
                    //.PartName = "Manway Clip"
                    //.PartNumber = "64"
                    aPart.IsCommon = true;


                    //=============================================
                    break;
                case uppPartTypes.ManwayWasher:
                    //.PartName = "Manway Clip Washer"
                    //.PartNumber = "63"
                    aPart.IsCommon = true;

                    //=============================================
                    break;
                case uppPartTypes.FlatWasher:
                    //.PartName = "Flat Washer"

                    //=============================================
                    break;
                case uppPartTypes.HexBolt:
                    //.PartName = "Hex Bolt"
                    //=============================================
                    break;
                case uppPartTypes.HexNut:
                    //.PartName = "Hex Nut"

                    //=============================================
                    break;
                case uppPartTypes.HoldDownWasher:
                    //.PartName = "Hold Down Washer"
                    aPart.IsCommon = true;


                    break;
            }


        }

        object ICloneable.Clone() => (object)Clone();

        #endregion

    }
}
