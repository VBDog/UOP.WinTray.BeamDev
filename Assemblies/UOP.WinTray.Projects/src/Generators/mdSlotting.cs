using System;
using System.Collections.Generic;
using System.Linq;
using UOP.DXFGraphics;
using UOP.DXFGraphics.Utilities;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Events;
using UOP.WinTray.Projects.Parts;

using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;
using static System.Collections.Specialized.BitVector32;

namespace UOP.WinTray.Projects.Generators
{

    public class mdSlotting
    {



        /// <summary>
        ///builds the slot zones for the passed assembly
        /// </summary>
        /// <remarks>
        ///creates the zone boundaries and invalid islands based on the deck sections of the assembly.
        /// </remarks>
        /// <param name="aAssy">the subject assembly</param>
        /// <param name="aDeckSections">the subect deck sections</param>
        /// <param name="aExistingZones">the current zones to persist current pitches and suppression locations</param>
        /// <returns></returns>
        internal static mdSlotZones SlotZones_Create(mdTrayAssembly aAssy, List<mdDeckSection> aDeckSections, mdSlotZones aExistingZones)
        {
          
            if (aAssy == null || aDeckSections == null) return new mdSlotZones { Invalid = false };
            // aAssy.RaiseStatusChangeEvent("Creating Slot Zones");
            mdSlotZones _rVal =  new mdSlotZones { Invalid = false };
            mdSlotZones oldzones =  aExistingZones == null ? new mdSlotZones { Invalid = false } :  aAssy._SlotZones ;
            oldzones.SetTrayAssembly(null);
            _rVal.SetTrayAssembly(aAssy);
            
            _rVal.Clear();
            _rVal.Invalid = true;
            if (aDeckSections.Count <=0) return _rVal;

            uppFlowSlotTypes sType = _rVal.SlotType;
            uppProjectTypes pType = _rVal.ProjectType;
            double wht = _rVal.WeirHeight;
            string sGUID = _rVal.RangeGUID;
            string pHndl = _rVal.ProjectHandle;
            double pctng = _rVal.SlottingPercentage;
            bool slotsrequired = _rVal.SlotsRequired;
            if (slotsrequired) aAssy.RaiseStatusChangeEvent($"Creating {aAssy.TrayName()} Slot Zones");

            foreach(var ds in aDeckSections)
            {
                if (ds == null) continue;
                uopSectionShape baseshape = ds.BaseShape;
                if (baseshape == null) continue;
                mdSlotZone oldzone = oldzones.Find(x => x.Handle == baseshape.Handle);
                double pnlXPitch = 0;
                double pnlYPitch = 0;
                dxxPitchTypes pnlPitch = dxxPitchTypes.Triangular;
                if (oldzone != null)
                {
                    pnlXPitch = oldzone.HPitch;
                    pnlYPitch = oldzone.VPitch;
                    pnlPitch = oldzone.PitchType;
                }
                else
                {
                    mdSlotZone similarzone = oldzones.Find(x => x.PanelIndex == ds.PanelIndex && x.IsManway == ds.IsManway && x.HPitch !=0 );
                    if (similarzone == null) similarzone = oldzones.Find(x => x.PanelIndex == ds.PanelIndex && x.HPitch != 0);
                    if (similarzone != null)
                    {
                        pnlXPitch = similarzone.HPitch;
                        pnlYPitch = similarzone.VPitch;
                        pnlPitch = similarzone.PitchType;
                    }
                }
                if (slotsrequired) aAssy.RaiseStatusChangeEvent($"Creating {aAssy.TrayName()} Section {baseshape.Handle} Slot Zone");


                mdSlotZone newzone = new mdSlotZone(baseshape, ds, aAssy, pnlXPitch, pnlYPitch, bUpdateSlots: slotsrequired);
                if (oldzone != null)
                {
                    uopVectors suppressed = oldzone.GridPoints(true);
                    if(suppressed != null && suppressed.Count > 0) 
                    {
                        uopVectors newpts = newzone.GridPts;
                        foreach (var sup in suppressed)
                        {
                            uopVector u1 = newpts.Nearest(sup, 0.05);
                            if (u1 != null) u1.Suppressed = true;

                        }
                    }
                    
                }
                    _rVal.Add(newzone);
            }
            _rVal.Invalid = false;

            return _rVal;
        }

    

        /// <summary>
        ///the number of flow slots required for the entire tray based on the function active area and the slot stype
        ///(function area * Fs)/ slot area
        ///only applicable for ECMD trays
        /// </summary>
        /// <param name="aSlotType"></param>
        /// <param name="aSlottingPercentage"></param>
        /// <param name="aFunctionalActiveArea"></param>
        /// <returns></returns>
        public static int TotalRequiredSlotCount(uppFlowSlotTypes aSlotType, double aSlottingPercentage, double aFunctionalActiveArea)
        {

            double sArea = aSlotType.SlotArea();
            double Fs = aSlottingPercentage / 100;

            return (sArea != 0) ? Convert.ToInt32(Math.Round(Fs * aFunctionalActiveArea / sArea, 0)) : 0;

        }

        internal static int SetSlotCounts(mdTrayAssembly aAssy, mdSlotZones aZones, double aFunctionalArea = 0, bool aRaiseEvents = false)
        {
            int _rVal = 0;
            if (aZones == null) return 0;
            try
            {
                aAssy ??= aZones.TrayAssembly();

                if (aAssy == null) return _rVal;

                if (aFunctionalArea <= 0) aFunctionalArea = aAssy.FunctionalActiveArea;

                aZones.SlottingPercentage = aAssy.Deck.SlottingPercentage;
                aZones.FunctionArea = aFunctionalArea;
                aZones.RequiredSlotCount = 0;
                aZones.SlotType = aAssy.Deck.SlotType;
                int totCnt = 0;
                double totArea = aAssy.FreeBubblingAreas.TotalFreeBubblingArea;

                double pfrac = 0;

                if (!aAssy.DesignFamily.IsEcmdDesignFamily() || aZones.SlottingPercentage <= 0)
                {
                    for (int i = 1; i <= aZones.Count; i++)
                    {
                        mdSlotZone tmdZone = aZones.Item(i);
                        tmdZone.Clear();
                        //aZones.Item(i).XPitch = 0;
                        //aZones.Item(i).YPitch =0;
                        //aZones.Item(i).GridPts = UVECTORS.Zero;
                    }
                    return _rVal;
                }
                if (aRaiseEvents) aAssy.SetPartGenerationStatus($"Setting {aAssy.NodeName} Required Slot Counts");

                totCnt = TotalRequiredSlotCount(aZones.SlotType, aZones.SlottingPercentage, aZones.FunctionArea);
                aZones.RequiredSlotCount = totCnt;
                _rVal = totCnt;
                List<mdDeckSection> decksections = aAssy.DeckSections.GetByVirtual(aVirtualValue: false);

                if (totCnt <= 0)
                {
                    for (int i = 1; i <= aZones.Count; i++)
                    {
                        mdSlotZone aZn = aZones.Item(i);
                        aZn.Clear();

                    }
                    return _rVal;
                }
                colMDDeckPanels DPs = aAssy.DeckPanels;

                //tabulate the total available mechanical area
                //totArea = aAssy.FreeBubblingAreas.TotalArea;
                //foreach (mdDeckPanel mem in DPs.GetByVirtual(false))
                //{

                //    totArea += mem.MechanicalArea * mem.OccuranceFactor;
                //}


                for (int i = 1; i <= DPs.Count; i++)
                {
                    uopFreeBubblingPanel fbpanel = null;
                    mdDeckPanel aDP = DPs.Item(i);
                    if (aDP.IsVirtual)
                    {
                        mdDeckPanel parent = aDP.Parent;
                        if (parent == null) continue;
                        aDP.TotalRequiredSlotCount = parent.TotalRequiredSlotCount;


                    }
                    else
                    {
                        //determine home much of the area this panel accounts for
                        fbpanel = aDP.FreeBubblingPanel(aAssy);
                        pfrac = fbpanel.TrayFraction;
                        aDP.TotalRequiredSlotCount = (int)Math.Round(pfrac * totCnt, 0);
                    }
                    fbpanel ??= aDP.FreeBubblingPanel(aAssy);
                    double panelFBA = aDP.FreeBubblingArea();
                    List<mdSlotZone> pzones = aZones.FindAll(x => x.PanelIndex == i);
                    foreach (mdSlotZone item in pzones)
                    {
                        mdDeckSection dsec = decksections.Find((x) => string.Compare(x.Handle, item.SectionHandle, true) == 0);
                        //double zoneArea = item.BoundaryV.Area;
                        if(dsec != null)
                        {
                            item.PanelFraction = dsec.MechanicalPanelFraction;
                            item.TotalRequiredSlotCount = (int)(item.PanelFraction * aDP.TotalRequiredSlotCount);
                        }
                    }
                }
            }
            finally
            {

                if (aRaiseEvents) aAssy.SetPartGenerationStatus("");


            }
            return _rVal;
        }
  

        public static void EstimatePitches(mdTrayAssembly aAssy,out double rIdealHPitch, out double rIdealVPitch)
        {
            rIdealHPitch = 0;
            rIdealVPitch = 0;

            if (aAssy == null) return;
            double HPitch = 0;
            double VPitch = 0;

            try
            {

                List<mdDeckPanel> panels = aAssy.DeckPanels.ActivePanels(aAssy, false);
                if (panels.Count == 0) return;

                mdDeckPanel panel = panels.Last();
                int target = panel.RequiredSlotCount;
                if (target <= 0) return;
                DowncomerDataSet dcdata = aAssy.DowncomerData;

                uopPanelSectionShapes panelshapes = dcdata.PanelShapes(true, panel.Index);
                if (panelshapes.Count <= 0) return;
                double minh = mdGlobals.MinSlotXPitch;
                uopPanelSectionShape panelshape = panelshapes.Last();
             
               
               
                if (panelshapes.Count == 2)
                {
                    uopPanelSectionShape shape1 = panelshapes[0];
                    uopPanelSectionShape shape2 = panelshapes[1];
                    double totarea = shape1.Area + shape2.Area;
                    if(shape1.Area > shape2.Area)
                    {
                        panelshape = shape1;
                        target = (int) Math.Truncate((shape1.Area / totarea) * target);
                    }
                    else
                    {
                        panelshape = shape2;
                        target = (int)Math.Truncate((shape1.Area / totarea) * target);
                    }
                }
                uopSectionShape section = panelshapes.Last().ToSectionShape(aAssy);

                uopCompoundShape slotbounds = section.SlotZoneBounds(out bool unslottable);
                if (unslottable) return;
                uopRectangle bounds = slotbounds.Bounds;
                double wd = bounds.Width;
                int maxcols = (int)Math.Truncate(wd / minh) - 1;
                if (!mzUtils.IsEven(maxcols)) maxcols++;
                int cols = maxcols;


                double minpitchH = wd / 3;
                double pitchH = wd / (double)(maxcols + 1);
                while (pitchH < minh) { if (maxcols - 2 < 4) break; maxcols -= 2; pitchH = wd / (double)(maxcols + 1); }
                cols = maxcols;
                double pitchY = pitchH;
                int dev = (int)Math.Truncate(1.05 * target) - target;
                mdSlotGrid grid = new mdSlotGrid(section) { HPitch = pitchH,VPitch = pitchY} ;
                grid.Generate(section,true);
                uopVectors gpts = grid.GridPoints();
                while (gpts.Count > target + dev)
                {
                    if (cols - 2 < 4) break;
                    cols -= 2;
                    pitchH = wd / (double)(cols + 1);
                    if (pitchH < mdGlobals.MinSlotXPitch) pitchH = mdGlobals.MinSlotXPitch;
                    pitchY = pitchH;
                    if (pitchY < mdGlobals.MinSlotYPitch) pitchH = mdGlobals.MinSlotYPitch;
                    grid.HPitch = pitchH;
                    grid.VPitch = pitchY;
                    grid.Generate(section, true);
                    gpts = grid.GridPoints();
                    if (pitchH <= mdGlobals.MinSlotXPitch) break;
                }
                double ystep = 0.1 * pitchH;

                while (gpts.Count < target - dev)
                {
                    
                    pitchY -= ystep;
                    if (pitchY  < mdGlobals.MinSlotYPitch) pitchY = mdGlobals.MinSlotYPitch;
                    grid.HPitch = pitchH;
                    grid.VPitch = pitchY;
                    grid.Generate(section, true);
                    gpts = grid.GridPoints();
                    if (pitchY <= mdGlobals.MinSlotYPitch) break;

                }
            
                HPitch = grid.HPitch;
                VPitch = grid.VPitch;
            }
            catch { }
            finally
            {

                rIdealHPitch = HPitch;
                rIdealVPitch = VPitch;

            }




        }

    }


}



