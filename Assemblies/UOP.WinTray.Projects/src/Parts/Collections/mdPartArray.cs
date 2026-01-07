
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Events;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Parts;

using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;

namespace UOP.WinTray.Projects
{


     public class mdPartArray
    {

        #region Constructors

        public mdPartArray()  => Init();

        public mdPartArray(mdTrayRange aRange) => Init(aRange);

        

        private void Init(mdTrayRange aRange = null) 
        {
            _RangeGUID = string.Empty;
         
            _RangeRef = null;
            DesignFamily = uppMDDesigns.Undefined;
            if (aRange == null) return;
            DesignFamily = aRange.DesignFamily;
            _RangeGUID = aRange.GUID;
            _RangeRef = new WeakReference<mdTrayRange>(aRange);
            Clear(aRange: aRange);
        }

        #endregion Constructors

        #region Properties

       public uppMDDesigns DesignFamily { get;set; }

        private WeakReference<mdTrayRange> _RangeRef;
        public mdTrayRange Range
        {
            get
            {
                if(_RangeRef == null) return null;
                if(!_RangeRef.TryGetTarget(out mdTrayRange _rVal))  Init(null);
                return _rVal;
                
            }
        }

        private string _RangeGUID;
        public string RangeGUID => _RangeGUID;

    
        private  uopPartList<mdEndAngle> EndAngles;
        private uopPartList<mdStiffener> Stiffeners;
        private uopPartList<mdSpliceAngle> ManwaySplicePlates;
        private uopPartList<mdSpliceAngle> SpliceAngles;
        private uopPartList<mdSpliceAngle> ManwayAngles;
        private uopPartList<mdSupplementalDeflector> SupplementalDeflectors;
        private uopPartList<mdBaffle> DeflectorPlates;
        private uopPartList<mdAPPan>APPans;
        private uopPartList<mdEndPlate> EndPlates;
        private uopPartList<mdEndSupport> EndSupports;
        private uopPartList<mdDowncomerBox> Boxes;
        private uopPartList<mdDeckSection> DeckSections;
        private uopPartList<mdDeckSection> AltRingDeckSections;
        private uopPartList<mdBeam> SupportBeams;

        #endregion Properties

        #region Methods

        public void Clear(bool bNullOnly = false, mdTrayRange aRange = null)
        {
            aRange ??= Range;
            bool needparts = aRange == null ? false : aRange.ProjectType == uppProjectTypes.MDDraw;
            if (!bNullOnly)
            {

                EndAngles = new uopPartList<mdEndAngle>(new List<mdEndAngle>()) { InvalidWhenEmpty = needparts };
                Stiffeners = new uopPartList<mdStiffener>(new List<mdStiffener>()) { InvalidWhenEmpty = needparts };
                ManwaySplicePlates = new uopPartList<mdSpliceAngle>(new List<mdSpliceAngle>()) { InvalidWhenEmpty = needparts };
                SpliceAngles = new uopPartList<mdSpliceAngle>(new List<mdSpliceAngle>()) { InvalidWhenEmpty = needparts };
                ManwayAngles = new uopPartList<mdSpliceAngle>(new List<mdSpliceAngle>()) { InvalidWhenEmpty = needparts };
                SupplementalDeflectors = new uopPartList<mdSupplementalDeflector>(new List<mdSupplementalDeflector>()) { InvalidWhenEmpty = needparts };
                DeflectorPlates = new uopPartList<mdBaffle>(new List<mdBaffle>()) { InvalidWhenEmpty = DesignFamily.IsEcmdDesignFamily() };
                APPans = new uopPartList<mdAPPan>(new List<mdAPPan>()) { InvalidWhenEmpty = needparts };
                EndPlates = new uopPartList<mdEndPlate>(new List<mdEndPlate>()) { InvalidWhenEmpty = needparts };
                EndSupports = new uopPartList<mdEndSupport>(new List<mdEndSupport>()) { InvalidWhenEmpty = needparts };
                Boxes = new uopPartList<mdDowncomerBox>(new List<mdDowncomerBox>()) { InvalidWhenEmpty = needparts };
                DeckSections = new uopPartList<mdDeckSection>(new List<mdDeckSection>()) { InvalidWhenEmpty = needparts };
                AltRingDeckSections = new uopPartList<mdDeckSection>(new List<mdDeckSection>()) { InvalidWhenEmpty = needparts };
                SupportBeams = new uopPartList<mdBeam>(new List<mdBeam>()) { InvalidWhenEmpty = false };
            }
            else
            {
                EndAngles ??= new uopPartList<mdEndAngle>(new List<mdEndAngle>()) { InvalidWhenEmpty = needparts };
                Stiffeners ??= new uopPartList<mdStiffener>(new List<mdStiffener>()) { InvalidWhenEmpty = needparts };
                ManwaySplicePlates ??= new uopPartList<mdSpliceAngle>(new List<mdSpliceAngle>()) { InvalidWhenEmpty = needparts };
                SpliceAngles ??= new uopPartList<mdSpliceAngle>(new List<mdSpliceAngle>()) { InvalidWhenEmpty = needparts };
                ManwayAngles ??= new uopPartList<mdSpliceAngle>(new List<mdSpliceAngle>()) { InvalidWhenEmpty = needparts };
                SupplementalDeflectors ??= new uopPartList<mdSupplementalDeflector>(new List<mdSupplementalDeflector>()) { InvalidWhenEmpty = needparts };
                DeflectorPlates ??= new uopPartList<mdBaffle>(new List<mdBaffle>()) { InvalidWhenEmpty = DesignFamily.IsEcmdDesignFamily() };
                APPans ??= new uopPartList<mdAPPan>(new List<mdAPPan>()) { InvalidWhenEmpty = needparts };
                EndPlates ??= new uopPartList<mdEndPlate>(new List<mdEndPlate>()) { InvalidWhenEmpty = needparts };
                EndSupports ??= new uopPartList<mdEndSupport>(new List<mdEndSupport>()) { InvalidWhenEmpty = needparts };
                Boxes ??= new uopPartList<mdDowncomerBox>(new List<mdDowncomerBox>()) { InvalidWhenEmpty = needparts };
                DeckSections ??= new uopPartList<mdDeckSection>(new List<mdDeckSection>()) { InvalidWhenEmpty = needparts };
                AltRingDeckSections ??= new uopPartList<mdDeckSection>(new List<mdDeckSection>()) { InvalidWhenEmpty = needparts };
                SupportBeams ??= new uopPartList<mdBeam>(new List<mdBeam>()) { InvalidWhenEmpty = false };
            }

            if (aRange == null ) return;
            mdTrayAssembly assy = aRange.TrayAssembly;
            uopDeckSplices splices = needparts ? assy.DeckSplices : null;
            DeflectorPlates.InvalidWhenEmpty = !needparts ? false :  aRange.DesignFamily.IsEcmdDesignFamily();
            APPans.InvalidWhenEmpty = !needparts ? false : assy.DesignOptions.HasAntiPenetrationPans;
            SupplementalDeflectors.InvalidWhenEmpty = !needparts ? false : assy.Downcomers.HasSupplementalDeflectors;
            ManwaySplicePlates.InvalidWhenEmpty = !needparts ? false : assy.DesignOptions.SpliceStyle == uppSpliceStyles.Tabs && splices.ManwayCount(false) > 0;
            SpliceAngles.InvalidWhenEmpty = !needparts ? false : assy.DesignOptions.SpliceStyle == uppSpliceStyles.Angle && splices.Count > 0;
            ManwayAngles.InvalidWhenEmpty = !needparts ? false : assy.DesignOptions.SpliceStyle == uppSpliceStyles.Angle && splices.Count > 0;
            AltRingDeckSections.InvalidWhenEmpty = !needparts ? false : assy.HasAlternateDeckParts;
            SupportBeams.InvalidWhenEmpty = aRange.DesignFamily.IsBeamDesignFamily();

        }
        public override string ToString()
        {
            mdTrayRange range = Range;
            return range == null ? $"mdPartArray" : $"mdPartArray {range.TrayName(true) }";
        }


        public bool UpdateParts(mdTrayRange aRange = null, bool bUpdateAll = false, List<uppPartTypes> aRegenList = null, mdProject aProject = null, mdPartMatrix aMatrix = null) 
        {
            aRange ??= Range;
            if (aRange == null) return false;
            mdTrayAssembly assy = aRange.TrayAssembly;
            aRegenList ??= new List<uppPartTypes>();

            string rguid = aRange.GUID;
            int tcount = aRange.TrayCount;
            string phandle = aRange.ProjectHandle;
            string tname = aRange.TrayName();
            List<mdDowncomer> dcs = assy.Downcomers.GetByVirtual(aVirtualValue: false);
            if (!assy.IsValid(uppPartTypes.Downcomers))  aRegenList.Add(uppPartTypes.EndAngle);
            
            Clear(bNullOnly: !bUpdateAll, aRange: aRange);
          if(  InvalidateParts(aRegenList) ==0) 
                return false;

            if (SupportBeams.Invalid)
                Update_Beams(aRange, aProject, aMatrix);


            if (Boxes.Invalid)
                Update_Boxes(aRange, aProject, aMatrix);

     
            if (DeckSections.Invalid)
                Update_DeckSections(aRange, aProject, aMatrix);

            if (APPans.Invalid)
                Update_APPans(aRange, aProject, aMatrix);

            //do the splice angles manway angles and manway  deck splices
            Update_SplicePlates(aRange, aProject, aMatrix);

            if ( EndAngles.Invalid)
                Update_EndAngles(aRange, aProject, aMatrix);
            
            if ( Stiffeners.Invalid)
                Update_Stiffeners(aRange, aProject, aMatrix);
                
            if ( SupplementalDeflectors.Invalid)
                Update_SupplementalDeflectos(aRange, aProject, aMatrix);
        
            if (DeflectorPlates.Invalid)
                Update_Deflectors(aRange, aProject, aMatrix);
     
            if (EndPlates.Invalid)
                Update_EndPlates(aRange, aProject, aMatrix);
       

            if ( EndSupports.Invalid)
                Update_EndSupports(aRange, aProject, aMatrix);


            return true;
        }
        private void Update_DeckSections(mdTrayRange aRange, mdProject aProject, mdPartMatrix aMatix = null)
        {
            DeckSections.Clear();
            ManwayAngles.Clear();
            ManwaySplicePlates.Clear();
            SpliceAngles.Clear();
            AltRingDeckSections.Clear();

            aRange ??= Range;
            if (aRange == null) return;
            //aProject ??= aRange.MDProject;
            string rguid = aRange.GUID;
            int tcount = aRange.TrayCount;
            string phandle = aRange.ProjectHandle;
            string tname = aRange.TrayName();
            mdTrayAssembly assy = aRange.TrayAssembly;
            aMatix?.GenerationStatus($"Generating {tname} - Unique Deck Sections", true);

            List<mdDeckSection> assyparts =  mdPartGenerator.DeckSections_ASSY(assy, out List<mdDeckSection> altsecs);
            foreach (var part in assyparts)
            {
                part.AssociateToRange(rguid, true);
                part.ProjectHandle = phandle;
                DeckSections.Add(part);
            }
            DeckSections.Invalid = false;
            if (altsecs.Count > 0)
            {
                foreach (var part in altsecs)
                {
                    part.AssociateToRange(rguid, true);
                    part.ProjectHandle = phandle;
                    AltRingDeckSections.Add(part);
                }

            }
            else
            {
                foreach (var part in DeckSections)
                {
                    AltRingDeckSections.Add(part);
                }
            }
            AltRingDeckSections.Invalid = false;
            aMatix?.GenerationStatus(string.Empty, false);

        }

        private void Update_Boxes(mdTrayRange aRange, mdProject aProject, mdPartMatrix aMatix = null)
        {
            string action = Boxes.Count == 0 ? "Generating" : "Updating";
            EndPlates.Invalid = true;
            EndAngles.Invalid = true;
            EndSupports.Invalid = true;
            DeflectorPlates.Invalid = true;

            Boxes.Clear();
            aRange ??= Range;
            if (aRange == null) return;
            //aProject ??= aRange.MDProject;
            string rguid = aRange.GUID;
            int tcount = aRange.TrayCount;
            string phandle = aRange.ProjectHandle;
            string tname = aRange.TrayName();
            mdTrayAssembly assy = aRange.TrayAssembly;

      
            aMatix?.GenerationStatus($"{action} {tname} - Downcomer Boxes", true);
        
            List<mdDowncomerBox> assyparts = mdPartGenerator.Boxes_ASSY(assy, aCountMultiplier: tcount);
            foreach (var part in assyparts)
            {
                part.AssociateToRange(rguid, true);
                part.ProjectHandle = phandle;
                Boxes.Add(part);
            }
            Boxes.Invalid = false;
            aMatix?.GenerationStatus(string.Empty, false);
        }

        private void Update_Beams(mdTrayRange aRange, mdProject aProject, mdPartMatrix aMatix = null)
        {
            string action = SupportBeams.Count == 0 ? "Generating" : "Updating";

            SupportBeams.Clear();
            aRange ??= Range;
            if (aRange == null) return;
            if (!aRange.DesignFamily.IsBeamDesignFamily()) return;

            //aProject ??= aRange.MDProject;
            string rguid = aRange.GUID;
            int tcount = aRange.TrayCount;
            string phandle = aRange.ProjectHandle;
            string tname = aRange.TrayName();
            mdTrayAssembly assy = aRange.TrayAssembly;


            aMatix?.GenerationStatus($"{action} {tname} - Tray Support Beams", true);

            List<mdBeam> assyparts = mdPartGenerator.Beams_ASSY(assy, aCountMultiplier: tcount);
            foreach (var part in assyparts)
            {
                part.AssociateToRange(rguid, true);
                part.ProjectHandle = phandle;
                SupportBeams.Add(part);
            }
            SupportBeams.Invalid = false;
            aMatix?.GenerationStatus(string.Empty, false);
        }

        private void Update_APPans(mdTrayRange aRange, mdProject aProject, mdPartMatrix aMatix = null)
        {
            string action = APPans.Count == 0 ? "Generating" : "Updating";

            aRange ??= Range;
            if (aRange == null) return;
            //aProject ??= aRange.MDProject;
            string rguid = aRange.GUID;
            int tcount = aRange.TrayCount;
            string phandle = aRange.ProjectHandle;
            string tname = aRange.TrayName();
            mdTrayAssembly assy = aRange.TrayAssembly;

            aMatix?.GenerationStatus($"{action} {tname} - AP Pans", true);

            List<mdAPPan> assyparts = mdPartGenerator.APPans_ASSY(assy, aCountMultiplier: tcount);
            foreach (var part in assyparts)
            {
                part.AssociateToRange(rguid, true);
                part.ProjectHandle = phandle;
                APPans.Add(part);
            }
            APPans.Invalid = false;
            aMatix?.GenerationStatus(string.Empty, false);
        }

        private void Update_EndAngles(mdTrayRange aRange, mdProject aProject, mdPartMatrix aMatix = null)
        {
            string action = EndAngles.Count == 0 ? "Generating" : "Updating";

            EndAngles.Clear();
            aRange ??= Range;
            if (aRange == null) return;
            //aProject ??= aRange.MDProject;
            string rguid = aRange.GUID;
            int tcount = aRange.TrayCount;
            string phandle = aRange.ProjectHandle;
            string tname = aRange.TrayName();
            mdTrayAssembly assy = aRange.TrayAssembly;
            
          
            aMatix?.GenerationStatus($"{action} {tname} - End Angles", true);
          
         
                List<mdEndAngle> parts = mdPartGenerator.EndAngles_ASSY(assy,bApplyInstances: true, aCountMultiplier: tcount);
                foreach (mdEndAngle part in parts)
                {
                    part.AssociateToRange(rguid, true);
                    part.ProjectHandle = phandle;
                    EndAngles.Add(part);
                }
            EndAngles.Invalid = false;
            aMatix?.GenerationStatus(string.Empty, false);
        }
        private void Update_Stiffeners(mdTrayRange aRange, mdProject aProject, mdPartMatrix aMatix = null)
        {
            string action = Stiffeners.Count == 0 ? "Generating" : "Updating";

            Stiffeners.Clear();
            aRange ??= Range;
            if (aRange == null) return;
            //aProject ??= aRange.MDProject;
            string rguid = aRange.GUID;
            int tcount = aRange.TrayCount;
            string phandle = aRange.ProjectHandle;
            string tname = aRange.TrayName();

            aMatix?.GenerationStatus($"{action}  {tname} - Stiffeners", true);
        
            List<mdStiffener> assyparts = mdPartGenerator.Stiffeners_ASSY(aRange.TrayAssembly, true, aCountMultiplier: tcount);
            foreach (var part in assyparts)
            {
                part.NodeName = string.Empty;
                part.AssociateToRange(rguid, true);
                part.ProjectHandle = phandle;
                Stiffeners.Add(part);
            }
            Stiffeners.Invalid =false;
            aMatix?.GenerationStatus(string.Empty, false);
        }

        private void Update_SupplementalDeflectos(mdTrayRange aRange, mdProject aProject, mdPartMatrix aMatix = null)
        {
            string action = SupplementalDeflectors.Count == 0 ? "Generating" : "Updating";

            SupplementalDeflectors.Clear();
            aRange ??= Range;
            if (aRange == null) return;
            //aProject ??= aRange.MDProject;
            string rguid = aRange.GUID;
            int tcount = aRange.TrayCount;
            string phandle = aRange.ProjectHandle;
            string tname = aRange.TrayName();
            mdTrayAssembly assy = aRange.TrayAssembly;

            aMatix?.GenerationStatus($"{action} {tname} - Supplemental Deflectors", true);
         
            List<mdSupplementalDeflector> assyparts = mdPartGenerator.SuppDefs_ASSY(assy, true, aCountMultiplier: tcount);
            foreach (var part in assyparts)
            {
                part.AssociateToRange(rguid, true);
                part.ProjectHandle = phandle;
                SupplementalDeflectors.Add(part);
            }
            SupplementalDeflectors.Invalid = false;
            aMatix?.GenerationStatus(string.Empty, false);
        }

        private void Update_Deflectors(mdTrayRange aRange, mdProject aProject, mdPartMatrix aMatix = null)
        {
            string action = DeflectorPlates.Count == 0 ? "Generating" : "Updating";
            DeflectorPlates.Clear();
            aRange ??= Range;
            if (aRange == null) return;
            aProject ??= aRange.MDProject;
            string rguid = aRange.GUID;
            int tcount = aRange.TrayCount;
            string phandle = aRange.ProjectHandle;
            string tname = aRange.TrayName();
            mdTrayAssembly assy = aRange.TrayAssembly;
           aMatix?.GenerationStatus($"{action}  {tname} - Deflector Plates", true);
            int basepn = 900;
            string colltr = aRange.ColumnLetter;

            

            List<mdBaffle> assyparts = mdPartGenerator.DeflectorPlates_ASSY(assy, bApplyInstances: true, aCountMultiplier: tcount);

            List<mdBaffle> base_set = new List<mdBaffle>(assyparts);
            //get the distributor deflectors
            if (aProject != null)
            {

                List<mdDistributor> distributors = aProject.Distributors.ToList();
                distributors = distributors != null? distributors.FindAll((x) => aRange.RingRange.Includes(x.TrayNumberBelow)) : new List<mdDistributor>();

                foreach (var part in aProject.Distributors)
                {
                    if (part == null) continue;
                    foreach( mdBaffle baffle in base_set)
                    {

                        mdBaffle clone = new mdBaffle(baffle);
                        baffle.Quantity -= tcount;

                         clone.Instances_Set(new TINSTANCES(baffle.Instances_Get()));
                        clone.Quantity = tcount;
                        clone.DCCenters = TVALUES.Null;
                        clone.DistributorIndex = part.Index;
                        clone.IsBlank = true;
                        clone.AssociateToRange(rguid, true);
                        assyparts.Add(clone);
                    }

                }
            }


            foreach (var part in assyparts)
            {
                part.OverridePartNumber = $"{basepn}{colltr}";
                part.AssociateToRange(rguid, true);
                part.ProjectHandle = phandle;
                DeflectorPlates.Add(part);
            }

      

            DeflectorPlates.Invalid = false;
            aMatix?.GenerationStatus(string.Empty, false);
        }

        private void Update_EndPlates(mdTrayRange aRange, mdProject aProject, mdPartMatrix aMatix = null)
        {
            string action = EndPlates.Count == 0 ? "Generating" : "Updating";
            EndPlates.Clear();
            aRange ??= Range;
            if (aRange == null) return;
            //aProject ??= aRange.MDProject;
            string rguid = aRange.GUID;
            int tcount = aRange.TrayCount;
            string phandle = aRange.ProjectHandle;
            string tname = aRange.TrayName();
            mdTrayAssembly assy = aRange.TrayAssembly;
         
            aMatix?.GenerationStatus($"{action}  {tname} - End Plates", true);
          
            List<mdEndPlate> assyparts = mdPartGenerator.EndPlates_ASSY(assy);
            foreach (var part in assyparts)
            {
                part.AssociateToRange(rguid, true);
                part.ProjectHandle = phandle;
                EndPlates.Add(part);
            }
            EndPlates.Invalid = false;
            aMatix?.GenerationStatus(string.Empty, false);
        }
        private void Update_EndSupports(mdTrayRange aRange, mdProject aProject, mdPartMatrix aMatix = null)
        {
            string action = EndSupports.Count == 0 ? "Generating" : "Updating";
            EndSupports.Clear();
            aRange ??= Range;
            if (aRange == null) return;
            //aProject ??= aRange.MDProject;
            string rguid = aRange.GUID;
            int tcount = aRange.TrayCount;
            string phandle = aRange.ProjectHandle;
            string tname = aRange.TrayName();
            mdTrayAssembly assy = aRange.TrayAssembly;
            aMatix?.GenerationStatus($"{action}  {tname} - End Supports", true);
            
            List<mdEndSupport> assyparts = mdPartGenerator.EndSupports_ASSY(assy);
            foreach (var part in assyparts)
            {
                part.AssociateToRange(rguid, true);
                part.ProjectHandle = phandle;
                EndSupports.Add(part);
            }
            EndSupports.Invalid = false;
            aMatix?.GenerationStatus(string.Empty, false);

        }
       
        private void Update_SplicePlates(mdTrayRange aRange, mdProject aProject, mdPartMatrix aMatix = null)
        {
            if (!ManwaySplicePlates.Invalid && !SpliceAngles.Invalid && !ManwayAngles.Invalid) return;
            string action1 = ManwaySplicePlates.Count == 0 ? "Generating" : "Updating";
            string action2 = SpliceAngles.Count == 0 ? "Generating" : "Updating";
            string action3 = ManwayAngles.Count == 0 ? "Generating" : "Updating";

            if (ManwaySplicePlates.Invalid ) ManwayAngles.Clear();
            if (SpliceAngles.Invalid) SpliceAngles.Clear();
            if (ManwayAngles.Invalid) ManwayAngles.Clear();
            aRange ??= Range;
            if (aRange == null) return;
            //aProject ??= aRange.MDProject;
            string rguid = aRange.GUID;
            int tcount = aRange.TrayCount;
            string phandle = aRange.ProjectHandle;
            string tname = aRange.TrayName();
            mdTrayAssembly assy = aRange.TrayAssembly;
            if (ManwaySplicePlates.Invalid)
            {

                aMatix?.GenerationStatus($"{action1} {tname} - Manway Splice Plates", true);
            
                List<mdSpliceAngle> assyparts = mdPartGenerator.ManwaySplices_ASSY(assy, true, aCountMultiplier: tcount);
                if(assyparts.Count > 0)
                {
                    foreach (var part in assyparts)
                    {
                        part.AssociateToRange(rguid, true);
                        part.ProjectHandle = phandle;
                        ManwaySplicePlates.Add(part);
                    }
                }
                
                ManwaySplicePlates.Invalid = false;
                aMatix?.GenerationStatus(string.Empty, false);
            }

            if (SpliceAngles.Invalid)
            {
                aMatix?.GenerationStatus($"{action2} {tname} - Splice Angles", true);
                SpliceAngles.Clear();
                List<mdSpliceAngle> assyparts = mdPartGenerator.SpliceAngles_ASSY(assy, true, aCountMultiplier: tcount);
                if (assyparts.Count > 0)
                {
                    foreach (var part in assyparts)
                    {
                        part.AssociateToRange(rguid, true);
                        part.ProjectHandle = phandle;
                        SpliceAngles.Add(part);
                    }
                }
                SpliceAngles.Invalid = false;
                aMatix?.GenerationStatus(string.Empty, false);
            }

            if (ManwayAngles.Invalid)
            {
                    aMatix?.GenerationStatus($"{action3} {tname} - Manway Angles", true);
                    
                List<mdSpliceAngle> assyparts = mdPartGenerator.ManwayAngles_ASSY(assy, true, aCountMultiplier: tcount);
                if (assyparts.Count > 0)
                {
                    foreach (var part in assyparts)
                    {
                        part.AssociateToRange(rguid, true);
                        part.ProjectHandle = phandle;
                        ManwayAngles.Add(part);
                    }
                }
                ManwayAngles.Invalid = false;
                aMatix?.GenerationStatus(string.Empty, false);
            }
        }

        internal void InvalidateParts(Message_PartsInvalidated aMessage)
        {
            if (aMessage == null) return;
            if (! string.IsNullOrWhiteSpace(aMessage.RangeGUID) &&  string.Compare(aMessage.RangeGUID, RangeGUID, true) != 0) return;
            if (aMessage.InvalidateAll) 
            {
                Clear();
            }
            else
            {
                InvalidateParts(aMessage.PartTypes);
          
            }


        }
        internal int InvalidateParts(List<uppPartTypes> aTypes)
        {

            int _rVal = 0;
            Clear(true);
            if (aTypes != null)
            {


                if (aTypes.Count > 0)
                {
                    if (uopUtils.PartListContainsPartType(aTypes, new List<uppPartTypes> { uppPartTypes.Downcomer, uppPartTypes.Downcomers, uppPartTypes.EndAngle, uppPartTypes.DowncomerBox }))
                        EndAngles.Invalid = true;
                    if (uopUtils.PartListContainsPartType(aTypes, new List<uppPartTypes> { uppPartTypes.APPan, uppPartTypes.Downcomers, uppPartTypes.Downcomer }))
                        APPans.Invalid = true;
                    if (uopUtils.PartListContainsPartType(aTypes, new List<uppPartTypes> { uppPartTypes.Deflector, uppPartTypes.Baffle }))
                        DeflectorPlates.Invalid = DesignFamily.IsEcmdDesignFamily();
                    if (uopUtils.PartListContainsPartType(aTypes, new List<uppPartTypes> { uppPartTypes.Downcomer, uppPartTypes.Downcomers, uppPartTypes.Stiffener, uppPartTypes.SupplementalDeflector, uppPartTypes.DowncomerBox }))
                    {
                        Stiffeners.Invalid = true;
                        SupplementalDeflectors.Invalid = true;
                    }

                    if (uopUtils.PartListContainsPartType(aTypes, new List<uppPartTypes> { uppPartTypes.ManwayAngle, uppPartTypes.SpliceAngle, uppPartTypes.ManwaySplicePlate }))
                    {
                        ManwaySplicePlates.Invalid = true;
                        SpliceAngles.Invalid = true;
                        ManwayAngles.Invalid = true;
                    }

                    if (uopUtils.PartListContainsPartType(aTypes, new List<uppPartTypes> { uppPartTypes.Downcomer, uppPartTypes.Downcomers, uppPartTypes.EndSupport, uppPartTypes.EndPlate, uppPartTypes.DowncomerBox }))
                    {
                        EndPlates.Invalid = true;
                        EndSupports.Invalid = true;
                    }
                    if (uopUtils.PartListContainsPartType(aTypes, new List<uppPartTypes> { uppPartTypes.DeckSplice, uppPartTypes.DeckSplices, uppPartTypes.DeckSection, uppPartTypes.DeckSections }))
                    {
                        ManwaySplicePlates.Invalid = true;
                        SpliceAngles.Invalid = true;
                        ManwayAngles.Invalid = true;
                        DeckSections.Invalid = true;

                    }
                }
            }

            AltRingDeckSections.Invalid = DeckSections.Invalid;
            if (Boxes.Invalid) 
                EndPlates.Invalid = true;
         
            if (EndPlates.Invalid)
            {
                SupplementalDeflectors.Invalid = true;
                EndSupports.Invalid = true;
                DeflectorPlates.Invalid = true;
            }

                       if (EndAngles.Invalid) _rVal++;
            if (Stiffeners.Invalid) _rVal++;
            if (SpliceAngles.Invalid) _rVal++;
            if (ManwaySplicePlates.Invalid) _rVal++;
            if (ManwayAngles.Invalid) _rVal++;
            if (SupplementalDeflectors.Invalid) _rVal++;
            if (DeflectorPlates.Invalid) _rVal++;
            if (APPans.Invalid) _rVal++;
            if (EndPlates.Invalid) _rVal++;
            if (EndSupports.Invalid) _rVal++;
            if (Boxes.Invalid) _rVal++;
            if (DeckSections.Invalid) _rVal++;
            if (AltRingDeckSections.Invalid) _rVal++;

            //if (Stiffener> Stiffeners;
            //private uopPartList<mdSpliceAngle> ManwaySplicePlates;
            //private uopPartList<mdSpliceAngle> SpliceAngles;
            //private uopPartList<mdSpliceAngle> ManwayAngles;
            //private uopPartList<mdSupplementalDeflector> SupplementalDeflectors;
            //private uopPartList<mdBaffle> DeflectorPlates;
            //private uopPartList<mdAPPan> APPans;
            //private uopPartList<mdEndPlate> EndPlates;
            //private uopPartList<mdEndSupport> EndSupports;
            //private uopPartList<mdDowncomerBox> Boxes;
            //private uopPartList<mdDeckSection> DeckSections;
            //private uopPartList<mdDeckSection> AltRingDeckSections;
            return _rVal;
        }

        public List<uopPart> Item(uppPartTypes aPartType, bool bAltRing = false, mdPartMatrix aMatrix = null,bool bRegen = false)
        {
            switch (aPartType)
            {
                case uppPartTypes.Stiffener:
                    if (Stiffeners.Invalid) Update_Stiffeners(Range, null, aMatrix);
                      return Stiffeners.ToList();
                case uppPartTypes.EndAngle:
                    if (EndAngles.Invalid) Update_EndAngles(Range, null, aMatrix);
                    return EndAngles.ToList();
                case uppPartTypes.SpliceAngle:
                    if (SpliceAngles.Invalid) Update_SplicePlates(Range, null, aMatrix);
                    return SpliceAngles.ToList();
                case uppPartTypes.ManwayAngle:
                    if (SpliceAngles.Invalid) Update_SplicePlates(Range, null, aMatrix);
                    return ManwayAngles.ToList();
                case uppPartTypes.ManwaySplicePlate:
                    if (SpliceAngles.Invalid) Update_SplicePlates(Range, null, aMatrix);
                    return ManwaySplicePlates.ToList();
                case uppPartTypes.Baffle:
                case uppPartTypes.Deflector:
                    if (DeflectorPlates.Invalid) Update_Deflectors(Range, null, aMatrix);
                    return DeflectorPlates.ToList();
                case uppPartTypes.APPan:
                    if (APPans.Invalid) Update_APPans(Range, null, aMatrix);
                    return APPans.ToList();
                case uppPartTypes.EndPlate:
                    if (EndPlates.Invalid) Update_EndPlates(Range, null, aMatrix);
                    return EndPlates.ToList();
                case uppPartTypes.EndSupport:
                    if (EndSupports.Invalid) Update_EndSupports(Range, null, aMatrix);
                    return EndSupports.ToList();
                case uppPartTypes.Downcomer:
                case uppPartTypes.Downcomers:
                case uppPartTypes.DowncomerBox:
                    if (Boxes.Invalid) Update_Boxes(Range, null, aMatrix);
                    return Boxes.ToList();
                case uppPartTypes.DeckSections:
                case uppPartTypes.DeckSection:
                    
                    if (DeckSections.Invalid || AltRingDeckSections.Invalid || bRegen) Update_DeckSections(Range, null, aMatrix);
                    List<uopPart> _rVal = !bAltRing ? DeckSections.ToList() : AltRingDeckSections.ToList();
                    return _rVal;

                case uppPartTypes.SupplementalDeflector:
                    if (SupplementalDeflectors.Invalid) Update_SupplementalDeflectos(Range, null, aMatrix);
                    return SupplementalDeflectors.ToList();

                case uppPartTypes.TraySupportBeam:
                    if (SupportBeams.Invalid) Update_Beams(Range, null, aMatrix);
                    return SupportBeams.ToList();
                default:
                    return null;
            }
        }
      
        public bool TryGetParts(uppPartTypes aPartType, out List<uopPart> rParts)
        {
            
            rParts = Item(aPartType);
            if (rParts != null)
            {
                if (rParts.Count <= 0) rParts = null;
            }
            return rParts != null;

        }


        public bool TryGetEqualPart(uopPart aPart, out uopPart rPart)
        {
            rPart = null;
            if (aPart == null) return false;
            if (!TryGetParts(aPart.PartType, out List<uopPart> parts)) return false;
            rPart = parts.Find(x => x.IsEqual(aPart));
            return rPart != null;

        }
        #endregion Methods

    }
}
