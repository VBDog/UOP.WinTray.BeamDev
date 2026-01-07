using UOP.DXFGraphics;
using System;
using System.Collections.Generic;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Materials;

using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;

namespace UOP.WinTray.Projects.Parts
{
    /// <summary>
    /// column tray range object.
    /// each uopColumn in a mdProject has a collection of mdTrayRanges defined for it.
    /// each TrayRange in the column has a mdTrayAssembly defined for it.
    /// </summary>
    public class mdTrayRange : uopTrayRange
    {
        public override uppPartTypes BasePartType => uppPartTypes.TrayRange;
        //!!PRIMARY CLASS
        //======== object properties
        private uopEventHandler _Events;
        private mdTrayAssembly _TrayAssembly;

        //======== numeric property indices
       
        //@raised when the range configuration data changes
        public delegate void MDRangePropertyChange(uopProperty aProperty);
        public event MDRangePropertyChange eventMDRangePropertyChange;
        //raised when part collection generation begins or ends
        public delegate void PartGeneration(bool Begin);
        public event PartGeneration eventPartGeneration;
        //raised when part collection generation begins or ends
        //public delegate void SubPartGeneration(bool Begin);
        //public event SubPartGeneration eventSubPartGeneration;
      
        private bool disposedValue;

        #region Constructors

        public mdTrayRange() : base(uppProjectFamilies.uopFamMD) { InitializeProperties(); }

        internal mdTrayRange(mdTrayRange aBasisToCopy) : base(uppProjectFamilies.uopFamMD) { InitializeProperties(aBasisToCopy); }

        private void InitializeProperties(mdTrayRange aBasisToCopy = null)
        {
            try
            {
                DowncomerRoundToLimit = uppMDRoundToLimits.Sixteenth;
                _Events = uopGlobals.goEvents;
                string newGuid = mzUtils.CreateGUID();
                TraySortOrder = uppTraySortOrders.TopToBottom;
                ProjectType = uppProjectTypes.MDSpout;
                Elevation = null;
                Requested = true;
                ColumnLetter = string.Empty;
                if (aBasisToCopy != null)
                {
                    Copy(aBasisToCopy);

                    ColumnLetter = aBasisToCopy.ColumnLetter;
                    GUID = newGuid;
                    Selected = false;
                    TraySortOrder = aBasisToCopy.TraySortOrder;
                    PropValSet("StageList", "", bSuppressEvnts: true);
                    Elevation = aBasisToCopy.Elevation.Clone();
                    _TrayAssembly = aBasisToCopy.TrayAssembly.Clone();

                }
                else
                {
                    AddProperty("ShellID", 0, aUnitType: uppUnitTypes.SmallLength);
                    AddProperty("RingID", 0, aUnitType: uppUnitTypes.SmallLength);
                    AddProperty("RingStart", 0);
                    AddProperty("RingEnd", 0);
                    AddProperty("RingThk", 0, aUnitType: uppUnitTypes.SmallLength, aDisplayName: "Ring Thk.");
                    AddProperty("OverrideRingClearance", 0, aDisplayName: "Ovrd. Ring Clrc.", aUnitType: uppUnitTypes.SmallLength, aNullVal:0);
                    AddProperty("RingSpacing", 0,  aDisplayName:"Tray Spacing", aUnitType: uppUnitTypes.SmallLength);
                    AddProperty("DesignFamily", uppMDDesigns.MDDesign, aDisplayName: "Tray Type", aDecodeString: dxfEnums.ValueNameList(typeof( uppMDDesigns) )); // "0=MD,1=ECMD,2=EMD,3=VGMD") // //TODO:uopMDDesign, , , , , "0=MD,1=ECMD,2=EMD,3=VGMD"); //TODO
                    AddProperty("HardwareMaterial", "Carbon Steel");
                    AddProperty("StageList", "");
                    AddProperty("RevampStrategy", "1-for-1", aDisplayName: "Revamp Type");
                    AddProperty("OverrideTrayDiameter", 0, aDisplayName: "Ovrd. Tray OD", aUnitType: uppUnitTypes.SmallLength, aNullVal: 0);
                    AddProperty("TrayCount", 0, bProtected: true);
                    AddProperty("RingWidth", 0, aUnitType: uppUnitTypes.SmallLength, bOptional:true);
                    AddProperty("ManholeID", 0, aDisplayName: "Ovrd. Manhole ID", aUnitType: uppUnitTypes.SmallLength, aNullVal: 0, bOptional: true);
                   
                    _TrayAssembly = new mdTrayAssembly();
                    
                }
               
                _TrayAssembly.SubPart(this);
                _TrayAssembly.PropertyChangeEvent += TrayAssembly_MDAssemblyPropertyChange;
                //uopEventHandler.eventRangeRequest += _Events_RangeRequest;
              
            }
            catch (Exception e) { throw e; }
        }
        /// <summary>
        /// executed to ensure that all assembly parts are defined and valid
        /// </summary>
        public void InitializeComponents()
        {
            mdTrayAssembly aAssy = TrayAssembly;
            bool wuz = Reading;
            Reading = true;
            if (aAssy.Downcomer().Count > 1)
            {
                if (aAssy.DeckSectionWidth(false) > 16)
                { aAssy.Deck.ManwayCount = 1; }
                else
                { aAssy.Deck.ManwayCount = 0; }
            }
            else
            { aAssy.Deck.ManwayCount = 0; }

            colMDDowncomers DComers = aAssy.Downcomers; //asking for the will ensure they are valid
            colMDDeckPanels DPs = aAssy.DeckPanels; //asking for the will ensure they are valid
            aAssy.GenerateSpoutGroups();

            Reading = wuz;
        }



        #endregion Constructors

        #region Properties


        public override uppMDDesigns DesignFamily => (uppMDDesigns)PropValI("DesignFamily");

        public override double OverrideRingClearance { get => PropValD("OverrideRingClearance"); set => PropValSet("OverrideRingClearance", value, bSuppressEvnts: true); }

        /// <summary>
        ///the thickness of the bolting bar in the tray range
        ///~same as RingThk
        /// </summary>
        /// 
        public override double BoltBarThk
        {
            get => PropValD("RingThk");
            set => Notify(PropValSet("RingThk", Math.Abs(value)));
        }
        /// <summary>
        ///returns the objects properties in a collection
        //signatures like "COLOR=RED"
        /// </summary>
         public override uopProperties CurrentProperties()  { UpdatePartProperties(); return base.CurrentProperties(); }

        /// <summary>
        /// //^the object which carries the deck design data for the tray assembly in this range
        /// </summary>
        public mdDeck Deck => TrayAssembly.Deck;

        /// <summary>
        /// the object which carries the downcomer design data for the tray assembly in this range
        /// </summary>
        public override mdDowncomer Downcomer => TrayAssembly.Downcomer();

        public string DrawingPath => $"Project.Drawings.Range({ PartIndex })";

        /// <summary>
        ///^returns True if the tray range has anti-penetration pans defined
        /// </summary>
        public override bool HasAntiPenetrationPans => TrayAssembly.HasAntiPenetrationPans;


        /// <summary>
        /// returns true if the range has a downcomer with triangular end plates
        /// </summary>
        public bool HasTriangularEndPlates => TrayAssembly.HasTriangularEndPlates;

        public override string INIPath => "COLUMN(" + ColumnIndex + ").RANGE(" + Index + ")";
        /// <summary>
        /// the index of the tray range in a collection of TrayRanges
        /// </summary>
        public override int Index { get => PartIndex; set { if (PartIndex == value) return; PartIndex = value; ResetParts(); } }

        /// <summary>
        /// a collection of notes assigned to the range
        /// </summary>
        public uopProperties Notes
        {
            get => GetNotes();
            set => Notify((uopProperty)SetNotes(value));

        }

        /// <summary>
        ///a value used to supercede the default tray diameter
        /// </summary>
        public double OverrideTrayDiameter
        {
            get => PropValD("OverrideTrayDiameter");
            set => Notify(PropValSet("OverrideTrayDiameter", value));
        }

        // <summary>
        /// the default tray diameter
        /// </summary>
        public double TrayDiameter
        {
            get
            {
                double oride = OverrideTrayDiameter;
                double _rVal = DefaultTrayDiameter;
                if (Math.Abs(oride) > 0)
                {
                    if (oride < _rVal - 1) { oride = _rVal - 1; PropValSet("OverrideTrayDiameter", oride, bSuppressEvnts: true); }
                    if (oride > _rVal + 1) { oride = _rVal + 1; PropValSet("OverrideTrayDiameter", oride, bSuppressEvnts: true); }
                    { _rVal = oride; }
                }
                return _rVal;
            }

        }
        public double DefaultTrayDiameter => uopUtils.DefaultTrayDiameter(this);

        public double DefaultAPPanClearance => mdUtils.DefaultAPPanClearance(this);


        public double DefaultCDP => mdUtils.DefaultCDP(this);


        /// <summary>
        ///returns the top level parts that makeup this part
        /// </summary>
        public colUOPParts Parts => GenerateParts();

        /// <summary>
        ///the parent mdProject object of the parent uopColumn of the tray range
        /// </summary>
        public new mdProject Project => GetMDProject();

        public dynamic SelectedSpoutGroupIndex
        {
            get => SpoutGroups.SelectedGroupIndex;
            set
            {
                SpoutGroups.SetSelected(value);
                //mdSpoutGroup aSG = null;

                //aSG = SpoutGroups.Item(value);
                //if (aSG != null)
                //{
                //     aSG.Selected = true;
                //}
            }
        }

        //int iDesign = 0;
        ///// <summary>
        ///// the column ID in the tray range
        ///// </summary>
        //public string Design
        //{
        //    get
        //    {
        //        bool rExists = false;
        //        return PropValGet(iDesign, ref rExists);
        //    }
        //    set
        //    {
        //        Notify(PropValSet(iDesign, value));

        //    }
        //}
        /// <summary>
        /// the column ID in the tray range
        /// </summary>
        public override double ShellID
        {
            get => PropValD("ShellID");
            set => Notify(PropValSet("ShellID", Math.Abs(value)));
        }
        /// <summary>
        /// controls how the optimimum downcomer spacing is calculated for the tray assembly
        ///~set at project wide level
        /// </summary>
        public uppMDSpacingMethods SpacingMethod
        {
            get
            {
                mdProject aProj = GetMDProject();
                return (aProj != null) ? aProj.SpacingMethod : uppMDSpacingMethods.Weighted;
            }
        }


        /// <summary>
        /// the spout groups of the ranges tray assembly
        /// </summary>
        public colMDSpoutGroups SpoutGroups => TrayAssembly.SpoutGroups;

        /// <summary>
        /// a collection of the indexes of the stages that are associated to this range
        /// </summary>
        public override List<int> StageIndices => mzUtils.ListToIntegerCollection(StageList, ",");
            
        /// <summary>
        /// //^a comma deliminated string that contains the stages associated to this range
        /// </summary>
        public string StageList
        {
            get => PropValS("StageList");
            set => Notify(PropValSet("StageList", value));

        }

        /// <summary>
        ///returns the stage that defined this range
        /// </summary>
        public colMDStages Stages
        {
            get
            {

                colMDStages stages = new colMDStages();
                mdProject aProj = GetMDProject();
                if (aProj == null) { return stages; }

                List<int> IDXS = StageIndices;
                if (IDXS.Count <= 0) { return stages; }

                colMDStages cStages = aProj.Stages;
                if (cStages == null) { return stages; }
                if (cStages.Count <= 0) { return stages; }


                mdStage aStage = null;

                for (int i = 0; i < IDXS.Count; i++)
                {
                    aStage = cStages.GetByStageNumber(IDXS[i]);
                    if (aStage != null)
                    {
                        stages.AddStage(aStage, IDXS[i]);
                    }
                }
                return stages;
            }
        }

        /// <summary>
        ///true if all the bolting on the tray is stainless steel
        /// </summary>
        public bool StainlessBolting => RangeStructure().HardwareMaterial.IsStainless;

        /// <summary>
        /// a string describing the revamp stategy for the tray
        /// </summary>
        public string RevampStrategy { get => PropValS("RevampStrategy"); set => Notify(PropValSet("RevampStrategy", value.Trim())); }


        /// <summary>
        /// the diameter of the bolt circle that all ring clip holes in the tray will fall on
        ///used to lay out ring clips correctly
        /// </summary>
        public override double RingClipRadius => BoundingRadius;

        //the radius of the parts parent tray deck panels
        public override double DeckRadius
        {
            get
            {
                double _rVal = 0;
                TPROPERTIES rProps = ActiveProps;
                if (rProps.Count > 0)
                {
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
                }
                return _rVal;
            }
        }

        public override double RingClearance
        {
            get
            {
                double defval = uopUtils.BoundingClearance(ShellID);
                double _rVal = OverrideRingClearance;
                if (_rVal <= 0)
                { _rVal = defval; }
                else
                {
                    if (Math.Round(defval - _rVal, 1) == 0) { PropValSet("OverrideRingClearance", 0, bSuppressEvnts: true); }

                }

                return _rVal;
            }
        }

        /// <summary>
        /// //^the ending support ring number of the tray range
        /// </summary>
        public override int RingEnd { get => PropValI("RingEnd"); set => Notify(PropValSet("RingEnd", Math.Abs(value))); }

        /// <summary>
        /// the support ring ID in the tray range
        /// </summary>
        public override double RingID { get => PropValD("RingID"); set => Notify(PropValSet("RingID", value)); }
        public override double RingRadius{ get => RingID / 2; }

        /// <summary>
        /// the distance between support rings in this tray range
        /// </summary>
        public override double RingSpacing { get => PropValD("RingSpacing"); set => Notify(PropValSet("RingSpacing", Math.Abs(value))); }

        /// <summary>
        /// the starting support ring number of the tray range
        /// </summary>
        public override int RingStart { get => PropValI("RingStart"); set => Notify(PropValSet("RingStart", Math.Abs(value))); }



        /// <summary>
        ///    //^the ring thickness
        /// </summary>
        public override double RingThk
        {
            get => base.RingThickness;
            set
            {
                value = Math.Abs(value);
                double colval = PropValD("RingThickness", uppPartTypes.Column);
                if (value != colval && value != 0) Notify(PropValSet("RingThk", value));
            }
        }


        /// <summary>
        /// the collection of supporting ring sections of the tray range
        ///~all members of the collection ar uopRing objects
        /// </summary>
        public override colUOPParts Rings => new colUOPParts(new uopRing(0, 360, this), this);

        public  colMDDowncomers Downcomers => TrayAssembly.Downcomers;

        public override uopTrayAssembly Assembly => (uopTrayAssembly)TrayAssembly;

        /// <summary>
        /// //^the tray assembly defined for the tray range
        /// </summary>
        public mdTrayAssembly TrayAssembly
        {
            get
            {
                if (_TrayAssembly == null)
                {
                    _TrayAssembly = new mdTrayAssembly();
                    _TrayAssembly.PropertyChangeEvent += TrayAssembly_MDAssemblyPropertyChange;
                }
                _TrayAssembly.SubPart(this);
                return _TrayAssembly;
            }
            set
            {
                if(_TrayAssembly != null) _TrayAssembly.PropertyChangeEvent -= TrayAssembly_MDAssemblyPropertyChange;
                _TrayAssembly = value;
                if (value != null)
                {
                    _TrayAssembly.PropertyChangeEvent += TrayAssembly_MDAssemblyPropertyChange;
                    _TrayAssembly.SubPart(this);
                }
            }
        }

     

        /// <summary>
        /// ///returns a string that represents the design family of the MD tray
        ///~like "MD Tray" or "ECMD Tray"
        /// </summary>
        public override string TrayTypeName => $"{TrayAssembly.FamilyName} Tray";

        public override uopTrayAssembly GetTrayAssembly(uopTrayAssembly aAssy = null) => TrayAssembly;

        public override uopSheetMetal DeckMaterial { get => Deck.SheetMetal; set => Deck.SheetMetal = value; }

        public override uopSheetMetal BeamMaterial { get => Downcomer.SheetMetal; set => Downcomer.SheetMetal = value; }

        //the manhole ID of the parent ray range
        public override double ManholeID
        {
            get
            {
                double _rVal = PropValD("ManholeID");
                return (_rVal <= 0) ? PropValD("ManholeId", aPartType: uppPartTypes.Column) : _rVal;
            }

            set
            {
                if (value < 0) return;
                if (value > 0 && value == PropValD("ManholeId", aPartType: uppPartTypes.Column)) value = 0;
                Notify(PropValSet("ManholeID", value));
            }
        }


        public override string WarningPath => "Project.Warnings.Range(" + PartIndex + ")";

        public uppMDRoundToLimits DowncomerRoundToLimit { get; set; }
     

        #endregion Properties

        #region Methods

        public override string ToString() { return Name(true); }



        public override string Name(bool bIncludeDesignIndicator = false) => base.TrayName(bIncludeDesignIndicator);

        public override colUOPParts SpliceAngles(bool bExcludeManwaySplices = false, bool bUpdateQuantities = false, colUOPParts aCollector = null)
        {
            colUOPParts _rVal = aCollector ?? new colUOPParts(this);
            colUOPParts SAngles = TrayAssembly.SpliceAngles();
            List<uopPart> aCol = SAngles.GetByPartType(uppPartTypes.SpliceAngle);
            int cnt = 0;
            mdSpliceAngle aMem = null;
            mdSpliceAngle bMem = null;
            int i = 0;
            aCol = SAngles.GetByPartType(uppPartTypes.SpliceAngle);
            if (aCol.Count > 0)
            {
                aMem = (mdSpliceAngle)aCol[0].Clone();
                if (bUpdateQuantities)
                {
                    cnt = 0;
                    for (i = 0; i < aCol.Count; i++)
                    {
                        bMem = (mdSpliceAngle)aCol[i];
                        if (Math.Round(bMem.X, 1) > 0)
                        { cnt += 2; }
                        else
                        { cnt += 1; }
                    }
                    aMem.Quantity = cnt;
                }
                _rVal.Add(aMem);
            }

            aCol = SAngles.GetByPartType(uppPartTypes.ManwayAngle);

            if (aCol.Count > 0)
            {
                aMem = (mdSpliceAngle)aCol[0].Clone();
                if (bUpdateQuantities)
                {
                    cnt = 0;
                    for (i = 0; i < aCol.Count; i++)
                    {
                        bMem = (mdSpliceAngle)aCol[i];
                        if (Math.Round(bMem.X, 1) > 0)
                        { cnt += 2; }
                        else
                        { cnt += 1; }
                    }
                    aMem.Quantity = cnt;
                }
                _rVal.Add(aMem);
            }

            if (!bExcludeManwaySplices)
            {
                aCol = SAngles.GetByPartType(uppPartTypes.ManwaySplicePlate);
                if (aCol.Count > 0)
                {
                    aMem = (mdSpliceAngle)aCol[0].Clone();
                    if (bUpdateQuantities)
                    {
                        cnt = 0;
                        for (i = 0; i < aCol.Count; i++)
                        {
                            bMem = aCol[i] as mdSpliceAngle;
                            bMem = (mdSpliceAngle)aCol[i];
                            if (Math.Round(bMem.X, 1) > 0)
                            { cnt += 2; }
                            else
                            { cnt += 1; }
                        }
                        aMem.Quantity = cnt;
                    }
                    _rVal.Add(aMem);
                }
            }
            return _rVal;
        }


        /// <summary>
        /// used by objects above this object in the object model to alert a sub object that a property above it in the object model has changed.
        /// alerts the objects below it of the change
        /// </summary>
        /// <param name="aProperty"></param>
        public override void Alert(uopProperty aProperty)
        {
            if (aProperty == null)
            {
                return;
            }
            string pname = aProperty.Name.ToUpper();
            uppPartTypes pType = aProperty.PartType;
          
            if (pType == uppPartTypes.Column)
            {
                if (pname == "RINGTHICKNESS")
                {
                    if (Math.Round(aProperty.LastValue, 4) == Math.Round(RingThk, 4))
                    {
                        PropValSet("RingThk", aProperty.ValueD, bSuppressEvnts: true);
                    }
                }
            }

            TrayAssembly.Alert(aProperty);
        }

        /// <summary>
        ///saves the stage number of the passed stage to the list of stages that this range is associated to
        /// </summary>
        /// <param name="aStage">the stage to associate to this range</param>
        public void AssociateToStage(mdStage aStage)
        {
            if (aStage == null) return;

            List<int> indices = StageIndices;
            if (indices.Contains(aStage.StageNumber)) return;
            indices.Add(aStage.StageNumber);
            StageList = mzUtils.ListToString(indices, ",");
            aStage.RangeGUID = GUID;
        }


        public override dxfRectangle Rectangle(double aScaler = 0) => (aScaler <= 0) ? new dxfRectangle(ShellID, ShellID) : new dxfRectangle(ShellID * aScaler, ShellID * aScaler);

        /// <summary>
        ///returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>
        public mdTrayRange Clone() => new mdTrayRange(this);

        public override uopPart Clone(bool aFlag = false) => (uopPart)this.Clone();


        public override uopProperty CurrentProperty(string aPropertyName, bool bSupressNotFoundError = false) { UpdatePartProperties(); return base.CurrentProperty(aPropertyName, bSupressNotFoundError); }


        /// <summary>
        /// uses a mdstage objects properties to define the md tray ranges properties.
        ///uop stages are define by an external  UOP application and written in MDH files.
        /// </summary>
        /// <param name="aStage">the hydraulic stage to use to define the tray range</param>
        /// <param name="aWarnings"></param>
        public void DefineByStage(mdStage aStage, uopDocuments aWarnings = null)
        {
            if (aStage == null)  return; 
            aWarnings ??= new uopDocuments();

       
            mdTrayAssembly aAssy = new mdTrayAssembly();
            mdDeck mDeck = aAssy.Deck;
            mdDowncomer DComer = new mdDowncomer();

         
            //range properties
            PropValSet("ShellID", aStage.PropValD("ShellID",aMultiplier : 12), bSuppressEvnts: true);
            PropValSet("RingID", ShellID - (2 * aStage.PropValD("RingWidth")), bSuppressEvnts: true);
            PropValSet("RingSpacing", aStage.PropValD("TraySpacing"), bSuppressEvnts: true);
            PropValSet("DesignFamily", aStage.PropValS("TrayType").Trim().ToUpper() == "ECMD" ? uppMDDesigns.ECMDDesign : uppMDDesigns.MDDesign, bSuppressEvnts: true);
            
            //downcomer properties
            int dcCnt =  aStage.PropValI("DCCount");
            DComer.PropValSet("How", aStage.PropValD("WeirHeight"), bSuppressEvnts: true);
            DComer.PropValSet("InsideHeight", aStage.PropValD("DCHeight"), bSuppressEvnts: true);
            DComer.PropValSet("Width", aStage.PropValD("DCWidth"), bSuppressEvnts: true);
            DComer.PropValSet("Count", dcCnt, bSuppressEvnts: true);
            double thickness = aStage.PropValD("DCThickness");
            
            if (aStage.PropValD("DCThickness") <= 0)
            {
                DComer.Material = uopGlobals.goSheetMetalOptions().GetByNearestThickness(uppMaterialTypes.SheetMetal, 0);
                aWarnings.AddWarning(this, "Missing Stage Material Thickness", $"The Stage Definition Use To Define This Range Has No Downcomer Thickness Defined. { DComer.Material.GageName } { DComer.Material.FamilyName } Was Selected As Default");
            }
            else
            {
                DComer.Material = uopGlobals.goSheetMetalOptions().GetByNearestThickness(uppMaterialTypes.SheetMetal, thickness);
            }

            //deck properties
            mDeck.PropValSet("ManwayCount", 0, bSuppressEvnts: true);
            mDeck.PropValSet("Fp", aStage.PropValD("PerfFraction",aMultiplier: 100), bSuppressEvnts: true);
            mDeck.PropValSet("PerfDiameter", aStage.PropValD("PerfDiameter"), bSuppressEvnts: true);
            mDeck.PropValSet("SlottingPercentage", aStage.PropValD("SlotFraction", aMultiplier: 100), bSuppressEvnts: true);
            if (aStage.PropValD("DeckThickness") <= 0)
            {

                mDeck.Material = (uopSheetMetal)uopGlobals.goSheetMetalOptions().GetByNearestThickness(uppMaterialTypes.SheetMetal, 0);
                aWarnings.AddWarning(this, "Missing Stage Material Thicknes", $"The Stage Definition Use To Define This Range Has No Deck Thickness Defined. { DComer.Material.GageName } { DComer.Material.FamilyName } Was Selected As Default");
            }
            else
            {
                mDeck.Material = (uopSheetMetal)uopGlobals.goSheetMetalOptions().GetByNearestThickness(uppMaterialTypes.SheetMetal, aStage.PropValD("DeckThickness"));
            }

            //do this last to generate the spout groups
            DComer.PropValSet("Asp", Math.Abs(aStage.PropValD("SpoutArea"))) ;
            aAssy.SetDowncomer(DComer);

            AssociateToStage(aStage);

            aAssy.PropValSet("StartupConfiguration", uppStartupSpoutConfigurations.TwoByTwo, bSuppressEvnts: true);

            PropValSet("RingWidth", RingWidth, bSuppressEvnts: true);
            PropValSet("TrayCount", TrayCount, bSuppressEvnts: true);

            aAssy.SubPart(this);
            aAssy.GenerateDowncomers(true);
            if (dcCnt > 1) 
            {
                mDeck.PropValSet("ManwayCount", aAssy.DeckSectionWidth(false) > 16 ? 1 : 0, bSuppressEvnts: true);
            }
           // aAssy.GenerateSpoutGroups(true);

            TrayAssembly = aAssy;
        }


        public override uopDocuments Documents(uppDocumentTypes aType = uppDocumentTypes.Undefined)

            => GenerateDocuments(null,null, aType);


        /// <summary>
        /// //^returns the collection of drawings pertinent to this tray range
        /// </summary>
        /// <param name="aProject"></param>
        /// <returns></returns>
        public override uopDocuments Drawings() { int idx = 0; return GenerateDrawings(null, null, null, ref idx); }

        /// <summary>
        ///returns the collection of calculations pertinent to this tray assembly
        /// </summary>
        /// <param name="aProject"></param>
        /// <param name="aCollector"></param>
        public uopDocuments GenerateCalculations(mdProject aProject, uopDocuments aCollector)
        {

            uopDocuments _rVal = aCollector ?? new uopDocuments(); 
            
            aProject = GetMDProject(aProject); 
            if (aProject == null) { return _rVal; }

            aProject.ReadStatus("Generating " + TrayName(true) + " Calculations", 2);

            _rVal.NewMemberCategory = TrayName(true);
            _rVal.NewMemberSubCategory = string.Empty;

            _rVal.AddCalculation(this, "Warnings", "Warnings", uppCalculationType.Warnings);
            if (ProjectType == uppProjectTypes.MDSpout)
            {
                _rVal.AddCalculation(this, "Mechanical Properties", "Mechanical Properties", uppCalculationType.DowncomerProperties);
                _rVal.AddCalculation(this, "Spacing Optimization", "Spacing Optimization", uppCalculationType.MDSpacing);
                _rVal.AddCalculation(this, "Spout Area Distribution", "Spout Area Distribution", uppCalculationType.MDSpoutArea);
                _rVal.AddCalculation(this, "Spout Group Layout", "Spout Group Layout", uppCalculationType.MDSpoutLayout);
                _rVal.AddCalculation(this, "Spout Group Constraints", "Spout Group Constraints", uppCalculationType.Constraints);
                _rVal.AddCalculation(this, "Feed Zones", "Feed Zones", uppCalculationType.FeedZones);
            }
            else
            {
                _rVal.AddCalculation(this, "Part Weights", "Part Weights", uppCalculationType.Weights);
                _rVal.AddCalculation(this, "Downcomer Properties", "Downcomer Properties", uppCalculationType.DowncomerProperties);
            }
            _rVal.NewMemberCategory = string.Empty;
            aProject.ReadStatus("", 2);
            return _rVal;
        }

        public uopDocuments GenerateDocuments(mdProject aProject, uopDocuments aCollector, uppDocumentTypes aType = uppDocumentTypes.Undefined)
        {
            uopDocuments _rVal = aCollector ?? new uopDocuments(); 
            string sname = string.Empty;

            aProject = GetMDProject(aProject);
            if (aProject == null) { return _rVal; }
            
            sname = TrayName(true);
          
            aProject.ReadStatus("Generating " + sname + " Documents", 1);
            if (aType == uppDocumentTypes.Calculation || aType == uppDocumentTypes.Undefined)
            {
                GenerateCalculations(aProject, aCollector: _rVal);
            }
            if (aType == uppDocumentTypes.Drawing || aType == uppDocumentTypes.Undefined)
            {
                int idx = 0;
                GenerateDrawings(aProject, "",  aCollector: _rVal ,ref idx);
            }
            //warnings included only by request
            if (aType == uppDocumentTypes.Warning)
            {
                GenerateWarnings(aProject,aCollector:  _rVal);
            }

            aProject.ReadStatus("", 1);
            return _rVal;

        }

        public override void GetDrawing(uopDocuments aCollector, uppDrawingTypes aType, uppUnitFamilies aUnits)
        {

            if (aCollector == null) return;
            if (aUnits != uppUnitFamilies.Metric) aUnits = uppUnitFamilies.English;
            string sname = $"{Name(false)} - ";
            if (ProjectType == uppProjectTypes.MDSpout)
            {
                switch (aType)
                {
                    case uppDrawingTypes.TestDrawing:
                        {
                            aCollector.AddDrawing(uppDrawingFamily.Design, this, $"{sname} Test", "Test Drawing", uppDrawingTypes.TestDrawing, aUnits: aUnits);
                            break;
                        }
                    case uppDrawingTypes.StartUpLines:
                        {
                            aCollector.AddDrawing(uppDrawingFamily.Design, this, $"{sname} Startup Lines", "Startup Lines", uppDrawingTypes.StartUpLines, aUnits: aUnits);
                            break;
                        }
                    case uppDrawingTypes.BlockedAreas:
                        {
                            if(ProjectType == uppProjectTypes.MDSpout)
                                aCollector.AddDrawing(uppDrawingFamily.Design, this, "Free Bubbling Areas", null, uppDrawingTypes.FreeBubbleAreas, aUnits: uppUnitFamilies.English);
                            else
                                aCollector.AddDrawing(uppDrawingFamily.Design, this, "Blocked Areas", "Blocked Areas", uppDrawingTypes.BlockedAreas, aUnits: uppUnitFamilies.English);
                            break;
                        }
                    case uppDrawingTypes.TraySketch:
                        {
                            aCollector.AddDrawing(uppDrawingFamily.Design, this, $"{sname} Tray Sketch", "Tray Sketch", uppDrawingTypes.TraySketch, aUnits: aUnits);
                            break;
                        }
                    case uppDrawingTypes.FeedZones:
                        {
                            aCollector.AddDrawing(uppDrawingFamily.Design, this, $"{sname} Feed Zones", "Feed Zones", uppDrawingTypes.FeedZones, aUnits: aUnits);
                            break;
                        }
                    case uppDrawingTypes.SectionSketch:
                        {
                            aCollector.AddDrawing(uppDrawingFamily.Design, this, $"{sname} Section Sketch", "Section Sketch", uppDrawingTypes.SectionSketch, aUnits: aUnits);
                            break;
                        }
                    case uppDrawingTypes.DowncomerManholeFit:
                        {
                            aCollector.AddDrawing(uppDrawingFamily.Design, this, $"{sname} Downcomer Manhole Fit", "Downcomer Manhole Fit", uppDrawingTypes.DowncomerManholeFit, aUnits: aUnits);
                            break;
                        }

                      
                }
            }
        }


        public uopDocuments GenerateDrawings(mdProject aProject, string aCategory, uopDocuments aCollector , ref int aSheetIndex)
        {
            //:::DRAWINGS

            uopDocuments _rVal = aCollector ?? new uopDocuments();
            aProject = GetMDProject(aProject);
            if (aProject == null) return _rVal;

            string sname = $"{Name(false)} - ";
            mdTrayAssembly aAssy = TrayAssembly;
            uppUnitFamilies unts = uppUnitFamilies.English;
            try
            {

                _rVal.NewMemberCategory = TrayName(true);
                _rVal.NewMemberSubCategory = string.Empty;

                if (ProjectType == uppProjectTypes.MDSpout)
                {
                    GetDrawing(_rVal, uppDrawingTypes.TestDrawing, aUnits: unts);
                    GetDrawing(_rVal, uppDrawingTypes.InputSketch, aUnits: unts);
                    GetDrawing(_rVal, uppDrawingTypes.StartUpLines, aUnits: unts);
                    GetDrawing(_rVal, uppDrawingTypes.BlockedAreas, aUnits: uppUnitFamilies.English);
                    GetDrawing(_rVal, uppDrawingTypes.TraySketch, aUnits: unts);
                    GetDrawing(_rVal, uppDrawingTypes.SectionSketch, aUnits: unts);
                    GetDrawing(_rVal, uppDrawingTypes.FeedZones, aUnits: unts);
              

                }
                else
                {
                    //================= PARTS =========================================
                    _rVal.NewMemberCategory = "MANUFACTURING";
                    _rVal.NewMemberSubCategory = string.Empty; // TrayName(true);

                    aAssy.GenerateDrawings(aProject, this, _rVal, ref aSheetIndex);
                }
             
            }
            catch (Exception e) 
            {
                Console.WriteLine(e.Message); 
                //UOP.WinTray.UI.Logger.ApplicationLogger.Instance.LogError(e); 
            }
            finally
            {
                _rVal.NewMemberSubCategory = string.Empty;
                _rVal.NewMemberCategory = string.Empty;
            }

            return _rVal;
        }

        /// <summary>
        ///returns the parts associated to the range
        /// </summary>
        /// <returns></returns>
        public colUOPParts GenerateParts()
        {
            colUOPParts _rVal = new colUOPParts();
            eventPartGeneration?.Invoke(true);
            SetReadStatus("Building " + TrayName(true) + " Parts Collection");
            _rVal.SubPart(this);
            _rVal.Clear();
            //explode the top level parts to obtain the detailed parts
            _rVal.Add((mdTrayAssembly)TrayAssembly);
            _rVal.Append(TrayAssembly.Parts);
            eventPartGeneration?.Invoke(false);
            _rVal.Invalid = false;

            return _rVal;
        }



        public override uopDocuments GenerateWarnings(uopProject aProject, string aCategory = null, uopDocuments aCollector = null, bool bJustOne = false)
        {

            uopDocuments _rVal = aCollector ?? new uopDocuments();
            if (bJustOne && _rVal.Count > 0) return _rVal;
            mdProject myProj;
           
            if (aProject == null) { myProj = GetMDProject(); } else { myProj = (aProject.ProjectFamily == uppProjectFamilies.uopFamMD) ? (mdProject)aProject : GetMDProject(); }
            if (myProj == null) { return _rVal; }
            try
            {
                string sname = TrayName(true);
                myProj.ReadStatus($"Generating { sname } Warnings", 1);
                _rVal.NewMemberRangeGUID = GUID;
                _rVal.NewMemberCategory = sname;
                _rVal.NewMemberSubCategory = string.Empty;
                _rVal.NewMemberOwnerName = TrayName();
                if (RingThk <= 0 & ProjectType == uppProjectTypes.MDDraw)
                {
                    _rVal.AddWarning(this, "Ring Thickness Warning", "Ring Thickness Is Zero");
                }
                TrayAssembly.GenerateWarnings(myProj, _rVal, aCategory, bJustOne);
            }
            finally
            {
                _rVal.NewMemberRangeGUID = string.Empty;
                _rVal.NewMemberCategory = string.Empty;
                _rVal.NewMemberOwnerName ="";
                aProject.ReadStatus("", 1);

            }
            return _rVal;
        }
     
        /// <summary>
        /// reads the properties of the tray range from a text file in INI file format
        /// </summary>
        /// <param name="aProject"></param>
        /// <param name="aFileSpec">the file name to read the tray range properties from</param>
        /// <param name="aFileSection"></param>
        /// <param name="inRingStart"></param>
        /// <param name="inRingEnd"></param>
        /// <param name="aWarnings"></param>
        public override void ImportProperties(uopProject aProject, string aFileSpec, string aFileSection, int inRingStart, int inRingEnd, ref uopDocuments ioWarnings )
        {

            try
            {


                ioWarnings ??= new uopDocuments();
                int i = aFileSection.IndexOf("Range(", StringComparison.OrdinalIgnoreCase);
                //chnged if conditon values becuase output diffrence of indexOf function in vb and c#
                if (i == -1) return;

                string Str = mzUtils.Right(aFileSection, aFileSection.Length - i - 6).Trim();
                Str = mzUtils.Left(Str, Str.Length - 1);

                if (!mzUtils.IsNumeric(Str))
                {
                    ioWarnings?.AddWarning(this, "Tray Import Error", $"The Passed File Section Doesn't Contain a Valid Range Index");
                    return;
                }
                uopPropertyArray fileProps = new uopPropertyArray() { Name = aFileSpec };
                fileProps.ReadFromINIFile(aFileSpec);

              
             
                List<string> aHeadings = fileProps.Headings;

                double aFileVersion = fileProps.ValueD("APPINFO", "Version", 0);
                uopPropertyArray myprops = fileProps.PropertiesStartingWith(aFileSpec);

                string prop1 = fileProps.ValueS(  aFileSection, "RingStart", out bool found);
                if (myprops.Count <=0)
                {
                    ioWarnings?.AddWarning(this, "Tray Import Error", $"The Passed File Section Doesn't Contain  Valid Range Data");
                    return;
                }

                ReadProperties(aProject, myprops,ref ioWarnings, aFileVersion, aFileSection, true);

                

            }catch (Exception e) { throw new Exception("[mdTrayRange.ImportProperties] " + e.Message); }
            finally { Reading = false; }
          
        }

        /// <summary>
        /// returns True if the passed range has the same properties as the range object
        /// </summary>
        /// <param name="aRange"></param>
        /// <returns></returns>
        public bool IsEqual(mdTrayRange aRange)
        { 
            if (aRange == null) return false;
            if (aRange.ShellID != ShellID) return false;
            if (aRange.RingID != RingID) return false;
            if (aRange.RingSpacing != RingSpacing) return false;
            if (aRange.DesignFamily != DesignFamily) return false;
            if (aRange.RingThk != RingThk) return false;
            if (aRange.OverrideRingClearance != OverrideRingClearance) return false;
            if (!aRange.TrayAssembly.Deck.IsEqual(TrayAssembly.Deck)) return false;
            if (aRange.TrayAssembly.Downcomer() != TrayAssembly.Downcomer()) return false;
            return true;
        }

        /// <summary>
        /// '^used by an object to respond to property changes of itself and of objects below it in the object model.
        ///also raises the changed property to the object above it it the object model.
        /// </summary>
        /// <param name="aProperty"></param>
        public override void Notify(uopProperty aProperty)
        {
            if (aProperty == null)
            {
                return;
            }
            bool bNotify = false;
            string pname = string.Empty;
            colUOPTrayRanges MyCollection = null;
            Enums.uppPartTypes pType;
            bNotify = true;
            pname = aProperty.Name.ToUpper();
            pType = (uppPartTypes)aProperty.PartType;
            if (pname == "SHELLID" || pname == "RINGID")
            {
                PropValSet("OverrideTrayDiameter", 0, bSuppressEvnts: true);

                if (_TrayAssembly != null) {_TrayAssembly.ResetComponents();}

                if (pname == "SHELLID") {PropValSet("OverrideRingClearance", 0, bSuppressEvnts: true);}
            }
            if (pType == uppPartTypes.TrayRange)
            {
                TrayAssembly.Alert(aProperty);
            }
            if (bNotify)
            {
                eventMDRangePropertyChange?.Invoke(aProperty);
                MyCollection = base.GetTrayRanges() ;
                if (MyCollection != null)
                {
                    MyCollection.NotifyMemberChange(this, aProperty);
                }
            }
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

            
                uopPropertyArray myprops = aFileProps.PropertiesStartingWith(aFileSection);
                if (myprops.Count <= 0)
                {
                    ioWarnings?.AddWarning(this, $"{PartName} Data Not Found", $"File '{ aFileProps.Name }' Does Not Contain {aFileSection} Info!");
                    return;
                }

                //read the ranges properties


                Reading = true;
                string sval;
                uopProperties rangeprops = myprops.Item(aFileSection);
                double dval = 0;

                //trap a bad revamp strategy
                if (rangeprops.Contains("RevampStrategy"))
                {
                    sval = rangeprops.ValueS("RevampStrategy");
                    if (string.IsNullOrWhiteSpace(sval)) rangeprops.SetValue("RevampStrategy", "1-for-1");

                }

                //persist the original GUID
                if (rangeprops.Contains("GUID"))
                {
                    sval = myprops.ValueS(aFileSection, "GUID");
                    if (!string.IsNullOrEmpty(sval)) GUID = sval;
                }
                dval = 0;
                if (rangeprops.Contains("RingClearance")) //aFileVersion < 2.32 
                {
                    dval = rangeprops.ValueD("RingClearance");
                }else if (rangeprops.Contains("OverrideRingClearance"))
                {
                    dval = rangeprops.ValueD("OverrideRingClearance");
                }

                if (dval > 0)
                {
                    double defval = uopUtils.BoundingClearance(myprops.ValueD(aFileSection,"ShellID"));
                    if (dval == defval) dval = 0;
                }
                if (dval < 0) dval = 0;
                if (!rangeprops.Contains("OverrideRingClearance")) //aFileVersion < 2.32 
                    rangeprops.Add(new uopProperty("OverrideRingClearance", dval, uppUnitTypes.SmallLength));
                else
                    myprops.SetValue(aFileSection, "OverrideRingClearance", dval);


                Dictionary<string, string> equalities = new Dictionary<string, string>
                {
                    { "StageList", "StageIndices" }
                };
                List<string> skippers = aSkipList == null ? new List<string>() : new List<string>(aSkipList);
                if (!skippers.Contains("TrayCount")) skippers.Add("TrayCount");
                if (!skippers.Contains("RingWidth")) skippers.Add("RingWidth");

                base.ReadProperties(aProject, myprops, ref ioWarnings, aFileVersion, aFileSection, bIgnoreNotFound, aSkipList: skippers, EqualNames: equalities);
                
                Reading = true;

                string fsec = $"{aFileSection}.TRAYASSEMBLY";
                // === TRAY ASSEMBLY
                if (!myprops.Contains(fsec))
                {
                    ioWarnings?.AddWarning(this, $"{PartName}.TrayAssembly Data Not Found", $"File '{ aFileProps.Name }' Does Not Contain {fsec} Info!");
                    return;
                }

                TrayAssembly.ReadProperties(aProject, myprops, ref ioWarnings, aFileVersion, fsec, bIgnoreNotFound);
            }
            catch (Exception e)
            {
                ioWarnings?.AddWarning(this, "Read Properties Error", e.Message);
            }
            finally
            {
                Reading = false;
                aProject?.ReadStatus("", 2);


            }
        }

        /// <summary>
        /// reads the properties of the tray range object from a text file in INI file format
        /// included as a single point location for reading in tray level properties
        /// </summary>
        /// <param name="aFileSpec">the file name to read the tray range properties from</param>
        /// <param name="aFileVersion"></param>
        /// <param name="aFileSection">the section of the file to read the properties from</param>
        /// <param name="aProject">the project to alert read status</param>
        /// <param name="aFileHeadings">acollection of the defined sections in the passed file</param>
        /// <param name="aWarnings">a collection that///returns any warnings that may be noted during the radeing of the data</param>
        private void ReadRangeProperties(string aFileSpec, string aFileVersion, string aFileSection, uopProject aProject, List<string> aFileHeadings, uopDocuments aWarnings)
        {
            if (aFileHeadings == null) aFileHeadings = uopUtils.GetINIFileHeadingsList(aFileSpec, true);
          
            string Str = string.Empty;
            double aVal = 0;
            double bVal = 0;
            if (aWarnings == null) { aWarnings = new uopDocuments(); }
            if (!string.IsNullOrEmpty(aFileVersion)) {aFileVersion = uopUtils.ReadValue<string>("APPINFO", "Version", "", aFileSpec); }

            Reading = true;
            Str = uopUtils.ReadValue<string>(aFileSection, "GUID", "", aFileSpec);
            if (!string.IsNullOrEmpty(Str))
            {
                GUID = Str;
            }

            TPROPERTIES props = ActiveProps;
            if (props.Count <= 0) InitializeProperties();
            props.SetValue("RingStart", uopUtils.ReadINI_Integer(aFileSpec, aFileSection, "RingStart"));
            props.SetValue("RingEnd", uopUtils.ReadINI_Integer(aFileSpec, aFileSection, "RingEnd"));
            props.SetValue("RingThk", uopUtils.ReadINI_Number(aFileSpec, aFileSection, "RingThk"));
            props.SetValue("ShellID", uopUtils.ReadINI_Number(aFileSpec, aFileSection, "ShellID"));
            props.SetValue("RingID", uopUtils.ReadINI_Number(aFileSpec, aFileSection, "RingID"));
            props.SetValue("RingSpacing", uopUtils.ReadINI_Number(aFileSpec, aFileSection, "RingSpacing",24));
            props.SetValue("DesignFamily", (uppMDDesigns)uopUtils.ReadINI_Integer(aFileSpec, aFileSection, "DesignFamily"));
            props.SetValue("StageList", uopUtils.ReadINI_String(aFileSpec, aFileSection, "StageList"));
            Str = uopUtils.ReadINI_String(aFileSpec, aFileSection, "RevampStrategy", "1-for-1");
            if (string.IsNullOrWhiteSpace(Str)) Str = "1-for-1";
            props.SetValue("RevampStrategy", Str);

            props.SetValue("OverrideTrayDiameter", uopUtils.ReadINI_Number(aFileSpec, aFileSection, "OverrideTrayDiameter")); //3.17 added for bug fix
            props.SetValue("ManholeID", uopUtils.ReadINI_Number(aFileSpec, aFileSection, "ManholeID"));

            props.SetValue("RingWidth", RingWidth);
            props.SetValue("TrayCount", TrayCount);
            uopHardwareMaterial hMat = null;

            Str = uopUtils.ReadValue<string>(aFileSection, "HardwareMaterial", "", aFileSpec);
            if (Str !=  string.Empty)
            {
                hMat = (uopHardwareMaterial)uopGlobals.goHardwareMaterialOptions().GetByMaterialName(uppMaterialTypes.Hardware, Str);
                if (hMat == null) { hMat = (uopHardwareMaterial)uopGlobals.goHardwareMaterialOptions().GetByFamilyName(uppMaterialTypes.Hardware, Str); }
            }
            if (hMat == null) { HardwareMaterial = (uopHardwareMaterial)uopGlobals.goHardwareMaterialOptions().Item(1); }
            else
            { HardwareMaterial = hMat; }

            props.SetValue("HardwareMaterial", RangeHardwareMaterial.MaterialName);

            if (double.TryParse(aFileVersion, out double fileVersion) && fileVersion < 2.32)
            { aVal = uopUtils.ReadINI_Number(aFileSpec, aFileSection, "RingClearance"); }
            else
            { aVal = uopUtils.ReadINI_Number(aFileSpec, aFileSection, "OverrideRingClearance"); }

            //if the read clearance is equal to the current default set the overrie to zero
            if (aVal < 0) aVal = 0;
            if (aVal > 0)
            {
                bVal = uopUtils.BoundingClearance(PropValD("ShellID"));
                if (aVal == bVal) aVal = 0;
            }
            props.SetValue("OverrideRingClearance", aVal);

            ActiveProps = props;

            //PropValSet("RingStart", uopUtils.ReadINI_Integer(aFileSpec, aFileSection, "RingStart"));
            //PropValSet("RingEnd", uopUtils.ReadINI_Integer(aFileSpec,aFileSection, "RingEnd" ));
            ////aProject.ReadStatus("Extracting " + TrayName + " Properties");

            //PropValSet("RingThk", uopUtils.ReadINI_Number(aFileSpec, aFileSection, "RingThk"));
            //PropValSet("ShellID", uopUtils.ReadINI_Number(aFileSpec, aFileSection, "ShellID"));
            //PropValSet("RingID", uopUtils.ReadINI_Number(aFileSpec, aFileSection, "RingID"));
            //PropValSet("RingSpacing", uopUtils.ReadINI_Number(aFileSpec, aFileSection, "RingSpacing",24));
            //PropValSet("DesignFamily", (uppMDDesigns)uopUtils.ReadINI_Integer(aFileSpec, aFileSection, "DesignFamily"));
            //PropValSet("StageList", uopUtils.ReadINI_String(aFileSpec, aFileSection, "StageList"));
            //Str = uopUtils.ReadINI_String(aFileSpec, aFileSection, "RevampStrategy", "1-for-1");
            //if (string.IsNullOrWhiteSpace(Str)) Str = "1-for-1";
            //PropValSet("RevampStrategy", Str);

            //PropValSet("OverrideTrayDiameter", uopUtils.ReadINI_Number(aFileSpec, aFileSection, "OverrideTrayDiameter")); //3.17 added for bug fix
            //PropValSet("ManholeID", uopUtils.ReadINI_Number(aFileSpec, aFileSection, "ManholeID")); 

            //PropValSet("RingWidth", RingWidth);
            //PropValSet("TrayCount", TrayCount);

            //uopHardwareMaterial hMat = null;

            //Str = uopUtils.ReadValue<string>(aFileSection, "HardwareMaterial", "", aFileSpec);
            //if (Str !=  string.Empty)
            //{
            //    hMat = (uopHardwareMaterial)uopGlobals.goHardwareMaterialOptions().GetByMaterialName(uppMaterialTypes.Hardware, Str);
            //    if (hMat == null){hMat = (uopHardwareMaterial)uopGlobals.goHardwareMaterialOptions().GetByFamilyName(uppMaterialTypes.Hardware, Str); }
            //}
            //if (hMat == null){ RangeHardwareMaterial = (uopHardwareMaterial)uopGlobals.goHardwareMaterialOptions().Item(1);}
            //else
            //{ RangeHardwareMaterial = hMat;}

            //PropValSet("HardwareMaterial", RangeHardwareMaterial.MaterialName);

            //if (double.TryParse(aFileVersion, out double fileVersion) && fileVersion < 2.32)
            //{aVal = uopUtils.ReadINI_Number(aFileSpec, aFileSection, "RingClearance"); }
            //else
            //{ aVal = uopUtils.ReadINI_Number(aFileSpec, aFileSection, "OverrideRingClearance"); }

            ////if the read clearance is equal to the current default set the overrie to zero
            //if (aVal < 0)  aVal = 0; 
            //if (aVal > 0) {  
            //    bVal = uopUtils.BoundingClearance(PropValD("ShellID"));
            //    if (aVal == bVal)  aVal = 0; 
            //}
            //PropValSet("OverrideRingClearance", aVal);

            //  aProject.ReadStatus("Extracting " + TrayName + " Notes"); //TODO
            ReadNotes(aFileSpec, aFileSection);
        }

        /// <summary>
        /// returns True if the tray range and it's child tray assembly object can be drawn.
        ///returns an error description in the first argument if the return value = False
        /// </summary>
        /// <param name="ErrString">returns the error description back to the caller if the tray range is not ready to draw</param>
        /// <returns></returns>

        public override bool ReadyToDraw(out string ErrString)  //TODO ERRString is optional ref varable in vb code
        {

            ErrString = string.Empty;
            try
            {

                if (ManholeID <= 0)
                {
                    ErrString = "Invalid Manhole ID Detected";
                    return false;

                }

                if (RingID <= 0)
                {
                    ErrString = "Invalid Ring ID Detected";
                    return false;
                }

                if (ShellID <= 0)
                {
                    ErrString = "Invalid Shell ID Detected";
                    return false;

                }

                if (RingID >= ShellID)
                {
                    ErrString = "Invalid Ring ID Detected";
                    return false;

                }

                return true;
            }
            catch (Exception e) { throw e;  }
            
        }

        /// <summary>
        /// removes the stage number of the passed stage from the list of stages that this range is associated to
        /// </summary>
        /// <param name="aStage">the stage to un-associate to this range</param>
        public void ReleaseStage(mdStage aStage)
        {
            if (aStage == null) return;
           
            List<int> nums = StageIndices;
            bool bHaveIt = nums.Contains(aStage.StageNumber);
            if (nums.Contains(aStage.StageNumber))
            {
                aStage.RangeGUID = string.Empty;
                nums.RemoveAll((x) => x == aStage.StageNumber);
                StageList = mzUtils.ListToString(nums, ",");
            }
          
        }


        /// <summary>
        /// forces the tray to recalculate its components
        /// </summary>
        public void ResetComponents()
        {
            try
            {
                TrayAssembly.ResetComponents();
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        public override void ResetParts() => TrayAssembly.ResetParts();

        /// <summary>
        ///returns the properties required to save the range to file
        /// signatures like "COLOR=RED"
        /// </summary>
        /// <returns></returns>
        public override uopPropertyArray SaveProperties(string aHeading = "")
        {
            UpdateProperties();
            uopProperties _rVal = new uopProperties(CurrentProperties().GetByHidden(false));

            aHeading = string.IsNullOrWhiteSpace(aHeading) ? INIPath : aHeading.Trim();

            _rVal.Add("GUID", RangeGUID, aHeading: aHeading);
            return new uopPropertyArray(_rVal, aName: aHeading, aHeading: aHeading);
      
        }
        private void UpdateProperties()
        {
            PropSetAttributes("TrayCount", bHidden: true);
            PropSetAttributes("RingWidth", bHidden: true);
        }
       
        public override void UpdatePartProperties()
        {
            PropValSet("HardwareMaterial", RangeHardwareMaterial.FriendlyName(), bSuppressEvnts: true);
            PropValSet("RingWidth", RingWidth, bSuppressEvnts: true);
            DescriptiveName = $"{TrayName(true)} ({ TrayCount })";
        }

        public override void UpdatePartWeight() { colUOPParts myParts = Parts; myParts.UpdatePartWeight(); Weight = myParts.Weight; }

        /// <summary>
        /// ^extracts the properties from project and range that are relavent to the stage def and sets the stage properties to match
        /// </summary>
        /// <param name="aStage"></param>
        private void UpdateStageData(mdStage aStage)
        {
            if (aStage == null) return;
            mdProject Proj = null;
            mdTrayAssembly mAssy = TrayAssembly;
            mdDeck mDeck = mAssy.Deck;
            mdDowncomer mDComer = mAssy.Downcomer();
            colMDDowncomers mDComers = mAssy.Downcomers;
    
            
            try
            {
                if (Proj != null)
                {
                    aStage.SetProperty("KeyNumber", Proj.Customer.Name);
                    aStage.SetProperty("CustomerName", Proj.KeyNumber);
                    aStage.SetProperty("FileType", "H");
                    aStage.SetProperty("InstallationLocation", Proj.Customer.Location);
                    aStage.SetProperty("CustomerService", Proj.Customer.Service);
                }
            
                //assembly properties
                aStage.SetProperty("TrayType", DesignFamilyName.ToString());

                //downcomer properties

                aStage.SetProperty("DCSpacing", mDComers.Spacing);
                aStage.SetProperty("WeirHeight", mDComer.How);
                aStage.SetProperty("DCHeight", mDComer.InsideHeight);
                aStage.SetProperty("DCWidth", mDComer.Width);
                aStage.SetProperty("DCCount", mDComer.Count);
                aStage.SetProperty("DCThickness", mDComer.Thickness);

                //range properties

                aStage.SetProperty("ShellID", ShellID / 12);
                aStage.SetProperty("RingWidth", RingWidth);
                aStage.SetProperty("TraySpacing", RingSpacing);
                //aStage.SetProperty ("Clearance", 0);

                //deck properties
                double Fp  = mDeck.Ap;

                if (mDComers.HasTriangularEndPlates)
                {
                    double actv1 =mAssy.FunctionalActiveAreas(false).TotalArea();
                    double actv2 = mAssy.FunctionalActiveAreas(true ,true).TotalArea();
                    double ratio = actv1 / actv2;
                    if (mAssy.DesignFamily.IsBeamDesignFamily())
                    {
                        aStage.SetProperty("LiquidRate", aStage.PropValD("LiquidRate") * ratio );
                        aStage.SetProperty("VaporRate", aStage.PropValD("VaporRate") * ratio);
                    }
                    else
                    {
                        Fp *= ratio;
                    }
                    
                }

                aStage.SetProperty("PerfFraction", Fp / 100);
                aStage.SetProperty("PerfDiameter", mDeck.DP);
                aStage.SetProperty("SlotFraction", mDeck.SlottingPercentage / 100);
                aStage.SetProperty("DeckThickness", mDeck.Thickness);
                aStage.SetProperty("SpoutArea", mAssy.TotalSpoutArea);
                aStage.SetProperty("WeirLength_ft", mDComers.TotalWeirLength / 12);

                aStage.RangeGUID = GUID;
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        ///  updates the properties of the member ranges stages with the info from the range
        /// </summary>
        public List<mdStage> UpdatedStages()
        {
            List<mdStage> aStages = Stages.ToList(bGetClones: false);
            List<mdStage> _rVal = new List<mdStage>();
            try
            {

                foreach (mdStage aStage in aStages)
                {
                    mdStage stg = new mdStage(aStage);
                    UpdateStageData(stg);
                    _rVal.Add(stg);
                }

            }
            catch (Exception e) { throw e; }

            return _rVal;
        }
     
      /// <summary>
        ///the warnings associated to this range
        /// </summary>
        public override uopDocuments Warnings() => GenerateWarnings(null, null, null);
        
       public override uopHoleArray HoleArray(uopTrayAssembly aAssy = null, string aTag = null) { return null; }


        public override bool IsEqual(uopPart aPart)
        {
            if (aPart == null) return false;

            if (aPart.PartType != PartType) return false;
            return IsEqual((mdTrayRange)aPart);
        }
        

        void TrayAssembly_MDAssemblyPropertyChange(uopProperty aProperty) => Notify(aProperty);
       
        /// <summary>
        /// executed internally to create the Tray Design Summary  page(s) for a MD Spout project
        /// </summary>
        /// <param name="aTableIndex"></param>
        /// <returns></returns>
        public uopProperties ReportProperties(int aTableIndex)
        {
            uopProperties _rVal = new uopProperties();
            mdTrayAssembly aAssy = TrayAssembly;
            mdDowncomer DComer = aAssy.Downcomer();
            colMDDowncomers DComers = aAssy.Downcomers;
            mdDeck Deck = aAssy.Deck;
            mdDesignOptions DOps = aAssy.DesignOptions;
            mdProject Proj = GetMDProject();
            dynamic aVal = null;
           
            int lVal = 0;
            
            if (aTableIndex == 3)
            {
                _rVal.Add("Section", Index);
                _rVal.Add("Tray Numbers", SpanName());
                _rVal.Add( "Weir Load (Ql/Bw)", "");
                _rVal.Add("Weir Load Max", "");
                _rVal.SetUnitCaption("CFS/ft", 3, 4);
                _rVal.Add("F-factor", "", uppUnitTypes.Velocity);
                _rVal.Add("F-factor Max", "", uppUnitTypes.Velocity);
                _rVal.Add("Froth Height", "", uppUnitTypes.SmallLength);
                _rVal.Add("ECMD Froth Height", "", uppUnitTypes.SmallLength);
                _rVal.Add("HfoVs", "");
                _rVal.Add("Margin of Safety", "", uppUnitTypes.SmallLength);
                _rVal.Add("Z Height", "", uppUnitTypes.SmallLength);
                _rVal.Add("Entrainment", "", uppUnitTypes.Percentage);
                _rVal.Add("Downcomer Velocity", "",uppUnitTypes.Velocity);
              
                _rVal.Add("Tray Pressure Drop", "",uppUnitTypes.Pressure);
            
                _rVal.Add("Liquid Density", "", uppUnitTypes.Density);
                _rVal.Add("Vapor Density", "", uppUnitTypes.Density);
                _rVal.Add("Density Difference", "", uppUnitTypes.Density);
                _rVal.Add("Surface Tension", "", uppUnitTypes.SurfaceTension);
                _rVal.Add("Applied Efficiency", "", uppUnitTypes.Percentage);
                _rVal.Add("Calculated Efficiency", "", uppUnitTypes.Percentage);
                _rVal.Add("System Factor", "");
                _rVal.Add("Alpha D", "");
                _rVal.Add("Alpha R", "");
                _rVal.Add("Stability Factor", "");
                _rVal.Add("Fluidization Factor", "");
                _rVal.Add("Stability @ Turndown", "");
                _rVal.Add("Spout Loss @ Turndown", "", uppUnitTypes.SmallLength);
                _rVal.Add("Weep @ Turndown", "", uppUnitTypes.Percentage);
            }
            else if (aTableIndex == 2)
            {
                _rVal.Add("Section", Index);
                _rVal.Add("Tray Numbers", SpanName());
                _rVal.Add("Tray Count", TrayCount);
                _rVal.Add("Tray Type", aAssy.FamilyName);
                _rVal.Add("Number of Downcomers", DComer.Count);
                _rVal.Add("Downcomer Type", "ND");
                _rVal.Add("Column Inner Diameter", ShellID, uppUnitTypes.SmallLength);
                _rVal.Add("Panel Width", aAssy.FunctionalPanelWidth, uppUnitTypes.SmallLength);
                _rVal.Add("Weir Height", DComer.How, uppUnitTypes.SmallLength);
                _rVal.Add("Downcomer Width", DComer.Width, uppUnitTypes.SmallLength);
                _rVal.Add("Downcomer Height", DComer.InsideHeight, uppUnitTypes.SmallLength);
                _rVal.Add("Tray Spacing", RingSpacing, uppUnitTypes.SmallLength);

                if (!aAssy.DesignFamily.IsEcmdDesignFamily())
                {
                    _rVal.Add("CDP Height", "-");
                }
                else
                {
                    _rVal.Add("CDP Height", DOps.CDP, uppUnitTypes.SmallLength);
                }

                _rVal.Add("Spout Area", aAssy.TotalSpoutArea, uppUnitTypes.SmallArea);
                _rVal.Add("Deck Perforation Fraction", Deck.Ap, uppUnitTypes.Percentage);
                _rVal.Add("Perforation Diameter", Deck.DP, uppUnitTypes.SmallLength);

                if (!DesignFamily.IsEcmdDesignFamily())
                {
                    _rVal.Add("Fraction Slotting", "-");
                    _rVal.Add("Slot Type", "-");
                }
                else
                {
                    _rVal.Add("Fraction Slotting", Deck.SlottingPercentage, uppUnitTypes.Percentage);
                   _rVal.Add("Slot Type", Deck.SlotType.GetDescription() );
                }

                if (DOps.HasAntiPenetrationPans)
                {
                    _rVal.Add("Addition Device", "APP");
                    _rVal.Add("Height Above Tray Deck", DOps.FEDorAPPHeight, uppUnitTypes.SmallLength);
                }
                else if (DOps.HasFlowEnhancement)
                {
                    _rVal.Add("Addition Device", "FED");
                    _rVal.Add("Height Above Tray Deck", DOps.FEDorAPPHeight, uppUnitTypes.SmallLength);
                }
                else
                {
                    _rVal.Add("Addition Device", "No");
                    _rVal.Add("Height Above Tray Deck", "-");
                }

            }
            else if (aTableIndex == 1)
            {
                mdStartupSpouts aSUs = aAssy.StartupSpouts;

                int suCount = aSUs.TotalCount;
                double fArea = aAssy.FunctionalActiveArea;

                //1
                _rVal.Add("Section", Index);
                //2
                _rVal.Add("Tray Numbers", SpanName());
                //3
                _rVal.Add("Number of Trays", TrayCount);
                //4
                _rVal.Add("Tray Type", aAssy.DesignFamilyName);
                aVal = (Proj.InstallationType != uppInstallationTypes.GrassRoots) ? RevampStrategy : "-";
                //5
                _rVal.Add("Revamp Strategy", aVal);
                //6
                _rVal.Add("Column Inner Diameter", ShellID, uppUnitTypes.SmallLength);
                //7
                _rVal.Add("Ring Inner Diameter", RingID, uppUnitTypes.SmallLength);
                //8
                _rVal.Add("Bolt Circle Diameter", 2 * aAssy.RingClipRadius, uppUnitTypes.SmallLength);
                //9
                _rVal.Add("Ring Width", RingWidth, uppUnitTypes.SmallLength);
                //10
                _rVal.Add("Tray Spacing", RingSpacing, uppUnitTypes.SmallLength, aPrecision: 2);
                //11
                _rVal.Add("Number of Downcomers", DComer.Count);
                //12
                _rVal.Add("Downcomer Spacing", DComers.Spacing, uppUnitTypes.SmallLength);
                //13
                _rVal.Add("Weir Height", DComer.How, uppUnitTypes.SmallLength);
                //14
                _rVal.Add("Downcomer Width", DComer.Width, uppUnitTypes.SmallLength);
                //15
                _rVal.Add("Downcomer Height", DComer.InsideHeight, uppUnitTypes.SmallLength);
                //16
                _rVal.Add("DC Material Thickness", DComer.Thickness, uppUnitTypes.SmallLength);

                aVal = (DComer.Material.Family != uppMetalFamilies.Unknown) ? ((uopSheetMetal)DComer.Material).FamilySelectName : "";
                if (aVal ==  string.Empty) aVal = !DComer.Material.IsStainless ? "CS" : "SS";
               
                //17
                _rVal.Add("Downcomer Material", aVal);
                //18
                _rVal.Add("DC to Ring Clearance", RingClearance, uppUnitTypes.SmallLength);
                //19
                _rVal.Add("Spout Diameter", DComer.SpoutDiameter, uppUnitTypes.SmallLength);
                //20
                _rVal.Add("Downcomer Spout Area", aAssy.TotalSpoutArea, uppUnitTypes.SmallArea);
                //21
                _rVal.Add("Total Weir Length", DComers.TotalWeirLength / 12, uppUnitTypes.BigLength);
                //22
                _rVal.Add("Total Downcomer Area", DComers.TotalBottomArea / 144, uppUnitTypes.BigArea);

                aVal = suCount;
                if (suCount == 0) aVal = "-";
                
                //23
                _rVal.Add("Number Of Startup Spouts", aVal);
                if (suCount == 0)
                { aVal = "-"; }
                else
                {  aVal = DComer.StartupDiameter; }
                //24
                _rVal.Add("Startup Spout Height", aVal, uppUnitTypes.SmallLength);
                if (suCount == 0)
                { aVal = "-"; }
                else
                { aVal = aSUs.Length; }

                //25
                _rVal.Add("Startup Spout Length", aVal, uppUnitTypes.SmallLength);
                if (suCount == 0)
                { aVal = "-"; }
                else
                { aVal = aAssy.StartupArea(true, aSUs); }

                //26
                _rVal.Add("Area Per Startup Spout", aVal, uppUnitTypes.SmallArea);
                if (suCount == 0)
                { aVal = "-"; }
                else
                { aVal = aAssy.StartupArea(false, aSUs); }

                //27
                _rVal.Add("Total Startup Spout Area", aVal, uppUnitTypes.SmallArea);
                //28
                _rVal.Add("Perforation Diameter", Deck.DP, uppUnitTypes.SmallLength);
                //29
                _rVal.Add("Functional Active Area", fArea / 144, uppUnitTypes.BigArea);
                //30
                _rVal.Add("Functional Perforation Area", Deck.Ap, uppUnitTypes.Percentage);
                //31
                _rVal.Add("Area of Perforations", fArea * (Deck.Ap / 100) / 144, uppUnitTypes.BigArea);
                aVal = 0;
                if (Deck.DP > 0)
                {
                    aVal = fArea * (Deck.Ap / 100);
                    aVal /= (Math.Pow(Deck.DP / 2, 2) * Math.PI);
                    aVal = Math.Round(aVal, 0);
                }
                //32
                _rVal.Add("Number of Perforations", aVal);
                //33
                _rVal.Add("Functional Panel Width", aAssy.FunctionalPanelWidth, uppUnitTypes.SmallLength);
                //34
                _rVal.Add("Free Bubbling Area", aAssy.FreeBubblingAreas.TotalFreeBubblingArea / 144, uppUnitTypes.BigArea);
                //35
                _rVal.Add("R-FACTOR", "");
                //36
                _rVal.Add("Tiled Decks", mzUtils.BooleanToString(DOps.HasTiledDecks, true));
                //37
                _rVal.Add("Bubble Promoters", mzUtils.BooleanToString(DOps.HasBubblePromoters, true));
               
                //38
                _rVal.Add("Deck Thickness", Deck.Thickness, uppUnitTypes.SmallLength);

                if (Deck.SheetMetalFamily != uppMetalFamilies.Unknown)
                {
                    aVal = Deck.Material.FamilySelectName;
                }
                else
                {
                    aVal= !Deck.Material.IsStainless ? "CS":"SS";
                }
                //39
                _rVal.Add("Deck Material", aVal);
                //40
                _rVal.Add("Number of Manways", Deck.ManwayCount);

                aVal = aAssy.DesignFamily.IsEcmdDesignFamily() ? Deck.SlottingPercentage.ToString() : "-";
               
                //41
                _rVal.Add("Functional Slotting", aVal, uppUnitTypes.Percentage);
              
                //42
                aVal = aAssy.DesignFamily.IsEcmdDesignFamily() ? Deck.SlotType.GetDescription() : "-";

                _rVal.Add("Slot Type", aVal);
                if (aAssy.DesignFamily.IsEcmdDesignFamily())
                {
                    aAssy.PanelSlotCounts(out lVal, out double _, out double _, out double _);
                    aVal = Convert.ToString(lVal);
                }
                else
                { aVal = "-"; }

                //43
                _rVal.Add("Number of Slots", aVal);
                aVal = aAssy.DesignFamily.IsEcmdDesignFamily() ? DOps.CDP.ToString() : "-";
               
                //44
                _rVal.Add("CDP Height", aVal, uppUnitTypes.SmallLength);
                if (DOps.HasAntiPenetrationPans)
                { aVal = "APP"; }
                else if (DOps.HasFlowEnhancement)
                { aVal = "FED"; }
                else
                { aVal = "-"; }

                //45
                _rVal.Add("Additional Devices", aVal);
                aVal = (DOps.HasAntiPenetrationPans || DOps.HasFlowEnhancement) ? DOps.FEDorAPPHeight.ToString() : "-";
                //46
                _rVal.Add("Height Above Tray Deck", aVal, uppUnitTypes.SmallLength);
                aVal = HardwareMaterial.FamilySelectName;
                //47
                _rVal.Add("Fastener Material", aVal);
                aVal = (Bolting == uppUnitFamilies.English) ? "UNC" : "METRIC";
                //48
                _rVal.Add("Fastener Thread Type", aVal);

                _rVal.SetHeadings("Section 1", 1, 1);
                _rVal.SetHeadings("Section 2", 2, 9);
                _rVal.SetHeadings("Section 3", 10, 21);
                _rVal.SetHeadings("Section 4", 22, 26);
                _rVal.SetHeadings("Section 5", 27, 40);
                _rVal.SetHeadings("Section 6", 41, 44);
                _rVal.SetHeadings("Section 7", 45, 46);
                _rVal.SetHeadings("Section 8", 47, 100);

                _rVal.SetProtected(true);
                _rVal.SetProtected(false, 4, 4);
                _rVal.SetProtected(false, 34, 34);
                _rVal.Item("R-FACTOR").Protected = false;
            }

            return _rVal;
        }
        public double ColumnArea() => Math.PI * Math.Pow(ShellID, 2);


        public override void Destroy()
        {
            if (!disposedValue)
            {
                _TrayAssembly.PropertyChangeEvent -= TrayAssembly_MDAssemblyPropertyChange;
                //uopEventHandler.eventRangeRequest -= _Events_RangeRequest;

                _TrayAssembly.Destroy();
                _TrayAssembly = null;
                _Events = null;
             
                disposedValue = true;
            }
        }

        /// <summary>
        /// make sure all persistent sub parts are up to date
        /// </summary>
        public override void UpdatePersistentSubParts(uopProject aProject, bool bForceRegen = false)
        {
            mdProject project = aProject == null ? GetMDProject() : aProject as mdProject;
           TrayAssembly.UpdatePersistentSubParts(project, this, bForceRegen);


        }

        #endregion Methods

    }
}