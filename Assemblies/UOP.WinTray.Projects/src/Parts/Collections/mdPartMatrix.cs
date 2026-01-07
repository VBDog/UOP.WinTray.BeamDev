using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Events;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Parts;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;

namespace UOP.WinTray.Projects
{
    public  class mdPartMatrix : List<mdPartArray>, IEnumerable<mdPartArray>
    {
        public delegate void PartGenerationHandler(string aStatusString, bool? bBegin = null);
        public event PartGenerationHandler eventPartGeneration;

        #region Constructors
        public mdPartMatrix(mdProject aProject) { Init(aProject); }


        internal void Init(mdProject aProject) 
        {
            _Updating = false;
            base.Clear();
            _ProjectRef = null;
            if (aProject == null) return;
            _ProjectRef = new WeakReference<mdProject> ( aProject );
            if (aProject.ProjectType == Enums.uppProjectTypes.MDDraw)
            {
                foreach (var range in aProject.TrayRanges)
                {
                    mdTrayRange mdrange = (mdTrayRange)range;
                    Add(new mdPartArray(mdrange));
                }
            }
        }
        #endregion Constructors

        #region Properties

        private WeakReference<mdProject> _ProjectRef;
        public mdProject Project
        {
            get
            {
                if (_ProjectRef == null) return null;
                if (!_ProjectRef.TryGetTarget(out mdProject _rVal)) Init(null);
                return _rVal;

            }
        }



        #endregion Properties
        #region Methods

        internal void GenerationStatus(string aStatusString, bool bBegin)
        {
            eventPartGeneration?.Invoke(aStatusString, bBegin);
        }

        public override string ToString()
        {
            return base.ToString();
        }

        private bool _Updating;
        public bool Update(mdProject aProject,  bool bUpdateAll = false, List<uppPartTypes> aRegenList = null) 
        {
            bool _rVal = false;
            aProject ??= Project;
            if (aProject == null) return false;
            string jreadwuz = aProject.ReadStatusString;
            string jpartwuz = aProject.PartGenStatusString;

            try
            {
                if (_Updating) return false;
                _Updating = true;   
                List<mdTrayRange> myranges = Ranges();
                colUOPTrayRanges pranges = aProject.TrayRanges;
       
           
                foreach (uopTrayRange item in pranges)
                {
                    mdTrayRange prange = (mdTrayRange)item;
                    mdPartArray array = Find((x) => string.Compare(x.RangeGUID, prange.GUID, true) == 0);
                    if (array == null)
                    {
                        array = new mdPartArray(prange);
                        Add(array);
                        _rVal = true;
                    }
                    if (array.UpdateParts(prange, bUpdateAll, aRegenList, aProject, this)) _rVal = true;

                }


            }
            catch { }
            finally 
            { 
                _Updating = false;
                if (!string.IsNullOrWhiteSpace(jreadwuz))
                    aProject.ReadStatus(jreadwuz);
                aProject.PartGenenerationStatus(!string.IsNullOrWhiteSpace(jpartwuz) ? jpartwuz : "");
            }
          

            return _rVal;
        }

        private List<mdTrayRange> Ranges()
        {
            List<mdTrayRange> _rVal = new List<mdTrayRange>();
            
            List<mdPartArray> removers = new List<mdPartArray>();
            foreach (mdPartArray rangeArray in this)
            {
                mdTrayRange range = rangeArray.Range;
                if (range == null) {  removers.Add(rangeArray); continue; }
                _rVal.Add(range);
            }
          
            foreach (mdPartArray rangeArray in removers) { Remove(rangeArray); }
                return _rVal;
        }

        /// <summary>
        /// returns the unique end angles in the parent project. 
        /// </summary>
        /// <remarks>one of each part number with the quantity set to the quantity required for the entire column without spares.</remarks>
        /// <returns></returns>
        public uopPartList<mdEndAngle> EndAngles(string aRangeGUID = null, string aDowncomerPN = null)
        {

            mdProject proj = Project;
            if (proj == null) return new uopPartList<mdEndAngle>(new List<mdEndAngle>());
            string dletter = proj.Column.ColumnLetter;

            List<mdTrayRange> ranges = Ranges();

            uopPartList<mdEndAngle> _rVal = new uopPartList<mdEndAngle>(new List<mdEndAngle>());

            List<List<mdTrayRange>> mdranges = new List<List<mdTrayRange>>();

            foreach (mdTrayRange range in ranges)
            {
                List<mdTrayRange> subset = mdranges.Find((x) => x.FirstOrDefault().TrayAssembly.EndAnglesAreEqual(range.TrayAssembly));

                if (subset != null)
                    subset.Add(range);
                else
                    mdranges.Add(new List<mdTrayRange> { range });
            }


            for (int i = 0; i < mdranges.Count; i++)
            {
                List<mdTrayRange> subset = mdranges[i];
                int totaltrays = 0;
                foreach (mdTrayRange item in subset) { totaltrays += item.TrayCount; }
                List<string> pns = null;
                List<uopPart> baseparts = null;
                for (int j = 0; j < subset.Count; j++)
                {
                    mdTrayRange range = subset[j];
                    mdPartArray rangeparts = Find((x) => x.RangeGUID == range.RangeGUID);
                    if (rangeparts == null) continue;
                    rangeparts.UpdateParts(range, aProject: proj);
                    List<uopPart> parts = rangeparts.Item(uppPartTypes.EndAngle, aMatrix: this);

                    if (j == 0)
                    {
                        //create the end angles for the first in the list
                        baseparts = parts;
                        pns = new List<string>();
                        int i1 = 0;
                        int i2 = 0;

                        foreach (uopPart part in baseparts)
                        {
                            mdEndAngle ea = (mdEndAngle)part;   //should this be a clone ??
                            ea.RangeGUID = range.RangeGUID;
                            ea.Quantity = ea.OccuranceFactor * totaltrays;
                            ea.RangeIndex = range.Index;
                            ea._RangeIDS = new List<string>();
                            for (int k = j + 1; k < subset.Count; k++)
                            {
                                ea.AssociateToRange(subset[k]);
                            }

                            if (ea.Chamfered)
                            {
                                i1++;
                                ea.OverridePartNumber = $"{100 * range.Index + i1 * 10}{dletter}";
                            }
                            else
                            {
                                i2++;
                                ea.OverridePartNumber = $"{100 * range.Index + i2 * 10 + 5}{dletter}";
                            }
                            pns.Add(ea.OverridePartNumber);

                            //only keep the end angles on the first of the sub set of matching ranges
                            if (string.IsNullOrWhiteSpace(aRangeGUID) || (!string.IsNullOrWhiteSpace(aRangeGUID) && ea.IsAssociatedToRange(aRangeGUID)))
                                _rVal.Add(ea);
                        }

                    }
                    else
                    {
                        for (int p = 1; p <= parts.Count; p++)
                        {
                            mdEndAngle baseea = (mdEndAngle)baseparts[p - 1];

                            mdEndAngle ea = (mdEndAngle)parts[p - 1];   //should this be a clone ??

                            //just set the properties of the end angles on the subsequent set
                            ea.RangeIndex = range.Index;
                            baseea.AssociateToRange(ea.RangeGUID);
                            baseea.AssociateToParent(ea.DowncomerBox.PartNumber);
                            ea.Quantity = baseea.Quantity;
                            ea.OverridePartNumber = baseea.OverridePartNumber;
                            ea.AssociateToRange(baseea.RangeGUID);
                            ea.AssociateToParent(baseea.DowncomerBox.PartNumber);
                            ea.IsVirtual = true;
                        }

                    }
                }

            }


            if (!string.IsNullOrWhiteSpace(aRangeGUID))
                _rVal.RemoveAll((x) => !x.IsAssociatedToRange(aRangeGUID));
            
            if (!string.IsNullOrWhiteSpace(aDowncomerPN))
                _rVal.RemoveAll((x) => !x.IsAssociatedToParent(aDowncomerPN));

            return _rVal;
        }


   

        /// <summary>
        /// returns the unique downcomer stiffeners in the parent project. 
        /// </summary>
        /// <remarks>one of each part number with the quantity set to the quantity required for the entire column without spares.</remarks>
        /// <returns></returns>
        public uopPartList<mdStiffener> Stiffeners(string aRangeGUID = null)
        {

            mdProject proj = Project;
            if (proj == null) return new uopPartList<mdStiffener>(new List<mdStiffener>());

          
            List<mdTrayRange> ranges = Ranges();

        
            uopPartList<mdStiffener> _rVal = new uopPartList<mdStiffener>(new List<mdStiffener>());

            foreach (var array in this)
            {

                mdTrayRange range  = array.Range;
                //array.UpdateParts(range, aProject: proj);
                List<uopPart> assyparts = array.Item(uppPartTypes.Stiffener, aMatrix: this);
                // uopPartList<mdStiffener> assyparts = array.Stiffeners;
                if (assyparts.Count <= 0) continue;
               // proj.PartGenenerationStatus($"Gathering {range.TrayName()} - Stiffeners", false);
          
                foreach (uopPart part in assyparts)
                {
                    mdStiffener item = (mdStiffener)part;
                    mdStiffener match = _rVal.Find(x => x.IsEqual(item));

                    if (match != null)
                    {
                        match.MergeRangeAssociations(item._RangeIDS);
                        match.MergeParentAssociations(item._ParentPartNumbers);
                        match.Quantity += item.Quantity;
                    }
                    else
                    {
                        item.AssociateToRange(array.RangeGUID, true);
                        _rVal.Add(item);
                    }


                }

            }

            if (!string.IsNullOrWhiteSpace(aRangeGUID))
            {
                _rVal.RemoveAll((x) => !x.IsAssociatedToRange(aRangeGUID));
            }
            return _rVal;
        }

        /// <summary>
        /// returns the unique  manway splice plates in the parent project. 
        /// </summary>
        /// <remarks>one of each part number with the quantity set to the quantity required for the entire column without spares.</remarks>
        /// <returns></returns>
        public uopPartList<mdSpliceAngle> ManwaySplicePlates(string aRangeGUID = null)
        {

            mdProject proj = Project;
            if (proj == null) return new uopPartList<mdSpliceAngle>(new List<mdSpliceAngle>());


            List<mdTrayRange> ranges = Ranges();
          
            uopPartList<mdSpliceAngle> _rVal = new uopPartList<mdSpliceAngle>(new List<mdSpliceAngle>());

            foreach (var array in this)
            {

                mdTrayRange range = array.Range;
               // array.UpdateParts(range, aProject: proj);
                //uopPartList<mdSpliceAngle> assyparts = array.ManwaySplicePlates;
                
                List<uopPart> assyparts = array.Item(uppPartTypes.ManwaySplicePlate, aMatrix: this);
                if (assyparts.Count <= 0) continue;
               // proj.PartGenenerationStatus($"Gathering {range.TrayName()} - Manway Splice Plates", false);

                foreach (uopPart  part in assyparts)
                {
                    mdSpliceAngle item = (mdSpliceAngle)part;
                    mdSpliceAngle match = _rVal.Find(x => x.IsEqual(item));

                    if (match != null)
                    {
                        match.MergeRangeAssociations(item._RangeIDS);
                        match.MergeParentAssociations(item._ParentPartNumbers);
                        match.Quantity += item.Quantity;
                    }
                    else
                    {
                        item.AssociateToRange(array.RangeGUID, true);
                        _rVal.Add(item);
                    }
                }

            }
            if (!string.IsNullOrWhiteSpace(aRangeGUID))
            {
                _rVal.RemoveAll((x) => !x.IsAssociatedToRange(aRangeGUID));
            }
            return _rVal;
        }

        /// <summary>
        /// returns the unique splice angles in the parent project. 
        /// </summary>
        /// <remarks>one of each part number with the quantity set to the quantity required for the entire column without spares.</remarks>
        /// <returns></returns>
        public uopPartList<mdSpliceAngle> SpliceAngles(string aRangeGUID = null)
        {

            mdProject proj = Project;
            if (proj == null) return new uopPartList<mdSpliceAngle>(new List<mdSpliceAngle>());


            List<mdTrayRange> ranges = Ranges();
         
            uopPartList<mdSpliceAngle> _rVal = new uopPartList<mdSpliceAngle>(new List<mdSpliceAngle>());

            foreach (var array in this)
            {

                mdTrayRange range = array.Range;
                //array.UpdateParts(range, aProject: proj);
                //uopPartList<mdSpliceAngle> assyparts = array.SpliceAngles;
                List<uopPart> assyparts = array.Item(uppPartTypes.SpliceAngle, aMatrix: this);
                if (assyparts.Count <= 0) continue;
               // proj.PartGenenerationStatus($"Gathering {range.TrayName()} - Splice Angles", false);

                foreach (uopPart part in assyparts)
                {
                    mdSpliceAngle item = (mdSpliceAngle)part;
                    mdSpliceAngle match = _rVal.Find(x => x.IsEqual(item));

                    if (match != null)
                    {
                        match.MergeRangeAssociations(item._RangeIDS);
                        match.MergeParentAssociations(item._ParentPartNumbers);
                        match.Quantity += item.Quantity;
                    }
                    else
                    {
                        item.AssociateToRange(array.RangeGUID, true);
                        _rVal.Add(item);
                    }


                }

            }
            if (!string.IsNullOrWhiteSpace(aRangeGUID))
            {
                _rVal.RemoveAll((x) => !x.IsAssociatedToRange(aRangeGUID));
            }

            return _rVal;
        }

        /// <summary>
        /// returns the unique splice angles in the parent project. 
        /// </summary>
        /// <remarks>one of each part number with the quantity set to the quantity required for the entire column without spares.</remarks>
        /// <returns></returns>
        public uopPartList<mdSpliceAngle> ManwayAngles(string aRangeGUID = null)
        {

            mdProject proj = Project;
            if (proj == null) return new uopPartList<mdSpliceAngle>(new List<mdSpliceAngle>());


            List<mdTrayRange> ranges = Ranges();
        
            uopPartList<mdSpliceAngle> _rVal = new uopPartList<mdSpliceAngle>(new List<mdSpliceAngle>());

            foreach (var array in this)
            {

                mdTrayRange range = array.Range;
                //array.UpdateParts(range, aProject: proj);
                //uopPartList<mdSpliceAngle> assyparts = array.ManwayAngles;
                List<uopPart> assyparts = array.Item(uppPartTypes.ManwayAngle, aMatrix: this);

                if (assyparts.Count <= 0) continue;
               // proj.PartGenenerationStatus($"Gathering {range.TrayName()} - Manway Angles", false);

                foreach (uopPart part in assyparts)
                {
                    mdSpliceAngle item = (mdSpliceAngle)part;
                    mdSpliceAngle match = _rVal.Find(x => x.IsEqual(item));

                    if (match != null)
                    {
                        match.MergeRangeAssociations(item._RangeIDS);
                        match.MergeParentAssociations(item._ParentPartNumbers);
                        match.Quantity += item.Quantity;
                    }
                    else
                    {
                        item.AssociateToRange(array.RangeGUID, true);
                        _rVal.Add(item);
                    }


                }

            }
            if (!string.IsNullOrWhiteSpace(aRangeGUID))
            {
                _rVal.RemoveAll((x) => !x.IsAssociatedToRange(aRangeGUID));
            }

            return _rVal;
        }

        /// <summary>
        /// returns the unique  manway splice plates in the parent project. 
        /// </summary>
        /// <remarks>one of each part number with the quantity set to the quantity required for the entire column without spares.</remarks>
        /// <returns></returns>
        public uopPartList<mdSupplementalDeflector> SupplementalDeflectors(string aRangeGUID = null)
        {

            mdProject proj = Project;
            if (proj == null) return new uopPartList<mdSupplementalDeflector>(new List<mdSupplementalDeflector>());


            List<mdTrayRange> ranges = Ranges();
         
            uopPartList<mdSupplementalDeflector> _rVal = new uopPartList<mdSupplementalDeflector>(new List<mdSupplementalDeflector>());

            foreach (var array in this)
            {

                mdTrayRange range = array.Range;
                //array.UpdateParts(range, aProject: proj);
                //uopPartList<mdSupplementalDeflector> assyparts = array.SupplementalDeflectors;
                List<uopPart> assyparts = array.Item(uppPartTypes.SupplementalDeflector, aMatrix: this);
                if (assyparts.Count <= 0) continue;
               // proj.PartGenenerationStatus($"Gathering {range.TrayName()} - Supplemental Deflectors", false);

                foreach (uopPart part in assyparts)
                {
                    mdSupplementalDeflector item = (mdSupplementalDeflector)part;
                    mdSupplementalDeflector match = _rVal.Find(x => x.IsEqual(item));

                    if (match != null)
                    {
                        match.MergeRangeAssociations(item._RangeIDS);
                        match.MergeParentAssociations(item._ParentPartNumbers);
                        match.Quantity += item.Quantity;
                    }
                    else
                    {
                        item.AssociateToRange(array.RangeGUID, true);
                        _rVal.Add(item);
                    }


                }

            }
            if (!string.IsNullOrWhiteSpace(aRangeGUID))
            {
                _rVal.RemoveAll((x) => !x.IsAssociatedToRange(aRangeGUID));
            }

            return _rVal;
        }

        /// <summary>
        /// returns the unique deflector plates (baffles) in the parent project. 
        /// </summary>
        /// <remarks>one of each part number with the quantity set to the quantity required for the entire column without spares.</remarks>
        /// <returns></returns>
        public uopPartList<mdBaffle> DeflectorPlates(string aRangeGUID = null)
        {
            mdProject proj = Project;
            if (proj == null) return new uopPartList<mdBaffle>(new List<mdBaffle>());

            List<mdTrayRange> ranges = Ranges();
           
            uopPartList<mdBaffle> _rVal = new uopPartList<mdBaffle>(new List<mdBaffle>());
            List<mdPartArray> ecmdranges = ECMDMembers();
            int basepn = 900;
            string designltr = proj.ColumnLetter;
            foreach (var array in ecmdranges)
            {
                mdTrayRange range = array.Range;
                //array.UpdateParts(range, aProject: proj);
                //uopPartList<mdBaffle> assyparts = array.DeflectorPlates;
                List<uopPart> assyparts = array.Item(uppPartTypes.Deflector, aMatrix: this);
                if (assyparts.Count <= 0) continue;
               // proj.PartGenenerationStatus($"Gathering {range.TrayName()} - Deflector Plates", false);

                foreach (uopPart part in assyparts)
                {
                    mdBaffle item = (mdBaffle)part;
                    mdBaffle match = _rVal.Find(x => x.IsEqual(item));

                    if (match != null)
                    {
                        match.MergeRangeAssociations(item._RangeIDS);
                        match.MergeParentAssociations(item._ParentPartNumbers);
                        match.Quantity += item.Quantity;
                        item.OverridePartNumber = match.OverridePartNumber;
                    }
                    else
                    {
                        item.OverridePartNumber = $"{basepn}{designltr}";
                        basepn++;
                        item.AssociateToRange(array.RangeGUID, true);
                        _rVal.Add(item);
                    }
                }
            }
            if (!string.IsNullOrWhiteSpace(aRangeGUID))
            {
                _rVal.RemoveAll((x) => !x.IsAssociatedToRange(aRangeGUID));
            }
            return _rVal;
        }

        /// <summary>
        /// returns the unique AP Pans in the parent project. 
        /// </summary>
        /// <remarks>one of each part number with the quantity set to the quantity required for the entire column without spares.</remarks>
        /// <returns></returns>
        public uopPartList<mdAPPan> APPans(string aRangeGUID = null)
        {
            mdProject proj = Project;
            if (proj == null) return new uopPartList<mdAPPan>(new List<mdAPPan>());

            List<mdTrayRange> ranges = Ranges();
         
            uopPartList<mdAPPan> _rVal = new uopPartList<mdAPPan>(new List<mdAPPan>());
          
            int basepn = 800;
            string designltr = proj.ColumnLetter;
            foreach (var array in this)
            {
                mdTrayRange range = array.Range;
                //array.UpdateParts(range, aProject: proj);
                //uopPartList<mdAPPan> assyparts = array.APPans;
                List<uopPart> assyparts = array.Item(uppPartTypes.APPan, aMatrix: this);
                if (assyparts.Count <= 0) continue;
               // proj.PartGenenerationStatus($"Gathering {range.TrayName()} - AP Pans", false);

                foreach (uopPart part in assyparts)
                {
                    mdAPPan item = (mdAPPan)part;
                    mdAPPan match = _rVal.Find(x => x.IsEqual(item));

                    if (match != null)
                    {
                        match.MergeRangeAssociations(item._RangeIDS);
                        match.MergeParentAssociations(item._ParentPartNumbers);
                        match.Quantity += item.Quantity;
                        item.OverridePartNumber = match.OverridePartNumber;
                    }
                    else
                    {
                        item.OverridePartNumber = $"{basepn}{designltr}";
                        item.NodeName = $"AP Pan {item.PartNumber}";
                        basepn++;
                        item.AssociateToRange(array.RangeGUID, true);
                        _rVal.Add(item);
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(aRangeGUID))
            {
                _rVal.RemoveAll((x) => !x.IsAssociatedToRange(aRangeGUID));
            }
            return _rVal;
        }

        /// <summary>
        /// returns the unique End Plates in the parent project. 
        /// </summary>
        /// <remarks>one of each part number with the quantity set to the quantity required for the entire column without spares.</remarks>
        /// <returns></returns>
        public uopPartList<mdEndPlate> EndPlates(string aRangeGUID = null)
        {
            mdProject proj = Project;
            if (proj == null) return new uopPartList<mdEndPlate>(new List<mdEndPlate>());

            List<mdTrayRange> ranges = Ranges();
          
            uopPartList<mdEndPlate> _rVal = new uopPartList<mdEndPlate>(new List<mdEndPlate>());
 
     
            foreach (var array in this)
            {
                mdTrayRange range = array.Range;
                //array.UpdateParts(range, aProject: proj);
                //uopPartList<mdEndPlate> assyparts = array.EndPlates;
                List<uopPart> assyparts = array.Item(uppPartTypes.EndPlate, aMatrix: this);
                if (assyparts.Count <= 0) continue;
               // proj.PartGenenerationStatus($"Gathering {range.TrayName()} - End Plates", false);

                foreach (uopPart part in assyparts)
                {
                    mdEndPlate item = (mdEndPlate)part;
                    mdEndPlate match = _rVal.Find(x => x.IsEqual(item));

                    if (match != null)
                    {
                        match.MergeRangeAssociations(item._RangeIDS);
                        match.MergeParentAssociations(item._ParentPartNumbers);
                        match.Quantity += item.Quantity;
                 
                    }
                    else
                    {
                   
                        item.AssociateToRange(array.RangeGUID, true);
                        _rVal.Add(item);
                    }
                }
            }
            if (!string.IsNullOrWhiteSpace(aRangeGUID))
            {
                _rVal.RemoveAll((x) => !x.IsAssociatedToRange(aRangeGUID));
            }
            return _rVal;
        }

        /// <summary>
        /// returns the unique downcomer boxes in the parent project. 
        /// </summary>
        /// <remarks>one of each part number with the quantity set to the quantity required for the entire column without spares.</remarks>
        /// <returns></returns>
        public uopPartList<mdEndSupport> EndSupports(string aRangeGUID = null)
        {
            mdProject proj = Project;
            if (proj == null) return new uopPartList<mdEndSupport>(new List<mdEndSupport>());

            List<mdTrayRange> ranges = Ranges();
            uopPartList<mdEndSupport> _rVal = new uopPartList<mdEndSupport>(new List<mdEndSupport>());


            foreach (var array in this)
            {
                mdTrayRange range = array.Range;
                //array.UpdateParts(range, aProject: proj);
                //uopPartList<mdEndSupport> assyparts = array.EndSupports;
                List<uopPart> assyparts = array.Item(uppPartTypes.EndSupport, aMatrix: this);
                if (assyparts.Count <= 0) continue;
               // proj.PartGenenerationStatus($"Gathering {range.TrayName()} - End Supports", false);

                foreach (uopPart part in assyparts)
                {
                    mdEndSupport item = (mdEndSupport)part;
                    mdEndSupport match = _rVal.Find(x => x.IsEqual(item));

                    if (match != null)
                    {
                        match.MergeRangeAssociations(item._RangeIDS);
                        match.MergeParentAssociations(item._ParentPartNumbers);
                        match.Quantity += item.Quantity;
    
                    }
                    else
                    {

                        item.AssociateToRange(array.RangeGUID, true);
                        _rVal.Add(item);
                    }
                }
            }
            if (!string.IsNullOrWhiteSpace(aRangeGUID))
            {
              _rVal.RemoveAll((x) => !x.IsAssociatedToRange(aRangeGUID));
            }

            return _rVal;
        }

        /// <summary>
        /// returns the unique Tray Support Beams in the parent project. 
        /// </summary>
        /// <remarks>one of each part number with the quantity set to the quantity required for the entire column without spares.</remarks>
        /// <returns></returns>
        public uopPartList<mdBeam> SupportBeams(string aRangeGUID = null)
        {
            mdProject proj = Project;
            if (proj == null) return new uopPartList<mdBeam>(new List<mdBeam>());

            List<mdTrayRange> ranges = Ranges();
            uopPartList<mdBeam> _rVal = new uopPartList<mdBeam>(new List<mdBeam>());


            foreach (var array in this)
            {
                mdTrayRange range = array.Range;
                //array.UpdateParts(range, aProject: proj);
                //uopPartList<mdDowncomerBox> assyparts = array.Boxes;
                List<uopPart> assyparts = array.Item(uppPartTypes.TraySupportBeam, aMatrix: this);

                if (assyparts.Count <= 0) continue;
                // proj.PartGenenerationStatus($"Gathering {range.TrayName()} - Downcomer Boxes", false);

                int pnidx = 851;

                foreach (uopPart part in assyparts)
                {
                    mdBeam item = (mdBeam)part;

                    mdBeam match = _rVal.Find(x => x.IsEqual(item));

                    if (match != null)
                    {
                        match.MergeRangeAssociations(item._RangeIDS);
                        match.Quantity += item.Quantity;
                    }
                    else
                    {

                        item.PartNumberIndex = pnidx;
                        pnidx++;
                        item.NodeName = $"Tray Support Beam {item.PartNumber}";
                        item.AssociateToRange(array.RangeGUID, true);
                        _rVal.Add(item);
                    }
                }
            }
            if (!string.IsNullOrWhiteSpace(aRangeGUID))
            {
                _rVal.RemoveAll((x) => !x.IsAssociatedToRange(aRangeGUID));
            }
            return _rVal;
        }

        /// <summary>
        /// returns the unique Downcmoer Boxes in the parent project. 
        /// </summary>
        /// <remarks>one of each part number with the quantity set to the quantity required for the entire column without spares.</remarks>
        /// <returns></returns>
        public uopPartList<mdDowncomerBox> Boxes(string aRangeGUID = null)
        {
            mdProject proj = Project;
            if (proj == null) return new uopPartList<mdDowncomerBox>(new List<mdDowncomerBox>());

            List<mdTrayRange> ranges = Ranges();
            uopPartList<mdDowncomerBox> _rVal = new uopPartList<mdDowncomerBox>(new List<mdDowncomerBox>());


            foreach (var array in this)
            {
                mdTrayRange range = array.Range;
                if (range == null) continue;
                if (!string.IsNullOrWhiteSpace(aRangeGUID) && string.Compare(range.GUID, aRangeGUID, true) != 0) continue;

                List<uopPart> assyparts = array.Item(uppPartTypes.DowncomerBox, aMatrix: this);

                foreach ( uopPart  part in assyparts)
                {
                    mdDowncomerBox item = (mdDowncomerBox)part;
                     item.AssociateToRange(array.RangeGUID, true);
                     _rVal.Add(item);
                }
            }
            if (!string.IsNullOrWhiteSpace(aRangeGUID))
            {
                _rVal.RemoveAll((x) => !x.IsAssociatedToRange(aRangeGUID));
            }
            return _rVal;
        }

        /// <summary>
        /// returns the unique Deck Sections in the parent project. 
        /// </summary>
        /// <remarks>one of each part number with the quantity set to the quantity required for the entire column without spares.</remarks>
        /// <returns></returns>
        public uopPartList<mdDeckSection> DeckSections(string aRangeGUID = null, bool bRegen = false, bool bIncludeAltRing = false)
        {
            mdProject proj = Project;
            if (proj == null) return new uopPartList<mdDeckSection>(new List<mdDeckSection>());

            List<mdTrayRange> ranges = Ranges();
         uopPartList<mdDeckSection> _rVal = new uopPartList<mdDeckSection>(new List<mdDeckSection>());

            List<mdPartArray> removers = new List<mdPartArray>();

            foreach (var array in this)
            {
                mdTrayRange range = array.Range;
                if (range == null) continue;
                if (!string.IsNullOrWhiteSpace(aRangeGUID) && string.Compare(range.GUID ,aRangeGUID,true) !=0 )continue;
               List<uopPart> assyparts = array.Item(uppPartTypes.DeckSection,bAltRing: false, aMatrix: this,  bRegen: bRegen);
               
                foreach (uopPart part in assyparts)
                {
                    mdDeckSection item = (mdDeckSection)part;
                    item.AssociateToRange(array.RangeGUID, true);
                    _rVal.Add(item);
          
                }

                if (bIncludeAltRing && range.TrayAssembly.HasAlternateDeckParts)
                {
                    List<uopPart> altparts = array.Item(uppPartTypes.DeckSection, bAltRing: true, aMatrix: this, bRegen: false);
                    foreach (uopPart part in altparts)
                    {
                        if(assyparts.FindIndex(x => x.PartNumber == part.PartNumber) <0)
                        {
                            mdDeckSection item = (mdDeckSection)part;
                            item.AssociateToRange(array.RangeGUID, true);
                            _rVal.Add(item);
                        }
                      

                    }

                   
                }
               



            }
            if (!string.IsNullOrWhiteSpace(aRangeGUID))
            {
                _rVal.RemoveAll((x) => !x.IsAssociatedToRange(aRangeGUID));
            }
            return _rVal;
        }

        /// <summary>
        /// returns the unique Alternate Ring Deck Sections in the parent project . 
        /// </summary>
        /// <remarks>one of each part number with the quantity set to the quantity required for the entire column without spares.</remarks>
        /// <returns></returns>
        public uopPartList<mdDeckSection> AltDeckSections(string aRangeGUID = null, bool bRegen = false)
        {
            mdProject proj = Project;
            if (proj == null) return new uopPartList<mdDeckSection>(new List<mdDeckSection>());

            List<mdTrayRange> ranges = Ranges();
            uopPartList<mdDeckSection> _rVal = new uopPartList<mdDeckSection>(new List<mdDeckSection>());


            foreach (var array in this)
            {
                mdTrayRange range = array.Range;
                if (range == null) continue;
                if (!string.IsNullOrWhiteSpace(aRangeGUID) && string.Compare(range.GUID, aRangeGUID, true) != 0) continue;

                List<uopPart> assyparts = array.Item(uppPartTypes.DeckSection, bAltRing: true, aMatrix: this, bRegen: bRegen);
      
                foreach (uopPart part in assyparts)
                {
                    mdDeckSection item = (mdDeckSection)part;
                    item.AssociateToRange(array.RangeGUID, true);
                    _rVal.Add(item);
                }
            }
            if (!string.IsNullOrWhiteSpace(aRangeGUID))
            {
                _rVal.RemoveAll((x) => !x.IsAssociatedToRange(aRangeGUID));
            }
           // proj.PartGenenerationStatus("", false);
            return _rVal;
        }


        internal new void Add(mdPartArray aArray) 
        { 
                if(aArray == null) return;
            if (string.IsNullOrWhiteSpace( aArray.RangeGUID)) return;
            int idx = FindIndex((x) => string.Compare(x.RangeGUID, aArray.RangeGUID, true) == 0);
            if (idx != -1) 
            { 
                if(idx == Count - 1)
                {
                    RemoveAt(idx);
                    base.Add(aArray);

                }
                else
                {
                    RemoveAt(idx);
                    Insert(idx, aArray);
                }
            } 
            else 
            { 
                base.Add(aArray);
            }
        }

        public bool TryGetEqualPart(uopPart aPart, out uopPart rPart)
        {
            rPart = null;
            if (aPart == null) return false;
            foreach (var item in this)
            {
                if (item.TryGetEqualPart(aPart, out rPart))
                {
                    return true;
                }
            }
            return false;
        }
        internal void InvalidateParts(Message_PartsInvalidated aMessage)
        {
            if(aMessage == null) return;
           
            foreach (var item in this)
            {
                item.InvalidateParts(aMessage);
            }
        }

        public void InvalidateParts(bool bInvalidateAll = false, string aRangeGUID = null, uppPartTypes? aPartType = null, List<uppPartTypes> aPartTypes = null)
        {
            InvalidateParts(new Message_PartsInvalidated(Project?.Handle, aRangeGUID: aRangeGUID, aPartType: aPartType, aPartTypes: aPartTypes));
        }

        public List<mdPartArray> ECMDMembers() => FindAll((x) => x.DesignFamily.IsEcmdDesignFamily());
        

        #endregion Methods

    }
}
