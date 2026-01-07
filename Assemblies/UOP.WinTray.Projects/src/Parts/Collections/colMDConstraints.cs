using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;

namespace UOP.WinTray.Projects.Parts
{
    /// <summary>
    /// a collection of mdConstraint object
    /// </summary>
    public class colMDConstraints :  uopParts, IEnumerable<mdConstraint>
    {

        public override uppPartTypes BasePartType => uppPartTypes.Constraints;

        public delegate void ConstraintMemberChangedHandler(uopProperty aProperty);
        public event ConstraintMemberChangedHandler eventConstraintMemberChanged;
        public delegate void ConstraintsInvalidatedHandler();
        public event ConstraintsInvalidatedHandler eventConstraintsInvalidated;

        #region Constructors

        public colMDConstraints() : base(uppPartTypes.Constraints,uppProjectFamilies.uopFamMD,  bBaseOne: true, bMaintainIndices: true) { }


        internal colMDConstraints(colMDConstraints aPartToCopy, mdTrayAssembly aParent = null, bool bDontCopyMembers = false) : base(aPartToCopy, bDontCopyMembers, aParent) 
        { 
            if(aParent != null && !bDontCopyMembers)
            {
                int iNd = aParent.Downcomer().Count;
                if(iNd > 0)
                {
                    int iNp = iNd + 1;
                    RemoveAll((x) => x.DowncomerIndex > iNd || x.PanelIndex > iNp);

                    for (int did = 1; did < iNd; did++)
                    {

                        for (int pid = 1; pid < iNp; pid++) 
                        {
                            mdConstraint aConstraint = (mdConstraint)this.Find((x) => x.PanelIndex == pid && x.DowncomerIndex == did);
                            if(aConstraint== null)
                            {
                                aConstraint = new mdConstraint() { PanelIndex = pid, DowncomerIndex = did };
                                Add(aConstraint);
                            }
                        }
                    }
                }
            }
        }

        #endregion Constructors

        #region IEnumerable Implementation

        public new IEnumerator<mdConstraint> GetEnumerator() { foreach (uopPart part in Members) { yield return (mdConstraint)part; } }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion IEnumerable Implementation




        /// <summary>
        /// used to add an item to the collection
        /// </summary>
        /// <param name="aConstraint">item to add to the collection</param>
        /// <param name="bAddClone"></param>
        /// <returns></returns>
        public mdConstraint Add(mdConstraint aConstraint, bool bAddClone = false)
        {
            if (aConstraint == null) return null;
            mdConstraint _rVal = (mdConstraint)base.Add(aConstraint, bAddClone);
            return _rVal;
            
        }
        /// <summary>
        /// used by objects above this object in the object model to alert a sub object that a property above it in the object model has changed.
        ///alerts the objects below it of the change
        /// </summary>
        /// <param name="aProperty"></param>
        void Alert(uopProperty aProperty)
        {
            if (aProperty == null)
            {
                return;
            }
            eventConstraintMemberChanged?.Invoke(aProperty);
        }


      
        public void ClearIDeals()
        {
            mdConstraint aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                aMem.TreatAsIdeal = false;
            }
        }
        /// <summary>
        ///returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>
        public colMDConstraints Clone(dynamic bReturnEmpty = null ) => new colMDConstraints(this,bDontCopyMembers:mzUtils.VarToBoolean( bReturnEmpty));

        public override uopPart Clone(bool aFlag = false) => (uopPart)this.Clone();


        /// <summary>
        /// applies the values from the passed constraint to the members of the collection
        /// </summary>
        /// <param name="aConstraint">the constraint to copy the values from</param>
        /// <param name="ConstraintCol">a collection of constraints to apply changes to other that the current collection</param>
        public void CopyValues(mdConstraint aConstraint, colMDConstraints ConstraintCol = null)
        {
            if (aConstraint == null) return;
            mdConstraint aMem;
            string cDef = aConstraint.Descriptor;
            colMDConstraints Cnsts= ConstraintCol ?? this;
            for (int i = 1; i <= Cnsts.Count; i++)
            {
                aMem = Cnsts.Item(i);
                aMem.DefineByString(cDef, false);
            }
        }

        public void DefineByString(string Descripts)
        {
            try
            {
                mdConstraint aMem;
                string[] vals = Descripts.Split(uopGlobals.Delim.ToCharArray());

                for (int i = 0; i < vals.Count(); i++)
                {
                    aMem = Item(i + 1);
                    if (aMem != null)
                    {
                        aMem.DefineByString(vals[i]);
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        /// <summary>
        /// a concantonated string of the descriptor strings of all the members of the collection
        /// </summary>
        public string Descriptors
        {
            get
            {
                string _rVal = string.Empty;
                mdConstraint aMem;
                //~see DefineByString
                for (int i = 1; i <= Count; i++)
                {
                    aMem = Item(i);
                    _rVal += uopGlobals.Delim + aMem.Descriptor;
                }
                return _rVal;
            }
        }
       

       
        /// <summary>
        ///returns the constraint from the collection whose downcomer index matches the passed value
        /// </summary>
        /// <param name="DowncomerIndex">the downcomer index to search for</param>
        /// <returns></returns>
        public List<mdConstraint> GetByDowncomerIndex(int DowncomerIndex, int aBoxIndex= 1)  => FindAll((x) => x.DowncomerIndex == DowncomerIndex).OfType<mdConstraint>().ToList();

        /// <summary>
        ///returns the constraint from the collection whose handle matches the passed value
        /// </summary>
        /// <param name="aHandle">handle to search for</param>
        /// <returns></returns>
        public mdConstraint GetByHandle(string aHandle) => ToList().Find((x) => string.Compare(x.Handle, aHandle, true) == 0);
        

        /// <summary>
        ///returns the constraint from the collection whose panel index matches the passed value
        /// </summary>
        /// <param name="PanelIndex">the deck panel index to search for</param>
        /// <returns></returns>
        public List<mdConstraint> GetByPanelIndex(int PanelIndex) => FindAll((x) => x.PanIndex == PanelIndex).OfType<mdConstraint>().ToList();
     


        /// <summary>
        ///returns the properties from all the constraints in the collection that are equal for each member

        /// </summary>
        /// <returns></returns>
        public uopProperties GetCommonProperties(int? downcomerIndex, int? panelIndex, int? excludePanelIndex)
        {
            uopProperties _rVal = new uopProperties();
            if (Count <= 0) return _rVal;
            
            mdConstraint aMem;
            uopProperties aBase = new uopProperties();
            uopProperties mProps;
            
            uopProperty mProp;
            List<mdConstraint> vMems = VisibleMembers(downcomerIndex, panelIndex, excludePanelIndex);
            if (vMems.Count <= 0) return _rVal; 

            aMem = vMems[0];
            aBase = new uopProperties(aMem.ActiveProps);
            aBase.Remove( "TreatAsIdeal");
            aBase.Remove("TreatAsGroup");
            aBase.Remove("OverrideSpoutArea");

            List<uopProperty> props = aMem.ActiveProps.ToPropertyList;
           

            if (vMems.Count == 1) return aBase;
            foreach (var item in props)
            {
                for (int j = 1; j < vMems.Count; j++)
                {
                    aMem = vMems[j];
                    mProps = new uopProperties(aMem.ActiveProps);
                    if (mProps.TryGet(item.Name, out mProp))
                    {

                        if (!item.IsEqual(mProp)) aBase.Remove(item.Name);
                    }
                }
            }

            _rVal = aBase;
            return _rVal;
            
          
        }

        /// <summary>
        ///returns the constraint from the collection whose panel and downcomer indices match the passed values
        /// </summary>
        /// <param name="aPanelIndex">the panel index to search for</param>
        /// <param name="aDowncomerIndex">the downcomer index to search for</param>
        /// <returns></returns>
        public mdConstraint GetConstraint(int aPanelIndex, int aDowncomerIndex, int aBoxIndex  )
        {
            for (int i = 1; i <= Count; i++)
            {
                mdConstraint aMem = Item(i);
                if (aMem.PanelIndex == aPanelIndex && aMem.DowncomerIndex == aDowncomerIndex && aMem.BoxIndex == aBoxIndex) return aMem;
            }
            return null;
        }
        /// <summary>
        /// returns onlt the constraints that apply to end spout groups
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public List<mdConstraint> GetEndGroupConstraints(mdTrayAssembly aAssy)
        {
            List<mdConstraint> _rVal = new List<mdConstraint>();
           aAssy ??= GetMDTrayAssembly();
            for (int i = 1; i <= Count; i++)
            {
                mdConstraint aMem = Item(i);
                mdSpoutGroup sGroup = aMem.SpoutGroup(aAssy);
                if (sGroup != null)
                {
                    if (sGroup.LimitedBounds ) _rVal.Add(aMem);
                    
                }
            }
            return _rVal;
        }

        /// <summary>
        /// returns onlt the constraints that apply to spout groups that are not considered end groups
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public List<mdConstraint> GetFieldGroupConstraints(mdTrayAssembly aAssy)
        {
         

            List<mdConstraint> _rVal = new List<mdConstraint>();
           aAssy ??= GetMDTrayAssembly();
            

            for (int i = 1; i <= Count; i++)
            {
                mdConstraint aMem = Item(i);
                mdSpoutGroup sGroup = aMem.SpoutGroup(aAssy);
                if (sGroup != null)
                {
                    if (!sGroup.LimitedBounds) _rVal.Add(aMem);
                }

            }
            return _rVal;
        }
        /// <summary>
        /// returns onlt the constraints that have the TreatAsGroup Flag = True
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public List<mdConstraint> GetGroupedConstraints(mdTrayAssembly aAssy)
        {
            List<mdConstraint> _rVal = new List<mdConstraint>();
            aAssy ??= GetMDTrayAssembly();
          
            for (int i = 1; i <= Count; i++)
            {
                mdConstraint  aMem = Item(i);
                if (aMem.TreatAsGroup)  _rVal.Add(aMem);
            }
            return _rVal;
        }

     
        /// <summary>
        /// returns True of any of the constraints has the TreatAsGroup Flag = True
        /// </summary>
        public bool HasGroupFlags
        {
            get
            {
                mdConstraint aMem;
              
                for (int i = 1; i <= Count; i++)
                {
                     aMem = Item(i);
                    if (aMem.TreatAsGroup) return true;
                }
                return false;
            }
        }
     
        public bool HasIdealFlags
        {
            get { 

                mdConstraint aMem;
                for (int i = 1; i <= Count; i++)
                {
                     aMem = Item(i);
                    if (aMem.TreatAsIdeal) return true;
                    
                }

                return false;
            }
        }
        public override string INIPath => $"COLUMN({ ColumnIndex }).RANGE({ RangeIndex }).TRAYASSEMBLY.CONSTRAINTS";
        /// <summary>
        /// flag indicating that something has changed and the collection needs to be regenerated
        /// </summary>
        public override bool Invalid
        {
            get => Count <= 0 || base.Invalid;
               set
            {
                if (base.Invalid == value) return;
                base.Invalid = value;
                if (value) eventConstraintsInvalidated?.Invoke();
            }

        }

        public override bool IsEqual(uopPart aPart)
        {
            if (aPart == null) return false;
            return aPart.PartType == PartType && this.IsEqual((colMDConstraints)aPart) ;
        }

        public bool IsEqual(colMDConstraints aCons)
        {
            if (aCons == null) return false;
            if (aCons.Count != Count) return false;
            mdConstraint aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                if (!aMem.IsEqual(aCons.Item(i))) return false;
            }
            return true;
        }

        public new List<mdConstraint> ToList(bool bGetClones = false)
        {

            List<mdConstraint> _rVal = new List<mdConstraint>();
           
            foreach (var part in CollectionObj)
            {
                if (!bGetClones)
                    _rVal.Add((mdConstraint)part);
                else
                    _rVal.Add((mdConstraint)part.Clone());
            }
            return _rVal;
        }

        /// <summary>
        ///returns the item from the collection at the requested index ! Base 1 !
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
        public new mdConstraint Item(int aIndex) => (mdConstraint)base.Item(aIndex);

        /// <summary>
        /// returns only the properties from the members of the collection
    //that will cause the parent spout groups properties to be other than default
        /// </summary>
        /// <param name="aUnitFamily"></param>
        /// <returns></returns>
        public uopProperties NonDefaultProperties(uppUnitFamilies aUnitFamily)
        {
            uopProperties nDProps;
            mdConstraint aMem;
            uopProperties _rVal = new uopProperties();
            for (int i = 1; i <= Count; i++)
            {
                 aMem = Item(i);
                nDProps = aMem.NonDefaultProperties;
                _rVal.Add("CONSTRAINT(" + aMem.Handle + ")", nDProps.Signatures(",", true, aUnitFamily));

            }
            return _rVal;
        }

        /// <summary>
        /// used by members of the collection to inform the collection of change
        ///also raises the changed property to the object above it it the object model.
        /// </summary>
        /// <param name="aMember"></param>
        /// <param name="aProperty"></param>
        public void NotifyMemberChange(mdConstraint aMember, uopProperty aProperty)
        {
            if (aMember == null || aProperty == null || SuppressEvents) return;
                eventConstraintMemberChanged?.Invoke(aProperty);
            
        }

        /// <summary>
        /// intializes the collection
        /// </summary>
        /// <param name="aTrayAssembly">tray assembly to match the constraints to</param>
        public void Populate(mdTrayAssembly aAssy)
        {
            
            if (aAssy == null) return;
            SubPart(aAssy);
            ProjectHandle = aAssy.ProjectHandle;
            RangeGUID = aAssy.RangeGUID;
            List<mdDowncomer> dcs = aAssy.Downcomers.GetByVirtual(false);

            int dcnt = dcs.Count;
            int pcnt = dcnt + 1;

           

            if (aAssy.IsStandardDesign && aAssy.OddDowncomers) pcnt -=1;
            this.Clear();

            for (int pidx = 1; pidx <= pcnt; pidx++)
            {
                for (int didx = 1; didx <= dcnt; didx++)
                {
                    
                    
                    List<mdDowncomerBox> boxes = dcs[didx -1].Boxes;
                    if(boxes.Count > 0) Add(new mdConstraint(null, boxes[0], pidx));
                  
                    
                    SubPart(aAssy);
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
        public override void  ReadProperties(uopProject aProject, uopPropertyArray aFileProps, ref uopDocuments ioWarnings, double aFileVersion, string aFileSection = null, bool bIgnoreNotFound = false, uopTrayAssembly aAssy = null, uopPart aPartParameter = null, List<string> aSkipList = null, Dictionary<string, string> EqualNames = null, uopProperties aProperties = null)
        {

            try
            {
                ioWarnings ??= new uopDocuments();
                aFileSection = string.IsNullOrWhiteSpace(aFileSection) ? INIPath : aFileSection.Trim();
                if (aFileProps == null) throw new Exception("The Passed File Property Array is Undefined");
                if (string.IsNullOrWhiteSpace(aFileSection)) throw new Exception("File Section is Undefined");
                if (aProject != null) SubPart(aProject);
                if (aAssy == null) throw new Exception("Tray Assembly is Undefined");

                mdTrayAssembly myassy = (mdTrayAssembly)aAssy;
                SubPart(myassy);

                uopProperties myprops = aFileProps.Item(aFileSection);
                if (myprops == null || myprops.Count <= 0)
                {
                    ioWarnings?.AddWarning(this, $"{PartName} Data Not Found", $"File '{ aFileProps.Name }' Does Not Contain {aFileSection} Info!");
                    return;
                }

                string searchFor = string.Empty;
                mdConstraint aMem;
                mdDowncomer aDC;

                uopProperties Props;
                string hndle = string.Empty;
                Reading = true;
                if (aFileVersion == 0) aFileVersion = uopUtils.ReadINI_Version(aFileProps.Name, "APPINFO", "Version", 0);


                if (aFileVersion == 1.5)
                {
                    for (int i = 1; i <= Count; i++)
                    {
                        searchFor = $"Constraint{i}";
                        string aStr = myprops.ValueS(searchFor);
                        if (!string.IsNullOrEmpty(aStr))
                        {
                            Props = new uopProperties();
                            Props.DefineByString(aStr);
                            hndle = Props.Value("handle");
                            aMem = GetByHandle(hndle);
                            if (aMem != null)
                            {
                                aDC = aMem.Downcomer(myassy);
                                aMem.PropValsCopy(Props, false);
                                if (Math.Round(aMem.Clearance, 5) == mdSpoutGroup.GetDefaultClearance( aDC)) aMem.PropValSet("Clearance", 0);

                            }
                        }
                        else
                        { break; }
                    }
                }
                else
                {
                    for (int i = 1; i <= Count; i++)
                    {
                        aMem = Item(i);
                        searchFor = $"Constraint({ aMem.Handle })";
                        //if(searchFor == "Constraint(2,4)")
                        //{
                        //    Console.WriteLine(searchFor);
                        //}

                        string dstr = myprops.ValueS(searchFor) ; 
                        if (!string.IsNullOrEmpty(dstr))
                        {
                            aDC = aMem.Downcomer(myassy);
                            if(string.Compare(dstr,"None", true) != 0)
                            {
                                aMem.Reading = true;

                                aMem.DefineValues(mzUtils.FixGlobalDelimError( dstr), false, aFileVersion);
                                aMem.Reading = false;

                            }
                        }
                        else
                        {
                            if (aFileVersion > 1.67)
                            {
                                ioWarnings?.AddWarning(aMem, "Missing Constraint Data", $"Data For Constraints Range({ RangeIndex }).Constraint({ aMem.Handle }) Not Found In File '{ aFileProps.Name }'");
                            }
                        }
                    }
                    Reading = false;
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
            }
        }

        /// <summary>
        /// removes the item from the collection at the requested index ! base 1 !
        /// </summary>
        /// <param name="Index"></param>
        public new mdConstraint Remove(int aIndex)
        {
            if (Index < 0 || aIndex >= Count) return null;
            mdConstraint _rVal = (mdConstraint)base.Remove(aIndex);
            return _rVal;
        }

        /// <summary>
        ///returns the properties required to save the constraints to file
        /// </summary>
        public override  uopPropertyArray SaveProperties(string aHeading = null)
        {
            
            mdConstraint aMem;
        
            uopProperties _rVal = new uopProperties();
            TPROPERTY sProp;

            aHeading = string.IsNullOrWhiteSpace(aHeading) ? INIPath : aHeading;
            for (int i = 1; i <= Count; i++)
            {
                 aMem = Item(i);
                TPROPERTIES sProps = new TPROPERTIES(aMem.SaveProperties().Item(1));
                string memprops = "None";
                if (sProps.Count > 0)
                {
                    for (int j = 1; j <= sProps.Count; j++)
                    {
                        sProp = sProps.Item(j);
                        if (sProp.IsNamed("PATTERNTYPE"))
                        {
                            //uppSpoutPatterns rslt = uopEnums.GetEnumValue<uppSpoutPatterns>(sProp.Value);
                            //sProp.Value = (int)rslt - 1;//As in VB undefined values starts from -1 butin c# starts from 0 hence there is mismatch in type selected and saved
                            //sProps.SetItem(j, sProp);
                        }

                    }
                    if (aMem.TreatAsGroup)
                       sProps.Add("TreatAsGroup", "True"); // for backward compatibility
                     memprops = sProps.DeliminatedString(uopGlobals.Delim, bIncludePropertyNames: true);
                }
                else
                {
                    
                }
                _rVal.Add($"Constraint({ aMem.Handle })", memprops, aHeading: aHeading);

            }
            return new uopPropertyArray(_rVal, aName: aHeading, aHeading: aHeading);

        }

        /// <summary>
        /// used to set the properties of the the members of collection to the values indicated in the passed string
        /// </summary>
        /// <param name="ConstraintDescriptor">a constraint descriptor string used to set the properties of all the members of the collection</param>
        /// <param name="SetFlags">flag to reset the ideal and group flags properties</param>
        public void SetConstraints(string ConstraintDescriptor, bool SetFlags = true)
        {
            try
            {
                mdConstraint aMem;
                for (int i = 1; i <= Count; i++)
                {
                     aMem = Item(i);
                    aMem.DefineByString(ConstraintDescriptor, SetFlags);
                }
            }
            catch (Exception e)
            {
                throw e;
            }


        }
        

       
        /// <summary>
        /// Filters and get all constraints values
        /// </summary>
        /// <param name="downcomerIndex"></param>
        /// <param name="panelIndex"></param>
        /// <returns></returns>
        public List<mdConstraint> VisibleMembers(int? downcomerIndex, int? panelIndex, int? excludePanelIndex = null)
        {
            List<mdConstraint> _rVal = ToList().FindAll(x => !x.IsVirtual);

            if (null != downcomerIndex) _rVal = _rVal?.FindAll(x => x.DowncomerIndex == downcomerIndex.Value);
            if (null != panelIndex) _rVal = _rVal?.FindAll(x => x.PanelIndex == panelIndex.Value);
            if (null != excludePanelIndex) _rVal = _rVal?.FindAll(x => x.PanelIndex != excludePanelIndex);
            return _rVal;
        }

        /// <summary>
        /// Compares two properties of constraints
        /// </summary>
        /// <param name="aProp"></param>
        /// <param name="bProp"></param>
        /// <returns></returns>
        private bool Compare(TPROPERTY aProp, TPROPERTY bProp)
        {
            if (!string.IsNullOrEmpty(aProp.Name))
            {
                if (aProp.Name.ToUpper() == "PATTERNTYPE")
                {
                    return uopEnums.GetEnumValue<uppSpoutPatterns>(aProp.Value) == uopEnums.GetEnumValue<uppSpoutPatterns>(bProp.Value);
                }
                else
                {
                    return EqualityComparer<double>.Default.Equals(aProp.Value, bProp.Value);
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks the existance of pattern type 'S1' or 'S2' or 'S3'
        /// </summary>
        /// <param name="downcomerIndex"></param>
        /// <param name="panelIndex"></param>
        /// <param name="excludePanelIndex"></param>
        /// <returns></returns>
        public bool IsPatternOfTypeSExist(int? downcomerIndex, int? panelIndex, int? excludePanelIndex = null)
        {
            List<mdConstraint> filteredList = VisibleMembers(downcomerIndex, panelIndex, excludePanelIndex);

            return null != filteredList && filteredList.Exists(x => x.PatternType == uppSpoutPatterns.S1 ||
                                                                       x.PatternType == uppSpoutPatterns.S2 ||
                                                                       x.PatternType == uppSpoutPatterns.S3);
        }
    }
}
