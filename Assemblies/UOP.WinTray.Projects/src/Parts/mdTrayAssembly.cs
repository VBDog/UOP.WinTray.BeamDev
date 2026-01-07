using UOP.DXFGraphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Documents;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Materials;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Tables;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;
using Newtonsoft.Json.Linq;
using UOP.WinTray.Projects.Events;
using UOP.WinTray.Projects.Interfaces;
using System.Security.Policy;
using ClosedXML.Graphics;

namespace UOP.WinTray.Projects.Parts
{
    public class mdTrayAssembly : uopTrayAssembly, IDisposable
    {

        public override uppPartTypes BasePartType => uppPartTypes.TrayAssembly;
        #region Variables

        //the sub objects of the assembly
        private mdDeck _Deck = null;
        internal colMDDowncomers _Downcomers = null;
        private mdDowncomer _Downcomer = null;
        internal colMDSpoutGroups _SpoutGroups = null;
        internal colMDConstraints _Constraints = null;
        private mdDesignOptions _DesignOptions = null;
        private colMDDeckPanels _DeckPanels = null;

        private mdDeckSections _DeckSections = null;
        private mdStartupSpouts _StartupSpouts = null;

        private colUOPParts _SpliceAngles = null;
        private colUOPParts _APPans = null;
        //private bool _Invalidating = false;

        private bool disposedValue;

        #endregion Variables

        #region Events
        //@raised when any sub object properties change

        public delegate void MDAssemblyPropertyChange(uopProperty aProperty);
        public event MDAssemblyPropertyChange PropertyChangeEvent;

        public delegate void MDAssemblyPartsInvalidatedHandler();
        public event MDAssemblyPartsInvalidatedHandler eventMDAssemblyPartsInvalidated;

        public delegate void GenerationEventHandler(uppTrayGenerationEventTypes aType, bool Begin, string aHandle);
        public event GenerationEventHandler eventGenerationEvent;

        #endregion

        #region Constructors

        public mdTrayAssembly() : base(uppProjectTypes.MDSpout, "", "") => InitializeProperties();


        internal mdTrayAssembly(mdTrayAssembly aPartToCopy, uopPart aParent = null) : base(uppProjectTypes.MDSpout, "", "")
        {
            InitializeProperties(aPartToCopy);
            SubPart(aParent);
        }

        /// <summary>
        /// ^creates the property collection for the object
        /// </summary>
        private void InitializeProperties(mdTrayAssembly aPartToCopy = null)
        {

            _SlotZones = new mdSlotZones { Invalid = true };

            if (ActiveProps.Count <= 0)
            {
                AddProperty("OverrideSpacing", 0, uppUnitTypes.SmallLength, aNullVal: 0);
                AddProperty("StartupConfiguration", (int)uppStartupSpoutConfigurations.TwoByTwo, uppUnitTypes.Undefined, false, "", "", "-1=Undefined,0=None,1=2x2,2=2x4,3=4x4");
                AddProperty("Spacing", 0, uppUnitTypes.SmallLength);
                AddProperty("OverrideStartupLength", 0, uppUnitTypes.SmallLength, aNullVal: 0);
                AddProperty("TheoreticalSpoutArea", 0, uppUnitTypes.SmallArea);

            }


            if (aPartToCopy != null)
            {

                base.Copy(aPartToCopy);
                _SlotZones = aPartToCopy.SlotZones.Clone();
                _BPSites = uopVectors.CloneCopy(aPartToCopy._BPSites);


                TraySortOrder = aPartToCopy.TraySortOrder;
            }

            // 'DATA OBJECTS
            Deck = (aPartToCopy == null) ? new mdDeck() : new mdDeck(aPartToCopy._Deck, this);

            DesignOptions = (aPartToCopy == null) ? new mdDesignOptions() : new mdDesignOptions(aPartToCopy._DesignOptions, this);

            //'VIRTUAL PARTS

            _StartupSpouts = (aPartToCopy == null) ? new mdStartupSpouts() : new mdStartupSpouts(aPartToCopy._StartupSpouts);
            _SpoutGroups = (aPartToCopy == null) ? new colMDSpoutGroups() : new colMDSpoutGroups(aPartToCopy._SpoutGroups, this);
            _DeckPanels = (aPartToCopy == null) ? new colMDDeckPanels() : new colMDDeckPanels(aPartToCopy._DeckPanels, this);
            _DeckSplices = (aPartToCopy == null) ? new uopDeckSplices(this) : new uopDeckSplices(this, aPartToCopy._DeckSplices);

            //'PHYSICAL PARTS
            _Downcomers = (aPartToCopy == null) ? new colMDDowncomers() : new colMDDowncomers(aPartToCopy._Downcomers, this);
            _DeckSections = (aPartToCopy == null) ? new mdDeckSections() : new mdDeckSections(aPartToCopy._DeckSections) { TrayAssembly = this};
            _APPans = (aPartToCopy == null) ? new colUOPParts() : new colUOPParts(aPartToCopy._APPans, aParentPart: this);
            _SpliceAngles = (aPartToCopy == null) ? new colUOPParts() : new colUOPParts(aPartToCopy._SpliceAngles, aParentPart: this);
            _Constraints = (aPartToCopy == null) ? new colMDConstraints() : new colMDConstraints(aPartToCopy._Constraints, this);
            _Beam = (aPartToCopy == null) ? new mdBeam() : new mdBeam(aPartToCopy._Beam, this);


            _DeckSplices.eventDeckSplicesInvalidated += _DeckSplices_DeckSplicesInvalidated;

            //_Deck.eventMDDeckInvalidated += _Deck_MDDeckInvalidated;


            _Downcomers.eventDowncomerMemberChanged += _Downcomers_DowncomerMemberChanged;
            _Downcomers.eventDowncomersInvalidated += _Downcomers_DowncomersInvalidated;
            _Downcomers.eventSpacingChanged += _Downcomers_SpacingChanged;
            _Downcomers.eventSpacingOptimizationBegin += _Downcomers_SpacingOptimizationBegin;
            _Downcomers.eventSpacingOptimizationEnd += _Downcomers_SpacingOptimizationEnd;

            //_Downcomer.eventPropertyChange += SubPartPropertyChangeEventHandler;

            // _SpoutGroups.eventSpoutGroupMemberChanged += _SpoutGroups_SpoutGroupMemberChanged;
            _SpoutGroups.eventSpoutGroupsGenerationEvent += _SpoutGroups_SpoutGroupsGenerationEvent;
            _SpoutGroups.eventSpoutGroupsInvalidated += _SpoutGroups_SpoutGroupsInvalidated;

            _Constraints.eventConstraintMemberChanged += _Constraints_ConstraintMemberChanged;
            _Constraints.eventConstraintsInvalidated += _Constraints_ConstraintsInvalidated;


            _DeckPanels.eventMDPanelMemberChanged += _DeckPanels_MDPanelMemberChanged;



            _DeckSections.eventDeckSectionsInvalidated += _DeckSections_DeckSectionsInvalidated;

            _Beam.eventMDBeamPropertyChange += SubPartPropertyChangeEventHandler;
            _Beam.SubPart(this);
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// the matrix that is the source of the target spout area for each spout group
        /// </summary>
        public mdSpoutAreaMatrices SpoutAreaMatrices { get; private set; }
        
        /// <summary>
        /// the height of the angle bent in a joggled panel splice
        /// </summary>
        public override double JoggleAngleHeight => DesignOptions.JoggleAngle;

        /// <summary>
        /// the max bolt spacing used to layout the bolts in a joggled deck splice
        /// </summary>
        public override double JoggleBoltSpacing => DesignOptions.JoggleBoltSpacing;

        public override uopPart RecieveingPanObj => null;

        public mdEndPlate EndPlate
        {
            get
            {
                mdEndPlate _rVal = null;
                colMDDowncomers aDCs = Downcomers;
                mdDowncomer aDC;
                int cnt = 0;
                string dclst = string.Empty;

                for (int i = 1; i <= aDCs.Count; i++)
                {
                    aDC = aDCs.Item(i);
                    if (!aDC.SpecialEndPlates)
                    {
                        if (_rVal == null) _rVal = aDC.Boxes[0].EndPlate(bTop: true);

                        cnt += aDC.OccuranceFactor * 2;
                        mzUtils.ListAdd(ref dclst, aDC.PartNumber);
                    }
                }
                cnt *= TrayCount;
                if (_rVal != null)
                {
                    _rVal.SubPart(this);
                    _rVal.TotalQuantity = cnt;
                    _rVal.ParentList = dclst;
                }
                return _rVal;
            }
        }

        private uopVectors _BPSites;
        internal uopVectors BPSites
        {
            get
            {
                if (_BPSites == null || _BPSites.Invalid || _BPSites.Count == 0)
                    _BPSites = DowncomerData.BPSites(this, _DeckSplices);
                else
                    DowncomerData.SuppressBPSites(_BPSites, _DeckSplices);

                _BPSites.Invalid = false;
                return _BPSites;

            }
            set => _BPSites = value;

        }

        public override int ManwayCount
        {
            get
            {
                if (ProjectType == uppProjectTypes.MDSpout) return Deck.ManwayCount;

                List<mdDeckSection> mansecs = UniqueDeckSections().FindAll(x => x.IsManway);
                int _rVal = 0;
                foreach (mdDeckSection item in mansecs)
                {
                    _rVal += item.Instances.Count + 1;
                }

                if (SpliceStyle == uppSpliceStyles.Tabs) _rVal /= 2;
                return _rVal;
            }
        }

        public uopPart ManwayFastener { get { if (!DesignOptions.UseManwayClips) { return ManwayClamp; } else { return ManwayClip; } } }

        public bool MetricSpouting => PropValB("MetricSpouting", uppPartTypes.Project);

        /// <summary>
        /// the height of an AP Pan for this assembly
        /// </summary>
        public double APPanHeight =>  Downcomer().DeckClearance - DesignOptions.FEDorAPPHeight;

        /// <summary>
        /// all the AP pans defined for the downcomers in the tray
        /// </summary>
        public colUOPParts APPans => GenerateAPPans();

        /// <summary>
        /// the percentage of the total panel area that is slotted
        /// </summary>
        public double ActualSlottingPercentage
        {
            get
            {
                double totArea = FunctionalActiveArea;
                return (totArea > 0) ? Deck.SlotArea * TotalSlotCount / totArea * 100 : 0;

            }
        }


        /// <summary>
        /// the actual spout area to theoretical spout area ratio for the tray
        /// </summary>
        public double AreaRatio
        {
            get
            {
                double ideal = TheoreticalSpoutArea;
                double actual = SpoutGroups.TotSpoutArea;
                if (ideal != 0) return actual / ideal;
                return (ideal == 0 & actual == 0) ? 1 : 0;

            }
        }



        /// <summary>
        /// the height of the baffle for the assembly
        /// </summary>
        public double BaffleHeight => DesignOptions.CDP - WeirHeight;

        public override bool DoubleNuts { get => DesignOptions.DoubleNuts; }

        public mdSlot FlowSlot => new mdSlot(Deck.SlotType);

        /// <summary>
        /// the factor applied when computing the mechanical area for areas
        /// ^of the tray that are lapped deck sections (splices)
        /// </summary>
        public double SpliceAreaFactor
        {
            get
            {
                uopObject aVendr = TrayVendor;
                double aVal = (aVendr != null) ? aVendr.Properties.ValueD("SpliceAreaFactor", -1) : -1;
                if (aVal < 0) aVal = 1;
                return mzUtils.LimitedValue(aVal, 0.5, 1.0);
            }
        }

        public uopObject TrayVendor
        {
            get
            {
                string tname = PropValS("TrayVendor", uppPartTypes.Project);
                //return (tname !=  string.Empty) ? uopGlobals.goTrayVendors.Member(tname) : null ;


                return null;
            }
        }

/// <summary>
        /// indicates which type of tray configuration the assembly is in
        /// ~constant = uopMultipleDowncomer
        /// </summary>
        public override uppTrayConfigurations Configuration => uppTrayConfigurations.MultipleDowncomer;

        /// <summary>
        /// the collection of defined spout group constraints for the tray assembly
        /// </summary>
        public colMDConstraints Constraints
        {
            get
            {
                if (_Constraints == null) GenerateSpoutGroups();
                _Constraints.SubPart(this);
                return _Constraints;
            }
            set
            {
                if (value != null) { _Constraints = value; } else { _Constraints.Clear(); };
                _Constraints.eventConstraintMemberChanged += _Constraints_ConstraintMemberChanged;
                _Constraints.eventConstraintsInvalidated += _Constraints_ConstraintsInvalidated;
                _Constraints.SubPart(this);

            }
        }

        /// <summary>
        /// the percent deviation that a downcomer or deck panels spout area can deviate from it's ideal without being considered out of spec.
        /// ~this is the number applied to the calculation of the downcomers center to center dimensions.
        /// </summary>
        public double ConvergenceLimit
        {
            get
            {
                mdProject mYProj = GetMDProject();
                return mYProj != null ? mYProj.ConvergenceLimit : 0.00001;
            }
        }
        
        /// <summary>
        /// controls how downcomer box and weir lengths are rounded
        /// </summary>
        ///<remarks> the default is the nearest 1/16th of an inch</remarks>
        public uppMDRoundToLimits DowncomerRoundToLimit
        {
            get
            {
                mdProject mYProj = GetMDProject();
                return (mYProj != null) ? mYProj.DowncomerRoundToLimit : uppMDRoundToLimits.Sixteenth;

            }

        }
        
        /// <summary>
        /// the basic cross braces installed in the tray
        /// </summary>
        public mdCrossBrace CrossBrace
        {
            get
            {
                List<mdDowncomer> DCs = Downcomers.GetByVirtual(aVirtualValue: false);

                if (DCs.Count <= 1 || !DesignOptions.CrossBraces) return null;


                mdDowncomer aDC;
                mdDowncomer bDC;
                double xl;
                double xr;

                mdCrossBrace _rVal = new mdCrossBrace()
                {
                    SheetMetalStructure = _Downcomer.SheetMetalStructure,
                    Quantity = 2,
                    SparePercentage = 2
                };


                if (OddDowncomers)
                {
                    aDC = DCs[DCs.Count - 2];
                    bDC = DCs[0];

                    xl = -aDC.X;
                    xr = bDC.X;
                }
                else
                {
                    aDC = DCs[DCs.Count - 1];
                    bDC = DCs[0];
                    xl = -aDC.X;
                    xr = bDC.X;
                }
                _rVal.Y = aDC.X;
                _rVal.Length = Math.Abs(xr - xl) + 3;
                _rVal.X = xl + Math.Abs(xr - xl) / 2;
                _rVal.Z = -(aDC.InsideHeight + aDC.Thickness - aDC.DeckThickness - aDC.How);

                _rVal.SubPart(this);

                return _rVal;
            }

        }

        /// <summary>
        /// the collection of all the sections of all the deck panels
        /// </summary>
        public mdDeckSections DeckSections => GenerateDeckSections();

        /// <summary>
        /// the deck object of tray assembly
        /// ~this object holds variables input by users that control the general layout of the
        /// ~components in the tray assembly.
        /// ~the material property controls the material of the deck panels.
        /// </summary>
        public mdDeck Deck
        {
            get { _Deck?.SubPart(this, null, true); return _Deck; }

            internal set
            {
                if (_Deck != null)
                {
                    _Deck.PropertyChangeEvent -= SubPartPropertyChangeEventHandler;
                    _Deck.EventMaterialChange += _Deck_MaterialChange;
                }
                _Deck = value;
                if (value == null) return;

                _Deck.PropertyChangeEvent += SubPartPropertyChangeEventHandler;
                _Deck.EventMaterialChange += _Deck_MaterialChange;

                _Deck.SubPart(this);
            }
        }
        
        /// <summary>
        /// the collection of deck panels required for the assemby of the tray
        /// ~contains the right side panels only. dynamically generated on each request
        /// </summary>
        public  colMDDeckPanels DeckPanels { get => _DeckPanels.Invalid ? GenerateDeckPanels() : _DeckPanels;   }

        public override uopParts DeckPanelsObj => DeckPanels;
        public override uopParts DeckPanelParts(uopTrayAssembly aAssy = null) => DeckPanels;


        private uopDeckSplices _DeckSplices;
        /// <summary>
        /// the splices used to create the deck panel sections
        /// </summary>
        public uopDeckSplices DeckSplices
        {
            get => ProjectType == uppProjectTypes.MDDraw ? GenerateSplices() : new uopDeckSplices(this);
            set { if (value == null) return; bool bRaiseIt = !_DeckSplices.IsEqual(value, this); SetDeckSplices(value, bCloneIt: true); if (bRaiseIt) { GenerateDeckSections(bRegen: true); Notify(uopProperty.Quick("SpliceLocations", "New Splices", "Old Splices", null)); } }

        }

        /// <summary>
        /// the design options object of tray assembly
        /// ~this object holds variables input by users that control the general layout of the
        /// ~components in the tray assembly.
        /// </summary>
        public mdDesignOptions DesignOptions
        {
            get { _DesignOptions.SubPart(this, null, true); return _DesignOptions; }
            set
            {
                if (_DesignOptions != null)
                {
                    _DesignOptions.PropertyChangeEvent -= SubPartPropertyChangeEventHandler;

                }
                _DesignOptions = value;
                if (value == null) return;

                _DesignOptions.PropertyChangeEvent += SubPartPropertyChangeEventHandler;

                _DesignOptions.SubPart(this);
            }

        }
        /// <summary>
        /// the weir height of the downcomer
        /// </summary>
        public double WeirHeight => Downcomer().How;

        /// <summary>
        /// returns the beam or the wall if the assembly is a non standard design family
        /// </summary>
        public DividerInfo Divider => DesignFamily.IsBeamDesignFamily() ? new DividerInfo(Beam) : null; // DesignFamily.IsDividedWallDesignFamily()? new DividerInfo(Wall);


        private mdBeam _Beam;
        /// <summary>
        /// the tray suuport beam used if the deisgn family requires a beam
        /// </summary>

        public mdBeam Beam
        {
            get
            {
                _Beam?.SubPart(this);
                return _Beam;

            }
            internal set
            {
                if (_Beam != null)
                {
                    _Beam.eventMDBeamPropertyChange -= SubPartPropertyChangeEventHandler;
                }

                _Beam = value;
                if (value == null) return;
                _Beam.eventMDBeamPropertyChange += SubPartPropertyChangeEventHandler;
                _Beam.SubPart(this);
            }

        }

       
        /// <summary>
        /// returns true if the tray below requires a differnent arrangement of the deck sections to align the manways
        /// </summary>
        public override bool HasAlternateDeckParts
        {
            get =>  IsStandardDesign ? RingRange.RingCount > 1 && Downcomer().Count > 1 && OddDowncomers && DeckSplices.ManwayCount(false) > 0 : false;
   
        }
        

        /// <summary>
        /// the distance between downcomer centers
        /// </summary>
        public double DowncomerSpacing
        {
            get
            {
                if (_Downcomers == null) return 0;
                double _rVal = _Downcomers.PropValD("OverrideSpacing");
                return _rVal == 0 ? _Downcomers.PropValD("OptimumSpacing") : _rVal;

            }
        }
        
        /// <summary>
        ///  the offset from center of the downcomer centers at the current optimized space
        /// </summary>
        /// <remarks>only applicable in the divided wall design family</remarks>

        public double DowncomerOffset => _Downcomers == null ? 0 : !DesignFamily.IsDividedWallDesignFamily() ? 0 : _Downcomers.PropValD("Offset");

        /// <summary>
        /// returns the solution set of the downcomer optimation at the current space
        /// </summary>
        public mdSpacingData SpacingData=> Downcomers.SpacingData; 

        /// <summary>
        /// a collection of downcomer information based n the properties of the main theoretical downcomer with memebrs at the current downcomer spacing
        /// </summary>
        public DowncomerDataSet DowncomerData => Downcomers.CurrentDataSet(this);

        /// <summary>
        /// the collection of the downcomers generated based n the properties of the main theoretical downcomer  
        /// </summary>
        public colMDDowncomers Downcomers
        {
            get => GenerateDowncomers();
            set
            {
                //^the collection of downcomers defined for the tray assembly
                if (_Downcomers != null)
                {
                    _Downcomers.eventDowncomerMemberChanged -= _Downcomers_DowncomerMemberChanged;
                    _Downcomers.eventDowncomersInvalidated -= _Downcomers_DowncomersInvalidated;
                    _Downcomers.eventSpacingChanged -= _Downcomers_SpacingChanged;
                    _Downcomers.eventSpacingOptimizationBegin -= _Downcomers_SpacingOptimizationBegin;
                    _Downcomers.eventSpacingOptimizationEnd -= _Downcomers_SpacingOptimizationEnd;
                }
                if (value != null)
                {
                    _Downcomers = value;
                    _Downcomers.eventDowncomerMemberChanged += _Downcomers_DowncomerMemberChanged;
                    _Downcomers.eventDowncomersInvalidated += _Downcomers_DowncomersInvalidated;
                    _Downcomers.eventSpacingChanged += _Downcomers_SpacingChanged;
                    _Downcomers.eventSpacingOptimizationBegin += _Downcomers_SpacingOptimizationBegin;
                    _Downcomers.eventSpacingOptimizationEnd += _Downcomers_SpacingOptimizationEnd;
                }
                else
                {
                    _Downcomers.Clear();
                    Invalidate(uppPartTypes.Downcomer);
                }
                _Downcomers.SubPart(this);

            }
        }

        /// <summary>
        /// the percentage that any spout error must be within to be considered acceptable
        /// </summary>
        public double ErrorLimit
        {
            get
            {
                mdProject Proj = GetMDProject();
                return (Proj != null) ? Proj.ErrorLimit : 2.5;

            }
        }
        
        /// <summary>
        /// the percentage that the current spout area deviates fron the ideal
        /// </summary>
        public double ErrorPercentage => AreaRatioDifferential(false) * 100;

        /// <summary>
        /// returns a string that represents the design family of the MD tray
        /// ~like "MD" or "ECMD"
        /// </summary>
        public override string FamilyName => uopEnums.Description(DesignFamily);

        /// <summary>
        /// the basic finger clip used to create the finger clips collection
        /// </summary>
        public mdFingerClip FingerClip => new mdFingerClip(this);
        /// <summary>
        /// returns the total functional area for the assembly. this is not the free bubbling area
        /// </summary>
        public double FunctionalActiveArea => Math.PI * Math.Pow(ShellRadius, 2) - FunctionalActiveAreas( out _, bIgnoreAngledEndPlates:false, bIgnoreBeams:false, bOmmitShellArea:true ).TotalArea() ; 

        /// <summary>
        /// the functional width of the deck panels which is the distance btween the downcomers with no gap
        /// </summary>
        /// <remarks >if there is only one downcomer it is the the deck radius minus 1/2 of the box width</remarks>   
        public double FunctionalPanelWidth
        {
            get
            {
                mdDowncomer aDC = Downcomer();
                colMDDowncomers DComers = Downcomers;
                return (aDC.Count > 1) ? DComers.Spacing - aDC.BoxWidth : DeckRadius - (aDC.BoxWidth * 0.5);

            }
        }
        /// <summary>
        /// returns True if the assembly has anti-penetration pans
        /// </summary>
        public override bool HasAntiPenetrationPans => DesignOptions.HasAntiPenetrationPans;

        /// <summary>
        /// returns True if the assembly has flow enhancement devices
        /// </summary>
        public bool HasFlowEnhancement => DesignOptions.HasFlowEnhancement;

        /// <summary>
        /// returns true if the assembly has a downcomer with triangular end plates
        /// </summary>
        public bool HasTriangularEndPlates => Downcomers.HasTriangularEndPlates;
        /// <summary>
        /// returns true if the assembly has a downcomer with foldover weirs
        /// </summary>
        public bool HasFoldovers => Downcomers.HasFoldovers;


        /// <summary>
        /// the large outer diameter fender washer used to hold deck panels down to the deck beams
        /// ~size changes with bolting property.
        /// ~created once per instance.
        /// ~material matches deck panel material but is always 10 ga.
        /// </summary>
        public hdwHoldDownWasher HoldDownWasher => new hdwHoldDownWasher(GetSheetMetal(true), this);

        /// <summary>
        /// the default path/heading in the project file that contains the parts persistent property data
        /// </summary>
        public override string INIPath => $"COLUMN({ColumnIndex}).RANGE({RangeIndex}).TRAYASSEMBLY";

        /// <summary>
        /// returns the target area for the total startup spout area for the assembly
        /// ~default = 1/15 of the theoretical spout area (Downcomer.Asp)
        /// </summary>
        public double IdealStartupArea => TheoreticalSpoutArea / 15;

     

        /// <summary>
        /// the oval clamp used to secure the manway panel to the support angles
        /// </summary>
        public uopManwayClamp ManwayClamp => new uopManwayClamp(this);

        /// <summary>
        /// the total number of manway clamps required to assembly a single tray.
        /// ~recalculated on each request.
        /// </summary>
        public int ManwayClampCount => ManwayFastenerCenters(true).Count;

        /// <summary>
        /// the slidding clip used to secure the manway panel to the support angles
        /// </summary>
        public uopManwayClip ManwayClip => new uopManwayClip(this);

        /// <summary>
        /// returns the maximum deviation of the current spout area distribution solution set
        /// ~this value is used to determine if the solution set is valid given the current constraints on the
        /// ~variable values.
        /// </summary>
        public double MaximumDistributionDeviation => SpoutAreaMatrices != null ? SpoutAreaMatrices.MaximimDeviation : 0;
              
        /// <summary>
        /// returns the total mechanical area for the assembly
        /// ~this is the total  free bubbling area minus the mechanically blocked area for the tray
        /// </summary>
        public double MechanicalActiveArea
        {
            get
            {
                return FreeBubblingAreas.MechanicalActiveArea(); //  DeckSections.MechanicalActiveArea(this, null, SpliceAreaFactor);

            }
        }

        /// <summary>
        /// the text used to dispay the part name in a tree
        /// </summary>
        public override string NodeName => TrayName(true);

        /// <summary>
        /// returns true if there is a odd number of total downcomers
        /// </summary>
        public bool OddDowncomers => Downcomer().OddDowncomers;

        /// <summary>
        /// the spacing to use other that the optimized value for the downcome spacing
        /// </summary>
        public double OverrideSpacing
        {
            get => PropValD("OverrideSpacing");

            set
            {
                _Downcomers.OverrideSpacing = Math.Abs(value);
                Notify(PropValSet("OverrideSpacing", Math.Abs(value)));

            }
        }


        /// <summary>
        /// the length to use for all startups which overrides the calculated value
        /// </summary>
        public double OverrideStartupLength { get => PropValD("OverrideStartupLength"); set => Notify(PropValSet("OverrideStartupLength", Math.Abs(value))); }

        /// <summary>
        /// the number of deck panels in the tray assembly
        /// </summary>
        public int PanelCount => Downcomer().Count + 1;

        /// <summary>
        /// the top level parts of the assembly
        /// </summary>
        public colUOPParts Parts => GenerateParts();

        //// <summary>
        ///the number of flow slots required for the entire tray based on the functional active area and the slot stype
        ///(function area * Fs)/ slot area
        ///only applicable for ECMD trays
        /// </summary>
        public int RequiredSlotCount => DesignFamily.IsEcmdDesignFamily() ? mdSlotting.TotalRequiredSlotCount(Deck.SlotType, Deck.SlottingPercentage, FunctionalActiveArea) : 0;

        /// <summary>
        /// the total number of ring clips required to assemble a single tray
        /// </summary>
        public int RingClipCount => DeckSections.RingClipCount + Downcomers.RingClipCount;

        /// <summary>
        /// the radius of the bolt circle that all ring clip holes in the tray will fall on
        /// ~used to lay out ring clips correctly
        /// </summary>
        public double RingClipRadius => base.BoundingRadius;

        /// <summary>
        /// the distance between support rings in this tray range
        /// </summary>
        public override double RingSpacing
        {
            get
            {
                double _rVal = base.RingSpacing;
                return (_rVal < 8) ? 8 : _rVal;
            }
            set
            {
                mdTrayRange aRange = GetMDRange();
                if (aRange == null) return;
                aRange.RingSpacing = value;
                SubPart(aRange);
            }
        }


        /// <summary>
        /// controls how the optimimum downcomer spacing is calculated
        /// ~set at project wide level
        /// </summary>
        public uppMDSpacingMethods SpacingMethod
        {
            get
            {
                mdTrayRange aRange = GetMDRange();
                return (aRange != null) ? aRange.SpacingMethod : uppMDSpacingMethods.Weighted;
            }
        }

        /// <summary>
        /// a string that describes the current splice style setting
        /// ~i.e. 'Slot & Tab' or 'Joggles' et.
        /// </summary>
        public string SpliceStyleName => uopEnums.Description(DesignOptions.SpliceStyle);

        /// <summary>
        /// the current splice style setting
        /// ~i.e. 'Slot & Tab' or 'Joggles' et.
        /// </summary>
        public override uppSpliceStyles SpliceStyle => DesignOptions.SpliceStyle;

        /// <summary>
        /// ^returns the collection of all spout groups defined for the assembly
        /// </summary>
        public colMDSpoutGroups SpoutGroups
        {
            get => GenerateSpoutGroups();
            set
            {
                //^returns the collection of all spout groups defined for the assembly
                if (value != null)
                {
                    _SpoutGroups.SubPart(this);
                    _SpoutGroups = value;
                    // _SpoutGroups.eventSpoutGroupMemberChanged += _SpoutGroups_SpoutGroupMemberChanged;
                    _SpoutGroups.eventSpoutGroupsGenerationEvent += _SpoutGroups_SpoutGroupsGenerationEvent;
                    _SpoutGroups.eventSpoutGroupsInvalidated += _SpoutGroups_SpoutGroupsInvalidated;
                }
                else
                {
                    Invalidate(uppPartTypes.SpoutGroup);
                }

            }
        }

        /// <summary>
        /// returns a collection of rectangulas that encompass the spout groups in the layout view of the tray
        /// </summary>
        public colDXFRectangles SpoutRegions
        {
            get
            {
                colDXFRectangles _rVal = new colDXFRectangles { MaintainIndices = true };
                colMDSpoutGroups sGroups = SpoutGroups;
                for (int i = 1; i <= sGroups.Count; i++)
                {
                    mdSpoutGroup sGroup = sGroups.Item(i);
                    if (!sGroup.IsVirtual)
                    {
                        USHAPE Perim = sGroup.PerimeterV;
                        dxfRectangle Region = Perim.Limits.ToDXFRectangle();
                        Region.Tag = sGroup.GroupIndex.ToString();
                        Region.Flag = sGroup.Handle;
                        _rVal.Add(Region);

                    }
                }
                return _rVal;
            }
        }
        /// <summary>
        /// the configuration last used to generate the trays startup spouts
        /// </summary>
        public uppStartupSpoutConfigurations StartupConfiguration
        {
            get
            {
                int iVal = PropValI("StartupConfiguration");
                uppStartupSpoutConfigurations _rVal = (uppStartupSpoutConfigurations)PropValI("StartupConfiguration");
                return _rVal;
            }
            set { if (value >= 0) PropValSet("StartupConfiguration", (int)value, bSuppressEvnts: true); }
        }
        public double StartupDiameter  { get{ double ht = MetricSpouting ? 10 / 25.4 : 0.375;   _Downcomer.PropValSet("StartupDiameter", ht, bSuppressEvnts : true); return ht; } }

        public double StartupLength => _Downcomer.PropValD("StartUpLength");

        public uopHole StartupSpout => StartupSpouts.StartupSpout;

        /// <summary>
        /// the startup spouts defined for the assembly
        /// </summary>
        public mdStartupSpouts StartupSpouts
        {
            get => GenerateStartupSpouts();

            set
            {
                //^the startup spouts defined for the assembly
                bool bChange = (value == null) || !value.IsEqual(_StartupSpouts, true);


                if (value == null) { _StartupSpouts.Clear(); } else { _StartupSpouts = value; }
                _StartupSpouts.SubPart(this);
                _StartupSpouts.Invalid = false;
                _StartupSpouts.TargetArea = IdealStartupArea;
                _Downcomer.PropValSet("StartupDiameter", _StartupSpouts.Height, bSuppressEvnts: true);
                _Downcomer.PropValSet("StartupLength", _StartupSpouts.Length, bSuppressEvnts: true);
                _Downcomers.RefreshProperties(this, _Downcomer);
                _Downcomers.SetStartupSpouts(_StartupSpouts);
                if (bChange && !Reading)
                {
                    Notify(uopProperty.Quick("StartupSpouts", "New Spouts", "Old Spouts", this));
                }

            }
        }


        /// <summary>
        ///the areas of the tray that are the free bubbling area shapes 
        /// </summary>
        public mdFreeBubblingAreas FreeBubblingAreas =>SpacingData?.FBAs;

        /// <summary>
        /// the ideal spout area for the entire assembly
        /// ~maps to the Assembly.Dowcomer.Asp property
        /// </summary>
        public double TheoreticalSpoutArea => Downcomer().ASP;

        /// <summary>
        /// returns the total unobstructed deck panel area for the entire tray
        /// </summary>
        public double TotalFreeBubblingArea => FreeBubblingAreas != null ? FreeBubblingPanels.TotalArea() : 0;

        /// <summary>
        /// the total area of all the deck panels in the assembly
        /// </summary>
        public double TotalPanelArea => DeckPanels.TotalPanelArea;

        /// <summary>
        /// returns the count of all the slots on all the panels in the entire tray. Returns 0 if the tray is not and ECMD design.
        /// </summary>
        public int TotalSlotCount => DesignFamily.IsEcmdDesignFamily() ? SlotZones.TotalSlotCount(this) : 0;


        /// <summary>
        /// the total area of all the spouts in all the spout groups in the assembly
        /// </summary>
        public double TotalSpoutArea => SpoutGroups.TotSpoutArea;

        /// <summary>
        /// returns the total weir length for the entire tray assembly
        /// ~this is the sum of all the downcomers weir lengths
        /// </summary>
        public double TotalWeirLength => Downcomers.TotalWeirLength;

        public override uopSheetMetal BeamMaterial => Downcomer().SheetMetal;

        public override uopSheetMetal DowncomerMaterial => Downcomer().SheetMetal;

        public override uopSheetMetal DeckMaterial => Deck.SheetMetal;
        public override uopPart DeckObj => Deck;

        public override uopPart DowncomerObj => Downcomer();

        public override uopPart DesignOptionsObj => DesignOptions;

        public bool IsStandardDesign => DesignFamily.IsStandardDesignFamily();

        public bool IsSymmetric => IsStandardDesign; // && !OddDowncomers;

        public bool IsECMD => DesignFamily.IsEcmdDesignFamily();

        internal mdSlotZones _SlotZones;

        public mdSlotZones SlotZones
        {
            get => GenerateSlotZones();


            set
            {
                if (value == null)
                {
                    _SlotZones.Clear();
                    _SlotZones.Invalid = true;
                }
                else
                {
                    _SlotZones = value;

                }
            }
        }

        public override uppProjectTypes ProjectType { get => base.ProjectType; 
            set 
            { 
                base.ProjectType = value; 
                if(_Downcomers != null) _Downcomers.ProjectType = value;
            } 
        }

        private uopFreeBubblingPanels _FreeBubblingPanels;
        /// <summary>
        /// the panels that contain the shape(s) of the deck panels free bubbling areas
        /// </summary>
        public uopFreeBubblingPanels FreeBubblingPanels
        {
            get
            {
                _FreeBubblingPanels??= DowncomerData.FreeBubblingPanels();
                return _FreeBubblingPanels;
            }
            set
            {
                _FreeBubblingPanels = value;
            }
        }

        public override List<uopDeckSplice> Splices => DeckSplices;


        /// <summary>
        /// returns the elevation of the bottom of the downcomers
        /// </summary>
        /// <returns></returns>
        public double ZBottom => -Downcomer().HeightBelowDeck;


        #endregion Properties

        #region Methods



        /// <summary>
        /// returns the active areas of the assembly. Used to compute the FunctionaActiveArea
        /// </summary>
        /// <remarks>the first shape is the ring circle. the rest are the negative ares representing the area removed to compute the Functional Active Area</remarks>
        public uopShapes FunctionalActiveAreas(bool bIgnoreAngledEnPlates = false, bool bIgnoreBeams = false, bool bOmmitShellArea = false)
         => FunctionalActiveAreas(out List<uopLinePair> _, bIgnoreAngledEnPlates, bIgnoreBeams, bOmmitShellArea);
        
              /// <summary>
              /// returns the active areas of the assembly. Used to compute the FunctionaActiveArea
              /// </summary>
              /// <remarks>the first shape is the ring circle. the rest are the negative ares representing the area removed to compute the Functional Active Area</remarks>
        public uopShapes FunctionalActiveAreas(out List<uopLinePair> rTheoreticalWeirs, bool bIgnoreAngledEndPlates, bool bIgnoreBeams, bool bOmmitShellArea)
        {
            uopShapes _rVal = !bOmmitShellArea ? new uopShapes() { new uopShape(new uopArc(ShellRadius), "SHELL_AREA") } : new uopShapes();
             rTheoreticalWeirs = Downcomers.TheoreticalWeirs(bIgnoreAngledEndPlates);
            double thk = Downcomer().Thickness;
            int i = 0;
            double botarea = 0;
            double weirarea = 0;
            foreach (var item in rTheoreticalWeirs)
            {
                i++;

                uopLine left = item.GetSide(uppSides.Left);
                uopLine right = item.GetSide(uppSides.Right);

                uopShape weirR = new uopShape(new uopVectors(new uopVector(right.X(), right.MaxY), new uopVector(right.X(), right.MinY), new uopVector(right.X() - thk, right.MinY), new uopVector(right.X() - thk, right.MaxY)), "THEORETICAL_WEIR_RIGHT");
                _rVal.Add(weirR);
                uopShape weirL = new uopShape(new uopVectors(new uopVector(left.X(), left.MaxY), new uopVector(left.X(), left.MinY), new uopVector(left.X() + thk, left.MinY), new uopVector(left.X() + thk, left.MaxY)), "THEORETICAL_WEIR_LEFT");
               _rVal.Add(weirL);

                weirarea += weirL.Area;
                weirarea += weirR.Area;

                uopVectors botverts = new uopVectors() { new uopVector(left.X() + thk, left.MaxY), new uopVector(left.X() + thk, left.MinY), new uopVector(right.X() - thk, right.MinY), new uopVector(right.X() - thk, right.MaxY) };
                
                uopShape bot = new uopShape(botverts, "DCAREA");

                _rVal.Add(bot);
                botarea += bot.Area;
            }

            if (DesignFamily.IsBeamDesignFamily() && !bIgnoreBeams) 
            {
                double srad = ShellRadius;
                List<ULINEPAIR> pairs = Downcomers.CurrentDataSet(this).CreateDividerLns(srad, 0);

                foreach (var item in pairs)
                {
                    uopVectors verts = new uopVectors( item.GetSide(aSide: uppSides.Top).Value.EndPoints);

                    verts.Sort(dxxSortOrders.RightToLeft);
                    verts[0].Radius = 0;
                    verts[1].Radius = srad;
                    uopVectors v2 = new uopVectors(item.GetSide(aSide: uppSides.Bottom).Value.EndPoints);
                    v2.Sort(dxxSortOrders.LeftToRight);
                    v2[0].Radius = 0;
                    v2[1].Radius = srad;
                    verts.Append(v2);
                    uopShape beam= new uopShape(verts, "BEAM");

                    _rVal.Add(beam);
                } 
            }
            //if()

            return _rVal;

         
        }

 
        public void SetDeckSplices(uopDeckSplices aSplices, bool bCloneIt = true)
        {
            if (aSplices == null) return;
            bool bRaiseIt = !_DeckSplices.IsEqual(aSplices, this);
            if (_DeckSplices != null) _DeckSplices.eventDeckSplicesInvalidated -= _DeckSplices_DeckSplicesInvalidated;
            _DeckSplices = bCloneIt ? aSplices.Clone() : aSplices;
            _DeckSplices.eventDeckSplicesInvalidated += _DeckSplices_DeckSplicesInvalidated;
            if (aSplices.SpliceStyle > uppSpliceStyles.Undefined) DesignOptions.PropValSet("SpliceStyle", aSplices.SpliceStyle, bSuppressEvnts: true);
             
            _DeckSplices.Invalid = false;
            Invalidate(uppPartTypes.FlowSlotZone);
            Invalidate(uppPartTypes.BubblePromoter);
        
            eventMDAssemblyPartsInvalidated?.Invoke();
            //to invalidate all the sub components that are dependent on the splices
            if (bRaiseIt)
            {
                _DesignOptions.PropValSet("HasTiledDecks", _DeckSplices.HasHorizontalMembers(false), bSuppressEvnts: true);
                _DesignOptions.PropValSet("SpliceStyle", _DeckSplices.SpliceStyle, bSuppressEvnts: true);
                Invalidate(uppPartTypes.SpliceAngle);
                Invalidate(uppPartTypes.BubblePromoter);
                Invalidate(uppPartTypes.DeckSection);
                _DeckSplices.Invalid = false;
                // GenerateDeckSections(true);



            }
        }

        public void SubPart(mdTrayRange aRange, string aCategory = null, bool? bHidden = null)
        {
            if (aRange == null) return;
            base.SubPart(aRange, aCategory, bHidden);
            ColumnLetter = aRange.ColumnLetter;
            ProjectType = aRange.ProjectType;
        }

        /// <summary>
        /// executed to optimize the FBA/Weir Length ratios of the downcomers in the collection
        /// </summary>
        /// <param name="aAssy"></param>
        public void OptimizeSpacing()
        {
            if (_Downcomers.Optimizing) return;
            ResetComponents();
            _Downcomers.RefreshProperties(this, _Downcomer);
            _Downcomers.OptimizeSpacing(this);
            _Downcomers.Invalid = false;
            double spacing = _Downcomers.Spacing;
            PropValSet("Spacing", spacing, bSuppressEvnts: true);
            _Downcomer.PropValSet("Spacing", _Downcomers.Spacing, bSuppressEvnts: true);

        }

        /// <summary>
        /// returns the tab spacing used to lay out tabs on the passed assembly
        /// </summary>
        /// <param name="bVertical"></param>
        /// <returns></returns>
        public double TabSpacing(bool bVertical) => DowncomerData.TabSpacing(bVertical, out _, out _);


        /// <summary>
        /// returns the tab spacing used to lay out tabs on the assembly
        /// </summary>
        /// <param name="bVertical"></param>
        /// <param name="rWidth"></param>
        /// <param name="rCount"></param>
        /// <returns></returns>
        public override double TabSpacing(bool bVertical, out double rWidth, out int rCount)
         =>  DowncomerData.TabSpacing(bVertical,out rWidth, out rCount);
            
        /// <summary>
        /// returns a collection of rectanguls that encompass the downcomers in the layout view of the tray
        /// </summary>
        public colDXFRectangles DowncomerRegions(bool bTrimmed = false)
        {

            colDXFRectangles _rVal = new colDXFRectangles() { MaintainIndices = true };
            colMDDowncomers DComers = Downcomers;
            double sRad = ShellID / 2 + 1;
            double bot;

            if (bTrimmed)
                bot = DeckPanels.LastPanel().Bounds.Left;
            else
                bot = 0;

            for (int i = 1; i <= DComers.Count; i++)
            {
                mdDowncomer DComer = DComers.Item(i);
                if (!DComer.IsVirtual)
                {

                    double width = (DComer.Width + 2 * DComer.ShelfWidth) * 1.1;
                    double top = Math.Sqrt(Math.Pow(sRad, 2) - Math.Pow(DComer.X - width / 2, 2));
                    if (!bTrimmed) bot = -top;
                    double height = top - bot;
                    dxfVector v1 = new dxfVector(DComer.X, bot + height / 2);
                    dxfRectangle rec = new dxfRectangle(v1, width, height, aTag: i.ToString());

                    _rVal.Add(rec);
                }

            }
            return _rVal;

        }

        /// <summary>
        /// the deck sections of the assembly reduced to only the unique members with instances defining the locations of each members equal members in the assemblies deck section collection
        /// </summary>
        ///<remarks>the unique sections are created and retained in the parent projects parts matrix </remarks>
        /// <param name="bAltRing">if true, the returned collection is the unique deck sections needed to assembly of the tray  on it's altenate ring</param>
        /// <returns></returns>
        public List<mdDeckSection> UniqueDeckSections(bool bAltRing = false)
        {

            mdProject proj = MDProject;
            if (proj == null) return DeckSections;
            List<mdDeckSection> _rVal = bAltRing ? proj.GetParts().AltDeckSections(RangeGUID) : proj.GetParts().DeckSections(RangeGUID);

            return _rVal;


        }

        /// <summary>
        /// returns the objects properties in a collection
        /// ~signatures like "COLOR=RED"
        /// #1flag to include object property properies in the returned collection
        /// </summary>
        public override uopProperties CurrentProperties() { UpdatePartProperties(); return base.CurrentProperties(); }


        /// <summary>
        /// used by an object to respond to property changes of itself and of objects below it in the object model.
        /// also raises the changed property to the object above it it the object model.
        /// </summary>
        /// <param name="aProperty"></param>
        public override void Notify(uopProperty aProperty)
        {
            if (aProperty == null || Reading) return;


            uppPartTypes pType = aProperty.PartType;
            string pname = aProperty.Name.ToUpper();
            bool bResetSlots = false;

            ResetSubComponents();

            if (aProperty.PartType == uppPartTypes.Deck)
            {
                if (string.Compare(aProperty.Name, "Material", true) == 0)
                {
                    if (_DeckSections != null) _DeckSections.Material = _Deck.SheetMetal;
                }
            }

            if (pname == "DESIGNFAMILY")
            {
                bResetSlots = true;
            }
            else if (pname == "FOLDOVERHEIGHT")
            {
                Invalidate(uppPartTypes.DeckSection);
            }

            ///'respond to my own property changes
            if (pType == uppPartTypes.TrayAssembly)
            {

                if (pname == "OVERRIDESPACING")
                {
                    if (_Downcomers != null)
                    {
                        _Downcomers.Optimized = false;
                    }
                }

            }
            else if (pType == uppPartTypes.Constraint || pType == uppPartTypes.SpoutGroup)
            {

            }
            else if (pType == uppPartTypes.TraySupportBeam)
            {
                if (pname == "WIDTH" || pname == "OFFSET" || pname == "OFFSETFACTOR")
                {
                    ResetOverride(0);
                    // Complete start over of all components
                    _Downcomers?.Clear();
                    ResetComponents(true);
                }
            }
            else if (pType == uppPartTypes.Downcomer)
            {
                if (aProperty.IsGlobal)
                {
                    switch (pname.ToUpper())
                    {
                        case "MATERIAL":
                            {
                                DesignOptions.SheetMetalStructure = _Downcomer.SheetMetalStructure;
                                if (_Downcomers != null)
                                {
                                    _Downcomers.Optimized = false;
                                    _Downcomers.UpdateMaterial(_Downcomer.SheetMetal);

                                    Invalidate(uppPartTypes.DeckPanel);
                                }
                                Invalidate(uppPartTypes.EndAngle);
                                break;
                            }
                        case "STARTUPDIAMETER":
                            {
                                Invalidate(uppPartTypes.StartupSpout);
                                break;
                            }

                        case "COUNT":
                            {

                                ResetOverride(0);
                                //'complete start over of all components
                                ResetComponents();

                                if (!Reading)
                                {
                                    _Deck.PropValSet("ManwayCount", 0, bSuppressEvnts: true);
                                }
                                break;
                            }
                        case "WIDTH":
                            {
                                ResetOverride(0);
                                //'complete start over of all components
                                ResetComponents();
                                break;
                            }
                        case "ASP":
                        case "SPOUTDIAMETER":
                            {
                                //these cause all spout groups to be regenerated on the next request
                                _Downcomers.UpdateProperty(aProperty.Name, aProperty.Value, false);
                                RecalculateSpoutGroups();
                                break;
                            }
                        case "INSIDEHEIGHT":
                            {
                                //these don't effect any other components so just update the collection downcomers to match
                                _Downcomers.UpdateProperty(aProperty.Name, aProperty.Value, false);
                                break;
                            }
                        case "HOW":
                            {
                                //these don't effect any other components so just update the collection downcomers to match
                                Invalidate(uppPartTypes.FlowSlotZone);
                                break;
                            }


                        case "ENDPLATEINSET":
                        case "MINIMUMCLIPCLEARANCE":
                        case "HASTRIANGULARENDPLATE":
                            {
                                //these can effect spacing but we leave it to the indiviual downcomers to raise an event to cause regen or reopt
                                ResetOverride(0);
                                Downcomers.UpdateProperty(aProperty.Name, aProperty.Value, true);
                                Invalidate(uppPartTypes.APPan);
                                break;
                            }
                    };
                }
                else
                {
                    Invalidate(uppPartTypes.APPan);
                    if (_Downcomers != null)
                    {

                        if (aProperty.IsNamed("MATERIAL,ENDPLATEINSET,HASTRIANGULARENDPLATE"))
                        {
                            _Downcomers.Optimized = false;
                            Invalidate(uppPartTypes.DeckPanel);
                        }
                        if (aProperty.IsNamed("SupplementalDeflectorHeight,StiffenerSites"))
                        {

                            Invalidate(uppPartTypes.Stiffener);
                            Invalidate(uppPartTypes.SupplementalDeflector);
                        }
                        if (!pname.ToUpper().Contains("STIFF") && !pname.ToUpper().Contains("STARTUP"))
                        {
                            if (!pname.ToUpper().Contains("FOLDOVER"))
                            {
                                if (pname == "HASTRIANGULARENDPLATE" || pname == "ENDPLATEINSET")
                                {
                                    Invalidate(uppPartTypes.SpoutGroup);
                                }
                                else
                                {
                                    Invalidate(uppPartTypes.DeckPanel);
                                }
                            }
                        }
                    }

                }
            }

            else if (pType == uppPartTypes.DesignOptions || pType == uppPartTypes.Deck)
            {
                // '===============================================

                if (pname == "HASANTIPENETRATIONPANS" || pname == "FEDorAPPHeight".ToUpper())
                { ResetSubComponents(); }

                //'any change to member properties causes a reopt to spark
                if (pname == "HASBUBBLEPROMOTERS" || pname == "HASTILEDDECKS")
                {
                    Invalidate(uppPartTypes.DeckSplice);
                }
                if (pname == "HASBUBBLEPROMOTERS") { bResetSlots = true; }
                //'any change to member properties causes a reopt to spark
                if (pname == "SPLICESTYLE" || pname == "HASBUBBLEPROMOTERS")
                {
                    Invalidate(uppPartTypes.DeckSplice);
                    bResetSlots = true;
                }
                else if (pname == "USEMANWAYCLIPS")
                {
                    bResetSlots = true;
                }

                else if (pname == "MAXRINGCLIPSPACING" || pname == "MOONRINGCLIPSPACING")
                {
                    bResetSlots = true;
                }
            }

            if (bResetSlots) ResetSlots();

            PropertyChangeEvent?.Invoke(aProperty);
            eventMDAssemblyPartsInvalidated?.Invoke();
        }

        /// <summary>
        /// returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>
        public mdTrayAssembly Clone() => new mdTrayAssembly(this);

        public override uopPart Clone(bool aFlag = false) => (uopPart)this.Clone();

        /// <summary>
        /// the absolute value of the difference of the area ratio (actual/theo) and 1
        /// ~represents the percentage over or under the required theoretical area
        /// #1flag to return the ablsolute value
        /// </summary>
        public double AreaRatioDifferential(bool AbsVal = true) => AbsVal ? Math.Abs(AreaRatio - 1) : AreaRatio - 1;

        public uopSheetMetal GetSheetMetal(bool bReturnThickest)
        {
            uopSheetMetal deck = Deck.SheetMetal;
            uopSheetMetal dc = Downcomer().SheetMetal;
            if (bReturnThickest)
                return deck.Thickness >= dc.Thickness ? deck : dc;
            else
                return deck.Thickness <= dc.Thickness ? deck : dc;

        }

        public uopVectors BPCenters(bool bTrayWide = false, bool? aSuppressedValue = null, bool bRegen = false)
        {
            if (!DesignOptions.HasBubblePromoters) return uopVectors.Zero;

            if (bRegen) _BPSites = null;
            uopVectors _rVal = new uopVectors(  BPSites);
            if (aSuppressedValue.HasValue)
            {
                _rVal.RemoveAll((x) => x.Suppressed != aSuppressedValue.Value);
            }
            if (bTrayWide && IsSymmetric)
            {
                uopVectors others = _rVal.GetVectors(aFilter: dxxPointFilters.GreaterThanX, aOrdinate: 0, bOnIsIn: false, aPrecis: 1, bReturnClones: true);
                others.Mirror(0,null);
                _rVal.Append(others);

            }

            return _rVal;
        }

        public override string ToString() => TrayName(true);

        public uopRectangle FeatureViewRectangle(bool bIncludePanels, bool bIncludDowncomers = true, bool bIncludFullDowncomers = false, bool bIncludeFullPanels = false, Double aWidthBuffer = 0, Double aHeightBuffer = 0, bool bIncludeVirtuals = false)
        {
            uopRectangle _rVal = SpoutGroups.Bounds(bIncludeVirtuals);

            if (bIncludDowncomers || bIncludePanels)
            {
                uopRectangle dprect = new uopRectangle();
                uopRectangle dcrect = Downcomers.Bounds(bExcludeVirtuals: !bIncludeVirtuals);
                if (bIncludePanels)
                {
                    dprect = DeckPanels.Bounds(bExcludeVirtuals: !bIncludeVirtuals);
                    if (dprect.Left < _rVal.Left) _rVal.Left = dprect.Left;
                    if (dprect.Left < _rVal.Bottom) _rVal.Bottom = dprect.Left;
                    if (dprect.Right > _rVal.Right) _rVal.Right = dprect.Right;
                    if (dprect.Right > _rVal.Top) _rVal.Top = dprect.Right;
                }
                else
                {
                    if (bIncludDowncomers)
                    {
                        if (dcrect.Left < _rVal.Left) _rVal.Left = dcrect.Left;
                        if (dcrect.Left < _rVal.Bottom) _rVal.Bottom = dcrect.Left;
                        if (dcrect.Right > _rVal.Right) _rVal.Right = dcrect.Right;


                    }
                }
                if (bIncludFullDowncomers)
                {
                    if (OddDowncomers)
                    {
                        _rVal.Top = ShellID / 2 + 1;
                        _rVal.Bottom = -_rVal.Top;
                    }
                    else
                    {
                        if (dcrect.Top > _rVal.Top) _rVal.Top = dcrect.Top;
                        if (dcrect.Bottom < _rVal.Bottom) _rVal.Bottom = dcrect.Bottom;
                    }


                }


                if (bIncludeFullPanels)
                {
                    if (!OddDowncomers)
                    {
                        _rVal.Top = ShellID / 2 + 1;
                        _rVal.Bottom = -_rVal.Top;
                    }
                    else
                    {
                        if (dprect.Top > _rVal.Top) _rVal.Top = dprect.Top;
                        if (dprect.Left < _rVal.Bottom) _rVal.Bottom = dprect.Left;
                    }


                }
            }

            _rVal.Stretch(aWidthBuffer, aHeightBuffer);

            return _rVal;
        }

        /// <summary>
        /// the drawings that are available for this part
        /// </summary>
        public override uopDocuments Drawings() => GenerateDrawings(null, GetMDRange());

        /// <summary>
        /// returns a collection of strings that are warnings about possible problems with
        /// ^the current tray assembly design.
        /// ~these warnings may or may not be fatal problems.
        /// </summary>
        public override uopDocuments Warnings() => GenerateWarnings(null);

        public override uopProperty CurrentProperty(string aPropertyName, bool bSupressNotFoundError = false)
        {
            UpdatePartProperties();
            return base.CurrentProperty(aPropertyName, bSupressNotFoundError);
        }

        public mzValues DetailList(mzValues aCollector)
        {
            mzValues _rVal = aCollector ?? new mzValues();
            colMDDowncomers DCs = null;
            uopDeckSplices aSplcs = DeckSplices;

            mzValues sTyps = aSplcs.SpliceTypes(out int iMnws);

            DCs = Downcomers;

            //ring clips
            if (RingClip(false).Size == uppRingClipSizes.ThreeInchRC)
            {
                _rVal.Add("A1", true);
            }
            else
            {
                _rVal.Add("A3", true);
            }
            if (RingClip(true).Size == uppRingClipSizes.ThreeInchRC)
            {
                _rVal.Add("A2", true);
            }
            else
            {
                _rVal.Add("A4", true);
            }
            //finger clips
            if (DesignOptions.WeldedStiffeners)
            {
                _rVal.Add("C1", true);
            }
            else
            {
                _rVal.Add("C2", true);
            }
            //end angles/end plates
            _rVal.Add("D1", true);
            if (DCs.LastMember().BoltOnEndplates)
            {
                _rVal.Add("D2", true);
            }

            //cross braces
            if (DCs.Count > 1 && DesignOptions.CrossBraces)
            {
                _rVal.Add("E1", true);
            }

            //splices
            //    uopSpliceUndefined = -1
            //    uopSpliceWithTabs = 0
            //    uopSpliceWithAngle = 1
            //    uopSpliceWithJoggle = 2
            //    uopSpliceManwayCenter = 3

            if (sTyps.ContainsValue(uppSpliceTypes.SpliceWithAngle))
            {
                _rVal.Add("F1", true);
            }
            if (sTyps.ContainsValue(uppSpliceTypes.SpliceWithTabs))
            {
                _rVal.Add("F3", true);
            }
            if (sTyps.ContainsValue(uppSpliceTypes.SpliceWithJoggle))
            {
                _rVal.Add("K3", true);
            }
            if (iMnws > 0)
            {
                if (aSplcs.SpliceStyle == uppSpliceStyles.Tabs)
                {

                    if (DesignOptions.UseManwayClips)
                    {
                        _rVal.Add("M4", true);
                        _rVal.Add("M13", true);
                        _rVal.Add("M14", true);
                    }
                    else
                    {
                        //footballs
                        _rVal.Add("M3", true);
                        _rVal.Add("M11", true);
                        _rVal.Add("M14", true);
                    }
                }
                else
                {
                    if (DesignOptions.UseManwayClips)
                    {
                        _rVal.Add("M2", true);
                        _rVal.Add("M12", true);
                        _rVal.Add("M13", true);
                    }
                    else
                    {
                        //footballs
                        _rVal.Add("M1", true);
                        _rVal.Add("M10", true);
                        _rVal.Add("M11", true);
                    }

                }
            }

            if (DesignFamily.IsEcmdDesignFamily())
            {
                _rVal.Add("Q1", true);
            }
            if (HasAntiPenetrationPans)
            {
                _rVal.Add("S1", true);
                _rVal.Add("S2", true);
            }

            return _rVal;
        }

        public uopVectors FingerClipPoints(int aDCIndex = 0, bool bTrayWide = true,bool bReturnCenters = false) => ProjectType == uppProjectTypes.MDDraw ? mdUtils.FingerClipPoints(this, null, mdPartGenerator.Stiffeners_ASSY(this, false, bTrayWide:bTrayWide), aDCIndex, bTrayWide: bTrayWide,bReturnCenters: bReturnCenters) : uopVectors.Zero;

        public double PerfFractionPercentOpen(out double rFunctionalArea, out double rMechanicalArea) => Deck.Fp * RFactor(out rFunctionalArea, out rMechanicalArea);

        public double RFactor(out double rFunctionalArea, out double rMechanicalArea)
        {

            rFunctionalArea = FunctionalActiveArea;
            rMechanicalArea = MechanicalActiveArea;
            double _rVal = rMechanicalArea != 0 ? rFunctionalArea / rMechanicalArea : 0;
            return _rVal;
        }

        /// <summary>
        /// the collection of Baffle plates for a ECMD Tray Assembly
        /// </summary>
        /// <param name="aDCIndex"></param>
        /// <returns></returns>
        public List<mdBaffle> DeflectorPlates(int aDCIndex = 0)
        {
            if (DesignFamily.IsEcmdDesignFamily())
            {
            
                return mdPartGenerator.DeflectorPlates_ASSY(this, false, aDCIndex);
            }
            else
            {
                return new List<mdBaffle>();
            }
        }
        /// <summary>
        /// the areas of the panels that are blocked by mechanical fasteners
        /// </summary>
        /// <returns></returns>
        public uopShapes BlockedAreas()
        {

            
            if (ProjectType != uppProjectTypes.MDDraw) return new uopShapes("BLOCKED AREAS");
         
            return  DeckSections.BaseShapes(false,this).GetSubShapes(uppSubShapeTypes.BlockedAreas,this );
            //mdFreeBubblingAreas fbas = FreeBubblingAreas;
            //  return (fbas == null) ?  new uopShapes("BLOCKED AREAS") : fbas.BlockedAreas;
           
        }


        public mdSlotZones GenerateSlotZones(bool bRegen = false)
        {
            _SlotZones ??= new mdSlotZones();
            if (_SlotZones.Invalid || bRegen)
            {
                _SlotZones.Invalid = false;
                if (DesignFamily.IsEcmdDesignFamily()) RaiseStatusChangeEvent($"Generating {TrayName()} Slot Zones");
                _SlotZones = mdSlotting.SlotZones_Create(this, DeckSections, _SlotZones);
                mdSlotting.SetSlotCounts(this, _SlotZones, FunctionalActiveArea);
                //_SlotZones.Structure_Set(zones);
                _SlotZones.Invalid = false;
            }
            else
            {
                if (_SlotZones.FunctionArea == 0 || _SlotZones.RequiredSlotCount == 0)
                {
                    mdSlotting.SetSlotCounts(this, _SlotZones, FunctionalActiveArea);
                }
            }
            _SlotZones.SubPart(this);
            return _SlotZones;

        }
  

        public mdSlotZone SlotZone(string aSectionHandle, bool bRegen = false)
        {

            mdSlotZones myzones = GenerateSlotZones(bRegen);
            return myzones?.Find((x) => x.SectionHandle == aSectionHandle);
        }

        public colDXFVectors SpliceAngleCenters(bool bSuppressLeftSide = false, bool bSuppressRightSide = false, bool bSuppressFieldAngles = false, bool bSuppressManwayAngles = false, bool bSuppressManwaySplices = false, colUOPParts rMembers = null)
        {
            colDXFVectors _rVal = new colDXFVectors();
            if (bSuppressLeftSide && bSuppressRightSide) return _rVal;

            colUOPParts aSAs = null;
            mdSpliceAngle aSA = null;
            bool bKeep = false;
            rMembers = new colUOPParts();
            aSAs = SpliceAngles();
            for (int i = 1; i <= aSAs.Count; i++)
            {
                aSA = (mdSpliceAngle)aSAs.Item(i);
                bKeep = false;
                if (aSA.AngleType == uppSpliceAngleTypes.SpliceAngle)
                {
                    bKeep = !bSuppressFieldAngles;
                }
                else if (aSA.PartType == uppPartTypes.ManwayAngle)
                {
                    bKeep = !bSuppressManwayAngles;
                }
                else if (aSA.PartType == uppPartTypes.ManwaySplicePlate)
                {
                    bKeep = !bSuppressManwaySplices;
                }
                if (bKeep)
                {
                    rMembers.Add(aSA, true).PartIndex = i;
                    if (!bSuppressRightSide || (bSuppressRightSide && Math.Round(aSA.X, 1) == 0))
                    {
                        SpliceAngleCenters().Add(aSA.X, aSA.Y, aSA.Z);
                    }
                    if (!bSuppressLeftSide)
                    {
                        if (Math.Round(aSA.X, 1) > 0)
                        {
                            SpliceAngleCenters().Add(-aSA.X, -aSA.Y, aSA.Z);
                        }
                        else
                        {
                            if (bSuppressRightSide)
                            {
                                SpliceAngleCenters().Add(-aSA.X, -aSA.Y, aSA.Z);
                            }
                        }
                    }
                }
            }
            return _rVal;
        }

        /// <summary>
        /// this downcomer is used as the basis for new downcomers
        /// ^added to the downcomers collection
        /// </summary>
        /// <param name="newobj"></param>
        public mdDowncomer SetDowncomer(mdDowncomer newobj)
        {
            if (newobj == null) return _Downcomer;

            if (_Downcomer != null)
            {
                _Downcomer.PropertyChangeEvent -= SubPartPropertyChangeEventHandler;
                _Downcomer.EventMaterialChange -= _Downcomer_MaterialChange;
            }

            _Downcomer = newobj;
            _Downcomer.PropertyChangeEvent += SubPartPropertyChangeEventHandler;
            _Downcomer.EventMaterialChange += _Downcomer_MaterialChange;

            _Downcomer.IsGlobal = true;
            _Downcomer.Index = 0;
            _Downcomer.DeckThickness = Deck.Thickness;
            _Downcomer.SubPart(this);
            return _Downcomer;
        }

        /// <summary>
        /// this downcomer is used as the basis for new downcomers
        /// ^added to the downcomers collection
        /// </summary>
        /// <param name="aIndex"></param>
        /// <returns></returns>
        public mdDowncomer Downcomer(int aIndex = 0)
        {
            mdDowncomer _rVal;
            if (aIndex <= 0)
            {
                _Downcomer ??= SetDowncomer(new mdDowncomer());
                if (_StartupSpouts != null && !Reading)
                {
                    if (!_StartupSpouts.Invalid)
                    {
                        _Downcomer.PropValSet("StartupDiameter", _StartupSpouts.Height, bSuppressEvnts: true);
                        _Downcomer.PropValSet("StartupLength", _StartupSpouts.Length, bSuppressEvnts: true);
                    }
                }

                _rVal = _Downcomer;
            }
            else
            {
                _rVal = Downcomers.Item(aIndex, bSuppressIndexError: true);
            }
            if (_rVal != null) _rVal.SubPart(this);

            return _rVal;
        }

        /// <summary>
        /// the finger clips defined for the tray
        /// </summary>
        /// <param name="aDCIndex"></param>
        /// <param name="aPanelIndex"></param>
        /// <param name="bBothSides"></param>
        /// <returns></returns>
        public colUOPParts FingerClips(int aDCIndex = 0, int aPanelIndex = 0, bool bBothSides = false) => mdUtils.FingerClips(this, null, aDCIndex: aDCIndex, aPanelIndex: aPanelIndex, aSide: uppSides.Undefined, bBothSides: bBothSides);

        /// <summary>
        /// executed internally to create the ap pans collection
        /// </summary>
        /// <returns></returns>
        public colUOPParts GenerateAPPans(bool bRegen = false)
        {
            _APPans ??= new colUOPParts();
            if (!DesignOptions.HasAntiPenetrationPans)
            {
                _APPans.Clear();
                _APPans.InvalidWhenEmpty = false;
                bRegen = false;
            }
            else
            {
                _APPans.InvalidWhenEmpty = true;
            }

            if (_APPans.Invalid || bRegen)
            {
                if (DesignOptions.HasAntiPenetrationPans) RaiseStatusChangeEvent($"Generating {TrayName()} AP Pans");

                _APPans.Clear();
                int basePN = 800;
                mdPartGenerator.APPans_ASSY(this, ref basePN, TrayCount, aCollector: _APPans);



            }
            _APPans.SubPart(this);
            return _APPans;
        }

        /// <summary>
        /// executed internally to create the set of defined deck panels for the tray assembly
        /// </summary>
        public colMDDeckPanels GenerateDeckPanels(bool bRegen = false)
        {


            if (_DeckPanels.Invalid || bRegen)
            {

                _DeckPanels.InvalidWhenEmpty = false;
                _DeckPanels.Invalid = false;

                if (_Downcomer.Count <= 0)
                {
                    _DeckPanels.Clear();
                    return _DeckPanels;
                }
                _DeckPanels.eventMDPanelMemberChanged -= _DeckPanels_MDPanelMemberChanged;
                eventGenerationEvent?.Invoke(uppTrayGenerationEventTypes.DeckPanels, true, "");
                
                List<mdDeckPanel> DPs = mdPanelGenerator.CreateDeckPanels(this);
                _DeckPanels.Populate(DPs);


                _DeckPanels.eventMDPanelMemberChanged += _DeckPanels_MDPanelMemberChanged;

                //Invalidate(uppPartTypes.DeckSplice);
                //if (ProjectType == uppProjectTypes.MDSpout)
                //{
                //    Invalidate(uppPartTypes.StartupSpout);
                //}
                _DeckPanels.SubPart(this);
                _DeckPanels.Invalid = false;

                eventGenerationEvent?.Invoke(uppTrayGenerationEventTypes.DeckPanels, false, "");
                eventMDAssemblyPartsInvalidated?.Invoke();

                _DeckPanels.InvalidWhenEmpty = _Downcomer.Count > 0;

            }
            return _DeckPanels;

        }



        /// <summary>
        /// executed internally to generate the deck sections collection
        /// </summary>
        /// <param name="bRegen"></param>
        public mdDeckSections GenerateDeckSections(bool bRegen = false)
        {
            //    Invalidate prtDeckSection

            if (_DeckSections == null ||_DeckSections.Invalid || bRegen)
            {

                Invalidate(uppPartTypes.BubblePromoter);
                Invalidate(uppPartTypes.FlowSlotZone);
                Invalidate(uppPartTypes.SpliceAngle);
                if (_SlotZones != null) _SlotZones.Invalid = true;
                if (ProjectType == uppProjectTypes.MDDraw)
                {
                    colMDDeckPanels myPanels = DeckPanels;
                    RaiseStatusChangeEvent($"Generating {TrayName()} Deck Sections");
                    uopDeckSplices mySplices = DeckSplices;
                    if (!SuppressEvents) eventGenerationEvent?.Invoke(uppTrayGenerationEventTypes.DeckSections, true, "");
                    _DeckSections.Invalid = false;
                    _DeckSections.SubPart(this);
                    _DeckSections.Clear();
                    _DeckSections.eventDeckSectionsInvalidated -= _DeckSections_DeckSectionsInvalidated;

                    RaiseStatusChangeEvent($"Creating {TrayName()} Deck Sections"); 
                    mdSection_Generator.GenerateDeckSections(this, aSplices:mySplices, bVerbose:true, bForAltRing:false, aCollector: _DeckSections);
                    _DeckSections.eventDeckSectionsInvalidated += _DeckSections_DeckSectionsInvalidated;

                    if (!SuppressEvents) eventGenerationEvent?.Invoke(uppTrayGenerationEventTypes.DeckSections, false, "");

                }
            }
            _DeckSections.Invalid = false;

            _DeckSections.SubPart(this);
            return _DeckSections;

        }

        public bool IsValid(uppPartTypes aPartType)
        {
            switch (aPartType)
            {
                case uppPartTypes.DowncomerBox:
                case uppPartTypes.Downcomers:
                case uppPartTypes.Downcomer:
                    {
                        if (Downcomer().Count <= 0)
                        {

                            _Downcomers.Clear();
                            return false;
                        }
                        return !_Downcomer.Invalid && _Downcomers.Count == Downcomer().Count;


                    }
            }
            return false;
        }

        /// <summary>
        /// builds the collection of downcomers for the assembly
        /// </summary>
        public colMDDowncomers GenerateDowncomers(bool bRegen = false)
        {


            if (Downcomer().Count <= 0)
            {

                _Downcomers.Clear();
                return _Downcomers;
            }

            if (!IsValid(uppPartTypes.Downcomers) || bRegen)
            {
                //build the basic collection
                if (_Downcomers.Count != Downcomer().Count)
                {
                    PropValSet("OverrideSpacing", 0, bSuppressEvnts: true);
                    Invalidate(uppPartTypes.Downcomer);
                }
                if (_Downcomers.Invalid)
                {
                    RaiseStatusChangeEvent($"Generating {TrayName()} Downcomers");

                    bool dospace = _Downcomers.Count != _Downcomer.Count;
                    _Downcomers.RefreshProperties(this, _Downcomer);


                    Invalidate(uppPartTypes.DeckPanel);
                    eventGenerationEvent?.Invoke(uppTrayGenerationEventTypes.Downcomers, true, "");
                    Invalidate(uppPartTypes.StartupSpout);
                    ResetComponents(false);
                    _Downcomers.Invalid = false;
                    _Downcomers.Initialize(this, dospace);
                    _Downcomers.Invalid = false;
                    eventGenerationEvent?.Invoke(uppTrayGenerationEventTypes.Downcomers, false, "");
                    PropValSet("Spacing", _Downcomers.Spacing, bSuppressEvnts: true);

                }
                // Done:
                if (!_Downcomers.Optimized && !Reading)
                {

                    _Downcomers.OptimizeSpacing(this);
                    PropValSet("Spacing", _Downcomers.Spacing, bSuppressEvnts: true);
                    _Downcomer.PropValSet("Spacing", _Downcomers.Spacing, bSuppressEvnts: true);
                    _Downcomers.Invalid = false;
                }

            }
            _Downcomers.SubPart(this);
            
            if (!Reading)
            {
                _Downcomers.Invalid = false;
                PropValSet("Spacing", _Downcomers.Spacing, bSuppressEvnts: true);
                _Downcomer.PropValSet("Spacing", _Downcomers.Spacing, bSuppressEvnts: true);
            }
            return _Downcomers;
        }

        public override void ResetComponents() => ResetComponents(true);

        /// <summary>
        /// the drawings that are available for this part
        /// </summary>
        /// <param name="aProject"></param>
        /// <param name="aCollector"></param>
        /// <param name="aSheetIndex"></param>
        public uopDocuments GenerateDrawings(mdProject aProject, mdTrayRange aRange, uopDocuments aCollector = null)
        { int SID = 0; return GenerateDrawings(aProject, aRange, aCollector, ref SID); }

        /// <summary>
        /// the drawings that are available for this part
        /// </summary>
        /// <param name="aProject"></param>
        /// <param name="aCollector"></param>
        /// <param name="aSheetIndex"></param>
        public uopDocuments GenerateDrawings(mdProject aProject, mdTrayRange aRange, uopDocuments aCollector, ref int aSheetIndex)
        {

            uopDocuments _rVal = aCollector ?? new uopDocuments();
            GetMDProject(aProject);
            if (aProject == null) return _rVal;
            aRange ??= GetMDRange(aRange);
            string nm;
            uopPart part;
            uopDocDrawing aDWG;
            string tname = $"{TrayName(false)} - ";
            uppUnitFamilies unts = aProject.ManufacturingDrawingUnits;

            if (DesignOptions.CrossBraces)
            {

                part = CrossBrace;
                if (part != null)
                {
                    nm = "Cross Brace";
                    aDWG = _rVal.AddDrawing(uppDrawingFamily.Manufacturing, part, tname + nm, nm, uppDrawingTypes.CrossBrace, uppBorderSizes.BSize_Landscape, aUnits: unts, aRange: aRange);

                    if (aSheetIndex > 0)
                    {
                        aDWG.SheetNumber = aSheetIndex;
                        aSheetIndex += 1;
                    }
                }
            }


            _rVal.Invalid = false;
            return _rVal;
        }
        /// <summary>
        /// returns the parts to assemble and install the part
        /// </summary>
        /// <returns></returns>
        public colUOPParts GenerateParts()
        {

            colUOPParts _rVal = new colUOPParts();

            _rVal.SubPart(this);
            _rVal.Clear();
            _rVal.Invalid = false;
            _rVal.Add(Deck);
            _rVal.Add(DesignOptions);
            _rVal.Add(Downcomer());
            _rVal.Append(Downcomers);
            if (ProjectType == uppProjectTypes.MDDraw)
            {
                GenerateSplices();
            }

            //.Append Constraints.SubParts

            _rVal.Invalid = false;
            return _rVal;
        }

        /// <summary>
        ///returns the properties required to save the object to file
        /// signatures like "COLOR=RED"
        /// </summary>
        public override uopPropertyArray SaveProperties(string aHeading = null)
        {
            UpdatePartProperties();
            uopProperties _rVal = base.CurrentProperties();
            aHeading = string.IsNullOrWhiteSpace(aHeading) ? INIPath : aHeading.Trim();
            return new uopPropertyArray(_rVal, aName: aHeading, aHeading: aHeading);
        }

        public colUOPParts GenerateSpliceAngles(bool bTrayWide)
        {
            List<mdSpliceAngle> angles = DeckSections.GenerateSpliceAngles(this, bTrayWide);
            colUOPParts _rVal = new colUOPParts();
            foreach (var angle in angles) _rVal.Add(angle);
            return _rVal;
        }


        /// <summary>
        /// executed internally to create the deck splices collection for the tray
        /// </summary>
        /// <param name="bRegen"></param>
        public uopDeckSplices GenerateSplices(bool bRegen = false)
        {
            if (_DeckSplices.Invalid && bRegen)
            {
                Invalidate(uppPartTypes.BubblePromoter);
                Invalidate(uppPartTypes.FlowSlotZone);

                if (ProjectType == uppProjectTypes.MDDraw)
                {

                    RaiseStatusChangeEvent($"Generating {TrayName()} Deck Splices");

                    eventGenerationEvent?.Invoke(uppTrayGenerationEventTypes.Splices, true, "");
                    _DeckSplices.SubPart(this);
                    if (_DeckSplices != null) _DeckSplices.eventDeckSplicesInvalidated -= _DeckSplices_DeckSplicesInvalidated;
                    _DeckSplices = uopDeckSplices.ReGenerateSplices(this, _DeckSplices);
                    _DeckSplices.eventDeckSplicesInvalidated += _DeckSplices_DeckSplicesInvalidated;

                    Invalidate(uppPartTypes.DeckSection);

                    //        Set _DeckSections = DeckPanels.GenerateSections(_DeckSplices, Me)
                    eventMDAssemblyPartsInvalidated?.Invoke();
                    eventGenerationEvent?.Invoke(uppTrayGenerationEventTypes.Splices, false, "");
                }
                _DeckSplices.SpliceStyle = DesignOptions.SpliceStyle;
                _DeckSplices.Invalid = false;
                _DeckSplices.SubPart(this);

             
            }
            else
            {
                _DeckSplices.SubPart(this);
            }

            return _DeckSplices;
        }
        /// <summary>
        /// executed to resh the theoretical spout area collection for the spout group
        /// </summary>
        private void GenerateSpoutAreas( mdSpoutZones aZones = null)
        {
        //    if (_SpoutGroups == null) return;
        //    aZones ??= new mdSpoutZones(this);
        //    uopMatrix mtrxLocks = null;
        //    uopMatrix mtrxGroups = null;
        //    string tname = TrayName(false);
        //    int iNd = Downcomer().Count;
        //    int iNp = iNd + 1;

        //    RaiseStatusChangeEvent($"Generating {tname} Spout Areas");
        //    _SpoutGroups.GetLockedAndGroupedAreaMatrices(this, out uopMatrix locks, out uopMatrix groups);


        //    mdSpoutAreaData dobject = new mdSpoutAreaData(this);
        //    //mtrxGroups.PrintToConsole();
        //    _Areas = dobject.GenerateAreaMatrix(this, mtrxLocks, mtrxGroups);



        //    mdSpoutAreaMatrix zoneareas = aZones.SpoutAreaMatrix(bVerbose:false);

        //    if(!_Areas.IsEqual(zoneareas, 4))
        //    {
        //        zoneareas.PrintToConsole(false, aHeading : $"ByZone {zoneareas.Dimensions}", aPrecis:4);
        //        _Areas.PrintToConsole(aHeading: $"BySG {_Areas.Dimensions}", aPrecis:4);
        //    }
        //    else
        //    {
        //        _Areas = zoneareas;
        //    }
   

        }

        /// <summary>
        /// executed internally to create the spout groups collection
        /// </summary>
        /// <param name="bGenerateSpouts"></param>
        private  bool _GeneratingSpouts = false;
        public colMDSpoutGroups GenerateSpoutGroups(bool bGenerateSpouts = true)
        {
            if (_GeneratingSpouts)
                return _SpoutGroups;

           
            int selID = _SpoutGroups != null ? _SpoutGroups.SelectedGroupIndex : 1;
            try
            {
               
                if (_SpoutGroups == null || _Downcomer == null) return null;
                if (_SpoutGroups.Invalid || SpoutAreaMatrices == null)
                {
                    if (Downcomer().Count <= 0)
                        return _SpoutGroups;

                    _GeneratingSpouts = true;
                    RaiseStatusChangeEvent($"Generating {TrayName()} Spout Groups");
                    eventGenerationEvent?.Invoke(uppTrayGenerationEventTypes.SpoutGroups, true, string.Empty);

                    List<mdDowncomer> DComers = Downcomers.GetByVirtual(aVirtualValue: false).FindAll((x) => x.Boxes.Count > 0); // just get the real ones
                    
                 
                    
                    if (_Constraints != null)
                    {
                        _Constraints.eventConstraintMemberChanged -= _Constraints_ConstraintMemberChanged;
                        _Constraints.eventConstraintsInvalidated -= _Constraints_ConstraintsInvalidated;
                    }

                    _Constraints = new colMDConstraints(_Constraints, this);
                    _Constraints.SuppressEvents = true;

                    SpoutAreaMatrices = new mdSpoutAreaMatrices(this, bGenerateSpouts);

                    // mdSpoutZones zones = SpoutAreaMatrices.Zones ;
                    //create/recreate the constraints
                    

                    int gidx = 0;
                  
                    double zvalue = Deck.Thickness + _Downcomer.How - _Downcomer.InsideHeight - 0.5 * _Downcomer.Thickness;
                    string tname = TrayName();

                    RaiseStatusChangeEvent($"Generating  {TrayName()} Spout Groups");
                    _SpoutGroups.Invalid = false;
                    _SpoutGroups.eventSpoutGroupsGenerationEvent -= _SpoutGroups_SpoutGroupsGenerationEvent;
                    _SpoutGroups.eventSpoutGroupsInvalidated -= _SpoutGroups_SpoutGroupsInvalidated;
                    colMDSpoutGroups oldgroups = null; 
                    if(_SpoutGroups != null)
                    {
                        oldgroups = _SpoutGroups;
                        oldgroups.SuppressEvents = true;
                    }
                    
                    _SpoutGroups = new colMDSpoutGroups();
                    _SpoutGroups.SubPart(this);
                    _SpoutGroups.SuppressEvents = true;
                    foreach (mdSpoutAreaMatrix matrix  in SpoutAreaMatrices)
                    {
                        mdSpoutZone zone = matrix.Zone;
                        //there should be one spout group per spout area
                        foreach (mdSpoutArea area in zone)
                        {
                            int dcidx = area.DowncomerIndex;
                            int dpidx = area.PanelIndex;

                            if (dcidx < 1 || dcidx > DComers.Count)
                                continue;

                            mdDowncomer DComer = DComers[dcidx - 1];
                            
                            mdDowncomerBox box = DComer.Boxes.Find((x) => !x.IsVirtual && x.Index == area.BoxIndex);
                            if (box == null)
                                continue;

                            RaiseStatusChangeEvent($"Creating {tname} Spout Group {dcidx},{dpidx},{box.Index}");

                            mdConstraint Cnstr = _Constraints.GetConstraint(dpidx, dcidx, box.Index);
                            if(Cnstr == null)
                            {
                                Cnstr = new mdConstraint(null, box, area.PanelIndex);
                                _Constraints.Add(Cnstr);
                            }
                            gidx = _SpoutGroups.ToList().FindAll((x) => x.PanelIndex == dpidx && x.DowncomerIndex == dcidx && x.BoxIndex == box.Index).Count + 1;
                            mdSpoutGroup sGroup  = new mdSpoutGroup(aBox: box, aConstraints: Cnstr, aSpoutArea: area, aGroupIndex: gidx)
                            {
                                SuppressEvents = true,
                                TheoreticalArea = area.IdealSpoutArea,
                                Z = zvalue
                            };
                           
                            uopVector scntr = sGroup.Perimeter.Center;
                            Cnstr.Instances = new uopInstances(area.Instances, Cnstr);

                            if (bGenerateSpouts)
                            {
                                RaiseStatusChangeEvent($"Creating {tname} Spout Group {dcidx},{dpidx},{box.Index} Spouts");
                                sGroup.RegenerateSpouts(this, false, true);
                            }
                            //if (Cnstr.PatternType == uppSpoutPatterns.SStar)
                            //{
                            //    //clear s* patterns for not triangular downcomers
                            //    Cnstr.PropValSet("PatternType", uppSpoutPatterns.Undefined, bSuppressEvnts: true);
                            //}
                            mdStartUps.UpdateSpoutObjects(sGroup, this);

                            _SpoutGroups.Add(sGroup);
                            sGroup.SuppressEvents = false;
                        }

                    }

             
                }
                




            }
            catch (Exception)
            {
                eventGenerationEvent?.Invoke(uppTrayGenerationEventTypes.SpoutGroups, false, string.Empty);
            }
            finally
            {


                if (_SpoutGroups != null)
                {

                    _SpoutGroups.Invalid = false;
                    _SpoutGroups.SelectedGroupIndex = selID;
                    eventGenerationEvent?.Invoke(uppTrayGenerationEventTypes.SpoutGroups, false, string.Empty);

        
                    _SpoutGroups.SubPart(this);
                    _SpoutGroups.eventSpoutGroupsGenerationEvent += _SpoutGroups_SpoutGroupsGenerationEvent;
                    _SpoutGroups.eventSpoutGroupsInvalidated += _SpoutGroups_SpoutGroupsInvalidated;
                    _SpoutGroups.SuppressEvents = false;
                }
                // save the current constraints
                
                if (_Constraints != null)
                {
                    _Constraints.SubPart(this);
                    _Constraints.eventConstraintMemberChanged += _Constraints_ConstraintMemberChanged;
                    _Constraints.eventConstraintsInvalidated += _Constraints_ConstraintsInvalidated;
                    _Constraints.SuppressEvents = false;

                }
                _GeneratingSpouts = false;

            }
            //if (bGenerateSpouts && !_GeneratingSpouts && _SpoutGroups != null && _Constraints != null)
            //    _SpoutGroups.GenerateSpouts(this, false, bSuppressEvnts:true);


            return _SpoutGroups;
        }

        /// <summary>
        /// executed internally to initialize the StartupSpouts collection
        /// </summary>
        public mdStartupSpouts GenerateStartupSpouts(bool bRegen = false)
        {
            if (Reading) return _StartupSpouts;

            _StartupSpouts.TargetArea = IdealStartupArea;
            if (this.StartupConfiguration != uppStartupSpoutConfigurations.None && _StartupSpouts.TotalCount <= 0) 
                bRegen = true;
            if (_StartupSpouts.Invalid || _StartupSpouts.Count <= 0 || bRegen)
            {
                eventGenerationEvent?.Invoke(uppTrayGenerationEventTypes.StartupSpouts, true, "");



                //get the best solution for the last configuration
                uppStartupSpoutConfigurations Config = StartupConfiguration;
                if (_StartupSpouts.Count == 0)
                {
                    _StartupSpouts = mdStartUps.Generate(this, IdealStartupArea, ref Config, OverrideStartupLength);
                }
                else
                {
                    _StartupSpouts = mdStartUps.Generate(this, IdealStartupArea, ref Config, OverrideStartupLength, _StartupSpouts);
                }

                _StartupSpouts.Invalid = false;
                _StartupSpouts.SubPart(this);
                _Downcomer.PropValSet("StartupDiameter", _StartupSpouts.Height, bSuppressEvnts: true);

                _Downcomer.PropValSet("StartupLength", _StartupSpouts.Length, bSuppressEvnts: true);

                Downcomers.SetStartupSpouts(_StartupSpouts);

                eventGenerationEvent?.Invoke(uppTrayGenerationEventTypes.StartupSpouts, false, "");
            }

            return _StartupSpouts;
        }

        public uopPart GetPart(uppPartTypes aPartType)
        {

            if (aPartType == uppPartTypes.Project) return GetMDProject();

            if (aPartType == uppPartTypes.Column) return base.Column;


            if (aPartType == uppPartTypes.TrayAssembly) return this;

            if (aPartType == uppPartTypes.TrayRange) return GetMDRange();

            if (aPartType == uppPartTypes.Deck) return Deck;

            if (aPartType == uppPartTypes.Downcomer) return Downcomer();
            if (aPartType == uppPartTypes.DesignOptions) return DesignOptions;

            return null;
        }
        /// <summary>
        /// returns the spout group by handle
        /// </summary>
        /// <param name="aHandle"></param>
        /// <returns></returns>
        public mdSpoutGroup GetSpoutGroup(string aHandle) => _SpoutGroups.GetByHandle(aHandle);

        public colDXFVectors GetStartupCenters() => Downcomers.StartupCenters;

        public override uopTrayAssembly GetTrayAssembly(uopTrayAssembly aAssy = null) => this;

        public uopTable GetTable(string aTableName, uppUnitFamilies aUnits, int MinRows = 0)
        {
            return mdTables.GetTable(this, aTableName, aUnits, MinRows, _Downcomers, _SpoutGroups, _DeckPanels);
        }

        public bool GetValidity(uppPartTypes aPartType)
        {
            if (aPartType == uppPartTypes.Downcomer)
            {
                if (_Downcomers != null) return !_Downcomers.Invalid;

            }
            else if (aPartType == uppPartTypes.DeckPanel)
            {
                if (_DeckPanels != null) return !_DeckPanels.Invalid;

            }
            else if (aPartType == uppPartTypes.SpoutGroup)
            {
                if (_SpoutGroups != null) return !_SpoutGroups.Invalid;

            }
            else if (aPartType == uppPartTypes.StartupSpout)
            {
                if (_StartupSpouts != null) return !_StartupSpouts.Invalid;

            }
            else if (aPartType == uppPartTypes.DeckSplice)
            {
                if (_DeckSplices != null) return !_DeckSplices.Invalid;

            }
            return false;
        }

    
        public bool EndAnglesAreEqual(mdTrayAssembly aAssy)
        {
            if (aAssy == null) return false;
            if (aAssy.Downcomer().Count != Downcomer().Count) return false;
            if (!TVALUES.CompareNumeric(aAssy.ShellID, ShellID, 3)) return false;
            if (!TVALUES.CompareNumeric(aAssy.RingWidth, RingWidth, 3)) return false;
            if (!TVALUES.CompareNumeric(aAssy.DowncomerSpacing, DowncomerSpacing, 3)) return false;
            if (!TVALUES.CompareNumeric(aAssy.Downcomer().Width, Downcomer().Width, 3)) return false;
            if (!TVALUES.CompareNumeric(aAssy.Downcomer().Thickness, Downcomer().Thickness, 3)) return false;
            return true;
        }

        public void Invalidate(uppPartTypes aPartType)
        {
            Message_PartsInvalidated msg = new Message_PartsInvalidated(ProjectHandle, RangeGUID);
            DowncomerDataSet dcdata = null;
            switch (aPartType)
            {
                case uppPartTypes.FlowSlotZone:
                case uppPartTypes.FlowSlot:
                    _SlotZones.Invalid = true;
                    break;
                case uppPartTypes.BubblePromoter:
                    _BPSites = null;
                    break;
                case uppPartTypes.APPan:
                    if (_APPans != null) _APPans.Invalid = true;
                    break;
                case uppPartTypes.Downcomer:
                case uppPartTypes.Downcomers:
                    if (_Downcomers != null)
                    {
                        _Downcomers.Invalid = true;
                        _Downcomers.Optimized = false;
                    }
                    msg.InvalidateAll = true;
                    _SlotZones.Invalid = true;
             
                    break;
                case uppPartTypes.ManwayAngle:
                case uppPartTypes.ManwaySplicePlate:
                case uppPartTypes.SpliceAngle:
                    if (_SpliceAngles != null) _SpliceAngles.Invalid = true;
                    break;
                case uppPartTypes.DeckPanel:
                case uppPartTypes.DeckPanels:
                    if (_DeckPanels != null) _DeckPanels.Invalid = true;
                    if (_DeckSections != null) _DeckSections.Invalid = true;
                    if(_Downcomers != null)
                    {
                       dcdata= _Downcomers._SpacingData != null ? _Downcomers._SpacingData.DowncomerData : null;
                        //if (dcdata != null)
                        //{
                        //    dcdata._PanelShapes = null;
                        //    dcdata._SectionShapes = null;
                        //}
                    }
                    
                    break;
                case uppPartTypes.DeckSplice:
                case uppPartTypes.DeckSplices:
                    if (_DeckSplices != null) _DeckSplices.Invalid = true;
                    if (_DeckSections != null) _DeckSections.Invalid = true;
                    if (_Downcomers != null)
                    {
                        dcdata = _Downcomers._SpacingData != null ? _Downcomers._SpacingData.DowncomerData : null;
                       
                    }
                    msg.PartTypes.Add(uppPartTypes.DeckSections);
                    msg.PartTypes.Add(uppPartTypes.SpliceAngle);
                    msg.PartTypes.Add(uppPartTypes.ManwayAngle);
                    msg.PartTypes.Add(uppPartTypes.ManwayClamp);
                    msg.PartTypes.Add(uppPartTypes.ManwayClip);
                    msg.PartTypes.Add(uppPartTypes.RingClip);
                    break;
                case uppPartTypes.DeckSection:
                case uppPartTypes.DeckSections:

                    msg.PartTypes.Add(uppPartTypes.DeckSections);
                    msg.PartTypes.Add(uppPartTypes.SpliceAngle);
                    msg.PartTypes.Add(uppPartTypes.ManwayAngle);
                    msg.PartTypes.Add(uppPartTypes.ManwayClamp);
                    msg.PartTypes.Add(uppPartTypes.ManwayClip);
                    msg.PartTypes.Add(uppPartTypes.RingClip);

                    if (_DeckSections != null) _DeckSections.Invalid = true;
                    _SlotZones.Invalid = true;
                    _BPSites = null;

                    if (_Downcomers != null)
                    {
                        dcdata = _Downcomers._SpacingData != null ? _Downcomers._SpacingData.DowncomerData : null;
                     
                    }
                    break;
                case uppPartTypes.StartupSpout:
                    if (_StartupSpouts != null) _StartupSpouts.Invalid = true;
                    break;
                case uppPartTypes.SpoutGroup:
                case uppPartTypes.SpoutGroups:
                    if (_SpoutGroups != null) _SpoutGroups.Invalid = true;
                    if (_StartupSpouts != null) _StartupSpouts.Invalid = true;
                    msg.PartTypes.Add(uppPartTypes.Downcomers);
                    msg.PartTypes.Add(uppPartTypes.DowncomerBox);
                    msg.PartTypes.Add(uppPartTypes.APPan);
                    break;



                case uppPartTypes.EndAngle:
                    break;

                default:
                    msg.PartTypes.Add(aPartType);
                    break;
            }

            if (ProjectType == uppProjectTypes.MDDraw)
            {
                if (!string.IsNullOrWhiteSpace(ProjectHandle) && !SuppressEvents)
                {
                    if (msg.PartTypes.Count > 0 || msg.InvalidateAll)
                        uopEvents.Aggregator.Publish<Message_PartsInvalidated>(msg);
                }
            }

        }

        public List<uopLinePair> LimitLines(int aDCIndex = 0) => DowncomerData.GetLimitLines(aDCIndex);

        /// <summary>
        /// the hole centers of the manway clips required to assembly a single tray.
        /// ~recalculated on each request.
        /// </summary>
        /// <param name="bBothSides"></param>
        /// <param name="bAsInstalled"></param>
        /// <returns></returns>
        public colDXFVectors ManwayFastenerCenters(bool bBothSides, bool bAsInstalled = false)
        {
            colDXFVectors _rVal = new colDXFVectors();

            uopPart aPart = null;
            colUOPParts Clps = ManwayFasteners(true, bBothSides);
            dxfVector v1 = null;
            uopManwayClip aClip = null;
            uopManwayClamp aClamp = null;
            bool bClips = DesignOptions.UseManwayClips;

            for (int i = 1; i <= Clps.Count; i++)
            {
                aPart = Clps.Item(i);

                if (bClips)
                {
                    aClip = (uopManwayClip)aPart;
                    v1 = aClip.CenterDXF.Clone();
                    v1.Z = 0;
                    v1.Rotation = aClip.Angle;
                    v1.Value = v1.Rotation;
                }
                else
                {
                    aClamp = (uopManwayClamp)aPart;
                    v1 = aClamp.CenterDXF.Clone();
                    v1.Z = 0;
                    if (aClamp.BottomSide)
                    {
                        //todo goto NextClip;
                    }
                    v1.Rotation = aClamp.Angle;
                    v1.Value = v1.Rotation;
                }
                v1.Flag = aPart.Flag;


                _rVal.Add(v1);

                //  NextClip:
            }
            return _rVal;
        }
        /// <summary>
        /// the manway clips or clamps in the assembly
        /// </summary>
        /// <param name="aTopSideOnly"></param>
        /// <param name="aBothSides"></param>
        /// <returns></returns>
        public colUOPParts ManwayFasteners(bool aTopSideOnly, bool aBothSides) => DeckSections.ManwayFasteners(this, aTopSideOnly, aBothSides);

        /// <summary>
        /// returns the maximum spout error
        /// ~the greatest error form the spout groups, downcomers, and panels
        /// </summary>
        /// <param name="aDescriptor"></param>
        /// <returns></returns>
        public double MaximumSpoutError(out string aDescriptor)
        {

            double e1 = 0;
            double e2 = 0;
            double e3 = 0;
            double e0 = 0;
            string t1 = string.Empty;
            string t2 = string.Empty;
            string t3 = string.Empty;
            string t0 = string.Empty;
            mdDowncomer aDC = null;
            mdDeckPanel aDP = null;
            mdSpoutGroup aSG = null;

            for (int i = 1; i <= Downcomers.Count; i++)
            {
                aDC = Downcomers.Item(i);
                e0 = aDC.ErrorPercentage(this);
                if (Math.Abs(e0) > Math.Abs(e1))
                {
                    e1 = e0;
                    t1 = "DC " + i;
                }
            }
            for (int i = 1; i <= DeckPanels.Count; i++)
            {
                aDP = DeckPanels.Item(i);
                e0 = aDP.ErrorPercentage(this);
                if (Math.Abs(e0) > Math.Abs(e2))
                {
                    e2 = e0;
                    t2 = "DP " + i;
                }
            }
            for (int i = 1; i <= SpoutGroups.Count; i++)
            {
                aSG = SpoutGroups.Item(i);
                e0 = aSG.ErrorPercentage;
                if (Math.Abs(e0) > Math.Abs(e3))
                {
                    e3 = e0;
                    t3 = "SG " + aSG.Handle;
                }
            }
            e0 = e1;
            t0 = t1;
            if (Math.Abs(e2) > Math.Abs(e0))
            {
                e0 = e2;
                t0 = t2;
            }
            if (Math.Abs(e3) > Math.Abs(e0))
            {
                e0 = e3;
                t0 = t3;
            }

            aDescriptor = t0;
            return e0;
        }

        /// <summary>
        /// the design gap between a deck panel edge and its supporting downcomers
        /// ~default = 0.0825
        /// </summary>
        /// <param name="bIncludeAdditionalTrim"></param>
        /// <returns></returns>
        public double PanelClearance(bool bIncludeAdditionalTrim = false)
        {
            double _rVal = mdGlobals.DefaultPanelClearance;
            if (bIncludeAdditionalTrim && ProjectType == uppProjectTypes.MDDraw)
            {
                if (DesignOptions.SpliceStyle == uppSpliceStyles.Tabs)
                {
                    if (_Downcomers.HasFoldovers) _rVal += mdGlobals.FolderWeirPanelClearanceAdder;

                }
            }

            return _rVal;
        }

        /// <summary>
        /// returns a collection whose memers are longs and are the number of flow slots
        /// ^required on each panel
        /// the slot zones of all the panels in the collection
        /// #1 returns the required numer of flow slots for the whole assembly
        /// </summary>
        /// <param name="rTotalRequired"></param>
        /// <param name="rSlotArea"></param>
        /// <param name="rFunctionActiveArea"></param>
        /// <param name="rFs"></param>
        /// <returns></returns>
        public List<object> PanelSlotCounts(out int rTotalRequired, out double rSlotArea, out double rFunctionActiveArea, out double rFs)
        {
            List<object> _rVal = new List<object>();

            colMDDeckPanels DPs = DeckPanels;
            mdDeckPanel aDP;
            int pcnt;
            int pTot = 0;
            double FBATot = FreeBubblingAreas.TotalFreeBubblingArea;
            double pFBA;

            rTotalRequired = 0;
            rSlotArea = 0;
            rFs = 0;
            rFunctionActiveArea = FunctionalActiveArea;

            if (DesignFamily.IsEcmdDesignFamily())
            {
                rSlotArea = Deck.SlotArea;
                rFs = Deck.SlottingPercentage;
                if (rSlotArea > 0) rTotalRequired = mzUtils.VarToInteger(rFunctionActiveArea * (rFs / 100) / rSlotArea);

            }



            for (int i = 1; i <= DPs.Count; i++)
            {

                aDP = DPs.Item(i);
                pFBA = aDP.FreeBubblingArea(this);
                pcnt = (FBATot > 0) ? mzUtils.VarToInteger(pFBA / FBATot * rTotalRequired) : 0;

                _rVal.Add(pcnt);
                pTot += pcnt * aDP.OccuranceFactor;
            }
            rTotalRequired = pTot;
            return _rVal;
        }

        /// <summary>
        /// the physical width of the deck sections
        /// </summary>
        /// <param name="bIncludePanelClearance"></param>
        /// <returns></returns>
        public double DeckSectionWidth(bool bIncludePanelClearance)
        {
            double _rVal;
            mdDowncomer aDC = Downcomer();
            colMDDowncomers DComers = Downcomers;

            if (aDC.Count > 1)
            {
                _rVal = DComers.Spacing - aDC.BoxWidth;
                if (bIncludePanelClearance) _rVal -= 2 * PanelClearance(true);

            }
            else
            {
                _rVal = DeckRadius - (aDC.BoxWidth * 0.5);
                if (bIncludePanelClearance) _rVal -= PanelClearance(true);

            }
            return _rVal;
        }

        public dxfRectangle Rectangle(double aScaler = 0)
        {
            if (aScaler <= 0) aScaler = 1;
            double wd = ShellID * aScaler;
            return new dxfRectangle(wd, wd);
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
        public override void ReadProperties(uopProject aProject, uopPropertyArray aFileProps, ref uopDocuments ioWarnings, double aFileVersion, string aFileSection = null, bool bIgnoreNotFound = false, uopTrayAssembly aAssy = null, uopPart aPartParameter = null, List<string> aSkipList = null, Dictionary<string, string> EqualNames = null, uopProperties aProperties = null)
        {

            try
            {
                ioWarnings ??= new uopDocuments();
                aFileSection = string.IsNullOrWhiteSpace(aFileSection) ? INIPath : aFileSection.Trim();
                if (aFileProps == null) throw new Exception("The Passed File Property Array is Undefined");
                if (string.IsNullOrWhiteSpace(aFileSection)) throw new Exception("File Section is Undefined");
                if (aProject != null) SubPart(aProject);
                if (_Beam == null)
                {
                    _Beam = new mdBeam();
                    _Beam.eventMDBeamPropertyChange += SubPartPropertyChangeEventHandler;
                }

                uopPropertyArray myprops = aFileProps.PropertiesStartingWith(aFileSection);
                if (myprops.Count <= 0)
                {
                    ioWarnings?.AddWarning(this, $"{PartName} Data Not Found", $"File '{aFileProps.Name}' Does Not Contain {aFileSection} Info!");
                    return;
                }

             
                base.ReadProperties(aProject, myprops, ref ioWarnings, aFileVersion, aFileSection, bIgnoreNotFound, aSkipList: aSkipList, EqualNames: EqualNames);
                Reading = true;

                uppMDDesigns family = DesignFamily;

                //create the downcomers collection by reading the differences from the file

                _Downcomers.Clear();
                _DeckPanels.Clear();

                _Downcomers.SubPart(this);

                string sname = TrayName(true);
                bool bGenSpouts = false;
                bool found = false;
                double ovrVal = 0;
                string txt = string.Empty;

                ResetComponents();

                aProject?.ReadStatus($"Reading {sname} Tray Assembly Properties");
                //get the deck object properties
                string fsec = $"{aFileSection}.DECK";
                aProject?.ReadStatus($"Reading {sname} Deck Properties");



                if (!myprops.Contains(fsec))
                    ioWarnings.AddWarning(this, "File Section Not Found", $"Tray Assembly Data For {fsec} Was Not Found In File '{aFileProps.Name}'");
                else
                    Deck.ReadProperties(aProject, myprops, ref ioWarnings, aFileVersion, fsec, bIgnoreNotFound, aSkipList: new List<string> { "MAxVLError" });


                //must follow deck
                fsec = $"{aFileSection}.DESIGNOPTIONS";
                if (!myprops.Contains(fsec))
                    ioWarnings.AddWarning(this, "File Section Not Found", $"Tray Assembly Data For {fsec} Was Not Found In File '{aFileProps.Name}'");
                else
                    DesignOptions.ReadProperties(aProject, myprops, ref ioWarnings, aFileVersion, fsec, bIgnoreNotFound, aAssy: this);

                //must preceed downcomers
                if (family.IsBeamDesignFamily())
                {
                    fsec = $"{aFileSection}.BEAM";
                    if (!myprops.Contains(fsec))
                        ioWarnings.AddWarning(this, "File Section Not Found", $"Tray Assembly Data For {fsec} Was Not Found In File '{aFileProps.Name}'");
                    else 
                    {
                       
                        _Beam.SubPart(this,aDCSpace: PropValD("Spacing"));
                        _Beam.ReadProperties(aProject, myprops, ref ioWarnings, aFileVersion, fsec, bIgnoreNotFound, aAssy: this);

                    }
                    
                }

                //get the basic downcomer properties
                mdDowncomer globaldc = Downcomer();
                fsec = $"{aFileSection}.DOWNCOMER";

                if (!myprops.Contains(fsec))
                {
                    ioWarnings.AddWarning(this, "File Section Not Found", $"Tray Assembly Data For {fsec} Was Not Found In File '{aFileProps.Name}'");
                }
                else
                {
                    globaldc.ReadProperties(aProject, myprops, ref ioWarnings, aFileVersion, fsec, bIgnoreNotFound, aAssy: this);
                    if (globaldc.Count <= 0)
                        ioWarnings?.AddWarning(globaldc, "Downcomer Read Error", "O Downcomer Count Detected");
                }

                uppStartupSpoutConfigurations suConfig = uppStartupSpoutConfigurations.TwoByFour;
                fsec = $"{aFileSection}.DOWNCOMERS";
                mdSpacingData solution = null;
                if (!myprops.Contains(fsec))
                {
                    ioWarnings.AddWarning(this, "File Section Not Found", $"Tray Assembly Data For {fsec} Was Not Found In File '{aFileProps.Name}'");
                    _Downcomers.Initialize(this, SetSpacing: false);
                    bGenSpouts = true;
                    Invalidate(uppPartTypes.Downcomer);
                    _Downcomers.OptimumSpacing = 0;
                    _Downcomers.OverrideSpacing = 0;
                    Reading = false;
                    GenerateDowncomers();
                    solution = Downcomers.SpacingData;
                    Reading = true;

                }
                else
                {
                    aProject?.ReadStatus($"Reading {sname} Downcomer Member Properties");
                    _Downcomers.Initialize(this, false);
                    _Downcomers.ReadProperties(aProject, myprops, ref ioWarnings, aFileVersion, fsec, bIgnoreNotFound, aAssy: this);
                    _Downcomers.Invalid = false;
                    _Downcomers.SubPart(this);
            

                    if (family.IsStandardDesignFamily())
                    {

                        aProject?.ReadStatus($"Confirming {sname} Downcomer Spacing");
                        double space = _Downcomers.Spacing;

                        solution = mdSpacingSolutions.OptimizedSpace(this, out _, aStartValue: family.IsStandardDesignFamily() ? space : 0);
                        double optVal = solution.Spacing;
                        if (Math.Round(Math.Abs(optVal - space), 3) > 0)
                        {
                            txt = $"The Saved Downcomer Spacing {space:0.000#}";
                            txt += "'' Is Not An Override And Does Not Match The Calculated Optimum Spacing Value ";
                            txt += $"{optVal:0.000#}''.";
                            txt += "The Saved Spacing Has Been Saved As An Override.";
                            ioWarnings.AddWarning(this, "DC Spacing Warning", txt);
                            ovrVal = space;
                        }
                        _Downcomers.PropValSet("OptimumSpacing", optVal, bSuppressEvnts: true);
                        _Downcomers.PropValSet("OverrideSpacing", ovrVal, bSuppressEvnts: true);
                        PropValSet("OverrideSpacing", ovrVal, bSuppressEvnts: true);

                        solution = new mdSpacingData(this, space, _Downcomers.Offset);
                        _Downcomers.SetSpacingData(this, solution);

                    }
                    else
                    {
                        // non-standard trays can not be overridden
                        aProject?.ReadStatus($"Optimizing {sname} Downcomer Spacing");
                        _Downcomers.OptimizeSpacing(this, bSuppressEvents: true);
                    }
           
                }
                //create the deck panels
                aProject?.ReadStatus($"Generating {sname} Deck Panels");
                GenerateDeckPanels(true);
                ////get the spout group constraints that differ from default
                fsec = $"{aFileSection}.CONSTRAINTS";
                if (_Constraints != null)
                {
                    _Constraints.eventConstraintMemberChanged -= _Constraints_ConstraintMemberChanged;
                    _Constraints.eventConstraintsInvalidated -= _Constraints_ConstraintsInvalidated;

                }
                _Constraints = new colMDConstraints();
                _Constraints.eventConstraintMemberChanged += _Constraints_ConstraintMemberChanged;
                _Constraints.eventConstraintsInvalidated += _Constraints_ConstraintsInvalidated;

                aProject?.ReadStatus($"Initializing {sname} Constraints");
                _Constraints.Populate(this);
                aProject?.ReadStatus($"Reading {sname} Constraints Non-Default Properties");
                try
                {
                    _Constraints.ReadProperties(aProject, myprops, ref ioWarnings, aFileVersion, fsec, bIgnoreNotFound, aAssy: this);
                }
                catch (Exception e) { ioWarnings.AddWarning(this, "Read Error Occured", e.Message); }


                aProject?.ReadStatus("Done");

                //create the spout groups
                aProject?.ReadStatus($"Initializing {sname} Spout Groups");
                if (_SpoutGroups != null)
                {
                    // _SpoutGroups.eventSpoutGroupMemberChanged -= _SpoutGroups_SpoutGroupMemberChanged;
                    _SpoutGroups.eventSpoutGroupsGenerationEvent -= _SpoutGroups_SpoutGroupsGenerationEvent;
                    _SpoutGroups.eventSpoutGroupsInvalidated -= _SpoutGroups_SpoutGroupsInvalidated;

                }
                _SpoutGroups = new colMDSpoutGroups();
                // _SpoutGroups.eventSpoutGroupMemberChanged += _SpoutGroups_SpoutGroupMemberChanged;
                _SpoutGroups.eventSpoutGroupsGenerationEvent += _SpoutGroups_SpoutGroupsGenerationEvent;
                _SpoutGroups.eventSpoutGroupsInvalidated += _SpoutGroups_SpoutGroupsInvalidated;

                fsec = $"{aFileSection}.SpoutGroups";
                if (!myprops.Contains(fsec))
                {
                    bGenSpouts = true;
                    if(!family.IsStandardDesignFamily())  ioWarnings.AddWarning(this, "File Section Not Found", $"Tray Assembly Data For {fsec} Was Not Found In File '{aFileProps.Name}'");
                }

                if (!bGenSpouts)
                {
                    GenerateSpoutGroups(bGenerateSpouts: false);
                    _SpoutGroups.ReadProperties(aProject, aFileProps, ref ioWarnings, aFileVersion, fsec, aAssy: this, bRegenOnly: true); // !this.DesignFamily.IsStandardDesignFamily());
                    _SpoutGroups.Invalid = false;
                }
                else
                {
                    aProject?.ReadStatus($"Generating {sname} Spout Groups");
                    Invalidate(uppPartTypes.SpoutGroup);
                    GenerateSpoutGroups(bGenerateSpouts:true);
                }

              //to assign the ideals
                SpoutAreaMatrices.DistributeSpoutArea(this, _SpoutGroups, bVerbose: false);

                //create the startup spouts
                aProject?.ReadStatus($"Generating {sname} Startup Spouts");

                if (aFileVersion >= 1.72)
                {
                    _StartupSpouts = mdStartUps.ByLocation(this);

                }
                else
                {
                    suConfig = (uppStartupSpoutConfigurations)aFileProps.ValueI($"{aFileSection}.Downcomer", "StartupConfiguration", out found, (int)uppStartupSpoutConfigurations.None);
                    _StartupSpouts = mdStartUps.Generate(this, IdealStartupArea, ref suConfig, 0);
                    PropValSet("StartupConfiguration", (int)suConfig, bSuppressEvnts: true);
                }
                _Downcomers.SetStartupSpouts(_StartupSpouts);
                _StartupSpouts.Invalid = false;

                if (_DeckSections != null)
                {
                    _DeckSections.eventDeckSectionsInvalidated -= _DeckSections_DeckSectionsInvalidated;
                }
                _DeckSections = new mdDeckSections();
                _DeckSections.eventDeckSectionsInvalidated += _DeckSections_DeckSectionsInvalidated;

                _DeckSections.SubPart(this);
                _DeckSections.Invalid = false;

                if (aProject.ProjectType == uppProjectTypes.MDDraw)
                {
                    //read the splices collection
                    fsec = $"{aFileSection}.DeckSplices";

                  
                  
                    _DeckSplices = new uopDeckSplices(this);
                    _DeckSplices.ReadProperties(aProject, aFileProps, ref ioWarnings, aFileVersion, fsec, aAssy: this, aPartParameter: _DeckPanels);
                    GenerateDeckSections(bRegen: true);

                }

                PropValSet("Spacing", _Downcomers.Spacing, bSuppressEvnts: true);
                PropValSet("OverrideSpacing", ovrVal, bSuppressEvnts: true);
                _Downcomers.PropValSet("OverrideSpacing", ovrVal, bSuppressEvnts: true);
                _SpoutGroups.SetSelected(1);
                _Downcomers.FetchSpacingData(this);

                if (aProject.ProjectType == uppProjectTypes.MDDraw)
                {
                    //initialize the zones

                    aProject?.ReadStatus($"Creating {sname} Slot Zones");
                    GenerateSlotZones(true);

                    if (family.IsEcmdDesignFamily())
                    {
                        //read the slot zone info
                        fsec = $"{aFileSection}.SlotZones";
                        if (myprops.Contains(fsec))
                            _SlotZones.ReadProperties(aProject, aFileProps, ref ioWarnings, aFileVersion, fsec, aAssy: this);
                        
                    }
                }
               
                _SlotZones.Invalid = false;
                _StartupSpouts.Invalid = false;
                if (aProject.ProjectType == uppProjectTypes.MDDraw) mdSlotting.SetSlotCounts(this, _SlotZones, FunctionalActiveArea);

                
            }
            catch (Exception e)
            {
                ioWarnings?.AddWarning(this, "Read Properties Error", e.Message);
            }
            finally
            {
                Reading = false;
                SuppressEvents = false;
                aProject?.ReadStatus("", 2);
            }
        }

        /// <summary>
        /// can be executed externally to force a spout group recalculation on the next request for them
        /// </summary>
        public void RecalculateSpoutGroups()
        {
            SpoutAreaMatrices = null;
            Invalidate(uppPartTypes.SpoutGroup);
            //Invalidate(uppPartTypes.DeckPanel);
            if (!SuppressEvents)
            {
                PropertyChangeEvent?.Invoke(uopProperty.Quick("SpoutGroups", "something", "nothing", this));
            }

            eventMDAssemblyPartsInvalidated?.Invoke();
        }
        /// <summary>
        /// #1the tray assembly to generate startup spouts for
        /// #2the aTarget area for the total startup spouts area
        /// #3the previous set of spouts

        /// ^re-creates the startup spouts for the passed tray assembly based on the suppression and properties of the passed existing set of startups
        /// </summary>
        /// <returns></returns>
        public mdStartupSpouts RegenerateStartupSpouts()
        {
            mdStartupSpouts regenerateStartupSpouts = null;
            uppStartupSpoutConfigurations Config = StartupConfiguration;
            _StartupSpouts = mdStartUps.Generate(this, IdealStartupArea, ref Config, OverrideStartupLength, _StartupSpouts);
            StartupConfiguration = Config;
            _StartupSpouts.Invalid = false;
            return regenerateStartupSpouts;
        }

        /// <summary>
        /// ^used to reset all of the dynamic components back to Nothing so they will
        /// ^be recreated on the next request for them.
        /// ~this should be executed every time any of the user controled design data changes.
        /// ~executing this routine will ensure that all components are configured based on the current design data.
        /// </summary>
        /// <param name="bResetDowncomers"></param>
        public void ResetComponents(bool bResetDowncomers = true)
        {
            SpoutAreaMatrices = null;

            if (bResetDowncomers)
            {
                Invalidate(uppPartTypes.Downcomer);

            }

            Invalidate(uppPartTypes.DeckPanel);
            Invalidate(uppPartTypes.SpoutGroup);
            Invalidate(uppPartTypes.DeckSplice);
            Invalidate(uppPartTypes.APPan);
            Invalidate(uppPartTypes.FlowSlotZone);
            ResetSubComponents();
        }

        void ResetOverride(double newOverride) => PropValSet("OverrideSpacing", Math.Abs(newOverride), bSuppressEvnts: true);


        public void ResetParts()
        {
            ResetSubComponents();
        }
        /// <summary>
        /// executes to rest the slot zone slots to empty
        /// </summary>
        public void ResetSlots()
        {
            Invalidate(uppPartTypes.FlowSlotZone);
        }

        /// <summary>
        /// ^Executed to force a resh of the downcomers sub components
        /// </summary>
        public void ResetSubComponents()
        {
            Invalidate(uppPartTypes.BubblePromoter);
            Invalidate(uppPartTypes.SpliceAngle);
            Invalidate(uppPartTypes.APPan);
            Invalidate(uppPartTypes.EndAngle);
        }

        /// <summary>
        /// the ring clip used to attach the assembly to the support ring and bolting bar
        /// ~size and material based on current assembly properties.
        /// </summary>
        /// <param name="bForEndSupports"></param>
        /// <returns></returns>
        public uopRingClip RingClip(bool bForEndSupports = false)
        {
            uopRingClip _rVal = new uopRingClip();
            _rVal.SubPart(this); //must be first

            //setting the size sets the dimensions
            _rVal.ForEndSupport = bForEndSupports;
            _rVal.Size = RingClipSize;

            _rVal.Material = uopGlobals.goSheetMetalOptions().GetByFamilyAndGauge(uopEventHandler.RetrieveFirstDeckMaterial(ProjectHandle).Family, uppSheetGages.Gage12);

            if (bForEndSupports)
            {
                _rVal.Category = "Downcomer To Ring";
                _rVal.PartNumber = (_rVal.Size == uppRingClipSizes.FourInchRC) ? "45" : "30";

            }
            else
            {
                _rVal.Category = "Deck Section To Ring";
                _rVal.PartNumber = (_rVal.Size == uppRingClipSizes.FourInchRC) ? "40" : "30";

            }

            if (_rVal.Size == uppRingClipSizes.ThreeInchRC)
            {
                if (Math.Round(PropValD("RingThk", uppPartTypes.TrayRange), 5) >= 0.625) _rVal.PartNumber = "32";

            }
            else
            {
                if (Math.Round(PropValD("RingThk", uppPartTypes.TrayRange), 5) >= 0.75) _rVal.PartNumber = "41";

            }
            return _rVal;
        }
        /// <summary>
        /// the ring clip used to attach the assembly to the support ring and bolting bar
        /// ~size and material based on current assembly properties.
        /// </summary>
        /// <returns></returns>
        public colUOPParts RingClips(bool bForEndSupports = false)
        {
            colUOPParts _rVal = new colUOPParts();
            colDXFVectors aPts;
            uopRingClip aRC = RingClip(bForEndSupports);
            uopRingClip bRC;

            dxfVector v1;


            _rVal.SubPart(this);

            if (aRC.Size == uppRingClipSizes.ThreeInchRC)
            {
                aPts = RingClipCenters(true);
                for (int i = 1; i <= aPts.Count; i++)
                {
                    v1 = aPts.Item(i);
                    bRC = aRC.Clone();
                    bRC.X = v1.X;
                    bRC.Y = v1.Y;
                    _rVal.Add(bRC);
                }
            }
            else
            {
                aPts = RingClipCenters(true, true);
                for (int i = 1; i <= aPts.Count; i++)
                {
                    v1 = aPts.Item(i);
                    bRC = aRC.Clone();
                    bRC.X = v1.X;
                    bRC.Y = v1.Y;
                    _rVal.Add(bRC);
                }
                aRC = RingClip(true);
                aPts = RingClipCenters(true, false, true);
                for (int i = 1; i <= aPts.Count; i++)
                {
                    v1 = aPts.Item(i);
                    bRC = aRC.Clone();
                    bRC.X = v1.X;
                    bRC.Y = v1.Y;
                    _rVal.Add(bRC);
                }
            }


            return _rVal;
        }

        public void GetRingClipCenters(bool bBothSides, bool bExcludeDowncomers, bool bExcludeDeckSections, out colDXFVectors rDowncomerPts, out colDXFVectors rDeckPts)
        {
            rDowncomerPts = new colDXFVectors();
            rDeckPts = new colDXFVectors();
            if (!bExcludeDowncomers)
            {
                Downcomers.RingClipCenters(this, bBothSides, rDowncomerPts);
            }
            if (!bExcludeDeckSections)
            {
                DeckSections.RingClipCenters(this, bBothSides, rDeckPts);
            }

        }


        public colDXFVectors RingClipCenters(bool bBothSides, bool bExcludeDowncomers = false, bool bExcludeDeckSections = false)
        {
            colDXFVectors _rVal = new colDXFVectors();
            if (!bExcludeDowncomers)
            {
                Downcomers.RingClipCenters(this, bBothSides, _rVal);
            }
            if (!bExcludeDeckSections)
            {
                DeckSections.RingClipCenters(this, bBothSides, _rVal);
            }

            return _rVal;
        }
    
        
        public bool SaveSpoutAreaFlags(mdSpoutAreaMatrices aMatrices)
        {
            
            if (aMatrices == null) return false;

            bool _rVal = false;
            List<mdSpoutGroup> sGroups = SpoutGroups.GetByVirtual(aVirtualValue:false);

            foreach(var matrix in aMatrices)
            {
                mdSpoutZone zone = matrix.Zone;
                foreach(var sa in zone)
                {
                    mdSpoutGroup sg = sGroups.Find((x) => x.PanelIndex == sa.PanelIndex && x.DowncomerIndex == sa.DowncomerIndex && x.BoxIndex == sa.BoxIndex);
                    if (sg != null) 
                    {
                        bool sgchange = sg.SpoutArea.Copy(sa);
                        mdConstraint cnst = sg.Constraints(this);
                        uopProperty prop = cnst.PropValSet("TreatAsIdeal", sa.TreatAsIdeal, bSuppressEvnts: true);
                        if (prop != null) { sgchange = true; Notify(prop); }
                        prop = cnst.PropValSet("OverrideSpoutArea", sa.TreatAsIdeal ? sa.OverrideSpoutArea : 0, bSuppressEvnts: true);
                        if (prop != null) { sgchange = true; Notify(prop); }
                        prop = cnst.PropValSet("AreaGroupIndex", sa.GroupIndex, bSuppressEvnts: true);
                        if (prop != null) { sgchange = true; Notify(prop); }

                        if (sgchange) 
                        {
                            sg.ResetSpouts();
                            _rVal = true;
                        }

                    }
                }
            }
            
       

            if (_rVal) 
                RecalculateSpoutGroups();

            return _rVal;
        }
        /// <summary>
        /// used by friends to set the deck panels collection of the assembly
        /// </summary>
        /// <param name="newobj"></param>
        void SetDeckPanels(colMDDeckPanels newobj)
        {
            if (newobj != null)
            {
                _DeckPanels = newobj;
                _DeckPanels.Invalid = false;
            }
            else
            {
                _DeckPanels.Clear();
                Invalidate(uppPartTypes.DeckPanel);
            }


        }

        public void SetStiffeners(colUOPParts aStiffeners, double aSpace, bool bSuppressEvnts = false)
        {
            if (aStiffeners == null) return;
            List<mdDowncomer> DComers = Downcomers.GetByVirtual(aVirtualValue: false);

            bool bChng = false;
            for (int i = 1; i <= DComers.Count; i++)
            {
                mdDowncomer DComer = DComers[i - 1];
                colUOPParts Stiffs = new colUOPParts(uppPartTypes.Stiffener, aStiffeners.GetByDowncomerIndex(i));
                mdUtils.StiffenersSort(Stiffs, this);


                if (DComer.PropValSet("StiffenerSites", Stiffs.Centers().GetOrdinateList(dxxOrdinateDescriptors.Y, aPrecision: 6), bSuppressEvnts: bSuppressEvnts) != null)
                {
                    if (bSuppressEvnts) DComer.Boxes = null;
                    bChng = true;
                }

                //DComer.CurrentProperties.PrintToConsole();
            }


            if (_DesignOptions.PropValSet("StiffenerSpacing", aSpace, bSuppressEvnts: bSuppressEvnts) != null)
            {
                bChng = true;
            }

            if (bChng && !bSuppressEvnts)
            {
                Notify(uopProperty.Quick("Stiffeners", "New Stiffeners", "Old Stiffeners", this));
            }


        }

       

        /// <summary>
        /// the splice angle used to splice two sections of deck panel together.
        /// ~this object is used as a template for the creation of all the required splice angles in
        /// ~the assembly.
        /// </summary>
        /// <param name="aType"></param>
        /// <returns></returns>
        public mdSpliceAngle SpliceAngle(uppSpliceAngleTypes aType, double aX = 0, double aY = 0)
        {


            double lng = SpliceFlangeLength(out int bcnt);
            uopSheetMetal mtrl = uopEventHandler.RetrieveFirstDeckMaterial(ProjectHandle);

            mdSpliceAngle _rVal = new mdSpliceAngle(aType)
            {
                SheetMetalStructure = mtrl.Structure,
                Width = mdGlobals.SpliceAngleWidth,
                Height = DesignOptions.SpliceAngle,
                Length = lng,
                BoltCount = bcnt,
                SparePercentage = 2,
                JoggleDepth = _Deck.Thickness,
                X = aX,
                Y = aY,
                ParentPartType = uppPartTypes.TrayAssembly,
            };

            _rVal.SubPart(this);
            return _rVal;
        }


        public double SpliceFlangeLength(out int rBoltCount)
        {
            double _rVal = Downcomers.Spacing - Downcomers.MaxBoxWidth - 2 * Downcomer().ShelfWidth;
            _rVal -= 0.375; //3/16th on both sides
            rBoltCount = mdUtils.SpliceBoltCount(Downcomers.Spacing - Downcomers.MaxBoxWidth - 2 * Downcomer().ShelfWidth, Deck.Thickness,0); // 3 / 16th on both sides

            return _rVal;
        }
        /// <summary>
        /// the collection of splice angles required for the assembly
        /// </summary>
        /// <param name="bTrayWide"></param>
        /// <returns></returns>
        public colUOPParts SpliceAngles(bool bTrayWide = false)
        {

            _SpliceAngles ??= new colUOPParts();
            if (bTrayWide)
            {
                return GenerateSpliceAngles(true);
            }
            else
            {
                if (_SpliceAngles.Invalid)
                {
                    _SpliceAngles.Clear();
                    _SpliceAngles = this.GenerateSpliceAngles(false);
                    _SpliceAngles.Invalid = false;
                }

                return _SpliceAngles;
            }


        }

        /// <summary>
        /// the startup spout area defined for the assembly
        /// </summary>
        /// <param name="bSingleSpout"></param>
        /// <param name="aSUs"></param>
        /// <returns></returns>
        public double StartupArea(bool bSingleSpout, mdStartupSpouts aSUs)
        {
            double _rVal = 0;
            if (aSUs == null)
            {
                aSUs = StartupSpouts;
            }
            if (aSUs.Count > 0)
            {
                if (bSingleSpout)
                { _rVal = aSUs.Item(1).Area; }
                else
                { _rVal = aSUs.TotalArea; }
            }
            return _rVal;
        }

        /// <summary>
        /// the basic stiffener for the assembly
        /// ~used as the template for the creation of the stiffeners collection
        /// </summary>
        /// <param name="aDC"></param>
        /// <param name="aYOrd"></param>
        /// <param name="aMaxYOrd"></param>
        /// <returns></returns>
        public mdStiffener Stiffener(mdDowncomer aDC = null, double aYOrd = 0, double aMaxYOrd = 0) => aDC == null ? Downcomer().Stiffener(aYOrd) : aDC.Stiffener(aYOrd);



        /// <summary>
        /// the collection of stiffereners defined for the tray
        /// </summary>
        /// <returns></returns>
        public List<mdStiffener> Stiffeners() => mdPartGenerator.Stiffeners_ASSY(this, false);


        /// <summary>
        /// the number of flow slots required for the entire tray
        /// ~(function area X Fs)/ slot area
        /// ~only applicable for ECMD trays
        /// </summary>
        /// <returns></returns>
        public int TotalRequiredSlotCount() => DesignFamily.IsEcmdDesignFamily() ? mdSlotting.TotalRequiredSlotCount(Deck.SlotType, Deck.SlottingPercentage, FunctionalActiveArea) : 0;

        public override void UpdatePartProperties() { PropValSet("TheoreticalSpoutArea", _Downcomer.ASP, bSuppressEvnts: true); }


        public override void UpdatePartWeight() { colUOPParts myParts = Parts; myParts.UpdatePartWeight(); base.Weight = myParts.Weight; }

        /// <summary>
        /// returns a collection of strings that are warnings about possible problems with
        /// ^the current tray assembly design.
        /// ~these warnings may or may not be fatal problems.
        /// </summary>
        /// <param name="aProject"></param>
        /// <param name="aCollector"></param>
        /// <param name="aCategory"></param>
        /// /// <param name="bJustOne"></param>
        public override uopDocuments GenerateWarnings(mdProject aProject, uopDocuments aCollector = null, string aCategory = "", bool bJustOne = false)
        {
            uopDocuments _rVal = aCollector ?? new uopDocuments();
            if (bJustOne && _rVal.Count > 0) return _rVal;

            aProject ??= GetMDProject(aProject);

            if (aProject == null) return _rVal;
            string txt;
            double lim1;
            double lim2;
            string sname = TrayName(true);
            string basestatus = $"Generating {sname} Warnings";
            aProject.ReadStatus(basestatus, 1);

            aCategory = string.IsNullOrWhiteSpace(aCategory) ? TrayName() : aCategory.Trim();

            if (string.IsNullOrWhiteSpace(aCategory)) aCategory = TrayName(true);



            if (ProjectType == uppProjectTypes.MDSpout)
            {
                aProject.ReadStatus($"{basestatus} - Spout Area Check", 1);

                lim1 = aProject.ConvergenceLimit;
                lim2 = Math.Abs(MaximumDistributionDeviation);
                if (lim2 > lim1)
                {
                    txt = $"Tray Spout Area Distribution Deviation {lim2:0.000##} That Exceeds The Project Limit of {lim1:0.000##}";
                    _rVal.AddWarning(this, "Spacing Optimization Warning", txt, uppWarningTypes.ReportFatal);
                    if (bJustOne && _rVal.Count > 0) return _rVal;
                }
                lim1 = aProject.ErrorLimit;
                lim2 = Math.Abs(ErrorPercentage);
                if (lim2 > lim1)
                {
                    txt = $"Tray Spout Area Deviation ({lim2:0.00}) That Exceeds The Project Limit of {lim1:0.00} %";
                    _rVal.AddWarning(this, "Spout Area Warning", txt, uppWarningTypes.ReportFatal);
                    if (bJustOne && _rVal.Count > 0) return _rVal;
                }
            }
            else
            {
                if (!bJustOne || (bJustOne && _rVal.Count <= 0))
                {
                    if (DesignFamily.IsEcmdDesignFamily())
                    {
                        aProject.ReadStatus($"{basestatus} - ECMD Slot Count Check", 1);
                        if (!SlotZones.Validate(this, out txt))
                        {
                            _rVal.AddWarning(this, "ECMD Slot Warning", txt, uppWarningTypes.ReportFatal);
                            if (bJustOne && _rVal.Count > 0) return _rVal;

                        }
                    }
                }


            }




            if (!bJustOne || (bJustOne && _rVal.Count <= 0)) { aProject.ReadStatus($"{basestatus} - Checking Deck Properties", 1); Deck.GenerateWarnings(this, aCategory, _rVal, bJustOne); }
            if (!bJustOne || (bJustOne && _rVal.Count <= 0))
            {
                aProject.ReadStatus($"{basestatus} - Checking Design Options Properties", 1); DesignOptions.GenerateWarnings(this, aCategory, _rVal, bJustOne);
            }

            if (!bJustOne || (bJustOne && _rVal.Count <= 0))
            {
                aProject.ReadStatus($"{basestatus} - Checking Global Downcomer Properties", 1);
                _Downcomer.GenerateWarnings(this, aCategory, _rVal, bJustOne);
            }

            if (!bJustOne || (bJustOne && _rVal.Count <= 0))
            {
                aProject.ReadStatus($"{basestatus} - Checking Downcomer Properties", 1); Downcomers.GenerateWarnings(this, aCategory, _rVal, bJustOne);
            }

            if (!bJustOne || (bJustOne && _rVal.Count <= 0))
            {
                aProject.ReadStatus($"{basestatus} - Checking Spout Group Properties", 1); SpoutGroups.GenerateWarnings(this, aCategory, _rVal, bJustOne);
            }

            if (ProjectType == uppProjectTypes.MDSpout)
            {
                if (!bJustOne || (bJustOne && _rVal.Count <= 0))
                {
                    aProject.ReadStatus($"{basestatus} - Checking Deck Panel Properties", 1);
                    DeckPanels.GenerateWarnings(this, aCategory, _rVal, bJustOne);
                }
            }
            else
            {
                if (!bJustOne || (bJustOne && _rVal.Count <= 0))
                {
                    aProject.ReadStatus($"{basestatus} - Checking Deck Section Properties", 1); DeckSections.GenerateWarnings(this, aCategory, _rVal, bJustOne);
                }

            }
            if (!bJustOne || (bJustOne && _rVal.Count <= 0))
            {
                aProject.ReadStatus($"{basestatus} - Checking Startup Spout Properties", 1); StartupSpouts.GenerateWarnings(this, aCategory, _rVal, bJustOne);

            }

            return _rVal;
        }
        

        public uopProperties AssemblyDetails()
        {
            uopProperties ret = new uopProperties
            {
                { "End Angle", "D1" }
            };
            if (RingClipSize == uppRingClipSizes.FourInchRC)
            {
                ret.Add("DC Ring Clip", "A4");
                ret.Add("Ring Clip", "A3");
            }
            else
            {
                ret.Add("DC Ring Clip", "A2");
                ret.Add("Ring Clip", "A1");
            }

            if (DesignOptions.WeldedStiffeners)
                ret.Add("Finger Clip", "C1");
            else
                ret.Add("Finger Clip", "C2");


            if (DesignOptions.SpliceStyle == uppSpliceStyles.Tabs)
            {
                ret.Add("Deck Splice", "F3");
                ret.Add("Manway Splice", "M14");

                if (DesignOptions.UseManwayClips)
                {
                    ret.Add("Manway", "M4");
                    ret.Add("Manway To Downcomer", "M13");
                }
                else
                {
                    ret.Add("Manway", "M3");
                    ret.Add("Manway To Downcomer", "M11");
                }
            }
            else
            {
                if (DesignOptions.SpliceStyle == uppSpliceStyles.Tabs)
                    ret.Add("Deck Splice", "K3");
                else
                    ret.Add("Deck Splice", "F1");

                if (DesignOptions.UseManwayClips)
                {
                    ret.Add("Manway", "M2");
                    ret.Add("Manway To Downcomer", "M13");
                }
                else
                {
                    ret.Add("Manway", "M1");
                    ret.Add("Manway To Downcomer", "M11");
                }
            }

            if (DesignOptions.UseManwayClips)
                ret.Add("Manway Angle", "M12");
            else
                ret.Add("Manway Angle", "M10");

            if (DesignOptions.CrossBraces)
                ret.Add("CROSS BRACE", "E1");

            if (DeckSplices.SpliceCount(uppSpliceTypes.SpliceWithJoggle) > 0)
                ret.Add("DECK JOGGLE", "K3");

            if (DesignFamily.IsBeamDesignFamily())
            {
                ret.Add("BEAM", "N12");
            }

            return ret;
        }

        public override void Destroy()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {


                    if (_DeckSplices != null)
                    {
                        _DeckSplices.eventDeckSplicesInvalidated -= _DeckSplices_DeckSplicesInvalidated;
                    }

                    if (_Deck != null)
                    {
                        _Deck.PropertyChangeEvent -= SubPartPropertyChangeEventHandler;
                        // _Deck.eventMDDeckInvalidated -= _Deck_MDDeckInvalidated;
                        _Deck.EventMaterialChange -= _Deck_MaterialChange;
                        _Deck.Destroy();
                    }

                    if (_Downcomers != null)
                    {

                        _Downcomers.eventDowncomerMemberChanged -= _Downcomers_DowncomerMemberChanged;
                        _Downcomers.eventDowncomersInvalidated -= _Downcomers_DowncomersInvalidated;
                        _Downcomers.eventSpacingChanged -= _Downcomers_SpacingChanged;
                        _Downcomers.eventSpacingOptimizationBegin -= _Downcomers_SpacingOptimizationBegin;
                        _Downcomers.eventSpacingOptimizationEnd -= _Downcomers_SpacingOptimizationEnd;
                        _Downcomers?.Dispose(true);
                    }


                    if (_SpoutGroups != null)
                    {
                        //_SpoutGroups.eventSpoutGroupMemberChanged -= _SpoutGroups_SpoutGroupMemberChanged;
                        _SpoutGroups.eventSpoutGroupsGenerationEvent -= _SpoutGroups_SpoutGroupsGenerationEvent;
                        _SpoutGroups.eventSpoutGroupsInvalidated -= _SpoutGroups_SpoutGroupsInvalidated;
                        _SpoutGroups.Dispose(true);
                    }
                    if (_Constraints != null)
                    {
                        _Constraints.eventConstraintMemberChanged -= _Constraints_ConstraintMemberChanged;
                        _Constraints.eventConstraintsInvalidated -= _Constraints_ConstraintsInvalidated;
                        _Constraints.Dispose(true);
                    }

                    if (_DesignOptions != null)
                    {
                        _DesignOptions.PropertyChangeEvent -= SubPartPropertyChangeEventHandler;
                        _DesignOptions.Destroy();
                    }

                    if (_DeckPanels != null)
                    {
                        _DeckPanels.eventMDPanelMemberChanged -= _DeckPanels_MDPanelMemberChanged;
                        _DeckPanels.Dispose(true);
                    }

                    if (_DeckSections != null)
                    {

                        _DeckSections.eventDeckSectionsInvalidated += _DeckSections_DeckSectionsInvalidated;
                       // _DeckSections.Dispose(true);
                    }

                    if (_Downcomer != null)
                    {

                        _DeckSections.eventDeckSectionsInvalidated += _DeckSections_DeckSectionsInvalidated;
                        _Downcomer.Destroy();

                    }
                    {
                    
                        _APPans?.Dispose(true);

                        _Deck = null;
                        _Downcomer = null;
                        _DesignOptions = null;
                        _Downcomers = null;
                        _SpoutGroups = null;
                        _Constraints = null;
                        _DeckPanels = null;
                        _DeckSplices = null;
                        _DeckSections = null;
                        _StartupSpouts = null;
                        _SpliceAngles = null;
                        _APPans = null;
                    }

                    SpoutAreaMatrices = null;
                    disposedValue = true;
                }
            }
        }


        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public override void SubPart(uopPart aPart, string aCategory = null, bool? bHidden = null)
        {
            if (aPart == null) return;
            try
            {
                if (aPart.GetType() == typeof(mdTrayRange))
                {

                    base.SubPart((mdTrayRange)aPart, aCategory, bHidden);

                }
                else
                {
                    base.SubPart(aPart, aCategory, bHidden);

                }
            }
            catch (Exception e)
            {
                Debug.Fail(e.Message);
            }
            finally
            {



            }

        }

        /// <summary>
        /// make sure all persistent sub parts are up to date
        /// </summary>
        public void UpdatePersistentSubParts(mdProject aProject, mdTrayRange aRange, bool bForceRegen = false)
        {
            aProject ??= GetMDProject();
            aRange ??= GetMDRange();
            RaiseStatusChangeEvent($"Updating {TrayName()} Persisent Sub-Parts");
            if (bForceRegen) ResetComponents(true);
            GenerateDowncomers();
            GenerateDeckPanels();
            GenerateSpoutGroups();
            if (ProjectType == uppProjectTypes.MDDraw)
            {
                GenerateDeckSections();
                // GenerateUniqueDeckSections(bForceRegen);
                GenerateSlotZones();

                GenerateAPPans();
            }
            if (_Beam == null)
            {
                _Beam = new mdBeam();
                _Beam.eventMDBeamPropertyChange += SubPartPropertyChangeEventHandler;
                _Beam.SubPart(this);

            }
        }

        /// <summary>
        /// returns the centers of the sdc stiffeners in the assembly
        /// </summary>
        /// <param name="aDCIndex">allows filtering to a single dowcomer</param>
        /// <param name="bTrayWide">if true, the points on the virtual side of a standrard tray are returned as well with a 180 value</param>
        /// <param name="bWithSupNotch">allows filterinf for dcs that hav supplemental deflectors</param>
        /// <returns></returns>
        public uopVectors StiffenerCenters(int? aDCIndex = null, bool bTrayWide = false, bool? bWithSupNotch = null, int aBoxIndex =0)
        {
            uopVectors _rVal = new uopVectors();
            List<mdDowncomer> dcs = Downcomers.GetByVirtual(aVirtualValue: false);
            if (aDCIndex.HasValue) dcs = dcs.FindAll((x) => x.Index == aDCIndex.Value);
            bool symmetric = DesignFamily.IsStandardDesignFamily();
            if (bWithSupNotch.HasValue)
            {
                dcs = bWithSupNotch.Value ? dcs.FindAll((x) => x.SupplementalDeflectorHeight > 0) : dcs.FindAll((x) => x.SupplementalDeflectorHeight <= 0);
            }
            foreach (mdDowncomer dc in dcs)
            {
                List<mdDowncomerBox> boxes = dc.Boxes.FindAll((x) => !x.IsVirtual);
                foreach (var box in boxes)
                {
                    if (aBoxIndex > 0 && box.Index != aBoxIndex) continue;
                    UVECTORS cntrs = box.StiffenerPoints(dc);
                    if (bTrayWide && box.OccuranceFactor > 1 )
                    {
                        if (symmetric)
                        {
                            cntrs.AppendMirrors(aX: 0, aY: null, aValue: 180);
                        }
                        else
                        {
                           if(box.OccuranceFactor > 1) cntrs.Append(box.Instances.ApplyTo(cntrs, bReturnBaseVectors: false, aMirrorX: 0, aMirrorY: -box.Y));
                        }
                        
                    }
                    _rVal.Append(cntrs, aPartIndex: dc.Index);

                }
            }
            return _rVal;
        }

        public override double PanelWidth()
        {
            double gap = PanelClearance(true);
            return FunctionalPanelWidth - 2 * gap;
        }


        #endregion Methods

        #region Event Handlers


        /// <summary>
        /// '^used by objects above this object in the object model to alert a sub object that a property above it in the object model has changed.
        /// ~alerts the objects below it of the change
        /// </summary>
        /// <param name="aProperty"></param>
        public override void Alert(uopProperty aProperty)
        {
            if (aProperty == null) return;

            uppPartTypes pType = aProperty.PartType;
            string pname = aProperty.Name.ToUpper();
            ResetSubComponents();
            if (pType == uppPartTypes.TraySupportBeam)
                Invalidate(uppPartTypes.TraySupportBeam);

            switch (pname)
            {
                case "DOWNCOMERROUNDTOLIMIT":
                    ResetComponents(true);
                    break;
                case "SPACINGMETHOD":
                    double spacwuz = Downcomers.Spacing;
                    double newspace = mdSpacingSolutions.OptimizedSpace(this,out _, aStartValue: spacwuz).Spacing;

                    if (newspace != spacwuz)
                        Invalidate(uppPartTypes.Downcomer);

                    break;

                case "MANHOLEID":
                    if (aProperty.LastValue > aProperty.Value)
                        Invalidate(uppPartTypes.DeckSplice);
                    break;


                case "DESIGNFAMILY":
                    uppMDDesigns lastDesignFamily = (uppMDDesigns)aProperty.LastValue;
                    DesignFamily = (uppMDDesigns)aProperty.ValueI;
                    DesignOptions.HasBubblePromoters = !DesignFamily.IsEcmdDesignFamily();
                    if (Math.Abs((int)lastDesignFamily - (int)DesignFamily) > 50)
                    {
                        PropValSet("OverrideSpacing", 0, bSuppressEvnts: true);
                        ResetComponents(true);
                        if (_Downcomers != null)
                        {
                            _Downcomers.Optimized = false;
                            _Downcomers.Clear();
                        }


                    }

                    break;


                case "SHELLID":
                case "RINGID":
                case "OVERRIDERINGCLEARANCE":
                    ResetComponents();
                    break;
                case "METRICSPOUTING":
                    if (aProperty.Value == true)
                    {
                        //Downcomer().SpoutDiameter = uopUtils.RoundTo(Downcomer().SpoutDiameter, dxxRoundToLimits.Millimeter);
                        Downcomer().StartupDiameter = uopUtils.RoundTo(Downcomer().StartupDiameter, dxxRoundToLimits.Millimeter);
                    }
                    else
                    {
                        //Downcomer().SpoutDiameter = uopUtils.RoundTo(Downcomer().SpoutDiameter, dxxRoundToLimits.Sixteenth);
                        Downcomer().StartupDiameter = uopUtils.RoundTo(Downcomer().StartupDiameter, dxxRoundToLimits.Sixteenth);

                    }

                    Invalidate(uppPartTypes.SpoutGroup);
                    Invalidate(uppPartTypes.StartupSpout);
                    break;
            }
        }

        // <summary>
        // sets the parts property with the passed name to the passed value
        //returns the property if the property value actually changes.
        // </summary>
        public override uopProperty PropValSet(string aName, object aPropVal, int aOccur = 0, bool? bSuppressEvnts = null, bool? bHiddenVal = null)
        {
            if (string.IsNullOrWhiteSpace(aName) || aPropVal == null) return null;
            bool supevnts = bSuppressEvnts.HasValue ? bSuppressEvnts.Value : SuppressEvents || Reading;
            uopProperty _rVal = base.PropValSet(aName, aPropVal, aOccur, supevnts, bHiddenVal);
            if (_rVal != null)
            {
                switch (aName.ToUpper())
                {
                    case "OVERRIDESPACING":
                        Downcomers.PropValSet(aName, _rVal.Value);
                        ResetComponents();
                        Invalidate(uppPartTypes.DeckSections);
                        break;
                    case "HOW":
                        if (_rVal.IsGlobal) Invalidate(uppPartTypes.FlowSlotZone);
                        break;
                }
            }

            if (_rVal == null || supevnts) return _rVal;


            Notify(_rVal);
            return _rVal;
        }
        private void _Constraints_ConstraintMemberChanged(uopProperty aProperty) => Notify(aProperty);


        private void _Constraints_ConstraintsInvalidated()
        {
            bool bRaiseIt = true;
            if (_SpoutGroups != null)
            {
                if (!_SpoutGroups.Invalid) bRaiseIt = false;
                Invalidate(uppPartTypes.SpoutGroup);
            }

            if (bRaiseIt) eventMDAssemblyPartsInvalidated?.Invoke();

        }

        private void _DeckPanels_MDPanelMemberChanged(uopProperty aProperty) => Notify(aProperty);

        private void _DeckSections_DeckSectionsInvalidated()
        {
            Invalidate(uppPartTypes.BubblePromoter);
        }

        private void _DeckSplices_DeckSplicesInvalidated()
        {

            Invalidate(uppPartTypes.SpliceAngle);
            Invalidate(uppPartTypes.DeckSection);


            Invalidate(uppPartTypes.FlowSlotZone);
            Invalidate(uppPartTypes.BubblePromoter);
            
        }

        private void _Deck_MDDeckInvalidated()
        {
            eventMDAssemblyPartsInvalidated?.Invoke();
        }

        private void SubPartPropertyChangeEventHandler(uopProperty aProperty)
        {
            if (aProperty == null) return;

            Notify(aProperty);


        }

        private void _Deck_MaterialChange(uopSheetMetal NewMaterial)
        {
            Invalidate(uppPartTypes.SpliceAngle);
            if (_StartupSpouts != null)
            {
                double value = NewMaterial.Thickness + Downcomer().StartupDiameter * 0.5;
                _StartupSpouts.SetZ(ref value);
            }
            if (!_DeckPanels.Invalid)
            {
                _DeckPanels.UpdateMaterial((uopSheetMetal)_Deck.Material);
            }
        }


        private void _Downcomer_MaterialChange(uopSheetMetal NewMaterial) => Notify(uopProperty.Quick("MATERIAL", NewMaterial.Descriptor, "", _Downcomer));

        private void _Downcomers_DowncomerMemberChanged(uopProperty aProperty, bool isGlobalDowncomer)
        {
            if (aProperty == null) return;
            aProperty.IsGlobal = false;
            Notify(aProperty);
        }

        private void _Downcomers_DowncomersInvalidated()
        {
            ResetSubComponents();

            Invalidate(uppPartTypes.DeckPanel);
            Invalidate(uppPartTypes.StartupSpout);
        }

        private void _Downcomers_SpacingChanged()
        {
            RecalculateSpoutGroups();
            ResetComponents(false);

            PropValSet("Spacing", _Downcomers.Spacing, bSuppressEvnts: true);
            _Downcomer.PropValSet("Spacing", _Downcomers.Spacing, bSuppressEvnts: true);
            Invalidate(uppPartTypes.DeckPanel);
        }

        private void _Downcomers_SpacingOptimizationBegin()
        {
            eventGenerationEvent?.Invoke(uppTrayGenerationEventTypes.DowncomerSpacing, true, "");
            Invalidate(uppPartTypes.DeckPanel);

        }

        private void _Downcomers_SpacingOptimizationEnd(double StartSpace, double EndSpace)
        {
            if (StartSpace != EndSpace)
            {
                RecalculateSpoutGroups();
                ResetComponents(false);
            }
            eventGenerationEvent?.Invoke(uppTrayGenerationEventTypes.DowncomerSpacing, false, "");

        }


        private void _SpoutGroups_SpoutGroupMemberChanged(uopProperty aProperty)
        {
            Invalidate(uppPartTypes.APPan);
            Notify(aProperty);
        }

        private void _SpoutGroups_SpoutGroupsGenerationEvent(bool aBegin, string aHandle)
        {
            Invalidate(uppPartTypes.APPan);
            if (aBegin)
            {
                eventGenerationEvent?.Invoke(uppTrayGenerationEventTypes.SpoutGroup, true, aHandle);
                //        If Not _StartupSpouts Is Nothing Then Invalidate prtStartupSpout
            }
            else
            {
                eventGenerationEvent?.Invoke(uppTrayGenerationEventTypes.SpoutGroup, false, aHandle);
                //        If Not _StartupSpouts Is Nothing Then Invalidate prtStartupSpout
            }
        }

        private void _SpoutGroups_SpoutGroupsInvalidated()
        {
            Invalidate(uppPartTypes.APPan);
            eventMDAssemblyPartsInvalidated?.Invoke();
        }

 
        #endregion Event Handlers


    }
}