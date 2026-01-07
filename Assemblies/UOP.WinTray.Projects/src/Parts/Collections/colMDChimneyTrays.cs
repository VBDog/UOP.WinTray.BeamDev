using System;
using System.Collections.Generic;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Interfaces;


namespace UOP.WinTray.Projects.Parts
{
    /// <summary>
    /// a collection of mdChimneyTray objects
    /// </summary>
    public class colMDChimneyTrays : uopCaseOwners, IEnumerable<mdChimneyTray>
    {

        public override uppPartTypes BasePartType => uppPartTypes.ChimneyTrays;

        
        private int _SelectedCase;
        public delegate void ChimneyTrayMemberChangedHandler(uopProperty aProperty);
        public event ChimneyTrayMemberChangedHandler eventChimneyTrayMemberChanged;

        public delegate void ChimneyTrayCountChangeHandler(uopProperty aProperty);
        public event ChimneyTrayCountChangeHandler eventChimneyTrayCountChange;

        public colMDChimneyTrays() : base(uppCaseOwnerOwnerTypes.ChimneyTray) { }


        internal colMDChimneyTrays(colMDChimneyTrays aPartToCopy, uopPart aParent = null,  bool bDontCopyMembers = false) : base(uppCaseOwnerOwnerTypes.ChimneyTray, aPartToCopy, aParent, bDontCopyMembers)
        {
 
            if (bDontCopyMembers) { Quantity = 0; return; }
            CaseCount = aPartToCopy.CaseCount;
       
            _SelectedCase = aPartToCopy._SelectedCase;

        }

        public colMDChimneyTrays Clone() => new colMDChimneyTrays(this);

        public override uopPart Clone(bool aFlag = false) => (uopPart)this.Clone();

        /// <summary>
        /// used to add an item to the collection
        /// won't add "Nothing" (no error raised).
        /// </summary>
        /// <param name="aChimneyTray">the item to add to the collection</param>
        /// <param name="bAddClone"></param>
        /// <returns></returns>
        public mdChimneyTray Add(mdChimneyTray aChimneyTray, bool bAddClone = false)
        {
            if (aChimneyTray == null) return null;
            EnforceCaseCount(aChimneyTray, 0);
            mdChimneyTray _rVal = (mdChimneyTray)base.Add(aChimneyTray,bAddClone);
          
                if (!SuppressEvents)
                {
                    eventChimneyTrayCountChange?.Invoke(uopProperty.Quick("Count", Count, Count - 1, this));
                }
           
            return _rVal;
        }

        public override iCaseOwner AddOwner(iCaseOwner aOwner, bool bAddClone = false) 
        {
            return (aOwner != null && aOwner.OwnerType == uppCaseOwnerOwnerTypes.ChimneyTray) ? Add((mdChimneyTray)aOwner, bAddClone) : null;
        }


        public List<mdChimneyTray> ToCollection
        {
            get
            {
                List<mdChimneyTray> _rVal = new List<mdChimneyTray>();
                for (int i = 1; i <= Count; i++){  _rVal.Add(Item(i)); }
                return _rVal;
            }
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


        #region IEnumerable Implementation

        public new IEnumerator<mdChimneyTray> GetEnumerator() { foreach (uopPart part in Members) { yield return (mdChimneyTray)part; } }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion IEnumerable Implementation



        public override uppCaseOwnerOwnerTypes OwnerType => uppCaseOwnerOwnerTypes.ChimneyTray;

  

        public override string INIPath => "CHIMNEYTRAYS";
        /// <summary>
        /// returns true if the passed collection is equal to this one
        /// each member must be equal by position in collection
        /// </summary>
        /// <param name="aTrayCol"></param>
        /// <returns></returns>
        public bool IsEqual(colMDChimneyTrays aTrayCol)
        {
            bool isEqual = false;
            try
            {
                if (aTrayCol == null)
                {
                    return isEqual;
                }
                if (aTrayCol.Count != Count)
                {
                    return isEqual;
                }
                isEqual = true;
                for (int i = 1; i <= Count; i++)
                {
                    if (!Item(i).IsEqual(aTrayCol.Item(i)))
                    {
                        isEqual = false;
                        break;
                    }
                }
                return isEqual;
            }
            catch (Exception)
            {

               
                return false;
            }
        }
        /// <summary>
        ///returns the item from the collection at the requested index ! Base 1 !
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
        public new mdChimneyTray Item(int aIndex) => (mdChimneyTray)base.Item(aIndex);

        /// <summary>
        /// used by members of the collection to inform the collection of change
        /// also raises the changed property to the object above it it the object model.
        /// </summary>
        /// <param name="As"></param>
        /// <param name=""></param>
        /// <param name="As"></param>
        /// <param name=""></param>
        public void NotifyMemberChange(mdChimneyTray Member, uopProperty aProperty)
        {
            if (Member == null) return;
            if (aProperty == null) return;
            if (!SuppressEvents) eventChimneyTrayMemberChanged?.Invoke(aProperty);
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

                    mdChimneyTray newmen = new mdChimneyTray
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
        public new mdChimneyTray Remove(int aIndex)
        {

            if (Index < 0 || aIndex >= Count) return null;
            mdChimneyTray _rVal = (mdChimneyTray)base.Remove(aIndex);
            if (!SuppressEvents)
            {
                eventChimneyTrayCountChange?.Invoke(uopProperty.Quick("Count", Count, Count + 1, this));
            }

            return _rVal;
        }

        /// <summary>
        ///returns the properties required to save the trays to file
        /// each memeer provides its own save properties
        /// </summary>
        public override uopPropertyArray SaveProperties(string aHeading = null)
        {
            List<uopProperties> _rVal = new List<uopProperties>();
            mdChimneyTray aMem;
            colUOPParts memCases;
            mdChimneyTrayCase aCase;
            uopProperties subProps;
            aHeading = string.IsNullOrWhiteSpace(aHeading) ? INIPath : aHeading;
            string hdr = string.Empty;

            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                hdr = aHeading + "(" + i.ToString() + ")";
                subProps = aMem.SaveProperties(hdr).Item(1);
                memCases = aMem.Cases;
                subProps.Add("CaseCount", memCases.Count, aHeading: hdr);

                _rVal.Add(subProps);
                for (int j = 1; j <= memCases.Count; j++)
                {
                    aCase = (mdChimneyTrayCase)memCases.Item(j);
                    subProps = aCase.SaveProperties(hdr + ".CASE(" + j.ToString() + ")").Item(1);
                    _rVal.Add(subProps);
                }
            }
            return new uopPropertyArray(_rVal) { Name = aHeading };
        }

     
        public int SelectedCase { get => _SelectedCase; set => _SelectedCase = value; }
        public new mdChimneyTray SelectedMember => (mdChimneyTray)base.SelectedMember; 



    }
}
