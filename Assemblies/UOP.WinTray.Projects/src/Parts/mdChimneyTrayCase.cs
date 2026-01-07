using System;
using System.Collections.Generic;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Interfaces;
using UOP.WinTray.Projects.Structures;

namespace UOP.WinTray.Projects.Parts
{
    /// <summary>
    /// represents a a Chimney Tray in an MD project
    /// </summary>
    public class mdChimneyTrayCase : uopPart, iCase
    {
        public override uppPartTypes BasePartType => uppPartTypes.ChimneyTrayCase;


        #region Events
        
        public delegate void ChimneyTrayCasePropertyChange(uopProperty aProperty);
        public event ChimneyTrayCasePropertyChange eventChimneyTrayCasePropertyChange;
        
        #endregion Events

        #region Constructors

        public mdChimneyTrayCase() : base(uppPartTypes.ChimneyTrayCase, uppProjectFamilies.uopFamMD, "","") { InitializeProperties(); }

        internal mdChimneyTrayCase(mdChimneyTrayCase aPartToCopy) : base(uppPartTypes.ChimneyTrayCase, uppProjectFamilies.uopFamMD, "", "") { base.Copy( aPartToCopy); }

        /// <summary>
        /// property initializer
        /// </summary>
        private void InitializeProperties()
        {
            double initval = 0d;
            AddProperty("Description", "");
            AddProperty("ChimneyTrayType", "Partial");
            AddProperty("LiquidDrawRate", initval, uppUnitTypes.BigMassRate, aNullVal: null);
            AddProperty("LiquidFromAbove", initval, uppUnitTypes.BigMassRate, aNullVal: null);
            AddProperty("LiquidDensity", initval, uppUnitTypes.Density, aNullVal: null);
            AddProperty("VaporFromBelow", initval, uppUnitTypes.BigMassRate, aNullVal: null);
            AddProperty("VaporDensity", initval, uppUnitTypes.Density, aNullVal: null);
            AddProperty("ResidenceTime", initval, uppUnitTypes.Seconds, aNullVal: null);
            AddProperty("MinimumOperatingRange", initval, uppUnitTypes.BigPercentage, aNullVal: null);
            AddProperty("MaximumOperatingRange", initval, uppUnitTypes.BigPercentage, aNullVal: null);
            AddProperty("DrawAmount", initval, uppUnitTypes.Percentage, true, aNullVal: null);
            AddProperty("LiquidToBelow", initval, uppUnitTypes.BigMassRate, true, aNullVal: null);

            PropsLockTypes(true);

        }

        #endregion Constructors

        #region Properties
        
        /// <summary>
        /// Case name
        /// </summary>
        public string CaseName => $"Chimney Tray { ChimneyTrayIndex } - Case { Index }";
        

        /// <summary>
        /// ChimneyTrayType
        /// </summary>
        public string ChimneyTrayType { get => PropValS("ChimneyTrayType"); set => PropValSet("ChimneyTrayType", value, bSuppressEvnts: true); }


    /// <summary>
    /// Description
    /// </summary>
    public override string Description
        {
            get
            {
                string _rVal = PropValS("Description");
                if (string.IsNullOrWhiteSpace(_rVal))
                {
                    _rVal = Name;
                    PropValSet("Description", _rVal, bSuppressEvnts: true);
                }

                return _rVal;

            }
            set
            { Notify(PropValSet("Description", value)); }
        }

        /// <summary>
        /// Draw amount
        /// </summary>
        public double DrawAmount { get => PropValD("DrawAmount"); set => Notify(PropValSet("DrawAmount", value)); }

        /// <summary>
        /// Friendly Name
        /// </summary>
        public string FriendlyName => $"ChimneyTrayType  {TrayAbove}-{ TrayBelow}";

        /// <summary>
        /// INI Path
        /// </summary>
        public override string INIPath => $"CHIMNEYTRAYS({ ChimneyTrayIndex }).CASE({ Index })";

     
        /// <summary>
        /// the feed liquid density
        /// </summary>
        public double LiquidDensity { get => PropValD("LiquidDensity"); set => Notify(PropValSet("LiquidDensity", value)); }
      

        /// <summary>
        /// the total rate of feed product introduction
        /// </summary>
        public double LiquidDrawRate { get => PropValD("LiquidDrawRate"); set => Notify(PropValSet("LiquidDrawRate", value)); } 
   

    /// <summary>
    /// the liquid rate from above
    /// </summary>
    public double LiquidFromAbove { get => PropValD("LiquidFromAbove"); set => Notify(PropValSet("LiquidFromAbove", value)); }

        /// <summary>
        /// Liquid To Below
        /// </summary>
        public double LiquidToBelow { get => PropValD("LiquidToBelow"); set => Notify(PropValSet("LiquidToBelow", value)); }

       

        /// <summary>
        /// the max operating range for the tray case
        /// </summary>
        public double MaximumOperatingRange { get => PropValD("MaximumOperatingRange"); set => Notify(PropValSet("MaximumOperatingRange", value)); }

        /// <summary>
        /// the minimum operating range for the tray case
        /// </summary>
        public double MinimumOperatingRange { get => PropValD("MinimumOperatingRange"); set => Notify(PropValSet("MinimumOperatingRange", value)); }

        /// <summary>
        /// Name
        /// </summary>
        public override string Name => $"Case {Index}";

        /// <summary>
        ///returns the min and max operating range values in a formated string
        /// like 10 - 200
        /// </summary>
        public string OperatingRange => PropValS("MinimumOperatingRange", formatted: true) + " - " + PropValS("MaximumOperatingRange", formatted: true);

        /// <summary>
        /// Handle
        /// </summary>
        public string Handle => $"Tray({ ChimneyTrayIndex }).Case({ Index })";

        /// <summary>
        /// Residence Time
        /// </summary>
        public int ResidenceTime { get => PropValI("ResidenceTime"); set => Notify(PropValSet("ResidenceTime", value)); }

       
      
        /// <summary>
        /// the tray number of the tray above this tray case
        /// </summary>
        public string TrayAbove
        {
            get
            {
                mdChimneyTray aTray = (mdChimneyTray)uopEventHandler.RetrieveChimneyTray(ProjectHandle, ChimneyTrayIndex);
                return (aTray != null) ? aTray.TrayAbove : "";
            }
        }

        /// <summary>
        /// the tray number of the tray above this tray case
        /// </summary>
        public string TrayBelow
        {
            get
            {
                mdChimneyTray aTray = (mdChimneyTray)uopEventHandler.RetrieveChimneyTray(ProjectHandle, ChimneyTrayIndex);
                return (aTray != null) ? aTray.TrayBelow : "";
            }
        }

        /// <summary>
        /// the density of the feed vapor from below
        /// </summary>
        public double VaporDensity { get =>  PropValD("VaporDensity"); set => Notify(PropValSet("VaporDensity", value)); }

        /// <summary>
        /// the vapor feed rate from below
        /// </summary>
        public double VaporFromBelow { get => PropValD("VaporFromBelow"); set => Notify(PropValSet("VaporFromBelow", value)); }

        #endregion Properties

        #region Methods
        /// <summary>
        ///returns the objects properties in a collection
        /// signatures like "COLOR=RED"
        /// </summary>
        public override uopProperties CurrentProperties() { UpdatePartProperties(); return base.CurrentProperties(); }
        //set
        //{
        //    if (value == null) return;
        //     ActiveProps =  value.Structure;
        //    RecalcDependantValues();
        //    UpdatePartProperties();

        //}
        // }
        public override bool SetCurrentProperties(uopProperties value)
        {
            if (value == null) return false;
            if (SetProps(value))
            {
                RecalcDependantValues();
                UpdatePartProperties();
                return true;
            }
            return false;
        }
        public override string ToString() { return CaseName.ToUpper(); }

        /// <summary>
        /// Notify the change
        /// </summary>
        /// <param name="uopProperty"></param>
        private void Notify(uopProperty aProperty)
        {
            if (aProperty == null || aProperty.Protected) return;

            eventChimneyTrayCasePropertyChange?.Invoke(aProperty);
            RecalcDependantValues();
            UpdatePartProperties();
            mdChimneyTray myTray = (mdChimneyTray)uopEventHandler.RetrieveChimneyTray(ProjectHandle, ChimneyTrayIndex);

            if (!SuppressEvents && myTray != null) myTray.Notify(aProperty);

        }

        /// <summary>
        ///returns the properties required to save the object to file
        /// signatures like "COLOR=RED"
        /// </summary>
        public override uopPropertyArray SaveProperties(string aHeading = null)
        {
            aHeading = string.IsNullOrWhiteSpace(aHeading) ? INIPath : aHeading;
            UpdatePartProperties();
            return base.SaveProperties(aHeading);
        }

        /// <summary>
        ///returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>
        public mdChimneyTrayCase Clone() => new mdChimneyTrayCase(this);
        

        public override uopPart Clone(bool aFlag = false) => (uopPart)this.Clone();

        /// <summary>
        /// IsEqual method
        /// </summary>
        /// <param name="aCase"></param>
        /// <returns></returns>
        public bool IsEqual(mdChimneyTrayCase aCase) => aCase != null && TPROPERTIES.Compare(aCase.ActiveProps, ActiveProps);


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
                Reading = true;


                uopPropertyArray myprops = aFileProps.PropertiesStartingWith(aFileSection);


                if (myprops == null || myprops.Count <= 0)
                {
                    ioWarnings?.AddWarning(this, $"{PartName} Data Not Found", $"File '{ aFileProps.Name }' Does Not Contain {aFileSection} Info!");
                    return;
                }
                base.ReadProperties(aProject, myprops, ref ioWarnings, aFileVersion, aFileSection, bIgnoreNotFound, aSkipList: aSkipList, EqualNames: EqualNames);
                Reading = true;
                uopProperties actprops = ActiveProperties;
                foreach (uopProperty item in actprops)
                {
                    if (item.HasUnits && item.ValueD < 0)
                        item.SetValue(0d);
                }

                ActiveProps = new TPROPERTIES(actprops);
           

            }
            catch (Exception e)
            {
                ioWarnings?.AddWarning(this, "Read Properties Error", e.Message);
            }
            finally
            {

                aProject?.ReadStatus("", 2);
                Reading = false;
                RecalcDependantValues();
            }
        }
        /// <summary>
        /// Recalc Dependant Values
        /// </summary>
        private void RecalcDependantValues(bool bUpdatePartProps = true)
        {
            double val1 = 0;
            double val2 = 0;
            if (LiquidFromAbove != 0)
            {
                val1 = LiquidDrawRate / LiquidFromAbove * 100;
                val2 = LiquidFromAbove - LiquidDrawRate; 
                
            }
          
            PropValSet("DrawAmount", val1, bSuppressEvnts: true, bHiddenVal: val1 == 0);
            PropValSet("LiquidToBelow", val2, bSuppressEvnts: true, bHiddenVal: val2 == 0);

            if (LiquidToBelow <= 0)
            { PropValSet("ChimneyTrayType", "Total", bSuppressEvnts: true); }
            else if (LiquidDrawRate <= 0)
            { PropValSet("ChimneyTrayType", "Distributor", bSuppressEvnts: true);  }
            else
                { PropValSet("ChimneyTrayType", "Partial", bSuppressEvnts: true);  }
            if (bUpdatePartProps) UpdatePartProperties();
        }

        /// <summary>
        /// Update Part Properties
        /// </summary>
        public override void UpdatePartProperties() { RecalcDependantValues(false); DescriptiveName = Name; }

        /// <summary>
        /// Update part weight
        /// </summary>
        public override void UpdatePartWeight() => Weight = 0;

        //the project chimney tray at the parts chimney tray index
        public mdChimneyTray ChimneyTray() => (mdChimneyTray)uopEventHandler.RetrieveChimneyTray(ProjectHandle, ChimneyTrayIndex);

        /// <summary>
        /// executed internally to create the page(s) for a MD Spout project report
        /// </summary>
        /// <param name="rTray"></param>
        /// <returns></returns>
        public uopProperties ReportProperties(dynamic rTray, uppReportTypes aReportType)
        {
            rTray = ChimneyTray();
            uopProperties _rVal = new uopProperties();
            if (rTray == null) return null;
            
            dynamic aVal = Description;
            //1
            _rVal.Add("Description", aVal);
            //2
            _rVal.Add("Tray", rTray.Description);
            //3
            _rVal.Add("Type", PropValS("ChimneyTrayType"));
            //4
            _rVal.Add("Nozzle Label", rTray.NozzleLabel);
            //5
            _rVal.Add("Tray Above", rTray.TrayAbove);
            //6
            _rVal.Add("Tray Below", rTray.TrayBelow);
            //7
            _rVal.Add("Design Liquid Draw Rate", PropValD("LiquidDrawRate"), uppUnitTypes.BigMassRate);
            //8
            _rVal.Add("Draw Amount", PropValD("DrawAmount"), uppUnitTypes.Percentage);
            //9
            _rVal.Add("Liquid From Above", PropValD("LiquidFromAbove"), uppUnitTypes.BigMassRate);
            //10
            _rVal.Add("Liquid To Trays Below", PropValD("LiquidToBelow"), uppUnitTypes.BigMassRate);
            //11
            _rVal.Add("Liquid Density", PropValD("LiquidDensity"), uppUnitTypes.Density);
            //12
            _rVal.Add("Vapor From Below", PropValD("VaporFromBelow"), uppUnitTypes.BigMassRate);
            //13
            _rVal.Add("Vapor Density", PropValD("VaporDensity"), uppUnitTypes.Density);
            TPROPERTY aProp = Prop("MinimumOperatingRange");
            TPROPERTY bProp = Prop("MaximumOperatingRange");
            //14
            if (aProp.Value != 0)
                
            {
                _rVal.Add("Operating Range", aProp.UnitValueString(uppUnitFamilies.English, aOverridePrecision: 0) + " - " + bProp.UnitValueString(uppUnitFamilies.English, aOverridePrecision: 0), bProtected: true, aUnitCaption: "%");
            }
            else
            {
                _rVal.Add("Operating Range", "-");
            }
            //15
            _rVal.Add("Required Residence Time", PropValI("ResidenceTime"), aUnitType: uppUnitTypes.Seconds, bProtected: true);
            
            _rVal.SetProtected(true);
            TPROPERTIES dProps = new TPROPERTIES( _rVal);
            for (int i = 1; i <= dProps.Count; i++)
            {
                aProp = dProps.Item(i);
                aVal = aProp.Value;
                if (aProp.IsNullValue) aProp.Value = string.Empty;
                
                aProp.Row = i;
                dProps.SetItem(i, aProp);
            }
            return new uopProperties(dProps);
        }

        /// <summary>
        /// iuoppart hole array
        /// </summary>
        /// <param name="aAssy_UNUSED"></param>
        /// <param name="aTag_UNUSED"></param>
        /// <returns></returns>

        /// <summary>
        /// iuoppart isequal 
        /// </summary>
        /// <param name="aPart"></param>
        /// <returns></returns>
        public override bool IsEqual(uopPart aPart)
        {
            bool iIsEqual = false;
            if (aPart == null)
            {
                return iIsEqual;

            }
            if (aPart.PartType != PartType)
            {
                return iIsEqual;

            }
            iIsEqual = IsEqual((mdChimneyTrayCase)aPart);
            return iIsEqual;
        }

    
      

        #endregion Methods

        #region iCase Support

        /// <summary>
        /// Icase Current Properties
        /// </summary>
        uopProperties iCase.CurrentProperties()  => CurrentProperties(); 

        /// <summary>
        /// Icase description
        /// </summary>
        string iCase.Description { get => Description; set => Description = value; }

        /// <summary>
        /// icase index
        /// </summary>
        int iCase.Index { get => Index; set => Index = value; }

        /// <summary>
        /// iCase MaximumOperatingRange
        /// </summary>
        double iCase.MaximumOperatingRange { get => MaximumOperatingRange; set => MaximumOperatingRange = value; }

        /// <summary>
        /// iCase MinimumOperatingRange
        /// </summary>
        double iCase.MinimumOperatingRange { get => MinimumOperatingRange; set => MinimumOperatingRange = value; }

        /// <summary>
        /// icase name
        /// </summary>
        string iCase.Name => Name;

        uppPartTypes iCase.PartType => uppPartTypes.ChimneyTrayCase;

        /// <summary>
        /// icase object type
        /// </summary>
        string iCase.ObjectType => "Chimney Tray Case";

        uppCaseOwnerOwnerTypes iCase.OwnerType => uppCaseOwnerOwnerTypes.ChimneyTray;



        /// <summary>
        /// icase owner index
        /// </summary>
        int iCase.OwnerIndex => ChimneyTrayIndex;

        

        /// <summary>
        /// icase part path
        /// </summary>
        string iCase.PartPath => PartPath();

        /// <summary>
        /// Icase clone
        /// </summary>
        /// <returns></returns>
        iCase iCase.Clone()
        {
            iCase iCase_Clone = null;
            iCase_Clone = Clone();
            return iCase_Clone;
        }
        /// <summary>
        /// icase get property value
        /// </summary>
        /// <param name="aPropertyNameorIndex"></param>
        /// <param name="bExists"></param>
        /// <returns></returns>
        dynamic iCase.GetPropertyValue(dynamic aPropertyNameorIndex, out bool bExists)
        { return PropValGet(aPropertyNameorIndex, out bExists); }

        /// <summary>
        /// icase prop val set
        /// </summary>
        /// <param name="PropertyNameorIndex"></param>
        /// <param name="NewValue"></param>
        /// <returns></returns>
        bool iCase.PropValSet(dynamic PropertyNameorIndex, dynamic NewValue)
        { return PropValSet(PropertyNameorIndex, NewValue); }

        /// <summary>
        /// icase RecalculateDependantValues
        /// </summary>
        void iCase.RecalculateDependantValues()
        { RecalcDependantValues(); }

        #endregion iCase Support


    }
}
