using System;
using System.Collections.Generic;
using UOP.DXFGraphics;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;

namespace UOP.WinTray.Projects.Parts
{
    public class mdDesignOptions : uopPart
    {
        public override uppPartTypes BasePartType => uppPartTypes.DesignOptions;

        #region Variables

        //!!PRIMARY
        //^MD design options object carries data related to the mechanical design of a MD tray assembly
        //~all mdTrayAssembly objects have a mdDesignOptions object property.
        // Option Explicit
        //@raised when the deck data is changed
        public delegate void MDDesignOptionsPropertyChange(uopProperty aProperty);
        public event MDDesignOptionsPropertyChange PropertyChangeEvent;
       


        #endregion

        #region Constructors
        public mdDesignOptions() : base(uppPartTypes.DesignOptions, uppProjectFamilies.uopFamMD, "", "", true) { InitializeProperties(); }

        internal mdDesignOptions(mdDesignOptions aPartToCopy, uopPart aParent = null) : base(uppPartTypes.DesignOptions, uppProjectFamilies.uopFamMD, "", "", true) {
            if (aPartToCopy == null) { InitializeProperties(); } else { base.Copy(aPartToCopy); }
            SubPart(aParent);
        }

        private void InitializeProperties()
        {

         
            AddProperty("WeldedStiffeners", true);
            AddProperty("UseManwayClips", true, aDisplayName: "Manway Fstnr.");
            AddProperty("MaxRingClipSpacing", mdGlobals.DefaultRingClipSpacing, aUnitType: uppUnitTypes.SmallLength, aDisplayName: "Ring Clip Spc.");
            AddProperty("MoonRingClipSpacing", mdGlobals.DefaultRingClipSpacing, aUnitType: uppUnitTypes.SmallLength, aDisplayName: "Moon RC Spc.");
            AddProperty("StiffenerSpacing", 18, aUnitType: uppUnitTypes.SmallLength, aDisplayName: "Stiffener Spc.");
            AddProperty("JoggleBoltSpacing", 6, aUnitType: uppUnitTypes.SmallLength, aDisplayName: "Joggle Bolt Spc.");
            AddProperty("JoggleAngle", 1, uppUnitTypes.SmallLength, aDisplayName: "Joggle Height");
            AddProperty("SpliceAngle", 2, uppUnitTypes.SmallLength, aDisplayName: "Splice Angle Height");
            AddProperty("SpliceStyle", uppSpliceStyles.Tabs, aDisplayName: "Splice Style", aDecodeString: "-1=Undefined,0=Slot & Tab,1=Angles,2=Joggles");
            AddProperty("BaffleMountPercentage", 50, uppUnitTypes.Percentage, aDisplayName: "Baffle Mount");
            AddProperty("APPanPerfDiameter", 0.375, uppUnitTypes.SmallLength, aDisplayName: "AP Pan Perf. Dia.");
            AddProperty("APPanPercentOpen", 10, uppUnitTypes.Percentage, aDisplayName: "AP Pan % Open");
           
            AddProperty("CrossBraces", false, aDisplayName: "Cross Braces");
            AddProperty("DoubleNuts", false, aDisplayName: "Double Nuts");
            AddProperty("CDP", 0, uppUnitTypes.SmallLength, aDisplayName: "Baffle Ht.");
            AddProperty("FlowDeviceType", uppFlowDevices.None,  aDisplayName: "Devices", aDecodeString: "0=None,1=AP Pans,2=FED");
            AddProperty("FEDorAPPHeight", 0, uppUnitTypes.SmallLength, aDisplayName: "Device Clrc.");
            AddProperty("HasTiledDecks", false, aDisplayName: "Tiled Decks");
            AddProperty("HasBubblePromoters", true, aDisplayName: "Bubble Promoters");
            AddProperty("BottomInstall", false, aDisplayName: "Bottom Install");
            AddProperty("BottomDCHeight", 0, aDisplayName: "Bottom DC Height", aUnitType: uppUnitTypes.SmallLength, aNullVal: 0, bOptional: true);

        }
        #endregion

        public override string ToString() { return "DESIGN OPTIONS"; }

        #region Properties

        public uppFlowDevices FlowDeviceType { get => (uppFlowDevices)PropValI("FlowDeviceType"); set { if (value >= 0 & (int)value <= 2) PropValSet("FlowDeviceType", value); } }

        public bool BottomInstall { get => PropValB("BottomInstall"); set => PropValSet("BottomInstall", value); }

        public string Devices {get { if (HasAntiPenetrationPans) return "AP Pans"; return HasFlowEnhancement ? "FED": "None"; } }

        public double APPanPercentOpen { get => PropValD("APPanPercentOpen"); set => PropValSet("APPanPercentOpen", value); }

        //^the size of the perforations in an ap pan if pans are required
        public double APPanPerfDiameter { get => PropValD("APPanPerfDiameter"); set => PropValSet("APPanPerfDiameter", value); }

        //^the percentage of the baffle height that is attached to the mounting stiffener bracket

        public double BaffleMountPercentage { get => PropValD("BaffleMountPercentage"); set => PropValSet("BaffleMountPercentage", value); }

        /// <summary>
        ///Baffle spacing
        /// </summary>
        /// <returns></returns>
        public double CDP { get { double _rVal = PropValD("CDP");  if (_rVal <= 0) { double defval = mdUtils.DefaultCDP(GetMDRange()); return defval; }  return PropValD("CDP");  }  set => PropValSet("CDP", Math.Abs(value)); }

        /// <summary>
        ///returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>
        public mdDesignOptions Clone() => new mdDesignOptions(this);

        public override uopPart Clone(bool aFlag = false) => (uopPart)this.Clone();


        //^flag to indicate if cross braces are required for the tray
        public bool CrossBraces { get => PropValB("CrossBraces"); set => PropValSet("CrossBraces", value); }

        //^returns the objects properties in a collection
        //~signatures like "COLOR=RED"
         public override uopProperties CurrentProperties()  { UpdatePartProperties(); return base.CurrentProperties(); }

        public override uopProperty CurrentProperty(string aPropertyName, bool bSupressNotFoundError) { UpdatePartProperties(); return base.CurrentProperty(aPropertyName, bSupressNotFoundError); }

        //^flag to indicate if double nuts are used rather that nuts and lock washers
        public new bool DoubleNuts { get => PropValB("DoubleNuts"); set => PropValSet("DoubleNuts", value); }

        //^the height that the FED or AP pans should clear the deck of the tray below
        public double FEDorAPPHeight { get => PropValD("FEDorAPPHeight"); set => PropValSet("FEDorAPPHeight", value); }

        //^flag indicating if the deck includes anti-penetration pans
        public bool HasAntiPenetrationPans  => FlowDeviceType == uppFlowDevices.APPans; 

        //^Flag indicating if the deck has bubble promoters
        public bool HasBubblePromoters { get => PropValB("HasBubblePromoters");  set => PropValSet("HasBubblePromoters", value); }

        //^flag indicating if the deck includes flow enhancement devices
        public virtual bool HasFlowEnhancement => FlowDeviceType == uppFlowDevices.FED;

        //^flag indicating if the deck includes tiled decks 
        public bool HasTiledDecks { get => PropValB("HasTiledDecks"); set => PropValSet("HasTiledDecks", value); }

        public override string INIPath => $"COLUMN({ ColumnIndex }).RANGE({RangeIndex }).TRAYASSEMBLY.DESIGNOPTIONS";

        //the height of the angle bent in a joggled panel splice
        public double JoggleAngle { get => PropValD("JoggleAngle"); set => PropValSet("JoggleAngle", value); }

        //^the height of the angle bent in a joggled panel splice
        public double SpliceAngle { get => PropValD("SpliceAngle"); set => PropValSet("SpliceAngle", value  ); }

        //^target spacing for bolting on joggle spliced deck panels
        public double JoggleBoltSpacing { get => PropValD("JoggleBoltSpacing"); set => PropValSet("JoggleBoltSpacing", value);  }

        //^the maximum distance between ring clips on strip decks and support plates
        public double MaxRingClipSpacing { get => PropValD("MaxRingClipSpacing"); set => PropValSet("MaxRingClipSpacing", Math.Abs(value)); }

        //^the maximum distance between ring clips on half moon decks
        public double MoonRingClipSpacing { get => PropValD("MoonRingClipSpacing"); set => PropValSet("MoonRingClipSpacing", Math.Abs(value)); }

        private void Notify(uopProperty aProperty) { if (aProperty == null || aProperty.Protected) return;  PropertyChangeEvent?.Invoke(aProperty); }
    

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
        public override void  ReadProperties(uopProject aProject, uopPropertyArray aFileProps, ref uopDocuments ioWarnings, double aFileVersion, string aFileSection = null, bool bIgnoreNotFound = false, uopTrayAssembly aAssy = null, uopPart aPartParameter = null, List<string> aSkipList = null, Dictionary<string, string> EqualNames = null, uopProperties aProperties = null)
        {

            try
            {
                ioWarnings ??= new uopDocuments();
                aFileSection = string.IsNullOrWhiteSpace(aFileSection) ? INIPath : aFileSection.Trim();
                if (aFileProps == null) throw new Exception("The Passed File Property Array is Undefined");
                if (string.IsNullOrWhiteSpace(aFileSection)) throw new Exception("File Section is Undefined");
                if (aProject != null) SubPart(aProject);


                uopProperties myprops = aFileProps.Item(aFileSection);
                if (myprops == null || myprops.Count <= 0)
                {
                    ioWarnings?.AddWarning(this, $"{PartName} Data Not Found", $"File '{ aFileProps.Name }' Does Not Contain {aFileSection} Info!");
                    return;
                }

                if (!myprops.Contains("FlowDeviceType")) 
                    myprops.Add(new uopProperty("FlowDeviceType", uppFlowDevices.None));

                if (myprops.Contains("HasFlowEnhancement"))
                {
                    if (myprops.ValueB("HasFlowEnhancement")) myprops.SetValue("FlowDeviceType", uppFlowDevices.FED);

                }
                if (myprops.Contains("HasAntiPenetrationPans"))
                {
                     if(myprops.ValueB("HasAntiPenetrationPans")) myprops.SetValue("FlowDeviceType", uppFlowDevices.APPans);

                }
                

                if (myprops.ValueI("FlowDeviceType") == (int)uppFlowDevices.None) myprops.SetValueD("FEDorAPPHeight", 0d);

                base.ReadProperties(aProject, aFileProps, ref ioWarnings, aFileVersion, aFileSection, bIgnoreNotFound, aSkipList: aSkipList, EqualNames: EqualNames);
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

        public override  uopPropertyArray SaveProperties(string aHeading = null)
        {
            
            //^returns the objects properties in a collection
            //~signatures like "COLOR=RED"
            aHeading = string.IsNullOrWhiteSpace(aHeading) ? INIPath : aHeading.Trim();
            UpdatePartProperties();
            uopProperties sprops = base.SaveProperties(aHeading).Item(1);
            sprops.Add("HasAntiPenetrationPans", HasAntiPenetrationPans,aHeading: aHeading);
            sprops.Add("HasFlowEnhancement", HasFlowEnhancement, aHeading : aHeading);
            return new uopPropertyArray( sprops, aName:aHeading, aHeading: aHeading );
           
        }

        //^flag indicating if the deck sections are connected with slots and tabs
        public bool SlotAndTab => SpliceStyle == uppSpliceStyles.Tabs;
        
        public uppSpliceStyles SpliceStyle { get => (uppSpliceStyles)PropValI("SpliceStyle"); set { if ((int) value >=0) PropValSet("SpliceStyle", value); } }

        //^target spacing for stiffeners and finger clips
        public double StiffenerSpacing { get => PropValD("StiffenerSpacing"); set => PropValSet("StiffenerSpacing", value); }

    

        public override void UpdatePartProperties() 
        { 
            base.Suppressed = true;
            TPROPERTIES props = ActiveProps;
            double dval = BottomDCHeight;
            mdTrayAssembly assy;
            if (dval != 0)
            {
                if(dval < 0)
                {
                    dval = 0;
                }
                else
                {
                    
                    assy = GetMDTrayAssembly();
                    if (assy != null)
                    {
                        double dht = assy.Downcomer().Height;
                        if (Math.Round(dval,3) <= Math.Round(dht, 3))
                        {
                            dval = 0;
                        }
                        else
                        {
                            if (dval >= 2 * dht) dval = dht;
                        }
                    }
                }

                if(dval != BottomDCHeight)
                {
                    props.SetValue("BottomDCHeight", dval);
                    ActiveProps = props;
                }
                
            }
           
          
        }

        public override void UpdatePartWeight() { base.Weight = 0; }

        //^flag to use sliding manway clips rather that manway clamps
        public bool UseManwayClips { get => PropValB("UseManwayClips"); set => PropValSet("UseManwayClips", value); }

        //^flag to indicate if the stiffeners should be welded to the downcomer box
        public bool WeldedStiffeners { get => PropValB("WeldedStiffeners"); set => PropValSet("WeldedStiffeners", value); }

        public double BottomDCHeight { get => PropValD("BottomDCHeight"); set => PropValSet("BottomDCHeight", value); }

        #endregion Properties

        #region Methods

        public override bool IsEqual(uopPart aPart)=> (aPart != null) && (aPart.PartType == uppPartTypes.DesignOptions && aPart.CurrentProperties().Equals(CurrentProperties()));
       


        public override uopDocuments Warnings() => GenerateWarnings(null);

        public uopDocuments GenerateWarnings(mdTrayAssembly aAssy, string aCategory = "", uopDocuments aCollector = null, bool bJustOne = false)
        {
            uopDocuments _rVal = aCollector ?? new uopDocuments();
            if (bJustOne && _rVal.Count > 0) return _rVal;

            //^returns a collection of strings that are warnings about possible problems with
            //^the current tray assembly design.
            //~these warnings may or may not be fatal problems.

            bool bFED = HasAntiPenetrationPans || HasFlowEnhancement;
            
            string txt = string.Empty;

            aAssy ??= GetMDTrayAssembly(aAssy);
            if (aAssy == null)  return _rVal;
            double tSpace = aAssy.RingSpacing;
            mdDowncomer aDC = aAssy.Downcomer();

            if (aCategory ==  string.Empty) { aCategory = TrayName(true); }

            if (!aAssy.DesignFamily.IsBeamDesignFamily())
            {
                if (aAssy.DesignFamily.IsEcmdDesignFamily())
                {
                    if (bFED)
                    {
                        if (HasBubblePromoters)
                        {
                            txt = "ECMD Trays With Flow Devices (APP or FED) Do Not Require Bubble Promoters";

                        }
                    }
                    else
                    {
                        if (!HasBubblePromoters)
                        {
                            txt = "ECMD Trays Without Flow Devices Require Bubble Promoters";
                        }
                    }
                }
                else
                {
                    if (!HasBubblePromoters)
                    {
                        txt = "MD Trays Require Bubble Promoters";
                    }
                }
            }

            if (txt !=  string.Empty)
            {
                    _rVal.AddWarning(this, "Bubble Promoter Warning", txt, uppWarningTypes.ReportFatal);
                if (bJustOne && _rVal.Count > 0) return _rVal;
            }
            txt = string.Empty;
            if (bFED)
            {
                //validate flow device cleance dimension
                double lim1 = FEDorAPPHeight;
                double lim2 = aDC.HeightAboveDeck - 0.25;

                if (lim1 == 0)
                { txt = "Flow Device (APP or FED) Clearance Is Zero"; }
                else if (lim1 > lim2)
                {  txt = "Flow Device (APP or FED) has Been Requested But The Clearance Height Above The Deck Is To Large To Accomdate The Device\""; }

                if (txt !=  string.Empty)
                {
                    _rVal.AddWarning(this, "Flow Device Warning", txt, uppWarningTypes.ReportFatal);
                    if (bJustOne && _rVal.Count > 0) return _rVal;
                }


            }
            txt = string.Empty;

            if (aAssy.DesignFamily.IsEcmdDesignFamily())
            {
                if (CDP == 0)
                {  txt = "CDP Height Has Not Been Provided"; }
                else
                {
                    if (CDP < 0.5 * tSpace)
                    { txt = "CDP Height Is Less Than 50% Of Range Tray Spacing"; }
                    else if (CDP > 0.85 * tSpace)
                    { txt = "CDP Height Is Greater Than 85% Of Range Tray Spacing"; }
                }

                if (txt ==  string.Empty)
                {
                    if(CDP - aAssy.WeirHeight > aAssy.ManholeID - 0.25)
                    {
                        if(aAssy.ProjectType == uppProjectTypes.MDSpout)
                        {
                            txt = "CDP - Weir Height Is Greater Than the Range Manhole ID - 0.25''";
                        }
                        else
                        {
                            txt = "Baffle Height Is Greater Than the Range Manhole ID - 0.25''";
                        }
                      
                    }
                } 
                
                if (txt !=  string.Empty)
                {
                    _rVal.AddWarning(this, "Baffle Height Warning", txt, uppWarningTypes.ReportFatal);
                    if (bJustOne && _rVal.Count > 0) return _rVal;
                }
                
            }
            //         If aAssy.DeckSectionWidth(True) <= aAssy.ManholeID - 0.5 And HasTiledDecks Then
            //            .AddWarning Me, "Tiled Decks Warning", "Tiled Decks Have Been Requested But Are Not Required"
            //        End If
       
            return _rVal;
        }

        // <summary>
        // sets the parts property with the passed name to the passed value
        //returns the property if the property value actually changes.
        /// </summary>
        public override uopProperty PropValSet(string aName, object aPropVal, int aOccur = 0, bool? bSuppressEvnts = null, bool? bHiddenVal = null)
        {

            bool supevnts = bSuppressEvnts.HasValue ? bSuppressEvnts.Value : SuppressEvents || Reading;
            uopProperty _rVal = base.PropValSet(aName, aPropVal, aOccur, supevnts, bHiddenVal);
            if (_rVal == null || supevnts) return _rVal;
            Notify(_rVal);
            return _rVal;
        }

        #endregion
    }
}
