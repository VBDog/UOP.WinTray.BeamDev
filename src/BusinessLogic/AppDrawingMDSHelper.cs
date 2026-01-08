
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Vml;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Office.Interop.Excel;
using Microsoft.VisualBasic.Devices;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI.WebControls;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using UOP.DXFGraphics;
using UOP.DXFGraphics.Utilities;
using UOP.WinTray.Projects;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Documents;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Parts;

using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;
using UOP.WinTray.UI.Interfaces;
using UOP.WinTray.UI.Model;
using UOP.WinTray.UI.Views.Windows.CADfx.Questions;
using static iText.Layout.Borders.Border;

namespace UOP.WinTray.UI.BusinessLogic
{
    /// <summary>
    /// interaction logic for drawings
    /// functions for generating MD Spouts Tray Drawings.
    /// </summary>
    public class AppDrawingMDSHelper : IappDrawingSource //: IAppDrawingMDSHelper
    {


        #region Events
        public delegate void StatusChangeHandler(string StatusString);
        public event IappDrawingSource.StatusChangeHandler StatusChange;

        public delegate void CanceledDelegate();
        public event CanceledDelegate Canceled;

        private string _Status;

        public string Status
        {
            get { return _Status; }
            set { _Status = value; StatusChange?.Invoke(_Status); }
        }

        private void ToolStatusChange(string aStatus)
        {


            if (SuppressWorkStatus) return;
            string stat = Status;
            if (string.IsNullOrWhiteSpace(stat))
            {
                if (!string.IsNullOrWhiteSpace(aStatus))
                    stat = "Drawing Helper : ";

                else
                    stat = "";
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(aStatus))
                    stat += " : ";

            }

            if (!string.IsNullOrWhiteSpace(aStatus)) stat += aStatus;
            StatusChange?.Invoke(stat);
        }
        #endregion Events

        #region Variables


        private dxfImage _Image = null;
        // the last point that the mouse was clicked within the display

        private uopDocDrawing _Drawing;


        #endregion


        #region Constructor
        public AppDrawingMDSHelper()
        {
            _Drawing = new uopDocDrawing( uppDrawingFamily.Undefined);

            _Tool = new AppDrawingSourceHelper();
            _Tool.StatusChange += ToolStatusChange;
        }
        #endregion

        #region Properties

        //dxfImage aImage = null;
        //Panel editSpout = null;

        private AppDrawingSourceHelper _Tool;
        public AppDrawingSourceHelper Tool
        {
            get { _Tool ??= new AppDrawingSourceHelper(); _Tool.SuppressWorkStatus = SuppressWorkStatus; return _Tool; }
            set { _Tool = value; }
        }

        public bool SuppressBorder { get; set; }






        public bool SuppressWorkStatus { get; set; }


        /// <summary>
        ///@flag which indicates if a drawing is being generated 
        /// </summary>
        public bool DrawinginProgress { get; set; }
        public uppProjectTypes ProjectType { get => (Project != null) ? Project.ProjectType : uppProjectTypes.Undefined; }
        public uopDocDrawing Drawing { get => _Drawing; set { _Drawing = value; Range = _Drawing.Range; MDProject = (mdProject)_Drawing.Project; } }
        public bool DrawlayoutRectangles { get; private set; }
        public dxfImage Image { get => _Image; set => _Image = value; }

        private mdTrayRange _MDRange;
        public mdTrayRange MDRange
        {
            get => _MDRange;
            set
            {
                if (value == null)
                {
                    _MDRange = null;
                    _Assy = null;
                }
                else
                {
                    _MDRange = value;
                    _Assy = _MDRange.TrayAssembly;

                }
            }
        }

        public uopTrayRange Range
        {
            get => MDRange;
            set
            {
                if (value == null)
                {
                    MDRange = null;
                }
                else
                {
                    if (value.ProjectFamily == uppProjectFamilies.uopFamMD)
                    {
                        MDRange = (mdTrayRange)value;
                    }
                }
            }
        }

        private mdTrayAssembly _Assy;
        public mdTrayAssembly Assy { get => _Assy; }

        private mdProject _MDProject;
        public mdProject MDProject { get => _MDProject; set => _MDProject = value; }

        public TitleBlockHelper TitleHelper => Tool.TitleBlockHelper;

        public dxfBlockSource BlockSource { get; set; }
        public uopProject Project
        {
            get => _MDProject;
            set
            {
                if (value == null)
                {
                    _MDProject = null;
                    return;
                }

                if (value.ProjectFamily == uppProjectFamilies.uopFamMD)
                {
                    _MDProject = (mdProject)value;
                }
            }
        }

        #endregion

        #region Functions


        private bool _CancelOps = false;
        public bool CancelOps
        {
            get => _CancelOps;
            set
            {
                _CancelOps = value;
                if (value)
                {
                    Canceled?.Invoke();
                    _Tool.CancelBorder();

                }
            }

        }
        bool IappDrawingSource.DrawLayoutRectangles => DrawlayoutRectangles;

        mdTrayAssembly IappDrawingSource.MDAssy { get => Assy; }

        /// <summary>
        /// this function is the main entry point for all of the drawing functions found in this class
        /// The caller passes an uopDocDrawing object which carries all of the information required to create a drawing.
        /// This function routes the request to the appropriate "Draw" function based on then uopDocDrawing.DrawingType.
        /// </summary>
        /// <param name="RHS">the drawing request object</param>
        /// <param name="aImage">the drawing tool to use</param>
        /// <param name="DisableApp"></param>
        /// <param name="bSuppressErrors"></param>
        /// <returns></returns>
        public dxfImage GenerateImage(uopDocDrawing argDrawing, bool bSuppressErrors = false, System.Drawing.Size aImageSize = new System.Drawing.Size(), uopTrayRange preSelectedTrayRange = null, bool bUsingViewer = true, System.Drawing.Color? BackColor = null, bool bSuppressIDEEffects = false)
        {
            if (argDrawing == null) return null;


            dxfImage _rVal = null;

            try
            {


                Status = $"Generating {argDrawing.Name}";
                DrawlayoutRectangles = uopUtils.RunningInIDE;
                //string ErrStr;
                //string ErrSource;
                //double sngTimeStart;
                bool ZoomExtents;
                //string sUnlock;
                //bool bCanelOps = false;
                //if (argDrawing.DrawingType == uppDrawingTypes.TraySketch) BackColor = System.Drawing.Color.White;
                if (!BackColor.HasValue) BackColor = appApplication.SketchColor;
                if (aImageSize.IsEmpty)
                    aImageSize = argDrawing.DeviceSize;

                if (_Image == null)
                {

                    _Image = new dxfImage(BackColor.Value, aImageSize);
                }
                else
                {
                    _Image.Display.BackgroundColor = BackColor.Value;
                    _Image.Display.Size = aImageSize;
                }

                _Image.UsingDxfViewer = bUsingViewer;

                // make the range and request available to all drawing functions.
                Drawing = argDrawing;

                //sngTimeStart = Timer;
                Drawing.DrawTime = string.Empty;

                //mdSpoutGroup aSG;
                //mdDowncomer aDC;
                ZoomExtents = Drawing.ZoomExtents;

                if (Drawing.DeviceSize.Width > 0 && Drawing.DeviceSize.Height > 0)
                    Image.Display.Size = Drawing.DeviceSize;

                // pass the requested draw settings to the draw object
                SetDrawProperties();



                // check for drawing type
                switch (Drawing.DrawingType)
                {

                    case uppDrawingTypes.DowncomerManholeFit:
                        DDWG_DowncomerFit();
                        break;
                    case uppDrawingTypes.SGInputSketch:

                       DDWG_SpoutGroupInputSketch();
                        break;
                    case uppDrawingTypes.SectionSketch:
                        DDWG_SectionSketch();
                        break;

                    case uppDrawingTypes.InputSketch:
                        if (Drawing.Range != null) DDWG_InputSketch();

                        ZoomExtents = false;
                        break;
                    case uppDrawingTypes.FreeBubbleAreas:
                    case uppDrawingTypes.BlockedAreas:
                        DDWG_FreeBubblingAreas();
                        break;
                    case uppDrawingTypes.StartUpLines:
                        DDWG_StartUpLines();
                        break;
                    case uppDrawingTypes.TestDrawing:
                        if (string.Compare(appApplication.User.NetworkName, "E342367", true) == 0)
                            DDWG_Test_MTZ();
                        else
                            DDWG_Test();
                        break;
                    case uppDrawingTypes.DefinitionLines:
                        DDWG_DefinitionLines();
                        break;
                    case uppDrawingTypes.FeedZones:
                        DDWG_FeedZones();
                        break;

                    case uppDrawingTypes.TraySketch:
                        ZoomExtents = false;
                        if (Drawing.Range != null) DDWG_TraySketch();
                        break;
                    case uppDrawingTypes.SpoutGroupSketch:
                        ZoomExtents = false;
                        Tool.DDWG_SpoutGroupInputSketch(this);
                        break;
                    case uppDrawingTypes.FunctionalActiveAreas:
                        DDWG_FunctionalActiveAreas();
                        break;
                    case uppDrawingTypes.SpoutAreas:
                        DDWG_SpoutAreas();
                    break;

                    
                
                    default:
                        break;
                }



                // zoomExtens
                if (ZoomExtents)
                {
                    Image.Display.ZoomExtents(1.05f);
                    Drawing.ExtentWidth = Image.Display.ViewWidth;
                    Drawing.ViewCenter = Image.Display.ViewCenter;
                }
            }
            catch (Exception)
            {
                //Retaining commented code for implementing timer and progressbar for future release
                //ErrStr = "(" + Err().Source + ")" + Err().Description;
                //DrawinginProgress = false;
                //GenerateImage = TerminateObjects;
                //argDrawing.DrawTime = FormatSeconds(ref Timer - sngTimeStart);

                //if (bDisableApp)
                //{
                //    goControl.EnableApplication(frmMain.instance, sUnlock);
                //}

                //Err.Raise(1000, "appDrawingsXF", ErrStr);
                //return GenerateImage;

            }
            finally
            {
                Image.UCS.Reset();
                //Image.Display.Refresh();

                if (Image.ErrorCount > 0 & !bSuppressErrors)
                {
                    Image.DisplayErrors(Drawing.DrawingName + " Drawing Errors");
                }

                Image.TextStyle().TextHeight = 0;


                if (Drawing.DrawingType != uppDrawingTypes.InputSketch)
                {
                    //remove unused layers
                    Image.Purge(false, true, true, true);
                }
                Status = "";

                DrawinginProgress = false;

                //Retaining commented code for implementing timer and progressbar for future release
                //GenerateImage = TerminateObjects;

                //if (bDisableApp)
                //{
                //    goControl.EnableApplication(frmMain.instance, sUnlock);
                //}
                //argDrawing.DrawTime = FormatSeconds(ref Timer - sngTimeStart);
                _rVal = Image;
                TerminateObjects();
            }


            return _rVal;
        }

        public void TerminateObjects()
        {

            MDRange = null;


            _MDProject = null;
            _Drawing = null;

            if (_Tool != null) _Tool.StatusChange -= ToolStatusChange;
            _Tool = null;

            _Image = null;
        }




        /// <summary>
        /// executed prior to all top level drawing functions to set the drawing tool properties
        /// based on the application settings and the settings passed in the current drawing .
        /// application colors (RGB longs) are converted to AutoCAD ACL colors here.
        /// </summary>
        private void SetDrawProperties()
        {
            Image.CollectErrors = true;


            dxoSettingsDim dsets = Image.DimSettings;
            dxoSettingsText tsets = Image.TextSettings;

            if (Drawing != null)
            {
                // Need to check
                dsets.DrawingUnits = dxxDrawingUnits.English;

                Image.Header.TextSize = 0.125f;

            }

            // Setting up the Image object
            Image.Header.UCSMode = dxxUCSIconModes.None;
            //Image.Styles.AddTextStyle("RomanD", "romand.shx", 0.1875f, 0.8f);
            //Image.Styles.AddTextStyle("RomanS", "RomanS.shx", 0f, 0.8f, "", false, false, false, false, true);
            Image.Header.TextSize = 0.125f;

            dsets.DimLayer = "DIM";
            dsets.DimLayerColor = dxxColors.Blue;
            dsets.LeaderLayer = "LEADERS";
            dsets.LeaderLayerColor = dsets.DimLayerColor;

            // image test settings
            tsets.LayerName = "TEXT";
            tsets.LayerColor = dxxColors.Cyan;

            // image table settings
            dxoSettingsTable tbsets = Image.TableSettings;
            tbsets.LayerName = "TABLES";
            tbsets.LayerColor = dsets.DimLayerColor;
            tbsets.TextColor = tsets.LayerColor;
            tbsets.BorderColor = tsets.LayerColor;
            tbsets.GridColor = tbsets.BorderColor;
            tbsets.TextSize = 0.125f;
            tbsets.TextGap = 1;

            // Image Symbol Settings
            dxoSettingsSymbol sysets = Image.SymbolSettings;
            sysets.LayerName = "SYMBOLS";
            sysets.TextColor = tsets.LayerColor;
            sysets.LayerColor = dsets.DimLayerColor;
            sysets.TextHeight = 0.125f;

            // Image LineType Layers
            dxsLinetypes lsets = Image.LinetypeLayers;

            lsets.Add("Center", dxfLinetypes.Center, dxxColors.Red);
            lsets.Add(dxfLinetypes.Hidden, dxfLinetypes.Hidden, dxxColors.Green);
            lsets.Add(dxfLinetypes.Phantom, dxfLinetypes.Phantom, dxxColors.LightGrey, dxxLineWeights.LW_015);

            dxoDimStyle dstyle = Image.DimStyle();

            //Setting up Image Dim Style, Need to check
            if (dsets.DrawingUnits == dxxDrawingUnits.English)
            {

                dstyle.SetZeroSuppressionDecimal(false,
                    true, Convert.ToInt16(appApplication.DrawSettings.LinearPrecision(uppUnitFamilies.English)),
                   Convert.ToInt16(appApplication.DrawSettings.AngularPrecision));
            }
            else
            {
                dstyle.SetZeroSuppressionDecimal(appApplication.DrawSettings.SuppressLeadingZeros,
                    appApplication.DrawSettings.SuppressTrailingZeros, Convert.ToInt16(appApplication.DrawSettings.MetricPrecision),
                    Convert.ToInt16(appApplication.DrawSettings.AngularPrecision));
            }

            dstyle.SetColors(dxxColors.ByLayer, tsets.LayerColor);
            dstyle.SetGapsAndOffsets(0.09f, 0.09);
            //dstyle.TextStyleName = "RomanS";
            dstyle.TextHeight = 0.125f;

            if (Drawing.DrawingUnits == uppUnitFamilies.Metric)
            {
                dstyle.LinearScaleFactor = 25.4;
                dstyle.LinearPrecision = 2;
            }
            else
            {
                dstyle.LinearScaleFactor = 1;
                dstyle.LinearPrecision = 3;
            }

            dxoStyle tstyle = Image.TextStyle();
            tstyle.FontName = "Arial Narrow.ttf";
        }

        /// <summary>
        /// used by higher level drawing functions to draw the spout groups in the tray assembly
        /// he view argument controls how the groups are drawn.
        /// </summary>
        /// <param name="aLayerName"></param>
        /// <param name="aErrLim"></param>


        public void Draw_SpoutGroups(string aLayerName = "", double aErrLim = 0)
        {

            if (string.IsNullOrWhiteSpace(aLayerName)) aLayerName = "SPOUTS";
            List<mdSpoutGroup> aSGs = Assy.SpoutGroups.GetByVirtual(aVirtualValue: false).FindAll((x) => x.SpoutCount(Assy) > 0);
            dxfDisplaySettings dsp = new( aLayerName, dxxColors.BlackWhite, dxfLinetypes.Continuous);
           

            // set SpoutGroup

            Status = "Drawing Spout Groups";
            try
            {
                dxfVector org = Image.UCS.Origin;
                bool moveum = !org.IsZero();
                dxoDrawingTool draw = Image.Draw;
                colMDSpoutGroups.GetSpoutEntities(aSGs, aErrLim, out List<dxeArc> validCircles, out List<dxeArc> invalidCircles, out List<dxePolyline> validPlines, out List<dxePolyline> invalidPlines, dsp);
                if (validCircles.Count > 0)
                {
                    dxoInstances insts = new dxoInstances();
                    dxeArc arc1 = validCircles[0];
                    for (int i = 1; i <= validCircles.Count; i++)
                    {
                        dxeArc arc2 = validCircles[i - 1];
                        insts.Add(aXOffset: arc2.X - arc1.X, aYOffset: arc2.Y - arc1.Y);

                    }
                    arc1.Instances = insts;
                    if (moveum) arc1.Center += org;
                    Image.Entities.Add(arc1);
                }

                if (validPlines.Count > 0)
                {
                    foreach (dxePolyline pl in validPlines)
                    {

                        if (moveum) pl.Translate(org);
                        Image.Entities.Add(pl);

                    }

                }
                if (invalidCircles.Count > 0)
                {
                    dxoInstances insts = new dxoInstances();
                    dxeArc arc1 = invalidCircles[0];
                    for (int i = 1; i <= invalidCircles.Count; i++)
                    {
                        dxeArc arc2 = invalidCircles[i - 1];
                        insts.Add(aXOffset: arc2.X - arc1.X, aYOffset: arc2.Y - arc1.Y);

                    }
                    arc1.Instances = insts;
                    if (moveum) arc1.Center += org;
                    Image.Entities.Add(arc1);
                }

                if (invalidPlines.Count > 0)
                {
                    foreach (dxePolyline pl in invalidPlines)
                    {

                        if (moveum) pl.Translate(org);
                        Image.Entities.Add(pl);

                    }

                }

                //Draw_SpoutGroups(dsp.LayerName, aErrLim);
            }
            catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
            finally { Status = string.Empty; }
        }

        private void DDWG_InputSketch()
        {
            try
            {
                MDRange = (mdTrayRange)Drawing.Range;
                if (MDRange == null) return;
                MDProject ??= MDRange.MDProject;
                mdProject proj = MDProject;

                if (proj == null) return;
                Status = "Refreshing Tray Sketch";

                colDXFEntities sEnts;
                colMDDeckPanels Panels = Assy.DeckPanels;
                dxfRectangle rect;

                double rad = Assy.DeckRadius;
                dxfVector v1 = dxfVector.Zero;
                dxoDrawingTool draw = Image.Draw;
                double errLim = (proj == null) ? Assy.ErrorLimit : proj.ErrorLimit;
                dxfDisplaySettings dsp;
                bool symmetric = Assy.IsSymmetric;
                uopRectangle urect = null;
                uppMDDesigns family = Assy.DesignFamily;
                if (symmetric)
                {
                    urect = Assy.FeatureViewRectangle(bIncludePanels: true, bIncludDowncomers: false, bIncludFullDowncomers: false, aWidthBuffer: Assy.Downcomer(1).Boxes.First().Width, aHeightBuffer: Assy.Downcomer(1).Boxes.First().Width);
                    urect.Bottom = !Assy.OddDowncomers ? Assy.Downcomers.GetByVirtual(true)[0].X : -Assy.Downcomer(1).BoxWidth / 2 - 0.5 * Assy.FunctionalPanelWidth;
                    urect.Left = urect.Bottom;
                }
                else
                {
                    double d1 = MDRange.ShellID * 0.5 * 1.05;
                    urect = new uopRectangle(-d1, d1, d1, -d1);
                }

                rect = urect.ToDXFRectangle();

                //if (uopUtils.RunningInIDE)
                //    draw.aRectangle(rect, aColor: dxxColors.Red);

                Image.Display.SetLimits(rect);

                Status = "Drawing Ring and Shell Arcs";
             
                dxeArc arc1 =  draw.aCircle(null, Assy.RingRadius, aDisplaySettings: new dxfDisplaySettings("RING", dxxColors.Green, dxfLinetypes.Hidden));
                arc1 = draw.aCircle(null,  Assy.ShellRadius, aDisplaySettings: new dxfDisplaySettings("SHELL", dxxColors.DarkGrey, dxfLinetypes.Continuous));
              
                
                
                //dxeLine cline = (dxeLine)Image.Entities.Add(new dxeLine(new dxfVector(0, rect.Top), new dxfVector(0, rect.Bottom), new dxfDisplaySettings(aLayer: "CENTERLINES", aColor: dxxColors.Red, aLineType: dxfLinetypes.Center)));
                //Image.Entities.Add(new dxeLine(new dxfVector(rect.Left, 0), new dxfVector(rect.Right, 0), new dxfDisplaySettings(aLayer: "0", aColor: dxxColors.Red, aLineType: dxfLinetypes.Center)));


                Drawing.ZoomExtents = false;
                double paperscale = Image.Display.ZoomExtents(bSetFeatureScale: true);
                draw.aCenterlines(arc1, 0.25 * paperscale, aDisplaySettings: new dxfDisplaySettings(aLayer: "CENTERLINES", aColor: dxxColors.Red, aLinetype: dxfLinetypes.Center));

                if (family.IsBeamDesignFamily()) Draw_Beams(uppViews.Plan, true);


                // draw the downcomers
                Status = "Drawing Downcomers";
                dsp = new dxfDisplaySettings("DOWNCOMERS", dxxColors.BlackWhite, dxfLinetypes.Continuous);
                List<mdDowncomer> dcomers = Assy.Downcomers.GetByVirtual(aVirtualValue: false);
                foreach (var dcomer in dcomers)
                {

                    dsp.Color = (Math.Abs(dcomer.ErrorPercentage(Assy)) > errLim) ? dxxColors.Red : dxxColors.BlackWhite;
                    List<mdDowncomerBox> boxes = dcomer.Boxes;
                    foreach (var box in boxes)
                    {
                        if (box.IsVirtual)
                            continue;
                        sEnts = box.Edges(dsp.LayerName, dsp.Color, dsp.Linetype, bQuickLine: false); // This needs to be modified to support multiple boxes
                        Image.Entities.Append(sEnts, false, aTag: $"DOWNCOMER{dcomer.Index}");
                        draw.aPolyline(box.EndPlate(bTop: true).EdgeVertices(), true, aDisplaySettings: dsp);
                        draw.aPolyline(box.EndPlate(bTop: false).EdgeVertices(), true, aDisplaySettings: dsp);
                    }
                }


                Status = "Drawing Deck Panels";
                //uopVectors vrts;

                //dxfBlock block = mdBlocks.DeckPanels_View_Plan(Image, Assy, bBothSides: false, bForTrayBelow: true);

                //draw.aInsert(block.Name, null, 0, aScaleFactor: Assy.DesignFamily.IsStandardDesignFamily() ? 1 : -1, aDisplaySettings: dxfDisplaySettings.Null(block.LayerName), aYScale: 1);

                var allpanelshapes = Assy.DowncomerData.PanelShapes(bIncludeClearance: false);

                List<uopPanelSectionShape> goodsections = new List<uopPanelSectionShape>();
                List<uopPanelSectionShape> badsections = new List<uopPanelSectionShape>();

                for (int i = 1; i <= Panels.Count; i++)
                {
                    mdDeckPanel Panel = Panels.Item(i);
                   List<uopPanelSectionShape> panelshapes = allpanelshapes.FindAll((x) => x.PanelIndex == Panel.Index);
                    foreach (var item in panelshapes)
                    {
                        if (item.IsVirtual) continue;
                    if (Math.Abs(Panel.ErrorPercentage(Assy)) > errLim)
                        badsections.Add(item); 
                    else 
                        goodsections.Add(item);

                    }


                    //}

                   

                    // break;
                }

                bool bothsides = Assy.DowncomerData.MultiPanel;

                double xscale = Assy.DesignFamily.IsStandardDesignFamily() ? 1 : -1;
                if (goodsections.Count > 0)
                {
                    dxfBlock panelblock = mdBlocks.DeckPanels_View_Plan(Image, Assy, "DECK PANELS", dxxColors.ByLayer, bBothSides: bothsides, bForTrayBelow: true, aBlockNameSuffix: string.Empty, aPanels: goodsections);
                    draw.aInsert(panelblock.Name, null, 0, aScaleFactor: xscale, aYScale: 1);

                }
                if (badsections.Count > 0)
                {
                    dxfBlock panelblock = mdBlocks.DeckPanels_View_Plan(Image, Assy, "DECK PANELS", dxxColors.Red, bBothSides: bothsides,  bForTrayBelow:true, aBlockNameSuffix: "ERROR_", aPanels: badsections);
                    draw.aInsert(panelblock.Name, null,  0, aScaleFactor: xscale, aYScale: 1);

                }

                // draw SpoutGroups
                Draw_SpoutGroups("SPOUTS", errLim);


                // draw Startups
                if (Assy.DesignOptions.HasBubblePromoters)
                {
                    Status = "Drawing Bubble Promoters";


                    uopVectors aBPs = Assy.BPCenters(bRegen: uopUtils.RunningInIDE);
                    dsp = new dxfDisplaySettings("BUBBLE_PROMOTERS", dxxColors.BlackWhite, dxfLinetypes.Continuous);
                    //get the unsuppressed sites and draw them black/white
                    uopVectors unsuppressed = aBPs.GetBySuppressed(false,out uopVectors suppressed);


                    if (unsuppressed.Count > 0) draw.aCircles(unsuppressed, mdGlobals.BPRadius, dsp);
                    //draw the suppressed sites in red
                    dsp.Color = dxxColors.Red;
                
                    if (suppressed.Count > 0) draw.aCircles(suppressed, mdGlobals.BPRadius, dsp);

                }


                Draw_Startups();


            }
            catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
            finally { Status = string.Empty; DrawinginProgress = false; }
        }

        private void DDWG_SpoutGroupInputSketch()
        {

            try
            {
                if (Drawing == null) return;
                if (Drawing.Part == null) return;

                mdSpoutGroup group = (mdSpoutGroup)Drawing.Part;
                //if (DrawlayoutRectangles)
                //{
                //    group.UpdateSpouts(Assy);
                //}

                Tool.DDWG_SpoutGroupInputSketch(this);

                ////initialize
                //if (Assy == null) return;
                //mdSpoutGroup sGroup = Assy.SpoutGroups.Item(aSpoutGroupIndex, bSuppressIndexError: true);
                //if (sGroup == null) return;

                //colDXFEntities Spouts = null;
                //dxePolyline EPlate = null;
                //dxePolyline aPl = null;
                //colDXFEntities bxEnts = null;
                //colDXFVectors verts = null;
                //dxePolyline Bound = null;
                //dxfVector v1 = dxfVector.Zero;
                //dxfVector v2 = dxfVector.Zero;
                //dxfVector dimPt = null;
                //colDXFEntities cLines = null;
                //uopHole RCHole = null;
                //dxeLine ln1 = null;
                //double trimY = 0;
                //dxeLine aL = null;
                //colDXFVectors DimPts = null;
                //int i = 0;
                //dxfRectangle aRect = null;
                //double xscal = 0;
                //dxfEntity aEnt = null;
                //mdDowncomer aDC = null;
                //uopHoles RCHoles = null;
                //dxfRectangle aRec = null;
                //DimPts = new colDXFVectors();

                //Bound = sGroup.SpoutGrid(Assy).SpoutLimits().Perimeter();
                //aDC = sGroup.Downcomer(Assy);
                //verts = new colDXFVectors(aDC.Boxes[0].EndPlate(bTop: true).EdgeVertices());

                //trimY = Bound.GetOrdinate(dxxOrdinateTypes.MinY, false) - 1;
                //if (trimY > verts.GetOrdinate(dxxOrdinateTypes.MinY))
                //{
                //    trimY = verts.GetOrdinate(dxxOrdinateTypes.MinY) - 1;
                //}
                //bxEnts = aDC.Boxes.First().EndSection(trimY, true); // This needs to be modified to support multiple boxes
                //RCHole = aDC.RingClipHole();

                //Spouts = sGroup.BlockEntities(bSuppressInstances: true);

                //dimPt = Spouts.Centers().GetVector(dxxPointFilters.GetLeftTop);

                //dxoSettingsDim dsets = Image.DimSettings;
                //dsets.DimLayer = "DIM";
                //dsets.DimLayerColor = dxxColors.Blue;
                //dsets.LeaderLayer = "LEADER";
                //dsets.LeaderLayerColor = dxxColors.Blue;


                //Image.TextSettings.LayerName = "TEXT";
                //Image.TextSettings.LayerColor = dxxColors.BlackWhite;

                //dxoSettingsTable tsets = Image.TableSettings;
                //tsets.LayerName = "TABLES";
                //tsets.LayerColor = dxxColors.BlackWhite;
                //tsets.TextColor = dxxColors.BlackWhite;
                //tsets.BorderColor = dxxColors.Blue;
                //tsets.GridColor = dxxColors.Blue;
                //Image.DimStyle().TextHeight = 0.085;
                //Image.DimStyle().SetGapsAndOffsets(aGap: 0.02, aArrowSize: 0.05);
                //Image.DimStyle().SetColors(dxxColors.Blue, dxxColors.BlackWhite);

                //for (i = 1; i <= bxEnts.Count; i++)
                //{
                //    aEnt = (dxfEntity)bxEnts.Item(i);
                //    if (aEnt.GraphicType == dxxGraphicTypes.Polyline)
                //    {
                //        aPl = (dxePolyline)aEnt;
                //        Image.Draw.aPolyline(aPl.Vertices, aPl.Closed, aDisplaySettings: new dxfDisplaySettings(aLayer: "0", aColor: aPl.Color, aLinetype: aPl.Linetype));
                //    }
                //    else if (aEnt.GraphicType == dxxGraphicTypes.Line)
                //    {
                //        aL = (dxeLine)aEnt;
                //        Image.Draw.aLine(aL.StartPt, aL.EndPt, "0", aLineType: aL.Linetype);
                //    }
                //}

                //EPlate = Image.Draw.aPolyline(verts, true);

                //Image.Display.ZoomExtents(1.5, true);
                //xscal = Image.Display.PaperScale;

                ////draw the spouts
                //Image.Entities.Append(sGroup.BlockEntities().Copy("0", dxxColors.BlackWhite, dxfLinetypes.Continuous), false);

                //aEnt = Spouts.GetByCenter(dimPt, bGetJustOne: true)[0];
                //cLines = new colDXFEntities();
                //aRec = aEnt?.BoundingRectangle(dxfPlane.World);

                //if (aRec != null)
                //{
                //    var allLines = new colDXFEntities();
                //    dxfEntity _IdxfEntity = Image.Draw.aLinePolarMPT(dimPt, 0, 1.1 * aRec.Width, ref allLines, aLayer: "0", aColor: Image.LinetypeLayers.LineColor(dxfLinetypes.Center), aLineType: dxfLinetypes.Center);
                //    cLines.Add(_IdxfEntity);
                //    _IdxfEntity = Image.Draw.aLinePolarMPT(dimPt, 90, 1.1 * aRec.Height, ref allLines, aLayer: "0", aColor: Image.LinetypeLayers.LineColor(dxfLinetypes.Center), aLineType: dxfLinetypes.Center);
                //    cLines.Add(_IdxfEntity);
                //}

                //RCHoles = aDC.Boxes[0].EndSupport(bTop: true).GenHoles("RING CLIP").Item(1);
                //if (RCHoles.Count > 0)
                //{
                //    dxfDisplaySettings dsp = new("0", dxxColors.BlackWhite, dxfLinetypes.Continuous);
                //    for (i = 1; i <= RCHoles.Count; i++)
                //    {
                //        RCHole = RCHoles.Item(i);
                //        v1 = new dxfVector(RCHole.X, RCHole.Y);
                //        Image.Draw.aCircle(v1, RCHole.Radius, dsp);
                //        Image.Draw.aCenterlines(v1, RCHole.Radius, 1.1);
                //    }
                //}


                //if (cLines != null)
                //{
                //    v1 = verts.GetVector(dxxPointFilters.GetLeftTop).Clone();
                //    v2 = cLines.DefinitionPoints(dxxEntDefPointTypes.EndPt).GetVector(dxxPointFilters.AtMaxY).Clone();
                //}

                //ln1 = (dxeLine)EPlate.Segments.Item(4);
                //dxfImage aImage = null;


                //for (i = 1; i <= RCHoles.Count; i++)
                //{
                //    RCHole = RCHoles.Item(i);
                //    v1 = RCHole.CenterDXF;
                //    v2 = v1.ProjectedToLine(ln1);
                //    Image.DimStyleOverrides.SuppressExtLine1 = true;
                //    Image.DimStyleOverrides.TextFit = dxxDimTextFitTypes.MoveArrowsFirst;
                //    aImage = null;

                //    Image.Draw.aDim.Aligned(v2, v1, -0.2, 0.1, aImage: aImage);
                //}


                //v1 = verts.GetVector(dxxPointFilters.GetBottomRight).Clone();
                //aPl = (dxePolyline)bxEnts.Item(2);
                //v2 = aPl.Vertices.GetVector(dxxPointFilters.GetRightTop).Clone();

                //Image.DimStyleOverrides.TextFit = dxxDimTextFitTypes.MoveTextAndArrows;

                //aPl = (dxePolyline)bxEnts.Item(1);
                //DimPts.Add(aPl.Vertices.GetVector(dxxPointFilters.GetLeftTop), bAddClone: true);
                //DimPts.Add(cLines.DefinitionPoints(dxxEntDefPointTypes.StartPt).GetVector(dxxPointFilters.AtMinX), bAddClone: true);
                //DimPts.Add(verts.GetVector(dxxPointFilters.GetLeftBottom), bAddClone: true);

                //Image.DimTool.Stack_Vertical(DimPts, -0.3 * xscal, aBotToTopLeftToRight: false, aImage: aImage);

                //aRect = Image.Display.ExtentRectangle;
                //v1 = aRect.TopRight;
                //v1.Move(0.25 * xscal);
                //Draw_SpoutGroupInfo(ref v1, ref sGroup);

            }
            catch (Exception e)
            {
                HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e);
            }
            finally
            {
                DrawinginProgress = false;
                Status = "";
            }

        }
        /// <summary>
        /// generates the Tray Sketch drawing shown on the Cross Flow input screen.
        /// </summary>
        private void DDWG_TraySketch()
        {
            if (MDRange == null || Image == null) return;
            try
            {
                Image.Display.SetDisplayWindow(2.3 * MDRange.ShellRadius, null, aDisplayHeight: 2.3 * MDRange.ShellRadius, bSetFeatureScales: true);
                Image.Header.LineTypeScale = 0.25 / Image.Display.ZoomFactor;
                Draw_Shell(uppViews.Plan);
                Draw_Ring(uppViews.Plan);
                if (Assy.DesignFamily.IsBeamDesignFamily()) Draw_Beams(uppViews.Plan, true);

                Draw_Downcomers(uppViews.LayoutPlan, bIncludeShelfAngles: true, bIncludeEndPlates: true, bIncludeEndSupports: true, bShowVirtualBoxes: false, bIncludeSpouts:false);
                Draw_DeckPanels("DECK_PANELS", dxxColors.BlackWhite, dxfLinetypes.Continuous, bDrawVirtuals: false);
                Draw_SpoutGroups("SPOUTS");
                Draw_DowncomersBelow(uppViews.Plan, Assy.DesignFamily.IsStandardDesignFamily());
                Draw_BubblePromoters(uppViews.Plan);
                Draw_Startups();

                if(appApplication.User.NetworkName =="E342367" && System.IO.Directory.Exists(@"C:\Junk\Test.bmp"))
                {

                    dxfBitmap dmap = Image.Bitmap(false);
                    Bitmap bmap = (Bitmap)dmap.Image;
                    bmap.Save(@"C:\Junk\Test.bmp");
                }
            }
            catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
            finally { Status = string.Empty; DrawinginProgress = false; }

        }

        private void DDWG_Test()
        {
            try
            {
                if (Image == null) return;


                double thk = Assy.Downcomer().Thickness;

                dxoDrawingTool draw = Image.Draw;

                Image.Display.SetDisplayWindow(2.3 * MDRange.ShellRadius, null, aDisplayHeight: 2.3 * MDRange.ShellRadius, bSetFeatureScales: true);
                double paperscale = Image.Display.PaperScale;
                Draw_Shell(uppViews.Plan);
                Draw_Ring(uppViews.Plan);

                //draw.aCircle(dxfVector.Zero, Assy.DeckRadius);
                if (Assy.DesignFamily.IsBeamDesignFamily())
                    Draw_Beams(uppViews.Plan);
                dxePolygon pg = null;
                foreach (var dc in Assy.Downcomers)
                {
                    List<mdDowncomerBox> boxes = dc.Boxes;
                    //if (!dc.IsVirtual) continue;


                    foreach (var box in boxes)
                    {
                        draw.aLine(box.BoxLine(bLeft: true), dxfDisplaySettings.Null(aColor: dxxColors.Yellow));
                        //draw.aLine(box.LimitLine(bTop:true), dxfDisplaySettings.Null(aColor: dxxColors.DarkGreen));
                        draw.aInsert(box.View_Plan(Assy, false, true, false, true, 0, dxfVector.Zero), box.Center);
                        pg = box.ShelfAngle(bLeft: true).View_Plan(bShowObscured: false);
                        Image.Entities.Add(pg);
                        // Tool.NumberVectors(draw, pg.Vertices, paperscale, 0.01, aEndIndex:5);
                        pg = box.ShelfAngle(bLeft: false).View_Plan(bShowObscured: false);
                        Image.Entities.Add(pg);

                        mdEndPlate ep = box.EndPlate(bTop: true);
                        pg = ep.View_Plan(bApplyFillets: true);
                        draw.aLine(ep.LimitLine, dxfDisplaySettings.Null(aColor: dxxColors.Green));
                        Image.Entities.Add(pg);
                        ep = box.EndPlate(bTop: false);
                        pg = ep.View_Plan(bApplyFillets: true);
                        Image.Entities.Add(pg);

                        mdEndSupport es = box.EndSupport(bTop: true);

                        // if (es.IntersectionType == uppIntersectionTypes.ToRing) draw.aInsert(pg, es.Center);
                        if (es.IntersectionType == uppIntersectionTypes.ToRing)
                        {
                            pg = es.View_Plan(bIncludeFilletFoints: true, aCenter: dxfVector.Zero);
                            draw.aInsert(pg, es.Center);

                        }

                        es = box.EndSupport(bTop: false);
                        if (es.IntersectionType == uppIntersectionTypes.ToRing)
                        {
                            pg = es.View_Plan(bIncludeFilletFoints: true, aCenter: dxfVector.Zero);
                            draw.aInsert(pg, es.Center);

                        }
                    }
                }

                List<mdEndSupport> ess = Assy.Downcomers.EndSupports(aSide: uppSides.Top, aIntersectionType: uppIntersectionTypes.ToDivider);
                foreach (var es in ess)
                {
                    pg = es.View_Plan(bIncludeFilletFoints: true, aCenter: dxfVector.Zero);
                    draw.aInsert(pg, es.Center);
                    draw.aLine(es.WeirLine(bLeft: true), dxfDisplaySettings.Null(aColor: dxxColors.Purple));
                    draw.aLine(es.WeirLine(bLeft: false), dxfDisplaySettings.Null(aColor: dxxColors.LightPurple));
                    pg.Translate(es.Center);
                    Tool.NumberVectors(draw, pg.Vertices, paperscale, 0.01, aEndIndex: 0);
                }
                DowncomerDataSet dcdata = new DowncomerDataSet(Assy, Assy.DowncomerSpacing, null, Assy.Downcomer());
                var panels = dcdata.PanelShapes();
                foreach (var item in panels)
                {
                    draw.aPolyline(item.Vertices, bClosed: true, aDisplaySettings: dxfDisplaySettings.Null(aColor: item.IsVirtual ? dxxColors.Yellow : dxxColors.BlackWhite));
                    //draw.aText(item.Center, $"COL: {item.Col}\\PROW: {item.Row}", aTextHeight: paperscale * 0.04, aAlignment: dxxMTextAlignments.MiddleCenter, aColor: dxxColors.BlackWhite);
                }
            }
            catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
            finally { Status = string.Empty; DrawinginProgress = false; }

            return;
        }

        private void DDWG_Test_MTZ()
        {
            try
            {
                if (Image == null) return;
             

                double thk = Assy.Downcomer().Thickness;

                dxoDrawingTool draw = Image.Draw;

                Image.Display.SetDisplayWindow(2.3 * Assy.ShellRadius, null, aDisplayHeight: 2.3 * Assy.ShellRadius, bSetFeatureScales: true);
                double paperscale = Image.Display.PaperScale;
                Draw_Shell(uppViews.Plan);
                Draw_Ring(uppViews.Plan);

                //draw.aCircle(dxfVector.Zero, Assy.DeckRadius);
                if (Assy.DesignFamily.IsBeamDesignFamily())
                    Draw_Beams(uppViews.Plan);
                colMDSpoutGroups SGs = Assy.SpoutGroups;

                Image.Layers.Add("SPOUTS", aColor: dxxColors.LightGrey);
                double rad = Assy.ShellRadius + 0.5 * paperscale;
                dxfBlock block = mdBlocks.DeckPanels_View_Plan(Image, Assy,bBothSides:false,bForTrayBelow:true);
               
                draw.aInsert(block.Name  ,null, 0, aScaleFactor: Assy.DesignFamily.IsStandardDesignFamily() ? 1 : -1, aDisplaySettings: dxfDisplaySettings.Null(block.LayerName), aYScale: 1);
                //draw.aInsert(block.Name, dxfVector.Zero, 0,  aDisplaySettings: dxfDisplaySettings.Null(block.LayerName));
                uopFreeBubblingPanels panels = Assy.FreeBubblingPanels;
                foreach (uopFreeBubblingPanel fbp in panels)
                {
                    if (fbp.Count <= 0)
                        continue;
                    block = new dxfBlock($"FBP_{fbp.PanelIndex}");
                    for (int i = 1; i <= fbp.Count; i++)
                    {
                        uopFreeBubblingArea fba = fbp[i - 1];
                        dxePolyline fbpl = block.Entities.AddShape(fba, dxfDisplaySettings.Null("FBPS"));
                        uopLinePair weirs = fba.WeirLines;
                        if (weirs.Line1 != null) block.Entities.AddLine(weirs.Line1, dxfDisplaySettings.Null("FBPS", dxxColors.Green));
                        if (weirs.Line2 != null) block.Entities.AddLine(weirs.Line2, dxfDisplaySettings.Null("FBPS", dxxColors.Blue));
                        //if(fba.OccuranceFactor == 2)
                        //{
                        //    fbpl = new dxePolyline(fbpl);
                        //    fbpl.Color = dxxColors.LightYellow;
                        //    fbpl.RotateAbout(dxfVector.Zero, 180);
                        //    block.Entities.Add(fbpl);
                        //}
                            

                    }
                    uopVector u1 = new uopVector(fbp.X, 0);
                    block.Entities.Translate(u1 * -1);
                    double yscale = Assy.DesignFamily.IsStandardDesignFamily() ? 1 : -1;
                    //block = Image.Blocks.Add(block);
                    //uopVector u2 = new uopVector(0, u1.X);
                   // u2 = u1;
                  //  draw.aInsert(block.Name,u2, 90, aDisplaySettings: dxfDisplaySettings.Null(block.LayerName),);


                }


                foreach (mdSpoutGroup sg in SGs)
                {
                    block = mdBlocks.SpoutGroup_View_Plan(Image, sg, Assy, bSetInstances: false, bSuppressCenterPoints: true, aLayerName: "SPOUTS");

                    draw.aInserts(block, block.Instances, false, aDisplaySettings: dxfDisplaySettings.Null("SPOUTS", aColor: dxxColors.Orange));

                }


                //                foreach (var dc in Assy.Downcomers)
                //                {
                //                    List<mdDowncomerBox> boxes = dc.Boxes;
                //                    //if (!dc.IsVirtual) continue;
                //                    dxfVector sp = new dxfVector(dc.X, +Math.Sqrt(Math.Pow(rad, 2) - Math.Pow(dc.X, 2)));
                //                    dxfVector ept = new dxfVector(dc.X, -Math.Sqrt(Math.Pow(rad, 2) - Math.Pow(dc.X, 2)));
                //                    dxeLine cl =  draw.aLine(sp, ept).Clone();
                //                    cl.RotateAbout(dxfVector.Zero, 90);
                //                    draw.aLine(cl, cl.DisplaySettings);

                //;                    foreach (var box in boxes)
                //                    {
                //                        draw.aLine(box.BoxLine(bLeft: true), dxfDisplaySettings.Null(aColor: dxxColors.Yellow));
                //                        //draw.aLine(box.LimitLine(bTop:true), dxfDisplaySettings.Null(aColor: dxxColors.DarkGreen));
                //                        draw.aInsert(box.View_Plan(Assy, false, true, false, true, 0, dxfVector.Zero), box.Center);
                //                        pg = box.ShelfAngle(bLeft: true).View_Plan(bShowObscured: false);
                //                        Image.Entities.Add(pg);
                //                        // Tool.NumberVectors(draw, pg.Vertices, paperscale, 0.01, aEndIndex:5);
                //                        pg = box.ShelfAngle(bLeft: false).View_Plan(bShowObscured: false);
                //                        Image.Entities.Add(pg);

                //                        mdEndPlate ep = box.EndPlate(bTop: true);
                //                        pg = ep.View_Plan(bApplyFillets: true);
                //                        draw.aLine(ep.LimitLine, dxfDisplaySettings.Null(aColor: dxxColors.Green));
                //                        Image.Entities.Add(pg);
                //                        ep = box.EndPlate(bTop: false);
                //                        pg = ep.View_Plan(bApplyFillets: true);
                //                        Image.Entities.Add(pg);

                //                        mdEndSupport es = box.EndSupport(bTop: true);

                //                        // if (es.IntersectionType == uppIntersectionTypes.ToRing) draw.aInsert(pg, es.Center);
                //                        //if (es.DowncomerIndex ==2)
                //                        //{
                //                        //pg = es.View_Plan(bIncludeFilletFoints: true, aCenter: dxfVector.Zero);
                //                        //draw.aInsert(pg, es.Center);
                //                        draw.aLine(es.WeirLine(bLeft: true), aDisplaySettings: dxfDisplaySettings.Null(aColor: dxxColors.Purple));
                //                        //    draw.aLine(es.WeirLine(bLeft: false), aDisplaySettings: dxfDisplaySettings.Null(aColor: dxxColors.LightPurple));

                //                        //draw.aLine(es.LapLine, aDisplaySettings: dxfDisplaySettings.Null(aColor: dxxColors.Blue));
                //                        dxePolyline pl = draw.aPolyline(es.Vertices(), bClosed: true);
                //                        Tool.NumberVectors(draw, pl.Vertices, paperscale, 0.01, aEndIndex: 4);
                //                        //}

                //                        es = box.EndSupport(bTop: false);
                //                        //if (es.DowncomerIndex ==2)
                //                        //{
                //                        //pg = es.View_Plan(bIncludeFilletFoints: true, aCenter: dxfVector.Zero);
                //                        //draw.aInsert(pg, es.Center);
                //                        //draw.aLine(es.WeirLine(bLeft: true), aDisplaySettings: dxfDisplaySettings.Null(aColor: dxxColors.Purple));
                //                        //draw.aLine(es.WeirLine(bLeft: false), aDisplaySettings: dxfDisplaySettings.Null(aColor: dxxColors.LightPurple));

                //                        //draw.aLine(es.LapLine, aDisplaySettings: dxfDisplaySettings.Null(aColor: dxxColors.Blue));
                //                        pl = draw.aPolyline(es.Vertices(), bClosed: true);
                //                        Tool.NumberVectors(draw, pl.Vertices, paperscale, 0.01, aEndIndex: 4);
                //                        //}

                //                    }
                //                }

                //List<mdEndSupport> ess = Assy.Downcomers.EndSupports(aSide: uppSides.Top, aIntersectionType: uppIntersectionTypes.ToDivider);
                //foreach (var es in ess)
                //{
                //    pg = es.View_Plan(bIncludeFilletFoints: true, aCenter: dxfVector.Zero);
                //    draw.aInsert(pg, es.Center);
                //    draw.aLine(es.WeirLine(bLeft: true), dxfDisplaySettings.Null(aColor: dxxColors.Purple));
                //    draw.aLine(es.WeirLine(bLeft: false), dxfDisplaySettings.Null(aColor: dxxColors.LightPurple));
                //    pg.Translate(es.Center);
                //    Tool.NumberVectors(draw, pg.Vertices, paperscale, 0.003, aEndIndex: 0);
                //}
                //DowncomerDataSet dcdata = new DowncomerDataSet(Assy, Assy.DowncomerSpacing, null, Assy.Downcomer());
                //List<uopShape> panels = dcdata.PanelShapes();
                //foreach (var item in panels)
                //{
                //    draw.aPolyline(item.Vertices, bClosed: true, aDisplaySettings: dxfDisplaySettings.Null(aColor: item.IsVirtual ? dxxColors.Yellow : dxxColors.BlackWhite));
                //    //draw.aText(item.Center, $"COL: {item.Col}\\PROW: {item.Row}", aTextHeight: paperscale * 0.04, aAlignment: dxxMTextAlignments.MiddleCenter, aColor: dxxColors.BlackWhite);
                //}
            }
            catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
            finally { Status = string.Empty; DrawinginProgress = false; }

            return;
        }
        private void DDWG_SpoutAreas() => Tool.DDWG_SpoutAreas(this);

        private void DDWG_DefinitionLines()
        {
            try
            {
                if (Image == null) return;

                //double thk = Assy.Downcomer().Thickness;


                //bool includeLimits = Drawing.Options.AnswerB("Include Limit Lines?", true);
                //bool includeWeirs = Drawing.Options.AnswerB("Include Weir Lines?", true);
                //bool includeOuterBox = Drawing.Options.AnswerB("Include Outer Box Lines?", true);
                //bool includeInnerBox = Drawing.Options.AnswerB("Include Inner Box Lines?", true);
                //bool includeOverhangs = Drawing.Options.AnswerB("Include End Plate Overhangs?", true);
                //bool includeVirtuals = Drawing.Options.AnswerB("Include Virtual Downcomers?", true);
                //bool includeEndPlates = Drawing.Options.AnswerB("Include End Plates?", false);
                //bool includeShelfs = Drawing.Options.AnswerB("Include Shelf Lines?", false);
                //bool includeEndSupports = Drawing.Options.AnswerB("Include End Support Lines?", false);
                //bool includePanels = Drawing.Options.AnswerB("Include Panel Shapes?", false);
                //bool includeFBAs = Drawing.Options.AnswerB("Include FBA Shapes?", false);

                //string rounto = Drawing.Options.AnswerS("Select Rounding Limits", MDProject.DowncomerRoundToLimit.ToString());
                //dxoDrawingTool draw = Image.Draw;

                //Image.Display.SetDisplayWindow(2.3 * MDRange.ShellID * 0.5, null, aDisplayHeight: 2.3 * MDRange.ShellID * 0.5, bSetFeatureScales: true);
                Draw_Shell(uppViews.Plan);
                Draw_Ring(uppViews.Plan);
                Tool.DDWG_DefinitionLines(this, MDProject);


                //double paperscale = Image.Display.PaperScale;

                //uppMDRoundToLimits RoundUnits = rounto.ToUpper() switch
                //{
                //    "SIXTEENTH" => uppMDRoundToLimits.Sixteenth,
                //    "MILLIMETER" => uppMDRoundToLimits.Millimeter,
                //    "NONE" => uppMDRoundToLimits.None,
                //    _ => MDProject.DowncomerRoundToLimit
                //};

                //DowncomerDataSet dcdata = new DowncomerDataSet(Assy, Assy.DowncomerSpacing, null, Assy.Downcomer(), aRoundMethod: RoundUnits);
                //List<uopLinePair> limits = dcdata.LimitLines;
                //List<uopLinePair> weirs = dcdata.WeirLines;
                //List<uopLinePair> outerboxes = dcdata.BoxLines;
                //List<uopLinePair> innerboxes = dcdata.BoxLines;


                //List<uopLinePair> limits_vir = limits.FindAll((x) => x.IsVirtual);
                //limits.RemoveAll((x) => x.IsVirtual);

                //List<uopLinePair> weirs_vir = weirs.FindAll((x) => x.IsVirtual);
                //weirs.RemoveAll((x) => x.IsVirtual);

                if (Assy.DesignFamily.IsBeamDesignFamily()) Draw_Beams(uppViews.Plan);
                if (Drawing.Options.AnswerB("Include End Plates?", false)) Draw_EndPlates(uppViews.Plan);

                //draw.aCircle(dxfVector.Zero, Assy.RingClipRadius, new dxfDisplaySettings(aLinetype: dxfLinetypes.Continuous, aColor: dxxColors.LightCyan, aLTScale: 4 * Image.Header.LineTypeScale));
                //dxxColors virolor = dxxColors.LightYellow;

                //if (includeLimits)
                //{
                //    Tool.DrawLinePairs(draw, limits, aLayer: "LIMITS", aColor: dxxColors.Orange);


                //    foreach (uopLinePair pair in limits)
                //    {
                //        if (uopUtils.RunningInIDE)
                //        {
                //            draw.aText(pair.EndPoints(false).GetVector(dxxPointFilters.GetLeftTop), $"COL:{pair.Col}\\PROW:{pair.Row}\\PT:{pair.IntersectionType1}\\PB:{pair.IntersectionType2}", 0.04 * paperscale, aAlignment: dxxMTextAlignments.MiddleCenter, aColor: dxxColors.BlackWhite);
                //        }
                //        uopVectors rcholes = pair.Line1.Points;
                //        foreach (var hole in rcholes)
                //        {
                //            draw.aCircle(hole, dcdata.RingClipHoleDiameter / 2, aDisplaySettings: dxfDisplaySettings.Null(aLayer: "RING_CLIP_HOLES", dxxColors.Orange));
                //        }
                //        rcholes = pair.Line2.Points;
                //        foreach (var hole in rcholes)
                //        {
                //            draw.aCircle(hole, dcdata.RingClipHoleDiameter / 2, aDisplaySettings: dxfDisplaySettings.Null(aLayer: "RING_CLIP_HOLES", dxxColors.Orange));
                //        }
                //    }
                //    if (includeVirtuals)
                //    {
                //        Tool.DrawLinePairs(draw, limits_vir, aLayer: "VIRTUALS", aColor: virolor);

                //        foreach (uopLinePair pair in limits_vir)
                //        {
                //            if (uopUtils.RunningInIDE)
                //            {
                //                draw.aText(pair.EndPoints(false).GetVector(dxxPointFilters.GetLeftTop), $"COL:{pair.Col}\\PROW:{pair.Row}\\PT:{pair.IntersectionType1}\\PB:{pair.IntersectionType2}", 0.04 * paperscale, aAlignment: dxxMTextAlignments.MiddleCenter, aColor: dxxColors.BlackWhite);
                //            }
                //            uopVectors rcholes = pair.Line1.Points;
                //            foreach (var hole in rcholes)
                //            {
                //                draw.aCircle(hole, dcdata.RingClipHoleDiameter / 2, aDisplaySettings: dxfDisplaySettings.Null(aLayer: "VIRTUALS", aColor: virolor));
                //            }
                //            rcholes = pair.Line2.Points;
                //            foreach (var hole in rcholes)
                //            {
                //                draw.aCircle(hole, dcdata.RingClipHoleDiameter / 2, aDisplaySettings: dxfDisplaySettings.Null(aLayer: "VIRTUALS", aColor: virolor));
                //            }
                //        }
                //    }



                //}

                //if (includeWeirs)
                //{
                //    Tool.DrawLinePairs(draw, weirs, aLayer: "WEIRS", aColor: dxxColors.Blue);
                //    if (uopUtils.RunningInIDE && !includeLimits)
                //    {
                //        foreach (var item in weirs)
                //        {
                //            draw.aText(item.EndPoints(false).GetVector(dxxPointFilters.GetLeftTop), $"COL:{item.Col}\\PROW:{item.Row}\\PT:{item.IntersectionType1}\\PB:{item.IntersectionType2}", 0.04 * paperscale, aAlignment: dxxMTextAlignments.MiddleCenter, aColor: dxxColors.BlackWhite);
                //        }
                //    }

                //    if (includeVirtuals)
                //    {
                //        Tool.DrawLinePairs(draw, weirs_vir, aLayer: "VIRTUALS", aColor: virolor);
                //        if (uopUtils.RunningInIDE && !includeLimits && !includeWeirs)
                //        {
                //            foreach (var item in weirs_vir)
                //            {
                //                draw.aText(item.EndPoints(false).GetVector(dxxPointFilters.GetLeftTop), $"COL:{item.Col}\\PROW:{item.Row}\\PT:{item.IntersectionType1}\\PB:{item.IntersectionType2}", 0.04 * paperscale, aAlignment: dxxMTextAlignments.MiddleCenter, aColor: dxxColors.BlackWhite);
                //            }
                //        }
                //    }
                //}

                //if (includeInnerBox || includeOuterBox)
                //{
                //    if (uopUtils.RunningInIDE && !includeLimits && !includeWeirs)
                //    {
                //        foreach (var item in outerboxes)
                //        {
                //            draw.aText(item.EndPoints(false).GetVector(dxxPointFilters.GetLeftTop), $"COL:{item.Col}\\PROW:{item.Row}\\PT:{item.IntersectionType1}\\PB:{item.IntersectionType2}", 0.04 * paperscale, aAlignment: dxxMTextAlignments.MiddleCenter, aColor: dxxColors.BlackWhite);
                //        }
                //    }

                //    if (!includeInnerBox || !includeOuterBox)
                //    {
                //        if (includeOuterBox)
                //        {
                //            Tool.DrawLinePairs(draw, outerboxes.FindAll((x) => !x.IsVirtual), aLayer: "OUTER BOX", aColor: dxxColors.Magenta);
                //            if (includeVirtuals) Tool.DrawLinePairs(draw, innerboxes.FindAll((x) => x.IsVirtual), aLayer: "VIRTUALS", aColor: virolor);

                //        }
                //        if (includeInnerBox)
                //        {
                //            Tool.DrawLinePairs(draw, outerboxes.FindAll((x) => !x.IsVirtual), aLayer: "INNER BOX", aColor: dxxColors.Magenta);
                //            if (includeVirtuals) Tool.DrawLinePairs(draw, innerboxes.FindAll((x) => x.IsVirtual), aLayer: "VIRTUALS", aColor: virolor);
                //        }
                //    }
                //    else
                //    {
                //        foreach (var outer in outerboxes)
                //        {
                //            //draw the left wall as a polyine

                //            Tool.DrawLinePair(draw, new uopLinePair(outer.GetSide(uppSides.Left, true), outer.GetSide(uppSides.Left).Moved(thk)), true, aLayer: !outer.IsVirtual ? "BOXES" : "VIRTUALS", aColor: !outer.IsVirtual ? dxxColors.Magenta : dxxColors.LightGreen);
                //            //draw the right wall as a polyine

                //            Tool.DrawLinePair(draw, new uopLinePair(outer.GetSide(uppSides.Right, true), outer.GetSide(uppSides.Right).Moved(-thk)), true, aLayer: !outer.IsVirtual ? "BOXES" : "VIRTUALS", aColor: !outer.IsVirtual ? dxxColors.Magenta : dxxColors.LightGreen);
                //        }

                //    }


                //}

                //if (includeOverhangs)
                //{

                //    uopLinePair overhang1;
                //    uopLinePair overhang2;
                //    foreach (var inner in innerboxes)
                //    {

                //        if (inner.IsVirtual && !includeVirtuals) continue;
                //        overhang1 = inner.Clone();
                //        overhang1.MoveOrtho(thk, -thk);
                //        overhang2 = overhang1.Clone();
                //        overhang1.Line1.EndPt.Y = overhang1.Line1.StartPt.Y + dcdata.EndPlateOverhang;
                //        overhang1.Line2 = overhang1.Line1.Moved(thk);
                //        Tool.DrawLinePair(draw, overhang1, true, aLayer: !inner.IsVirtual ? "OVERHANGS" : "VIRTUALS", aColor: !inner.IsVirtual ? dxxColors.Blue : virolor);

                //        overhang1.Mirror(null, inner.Line1.MidPt.Y);
                //        Tool.DrawLinePair(draw, overhang1, true, aLayer: !inner.IsVirtual ? "OVERHANGS" : "VIRTUALS", aColor: !inner.IsVirtual ? dxxColors.Blue : virolor);


                //        overhang2.Line1 = inner.Line2.Moved(-thk);
                //        overhang2.Line1.EndPt.Y = overhang2.Line1.StartPt.Y + dcdata.EndPlateOverhang;
                //        overhang2.Line2 = overhang2.Line1.Moved(-thk);
                //        Tool.DrawLinePair(draw, overhang2, true, aLayer: !inner.IsVirtual ? "OVERHANGS" : "VIRTUALS", aColor: !inner.IsVirtual ? dxxColors.Blue : virolor);

                //        overhang2.Mirror(null, inner.Line2.MidPt.Y);
                //        Tool.DrawLinePair(draw, overhang2, true, aLayer: !inner.IsVirtual ? "OVERHANGS" : "VIRTUALS", aColor: !inner.IsVirtual ? dxxColors.Blue : virolor);

                //    }
                //}

                //if (includeShelfs)
                //{
                //    uopLine l1;
                //    uopLine l2;
                //    List<uopLinePair> shelflines = dcdata.ShelfLines;
                //    foreach (var shelf in shelflines)
                //    {
                //        if (shelf.IsVirtual && !includeVirtuals) continue;
                //        l1 = shelf.GetSide(uppSides.Left);
                //        if (l1 != null)
                //        {
                //            l2 = l1.Moved(dcdata.ShelfWidth);
                //            // l1.Move(thk);
                //            Tool.DrawLinePair(draw, new uopLinePair(l1, l2), true, aLayer: !shelf.IsVirtual ? "SHELFS" : "VIRTUALS", aColor: !shelf.IsVirtual ? dxxColors.DarkGrey : virolor);
                //        }
                //        l1 = shelf.GetSide(uppSides.Right);
                //        if (l1 != null)
                //        {
                //            l2 = l1.Moved(-dcdata.ShelfWidth);
                //            //l1.Move(-thk);
                //            Tool.DrawLinePair(draw, new uopLinePair(l1, l2), true, aLayer: !shelf.IsVirtual ? "SHELFS" : "VIRTUALS", aColor: !shelf.IsVirtual ? dxxColors.DarkGrey : virolor);
                //        }
                //    }

                //}

                //if (includeEndSupports)
                //{
                //    draw.aCircle(dxfVector.Zero, dcdata.DeckRadius, dxfDisplaySettings.Null(aLayer: "END SUPPORTS", aColor: dxxColors.Purple, aLinetype: "Dot2"));
                //    List<uopLinePair> eslines = dcdata.GetEndSupportLines();
                //    foreach (var es in eslines)
                //    {
                //        if (es.IsVirtual && !includeVirtuals) continue;
                //        Tool.DrawLinePair(draw, es, false, aLayer: !es.IsVirtual ? "END SUPPORTS" : "VIRTUALS", aColor: !es.IsVirtual ? dxxColors.Purple : virolor);


                //    }

                //}
                //if (includePanels)
                //{

                //    uopShapes shapes = dcdata.PanelShapes(bIncludeClearance: true, out List<uopLine> pnlLns);
                //    foreach (var shape in shapes)
                //    {
                //        draw.aPolyline(shape.Vertices, bClosed: true, dxfDisplaySettings.Null(aLayer: "PANELS", aColor: !shape.IsVirtual ? dxxColors.z_41 : virolor));
                //    }
                //    if (uopUtils.RunningInIDE)
                //    {
                //        foreach (var pnlLn in pnlLns)
                //        {
                //            if (pnlLn.Length <= 0) continue;
                //            draw.aLine(pnlLn, dxfDisplaySettings.Null(aLayer: "PANELS", aColor: dxxColors.z_41, aLinetype: dxfLinetypes.Hidden));

                //            draw.aCircles(pnlLn.Points, aRadius: 0.01 * paperscale, dxfDisplaySettings.Null(aLayer: "PANELS", aColor: dxxColors.z_41));

                //        }

                //    }
                //}
                //if (includeFBAs)
                //{

                //    uopShapes shapes = dcdata.FreeBubblingShapes(out List<uopLine> pnlLns);
                //    foreach (var shape in shapes)
                //    {
                //        draw.aPolyline(shape.Vertices, bClosed: true, dxfDisplaySettings.Null(aLayer: "FBA", aColor: !shape.IsVirtual ? dxxColors.Blue : virolor));
                //    }
                //    if (uopUtils.RunningInIDE)
                //    {
                //        foreach (var pnlLn in pnlLns)
                //        {
                //            if (pnlLn.Length <= 0) continue;
                //            draw.aLine(pnlLn, dxfDisplaySettings.Null(aLayer: "FBA", aColor: dxxColors.Blue, aLinetype: dxfLinetypes.Hidden));

                //            draw.aCircles(pnlLn.Points, aRadius: 0.01 * paperscale, dxfDisplaySettings.Null(aLayer: "FBA", aColor: dxxColors.Blue));

                //        }

                //    }
                //}

                //if (Assy.DesignFamily.IsBeamDesignFamily())
                //{
                //    List<uopLinePair> edges = Assy.Beam.GetEdges(0.5);// Assy.RingClearance);
                //    foreach (uopLinePair edge in edges)
                //    {
                //        Tool.DrawLinePair(draw, edge, aLayer: "TRIMMERS", aColor: dxxColors.Orange);
                //    }
                //}

                //double shellrad = Assy.ShellID / 2 + 0.25 * paperscale;
                //foreach (var dc in Assy.Downcomers)
                //{
                //    double y1 = Math.Sqrt(Math.Pow(shellrad, 2) - Math.Pow(dc.X, 2));
                //    uopLine l1 = new uopLine(new uopVector(dc.X, y1), new uopVector(dc.X, -y1));
                //    draw.aLine(l1, aLineType: dxfLinetypes.Center);
                //}

                //dxfRectangle bounds = Image.Entities.BoundingRectangle();
                //uopUnit linearU = uopUnits.GetUnit(uppUnitTypes.SmallLength);
                //uppUnitFamilies units = Drawing.DrawingUnits;
                //dxfVector v1 = bounds.TopRight.Moved(0.15 * paperscale);
                //dxoDimStyle dstyle = Image.DimStyle();
                //string txt = $"TRAY ASSY : {Assy.RingRange.SpanName}";
                //txt += $"\\PROUNDING METHOD : {dcdata.RoundingMethod}";
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
                //draw.aText(v1, txt, aAlignment: dxxMTextAlignments.TopLeft);

            }
            catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
            finally { Status = string.Empty; DrawinginProgress = false; }

            return;
        }
        private void DDWG_StartUpLines()
        {
            try
            {
                if (Image == null || MDRange == null) return;

                string config = Drawing.Options.AnswerS("Select Configuration", "By Default");
                bool drawpanels = Drawing.Options.AnswerB("Show Panels Below?", false);
                mdTrayAssembly assy = Assy;
                string txt = string.Empty;
                int cnt = 0;
                List<mdSpoutGroup> sGroups = assy.SpoutGroups.GetByVirtual(aVirtualValue: false);
                List<mdDowncomer> DComers = assy.Downcomers.GetByVirtual(aVirtualValue: false);
                Image.TextStyle().WidthFactor = 0.8;
                //Image.TextStyle().FontName = "RomanS.shx";
                Image.Display.SetDisplayWindow(assy.SpoutGroups.Bounds(false).ToDXFRectangle().Expanded(1.05), true);
                double paperscale = Image.Display.PaperScale;
                dxoDrawingTool draw = Image.Draw;
                dxoLeaderTool ldrs = Image.LeaderTool;
                dxfDisplaySettings red = dxfDisplaySettings.Null(aLayer: "STARTUPLIMITS", aColor: dxxColors.Red);
                dxfDisplaySettings blue = dxfDisplaySettings.Null(aLayer: "BOX", aColor: dxxColors.Blue);
                uopUnit aunits = uopUnits.GetUnit(uppUnitTypes.SmallArea);

                foreach (mdDowncomer aDC in DComers)
                {

                    foreach (var box in aDC.Boxes)
                    {
                        if (box.IsVirtual) continue;

                        uopLinePair lims = box.StartUpLimitLines;
                        draw.aLine(lims.Line1, red);
                        draw.aLine(lims.Line2, red);
                        draw.aLine(box.LimitLine(bTop: true), blue);
                        draw.aLine(box.LimitLine(bTop: false), blue);
                       Image.Entities.Append(box.Edges("BOX", dxxColors.BlackWhite, dxfLinetypes.Continuous));
                    }
                }


                dxfDisplaySettings magenta = new("0", dxxColors.LightMagenta, dxfLinetypes.Continuous);
                dxfDisplaySettings lightgreen = new("STARTUP BOUNDS", dxxColors.LightGreen, dxfLinetypes.Continuous);
                dxfDisplaySettings lightgrey = new("SPOUT BOUNDS", dxxColors.LightGrey, dxfLinetypes.Continuous);
                
                foreach (mdSpoutGroup sGroup in sGroups)
                {
                    uopShape bound = sGroup.StartupBound(bRegen: true);
                    if (bound == null) 
                        continue;
                    Image.Entities.AddShape(bound, lightgreen);
                    Image.Entities.AddShape(sGroup.SpoutBoundary, lightgrey);
                    draw.aCircle(sGroup.SpoutCenter, 0.375, magenta);

                }

                Image.LinetypeLayers.Setting = dxxLinetypeLayerFlag.Suppressed;

                paperscale = Image.Display.ZoomExtents(1.05, true);

                Drawing.ZoomExtents = false;

                uppStartupSpoutConfigurations  suconfig = uopEnums.SpoutConfigurationFromString(config,out string err);
                if (suconfig == uppStartupSpoutConfigurations.ByDefault || suconfig == uppStartupSpoutConfigurations.Undefined || !string.IsNullOrEmpty(err)) suconfig = Assy.StartupConfiguration;

                mdStartupSpouts spouts = mdStartUps.Generate(Assy, Assy.IdealStartupArea, ref suconfig, 0);

                mdStartupLines suLines = mdStartUps.Lines_Basic(assy.SpoutGroups, Assy, suconfig, bRegenBoundsAndLines: true);

             
                Image.TextSettings.LayerName = string.Empty;
                Image.Display.SetFeatureScales(0.5 * paperscale);
                double dst = 0.08 * paperscale;
                dxfEntity ptr;
                double ptrwd = assy.FunctionalPanelWidth / 6 / 1.2;
                mdStartupLine aLine;
                dxePolyline aLineMarker;
                dxfVector p2 = new();
                double ang = 0;
                List<mdStartupLine> sgLines;
                dxxMTextAlignments alng = dxxMTextAlignments.MiddleRight;
                int spoutcount = 0;
                foreach (mdSpoutGroup sGroup in sGroups)
                {
                    sgLines = suLines.FindAll((x) => string.Compare(x.SpoutGroupHandle, sGroup.Handle,true) ==0);
                    
                    for (int j = 0; j < sgLines.Count; j++)
                    {
                        aLine = sgLines[j];
                        aLineMarker = aLine.DisplayLine(dst);
                        aLineMarker.LayerName = "CONTROL LINES";
                        uopVector u1 = aLine.ReferencePt;
                        if (u1 != null)
                        {
                            //draw.aCircle(v1, 0.375, aColor: aLine.Color);
                            //p1 = aLine.MidPt;


                            if (aLine.Tag == "UR")
                            {
                                //p2.MoveToVector(p1, 0.2 * paperscale, -0.2 * paperscale);
                                aLineMarker.Move(aLine.Suppressed ?- 0.01 * paperscale : 0.01 * paperscale); 
                                ang = -90;
                                alng = dxxMTextAlignments.MiddleLeft;
                            }
                            else if (aLine.Tag == "LR")
                            {
                                //p2.MoveToVector(p1, 0.2 * paperscale, 0.2 * paperscale);
                                aLineMarker.Move(aLine.Suppressed ? -0.01 * paperscale : 0.01 * paperscale);
                                ang = -90;
                                alng = dxxMTextAlignments.MiddleLeft;
                            }
                            else if (aLine.Tag == "UL")
                            {
                                //p2.MoveToVector(p1, -0.2 * paperscale, -0.2 * paperscale);
                                aLineMarker.Move(!aLine.Suppressed ? -0.01 * paperscale : 0.01 * paperscale);
                                ang = 90;
                                alng = dxxMTextAlignments.MiddleRight;
                            }
                            else if (aLine.Tag == "LL")
                            {
                                //p2.MoveToVector(p1, -0.2 * paperscale, 0.2 * paperscale);
                                aLineMarker.Move(!aLine.Suppressed ? -0.01 * paperscale : 0.01 * paperscale);
                                ang = 90;
                                alng = dxxMTextAlignments.MiddleRight;
                            }
                            ptr = draw.aPointer(u1, ptrwd, 1.2, ang, aLine.Suppressed, aDisplaySettings: new dxfDisplaySettings("POINTERS", aLine.Color, "continuous"));
                            u1 = (ang == 90) ? new uopVector( ptr.BoundingRectangle().MiddleLeft().Moved(-dst / 2)) : new uopVector( ptr.BoundingRectangle().MiddleRight().Moved(dst / 2));
                            //v1 = (ang == 90) ? ptr.BoundingRectangle().MiddleLeft() : ptr.BoundingRectangle().MiddleRight();
                            //draw.aCircle(v1, 0.125);

                            txt = aLine.Handle;
                            if (!aLine.Suppressed)
                            {
                                cnt++;
                                txt += $" - { cnt}\\Poccurs:{sGroup.OccuranceFactor}";
                                spoutcount += sGroup.OccuranceFactor;

                            }
                            draw.aText(u1, txt, aTextHeight: ptrwd / 3, aAlignment: alng);
                            //ldrs.Text(p1, p2, aLine.Handle , null, false, "LEADERS", aLine.Color, aLineMarker.Linetype);
                            aLineMarker.LayerName = "CONTROL LINES";
                            Image.Entities.Add(aLineMarker);
                        }

                    }
                }

                dxfRectangle rec = Image.Entities.BoundingRectangle();
                paperscale *= 2;
                Image.Display.SetFeatureScales(paperscale);
                dxfVector v1 = rec.BottomCenter.Moved(0 ,-0.5 * paperscale);
                
                uopHole spout = spouts.StartupSpout;

                txt = $"TRAY {Assy.SpanName()} STARTUP LINES";
                txt += $"\\PCONFIGURATION = {suconfig.Description()}";
                txt += $"\\PIDEAL AREA =  {aunits.UnitValueString( Assy.IdealStartupArea, uppUnitFamilies.English)}";
                txt += $"\\PSU SPOUT = {spout.Diameter:0.000} [ {(spout.Diameter*25.4):0.0} mm ] x {spout.Length:0.000} [ {(spout.Length * 25.4):0.0} mm ]={aunits.UnitValueString( spout.Area, uppUnitFamilies.English) }";
                txt += $"\\PTOTAL SPOUTS = {spouts.TotalCount}";


                dxxColors clr = Math.Abs(spouts.TotalArea) < mdStartUps.DeviationLimit ? dxxColors.Red : dxxColors.Green; 
                txt += $"\\P\\C{(int)clr};{ spouts.TotalCount} x {spouts.SingleSpoutArea:0.000}  ={aunits.UnitValueString( spouts.TotalArea, uppUnitFamilies.English)}";
                draw.aText(v1, txt, aAlignment: dxxMTextAlignments.TopCenter);

                draw.aShape(rec,lightgreen);

                if (drawpanels)
                {
                    var panels = mdBlocks.DeckPanels_View_Plan(Image, Assy, bBothSides: true);
                    panels.Entities.SetDisplayVariable(dxxDisplayProperties.Linetype, dxfLinetypes.Hidden);
                    draw.aInsert(panels.Name, dxfVector.Zero, -90);

                }


            }
           

            catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
            finally { Status = string.Empty; DrawinginProgress = false; }

            return;
        }


        private void DDWG_SectionSketch()
        {
        
            // ^generates the the elevation section sketch of a tray.

            if (Image == null || MDRange == null)
            {
                return;
            }

            try
            {
                Image.DimStyle().TextHeight = 0.125;
                Image.DimStyle().SetGapsAndOffsets(0.03, 0.09);
                Image.DimStyle().SetColors(dxxColors.Blue, dxxColors.BlackWhite);

                Image.DimSettings.DimLayerColor = dxxColors.Blue;
                var assy = Assy;

                mdDowncomer aDComer = assy.Downcomer();
                dxfVector p1 = dxfVector.Zero;
                dxfVector p2 = dxfVector.Zero;
                colDXFVectors fPts;
                colDXFVectors dPts;
                colDXFVectors ePts;
                double thk = aDComer.Thickness;
                double wd = aDComer.OutsideWidth;
                double ht = aDComer.OutsideHeight;
                double lg = 4 * wd;
                double ringSpc = MDRange.RingSpacing;
                double how = aDComer.How;
                double apht = 0;
                double apthk = 0;
                double dkthk = assy.Deck.Thickness;
                colDXFVectors aPts = new colDXFVectors(new dxfVector(lg,0), new dxfVector(0, 0), new dxfVector(0, ht), new dxfVector(lg, ht));
                colDXFVectors bPts = new colDXFVectors(new dxfVector(lg, thk), new dxfVector(thk, thk), new dxfVector(thk, ht));
                colDXFVectors cPts = new colDXFVectors(new dxfVector(-wd, ht - how), new dxfVector(0, ht - how), new dxfVector(0, ht - how - dkthk), new dxfVector(-wd, ht - how - dkthk));

                colDXFVectors apVerts;
                
                if (assy.DesignOptions.HasAntiPenetrationPans)
                {
                    mdAPPan pan = new mdAPPan(assy);
                    apht = pan.Height;
                    apthk = pan.Thickness;
                }

                Image.Header.Color = dxxColors.BlackWhite;
                Image.DimStyleOverrides.AutoReset = true;

                
                dxfDisplaySettings dsp = dxfDisplaySettings.Null(aLayer: "0", aColor: dxxColors.BlackWhite, aLinetype: dxfLinetypes.Continuous);

                Image.Draw.aPolyline(aPts, false, aDisplaySettings: dsp);
                Image.Draw.aPolyline(bPts, false, aDisplaySettings: dxfDisplaySettings.Null(aLayer: "0", aColor: dxxColors.Green, aLinetype: dxfLinetypes.Hidden));
                Image.Draw.aPolyline(cPts, false, aDisplaySettings: dsp);

                dPts = new colDXFVectors( cPts);
                dPts.Move(aChangeY: -ringSpc);
                dPts.Item(2).Move(0.25 * wd);
                dPts.Item(3).Move(0.25 * wd);
                Image.Draw.aPolyline(dPts, false, aDisplaySettings: dsp);

                ePts = new colDXFVectors(dPts.Item(2).Moved(thk / 2, how));

                ePts.AddRelative(aY: -ht + thk / 2);
                ePts.AddRelative(wd - thk);
                ePts.AddRelative(aY: ht - thk / 2);

                Image.Draw.aPolylineTrace(ePts, thk, false, aLayerName: "0", aColor: dxxColors.BlackWhite, aLineType: dxfLinetypes.Continuous);

                fPts = new colDXFVectors(dPts);
                fPts.Mirror(dPts.LineSegment(2));
                fPts.Move(wd);

                fPts.Item(1).X = lg;
                fPts.Item(4).X = lg;

                Image.Draw.aPolyline(fPts, false, aDisplaySettings: dsp);

                double paperscale = Image.Display.ZoomExtents(1.3,bSetFeatureScale:true);
                Image.DimStyle().FeatureScaleFactor = paperscale * 0.8;
                Image.DimStyle().TextHeight = 0.125;

                // ring space
                Image.Draw.aDim.Vertical(cPts.Item(4), dPts.Item(4), -0.55, aSuffix: "\\PRING SPACE");

                // dc belowe deck
                Image.DimStyleOverrides.SetSuppressionFlags(aSupExtLn1: true, aSupExtLn2: false);
                Image.Draw.aDim.Vertical(cPts.Item(3), aPts.Item(2), -0.35);

                // wier height
                Image.DimStyleOverrides.SetSuppressionFlags(aSupExtLn1: true, aSupExtLn2: false);
                Image.Draw.aDim.Vertical(dPts.Item(2), ePts.Item(1).Moved(-thk / 2), -0.3, 0.01);

                // deck thickness
                Image.Draw.aDim.Vertical(dPts.Item(4), dPts.Item(1), -0.35);

                // dc thickness
                Image.DimStyleOverrides.SetSuppressionFlags(aSupExtLn1: true, aSupExtLn2: true);
                Image.Draw.aDim.Horizontal(ePts.Item(2).Moved(-thk / 2, 4 * thk), ePts.Item(2).Moved(thk / 2, 4 * thk), -0.15);

                // dc inside width
                Image.Draw.aDim.Horizontal(ePts.Item(2).Moved(thk / 2, -thk / 2), ePts.Item(3).Moved(-thk / 2, -thk / 2), -0.3, aSuffix: "\\PINSIDE"); // What is \P doing???  New Line

                // deck below to dc bottom
                Image.Draw.aDim.Vertical(aPts.Item(1), fPts.Item(1), -0.35);

                // app pan
                if (apht != 0)
                {
                    thk = assy.GetSheetMetal(false).Thickness; //the ap material'
                    apVerts = new colDXFVectors(new dxfVector(0.5 * lg - 0.5 * wd + thk / 2, 0));
                    apVerts.AddRelative(aY: -apht + apthk / 2);
                    apVerts.AddRelative(wd - apthk);
                    apVerts.AddRelative(aY: apht - apthk / 2);

                    Image.Draw.aPolylineTrace(apVerts, apthk, false);

                    p1 = apVerts.LastVector().Moved(apthk / 2);
                    p2 = p1.Moved(aYChange: -apht + apthk);

                    // inside ap height
                    Image.DimStyleOverrides.SetSuppressionFlags(aSupExtLn1: true, aSupExtLn2: false);
                    Image.Draw.aDim.Vertical(p1, p2, -0.4, aSuffix: "\\PINSIDE");

                    // ap thickness
                    p1 = p2.Moved(aYChange: -apthk);
                    Image.Draw.aDim.Vertical(p1, p2, 0.6);

                    Image.DimStyleOverrides.SetSuppressionFlags(aSupExtLn1: true, aSupExtLn2: true);
                    p1.Move(-0.5 * wd);
                    p2.SetCoordinates(p1.X, fPts.Item(1).Y);

                    // clearance dim
                    Image.Draw.aDim.Vertical(p1, p2, 0.4);
                }

                p1 = Image.Display.ExtentRectangle.BottomCenter.Moved(aYChange: -0.5 * Image.Display.PaperScale);
                Image.Draw.aText(p1, assy.TrayName(true).ToUpper(), Image.Display.PaperScale * 0.25, dxxMTextAlignments.TopCenter, aLayer: "0", aColor: dxxColors.BlackWhite);
                Image.Display.ZoomExtents();

                DrawinginProgress = false;
            }
            catch (Exception e)
            {
                MessageBox.Show($"{e}", "Exception Thrown", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DDWG_FunctionalActiveAreas()
        {
            Tool.DDWG_FunctionalActiveAreas(this);
        }

        private void DDWG_FreeBubblingAreas()  => Tool.DDWG_FreeBubblingAreas(this);
       

        private void DDWG_FeedZones()
        {
            // ^generates the Tray Sketch drawing shown on the Cross Flow input screen.

            // initialize

            try
            {
                List<mdDeckPanel> Panels = Assy.DeckPanels.GetByVirtual(aVirtualValue: null);
                dxePolyline Perim;
                List<dxePolyline> Perims = new();
                dxfVector p1 = dxfVector.Zero;
                dxeLine aAxis = dxfPlane.World.ZAxis();
                Image.Layers.Add("FEED_ZONES", dxxColors.Grey);
                Image.Layers.Add("SPOUT_GROUPS", dxxColors.LightGrey);
                dxfDisplaySettings dsp1 = new("FEED_ZONES", dxxColors.ByLayer, "ByLayer");
                dxfDisplaySettings dsp2 = new("SPOUT_GROUPS", dxxColors.ByLayer, "ByLayer");

                dxoDrawingTool draw = Image.Draw;
                for (int i = 1; i <= Panels.Count; i++)
                {
                    mdDeckPanel Panel = Panels[i - 1];
                    List<mdFeedZone> Zones = Panel.FeedZones(Assy);

                    for (int j = 1; j <= Zones.Count; j++)
                    {
                        mdFeedZone aZone = Zones[j - 1];
                        mdSpoutGroup sGroup = aZone.SpoutGroup(Assy);

                        colDXFVectors vrts = new colDXFVectors(aZone.Bounds.Vertices);
                        //vrts.RotateAbout(aAxis, 90);

                        Perim = draw.aPolyline(vrts, true, dsp1);
                        Perim.Tag = aZone.Handle;

                        Perims.Add(Perim);


                        if (sGroup != null)
                        {
                            vrts = new colDXFVectors(sGroup.Perimeter.Vertices);
                            vrts.RotateAbout(aAxis, -90);
                            Perim = draw.aPolyline(vrts, true, dsp2);
                            Perim.Tag = aZone.Handle;

                        }
                    }
                }

                double tht = 0.125 * Image.Display.ZoomExtents(1.05, true);

                for (int j = 1; j <= Perims.Count; j++)
                {
                    Perim = Perims[j - 1];

                    draw.aText(Perim.Center(), Perim.Tag, tht, dxxMTextAlignments.MiddleCenter, aColor: dxxColors.Blue);
                }

            }
            catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
            finally { Status = string.Empty; DrawinginProgress = false; }
        }

        private void DDWG_DowncomerFit()
        {
            // ^this function Included for test purposes.

            mdTrayAssembly assy = MDRange.TrayAssembly;
            if (assy == null) return;


            try
            {
                int j;
                mdTrayAssembly aAssy = assy;
                colUOPTrayRanges aRngs = MDProject.TrayRanges;
                colDXFEntities aEnts = new();

                dxfRectangle aRec;
                dxfRectangle bRec = null;
                double d1;
                List<colDXFEntities> aGroups = new();
                dxfVector v1 = dxfVector.Zero;
                dxeArc aAr;
                dxeArc mAr = new(dxfVector.Zero, aAssy.ManholeID / 2);
                dxeText aTxt = new() { LayerName = "TEXT", Alignment = dxxMTextAlignments.MiddleCenter };
                dxeText bTxt;
                string txt;
                mdDowncomer aDC = aAssy.Downcomers.LastMember();
                double clrc;
                dxePolygon aPG;



                Image.Layers.Add("FIT_CIRCLES", dxxColors.Blue);
                Image.Layers.Add("MAN_CIRCLES", dxxColors.LightGrey);
                Image.Layers.Add("TEXT", dxxColors.BlackWhite);

                mAr.LCLSet("MAN_CIRCLES", dxxColors.ByLayer);


                aPG = aDC.View_ManholeFit(aAssy, v1);
                aAr = aPG.ExtentPoints.BoundingCircle();

                clrc = mAr.Diameter - aAr.Diameter;

                aEnts.Add(aPG);
                txt = aAssy.TrayName(false).ToUpper();
                txt = $"{txt}\\PDC {aDC.PartNumber}";
                txt = $"{txt}\\P{{\\C5;FIT DIA. = {Image.FormatNumber(aAr.Diameter, true, false, true)}}}";
                txt = $"{txt}\\P{{\\C9;MANHOLE DIA. = {Image.FormatNumber(mAr.Diameter, true, false, true)}}}";
                if (clrc > 0)
                {
                    if (clrc < 0.5)
                    {
                        txt = $"{txt}\\P{{\\C30;CLEARANCE = {Image.FormatNumber(clrc, true, false, true)}";
                        txt = $"{txt}\\P!! VIOLATES {Image.FormatNumber(0.5, true, false, true)} MARGIN !!}}";
                    }
                    else
                    {
                        txt = $"{txt}\\P{{\\C3;CLEARANCE = {Image.FormatNumber(clrc, true, false, true)}}}";
                    }
                }
                else
                {
                    txt = $"{txt}\\P{{\\C1;CLEARANCE = {Image.FormatNumber(clrc, true, false, true)}";
                    txt = $"{txt}\\P!! WILL NOT FIT !!}}";
                }

                aTxt.Value = clrc;
                aTxt.TextString = txt;

                aTxt.MoveTo(aAr.Center);

                bTxt = (dxeText)aEnts.Add(aTxt, bAddClone: true);

                aAr.LCLSet("FIT_CIRCLES", dxxColors.ByLayer);

                aEnts.Add(aAr);

                mAr.MoveTo(aAr.Center);
                aEnts.Add(mAr, bAddClone: true);

                aEnts.Move(aChangeY: -aAr.Y);

                if (mAr.Radius > aAr.Radius)
                {
                    aPG.Value = mAr.Radius;
                }
                else
                {
                    aPG.Value = aAr.Radius;
                }

                aGroups.Add(aEnts);

                v1.SetCoordinates(0, 0);
                j = 1;
                for (int i = 1; i <= aGroups.Count; i++)
                {
                    aEnts = aGroups[i - 1];
                    aRec = aEnts.BoundingRectangle();

                    if (i == 1)
                    {
                        Image.Entities.Append(aEnts);
                    }
                    else
                    {
                        if (j == 1)
                        {
                            d1 = bRec.Height / 2 + aRec.Height / 2 + 2;
                            v1.Y -= d1;
                        }
                        else
                        {
                            d1 = bRec.Width / 2 + aRec.Width / 2 + 2;
                            v1.Y = 0;
                            v1.X += d1;
                        }
                        aEnts.Move(v1.X, v1.Y);
                        aRec = aEnts.BoundingRectangle();
                        Image.Entities.Append(aEnts);
                        if (i > 1) j *= -1;

                    }

                    bRec = aRec.Clone();


                    Image.Display.ZoomExtents();
                }


            }
            catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
            finally { Status = string.Empty; DrawinginProgress = false; }
        }


        /// <summary>
        ///#1returns the vertical centerline used for dimensions in other functions
        ///#2returns horizontal centerline used for dimensions in other functions
        ///#3the view to draw
        ///^used by higher level drawing functions to draw the column shell
        ///~the view argument controls how the shell is drawn.
        /// </summary>
        /// <param name="View"></param>
        private void Draw_Shell(uppViews View) => Draw_Shell(View, out dxeLine _, out dxeLine _);
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
        private void Draw_Shell(uppViews View, out dxeLine HorCenterLn, out dxeLine VerCenterLn)
        {
            HorCenterLn = null;
            VerCenterLn = null;
            if (Image == null || MDRange == null) return;
            string statuswuz = Status;
            Status = "Drawing Shell";

            List<dxeLine> cLines;

            //put lines or a single circle into the dxf but draw rectangles or double circles to the screen
            try
            {
                Image.Layers.Add("HEAVY", dxxColors.Grey, dxfLinetypes.Continuous);


                if (View == uppViews.Plan || View == uppViews.AttachPlan || View == uppViews.LayoutPlan || View == uppViews.InstallationPlan)
                {
                    //draw and add the inner diameter circle to the screen and to the dxf
                    Image.Draw.aCircle(null, MDRange.ShellID * 0.5, dxfDisplaySettings.Null("HEAVY"));
                    cLines = Image.Draw.aCenterlines(dxfVector.Zero, MDRange.ShellID * 0.5 + 0.75, 1.05);
                    HorCenterLn = cLines.Find((x) => x.Flag == "HORIZONTAL");
                    VerCenterLn = cLines.Find((x) => x.Flag == "VERTICAL");
                }
            }
            catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
            finally { Status = statuswuz; }
        }

        /// <summary>
        ///#1the view to draw
        ///^used by higher level drawing functions to draw the support ring
        ///~the view argument controls how the ring is drawn.
        /// </summary>
        /// <param name="View"></param>
        private void Draw_Ring(uppViews View)
        {
            if (MDRange == null || Image == null) return;
            string statuswuz = Status;

            try
            {
                if (View == uppViews.Plan || View == uppViews.LayoutPlan)
                {
                    Status = "Drawing Ring";

                    Image.Layers.Add("Ring", Image.LinetypeLayers.LineColor(dxfLinetypes.Hidden), dxfLinetypes.Hidden);
                    Image.Draw.aCircle(null, MDRange.RingID / 2, dxfDisplaySettings.Null(aLayer: "Ring"));
                }
            }
            catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
            finally { Status = statuswuz; }

        }

        /// <summary>
        /// This function draws the StartUp Spout Pointers
        /// </summary>
        private void Draw_Startups()
        {
            if (MDRange == null || Image == null) return;
            string statuswuz = Status;
            try
            {
                Status = "Drawing Startup Pointers";
                mdTrayAssembly assy = Assy;
                //assy.StartupSpouts.GetCenters(assy, out colDXFVectors p1, out colDXFVectors p2, out colDXFVectors p3, out colDXFVectors p4, bSeperateObscured: ProjectType == uppProjectTypes.MDDraw);

                uopVectors centers = assy.StartupSpouts.Centers(aXOffset: 0.5);
                double wd = assy.FunctionalPanelWidth / 6.5;
                wd = Image.Display.PaperScale * 0.125;
                if (wd < 0.125 *assy.FunctionalPanelWidth) wd = 0.125 * assy.FunctionalPanelWidth;
                if (wd > 0.333 * assy.FunctionalPanelWidth) wd = 0.333 * assy.FunctionalPanelWidth;
                //Image.Display.ZoomExtents(bSetFeatureScale: true);
                //wd = 0.25 * Image.Display.PaperScale;
                //dxeSolid ptr;
                //dxfVector v1;
                dxfDisplaySettings dsp = new("STARTUP_POINTERS", dxxColors.BlackWhite, dxfLinetypes.Continuous);
                dxoDrawingTool draw = Image.Draw;
                draw.aPointers(centers.FindAll((x) => !x.Suppressed), wd, aRotation: null, aDisplaySettings: dsp);

                //if (p1.Count > 0) draw.aPointers(p1, wd, aRotation: 90, aDisplaySettings: dsp);
                //if (p2.Count > 0) draw.aPointers(p2, wd, aRotation: -90, aDisplaySettings: dsp);
                //dsp.Color = dxxColors.Red;
                //if (p3.Count > 0) draw.aPointers(p3, wd, aRotation: 90, aDisplaySettings: dsp);
                //if (p4.Count > 0) draw.aPointers(p4, wd, aRotation: -90, aDisplaySettings: dsp);


            }
            catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
            finally { Status = statuswuz; }


        }

        public void Draw_Beams(uppViews aView, bool bSuppressHoles = false, string aLayerName = "BEAM")
        {
            if (MDRange == null || Image == null) return;
            if (!Assy.DesignFamily.IsBeamDesignFamily()) return;
            dxoDrawingTool draw = Image.Draw;
            if (!string.IsNullOrWhiteSpace(aLayerName))
                aLayerName = Image.GetOrAddReference(aName: aLayerName, aRefType: dxxReferenceTypes.LAYER).Name;
            else
                aLayerName = "GEOMETRY";

            string statuswuz = Status;
            double paperscale = Image.Display.PaperScale;
            try
            {
                if (aView == uppViews.Plan || aView == uppViews.ZoneView || aView == uppViews.LayoutPlan)
                {
                    mdBeam beam = Assy.Beam;
                    Status = "Drawing Support Beam";
                    draw.aInserts(mdBlocks.Beam_View_Plan(Image, beam, Assy, true, true, true,aLayerName: aLayerName), null, bOverrideExisting: false,aDisplaySettings:dxfDisplaySettings.Null(aLayerName));
                    //dxePolygon pg = beam.View_Plan(dxfVector.Zero, bShowObscured: true, bSuppressHoles: bSuppressHoles, aLayerName: aLayerName, aCenterLineLength: beam.Length + 0.5 * paperscale);
                    //pg.BlockName = "SUPPORT_BEAM";
                    //dxeInsert insert = draw.aInsert(pg, new dxfVector(beam.X, beam.Y), aDisplaySettings: dxfDisplaySettings.Null(aLinetype: dxfLinetypes.Hidden), aLTLSetting: dxxLinetypeLayerFlag.ForceToColor);
                    
                }
            }
            catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
            finally { Status = statuswuz; }

        }

        /// <summary>
        ///#1the view to draw
        ///^used by higher level drawing functions to draw the downcomer boxes in the tray assembly
        ///~the view argument controls how the downcomer box is drawn.
        /// </summary>
        /// <param name="aView"></param>
        /// <param name="bIncludeShelfAngles"></param>
        /// <param name="bIncludeEndPlates"></param>
        /// <param name="bIncludeEndSupports"></param>
        /// <param name="aIndex"></param>
        private void Draw_Downcomers(uppViews aView, bool bIncludeShelfAngles, bool bIncludeEndPlates, bool bIncludeEndSupports, int aIndex = 0, bool bShowVirtualBoxes = false, bool bIncludeSpouts = true)
        {
            if (MDRange == null || Image == null) return;

            dxoDrawingTool draw = Image.Draw;

            string statuswuz = Status;
            try
            {
                dxsLinetypes ltl = Image.LinetypeLayers;
                // ltl.Add(dxfLinetypes.Hidden, dxxColors.Blue, "HIDDEN");

                if (aView == uppViews.Plan || aView == uppViews.ZoneView || aView == uppViews.LayoutPlan)
                {
                    string lname = "DOWNCOMERS";
                    Image.Layers.Add(lname);
                    Status = "Drawing Downcomers";

                    List<mdDowncomer> dcomers = Assy.Downcomers.GetByVirtual(aVirtualValue: Assy.DesignFamily.IsStandardDesignFamily() ? false : null);
                    foreach (var dcomer in dcomers)
                    {
                        if (aIndex != 0 && dcomer.Index != aIndex) continue;

                        List<mdDowncomerBox> boxes = dcomer.Boxes;
                        foreach (var item in boxes)
                        {
                            if (item.IsVirtual && !bShowVirtualBoxes) continue;
                            dxfBlock block = mdBlocks.DowncomerBox_View_Plan(Image, item, Assy, bSetInstances: bShowVirtualBoxes, bOutLineOnly: false, bSuppressHoles: true, bIncludeSpouts: bIncludeSpouts, bShowObscured: true, aLayerName: lname, bIncludeEndPlates: bIncludeEndPlates, bIncludeEndSupports: bIncludeEndSupports, bIncludeShelfAngles: bIncludeShelfAngles, bIncludeStiffeners: false, bIncludeBaffles: false, bIncludeSupDefs: false, bIncludeFingerClips: false, bIncludeEndAngles: false);
                            List<dxeInsert> inserts = draw.aInserts(block, block.Instances, bOverrideExisting: false);

                        } // box loop
                        uopVector u1 = new uopVector(dcomer.X, Math.Sqrt(Math.Pow(1.1 * MDRange.ShellID * 0.5, 2) - Math.Pow(dcomer.X, 2)));
                        uopVector u2 = new uopVector(dcomer.X, -u1.Y);
                       dxeLine cline =  draw.aLine(u1, u2, aLineType: dxfLinetypes.Center);
                        if (bShowVirtualBoxes && dcomer.OccuranceFactor >1)
                        {
                            u1.X *= -1;
                            u2.X *= -1;
                            cline = draw.aLine(u1, u2, aLineType: dxfLinetypes.Center);
                        }
                    } // downcomer loop


                }
            }
            catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
            finally { Status = statuswuz; }
        }

        private void Draw_EndPlates(uppViews aView, int aIndex = 0)
        {
            if (MDRange == null || Image == null) return;

            dxoDrawingTool draw = Image.Draw;
            string bWeldOns = string.Empty;
            string statuswuz = Status;
            try
            {
                if (aView == uppViews.Plan || aView == uppViews.ZoneView || aView == uppViews.LayoutPlan)
                {
                    string lname = Image.Layers.Add("END PLATES").Name;
                    Status = "Drawing End PLates";
                    if (aView == uppViews.LayoutPlan || aView == uppViews.Plan)
                    {
                        List<mdDowncomer> dcomers = Assy.Downcomers.GetByVirtual(aVirtualValue: false);
                        foreach (var dcomer in dcomers)
                        {
                            if (aIndex != 0 && dcomer.Index != aIndex) continue;
                            dxePolygon aPGRight = null;
                            dxeInsert insR = null;
                            List<mdEndPlate> plates = dcomer.Boxes.SelectMany(b => b.EndPlates()).ToList();
                            foreach (var plate in plates)
                            {
                                aPGRight = plate.View_Plan(true, aCenter: dxfVector.Zero, aLayerName: lname);
                                insR = draw.aInsert(aPGRight, new dxfVector(plate.X, plate.Y), aLTLSetting: dxxLinetypeLayerFlag.ForceToColor);
                                if (dcomer.X > 0 && dcomer.OccuranceFactor > 1)
                                {
                                    draw.aInsert(insR.BlockName, new dxfVector(-dcomer.X, dcomer.Y), 180);
                                }
                            }
                        }
                    }

                }
            }
            catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
            finally { Status = statuswuz; }
        }

        private void Draw_EndSupports(uppViews aView, int aIndex = 0, uppSides aSide = uppSides.Undefined)
        {
            if (MDRange == null || Image == null) return;

            dxoDrawingTool draw = Image.Draw;
            string bWeldOns = string.Empty;
            string statuswuz = Status;

            try
            {
                if (aView == uppViews.Plan || aView == uppViews.ZoneView || aView == uppViews.LayoutPlan)
                {
                    string lname = Image.Layers.Add("END SUPPORTS").Name;
                    Status = "Drawing End Supports";
                    if (aView == uppViews.LayoutPlan || aView == uppViews.Plan)
                    {
                        List<mdDowncomer> dcomers = Assy.Downcomers.GetByVirtual(aVirtualValue: false);
                        foreach (var dcomer in dcomers)
                        {
                            if (aIndex != 0 && dcomer.Index != aIndex) continue;
                            dxePolygon aPGRight = null;
                            dxeInsert insR = null;
                            List<mdEndSupport> supports = dcomer.EndSupports(aSide: aSide);
                            foreach (var support in supports)
                            {
                                aPGRight = support.View_Plan(aCenter: dxfVector.Zero, aLayerName: lname);
                                insR = draw.aInsert(aPGRight, new dxfVector(support.X, support.Y), aLTLSetting: dxxLinetypeLayerFlag.ForceToColor);
                                if (dcomer.X > 0 && dcomer.OccuranceFactor > 1)
                                {
                                    draw.aInsert(insR.BlockName, new dxfVector(-dcomer.X, dcomer.Y), 180);
                                }
                                if (uopUtils.RunningInIDE)
                                {
                                    aPGRight.MoveTo(insR);
                                    dxfPlane pln = aPGRight.Plane;

                                    dxeSymbol axis = Image.EntityTool.CreateSymbol_Axis(aPGRight.Plane, aAxisLength: 0.15, bScaleToScreen: true, aName: $"{insR.BlockName}_Axis");
                                    dxfBlock axisblock = axis.GetBlock(Image);
                                    ////axisblock.Entities.Translate(new dxfVector(-axis.X, -axis.Y));
                                    axisblock = Image.Blocks.Add(axisblock);
                                    draw.aInsert(axisblock.Name, insR.InsertionPt);
                                    //draw.aSymbol.Axis(pln, insR.InsertionPt, aAxisLength: 0.15 * Image.Display.PaperScale  );

                                    Tool.NumberVectors(draw, aPGRight.Vertices, aPaperScale: Image.Display.PaperScale, aTextHeight: 0.01, aStartIndex: 1, aEndIndex: 13);
                                }
                            }
                        }
                    }

                }
            }
            catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
            finally { Status = statuswuz; }
        }


        /// <summary>
        /// Draw Deck Panels
        /// </summary>
        /// <param name="aLayerName"></param>
        /// <param name="aColor"></param>
        /// <param name="aLineType"></param>
        /// <param name="bDrawVirtuals"></param>
        private void Draw_DeckPanels(string aLayerName, dxxColors aColor, string aLineType, bool bDrawVirtuals)
        {
            if (MDRange == null || Image == null) return;
            string statuswuz = Status;
            try
            {
                Status = "Drawing Deck Panels";

                dxeLine aAxis = bDrawVirtuals ? dxeLine.CreateAxis(dxxAxisDescriptors.Z) : null;
                dxoDrawingTool draw = Image.Draw;

                dxfBlock blok = mdBlocks.DeckPanels_View_Plan(Image, Assy, aLayerName, aColor, aLineType, bDontAddToImage: false, bBothSides: bDrawVirtuals, bIncludeClearance: true);
                draw.aInsert(blok.Name, null, 0);


                //DowncomerDataSet dcdata = Assy.DowncomerData;
                //dxfDisplaySettings dsp = new dxfDisplaySettings(aLayerName, aColor, aLineType);
                // colMDDeckPanels aDPs = Assy.DeckPanels;


                //for (int i = 1; i <= aDPs.Count; i++)
                //{
                //    mdDeckPanel panel = aDPs.Item(i);
                //    if (panel.IsVirtual) continue;
                //        uopPanelSectionShape shape = panel.BaseShape;
                //    if (shape == null) continue;

                //    dxoInstances insts = panel.Instances.ToDXFInstances();
                //    if(!bDrawVirtuals) insts.Clear();
                //    draw.aShape(shape, dsp, insts);
                       
                //}

              

            }
            catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
            finally { Status = statuswuz; }
        }

        /// <summary>
        /// Draw Downcomers below 
        ///#1the view to draw
        ///^used by higher level drawing functions to draw the downcomer boxes in the tray assembly
        ///~the view argument controls how the downcomer box is drawn.
        /// </summary>
        /// <param name="View"></param>
        private void Draw_DowncomersBelow(uppViews View, bool showVirtual = true)
        {
            if (MDRange == null || Image == null) return;
            string statuswuz = Status;
            try
            {
                dxoDrawingTool draw = Image.Draw;
                Status = "Drawing Downcomers Below";
                if (View == uppViews.Plan)
                {

                    dxfBlock block = mdBlocks.DowncomersBelow_View_Plan(Image, Assy, $"DCS_BELOW", bBothSides: showVirtual);
                    Image.Draw.aInsert(block, null,aDisplaySettings:dxfDisplaySettings.Null(aLayer: block.LayerName));

                    bool mirrorthem = Assy.DesignFamily.IsStandardDesignFamily();

                    //List<mdDowncomer> dcomers = Assy.Downcomers.GetByVirtual(aVirtualValue: false);
                    //foreach (var dcomer in dcomers)
                    //{
                    //    foreach (var box in dcomer.Boxes)
                    //    {
                    //        if (!showVirtual && box.IsVirtual) continue;

                    //        colDXFVectors verts = box.ExtractDowncomerBelowVertices();

                    //        draw.aPolyline(verts, true, aDisplaySettings: dxfDisplaySettings.Null(aLinetype: dxfLinetypes.Hidden));
                    //        if (mirrorthem && dcomer.OccuranceFactor >1)
                    //        {
                    //            verts.MirrorPlanar(null, 0);
                    //            draw.aPolyline(verts, true, aDisplaySettings: dxfDisplaySettings.Null(aLinetype: dxfLinetypes.Hidden));
                    //        }
                    //    }
                    //}
                }
            }
            catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
            finally { Status = statuswuz; }


        }

        /// <summary>
        /// #1the view to draw
        /// used by higher level drawing functions to draw start up spouts in the tray assembly
        /// the view argument controls how the beam is drawn.
        /// </summary>
        /// <param name="View"></param>
        private void Draw_BubblePromoters(uppViews View, bool bTrayWide = true)
        {
            if (MDRange == null || Image == null) return;
            string statuswuz = Status;
            try
            {
                uopVectors BPSites = Assy.BPCenters(bTrayWide, false);

                if (BPSites.Count > 0)
                {
                    Status = "Drawing Bubble Promoters";
                    if (View == uppViews.Plan)
                    {
                        Image.Draw.aCircles(BPSites, 1.275, new dxfDisplaySettings("BUBBLE_PROMOTERS", dxxColors.BlackWhite, dxfLinetypes.Continuous));
                    }
                }
            }
            catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
            finally { Status = statuswuz; }

        }


        /// <summary>
        /// #1the view to draw
        /// used by higher level drawing functions to draw start up spouts  groups Infor in the tray assembly
        /// the view argument controls how the beam is drawn.
        /// </summary>
        /// <param name="InsertPt"></param>
        /// <param name="SpoutGroup"></param>
        /// <returns></returns>

        //Exluding Spout Sketch Page after confirming from Mike - as users will no longer be needing this page in Report
        //Retaining the code as we may need it in future
        private dxeText Draw_SpoutGroupInfo(ref dxfVector InsertPt, ref mdSpoutGroup SpoutGroup)
        {
            dxeText _rVal = null;
            if (InsertPt == null || SpoutGroup == null || Image == null) return _rVal;
            string statuswuz = Status;
            try
            {

                Status = "Drawing Spout Group Info";

                uopHole Spout = SpoutGroup.Spout;
                mdDowncomer DComer = SpoutGroup.Downcomer(Assy);



                string txt = $"{MDRange.Name(true)}\\P";
                txt += $"Spout Group - {SpoutGroup.Handle}\\P";
                txt += $"Pattern Type - {SpoutGroup.PatternName}\\P";
                double tht = Image.DimStyle().TextHeight * Image.DimStyle().FeatureScaleFactor;
                double dia = SpoutGroup.SpoutDiameter();
                double multi = Drawing.DrawingUnits == uppUnitFamilies.English ? 1 : 25.4;
                if (Spout != null)
                {
                    dia = Spout.Diameter;
                }
                if (SpoutGroup.PatternType != uppSpoutPatterns.SStar)
                {
                    if (Spout != null)
                    {

                        if (Spout.Length == Spout.Diameter)
                            txt += $"Spout - {dia * multi:0.0} Hole\\P";
                        else
                            txt += $"Spout - {dia * multi:0.0} x {Spout.Length * multi:0.0} Slot\\P";
                    }
                }
                else
                {

                    txt += $"Spout - {dia * multi:0.0} x 'L' Slot \\P";

                }

                txt += $"Vertical Pitch - {Image.DimStyle().FormatNumber(SpoutGroup.VerticalPitch)}\\P";
                if (SpoutGroup.HorizontalPitch != 0)
                {
                    txt += $"Horizontal Pitch - {Image.DimStyle().FormatNumber(SpoutGroup.HorizontalPitch)}\\P";
                }

                if (DComer != null)
                {
                    txt += $"Downcomer Box Length - {Image.DimStyle().FormatNumber(DComer.BoxLength(true))}\\P";
                }

                _rVal = Image.Draw.aText(InsertPt, txt, tht, dxxMTextAlignments.TopLeft, aTextStyle: "Standard", aColor: dxxColors.BlackWhite);

                if (SpoutGroup.PatternType == uppSpoutPatterns.SStar)
                {
                    Image.TableSettings.TextStyleName = "Standard";
                    Image.TableSettings.TextSize = 0.075;
                    Image.TableSettings.TextGap = 0.05;
                    Image.TableSettings.RowGap = 0;

                    List<string> tbl = new();

                    dxfVector v1 = _rVal.BoundingRectangle().BottomLeft;

                    v1.Move();

                    tbl.Add("SPOUT|LENGTH 'L'");
                    uopHoles aSpts = SpoutGroup.Spouts;

                    for (int i = 0; i < aSpts.Count; i++)
                    {
                        Spout = aSpts.Item(i);

                        tbl.Add($"{i}|{Spout.Length * multi:0.00#}");


                    }
                    Image.TableSettings.TextSize = 0.1;
                    Image.TableSettings.TextGap = 0.08;
                    Image.TableSettings.HeaderTextColor = dxxColors.BlackWhite;

                    Image.Draw.aTable("SPOUT_GROUPS", v1, tbl);
                }
            }
            catch (Exception e) { HandleError(System.Reflection.MethodBase.GetCurrentMethod(), e); }
            finally { Status = statuswuz; }

            return _rVal;
        }


        public void HandleError(System.Reflection.MethodBase aMethod, Exception e)
        {
            if (Image == null || e == null || aMethod == null) return;
            Image.HandleError(string.IsNullOrWhiteSpace(Status) ? aMethod.Name : $"{aMethod.Name}~{Status}", this.GetType().Name, e.Message + (uopUtils.RunningInIDE ? e.StackTrace : ""));
        }
        #endregion
    }
}
