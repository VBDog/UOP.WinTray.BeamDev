using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Vml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Controls;
using System.Windows.Media;
using UOP.DXFGraphics;
using UOP.DXFGraphics.Utilities;
using UOP.WinTray.Projects;
using UOP.WinTray.Projects.Documents;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Materials;
using UOP.WinTray.Projects.Parts;

using UOP.WinTray.Projects.Tables;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;
using UOP.WinTray.UI.Interfaces;
using UOP.WinTray.UI.Model;

namespace UOP.WinTray.UI.BusinessLogic
{
    public class AppDrawingSourceHelper
    {
        private dxfBlock oDBorder = null;
        private dxfBlock oBBorder = null;
        private bool bCancelBorder;
        public delegate void StatusChangeHandler(string StatusString);
        public event StatusChangeHandler StatusChange;


        private string _Status;
        public string Status
        {
            get => _Status;
            set { _Status = value; if (!SuppressWorkStatus) StatusChange?.Invoke(_Status); }
        }

        private string _BaseStatus;
        public string BaseStatus
        {
            get { return _BaseStatus; }
            set { _BaseStatus = value; }
        }


        public TitleBlockHelper TitleBlockHelper { get; set; }

        public bool SuppressWorkStatus { get; set; }

        public IappDrawingSource Source { get; set; }


        public string CurrentLayer { get; set; }

        public void CancelBorder()
        {
            bCancelBorder = true;
        }

        public colDXFEntities CreatePNBubbles(dxfImage aImage, dxfVector aInsertionPt, double aScaleFactor, dxxOrthoDirections aDirection = dxxOrthoDirections.Right,
                                            string aPillString = "", string aHexString = "", string aTrailer = "", bool bTrailerBelow = false,
                                            double aXOffset = 0, double aYOffset = 0, bool bDontAddToImage = false, colDXFEntities aCollector = null,
                                            string aPillLayer = null, dxxTextTypes aTextType = dxxTextTypes.Multiline, bool bCenterOfPill = false)
        {
            colDXFEntities _rVal = new();

            if (aImage == null) return _rVal;

            aPillString ??= string.Empty;
            aHexString ??= string.Empty;
            aTrailer ??= string.Empty;

            if (aInsertionPt == null) aInsertionPt = dxfVector.Zero;
            aPillString = aPillString.Trim();
            aHexString = aHexString.Trim();
            aTrailer = aTrailer.Trim();

            if (string.IsNullOrWhiteSpace(aPillString) && string.IsNullOrWhiteSpace(aHexString))
            {
                bTrailerBelow = false;
            }

            dxfVector v1;

            double ang = 0;
            string[] sVals;
            string aStr;
            dxfVector v2;
            dxfDirection aDir;
            dxePolyline aPl;
            dxePolyline bPl;
            double tang = 0;
            dxfPrimatives prms = aImage.Primatives;
            dxoEntityTool etl = aImage.EntityTool;
            dxeText aTxt;
            double xscal = Math.Abs(aScaleFactor);
            if (xscal <= 0) xscal = aImage.Display.PaperScale;
            double bubLn = 0.75 * xscal;
            double bubHt = 0.375 * xscal;
            double hexLn = 0.433 * xscal;
            string lname = aImage.SymbolSettings.LayerName;
            dxxColors tclr = dxxColors.Undefined;

            dxxMTextAlignments talng;


            aImage.GetOrAddReference(lname, dxxReferenceTypes.LAYER, aColor: aImage.SymbolSettings.LayerColor);
            dxoLayer aLyr = (dxoLayer)aImage.GetOrAddReference(aImage.TextSettings.LayerName, dxxReferenceTypes.LAYER, aColor: aImage.TextSettings.LayerColor);


            if (aLyr != null) tclr = aLyr.Color;

            if (aDirection == dxxOrthoDirections.Down)
            {
                ang = 270;
                aDir = aImage.UCS.YDirection.Inverse();
                tang = 90;
                talng = dxxMTextAlignments.MiddleRight;
            }
            else if (aDirection == dxxOrthoDirections.Up)
            {
                ang = 90;
                aDir = aImage.UCS.YDirection;
                tang = 90;
                talng = dxxMTextAlignments.MiddleLeft;
            }
            else if (aDirection == dxxOrthoDirections.Right)
            {
                ang = 180;
                aDir = aImage.UCS.XDirection;
                talng = dxxMTextAlignments.MiddleLeft;
            }
            else
            {
                aDir = aImage.UCS.XDirection.Inverse();
                talng = dxxMTextAlignments.MiddleRight;
            }

            dxfPlane ocs = aImage.UCS;
            v1 = ocs.Vector(aInsertionPt.X + aXOffset, aInsertionPt.Y + aYOffset);


            v2 = v1.Clone();
            if (!string.IsNullOrWhiteSpace(aPillString))
            {


                aPl = (dxePolyline)prms.Pill(dxfVector.Zero, bubLn, bubHt, ang);
                aPl.Tag = "PILL";
                aPl.LCLSet(string.IsNullOrWhiteSpace(aPillLayer) ? lname : aPillLayer, dxxColors.ByLayer, dxfLinetypes.Continuous);
                sVals = aPillString.Split(new char[] { ',' });
                if (bCenterOfPill) v2 += aDir * -bubLn / 2;
                for (int i = 0; i < sVals.Length; i++)
                {


                    aStr = sVals[i].Trim();
                    v2 += aDir * bubLn / 2;
                    bPl = aPl.Clone();
                    bPl.Translate(v2);
                    bPl.Flag = (i + 1).ToString();
                    if (!bDontAddToImage)
                        bPl = (dxePolyline)aImage.Entities.Add(bPl);

                    _rVal.Add(bPl);
                    if (aCollector != null) aCollector.Add(bPl);


                    if (!string.IsNullOrWhiteSpace(aStr))
                    {
                        aTxt = etl.Create_Text(v2, aStr, 0.125 * xscal, dxxMTextAlignments.MiddleCenter, lname, "Standard", tclr, aTextAngle: tang, aTextType: aTextType, bSuppressUCS: true);
                        aTxt.Tag = "PILL_TEXT";
                        if (aTextType != dxxTextTypes.Multiline) aTxt.AttributeTag = aTxt.Tag;
                        aTxt.Flag = bPl.Flag;

                        if (!bDontAddToImage) aTxt = (dxeText)aImage.Entities.Add(aTxt);

                        _rVal.Add(aTxt);
                        if (aCollector != null) aCollector.Add(aTxt);

                    }
                    v2 += aDir * (bubLn / 2);
                }
            }

            if (!string.IsNullOrWhiteSpace(aHexString))
            {
                aPl = (dxePolyline)prms.Polygon(null, false, 6, bubHt / 2, ang, true);
                aPl.LCLSet(lname, dxxColors.ByLayer, dxfLinetypes.Continuous);
                aPl.Tag = "HEX";
                sVals = mzUtils.ListValues(aHexString, ",").ToArray();
                for (int i = 0; i < sVals.Count(); i++)
                {
                    aStr = sVals[i].Trim();
                    v2 += aDir * (hexLn / 2);
                    bPl = (dxePolyline)aPl.Clone();
                    bPl.Flag = (i + 1).ToString();
                    bPl.Move(v2.X, v2.Y);
                    if (!bDontAddToImage) bPl = (dxePolyline)aImage.Entities.Add(bPl);
                    _rVal.Add(bPl);
                    if (aCollector != null) aCollector.Add(bPl);

                    if (!string.IsNullOrWhiteSpace(aStr))
                    {
                        aTxt = etl.Create_Text(v2, aStr, 0.125 * xscal, dxxMTextAlignments.MiddleCenter, lname, "Standard", tclr, aTextAngle: tang, aTextType: aTextType, bSuppressUCS: true);
                        aTxt.Tag = "HEX_TEXT";
                        if (aTextType != dxxTextTypes.Multiline) aTxt.AttributeTag = aTxt.Tag;

                        aTxt.Flag = bPl.Flag;

                        if (!bDontAddToImage) aTxt = (dxeText)aImage.Entities.Add(aTxt);
                        _rVal.Add(aTxt);
                        if (aCollector != null) aCollector.Add(aTxt);
                    }
                    v2 += aDir * (hexLn / 2);
                }
            }

            if (!string.IsNullOrWhiteSpace(aTrailer))
            {
                if (!bTrailerBelow)
                {
                    v2.Project(aDir, 0.09 * xscal);
                }
                else
                {
                    v2 = v1.Clone();
                    if (aDirection == dxxOrthoDirections.Down)
                    {
                        aDir = aImage.UCS.XDirection;
                        talng = dxxMTextAlignments.TopRight;
                    }
                    else if (aDirection == dxxOrthoDirections.Up)
                    {
                        aDir = aImage.UCS.XDirection;
                        talng = dxxMTextAlignments.TopLeft;
                    }
                    else if (aDirection == dxxOrthoDirections.Right)
                    {
                        aDir = aImage.UCS.YDirection.Inverse();
                        talng = dxxMTextAlignments.TopLeft;
                    }
                    else
                    {
                        aDir = aImage.UCS.YDirection.Inverse();
                        talng = dxxMTextAlignments.TopRight;
                    }
                    v2 += aDir * (bubHt / 2 + 0.045 * xscal);
                }

                aTxt = etl.Create_Text(v2, aTrailer, 0.125 * xscal, talng, lname, "Standard", tclr, aTextAngle: tang, aTextType: aTextType, bSuppressUCS: true);
                aTxt.Tag = "TRAILER_TEXT";
                if (aTextType != dxxTextTypes.Multiline) aTxt.AttributeTag = aTxt.Tag;


                if (!bDontAddToImage) aTxt = (dxeText)aImage.Entities.Add(aTxt);

                _rVal.Add(aTxt);
                if (aCollector != null) aCollector.Add(aTxt);

            }

            return _rVal;
        }

        public colDXFEntities CreatePNBubbleStack(colDXFVectors aTaggedVectors, dxfVector aInsertionPt, dxfImage aImage, double aScaleFactor, double aXOffset = 0, double aYOffset = 0, bool bDontAddToImage = false, colDXFEntities aCollector = null)
        {
            colDXFEntities _rVal = new();

            if (aImage == null || aTaggedVectors == null) return _rVal;
            if (aTaggedVectors.Count <= 0) return _rVal;

            dxfVector ip = aInsertionPt == null ? dxfVector.Zero : aInsertionPt.Clone();
            for (int i = 1; i <= aTaggedVectors.Count; i++)
            {
                dxfVector v1 = aTaggedVectors.Item(i);
                if (string.IsNullOrWhiteSpace(v1.Tag)) continue;
                colDXFEntities pillents = CreatePNBubbles(aImage, ip, aScaleFactor, aDirection: dxxOrthoDirections.Right, aPillString: v1.Tag, aHexString: "", aTrailer: "", bTrailerBelow: false, aXOffset: aXOffset, aYOffset: aYOffset, bDontAddToImage: bDontAddToImage, aCollector: aCollector);
                dxePolyline pill = pillents.Polylines()[0];
                _rVal.Append(pillents);
                ip.Move(aYChange: -pill.BoundingRectangle().Height);
            }


            return _rVal;
        }

        public void SetDrawProperties(dxfImage aImage, uopDocDrawing aDrawing, appDrawSettings aSettings = null, uppUnitFamilies aUnits = uppUnitFamilies.Undefined)
        {
            // ^executed prior to all top level drawing functions to set the drawing tool properties
            // ^based on the application settings and the settings passed in the current drawing .
            // " , "application colors (RGB longs) are converted to AutoCAD ACL colors here.

            try
            {
                if (aImage == null) return;

                if (aSettings == null) aSettings = appApplication.DrawSettings; // It was ThisApp.DrawSettins in VB

                aImage.UsingDxfViewer = true;
                aImage.CollectErrors = true;

                bool bNoText = false;

                if (aDrawing != null)
                {
                    bNoText = aDrawing.NoText;
                    if (aUnits == uppUnitFamilies.Undefined) aUnits = aDrawing.DrawingUnits;

                    //aImage.Display.SetDisplayWindow(aDrawing.ExtentWidth, aDrawing.ViewCenter);
                }
                if (aUnits == uppUnitFamilies.Undefined) aUnits = uppUnitFamilies.Metric;

                var ltlsets = aImage.LinetypeLayers;
                ltlsets.Add("Center", "CENTER", aSettings.CenterLineColor); // needs to be exposed as property in aSettings
                ltlsets.Add(dxfLinetypes.Hidden, "HIDDEN", aSettings.HiddenLineColor); // needs to be exposed as property in aSettings
                ltlsets.Add(dxfLinetypes.Phantom, "PHANTOM", dxxColors.LightGrey, dxxLineWeights.LW_015);
                ltlsets.Add("Dashed", "DASHED", aSettings.DottedLineColor); // needs to be exposed as property
                ltlsets.Setting = dxxLinetypeLayerFlag.ForceToLayer;
                if (!bNoText)
                {
                    //aImage.Layout().SetPrinter( "Ton_11-135-8150", "11x17", pltA_Extents, pltR_90CCW, pltS_ScaledToFit, pltF_PIntitializing + pltF_PUpdatePaper + pltF_PModelType + pltF_PDrawViewportsFirst + pltF_PrintLineweights + pltF_PlotPlotStyle + pltF_UseStandardScale + pltF_PlotCentered + pltF_PlotHidden + pltF_ShowPlotStyles, "LaserJets.ctb") // Method does not exist in the dxfObject class

                    //aImage.Styles.AddTextStyle("Standard", "romand.shx", 0.1875, 0.8);
                    //aImage.Styles.AddTextStyle("Standard", "RomanS.shx", 0, 0.8, bMakeCurrent: true, OverrideExisting: true);
                    dxoStyle tstyle;

                    tstyle = aImage.TextStyle("Standard");
                    //tstyle.FontName = "Arial Narrow";

                    dxsHeader hdr = aImage.Header;
                    hdr.TextSize = 0.125;
                    hdr.TextStyleName = tstyle.Name;

                    dxoSettingsDim dimsets = aImage.DimSettings;
                    dimsets.DimLayer = "DIM";
                    dimsets.DimLayerColor = aSettings.DimensionLineColor; // needs to be exposed as property in aSettings
                    dimsets.LeaderLayer = "DIM";
                    dimsets.LeaderLayerColor = dimsets.DimLayerColor;
                    dimsets.DrawingUnits = aUnits == uppUnitFamilies.English ? dxxDrawingUnits.English : dxxDrawingUnits.Metric;

                    dxoSettingsText txtsets = aImage.TextSettings;
                    txtsets.LayerName = "TEXT";
                    txtsets.LayerColor = aSettings.TextColor; // needs to be exposed as property aSettings
                    txtsets.LayerLineWeight = dxxLineWeights.LW_015;

                    dxoSettingsTable tblsets = aImage.TableSettings;
                    tblsets.LayerName = "TABLES";
                    tblsets.LayerColor = aSettings.TextColor;
                    tblsets.TextColor = aImage.TextSettings.LayerColor;
                    tblsets.TextStyleName = tstyle.Name;
                    tblsets.TextSize = 0.125;
                    tblsets.TextGap = 0.125;
                    tblsets.ColumnGap = 0.25;
                    tblsets.BorderColor = dxxColors.BlackWhite;
                    tblsets.GridColor = dxxColors.BlackWhite;
                    dxoSettingsSymbol symsets = aImage.SymbolSettings;
                    symsets.LayerName = "DIM";
                    symsets.LayerColor = dimsets.DimLayerColor;
                    symsets.TextColor = aImage.TextSettings.LayerColor;
                    symsets.TextHeight = 0.125;
                    symsets.TextGap = 0.03;
                    symsets.TextStyleName = tstyle.Name;
                    symsets.AxisStyle = 1;
                    symsets.ArrowStyle = dxxArrowStyles.StraightFull;

                    if (!aImage.ReferenceExists(dxxReferenceTypes.DIMSTYLE, "DIMENSION"))
                    {
                        aImage.DimStyles.Add("DIMENSION", tstyle.Name,bMakeCurrent: true);
                    }

                    dxoDimStyle dstyle = aImage.DimStyle("DIMENSION");

                    dstyle.TextHeight = 0.125;
                    dstyle.SetZeroSuppressionDecimal(false, false, aSettings.LinearPrecision(aUnits), aSettings.AngularPrecision);
                    dstyle.TextColor = aImage.TextSettings.LayerColor;
                    dstyle.ZeroSuppressionAngular = dxxZeroSuppression.LeadingAndTrailing;
                    dstyle.TextStyleName = tstyle.Name;
                    dstyle.SetGapsAndOffsets(0.09, 0.125, 0.09);
                    dstyle.LinearPrecision = aSettings.LinearPrecision(aUnits);
                    dstyle.LinearScaleFactor = (aUnits == uppUnitFamilies.English) ? 1 : 25.4;
                    dstyle.AngularPrecision = aSettings.AngularPrecision;
                    dstyle.ArrowSize = 0.09;
                    dstyle.ExtLineOffset = 0.04;
                    dstyle.Activate();

                    dxoDimStyle stdstyle = aImage.DimStyle("Standard");
                    stdstyle.CopyStyleProperties(dstyle);

                    //aImage.DimStyle("Standard").UpdateToImage(dstyle.Name, aImage: aImage); 
                    dxsDimOverrides dimoverrides = aImage.DimStyleOverrides;
                    dimoverrides.AutoReset = true;
                    dimoverrides.CopyStyleProperties(dstyle);

                    // hdr.SetCurrentReferenceName();

                    aImage.Layers.Add("TEXT", aImage.TextSettings.LayerColor, aLineWeight: dxxLineWeights.LW_015, bOverrideExisting: true);
                    aImage.Layers.Add("HIDDEN", dxxColors.Green, dxfLinetypes.Hidden, aLineWeight: dxxLineWeights.LW_015, bOverrideExisting: true);
                }

                //DoEvents // I don't know what it does
            }
            catch (Exception e)
            {
                { aImage.HandleError(System.Reflection.MethodBase.GetCurrentMethod(), this.GetType(), e); }
            }
        }


        // The last two arguments are optional in VB but it is not possible here
        public bool DecodeScaleString(dynamic aScale, out double rScale, out string rScaleString)
        {
            bool decodeScaleString = false;

            string aStr;
            rScale = 0;
            rScaleString = string.Empty;

            string[] sVals;
            dynamic numer;
            dynamic denom;


            try
            {
                aStr = aScale.ToString().Replace(" ", "").Trim();
                if (string.IsNullOrWhiteSpace(aStr))
                {
                    return decodeScaleString;
                }

                if (aStr.Contains(":"))
                {
                    sVals = aStr.Split(new char[] { ':' });
                    numer = sVals[0].Trim();
                    denom = sVals[1].Trim();
                    if (!double.TryParse(numer.ToString(), out double s1))
                    {
                        return decodeScaleString;
                    }
                    if (!double.TryParse(denom.ToString(), out double s2))
                    {
                        return decodeScaleString;
                    }

                    s1 = Math.Abs(s1);
                    s2 = Math.Abs(s2);

                    if (s1 == 0 || s2 == 0)
                    {
                        return decodeScaleString;
                    }

                    decodeScaleString = true;
                    rScale = s1 / s2;
                    rScaleString = $"{s1}:{s2}";
                }
            }
            catch (Exception)
            {
                decodeScaleString = false;
            }

            return decodeScaleString;
        }

        // I had to change aProject argument type from "iuopProject" to "uopProject" because it does not exist
        public dxfAttributes BorderAttributes(string aTitle1 = "", string aTitle2 = "", string aTitle3 = "", string aScale = "", uopProject aProject = null, string aSheetNo = "", string aDwgNo = "")
        {
            dxfAttributes _rVal = new();

            _rVal.AddTagValue("TITLE1", aTitle1);
            _rVal.AddTagValue("TITLE2", aTitle2);
            _rVal.AddTagValue("TITLE3", aTitle3);
            _rVal.AddTagValue("SCALE", aScale);
            _rVal.AddTagValue("BYDATE", $"{appApplication.User.Initials.ToUpper()}-{DateTime.Now:MM/dd/yy}"); // it was "goUser.Initials" in VB
            _rVal.AddTagValue("CHECKEDDATE", "");
            _rVal.AddTagValue("REVIEWEDDATE", "");
            _rVal.AddTagValue("APPROVEDDATE", "");
            _rVal.AddTagValue("NOOFSHEETS", "");
            _rVal.AddTagValue("SHEETNO", aSheetNo);
            _rVal.AddTagValue("REVISIONNO", "0");
            _rVal.AddTagValue("DRAWINGNUMBER", aDwgNo);

            if (aProject == null)
            {
                _rVal.AddTagValue("CUSTOMER", "");
                _rVal.AddTagValue("LOCATION", "");
                _rVal.AddTagValue("ITEMNO", "");
                _rVal.AddTagValue("PONO", "");
            }
            else
            {
                _rVal.AddTagValue("CUSTOMER", aProject.Customer.Name.ToUpper());
                _rVal.AddTagValue("LOCATION", aProject.Customer.Location.ToUpper());
                _rVal.AddTagValue("ITEMNO", aProject.Customer.Item.ToUpper());
                _rVal.AddTagValue("PONO", aProject.Customer.PO.ToUpper());
            }

            return _rVal;
        }

        // I had to use "uopProject" and "uopTrayAssembly" instead of "iuopProject" and "iuopTrayAssembly" because they do not exist
        public dxfBlock CreateBlock(string aBlockName, dxfImage aImage, uopProject aProject = null, uopTrayAssembly aAssy = null, uopDocDrawing aDrawing = null, double aValue1 = 0, double aValue2 = 0,  dxfAttributes aAttribs = null, bool bRegen = false, bool bAttribsToText = false, bool bCrossOut = false)
        {
            if (aImage == null || string.IsNullOrWhiteSpace(aBlockName)) return null;
            aBlockName = aBlockName.ToUpper();

            mdTrayAssembly mdAssy = (aAssy != null && aAssy.ProjectFamily == uppProjectFamilies.uopFamMD) ? (mdTrayAssembly)aAssy : null;
            string wuz;

            dxfBlock createBlock = null;

            wuz = Status;
            try
            {

                switch (aBlockName)
                {
                    case "DBORDER":
                        createBlock = CreateBlock_BorderD(aImage, aProject, aAttribs, bRegen);
                        break;
                    case "BBORDER":
                        createBlock = CreateBlock_BorderB(aImage, aProject, aAttribs, bRegen);
                        break;
                    case "DREVISIONS":
                        createBlock = CreateBlock_RevisionsD(aImage);
                        break;
                    case "BREVISIONS":
                        createBlock = CreateBlock_RevisionsB(aImage);
                        break;
                    case "SYMBOL_KEY":
                        createBlock = CreateBlock_SymbolKey(aBlockName, aImage);
                        break;
                    case "BP_DETAIL":
                        createBlock = CreateBlock_BubblePromoter(aBlockName, aImage, aDrawing);
                        break;
                    case "DC_XSECTION":
                        createBlock = CreateBlock_MDDCCrossSection(aBlockName, aImage, mdAssy, aDrawing);
                        break;
                    case "TRAY_MATERIAL":
                        createBlock = CreateBlock_MDMaterials(aBlockName, aImage, mdAssy, aDrawing);
                        break;
                    case "DC_ELEVATION":
                        createBlock = CreateBlock_MDDCElevation(aBlockName, aImage, mdAssy, aDrawing);
                        break;
                    case "BEAM_DETAIL":
                        createBlock = CreateBlock_BeamDetail(aBlockName, aImage, mdAssy, aDrawing);
                        break;
                    case "MDFUNCTIONAL_TABLE":
                        createBlock = CreateBlock_MDTable_Functional(aBlockName, aImage, mdAssy, aDrawing, aValue1, aValue2);
                        break;
                    case "STARTUP_TABLE":
                        createBlock = CreateBlock_MDTable_Startup(aBlockName, aImage, mdAssy, aDrawing);
                        break;
                    case "DC_TABLE":
                        createBlock = CreateBlock_MDTable_DCTable(aBlockName, aImage, mdAssy, aDrawing);
                        break;
                    case "DESIGN_DATA":
                        createBlock = CreateBlock_MDTable_DesignData(aBlockName, aImage, mdAssy, aDrawing, aValue1, aValue2);
                        break;
                    case "SPOUT_TABLE":
                        createBlock = CreateBlock_MDTable_SpoutTable(aBlockName, aImage, mdAssy, aDrawing);
                        break;
                    case "ECMD_DESIGN_DATA":
                        createBlock = CreateBlock_MDTable_ECMDData(aBlockName, aImage, mdAssy, aDrawing, aValue1, aValue2, bCrossOut);
                        break;
                    case "AP_DESIGN":
                        createBlock = CreateBlock_MDTable_APData(aBlockName, aImage, mdAssy, aDrawing, bCrossOut);
                        break;
                    case "FED":
                        createBlock = CreateBlock_MDTable_FEDData(aBlockName, aImage, mdAssy, aDrawing);
                        break;
                    case "BLOCKED_AREA":
                        createBlock = CreateBlock_MDTable_BlockedAreas(aBlockName, aImage, mdAssy, aDrawing);
                        break;
                    case "DCNOTES":
                        createBlock = CreateBlock_MDTable_DCNotes(aBlockName, aImage, mdAssy, aDrawing);
                        break;
                    default:
                        {
                            //empty block place holder for blocks defined by file
                            createBlock = new dxfBlock(aBlockName);

                            break;
                        }
                }
            }
            catch (Exception e)
            {
                { aImage.HandleError(System.Reflection.MethodBase.GetCurrentMethod(), this.GetType(), e); }

                createBlock = null;
            }
            finally
            {
                if (bAttribsToText && createBlock != null)
                {
                    createBlock.Entities.ConvertText(dxxTextTypes.AttDef, dxxTextTypes.Multiline);

                }

                Status = wuz; // It was "frmMain.WorkSubStatus = wuz" in VB

            }



            return createBlock;
        }

        public void UpdateBorderAttribs(dxfImage aImage, uppBorderSizes aBorderSize, dxfBlock aBorderBlock, dxfAttributes aAttribs)
        {
            if (aBorderBlock == null || aImage == null || aAttribs == null)
            {
                return;
            }

            dxfEntity aEnt; // In VB it was idxfEntity
            dxeText aTxt;
            double lim;
            string tg;
            string txt;
            double lng;

            try
            {
                Status = (aBorderSize == uppBorderSizes.DSize_Landscape) ? "Updating D-Size Border Attributes" : "Updating B-Size Border Attributes";


                //aBorderBlock.ImageGUID = aImage.GUID; // the property is readonly
                for (int i = 1; i <= aBorderBlock.Entities.Count; i++)
                {
                    aEnt = aBorderBlock.Entities.Item(i);
                    if (aEnt.EntityType == dxxEntityTypes.Attdef)
                    {
                        aTxt = (dxeText)aEnt;
                        tg = aTxt.AttributeTag;
                        aTxt.TextString = aAttribs.GetValue(tg);
                        txt = aTxt.TextString;
                        lim = 0;
                        if (aBorderSize == uppBorderSizes.DSize_Landscape)
                        {
                            switch (tg)
                            {
                                case "DRAWING#":
                                    lim = 1.35;
                                    break;
                                case "BY-DATE":
                                case "APPROVED-DATE":
                                case "REVIEWED-DATE":
                                case "CHECKED-DATE":
                                    lim = 0.75;
                                    break;
                                case "REV#":
                                    lim = 0.25;
                                    break;
                                case "SCALE":
                                    lim = 0.95;
                                    break;
                                case "TITLE1":
                                case "TITLE2":
                                case "TITLE3":
                                    lim = 1.85;
                                    break;
                                case "LOCATION":
                                case "CUSTOMER":
                                case "P.O. NO.":
                                case "ITEM NO.":
                                    lim = 2.25;
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            switch (tg)
                            {
                                case "DRAWING#":
                                    lim = 1.25;
                                    break;
                                case "BY-DATE":
                                    lim = 0.5;
                                    break;
                                case "APPROVED-DATE":
                                case "REVIEWED-DATE":
                                case "CHECKED-DATE":
                                    lim = 0.5;
                                    break;
                                case "REV#":
                                    lim = 0.25;
                                    break;
                                case "SCALE":
                                    lim = 0.95;
                                    break;
                                case "TITLE1":
                                case "TITLE2":
                                case "TITLE3":
                                    lim = 1.85;
                                    break;
                                case "LOCATION":
                                case "CUSTOMER":
                                case "P.O. NO.":
                                case "ITEM NO.":
                                    lim = 2.25;
                                    break;
                                default:
                                    break;
                            }
                        }

                        if (lim > 0 && !string.IsNullOrWhiteSpace(txt))
                        {
                            lng = aTxt.BoundingRectangle().Width;
                            if (lng > lim)
                            {
                                aTxt.WidthFactor = lim / lng;
                            }
                            aTxt.TextString = string.Empty;
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        // I had to use "uopProject" instead of "iuopProject"
        private dxfBlock CreateBlock_BorderD(dxfImage aImage, uopProject aProject, dxfAttributes aAttribs = null, bool bRegen = false)
        {
            Status = "Retrieving D-Size Border";


            double x0;
            double X1;
            double x2;
            double x3;
            double x4;
            string txt;
            double y1;
            double stp;
            string tg;
            dxoEntityTool tool = aImage.EntityTool;
            dxeText aTxt;

            dxfBlock _rVal;
            //aAttribs ??= BorderAttributes(aProject: aProject);
            stp = 0.2;

            aImage.Layers.Add("BORDER", dxxColors.BlackWhite, dxfLinetypes.Continuous);
            aImage.Layers.Add("TEXT", dxxColors.Cyan, dxfLinetypes.Continuous, aLineWeight: dxxLineWeights.LW_015, bOverrideExisting: true);

            //aImage.Styles.AddTextStyle("Standard", "RomanS.shx", 0, 0.8, OverrideExisting: true);
            //aImage.Styles.AddTextStyle("Standard", "romand.shx", 0, 0.8, OverrideExisting: true);

            if (oDBorder == null || bRegen)
            {
                Status = "Creating D-Size Border";

                oDBorder = new dxfBlock();
                X1 = 28.4934;
                x2 = 32.4819;
                x3 = 33.6773;
                x4 = 34.3954;

                oDBorder.Name = "BORDER";
                oDBorder.ImageGUID = aImage.GUID;
                dxfDisplaySettings dsp = new("BORDER", dxxColors.ByLayer, dxfLinetypes.Continuous);
                oDBorder.Entities.AddPolyline("(0,22.8899)¸(35,22.8899)¸(35,0)¸(0,0)", true, dsp, 0.02);
                oDBorder.Entities.AddPolyline("(35,4.5019)¸(28.4934,4.5019)¸(28.4934,0)", false, dsp, 0.02);
                oDBorder.Entities.AddPolyline("(35,3.1996)¸(28.4934,3.1996)", false, dsp, 0.02);
                oDBorder.Entities.AddPolyline("(28.4934,1.1444)¸(32.4819,1.1444)¸(32.4819,0)", false, dsp, 0.02);
                oDBorder.Entities.AddLine(X1, 4.2391, 35, 4.2391, dsp);
                oDBorder.Entities.AddLine(X1, 2.1419, x2, 2.1419, dsp);
                oDBorder.Entities.AddLine(x2, 1.8775, 35, 1.8775, dsp);
                oDBorder.Entities.AddLine(x2, 1.5073, 35, 1.5073, dsp);
                oDBorder.Entities.AddLine(x2, 1.1444, x3, 1.1444, dsp);
                oDBorder.Entities.AddLine(x2, 0.7742, x3, 0.7742, dsp);
                oDBorder.Entities.AddLine(x2, 0.404, 35, 0.404, dsp);
                oDBorder.Entities.AddLine(x3, 0.954, x4, 0.954, dsp);
                oDBorder.Entities.AddLine(x3, 1.8775, x3, 0.404, dsp);
                oDBorder.Entities.AddLine(x4, 1.5073, x4, 0.404, dsp);
                oDBorder.Entities.AddLine(x2, 3.1996, x2, 0, dsp);

                txt = "CONFIDENTIAL";
                oDBorder.Entities.Add(tool.Create_Text(new dxfVector(31.747, 4.3705), txt, 0.125, dxxMTextAlignments.MiddleCenter, "TEXT", "Standard"));

                y1 = 4.1;
                txt = "{\\W0.9268;THE INFORMATION CONTAINED HEREIN IS PROPRIETARY INFORMATION}";
                txt += $"\\P{{\\W0.8478;AND SHALL NOT BE COPIED AND/OR REPRODUCED IN ANY MANNER, NOR}}";
                txt += $"\\P{{\\W0.8771;USED FOR ANY PURPOSE WHATSOEVER EXCEPT AS SPECIFICALLY PER-}}";
                txt += $"\\P{{\\W0.8;MITTED PURSUANT TO WRITTEN AGREEMENT WITH UOP.}}";
                oDBorder.Entities.Add(tool.Create_Text(new dxfVector(X1 + 0.25, y1), txt, 0.125, dxxMTextAlignments.TopLeft, "TEXT", "Standard"));

                y1 = 3.05;
                stp = 0.25;
                x0 = 29.4;

                txt = "CUSTOMER";
                oDBorder.Entities.Add(tool.Create_Text(new dxfVector(X1 + 0.0825, y1), txt, 0.125, dxxMTextAlignments.MiddleLeft, "TEXT", "Standard", aWidthFactor: 0.8, aTextType: dxxTextTypes.DText));
                oDBorder.Entities.Add(tool.Create_Text(new dxfVector(x0, y1), ":", 0.125, dxxMTextAlignments.MiddleCenter, "TEXT", "Standard", aWidthFactor: 0.8, aTextType: dxxTextTypes.DText));
                tg = "Customer";
                txt = string.Empty;
                oDBorder.Entities.Add(tool.Create_Text(new dxfVector(x0 + 0.0825, y1), txt, 0.125, dxxMTextAlignments.MiddleLeft, "TEXT", "Standard", aWidthFactor: 0.8, aTextType: dxxTextTypes.AttDef, aAttributeTag: tg.ToUpper(), aAttributePrompt: tg));

                y1 -= stp;
                txt = "LOCATION";
                oDBorder.Entities.Add(tool.Create_Text(new dxfVector(X1 + 0.0825, y1), txt, 0.125, dxxMTextAlignments.MiddleLeft, "TEXT", "Standard", aWidthFactor: 0.8, aTextType: dxxTextTypes.DText));
                oDBorder.Entities.Add(tool.Create_Text(new dxfVector(x0, y1), ":", 0.125, dxxMTextAlignments.MiddleCenter, "TEXT", "Standard", aWidthFactor: 0.8, aTextType: dxxTextTypes.DText));
                tg = "Location";
                txt = string.Empty;
                oDBorder.Entities.Add(tool.Create_Text(new dxfVector(x0 + 0.0825, y1), txt, 0.125, dxxMTextAlignments.MiddleLeft, "TEXT", "Standard", aWidthFactor: 0.8, aTextType: dxxTextTypes.AttDef, aAttributeTag: tg.ToUpper(), aAttributePrompt: tg));

                y1 -= stp;
                txt = "ITEM NO.";
                oDBorder.Entities.Add(tool.Create_Text(new dxfVector(X1 + 0.0825, y1), txt, 0.125, dxxMTextAlignments.MiddleLeft, "TEXT", "Standard", aWidthFactor: 0.8, aTextType: dxxTextTypes.DText));
                oDBorder.Entities.Add(tool.Create_Text(new dxfVector(x0, y1), ":", 0.125, dxxMTextAlignments.MiddleCenter, "TEXT", "Standard", aWidthFactor: 0.8, aTextType: dxxTextTypes.DText));
                tg = "ITEM NO.";
                txt = string.Empty;
                oDBorder.Entities.Add(tool.Create_Text(new dxfVector(x0 + 0.0825, y1), txt, 0.125, dxxMTextAlignments.MiddleLeft, "TEXT", "Standard", aWidthFactor: 0.8, aTextType: dxxTextTypes.AttDef, aAttributeTag: tg.ToUpper(), aAttributePrompt: "Item No."));
                y1 -= stp;
                txt = "P.O. NO.";
                oDBorder.Entities.Add(tool.Create_Text(new dxfVector(X1 + 0.0825, y1), txt, 0.125, dxxMTextAlignments.MiddleLeft, "TEXT", "Standard", aWidthFactor: 0.8, aTextType: dxxTextTypes.DText));
                oDBorder.Entities.Add(tool.Create_Text(new dxfVector(x0, y1), ":", 0.125, dxxMTextAlignments.MiddleCenter, "TEXT", "Standard", aWidthFactor: 0.8, aTextType: dxxTextTypes.DText));
                tg = "P.O. NO.";
                txt = string.Empty;
                oDBorder.Entities.Add(tool.Create_Text(new dxfVector(x0 + 0.0825, y1), txt, 0.125, dxxMTextAlignments.MiddleLeft, "TEXT", "Standard", aWidthFactor: 0.8, aTextType: dxxTextTypes.AttDef, aAttributeTag: tg.ToUpper(), aAttributePrompt: "P.O. No."));

                y1 = 1.925;
                stp = 0.25;
                txt = "{\\W1.0117;DIRECT INQUIRIES PERTAINING TO THIS}";
                txt += $"\\P{{\\W0.9302;DRAWING AND ASSOCIATED EQUIPMENT TO}}";
                txt += $"\\P{{\\W0.8;UOP TONAWANDA}}";

                oDBorder.Entities.Add(tool.Create_Text(new dxfVector(X1 + 0.125, y1), txt, 0.125, dxxMTextAlignments.TopLeft, "TEXT", "Standard"));

                txt = "175 EAST PARK DRIVE";
                txt += $"\\PTONAWANDA, NY 14151-0986";
                txt += $"\\PTEL: (716)-879-2734";
                txt += $"\\PFAX: (716)-879-7215";
                oDBorder.Entities.Add(tool.Create_Text(new dxfVector(32.2736, 0.675), txt, 0.09, dxxMTextAlignments.TopRight, "TEXT", "Standard"));

                oDBorder.Entities.Append(UOPLogo(new dxfVector(29.3463, 0.7116)));

                y1 = 3.1996;
                txt = "TITLE:";
                oDBorder.Entities.Add(tool.Create_Text(new dxfVector(x2 + 0.08, y1 - 0.08), txt, 0.125, dxxMTextAlignments.TopLeft, "TEXT", "Standard"));
                y1 = 2.6829;
                x0 = 33.7295;
                tg = "Title1";
                txt = string.Empty;
                oDBorder.Entities.Add(tool.Create_Text(new dxfVector(x0, y1), txt, 0.125, dxxMTextAlignments.MiddleCenter, "TEXT", "Standard", aWidthFactor: 0.8, aTextType: dxxTextTypes.AttDef, aAttributeTag: tg.ToUpper(), aAttributePrompt: "Title, Line 1"));

                y1 -= 0.3;
                tg = "Title2";
                txt = string.Empty;
                oDBorder.Entities.Add(tool.Create_Text(new dxfVector(x0, y1), txt, 0.125, dxxMTextAlignments.MiddleCenter, "TEXT", "Standard", aWidthFactor: 0.8, aTextType: dxxTextTypes.AttDef, aAttributeTag: tg.ToUpper(), aAttributePrompt: "Title, Line 2"));

                y1 -= 0.3;
                tg = "Title3";
                txt = string.Empty;
                oDBorder.Entities.Add(tool.Create_Text(new dxfVector(x0, y1), txt, 0.125, dxxMTextAlignments.MiddleCenter, "TEXT", "Standard", aWidthFactor: 0.8, aTextType: dxxTextTypes.AttDef, aAttributeTag: tg.ToUpper(), aAttributePrompt: "Title, Line 3"));

                y1 = 1.8775;
                txt = "BY";
                oDBorder.Entities.Add(tool.Create_Text(new dxfVector(x2 + 0.0625, y1 - 0.0325), txt, 0.0675, dxxMTextAlignments.TopLeft, "TEXT", "Standard"));

                txt = "DATE";
                oDBorder.Entities.Add(tool.Create_Text(new dxfVector(x2 + 0.75, y1 - 0.0325), txt, 0.0675, dxxMTextAlignments.TopLeft, "TEXT", "Standard"));
                tg = "BY-DATE";
                txt = string.Empty;
                oDBorder.Entities.Add(tool.Create_Text(new dxfVector(33.0796, y1 - 0.235), txt, 0.125, dxxMTextAlignments.MiddleCenter, "TEXT", "Standard", aWidthFactor: 0.8, aTextType: dxxTextTypes.AttDef, aAttributeTag: tg.ToUpper(), aAttributePrompt: "By and Date"));

                txt = "SCALE";
                oDBorder.Entities.Add(tool.Create_Text(new dxfVector(x3 + 0.0625, y1 - 0.0325), txt, 0.0675, dxxMTextAlignments.TopLeft, "TEXT", "Standard"));
                tg = "SCALE";
                txt = string.Empty;
                oDBorder.Entities.Add(tool.Create_Text(new dxfVector(34.3386, 1.6292), txt, 0.125, dxxMTextAlignments.MiddleCenter, "TEXT", "Standard", aWidthFactor: 0.8, aTextType: dxxTextTypes.AttDef, aAttributeTag: tg.ToUpper(), aAttributePrompt: "Scale"));

                y1 = 1.5073;
                txt = "CHECKED";
                oDBorder.Entities.Add(tool.Create_Text(new dxfVector(x2 + 0.0625, y1 - 0.0325), txt, 0.0675, dxxMTextAlignments.TopLeft, "TEXT", "Standard"));
                tg = "CHECKED-DATE";
                txt = string.Empty;
                oDBorder.Entities.Add(tool.Create_Text(new dxfVector(33.0796, y1 - 0.235), txt, 0.125, dxxMTextAlignments.MiddleCenter, "TEXT", "Standard", aWidthFactor: 0.8, aTextType: dxxTextTypes.AttDef, aAttributeTag: tg.ToUpper(), aAttributePrompt: "Checked and Date"));

                txt = "NO. SHTS.";
                oDBorder.Entities.Add(tool.Create_Text(new dxfVector(x3 + 0.0625, y1 - 0.0325), txt, 0.0675, dxxMTextAlignments.TopLeft, "TEXT", "Standard"));
                tg = "#SHEETS";
                txt = string.Empty;
                oDBorder.Entities.Add(tool.Create_Text(new dxfVector(34.0364, 1.1554), txt, 0.125, dxxMTextAlignments.MiddleCenter, "TEXT", "Standard", aWidthFactor: 0.8, aTextType: dxxTextTypes.AttDef, aAttributeTag: tg.ToUpper(), aAttributePrompt: "Number of Sheets"));

                txt = "LATEST\\PREVISION";
                oDBorder.Entities.Add(tool.Create_Text(new dxfVector(x4 + 0.0625, y1 - 0.0325), txt, 0.0675, dxxMTextAlignments.TopLeft, "TEXT", "Standard"));
                tg = "REV#";
                txt = string.Empty;
                oDBorder.Entities.Add(tool.Create_Text(new dxfVector(34.6863, 0.8021), txt, 0.25, dxxMTextAlignments.MiddleCenter, "TEXT", "Standard", aWidthFactor: 0.8, aTextType: dxxTextTypes.AttDef, aAttributeTag: tg.ToUpper(), aAttributePrompt: "Latest Alteration Number")); // is the 0.25 a typo?

                y1 = 1.1444;
                txt = "REVIEWED";
                oDBorder.Entities.Add(tool.Create_Text(new dxfVector(x2 + 0.0625, y1 - 0.0325), txt, 0.0675, dxxMTextAlignments.TopLeft, "TEXT", "Standard"));
                tg = "REVIEWED-DATE";
                txt = string.Empty;
                oDBorder.Entities.Add(tool.Create_Text(new dxfVector(33.0796, y1 - 0.235), txt, 0.125, dxxMTextAlignments.MiddleCenter, "TEXT", "Standard", aWidthFactor: 0.8, aTextType: dxxTextTypes.AttDef, aAttributeTag: tg.ToUpper(), aAttributePrompt: "Reviewed and Date"));

                y1 = 0.954;
                txt = "SHT. NO.";
                oDBorder.Entities.Add(tool.Create_Text(new dxfVector(x3 + 0.0625, y1 - 0.0325), txt, 0.0675, dxxMTextAlignments.TopLeft, "TEXT", "Standard"));
                tg = "SHEETS#";
                txt = string.Empty;
                oDBorder.Entities.Add(tool.Create_Text(new dxfVector(34.0364, 0.6043), txt, 0.125, dxxMTextAlignments.MiddleCenter, "TEXT", "Standard", aWidthFactor: 0.8, aTextType: dxxTextTypes.AttDef, aAttributeTag: tg.ToUpper(), aAttributePrompt: "Sheet Number"));

                y1 = 0.7742;
                txt = "APPROVED";
                oDBorder.Entities.Add(tool.Create_Text(new dxfVector(x2 + 0.0625, y1 - 0.0325), txt, 0.0675, dxxMTextAlignments.TopLeft, "TEXT", "Standard"));
                tg = "APPROVED-DATE";
                txt = string.Empty;
                oDBorder.Entities.Add(tool.Create_Text(new dxfVector(33.0796, y1 - 0.235), txt, 0.125, dxxMTextAlignments.MiddleCenter, "TEXT", "Standard", aWidthFactor: 0.8, aTextType: dxxTextTypes.AttDef, aAttributeTag: tg.ToUpper(), aAttributePrompt: "Approved and Date"));

                y1 = 0.404;
                txt = "DWG.\\PNO.";
                oDBorder.Entities.Add(tool.Create_Text(new dxfVector(x2 + 0.0625, y1 - 0.0325), txt, 0.0675, dxxMTextAlignments.TopLeft, "TEXT", "Standard"));

                y1 = 0.185;
                x0 = x2 + 0.3;
                txt = "D-";
                aTxt = (dxeText)oDBorder.Entities.Add(tool.Create_Text(new dxfVector(x0, y1), txt, 0.25, dxxMTextAlignments.MiddleLeft, "TEXT", "Standard"));

                x0 = x0 + aTxt.BoundingRectangle().Width + 0.0625;
                tg = "DRAWING#";
                txt = string.Empty;
                oDBorder.Entities.Add(tool.Create_Text(new dxfVector(x0, y1), txt, 0.25, dxxMTextAlignments.MiddleLeft, "TEXT", "Standard", aWidthFactor: 0.8, aTextType: dxxTextTypes.AttDef, aAttributeTag: tg.ToUpper(), aAttributePrompt: "Drawing Number D-"));

                _rVal = oDBorder.Clone();
            }
            else
            {
                Status = "Copying D-Size Border";

                _rVal = oDBorder.Clone();
            }

            return _rVal;
        }

        // I had to use "uopProject" instead of "iuopProject" because it does not exist
        private dxfBlock CreateBlock_BorderB(dxfImage aImage, uopProject aProject, dxfAttributes aAttribs = null, bool bRegen = false)
        {
            Status = "Retrieving B-Size Border";

            double x0;
            double X1;
            double x2;
            double x3;
            double x4;
            string txt;
            double y1;
            string tg;
            dxeText aTxt;

            double[] yvals = new double[7];

            dxfBlock _rVal;

            // aAttribs ??= BorderAttributes(aProject: aProject);


            aImage.Layers.Add("BORDER", dxxColors.BlackWhite, dxfLinetypes.Continuous);
            aImage.Layers.Add("TEXT", dxxColors.Cyan, dxfLinetypes.Continuous, aLineWeight: dxxLineWeights.LW_015, bOverrideExisting: true);

            //aImage.Styles.AddTextStyle("Standard", "RomanS.shx", 0, 0.8, OverrideExisting: true);
            //aImage.Styles.AddTextStyle("Standard", "romand.shx", 0, 0.8, OverrideExisting: true);

            if (oBBorder == null || bRegen)
            {
                Status = "Creating B-Size Border";
                oBBorder = new dxfBlock();
                X1 = 11.4588;
                x2 = 13.9788;
                x3 = 14.9688;
                x4 = 15.4688;
                yvals[0] = 2.0413;
                yvals[1] = 1.0413;
                yvals[2] = 1.6242;
                yvals[3] = 1.239;
                yvals[4] = 0.8539;
                yvals[5] = 0.4688;
                yvals[6] = 1.0465;
                dxoEntityTool tool = aImage.EntityTool;
                oBBorder.Name = "BORDER";
                //oBBorder.ImageGUID = aImage.GUID; // the property is readonly
                dxfDisplaySettings dsp = new("BORDER", dxxColors.ByLayer, dxfLinetypes.Continuous);

                oBBorder.Entities.AddPolyline("(0,10.0)¸(16,10.0)¸(16,0)¸(0,0)", true, dsp, 0.03);

                oBBorder.Entities.AddPolyline($"(16,{yvals[0]})¸({X1},{yvals[0]})¸({X1},0)", false, dsp, 0.03);
                oBBorder.Entities.AddPolyline($"({X1},{yvals[1]})¸({x2},{yvals[1]})", false, dsp, 0.02);
                oBBorder.Entities.AddPolyline($"({x2},{yvals[0]})¸({x2},0)", false, dsp, 0.02);

                oBBorder.Entities.AddLine(x2, yvals[2], 16, yvals[2], dsp);
                oBBorder.Entities.AddLine(x2, yvals[3], x3, yvals[3], dsp);
                oBBorder.Entities.AddLine(x2, yvals[4], x3, yvals[4], dsp);
                oBBorder.Entities.AddLine(x2, yvals[5], 16, yvals[5], dsp);
                oBBorder.Entities.AddLine(x3, yvals[6], x4, yvals[6], dsp);
                oBBorder.Entities.AddLine(x3, yvals[0], x3, yvals[5], dsp);
                oBBorder.Entities.AddLine(x4, yvals[2], x4, yvals[5], dsp);

                txt = "TITLE:";
                oBBorder.Entities.Add(tool.Create_Text(new dxfVector(X1 + 0.08, yvals[0] - 0.08), txt, 0.09, dxxMTextAlignments.TopLeft, "TEXT", "Standard"));

                y1 = 1.7209;
                x0 = X1 + (x2 - X1) / 2;
                tg = "Title1";
                txt = string.Empty;
                oBBorder.Entities.Add(tool.Create_Text(new dxfVector(x0, y1), txt, 0.125, dxxMTextAlignments.MiddleCenter, "TEXT", "Standard", aWidthFactor: 0.8, aTextType: dxxTextTypes.AttDef, aAttributeTag: tg.ToUpper(), aAttributePrompt: "Title, Line 1"));

                y1 -= 0.3;
                tg = "Title2";
                txt = string.Empty;
                oBBorder.Entities.Add(tool.Create_Text(new dxfVector(x0, y1), txt, 0.125, dxxMTextAlignments.MiddleCenter, "TEXT", "Standard", aWidthFactor: 0.8, aTextType: dxxTextTypes.AttDef, aAttributeTag: tg.ToUpper(), aAttributePrompt: "Title, Line 2"));

                y1 -= 0.3;
                tg = "Title3";
                txt = string.Empty;
                oBBorder.Entities.Add(tool.Create_Text(new dxfVector(x0, y1), txt, 0.125, dxxMTextAlignments.MiddleCenter, "TEXT", "Standard", aWidthFactor: 0.8, aTextType: dxxTextTypes.AttDef, aAttributeTag: tg.ToUpper(), aAttributePrompt: "Title, Line 3"));

                oBBorder.Entities.Append(UOPLogo(new dxfVector(12.0034, 0.8285), 0.65));

                txt = "175 EAST PARK DRIVE";
                txt += "\\PTONAWANDA, NEW YORK";
                oBBorder.Entities.Add(tool.Create_Text(new dxfVector(11.5988, 0.365), txt, 0.08, dxxMTextAlignments.TopLeft, "TEXT", "Standard"));

                y1 = yvals[0];
                X1 = x2 + (x3 - x2) / 2;
                txt = "BY";
                oBBorder.Entities.Add(tool.Create_Text(new dxfVector(x2 + 0.0625, y1 - 0.0325), txt, 0.0675, dxxMTextAlignments.TopLeft, "TEXT", "Standard"));

                txt = "DATE";
                oBBorder.Entities.Add(tool.Create_Text(new dxfVector(x2 + 0.75, y1 - 0.0325), txt, 0.0675, dxxMTextAlignments.TopLeft, "TEXT", "Standard"));
                tg = "BY-DATE";
                txt = string.Empty;
                oBBorder.Entities.Add(tool.Create_Text(new dxfVector(X1, y1 - 0.265), txt, 0.125, dxxMTextAlignments.MiddleCenter, "TEXT", "Standard", aWidthFactor: 0.8, aTextType: dxxTextTypes.AttDef, aAttributeTag: tg.ToUpper(), aAttributePrompt: "By and Date"));

                y1 = yvals[2];
                txt = "CHECKED";
                oBBorder.Entities.Add(tool.Create_Text(new dxfVector(x2 + 0.0625, y1 - 0.0325), txt, 0.0675, dxxMTextAlignments.TopLeft, "TEXT", "Standard"));
                tg = "BY-CHECKED";
                txt = string.Empty;
                oBBorder.Entities.Add(tool.Create_Text(new dxfVector(X1, y1 - 0.235), txt, 0.125, dxxMTextAlignments.MiddleCenter, "TEXT", "Standard", aWidthFactor: 0.8, aTextType: dxxTextTypes.AttDef, aAttributeTag: tg.ToUpper(), aAttributePrompt: "Checked and Date"));

                y1 = yvals[3];
                txt = "REVIEWED";
                oBBorder.Entities.Add(tool.Create_Text(new dxfVector(x2 + 0.0625, y1 - 0.0325), txt, 0.0675, dxxMTextAlignments.TopLeft, "TEXT", "Standard"));
                tg = "REVIEWED-DATE";
                txt = string.Empty;
                oBBorder.Entities.Add(tool.Create_Text(new dxfVector(X1, y1 - 0.235), txt, 0.125, dxxMTextAlignments.MiddleCenter, "TEXT", "Standard", aWidthFactor: 0.8, aTextType: dxxTextTypes.AttDef, aAttributeTag: tg.ToUpper(), aAttributePrompt: "Reviewed and Date"));

                y1 = yvals[4];
                txt = "APPROVED";
                oBBorder.Entities.Add(tool.Create_Text(new dxfVector(x2 + 0.0625, y1 - 0.0325), txt, 0.0675, dxxMTextAlignments.TopLeft, "TEXT", "Standard"));
                tg = "APPROVED-DATE";
                txt = string.Empty;
                oBBorder.Entities.Add(tool.Create_Text(new dxfVector(X1, y1 - 0.235), txt, 0.125, dxxMTextAlignments.MiddleCenter, "TEXT", "Standard", aWidthFactor: 0.8, aTextType: dxxTextTypes.AttDef, aAttributeTag: tg.ToUpper(), aAttributePrompt: "Approved and Date"));

                y1 = yvals[0];
                X1 = x3 + (16 - x3) / 2;
                txt = "SCALE";
                oBBorder.Entities.Add(tool.Create_Text(new dxfVector(x3 + 0.0425, y1 - 0.0325), txt, 0.0675, dxxMTextAlignments.TopLeft, "TEXT", "Standard"));
                tg = "SCALE";
                txt = string.Empty;
                oBBorder.Entities.Add(tool.Create_Text(new dxfVector(X1, y1 - 0.265), txt, 0.125, dxxMTextAlignments.MiddleCenter, "TEXT", "Standard", aWidthFactor: 0.8, aTextType: dxxTextTypes.AttDef, aAttributeTag: tg.ToUpper(), aAttributePrompt: "Scale"));

                y1 = yvals[2];
                X1 = x3 + (x4 - x3) / 2;
                txt = "NO. SHTS.";
                oBBorder.Entities.Add(tool.Create_Text(new dxfVector(x3 + 0.0425, y1 - 0.0325), txt, 0.0675, dxxMTextAlignments.TopLeft, "TEXT", "Standard"));
                tg = "#SHEETS";
                txt = string.Empty;
                oBBorder.Entities.Add(tool.Create_Text(new dxfVector(X1, y1 - 0.35), txt, 0.125, dxxMTextAlignments.MiddleCenter, "TEXT", "Standard", aWidthFactor: 0.8, aTextType: dxxTextTypes.AttDef, aAttributeTag: tg.ToUpper(), aAttributePrompt: "Number of Sheets"));

                y1 = yvals[6];
                txt = "SHT. NO.";
                oBBorder.Entities.Add(tool.Create_Text(new dxfVector(x3 + 0.0425, y1 - 0.0325), txt, 0.0675, dxxMTextAlignments.TopLeft, "TEXT", "Standard"));
                tg = "SHEETS#";
                txt = string.Empty;
                oBBorder.Entities.Add(tool.Create_Text(new dxfVector(X1, y1 - 0.35), txt, 0.125, dxxMTextAlignments.MiddleCenter, "TEXT", "Standard", aWidthFactor: 0.8, aTextType: dxxTextTypes.AttDef, aAttributeTag: tg.ToUpper(), aAttributePrompt: "Sheet Number"));

                y1 = yvals[2];
                X1 = x4 + (16 - x4) / 2;
                txt = "LATEST\\PREV.";
                oBBorder.Entities.Add(tool.Create_Text(new dxfVector(x4 + 0.0325, y1 - 0.0325), txt, 0.0675, dxxMTextAlignments.TopLeft, "TEXT", "Standard"));
                tg = "REV#";
                txt = string.Empty;
                oBBorder.Entities.Add(tool.Create_Text(new dxfVector(X1, y1 - 0.65), txt, 0.1875, dxxMTextAlignments.MiddleCenter, "TEXT", "Standard", aWidthFactor: 0.8, aTextType: dxxTextTypes.AttDef, aAttributeTag: tg.ToUpper(), aAttributePrompt: "Latest Alteration Number"));

                y1 = yvals[5] - yvals[5] / 2;
                x0 = x2 + 0.0625;
                txt = "B-";
                aTxt = (dxeText)oBBorder.Entities.Add(tool.Create_Text(new dxfVector(x0, y1), txt, 0.1875, dxxMTextAlignments.MiddleLeft, "TEXT", "Standard"));

                x0 = x0 + aTxt.BoundingRectangle().Width + 0.0625;
                tg = "DRAWING#";
                txt = string.Empty;
                oBBorder.Entities.Add(tool.Create_Text(new dxfVector(x0, y1), txt, 0.1875, dxxMTextAlignments.MiddleLeft, "TEXT", "Standard", aWidthFactor: 0.8, aTextType: dxxTextTypes.AttDef, aAttributeTag: tg.ToUpper(), aAttributePrompt: "Drawing Number D-"));

                _rVal = oBBorder.Clone();
            }
            else
            {
                Status = "Copying B-Size Border";

                _rVal = oBBorder.Clone();
            }

            return _rVal;
        }

        private dxfBlock CreateBlock_RevisionsD(dxfImage aImage)
        {
            Status = "Creating D-Size Revisions Block";


            dxfBlock _rVal = new();
            double X1;

            string txt;
            double y1;
            double y2;
            double y3;
            double y0;
            double stp;
            string tg = string.Empty;
            string prmt = string.Empty;
            int rws;
            double[] xVals = new double[8];
            double ytp;
            dxxMTextAlignments almnt;

            rws = 6;
            xVals[0] = 14.3018;
            xVals[1] = 14.8956;
            xVals[2] = 24.2807;
            xVals[3] = 25.0681;
            xVals[4] = 25.6587;
            xVals[5] = 26.6036;
            xVals[6] = 27.5485;
            xVals[7] = 28.4934;
            stp = 0.225;
            ytp = 1.6337;
            y1 = ytp - (rws * stp);

            dxoEntityTool tool = aImage.EntityTool;
            //_rVal.ImageGUID = aImage.GUID; // property is readonly
            _rVal.Name = "REVISIONS";

            dxfDisplaySettings dsp = new("BORDER", dxxColors.ByLayer, dxfLinetypes.Continuous);
            txt = $"({xVals[0]},{0})¸({xVals[0]},{ytp})¸({xVals[7]},{ytp})";
            _rVal.Entities.AddPolyline(txt, false, dsp);

            for (int i = 1; i <= rws; i++)
            {
                y0 = ytp - i * stp;
                txt = $"({xVals[0]},{y0})¸({xVals[7]},{y0})";
                _rVal.Entities.AddPolyline(txt, false, dsp);
            }

            for (int i = 2; i <= 7; i++)
            {
                txt = $"({xVals[i - 1]},0.0)¸({xVals[i - 1]},{ytp})";
                _rVal.Entities.AddPolyline(txt, false, dsp);
            }

            y2 = 0.65 * stp;
            for (int i = 2; i <= 8; i++)
            {
                txt = i switch
                {
                    2 => "LETTER",
                    3 => "ALTERATION",
                    4 => "DATE",
                    5 => "BY",
                    6 => "CHK'D",
                    7 => "REV'D",
                    8 => "APPV'D",
                    _ => "XXXX"
                };


                X1 = xVals[i - 2] + (xVals[i - 1] - xVals[i - 2]) / 2;
                _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y2), txt, 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard", aWidthFactor: 0.8));
            }

            y3 = y1 + 0.0625;
            for (int j = 1; j <= rws; j++)
            {
                txt = "XXX";

                for (int i = 2; i <= 8; i++)
                {
                    almnt = dxxMTextAlignments.BaselineMiddle;
                    switch (i)
                    {
                        case 2:
                            tg = $"REV{j}";
                            prmt = $"Revision {j} Letter";
                            break;
                        case 3:
                            tg = $"REVISION{j}";
                            prmt = $"Revision {j} Description";
                            almnt = dxxMTextAlignments.BaselineLeft;
                            break;
                        case 4:
                            tg = $"DATE{j}";
                            prmt = $"Revision {j} Date";
                            break;
                        case 5:
                            tg = $"BY{j}";
                            prmt = $"Revision {j} By";
                            break;
                        case 6:
                            tg = $"CHECKED{j}";
                            prmt = $"Revision {j} Checked";
                            break;
                        case 7:
                            tg = $"REVIEWED{j}";
                            prmt = $"Revision {j} Reviewed";
                            break;
                        case 8:
                            tg = $"APPROVED{j}";
                            prmt = $"Revision {j} Approved";
                            break;
                        default:
                            break;
                    }
                    if (almnt == dxxMTextAlignments.BaselineMiddle)
                    {
                        X1 = xVals[i - 2] + (xVals[i - 1] - xVals[i - 2]) / 2;
                    }
                    else
                    {
                        X1 = xVals[i - 2] + 0.0625;
                    }

                    _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y3), "", 0.125, almnt, aWidthFactor: 0.8, aTextType: dxxTextTypes.AttDef, aAttributeTag: tg, aAttributePrompt: prmt));
                }

                y3 += stp;
            }

            return _rVal;
        }

        private dxfBlock CreateBlock_RevisionsB(dxfImage aImage)
        {
            Status = "Creating B-Size Revisions Block";


            dxfBlock _rVal = new();
            double X1;

            string txt;

            double y2;
            double y3;
            double y0;
            double stp;
            string tg = string.Empty;
            string prmt = string.Empty;
            int rws;
            double[] xVals = new double[7];
            double ytp;
            dxxMTextAlignments almnt;
            dxoEntityTool tool = aImage.EntityTool;

            rws = 3;
            xVals[0] = 0;
            xVals[1] = 0.5994;
            xVals[2] = 8.1584;
            xVals[3] = 8.7796;
            xVals[4] = 9.4672;
            xVals[5] = 10.433;
            xVals[6] = 11.4588;
            stp = 0.2344;
            ytp = 0.9375;



            //_rVal.ImageGUID = aImage.GUID; // property is readonly
            _rVal.Name = "REVISIONS";
            dxfDisplaySettings dsp = new("BORDER", dxxColors.ByLayer, dxfLinetypes.Continuous);
            txt = $"({xVals[0]},{ytp})¸({xVals[6]},{ytp})";
            _rVal.Entities.AddPolyline(txt, false, dsp, 0.0125);

            for (int i = 1; i <= rws; i++)
            {
                y0 = ytp - i * stp;
                txt = $"({xVals[0]},{y0})¸({xVals[6]},{y0})";
                _rVal.Entities.AddPolyline(txt, false, dsp, 0.0125);
            }

            for (int i = 2; i <= 6; i++)
            {
                txt = $"({xVals[i - 1]},0.0)¸({xVals[i - 1]},{ytp})";
                _rVal.Entities.AddPolyline(txt, false, dsp, 0.0125);
            }

            y2 = 0.5 * stp;
            for (int i = 2; i <= 7; i++)
            {
                switch (i)
                {
                    case 2:
                        txt = "LETTER";
                        break;
                    case 3:
                        txt = "ALTERATION";
                        break;
                    case 4:
                        txt = "DATE";
                        break;
                    case 5:
                        txt = "BY";
                        break;
                    case 6:
                        txt = "CHK'D";
                        break;
                    case 7:
                        txt = "REV'D";
                        break;
                    default:
                        txt = "XXXX";
                        break;
                }

                X1 = xVals[i - 2] + (xVals[i - 1] - xVals[i - 2]) / 2;
                _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y2), txt, 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard", aWidthFactor: 0.8));
            }

            y3 = stp + 0.0625;
            for (int j = 1; j <= rws; j++)
            {
                txt = "XXX";

                for (int i = 2; i <= 7; i++)
                {
                    almnt = dxxMTextAlignments.BaselineMiddle;
                    switch (i)
                    {
                        case 2:
                            tg = $"REV{j}";
                            prmt = $"Revision {j} Letter";
                            break;
                        case 3:
                            tg = $"REVISION{j}";
                            prmt = $"Revision {j} Description";
                            almnt = dxxMTextAlignments.BaselineLeft;
                            break;
                        case 4:
                            tg = $"DATE{j}";
                            prmt = $"Revision {j} Date";
                            break;
                        case 5:
                            tg = $"BY{j}";
                            prmt = $"Revision {j} By";
                            break;
                        case 6:
                            tg = $"CHECKED{j}";
                            prmt = $"Revision {j} Checked";
                            break;
                        case 7:
                            tg = $"APPROVED{j}";
                            prmt = $"Revision {j} Approved";
                            break;
                        default:
                            break;
                    }
                    if (almnt == dxxMTextAlignments.BaselineMiddle)
                    {
                        X1 = xVals[i - 2] + (xVals[i - 1] - xVals[i - 2]) / 2;
                    }
                    else
                    {
                        X1 = xVals[i - 2] + 0.0625;
                    }

                    _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y3), "", 0.125, almnt, aWidthFactor: 0.8, aTextType: dxxTextTypes.AttDef, aAttributeTag: tg, aAttributePrompt: prmt));
                }

                y3 += stp;
            }

            return _rVal;
        }

        private dxfBlock CreateBlock_SymbolKey(string aBlockName, dxfImage aImage)
        {
            Status = $"Creating Block '{aBlockName}'";


            dxfBlock _rVal = new(aBlockName);
            dxfDisplaySettings dSets = aImage.GetDisplaySettings(dxxEntityTypes.DimLinearV);
            double X1 = -2.035;
            double x2 = -1.6395;
            double y1 = -0.5125;
            double stp = 0.425;
            double rad = 0.1875;
            string txt;
            dxoEntityTool tool = aImage.EntityTool;
            dSets.Linetype = dxfLinetypes.Continuous;
            dxfDisplaySettings dsp = new("BORDER", dxxColors.ByLayer, dxfLinetypes.Continuous);

            _rVal.Entities.AddPolyline("(0.0,0.0)¸(-2.48843,0.0)¸(-2.48843,-2.8682)", false, dsp, 0.02);

            _rVal.Entities.AddLine(-2.48843, -0.2627, 0, -0.2627, dsp);
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(-1.2442, -0.1313), "SYMBOL KEY", 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));

            _rVal.Entities.Add((dxfEntity)aImage.Primatives.Pill(new dxfVector(X1, y1), 0.7125, 2 * rad, aDisplaySettings: dSets));
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), "XXX", 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(x2, y1), "PART NUMBER", 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard"));

            y1 -= stp;
            _rVal.Entities.AddArc(X1, y1, rad, aDisplaySettings: dSets);
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), "XX", 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(x2, y1), "NOZZLE", 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard"));

            y1 -= stp;
            _rVal.Entities.AddArc(X1, y1, rad, aDisplaySettings: dSets);
            _rVal.Entities.AddLine(X1 - rad, y1, X1 + rad, y1, dSets);
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1 + 0.08), "XX", 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1 - 0.08), "X", 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(x2, y1), "NOZZLE", 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard"));

            y1 -= stp;
            _rVal.Entities.Add((dxfEntity)aImage.Primatives.Polygon(new dxfVector(X1, y1), false, 3, rad, aLayer: dSets.LayerName, aColor: dxxColors.ByLayer, aLineType: dxfLinetypes.Continuous));
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), "X", 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(x2, y1), "REVISION", 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard"));

            y1 -= stp + 0.0625;
            _rVal.Entities.Add((dxfEntity)aImage.Primatives.Polygon(new dxfVector(X1, y1), false, 6, 0.2165, aLayer: dSets.LayerName, aColor: dxxColors.ByLayer, aLineType: dxfLinetypes.Continuous));
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), "XXX", 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(x2, y1), "ASSEMBLY DETAILS", 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard"));

            y1 -= stp;
            txt = $"({X1 - 0.1435},{y1})";
            txt += $"¸({X1},{y1 - 0.2145})";
            txt += $"¸({X1 + 0.1435},{y1})";
            txt += $"¸({X1},{y1 + 0.2145})";
            _rVal.Entities.AddPolyline(txt, true, new dxfDisplaySettings(dSets.LayerName, dxxColors.ByLayer, dxfLinetypes.Continuous));

            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), "XX", 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(x2, y1), "NOTES", 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard"));

            return _rVal;
        }

        private dxfBlock CreateBlock_BubblePromoter(string aBlockName, dxfImage aImage, uopDocDrawing aDrawing)
        {
            Status = $"Creating Block '{aBlockName}'";


            dxfBlock rBlock = new(aBlockName);
            dxfDisplaySettings dSets = aImage.GetDisplaySettings(dxxEntityTypes.DimLinearV);
            dxeSolid aSld = new(dxfVector.Zero, new dxfVector(-0.1, 0.0167), new dxfVector(-0.1, -0.0167)) { DisplaySettings = dSets };
            colDXFEntities ents = new();
            dxoEntityTool tool = aImage.EntityTool;


            dxfDisplaySettings dsets = new("GEOMETRY", dxxColors.ByLayer, dxfLinetypes.Continuous);

            // border
            colDXFVectors verts = new colDXFVectors("(2.6519,-0.2334)¸(2.6519,-2.8004)¸(2.4185,-3.0338)¸(-2.4185,-3.0338)¸(-2.6519,-2.8004)¸(-2.6519,-0.2334)¸(-2.4185,0.0)¸(2.4185,0.0)¸(2.6519,-0.2334)");
            ents.AddPolyline(verts, true, dsets.Copy("HEAVY"));
            // bubble polyline
            //verts = new colDXFVectors("(-0.6823,-1.5494)¸(-0.9241,-1.5494)¸(-0.9469,-1.4911)¸(-0.7041,-1.4911,-1.0489)¸(0.91102,-1.4911)¸(1.1538,-1.4911)¸(1.1301,-1.5494)¸(0.8831,-1.5494,0.9905)");
            verts = new colDXFVectors("(-0.6823,-1.5494)¸(-0.9241,-1.5494)¸(-0.9469,-1.4911)¸(-0.7041,-1.4911)¸(0.91102,-1.4911)¸(1.1538,-1.4911)¸(1.1301,-1.5494)¸(0.8831,-1.5494)");
            verts.Item(4).VertexRadius = -1.0489;
            verts.Item(8).VertexRadius = 0.9905;
            dxePolyline aPl = new(verts, true, dsets);
            ents.Add(aPl, aTag: "HATCH BOUNDS");
            // hatch on bubble
            dxeHatch aHatch = new()
            {
                DisplaySettings = dsets,
                HatchStyle = dxxHatchStyle.dxfHatchUserDefined,
                LineStep = 0.065,
                Rotation = 45,
                Boundary = aPl,
                ImageGUID = aImage.GUID
            };

            ents.Add(aHatch);


            // break lines
            ents.AddPolyline("(-0.9615,-1.5868)¸(-0.9241,-1.5494)¸(-0.9469,-1.4911)¸(-0.9095,-1.4535)", false, dsets);
            ents.AddPolyline("(1.1684,-1.5868)¸(1.1310,-1.5494)¸(1.1538,-1.4911)¸(1.1165,-1.4537)", false, dsets);
            // deck thickness lines
            ents.AddLine(0.8831, -1.5494, -0.6762, -1.5494, dsets);
            dxeLine aLn = ents.AddLine(-0.7041, -1.4911, 0.911, -1.4911, new dxfDisplaySettings("HIDDEN", dxxColors.ByLayer, dxfLinetypes.Hidden));
            aImage.LinetypeLayers.ApplyTo(aLn, aImage: aImage);

            // dimensions

            // bubble width
            dsets = new dxfDisplaySettings(dSets.LayerName, dxxColors.ByLayer, dxfLinetypes.Continuous);
            ents.AddLine(-0.7041, -1.4286, -0.7041, -0.7544, dsets);
            ents.AddLine(0.911, -1.4286, 0.911, -0.7544, dsets);
            aLn = ents.AddLine(-0.6041, -0.8544, 0.811, -0.8544, dsets);
            ents.Add(aSld.Rotated(0, 0.911, -0.8544));
            ents.Add(aSld.Rotated(180, -0.7041, -0.8544));

            ents.Add(tool.Create_Text(aLn.MidPt.Moved(aYChange: 0.1), aDrawing.DrawingUnits == uppUnitFamilies.English ? "2.5 APPROX." : "63.5 APPROX.", 0.125, dxxMTextAlignments.BottomCenter));

            // bubble ht
            ents.AddLine(0.1887, -1.1115, 1.7166, -1.1115, dsets);
            ents.AddLine(1.1814, -1.4911, 1.7166, -1.4911, dsets);
            ents.AddLine(1.6266, -1.0115, 1.6266, -0.9115, dsets);
            ents.AddLine(1.6266, -1.5911, 1.6266, -1.6911, dsets);
            ents.Add(aSld.Rotated(-90, 1.6266, -1.1115));
            ents.Add(aSld.Rotated(90, 1.6266, -1.4911));


            ents.Add(tool.Create_Text(new dxfVector(1.6266, -1.3013), aDrawing.DrawingUnits == uppUnitFamilies.English ? ".50" : "12.7", 0.125, dxxMTextAlignments.MiddleCenter));


            // tray deck leader
            aPl = ents.AddPolyline("(-0.8372,-1.4040)¸(-0.9652,-0.9180)¸(-1.0553,-0.9180)", false, dsets);
            ents.Add(aSld.Rotated(-75.2, -0.8142, -1.4911));

            ents.Add(tool.Create_Text(aPl.LastVertex().Moved(-0.1), "TRAY DECK", 0.125, dxxMTextAlignments.MiddleRight));

            // bubble rad leader
            aPl = ents.AddPolyline("(0.4230,-1.3292)¸(0.1103,-2.1427)¸(-0.0337,-2.1427)", false, dsets);
            ents.Add(aSld.Rotated(68.97, 0.4589, -1.2358));
            ents.Add(tool.Create_Text(aPl.LastVertex().Moved(-0.1), aDrawing.DrawingUnits == uppUnitFamilies.English ? "1.875 RAD" : "47.6 RAD", 0.125, dxxMTextAlignments.MiddleRight));

            // detail label
            ents.Add(tool.Create_Text(new dxfVector(0, -2.928), @"\LBUBBLE PROMOTER", 0.1875, dxxMTextAlignments.BottomCenter));



            rBlock.Entities.Append(ents, bAddClones: false);
            return rBlock;
        }

        private dxfBlock CreateBlock_MDDCCrossSection(string aBlockName, dxfImage aImage, mdTrayAssembly aAssy, uopDocDrawing aDrawing)
        {
            Status = $"Creating Block '{aBlockName}'";


            dxfBlock _rVal = new(aBlockName);
            dxfDisplaySettings dSets = aImage.GetDisplaySettings(dxxEntityTypes.DimLinearV);
            dxfDisplaySettings tSets = aImage.GetDisplaySettings(dxxEntityTypes.MText);
            dxeSolid aSld = new(dxfVector.Zero, new dxfVector(-0.1, 0.0167), new dxfVector(-0.1, -0.0167), aDisplaySettings: dSets);
            dxeText aTxt = new(dxxTextTypes.Multiline, aDisplaySettings: tSets) { Alignment = dxxMTextAlignments.MiddleCenter, TextHeight = 0.125, TextStyleName = "Standard" };
            colDXFEntities ents = new();


            _rVal.Name = aBlockName;
            // border
            dxfDisplaySettings dsets = new("HEAVY", dxxColors.ByLayer, dxfLinetypes.Continuous);

            ents.AddPolyline("(-2.4185,0.0)¸(2.4185,0.0)¸(2.6519,-0.2334)¸(2.6519,-2.8004)¸(2.4185,-3.0339)¸(-2.4185,-3.0338)¸(-2.6519,-2.8004)¸(-2.6519,-0.23337)¸(-2.4185,0.0)", true, dsets);
            dsets.LayerName = "GEOMETRY";
            // dc body
            dxePolyline aPl = new("(-0.3490,-2.3257)¸(0.3490,-2.3257)¸(0.3490,-0.7767)¸(0.4074,-0.7767)¸(0.4074,-2.3841)¸(-0.4074,-2.3841)¸(-0.4074,-0.7767)¸(-0.3490,-0.7767)", true, dsets);
            ents.Add(aPl);

            dxeHatch aHatch = new()
            {
                HatchStyle = dxxHatchStyle.dxfHatchUserDefined,
                LineStep = 0.065,
                Rotation = 45,
                Boundary = aPl,
                LayerName = "GEOMETRY",
                Color = dxxColors.ByLayer,
                Linetype = dxfLinetypes.Continuous
            };

            ents.Add(aHatch);

            dxeLine cLn = ents.AddLine(0, -2.6053, 0, -0.6028, new dxfDisplaySettings("Center", dxxColors.ByLayer, dxfLinetypes.Center));
            aImage.LinetypeLayers.ApplyTo(cLn, aImage: aImage);

            // deck
            aPl = new dxePolyline("(-0.8297,-1.0908)¸(-0.4932,-1.0908)¸(-0.4932,-1.1491)¸(-0.8372,-1.1491)¸(-0.81418,-1.1327)¸(-0.8459,-1.1151)", true, dsets);
            ents.Add(aPl, bAddClone: true);
            ents.Add(aHatch.Clone(aPl));

            aPl.Vertices.Mirror(cLn);
            ents.Add(aPl);


            ents.Add(aHatch.Clone(aPl));

            // deck break lines
            aPl = new dxePolyline("(-0.8313,-1.1721)¸(-0.8446,-1.1544)¸(-0.8142,-1.1327)¸(-0.8459,-1.1151)¸(-0.8248,-1.0833)¸(-0.8407,-1.0701)", false, dsets);
            ents.Add(aPl, bAddClone: true);

            aPl.Vertices.Mirror(cLn);
            ents.Add(aPl);

            // support angles
            aPl = new dxePolyline("(-0.4074,-1.4172)¸(-0.4074,-1.1491)¸(-0.6754,-1.1491)¸(-0.6754,-1.2075)¸(-0.4657,-1.2075)¸(-0.4657,-1.4172)", true, dsets);
            ents.Add(aPl, bAddClone: true);
            ents.Add(aHatch.Clone(aPl, aRotation: 135));


            aPl.Vertices.Mirror(cLn);
            ents.Add(aPl, bAddClone: true);

            ents.Add(aHatch.Clone(aPl, aRotation: 135));

            // tray deck leader
            dsets.LayerName = dSets.LayerName;
            aPl = ents.AddPolyline("(-0.6432,-1.0032)¸(-0.7213,-0.6750)¸(-0.8113,-0.6750)", false, dsets);
            ents.Add(aSld.Rotated(-76.61, -0.6224, -1.0908));


            ents.Add(aTxt.Clone(aPl.Vertex(3).Moved(-0.09), "TRAY DECK", aNewAlignment: dxxMTextAlignments.MiddleRight));

            // dimesions

            // dc width
            ents.AddLine(-0.349, -0.7137, -0.349, -0.4036, dsets);
            ents.AddLine(0.349, -0.7261, 0.349, -0.4036, dsets);
            dxeLine aLn = ents.AddLine(-0.249, -0.4666, 0.249, -0.4666, dsets);
            ents.Add(aSld.Rotated(0, 0.349, -0.4666));
            ents.Add(aSld.Rotated(180, -0.349, -0.4666));


            string txt = aImage.FormatNumber(aAssy?.Downcomer().Width);
            ents.Add(aTxt.Clone(aLn.MidPt.Moved(aYChange: 0.125), txt));

            // deck thickness
            ents.AddLine(0.8927, -1.0908, 1.2122, -1.0908, dsets);
            ents.AddLine(0.9002, -1.1491, 1.2122, -1.1491, dsets);
            dxeLine arrow = ents.AddLine(1.1492, -0.9908, 1.1492, -0.8908, dsets);
            ents.AddLine(1.1492, -1.2491, 1.1492, -1.3491, dsets);
            ents.Add(aSld.Rotated(-90, 1.1492, -1.0908));
            ents.Add(aSld.Rotated(90, 1.1492, -1.1491));

            txt = aImage.FormatNumber(aAssy?.Deck.Thickness);

            ents.Add(aTxt.Clone(arrow.EndPt.Moved(aYChange: 0.05), txt, aNewAlignment: dxxMTextAlignments.BottomCenter));

            // detail label

            ents.Add(aTxt.Clone(new dxfVector(0, -2.95), @"\LX-SECTION OF DOWNCOMER", aNewTextHeight: 0.1875, aNewAlignment: dxxMTextAlignments.BottomCenter));

            _rVal.Entities.Populate(ents, false);

            return _rVal;
        }

        private dxfBlock CreateBlock_MDMaterials(string aBlockName, dxfImage aImage, mdTrayAssembly aAssy, uopDocDrawing aDrawing)
        {
            Status = $"Creating Block '{aBlockName}'";


            dxfBlock _rVal = new(aBlockName);

            colDXFEntities ents = new();
            string txt;
            string mhtxt;
            uopUnit inchesmm = uopUnits.GetUnit(uppUnitTypes.SmallLength);
            //txt = $"DOWNCOMER - {aAssy.Downcomer().Material.FriendlyName().ToUpper()} - {inchesmm.UnitValueString(aAssy.Downcomer().Thickness,uppUnitFamilies.English)} [{  inchesmm.UnitValueString(aAssy.Downcomer().Thickness, uppUnitFamilies.Metric) }]";
            //txt += $"\\PTRAY DECK - {aAssy.Deck.Material.FriendlyName().ToUpper()} - {inchesmm.UnitValueString(aAssy.Deck.Thickness, uppUnitFamilies.English)} [{  inchesmm.UnitValueString(aAssy.Deck.Thickness, uppUnitFamilies.Metric) }]";
            //txt += $"\\PFASTENERS - {aAssy.GetMDRange().HardwareMaterial.MaterialName.ToUpper()}";

            txt = $"DOWNCOMER - {aAssy.Downcomer().Material.FriendlyName().ToUpper()} - {inchesmm.UnitValueString(aAssy.Downcomer().Thickness, uppUnitFamilies.English)}";
            txt += $"\\PTRAY DECK - {aAssy.Deck.Material.FriendlyName().ToUpper()} - {inchesmm.UnitValueString(aAssy.Deck.Thickness, uppUnitFamilies.English)}";
            txt += $"\\PFASTENERS - {aAssy.GetMDRange().HardwareMaterial.MaterialName.ToUpper()}";

            if (aDrawing.Project.Bolting == uppUnitFamilies.English)
                txt += " - UNC ";
            else
                txt += " - METRIC";



            mhtxt = $"COLUMN MANHOLE I.D. = {inchesmm.UnitValueString(aAssy.ManholeID, aDrawing.DrawingUnits)}";


            _rVal.Name = aBlockName;
            ents.AddPolyline("(-3.5647,-0.2334)¸(-3.5627,-1.4556)¸(-3.3294,-1.6890)¸(3.3332,-1.6890)¸(3.5666,-1.4556)¸(3.5647,-0.2334)¸(3.3313,0.0)¸(-3.3313,0.0)¸(-3.5647,-0.2334)", true, new dxfDisplaySettings("HEAVY", dxxColors.ByLayer, dxfLinetypes.Continuous));
            dxeText aTxt = new() { TextString = @"\LTRAY MATERIAL", TextHeight = 0.1875, InsertionPt = new dxfVector(0, -0.0928), DisplaySettings = aImage.GetDisplaySettings(dxxEntityTypes.MText), TextStyleName = "Standard", Alignment = dxxMTextAlignments.TopCenter };

            ents.Add(aTxt);

            aTxt = aTxt.Clone(new dxfVector(-2.5364, -0.9618), txt, 0.125, dxxMTextAlignments.MiddleLeft);

            ents.Add(aTxt);

            aTxt = aTxt.Clone(new dxfVector(-3.3294, -1.7515), mhtxt, 0.125, dxxMTextAlignments.TopLeft);

            ents.Add(aTxt);

            _rVal.Entities.Populate(ents, false);
            return _rVal;
        }

        private dxfBlock CreateBlock_MDDCElevation(string aBlockName, dxfImage aImage, mdTrayAssembly aAssy, uopDocDrawing aDrawing)
        {
            Status = $"Creating Block '{aBlockName}'";


            dxfBlock _rVal = new(aBlockName);
            dxeLine aLn;
            dxfDisplaySettings dSets = aImage.GetDisplaySettings(dxxEntityTypes.DimLinearV);
            dxfDisplaySettings dsets = new("GEOMETRY", aLinetype: dxfLinetypes.Continuous);
            dxeText bTxt;
            colDXFEntities ents = new();

            dxeSolid aSld = new() { DisplaySettings = dSets, Triangular = true, Vertex2 = new dxfVector(-0.1, 0.0167), Vertex3 = new dxfVector(-0.1, -0.0167) };
            dxeText aTxt = new() { DisplaySettings = aImage.GetDisplaySettings(dxxEntityTypes.MText), Alignment = dxxMTextAlignments.MiddleCenter, TextHeight = 0.125, TextStyleName = "Standard" };

            // border
            ents.AddPolyline("(-2.6519,-0.2334)¸(-2.6519,-2.8004)¸(-2.4185,-3.0338)¸(2.41851,-3.0338)¸(2.6519,-2.8004)¸(2.6519,-0.2334)¸(2.4185,0.0)¸(-2.4185,0.0)¸(-2.6519,-0.2334)", true, dsets.Copy("HEAVY"));



            // break lines
            ents.AddPolyline("(-0.4954,-0.6699)¸(-0.4954,-1.5370)¸(-0.6401,-1.5370)¸(-0.4061,-1.6263)¸(-0.4954,-1.6263)¸(-0.4954,-2.5052)", false, dsets);
            ents.AddPolyline("(1.4092,-0.6699)¸(1.4092,-1.5370)¸(1.2646,-1.5370)¸(1.4986,-1.6263)¸(1.4092,-1.6263)¸(1.4092,-2.5052)", false, dsets);

            // dc top and botton


            ents.AddLine(-0.4954, -0.7767, 1.4092, -0.7767, dsets.Copy(aColor: dxxColors.LightGrey));
            ents.AddLine(-0.4954, -2.384, 1.4092, -2.384, dsets.Copy(aColor: dxxColors.LightGrey));

            // slot
            ents.AddPolyline("(0.2605,-0.9283)¸(0.7014,-0.9283,-0.0494)¸(0.7014,-1.0271)¸(0.2605,-1.0271,-0.0494)", true, dsets);

            // horizontal deck lines
            ents.AddLine(-0.4954, -1.0908, 1.4092, -1.0908, dsets);
            ents.AddLine(-0.4954, -1.1491, 1.4092, -1.1491, dsets);
            ents.AddLine(-0.4954, -1.2075, 1.4092, -1.2075, dsets);
            ents.AddLine(-0.4954, -1.4172, 1.4092, -1.4172, dsets);
            aLn = ents.AddLine(-0.4954, -2.3257, 1.4092, -2.3257, new dxfDisplaySettings("HIDDEN", dxxColors.ByLayer, dxfLinetypes.Hidden));
            aImage.LinetypeLayers.ApplyTo(aLn, aImage: aImage);

            // dimensions
            // dc height
            dsets.LayerName = dSets.LayerName;
            ents.AddLine(-0.5538, -2.384, -0.6985, -2.384, dsets);
            ents.AddLine(-0.5538, -2.3257, -1.7222, -2.3257, dsets);
            ents.AddLine(-0.5538, -0.7767, -1.7222, -0.7767, dsets);
            ents.AddLine(-1.6288, -1.3712, -1.6288, -0.87, dsets);
            ents.AddLine(-1.6288, -2.2323, -1.6288, -1.7187, dsets);
            ents.Add(aSld.Rotated(90, -1.6288, -0.7767));
            ents.Add(aSld.Rotated(-90, -1.6288, -2.3257));

            string txt = aImage.FormatNumber(aAssy.Downcomer().InsideHeight);
            ents.Add(aTxt.Clone(new dxfVector(-1.6208, -1.5413), txt));

            // weir height
            ents.AddLine(-0.5538, -1.0908, -1.152, -1.0908, dsets);
            ents.AddLine(-1.0586, -1.1841, -1.0586, -1.2775, dsets);
            ents.AddLine(-1.0586, -0.6833, -1.0586, -0.59, dsets);
            ents.Add(aSld.Rotated(-90, -1.0586, -0.7767));
            ents.Add(aSld.Rotated(90, -1.0586, -1.0908));
            txt = aImage.FormatNumber(aAssy.Downcomer().How);
            ents.Add(aTxt.Clone(new dxfVector(-1.051, -0.9302), txt));

            // su height
            ents.AddLine(0.7598, -0.9283, 1.9058, -0.9283, dsets);
            ents.AddLine(0.7598, -1.0271, 1.9058, -1.0271, dsets);
            ents.AddLine(1.8125, -1.1204, 1.8125, -1.2138, dsets);
            ents.AddLine(1.8125, -0.835, 1.8125, -0.7416, dsets);
            ents.Add(aSld.Rotated(-90, 1.8125, -0.9283));
            ents.Add(aSld.Rotated(90, 1.8125, -1.0271));

            txt = aImage.FormatNumber(aAssy.Downcomer().StartupDiameter);
            ents.Add(aTxt.Clone(new dxfVector(1.8043, -0.6162), txt));



            // su width
            ents.AddLine(0.2112, -0.9194, 0.2112, -0.3075, dsets);
            ents.AddLine(0.7508, -0.9194, 0.7508, -0.3075, dsets);
            ents.AddLine(0.3045, -0.4008, 0.6575, -0.4008, dsets);
            ents.Add(aSld.Rotated(0, 0.7508, -0.4008));
            ents.Add(aSld.Rotated(180, 0.2112, -0.4008));

            txt = aImage.FormatNumber(aAssy.Downcomer().StartupLength);
            ents.Add(aTxt.Clone(new dxfVector(0.4778, -0.1667), txt));


            // detail label
            bTxt = aTxt.Clone(new dxfVector(0, -2.95), @"\LELEVATION VIEW OF DOWNCOMER", 0.1875, dxxMTextAlignments.BottomCenter);

            ents.Add(bTxt);

            _rVal.Entities.Populate(ents, false);
            return _rVal;
        }

        private dxfBlock CreateBlock_BeamDetail(string aBlockName, dxfImage aImage, mdTrayAssembly aAssy, uopDocDrawing aDrawing)
        {
            Status = $"Creating Block '{aBlockName}'";


            dxfBlock _rVal = new(aBlockName);
            colDXFEntities ents = new();

            mdBeam beam = aAssy.Beam;
            double distanceFromLastOpeningToBeamEnd = FindDistanceBetweenLastOpeningAndEndOfBeam(beam.WebOpeningSize, beam.WebOpeningSize, beam.WebOpeningCount, beam.Length);

            double beamLengthToDraw; // We do not draw the whole beam. We only draw part of it (at max, half of it). This variable holds the length of the beam to draw.
            if (beam.WebOpeningCount <= 4)
            {
                // This drawing is supposed to show the last two openings plus half of the gap before the first opening.
                // In this case, because there are not enough openings, we have to draw half of the beam. However, we do not expect this to happen in practice.
                beamLengthToDraw = beam.Length / 2;
            }
            else
            {
                // Include last two openings plus half of the gap before the first opening
                beamLengthToDraw = distanceFromLastOpeningToBeamEnd + 3.5 * beam.WebOpeningSize;
            }

            double halfBeamLengthToDraw = beamLengthToDraw / 2;
            double halfBeamHeight = beam.Height / 2;
            double halfWebOpeningSize = beam.WebOpeningSize / 2;
            double openingCornerRadius = 30 / 25.4; // 30mm in inches

            // Y ordinates
            double beamTopOut = halfBeamHeight;
            double beamTopIn = halfBeamHeight - beam.FlangeThickness;
            double beamBottomIn = -halfBeamHeight + beam.FlangeThickness;
            double beamBottomOut = -halfBeamHeight;
            double beamTopWeb = halfWebOpeningSize;
            double beamBottomWeb = -halfWebOpeningSize;

            // X ordinates
            double beamLeftSide = -halfBeamLengthToDraw;
            double beamRightSide = halfBeamLengthToDraw;

            // ****************************** Draw top and bottom sides of the beam ******************************
            var beamTopPartPolyline = ents.AddRectangle(beamLeftSide, beamRightSide, beamTopOut, beamTopIn);
            beamTopPartPolyline.LayerName = "IBeam";
            var beamBottomPartPolyline = ents.AddRectangle(beamLeftSide, beamRightSide, beamBottomIn, beamBottomOut);
            beamBottomPartPolyline.LayerName = "IBeam";

            // ****************************** Draw left and right sides of the beam ******************************
            var beamLeftVerticalLine = ents.AddLine(beamLeftSide + beam.WebInset, beamTopIn, beamLeftSide + beam.WebInset, beamBottomIn);
            beamLeftVerticalLine.LayerName = "IBeam";
            var breakLineEntities = GetBreakLineForBeamDetail(beamRightSide, beamTopOut, beamBottomOut, 2, 65, 2);
            foreach (var breakLineEntity in breakLineEntities)
            {
                breakLineEntity.LayerName = "IBeam";
                ents.Add(breakLineEntity);
            }

            // ****************************** Draw the beam hole *************************************************
            var bottomLeftHole = beam.GenHoles(aTag: "BEAM", aFlag: "BOTTOM LEFT").Item(1);
            double halfHoleLength = bottomLeftHole.Length / 2;
            var bottomLeftHoleCenter = bottomLeftHole.Centers.First;
            double distanceFromBeamLeftSideToHoleCenter = FindBottomLeftHoleDistanceFromBeamLeftSide(bottomLeftHoleCenter, beam);
            double centerHoleX = beamLeftSide + distanceFromBeamLeftSideToHoleCenter;
            double leftHoleX = centerHoleX - halfHoleLength;
            double rightHoleX = centerHoleX + halfHoleLength;

            var holeLefVerticalLine = ents.AddLine(leftHoleX, beamBottomIn, leftHoleX, beamBottomOut);
            holeLefVerticalLine.LayerName = "IBeam";
            holeLefVerticalLine.Linetype = "HIDDEN2";
            holeLefVerticalLine.Color = dxxColors.Green;

            var holeCenterVerticalLine = ents.AddLine(centerHoleX, beamBottomIn + 1.0236, centerHoleX, beamBottomOut - 1.0236);
            holeCenterVerticalLine.LayerName = "IBeam";
            holeCenterVerticalLine.Linetype = "CENTER2";
            holeCenterVerticalLine.Color = dxxColors.Red;

            var holeRightVerticalLine = ents.AddLine(rightHoleX, beamBottomIn, rightHoleX, beamBottomOut);
            holeRightVerticalLine.LayerName = "IBeam";
            holeRightVerticalLine.Linetype = "HIDDEN2";
            holeRightVerticalLine.Color = dxxColors.Green;

            // ****************************** Draw web openings **************************************************
            var webOpeningEntities = GetWebOpeningsForBeamDetail(beam.WebOpeningSize, beam.WebOpeningSize, beamRightSide, 0, openingCornerRadius, beam.WebOpeningCount);
            foreach (var webOpeningEntity in webOpeningEntities)
            {
                webOpeningEntity.LayerName = "IBeam";
                ents.Add(webOpeningEntity);
            }

            // ****************************** Draw dimensions ****************************************************
            // Dimension for beam height
            var heightDimensionEntities = GetBeamHeightDimensionForBeamDetail(beamLeftSide, beamTopOut, beamBottomOut, aImage);
            foreach (var dimensionEntity in heightDimensionEntities)
            {
                if (dimensionEntity is not dxeText)
                {
                    dimensionEntity.LayerName = "AM_Dimension";
                }
                ents.Add(dimensionEntity);
            }

            // Dimension to show the distance from the last opening to the end of the beam
            if (beam.WebOpeningCount > 0)
            {
                var distanceToLastWebOpeningDimensionEntities = GetLastWebOpeningDistanceToEndOfBeamDimensionForBeamDetail(distanceFromLastOpeningToBeamEnd, beamLeftSide, beamBottomOut, beamBottomWeb, aImage);
                foreach (var dimensionEntity in distanceToLastWebOpeningDimensionEntities)
                {
                    if (dimensionEntity is not dxeText)
                    {
                        dimensionEntity.LayerName = "AM_Dimension";
                    }
                    ents.Add(dimensionEntity);
                }
            }

            // Dimension for web opening width
            if (beam.WebOpeningCount > 0)
            {
                double dimensionVerticalLinesLength = 27.031;
                double lastWebOpeningLeftX = beamLeftSide + distanceFromLastOpeningToBeamEnd;
                double lastWebOpeningRightX = Math.Min(lastWebOpeningLeftX + beam.WebOpeningSize, beamRightSide); // This is for the condition that we have only one opening
                var webOpeningWidthDimensionEntities = GetWebOpeningWidthDimensionForBeamDetail(lastWebOpeningLeftX, lastWebOpeningRightX, beamBottomWeb, dimensionVerticalLinesLength, aImage);
                foreach (var dimensionEntity in webOpeningWidthDimensionEntities)
                {
                    if (dimensionEntity is not dxeText)
                    {
                        dimensionEntity.LayerName = "AM_Dimension";
                    }
                    ents.Add(dimensionEntity);
                }
            }

            // Dimension for the gap between web openings
            if (beam.WebOpeningCount >= 2)
            {
                // The method for getting the dimension for web opening width works properly for gap between openings as well. That is why the method is reused here.
                double dimensionVerticalLinesLength = 16.7105;
                double lastGapLeftX = beamLeftSide + distanceFromLastOpeningToBeamEnd + beam.WebOpeningSize;
                double lastGapRightX = Math.Min(lastGapLeftX + beam.WebOpeningSize, beamRightSide); // This is for the condition that we have only one opening
                var webOpeningWidthDimensionEntities = GetWebOpeningWidthDimensionForBeamDetail(lastGapLeftX, lastGapRightX, beamBottomWeb, dimensionVerticalLinesLength, aImage);
                foreach (var dimensionEntity in webOpeningWidthDimensionEntities)
                {
                    if (dimensionEntity is not dxeText)
                    {
                        dimensionEntity.LayerName = "AM_Dimension";
                    }
                    ents.Add(dimensionEntity);
                }
            }

            // Dimension for web opening height
            if (beam.WebOpeningCount > 0)
            {
                double firstWebOpeningRightX = beamRightSide; // For 1 and 3 openings, we have a half opening at the center of the beam. So, the right X is at the beam edge.
                if (beam.WebOpeningCount != 1 && beam.WebOpeningCount != 3)
                {
                    firstWebOpeningRightX -= beam.WebOpeningSize / 2; // we have a half gap between the right edge of the beam and the right edge of the first opening.
                }

                var webOpeningHeightDimensionEntities = GetWebOpeningHeightDimensionForBeamDetail(firstWebOpeningRightX, beamTopWeb, beamBottomWeb, beamRightSide, aImage);
                foreach (var dimensionEntity in webOpeningHeightDimensionEntities)
                {
                    if (dimensionEntity is not dxeText)
                    {
                        dimensionEntity.LayerName = "AM_Dimension";
                    }
                    ents.Add(dimensionEntity);
                }
            }

            // ****************************** Draw leaders *******************************************************
            // Leader for web opening corner radius
            if (beam.WebOpeningCount > 0)
            {
                double webOpeningRadiusLeaderX = beamRightSide - (beam.WebOpeningSize / 2) + openingCornerRadius; // For cases with 1 or 3 openings, we point to the start of the arc.
                double webOpeningRadiusLeaderY = beamTopWeb;
                double arrowLineAngle = 90;
                if (beam.WebOpeningCount != 1)
                {
                    double distanceFromArcToTheOpeningEdge = 0.5 * openingCornerRadius * (2 - Math.Sqrt(2)); // r - (sqrt(2)/2) * r
                    double lastWebOpeningRightX = beamLeftSide + distanceFromLastOpeningToBeamEnd + beam.WebOpeningSize;
                    webOpeningRadiusLeaderX = lastWebOpeningRightX - distanceFromArcToTheOpeningEdge; // The contact point is calculated based on 45 degree line from center of the arc.
                    webOpeningRadiusLeaderY = beamTopWeb - distanceFromArcToTheOpeningEdge;
                    arrowLineAngle = 45;
                }

                string radiusLeaderText = $"\\A1;R{aImage.FormatNumber(openingCornerRadius)} [R{aImage.FormatNumber(openingCornerRadius * 25.4)}mm] (TYP)";
                var webOpeningRadiusLeaderEntities = GetWebOpeningLeaderForBeamDetail(radiusLeaderText, webOpeningRadiusLeaderX, webOpeningRadiusLeaderY, arrowLineAngle, 13.3543, 2.9143, aImage);
                foreach (var dimensionEntity in webOpeningRadiusLeaderEntities)
                {
                    if (dimensionEntity is not dxeText)
                    {
                        dimensionEntity.LayerName = "AM_Dimension";
                    }
                    ents.Add(dimensionEntity);
                }
            }

            // Leader for the web opening
            if (beam.WebOpeningCount > 0)
            {
                double distanceFromArcToTheOpeningEdge = 0.5 * openingCornerRadius * (2 - Math.Sqrt(2)); // r - (sqrt(2)/2) * r
                double lastWebOpeningLeftX = beamLeftSide + distanceFromLastOpeningToBeamEnd;

                double webOpeningLeaderX = lastWebOpeningLeftX + distanceFromArcToTheOpeningEdge; // The contact point is calculated based on 135 degree line from center of the arc.
                double webOpeningLeaderY = beamTopWeb - distanceFromArcToTheOpeningEdge;
                double arrowLineAngle = 135;

                string webOpeningLeaderText = "\\A1;OPEN (TYP)";
                var webOpeningLeaderEntities = GetWebOpeningLeaderForBeamDetail(webOpeningLeaderText, webOpeningLeaderX, webOpeningLeaderY, arrowLineAngle, 10.4382, -4, aImage);
                foreach (var dimensionEntity in webOpeningLeaderEntities)
                {
                    if (dimensionEntity is not dxeText)
                    {
                        dimensionEntity.LayerName = "AM_Dimension";
                    }
                    ents.Add(dimensionEntity);
                }
            }

            // ****************************** Draw the beam detail label *****************************************
            dxeText beamDetailLabel = new()
            {
                DisplaySettings = aImage.GetDisplaySettings(dxxEntityTypes.MText),
                Alignment = dxxMTextAlignments.MiddleCenter,
                TextHeight = 5.4643,
                TextStyleName = "ROMANS",
                InsertionPt = new dxfVector(0, -45),
                TextString = @"{\Fromand|c0;\LI-BEAM WITH WEB OPENING}",
                LineWeight = dxxLineWeights.LW_200
            };
            ents.Add(beamDetailLabel);

            _rVal.Entities.Populate(ents, false);
            return _rVal;
        }

        private dxfBlock CreateBlock_MDTable_Functional(string aBlockName, dxfImage aImage, mdTrayAssembly aAssy, uopDocDrawing aDrawing, double aMechanicalArea = 0, double aFunctionalArea = 0)
        {
            Status = $"Creating Block '{aBlockName}'";


            dxfBlock _rVal = new(aBlockName);
            string txt;
            double multi = 1.0 / 144;
            double multi2 = 1;
            if (aDrawing.DrawingUnits == uppUnitFamilies.Metric)
            {
                multi = Math.Pow(2.54, 2);
                multi2 = 25.4;
            }

            double mechArea = aMechanicalArea;
            double funcArea = aFunctionalArea;
            double RFactor = (mechArea <= 0 || funcArea <= 0) ? aAssy.RFactor(out funcArea, out mechArea) : funcArea / mechArea;
            double aFP = aAssy.Deck.Fp;
            dxoEntityTool tool = aImage.EntityTool;
            dxfDisplaySettings dsp = new("BORDER", dxxColors.LightGrey, dxfLinetypes.Continuous);

            // border
            _rVal.Entities.AddPolyline("(-5.8246,-0.5834)¸(-5.8246,0.0)¸(5.8246,0.0)¸(5.8246,-1.9836)¸(-5.8246,-1.9836)¸(-5.8246,-0.5834)¸(5.8246,-0.5834)", false, dsp.Copy("HEAVY", aColor: dxxColors.ByLayer));

            // Vertical lines

            _rVal.Entities.AddLine(-4.6218, 0, -4.6218, -1.9836, dsp);
            _rVal.Entities.AddLine(-3.0476, 0, -3.0476, -1.9836, dsp);
            _rVal.Entities.AddLine(-1.8472, 0, -1.8472, -1.9836, dsp);
            _rVal.Entities.AddLine(-0.1482, 0, -0.1482, -1.9836, dsp);
            _rVal.Entities.AddLine(1.0649, 0, 1.0649, -1.9836, dsp);
            _rVal.Entities.AddLine(2.7972, 0, 2.7972, -1.9836, dsp);
            _rVal.Entities.AddLine(4.2951, 0, 4.2951, -1.9836, dsp);
            // horizontal lines
            _rVal.Entities.AddLine(-5.8246, -0.9335, 5.8246, -0.9335, dsp);
            _rVal.Entities.AddLine(-5.8246, -1.2835, 5.8246, -1.2835, dsp);
            _rVal.Entities.AddLine(-5.8246, -1.6336, 5.8246, -1.6336, dsp);

            // table headers
            txt = @"TRAY\PNO.";
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(-5.2232, -0.2917), txt, 0.125, dxxMTextAlignments.MiddleCenter));

            txt = @"\A2;FUNCTIONAL\PAREA (";
            txt += aDrawing.DrawingUnits == uppUnitFamilies.English ? "FT" : "CM";
            txt += @"{\H0.65x;\S2^;})";
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(-3.8347, -0.2917), txt, 0.125, dxxMTextAlignments.MiddleCenter));

            txt = @"FUNCTIONAL\PFP (%)";
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(-2.4474, -0.2917), txt, 0.125, dxxMTextAlignments.MiddleCenter));

            txt = @"\A2;ACTUAL PERF.\PAREA (";
            txt += aDrawing.DrawingUnits == uppUnitFamilies.English ? "FT" : "CM";
            txt += @"{\H0.65x;\S2^;})";
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(-0.9977, -0.2917), txt, 0.125, dxxMTextAlignments.MiddleCenter));

            txt = @"PERF. DIA. \P(";
            txt += aDrawing.DrawingUnits == uppUnitFamilies.English ? "INCHES" : "MM";
            txt += ")";
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(0.4583, -0.2917), txt, 0.125, dxxMTextAlignments.MiddleCenter));

            txt = @"CALC. NO. OPEN\PHOLES PER TRAY";
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(1.931, -0.2917), txt, 0.125, dxxMTextAlignments.MiddleCenter));

            txt = @"\A2;MECH. ACTIVE\PAREA (";
            txt += aDrawing.DrawingUnits == uppUnitFamilies.English ? "FT" : "CM";
            txt += @"{\H0.65x;\S2^;})";
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(3.5462, -0.2917), txt, 0.125, dxxMTextAlignments.MiddleCenter));

            txt = @"PERF. FRACT.\POPEN (%)";
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(5.0599, -0.2917), txt, 0.125, dxxMTextAlignments.MiddleCenter));

            // table text
            txt = aAssy.SpanName();
            dxeText attrib = tool.Create_Text(new dxfVector(-5.2232, -0.7584), txt, 0.125, dxxMTextAlignments.MiddleCenter, aTextType: dxxTextTypes.AttDef, aAttributeTag: "TRAYNO", aAttributePrompt: "Tray Number");
            _rVal.Entities.Add(attrib);

            txt = string.Format("{0:#,0.000}", funcArea * multi);
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(-3.8347, -0.7584), txt, 0.125, dxxMTextAlignments.MiddleCenter, aTextType: dxxTextTypes.AttDef, aAttributeTag: "FUNCTAREA", aAttributePrompt: "Functional Area"));

            txt = string.Format("{0:0.00}", aFP);
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(-2.4474, -0.7584), txt, 0.125, dxxMTextAlignments.MiddleCenter, aTextType: dxxTextTypes.AttDef, aAttributeTag: "FUNCTFP", aAttributePrompt: "Functional FP%"));

            txt = string.Format("{0:#,0.000}", funcArea * multi * (aFP / 100));
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(-0.9977, -0.7584), txt, 0.125, dxxMTextAlignments.MiddleCenter, aTextType: dxxTextTypes.AttDef, aAttributeTag: "PERFAREA", aAttributePrompt: "Perforation Area"));

            txt = string.Format("{0:#.0###}", aAssy.Deck.DP * multi2);
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(0.4583, -0.7584), txt, 0.125, dxxMTextAlignments.MiddleCenter, aTextType: dxxTextTypes.AttDef, aAttributeTag: "PERFDIA", aAttributePrompt: "Perforation Diameter"));

            txt = aAssy.Deck.DP != 0 ? string.Format("{0:#,0}", (int)(funcArea * (aFP / 100) / (Math.PI * Math.Pow(aAssy.Deck.DP / 2, 2)))) : "";
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(1.931, -0.7584), txt, 0.125, dxxMTextAlignments.MiddleCenter, aTextType: dxxTextTypes.AttDef, aAttributeTag: "HOLECNT", aAttributePrompt: "Perforations Per Tray"));

            txt = string.Format("{0:#,0.000}", mechArea * multi);
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(3.5462, -0.7584), txt, 0.125, dxxMTextAlignments.MiddleCenter, aTextType: dxxTextTypes.AttDef, aAttributeTag: "MECHAREA", aAttributePrompt: "Mechanical Active Area"));

            txt = string.Format("{0:0.00}", aFP * RFactor);
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(5.0599, -0.7584), txt, 0.125, dxxMTextAlignments.MiddleCenter, aTextType: dxxTextTypes.AttDef, aAttributeTag: "PERFFRAC", aAttributePrompt: "Per. Fraction (%Open)"));

            return _rVal;
        }

        private dxfBlock CreateBlock_MDTable_Startup(string aBlockName, dxfImage aImage, mdTrayAssembly aAssy, uopDocDrawing aDrawing)
        {
            Status = $"Creating Block '{aBlockName}'";


            dxfBlock _rVal = new(aBlockName);
            string txt;
            double multi;
            double multi2;
            double aVal;
            dxoEntityTool tool = aImage.EntityTool;
            mdStartupSpout aSU = null;
            int cnt;
            if (aDrawing.DrawingUnits == uppUnitFamilies.English)
            {
                multi = 1;
                multi2 = 1;
            }
            else
            {
                multi = Math.Pow(2.54, 2);
                multi2 = 25.4;
            }
            cnt = aAssy.StartupSpouts.TotalCount;
            if (cnt > 0)
            {
                aSU = aAssy.StartupSpouts.Item(1);
            }

            _rVal.Name = aBlockName;

            dxfDisplaySettings dsp = new("BORDER", dxxColors.LightGrey, dxfLinetypes.Continuous);
            // border
            _rVal.Entities.AddPolyline("(-5.8246,-0.5834)¸(-5.8246,0.0)¸(5.8246,0.0)¸(5.8246,-1.9836)¸(-5.8246,-1.9836)¸(-5.8246,-0.5834)¸(5.8246,-0.5834)", false, dsp.Copy("HEAVY", dxxColors.ByLayer));
            // Vertical lines
            _rVal.Entities.AddLine(-4.6218, 0, -4.6218, -1.983, dsp);
            _rVal.Entities.AddLine(-2.1844, 0, -2.1844, -1.983, dsp);
            _rVal.Entities.AddLine(0.6261, 0, 0.6261, -1.983, dsp);
            _rVal.Entities.AddLine(3.2981, 0, 3.2981, -1.983, dsp);
            // horizontal lines
            _rVal.Entities.AddLine(-5.8246, -0.9335, 5.8246, -0.9335, dsp);
            _rVal.Entities.AddLine(-5.8246, -1.2835, 5.8246, -1.2835, dsp);
            _rVal.Entities.AddLine(-5.8246, -1.6336, 5.8246, -1.633, dsp);

            // table headers
            txt = @"TRAY\PNO.";
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(-5.2232, -0.2917), txt, 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));

            txt = @"\A2;STARTUP OPENING\PAREA (";
            txt += aDrawing.DrawingUnits == uppUnitFamilies.English ? "IN" : "MM";
            txt += @"{\H0.65x;\S2^;})";
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(-3.4031, -0.2917), txt, 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));

            txt = @"\A2;AREA OF OPENING (";
            txt += aDrawing.DrawingUnits == uppUnitFamilies.English ? "IN" : "MM";
            txt += @"{\H0.65x;\S2^;})";
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(-0.7792, -0.2917), txt, 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));

            txt = "NUMBER OF OPENINGS\\PPER TRAY";
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(1.9621, -0.2917), txt, 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));

            txt = "OPENING DIMENSIONS\\P(";
            txt += aDrawing.DrawingUnits == uppUnitFamilies.English ? "INCHES" : "MM";
            txt += ")";
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(4.5614, -0.2917), txt, 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));

            // table text
            txt = aAssy.SpanName();
            dxeText atrib = tool.Create_Text(new dxfVector(-5.2232, -0.7584), txt, 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard", aTextType: dxxTextTypes.AttDef, aAttributeTag: "TRAYNO", aAttributePrompt: "Tray Number");
            _rVal.Entities.Add(atrib);


            aVal = aAssy.StartupSpouts.TotalArea * multi;
            txt = string.Format("{0:#,0.000}", aVal);
            atrib = atrib.Clone(new dxfVector(-3.4031, -0.7584), txt);
            atrib.AttributeTag = "SUAREA"; atrib.Prompt = "Total Spout Area";
            _rVal.Entities.Add(atrib);

            //_rVal.Entities.Add(tool.Create_Text(new dxfVector(-3.4031, -0.7584), txt, 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard", aTextType: dxxTextTypes.AttDef, aAttributeTag: "SUAREA", aAttributePrompt: "Total Spout Area"));

            txt = cnt > 0 ? string.Format("{0:#.000}", aSU.Slot.Area * multi) : "-";
            atrib = atrib.Clone(new dxfVector(-0.7792, -0.7584), txt);
            atrib.AttributeTag = "SLTAREA"; atrib.Prompt = "Slot Area";
            _rVal.Entities.Add(atrib);

            //_rVal.Entities.Add(tool.Create_Text(new dxfVector(-0.7792, -0.7584), txt, 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard", aTextType: dxxTextTypes.AttDef, aAttributeTag: "SLTAREA", aAttributePrompt: "Slot Area"));

            txt = string.Format("{0:#,0}", cnt);
            atrib = atrib.Clone(new dxfVector(1.9621, -0.7584), txt);
            atrib.AttributeTag = "SUCOUNT"; atrib.Prompt = "Slot Count";
            _rVal.Entities.Add(atrib);

            //_rVal.Entities.Add(tool.Create_Text(new dxfVector(1.9621, -0.7584), txt, 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard", aTextType: dxxTextTypes.AttDef, aAttributeTag: "SUCOUNT", aAttributePrompt: "Slot Count"));

            txt = cnt > 0 ? $"{string.Format("{0:#.000#}", aSU.Length * multi2)} X {string.Format("{0:#.000#}", aSU.Height * multi2)}" : "-";
            atrib = atrib.Clone(new dxfVector(4.5614, -0.7584), txt);
            atrib.AttributeTag = "SUDIMS"; atrib.Prompt = "Spout Dimensions";
            _rVal.Entities.Add(atrib);


            //_rVal.Entities.Add(tool.Create_Text(new dxfVector(4.5614, -0.7584), txt, 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard", aTextType: dxxTextTypes.AttDef, aAttributeTag: "SUDIMS", aAttributePrompt: "Spout Dimensions"));

            return _rVal;
        }

        private dxfBlock CreateBlock_MDTable_DCTable(string aBlockName, dxfImage aImage, mdTrayAssembly aAssy, uopDocDrawing aDrawing)
        {
            Status = $"Creating Block '{aBlockName}'";


            dxfBlock _rVal = new(aBlockName);
            string txt;
            double multi;
            double aVal;
            double wd1;
            double wd2;
            dxoEntityTool tool = aImage.EntityTool;
            wd1 = 1.25;
            wd2 = 1.725;

            multi = aDrawing.DrawingUnits == uppUnitFamilies.English ? 1 : Math.Pow(2.54, 2);

            _rVal.Name = aBlockName;
            dxfDisplaySettings dsp = new("BORDER", dxxColors.LightGrey, dxfLinetypes.Continuous);
            // border
            _rVal.Entities.AddPolyline($"({-wd1},-0.5834)¸({-wd1},0.0)¸({wd2},0.0)¸({wd2},-1.9836)¸({-wd1},-1.9836)¸({-wd1},-0.5834)¸({wd2},-0.5834)", false, dsp.Copy("HEAVY", dxxColors.ByLayer));
            // Vertical lines
            _rVal.Entities.AddLine(-0, 0, 0, -1.9836, dsp);
            // horizontal lines
            _rVal.Entities.AddLine(-wd1, -0.9335, wd2, -0.9335, dsp);
            _rVal.Entities.AddLine(-wd1, -1.2835, wd2, -1.2835, dsp);
            _rVal.Entities.AddLine(-wd1, -1.6336, wd2, -1.6336, dsp);

            // table headers
            txt = "TRAY\\PNO.";
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(-wd1 / 2, -0.2917), txt, 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));

            txt = "\\A2;TOTAL SPOUT AREA\\PPER TRAY (";
            txt += aDrawing.DrawingUnits == uppUnitFamilies.English ? "IN" : "MM";
            txt += @"{\H0.65x;\S2^;})";

            _rVal.Entities.Add(tool.Create_Text(new dxfVector(wd2 / 2, -0.2917), txt, 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));

            // table text
            txt = aAssy.SpanName();
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(-wd1 / 2, -0.7584), txt, 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard", aTextType: dxxTextTypes.AttDef, aAttributeTag: "TRAYNO", aAttributePrompt: "Tray Number"));

            aVal = aAssy.TotalSpoutArea * multi;
            txt = string.Format("{0:#,0.000}", aVal);
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(wd2 / 2, -0.7584), txt, 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard", aTextType: dxxTextTypes.AttDef, aAttributeTag: "SPOUTAREA", aAttributePrompt: "Total Tray Spout Area"));

            _rVal.Entities.Move(-(wd2 - wd1) / 2);

            return _rVal;
        }

        private dxfBlock CreateBlock_MDTable_DesignData(string aBlockName, dxfImage aImage, mdTrayAssembly aAssy, uopDocDrawing aDrawing, double aMechanicalArea = 0, double aFunctionalArea = 0)
        {
            Status = $"Creating Block '{aBlockName}'";


            dxfBlock _rVal = new(aBlockName);
            string txt;
            double multi;
            string uni;
            string fmat;
            double RFactor = 0;
            double mechArea = aMechanicalArea;
            double funcArea = aFunctionalArea;
            double y1;
            double X1;
            double x2 = 0;
            dxoEntityTool tool = aImage.EntityTool;

            if (mechArea <= 0 || funcArea <= 0)
            {
                mechArea = aAssy.MechanicalActiveArea;
                funcArea = aAssy.FunctionalActiveArea;
            }
            if (mechArea != 0) RFactor = funcArea / mechArea;

            if (aDrawing.DrawingUnits == uppUnitFamilies.English)
            {
                multi = 1;
                uni = " IN.";
                fmat = "{0:0.000#}";
            }
            else
            {
                multi = 25.4;
                uni = " MM";
                fmat = "{0:#,0.0#}";
            }

            _rVal.Name = aBlockName;

            // border
            _rVal.Entities.AddPolyline("(3.3313,-2.2958)¸(3.5647,-2.0624)¸(3.5647,-0.2334)¸(3.3313,0.0)¸(-3.3313,0.0)¸(-3.5647,-0.2334)¸(-3.5647,-2.0624)¸(-3.3313,-2.2958)¸(3.3313,-2.2958)", false, new dxfDisplaySettings("HEAVY", dxxColors.ByLayer, dxfLinetypes.Continuous));

            // table headers
            txt = "\\LADDITIONAL DESIGN DATA";
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(0, -0.25), txt, 0.1875, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));

            y1 = -0.65;
            X1 = -3.0126;
            txt = "COLUMN INSIDE DIAMETER";
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), txt, 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard"));
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(x2, y1), ":", 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard"));

            txt = "RING INSIDE DIAMETER";
            y1 -= 0.25;
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), txt, 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard"));
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(x2, y1), ":", 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard"));

            txt = "TRAY DIAMETER";
            y1 -= 0.25;
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), txt, 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard"));
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(x2, y1), ":", 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard"));

            txt = "BOLT CIRCLE DIAMETER";
            y1 -= 0.25;
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), txt, 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard"));
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(x2, y1), ":", 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard"));

            txt = "R-FACTOR USED";
            y1 -= 0.25;
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), txt, 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard"));
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(x2, y1), ":", 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard"));

            txt = "TRAY SPACING";
            y1 -= 0.25;
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), txt, 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard"));
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(x2, y1), ":", 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard"));

            //=======================================================================

            // table text
            y1 = -0.65;
            X1 = 0.2;
            txt = string.Format(fmat, aAssy.ShellID * multi) + uni;
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), txt, 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard", aTextType: dxxTextTypes.AttDef, aAttributeTag: "COLDIA", aAttributePrompt: "Column Diameter"));

            y1 -= 0.25;
            txt = string.Format(fmat, aAssy.RingID * multi) + uni;
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), txt, 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard", aTextType: dxxTextTypes.AttDef, aAttributeTag: "RINGDIA", aAttributePrompt: "Ring Diameter"));

            y1 -= 0.25;
            txt = string.Format(fmat, aAssy.DeckRadius * 2 * multi) + uni;
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), txt, 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard", aTextType: dxxTextTypes.AttDef, aAttributeTag: "TRAYDIA", aAttributePrompt: "Tray Diameter"));

            y1 -= 0.25;
            txt = string.Format(fmat, aAssy.RingClipRadius * 2 * multi) + uni;
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), txt, 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard", aTextType: dxxTextTypes.AttDef, aAttributeTag: "BCDIA", aAttributePrompt: "Bolt Circle Diameter"));

            y1 -= 0.25;
            txt = string.Format("{0:0.000#}", RFactor);
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), txt, 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard", aTextType: dxxTextTypes.AttDef, aAttributeTag: "RFACT", aAttributePrompt: "R-Factor"));

            y1 -= 0.25;
            txt = string.Format(fmat, aAssy.RingSpacing * multi) + uni;
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), txt, 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard", aTextType: dxxTextTypes.AttDef, aAttributeTag: "TRAYSPC", aAttributePrompt: "Tray Spacing"));

            return _rVal;
        }

        private dxfBlock CreateBlock_MDTable_SpoutTable(string aBlockName, dxfImage aImage, mdTrayAssembly aAssy, uopDocDrawing aDrawing)
        {
            Status = $"Creating Block '{aBlockName}'";


            dxfBlock _rVal = new(aBlockName);

            string uni = aDrawing.DrawingUnits == uppUnitFamilies.English ? "IN." : "MM";
            colMDDowncomers aDCs = aAssy.Downcomers;
            mdDowncomer aDC;
            List<mdSpoutGroup> aSGs = aAssy.SpoutGroups.GetByVirtual(aVirtualValue: false);


            int precis = aDrawing.DrawingUnits == uppUnitFamilies.English ? 4 : 1;
            uopHoles aSpts;
            uopHole aSpt;
            dxoEntityTool tool = aImage.EntityTool;
            dxoDimStyle dstyle = aImage.DimStyle();
            List<double> xVals = new List<double>() { -3.0726, -2.0885, -0.9385, 0.268, 1.312, 2.2721, 3.1507 };


            _rVal.Name = aBlockName;
            dxfDisplaySettings dsp = new("HEAVY", dxxColors.ByLayer, dxfLinetypes.Continuous);
            // border
            dxePolyline aPl = _rVal.Entities.AddPolyline("(-3.3313,0.0)¸(-3.5647,-0.2334)¸(-3.5647,-8.9952)¸(-3.3313,-9.2286)¸(3.3313,-9.2286)¸(3.5647,-8.9952)¸(3.5647,-0.2334)¸(3.3313,0.0)", true, dsp);

            // horizontal lines
            _rVal.Entities.AddLine(-3.5647, -0.5553, 3.5647, -0.5553, dsp);
            _rVal.Entities.AddLine(-3.5647, -1.2759, 3.5647, -1.2759, dsp);

            // vertical lines
            dxeLine l1 = _rVal.Entities.AddLine(-2.5805, -0.5553, -2.5805, -9.2286, dsp);
            dxeLine l2 = _rVal.Entities.AddLine(-1.5964, -0.5553, -1.5964, -9.2286, dsp);
            dxeLine l3 = _rVal.Entities.AddLine(-0.2806, -0.5553, -0.2806, -9.2286, dsp);
            dxeLine l4 = _rVal.Entities.AddLine(0.8166, -0.5553, 0.8166, -9.2286, dsp);
            dxeLine l5 = _rVal.Entities.AddLine(1.8074, -0.5553, 1.8074, -9.2286, dsp);
            dxeLine l6 = _rVal.Entities.AddLine(2.7367, -0.5553, 2.7367, -9.2286, dsp);

            // table headers
            string txt = "\\LSPOUT INFORMATION TABLE";
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(0, -0.3206), txt, 0.1875, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));

            double y1 = -0.9156;
            txt = "DNCMR.\\PPART #";
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(xVals[0], y1), txt, 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));

            txt = "DNCMR#,\\PGROUP#";
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(xVals[1], y1), txt, 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));


            if (!aAssy.MetricSpouting)
            {
                txt = dstyle.FormatNumber(aAssy.Downcomer().SpoutDiameter, bApplyLinearMultiplier: aDrawing.DrawingUnits != uppUnitFamilies.English, aPrecision: precis);

                txt = $"NUMBER OF\\P%%C{txt} HOLES";
            }
            else
            {
                txt = dstyle.FormatNumber(aAssy.Downcomer().SpoutDiameter * 25.4, bApplyLinearMultiplier: false, aPrecision: 0);
                txt = $"NUMBER OF\\P%%C{txt} mm HOLES";
            }

            _rVal.Entities.Add(tool.Create_Text(new dxfVector(xVals[2], y1), txt, 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));

            txt = "NO. OF\\PSLOTS";
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(xVals[3], y1), txt, 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));

            txt = "PATTERN\\PNAME";
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(xVals[4], y1), txt, 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));

            txt = $"Y PITCH\\P({uni})";
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(xVals[5], y1), txt, 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));

            txt = "SLOT\\PLENGTH";
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(xVals[6], y1), txt, 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));

            //=======================================================================

            // table text
            y1 = -1.5783;


            foreach (mdSpoutGroup aSG in aSGs)
            {
                if (aSG.SpoutCount(aAssy) > 0)
                {
                    aDC = aDCs.Item(aSG.DowncomerIndex);

                    if (aSG.PatternType == uppSpoutPatterns.SStar)
                    {
                        aSpts = aSG.Spouts;
                        for (int k = 1; k <= aSpts.Count; k++)
                        {
                            aSpt = aSpts.Item(k);
                            for (int j = 1; j <= 7; j++)
                            {
                                txt = "-";
                                switch (j)
                                {
                                    case 1:
                                        txt = aDC.PartNumber;
                                        break;
                                    case 2:
                                        txt = aSG.Handle;
                                        break;
                                    case 3:
                                        txt = aSpt.HoleType == uppHoleTypes.Hole ? "1" : "-";
                                        break;
                                    case 4:
                                        txt = aSpt.HoleType == uppHoleTypes.Hole ? "-" : "1";
                                        break;
                                    case 5:
                                        txt = "*S*";
                                        break;
                                    case 6:
                                        txt = dstyle.FormatNumber(aSG.VerticalPitch, aPrecision: precis);
                                        break;
                                    case 7:
                                        txt = aSpt.HoleType == uppHoleTypes.Hole ? "-" : dstyle.FormatNumber(aSpt.Length, aPrecision: precis);
                                        break;
                                    default:
                                        break;
                                }
                                _rVal.Entities.Add(tool.Create_Text(new dxfVector(xVals[j - 1], y1), txt, 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));
                            }
                            y1 -= 0.1925;
                        }
                    }
                    else
                    {
                        for (int j = 1; j <= 7; j++)
                        {
                            txt = "-";
                            switch (j)
                            {
                                case 1:
                                    txt = aDC.PartNumber;
                                    break;
                                case 2:
                                    txt = aSG.Handle;
                                    break;
                                case 3:
                                    if (aSG.PatternType < uppSpoutPatterns.S3)
                                    {
                                        txt = aSG.SpoutCount().ToString();
                                    }
                                    break;
                                case 4:
                                    if (aSG.PatternType >= uppSpoutPatterns.S3)
                                    {
                                        txt = aSG.SpoutCount().ToString();
                                    }
                                    break;
                                case 5:
                                    txt = aSG.PatternName; // it was PatternName(true) in VB
                                    break;
                                case 6:
                                    txt = dstyle.FormatNumber(aSG.VerticalPitch, aPrecision: precis);
                                    break;
                                case 7:
                                    if (aSG.PatternType >= uppSpoutPatterns.S3)
                                    {
                                        txt = dstyle.FormatNumber(aSG.SlotLength, aPrecision: precis);
                                    }
                                    break;
                                default:
                                    break;
                            }
                            _rVal.Entities.Add(tool.Create_Text(new dxfVector(xVals[j - 1], y1), txt, 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));
                        }
                        y1 -= 0.1925;
                    }

                }
            }
            aPl.Vertex(3).Y = y1;
            aPl.Vertex(4).Y = y1 - 0.2334;
            aPl.Vertex(5).Y = y1 - 0.2334;
            aPl.Vertex(6).Y = y1;
            l1.EndPt.Y = y1 - 0.2334;
            l2.EndPt.Y = y1 - 0.2334;
            l3.EndPt.Y = y1 - 0.2334;
            l4.EndPt.Y = y1 - 0.2334;
            l5.EndPt.Y = y1 - 0.2334;
            l6.EndPt.Y = y1 - 0.2334;

            return _rVal;
        }

        private dxfBlock CreateBlock_MDTable_ECMDData(string aBlockName, dxfImage aImage, mdTrayAssembly aAssy, uopDocDrawing aDrawing, double aMechanicalArea = 0, double aFunctionalArea = 0, bool bCrossOut = false)
        {
            Status = $"Creating Block '{aBlockName}'";


            dxfBlock _rVal = new();
            string txt;
            double mLin;
            double mAre;
            double aVal = 0;

            string uLin;
            string uAre;
            bool bNoData = !aAssy.DesignFamily.IsEcmdDesignFamily();
            double X1;
            double y1;
            mdSlot aFS;
            int cnt = 0;
            double mechArea;
            double funcArea;
            dxxColors aClr;
            mdSlotZones aSZs;
            double x2;
            dxoEntityTool tool = aImage.EntityTool;


            mechArea = aMechanicalArea;
            funcArea = aFunctionalArea;
            aSZs = aAssy.SlotZones;
            if (aSZs.FunctionArea != funcArea)
            {
                aAssy.Invalidate(uppPartTypes.FlowSlotZone);
                aSZs = aAssy.SlotZones;
            }

            if (!bNoData)
            {

                aFS = aAssy.FlowSlot;

                cnt = aSZs.TotalSlotCount(aAssy);
                if (mechArea <= 0 || funcArea <= 0)
                {
                    mechArea = aAssy.MechanicalActiveArea;
                    funcArea = aAssy.FunctionalActiveArea;
                }
            }
            else
            {
                bCrossOut = true;
            }

            if (aDrawing.DrawingUnits == uppUnitFamilies.English)
            {
                mLin = 1;
                mAre = 1.0 / 144;
                uLin = " IN.";
                mechArea /= 144;
                funcArea /= 144;
                uAre = " FT.";
            }
            else
            {
                mLin = 25.4;
                mAre = Math.Pow(2.54, 2);
                uLin = " MM";
                uAre = " CM";
                mechArea *= Math.Pow(2.54, 2);
                funcArea *= Math.Pow(2.54, 2);
            }

            _rVal.Name = aBlockName;
            dxfDisplaySettings dsp = new dxfDisplaySettings("HEAVY", dxxColors.ByLayer, dxfLinetypes.Continuous);
            // border
            colDXFVectors box = new colDXFVectors("(3.3147,0.0)¸(-3.3147,0.0)¸(-3.5647,-0.25)¸(-3.5647,-2.2009)¸(-3.3147,-2.5435)¸(3.3147,-2.5435)¸(3.5647,-2.2009)¸(3.5647,-0.25)");
            _rVal.Entities.AddPolyline(box, true, aDisplaySettings: dsp);
            if (bCrossOut)
            {
                dxfRectangle rect = box.BoundingRectangle();
                _rVal.Entities.Add(new dxeLine(rect.TopLeft, rect.BottomRight, dsp));
                _rVal.Entities.Add(new dxeLine(rect.BottomLeft, rect.TopRight, dsp));
            }


            // table headers
            txt = "\\LECMD TRAY DESIGN DATA";
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(0, -0.25), txt, 0.1875, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));

            y1 = -0.65;
            X1 = -3.0126;
            x2 = 0.41;
            txt = "BAFFLE HEIGHT (FROM TOP OF TRAY DECK)";
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), txt, 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard"));
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(x2, y1), ":", 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard"));

            y1 -= 0.25;
            txt = "SLOT TYPE";
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), txt, 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard"));
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(x2, y1), ":", 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard"));

            y1 -= 0.25;
            txt = "FS% REQUIRED";
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), txt, 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard"));
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(x2, y1), ":", 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard"));

            y1 -= 0.25;
            txt = "NUMBER OF SLOTS REQURED FOR TRAY";
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), txt, 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard"));
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(x2, y1), ":", 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard"));

            y1 -= 0.25;
            txt = "ACTUAL NO. OF SLOTS PER TRAY";
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), txt, 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard"));
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(x2, y1), ":", 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard"));

            y1 -= 0.25;
            txt = $"SLOTS/MECH. AREA. (SQ.{uAre})";
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), txt, 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard"));
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(x2, y1), ":", 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard"));

            y1 -= 0.25;
            txt = $"SLOTS/FUNCT. AREA. (SQ.{uAre})";
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), txt, 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard"));
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(x2, y1), ":", 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard"));

            //=======================================================================
            y1 = -0.65;
            aClr = dxxColors.ByLayer;
            if (bNoData)
            {
                txt = "-";
                X1 = 0.95;
            }
            else
            {
                X1 = 0.65;

                txt = string.Format("{0:0.000}", aAssy.DesignOptions.CDP * mLin) + uLin;
            }
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), txt, 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard", aColor: aClr, aTextType: dxxTextTypes.AttDef, aAttributeTag: "BAFFHT", aAttributePrompt: "Baffle Height"));

            y1 -= 0.25;
            aClr = dxxColors.ByLayer;
            if (!bNoData)
            {
                txt = aAssy.Deck.PropValGet("SlotType", out _, bDecodedValue: true).ToUpper(); // in VB it was "Deck.Part.PropValGet"
            }
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), txt, 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard", aColor: aClr, aTextType: dxxTextTypes.AttDef, aAttributeTag: "SLOTTYP", aAttributePrompt: "Slot Type"));

            y1 -= 0.25;
            aClr = dxxColors.ByLayer;
            if (!bNoData)
            {
                aVal = aAssy.Deck.SlottingPercentage;
                txt = string.Format("{0:0.00}", aVal) + " %";
                if (aVal <= 0)
                {
                    aClr = dxxColors.Red;
                }
            }
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), txt, 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard", aColor: aClr, aTextType: dxxTextTypes.AttDef, aAttributeTag: "FSPCT", aAttributePrompt: "Function Slotting Percentage"));

            y1 -= 0.25;
            aClr = dxxColors.ByLayer;
            if (!bNoData)
            {
                aAssy.PanelSlotCounts(out int aLVal, out _, out _, out _);
                txt = aLVal.ToString();
                if (aVal <= 0)
                {
                    aClr = dxxColors.Red;
                }
            }
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), txt, 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard", aColor: aClr, aTextType: dxxTextTypes.AttDef, aAttributeTag: "SLTPERTRAY", aAttributePrompt: "Number of Slots Required"));

            y1 -= 0.25;
            aClr = dxxColors.ByLayer;
            if (!bNoData)
            {
                txt = cnt.ToString();
                if (cnt <= 0)
                {
                    aClr = dxxColors.Red;
                }
            }
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), txt, 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard", aColor: aClr, aTextType: dxxTextTypes.AttDef, aAttributeTag: "ACTSLTS", aAttributePrompt: "Actual Number of Slots"));

            y1 -= 0.25;
            aClr = dxxColors.ByLayer;
            if (!bNoData)
            {
                aVal = mechArea != 0 ? cnt / mechArea : 0;
                txt = string.Format("{0:0.000}", aVal);
                if (aVal <= 0)
                {
                    aClr = dxxColors.Red;
                }
            }
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), txt, 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard", aColor: aClr, aTextType: dxxTextTypes.AttDef, aAttributeTag: "SLTSPERMECHAREA", aAttributePrompt: "Slots Per Mechanical Area"));

            y1 -= 0.25;
            aClr = dxxColors.ByLayer;
            if (!bNoData)
            {
                aVal = funcArea != 0 ? cnt / funcArea : 0;
                txt = string.Format("{0:0.000}", aVal);
                if (aVal <= 0)
                {
                    aClr = dxxColors.Red;
                }
            }
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), txt, 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard", aColor: aClr, aTextType: dxxTextTypes.AttDef, aAttributeTag: "SLTSPERFUNCTAREA", aAttributePrompt: "Slots Per Functional Area"));

            return _rVal;
        }

        private dxfBlock CreateBlock_MDTable_APData(string aBlockName, dxfImage aImage, mdTrayAssembly aAssy, uopDocDrawing aDrawing, bool bCrossOut = false)
        {
            Status = $"Creating Block '{aBlockName}'";


            dxfBlock _rVal = new(aBlockName);
            string txt;

            string uni;
            double y1;
            double X1;
            double x2;
            colUOPParts Pans;
            mdAPPan aPan = null;
            bool bNoData = false;
            dxxColors clr;
            dxoEntityTool tool = aImage.EntityTool;

            Pans = aAssy.APPans;
            if (Pans.Count <= 0)
            {
                bNoData = true;
            }
            else
            {
                aPan = (mdAPPan)Pans.Item(1);
            }

            uni = aDrawing.DrawingUnits == uppUnitFamilies.English ? " IN." : " MM";
            _rVal.Name = aBlockName;

            // border
            colDXFVectors box = new colDXFVectors("(3.3313,0.0)¸(-3.3313,0.0)¸(-3.5647,-0.2334)¸(-3.5647,-1.70565)¸(-3.3313,-1.9390)¸(3.3313,-1.9390)¸(3.5647,-1.70565)¸(3.5647,-0.2334)");
            dxfDisplaySettings dsp = new dxfDisplaySettings("HEAVY", dxxColors.ByLayer, dxfLinetypes.Continuous);
            _rVal.Entities.AddPolyline(box, true, dsp);
            if (bCrossOut)
            {
                dxfRectangle rect = box.BoundingRectangle();
                _rVal.Entities.Add(new dxeLine(rect.TopLeft, rect.BottomRight, dsp));
                _rVal.Entities.Add(new dxeLine(rect.BottomLeft, rect.TopRight, dsp));
            }
            // table headers
            txt = "\\LANTI-PENETRATION PANS";
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(0, -0.25), txt, 0.1875, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));

            y1 = -0.65;
            X1 = -3.0126;
            x2 = 0.41;
            txt = "DISTANCE BELOW DOWNCOMER";
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), txt, 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard"));
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(x2, y1), ":", 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard"));

            txt = "HEIGHT ABOVE DECK";
            y1 -= 0.25;
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), txt, 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard"));
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(x2, y1), ":", 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard"));

            txt = "FP%";
            y1 -= 0.25;
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), txt, 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard"));
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(x2, y1), ":", 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard"));

            txt = "PERF. DIA.";
            y1 -= 0.25;
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), txt, 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard"));
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(x2, y1), ":", 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard"));

            txt = "MIN. DISTANCE FROM LIP TO WEIR";
            y1 -= 0.25;
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), txt, 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard"));
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(x2, y1), ":", 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard"));

            //=======================================================================

            // table text
            y1 = -0.65;
            clr = dxxColors.Undefined;
            if (bNoData)
            {
                txt = "-";
                X1 = 1.4;
            }
            else
            {
                X1 = 0.9;

                txt = aImage.FormatNumber(aPan.Height) + uni;
            }

            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), txt, 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard", aTextType: dxxTextTypes.AttDef, aAttributeTag: "DISTBELOW", aAttributePrompt: "Distance Below DC"));

            y1 -= 0.25;
            if (!bNoData)
            {
                txt = aImage.FormatNumber(aAssy.DesignOptions.FEDorAPPHeight) + uni;
            }
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), txt, 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard", aTextType: dxxTextTypes.AttDef, aAttributeTag: "APHEIGHT", aAttributePrompt: "Deck Clearance"));

            y1 -= 0.25;
            if (!bNoData)
            {
                txt = string.Format("{0:0.00#}", aPan.PercentOpen) + "%";
            }
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), txt, 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard", aTextType: dxxTextTypes.AttDef, aAttributeTag: "APPERFFRAC", aAttributePrompt: "Percent Open"));

            y1 -= 0.25;
            if (!bNoData)
            {
                txt = aImage.FormatNumber(aPan.PerforationDiameter) + uni;
            }
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), txt, 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard", aTextType: dxxTextTypes.AttDef, aAttributeTag: "APPERFDIA", aAttributePrompt: "Perforation Diameter"));

            y1 -= 0.25;
            if (!bNoData)
            {
                txt = "?????";
                clr = dxxColors.Red;
            }

            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), txt, 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard", aColor: clr, aTextType: dxxTextTypes.AttDef, aAttributeTag: "LIPTOWEIR", aAttributePrompt: "Min. Lip To Weir"));

            return _rVal;
        }

        private dxfBlock CreateBlock_MDTable_FEDData(string aBlockName, dxfImage aImage, mdTrayAssembly aAssy, uopDocDrawing aDrawing)
        {
            Status = $"Creating Block '{aBlockName}'";


            dxfBlock _rVal = new(aBlockName);
            string txt;
            double y1;
            double X1;
            double x2;
            colUOPParts Pans = aAssy.APPans;
            mdAPPan aPan = Pans.Count > 0 ? (mdAPPan)Pans.Item(1) : null;
            dxfDisplaySettings dSets = aImage.GetDisplaySettings(dxxEntityTypes.DimLinearV);
            dxeSolid aSld = new(dxfVector.Zero, new dxfVector(-0.1, 0.0167), new dxfVector(-0.1, -0.0167), aDisplaySettings: dSets);
            dxoEntityTool tool = aImage.EntityTool;
            _rVal.Name = aBlockName;
            dxfDisplaySettings dsp = new("HEAVY", dxxColors.ByLayer, dxfLinetypes.Continuous);
            // border
            _rVal.Entities.AddPolyline("(2.3022,-0.1945)¸(2.1077,0.0)¸(-2.1077,0.0)¸(-2.3022,-0.1945)¸(-2.3022,-4.6171)¸(-2.1077,-4.8116)¸(2.1077,-4.8116)¸(2.3022,-4.6171)¸(2.3022,-0.1945)¸(2.3022,-4.6171)", true, dsp);

            // sketch
            _rVal.Entities.AddPolyline("(-1.0535,-0.1429)¸(-0.3735,-0.8229)¸(0.2967,-0.8229)¸(0.9767,-0.1429)", false, dsp);
            _rVal.Entities.AddPolyline("(-0.7184,-0.5900)¸(-0.0384,-1.270)¸(0.6416,-0.590)", false, dsp);
            _rVal.Entities.AddLine(-0.9679, -1.7392, 1.3361, -1.7392, dsp);

            // dims
            dsp = dsp.Copy(dSets.LayerName);
            _rVal.Entities.AddArc(-0.0384, -1.27, 0.3403, 151, 196, aDisplaySettings: dsp);
            _rVal.Entities.AddArc(-0.0384, -1.27, 0.3403, 344, 29, aDisplaySettings: dsp);
            _rVal.Entities.AddSolid("(-0.3539,-1.1018)¸(-0.3250,-1.1183)¸(-0.2900,-1.0231)", dsp);
            _rVal.Entities.AddSolid("(0.2771,-1.1018)¸(0.2481,-1.1183)¸(0.2131,-1.0231)", dsp);
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(-0.0384, -0.9935), "A", 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));

            _rVal.Entities.AddLine(0.012, -1.3078, 0.3839, -1.5865, dsp);
            _rVal.Entities.AddLine(0.3471, -0.8606, 0.7191, -1.1394, dsp);
            _rVal.Entities.AddLine(0.2736, -1.6288, 0.2136, -1.7088, dsp);
            _rVal.Entities.AddLine(0.7286, -1.0216, 0.7886, -0.9416, dsp);
            _rVal.Entities.AddSolid("(0.2602,-1.6188)¸(0.2869,-1.6388)¸(0.3335,-1.5488)", dsp.Copy(dSets.LayerName));
            _rVal.Entities.AddSolid("(0.7153,-1.0116)¸(0.7420,-1.0316)¸(0.6686,-1.1016)", dsp.Copy(dSets.LayerName));
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(0.5011, -1.3252), "B", 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));

            _rVal.Entities.AddLine(-0.3735, -0.7599, -0.3735, -0.2333, dsp);
            _rVal.Entities.AddLine(0.2967, -0.7599, 0.2967, -0.2333, dsp);
            _rVal.Entities.AddLine(-0.2735, -0.2963, -0.1426, -0.2963, dsp);
            _rVal.Entities.AddLine(0.1967, -0.2963, 0.0657, -0.2963, dsp);
            _rVal.Entities.Add(aSld.Rotated(180, -0.3735, -0.2963));
            _rVal.Entities.Add(aSld.Rotated(0, 0.2967, -0.2963));
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(-0.0384, -0.2963), "C", 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));

            _rVal.Entities.AddLine(-0.1014, -1.27, -1.3386, -1.27, dsp);

            _rVal.Entities.AddLine(-1.0309, -1.7392, -1.3386, -1.7392, dsp);
            _rVal.Entities.AddLine(-1.2756, -1.37, -1.2756, -1.4187, dsp);
            _rVal.Entities.Add(aSld.Rotated(270, -1.2756, -1.7392));
            _rVal.Entities.AddLine(-1.2756, -1.6392, -1.2756, -1.5904, dsp);
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(-1.2756, -1.5046), "E", 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));

            _rVal.Entities.AddLine(-0.4365, -0.8229, -0.8489, -0.8229, dsp);
            _rVal.Entities.AddLine(-0.7859, -1.37, -0.7859, -1.47, dsp);
            _rVal.Entities.Add(aSld.Rotated(90, -0.7859, -1.27));
            _rVal.Entities.AddLine(-0.7859, -0.7229, -0.7859, -0.6229, dsp);
            _rVal.Entities.Add(aSld.Rotated(270, -0.7859, -0.8229));
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(-0.7859, -1.0464), "D", 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));

            _rVal.Entities.AddLine(-1.1165, -0.1429, -1.3386, -0.1429, dsp);
            _rVal.Entities.AddLine(-1.2756, -0.2429, -1.2756, -0.5814, dsp);
            _rVal.Entities.Add(aSld.Rotated(90, -1.2756, -0.1429));
            _rVal.Entities.AddLine(-1.2756, -1.17, -1.2756, -0.8314, dsp);
            _rVal.Entities.Add(aSld.Rotated(270, -1.2756, -1.27));
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(-1.2756, -0.7064), "G", 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));

            _rVal.Entities.Add(tool.Create_Text(new dxfVector(0.8701, -1.6427), "TRAY DECK", 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));

            _rVal.Entities.Add(tool.Create_Text(new dxfVector(0, -2.0209), "\\LFLOW ENHANCEMENT DEVICES", 0.1875, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));

            X1 = -0.55;
            x2 = -0.2;
            y1 = -2.35;
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), "A", 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(x2, y1), "=", 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));
            y1 -= 0.25;
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), "B", 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(x2, y1), "=", 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));
            y1 -= 0.25;
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), "C", 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(x2, y1), "=", 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));
            y1 -= 0.25;
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), "D", 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(x2, y1), "=", 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));
            y1 -= 0.25;
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), "E", 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(x2, y1), "=", 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));
            y1 -= 0.25;
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), "F", 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(x2, y1), "=", 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));
            y1 -= 0.25;
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), "G", 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(x2, y1), "=", 0.125, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));

            X1 = 0.2;
            y1 = -2.35;
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), "??", 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard", aTextType: dxxTextTypes.AttDef, aAttributeTag: "FEDADIM", aAttributePrompt: "A"));
            y1 -= 0.25;
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), "??", 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard", aTextType: dxxTextTypes.AttDef, aAttributeTag: "FEDBDIM", aAttributePrompt: "B"));
            y1 -= 0.25;
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), "??", 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard", aTextType: dxxTextTypes.AttDef, aAttributeTag: "FEDCDIM", aAttributePrompt: "C"));
            y1 -= 0.25;
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), "??", 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard", aTextType: dxxTextTypes.AttDef, aAttributeTag: "FEDDIM", aAttributePrompt: "D"));
            y1 -= 0.25;
            txt = aImage.FormatNumber(aAssy.DesignOptions.FEDorAPPHeight);
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), txt, 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard", aTextType: dxxTextTypes.AttDef, aAttributeTag: "FEDEDIM", aAttributePrompt: "(E) Height Above Deck"));
            y1 -= 0.25;
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), "??", 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard", aTextType: dxxTextTypes.AttDef, aAttributeTag: "FEDFDIM", aAttributePrompt: "F"));
            y1 -= 0.25;
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), "??", 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard", aTextType: dxxTextTypes.AttDef, aAttributeTag: "FEDGDIM", aAttributePrompt: "G"));

            X1 = -1.85;
            x2 = 0.35;
            y1 -= 0.25;
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), "TRAY NUMBERS", 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard"));
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(x2, y1), aAssy.SpanName(), 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard", aTextType: dxxTextTypes.AttDef, aAttributeTag: "TRAYNO", aAttributePrompt: "Tray Number"));
            y1 -= 0.25;
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), "PEFORATION DIAMETER", 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard"));
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(x2, y1), "??", 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard", aTextType: dxxTextTypes.AttDef, aAttributeTag: "FEDPERF", aAttributePrompt: "Perf. Diameter"));

            y1 -= 0.25;
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(X1, y1), "FP%", 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard"));
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(x2, y1), "??", 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard", aTextType: dxxTextTypes.AttDef, aAttributeTag: "FEDFP", aAttributePrompt: "(FP%) Percent Open"));

            return _rVal;
        }

        private dxfBlock CreateBlock_MDTable_BlockedAreas(string aBlockName, dxfImage aImage, mdTrayAssembly aAssy, uopDocDrawing aDrawing)
        {
            Status = $"Creating Block '{aBlockName}'";


            dxfBlock _rVal = new(aBlockName);
            string txt;
            string uni;
            string fmat;

            double aArea = 0;

            uopShapes aBlockedAreas = aAssy.BlockedAreas();

            List<uopShape> aPlines = aBlockedAreas.FindAll(x => string.Compare(x.Tag, "VTAB", true) == 0);
            List<uopShape> bPlines = aBlockedAreas.FindAll(x => string.Compare(x.Tag, "VFLANGE", true) == 0);
            dxoEntityTool tool = aImage.EntityTool;
            bool bNoData = aPlines.Count <= 0 && bPlines.Count <= 0;

            if (!bNoData)
            {
                for (int i = 1; i <= aPlines.Count; i++)
                {
                    uopShape barea = aPlines[i - 1];
                    aArea += 2 * barea.Area; // in VB it was aPl.Area(true)
                }
                for (int i = 1; i <= bPlines.Count; i++)
                {
                    uopShape barea = bPlines[i - 1];
                    aArea += 2 * barea.Area; // in VB it was aPl.Area(true)
                }
            }

            if (aDrawing.DrawingUnits == uppUnitFamilies.English)
            {
                uni = " SQR. FT.";
                fmat = "{0:0.000#}";
                aArea /= 144;
            }
            else
            {
                fmat = "{0:#,0.0#}";
                uni = " SQR. CM.";
                aArea /= Math.Pow(2.54, 2);
            }

            _rVal.Name = aBlockName;

            // border
            dxfDisplaySettings dsp = new dxfDisplaySettings("HEAVY", dxxColors.ByLayer, dxfLinetypes.Continuous);

            colDXFVectors box = new colDXFVectors("(3.3147,0.0)¸(-3.3147,0.0)¸(-3.5647,-0.25)¸(-3.5647,-1.2009)¸(-3.3147,-1.5435)¸(3.3147,-1.5435)¸(3.5647,-1.2009)¸(3.5647,-0.25)");
            _rVal.Entities.AddPolyline(box, true, aDisplaySettings: dsp);
            if (bNoData)
            {
                dxfRectangle rect = box.BoundingRectangle();
                _rVal.Entities.Add(new dxeLine(rect.TopLeft, rect.BottomRight, dsp));
                _rVal.Entities.Add(new dxeLine(rect.BottomLeft, rect.TopRight, dsp));
            }


            // table headers
            txt = "\\LBLOCKED AREA ADJUSTMENTS";
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(0, -0.25), txt, 0.1875, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));

            if (!bNoData)
            {
                txt = $"VERTICAL SPLICE : {string.Format(fmat, aArea)}{uni}";
                _rVal.Entities.Add(tool.Create_Text(new dxfVector(-3.0126, -0.75), txt, 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard"));
            }

            return _rVal;
        }

        private dxfBlock CreateBlock_MDTable_DCNotes(string aBlockName, dxfImage aImage, mdTrayAssembly aAssy, uopDocDrawing aDrawing)
        {
            Status = $"Creating Block '{aBlockName}'";
            dxfBlock _rVal = new(aBlockName);
            string txt;
            double y1 = -0.725;
            colMDDowncomers aDCs = aAssy.Downcomers;
            mdDowncomer aDC;
            List<string> txts = new();
            List<int> fldovrs = new();
            List<int> gusts = new();
            List<int> supdef = new();
            List<int> bltdon = new();

            dxoEntityTool tool = aImage.EntityTool;
            double y2 = -1.2009;
            double y3 = y2 - 0.25;
            double dY = 0;

            aDCs = aAssy.Downcomers;

            for (int i = 1; i <= aDCs.Count; i++)
            {
                aDC = aDCs.Item(i);
                if (aDC.FoldOverWeirs) fldovrs.Add(i);
                if (aDC.GussetedEndplates) gusts.Add(i);
                if (aDC.HasSupplementalDeflector) supdef.Add(i);
                if (aDC.BoltOnEndplates) bltdon.Add(i);
            }

            if (fldovrs.Count > 0)
            {
                txt = (fldovrs.Count > 1) ? $"FOLD OVER WEIRS REQUIRED ON DOWNCOMERS " : $"FOLD OVER WEIRS REQUIRED ON DOWNCOMER ";
                txts.Add(mzUtils.ListToString(fldovrs, ", ", " AND ", aPrefix: txt));
            }

            if (gusts.Count > 0)
            {
                txt = (gusts.Count > 1) ? $"GUSSETED END PLATES REQUIRED ON DOWNCOMERS " : $"GUSSETED END PLATES REQUIRED ON DOWNCOMER ";
                txts.Add(mzUtils.ListToString(gusts, ", ", " AND ", aPrefix: txt));
            }

            if (supdef.Count > 0)
            {
                txt = (supdef.Count > 1) ? $"SUPPLEMENTAL DEFLECTORS REQUIRED ON DOWNCOMERS " : $"SUPPLEMENTAL DEFLECTORS REQUIRED ON DOWNCOMER ";
                txts.Add(mzUtils.ListToString(supdef, ", ", " AND ", aPrefix: txt));

            }

            if (bltdon.Count > 0)
            {
                txt = (bltdon.Count > 1) ? $"BOLT ON END PLATES REQUIRED ON DOWNCOMERS " : $"BOLT ON END PLATES REQUIRED ON DOWNCOMER ";
                txts.Add(mzUtils.ListToString(bltdon, ", ", " AND ", aPrefix: txt));

            }

            if (aAssy.DesignOptions.BottomDCHeight > aAssy.Downcomer().Height)
            {
                txt = $"DOWNCOMER HEIGHT ON TRAY {aAssy.RingRange.BottomRing} = {aAssy.DesignOptions.BottomDCHeight:#,0.0000} IN. ";
                txts.Add(txt);
            }
            _rVal.Name = aBlockName;

            // table headers
            txt = @"\LDOWNCOMER NOTES";
            _rVal.Entities.Add(tool.Create_Text(new dxfVector(0, -0.25), txt, 0.1875, dxxMTextAlignments.MiddleCenter, aStyleName: "Standard"));
            dY = 0.1925;
            for (int i = 1; i <= txts.Count; i++)
            {
                txt = txts[i - 1];

                _rVal.Entities.Add(tool.Create_Text(new dxfVector(-3.0126, y1), txt, 0.125, dxxMTextAlignments.MiddleLeft, aStyleName: "Standard"));
                y1 -= dY;
                if (i > 1)
                {
                    y2 -= dY;
                    y3 = y2 - 0.25;
                }
            }
            // border
            dxfDisplaySettings dsp = new dxfDisplaySettings("HEAVY", dxxColors.ByLayer, dxfLinetypes.Continuous);

            colDXFVectors box = new colDXFVectors($"(3.3147,0.0)¸(-3.3147,0.0)¸(-3.5647,-0.25)¸(-3.5647,{y2})¸(-3.3147,{y3})¸(3.3147,{y3})¸(3.5647,{y2})¸(3.5647,-0.25)");
            _rVal.Entities.AddPolyline(box, true, aDisplaySettings: dsp);
            if (txts.Count <= 0)
            {
                dxfRectangle rect = box.BoundingRectangle();
                _rVal.Entities.Add(new dxeLine(rect.TopLeft, rect.BottomRight, dsp));
                _rVal.Entities.Add(new dxeLine(rect.BottomLeft, rect.TopRight, dsp));
            }


            return _rVal;
        }

        public colDXFEntities UOPLogo(dxfVector aCenter, double aScaleFactor = 0)
        {
            colDXFEntities uOPLogo = new();

            dxePolyline aPl;
            double rad;
            string txt;
            double wd;
            double cx;
            double cy;
            dxfVector v1;
            dxeText aTxt;

            v1 = aCenter != null ? aCenter.Clone() : new dxfVector();
            cx = v1.X;
            cy = v1.Y;
            wd = 0.108;
            rad = 0.1757;

            // the O
            txt = $"({cx - rad},{cy},{rad})";
            txt += $"¸({cx},{cy - rad},{rad})";
            txt += $"¸({cx + rad},{cy},{rad})";
            txt += $"¸({cx},{cy + rad},{rad})";

            dxfDisplaySettings dsp = new("TEXT", dxxColors.ByLayer, dxfLinetypes.Continuous);
            aPl = uOPLogo.AddPolyline(txt, true, dsp, wd);

            // the U
            txt = $"({cx - rad},{cy + rad + wd / 2})";
            txt += $"¸({cx - rad},{cy},{rad})";
            txt += $"¸({cx},{cy - rad},{rad})";
            txt += $"¸({cx + rad},{cy})";
            txt += $"¸({cx + rad},{cy + rad + wd / 2})";

            aPl = uOPLogo.AddPolyline(txt, false, dsp, wd);
            aPl.Move(-(2 * rad + wd));

            // the P
            txt = $"({cx - rad},{cy},{rad})";
            txt += $"¸({cx},{cy - rad},{rad})";
            txt += $"¸({cx + rad},{cy},{rad})";
            txt += $"¸({cx},{cy + rad},{rad})";
            txt += $"¸({cx - rad},{cy})";
            txt += $"¸({cx - rad},{cy - 2 * rad})";

            aPl = uOPLogo.AddPolyline(txt, false, dsp, wd);
            aPl.Move(2 * rad + wd);

            aTxt = new dxeText
            {
                Alignment = dxxMTextAlignments.TopLeft,
                TextString = "\\fArial|b1|i0|\\W1;A Honeywell Company", // I am not sure about the \\f
                TextHeight = 0.125,
                LayerName = "TEXT",
                AlignmentPt1 = new dxfVector(cx - 3 * rad - 1.5 * wd, cy - 2.25 * rad)
            };
            uOPLogo.Add(aTxt);

            if (aScaleFactor > 0)
            {
                uOPLogo.Rescale(aScaleFactor, v1);
            }

            return uOPLogo;
        }

        public string StampPNLeader(dxfImage aImage, bool TwoLines = true)
        {
            // #1the aImage object to use
            // #2the letter size to put in the string
            // #3flag to include a line break in the returned string
            string lsz;
            string lbl;

            // ^creates the string used to mark a part with a string that indicates where the part number should be stamped

            lsz = new string((char)189, 1);
            lbl = "INCH";
            if (aImage != null)
            {
                if (aImage.DimSettings.DrawingUnits == dxxDrawingUnits.Metric)
                {
                    lsz = "13";
                    lbl = "MM";
                }
            }

            string stampPNLeader = "STEEL STAMP PART NO. WITH";
            stampPNLeader += TwoLines ? $"\\P{lsz} {lbl} HIGH CHARACTERS" : $" {lsz} {lbl} HIGH CHARACTERS";

            return stampPNLeader;
        }
        public dxeDimension CreateHoleLeader(dxfImage aImage, dxfEntity aHole, dxfVector aPlacementPt, dxoDimTool aDimTool, int aCount, bool aTwoLines = true, bool bParens = true, bool bSuppressMetricRounding = false, uopHardware aWeldedHDW = null, bool bSuppressTackText = false, bool bCreateOnly = false,string aOverideText = "")
        {

            if (aImage == null || aHole == null) return null;
            aDimTool ??= aImage.DimTool;
            dxfVector center = aHole.DefinitionPoint(dxxEntDefPointTypes.MiddleCenter);
            aPlacementPt ??= center.Clone();
            double pang = aImage.UCS.XDirection.AngleTo(center.DirectionTo(aPlacementPt));
            double dist = center.DistanceTo(aPlacementPt);
            string prefix = string.Empty;
            dxeArc arc = null;
            switch (aHole.GraphicType)
            {
                case dxxGraphicTypes.Arc:
                    arc = (dxeArc)aHole;
                    //prefix = "%%C";
                    break;
                case dxxGraphicTypes.Polygon:
                    dxePolygon pgon = (dxePolygon)aHole;
                    arc = (dxeArc)pgon.Segments.Nearest(aPlacementPt, aEntPointType: dxxEntDefPointTypes.Center, aEntityType: dxxEntityTypes.Arc);
                    break;
                case dxxGraphicTypes.Polyline:
                    dxePolyline pline = (dxePolyline)aHole;
                    arc = (dxeArc)pline.Segments.Nearest(aPlacementPt, aEntPointType: dxxEntDefPointTypes.Center, aEntityType: dxxEntityTypes.Arc);
                    break;
            }
            if (arc == null) return null;
            //arc = new dxeArc(arc.Center, arc.Radius); // to account for rotated planes in the arc

            string suffix = string.IsNullOrWhiteSpace(aOverideText) ? HoleLeader(aImage, aHole, aCount, aTwoLines, bParens, bSuppressMetricRounding, aWeldedHDW, bSuppressTackText, true) : string.Empty;
            int precis = aImage.DimSettings.DrawingUnits == dxxDrawingUnits.Metric ? 0 : 3;

            dxeDimension _rVal = aDimTool.RadialD(arc, pang, dist, aPrefix: prefix, aSuffix: suffix, aOverideText: aOverideText, aCenterMarkSize: 0, bCreateOnly: bCreateOnly, aOverridePrecision: precis);
            return _rVal;

        }


        public string HoleLeader(dxfImage aImage, object aHole, int aCount, bool aTwoLines = true, bool bParens = true, bool bSuppressMetricRounding = false, uopHardware aWeldedHDW = null, bool bSuppressTackText = false, bool bSuffixOnly = false, double? aDiameter = null)
        {
            // #1the drawing object to use
            // #2the hole to build the description based on
            // #3the number of holes to put in the description
            // #4flag to return a string with a line break or a single line of text
            // ^creates the string used to leader a hole with its desciption
            // ~like "0.5 Hole 4 Places" or "1.0 x 3.0 Slot 18 Places"

            if (aImage == null || aHole == null) return string.Empty;

            int aPrec;
            dxoDimStyle DStyle;
            dxeHole aHl;
            dxeArc aAr;
            dxePolyline aPl;
            bool metric = aImage.DimSettings.DrawingUnits == dxxDrawingUnits.Metric;
            aPrec = metric ? 0 : 3;
            double lng = 0;
            double dia = 0;
            string odstr;
            string lngstr;
            uopHole uHl;
            bool bSqr = false;
            double multi = metric ? 25.5 : 1;
            string _rVal;
            string fmat = metric ? "#0" : "#0.000";
            DStyle = aImage.DimStyle();



            switch (aHole)
            {
                case dxeHole:
                    aHl = (dxeHole)aHole;
                    dia = aHl.Diameter;
                    lng = aHl.Length;
                    bSqr = aHl.IsSquare;
                    break;
                case dxeArc:
                    aAr = (dxeArc)aHole;
                    dia = aAr.Diameter;
                    break;
                case dxePolyline:
                    aPl = (dxePolyline)aHole;
                    aPl.BoundingRectangle().GetDimensions(ref lng, ref dia);
                    lng = double.Parse(string.Format("{0:0.000000}", lng).Substring(0, 6)); // 4 decimals no rounding
                    dia = double.Parse(string.Format("{0:0.000000}", dia).Substring(0, 6)); // 4 decimals no rounding
                    mzUtils.SortTwoValues(true, ref dia, ref lng);
                    break;
                case uopHole:
                    uHl = (uopHole)aHole;
                    dia = uHl.Diameter;
                    lng = uHl.Length;
                    bSqr = uHl.IsSquare;
                    break;
                default:
                    break;
            }
            if (lng <= 0) lng = dia;
            if (aDiameter.HasValue)
            {
                if (aDiameter.Value > 0)
                {
                    dia = aDiameter.Value;
                    lng = dia;
                }
            }
            mzUtils.SortTwoValues(true, ref dia, ref lng);
            odstr = Math.Round(dia * multi, aPrec).ToString(fmat);
            lngstr = Math.Round(lng * multi, aPrec).ToString(fmat);



            if (lng <= dia)
            {

                if (!bSuffixOnly)
                    _rVal = !bSqr ? $"%%C{odstr} HOLE" : $"{odstr} SQR. HOLE";
                else
                    _rVal = !bSqr ? $" HOLE" : $" SQR. HOLE";
                if (aWeldedHDW != null)
                {
                    _rVal += $"\\P/W {aWeldedHDW.FriendlyName.ToUpper()}";
                    if (!bSuppressTackText)
                    {
                        _rVal += $"\\PTACK WELDED IN PLACE";
                    }
                }
            }
            else
            {
                if (!bSuffixOnly)
                    _rVal = $"{odstr} x {lngstr} SLOT";
                else
                    _rVal = $" x {lngstr} SLOT";


            }

            if (aCount > 1)
            {
                if (!bParens)
                {
                    _rVal += aTwoLines ? $"\\P{aCount} PLACES" : $" {aCount} PLACES";
                }
                else
                {
                    _rVal += aTwoLines ? $"\\P({aCount} PLACES)" : $" ({aCount} PLACES)";
                }
            }
            return _rVal;
        }

        public List<dxfEntity> DrawLinePair(dxoDrawingTool aDraw, uopLinePair aPair, bool bDrawShape = false, string aLayer = "", dxxColors aColor = dxxColors.Undefined, string aLineType = "", uppSides? aSide = null)
        {
            List<dxfEntity> _rVal = new List<dxfEntity>();
            if (aDraw == null || aPair == null) return _rVal;
            if (!bDrawShape)
            {
                uopLine l1 = aSide.HasValue ? aPair.GetSide(aSide.Value) : aPair.Line1;
                uopLine l2 = !aSide.HasValue ? aPair.Line2 : null;

                if (l1 != null) _rVal.Add(aDraw.aLine(l1, aLayer: aLayer, aColor: l1.Suppressed ? dxxColors.Red : aColor, aLineType: aLineType));
                if (l2 != null) _rVal.Add(aDraw.aLine(l2, aLayer: aLayer, aColor: l2.Suppressed ? dxxColors.Red : aColor, aLineType: aLineType));

            }
            else
            {
                _rVal.Add(aDraw.aPolyline(aPair.EndPoints(true), bClosed: true, aDisplaySettings: dxfDisplaySettings.Null(aLayer, aPair.Suppressed ? dxxColors.Red : aColor, aLineType)));
            }
            return _rVal;
        }
        public List<dxfEntity> DrawLinePairs(dxoDrawingTool aDraw, IEnumerable<uopLinePair> aPairs, bool bDrawShape = false, string aLayer = "", dxxColors aColor = dxxColors.Undefined, string aLineType = "")
        {
            List<dxfEntity> _rVal = new List<dxfEntity>();
            if (aDraw == null || aPairs == null) return _rVal;
            foreach (var item in aPairs)
            {
                _rVal.AddRange(DrawLinePair(aDraw, item, bDrawShape, aLayer, item.Suppressed ? dxxColors.Red : aColor, aLineType));
            }
            return _rVal;
        }
        public dxfVector DrawNotes(dxfImage aImage, iVector InsertVectorXY, List<string> Notes, string DrawingTitle = "", bool UnderLineTitle = false, bool AddNotesHeading = true, uopSheetMetal SheetMetal = null)
        {
            if (aImage == null || InsertVectorXY == null || Notes == null)
            {
                return null;
            }

            // #1the drawing object to use
            // #2the upper left point to start the notes
            // #3a collection of strings
            // #4an additional string to write above the notes
            // #5flag to request that the additional string be underlined
            // #6flag to request the "Notes:" heading


            // ^used to add the notes section to all drawings (if required)
            // ~returns the point where the next line would start.

            dxfVector p;
            string lbl;
            dxeText MText;
            string aStr = string.Empty;
            string aTabStr = string.Empty;
            string nStr;

            dxfVector drawNotes = null;

            p = new dxfVector(InsertVectorXY);

            DrawingTitle = DrawingTitle.Trim();

            if (SheetMetal != null)
            {
                Notes.Add($"MATERIAL-{SheetMetal.FriendlyName().ToUpper()}");
            }

            if (!string.IsNullOrWhiteSpace(DrawingTitle))
            {
                if (UnderLineTitle)
                {
                    aStr = "\\L";
                }
                MText = aImage.Draw.aText(p, aStr + DrawingTitle, aAlignment: dxxMTextAlignments.TopLeft, aTextStyle: "Standard");
                p.MoveTo(MText.TextRectangle().BottomLeft, 0, -0.1 * aImage.Display.ZoomFactor); // second parameter does not exist in VB but here it is not optional so I used the 0 for it
                aStr = string.Empty;
            }

            if (AddNotesHeading)
            {
                aStr = $"NOTES:\n";
                aTabStr = "   ";
            }

            for (int i = 1; i <= Notes.Count; i++)
            {
                nStr = (string)Notes[i];
                nStr = nStr.Replace("    ", "");
                nStr = nStr.Replace("   ", "");
                nStr = i < 10 ? nStr.Replace("\n", "\n   ") : nStr.Replace("\n", "\n    ");

                lbl = $"{aTabStr}{i}.) ";
                aStr += $"{lbl}{nStr}\n";
                //DoEvents // I don't know what it does
            }

            MText = aImage.Draw.aText(p, aStr, aAlignment: dxxMTextAlignments.TopLeft, aTextStyle: "Standard");

            // return the point where the next line would go

            drawNotes = new dxfVector();
            return drawNotes;
        }

        public dxeInsert Border(IappDrawingSource aSource, dxfRectangle aSubjectRectangle, ref dxfRectangle rDrawingAreaRectangle, ref double arPaperScale, double aWidthBuffer = 0, double aHeightBuffer = 0, bool bSuppressAnsiScales = false, string aScaleString = "", dynamic aSheetNo = null, uppBorderSizes arBorderSize = uppBorderSizes.Undefined)
        {
            rDrawingAreaRectangle = new dxfRectangle();
            aScaleString ??= string.Empty;
            if (bSuppressAnsiScales && aScaleString.Contains(':') && !aScaleString.Contains('.')) aScaleString = string.Empty;
            bCancelBorder = false;
            if (aSource == null) return null;

            string wuz;
            double aInsertScale;
            dxfAttributes aAttribs;
            string sclrstr = string.Empty;
            string Sht = string.Empty;
            string ttl1 = string.Empty;
            string ttl2 = string.Empty;
            string ttl3 = string.Empty;
            string txt;

            dxfVector v0;
            string ltr;
            dxfImage aImage = aSource.Image;
            uopDocDrawing aDrawing = aSource.Drawing;
            uopProject aProject = aSource.Project;
            uopTrayRange aRng = aSource.Range;
            double buf;
            if (aImage == null || aDrawing == null) return null;
            if (bSuppressAnsiScales && aDrawing.BorderScale.Contains(':') && !aDrawing.BorderScale.Contains('.')) aDrawing.BorderScale = string.Empty;
            dxeInsert border = null;
            bool metric = aDrawing.DrawingUnits == uppUnitFamilies.Metric;


            try
            {


                //aImage.Display.SetFeatureScales(arPaperScale, true, true);
                //aImage.Header.TextSize = 0.125 * arPaperScale;

                //aImage.SetTextStyleProperty("Standard", "TextHeight", 0.1875 * arPaperScale);

                //aDrawing.ZoomExtents = false;
                //aDrawing.BorderScale = sclrstr;

                //Status = wuz; // It was "frmMain.WorkSubStatus = wuz" in VB. Also, it seems that wus hasn't been initialized properly.
                //return null;
                if (arBorderSize == uppBorderSizes.Undefined) arBorderSize = aDrawing.BorderSize;
                if (arBorderSize == uppBorderSizes.Undefined) arBorderSize = uppBorderSizes.BSize_Landscape;
                if (aProject == null) aProject = aDrawing.Project;

                if (aSheetNo != null)
                {
                    Sht = $"{aSheetNo}";
                    Sht = Sht.Trim();
                }

                if (string.IsNullOrWhiteSpace(Sht))
                {
                    if (aDrawing.SheetNumber > 0)
                    { Sht = aDrawing.SheetNumber.ToString(); }

                }

                if (bCancelBorder) return null;


                var rBorderRectangle = new dxfRectangle();
                wuz = Status;
                v0 = dxfVector.Zero;
                buf = 0.09375;

                if (arBorderSize == uppBorderSizes.DSize_Landscape)
                {
                    Status = (!aDrawing.SuppressBorder) ? "Inserting UOP D-Size Border" : "Scaling To UOP D-Size Border";

                    rBorderRectangle.SetDimensions(35, 22.8899);
                    rBorderRectangle.SetCoordinates(0.5 * rBorderRectangle.Width, 0.5 * rBorderRectangle.Height);

                    rDrawingAreaRectangle.SetDimensions(rBorderRectangle.Width - (buf * 2), 17.9);
                    rDrawingAreaRectangle.SetCoordinates(rBorderRectangle.X, rBorderRectangle.Height - rDrawingAreaRectangle.Height / 2 - buf);
                    ltr = "D";
                }
                else
                {
                    Status = (!aDrawing.SuppressBorder) ? "Inserting UOP B-Size Border" : "Scaling To UOP B-Size Border";

                    rBorderRectangle.SetDimensions(16, 10);
                    rBorderRectangle.SetCoordinates(0.5 * rBorderRectangle.Width, 0.5 * rBorderRectangle.Height);

                    rDrawingAreaRectangle.SetDimensions(rBorderRectangle.Width - (buf * 2), rBorderRectangle.Height - 2.0413 - (buf * 2));
                    rDrawingAreaRectangle.SetCoordinates(rBorderRectangle.X, rBorderRectangle.Height - rDrawingAreaRectangle.Height / 2 - buf);
                    ltr = "B";
                }

                aSubjectRectangle ??= rDrawingAreaRectangle.Clone();


                if (bCancelBorder) return null;


                if (!string.IsNullOrWhiteSpace(aDrawing.BorderScale) && aScaleString.ToUpper() != "NTS")
                {
                    if (!DecodeScaleString(aDrawing.BorderScale, out arPaperScale, out sclrstr))
                    {
                        arPaperScale = 0;
                    }
                    else
                    {
                        aScaleString = sclrstr;
                        arPaperScale = 1 / arPaperScale;
                    }
                }

                if (arPaperScale <= 0)
                {
                    aInsertScale = rDrawingAreaRectangle.BestFitScale(aSubjectRectangle.Width, aSubjectRectangle.Height, false, !bSuppressAnsiScales, ref sclrstr, aWidthBuffer, aHeightBuffer, metric);
                    if (!string.IsNullOrWhiteSpace(aScaleString))
                    {
                        sclrstr = aScaleString;
                    }
                }
                else
                {
                    aInsertScale = arPaperScale;
                    sclrstr = string.IsNullOrWhiteSpace(aScaleString) ? $"1:{string.Format("{0:0.00#}", arPaperScale)}" : aScaleString;

                }

                if (bCancelBorder) return null;

                arPaperScale = aInsertScale;
                rBorderRectangle.Rescale(aInsertScale, v0);
                rDrawingAreaRectangle.Rescale(aInsertScale, v0);
                aImage.Display.SetDisplayRectangle(rBorderRectangle, 1.05);

                aDrawing.ViewCenter = rBorderRectangle.Center;
                aDrawing.ExtentWidth = aImage.Display.ViewPlane.Width;

                if (bCancelBorder) return null;


                if (!aDrawing.SuppressBorder && aProject != null)
                {
                    ttl1 = "MANUFACTURING DETAILS";
                    aRng = aProject.TrayRanges.Item(aDrawing.RangeGUID);
                    ttl2 = aRng != null ? $"TRAYS {aRng.SpanName()}" : aProject.Customer.Name.ToUpper();

                    ttl3 = aProject.Column.Name.ToUpper();
                    if (string.IsNullOrWhiteSpace(ttl3))
                    {
                        ttl3 = aProject.Customer.Item;
                        ttl3 = $"{ttl3} {aProject.Customer.Service.ToUpper()}".Trim();
                    }

                    if (aDrawing.DrawingType == uppDrawingTypes.Functional)
                    {
                        ttl1 = "FUNCTIONAL DESIGN";
                    }
                    else if (aDrawing.DrawingType == uppDrawingTypes.Installation)
                    {
                        ttl1 = "TRAY INSTALLATION";
                    }
                    else
                    {
                        if (arBorderSize == uppBorderSizes.BSize_Landscape)
                        {
                            txt = aProject.Customer.Name;
                            if (!string.IsNullOrWhiteSpace(txt))
                            {
                                ttl2 = txt.ToUpper();
                            }
                        }
                    }
                }

                if (bCancelBorder) return null;

                string dwgno = aProject == null ? "" : aProject?.DrawingNumbers.ValueS(aDrawing.Category);
                aAttribs = BorderAttributes(ttl1, ttl2, ttl3, sclrstr, aProject, Sht, dwgno);



                aAttribs.SetValue("Drawing#", aProject?.DrawingNumbers.ValueS(aDrawing.Category));
                //SetDrawProperties(aImage, aDrawing);

                //aBlk = aImage.Blocks.Item("Border"); // it was Item("Border") in VB
                //if (aBlk == null)
                //{
                //    aImage.ReadFromFile($"{appApplication.BordersFolder}\\{ltr} - Border.dxf", 0, false, "ENTITIES"); // The "ThisApp" in VB is of type appApplication which here did not have the BordersFolder. We added it to it. Also, in VB, the "aCallingForm" argument is called with frmMain.hwnd but here it's not used so I passed the default value to it.
                //    aBlk = aImage.Blocks.Item("Border"); // it was Item("Border") in VB
                //}
                //if (bCancelBorder)
                //{
                //    return null;
                //}
                //if (aBlk == null)
                //{
                //    aBlk = CreateBlock($"{ltr}BORDER", aImage, aProject, aAttribs: aAttribs);
                //    aBlk = aImage.Blocks.Add(aBlk);
                //}
                //else
                //{
                //    bBlockFound = true;
                //}
                //if (bCancelBorder)
                //{
                //    return null;
                //}
                //if (aBlk != null)
                //{
                //    UpdateBorderAttribs(aImage, arBorderSize, aBlk, aAttribs);
                //    border = aImage.Draw.aInsert("Border", v0, 0, aInsertScale, "Border", aAttribs, dxxColors.ByLayer, dxfLinetypes.Continuous);
                //}
                //else
                //{
                //    aImage.Display.SetFeatureScales(arPaperScale, true, true);
                //    aImage.Header.TextSize = 0.125 * arPaperScale;
                //    
                //    aImage.SetTextStyleProperty("Standard", "TextHeight", 0.1875 * arPaperScale);

                //    aDrawing.ZoomExtents = false;
                //    aDrawing.BorderScale = sclrstr;
                //   Status = wuz; // It was "frmMain.WorkSubStatus = wuz" in VB
                //    return null;
                //}

                //if (bCancelBorder)
                //{
                //    return null;
                //}
                //if (!string.IsNullOrWhiteSpace(aSubBlocks))
                //{
                //    sBlocks = aSubBlocks.Split(',');
                //    for (int i = 0; i < sBlocks.Length; i++)
                //    {
                //        sBlk = sBlocks[i].ToUpper();

                //        aBlk = aImage.Blocks.Item(sBlk); // it was Item(sBlk) in VB
                //        bBlockFound = aBlk != null;
                //        if (!bBlockFound)
                //        {
                //            if (sBlk == "REVISIONS")
                //            {
                //                sBlk = ltr + sBlk;
                //            }
                //            aBlk = CreateBlock(sBlk, aImage, aProject);
                //            aBlk = aImage.Blocks.Add(aBlk);
                //            bBlockFound = true;
                //        }

                //        if (bBlockFound)
                //        {
                //            Status = $"Inserting Block '{sBlocks[i].ToUpper()}'"; // It was "frmMain.WorkSubStatus = $"Inserting Block '{sBlocks[i].ToUpper()}'"" in VB
                //            
                //            if (sBlk != ltr + "REVISIONS")
                //            {
                //                v0 = aBlk.InsertionPt.Clone();
                //                if (sBlk == "SYMBOL_KEY" && !bBlockFound)
                //                {
                //                    v0.SetCoordinates(28.4934, 4.5019);
                //                }
                //                v0.Rescale(aInsertScale, new dxfVector(), aInsertScale, 0);
                //            }
                //            aImage.Draw.aInsert(aBlk.Name, v0, aScaleFactor: aInsertScale);
                //        }
                //    }
                //}
                //else
                //{
                //    if (bCancelBorder)
                //    {
                //        return null;
                //    }
                //    SetDrawProperties(aImage, aDrawing);
                //    aImage.Draw.aPolyline(rBorderRectangle, true, null, "Border", dxxColors.ByLayer, dxfLinetypes.Continuous);
                //    aImage.Display.ZoomExtents();

                //    if (bCancelBorder)
                //    {
                //        return null;
                //    }
                //    v0 = rBorderRectangle.BottomRight;
                //    v0.Move(aYChange: -0.0625 * aInsertScale);
                //    ttl1 = (arBorderSize == uppBorderSizes.BSize_Landscape ? "B Size Border Scale = " : "D Size Border Scale = ") + aInsertScale;
                //    if (!string.IsNullOrWhiteSpace(sclrstr))
                //    {
                //        ttl1 += $" = ({sclrstr})";
                //    }
                //    aImage.Draw.aText(v0, ttl1, 0.125 * aInsertScale, dxxMTextAlignments.TopRight, aTextStyle: "Standard");
                //}

                //If the border file is found it is inserted
                TitleBlockHelper = new TitleBlockHelper(ltr, aAttribs, v0, aInsertScale);
                if (!TitleBlockHelper.IsValid)
                {
                    aImage.Display.SetFeatureScales(arPaperScale, true, true);
                    aImage.Header.TextSize = 0.125 * arPaperScale;

                    aImage.SetTextStyleProperty("Standard", "TextHeight", 0.1875 * arPaperScale);

                    aDrawing.ZoomExtents = false;
                    aDrawing.BorderScale = (!bSuppressAnsiScales) ? sclrstr : "";
                    Status = wuz;

                    //SetDrawProperties(aImage, aDrawing);
                    aImage.Draw.aPolyline(rBorderRectangle, aDisplaySettings: new dxfDisplaySettings("Border", dxxColors.ByLayer, dxfLinetypes.Continuous));
                    aImage.Display.ZoomExtents();

                    v0 = rBorderRectangle.BottomRight;
                    v0.Move(aYChange: -0.0625 * aInsertScale);
                    ttl1 = (arBorderSize == uppBorderSizes.BSize_Landscape ? "B Size Border Scale = " : "D Size Border Scale = ") + aInsertScale;
                    if (!string.IsNullOrWhiteSpace(sclrstr))
                    {
                        ttl1 += $" = ({sclrstr})";
                    }
                    aImage.Draw.aText(v0, ttl1, 0.125 * aInsertScale, dxxMTextAlignments.TopRight, aTextStyle: "Standard");
                }

                aImage.Display.SetFeatureScales(arPaperScale, true, true);
                aImage.Header.TextSize = 0.125 * arPaperScale;

                //aImage.SetTextStyleProperty("Standard", "TextHeight", 0.1875 * arPaperScale);
                //aImage.Styles.AddTextStyle("Standard", "romand.shx", 0.1875 * arPaperScale);


                aDrawing.ZoomExtents = false;
                if (sclrstr.Contains(":")) // If it is a valid scale string
                {
                    aDrawing.BorderScale = sclrstr;
                }
                Status = wuz; // It was "frmMain.WorkSubStatus = wuz" in VB
            }
            catch (Exception e)
            {
                { aImage.HandleError(System.Reflection.MethodBase.GetCurrentMethod(), this.GetType().Name, e.Message); }
                border = null;
            }

            return border;
        }

        public string RadiusLeaderText(dxfImage aImage, uppUnitFamilies aUnits, double aRadius, bool bAddTypical = true)
        {

            if (aImage == null)
            {
                string fmat = (aUnits == uppUnitFamilies.English) ? "0.000" : "0";

                return bAddTypical ? $"R{aRadius.ToString(fmat)} (TYP)" : $"R{aRadius.ToString(fmat)}";

            }
            else
            {
                if (bAddTypical)
                {
                    return $"R{aImage.DimStyle().FormatNumber(aRadius, true, aPrecision: aUnits == uppUnitFamilies.English ? -1 : 0)} (TYP)";
                }
                else
                {
                    return $"R{aImage.DimStyle().FormatNumber(aRadius, true, aPrecision: aUnits == uppUnitFamilies.English ? -1 : 0)}";
                }

            }
        }

        public void NumberVectors(dxoDrawingTool aDrawingTool, IEnumerable<iVector> aVectors, double aPaperScale, double aTextHeight = 0.06, int aStartIndex = 0, int aEndIndex = 0,  dxfDisplaySettings aDisplaySettings = null)
        {
            if (aDrawingTool == null || aVectors == null) return;
            if (aPaperScale <= 0) aPaperScale = aDrawingTool.PaperScale;
            int n = 0;
            int si = 1;
            int ei = aVectors.Count();
            aDisplaySettings ??= dxfDisplaySettings.Null(aDrawingTool.Image.TextSettings.LayerName);

            if (!dxfUtils.LoopIndices(aVectors.Count(), aStartIndex, aEndIndex, ref si, ref ei)) return;
            foreach (var item in aVectors)
            {
                if (item == null) continue;
                n++;
                if (n < si || n > ei) continue;
                aDrawingTool.aText(item, $"\\Ftxt.shx;{n}", aTextHeight * aPaperScale, aAlignment: dxxMTextAlignments.MiddleCenter, aColor: aDisplaySettings.Color, aLayer: aDisplaySettings.LayerName);

            }

        }

        public dxeInsert AddBorderNotes(uopPart aPart, uopProject aProject, string aHeading, bool bSubPart, dxfImage aImage, dxfRectangle aFitRectangle, double aScale, string aStyleName = "Standard", double aTextHeight = 0)
        {
            if (aPart == null || aImage == null) return null;
            aPart.UpdatePartProperties();
            if (string.IsNullOrWhiteSpace(aHeading)) aHeading = aPart.PartType.Description().ToUpper();
            string txtforparts = string.Empty;

            if (bSubPart)

            {
                aHeading = $"{aHeading} (SUB-PART)";

                txtforparts = aPart.ParentPartType.Description().ToUpper();
                txtforparts = txtforparts.Replace(" BOX", "");
                if (aPart.ParentList.Contains(",")) txtforparts += "S";
                txtforparts = $"FOR {aPart.AssociatedParentList(txtforparts + " ")}";

            }

            string txtfortrays = !aPart.HasRingRanges ? aPart.RangeSpanNames(aProject, ", ").ToUpper() : aPart.RingRanges.SpanNameList(", ");
            if (txtfortrays.Contains('-') || txtfortrays.Contains(','))
                txtfortrays = $"FOR TRAYS {txtfortrays}";
            else
                txtfortrays = $"FOR TRAY {txtfortrays}";

            string txt = !bSubPart ? $"PART NUMBER: {aPart.PartNumber}" : "";


            AppDrawingBorderData notes = new()
            {
                PartName_Value = aHeading,
                PartNumber_Value = $"{txt}",
                ForParts_Value = $"{txtforparts}",
                ForMaterial_Tag = "FOR MATERIAL SEE SHEET 2",
                NumberRequired_Value = $"NUMBER REQUIRED:  {string.Format("{0:#,0}", aPart.Quantity)}",
                Location_Value = txtfortrays,

            };




            return Add_B_BorderNotes(aImage, notes, aFitRectangle, aScale);



        }
        public dxeText AddBorderNotes(dxfImage aImage, string aNote, dxfRectangle aFitRectangle, double aScale, string aStyleName = "Standard", double aTextHeight = 0)
        {
            if (aImage == null || aFitRectangle == null || string.IsNullOrWhiteSpace(aNote.Trim())) return null;

            if (aScale <= 0)
            {
                aScale = aImage.Display.PaperScale;
            }
            string aStats = Status;
            dxfVector v1;
            Status = "Adding Border Notes";

            if (aTextHeight <= 0) aTextHeight = 0.1875 * aScale;

            dxoStyle aStyle = aImage.TextStyle(string.IsNullOrWhiteSpace(aStyleName) ? "Standard" : aStyleName.Trim());

            dxeText _rVal = aStyle.CreateText(aNote, dxxTextTypes.Multiline, dxxTextDrawingDirections.Horizontal, aTextHeight, dxxMTextAlignments.BottomLeft, aImage: aImage);

            _rVal.DisplaySettings = aImage.GetDisplaySettings(dxxEntityTypes.MText, aStyleName: aStyle.Name);
            v1 = aFitRectangle.BottomRight.Moved(-_rVal.Width - (1.0 * aScale), 0.125 * aScale);
            _rVal.Move(v1.X, v1.Y);

            aImage.Entities.Add(_rVal);
            Status = aStats;

            //aImage.Entities.Add(aFitRectangle.Perimeter(false, dxxColors.LightGrey));
            //aImage.Entities.Add(_rVal.ExtentPoints.BoundingRectangle().Perimeter(false, dxxColors.Red));
            return _rVal;
        }

        public dxeInsert Add_B_BorderNotes(dxfImage aImage, AppDrawingBorderData aRequest, dxfRectangle aFitRectangle, double aScale, string aStyleName = "Standard", double aTextHeight = 0)
        {
            if (aImage == null || aRequest == null) return null;

            if (aScale <= 0) aScale = aImage.Display.PaperScale;
            if (aTextHeight <= 0) aTextHeight = 0.1875;
            aStyleName = string.IsNullOrWhiteSpace(aStyleName) ? "Standard" : aStyleName.Trim();
            string aStats = Status;
            dxoStyle aStyle = aImage.TextStyle(aStyleName);
            dxfVector v1 = dxfVector.Zero;

            dxoEntityTool tool = aImage.EntityTool;
            Status = "Adding Border Notes";
            dxfBlock noteblock = new("Notes");
            dxeText attr = null;

            if (!string.IsNullOrWhiteSpace(aRequest.PartName_Value))
            {
                attr = tool.Create_Text(v1, aRequest.PartName_Value, aTextHeight * aScale, dxxMTextAlignments.BottomLeft, aStyleName: aStyle.Name, aTextType: dxxTextTypes.AttDef, aAttributeTag: aRequest.PartName_Tag, aAttributePrompt: aRequest.PartName_Prompt);
                noteblock.Entities.Add(attr);
                v1 = v1.Moved(aYChange: -attr.TextHeight - 0.125 * aScale);

            }

            if (!string.IsNullOrWhiteSpace(aRequest.PartNumber_Value))
            {
                attr = tool.Create_Text(v1, aRequest.PartNumber_Value, aTextHeight * aScale, dxxMTextAlignments.BottomLeft, aStyleName: aStyle.Name, aTextType: dxxTextTypes.AttDef, aAttributeTag: aRequest.PartNumber_Tag, aAttributePrompt: aRequest.PartNumber_Prompt);
                noteblock.Entities.Add(attr);
                v1 = v1.Moved(aYChange: -attr.TextHeight - 0.125 * aScale);
            }
            if (!string.IsNullOrWhiteSpace(aRequest.ForParts_Value))
            {
                var lines = BreakPartNumbersToLines(aRequest.ForParts_Value, 2, 6);
                foreach (var line in lines)
                {
                    attr = tool.Create_Text(v1, line, aTextHeight * aScale, dxxMTextAlignments.BottomLeft, aStyleName: aStyle.Name, aTextType: dxxTextTypes.AttDef, aAttributeTag: aRequest.ForParts_Tag, aAttributePrompt: aRequest.ForParts_Prompt);
                    noteblock.Entities.Add(attr);
                    v1 = v1.Moved(aYChange: -attr.TextHeight - 0.125 * aScale);
                }
            }
            if (!string.IsNullOrWhiteSpace(aRequest.NumberRequired_Value))
            {
                attr = tool.Create_Text(v1, aRequest.NumberRequired_Value, aTextHeight * aScale, dxxMTextAlignments.BottomLeft, aStyleName: aStyle.Name, aTextType: dxxTextTypes.AttDef, aAttributeTag: aRequest.NumberRequired_Tag, aAttributePrompt: aRequest.NumberRequired_Prompt);
                noteblock.Entities.Add(attr);
                v1 = v1.Moved(aYChange: -attr.TextHeight - 0.125 * aScale);
            }
            if (!string.IsNullOrWhiteSpace(aRequest.ForMaterial_Value))
            {
                attr = tool.Create_Text(v1, aRequest.ForMaterial_Value, aTextHeight * aScale, dxxMTextAlignments.BottomLeft, aStyleName: aStyle.Name, aTextType: dxxTextTypes.AttDef, aAttributeTag: aRequest.ForMaterial_Tag, aAttributePrompt: aRequest.ForMaterial_Prompt);
                noteblock.Entities.Add(attr);
                v1 = v1.Moved(aYChange: -attr.TextHeight - 0.125 * aScale);
            }

            if (!string.IsNullOrWhiteSpace(aRequest.Location_Value))
            {
                attr = tool.Create_Text(v1, aRequest.Location_Value, aTextHeight * aScale, dxxMTextAlignments.BottomLeft, aStyleName: aStyle.Name, aTextType: dxxTextTypes.AttDef, aAttributeTag: aRequest.Location_Tag, aAttributePrompt: aRequest.Location_Prompt);
                noteblock.Entities.Add(attr);
                v1 = v1.Moved(aYChange: -attr.TextHeight - 0.125 * aScale);
            }
            if (noteblock.Entities.Count <= 0) return null;

            noteblock.InsertionPt = attr.InsertionPt;
            noteblock = aImage.Blocks.Add(noteblock);


            dxfDisplaySettings dsp = aImage.GetDisplaySettings(dxxEntityTypes.MText, aStyleName: aStyle.Name);
            if (aFitRectangle != null)
                v1 = aFitRectangle.BottomRight.Moved(-(1.25 * noteblock.BoundingRectangle(dxfPlane.World).Width) - (0.5 * aScale), 0.125 * aScale);
            else
                v1 = dxfVector.Zero;

            dxeInsert _rVal = aImage.Draw.aInsert(noteblock.Name, v1, aScaleFactor: 1, aDisplaySettings: dsp);


            Status = aStats;

            //aImage.Entities.Add(aFitRectangle.Perimeter(false, dxxColors.LightGrey));
            //aImage.Entities.Add(_rVal.ExtentPoints.BoundingRectangle().Perimeter(false, dxxColors.Red));
            return _rVal;
        }

        // This method receives the comma separated list of part numbers. It will break that string into lines based on this criteria:
        // The first lines should have maximum of "firstLineLimit" elements in it
        // The subsequent lines should have maxumum of "subsequentLinesLimit" elements in them
        // Except the last line, lines should end with a "," and elements in each line are connected using ", " string.
        private IEnumerable<string> BreakPartNumbersToLines(string commaSeparatedPartNumbers, int firstLineLimit, int subsequentLinesLimit)
        {
            var partNumbers = commaSeparatedPartNumbers.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);

            List<string> lines = new List<string>();

            if (firstLineLimit >= partNumbers.Length)
            {
                // One line is enough, so there is no need to change the input string.
                lines.Add(commaSeparatedPartNumbers);
            }
            else
            {
                var firstLinePartNumbers = partNumbers.Take(firstLineLimit).ToArray();
                var subsequentLinesPartNumbers = partNumbers.Skip(firstLineLimit).ToArray();

                // Adding first line
                string firstLine = string.Join(", ", firstLinePartNumbers);
                if (subsequentLinesPartNumbers.Length > 0)
                {
                    firstLine += ",";
                }
                lines.Add(firstLine);

                // Adding subsequent lines
                var numberOfLines = (int)Math.Ceiling(subsequentLinesPartNumbers.Length / (1.0 * subsequentLinesLimit));
                string[] temp = new string[subsequentLinesLimit];
                string lineText;

                for (int i = 0; i < numberOfLines; i++)
                {
                    temp = new string[subsequentLinesLimit];

                    // Fill the temporary array elements with their corresponding part number for the current line
                    for (int j = 0; j < subsequentLinesLimit; j++)
                    {
                        if ((i * subsequentLinesLimit) + j >= subsequentLinesPartNumbers.Length) // check for lines shorter than the subsequent line limit
                        {
                            break;
                        }

                        temp[j] = subsequentLinesPartNumbers[(i * subsequentLinesLimit) + j];
                    }

                    // Assemble the part numbers into a single string and add a comma if needed
                    lineText = string.Join(", ", temp.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray());
                    if (i < (numberOfLines - 1))
                    {
                        lineText += ",";
                    }

                    // Add the line to the list of lines
                    if (!string.IsNullOrWhiteSpace(lineText))
                    {
                        lines.Add(lineText);
                    }
                }
            }

            return lines;
        }


        public double BestShellScale(uppBorderSizes aBorderSize, double aShellDia, double aTarget, bool bMetric, out string rScaleString, bool bSuppressAnsiScales = false)
        {
            rScaleString = string.Empty;
            if (aShellDia <= 0 || aTarget <= 0)
            {
                return 0; // should it be zero or a negative value???
            }
            double wd1;
            double wd2;
            double scl;
            if (aBorderSize == uppBorderSizes.BSize_Landscape)
            {
                wd1 = 16;
            }
            else if (aBorderSize == uppBorderSizes.DSize_Landscape)
            {
                wd1 = 35;
            }
            else
            {
                return 0; // should it be zero or a negative value???
            }

            wd2 = wd1 / aTarget * aShellDia;
            scl = wd2 / wd1;
            double bestShellScale = 0;

            if (!bSuppressAnsiScales)
            {
                rScaleString = dxfUtils.NearestAnsiScale(scl, ref bestShellScale, bMetric);

            }
            else
            {
                bestShellScale = scl;
                rScaleString = $"1:{scl:0.000}";
            }

            return bestShellScale;
        }

        public dxfBlock LoadBlock(IappDrawingSource aHelper, string aSource, dxfBlockSource aBlockSource, string aBlockName, dxfAttributes aAttribs = null, bool bConvertAttribsToText = false, bool bRedefine = false, iVector aInsertionPt = null, double aInsertionScale = 1, dxfDisplaySettings aDisplaySettings = null)
        {
            dxfBlock _rVal = null;

            if (aHelper == null || aBlockSource == null || string.IsNullOrWhiteSpace(aBlockName)) return null;
            dxfImage image = aHelper.Image;
            if (image == null) return null;
            aBlockName = aBlockName.Trim();
            string statwuz = aHelper.Status;
            int fidx = 0;
            string err = string.Empty;
            try
            {
                if (!aBlockSource.FileExists(aSource, ref fidx))
                {
                    err = $"Block Source File '{aSource}' Was Not Found";
                    return null;
                }
                string fspec = aBlockSource.FileName(fidx);

                if (bRedefine)
                    aBlockSource.ClearExistingBlocks(aBlockName);
                if (!aBlockSource.FileIsLoaded(fidx))
                {
                    Status = $"Loading Block Source File '{aSource}'";
                    aBlockSource.LoadFile(fidx, ref err, true);
                    if (!string.IsNullOrWhiteSpace(err)) return null;

                }

                Status = $"Transfering Block '{aBlockName}' From Block Source File '{aSource}' To Image '{image.Name}'";
                _rVal = aBlockSource.TransferBlock(image, fspec, aBlockName, false, bConvertAttribsToText, ref err, true);



            }
            catch (Exception e)
            {
                err = e.Message;
                _rVal = null;
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(err))
                    image?.HandleError(System.Reflection.MethodBase.GetCurrentMethod(), this.GetType().Name, err);

                Status = statwuz;

                if (_rVal != null && aInsertionPt != null)
                {
                    try
                    {
                        Status = $"Inserting Block '{_rVal.Name}' From Block Source File '{aSource}' Into Image '{image.Name}'";
                        image.Draw.aInsert(_rVal.Name, aInsertionPt, aRotationAngle: 0, aScaleFactor: aInsertionScale, aDisplaySettings: aDisplaySettings);
                    }
                    catch (Exception e)
                    {
                        err = e.Message;

                    }
                    finally
                    {
                        if (!String.IsNullOrWhiteSpace(err))
                            image?.HandleError(System.Reflection.MethodBase.GetCurrentMethod(), this.GetType().Name, err);

                        _rVal = image.Blocks.Find((x) => string.Compare(x.Name, aBlockName, true) == 0);
                        Status = statwuz;
                    }

                }
            }

            return _rVal;

        }

        public colDXFEntities DrawHoleOld(dxfImage aImage, dxeHole Hole, out dxeLine HorizontalLine, out dxeLine VerticalLine, bool DrawCenterlines = true, bool SuppressVerticalCenterline = false, bool SuppressHorizontalCenterline = false, double CenterlineScaleFactor = 2, dxePolygon Polygon = null, bool DrawHidden = false, bool bSetLCL = true, string aLayer = "")
        {
            // #1the drawing object to use
            // #2the hole to draw
            // #3the point where the hole will be drawn (or return the point it was drawn if nothing is passed)
            // #4the requested view
            // #5flag control the drawing of centerlines
            // #6flag suppress vertical centerline
            // #7flag suppress horizontal centerline
            // #8scale factor to apply to the hole

            // ^used by various functions to draw holes and slots
            // ~returns the hole centerlines if they are requested.

            colDXFEntities drawHoleOld = new();
            HorizontalLine = null;
            VerticalLine = null;
            if (aImage == null || Hole == null)
            {
                return drawHoleOld;
            }

            dxeHole aHole = null;
            dxeHole bHole;
            colDXFEntities cLines;
            colDXFEntities aHls;

            aHls = new colDXFEntities();
            if (CenterlineScaleFactor <= 0)
            {
                DrawCenterlines = false;
            }

            if (DrawHidden)
            {
                aLayer = "HIDDEN";
            }
            bHole = aImage.Draw.aHoleObj(aHole, aLayer, dxxColors.ByLayer, "ByLayer");

            if (CenterlineScaleFactor > 0 && DrawCenterlines)
            {
                aHls.Add(bHole);


                cLines = aImage.Draw.aHoleCenterLines(aHls, CenterlineScaleFactor, SuppressHorizontalCenterline, SuppressVerticalCenterline);
                if (!SuppressHorizontalCenterline)
                {

                    HorizontalLine = (dxeLine)cLines.GetFlagged("Horizontal");
                }
                if (!SuppressVerticalCenterline)
                {
                    HorizontalLine = (dxeLine)cLines.GetFlagged("Vertical");
                }

                drawHoleOld.Add(HorizontalLine);
                drawHoleOld.Add(VerticalLine);
            }

            return drawHoleOld;
        }

        public dxfVector FilletLines(dxfImage aImage, dxeLine aSegment, dxeLine bSegment, out dxePolyline rPolyline, double aGap = 0, dxfDisplaySettings aDisplaySettings = null, colDXFEntities aCollector = null)
        {
            rPolyline = null;
            if (aSegment == null || bSegment == null) return null;

            dxfVector _rVal = aSegment.IntersectPoint(bSegment);
            if (_rVal == null) return null;



            double d1 = 0;
            double d2 = 0;

            dxfVector v1 = aSegment.EndPoints().NearestVector(_rVal, ref d1, bReturnClone: true);
            if (d1 <= 0) return _rVal;

            dxfVector v2 = bSegment.EndPoints().NearestVector(_rVal, ref d2, bReturnClone: true);
            if (d2 <= 0) return _rVal;

            if (aGap > 0)
            {
                double gp = aGap < 0.9 * d1 ? aGap : 0.9 * d1;
                v1 += v1.DirectionTo(_rVal) * gp;
                gp = aGap < 0.9 * d2 ? aGap : 0.9 * d2;
                v2 += v2.DirectionTo(_rVal) * gp;
            }

            rPolyline = new dxePolyline(new colDXFVectors(v1, _rVal.Clone(), v2), false) { Linetype = dxfLinetypes.Phantom };

            if (aDisplaySettings != null)
            {
                rPolyline.DisplaySettings = aDisplaySettings;
            }
            else
            {
                if (aImage != null)
                {
                    dxoLayer phntm = (dxoLayer)aImage.GetOrAddReference(dxfLinetypes.Phantom, dxxReferenceTypes.LAYER, dxxColors.LightGrey, dxfLinetypes.Phantom);
                    rPolyline.LayerName = phntm.Name;
                }
            }


            if (aImage != null)
            {

                rPolyline = (dxePolyline)aImage.Entities.Add(rPolyline);
            }
            aCollector?.Add(rPolyline);

            return _rVal;
        }

        public string HolesString(uopHoles aHoles, uppUnitFamilies aUnits, dxoDimStyle aDStyle, string aLineFeed = "\\P")
        {
            if (aHoles == null || aHoles.Count <= 0)
            {
                return string.Empty;
            }

            uopHole aHl;
            Dictionary<string, int> aLngs = new(); // This was of type mzNameValues in VB which has been removed from this project's compile list
            string txt;
            int prec = aUnits == uppUnitFamilies.English ? 3 : 0;
            string fmat = aUnits == uppUnitFamilies.English ? "#,0.000" : "#,0";

            string _rVal = string.Empty;

            for (int i = 1; i <= aHoles.Count; i++)
            {
                aHl = aHoles.Item(i);

                if (Math.Round(aHl.Length - aHl.Diameter, 1) == 0)
                {
                    if (aDStyle != null)
                    {
                        txt = aDStyle.FormatNumber(aHl.Diameter, aPrecision: prec);
                    }
                    else
                    {
                        txt = aHl.Diameter.ToString(fmat);
                    }
                }
                else
                {
                    if (aDStyle != null)
                    {
                        txt = $"{aDStyle.FormatNumber(aHl.Diameter, aPrecision: prec)} x {aDStyle.FormatNumber(aHl.Length, aPrecision: prec)}";
                    }
                    else
                    {
                        txt = $"{aHl.Diameter.ToString(fmat)} x {aHl.Length.ToString(fmat)}";
                    }
                }

                if (aLngs.ContainsKey(txt))
                {
                    aLngs[txt] += 1;
                }
                else
                {
                    aLngs.Add(txt, 1);
                }
            }

            foreach (var kv in aLngs)
            {
                txt = $"{kv.Value} - {kv.Key}";
                txt += kv.Key.Contains(" x ") ? " SLOT" : " HOLE";
                if (kv.Value > 1)
                {
                    txt += "S";
                }
                if (!string.IsNullOrWhiteSpace(_rVal))
                {
                    _rVal += aLineFeed;
                }
                _rVal += txt;
            }

            return _rVal;
        }

        public colDXFEntities DeckSectionFlowSymbol(dxfImage aImage, double aScale, string aStyleName, double aRotation, mdSlotZone aSlotZone, mdTrayAssembly aAssy, mdDeckSection aDeckSection)
        {
            colDXFEntities _rVal = new();

            try
            {
                if (aScale <= 0)
                    aScale = aImage.Display.PaperScale;


                dxfVector v1;
                dxfVector v2;
                dxfVector v3;
                dxeLine bLn;
                dxeSolid aSld;
                dxeSolid bSld;
                colDXFEntities recSegs;
                double rad;

                double ang;

                dxeText aTxt;
                dxeText bTxt;
                dxfRectangle aRec;
                dxfPlane aPln;


                aSlotZone ??= aDeckSection.SlotZone(aAssy);
                aSlotZone.UpdateSlotPoints(aAssy);
                uopVectors aPts = aSlotZone.GridPoints(false,true);
                List<double> aAngs = aSlotZone.SlotAngles;

                if (aAngs.Count <= 0)
                {
                    aSlotZone.UpdateSlotPoints(aAssy, bForceRegen: true);
                    aAngs = aSlotZone.SlotAngles;
                    if (aAngs.Count <= 0)
                        return _rVal;
                }
                rad = 0.5 * aScale;
                aTxt = aImage.TextStyle(aStyleName).CreateText("FLOW", dxxTextTypes.Multiline, dxxTextDrawingDirections.Horizontal, 0.125 * aScale, dxxMTextAlignments.MiddleCenter, aImage: aImage);
                aTxt.DisplaySettings = aImage.GetDisplaySettings(dxxEntityTypes.Text, "TEXT", aStyleName: aStyleName);
                aRec = aTxt.BoundingRectangle();
                aRec.Stretch(0.125 * aScale);
                aPln = aRec.Plane;
                v1 = aRec.Center;
                dxeLine aLn = new(v1, dxfVector.Zero, aImage.GetDisplaySettings(dxxEntityTypes.DimLinearH));

                _rVal.Add(aTxt);
                recSegs = aRec.BorderLines();
                aSld = new dxeSolid(dxfVector.Zero, new dxfVector(-1 / 8 * aScale, 1 / 32 * aScale), new dxfVector(-1 / 8 * aScale, -1 / 32 * aScale), aDisplaySettings: aLn.DisplaySettings);

                for (int i = 1; i <= aAngs.Count; i++)
                {
                    ang = aAngs[i - 1];
                    ang = dxfUtils.NormalizeAngle(ang + aRotation + 180, false, false, true);
                    bLn = aLn.Clone();
                    v3 = aPln.VectorPolar(rad, ang);
                    bLn.EndPt = v3;
                    v2 = aRec.IntersectionPts(bLn, false).NearestVector(bLn.EndPt);

                    bLn.StartPt = v2;
                    _rVal.Add(bLn);

                    bSld = aSld.Clone();
                    bSld.Rotate(bLn.AngleOfInclination());
                    bSld.Move(v3.X, v3.Y);
                    _rVal.Add(bSld);

                    bTxt = aTxt.Clone();
                    bTxt.TextString = string.Format("{0:0}", ang) + "%%D";
                    bTxt.AlignmentPt1 = v3.Projected(bLn.Direction(), 0.125 * aScale);
                    _rVal.Add(bTxt);
                }

                v3 = aPln.VectorPolar(rad, 0);
                aLn.EndPt = v3;
                aLn.StartPt = aRec.MiddleRight();
                _rVal.Add(aLn);
                bTxt = aTxt.Clone();
                bTxt.TextString = "0%%D";
                bTxt.AlignmentPt1 = aLn.EndPt.Projected(aLn.Direction(), 0.125 * aScale);
                _rVal.Add(bTxt);
            }
            catch (Exception e)
            {
                aImage.HandleError(System.Reflection.MethodBase.GetCurrentMethod(), this.GetType().Name, e.Message);
            }

            return _rVal;
        }

        private double FindDistanceBetweenLastOpeningAndEndOfBeam(double openingWidth, double gapBetweenOpenings, int numberOfOpenings, double beamLength)
        {
            if (numberOfOpenings == 0)
            {
                return beamLength / 2;
            }

            double numberOfOpeningsInHalfOfBeam = numberOfOpenings / 2.0; // It is double to cover the odd number of openings

            // Note that, in even case, the first gap is in half (with respect to the middle of the beam) and there are as many gaps as openings in half of beam
            // In odd case, we have as many gaps as the full openings which is a half less than the number of openings in half of beam
            // This is why the formula is the same in both cases.
            double numberOfGapsInHalfOfBeam = numberOfOpeningsInHalfOfBeam - 0.5; // It is double to cover the half gaps

            double distanceFromCenterOfBeamToLastOpening = numberOfOpeningsInHalfOfBeam * openingWidth + numberOfGapsInHalfOfBeam * gapBetweenOpenings;
            double distanceBetweenLastOpeningAndEndOfBeam = (beamLength / 2) - distanceFromCenterOfBeamToLastOpening;

            return distanceBetweenLastOpeningAndEndOfBeam;
        }

        private IEnumerable<dxfEntity> GetWebOpeningsForBeamDetail(double webOpeningSize, double webOpeningsGap, double x, double y, double cornerRadius, int numberOfWebOpenings)
        {
            // x and y are the coordinates of the beam from where we start laying out the web openings. It is expected to be the middle of the right edge of the partial beam
            double halfWebOpeningSize = webOpeningSize / 2;
            double halfWebOpeningsGap = webOpeningsGap / 2;

            List<dxfEntity> dxfEntities = new List<dxfEntity>();

            switch (numberOfWebOpenings)
            {
                case 0:
                    break;
                case 1:
                    dxfEntities.AddRange(GetLeftHalfWebOpening(webOpeningSize, x, y, cornerRadius));
                    break;
                case 2:
                    dxfEntities.AddRange(GetWebOpening(webOpeningSize, x - halfWebOpeningsGap - halfWebOpeningSize, y, cornerRadius)); // Center is half gap and half opening size away
                    break;
                case 3:
                    dxfEntities.AddRange(GetLeftHalfWebOpening(webOpeningSize, x, y, cornerRadius));
                    dxfEntities.AddRange(GetWebOpening(webOpeningSize, x - webOpeningSize - webOpeningsGap, y, cornerRadius)); // Center is half opening size for the first half opening, full gap, and another half opening size away
                    break;
                default:
                    // 4 and more openings
                    dxfEntities.AddRange(GetWebOpening(webOpeningSize, x - halfWebOpeningsGap - halfWebOpeningSize, y, cornerRadius)); // Center is half gap and half opening size away
                    dxfEntities.AddRange(GetWebOpening(webOpeningSize, x - (1.5 * webOpeningsGap) - (1.5 * webOpeningSize), y, cornerRadius)); // Center is 1.5 gaps and 1.5 opening sizes away
                    break;
            }

            return dxfEntities;
        }

        private IEnumerable<dxfEntity> GetLeftHalfWebOpening(double webOpeningSize, double webOpeningX, double webOpeningY, double cornerRadius)
        {
            colDXFEntities entities = new colDXFEntities();

            double halfWebOpeningSize = webOpeningSize / 2;
            double distanceToCornerRadius = halfWebOpeningSize - cornerRadius;

            double topArcCenterY = webOpeningY + distanceToCornerRadius;
            double bottomArcCenterY = webOpeningY - distanceToCornerRadius;
            double leftArcCenterX = webOpeningX - distanceToCornerRadius;

            double topWebOpeningY = webOpeningY + halfWebOpeningSize;
            double bottomWebOpeningY = webOpeningY - halfWebOpeningSize;
            double leftWebOpeningX = webOpeningX - halfWebOpeningSize;

            entities.AddLine(webOpeningX, topWebOpeningY, leftArcCenterX, topWebOpeningY); // Top line of the web opening
            entities.AddArc(leftArcCenterX, topArcCenterY, cornerRadius, 90, 180); // Top left arc of the web opening
            entities.AddLine(leftWebOpeningX, topArcCenterY, leftWebOpeningX, bottomArcCenterY); // Left line of the web opening
            entities.AddArc(leftArcCenterX, bottomArcCenterY, cornerRadius, 180, 270); // Bottom left arc of the web opening
            entities.AddLine(leftArcCenterX, bottomWebOpeningY, webOpeningX, bottomWebOpeningY); // Bottom line of the web opening

            return entities;
        }

        private IEnumerable<dxfEntity> GetWebOpening(double webOpeningSize, double webOpeningX, double webOpeningY, double cornerRadius)
        {
            colDXFEntities entities = new colDXFEntities();

            double halfWebOpeningSize = webOpeningSize / 2;
            double distanceToCornerRadius = halfWebOpeningSize - cornerRadius;

            double topArcCenterY = webOpeningY + distanceToCornerRadius;
            double bottomArcCenterY = webOpeningY - distanceToCornerRadius;
            double leftArcCenterX = webOpeningX - distanceToCornerRadius;
            double rightArcCenterX = webOpeningX + distanceToCornerRadius;

            double topWebOpeningY = webOpeningY + halfWebOpeningSize;
            double bottomWebOpeningY = webOpeningY - halfWebOpeningSize;
            double leftWebOpeningX = webOpeningX - halfWebOpeningSize;
            double rightWebOpeningX = webOpeningX + halfWebOpeningSize;

            entities.AddLine(rightArcCenterX, topWebOpeningY, leftArcCenterX, topWebOpeningY); // Top line of the web opening
            entities.AddArc(leftArcCenterX, topArcCenterY, cornerRadius, 90, 180); // Top left arc of the web opening
            entities.AddLine(leftWebOpeningX, topArcCenterY, leftWebOpeningX, bottomArcCenterY); // Left line of the web opening
            entities.AddArc(leftArcCenterX, bottomArcCenterY, cornerRadius, 180, 270); // Bottom left arc of the web opening
            entities.AddLine(leftArcCenterX, bottomWebOpeningY, rightArcCenterX, bottomWebOpeningY); // Bottom line of the web opening
            entities.AddArc(rightArcCenterX, bottomArcCenterY, cornerRadius, 270, 360); // Bottom right arc of the web opening
            entities.AddLine(rightWebOpeningX, bottomArcCenterY, rightWebOpeningX, topArcCenterY); // Right line of the web opening
            entities.AddArc(rightArcCenterX, topArcCenterY, cornerRadius, 0, 90); // Top right arc of the web opening

            return entities;
        }

        private IEnumerable<dxfEntity> GetBeamHeightDimensionForBeamDetail(double beamLeftEdge, double beamTopOut, double beamBottomOut, dxfImage image)
        {
            colDXFEntities entities = new colDXFEntities();

            double dimensionLineGap = 1.4; // Gap between the left edge of the beam and the horizontal dimension lines
            double dimensionArrowGap = 2.9143; // Gap between the dimension arrow line and the left end of the horizontal dimension lines
            double arrowLineLength = 2.9143; // Length of the dimension arrow line
            double arrowSolidLength = 2.9143; // Length of the arrow's solid part in non rotated shape (facing toward right)
            double arrowSolidHalfHeight = 0.4857; // Half height of the arrow's solid part in non rotated shape (facing toward right)

            double beamHeight = beamTopOut - beamBottomOut; // Height of the beam
            double textHeight = 3.6429;
            bool needsRotation = textHeight >= beamHeight;
            bool textShouldBeSingleLine = (textHeight * 2) >= beamHeight;

            // Dimension text
            string heightStringInInches = $"{image.FormatNumber(beamHeight)}";
            string heightStringInMilliMeters = $"[{image.FormatNumber(beamHeight * 25.4)}mm]";
            string separator = textShouldBeSingleLine ? " " : "\r\n";
            string txt = $"\\A1;{heightStringInInches}{separator}{heightStringInMilliMeters}";

            dxeText dimensionText = new() { DisplaySettings = image.GetDisplaySettings(dxxEntityTypes.MText), Alignment = dxxMTextAlignments.MiddleCenter, TextHeight = textHeight, TextStyleName = "ROMANS" };
            dimensionText.TextString = txt;

            double dimensionHorizontalLineLength = (2 * dimensionArrowGap); // Length of the horizontal dimension line
            if (!needsRotation)
            {
                dimensionHorizontalLineLength += (dimensionText.Length / 2); // Length of the horizontal dimension line
            }

            double dimensionHorizontalLineLeftX = beamLeftEdge - dimensionLineGap - dimensionHorizontalLineLength;
            double dimensionHorizontalLineRightX = beamLeftEdge - dimensionLineGap;
            double dimensionVerticalLineX = dimensionHorizontalLineLeftX + dimensionArrowGap;



            // Dimension horizontal lines
            entities.AddLine(dimensionHorizontalLineLeftX, beamTopOut, dimensionHorizontalLineRightX, beamTopOut); // Top horizontal dimension line
            entities.AddLine(dimensionHorizontalLineLeftX, beamBottomOut, dimensionHorizontalLineRightX, beamBottomOut); // Bottom horizontal dimension line

            // Arrow vertical lines
            if (needsRotation)
            {
                // Arrow vertical lines should be within the horizontal dimention lines
                entities.AddLine(dimensionVerticalLineX, beamTopOut - arrowSolidLength, dimensionVerticalLineX, beamTopOut - arrowSolidLength - arrowLineLength); // Top dimension arrow line
                entities.AddLine(dimensionVerticalLineX, beamBottomOut + arrowSolidLength + arrowLineLength, dimensionVerticalLineX, beamBottomOut + arrowSolidLength); // Bottom dimension arrow line 
            }
            else
            {
                // Arrow vertical lines should be out of the horizontal dimention lines
                entities.AddLine(dimensionVerticalLineX, beamTopOut + arrowSolidLength + arrowLineLength, dimensionVerticalLineX, beamTopOut + arrowSolidLength); // Top dimension arrow line
                entities.AddLine(dimensionVerticalLineX, beamBottomOut - arrowSolidLength, dimensionVerticalLineX, beamBottomOut - arrowSolidLength - arrowLineLength); // Bottom dimension arrow line 
            }

            // Arrow solid parts
            dxeSolid aSld = new() { Triangular = true, Vertex2 = new dxfVector(-arrowSolidLength, arrowSolidHalfHeight), Vertex3 = new dxfVector(-arrowSolidLength, -arrowSolidHalfHeight) };

            if (needsRotation)
            {
                entities.Add(aSld.Rotated(90, dimensionVerticalLineX, beamTopOut)); // Top dimension arrow solid
                entities.Add(aSld.Rotated(270, dimensionVerticalLineX, beamBottomOut)); // Bottom dimension arrow solid 
            }
            else
            {
                entities.Add(aSld.Rotated(270, dimensionVerticalLineX, beamTopOut)); // Top dimension arrow solid
                entities.Add(aSld.Rotated(90, dimensionVerticalLineX, beamBottomOut)); // Bottom dimension arrow solid 
            }


            // Add dimension text
            if (needsRotation)
            {
                double textInsertionX = dimensionHorizontalLineLeftX - 8 - (dimensionText.TextHeight / 2);
                dimensionText.InsertionPt = new dxfVector(textInsertionX, (beamTopOut + beamBottomOut) / 2); // We expect the y to be right in the middle
                dimensionText.Rotation = 90;
            }
            else
            {
                dimensionText.InsertionPt = new dxfVector(dimensionVerticalLineX, (beamTopOut + beamBottomOut) / 2); // We expect the y to be right in the middle
            }

            entities.Add(dimensionText);

            return entities;
        }

        private IEnumerable<dxfEntity> GetLastWebOpeningDistanceToEndOfBeamDimensionForBeamDetail(double distance, double beamLeftEdge, double beamBottomOut, double webOpeningBottom, dxfImage image)
        {
            colDXFEntities entities = new colDXFEntities();

            double leftVerticalDimensionLineGap = 1.4; // Gap between the left vertical dimension line and bottom of the beam
            double rightVerticalDimensionLineLength = 27.031;
            double dimensionArrowGap = 2.9143; // Gap between the dimension arrow line and the bottom end of the vertical dimension lines
            double arrowLineLength = 2.5567; // Length of the dimension arrow line
            double arrowSolidLength = 2.9143; // Length of the arrow's solid part in non rotated shape (facing toward right)
            double arrowSolidHalfHeight = 0.4857; // Half height of the arrow's solid part in non rotated shape (facing toward right)

            double rightDimensionVerticalLineX = beamLeftEdge + distance;
            double rightDimensionVerticalLineBottomY = webOpeningBottom - rightVerticalDimensionLineLength;
            double leftDimensionVerticalLineTopY = beamBottomOut - leftVerticalDimensionLineGap;

            double arrowDimensionLinesY = rightDimensionVerticalLineBottomY + dimensionArrowGap;
            double leftDimensionArrowLineRightX = beamLeftEdge + arrowSolidLength + arrowLineLength;
            double rightDimensionArrowLineLeftX = rightDimensionVerticalLineX - arrowSolidLength - arrowLineLength;

            // Dimension vertical lines
            entities.AddLine(beamLeftEdge, leftDimensionVerticalLineTopY, beamLeftEdge, rightDimensionVerticalLineBottomY); // Left vertical dimension line
            entities.AddLine(rightDimensionVerticalLineX, webOpeningBottom, rightDimensionVerticalLineX, rightDimensionVerticalLineBottomY); // Right vertical dimension line

            // Arrow horizontal lines
            entities.AddLine(beamLeftEdge + arrowSolidLength, arrowDimensionLinesY, leftDimensionArrowLineRightX, arrowDimensionLinesY); // Top leader arrow line
            entities.AddLine(rightDimensionArrowLineLeftX, arrowDimensionLinesY, rightDimensionVerticalLineX - arrowSolidLength, arrowDimensionLinesY); // Bottom leader arrow line

            // Arrow solid parts
            dxeSolid aSld = new() { Triangular = true, Vertex2 = new dxfVector(-arrowSolidLength, arrowSolidHalfHeight), Vertex3 = new dxfVector(-arrowSolidLength, -arrowSolidHalfHeight) };

            entities.Add(aSld.Rotated(180, beamLeftEdge, arrowDimensionLinesY)); // Left dimension arrow solid
            entities.Add(aSld.Rotated(0, rightDimensionVerticalLineX, arrowDimensionLinesY)); // Right dimension arrow solid

            // Dimension text
            string distanceInInches = $"{image.FormatNumber(distance)}";
            string distanceInMilliMeters = $"[{image.FormatNumber(distance * 25.4)}mm]";
            string txt = $"\\A1;{distanceInInches}\r\n[{distanceInMilliMeters}mm]";

            dxeText dimensionText = new() { DisplaySettings = image.GetDisplaySettings(dxxEntityTypes.MText), Alignment = dxxMTextAlignments.MiddleCenter, TextHeight = 3.6429, TextStyleName = "ROMANS" };
            dimensionText.InsertionPt = new dxfVector((beamLeftEdge + rightDimensionVerticalLineX) / 2, arrowDimensionLinesY); // We expect the x to be right in the middle
            dimensionText.TextString = txt;

            if (dimensionText.Length >= rightDimensionArrowLineLeftX - leftDimensionArrowLineRightX) // Text does not fit
            {
                dimensionText.TextString = $"\\A1;{distanceInInches} {distanceInMilliMeters}";

                double offset = dimensionArrowGap + 2 + (dimensionText.TextHeight / 2);
                dxfVector newInsertionPoint = new dxfVector(dimensionText.InsertionPt.X, dimensionText.InsertionPt.Y - offset);
                dimensionText.InsertionPt = newInsertionPoint;
            }

            entities.Add(dimensionText);

            return entities;
        }

        private IEnumerable<dxfEntity> GetWebOpeningWidthDimensionForBeamDetail(double webOpeningLeftX, double webOpeningRightX, double webOpeningBottom, double dimensionVerticalLinesLength, dxfImage image)
        {
            colDXFEntities entities = new colDXFEntities();

            double dimensionArrowGap = 2.9143; // Gap between the dimension arrow line and the bottom end of the vertical dimension lines
            double arrowLineLength = 2.9143; // Length of the dimension arrow line
            double arrowSolidLength = 2.9143; // Length of the arrow's solid part in non rotated shape (facing toward right)
            double arrowSolidHalfHeight = 0.4857; // Half height of the arrow's solid part in non rotated shape (facing toward right)
            double gapBetweenDimensionTextAndArrowLine = 1.7546; // Gap between the left edge of the dimension text and right end of the arrow line

            double webOpeningWidth = webOpeningRightX - webOpeningLeftX;
            double dimensionVerticalLinesBottomY = webOpeningBottom - dimensionVerticalLinesLength;
            double arrowLinesY = dimensionVerticalLinesBottomY + dimensionArrowGap;
            double totalArrowLength = arrowLineLength + arrowSolidLength;

            // Dimension vertical lines
            entities.AddLine(webOpeningLeftX, webOpeningBottom, webOpeningLeftX, dimensionVerticalLinesBottomY); // Left vertical dimension line
            entities.AddLine(webOpeningRightX, webOpeningBottom, webOpeningRightX, dimensionVerticalLinesBottomY); // Right vertical dimension line

            // Arrow horizontal lines
            entities.AddLine(webOpeningLeftX - totalArrowLength, arrowLinesY, webOpeningLeftX - arrowSolidLength, arrowLinesY); // Left dimension arrow line
            entities.AddLine(webOpeningRightX + arrowSolidLength, arrowLinesY, webOpeningRightX + totalArrowLength, arrowLinesY); // Right dimension arrow line

            // Arrow solid parts
            dxeSolid aSld = new() { Triangular = true, Vertex2 = new dxfVector(-arrowSolidLength, arrowSolidHalfHeight), Vertex3 = new dxfVector(-arrowSolidLength, -arrowSolidHalfHeight) };

            entities.Add(aSld.Rotated(0, webOpeningLeftX, arrowLinesY)); // Left dimension arrow solid
            entities.Add(aSld.Rotated(180, webOpeningRightX, arrowLinesY)); // Right dimension arrow solid

            // Dimension text
            string txt = $"\\A1;{image.FormatNumber(webOpeningWidth)} [{image.FormatNumber(webOpeningWidth * 25.4)}mm] (TYP)";

            dxeText dimensionText = new() { DisplaySettings = image.GetDisplaySettings(dxxEntityTypes.MText), Alignment = dxxMTextAlignments.MiddleCenter, TextHeight = 3.6429, TextStyleName = "ROMANS" };
            dimensionText.TextString = txt;
            dimensionText.InsertionPt = new dxfVector(webOpeningRightX + totalArrowLength + gapBetweenDimensionTextAndArrowLine + dimensionText.Length / 2, arrowLinesY);

            entities.Add(dimensionText);

            return entities;
        }

        private IEnumerable<dxfEntity> GetWebOpeningHeightDimensionForBeamDetail(double webOpeningRightX, double webOpeningTopY, double webOpeningBottomY, double beamRightSide, dxfImage image)
        {
            colDXFEntities entities = new colDXFEntities();

            double dimensionArrowGap = 2.9143; // Gap between the dimension arrow line and the right end of the horizontal dimension lines
            double arrowLineLength = 2.9143; // Length of the dimension arrow line
            double arrowSolidLength = 2.9143; // Length of the arrow's solid part in non rotated shape (facing toward right)
            double arrowSolidHalfHeight = 0.4857; // Half height of the arrow's solid part in non rotated shape (facing toward right)
            double horizontalDimensionLinesMinLength = 36; // Length of the horizontal dimension line after right edge of the beam

            double webOpeningHeight = webOpeningTopY - webOpeningBottomY;
            double horizontalDimensionLinesRightX = beamRightSide + horizontalDimensionLinesMinLength;
            double dimensionArrowLinesX = horizontalDimensionLinesRightX - dimensionArrowGap;
            double totalArrowLength = arrowLineLength + arrowSolidLength;

            // Dimension horizontal lines
            entities.AddLine(webOpeningRightX, webOpeningTopY, horizontalDimensionLinesRightX, webOpeningTopY); // Top horizontal dimension line
            entities.AddLine(webOpeningRightX, webOpeningBottomY, horizontalDimensionLinesRightX, webOpeningBottomY); // Bottom horizontal dimension line

            // Arrow vertical lines
            entities.AddLine(dimensionArrowLinesX, webOpeningTopY + totalArrowLength, dimensionArrowLinesX, webOpeningTopY); // Top dimension arrow line
            entities.AddLine(dimensionArrowLinesX, webOpeningBottomY, dimensionArrowLinesX, webOpeningBottomY - totalArrowLength); // Bottom dimension arrow line

            // Arrow solid parts
            dxeSolid aSld = new() { Triangular = true, Vertex2 = new dxfVector(-arrowSolidLength, arrowSolidHalfHeight), Vertex3 = new dxfVector(-arrowSolidLength, -arrowSolidHalfHeight) };

            entities.Add(aSld.Rotated(270, dimensionArrowLinesX, webOpeningTopY)); // Top dimension arrow solid
            entities.Add(aSld.Rotated(90, dimensionArrowLinesX, webOpeningBottomY)); // Bottom dimension arrow solid

            // Dimension text
            string txt = $"\\A1;{image.FormatNumber(webOpeningHeight)} [{image.FormatNumber(webOpeningHeight * 25.4)}mm] (TYP)";

            dxeText dimensionText = new() { DisplaySettings = image.GetDisplaySettings(dxxEntityTypes.MText), Alignment = dxxMTextAlignments.MiddleCenter, TextHeight = 3.6429, TextStyleName = "ROMANS" };
            dimensionText.TextString = txt;
            dimensionText.InsertionPt = new dxfVector(dimensionArrowLinesX, (webOpeningTopY + webOpeningBottomY) / 2);

            entities.Add(dimensionText);

            return entities;
        }

        private IEnumerable<dxfEntity> GetWebOpeningLeaderForBeamDetail(string text, double leaderPointX, double leaderPointY, double angle, double arrowLineLength, double lengthOfLineAttachedToText, dxfImage image)
        {
            colDXFEntities entities = new colDXFEntities();

            double sign = lengthOfLineAttachedToText >= 0 ? 1 : -1; // Note that the lengthOfLineAttachedToText can be negative to support moving in both directions
            double arrowSolidLength = 2.9143; // Length of the arrow's solid part in non rotated shape (facing toward right)
            double arrowSolidHalfHeight = 0.4857; // Half height of the arrow's solid part in non rotated shape (facing toward right)
            double textGap = 1.7486; // Gap between the leader line attached to text and the text

            dxfVector arrowLineStart = new dxfVector(leaderPointX, leaderPointY);
            arrowLineStart.MovePolar(angle, arrowSolidLength);
            dxfVector arrowLineEnd = new dxfVector(arrowLineStart.X, arrowLineStart.Y);
            arrowLineEnd.MovePolar(angle, arrowLineLength);
            double leaderLineToTextX = arrowLineEnd.X + lengthOfLineAttachedToText; // The leader line that attaches to text has a side closer to text. This is its X.


            // Leader arrow lines
            entities.AddLine(arrowLineStart.X, arrowLineStart.Y, arrowLineEnd.X, arrowLineEnd.Y); // Leader arrow line
            entities.AddLine(arrowLineEnd.X, arrowLineEnd.Y, leaderLineToTextX, arrowLineEnd.Y); // Leader line attached to text

            // Leader solid part
            dxeSolid aSld = new() { Triangular = true, Vertex2 = new dxfVector(-arrowSolidLength, arrowSolidHalfHeight), Vertex3 = new dxfVector(-arrowSolidLength, -arrowSolidHalfHeight) };
            entities.Add(aSld.Rotated(180 + angle, leaderPointX, leaderPointY)); // Top dimension arrow solid

            // Leader text
            dxeText dimensionText = new() { DisplaySettings = image.GetDisplaySettings(dxxEntityTypes.MText), Alignment = dxxMTextAlignments.MiddleCenter, TextHeight = 3.6429, TextStyleName = "ROMANS" };
            dimensionText.TextString = text;
            dimensionText.InsertionPt = new dxfVector(leaderLineToTextX + (sign * (textGap + dimensionText.Length / 2)), arrowLineEnd.Y);

            entities.Add(dimensionText);

            return entities;
        }

        private IEnumerable<dxfEntity> GetBreakLineForBeamDetail(double beamRightSide, double beamTopOut, double beamBottomOut, double zigZagLength, double zigZagLineAngle, double extension)
        {
            // This method draws two zigzags, one in top half of the beam height and another in the bottom half.
            // The center of the zigzagz will be at the middle of each half (or at 1/4 and 3/4 beam height).
            // The break line will extend as much as extension in top and bottom of the beam.
            // More info about the definition of the zigzag angel in "FindZigZagPoints" method

            double oneFourthOfBeamHeight = (beamTopOut - beamBottomOut) * 0.25;
            double halfZigZagLength = 0.5 * zigZagLength;
            double zigZagCenterY;

            dxfVector topZigZagPointUpperHalf, bottomZigZagPointUpperHalf;
            zigZagCenterY = beamTopOut - oneFourthOfBeamHeight;
            double upperZigZagStartY = zigZagCenterY + halfZigZagLength;
            double upperZigZagEndY = zigZagCenterY - halfZigZagLength;
            (topZigZagPointUpperHalf, bottomZigZagPointUpperHalf) = FindZigZagPoints(upperZigZagStartY, upperZigZagEndY, beamRightSide, zigZagLineAngle);

            dxfVector topZigZagPointLowerHalf, bottomZigZagPointLowerHalf;
            zigZagCenterY = beamBottomOut + oneFourthOfBeamHeight;
            double lowerZigZagStartY = zigZagCenterY + halfZigZagLength;
            double lowerZigZagEndY = zigZagCenterY - halfZigZagLength;
            (topZigZagPointLowerHalf, bottomZigZagPointLowerHalf) = FindZigZagPoints(lowerZigZagStartY, lowerZigZagEndY, beamRightSide, zigZagLineAngle);

            colDXFEntities entities = new colDXFEntities();
            List<dxfVector> polylinePoints = new List<dxfVector>();

            polylinePoints.Add(new dxfVector(beamRightSide, beamTopOut + extension));
            polylinePoints.Add(new dxfVector(beamRightSide, upperZigZagStartY));
            polylinePoints.Add(topZigZagPointUpperHalf);
            polylinePoints.Add(bottomZigZagPointUpperHalf);
            polylinePoints.Add(new dxfVector(beamRightSide, upperZigZagEndY));
            polylinePoints.Add(new dxfVector(beamRightSide, lowerZigZagStartY));
            polylinePoints.Add(topZigZagPointLowerHalf);
            polylinePoints.Add(bottomZigZagPointLowerHalf);
            polylinePoints.Add(new dxfVector(beamRightSide, lowerZigZagEndY));
            polylinePoints.Add(new dxfVector(beamRightSide, beamBottomOut - extension));

            entities.AddPolyline(polylinePoints, false);

            return entities;
        }

        /// <summary>
        /// This method returns the top and bottom zigzag points. It assumes that the line is completely vertical.
        /// </summary>
        /// <param name="topY">Ordinate from which the zigzag line starts at the top</param>
        /// <param name="bottomY">Ordinate at which the zigzag line ends at the bottom</param>
        /// <returns></returns>
        private (dxfVector, dxfVector) FindZigZagPoints(double topY, double bottomY, double x, double angle)
        {
            // Each zigzag comprises of two similar isosceles triangles, one in top right and one in bottom left.
            // Each of these isosceles triangles contains two right-angled triangles (so, 4 in total).
            // Each right-angled triangle has a vertical side as long as 1/4 of the distance between top and the bottom of the zigzag area.
            // The "angle" value is the angle between the vertical side and the hypotenuse of the right-angled triangle (at the very top).
            // We need to find the length of the other side and to do that we need to find the length of the hypotenuse first.

            double angleInRadians = angle * (Math.PI / 180);

            double adjacentSideLength = (topY - bottomY) / 4;
            double hypotenuse = adjacentSideLength / Math.Cos(angleInRadians);
            double oppositeSideLength = hypotenuse * Math.Sin(angleInRadians);

            dxfVector topZigZagPoint = new dxfVector(x + oppositeSideLength, topY - adjacentSideLength);
            dxfVector bottomZigZagPoint = new dxfVector(x - oppositeSideLength, bottomY + adjacentSideLength);

            return (topZigZagPoint, bottomZigZagPoint);
        }

        private double FindBottomLeftHoleDistanceFromBeamLeftSide(uopVector bottomLeftHoleCenter, mdBeam beam)
        {
            var verticies = beam.Vertices();
            var topLeftVertex = new dxfVector(verticies[0]);
            var bottomLeftVertex = new dxfVector(verticies[1]);
            dxeLine beamLeftEdge = new dxeLine(topLeftVertex, bottomLeftVertex); // This is the left edge of the beam

            dxfVector holeCenter = new dxfVector(bottomLeftHoleCenter);

            dxfVector projectedPoint = holeCenter.ProjectedToLine(beamLeftEdge);

            double distance = Math.Sqrt(Math.Pow(holeCenter.X - projectedPoint.X, 2) + Math.Pow(holeCenter.Y - projectedPoint.Y, 2));
            return distance;
        }

        public List<dxfBlock>  DDWG_FreeBubblingAreas(IappDrawingSource aSource, bool bRegenBlockedAreas = false)
        {
            List < dxfBlock > _rVal = new List<dxfBlock> ();

            try
            {
                if (aSource.Image == null || aSource.MDAssy == null) return _rVal;
                dxfImage Image = aSource.Image;
           
                mdTrayAssembly Assy = aSource.MDAssy;

                bool isspout = Assy.ProjectType == uppProjectTypes.MDSpout;

                dxoDrawingTool draw = Image.Draw;
                Status = !isspout ? "Drawing Free Bubbling Areas And Blocked Areas" : "Drawing Free Bubbling Areas";

                if (isspout) bRegenBlockedAreas = false;
                //Image.TextStyle().FontName = "RomanS.shx";
                //Image.TextStyle().WidthFactor = 0.8;
                dxoDimStyle dstyle = Image.DimStyle();
                uopFreeBubblingPanels fbps = Assy.FreeBubblingPanels;

              
                int pid = 0;
                string txt;
                dxfVector txtpt = null;

                double maxWd = 0;
                dxeText thistxt = null;
                bool swapit = pid <= 0;
                dxfVector v1;
                uopUnit aunits = uopUnits.GetUnit(uppUnitTypes.SmallArea);
                uopUnit lunits = uopUnits.GetUnit(uppUnitTypes.SmallLength);
                uopUnit baunits = uopUnits.GetUnit(uppUnitTypes.BigArea);
                uopUnit blunits = uopUnits.GetUnit(uppUnitTypes.BigLength);
                double shellrad = Assy.ShellID * 0.5;
                bool symmetric = Assy.IsSymmetric;
                dxxColors vircolor = dxxColors.Yellow;
                dxfDisplaySettings dsp1 = new dxfDisplaySettings(aLayer: "TEXT", aColor: dxxColors.Blue);
                dxfDisplaySettings dsp2 = new dxfDisplaySettings(aLayer: "TEXT", aColor: vircolor);
                mdSpacingData spacingdata = Assy.SpacingData;
              
                uopShapes blockedareas = new uopShapes("BLOCKED AREAS"); 
                dxoEntityTool ents = Image.EntityTool;
                uppUnitFamilies units = aSource.Drawing.DrawingUnits;
                if (!isspout)
                {
                    Image.Layers.Add("BLOCKED AREAS", dxxColors.Red);
                }
                draw.aCircle(dxfVector.Zero, shellrad);
                draw.aCircle(dxfVector.Zero, Assy.RingID / 2, dxfDisplaySettings.Null(aLinetype: dxfLinetypes.Hidden));
                draw.aCircle(dxfVector.Zero, Assy.BoundingRadius, dxfDisplaySettings.Null(aColor: dxxColors.LightBlue, aLinetype: dxfLinetypes.Phantom));
                draw.aCircle(dxfVector.Zero, Assy.DeckRadius, dxfDisplaySettings.Null(aColor: vircolor, aLinetype: dxfLinetypes.Continuous));

                double paperscale = Image.Display.ZoomExtents(bSetFeatureScale: true);

                if (Assy.DesignFamily.IsBeamDesignFamily()) 
                {
                    dxfBlock blk = mdBlocks.Beam_View_Plan(Image, Assy.Beam,Assy, bSetInstances: true, bObscured: true, bSuppressHoles: true, aLayerName: "BEAMS", aLTLSetting: dxxLinetypeLayerFlag.ForceToColor);
                    draw.aInserts(blk, blk.Instances, false);
                }

                foreach (mdDowncomer dc in Assy.Downcomers)
                {
                    List<mdDowncomerBox> boxes = dc.Boxes;
                    foreach (mdDowncomerBox box in boxes)
                    {
                        Image.Entities.Append(box.Edges("BOXES", dxxColors.BlackWhite, dxfLinetypes.Continuous));
                        draw.aPolyline(box.EndPlate(bTop: true).EdgeVertices(), true, aDisplaySettings: new dxfDisplaySettings("END PLATES", dxxColors.BlackWhite, dxfLinetypes.Continuous));
                        draw.aPolyline(box.EndPlate(bTop: false).EdgeVertices(), true, aDisplaySettings: new dxfDisplaySettings("END PLATES", dxxColors.BlackWhite, dxfLinetypes.Continuous));
                    }

                }


                foreach (uopFreeBubblingPanel fbp in fbps)
                {
                    foreach (uopFreeBubblingArea fba in fbp)
                    {
                        if (fba.IsVirtual) continue;


                        int occurs = fba.OccuranceFactor;
                        uopLinePair weirs = fba.WeirLines;
                        uopLine weirL = weirs.GetSide(uppSides.Left);
                        uopLine weirR = weirs.GetSide(uppSides.Right);

                        v1 = new dxfVector(fba.Center);
                
                        dxfBlock blk = fba.Block(Assy, null, out uopShapes fbaBAs, aBoundColor: fba.IsVirtual ? vircolor : dxxColors.Blue, aLeftWeirColor: fba.IsVirtual ? vircolor : dxxColors.Green, aRightWeirColor: fba.IsVirtual ? vircolor : dxxColors.LightBlue, bSetInstances: true, bIncludeBlockedAreas: !isspout,bRegenBlockedAreas: bRegenBlockedAreas); //, bIncludeBlockedAreas: !isspout,  aBlockedAreas: blockedareas);
                        blk.Entities.Add(ents.Create_Text(null, fba.Handle, aTextHeight: 0.08 * paperscale, aAlignment: dxxMTextAlignments.MiddleCenter));
                        if (!isspout) blockedareas.AddRange(fbaBAs);
                        
                        dxeInsert insert= draw.aInserts(blk, blk.Instances, false)[0];
                        _rVal.Add(Image.Blocks.Item(insert.BlockName));

                        v1.Y = -shellrad - 0.25 * paperscale;
                        txt = $"FBA({fba.Handle})";
                        txt = $"{txt}\\POCCURS: {occurs}";

                        if (occurs <= 1)
                        {
                            txt = $"{txt}\\PTOTAL AREA: {aunits.UnitValueString(fba.Area, units)}";
                            txt = $"{txt}\\PTOTAL WEIR LENGTH: {lunits.UnitValueString(fba.WeirLength_Left, units, bAddLabel: false)} + {lunits.UnitValueString(fba.WeirLength_Right, units, bAddLabel: false)} = {lunits.UnitValueString(fba.TotalWeirLength, units)}";

                        }
                        else
                        {
                            txt = $"{txt}\\PTOTAL AREA: {occurs} x  {aunits.UnitValueString(fba.Area, units)} =  {aunits.UnitValueString(fba.TotalArea, units)}";
                            txt = $"{txt}\\PTOTAL WEIR LENGTH: {occurs} x ({lunits.UnitValueString(fba.WeirLength_Left, units, bAddLabel: false)} + {lunits.UnitValueString(fba.WeirLength_Right, units, bAddLabel: false)}) = {lunits.UnitValueString(fba.TotalWeirLength, units)}";

                        }
                        if (isspout)
                        {
                            txt = $"{txt}\\PWEIR FRACTION: {fba.WeirFraction:0.0000}";
                            txt = $"{txt}\\PFBA/WL: {fba.WeirLengthRatio:0.0000}";
                            txt = $"{txt}\\PVL ERROR: {fba.VLError(Assy):0.00} %";
                        }

                        //if (!isspout) txt = $"{txt}\\PBA: {blocked.Count}";




                        txtpt ??= new dxfVector(shellrad + 0.125 * paperscale, shellrad);

                        if (swapit && fba.IsVirtual && symmetric)
                        {
                            txtpt = new dxfVector(-shellrad - maxWd - 0.125 * paperscale, shellrad);
                            swapit = false;
                        }

                        thistxt = draw.aText(txtpt, txt, aDisplaySettings: !fba.IsVirtual ? dsp1 : dsp2, aTextHeight: 0.075 * paperscale, aAlignment: dxxMTextAlignments.TopLeft);
                        dxfRectangle trec = thistxt.BoundingRectangle();
                        txtpt.Y = trec.Bottom - 0.125 * paperscale;
                        if (trec.Width > maxWd) maxWd = trec.Width;
                        // }



                    }
                }

           
                v1 = new dxfVector(0, Image.Entities.BoundingRectangle().Bottom - 0.5 * paperscale);
                txt = $"{Assy.TrayName(true)}";
                double totalfba = fbps.TotalArea();
                txt = $"{txt}\\P{{\\C{(int)dxxColors.Blue};TOTAL FBA : {aunits.UnitValueString(totalfba, units)} [{baunits.UnitValueString(totalfba / 144, units)}]}} ";
                double d1 = fbps.TotalWeirLength();
                txt = $"{txt}\\PTOTAL WEIR LENGTH: {lunits.UnitValueString(d1, units)} [{blunits.UnitValueString(d1 / 12, units)}]";

                if (!isspout)
                {
                    List<string> tags = blockedareas.Tags(true);
                    foreach (string tag in tags)
                    {
                        uopShapes bareas = new uopShapes(blockedareas.FindAll(x => string.Compare(x.Tag, tag, true) == 0),false);
                        uopShape shape1 = bareas.Item(1);
                        txt = $"{txt}\\P{{\\C{(int)dxxColors.Red};'{tag}': -{aunits.UnitValueString(bareas.TotalArea(true), units, bAddLabel: false)} [{bareas.TotalOccurances()} x {aunits.UnitValueString(shape1.Area, units, bAddLabel: false)}]}}";
                    }
                    d1 = blockedareas.TotalArea(true);

                    double mecharea = totalfba - d1;
                    txt = $"{txt}\\P{{\\C{(int)dxxColors.Red};TOTAL BLOCKED AREA : -{aunits.UnitValueString(d1, units)} [{baunits.UnitValueString(d1 / 144, units)}]}} ";
                    txt = $"{txt}\\P{{\\C{(int)dxxColors.Blue};MECHANICAL ACTIVE AREA (A\\H0.5x;\\S^m;\\H2.0x;): {aunits.UnitValueString(mecharea, units)} [{baunits.UnitValueString((totalfba - d1) / 144, units)}]}} ";
                    double functarea = Assy.FunctionalActiveArea;
                    txt = $"{txt}\\P{{\\C{(int)dxxColors.Blue};FUNCTIONAL ACTIVE AREA (A\\H0.5x;\\S^f;\\H2.0x;) : {aunits.UnitValueString(functarea, units)} [{baunits.UnitValueString(functarea / 144, units)}]}} ";

                    double rfactor = mecharea != 0 ? functarea / mecharea : 0;
                    txt = $"{txt}\\P{{\\C{(int)dxxColors.Blue};R-FACTOR (A\\H0.5x;\\S^f;\\H2.0x; / A\\H0.5x;\\S^m;\\H2.0x;) : {rfactor:0.0000}}} ";

                }
                else
                {
                    txt = $"{txt}\\POVERALL FBA/WL: {Assy.Downcomers.FBA2WLRatio:0.0000}";

                    d1 = Assy.Downcomers.Spacing;
                    double d2 = Assy.Downcomers.OptimumSpacing;

                    txt = $"{txt}\\PCURRENT SPACE: {lunits.UnitValueString(d1, units)}";
                    txt = $"{txt}\\PIDEAL SPACE: {lunits.UnitValueString(d2, units)}";
                }
                draw.aText(v1, txt, 0.1 * paperscale, aAlignment: dxxMTextAlignments.TopCenter, aColor: dsp1.Color);


            }
            catch (Exception e) { aSource.HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
            finally { aSource.Status = string.Empty; aSource.DrawinginProgress = false; }
            return _rVal;

        }

        public void DDWG_FunctionalActiveAreas(IappDrawingSource aSource)
        {
          
            try
            {
                if (aSource.Image == null || aSource.MDAssy == null) return;
                dxfImage Image = aSource.Image;

                mdTrayAssembly Assy = aSource.MDAssy;

                bool isspout = Assy.ProjectType == uppProjectTypes.MDSpout;
                Status = "Drawing Functional Active Areas";
                dxoDrawingTool draw = Image.Draw;
                Image.Layers.Add("HEAVY", dxxColors.Grey, aLineWeight: dxxLineWeights.LW_025);
                Image.Layers.Add("POSITIVE AREAS", dxxColors.Green);
                Image.Layers.Add("NEGATIVE AREAS", dxxColors.Red);
                dxeArc shell = draw.aCircle(dxfVector.Zero, Assy.ShellRadius, dxfDisplaySettings.Null("HEAVY"));
                dxeArc arctrimmer = draw.aCircle(dxfVector.Zero, Assy.BoundingRadius, dxfDisplaySettings.Null("0", dxxColors.LightBlue));
                if (Assy.DesignFamily.IsBeamDesignFamily())
                {
                    dxfBlock blk = mdBlocks.Beam_View_Plan(Image, Assy.Beam, Assy, bSetInstances: true, bObscured: true, bSuppressHoles: true, aLayerName: "BEAMS", aLTLSetting: dxxLinetypeLayerFlag.ForceToColor);
                    draw.aInserts(blk, blk.Instances, false);

                    mdBeam beam = Assy.Beam;
                    List<uopLinePair> trimmers = beam.GetEdges(0.5);
                    foreach (var item in trimmers)
                    {
                        draw.aLine(item.Line1, arctrimmer.DisplaySettings);
                        draw.aLine(item.Line2, arctrimmer.DisplaySettings);
                    }

                }

                double rad = 1.05 * Assy.ShellRadius;
                colMDDowncomers dcs = Assy.Downcomers;
                foreach (var dc in dcs)
                {
                    draw.aLine(dc.X, Math.Sqrt(Math.Pow(rad, 2) - Math.Pow(dc.X, 2)), dc.X, -Math.Sqrt(Math.Pow(rad, 2) - Math.Pow(dc.X, 2)), aLineType: dxfLinetypes.Center);
                }

                double paperscale = Image.Display.ZoomExtents(bSetFeatureScale: true);
                draw.aCenterlines(shell, 0.25 * paperscale);
                uopUnit aunits = uopUnits.GetUnit(uppUnitTypes.SmallArea);
                uopUnit bunits = uopUnits.GetUnit(uppUnitTypes.BigArea);
                uppUnitFamilies units = aSource.Drawing.DrawingUnits;

                double posarea = 0;
                double negarea = 0;

                uopShapes functionareas = Assy.FunctionalActiveAreas(out List<uopLinePair> weirs, false, false, false);
                for (int i = 1; i <= functionareas.Count; i++)
                {
                    string lname = i == 1 ? "POSITIVE AREAS" : "NEGATIVE AREAS";
                    uopShape aShp = functionareas.Item(i);
                    if (aShp.Vertices.Count <= 0) continue;
                    //if (aShp.Name == "DCAREA") continue;
                    dxePolyline bounds = draw.aPolyline(aShp.Vertices, bClosed: true, aDisplaySettings: dxfDisplaySettings.Null(lname));
                    if (i == 1)
                        posarea = bounds.Area;
                    else
                        negarea += bounds.Area;
                }

                dxfVector v1 = new dxfVector(0, -Assy.RingRadius - 0.5 * paperscale);
                string txt = $"Functional Active Areas {Assy.TrayName()}";

                double wl = 0;
                foreach (var item in weirs)
                {
                    wl += item.Length(1) + item.Length(2);
                }

                txt = $"{txt}\\P\\P{{\\C{(int)dxxColors.Blue};FUNCTIONAL WEIR LENGTH = {uopUnits.GetUnit(uppUnitTypes.SmallLength).UnitValueString(wl, units)} [{uopUnits.GetUnit(uppUnitTypes.BigLength).UnitValueString(wl / 12, units)}]}} ";
                txt = $"{txt}\\P{{\\C{(int)dxxColors.Green};POSITIVE AREA = {aunits.UnitValueString(posarea, units)} [{bunits.UnitValueString(posarea / 144, units)}]}} ";
                txt = $"{txt}\\P{{\\C{(int)dxxColors.Red};NEGATIVE AREA = {aunits.UnitValueString(negarea, units)} [{bunits.UnitValueString(negarea / 144, units)}]}} ";
                double funcArea = posarea - negarea;
                txt = $"{txt}\\P{{\\C{(int)dxxColors.Blue}FUNCTIONAL ACTIVE AREA (A\\H0.5x;\\S^f;\\H2.0x;) = {aunits.UnitValueString(funcArea, units)} [{bunits.UnitValueString((funcArea) / 144, units)}]}} ";
                if (Assy.ProjectType == uppProjectTypes.MDDraw)
                {
                    double mechArea = Assy.MechanicalActiveArea;
                    double RFactor = mechArea != 0 ? funcArea / mechArea : 1;

                    txt = $"{txt}\\P{{\\C{(int)dxxColors.Blue}MECHANICAL ACTIVE AREA (A\\H0.5x;\\S^m;\\H2.0x;) = {aunits.UnitValueString(mechArea, units)} [{bunits.UnitValueString((mechArea) / 144, units)}]}} ";
                    txt = $"{txt}\\P{{\\C{(int)dxxColors.Blue}R-FACTOR (A\\H0.5x;\\S^f;\\H2.0x; / A\\H0.5x;\\S^m;\\H2.0x;) = {RFactor:0.0000} ";
                }

                draw.aText(v1, txt, 0.125 * paperscale, dxxMTextAlignments.TopCenter, aTextStyle: "Standard");
            }
            catch (Exception e) { aSource.HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
            finally { aSource.Status = string.Empty; aSource.DrawinginProgress = false; }

        }

        public void DDWG_DefinitionLines(IappDrawingSource aSource, mdProject aProject)
        {
            try
            {
                if (aSource == null) return;
                if (aSource.Image == null || aSource.MDAssy == null) return;
                mdProject MDProject = (mdProject)aSource.Project;
                mdTrayAssembly Assy = aSource.MDAssy;

                double thk = Assy.Downcomer().Thickness;


                bool includeLimits = aSource.Drawing.Options.AnswerB("Include Limit Lines?", true);
                bool includeWeirs = aSource.Drawing.Options.AnswerB("Include Weir Lines?", true);
                bool includeOuterBox = aSource.Drawing.Options.AnswerB("Include Outer Box Lines?", true);
                bool includeInnerBox = aSource.Drawing.Options.AnswerB("Include Inner Box Lines?", true);
                bool includeOverhangs = aSource.Drawing.Options.AnswerB("Include End Plate Overhangs?", true);
                bool includeVirtuals = aSource.Drawing.Options.AnswerB("Include Virtual Downcomers?", true);
                bool includeEndPlates = aSource.Drawing.Options.AnswerB("Include End Plates?", false);
                bool includeShelfs = aSource.Drawing.Options.AnswerB("Include Shelf Lines?", false);
                bool includeEndSupports = aSource.Drawing.Options.AnswerB("Include End Support Lines?", false);
                bool includePanels = aSource.Drawing.Options.AnswerB("Include Panel Shapes?", false);
                bool includeFBAs = aSource.Drawing.Options.AnswerB("Include FBA Shapes?", false);

                string rounto = aSource.Drawing.Options.AnswerS("Select Rounding Limits", MDProject.DowncomerRoundToLimit.ToString());
                dxoDrawingTool draw = aSource.Image.Draw;

                aSource.Image.Display.SetDisplayWindow(2.3 * aSource.Range.ShellID * 0.5, null, aDisplayHeight: 2.3 * aSource.Range.ShellID * 0.5, bSetFeatureScales: true);
                //Draw_Shell(aSource, uppViews.Plan, out _, out _);
                //Draw_Rings(uppViews.Plan);
                double paperscale = aSource.Image.Display.PaperScale;

                uppMDRoundToLimits RoundUnits = rounto.ToUpper() switch
                {
                    "SIXTEENTH" => uppMDRoundToLimits.Sixteenth,
                    "MILLIMETER" => uppMDRoundToLimits.Millimeter,
                    "NONE" => uppMDRoundToLimits.None,
                    _ => MDProject.DowncomerRoundToLimit
                };

                DowncomerDataSet dcdata = new DowncomerDataSet(Assy, Assy.DowncomerSpacing, null, Assy.Downcomer(), aRoundMethod: RoundUnits);
                List<uopLinePair> limits = dcdata.LimitLines;
                List<uopLinePair> weirs = dcdata.WeirLines;
                List<uopLinePair> outerboxes = dcdata.BoxLines;
                List<uopLinePair> innerboxes = dcdata.BoxLines;


                List<uopLinePair> limits_vir = limits.FindAll((x) => x.IsVirtual);
                limits.RemoveAll((x) => x.IsVirtual);

                List<uopLinePair> weirs_vir = weirs.FindAll((x) => x.IsVirtual);
                weirs.RemoveAll((x) => x.IsVirtual);

                //if (Assy.DesignFamily.IsBeamDesignFamily()) Draw_Beams(uppViews.Plan);
                //if (includeEndPlates) Draw_EndPlates(uppViews.Plan);

                draw.aCircle(dxfVector.Zero, Assy.RingClipRadius, new dxfDisplaySettings(aLinetype: dxfLinetypes.Continuous, aColor: dxxColors.LightCyan, aLTScale: 4 * aSource.Image.Header.LineTypeScale));
                dxxColors virolor = dxxColors.LightYellow;

                if (includeLimits)
                {
                    DrawLinePairs(draw, limits, aLayer: "LIMITS", aColor: dxxColors.Orange);


                    foreach (uopLinePair pair in limits)
                    {
                        if (uopUtils.RunningInIDE)
                        {
                            draw.aText(pair.EndPoints(false).GetVector(dxxPointFilters.GetLeftTop), $"COL:{pair.Col}\\PROW:{pair.Row}\\PT:{pair.IntersectionType1}\\PB:{pair.IntersectionType2}", 0.04 * paperscale, aAlignment: dxxMTextAlignments.MiddleCenter, aColor: dxxColors.BlackWhite);
                        }
                        uopVectors rcholes = pair.Line1.Points;
                        foreach (var hole in rcholes)
                        {
                            draw.aCircle(hole, dcdata.RingClipHoleDiameter / 2, aDisplaySettings: dxfDisplaySettings.Null(aLayer: "RING_CLIP_HOLES", dxxColors.Orange));
                        }
                        rcholes = pair.Line2.Points;
                        foreach (var hole in rcholes)
                        {
                            draw.aCircle(hole, dcdata.RingClipHoleDiameter / 2, aDisplaySettings: dxfDisplaySettings.Null(aLayer: "RING_CLIP_HOLES", dxxColors.Orange));
                        }
                    }
                    if (includeVirtuals)
                    {
                        DrawLinePairs(draw, limits_vir, aLayer: "VIRTUALS", aColor: virolor);

                        foreach (uopLinePair pair in limits_vir)
                        {
                            if (uopUtils.RunningInIDE)
                            {
                                draw.aText(pair.EndPoints(false).GetVector(dxxPointFilters.GetLeftTop), $"COL:{pair.Col}\\PROW:{pair.Row}\\PT:{pair.IntersectionType1}\\PB:{pair.IntersectionType2}", 0.04 * paperscale, aAlignment: dxxMTextAlignments.MiddleCenter, aColor: dxxColors.BlackWhite);
                            }
                            uopVectors rcholes = pair.Line1.Points;
                            foreach (var hole in rcholes)
                            {
                                draw.aCircle(hole, dcdata.RingClipHoleDiameter / 2, aDisplaySettings: dxfDisplaySettings.Null(aLayer: "VIRTUALS", aColor: virolor));
                            }
                            rcholes = pair.Line2.Points;
                            foreach (var hole in rcholes)
                            {
                                draw.aCircle(hole, dcdata.RingClipHoleDiameter / 2, aDisplaySettings: dxfDisplaySettings.Null(aLayer: "VIRTUALS", aColor: virolor));
                            }
                        }
                    }



                }

                if (includeWeirs)
                {
                    DrawLinePairs(draw, weirs, aLayer: "WEIRS", aColor: dxxColors.Blue);
                    if (uopUtils.RunningInIDE && !includeLimits)
                    {
                        foreach (var item in weirs)
                        {
                            draw.aText(item.EndPoints(false).GetVector(dxxPointFilters.GetLeftTop), $"COL:{item.Col}\\PROW:{item.Row}\\PT:{item.IntersectionType1}\\PB:{item.IntersectionType2}", 0.04 * paperscale, aAlignment: dxxMTextAlignments.MiddleCenter, aColor: dxxColors.BlackWhite);
                        }
                    }

                    if (includeVirtuals)
                    {
                        DrawLinePairs(draw, weirs_vir, aLayer: "VIRTUALS", aColor: virolor);
                        if (uopUtils.RunningInIDE && !includeLimits && !includeWeirs)
                        {
                            foreach (var item in weirs_vir)
                            {
                                draw.aText(item.EndPoints(false).GetVector(dxxPointFilters.GetLeftTop), $"COL:{item.Col}\\PROW:{item.Row}\\PT:{item.IntersectionType1}\\PB:{item.IntersectionType2}", 0.04 * paperscale, aAlignment: dxxMTextAlignments.MiddleCenter, aColor: dxxColors.BlackWhite);
                            }
                        }
                    }
                }

                if (includeInnerBox || includeOuterBox)
                {
                    if (uopUtils.RunningInIDE && !includeLimits && !includeWeirs)
                    {
                        foreach (var item in outerboxes)
                        {
                            draw.aText(item.EndPoints(false).GetVector(dxxPointFilters.GetLeftTop), $"COL:{item.Col}\\PROW:{item.Row}\\PT:{item.IntersectionType1}\\PB:{item.IntersectionType2}", 0.04 * paperscale, aAlignment: dxxMTextAlignments.MiddleCenter, aColor: dxxColors.BlackWhite);
                        }
                    }

                    if (!includeInnerBox || !includeOuterBox)
                    {
                        if (includeOuterBox)
                        {
                            DrawLinePairs(draw, outerboxes.FindAll((x) => !x.IsVirtual), aLayer: "OUTER BOX", aColor: dxxColors.Magenta);
                            if (includeVirtuals) DrawLinePairs(draw, innerboxes.FindAll((x) => x.IsVirtual), aLayer: "VIRTUALS", aColor: virolor);

                        }
                        if (includeInnerBox)
                        {
                            DrawLinePairs(draw, outerboxes.FindAll((x) => !x.IsVirtual), aLayer: "INNER BOX", aColor: dxxColors.Magenta);
                            if (includeVirtuals) DrawLinePairs(draw, innerboxes.FindAll((x) => x.IsVirtual), aLayer: "VIRTUALS", aColor: virolor);
                        }
                    }
                    else
                    {
                        foreach (var outer in outerboxes)
                        {
                            //draw the left wall as a polyine

                            DrawLinePair(draw, new uopLinePair(outer.GetSide(uppSides.Left, true), outer.GetSide(uppSides.Left).Moved(thk)), true, aLayer: !outer.IsVirtual ? "BOXES" : "VIRTUALS", aColor: !outer.IsVirtual ? dxxColors.Magenta : dxxColors.LightGreen);
                            //draw the right wall as a polyine

                            DrawLinePair(draw, new uopLinePair(outer.GetSide(uppSides.Right, true), outer.GetSide(uppSides.Right).Moved(-thk)), true, aLayer: !outer.IsVirtual ? "BOXES" : "VIRTUALS", aColor: !outer.IsVirtual ? dxxColors.Magenta : dxxColors.LightGreen);
                        }

                    }


                }

                if (includeOverhangs)
                {

                    uopLinePair overhang1;
                    uopLinePair overhang2;
                    foreach (var inner in innerboxes)
                    {

                        if (inner.IsVirtual && !includeVirtuals) continue;
                        overhang1 = inner.Clone();
                        overhang1.MoveOrtho(thk, -thk);
                        overhang2 = overhang1.Clone();
                        overhang1.Line1.EndPt.Y = overhang1.Line1.StartPt.Y + dcdata.EndPlateOverhang;
                        overhang1.Line2 = overhang1.Line1.Moved(thk);
                        DrawLinePair(draw, overhang1, true, aLayer: !inner.IsVirtual ? "OVERHANGS" : "VIRTUALS", aColor: !inner.IsVirtual ? dxxColors.Blue : virolor);

                        overhang1.Mirror(null, inner.Line1.MidPt.Y);
                        DrawLinePair(draw, overhang1, true, aLayer: !inner.IsVirtual ? "OVERHANGS" : "VIRTUALS", aColor: !inner.IsVirtual ? dxxColors.Blue : virolor);


                        overhang2.Line1 = inner.Line2.Moved(-thk);
                        overhang2.Line1.EndPt.Y = overhang2.Line1.StartPt.Y + dcdata.EndPlateOverhang;
                        overhang2.Line2 = overhang2.Line1.Moved(-thk);
                        DrawLinePair(draw, overhang2, true, aLayer: !inner.IsVirtual ? "OVERHANGS" : "VIRTUALS", aColor: !inner.IsVirtual ? dxxColors.Blue : virolor);

                        overhang2.Mirror(null, inner.Line2.MidPt.Y);
                        DrawLinePair(draw, overhang2, true, aLayer: !inner.IsVirtual ? "OVERHANGS" : "VIRTUALS", aColor: !inner.IsVirtual ? dxxColors.Blue : virolor);

                    }
                }

                if (includeShelfs)
                {
                    uopLine l1;
                    uopLine l2;
                    List<uopLinePair> shelflines = dcdata.ShelfLines;
                    foreach (var shelf in shelflines)
                    {
                        if (shelf.IsVirtual && !includeVirtuals) continue;
                        l1 = shelf.GetSide(uppSides.Left);
                        if (l1 != null)
                        {
                            l2 = l1.Moved(dcdata.ShelfWidth);
                            // l1.Move(thk);
                            DrawLinePair(draw, new uopLinePair(l1, l2), true, aLayer: !shelf.IsVirtual ? "SHELFS" : "VIRTUALS", aColor: !shelf.IsVirtual ? dxxColors.DarkGrey : virolor);
                        }
                        l1 = shelf.GetSide(uppSides.Right);
                        if (l1 != null)
                        {
                            l2 = l1.Moved(-dcdata.ShelfWidth);
                            //l1.Move(-thk);
                            DrawLinePair(draw, new uopLinePair(l1, l2), true, aLayer: !shelf.IsVirtual ? "SHELFS" : "VIRTUALS", aColor: !shelf.IsVirtual ? dxxColors.DarkGrey : virolor);
                        }
                    }

                }

                if (includeEndSupports)
                {
                    draw.aCircle(dxfVector.Zero, dcdata.DeckRadius, dxfDisplaySettings.Null(aLayer: "END SUPPORTS", aColor: dxxColors.Purple, aLinetype: "Dot2"));
                    List<uopLinePair> eslines = dcdata.GetEndSupportLines();
                    foreach (var es in eslines)
                    {
                        if (es.IsVirtual && !includeVirtuals) continue;
                        DrawLinePair(draw, es, false, aLayer: !es.IsVirtual ? "END SUPPORTS" : "VIRTUALS", aColor: !es.IsVirtual ? dxxColors.Purple : virolor);


                    }

                }
                if (includePanels)
                {

                    uopPanelSectionShapes shapes =   dcdata.CreatePanelShapes(dcdata.PanelClearance,dcdata.DeckLap, out _, out _, includeVirtuals,false);
                    foreach (var shape in shapes)
                    {
                        draw.aPolyline(shape.Vertices, bClosed: true, dxfDisplaySettings.Null(aLayer: "PANELS", aColor: !shape.IsVirtual ? dxxColors.z_41 : virolor));
                    }
                   
                }
                if (includeFBAs)
                {

                    uopPanelSectionShapes shapes = dcdata.FreeBubblingShapes(out List<uopLine> pnlLns);
                    foreach (var shape in shapes)
                    {
                        draw.aPolyline(shape.Vertices, bClosed: true, dxfDisplaySettings.Null(aLayer: "FBA", aColor: !shape.IsVirtual ? dxxColors.Blue : virolor));
                    }
                    if (uopUtils.RunningInIDE)
                    {
                        foreach (var pnlLn in pnlLns)
                        {
                            if (pnlLn.Length <= 0) continue;
                            draw.aLine(pnlLn, dxfDisplaySettings.Null(aLayer: "FBA", aColor: dxxColors.Blue, aLinetype: dxfLinetypes.Hidden));

                            draw.aCircles(pnlLn.Points, aRadius: 0.01 * paperscale, dxfDisplaySettings.Null(aLayer: "FBA", aColor: dxxColors.Blue));

                        }

                    }
                }

                if (Assy.DesignFamily.IsBeamDesignFamily())
                {
                    List<uopLinePair> edges = Assy.Beam.GetEdges(0.5);// Assy.RingClearance);
                    foreach (uopLinePair edge in edges)
                    {
                        DrawLinePair(draw, edge, aLayer: "TRIMMERS", aColor: dxxColors.Orange);
                    }
                }

                double shellrad = Assy.ShellID / 2 + 0.25 * paperscale;
                foreach (var dc in Assy.Downcomers)
                {
                    double y1 = Math.Sqrt(Math.Pow(shellrad, 2) - Math.Pow(dc.X, 2));
                    uopLine l1 = new uopLine(new uopVector(dc.X, y1), new uopVector(dc.X, -y1));
                    draw.aLine(l1, aLineType: dxfLinetypes.Center);
                }

                dxfRectangle bounds = aSource.Image.Entities.BoundingRectangle();
                uopUnit linearU = uopUnits.GetUnit(uppUnitTypes.SmallLength);
                uppUnitFamilies units = aSource.Drawing.DrawingUnits;
                dxfVector v1 = bounds.TopRight.Moved(0.15 * paperscale);
                dxoDimStyle dstyle = aSource.Image.DimStyle();
                string txt = $"TRAY ASSY : {Assy.RingRange.SpanName}";
                txt += $"\\PROUNDING METHOD : {dcdata.RoundingMethod}";
                txt += $"\\PDC SPACE : {dstyle.FormatNumber(dcdata.Spacing)} {linearU.Label(units)}";
                txt += $"\\PDC INSIDE WIDTH : {dstyle.FormatNumber(dcdata.InsideWidth)} {linearU.Label(units)}";
                txt += $"\\PDC THICKNESS : {dstyle.FormatNumber(dcdata.Thickness)} {linearU.Label(units)}";
                txt += $"\\PRING RADIUS : {dstyle.FormatNumber(dcdata.RingRadius)} {linearU.Label(units)}";
                txt += $"\\PRING CLEARANCE : {dstyle.FormatNumber(dcdata.Clearance)} {linearU.Label(units)}";
                txt += $"\\PBOUND RADIUS : {dstyle.FormatNumber(dcdata.RingClipRadius)} {linearU.Label(units)}";
                txt += $"\\PRING CLIP SIZE : {uopEnums.Description(dcdata.RingClipSize)}";
                txt += $"\\PRING CLIP CLEARANCE : {dstyle.FormatNumber(dcdata.DefaultRingClipClearance)} {linearU.Label(units)}";
                txt += $"\\PEND PLATE OVERHANGE : {dstyle.FormatNumber(dcdata.EndPlateOverhang)} {linearU.Label(units)}";

                if (!dcdata.IsStandardDesign)
                {
                    if (dcdata.DesignFamily.IsBeamDesignFamily())
                    {
                        if (dcdata.Divider.Offset != 0) txt += $"\\PBEAM OFFSET : {dstyle.FormatNumber(dcdata.Divider.Offset)} {linearU.Label(units)}";
                        txt += $"\\PBEAM WIDTH : {dstyle.FormatNumber(dcdata.Divider.Width)} {linearU.Label(units)}";
                        txt += $"\\PBEAM CLEARANCE : {dstyle.FormatNumber(dcdata.Divider.Clearance)} {linearU.Label(units)}";
                    }
                }
                draw.aText(v1, txt, aAlignment: dxxMTextAlignments.TopLeft);

            }
            catch (Exception e) { aSource.HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
            finally { aSource.Status = string.Empty; aSource.DrawinginProgress = false; }

            return;
        }

        public void DDWG_SpoutGroupInputSketch(IappDrawingSource aSource)
        {
            if (aSource == null || aSource.Drawing == null) return;
            if (aSource.Drawing.Part == null) return;

            mdTrayAssembly assy = aSource.MDAssy;

            if (assy == null) return;
        


            dxfImage img = aSource.Image;
      
            try
            {
                mdSpoutGroup group = (mdSpoutGroup)aSource.Drawing.Part;
                if (group == null) return;

                group.UpdateSpouts(assy);

                mdDowncomerBox box = group.DowncomerBox;
                if (box == null) return;
                mdSpoutGrid aGrid = null;
                if (aSource.Drawing.Grid == null)
                {
                    aGrid = group.Grid;
                }
                else
                {
                    aGrid = (mdSpoutGrid)aSource.Drawing.Grid;
                    //aGrid.Generate_ByConstraints(group, aGrid.Constraints);
                }
                if (aGrid == null) return;
                mdSpoutArea SA = aGrid.SpoutArea;
               
                uopHoles spouts = aGrid.Spouts;
                dxeLine aLn;
                bool bDrawLimitLine = SA.LimitedBounds;
                double dcWd = aGrid.DCInfo.InsideWidth;
                double cx = aGrid.DCInfo.X;
                uopRectangle panelLimits = SA.PanelLimits;
                double pnlMin = panelLimits.Bottom;
                double pnlMax = panelLimits.Top;
                double yBot = pnlMin - 0.5 * dcWd;
                double yTop = (aGrid.GroupIndex != 1) ? pnlMax + 0.5 * dcWd : 2.6E+26;
                double cy = aGrid.PanelY; // pnlMin + (pnlMax - pnlMin) / 2;
                double panelHt = assy.FunctionalPanelWidth;
                uopLine limitLine = box.LimitLine(bTop: SA.Direction == dxxOrthoDirections.Up); // ? uppSides.Top : uppSides.Bottom);
                uopShape maxbounds= aGrid.SpoutCenterBoundary(bMaxed: true, bIgnoreMargins:true);
                uopRectangle bndRec = maxbounds.Limits(aGrid.SpoutDiameter, aGrid.SpoutDiameter);
                uopRectangle zRec = panelLimits.Stretched(4 * aGrid.SpoutDiameter, 0);
                if (bDrawLimitLine)
                {
                    if(SA.Direction == dxxOrthoDirections.Up)
                    {
                        zRec.Top = Math.Max(Math.Max(limitLine.MaxY + 2, bndRec.Top), zRec.Top);
                        zRec.Bottom = Math.Min(Math.Min(limitLine.MinY - 2, bndRec.Bottom), zRec.Bottom);

                   }
                    else
                    {
                        zRec.Top = Math.Max(Math.Max(limitLine.MaxY + 2, bndRec.Top), zRec.Top);
                        zRec.Bottom = Math.Min(Math.Min(limitLine.MinY - 2, bndRec.Bottom), zRec.Bottom);
                    }

                }
                else
                {
                    zRec.Top = Math.Max(bndRec.Top, zRec.Top);
                    zRec.Bottom = Math.Min( bndRec.Bottom, zRec.Bottom);

                }
                //zRec.Move(SA.X, 0);
                //to match the aspect ratio of the disired output window
                //img.Display.Size = Viewer.Size;
                img.Header.UCSMode = dxxUCSIconModes.None;

                // set the display parameters
                img.Display.ZoomOnRectangle(zRec, 1, 0.90, bSetFeatureScales: true, bSuppressAutoRedraw: false, bForceRedraw: true, bSaveAsLimits: true);
                double ht = zRec.Width / 2;
                double paperscale = img.Display.PaperScale;
                img.LayerColorAndLineType_Set("0", dxxColors.BlackWhite, dxfLinetypes.Continuous, true);
                img.LinetypeLayers.Clear();

                dxoDrawingTool draw = img.Draw;
                
                //draw the max bounds if it isnt used by the grid
                dxePolyline maxboundPL = !aGrid.UsesMaxBounds ? draw.aShape(maxbounds, aDisplaySettings: new dxfDisplaySettings(aLayer: "TEST LAYER", aColor: dxxColors.Purple, aLinetype: dxfLinetypes.Hidden2)) : null;

                // draw the generator bound
                dxePolyline boundPL = draw.aShape(aGrid, new dxfDisplaySettings("0", dxxColors.Grey, dxfLinetypes.Hidden));
                draw.aCircle(aGrid.Origin, 0.05 * paperscale, new dxfDisplaySettings("TEST LAYER", dxxColors.LightGrey, dxfLinetypes.Continuous));

                if (aSource.DrawLayoutRectangles)
                {

                    draw.aShape(zRec, aDisplaySettings: new dxfDisplaySettings(aColor: dxxColors.Cyan));

                    // the spout area shape
                    //draw.aShape(aGrid.SpoutArea, aDisplaySettings: new dxfDisplaySettings(aColor: dxxColors.Magenta));


                    dxfDisplaySettings dsp2 = new dxfDisplaySettings("TEST LAYER", dxxColors.LightGrey, dxfLinetypes.Hidden2);
                 
                    uopLines lines = aGrid.RowLines();
                   
                    draw.aLines(lines, dsp2);
                    NumberVectors(draw, lines.HandlePoints(uppSegmentPoints.StartPt), paperscale, aDisplaySettings: dsp2);
                    lines = aGrid.ColumnLines();
                    draw.aLines(lines, dsp2);
                    NumberVectors(draw, lines.HandlePoints(uppSegmentPoints.StartPt), paperscale, aDisplaySettings: dsp2);

                    if (maxboundPL != null) NumberVectors(draw, maxbounds.Vertices, paperscale, aDisplaySettings: maxboundPL.DisplaySettings);
                    if(boundPL != null) NumberVectors(draw, aGrid.Vertices, paperscale, aDisplaySettings: boundPL.DisplaySettings);

                    uopVector u1 = new uopVector(aGrid.DCInfo.X_Outside_Left - 0.75 * paperscale, aGrid.SpoutArea.Y);
                    img.SymbolSettings.LineDisplaySettings = new dxfDisplaySettings("TEST LAYER", dxxColors.Blue);
       
                    draw.aSymbol.Arrow(u1, aGrid.Direction == dxxOrthoDirections.Up ? 90 : -90,  3 * paperscale);
                }

                    // draw the downcomer box
                    img.Entities.Append(box.Edges("0", dxxColors.BlackWhite, dxfLinetypes.Continuous, zRec.Top, zRec.Bottom ));

                // draw the spout
                if (spouts.Count <= 0)
                {
                    // draw the bounding perimeter of the spouts
                    draw.aShape(group.Perimeter, aDisplaySettings: new dxfDisplaySettings("0", dxxColors.Blue, dxfLinetypes.Hidden));
                }
                else
                {
                    bndRec = aGrid.SpoutLimits;
                    draw.aShape(bndRec, aDisplaySettings: new dxfDisplaySettings( "0", dxxColors.Blue, dxfLinetypes.Hidden));

                       spouts.DrawToImage(img, "0", aColor: Math.Abs(aGrid.ErrorPercentage) > aGrid.ErrorLimit ? dxxColors.Red : dxxColors.BlackWhite, aLinetype: dxfLinetypes.Continuous, bSuppressInstances: true);
                }

                // draw the endplate and limit line
                if (bDrawLimitLine)
                {
                    mdEndPlate ep = null;
                   draw.aLine(limitLine, aDisplaySettings: new dxfDisplaySettings("0", dxxColors.Purple, dxfLinetypes.Hidden) );

                    if (aGrid.LimitedTop)
                    {
                        ep = box.EndPlate(bTop: true);
                        draw.aPolyline(ep.EdgeVertices(), bClosed: true, aDisplaySettings: dxfDisplaySettings.Null("0", dxxColors.BlackWhite, dxfLinetypes.Continuous));

                    }

                    if (aGrid.LimitedBottom)
                    {
                        ep = box.EndPlate(bTop: false);
                        draw.aPolyline(ep.EdgeVertices(), bClosed: true, aDisplaySettings: dxfDisplaySettings.Null("0", dxxColors.BlackWhite, dxfLinetypes.Continuous));
                    }
                }



                // draw the deck panel below
                aLn = draw.aLine(cx - ht, cy, cx + ht, cy, "0", dxxColors.Red, dxfLinetypes.Center);

                aLn = draw.aLine(cx - ht, panelLimits.Bottom, cx + ht, panelLimits.Bottom, "0", dxxColors.Grey, dxfLinetypes.Continuous);

                aLn = draw.aLine(cx - ht, panelLimits.Top, cx + ht, panelLimits.Top, "0", dxxColors.Grey, dxfLinetypes.Continuous);

                // draw the dwoncomer center line
                aLn = draw.aLine(cx, zRec.Top, cx, zRec.Bottom, "0", dxxColors.Red, dxfLinetypes.Center);

                dxfDisplaySettings dsp = new dxfDisplaySettings("0", dxxColors.Orange, dxfLinetypes.Hidden);

                uopLine limL  = SA.LimitedBounds ? aGrid.LimitLine: null;
                    draw.aLine(limL, aDisplaySettings: dsp);
               
                // draw the clearance
                uopLine l1 = new uopLine(cx - dcWd / 2 + aGrid.AppliedClearance, maxbounds.Bottom,cx - dcWd / 2 + aGrid.AppliedClearance, maxbounds.Top);
                l1.ExtendTo(limL, bTrimTo: true);
                draw.aLine(l1,dsp);
                l1 = new uopLine(cx + dcWd / 2 - aGrid.AppliedClearance, maxbounds.Bottom,cx + dcWd / 2 - aGrid.AppliedClearance, maxbounds.Top);
                l1.ExtendTo(limL, bTrimTo: true);
                draw.aLine( l1,dsp);


                

                // draw margin lines
                //yBot = aGrid.AppliedMargin;
                //if (yBot != 0)
                //{
                //    aLn = draw.aLine(cx - dcWd / 2, pnlMin + yBot, cx + dcWd / 2, pnlMin + yBot, "0", dxxColors.Orange, dxfLinetypes.Hidden);

                //    if (aGrid.PanelIndex > 1)
                //    {
                //        aLn = draw.aLine(cx - dcWd / 2, pnlMax - yBot, cx + dcWd / 2, pnlMax - yBot, "0", dxxColors.Orange, dxfLinetypes.Hidden);
                //    }
                //}
                yBot = aGrid.Y;
                aLn = draw.aLine(cx - dcWd / 2 - 0.3 * paperscale, yBot, cx + dcWd / 2 + 0.3 * paperscale, yBot, "0", dxxColors.Yellow, dxfLinetypes.Center);

                if (spouts.Count > 0) draw.aCircle(aGrid.Origin, 0.045 * paperscale, new dxfDisplaySettings("0", dxxColors.Blue, dxfLinetypes.Continuous));
                // img.Display.ZoomOnRectangle(zRec, 0.05, 0.045, true, false, true);

                // img.Display.ZoomExtents();
                // Viewer.ZoomExtents();
                //img.Display.ZoomExtents();
                //img.Bitmap(false).CopyToClipBoard();

            }
            catch (Exception e)
            {
                aSource.HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }
            finally
            {
                aSource.Status = string.Empty;
                aSource.DrawinginProgress = false;
                aSource.Drawing.Options.Clear();
            }

        }

        public void DDWG_SpoutAreas(IappDrawingSource aSource)
        {
      
            mdTrayAssembly Assy = aSource?.MDAssy;
            if (aSource == null || Assy == null) return;
            try
            {

                dxfImage img = aSource.Image;
                Draw_Shell(aSource, uppViews.Plan, out dxeLine vcl, out dxeLine hcl);
                double paperscale =img.Display.ZoomExtents(bSetFeatureScale: true);
                dxoDrawingTool draw =img.Draw;

                //mdSpoutAreaMatrices matrices = Assy

                int dccnt = Assy.Downcomer().Count;
                int pcnt = dccnt + 1;
                double thk = Assy.Downcomer().Thickness;
                Status = "Regenerating Spout Area Matrices";

                bool optB = !Assy.DesignFamily.IsStandardDesignFamily();

                mdSpoutZones zones = new mdSpoutZones(Assy, Assy.Constraints); ;
                mdSpoutAreaMatrices matrices = zones.Matrices(bDistributeArea:true);
                bool numberVerts = aSource.Drawing.Options.AnswerB("Number Vertices?", false);

                int z = 0;
                foreach (mdSpoutZone zone in zones)
                {
                    z++;
                    List<uopLinePair> weirs = zone.WeirLines;

                    foreach (var pair in weirs)
                    {
                       // if (pair.IsVirtual) continue;
                        uopLine left = pair.GetSide(uppSides.Left, true);
                        uopLine right = pair.GetSide(uppSides.Right, true);

                        left.Move(-thk);
                        right.Move(thk);

                        draw.aLine(left, dxfDisplaySettings.Null(aLayer: "WEIRS", aColor: dxxColors.Blue, aLinetype:dxfLinetypes.ByLayer));
                        draw.aLine(right, dxfDisplaySettings.Null(aLayer: "WEIRS", aColor: dxxColors.Green, aLinetype: dxfLinetypes.ByLayer));

                    }


                    foreach (mdSpoutArea spoutarea in zone)
                    {
                        uopVectors verts = spoutarea.Vertices;

                        int dcindex = spoutarea.DowncomerIndex;
                        int boxindex = spoutarea.BoxIndex;
                        int panelindex = spoutarea.PanelIndex;
                        string bname = $"SPOUT AREA PANEL-{panelindex} DC-{dcindex} BOX-{boxindex}";
                        dxfBlock block = new dxfBlock(bname);
                        dxxColors clr = z == 1 ? dxxColors.DarkGrey : dxxColors.Orange;
                        //if (z == 1 && spoutarea.LimitedBounds) 
                        //    clr = dxxColors.Magenta;
                        block.Entities.AddShape(spoutarea, aDisplaySettings: dxfDisplaySettings.Null(aLayer: "SPOUT AREAS", aColor: clr, aLinetype: "ByLayer"));
                        //block.Entities.Add(new dxePolyline(verts.Clone(), bClosed: true, aDisplaySettings: dxfDisplaySettings.Null(aLayer: "SPOUT AREAS", aColor:  clr, aLinetype: "ByLayer")));
                        block.Entities.Add(aSource.Image.EntityTool.Create_Text(spoutarea.Center, $"\\fTxt.shx;{spoutarea.Handle}\\PBOX Y:{spoutarea.BoxY:0.000}\\PDIRECTION{spoutarea.Direction.Description()}", aTextHeight: 0.05 * paperscale, aAlignment: dxxMTextAlignments.MiddleCenter, aLayer: "SPOUT AREAS", aColor: spoutarea.LimitedBounds ? dxxColors.LightYellow : clr));
                        clr = z == 1 ? dxxColors.Magenta : dxxColors.LightBlue;
                        block.Entities.Append(spoutarea.PanelLimitLines.ToDXFLines(new dxfDisplaySettings("SPOUT AREAS", clr)));
                        block.Entities.Translate(spoutarea.Center * -1);
                        block =img.Blocks.Add(block);
                        block.Instances = new dxoInstances(spoutarea.Center); // : spoutarea.Instances.ToDXFInstances();

                        draw.aInserts(block, block.Instances, bOverrideExisting: false);
                         if(numberVerts) NumberVectors(draw, verts, paperscale, 0.05);
                        if (panelindex == pcnt && dcindex == 6)
                        {
                            draw.aCircles(verts, 0.03 * paperscale);

                        }
                    }

                }

                dxfBlock panels = mdBlocks.DeckPanels_View_Plan(img, Assy, aLineType: dxfLinetypes.Hidden, bBothSides: false, bForTrayBelow:true);
               
                double xscale =Assy.DesignFamily.IsStandardDesignFamily() ? 1 : -1;
                draw.aInsert(panels.Name, dxfVector.Zero, 0, aScaleFactor: xscale, aYScale: 1);
          
                dxfRectangle bounds =img.Entities.BoundingRectangle();
                uopUnit linearU = uopUnits.GetUnit(uppUnitTypes.SmallLength);
                uppUnitFamilies units = aSource.Drawing.DrawingUnits;
                dxfVector v1 = bounds.TopRight.Moved(0.15 * paperscale);
                dxoDimStyle dstyle =img.DimStyle();
                string txt = $"TRAY ASSY : {Assy.RingRange.SpanName}";
                
                //txt += $"\\PDC SPACE : {dstyle.FormatNumber(dcdata.Spacing)} {linearU.Label(units)}";
                //txt += $"\\PDC INSIDE WIDTH : {dstyle.FormatNumber(dcdata.InsideWidth)} {linearU.Label(units)}";
                //txt += $"\\PDC THICKNESS : {dstyle.FormatNumber(dcdata.Thickness)} {linearU.Label(units)}";
                //txt += $"\\PRING RADIUS : {dstyle.FormatNumber(dcdata.RingRadius)} {linearU.Label(units)}";
                //txt += $"\\PRING CLEARANCE : {dstyle.FormatNumber(dcdata.Clearance)} {linearU.Label(units)}";
                //txt += $"\\PBOUND RADIUS : {dstyle.FormatNumber(dcdata.RingClipRadius)} {linearU.Label(units)}";
                //txt += $"\\PRING CLIP SIZE : {uopEnums.Description(dcdata.RingClipSize)}";
                //txt += $"\\PRING CLIP CLEARANCE : {dstyle.FormatNumber(dcdata.DefaultRingClipClearance)} {linearU.Label(units)}";
                //txt += $"\\PEND PLATE OVERHANGE : {dstyle.FormatNumber(dcdata.EndPlateOverhang)} {linearU.Label(units)}";

                //if (!dcdata.IsStandardDesign)
                //{
                //    if (dcdata.DesignFamily.IsBeamDesignFamily())
                //    {
                //        if (dcdata.Divider.Offset != 0) txt += $"\\PBEAM OFFSET : {dstyle.FormatNumber(dcdata.Divider.Offset)} {linearU.Label(units)}";
                //        txt += $"\\PBEAM WIDTH : {dstyle.FormatNumber(dcdata.Divider.Width)} {linearU.Label(units)}";
                //        txt += $"\\PBEAM CLEARANCE : {dstyle.FormatNumber(dcdata.Divider.Clearance)} {linearU.Label(units)}";
                //    }
                //}
                v1 =  draw.aText(v1, txt, aAlignment: dxxMTextAlignments.TopLeft).BoundingRectangle().BottomLeft.Moved(0, -0.5 * paperscale);

               img.TableSettings.TextGap = 0.05;


                z = 0;
                foreach(mdSpoutAreaMatrix matrix in matrices)
                {
                    z++;
                    uopTable table = matrix.GetTable(uppUnitFamilies.English,true, true, 0, true);


                    v1 = draw.aTable($"ZONE_{z}", v1, table.ToStrings(false), aTableAlign: dxxRectangularAlignments.TopLeft,aGridStyle: dxxTableGridStyles.All,bScaleToScreen:false ).BoundingRectangle().BottomLeft.Moved(0, -0.5 * paperscale);
                }

            }
            catch (Exception e) { if (aSource != null) { aSource.HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); } }
            finally { if (aSource != null) { aSource.Status = string.Empty; aSource.DrawinginProgress = false; } }
        }

        /// <summary>
        ///#1returns the vertical centerline used for dimensions in other functions
        ///#2returns horizontal centerline used for dimensions in other functions
        ///#3the view to draw
        ///^used by higher level drawing functions to draw the column shell
        ///~the view argument controls how the shell is drawn.
        /// </summary>
        /// <param name="VerCenterLn"></param>
        /// <param name="HorCenterLn"></param>
        /// <param name="View"></param>
        private void Draw_Shell(IappDrawingSource aSource, uppViews View, out dxeLine HorCenterLn, out dxeLine VerCenterLn)
        {
            HorCenterLn = null;
            VerCenterLn = null;
            if (aSource ==null) return;
            if (aSource.Image == null || aSource.MDAssy == null) return;
            string statuswuz = Status;
            Status = "Drawing Shell";

            List<dxeLine> cLines;

            //put lines or a single circle into the dxf but draw rectangles or double circles to the screen
            try
            {
                aSource.Image.Layers.Add("HEAVY", dxxColors.Grey, dxfLinetypes.Continuous);


                if (View == uppViews.Plan || View == uppViews.AttachPlan || View == uppViews.LayoutPlan || View == uppViews.InstallationPlan)
                {
                    //draw and add the inner diameter circle to the screen and to the dxf
                    aSource.Image.Draw.aCircle(null, aSource.Range.ShellID * 0.5, dxfDisplaySettings.Null("HEAVY"));
                    cLines = aSource.Image.Draw.aCenterlines(dxfVector.Zero, aSource.Range.ShellID * 0.5 + 0.75, 1.05);
                    HorCenterLn = cLines.Find((x) => x.Flag == "HORIZONTAL");
                    VerCenterLn = cLines.Find((x) => x.Flag == "VERTICAL");
                }
            }
            catch (Exception e) { aSource.HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
            finally { aSource.Status = statuswuz; }
        }
    }
}

