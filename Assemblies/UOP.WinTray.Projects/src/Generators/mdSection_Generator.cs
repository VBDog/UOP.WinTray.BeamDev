using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Hosting;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UOP.DXFGraphics;
using UOP.DXFGraphics.Utilities;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Interfaces;
using UOP.WinTray.Projects.Parts;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;

namespace UOP.WinTray.Projects.Generators
{
    public class mdSection_Generator
    {
        /// <summary>
        /// generates the sections of the panel based on the splice centers
        /// if there are no splices then the panel is returned as a whole
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aSplices"></param>
        /// <returns></returns>
        public static List<mdDeckSection> GenerateDeckSections(mdTrayAssembly aAssy,  uopDeckSplices aSplices, bool bVerbose = true, bool bForAltRing = false, bool bCreatePerimeters = true, mdDeckSections aCollector = null)
        {

            List<mdDeckSection> _rVal = new List<mdDeckSection>();
            try
            {



                if (aAssy == null) return _rVal;
                if (aAssy.ProjectType != uppProjectTypes.MDDraw) return _rVal;

                DowncomerDataSet dcdata = aAssy.DowncomerData;
                dcdata ??= new DowncomerDataSet(aAssy,null,null);
                bool altsplices = aSplices != null;
                uopSectionShapes sectionShapes = null;
                
                if (aSplices == null || !bForAltRing)
                {
                    aSplices = aAssy.DeckSplices;
                }
                else
                {
                    mdSplicing.VerifySplices(aSplices, aAssy);
                }

          

                if (bVerbose) aAssy.RaiseStatusChangeEvent($"Verifying {aAssy.TrayName()} Splices");
                //if (bVerbose) aAssy.RaiseStatusChangeEvent($"Creating {aAssy.TrayName()} Deck Section Shapes");
                //if (!bForAltRing)
                //{
                //    dcdata.ResetSections();
                //    sectionShapes = dcdata.SectionShapes( bRegen:true);
                //}
                //else
                //{
                    sectionShapes = dcdata.CreateSectionShapes(aSplices, bUpdatePanels: !altsplices, bVerbose: bVerbose);
               // }
                    

                colMDDeckPanels panels = aAssy.DeckPanels;

                foreach( var shape in sectionShapes) 
                {
                    if (bVerbose) aAssy.RaiseStatusChangeEvent($"Creating {aAssy.TrayName()} Deck Section {shape.Handle}");
                    mdDeckPanel panel = (mdDeckPanel)panels.Find(x => x.Index == shape.PanelIndex);
                    if (panel == null) 
                        continue;

                    //create the deck section based on the basic section shape
                    mdDeckSection section = new mdDeckSection(shape,panel) ;
                    section.SubPart(aAssy);
                    if(bCreatePerimeters)   
                        section.GeneratePerimeter(aAssy,true, bVerbose: bVerbose);
                    _rVal.Add(section);
                    if(aCollector!= null) aCollector.Add(section);
                }

            }
            finally
            {
                if (bVerbose) aAssy.RaiseStatusChangeEvent(string.Empty, string.Empty, bBegin: false);

            }

            return _rVal;
        }
     

        /// <summary>
        /// creates the subset of the assemblies deck sections reduces to only the inique members with their instance defines
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aSplices"></param>
        /// <param name="aSections"></param>
        /// <returns></returns>
        public static List<mdDeckSection> UniqueSections(mdDeckSections aSections, mdTrayAssembly aAssy, uopDeckSplices aSplices)
        {

            if (aSections == null || aSections.Count <= 0) return new List<mdDeckSection>();
            aAssy ??= aSections.GetMDTrayAssembly();
            if (aAssy == null ) return new List<mdDeckSection>();
            aSplices ??= aAssy.DeckSplices;
            uppSpliceStyles style = aAssy.SpliceStyle;
            List<mdDeckSection> sections = aSections.ToList();
            // to be sure the bp centers are defined
            UVECTORS bpsites = new UVECTORS(aAssy.BPSites);

            List<mdDeckSection> _rVal = new List<mdDeckSection>();

            int cnt = mdDeckSections.GetCount(sections, out string _, out int pcnt);
            bool invEq;
            List<int> pCnts = new List<int> { };
            bool symmetric = aAssy.IsSymmetric;
            bool specialcase = aSections.HasAlternateSections;
            int traycount = aAssy.TrayCount;
            bool bSlots = aAssy.DesignFamily.IsEcmdDesignFamily();
            List<double> skipX = new List<double>();
            List<int> manwaypanels = new List<int>();
            List<mdDeckSection> clones = new List<mdDeckSection>();
            try
            {

                //inititialize the base deck section instances
                foreach (mdDeckSection section in sections)
                {
                    section.PartNumberIndex = 0;
                    section.PartNumber = string.Empty;
                    section.UniqueIndex = 0;


                    if (specialcase)
                    {
                        if (section.PanelIndex == pcnt || section.PanelIndex == pcnt - 1)
                            section.PanelOccuranceFactor = 1;
                    }

                    if (section.PanelIndex > pCnts.Count) pCnts.Add(0);

                    mdDeckSection clone = new mdDeckSection(section) { Parent = section };
                    uopSectionShapes.SetMDSectionInstances(clone.BaseShape, aAssy);
                    clones.Add(clone);
                }


                List<int> skippanels = new List<int>();
                if (specialcase)
                {

                    skippanels.Add(pcnt);
                    skippanels.Add(pcnt - 1);
                    skipX.Add(aAssy.DeckPanels.Item(pcnt).X);
                }

                int manidx = -1;
                //look for matches on the return members
                foreach (mdDeckSection section in clones)
                {

                    if (section.IsManway && style == uppSpliceStyles.Tabs) 
                        manidx *= -1;
                    bool ignoreslots = specialcase && section.PanelIndex == pcnt;
                    section.BaseShape.TrayAssembly = aAssy;
                    // find a unique member that is Equal to this one
                    invEq = false;

                    mdDeckSection match = _rVal.Count > 0 && !section.IsHalfMoon ? mdSection_Generator.FindEqualSection(section, _rVal, aAssy, out invEq, bIgnoreSlots: ignoreslots) : null; //: null; // _rVal.Find(x => x.CompareTo(section) == true): null;
                    if (section.IsManway && style == uppSpliceStyles.Tabs)
                        invEq = manidx == -1;
                    if (match == null)
                    {
                        //there is no match so keep this section
                        int pCnt = pCnts[section.PanelIndex - 1] + 1;
                        section.PartNumberIndex = (100 * section.PanelIndex) + pCnt;
                        section.UniqueIndex = _rVal.Count + 1;

                        pCnts[section.PanelIndex - 1] = pCnt;

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

                        uopVector ctr1 = new uopVector(match.X, match.Y);
                        uopVector ctr2 = new uopVector(section.X, section.Y);
                        uopVector displacment = ctr2 - ctr1;

                        section.PartNumberIndex = match.PartNumberIndex;
                        section.UniqueIndex = match.UniqueIndex;
                        uopInstances insts = match.Instances;
                        insts.Owner = null;  // to suppress updates
                        insts.Add(displacment, invEq ? 180 : 0, aPartIndex: section.PanelIndex);
                        

                        mdDeckSection parent = match.Parent;
                        if(parent != null && parent.Instances.Count > 0)
                        {
                            ctr1 = new uopVector(parent.X, parent.Y); 
                            uopInstances pinsts = parent.Instances;
                            foreach(var pinst in pinsts)
                            {
                                //ctr1 = new uopVector(pinsts.BasePt) + new uopVector(pinst.DX, pinst.DY);
                                uopVector dspl = ctr1 -ctr2   + new uopVector(pinst.DX, pinst.DY); // - displacment;
                                insts.Add(dspl, mzUtils.NormAng(pinst.Rotation +  (invEq ? 180 : 0),false,true,true) , aPartIndex: section.PanelIndex);
                            }
                        }
                        match.Quantity = match.OccuranceFactor * traycount;
                        //set the parent of the base section to the matcher
                        section.AssociateToParent(match);
                    }

                }


                // add instances for the other side if the assembly is symmetric 

                foreach (mdDeckSection unique in _rVal)
                {
                    //set the number of times the unique section occurs in the entire tray range
                    unique.Quantity = unique.OccuranceFactor * traycount;
                    unique.Parent.Quantity = unique.Quantity;
                    unique.Parent.UniqueIndex = unique.UniqueIndex;
                    unique.Parent.PartNumberIndex = unique.PartNumberIndex;


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
        /// <param name="aSplices"></param>
        /// <param name="aSections"></param>
        /// <returns></returns>
        public static List<mdDeckSection> UniqueSections(mdTrayAssembly aAssy, List<uopDeckSplice> aSplices, List<mdDeckSection> aSections)
        {


            if (aAssy == null || aSplices == null || aSections == null) return new List<mdDeckSection>();
            if (aSections.Count <= 0) return new List<mdDeckSection>();
            // to be sure the bp centers are defined
            UVECTORS bpsites = new UVECTORS(aAssy.BPSites);

            List<mdDeckSection> _rVal = new List<mdDeckSection>();
            
            int cnt = mdDeckSections.GetCount(aSections, out string _, out int pcnt);
            bool invEq;
            List<int> pCnts = new List<int> { };
            bool symmetric = aAssy.IsSymmetric;
            bool specialcase = symmetric && aAssy.OddDowncomers && aSections.FindAll((x) => x.IsManway).Count > 0;
            int traycount = aAssy.TrayCount;
            bool bSlots = aAssy.DesignFamily.IsEcmdDesignFamily();
            List<double> skipX = new List<double>();
            List<int> manwaypanels = new List<int>();
            List<mdDeckSection> clones = new List<mdDeckSection>();
            try
            {

                //inititialize the base deck section instances
                foreach (mdDeckSection section in aSections)
                {
                    section.PartNumberIndex = 0;
                    section.PartNumber = string.Empty;
                    section.UniqueIndex = 0;

                   
                    if (specialcase)
                    {
                        if (section.PanelIndex == pcnt || section.PanelIndex == pcnt - 1)
                            section.PanelOccuranceFactor = 1;
                    }

                    if (section.PanelIndex > pCnts.Count) pCnts.Add(0);
                   
                    mdDeckSection clone = new mdDeckSection(section) { Parent = section };
                    uopSectionShapes.SetMDSectionInstances(clone.BaseShape, aAssy);
                    clones.Add(clone);
                }


                List<int> skippanels = new List<int>();
                if (specialcase)
                {

                    skippanels.Add(pcnt);
                    skippanels.Add(pcnt - 1);
                    skipX.Add(aAssy.DeckPanels.Item(pcnt).X);
                }

                //look for matches on the return members
                foreach (mdDeckSection section in clones)
                {

                    bool ignoreslots = specialcase && section.PanelIndex == pcnt;
                    section.BaseShape.TrayAssembly = aAssy;
                    // find a unique member that is Equal to this one
                    invEq = false;
                   
                    mdDeckSection match = _rVal.Count > 0 && !section.IsHalfMoon ? mdSection_Generator.FindEqualSection(section, _rVal, aAssy, out invEq, bIgnoreSlots: ignoreslots) : null; //: null; // _rVal.Find(x => x.CompareTo(section) == true): null;

                    if (match == null)
                    {
                        //there is no match so keep this section
                        int pCnt = pCnts[section.PanelIndex - 1] + 1;
                        section.PartNumberIndex = (100 * section.PanelIndex) + pCnt;
                        section.UniqueIndex = _rVal.Count + 1;
                      
                        pCnts[section.PanelIndex - 1] = pCnt;
                        
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

                        uopVector ctr1 = new uopVector(match.X, match.Y);
                        uopVector ctr2 = new uopVector(section.X, section.Y);
                        uopVector displacment = ctr2 - ctr1;

                        section.PartNumberIndex = match.PartNumberIndex;
                        section.UniqueIndex = match.UniqueIndex;
                        uopInstances insts = match.Instances;
                        insts.Owner = null;  // to suppress updates
                        insts.Add(displacment, invEq ? 180 : 0, aPartIndex: section.PanelIndex);
                        match.Quantity = match.OccuranceFactor * traycount;

                        //set the parent of the base section to the matcher
                        section.AssociateToParent(match);
                    }

                }


                // add instances for the other side if the assembly is symmetric 
                
                    foreach (mdDeckSection unique in _rVal)
                    {
                     //set the number of times the unique section occurs in the entire tray range
                    unique.Quantity = unique.OccuranceFactor * traycount;
                    unique.Parent.Quantity = unique.Quantity;
                    unique.Parent.UniqueIndex = unique.UniqueIndex;
                    unique.Parent.PartNumberIndex = unique.PartNumberIndex;


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

        public static mdDeckSection FindEqualSection(mdDeckSection aSection, List<mdDeckSection> aSections, mdTrayAssembly aAssy, out bool rInverseEqual, bool bShapeCompare = false, bool bReturnParent = false, bool bIgnoreSlots = false)
        {
            
            rInverseEqual = false;
            if (aSection == null || aSections == null) return null;
            
            List<mdDeckSection> possibles = aSections.FindAll(x => x != aSection && x.IsManway == aSection.IsManway && x.LapsRing == aSection.LapsRing && x.LapsDivider == aSection.LapsDivider  && x.IsHalfMoon == aSection.IsHalfMoon);
            if (possibles.Count <= 0) return null;
            mdDeckSection _rVal = null;
            foreach (mdDeckSection item in possibles)
            {


                if (mdSection_Generator.SectionsCompare(aSection, item, aAssy, aAssy, false, out rInverseEqual, bShapeCompare, bIgnoreSlots))
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
            if(aSection.SingleSection != bSection.SingleSection) return false;
            if (aSection.RequiresSlotting != bSection.RequiresSlotting) return false;
            if (aSection.IsHalfMoon != bSection.IsHalfMoon) return false;
            if (aSection.IsManway != bSection.IsManway) return false;
            if (aSection.LapsRing != bSection.LapsRing) return false;
            if (aSection.LapsDivider != bSection.LapsDivider) return false;

            if (aSection.LapsDivider && aSection.X != 0 && !bShapeCompare)
            {
                if ((aSection.X > 0 & bSection.X > 0) || (aSection.X < 0 & bSection.X < 0)) return false;
                if ((aSection.Y > 0 & bSection.Y > 0) || (aSection.Y < 0 & bSection.Y < 0)) return false;
            }

           

            aAssy ??= aSection.GetMDTrayAssembly();
            if (aAssy == null) return false;

            bAssy ??= bSection.GetMDTrayAssembly();
            if (bAssy == null) return false;

            bool sameassembly = aAssy == bAssy;
            if (sameassembly)
            {
                if (aAssy.DesignFamily != bAssy.DesignFamily) return false;
                if (bCompareMaterial)
                {
                    if (!aSection.Material.IsEqual(bSection.Material)) return false;
                }

                if (aAssy.SpliceStyle != bAssy.SpliceStyle) return false;
            }
            if (sameassembly && (aSection.IsHalfMoon || aSection.IsManway))
            {
                rInverseEqual = aSection.TopSpliceType == bSection.TopSpliceType;
                return true;
            }
                

            if (aSection.LapsRing && sameassembly)
            {
                //if (aSection.X < 0 && bSection.X < 0) 
                //    return false;
                //if (aSection.X > 0 && bSection.X > 0) 
                //    return false;
                if (aSection.BaseShape.PanelX !=  bSection.BaseShape.PanelX)
                    return false;
                if(!uopDeckSplice.CompareSplices(aSection.BottomSplice, bSection.BottomSplice))
                    return false;
                if (!uopDeckSplice.CompareSplices(aSection.TopSplice, bSection.TopSplice))
                    return false;


            }

            aSection.GeneratePerimeter(aAssy);
            bSection.GeneratePerimeter(bAssy);
            List<dxfVector> bps1 = new List<dxfVector>();
            List<dxfVector> bps2 = new List<dxfVector>();
            if (aAssy.DesignOptions.HasBubblePromoters || bAssy.DesignOptions.HasBubblePromoters)
            {
                dxfPlane plane = aSection.Perimeter.Plane;
                List<dxeArc> bpcircles = aSection.Perimeter.AdditionalSegments.Arcs().FindAll(x => x.Tag == "BUBBLE PROMOTER");
        
                foreach (var arc in bpcircles) bps1.Add(arc.Center.WithRespectToPlane(plane).Rounded(3));

                plane = bSection.Perimeter.Plane;
                bpcircles = bSection.Perimeter.AdditionalSegments.Arcs().FindAll(x => x.Tag == "BUBBLE PROMOTER");
                
                foreach (var arc in bpcircles) bps2.Add(arc.Center.WithRespectToPlane(plane).Rounded(3));

                if (bps1.Count != bps2.Count)
                    return false;

               
            }

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
                    if (bSlotZone != null) bPts = bSlotZone.GridPts.GetBySuppressed(false, true);
                    _rVal = uopVectors.MatchPlanar(aPts, bPts,  aPrecis:2);

                }
            }
            else
            {
                //compare the perimter points 
                aPts = aSection.PerimeterPts( bIncludeBPSites: true, bIncludeSlotSites: false);
                bPts = bSection.PerimeterPts( bIncludeBPSites: true, bIncludeSlotSites: false);
                _rVal = uopVectors.MatchPlanar(aPts, bPts, aPrecis: 2);
                if (_rVal && bECMD && !bIgnoreSlots)
                {
                    mdSlotZone aZone = aSection.SlotZone(aAssy);
                    mdSlotZone bZone = bSection.SlotZone(bAssy);
                    if (aZone == null || bZone == null)
                        return false;

                   
                    uopArcRecs aIslands = aZone.Islands;
                    uopArcRecs bIslands = bZone.Islands;
                    aIslands ??= uopArcRecs.Null;
                    bIslands ??= uopArcRecs.Null;


                    if (aZone.PitchType != bZone.PitchType || Math.Round(aZone.HPitch - bZone.HPitch, 3) != 0 || Math.Round(aZone.VPitch - bZone.VPitch, 3) != 0 || aIslands.Count != bIslands.Count)
                        return false;
                    //_rVal = uopVectors.MatchPlanar(aZone.GridPts, bZone.GridPts);
                    if (aZone.GridPts == null && aZone.GridPts == null) return true;
                    if (aZone.GridPts == null || aZone.GridPts == null) return false;
                    _rVal = uopVectors.MatchPlanar(aZone.GridPts.GetBySuppressed(false), bZone.GridPts.GetBySuppressed(false), aPrecis: 2);


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

            if( _rVal && !rInverseEqual & bps1.Count > 0)
            {
                foreach(var pt in bps1)
                {
                    if (bps2.FindIndex(x => x == pt) <0)
                    {
                        rInverseEqual = true;
                        break;  
                    }
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
        ///  creates the subset of the assemblies deck sections for the alternate ring with their instances set for each occurance in the assemblies alternate deck section assembly
        /// </summary>
        /// <param name="aAssySections">the full set of deck sections from the assembly (1 per instance)</param>
        /// <param name="aUniqueSections">the unique subset of the full set on the primary ring</param>
        /// <param name="aAssy">the parent assembly</param>
        /// <param name="aSplices">the parent splices</param>
        /// <returns></returns>

        public static List<mdDeckSection> UniqueAltRingSections(mdDeckSections aAssySections, List<mdDeckSection> aUniqueSections, mdTrayAssembly aAssy, uopDeckSplices aSplices)
        {

            if (aAssy == null || aUniqueSections == null || aSplices == null || aAssySections == null || !aAssySections.HasAlternateSections) return new List<mdDeckSection>();

            bool symmetric = aAssy.IsSymmetric;
            bool specialcase = symmetric && aAssy.OddDowncomers;

            List<mdDeckSection> _rVal = new List<mdDeckSection>();
            List<mdDeckSection> uniques = aUniqueSections.ToList();
            List<mdDeckSection> assysections = aAssySections.ToList();
 
            uopDeckSplices assysplices = aSplices == null ?  new uopDeckSplices(aAssy, aAssy.DeckSplices) : aSplices;

            List<int> manpanels = new List<int>(); // aAssy.DeckSections.ManwayPanelIndexes();
            List<int> panelIDS = aAssySections.PanelIndexes(out List<int> panelsectioncounts);

            uopDeckSplices altsplices = new uopDeckSplices(aAssy);
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
                            if (!manpanels.Contains(pid)) manpanels.Add(pid);

                    }
                    maxpartidxs.Add(maxpn);

                    //create the deck splices by mirror the splices that lie on the panels that have manways   
                    List<uopDeckSplice> psplices = assysplices.FindAll((x) => x.PanelIndex == pid);
                    if (manpanels.Contains(pid))
                    {
                        foreach (uopDeckSplice item in psplices)
                            item.Mirror(null, item.PanelY);
                        psplices.Reverse();

                    }
                    altsplices.AddRange(psplices);
                }

                //create the panel alternate ring deck sections based on the the new splices collection
                List<mdDeckSection> altsecs = mdSection_Generator.GenerateDeckSections(aAssy, altsplices, bVerbose:false, bForAltRing: true);

                //deal with slot zones
                mdSlotZones altzones = new mdSlotZones();
                mdSlotZones basezones = new mdSlotZones();
                bool ecmd = aAssy.IsECMD;
                if (ecmd)
                {
                    basezones = aAssy.SlotZones;
                   altzones = mdSlotting.SlotZones_Create(aAssy, altsecs, basezones);
                    basezones.AlternateZones = new List<mdSlotZone>(altzones);

                    //create the new slot zones based on the new splices collection

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
                        }
                    }
                }


                //generate the alternate ring unique deck section from the rearanged splices 
                List<mdDeckSection> altuniques = mdSection_Generator.UniqueSections(aAssy, altsplices, altsecs);

                foreach (mdDeckSection altsection in altuniques)
                {
                    int pid = altsection.PanelIndex;
                    bool inverted = manpanels.Contains(pid);
                    int sectioncount = panelsectioncounts[pid - 1];
                    string althandle = inverted ? $"{pid},{uopUtils.OpposingIndex(altsection.SectionIndex, sectioncount)}" : altsection.Handle;
                    mdDeckSection assyunique = aUniqueSections.Find((x) => x.Handle == althandle);
                    List<mdDeckSection> searchin = aUniqueSections.FindAll((x) => x.LapsRing == altsection.LapsRing && x.IsManway == altsection.IsManway && x.LapsDivider == altsection.LapsDivider);
                    if (altsection.LapsRing || altsection.TruncatedFlange)
                        searchin = searchin.FindAll((x) => x.Y <= 0 == altsection.Y <= 0);

                    bool ignoreslots = specialcase && pid == panelsectioncounts.Count;
                    if(altsection.PanelIndex == 3)
                    {
                        //Console.WriteLine("HERE");
                    }
                    mdDeckSection match = mdSection_Generator.FindEqualSection(altsection, searchin, aAssy, out bool invEq, bShapeCompare: true, bReturnParent: false, bIgnoreSlots: true); // ignoreslots);

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
                        if (ecmd)
                        {
                            mdSlotZone altzone = altzones.Find((x) => x.SectionHandle == altsection.Handle);
                            altsection.AltSlotZone = altzone;

                        }
                    }
                    else
                    {
                        // this section occurs on both the primary and secondary ring
                        altsection.Inverted = inverted && Math.Round(altsection.Y, 2) != Math.Round(match.Y, 2);
                        altsection.PartNumberIndex = match.PartNumberIndex;
                        if (ecmd)
                        {

                            mdSlotZone altzone = basezones.Find((x) => x.SectionHandle == altsection.Handle);
                            altsection.AltSlotZone = new mdSlotZone( altsection.BaseShape,altsection,aAssy,altzone.HPitch,altzone.VPitch);
                            altsection.Parent = match;
                        }
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



       
        public static uopRectangles GetSpliceLimits(List<mdDeckSection> aSections, mdTrayAssembly aAssy, double aAdder = 0)
        {
            uopRectangles _rVal = new uopRectangles();
            if (aSections == null) return _rVal;
            double jht = aAssy == null ? 1 : aAssy.JoggleAngleHeight;
            foreach (mdDeckSection item in aSections)
            {
                uopDeckSplice aSplc = item.BottomSplice;
                if (aSplc != null)
                {
                    URECTANGLE aRec = aSplc.Limits();
                    if (aAdder != 0) aRec.Stretch(aAdder);
                    _rVal.Add(aRec);
                }
            }


            return _rVal;
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
    
    }
}
