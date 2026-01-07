using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UOP.DXFGraphics;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Parts;

using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;

namespace UOP.WinTray.Projects.Generators
{
    public class mdSplicing
    {
        public static Task<uopDeckSplices> GenerateSplices_UNIFORM(mdTrayAssembly aAssy, uopDeckSplices aOldSplices = null, uppSpliceStyles aSpliceStyle = uppSpliceStyles.Undefined, double aManwayHeight = 0, double aPanelHeight = 0, Action<string> statusUpdater = null)
        {
            return Task.Run<uopDeckSplices>(() =>
            {
                uopDeckSplices _rVal = new uopDeckSplices(aAssy);
                //^used to create the default splicing for the assembly
                if (aAssy == null) return _rVal;

                _rVal.SubPart(aAssy);


                List<mdDeckPanel> DPs = aAssy.DeckPanels.GetByVirtual(false);
                List<uopDeckSplice> vCol = new List<uopDeckSplice>();
                uopDeckSplice aSpl = null;

                List<string> manhdls = null;
                double fldHt = Math.Round(aPanelHeight, 6);
                double ManHt = Math.Round(aManwayHeight, 6);
                double y1 = 0;
                double x1 = 0;
                double rrad = aAssy.RingID / 2;
                double yMax = rrad - 4;

                uppSpliceStyles sstyle = uppSpliceStyles.Undefined;
                int cnt = 0;

               

                if (aOldSplices != null)
                {
                    manhdls = aOldSplices.ManwayHandles();
                    vCol = aOldSplices.GetVertical(bReturnClones: true);
                    aOldSplices.GetVars(out ManHt, out fldHt, out sstyle);

                    if ((int)aSpliceStyle < 0 || (int)aSpliceStyle > 2) aSpliceStyle = sstyle;
                    if (aOldSplices.Count == 0)
                    {
                        fldHt = Math.Round(aPanelHeight, 6);
                        ManHt = Math.Round(aManwayHeight, 6);
                    }
                }
                else
                {
                    sstyle = aSpliceStyle;
                }

                if ((int)sstyle < 0 || (int)sstyle > 2) sstyle = aAssy.DesignOptions.SpliceStyle;
                if (ManHt <= 0) ManHt = mdUtils.IdealManwayHeight( aAssy,sstyle);
                fldHt = mzUtils.LimitedValue(fldHt, 4, 4 * aAssy.DowncomerSpacing, aAssy.DowncomerSpacing / 2);

                if (ManHt <= 8) ManHt = 8;

                _rVal.SubPart(aAssy);
                _rVal.ManwayHeight = ManHt;
                _rVal.SpliceStyle = sstyle;
                List<double> yVals = new List<double>();
                y1 = 0;
                while (y1 < yMax)
                {
                    y1 = (y1 == 0) ? 0.5 * fldHt : y1 + fldHt;

                    if (y1 <= yMax)
                    {
                        yVals.Add(y1);
                        yVals.Add(-y1);
                    }

                }
                yVals.Sort();
                yVals.Reverse();

                foreach (mdDeckPanel aDP in DPs)
                {
                    var panelShapes = mdDeckPanel.GetPanelShapes(aDP, aAssy);
                    if (aDP == null) continue;
                    if (aDP.IsHalfMoon)
                    {
                        _rVal.Sort(aSubset: vCol);
                        for (int j = 1; j <= vCol.Count; j++)
                        {
                            if (statusUpdater != null)
                            {
                                statusUpdater($"Generating splices for Panel {aDP.Index} ({j}/{vCol.Count})");
                            }

                            _rVal.AddVerticalSplices(aAssy, sstyle, aOrdinate: vCol[j - 1].Ordinate);
                        }
                    }
                    else
                    {
                        cnt = 0;
                        x1 = aDP.Left(true);
                        yMax = Math.Sqrt(Math.Pow(rrad, 2) - Math.Pow(x1, 2)) - 1;

                        for (int j = 0; j < yVals.Count; j++)
                        {
                            if (statusUpdater != null)
                            {
                                statusUpdater($"Generating splices for Panel {aDP.Index} ({j + 1}/{yVals.Count})");
                            }

                            y1 = yVals[j];

                            aSpl = null;
                            if (panelShapes.Any(ps => mdDeckPanel.IsValidPanelShapeForDeckSpliceOrdinate(ps, y1, mdDeckPanel.MinDistanceFromTopOrBottom)))
                            {
                                aSpl = _rVal.AddHorizontalSpliceMD(aAssy: aAssy, aPanelIndex: aDP.Index, aOrdinate: y1, aSnapToMax: false, bDontVerify: true, aSpliceStyle: sstyle);

                                if (aSpl != null)
                                {
                                    cnt += 1;
                                    aSpl.Index = cnt;
                                }
                            }
                        }
                    }
                }

                if (statusUpdater != null)
                {
                    statusUpdater("");
                }

                string sErr = string.Empty;
                for (int i = 1; i <= manhdls.Count; i++)
                {
                    _rVal.AddManway(aAssy, ManHt, Convert.ToString(manhdls[i - 1]), out sErr);
                }
                _rVal.SpliceStyle = aSpliceStyle;
                _rVal.Verify(aAssy);

                return _rVal;
            });
        }

        /// <summary>
        /// used to create the default splicing for the assembly
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aBuffer"></param>
        /// <param name="aOldSplices"></param>
        /// <param name="aSpliceStyle"></param>
        /// <param name="aManwayHeight"></param>
        /// <returns></returns>
        public static Task<uopDeckSplices> GenerateSplices_STRADLE(mdTrayAssembly aAssy, double aPanelHeight, uopDeckSplices aOldSplices = null, uppSpliceStyles aSpliceStyle = uppSpliceStyles.Undefined, double aManwayHeight = 0, Action<string> statusUpdater = null)
        {
            return Task.Run<uopDeckSplices>(() =>
            {
                uopDeckSplices _rVal = new uopDeckSplices(aAssy);
                if (aAssy == null) return _rVal;

                _rVal.SubPart(aAssy);

                List<mdDeckPanel> DPs = aAssy.DeckPanels.GetByVirtual(false);
                List<mdDowncomer> DCs = aAssy.Downcomers.GetByVirtual(false);
                if (aAssy.IsStandardDesign && aAssy.OddDowncomers)
                    DPs.Add(aAssy.DeckPanels.Item(DPs.Count + 1));

                List<uopDeckSplice> vCol = new List<uopDeckSplice>();
                uopDeckSplice aSpl = null;

                List<string> manhdls = null;
                double rrad = aAssy.RingID / 2;
                double pht = Math.Round(aPanelHeight, 6);
                double ManHt = Math.Round(aManwayHeight, 6);
                double y1 = 0;
                double x1 = 0;
                double yMax = rrad - 4;
                double dcWd = aAssy.Downcomer().BoxWidth / 2;

                List<double> yVals = new List<double>();
                foreach (mdDowncomer dc in DCs)
                {
                    y1 = Math.Round(dc.X, 6);
                    yVals.Add(y1 + pht / 2);
                    yVals.Add(-(y1 + pht / 2));
                    yVals.Add(y1 - pht / 2);
                    yVals.Add(-(y1 - pht / 2));
                }

                yVals.Sort();
                yVals.Reverse();

                uppSpliceStyles sstyle = uppSpliceStyles.Undefined;
                int cnt = 0;

                if (aOldSplices != null)
                {
                    manhdls = aOldSplices.ManwayHandles();
                    vCol = aOldSplices.GetVertical(bReturnClones: true);
                    aOldSplices.GetVars(out ManHt, out double _pht, out sstyle);

                    if ((int)aSpliceStyle < 0 || (int)aSpliceStyle > 2) aSpliceStyle = sstyle;

                }
                else
                {
                    sstyle = aSpliceStyle;
                }

                if (sstyle < 0 || (int)sstyle > 2) sstyle = aAssy.SpliceStyle;

                if (ManHt <= 0) ManHt = mdUtils.IdealManwayHeight( aAssy,sstyle);
                if (pht < 4) pht = mdUtils.IdealSectionHeight(aAssy,sstyle);

                if (ManHt <= 8) ManHt = 8;
                //if (pht < -(dcWd - 1.5))  pht = -(dcWd - 1.5);

                _rVal.SubPart(aAssy);
                _rVal.ManwayHeight = ManHt;
                _rVal.SpliceStyle = sstyle;

                


                foreach (mdDeckPanel dp in DPs)
                {
                    //var panelShapes =   mdDeckPanel.GetPanelShapesUsingPanelIndex(aAssy, dp.Index);
                    var panelShapes = mdDeckPanel.GetPanelShapes(dp, aAssy);
                   
                    if (dp.IsHalfMoon)
                    {
                        _rVal.Sort(aSubset: vCol);
                        for (int j = 1; j <= vCol.Count; j++)
                        {
                            if (statusUpdater != null)
                                statusUpdater($"Generating splices for Panel {dp.Index} ({j}/{vCol.Count})");

                            _rVal.AddVerticalSplices(aAssy, sstyle, aOrdinate: vCol[j - 1].Ordinate);
                        }
                    }
                    else
                    {
                        cnt = 0;
                        x1 = dp.Left();
                        yMax = Math.Sqrt(Math.Pow(rrad, 2) - Math.Pow(x1, 2)) - 1;

                        for (int j = 0; j < yVals.Count; j++)
                        {
                            if (statusUpdater != null)
                                statusUpdater($"Generating splices for Panel {dp.Index} ({j + 1}/{yVals.Count})");

                            y1 = yVals[j];
                            aSpl = null;
                            if (panelShapes.Any(ps => mdDeckPanel.IsValidPanelShapeForDeckSpliceOrdinate(ps, y1, mdDeckPanel.MinDistanceFromTopOrBottom)))
                            {
                                aSpl = _rVal.AddHorizontalSpliceMD(aAssy: aAssy, aPanelIndex: dp.Index, aOrdinate: y1, aSnapToMax: false, bDontVerify: true, aSpliceStyle: sstyle);

                                if (aSpl != null)
                                {
                                    cnt += 1;
                                    aSpl.Index = cnt;
                                }
                            }
                        }
                    }
                }

                if (statusUpdater != null)
                    statusUpdater(string.Empty);
      
                for (int i = 1; i <= manhdls.Count; i++)
                    _rVal.AddManway(aAssy, ManHt, Convert.ToString(manhdls[i - 1]), out string sErr);

                _rVal.SpliceStyle = aSpliceStyle;
                _rVal.Verify(aAssy);

                return _rVal;
            });
        }


   
    
        /// <summary>
        /// ensures that the splices are symmetric across both X and Y
        /// </summary>
        /// <param name="aSplices"></param>
        /// <param name="aAssy"></param>
        public static void VerifySplices(List<uopDeckSplice> aSplices, mdTrayAssembly aAssy)
        {

            List<uopDeckSplice> _rVal = new List<uopDeckSplice>();
            if (aSplices == null || aAssy == null || aSplices.Count <= 0) return;
            try
            {
                   
                mdSplicing.Sort(aSplices, aAssignIndices: true);

                colMDDeckPanels DPs = aAssy.DeckPanels;
                uppSpliceStyles aStyle = aAssy.DesignOptions.SpliceStyle;
                List<uopDeckSplice> members = aSplices;

                List<uopDeckSplice> panelmembers;
                List<Tuple<double, uopDeckSplice>> sortlist;
                uopDeckSplice splicebefore;
                uopDeckSplice spliceafter;
                uopDeckSplice splice;
                bool keep;
                if (aStyle < 0) aStyle = aAssy.DesignOptions.SpliceStyle;

                foreach (mdDeckPanel panel in DPs)
                {

                    panelmembers = members.FindAll(x => x.PanelIndex == panel.Index);
                    sortlist = new List<Tuple<double, uopDeckSplice>>();
                    //filter for bounds
                    for (int i = 1; i <= panelmembers.Count; i++)
                    {

                        splice = panelmembers[i - 1];
                
                        splice.SpliceStyle = aStyle;
                        //save only valid members

                        keep = true;
                        if (panel.IsHalfMoon)
                        {
                            if (Math.Round(Math.Abs(splice.X) - splice.MinOrd, 1) < 0 || Math.Round(splice.MaxOrd - Math.Abs(splice.X), 1) < 0)
                                keep = false;
                        }
                        else
                        {
                            if (Math.Round(splice.MaxOrd - Math.Abs(splice.Y), 1) < 0) keep = false;

                        }
                        if (keep) sortlist.Add(new Tuple<double, uopDeckSplice>(splice.Ordinate, splice));

                    }


                    //sort high to low for field panels and low to high for moon panels                    
                    if (panel.IsHalfMoon)
                        if (sortlist.Count > 1) sortlist = sortlist.OrderByDescending(t => t.Item1).ToList();
                        else
                        if (sortlist.Count > 1) sortlist = sortlist.OrderBy(t => t.Item1).ToList();


                    //set splice indices and filter by proximity
                    panelmembers = new List<uopDeckSplice>();
                    for (int i = 1; i <= sortlist.Count; i++)
                    {
                        splice = sortlist[i - 1].Item2;
                        keep = true;
                        splicebefore = i <= 1 ? null : sortlist[i - 2].Item2;
                        spliceafter = i >= sortlist.Count ? null : sortlist[i].Item2;
                        if (panel.IsHalfMoon)
                        {
                            if (splicebefore != null && Math.Abs(splicebefore.Ordinate - splice.Ordinate) <= 2) keep = false;
                        }
                        else
                        {
                            if (splicebefore != null && Math.Abs(splicebefore.Ordinate - splice.Ordinate) <= 2) keep = splice.SupportsManway;
                        }


                        if (keep)
                        {
                            splice.SpliceIndex = panelmembers.Count + 1;
                       
                            panelmembers.Add(splice);
                        }
                    }

                    //set the tags
                    //SetSpliceMFTags(panelmembers, panel.Index, aStyle, aAssy);

                    _rVal.AddRange(panelmembers);

                }


            }
            catch (Exception exception)
            {
                throw exception;
            }
            finally
            {
                 if(aSplices != null)aSplices.Clear();
                foreach (uopDeckSplice item in _rVal)
                {
                    aSplices.Add(item);
                }
            }


        }
        

        /// <summary>
        /// sorts the splices in the collection in the requested order
        /// </summary>
        /// <param name="aSplices"></param>
        /// <param name="aAssignIndices">the reference point to use for sorting clockwise or counter-clockwise or nearest to farthest </param>
        public static List<uopDeckSplice> Sort(List<uopDeckSplice> aSplices, bool aAssignIndices = true)
        {
            if (aSplices == null) return null;

            List<int> pids = GetPanelIDS(aSplices);

            List<uopDeckSplice> keep = new List<uopDeckSplice>();

            foreach (var pid in pids)
            {
                List<uopDeckSplice> panel = aSplices.FindAll((x) => x.PanelIndex == pid);
                panel = mdSplicing.SortSet(panel, pid == 1);
                foreach (var splice in panel)
                {
                    if (aAssignIndices) splice.SpliceIndex = keep.Count + 1;
                    keep.Add(splice);
                }
            }

            return keep;


        }
        public static List<int> GetPanelIDS(List<uopDeckSplice> aSplices, bool bHighToLow = false)
        {
            List<int> _rVal = new List<int>();
            if (aSplices == null) return _rVal;
            foreach (uopDeckSplice item in aSplices)
            {
                if (!_rVal.Contains(item.PanelIndex)) _rVal.Add(item.PanelIndex);
            }
            _rVal.Sort();
            if (bHighToLow) _rVal.Reverse();
            return _rVal;
        }
     
    
        /// <summary>
        /// sorts the splices in the collection in the requested order
        /// </summary>
        /// <param name="aSplices"></param>
        /// <param name="aAssignIndices">the reference point to use for sorting clockwise or counter-clockwise or nearest to farthest </param>
        public static List<uopDeckSplice> SortSet(List<uopDeckSplice> aSplices, bool bLowToHigh)
        {
            if (aSplices == null) return null;

            List<uopDeckSplice> keep = new List<uopDeckSplice>();
            List<Tuple<double, uopDeckSplice>> sort;

            uopDeckSplice splice;

            sort = new List<Tuple<double, uopDeckSplice>>();
            foreach (var splc in aSplices)
            {
                sort.Add(new Tuple<double, uopDeckSplice>(splc.Ordinate, splc));

            }

            if (!bLowToHigh)
            {
                sort = sort.OrderByDescending(t => t.Item1).ToList();
            }
            else
            {
                sort = sort.OrderBy(t => t.Item1).ToList();
            }

            foreach (Tuple<double, uopDeckSplice> tpl in sort)
            {
                splice = tpl.Item2;

                keep.Add(splice);

            }

            return keep;
        }
     
    

    }


}
