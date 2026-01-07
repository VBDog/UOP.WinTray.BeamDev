using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UOP.DXFGraphics;
using UOP.DXFGraphics.Utilities;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Interfaces;
using UOP.WinTray.Projects.Parts;

using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;
using static System.Collections.Specialized.BitVector32;

namespace UOP.WinTray.Projects.Generators
{
    public class mdSectionGenerator
    {
        private static double _CurThk;
        private static double _CurGap;

        private static dxePolygon _SlotPolyline;
        private static colDXFVectors _TabVertices;
        private static colDXFEntities _TabBends;






        public static void UpdatePerimeters(List<mdDeckSection> aSections, mdTrayAssembly aAssy, List<uopDeckSplice> aSplices, bool bSuppressEvents = false)
        {


            if (aAssy == null || aSections == null) return;
            if (!bSuppressEvents) aAssy.RaiseStatusChangeEvent($"Updating {aAssy.TrayName()} Deck Section Perimiters");
            aSplices ??= aAssy.DeckSplices;
            foreach (mdDeckSection section in aSections)
                section.GeneratePerimeter(aAssy,true); //  mdSectionGenerator.CreateSectionPerimeter(section, aAssy, aSplices: aSplices, bUpdateSpliceFlangeLines: true, bSuppressEvents: bSuppressEvents);
        }
          
       
      

        /// <summary>
        /// returns true if the passed panel is held down by a finger clip
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aSection"></param>
        /// <param name="aFingerClipPoints"></param>
        /// <returns></returns>
        internal static bool SectionIsHeldDown(mdTrayAssembly aAssy, mdDeckSection aSection, uopVectors aFingerClipPoints)
        {
            bool _rVal = true;
            if (aSection == null) return _rVal;
            aAssy = aSection.GetMDTrayAssembly(aAssy);
            if (aAssy == null) return _rVal;

            if (aSection.IsHalfMoon || aSection.IsManway || aSection.LapsRing || aSection.LapsBeam) return _rVal;



            if (aSection.BaseVertices.Count <= 0) return _rVal;


            _rVal = false;

            double YTop = aSection.Limits.Top;
            double YBot = aSection.Limits.Bottom;
            uopVectors DCFCPts = aFingerClipPoints.GetByPartIndex(aSection.RightDowncomerIndex);
     


            for (int j = 1; j <= DCFCPts.Count; j++)
            {
                uopVector u1 = DCFCPts.Item(j);
                if (u1.Tag == "LEFT")
                {
                    if (u1.Y - 1.5 > YBot && u1.Y - 1.5 < YTop)
                    {
                        _rVal = true;
                        break;
                    }
                    if (u1.Y + 1.5 > YBot && u1.Y + 1.5 < YTop)
                    {
                        _rVal = true;
                        break;
                    }
                }
            }
            if (!_rVal)
            {
                DCFCPts = aFingerClipPoints.GetByPartIndex(aSection.LeftDowncomerIndex);

                for (int j = 1; j <= DCFCPts.Count; j++)
                {
                    uopVector u1 = DCFCPts.Item(j);
                    if (u1.Tag == "RIGHT")
                    {
                        if (u1.Y - 1.5 > YBot && u1.Y - 1.5 < YTop)
                        {
                            _rVal = true;
                            break;
                        }
                        if (u1.Y + 1.5 > YBot && u1.Y + 1.5 < YTop)
                        {
                            _rVal = true;
                            break;
                        }
                    }
                }
            }

            return _rVal;
        }




        /// <summary>
        /// generates the sections of the panel based on the splice centers
        /// if there are no splices then the panel is returned as a whole
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aPanels"></param>
        /// <param name="aSplices"></param>
        /// <param name="aCollector"></param>
        /// <returns></returns>
        public static List<mdDeckSection> GenerateDeckSections(mdTrayAssembly aAssy, colMDDeckPanels aPanels, uopDeckSplices aSplices, mdDeckSections aCollector = null, bool bSuppressEvents = false, bool bForAltRing = false)
        {

            List<mdDeckSection> _rVal = new List<mdDeckSection>();
            try
            {
                Reset();


                if (aAssy == null) return _rVal;
                if (aAssy.ProjectType != uppProjectTypes.MDDraw) return _rVal;
                aPanels ??= aAssy.DeckPanels;
                if (aPanels == null) return _rVal;
                bool altsplices = aSplices != null;
                
                UVECTORS aBPs = new UVECTORS(aAssy.DowncomerData.BPSites(aAssy, aSplices));

                if (!altsplices)  aSplices = aAssy.DeckSplices;
                 if (!bSuppressEvents) aAssy.RaiseStatusChangeEvent($"Verifying {aAssy.TrayName()} Splices");
                 mdSplicing.VerifySplices(aSplices, aAssy);

                if (!bSuppressEvents) aAssy.RaiseStatusChangeEvent($"Creating  {aAssy.TrayName()} Section Shapes");
                uopSectionShapes sectionShapes = aAssy.DowncomerData.CreateSectionShapes(aSplices,null, !altsplices,!bSuppressEvents,true) ;


                    List<mdDeckPanel> dps = aPanels.GetByVirtual(aVirtualValue: false);
                //if (aAssy.IsStandardDesign && aAssy.OddDowncomers)
                //{
                //    // special case for odd downcomers
                //    mdDeckPanel dp = aPanels.GetByVirtual(aVirtualValue: true).FirstOrDefault();
                //    if (dp != null)dps.Add(dp);

                //}
               
                
                foreach (mdDeckPanel dp in dps)
                {
                    
                    if (!bSuppressEvents) aAssy.RaiseStatusChangeEvent($"Creating {aAssy.TrayName()} Panel {dp.Index} Deck Sections");
                    GenerateSectionShapes(_rVal, aAssy, dp, aSplices, sectionShapes, bSuppressEvents);
                }

                //assign the bubble promoters
                if (aAssy.DesignOptions.HasBubblePromoters)
                {
                    if (!bSuppressEvents) aAssy.RaiseStatusChangeEvent($"Locating {aAssy.TrayName()}  Bubble Promoter Sites");
                   
                    //if (!aBPs.Suppressed)
                    //{
                    //    foreach (mdDeckSection section in _rVal)
                    //    {
                    //        section.BPSites = aBPs.GetByTag(section.Handle, false);
                    //    }
                    //}

                }
                else
                {
                    //aAssy.BPSites = UVECTORS.Zero;
                }


                UpdatePerimeters(_rVal, aAssy, aSplices, bSuppressEvents);

                if (aCollector != null) aCollector.Populate(_rVal);


            }
            finally
            {
                if (!bSuppressEvents) aAssy.RaiseStatusChangeEvent(string.Empty, string.Empty, bBegin: false);
                Reset();
            }



            return _rVal;
        }

   

     
        public static mdDeckSection FindEqualSection(mdDeckSection aSection, List<mdDeckSection> aSections, mdTrayAssembly aAssy, out bool rInverseEqual, bool bShapeCompare = false, bool bReturnParent = false, bool bIgnoreSlots = false)
        {
            mdDeckSection _rVal = null;
            rInverseEqual = false;
            if (aSection == null || aSections == null) return _rVal;

            foreach (mdDeckSection item in aSections)
            {


                if (mdSectionGenerator.SectionsCompare(aSection, item, aAssy, aAssy, false, out rInverseEqual, bShapeCompare, bIgnoreSlots))
                {
                    _rVal = item;
                    break;
                }
            }
            if (_rVal != null && bReturnParent)
            {
                _rVal = mzUtils.ThisOrThat(_rVal.Parent, _rVal);
            }
            return _rVal;


        }

        public static bool SectionsCompare(mdDeckSection aSection, mdDeckSection bSection, mdTrayAssembly aAssy, mdTrayAssembly bAssy, bool bCompareMaterial, out bool rInverseEqual, bool bShapeCompare = false, bool bIgnoreSlots = false)
        {
            rInverseEqual = false;
            if (aSection == null || bSection == null) return false;
            if (aSection.RequiresSlotting != bSection.RequiresSlotting) return false;
            if (aSection.IsHalfMoon != bSection.IsHalfMoon) return false;
            if (aSection.IsManway != bSection.IsManway) return false;
            if (aSection.LapsRing != bSection.LapsRing) return false;
            if (aSection.LapsBeam != bSection.LapsBeam) return false;

            if (aSection.LapsRing && aSection.X != 0 && !bShapeCompare)
            {
                if ((aSection.X > 0 & bSection.X > 0) || (aSection.X < 0 & bSection.X < 0)) return false;
                if ((aSection.Y > 0 & bSection.Y > 0) || (aSection.Y < 0 & bSection.Y < 0)) return false;
            }

            aAssy ??= aSection.GetMDTrayAssembly(aAssy);
            if (aAssy == null) return false;

            bAssy ??= bSection.GetMDTrayAssembly(bAssy);
            if (bAssy == null) return false;
            if (aAssy != bAssy)
            {
                if (aAssy.DesignFamily != bAssy.DesignFamily) return false;
                if (bCompareMaterial)
                {
                    if (!aSection.Material.IsEqual(bSection.Material)) return false;
                }

                if (aAssy.SpliceStyle != bAssy.SpliceStyle) return false;
            }

            aSection.GeneratePerimeter(aAssy);
            bSection.GeneratePerimeter(bAssy);

            if (!bShapeCompare)
            {
                if (aSection.TruncatedFlangeLine(null)) return false;
                if (aSection.TruncatedFlangeLine(null)) return false;

            }

            bool SlotAndTab = aAssy.SpliceStyle == uppSpliceStyles.Tabs;
            string aTpslc = aSection.TopSpliceTypeName();
            string aBsplc = aSection.BottomSpliceTypeName();
            string bTpslc = bSection.TopSpliceTypeName();
            string bBsplc = bSection.BottomSpliceTypeName();
            bool bInv = false;
            if (SlotAndTab)
            {
                if (aSection.IsManway && aAssy.RangeGUID == bAssy.RangeGUID)
                {
                    rInverseEqual = string.Compare(aTpslc, bTpslc, true) != 0;
                    return true;
                }



                //compare the splice names. if the inverse combo is equal then we compare the shapes inverted
                if (!uopUtils.CompareStrings(aTpslc, aBsplc, bTpslc, bBsplc, out bInv)) return false;

            }

            //special case
            if (!aSection.SplicedOnTop && !aSection.SplicedOnBottom && !bSection.SplicedOnTop && !bSection.SplicedOnBottom && !aSection.IsHalfMoon)
            {
                if ((aSection.X < 0 && bSection.X > 0) || (aSection.X > 0 && bSection.X < 0)) bInv = true;
            }

            //if the gross boundary dimensions don't match the sections cannot be the same
            if (!aSection.Limits.CompareDimensions(bSection.Limits, aPrecis: 3)) return false;

            if (SlotAndTab)
            {
                List<uopFlangeLine> aflngLns = aSection.FlangeLines();
                List<uopFlangeLine> bflngLns = bSection.FlangeLines();
                if (aflngLns.Count != bflngLns.Count)
                    return false;
                foreach (uopFlangeLine fline in aflngLns)
                {

                    if (bflngLns.FindIndex((x) => Math.Round(x.Length, 3) == Math.Round(fline.Length, 3)) < 0)
                        return false;

                }

            }

            bool bECMD = aAssy.DesignFamily.IsEcmdDesignFamily();
            bool _rVal = false;
            uopVectors aPts = uopVectors.Zero;
            uopVectors bPts = uopVectors.Zero;

            if (aSection.IsManway)
            {
                if (bECMD)
                {
                    //just compare the slotting
                    mdSlotZone aSlotZone = aSection.SlotZone(aAssy);
                    mdSlotZone bSlotZone = aSection.SlotZone(bAssy);
                    if (aSlotZone != null) aPts = aSlotZone.GridPts.GetBySuppressed(false, true);
                    if (bSlotZone != null) bPts = bSlotZone.GridPts.GetBySuppressed(false,true);
                    _rVal = uopVectors.MatchPlanar(aPts, bPts, aPrecis: 2);

                }
            }
            else
            {
                //compare the perimter points 
                aPts = aSection.PerimeterPts(bIncludeBPSites: true, bIncludeSlotSites: false);
                bPts = bSection.PerimeterPts(bIncludeBPSites: true, bIncludeSlotSites: false);
                _rVal = uopVectors.MatchPlanar(aPts, bPts, aPrecis: 2);
                if (_rVal && bECMD && !bIgnoreSlots)
                {
                    mdSlotZone aZone = aSection.SlotZone(aAssy);
                    mdSlotZone bZone = bSection.SlotZone(bAssy);
                    if (aZone == null || bZone == null)
                        return false;

                    uopArcRecs aIslands = aZone.Islands;
                    uopArcRecs bIslands = bZone.Islands;


                    if (aZone.PitchType != bZone.PitchType || Math.Round(aZone.HPitch - bZone.HPitch, 3) != 0 || Math.Round(aZone.VPitch - bZone.VPitch, 3) != 0 || aIslands.Count != bIslands.Count)
                        return false;
                    //_rVal = uopVectors.MatchPlanar(aZone.GridPts, bZone.GridPts);

                    _rVal = uopVectors.MatchPlanar(aZone.GridPoints(false), bZone.GridPoints(false),  2);


                }
            }

            rInverseEqual = false;
            if (_rVal)
            {
                rInverseEqual = string.Compare(aTpslc, bTpslc, true) != 0;
                //special case
                if (!aSection.SplicedOnTop && !aSection.SplicedOnBottom && !bSection.SplicedOnTop && !bSection.SplicedOnBottom && !aSection.IsHalfMoon)
                {
                    if ((aSection.X < 0 && bSection.X > 0) || (aSection.X > 0 && bSection.X < 0)) rInverseEqual = true;
                }
                else if (aSection.SupportsManway(out bool ontop1) && bSection.SupportsManway(out bool ontop2))
                {
                    rInverseEqual = ontop1 != ontop2;
                }
            }

            // _rVal = UVECTORS.CompareSet(aPts, bPts, false, bInv, false, false);
            //if (!_rVal && !aSection.IsManway)
            //{
            //    if (aTpslc == aBsplc)
            //    {
            //        bInv = !bInv;
            //        _rVal = UVECTORS.CompareSet(aPts, bPts, false, bInv, false, false);
            //    }
            //}
            //rInverseEqual = bInv && _rVal;
            return _rVal;
        }

        /// <summary>
        /// creates the subset of the assemblies deck sections reduces to only the inuque members with their instance defines
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aSplices"></param>
        /// <param name="aSections"></param>
        /// <returns></returns>
        public static List<mdDeckSection> UniqueSections(mdTrayAssembly aAssy, List<uopDeckSplice> aSplices, List<mdDeckSection> aSections)
        {


            if (aAssy == null || aSplices == null || aSections == null) return new List<mdDeckSection>();
            if (aSections.Count <= 0) return new List<mdDeckSection>();
            // to be sure the bp centers are defined
            UVECTORS bpsites = new UVECTORS( aAssy.BPSites);

            List<mdDeckSection> _rVal = new List<mdDeckSection>();
            uopVector ctr1;
            uopVector ctr2;
            int cnt = mdDeckSections.GetCount(aSections, out string _, out int pcnt);
            bool invEq;
            List<int> pCnts = new List<int> { };
            uopVector displacment;
            bool symmetric = aAssy.IsSymmetric;
            bool specialcase = symmetric && aAssy.OddDowncomers && aSections.FindAll((x) => x.IsManway).Count > 0;
            int traycount = aAssy.TrayCount;
            bool bSlots = aAssy.DesignFamily.IsEcmdDesignFamily();
            List<double> skipX = new List<double>();
            List<int> manwaypanels = new List<int>();

            try
            {
                //inititialize member instances
                foreach (mdDeckSection section in aSections)
                {
                    section.PartNumberIndex = 0;
                    section.PartNumber = string.Empty;
                    section.UniqueIndex = 0;
                    section.Instances.Clear();
                    if (specialcase)
                    {
                        if (section.PanelIndex == pcnt || section.PanelIndex == pcnt - 1)
                        {
                            section.PanelOccuranceFactor = 1;
                        }


                    }
                    section.OccuranceFactor = 1;
                    section.Quantity = 1;
                    if (section.PanelIndex > pCnts.Count) pCnts.Add(0);

                }


              
                List<int> skippanels = new List<int>();
                if (specialcase)
                {

                    skippanels.Add(pcnt);
                    skippanels.Add(pcnt - 1);
                    skipX.Add(aAssy.DeckPanels.Item(pcnt).X);
                }

                //look for matches on the return members
                foreach (mdDeckSection section in aSections)
                {

                    bool ignoreslots = specialcase && section.PanelIndex == pcnt;

                    // find a unique member that is Equal to this one
                    invEq = false;
                    mdDeckSection match = mdSectionGenerator.FindEqualSection(section, _rVal, aAssy, out invEq, bIgnoreSlots: ignoreslots); //: null; // _rVal.Find(x => x.CompareTo(section) == true): null;
                    
                    if (match == null)
                    {
                        //there is no match so keep this section
                        int pCnt = pCnts[section.PanelIndex - 1] + 1;
                        section.PartNumberIndex = (100 * section.PanelIndex) + pCnt;
                        section.UniqueIndex = _rVal.Count + 1;
                        pCnts[section.PanelIndex - 1] = pCnt;
                        section.OccuranceFactor = 1;
                        section.Quantity = section.OccuranceFactor * traycount;
                        _rVal.Add(section);
                        if (section.IsManway)
                        {
                            if (!manwaypanels.Contains(section.PanelIndex)) manwaypanels.Add(section.PanelIndex);
                        }

                    }
                    else
                    {


                        //just add an instance to the matching unique section

                        ctr1 = new uopVector(match.X, match.Y);
                        ctr2 = new uopVector(section.X, section.Y);
                        displacment = ctr2 - ctr1;

                        section.PartNumberIndex = match.PartNumberIndex;
                        section.UniqueIndex = match.UniqueIndex;
                        match.Instances.Add(displacment, invEq ? 180 : 0, aPartIndex: section.PanelIndex);
                        match.OccuranceFactor = match.Instances.Count + 1;
                        match.Quantity = match.OccuranceFactor * traycount;

                        //set the parent of the base section to the matcher
                        section.AssociateToParent(match);
                    }

                }


                // add instances for the other side if the assembly is symmetric 
                if (aAssy.IsStandardDesign)
                {
                    foreach (mdDeckSection unique in _rVal)
                    {

                        if (!skippanels.Contains(unique.PanelIndex))
                        {


                            if (unique.PanelOccuranceFactor > 1)
                            {
                                ctr1 = new uopVector(unique.X, unique.Y);

                                colDXFVectors ipts = unique.Instances.MemberPointsDXF();
                                foreach (dxfVector ipt in ipts)
                                {
                                    if (Math.Round(ipt.X, 1) > 0)
                                    {
                                        ctr2 = new uopVector(-ipt.X, -ipt.Y);
                                        if (!skipX.Contains(ctr2.X))
                                        {
                                            displacment = ctr2 - ctr1;
                                            invEq = (ctr1.Y > 0 && ctr2.Y < 0) || (ctr1.Y < 0 && ctr2.Y > 0) || unique.IsHalfMoon || (ipts.Count == 1 && ctr1.X != 0);
                                            unique.Instances.Add(displacment, invEq ? 180 : 0, aPartIndex: uopUtils.OpposingIndex(unique.PanelIndex, aAssy.DeckPanels.Count));
                                        }
                                    }

                                }
                            }
                        }


                        //set the number of times the unique section occurs on a single tray
                        unique.OccuranceFactor = unique.Instances.Count + 1;
                        //set the number of times the unique section occurs in the entire tray range
                        unique.Quantity = unique.OccuranceFactor * traycount;
                    }

                }


            }

            catch
            {
                return _rVal;
            }
            finally
            {
                List<uopPart> rparts = _rVal.OfType<uopPart>().ToList();
                uopParts.SortPartsByPartNumber(rparts);
                _rVal = rparts.OfType<mdDeckSection>().ToList();
            }




            return _rVal;
        }

        /// <summary>
        /// creates the subset of the assemblies deck sections reduces to only the inuque members with their instance defines
        /// </summary>
        /// <param name="aAssy"></param>

        /// <returns></returns>
        public static mdDeckSections UniqueDeckSections(mdTrayAssembly aAssy, out List<mdDeckSection> rAltRingSections, mdDeckSections aCollector = null)
        {
            rAltRingSections = new List<mdDeckSection>();
            if (aAssy == null) return new mdDeckSections();

            uopDeckSplices splices = aAssy.DeckSplices;
            List<mdDeckSection> sections = aAssy.DeckSections.ToList();
            List<mdDeckSection> uniques = UniqueSections(aAssy, splices, sections);
            mdDeckSections _rVal;
            if (aCollector == null)
            {
                _rVal = new mdDeckSections(uniques);
            }
            else
            {
                _rVal = aCollector;
                _rVal.Populate(uniques);
            }



            if (aAssy.HasAlternateDeckParts)
            {
                rAltRingSections = UniqueAltRingSections(aAssy, splices, uniques);
            }
            else { rAltRingSections = _rVal.ToList(); }





            return _rVal;
        }

        public static List<mdDeckSection> UniqueAltRingSections(mdTrayAssembly aAssy, uopDeckSplices aSplices, List<mdDeckSection> aUniqueSections)
        {

            if (aAssy == null || aUniqueSections == null || aSplices == null) return new List<mdDeckSection>();

            bool symmetric = aAssy.IsSymmetric;
            bool specialcase = symmetric && aAssy.OddDowncomers;

            List<mdDeckSection> _rVal = new List<mdDeckSection>();
            List<mdDeckSection> uniques = aUniqueSections.ToList();
            if (!aAssy.HasAlternateDeckParts) return null;



            List<mdDeckSection> assysections = aAssy.DeckSections;
            aSplices ??= aAssy.DeckSplices;

          
            List<int> manpanels = new List<int>(); // aAssy.DeckSections.ManwayPanelIndexes();
            List<int> panelIDS = aAssy.DeckSections.PanelIndexes(out List<int> panelsectioncounts);

            uopDeckSplices panelsplices = new uopDeckSplices(aAssy);
            List<int> maxpartidxs = new List<int>();
            uopRingRange basealtrange = aAssy.RingRange.FirstAlternatingRange;
            int basealttraycount = basealtrange.RingCount;

            uopRingRange altrange = aAssy.RingRange.SecondAlternatingRange;
            int alttraycount = altrange.RingCount;
            //List<int> manwaypanels = new List<int>();
            try
            {

                // get the max part number indices per panel so we can assign part numbers to alternate ring sections that are unique to the alternate ring 
                foreach (int pid in panelIDS)
                {
                    List<mdDeckSection> panelsecs = aUniqueSections.FindAll((x) => x.PanelIndex == pid);
                    int maxpn = 0;
                    foreach (mdDeckSection panelsec in panelsecs)
                    {
                        if (panelsec.PartNumberIndex > maxpn) maxpn = panelsec.PartNumberIndex;
                        if (panelsec.IsManway)
                        {
                            if (!manpanels.Contains(pid)) manpanels.Add(pid);
                        }

                    }
                    maxpartidxs.Add(maxpn);

                    //create the deck splices by mirror the splices that lie on the panels that have manways   
                    List<uopDeckSplice> psplices = aSplices.FindAll((x) => x.PanelIndex == pid);
                    if (manpanels.Contains(pid))
                    {
                        foreach (uopDeckSplice item in psplices)
                        {
                            item.Mirror(null, item.PanelY);
                        }
                        psplices.Reverse();


                    }
                    panelsplices.AddRange(psplices);
                }

                //create the alternate ring deck sections based on the the new splices collection
                List<mdDeckSection> altsecs = mdSectionGenerator.GenerateDeckSections(aAssy, null, panelsplices, bSuppressEvents: true, bForAltRing: true);

                //deal with slot zones

                bool ecmd = aAssy.IsECMD;
                if (ecmd)
                {
                    mdSlotZones basezones = aAssy.SlotZones;
              

                    //create the new slot zones based ont new splices collection
                    mdSlotZones altzones = mdSlotting.SlotZones_Create(aAssy, altsecs, basezones);
                    mdSlotting.SetSlotCounts(aAssy, altzones, aAssy.FunctionalActiveArea);

                    foreach (int pid in panelIDS)
                    {
                        bool inverted = manpanels.Contains(pid);
                        int sectioncount = panelsectioncounts[pid - 1];

                        //List<mdDeckSection> basesections = assysections.FindAll((x) => x.PanelIndex == pid);
                        List<mdDeckSection> altsections = altsecs.FindAll((x) => x.PanelIndex == pid);  // these are in reverse order from the prinaer ring arangement
                                                                                                        //copy the pitches from the assembly's
                                                                                                        //slot zones to the new slot zones

                        foreach (mdDeckSection altsection in altsections)
                        {
                            //find the corresponding base slot zone from the primary slot zones by handle and copy the slot layout info
                            string althandle = inverted ? $"{pid},{uopUtils.OpposingIndex(altsection.SectionIndex, sectioncount)}" : altsection.Handle;
                            mdSlotZone altzone = altzones.Find((x) => x.SectionHandle == altsection.Handle);
                            mdSlotZone basezone = basezones.Find((x) => x.SectionHandle == althandle);
                            altzone.DeckSection = altsection;
                            if (basezone != null)
                            {
                                altzone.HPitch = basezone.HPitch;
                                altzone.VPitch = basezone.VPitch;
                            }
                            altsection.AltSlotZone = altzone;
                            altzone.UpdateSlotPoints(aAssy, bForceRegen: true);

                        }

                    }
                }


                //generate the alternate ring unique deck section from the rearanged splices 
                List<mdDeckSection> altuniques = mdSectionGenerator.UniqueSections(aAssy, panelsplices, altsecs);

                foreach (mdDeckSection altsection in altuniques)
                {
                    int pid = altsection.PanelIndex;

                    bool inverted = manpanels.Contains(pid);
                    int sectioncount = panelsectioncounts[pid - 1];



                    string althandle = inverted ? $"{pid},{uopUtils.OpposingIndex(altsection.SectionIndex, sectioncount)}" : altsection.Handle;
                    mdDeckSection assyunique = aUniqueSections.Find((x) => x.Handle == althandle);


                    List<mdDeckSection> searchin = aUniqueSections.FindAll((x) => x.LapsRing == altsection.LapsRing && x.IsManway == altsection.IsManway && x.LapsBeam == altsection.LapsBeam);

                    if (altsection.LapsRing || altsection.TruncatedFlange)
                        searchin = searchin.FindAll((x) => x.Y <= 0 == altsection.Y <= 0);

                    bool ignoreslots = specialcase && pid == panelsectioncounts.Count;

                    mdDeckSection match = mdSectionGenerator.FindEqualSection(altsection, searchin, aAssy, out bool invEq, bShapeCompare: true, bReturnParent: false, bIgnoreSlots: ignoreslots);

                    if (match == null)
                    {

                        // this section is unique to the alternate ring assembly
                        maxpartidxs[pid - 1]++;
                        altsection.PartNumberIndex = maxpartidxs[pid - 1];
                        altsection.RingRange = altrange;
                        altsection.Quantity = altsection.OccuranceFactor * alttraycount;
                        altsection.AlternateRingType = uppAlternateRingTypes.AtlernateRing2;

                        if (assyunique != null)
                        {
                            assyunique.RingRange = basealtrange;
                            assyunique.Quantity = altsection.OccuranceFactor * basealttraycount;
                            assyunique.AlternateRingType = uppAlternateRingTypes.AtlernateRing1;

                        }
                    }
                    else
                    {
                        altsection.Inverted = inverted && Math.Round(altsection.Y, 2) != Math.Round(match.Y, 2);
                        altsection.PartNumberIndex = match.PartNumberIndex;
                        altsection.Parent = match;
                    }
                    _rVal.Add(altsection);
                }

                //return _rVal;
            }

            catch
            {
                _rVal = null;
            }
            finally
            {
                //List<uopPart> rparts = _rVal.OfType<uopPart>().ToList();
                //uopParts.SortPartsByPartNumber(rparts);
                //_rVal = rparts.OfType<mdDeckSection>().ToList();
            }




            return _rVal;
        }


   
        public static List<mdDeckSection> GenerateSectionShapes(List<mdDeckSection> aCollector, mdTrayAssembly aAssy, mdDeckPanel aPanel, uopDeckSplices aSplices, uopSectionShapes aBaseShapes, bool bSuppressEvents = false)
        {

            List<mdDeckSection> _rVal = aCollector ?? new List<mdDeckSection>();

            if (aPanel == null || aSplices == null) return _rVal;
            int pid = aPanel.Index;

            aBaseShapes ??= aAssy.DowncomerData.CreateSectionShapes(aSplices);
            //List<uopDeckSplice> mySplices = aSplices.FindAll((x) => x.PanelIndex == pid);
            //mySplices = mdSplicing.SortSet(mySplices, false);
          
            URECTANGLE mechanicalLims = URECTANGLE.Null;
            URECTANGLE splLims;
            var panelShapes = mdDeckPanel.GetPanelShapes(aPanel, aAssy);
            uopPanelSectionShape panelshape = panelShapes.Item(1).ParentShape;

            DowncomerInfo aLeftDC = panelshape.LeftDowncomerInfo;
            DowncomerInfo aRightDC = panelshape.RightDowncomerInfo;

            double? lDCX = null; if( aLeftDC != null ) lDCX = aLeftDC.X_Outside_Right + aLeftDC.ShelfWidth;
            double? rDCX = null; if( aRightDC != null ) rDCX = aRightDC.X - aRightDC.ShelfWidth ;

            double jht = aAssy != null ? aAssy.DesignOptions.JoggleAngle : 1.5;
            double thk = aAssy != null ? aAssy.Deck.Thickness : 0;
            double gap = aAssy != null ? aAssy.PanelClearance(pid > 1) : 0;
            
       
            List<uopSectionShape> pshapes = aBaseShapes.FindAll(x => x.PanelIndex == pid);
           
            foreach (uopSectionShape secshape in pshapes)
            {

                secshape.RectifySplices(out uopDeckSplice topSplice, out uopDeckSplice bottomSplice);

                mdDeckSection newSection = new mdDeckSection(secshape,aPanel);
                newSection.SubPart(aAssy);
          
                
            
                mechanicalLims = new URECTANGLE { Left = secshape.Left, Right = secshape.Right };
                if (lDCX.HasValue && lDCX.Value > newSection.Limits.Left) mechanicalLims.Left = lDCX.Value;
                if (rDCX.HasValue && rDCX.Value < newSection.Limits.Right) mechanicalLims.Right = rDCX.Value;

                if (secshape.IsHalfMoon)
                {
                    if (secshape.TopSplice != null)
                    {
                        mechanicalLims.Right = secshape.TopSplice.Ordinate;
                        if (secshape.TopSplice.SpliceType == uppSpliceTypes.SpliceWithJoggle)
                        {
                            splLims = secshape.TopSplice.Limits();
                            mechanicalLims.Right = splLims.Right;
                            if (!topSplice.Female) newSection.Depth = jht;

                        }
                        else if (secshape.TopSplice.SpliceType == uppSpliceTypes.SpliceWithTabs)
                        {
                            if (secshape.TopSplice.Female) newSection.Depth = jht;
                        }
                    }

                    if (bottomSplice != null)
                    {
                        mechanicalLims.Left = bottomSplice.Ordinate;

                        if (bottomSplice.SpliceType == uppSpliceTypes.SpliceWithJoggle)
                        {
                            splLims = bottomSplice.Limits();
                            mechanicalLims.Left = splLims.Right;
                            if (!bottomSplice.Female) newSection.Depth = jht;
                        }
                        else if (bottomSplice.SpliceType == uppSpliceTypes.SpliceWithTabs)
                        {
                            if (bottomSplice.Female) newSection.Depth = jht;
                        }
                    }
                }
                else
                {
                    mechanicalLims.Top = newSection.RingRadius;
                    mechanicalLims.Bottom = -newSection.RingRadius;
                    if (topSplice != null)
                    {
                        mechanicalLims.Top = topSplice.Ordinate;

                        if (topSplice.SpliceType == uppSpliceTypes.SpliceWithJoggle)
                        {
                            splLims = topSplice.Limits();
                            mechanicalLims.Top = (!topSplice.Female) ? splLims.Bottom : splLims.Top;
                            if (!topSplice.Female) newSection.Depth = jht;
                        }
                        else if (topSplice.SpliceType == uppSpliceTypes.SpliceWithTabs)
                        {
                            if (topSplice.Female) newSection.Depth = jht;
                        }
                    }

                    if (bottomSplice != null)
                    {
                        mechanicalLims.Bottom = bottomSplice.Ordinate;

                        if (bottomSplice.SpliceType == uppSpliceTypes.SpliceWithJoggle)
                        {
                            splLims = bottomSplice.Limits();
                            mechanicalLims.Bottom = (!bottomSplice.Female) ? splLims.Top : splLims.Bottom;

                            if (!bottomSplice.Female) newSection.Depth = jht;

                        }
                        else if (bottomSplice.SpliceType == uppSpliceTypes.SpliceWithTabs)
                        {
                            if (bottomSplice.Female) newSection.Depth = jht;

                        }
                    }

                    
                }

                //newSection.MechanicalLimits = mechanicalLims;
                //newSection.PanelOccuranceFactor = aPanel.OccuranceFactor;
              
              //  xSectionBound(newSection, aAssy, aPanel, secshape.TopSplice, secshape.BottomSplice, aLeftDC, aRightDC, gap, thk, jht);

                _rVal.Add(newSection);

            }


            ////// Here, for each shape, we create a list of splices whose ordinate are between the min and max Y of the shape. Means, they cut the shape.
            ////// Our assumption here is that no two shapes on the same panel will share a splice. This may happen in two beam scenario.
            ////// There will be a validation check to make sure this will not happen.
            ////Dictionary<uopShape, List<uopDeckSplice>> shapeDeckSpliceMap = new Dictionary<uopShape, List<uopDeckSplice>>();
            ////int currentDeckSpliceIndex = 0;
            ////double panelShapeMinY, panelShapeMaxY;
            ////double panelShapeMinX, panelShapeMaxX;
            ////uopDeckSplice currentDeckSplice;
            ////foreach (var panelShape in panelShapes)
            ////{
            ////    shapeDeckSpliceMap.Add(panelShape, new List<uopDeckSplice>());

            ////    (panelShapeMinY, panelShapeMaxY) = mdDeckPanel.GetPanelShapeMinMaxY(panelShape);
            ////    (panelShapeMinX, panelShapeMaxX) = mdDeckPanel.GetPanelShapeMinMaxX(panelShape);

            ////    while (mySplices.Count > 0 && currentDeckSpliceIndex < mySplices.Count)
            ////    {
            ////        currentDeckSplice = mySplices[currentDeckSpliceIndex];
            ////        if (currentDeckSplice.Vertical)
            ////        {
            ////            if (currentDeckSplice.Ordinate <= panelShapeMinX || currentDeckSplice.Ordinate >= panelShapeMaxX)
            ////            {
            ////                break;
            ////            }
            ////        }
            ////        else
            ////        {
            ////            if (currentDeckSplice.Ordinate <= panelShapeMinY || currentDeckSplice.Ordinate >= panelShapeMaxY)
            ////            {
            ////                break;
            ////            }
            ////        }

            ////        shapeDeckSpliceMap[panelShape].Add(currentDeckSplice);
            ////        currentDeckSpliceIndex++;
            ////    }
            ////}

            //// This part of the code assigns each shape a list of two splices (which may be null).
            //// Each pair of splices define the top and/or bottom splices of a deck section contained in the shape.
            //// The name "arguments" is used because these will be used as the argument for calling the method which is responsible for creating the deck sections.
            //Dictionary<uopShape, List<(uopDeckSplice, uopDeckSplice)>> arguments = new Dictionary<uopShape, List<(uopDeckSplice, uopDeckSplice)>>();
            //uopDeckSplice topSplice = null, bottomSplice = null;
            //foreach (var kv in shapeDeckSpliceMap)
            //{
            //    currentDeckSpliceIndex = -1;

            //    arguments.Add(kv.Key, new List<(uopDeckSplice, uopDeckSplice)>());

            //    do
            //    {
            //        currentDeckSpliceIndex++;

            //        currentDeckSplice = currentDeckSpliceIndex < kv.Value.Count ? kv.Value[currentDeckSpliceIndex] : null;

            //        topSplice = bottomSplice;
            //        bottomSplice = currentDeckSplice;

            //        arguments[kv.Key].Add((topSplice, bottomSplice));
            //    }
            //    while (currentDeckSpliceIndex < kv.Value.Count);
            //}

            //if (aPanel.IsCenter) lDCX = -rDCX;

            //// This part of the code goes over the arguments to create all the deck sections.
            //// Note that each argument has a shape and all the possible deck sections within it.
            //int panelSectionCount = arguments.Sum(kv => kv.Value.Count);
            //mdDeckSection newSection;
            //int newSectionIndex = 1;
            //foreach (var argument in arguments)
            //{
            //    foreach (var splicePair in argument.Value)
            //    {
            //        (topSplice, bottomSplice) = splicePair;
            //        newSection = aPanel.BasicSection(argument.Key, aAssy, topSplice, bottomSplice);
            //        newSection.SectionIndex = newSectionIndex++;
            //        newSection.PanelSectionCount = panelSectionCount;
            //        newSection.SingleSection = arguments.Count > 1 || argument.Value.Count > 1;

            //        newSection.PanelShapeIndex = mdDeckPanel.ExtractPanelShapeIndexFromPanelShapeName(argument.Key.Name);

            //        mechanicalLims = new URECTANGLE { Left = newSection.Limits.Left, Right = newSection.Limits.Right };
            //        if (lDCX != 0 && lDCX > newSection.Limits.Left) mechanicalLims.Left = lDCX;
            //        if (rDCX != 0 && rDCX < newSection.Limits.Right) mechanicalLims.Right = rDCX;

            //        if (aPanel.IsHalfMoon)
            //        {
            //            if (topSplice != null)
            //            {
            //                mechanicalLims.Right = topSplice.Ordinate;
            //                if (topSplice.SpliceType == uppSpliceTypes.SpliceWithJoggle)
            //                {
            //                    splLims = topSplice.Limits();
            //                    mechanicalLims.Right = splLims.Right;
            //                    if (!topSplice.Female) newSection.Depth = jht;

            //                }
            //                else if (topSplice.SpliceType == uppSpliceTypes.SpliceWithTabs)
            //                {
            //                    if (topSplice.Female) newSection.Depth = jht;
            //                }
            //            }

            //            if (bottomSplice != null)
            //            {
            //                mechanicalLims.Left = bottomSplice.Ordinate;

            //                if (bottomSplice.SpliceType == uppSpliceTypes.SpliceWithJoggle)
            //                {
            //                    splLims = bottomSplice.Limits();
            //                    mechanicalLims.Left = splLims.Right;
            //                    if (!bottomSplice.Female) newSection.Depth = jht;
            //                }
            //                else if (bottomSplice.SpliceType == uppSpliceTypes.SpliceWithTabs)
            //                {
            //                    if (bottomSplice.Female) newSection.Depth = jht;
            //                }
            //            }
            //        }
            //        else
            //        {
            //            mechanicalLims.Top = newSection.RingRadius;
            //            mechanicalLims.Bottom = -newSection.RingRadius;
            //            if (topSplice != null)
            //            {
            //                mechanicalLims.Top = topSplice.Ordinate;

            //                if (topSplice.SpliceType == uppSpliceTypes.SpliceWithJoggle)
            //                {
            //                    splLims = topSplice.Limits();
            //                    mechanicalLims.Top = (!topSplice.Female) ? splLims.Bottom : splLims.Top;
            //                    if (!topSplice.Female) newSection.Depth = jht;
            //                }
            //                else if (topSplice.SpliceType == uppSpliceTypes.SpliceWithTabs)
            //                {
            //                    if (topSplice.Female) newSection.Depth = jht;
            //                }
            //            }

            //            if (bottomSplice != null)
            //            {
            //                mechanicalLims.Bottom = bottomSplice.Ordinate;

            //                if (bottomSplice.SpliceType == uppSpliceTypes.SpliceWithJoggle)
            //                {
            //                    splLims = bottomSplice.Limits();
            //                    mechanicalLims.Bottom = (!bottomSplice.Female) ? splLims.Top : splLims.Bottom;

            //                    if (!bottomSplice.Female) newSection.Depth = jht;

            //                }
            //                else if (bottomSplice.SpliceType == uppSpliceTypes.SpliceWithTabs)
            //                {
            //                    if (bottomSplice.Female) newSection.Depth = jht;

            //                }
            //            }

            //            newSection.TabDirection = (Math.Round(newSection.Y, 1) >= 0) ? dxxOrthoDirections.Down : dxxOrthoDirections.Up;
            //        }

            //        newSection.MechanicalLimits = mechanicalLims;
            //        newSection.PanelOccuranceFactor = aPanel.OccuranceFactor;

            //        xSectionBound(newSection, aAssy, aPanel, topSplice, bottomSplice, aLeftDC, aRightDC, gap, thk, jht);

            //        _rVal.Add(newSection);
            //    }
            //}

            return _rVal;
        }

  
        internal static USHAPE ManholeBound(mdDeckSection aSection)
        { return ManholeBound(aSection, out double WD, out double HT); }

        internal static USHAPE ManholeBound(mdDeckSection aSection, out double rWidth, out double rHeight)
        {
            USHAPE _rVal = aSection.PhysicalBoundaryV;
            rWidth = _rVal.Width;
            rHeight = _rVal.Height;
            if ((!aSection.LapsRing && !aSection.LapsBeam) || aSection.PanelIndex <= 1) return _rVal;

            if (aSection.SingleSection || aSection.IsCenterSection) return _rVal;
            dxePolyline aPl = _rVal.ToDXFPolyline();

            dxeArc aArc = (dxeArc)aPl.Segments.GetArcs(aSection.Radius).FirstOrDefault();
            if (aArc == null) return new USHAPE("");


            dxfVector v1 = aArc.MidPt;
            dxfPlane aPln = new dxfPlane(aArc.MidPt);
            aPln.AlignYTo(aPln.Origin.DirectionTo(dxfVector.Zero));
            colDXFVectors aPts = new colDXFVectors( aSection.ExtentPoints);
            aPts.Add(v1);

            dxfRectangle aRec = aPts.BoundingRectangle(aPln);
            double wd = aRec.Width;
            double ht = aRec.Height;
            mzUtils.SortTwoValues(true, ref wd, ref ht);
            if (wd < _rVal.Width && wd < _rVal.Height)
            {
                rWidth = aRec.Width;
                rHeight = aRec.Height;
                _rVal = USHAPE.FromDXFPolyline(aRec.Perimeter());
            }
            return _rVal;
        }

        /// <summary>
        /// the sections of the panel based on the splice centers
        /// if there are no splices then the panel is returned as a whole
        /// </summary>
        /// <param name="aAssy">the assy that owns the panel</param>
        /// <param name="aPanel">the panel to create sections for</param>
        /// <param name="aTopSplice">the splice centers to use in the calcs other that the panels current set of splices</param>
        /// <param name="aBotSplice"></param>
        /// <param name="aLeftDC"></param>
        /// <param name="aRightDC"></param>
        /// <param name="aGap"></param>
        /// <param name="aThickness"></param>
        /// <param name="aJoggleHt"></param>
        /// <returns></returns>
        private static USHAPE xSectionShape(mdTrayAssembly aAssy, mdDeckPanel aPanel, uopDeckSplice aTopSplice, uopDeckSplice aBotSplice, mdDowncomer aLeftDC, mdDowncomer aRightDC = null, double aGap = 0, double aThickness = 0, double aJoggleHt = 0, int? panelShapeIndex = null)
        {

            //URECTANGLE splLims;
            //USHAPE aShp;
            //URECTANGLE bLims;
            //int pid = aPanel.Index;
            //double rad = aPanel.Radius;
            //double bht;

            //if (aJoggleHt <= 0) aJoggleHt = aAssy.DesignOptions.JoggleAngle;

            //if (aGap <= 0) aGap = aAssy.PanelClearance(pid != 1);
            //if (rad <= 0)
            //{
            //    rad = aAssy.DeckRadius;
            //    aPanel.Radius = rad;
            //}

            //double xForNoLeftDowncomer = aAssy.DesignFamily.IsBeamDesignFamily() ? -(rad + 10) : 0;
            //double lDCX = aLeftDC != null ? aLeftDC.X + aLeftDC.BoxWidth / 2 + aGap : xForNoLeftDowncomer;
            //double rDCX = aRightDC != null ? aRightDC.X - aRightDC.BoxWidth / 2 - aGap : rad + 10;
            //if (aPanel.IsCenter) lDCX = -rDCX;
            //URECTANGLE aLims = new URECTANGLE(lDCX, rad + 10, rDCX, -(rad + 10));

            //if (aThickness <= 0) aThickness = aAssy.Deck.Thickness;
            //URECTANGLE limits = URECTANGLE.Null;
            //if (aTopSplice == null && aBotSplice == null)
            //{
            //    aShp = USHAPE.CircleSection(0, 0, rad, 0, aLims.Left, aLims.Right, aLims.Top, aLims.Bottom);
            //    aShp.Depth = aThickness;
            //    aShp.Name = $"{pid},1";
            //    aShp.Value = (aPanel.Index == 1) ? aShp.Limits.Width : aShp.Limits.Height;
            //    limits = aShp.Limits;
            //}
            //else
            //{
            //    double dpth = aThickness;


            //    uopPanelSectionShape panelShape = null;

            //    if (panelShapeIndex.HasValue && panelShapeIndex.Value > 0)
            //    {
            //        var panelShapes = mdDeckPanel.GetPanelShapes(aPanel, aAssy);
            //        if (panelShapes != null)
            //        {
            //            panelShape = panelShapes.First(ps =>  ps.SectionIndex == panelShapeIndex.Value);
            //        }


            //    }

            //    if (panelShape != null) 
            //    {
            //        limits = panelShape.ParentShape.BoundsV;
            //    }


            //    bLims = aLims;
            //    bLims.Left = limits.Left;
            //    bLims.Right = limits.Right;
            //    bool mFlg1 = false;
            //    bool mFlg2 = false;

            //    if (!aPanel.IsHalfMoon)
            //    {
            //        double d1 = (lDCX == -rDCX) ? rad : Math.Sqrt(Math.Pow(rad, 2) - (lDCX >= 0 ? Math.Pow(lDCX, 2) : Math.Pow(rDCX, 2)));

            //        double d2 = -d1;

            //        if (aTopSplice != null)
            //        {
            //            d1 = aTopSplice.Ordinate;
            //            mFlg1 = aTopSplice.SupportsManway;
            //            splLims = uopDeckSplice.SpliceLimits(aTopSplice, true, true);

            //            bLims.Top = splLims.Top;

            //            if (aTopSplice.SpliceType == uppSpliceTypes.SpliceWithJoggle)
            //            {
            //                if (!aTopSplice.Female) dpth = aJoggleHt;

            //            }
            //            else if (aTopSplice.SpliceType == uppSpliceTypes.SpliceWithTabs)
            //            {
            //                if (aTopSplice.Female)
            //                { dpth = aJoggleHt; }
            //                else
            //                {
            //                    if (dpth < aJoggleHt) dpth = 2 * aThickness;
            //                }
            //            }
            //        }

            //        if (aBotSplice != null)
            //        {
            //            d2 = aBotSplice.Ordinate;
            //            if (mFlg1)
            //            {
            //                if (aBotSplice.SupportsManway)
            //                {
            //                    if (aTopSplice.ManTag == "TOP")
            //                    {
            //                        if (aBotSplice.ManTag == "CENTER" || aBotSplice.ManTag == "BOTTOM") mFlg2 = true;

            //                    }
            //                    else if (aTopSplice.ManTag == "CENTER")
            //                    {
            //                        if (aBotSplice.ManTag == "BOTTOM") mFlg2 = true;

            //                    }
            //                }
            //            }

            //            splLims = uopDeckSplice.SpliceLimits(aBotSplice, true, true);
            //            bLims.Bottom = splLims.Bottom;

            //            if (aBotSplice.SpliceType == uppSpliceTypes.SpliceWithJoggle)
            //            {
            //                if (!aBotSplice.Female) dpth = aJoggleHt;

            //            }
            //            else if (aBotSplice.SpliceType == uppSpliceTypes.SpliceWithTabs)
            //            {
            //                if (aBotSplice.Female)
            //                { dpth = aJoggleHt; }
            //                else
            //                {
            //                    if (dpth < aJoggleHt) dpth = 2 * aThickness;
            //                }
            //            }
            //        }
            //        bht = d1 - d2;
            //    }
            //    else
            //    {
            //        double d1 = rad;
            //        double d2 = lDCX;

            //        if (aTopSplice != null)
            //        {
            //            d1 = aTopSplice.Ordinate;
            //            splLims = uopDeckSplice.SpliceLimits(aTopSplice, true, true);
            //            bLims.Right = splLims.Right;
            //            if (aTopSplice.SpliceType == uppSpliceTypes.SpliceWithJoggle)
            //            {
            //                if (!aTopSplice.Female) dpth = aJoggleHt;

            //            }
            //            else if (aTopSplice.SpliceType == uppSpliceTypes.SpliceWithTabs)
            //            {
            //                if (aTopSplice.Female)
            //                { dpth = aJoggleHt; }
            //                else
            //                {
            //                    if (dpth < aJoggleHt) dpth = 2 * aThickness;
            //                }
            //            }
            //        }

            //        if (aBotSplice != null)
            //        {
            //            d2 = aBotSplice.Ordinate;
            //            splLims = uopDeckSplice.SpliceLimits(aBotSplice, true, true);
            //            bLims.Left = splLims.Left;

            //            if (aBotSplice.SpliceType == uppSpliceTypes.SpliceWithJoggle)
            //            {
            //                if (!aBotSplice.Female) dpth = aJoggleHt;
            //            }
            //            else if (aBotSplice.SpliceType == uppSpliceTypes.SpliceWithTabs)
            //            {
            //                if (aBotSplice.Female)
            //                { dpth = aJoggleHt; }
            //                else
            //                {
            //                    if (dpth < aJoggleHt) dpth = 2 * aThickness;
            //                }
            //            }
            //        }
            //        bht = d1 - d2;
            //    }
            //    aShp = USHAPE.CircleSection(0, 0, rad, 0, bLims.Left, bLims.Right, bLims.Top, bLims.Bottom);
            //    aShp.Depth = dpth;
            //    aShp.Value = bht;
            //    aShp.Name = $"{pid},{topSplice?.SectionIndex}";
            //    if (mFlg1 && mFlg2) aShp.Tag = "MANWAY";


            //}
            //aShp.Value = Math.Min(Math.Min(aShp.Value, aShp.Height), aShp.Width);
            //return aShp;
            return new USHAPE("");
        }

        private static void xSectionBound(mdDeckSection aSection, mdTrayAssembly aAssy, mdDeckPanel aPanel, uopDeckSplice aTopSplice, uopDeckSplice aBotSplice, mdDowncomer aLeftDC, mdDowncomer aRightDC = null, double aGap = 0, double aThickness = 0, double aJoggleHt = 0)
        {

            aSection.ManholeID = aAssy.ManholeID;
            //aSection.PhysicalBoundaryV =   xSectionShape(aAssy, aPanel, aTopSplice, aBotSplice, aLeftDC, aRightDC, aGap, aThickness, aJoggleHt, panelShapeIndex: aSection.PanelShapeIndex);
              //if (!aSection.SingleSection)
            //{
            //    aSection.LapsRing = (aPanel.IsHalfMoon) || aSection.PhysicalBoundaryV.HasArcs;

            //}
            //else
            //{
            //    aSection.LapsRing = true;
            //}

            //aSection.FitWidth = (aSection.PhysicalBoundaryV.Width < aSection.PhysicalBoundaryV.Height) ? aSection.PhysicalBoundaryV.Width : aSection.PhysicalBoundaryV.Height;

            //if (!aSection.LapsRing || aSection.PanelIndex <= 1 || aSection.SingleSection || aSection.IsCenterSection) return;


            //ManholeBound(aSection, out double wd, out double ht);
            //aSection.FitWidth = Math.Min(wd, aSection.FitWidth);
            //aSection.FitWidth = Math.Min(ht, aSection.FitWidth);

        }


        public static uopShapes SectionBlockAreas(mdDeckSection aSection, mdTrayAssembly aAssy, double aSpliceAreaFactor = 0, bool bBothSides = false)
        {
            uopShapes _rVal = new uopShapes();
            try
            {
                if (aSection == null) return _rVal;

                aAssy = aSection.GetMDTrayAssembly(aAssy);
                if (aAssy == null) return _rVal;

                if (aSpliceAreaFactor <= 0) aSpliceAreaFactor = aAssy.SpliceAreaFactor;
                aSection.GetSplices(out uopDeckSplice tSplice, out uopDeckSplice bSplice);
                uopHoleArray aHls = aSection.GenHoles(aAssy);

                dxePolygon perim = aSection.Perimeter;
                uopHole aHl = null;
                double trimrad = aAssy.RingClipRadius;

                dxePolyline aPl = null;

                URECTANGLE mechLims = aSection.MechanicalLimits;
                URECTANGLE splLims;
                string hndl = aSection.Handle;

                dxeEllipse aEllipse = null;
                uopManwayClamp aClamp = null;
                dxfVector v1 = null;
                double fctr = aSpliceAreaFactor;
                double cX = aSection.X;
                double cY = aSection.Y;
                URECTANGLE lims = URECTANGLE.Null;
                int pid = aSection.PanelIndex;
                USHAPE blockedarea;


                if (fctr <= 0 || fctr > 1) fctr = 1;



                //============= RING CLIPS ======================
                if (aHls.TryGet("RING CLIP", out uopHoles Hls))
                {
                    double rad = mdGlobals.HoldDownWasherRadius;

                    for (int i = 1; i <= Hls.Count; i++)
                    {
                        aHl = Hls.Item(i);
                        blockedarea = USHAPE.Circle(aHl.X, aHl.Y, rad, aHl.Tag, aHl.Tag);
                        blockedarea.PartIndex = pid;
                        blockedarea.Handle = hndl;
                        _rVal.Add(blockedarea);

                    }
                }

                //================ MANWAY CLAMPS ===========================
                if (aSection.IsManway)
                {
                    v1 = dxfVector.Zero;
                    if (aHls.TryGet("MANWAY", out Hls, true))
                    {
                        if (!aAssy.DesignOptions.UseManwayClips)
                        {



                            aClamp = new uopManwayClamp();

                            for (int i = 1; i <= Hls.Count; i++)
                            {
                                aHl = Hls.Item(i);
                                aClamp.CenterDXF = aHl.CenterDXF;
                                v1.SetCoordinates(aHl.X, aHl.Y);
                                aEllipse = new dxeEllipse();
                                double value = 1.75;
                                aEllipse.MajorDiameter = value;
                                aEllipse.MinorDiameter = 1d;

                                if (aHl.Flag == "RIGHT")
                                {

                                    aEllipse.StartAngle = 55.285d;
                                    aEllipse.EndAngle = 360 - aEllipse.StartAngle;
                                }
                                else if (aHl.Flag == "LEFT")
                                {
                                    aEllipse.EndAngle = 126.285d;
                                    aEllipse.StartAngle = 360 - aEllipse.EndAngle;
                                }
                                else if (aHl.Flag == "TOP")
                                {
                                    v1.Move(-0.2015);
                                    aEllipse.Rotate(90);
                                    aEllipse.StartAngle = 55.285d;
                                    aEllipse.EndAngle = 360 - aEllipse.StartAngle;

                                }
                                else if (aHl.Flag == "BOTTOM")
                                {
                                    v1.Move(0.2015);
                                    aEllipse.Rotate(90);
                                    aEllipse.EndAngle = 126.285d;
                                    aEllipse.StartAngle = 360 - aEllipse.EndAngle;
                                }
                                aEllipse.Center = v1;

                                aPl = aEllipse.Perimeter(30, true);
                                blockedarea = USHAPE.FromDXFPolyline(aPl, "MANWAY CLAMP");
                                blockedarea.PartIndex = pid;
                                blockedarea.Handle = hndl;
                                _rVal.Add(blockedarea);
                            }
                        }
                        else
                        {
                            for (int j = 1; j <= aHls.Count; j++)
                            {
                                Hls = aHls.Item(j);
                                for (int i = 1; i <= Hls.Count; i++)
                                {
                                    aHl = Hls.Item(i);
                                    if (aHl.Rotation == 0 || aHl.Rotation == 180)
                                    {
                                        if (aHl.X < cX)
                                        {
                                            lims.Left = mechLims.Left;
                                            lims.Right = aHl.X + 0.5 * aHl.Length + 0.0937;
                                        }
                                        else
                                        {
                                            lims.Left = aHl.X - 0.5 * aHl.Length - 0.0937;
                                            lims.Right = mechLims.Right;
                                        }
                                        lims.Top = aHl.Y + 0.5;
                                        lims.Bottom = aHl.Y - 0.5;
                                    }
                                    else
                                    {
                                        if (aHl.Y < cY)
                                        {
                                            lims.Bottom = mechLims.Bottom + 1.0625;
                                            lims.Top = aHl.Y + 0.5 * aHl.Length + 0.0937;
                                        }
                                        else
                                        {
                                            lims.Bottom = aHl.Y - 0.5 * aHl.Length - 0.0937;
                                            lims.Top = mechLims.Top - 1.0625;
                                        }
                                        lims.Left = aHl.X - 0.5;
                                        lims.Right = aHl.X + 0.5;
                                    }

                                    blockedarea = lims.ToShape("MANWAY CLAMP");
                                    blockedarea.PartIndex = pid;
                                    blockedarea.Handle = hndl;
                                    _rVal.Add(blockedarea);

                                }

                            }
                        }
                    }
                }

                //================ SPLICES ===========================
                if (bSplice != null)
                {

                    splLims = bSplice.Limits(true);

                    if (aSection.PanelIndex == 1)
                    {
                        if (bSplice.SpliceType == uppSpliceTypes.SpliceWithTabs)
                        {
                            List<double> tOrds = aSection.TabOrdinates;
                            double ord;
                            lims = new URECTANGLE(aLeft: mechLims.Left, aRight: splLims.Right);
                            for (int j = 1; j <= tOrds.Count; j++)
                            {
                                ord = tOrds[j - 1];

                                lims.Define(aTop: ord + 1, aBot: ord - 1);
                                lims.TrimWithArc(trimrad);
                                blockedarea = lims.ToShape("TAB");
                                blockedarea.Handle = hndl;
                                blockedarea.PartIndex = pid;
                                _rVal.Add(blockedarea);


                            }
                        }
                    }
                    else
                    {
                        if (bSplice.SpliceType == uppSpliceTypes.SpliceWithTabs)
                        {
                            if (bSplice.Female)
                            {
                                List<double> tOrds = aSection.TabOrdinates;
                                double ord;

                                double minX = perim.GetVertex(dxxPointFilters.GetBottomLeft).X;
                                double maxX = perim.GetVertex(dxxPointFilters.GetBottomRight).X;
                                lims = new URECTANGLE(aTop: splLims.Top, aBottom: mechLims.Bottom);
                                for (int j = 1; j <= tOrds.Count; j++)
                                {
                                    ord = tOrds[j - 1];

                                    if (ord > minX && ord < maxX)
                                    {
                                        lims.Define(ord + 1, ord - 1);
                                        blockedarea = lims.ToShape(bSplice.Vertical ? "VTAB" : "TAB");
                                        blockedarea.PartIndex = pid;
                                        blockedarea.Handle = hndl;
                                        _rVal.Add(blockedarea);
                                    }

                                }
                            }
                            else
                            {
                                lims = new URECTANGLE(splLims.Left, splLims.Top, splLims.Right, mechLims.Bottom);
                                lims.TrimWithArc(trimrad);
                                blockedarea = lims.ToShape(bSplice.Vertical ? "VFLANGE" : "FLANGE");
                                blockedarea.Handle = hndl;
                                blockedarea.PartIndex = pid;
                                _rVal.Add(blockedarea);

                            }
                        }
                        else if (bSplice.SpliceType == uppSpliceTypes.SpliceWithJoggle)
                        {
                            if (bSplice.Female)
                            {
                                lims = new URECTANGLE(splLims.Left, splLims.Top, splLims.Right, mechLims.Bottom);
                                blockedarea = lims.ToShape("JOGGLE");
                                blockedarea.Handle = hndl;
                                _rVal.Add(blockedarea);
                            }
                        }
                        else if (bSplice.SpliceType == uppSpliceTypes.SpliceWithAngle)
                        {

                            lims = new URECTANGLE(splLims.Left, splLims.Top, splLims.Right, mechLims.Bottom);
                            blockedarea = lims.ToShape("SPLICE ANGLE");
                            blockedarea.PartIndex = pid;
                            blockedarea.Handle = hndl;
                            _rVal.Add(blockedarea);


                        }
                        else if (bSplice.SpliceType == uppSpliceTypes.SpliceManwayCenter)
                        {
                            lims = new URECTANGLE(splLims.Left, splLims.Top, splLims.Right, mechLims.Bottom);
                            blockedarea = lims.ToShape("MANWAY SPLICE");
                            blockedarea.PartIndex = pid;
                            blockedarea.Handle = hndl;
                            _rVal.Add(blockedarea);

                        }
                    }
                }

                if (tSplice != null)
                {
                    splLims = tSplice.Limits(true);

                    if (aSection.PanelIndex == 1)
                    {
                        if (tSplice.SpliceType == uppSpliceTypes.SpliceWithTabs)
                        {
                            lims = new URECTANGLE(splLims.Left, splLims.Top, mechLims.Right, splLims.Bottom);
                            blockedarea = lims.ToShape("VFLANGE");
                            blockedarea.PartIndex = pid;
                            blockedarea.Handle = hndl;
                            _rVal.Add(blockedarea);

                        }
                        else if (tSplice.SpliceType == uppSpliceTypes.SpliceWithJoggle)
                        {
                            double radius = aSection.RingRadius;
                            aPl = (dxePolyline)dxfPrimatives.CreateCircleSection(null, radius, false, aLeftEdge: splLims.Left, aRightEdge: mechLims.Right, aTopEdge: splLims.Top, aBottomEdge: splLims.Bottom);
                            aPl.Closed = true;
                            blockedarea = USHAPE.FromDXFPolyline(aPl, tSplice.Vertical ? "VFLANGE" : "FLANGE");
                            blockedarea.Handle = hndl;
                            blockedarea.PartIndex = pid;
                            _rVal.Add(blockedarea);



                        }
                    }
                    else
                    {
                        if (tSplice.SpliceType == uppSpliceTypes.SpliceWithTabs)
                        {
                            if (tSplice.Female)
                            {
                                List<double> tOrds = aSection.TabOrdinates;
                                double ord;
                                double minX = perim.GetVertex(dxxPointFilters.GetTopLeft).X;
                                double maxX = perim.GetVertex(dxxPointFilters.GetTopRight).X;
                                lims = new URECTANGLE(aTop: mechLims.Top, aBottom: splLims.Bottom);
                                for (int j = 1; j <= tOrds.Count; j++)
                                {
                                    ord = tOrds[j - 1];

                                    if (ord > minX && ord < maxX)
                                    {
                                        lims.Define(ord - 1, ord + 1);
                                        blockedarea = lims.ToShape(tSplice.Vertical ? "VTAB" : "TAB");
                                        blockedarea.Handle = hndl;
                                        blockedarea.PartIndex = pid;
                                        _rVal.Add(blockedarea);

                                    }



                                }
                            }
                            else
                            {
                                lims = new URECTANGLE(splLims.Left, mechLims.Top, splLims.Right, splLims.Bottom);
                                lims.TrimWithArc(trimrad);
                                blockedarea = lims.ToShape(tSplice.Vertical ? "VFLANGE" : "FLANGE");
                                blockedarea.PartIndex = pid;
                                blockedarea.Handle = hndl;
                                _rVal.Add(blockedarea);

                            }
                        }
                        else if (tSplice.SpliceType == uppSpliceTypes.SpliceWithJoggle)
                        {
                            if (tSplice.Female)
                            {
                                lims = new URECTANGLE(splLims.Left, mechLims.Top, splLims.Right, splLims.Bottom);
                                blockedarea = lims.ToShape("JOGGLE");
                                blockedarea.PartIndex = pid;
                                blockedarea.Handle = hndl;
                                _rVal.Add(blockedarea);

                            }
                        }
                        else if (tSplice.SpliceType == uppSpliceTypes.SpliceWithAngle)
                        {
                            lims = new URECTANGLE(splLims.Left, mechLims.Top, splLims.Right, splLims.Bottom);
                            blockedarea = lims.ToShape("SPLICE ANGLE");
                            blockedarea.PartIndex = pid;
                            blockedarea.Handle = hndl;
                            _rVal.Add(blockedarea);

                        }
                        else if (tSplice.SpliceType == uppSpliceTypes.SpliceManwayCenter)
                        {
                            lims = new URECTANGLE(splLims.Left, mechLims.Top, splLims.Right, splLims.Bottom);
                            blockedarea = lims.ToShape("MANWAY SPLICE");
                            blockedarea.PartIndex = pid;
                            blockedarea.Handle = hndl;
                            _rVal.Add(blockedarea);
                        }
                    }
                }

                if (aSection.OccuranceFactor > 1 && bBothSides && Math.Round(aSection.X, 2) > 0)
                {
                    int cnt = _rVal.Count;

                    for (int i = 1; i <= cnt; i++)
                    {
                        blockedarea = new USHAPE(_rVal.Item(i));
                        UVECTOR u1 = blockedarea.Center;
                        blockedarea.Translate(-2 * u1.X, -2 * u1.Y);
                        blockedarea.PartIndex = pid;
                        blockedarea.Mark = true;
                        blockedarea.Handle = hndl;
                        _rVal.Add(blockedarea);
                    }
                }

                return _rVal;
            }
            catch (Exception)
            {
                return _rVal;
            }
        }

        /// <summary>
        /// returns the plan view of a deck section slot & tab slot
        /// </summary>
        /// <param name="aMatThk"></param>
        /// <param name="aGap"></param>
        /// <returns></returns>
        private static dxePolygon xxSlotPolygon(double aMatThk, double aGap)
        {
            dxePolygon _rVal = null;
            if (_CurThk != aMatThk && _CurGap != Math.Abs(aGap)) _SlotPolyline = null;


            _CurThk = aMatThk;
            _CurGap = Math.Abs(aGap);
            //if (_SlotPolyline == null)
            //{
            try
            {
                colDXFVectors verts = new colDXFVectors();

                double rad = ((aMatThk * 25.4) <= 2.7) ? 10 / 25.4 / 2 : 12 / 25.4 / 2;
                double slen = mdGlobals.DeckTabSlotLength - 2 * rad;

                double x = -slen / 2;
                dxeArc aArc = new dxeArc(new dxfVector(-slen / 2, 0), rad, aStartAngle: 90, aEndAngle: 270);
                dxeLine aL = new dxeLine(new dxfVector(-3 * mdGlobals.DeckTabSlotLength, -_CurGap), new dxfVector(3 * mdGlobals.DeckTabSlotLength, -_CurGap));
                dxfVector v1 = aL.Intersections(aArc, true, false).Item(1);

                verts.Add(-slen / 2, rad, aVertexRadius: rad, aTag: "SLOT_FLANGE", aFlag: "NODIM");
                verts.Add(v1.X, v1.Y, aVertexRadius: rad);
                verts.Add(-slen / 2, -rad);
                verts.Add(slen / 2, -rad, aVertexRadius: rad);
                verts.Add(-v1.X, v1.Y, aVertexRadius: rad);
                verts.Add(slen / 2, rad, aVertexRadius: rad);
                _SlotPolyline = new dxePolygon(verts, dxfVector.Zero, bClosed: true, "TAB_SLOT") { Tag = "TABSLOT" };
                _rVal = _SlotPolyline;

            }
            catch (Exception)
            {
                return _rVal;
            }
            //}
            //else _rVal = _SlotPolyline;
            return _rVal;  //.Clone();
        }

        public static dxePolygon xaSplice_Female(mdTrayAssembly aAssy, mdDeckSection aSection, uopDeckSplice aSplice, bool bFaceUp = true, bool bLeftToRight = true, bool bFoldovers = false)
        {

            if (aSplice == null || aSection == null) return new dxePolygon();




            dxePolygon _rVal = aSplice.SpliceType switch
            {
                uppSpliceTypes.SpliceWithTabs => xSpliceFemale_SlotAndTab(aAssy, aSection, aSplice, bFaceUp, bLeftToRight, bFoldovers),
                uppSpliceTypes.SpliceWithAngle => xSpliceFemale_Angle(aAssy, aSection, aSplice, bFaceUp, bLeftToRight, bFoldovers),
                uppSpliceTypes.SpliceManwayCenter => xSpliceMale_ManwayAngle(aAssy, aSection, aSplice, bFaceUp, bLeftToRight, bFoldovers),
                uppSpliceTypes.SpliceWithJoggle => xSpliceFemale_Joggle(aAssy, aSection, aSplice, bFaceUp, bLeftToRight, bFoldovers),

                _ => new dxePolygon()
            };


            _rVal.InsertionPt.SetCoordinates(aSection.X, aSection.Y);

            return _rVal;
        }

        private static dxePolygon xSpliceFemale_Angle(mdTrayAssembly aAssy, mdDeckSection aSection, uopDeckSplice aSplice, bool bFaceUp = true, bool bLeftToRight = true, bool bFoldovers = false)
        {
            dxePolygon _rVal = new dxePolygon();
            try
            {

                _rVal.InsertionPt.SetCoordinates(aSection.X, aSection.Y);

                double addr = uopDeckSplice.SpliceLengthAdder(aSplice.SpliceType, aSplice.IsTop, aSection.IsManway, false, out double OPSS);
                double y1 = aSplice.Ordinate + addr;

                var panelShape = aSection.ParentShape; // mdDeckPanel.GetPanelShapeUsingPanelAndShapeIndices(aAssy, aSection.PanelIndex, aSection.PanelShapeIndex);
                (double minPanelShapeX, double maxPanelShapeX) = mdDeckPanel.GetPanelShapeMinMaxX(panelShape);

                double x1 = aSection.Limits.Left;
                var leftIntersection = GetSpliceIntersectionWithPanelShape(y1, minPanelShapeX - 1, aSplice.FlangeLine.MidPt.X, panelShape);
                if (leftIntersection.Item1 != null)
                {
                    if (x1 <= leftIntersection.Item1.X)
                    {
                        x1 = leftIntersection.Item1.X;
                    }
                }

                double x2 = aSection.Limits.Right;
                var rightIntersection = GetSpliceIntersectionWithPanelShape(y1, aSplice.FlangeLine.MidPt.X, maxPanelShapeX + 1, panelShape);
                if (rightIntersection.Item1 != null)
                {
                    if (x2 >= rightIntersection.Item1.X)
                    {
                        x2 = rightIntersection.Item1.X;
                    }
                }

                if (aSection.IsManway)
                {
                    x1 += 0.125;
                    x2 -= 0.125;
                }

                if (bLeftToRight)
                {
                    _rVal.AddVertex(x1, y1, 0);
                    if (rightIntersection.Item1 != null && rightIntersection.Item2 == "Ring")
                    {
                        _rVal.AddVertex(x2, y1, aSection.Radius);
                    }
                    else
                    {
                        _rVal.AddVertex(x2, y1, 0);
                    }
                }
                else
                {
                    _rVal.AddVertex(x2, y1, 0);
                    if (leftIntersection.Item1 != null && leftIntersection.Item2 == "Ring")
                    {
                        _rVal.AddVertex(x1, y1, aSection.Radius);
                    }
                    else
                    {
                        _rVal.AddVertex(x1, y1);
                    }
                }

                _rVal.Value = bLeftToRight ? 0 : 1;
                _rVal.Closed = false;
                return _rVal;
            }
            catch (Exception)
            {
                return _rVal;
            }
        }

        private static dxePolygon xSpliceFemale_Joggle(mdTrayAssembly aAssy, mdDeckSection aSection, uopDeckSplice aSplice, bool bFaceUp = true, bool bLeftToRight = true, bool bFoldovers = false)
        {
            dxePolygon _rVal = new dxePolygon();
            try
            {
                double addr = uopDeckSplice.SpliceLengthAdder(aSplice.SpliceType, aSplice.IsTop, aSplice.SupportsManway, aSplice.Female, out double OPSS);
                double rad = aSection.Radius;

                if (aSplice.Vertical)
                {
                    _rVal.InsertionPt.SetCoordinates(aSection.X, aSection.Y);
                    double x1 = aSplice.Ordinate + addr;
                    double y1 = Math.Sqrt(Math.Pow(rad, 2) - Math.Pow(x1, 2));
                    _rVal.Vertices.Add(x1, -y1);
                    _rVal.Vertices.Add(x1, y1);
                    _rVal.Closed = false;
                    _rVal.InsertionPt.SetCoordinates(aSplice.Ordinate, 0);

                    //// ******************** Trimming The Flange Line ********************
                    //// This part of the code is not related to the main task of the function.
                    //// With the current code architecture, part of the flange line trimming logic happens inside the male counterpart's polygon generation method.
                    //// Yet we need the correct flange line in female side too, for the hole generation. This is why we need to do this here, as well.

                    //// Find flange line X ordinate of the male part
                    //double maleAddr = uopDeckSplice.SpliceLengthAdder(aSplice.SpliceType, !aSplice.IsTop, false, false, out _);
                    //double maleFlangeBaseX = aSplice.Ordinate - maleAddr;
                    //aSplice.FlangeLine = new uopFlangeLine( GetTrimmedFlangeLineForVerticalMaleJoggleSplice(aSplice.FlangeLn, maleFlangeBaseX, aSection.TrimRadius));
                    //// ******************** Trimming The Flange Line ********************
                }
                else
                {
                    double y1 = aSplice.Ordinate + addr;
                    double x1 = aSection.Limits.Left;
                    double x2 = aSection.Limits.Right;

                    // Find the left and right intersections with the shape
                    var panelShape = aSection.ParentShape; // mdDeckPanel.GetPanelShapeUsingPanelAndShapeIndices(aAssy, aSection.PanelIndex, aSection.PanelShapeIndex);
                    (double minPanelShapeX, double maxPanelShapeX) = mdDeckPanel.GetPanelShapeMinMaxX(panelShape);

                    bool intersectsWithBeamOnLeft = false;
                    var leftIntersection = GetSpliceIntersectionWithPanelShape(y1, minPanelShapeX - 1, aSplice.FlangeLine.MidPt.X, panelShape);
                    if (leftIntersection.Item1 != null)
                    {
                        x1 = leftIntersection.Item1.X;
                        if (leftIntersection.Item2 == "Beam")
                        {
                            intersectsWithBeamOnLeft = true;
                        }
                    }

                    bool intersectsWithBeamOnRight = false;
                    var rightIntersection = GetSpliceIntersectionWithPanelShape(y1, aSplice.FlangeLine.MidPt.X, maxPanelShapeX + 1, panelShape);
                    if (rightIntersection.Item1 != null)
                    {
                        x2 = rightIntersection.Item1.X;
                        if (rightIntersection.Item2 == "Beam")
                        {
                            intersectsWithBeamOnRight = true;
                        }
                    }
                    // Find the left and right intersections with the shape

                    // ******************** Trimming The Flange Line ********************
                    // This part of the code is not related to the main task of the function.
                    // With the current code architecture, part of the flange line trimming logic happens inside the male counterpart's polygon generation method.
                    // Yet we need the correct flange line in female side too, for the hole generation. This is why we need to do this here, as well.

                    // Find flange line Y ordinates of the male part
                    double maleAddr = uopDeckSplice.SpliceLengthAdder(aSplice.SpliceType, !aSplice.IsTop, false, false, out _);
                    double maleFlangeY = aSplice.Ordinate + maleAddr;
                    double maleFlangeBaseY = aSplice.Ordinate - maleAddr;

                    bool isUpward = aSplice.Ordinate <= 0;

                    //aSplice.FlangeLine = new uopFlangeLine( GetTrimmedFlangeLineForMaleJoggleSplice(aSplice.FlangeLn, maleFlangeY, maleFlangeBaseY, intersectsWithBeamOnLeft, intersectsWithBeamOnRight, isUpward, aSection.TrimRadius, aAssy.RingClearance, aAssy));
                    // ******************** Trimming The Flange Line ********************

                    if (bLeftToRight)
                    {
                        _rVal.Vertices.Add(x1, y1);
                        if (rightIntersection.Item1 != null && rightIntersection.Item2 == "Ring")
                        {
                            _rVal.Vertices.Add(x2, y1, aSection.Radius);
                        }
                        else
                        {
                            _rVal.Vertices.Add(x2, y1);
                        }
                    }
                    else
                    {
                        _rVal.Vertices.Add(x2, y1);
                        if (leftIntersection.Item1 != null && leftIntersection.Item2 == "Ring")
                        {
                            _rVal.Vertices.Add(x1, y1, aSection.Radius);
                        }
                        else
                        {
                            _rVal.Vertices.Add(x1, y1);
                        }
                    }

                    _rVal.Closed = false;
                    _rVal.InsertionPt.SetCoordinates(aSplice.Ordinate, 0);
                    _rVal.Value = bLeftToRight ? 0 : 1;
                }
                return _rVal;
            }
            catch (Exception)
            {
                return _rVal;
            }
        }

        private static dxePolygon xSpliceFemale_SlotAndTab(mdTrayAssembly aAssy, mdDeckSection aSection, uopDeckSplice aSplice, bool bFaceUp = true, bool bLeftToRight = true, bool bFoldovers = false)
        {
            dxePolygon _rVal = new dxePolygon();
            try
            {
                _rVal.InsertionPt.SetCoordinates(aSection.X, aSection.Y);
                double rad = 0.125;
                double thk = aSection.Thickness;
                double gap = aSplice.GapValue(aSection.IsManway);
                colDXFVectors verts = new colDXFVectors();
                uopLine flngLn = aSplice.FlangeLine;
                UVECTORS tSlotCtrs = UVECTORS.Zero;

                if (aSplice.Vertical)
                {
                    double x1 = aSplice.Ordinate + Math.Abs(gap);
                    double x2 = x1 - mdGlobals.DeckTabFlangeHeight;
                    double x4 = x1 + 0.5;
                    double x3 = aSplice.Ordinate - Math.Abs(gap);

                    double y1 = Math.Sqrt(Math.Pow(aSection.Radius, 2) - Math.Pow(x4, 2));
                    double y3 = Math.Sqrt(Math.Pow(aSection.TrimRadius, 2) - Math.Pow(x3, 2));
                    double y2 = uopUtils.ComputeBeamLength(x2 + thk, aSection.TrimRadius, true) / 2;
                    if (y2 < y3) y3 = y2;

                    x3 = x2 + thk;
                    y2 = y3 + 2 * rad;

                    double lim1 = y3;
                    double lim2 = -y3;
                    verts.Add(x4, y1, aTag: "SLOT_FLANGE");
                    verts.Add(x1, y1, aTag: "SLOT_FLANGE", aFlag: "NODIM");
                    verts.Add(x1, y2, aVertexRadius: rad, bClockwise: true, aTag: "SLOT_FLANGE", aFlag: "NODIM");
                    verts.Add(x1 + rad, y2 - rad, aVertexRadius: rad, bClockwise: true, aTag: "SLOT_FLANGE", aFlag: "NODIM");
                    verts.Add(x1, y3, aTag: "SLOT_FLANGE", aFlag: "NODIM");
                    verts.Add(x1 - 2 * Math.Abs(gap), y3, aLineType: dxfLinetypes.Hidden, aTag: "SLOT_FLANGE", aFlag: "NODIM");
                    verts.Add(x2, y3, aLineType: dxfLinetypes.Hidden, aTag: "SLOT_FLANGE", aFlag: "NOV");

                    verts.Add(x2, -y3, aLineType: dxfLinetypes.Hidden, aTag: "SLOT_FLANGE", aFlag: "NOV");
                    verts.Add(x1 - 2 * Math.Abs(gap), -y3, aTag: "SLOT_FLANGE", aFlag: "NODIM");
                    verts.Add(x1, -y3, aVertexRadius: rad, bClockwise: true, aTag: "SLOT_FLANGE", aFlag: "NODIM");
                    verts.Add(x1 + rad, -y3 - rad, aVertexRadius: rad, bClockwise: true, aTag: "SLOT_FLANGE", aFlag: "NODIM");
                    verts.Add(x1, -y2, aTag: "SLOT_FLANGE", aFlag: "NODIM");
                    verts.Add(x1, -y1, aTag: "SLOT_FLANGE", aFlag: "NODIM");
                    verts.Add(x4, -y1, aTag: "SLOT_FLANGE");

                    //flngLn.ep.Y = y3;
                    //flngLn.sp.Y = -y3;

                    _rVal.Vertices = verts;
                    _rVal.AdditionalSegments.AddLine(x3, y3, x3, -y3, dxfDisplaySettings.Null(aLinetype: dxfLinetypes.Hidden), aTag: "THICKNESS");
                    _rVal.Closed = false;
                    //add the slots and bend lines
                    _rVal.AdditionalSegments.Append(xxGetSlotEntities(aAssy, aSection, aSplice.Ordinate, gap, aSplice.Vertical, bFaceUp, y3, -y3, out tSlotCtrs));
                    //====================
                }
                else
                {
                    //====================

                    int swap = !bFaceUp ? -1 : 1;
                    bool bInv = !bFaceUp ? bLeftToRight : !bLeftToRight;
                    bool bTrimmed = false;
                    double y1 = aSplice.Ordinate - (gap * swap);
                    double y2 = aSplice.Ordinate + (gap * swap);
                    double y3 = y1 + (mdGlobals.DeckTabFlangeHeight - thk) * swap;
                    double y4 = y1 + mdGlobals.DeckTabFlangeHeight * swap;

                    //// This part makes sure that the flange line will be trimmed by the beam clearance
                    //uopLine uopFlangeLine = uopFlangeLine = new uopLine(flngLn.sp.X, y4, flngLn.ep.X, y4);
                    //var beamClearanceFlangeIntersection = GetFlangeLineIntersectionWithBeamClearance(uopFlangeLine, aAssy.RingClearance, aAssy);
                    //if (beamClearanceFlangeIntersection != null)
                    //{
                    //    if (y4 > aSplice.Ordinate)
                    //    {
                    //        // If the flange line is above the splice and has intersection with beam clearance, its min X needs to change.
                    //        if (flngLn.sp.X < flngLn.ep.X)
                    //        {
                    //            flngLn.sp.X = beamClearanceFlangeIntersection.X;
                    //        }
                    //        else
                    //        {
                    //            flngLn.ep.X = beamClearanceFlangeIntersection.X;
                    //        }
                    //    }
                    //    else
                    //    {
                    //        // If the flange line is below the splice and has intersection with beam clearance, its max X needs to change.
                    //        if (flngLn.sp.X < flngLn.ep.X)
                    //        {
                    //            flngLn.ep.X = beamClearanceFlangeIntersection.X;
                    //        }
                    //        else
                    //        {
                    //            flngLn.sp.X = beamClearanceFlangeIntersection.X;
                    //        }
                    //    }
                    //}
                    
                    double x1 = aSection.Limits.Left;
                    double x3 = flngLn.sp.X;
                    double x4 = flngLn.ep.X;
                    double x6 = aSection.Limits.Right;
                    int swap2 = bLeftToRight ? 1 : -1;
                    //swap2 = 1;
                    double x2 = x3 - (2 * rad);
                    double x5 = x4 + (2 * rad);
                    if (aSection.X < 0)
                    {
                        if (aSplice.Y > aSection.Y)
                        {
                            x2 = x3 - (2 * rad * swap2);
                            x5 = x4 + (2 * rad * swap2);

                        }
                        else
                        {
                            x2 = x3 + (2 * rad * swap2);
                            x5 = x4 - (2 * rad * swap2);
                        }
                    }

                    double x7 = Math.Sqrt(Math.Pow(aSection.Radius, 2) - Math.Pow(y1, 2));
                    if (x6 > x7)
                    {
                        bTrimmed = true;
                        x6 = x7;
                    }

                    //// Check for beam intersection
                    //var shape = aSection.ParentShape; // mdDeckPanel.GetPanelShapeUsingPanelAndShapeIndices(aAssy, aSection.PanelIndex, aSection.PanelShapeIndex);
                    //dxeLine lineAlongPanel = new dxeLine(new dxfVector(x1, y1), new dxfVector(x6, y1));
                    //var intersectionPoints = shape.Perimeter().Intersections(lineAlongPanel);
                    //if (intersectionPoints.Count > 1)
                    //{
                    //    x1 = intersectionPoints.Min(ip => ip.X);
                    //    var intersectionMax = intersectionPoints.Max(ip => ip.X);
                    //    if (x6 > intersectionMax)
                    //    {
                    //        x6 = intersectionMax;
                    //        bTrimmed = false;
                    //    }
                    //}

                    mzUtils.SortTwoValues(bLeftToRight, ref x1, ref x6);
                    mzUtils.SortTwoValues(bLeftToRight, ref x3, ref x4);
                    mzUtils.SortTwoValues(bLeftToRight, ref x2, ref x5);

                    //shorten the flange if it intersects the trimming radius (RC radius)
                    x7 = Math.Max(Math.Abs(x3), Math.Abs(x4));
                    if (Math.Abs(y4) > Math.Sqrt(Math.Pow(aSection.TrimRadius, 2) - Math.Pow(x7, 2)))
                    {
                        double dX = Math.Sqrt(Math.Pow(aSection.TrimRadius, 2) - Math.Pow(y4, 2)) - x7;
                        if (dX != 0)
                        {
                            if (x3 < 0) // It is the left side and we need to push the left 
                            {
                                if (x3 < x4)
                                {
                                    x2 -= dX;
                                    x3 -= dX;
                                }
                                else
                                {
                                    x4 -= dX;
                                    x5 -= dX;
                                }
                            }
                            else
                            {
                                if (x3 < x4)
                                {
                                    x4 += dX;
                                    x5 += dX;
                                }
                                else
                                {
                                    x2 += dX;
                                    x3 += dX;
                                }
                            }
                        }

                    }

                    dxfVector v1 = verts.Add(x1, y1, aTag: "SLOT_FLANGE");
                    verts.Add(x2, y1, aVertexRadius: rad, bClockwise: bInv, aTag: "SLOT_FLANGE", aFlag: "NODIM");
                    verts.Add(x2 + rad * swap2, y1 - rad * swap, aVertexRadius: rad, bClockwise: bInv, aTag: "SLOT_FLANGE", aFlag: "NODIM");
                    verts.Add(x3, y1, aTag: "SLOT_FLANGE", aFlag: "NODIM");
                    verts.Add(x3, y2, aLineType: dxfLinetypes.Hidden, aTag: "SLOT_FLANGE", aFlag: "NODIM");
                    verts.Add(x3, y4, aLineType: dxfLinetypes.Hidden, aTag: "SLOT_FLANGE", aFlag: "NOH");

                    verts.Add(x4, y4, aLineType: dxfLinetypes.Hidden, aTag: "SLOT_FLANGE", aFlag: "NOH");
                    verts.Add(x4, y2, aTag: "SLOT_FLANGE", aFlag: "NODIM");
                    verts.Add(x4, y1, aVertexRadius: rad, bClockwise: bInv, aTag: "SLOT_FLANGE", aFlag: "NODIM");
                    verts.Add(x4 + rad * swap2, y1 - rad * swap, aVertexRadius: rad, bClockwise: bInv, aTag: "SLOT_FLANGE", aFlag: "NODIM");
                    verts.Add(x5, y1, aTag: "SLOT_FLANGE", aFlag: "NODIM");
                    dxfVector v2 = verts.Add(x6, y1, aTag: "SLOT_FLANGE", aVertexRadius: bTrimmed && !bFaceUp ? aSection.Radius : 0);

                    mzUtils.SortTwoValues(aSection.X >= 0, ref x3, ref x4);
                    //flngLn.sp.X = x3;
                    //flngLn.ep.X = x4;

                    _rVal.Value = bLeftToRight ? 0 : 1;

                    // To deal with the curves, in right half circle, trimmed by the ring, we only need to set radius if it is a bottom splice
                    if (bTrimmed && aSection.X >= 0 && !bFaceUp)
                    {
                        var rightMostV = verts.GetVector(dxxPointFilters.AtMaxX);
                        rightMostV.VertexRadius = aSection.Radius;
                    }
                    // To deal with the curves, in left half circle, trimmed by the ring, we only need to set radius if it is a top splice
                    if (bTrimmed && aSection.X < 0 && bFaceUp)
                    {
                        var leftMostV = verts.GetVector(dxxPointFilters.AtMinX);
                        leftMostV.VertexRadius = aSection.Radius;
                    }
                    
                    _rVal.Vertices = verts;
                    _rVal.AdditionalSegments.AddLine(x3, y3, x4, y3, dxfDisplaySettings.Null(aLinetype: dxfLinetypes.Hidden), aTag: "THICKNESS");

                    _rVal.Closed = false;
                    //add the slots and bend lines
                    _rVal.AdditionalSegments.Append(xxGetSlotEntities(aAssy, aSection, aSplice.Ordinate, gap, aSplice.Vertical, bFaceUp, x3, x4, out tSlotCtrs));
                }
                //aSplice.FlangeLine = new uopFlangeLine(flngLn);
                if (tSlotCtrs.Count > 0)
                {
                    aSection.SlotPoints.Append(tSlotCtrs);
                }

                return _rVal;
            }
            catch (Exception)
            {
                return _rVal;
            }

        }
        /// <summary>
        /// the line used to create the bolt holes on the splice
        /// </summary>
        /// <param name="aOrdinate"></param>
        /// <param name="aDeckPanel"></param>
        /// <returns></returns>
        internal static ULINE GetJoggleBoltLine(double aOrdinate, mdDeckPanel aDeckPanel)
        {

            if (aDeckPanel == null) return new ULINE();
            return xxJoggleBoltLine(aOrdinate, aDeckPanel);
        }
        /// <summary>
        /// the line used to create the bolt holes on the splice
        /// </summary>
        /// <param name="aOrdinate"></param>
        /// <param name="aPanel"></param>
        /// <returns></returns>
        internal static ULINE xxJoggleBoltLine(double aOrdinate, mdDeckPanel aPanel)
        {
            ULINE _rVal = ULINE.Null;
            double x1 = 0;
            double x2 = 0;
            double y1 = 0;
            double y2 = 0;
            double pRad = 0;
            double rrad = 0;
            pRad = aPanel.Radius;
            rrad = aPanel.RingRadius;
            if (pRad == 0 || rrad == 0)
            {
                return _rVal;
            }

            if (aPanel.IsHalfMoon)
            {
                x1 = Math.Abs(aOrdinate);
                y1 = Math.Sqrt(Math.Pow(rrad, 2) - Math.Pow(x1, 2)) - 0.625;
                x2 = x1;
                y2 = -y1;
            }
            else
            {
                x1 = aPanel.Limits.Left + 1.125;
                x2 = aPanel.Limits.Right - 1.125;
                y1 = aOrdinate;
                y2 = y1;
            }
            _rVal.sp.X = x1;
            _rVal.sp.Y = y1;
            _rVal.ep.X = x2;
            _rVal.ep.Y = y2;
            return _rVal;
        }

        public static dxePolygon xaSplice_Male(mdTrayAssembly aAssy, mdDeckSection aSection, uopDeckSplice aSplice, bool bFaceUp = true, bool bLeftToRight = true, bool bFoldovers = false)
        {

            if (aSplice == null || aSection == null) return new dxePolygon();



            dxePolygon _rVal = aSplice.SpliceType switch
            {
                uppSpliceTypes.SpliceWithTabs => xSpliceMale_SlotAndTab(aAssy, aSection, aSplice, bFaceUp, bLeftToRight, bFoldovers),
                uppSpliceTypes.SpliceWithAngle => xSpliceMale_Angle(aAssy, aSection, aSplice, bFaceUp, bLeftToRight, bFoldovers),
                uppSpliceTypes.SpliceManwayCenter => xSpliceMale_ManwayAngle(aAssy, aSection, aSplice, bFaceUp, bLeftToRight, bFoldovers),
                uppSpliceTypes.SpliceWithJoggle => xSpliceMale_Joggle(aAssy, aSection, aSplice, bFaceUp, bLeftToRight, bFoldovers),

                _ => new dxePolygon()
            };

            _rVal.InsertionPt.SetCoordinates(aSplice.X, aSplice.Y);

            return _rVal;
        }

        private static dxePolygon xSpliceMale_Angle(mdTrayAssembly aAssy, mdDeckSection aSection, uopDeckSplice aSplice, bool bFaceUp = true, bool bLeftToRight = true, bool bFoldovers = false)
        {
            dxePolygon _rVal = new dxePolygon();
            try
            {
                _rVal.InsertionPt.SetCoordinates(aSection.X, aSection.Y);


                double addr = uopDeckSplice.SpliceLengthAdder(aSplice.SpliceType, aSplice.IsTop, aSection.IsManway, false, out double OPSS);
                double y1 = aSplice.Ordinate + addr;
                double x1 = aSection.Limits.Left;
                double x2 = aSection.Limits.Right;
                if (aSection.IsManway)
                {
                    x1 += 0.125;
                    x2 -= 0.125;
                }
                mzUtils.SortTwoValues(bLeftToRight, ref x1, ref x2);

                _rVal.AddVertex(x1, y1, 0);
                _rVal.AddVertex(x2, y1, 0);
                _rVal.Value = bLeftToRight ? 0 : 1;
                _rVal.Closed = false;
                return _rVal;
            }
            catch (Exception)
            {
                return _rVal;
            }
        }

        private static dxePolygon xSpliceMale_Joggle(mdTrayAssembly aAssy, mdDeckSection aSection, uopDeckSplice aSplice, bool bFaceUp = true, bool bLeftToRight = true, bool bFoldovers = false)
        {
            dxePolygon _rVal = new dxePolygon();
            try
            {
                _rVal.InsertionPt.SetCoordinates(aSection.X, aSection.Y);

                double thk = aSection.Thickness;

                double rrad = aSection.RingRadius;
                double sRad = aSection.Radius;
                colDXFVectors verts = new colDXFVectors();
                double swap = 0;

                double addr = uopDeckSplice.SpliceLengthAdder(aSplice.SpliceType, aSplice.IsTop, false, false, out double addr2);


                if (aSplice.Vertical)
                {
                    double x1 = aSplice.Ordinate + addr;
                    double x2 = aSplice.Ordinate - addr;
                    double x3 = aSplice.Ordinate + addr2;
                    double x4 = x2 + 0.5;
                    double y2 = Math.Sqrt(Math.Pow(sRad, 2) - Math.Pow(x4, 2));

                    //aSplice.FlangeLine = new uopFlangeLine(GetTrimmedFlangeLineForVerticalMaleJoggleSplice(aSplice.FlangeLn, x2, aSection.TrimRadius));
                    double y1 = aSplice.FlangeLine.MaxY;
                    
                    verts.Add(x4, y2);
                    verts.Add(x3, y2, aLineType: dxfLinetypes.Hidden);
                    verts.Add(x2, y2, aLineType: dxfLinetypes.Hidden);
                    verts.Add(x2, y1, aLineType: dxfLinetypes.Hidden);
                    verts.Add(x1, y1, aLineType: dxfLinetypes.Hidden);
                    verts.Add(x1, -y1, aLineType: dxfLinetypes.Hidden);
                    verts.Add(x2, -y1, aLineType: dxfLinetypes.Hidden);
                    verts.Add(x2, -y2, aLineType: dxfLinetypes.Hidden);
                    verts.Add(x3, -y2);

                    verts.Add(x4, -y2);


                    _rVal.Vertices = verts;
                    _rVal.AdditionalSegments.AddLine(x1 + thk, y1, x1 + thk, -y1, new dxfDisplaySettings() { Linetype = dxfLinetypes.Hidden });
                    _rVal.Closed = false;

                    //============================================================
                }
                else
                {
                    //============================================================
                    swap = bFaceUp ? -1 : 1;


                    double y2 = aSplice.Ordinate - addr;
                    double y1 = aSplice.Ordinate + addr2;

                    double y4 = aSplice.Ordinate + addr;
                    double y3 = y4 + thk * swap;

                    double x2 = aSplice.FlangeLine.sp.X;
                    double x3 = aSplice.FlangeLine.ep.X;

                    // Find the left and right intersections with the shape
                    var panelShape = aSection.ParentShape; // mdDeckPanel.GetPanelShapeUsingPanelAndShapeIndices(aAssy, aSection.PanelIndex, aSection.PanelShapeIndex);
                    (double minPanelShapeX, double maxPanelShapeX) = mdDeckPanel.GetPanelShapeMinMaxX(panelShape);

                    double x1 = aSection.Limits.Left;
                    bool intersectsWithBeamOnLeft = false;
                    var leftIntersection = GetSpliceIntersectionWithPanelShape(y1, minPanelShapeX - 1, aSplice.FlangeLine.MidPt.X, panelShape);
                    if (leftIntersection.Item1 != null)
                    {
                        x1 = leftIntersection.Item1.X;
                        if (leftIntersection.Item2 == "Beam")
                        {
                            intersectsWithBeamOnLeft = true; 
                        }
                    }

                    double x4 = aSection.Limits.Right;
                    bool intersectsWithBeamOnRight = false;
                    var rightIntersection = GetSpliceIntersectionWithPanelShape(y1, aSplice.FlangeLine.MidPt.X, maxPanelShapeX + 1, panelShape);
                    if (rightIntersection.Item1 != null)
                    {
                        x4 = rightIntersection.Item1.X;
                        if (rightIntersection.Item2 == "Beam")
                        {
                            intersectsWithBeamOnRight = true; 
                        }
                    }
                    // Find the left and right intersections with the shape

                    // Trimming the flange line
                    bool isUpward = aSplice.Ordinate <= 0;

                    //aSplice.FlangeLine = new uopFlangeLine(GetTrimmedFlangeLineForMaleJoggleSplice(aSplice.FlangeLn, y4, y2, intersectsWithBeamOnLeft, intersectsWithBeamOnRight, isUpward, aSection.TrimRadius, aAssy.RingClearance, aAssy));
                    // Trimming the flange line

                    // Find the left and right intersection with beam at y2
                    // When flange line is upward and the male connection is trimmed by the beam from the left or
                    // when flange line is downward and the male connection is trimmed by the beam from the right,
                    // the shape protrudes a little bit. This part is to make sure that won't happen and the point is along the shape (doesn't create a rectangular shape)

                    var leftIntersectionAtY2 = GetSpliceIntersectionWithPanelShape(y2, minPanelShapeX - 1, aSplice.FlangeLine.MidPt.X, panelShape);
                    var rightIntersectionAtY2 = GetSpliceIntersectionWithPanelShape(y2, aSplice.FlangeLine.MidPt.X, maxPanelShapeX + 1, panelShape);
                    // Find the left and right intersection with beam at y2

                    if (bLeftToRight)
                    {
                        verts.Add(x1, y1, aLineType: dxfLinetypes.Hidden);
                        verts.Add(x1, y2, aLineType: dxfLinetypes.Hidden);
                        verts.Add(x2, y2, aLineType: dxfLinetypes.Hidden);
                        verts.Add(x2, y4, aLineType: dxfLinetypes.Hidden);
                        //-----------------------------------------------------
                        verts.Add(x3, y4, aLineType: dxfLinetypes.Hidden);
                        verts.Add(x3, y2, aLineType: dxfLinetypes.Hidden);

                        if (rightIntersectionAtY2.Item1 != null && rightIntersectionAtY2.Item2 == "Beam")
                        {
                            // When it is downward and it has an intersection with the beam at the right side, we need to use the intersection point's X
                            verts.Add(rightIntersectionAtY2.Item1.X, y2, aLineType: dxfLinetypes.Hidden);
                        }
                        else
                        {
                            verts.Add(x4, y2, aLineType: dxfLinetypes.Hidden);
                        }

                        
                        if (rightIntersection.Item1!=null && rightIntersection.Item2 == "Ring")
                        {
                            verts.Add(x4, y1, aVertexRadius: aSection.Radius);
                        }
                        else
                        {
                            verts.Add(x4, y1);
                        }
                    }
                    else
                    {
                        verts.Add(x4, y1, aLineType: dxfLinetypes.Hidden);
                        verts.Add(x4, y2, aLineType: dxfLinetypes.Hidden);
                        verts.Add(x3, y2, aLineType: dxfLinetypes.Hidden);
                        verts.Add(x3, y4, aLineType: dxfLinetypes.Hidden);
                        //-----------------------------------------------------
                        verts.Add(x2, y4, aLineType: dxfLinetypes.Hidden);
                        verts.Add(x2, y2, aLineType: dxfLinetypes.Hidden);

                        if (leftIntersectionAtY2.Item1 != null && leftIntersectionAtY2.Item2 == "Beam")
                        {
                            // When it is upward and it has an intersection with the beam at the left side, we need to use the intersection point's X
                            verts.Add(leftIntersectionAtY2.Item1.X, y2, aLineType: dxfLinetypes.Hidden);
                        }
                        else
                        {
                            verts.Add(x1, y2, aLineType: dxfLinetypes.Hidden);
                        }
                        
                        if (leftIntersection.Item1 != null && leftIntersection.Item2 == "Ring")
                        {
                            verts.Add(x1, y1, aVertexRadius: aSection.Radius);
                        }
                        else
                        {
                            verts.Add(x1, y1);
                        }
                    }
                    
                    _rVal.Vertices = verts;

                    _rVal.AdditionalSegments.AddLine(x2, y3, x3, y3, new dxfDisplaySettings() { Linetype = dxfLinetypes.Hidden });

                    _rVal.Closed = false;
                    _rVal.Value = bLeftToRight ? 0 : 1;
                }
                return _rVal;
            }
            catch (Exception)
            {
                return _rVal;
            }
        }

        private static dxePolygon xSpliceMale_ManwayAngle(mdTrayAssembly aAssy, mdDeckSection aSection, uopDeckSplice aSplice, bool bFaceUp = true, bool bLeftToRight = true, bool bFoldovers = false)
        {
            dxePolygon _rVal = new dxePolygon();
            try
            {
                _rVal.InsertionPt.SetCoordinates(aSection.X, aSection.Y);
                aAssy ??= aSection.GetMDTrayAssembly();
                int swap = 0;
                double y1 = 0;
                double x1 = 0;
                double x2 = 0;
                mdSpliceAngle aSA = aAssy.SpliceAngle( uppSpliceAngleTypes.ManwaySplicePlate);

                dxfVector v1 = null;
                double swapX = 0;
                colDXFVectors aPts = new colDXFVectors();
                double addr = 0;
                ULINE bLine;
                string lt = dxfLinetypes.ByLayer;
                colDXFVectors verts = new colDXFVectors();

                aSA.X = aSection.X;
                colDXFVectors vPts = aSA.GenHoles().DXFCenters();
                vPts.GetVectors(aFilter: dxxPointFilters.LessThanY, aOrdinate: aSA.Center.Y, bRemove: true);
                if (bLeftToRight)
                {
                    vPts.Sort(dxxSortOrders.LeftToRight);
                    x1 = aSection.Limits.Left;
                    x2 = aSection.Limits.Right;
                    swapX = 1;
                }
                else
                {
                    vPts.Sort(dxxSortOrders.RightToLeft);
                    x1 = aSection.Limits.Right;
                    x2 = aSection.Limits.Left;
                    swapX = -1;
                }


                addr = uopDeckSplice.SpliceLengthAdder(aSplice.SpliceType, aSplice.IsTop, false, false, out double OPSS);
                swap = (addr > 0) ? -1 : 1;

                y1 = aSplice.Ordinate + addr;
                bLine = new ULINE( aSplice.FlangeLine);
                bLine.sp.X = aSplice.X - aSA.Length / 2;
                bLine.ep.X = aSplice.X + aSA.Length / 2;
                aSplice.FlangeLine = new uopFlangeLine( bLine);

                verts.Add(x1, y1, aLineType: lt, aTag: "MANGLE", aFlag: "NODIM");

                for (int i = 1; i <= vPts.Count; i++)
                {
                    v1 = vPts.Item(i);
                    x1 = v1.X;
                    verts.Add(x1 - 0.2359 * swapX, y1, aLineType: lt, aTag: "c", aFlag: "NODIM");
                    verts.AddRelative(0, -0.748 * swap, aVertexRadius: -0.236, aLineType: lt, aTag: "MANGLE", aFlag: "NODIM");
                    verts.AddRelative(0.472 / 2 * swapX, -0.472 / 2 * swap, aVertexRadius: -0.236, aLineType: lt, aTag: "MANGLE", aFlag: "NODIM");
                    verts.AddRelative(0.472 / 2 * swapX, 0.472 / 2 * swap, aLineType: lt, aTag: "MANGLE", aFlag: "NODIM");
                    verts.AddRelative(0, 0.748 * swap, aLineType: lt, aTag: "MANGLE", aFlag: "NODIM");
                }
                verts.Add(x2, y1, aLineType: lt, aTag: "MANGLE", aFlag: "NODIM");
                verts.RemoveCoincidentVectors();
                _rVal.Vertices = verts;
                _rVal.Value = bLeftToRight ? 0 : 1;
                _rVal.Closed = false;
                return _rVal;
            }
            catch (Exception)
            {
                return _rVal;
            }
        }

        private static dxePolygon xSpliceMale_SlotAndTab(mdTrayAssembly aAssy, mdDeckSection aSection, uopDeckSplice aSplice, bool bFaceUp = true, bool bLeftToRight = true, bool bFoldovers = false)
        {
            dxePolygon _rVal = new dxePolygon();
            try
            {

                _rVal.InsertionPt.SetCoordinates(aSection.X, aSection.Y);
                dxfVector v2 = null;
                List<double> tOrds;
                double swap = 0;
                double x1 = 0;
                double x2 = 0;
                double x3 = 0;
                double gap = aSplice.GapValue(aSection.IsManway);
                double thk = aSection.Thickness;

                double y1 = 0;
                double y2 = 0;
                colDXFVectors verts = new colDXFVectors();

                double step = 0;
                colDXFVectors cverts = null;
                colDXFEntities bendLns = null;
                colDXFEntities addLns = null;
                dxeLine aAxis = new dxeLine(dxfVector.Zero, new dxfVector(0, 0, 100));

                bool bTrimmed = false;


                if (aSplice.Vertical)
                {

                    colDXFVectors pverts = xxTabVertices(thk, gap, out bendLns, false).Clone();
                    pverts.SetTagsAndFlags(aFlag: "NOV", aSearchFlag: "NOH");
                    pverts.RotateAbout(aAxis, -90);
                    bendLns.RotateAbout(aAxis, -90);

                    x1 = aSplice.Ordinate - Math.Abs(gap);
                    y1 = -Math.Sqrt(Math.Pow(aSection.Radius, 2) - Math.Pow(x1, 2));
                    y2 = -y1;

                    x2 = pverts.Item(1).X;
                    pverts.Move(x1 - x2);
                    bendLns.Move(x1 - x2);

                    tOrds = aSection.TabOrdinates;
                    
                    verts.Add(x1, y1);

                    for (int i = 1; i <= tOrds.Count; i++)
                    {
                        cverts = pverts.Clone();
                        addLns = bendLns.Clone();
                        //to do need to verify parameter order
                        addLns.Move(aChangeY: tOrds[i - 1]);
                        cverts.Move(aChangeY: tOrds[i - 1]);
                        verts.Append(cverts);
                        _rVal.AdditionalSegments.Append(addLns, false);
                    }
                    verts.Add(x1, y2);
                    _rVal.Vertices = verts;

                    //===========================
                }
                else
                {
                    //===========================
                    
                    colDXFVectors pverts = xxTabVertices(thk, gap, out bendLns).Clone();
                    var halfTabWidth = pverts.BoundingRectangle().Width / 2;
                    x3 = Math.Sqrt(Math.Pow(aSection.Radius, 2) - Math.Pow(Math.Abs(aSplice.Ordinate) + Math.Abs(gap), 2));
                    swap = !bFaceUp ? 1 : -1;

                    if (bLeftToRight)
                    {
                        x1 = aSection.Limits.Left;
                        x2 = aSection.Limits.Right;

                        if (x2 > x3)
                        {
                            bTrimmed = true;
                            x2 = x3;
                        }

                        // Check for beam intersection
                        var shape = aSection.ParentShape; // mdDeckPanel.GetPanelShapeUsingPanelAndShapeIndices(aAssy, aSection.PanelIndex, aSection.PanelShapeIndex);
                        dxeLine lineAlongPanel = new dxeLine(new dxfVector(x1, aSplice.Ordinate + gap * swap), new dxfVector(x2, aSplice.Ordinate + gap * swap));
                        var intersectionPoints = shape.Perimeter().Intersections(lineAlongPanel);
                        if (intersectionPoints.Count > 1)
                        {
                            x1 = intersectionPoints.Min(ip => ip.X);
                            var intersectionMax = intersectionPoints.Max(ip => ip.X);
                            if (x2 > intersectionMax)
                            {
                                x2 = intersectionMax;
                                bTrimmed = false;
                            }
                        }

                        if (!bFaceUp)
                        {
                            pverts.RotateAbout(aAxis, -180);
                            bendLns.RotateAbout(aAxis, -180);
                        }
                    }
                    else
                    {
                        x1 = aSection.Limits.Right;
                        x2 = aSection.Limits.Left;

                        if (x1 > x3)
                        {
                            bTrimmed = true;
                            x1 = x3;
                        }

                        // Check for beam intersection
                        var shape = aSection.ParentShape; // mdDeckPanel.GetPanelShapeUsingPanelAndShapeIndices(aAssy, aSection.PanelIndex, aSection.PanelShapeIndex);
                        dxeLine lineAlongPanel = new dxeLine(new dxfVector(x1, aSplice.Ordinate + gap * swap), new dxfVector(x2, aSplice.Ordinate + gap * swap));
                        var intersectionPoints = shape.Perimeter().Intersections(lineAlongPanel);
                        if (intersectionPoints.Count > 1)
                        {
                            x2 = intersectionPoints.Min(ip => ip.X);
                            var intersectionMax = intersectionPoints.Max(ip => ip.X);
                            if (x1 > intersectionMax)
                            {
                                x1 = intersectionMax;
                                bTrimmed = false;
                            }
                        }

                        if (!bFaceUp)
                        {
                            pverts.RotateAbout(aAxis, 180);
                            bendLns.RotateAbout(aAxis, 180);
                        }
                    }

                    y1 = aSplice.Ordinate + gap * swap;
                    y2 = pverts.Item(1).Y;

                    pverts.Move(aChangeY: y1 - y2);
                    bendLns.Move(aChangeY: y1 - y2);

                    // This part makes sure that the flange line will be trimmed by the beam clearance
                    // The flange line is displayed as part of the female connection. But we need to find it here.
                    // Flange line is used here to eliminate the tab ordinates which are not fit within the flange line.
                    double flangeLineY = aSplice.Ordinate - (gap * swap) + (mdGlobals.DeckTabFlangeHeight * swap);
                    var flngLn = aSplice.FlangeLine;
                    uopLine uopFlangeLine = uopFlangeLine = new uopLine(flngLn.sp.X, flangeLineY, flngLn.ep.X, flangeLineY);

                    // Check intersection with trim circle
                    var intersectionWithTrimRadiusX = Math.Sqrt(Math.Pow(aSection.TrimRadius, 2) - Math.Pow(flangeLineY, 2));
                    if (intersectionWithTrimRadiusX < Math.Abs(uopFlangeLine.ep.X))
                    {
                        if (uopFlangeLine.ep.X < 0)
                        {
                            intersectionWithTrimRadiusX = -intersectionWithTrimRadiusX;
                        }

                        flngLn.ep.X = intersectionWithTrimRadiusX;
                        uopFlangeLine.ep.X = intersectionWithTrimRadiusX;
                    }

                    var beamClearanceFlangeIntersection = GetFlangeLineIntersectionWithBeamClearance(uopFlangeLine, aAssy.RingClearance, aAssy);
                    if (beamClearanceFlangeIntersection != null)
                    {
                        if (flangeLineY > aSplice.Ordinate)
                        {
                            // If the flange line is above the splice and has intersection with beam clearance, its min X needs to change.
                            if (flngLn.sp.X < flngLn.ep.X)
                            {
                                flngLn.sp.X = beamClearanceFlangeIntersection.X;
                            }
                            else
                            {
                                flngLn.ep.X = beamClearanceFlangeIntersection.X;
                            }
                        }
                        else
                        {
                            // If the flange line is below the splice and has intersection with beam clearance, its max X needs to change.
                            if (flngLn.sp.X < flngLn.ep.X)
                            {
                                flngLn.ep.X = beamClearanceFlangeIntersection.X;
                            }
                            else
                            {
                                flngLn.sp.X = beamClearanceFlangeIntersection.X;
                            }
                        }
                    }

                    tOrds = aSection.TabOrdinates;
                    tOrds = tOrds.Where(o => o >= flngLn.MinX + halfTabWidth && o <= flngLn.MaxX - halfTabWidth).ToList();
              


                    if (tOrds.Count > 0)
                    {
                        double dx = (!bLeftToRight) ? tOrds[tOrds.Count - 1] : tOrds[0];

                        if (!bLeftToRight) step = -step;

                        pverts.Move(dx);
                        bendLns.Move(dx);
                    }

                    verts.Add(x1, y1);
                    for (int i = 1; i <= tOrds.Count; i++)
                    {
                        verts.Append(pverts, bAppendClones: true);
                        _rVal.AdditionalSegments.Append(bendLns, true);
                        if (i < tOrds.Count)
                        {
                            pverts.Move(step);
                            bendLns.Move(step);
                        }
                    }
                    v2 = verts.Add(x2, y1);
                    if (bTrimmed && bLeftToRight) v2.VertexRadius = aSection.Radius;

                    // To deal with the curves, in right half circle, trimmed by the ring, we only need to set radius if it is a bottom splice
                    if (bTrimmed && aSection.X >= 0 && !bFaceUp)
                    {
                        var rightMostV = verts.GetVector(dxxPointFilters.AtMaxX);
                        rightMostV.VertexRadius = aSection.Radius;
                    }
                    // To deal with the curves, in left half circle, trimmed by the ring, we only need to set radius if it is a top splice
                    if (bTrimmed && aSection.X < 0 && bFaceUp)
                    {
                        var leftMostV = verts.GetVector(dxxPointFilters.AtMinX);
                        leftMostV.VertexRadius = aSection.Radius;
                    }
                    
                    _rVal.Vertices = verts;
                }
                _rVal.Value = bLeftToRight ? 0 : 1;
                _rVal.Closed = false;
                //if (bTrimmed)
                //{
                //    y1 = aSplice.Ordinate;
                //    ULINE flngln = aSplice.FlangeLn;
                //    if (bFaceUp)
                //    {

                //    }


                //}

                return _rVal;
            }
            catch (Exception)
            {
                return _rVal;
            }
        }

        private static colDXFEntities xxGetSlotEntities(mdTrayAssembly aAssy, mdDeckSection aSection, double aOrdinate, double aGap, bool bVertical, bool bFaceUp, double aLimit1, double aLimit2, out UVECTORS rTabCenters)
        {
            rTabCenters = UVECTORS.Zero;
            colDXFEntities _rVal = new colDXFEntities();
            try
            {

                dxePolygon aPline = xxSlotPolygon(aSection.Thickness, aGap);
                List<double> tOrds = aSection.TabOrdinates;
                var leftLimit = Math.Min(aLimit1, aLimit2);
                var rightLimit = Math.Max(aLimit1, aLimit2);
                var halfTabWidth = (aPline.GetVertex(dxxPointFilters.AtMaxX).X - aPline.GetVertex(dxxPointFilters.AtMinX).X) / 2;
                tOrds = tOrds.Where(o => o >= leftLimit + halfTabWidth && o <= rightLimit - halfTabWidth).ToList();

                dxfVector v1 = aPline.Vertex(5, true);
                dxfVector v2 = dxfVector.Zero;
                dxePolyline bPline = null;
                dxePolyline lPline = null;
                dxfDisplaySettings dsp = new dxfDisplaySettings();
                UVECTOR u1;
                double oset = Math.Abs(v1.Y);
                double d1 = 2 * v1.X;

                if (bVertical)
                {
                    aPline.Rotate(-90);

                    for (int i = 1; i <= tOrds.Count; i++)
                    {
                        bPline = new dxePolyline(aPline.Vertices, bClosed: true) { Tag = "TABSLOT" }; // (dxePolygon)aPline.Clone();



                        //bPline.Move(aOrdinate, tOrds[i - 1]);


                        ////slotSegs = bPline.Segments;

                        u1 = rTabCenters.Add(aOrdinate - mdGlobals.DefaultPanelClearance, tOrds[i - 1]);
                        _rVal.AddPoint(u1.X, u1.Y, aTag: "TABSLOT", aFlag: "CENTER");
                        bPline.SetCoordinates(u1.X, u1.Y);
                        _rVal.Add(bPline);


                        if (i == 1)
                        {
                            v1 = bPline.Vertex(5, bReturnClone: true);
                            v2.SetCoordinates(v1.X, aLimit2);
                            v2.Y = aLimit2;

                            _rVal.AddLine(v1, v2, dsp, aTag: "TABBEND");
                            v1.X += 2 * oset;
                            v2.X += 2 * oset;
                            _rVal.AddLine(v1, v2, dsp, aTag: "TABBEND");
                        }
                        else
                        {
                            if (tOrds.Count > 1)
                            {
                                v1 = lPline.Vertex(5, bReturnClone: true);
                                v1.Y += d1;
                                v2 = bPline.Vertex(5, bReturnClone: true);
                                _rVal.AddLine(v1, v2, dsp, aTag: "TABBEND");
                                v1.X += 2 * oset;
                                v2.X += 2 * oset;
                                _rVal.AddLine(v1, v2, dsp, aTag: "TABBEND");
                            }
                            if (i == tOrds.Count)
                            {
                                v1 = bPline.Vertex(5, bReturnClone: true);
                                v1.Y += d1;
                                v2.SetCoordinates(v1.X, aLimit2);
                                v2.Y = aLimit1;
                                _rVal.AddLine(v1, v2, dsp, aTag: "TABBEND");
                                v1.X += 2 * oset;
                                v2.X += 2 * oset;
                                _rVal.AddLine(v1, v2, dsp, aTag: "TABBEND");
                            }
                        }
                        lPline = bPline;
                    }
                }
                else
                {
                    double f1 = bFaceUp ? -1 : 1;
                    if (bFaceUp) aPline.Rotate(f1 * 180);
                    aPline.Move(aYChange: f1 * 0.825);
                    mzUtils.SortTwoValues(true, ref aLimit1, ref aLimit2);
                    for (int i = 1; i <= tOrds.Count; i++)
                    {
                        bPline = new dxePolyline(aPline.Vertices, bClosed: true) { Tag = "TABSLOT" }; //(dxePolygon)aPline.Clone();



                        u1 = rTabCenters.Add(tOrds[i - 1], aOrdinate + f1 * mdGlobals.DefaultPanelClearance);
                        _rVal.AddPoint(u1.X, u1.Y, aTag: "TABSLOT", aFlag: "CENTER");
                        bPline.SetCoordinates(u1.X, u1.Y);

                        _rVal.Add(bPline);

                        if (i == 1)
                        {
                            v1 = bPline.GetVertex(dxxPointFilters.AtMinX, bReturnClone: true);

                            v2.SetCoordinates(aLimit1, v1.Y);
                            _rVal.AddLine(v1, v2, dsp, aTag: "TABBEND");
                            v1.Y += f1 * 2 * oset;
                            v2.Y += f1 * 2 * oset;
                            _rVal.AddLine(v1, v2, dsp, aTag: "TABBEND");
                        }
                        else
                        {
                            if (tOrds.Count > 1)
                            {
                                v1 = lPline.GetVertex(dxxPointFilters.AtMaxX, bReturnClone: true);
                                v2 = bPline.GetVertex(dxxPointFilters.AtMinX, bReturnClone: true);

                                _rVal.AddLine(v1, v2, dsp, aTag: "TABBEND");
                                v1.Y += f1 * 2 * oset;
                                v2.Y += f1 * 2 * oset;
                                _rVal.AddLine(v1, v2, dsp, aTag: "TABBEND");

                            }
                        }

                        if (i == tOrds.Count)
                        {
                            v1 = bPline.GetVertex(dxxPointFilters.AtMaxX, bReturnClone: true);
                            v2 = new dxfVector(aLimit2, v1.Y);


                            _rVal.AddLine(v1, v2, dsp, aTag: "TABBEND");
                            v1.Y += f1 * 2 * oset;
                            v2.Y += f1 * 2 * oset;
                            _rVal.AddLine(v1, v2, dsp, aTag: "TABBEND");

                        }
                        lPline = bPline;
                    }
                }

                
                return _rVal;
            }
            catch (Exception)
            {
                return _rVal;
            }
        }

        private static dxePolygon xxGetSplicePolygon(mdTrayAssembly aAssy, mdDeckSection aSection, uopDeckSplice aSplice, bool SpliceOnTop, bool JoggleUp = false, bool bFoldovers = false)
        {
            if (aSplice == null) return null;

           

            bool bUp = SpliceOnTop;
            bool LtoR = false;

            if (!aSplice.Vertical)
            {
                switch (aSplice.SpliceType)
                {
                    //===========================================================
                    case uppSpliceTypes.SpliceWithJoggle:
                        //===========================================================
                        bUp = (!aSplice.Female) ? aSplice.Ordinate <= 0 : aSplice.Ordinate > 0;
                        LtoR = !SpliceOnTop;
                        //===========================================================
                        break;
                    case uppSpliceTypes.SpliceWithTabs:
                        LtoR = !SpliceOnTop;
                        break;
                    default:
                        LtoR = !SpliceOnTop;
                        break;
                }
            }



            dxePolygon _rVal = aSplice.Female ? xaSplice_Female(aAssy, aSection, aSplice, bUp, LtoR, bFoldovers) : xaSplice_Male(aAssy, aSection, aSplice, bUp, LtoR, bFoldovers);
            _rVal.Closed = false;
            return _rVal;
        }

        private static void xxGetSplicePolygons(mdTrayAssembly aAssy, mdDeckSection aSection, uopDeckSplice aTopSplice, uopDeckSplice aBottomSplice, out dxePolygon rTPgon, out dxePolygon rBPgon, out bool rContainsArcs, bool bFoldovers)
        {
            rContainsArcs = false;
            rTPgon = null;
            rBPgon = null;
            dxfVector v1;
            dxfVector v2;
            double rad = aSection.Radius;

            double cX = aSection.X;
            bool LtoR = aBottomSplice == null;

            if (rad <= 0)
            {
                aAssy ??= aSection.GetMDTrayAssembly(aAssy);

                if (aAssy != null) aSection.Radius = aAssy.DeckRadius;
                rad = aSection.Radius;
            }


            aSection.Radius = rad;

            double wd = aSection.Width;



            rTPgon = xxGetSplicePolygon(aAssy, aSection, aTopSplice, true, LtoR, bFoldovers: bFoldovers);
            if (rTPgon != null)
            {
                LtoR = !mzUtils.VarToBoolean(rTPgon.Value);
            }
            rBPgon = xxGetSplicePolygon(aAssy, aSection, aBottomSplice, false, LtoR, bFoldovers: bFoldovers);

            if (aSection.IsHalfMoon)
            {
                bool rightHalfMoon = aSection.X > 0;
                if (rightHalfMoon)
                {
                    if (rBPgon == null)
                    {
                        rContainsArcs = true;
                        rBPgon = new dxePolygon();
                        v1 = new dxfVector(aSection.Limits.Left, aSection.Limits.Top);
                        v2 = v1.Clone();
                        v2.X += 0.5;
                        v2.Y = Math.Sqrt(Math.Pow(rad, 2) - Math.Pow(v2.X, 2));
                        v1.Y = v2.Y;

                        rBPgon.AddVertex(v2.X, v2.Y);
                        rBPgon.AddVertex(v1.X, v2.Y);

                        rBPgon.AddVertex(v1.X, -v2.Y);
                        rBPgon.AddVertex(v2.X, -v2.Y);
                        if (rTPgon != null)
                        {
                            v1 = rTPgon.GetVertex(dxxPointFilters.AtMaxY);
                            v1.VertexRadius = rad;
                        }
                    }
                    if (rTPgon == null)
                    {
                        rContainsArcs = true;
                        rTPgon = new dxePolygon();
                        v1 = rBPgon.GetVertex(dxxPointFilters.GetRightBottom);
                        v1.VertexRadius = rad;
                        rTPgon.AddVertex(v1.X, v1.Y);
                        rTPgon.AddVertex(rad, 0);
                    }
                }
                else
                {
                    if (rBPgon == null)
                    {
                        rContainsArcs = true;
                        rBPgon = new dxePolygon();
                        v1 = new dxfVector(aSection.Limits.Right, aSection.Limits.Bottom);
                        v2 = v1.Clone();
                        v2.X -= 0.5;
                        v2.Y = -Math.Sqrt(Math.Pow(rad, 2) - Math.Pow(v2.X, 2));
                        v1.Y = v2.Y;

                        rBPgon.AddVertex(v2.X, v2.Y);
                        rBPgon.AddVertex(v1.X, v2.Y);

                        rBPgon.AddVertex(v1.X, -v2.Y);
                        rBPgon.AddVertex(v2.X, -v2.Y);
                        if (rTPgon != null)
                        {
                            v1 = rTPgon.GetVertex(dxxPointFilters.AtMinY);
                            v1.VertexRadius = rad;
                        }
                    }
                    if (rTPgon == null)
                    {
                        rContainsArcs = true;
                        rTPgon = new dxePolygon();
                        v1 = rBPgon.GetVertex(dxxPointFilters.GetLeftTop);
                        v1.VertexRadius = rad;
                        rTPgon.AddVertex(v1.X, v1.Y);
                        rTPgon.AddVertex(-rad, 0);
                    }
                }
                //===========================================
            }
            else
            {
                //===========================================
                if (rBPgon == null && rTPgon == null)
                {
                    // If there is no splicer, we just use the panel shape.
                    rTPgon = new dxePolygon();
                    rBPgon = new dxePolygon();

                    var panelShape = aSection.ParentShape; // mdDeckPanel.GetPanelShapeUsingPanelAndShapeIndices(aAssy, aSection.PanelIndex, aSection.PanelShapeIndex);
                    rBPgon = new dxePolygon(panelShape.Vertices);
                }
                else if (rBPgon != null && rTPgon != null)
                {
                    // If we have both splicers, we should attach them. The top part will replace some of the original shapes vertices. The same is true for the bottom part.
                    // We find the shape vertices that are between the top and bottom splicer ordinates.
                    // Here we use this criteria which is not 100% correct in general cases but it works for most cases in this scenario.
                    // As the vertices are counter clockwise, we append the vertices in the left half to the top polygon and in the right half to the bottom polygon.
                    var panelShape = aSection.ParentShape; //mdDeckPanel.GetPanelShapeUsingPanelAndShapeIndices(aAssy, aSection.PanelIndex, aSection.PanelShapeIndex);
                    var verticiesBetweenPolygons = panelShape.Perimeter().Vertices.Where(v => v.Y < aTopSplice.Ordinate && v.Y > aBottomSplice.Ordinate).ToArray();
                    var appendToTop = verticiesBetweenPolygons.Where(v => v.X < aSection.X).ToArray();
                    var appendToBottom = verticiesBetweenPolygons.Where(v => v.X > aSection.X).ToArray();
                    if (appendToTop.Length > 0)
                    {
                        rTPgon.Vertices.Append(appendToTop);
                    }
                    if (appendToBottom.Length > 0)
                    {
                        rBPgon.Vertices.Append(appendToBottom);
                    }

                    List<dxfVector> arcpnts = rTPgon.Vertices.FindAll((x) => Math.Round(x.Length, 3) == Math.Round(rad, 3));
                    if (arcpnts.Count > 0) rContainsArcs = true;
                    arcpnts = rBPgon.Vertices.FindAll((x) => Math.Round(x.Length, 3) == Math.Round(rad, 3));
                    if (arcpnts.Count > 0) rContainsArcs = true;
                }
                else
                {
                    // The same idea as the case with both top and bottom about cutting the shape using splicer ordinate.
                    // However, here we need to make sure the order or adding the vertices are correct, specially when there is only the bottom splicer.
                    if (rBPgon == null)
                    {
                        v1 = rTPgon.Vertex(1, bReturnClone: true);
                        v2 = rTPgon.Vertex(rTPgon.VertexCount, bReturnClone: true);
                        var spliceY = Math.Min(v1.Y, v2.Y);

                        var panelShape = aSection.ParentShape; // mdDeckPanel.GetPanelShapeUsingPanelAndShapeIndices(aAssy, aSection.PanelIndex, aSection.PanelShapeIndex);

                        List<uopVector> bottomSideShapeVertices = panelShape.Vertices.Where(v => v.Y < spliceY).ToList();
                        
                        rContainsArcs = bottomSideShapeVertices.Any(v => v.Radius != 0);

                        rBPgon = new dxePolygon();
                        foreach (var v in bottomSideShapeVertices)
                        {
                            rBPgon.AddVertex(v.X, v.Y, aVertexRadius: v.Radius);
                        }
                    }

                    if (rTPgon == null)
                    {
                        v1 = rBPgon.Vertex(1, bReturnClone: true);
                        v2 = rBPgon.Vertex(rBPgon.VertexCount, bReturnClone: true);
                        var spliceY = Math.Max(v1.Y, v2.Y);

                        var panelShape = aSection.ParentShape; // mdDeckPanel.GetPanelShapeUsingPanelAndShapeIndices(aAssy, aSection.PanelIndex, aSection.PanelShapeIndex);

                        // Here we go over the shape vertices counter clockwise. As soon as the Y goes below the splicer ordinate, we switch to adding them to the other set.
                        // This way we can add the vertices in the proper order.
                        List<uopVector> before = new List<uopVector>();
                        List<uopVector> after = new List<uopVector>();
                        bool addToBefore = true;
                        foreach (var vertex in panelShape.Vertices)
                        {
                            if (vertex.Y <= spliceY)
                            {
                                addToBefore = false;
                            }
                            else
                            {
                                if (addToBefore)
                                {
                                    before.Add(vertex);
                                }
                                else
                                {
                                    after.Add(vertex);
                                }
                            }
                        }

                        List<uopVector> topSideShapeVertices = after.Concat(before).ToList();

                        rContainsArcs = topSideShapeVertices.Any(v => v.Radius != 0);

                        rTPgon = new dxePolygon();
                        foreach (var v in topSideShapeVertices)
                        {
                            rTPgon.AddVertex(v.X, v.Y, aVertexRadius: v.Radius);
                        }
                    }
                }
            }

            rTPgon.Tag = "TOP";
            rBPgon.Tag = "BOTTOM";
            rBPgon.Vertex(1).Tag = "BASEPT";

        }

        private static dxePolygon GetSplicePolygon(mdTrayAssembly aAssy, uopSectionShape aSection, uopDeckSplice aSplice, bool SpliceOnTop, bool JoggleUp = false, bool bFoldovers = false)
        {
            if (aSplice == null) return null;



            bool bUp = SpliceOnTop;
            bool LtoR = false;

            if (!aSplice.Vertical)
            {
                switch (aSplice.SpliceType)
                {
                    //===========================================================
                    case uppSpliceTypes.SpliceWithJoggle:
                        //===========================================================
                        bUp = (!aSplice.Female) ? aSplice.Ordinate <= 0 : aSplice.Ordinate > 0;
                        LtoR = !SpliceOnTop;
                        //===========================================================
                        break;
                    case uppSpliceTypes.SpliceWithTabs:
                        LtoR = !SpliceOnTop;
                        break;
                    default:
                        LtoR = !SpliceOnTop;
                        break;
                }
            }



           // dxePolygon _rVal = aSplice.Female ? xaSplice_Female(aAssy, aSection, aSplice, bUp, LtoR, bFoldovers) : xaSplice_Male(aAssy, aSection, aSplice, bUp, LtoR, bFoldovers);
           // _rVal.Closed = false;
            return null; //_rVal
        }
        private static void GetSplicePolygons(mdTrayAssembly aAssy, uopSectionShape aSection,  out dxePolygon rTPgon, out dxePolygon rBPgon, out bool rContainsArcs, bool bFoldovers)
        {
            rContainsArcs = false;
            rTPgon = null;
            rBPgon = null;
            dxfVector v1;
            dxfVector v2;
            double rad = aSection.DeckRadius;
            double wd = aSection.Width;

            aSection.RectifySplices(out uopDeckSplice aTopSplice, out uopDeckSplice aBottomSplice);

            double cX = aSection.X;
            bool LtoR = aBottomSplice == null;

            rTPgon = GetSplicePolygon(aAssy, aSection, aTopSplice, true, LtoR, bFoldovers: bFoldovers);
            if (rTPgon != null)
            {
                LtoR = !mzUtils.VarToBoolean(rTPgon.Value);
            }
            rBPgon = GetSplicePolygon(aAssy, aSection, aBottomSplice, false, LtoR, bFoldovers: bFoldovers);

            if (aSection.IsHalfMoon)
            {
                bool rightHalfMoon = aSection.X > 0;
                if (rightHalfMoon)
                {
                    if (rBPgon == null)
                    {
                        rContainsArcs = true;
                        rBPgon = new dxePolygon();
                        v1 = new dxfVector(aSection.Left, aSection.Top);
                        v2 = v1.Clone();
                        v2.X += 0.5;
                        v2.Y = Math.Sqrt(Math.Pow(rad, 2) - Math.Pow(v2.X, 2));
                        v1.Y = v2.Y;

                        rBPgon.AddVertex(v2.X, v2.Y);
                        rBPgon.AddVertex(v1.X, v2.Y);

                        rBPgon.AddVertex(v1.X, -v2.Y);
                        rBPgon.AddVertex(v2.X, -v2.Y);
                        if (rTPgon != null)
                        {
                            v1 = rTPgon.GetVertex(dxxPointFilters.AtMaxY);
                            v1.VertexRadius = rad;
                        }
                    }
                    if (rTPgon == null)
                    {
                        rContainsArcs = true;
                        rTPgon = new dxePolygon();
                        v1 = rBPgon.GetVertex(dxxPointFilters.GetRightBottom);
                        v1.VertexRadius = rad;
                        rTPgon.AddVertex(v1.X, v1.Y);
                        rTPgon.AddVertex(rad, 0);
                    }
                }
                else
                {
                    if (rBPgon == null)
                    {
                        rContainsArcs = true;
                        rBPgon = new dxePolygon();
                        v1 = new dxfVector(aSection.Right, aSection.Bottom);
                        v2 = v1.Clone();
                        v2.X -= 0.5;
                        v2.Y = -Math.Sqrt(Math.Pow(rad, 2) - Math.Pow(v2.X, 2));
                        v1.Y = v2.Y;

                        rBPgon.AddVertex(v2.X, v2.Y);
                        rBPgon.AddVertex(v1.X, v2.Y);

                        rBPgon.AddVertex(v1.X, -v2.Y);
                        rBPgon.AddVertex(v2.X, -v2.Y);
                        if (rTPgon != null)
                        {
                            v1 = rTPgon.GetVertex(dxxPointFilters.AtMinY);
                            v1.VertexRadius = rad;
                        }
                    }
                    if (rTPgon == null)
                    {
                        rContainsArcs = true;
                        rTPgon = new dxePolygon();
                        v1 = rBPgon.GetVertex(dxxPointFilters.GetLeftTop);
                        v1.VertexRadius = rad;
                        rTPgon.AddVertex(v1.X, v1.Y);
                        rTPgon.AddVertex(-rad, 0);
                    }
                }
                //===========================================
            }
            else
            {
                //===========================================
                if (rBPgon == null && rTPgon == null)
                {
                    // If there is no splicer, we just use the panel shape.
                    rTPgon = new dxePolygon();
                    rBPgon = new dxePolygon();

                    var panelShape = aSection.ParentShape; 
                    rBPgon = new dxePolygon(panelShape.Vertices);
                }
                else if (rBPgon != null && rTPgon != null)
                {
                    // If we have both splicers, we should attach them. The top part will replace some of the original shapes vertices. The same is true for the bottom part.
                    // We find the shape vertices that are between the top and bottom splicer ordinates.
                    // Here we use this criteria which is not 100% correct in general cases but it works for most cases in this scenario.
                    // As the vertices are counter clockwise, we append the vertices in the left half to the top polygon and in the right half to the bottom polygon.
                    var panelShape = aSection.ParentShape; //mdDeckPanel.GetPanelShapeUsingPanelAndShapeIndices(aAssy, aSection.PanelIndex, aSection.PanelShapeIndex);
                    var verticiesBetweenPolygons = panelShape.Vertices.Where(v => v.Y < aTopSplice.Ordinate && v.Y > aBottomSplice.Ordinate).ToArray();
                    var appendToTop = verticiesBetweenPolygons.Where(v => v.X < aSection.X).ToArray();
                    var appendToBottom = verticiesBetweenPolygons.Where(v => v.X > aSection.X).ToArray();
                    if (appendToTop.Length > 0)
                    {
                        rTPgon.Vertices.Append(appendToTop);
                    }
                    if (appendToBottom.Length > 0)
                    {
                        rBPgon.Vertices.Append(appendToBottom);
                    }

                    List<dxfVector> arcpnts = rTPgon.Vertices.FindAll((x) => Math.Round(x.Length, 3) == Math.Round(rad, 3));
                    if (arcpnts.Count > 0) rContainsArcs = true;
                    arcpnts = rBPgon.Vertices.FindAll((x) => Math.Round(x.Length, 3) == Math.Round(rad, 3));
                    if (arcpnts.Count > 0) rContainsArcs = true;
                }
                else
                {
                    // The same idea as the case with both top and bottom about cutting the shape using splicer ordinate.
                    // However, here we need to make sure the order or adding the vertices are correct, specially when there is only the bottom splicer.
                    if (rBPgon == null)
                    {
                        v1 = rTPgon.Vertex(1, bReturnClone: true);
                        v2 = rTPgon.Vertex(rTPgon.VertexCount, bReturnClone: true);
                        var spliceY = Math.Min(v1.Y, v2.Y);

                        var panelShape = aSection.ParentShape; // mdDeckPanel.GetPanelShapeUsingPanelAndShapeIndices(aAssy, aSection.PanelIndex, aSection.PanelShapeIndex);

                        List<uopVector> bottomSideShapeVertices = panelShape.Vertices.Where(v => v.Y < spliceY).ToList();

                        rContainsArcs = bottomSideShapeVertices.Any(v => v.Radius != 0);

                        rBPgon = new dxePolygon();
                        foreach (var v in bottomSideShapeVertices)
                        {
                            rBPgon.AddVertex(v.X, v.Y, aVertexRadius: v.Radius);
                        }
                    }

                    if (rTPgon == null)
                    {
                        v1 = rBPgon.Vertex(1, bReturnClone: true);
                        v2 = rBPgon.Vertex(rBPgon.VertexCount, bReturnClone: true);
                        var spliceY = Math.Max(v1.Y, v2.Y);

                        var panelShape = aSection.ParentShape; // mdDeckPanel.GetPanelShapeUsingPanelAndShapeIndices(aAssy, aSection.PanelIndex, aSection.PanelShapeIndex);

                        // Here we go over the shape vertices counter clockwise. As soon as the Y goes below the splicer ordinate, we switch to adding them to the other set.
                        // This way we can add the vertices in the proper order.
                        List<uopVector> before = new List<uopVector>();
                        List<uopVector> after = new List<uopVector>();
                        bool addToBefore = true;
                        foreach (var vertex in panelShape.Vertices)
                        {
                            if (vertex.Y <= spliceY)
                            {
                                addToBefore = false;
                            }
                            else
                            {
                                if (addToBefore)
                                {
                                    before.Add(vertex);
                                }
                                else
                                {
                                    after.Add(vertex);
                                }
                            }
                        }

                        List<uopVector> topSideShapeVertices = after.Concat(before).ToList();

                        rContainsArcs = topSideShapeVertices.Any(v => v.Radius != 0);

                        rTPgon = new dxePolygon();
                        foreach (var v in topSideShapeVertices)
                        {
                            rTPgon.AddVertex(v.X, v.Y, aVertexRadius: v.Radius);
                        }
                    }
                }
            }

            rTPgon.Tag = "TOP";
            rBPgon.Tag = "BOTTOM";
            rBPgon.Vertex(1).Tag = "BASEPT";

        }


        /// <summary>
        /// returns a dxePolygon that is used to draw the layout view of the section
        /// this view includes the details of the splices of the section
        /// </summary>
        /// <param name="aSection"></param>
        /// <param name="aAssy"></param>
        /// <param name="rSplicePolygons"></param>
        /// <param name="aSplices"></param>
        /// <returns></returns>
        public static void CreateSectionPerimeters(uopSectionShapes aSections, bool bUpdateSpliceFlangeLines = false, bool bSuppressEvents = false)
        {
            try
            {
                Reset();
                if (aSections == null) return;
                mdTrayAssembly aAssy = aSections.GetMDTrayAssembly();

                if (aAssy == null) return;
                if (aAssy.Downcomer().Count <= 0) return;

                foreach (var section in aSections)
                {
                    mdDeckPanel aDP = aAssy.DeckPanels.Item(section.PanelIndex);

                    if (!bSuppressEvents) aAssy.RaiseStatusChangeEvent($"Creating {aAssy.TrayName()}  Deck Section {section.Handle} Perimeter");

                    bool leftSide = aDP.X < 0;
                    double jht = aAssy.DesignOptions.JoggleAngle;
                    bool foldovers = aAssy.HasFoldovers && aAssy.DesignOptions.SpliceStyle == uppSpliceStyles.Tabs;

    
                    //get the splices
                    section.RectifySplices(out uopDeckSplice tSplice, out uopDeckSplice bSplice);


                    //get the splice polygons
                    GetSplicePolygons(aAssy, section,  out dxePolygon TPgon, out dxePolygon bPGon, out bool containsarcs, foldovers);

                    //if (rSplicePolygons != null)
                    //{
                    //    if (TPgon != null) rSplicePolygons.Add(TPgon);
                    //    if (bPGon != null) rSplicePolygons.Add(bPGon);
                    //}

                    //update the splice flange lines
                    //if (aSplices != null && bUpdateSpliceFlangeLines)
                    //{

                    //    if (tSplice != null)
                    //    {
                    //        uopDeckSplice assySplc = aSplices.Find((x) => x.Handle == tSplice.Handle);
                    //        if (assySplc != null)
                    //            assySplc.FlangeLine = new ULINE(tSplice.FlangeLine);

                    //    }
                    //    if (bSplice != null)
                    //    {
                    //        uopDeckSplice assySplc = aSplices.Find((x) => x.Handle == bSplice.Handle);
                    //        if (assySplc != null)
                    //            assySplc.FlangeLine = new ULINE(bSplice.FlangeLine);
                    //    }
                    //}

                    //if (bSplice != null)
                    //{
                    //    if (bid == 1)
                    //        section.Splice1 = bSplice;
                    //    else
                    //        section.Splice2 = bSplice;
                    //}
                    //if (tSplice != null)
                    //{
                    //    if (tid == 1)
                    //        section.Splice1 = tSplice;
                    //    else
                    //        section.Splice2 = tSplice;
                    //}



                    //create the perimeter by joining the bottom and top polygons
                    dxePolygon perimeter = new dxePolygon();

                    colDXFVectors verts = bPGon.Vertices.Clone();
                    if (section.IsHalfMoon && section.SectionIndex > 1)
                    {
                        dxfVector v1 = verts.GetVector(dxxPointFilters.GetBottomRight);
                        if (v1 != null) v1.VertexRadius = aDP.Radius;
                    }
                    colDXFVectors vertst = TPgon.Vertices.Clone();

                    verts.Append(vertst, bAppendClones: true);
                    List<dxfVector> rmv = verts.RemoveCoincidentVectors();


                    if (containsarcs)
                    {
                        // We set the radius (if needed) for non half-moon shapes when creating their polygons. Therefore, we do not need to do it here any more.
                        // section.LapsRing = true;
                        dxxPointFilters search;
                        if (section.IsHalfMoon)
                        {
                            search = section.IsHalfMoon ? dxxPointFilters.GetRightTop : dxxPointFilters.GetLeftTop;

                            //section.LapsRing = true;
                            search = (section.Y >= 0 || section.PanelIndex == 1) ? dxxPointFilters.GetRightTop : dxxPointFilters.GetBottomLeft;
                            dxfVector v1 = verts.GetVector(search);
                            if (v1 != null) v1.VertexRadius = aDP.Radius;
                            if (!section.IsHalfMoon && section.Y < 0)
                            {
                                search = dxxPointFilters.GetRightBottom;
                                v1 = verts.GetVector(search);
                                if (v1 != null) v1.VertexRadius = 0;
                            }
                        }
                    }
                    perimeter.Vertices = verts;

                    //to capture the slot in a slot and tab or the thickness lines of a integral joint
                    perimeter.AdditionalSegments.Append(bPGon.AdditionalSegments);
                    perimeter.AdditionalSegments.Append(TPgon.AdditionalSegments);

                    //:done
                    //dxfRectangle bounds = perimeter.Vertices.BoundingRectangle();
                    //section.Center = UVECTOR.FromDXFVector(bounds.Center);
                    perimeter.InsertionPt = new dxfVector(section.X, section.Y, 0.5 * section.Thickness);

                    if (section.IsManway)
                    {



                        dxfVector v1 = perimeter.Vertices.GetVector(dxxPointFilters.GetLeftTop);
                        if (v1 != null) perimeter.FilletAtVertex(v1, 0.5);

                        v1 = perimeter.Vertices.GetVector(dxxPointFilters.GetLeftBottom);
                        if (v1 != null) perimeter.FilletAtVertex(v1, 0.5);

                        v1 = perimeter.Vertices.GetVector(dxxPointFilters.GetRightTop);
                        if (v1 != null) perimeter.FilletAtVertex(v1, 0.5);

                        v1 = perimeter.Vertices.GetVector(dxxPointFilters.GetRightBottom);
                        if (v1 != null) perimeter.FilletAtVertex(v1, 0.5);

                    }

                   


                }



            }
            catch (Exception e)
            {
                if (uopUtils.RunningInIDE) Debug.Fail(e.Message);
            }
            finally
            {

                //if (perimeter != null)
                //{
                //    dxfRectangle aRec = perimeter.Vertices.BoundingRectangle(null, true);
                //    section.Width = aRec.Width;
                //    section.Height = aRec.Height;
                //    section.PerimeterPtsV = UVECTORS.FromDXFVectors(perimeter.Vertices);
                //    perimeter.InsertionPt.SetCoordinates(section.X, section.Y);

                //    //determine if laps beam
                //    for (int vi = 0; vi < perimeter.Vertices.Count; vi++)
                //    {
                //        var v1 = perimeter.Vertices[vi];
                //        if (v1.Radius != 0.0) continue;

                //        var v2 = perimeter.Vertices[(vi + 1) % perimeter.Vertices.Count];
                //        double dx = Math.Abs(v1.X - v2.X);
                //        double dy = Math.Abs(v1.Y - v2.Y);
                //        if (Math.Abs(dx - dy) < 0.001) section.LapsBeam = true;
                //    }
                //}
                //aAssy = null;

            }
            //return perimeter;
        }
        /// <summary>
        /// returns a dxePolygon that is used to draw the layout view of the section
        /// this view includes the details of the splices of the section
        /// </summary>
        /// <param name="aSection"></param>
        /// <param name="aAssy"></param>
        /// <param name="rSplicePolygons"></param>
        /// <param name="aSplices"></param>
        /// <returns></returns>
        public static dxePolygon CreateSectionPerimeter(mdDeckSection aSection, mdTrayAssembly aAssy = null, colDXFEntities rSplicePolygons = null, List<uopDeckSplice> aSplices = null, bool bUpdateSpliceFlangeLines = false, bool bSuppressEvents = false)
        {
            dxePolygon _rVal = null;
            try
            {
                Reset();
                if (aSection == null) return _rVal;
                aAssy ??= aSection.GetMDTrayAssembly();

                if (aAssy == null) return _rVal;
                if (aAssy.Downcomer().Count <= 0) return _rVal;

                mdDeckPanel aDP = aAssy.DeckPanels.Item(aSection.PanelIndex);
                if (aDP == null) return _rVal;

                if (!bSuppressEvents) aAssy.RaiseStatusChangeEvent($"Creating {aAssy.TrayName()}  Deck Section {aSection.Handle} Perimeter");

                bool leftSide = aDP.X < 0;
                double jht = aAssy.DesignOptions.JoggleAngle;
                bool foldovers = aAssy.HasFoldovers && aAssy.DesignOptions.SpliceStyle == uppSpliceStyles.Tabs;

                //initialize

                if (bUpdateSpliceFlangeLines) aSplices ??= aAssy.DeckSplices;

          

                //get the splices
                aSection.GetSplices(out uopDeckSplice tSplice, out uopDeckSplice bSplice);
         
                aSection.ManholeID = aAssy.ManholeID;


                //get the splice polygons
                xxGetSplicePolygons(aAssy, aSection, tSplice, bSplice, out dxePolygon TPgon, out dxePolygon bPGon, out bool containsarcs, foldovers);

                if (rSplicePolygons != null)
                {
                    if (TPgon != null) rSplicePolygons.Add(TPgon);
                    if (bPGon != null) rSplicePolygons.Add(bPGon);
                }

                //update the splice flange lines
                if (aSplices != null && bUpdateSpliceFlangeLines)
                {

                    if (tSplice != null)
                    {
                        uopDeckSplice assySplc = aSplices.Find((x) => x.Handle == tSplice.Handle);
                        if (assySplc != null)
                            assySplc.FlangeLine = new uopFlangeLine(tSplice.FlangeLine);

                    }
                    if (bSplice != null)
                    {
                        uopDeckSplice assySplc = aSplices.Find((x) => x.Handle == bSplice.Handle);
                        if (assySplc != null)
                            assySplc.FlangeLine = new uopFlangeLine(bSplice.FlangeLine);
                    }
                }

                //if (bSplice != null)
                //{
                //    if (bid == 1)
                //        aSection.Splice1 = bSplice;
                //    else
                //        aSection.Splice2 = bSplice;
                //}
                //if (tSplice != null)
                //{
                //    if (tid == 1)
                //        aSection.Splice1 = tSplice;
                //    else
                //        aSection.Splice2 = tSplice;
                //}



                //create the perimeter by joining the bottom and top polygons
                _rVal = new dxePolygon();

                colDXFVectors verts = bPGon.Vertices.Clone();
                if (aSection.IsHalfMoon && aSection.SectionIndex > 1)
                {
                    dxfVector v1 = verts.GetVector(dxxPointFilters.GetBottomRight);
                    if (v1 != null) v1.VertexRadius = aDP.Radius;
                }
                colDXFVectors vertst = TPgon.Vertices.Clone();

                verts.Append(vertst, bAppendClones: true);
                List<dxfVector> rmv = verts.RemoveCoincidentVectors();


                if (containsarcs)
                {
                    // We set the radius (if needed) for non half-moon shapes when creating their polygons. Therefore, we do not need to do it here any more.
                    // aSection.LapsRing = true;
                    dxxPointFilters search;
                    if (aSection.IsHalfMoon)
                    {
                        search = aSection.IsHalfMoon ? dxxPointFilters.GetRightTop : dxxPointFilters.GetLeftTop;

                        //aSection.LapsRing = true;
                        search = (aSection.Y >= 0 || aSection.PanelIndex == 1) ? dxxPointFilters.GetRightTop : dxxPointFilters.GetBottomLeft;
                        dxfVector v1 = verts.GetVector(search);
                        if (v1 != null) v1.VertexRadius = aDP.Radius;
                        if (!aSection.IsHalfMoon && aSection.Y < 0)
                        {
                            search = dxxPointFilters.GetRightBottom;
                            v1 = verts.GetVector(search);
                            if (v1 != null) v1.VertexRadius = 0;
                        }
                    }
                }
                _rVal.Vertices = verts;

                //to capture the slot in a slot and tab or the thickness lines of a integral joint
                _rVal.AdditionalSegments.Append(bPGon.AdditionalSegments);
                _rVal.AdditionalSegments.Append(TPgon.AdditionalSegments);

                //:done
                //dxfRectangle bounds = _rVal.Vertices.BoundingRectangle();
                //aSection.Center = UVECTOR.FromDXFVector(bounds.Center);
                _rVal.InsertionPt = new dxfVector(aSection.X, aSection.Y, aSection.Z);

                if (aSection.IsManway)
                {



                    dxfVector v1 = _rVal.Vertices.GetVector(dxxPointFilters.GetLeftTop);
                    if (v1 != null) _rVal.FilletAtVertex(v1, 0.5);

                    v1 = _rVal.Vertices.GetVector(dxxPointFilters.GetLeftBottom);
                    if (v1 != null) _rVal.FilletAtVertex(v1, 0.5);

                    v1 = _rVal.Vertices.GetVector(dxxPointFilters.GetRightTop);
                    if (v1 != null) _rVal.FilletAtVertex(v1, 0.5);

                    v1 = _rVal.Vertices.GetVector(dxxPointFilters.GetRightBottom);
                    if (v1 != null) _rVal.FilletAtVertex(v1, 0.5);

                }

                aSection.MechanicalArea = USHAPE.CircleSection(0, 0, aDP.RingRadius, 0, aSection.MechanicalLimits.Left, aSection.MechanicalLimits.Right, aSection.Limits.Top, aSection.Limits.Bottom).Area;

                return _rVal;

            }
            catch (Exception e)
            {
                if (uopUtils.RunningInIDE) Debug.Fail(e.Message);
            }
            finally
            {

                if (_rVal != null)
                {
                    dxfRectangle aRec = _rVal.Vertices.BoundingRectangle(null, true);
                    //aSection.Width = aRec.Width;
                    //aSection.Height = aRec.Height;
                    _rVal.InsertionPt.SetCoordinates(aSection.X, aSection.Y);

                    //determine if laps beam
                    //for (int vi = 0; vi < _rVal.Vertices.Count; vi++)
                    //{
                    //    var v1 = _rVal.Vertices[vi];
                    //    if (v1.Radius != 0.0) continue;

                    //    var v2 = _rVal.Vertices[(vi + 1) % _rVal.Vertices.Count];
                    //    double dx = Math.Abs(v1.X - v2.X);
                    //    double dy = Math.Abs(v1.Y - v2.Y);
                    //}
                }
                aAssy = null;

            }
            return _rVal;
        }

        private static colDXFVectors xxTabVertices(double aMatThk, double aGap, out colDXFEntities rTabBendLines, bool bSimple = false)
        {
            rTabBendLines = new colDXFEntities();
            if (_CurThk != aMatThk || _CurGap != Math.Abs(aGap)) _TabVertices = null;

            _CurThk = aMatThk;
            _CurGap = Math.Abs(aGap);

            if (_TabVertices == null)
            {
                try
                {
                    _TabVertices = new colDXFVectors();
                    _TabBends = new colDXFEntities();
                    dxePolygon aPline = xxSlotPolygon(_CurThk, _CurGap);
                    double rad = aPline.Vertex(1).Radius;

                    dxfVector v0 = aPline.Vertex(5);
                    dxfVector v1 = new dxfVector(1, v0.Y + rad + 1 / 16);
                    dxfVector v2 = new dxfVector(1, v0.Y + mdGlobals.DeckTabHeight);
                    _TabVertices.Add(v0.X, v0.Y, aTag: "TAB", aFlag: "NODIM");
                    if (!bSimple)
                    {
                        _TabVertices.Add(v1.X + 0.125, v0.Y, aVertexRadius: -0.125, aTag: "TAB", aFlag: "NODIM");
                        _TabVertices.Add(v1.X, v0.Y + 0.125, aTag: "TAB", aFlag: "NODIM");
                    }
                    else
                    {
                        _TabVertices.Add(v1.X + 0.125, v0.Y, aVertexRadius: -0.125, aTag: "TAB", aFlag: "NODIM");
                        _TabVertices.Add(v1.X, v0.Y + 0.125, aTag: "TAB", aFlag: "NODIM");
                    }

                    _TabVertices.Add(v1.X, v1.Y, aLineType: dxfLinetypes.Hidden, aTag: "TAB", aFlag: "NODIM");
                    _TabVertices.Add(v2.X, v2.Y, aLineType: dxfLinetypes.Hidden, aTag: "TAB", aFlag: "NODIM");

                    _TabVertices.Add(-v2.X, v2.Y, aLineType: dxfLinetypes.Hidden, aTag: "TAB", aFlag: "NODIM");
                    _TabVertices.Add(-v1.X, v1.Y, aTag: "TAB", aFlag: "NODIM");
                    _TabVertices.Add(-v1.X, v0.Y + 0.125, aVertexRadius: -0.125, aTag: "TAB", aFlag: "NODIM");
                    _TabVertices.Add(-v1.X - 0.125, v0.Y, aTag: "TAB", aFlag: "NODIM");
                    _TabVertices.Add(-v0.X, v0.Y, aTag: "TAB", aFlag: "NODIM");
                    double epy = v0.Y + 0.125;

                    rTabBendLines.AddLine(v1.X, epy, -v1.X, epy, aTag: "TABBEND");
                    epy = v0.Y + 0.125 + 0.1;
                    rTabBendLines.AddLine(v1.X, epy, -v1.X, epy, aTag: "TABBEND");
                    rTabBendLines.AddPoint(0, v2.Y, aTag: "TABPOINT");
                }
                catch (Exception)
                {
                    return _TabVertices;
                }
            }
            _TabBends = rTabBendLines.Clone();
            return _TabVertices;

        }

        public static void Reset()
        {
            _TabVertices = null;
            _SlotPolyline = null;
            _TabBends = null;

        }

        /// <summary>
        /// This method find the first intersection point between the provided flange line and the clearance line from the beam(s).
        /// </summary>
        /// <param name="flangeLine"></param>
        /// <param name="beamClearance"></param>
        /// <param name="assembly"></param>
        /// <returns></returns>
        private static uopVector GetFlangeLineIntersectionWithBeamClearance(uopLine flangeLine, double beamClearance, mdTrayAssembly assembly)
        {
            double flangeLineY = flangeLine.sp.Y;
            bool onSecondLine = false;
            uopVector intersectionPoint = null;

            var beamClearanceLinePairs = uopLinePair.FromList(assembly.DowncomerData.CreateDividerLns(assembly.RingRadius, aOffset: beamClearance));
            var beamClearanceLines = beamClearanceLinePairs.SelectMany(lp => new[] { lp.Line1, lp.Line2 }).ToList();
            
            foreach (var line in beamClearanceLines)
            {
                intersectionPoint = line.IntersectionPt(flangeLine, out _, out _, out _, out onSecondLine, out _);
                if (onSecondLine)
                {
                    break;
                }
            }

            if (onSecondLine)
            {
                return intersectionPoint; 
            }
            else
            {
                return null;
            }
        }

        private static (uopVector, string) GetSpliceIntersectionWithPanelShape(double ordinate, double startX, double endX, uopShape panelShape)
        {
            var horizontalLine = new uopLine(startX, ordinate, endX, ordinate);

            foreach (var segment in panelShape.Segments)
            {
                if (segment.IsArc)
                {
                    var intersections = segment.Arc.Intersections(horizontalLine);
                    if (intersections != null && intersections.Count > 0)
                    {
                        return (intersections.First, "Ring");
                    }
                }
                else
                {
                    var intersection = segment.Line.IntersectionPt(horizontalLine, out _, out _, out bool onFirstLine, out bool onSecondLine, out _);
                    if (onFirstLine && onSecondLine)
                    {
                        if (segment.Line.IsVertical())
                        {
                            return (intersection, "Panel");
                        }
                        else
                        {
                            return (intersection, "Beam");
                        }
                    }
                }
            }

            return (null, "");
        }

        private static ULINE GetTrimmedFlangeLineForMaleJoggleSplice(ULINE currentFlangeLine, double flangeYOut, double flangeYIn, bool intersectsBeamOnLeft, bool intersectsBeamOnRight, bool isUpward, double ringClearance, double beamClearance, mdTrayAssembly assembly)
        {
            // The flangeYOut is the Y of the outermost part of the flange; on the other hand, flangeYIn is the Y of the part of the flange that is closer to the splice.

            ULINE trimmedFlangeLine = currentFlangeLine;

            // Trim the flange line's end point (which is the side closer to the ring in both left and right side of the tray) with ring clearance
            double absoluteFlangeX = Math.Sqrt(Math.Pow(ringClearance, 2) - Math.Pow(flangeYIn, 2));
            if (absoluteFlangeX < Math.Abs(currentFlangeLine.ep.X))
            {
                trimmedFlangeLine.ep.X = currentFlangeLine.ep.X >= 0 ? absoluteFlangeX : -absoluteFlangeX;
            }

            // Trim flange line from the left with beam clearance, if needed
            if (intersectsBeamOnLeft)
            {
                double trimY = isUpward ? flangeYOut : flangeYIn; // If direction is UP, we need to use the top of the flange line to check the intersection
                uopLine uopFlangeLine = uopFlangeLine = new uopLine(trimmedFlangeLine.MinX, trimY, trimmedFlangeLine.MidPt.X, trimY);
                var beamClearanceFlangeIntersection = GetFlangeLineIntersectionWithBeamClearance(uopFlangeLine, beamClearance, assembly);
                if (beamClearanceFlangeIntersection != null)
                {
                    if (trimmedFlangeLine.sp.X < trimmedFlangeLine.ep.X)
                    {
                        if (beamClearanceFlangeIntersection.X > trimmedFlangeLine.sp.X)
                        {
                            trimmedFlangeLine.sp.X = beamClearanceFlangeIntersection.X;
                        }
                    }
                    else
                    {
                        if (beamClearanceFlangeIntersection.X > trimmedFlangeLine.ep.X)
                        {
                            trimmedFlangeLine.ep.X = beamClearanceFlangeIntersection.X;
                        }
                    }
                }
            }

            // Trim flange line from the right with beam clearance, if needed
            if (intersectsBeamOnRight)
            {
                double trimY = isUpward ? flangeYIn : flangeYOut; // If direction is DOWN, we need to use the bottom of the flange line to check the intersection
                uopLine uopFlangeLine = uopFlangeLine = new uopLine(trimmedFlangeLine.MidPt.X, trimY, trimmedFlangeLine.MaxX, trimY);
                var beamClearanceFlangeIntersection = GetFlangeLineIntersectionWithBeamClearance(uopFlangeLine, beamClearance, assembly);
                if (beamClearanceFlangeIntersection != null)
                {
                    if (trimmedFlangeLine.sp.X < trimmedFlangeLine.ep.X)
                    {
                        if (beamClearanceFlangeIntersection.X < trimmedFlangeLine.ep.X)
                        {
                            trimmedFlangeLine.ep.X = beamClearanceFlangeIntersection.X;
                        }
                    }
                    else
                    {
                        if (beamClearanceFlangeIntersection.X < trimmedFlangeLine.sp.X)
                        {
                            trimmedFlangeLine.sp.X = beamClearanceFlangeIntersection.X;
                        }
                    }
                }
            }

            return trimmedFlangeLine;
        }

        private static ULINE GetTrimmedFlangeLineForVerticalMaleJoggleSplice(ULINE currentFlangeLine, double flangeXIn, double ringClearance)
        {
            // The flangeXIn is the X of the part of the flange that is closer to the splice.

            ULINE trimmedFlangeLine = currentFlangeLine;

            double absoluteTrimY = Math.Sqrt(Math.Pow(ringClearance, 2) - Math.Pow(flangeXIn, 2));
            if (Math.Round(Math.Abs(currentFlangeLine.ep.Y), 3) >= Math.Round(absoluteTrimY, 3)) // Checks to see if the base of the flange intersects with the trim radius
            {
                trimmedFlangeLine.ep.Y = absoluteTrimY;
                trimmedFlangeLine.sp.Y = -absoluteTrimY;
            }

            return trimmedFlangeLine;
        }

    }
}