using UOP.DXFGraphics;
using System;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Materials;
using UOP.WinTray.Projects.Utilities;
using static System.Math;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;
using System.ComponentModel;

namespace UOP.WinTray.Projects.Parts
{
    /// <summary>
    /// MD deck object which carries all of the user input data that controls the deck design for a tray range
    ///'~all mdTrayAssembly objects have a mdDeck object property
    /// </summary>
    public class mdDeck : uopPart
    {
        public override uppPartTypes BasePartType => uppPartTypes.Deck;
        public delegate void MDDeckPropertyChange(uopProperty aProperty);
        public event MDDeckPropertyChange PropertyChangeEvent;
        //public delegate void MDDeckInvalidatedHandler();
        //public event MDDeckInvalidatedHandler eventMDDeckInvalidated;
        public delegate void MaterialChange(uopSheetMetal NewMaterial);
        public event MaterialChange EventMaterialChange;
   
     
        #region Constructors

        public mdDeck() : base(uppPartTypes.Deck, uppProjectFamilies.uopFamMD, "","",true) 
        { 
            InitializeProperties();
            //base.PartEvent += PartEvent;
            base.PerforationDiameter = DP;

        }



        internal mdDeck(mdDeck aPartToCopy, uopPart aParent = null) : base(uppPartTypes.Deck, uppProjectFamilies.uopFamMD, "", "", true)
        {
            if (aPartToCopy == null) { InitializeProperties(); } else { base.Copy(aPartToCopy); }
            SubPart(aParent);
            base.PerforationDiameter = DP;
        }


        public mdDeck(string aProjectHandle = "", string aRangeGUID = null) : base(uppPartTypes.Deck, uppProjectFamilies.uopFamMD, aProjectHandle, aRangeGUID)
        {
            InitializeProperties();
            //base.PartEvent += PartEvent;
            base.PerforationDiameter = DP;

        }


        private void InitializeProperties()
        {
          
            //eventPartEvent += OPart_eventPartEvent;

            AddProperty("Fp", 0, aDisplayName: "Percent Open (Fp)", aUnitType: uppUnitTypes.Percentage);
            AddProperty("PerfDiameter", 0.1875, aDisplayName: "Perforation Diameter",aUnitType: uppUnitTypes.SmallLength);
            AddProperty("PitchType", dxxPitchTypes.Triangular, bIsHidden: true, aDecodeString: "1=Triangular,2=Rectangular,3=Triangular");
            AddProperty("ManwayCount", 0, aDisplayName: "Manways");
            AddProperty("PunchDirection", uppPunchDirections.Either, aDecodeString: "0=Either,1=From Above,2=From Below");
            AddProperty("SlottingPercentage", 0.4, aDisplayName:"Slotting Pct. (Fs)", aUnitType:uppUnitTypes.Percentage);
            AddProperty("SlotType",  uppFlowSlotTypes.HalfC , aDisplayName:"Slot Type", aDecodeString: "1=Full C,2=Half C");
            AddProperty("Material", "", aDisplayName: "Deck Mtrl.");
            AddProperty("MaxVLError", 0, aDisplayName: "Max V/L Err");
            SheetMetal = uopGlobals.goSheetMetalOptions().GetByFamilyAndGauge(uppMetalFamilies.CarbonSteel, uppSheetGages.Gage12);
            PropValSet("Material", SheetMetal.Descriptor,bSuppressEvnts:true);

        }


        #endregion
      

        public override string ToString() { return "MD DECK"; }

        public new uopSheetMetal Material { get => SheetMetal; set { SheetMetal = value; if (value != null) { PropValSet("Material", value.Descriptor, bSuppressEvnts: true); } } }

        /// <summary>
        /// the percent open area for the deck panels
        /// same as Fp property was renamed but access remains for support of old files
        /// </summary>
        public double Ap { get =>Fp; set => Fp = value; }

        /// <summary>
        ///returns the objects properties in a collection
        /// </summary>
        public override uopProperties CurrentProperties() { UpdatePartProperties(); return base.CurrentProperties(); }

        // <summary>
        // sets the parts property with the passed name to the passed value
        //returns the property if the property value actually changes.
        // </summary>
        public override uopProperty PropValSet(string aName, object aPropVal, int aOccur = 0, bool? bSuppressEvnts = null, bool? bHiddenVal = null)
        {

            bool supevnts = bSuppressEvnts.HasValue ? bSuppressEvnts.Value : SuppressEvents || Reading;
            uopProperty _rVal = base.PropValSet(aName, aPropVal, aOccur, supevnts, bHiddenVal);
            if (_rVal == null || supevnts) return _rVal;
            Notify(_rVal);
            return _rVal;
        }
      
        /// <summary>
        /// the diameter of the perforation holes in the deck panels
        /// </summary>
        public double DP { get => PropValD("PerfDiameter"); set { if (Math.Abs(value) > 0)  PropValSet("PerfDiameter", Math.Abs(value)); base.PerforationDiameter = PropValD("PerfDiameter"); } }

        public override double PerforationDiameter { get => DP; set => DP = value; }


        /// <summary>
        /// used by an object to respond to property changes of itself and of objects below it in the object model.
        /// also raises the changed property to the object above it it the object model.
        /// </summary>
        /// <param name="aProperty"></param>
        private void Notify(uopProperty aProperty) { if (aProperty == null || aProperty.Protected) return; PropertyChangeEvent?.Invoke(aProperty); }
       

        /// <summary>
        /// the percent open area for the deck panels
        /// </summary>
        public double Fp { get => PropValD("Fp"); set { Notify(PropValSet("Fp", Abs(value))); base.PercentOpen = PropValD("Fp"); } }
        
        public override double PercentOpen { get => Fp; set => Fp =value; }

        public override string INIPath => $"COLUMN({ ColumnIndex }).RANGE({ RangeIndex }).TRAYASSEMBLY.DECK";
       
        public int ManwayCount { get => PropValI("ManwayCount"); set =>  PropValSet("ManwayCount", value); }


        /// <summary>
        /// the numeric value of the pitch type for the perforation layout
        /// </summary>
        public dxxPitchTypes PitchType
        {
            get => (dxxPitchTypes)PropValI("PitchType");
            set => PropValSet("PitchType", value);
        }
        /// <summary>
        /// the pitch type as a string (like "Triangular")
        /// </summary>
        public string PitchTypeName => dxfEnums.Description(PitchType);
      


        /// <summary>
        /// the direction that deck perforations should be punched from
        /// </summary>
        public uppPunchDirections PunchDirection {
            get => (uppPunchDirections)PropValI("PunchDirection");
            set => PropValSet("PunchDirection", value);
        }

        
        /// <summary>
        ///returns the objects properties in a collection
        /// </summary>
        public override  uopPropertyArray SaveProperties(string aHeading = "")
        {
            aHeading = string.IsNullOrWhiteSpace(aHeading) ? INIPath : aHeading;
              uopProperties _rVal = new uopProperties(base.ActiveProps, aHeading);
            _rVal.RemoveByName("MaxVLError,Thickness");
            //_rVal.Add("Thickness", Thickness, uppUnitTypes.SmallLength,aHeading : aHeading);
            return new uopPropertyArray( _rVal ,aName: aHeading, aHeading: aHeading);
        }


        /// <summary>
        /// the area of a single flow slot in sqr. inches
        /// </summary>
        public double SlotArea => SlotType.SlotArea();

        /// <summary>
        /// the type of flow slots to use on the tray decks
        /// </summary>
        public uppFlowSlotTypes SlotType
        {
            get
            {
                uppFlowSlotTypes _rVal;
                _rVal = (uppFlowSlotTypes)PropValI("SlotType");
                if (_rVal != uppFlowSlotTypes.FullC && _rVal != uppFlowSlotTypes.HalfC)
                {
                    PropValSet("SlotType", uppFlowSlotTypes.FullC, bSuppressEvnts: true);
                    _rVal = uppFlowSlotTypes.FullC;
                }

                return _rVal;
            }
            set
            {
                if (value == uppFlowSlotTypes.FullC || value == uppFlowSlotTypes.HalfC)
                { PropValSet("SlotType", value); }

            }
        }


        /// <summary>
        /// the functional slotting percentage (Fs)
        /// </summary>
        public double SlottingPercentage { get => PropValD("SlottingPercentage"); set => PropValSet("SlottingPercentage", Abs(value)); }

     
      
        /// <summary>
        /// Event Handler for Part chnaged event updates Material data.
        /// </summary>
        /// <param name="aType"></param>
        /// <param name="aValue"></param>
        /// <param name="bValue"></param>
        /// <param name="aObject"></param>
        private void OPart_eventPartEvent(uppPartEventTypes aType, dynamic aValue, dynamic bValue, object aObject)
        {
            if (aType == uppPartEventTypes.SheetMetalChange1)
            {
                Notify(CurrentProperty("Material"));
                EventMaterialChange?.Invoke(aObject as uopSheetMetal);
            }
        }

        /// <summary>
        ///returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>
        public mdDeck Clone() => new mdDeck(this);

        public override uopPart Clone(bool aFlag = false) => (uopPart)this.Clone();

        public double MaxVLError {  get { mdTrayAssembly aAssy = GetMDTrayAssembly(); return (aAssy !=null) ? aAssy.DeckPanels.MaxVLError(aAssy) : 0; } }

        /// <summary>
        /// returns True if the passed deck has the same properties as the deck object
        /// </summary>
        /// <param name="aDeck"></param>
        /// <returns></returns>
        public bool IsEqual(mdDeck aDeck) => aDeck != null && CurrentProperties().IsEqual(aDeck.CurrentProperties());
        public override void UpdatePartProperties()
        {
            PropValSet("Material", Material.Descriptor, bSuppressEvnts: true);
        }

        public override void UpdatePartWeight()  { base.Weight = 0; }

        public override uopDocuments Warnings() => GenerateWarnings(null);
        
        /// <summary>
        ///returns a collection of strings that are warnings about possible problems with the current tray assembly design.
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aCategory"></param>
        /// <returns></returns>
        public  uopDocuments GenerateWarnings(mdTrayAssembly aAssy, string aCategory = null,uopDocuments aCollector = null, bool bJustOne = false)
        {


          
            uopDocuments _rVal = aCollector ?? new uopDocuments();
            if (bJustOne && _rVal.Count > 0) return _rVal;
            aAssy = base.GetMDTrayAssembly(aAssy);
            if (aAssy == null) return _rVal;

            double d1 = 0;
            int mcnt = 0;
            aCategory = string.IsNullOrWhiteSpace(aCategory) ? TrayName() : aCategory.Trim();

            if (Fp <= 0)
            {
                _rVal.AddWarning(this, "Fp% Warning", "The Deck Panel Open Area Percentage (Fp%) = 0", aOwnerName: "Deck");
                if (bJustOne && _rVal.Count > 0) return _rVal;
            }



                if (aAssy.DesignFamily.IsEcmdDesignFamily())
            {
                if (SlottingPercentage <= 0)
                {
                    _rVal.AddWarning(this, "Slotting Warning", "The Requested Functional ECMD Slotting Percentage Has Not Been Defined", aOwnerName: "Deck");
                    if (bJustOne && _rVal.Count > 0) return _rVal;
                }
            }

            if (aAssy != null)
            {
                if (aAssy.ProjectType == uppProjectTypes.MDSpout)//todo && aAssy.Downcomer.Count() > 1)
                {
                    d1 = aAssy.DeckSectionWidth(false);
                    mcnt = ManwayCount;
                    if (d1 > 16)
                    {
                        if (mcnt <= 0)
                        {
                            _rVal.AddWarning(this, "Manway Count Warning", "No Manways Have Been Specified But The Current Deck Panel Width Allows For Manways", aOwnerName: "Deck");
                            if (bJustOne && _rVal.Count > 0) return _rVal;
                        }
                    }
                    else
                    {
                        if (mcnt > 0)  _rVal.AddWarning(this, "Manway Count Warning", "Manways Have Been Requested But The Current Deck Panel Width Is Too Small (" + mzUtils.Format(d1, "0.0##") + "''<=16'') To Accomodate Manways", aOwnerName: "Deck");
                        if (bJustOne && _rVal.Count > 0) return _rVal;
                    }

                }

            }
            return _rVal;
        }



        public override bool IsEqual(uopPart aPart)
        {
           
            if (aPart == null)  return false;

          
            if (aPart.PartType != PartType) return false;

            if (!this.CurrentProperties().IsEqual(aPart.CurrentProperties())) return false;
            return this.SheetMetal.IsEqual(aPart.SheetMetal);
        }

    }
}

