using System;
using System.Collections.Generic;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Parts;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;

namespace UOP.WinTray.Projects
{
    /// <summary>
    /// represents a hydraulic stage within a MD column
    /// this object is simply defined bby it properties collection
    /// </summary>
    public class mdStage : uopPart
    {
        public override uppPartTypes BasePartType => uppPartTypes.Stage;
        public mdStage() : base(uppPartTypes.Stage, uppProjectFamilies.uopFamMD, "", "", false)
        {
            InitializeProperties();
        }

        internal mdStage(mdStage aPartToCopy)  :base(uppPartTypes.Stage, uppProjectFamilies.uopFamMD, "","",false)
        {
            if (aPartToCopy == null)
            { InitializeProperties(); }
            else
            { Copy(aPartToCopy); }
        }

        private void InitializeProperties()
        {

            AddProperty("StageNumber", 0, aDisplayName: "Stage No.", aCategory: "Other");
            AddProperty("KeyNumber", "", aDisplayName: "Key", aCategory: "Other");
            AddProperty("CustomerName", "", aDisplayName: "Customer", aCategory: "Other");
            AddProperty("FileType", "", bIsHidden: true, aCategory: "Other");
            AddProperty("DisplayUnits", "", bIsHidden: true, aCategory: "Other");
            AddProperty("OutputUnits", "", bIsHidden: true, aCategory: "Other");
            AddProperty("StageTitle", "", aDisplayName: "Title", aCategory: "Other");
            AddProperty("LiquidRate", 0, uppUnitTypes.BigMassRate, aDisplayName: "Liq. Rate", aCategory: "Other");
            AddProperty("VaporRate", 0, uppUnitTypes.BigMassRate, aDisplayName: "Vap. Rate", aCategory: "Other");
            AddProperty("LiquidDensity", 0.0, uppUnitTypes.Density, aDisplayName: "Liq. Density", aCategory: "Other");
            AddProperty("VaporDensity", 0.0, uppUnitTypes.Density, aDisplayName: "Vap. Density", aCategory: "Other");
            AddProperty("LiquidViscosity", 0.0, uppUnitTypes.Viscosity, aDisplayName: "Liq. Viscos.", aCategory: "Other");
            AddProperty("VaporViscosity", 0.0, uppUnitTypes.Viscosity, aDisplayName: "Vap. Viscos.", aCategory: "Other");
            AddProperty("SurfaceTension", 0.0, uppUnitTypes.SurfaceTension, aDisplayName: "Surface Tension", aCategory: "Other");
            AddProperty("ShellID", 0.0, uppUnitTypes.BigLength, aDisplayName: "Shell ID", aCategory: "Mechanical");
            AddProperty("DCCount", 0, aDisplayName: "DC Count", aCategory: "Mechanical");
            AddProperty("DCWidth", 0.0, uppUnitTypes.SmallLength, aDisplayName: "DC Width", aCategory: "Mechanical");
            AddProperty("DCType", "", aDisplayName: "DC Type", aCategory: "Other");
            AddProperty("TraySpacing", 0.0, uppUnitTypes.SmallLength, aDisplayName: "Tray Spacing", aCategory: "Mechanical");
            AddProperty("DCHeight", 0.0, uppUnitTypes.SmallLength, aDisplayName: "DC Height", aCategory: "Mechanical");
            AddProperty("DeckThickness", 0.0, uppUnitTypes.SmallLength, aDisplayName: "Deck Thk.", aCategory: "Mechanical");
            AddProperty("PerfDiameter", 0.0, uppUnitTypes.SmallLength, aDisplayName: "Perf. Dia.", aCategory: "Mechanical");
            AddProperty("DCSpacing", 0.0, uppUnitTypes.SmallLength, aDisplayName: "DC Spacing", aCategory: "Other");
            AddProperty("WeirHeight", 0.0, uppUnitTypes.SmallLength, aDisplayName: "Weir Height", aCategory: "Mechanical");
            AddProperty("StabilityFactor", 0.0, aDisplayName: "Stab. Fctr.", aCategory: "Mechanical");
            AddProperty("PerfFraction", 0.0, aDisplayName: "Perf. Fract.", aCategory: "Mechanical");
            AddProperty("SlotFraction", 0.0, aDisplayName: "Slot Fract.", aCategory: "Mechanical");
            AddProperty("AlphaT", "", aDisplayName: "Alpha T", aCategory: "Other");
            AddProperty("AlphaD", "", aDisplayName: "Alpha D", aCategory: "Other");
            AddProperty("AlphaR", "", aDisplayName: "Alpha R", aCategory: "Other");
            AddProperty("SpoutArea", 0.0, uppUnitTypes.SmallArea, aDisplayName: "Spout Area", aCategory: "Mechanical");
            AddProperty("SystemFactor", "", aDisplayName: "System Factor", aCategory: "Other");
            AddProperty("DCThickness", 0.0, uppUnitTypes.SmallLength, aDisplayName: "DC Thk.", aCategory: "Mechanical");
            AddProperty("Seal", "", aDisplayName: "Seal", aCategory: "Other");
            AddProperty("Clearance", "", aDisplayName: "Clearance", aCategory: "Mechanical");
            AddProperty("RingWidth", 0.0, uppUnitTypes.SmallLength, aDisplayName: "Ring Width", aCategory: "Mechanical");
            AddProperty("WeirLength_ft", 0.0, uppUnitTypes.BigLength, aDisplayName: "Weir Lg.", aCategory: "Other");
            AddProperty("FlowMultiplier1", "", aDisplayName: "Flow Multi1", aCategory: "Other");
            AddProperty("FlowMultiplier2", "", aDisplayName: "Flow Multi2", aCategory: "Other");
            AddProperty("FlowMultiplier3", "", aDisplayName: "Flow Multi3", aCategory: "Other");
            AddProperty("PerfCoefficient", "", aDisplayName: "Perf. Coeff.", aCategory: "Other");
            AddProperty("TrayType", "", aDisplayName: "Tray Type", aCategory: "Mechanical");
            AddProperty("InstallationLocation", "", aDisplayName: "Loc.", aCategory: "Other");
            AddProperty("CustomerService", "", aDisplayName: "Service", aCategory: "Other");
            AddProperty("RangeGUID", "", bIsHidden: true);

            TPROPERTIES myProps = ActiveProps;
        }
        /// <summary>
        /// returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>
        public mdStage Clone() => new mdStage(this);


        public override uopPart Clone(bool aFlag = false) => (uopPart)this.Clone();

        /// <summary>
        /// the collection that this stage is a member of
        /// </summary>
        public new colMDStages Collection
        {
            get
            {
                mdProject Proj = GetMDProject();
                return Proj?.Stages;
                
            }
        }
        /// <summary>
        /// returns the objects properties in a collection
        ///  signatures like "COLOR=RED"
        /// </summary>
         public override uopProperties CurrentProperties()  { UpdatePartProperties(); return base.CurrentProperties(); }

        public override uopProperty CurrentProperty(string aPropertyName, bool bSupressNotFoundError = false)
        {
            UpdatePartProperties();
            return base.CurrentProperty(aPropertyName, bSupressNotFoundError);
        }


        public override string Name { get => StageName; set {  } }

        /// <summary>
        /// 1a coolection of strings to use to define the stage properties
        /// defines the 43 stage properties with the passed string collection
        /// </summary>
        /// <param name="aValCol"></param>
        public void DefinePropertyValues(List<string> aValCol)
        {
            if (aValCol == null) throw new Exception("[mdstage.DefinePropertyValues] The Passed Collection Is Undefined");
            
            if (aValCol.Count < 42)
            {
                throw new Exception("[mdstage.DefinePropertyValues] The Passed Collection Does Not Contain 43 Values As Required");
            }

            TPROPERTIES Props = ActiveProps;
          
            int pid = 0;

            for (int i = 0; i < aValCol.Count; i++)
            {
                pid++;
                TPROPERTY aProp = Props.Item(pid);
                string sVal = aValCol[i];

                if (i == 24)
                {
                    if (double.TryParse(sVal, out double etatorfracperf) && etatorfracperf <= 0.4)
                    {
                        aProp = Props.Item(pid + 1);  //skip
                    }
                    pid++;
                }

                string tname = aProp.Value.GetType().ToString().ToUpper().Replace("SYSTEM.", "");

                if (tname == "STRING")
                {
                    aProp.SetValue(sVal);
                }
                else if ((tname == "INT32" || tname == "LONG") && int.TryParse(sVal, out int intVal))
                {
                    aProp.SetValue(intVal);
                }
                else if ((tname == "DOUBLE" || tname == "SINGLE") && double.TryParse(sVal, out double doubleVal))
                {
                    aProp.SetValue(doubleVal);
                }
                else
                {
                    sVal = aValCol[i];
                }

                if (string.Compare(aProp.Name, "Clearance", true) == 0)
                {
                    aProp.Value = string.Empty;
                }

                Props.SetItem(aProp.Index, aProp);
            }

            ActiveProps = Props;
        }

           public override string INIPath => $"STAGES({ Index })";
            
        
        /// <summary>
        /// returns true if the stage is equivalent to the passed stage
        /// </summary>
        /// <param name="aStage">the stage to compare this one too</param>
        /// <param name="bMechanicallyOnly">flag to only compare on the properties that are mechanical</param>
        /// <param name="aPrecis"></param>
        /// <returns></returns>
        public bool IsEqual(mdStage aStage, bool bMechanicallyOnly = false, int aPrecis = 4)
        {
           
            if (aStage == null) return false;
            uopProperties myProps = ActiveProperties;
            uopProperties herProps = aStage.ActiveProperties;
            List<string> skippers = null;
            if (bMechanicallyOnly)
            {
                myProps = new uopProperties( myProps.FindAll(x => string.Compare(x.Category, "Mechanical",true) ==0));
                herProps = new uopProperties(herProps.FindAll(x => string.Compare(x.Category, "Mechanical", true) == 0));
                skippers = new List<string>() { "StabilityFactor" };
            }
            uopProperties difs = myProps.GetDifferences(herProps, aPrecis,false);
            if (bMechanicallyOnly)
            {
                if (difs.Count > 1) return false;
                if (difs.Count <= 0) return true;
                if(string.Compare(difs[0].Name, "StabilityFactor", true) != 0) return false;
                //the only difference is stability factor so if they are greater than 1 ignore them
                return (myProps.ValueD("StabilityFactor") >= 1 && herProps.ValueD("StabilityFactor") >= 1);
            }
            else
            {
                return difs.Count > 0;
            }
           //return TPROPERTIES.Compare(myProps, herProps, aPrecis, skippers);
          
        }
        /// <summary>
        /// '^used by an object to respond to property changes of itself and of objects below it in the object model.
        /// also raises the changed property to the object above it it the object model.
        /// </summary>
        /// <param name="aProperty"></param>
        public void Notify(uopProperty aProperty)
        {
            if (aProperty == null) return;
            colMDStages myCol = Collection;
            if (myCol != null) myCol.NotifyMemberChange(this, aProperty);
        }

    
       
 

        /// <summary>
        /// builds a collection of all the know mdh column stage property names as defined
        /// in a MDH file comma delimated file
        /// </summary>
        public List<string> PropertyNames
        {
            get
            {
                List<string> propNames = new List<string>
                {
                    //create a collection of 43 know property names in order that the appear
                    //in the comma delimated stage definition strings
                    "StageNumber",
                    "KeyNumber",
                    "CustomerName",
                    "FileType",
                    "DisplayUnits",
                    "OutputUnits",
                    "StageTitle",
                    "LiquidRate",
                    "VaporRate",
                    "LiquidDensity",
                    "VaporDensity",
                    "LiquidViscosity",
                    "VaporViscosity",
                    "SurfaceTension",
                    "ShellID",
                    "DCCount",
                    "DCWidth",
                    "DCType",
                    "TraySpacing",
                    "DCHeight",
                    "DeckThickness",
                    "PerfDiameter",
                    "DCSpacing",
                    "WeirHeight",
                    "StabilityFactor",
                    "PerfFraction",
                    "SlotFraction",
                    "AlphaT",
                    "AlphaD",
                    "AlphaR",
                    "SpoutArea",
                    "SystemFactor",
                    "DCThickness",
                    "Seal",
                    "Clearance",
                    "RingWidth",
                    "WeirLength_ft",
                    "FlowMultiplier1",
                    "FlowMultiplier2",
                    "FlowMultiplier3",
                    "PerfCoefficient",
                    "TrayType",
                    "InstallationLocation",
                    "CustomerService"
                };
                return propNames;
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


 
              
                string sval = aFileProps.ValueS(aFileSection, "DefLine1", out bool found);
             
                if (sval ==  string.Empty || !found)
                {
                    //in support of original saving method (obsolete)
                    aFileSection = $"Column(1).{ aFileSection}";
                    sval = aFileProps.ValueS(aFileSection, "DefLine1", out found);
                }
                if (sval ==  string.Empty || !found)
                {
                    return;
                }
                sval = sval.Substring(1, sval.Length - 2);

                string defline = sval + ",";
                for (int i = 2; i <= 8; i++)
                {
                    sval = aFileProps.ValueS(aFileSection, $"DefLine{i}", out found);
                    if (found)
                    {
                        sval = sval.Substring(1, sval.Length - 2);
                    }
                    if (i == 2 && (!found || sval ==  string.Empty)) sval = new string(',', 2);

                    if (i == 3 && (!found || sval ==  string.Empty)) sval = string.Empty;

                    if (i == 4 && (!found || sval ==  string.Empty)) sval = new string(',', 5); 

                    if (i == 5 && (!found || sval ==  string.Empty)) sval =  new string(',', 9); //",,,,,,,,,"

                    if (i == 6 && (!found || sval ==  string.Empty)) sval =  new string(',', 7); //",,,,,,,";

                    if (i == 7 && (!found || sval ==  string.Empty)) sval = new string(',', 7); //",,,,,,,";

                    if (i == 8 && (!found || sval ==  string.Empty)) sval = new string(',', 2); //",,";

                    defline += sval;
                    if (i < 8) defline += ",";

                }

                List<string> vals = uopUtils.ParseDeliminatedString(defline, ',', true);
                DefinePropertyValues(vals);
                RangeGUID = aFileProps.ValueS(aFileSection,"RangeGUID");
                PropValSet("RangeGUID", RangeGUID, bSuppressEvnts: true);
               
            }
            catch (Exception e)
            {
                ioWarnings?.AddWarning(this, "Read Properties Error", e.Message);
            }
            finally
            {

                aProject?.ReadStatus("", 2);
                Reading = false;
            }
        }


        private string SaveLine(int LineNum)
        {
            string _rVal = string.Empty;
            
            switch (LineNum)
            {
                case 1:
                    _rVal = PropString(",", 1, 3, false, ((Char)34).ToString());
                    break;
                case 2:
                    _rVal = PropString(",", 4, 6, false, ((Char)34).ToString());
                    break;
                case 3:
                    _rVal = PropString(",", 7, 7, false, ((Char)34).ToString());
                    break;
                case 4:
                    _rVal = PropString(",", 8, 14, false, ((Char)34).ToString());
                    break;
                case 5:
                    _rVal = PropString(",", 15, 24, false, ((Char)34).ToString());
                    break;
                case 6:
                    double pfrac = 0;
                    double.TryParse(Convert.ToString(base.PropValD("PerfFraction")), out pfrac);
                    _rVal = pfrac + "," + PropString(",", 27, 33, false, ((Char)34).ToString());
                    break;
                case 7:
                    _rVal = PropString(",", 34, 41, false, ((Char)34).ToString());
                    break;
                case 8:
                    _rVal = PropString(",", 42, 44, false, ((Char)34).ToString());
                    break;
            }
            return _rVal;
        }

        public virtual List<string> SaveLines
        {
            get
            {
                List<string> SaveLines = new List<string>
                {
                    //^the 8 lines of text that define the stage in an mdh files
                    SaveLine(1),
                    SaveLine(2),
                    SaveLine(3),
                    SaveLine(4),
                    SaveLine(5),
                    SaveLine(6),
                    SaveLine(7),
                    SaveLine(8)
                };
                return SaveLines;
            }
        }
        /// <summary>
        /// returns only the properties that have a value defined for saving
        /// </summary>
        public override  uopPropertyArray SaveProperties(string aHeading = null)
        {
            try
            {
                aHeading = string.IsNullOrWhiteSpace(aHeading) ? INIPath : aHeading.Trim();
                uopProperties _rVal = new uopProperties();
                   
                for (int i = 1; i <= 8; i++)
                {
                _rVal.Add($"DefLine{ i}", "{" + SaveLine(i) + "}", aHeading: aHeading);
                }
            _rVal.Add("RangeGUID", RangeGUID, aHeading: aHeading);
                //_rVal.SetHeadings(INIPath);
                return new uopPropertyArray( _rVal , aName: aHeading, aHeading: aHeading);
            }
            catch (Exception e)
            {
                return null;
            throw e;
            }
            
        }
        /// <summary>
        /// sets the property with the passed name to the passed value
        /// </summary>
        /// <param name="PropName">the property to set</param>
        /// <param name="PropValGet">the value to set the property value to</param>
        public void SetProperty(string PropName, dynamic aPropVal)
        {
            uopProperty aProp = PropValSet(PropName, aPropVal);
            if (aProp != null) Notify(aProp);
        }

        /// <summary>
        /// a simple string used to refer to the stage
        /// </summary>
        public string StageName  => $"Stage {Index}";
            
        /// <summary>
        /// returns the stage number string for the stage
        /// </summary>
        public int StageNumber { get => PropValI("StageNumber"); set => SetProperty("StageNumber", value); }
    
     
        public override void UpdatePartWeight() => base.Weight = 0;
      
     
    }
}
