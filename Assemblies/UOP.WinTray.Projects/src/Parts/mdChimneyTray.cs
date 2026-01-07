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
    public class mdChimneyTray : uopPart, iCaseOwner
    {

        public override uppPartTypes BasePartType => uppPartTypes.ChimneyTray;

       
        private colUOPParts _Cases = null;
    

        public mdChimneyTray() : base(uppPartTypes.ChimneyTray, uppProjectFamilies.uopFamMD, "","")
        {
            InitializeProperties();
        }


        internal mdChimneyTray(mdChimneyTray aPartToCopy, colUOPParts aCases) : base(uppPartTypes.ChimneyTray, uppProjectFamilies.uopFamMD, "", "")
        {
            _Cases = aCases ?? new colUOPParts();
             base.Copy(aPartToCopy);
            _Cases.MaintainIndices = true;
        }
        private void InitializeProperties()
        {
            _Cases = new colUOPParts(this) { MaintainIndices = true };


            AddProperty("Description", "");
            AddProperty("NozzleLabel", "");
            AddProperty("TrayAbove", "", aNullVal: "");
            AddProperty("TrayBelow", "", aNullVal: "");
            PropsLockTypes(true);
        }



        public override string ToString() => $"CHIMNEYTRAY({ Index })";

        /// <summary>
        /// the flow cases defined for this distributor
        /// </summary>
        public colUOPParts Cases
        {
            get { _Cases.SubPart(this); return _Cases; }
            set
            {
                if (value == null)
                { _Cases.Clear(); }
                else
                { value.SubPart(this); }
                if (_Cases.Count <= 0) AddCase(new mdChimneyTrayCase());

            }
        }

        /// <summary>
        /// the collection that this tray is a member of
        /// </summary>
        public new colMDChimneyTrays Collection => (colMDChimneyTrays)base.Collection();

        /// <summary>
        ///returns the objects properties in a collection
        /// signatures like "COLOR=RED"
        /// </summary>
        public override uopProperties CurrentProperties() { UpdatePartProperties(); return base.CurrentProperties(); }

        uppCaseOwnerOwnerTypes iCaseOwner.OwnerType => uppCaseOwnerOwnerTypes.ChimneyTray;
        public bool AddCase(iCase aCase)
        {
            if (aCase == null) return false;
            if (aCase.OwnerType !=  uppCaseOwnerOwnerTypes.ChimneyTray || string.IsNullOrWhiteSpace(aCase.Description)) return false;
            if (Cases.FindIndex(x => string.Compare(aCase.Description, x.Description, true) == 0) > 0) return false;
            Cases.Add((uopPart)aCase);
            return true;

        }
       
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
            set=> Notify(PropValSet("Description", value)); 
        }
        public override string DescriptiveName
        {
            get
            {
                string _rVal = Description.Trim();
                if (_rVal ==  string.Empty) return _rVal;
                string tb = TrayBelow.Trim();
                string ta = TrayAbove.Trim();
                if (!string.IsNullOrWhiteSpace(ta) && string.Compare(ta, "-") != 0)
                {
                    _rVal += $" Below Tray {ta}";
                }
                else if (!string.IsNullOrWhiteSpace(tb) && string.Compare(tb, "-") != 0)
                {
                    _rVal += $" Above Tray {tb}";
                    
                }
                    return _rVal;
            }
        }

        /// <summary>
        /// INIPath
        /// </summary>
        public override string INIPath => "CHIMNEYTRAYS(" + Index + ")";
        
        /// <summary>
        /// the collection index of the tray
        /// </summary>
        public override int Index { get => PartIndex; set => PartIndex = Math.Abs(value); }

        /// <summary>
        /// Name
        /// </summary>
        public override string Name => "Tray " + Index;
        
        /// <summary>
        /// the name of the nozzle associated to this object
        /// </summary>
        public string NozzleLabel { get => PropValS("NozzleLabel"); set => Notify(PropValSet("NozzleLabel", value)); }

        /// <summary>
        /// Notifies the change of properties
        /// </summary>
        /// <param name="aProperty"></param>
        public void Notify(uopProperty aProperty)
        { if (aProperty == null || aProperty.Protected) return;  colMDChimneyTrays myCol = Collection; if (myCol != null) myCol.NotifyMemberChange(this, aProperty); }

        /// <summary>
        ///returns the properties required to save the object to file
        /// signatures like "COLOR=RED"
        /// </summary>
        public override  uopPropertyArray SaveProperties(string aHeading = null)
        {

            aHeading = string.IsNullOrWhiteSpace(aHeading) ? INIPath : aHeading;
                UpdatePartProperties();
            return base.SaveProperties(aHeading);
            
        }

        /// <summary>
        /// the tray number of the tray above this chimney tray
        /// </summary>
        public string TrayAbove { get => PropValS("TrayAbove"); set => Notify(PropValSet("TrayAbove", value)); }

        /// <summary>
        /// the tray number of the tray below this chimney tray
        /// </summary>
        public string TrayBelow { get => PropValS("TrayBelow"); set => Notify(PropValSet("TrayBelow", value)); }

        /// <summary>
        /// Span Name
        /// </summary>
        public new string SpanName => TrayAbove + "/" + TrayBelow;


        /// <summary>
        ///returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>
        public mdChimneyTray Clone() => new mdChimneyTray(this,Cases.Clone());
       
        public override uopPart Clone(bool aFlag = false) => (uopPart)this.Clone();

        /// <summary>
        /// Method to get cases
        /// </summary>
        /// <param name="aCaseIndex"></param>
        /// <returns></returns>
        public mdChimneyTrayCase GetCase(int aCaseIndex)
        {
            mdChimneyTrayCase GetCase = null;
            //if (aCaseIndex <= 0)
            //{
            //    return GetCase;

            //}
            if (aCaseIndex > Cases.Count)
            {
                return GetCase;

            }
            GetCase = (mdChimneyTrayCase)Cases.Item(aCaseIndex);
            GetCase.SubPart(this);
            return GetCase;
        }

        /// returns true if the passed part is equal to this one
        /// </summary>
        /// <param name="aTray"></param>
        /// <returns></returns>
        public bool IsEqual(mdChimneyTray aTray)
        {
            if (aTray == null) return false;
            if (aTray.Cases.Count != Cases.Count) return false;
            return TPROPERTIES.Compare(aTray.ActiveProps, ActiveProps, 4) && aTray.Cases.IsEqual(Cases);
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


                uopPropertyArray myprops = aFileProps.PropertiesStartingWith(aFileSection);


                if (myprops == null || myprops.Count <= 0)
                {
                    ioWarnings?.AddWarning(this, $"{PartName} Data Not Found", $"File '{ aFileProps.Name }' Does Not Contain {aFileSection} Info!");
                    return;
                }
                base.ReadProperties(aProject, myprops, ref ioWarnings, aFileVersion, aFileSection, bIgnoreNotFound, aSkipList: aSkipList, EqualNames: EqualNames);
                Reading = true;
                UpdatePartProperties();

                Cases.CollectionObj.Clear();
                for (int i = 1; i <= 1000; i++)
                {
                    string fsec = $"{aFileSection}.CASE({i})";
                    if (!myprops.Contains(fsec)) break;

                    mdChimneyTrayCase newcase = new mdChimneyTrayCase();
                    uopPropertyArray caseprops = aFileProps.PropertiesStartingWith(fsec);
                    newcase.Index = i;
                    newcase.ReadProperties(aProject, caseprops, ref ioWarnings, aFileVersion, fsec, bIgnoreNotFound, aSkipList: aSkipList);
                    newcase.Reading = true;
                    newcase.UpdatePartProperties();
                    newcase.SubPart(this);
                    newcase.Reading = false;
                    Cases.CollectionObj.Add(newcase);
                }


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
        /// <summary>
        /// Update Part Properties
        /// </summary>
        public override void UpdatePartProperties(){ DescriptiveName = "ChimneyTray(" + SpanName + ")"; }
        public uopDocuments GenerateWarnings(uopProject aProject, string aCategory = null, uopDocuments aCollector = null, bool bJustOne = false)
        {

            uopDocuments _rVal = aCollector ?? new uopDocuments();
            if (bJustOne && _rVal.Count > 0) return _rVal;
            mdProject myProj;

            if (aProject == null) { myProj = GetMDProject(); } else { myProj = (aProject.ProjectFamily == uppProjectFamilies.uopFamMD) ? (mdProject)aProject : GetMDProject(); }
            if (myProj == null) return _rVal;


            string sname = DescriptiveName;
            myProj.ReadStatus($"Generating { sname } Warnings", 1);


            _rVal.NewMemberCategory = sname;
            _rVal.NewMemberSubCategory = string.Empty;
            if (myProj.ProjectType == uppProjectTypes.MDSpout)
            {
                foreach (uopPart item in Cases)
                {

                mdChimneyTrayCase mycase = (mdChimneyTrayCase)item;
                mycase.UpdatePartProperties();
                if (mycase.LiquidFromAbove * mycase.LiquidDensity <= 0)
                {
                    _rVal.AddWarning(this, "Invalid Chimney Tray Load Case", $" Chimney Tray '{Description} - {mycase.Description}' Liquid Rate is Zero.", aOwnerName: $"ChimneysTrays({Index}).Cases({mycase.Index})", aCategory: aCategory);
                    if (bJustOne) break;
                }
                if (bJustOne && _rVal.Count > 0) return _rVal;
                if (mycase.VaporFromBelow * mycase.LiquidDensity <= 0)
                {
                    _rVal.AddWarning(this, "Invalid Chimney Tray Load Case", $" Chimney Tray '{Description} - {mycase.Description}' Vapor Rate is Zero.", aOwnerName: $"ChimneysTrays({Index}).Cases({mycase.Index})", aCategory: aCategory);
                    if (bJustOne) break;
                }
                if (bJustOne && _rVal.Count > 0) return _rVal;

                if (mycase.MinimumOperatingRange <= 0)
                {
                    _rVal.AddWarning(this, "Invalid Chimney Tray Load Case", $" Chimney Tray '{Description} - {mycase.Description}' Minimum Operating Range is Zero.", aOwnerName: $"ChimneysTrays({Index}).Cases({mycase.Index})", aCategory: aCategory);
                    if (bJustOne) break;
                }
                if (bJustOne && _rVal.Count > 0) return _rVal;
                if (mycase.MaximumOperatingRange <= 0)
                {
                    _rVal.AddWarning(this, "Invalid Chimney Tray Load Case", $" Chimney Tray '{Description} - {mycase.Description}' Maximum Operating Range is Zero.", aOwnerName: $"ChimneysTrays({Index}).Cases({mycase.Index})", aCategory: aCategory);
                    if (bJustOne) break;
                }
                if (bJustOne && _rVal.Count > 0) return _rVal;
            }
        }


            return _rVal;
        }

        /// <summary>
        /// Update Part Weight
        /// </summary>
        public override void UpdatePartWeight() => base.Weight = 0;

        /// <summary>
        /// IsEqual
        /// </summary>
        /// <param name="aPart"></param>
        /// <returns></returns>
        public override bool IsEqual(uopPart aPart)
        {

            if (aPart == null) return false;
            if (aPart.PartType != PartType) return false;

            mdChimneyTray thePart = (mdChimneyTray)aPart;

            return this.IsEqual(thePart);
        }


        #region ICaseOwner Support

        /// <summary>
        /// Cases
        /// </summary>
        colUOPParts iCaseOwner.Cases { get => Cases; set => Cases = value; }


        /// <summary>
        /// Cases
        /// </summary>
        List<iCase> iCaseOwner.CaseList 
        {
            get 
            {
                colUOPParts cases  = Cases;
                List<iCase> _rVal = new List<iCase>();
                foreach (var item in cases) { _rVal.Add((iCase)item); }
                return _rVal;
            }

            set 
            {
                colUOPParts cases = Cases;
                cases.Clear();
                if (value == null) return;
                foreach (var item in value) { cases.Add((uopPart)item); }
            } 
        }

        /// <summary>
        /// CurrentProperties
        /// </summary>
        uopProperties iCaseOwner.CurrentProperties() => CurrentProperties();

        /// <summary>
        /// Description
        /// </summary>
        string iCaseOwner.Description { get => Description; set => Description = value; }

        int iCaseOwner.CaseCount => Cases.Count;

        /// <summary>
        /// Index
        /// </summary>
        int iCaseOwner.Index => Index;

        /// <summary>
        /// Name
        /// </summary>
        string iCaseOwner.Name => Name;

        /// <summary>
        /// Nozzle Label
        /// </summary>
        string iCaseOwner.NozzleLabel { get => NozzleLabel; set => NozzleLabel = value; }

        /// <summary>
        /// Object Type
        /// </summary>
        string iCaseOwner.ObjectType => "Chimney Tray";



        /// <summary>
        /// Part path
        /// </summary>
        string iCaseOwner.PartPath => PartPath();

        /// <summary>
        /// TrayAbove
        /// </summary> 
        string iCaseOwner.TrayAbove { get => TrayAbove; set => TrayAbove = value; }

        /// <summary>
        /// TrayBelow
        /// </summary>
        string iCaseOwner.TrayBelow { get => TrayBelow; set => TrayBelow = value; }


        /// <summary>
        /// iCaseOwner clone
        /// </summary>
        /// <returns></returns>
        iCaseOwner iCaseOwner.Clone() => (iCaseOwner)Clone();
        
        /// <summary>
        /// Get case
        /// </summary>
        /// <param name="aCaseIndex"></param>
        /// <returns></returns>
        iCase iCaseOwner.GetCase(int aCaseIndex) => (iCase)GetCase(aCaseIndex);
        

        /// <summary>
        /// GetPropertyValue
        /// </summary>
        /// <param name="aPropertyNameorIndex"></param>
        /// <param name="bExists"></param>
        /// <returns></returns>
        dynamic iCaseOwner.GetPropertyValue(dynamic aPropertyNameorIndex, out bool bExists) => PropValGet(aPropertyNameorIndex, out bExists, bDecodedValue: false, aPartType: uppPartTypes.Undefined);

        /// <summary>
        /// PropValSet
        /// </summary>
        /// <param name="aPropertyNameorIndex"></param>
        /// <param name="aNewValue"></param>
        /// <returns></returns>
        bool iCaseOwner.PropValSet(dynamic aPropertyNameorIndex, dynamic aNewValue) =>PropValSet(aPropertyNameorIndex, aNewValue) != null;

        public bool ReNameCase(string aCurrentName, string aNewName)
        {
            if (Cases.Count <= 0 || string.IsNullOrWhiteSpace(aCurrentName) || string.IsNullOrWhiteSpace(aNewName)) return false;
            aNewName = aNewName.Trim();
            bool _rVal = false;
            int idx = Cases.FindIndex(x => string.Compare(aCurrentName, x.Description, true) == 0);
            if (idx <= 0) return false;
            uopPart acase = Cases.Item(idx);
            _rVal = string.Compare(acase.Description, aNewName, false) != 0;
            acase.Description = aNewName;
            return _rVal;

        }

        #endregion


    }
}
