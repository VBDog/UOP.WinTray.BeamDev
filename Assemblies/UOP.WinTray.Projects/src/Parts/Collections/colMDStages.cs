using System;
using System.Collections.Generic;
using UOP.WinTray.Projects.Enums;

namespace UOP.WinTray.Projects.Parts
{
    /// <summary>
    /// a collection of mdstage objects
    /// </summary>
    public class colMDStages :  uopParts, IEnumerable<mdStage>
    {
        //raised when the range configuration data changes

        public override uppPartTypes BasePartType => uppPartTypes.Stages;

        #region constructor
        public colMDStages() : base(uppPartTypes.Stages,uppProjectFamilies.uopFamMD, bBaseOne: true, bMaintainIndices: true) { }
        internal colMDStages(colMDStages aPartToCopy ,bool bDontCopyMembers = false, uopPart aParent = null) : base(aPartToCopy, bDontCopyMembers, aParent) { base.MaintainIndices = true; }
        #endregion


        #region IEnumerable Implementation

        public new IEnumerator<mdStage> GetEnumerator() { foreach (uopPart part in Members) { yield return (mdStage)part; } }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion IEnumerable Implementation

        /// <summary>
        /// used to add an item to the collection
        /// won't add "Nothing" (no error raised).
        /// </summary>
        /// <param name="stage">the item to add to the collection</param>
        public void AddStage(mdStage stage, int stageIndex=-1)
        {
            if (stage == null)  return;
            if (stageIndex < 0)
            {
                stage.Index = Count + 1;
            }
            else
            {
                stage.Index = stageIndex;
            }
            Add(stage);
            RaiseCountChangeEvent(Count - 1, Count);
        }


        /// <summary>
        /// ^returns an new collection whose members are clones of the members of this collection
        /// </summary>
        /// <returns></returns>
        public colMDStages Clone() => new colMDStages(this);

        public override uopPart Clone(bool aFlag = false) => (uopParts)this.Clone();

        public new List<mdStage> ToList(bool bGetClones = false)
        {

            List<mdStage> _rVal = new List<mdStage>();
           
            foreach (var part in CollectionObj)
            {
                if (!bGetClones)
                    _rVal.Add((mdStage)part);
                else
                    _rVal.Add((mdStage)part.Clone());
            }
            return _rVal;
        }

        /// <summary>
        /// removes the stage from the collection that has a "StageNumber" property equal to the passed string
        /// </summary>
        /// <param name="StageNumber"></param>
        public void DeleteByStageNumber(int StageNumber)
        {
            mdStage Stage;
            for (int i = Count; i >=1; i--)
            {
                Stage = Item(i);
                if (Stage.StageNumber == StageNumber)
                {
                    Remove(i);
                    RaiseCountChangeEvent(Count + 1, Count);
                    break;
                }
            }
        }

        /// <summary>
        ///returns the member whose stage name matches the passed value
        /// </summary>
        /// <param name="aStageName"></param>
        /// <returns></returns>
        public mdStage GetByStageName(string aStageName)
       => (mdStage)base.GetByName(aStageName);

        /// <summary>
        ///returns the stage from the collection that has a "StageNumber" property equal to the passed string
        /// </summary>
        /// <param name="StageNumber"></param>
        /// <returns></returns>
        public mdStage GetByStageNumber(int StageNumber)
        {
           
            
            for (int i = 1; i <= Count; i++)
            {
                mdStage aMem = (mdStage)base.Item(i);
                if (aMem.StageNumber == StageNumber) return aMem;
                
            }
            return null;
        }

        public List<mdStage> GetUniqueStages(bool MechanicallyOnly)
        {
            mdStage aMem;
            mdStage bMem;
            bool keep = false;
        
            List<mdStage> _rVal = new List<mdStage>();
            for (int i = 1; i <= Count; i++)
            {
                aMem = (mdStage)base.Item(i);
                keep = true;
                for (int j = 1; j <= _rVal.Count; j++)
                {
                    bMem = _rVal[j -1];
                    if (aMem.IsEqual(bMem, MechanicallyOnly))
                    {
                        keep = false;
                        break;
                    }
                }
                if (keep)  _rVal.Add(aMem);
               
            }
            return _rVal;
        }

        /// <summary>
        ///returns the item from the collection at the requested index
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
        public new mdStage Item(int aIndex) => (mdStage)base.Item(aIndex);

        /// <summary>
        /// used by members of the collection to inform the collection of change
        /// also raises the changed property to the object above it it the object model.
        /// </summary>
        /// <param name="aMember"></param>
        /// <param name="aProperty"></param>
        public void NotifyMemberChange(mdStage aMember, uopProperty aProperty)
        {
            if (ProjectHandle ==  string.Empty || aMember == null || aProperty == null) { return; }
            NotifyProject(aProperty);
        }
     

        /// <summary>
        /// raised to notify listeners of a count change
        /// </summary>
        /// <param name="CountWas"></param>
        /// <param name="CountIs"></param>
        private void RaiseCountChangeEvent(int CountWas, int CountIs)
        {
            if (!string.IsNullOrEmpty(ProjectHandle))
            {
                NotifyProject( uopProperty.Quick("Count", CountIs, CountWas,this));
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
                CollectionObj.Clear();
                if (aProject != null) SubPart(aProject);
                Reading = true;
                aFileSection = "STAGES";

                uopPropertyArray myprops = aFileProps.PropertiesStartingWith(aFileSection);
                if ((myprops == null || myprops.Count <= 0) && aFileVersion < 4) 
                {
                    aFileSection = "COLUMN(1).STAGES";
                    myprops = aFileProps.PropertiesStartingWith(aFileSection);
                }
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

                    mdStage newmen = new mdStage
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
              
            }
        }
        /// <summary>
        /// removes the item from the collection at the requested index
        /// </summary>
        /// <param name="Index"></param>
        public override uopPart Remove(int aIndex)
        {

            if (aIndex < 1 || aIndex > Count) return null;
            uopPart _rVal = base.Remove(aIndex);
            RaiseCountChangeEvent(Count + 1, Count);
            return _rVal;
}

        /// <summary>
        ///the properties that are saved to file 
        /// </summary>
        public override uopPropertyArray SaveProperties(string aHeading = null)
        {
            aHeading = string.IsNullOrWhiteSpace(aHeading) ? INIPath : aHeading.Trim();

            uopPropertyArray _rVal = new uopPropertyArray() ;
            uopProperties subprops= base.CurrentProperties();

            if(subprops != null && subprops.Count > 0)
            {
                subprops.SetHeadings(aHeading);
                _rVal.Add(subprops);

            }
            mdStage aMem;
           
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                subprops = aMem.SaveProperties(aHeading + "(" + i.ToString() + ")").Item(1);
                if (subprops != null && subprops.Count > 0) _rVal.Add(subprops);
            }

            return _rVal;

        }

        public override string INIPath => "STAGES";

        public void Terminate()
        {
            this.Clear();
            Quantity = Count;
        }
    }

}


