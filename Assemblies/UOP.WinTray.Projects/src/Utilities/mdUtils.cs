using UOP.DXFGraphics;
using System;
using System.Collections.Generic;
using System.Linq;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Materials;
using UOP.WinTray.Projects.Parts;

using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Tables;
using static System.Math;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;
using Unity.Builder;
using UOP.WinTray.Projects.Interfaces;

namespace UOP.WinTray.Projects.Utilities
{
    public class mdUtils
    {
        public static uopProperties GetDisplayListProperties(mdProject aProj, uppPartTypes aPartType, int aRangeIndex = 0, uppUnitFamilies aUnits = uppUnitFamilies.Undefined) => GetDisplayListProperties(aProj, aPartType.DisplayListType(), aRangeIndex, aUnits);


        public static uopProperties GetDisplayListProperties(mdProject aProj,
                                    uppDisplayTableTypes aTableType,
                                    int aRangeIndex = 0,
                                    uppUnitFamilies aUnits = uppUnitFamilies.Undefined)
        {

            uopProperties _rVal = new uopProperties(aTableType.GetDescription());
            if (aProj == null || aTableType == uppDisplayTableTypes.Undefined) return _rVal;
            try
            {
                uopColumn col = aProj.Column;
                uopCustomer cust;
                uppUnitFamilies units = (aUnits == uppUnitFamilies.Undefined) ? aProj.DisplayUnits : aUnits;
                uppProjectTypes ptype = aProj.ProjectType;
                _rVal.UnitSystem = units;
                mdTrayRange range;
                uopProperty prop;
                mdTrayAssembly assy;
                mdDowncomer dc;
                mdDesignOptions dopt;
                uopProperties props;
                _rVal.DisplayUnits = units;
                string clr = "Black";
                switch (aTableType)
                {
                    //  PROJECT TABLE ==========================================================================
                    case uppDisplayTableTypes.ProjectProperties:
                        {
                            cust = aProj.Customer;
                            if (ptype == uppProjectTypes.MDSpout)
                            {
                                props = aProj.CurrentProperties();
                                uopProperties cprops = cust.CurrentProperties();

                                _rVal.Add("Project", aProj.Name, aPartType: uppPartTypes.Project);
                                _rVal.AddPartProp(aProj, "IDNumber", aDisplayName: "ID No.");
                                _rVal.AddPartProp(cust, "Item");
                                _rVal.AddPartProp(cust, "Service");
                                _rVal.AddPartProp(cust, "Name");
                                _rVal.AddPartProp(aProj, "Contractor");
                                _rVal.AddPartProp(aProj, "ProcessLicensor", aDisplayName: "Licensor");
                                _rVal.AddPartProp(cust, "Location");
                                _rVal.AddPartProp(aProj, "InstallationType");
                                _rVal.Add(new uopProperty("Tay Count", aProj.TrayCount.ToString()));
                                _rVal.Add("Tray Order", aProj.ReverseSort ? "Bottom to Top" : "Top to Bottom", aPartType: uppPartTypes.Project);
                                _rVal.AddPartProp(aProj, "Bolting");
                                _rVal.AddPartProp(col, "ManholeID", aColor: (col.ManholeID > 0) ? "Black" : "Red");
                                _rVal.AddPartProp(aProj, "DowncomerRoundToLimit", aDisplayName: "Rounding Type");
                                _rVal.AddPartProp(aProj, "MetricSpouting");
                                _rVal.AddPartProp(aProj, "MetricRings");
                                _rVal.AddPartProp(aProj, "SpacingMethod");

                                prop = aProj.GetProperty("ErrorLimit"); clr = (prop.ValueD > 0) ? "Black" : "Red";
                                if (prop.HasDefaultValue && prop.ValueD > 0 && !prop.IsDefault) clr = "Blue";
                                _rVal.Add(prop, aColor: clr);
                                prop = aProj.GetProperty("ConvergenceLimit"); clr = (prop.ValueD > 0) ? "Black" : "Red";
                                if (prop.HasDefaultValue && prop.ValueD > 0 && !prop.IsDefault) clr = "Blue";
                                _rVal.Add(prop, aColor: clr);


                                //_rVal.Add("Spout Area Error Limit", prop.UnitValueString(units, bIncludeUnitString: true), aColor: clr);
                                //prop = aProj.GetProperty("ConvergenceLimit"); clr = (prop.ValueD > 0) ? "Black" : "Red";
                                //if (prop.HasDefaultValue && prop.ValueD > 0 && !prop.IsDefault) clr = "Blue";
                                //_rVal.Add("Convergence Limit", prop.UnitValueString(units, bIncludeUnitString: true), aColor: clr);


                            }
                            else

                            {
                                _rVal.Add("Project", aProj.Name, aPartType: uppPartTypes.Project);
                                _rVal.AddPartProp(cust, "PO");
                                _rVal.AddPartProp(cust, "Item");
                                _rVal.AddPartProp(cust, "Service");
                                _rVal.AddPartProp(aProj, "SAPNumber");
                                _rVal.AddPartProp(cust, "Name");

                                _rVal.AddPartProp(aProj, "InstallationType");
                                _rVal.AddPartProp(col, "ColumnLetter");
                                _rVal.AddPartProp(cust, "Location");

                                _rVal.Add("Tray Order", aProj.ReverseSort ? "Bottom to Top" : "Top to Bottom", aPartType: uppPartTypes.Project);
                                _rVal.AddPartProp(aProj, "MetricSpouting");
                                _rVal.AddPartProp(aProj, "TrayVendor");
                                _rVal.AddPartProp(aProj, "Bolting");
                                _rVal.AddPartProp(col, "ManholeID", aColor: (col.ManholeID > 0) ? "Black" : "Red");
                                _rVal.AddPartProp(col, "RingThickness", aColor: (col.RingThickness > 0) ? "Black" : "Red");
                                _rVal.AddPartProp(aProj, "SparePercentage");
                                _rVal.AddPartProp(aProj, "ClipSparePercentage");
                                _rVal.AddPartProp(aProj, "CustomerDrawingUnits");
                                _rVal.AddPartProp(aProj, "ManufacturingDrawingUnits");

                                _rVal.Add("Functional DN", aProj.DrawingNumbers.ValueS("Functional"));
                                _rVal.Add("Attachment DN", aProj.DrawingNumbers.ValueS("Attachment"));
                                _rVal.Add("Installation DN", aProj.DrawingNumbers.ValueS("Installation"));
                                _rVal.Add("Manufacturing DN", aProj.DrawingNumbers.ValueS("Manufacturing"));

                            }
                            break;
                        }
                    //  RANGE TABLE ==========================================================================
                    case uppDisplayTableTypes.RangeProperties:
                        {
                            range = (aRangeIndex > 0 && aRangeIndex <= col.TrayRanges.Count) ? (mdTrayRange)col.TrayRanges.Item(aRangeIndex) : (mdTrayRange)col.TrayRanges.SelectedRange;
                            if (range != null)
                            {


                                assy = range.TrayAssembly;
                                _rVal.Add("RingStart", range.SpanName(true), aDisplayName: "Rings", aPartType: uppPartTypes.TrayRange);
                                uppMDDesigns designfam = range.DesignFamily;
                                _rVal.Add("TrayType",designfam.Description(), aPartType: uppPartTypes.TrayRange);
                                if (aProj.InstallationType == uppInstallationTypes.Revamp) _rVal.AddPartProp(range, "RevampStrategy", aDisplayName: "Revamp Type");
                                _rVal.Add("TrayCount", range.TrayCount, aPartType: uppPartTypes.TrayRange);
                                _rVal.AddPartProp(range, "RingSpacing", aColor: (range.RingSpacing > 0) ? "Black" : "Red");
                                double d1 = range.RingWidth;
                                prop = new uopProperty("RingWidth", d1, uppUnitTypes.SmallLength, range.PartType);
                                _rVal.Add(prop, aColor: (d1 > 0) ? "Black" : "Red");


                                if (ptype == uppProjectTypes.MDDraw)
                                {
                                    prop = _rVal.AddPartProp(range, "RingThk", aColor: (range.PropValD("RingThk") > 0) ? "Blue" : "Black", aOverrideValue: range.RingThickness);
                                    if (prop.Value <= 0) prop.DisplayColor = "Red";
                                }
                                _rVal.AddPartProp(range, "OverrideRingClearance", aDisplayName: "Ring Clrc.", aColor: (range.OverrideRingClearance <= 0) ? "Black" : "Blue", aOverrideValue: range.RingClearance, bSuppressNullProps: true);
                                _rVal.AddPartProp(range, "ShellID", aColor: (range.ShellID > 0) ? "Black" : "Red");
                                _rVal.AddPartProp(range, "RingID", aColor: (range.RingID > 0) ? "Black" : "Red");
                                _rVal.AddPartProp(range, "OverrideTrayDiameter", aDisplayName: "Tray OD", aColor: (range.OverrideTrayDiameter <= 0) ? "Black" : "Blue", aOverrideValue: range.TrayDiameter, bSuppressNullProps: true);
                                _rVal.AddPartProp(range, "ManholeID", aDisplayName: "Manhole ID", aColor: (range.ManholeID == col.ManholeID) ? "Black" : "Blue", aOverrideValue: range.ManholeID);
                                d1 = range.RingClearance;
                                prop = new uopProperty("Clearance", d1, uppUnitTypes.SmallLength, range.PartType);
                                _rVal.Add(prop, aColor: (d1 != uopUtils.BoundingClearance(range.ShellID) ? "Blue" : "Black"));
                            }
                            break;
                        }
                    //  DOWNCOMER TABLE ==========================================================================
                    case uppDisplayTableTypes.DowncomerProperties:
                        {
                            range = (aRangeIndex > 0 && aRangeIndex <= col.TrayRanges.Count) ? (mdTrayRange)col.TrayRanges.Item(aRangeIndex) : (mdTrayRange)col.TrayRanges.SelectedRange;
                            if (range != null)
                            {

                                assy = range.TrayAssembly;
                                colMDDowncomers dcs = assy.Downcomers;
                                dc = assy.Downcomer();  //global downcomer
                                _rVal.AddPartProp(dc, "Count");
                                double d1 = dcs.Spacing;
                                double d2 = dcs.OptimumSpacing;
                                double d3 = dcs.OverrideSpacing;
                                if (d3 <= 0) d3 = d1;
                                prop = new uopProperty("Spacing", d1, uppUnitTypes.SmallLength, uppPartTypes.Downcomer) { DisplayName = "DC Spacing" };

                                _rVal.Add(prop, aColor: (Math.Round(d1 - d2, 1) != 0) ? "Blue" : "Black");
                                _rVal.AddPartProp(dc, "How");
                                _rVal.AddPartProp(dc, "Width", aDisplayName: "Inside Width");
                                _rVal.AddPartProp(dc, "InsideHeight");
                                _rVal.Add("Thickness", dc.Thickness, aUnitType: uppUnitTypes.SmallLength, aPartType: uppPartTypes.Downcomer);
                                if (ptype == uppProjectTypes.MDSpout)
                                {
                                    _rVal.AddPartProp(dc, "Asp", aDisplayName: "Ideal Spout Area");
                                }
                                else
                                {
                                    _rVal.Add("Total DC Area", assy.Downcomers.TotalBottomArea / Math.Pow(12, 2), aUnitType: uppUnitTypes.BigArea);
                                }
                                    
                                
                            }
                            break;
                        }
                    //  DECK TABLE ==========================================================================
                    case uppDisplayTableTypes.DeckProperties:
                        {
                            range = (aRangeIndex > 0 && aRangeIndex <= col.TrayRanges.Count) ? (mdTrayRange)col.TrayRanges.Item(aRangeIndex) : (mdTrayRange)col.TrayRanges.SelectedRange;
                            if (range != null)
                            {
                                assy = range.TrayAssembly;
                                mdDeck deck = assy.Deck;
                                dopt = assy.DesignOptions;

                                _rVal.AddPartProp(deck, "Fp", "Percent Open (Fp)", aColor: (deck.Fp <= 0) ? "Red" : "Black");
                                _rVal.AddPartProp(deck, "PerfDiameter", aColor: (deck.PerforationDiameter <= 0) ? "Red" : "Black");
                                _rVal.AddPartProp(deck, "PunchDirection");
                                _rVal.Add(new uopProperty("Thickness", deck.Thickness, uppUnitTypes.SmallLength));

                                if (ptype == uppProjectTypes.MDSpout)
                                {
                                    clr = "Black";

                                    _rVal.AddPartProp(dopt, "HasBubblePromoters");
                                    prop = dopt.GetProperty("HasTiledDecks");
                                    if (assy.FunctionalPanelWidth > range.ManholeID - 0.5)
                                        clr = !prop.ValueB ? "Red" : "Black";
                                    else
                                        clr = prop.ValueB ? "Red" : "Black";

                                    _rVal.AddPartProp(dopt, "HasTiledDecks", aColor: clr);
                                    _rVal.AddPartProp(deck, "ManwayCount");
                                    _rVal.AddPartProp(dopt, "FlowDeviceType", "Devices");
                                    if (dopt.FlowDeviceType != uppFlowDevices.None)
                                    {
                                        prop = _rVal.AddPartProp(dopt, "FEDorAPPHeight", "AP Pan Clearance", aColor: (dopt.FEDorAPPHeight != mdUtils.DefaultAPPanClearance(range)) ? "Blue" : "Black");
                                        if (prop.ValueD <= 0) prop.DisplayColor = "Red";
                                        if (dopt.FlowDeviceType == uppFlowDevices.FED) prop.DisplayName = "FED Clearance";
                                    }

                                    if (range.DesignFamily.IsEcmdDesignFamily())
                                    {
                                        dynamic orride = null;

                                        if (dopt.PropValD("CDP") <= 0) orride = mdUtils.DefaultCDP(range);
                                            prop = _rVal.AddPartProp(dopt, "CDP", aDisplayName: "Deflector Ht. (CDP)",aOverrideValue: orride,  aColor: (dopt.CDP != mdUtils.DefaultCDP(range)) ? "Blue" : "Black");
                                        if (prop.ValueD <= 0) prop.DisplayColor = "Red";
                                        _rVal.AddPartProp(deck, "SlottingPercentage", aColor: (deck.SlottingPercentage <= 0) ? "Red" : "Black");
                                        _rVal.AddPartProp(deck, "SlotType", "ECMD Slot Type");
                                    }

                                }
                                else
                                {
                                    if (range.DesignFamily.IsEcmdDesignFamily())
                                    {
                                        mdSlotZones zones = assy.SlotZones;
                                        clr = !zones.Validate(assy, out string txt, out int req, out int act, out  _, out _) ? "Red" : "Black";
                                        _rVal.AddPartProp(deck, "SlotType", "ECMD Slot Type");
                                        _rVal.AddPartProp(deck, "SlottingPercentage", aColor: clr);


                                        _rVal.Add("Required Slot Count", req.ToString("#,0"), aColor: clr);
                                        _rVal.Add("Actual Slot Count", act.ToString("#,0"), aColor: clr);

                                    }
                                }

                            }
                            break;
                        }
                    //  DESIGN OPTIONS TABLE ==========================================================================
                    case uppDisplayTableTypes.DesignOptions:
                        {
                            range = (aRangeIndex > 0 && aRangeIndex <= col.TrayRanges.Count) ? (mdTrayRange)col.TrayRanges.Item(aRangeIndex) : (mdTrayRange)col.TrayRanges.SelectedRange;
                            if (range != null)
                            {

                                assy = range.TrayAssembly;
                                dc = assy.Downcomer();
                                dopt = assy.DesignOptions;
                                if (ptype == uppProjectTypes.MDSpout)
                                {

                                    double serr = assy.ErrorPercentage;
                                    clr = (Math.Abs(serr) > aProj.ErrorLimit) ? "Red" : "Green";
                                    prop = dc.GetProperty("Asp");
                                    prop.Name = "Actual Spout Area";
                                    prop.SetValue(assy.TotalSpoutArea);
                                    _rVal.Add(prop, aColor: clr);

                                    _rVal.Add("Spout Area Error", assy.ErrorPercentage, aUnitType: uppUnitTypes.Percentage, aColor: clr);
                                    _rVal.Add("Total Weir Length", assy.TotalWeirLength / 12, aUnitType: uppUnitTypes.BigLength);
                                    _rVal.Add("Total DC Area", assy.Downcomers.TotalBottomArea / Math.Pow(12, 2), aUnitType: uppUnitTypes.BigArea);
                                    _rVal.Add("Column Area", Math.PI * Math.Pow(range.ShellID / 2 / 12, 2), aUnitType: uppUnitTypes.BigArea);
                                    _rVal.Add("Max V/L Error", assy.DeckPanels.MaxVLError(assy), aUnitType: uppUnitTypes.Percentage);
                                    _rVal.Add("Free Bubbling Area", assy.TotalFreeBubblingArea / 144, aUnitType: uppUnitTypes.BigArea);
                                    _rVal.Add("Active Area", assy.FunctionalActiveArea / 144, aUnitType: uppUnitTypes.BigArea);
                                    _rVal.Add("Panel Width", assy.FunctionalPanelWidth, aUnitType: uppUnitTypes.SmallLength);

                                    double lim1 = aProj.ConvergenceLimit;
                                    double lim2 = Math.Abs(assy.MaximumDistributionDeviation);
                                    _rVal.Add("SA Distrib. Dev.", $"{lim2:0.00E+00}" , aColor: lim2 > lim1 ? "Red" : "Green");
                                }

                                else
                                {
                                    _rVal.Add("Splice Style", assy.DesignOptions.SpliceStyle.GetDescription());
                                    _rVal.Add("Devices", dopt.Devices);
                                    if (dopt.FlowDeviceType != uppFlowDevices.None)
                                    {
                                        prop = _rVal.AddPartProp(dopt, "FEDorAPPHeight", "AP Pan Clearance", aColor: (dopt.FEDorAPPHeight != mdUtils.DefaultAPPanClearance(range)) ? "Blue" : "Black");
                                        if (prop.ValueD <= 0) prop.DisplayColor = "Red";

                                    }
                                    if (range.DesignFamily.IsEcmdDesignFamily())
                                    {
                                        prop = _rVal.AddPartProp(dopt, "CDP", aDisplayName: "Deflector Ht. (CDP)", aColor: (dopt.CDP <= 0) ? "Red" : "Black");
                                        if (prop.ValueD <= 0) prop.DisplayColor = "Red";
                                        prop = _rVal.AddPartProp(dopt, "BaffleMountPercentage", aDisplayName: "Deflector Mount %", aColor: (dopt.BaffleMountPercentage != 50) ? "Blue" : "Black");
                                        if (prop.ValueD <= 0) prop.DisplayColor = "Red";
                                    }
                                    _rVal.AddPartProp(dopt, "HasBubblePromoters");
                                    _rVal.AddPartProp(dopt, "WeldedStiffeners", aColor: (!dopt.WeldedStiffeners) ? "Blue" : "Black");
                                    _rVal.AddPartProp(dopt, "MaxRingClipSpacing", aDisplayName: "RC Spacing (Max)", aColor: dopt.MaxRingClipSpacing != 9 ? "Blue" : "Black");
                                    _rVal.AddPartProp(dopt, "MoonRingClipSpacing", aDisplayName: "RC Spacing (Moon)", aColor: dopt.MoonRingClipSpacing != 9 ? "Blue" : "Black");
                                    _rVal.AddPartProp(dopt, "JoggleBoltSpacing", aDisplayName: "Splice Angle Bolt Space", aColor: dopt.JoggleBoltSpacing != 6 ? "Blue" : "Black");
                                     _rVal.AddPartProp(dopt, "JoggleAngle", aDisplayName: "Joggle Angle Height", aColor: dopt.JoggleAngle < 1 ? "Red" : "Black");
                                     _rVal.AddPartProp(dopt, "SpliceAngle", aDisplayName: "Splice Angle Height", aColor: dopt.SpliceAngle < 2 ? "Red" : "Black");

                                    if (dopt.BottomDCHeight > 0)
                                        _rVal.AddPartProp(dopt, "BottomDCHeight", aDisplayName: "Bottom DC Height", aColor: dopt.BottomDCHeight < assy.Downcomer().Height + 0.5 ? "Red" : "Blue");

                                }
                            }
                            break;
                        }
                    //  MATERIAL TABLE ==========================================================================
                    case uppDisplayTableTypes.Materials:
                        {
                            range = (aRangeIndex > 0 && aRangeIndex <= col.TrayRanges.Count) ? (mdTrayRange)col.TrayRanges.Item(aRangeIndex) : (mdTrayRange)col.TrayRanges.SelectedRange;
                            if (range != null)
                            {
                                assy = range.TrayAssembly;
                                string threads = (aProj.Bolting == uppUnitFamilies.English) ? " U" : " M";
                                uopHardwareMaterial hmat = assy.HardwareMaterial;
                                _rVal.Add("Hardware", hmat.FriendlyName(false) + threads);
                                _rVal.Add("Deck", assy.DeckMaterial.FriendlyName(false));
                                _rVal.Add("Downcomer", assy.DowncomerMaterial.FriendlyName(false));

                            }
                            break;
                        }
                    //  Startup TABLE ==========================================================================
                    case uppDisplayTableTypes.StartupSpouts:
                        {
                            range = (aRangeIndex > 0 && aRangeIndex <= col.TrayRanges.Count) ? (mdTrayRange)col.TrayRanges.Item(aRangeIndex) : (mdTrayRange)col.TrayRanges.SelectedRange;
                            if (range != null)
                            {
                                assy = range.TrayAssembly;
                                mdStartupSpouts spouts = assy.StartupSpouts;
                                double area = spouts.TotalArea;
                                double tarea = spouts.TargetArea;
                                double dev = (area <= 0) ? -100 : (area - tarea) / tarea * 100;
                                int cnt = spouts.TotalCount;
                                uopUnit lunits = uopUnits.GetUnit(uppUnitTypes.SmallLength);
                                uopUnit aunits = uopUnits.GetUnit(uppUnitTypes.SmallArea);
                                uppUnitFamilies ufam = assy.MetricSpouting ? uppUnitFamilies.Metric : uppUnitFamilies.English;
                                clr = (cnt <= 0 || Math.Abs(dev) > mdStartUps.DeviationLimit) ? "Red" : "Green";
                                _rVal.Add("Configuration", assy.StartupConfiguration.Description(), aColor: "Black");
                                _rVal.Add("Count", cnt, aColor: clr);
                                _rVal.Add("Spout Height",  lunits.UnitValueString( spouts.Height,ufam));
                                _rVal.Add("Spout Length", lunits.UnitValueString(spouts.Length,ufam));
                       
                                //if(ufam == uppUnitFamilies.Metric) txt += $"[{aunits.UnitValueString(area, uppUnitFamilies.English)}]";
                                _rVal.Add("Total Area", area, uppUnitTypes.SmallArea, aColor: clr);
                                _rVal.Add("Ideal Area", tarea, uppUnitTypes.SmallArea, aColor: clr);
                                //_rVal.Add("Total Area",aunits.UnitValueString(area, ufam) , aColor: clr);
                                //_rVal.Add("Ideal Area", aunits.UnitValueString(tarea, ufam), aColor: clr);
                                _rVal.Add("Deviation", dev, aUnitType: uppUnitTypes.BigPercentage, aColor: clr);

                            }
                            break;
                        }
                    //  BEAM TABLE ==========================================================================
                    case uppDisplayTableTypes.BeamProperties:
                        range = (aRangeIndex > 0 && aRangeIndex <= col.TrayRanges.Count) ? (mdTrayRange)col.TrayRanges.Item(aRangeIndex) : (mdTrayRange)col.TrayRanges.SelectedRange;
                        if (range != null)
                        {
                            assy = range.TrayAssembly;
                            if (assy.DesignFamily.IsBeamDesignFamily() )
                            {
                                mdBeam beam = assy.Beam;
                                if (beam.OccuranceFactor > 0)
                                {
                                    _rVal.Add("Offset", beam.Offset, aUnitType: uppUnitTypes.SmallLength);
                                }
                                _rVal.AddPartProp(beam, "Width", "Beam Width");
                                _rVal.AddPartProp(beam, "Height", "Beam Height");
                                _rVal.Add("Length", beam.Length ,aUnitType: uppUnitTypes.SmallLength);
                                _rVal.AddPartProp(beam, "WebThickness", "Web Thickness");
                                _rVal.AddPartProp(beam, "FlangeThickness", "Flange Thickness");
                                _rVal.Add("Web Opening Size", beam.WebOpeningSize, aUnitType: uppUnitTypes.SmallLength);
                                _rVal.Add("Web Opening Count", beam.WebOpeningCount);
                            }
                        }
                        break;
                    default:
                        break;

                }


            }
            catch { }

            return _rVal;
        }

        public static uopTable GetDisplayTableProperties(mdProject aProj, uppPartTypes aPartType, int aRangeIndex = 0, uppUnitFamilies aUnits = uppUnitFamilies.Undefined)
         => GetDisplayTableProperties(aProj, aPartType.DisplayTableType(), aRangeIndex, aUnits);

        public static uopTable GetDisplayTableProperties(mdProject aProj, uppDisplayTableTypes aTableType, int aRangeIndex = 0, uppUnitFamilies aUnits = uppUnitFamilies.Undefined)
        {
            uopTable _rVal = new uopTable(aTableType);
            if (aProj == null || aTableType == uppDisplayTableTypes.Undefined) return _rVal;
            uopColumn col = aProj.Column;
            _rVal.ProjectFamily = aProj.ProjectFamily;
            _rVal.ProjectType = aProj.ProjectType;
            uppUnitFamilies units = (aUnits == uppUnitFamilies.Undefined) ? aProj.DisplayUnits : aUnits;
            uppProjectTypes ptype = aProj.ProjectType;
            uopProperties row = null;
            mdTrayRange range = aProj.SelectedRange;
            mdTrayAssembly assy = range?.TrayAssembly;
            _rVal.DisplayUnits = units;
            bool stddesgn = assy == null ? true : assy.DesignFamily.IsStandardDesignFamily();
            string clr = "Black";
            string txt = string.Empty;
            int dccnt = assy == null ? 0 : assy.Downcomer().Count;
            double dval =0;
            switch (aTableType)
            {

                //  SPOUTGROUPS TABLE ==========================================================================
                case uppDisplayTableTypes.SpoutGroupsProperties:
                    {
                        _rVal.PartType = uppPartTypes.SpoutGroups;
                        if (assy == null) return _rVal;
                        List<mdSpoutGroup> sGS = assy.SpoutGroups.GetByVirtual(aVirtualValue: false);
                        double lim = assy.ErrorLimit;
                        for (int i = 1; i <= sGS.Count; i++)
                        {
                            mdSpoutGroup SG = sGS[i - 1];
                            mdConstraint constraints = SG.Constraints(assy);
                            row = new uopProperties();
                            mdSpoutArea sa = SG.SpoutArea;
                            mdSpoutGrid grid = SG.Grid;
                            clr = (Math.Abs(SG.ErrorPercentage) >= lim) ? "Red" : "Black";

                            row.Add("No.", SG.Handle, aColor: clr);
                            row.Add("Spout Count", SG.SpoutCount(assy), aColor: (constraints.SpoutCount >= 0) ? "Blue" : "Black");
                            row.Add("Pat. Type", SG.PatternName, aColor: (constraints.PatternType != uppSpoutPatterns.Undefined) ? "Blue" : "Black");
                            row.Add("Spout Length", SG.Spout.Length, uppUnitTypes.SmallLength, aColor: (constraints.SpoutLength != 0 || constraints.SpoutDiameter != 0) ? "Blue" : "Black");
                            row.Add("Spouts Per Row", SG.SPRString, aColor: "Black");
                            row.Add("Vertical Pitch", SG.VerticalPitch, uppUnitTypes.SmallLength, aColor: (constraints.VerticalPitch != 0) ? "Blue" : "Black");
                            row.Add("Pat. Length", SG.PatternLength, uppUnitTypes.SmallLength, aColor: (constraints.VerticalPitch != 0) ? "Blue" : "Black");
                            
                            if (grid.MaxMargin.HasValue)
                            {
                                string clr2 =  constraints.Margin != 0 ? "Blue" : "Black";
                                if (grid.ViolatesSafeMargin)
                                    clr2 = "Red";

                                row.Add("Actual Margin", SG.ActualMargin, uppUnitTypes.SmallLength, aColor: clr2);
                            }
                            else
                            {
                                row.Add("Actual Margin", string.Empty, aColor: "Black");
                            }


                            // row.Add(new uopProperty("Ideal Area", SG.TargetArea(assy), uppUnitTypes.SmallArea) { ForeColor = sa.Color.Item2 });
                            string clr3 = constraints.HasAreaConstraints ? "Blue" : "Black";
                                                    

                            uopProperty areaprop = row.Add("Ideal Area", SG.TargetArea(assy), uppUnitTypes.SmallArea, aColor:clr3);
                            if (constraints.HasAreaConstraints && constraints.TreatAsGroup && constraints.AreaGroupIndex <= mdSpoutAreaMatrix.MaxGroupings)
                            {
                                (dxxColors, System.Drawing.Color) groupclr = mdSpoutAreaMatrix.GetGroupColor(constraints.AreaGroupIndex);
                                areaprop.ForeColor = groupclr.Item2;
                            }
                            row.Add("Actual Area", SG.ActualArea, uppUnitTypes.SmallArea, aColor: "Black");
                            row.Add("Err", SG.ErrorPercentage, uppUnitTypes.Percentage, aColor: clr);
                            row.Add("Index", i.ToString());
                            row.Add("Handle", SG.Handle);

                            _rVal.AddRow(row);
                        }
                        _rVal.SelectedRow = assy.SpoutGroups.SelectedGroupIndex;
                        break;
                    }
                //  DECK PANELS TABLE ==========================================================================
                case uppDisplayTableTypes.DeckPanelsProperties:
                    {
                        if (assy == null) return _rVal;


                        if (ptype == uppProjectTypes.MDSpout)
                        {

                            _rVal.PartType = uppPartTypes.DeckPanels;
                            mdFreeBubblingAreas fbas = assy.FreeBubblingAreas;
                            uopFreeBubblingPanels fbps = assy.FreeBubblingPanels;
                            List<mdDeckPanel> dps = assy.DeckPanels.GetByVirtual(aVirtualValue: false);
                            //foreach (uopFreeBubblingArea fba in fbas)
                            //{
                            //    mdDeckPanel dp = dps.Find((x) => x.Index == fba.PanelIndex);
                            //    row = new uopProperties();
                            //    double spouterr = fba.ErrorPercentage(assy);
                            //    double VLerr = dp.VLError(assy);
                            //    double fba = fba.Area;
                            //    double weir = fba.WeirLength;

                            //    clr = Math.Abs(spouterr) > assy.ErrorLimit ? "Red" : "Black";

                            //    txt = $"{dp.Index}";
                            //    if (stddesgn && dp.OccuranceFactor == 2)
                            //    {
                            //        txt += $" & {uopUtils.OpposingIndex(dp.Index, dccnt + 1)}";
                            //    }

                            //    row.Add("No", txt);
                            //    row.Add("FBA", fba.Area, uppUnitTypes.SmallArea);
                            //    row.Add("FBA/WL", fba.WeirLengthRatio, uppUnitTypes.Percentage);
                            //    row.Add("V/L Err", fba.VLError(assy), uppUnitTypes.Percentage, aColor: (Math.Abs(VLerr) > 2.5) ? "Red" : "Black");
                            //    row.Add("Ideal Area", dp.IdealSpoutArea(assy), uppUnitTypes.SmallArea);
                            //    row.Add("Actual Area", dp.TotalSpoutArea(assy), uppUnitTypes.SmallArea);
                            //    row.Add("Err", spouterr, uppUnitTypes.Percentage, aColor: clr);
                            //    _rVal.AddRow(row);
                            //}


                            int iNp = dps.Count;
                            if (!assy.IsSymmetric && stddesgn) iNp -= 1;
                            
                            for (int i = 1; i <= iNp; i++)
                            {
                                List < uopFreeBubblingArea > pfbas  = fbas.FindAll((x) => x.PanelIndex == i);
                                
                                uopFreeBubblingArea pfba = fbas[i - 1];
                                var dp = dps[i - 1];
                                row = new uopProperties();
                                double spouterr = dp.ErrorPercentage(assy);
                                double VLerr = dp.VLError(assy);
                                double fba = pfba.Area;
                                double weir = pfba.WeirLength;

                                clr = Math.Abs(spouterr) > assy.ErrorLimit ? "Red" : "Black";

                                txt = $"{dp.Index}";
                                int opindex = uopUtils.OpposingIndex(dp.Index, dccnt + 1);
                                if (opindex != dp.Index)
                                      txt += $" & {opindex}";
                                row.Add("No", txt);
                                row.Add("FBA", pfba.Area, uppUnitTypes.SmallArea);
                                row.Add("FBA/WL", pfba.WeirLengthRatio, uppUnitTypes.Percentage);
                                row.Add("V/L Err", pfba.VLError(assy), uppUnitTypes.Percentage, aColor: (Math.Abs(VLerr) > 2.5) ? "Red" : "Black");
                                row.Add("Ideal Area", dp.IdealSpoutArea(assy), uppUnitTypes.SmallArea);
                                row.Add("Actual Area", dp.TotalSpoutArea(assy,true), uppUnitTypes.SmallArea);
                                row.Add("Err", spouterr, uppUnitTypes.Percentage, aColor: clr);
                                _rVal.AddRow(row);
                            }
                            _rVal.SelectedRow = dps.FindIndex(x => x.Selected == true) + 2;
                        }
                        else
                        {
                            _rVal.PartType = uppPartTypes.DeckSections;
                            mdDeckSections dss = assy.DeckSections;
                            for (int i = 1; i <= dss.Count; i++)
                            {
                                var ds = dss.Item(i);
                                row = new uopProperties
                                {
                                    { "PN", ds.PartNumber },
                                    { "Width", ds.Width, uppUnitTypes.SmallLength },
                                    { "Height", ds.Height, uppUnitTypes.SmallLength },
                                    { "Is Manway", ds.IsManway }
                                };

                                _rVal.AddRow(row);
                            }
                            _rVal.SelectedRow = dss.SelectedIndex;
                        }



                        break;
                    }
                //  DOWNCOMERS TABLE ==========================================================================
                case uppDisplayTableTypes.DowncomersProperties:
                    _rVal.PartType = uppPartTypes.Downcomers;
                    if (assy == null) return _rVal;

                    List<mdDowncomerBox> boxes = assy.Downcomers.Boxes();
                    double totalweir = 0;
                    double dval2 = 0;
                    double totalideal = 0;
                    double totalactual = 0;
                    double totalerr = 0;
                    int occr = 0;
                    for (int i = 1; i <= boxes.Count; i++)
                    {
                        mdDowncomerBox box = boxes[i - 1];
                        occr = box.OccuranceFactor;
                        mdDowncomer dc = box.Downcomer;
                        row = new uopProperties();

                        if (ptype == uppProjectTypes.MDSpout)
                        {
                            double spouterr = 0;
                            //clr = Math.Abs(spouterr) > assy.ErrorLimit ? "Red" : "Black";
                            txt = $"{dc.Index}";
                            
                            if (box.OccuranceFactor == 2)
                            {
                                if(stddesgn ||(!stddesgn && box.X !=0))
                                txt += $" & {uopUtils.OpposingIndex(dc.Index, dccnt)}";
                            }

                            row.Add("No", txt);
                            dval = box.WeirLength();
                            totalweir += dval * occr;
                            row.Add("Weir Length", dval, uppUnitTypes.SmallLength);
                            dval = box.IdealSpoutArea(assy);
                            totalideal += dval * occr;
                            row.Add("Ideal Area",dval, uppUnitTypes.SmallArea);
                            dval2 = box.TotalSpoutArea(assy);
                            totalactual += dval2 * occr;
                           
                            row.Add("Actual Area", dval2, uppUnitTypes.SmallArea);
                             spouterr =   uopUtils.TabulateAreaDeviation(dval, dval2, out _);
                            totalerr += spouterr;
                            clr = Math.Abs(spouterr) > assy.ErrorLimit ? "Red" : "Black";
                            row.Add("Err", spouterr, uppUnitTypes.Percentage, aColor: clr);
                        }
                        else
                        {
                            row.Add("PN", box.PartNumber);
                            row.Add("Box Lengh",  box.BoxLength(box.X > 0), uppUnitTypes.SmallLength);
                            row.Add("Fld. Weir", box.FoldOverHeight, uppUnitTypes.SmallLength);
                            row.Add("Supl. Defl.", box.SupplementalDeflectorHeight, uppUnitTypes.SmallLength);

                            row.Add("Gussets", dc.GussetedEndplates ? "Yes" : "");

                        }
                      _rVal.AddRow(row);
                    }
                    if (ptype == uppProjectTypes.MDSpout && boxes.Count > 0)
                    {
                        double spouterr = uopUtils.TabulateAreaDeviation(totalideal,totalactual, out _);
                        row = new uopProperties(row);
                        row.SetValue(1,"Totals");
                        row.SetValue(2, totalweir);
                        row.SetValue(3, totalideal);
                        row.SetValue(4, totalactual);
                        row.SetValue(5, spouterr);
                        row.Last().DisplayColor = Math.Abs(spouterr) > assy.ErrorLimit ? "Red" : "Black";
                        
                        _rVal.AddRow(row);
                    }

                    break;

                default:
                    break;
            }

            return _rVal;
        }

        public static colUOPParts FingerClips(mdTrayAssembly aAssy, List<mdStiffener> aStiffeners = null, int aDCIndex = 0, int aPanelIndex = 0, uppSides aSide = uppSides.Undefined, bool bBothSides = false, bool bTrayWide = false)
        {
            colUOPParts _rVal = new colUOPParts();
            if (aAssy == null) return _rVal;

            List<mdDowncomer> dcs = aAssy.Downcomers.GetByVirtual(aVirtualValue: false);

            uopVectors fCPts = mdUtils.FingerClipPoints(aAssy, dcs, aAssyStiffeners: aStiffeners, aDCIndex: aDCIndex, aPanelIndex, aSide: aSide, bUnsuppressedOnly: false, bTrayWide: bTrayWide);
            if (fCPts.Count <= 0) return _rVal;

            mdDowncomer dc;
            
            mdFingerClip aFC = aAssy.FingerClip;

            for (int i = 1; i <= fCPts.Count; i++)
            {
                uopVector u1 = fCPts.Item(i);
                if (u1.Suppressed)
                    continue;
                dc = u1.PartIndex > 0 && u1.PartIndex <= dcs.Count ? dcs[u1.PartIndex - 1] : null;

                aFC.Suppressed = u1.Suppressed || u1.Mark;

                aFC.X = u1.X;
                aFC.Y = u1.Y;
                aFC.Z = u1.Value;
                aFC.OccuranceFactor = dc == null ? u1.Col : dc.OccuranceFactor;
                aFC.PanelIndex = u1.Row;

                aFC.Side = u1.Tag == "LEFT" ? uppSides.Left : uppSides.Right;
                aFC.Rotation = aFC.Side == uppSides.Left ? 180 : 0;

                aFC.DowncomerIndex = (int)u1.DownSet;
                if (!aFC.Suppressed)
                {
                    aFC.SubPart(dc);
                    _rVal.Add(aFC, bAddClone: true);

                }
                if (bBothSides)
                {
                    if (aFC.OccuranceFactor > 1)
                    {
                        mdFingerClip bFC = aFC.Clone();
                        bFC.X = -u1.X;
                        bFC.Y = -u1.Y;

                        bFC.Side = aFC.Side == uppSides.Right ? uppSides.Left : uppSides.Right;
                        bFC.Rotation = bFC.Side == uppSides.Left ? 180 : 0;
                        if (u1.Mark)
                            bFC.Suppressed = false;
                        if (!bFC.Suppressed)
                        {
                            bFC.SubPart(dc);
                            _rVal.Add(bFC, bAddClone: false);
                        }
                    }
                }
            }
            return _rVal;
        }

        public static colDXFVectors FingerClipCenters(mdTrayAssembly aAssy, int aDCIndex = 0, int aPanelIndex = 0, uppSides aSide = uppSides.Undefined, bool? bSuppressedValue = false, bool bGetEndAnglePts = false)
        {
            colDXFVectors _rVal = colDXFVectors.Zero;

            if (aAssy == null) return _rVal;

            UVECTORS fcpts = FingerClipPoints(aAssy.Downcomers, aAssy, null, aDCIndex, aPanelIndex, aSide, false, bGetEndAnglePts: bGetEndAnglePts);
            for (int i = 1; i <= fcpts.Count; i++)
            {
                UVECTOR u1 = fcpts.Item(i);
                u1.Suppressed = u1.Suppressed || u1.Mark;
                if (bSuppressedValue.HasValue)
                {
                    if (u1.Suppressed == bSuppressedValue.Value)
                    {
                        _rVal.Add(u1.ToDXFVector());
                    }
                }
                else
                {
                    _rVal.Add(u1.ToDXFVector());
                }
            }

            return _rVal;
        }



        internal static uopVectors FingerClipPoints(mdTrayAssembly aAssy, List<mdDowncomer> aDowncomers,  List<mdStiffener> aAssyStiffeners , int aDCIndex = 0, int aPanelIndex = 0, uppSides aSide = uppSides.Undefined, bool bUnsuppressedOnly = true,  bool bGetEndAnglePts = false, bool bTrayWide = true, bool bReturnCenters = false)
        {
            uopVectors _rVal = uopVectors.Zero;

            if (aAssy == null || aAssy.ProjectType != uppProjectTypes.MDDraw) return _rVal;
            aDowncomers ??= aAssy.Downcomers.GetByVirtual(aVirtualValue: false);
            aAssyStiffeners ??= mdPartGenerator.Stiffeners_ASSY(aAssy, false);
            uppMDDesigns design = aAssy.DesignFamily;
            double wd = aAssy.FingerClip.Width / 2; // + aAssy.Downcomer().Thickness/2;
            foreach (mdDowncomer dc in aDowncomers)
            {

                if (aDCIndex <= 0 || dc.Index == aDCIndex)
                {
                    List<mdStiffener> dcstfs = aAssyStiffeners.FindAll((x) => x.DowncomerIndex == dc.Index);
                    foreach (var box in dc.Boxes.Where(b => !b.IsVirtual))
                    {
                        UVECTORS fcpoints = box.FingerClipPts( aAssy, dcstfs, aSide, bUnsuppressedOnly, null, bGetEndAnglePts);
                        for(int i = 1; i <= fcpoints.Count; i++)
                        {
                            UVECTOR u1 = fcpoints.Item(i);
                            UVECTOR u2 = new UVECTOR(u1);
                            if (bReturnCenters)
                            {
                                u2.X += (u2.Tag == "LEFT" ? -1 : 1) * wd ;
                            }

                            _rVal.Add(u2);
                            if (bTrayWide)
                            {
                                if (design.IsBeamDesignFamily())
                                {
                                    //if(Math.Round(dc.X,1) != 0)
                                    //{
                                    u2 = new UVECTOR(u1) * -1;
                                    u2.Value = u2.Value == 180 ? 0 : 180;
                                    u2.Tag = u2.Tag == "LEFT" ? "RIGHT" : "LEFT";
                                    if (bReturnCenters)
                                    {
                                        u2.X += (u2.Tag == "LEFT" ? -1 : 1) * wd ;
                                    }
                                    _rVal.Add(u2);

                                    // }

                                }
                                else if (design.IsStandardDesignFamily())
                                {
                                    if(dc.OccuranceFactor >1)
                                    {
                                        u2 = new UVECTOR(u1) * -1;
                                        u2.Value = u2.Value == 180 ? 0 : 180;
                                        u2.Tag = u2.Tag == "LEFT" ? "RIGHT" : "LEFT";
                                        if (bReturnCenters)
                                        {
                                            u2.X += ( u2.Tag == "LEFT" ? -1 : 1) * wd  ;
                                        }
                                        _rVal.Add(u2);

                                    }
                                }


                            }
                        }
                        //_rVal.Append(fcpoints);
                    }
                }
            }

            return _rVal;
        }

        internal static UVECTORS FingerClipPoints(colMDDowncomers aDowncomers, mdTrayAssembly aAssy, List<mdStiffener> aAssyStiffeners, int aDCIndex = 0, int aPanelIndex = 0, uppSides aSide = uppSides.Undefined, bool bUnsuppressedOnly = true, colDXFVectors aCollector = null, bool bGetEndAnglePts = false)
        {
            UVECTORS _rVal = UVECTORS.Zero;
            aAssy ??= aDowncomers?.GetMDTrayAssembly(aAssy);
            if (aAssy == null) return _rVal;
            aDowncomers ??= aAssy.Downcomers;

            aAssyStiffeners ??= mdPartGenerator.Stiffeners_ASSY(aAssy, false);

            for (int i = 1; i <= aDowncomers.Count; i++)
            {
                if (aDCIndex <= 0 || aDCIndex == i)
                {

                    mdDowncomer aDC = aDowncomers.Item(i);
                    if (aDC.IsVirtual) continue;

                    List<mdStiffener> dcStfs = aAssyStiffeners.FindAll(x => x.DowncomerIndex == i);

                    foreach (var box in aDC.Boxes.Where(b => !b.IsVirtual))
                    {
                        UVECTORS fcpoints = box.FingerClipPts(aAssy, dcStfs, aSide, bUnsuppressedOnly, aCollector, bGetEndAnglePts);
                        //if (aDC.OccuranceFactor > 1) fcpoints.Append(fcpoints.Rotated(UVECTOR.Zero,180));
                        _rVal.Append(fcpoints);
                    }
                }
            }

            return _rVal;
        }


        /// <summary>
        /// the ideal height for a manway panel based on the current splice style
        /// </summary>
        /// <param name="aSpliceStyle"></param>
        /// <returns></returns>
        public static double IdealManwayHeight(mdTrayAssembly aAssy, uppSpliceStyles? aSpliceStyle = null)
        {
            if (aAssy == null || aAssy.Downcomers.Count <= 0) return 24;
            double _rVal =  aAssy.Downcomers.Spacing - aAssy.Downcomer().BoxWidth;
            if (!aSpliceStyle.HasValue) aSpliceStyle = aAssy.SpliceStyle;
            if (aSpliceStyle.Value == uppSpliceStyles.Joggle || aSpliceStyle.Value == uppSpliceStyles.Angle)
            {
                _rVal += 1.0625 * 2;
            }
            else if (aSpliceStyle.Value == uppSpliceStyles.Tabs)
            {
                _rVal += 1.1975 * 2;
            }


            return _rVal;
        }



        public static double IdealSectionHeight(mdTrayAssembly aAssy, uppSpliceStyles? aSpliceStyle = null)
        {
            if (aAssy == null) return 24;

            if (!aSpliceStyle.HasValue) aSpliceStyle = aAssy.SpliceStyle;

            double _rVal = aAssy.ManholeID;
            if (aSpliceStyle.Value == uppSpliceStyles.Tabs) _rVal -= 1.992 + 1.1050 - 1.5;
            if (aSpliceStyle.Value == uppSpliceStyles.Tabs) _rVal -= 1.1050;
            _rVal -= 0.5;
            return _rVal;

        }

        internal static UHOLEARRAY GetSpoutGroupsSpouts(IEnumerable< mdSpoutGroup> aSpoutGroups, bool bReturnQuad2 = false, bool bReturnQuad3 = false, bool bReturnQuad4 = false, double aDepth = 0, dynamic aZ = null)
        {
            UHOLEARRAY _rVal = new UHOLEARRAY(false);
            if (aSpoutGroups == null) return _rVal;
            List<mdSpoutGroup> groups = aSpoutGroups.ToList();
            mdSpoutGroup aSG;
            for (int i = 1; i <= groups.Count; i++)
            {
                aSG = groups[i-1];
                _rVal.Add(aSG.GetSpoutGroupSpouts(bReturnQuad2, bReturnQuad3, bReturnQuad4, aDepth, aZ));
            }
            return _rVal;
        }
        internal static UHOLEARRAY GetSpoutGroupsSpouts(IEnumerable<mdSpoutGroup> aSpoutGroups, bool bReturnInstances =false, double aDepth = 0, dynamic aZ = null)
        {
            UHOLEARRAY _rVal = new UHOLEARRAY(false);
            if (aSpoutGroups == null) return _rVal;
            List<mdSpoutGroup> groups = aSpoutGroups.ToList();
            mdSpoutGroup aSG;
            for (int i = 1; i <= groups.Count; i++)
            {
                aSG = groups[i - 1];
                _rVal.Add(aSG.GetSpoutGroupSpouts(bReturnInstances, aDepth, aZ));
            }
            return _rVal;
        }

        public static double SpliceBoltSpacing(int aBoltCount, double aLength, double aInset = 1)
         => (aBoltCount > 1 && aLength > 0) ? (aLength - (2 * aInset)) / (aBoltCount - 1) : 0;

        internal static UHOLES SpliceBoltHoles(ULINE aLine, int aCount, UHOLE aHole, double aZ, UHOLES aCollector) => SpliceBoltHoles(aLine, aCount, aHole, aZ, aCollector, out double _);

        internal static UHOLES SpliceBoltHoles(ULINE aLine, int aCount, UHOLE aHole, double aZ, UHOLES aCollector, out double rSpace)
        {
            UHOLES _rVal;
            _rVal = aCollector;
            rSpace = mdUtils.SpliceBoltSpacing(aCount, Math.Sqrt(Math.Pow(aLine.ep.X - aLine.sp.X, 2) + Math.Pow(aLine.ep.Y - aLine.sp.Y, 2)));
            UHOLE iHole = aHole;
            iHole.Elevation = aZ;

            double f1;
            if (_rVal.Centers.Count <= 0) _rVal.Member = iHole;

            if (aCount <= 0) return _rVal;

            //horizontal line
            if (aCount == 1)
            {
                iHole.Center = aLine.MidPt;
                _rVal.Centers.Add(iHole.Center);
            }
            else
            {
                if (Math.Round(aLine.sp.Y, 2) == Math.Round(aLine.ep.Y, 2))
                {
                    f1 = (aLine.sp.X < aLine.ep.X) ? 1 : -1;

                    iHole.X = aLine.sp.X + 1 * f1;
                    iHole.Y = aLine.sp.Y;

                    for (int i = 1; i <= aCount; i++)
                    {
                        _rVal.Centers.Add(iHole.Center);
                        iHole.X += rSpace * f1;
                    }
                }
                else
                { //vertical line
                    f1 = (aLine.sp.Y < aLine.ep.Y) ? 1 : -1;

                    iHole.X = aLine.sp.X;
                    iHole.Y = aLine.sp.Y + 1 * f1;

                    for (int i = 1; i <= aCount; i++)
                    {
                        _rVal.Centers.Add(iHole.Center);
                        iHole.Y += rSpace * f1;
                    }
                }
            }
            return _rVal;
        }

        internal static UVECTORS CreateDowncomerShapeVerts(mdDowncomer aDC, double aTopY, double aBottomY, bool bApplyLimitLine, ULINE aLimitLine, double aEdgeClearance, out bool rClipped, out double rLeftX, out double rRightX)
        {
            UVECTORS _rVal = UVECTORS.Zero;
            rLeftX = 0;
            rRightX = 0;
            rClipped = false;
            if (aDC == null) return _rVal;

            double wd = aDC.Width / 2;
            double clrc = (aEdgeClearance < 0) ?  mdSpoutGroup.GetDefaultClearance(aDC) : aEdgeClearance;
            UVECTOR u1;
            UVECTOR u2;
            UVECTOR u3;
            double YTop = aTopY;
            double YBot = aBottomY;
            bool bTria = false;
            double YMin = 0;
            double cX = aDC.X;

            mzUtils.SortTwoValues(true, ref YBot, ref YTop);

            if (YTop < 0 && YBot < 0)
            {
                if (aDC.DesignFamily.IsBeamDesignFamily()) return _rVal;
            }

            if (bApplyLimitLine)
            {
                if (aLimitLine.Length == 0) aLimitLine = aDC.LimLines(true).First().Line1.Value;


                bTria = Round(aLimitLine.DeltaY, 2) != 0;
                YMin = aLimitLine.MinY;
            }

            if (clrc > wd) clrc = wd;
            rRightX = cX + (wd - clrc);
            rLeftX = cX - (wd - clrc);
            if (rRightX < cX) rRightX = cX;
            if (rLeftX > cX) rLeftX = cX;

            if (!bApplyLimitLine)
            {
                //full rectangle
                _rVal.Add(rLeftX, YTop);
                _rVal.Add(rLeftX, YBot);
                _rVal.Add(rRightX, YBot);
                _rVal.Add(rRightX, YTop);
            }
            else
            {
                u1 = aLimitLine.IntersectionPt(new ULINE(rLeftX, 0, rLeftX, 100));
                if (u1.Y > YBot)
                {
                    if (!bTria)
                    {
                        if (YMin < YTop)
                        {
                            rClipped = true;
                            YTop = YMin;
                        }
                        //full rectangle
                        _rVal.Add(rLeftX, YTop);
                        _rVal.Add(rLeftX, YBot);
                        _rVal.Add(rRightX, YBot);
                        _rVal.Add(rRightX, YTop);
                    }
                    else
                    {
                        if (u1.Y <= YTop)
                        {
                            //limit line intersects left edge
                            _rVal.Add(rLeftX, u1.Y);
                            _rVal.Add(rLeftX, YBot);
                            u1 = aLimitLine.IntersectionPt(new ULINE(0, YBot, 100, YBot));
                            if (u1.X < rRightX)
                            {
                                //triangle
                                rClipped = true;
                                _rVal.Add(u1.X, YBot);
                            }
                            else
                            {
                                //trapezoid
                                rClipped = true;
                                _rVal.Add(rRightX, YBot);
                                u1 = aLimitLine.IntersectionPt(new ULINE(rRightX, 0, rRightX, 100));
                                _rVal.Add(rRightX, u1.Y);
                            }
                        }
                        else
                        {
                            _rVal.Add(rLeftX, YTop);
                            _rVal.Add(rLeftX, YBot);
                            u1 = aLimitLine.IntersectionPt(new ULINE(0, YTop, 100, YTop));

                            if (u1.X < rRightX)
                            {
                                //limit line intersects top edge
                                u2 = aLimitLine.IntersectionPt(new ULINE(0, YBot, 100, YBot));

                                if (u2.X < rRightX)
                                {
                                    //limit line intersects bottom edge
                                    rClipped = true;
                                    _rVal.Add(u2.X, YBot);
                                    _rVal.Add(u1.X, YTop);
                                }
                                else
                                {
                                    //clipped corner
                                    rClipped = true;
                                    _rVal.Add(rRightX, YBot);
                                    u3 = aLimitLine.IntersectionPt(new ULINE(rRightX, 0, rRightX, 100));

                                    _rVal.Add(rRightX, u3.Y);
                                    _rVal.Add(u1.X, YTop);
                                }
                            }
                            else
                            {
                                //full rectangle
                                _rVal = UVECTORS.Zero;
                                _rVal.Add(rLeftX, YTop);
                                _rVal.Add(rLeftX, YBot);
                                _rVal.Add(rRightX, YBot);
                                _rVal.Add(rRightX, YTop);

                            }
                        }
                    }
                }
            }


            return _rVal;
        }

        internal static USHAPE CreateDowncomerShape(mdDowncomer aDC, double aTopY, double aBottomY, bool bApplyLimitLine, ULINE aLimitLine, double aEdgeClearance, out bool rClipped, out double rLeftX, out double rRightX)
        {
            rClipped = false;
            rLeftX = 0;
            rRightX = 0;
            USHAPE _rVal = new USHAPE("")
            {
                Vertices = CreateDowncomerShapeVerts(aDC, aTopY, aBottomY, bApplyLimitLine, aLimitLine, aEdgeClearance, out rClipped, out rLeftX, out rRightX)
            };
            _rVal.Update();
            return _rVal;
        }

       
        /// <summary>
        /// creates the holes in the manway sections for the fasteners (clips or clamps)
        /// </summary>
        /// <param name="aSection"></param>
        /// <param name="aAssy"></param>
       
        /// <returns></returns>
        internal static UHOLEARRAY GenerateManwayFastenerHoles(uopSectionShape aSection, mdTrayAssembly aAssy)
        {
            UHOLEARRAY _rVal = new UHOLEARRAY(false);
            if(aSection == null) return _rVal;
            aSection.GetSplices(out uopDeckSplice aTopSplice, out uopDeckSplice aBottomSplice);
            aAssy ??= aSection.MDTrayAssembly;
            if ( aAssy == null || aTopSplice == null || aBottomSplice == null) return _rVal;

            if (!aSection.IsManway) return _rVal;

            UHOLE aHole;
            UHOLE bHole;
            double span;
            UHOLES aHls = new UHOLES("MANWAY");
            UHOLES bHls = new UHOLES("MANWAY");
            ULINE aLine;
            double cX = 0;
            double cY = 0;
            uopVector cp = aSection.Center;
            uopVector u1 = uopVector.Zero;
            uopVector u2 = uopVector.Zero;
            uopVector u3 = uopVector.Zero;
            double d1 = 0;
            double spcs = 0;
            double hspc = 0;
            double iset = 0;



            //add the slots for the manway clips/clamps
            //---------------------------------------------------------------------------------------------
            if (!aAssy.DesignOptions.UseManwayClips)
            {
                //---------------------------------------------------------------------------------------------
                //FOOTBALLS

                u1 = aSection.Bounds.TopLeft;
                u2 = aSection.Bounds.TopRight;
                span = Abs(u1.Y - u2.Y);
                aHole = new UHOLE(mdGlobals.gsBigHole, 0, 0, aDepth: aSection.Thickness, aTag: "MANWAY CLAMP", aFlag: "HOLE", aElevation: 0.5 * aSection.Thickness, aDownSet: 1.38, aInset: 1.25);
                aHls.Member = aHole;

                cX = u1.X + aHole.Inset;
                aLine = new ULINE(new UVECTOR(cX, u1.Y), new UVECTOR(cX, u2.Y));

                if (span - 4 > 4.5)
                { aHls = uopUtils.LayoutHolesOnLine2(aLine, aHole,aTargetSpace: 6, aEndBuffer: 2.5,  aFlag: "LEFT"); }
                else
                { aHls.Centers.Add(aLine.MidPt, "LEFT"); }
                bHls = new UHOLES(aHls, true);
                aHls.Rotation = 180;
                bHls.Rotation = 0;

                cX = cp.X + Abs(cp.X - cX);
                for (int i = 1; i <= aHls.Centers.Count; i++)
                { bHls.Centers.Add(cX, aHls.Centers.Item(i).Y); }

                aHls.Member.ZDirection = "0,0,1";
                bHls.Member.ZDirection = "0,0,1";

                _rVal.Add(aHls, "MANWAY");
                _rVal.Add(bHls, "MANWAY");


                if (aAssy.DesignOptions.SpliceStyle != uppSpliceStyles.Tabs)
                {
                    aHls.Centers = UVECTORS.Zero;
                    u2 =  aSection.Vertices.GetVector(dxxPointFilters.GetLeftTop);
                    span = Abs(u2.X - u1.X);

                    cY = u1.Y - aHole.Center.DownSet;
                    aLine = new ULINE(new UVECTOR(u1.X, cY), new UVECTOR(u2.X, cY));

                    if (span - 3 > 4.5)
                    { aHls = uopUtils.LayoutHolesOnLine2(aLine, aHole,aTargetSpace: 6, aEndBuffer:3, aFlag:"TOP"); }
                    else
                    { aHls.Centers.Add(aLine.MidPt, "TOP"); }
                    bHls = new UHOLES(aHls, true);

                    aHls.Rotation = 90;
                    bHls.Rotation = 270;


                    cY = cp.Y - Abs(cY - cp.Y);
                    for (int i = 1; i <= aHls.Centers.Count; i++)
                    {
                        bHls.Centers.Add(aHls.Centers.Item(i).X, cY);
                    }
                    aHls.Member.ZDirection = "0,0,1";
                    bHls.Member.ZDirection = "0,0,1";

                    _rVal.Add(aHls, "MANWAY");
                    _rVal.Add(bHls, "MANWAY");


                }


                //---------------------------------------------------------------------------------------------
            }
            else
            {
                //---------------------------------------------------------------------------------------------
                //SLIDERS

                aHole = new UHOLE(mdGlobals.ManwayClipMountingSlot(aAssy, 180));  
                aHls.Member = aHole;
                bHls.Member = aHole;
                aHls.Rotation = 180;
                bHole = aHole;

                u3.X = aSection.Right - aHole.Inset;
                u1.X = aSection.Left + aHole.Inset;
                aHole.X = u1.X;
                bHole.X = u3.X;
                iset = (aAssy.DesignOptions.SpliceStyle != uppSpliceStyles.Tabs) ? 5.5 : 3;


                u1.Y = aTopSplice.Ordinate - (aTopSplice.GapValue(true) + iset);
                if (aTopSplice.SpliceType == uppSpliceTypes.SpliceWithAngle) u1.Y -= 0.0625;


                u2.Y = aBottomSplice.Ordinate + (aBottomSplice.GapValue(true) + iset);
                if (aBottomSplice.SpliceType == uppSpliceTypes.SpliceWithAngle) u2.Y += 0.0625;

                d1 = u1.Y - u2.Y;
                aHole.Y = u1.Y;
                bHole.Y = aHole.Y;

                if (u2.Y >= u1.Y || d1 <= 4.5)
                {
                    aHole.Y = aSection.Y;
                    bHole.Y = aHole.Y;
                    aHole.X = u1.X;
                    aHls.Centers.Add(aHole.Center, "LEFT");
                    bHls.Centers.Add(bHole.Center, "RIGHT");

                }
                else
                {
                    aHls.Centers.Add(aHole.Center, "LEFT");
                    bHls.Centers.Add(bHole.Center, "RIGHT");

                    spcs = Math.Truncate(d1 / 9);
                    hspc = d1 / (spcs + 1);
                    while (!(spcs <= 0))
                    {
                        aHole.Y -= hspc;
                        bHole.Y = aHole.Y;

                        aHls.Centers.Add(aHole.Center, "LEFT");
                        bHls.Centers.Add(bHole.Center, "RIGHT");
                        spcs -= 1;
                    }
                    aHole.Y = u2.Y;
                    bHole.Y = aHole.Y;
                    aHls.Centers.Add(aHole.Center, "LEFT");
                    bHls.Centers.Add(bHole.Center, "RIGHT");


                }

                if (aHls.Centers.Count > 0) _rVal.Add(aHls, "MANWAY");
                if (bHls.Centers.Count > 0) _rVal.Add(bHls, "MANWAY");



                if (aAssy.DesignOptions.SpliceStyle != uppSpliceStyles.Tabs)
                {
                    aHls.Centers = UVECTORS.Zero;
                    bHls.Centers = aHls.Centers;
                    aHls.Rotation = 90;
                    bHls.Rotation = 270;

                    u1.X = aSection.Left + 3;
                    u2.X = aSection.Right - 3;
                    u1.Y = aTopSplice.Ordinate - (aTopSplice.GapValue(true) + 0.0625 + aHole.DownSet);
                    u2.Y = u1.Y;
                    u3.Y = aBottomSplice.Ordinate + (aBottomSplice.GapValue(true) + 0.0625 + aHole.DownSet);
                    aHole.Y = u1.Y;
                    bHole.Y = u3.Y;
                    aHole.X = u1.X;
                    bHole.X = aHole.X;

                    d1 = u2.X - u1.X;
                    if (u1.X >= u2.X || d1 <= 4.5)
                    {
                        aHole.X = aSection.X;
                        bHole.X = aHole.X;
                        aHls.Centers.Add(aHole.Center, "TOP");
                        bHls.Centers.Add(bHole.Center, "BOTTOM");

                    }
                    else
                    {
                        aHls.Centers.Add(aHole.Center, "TOP");
                        bHls.Centers.Add(bHole.Center, "BOTTOM");

                        spcs = Math.Truncate(d1 / 9);
                        hspc = d1 / (spcs + 1);
                        while (!(spcs <= 0))
                        {
                            aHole.X += hspc;
                            bHole.X = aHole.X;

                            aHls.Centers.Add(aHole.Center, "TOP");
                            bHls.Centers.Add(bHole.Center, "BOTTOM");
                            spcs--;
                        }
                        aHole.X = u2.X;
                        bHole.X = aHole.X;
                        aHls.Centers.Add(aHole.Center, "TOP");
                        bHls.Centers.Add(bHole.Center, "BOTTOM");


                    }


                    if (aHls.Centers.Count > 0) _rVal.Add(aHls, "MANWAY");
                    if (bHls.Centers.Count > 0) _rVal.Add(bHls, "MANWAY");


                }


            }

            return _rVal;
        }

        /// <summary>
        /// used by tray assmblies to layout its stiffeners and finger clips
        /// </summary>
        /// <param name="aSpace"></param>
        /// <param name="aDowncomer"></param>
        /// <param name="aAssy"></param>
        /// <param name="bStraddle"></param>
        /// <param name="bBestFit"></param>
        /// <returns></returns>
        public static List<double> StiffenersGenerate(double aSpace, mdDowncomer aDowncomer, mdTrayAssembly aAssy, bool bStraddle = false, bool bBestFit = false)
        {
            List<double> _rVal = new List<double>();
            if (aDowncomer == null) return _rVal;

            if (aDowncomer.ProjectType == uppProjectTypes.MDSpout) return _rVal;

            aAssy = aDowncomer.GetMDTrayAssembly(aAssy);

            if (aAssy == null) return _rVal;

            if (aAssy.ProjectType == uppProjectTypes.MDSpout) return _rVal;

            double topMandatoryStiffenerRangeMaxY, topMandatoryStiffenerRangeMinY, bottomMandatoryStiffenerRangeMaxY, bottomMandatoryStiffenerRangeMinY;

            List<double> aVals = new List<double>();

            aSpace = Abs(aSpace);
            if (aSpace <= 0) aSpace = 18;
            if (aSpace <= 3.5) aSpace = 3.5;
            aSpace = Round(aSpace, 5);

            foreach (var box in aDowncomer.Boxes)
            {
                if (box.IsVirtual)
                {
                    continue;
                }

                (topMandatoryStiffenerRangeMaxY, topMandatoryStiffenerRangeMinY) = box.TopMandatoryStiffenerRangeOrdinates();
                (bottomMandatoryStiffenerRangeMaxY, bottomMandatoryStiffenerRangeMinY) = box.BottomMandatoryStiffenerRangeOrdinates();

                if (bBestFit)
                {
                    aVals = GetStiffenerOrdinatesUsingBestFitStrategy(topMandatoryStiffenerRangeMaxY, topMandatoryStiffenerRangeMinY, bottomMandatoryStiffenerRangeMaxY, bottomMandatoryStiffenerRangeMinY, aSpace, bStraddle);
                }
                else
                {
                    aVals = GetStiffenerOrdinatesUsingExactStrategy(topMandatoryStiffenerRangeMaxY, topMandatoryStiffenerRangeMinY, bottomMandatoryStiffenerRangeMaxY, bottomMandatoryStiffenerRangeMinY, aSpace, bStraddle);
                }

                _rVal.AddRange(aVals);
            }

            _rVal.Sort();
            _rVal.Reverse();
            return _rVal;
        }

        private static List<double> GetStiffenerOrdinatesUsingBestFitStrategy(double topMandatoryStiffenerRangeMaxY, double topMandatoryStiffenerRangeMinY, double bottomMandatoryStiffenerRangeMaxY, double bottomMandatoryStiffenerRangeMinY, double space, bool straddle)
        {
            List<double> aVals = new List<double>();

            double maxY = topMandatoryStiffenerRangeMaxY;
            double minY = bottomMandatoryStiffenerRangeMinY;

            double spcs = FindNumberOfStiffenersForBestFitStrategy(minY, maxY, space, straddle);

            double lng = maxY - minY;
            double d1 = lng / spcs;

            double lY = uopUtils.RoundTo(d1, dxxRoundToLimits.Eighth, false, true);
            double sY = minY + (lng - spcs * lY) / 2;

            d1 = lY;

            aVals.Add(sY);
            for (int i = 1; i <= spcs; i++)
            {
                sY += d1;
                aVals.Add(sY);
            }

            if (!aVals.Any(y => y <= (topMandatoryStiffenerRangeMaxY + 0.01) && y >= (topMandatoryStiffenerRangeMinY - 0.01)))
            {
                aVals.Add(topMandatoryStiffenerRangeMaxY);
            }

            if (!aVals.Any(y => y <= (bottomMandatoryStiffenerRangeMaxY + 0.01) && y >= (bottomMandatoryStiffenerRangeMinY - 0.01)))
            {
                aVals.Add(bottomMandatoryStiffenerRangeMinY);
            }

            return aVals;
        }

        private static double FindNumberOfStiffenersForBestFitStrategy(double minY, double maxY, double space, bool straddle)
        {
            double lng = maxY - minY;
            double spcs = Round(lng / space, 0);
            if (spcs < 1) spcs = 1;

            double d1 = lng / spcs;
            if (d1 > space) spcs += 1;

            if (straddle)
            {
                if (!mzUtils.IsOdd(spcs)) spcs++;
            }
            else
            {
                if (mzUtils.IsOdd(spcs)) spcs++;
            }

            return spcs;
        }

        private static List<double> GetStiffenerOrdinatesUsingExactStrategy(double topMandatoryStiffenerRangeMaxY, double topMandatoryStiffenerRangeMinY, double bottomMandatoryStiffenerRangeMaxY, double bottomMandatoryStiffenerRangeMinY, double space, bool straddle)
        {
            List<double> aVals = new List<double>();

            double maxY = topMandatoryStiffenerRangeMaxY;
            double minY = bottomMandatoryStiffenerRangeMinY;

            double startY = straddle ? 0.5 * space : 0;

            if (minY < 0 && maxY > 0) // The range that contains zero Y and may need the straddle condition to be considered
            {
                // Going upward
                if (startY - minY < 3.5)
                {
                    aVals.Add(minY);
                }
                else
                {
                    aVals.Add(startY);
                }

                AddPossibleStiffenerOrdinatesUsingExactStrategyForSpecifiedRange(startY, maxY, space, aVals);

                // Going downward
                startY = straddle ? -0.5 * space : 0; // reset the sY for direction change

                if (straddle)
                {
                    if (maxY - startY < 3.5)
                    {
                        aVals.Add(maxY);
                    }
                    else
                    {
                        aVals.Add(startY);
                    }
                }

                AddPossibleStiffenerOrdinatesUsingExactStrategyForSpecifiedRange(startY, minY, -space, aVals);
            }
            else
            {
                bool goUpward = minY > 0 && maxY > 0;

                if (goUpward)
                {
                    // Find the first Y
                    while (startY < minY)
                    {
                        startY += space;
                    }

                    if (startY - minY < 3.5)
                    {
                        aVals.Add(minY);
                    }
                    else
                    {
                        aVals.Add(startY);
                    }

                    AddPossibleStiffenerOrdinatesUsingExactStrategyForSpecifiedRange(startY, maxY, space, aVals);
                }
                else
                {
                    // Find the first Y
                    while (startY > maxY)
                    {
                        startY -= space;
                    }

                    if (maxY - startY < 3.5)
                    {
                        aVals.Add(maxY);
                    }
                    else
                    {
                        aVals.Add(startY);
                    }

                    AddPossibleStiffenerOrdinatesUsingExactStrategyForSpecifiedRange(startY, minY, -space, aVals);
                }
            }

            if (!aVals.Any(y => y <= (topMandatoryStiffenerRangeMaxY + 0.01) && y >= (topMandatoryStiffenerRangeMinY - 0.01)))
            {
                aVals.Add(topMandatoryStiffenerRangeMaxY);
            }

            if (!aVals.Any(y => y <= (bottomMandatoryStiffenerRangeMaxY + 0.01) && y >= (bottomMandatoryStiffenerRangeMinY - 0.01)))
            {
                aVals.Add(bottomMandatoryStiffenerRangeMinY);
            }

            return aVals;
        }

        private static void AddPossibleStiffenerOrdinatesUsingExactStrategyForSpecifiedRange(double currentPosition, double to, double space, List<double> ordinates)
        {
            if (space >= 0) // Going upward
            {
                while (currentPosition < to)
                {
                    currentPosition += space;
                    if (currentPosition <= to)
                    {
                        if (to - currentPosition < 3.5)
                        {
                            ordinates.Add(to);
                        }
                        else
                        {
                            ordinates.Add(currentPosition);
                        }
                    }
                }
            }
            else
            {
                while (currentPosition > to)
                {
                    currentPosition += space;
                    if (currentPosition >= to)
                    {
                        if (currentPosition - to < 3.5)
                        {
                            ordinates.Add(to);
                        }
                        else
                        {
                            ordinates.Add(currentPosition);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 1 the first splice handle to search for
        /// 2 the second splice handle to search for
        /// returns the first member in the collection with the passed splice handle
        /// </summary>
        /// <param name="aAngles"></param>
        /// <param name="aSpliceHandle"></param>
        /// <param name="bSpliceHandle"></param>
        /// <returns></returns>
        public static colUOPParts GetAnglesBySpliceHandles(colUOPParts aAngles, string aSpliceHandle, string bSpliceHandle)
        {
            colUOPParts _rVal = new colUOPParts
            {
                MaintainIndices = false
            };
            if (string.IsNullOrWhiteSpace(aSpliceHandle) && string.IsNullOrWhiteSpace(bSpliceHandle)) return _rVal;
            if (aAngles == null) return _rVal;
            string h1 = string.IsNullOrWhiteSpace(aSpliceHandle) ? string.Empty : aSpliceHandle.Trim();
            string h2 = string.IsNullOrWhiteSpace(bSpliceHandle) ? string.Empty : bSpliceHandle.Trim();
            mdSpliceAngle aMem;
            mdSpliceAngle aRet = null;
            mdSpliceAngle bRet = null;
            if (string.Compare(h1, h2, ignoreCase: true) == 0) h2 = string.Empty;

            bool bJustOne = h1 ==  string.Empty || h2 == string.Empty;

            for (int i = 1; i <= aAngles.Count; i++)
            {
                aMem = (mdSpliceAngle)aAngles.Item(i);
                if (bJustOne)
                {
                    if (h2 ==  string.Empty)
                    {
                        if (string.Compare(aMem.SpliceHandle, h1, ignoreCase: true) == 0)
                        {
                            _rVal.Add(aMem);
                            return _rVal;

                        }
                    }
                    else
                    {
                        if (string.Compare(aMem.SpliceHandle, h2, ignoreCase: true) == 0)
                        {
                            _rVal.Add(aMem);
                            return _rVal;

                        }
                    }
                }
                else
                {
                    if (aRet == null && aSpliceHandle !=  string.Empty)
                    {
                        if (string.Compare(aMem.SpliceHandle, h1, ignoreCase: true) == 0) aRet = aMem;

                    }
                    if (bRet == null && bSpliceHandle !=  string.Empty)
                    {
                        if (string.Compare(aMem.SpliceHandle, h2, ignoreCase: true) == 0) bRet = aMem;

                    }
                    if (aRet != null && bRet != null) break;

                }
            }
            if (aRet != null && bRet != null)
            {
                if (aRet == bRet) bRet = null;

            }

            _rVal.Add(aRet);
            _rVal.Add(bRet);
            return _rVal;
        }

       
     

       

        /// <summary>
        /// returns bubble promoters for all the panels in the tray
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aDeckSplices"></param>
        /// <returns></returns>
        public static uopVectors BPCenters(mdTrayAssembly aAssy, List<uopDeckSplice> aDeckSplices) => aAssy == null ? uopVectors.Zero :new uopVectors( aAssy.DowncomerData.BPSites(aAssy, aDeckSplices));

        /// <summary>
        /// returns bubble promoters for all the panels in the tray
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        internal static UVECTORS BPSites(mdTrayAssembly aAssy, DowncomerDataSet aDowncomerDataSet = null)
        {
            UVECTORS _rVal = UVECTORS.Zero;
            if (aAssy == null) return _rVal;
            DowncomerDataSet dcdata = aDowncomerDataSet  == null ? aAssy.DowncomerData : aDowncomerDataSet;

            if (dcdata.Count <= 0) return _rVal;

            _rVal.Suppressed = !aAssy.DesignOptions.HasBubblePromoters;
            List<mdSpoutGroup> sGroups = aAssy.SpoutGroups.GetByVirtual(aVirtualValue: false, bNonZeroOnly: true);
       
            double rad = mdGlobals.BPRadius;
            bool symmetric = dcdata.DesignFamily.IsStandardDesignFamily();
            bool specialcase = symmetric && dcdata.OddDowncomers && dcdata.Count > 1;


            int pcnt = dcdata.Count + 1;

            foreach (mdSpoutGroup sGroup in sGroups)
            {

                int dcidx = sGroup.DowncomerIndex;
                int dpidx = sGroup.PanelIndex;
                DowncomerInfo dcinfo = dcdata.Item(dcidx);

                if (dcinfo == null) continue;

                double x1 = sGroup.PatternY;
                double y1 = dcinfo.X;
                _rVal.Add(x1, y1,aRow: dpidx, aCol: dcidx, rad);

                if (!symmetric) continue;

                if (Round(Abs(y1), 2) > 0.01)
                {
                    _rVal.Add(x1, -y1, dpidx, dcidx, rad);

                }
                if (specialcase && dpidx == pcnt - 1)
                {
                    _rVal.Add(-x1, y1, pcnt, dcidx, rad);
                    if (Round(Abs(y1), 2) > 0.01)
                    {
                        _rVal.Add(-x1, -y1, pcnt, dcidx, rad);

                    }
                }

            }


            _rVal.Invalid = false;
            return _rVal;
        }


        private static dxePolygon _SlotPolyline;
        private static double _CurThk;
        private static double _CurGap;

        /// <summary>
        /// returns the plan view of a deck section slot & tab slot
        /// </summary>
        /// <param name="aMatThk"></param>
        /// <param name="aGap"></param>
        /// <returns></returns>


        public static dxePolygon TabSlotPolygon(double aMatThk, double aGap)
        {
            dxePolygon _rVal = null;
            if (_CurThk != aMatThk && _CurGap != Math.Abs(aGap)) _SlotPolyline = null;


            _CurThk = aMatThk;
            _CurGap = Math.Abs(aGap);
            if (_SlotPolyline == null)
            {
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
            }
            else _rVal = _SlotPolyline;
            return _rVal;  //.Clone();
        }





        /// <summary>
        /// used by panels and downcomers to layout their ring clip holes
        /// </summary>
        /// <param name="aRingClipSegments"></param>
        /// <param name="aSpacing"></param>
        /// <param name="bAtLeastOne"></param>
        /// <param name="bSavePointsToSegments"></param>
        /// <returns></returns>
        internal static uopHoles LayoutRingClipHoles(List<iSegment> aRingClipSegments,  double aRingRadius, double aSpacing = 0, bool bAtLeastOne = false, bool bSavePointsToSegments = false, double aDepth = 0)
        {
            uopHoles _rVal = new uopHoles("RING CLIP", new uopHole(mdGlobals.gsBigHole, aTag: "RING CLIP", aFlag: "HOLE", aDepth: aDepth, aElevation: aDepth/2));
            if (aRingClipSegments == null) return _rVal;
            if (aRingClipSegments.Count <= 0 ) return _rVal;


            uopHole hole = _rVal.Member;
            double rad = hole.Radius;

            double MAXSPC = (aSpacing <= 0) ? mdGlobals.DefaultRingClipSpacing : aSpacing;
            double hdOD = mdGlobals.HoldDownWasherDiameter;

       
            try
            {
                List<uopArc> arcs = new List<uopArc>();
                List<uopLine> lines = new List<uopLine>();
                foreach (iSegment seg in aRingClipSegments)
                {
                    if (seg.IsArc) arcs.Add(seg.Arc); else lines.Add(seg.Line);
                }

                List<uopArc> workArcs = new List<uopArc>();


                if (arcs.Count > 1)
                {

                    List<int> used = new List<int>();
                    for (int i = 1; i <= arcs.Count; i++)
                    {
                        if (used.Contains(i - 1)) continue;
                        uopArc arc1 = arcs[i - 1];
                        uopArc arc3 = null;
                        for (int j = 1; j <= arcs.Count; j++)
                        {
                            if (i == j ) continue;
                            uopArc arc2 = arcs[j - 1];
                            if( Math.Round(arc1.EndAngle,1) ==Math.Round(arc2.EndAngle,1))
                            {
                                arc3 = arc2;
                                used.Add(arcs.IndexOf(arc2) );
                                break;
                            }
                           
                        }
                        if(arc3 != null) 
                        { 
                            arc1 = new uopArc(arc1);
                            arc1.EndAngle = arc3.EndAngle;
                        }
                        int idx = arcs.IndexOf(arc1);
                        if (!used.Contains(idx))
                            workArcs.Add(arc1);
                        else
                            used.Add(idx);

                    }

                }
                else
                {
                    workArcs = arcs;
                }
                aRingClipSegments.Clear();


                foreach (uopArc arc in workArcs)
                {
                    aRingClipSegments.Add(arc);
                    if (bSavePointsToSegments) arc.Points.Clear();

                    double alen = arc.Length;
                    if (alen >= 1.5 * hdOD)
                    {
                        uopVectors centers = uopUtils.LayoutPointsOnArc(arc, aTargetSpace: MAXSPC, aDiameter: hole.Diameter, bCenterOnArc: true, aElevation: _rVal.Elevation, bSaveToArc: true, aTag: "RING CLIP");
                        _rVal.Centers.Append(centers, aTag: "RING CLIP", aFlag: "TO RING", aRadius: rad, bNoDupes: true, aNoDupesPrecis: 1);

                    }
                    else
                    {
                        if (alen >= hdOD || bAtLeastOne) _rVal.Centers.Add(arc.MidPoint, aTag: "RING CLIP", aRadius: rad, aFlag: "TO RING", bNoDupes: true, aNoDupesPrecis: 1); ;

                    }
                }

                foreach(uopLine line in lines)
                {
                    aRingClipSegments.Add(line);
                    if (bSavePointsToSegments) line.Points.Clear();
                    var alen = line.Length;
                    if (alen >= 1.5 * hdOD)
                    {
                        if (alen >= MAXSPC)
                        {
                               uopVectors centers = uopUtils.LayoutHolesOnLine(line, aTargetSpace: MAXSPC, aDiameter: hole.Diameter, aElevation: _rVal.Elevation, aTag: "RING CLIP", bSaveToLine: bSavePointsToSegments).Centers;
                            _rVal.Centers.Append(centers, aTag: "RING CLIP", aRadius: rad, aFlag: "TO DIVIDER", bNoDupes: true, aNoDupesPrecis: 1);
                        }
                        else
                        {
                            if (alen >= 0.65 * MAXSPC)
                            {
                                _rVal.Centers.Add(line.StartPt, aTag: "RING CLIP", aRadius: rad, aFlag: "TO DIVIDER", bNoDupes: true, aNoDupesPrecis: 1);
                                _rVal.Centers.Add(line.EndPt, aTag: "RING CLIP", aRadius: rad, aFlag: "TO DIVIDER", bNoDupes: true, aNoDupesPrecis: 1);
                            }
                            else
                            {
                                _rVal.Centers.Add(line.MidPt, aTag: "RING CLIP", aRadius: rad, aFlag: "TO DIVIDER", bNoDupes: true, aNoDupesPrecis: 1);
                            }
                        }
                    }
                    else
                    {
                        if (alen >= 2 * hdOD || bAtLeastOne)
                        {
                            if (aRingRadius > 0 && line.MidPt.Length() <= aRingRadius - (0.5 * hdOD))
                            {
                                _rVal.Centers.Add(line.MidPt, aTag: "RING CLIP", aRadius: rad, aFlag: "TO DIVIDER", bNoDupes: true, aNoDupesPrecis: 1);
                            }
                        }
                    }
                }

        
               
                return _rVal;
            }
            catch (Exception ex)
            {
         
                throw ex;
            }


        }

        public static int SpliceBoltCount(double aAngleLength, double aDeckThickness, double aEndBuffer = 0.375)
        {

            aAngleLength = Abs(aAngleLength) - Abs(aEndBuffer);


            if (Round(aDeckThickness, 3) > 0.109)
            {
                //thick decking has 9" MAX spacing
                if (aAngleLength < 11) return 2;

                if (aAngleLength < 20) return 3;

                else if (aAngleLength < 29) return 4;
                return 5;

            }
            else
            { //normal spacing is 8"
                if (aAngleLength < 3.5) return 1;

                if (aAngleLength < 10) return 2;

                if (aAngleLength < 18) return 3;

                if (aAngleLength < 26) return 4;

                return 5;

            }


        }



        public static int MaxDowcomerCount(double aRingID, double aSpacing, int aCount, double aDCWidth)
        {
            if (aSpacing <= 0 || aRingID <= 0 || aCount <= 0) return 0;
            aDCWidth = Math.Abs(aDCWidth);
            int count = aCount;
            double rad = aRingID / 2;

            int _rVal = 0;
            bool even = uopUtils.IsEven(aCount);

            double X1 = even ? -0.5 * aSpacing : -aSpacing;

            while (X1 + aSpacing + aDCWidth <= rad - 1.5) { _rVal++; if (X1 != 0) _rVal++; X1 += aSpacing; }
            return _rVal;

        }
         

        public static List<double> DowncomerXValues(mdTrayAssembly aAssy, double? aSpace = null, double? aOffset = null, mdDowncomer aBasis = null)
        {
            List<double> _rVal = new List<double>();
            if (aAssy == null) return _rVal;
            if (!aSpace.HasValue)
                aSpace = aAssy.DowncomerSpacing;
            if (!aOffset.HasValue)
                aOffset = aAssy.DowncomerOffset;

            aBasis ??= aAssy.Downcomer();
            int cnt = aBasis.Count;
            if (aSpace.Value == 0 && cnt > 1)
            {
                aSpace = mdSpacingSolutions.ComputeOptimizedSpace(aAssy, out _);
                aAssy.Downcomers.OptimumSpacing = aSpace.Value;
            }

            if (cnt <= 1 || aSpace <= 0)
            {
                _rVal.Add(aOffset.Value);
                return _rVal;
            }

            double halfcnt = Math.Truncate((double)cnt / 2);
            if (!uopUtils.IsOdd(cnt)) halfcnt -= 0.5;
            double x1 = aSpace.Value * halfcnt + aOffset.Value;
            _rVal.Add(x1);
            do
            {
                x1 -= aSpace.Value;
                _rVal.Add(x1);
            } while (_rVal.Count < cnt);

            return _rVal;
        }


        /// <summary>
        /// the table that describes the materials of the tray
        /// </summary>
        /// <param name="aProject"></param>
        /// <param name="aUnits"></param>
        /// <returns></returns>
        public static uopTable MaterialTable(mdProject aProject, uppUnitFamilies aUnits = uppUnitFamilies.English)
        {
            throw new NotImplementedException("MaterialTable is not implemented yet. Please use the appropriate method from the mdUtils class.");
            //uopTable _rVal = new uopTable();
            //if (aProject == null) return _rVal;
            //mdTrayRange aRange = null;
            //mdTrayAssembly aA = null;
            //List<string> tbl2 = null;
            //mdDowncomer aDC = null;
            //uopSheetMetal aSM = null;

            //int i = 0;
            //double thk = 0;
            //double mt = 0;
            //string pn = string.Empty;

            //double thk2 = 0;
            //uopPart P1 = null;
            //string txt = string.Empty;

            //colUOPTrayRanges aRanges = null;

            //List<uopMaterial> aMtrls = null;

            //uopMaterial aMtrl = null;

            //string tlist = string.Empty;

            //bool bmatvaries = false;

            //string rList = string.Empty;


            //aRanges = aProject.TrayRanges;


            //tbl2 = new List<string>();

            //if (aRanges.Count <= 0) return _rVal;

            //for (i = 1; i <= aRanges.Count; i++)
            //{
            //    mzUtils.ListAdd(ref rList, aRanges.Item(i).SpanName());
            //}
            //rList = uopUtils.TrayList(rList, ",", true, false);

            //if (aUnits != uppUnitFamilies.English)
            //{
            //    aUnits = uppUnitFamilies.Metric;
            //}
            //if (aUnits == uppUnitFamilies.English)
            //{
            //    mt = 1;
            //}
            //else
            //{
            //    mt = 25.4;
            //}


            ////===================== SHEET METAL MATERIAL TABLE =================================
            //tbl2 = new List<string>
            //{
            //    //the header row
            //    "PART|TRAYS|THICKNESS|MATERIAL|SPECIFICATION"
            //};
            //aRange = (mdTrayRange)aRanges.Item(1);
            //aA = aRange.TrayAssembly;
            //aDC = aA.Downcomer();
            //P1 = aDC;
            //aSM = (uopSheetMetal)aDC.Material;
            //thk = aSM.Thickness;
            //thk2 = thk;

            ////default lines
            //pn = aProject.GasketSpec;
            //if (string.IsNullOrEmpty(pn))
            //{
            //    pn = uopMaterialSpecs.FirstName(uppSpecTypes.Gasket);
            //}

            //if (pn.IndexOf("DURA", StringComparison.OrdinalIgnoreCase) > 0 || pn ==  string.Empty)
            //{
            //    tbl2.Add("GASKET|-|1/16 INCH|DURABLA (NON-ASBESTOS)|DURALON 8500 (GREEN)");
            //}
            //else
            //{
            //    tbl2.Add("GASKET|-|1/16 INCH|???|" + pn.ToUpper());
            //}

            //tbl2.Add("PIPE|-|SEE DWGS.|???|???");
            //tbl2.Add("FLANGE|-|150# R.F.S.O.|???|???");
            //tbl2.Add("PLATE|-|SEE DWGS.|???|???");

            //xAddTableLine2(aProject, tbl2, "TROUGH", "-", aSM, mt, aUnits);
            //xAddTableLine2(aProject, tbl2, "CHUTE", "-", aSM, mt, aUnits);


            //aMtrls = aRanges.GetMaterials(uppPartTypes.Downcomer, true);
            //txt = "DOWNCOMER";
            //if (aMtrls.Count > 0)
            //{
            //    aMtrl = aMtrls[0];
            //    if (aMtrls.Count <= 1)
            //    {
            //        xAddTableLine2(aProject, tbl2, txt, rList, aMtrl, mt, aUnits);
            //    }
            //    else
            //    {
            //        bmatvaries = true;
            //        for (i = 0; i < aMtrls.Count; i++)
            //        {
            //            aMtrl = aMtrls[i];
            //            xAddTableLine2(aProject, tbl2, txt, uopUtils.TrayList(aMtrl.SpanName, ",", true, false), aMtrl, mt, aUnits);
            //        }

            //    }
            //}

            //aMtrls = aRanges.GetMaterials(uppPartTypes.SupplementalDeflector, true);
            //txt = "SUPPLEMENTAL DEFLECTOR";
            //if (aMtrls.Count > 0)
            //{
            //    aMtrl = aMtrls[0];
            //    if (aMtrls.Count <= 1)
            //    {
            //        xAddTableLine2(aProject, tbl2, txt, rList, aMtrl, mt, aUnits);
            //    }
            //    else
            //    {
            //        bmatvaries = true;
            //        for (i = 0; i < aMtrls.Count; i++)
            //        {
            //            aMtrl = aMtrls[i];
            //            xAddTableLine2(aProject, tbl2, txt, uopUtils.TrayList(aMtrl.SpanName, ",", true, false), aMtrl, mt, aUnits);
            //        }

            //    }
            //}

            //aMtrls = aRanges.GetMaterials(uppPartTypes.CrossBrace, true);
            //txt = "CROSSBRACE / END ANGLE";
            //if (aMtrls.Count <= 0)
            //{
            //    txt = "END ANGLE";
            //    aMtrls = aRanges.GetMaterials(uppPartTypes.EndAngle, true);
            //}

            //if (aMtrls.Count > 0)
            //{
            //    if (aMtrls.Count <= 1)
            //    {
            //        xAddTableLine2(aProject, tbl2, txt, rList, aMtrls[0], mt, aUnits);
            //    }
            //    else
            //    {
            //        bmatvaries = true;
            //        for (i = 0; i < aMtrls.Count; i++)
            //        {
            //            aMtrl = aMtrls[0];

            //            xAddTableLine2(aProject, tbl2, txt, uopUtils.TrayList(aMtrl.SpanName, ",", true, false), aMtrl, mt, aUnits);
            //        }
            //    }
            //}


            //aMtrls = aRanges.GetMaterials(uppPartTypes.DeckSection, true);
            //txt = "TRAY DECK";
            //if (aMtrls.Count > 0)
            //{
            //    aMtrl = aMtrls[0];
            //    if (aMtrls.Count <= 1)
            //    {
            //        xAddTableLine2(aProject, tbl2, txt, rList, aMtrl, mt, aUnits);
            //    }
            //    else
            //    {
            //        bmatvaries = true;
            //        for (i = 0; i < aMtrls.Count; i++)
            //        {
            //            aMtrl = aMtrls[i];
            //            xAddTableLine2(aProject, tbl2, txt, uopUtils.TrayList(aMtrl.SpanName, ",", true, false), aMtrl, mt, aUnits);
            //        }

            //    }
            //}

            //aMtrls = aRanges.GetMaterials(uppPartTypes.FingerClip, true);
            //txt = "FINGER CLIP (PN 50)";
            //if (aMtrls.Count > 0)
            //{
            //    aMtrl = aMtrls[0];
            //    if (aMtrls.Count <= 1)
            //    {
            //        xAddTableLine2(aProject, tbl2, txt, rList, aMtrl, mt, aUnits);
            //    }
            //    else
            //    {
            //        bmatvaries = true;
            //        for (i = 0; i < aMtrls.Count; i++)
            //        {
            //            aMtrl = aMtrls[i];
            //            xAddTableLine2(aProject, tbl2, txt, uopUtils.TrayList(aMtrl.SpanName, ",", true, false), aMtrl, mt, aUnits);
            //        }

            //    }
            //}

            //aMtrls = aRanges.GetMaterials(uppPartTypes.RingClip, true, uppRingClipSizes.ThreeInchRC);
            //txt = "RING CLIP (PN 30)";
            //if (aMtrls.Count > 0)
            //{
            //    aMtrl = aMtrls[0];
            //    if (aMtrls.Count <= 1)
            //    {
            //        xAddTableLine2(aProject, tbl2, txt, rList, aMtrl, mt, aUnits);
            //    }
            //    else
            //    {
            //        bmatvaries = true;
            //        for (i = 0; i < aMtrls.Count; i++)
            //        {
            //            aMtrl = aMtrls[i];
            //            xAddTableLine2(aProject, tbl2, txt, uopUtils.TrayList(aMtrl.SpanName, ",", true, false), aMtrl, mt, aUnits);
            //        }

            //    }
            //}

            //aMtrls = aRanges.GetMaterials(uppPartTypes.RingClip, true, aSubPartType: uppRingClipSizes.FourInchRC);
            //txt = "RING CLIP (PN 40/45)";
            //if (aMtrls.Count > 0)
            //{
            //    aMtrl = aMtrls[0];
            //    if (aMtrls.Count <= 1)
            //    {
            //        xAddTableLine2(aProject, tbl2, txt, rList, aMtrl, mt, aUnits);
            //    }
            //    else
            //    {
            //        bmatvaries = true;
            //        for (i = 0; i < aMtrls.Count; i++)
            //        {
            //            aMtrl = aMtrls[i];
            //            xAddTableLine2(aProject, tbl2, txt, uopUtils.TrayList(aMtrl.SpanName, ",", true, false), aMtrl, mt, aUnits);
            //        }

            //    }
            //}

            //aMtrls = aRanges.GetMaterials(uppPartTypes.ManwayClamp, true);
            //txt = "RING CLIP (PN 40/45)";
            //if (aMtrls.Count > 0)
            //{
            //    aMtrl = aMtrls[0];
            //    if (aMtrls.Count <= 1)
            //    {
            //        xAddTableLine2(aProject, tbl2, txt, rList, aMtrl, mt, aUnits);
            //    }
            //    else
            //    {
            //        bmatvaries = true;
            //        for (i = 0; i < aMtrls.Count; i++)
            //        {
            //            aMtrl = aMtrls[i];
            //            xAddTableLine2(aProject, tbl2, txt, uopUtils.TrayList(aMtrl.SpanName, ",", true, false), aMtrl, mt, aUnits);
            //        }

            //    }
            //}


            ////        Set P1 = aA.SpliceAngles.Item(1)
            ////        If P1 Is Nothing Then Set P1 = aA.SpliceAngle
            ////        xAddTableLine1 tbl2, P1, "SPLICE ANGLE / MANWAY ANGLE", mt, aUnits

            ////        If aRange.DesignFamily = uopECMDDesign Then
            ////            xAddTableLine1 tbl2, aA.Deck, "DEFLECTOR PLATE", mt, aUnits
            ////        End If

            ////        If aA.HasAntiPenetrationPans Then
            ////            Set P1 = aA.APPans.Item(1)
            ////            If Not P1 Is Nothing Then xAddTableLine1 tbl2, P1, "ANTI-PENETRATION PAN", mt, aUnits
            ////        End If

            ////        xAddTableLine1 tbl2, aA.RingClip, "RING CLIP BODY (PN 30)", mt, aUnits

            ////        Set P1 = aA.RingClip
            ////        pn = "RING CLIP BODY (PN 30)"
            ////        Set aSM = P1.SheetMetal
            ////        GoSub AddLine:

            ////        Set P2 = aA.RingClip(True)
            ////        If P2.PartType <> P1.PartType Then
            ////            Set P1 = P2
            ////            pn = "RING CLIP BODY (PN 45)"
            ////            GoSub AddLine:
            ////        End If

            ////       Set aFC = aA.FingerClip
            ////        Set P1 = aFC
            ////        pn = "FINGER CLIP (PN 50)"
            ////        GoSub AddLine:

            ////        If aA.DesignOptions.UseManwayClips Then
            ////            Set P1 = aA.ManwayClip
            ////            pn = "MANWAY CLAMP & WASHER (PN 64/63)"
            ////        Else
            ////            Set P1 = aA.ManwayClamp
            ////            pn = "MANWAY CLAMP (PN 62)"
            ////        End If

            ////        GoSub AddLine:

            ////        Set P1 = aA.HoldDownWasher
            ////        If Not P1 Is Nothing Then
            ////            pn = "FLATWASHER"
            ////            If aUnits = English Then
            ////                If aProject.Bolting = English Then
            ////                    pn = pn & " (1.75 OD/0.438 ID)"
            ////                Else
            ////                    pn = pn & " (" & Format(44 / 25.4, "0.000") & " OD/" & Format(11 / 25.4, "0.000") & " ID)"
            ////                End If

            ////            Else
            ////                If aProject.Bolting = English Then
            ////                    pn = pn & " (" & Format(1.75 * 25.4, "0.0") & "mm OD/" & Format(0.438 * 25.4, "0.000") & "mm ID)"
            ////                Else
            ////                    pn = pn & " (44mm OD/11mm ID)"
            ////                End If

            ////            End If
            ////            GoSub AddLine:
            ////        End If

            ////        '===================== HARDWARE MATERIAL ENTRIES =================================
            ////        If aA.DesignOptions.UseManwayClips Then
            ////            Set P1 = aA.ManwayClip.SetScrew
            ////        Else
            ////            Set P1 = aA.ManwayClamp.Stud
            ////        End If


            ////        pn = UCase(P1.DescriptiveName)
            ////        Set iH = P1
            ////       Set aPart = P1.Part
            ////       GoSub AddLine2:

            ////       Set aBlt = aFC.Bolt
            ////       Set P1 = aBlt
            ////        pn = "BOLT"
            ////        Set iH = P1
            ////       Set aPart = P1.Part
            ////       GoSub AddLine2:

            ////        Set P1 = aBlt.Nut
            ////        pn = "NUT"
            ////        Set iH = P1
            ////       Set aPart = P1.Part
            ////       GoSub AddLine2:

            ////        Set P1 = aBlt.Washer
            ////        pn = "FLAT WASHER"
            ////        Set iH = P1
            ////       Set aPart = P1.Part
            ////       GoSub AddLine2:

            ////      Set P1 = aBlt.LockWasher
            ////        pn = "LOCK WASHER"
            ////        Set iH = P1
            ////       Set aPart = P1.Part
            ////       GoSub AddLine2:

            ////Done:
            //if (!bmatvaries)
            //{
            //    _rVal.SetDimensions(tbl2.Count, 4);
            //}
            //else
            //{
            //    _rVal.SetDimensions(tbl2.Count, 5);
            //}

            //for (i = 1; i <= tbl2.Count; i++)
            //{
            //    tlist = tbl2[i - 1];
            //    List<string> row = mzUtils.StringsFromDelimitedList(tlist, "|", true);
            //    if (!bmatvaries)
            //    {

            //        _rVal.SetByCollection(i, false, row, aSkipList: new List<int> { 2 });
            //    }
            //    else
            //    {
            //        _rVal.SetByCollection(i, false, row);
            //    }

            //}

            ////Exit Function
            ////AddLine:
            ////    xAddTableLine1 tbl2, P1, pn, mt, aUnits

            ////     Return
            ////Exit Function
            ////AddLine2:


            ////        txt = pn & "|"
            ////        If iH.IsMetric Then
            ////            txt = txt & "METRIC|"
            ////        Else
            ////            txt = txt & "UNC|"
            ////        End If

            ////         txt = txt & iH.Material.FamilySelectName & "|" & P1.HardwareSpec

            ////        tbl2.Add txt


            ////    Return

            //return _rVal;
        }

        public static string PatternName(uppSpoutPatterns aPatternType, bool bNullForUndefined = false)
        {

            return aPatternType switch
            {
                uppSpoutPatterns.B => "B",
                uppSpoutPatterns.C => "C",
                uppSpoutPatterns.D => "D",
                uppSpoutPatterns.A => "A",
                uppSpoutPatterns.Astar => "*A*",
                uppSpoutPatterns.S1 => "S1",
                uppSpoutPatterns.S2 => "S2",
                uppSpoutPatterns.S3 => "S3",
                uppSpoutPatterns.SStar => "*S*",
                _ => !bNullForUndefined ? "Undefined" : ""
            };

        }

        public static bool RequiresTwoEndSupportBolts(double aX, double aDCInsideWidth, double aDeckRadius, double aThickness)
        {
            if (aDeckRadius <= 0) return false;
            double wd = Round(aDCInsideWidth, 5);
            aThickness = Abs(aThickness);
            if (wd >= 8) return true;
            if (wd < 5) return false;

            double xr = Math.Abs(aX) + (wd / 2d);

            if (xr > aDeckRadius) return false;

            double xl = xr - wd;
            double yR = Math.Sqrt(Math.Pow(aDeckRadius, 2) - Math.Pow(xr + aThickness, 2));
            double yL = Round(aX, 5) != 0 ? Math.Sqrt(Math.Pow(aDeckRadius, 2) - Math.Pow(xl, 2)) : yR;

            double d1 = Math.Round(Math.Sqrt(Math.Pow(xr - xl, 2) + Math.Pow(yR - yL, 2)), 5);
            return  d1>= mdGlobals.LimitLineLengthLimitForTwoRingClips;

        }

        
        public static uopProperties SetConstraintPropertyLimits(uopProperties aProperties, mdSpoutGroup aSpoutGroup, mdSpoutGrid aGrid)
        {


            if (aProperties == null || aSpoutGroup == null) return null;


            TPROPERTIES aProps = new TPROPERTIES(aProperties);
            DowncomerInfo dcinfo = aSpoutGroup.DCInfo;
            if (dcinfo == null) return aProperties;
            aGrid ??= aSpoutGroup.Grid;
            double sdia = aProps.ValueD("SpoutDiameter");
            double aMin = 0;
            double aMax = 0;
            double aMarg = aProps.ValueD("Margin");
            double aclrc = mzUtils.ThisOrThat(aProps.ValueD("Clearance"), dcinfo.SpoutGroupClearance, 0);
            double epclrc = aProps.ValueD("EndPlateClearance");
            bool bMetric = dcinfo.MetricSpouting;
          

            double sgHt = aSpoutGroup.PerimeterV.Height;


            uppSpoutPatterns pType = (uppSpoutPatterns)aProps.ValueI("PatternType", aDefault: (int)uppSpoutPatterns.Undefined);

            //if (pType == uppSpoutPatterns.Undefined)  return _rVal;



            if (epclrc <= 0) epclrc = 0.25;
            if (sdia <= 0) sdia = dcinfo.SpoutDiameter;

            if (sdia <= 0) sdia = !bMetric ? 0.75 : 19 / 25.4;


            URECTANGLE pBigArea = aSpoutGroup.SpoutArea.BoxWeirs.BoundsV();
            bool bisHole = pType < uppSpoutPatterns.S3;

            aProps.SetMinMax("Clearance", 0.0625, 0.5 * dcinfo.InsideWidth - sdia / 2.0, 0, aIncrement: 0.05);
            aProps.SetMinMax("EndPlateClearance", 0.0625, Round(0.5 * pBigArea.Height, 0), 0, aIncrement: 0.0625);
            aProps.SetMinMax("Margin", 0, 0.5 * sgHt - sdia / 2.0, 0, aIncrement: 0.0625);

            if (pType != uppSpoutPatterns.SStar)
            { aProps.SetMinMax("YOffset", 0, 0, 0, aIncrement: 0); }
            else
            { aProps.SetMinMax("YOffset", -10, 10, 0, aIncrement: 0.0625); }

            if (pType == uppSpoutPatterns.Undefined)
                 aProps.SetMinMax("SpoutCount", 0, 0, -1, aIncrement: 1); 
            else
                aProps.SetMinMax("SpoutCount", -1, 20000, -1, aIncrement: 1); 

            if (bisHole || pType == uppSpoutPatterns.SStar)
            { aProps.SetMinMax("SpoutLength", 0, 0, 0, aIncrement: 0); }
            else
            {
                aMin = (!bMetric) ? sdia + 0.125 : sdia + 1 / 25.4;

                aMax = mdSpoutGrid.MaxSpoutLength(dcinfo, pType, aclrc, true, bMetric);
                aProps.SetMinMax("SpoutLength", aMin, aMax, aIncrement: !bMetric ? 0.0625 : 1 / 25.4);
            }

            if (!bisHole || pType == uppSpoutPatterns.Undefined)
            { aProps.SetMinMax("HorizontalPitch", 0, 0, 0, aIncrement: 0.0625); }
            else
            {
                aMin = sdia + 1 / 8.0;
                aMax = pBigArea.Right - pBigArea.Left - sdia;
                aProps.SetMinMax("HorizontalPitch", aMin, aMax, 0, aIncrement: 0.0625);
            }

            if (pType == uppSpoutPatterns.Undefined)
            { aProps.SetMinMax("VerticalPitch", 0, 0, 0, aIncrement: 0); }
            else
            {
                if (pType == uppSpoutPatterns.A || pType == uppSpoutPatterns.Astar)
                { aMin = uopUtils.RoundTo(sdia + (1 / 16.0), dxxRoundToLimits.Sixteenth, true); }
                else
                { aMin = uopUtils.RoundTo(sdia + (3 / 16.0), dxxRoundToLimits.Sixteenth, true); }
                aMax = 0.5 * dcinfo.InsideWidth;
                aProps.SetMinMax("VerticalPitch", aMin, aMax, 0, aIncrement: 0.0625);
            }


            return new uopProperties(aProps);
        }

    

        /// <summary>
        /// sorts the stiffeners in the collection in top to bottom
        /// </summary>
        /// <param name="aStiffeners"></param>
        /// <param name="aAssy"></param>
        public static void StiffenersSort(colUOPParts aStiffeners, mdTrayAssembly aAssy)
        {

            if (aStiffeners == null) return;
            aAssy ??= aStiffeners.GetMDTrayAssembly(aAssy);
            if (aAssy == null) return;


            List<uopPart> newCol = new List<uopPart>();


            List<mdDowncomer> DComers = aAssy.Downcomers.GetByVirtual(aVirtualValue: false);
            List<mdDeckSection> manSecs = aAssy.DeckSections.Manways;
            List<int> dcIDs = aStiffeners.DowncomerIDs();
            List<uopPart> aSet;
            colUOPParts aParts;
            double aX;
            double aY;


            for (int i = 0; i < dcIDs.Count; i++)
            {
                int dcid = dcIDs[i];
                if (dcid > DComers.Count) dcid = uopUtils.OpposingIndex(dcid, aAssy.Downcomer().Count);
                mdDowncomer aDC = DComers[dcid - 1];
                aParts = new colUOPParts(aStiffeners.GetByDowncomerIndex(dcid));


                if (aDC != null && aParts.Count > 0)
                {
                    //sort high to low
                    aParts.SortByOrdinate(dxxOrdinateDescriptors.Y, false, true, 3);
                    aX = aDC.X;

                    for (int j = 1; j <= aParts.Count; j++)
                    {
                        mdStiffener aStf = (mdStiffener)aParts.Item(j);
                        aStf.Mark = false;
                        aStf.X = aX;

                        aParts.SetItem(j, aStf);
                    }

                    if (aAssy.DesignFamily.IsBeamDesignFamily())
                    {
                        foreach (mdStiffener stiffener in aParts)
                        {
                            var containingBox = aDC.Boxes.FirstOrDefault(b => b.IsValidStiffenerOrdinate(stiffener.Y));
                            if (containingBox == null) // If it cannot find the box, it means that the stiffener's ordinate is not within any of the downcomer boxes min and max ordinates
                            {
                                stiffener.Mark = true;
                            }
                        }
                    }
                    else
                    {
                        //make sure none are too high
                        aY = aDC.MaxStiffenerOrdinate(aAssy);
                        aSet = aParts.GetAboveOrdinate(dxxOrdinateDescriptors.Y, aY, bOnIsIn: false);
                        if (aSet.Count > 0)
                        {
                            for (int k = 0; k < aSet.Count; k++)
                            {
                                mdStiffener aStf = (mdStiffener)aSet[0];
                                if (k == 1)
                                {
                                    if (aParts.GetBetweenOrdinate(dxxOrdinateDescriptors.Y, aY - 3, aY, bOnIsIn: true).Count <= 0)
                                    { aStf.Y = aY; }
                                    else
                                    { aStf.Mark = true; }
                                }
                                else
                                { aStf.Mark = true; }

                            }
                        }

                        //make sure none are too low
                        aY = -aY;
                        aSet = aParts.GetBelowOrdinate(dxxOrdinateDescriptors.Y, aY, false);
                        if (aSet.Count > 0)
                        {
                            for (int k = 0; k < aSet.Count; k++)
                            {
                                mdStiffener aStf = (mdStiffener)aSet[k];
                                if (k == 0)
                                {
                                    if (aParts.GetBetweenOrdinate(dxxOrdinateDescriptors.Y, aY - 3, aY, true).Count <= 0)
                                    { aStf.Y = aY; }
                                    else
                                    { aStf.Mark = true; }
                                }
                                else
                                { aStf.Mark = true; }

                            }


                            //make sure non are to close together
                            for (int j = 1; j <= aParts.Count; j++)
                            {
                                mdStiffener aStf = (mdStiffener)aParts.Item(j);

                                aY = aStf.Y;

                                if (aY >= 0)
                                {
                                    if (!aStf.Mark)
                                    {
                                        aSet = aParts.GetBetweenOrdinate(dxxOrdinateDescriptors.Y, aY - 3, aY + 3, false);
                                        if (aSet.Count > 1)
                                        {
                                            for (int k = 0; k < aSet.Count; k++)
                                            {
                                                mdStiffener bStf = (mdStiffener)aSet[k];
                                                if (bStf != aStf) bStf.Mark = true;


                                            }
                                        }
                                    }

                                }
                                else
                                {
                                    break;
                                }

                            }
                            for (int j = 1; j <= aParts.Count; j++)
                            {
                                mdStiffener aStf = (mdStiffener)aParts.Item(j);
                                aY = aStf.Y;
                                if (aY < 0)
                                {
                                    if (!aStf.Mark)
                                    {
                                        aSet = aParts.GetBetweenOrdinate(dxxOrdinateDescriptors.Y, aY - 3, aY + 3, false);
                                        if (aSet.Count > 1)
                                        {
                                            for (int k = 0; k < aSet.Count; k++)
                                            {
                                                mdStiffener bStf = (mdStiffener)aSet[k];
                                                if (bStf != aStf) bStf.Mark = true;

                                            }
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }

                                }



                            }


                        }
                    }


                    //remove the marked ones
                    aParts.GetByMark(true, bRemove: true);

                    //return the good ones to the passed collection
                    for (int j = 1; j <= aParts.Count; j++)
                    {
                        mdStiffener aStf = (mdStiffener)aParts.Item(j);
                        //set the finger clip points
                        aStf.FingerClipPts = FingerClipPoints(aAssy, aStf, manSecs, aDC);
                        newCol.Add(aStf);
                    }


                }



            }
            aStiffeners.Populate(newCol);
        }

   

        /// <summary>
        /// returns a collection of strings that are the names of the available spout patterns
        /// </summary>
        /// <returns></returns>
        public static List<string> SpoutPatternNames() => new List<string>() { "By Default", "D", "C", "B", "A", "*A*", "S3", "S2", "S1", "*S*" };

        /// <summary>
        /// returns the maximum distance between any consecutive members in the collection
        /// </summary>
        /// <param name="aDowncomer"></param>
        /// <returns></returns>
        public static double StiffenerMaxSpacing(mdDowncomer aDowncomer)
        => aDowncomer == null ? 0 :  mzUtils.MaxDifference(aDowncomer.StiffenerSites, iPrecis: 5);





        /// <summary>
        /// True if all stiffener positions are symetric across then X axis
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aStiffeners"></param>
        /// <param name="rNonSymetricCenters"></param>
        /// <returns></returns>
        public static bool StiffenersAreSymetric(mdTrayAssembly aAssy, colUOPParts aStiffeners, colDXFVectors rNonSymetricCenters = null)
        {

            if (aAssy == null) return false;

            aStiffeners ??= colUOPParts.FromPartsList(mdPartGenerator.Stiffeners_ASSY(aAssy, false));

            bool _rVal = true;
            List<mdDowncomer> DCs = aAssy.Downcomers.GetByVirtual(aVirtualValue: false);
            dxfPlane aPlane = dxfPlane.World;

            for (int i = 1; i <= DCs.Count; i++)
            {
                mdDowncomer dc = DCs[i - 1];
                aPlane.Y = dc.Y;
                colUOPParts Stfs = new colUOPParts(aStiffeners.GetByDowncomerIndex(i), false, bMaintainIndices: false);
                colDXFVectors aCtrs = Stfs.CentersDXF();

                if (!aCtrs.AreSymetric(dxxOrdinateDescriptors.Y, null, aPlane, rNonSymetricCenters))
                {
                    _rVal = false;
                }

            }
            return _rVal;
        }

        /// <summary>
        /// executed internally to create the stiffeners collection for the downcomer
        /// </summary>
        /// <param name="aDowncomers"></param>
        /// <param name="aAssy"></param>
        /// <param name="aSpacing"></param>
        /// <param name="bRegen"></param>
        /// <param name="bStraddle"></param>
        /// <param name="bBestFit"></param>
        /// <returns></returns>
        public static colUOPParts StiffenersByDowncomers(colMDDowncomers aDowncomers, mdTrayAssembly aAssy, double aSpacing = 0, bool bRegen = false, bool bStraddle = false, bool bBestFit = false)
        {
            colUOPParts _rVal = new colUOPParts();
            if (aDowncomers == null) return _rVal;
            aAssy = aDowncomers.GetMDTrayAssembly(aAssy);
            if (aAssy == null) return _rVal;

            string Sites = string.Empty;
            mdDowncomer aDC;
            mdStiffener aStf = null;
            double cY = 0;
            List<double> YOrds = new List<double>();
            _rVal.SubPart(aAssy);

            for (int j = 1; j <= aDowncomers.Count; j++)
            {
                aDC = aDowncomers.Item(j);
                Sites = (!bRegen) ? aDC.StiffenerSites : "";
                if (Sites ==  string.Empty & aSpacing == 0) aSpacing = aAssy.DesignOptions.StiffenerSpacing;

                if (Sites ==  string.Empty)
                {
                    if (aSpacing <= 0) aSpacing = 18;
                    if (aSpacing <= 3.5) aSpacing = 3.5;
                    YOrds = StiffenersGenerate(aSpacing, aDC, aAssy, bStraddle, bBestFit);
                    for (int k = 0; k < YOrds.Count; k++) { mzUtils.ListAdd(ref Sites, YOrds[k].ToString("0.0####"), bSuppressTest: true, aDelimitor: ","); }
                    aDC.PropValSet("StiffenerSites", Sites, bSuppressEvnts: true);
                    aDowncomers.SetItem(j, aDC);
                }
                else
                {
                    YOrds = TVALUES.FromDelimitedList(Sites, ",", false, false, true, true).ToNumericList();
                    YOrds.Sort();
                    YOrds.Reverse();
                }

                for (int i = 0; i < YOrds.Count; i++)
                {
                    cY = YOrds[i];
                    var containingBox = aDC.Boxes.FirstOrDefault(b => b.IsValidStiffenerOrdinate(cY)); 
                    if (containingBox != null)
                    {
                        aStf = aAssy.Stiffener(aDC, cY, containingBox.TopMandatoryStiffenerRangeOrdinates().MaxY);
                        aStf.Quantity = aDC.OccuranceFactor;
                        aStf.SubPart(aDC);
                        _rVal.Add(aStf);
                    }
                }
            }
            StiffenersSort(_rVal, aAssy);
            return _rVal;
        }

        /// <summary>
        /// executed to coy the stiffenr locations from one tray to another
        /// </summary>
        /// <param name="aToAssy"></param>
        /// <param name="aFromAssy"></param>
        /// <returns></returns>
        public static colUOPParts StiffenersCopy(mdTrayAssembly aToAssy, mdTrayAssembly aFromAssy)
        {
            colUOPParts _rVal = new colUOPParts();
            if (aToAssy == null || aFromAssy == null) return _rVal;
            List<mdDowncomer> aDCs = aToAssy.Downcomers.GetByVirtual(aVirtualValue: false);
            List<mdDowncomer> bDCs = aFromAssy.Downcomers.GetByVirtual(aVirtualValue: false);

            _rVal.SubPart(aToAssy);


            for (int j = 1; j <= aDCs.Count; j++)
            {
                mdDowncomer aDC = aDCs[j - 1];
                if (j > bDCs.Count) break;
                mdDowncomer bDC = bDCs[j - 1];

                double maxY = aDC.MaxStiffenerOrdinate(aToAssy);

                List<double> yvals = mzUtils.ListToNumericCollection(bDC.StiffenerSites, ",");
                for (int i = 1; i <= yvals.Count; i++)
                {
                    double cY = yvals[i - 1];

                    // If it is a beam desgin, we need to make sure there is a box in the distination downcomer that can contian the stiffener ordinate.
                    // If it is not a beam design, it is sufficient that the ordinate is within the max ordinate of the downcomer.
                    bool shouldAdd = aToAssy.DesignFamily.IsBeamDesignFamily() ? aDC.Boxes.FirstOrDefault(b => b.IsValidStiffenerOrdinate(cY))   != null : Abs(cY) <= maxY;

                    if (shouldAdd)
                    {
                        mdStiffener aStf = aDC.Stiffener(cY);
                        aStf.Quantity = aDC.OccuranceFactor;
                        aStf.SubPart(aDC);
                        _rVal.Add(aStf);
                    }

                }


            }


            StiffenersSort(_rVal, aToAssy);
            return _rVal;
        }

        /// <summary>
        /// executed internally to create the stiffeners collection for the downcomer
        /// </summary>
        /// <param name="aDowncomers"></param>
        /// <param name="aAssy"></param>
        /// <param name="aBuffer"></param>
        /// <returns></returns>
        public static colUOPParts StiffenersStraddleDowncomers(List<mdDowncomer> aDowncomers, mdTrayAssembly aAssy, double aBuffer)
        {
            colUOPParts _rVal = new colUOPParts();
            if (aDowncomers == null) return _rVal;
            if (aDowncomers.Count <= 0) return _rVal;
            mdDowncomer aDC = aDowncomers[0];
            aAssy ??= aDC.GetMDTrayAssembly(aAssy);

            if (aAssy == null) return _rVal;

            mdStiffener aStf = null;
            double cY = 0;
            mzValues YOrds = new mzValues();
            double maxY = 0;
            double wd = aAssy.Downcomer().BoxWidth / 2 + Abs(aBuffer);


            _rVal.SubPart(aAssy);
            for (int j = 1; j <= aDowncomers.Count; j++)
            {
                aDC = aDowncomers[j - 1];
                cY = aDC.X + wd;
                YOrds.Add(cY);
                if (Round(aDC.X, 1) > 0)
                {
                    cY = aDC.X - wd;
                    YOrds.Add(cY);
                }

            }


            for (int j = 1; j <= aDowncomers.Count; j++)
            {
                aDC = aDowncomers[j - 1];

                if (aAssy.DesignFamily.IsBeamDesignFamily())
                {
                    if (aDC.IsVirtual || aDC.Boxes.All(b => b.IsVirtual))
                    {
                        continue;
                    }

                    for (int i = 1; i <= YOrds.Count; i++)
                    {
                        cY = YOrds.Item(i);

                        if (aDC.Boxes.FirstOrDefault(b => b.IsValidStiffenerOrdinate(cY)) != null)
                        {
                            aStf = aDC.Stiffener(cY);
                            aStf.Quantity = aDC.OccuranceFactor;
                            aStf.SubPart(aDC);
                            _rVal.Add(aStf);
                        }

                        if (aDC.Boxes.FirstOrDefault(b => b.IsValidStiffenerOrdinate(-cY)) != null)
                        {
                            aStf = aDC.Stiffener(-cY);
                            aStf.Quantity = aDC.OccuranceFactor;
                            aStf.SubPart(aDC);
                            _rVal.Add(aStf);
                        }
                    }
                }
                else
                {
                    maxY = aDC.MaxStiffenerOrdinate(aAssy);
                    for (int i = 1; i <= YOrds.Count; i++)
                    {
                        cY = YOrds.Item(i);
                        if (Abs(cY) <= maxY)
                        {
                            aStf = aDC.Stiffener(cY);
                            aStf.Quantity = aDC.OccuranceFactor;
                            aStf.SubPart(aDC);
                            _rVal.Add(aStf);

                            aStf = aDC.Stiffener(-cY);
                            aStf.Quantity = aDC.OccuranceFactor;
                            aStf.SubPart(aDC);
                            _rVal.Add(aStf);

                        }

                    }
                }
            }
            StiffenersSort(_rVal, aAssy);
            return _rVal;
        }




        /// <summary>
        /// executed to create the centers for a downcomers stiffeners collection
        /// </summary>
        /// <param name="aDowncomer"></param>
        /// <param name="rMaxOrd"></param>
        /// <returns></returns>
        internal static UVECTORS StiffenerPoints(mdDowncomer aDowncomer, out List<(double, double)> minMaxOrds)
        {
            UVECTORS _rVal = UVECTORS.Zero;
            minMaxOrds = new List<(double, double)>();
            if (aDowncomer == null) return _rVal;
            double cY;
            double cX = aDowncomer.X;
            double supht = aDowncomer.SupplementalDeflectorHeight;
            List<double> YOrds = mzUtils.ListToNumericCollection(aDowncomer.StiffenerSites, ",", false, 5);
            string yVals = string.Empty;

            YOrds.Sort();
            YOrds.Reverse();

            minMaxOrds = aDowncomer.Boxes.Select(b => (b.BottomMandatoryStiffenerRangeOrdinates().MinY, b.TopMandatoryStiffenerRangeOrdinates().MaxY)).ToList();
            for (int i = 0; i < YOrds.Count; i++)
            {
                cY = YOrds[i];

                mdDowncomerBox containingBox = aDowncomer.Boxes.FirstOrDefault(b => b.IsValidStiffenerOrdinate(cY));  
                if (containingBox != null)
                {
                    mzUtils.ListAdd(ref yVals, cY.ToString());
                    _rVal.Add(cX, cY, aRadius: supht);
                }
            }
            aDowncomer.PropValSet("StiffenerSites", yVals, bSuppressEvnts: true);

            return _rVal;
        }



        /// <summary>
        /// suppresses the clips that fall on manways
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aStiffener"></param>
        /// <param name="aManwaySections"></param>
        /// <param name="aDowncomer"></param>
        /// <param name="bReturnSuppressed"></param>
        /// <returns></returns>
        internal static UVECTORS FingerClipPoints(mdTrayAssembly aAssy, mdStiffener aStiffener, List<mdDeckSection> aManwaySections, mdDowncomer aDowncomer, bool bReturnSuppressed = false)
        {
            UVECTORS _rVal = UVECTORS.Zero;
            if (aStiffener == null) return _rVal;

            aAssy ??= aStiffener.GetMDTrayAssembly();
            if (aAssy == null) return _rVal;

            aDowncomer ??= aAssy.Downcomers.Item(aStiffener.DowncomerIndex);
            if (aDowncomer == null) return _rVal;

            int pnlCnt = aAssy.DeckPanels.Count;
            double cY = aStiffener.Y;
            double cX = aStiffener.X;
            double wd = aDowncomer.Width;
            bool dontsuppressonmanways = (aAssy.IsSymmetric && aAssy.OddDowncomers) || !aAssy.DesignFamily.IsStandardDesignFamily();

            UVECTOR vleft = new UVECTOR(cX - wd / 2, cY) { Tag = "LEFT", Row = aDowncomer.Index }; //the points downcomer index
            UVECTOR vright = new UVECTOR(cX + wd / 2, cY) { Tag = "RIGHT", Col = aDowncomer.Index };  //the points panel index

            vleft.Col = (vleft.Row + 1 <= pnlCnt) ? vleft.Row + 1 : -vleft.Row;
            vleft.Value = aAssy.Deck.Thickness;
            vright.Value = vleft.Value;

            aManwaySections ??= aAssy.DeckSections.Manways;

            if (aManwaySections.Count <= 0)
            {
                _rVal.Add(vright);
                _rVal.Add(vleft);
                return _rVal;

            }

            List<mdDeckSection> manways = aManwaySections.FindAll(x => x.IsManway && x.PanelIndex == aDowncomer.Index + 1);
            foreach (mdDeckSection manway in manways)
            {
                if ((vleft.Y <= manway.TopSplice.Y + 1.5) && (vleft.Y >= manway.BottomSplice.Y - 1.5))
                {

                    vleft.Suppressed = !dontsuppressonmanways;
                    vleft.Mark = true;
                }



            }

            if (aDowncomer.Index > 1)
            {
                manways = aManwaySections.FindAll(x => x.IsManway && x.PanelIndex == aDowncomer.Index);
                foreach (mdDeckSection manway in manways)
                {
                    if ((vright.Y <= manway.TopSplice.Y + 0.5 * mdFingerClip.DefaultLength) && (vright.Y >= manway.BottomSplice.Y - 0.5 * mdFingerClip.DefaultLength))
                    {

                        vright.Suppressed = !dontsuppressonmanways;
                        vright.Mark = true;
                    }


                    //}

                }

            }


            if (!vright.Suppressed || bReturnSuppressed) _rVal.Add(new UVECTOR(vright));
            if (!vleft.Suppressed || bReturnSuppressed) _rVal.Add(new UVECTOR(vleft));


            return _rVal;
        }

        /// <summary>
        /// suppresses the clips that fall on manways
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aManwaySections"></param>
        /// <param name="bReturnSuppressed"></param>
        /// <param name="aStiffenerCenters"></param>
        /// <returns></returns>
        public static uopVectors FingerClipPoints(mdTrayAssembly aAssy, List<mdDowncomer> aDowncomers, List<mdDeckSection> aManwaySections, bool bReturnSuppressed = false, uopVectors aStiffenerCenters = null)
        {
            uopVectors _rVal = new uopVectors();


            if (aAssy == null) return _rVal;
            aManwaySections ??= aAssy.DeckSections.Manways;
            aStiffenerCenters ??= aAssy.StiffenerCenters(bTrayWide: false);
            bool dontsuppressonmanways = (aAssy.IsSymmetric && aAssy.OddDowncomers) || !aAssy.DesignFamily.IsStandardDesignFamily();
            double wd = aAssy.Downcomer().Width;
            aDowncomers ??= aAssy.Downcomers.GetByVirtual(aVirtualValue: false);
            int pnlCnt = aAssy.Downcomer().Count + 1;
            foreach (var item in aDowncomers)
            {
                List<double> stfys = dxfVectors.GetOrdinates(aStiffenerCenters.FindAll((x) => x.PartIndex == item.Index), aOrdType: dxxOrdinateDescriptors.Y, bUniqueValues: true);
                foreach (var y in stfys)
                {
                    UVECTOR left = new UVECTOR(item.X - wd / 2, y) { Tag = "LEFT", Row = item.Index, PartIndex = item.Index }; //the points downcomer index
                    UVECTOR right = new UVECTOR(item.X + wd / 2, y) { Tag = "RIGHT", Col = item.Index, PartIndex = item.Index };  //the points panel index
                    left.Col = (left.Row + 1 <= pnlCnt) ? left.Row + 1 : -left.Row;
                    left.Value = aAssy.Deck.Thickness;
                    right.Value = left.Value;
                    if (aManwaySections.Count <= 0 || dontsuppressonmanways)
                    {
                        _rVal.Add(right, aPartIndex: item.Index);
                        _rVal.Add(left, aPartIndex: item.Index);
                        continue;

                    }
                    List<mdDeckSection> manwaysleft = aManwaySections.FindAll(x => x.IsManway && x.PanelIndex == item.Index + 1);
                    foreach (mdDeckSection manwayl in manwaysleft)
                    {
                        if ((left.Y <= manwayl.TopSplice.Y + 0.5 * mdFingerClip.DefaultLength) && (left.Y >= manwayl.BottomSplice.Y - 0.5 * mdFingerClip.DefaultLength))
                        {

                            left.Suppressed = true;
                            left.Mark = true;
                        }

                        List<mdDeckSection> manwaysright = aManwaySections.FindAll(x => x.IsManway && x.PanelIndex == item.Index);
                        foreach (mdDeckSection manwayr in manwaysright)
                        {
                            if ((right.Y <= manwayr.TopSplice.Y + 0.5 * mdFingerClip.DefaultLength) && (right.Y >= manwayr.BottomSplice.Y - 0.5 * mdFingerClip.DefaultLength))
                            {

                                left.Suppressed = true;
                                left.Mark = true;
                            }
                        }
                    }
                    if (!right.Suppressed || bReturnSuppressed) _rVal.Add(right, aPartIndex: item.Index);
                    if (!left.Suppressed || bReturnSuppressed) _rVal.Add(left, aPartIndex: item.Index);
                }


            }


            return _rVal;
        }

        /// <summary>
        /// returns error strings in a collection if the passed stage is mechaically invalid
        /// </summary>
        /// <param name="aStage"></param>
        /// <returns></returns>
        public static List<object> ValidateStage(mdStage aStage)
        {
            List<object> _rVal = null;
            _rVal = new List<object>();
            if (aStage == null)
            {
                _rVal.Add("The Passed Stage Is Undefined");
                return _rVal;
            }

            if (aStage.PropValD("ShellID", aMultiplier: 12) < 32) _rVal.Add("Bad Shell ID Detected (32'' min).");
            if (aStage.PropValD("DCWidth") <= 0) _rVal.Add("Bad Downcomer Width Detected.");
            if (aStage.PropValD("DCHeight") <= 0) _rVal.Add("Bad Downcomer Width Detected.");
            if (aStage.PropValD("DCWidth") <= 0) _rVal.Add("Bad Downcomer Width Detected.");
            if (aStage.PropValD("WeirHeight") <= 0) _rVal.Add("Bad Weir Height Detected.");
            if (aStage.PropValD("TraySpacing") <= 0) _rVal.Add("Bad Tray Spacing Detected.");
            if (aStage.PropValD("SpoutArea") <= 0) _rVal.Add("Bad Spout Area Detected.");
            if (aStage.PropValD("PerfDiameter") <= 0) _rVal.Add("Bad Deck Perforation Diameter Detected.");
            return _rVal;
        }

        private static double xSpoutArea(double aRadius, double aLength, dynamic aMultiplier = null)
        {
            double xSpoutArea = 0;
            xSpoutArea = PI * aRadius * (PI * aRadius);
            if (aLength > (2 * aRadius))
            {
                xSpoutArea += (aLength - (2 * aRadius)) * (2 * aRadius);

            }
            //todo
            //if (!IsMissing(aMultiplier))
            //{
            //    xSpoutArea = xSpoutArea * aMultiplier;
            //}
            return xSpoutArea;
        }

        public static double DefaultAPPanClearance(mdTrayRange aRange)
        {
            if (aRange == null) return 0;
            mdTrayAssembly assy = aRange.TrayAssembly;
            mdDeck dk = assy.Deck;
            mdDowncomer dc = assy.Downcomer();


            return DefaultAPPanClearance(aRange.RingSpacing, dc.InsideHeight, dc.How, dc.Thickness, dk.Thickness);

        }



        public static double DefaultAPPanClearance(double aTraySpace, double aDowncomerInsideHeight, double aWeirHeight, double aDeckThickness, double aDowncomerThickness, double aAPThickness = 0)
        {
            double tspace = Math.Abs(aTraySpace);
            double dcht = Math.Abs(aDowncomerInsideHeight);
            double wht = Math.Abs(aWeirHeight);
            double dcthk = Math.Abs(aDowncomerThickness);
            double dkthk = Math.Abs(aDowncomerThickness);
            double apthk = Math.Abs(aAPThickness);
            if (apthk <= 0) apthk = Math.Min(dcthk, dkthk);

            double dc2deck = tspace - dkthk - (dcht - wht - dkthk + dcthk);
            double apht = ((dc2deck - apthk) / 2 > 6) ? dc2deck - apthk - 6 : (dc2deck - apthk) / 2;
            apht = uopUtils.RoundTo(apht, dxxRoundToLimits.Eighth, bRoundDown: true);
            double dstbelow = apht + apthk;
            double clrc = dc2deck - apht - apthk;
            return clrc;
        }

        public static double DefaultCDP(mdTrayRange aRange) => (aRange == null) ? 0 : DefaultCDP(aRange.RingSpacing);

        public static double DefaultCDP(double aTraySpace)
        {
            return uopUtils.RoundTo(Math.Abs(aTraySpace) * 0.75, dxxRoundToLimits.Sixteenth, bRoundUp: true);

        }

    }
}
