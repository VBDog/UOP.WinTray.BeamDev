using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media;
using UOP.DXFGraphics;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Documents;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Events;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Materials;

using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Tables;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;

namespace UOP.WinTray.Projects.Parts
{
    /// <summary>
    ///  extends the uopProject interface and is the top level parent object for a UOP MD project
    ///  used by the WinTray application to refer to and acces a MD Tray projects data
    ///  Extends the project interface
    /// </summary>
    public class mdProject : uopProject, IDisposable, IEventSubscriber<Message_ProjectRequest>, IEventSubscriber<Message_PartsInvalidated>
    {
        public override uppPartTypes BasePartType => uppPartTypes.Project;


        #region Variables

        private bool _HasChanged;
        private string _SelectedDrawing;
        private string _SelectedCalc;
        private bool _Locked;
        // private uopEventHandler _Event;
        private uopProjectSpecs _Specs;
        private uopCustomer _Customer;
        private uopColumn _Column;
        private colMDChimneyTrays _ChimneyTrays;
        private colMDDistributors _Distributors;
        private colMDStages _Stages;
        private uopDocuments _Documents;
        private FileStream _LogFile;
        private colUOPRingSpecs _RingSpecs;


        #endregion

        #region Events



        public delegate void MDProjectPropertyChange(uopProperty aProperty);
        public event MDProjectPropertyChange eventMDProjectPropertyChange;



        public delegate void SavingHandler(bool aBegin);
        public event SavingHandler eventSaving;




        #endregion

        //check for with event registration for _Event,ocolumn ,  _ChimneyTrays,_Distributors

        static readonly Version App = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        private bool disposedValue;


        #region Constructors

        public mdProject() : base(uppProjectTypes.MDSpout)
        {
            InitializeProperties();

            _Customer.eventCustomerPropertyChange += _Customer_CustomerPropertyChange;

            _Column.eventColumnPropertyChange += _Column_ColumnPropertyChange;
            _Column.eventRangeCountChange += _Column_RangeCountChange;

            _ChimneyTrays.eventChimneyTrayCountChange += _ChimneyTrays_ChimneyTrayCountChange;
            _ChimneyTrays.eventChimneyTrayMemberChanged += _ChimneyTrays_ChimneyTrayMemberChanged;

            _Distributors.eventDistributorCountChange += _Distributors_DistributorCountChange;
            _Distributors.eventDistributorMemberChanged += _Distributors_DistributorMemberChanged;

            HardwareMaterial = new uopHardwareMaterial(TMATERIAL.DefaultHardware(), this);
            uopEvents.Aggregator.Subscribe(this);
        }

        internal mdProject(mdProject aPartToCopy) : base(aPartToCopy != null ? aPartToCopy.ProjectType: uppProjectTypes.MDSpout)
        {
            InitializeProperties();
            HardwareMaterial = new uopHardwareMaterial(TMATERIAL.DefaultHardware(), this);
            if (aPartToCopy != null)
            {
                Copy(aPartToCopy);
                _Customer = new uopCustomer(aPartToCopy.Customer);
                _Column = new uopColumn(aPartToCopy.Column);
                _ChimneyTrays = new colMDChimneyTrays(aPartToCopy.ChimneyTrays);
                _Distributors = new colMDDistributors(aPartToCopy.Distributors);
            }
            else { InitializeProperties(); }

            _Customer.eventCustomerPropertyChange += _Customer_CustomerPropertyChange;

            _Column.eventColumnPropertyChange += _Column_ColumnPropertyChange;
            _Column.eventRangeCountChange += _Column_RangeCountChange;

            _ChimneyTrays.eventChimneyTrayCountChange += _ChimneyTrays_ChimneyTrayCountChange;
            _ChimneyTrays.eventChimneyTrayMemberChanged += _ChimneyTrays_ChimneyTrayMemberChanged;

            _Distributors.eventDistributorCountChange += _Distributors_DistributorCountChange;
            _Distributors.eventDistributorMemberChanged += _Distributors_DistributorMemberChanged;

            uopEvents.Aggregator.Subscribe(this);
        }

        private void InitializeProperties()
        {
            try
            {
                uopGlobals.goEvents ??= new uopEventHandler();
                //_Event = uopGlobals.goEvents;
                //uopEventHandler.eventProjectRequest += OEvent_eventProjectRequest;

                _Documents = new uopDocuments
                {
                    InvalidWhenEmpty = true
                };
                _Column ??= new uopColumn(this);
                _Customer ??= new uopCustomer();
                _Distributors ??= new colMDDistributors(null, this, true);
                _ChimneyTrays ??= new colMDChimneyTrays(null, this, true);
                _Distributors.Clear();
                _ChimneyTrays.Clear();
                _Distributors.MaintainIndices = true;
                _ChimneyTrays.MaintainIndices = true;

                base.ActiveProps = new TPROPERTIES();

                uopGlobals.goEvents.RegisterProject(this);
                ParentPath = string.Empty;
                _Column.Index = 1;
                AddProperty("KeyNumber", "");
                AddProperty("Revision", 0, aHeading: "Project");
                AddProperty("Designer", "");
                AddProperty("ImportFileName", "");
                AddProperty("SAPNumber", "", aDisplayName: "Network");
                AddProperty("IDNumber", "");
                AddProperty("Bolting", uppUnitFamilies.Metric, aDecodeString: "0=UNC,1=Metric");
                AddProperty("MetricRings", false, aDisplayName: "Metric Ring Specs.");
                AddProperty("Contractor", "");
                AddProperty("ProcessLicensor", "");
                AddProperty("TrayVendor", "", bOptional: true);
                AddProperty("ErrorLimit", 2.5, aDisplayName: "Spout Area Error Limit", aUnitType: uppUnitTypes.Percentage, bSetDefault: true, bOptional: true);
                AddProperty("ConvergenceLimit", 0.00001, aDisplayName: "Convergence Limit", bSetDefault: true, bOptional: true);
                AddProperty("InstallationType", uppInstallationTypes.GrassRoots, aDecodeString: "0=Grassroots,1=Revamp");
                AddProperty("SpacingMethod", uppMDSpacingMethods.Weighted, aDecodeString: "0=Standard,1=Weighted", bSetDefault: true);
                AddProperty("ReverseSort", false);
                // Modified by CADfx
                AddProperty("SparePercentage", 5, uppUnitTypes.Percentage, aDisplayName: "Hardware Spare Pct.");
                // Modified by CADfx
                AddProperty("ColumnSketchCount", 1, bIsHidden: true);
                AddProperty("MetricSpouting", true);
                AddProperty("ClipSparePercentage", 2, uppUnitTypes.Percentage, aDisplayName: "Clip Spare Pct.", bOptional: true);
                AddProperty("CustomerDrawingUnits", uppUnitFamilies.Metric, aDecodeString: "0=English,1=Metric", bOptional: true);
                AddProperty("ManufacturingDrawingUnits", uppUnitFamilies.Metric, aDecodeString: "0=English,1=Metric", bOptional: true);
                AddProperty("DowncomerRoundToLimit", uppMDRoundToLimits.Sixteenth, aDecodeString: "0=Sixteenth,1=Millimeter,2=None", bOptional: true);
                AddProperty("DataFilename", "");



                //System.Diagnostics.Debug.WriteLine(base.GetProps().Count);


                _Customer.SubPart(this);
                _Column.SubPart(this);
                _Specs = uopMaterialSpecs.DefaultProjectSpecs;
                _Specs.ProjectHandle = Handle;
                _RingSpecs = new colUOPRingSpecs();
            }
            catch (Exception exception)
            {
                LoggerManager log = new LoggerManager();
                log.LogError(exception.Message);
            }
        }

        #endregion Constructors

        #region Messages
        void IEventSubscriber<Message_ProjectRequest>.OnAggregateEvent(Message_ProjectRequest message)
        {
            if (string.Compare(message.ProjectHandle, Handle, ignoreCase: true) == 0)
                message.Project = this;
        }

        void IEventSubscriber<Message_PartsInvalidated>.OnAggregateEvent(Message_PartsInvalidated message)
        {
            if (string.Compare(message.ProjectHandle, Handle, ignoreCase: true) == 0)
            {
                if (ProjectType == uppProjectTypes.MDDraw)
                {
                    if (_Parts != null)
                        _Parts.InvalidateParts(message);
                }
            }

        }
        #endregion Messages

        public override uppUnitFamilies CustomerDrawingUnits
        {
            get => ProjectType == uppProjectTypes.MDDraw ? uopEnums.GetEnumValue<uppUnitFamilies>(PropValI("CustomerDrawingUnits")) : uppUnitFamilies.English;

            set
            {
                if (value != uppUnitFamilies.English && value != uppUnitFamilies.Metric) value = uppUnitFamilies.Metric;
                Notify(PropValSet("CustomerDrawingUnits", value));
            }
        }
        public override uppUnitFamilies ManufacturingDrawingUnits
        {
            get => ProjectType == uppProjectTypes.MDDraw ? uopEnums.GetEnumValue<uppUnitFamilies>(PropValI("ManufacturingDrawingUnits")) : uppUnitFamilies.English;

            set
            {
                if (value != uppUnitFamilies.English && value != uppUnitFamilies.Metric) value = uppUnitFamilies.Metric;
                Notify(PropValSet("ManufacturingDrawingUnits", value));
            }
        }

        public mdProject Clone() => new mdProject(this);

        public override uopPart Clone(bool aFlag = false) => (uopPart)this.Clone();

        public override bool Locked { get => _Locked; set => _Locked = value; }

        public override string DefaultFileName => $"{FriendlyFileName}.{FileExtension}";

        public override bool HasReport => true;
        public override void Destroy()
        {
            base.Destroy();
            uopGlobals.goEvents.UnRegisterProject(this);
            _Customer.eventCustomerPropertyChange -= _Customer_CustomerPropertyChange;

            _Column.eventColumnPropertyChange -= _Column_ColumnPropertyChange;
            _Column.eventRangeCountChange -= _Column_RangeCountChange;

            _ChimneyTrays.eventChimneyTrayCountChange -= _ChimneyTrays_ChimneyTrayCountChange;
            _ChimneyTrays.eventChimneyTrayMemberChanged -= _ChimneyTrays_ChimneyTrayMemberChanged;

            _Distributors.eventDistributorCountChange -= _Distributors_DistributorCountChange;
            _Distributors.eventDistributorMemberChanged -= _Distributors_DistributorMemberChanged;

            _Customer.Destroy();
            _Column.Destroy();
            _ChimneyTrays.Dispose(true);
            _Distributors.Dispose(true);

            _Stages?.Dispose(true);
            _Stages = null;

            _Documents?.Dispose(true);
            _Documents = null;

            _LogFile = null;
            _RingSpecs.Dispose(true);
            _RingSpecs = null;

            _Customer = null;
            _Column = null;
            _ChimneyTrays = null;
            _Distributors = null;
            GC.Collect();

        }
        public override uopTable GetTable(string aTableName, string aList = "") => mdTables.GetProjectTable(this, aTableName, aList);

        public mzValues DetailList()
        {
            mzValues _rVal = new mzValues();
            mdTrayRange aRng;
            colUOPTrayRanges aRngs = TrayRanges;
            for (int i = 1; i <= aRngs.Count; i++)
            {
                aRng = (mdTrayRange)aRngs.Item(i);
                aRng.TrayAssembly.DetailList(_rVal);
            }
            return _rVal;
        }

        public void ManwayClipsClamps(out uopManwayClip rClip, out uopManwayClamp rClamp)
        {
            rClamp = null;
            rClip = null;
            colUOPTrayRanges aRngs = TrayRanges;
            mdTrayRange aRng;
            mdTrayAssembly aAssy;
            int qty1 = 0;
            int qty2 = 0;
            List<mdDeckSection> mWays = null;

            mdDeckSection aDS = null;
            double qty = 0;

            for (int i = 1; i <= aRngs.Count; i++)
            {
                aRng = (mdTrayRange)aRngs.Item(i);
                aAssy = aRng.TrayAssembly;
                mWays = aAssy.DeckSections.Manways;
                if (mWays.Count > 0)
                {
                    if (aAssy.DesignOptions.UseManwayClips)
                    {
                        if (rClip == null) rClip = aAssy.ManwayClip;
                        qty = 0;
                        for (int j = 1; j <= mWays.Count; j++)
                        {
                            aDS = mWays[j - 1];
                            qty = qty + aDS.GenHolesV(aAssy, "MANWAY").Count + aDS.OccuranceFactor;
                        }
                        qty1 = (int)(qty1 + qty * aAssy.TrayCount);
                        rClip.AssociateToRange(aRng.GUID);
                    }
                    else
                    {
                        if (rClamp == null) rClamp = aAssy.ManwayClamp;
                        qty = 0;
                        for (int j = 1; j <= mWays.Count; j++)
                        {
                            aDS = mWays[j - 1];
                            qty = qty + aDS.GenHolesV(aAssy, "MANWAY").Count + aDS.OccuranceFactor;
                        }
                        qty2 = (int)(qty2 + qty * aAssy.TrayCount);
                        rClamp.AssociateToRange(aRng.GUID);
                    }
                }
            }
            if (rClip != null) rClip.Quantity = qty1;
            if (rClamp != null) rClamp.Quantity = qty2;
        }


        public override string TrayVendor { get => PropValS("TrayVendor"); set => Notify(PropValSet("TrayVendor", value)); }


    
        public uopPartList<mdEndAngle> EndAngles(string aRangeGUID = null, string aBoxPN = null, bool? bChamfered = null)
        {
            mdPartMatrix matrix = GetParts();

            uopPartList<mdEndAngle> eas = matrix.EndAngles(aRangeGUID, aBoxPN);

            if (bChamfered.HasValue) eas.RemoveAll(x => x.Chamfered != bChamfered.Value);

            return eas;



        }
        /// <summary>
        /// the bolting setting for all parts defined in the project
        /// passed down to all defined tray assemblies when set.
        /// English or Metric.  default = English
        /// </summary>
        public override uppUnitFamilies Bolting
        {
            get => uopEnums.GetEnumValue<uppUnitFamilies>(PropValI("Bolting"));

            set
            {
                if (value != uppUnitFamilies.English && value != uppUnitFamilies.Metric) value = uppUnitFamilies.Metric;
                Notify(PropValSet("Bolting", value));
            }
        }
        /// <summary>
        ///returns all the defined calculations for the project
        /// </summary>
        public override uopDocuments Calculations() => Documents().GetByDocumentType(uppDocumentTypes.Calculation);
        /// <summary>
        /// a collection of chimney trays defined in the project
        /// </summary>
        public override colMDChimneyTrays ChimneyTrays
        {
            get
            {
                if (_ChimneyTrays == null)
                {
                    _ChimneyTrays = new colMDChimneyTrays();
                    _ChimneyTrays.eventChimneyTrayCountChange += _ChimneyTrays_ChimneyTrayCountChange;
                    _ChimneyTrays.eventChimneyTrayMemberChanged += _ChimneyTrays_ChimneyTrayMemberChanged;

                }

                _ChimneyTrays.SubPart(Column);
                return _ChimneyTrays;
            }

        }



        public colMDSpoutGroups SpoutGroups(colUOPTrayRanges aTrayCol = null, bool aLimitedBounds = false, bool aTriangularBounds = false)
        {
            colMDSpoutGroups _rVal = new colMDSpoutGroups();
            try
            {

                colUOPTrayRanges aCol = aTrayCol ?? TrayRanges;
                colMDSpoutGroups aSGs;
                mdTrayRange aRange;
                mdSpoutGroup aSG;
                bool bKeep = false;
                for (int i = 1; i <= aCol.Count; i++)
                {
                    aRange = (mdTrayRange)aCol.Item(i);
                    aSGs = aRange.TrayAssembly.SpoutGroups;
                    for (int j = 1; j <= aSGs.Count; j++)
                    {
                        aSG = aSGs.Item(j);
                        bKeep = aSG.SpoutCount() > 0;
                        if (bKeep)
                        {
                            if (aLimitedBounds || aTriangularBounds)
                            {
                                if (aTriangularBounds)
                                    if (!aSG.TriangularBounds)
                                        bKeep = false;
                                if (aLimitedBounds)
                                    if (!aSG.LimitedBounds)
                                        bKeep = false;
                            }
                        }
                        if (bKeep) _rVal.Add(aSG);
                    }
                }

                return _rVal;
            }
            catch (Exception exception)
            {
                LoggerManager log = new LoggerManager();
                log.LogError(exception.Message);
                return null;
            }
        }

        public override uopProjectSpecs MaterialSpecs => _Specs;

        /// <summary>
        /// the only column in the project
        /// </summary>
        public override uopColumn Column
        {
            get
            {
                if (_Column == null)
                {
                    _Column = new uopColumn(this);
                    _Column.eventColumnPropertyChange += _Column_ColumnPropertyChange;
                    _Column.eventRangeCountChange += _Column_RangeCountChange;
                }
                _Column.Index = 1;
                _Column.SubPart(this);
                return _Column;
            }
        }

        /// <summary>
        /// the number of column sketchs to put in the main spouting report
        /// </summary>
        public int ColumnSketchCount
        {
            get => PropValI("ColumnSketchCount"); set => Notify(PropValSet("ColumnSketchCount", Math.Abs(value)));
        }
        /// <summary>
        /// the name assigned to the projects column
        /// </summary>
        public string ColumnName
        {
            get
            {
                string _rVal = Column.Name;
                if (string.IsNullOrWhiteSpace(_rVal))
                {
                    _rVal = Customer.Item;
                    if (!string.IsNullOrWhiteSpace(_rVal))
                        _rVal += " (" + Customer.Service + ")";
                    else
                        _rVal = Customer.Service;
                }
                return _rVal;
            }
        }

        /// <summary>
        /// the name of the contractor assigned to the project
        /// </summary>
        public override string Contractor { get => PropValS("Contractor"); set => Notify(PropValSet("Contractor", value.Trim())); }


        /// <summary>
        /// controls how downcomer box and weir lengths are rounded
        /// </summary>

        public uppMDRoundToLimits DowncomerRoundToLimit
        {
            get
            {
                return (uppMDRoundToLimits)PropValI("DowncomerRoundToLimit");

            }
            set => Notify(PropValSet("DowncomerRoundToLimit", value));

        }
        /// <summary>
        /// the percent deviation that a downcomer or deck panels spout area can deviate from it's ideal without being considered out of spec.
        /// this is the number applied to the calculation of the downcomers center to center dimension
        /// </summary>

        public double ConvergenceLimit
        {
            get
            {
                double _rVal = PropValD("ConvergenceLimit");
                if (_rVal <= 0) { _rVal = 0.00001; PropValSet("ConvergenceLimit", _rVal, bSuppressEvnts: true); }
                return _rVal;
            }
            set => Notify(PropValSet("ConvergenceLimit", value));

        }
        /// <summary>
        /// converts a md spout project to an md draw project
        /// </summary>
        /// <returns></returns>

        public mdProject ConvertToMDD()
        {
            mdTrayRange aRange;
            colUOPTrayRanges aRanges = TrayRanges;

            double aSpacing = 0;
            mdDowncomer aDC;
            if (ProjectType != uppProjectTypes.MDSpout) return this;
            ReadStatus($"Converting Spouting Project {Name} To MD Draw Project Type");

            ProjectType = uppProjectTypes.MDDraw;
            ImportFileName = DataFileName;
            DataFileName = string.Empty;

            PropSetAttributes("ConvergenceLimit", bHidden: true);
            PropSetAttributes("SpacingMethod", bHidden: true);
            PropSetAttributes("ErrorLimit", bHidden: true);
            mdTrayAssembly aAssy;

            for (int i = 1; i <= aRanges.Count; i++)
            {
                aRange = (mdTrayRange)aRanges.Item(i);
                string rngname = aRange.Name(true);
                aRange.ProjectType = uppProjectTypes.MDDraw;
                aAssy = aRange.TrayAssembly;
                ReadStatus($"Generating {rngname} Deck Splices");
                mdDesignOptions dops = aAssy.DesignOptions;
                double pwidth = aAssy.FunctionalPanelWidth - 2 * mdGlobals.DefaultPanelClearance;

                uppSpliceStyles sstyle = (aAssy.ManholeID - 0.5d < pwidth) ? uppSpliceStyles.Tabs : uppSpliceStyles.Angle;
                dops.PropValSet("SpliceStyle", sstyle, bSuppressEvnts: true);

                aAssy.GenerateSplices(true);

                ReadStatus($"Generating {rngname} Deck Sections");
                aAssy.GenerateDeckSections(true);

                ReadStatus($"Generating {rngname} Stiffeners");
                aSpacing = aAssy.DesignOptions.StiffenerSpacing;

                colUOPParts assyStfs = mdUtils.StiffenersByDowncomers(aAssy.Downcomers, aAssy, aSpacing, true, false);
                aAssy.SetStiffeners(assyStfs, aSpacing);
                aAssy.DesignOptions.PropValSet("CrossBraces", false, bSuppressEvnts: true);
                aAssy.DesignOptions.PropValSet("MaxRingClipSpacing", 9, bSuppressEvnts: true);
                aAssy.DesignOptions.PropValSet("MoonRingClipSpacing", 9, bSuppressEvnts: true);
                if (aAssy.OddDowncomers)
                {
                    aDC = aAssy.Downcomers.LastMember();
                    aDC.BoltOnEndplates = true;
                }

                aAssy.PropValSet("OverrideStartupLength", aAssy.Downcomer().PropValD("StartUpLength"), bSuppressEvnts: true);
                aAssy.StartupSpouts.Locked = true;
            }
            PropValSet("ImportfileName", PropValS("DatafileName"), bSuppressEvnts: true);
            PropValSet("DatafileName", "", bSuppressEvnts: true);
            _Documents.Invalid = true;
            return this;
        }



        /// <summary>
        ///returns the objects properties in a collection
        /// signatures like "Name=Project1"
        /// </summary>
        public override uopProperties CurrentProperties() { UpdatePartProperties(); return base.CurrentProperties(); }

        public override uopProperty CurrentProperty(string aPropertyName, bool bSupressNotFoundError = false)
        {
            UpdatePartProperties();

            return base.CurrentProperty(aPropertyName, bSupressNotFoundError);
        }


        /// <summary>
        /// a uopCustomer object property used to carry customer related data
        /// </summary>
        public override uopCustomer Customer
        {
            get
            {
                if (_Customer == null)
                {
                    _Customer = new uopCustomer();
                    _Customer.eventCustomerPropertyChange += _Customer_CustomerPropertyChange;
                }
                _Customer.SubPart(this);
                return _Customer;
            }
            set
            {
                if (value == null) return;
                _Customer = value;
                _Customer.eventCustomerPropertyChange += _Customer_CustomerPropertyChange;
                if (_Customer != null) _Customer.ProjectHandle = Handle;
            }
        }

        /// <summary>
        /// ///returns a tray assembly that is the default for this tray range
        /// return changes depending on the configuration property of the range
        /// </summary>
        /// <param name="TrayRange"></param>
        /// <returns></returns>
        public mdTrayAssembly DefaultTray(mdTrayRange TrayRange)
        {
            if (TrayRange == null) return null;
            try
            {
                mdTrayAssembly _rVal = new mdTrayAssembly(null, TrayRange);
                if (TrayRange.ShellID <= 0)
                {
                    TrayRange.BoltBarThk = 0.236;
                    TrayRange.RingStart = 1;
                    TrayRange.RingEnd = 2;
                    TrayRange.RingID = 144.5;
                    TrayRange.ShellID = 150;
                }
                _rVal.Downcomer().Count = 1;
                _rVal.Downcomer().ASP = 200;
                return _rVal;
            }
            catch (Exception exception)
            {
                LoggerManager log = new LoggerManager();
                log.LogError(exception.Message);
                return null;
            }
        }

        public override uopTrayAssembly DefaultTray(uopTrayRange RHS) { return (uopTrayAssembly)this.DefaultTray((mdTrayRange)RHS); }

        /// <summary>
        /// the Designer assigned to the project
        /// </summary>
        public override string Designer { get => PropValS("Designer").ToString(); set => Notify(PropValSet("Designer", value.Trim())); }

        /// <summary>
        /// a collection of distributors defined in the project
        /// </summary>
        public override colMDDistributors Distributors
        {
            get
            {

                if (_Distributors == null)
                {
                    _Distributors = new colMDDistributors();
                    _Distributors.eventDistributorCountChange += _Distributors_DistributorCountChange;
                    _Distributors.eventDistributorMemberChanged += _Distributors_DistributorMemberChanged;

                }

                _Distributors.SubPart(Column);
                return _Distributors;
            }

        }

        /// <summary>
        /// the drawings,calcs,reports and warnings defined for the project
        /// </summary>
        /// <param name="bRefresh"></param>
        /// <returns></returns>
        public override uopDocuments Documents(bool bRefresh = false)
        {
            if (_Documents.Invalid || bRefresh)
                GenerateDocuments(bRefresh);
            return _Documents;
        }


        /// <summary>
        /// the drawings defined in the project
        /// </summary>
        public override uopDocuments Drawings() => Documents().GetByDocumentType(uppDocumentTypes.Drawing);

        /// <summary>
        /// the percent deviation that a spout area can deviate from it's ideal without being considered out of spec.
        /// </summary>
        /// 
        public double ErrorLimit { get => PropValD("ErrorLimit"); set => Notify(PropValSet("ErrorLimit", value)); }


        /// <summary>
        /// the properties required to save the project to file
        /// </summary>
        /// <param name="aAppName"></param>
        /// <param name="aAppVersion"></param>
        /// <returns></returns>
        public uopPropertyArray FileProperties(string aAppName, string aAppVersion, string aDataFileName)
        {
            uopPropertyArray _rVal = new uopPropertyArray() { Name = $"{Name} - File Properties" };
            try
            {
                if (string.IsNullOrWhiteSpace(aAppName)) aAppName = mzUtils.GetLastString(Assembly.GetExecutingAssembly().GetName().Name);
                aAppVersion ??= string.Empty;
                aAppVersion = aAppVersion.Trim();

                colUOPTrayRanges aRanges = TrayRanges;

                uopPropertyArray props;
                AppName = aAppName;
                AppVersion = aAppVersion;
                if (string.IsNullOrWhiteSpace(aDataFileName)) aDataFileName = DataFileName;
                PropValSet("DataFilename", aDataFileName, bSuppressEvnts: true);
                uopProperties myProps = CurrentProperties();

                string heading = "APPINFO";

                _rVal.Add(new uopProperty("Application", "WinTray"), heading);

                _rVal.Add(new uopProperty("Version", Version), heading);

                uopProperties subProps = new uopProperties() { Name = "APPINFO" };

                heading = "PROJECT";
                _rVal.Add(new uopProperty("ProjectType", ProjectTypeName), heading);
                _rVal.Add(new uopProperty("Name", ProjectName), heading);

                DLLVersion = mzUtils.VersionToDouble(aAppVersion, DLLVersion);

                _rVal.Add(new uopProperty("DLL.Version", DLLVersion), heading);

                subProps = new uopProperties() { Name = "PROJECT" };
                _rVal.Add(myProps, heading, heading);
                _rVal.Add(new uopProperty("LastSaveDate", DateTime.Now.ToString()), heading);

                _rVal.Add(new uopProperty("ProjectFolder", ProjectFolder), heading);
                _rVal.Add(new uopProperty("DisplayUnits", DisplayUnits) { DecodeString = "0=English,1=Metric" }, heading);
                _rVal.Add(new uopProperty("RangeCount", TrayRanges.Count), heading);

                uopProperties notes = Notes;
                for (int i = 1; i <= notes.Count; i++)
                {
                    _rVal.Add(new uopProperty($"Note{i}", notes.Item(i).ValueS), heading);
                }



                try
                {


                    _rVal.Append(Customer.SaveProperties());

                    if (ProjectType == uppProjectTypes.MDDraw)
                    {
                        subProps = DrawingNumbers;
                        _rVal.Add(subProps, aName: "DRAWING NUMBERS", aHeading: "DRAWING NUMBERS");

                    }

                    _rVal.Append(Distributors.SaveProperties());
                    _rVal.Append(ChimneyTrays.SaveProperties());
                    _rVal.Append(Stages.SaveProperties());

                    _rVal.Append(Column.SaveProperties());


                    for (int i = 1; i <= aRanges.Count; i++)
                    {
                        mdTrayRange aRange = (mdTrayRange)aRanges.Item(i);

                        _rVal.Append(aRange.SaveProperties());

                        mdTrayAssembly aAssy = aRange.TrayAssembly;



                        props = aAssy.SaveProperties();
                        _rVal.Append(props);
                        props = aAssy.Deck.SaveProperties();
                        _rVal.Append(props);
                        props = aAssy.DesignOptions.SaveProperties();
                        _rVal.Append(props);
                        props = aAssy.Downcomer().SaveProperties(aAssy, true);
                        _rVal.Append(props);

                        props = aAssy.Constraints.SaveProperties();
                        _rVal.Append(props);
                        props = aAssy.SpoutGroups.SaveProperties();
                        _rVal.Append(props);
                        props = aAssy.Downcomers.SaveProperties();
                        _rVal.Append(props);
                        if (aRange.DesignFamily.IsBeamDesignFamily())
                        {
                            props = aAssy.Beam.SaveProperties();
                            _rVal.Append(props);

                        }

                        if (ProjectType == uppProjectTypes.MDDraw)
                        {
                            props = aAssy.DeckSplices.SaveProperties(aAssy);
                            _rVal.Append(props);

                            if (aRange.DesignFamily.IsEcmdDesignFamily())
                                props = aAssy.SlotZones.SaveProperties(aAssy);
                            _rVal.Append(props);


                        }
                    }
                    return _rVal;
                }
                catch (Exception exception)
                {
                    LoggerManager log = new LoggerManager();
                    log.LogError(exception.Message);
                    return null;
                }
            }
            catch (Exception exception)
            {
                LoggerManager log = new LoggerManager();
                log.LogError(exception.Message);
                return null;
            }
        }

        /// <summary>
        /// the filter string to use in a common dialogue to select this project type
        /// </summary>
        /// 
        public override string Filter => $"{ProjectTypeName} (*.{FileExtension})|*.{FileExtension}";


        /// <summary>
        ///returns a string that can be used as a valid filename based on the project's name property
        /// </summary>
        /// <returns></returns>
        public override string FriendlyFileName => uopUtils.FriendlyFileName(Name);


        /// <summary>
        /// make sure all persistent sub parts are up to date
        /// </summary>
        public override void UpdatePersistentSubParts(bool bForceRegen = false, string aRangeGUID = null)
        {
            Column.UpdatePersistentSubParts(this, bForceRegen, aRangeGUID);

        }


        public void GenerateDocuments(bool bForceRefresh)
        {

            uopDocuments pReports = null;
            if (_Documents != null && !bForceRefresh)
            {
                pReports = _Documents.GetByDocumentType(uppDocumentTypes.Report, bReturnClones: false, bRemove: true);
                if (pReports.Count <= 0 || _Documents.Invalid) pReports = null;
            }


            _Documents.Dispose(true);
            _Documents = new uopDocuments
            {
                InvalidWhenEmpty = true
            };
            ReadStatus($"Generating {Name} Documents");

            GenerateDrawings(_Documents);
            GenerateCalculations(_Documents);

            if (pReports == null)
                GenerateReports(_Documents);
            else
                _Documents.Append(pReports);
            _Documents.Invalid = false;
            _Documents.InvalidWhenEmpty = true;
        }
        public uopDocuments GenerateDrawings(uopDocuments aCollector)
        {
            uopDocuments _rVal = aCollector ?? new uopDocuments();

            colUOPTrayRanges rRanges = TrayRanges;

            mdTrayRange mRange;
            //colUOPParts projParts;

            //mzValues Dtls = null;

            //uopDocDrawing DtlDWG = null;
            List<string> allTrays = new List<string>();
            List<string> ecmdTrays = new List<string>();
            uopDocDrawing aDWG;
            string defTray = string.Empty;
            mzQuestions ecmdQ = null;
            string tooltip;
            string aLst;
            List<string> dcqry = new List<string>();
            List<string> dcpns = new List<string>();
            mzQuestions dcquestion;
            int sht = 0;
            bool attachmentsrequired = TrayRanges.FindIndex((x)=> x.DesignFamily.IsBeamDesignFamily()) > -1;
            uppUnitFamilies cust = CustomerDrawingUnits;
            uppUnitFamilies mfg = ManufacturingDrawingUnits;
            mzQuestions trayQunits = new mzQuestions();
            mzQuestions trayQ = new mzQuestions();
            if (rRanges.Count <= 0) return _rVal;

        
            string rangename;
            sht = 1;
            ReadStatus($"Generating {Name} Drawings");

            for (int i = 1; i <= rRanges.Count; i++)
            {
                mRange = (mdTrayRange)rRanges.Item(i);

                rangename = mRange.Name(false);
                allTrays.Add(rangename);
                if (mRange.DesignFamily.IsEcmdDesignFamily()) ecmdTrays.Add(rangename);
                if (mRange.Selected) defTray = rangename;

                dcqry.Add($"{rangename}|{mzUtils.CreateIndexList(1, mRange.TrayAssembly.Downcomers.GetByVirtual(false).Count, aItemPrefix: "Downcomer ", aDelimitor: "|")}");

                if (mRange.DesignFamily.IsEcmdDesignFamily()) ecmdTrays.Add(rangename);
                if (mRange.Selected) defTray = rangename;
            }


            trayQ.AddSingleSelect("Select Tray Range:", allTrays, defTray); //!! dont change the prompt from "Select Tray Range:" !!
            if (ecmdTrays.Count > 0)
            {
                ecmdQ = new mzQuestions();
                ecmdQ.AddSingleSelect("Select Tray Range:", ecmdTrays, defTray); //!! dont change the prompt from "Select Tray Range:" !!
                ecmdQ.AddSingleSelect("Drawing Units", new List<string> { "English", "Metric" }, "English");

            }


            trayQunits.AddSingleSelect("Select Tray Range:", allTrays, defTray); //!! dont change the prompt from "Select Tray Range:" !!
            trayQunits.AddSingleSelect("Drawing Units", new List<string> { "English", "Metric" }, mfg == uppUnitFamilies.English ? "English" : "Metric");
            if (ProjectType == uppProjectTypes.MDDraw)
            {
                //================= FUNCTIONAL & INSTALLATION =========================================


                for (int i = 1; i <= rRanges.Count; i++)
                {
                    mRange = (mdTrayRange)rRanges.Item(i);

                    rangename = mRange.Name(false);

                    _Documents.AddDrawing(uppDrawingFamily.Functional, mRange, $"Functional {rangename}", $"Functional {rangename}", uppDrawingTypes.Functional,
                       uppBorderSizes.DSize_Landscape, aCategory: "FUNCTIONAL", bCancelable: true, aUnits: uppUnitFamilies.English);

                    _Documents.AddDrawing(uppDrawingFamily.Installation, mRange, $"Installation {rangename}", $"Installation {rangename}", uppDrawingTypes.Installation,
                       uppBorderSizes.DSize_Landscape, aCategory: "INSTALLATION", bCancelable: true, aUnits: cust);

                    if (attachmentsrequired && mRange.DesignFamily.IsBeamDesignFamily())
                    {
                        _Documents.AddDrawing(uppDrawingFamily.Attachment, mRange, $"I-Beam Supports {rangename}", $"I-Beam Supports {rangename}", uppDrawingTypes.BeamAttachments,
                     uppBorderSizes.DSize_Landscape, aCategory: "ATTACHMENTS", bCancelable: true, aUnits: cust);


                    }
                }



                //=================== FABRICATION DRAWINGS===================================
                _Documents.NewMemberCategory = "MANUFACTURING";
                _Documents.NewMemberSubCategory = string.Empty;


                ReadStatus($"Generating {Name} Standard Sheets");
                {
                    aDWG = _Documents.AddDrawing(uppDrawingFamily.Manufacturing, this, $"Sheet {sht} ", $"Sheet {sht} (Sheet Index)", uppDrawingTypes.SheetIndex, uppBorderSizes.BSize_Landscape, bProjectWide: true, aSheetIndex: sht, aUnits: mfg);
                    sht++;

                    aLst = "SHEET NO.|TITLE";
                    mzUtils.ListAdd(ref aLst, "1|SHEET INDEX");
                    mzUtils.ListAdd(ref aLst, "2|MATERIAL SPECIFICATIONS");
                    mzUtils.ListAdd(ref aLst, "3|MANUFACTURING NOTES");
                    mzUtils.ListAdd(ref aLst, "4|STANDARD DECK DETAILS AND VIEWS");

                    aDWG.Tag = aLst;

                    aDWG = _Documents.AddDrawing(uppDrawingFamily.Manufacturing, this, "Materials & Specs.", $"Sheet {sht} (Materials & Specs.)", uppDrawingTypes.Sheet2, uppBorderSizes.BSize_Landscape, bProjectWide: true, aSheetIndex: sht, aUnits: mfg);
                    sht++;

                    aDWG = _Documents.AddDrawing(uppDrawingFamily.Manufacturing, this, "Manufacturing Notes", $"Sheet {sht} (Manufacturing Notes)", uppDrawingTypes.Sheet3, uppBorderSizes.BSize_Landscape, bProjectWide: true, aSheetIndex: sht, aUnits: mfg);
                    sht++;

                    aDWG = _Documents.AddDrawing(uppDrawingFamily.Manufacturing, this, "Standard Deck Details and Views", $"Sheet {sht} (Standard Deck Details and Views)", uppDrawingTypes.Sheet4, uppBorderSizes.BSize_Landscape, bProjectWide: true, aSheetIndex: sht, aUnits: mfg);
                    sht++;

                    

                }



                {
                    mdPartMatrix myparts = GetParts();

                    uppDrawingTypes dwgtype = uppDrawingTypes.Undefined;
                    //string dwgname;
                    //bool splanglesadded = false;
                    string subcat;
                    uppPartTypes ptype = uppPartTypes.Undefined;
                    // uopPartArray commonparts = GenerateCommonParts(true);

                    ReadStatus($"Generating {Name} Mfg. Drawings");

                    //============== TABULAR PARTS ===========================
                    #region Tabular Parts
                    if (myparts.EndAngles().Count > 0)
                        _Documents.Add(new uopDocDrawing(uppDrawingFamily.Manufacturing, uppDrawingTypes.EndAngles, $"End Angles (All Trays)", aBorderSize: uppBorderSizes.BSize_Landscape, bProjectWide: true, aSheetIndex: sht, aUnits: mfg, aSubCat: "TABULAR PARTS"), ref sht);

                    if (myparts.ManwaySplicePlates().Count > 0)
                        _Documents.Add(new uopDocDrawing(uppDrawingFamily.Manufacturing, uppDrawingTypes.ManwaySplicePlates, $"Manway Splices (All Trays)", aBorderSize: uppBorderSizes.BSize_Landscape, bProjectWide: true, aSheetIndex: sht, aUnits: mfg, aSubCat: "TABULAR PARTS"), ref sht);

                    if (myparts.SpliceAngles().Count > 0 || myparts.ManwayAngles().Count > 0)
                        _Documents.Add(new uopDocDrawing(uppDrawingFamily.Manufacturing, uppDrawingTypes.SpliceAngles, $"Splice Angles (All Trays)", aBorderSize: uppBorderSizes.BSize_Landscape, bProjectWide: true, aSheetIndex: sht, aUnits: mfg, aSubCat: "TABULAR PARTS"), ref sht);

                    if (myparts.SupplementalDeflectors().Count > 0)
                        _Documents.Add(new uopDocDrawing(uppDrawingFamily.Manufacturing, uppDrawingTypes.SupplDeflectors, $"Supplemental Deflectors (All Trays)", aBorderSize: uppBorderSizes.BSize_Landscape, bProjectWide: true, aSheetIndex: sht, aUnits: mfg, aSubCat: "TABULAR PARTS"), ref sht);

                    #endregion Tabular Parts

                    #region Individual Parts
                    var stfs = myparts.Stiffeners();
                    if (stfs.Count > 0)
                    {
                        ptype = uppPartTypes.Stiffener;
                        dwgtype = uopUtils.PartTypeToDrawingType(ptype, out string ptypename, out subcat);

                        foreach (var part in stfs)
                        {
                            tooltip = $"DCS: {part.AssociatedParentList()}";
                            _Documents.Add(new uopDocDrawing(uppDrawingFamily.Manufacturing, dwgtype, $"{part.NodeName}", aPart: part, aBorderSize: uppBorderSizes.BSize_Landscape, aSheetIndex: sht, aUnits: mfg, aSubCat: subcat) { ToolTip = tooltip }, ref sht);
                        }
                    }
                    var defls = myparts.DeflectorPlates();
                    if (defls.Count > 0)
                    {
                        ptype = uppPartTypes.Deflector;
                        dwgtype = uopUtils.PartTypeToDrawingType(ptype, out string ptypename, out subcat);

                        foreach (var part in defls)
                        {
                            tooltip = $"DCS: {part.AssociatedParentList()}";
                            _Documents.Add(new uopDocDrawing(uppDrawingFamily.Manufacturing, dwgtype, $"{part.NodeName}", aPart: part, aBorderSize: uppBorderSizes.BSize_Landscape, aSheetIndex: sht, aUnits: mfg, aSubCat: subcat) { ToolTip = tooltip }, ref sht);

                        }
                    }
                    var eplts = myparts.EndPlates();
                    if (eplts.Count > 0)
                    {
                        ptype = uppPartTypes.EndPlate;
                        dwgtype = uopUtils.PartTypeToDrawingType(ptype, out string ptypename, out subcat);

                        foreach (var part in eplts)
                        {
                            tooltip = $"DCS: {part.AssociatedParentList()}";
                            _Documents.Add(new uopDocDrawing(uppDrawingFamily.Manufacturing, dwgtype, $"{part.NodeName}", aPart: part, aBorderSize: uppBorderSizes.BSize_Landscape, aSheetIndex: sht, aUnits: mfg, aSubCat: subcat) { ToolTip = tooltip }, ref sht);

                        }
                    }

                    var esups = myparts.EndSupports();
                    if (esups.Count > 0)
                    {
                        ptype = uppPartTypes.EndSupport;
                        dwgtype = uopUtils.PartTypeToDrawingType(ptype, out string ptypename, out subcat);

                        foreach (var part in esups)
                        {
                            string boxSuffix = part.DesignFamily.IsBeamDesignFamily() ? $" - Box {part.DowncomerBox.Index}" : "";
                            tooltip = $"DCS: {part.AssociatedParentList()}";
                            _Documents.Add(new uopDocDrawing(uppDrawingFamily.Manufacturing, dwgtype, $"{part.NodeName}{boxSuffix}", aPart: part, aBorderSize: uppBorderSizes.BSize_Landscape, aSheetIndex: sht, aUnits: mfg, aSubCat: subcat) { ToolTip = tooltip }, ref sht);

                        }
                    }

                    var bxs = myparts.Boxes();
                    if (bxs.Count > 0)
                    {
                        ptype = uppPartTypes.DowncomerBox;
                        dwgtype = uopUtils.PartTypeToDrawingType(ptype, out string ptypename, out subcat);

                        foreach (var part in bxs)
                        {
                            tooltip = string.Empty; // $"DCS: {part.AssociatedParentList()}";
                            _Documents.Add(new uopDocDrawing(uppDrawingFamily.Manufacturing, dwgtype, $"{part.NodeName}", aPart: part, aBorderSize: uppBorderSizes.BSize_Landscape, aSheetIndex: sht, aUnits: mfg, aSubCat: subcat) { ToolTip = tooltip }, ref sht);
                            dcpns.Add($"{part.PartNumber}");

                        }
                    }
                    var pans = myparts.APPans();
                    if (pans.Count > 0)
                    {
                        ptype = uppPartTypes.APPan;
                        dwgtype = uopUtils.PartTypeToDrawingType(ptype, out string ptypename, out subcat);

                        foreach (var part in pans)
                        {
                            tooltip = $"DCS: {part.AssociatedParentList()}";
                            _Documents.Add(new uopDocDrawing(uppDrawingFamily.Manufacturing, dwgtype, $"{part.NodeName}", aPart: part, aBorderSize: uppBorderSizes.BSize_Landscape, aSheetIndex: sht, aUnits: mfg, aSubCat: subcat) { ToolTip = tooltip }, ref sht);

                        }
                    }

                    var secs = myparts.DeckSections(bIncludeAltRing:true);
                   
                    if (secs.Count > 0)
                    {
                        ptype = uppPartTypes.DeckSection;
                        dwgtype = uopUtils.PartTypeToDrawingType(ptype, out string ptypename, out subcat);

                        foreach (var part in secs)
                        {
                            tooltip = string.Empty; // $"DCS: {part.AssociatedParentList()}";
                            _Documents.Add(new uopDocDrawing(uppDrawingFamily.Manufacturing, dwgtype, $"{part.NodeName}", aPart: part, aBorderSize: uppBorderSizes.BSize_Landscape, aSheetIndex: sht, aUnits: mfg, aSubCat: subcat) { ToolTip = tooltip }, ref sht);

                        }
                    }

                    var beams = myparts.SupportBeams();
                    if (beams.Count > 0)
                    {
                        ptype = uppPartTypes.TraySupportBeam;
                        dwgtype = uopUtils.PartTypeToDrawingType(ptype, out string ptypename, out subcat);

                        foreach (var part in beams)
                        {
                            tooltip = string.Empty; // $"DCS: {part.AssociatedParentList()}";
                            _Documents.Add(new uopDocDrawing(uppDrawingFamily.Manufacturing, dwgtype, $"{part.NodeName}", aPart: part, aBorderSize: uppBorderSizes.BSize_Landscape, aSheetIndex: sht, aUnits: mfg, aSubCat: subcat) { ToolTip = tooltip }, ref sht);

                        }
                    }

                    #endregion Individual Parts

                }

            }
            else
            {
                _Documents.AddDrawing(uppDrawingFamily.Design, this, "Test", "Test Drawing", uppDrawingTypes.TestDrawing, bQuestionsAreTrayPrompt: true, aQuestions: trayQunits, aUnits: uppUnitFamilies.English);
                aDWG = _Documents.AddDrawing(uppDrawingFamily.Design, this, "Spout Areas", null, uppDrawingTypes.SpoutAreas, bQuestionsAreTrayPrompt: true, aQuestions: trayQunits, aUnits: uppUnitFamilies.English);
                if (aDWG != null)
                {
                   // aDWG.Options.AddCheckVal("Include Virtual Areas?", false);
                    aDWG.Options.AddCheckVal("Number Vertices?", false);
                    //aDWG.Options.AddSingleSelect("Select a Pattern", mdUtils.SpoutPatternNames());
                }

                aDWG = _Documents.AddDrawing(uppDrawingFamily.Design, this, "Spout Group Input Sketch", null, uppDrawingTypes.SGInputSketch, bQuestionsAreTrayPrompt: true, aQuestions: trayQunits, aUnits: uppUnitFamilies.English);
                


                aDWG = _Documents.AddDrawing(uppDrawingFamily.Design, this, "Definition Lines", null, uppDrawingTypes.DefinitionLines, bQuestionsAreTrayPrompt: true, aQuestions: trayQunits, aUnits: uppUnitFamilies.English);
                 if (aDWG != null)
                {
                    aDWG.Options.AddSingleSelect("Select Rounding Limits", uopEnums.GetNameList(typeof(uppMDRoundToLimits)), DowncomerRoundToLimit.ToString(), true);
                    aDWG.Options.AddCheckVal("Include Limit Lines?", true);
                    aDWG.Options.AddCheckVal("Include Weir Lines?", false);
                    aDWG.Options.AddCheckVal("Include Outer Box Lines?", true);
                    aDWG.Options.AddCheckVal("Include Inner Box Lines?", false);
                    aDWG.Options.AddCheckVal("Include End Plate Overhangs?", true);
                    aDWG.Options.AddCheckVal("Include Virtual Downcomers?", true);
                    aDWG.Options.AddCheckVal("Include End Plates?", false);
                    aDWG.Options.AddCheckVal("Include Shelf Lines?", false);
                    aDWG.Options.AddCheckVal("Include End Support Lines?", false);
                    aDWG.Options.AddCheckVal("Include Panel Shapes?", false);
                    aDWG.Options.AddCheckVal("Include FBA Shapes?", false);

                }

                _Documents.AddDrawing(uppDrawingFamily.Design, this, "Input Sketch", null, uppDrawingTypes.InputSketch, bQuestionsAreTrayPrompt: true, aQuestions: trayQunits, aUnits: uppUnitFamilies.English);
                aDWG = _Documents.AddDrawing(uppDrawingFamily.Design, this, "Startup Lines", null, uppDrawingTypes.StartUpLines, bQuestionsAreTrayPrompt: true, aQuestions: trayQunits, aUnits: uppUnitFamilies.English);
                if (aDWG != null)
                {
                    aDWG.Options.AddSingleSelect("Select Configuration", uopEnums.GetNameList(typeof(uppStartupSpoutConfigurations)), "By Default", true);
                    aDWG.Options.AddCheckVal("Show Panels Below?", true, "select yes to draw the panels below");
                }
                _Documents.AddDrawing(uppDrawingFamily.Design, this, "Tray Sketch", null, uppDrawingTypes.TraySketch, bQuestionsAreTrayPrompt: true, aQuestions: trayQunits, aUnits: uppUnitFamilies.English);
                _Documents.AddDrawing(uppDrawingFamily.Design, this, "Functional Active Areas", null, uppDrawingTypes.FunctionalActiveAreas, bQuestionsAreTrayPrompt: true, aQuestions: trayQunits, aUnits: uppUnitFamilies.English);
                _Documents.AddDrawing(uppDrawingFamily.Design, this, "Free Bubbling Areas", "Free Bubbling Areas", uppDrawingTypes.FreeBubbleAreas, bQuestionsAreTrayPrompt: true, aQuestions: trayQunits, aUnits: uppUnitFamilies.English);
                _Documents.AddDrawing(uppDrawingFamily.Design, this, "Feed Zones", null, uppDrawingTypes.FeedZones, bQuestionsAreTrayPrompt: true, aQuestions: trayQunits, aUnits: uppUnitFamilies.English);
                _Documents.AddDrawing(uppDrawingFamily.Design, this, "Section Sketch", null, uppDrawingTypes.SectionSketch, bQuestionsAreTrayPrompt: true, aQuestions: trayQunits, aUnits: uppUnitFamilies.English);
                _Documents.AddDrawing(uppDrawingFamily.Design, this, "Downcomer Manhole Fit", null, uppDrawingTypes.DowncomerManholeFit, bQuestionsAreTrayPrompt: true, aQuestions: trayQunits, aUnits: uppUnitFamilies.English);
                return _rVal;
            }


            for (int i = 1; i <= rRanges.Count; i++)
            {
                mRange = (mdTrayRange)rRanges.Item(i);

                if (ProjectType == uppProjectTypes.MDDraw)
                {
                    mRange.GenerateDrawings(this, "", _rVal, ref sht);
                    // if (Dtls != null) mRange.TrayAssembly.DetailList(Dtls);

                }

            }

            //if (DtlDWG != null)
            //{
            //    if (Dtls.Count > 0)
            //    {
            //        DtlDWG.ProjectWide = true;
            //        object Delim = null;
            //        DtlDWG.Options.AddMultiSelect("Select Details To Include", uopGlobals.gstrDetailChoices, Dtls.ToList("|"), 1, "Detail|Title|Description", true, "|", Delim.ToString(), "10,30,60");
            //    }
            //}
            if (allTrays.Count > 0)
            {
                //=================== TRAY VIEW===================================
                _Documents.NewMemberCategory = "Tray Views";
                aDWG = _Documents.AddDrawing(uppDrawingFamily.TrayView, this, "Plan View", "Plan View", uppDrawingTypes.LayoutPlan, bCancelable: true, bQuestionsAreTrayPrompt: true, aQuestions: trayQunits, aUnits: mfg);
                aDWG.Options.AddCheckVal("Draw Full Tray?",  true);
                if (ecmdTrays.Count > 0) aDWG.Options.AddCheckVal("Suppress ECMD Slots?", false);
                aDWG.Options.AddCheckVal("Suppress Spouts?", false);
                aDWG.Options.AddCheckVal("Circles On Startup Spouts?", false);
                aDWG.Options.AddCheckVal("Draw Downcomers Below?", true);
                aDWG.Options.AddCheckVal("Show Part Numbers?", false);

                aDWG = _Documents.AddDrawing(uppDrawingFamily.Manufacturing, this, "Elevation View", "Elevation View", uppDrawingTypes.LayoutElevation, bCancelable: true, bQuestionsAreTrayPrompt: true, aQuestions: trayQunits, aUnits: mfg);
                aDWG.Options.AddCheckVal("Draw Phantom?", false);
                _Documents.NewMemberCategory = string.Empty;
            }

            if (allTrays.Count > 0)
            {
                //=================== DESIGN DRAWINGS===================================

                _Documents.NewMemberCategory = "Design Drawings";

                _Documents.AddDrawing(uppDrawingFamily.Design, this, "Test", "Test Drawing", uppDrawingTypes.TestDrawing, bQuestionsAreTrayPrompt: true, aQuestions: trayQunits, aUnits: uppUnitFamilies.English);
                aDWG = _Documents.AddDrawing(uppDrawingFamily.Design, this, "Spout Areas", null, uppDrawingTypes.SpoutAreas, bQuestionsAreTrayPrompt: true, aQuestions: trayQunits, aUnits: uppUnitFamilies.English);
                if (aDWG != null)
                {
                    //aDWG.Options.AddCheckVal("Include Virtual Areas?", false);
                    aDWG.Options.AddCheckVal("Number Vertices?", false);
                    //aDWG.Options.AddSingleSelect("Select a Pattern", mdUtils.SpoutPatternNames());
                }
                aDWG = _Documents.AddDrawing(uppDrawingFamily.Design, this, "Spout Group Input Sketch", null, uppDrawingTypes.SGInputSketch, bQuestionsAreTrayPrompt: true, aQuestions: trayQunits, aUnits: uppUnitFamilies.English);

                aDWG = _Documents.AddDrawing(uppDrawingFamily.Design, this, "Definition Lines", null, uppDrawingTypes.DefinitionLines, bQuestionsAreTrayPrompt: true, aQuestions: trayQunits, aUnits: uppUnitFamilies.English);
                if(aDWG != null)
                {
                    aDWG.Options.AddSingleSelect("Select Rounding Limits", uopEnums.GetNameList(typeof(uppMDRoundToLimits)), DowncomerRoundToLimit.ToString(), true);
                    aDWG.Options.AddCheckVal("Include Limit Lines?", true);
                    aDWG.Options.AddCheckVal("Include Weir Lines?", false);
                    aDWG.Options.AddCheckVal("Include Outer Box Lines?", true);
                    aDWG.Options.AddCheckVal("Include Inner Box Lines?", false);
                    aDWG.Options.AddCheckVal("Include End Plate Overhangs?", true);
                    aDWG.Options.AddCheckVal("Include Virtual Downcomers?", true);
                    aDWG.Options.AddCheckVal("Include End Plates?", false);
                    aDWG.Options.AddCheckVal("Include Shelf Lines?", false);
                    aDWG.Options.AddCheckVal("Include End Support Lines?", false);
                    aDWG.Options.AddCheckVal("Include Panel Shapes?", false);
                    aDWG.Options.AddCheckVal("Include FBA Shapes?", false);
                }
               
                _Documents.AddDrawing(uppDrawingFamily.Design, this, "Stiffener Edit View", null, uppDrawingTypes.StiffenerEdit, bQuestionsAreTrayPrompt: true, aQuestions: trayQunits, aUnits: uppUnitFamilies.English);
                aDWG = _Documents.AddDrawing(uppDrawingFamily.Design, this, "Section Shapes", null, uppDrawingTypes.SectionShapes, bQuestionsAreTrayPrompt: true, aQuestions: trayQunits, aUnits: uppUnitFamilies.English);
                if (aDWG != null)
                {
                    aDWG.Options.AddCheckVal("Draw Both Sides?", false, "select yes to draw all section instances");
                    aDWG.Options.AddCheckVal("Only Unique Sections?", true, "select yes to only draw the unique sections");
                    aDWG.Options.AddCheckVal("Regenerate Section Shapes?", false, "select yes to force a regeneration of the section shapes");
                    aDWG.Options.AddCheckVal("Regenerate Perimeters?", uopUtils.RunningInIDE, "select yes to force a regeneration of the section shape perimeters");
                    aDWG.Options.AddCheckVal("Show Ring Clip Segments?", false, "select yes to draw the ring clip segments of each shape");
                    aDWG.Options.AddCheckVal("Show Perimeters?", true, "select yes to draw the deck section perimeter polygon of each shape");
                    aDWG.Options.AddCheckVal("Show Section Shapes?", false, "select yes to draw the basic section shape of each section");
                    aDWG.Options.AddCheckVal("Show Mechanical Bound?", false, "select yes to draw the simple mechanical boundary polyline of each shape");
                    aDWG.Options.AddCheckVal("Show Free Bubbling Areas?", false, "select yes to draw the Free Bubbling Area boundary polyline of each shape");
                    aDWG.Options.AddCheckVal("Show Slot Grid Bounds?", false, "select yes to draw the Slot Grid boundary polyline of each shape");
                }
            

            aDWG =_Documents.AddDrawing(uppDrawingFamily.Design, this, "Startup Lines", null, uppDrawingTypes.StartUpLines, bQuestionsAreTrayPrompt: true, aQuestions: trayQunits, aUnits: uppUnitFamilies.English);
                if (aDWG != null)
                {
                    aDWG.Options.AddSingleSelect("Select Configuration", uopEnums.GetNameList(typeof(uppStartupSpoutConfigurations)), "By Default", true);
                    aDWG.Options.AddCheckVal("Show Panels Below?", true, "select yes to draw the panels below");
                }
                _Documents.AddDrawing(uppDrawingFamily.Design, this, "Input Sketch", null, uppDrawingTypes.InputSketch, bQuestionsAreTrayPrompt: true, aQuestions: trayQunits, aUnits: uppUnitFamilies.English);
                aDWG = _Documents.AddDrawing(uppDrawingFamily.Design, this, "Ring Clip Segments", null, uppDrawingTypes.RingClipSegments, bQuestionsAreTrayPrompt: true, aQuestions: trayQunits, aUnits: uppUnitFamilies.English);
                if (aDWG != null)
                {
                    aDWG.Options.AddCheckVal("Draw Both Sides?", false,"select yes to draw all section instances");
                    aDWG.Options.AddCheckVal("Show Washers?", true, "select yes to draw the ring clip washers");
                    aDWG.Options.AddCheckVal("Regenerate Section Shapes?", false, "select yes to regenerate the section shapes");
                }
                _Documents.AddDrawing(uppDrawingFamily.Design, this, "Section Sketch", null, uppDrawingTypes.SectionSketch, bQuestionsAreTrayPrompt: true, aQuestions: trayQunits, aUnits: uppUnitFamilies.English);
                _Documents.AddDrawing(uppDrawingFamily.Design, this, "Functional Active Areas", null, uppDrawingTypes.FunctionalActiveAreas, bQuestionsAreTrayPrompt: true, aQuestions: trayQunits, aUnits: uppUnitFamilies.English);
                _Documents.AddDrawing(uppDrawingFamily.Design, this, "Blocked Areas", null, uppDrawingTypes.BlockedAreas, bCancelable: true, bQuestionsAreTrayPrompt: true, aQuestions: trayQunits, aUnits: uppUnitFamilies.English);
                _Documents.AddDrawing(uppDrawingFamily.Design, this, "Downcomer Manhole Fit", null, uppDrawingTypes.DowncomerManholeFit, aUnits: uppUnitFamilies.English);

               // _Documents.AddDrawing(uppDrawingFamily.Design, this, "Equal Deck Sections", null, uppDrawingTypes.EqualSections, bCancelable: true, bQuestionsAreTrayPrompt: true, aQuestions: trayQunits, aUnits: uppUnitFamilies.English);

                aDWG = _Documents.AddDrawing(uppDrawingFamily.Design, this, "Deck Sections Only", null, uppDrawingTypes.PanelsOnly, bCancelable: true, bQuestionsAreTrayPrompt: true, aQuestions: trayQunits, aUnits: uppUnitFamilies.English);
                aDWG.Options.AddCheckVal("Draw Both Sides?", true);
                aDWG.Options.AddCheckVal("Suppress ECMD Slots?", false);

                // aDWG.Options.AddCheckVal("Show Suppressed Bubble Promoters?", false);
                aDWG.Options.AddCheckVal("Show Part Numbers?", false);
                aDWG.Options.AddCheckVal("Show Quantities?", false);
                aDWG.Options.AddCheckVal("Show Splice Angles?", true);
                if (uopUtils.RunningInIDE)
                    aDWG.Options.AddCheckVal("Regenerate Deck Sections?", false);

                aDWG = _Documents.AddDrawing(uppDrawingFamily.Design, this, "Downcomers Only", null, uppDrawingTypes.DowncomersOnly, bCancelable: true, bQuestionsAreTrayPrompt: true, aQuestions: trayQunits, aUnits: uppUnitFamilies.English);
                aDWG.Options.AddCheckVal("Suppress Spouts?", false);
                aDWG.Options.AddCheckVal("Draw Both Sides?", false);
                aDWG.Options.AddCheckVal("Draw Finger Clips?", false);
                aDWG.Options.AddCheckVal("Draw End Angles?", false);
                aDWG.Options.AddCheckVal("Draw Deflector Plates?", true);
                aDWG.Options.AddCheckVal("Draw AP Pans?", true);
                aDWG.Options.AddCheckVal("Suppress Stiffeners?", false);
                aDWG.Options.AddCheckVal("Suppress End Plates?", false);
                aDWG.Options.AddCheckVal("Suppress End Supports?", false);
                aDWG.Options.AddCheckVal("Suppress Deck Support Angles?", false);
                aDWG.Options.AddCheckVal("Suppress Supplemental Deflectors?", false);
                if (ecmdTrays.Count > 0)
                {
                    aDWG =_Documents.AddDrawing(uppDrawingFamily.Design, this, "ECMD Slot Layout", null, uppDrawingTypes.PanelsAndSlots, bCancelable: true, bQuestionsAreTrayPrompt: true, aQuestions: ecmdQ, aUnits: uppUnitFamilies.English);
                    aDWG.Options.AddCheckVal("Include Deck Section Perimeters?", true);
                    aDWG.Options.AddCheckVal("Include Zone Boundary?", true);
                    aDWG.Options.AddCheckVal("Include Zone Blocked Areas?", true);
                    aDWG.Options.AddCheckVal("Show Suppressed Slots?", true);
                    aDWG.Options.AddCheckVal("Show Zone Origin?", false);
                    aDWG.Options.AddCheckVal("Show Zone Mirror Line?", false);
                    aDWG.Options.AddCheckVal("Show Grid Lines?" ,false);
                    aDWG.Options.AddCheckVal("Show Weir Lines?", true);
                    aDWG.Options.AddCheckVal("Show Splice Angles?", true);
                    aDWG.Options.AddCheckVal("Show Zone Names?", true);
                    aDWG.Options.AddCheckVal("Show Slot Center Points?", true);
                    if (uopUtils.RunningInIDE) aDWG.Options.AddCheckVal("Regenerate Slots?", false);

                    aDWG = _Documents.AddDrawing(uppDrawingFamily.Design, this, "ECMD Baffle Layout", null, uppDrawingTypes.Baffles, bCancelable: true, bQuestionsAreTrayPrompt: true, aQuestions: ecmdQ, aUnits: uppUnitFamilies.English);
                    aDWG.Options.AddCheckVal("Ommit Downcomer Notches?", false);


                }

                if(dcpns.Count > 0)
                {
                    dcquestion = new mzQuestions();
                    dcquestion.AddSingleSelect("Select Downcomer", dcpns, dcpns[0]);
                    //dcquestion.AddDualStringChoice("Select Tray:|Select Downcomer:", dcqry, $"{defTray}|Downcomer 1", aSubChoiceDelimiter: "|");
                    dcquestion.AddSingleSelect("Drawing Units", new List<string> { "English", "Metric" }, "English");
                    if (uopUtils.RunningInIDE)
                    {
                        dcquestion.AddCheckVal("Suppress End View?", false);
                        dcquestion.AddCheckVal("Center At Box End?", false);
                    }


                    aDWG = _Documents.AddDrawing(uppDrawingFamily.Design, this, "Downcomer Assembly", null, uppDrawingTypes.DowncomerDesign, bCancelable: true, aPartType: uppPartTypes.Downcomer, bQuestionsAreTrayPrompt: true, aQuestions: dcquestion, aUnits: uppUnitFamilies.English);

        
                }

                _Documents.NewMemberCategory = string.Empty;
            }
            return _rVal;
        }
        /// <summary>
        ///returns the deck material of the first tray range in the project
        /// </summary>
        public override uopSheetMetal DeckMateral { get { mdTrayRange aRange = (mdTrayRange)TrayRanges.Item(1); return (aRange != null) ? (uopSheetMetal)aRange.TrayAssembly.Deck.Material : new uopSheetMetal(); } }

        /// <summary>
        ///returns the downcomer material of the first tray range in the project
        /// </summary>
        public override uopSheetMetal DowncomerMaterial { get { mdTrayRange aRange = (mdTrayRange)TrayRanges.Item(1); return (aRange != null) ? (uopSheetMetal)aRange.TrayAssembly.Downcomer().Material : new uopSheetMetal(); } }

        public uopDocuments GenerateWarnings(uopDocuments aCollector = null, bool bJustOne = false)
        {
            uopDocuments _rVal = aCollector ?? new uopDocuments();
            colUOPTrayRanges rRanges = TrayRanges;

            ReadStatus($"Generating {Name} Warnings");
            foreach (var item in rRanges)
            {
                item.GenerateWarnings(this, aCollector: _rVal, bJustOne: bJustOne);
                if (bJustOne && _rVal.Count > 0)
                    return _rVal;
            }


            //if(ProjectType == uppProjectTypes.MDSpout)
            //{
            //    for (int i = 1; i <= Distributors.Count; i++)
            //    {
            //        Distributors.Item(i).GenerateWarnings(this, aCollector: _rVal, bJustOne: bJustOne);
            //        if (bJustOne && _rVal.Count > 0)
            //            return _rVal;
            //    }
            //    if (bJustOne && _rVal.Count > 0)
            //        return _rVal;
            //    for (int i = 1; i <= ChimneyTrays.Count; i++)
            //    {
            //        ChimneyTrays.Item(i).GenerateWarnings(this, aCollector: _rVal, bJustOne: bJustOne);
            //        if (bJustOne && _rVal.Count > 0)
            //            return _rVal;
            //    }

            //}
            return _rVal;
        }
        public uopDocuments GenerateCalculations(uopDocuments aCollector = null)
        {
            uopDocuments _rVal = aCollector ?? new uopDocuments();

            colUOPTrayRanges rRanges = TrayRanges;

            mdTrayRange mRange;
            mzQuestions trayQ = new mzQuestions();
            string allTrays = string.Empty;
            string defTray = string.Empty;
            if (rRanges.Count <= 0) return _rVal;

            ReadStatus($"Generating {Name} Calculations");

            for (int i = 1; i <= rRanges.Count; i++)
            {
                mRange = (mdTrayRange)rRanges.Item(i);
                mzUtils.ListAdd(ref allTrays, mRange.Name(false));
                if (mRange.Selected)
                    defTray = mRange.Name(false);
            }
            trayQ.AddSingleSelect(
                  "Select Tray Range:", allTrays,
                  defTray); //!! dont change the prompt from "Select Tray Range:" !!

            _rVal.NewMemberCategory = "CALCULATIONS";
            _rVal.NewMemberSubCategory = string.Empty;
            //_rVal.AddCalculation(this, "Warnings", "Warnings", uppCalculationType.Warnings, bQuestionsAreTrayPrompt: true, aQuestions: trayQ);
            if (ProjectType == uppProjectTypes.MDSpout)
            {
                _rVal.AddCalculation(this, "Mechanical Properties", "Mechanical Properties", uppCalculationType.DowncomerProperties, bQuestionsAreTrayPrompt: true, aQuestions: trayQ);
                _rVal.AddCalculation(this, "Spacing Optimization", "Spacing Optimization", uppCalculationType.MDSpacing, bQuestionsAreTrayPrompt: true, aQuestions: trayQ);
                _rVal.AddCalculation(this, "Spout Area Distribution", "Spout Area Distribution", uppCalculationType.MDSpoutArea, bQuestionsAreTrayPrompt: true, aQuestions: trayQ);
                _rVal.AddCalculation(this, "Spout Group Layout", "Spout Group Layout", uppCalculationType.MDSpoutLayout, bQuestionsAreTrayPrompt: true, aQuestions: trayQ);
                _rVal.AddCalculation(this, "Spout Group Constraints", "Spout Group Constraints", uppCalculationType.Constraints, bQuestionsAreTrayPrompt: true, aQuestions: trayQ);
                _rVal.AddCalculation(this, "Feed Zones", "Feed Zones", uppCalculationType.FeedZones, bQuestionsAreTrayPrompt: true, aQuestions: trayQ);
            }
            else
            {
                _rVal.AddCalculation(this, "Part Weights", "Part Weights", uppCalculationType.Weights, bQuestionsAreTrayPrompt: true, aQuestions: trayQ);
                _rVal.AddCalculation(this, "Downcomer Properties", "Downcomer Properties", uppCalculationType.DowncomerProperties, bQuestionsAreTrayPrompt: true, aQuestions: trayQ);
            }
            _rVal.NewMemberCategory = string.Empty;
            _rVal.NewMemberSubCategory = string.Empty;
            ReadStatus(string.Empty);
            return _rVal;

        }

        /// <summary>
        /// returns the updated parts matrix
        /// </summary>
        /// <returns></returns>

        private mdPartMatrix _Parts;

        internal mdPartMatrix Parts => _Parts;

        public void InvalidateParts(bool bInvalidateAll = false, string aRangeGUID = null, uppPartTypes? aPartType = null, List<uppPartTypes> aPartTypes = null)
        {
           if (_Parts != null) _Parts.InvalidateParts(new Message_PartsInvalidated(Project?.Handle, aRangeGUID: aRangeGUID, aPartType: aPartType, aPartTypes: aPartTypes));
        }


        public mdPartMatrix GetParts()
        {
            UpdateParts();
            return _Parts;
        }
        public override bool UpdateParts(bool bUpdateAll = false)
        {
            string statutswuz = ReadStatusString;
            bool _rVal = _Parts == null;
            _Parts ??= new mdPartMatrix(this);
            if (ProjectType == uppProjectTypes.MDDraw)
            {
                ReadStatus($"Updating Project {Name} Parts");
                try
                {
                    _Parts.eventPartGeneration -= PartMatrixGenerationHandler;
                    _Parts.eventPartGeneration += PartMatrixGenerationHandler;
                    if (_Parts.Update(this, bUpdateAll)) _rVal = true;

                }
                catch (Exception)
                {

                    throw;
                }
                finally
                {
                    ReadStatus(statutswuz);
                    _Parts.eventPartGeneration -= PartMatrixGenerationHandler;
                }
            }
            else
            {
                // MD spout projects don't have project level parts !!
                return false;
            }

            return _rVal;
        }
        private void PartMatrixGenerationHandler(string aStatusString, bool? bBegin = null)
        {
            PartGenenerationStatus(aStatusString, bBegin);
        }
        /// <summary>
        ///returns the list of available reports for the project
        /// </summary>
        /// <param name="_Documents"></param>
        public uopDocuments GenerateReports(uopDocuments aCollector = null)
        {
            uopDocuments _rVal = aCollector ?? new uopDocuments();

            uopDocReport aRpt;

            uopDocReportPage aPage;

            if (TrayRanges.Count > 0)
            {
                if (ProjectType == uppProjectTypes.MDDraw)
                {
                    aRpt = _rVal.AddReport(this, uppReportTypes.MDDCStressReport, "Downcomer Strength", true, DisplayUnits,
                           $"{Name} - Downcomer Strength", OutputFolder, true, true, bProtectSheets: true, bLookForMechTemplate: true, aTemplateName: "Downcomer Strength");
                    aRpt.AddPage(uppReportPageTypes.DCStressPage, true, bProtected: true);
                    aRpt.ProjectName = Name;
                    aRpt.AllPagesOnly = true;

                    aRpt = _rVal.AddReport(this, uppReportTypes.HardwareReport, "Tray Hardware", true, DisplayUnits,
                           $"{Name} - Hardware", OutputFolder, true, true, bProtectSheets: true, bAllPagesOnly: true, bLookForMechTemplate: true, aTemplateName: "MD-ECMD Hardware");
                    aRpt.FirstTab = "Totals";
                    aPage = aRpt.AddPage(uppReportPageTypes.HardwareTotals, true, "Totals", "Totals", bProtected: true);
                    aPage.NoTemplate = true;
                    aPage.DontSaveWithPassword = true;
                    aPage.SuppressTabName = true;
                    aRpt.AddPage(uppReportPageTypes.TrayHardwarePage, true, bProtected: true, aPassWord: "UOPMDUOP");
                    aRpt.ProjectName = Name;
                    aRpt.FirstTab = "Totals";
                }
                else
                {
                    //the main data output report for a md spout project
                    //======================================================
                    aRpt = _rVal.AddReport(this, uppReportTypes.MDSpoutReport, bIsRequested: true, aUnits: uppUnitFamilies.English, aFileName: "Spouting Data Report", FolderName: OutputFolder, bUnitsLocked: true, aFileNameLocked: true, bFolderNameLocked: true, bProtectSheets: true, bAllPagesOnly: true, bAllRangesOnly: true, bMaintainRevisionHistory: true, aTemplateName: "MDSpoutReport");

                    aRpt.UnitsLocked = true;
                    aRpt.ProjectName = Name;
                    aRpt.AddPage(uppReportPageTypes.ProjectSummaryPage, true, bProtected: true, aPassWord: "UOPMDUOP");
                    aRpt.AddPage(uppReportPageTypes.ColumnSketchPage, true, bProtected: true, aPassWord: "UOPMDUOP");
                    aRpt.AddPage(uppReportPageTypes.TraySummaryPage, true, bProtected: true, aPassWord: "UOPMDUOP");
                    aRpt.AddPage(uppReportPageTypes.MDDistributorPage, false, bProtected: true, aPassWord: "UOPMDUOP");
                    aRpt.AddPage(uppReportPageTypes.MDChimneyTrayPage, false, bProtected: true, aPassWord: "UOPMDUOP");
                    aRpt.AddPage(uppReportPageTypes.TraySketchPage, true, bProtected: true, aPassWord: "UOPMDUOP");
                    aRpt.AddPage(uppReportPageTypes.MDOptimizationPage, true, bProtected: true, aPassWord: "UOPMDUOP");
                    aRpt.AddPage(uppReportPageTypes.MDSpoutDetailPage, true, bProtected: true, aPassWord: "UOPMDUOP");
                    //Exluding Spout Sketch Page after confirming from Mike - as users will no longer be needing this page in Report
                    //aRpt.AddPage(uppReportPageTypes.MDSpoutSketchPage, true)
                    aRpt.SketchCount = ColumnSketchCount;

                    // //the downcomer spacing optimization report
                    // //======================================================

                    // aRpt = _rVal.AddReport(this, uppReportTypes.MDSpacingReport, bIsRequested: false, aUnits: uppUnitFamilies.English, aFileName: "Spacing Optimization Report",
                    // FolderName: OutputFolder, bUnitsLocked: true, aFileNameLocked: true, bFolderNameLocked: true, aTemplateName: "MDSpoutReport");

                    // aRpt.ProjectName = Name;
                    // aRpt.AddPage(uppReportPageTypes.DCSpacingOptimizationPage, true);

                    //the feed zone report
                    //======================================================

                    aRpt = _rVal.AddReport(this, uppReportTypes.MDFeedZoneReport, bIsRequested: false, aUnits: uppUnitFamilies.English, aFileName: "Feed Zone Report",
                   FolderName: OutputFolder, bUnitsLocked: true, aFileNameLocked: true, bFolderNameLocked: true, aTemplateName: "MDSpoutReportXLT");

                    aRpt.ProjectName = Name;
                    aRpt.AddPage(uppReportPageTypes.MDFeedZonePage, true);

                    //the DDM report
                    //======================================================
                    aRpt = _rVal.AddReport(this, uppReportTypes.MDEDMReport, bIsRequested: false, aUnits: uppUnitFamilies.English, aFileName: "DDM Data Report",
                    FolderName: OutputFolder, bUnitsLocked: true, aFileNameLocked: true, bFolderNameLocked: true, aTemplateName: "MDSpoutReportXLT");

                    aRpt.ProjectName = Name;
                    aRpt.AddPage(uppReportPageTypes.MDEDMPage1, true);
                    aRpt.AddPage(uppReportPageTypes.MDEDMPage2, true);
                    aRpt.AddPage(uppReportPageTypes.MDEDMPage3, true);

                    // //the Distrubutor detail
                    // //======================================================
                    // aRpt = _rVal.AddReport(this, uppReportTypes.MDDistributorReport, bIsRequested: false, aUnits: uppUnitFamilies.English, aFileName: "Distributor Detail Report.XLS",
                    // FolderName: OutputFolder, bUnitsLocked: true, aFileNameLocked: true, bFolderNameLocked: true, aTemplateName: "MDSpoutReportXLT");

                    // aRpt.ProtectSheets = true;
                    // aRpt.UnitsLocked = true;
                    // aRpt.ProjectName = Name;
                    // aRpt.AddPage(uppReportPageTypes.MDDistributorPage, false, bProtected: true, aPassWord: "UOPMDUOP");
                }
            }
            return _rVal;
        }

        /// <summary>
        /// used by test if a change has been made to the curent project
        /// </summary>
        public override bool HasChanged
        {
            get => _HasChanged;
            set
            {
                _HasChanged = value;
                if (_HasChanged && _Documents != null) _Documents.Invalid = true;
            }
        }


        /// <summary>
        /// returns true if any range has a doencomer with triangular end plates
        /// </summary>
        public override bool HasTriangularEndPlates
        {
            get
            {

                colUOPTrayRanges aRanges = TrayRanges;
                mdTrayRange aRange;
                for (int i = 1; i <= aRanges.Count; i++)
                {
                    aRange = (mdTrayRange)aRanges.Item(i);
                    if (aRange.HasTriangularEndPlates)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        /// <summary>
        /// the UOP ID number assigned to the project
        /// </summary>
        public override string IDNumber { get => PropValS("IDNumber"); set => Notify(PropValSet("IDNumber", value.Trim())); }

        /// <summary>
        ///returns the file filter string used with a common dialogue when importing md spout project data
        /// </summary>
        public override string ImportFileFilter
        {
            get
            {
                string _rVal;
                if (ProjectType == uppProjectTypes.MDDraw)
                {
                    _rVal = ProjectTypeName + " Project Importable Files (*.MDP)|*.MDP";

                    _rVal += "|MD Spout Data Files (*.MDP)|*.MDP";
                }
                else
                {
                    _rVal = ProjectTypeName + " Project Importable Files (*.MDH)|*.MDH";
                    _rVal += "|MD Hydraulics Files (*.MDH)|*.MDH";

                }
                return _rVal;
            }
        }
        /// <summary>
        /// an external file that was imported to create this project
        /// an MDH file
        /// </summary>
        public override string ImportFileName { get => PropValS("ImportFileName"); set => Notify(PropValSet("ImportFileName", value.Trim())); }
        //the file the part was last saved to
        public override string DataFileName { get => PropValS("DataFilename"); set => Notify(PropValSet("DataFilename", value)); }

        public string GasketSpec { get => _Specs.GetSpecV(uppSpecTypes.Gasket, false, false, out bool FND).Name; set => MaterialSpecs.SetSpec(uopMaterialSpecs.GetByName(uppSpecTypes.Gasket, value)); }

        /// <summary>
        /// the type of installation for th eproejct
        /// grassroots or refit
        /// </summary>
        public override uppInstallationTypes InstallationType
        {
            get => (uppInstallationTypes)PropValI("InstallationType"); set => Notify(PropValSet("InstallationType", value));
        }
        /// <summary>
        /// the UOP key number assigned to the project
        /// 5 character max
        /// </summary>
        public override string KeyNumber
        {
            get => PropValS("KeyNumber");
            set
            {
                value = (!string.IsNullOrWhiteSpace(value)) ? value.ToUpper().Trim() : "";
                if (value.Length > 6) value = value.Substring(0, 6);
                Notify(PropValSet("KeyNumber", value));
            }
        }


        public colUOPParts LooseHardware => null;

        /// <summary>
        /// the manhole ID of the first column in the project
        /// </summary>
        public override double ManholeID { get => Column.ManholeID; set => Column.ManholeID = value; }

        /// <summary>
        /// true if the projects support rings are metric
        /// </summary>
        public override bool MetricRings
        {
            get => PropValB("MetricRings"); set => Notify(PropValSet("MetricRings", value));
        }
        /// <summary>
        /// true if the projects spout lengths are rounded to the nearest millimeter
        /// </summary>
        public bool MetricSpouting { get => PropValB("MetricSpouting"); set => Notify(PropValSet("MetricSpouting", value)); }

        /// <summary>
        /// the project name
        /// wintray projects are named based on the project key number - revision
        /// </summary>
        public override string Name => ProjectName;

        public override string ProjectName => PropValS("KeyNumber") + "-" + PropValS("Revision");

        /// <summary>
        /// a collection of notes assigned to the object
        /// </summary>
        public override uopProperties Notes
        {
            get
            {

                string aStr = string.Empty;
                if (string.Compare(ProcessLicensor, "UOP LLC", true) == 0)
                    if (string.Compare(Customer.Service, "Amine Absorber", true) == 0)
                    {
                        aStr = "UOP Gas Processes Requires the Following:";
                        aStr += " Full length sloped chutes with both ends open.";
                        aStr += " Feed gas inlet nozzle to be oriented 90" + (char)176 + " to downcomers.";
                        aStr += " A feed gas distributor based on UOP Standard Drawing 3-190-9 must be applied.";
                    }
                return base.GetNotes(aStr);

            }
            set => Notify(base.SetNotes(value));

        }

        public uopProperties GetDisplayTableProperties(uppDisplayTableTypes aTableType, int aRangeIndex = 0)
        {
            return mdUtils.GetDisplayListProperties(this, aTableType, aRangeIndex);
        }

        /// <summary>
        /// used by an object to respond to property changes of itself and of objects below it in the object model.
        /// also raises the changed property to the object above it it the object model.
        /// </summary>
        /// <param name="aProperty">the property that is notifying me (the project) that it has changes</param>
        public override void Notify(uopProperty aProperty)
        {
            if (aProperty == null || aProperty.Protected) return;
            string pname;
            uppPartTypes pType;
            bool bInvalidateParts = false, bInvalidateSubParts = false;
            pname = aProperty.Name.ToUpper();
            pType = aProperty.PartType;
            if (Reading) return;
            if ("SILENT".Equals(Tag)) return;
            if (pType == uppPartTypes.Distributor || pType == uppPartTypes.DistributorCase || pType == uppPartTypes.ChimneyTray || pType == uppPartTypes.DistributorCase)
            {
                return;
            }
            if ("DRAWING NUMBERS".Equals(aProperty.Heading))
            {
                HasChanged = true;
                return;
            }
            if (!pname.Equals("LASTSAVEDATE") && !pname.Equals("DISPLAYUNITS"))
            {
                bInvalidateSubParts = true;
                HasChanged = true;
            }
            eventMDProjectPropertyChange?.Invoke(aProperty);
            if (pType == uppPartTypes.Project)
            {
                Column?.Alert(aProperty);
            }
            if (bInvalidateParts)
            {
                if (mzUtils.ListContains(pname, "COUNT,SORTORDER,SHELLID,RINGID,X,SPACING"))
                {
                    bInvalidateParts = true;
                }
            }
            if (bInvalidateParts)
            {
                _Documents.Invalid = true;
            }
            if (bInvalidateSubParts)
            {
                _Documents.Invalid = true;
            }
            if (pType == uppPartTypes.Project && (pname.ToUpper().Equals("KEYNUMBER") || pname.ToUpper().Equals("REVISION")))
            {
                DataFileName = string.Empty;
                ImportFileName = string.Empty;
            }
        }
        /// <summary>
        /// the path for the main folder for the project files
        /// </summary>
        public override string OutputFolder { get => base.ProjectFolder; }



        /// <summary>
        /// the name of the process licensor assigned to the project
        /// </summary>
        public override string ProcessLicensor { get => PropValS("ProcessLicensor"); set => Notify(PropValSet("ProcessLicensor", value.Trim())); }

    
        /// <summary>
        /// the ProjectType of the project
        /// uopProjMDSpout or uopProjMDDraw
        /// </summary>
        public override uppProjectTypes ProjectType
        {
            get => base.ProjectType;

            set
            {
                if (value == uppProjectTypes.MDDraw || value == uppProjectTypes.MDSpout)
                {
                    bool doAgain = true;
                    do
                    {
                        doAgain = false;
                        base.ProjectType = value;
                        if (base.PropertyCount() <= 0)
                        {
                            InitializeProperties();
                            doAgain = true;
                        }
                        if (!doAgain)
                        {
                            PropSetAttributes("ConvergenceLimit", bHidden: ProjectType == uppProjectTypes.MDDraw);
                            PropSetAttributes("SpacingMethod", bHidden: ProjectType == uppProjectTypes.MDDraw);
                            PropSetAttributes("ErrorLimit", bHidden: ProjectType == uppProjectTypes.MDDraw);
                            PropSetAttributes("SparePercentage", bHidden: ProjectType == uppProjectTypes.MDSpout);
                            PropSetAttributes("ClipSparePercentage", bHidden: ProjectType == uppProjectTypes.MDSpout);
                        }
                    } while (doAgain);
                }
            }
        }

        /// <summary>
        ///returns the collection of hardware materials assigned to all the ranges in the project
        /// </summary>
        public uopMaterials RangeBoltingMaterials
        {
            get
            {
                uopMaterials _rVal = new uopMaterials();
                colUOPTrayRanges Ranges = TrayRanges;
                mdTrayRange aRange = null;

                for (int i = 1; i <= Ranges.Count; i++)
                {
                    aRange = (mdTrayRange)Ranges.Item(i);
                    _rVal.Add(aRange.HardwareMaterial);
                }
                return _rVal;
            }
        }

        //        '#1the filename to write the project data to
        //'#2the name of the client app calling this function
        //'#3the  version of the calling app
        //'#4the minor version of the calling app

        //'^used by client applications to save a project's property values to a text file
        //'~the resulting file can be read in by executing the ReadFromFile method of this class.
        //'~the file is written like an INI file so data can be extracted using Win32 API calls designed for reading INI files.
        public override void SaveToFile(string FileSpec, string aAppName, string aAppVersion, string aFinalDestination = null)
        {

            string oldfile = string.Empty;

            try
            {


                eventSaving?.Invoke(true);

                if (string.IsNullOrWhiteSpace(FileSpec)) throw new Exception("[mdProject.SaveToFile] - null file path detected");



                uopPropertyArray saveprops = FileProperties(aAppName, aAppVersion, FileSpec);

                if (File.Exists(FileSpec))
                {
                    oldfile = FileSpec + ".bak";
                    if (File.Exists(oldfile)) File.Delete(oldfile);
                    File.Copy(FileSpec, oldfile);
                    if (!File.Exists(oldfile)) oldfile = string.Empty;
                    File.Delete(FileSpec);
                }

                LastSaveDate = DateTime.Now.ToString();
                DataFileName = FileSpec;

                if (!string.IsNullOrWhiteSpace(aFinalDestination))
                {
                    DataFileName = aFinalDestination;
                    saveprops.SetValue("Project", "DataFileName", DataFileName);
                    saveprops.SetValue("Project", "ProjectFolder", Path.GetDirectoryName(aFinalDestination));


                }

                saveprops.WriteToINIFile(FileSpec, false);
                HasChanged = false;


                //Props.WriteToINIFile(FileSpec, false);
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                if (oldfile !=  string.Empty)
                {
                    if (File.Exists(oldfile))
                    {
                        File.Copy(oldfile, FileSpec);
                        File.Delete(oldfile);
                    }
                }
                eventSaving?.Invoke(false);

            }



        }

        /// <summary>
        /// used by client applications to read a projects property values from a text file
        /// the passed filename should point to a file that was written by executing the
        /// SaveToFile method of this class.
        /// </summary>
        /// <param name="aFileSpec">the filename to read the project info from</param>
        /// <param name="AppName">the name of the client app calling this function</param>
        /// <param name="AppVersion">the version of the calling app</param>
        /// <param name="aWarnings">VB collection that///returns any warnings encountered when the file is read.</param>
        public override void ReadFromFile(string aFileSpec, string aAppName, string aAppVersion, out uopDocuments rWarnings)
        {
            string errStr = string.Empty;
            rWarnings = new uopDocuments();

            try
            {

                Reading = true;

                string Str = string.Empty;

                double aFileVersion = 0;
                double cnvg = 0;

                List<string> aHeadings = uopUtils.GetINIFileHeadingsList(aFileSpec, true);


                Reading = true;
                _Documents.Clear();

                ReadStatus($"Reading MD Project Data From '{aFileSpec}'");

                uopPropertyArray filedata = new uopPropertyArray() { Name = aFileSpec };


                filedata.ReadFromINIFile(aFileSpec);
                //global stuff

                //make sure this is a md project file
                Str = filedata.ValueS("APPINFO", "Application");
                if (string.Compare(Str, aAppName, true) != 0)
                    throw new Exception($"File '{aFileSpec}' Does Not Contain Does Not Contain {aAppName} Data");

                aAppName = Str;

                //check the application version
                Str = filedata.ValueS("APPINFO", "Version", out bool found);
                if (found) aAppVersion = Str;

                //check that this file md project  data
                Str = filedata.ValueS("PROJECT", "ProjectType", out found);
                if (Str ==  string.Empty || !found) Str = filedata.ValueS("project_level", "ProjectType", out found);
                Str = Str.ToUpper();
                if (Str != "MDSPOUT" && Str != "MD SPOUT PROJECT" && Str != "MD DRAW PROJECT")
                    throw new Exception($"'{aFileSpec}' Does Not Contain MD Project Data");

                ProjectType = Str.Contains("DRAW") ? uppProjectTypes.MDDraw : uppProjectTypes.MDSpout;


                //get the numeric version of the file
                double curentversion = Convert.ToDouble(App.Major + "." + App.Minor + App.Revision);
                double vers = mzUtils.VersionToDouble(aAppVersion, curentversion);

                aFileVersion = filedata.ValueD("Project", "DLL.Version", vers);
                bool oldversion = curentversion > aFileVersion;
                DLLVersion = aFileVersion;

                ReadStatus("Extracting Project Properties");

                base.ReadProperties(this, filedata, ref rWarnings, aFileVersion, "Project", bIgnoreNotFound: oldversion);


                if (ColumnSketchCount <= 0) PropValSet("ColumnSketchCount", 1);
                cnvg = filedata.ValueD("Project", "ConvergenceLimit", 0.00001d);
                if (cnvg <= 0) cnvg = 0.00001d;

                PropValSet("ConvergenceLimit", cnvg);

                ReadStatus("Extracting Project Notes");
                Notes = filedata.SubPropertiesStartingWith("Project", "Note");

                uopProperties props = CurrentProperties();

                if (filedata.TryGet("DRAWING NUMBERS", out uopProperties dwgnos))
                {
                    DrawingNumbers.CopyValues(dwgnos, false);
                }


                Customer.ReadProperties(this, filedata, ref rWarnings, aFileVersion, bIgnoreNotFound: oldversion);

                ReadStatus("Extracting Column Properties");

                if (_Column != null)
                {
                    _Column.eventColumnPropertyChange -= _Column_ColumnPropertyChange;
                    _Column.eventRangeCountChange -= _Column_RangeCountChange;

                }

                _Column = new uopColumn(this);

                _Column.ReadProperties(this, filedata, ref rWarnings, aFileVersion, bIgnoreNotFound: oldversion);
                _Column.eventColumnPropertyChange += _Column_ColumnPropertyChange;
                _Column.eventRangeCountChange += _Column_RangeCountChange;


                _Stages = new colMDStages();
                uopPropertyArray subdata = filedata.PropertiesStartingWith("STAGES");
                if (subdata.Count > 0)
                {
                    _Stages.ReadProperties(this, subdata, ref rWarnings, aFileVersion, bIgnoreNotFound: oldversion);
                }

                _Stages.SubPart(this);

                if (_Distributors != null)
                {
                    _Distributors.eventDistributorCountChange -= _Distributors_DistributorCountChange;
                    _Distributors.eventDistributorMemberChanged -= _Distributors_DistributorMemberChanged;
                }

                _Distributors = new colMDDistributors(null, this, true)
                {
                    MaintainIndices = true
                };
                subdata = filedata.PropertiesStartingWith("DISTRIBUTORS");
                if (subdata.Count > 0)
                {
                    _Distributors.ReadProperties(this, subdata, ref rWarnings, aFileVersion, bIgnoreNotFound: oldversion);
                }

                _Distributors.eventDistributorCountChange += _Distributors_DistributorCountChange;
                _Distributors.eventDistributorMemberChanged += _Distributors_DistributorMemberChanged;
                _Distributors.SubPart(this);

                _ChimneyTrays = new colMDChimneyTrays();
                subdata = filedata.PropertiesStartingWith("CHIMNEYTRAYS");
                if (subdata.Count > 0)
                {
                    _ChimneyTrays.ReadProperties(this, subdata, ref rWarnings, aFileVersion, bIgnoreNotFound: oldversion);
                }

                _ChimneyTrays.eventChimneyTrayCountChange += _ChimneyTrays_ChimneyTrayCountChange;
                _ChimneyTrays.eventChimneyTrayMemberChanged += _ChimneyTrays_ChimneyTrayMemberChanged;
                _ChimneyTrays.SubPart(this);
                _ChimneyTrays.MaintainIndices = true;



                ReadStatus("Sorting Trays");
                TrayRanges.SortByRingStart();
                ReadStatus("");
                DLLVersion = mzUtils.VarToDouble($"{App.Major}.{App.Minor}{App.Revision}");
                DataFileName = aFileSpec;
                DisplayUnits = (uppUnitFamilies)filedata.ValueI("Project", "DisplayUnits", (int)uppUnitFamilies.English);
                DrawingUnits = (uppUnitFamilies)filedata.ValueI("Project", "DrawingUnits", (int)uppUnitFamilies.English);
                LastSaveDate = filedata.ValueS("Project", "LastSaveDate", "");
                PropValSet("DataFilename", aFileSpec, bSuppressEvnts: true);


                Reading = false;

                HasChanged = false;

                if (aFileVersion < 2.32)
                    rWarnings.AddWarning(this, "Version 2.5 Warning", "Files Saved In WinTray Versions Prior To V2.5.0 May Vary Slightly When Viewed Due To Changes To WinTray Modifications To The Basic Weir Length Calculations and Various Default Clearances");
                HasChanged = false;

            }
            catch (Exception exception)
            {
                LoggerManager log = new LoggerManager();
                log.LogError(exception.Message);
                errStr = exception.Message;

                throw new Exception($"[mdProject.ReadFromFile] '{aFileSpec}' ERR: {errStr}");
            }
            finally
            {
                Reading = false;
                if (_LogFile != null)
                {
                    _LogFile.Close();
                    _LogFile = null;
                }
                if (!string.IsNullOrWhiteSpace(errStr)) rWarnings.AddWarning(this, "mdProject.ReadFromFile Error", errStr);

            }
        }

        /// <summary>
        /// Get Unique Ranges
        /// ^returns a collection of md tray range collections
        /// ~all the members in each collection are unique by shell ID and flow devices
        /// </summary>
        /// <returns></returns>
        public List<List<uopTrayRange>> GetUniqueRanges()
        {

            colUOPTrayRanges mRanges = TrayRanges;
            mdTrayRange mRange;
            List<uopTrayRange> mdRanges;
            List<uopTrayRange> matchCol = null;
            mdTrayRange matchRange = null;
            List<List<uopTrayRange>> _rVal = new List<List<uopTrayRange>>();
            if (mRanges.Count <= 0) return _rVal;


            mdRanges = new List<uopTrayRange>();
            for (int i = 1; i <= mRanges.Count; i++)
            {
                mRange = (mdTrayRange)mRanges.Item(i);
                if (i == 1)
                {
                    mdRanges.Add(mRange);
                    _rVal.Add(mdRanges);
                }
                else
                {
                    GetMatchCol(mRange, ref mdRanges, ref matchCol, ref matchRange, _rVal);
                    if (mdRanges == null)
                    {
                        mdRanges = new List<uopTrayRange>();
                        _rVal.Add(mdRanges);
                    }
                    mdRanges.Add(mRange);
                }
            }

            return _rVal;
        }

        /// <summary>
        /// Get Match Col
        /// </summary>
        /// <param name="mRange"></param>
        /// <param name="mdRanges"></param>
        /// <param name="matchCol"></param>
        /// <param name="matchRange"></param>
        /// <param name="getUniqueRanges"></param>
        private static void GetMatchCol(mdTrayRange mRange, ref List<uopTrayRange> mdRanges, ref List<uopTrayRange> matchCol, ref mdTrayRange matchRange, List<List<uopTrayRange>> getUniqueRanges)
        {
            mdRanges = null;
            for (int j = 0; j < getUniqueRanges.Count; j++)
            {
                matchCol = getUniqueRanges[j];
                matchRange = (mdTrayRange)matchCol[0];

                if (Math.Round(mRange.ShellID, 4) == Math.Round(matchRange.ShellID, 4))
                {
                    if (mRange.TrayAssembly.DesignOptions.HasAntiPenetrationPans == matchRange.TrayAssembly.DesignOptions.HasAntiPenetrationPans)
                    {
                        if (mRange.TrayAssembly.DesignOptions.HasFlowEnhancement == matchRange.TrayAssembly.DesignOptions.HasFlowEnhancement)
                        {
                            mdRanges = matchCol;
                            break;
                        }
                    }
                }
            }
        }

        public override void ReadStatus(string Status, int aIndex = 1)
        {
            base.ReadStatus(Status, aIndex);

            if (!string.IsNullOrWhiteSpace(Status))
                if (_LogFile != null)
                {
                    StreamWriter streamWriter = new StreamWriter(_LogFile);
                    streamWriter.WriteLine($"STATUS({aIndex}): {Status}");
                    streamWriter.Dispose();
                }
        }
        /// <summary>
        /// excuted to deletes the stage data from the project
        /// </summary>
        public void RemoveStages() => _Stages = null;

        /// <summary>
        ///returns the list of available reports for the project
        /// </summary>
        public override uopDocuments Reports
        {
            get
            {
                uopDocuments _rVal = Documents().GetByDocumentType(uppDocumentTypes.Report, false);
                uopDocReport aRep;
                for (int i = 1; i <= _rVal.Count; i++)
                {
                    aRep = (uopDocReport)_rVal.Item(i);
                    aRep.ProjectName = Name;
                    aRep.Project = this;
                }
                return _rVal;
            }
        }
        /// <summary>
        /// forces the all trays to recalculate its components
        /// </summary>
        public void ResetComponents()
        {
            try
            {
                ReadStatus("Resetting Project " + Name + " Sub-Components");
                _Documents.Invalid = true;
                mdTrayRange aTray = null;
                colUOPTrayRanges Trays = TrayRanges;
                for (int i = 1; i <= Trays.Count; i++)
                {
                    aTray = (mdTrayRange)Trays.Item(i);
                    aTray.ResetComponents();
                }
            }
            catch (Exception exception)
            {
                LoggerManager log = new LoggerManager();
                log.LogError(exception.Message);
            }
        }
        /// <summary>
        /// true causes the trays to be listed with the highest ring numbers first
        /// </summary>
        public override bool ReverseSort { get => PropValB("ReverseSort"); set { uopProperty aProp = PropValSet("ReverseSort", value); if (aProp != null) { Notify(aProp); Column.TraySortOrder = value ? uppTraySortOrders.BottomToTop : uppTraySortOrders.TopToBottom; } } }

        /// <summary>
        /// the revision number assigned to the project// ToDo
        /// </summary>
        public override int Revision { get => PropValI("Revision"); set => Notify(PropValSet("Revision", Math.Abs(value))); }

        /// <summary>
        /// the collection of ring specs to apply to the columns in the project
        /// </summary>
        public override colUOPRingSpecs RingSpecs { get => _RingSpecs; set => _RingSpecs = value; }

        /// <summary>
        /// the UOP SAP system number assigned to the project
        /// </summary>
        public override string SAPNumber { get => PropValS("SAPNumber"); set => Notify(PropValSet("SAPNumber", value.Trim())); }



        public override string SelectName => uopUtils.ProjectSelectName(KeyNumber, Revision, ProjectType);

        public override string SelectedDrawing
        {
            get
            {
                string _rVal;
                if (_SelectedDrawing ==  string.Empty)
                {
                    mdTrayRange aRange = null;
                    aRange = (mdTrayRange)TrayRanges.SelectedRange;
                    if (aRange != null)
                    {
                        _rVal = aRange.PartPath().ToUpper() + ".DRAWINGS.TRAY VIEWS(PLAN VIEW)";
                        return _rVal;
                    }
                }
                return _SelectedDrawing;
            }
            set => _SelectedDrawing = value;
        }

        public override string SelectedCalculation
        {
            get
            {
                if (_SelectedCalc ==  string.Empty)
                {
                    mdTrayRange aRange = (mdTrayRange)TrayRanges.SelectedRange;

                    if (aRange != null)
                    {
                        _SelectedCalc = aRange.PartPath().ToUpper() + ".Warnings";
                        return "";
                    }
                }
                return _SelectedCalc;
            }
            set => _SelectedCalc = value;
        }
        /// <summary>
        /// the project tray range that is currently marked as selected
        /// </summary>
        public new mdTrayRange SelectedRange => (mdTrayRange)base.SelectedRange;



        public override bool SingleColumnProject => true;

        /// <summary>
        ///returns the filename or the import filename
        /// </summary>
        public override string SourceFileName => mzUtils.ThisOrThat(DataFileName, ImportFileName);

        /// <summary>
        /// controls how the optimimum downcomer spacing is calculated for all trays in the project
        /// </summary>
        public uppMDSpacingMethods SpacingMethod
        {
            get => (uppMDSpacingMethods)PropValI("SpacingMethod");
            set
            {
                if (value == uppMDSpacingMethods.NonWeighted || value == uppMDSpacingMethods.Weighted)
                    Notify(PropValSet("SpacingMethod", value));
            }
        }

        /// <summary>
        /// the percentage of extra hardware required when the trays are shipped
        /// ~default = 5
        /// </summary>
        public new double SparePercentage { get => PropValD("SparePercentage"); set { base.SparePercentage = value; Notify(PropValSet("SparePercentage", value)); } }
        public double ClipSparePercentage { get => PropValD("ClipSparePercentage"); set => Notify(PropValSet("ClipSparePercentage", value)); }


        /// <summary>
        /// the collection of stages defined for the column in the project
        /// </summary>
        public override colMDStages Stages
        {
            get
            {
                _Stages ??= new colMDStages();
                _Stages.SubPart(this);
                return _Stages;
            }
            set
            {
                _Stages = value;
                if (_Stages != null) _Stages.SubPart(this);
            }
        }


        /// <summary>
        ///returns a string that escribes the project
        /// </summary>
        public string Title
        {
            get
            {
                string t1 = Name;
                string t2 = Customer.Name;
                string t3 = Customer.Service;
                string t4 = t1;
                if (t4 !=  string.Empty)
                {
                    if (t2 !=  string.Empty) t4 = t4 + " - " + t2;
                }
                else
                {
                    t4 = t2;
                }
                if (t4 !=  string.Empty)
                {
                    if (t3 !=  string.Empty) t4 = t4 + " - " + t3;
                }
                else
                {
                    t4 = t3;
                }
                return t4;
            }
        }
        /// <summary>
        /// the total number of trays
        /// </summary>
        public override int TotalTrayCount => TrayRanges.TotalTrayCount;


        public List<mdTrayRange> GetMDTrays(uppMDDesigns aDesignType = uppMDDesigns.Undefined) => Column.GetMDTrays(aDesignType);

        /// <summary>
        /// ^returns all of the defined tray ranges in the column
        /// </summary>
        public override colUOPTrayRanges TrayRanges => Column.TrayRanges;

        public override uopSheetMetal FirstDeckMaterial
        {
            get
            {
                colUOPTrayRanges myRanges = TrayRanges;

                if (myRanges != null && myRanges.Count > 0)
                {
                    mdTrayRange rng = (mdTrayRange)myRanges.Item(1);
                    mdTrayAssembly assy = rng.TrayAssembly;
                    return assy.Deck.SheetMetal;
                }
                return new uopSheetMetal();

            }
        }

        public override uopSheetMetal FirstDowncomerMaterial
        {
            get
            {
                colUOPTrayRanges myRanges = TrayRanges;

                if (myRanges != null && myRanges.Count > 0)
                {
                    mdTrayRange rng = (mdTrayRange)myRanges.Item(1);
                    mdTrayAssembly assy = rng.TrayAssembly;
                    return assy.Downcomer().SheetMetal;
                }
                return new uopSheetMetal();
            }
        }

        public override void UpdatePartProperties()
        {
            //PropValSet("RangeCount", TrayRanges.Count, bSuppressEvnts: true);
            base.DescriptiveName = $"{ProjectTypeName }  { Name}";
        }

        public override void UpdatePartWeight() => base.Weight = 0;


        /// <summary>
        /// excuted prior to export of stages to update the stages mechanical properties with its associated
        /// </summary>
        public  List<mdStage> UpdatedStages()
        {
            List<mdStage> _rVal = new List<mdStage>();
            mdTrayRange aRange = null;
            colUOPTrayRanges mRanges = TrayRanges;
            for (int i = 1; i <= mRanges.Count; i++)
            {
                aRange = (mdTrayRange)mRanges.Item(i);
                _rVal.AddRange( aRange.UpdatedStages());
            }
            return _rVal;
        }
        /// <summary>
        /// the version of the application that last saved this project
        /// like "1.0.1"
        /// </summary>
        public override string Version { get => base.AppVersion; set => base.AppVersion = value; }

        /// <summary>
        /// all the warnings define for the project
        /// </summary>
        public override uopDocuments Warnings(bool bJustOne = false) => GenerateWarnings(null, bJustOne);

        public void InvalidateDocuments()
        {
            if (_Documents != null) _Documents.Invalid = true;
        }

        private void _ChimneyTrays_ChimneyTrayCountChange(uopProperty aProperty)
        {
            if (aProperty == null) return;
            Notify(aProperty);
        }

        private void _ChimneyTrays_ChimneyTrayMemberChanged(uopProperty aProperty)
        {
            if (aProperty == null) return;
            Notify(aProperty);
        }
        void _Column_ColumnPropertyChange(uopProperty aProperty)
        {
            Notify(aProperty);
        }
        void _Column_RangeCountChange(int NewCount)
        {
            Notify(uopProperty.Quick("RangeCount", NewCount, _Column.TrayRanges.Count, this));
            _Documents.Invalid = true;
        }
        void _Customer_CustomerPropertyChange(uopProperty aProperty)
        {
            Notify(aProperty);
        }
        private void _Distributors_DistributorCountChange(uopProperty aProperty)
        {
            if (aProperty == null) return;
            Notify(aProperty);
        }
        private void _Distributors_DistributorMemberChanged(uopProperty aProperty)
        {
            if (aProperty == null) return;
            Notify(aProperty);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Destroy();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }



        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Import MDH File
        ///  '^imports a md spout MDProject from the selected file name
        ///  '~included to support mdh files generated by mdTray hydraulics application
        /// </summary>
        public static void ImportMDHFile(mdProject aProject, ref uopDocuments aWarnings, out List<mdTrayRange> rRanges, bool bDontAddRagesToProject)
        {
            aWarnings ??= new uopDocuments();
            rRanges = new List<mdTrayRange>();


            if (aProject == null) return;
            string mdhpath = aProject.ImportFileName;
            if (string.IsNullOrWhiteSpace(mdhpath)) return;
            if (!File.Exists(mdhpath))
            {
                aWarnings.AddWarning(aProject, "Stage Import Warning", $"File Not Found '{mdhpath}'", uppWarningTypes.General, "Stage Error Detected");
                return;
            }

            try
            {
                aProject.SuppressEvents = true;


                uopColumn column = aProject.Column;
                colMDStages stages = new colMDStages();

                List<string> lines = new List<string>();
                int j;

                string fType = string.Empty;

                bool sortWas = aProject.ReverseSort;
                List<object> errorStrs;
                double manid = aProject.ManholeID;

                aProject.ReverseSort = false;
                // to force a recheck of tiled decks
                aProject.ManholeID = 0;

                // create a collection of 43 know property names in order that the appear
                // in the comma delimated stage definition strings
                // read the file into a collection
                using (StreamReader fileStream = new StreamReader(mdhpath))
                {
                    string line = string.Empty;
                    while ((line = fileStream.ReadLine()) != null)
                    {
                        line = line.Trim();
                        if (!string.IsNullOrEmpty(line))
                        {
                            lines.Add(line);
                            if (lines.Count == 2)
                            {
                                fType = line.Substring(0, 3);
                            }
                        }
                    }
                }

                // react to MDO or MDE files
                // this shouldn't happen if the passed file is MDH
                int linesPerStage = 8;
                if (fType.Length == 3)
                {
                    fType = fType.Substring(1, 1).ToUpper();
                    if (fType == "C")
                        linesPerStage = 12;
                    if (fType == "O")
                        linesPerStage = 9;
                }

                // divide the lines into groups of linesPerStage
                // Status2 ="Extracting Project Stage Information"
                aProject.ReadStatus("Extracting Project Stage Information");
                // Status3 = string.Empty;
                for (int i = 0; i < (lines.Count / linesPerStage); i++)
                {
                    bool bGoodStage = true;
                    j = i * linesPerStage;
                    // just get the first 8 lines per group
                    string dat = $"{lines[j]} ,{lines[j + 1]},{lines[j + 2]},{lines[j + 3]},{lines[j + 4]},{lines[j + 5]},{lines[j + 6]},{lines[j + 7]}";

                    //if there aren't 43 values it is invalid stage definition
                    List<string> values = uopUtils.ParseDeliminatedString(dat, ',', true);
                    // validate data
                    if (values.Count != 43)
                    {
                        aWarnings.AddWarning(aProject, "Stage Import Warning", String.Format("Stage {0} Has Incomplete Data. 43 Data Fields Are Expected And Only {1} Were Found. Stage Was Ignored.", i + 1, values.Count), uppWarningTypes.General, "Stage Error Detected");
                        bGoodStage = false;
                    }

                    if (string.Compare(values[4], "US") != 0 | string.Compare(values[5], "US") != 0)
                    {
                        aWarnings.AddWarning(aProject, "Stage Import Warning", String.Format("Stage {0} Has Invalide Data. Fields 5 and 6 Must Be 'US' To Be Imported.  Stage Was Ignored.", i + 1), uppWarningTypes.General, "Stage Error Detected");
                        bGoodStage = false;
                    }

                    if (!bGoodStage) continue;


                    //'define the stages properties
                    mdStage aStage = new mdStage();
                    aStage.DefinePropertyValues(values);

                    aStage.StageNumber = i + 1;
                    //'see if this stage is mechanically valid
                    errorStrs = mdUtils.ValidateStage(aStage);

                    // bail on the stage if there are undefined\invalid mechanical props
                    bGoodStage = errorStrs.Count == 0;
                    if (!bGoodStage)
                    {
                        for (j = 0; j < errorStrs.Count; j++)
                        {
                            aWarnings.AddWarning(aProject, "Stage Import Warning", $"Invalid Stage Data Detected (Stage-{i + 1}). {errorStrs[j]} Stage Ignored.", uppWarningTypes.General, "Stage Error Detected");
                        }

                        continue;
                    }

                    // fill in the ring spec width if it was not passed
                    if (aStage.PropValD("RingWidth") <= 0)
                    {
                        aStage.PropValSet("RingWidth", aProject.RingSpecs.GetRingWidth(aStage.PropValD("ShellID", aMultiplier: 12), (uopProject)aProject), bSuppressEvnts: true);
                        aWarnings.AddWarning(aProject, "Stage Import Warning", $"Zero Ring Width (Stage-{i + 1}). {aStage.PropValD("RingWidth")}'' Assumed.", uppWarningTypes.General, "Stage Error Detected");
                    }

                    // make key numbers match first one found
                    //If (ky ==  string.Empty){ ky = aStage.PropValS("KeyNumber"); } else { aStage.PropValSet("KeyNumber", ky); }
                    stages.AddStage(aStage);
                }

                aProject.Stages = stages;
                if (stages.Count <= 0)
                {
                    aWarnings.AddWarning(aProject, "Stage Import Warning", "No Valid Stage Definitions Found");
                    return;
                }


                // define tray ranges based on the stages that are unique mechanically
                //Status2 = "Creating Tray Sections Based on Stage Data";
                //Status3 = string.Empty;
                aProject.ReadStatus("Creating Tray Sections Based on Stage Data");

                //define some MDProject properties based on the first stages data
                List<mdStage> uniquestages = new List<mdStage>();
                if (stages.Count > 0)
                {
                    mdStage aStage = stages.Item(1);
                    if (string.IsNullOrEmpty(aProject.Customer.Location))
                        aProject.Customer.Location = aStage.PropValS("InstallationLocation");
                    if (string.IsNullOrEmpty(aProject.Customer.Name))
                        aProject.Customer.Name = aStage.PropValS("CustomerName");
                    if (string.IsNullOrEmpty(aProject.Customer.Service))
                        aProject.Customer.Service = aStage.PropValS("CustomerService");

                    //get only the mechanically unique stages
                    uniquestages = stages.GetUniqueStages(true);
                }

                aProject.ReadStatus(string.Empty);
                aProject.ReadStatus($"Defining {uniquestages.Count} Tray Sections From {stages.Count} Defined Stages");

                // define a tray range for each stage
                j = aProject.TrayRanges.HighestUsedRing();
                if (sortWas)
                    j = j + (2 * uniquestages.Count) - 2;

                for (int i = 1; i <= uniquestages.Count; i++)
                {
                    mdStage aStage = uniquestages[i - 1];
                    mdTrayRange range = new mdTrayRange
                    {
                        Index = i,
                        RingStart = j + 1,
                        RingEnd = j + 2

                    };


                    if (!sortWas)
                        j += 2;
                    else
                        j -= 2;
                    range.DefineByStage(aStage, aWarnings);



                    rRanges.Add(range);

                    if (!bDontAddRagesToProject) column.TrayRanges.Add(range);
                }
                aProject.ReadStatus(string.Empty);
                aProject.ReadStatus("Assigning Stage Indices To New Sections");



                foreach (mdTrayRange range in rRanges)
                {

                    int stageValue = string.IsNullOrEmpty(range.StageList) ? 0 : Convert.ToInt32(range.StageList);
                    mdStage aStage = stages.GetByStageNumber(stageValue);
                    if (aStage != null)
                    {
                        foreach (mdStage bStage in stages)
                        {
                            if (aStage != bStage)
                            {
                                if (aStage.IsEqual(bStage, true))
                                    range.AssociateToStage(bStage);
                            }
                        }

                    }
                }

                // save the column to the aProject and intitialize the spouts on the first range (for faster display)
                aProject.ReadStatus($"{rRanges.Count} Trays Extracted - Initializing Tray Assemblies");

                aProject.ReverseSort = sortWas;


                //generate the downcomers and spouts for each range



                foreach (mdTrayRange range in rRanges)
                {

                    mdTrayAssembly assy = range.TrayAssembly;
                    aProject.ReadStatus($"Generating Section {range.Name(true)} Components ({j} of {rRanges.Count})");
                    range.InitializeComponents();

                    mdDowncomer dcomer = assy.Downcomer();
                    aProject.ReadStatus($"Generating Section {rRanges.IndexOf(range) + 1} of {rRanges.Count} {range.Name(true)} Spouts");
                    dcomer.PropValSet("TotalWeir", dcomer.WeirLength(uppSides.Undefined), bSuppressEvnts: true);
                    dcomer.PropValSet("TotalArea", dcomer.IdealSpoutArea(assy), bSuppressEvnts: true);
                    dcomer.PropValSet("SpoutArea", dcomer.TotalSpoutArea(assy), bSuppressEvnts: true);
                    dcomer.PropValSet("Thickness", dcomer.Thickness, bSuppressEvnts: true);

                    //assy.DesignOptions.PropValSet("CDP", assy.DesignOptions.CDP, bSuppressEvnts: true);
                    //assy.DesignOptions.PropValSet("FEDorAPPHeight", assy.DesignOptions.FEDorAPPHeight, bSuppressEvnts: true);

                    assy.Deck.PropValSet("MaxVLError", assy.DeckPanels.MaxVLError(range.TrayAssembly), bSuppressEvnts: true);
                    //assy.Deck.PropValSet("FP", assy.Deck.Fp, bSuppressEvnts: true);
                }
                aProject.SuppressEvents = false;
                // to force a recheck of tiled decks
                aProject.ManholeID = manid;

                //string isMeticSpotingNeeded = System.Configuration.ConfigurationManager.AppSettings.Get(IS_METRIC_SPOUTING_NEEDED_BYDEFAULT);
                //if (!string.IsNullOrEmpty(isMeticSpotingNeeded) && Boolean.TryParse(isMeticSpotingNeeded, out bool mSpouting) && mSpouting)
                //if (mzUtils.VarToBoolean(System.Configuration.ConfigurationManager.AppSettings.Get("IsMetricSpoutingNeededByDefault")))
                //{
                aProject.MetricSpouting = true;
            }
            catch (Exception e)
            {
                aWarnings.AddWarning(aProject, "Stage Import Exception", e.Message, uppWarningTypes.General, "Stage Error Detected");
            }
            finally
            {
                aProject.SuppressEvents = false;
                aProject.ReadStatus(string.Empty);
            }

        }
    }
}