using System;
using System.Collections.Generic;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Events;
using UOP.WinTray.Projects.Interfaces;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;

namespace UOP.WinTray.Projects.Parts
{
    /// <summary>
    ///^represents as Distributor in an MD project
    /// </summary>
    public class mdDistributorCase : uopPart, iCase
    {


        public override uppPartTypes BasePartType => uppPartTypes.DistributorCase;

      
        //property indices for faster erence
     
        //public delegate void DistributorCasePropertyChange(uopProperty aProperty);
        //public event DistributorCasePropertyChange eventDistributorCasePropertyChange;

        #region Constructors

        public mdDistributorCase() :base(uppPartTypes.DistributorCase, uppProjectFamilies.uopFamMD, "","",false) { InitializeProperties(); }


        internal mdDistributorCase(mdDistributorCase aPartToCopy ) : base(uppPartTypes.DistributorCase, uppProjectFamilies.uopFamMD, "", "", false)
        {
            if (aPartToCopy == null) { InitializeProperties(); } else { base.Copy(aPartToCopy); }
        }

        private void InitializeProperties()
        {
            double initval = 0d;
            
           AddProperty("Description", "");
           AddProperty("LiquidRate", initval, uppUnitTypes.BigMassRate, bProtected: false, aNullVal: null);
           AddProperty("LiquidDensity", initval, uppUnitTypes.Density, bProtected: false, aNullVal: null);
           AddProperty("VaporRate", initval, uppUnitTypes.BigMassRate, bProtected: false, aNullVal: null);
           AddProperty("VaporDensity", initval, uppUnitTypes.Density, bProtected: false, aNullVal: null);
           AddProperty("LiquidFromAbove", initval, uppUnitTypes.BigMassRate, bProtected: false, aNullVal: null);
           AddProperty("LiquidDensityFromAbove", initval, uppUnitTypes.Density, bProtected: false, aNullVal: null);
           AddProperty("VaporFromBelow", initval, uppUnitTypes.BigMassRate, bProtected: false, aNullVal: null);
           AddProperty("VaporDensityFromBelow", initval, uppUnitTypes.Density, bProtected: false, aNullVal: null);
           AddProperty("MinimumOperatingRange", initval, uppUnitTypes.BigPercentage, bProtected: false, aNullVal: null);
           AddProperty("MaximumOperatingRange", initval, uppUnitTypes.BigPercentage, bProtected: false, aNullVal: null);

            //dependent properties
           AddProperty("DesignRate", 0!, uppUnitTypes.BigMassRate, bProtected: true);
           AddProperty("VaporPercentage", 0!, uppUnitTypes.VolumePercentage, bProtected: true);
           AddProperty("FeedLiquidPercentage", 0!, uppUnitTypes.VolumePercentage, bProtected: true);
           AddProperty("FeedVaporPercentage", 0!, uppUnitTypes.VolumePercentage, bProtected: true);
           AddProperty("FeedLiquidVolumeRate", 0!, uppUnitTypes.VolumeRate, bProtected: true);
           AddProperty("FeedVaporVolumeRate", 0!, uppUnitTypes.VolumeRate, bProtected: true);
           AddProperty("DistributorType", "Reflux", bIsHidden: true, bProtected: true);
           AddProperty("TotalLiquidVolumeRate", 0!, uppUnitTypes.VolumeRate, bIsHidden: true,aDisplayName: "Total Liquid Vol. Rate",bProtected: true);
           AddProperty("TotalVaporVolumeRate", 0!, uppUnitTypes.VolumeRate, bIsHidden: true, aDisplayName:"Total Vapor Vol. Rate", bProtected: true);
           AddProperty("AdditionalTroughLiquidRate", initval, uppUnitTypes.BigMassRate, bProtected: false, aNullVal: null);
            AddProperty("FeedVolumnRate", 0!, uppUnitTypes.VolumeRate, bIsHidden: true, aDisplayName: "Feed Vol. Rate", bProtected: true);
            AddProperty("FeedDensity", 0!, uppUnitTypes.Density, bIsHidden: true, aDisplayName: "Feed Density (avg.)", bProtected: true);

            PropsLockTypes(true);
        }

        #endregion Constructors

        #region Properties

        public string CaseName => "Distributor " + DistributorIndex + " - Case " + Index;
    
        public string Handle => "Tray(" + DistributorIndex + ").Case(" + Index + ")";

       
        public override string Description { get
            {
                string _rVal = PropValS("Description");
                if (string.IsNullOrEmpty(_rVal))
                {
                    PropValSet("Description", Name, bSuppressEvnts: true);
                    return Name;
                }
                return _rVal;
            } set => Notify(PropValSet("Description", value));  }
        
        /// <summary>
        /// the total rate of feed product introduction
        /// </summary>
        public double DesignRate { get=> ActiveProps.ValueD("DesignRate"); set =>Notify(PropValSet("DesignRate", value)); }

        public double FeedVolumnRate => CurrentProperties().ValueD("FeedVolumnRate");
        public string DistributorType => PropValS("DistributorType");

        public uopProperty FeedDensity => new uopProperty("FeedDensity", FeedAverageDensity, uppUnitTypes.Density);
             

        public double FeedAverageDensity
        {
            get
            {
                double _rVal = 0;

                if (LiquidFeedVolume > 0 && VaporFeedVolume > 0)
                {
                    _rVal = uopUtils.AverageDensity(LiquidDensity, VaporDensity, LiquidRate, VaporRate);
                }
                else
                {
                    if (LiquidFeedVolume > 0) _rVal = LiquidDensity;

                    if (VaporFeedVolume > 0) _rVal= VaporDensity;

                }

                return _rVal;
            }
        }
        public double FeedLiquidPercentage { get => PropValD("FeedLiquidPercentage"); set => Notify(PropValSet("FeedLiquidPercentage", value));  }

        /// <summary>
        /// the type of feed product that this distributor case handles
        ///~LIQUID, VAPOR or MIXED
        /// </summary>
        public string FeedType
        {
            get
            {
                if (FeedVaporPercentage == 0) return "LIQUID";
                if (FeedLiquidPercentage == 0) return "VAPOR";
                return "MIXED";
            }
        }
        
        public double FeedVaporPercentage {get => PropValD("FeedVaporPercentage"); set => Notify(PropValSet("FeedVaporPercentage", value)); }
        
        /// <summary>
        /// the volumetric rate of the feed fluid
        /// </summary>
        public uopProperty FeedVolumeRate { get { uopProperty _rVal = new uopProperty("FeedVolumeRate", TotalFeedVolume, uppUnitTypes.VolumeRate, aDefaultValue: 0); _rVal.SubPart(this); return _rVal; } }
      
        public string FriendlyName => $"{DistributorType } { TrayAbove }-{ TrayBelow}";

        public override string INIPath => $"DISTRIBUTORS({ DistributorIndex }).CASE({ Index })";
       
        /// <summary>
        /// the feed liquid density
        /// </summary>
        public double LiquidDensity { get => PropValD("LiquidDensity"); set =>Notify(PropValSet("LiquidDensity", value)); }

        /// <summary>
        /// the liquid density of the fluid from above
        /// </summary>
        public double LiquidDensityFromAbove { get => PropValD("LiquidDensityFromAbove"); set => Notify(PropValSet("LiquidDensityFromAbove", value)); }

        public double LiquidFeedVolume => (LiquidDensity > 0 && LiquidRate > 0) ? LiquidRate / LiquidDensity : 0;
       
        /// <summary>
        /// the liquid rate from above
        /// </summary>
        public double LiquidFromAbove { get  => PropValD("LiquidFromAbove"); set => Notify(PropValSet("LiquidFromAbove", value)); }

        public double LiquidFromAboveVolume => (LiquidDensityFromAbove > 0 & LiquidFromAbove > 0) ? (LiquidFromAbove / LiquidDensityFromAbove) : 0;
        
        /// <summary>
        /// the total liquid feed rate
        /// </summary>
        public double LiquidRate { get => PropValD("LiquidRate"); set => Notify(PropValSet("LiquidRate", value)); }

        /// <summary>
        /// the additional liquid feed rate to the troughs
        /// </summary>
        public double AdditionalTroughLiquidRate { get => PropValD("AdditionalTroughLiquidRate"); set => Notify(PropValSet("AdditionalTroughLiquidRate", value)); }
      
        /// <summary>
        /// the max operating range for the distributor case
        /// </summary>
        public double MaximumOperatingRange { get => PropValD("MaximumOperatingRange"); set => Notify(PropValSet("MaximumOperatingRange", value)); }
       
        /// <summary>
        /// the minimum operating range for the distributor case
        /// </summary>
        public double MinimumOperatingRange { get => PropValD("MinimumOperatingRange"); set => Notify(PropValSet("MinimumOperatingRange", value)); }
        
        public override string Name => $"Case {Index}";

        /// <summary>
        /// the nozzle of the distributor that owns this case
        /// </summary>
        public uopPipe NozzleObject { get { mdDistributor aDistrib = Distributor(); return aDistrib?.NozzleObject; } }

        /// <summary>
        /// ///returns the min and max operating range values in a formated string
        ///~like 10.50-200.12
        /// </summary>
        public string OperatingRange => PropStringVal("MinimumOperatingRange", true) + " - " + PropStringVal("MaximumOperatingRange", true);
      
 
        /// <summary>
        /// the pipe of the distributor that owns this case
        /// </summary>
        public uopPipe PipeObject { get { mdDistributor aDistrib = Distributor(); return aDistrib?.PipeObject; } }
       
        
          
        public double TotalFeedVolume => LiquidFeedVolume + VaporFeedVolume;
        public double TotalLiquidVolume => LiquidFeedVolume + LiquidFromAboveVolume;
        public double TotalVaporVolume => VaporFeedVolume + VaporFromBelowVolume;
        /// <summary>
        /// the tray number of the tray above this distributor case
        /// </summary>
        public string TrayAbove { get { mdDistributor aDistrib = Distributor(); return (aDistrib != null) ? aDistrib.TrayAbove : ""; } }

        /// <summary>
        /// the tray number of the tray below this distributor case
        /// </summary>
        public string TrayBelow { get { mdDistributor aDistrib = Distributor(); return (aDistrib != null) ? aDistrib.TrayBelow : ""; } }
       
        /// <summary>
        /// the density of the feed vapor from below
        /// </summary>
        public double VaporDensity { get => PropValD("VaporDensity"); set => Notify(PropValSet("VaporDensity", value)); }
     
        /// <summary>
        /// the density of the vapor from below
        /// </summary>
        public double VaporDensityFromBelow { get => PropValD("VaporDensityFromBelow"); set => Notify(PropValSet("VaporDensityFromBelow", value)); }
       

        public double VaporFeedVolume => (VaporDensity > 0 && VaporRate > 0) ? (VaporRate / VaporDensity) : 0;
        
        /// <summary>
        /// the vapor feed rate from below
        /// </summary>
        public double VaporFromBelow { get => PropValD("VaporFromBelow"); set => Notify(PropValSet("VaporFromBelow", value)); }
        
        public double VaporFromBelowVolume => (VaporDensityFromBelow > 0) ? (VaporFromBelow / VaporDensityFromBelow) : 0;
      
        /// <summary>
        /// the volume percentage of vapor in the feed product
        /// </summary>
        public double VaporPercentage { get => PropValD("VaporPercentage"); set => Notify(PropValSet("VaporPercentage", value)); }
       
        /// <summary>
        /// the total vapor feed rate
        /// </summary>
        public double VaporRate { get => PropValD("VaporRate"); set => Notify(PropValSet("VaporRate", value)); }

        #endregion Properties

        #region Methods

        public override string ToString() { return CaseName.ToUpper(); }
        /// <summary>
        ///^returns the properties required to save the object to file
        ///~signatures like "COLOR=RED"
        /// </summary>
        public override uopPropertyArray SaveProperties(string aHeading = null)
        {
            aHeading = string.IsNullOrWhiteSpace(aHeading) ? INIPath : aHeading.Trim();
            UpdatePartProperties();
            return base.SaveProperties(aHeading);

        }


        /// <summary>
        ///^returns the objects properties in a collection
        /// ~signatures like "COLOR=RED"
        /// </summary>
        public override uopProperties CurrentProperties() { UpdatePartProperties(); return base.CurrentProperties(); }

        public override bool SetCurrentProperties(uopProperties value)
        {
            if (value == null) return false;
            if (base.SetProps(value))
            {
                RecalcDependantValues();
                UpdatePartProperties();
                return true;
            }
            return false;
        }

        /// <summary>
        /// the distributor that owns this case
        /// </summary>
        public mdDistributor Distributor() => (mdDistributor)uopEvents.RetrieveDistributor(ProjectHandle, DistributorIndex);



        public override uopPart Clone(bool aFlag = false) => (uopPart)this.Clone();

        /// <summary>
        ///returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>
        public mdDistributorCase Clone() => new mdDistributorCase(this);

        
        public bool IsEqual(mdDistributorCase aCase) => aCase != null && TPROPERTIES.Compare(aCase.ActiveProps, ActiveProps);
       

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


                uopPropertyArray myfileprops = aFileProps.PropertiesStartingWith(aFileSection);


                if (myfileprops == null || myfileprops.Count <= 0)
                {
                    ioWarnings?.AddWarning(this, $"{PartName} Data Not Found", $"File '{ aFileProps.Name }' Does Not Contain {aFileSection} Info!");
                    return;
                }
                List<string> skippers = aSkipList == null ? new List<string>() : new List<string>(aSkipList);
               
                //uopProperties myprops = base.ActiveProperties;
                //List<uopProperty> mydependantprops = myprops.FindAll((x) => x.Protected == true);
                //foreach (uopProperty prop in mydependantprops)
                //{
                //    if (!skippers.Contains(prop.Name)) skippers.Add(prop.Name);
                //}

                base.ReadProperties(aProject, myfileprops, ref ioWarnings, aFileVersion, aFileSection, bIgnoreNotFound, aSkipList: skippers, EqualNames: EqualNames);
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

                RecalcDependantValues();
                aProject?.ReadStatus("", 2);
                Reading = false;
             
            }
        }
        public virtual void SubPart(iCaseOwner aOwner, string aCategory = null, bool? bHidden = null)
        {
            if (aOwner == null) return;
            if (aOwner.OwnerType != uppCaseOwnerOwnerTypes.Distributor) return;
            PropValSet("DistributorType", aOwner.Description, bSuppressEvnts: true);

        }

         void RecalcDependantValues(bool bUpdatePartProperties = true)
        {
            try
            {
                if(bUpdatePartProperties) UpdatePartProperties();
                double denom1 = TotalFeedVolume;
                double denom2 = TotalLiquidVolume;
                double denom3 = TotalVaporVolume;

                if (LiquidFromAbove <= 0)
                    {  PropValSet("DistributorType", "Reflux", bSuppressEvnts: true); }
                else
                    { PropValSet("DistributorType", "Feed", bSuppressEvnts: true); }

                PropValSet("DesignRate", 0, bSuppressEvnts: true);
                if (VaporRate > 0) DesignRate = VaporRate;
              
                if (LiquidRate > 0)  PropValSet("DesignRate", ActiveProps.ValueD("DesignRate") + LiquidRate, bSuppressEvnts: true);

                //    DesignRate = VaporRate + LiquidRate

                VaporPercentage = (denom1 > 0 && VaporDensity > 0) ? VaporRate / VaporDensity / denom1 * 100 : 0;

                FeedLiquidPercentage = (denom2 > 0) ? LiquidFeedVolume / denom2 * 100 : 0;

                FeedVaporPercentage = (denom3 > 0) ? VaporFeedVolume / denom3 * 100 : 0;

            }
            catch (Exception)
            {
                //Log error
            }
        }


        public override void UpdatePartProperties()
        {
            DescriptiveName = Name;
            PropValSet("FeedLiquidVolumeRate", LiquidFeedVolume, bSuppressEvnts: true);
            PropValSet("FeedVaporVolumeRate", VaporFeedVolume, bSuppressEvnts: true);
            PropValSet("TotalLiquidVolumeRate", TotalLiquidVolume, bSuppressEvnts: true);
            PropValSet("TotalVaporVolumeRate", TotalVaporVolume, bSuppressEvnts: true);
            PropValSet("FeedVolumnRate", TotalFeedVolume , bSuppressEvnts: true);
            PropValSet("FeedDensity", FeedAverageDensity, bSuppressEvnts: true);
            RecalcDependantValues(false);
        }

        public override void UpdatePartWeight() => base.Weight = 0;



        /// <summary>
        /// executed internally to create the page(s) for a MD Spout project report
        /// </summary>
        /// <param name="rTray"></param>
        /// <param name="aReportType"></param>
        /// <returns></returns>
        public uopProperties ReportProperties(dynamic rTray, uppReportTypes aReportType)
        {
            
            uopPipe nPipe = null;
            uopPipe pPipe = null;
           
            
            string txt = string.Empty;
            TVALUES rVals = new TVALUES();
            bool bDistribReport = aReportType == uppReportTypes.MDDistributorReport;
            rVals.SetValues(string.Empty, aNewCount: 9);
            rTray = Distributor();
            uopProperties _rVal = new uopProperties();
            if (rTray == null)  return _rVal;
         
            if (bDistribReport)
            {
                nPipe = rTray.NozzleObject;
                pPipe = rTray.PipeObject;
            }

            

            txt = Description;
            //1
            _rVal.Add(txt, txt);
            if (rTray.Description == string.Empty)
            {
                _rVal.Add("Distributor", rTray.Name);
            }
            else
            {
                _rVal.Add("Distributor", rTray.Description);
            }
            //3
            _rVal.Add("Nozzle Label", rTray.NozzleLabel);
            //4
            _rVal.Add("Tray Above", rTray.TrayAbove);
            //5
            _rVal.Add( "Tray Below", rTray.TrayBelow);
            //6
            _rVal.Add(Prop("DesignRate"), "Design Rate");
            //7
            _rVal.Add(Prop("VaporPercentage"), "Vapor");

            TPROPERTY aProp = Prop("LiquidRate");
            TPROPERTY bProp = Prop("LiquidDensity");
            if (mzUtils.IsNumeric(aProp.Value) && aProp.Value <= 0)  bProp.Value = "-";
      
            //8
            _rVal.Add(aProp, "Feed Liquid Rate");
            //9
            _rVal.Add(bProp, "Feed Liquid Density");

            aProp = Prop("VaporRate");
            bProp = Prop("VaporDensity");
            if (mzUtils.IsNumeric(aProp.Value) && aProp.Value <= 0) 
            {
                string ucap = bProp.Units.GetUnitLabel(uppUnitFamilies.English);
                bProp = new TPROPERTY("Feed Vapor Density", "-");
                bProp.SetUnitCaption(ucap);
                
            }
               
        

            //10
            _rVal.Add(aProp, "Feed Vapor Rate");
            //11
            _rVal.Add(bProp, "Feed Vapor Density");

            aProp = Prop("LiquidFromAbove");
            bProp = Prop("LiquidDensityFromAbove");
            if (mzUtils.IsNumeric(aProp.Value) && aProp.Value <= 0)
            {
                string ucap = bProp.Units.GetUnitLabel(uppUnitFamilies.English);
                bProp = new TPROPERTY("Liquid Density From Above", "-");
                bProp.SetUnitCaption(ucap);

            }
            //12
            _rVal.Add(aProp, "Liquid From Above");
            if (bDistribReport)
            {
                _rVal.Add(Prop("AdditionalTroughLiquidRate"), "Addt'l Liq.To Troughs");
            }

            //13
            _rVal.Add(bProp, "Liquid Density From Above");

            //14
            _rVal.Add(Prop("FeedLiquidPercentage"), "Feed Liquid/Total Liquid");

            aProp = Prop("VaporFromBelow");
            bProp = Prop("VaporDensityFromBelow");
            if (mzUtils.IsNumeric(aProp.Value) && aProp.Value <= 0)
            {
                string ucap = bProp.Units.GetUnitLabel(uppUnitFamilies.English);
                bProp = new TPROPERTY("Vapor Density From Below", "-");
                bProp.SetUnitCaption(ucap);

            }

            //15
            _rVal.Add(aProp, "Vapor From Below");
            //16
            _rVal.Add(bProp, "Vapor Density From Below");

            //17
            _rVal.Add(Prop("FeedVaporPercentage"), "Feed Vapor/Total Vapor");

            aProp = Prop("MinimumOperatingRange");
            bProp = Prop("MaximumOperatingRange");
            //18

            if (aProp.Value != 0 && bProp.Value != 0)
            {
                _rVal.Add("Operating Range", aProp.UnitValueString(uppUnitFamilies.English, aOverridePrecision: 0) + " - " + bProp.UnitValueString(uppUnitFamilies.English, aOverridePrecision: 0), bProtected: true, aUnitCaption: "%");
            }
            else
            {
                _rVal.Add("Operating Range", "-");
            }
            if (!bDistribReport)
            {
                //19
                _rVal.Add("Distributor Required", mzUtils.BooleanToString(rTray.DistributorRequired, true));

                //20
                if (rTray.DistributorRequired)
                {
                    _rVal.Add("Minimize Pressure Drop", mzUtils.BooleanToString(rTray.MinimizePressureDrop, true));
                }
                else
                {
                    _rVal.Add("Minimize Pressure Drop", "-");
                }

                txt = rTray.ContainsHFTubes?.Trim();
                if (txt != string.Empty && (txt == "N\\A" || txt == "N/A" )) txt = "-"; 


                //21
                _rVal.Add("High Flux Tubes", txt);

            }
            //TODO : the below code is for uopMDDistributorReport type not required for current release
            //TPROPERTIES dProps = rTray.ActiveProps;
            //else
            //{
            //    //       vals_SetValue rVals, 7, Format(rTray.TroughHead(Me).Value, "0.00")
            //    //       vals_SetValue rVals, 8, Format(rTray.TroughHead(Me, MaximumOperatingRange).Value, "0.00")
            //    //       vals_SetValue rVals, 9, Format(rTray.TroughHead(Me, MinimumOperatingRange).Value, "0.00")

            //    //22    .AddProperty2 "Liquid Head at Design", rVals.Members(6), uniSmallLength, bProt
            //    //23     .AddProperty2 "Liquid Head at Maximum", rVals.Members(7), uniSmallLength, bProt
            //    //24     .AddProperty2 "Liquid Head at Minimum", rVals.Members(8), uniSmallLength, bProt


            //    vals_SetValues(rVals, "");

            //    rVals.SetValue( 1, uopProps.prop_Value<string, string>(dProps, "NozzleCount"));
            //    bProt = nPipe != null && rVals.Members[0] > 0;
            //    if (bProt)
            //    {
            //        rVals.SetValue( 2, nPipe.Schedule);
            //        rVals.SetValue( 3, nPipe.NominalDia);
            //        rVals.SetValue( 4, nPipe.IDProperty().GetUnitVariant(aToUnits: uppUnitFamilies.English));
            //        rVals.SetValue( 5, rTray.NozzleVelocity(this, nPipe).Value);
            //        rVals.SetValue( 6, rTray.NozzleRhoVsqr(this, nPipe).Value);
            //    }

            //    //25
            //    _rVal.AddProperty2("Number of Nozzles", rVals.Members[0]);
            //    //26
            //    _rVal.AddProperty2("Nozzle Schedule", rVals.Members[1]);
            //    //27
            //    _rVal.AddProperty2("Nominal Nozzle Size", rVals.Members[2]);
            //    //28
            //    _rVal.AddProperty2("Nozzle Inner Diameter", rVals.Members[3], uppUnitTypes.SmallLength, bProt);
            //    //29
            //    _rVal.AddProperty2("Nozzle Velocity", rVals.Members[4], uppUnitTypes.Velocity, bProt);
            //    //30
            //    _rVal.AddProperty2("Nozzle " + "rv" + mzUtils.SafeChr(178), rVals.Members[5], uppUnitTypes.RhoVSqr, bProt);

            //    vals_SetValues(rVals, "");
            //    bProt = pPipe != null;
            //    if (bProt)
            //    {
            //        rVals.SetValue( 1, uopProps.prop_Value<string, string>(dProps, "PipeConfiguration", 0, true));
            //        rVals.SetValue( 2, pPipe.Schedule);
            //        rVals.SetValue( 3, pPipe.NominalDia);
            //        rVals.SetValue( 4, pPipe.IDProperty().GetUnitVariant(aToUnits: uppUnitFamilies.English));
            //        rVals.SetValue( 5, rTray.PipeVelocity(this).Value);
            //        rVals.SetValue( 6, rTray.PipeRhoVsqr(this).Value);
            //    }

            //    //31
            //    _rVal.AddProperty2("Pipe Configuration", rVals.Members[0]);
            //    //32
            //    _rVal.AddProperty2("Pipe Schedule", rVals.Members[1]);
            //    //33
            //    _rVal.AddProperty2("Nominal Pipe Size", rVals.Members[2]);
            //    //34
            //    _rVal.AddProperty2("Pipe Inner Diameter", rVals.Members[3], uppUnitTypes.SmallLength, bProt);
            //    //35
            //    _rVal.AddProperty2("Final Pipe Velocity", rVals.Members[4], uppUnitTypes.Velocity, bProt);
            //    //36
            //    _rVal.AddProperty2("Pipe " + "rv" + mzUtils.SafeChr(178), rVals.Members[5], uppUnitTypes.RhoVSqr, bProt);

            //    vals_SetValues(rVals, "");
            //    rVals.SetValue( 1, uopProps.prop_Value<string, string>(dProps, "PipeHoleCount"));
            //    bProt = pPipe != null && rVals.Members[0] > 0;
            //    if (bProt)
            //    {
            //        rVals.SetValue( 2, rTray.PipeHoleDiameter.Value);
            //        rVals.SetValue( 3, rTray.PipeHoleArea().Value);
            //        rVals.SetValue( 4, rTray.PipeHoleVelocity(this).Value);
            //        rVals.SetValue( 5, rTray.PipeHoleRhoVSqr(this).Value);
            //        rVals.SetValue( 6, rTray.PipeHoleVelocity(this, true).Value);
            //        rVals.SetValue( 7, Convert.ToDouble(rTray.PipeHoleAreaRatio().Value).ToString("0.00"));

            //    }

            //    //37
            //    _rVal.AddProperty2("Number of Holes in Pipe", rVals.Members[0]);
            //    //38
            //    _rVal.AddProperty2("Pipe Hole Size", rVals.Members[1], uppUnitTypes.SmallLength, bProt);
            //    //39
            //    _rVal.AddProperty2("Total Pipe Hole Area", rVals.Members[2], uppUnitTypes.BigArea, bProt);
            //    //40
            //    _rVal.AddProperty2("Pipe Hole Velocity", rVals.Members[3], uppUnitTypes.Velocity, bProt);
            //    //41
            //    _rVal.AddProperty2("Pipe Hole " + "rv" + mzUtils.SafeChr(178), rVals.Members[4], uppUnitTypes.RhoVSqr, bProt);
            //    //42
            //    _rVal.AddProperty2("Hole Velocity (Turndown)", rVals.Members[5], uppUnitTypes.Velocity, bProt);
            //    //43
            //    _rVal.AddProperty2("Pipe/Hole Area", rVals.Members[6]);
            //    //44        .AddProperty2 "Calculated Pressure Drop", "", , False, , "psi"

            //    rVals.SetValue( 1, uopProps.prop_Value<string, string>(dProps, "Tiers"));
            //    //45
            //    _rVal.AddProperty2("Number of Tiers", rVals.Members[0]);

            //    rVals.SetValue( 1, uopProps.prop_Value<string, string>(dProps, "Troughs"));
            //    //46
            //    _rVal.AddProperty2("Number of Troughs", rVals.Members[0]);

            //    vals_SetValues(rVals, "");
            //    rVals.SetValue( 1, rTray.TroughHoleCount);
            //    rVals.SetValue( 2, rTray.Troughs);
            //    rVals.SetValue( 3, rTray.TroughHeight);
            //    bProt = rVals.Members[0] > 0 && rVals.Members[1] > 0 && rVals.Members[2] > 0;
            //    if (bProt)
            //    {
            //        rVals.SetValue( 4, rTray.TroughHoleSize);
            //        rVals.SetValue( 5, rTray.TroughHoleArea().Value);
            //        rVals.SetValue( 6, uopProps.prop_Value<string, string>(dProps, "DispersionDevice", 0, true));
            //        rVals.SetValue( 7, Convert.ToDouble(rTray.TroughHead(this).Value).ToString("0.00"));
            //        rVals.SetValue( 8, Convert.ToDouble(rTray.TroughHead(this, MaximumOperatingRange).Value).ToString("0.00"));
            //        rVals.SetValue( 9, Convert.ToDouble(rTray.TroughHead(this, MinimumOperatingRange).Value).ToString("0.00"));
            //    }

            //    //47
            //    _rVal.AddProperty2("Number of Holes in Troughs", rVals.Members[0]);
            //    //48
            //    _rVal.AddProperty2("Trough Hole Size", rVals.Members[3], uppUnitTypes.SmallLength, bProt);
            //    //49
            //    _rVal.AddProperty2("Total Trough Hole Area", rVals.Members[4], uppUnitTypes.BigArea, bProt);
            //    //50
            //    _rVal.AddProperty2("Trough Height (inside)", rVals.Members[2], uppUnitTypes.SmallLength, bProt);
            //    //51
            //    _rVal.AddProperty2("Dispersion Device", rVals.Members[5]);
            //    //52
            //    _rVal.AddProperty2("Liquid Head at Design", rVals.Members[6], uppUnitTypes.SmallLength, bProt);
            //    //53
            //    _rVal.AddProperty2("Liquid Head at Maximum", rVals.Members[7], uppUnitTypes.SmallLength, bProt);
            //    //54
            //    _rVal.AddProperty2("Liquid Head at Minimum", rVals.Members[8], uppUnitTypes.SmallLength, bProt);

            //    rVals.SetValue( 0, uopProps.prop_Value<string, string>(dProps, "Tiers"));
            //    if (rVals.SetValue( 1, 0))
            //    {
            //        dProps = _rVal.Structure;
            //        for (int i = 40; i < 48; i++)
            //        {
            //            dProps.Members[i - 1].Value = "-";
            //        }
            //        _rVal.Structure = dProps;

            //    }
            //}

           TPROPERTIES dProps =  new TPROPERTIES(_rVal);
            for (int i = 1; i <= dProps.Count; i++)
            {
                aProp = dProps.Item(i);
              
                aProp.Row = i;
                aProp.Protected = true;
                dProps.SetItem(i, aProp);
            }
            
           
                return new uopProperties(dProps);
        }
 

        public override bool IsEqual(uopPart aPart)
        {
            if (aPart == null)  return false;
            return aPart.PartType == PartType && IsEqual((mdDistributorCase)aPart);
        }


        /// <summary>
        /// '^used by an object to respond to property changes of itself and of objects below it in the object model.
        ///'^also raises the changed property to the object above it it the object model.
        /// </summary>
        /// <param name="aProperty"></param>
        private void Notify(uopProperty aProperty)
        {


            if (aProperty == null || aProperty.Protected) return;
         

            //TODO//DistributorCasePropertyChange(aProperty);

            mdDistributor myParent;
            string pname;
            pname = aProperty.Name.ToUpper();

            if (string.Compare(aProperty.Name, "Name") != 0)
            {
                RecalcDependantValues();
                UpdatePartProperties();
            }

            if (!SuppressEvents)
            {
                myParent = Distributor();
                if (myParent != null) myParent.Notify(aProperty);
      
            }
        }

        //Public Property Get NozzleObject() As uopPipe
        //    '^the nozzle of the distributor that owns this case
        //    Dim aDistrib As mdDistributor
        //    Set aDistrib = Distributor
        //    if Not aDistrib Is Nothing Then Set NozzleObject = aDistrib.NozzleObject
        //}


        public override uopProperty CurrentProperty(string aPropertyName, bool bSupressNotFoundError = false) { UpdatePartProperties(); return base.CurrentProperty(aPropertyName, bSupressNotFoundError); }
        /// <summary>
        /// Gets value of the property
        /// </summary>
        /// <param name="aPropertyNameorIndex"></param>
        /// <param name="bExists"></param>
        /// <returns></returns>
        public dynamic GetPropertyValue(dynamic aPropertyNameorIndex, out bool bExists) => PropValGet(aPropertyNameorIndex, out bExists, bDecodedValue: false, aPartType: uppPartTypes.Undefined);

        #endregion Methods

        #region ICase Support

        /// <summary>
        /// Clone method inherited from iCase
        /// </summary>
        /// <returns></returns>
        iCase iCase.Clone() => (iCase)Clone();


        void iCase.RecalculateDependantValues() => RecalcDependantValues();


        string iCase.Description { get => Description; set => Description = value; }

        int iCase.Index { get => Index; set => Index = value; }

        double iCase.MaximumOperatingRange { get => MaximumOperatingRange; set => MaximumOperatingRange = value; }

        double iCase.MinimumOperatingRange { get => MinimumOperatingRange; set => MinimumOperatingRange = value; }

        string iCase.Name => Name;

        uppPartTypes iCase.PartType => uppPartTypes.DistributorCase;

        string iCase.ObjectType => "Distributor Case";

        uppCaseOwnerOwnerTypes iCase.OwnerType => uppCaseOwnerOwnerTypes.Distributor; 

        int iCase.OwnerIndex => DistributorIndex;


        string iCase.PartPath => PartPath();
       
        dynamic iCase.GetPropertyValue(dynamic aPropertyNameorIndex, out bool bExists)
        { return PropValGet(aPropertyNameorIndex, out bExists); }

        bool iCase.PropValSet(dynamic PropertyNameorIndex, dynamic NewValue)
        {
            uopProperty aProp = PropValSet(PropertyNameorIndex, NewValue);
            if (aProp == null) return false;
            Notify(aProp);
            return true;
        }


        #endregion ICase Support

    }
}