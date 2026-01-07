using UOP.DXFGraphics;
using System;
using System.Collections.Generic;
using System.Linq;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Materials;
using UOP.WinTray.Projects.Parts;

using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using static System.Math;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;
using System.Diagnostics;
using UOP.WinTray.Projects.src.Utilities.ExtensionMethods;

namespace UOP.WinTray.Projects.Generators
{
    public class mdPartGenerator
    {
        /// <summary>
        /// creates the part array that contains the common parts of a mdProject
        /// </summary>
        /// <param name="aProject"></param>
        /// <param name="aBasePartArray"></param>

        /// <returns></returns>




        private static List<mdDowncomerBox> Get_DCBOXES(string aProjectHandle, mdTrayAssembly aAssy)
        {
            List<mdDowncomerBox> _rVal = new List<mdDowncomerBox>();

            try
            {

                int traycount = aAssy.RingRange.RingCount;


                if (traycount <= 0) return _rVal;
                string colltr = aAssy.ColumnLetter;
                double bottomdcheight = aAssy.DesignOptions.BottomDCHeight;
                if (Math.Round(bottomdcheight, 3) < Math.Round(aAssy.Downcomer().Height + 0.5, 3) || traycount == 1) bottomdcheight = 0;

                uopRingRange ringRange1 = aAssy.RingRange;
                uopRingRange ringRange2 = aAssy.RingRange;
                if (bottomdcheight > 0)
                {
                    ringRange2 = new uopRingRange(ringRange1) { RingStart = ringRange1.BottomRing, RingEnd = ringRange1.BottomRing, StackPattern = uppStackPatterns.Continuous, SortOrder = ringRange1.SortOrder };
                    ringRange1 = new uopRingRange(ringRange1) { RingStart = ringRange1.TopRing, RingEnd = ringRange1.BottomRing + 1, StackPattern = uppStackPatterns.Continuous, SortOrder = ringRange1.SortOrder };
                    traycount -= 1;

                }
                List<mdDowncomerBox> assyparts = Boxes_ASSY(aAssy, aCountMultiplier: traycount, aProjectHandle: aProjectHandle);

                if (assyparts.Count <= 0) return _rVal;
                string rguid = aAssy.RangeGUID;
                foreach (mdDowncomerBox item in assyparts)
                {
                    item.AssociateToParent(item.PartNumber, null, true);
                    item.AssociateToRange(rguid, true);
                    item.OverridePartNumber = $"{uopPart.DefaultPartNumber(item)}{colltr}";
                    item.NodeName = $"Downcomer Body {item.PartNumber}";
                    item.RingRange = ringRange1;
                    item.Quantity = item.OccuranceFactor * ringRange1.RingCount;
                    item.RingRanges = new uopRingRanges(new uopRingRange(item));
                    _rVal.Add(item);
                    if (bottomdcheight > 0)
                    {


                        mdDowncomerBox bbox = item.Clone();

                        bbox.AssociateToRange(rguid, true);
                        bbox.Height = bottomdcheight;
                        bbox.OverridePartNumber = $"{uopPart.DefaultPartNumber(item, 50)}{colltr}";
                        bbox.AssociateToParent(bbox.PartNumber, null, true);
                        bbox.NodeName = $"Downcomer Body {bbox.PartNumber}";
                        bbox.RingRange = ringRange2;
                        bbox.Quantity = item.OccuranceFactor * ringRange2.RingCount;
                        bbox.RingRanges = new uopRingRanges(new uopRingRange(bbox));
                        bbox.IsVirtual = true;
                        item.VirtualChildPartNumber = bbox.OverridePartNumber;

                        _rVal.Add(bbox);


                    }

                }



            }
            catch { }
            return _rVal;

        }


        /// <summary>
        /// executed internally to create the AP Pans collection for an assembly
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public static List<mdAPPan> APPans_ASSY(mdTrayAssembly aAssy, ref int aBasePN, int? aCountMultiplier = null, string aProjectHandle = null, colUOPParts aCollector = null)
        {
            List<mdAPPan> _rVal = new List<mdAPPan>();
            if (aAssy == null) return _rVal;
            if (!aAssy.HasAntiPenetrationPans) return _rVal;
            aAssy.RaiseStatusChangeEvent($"Generating {aAssy.TrayName()} AP Pans", bBegin: true);
            aProjectHandle ??= aAssy.ProjectName;
            string rGUID = aAssy.RangeGUID;
            List<mdDowncomer> aDCs = aAssy.Downcomers.GetByVirtual(aVirtualValue: false);
            string colltr = aAssy.ColumnLetter;
            double maxpattern = aAssy.SpoutGroups.MaxPatternLength;
            if (!aCountMultiplier.HasValue) aCountMultiplier = aAssy.TrayCount;
            foreach (mdDowncomer dc in aDCs)
            {
                List<mdAPPan> dcpans = APPans_DC(aAssy, dc.Index, maxpattern);

                foreach (mdAPPan pan in dcpans)
                {
                    pan.AssociateToRange(rGUID, true);
                    pan.AssociateToParent(dc, true);
                    if (dc.OccuranceFactor > 1)
                    {
                        List<uopInstance> mirrors = pan.Instances.AppendRotations(uopVector.Zero, 180);
                        pan.Quantity = pan.Instances.Count + 1;

                    }

                    pan.Quantity = (pan.Instances.Count + 1) * aCountMultiplier.Value;

                    mdAPPan match = _rVal.Find(x => x.CompareTo(pan, false));

                    if (match == null)
                    {

                        pan.PartNumber = $"{aBasePN}{colltr}";
                        aBasePN++;

                        _rVal.Add(pan);
                        if (aCollector != null) aCollector.Add(pan);
                    }
                    else
                    {
                        match.Instances.Append(pan.Instances);
                        match.MergeParentAssociations(pan.ParentList);
                        match.Quantity = (match.Instances.Count + 1) * aCountMultiplier.Value;
                        pan.PartNumber = match.PartNumber;
                    }
                }
            }
            return _rVal;
        }


        /// <summary>
        /// executed internally to create the AP Pans collection for an assembly
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public static List<mdAPPan> APPans_ASSY(mdTrayAssembly aAssy, int? aCountMultiplier = null, string aProjectHandle = null, colUOPParts aCollector = null)
        {
            List<mdAPPan> _rVal = new List<mdAPPan>();
            if (aAssy == null) return _rVal;
            if (!aAssy.HasAntiPenetrationPans) return _rVal;
            aAssy.RaiseStatusChangeEvent($"Generating {aAssy.TrayName()} AP Pans");
            aProjectHandle ??= aAssy.ProjectName;
            string rGUID = aAssy.RangeGUID;
            List<mdDowncomer> aDCs = aAssy.Downcomers.GetByVirtual(aVirtualValue: false);
            string colltr = aAssy.ColumnLetter;
            double maxpattern = aAssy.SpoutGroups.MaxPatternLength;
            if (!aCountMultiplier.HasValue) aCountMultiplier = aAssy.TrayCount;
            foreach (mdDowncomer dc in aDCs)
            {
                List<mdAPPan> dcpans = APPans_DC(aAssy, dc.Index, maxpattern);

                foreach (mdAPPan pan in dcpans)
                {
                    pan.AssociateToRange(rGUID, true);
                    pan.AssociateToParent(dc, true);
                    if (dc.OccuranceFactor > 1)
                    {
                        List<uopInstance> mirrors = pan.Instances.AppendRotations(uopVector.Zero, 180);
                        pan.Quantity = pan.Instances.Count + 1;

                    }

                    pan.Quantity = (pan.Instances.Count + 1) * aCountMultiplier.Value;

                    mdAPPan match = _rVal.Find(x => x.CompareTo(pan, false));

                    if (match == null)
                    {
                        _rVal.Add(pan);
                        if (aCollector != null) aCollector.Add(pan);
                    }
                    else
                    {
                        match.Instances.Append(pan.Instances);
                        match.MergeParentAssociations(pan.ParentList);
                        match.Quantity = (match.Instances.Count + 1) * aCountMultiplier.Value;

                    }
                }
            }
            return _rVal;
        }


        /// <summary>
        /// executed internally to create the AP Pans collection for an assembly
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public static List<mdAPPan> APPans_DC(mdTrayAssembly aAssy, int aDCIndex, double? aMaxPatternLength = null)
        {
            List<mdAPPan> _rVal = new List<mdAPPan>();
            if (aAssy == null) return _rVal;
            if (!aAssy.HasAntiPenetrationPans) return _rVal;

            mdDowncomer downcomer = aDCIndex <= 0 ? null : aAssy.Downcomers.Item(aDCIndex, bSuppressIndexError: true);
            if (downcomer == null) return _rVal;

            if (!aMaxPatternLength.HasValue) aMaxPatternLength = aAssy.SpoutGroups.MaxPatternLength;

            string aProjectHandle = aAssy.ProjectName;

            List<mdSpoutGroup> fieldGroups = aAssy.SpoutGroups.FieldGroups.FindAll(x => x.DowncomerIndex == downcomer.Index);
            string rGUID = aAssy.RangeGUID;
            mdAPPan pan = new mdAPPan(aAssy);

            colDXFVectors aCenters = new colDXFVectors();
            List<mdSpoutGroup> endGroups = aAssy.SpoutGroups.EndGroups.FindAll(x => x.DowncomerIndex == downcomer.Index);

            double thk = pan.Thickness;

            double aZ = pan.Z;
            double holeinset = pan.HoleInset;
            double d1 = pan.FlangeLength - holeinset;
            double MaxHt = uopUtils.RoundTo(aMaxPatternLength.Value + (0.25 + thk) * 2, dxxRoundToLimits.Millimeter, bRoundUp: true);
            uopInstances insts = null;
            uopVector sgcenter;
            mdAPPan fldPan = null;
            mdAPPan endPan = null;
            double y1;
            double y2;
            uopVector cp;
            //one pan with multiple centers

            foreach (mdSpoutGroup group in fieldGroups)
            {
                sgcenter = group.Perimeter.Center;
                y1 = sgcenter.Y + (0.5 * MaxHt);
                y2 = sgcenter.Y - (0.5 * MaxHt);

                cp = new uopVector(sgcenter.X, y1 + d1, aElevation: aZ);

                if (fldPan is null)
                {
                    fldPan = pan.Clone();
                    fldPan.Length = MaxHt;
                    fldPan.PartNumber = "800";
                    fldPan.SetCoordinates(cp.X, cp.Y, aZ);
                    fldPan.AssociateToParent(downcomer, true);
                    fldPan.AssociateToRange(rGUID, true);
                    fldPan.DowncomerIndex = downcomer.Index;
                    fldPan.OpenEnded = false;
                    fldPan.BoltCount = 1;
                    fldPan.Z = aZ;
                    fldPan.ProjectHandle = aProjectHandle;
                    fldPan.DowncomerIndex = downcomer.Index;
                    _rVal.Add(fldPan);


                    insts = fldPan.Instances;
                    insts.Owner = null;

                }
                else
                {
                    uopVector offset = new uopVector(cp.X - fldPan.X, cp.Y - fldPan.Y);
                    insts.Add(offset);
                }

                uopInstance inst = group.Instances.Find((x) => x.XOffset == 0);
                if (inst != null)
                {
                    cp = inst.ApplyTo(cp, group.Center);
                    uopVector offset = new uopVector(cp.X - fldPan.X, cp.Y - fldPan.Y);
                    insts.Add(offset, 180);
                }

            }

            if (fldPan != null) fldPan.Instances = insts;



            if (endGroups.Count <= 0) return _rVal;

            mdSpoutGroup egroup = endGroups[0];

            endPan = new mdAPPan( pan);
            endPan.PartNumber = "801";
            endPan.IsEndPan = true;
            endPan.DowncomerIndex = downcomer.Index;
            endPan.AssociateToParent(downcomer);
            endPan.AssociateToRange(rGUID, bClear: true);
            sgcenter = egroup.Center.Clone();
            double patlength = egroup.PatternLength;
            y1 = sgcenter.Y + (0.5 * patlength) + (0.25 + thk);
            y2 = sgcenter.Y - (0.5 * patlength) - (0.25 + thk);

            endPan.BoltCount = egroup.Perimeter.IsRectangular() ? 1 : 2;

            mdDowncomerBox bx = downcomer.Boxes.Last();
            ULINE l1 = bx.EndLn(bTop:true, (-12 / 25.4));
            double toedge = 0.5 * downcomer.BoxWidth + pan.TabInset + 0.5 * pan.TabWidth;

            ULINE l2 = new ULINE(new UVECTOR(downcomer.X - 0.5 * downcomer.BoxWidth + pan.TabInset + 0.5 * pan.TabWidth, -100), new UVECTOR(downcomer.X - 0.5 * downcomer.BoxWidth + pan.TabInset + 0.5 * pan.TabWidth, 100));
            UVECTOR u1 = l1.IntersectionPt(l2);


            double yhole = u1.Y;
            y1 = u1.Y - d1;
            endPan.Length = uopUtils.RoundTo(y1 - y2, dxxRoundToLimits.Millimeter, bRoundUp: true);
            cp = new uopVector(downcomer.X, yhole, pan.Z);
            endPan.SetCoordinates(cp.X, cp.Y, aZ);
            //see if the end pan is the same as one of the already defined pans
            if (endPan.BoltCount == 1 && Round(endPan.Length, 1) == Round(MaxHt, 1) && fldPan != null)
            {
                insts = fldPan.Instances;
                insts.Owner = null;
                insts.Add(cp - fldPan.Center);
                insts.Add(cp.Mirrored(null, downcomer.Y) - fldPan.Center);
                fldPan.Instances = insts;
            }
            else
            {
                insts = endPan.Instances;
                insts.Owner = null;
                uopInstance inst = egroup.Instances.Find((x) => x.XOffset == 0);
                if (inst != null)
                {
                    cp = inst.ApplyTo(cp, egroup.Center);
                    uopVector offset = new uopVector(cp.X - endPan.X, cp.Y - endPan.Y);
                    insts.Add(offset, 180);

                }

                endPan.Instances = insts;
                _rVal.Add(endPan);
            }

            ///////////////////////////////

            //mdDowncomer aDC = aDCs.Item(aSG.DowncomerIndex);
            //double aX = aDC.X;
            //double sgY = aSG.SpoutCenter.Y;

            //aCenters = aSG.Instances.MemberPointsDXF();
            //string sgtag = aCenters.Item(1).Tag;
            ////MaxHt = uopUtils.RoundTo(MaxHt, dxxRoundToLimits.Millimeter, bRoundUp: true);
            //double y1 = sgY + (0.5 * aSG.PatternLength) + 0.25 + thk;
            //double y2 = sgY - (0.5 * aSG.PatternLength) - 0.25 - thk;
            //MaxHt = y1 - y2;
            //dxfVector cp = new dxfVector(aX, y2 + (0.5 * MaxHt), aZ);
            //double yhole = y1 + d1;
            //int boltcount = aSG.Perimeter.IsRectangular() ? 1 : 2;

            //ULINE l1 = aDC.Box.EndLn(false, -(12 / 25.4));
            //ULINE l2 = new ULINE(new UVECTOR(aX - 0.5 * aDC.BoxWidth + pan.TabInset + 0.5 * pan.TabWidth, -100), new UVECTOR(aX - 0.5 * aDC.BoxWidth + pan.TabInset + 0.5 * pan.TabWidth, 100));
            //u1 = l1.IntersectionPt(l2);


            //yhole = u1.Y;
            //y1 = yhole - d1;
            //MaxHt = y1 - y2;
            //double dy = yhole - sgY;
            //// see if the end pan is the same as one of the already defined pans
            //mdAPPan endPan = rEndPans.Find(x => Math.Round(x.Length - MaxHt, 3) == 0 && x.BoltCount == boltcount);

            //if (endPan == null)
            //{
            //    endPan = pan.Clone();
            //    endPan.Tag = $"ENDGROUP_{rEndPans.Count + 1}";

            //    UVECTORS pancenters = new UVECTORS(false, endPan.Tag);
            //    //angled end groups get two bolt holes
            //    endPan.BoltCount = boltcount;
            //    endPan.Z = aZ;
            //    endPan.Length = MaxHt;

            //    endPan.OpenEnded = false;

            //    foreach (dxfVector item in aCenters)
            //    {
            //        u1 = UVECTOR.FromDXFVector(item, bRotationsAsValues: true);
            //        double f1 = (item.Rotation == 0) ? 1 : -1;
            //        u1.Y = u1.Y > 0 ? sgY + f1 * dy : -sgY + f1 * dy;
            //        pancenters.Add(u1);
            //    }


            //    endPan.AssociateToParent(aDC);
            //    endPan.AssociateToRange(rGUID, bClear: true);

            //    rEndPans.Add(endPan);
            //    centersv.Add(pancenters);
            //}
            //else
            //{
            //    UVECTORS pancenters = centersv.Find((x) => x.Name == endPan.Tag);
            //    u1 = new UVECTOR(cp.X, cp.Y + (0.5 * MaxHt) + d1) { Tag = sgtag };
            //    pancenters.Add(u1);
            //    pancenters.Add(u1.X, -u1.Y, aValue: 180, aTag: sgtag);
            //    if (Round(u1.X, 1) != 0)
            //    {
            //        pancenters.Add(-u1.X, -u1.Y, aValue: 180, aTag: sgtag);
            //        pancenters.Add(-u1.X, u1.Y, aValue: 0, aTag: sgtag);
            //    }

            //    endPan.AssociateToParent(aDC);
            //    centersv[centersv.IndexOf(pancenters)] = pancenters;



            //}


            //   }


            //foreach (mdAPPan item in rPans)
            //{
            //    UVECTORS pancenters = centersv[0];
            //    u1 = pancenters.Item(1);
            //    item.SetCoordinates(u1.X, u1.Y, aZ);
            //    uopInstances insts = uopInstances.FromVectors(pancenters, item.CenterV);
            //    item.Instances = insts;
            //    item.Quantity = item.Instances.Count + 1;
            //    item.PartNumber = $"{pn}{colltr}";
            //    item.Z = aZ;
            //    pn++;
            //    if (aProjectHandle != null) item.ProjectHandle = aProjectHandle;
            //    if (aCountMultiplier.HasValue) item.Quantity *= aCountMultiplier.Value;
            //    _rVal.Add(item);
            //    if (aCollector != null) aCollector.Add(item);
            //}
            //foreach (mdAPPan item in rEndPans)
            //{
            //    UVECTORS pancenters = centersv.Find((x) => x.Name == item.Tag);
            //    u1 = pancenters.Item(1);
            //    item.SetCoordinates(u1.X, u1.Y, aZ);
            //    item.Instances = uopInstances.FromVectors(pancenters, pancenters.Item(1));

            //    item.PartNumber = $"{pn}{colltr}";
            //    item.Z = aZ;
            //    pn++;
            //    if (aCountMultiplier.HasValue) item.Quantity *= aCountMultiplier.Value;
            //    if (aProjectHandle != null) item.ProjectHandle = aProjectHandle;
            //    _rVal.Add(item);
            //    if (aCollector != null) aCollector.Add(item);
            //}

            //if (!bApplyInstances && downcomer != null)
            //{
            //    mdDowncomer aDC = downcomer;
            //    List<mdAPPan> pans = _rVal.FindAll(x => x.IsAssociatedToParent(aDC.PartNumber));
            //    List<mdAPPan> rpans = new List<mdAPPan>();
            //    foreach (mdAPPan item in pans)
            //    {
            //        uopInstances insts = item.Instances;

            //        aCenters = insts.MemberPointsDXF();
            //        List<dxfVector> cntrs = aCenters.GetVectors(dxxPointFilters.AtX, aDC.X, aPrecis: 2);
            //        foreach (dxfVector ctr in cntrs)
            //        {

            //            pan = item.Clone();
            //            uopInstances pinsts = pan.Instances;
            //            pinsts.Owner = null;
            //            pinsts.Clear();
            //            pan.Z = aZ;
            //            pan.AssociateToRange(rGUID, true);
            //            pan.DowncomerIndex = aDCIndex;
            //            pan.AssociateToParent(aDC, true);
            //            //aCenters = new colDXFVectors(ctr.Clone());
            //            //pan.CentersV = UVECTORS.FromDXFVectors(aCenters, bRotationsAsValues: true);

            //            pan.SetCoordinates(ctr.X, ctr.Y, aZ);
            //            pinsts = uopInstances.FromVectors(aCenters, aCenters.Item(1));
            //            pan.Instances = pinsts;
            //            rpans.Add(pan);
            //            if (aCollector != null) aCollector.Add(pan);
            //        }
            //    }
            //    _rVal = rpans;
            //}

            return _rVal;
        }

     

        public static List<mdBaffle>DeflectorPlates_ASSY(mdTrayAssembly aAssy, bool bApplyInstances = true, int aDCIndex = 0, int? aCountMultiplier = null, string aProjectHandle = null)
        {
            List<mdBaffle> _rVal = new List<mdBaffle>();


            if (aAssy == null) return _rVal;

            if (!aAssy.DesignFamily.IsEcmdDesignFamily()) return _rVal;

            if (aAssy.DesignOptions.CDP <= 0) return _rVal;
            List<mdDowncomerBox> boxes = aAssy.Downcomers.Boxes(aDCIndex);
            double aHeight = aAssy.BaffleHeight;
            double aZ = aAssy.Deck.Thickness + aAssy.Downcomer().How;
            double aZLim = aAssy.RingSpacing - aAssy.Downcomer().HeightBelowDeck - 0.5 - aZ;
            List<double> dccenters = aAssy.Downcomers.XValues(true);
            dccenters.Sort();
            dccenters.Reverse();
            List<mdBaffle> assyBafs = new List<mdBaffle>();

            uppMDDesigns family = aAssy.DesignFamily;
            foreach (var box in boxes)
            {
                List<mdBaffle> DCBafs = mdPartGenerator.DeflectorPlates_DCBox(box,aAssy,aHeight,aZ,aZLim, dccenters,true) ;
                foreach (var baffle in DCBafs)
                {
                   
                    if (bApplyInstances)
                    {
                        if (box.OccuranceFactor > 1)
                        {
                            uopInstances insts = box.Instances;
                            foreach (var inst in insts)
                                baffle.Instances.Add(new uopInstance(inst));
                            
                            
                        }
                        baffle.Quantity = baffle.Instances.Count + 1;
                        mdBaffle match = assyBafs.Find((x) => x.CompareTo(baffle,false));
                        if (match != null)
                        {
                            match.OccuranceFactor += baffle.OccuranceFactor;
                            match.Instances.Append(baffle.Instances);
                            match.Quantity = match.Instances.Count + 1;
                        }
                        else
                        {
                            assyBafs.Add(baffle);
                        }
                    }
                    else
                    {
                        assyBafs.Add(baffle);
                    }
                    
                }
                

            }



            uopRingRanges assyrings = new uopRingRanges(aAssy.RingRange);
            foreach (mdBaffle item in assyBafs)
            {
                item.RingRanges = assyrings.Clone();
                //item.OccuranceFactor = item.Quantity;

                if (aCountMultiplier.HasValue)
                    item.Quantity *= aCountMultiplier.Value;

                if (!string.IsNullOrWhiteSpace(aProjectHandle)) 
                    item.ProjectHandle = aProjectHandle;
                _rVal.Add(item);
            }
            return _rVal;
        }

        /// <summary>
        /// the Baffles that are designed for the downcomer
        /// </summary>
        /// <param name="aDowncomer"></param>
        /// <param name="aAssy"></param>
        /// <param name="aHeight"></param>
        /// <param name="aZ"></param>
        /// <param name="aZLim"></param>
        /// <param name="aDCCenters"></param>
        /// <returns></returns>        
        public static List<mdBaffle>DeflectorPlates_DC(mdDowncomer aDowncomer, mdTrayAssembly aAssy, double? aHeight = null, double? aZ = null, double? aZLim = null, mzValues aDCCenters = null, bool bSuppressInstances = false, int aBoxIndex = 0)
        {
            List<mdBaffle> _rVal = new List<mdBaffle>();
            if (aDowncomer == null) return _rVal;

            aAssy ??= aDowncomer.GetMDTrayAssembly(aAssy);
            if (aAssy == null) return _rVal;

            if (!aAssy.DesignFamily.IsEcmdDesignFamily()) return _rVal;
            
            List<mdDowncomerBox> boxes = aDowncomer.Boxes.FindAll((x) => x.IsVirtual ==false);
            if (boxes.Count <=0) return _rVal;

            List<double> dccenters = aDCCenters != null ? aDCCenters.ToNumericList() : aAssy.Downcomers.XValues(true);
     
            List<mdBaffle> segments = new List<mdBaffle>();
            if (!aHeight.HasValue) aHeight = aAssy.BaffleHeight;
            if(! aZ.HasValue) aZ= aAssy.Deck.Thickness + aAssy.Downcomer().How;
            if(!aZLim.HasValue) aZLim = aAssy.RingSpacing - aAssy.Downcomer().HeightBelowDeck - 0.5 - aZ;
            
            foreach (var box in boxes)
            {
                if(aBoxIndex > 0 &&  box.Index != aBoxIndex) continue;

                _rVal.AddRange(mdPartGenerator.DeflectorPlates_DCBox(box, aAssy, aHeight, aZ, aZLim, dccenters, bSuppressInstances));

            }




            //_rVal.SetPartIndexs();

            return _rVal;
        }
        /// <summary>
        /// the Baffles that are designed for the downcomer box
        /// </summary>
        /// <param name="aBox"></param>
        /// <param name="aAssy"></param>
        /// <param name="aHeight"></param>
        /// <param name="aZ"></param>
        /// <param name="aZLim"></param>
        /// <param name="aDCCenters"></param>
        /// <returns></returns>        
        public static List<mdBaffle>DeflectorPlates_DCBox(mdDowncomerBox aBox, mdTrayAssembly aAssy, double? aHeight = null, double? aZ = null, double? aZLim = null, List<double> aDCCenters = null, bool bSuppressInstances = false)
        {
            List<mdBaffle> _rVal = new List<mdBaffle>();
            if (aBox == null || aBox.IsVirtual) return _rVal;

            aAssy ??= aBox.GetMDTrayAssembly(aAssy);
            if (aAssy == null) return _rVal;

            if (!aAssy.DesignFamily.IsEcmdDesignFamily()) return _rVal;

        

            mzValues dccenters = aDCCenters== null? new mzValues(aAssy.Downcomers.XValues(true)): new mzValues(aDCCenters);
            List<mdBaffle> segments = new List<mdBaffle>();
            if (!aHeight.HasValue) aHeight = aAssy.BaffleHeight;
            if (!aZ.HasValue) aZ = aAssy.Deck.Thickness + aAssy.Downcomer().How;
            if (!aZLim.HasValue) aZLim = aAssy.RingSpacing - aAssy.Downcomer().HeightBelowDeck - 0.5 - aZ;

            mdBaffle aBaffle = new mdBaffle(aBox, aHeight, aZ, aZLim, dccenters);
            double lenlim = mdBaffle.LengthLimit;
            string rguid = aAssy.RangeGUID;
            //lenlim = 96;  // temp for testing should be 10 feet (120)

            if (aBaffle.Length <= lenlim)
            {
                segments.Add(aBaffle);
            }
            else
            {
                if (aAssy.DesignFamily.IsStandardDesignFamily())
                {
                    segments = mdBaffle.DivideBaffles_CenterOut(aBox, aBaffle, aAssy, bSuppressInstances);
                }
                else
                {
                    segments.Add(aBaffle);  //need to add length division in beam case

                }

            }




            foreach (mdBaffle item in segments)
            {
                item._RangeIDS.Clear();
                item.RangeGUID = rguid;
                item.OccuranceFactor = !bSuppressInstances ? item.Instances.Count + 1 : 1;
                item.Quantity = item.OccuranceFactor;
                item.AssociateToDowncomer(aBox);
                _rVal.Add(item);

            }




            //_rVal.SetPartIndexs();

            return _rVal;
        }
        public static List<mdEndPlate> EndPlates_ASSY(mdTrayAssembly aAssy, string aProjectHandle = null, List<mdDowncomerBox> aAssyBoxes = null)
        {
            List<mdEndPlate> _rVal = new List<mdEndPlate>();

            if (aAssy == null) return _rVal;
            List<mdDowncomer> dcs = aAssy.Downcomers.GetByVirtual(aVirtualValue: false);
            if (string.IsNullOrWhiteSpace(aProjectHandle)) aProjectHandle = aAssy.ProjectHandle;


            aAssyBoxes ??= Get_DCBOXES(aProjectHandle, aAssy);
            string rguid = aAssy.RangeGUID;
            foreach (mdDowncomerBox dcbox in aAssyBoxes)
            {
                mdDowncomer dc = dcs[dcbox.DowncomerIndex - 1]; // dcbox.GetMDDowncomer(aAssy);
                List<mdEndPlate> dcparts = mdPartGenerator.EndPlates_DC(dc, dcbox.Height - dcbox.Thickness);
                foreach (mdEndPlate ep in dcparts)
                {
                    ep.AssociateToParent(dcbox, true);
                    ep.AssociateToRange(rguid, true);
                    ep.RingRange = dcbox.RingRange;
                    ep.ProjectHandle = aProjectHandle;
                    ep.Quantity = ep.OccuranceFactor * dcbox.RingRange.RingCount;
                    ep.DowncomerBox = dcbox;

                    mdEndPlate match = _rVal.Find(x => x.CompareTo(ep, false));


                    if (match == null)
                    {
                        string suffix = ep.Y > dcbox.Y ? " - TOP" : " - BOTTOM";
                        if (dcparts.Count <= 1) suffix = string.Empty;

                        ep.NodeName = $"End Plate {dcbox.PartNumber}{suffix}";
                        _rVal.Add(ep);

                    }
                    else
                    {

                        match.Instances.Append(ep.Instances);
                        match.MergeParentAssociations(ep.ParentList);
                        match.Quantity = match.Instances.Count + 1;

                    }
                }
            }


            return _rVal;
        }



        /// <summary>
        /// the end plates that are designed for the downcomer
        /// </summary>
        /// <param name="aDowncomer"></param>
        /// <returns></returns>        
        public static List<mdEndPlate> EndPlates_DC(mdDowncomer aDowncomer, double? aHeight = null)
        {
            List<mdEndPlate> _rVal = new List<mdEndPlate>();
            if (aDowncomer == null) return _rVal;
            List<mdDowncomerBox> boxes = aDowncomer.Boxes;

            foreach (var box in boxes)
            {
                mdEndPlate ep1 = box.EndPlate(bTop: true, aHeight);
                mdEndPlate ep2 = box.EndPlate(bTop: false, aHeight);
                ep1.AssociateToParent(aDowncomer, true);
                ep2.AssociateToParent(aDowncomer, true);

                ep1.NodeName = $"End Plate {aDowncomer.PartNumber}";
                if (ep1.CompareTo(ep2, false))
                {
                    ep1.Instances.Add(ep2.Center - ep1.Center, 180, true);
                    if (aDowncomer.OccuranceFactor > 1)
                        ep1.Instances.AppendRotations(uopVector.Zero, 180);

                    ep1.OccuranceFactor = ep1.Instances.Count + 1;
                    ep1.Quantity = ep1.OccuranceFactor;
                    _rVal.Add(ep1);

                }
                else
                {
                    if (aDowncomer.OccuranceFactor > 1)
                    {
                        ep1.Instances.AppendRotations(uopVector.Zero, 180);
                        ep2.Instances.AppendRotations(uopVector.Zero, 180);
                    }
                    ep1.NodeName = $"End Plate {aDowncomer.PartNumber} - TOP";
                    ep2.NodeName = $"End Plate {aDowncomer.PartNumber} - BOTTOM";
                    ep1.OccuranceFactor = ep1.Instances.Count + 1;
                    ep1.Quantity = ep1.OccuranceFactor;
                    ep2.OccuranceFactor = ep2.Instances.Count + 1;
                    ep2.Quantity = ep2.OccuranceFactor;
                    _rVal.Add(ep1);
                    _rVal.Add(ep2);
                }
            }

            return _rVal;

        }

        public static List<mdEndSupport> EndSupports_ASSY(mdTrayAssembly aAssy, bool bApplyInstances = true, int aDCIndex = 0, int? aCountMultiplier = null, string aProjectHandle = null)
        {
            List<mdEndSupport> _rVal = new List<mdEndSupport>();

            if (aAssy == null) return _rVal;
            List<mdDowncomer> dcs = aAssy.Downcomers.GetByVirtual(aVirtualValue: false);
            if (string.IsNullOrWhiteSpace(aProjectHandle)) aProjectHandle = aAssy.ProjectHandle;
            if (!aCountMultiplier.HasValue) aCountMultiplier = 1;
            foreach (mdDowncomer dcomer in dcs)
            {
                List<mdEndSupport> dcparts = mdPartGenerator.EndSupports_DC(dcomer);

                foreach (mdEndSupport part in dcparts)
                {

                    part.AssociateToRange(aAssy.RangeGUID, true);
                    part.ProjectHandle = aProjectHandle;
                    mdEndSupport match = _rVal.Find(x => x.CompareTo(part, false));


                    if (match == null)
                    {
                        _rVal.Add(part);

                    }
                    else
                    {

                        match.Instances.Append(part.Instances);
                        match.MergeParentAssociations(part.ParentList);
                        match.Quantity = match.Instances.Count + 1;

                    }
                }
            }


            if (aCountMultiplier.HasValue)
            {
                foreach (mdEndSupport part in _rVal)
                {
                    part.Quantity *= aCountMultiplier.Value;
                }

            }


            return _rVal;
        }


        /// <summary>
        /// the end support that are designed for the downcomer
        /// </summary>
        /// <param name="aDowncomer"></param>
        /// <returns></returns>        
        public static List<mdEndSupport> EndSupports_DC(mdDowncomer aDowncomer)
        {
            List<mdEndSupport> _rVal = new List<mdEndSupport>();
            if (aDowncomer == null) return _rVal;
            bool symmetric = aDowncomer.DesignFamily.IsStandardDesignFamily();
            mdEndSupport es1;
            mdEndSupport es2;


            List<mdDowncomerBox> boxes = aDowncomer.Boxes;
            foreach (var box in boxes)
            {
                List<mdEndSupport> supports = box.EndSupports();
                es1 = supports[0];
                es2 = supports[1];
                es1.AssociateToParent(box);
                es2.AssociateToParent(box);

                if (es1.CompareTo(es2, false))
                {
                    es1.Instances.Add(es2.Center - es1.Center, 180, true);
                    if (aDowncomer.OccuranceFactor > 1)
                        es1.Instances.AppendRotations(uopVector.Zero, 180);

                    es1.Quantity = es1.Instances.Count + 1;
                    _rVal.Add(es1);

                }
                else
                {
                    if (aDowncomer.OccuranceFactor > 1)
                    {
                        es1.Instances.AppendRotations(uopVector.Zero, 180);
                        es2.Instances.AppendRotations(uopVector.Zero, 180);
                    }
                    es1.NodeName = $"End Support {aDowncomer.PartNumber} - TOP";
                    es2.NodeName = $"End Support {aDowncomer.PartNumber} - BOTTOM";

                    es1.Quantity = es1.Instances.Count + 1;
                    es2.Quantity = es2.Instances.Count + 1;
                    _rVal.Add(es1);
                    _rVal.Add(es2);
                }

            }






            return _rVal;



        }


        /// <summary>
        ///  executed internally to create the stiffeners collection for the downcomer
        /// </summary>
        /// <param name="aDowncomer"></param>
        /// <param name="aAssy"></param>
        /// <param name="bTrayWide"></param>
        /// <param name="aBoxIndex"></param>
        /// <returns></returns>
        public static List<mdStiffener> Stiffeners_DC(mdDowncomer aDowncomer,  mdTrayAssembly aAssy,  bool bTrayWide = false, int aBoxIndex = 0)
        {   List<mdStiffener> _rVal = new List<mdStiffener>();
            if (aDowncomer == null) return _rVal;
            aAssy ??= aDowncomer.GetMDTrayAssembly();

            if (aAssy == null || aAssy.ProjectType == uppProjectTypes.MDSpout) return _rVal;

         
            List<mdDowncomerBox> boxes = aDowncomer.Boxes.FindAll((x) => !x.IsVirtual);
            if (aBoxIndex > 0) boxes = boxes.FindAll((x) => x.Index == aBoxIndex);

            if (boxes.Count  <= 0 ) return _rVal;

            uppMDDesigns design = aAssy.DesignFamily;

            foreach (mdDowncomerBox box in boxes)
            {

                

                double dcx = Math.Round(box.X, 2);
                List<mdStiffener> stiffnrs = box.Stiffeners();
               
                if(stiffnrs.Count > 0)
                {
                    foreach (mdStiffener stif in stiffnrs)
                    {

                        uopVector u1 = new uopVector(stif.Center);
                        uopVector u2 = null;
                        if (bTrayWide)
                        {
                            if (design.IsStandardDesignFamily())
                            {
                                if (dcx != 0)
                                {
                                    u2 = u1 * -1;
                                    u2.Value = mzUtils.NormAng(u1.Value + 180, false, true);


                                }
                            }
                            else if (design.IsBeamDesignFamily())
                            {
                                u2 = u1 * -1;
                                u2.Value = mzUtils.NormAng(u1.Value + 180, false, true);

                            }
                        }

                        _rVal.Add(stif);

                        if (u2 != null)
                        {
                            _rVal.Add(new mdStiffener(box, u2.Y));
                        }
                    }
                }

            }
            return _rVal;
        }


        public static List<mdStiffener> Stiffeners_ASSY(mdTrayAssembly aAssy, bool bApplyInstances = false, int aDCIndex = 0, int? aCountMultiplier = null, string aProjectHandle = null, bool bTrayWide = true, int aBoxIndex = 0)
        {
            List<mdStiffener> _rVal = new List<mdStiffener>(); // colUOPParts() { PartType = uppPartTypes.Stiffener };

            if (aAssy == null) return _rVal;
            List<mdDeckSection> mansecs = aAssy.DeckSections.Manways;

            List<mdDowncomer> dcomers = aAssy.Downcomers.GetByVirtual(aVirtualValue: false);
            if (aDCIndex > 0) dcomers = dcomers.FindAll((x) => x.Index == aDCIndex);

            mdStiffener stiffnr = null;

            uopVectors cntrs = aAssy.StiffenerCenters(bTrayWide: bTrayWide);  // the radius of the points should carry the supplemental deflector height
            if (cntrs.Count <= 0) return _rVal;

            List<double> notches = cntrs.Radii(bUnique: true, aPrec: 3);
            foreach (var c in notches)
            {
                uopVectors v = new uopVectors(cntrs.FindAll((x) => Math.Round(x.Radius, 3) == c), bCloneMembers: false);
                List<double> Xs = v.Ordinates(bGetY: false, 3);
                List<mdDowncomer> dcs = new List<mdDowncomer>();
                foreach (var xval in Xs)
                {
                    mdDowncomer xdc = dcomers.Find((x) => Math.Round(x.X, 3) == xval);
                    if (xdc != null)
                        dcs.Add(xdc);

                }
                if (dcs.Count <= 0)
                    continue;

                if (bApplyInstances)
                {
                    uopVector v1 = v[0];
                    mdDowncomer dc = dcs[0];
                    stiffnr = dc.Stiffener(v1.Y, aAssy);
                    if (!string.IsNullOrWhiteSpace(aProjectHandle)) stiffnr.ProjectHandle = aProjectHandle;

                    stiffnr.OccuranceFactor = v.Count();
                    stiffnr.Quantity = aCountMultiplier.HasValue ? aCountMultiplier.Value * stiffnr.OccuranceFactor : stiffnr.OccuranceFactor;
                    stiffnr.Instances = uopInstances.FromVectors(v, v1);
                    foreach (var dc1 in dcs) { stiffnr.AssociateToParent(dc1); }
                    _rVal.Add(stiffnr);
                }
                else
                {
                    foreach (var dc in dcs)
                    {
                        if (dc == null) continue;
                        List<uopVector> dcpts = v.FindAll((x) => Math.Round(x.X,1) == Math.Round(dc.X,1));
                        foreach (var v1 in dcpts)
                        {
                            stiffnr = dc.Stiffener(v1.Y, aAssy);
                            if (!string.IsNullOrWhiteSpace(aProjectHandle)) stiffnr.ProjectHandle = aProjectHandle;
                            stiffnr.OccuranceFactor = dc.OccuranceFactor;
                            stiffnr.FingerClipPts = mdUtils.FingerClipPoints(aAssy, stiffnr, mansecs, dc);
                            stiffnr.AssociateToParent(dc);
                            _rVal.Add(stiffnr);
                        }
                    }
                }
            }

            return _rVal;
        }

        /// <summary>
        /// the downcomer boxes that are designed for the downcomer
        /// </summary>
        /// <param name="aDowncomer"></param>
        /// <returns></returns>        
        public static List<mdDowncomerBox> Boxes_DC(mdDowncomer aDowncomer)
        {
            List<mdDowncomerBox> _rVal = new List<mdDowncomerBox>();
            if (aDowncomer == null) return _rVal;
            List<mdDowncomerBox> boxes = aDowncomer.Boxes.FindAll((x) => !x.IsVirtual);
            
            foreach (var box in boxes)
            {
                box.AssociateToParent(aDowncomer, true);
                uopInstances insts = box.Instances;
                insts.Owner = null;
                insts.Clear();
                if (box.OccuranceFactor > 1)
                    insts.Add(new uopVector(-2 * box.X, -2 *box.Y), 180);
                box.Instances = insts;

                box.Quantity = box.Instances.Count + 1;
            
                box.NodeName = $"Downcomer Box {box.PartNumber}";
                _rVal.Add(box);
            }


            return _rVal;
        }
        public static List<mdDowncomerBox> Boxes_ASSY(mdTrayAssembly aAssy, int aDCIndex = 0, int? aCountMultiplier = null, string aProjectHandle = null)
        {
            List<mdDowncomerBox> _rVal = new List<mdDowncomerBox>();

            if (aAssy == null) return _rVal;
            List<mdDowncomer> dcs = aAssy.Downcomers.GetByVirtual(aVirtualValue: false);

            for (int i = 1; i <= dcs.Count; i++)
            {
                mdDowncomer dc = dcs[i - 1];
                if (aDCIndex <= 0 || aDCIndex == i)
                {
                    List<mdDowncomerBox> parts = mdPartGenerator.Boxes_DC(dc);
                    if (parts.Count <= 0) continue;


                    foreach (mdDowncomerBox part in parts)
                    {

                        part.AssociateToRange(aAssy.RangeGUID);
                        if (aCountMultiplier.HasValue) part.Quantity *= aCountMultiplier.Value;
                        if (!string.IsNullOrWhiteSpace(aProjectHandle)) part.ProjectHandle = aProjectHandle;
                        _rVal.Add(part);

                    }

                }

            }


            return _rVal;
        }

        public static List<mdBeam> Beams_ASSY(mdTrayAssembly aAssy, int? aCountMultiplier = null, string aProjectHandle = null)
        {
            List<mdBeam> _rVal = new List<mdBeam>();

            if (aAssy == null) return _rVal;
            if (!aAssy.DesignFamily.IsBeamDesignFamily()) return _rVal;
            mdBeam beam = new mdBeam(aAssy.Beam);
            beam.Quantity = (aCountMultiplier != null) ? aCountMultiplier.Value * beam.OccuranceFactor : beam.OccuranceFactor;
            if(!string.IsNullOrWhiteSpace(aProjectHandle)) beam.ProjectHandle = aProjectHandle;
            _rVal.Add(beam);

            return _rVal;
        }

        public static List<mdDeckSection> DeckSections_ASSY(mdTrayAssembly aAssy, out List<mdDeckSection> rAltRingSections)
        {
            rAltRingSections = new List<mdDeckSection>();

            if (aAssy == null) return new List<mdDeckSection>();
            
            List<mdDeckSection> _rVal = aAssy.DeckSections.UniqueSections(aAssy,out rAltRingSections);

            return _rVal;
        }



        public static List<mdSpliceAngle> ManwaySplices_ASSY(mdTrayAssembly aAssy, bool bApplyInstances = true, int? aCountMultiplier = null, string aProjectHandle = null)
        {
            List<mdSpliceAngle> _rVal = new List<mdSpliceAngle>();


            if (aAssy == null) return _rVal;
            mdSpliceAngle angle;

            string pn = $"{100 * aAssy.RangeIndex + 80}{aAssy.ColumnLetter}";
            List<uopPart> angles = aAssy.SpliceAngles().GetByPartType(uppPartTypes.ManwaySplicePlate);
            if (angles.Count <= 0) return _rVal;

            if (bApplyInstances)
            {
                colDXFVectors ips = new colDXFVectors();
                angle = (mdSpliceAngle)angles[0];
                angle.OccuranceFactor = angles.Count;
                angle.OverridePartNumber = pn;
                ips.Add(angle.CenterDXF);
                if (!string.IsNullOrEmpty(aProjectHandle)) angle.ProjectHandle = aProjectHandle;

                for (int i =2; i <= angles.Count; i++)
                {
                    mdSpliceAngle bangle = (mdSpliceAngle)angles[i-1];
                    ips.Add(bangle.CenterDXF);

                }
                angle.Instances = uopInstances.FromVectors(ips, ips.Item(1));
                angle.Quantity = angle.Instances.Count + 1;
                if (aCountMultiplier.HasValue) angle.Quantity *= aCountMultiplier.Value;
                _rVal.Add(angle);
            }
            else
            {
                foreach (uopPart part in angles)
                {
                    angle = (mdSpliceAngle)part.Clone();
                    angle.OverridePartNumber = pn;
                    if (!string.IsNullOrEmpty(aProjectHandle)) angle.ProjectHandle = aProjectHandle;
                    angle.Quantity = angle.OccuranceFactor;
                    if (aCountMultiplier.HasValue) angle.Quantity *= aCountMultiplier.Value;
                    _rVal.Add(angle);
                }

            }


            return _rVal;
        }

        public static List<mdSpliceAngle> SpliceAngles_ASSY(mdTrayAssembly aAssy, bool bApplyInstances = true, int? aCountMultiplier = null, string aProjectHandle = null)
        {
            List<mdSpliceAngle> _rVal = new List<mdSpliceAngle>();


            if (aAssy == null) return _rVal;
            mdSpliceAngle angle;

            string pn = $"{100 * aAssy.RangeIndex + 60}{aAssy.ColumnLetter}";
            colUOPParts splangles = aAssy.GenerateSpliceAngles(false);
            List<uopPart> angles = splangles.GetByPartType(uppPartTypes.SpliceAngle);
            if (angles.Count <= 0) return _rVal;

            if (bApplyInstances)
            {
                uopVectors ips = uopVectors.Zero;
                angle = (mdSpliceAngle)angles[0];
                angle.OccuranceFactor = angles.Count;
                angle.OverridePartNumber = pn;
                ips.Add(angle.CenterDXF);
                if (!string.IsNullOrEmpty(aProjectHandle)) angle.ProjectHandle = aProjectHandle;

                foreach (uopPart part in angles)
                    ips.Add(part.Center);
                angle.Instances = uopInstances.FromVectors(ips, ips.Item(1));
                angle.Quantity = angle.Instances.Count + 1;
                if (aCountMultiplier.HasValue) angle.Quantity *= aCountMultiplier.Value;
                _rVal.Add(angle);
            }
            else
            {
                foreach (uopPart part in angles)
                {
                    angle = (mdSpliceAngle)part.Clone();
                    angle.OverridePartNumber = pn;
                    angle.Quantity = angle.OccuranceFactor;
                    if (aCountMultiplier.HasValue) angle.Quantity *= aCountMultiplier.Value;
                    if (!string.IsNullOrEmpty(aProjectHandle)) angle.ProjectHandle = aProjectHandle;
                    _rVal.Add(angle);
                }

            }


            return _rVal;
        }

        public static List<mdSpliceAngle> ManwayAngles_ASSY(mdTrayAssembly aAssy, bool bApplyInstances = true, int? aCountMultiplier = null, string aProjectHandle = null)
        {
            List<mdSpliceAngle> _rVal = new List<mdSpliceAngle>();


            if (aAssy == null) return _rVal;
            mdSpliceAngle angle;

            string pn = $"{100 * aAssy.RangeIndex + 80}{aAssy.ColumnLetter}";
            colUOPParts splangles = aAssy.GenerateSpliceAngles(false);
            List<uopPart> angles = splangles.GetByPartType(uppPartTypes.ManwayAngle);
            if (angles.Count <= 0) return _rVal;

            if (bApplyInstances)
            {
                colDXFVectors ips = new colDXFVectors();
                angle = (mdSpliceAngle)angles[0];
                angle.OccuranceFactor = angles.Count;
                angle.OverridePartNumber = pn;
                ips.Add(angle.CenterDXF);
                if (!string.IsNullOrEmpty(aProjectHandle)) angle.ProjectHandle = aProjectHandle;

                for(int i =2; i <= angles.Count; i++)
                {
                    mdSpliceAngle bangle = (mdSpliceAngle)angles[i-1];
                    ips.Add(bangle.CenterDXF);

                }
                angle.Instances = uopInstances.FromVectors(ips, ips.Item(1));
                angle.Quantity = angle.Instances.Count + 1;
                if (aCountMultiplier.HasValue) angle.Quantity *= aCountMultiplier.Value;
                _rVal.Add(angle);
            }
            else
            {
                foreach (uopPart part in angles)
                {
                    angle = (mdSpliceAngle)part.Clone();
                    angle.OverridePartNumber = pn;
                    if (!string.IsNullOrEmpty(aProjectHandle)) angle.ProjectHandle = aProjectHandle;
                    angle.Quantity = angle.OccuranceFactor;
                    if (aCountMultiplier.HasValue) angle.Quantity *= aCountMultiplier.Value;
                    _rVal.Add(angle);
                }

            }


            return _rVal;
        }
        public static List<mdSupplementalDeflector> SuppDefs_ASSY(mdTrayAssembly aAssy, bool bApplyInstances = true, int aDCIndex = 0, int? aCountMultiplier = null, string aProjectHandle = null)
        {
            List<mdSupplementalDeflector> _rVal = new List<mdSupplementalDeflector>();

            if (aAssy == null) return _rVal;
            List<mdDowncomer> dcs = aAssy.Downcomers.GetByVirtual(aVirtualValue: false);

            for (int i = 1; i <= dcs.Count; i++)
            {
                mdDowncomer dc = dcs[i - 1];
                if (aDCIndex <= 0 || aDCIndex == i)
                {
                    if (dc.SupplementalDeflectorHeight > 0)
                    {

                        List<mdDowncomerBox> boxes = dc.Boxes.FindAll((x) => !x.IsVirtual);

                        foreach (var box in boxes)
                        {
                            if (!box.HasSupplementalDeflector) continue;
                            mdSupplementalDeflector part = box.SupplementalDeflector;
                            if (part == null) continue;
                            part.AssociateToParent(box, true);
                            colDXFVectors ips = new colDXFVectors(part.CenterDXF);
                            List<mdSupplementalDeflector> parts = new List<mdSupplementalDeflector> { part };

                            if (!bApplyInstances)
                            {
                                _rVal.AddRange(parts);
                            }
                            else
                            {
                                if (dc.OccuranceFactor > 1) ips.Add(-part.X, -part.Y, part.Z);
                                part.Instances = uopInstances.FromVectors(ips, ips.Item(1));
                                part.Quantity = part.Instances.Count + 1;
                                if (aCountMultiplier.HasValue)
                                    part.Quantity *= aCountMultiplier.Value;

                                foreach (mdSupplementalDeflector item in parts)
                                {

                                    item.AssociateToRange(aAssy.RangeGUID);
                                    mdSupplementalDeflector retpart = _rVal.Find(x => x.IsEqual(item));
                                    if (!string.IsNullOrWhiteSpace(aProjectHandle)) part.ProjectHandle = aProjectHandle;
                                    if (retpart == null)
                                    {

                                        _rVal.Add(part);

                                    }
                                    else
                                    {

                                        retpart.Instances.AppendWithPoints(part.Instances.MemberPoints(), retpart.Instances.BasePt);
                                        retpart.MergeParentAssociations(part.ParentList);
                                        retpart.Quantity += part.Quantity;
                                    }
                                }
                            }
                        }

                       
                    }


                }

            }



            return _rVal;
        }


        public static List<mdEndAngle> EndAngles_ASSY(mdTrayAssembly aAssy, bool bApplyInstances = true, int aDCIndex = 0, int? aCountMultiplier = null, string aProjectHandle = null)
        {
            List<mdEndAngle> _rVal = new List<mdEndAngle>();

            if (aAssy == null) return _rVal;
            List<mdDowncomer> dcs = aAssy.Downcomers.GetByVirtual(aVirtualValue: false);
            uppMDDesigns design = aAssy.DesignFamily;
            foreach (mdDowncomer dc in dcs)
            {
                if (aDCIndex <= 0 || aDCIndex == dc.Index)
                {

                    List<mdDowncomerBox> boxes = dc.Boxes;
                    foreach(mdDowncomerBox box in boxes)
                    {
                        //get all the end angles for the box (should be 4  per box 8 if the box occuance factor is 2) 
                        List<mdEndAngle> parts = mdPartGenerator.EndAngles_DCBox( box, bTrayWide: bApplyInstances);

                        if (parts.Count <= 0) continue;
                  

                        foreach (mdEndAngle part in parts)
                        {
                            if (!string.IsNullOrWhiteSpace(aProjectHandle)) part.ProjectHandle = aProjectHandle;

                            if (!bApplyInstances)
                            {
                                _rVal.Add(part);

                            }
                            else
                            {
                                mdEndAngle match = _rVal.Find(x => x.IsEqual(part, bSuppressMaterialCheck: true));
                                if (match == null)
                                {
                                    _rVal.Add(part);
                                }
                                else
                                {
                                   
                                    match.Instances.Add(part.X - match.X, part.Y - match.Y, aRotation: match.Side == part.Side ? 0 : 180, aPartIndex: box.Index, bVirtual: part.IsVirtual); //  aRotation: match.End == part.End ? 0 : 180
                                    match.OccuranceFactor = match.Instances.Count + 1;
                                    match.Quantity += match.OccuranceFactor;
                                    match.AssociateToParent(box.PartNumber);
                                }
                            }


                        }

                    }

                }

            }


            foreach (mdEndAngle part in _rVal)
            {
                if (aCountMultiplier.HasValue) part.Quantity *= aCountMultiplier.Value;

            }

            return _rVal;
        }


        /// <summary>
        /// executed internally to create the end angles collection
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aDC"></param>

        public static List<mdEndAngle> EndAngles_DC(mdDowncomer aDC, mdTrayAssembly aAssy = null,  bool bTrayWide = false, int aBoxIndex = 0)
        {
            List<mdEndAngle> _rVal = new List<mdEndAngle>();
            if (aDC == null) return _rVal;



            List<mdDowncomerBox> boxes = aDC.Boxes.FindAll((x) => !x.IsVirtual);
            if (boxes.Count <= 0) return _rVal;

            aAssy ??= aDC.GetMDTrayAssembly(aAssy);
            uppMDDesigns design = aAssy != null ? aAssy.DesignFamily : aDC.DesignFamily;

            foreach (mdDowncomerBox box in boxes)
            {
                if(aBoxIndex > 0 && box.Index != aBoxIndex) continue;

                List<mdEndAngle> boxangles = mdPartGenerator.EndAngles_DCBox(box,bTrayWide: bTrayWide);
                _rVal.AddRange(boxangles);


            }

            return _rVal;

        }
        public static List<mdEndAngle> EndAngles_DCBox(mdDowncomerBox aBox,bool bTrayWide = false)
        {
            List<mdEndAngle> _rVal = new List<mdEndAngle>();
            if (aBox == null) return _rVal;
            if (aBox.IsVirtual) return _rVal;
            mdEndSupport ESTop = aBox.EndSupport(bTop: true);
            mdEndSupport ESBot = aBox.EndSupport(bTop: false);
            bool symmetric = aBox.DesignFamily.IsStandardDesignFamily();
            bool bCenterDC = Math.Round(aBox.X, 1) == 0;
            uopHoles bxHoles = new uopHoleArray(aBox.GenHolesV(null, aTag: "END ANGLE")).Item(1);
            uopHoles EsHolesTop = ESTop.GenHoles("END ANGLE").Item(1);
            uopHoles EsHolesBot = ESBot.GenHoles("END ANGLE").Item(1);

            double boxLeftEdge = aBox.X - 0.5 * aBox.Width; // .BoxLns.GetSide(uppSides.Left, true).Value.sp.X;
            double boxRightEdge = aBox.X + 0.5 * aBox.Width; // aBox.BoxLns.GetSide(uppSides.Right, true).Value.sp.X;

            if (!symmetric) symmetric = aBox.IntersectionType_Top == uppIntersectionTypes.ToRing && aBox.IntersectionType_Bot == uppIntersectionTypes.ToRing;

            //=========== TOP RIGHT
            // Upper right end angle
            uopHole aHole = EsHolesTop.GetFlagged("RIGHT");
            uopHole bSlot = bxHoles.GetFlagged("UR");
            double lg = aHole.Y - bSlot.Y;
            uopVector ip = new uopVector(boxRightEdge, aHole.Y - 0.5 * lg, aElevation: aBox.DeckThickness);
            lg += 2 * aHole.Inset;
            mdEndAngle ea_TR = new mdEndAngle(aBox, uppSides.Right, uppSides.Top)
            {
                HoleInset = aHole.Inset,
                Length = lg ,
                Center = ip,
                HoleSpan = lg - 2 * aHole.Inset,
            };
            _rVal.Add(ea_TR);

            // Upper left end angle

            //=========== TOP LEFT
            aHole = EsHolesTop.GetFlagged("LEFT");
            bSlot = bxHoles.GetFlagged("UL");
            lg = aHole.Y - bSlot.Y;
            ip = new uopVector(boxLeftEdge, aHole.Y - 0.5 * lg, aElevation: aBox.DeckThickness);
            lg += 2 * aHole.Inset;
            mdEndAngle ea_TL = new mdEndAngle(aBox, uppSides.Left,uppSides.Top)
            {
                HoleInset = aHole.Inset,
                Length = lg,
                Center = ip,
                HoleSpan = lg - 2 * aHole.Inset,
            };
            _rVal.Add(ea_TL);



            //=========== LOWER RIGHT
            mdEndAngle ea_BR =  null;
            if (symmetric)
            {
                ea_BR = new mdEndAngle(ea_TR) { End = uppSides.Bottom };
                ea_BR.Y *= -1;
                ea_BR.AssociateToParent(aBox, true);
            }
            else
            {
                bSlot = bxHoles.GetFlagged("LR");
                aHole = EsHolesBot.GetFlagged("RIGHT");
                lg = bSlot.Y - aHole.Y;
                ip = new uopVector(boxRightEdge, aHole.Y + 0.5 * lg, aElevation: aBox.DeckThickness);
                lg += 2 * aHole.Inset;
                ea_BR = new mdEndAngle(aBox, uppSides.Right, uppSides.Bottom)
                {
                    HoleInset = aHole.Inset,
                    Length = lg,
                    Center = ip,
                    HoleSpan = lg - 2 * aHole.Inset,
                };
            }

            _rVal.Add(ea_BR);

            //=========== BOTTOM LEFT
            mdEndAngle ea_BL = null;
            if (symmetric)
            {
                ea_BL = new mdEndAngle(ea_TL) { End = uppSides.Bottom };
                ea_BL.Y *= -1;
                ea_BL.AssociateToParent(aBox, true);
            }
            else
            {

                aHole = EsHolesBot.GetFlagged("LEFT");
                bSlot = bxHoles.GetFlagged("LL");
                lg = bSlot.Y - aHole.Y;
                ip = new uopVector(boxLeftEdge, aHole.Y + 0.5 * lg, aElevation: aBox.DeckThickness);
                lg += 2 * aHole.Inset;
                ea_BL = new mdEndAngle(aBox, uppSides.Left, uppSides.Bottom)
                {
                    HoleInset = aHole.Inset,
                    Length = lg,
                    Center = ip,
                    HoleSpan = lg - 2 * aHole.Inset,
                };
            }

            _rVal.Add(ea_BL);

            string rguid = aBox.RangeGUID;
            for(int i = 1; i <= 4; i++ )
            {
                mdEndAngle ea = _rVal[i - 1];
                if (bTrayWide && aBox.OccuranceFactor > 1)
                {
                    mdEndAngle virea = new mdEndAngle(ea)
                    {
                        X = -ea.X,
                        Y = -ea.Y,
                        IsVirtual = true, // this is a virtual part
                        Side = ea.Side == uppSides.Left ? uppSides.Right : uppSides.Left,
                        End = ea.End == uppSides.Top ? uppSides.Bottom : uppSides.Top,
                       // Rotation = ea.Rotation == 180 ? 0 : 180
                    };

                    virea.AssociateToRange(rguid);
                    virea.AssociateToParent(aBox, true);
                    _rVal.Add(virea);
                }

            }
            // Setting quantity and occurance factor

            return _rVal;
        }

        /// <summary>
        /// executed internally to create the finger clips collection
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aDC"></param>

        public static List<mdFingerClip> FingerClips_ASSY(mdTrayAssembly aAssy, List<mdStiffener> aAssyStiffeners = null, bool bApplyInstances = false, bool bTrayWide = true)
        {
            List<mdFingerClip> _rVal = new List<mdFingerClip>();
            aAssyStiffeners ??= mdPartGenerator.Stiffeners_ASSY(aAssy,bApplyInstances: false,bTrayWide: bTrayWide);

            if (aAssy == null) return _rVal;

            List<mdDowncomer> dcs = aAssy.Downcomers.GetByVirtual(aVirtualValue: false);

            uopVectors fCPts = mdUtils.FingerClipPoints(aAssy, dcs, aAssyStiffeners: aAssyStiffeners, aDCIndex: 0, 0, aSide: uppSides.Undefined, bUnsuppressedOnly: false, bTrayWide: bTrayWide);
            if (fCPts.Count <= 0) return _rVal;

            UVECTOR u0 = UVECTOR.Zero;
            uopInstances insts = null;

            for (int i = 1; i <= fCPts.Count; i++)
            {
                uopVector u1 = fCPts.Item(i);
                if (u1.Suppressed)
                    continue;
                mdDowncomer dc = u1.PartIndex > 0 && u1.PartIndex <= dcs.Count ? dcs[u1.PartIndex - 1] : null;
                mdFingerClip aFC = aAssy.FingerClip;
                aFC.Suppressed = u1.Suppressed || u1.Mark;
                aFC.AssociateToRange(aAssy.RangeGUID, true);
                aFC.AssociateToParent(dc);
                aFC.SetCoordinates(u1.X, u1.Y, u1.Value);
                aFC.OccuranceFactor = dc == null ? u1.Col : dc.OccuranceFactor;
                aFC.PanelIndex = u1.Row;

                aFC.Side = u1.Tag.ToUpper() == "LEFT" ? uppSides.Left : uppSides.Right;

                //aFC.Side = aFC.X < dc.X ? uppSides.Left : uppSides.Right;
                aFC.Rotation = aFC.Side == uppSides.Left ? 180 : 0;
                aFC.SubPart(dc);

                aFC.DowncomerIndex = dc == null ? u1.PartIndex : dc.Index;
                if (!aFC.Suppressed)
                {

                    _rVal.Add(aFC);
                    if (insts == null)
                    {
                        u0 = new UVECTOR(aFC.X, aFC.Y) { Value = aFC.Rotation };
                        insts = new uopInstances() { BasePt = new uopVector(u0) { PartIndex = aFC.PartIndex } };
                    }
                    else
                    {
                        insts.Add(aFC.X - u0.X, aFC.Y - u0.Y, u0.Value + aFC.Rotation, aPartIndex: aFC.PartIndex);
                    }
                }

               

                if (aFC.OccuranceFactor > 1  && !u1.Mark)
                {
                    mdFingerClip bFC = aFC.Clone();
                    bFC.X = -u1.X;
                    bFC.Y = -u1.Y;

                    bFC.Side = aFC.Side == uppSides.Right ? uppSides.Left : uppSides.Right;
                    bFC.Rotation = bFC.Side == uppSides.Left ? 180 : 0;
                    //if (u1.Mark)
                    //    bFC.Suppressed = false;
                    //if (!bFC.Suppressed)
                    //{
                        bFC.SubPart(dc);
                        _rVal.Add(bFC);

                        if (insts == null)
                        {
                            u0 = new UVECTOR(bFC.X, bFC.Y) { Value = bFC.Rotation };
                            insts = new uopInstances() { BasePt = new uopVector(u0) { PartIndex = aFC.PartIndex } };
                        }
                        else
                        {
                            insts.Add(bFC.X - u0.X, bFC.Y - u0.Y, u0.Value + bFC.Rotation, aPartIndex: aFC.PartIndex);
                        }
                    //}
                }

            }

            if (bApplyInstances && _rVal.Count > 1)
            {
                mdFingerClip aFC = _rVal[0];
                _rVal.Clear();
                aFC.Instances = insts;
                aFC.Quantity = aFC.Instances.Count + 1;
                _rVal.Add(aFC);
            }

            return _rVal;
        }

        /// <summary>
        /// executed internally to create the finger clips collection
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aDC"></param>

        public static List<mdFingerClip> FingerClips_DC(mdTrayAssembly aAssy, mdDowncomer aDC, List<mdStiffener> aAssyStiffeners = null)
        {
            List<mdFingerClip> _rVal = new List<mdFingerClip>();
            if (aDC == null) return _rVal;
            aAssy ??= aDC.GetMDTrayAssembly(aAssy);

            aAssyStiffeners ??= mdPartGenerator.Stiffeners_ASSY(aAssy, false);


            colUOPParts fcs = mdUtils.FingerClips(aAssy, aAssyStiffeners, aDC.Index);
            foreach (mdFingerClip fc in fcs)
            {
                fc.AssociateToRange(aAssy.RangeGUID, true);
                _rVal.Add(fc);
            }

            return _rVal;
        }



    }
}
