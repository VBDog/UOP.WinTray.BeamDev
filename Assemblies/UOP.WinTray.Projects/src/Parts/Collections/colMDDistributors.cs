using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Interfaces;

using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;

namespace UOP.WinTray.Projects.Parts
{
    /// <summary>
    /// collection of mdDistributor objects
    /// </summary>
    public class colMDDistributors : uopCaseOwners, IEnumerable<mdDistributor>
    {

        public override uppPartTypes BasePartType => uppPartTypes.Distributors;

        public delegate void DistributorMemberChangedHandler(uopProperty aProperty);
        public event DistributorMemberChangedHandler eventDistributorMemberChanged;

        public delegate void DistributorCountChangeHandler(uopProperty aProperty);
        public event DistributorCountChangeHandler eventDistributorCountChange;


        #region Constructors

        public colMDDistributors() : base(uppCaseOwnerOwnerTypes.Distributor) { }

        internal colMDDistributors(colMDDistributors aPartToCopy, uopPart aParent = null, bool bDontCopyMembers = false) : base(uppCaseOwnerOwnerTypes.Distributor,aPartToCopy, aParent, bDontCopyMembers)
        {
            CaseCount = 1;
             if (bDontCopyMembers) { Quantity = 0; return; }
            CaseCount = aPartToCopy.CaseCount;
        
        }


        #endregion Constructors

        #region IEnumerable Implementation

        public new IEnumerator<mdDistributor> GetEnumerator() { foreach (uopPart part in Members) { yield return (mdDistributor)part; } }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion IEnumerable Implementation


        /// <summary>
        /// used to add an item to the collection
        /// won't add "Nothing" (no error raised)
        /// </summary>
        /// <param name="aDistributor">the item to add to the collection</param>
        /// <param name="bAddClone"></param>
        /// <returns></returns>
        public mdDistributor Add(mdDistributor aDistributor, bool bAddClone = false) 
        {
            if (aDistributor == null) return null;
            EnforceCaseCount(aDistributor);
            mdDistributor _rVal = (mdDistributor)base.Add(aDistributor, bAddClone);

            if (_rVal == null) return null;
            
            if (!SuppressEvents)
            {
                eventDistributorCountChange?.Invoke(new uopProperty(TPROPERTY.Quick("Count", Count, Count - 1, this)));
            }
            return _rVal;
        }

      

     

        

        public void Clear() => base.Clear();
        
        /// <summary>
        ///returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>
        public colMDDistributors Clone() => new colMDDistributors(this);

        public override uopPart Clone(bool aFlag = false) => (uopPart)this.Clone();

        public new List<mdDistributor> ToList(bool bGetClones = false)
        {

            List<mdDistributor> _rVal = new List<mdDistributor>();
           
            foreach (var part in CollectionObj)
            {
                if (!bGetClones)
                    _rVal.Add((mdDistributor)part);
                else
                    _rVal.Add((mdDistributor)part.Clone());
            }
            return _rVal;
        }
        public List<iCaseOwner> ToOwnerList(bool bGetClones = false)
        {

            List<iCaseOwner> _rVal = new List<iCaseOwner>();

            foreach (var part in CollectionObj)
            {
                if (!bGetClones)
                    _rVal.Add((iCaseOwner)part);
                else
                    _rVal.Add((iCaseOwner)part.Clone());
            }
            return _rVal;
        }
        /// <summary>
        ///returns the number of items in the collection
        /// </summary>
        /// <param name="bPhysicalOnly"></param>
        /// <returns></returns>
        public int GetCount(bool bPhysicalOnly = false)
        {
            int _rVal = 0;
            if (!bPhysicalOnly) return base.Count;
            
             mdDistributor aMem;
                for (int i = 1; i <= base.Count; i++)
                {
                    aMem = Item(i);
                    if (aMem.DistributorRequired) _rVal += 1;
                   
                }
            return _rVal;
        }

        /// <summary>
        ///returns the the ring numbers of the trays below the memmers when DIstributorRequired is true
        /// </summary>
        /// <param name="bPhysicalOnly"></param>
        /// <returns></returns>
        public List<int> TraysBelow()
        {
            List<int> _rVal = new List<int>();
         

       
            for (int i = 1; i <= base.Count; i++)
            {
                mdDistributor aMem = Item(i);
                if (aMem.DistributorRequired) _rVal.Add(mzUtils.VarToInteger(aMem.TrayBelow));

            }
            return _rVal;
        }

        public override iCaseOwner AddOwner(iCaseOwner aOwner, bool bAddClone = false)
        {
            return (aOwner != null && aOwner.OwnerType == uppCaseOwnerOwnerTypes.Distributor) ? Add((mdDistributor)aOwner, bAddClone) : null;
        }

        public override uppCaseOwnerOwnerTypes OwnerType => uppCaseOwnerOwnerTypes.Distributor;

       

      

        public override string INIPath => "DISTRIBUTORS";
        /// <summary>
        /// returns true if the passed collection is equal to this one
        /// each member must be equal by position in collection
        /// </summary>
        /// <param name="aDistribs"></param>
        /// <returns></returns>
        public bool IsEqual(colMDDistributors aDistribs)
        {
            
            try
            {
                if (aDistribs == null) return false;
                
                if (aDistribs.Count != Count) return false;
                for (int i = 1; i <= Count; i++)
                {
                    if (!Item(i).IsEqual(aDistribs.Item(i))) return false;
                }
                return true;
            }
            catch (Exception exception)
            {
                LoggerManager log = new LoggerManager();
                log.LogError(exception.Message);
                return false;
            }
        }

      

        /// <summary>
        ///returns the item from the collection at the requested index
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
        public new mdDistributor Item(int aIndex) => (aIndex > 0)? (mdDistributor)base.Item(aIndex): null;
        
         /// <summary>
        /// returns only the liquid distributors from the collection
        /// </summary>
        public List<mdDistributor> LiquidDistributors()
        {
            List<mdDistributor> _rVal = new List<mdDistributor>();
            mdDistributor aMem ;
            mdDistributorCase aCase = null;
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                if (aMem.DistributorRequired)
                {
                    aCase = (mdDistributorCase)aMem.Cases.Item(1);
                    if (string.Compare( aCase.FeedType,"LIQUID" ,ignoreCase:true)==0 )  _rVal.Add(aMem);
                      
                }
            }
            return _rVal;
        }

    /// <summary>
        /// returns only the mixed phase distributors from the collection
        /// </summary>
        public List<mdDistributor> MixedPhaseDistributors()
        {
            List<mdDistributor> _rVal = new List<mdDistributor>();
            mdDistributor aMem;
            mdDistributorCase aCase;
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                if (aMem.DistributorRequired)
                {
                    aCase = (mdDistributorCase)aMem.Cases.Item(1);
                    if (string.Compare(aCase.FeedType, "MIXED", ignoreCase: true) == 0)  _rVal.Add(aMem);
                        
                }
            }
            return _rVal;
            
        }

        public Collection<mdDistributor> ToCollection
        {
            get
            {
                Collection<mdDistributor> _rVal = new Collection<mdDistributor>();
                for (int i = 1; i <= Count; i++) { _rVal.Add(Item(i)); }
                return _rVal;
            }
        }


        /// <summary>
        /// used by members of the collection to inform the collection of change
        /// also raises the changed property to the object above it it the object model.
        /// </summary>
        /// <param name="Member"></param>
        /// <param name="aProperty"></param>
        public void NotifyMemberChange(mdDistributor Member, uopProperty aProperty)
        {
            //If Member Is Nothing Then Exit Sub
            if (aProperty == null) return;
            if (!SuppressEvents) eventDistributorMemberChanged?.Invoke(aProperty);
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
                CollectionObj.Clear();
                if (aProject != null) SubPart(aProject);
                Reading = true;


                uopPropertyArray myprops = aFileProps.PropertiesStartingWith(aFileSection);
                if (myprops == null || myprops.Count <= 0)
                {
                    ioWarnings?.AddWarning(this, $"{PartName} Data Not Found", $"File '{ aFileProps.Name }' Does Not Contain {aFileSection} Info!");
                    return;
                }

   
                for (int i = 1; i <= 10000; i++)
                {
                    string FSec = $"{aFileSection}({i})";

                    uopPropertyArray memprops = myprops.PropertiesStartingWith(FSec);
                    if (memprops.Count <= 0) break;

                    mdDistributor newmen = new mdDistributor
                    {
                        Index = i
                    };
                    newmen.ReadProperties(aProject, memprops, ref ioWarnings, aFileVersion, FSec, bIgnoreNotFound);
                    newmen.SubPart(this);
                    CollectionObj.Add(newmen);
             
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
                CaseCount = MaxCaseCount;
                Reading = false;
                EnforceCaseCount();
                EnforceCaseDescriptions();
            }
        }

        /// <summary>
        /// removes the item from the collection at the requested index
        /// </summary>
        /// <param name="Index"></param>
        public new mdDistributor Remove(int aIndex)
        {

            mdDistributor _rVal = (mdDistributor)base.Remove(aIndex);
            if (_rVal == null) return null;
            if (Count == 0) CaseCount = 0;
            if (!SuppressEvents) eventDistributorCountChange?.Invoke(uopProperty.Quick("Count", Count, Count + 1, this));
            return _rVal;
           
        }

        /// <summary>
        ///returns the properties required to save the distributors to file
        /// each memeer provides its own save properties
        /// </summary>
        public override uopPropertyArray SaveProperties(string aHeading = null)
        {
            uopPropertyArray _rVal = new uopPropertyArray();
            mdDistributor aMem;
            colUOPParts memCases;
            mdDistributorCase aCase;
            uopProperties subProps;
            aHeading = string.IsNullOrWhiteSpace(aHeading) ? INIPath : aHeading;
            string hdr = string.Empty;

            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                hdr = $"{aHeading}({i})";
                subProps = aMem.SaveProperties(hdr).Item(1);
                memCases = aMem.Cases;
                subProps.Add("CaseCount", memCases.Count, aHeading: hdr);

                _rVal.Add(subProps);
                for (int j = 1; j <= memCases.Count; j++)
                {
                    aCase = (mdDistributorCase)memCases.Item(j);
                    subProps = aCase.SaveProperties( $"{hdr}.CASE({j})").Item(1);
                    _rVal.Add(subProps);
                }
            }
            return _rVal;
        }
 

        public new mdDistributor SelectedMember { get => (mdDistributor)base.SelectedMember; }

        /// <summary>
        /// returns only the vapor distributors from the collection
        /// </summary>
        public List<mdDistributor> VaporDistributors()
        {
            List<mdDistributor> _rVal = new List<mdDistributor>();
            mdDistributor aMem;
            mdDistributorCase aCase = null;
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                if (aMem.DistributorRequired)
                {
                    aCase = (mdDistributorCase)aMem.Cases.Item(1);
                    if (string.Compare( aCase.FeedType , "VAPOR",ignoreCase:true)==0  )  _rVal.Add(aMem);
                
                }
            }
            return _rVal;
            
        }

      
    }
}
