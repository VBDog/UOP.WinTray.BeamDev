using System;
using System.Collections.Generic;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Parts;

using UOP.WinTray.Projects.Tables;

namespace UOP.WinTray.Projects.Utilities
{
    public class mdTables
    {
        public static uopTable GetProjectTable(mdProject aProject, string aTableName, string aList = "")
        {
            uopTable _rVal = new uopTable();
            if (aProject == null) return null;
            aTableName = aTableName.ToUpper().Trim();
            if (aTableName ==  string.Empty) return null;
            switch (aTableName.ToUpper())
            {
                case "HARDWARE":
                    _rVal = xHardware_Project(aProject, aList);
                    break;
            }
            return _rVal;
        }

        private static uopTable xHardware_Project(mdProject aProject, string aList = "")
        {
            uopTable xHardware_Project = new uopTable();
            aList = aList.Trim();
            if (aProject == null) return null;
            try
            {
                int i;
                mdTrayRange aRng;
                colUOPTrayRanges aRngs;
                mdTrayAssembly aAssy = null;
                uopHoleArray aArray;
                uopHoles aHoles;
                int j;
                string hName;
                hdwHexBolt blt = new hdwHexBolt();
                hdwFlatWasher fw = new hdwFlatWasher();
                hdwLockWasher lw = new hdwLockWasher();
                hdwHexNut nt = new hdwHexNut();
                hdwLockWasher lw2 = new hdwLockWasher();
                hdwHexNut nt2 = new hdwHexNut();
                hdwFlatWasher fw2 = new hdwFlatWasher();
                hdwSetScrew sc = new hdwSetScrew();
                hdwStud st = null;
                hdwHoldDownWasher hd = new hdwHoldDownWasher();
                int tcnt = 0;
                double sprs;
                bool bIDE;
                int qty;
                colUOPParts aSAs;
                List<uopPart> SAs;
                mdSpliceAngle aSA;
                colMDDowncomers aDCs = null;
                string aStr;
                int cnt;
                bIDE = uopUtils.RunningInIDE;
                sprs = aProject.SparePercentage;
                
                hd.SubPart(aProject);
                if (aProject.Bolting == uppUnitFamilies.Metric)
                {
                    blt.Size = uppHardwareSizes.M10;
                    blt.Length = 25 / 25.4;
                    nt2.Size = uppHardwareSizes.M12;
                    sc.Length = 50 / 25.4;
                }
                else
                {
                    blt.Size =uppHardwareSizes.ThreeEights;
                    blt.Length = 1;
                    nt2.Size = uppHardwareSizes.OneHalf;
                    sc.Length = 2;
                }
                lw.Size = blt.Size;
                nt.Size = blt.Size;
                sc.Size = blt.Size;
                fw.Size = blt.Size;
                lw2.Size = nt2.Size;
                fw2.Size = nt2.Size;
                aRngs = aProject.TrayRanges;

                if (aRngs.Count <= 0) return null;
                xHardware_Project.AddByString(false, "PART NO.|DESCRIPTION OF PART|TTL REQ'D", aDelimitor: "|");
                for (i = 1; i <= aRngs.Count; i++)
                {
                    aRng = (mdTrayRange)aRngs.Item(i);
                    if (mzUtils.ListContains(aRng.Name(false), aList, bReturnTrueForNullList: true))
                    {
                        tcnt = aRng.TrayCount;
                        aAssy = aRng.TrayAssembly;
                        aDCs = aAssy.Downcomers;
                        aArray = aDCs.GenHoles(aAssy, bSuppressSpouts: true, bTrayWide: true);

                        for (j = 1; j <= aArray.Count; j++)
                        {
                            aHoles = aArray.Item(j);
                            hName = aHoles.Name;
                            switch (hName.ToUpper())
                            {
                                case "END ANGLE":
                                    blt.Add(aHoles.Count * tcnt);
                                    lw.Add(aHoles.Count * tcnt);
                                    nt.Add(aHoles.Count * tcnt);
                                    break;
                                case "BAFFLE MOUNT":
                                case "SPLICE":
                                case "BAFFLE SPLICE":
                                    blt.Add(aHoles.Count * tcnt);
                                    lw.Add(aHoles.Count * tcnt);
                                    nt.Add(aHoles.Count * tcnt);
                                    fw.Add(aHoles.Count * 2 * tcnt);
                                    break;
                                case "FINGER CLIP":
                                    if (aAssy.DesignOptions.WeldedStiffeners)
                                    {
                                        blt.Add(aHoles.Count * tcnt);
                                        lw.Add(aHoles.Count * tcnt);
                                        nt.Add(aHoles.Count * tcnt);
                                    }
                                    break;
                                case "CROSSBRACE":
                                case "APPAN_HOLE":
                                    if (!aAssy.Downcomer().WeldedBottomNuts && !aAssy.DesignOptions.BottomInstall)
                                    {
                                        blt.Add(aHoles.Count * tcnt);
                                    }
                                    lw.Add(aHoles.Count * tcnt);
                                    nt.Add(aHoles.Count * tcnt);
                                    break;
                                case "RING CLIP":
                                    if (aAssy.RingClipSize == uppRingClipSizes.FourInchRC)
                                    {
                                        lw2.Add(aHoles.Count * tcnt);
                                        nt2.Add(aHoles.Count * tcnt);
                                        fw2.Add(aHoles.Count * tcnt);
                                    }
                                    else
                                    {
                                        hd.Add(aHoles.Count * tcnt);
                                        lw.Add(aHoles.Count * tcnt);
                                        nt.Add(aHoles.Count * tcnt);
                                    }
                                    break;
                            }
                        }
                        aArray = aAssy.DeckSections.GenHoles(aAssy, bTrayWide: true);
                        for (j = 1; j <= aArray.Count; j++)
                        {
                            aHoles = aArray.Item(j);
                            hName = aHoles.Name;
                            switch (hName.ToUpper())
                            {
                                case "MANWAY":
                                    if (aAssy.DesignOptions.UseManwayClips)
                                    {
                                        //todo sc.Add(aHoles.Count * tcnt);
                                    }
                                    else
                                    {
                                        if (st == null)
                                        {
                                            //todo st = aAssy.ManwayClamp.Stud;
                                            st.Quantity = aHoles.Count * tcnt;
                                        }
                                        else
                                        {
                                            st.Add(aHoles.Count * tcnt);
                                        }
                                    }
                                    lw.Add(aHoles.Count * tcnt * 2);
                                    nt.Add(aHoles.Count * tcnt * 2);
                                    break;
                                case "RING CLIP":
                                    hd.Add(aHoles.Count * tcnt);
                                    lw.Add(aHoles.Count * tcnt);
                                    nt.Add(aHoles.Count * tcnt);
                                    break;
                                case "BOLT":
                                    fw.Add(aHoles.Count * tcnt);
                                    lw.Add(aHoles.Count * tcnt);
                                    nt.Add(aHoles.Count * tcnt);
                                    break;
                            }
                        }
                        aSAs = aAssy.SpliceAngles(true);
                        SAs = aSAs.GetByPartType(uppPartTypes.ManwaySplicePlate);
                        if (SAs.Count > 0)
                        {
                            aSA = (mdSpliceAngle)SAs[0];
                            qty = aSA.BoltCount * 2 * SAs.Count * tcnt;
                            fw.Add(qty);
                            lw.Add(qty);
                            nt.Add(qty);
                        }
                        SAs = aSAs.GetByPartType(uppPartTypes.SpliceAngle);
                        if (SAs.Count > 0)
                        {
                            aSA = (mdSpliceAngle)SAs[0];
                            qty = aSA.BoltCount * 2 * SAs.Count * tcnt;
                            fw.Add(qty);
                            lw.Add(qty);
                            nt.Add(qty);
                        }
                        SAs = aSAs.GetByPartType(uppPartTypes.ManwayAngle);
                        if (SAs.Count > 0)
                        {
                            aSA = (mdSpliceAngle)SAs[1];
                            qty = aSA.BoltCount * SAs.Count * tcnt;
                            fw.Add(qty);
                            lw.Add(qty);
                            nt.Add(qty);
                        }

                    }

                    if (aAssy.Downcomer().Width > 4)
                    {
                        if (aAssy.DesignOptions.HasAntiPenetrationPans || aAssy.DesignOptions.CrossBraces)
                        {
                            aStr = string.Empty;
                            cnt = 0;
                            if (aDCs.HasSupplementalDeflectors && aAssy.Downcomer().Width <= 8)
                            {
                                for (j = 1; j <= aDCs.Count; j++)
                                {
                                    if (aDCs.Item(j).WeldedBottomNuts)
                                    {
                                        mzUtils.ListAdd(ref aStr, aDCs.Item(j).PartNumber);
                                        //todo dcHoles = aDCs.Item(j).GenHoles(aAssy, , , true, true);
                                        //todo cnt = cnt + dcHoles.MembersCount("CROSSBRACE,APPAN_HOLE");
                                    }
                                }
                            }
                            if (cnt >= 0)
                            {
                                nt.Add(-cnt * tcnt);
                            }
                        }
                    }
                }

                //shaved studs
                if (st != null)
                {
                    if (st.Quantity > 0)
                    {
                        //todo xHardware_Project.AddByString(false, "65|" + st.FriendlyName + " **|" + uopUtils.CalcSpares(st.Quantity, sprs, minCnt), aDelimitor: "|");
                        xHardware_Project.SetDimensions(xHardware_Project.Rows + 1, xHardware_Project.Cols);
                    }
                }
                //fender washers
                if (hd.Quantity > 0)
                {
                    //todo xHardware_Project.AddByString(false, "-|" + hd.FriendlyName + " **|" + uopUtils.CalcSpares(hd.Quantity, sprs, minCnt),aDelimitor: "|");
                    xHardware_Project.SetDimensions(xHardware_Project.Rows + 1, xHardware_Project.Cols);
                }

                //Bolts
                qty = 0;
                if (blt.Quantity > 0)
                {
                    qty = blt.Quantity;
                    //todo xHardware_Project.AddByString(false, "-|" + blt.FriendlyName + " **|" + uopUtils.CalcSpares(blt.Quantity, sprs, minCnt), aDelimitor: "|");
                }
                if (qty > 1)
                {
                    xHardware_Project.SetDimensions(xHardware_Project.Rows + 1, xHardware_Project.Cols);
                }
                //'nuts

                qty = 0;
                if (nt.Quantity > 0)
                {
                    qty += nt.Quantity;
                    //todo xHardware_Project.AddByString(false, "-|" + nt.FriendlyName + " **|" + uopUtils.CalcSpares(nt.Quantity, sprs, minCnt), aDelimitor: "|");
                }
                if (nt2.Quantity > 0)
                {
                    qty += nt2.Quantity;
                    //todo xHardware_Project.AddByString(false, "-|" + nt2.FriendlyName + " **|" + uopUtils.CalcSpares(nt2.Quantity, sprs, minCnt), aDelimitor: "|");
                }

                if (qty > 1)
                {
                    xHardware_Project.SetDimensions(xHardware_Project.Rows + 1, xHardware_Project.Cols);
                }
                //lockwashers
                qty = 0;
                if (lw.Quantity > 0)
                {
                    qty += lw.Quantity;
                    //todo xHardware_Project.AddByString(false, "-|" + lw.FriendlyName + " **|" + uopUtils.CalcSpares(lw.Quantity, sprs, minCnt), aDelimitor: "|");
                }
                if (lw2.Quantity > 0)
                {
                    qty += lw2.Quantity;
                    //todo xHardware_Project.AddByString(false, "-|" + lw2.FriendlyName + " **|" + uopUtils.CalcSpares(lw2.Quantity, sprs, minCnt), aDelimitor: "|");
                }

                if (qty > 1)
                {
                    xHardware_Project.SetDimensions(xHardware_Project.Rows + 1, xHardware_Project.Cols);
                }
                //'lockwashers
                qty = 0;
                if (fw.Quantity > 0)
                {
                    qty += fw.Quantity;
                    //todo xHardware_Project.AddByString(false, "-|" + fw.FriendlyName + " **|" + uopUtils.CalcSpares(fw.Quantity, sprs, minCnt), aDelimitor: "|");
                }
                if (fw2.Quantity > 0)
                {
                    qty += fw2.Quantity;
                    //todo xHardware_Project.AddByString(false, "-|" + fw2.FriendlyName + " **|" + uopUtils.CalcSpares(fw2.Quantity, sprs, minCnt), aDelimitor: "|");
                }

                if (qty > 1)
                {
                    xHardware_Project.SetDimensions(xHardware_Project.Rows + 1, xHardware_Project.Cols);
                }
                //set screw
                //if (sc.Quantity > 0)
                {
                    //xHardware_Project.AddByString(false, "-|" + sc.FriendlyName + " **|" + uopUtils.CalcSpares(sc.Quantity, sprs, minCnt), aDelimitor: "|");
                    xHardware_Project.SetDimensions(xHardware_Project.Rows + 1, xHardware_Project.Cols);
                }
                return xHardware_Project;
            }
            catch (Exception exception)
            {
                LoggerManager log = new LoggerManager();
                log.LogError(exception.Message);
                return null;
            }
        }
        internal static uopTable GetTable(mdTrayAssembly aAssy  , string aTableName  , uppUnitFamilies aUnits  ,int MinRows = 0, colMDDowncomers aDowncomers = null,  colMDSpoutGroups aSpoutGroups = null, colMDDeckPanels aDeckPanels = null)
        {
            throw new NotImplementedException();
        }
    }
}
